using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpCentrix.Models;
using OpCentrix.Services;
using System.Text.Json;

namespace OpCentrix.Controllers.Api
{
    /// <summary>
    /// API Controller for handling bug reports
    /// Provides endpoints for submitting and managing bug reports across all OpCentrix pages
    /// </summary>
    [Route("Api/[controller]")]
    [ApiController]
    public class BugReportController : ControllerBase
    {
        private readonly IBugReportService _bugReportService;
        private readonly ILogger<BugReportController> _logger;

        public BugReportController(IBugReportService bugReportService, ILogger<BugReportController> logger)
        {
            _bugReportService = bugReportService;
            _logger = logger;
        }

        /// <summary>
        /// Submit a new bug report
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitBugReport([FromForm] BugReportSubmission submission)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformation("?? [BUG-REPORT-API-{OperationId}] Receiving bug report submission: {Title}", 
                    operationId, submission.Title);

                // Validate required fields
                if (string.IsNullOrEmpty(submission.Title) || string.IsNullOrEmpty(submission.Description))
                {
                    return BadRequest(new { success = false, message = "Title and description are required" });
                }

                // Create bug report from submission
                var bugReport = CreateBugReportFromSubmission(submission);
                
                // Auto-fill context from HTTP request
                FillContextFromRequest(bugReport);

                // Save to database and log to files
                var savedBugReport = await _bugReportService.CreateBugReportAsync(bugReport);

                _logger.LogInformation("? [BUG-REPORT-API-{OperationId}] Bug report created successfully: {BugId}", 
                    operationId, savedBugReport.BugId);

