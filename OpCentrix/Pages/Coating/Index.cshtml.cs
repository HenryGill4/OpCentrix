using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Authorization;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.JobStaging;
using OpCentrix.Services;

namespace OpCentrix.Pages.Coating
{
    [CoatingAccess]
    public class IndexModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<IndexModel> _logger;
        private readonly ICohortManagementService? _cohortService;

        public IndexModel(SchedulerContext context, ILogger<IndexModel> logger, ICohortManagementService? cohortService = null)
        {
            _context = context;
            _logger = logger;
            _cohortService = cohortService;
        }

        // Dashboard metrics
        public int TotalOperations { get; set; }
        public int PendingOperations { get; set; }
        public int InProgressOperations { get; set; }
        public int CompletedOperations { get; set; }
        public int RejectedOperations { get; set; }
        public double OverallQualityRate { get; set; }

        // OPTION A: Stage-aware properties
        public List<Job> CoatingStageJobs { get; set; } = new();
        public List<Job> ReadyForCoatingJobs { get; set; } = new();
        public List<BuildCohort> ActiveCohorts { get; set; } = new();
        public Dictionary<int, CoatingStageInfo> CohortCoatingProgress { get; set; } = new();

        // Data collections - Using object lists until database tables are implemented
        public List<object> Operations { get; set; } = new();
        public List<object> TodaysOperations { get; set; } = new();
        public List<Part> AvailableParts { get; set; } = new();

