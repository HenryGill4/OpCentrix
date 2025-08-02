using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services.Admin;
using OpCentrix.ViewModels.Stages;

namespace OpCentrix.Pages.Stages
{
    /// <summary>
    /// SLS Printing Stage Dashboard - Shows jobs and schedule specific to SLS operations
    /// Operators see only assigned jobs, admins see full scope
    /// </summary>
    [Authorize]
    public class SLSDashboardModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly IMultiStageJobService _multiStageJobService;
        private readonly IStagePermissionService _stagePermissionService;
        private readonly ILogger<SLSDashboardModel> _logger;

        public SLSDashboardModel(
            SchedulerContext context,
            IMultiStageJobService multiStageJobService,
            IStagePermissionService stagePermissionService,
            ILogger<SLSDashboardModel> logger)
        {
            _context = context;
            _multiStageJobService = multiStageJobService;
            _stagePermissionService = stagePermissionService;
            _logger = logger;
        }

        // Dashboard Properties
        public StageDashboardViewModel Dashboard { get; set; } = new();
        public bool IsAdmin { get; set; }
        public bool CanSchedule { get; set; }
        public bool CanApprove { get; set; }
        public string UserName { get; set; } = "";
        public string StageName { get; set; } = "SLS Printing";
        public int StageId { get; set; } = 1; // SLS Stage ID
        
        // Schedule Properties
        public List<StageJobInfo> ScheduledJobs { get; set; } = new();
        public List<StageJobInfo> ActiveJobs { get; set; } = new();
        public List<StageJobInfo> PendingApproval { get; set; } = new();
        public List<StageJobInfo> MyAssignedJobs { get; set; } = new();
        
        // Capacity Properties
        public List<MachineCapacityInfo> MachineCapacity { get; set; } = new();
        public int TotalCapacity { get; set; }
        public int UsedCapacity { get; set; }
        public double UtilizationPercent { get; set; }

        // Filter Properties
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public async Task OnGetAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            UserName = User.Identity?.Name ?? "unknown";
            
            _logger.LogInformation("Loading SLS Dashboard for user {User} [Operation: {OperationId}]", 
                UserName, operationId);

            try
            {
                // Set default date range
                StartDate ??= DateTime.Today;
                EndDate ??= DateTime.Today.AddDays(7);

                // Check user permissions
                await LoadUserPermissionsAsync();

                // Load dashboard data based on user role
                if (IsAdmin)
                {
                    await LoadAdminDashboardAsync(operationId);
                }
                else
                {
                    await LoadOperatorDashboardAsync(operationId);
                }

                // Load machine capacity
                await LoadMachineCapacityAsync();

                _logger.LogInformation("SLS Dashboard loaded - Jobs: {JobCount}, Admin: {IsAdmin} [Operation: {OperationId}]", 
                    ScheduledJobs.Count, IsAdmin, operationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading SLS Dashboard [Operation: {OperationId}]", operationId);
                throw;
            }
        }

        private async Task LoadUserPermissionsAsync()
        {
            UserName = User.Identity?.Name ?? "unknown";
            
            // Check if user is admin
            IsAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
            
            // Check specific stage permissions
            CanSchedule = IsAdmin || await _stagePermissionService.CanUserScheduleStageAsync(UserName, StageId);
            CanApprove = IsAdmin || await _stagePermissionService.CanUserApproveStageAsync(UserName, StageId);
        }

        private async Task LoadAdminDashboardAsync(string operationId)
        {
            // Admins see ALL SLS jobs across all operators
            var allSLSJobs = await _context.JobStages
                .Include(js => js.Job)
                    .ThenInclude(j => j.Part)
                .Where(js => js.StageType == "SLS Printing" && 
                           js.ScheduledStart >= StartDate && 
                           js.ScheduledStart <= EndDate)
                .OrderBy(js => js.ScheduledStart)
                .ToListAsync();

            ScheduledJobs = MapToStageJobInfo(allSLSJobs);
            
            // Get active jobs (in progress)
            ActiveJobs = ScheduledJobs.Where(j => j.Status == "InProgress").ToList();
            
            // Get jobs pending approval
            PendingApproval = await GetPendingApprovalJobsAsync();

            _logger.LogDebug("Admin dashboard loaded - {TotalJobs} jobs, {ActiveJobs} active [Operation: {OperationId}]", 
                ScheduledJobs.Count, ActiveJobs.Count, operationId);
        }

