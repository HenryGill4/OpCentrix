using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using System.Text.Json;

namespace OpCentrix.Pages.Admin.Workflows
{
    /// <summary>
    /// Advanced Stage Management page for complex manufacturing workflows
    /// Provides management for workflow templates, dependencies, and resource allocation
    /// This is the enhanced stage management system referenced in your workflow plan
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class StageManagementModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<StageManagementModel> _logger;

        public StageManagementModel(SchedulerContext context, ILogger<StageManagementModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Data properties for the page - using full namespace to avoid conflicts
        public List<ProductionStage> ProductionStages { get; set; } = new List<ProductionStage>();
        public List<WorkflowTemplate> WorkflowTemplates { get; set; } = new List<WorkflowTemplate>();
        public List<OpCentrix.Models.StageDependency> StageDependencies { get; set; } = new List<OpCentrix.Models.StageDependency>();
        public List<ResourcePool> ResourcePools { get; set; } = new List<ResourcePool>();
        public List<Machine> AvailableMachines { get; set; } = new List<Machine>();
        public Dictionary<int, WorkflowStats> WorkflowStatistics { get; set; } = new Dictionary<int, WorkflowStats>();

        // Form binding properties - using full namespace to avoid conflicts
        [BindProperty]
        public WorkflowTemplate NewWorkflowTemplate { get; set; } = new WorkflowTemplate();

        [BindProperty]
        public OpCentrix.Models.StageDependency NewStageDependency { get; set; } = new OpCentrix.Models.StageDependency();

        [BindProperty]
        public ResourcePool NewResourcePool { get; set; } = new ResourcePool();

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading Advanced Stage Management page");

                await LoadStageManagementDataAsync();
                await LoadWorkflowStatisticsAsync();

                _logger.LogInformation("Loaded {StageCount} stages, {TemplateCount} templates, {DependencyCount} dependencies",
                    ProductionStages.Count, WorkflowTemplates.Count, StageDependencies.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stage management data");
                TempData["ErrorMessage"] = "Error loading stage management data. Please try again.";
            }
        }

        #region Workflow Template Management

        public async Task<IActionResult> OnPostCreateWorkflowTemplateAsync()
        {
            try
            {
                _logger.LogInformation("Creating workflow template: {TemplateName}", NewWorkflowTemplate?.Name ?? "null");

                if (NewWorkflowTemplate == null)
                {
                    TempData["ErrorMessage"] = "Invalid template data received.";
                    return RedirectToPage();
                }

                // Validate template
                if (string.IsNullOrWhiteSpace(NewWorkflowTemplate.Name))
                {
                    TempData["ErrorMessage"] = "Template name is required.";
                    return RedirectToPage();
                }

                // Check for duplicate names
                var existingTemplate = await _context.WorkflowTemplates
                    .FirstOrDefaultAsync(wt => wt.Name.ToLower() == NewWorkflowTemplate.Name.ToLower() && wt.IsActive);

                if (existingTemplate != null)
                {
                    TempData["ErrorMessage"] = $"A workflow template with the name '{NewWorkflowTemplate.Name}' already exists.";
                    return RedirectToPage();
                }

                // Set template properties
                NewWorkflowTemplate.CreatedDate = DateTime.UtcNow;
                NewWorkflowTemplate.CreatedBy = User.Identity?.Name ?? "System";
                NewWorkflowTemplate.LastModifiedBy = User.Identity?.Name ?? "System";
                NewWorkflowTemplate.IsActive = true;

                // Default template configuration if not provided
                if (string.IsNullOrEmpty(NewWorkflowTemplate.StageConfiguration))
                {
                    NewWorkflowTemplate.StageConfiguration = "[]";
                }

                _context.WorkflowTemplates.Add(NewWorkflowTemplate);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created workflow template: {TemplateName} with ID: {TemplateId}",
                    NewWorkflowTemplate.Name, NewWorkflowTemplate.Id);
                TempData["SuccessMessage"] = $"Workflow template '{NewWorkflowTemplate.Name}' created successfully.";

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workflow template: {TemplateName}", NewWorkflowTemplate?.Name ?? "null");
                TempData["ErrorMessage"] = $"Error creating workflow template: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteWorkflowTemplateAsync(int templateId)
        {
            try
            {
                _logger.LogInformation("Attempting to delete workflow template with ID: {TemplateId}", templateId);

                var template = await _context.WorkflowTemplates.FindAsync(templateId);
                if (template == null)
                {
                    TempData["ErrorMessage"] = "Workflow template not found.";
                    return RedirectToPage();
                }

                // Check if template is being used
                var usageCount = await _context.PartStageRequirements
                    .CountAsync(psr => psr.WorkflowTemplateId == templateId && psr.IsActive);

                if (usageCount > 0)
                {
                    _logger.LogWarning("Cannot delete template {TemplateId} - it's used by {UsageCount} parts",
                        templateId, usageCount);
                    TempData["ErrorMessage"] = $"Cannot delete '{template.Name}' - it's currently used by {usageCount} parts. Please remove those references first.";
                    return RedirectToPage();
                }

                // Soft delete
                template.IsActive = false;
                template.LastModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted workflow template: {TemplateName}", template.Name);
                TempData["SuccessMessage"] = $"Workflow template '{template.Name}' deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow template {TemplateId}", templateId);
                TempData["ErrorMessage"] = "Error deleting workflow template. Please try again.";
            }

            return RedirectToPage();
        }

