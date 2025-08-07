using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.JobStaging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace OpCentrix.Pages.Admin;

/// <summary>
/// Enhanced Stage Management - Integrates ProductionStage templates with Job execution stages
/// Provides unified workflow for stage configuration and job stage execution
/// Refactored to align with B&T Manufacturing requirements
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

    // Production Stage Templates
    public List<ProductionStage> ProductionStageTemplates { get; set; } = new List<ProductionStage>();
    public Dictionary<int, StageUsageStats> StageTemplateUsage { get; set; } = new Dictionary<int, StageUsageStats>();

    // Active Job Stages
    public List<JobStage> ActiveJobStages { get; set; } = new List<JobStage>();
    public List<JobStage> OverdueStages => ActiveJobStages.Where(s => s.IsOverdue).ToList();
    public List<string> Departments => ActiveJobStages.Select(s => s.Department).Distinct().Where(d => !string.IsNullOrEmpty(d)).ToList();
    public Dictionary<string, int> StatusCounts => ActiveJobStages.GroupBy(s => s.Status).ToDictionary(g => g.Key, g => g.Count());

    // Available Resources
    public List<Machine> AvailableMachines { get; set; } = new List<Machine>();
    public List<Job> ActiveJobs { get; set; } = new List<Job>();

    // Stage Dependencies
    public List<JobStageDependency> StageDependencies { get; set; } = new List<JobStageDependency>();

    // Filter properties
    public string? SelectedDepartment { get; set; }
    public string? SelectedStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SelectedView { get; set; } = "active"; // active, templates, dependencies

    // Form binding properties
    [BindProperty]
    public CreateStageFromTemplateViewModel NewStageFromTemplate { get; set; } = new CreateStageFromTemplateViewModel();

    [BindProperty]
    public CreateJobStageViewModel NewJobStage { get; set; } = new CreateJobStageViewModel();

    [BindProperty]
    public UpdateProductionStageViewModel UpdateStageTemplate { get; set; } = new UpdateProductionStageViewModel();

    public async Task OnGetAsync(string? department = null, string? status = null, DateTime? startDate = null, 
        DateTime? endDate = null, string? view = "active")
    {
        try
        {
            _logger.LogInformation("Loading Enhanced Stage Management page with view: {View}", view);
            
            // Set filter properties
            SelectedDepartment = department;
            SelectedStatus = status;
            StartDate = startDate;
            EndDate = endDate;
            SelectedView = view ?? "active";
            
            // FIXED: Initialize form with proper default values
            NewStageFromTemplate = new CreateStageFromTemplateViewModel
            {
                ScheduledStart = DateTime.Now.AddHours(1), // Default to 1 hour from now
                Priority = 3 // Default priority
            };
            
            await LoadStageDataAsync();
            
            _logger.LogInformation("Loaded {TemplateCount} stage templates, {ActiveStageCount} active job stages", 
                ProductionStageTemplates.Count, ActiveJobStages.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading stage management data");
            TempData["ErrorMessage"] = "Error loading stage management data. Please try again.";
        }
    }

    #region Stage Template Management

    public async Task<IActionResult> OnPostCreateStageFromTemplateAsync()
    {
        try
        {
            _logger.LogInformation("Creating job stage from template: {TemplateId} for Job: {JobId}", 
                NewStageFromTemplate?.ProductionStageId ?? 0, NewStageFromTemplate?.JobId ?? 0);
            
            // ENHANCED: Better validation and error messaging
            if (NewStageFromTemplate == null)
            {
                ModelState.AddModelError("", "Invalid stage data received.");
                TempData["ErrorMessage"] = "Invalid stage data received.";
                await LoadStageDataAsync(); // Reload data for display
                return Page();
            }

            // FIXED: Validate model state properly
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed for new stage from template");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogDebug("Validation error: {Error}", error.ErrorMessage);
                }
                
                // Reload data for display
                await LoadStageDataAsync();
                return Page();
            }

            // Validate job exists and is in correct status
            var job = await _context.Jobs.FindAsync(NewStageFromTemplate.JobId);
            if (job == null)
            {
                ModelState.AddModelError("NewStageFromTemplate.JobId", "Job not found.");
                TempData["ErrorMessage"] = "Job not found.";
                await LoadStageDataAsync();
                return Page();
            }

            if (job.Status == "Completed" || job.Status == "Cancelled")
            {
                ModelState.AddModelError("NewStageFromTemplate.JobId", "Cannot add stages to completed or cancelled jobs.");
                TempData["ErrorMessage"] = "Cannot add stages to completed or cancelled jobs.";
                await LoadStageDataAsync();
                return Page();
            }

            // Get the production stage template
            var template = await _context.ProductionStages.FindAsync(NewStageFromTemplate.ProductionStageId);
            if (template == null)
            {
                ModelState.AddModelError("NewStageFromTemplate.ProductionStageId", "Production stage template not found.");
                TempData["ErrorMessage"] = "Production stage template not found.";
                await LoadStageDataAsync();
                return Page();
            }

            // Check for duplicate stage names within the same job
            var existingStage = await _context.JobStages
                .FirstOrDefaultAsync(js => js.JobId == NewStageFromTemplate.JobId && 
                                          js.StageName.ToLower() == template.Name.ToLower());
            
            if (existingStage != null)
            {
                ModelState.AddModelError("NewStageFromTemplate.ProductionStageId", $"A stage with the name '{template.Name}' already exists for this job.");
                TempData["ErrorMessage"] = $"A stage with the name '{template.Name}' already exists for this job.";
                await LoadStageDataAsync();
                return Page();
            }

            // Get next execution order
            var maxOrder = await _context.JobStages
                .Where(js => js.JobId == NewStageFromTemplate.JobId)
                .MaxAsync(js => (int?)js.ExecutionOrder) ?? 0;

            // Create job stage from template
            var jobStage = new JobStage
            {
                JobId = NewStageFromTemplate.JobId,
                StageType = template.Name,
                StageName = template.Name,
                Department = template.Department ?? "Production",
                MachineId = NewStageFromTemplate.MachineId ?? template.DefaultMachineId,
                ExecutionOrder = NewStageFromTemplate.ExecutionOrder ?? (maxOrder + 1),
                ScheduledStart = NewStageFromTemplate.ScheduledStart,
                ScheduledEnd = NewStageFromTemplate.ScheduledStart.AddHours(template.DefaultDurationHours),
                EstimatedDurationHours = template.DefaultDurationHours,
                Priority = NewStageFromTemplate.Priority,
                Status = "Scheduled",
                CanStart = (NewStageFromTemplate.ExecutionOrder ?? (maxOrder + 1)) == 1,
                SetupTimeHours = template.DefaultSetupMinutes / 60.0,
                CooldownTimeHours = 0,
                AssignedOperator = NewStageFromTemplate.AssignedOperator,
                Notes = NewStageFromTemplate.Notes,
                QualityRequirements = template.RequiresQualityCheck ? "Quality check required as per stage template" : null,
                RequiredMaterials = "[]", // Will be populated from template custom fields
                RequiredTooling = "",
                EstimatedCost = template.GetTotalEstimatedCost(),
                IsBlocking = !template.AllowSkip,
                AllowParallel = template.AllowParallelExecution,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedDate = DateTime.UtcNow,
                LastModifiedBy = User.Identity?.Name ?? "System",
                LastModifiedDate = DateTime.UtcNow
            };

            // Note: JobStage doesn't have StageParameters property, so we store custom field data in Notes
            var customFields = template.GetCustomFields();
            if (customFields.Any())
            {
                var customFieldsJson = JsonSerializer.Serialize(customFields.ToDictionary(
                    cf => cf.Name,
                    cf => cf.DefaultValue ?? ""
                ));
                jobStage.Notes = string.IsNullOrEmpty(jobStage.Notes) 
                    ? $"Template custom fields: {customFieldsJson}"
                    : $"{jobStage.Notes}\n\nTemplate custom fields: {customFieldsJson}";
            }

            _context.JobStages.Add(jobStage);
            await _context.SaveChangesAsync();

            // Create part stage requirement if this job has parts
            var jobParts = await _context.Jobs
                .Where(j => j.Id == NewStageFromTemplate.JobId)
                .Select(j => j.PartNumber)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(jobParts))
            {
                var part = await _context.Parts.FirstOrDefaultAsync(p => p.PartNumber == jobParts);
                if (part != null)
                {
                    var existingRequirement = await _context.PartStageRequirements
                        .FirstOrDefaultAsync(psr => psr.PartId == part.Id && psr.ProductionStageId == template.Id);

                    if (existingRequirement == null)
                    {
                        var partStageReq = new PartStageRequirement
                        {
                            PartId = part.Id,
                            ProductionStageId = template.Id,
                            ExecutionOrder = jobStage.ExecutionOrder,
                            IsRequired = !template.IsOptional,
                            IsActive = true,
                            EstimatedHours = template.DefaultDurationHours,
                            EstimatedCost = template.GetTotalEstimatedCost(),
                            AssignedMachineId = jobStage.MachineId
                        };
                        _context.PartStageRequirements.Add(partStageReq);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            _logger.LogInformation("Successfully created job stage: {StageName} with ID: {StageId}", 
                jobStage.StageName, jobStage.Id);
            TempData["SuccessMessage"] = $"Job stage '{jobStage.StageName}' created successfully from template.";
            
            return RedirectToPage(new { view = "active" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job stage from template");
            ModelState.AddModelError("", $"Error creating job stage: {ex.Message}");
            TempData["ErrorMessage"] = $"Error creating job stage: {ex.Message}";
            
            // Reload data for display
            await LoadStageDataAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateStageTemplateAsync()
    {
        try
        {
            _logger.LogInformation("Updating production stage template: {StageId}", UpdateStageTemplate.Id);
            
            var stage = await _context.ProductionStages.FindAsync(UpdateStageTemplate.Id);
            if (stage == null)
            {
                TempData["ErrorMessage"] = "Production stage template not found.";
                return RedirectToPage();
            }

            // Update basic properties
            stage.Name = UpdateStageTemplate.Name;
            stage.Description = UpdateStageTemplate.Description;
            stage.Department = UpdateStageTemplate.Department;
            stage.DefaultDurationHours = UpdateStageTemplate.DefaultDurationHours;
            stage.DefaultSetupMinutes = UpdateStageTemplate.DefaultSetupMinutes;
            stage.DefaultHourlyRate = UpdateStageTemplate.DefaultHourlyRate;
            stage.DefaultMaterialCost = UpdateStageTemplate.DefaultMaterialCost;
            stage.RequiresQualityCheck = UpdateStageTemplate.RequiresQualityCheck;
            stage.RequiresApproval = UpdateStageTemplate.RequiresApproval;
            stage.AllowSkip = UpdateStageTemplate.AllowSkip;
            stage.IsOptional = UpdateStageTemplate.IsOptional;
            stage.AllowParallelExecution = UpdateStageTemplate.AllowParallelExecution;
            stage.RequiredRole = UpdateStageTemplate.RequiredRole;
            stage.LastModifiedBy = User.Identity?.Name ?? "System";
            stage.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated production stage template: {StageName}", stage.Name);
            TempData["SuccessMessage"] = $"Production stage template '{stage.Name}' updated successfully.";
            
            return RedirectToPage(new { view = "templates" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating production stage template");
            TempData["ErrorMessage"] = $"Error updating stage template: {ex.Message}";
            return RedirectToPage();
        }
    }

    #endregion

    #region Job Stage Management

    public async Task<IActionResult> OnPostStartStageAsync(int stageId)
    {
        try
        {
            var stage = await _context.JobStages.FindAsync(stageId);
            if (stage == null)
            {
                return new JsonResult(new { success = false, message = "Stage not found" });
            }

            if (!stage.CanStart)
            {
                return new JsonResult(new { success = false, message = "Stage cannot be started - dependencies not met" });
            }

            if (stage.Status != "Scheduled" && stage.Status != "Ready")
            {
                return new JsonResult(new { success = false, message = $"Stage is {stage.Status} and cannot be started" });
            }

            stage.Status = "In-Progress";
            stage.ActualStart = DateTime.UtcNow;
            stage.ProgressPercent = 0;
            stage.LastModifiedBy = User.Identity?.Name ?? "System";
            stage.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update dependent stages
            await UpdateDependentStagesAsync(stageId);

            return new JsonResult(new { success = true, message = "Stage started successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting stage {StageId}", stageId);
            return new JsonResult(new { success = false, message = "Error starting stage" });
        }
    }

    public async Task<IActionResult> OnPostCompleteStageAsync(int stageId, decimal? actualCost)
    {
        try
        {
            var stage = await _context.JobStages.FindAsync(stageId);
            if (stage == null)
            {
                return new JsonResult(new { success = false, message = "Stage not found" });
            }

            if (stage.Status != "In-Progress")
            {
                return new JsonResult(new { success = false, message = $"Stage is {stage.Status} and cannot be completed" });
            }

            stage.Status = "Completed";
            stage.ActualEnd = DateTime.UtcNow;
            stage.ProgressPercent = 100;
            
            // ActualDurationHours is a computed property, no need to set it directly
            
            if (actualCost.HasValue)
            {
                stage.ActualCost = actualCost.Value;
            }

            stage.LastModifiedBy = User.Identity?.Name ?? "System";
            stage.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update dependent stages
            await UpdateDependentStagesAsync(stageId);

            return new JsonResult(new { success = true, message = "Stage completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing stage {StageId}", stageId);
            return new JsonResult(new { success = false, message = "Error completing stage" });
        }
    }

    #endregion

    #region Helper Methods

    private async Task LoadStageDataAsync()
    {
        // Load production stage templates
        ProductionStageTemplates = await _context.ProductionStages
            .Where(ps => ps.IsActive)
            .OrderBy(ps => ps.DisplayOrder)
            .ToListAsync();

        // Calculate template usage statistics
        foreach (var template in ProductionStageTemplates)
        {
            var usage = new StageUsageStats
            {
                PartUsageCount = await _context.PartStageRequirements.CountAsync(psr => psr.ProductionStageId == template.Id && psr.IsActive),
                PrototypeUsageCount = await _context.ProductionStageExecutions.CountAsync(pse => pse.ProductionStageId == template.Id && pse.PrototypeJobId != null),
                CompletedExecutions = await _context.ProductionStageExecutions.CountAsync(pse => pse.ProductionStageId == template.Id && pse.Status == "Completed")
            };

            // Get actual duration from ProductionStageExecutions, not JobStages
            var completedExecutions = await _context.ProductionStageExecutions
                .Where(pse => pse.ProductionStageId == template.Id && pse.ActualEndTime.HasValue && pse.ActualStartTime.HasValue)
                .Select(pse => (pse.ActualEndTime!.Value - pse.ActualStartTime!.Value).TotalHours)
                .ToListAsync();

            usage.AverageActualHours = completedExecutions.Any() ? completedExecutions.Average() : 0;
            StageTemplateUsage[template.Id] = usage;
        }

        // Load active job stages with filters
        var jobStageQuery = _context.JobStages
            .Include(js => js.Job)
            .Include(js => js.Dependencies)
                .ThenInclude(d => d.RequiredStage)
            .Include(js => js.Dependents)
                .ThenInclude(d => d.DependentStage)
            .Include(js => js.StageNotes)
            .Where(js => js.Status != "Completed" && js.Status != "Cancelled")
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(SelectedDepartment))
        {
            jobStageQuery = jobStageQuery.Where(js => js.Department == SelectedDepartment);
        }

        if (!string.IsNullOrEmpty(SelectedStatus))
        {
            jobStageQuery = jobStageQuery.Where(js => js.Status == SelectedStatus);
        }

        if (StartDate.HasValue)
        {
            jobStageQuery = jobStageQuery.Where(js => js.ScheduledStart >= StartDate.Value);
        }

        if (EndDate.HasValue)
        {
            jobStageQuery = jobStageQuery.Where(js => js.ScheduledEnd <= EndDate.Value);
        }

        ActiveJobStages = await jobStageQuery
            .OrderBy(js => js.JobId)
            .ThenBy(js => js.ExecutionOrder)
            .ToListAsync();

        // Load stage dependencies
        StageDependencies = await _context.StageDependencies
            .Include(sd => sd.DependentStage)
            .Include(sd => sd.RequiredStage)
            .Where(sd => ActiveJobStages.Select(js => js.Id).Contains(sd.DependentStageId) ||
                        ActiveJobStages.Select(js => js.Id).Contains(sd.RequiredStageId))
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

    private async Task UpdateDependentStagesAsync(int completedStageId)
    {
        var dependentStages = await _context.StageDependencies
            .Include(sd => sd.DependentStage)
            .Where(sd => sd.RequiredStageId == completedStageId)
            .Select(sd => sd.DependentStage)
            .ToListAsync();

        foreach (var stage in dependentStages)
        {
            if (stage.Status == "Scheduled")
            {
                // Check if all dependencies are met
                var allDependencies = await _context.StageDependencies
                    .Include(sd => sd.RequiredStage)
                    .Where(sd => sd.DependentStageId == stage.Id && sd.IsMandatory)
                    .ToListAsync();

                var allMet = allDependencies.All(dep => dep.RequiredStage.Status == "Completed");
                
                if (allMet)
                {
                    stage.CanStart = true;
                    stage.Status = "Ready";
                    stage.LastModifiedDate = DateTime.UtcNow;
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IActionResult> OnGetStageTemplateDetailsAsync(int stageId)
    {
        try
        {
            var stage = await _context.ProductionStages.FindAsync(stageId);
            if (stage == null)
            {
                return NotFound("Production stage template not found");
            }

            var customFields = stage.GetCustomFields();
            var assignedMachines = stage.GetAssignedMachineIds();

            return new JsonResult(new
            {
                id = stage.Id,
                name = stage.Name,
                description = stage.Description,
                department = stage.Department,
                defaultDurationHours = stage.DefaultDurationHours,
                defaultSetupMinutes = stage.DefaultSetupMinutes,
                defaultHourlyRate = stage.DefaultHourlyRate,
                defaultMaterialCost = stage.DefaultMaterialCost,
                requiresQualityCheck = stage.RequiresQualityCheck,
                requiresApproval = stage.RequiresApproval,
                allowSkip = stage.AllowSkip,
                isOptional = stage.IsOptional,
                allowParallelExecution = stage.AllowParallelExecution,
                requiredRole = stage.RequiredRole,
                stageColor = stage.StageColor,
                stageIcon = stage.StageIcon,
                requiresMachineAssignment = stage.RequiresMachineAssignment,
                assignedMachineIds = assignedMachines,
                defaultMachineId = stage.DefaultMachineId,
                customFields = customFields,
                totalEstimatedCost = stage.GetTotalEstimatedCost()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stage template details for {StageId}", stageId);
            return BadRequest("Error loading stage template details");
        }
    }

    #endregion
}

#region View Models

public class CreateStageFromTemplateViewModel
{
    [Required(ErrorMessage = "Please select a production stage template.")]
    public int ProductionStageId { get; set; }

    [Required(ErrorMessage = "Please select a job.")]
    public int JobId { get; set; }

    public int? ExecutionOrder { get; set; }

    [Required(ErrorMessage = "Please specify the scheduled start time.")]
    public DateTime ScheduledStart { get; set; } = DateTime.Now.AddHours(1);

    public string? MachineId { get; set; }

    [Range(1, 5, ErrorMessage = "Priority must be between 1 and 5.")]
    public int Priority { get; set; } = 3;

    [StringLength(100, ErrorMessage = "Assigned operator name cannot exceed 100 characters.")]
    public string? AssignedOperator { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters.")]
    public string? Notes { get; set; }
}

public class CreateJobStageViewModel
{
    [Required]
    public int JobId { get; set; }

    [Required]
    [StringLength(50)]
    public string StageType { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string StageName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Department { get; set; } = string.Empty;

    [StringLength(50)]
    public string? MachineId { get; set; }

    [Range(1, 100)]
    public int ExecutionOrder { get; set; } = 1;

    [Required]
    public DateTime ScheduledStart { get; set; } = DateTime.Now;

    [Range(0.1, 168)]
    public double EstimatedDurationHours { get; set; } = 1.0;

    [Range(1, 5)]
    public int Priority { get; set; } = 3;

    [StringLength(100)]
    public string? AssignedOperator { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public bool IsBlocking { get; set; } = true;
    public bool AllowParallel { get; set; } = false;
}

public class UpdateProductionStageViewModel
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Department { get; set; }

    public double DefaultDurationHours { get; set; } = 1.0;
    public int DefaultSetupMinutes { get; set; } = 30;
    public decimal DefaultHourlyRate { get; set; } = 85.00m;
    public decimal DefaultMaterialCost { get; set; } = 0.00m;
    public bool RequiresQualityCheck { get; set; } = true;
    public bool RequiresApproval { get; set; } = false;
    public bool AllowSkip { get; set; } = false;
    public bool IsOptional { get; set; } = false;
    public bool AllowParallelExecution { get; set; } = false;

    [StringLength(50)]
    public string? RequiredRole { get; set; }
}

public class StageUsageStats
{
    public int PartUsageCount { get; set; }
    public int PrototypeUsageCount { get; set; }
    public int CompletedExecutions { get; set; }
    public double AverageActualHours { get; set; }
    public int TotalUsageCount => PartUsageCount + PrototypeUsageCount + CompletedExecutions;
}

#endregion#region