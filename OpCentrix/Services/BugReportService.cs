using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using System.Text.Json;

namespace OpCentrix.Services
{
    /// <summary>
    /// Service for managing bug reports across all OpCentrix pages
    /// Provides auto-filling with page info and logging to master bug log with separate page-specific files
    /// </summary>
    public interface IBugReportService
    {
        Task<BugReport> CreateBugReportAsync(BugReport bugReport);
        Task<BugReport?> GetBugReportAsync(int id);
        Task<BugReport?> GetBugReportByBugIdAsync(string bugId);
        Task<List<BugReport>> GetBugReportsAsync(BugReportFilter filter);
        Task<BugReportStatistics> GetBugReportStatisticsAsync();
        Task<BugReportStatistics> GetBugReportStatisticsForPageAsync(string pageArea);
        Task<BugReport> UpdateBugReportAsync(BugReport bugReport);
        Task<bool> DeleteBugReportAsync(int id);
        Task<List<BugReport>> GetRecentBugReportsAsync(int count = 10);
        Task<List<BugReport>> GetHighPriorityBugReportsAsync();
        Task<List<BugReport>> GetBugReportsForPageAsync(string pageUrl, int count = 10);
        Task LogBugToFileAsync(BugReport bugReport);
        Task<string> ExportBugReportsAsync(BugReportFilter filter, string format = "json");
        BugReport CreateBugReportFromContext(HttpContext context, string title, string description);
    }

    public class BugReportService : IBugReportService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<BugReportService> _logger;
        private readonly IWebHostEnvironment _environment;

        public BugReportService(SchedulerContext context, ILogger<BugReportService> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        public async Task<BugReport> CreateBugReportAsync(BugReport bugReport)
        {
            try
            {
                // Ensure unique BugId
                while (await _context.BugReports.AnyAsync(b => b.BugId == bugReport.BugId))
                {
                    bugReport.BugId = Guid.NewGuid().ToString("N")[..12];
                }

                // Set timestamps
                bugReport.ReportedDate = DateTime.UtcNow;
                bugReport.CreatedDate = DateTime.UtcNow;
                bugReport.LastModifiedDate = DateTime.UtcNow;

                _context.BugReports.Add(bugReport);
                await _context.SaveChangesAsync();

                // Log to file system
                await LogBugToFileAsync(bugReport);

                _logger.LogInformation("?? [BUG-REPORT-{BugId}] New bug report created: {Title} on {PageName}",
                    bugReport.BugId, bugReport.Title, bugReport.PageName);

                return bugReport;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT] Error creating bug report: {Title}", bugReport.Title);
                throw;
            }
        }

        public async Task<BugReport?> GetBugReportAsync(int id)
        {
            try
            {
                var bugReport = await _context.BugReports.FindAsync(id);
                if (bugReport != null)
                {
                    // Update view tracking
                    bugReport.ViewCount++;
                    bugReport.LastViewedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return bugReport;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT] Error getting bug report by ID: {Id}", id);
                return null;
            }
        }

        public async Task<BugReport?> GetBugReportByBugIdAsync(string bugId)
        {
            try
            {
                return await _context.BugReports.FirstOrDefaultAsync(b => b.BugId == bugId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT] Error getting bug report by BugId: {BugId}", bugId);
                return null;
            }
        }

