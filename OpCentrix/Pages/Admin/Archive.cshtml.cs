using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using OpCentrix.Services.Admin;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.Admin;

/// <summary>
/// Admin page for managing job archive and cleanup operations
/// Task 15: Job Archive & Cleanup - Complete data management interface
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class ArchiveModel : PageModel
{
    private readonly IJobArchiveService _jobArchiveService;
    private readonly ILogger<ArchiveModel> _logger;

    public ArchiveModel(IJobArchiveService jobArchiveService, ILogger<ArchiveModel> logger)
    {
        _jobArchiveService = jobArchiveService;
        _logger = logger;
    }

    // Page Properties
    public List<ArchivedJob> ArchivedJobs { get; set; } = new();
    public List<Job> EligibleJobs { get; set; } = new();
    public Dictionary<string, object> ArchiveStatistics { get; set; } = new();
    public Dictionary<string, int> CleanupRecommendations { get; set; } = new();
    public List<string> ArchiveReasons { get; set; } = new();
    public List<string> ArchiveMachines { get; set; } = new();

    // Filtering and Search
    [BindProperty(SupportsGet = true)]
    public string SearchTerm { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string MachineFilter { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string StatusFilter { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string ReasonFilter { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public string ActiveTab { get; set; } = "archive";

    [BindProperty(SupportsGet = true)]
    public int EligibleDays { get; set; } = 30;

    // Form Input Models
    [BindProperty]
    public ArchiveJobInputModel ArchiveJobInput { get; set; } = new();

    [BindProperty]
    public BulkArchiveInputModel BulkArchiveInput { get; set; } = new();

    [BindProperty]
    public CleanupInputModel CleanupInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Loading job archive management page - Admin: {Admin}", User.Identity?.Name);

            // Clear any existing model state issues for GET requests
            ModelState.Clear();

            // Load data based on active tab
            await LoadDataForActiveTabAsync();

            _logger.LogInformation("Archive page loaded - Tab: {ActiveTab}, ArchivedJobs: {Count}", ActiveTab, ArchivedJobs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading job archive management page");
            TempData["Error"] = "Error loading archive data. Please try again.";
            
            // Initialize with empty data on error
            ArchivedJobs = new List<ArchivedJob>();
            EligibleJobs = new List<Job>();
            ArchiveStatistics = new Dictionary<string, object>();
            CleanupRecommendations = new Dictionary<string, int>();
        }
    }

    public async Task<IActionResult> OnPostArchiveJobAsync()
    {
        try
        {
            _logger.LogInformation("Archiving single job: {JobId}", ArchiveJobInput.JobId);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the validation errors and try again.";
                await LoadDataForActiveTabAsync();
                return Page();
            }

            var userName = User.Identity?.Name ?? "Admin";
            var result = await _jobArchiveService.ArchiveJobAsync(
                ArchiveJobInput.JobId, 
                userName, 
                ArchiveJobInput.Reason);

            if (result > 0)
            {
                TempData["Success"] = $"Job #{ArchiveJobInput.JobId} archived successfully.";
                _logger.LogInformation("Admin {Admin} archived job {JobId}", userName, ArchiveJobInput.JobId);
            }
            else
            {
                TempData["Error"] = "Failed to archive job. Please check if the job exists and is eligible for archival.";
            }

            // Clear the form
            ArchiveJobInput = new ArchiveJobInputModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving job {JobId}", ArchiveJobInput.JobId);
            TempData["Error"] = "An error occurred while archiving the job.";
        }

        return RedirectToPage(new { ActiveTab = "eligible" });
    }

    public async Task<IActionResult> OnPostBulkArchiveAsync()
    {
        try
        {
            if (!BulkArchiveInput.JobIds.Any())
            {
                TempData["Error"] = "Please select at least one job to archive.";
                return RedirectToPage(new { ActiveTab = "eligible" });
            }

            _logger.LogInformation("Bulk archiving {Count} jobs", BulkArchiveInput.JobIds.Count);

            var userName = User.Identity?.Name ?? "Admin";
            var archivedCount = await _jobArchiveService.BulkArchiveJobsAsync(
                BulkArchiveInput.JobIds, 
                userName, 
                BulkArchiveInput.Reason);

            if (archivedCount > 0)
            {
                TempData["Success"] = $"Successfully archived {archivedCount} of {BulkArchiveInput.JobIds.Count} selected jobs.";
                _logger.LogInformation("Admin {Admin} bulk archived {Count} jobs", userName, archivedCount);
            }
            else
            {
                TempData["Error"] = "No jobs were archived. Please check if the selected jobs are eligible for archival.";
            }

            // Clear the form
            BulkArchiveInput = new BulkArchiveInputModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk archive operation");
            TempData["Error"] = "An error occurred during bulk archival.";
        }

        return RedirectToPage(new { ActiveTab = "eligible" });
    }

    public async Task<IActionResult> OnPostAutoArchiveAsync()
    {
        try
        {
            _logger.LogInformation("Starting auto-archive for jobs older than {Days} days", CleanupInput.OlderThanDays);

            var archivedCount = await _jobArchiveService.AutoArchiveOldJobsAsync(
                CleanupInput.OlderThanDays, 
                "Auto Cleanup");

            if (archivedCount > 0)
            {
                TempData["Success"] = $"Auto-archive completed: {archivedCount} jobs archived.";
                _logger.LogInformation("Admin {Admin} triggered auto-archive - {Count} jobs archived", 
                    User.Identity?.Name, archivedCount);
            }
            else
            {
                TempData["Info"] = "No jobs were eligible for auto-archival.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during auto-archive operation");
            TempData["Error"] = "An error occurred during auto-archival.";
        }

        return RedirectToPage(new { ActiveTab = "cleanup" });
    }

    public async Task<IActionResult> OnPostRestoreJobAsync(int archivedJobId)
    {
        try
        {
            var userName = User.Identity?.Name ?? "Admin";
            var success = await _jobArchiveService.RestoreArchivedJobAsync(archivedJobId, userName);

            if (success)
            {
                TempData["Success"] = "Job restored successfully from archive.";
                _logger.LogInformation("Admin {Admin} restored archived job {ArchivedJobId}", userName, archivedJobId);
            }
            else
            {
                TempData["Error"] = "Failed to restore job. The job may not exist or there may be a conflict.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring archived job {ArchivedJobId}", archivedJobId);
            TempData["Error"] = "An error occurred while restoring the job.";
        }

        return RedirectToPage(new { ActiveTab = "archive" });
    }

    public async Task<IActionResult> OnPostDeleteArchivedJobAsync(int archivedJobId)
    {
        try
        {
            var success = await _jobArchiveService.DeleteArchivedJobAsync(archivedJobId);

            if (success)
            {
                TempData["Success"] = "Archived job deleted successfully.";
                _logger.LogWarning("Admin {Admin} deleted archived job {ArchivedJobId}", 
                    User.Identity?.Name, archivedJobId);
            }
            else
            {
                TempData["Error"] = "Failed to delete archived job.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting archived job {ArchivedJobId}", archivedJobId);
            TempData["Error"] = "An error occurred while deleting the archived job.";
        }

        return RedirectToPage(new { ActiveTab = "archive" });
    }

    public async Task<IActionResult> OnPostCleanupOldArchivesAsync()
    {
        try
        {
            _logger.LogInformation("Starting cleanup of archived jobs older than {Days} days", CleanupInput.ArchiveCleanupDays);

            var deletedCount = await _jobArchiveService.CleanupOldArchivedJobsAsync(CleanupInput.ArchiveCleanupDays);

            if (deletedCount > 0)
            {
                TempData["Success"] = $"Cleanup completed: {deletedCount} old archived jobs deleted.";
                _logger.LogWarning("Admin {Admin} triggered archive cleanup - {Count} archived jobs deleted", 
                    User.Identity?.Name, deletedCount);
            }
            else
            {
                TempData["Info"] = "No archived jobs were old enough for cleanup.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during archive cleanup operation");
            TempData["Error"] = "An error occurred during archive cleanup.";
        }

        return RedirectToPage(new { ActiveTab = "cleanup" });
    }

    // Helper Methods
    private async Task LoadDataForActiveTabAsync()
    {
        switch (ActiveTab.ToLower())
        {
            case "archive":
                await LoadArchivedJobsAsync();
                break;
            case "eligible":
                await LoadEligibleJobsAsync();
                break;
            case "cleanup":
                await LoadCleanupDataAsync();
                break;
            case "statistics":
                await LoadStatisticsAsync();
                break;
            default:
                await LoadArchivedJobsAsync();
                break;
        }

        // Always load these for dropdowns
        ArchiveReasons = await _jobArchiveService.GetArchiveReasonsAsync();
        ArchiveMachines = await _jobArchiveService.GetArchiveMachinesAsync();
    }

    private async Task LoadArchivedJobsAsync()
    {
        if (!string.IsNullOrEmpty(SearchTerm))
        {
            ArchivedJobs = await _jobArchiveService.SearchArchivedJobsAsync(SearchTerm);
        }
        else if (StartDate.HasValue && EndDate.HasValue)
        {
            ArchivedJobs = await _jobArchiveService.GetArchivedJobsByDateRangeAsync(StartDate.Value, EndDate.Value);
        }
        else if (!string.IsNullOrEmpty(MachineFilter))
        {
            ArchivedJobs = await _jobArchiveService.GetArchivedJobsByMachineAsync(MachineFilter);
        }
        else if (!string.IsNullOrEmpty(StatusFilter))
        {
            ArchivedJobs = await _jobArchiveService.GetArchivedJobsByStatusAsync(StatusFilter);
        }
        else
        {
            ArchivedJobs = await _jobArchiveService.GetArchivedJobsAsync();
        }

        // Apply additional filters
        if (!string.IsNullOrEmpty(ReasonFilter))
        {
            ArchivedJobs = ArchivedJobs.Where(aj => aj.ArchiveReason == ReasonFilter).ToList();
        }
    }

    private async Task LoadEligibleJobsAsync()
    {
        EligibleJobs = await _jobArchiveService.GetEligibleJobsForArchivalAsync(EligibleDays);
    }

    private async Task LoadCleanupDataAsync()
    {
        CleanupRecommendations = await _jobArchiveService.GetCleanupRecommendationsAsync();
    }

    private async Task LoadStatisticsAsync()
    {
        ArchiveStatistics = await _jobArchiveService.GetArchiveStatisticsAsync();
    }

    public string GetJobStatusBadgeClass(string status)
    {
        return status.ToLower() switch
        {
            "completed" => "bg-success",
            "cancelled" => "bg-secondary",
            "restored" => "bg-info",
            _ => "bg-warning"
        };
    }

    public string GetArchiveReasonBadgeClass(string reason)
    {
        return reason.ToLower() switch
        {
            "auto cleanup" => "bg-primary",
            "manual archive" => "bg-info",
            "bulk archive" => "bg-warning",
            _ => "bg-secondary"
        };
    }

    public string FormatDuration(TimeSpan? duration)
    {
        if (!duration.HasValue) return "-";
        
        var d = duration.Value;
        if (d.TotalDays >= 1)
            return $"{d.Days}d {d.Hours}h {d.Minutes}m";
        else if (d.TotalHours >= 1)
            return $"{d.Hours}h {d.Minutes}m";
        else
            return $"{d.Minutes}m";
    }
}

/// <summary>
/// Input model for archiving a single job
/// </summary>
public class ArchiveJobInputModel
{
    [Required]
    [Display(Name = "Job ID")]
    public int JobId { get; set; }

    [Required]
    [StringLength(500)]
    [Display(Name = "Archive Reason")]
    public string Reason { get; set; } = "Manual Archive";
}

/// <summary>
/// Input model for bulk archiving jobs
/// </summary>
public class BulkArchiveInputModel
{
    [Display(Name = "Selected Jobs")]
    public List<int> JobIds { get; set; } = new();

    [Required]
    [StringLength(500)]
    [Display(Name = "Archive Reason")]
    public string Reason { get; set; } = "Bulk Archive";
}

/// <summary>
/// Input model for cleanup operations
/// </summary>
public class CleanupInputModel
{
    [Range(1, 365)]
    [Display(Name = "Jobs older than (days)")]
    public int OlderThanDays { get; set; } = 30;

    [Range(30, 3650)]
    [Display(Name = "Archive cleanup older than (days)")]
    public int ArchiveCleanupDays { get; set; } = 365;
}