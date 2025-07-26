using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using OpCentrix.Services.Admin;

namespace OpCentrix.Pages.Admin
{
    /// <summary>
    /// Stage management page for multi-stage scheduling
    /// Task 11: Multi-Stage Scheduling
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class StagesModel : PageModel
    {
        private readonly IMultiStageJobService _multiStageJobService;
        private readonly IStagePermissionService _stagePermissionService;
        private readonly ILogger<StagesModel> _logger;

        public StagesModel(
            IMultiStageJobService multiStageJobService,
            IStagePermissionService stagePermissionService,
            ILogger<StagesModel> logger)
        {
            _multiStageJobService = multiStageJobService;
            _stagePermissionService = stagePermissionService;
            _logger = logger;
        }

        // Properties for the page
        public List<JobStage> Stages { get; set; } = new List<JobStage>();
        public List<string> Departments { get; set; } = new List<string>();
        public List<string> StageTypes { get; set; } = new List<string>();
        public Dictionary<string, int> StatusCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, double> DepartmentUtilization { get; set; } = new Dictionary<string, double>();
        public List<JobStage> OverdueStages { get; set; } = new List<JobStage>();

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public string? SelectedDepartment { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("Loading stage management page - Admin: {User} [Operation: {OperationId}]", 
                User.Identity?.Name, operationId);

            try
            {
                // Set default date range if not provided
                StartDate ??= DateTime.Today.AddDays(-7);
                EndDate ??= DateTime.Today.AddDays(14);

                // Load user departments
                var userName = User.Identity?.Name ?? "unknown";
                Departments = await _stagePermissionService.GetUserDepartmentsAsync(userName);
                StageTypes = await _stagePermissionService.GetAllowedStageTypesAsync(userName);

                // Load stages based on filters
                await LoadStagesAsync(operationId);

                // Load statistics
                await LoadStatisticsAsync(operationId);

                _logger.LogInformation("Stage management page loaded successfully - {StageCount} stages, {DepartmentCount} departments [Operation: {OperationId}]", 
                    Stages.Count, Departments.Count, operationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stage management page [Operation: {OperationId}]", operationId);
                throw;
            }
        }

        private async Task LoadStagesAsync(string operationId)
        {
            try
            {
                if (!string.IsNullOrEmpty(SelectedDepartment))
                {
                    Stages = await _multiStageJobService.GetStagesByDepartmentAsync(
                        SelectedDepartment, StartDate ?? DateTime.Today.AddDays(-7), EndDate ?? DateTime.Today.AddDays(14));
                }
                else
                {
                    // Load stages for all user departments
                    var allStages = new List<JobStage>();
                    
                    foreach (var department in Departments)
                    {
                        var deptStages = await _multiStageJobService.GetStagesByDepartmentAsync(
                            department, StartDate ?? DateTime.Today.AddDays(-7), EndDate ?? DateTime.Today.AddDays(14));
                        allStages.AddRange(deptStages);
                    }
                    
                    Stages = allStages.OrderBy(s => s.ScheduledStart).ToList();
                }

                // Apply additional filters
                if (!string.IsNullOrEmpty(SelectedStatus))
                {
                    Stages = Stages.Where(s => s.Status == SelectedStatus).ToList();
                }

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    Stages = Stages.Where(s => 
                        s.StageName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        s.Job.PartNumber.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (s.MachineId != null && s.MachineId.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (s.AssignedOperator != null && s.AssignedOperator.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))).ToList();
                }

                _logger.LogDebug("Loaded {StageCount} stages with filters [Operation: {OperationId}]", Stages.Count, operationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stages [Operation: {OperationId}]", operationId);
                Stages = new List<JobStage>();
            }
        }

        private async Task LoadStatisticsAsync(string operationId)
        {
            try
            {
                // Calculate status counts
                StatusCounts = Stages
                    .GroupBy(s => s.Status)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Get department utilization
                DepartmentUtilization = await _multiStageJobService.GetDepartmentUtilizationAsync(
                    StartDate ?? DateTime.Today.AddDays(-7), EndDate ?? DateTime.Today.AddDays(14));

                // Get overdue stages
                OverdueStages = await _multiStageJobService.GetOverdueStagesAsync();

                _logger.LogDebug("Loaded statistics - {StatusCount} status types, {OverdueCount} overdue stages [Operation: {OperationId}]", 
                    StatusCounts.Count, OverdueStages.Count, operationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading statistics [Operation: {OperationId}]", operationId);
                StatusCounts = new Dictionary<string, int>();
                DepartmentUtilization = new Dictionary<string, double>();
                OverdueStages = new List<JobStage>();
            }
        }

        public async Task<IActionResult> OnPostStartStageAsync(int stageId)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                var userName = User.Identity?.Name ?? "unknown";
                
                if (!await _stagePermissionService.CanUserStartStageAsync(userName, stageId))
                {
                    _logger.LogWarning("User {User} denied permission to start stage {StageId} [Operation: {OperationId}]", 
                        userName, stageId, operationId);
                    return new JsonResult(new { success = false, message = "Access denied" });
                }

                var result = await _multiStageJobService.StartStageAsync(stageId, userName);

                if (result)
                {
                    _logger.LogInformation("Stage {StageId} started by user {User} [Operation: {OperationId}]", 
                        stageId, userName, operationId);
                    return new JsonResult(new { success = true, message = "Stage started successfully" });
                }
                else
                {
                    _logger.LogWarning("Failed to start stage {StageId} [Operation: {OperationId}]", stageId, operationId);
                    return new JsonResult(new { success = false, message = "Failed to start stage" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting stage {StageId} [Operation: {OperationId}]", stageId, operationId);
                return new JsonResult(new { success = false, message = "An error occurred" });
            }
        }

        public async Task<IActionResult> OnPostCompleteStageAsync(int stageId, decimal? actualCost)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                var userName = User.Identity?.Name ?? "unknown";
                
                if (!await _stagePermissionService.CanUserCompleteStageAsync(userName, stageId))
                {
                    _logger.LogWarning("User {User} denied permission to complete stage {StageId} [Operation: {OperationId}]", 
                        userName, stageId, operationId);
                    return new JsonResult(new { success = false, message = "Access denied" });
                }

                var result = await _multiStageJobService.CompleteStageAsync(stageId, actualCost);

                if (result)
                {
                    _logger.LogInformation("Stage {StageId} completed by user {User} [Operation: {OperationId}]", 
                        stageId, userName, operationId);
                    return new JsonResult(new { success = true, message = "Stage completed successfully" });
                }
                else
                {
                    _logger.LogWarning("Failed to complete stage {StageId} [Operation: {OperationId}]", stageId, operationId);
                    return new JsonResult(new { success = false, message = "Failed to complete stage" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing stage {StageId} [Operation: {OperationId}]", stageId, operationId);
                return new JsonResult(new { success = false, message = "An error occurred" });
            }
        }

        public async Task<IActionResult> OnPostUpdateProgressAsync(int stageId, double progressPercent)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                var userName = User.Identity?.Name ?? "unknown";
                
                if (!await _stagePermissionService.CanUserEditStageAsync(userName, stageId))
                {
                    _logger.LogWarning("User {User} denied permission to update stage {StageId} [Operation: {OperationId}]", 
                        userName, stageId, operationId);
                    return new JsonResult(new { success = false, message = "Access denied" });
                }

                var result = await _multiStageJobService.UpdateStageProgressAsync(stageId, progressPercent);

                if (result)
                {
                    _logger.LogInformation("Stage {StageId} progress updated to {Progress}% by user {User} [Operation: {OperationId}]", 
                        stageId, progressPercent, userName, operationId);
                    return new JsonResult(new { success = true, message = "Progress updated successfully" });
                }
                else
                {
                    _logger.LogWarning("Failed to update progress for stage {StageId} [Operation: {OperationId}]", stageId, operationId);
                    return new JsonResult(new { success = false, message = "Failed to update progress" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating progress for stage {StageId} [Operation: {OperationId}]", stageId, operationId);
                return new JsonResult(new { success = false, message = "An error occurred" });
            }
        }

        public async Task<IActionResult> OnPostRescheduleStageAsync(int stageId, DateTime newStart, DateTime newEnd)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                var userName = User.Identity?.Name ?? "unknown";
                
                if (!await _stagePermissionService.CanUserRescheduleStagesAsync(userName))
                {
                    _logger.LogWarning("User {User} denied permission to reschedule stage {StageId} [Operation: {OperationId}]", 
                        userName, stageId, operationId);
                    return new JsonResult(new { success = false, message = "Access denied" });
                }

                var result = await _multiStageJobService.RescheduleStageAsync(stageId, newStart, newEnd);

                if (result)
                {
                    _logger.LogInformation("Stage {StageId} rescheduled by user {User} [Operation: {OperationId}]", 
                        stageId, userName, operationId);
                    return new JsonResult(new { success = true, message = "Stage rescheduled successfully" });
                }
                else
                {
                    _logger.LogWarning("Failed to reschedule stage {StageId} [Operation: {OperationId}]", stageId, operationId);
                    return new JsonResult(new { success = false, message = "Failed to reschedule stage" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescheduling stage {StageId} [Operation: {OperationId}]", stageId, operationId);
                return new JsonResult(new { success = false, message = "An error occurred" });
            }
        }

        public async Task<IActionResult> OnGetStageDetailsAsync(int stageId)
        {
            try
            {
                var stage = await _multiStageJobService.GetStageByIdAsync(stageId);
                if (stage == null)
                {
                    return new JsonResult(new { success = false, message = "Stage not found" });
                }

                var userName = User.Identity?.Name ?? "unknown";
                if (!await _stagePermissionService.CanUserAccessStageAsync(userName, stage.Department, "View"))
                {
                    return new JsonResult(new { success = false, message = "Access denied" });
                }

                return new JsonResult(new 
                { 
                    success = true, 
                    stage = new 
                    {
                        stage.Id,
                        stage.StageName,
                        stage.Department,
                        stage.Status,
                        stage.ScheduledStart,
                        stage.ScheduledEnd,
                        stage.ProgressPercent,
                        stage.MachineId,
                        stage.AssignedOperator,
                        stage.Notes,
                        stage.EstimatedCost,
                        stage.ActualCost
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stage details for stage {StageId}", stageId);
                return new JsonResult(new { success = false, message = "An error occurred" });
            }
        }
    }
}