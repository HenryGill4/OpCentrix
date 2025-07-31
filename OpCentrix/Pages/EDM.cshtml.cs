using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpCentrix.Authorization;
using OpCentrix.Data;
using OpCentrix.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace OpCentrix.Pages
{
    [Authorize]
    [EDMAccess]
    public class EDMModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<EDMModel> _logger;

        public EDMModel(SchedulerContext context, ILogger<EDMModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Statistics for dashboard
        public int TotalLogsCount { get; set; }
        public int TodayLogsCount { get; set; }
        public string LastLogTime { get; set; } = "Never";
        public int NextLogNumber { get; set; } = 1;

        // Recent logs for display
        public List<EDMLog> RecentLogs { get; set; } = new();
        public List<Part> AvailableParts { get; set; } = new();

        [BindProperty]
        public EDMLogEntry LogEntry { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                await LoadDashboardDataAsync();
                await LoadRecentLogsAsync();
                await LoadAvailablePartsAsync();
                await SetNextLogNumberAsync();

                _logger.LogInformation("EDM Operations page loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading EDM Operations page");
                await InitializeDefaultsAsync();
            }
        }

        public async Task<IActionResult> OnPostSaveLogAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadDashboardDataAsync();
                    return Page();
                }

                // Check if this is an update operation
                if (LogEntry.Id > 0)
                {
                    return await OnPostUpdateLogAsync();
                }

                // Generate log number for new entries
                var logNumber = await GenerateLogNumberAsync();

                // Create EDM log entry
                var edmLog = new EDMLog
                {
                    LogNumber = logNumber,
                    PartNumber = LogEntry.PartNumber,
                    Quantity = LogEntry.Quantity,
                    LogDate = LogEntry.LogDate,
                    Shift = LogEntry.Shift ?? "",
                    OperatorName = LogEntry.OperatorName,
                    OperatorInitials = LogEntry.OperatorInitials,
                    StartTime = LogEntry.StartTime ?? "",
                    EndTime = LogEntry.EndTime ?? "",
                    Measurement1 = LogEntry.Measurement1 ?? "",
                    Measurement2 = LogEntry.Measurement2 ?? "",
                    ToleranceStatus = LogEntry.ToleranceStatus ?? "",
                    ScrapIssues = LogEntry.ScrapIssues ?? "",
                    Notes = LogEntry.Notes ?? "",
                    TotalTime = CalculateTotalTime(LogEntry.StartTime, LogEntry.EndTime),
                    CreatedBy = User?.Identity?.Name ?? "System",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = User?.Identity?.Name ?? "System",
                    LastModifiedDate = DateTime.UtcNow,
                    IsActive = true,
                    MachineUsed = LogEntry.MachineUsed ?? "",
                    ProcessType = "EDM",
                    QualityNotes = LogEntry.QualityNotes ?? "",
                    IsCompleted = true,
                    RequiresReview = false
                };

                // Try to link to existing part
                var existingPart = await _context.Parts
                    .FirstOrDefaultAsync(p => p.PartNumber == LogEntry.PartNumber && p.IsActive);
                if (existingPart != null)
                {
                    edmLog.PartId = existingPart.Id;
                }

                // Save to database
                _context.EDMLogs.Add(edmLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("EDM Log {LogNumber} created successfully by {User}", 
                    logNumber, User?.Identity?.Name ?? "System");

                // Return success response for AJAX
                if (Request.Headers.Accept.ToString().Contains("application/json"))
                {
                    return new JsonResult(new { 
                        success = true, 
                        logNumber = logNumber,
                        message = $"EDM Log #{logNumber} saved successfully!" 
                    });
                }

                TempData["Success"] = $"EDM Log #{logNumber} saved successfully!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving EDM log entry");
                
                if (Request.Headers.Accept.ToString().Contains("application/json"))
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Failed to save EDM log. Please try again." 
                    });
                }

                TempData["Error"] = "Failed to save EDM log. Please try again.";
                await LoadDashboardDataAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnGetLogsJsonAsync()
        {
            try
            {
                var logs = await _context.EDMLogs
                    .Where(l => l.IsActive)
                    .OrderByDescending(l => l.CreatedDate)
                    .Take(50)
                    .Select(l => new
                    {
                        l.Id,
                        l.LogNumber,
                        l.PartNumber,
                        l.Quantity,
                        l.LogDate,
                        l.OperatorName,
                        l.OperatorInitials,
                        l.ToleranceStatus,
                        l.Notes,
                        l.CreatedDate
                    })
                    .ToListAsync();

                return new JsonResult(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving EDM logs");
                return new JsonResult(new { error = "Failed to retrieve logs" });
            }
        }

        public async Task<IActionResult> OnGetLogDetailAsync(int id)
        {
            try
            {
                var log = await _context.EDMLogs
                    .Where(l => l.Id == id && l.IsActive)
                    .Select(l => new
                    {
                        l.Id,
                        l.LogNumber,
                        l.PartNumber,
                        l.Quantity,
                        l.LogDate,
                        l.Shift,
                        l.OperatorName,
                        l.OperatorInitials,
                        l.StartTime,
                        l.EndTime,
                        l.Measurement1,
                        l.Measurement2,
                        l.ToleranceStatus,
                        l.ScrapIssues,
                        l.Notes,
                        l.TotalTime,
                        l.CreatedDate,
                        l.CreatedBy
                    })
                    .FirstOrDefaultAsync();

                if (log == null)
                {
                    return new JsonResult(new { error = "Log not found" });
                }

                return new JsonResult(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving EDM log detail for ID: {LogId}", id);
                return new JsonResult(new { error = "Failed to retrieve log details" });
            }
        }

        public async Task<IActionResult> OnPostDeleteLogAsync(int id)
        {
            try
            {
                var log = await _context.EDMLogs
                    .FirstOrDefaultAsync(l => l.Id == id && l.IsActive);

                if (log == null)
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Log not found or already deleted" 
                    });
                }

                // Soft delete - mark as inactive
                log.IsActive = false;
                log.LastModifiedBy = User?.Identity?.Name ?? "System";
                log.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("EDM Log {LogNumber} (ID: {LogId}) deleted by {User}", 
                    log.LogNumber, id, User?.Identity?.Name ?? "System");

                return new JsonResult(new { 
                    success = true, 
                    message = $"Log #{log.LogNumber} deleted successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting EDM log with ID: {LogId}", id);
                return new JsonResult(new { 
                    success = false, 
                    message = "Failed to delete log. Please try again." 
                });
            }
        }

        public async Task<IActionResult> OnPostUpdateLogAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Invalid data provided" 
                    });
                }

                var existingLog = await _context.EDMLogs
                    .FirstOrDefaultAsync(l => l.Id == LogEntry.Id && l.IsActive);

                if (existingLog == null)
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Log not found" 
                    });
                }

                // Update the existing log
                existingLog.PartNumber = LogEntry.PartNumber;
                existingLog.Quantity = LogEntry.Quantity;
                existingLog.LogDate = LogEntry.LogDate;
                existingLog.Shift = LogEntry.Shift ?? "";
                existingLog.OperatorName = LogEntry.OperatorName;
                existingLog.OperatorInitials = LogEntry.OperatorInitials;
                existingLog.StartTime = LogEntry.StartTime ?? "";
                existingLog.EndTime = LogEntry.EndTime ?? "";
                existingLog.Measurement1 = LogEntry.Measurement1 ?? "";
                existingLog.Measurement2 = LogEntry.Measurement2 ?? "";
                existingLog.ToleranceStatus = LogEntry.ToleranceStatus ?? "";
                existingLog.ScrapIssues = LogEntry.ScrapIssues ?? "";
                existingLog.Notes = LogEntry.Notes ?? "";
                existingLog.TotalTime = CalculateTotalTime(LogEntry.StartTime, LogEntry.EndTime);
                existingLog.LastModifiedBy = User?.Identity?.Name ?? "System";
                existingLog.LastModifiedDate = DateTime.UtcNow;

                // Try to link to existing part
                var existingPart = await _context.Parts
                    .FirstOrDefaultAsync(p => p.PartNumber == LogEntry.PartNumber && p.IsActive);
                if (existingPart != null)
                {
                    existingLog.PartId = existingPart.Id;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("EDM Log {LogNumber} (ID: {LogId}) updated by {User}", 
                    existingLog.LogNumber, existingLog.Id, User?.Identity?.Name ?? "System");

                return new JsonResult(new { 
                    success = true, 
                    message = $"Log #{existingLog.LogNumber} updated successfully",
                    logNumber = existingLog.LogNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating EDM log");
                return new JsonResult(new { 
                    success = false, 
                    message = "Failed to update log. Please try again." 
                });
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            var today = DateTime.Today;
            
            TotalLogsCount = await _context.EDMLogs.CountAsync(l => l.IsActive);
            TodayLogsCount = await _context.EDMLogs.CountAsync(l => l.IsActive && l.LogDate == today);
            
            var lastLog = await _context.EDMLogs
                .Where(l => l.IsActive)
                .OrderByDescending(l => l.CreatedDate)
                .FirstOrDefaultAsync();
            
            LastLogTime = lastLog?.CreatedDate.ToString("yyyy-MM-dd HH:mm") ?? "Never";
        }

        private async Task LoadRecentLogsAsync()
        {
            RecentLogs = await _context.EDMLogs
                .Where(l => l.IsActive)
                .OrderByDescending(l => l.CreatedDate)
                .Take(10)
                .ToListAsync();
        }

        private async Task LoadAvailablePartsAsync()
        {
            AvailableParts = await _context.Parts
                .Where(p => p.IsActive)
                .OrderBy(p => p.PartNumber)
                .ToListAsync();
        }

        private async Task SetNextLogNumberAsync()
        {
            var lastLog = await _context.EDMLogs
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync();

            if (lastLog != null && int.TryParse(lastLog.LogNumber, out int lastNumber))
            {
                NextLogNumber = lastNumber + 1;
            }
            else
            {
                NextLogNumber = 1;
            }
        }

        private async Task<string> GenerateLogNumberAsync()
        {
            var lastLog = await _context.EDMLogs
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastLog != null && int.TryParse(lastLog.LogNumber, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            return nextNumber.ToString();
        }

        private string CalculateTotalTime(string? startTime, string? endTime)
        {
            if (string.IsNullOrWhiteSpace(startTime) || string.IsNullOrWhiteSpace(endTime))
                return "N/A";

            try
            {
                if (TimeSpan.TryParse(startTime, out var start) && TimeSpan.TryParse(endTime, out var end))
                {
                    var diff = end - start;
                    if (diff.TotalMinutes < 0)
                        diff = diff.Add(TimeSpan.FromDays(1)); // Handle overnight

                    var hours = (int)diff.TotalHours;
                    var minutes = diff.Minutes;

                    return hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating time difference between {StartTime} and {EndTime}", startTime, endTime);
            }

            return "N/A";
        }

        private async Task InitializeDefaultsAsync()
        {
            TotalLogsCount = 0;
            TodayLogsCount = 0;
            LastLogTime = "Never";
            NextLogNumber = 1;
            RecentLogs = new List<EDMLog>();
            AvailableParts = new List<Part>();
        }
    }

    public class EDMLogEntry
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, 9999)]
        public int Quantity { get; set; }

        [Required]
        public DateTime LogDate { get; set; } = DateTime.Today;

        public string? Shift { get; set; }

        [Required]
        [StringLength(100)]
        public string OperatorName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string OperatorInitials { get; set; } = string.Empty;

        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? Measurement1 { get; set; }
        public string? Measurement2 { get; set; }
        public string? ToleranceStatus { get; set; }
        public string? ScrapIssues { get; set; }
        public string? Notes { get; set; }
        public string? MachineUsed { get; set; }
        public string? QualityNotes { get; set; }
    }
}
