using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;
using OpCentrix.Services.Admin;
using System.Security.Claims;

namespace OpCentrix.Pages.Operations
{
    /// <summary>
    /// Stage Dashboard with role-based views for manufacturing workflow management
    /// Leverages PrintTracking patterns for proven mobile-responsive design
    /// </summary>
    public class StageDashboardModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<StageDashboardModel> _logger;

        // Role-based view properties (copied from PrintTracking)
        public bool IsAdminView { get; set; }
        public bool IsOperatorView => !IsAdminView;
        public string UserRole { get; set; } = string.Empty;
        public string CurrentOperator { get; set; } = string.Empty;

        // Stage dashboard data
        public List<StageJob> StageJobs { get; set; } = new();
        public List<ProductionStage> ProductionStages { get; set; } = new();
        public string ViewMode { get; set; } = "stage"; // stage, machine, operator
        public DateTime CurrentDate { get; set; } = DateTime.Today;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Statistics (following PrintTracking pattern)
        public Dictionary<int, int> ActiveJobsByStage { get; set; } = new();
        public Dictionary<int, int> QueuedJobsByStage { get; set; } = new();
        public Dictionary<int, double> UtilizationByStage { get; set; } = new();
        public int TotalActiveJobs { get; set; }
        public int TotalCompletedToday { get; set; }
        public double TotalHoursToday { get; set; }

        public StageDashboardModel(
            SchedulerContext context,
            ILogger<StageDashboardModel> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> OnGetAsync(string? view = "stage", DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Determine user role and view type (copied from PrintTracking)
                UserRole = GetCurrentUserRole();
                IsAdminView = UserRole == "Admin" || UserRole == "Manager";
                CurrentOperator = User.Identity?.Name ?? "Unknown";

                // Set view parameters
                ViewMode = view ?? "stage";
                StartDate = startDate ?? DateTime.Today;
                EndDate = endDate ?? DateTime.Today.AddDays(7);

                // Load production stages
                await LoadProductionStagesAsync();

                // Load jobs and stage data
                await LoadStageJobsAsync();

                // Calculate statistics
                await CalculateStageStatisticsAsync();

                _logger.LogInformation("Stage dashboard loaded for user {UserId} ({UserRole}) with {StageCount} stages and {JobCount} jobs. Admin view: {IsAdminView}",
                    userId, UserRole, ProductionStages.Count, StageJobs.Count, IsAdminView);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stage dashboard for user {UserId}", GetCurrentUserId());

                // Return fallback dashboard
                await CreateFallbackDashboard();
                TempData["Error"] = "Error loading stage dashboard data. Please refresh the page.";

                return Page();
            }
        }

        public async Task<IActionResult> OnGetRefreshStagesAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                UserRole = GetCurrentUserRole();
                IsAdminView = UserRole == "Admin" || UserRole == "Manager";
                CurrentOperator = User.Identity?.Name ?? "Unknown";

                await LoadProductionStagesAsync();
                await LoadStageJobsAsync();
                await CalculateStageStatisticsAsync();

