using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using OpCentrix.Services;

namespace OpCentrix.Pages.Admin
{
    /// <summary>
    /// Admin page for managing bug reports
    /// View, filter, and manage bug reports submitted from all pages
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class BugReportsModel : PageModel
    {
        private readonly IBugReportService _bugReportService;
        private readonly ILogger<BugReportsModel> _logger;

        public BugReportsModel(IBugReportService bugReportService, ILogger<BugReportsModel> logger)
        {
            _bugReportService = bugReportService;
            _logger = logger;
        }

        // Properties for display
        public List<BugReport> BugReports { get; set; } = new();
        public BugReportStatistics Statistics { get; set; } = new();
        public int TotalPages { get; set; }
        public bool HasPrevious => Filter.Page > 1;
        public bool HasNext => Filter.Page < TotalPages;

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public BugReportFilter Filter { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var operationId = Guid.NewGuid().ToString("N")[..8];
                _logger.LogInformation("?? [ADMIN-BUG-REPORTS-{OperationId}] Loading bug reports page - User: {User}, Page: {Page}",
                    operationId, User.Identity?.Name, Filter.Page);

                // Load bug reports and statistics
                BugReports = await _bugReportService.GetBugReportsAsync(Filter);
                Statistics = await _bugReportService.GetBugReportStatisticsAsync();

                // Calculate pagination (simplified estimate)
                TotalPages = Math.Max(1, (int)Math.Ceiling((double)Statistics.TotalBugs / Filter.PageSize));

                _logger.LogInformation("? [ADMIN-BUG-REPORTS-{OperationId}] Bug reports loaded - {Count} reports, {Pages} pages",
                    operationId, BugReports.Count, TotalPages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [ADMIN-BUG-REPORTS] Error loading bug reports page");
                BugReports = new List<BugReport>();
                Statistics = new BugReportStatistics();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var operationId = Guid.NewGuid().ToString("N")[..8];
                _logger.LogInformation("??? [ADMIN-BUG-REPORTS-{OperationId}] Deleting bug report: {Id} by {User}",
                    operationId, id, User.Identity?.Name);

                var success = await _bugReportService.DeleteBugReportAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Bug report deleted successfully.";
                    _logger.LogInformation("? [ADMIN-BUG-REPORTS-{OperationId}] Bug report deleted successfully: {Id}",
                        operationId, id);
                }
                else
                {
                    TempData["ErrorMessage"] = "Bug report not found or could not be deleted.";
                    _logger.LogWarning("?? [ADMIN-BUG-REPORTS-{OperationId}] Bug report not found for deletion: {Id}",
                        operationId, id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [ADMIN-BUG-REPORTS] Error deleting bug report: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the bug report.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
        {
            try
            {
                var operationId = Guid.NewGuid().ToString("N")[..8];
                _logger.LogInformation("?? [ADMIN-BUG-REPORTS-{OperationId}] Updating bug report status: {Id} to {Status} by {User}",
                    operationId, id, status, User.Identity?.Name);

                var bugReport = await _bugReportService.GetBugReportAsync(id);
                if (bugReport != null)
                {
                    bugReport.Status = status;
                    bugReport.LastModifiedBy = User.Identity?.Name ?? "System";
                    
                    if (status == "Resolved" && !bugReport.ResolvedDate.HasValue)
                    {
                        bugReport.ResolvedDate = DateTime.UtcNow;
                        bugReport.ResolvedBy = User.Identity?.Name ?? "System";
                    }

                    await _bugReportService.UpdateBugReportAsync(bugReport);
                    TempData["SuccessMessage"] = $"Bug report status updated to {status}.";
                    
                    _logger.LogInformation("? [ADMIN-BUG-REPORTS-{OperationId}] Bug report status updated: {Id} to {Status}",
                        operationId, id, status);
                }
                else
                {
                    TempData["ErrorMessage"] = "Bug report not found.";
                    _logger.LogWarning("?? [ADMIN-BUG-REPORTS-{OperationId}] Bug report not found for status update: {Id}",
                        operationId, id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [ADMIN-BUG-REPORTS] Error updating bug report status: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while updating the bug report status.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetExportAsync(string format = "json")
        {
            try
            {
                var operationId = Guid.NewGuid().ToString("N")[..8];
                _logger.LogInformation("?? [ADMIN-BUG-REPORTS-{OperationId}] Exporting bug reports as {Format} by {User}",
                    operationId, format, User.Identity?.Name);

                var exportData = await _bugReportService.ExportBugReportsAsync(Filter, format);
                
                var contentType = format.ToLower() switch
                {
                    "csv" => "text/csv",
                    _ => "application/json"
                };

                var fileName = $"bug-reports-{DateTime.UtcNow:yyyy-MM-dd}.{format}";

                _logger.LogInformation("? [ADMIN-BUG-REPORTS-{OperationId}] Bug reports exported successfully as {Format}",
                    operationId, format);

                return File(System.Text.Encoding.UTF8.GetBytes(exportData), contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [ADMIN-BUG-REPORTS] Error exporting bug reports");
                TempData["ErrorMessage"] = "An error occurred while exporting bug reports.";
                return RedirectToPage();
            }
        }

        // Helper methods for display
        public string GetSeverityClass(string severity) => severity.ToLower() switch
        {
            "critical" => "bg-red-100 text-red-800",
            "high" => "bg-orange-100 text-orange-800",
            "medium" => "bg-yellow-100 text-yellow-800",
            "low" => "bg-green-100 text-green-800",
            _ => "bg-gray-100 text-gray-800"
        };

        public string GetStatusClass(string status) => status.ToLower() switch
        {
            "new" => "bg-blue-100 text-blue-800",
            "inprogress" => "bg-orange-100 text-orange-800",
            "resolved" => "bg-green-100 text-green-800",
            "closed" => "bg-gray-100 text-gray-800",
            "reopened" => "bg-red-100 text-red-800",
            _ => "bg-gray-100 text-gray-800"
        };

        public string GetCategoryClass(string category) => category.ToLower() switch
        {
            "ui" => "bg-purple-100 text-purple-800",
            "backend" => "bg-indigo-100 text-indigo-800",
            "database" => "bg-blue-100 text-blue-800",
            "performance" => "bg-yellow-100 text-yellow-800",
            "security" => "bg-red-100 text-red-800",
            "forms" => "bg-green-100 text-green-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }
}