        // Filtering and pagination
        public string SearchTerm { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public string CoatingTypeFilter { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public async Task OnGetAsync(string? search, string? status, string? coatingType, int? page)
        {
            try
            {
                SearchTerm = search ?? string.Empty;
                StatusFilter = status ?? string.Empty;
                CoatingTypeFilter = coatingType ?? string.Empty;
                PageNumber = page ?? 1;

                await LoadDashboardMetricsAsync();
                await LoadOperationsAsync();
                await LoadTodaysOperationsAsync();
                await LoadAvailablePartsAsync();
                
                // OPTION A: Load stage-aware data
                await LoadStageAwareDataAsync();

                _logger.LogInformation("Coating operations page loaded successfully with {CoatingJobs} coating stage jobs", CoatingStageJobs.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading coating operations page");
                await InitializeWithDefaults();
            }
        }

        // OPTION A: Start coating operations on a job
        public async Task<IActionResult> OnPostStartCoatingAsync(int jobId)
        {
            try
            {
                var job = await _context.Jobs
                    .Include(j => j.Part)
                    .FirstOrDefaultAsync(j => j.Id == jobId);

                if (job == null)
                {
                    TempData["Error"] = "Job not found";
                    return RedirectToPage();
                }

                // Validate job is ready for coating (EDM completed or in Coating stage)
                if (job.WorkflowStage != "Coating" && !(job.WorkflowStage == "EDM" && job.Status == "Ready for Coating"))
                {
                    TempData["Error"] = "Job is not ready for coating. EDM stage must be completed first.";
                    return RedirectToPage();
                }

                // Update job status to coating in progress
                job.Status = "Coating In Progress";
                job.WorkflowStage = "Coating";
                job.LastModifiedDate = DateTime.UtcNow;
                job.LastModifiedBy = User.Identity?.Name ?? "Coating Operator";

                // Create stage history entry
                var stageHistory = new JobStageHistory
                {
                    JobId = jobId,
                    Action = "StageStarted",
                    Operator = User.Identity?.Name ?? "Coating Operator",
                    Timestamp = DateTime.UtcNow,
                    Notes = "Coating operations started"
                };
                _context.JobStageHistories.Add(stageHistory);

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Coating operations started for job {job.PartNumber}";
                _logger.LogInformation("Coating started for job {JobId} ({PartNumber})", jobId, job.PartNumber);
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting coating for job {JobId}", jobId);
                TempData["Error"] = "Error starting coating operations";
                return RedirectToPage();
            }
        }

        // OPTION A: Complete coating operations and advance to QC
        public async Task<IActionResult> OnPostCompleteCoatingAsync(int jobId)
        {
            try
            {
                var job = await _context.Jobs
                    .Include(j => j.Part)
                    .FirstOrDefaultAsync(j => j.Id == jobId);

                if (job == null)
                {
                    TempData["Error"] = "Job not found";
                    return RedirectToPage();
                }

                // Validate job is in coating stage
                if (job.WorkflowStage != "Coating")
                {
                    TempData["Error"] = "Job is not in coating stage";
                    return RedirectToPage();
                }

                // Mark coating stage complete and advance to QC
                job.WorkflowStage = "QC";
                job.StageOrder = (job.StageOrder ?? 2) + 1;
                job.Status = "Ready for QC";
                job.LastModifiedDate = DateTime.UtcNow;
                job.LastModifiedBy = User.Identity?.Name ?? "Coating Operator";

                // Update cohort if needed
                if (job.BuildCohortId.HasValue && _cohortService != null)
                {
                    await _cohortService.UpdateCohortStageProgressAsync(job.BuildCohortId.Value, "Coating", "Completed");
                }

                // Create stage history entry
                var stageHistory = new JobStageHistory
                {
                    JobId = jobId,
                    Action = "StageCompleted",
                    Operator = User.Identity?.Name ?? "Coating Operator",
                    Timestamp = DateTime.UtcNow,
                    Notes = "Coating operations completed successfully"
                };
                _context.JobStageHistories.Add(stageHistory);

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Job {job.PartNumber} completed coating and advanced to QC";
                _logger.LogInformation("Job {JobId} ({PartNumber}) completed coating stage and advanced to QC", jobId, job.PartNumber);
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing coating for job {JobId}", jobId);
                TempData["Error"] = "Error completing coating operations";
                return RedirectToPage();
            }
        }

        // OPTION A: Hold coating operations
        public async Task<IActionResult> OnPostHoldCoatingAsync(int jobId, string reason)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(jobId);
                if (job == null)
                {
                    TempData["Error"] = "Job not found";
                    return RedirectToPage();
                }

                job.Status = "Coating On Hold";
                job.LastModifiedDate = DateTime.UtcNow;
                job.LastModifiedBy = User.Identity?.Name ?? "Coating Operator";

                // Create stage history entry
                var stageHistory = new JobStageHistory
                {
                    JobId = jobId,
                    Action = "StageHeld",
                    Operator = User.Identity?.Name ?? "Coating Operator",
                    Timestamp = DateTime.UtcNow,
                    Notes = $"Coating operations placed on hold: {reason}"
                };
                _context.JobStageHistories.Add(stageHistory);

                await _context.SaveChangesAsync();

                TempData["Warning"] = $"Job {job.PartNumber} placed on hold: {reason}";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error holding coating for job {JobId}", jobId);
                TempData["Error"] = "Error placing coating operations on hold";
                return RedirectToPage();
            }
        }

        // OPTION A: Load stage-aware data
        private async Task LoadStageAwareDataAsync()
        {
            try
            {
                // Load jobs currently in coating stage
                CoatingStageJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.WorkflowStage == "Coating")
                    .OrderBy(j => j.ScheduledStart)
                    .ToListAsync();

                // Load jobs ready for coating (EDM completed)
                ReadyForCoatingJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.WorkflowStage == "EDM" && j.Status == "Ready for Coating")
                    .OrderBy(j => j.LastModifiedDate)
                    .Take(10)
                    .ToListAsync();

                // Load active cohorts with coating work
                if (_cohortService != null)
                {
                    ActiveCohorts = await _context.BuildCohorts
                        .Where(c => c.Status == "InProgress")
                        .ToListAsync();

                    // Calculate coating progress for each cohort
                    foreach (var cohort in ActiveCohorts)
                    {
                        var coatingInfo = await CalculateCoatingProgressAsync(cohort.Id);
                        CohortCoatingProgress[cohort.Id] = coatingInfo;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stage-aware data for coating");
                // Initialize empty collections on error
                CoatingStageJobs = new List<Job>();
                ReadyForCoatingJobs = new List<Job>();
                ActiveCohorts = new List<BuildCohort>();
                CohortCoatingProgress = new Dictionary<int, CoatingStageInfo>();
            }
        }

        private async Task<CoatingStageInfo> CalculateCoatingProgressAsync(int cohortId)
        {
            var totalJobs = await _context.Jobs
                .Where(j => j.BuildCohortId == cohortId)
                .CountAsync();

            var jobsNeedingCoating = await _context.Jobs
                .Where(j => j.BuildCohortId == cohortId && 
                           (j.WorkflowStage == "Coating" || j.Status == "Ready for Coating"))
                .CountAsync();

            var coatingInProgress = await _context.Jobs
                .Where(j => j.BuildCohortId == cohortId && j.Status == "Coating In Progress")
                .CountAsync();

            var coatingCompleted = await _context.Jobs
                .Where(j => j.BuildCohortId == cohortId && 
                           (j.WorkflowStage == "QC" || j.Status == "Ready for QC"))
                .CountAsync();

            return new CoatingStageInfo
            {
                TotalJobs = totalJobs,
                JobsNeedingCoating = jobsNeedingCoating,
                CoatingInProgress = coatingInProgress,
                CoatingCompleted = coatingCompleted,
                ProgressPercentage = totalJobs > 0 ? (coatingCompleted * 100 / totalJobs) : 0
            };
        }

        public async Task<IActionResult> OnPostCreateOperationAsync()
        {
            // TODO: Implement when CoatingOperation table is available
            TempData["Info"] = "Coating operations feature is under development";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
        {
            // TODO: Implement when CoatingOperation table is available
            TempData["Info"] = "Coating operations feature is under development";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteOperationAsync(int id)
        {
            // TODO: Implement when CoatingOperation table is available
            TempData["Info"] = "Coating operations feature is under development";
            return RedirectToPage();
        }

        private async Task LoadDashboardMetricsAsync()
        {
            // Enhanced metrics with stage awareness
            try
            {
                var coatingJobs = await _context.Jobs
                    .Where(j => j.WorkflowStage == "Coating")
                    .ToListAsync();

                TotalOperations = coatingJobs.Count;
                PendingOperations = coatingJobs.Count(j => j.Status == "Ready for Coating");
                InProgressOperations = coatingJobs.Count(j => j.Status == "Coating In Progress");
                CompletedOperations = coatingJobs.Count(j => j.Status == "Ready for QC");
                RejectedOperations = coatingJobs.Count(j => j.Status == "Coating On Hold");
                
                // Calculate quality rate based on completed vs rejected
                var totalProcessed = CompletedOperations + RejectedOperations;
                OverallQualityRate = totalProcessed > 0 ? (CompletedOperations * 100.0 / totalProcessed) : 100.0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard metrics");
                await InitializeWithDefaults();
            }
        }

        private async Task LoadOperationsAsync()
        {
            // Placeholder implementation without database
            Operations = new List<object>();
            TotalCount = 0;
        }

        private async Task LoadTodaysOperationsAsync()
        {
            // Placeholder implementation without database
            TodaysOperations = new List<object>();
        }

        private async Task LoadAvailablePartsAsync()
        {
            // Load from actual Parts table (this still exists)
            AvailableParts = await _context.Parts
                .Where(p => p.IsActive)
                .OrderBy(p => p.PartNumber)
                .ToListAsync();
        }

        private async Task InitializeWithDefaults()
        {
            TotalOperations = 0;
            PendingOperations = 0;
            InProgressOperations = 0;
            CompletedOperations = 0;
            RejectedOperations = 0;
            OverallQualityRate = 100;
            Operations = new List<object>();
            TodaysOperations = new List<object>();
            AvailableParts = new List<Part>();
            TotalCount = 0;
            CoatingStageJobs = new List<Job>();
            ReadyForCoatingJobs = new List<Job>();
            ActiveCohorts = new List<BuildCohort>();
            CohortCoatingProgress = new Dictionary<int, CoatingStageInfo>();
        }

        // Helper methods for the view
        public string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "Ready for QC" => "bg-green-100 text-green-800",
                "Coating In Progress" => "bg-yellow-100 text-yellow-800",
                "Coating On Hold" => "bg-red-100 text-red-800",
                "Ready for Coating" => "bg-blue-100 text-blue-800",
                "Completed" => "bg-green-100 text-green-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }

        public string GetPriorityColor(string status)
        {
            return status switch
            {
                "Rush" => "text-red-600",
                "Expedited" => "text-orange-600",
                "Standard" => "text-green-600",
                _ => "text-gray-600"
            };
        }

        public string GetStageProgressColor(int progress)
        {
            return progress switch
            {
                >= 80 => "bg-green-500",
                >= 60 => "bg-yellow-500",
                >= 40 => "bg-orange-500",
                _ => "bg-red-500"
            };
        }
    }

    // Helper class for coating stage information
    public class CoatingStageInfo
    {
        public int TotalJobs { get; set; }
        public int JobsNeedingCoating { get; set; }
        public int CoatingInProgress { get; set; }
        public int CoatingCompleted { get; set; }
        public int ProgressPercentage { get; set; }
    }
}