                return Partial("_StageContent", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing stage dashboard for user {UserId}", GetCurrentUserId());
                return StatusCode(500, "Error refreshing dashboard");
            }
        }

        public async Task<IActionResult> OnPostPunchInAsync(int jobId, int stageId, string operatorName)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Validate the job and stage exist
                var job = await _context.Jobs.FindAsync(jobId);
                var stage = await _context.ProductionStages.FindAsync(stageId);

                if (job == null || stage == null)
                {
                    return new JsonResult(new { success = false, message = "Job or stage not found" });
                }

                // Check if operator is already working on this stage
                var existingExecution = await _context.ProductionStageExecutions
                    .FirstOrDefaultAsync(pse => pse.JobId == jobId && 
                                              pse.ProductionStageId == stageId && 
                                              pse.Status == "InProgress");

                if (existingExecution != null)
                {
                    return new JsonResult(new { success = false, message = "Already working on this stage" });
                }

                // Create new stage execution with correct property names
                var execution = new ProductionStageExecution
                {
                    JobId = jobId,
                    ProductionStageId = stageId,
                    ExecutedBy = operatorName,
                    OperatorName = operatorName,
                    Status = "InProgress",
                    StartDate = DateTime.UtcNow,
                    ActualStartTime = DateTime.UtcNow,
                    EstimatedHours = stage.DefaultSetupMinutes / 60.0m,
                    CreatedBy = operatorName,
                    CreatedDate = DateTime.UtcNow
                };

                _context.ProductionStageExecutions.Add(execution);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Operator {OperatorName} punched in to stage {StageId} for job {JobId}",
                    operatorName, stageId, jobId);

                return new JsonResult(new { success = true, message = "Punched in successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error punching in to stage {StageId} for job {JobId}", stageId, jobId);
                return new JsonResult(new { success = false, message = "Error punching in" });
            }
        }

        public async Task<IActionResult> OnPostPunchOutAsync(int jobId, int stageId)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Find the active execution
                var execution = await _context.ProductionStageExecutions
                    .FirstOrDefaultAsync(pse => pse.JobId == jobId && 
                                              pse.ProductionStageId == stageId && 
                                              pse.Status == "InProgress");

                if (execution == null)
                {
                    return new JsonResult(new { success = false, message = "No active execution found" });
                }

                // Complete the execution with correct property names
                execution.Status = "Completed";
                execution.CompletionDate = DateTime.UtcNow;
                execution.ActualEndTime = DateTime.UtcNow;
                execution.LastModifiedBy = User.Identity?.Name ?? "Unknown";
                execution.LastModifiedDate = DateTime.UtcNow;

                // Calculate actual hours
                if (execution.StartDate.HasValue)
                {
                    execution.ActualHours = (decimal)(DateTime.UtcNow - execution.StartDate.Value).TotalHours;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Completed stage execution {ExecutionId} for job {JobId} stage {StageId}",
                    execution.Id, jobId, stageId);

                return new JsonResult(new { success = true, message = "Punched out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error punching out of stage {StageId} for job {JobId}", stageId, jobId);
                return new JsonResult(new { success = false, message = "Error punching out" });
            }
        }

        #region Helper Methods

        /// <summary>
        /// Get count of active jobs for a stage
        /// </summary>
        public int GetActiveJobsCount(int stageId)
        {
            return ActiveJobsByStage.GetValueOrDefault(stageId, 0);
        }

        /// <summary>
        /// Get count of queued jobs for a stage
        /// </summary>
        public int GetQueuedJobsCount(int stageId)
        {
            return QueuedJobsByStage.GetValueOrDefault(stageId, 0);
        }

        /// <summary>
        /// Get current operator name
        /// </summary>
        public string GetCurrentOperator()
        {
            return User.Identity?.Name ?? "Unknown";
        }

        /// <summary>
        /// Check if user is in admin role
        /// </summary>
        public bool IsAdminUser()
        {
            return User.IsInRole("Admin") || User.IsInRole("Manager");
        }

        /// <summary>
        /// Get CSS class for job status badge
        /// </summary>
        public string GetJobStatusClass(string status)
        {
            return status?.ToLower() switch
            {
                "scheduled" => "badge-secondary",
                "inprogress" => "badge-primary", 
                "in progress" => "badge-primary",
                "completed" => "badge-success",
                "cancelled" => "badge-danger",
                "on hold" => "badge-warning",
                _ => "badge-secondary"
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Load production stages from database
        /// </summary>
        private async Task LoadProductionStagesAsync()
        {
            try
            {
                ProductionStages = await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .OrderBy(ps => ps.DisplayOrder)
                    .ToListAsync();

                if (!ProductionStages.Any())
                {
                    _logger.LogWarning("No active production stages found. Creating fallback stages.");
                    ProductionStages = CreateFallbackStages();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading production stages");
                ProductionStages = CreateFallbackStages();
            }
        }

        /// <summary>
        /// Load jobs with stage information
        /// </summary>
        private async Task LoadStageJobsAsync()
        {
            try
            {
                var jobs = await _context.Jobs
                    .Where(j => j.ScheduledStart >= StartDate && j.ScheduledStart < EndDate)
                    .Include(j => j.Part)
                    .ThenInclude(p => p.PartStageRequirements)
                    .ThenInclude(psr => psr.ProductionStage)
                    .OrderBy(j => j.ScheduledStart)
                    .ToListAsync();

                StageJobs = jobs.Select(job => new StageJob
                {
                    BaseJob = job,
                    RequiredStages = job.Part?.PartStageRequirements?
                        .Where(psr => psr.IsRequired && psr.ProductionStage.IsActive)
                        .Select(psr => psr.ProductionStage)
                        .OrderBy(ps => ps.DisplayOrder)
                        .ToList() ?? new List<ProductionStage>(),
                    StageExecutions = new List<ProductionStageExecution>()
                }).ToList();

                // Load stage executions for each job
                await LoadStageExecutionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stage jobs");
                StageJobs = new List<StageJob>();
            }
        }

        /// <summary>
        /// Load stage executions for jobs
        /// </summary>
        private async Task LoadStageExecutionsAsync()
        {
            try
            {
                var jobIds = StageJobs.Select(sj => sj.BaseJob.Id).ToList();

                var executions = await _context.ProductionStageExecutions
                    .Where(pse => jobIds.Contains(pse.JobId.GetValueOrDefault()))
                    .ToListAsync();

                foreach (var stageJob in StageJobs)
                {
                    stageJob.StageExecutions = executions
                        .Where(e => e.JobId == stageJob.BaseJob.Id)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stage executions");
            }
        }

        /// <summary>
        /// Calculate statistics for stages
        /// </summary>
        private async Task CalculateStageStatisticsAsync()
        {
            try
            {
                var today = DateTime.Today;

                // Active jobs by stage
                foreach (var stage in ProductionStages)
                {
                    var activeCount = await _context.ProductionStageExecutions
                        .CountAsync(pse => pse.ProductionStageId == stage.Id && pse.Status == "InProgress");
                    ActiveJobsByStage[stage.Id] = activeCount;

                    var queuedCount = StageJobs.Count(sj => sj.RequiredStages.Any(rs => 
                        rs.Id == stage.Id && sj.GetStageStatus(rs) == "Pending"));
                    QueuedJobsByStage[stage.Id] = queuedCount;
                }

                // Total statistics
                TotalActiveJobs = await _context.ProductionStageExecutions
                    .CountAsync(pse => pse.Status == "InProgress");

                TotalCompletedToday = await _context.ProductionStageExecutions
                    .CountAsync(pse => pse.Status == "Completed" && 
                                      pse.CompletionDate.HasValue && 
                                      pse.CompletionDate.Value.Date == today);

                var hoursToday = await _context.ProductionStageExecutions
                    .Where(pse => pse.Status == "Completed" && 
                                 pse.CompletionDate.HasValue && 
                                 pse.CompletionDate.Value.Date == today &&
                                 pse.ActualHours.HasValue)
                    .SumAsync(pse => (double)pse.ActualHours.Value);

                TotalHoursToday = hoursToday;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating stage statistics");
            }
        }

        /// <summary>
        /// Create fallback dashboard when errors occur
        /// </summary>
        private async Task CreateFallbackDashboard()
        {
            CurrentOperator = User.Identity?.Name ?? "Unknown";
            UserRole = GetCurrentUserRole();
            IsAdminView = UserRole == "Admin" || UserRole == "Manager";
            ProductionStages = CreateFallbackStages();
            StageJobs = new List<StageJob>();
            ActiveJobsByStage = new Dictionary<int, int>();
            QueuedJobsByStage = new Dictionary<int, int>();
        }

        /// <summary>
        /// Create fallback production stages
        /// </summary>
        private List<ProductionStage> CreateFallbackStages()
        {
            return new List<ProductionStage>
            {
                new ProductionStage { Id = 1, Name = "3D Printing (SLS)", StageColor = "#007bff", Department = "3D Printing", DisplayOrder = 1, IsActive = true },
                new ProductionStage { Id = 2, Name = "CNC Machining", StageColor = "#28a745", Department = "CNC Machining", DisplayOrder = 2, IsActive = true },
                new ProductionStage { Id = 3, Name = "EDM", StageColor = "#ffc107", Department = "EDM", DisplayOrder = 3, IsActive = true },
                new ProductionStage { Id = 4, Name = "Laser Engraving", StageColor = "#fd7e14", Department = "Laser Operations", DisplayOrder = 4, IsActive = true },
                new ProductionStage { Id = 5, Name = "Sandblasting", StageColor = "#6c757d", Department = "Finishing", DisplayOrder = 5, IsActive = true },
                new ProductionStage { Id = 6, Name = "Coating/Cerakote", StageColor = "#17a2b8", Department = "Finishing", DisplayOrder = 6, IsActive = true },
                new ProductionStage { Id = 7, Name = "Assembly", StageColor = "#dc3545", Department = "Assembly", DisplayOrder = 7, IsActive = true }
            };
        }

        /// <summary>
        /// Get current user ID safely (copied from PrintTracking)
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            _logger.LogWarning("Unable to get user ID from claims for user {UserName}", User.Identity?.Name ?? "Unknown");
            return 1; // Default admin user ID
        }

        /// <summary>
        /// Get current user role safely (copied from PrintTracking)
        /// </summary>
        private string GetCurrentUserRole()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ??
                          User.FindFirst("Role")?.Value ?? "Operator";
            return userRole;
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Stage job view model
    /// </summary>
    public class StageJob
    {
        public Job BaseJob { get; set; } = new();
        public List<ProductionStage> RequiredStages { get; set; } = new();
        public List<ProductionStageExecution> StageExecutions { get; set; } = new();

        public string GetStageStatus(ProductionStage stage)
        {
            var execution = StageExecutions.FirstOrDefault(se => se.ProductionStageId == stage.Id);
            return execution?.Status ?? "Pending";
        }

        public double GetStageProgress(ProductionStage stage)
        {
            var execution = StageExecutions.FirstOrDefault(se => se.ProductionStageId == stage.Id);
            return execution?.Status switch
            {
                "Completed" => 100.0,
                "InProgress" => 50.0,
                _ => 0.0
            };
        }

        public bool CanOperatorPunch(ProductionStage stage)
        {
            var status = GetStageStatus(stage);
            return status == "Pending";
        }
    }

    #endregion
}