        private async Task LoadOperatorDashboardAsync(string operationId)
        {
            // Operators see ONLY jobs assigned to them
            var myJobs = await _context.JobStages
                .Include(js => js.Job)
                    .ThenInclude(j => j.Part)
                .Where(js => js.StageType == "SLS Printing" && 
                           js.AssignedOperator == UserName &&
                           js.ScheduledStart >= StartDate && 
                           js.ScheduledStart <= EndDate)
                .OrderBy(js => js.ScheduledStart)
                .ToListAsync();

            MyAssignedJobs = MapToStageJobInfo(myJobs);
            ScheduledJobs = MyAssignedJobs; // Operators only see their jobs
            
            // Get active jobs for this operator
            ActiveJobs = MyAssignedJobs.Where(j => j.Status == "InProgress").ToList();

            _logger.LogDebug("Operator dashboard loaded - {AssignedJobs} assigned jobs [Operation: {OperationId}]", 
                MyAssignedJobs.Count, operationId);
        }

        private async Task<List<StageJobInfo>> GetPendingApprovalJobsAsync()
        {
            // Get jobs that require approval before starting next stage
            var pendingJobs = await _context.JobStages
                .Include(js => js.Job)
                    .ThenInclude(j => j.Part)
                .Where(js => js.StageType == "SLS Printing" && 
                           js.Status == "Completed" &&
                           // Check if next stage requires approval
                           _context.JobStages.Any(nextStage => 
                               nextStage.JobId == js.JobId && 
                               nextStage.ExecutionOrder == js.ExecutionOrder + 1 &&
                               nextStage.Status == "PendingApproval"))
                .ToListAsync();

            return MapToStageJobInfo(pendingJobs);
        }

        private async Task LoadMachineCapacityAsync()
        {
            // Get SLS machines and their current capacity
            var slsMachines = await _context.Machines
                .Where(m => m.MachineType == "SLS" && m.IsActive)
                .ToListAsync();

            MachineCapacity = new List<MachineCapacityInfo>();
            TotalCapacity = 0;
            UsedCapacity = 0;

            foreach (var machine in slsMachines)
            {
                var activeJobs = await _context.JobStages
                    .Where(js => js.MachineId == machine.MachineId && 
                               js.Status == "InProgress" &&
                               js.StageType == "SLS Printing")
                    .CountAsync();

                var capacity = new MachineCapacityInfo
                {
                    MachineId = machine.MachineId,
                    MachineName = machine.MachineName,
                    MaxCapacity = 1, // Assuming 1 job per SLS machine
                    CurrentJobs = activeJobs,
                    IsAvailable = activeJobs == 0,
                    SupportedMaterials = machine.SupportedMaterials?.Split(',').ToList() ?? new List<string>(),
                    CurrentMaterial = machine.CurrentMaterial
                };

                MachineCapacity.Add(capacity);
                TotalCapacity += capacity.MaxCapacity;
                UsedCapacity += capacity.CurrentJobs;
            }

            UtilizationPercent = TotalCapacity > 0 ? (double)UsedCapacity / TotalCapacity * 100 : 0;
        }

        private List<StageJobInfo> MapToStageJobInfo(List<JobStage> jobStages)
        {
            return jobStages.Select(js => new StageJobInfo
            {
                JobStageId = js.Id,
                JobId = js.JobId,
                PartNumber = js.Job.PartNumber,
                PartDescription = js.Job.Part?.Description ?? "",
                StageName = js.StageName,
                Status = js.Status,
                ScheduledStart = js.ScheduledStart,
                ScheduledEnd = js.ScheduledEnd,
                ActualStart = js.ActualStart,
                ActualEnd = js.ActualEnd,
                ProgressPercent = js.ProgressPercent,
                MachineId = js.MachineId,
                AssignedOperator = js.AssignedOperator,
                EstimatedHours = js.EstimatedDurationHours,
                ActualHours = js.ActualStart.HasValue && js.ActualEnd.HasValue 
                    ? (js.ActualEnd.Value - js.ActualStart.Value).TotalHours 
                    : null,
                CanStart = js.CanStart,
                RequiresApproval = await RequiresApprovalCheckAsync(js.Id),
                IsBlocking = js.IsBlocking,
                Priority = js.Priority,
                Notes = js.Notes,
                Material = js.Job.SlsMaterial,
                Quantity = js.Job.Quantity
            }).ToList();
        }

