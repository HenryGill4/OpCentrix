using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Services.Admin;
using System.Net.Mime;

namespace OpCentrix.Pages.Admin;

/// <summary>
/// Admin page for viewing system logs
/// Task 2.5: Global Logging and Error Handling
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class LogsModel : PageModel
{
    private readonly ILogViewerService _logViewerService;
    private readonly ILogger<LogsModel> _logger;

    public LogsModel(ILogViewerService logViewerService, ILogger<LogsModel> logger)
    {
        _logViewerService = logViewerService;
        _logger = logger;
    }

    // Properties for the page
    public List<LogEntry> LogEntries { get; set; } = new();
    public List<string> LogLevels { get; set; } = new();
    public List<string> LogFiles { get; set; } = new();
    public LogStatistics Statistics { get; set; } = new();

    // Filter properties
    [BindProperty(SupportsGet = true)]
    public string? LevelFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 50;

    public int TotalPages { get; set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Loading admin logs page - User: {User}, Page: {Page}, Level: {Level}, Search: {Search}",
                User.Identity?.Name, CurrentPage, LevelFilter, SearchTerm);

            // Load log data
            LogEntries = await _logViewerService.GetLogsAsync(CurrentPage, PageSize, LevelFilter, SearchTerm);
            LogLevels = await _logViewerService.GetLogLevelsAsync();
            LogFiles = await _logViewerService.GetLogFilesAsync();
            Statistics = await _logViewerService.GetLogStatisticsAsync();

            // Calculate pagination (simplified since we're reading from files)
            var totalEntries = Statistics.TotalEntries;
            TotalPages = (int)Math.Ceiling((double)totalEntries / PageSize);

            _logger.LogInformation("Admin logs page loaded successfully - {EntryCount} entries, {PageCount} pages",
                LogEntries.Count, TotalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin logs page");

            // Initialize with empty data on error
            LogEntries = new List<LogEntry>();
            LogLevels = new List<string>();
            LogFiles = new List<string>();
            Statistics = new LogStatistics();
            TotalPages = 1;
        }
    }

    public async Task<IActionResult> OnPostClearLogsAsync()
    {
        try
        {
            _logger.LogWarning("Admin {User} is clearing all log files", User.Identity?.Name);

            var success = await _logViewerService.ClearLogsAsync();

            if (success)
            {
                _logger.LogInformation("Log files cleared successfully by {User}", User.Identity?.Name);
                TempData["Success"] = "All log files have been cleared successfully.";
            }
            else
            {
                _logger.LogError("Failed to clear log files");
                TempData["Error"] = "Failed to clear log files. Please check permissions.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing log files");
            TempData["Error"] = "An error occurred while clearing log files.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDownloadLogAsync(string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("File name is required");

            _logger.LogInformation("Admin {User} downloading log file: {FileName}", User.Identity?.Name, fileName);

            var content = await _logViewerService.GetLogFileContentAsync(fileName);
            if (string.IsNullOrEmpty(content))
                return NotFound("Log file not found");

            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            return File(bytes, "text/plain", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading log file {FileName}", fileName);
            return BadRequest("Error downloading log file");
        }
    }

    public async Task<IActionResult> OnGetRefreshAsync()
    {
        try
        {
            _logger.LogInformation("Admin {User} refreshing logs page", User.Identity?.Name);
            await OnGetAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing logs page");
            TempData["Error"] = "Error refreshing logs.";
            return Page();
        }
    }

    // Helper methods for the view
    public string GetLogLevelBadgeClass(string level)
    {
        return level.ToLower() switch
        {
            "information" or "info" => "bg-blue-100 text-blue-800",
            "warning" or "warn" => "bg-yellow-100 text-yellow-800",
            "error" => "bg-red-100 text-red-800",
            "debug" => "bg-gray-100 text-gray-800",
            "fatal" => "bg-red-200 text-red-900 font-bold",
            _ => "bg-gray-100 text-gray-600"
        };
    }

    public string FormatTimestamp(DateTime timestamp)
    {
        return timestamp.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public string TruncateMessage(string message, int maxLength = 100)
    {
        if (string.IsNullOrEmpty(message) || message.Length <= maxLength)
            return message;

        return message.Substring(0, maxLength) + "...";
    }
}