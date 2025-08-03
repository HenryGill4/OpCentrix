using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.JobStaging;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.Admin;

/// <summary>
/// Advanced Job Stage Management - Task 11: Modular Multi-Stage Scheduling
/// Provides comprehensive CRUD operations for job stages with dependency management
/// Enhanced for manufacturing workflow orchestration and resource allocation
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class StagesModel : PageModel
{
    private readonly SchedulerContext _context;
    private readonly ILogger<StagesModel> _logger;

    public StagesModel(SchedulerContext context, ILogger<StagesModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Data properties for the page
    public List<JobStage> JobStages { get; set; } = new List<JobStage>();
    public List<JobStageDependency> StageDependencies { get; set; } = new List<JobStageDependency>();
    public List<Machine> AvailableMachines { get; set; } = new List<Machine>();
    public List<Job> ActiveJobs { get; set; } = new List<Job>();
    public Dictionary<string, int> StageStatistics { get; set; } = new Dictionary<string, int>();

    // Additional properties needed by the Razor page
    public List<JobStage> Stages => JobStages;
    public List<JobStage> OverdueStages => JobStages.Where(s => s.IsOverdue).ToList();
    public List<string> Departments => JobStages.Select(s => s.Department).Distinct().Where(d => !string.IsNullOrEmpty(d)).ToList();
    public Dictionary<string, int> StatusCounts => JobStages.GroupBy(s => s.Status).ToDictionary(g => g.Key, g => g.Count());
    public Dictionary<string, double> DepartmentUtilization => CalculateDepartmentUtilization();
    
    // Filter properties
    public string? SelectedDepartment { get; set; }
    public string? SelectedStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Form binding properties
    [BindProperty]
    public CreateStageViewModel NewStage { get; set; } = new CreateStageViewModel();

    [BindProperty]
    public CreateDependencyViewModel NewDependency { get; set; } = new CreateDependencyViewModel();

    public async Task OnGetAsync(string? department = null, string? status = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("Loading Job Stages Management page");
            
            // Set filter properties
            SelectedDepartment = department;
            SelectedStatus = status;
            StartDate = startDate;
            EndDate = endDate;
            
            await LoadStageDataAsync();
            await LoadStageStatisticsAsync();
            
            _logger.LogInformation("Loaded {StageCount} stages, {DependencyCount} dependencies", 
                JobStages.Count, StageDependencies.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading stage management data");
            TempData["ErrorMessage"] = "Error loading stage management data. Please try again.";
        }
    }

    #region Stage Management

    public async Task<IActionResult> OnPostCreateStageAsync()
    {
        try
        {
            _logger.LogInformation("Creating job stage: {StageName}", NewStage?.StageName ?? "null");
            
            if (NewStage == null)
            {
                TempData["ErrorMessage"] = "Invalid stage data received.";
                return RedirectToPage();
            }

            // Validate stage data
            if (string.IsNullOrWhiteSpace(NewStage.StageName))
            {
                TempData["ErrorMessage"] = "Stage name is required.";
                return RedirectToPage();
            }

            // Check for duplicate stage names within the same job
            if (NewStage.JobId > 0)
            {
                var existingStage = await _context.JobStages
                    .FirstOrDefaultAsync(js => js.JobId == NewStage.JobId && 
                                              js.StageName.ToLower() == NewStage.StageName.ToLower());
                
                if (existingStage != null)
                {
                    TempData["ErrorMessage"] = $"A stage with the name '{NewStage.StageName}' already exists for this job.";
                    return RedirectToPage();
                }
            }

            // Create new job stage
            var jobStage = new JobStage
            {
                JobId = NewStage.JobId,
                StageType = NewStage.StageType,
                StageName = NewStage.StageName,
                Department = NewStage.Department,
                MachineId = NewStage.MachineId,
                ExecutionOrder = NewStage.ExecutionOrder,
                ScheduledStart = NewStage.ScheduledStart,
                ScheduledEnd = NewStage.ScheduledEnd,
                EstimatedDurationHours = NewStage.EstimatedDurationHours,
                Priority = NewStage.Priority,
                Status = "Scheduled",
                CanStart = NewStage.ExecutionOrder == 1, // First stage can start immediately
                SetupTimeHours = NewStage.SetupTimeHours,
                CooldownTimeHours = NewStage.CooldownTimeHours,
                AssignedOperator = NewStage.AssignedOperator,
                Notes = NewStage.Notes,
                QualityRequirements = NewStage.QualityRequirements,
                RequiredMaterials = NewStage.RequiredMaterials,
                RequiredTooling = NewStage.RequiredTooling,
                EstimatedCost = NewStage.EstimatedCost,
                IsBlocking = NewStage.IsBlocking,
                AllowParallel = NewStage.AllowParallel,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedDate = DateTime.UtcNow,
                LastModifiedBy = User.Identity?.Name ?? "System",
                LastModifiedDate = DateTime.UtcNow
            };

            _context.JobStages.Add(jobStage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully created job stage: {StageName} with ID: {StageId}", 
                jobStage.StageName, jobStage.Id);
            TempData["SuccessMessage"] = $"Job stage '{jobStage.StageName}' created successfully.";
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job stage: {StageName}", NewStage?.StageName ?? "null");
            TempData["ErrorMessage"] = $"Error creating job stage: {ex.Message}";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostDeleteStageAsync(int stageId)
    {
        try
        {
            _logger.LogInformation("Attempting to delete job stage with ID: {StageId}", stageId);
            
            var jobStage = await _context.JobStages
                .Include(js => js.Dependencies)
                .Include(js => js.Dependents)
                .FirstOrDefaultAsync(js => js.Id == stageId);
            
            if (jobStage == null)
            {
                TempData["ErrorMessage"] = "Job stage not found.";
                return RedirectToPage();
            }

            // Check if stage has dependencies or dependents
            if (jobStage.Dependencies.Any() || jobStage.Dependents.Any())
            {
                _logger.LogWarning("Cannot delete stage {StageId} - it has dependencies", stageId);
                TempData["ErrorMessage"] = $"Cannot delete '{jobStage.StageName}' - it has dependencies. Please remove dependencies first.";
                return RedirectToPage();
            }

            // Check if stage is in progress or completed
            if (jobStage.Status == "In-Progress" || jobStage.Status == "Completed")
            {
                _logger.LogWarning("Cannot delete stage {StageId} - it is {Status}", stageId, jobStage.Status);
                TempData["ErrorMessage"] = $"Cannot delete '{jobStage.StageName}' - it is {jobStage.Status.ToLower()}. ";
                return RedirectToPage();
            }

            _context.JobStages.Remove(jobStage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted job stage: {StageName}", jobStage.StageName);
            TempData["SuccessMessage"] = $"Job stage '{jobStage.StageName}' deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job stage {StageId}", stageId);
            TempData["ErrorMessage"] = "Error deleting job stage. Please try again.";
        }

        return RedirectToPage();
    }

    #endregion

    #region Dependency Management

    public async Task<IActionResult> OnPostCreateDependencyAsync()
    {
        try
        {
            _logger.LogInformation("Creating stage dependency: {DependentStage} depends on {RequiredStage}", 
                NewDependency?.DependentStageId, NewDependency?.RequiredStageId);
            
            if (NewDependency == null)
            {
                TempData["ErrorMessage"] = "Invalid dependency data received.";
                return RedirectToPage();
            }

            // Validate dependency
            if (NewDependency.DependentStageId == NewDependency.RequiredStageId)
            {
                TempData["ErrorMessage"] = "A stage cannot depend on itself.";
                return RedirectToPage();
            }

            // Check for duplicate dependencies
            var existingDependency = await _context.StageDependencies
                .FirstOrDefaultAsync(sd => sd.DependentStageId == NewDependency.DependentStageId &&
                                          sd.RequiredStageId == NewDependency.RequiredStageId);
            
            if (existingDependency != null)
            {
                TempData["ErrorMessage"] = "This dependency already exists.";
                return RedirectToPage();
            }

            // Check for circular dependencies
            if (await WouldCreateCircularDependencyAsync(NewDependency.DependentStageId, NewDependency.RequiredStageId))
            {
                TempData["ErrorMessage"] = "This dependency would create a circular reference.";
                return RedirectToPage();
            }

            // Create stage dependency
            var stageDependency = new JobStageDependency
            {
                DependentStageId = NewDependency.DependentStageId,
                RequiredStageId = NewDependency.RequiredStageId,
                DependencyType = NewDependency.DependencyType,
                LagTimeHours = NewDependency.LagTimeHours,
                IsMandatory = NewDependency.IsMandatory,
                Notes = NewDependency.Notes,
                CreatedDate = DateTime.UtcNow
            };

            _context.StageDependencies.Add(stageDependency);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully created stage dependency with ID: {DependencyId}", stageDependency.Id);
            TempData["SuccessMessage"] = "Stage dependency created successfully.";
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stage dependency");
            TempData["ErrorMessage"] = $"Error creating stage dependency: {ex.Message}";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostDeleteDependencyAsync(int dependencyId)
    {
        try
        {
            _logger.LogInformation("Attempting to delete stage dependency with ID: {DependencyId}", dependencyId);
            
            var stageDependency = await _context.StageDependencies.FindAsync(dependencyId);
            if (stageDependency == null)
            {
                TempData["ErrorMessage"] = "Stage dependency not found.";
                return RedirectToPage();
            }

            _context.StageDependencies.Remove(stageDependency);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted stage dependency");
            TempData["SuccessMessage"] = "Stage dependency deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stage dependency {DependencyId}", dependencyId);
            TempData["ErrorMessage"] = "Error deleting stage dependency. Please try again.";
        }

        return RedirectToPage();
    }

    #endregion

    #region Helper Methods

    private async Task LoadStageDataAsync()
    {
        var query = _context.JobStages
            .Include(js => js.Job)
            .Include(js => js.Dependencies)
                .ThenInclude(d => d.RequiredStage)
            .Include(js => js.Dependents)
                .ThenInclude(d => d.DependentStage)
            .Include(js => js.StageNotes)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(SelectedDepartment))
        {
            query = query.Where(js => js.Department == SelectedDepartment);
        }

        if (!string.IsNullOrEmpty(SelectedStatus))
        {
            query = query.Where(js => js.Status == SelectedStatus);
        }

        if (StartDate.HasValue)
        {
            query = query.Where(js => js.ScheduledStart >= StartDate.Value);
        }

        if (EndDate.HasValue)
        {
            query = query.Where(js => js.ScheduledEnd <= EndDate.Value);
        }

        // Load job stages with related data
        JobStages = await query
            .OrderBy(js => js.JobId)
            .ThenBy(js => js.ExecutionOrder)
            .ToListAsync();

        // Load stage dependencies
        StageDependencies = await _context.StageDependencies
            .Include(sd => sd.DependentStage)
            .Include(sd => sd.RequiredStage)
            .OrderBy(sd => sd.DependentStage.JobId)
            .ThenBy(sd => sd.DependentStage.ExecutionOrder)
            .ToListAsync();

        // Load available machines
        AvailableMachines = await _context.Machines
            .Where(m => m.IsActive)
            .OrderBy(m => m.MachineId)
            .ToListAsync();

        // Load active jobs
        ActiveJobs = await _context.Jobs
            .Where(j => j.Status == "In Progress" || j.Status == "Scheduled")
            .OrderBy(j => j.ScheduledStart)
            .ToListAsync();
    }

    private async Task LoadStageStatisticsAsync()
    {
        try
        {
            StageStatistics["TotalStages"] = await _context.JobStages.CountAsync();
            StageStatistics["ActiveStages"] = await _context.JobStages.CountAsync(js => js.Status == "In-Progress");
            StageStatistics["CompletedStages"] = await _context.JobStages.CountAsync(js => js.Status == "Completed");
            StageStatistics["ScheduledStages"] = await _context.JobStages.CountAsync(js => js.Status == "Scheduled");
            StageStatistics["BlockedStages"] = await _context.JobStages.CountAsync(js => !js.CanStart && js.Status == "Scheduled");
            StageStatistics["TotalDependencies"] = await _context.StageDependencies.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading stage statistics");
            // Continue without statistics rather than failing completely
        }
    }

    private Dictionary<string, double> CalculateDepartmentUtilization()
    {
        var utilization = new Dictionary<string, double>();
        
        var departmentGroups = JobStages
            .Where(s => !string.IsNullOrEmpty(s.Department))
            .GroupBy(s => s.Department);

        foreach (var group in departmentGroups)
        {
            var totalHours = group.Sum(s => s.EstimatedDurationHours);
            var activeHours = group.Where(s => s.Status == "In-Progress").Sum(s => s.EstimatedDurationHours);
            var utilizationPercent = totalHours > 0 ? (activeHours / totalHours) * 100 : 0;
            utilization[group.Key] = utilizationPercent;
        }

        return utilization;
    }

    private async Task<bool> WouldCreateCircularDependencyAsync(int dependentStageId, int requiredStageId)
    {
        // Check if requiredStageId eventually depends on dependentStageId
        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(requiredStageId);

        while (queue.Count > 0)
        {
            var currentStageId = queue.Dequeue();
            
            if (currentStageId == dependentStageId)
            {
                return true; // Circular dependency found
            }

            if (visited.Contains(currentStageId))
            {
                continue;
            }

            visited.Add(currentStageId);

            // Get all stages that depend on the current stage
            var dependentStages = await _context.StageDependencies
                .Where(sd => sd.RequiredStageId == currentStageId)
                .Select(sd => sd.DependentStageId)
                .ToListAsync();

            foreach (var stage in dependentStages)
            {
                queue.Enqueue(stage);
            }
        }

        return false;
    }

    public async Task<IActionResult> OnGetStageDetailsAsync(int stageId)
    {
        try
        {
            var stage = await _context.JobStages
                .Include(js => js.Job)
                .Include(js => js.Dependencies)
                    .ThenInclude(d => d.RequiredStage)
                .Include(js => js.Dependents)
                    .ThenInclude(d => d.DependentStage)
                .Include(js => js.StageNotes)
                .FirstOrDefaultAsync(js => js.Id == stageId);
            
            if (stage == null)
            {
                return NotFound("Stage not found");
            }

            return new JsonResult(new
            {
                id = stage.Id,
                jobId = stage.JobId,
                stageName = stage.StageName,
                stageType = stage.StageType,
                department = stage.Department,
                status = stage.Status,
                progressPercent = stage.ProgressPercent,
                scheduledStart = stage.ScheduledStart,
                scheduledEnd = stage.ScheduledEnd,
                actualStart = stage.ActualStart,
                actualEnd = stage.ActualEnd,
                estimatedDurationHours = stage.EstimatedDurationHours,
                actualDurationHours = stage.ActualDurationHours,
                machineId = stage.MachineId,
                assignedOperator = stage.AssignedOperator,
                priority = stage.Priority,
                executionOrder = stage.ExecutionOrder,
                canStart = stage.CanStart,
                isBlocking = stage.IsBlocking,
                allowParallel = stage.AllowParallel,
                notes = stage.Notes,
                qualityRequirements = stage.QualityRequirements,
                requiredMaterials = stage.RequiredMaterials,
                requiredTooling = stage.RequiredTooling,
                estimatedCost = stage.EstimatedCost,
                actualCost = stage.ActualCost,
                setupTimeHours = stage.SetupTimeHours,
                cooldownTimeHours = stage.CooldownTimeHours,
                isOverdue = stage.IsOverdue,
                statusColor = stage.StatusColor,
                departmentColor = stage.DepartmentColor,
                dependencies = stage.Dependencies.Select(d => new
                {
                    id = d.Id,
                    requiredStageId = d.RequiredStageId,
                    requiredStageName = d.RequiredStage.StageName,
                    dependencyType = d.DependencyType,
                    lagTimeHours = d.LagTimeHours,
                    isMandatory = d.IsMandatory
                }),
                dependents = stage.Dependents.Select(d => new
                {
                    id = d.Id,
                    dependentStageId = d.DependentStageId,
                    dependentStageName = d.DependentStage.StageName,
                    dependencyType = d.DependencyType,
                    lagTimeHours = d.LagTimeHours,
                    isMandatory = d.IsMandatory
                }),
                stageNotes = stage.StageNotes.OrderByDescending(sn => sn.CreatedDate).Select(sn => new
                {
                    id = sn.Id,
                    note = sn.Note,
                    noteType = sn.NoteType,
                    priority = sn.Priority,
                    isPublic = sn.IsPublic,
                    createdBy = sn.CreatedBy,
                    createdDate = sn.CreatedDate,
                    noteTypeColor = sn.NoteTypeColor
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stage details for {StageId}", stageId);
            return BadRequest("Error loading stage details");
        }
    }

    #endregion
}

#region View Models

public class CreateStageViewModel
{
    [Required]
    [Display(Name = "Job ID")]
    public int JobId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Stage Type")]
    public string StageType { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Stage Name")]
    public string StageName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Department")]
    public string Department { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Machine ID")]
    public string? MachineId { get; set; }

    [Range(1, 100)]
    [Display(Name = "Execution Order")]
    public int ExecutionOrder { get; set; } = 1;

    [Required]
    [Display(Name = "Scheduled Start")]
    public DateTime ScheduledStart { get; set; } = DateTime.Now;

    [Required]
    [Display(Name = "Scheduled End")]
    public DateTime ScheduledEnd { get; set; } = DateTime.Now.AddHours(1);

    [Range(0.1, 168)]
    [Display(Name = "Estimated Duration (hours)")]
    public double EstimatedDurationHours { get; set; } = 1.0;

    [Range(1, 5)]
    [Display(Name = "Priority")]
    public int Priority { get; set; } = 3;

    [Range(0, 24)]
    [Display(Name = "Setup Time (hours)")]
    public double SetupTimeHours { get; set; } = 0;

    [Range(0, 24)]
    [Display(Name = "Cooldown Time (hours)")]
    public double CooldownTimeHours { get; set; } = 0;

    [StringLength(100)]
    [Display(Name = "Assigned Operator")]
    public string? AssignedOperator { get; set; }

    [StringLength(2000)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [StringLength(1000)]
    [Display(Name = "Quality Requirements")]
    public string? QualityRequirements { get; set; }

    [StringLength(1000)]
    [Display(Name = "Required Materials")]
    public string? RequiredMaterials { get; set; }

    [StringLength(1000)]
    [Display(Name = "Required Tooling")]
    public string? RequiredTooling { get; set; }

    [Range(0, 100000)]
    [Display(Name = "Estimated Cost")]
    public decimal EstimatedCost { get; set; } = 0;

    [Display(Name = "Is Blocking")]
    public bool IsBlocking { get; set; } = true;

    [Display(Name = "Allow Parallel")]
    public bool AllowParallel { get; set; } = false;
}

public class CreateDependencyViewModel
{
    [Required]
    [Display(Name = "Dependent Stage")]
    public int DependentStageId { get; set; }

    [Required]
    [Display(Name = "Required Stage")]
    public int RequiredStageId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Dependency Type")]
    public string DependencyType { get; set; } = "FinishToStart";

    [Range(-24, 168)]
    [Display(Name = "Lag Time (hours)")]
    public double LagTimeHours { get; set; } = 0;

    [Display(Name = "Is Mandatory")]
    public bool IsMandatory { get; set; } = true;

    [StringLength(500)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}

#endregion