        private async Task<bool> RequiresApprovalCheckAsync(int jobStageId)
        {
            var stage = await _context.ProductionStages
                .FirstOrDefaultAsync(ps => ps.Name == "SLS Printing");
            
            return stage?.RequiresApproval ?? false;
        }

        #region Action Handlers

        public async Task<IActionResult> OnPostStartJobAsync(int jobStageId)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                // Check permissions
                if (!await _stagePermissionService.CanUserStartStageAsync(UserName, jobStageId))
                {
                    return new JsonResult(new { success = false, message = "Access denied" });
                }

                var result = await _multiStageJobService.StartStageAsync(jobStageId, UserName);

                if (result)
                {
                    _logger.LogInformation("SLS job {JobStageId} started by {User} [Operation: {OperationId}]", 
                        jobStageId, UserName, operationId);
                    return new JsonResult(new { success = true, message = "Job started successfully" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to start job" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting SLS job {JobStageId} [Operation: {OperationId}]", 
                    jobStageId, operationId);
                return new JsonResult(new { success = false, message = "An error occurred" });
            }
        }

        public async Task<IActionResult> OnPostCompleteJobAsync(int jobStageId, string? notes)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                // Check permissions
                if (!await _stagePermissionService.CanUserCompleteStageAsync(UserName, jobStageId))
                {
                    return new JsonResult(new { success = false, message = "Access denied" });
                }

                var result = await _multiStageJobService.CompleteStageAsync(jobStageId, null, notes);

                if (result)
                {
                    _logger.LogInformation("SLS job {JobStageId} completed by {User} [Operation: {OperationId}]", 
                        jobStageId, UserName, operationId);
                    return new JsonResult(new { success = true, message = "Job completed successfully" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to complete job" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing SLS job {JobStageId} [Operation: {OperationId}]", 
                    jobStageId, operationId);
                return new JsonResult(new { success = false, message = "An error occurred" });
            }
        }

        public async Task<IActionResult> OnPostApproveJobAsync(int jobStageId, string? approvalNotes)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                // Only admins can approve
                if (!CanApprove)
                {
                    return new JsonResult(new { success = false, message = "Access denied - approval required" });
                }

                var result = await _multiStageJobService.ApproveStageAsync(jobStageId, UserName, approvalNotes);

                if (result)
                {
                    _logger.LogInformation("SLS job {JobStageId} approved by {User} [Operation: {OperationId}]", 
                        jobStageId, UserName, operationId);
                    return new JsonResult(new { success = true, message = "Job approved successfully" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to approve job" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving SLS job {JobStageId} [Operation: {OperationId}]", 
                    jobStageId, operationId);
                return new JsonResult(new { success = false, message = "An error occurred" });
            }
        }

        public async Task<IActionResult> OnPostAddToScheduleAsync(int partId, DateTime startDate, string machineId)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                // Only admins can add to schedule
                if (!CanSchedule)
                {
                    return new JsonResult(new { success = false, message = "Access denied - scheduling permission required" });
                }

                var result = await _multiStageJobService.CreateStageJobAsync(partId, StageId, startDate, machineId, UserName);

                if (result > 0)
                {
                    _logger.LogInformation("SLS job created for part {PartId} by {User} [Operation: {OperationId}]", 
                        partId, UserName, operationId);
                    return new JsonResult(new { success = true, message = "Job added to schedule successfully" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to add job to schedule" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding SLS job to schedule [Operation: {OperationId}]", operationId);
                return new JsonResult(new { success = false, message = "An error occurred" });
            }
        }

        #endregion
    }
}