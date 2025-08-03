using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Authorization;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;

namespace OpCentrix.Pages.EDM
{
    [EDMAccess]
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
        public int SetupOperations { get; set; }
        public int RunningOperations { get; set; }
        public int CompletedOperations { get; set; }
        public int OnHoldOperations { get; set; }
        public double AverageEfficiency { get; set; }
        public double TotalEDMHours { get; set; }

        // OPTION A: Stage-aware properties
        public List<Job> EDMStageJobs { get; set; } = new();
        public List<Job> PreEDMJobs { get; set; } = new();
        public List<BuildCohort> ActiveCohorts { get; set; } = new();
        public Dictionary<int, int> CohortProgress { get; set; } = new();

        // Data collections - Using object lists until database tables are implemented
        public List<object> Operations { get; set; } = new();
        public List<object> ActiveOperations { get; set; } = new();
        public List<Part> AvailableParts { get; set; } = new();

        // Filtering and pagination
        public string SearchTerm { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public string EDMTypeFilter { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public async Task OnGetAsync(string? search, string? status, string? edmType, int? page)
        {
            try
            {
                SearchTerm = search ?? string.Empty;
                StatusFilter = status ?? string.Empty;
                EDMTypeFilter = edmType ?? string.Empty;
                PageNumber = page ?? 1;

                await LoadDashboardMetricsAsync();
                await LoadOperationsAsync();
                await LoadActiveOperationsAsync();
                await LoadAvailablePartsAsync();
                
                // OPTION A: Load stage-aware data
                await LoadStageAwareDataAsync();

                _logger.LogInformation("EDM operations page loaded successfully with {EDMJobs} EDM stage jobs", EDMStageJobs.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading EDM operations page");
                await InitializeWithDefaults();
            }
        }

        // OPTION A: Stage completion for manufacturing workflow
        public async Task<IActionResult> OnPostCompleteJobAsync(int jobId)
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

                // Validate job is in EDM stage
                if (job.WorkflowStage != "EDM" && job.WorkflowStage != "CNC")
                {
                    TempData["Error"] = "Job is not in EDM stage";
                    return RedirectToPage();
                }

                // Mark EDM stage complete and advance to Coating
                job.WorkflowStage = "Coating";
                job.StageOrder = (job.StageOrder ?? 1) + 1;
                job.Status = "Ready for Coating";
                job.LastModifiedDate = DateTime.UtcNow;
                job.LastModifiedBy = User.Identity?.Name ?? "EDM Operator";

                // Update cohort if needed
                if (job.BuildCohortId.HasValue && _cohortService != null)
                {
                    await _cohortService.UpdateCohortStageProgressAsync(job.BuildCohortId.Value, "EDM", "Completed");
                }

                // Create stage history entry
                var stageHistory = new JobStageHistory
                {
                    JobId = jobId,
                    Action = "StageCompleted",
                    Operator = User.Identity?.Name ?? "EDM Operator",
                    Timestamp = DateTime.UtcNow,
                    Notes = "EDM operations completed successfully"
                };
                _context.JobStageHistories.Add(stageHistory);

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Job {job.PartNumber} completed EDM stage and advanced to Coating";
                _logger.LogInformation("Job {JobId} ({PartNumber}) completed EDM stage and advanced to Coating", jobId, job.PartNumber);

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing EDM job {JobId}", jobId);
                TempData["Error"] = "Error completing EDM job";
                return RedirectToPage();
            }
        }

        // OPTION A: Start EDM operations on a job
        public async Task<IActionResult> OnPostStartEDMAsync(int jobId)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(jobId);
                if (job == null)
                {
                    TempData["Error"] = "Job not found";
                    return RedirectToPage();
                }

                // Update job status to EDM in progress
                job.Status = "EDM In Progress";
                job.WorkflowStage = "EDM";
                job.LastModifiedDate = DateTime.UtcNow;
                job.LastModifiedBy = User.Identity?.Name ?? "EDM Operator";

                // Create stage history entry
                var stageHistory = new JobStageHistory
                {
                    JobId = jobId,
                    Action = "StageStarted",
                    Operator = User.Identity?.Name ?? "EDM Operator",
                    Timestamp = DateTime.UtcNow,
                    Notes = "EDM operations started"
                };
                _context.JobStageHistories.Add(stageHistory);

                await _context.SaveChangesAsync();

                TempData["Success"] = $"EDM operations started for job {job.PartNumber}";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting EDM for job {JobId}", jobId);
                TempData["Error"] = "Error starting EDM operations";
                return RedirectToPage();
            }
        }

        // OPTION A: Load stage-aware data
        private async Task LoadStageAwareDataAsync()
        {
            try
            {
                // Load jobs currently in EDM stage or ready for EDM
                EDMStageJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.WorkflowStage == "EDM" || 
                               (j.WorkflowStage == "CNC" && j.Status == "Completed"))
                    .OrderBy(j => j.ScheduledStart)
                    .ToListAsync();

                // Load jobs that might need EDM (from SLS stage completed)
                PreEDMJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.WorkflowStage == "SLS" && j.Status == "Completed")
                    .OrderBy(j => j.ScheduledStart)
                    .Take(10)
                    .ToListAsync();

                // Load active cohorts with EDM work
                if (_cohortService != null)
                {
                    ActiveCohorts = await _context.BuildCohorts
                        .Where(c => c.Status == "InProgress")
                        .ToListAsync();

                    // Calculate progress for each cohort
                    foreach (var cohort in ActiveCohorts)
                    {
                        var edmJobs = await _context.Jobs
                            .Where(j => j.BuildCohortId == cohort.Id && j.WorkflowStage == "EDM")
                            .CountAsync();
                        var completedEdmJobs = await _context.Jobs
                            .Where(j => j.BuildCohortId == cohort.Id && 
                                       j.WorkflowStage == "Coating" || 
                                       (j.WorkflowStage == "EDM" && j.Status == "Completed"))
                            .CountAsync();

                        CohortProgress[cohort.Id] = edmJobs > 0 ? (completedEdmJobs * 100 / edmJobs) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stage-aware data for EDM");
                // Initialize empty collections on error
                EDMStageJobs = new List<Job>();
                PreEDMJobs = new List<Job>();
                ActiveCohorts = new List<BuildCohort>();
                CohortProgress = new Dictionary<int, int>();
            }
        }

        public async Task<IActionResult> OnPostCreateOperationAsync()
        {
            // TODO: Implement when EDMOperation table is available
            TempData["Info"] = "EDM operations feature is under development";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
        {
            // TODO: Implement when EDMOperation table is available
            TempData["Info"] = "EDM operations feature is under development";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateProcessParametersAsync(int id, double current, double voltage, 
            double pulseOnTime, double pulseOffTime, double flowRate)
        {
            // TODO: Implement when EDMOperation table is available
            TempData["Info"] = "EDM operations feature is under development";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteOperationAsync(int id)
        {
            // TODO: Implement when EDMOperation table is available
            TempData["Info"] = "EDM operations feature is under development";
            return RedirectToPage();
        }

        private async Task LoadDashboardMetricsAsync()
        {
            // Enhanced metrics with stage awareness
            try
            {
                var edmJobs = await _context.Jobs
                    .Where(j => j.WorkflowStage == "EDM")
                    .ToListAsync();

                TotalOperations = edmJobs.Count;
                SetupOperations = edmJobs.Count(j => j.Status == "Setup");
                RunningOperations = edmJobs.Count(j => j.Status == "EDM In Progress");
                CompletedOperations = edmJobs.Count(j => j.Status == "Completed");
                OnHoldOperations = edmJobs.Count(j => j.Status == "On Hold");
                
                if (edmJobs.Any())
                {
                    TotalEDMHours = edmJobs.Sum(j => j.EstimatedHours);
                    AverageEfficiency = edmJobs.Where(j => j.ActualEnd.HasValue)
                                              .Average(j => j.EstimatedHours / 
                                                          (j.ActualEnd.Value - j.ActualStart.GetValueOrDefault()).TotalHours) * 100;
                }
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

        private async Task LoadActiveOperationsAsync()
        {
            // Placeholder implementation without database
            ActiveOperations = new List<object>();
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
            SetupOperations = 0;
            RunningOperations = 0;
            CompletedOperations = 0;
            OnHoldOperations = 0;
            AverageEfficiency = 0;
            TotalEDMHours = 0;
            Operations = new List<object>();
            ActiveOperations = new List<object>();
            AvailableParts = new List<Part>();
            TotalCount = 0;
            EDMStageJobs = new List<Job>();
            PreEDMJobs = new List<Job>();
            ActiveCohorts = new List<BuildCohort>();
            CohortProgress = new Dictionary<int, int>();
        }

        // Helper methods for the view
        public string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "Completed" => "bg-green-100 text-green-800",
                "EDM In Progress" => "bg-blue-100 text-blue-800",
                "Setup" => "bg-yellow-100 text-yellow-800",
                "On Hold" => "bg-red-100 text-red-800",
                "Ready for Coating" => "bg-purple-100 text-purple-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }

        public string GetEDMTypeIcon(string edmType)
        {
            return edmType switch
            {
                "Wire EDM" => "?",
                "Sinker EDM" => "??",
                "Small Hole EDM" => "???",
                _ => "??"
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
}