        public async Task<List<BugReport>> GetBugReportsAsync(BugReportFilter filter)
        {
            try
            {
                var query = _context.BugReports.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    query = query.Where(b => b.Title.Contains(filter.SearchTerm) || 
                                           b.Description.Contains(filter.SearchTerm) ||
                                           b.BugId.Contains(filter.SearchTerm));
                }

                if (!string.IsNullOrEmpty(filter.Status))
                    query = query.Where(b => b.Status == filter.Status);

                if (!string.IsNullOrEmpty(filter.Severity))
                    query = query.Where(b => b.Severity == filter.Severity);

                if (!string.IsNullOrEmpty(filter.Priority))
                    query = query.Where(b => b.Priority == filter.Priority);

                if (!string.IsNullOrEmpty(filter.Category))
                    query = query.Where(b => b.Category == filter.Category);

                if (!string.IsNullOrEmpty(filter.PageArea))
                    query = query.Where(b => b.PageArea == filter.PageArea);

                if (!string.IsNullOrEmpty(filter.AssignedTo))
                    query = query.Where(b => b.AssignedTo == filter.AssignedTo);

                if (!string.IsNullOrEmpty(filter.ReportedBy))
                    query = query.Where(b => b.ReportedBy == filter.ReportedBy);

                if (filter.FromDate.HasValue)
                    query = query.Where(b => b.ReportedDate >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(b => b.ReportedDate <= filter.ToDate.Value);

                if (filter.IsHighPriority.HasValue && filter.IsHighPriority.Value)
                    query = query.Where(b => b.Priority == "Critical" || b.Priority == "High" || 
                                           b.Severity == "Critical" || b.Severity == "High");

                if (filter.IsRecent.HasValue && filter.IsRecent.Value)
                    query = query.Where(b => b.ReportedDate > DateTime.UtcNow.AddDays(-7));

                // Apply sorting
                query = filter.SortBy.ToLower() switch
                {
                    "title" => filter.SortDirection == "desc" ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
                    "status" => filter.SortDirection == "desc" ? query.OrderByDescending(b => b.Status) : query.OrderBy(b => b.Status),
                    "severity" => filter.SortDirection == "desc" ? query.OrderByDescending(b => b.Severity) : query.OrderBy(b => b.Severity),
                    "priority" => filter.SortDirection == "desc" ? query.OrderByDescending(b => b.Priority) : query.OrderBy(b => b.Priority),
                    "reportedby" => filter.SortDirection == "desc" ? query.OrderByDescending(b => b.ReportedBy) : query.OrderBy(b => b.ReportedBy),
                    _ => filter.SortDirection == "desc" ? query.OrderByDescending(b => b.ReportedDate) : query.OrderBy(b => b.ReportedDate)
                };

                // Apply pagination
                return await query.Skip((filter.Page - 1) * filter.PageSize)
                                 .Take(filter.PageSize)
                                 .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT] Error getting bug reports with filter");
                return new List<BugReport>();
            }
        }

        public async Task<BugReportStatistics> GetBugReportStatisticsAsync()
        {
            try
            {
                var stats = new BugReportStatistics();
                var allBugs = await _context.BugReports.Where(b => b.IsActive).ToListAsync();

                stats.TotalBugs = allBugs.Count;
                stats.NewBugs = allBugs.Count(b => b.Status == "New");
                stats.InProgressBugs = allBugs.Count(b => b.Status == "InProgress");
                stats.ResolvedBugs = allBugs.Count(b => b.Status == "Resolved" || b.Status == "Closed");
                stats.CriticalBugs = allBugs.Count(b => b.Severity == "Critical");
                stats.HighPriorityBugs = allBugs.Count(b => b.Priority == "Critical" || b.Priority == "High");
                stats.BugsThisWeek = allBugs.Count(b => b.ReportedDate > DateTime.UtcNow.AddDays(-7));
                stats.BugsThisMonth = allBugs.Count(b => b.ReportedDate > DateTime.UtcNow.AddDays(-30));

                // Calculate average resolution time
                var resolvedBugs = allBugs.Where(b => b.ResolvedDate.HasValue).ToList();
                if (resolvedBugs.Any())
                {
                    stats.AverageResolutionDays = (decimal)resolvedBugs.Average(b => (b.ResolvedDate!.Value - b.ReportedDate).TotalDays);
                }

                // Group statistics
                stats.BugsByPage = allBugs.GroupBy(b => b.PageArea).ToDictionary(g => g.Key ?? "Unknown", g => g.Count());
                stats.BugsByCategory = allBugs.GroupBy(b => b.Category).ToDictionary(g => g.Key, g => g.Count());
                stats.BugsBySeverity = allBugs.GroupBy(b => b.Severity).ToDictionary(g => g.Key, g => g.Count());

                // Recent and high priority bugs
                stats.RecentBugs = allBugs.OrderByDescending(b => b.ReportedDate).Take(5).ToList();
                stats.HighPriorityBugsList = allBugs.Where(b => b.Priority == "Critical" || b.Priority == "High")
                                               .OrderByDescending(b => b.ReportedDate).Take(10).ToList();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT] Error getting bug report statistics");
                return new BugReportStatistics();
            }
        }

        public async Task<BugReportStatistics> GetBugReportStatisticsForPageAsync(string pageArea)
        {
            try
            {
                var stats = new BugReportStatistics();
                var pageBugs = await _context.BugReports.Where(b => b.IsActive && b.PageArea == pageArea).ToListAsync();

                stats.TotalBugs = pageBugs.Count;
                stats.NewBugs = pageBugs.Count(b => b.Status == "New");
                stats.InProgressBugs = pageBugs.Count(b => b.Status == "InProgress");
                stats.ResolvedBugs = pageBugs.Count(b => b.Status == "Resolved" || b.Status == "Closed");
                stats.CriticalBugs = pageBugs.Count(b => b.Severity == "Critical");
                stats.HighPriorityBugs = pageBugs.Count(b => b.Priority == "Critical" || b.Priority == "High");
                stats.BugsThisWeek = pageBugs.Count(b => b.ReportedDate > DateTime.UtcNow.AddDays(-7));
                stats.BugsThisMonth = pageBugs.Count(b => b.ReportedDate > DateTime.UtcNow.AddDays(-30));

                stats.BugsByCategory = pageBugs.GroupBy(b => b.Category).ToDictionary(g => g.Key, g => g.Count());
                stats.BugsBySeverity = pageBugs.GroupBy(b => b.Severity).ToDictionary(g => g.Key, g => g.Count());

                stats.RecentBugs = pageBugs.OrderByDescending(b => b.ReportedDate).Take(5).ToList();
                stats.HighPriorityBugsList = pageBugs.Where(b => b.Priority == "Critical" || b.Priority == "High")
                                               .OrderByDescending(b => b.ReportedDate).Take(10).ToList();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT] Error getting bug report statistics for page: {PageArea}", pageArea);
                return new BugReportStatistics();
            }
        }

        public async Task<BugReport> UpdateBugReportAsync(BugReport bugReport)
        {
            try
            {
                bugReport.LastModifiedDate = DateTime.UtcNow;
                _context.BugReports.Update(bugReport);
                await _context.SaveChangesAsync();

                _logger.LogInformation("?? [BUG-REPORT-{BugId}] Bug report updated: {Title}", bugReport.BugId, bugReport.Title);
                return bugReport;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT] Error updating bug report: {BugId}", bugReport.BugId);
                throw;
            }
        }

        public async Task<bool> DeleteBugReportAsync(int id)
        {
            try
            {
                var bugReport = await _context.BugReports.FindAsync(id);
                if (bugReport != null)
                {
                    _context.BugReports.Remove(bugReport);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("??? [BUG-REPORT-{BugId}] Bug report deleted: {Title}", bugReport.BugId, bugReport.Title);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT] Error deleting bug report: {Id}", id);
                return false;
            }
        }

        public async Task<List<BugReport>> GetRecentBugReportsAsync(int count = 10)
        {
            try
            {
                return await _context.BugReports
                    .Where(b => b.IsActive)
                    .OrderByDescending(b => b.ReportedDate)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT] Error getting recent bug reports");
                return new List<BugReport>();
            }
        }

        public async Task<List<BugReport>> GetHighPriorityBugReportsAsync()
        {
            try
            {
                return await _context.BugReports
                    .Where(b => b.IsActive && (b.Priority == "Critical" || b.Priority == "High" || 
                                              b.Severity == "Critical" || b.Severity == "High"))
                    .OrderByDescending(b => b.ReportedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT] Error getting high priority bug reports");
                return new List<BugReport>();
            }
        }

        public async Task<List<BugReport>> GetBugReportsForPageAsync(string pageUrl, int count = 10)
        {
            try
            {
                return await _context.BugReports
                    .Where(b => b.IsActive && b.PageUrl.Contains(pageUrl))
                    .OrderByDescending(b => b.ReportedDate)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT] Error getting bug reports for page: {PageUrl}", pageUrl);
                return new List<BugReport>();
            }
        }

        public async Task LogBugToFileAsync(BugReport bugReport)
        {
            try
            {
                var logDirectory = Path.Combine(_environment.ContentRootPath, "logs", "bugs");
                Directory.CreateDirectory(logDirectory);

                // Master bug log
                var masterLogFile = Path.Combine(logDirectory, "master-bug-log.json");
                
                // Page-specific bug log
                var pageArea = string.IsNullOrEmpty(bugReport.PageArea) ? "general" : bugReport.PageArea.ToLower();
                var pageLogFile = Path.Combine(logDirectory, $"bugs-{pageArea}.json");

                var logEntry = new
                {
                    BugId = bugReport.BugId,
                    Title = bugReport.Title,
                    Description = bugReport.Description,
                    Severity = bugReport.Severity,
                    Priority = bugReport.Priority,
                    Status = bugReport.Status,
                    Category = bugReport.Category,
                    PageUrl = bugReport.PageUrl,
                    PageName = bugReport.PageName,
                    PageArea = bugReport.PageArea,
                    ReportedBy = bugReport.ReportedBy,
                    ReportedDate = bugReport.ReportedDate,
                    UserAgent = bugReport.UserAgent,
                    ErrorMessage = bugReport.ErrorMessage,
                    OperationId = bugReport.OperationId,
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var logJson = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });

                // Append to master log
                await File.AppendAllTextAsync(masterLogFile, logJson + Environment.NewLine + "---" + Environment.NewLine);

                // Append to page-specific log
                await File.AppendAllTextAsync(pageLogFile, logJson + Environment.NewLine + "---" + Environment.NewLine);

                _logger.LogInformation("?? [BUG-REPORT-{BugId}] Bug logged to files: master and {PageArea}", bugReport.BugId, pageArea);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT] Error logging bug to file: {BugId}", bugReport.BugId);
            }
        }

        public async Task<string> ExportBugReportsAsync(BugReportFilter filter, string format = "json")
        {
            try
            {
                var bugReports = await GetBugReportsAsync(filter);
                
                return format.ToLower() switch
                {
                    "json" => JsonSerializer.Serialize(bugReports, new JsonSerializerOptions { WriteIndented = true }),
                    "csv" => ConvertToCsv(bugReports),
                    _ => JsonSerializer.Serialize(bugReports, new JsonSerializerOptions { WriteIndented = true })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT] Error exporting bug reports");
                return string.Empty;
            }
        }

        public BugReport CreateBugReportFromContext(HttpContext context, string title, string description)
        {
            try
            {
                var request = context.Request;
                var user = context.User;

                var bugReport = new BugReport
                {
                    BugId = Guid.NewGuid().ToString("N")[..12],
                    Title = title,
                    Description = description,
                    PageUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
                    PageName = GetPageNameFromUrl(request.Path),
                    PageArea = GetPageAreaFromUrl(request.Path),
                    PageController = GetControllerFromUrl(request.Path),
                    PageAction = GetActionFromUrl(request.Path),
                    ReportedBy = user.Identity?.Name ?? "Anonymous",
                    UserRole = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "",
                    UserEmail = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "",
                    UserAgent = request.Headers["User-Agent"].ToString(),
                    BrowserName = GetBrowserName(request.Headers["User-Agent"].ToString()),
                    BrowserVersion = GetBrowserVersion(request.Headers["User-Agent"].ToString()),
                    OperatingSystem = GetOperatingSystem(request.Headers["User-Agent"].ToString()),
                    IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "",
                    ReportedDate = DateTime.UtcNow,
                    CreatedBy = user.Identity?.Name ?? "System",
                    Status = "New",
                    Severity = "Medium",
                    Priority = "Medium",
                    Category = "General"
                };

                return bugReport;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT] Error creating bug report from context");
                throw;
            }
        }

        // Helper methods
        private string GetPageNameFromUrl(string path)
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length > 0 ? segments.Last() : "Home";
        }

        private string GetPageAreaFromUrl(string path)
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length > 0 ? segments.First() : "General";
        }

        private string GetControllerFromUrl(string path)
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length > 1 ? segments[segments.Length - 2] : "";
        }

        private string GetActionFromUrl(string path)
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length > 0 ? segments.Last() : "";
        }

        private string GetBrowserName(string userAgent)
        {
            if (userAgent.Contains("Chrome")) return "Chrome";
            if (userAgent.Contains("Firefox")) return "Firefox";
            if (userAgent.Contains("Safari")) return "Safari";
            if (userAgent.Contains("Edge")) return "Edge";
            if (userAgent.Contains("Opera")) return "Opera";
            return "Unknown";
        }

        private string GetBrowserVersion(string userAgent)
        {
            // Simplified version extraction
            try
            {
                if (userAgent.Contains("Chrome"))
                {
                    var start = userAgent.IndexOf("Chrome/") + 7;
                    var end = userAgent.IndexOf(" ", start);
                    return userAgent.Substring(start, end - start);
                }
                // Add more browser version extraction logic as needed
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetOperatingSystem(string userAgent)
        {
            if (userAgent.Contains("Windows")) return "Windows";
            if (userAgent.Contains("Mac")) return "macOS";
            if (userAgent.Contains("Linux")) return "Linux";
            if (userAgent.Contains("Android")) return "Android";
            if (userAgent.Contains("iOS")) return "iOS";
            return "Unknown";
        }

        private string ConvertToCsv(List<BugReport> bugReports)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("BugId,Title,Description,Severity,Priority,Status,Category,PageArea,ReportedBy,ReportedDate");
            
            foreach (var bug in bugReports)
            {
                csv.AppendLine($"\"{bug.BugId}\",\"{bug.Title}\",\"{bug.Description}\",\"{bug.Severity}\",\"{bug.Priority}\",\"{bug.Status}\",\"{bug.Category}\",\"{bug.PageArea}\",\"{bug.ReportedBy}\",\"{bug.ReportedDate}\"");
            }
            
            return csv.ToString();
        }
    }
}