                return Ok(new { 
                    success = true, 
                    message = "Bug report submitted successfully",
                    bugId = savedBugReport.BugId,
                    id = savedBugReport.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT-API-{OperationId}] Error submitting bug report: {Title}", 
                    operationId, submission.Title);
                
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while submitting the bug report" 
                });
            }
        }

        /// <summary>
        /// Get bug report statistics for a specific page area
        /// </summary>
        [HttpGet("Stats")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetBugStats([FromQuery] string? pageArea = null)
        {
            try
            {
                _logger.LogInformation("?? [BUG-REPORT-API] Getting bug statistics for page area: {PageArea}", pageArea ?? "all");

                var stats = string.IsNullOrEmpty(pageArea) 
                    ? await _bugReportService.GetBugReportStatisticsAsync()
                    : await _bugReportService.GetBugReportStatisticsForPageAsync(pageArea);

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT-API] Error getting bug statistics for page area: {PageArea}", pageArea);
                return StatusCode(500, new { success = false, message = "Error loading bug statistics" });
            }
        }

        /// <summary>
        /// Get bug reports with filtering
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetBugReports([FromQuery] BugReportFilter filter)
        {
            try
            {
                _logger.LogInformation("?? [BUG-REPORT-API] Getting bug reports with filter: Page {Page}, Size {PageSize}", 
                    filter.Page, filter.PageSize);

                var bugReports = await _bugReportService.GetBugReportsAsync(filter);
                return Ok(bugReports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT-API] Error getting bug reports");
                return StatusCode(500, new { success = false, message = "Error loading bug reports" });
            }
        }

        /// <summary>
        /// Get a specific bug report by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetBugReport(int id)
        {
            try
            {
                var bugReport = await _bugReportService.GetBugReportAsync(id);
                if (bugReport == null)
                {
                    return NotFound(new { success = false, message = "Bug report not found" });
                }

                return Ok(bugReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT-API] Error getting bug report: {Id}", id);
                return StatusCode(500, new { success = false, message = "Error loading bug report" });
            }
        }

        /// <summary>
        /// Update a bug report (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateBugReport(int id, [FromBody] BugReport bugReport)
        {
            try
            {
                if (id != bugReport.Id)
                {
                    return BadRequest(new { success = false, message = "ID mismatch" });
                }

                bugReport.LastModifiedBy = User.Identity?.Name ?? "System";
                var updatedBugReport = await _bugReportService.UpdateBugReportAsync(bugReport);

                _logger.LogInformation("?? [BUG-REPORT-API] Bug report updated: {BugId}", updatedBugReport.BugId);

                return Ok(new { 
                    success = true, 
                    message = "Bug report updated successfully",
                    bugReport = updatedBugReport
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT-API] Error updating bug report: {Id}", id);
                return StatusCode(500, new { success = false, message = "Error updating bug report" });
            }
        }

        /// <summary>
        /// Delete a bug report (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteBugReport(int id)
        {
            try
            {
                var success = await _bugReportService.DeleteBugReportAsync(id);
                if (!success)
                {
                    return NotFound(new { success = false, message = "Bug report not found" });
                }

                _logger.LogInformation("??? [BUG-REPORT-API] Bug report deleted: {Id}", id);

                return Ok(new { success = true, message = "Bug report deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT-API] Error deleting bug report: {Id}", id);
                return StatusCode(500, new { success = false, message = "Error deleting bug report" });
            }
        }

        /// <summary>
        /// Export bug reports (Admin only)
        /// </summary>
        [HttpGet("Export")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ExportBugReports([FromQuery] BugReportFilter filter, [FromQuery] string format = "json")
        {
            try
            {
                var exportData = await _bugReportService.ExportBugReportsAsync(filter, format);
                
                var contentType = format.ToLower() switch
                {
                    "csv" => "text/csv",
                    _ => "application/json"
                };

                var fileName = $"bug-reports-{DateTime.UtcNow:yyyy-MM-dd}.{format}";

                return File(System.Text.Encoding.UTF8.GetBytes(exportData), contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [BUG-REPORT-API] Error exporting bug reports");
                return StatusCode(500, new { success = false, message = "Error exporting bug reports" });
            }
        }

        // Helper methods
        private BugReport CreateBugReportFromSubmission(BugReportSubmission submission)
        {
            return new BugReport
            {
                BugId = submission.BugId ?? Guid.NewGuid().ToString("N")[..12],
                Title = submission.Title,
                Description = submission.Description,
                Severity = submission.Severity ?? "Medium",
                Priority = submission.Priority ?? "Medium",
                Status = "New",
                Category = submission.Category ?? "General",
                PageUrl = submission.PageUrl ?? "",
                PageName = submission.PageName ?? "",
                PageArea = submission.PageArea ?? "",
                PageController = submission.PageController ?? "",
                PageAction = submission.PageAction ?? "",
                ReportedBy = submission.ReportedBy ?? "Anonymous",
                UserRole = submission.UserRole ?? "",
                UserEmail = submission.UserEmail ?? "",
                StepsToReproduce = submission.StepsToReproduce ?? "",
                ExpectedBehavior = submission.ExpectedBehavior ?? "",
                ActualBehavior = submission.ActualBehavior ?? "",
                AdditionalNotes = submission.AdditionalNotes ?? "",
                UserAgent = submission.UserAgent ?? "",
                BrowserName = submission.BrowserName ?? "",
                BrowserVersion = submission.BrowserVersion ?? "",
                OperatingSystem = submission.OperatingSystem ?? "",
                ScreenResolution = submission.ScreenResolution ?? "",
                FormData = submission.FormData ?? "",
                ConsoleErrors = submission.ConsoleErrors ?? "",
                OperationId = submission.OperationId ?? "",
                ReportedDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "System"
            };
        }

        private void FillContextFromRequest(BugReport bugReport)
        {
            // Fill IP address
            bugReport.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

            // Update user information from current context if not provided
            if (string.IsNullOrEmpty(bugReport.ReportedBy))
            {
                bugReport.ReportedBy = User.Identity?.Name ?? "Anonymous";
            }

            if (string.IsNullOrEmpty(bugReport.UserRole))
            {
                bugReport.UserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            }

            if (string.IsNullOrEmpty(bugReport.UserEmail))
            {
                bugReport.UserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
            }

            // Update user agent if not provided
            if (string.IsNullOrEmpty(bugReport.UserAgent))
            {
                bugReport.UserAgent = Request.Headers["User-Agent"].ToString();
            }
        }
    }

    /// <summary>
    /// Model for bug report form submission
    /// </summary>
    public class BugReportSubmission
    {
        public string? BugId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string? Severity { get; set; }
        public string? Priority { get; set; }
        public string? Category { get; set; }
        public string? PageUrl { get; set; }
        public string? PageName { get; set; }
        public string? PageArea { get; set; }
        public string? PageController { get; set; }
        public string? PageAction { get; set; }
        public string? ReportedBy { get; set; }
        public string? UserRole { get; set; }
        public string? UserEmail { get; set; }
        public string? StepsToReproduce { get; set; }
        public string? ExpectedBehavior { get; set; }
        public string? ActualBehavior { get; set; }
        public string? AdditionalNotes { get; set; }
        public string? UserAgent { get; set; }
        public string? BrowserName { get; set; }
        public string? BrowserVersion { get; set; }
        public string? OperatingSystem { get; set; }
        public string? ScreenResolution { get; set; }
        public string? FormData { get; set; }
        public string? ConsoleErrors { get; set; }
        public string? OperationId { get; set; }
        public string? ReportedDate { get; set; }
    }
}