        #endregion

        #region Stage Dependency Management

        public async Task<IActionResult> OnPostCreateStageDependencyAsync()
        {
            try
            {
                _logger.LogInformation("Creating stage dependency: {DependentStage} depends on {PrerequisiteStage}",
                    NewStageDependency?.DependentStageId, NewStageDependency?.PrerequisiteStageId);

                if (NewStageDependency == null)
                {
                    TempData["ErrorMessage"] = "Invalid dependency data received.";
                    return RedirectToPage();
                }

                // Validate dependency
                if (NewStageDependency.DependentStageId == NewStageDependency.PrerequisiteStageId)
                {
                    TempData["ErrorMessage"] = "A stage cannot depend on itself.";
                    return RedirectToPage();
                }

                // Check for duplicate dependencies - use explicit table reference
                var existingDependency = await _context.Set<OpCentrix.Models.StageDependency>()
                    .FirstOrDefaultAsync(sd => sd.DependentStageId == NewStageDependency.DependentStageId &&
                                              sd.PrerequisiteStageId == NewStageDependency.PrerequisiteStageId &&
                                              sd.IsActive);

                if (existingDependency != null)
                {
                    TempData["ErrorMessage"] = "This dependency already exists.";
                    return RedirectToPage();
                }

                // Check for circular dependencies
                if (await WouldCreateCircularDependencyAsync(NewStageDependency.DependentStageId, NewStageDependency.PrerequisiteStageId))
                {
                    TempData["ErrorMessage"] = "This dependency would create a circular reference.";
                    return RedirectToPage();
                }

                // Set dependency properties
                NewStageDependency.CreatedDate = DateTime.UtcNow;
                NewStageDependency.CreatedBy = User.Identity?.Name ?? "System";
                NewStageDependency.LastModifiedBy = User.Identity?.Name ?? "System";
                NewStageDependency.IsActive = true;

                _context.Set<OpCentrix.Models.StageDependency>().Add(NewStageDependency);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created stage dependency with ID: {DependencyId}", NewStageDependency.Id);
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

        public async Task<IActionResult> OnPostDeleteStageDependencyAsync(int dependencyId)
        {
            try
            {
                _logger.LogInformation("Attempting to delete stage dependency with ID: {DependencyId}", dependencyId);

                var dependency = await _context.Set<OpCentrix.Models.StageDependency>().FindAsync(dependencyId);
                if (dependency == null)
                {
                    TempData["ErrorMessage"] = "Stage dependency not found.";
                    return RedirectToPage();
                }

                // Soft delete
                dependency.IsActive = false;
                dependency.LastModifiedBy = User.Identity?.Name ?? "System";

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

        #region Resource Pool Management

        public async Task<IActionResult> OnPostCreateResourcePoolAsync()
        {
            try
            {
                _logger.LogInformation("Creating resource pool: {PoolName}", NewResourcePool?.Name ?? "null");

                if (NewResourcePool == null)
                {
                    TempData["ErrorMessage"] = "Invalid resource pool data received.";
                    return RedirectToPage();
                }

                // Validate resource pool
                if (string.IsNullOrWhiteSpace(NewResourcePool.Name))
                {
                    TempData["ErrorMessage"] = "Resource pool name is required.";
                    return RedirectToPage();
                }

                // Check for duplicate names
                var existingPool = await _context.ResourcePools
                    .FirstOrDefaultAsync(rp => rp.Name.ToLower() == NewResourcePool.Name.ToLower() && rp.IsActive);

                if (existingPool != null)
                {
                    TempData["ErrorMessage"] = $"A resource pool with the name '{NewResourcePool.Name}' already exists.";
                    return RedirectToPage();
                }

                // Set resource pool properties
                NewResourcePool.CreatedDate = DateTime.UtcNow;
                NewResourcePool.CreatedBy = User.Identity?.Name ?? "System";
                NewResourcePool.LastModifiedBy = User.Identity?.Name ?? "System";
                NewResourcePool.IsActive = true;

                if (string.IsNullOrEmpty(NewResourcePool.ResourceConfiguration))
                {
                    NewResourcePool.ResourceConfiguration = "[]";
                }

                _context.ResourcePools.Add(NewResourcePool);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created resource pool: {PoolName} with ID: {PoolId}",
                    NewResourcePool.Name, NewResourcePool.Id);
                TempData["SuccessMessage"] = $"Resource pool '{NewResourcePool.Name}' created successfully.";

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating resource pool: {PoolName}", NewResourcePool?.Name ?? "null");
                TempData["ErrorMessage"] = $"Error creating resource pool: {ex.Message}";
                return RedirectToPage();
            }
        }

        #endregion

        #region Helper Methods

        private async Task LoadStageManagementDataAsync()
        {
            // Load production stages
            ProductionStages = await _context.ProductionStages
                .Where(ps => ps.IsActive)
                .OrderBy(ps => ps.DisplayOrder)
                .ToListAsync();

            // Load workflow templates
            WorkflowTemplates = await _context.WorkflowTemplates
                .Where(wt => wt.IsActive)
                .OrderBy(wt => wt.Name)
                .ToListAsync();

            // Load stage dependencies - use explicit table reference to avoid conflicts
            StageDependencies = await _context.Set<OpCentrix.Models.StageDependency>()
                .Include(sd => sd.DependentStage)
                .Include(sd => sd.PrerequisiteStage)
                .Where(sd => sd.IsActive)
                .OrderBy(sd => sd.DependentStage.DisplayOrder)
                .ToListAsync();

            // Load resource pools
            ResourcePools = await _context.ResourcePools
                .Where(rp => rp.IsActive)
                .OrderBy(rp => rp.Name)
                .ToListAsync();

            // Load available machines
            AvailableMachines = await _context.Machines
                .Where(m => m.IsActive)
                .OrderBy(m => m.MachineId)
                .ToListAsync();
        }

        private async Task LoadWorkflowStatisticsAsync()
        {
            try
            {
                foreach (var template in WorkflowTemplates)
                {
                    var usageCount = await _context.PartStageRequirements
                        .CountAsync(psr => psr.WorkflowTemplateId == template.Id && psr.IsActive);

                    var completedJobs = await _context.ProductionStageExecutions
                        .Where(pse => pse.WorkflowTemplateId == template.Id && pse.Status == "Completed")
                        .ToListAsync();

                    var avgDuration = completedJobs.Any()
                        ? completedJobs.Average(job => job.ActualHours ?? 0)
                        : 0;

                    var avgCost = completedJobs.Any()
                        ? completedJobs.Average(job => job.ActualCost ?? 0)
                        : 0;

                    WorkflowStatistics[template.Id] = new WorkflowStats
                    {
                        UsageCount = usageCount,
                        CompletedJobs = completedJobs.Count,
                        AverageDuration = (decimal)avgDuration,
                        AverageCost = (decimal)avgCost,
                        SuccessRate = completedJobs.Count > 0 ?
                            (decimal)completedJobs.Count(j => j.Status == "Completed") / completedJobs.Count * 100 : 0
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading workflow statistics");
                // Continue without statistics rather than failing completely
            }
        }

        private async Task<bool> WouldCreateCircularDependencyAsync(int dependentStageId, int prerequisiteStageId)
        {
            // Check if prerequisiteStageId eventually depends on dependentStageId
            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(prerequisiteStageId);

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

                // Get all stages that depend on the current stage - use explicit table reference
                var dependentStages = await _context.Set<OpCentrix.Models.StageDependency>()
                    .Where(sd => sd.PrerequisiteStageId == currentStageId && sd.IsActive)
                    .Select(sd => sd.DependentStageId)
                    .ToListAsync();

                foreach (var stage in dependentStages)
                {
                    queue.Enqueue(stage);
                }
            }

            return false;
        }

        public async Task<IActionResult> OnGetWorkflowTemplateDetailsAsync(int templateId)
        {
            try
            {
                var template = await _context.WorkflowTemplates
                    .FirstOrDefaultAsync(wt => wt.Id == templateId);

                if (template == null)
                {
                    return NotFound("Template not found");
                }

                var stats = WorkflowStatistics.GetValueOrDefault(templateId, new WorkflowStats());

                return new JsonResult(new
                {
                    id = template.Id,
                    name = template.Name,
                    description = template.Description ?? "",
                    category = template.Category ?? "",
                    complexity = template.Complexity,
                    estimatedDuration = template.EstimatedDurationHours,
                    estimatedCost = template.EstimatedCost,
                    stageConfiguration = template.StageConfiguration,
                    stats = new
                    {
                        usageCount = stats.UsageCount,
                        completedJobs = stats.CompletedJobs,
                        averageDuration = stats.AverageDuration,
                        averageCost = stats.AverageCost,
                        successRate = stats.SuccessRate
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow template details for {TemplateId}", templateId);
                return BadRequest("Error loading template details");
            }
        }

        #endregion
    }

    /// <summary>
    /// Statistics for workflow templates
    /// </summary>
    public class WorkflowStats
    {
        public int UsageCount { get; set; }
        public int CompletedJobs { get; set; }
        public decimal AverageDuration { get; set; }
        public decimal AverageCost { get; set; }
        public decimal SuccessRate { get; set; }
    }
}