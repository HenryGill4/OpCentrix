using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services
{
    /// <summary>
    /// Service interface for Stage Template management
    /// Phase 6: Advanced Stage Management & Templates
    /// </summary>
    public interface IStageTemplateService
    {
        // Template CRUD operations
        Task<List<StageTemplate>> GetActiveTemplatesAsync();
        Task<List<StageTemplate>> GetTemplatesByCategoryAsync(int categoryId);
        Task<List<StageTemplate>> GetTemplatesForPartAsync(string industry, string materialType, string complexityLevel);
        Task<StageTemplate?> GetTemplateByIdAsync(int templateId);
        Task<StageTemplate> CreateTemplateAsync(StageTemplate template);
        Task<StageTemplate> UpdateTemplateAsync(StageTemplate template);
        Task<bool> DeleteTemplateAsync(int templateId);
        
        // Template step management
        Task<List<StageTemplateStep>> GetTemplateStepsAsync(int templateId);
        Task<StageTemplateStep> AddTemplateStepAsync(StageTemplateStep step);
        Task<StageTemplateStep> UpdateTemplateStepAsync(StageTemplateStep step);
        Task<bool> RemoveTemplateStepAsync(int stepId);
        Task<bool> ReorderTemplateStepsAsync(int templateId, List<int> stepIds);
        
        // Template application
        Task<List<PartStageRequirement>> ApplyTemplateToPartAsync(int partId, int templateId, string appliedBy);
        Task<bool> ValidateTemplateForPartAsync(int templateId, Part part);
        Task<decimal> CalculateTemplateCostAsync(int templateId, Part part);
        
        // Template suggestions
        Task<List<StageTemplate>> SuggestTemplatesForPartAsync(Part part);
        Task<StageTemplate> CreateTemplateFromPartAsync(int partId, string templateName, string createdBy);
        
        // Category management
        Task<List<StageTemplateCategory>> GetActiveCategoriesAsync();
        Task<StageTemplateCategory> CreateCategoryAsync(StageTemplateCategory category);
        
        // Analytics and reporting
        Task<Dictionary<string, int>> GetTemplateUsageStatisticsAsync();
        Task<List<StageTemplate>> GetMostUsedTemplatesAsync(int count = 10);
        Task<bool> UpdateTemplateUsageAsync(int templateId);
    }
    
    /// <summary>
    /// Implementation of Stage Template management service
    /// Provides comprehensive template creation, application, and management
    /// </summary>
    public class StageTemplateService : IStageTemplateService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<StageTemplateService> _logger;
        private readonly IPartStageService _partStageService;
        
        public StageTemplateService(
            SchedulerContext context, 
            ILogger<StageTemplateService> logger,
            IPartStageService partStageService)
        {
            _context = context;
            _logger = logger;
            _partStageService = partStageService;
        }
        
        public async Task<List<StageTemplate>> GetActiveTemplatesAsync()
        {
            try
            {
                return await _context.StageTemplates
                    .Where(st => st.IsActive)
                    .Include(st => st.TemplateSteps)
                        .ThenInclude(ts => ts.ProductionStage)
                    .OrderBy(st => st.SortOrder)
                    .ThenBy(st => st.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active stage templates");
                return new List<StageTemplate>();
            }
        }
        
        public async Task<List<StageTemplate>> GetTemplatesByCategoryAsync(int categoryId)
        {
            try
            {
                return await _context.StageTemplates
                    .Where(st => st.IsActive)
                    .Include(st => st.TemplateSteps)
                        .ThenInclude(ts => ts.ProductionStage)
                    .OrderBy(st => st.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving templates for category {CategoryId}", categoryId);
                return new List<StageTemplate>();
            }
        }
        
        public async Task<List<StageTemplate>> GetTemplatesForPartAsync(string industry, string materialType, string complexityLevel)
        {
            try
            {
                var query = _context.StageTemplates
                    .Where(st => st.IsActive);
                
                if (!string.IsNullOrEmpty(industry))
                    query = query.Where(st => st.Industry == industry || st.Industry == "General");
                    
                if (!string.IsNullOrEmpty(materialType))
                    query = query.Where(st => st.MaterialType == materialType || st.MaterialType == "General");
                    
                if (!string.IsNullOrEmpty(complexityLevel))
                    query = query.Where(st => st.ComplexityLevel == complexityLevel || st.ComplexityLevel == "General");
                
                return await query
                    .Include(st => st.TemplateSteps)
                        .ThenInclude(ts => ts.ProductionStage)
                    .OrderByDescending(st => st.UsageCount)
                    .ThenBy(st => st.SortOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving templates for part criteria");
                return new List<StageTemplate>();
            }
        }
        
        public async Task<StageTemplate?> GetTemplateByIdAsync(int templateId)
        {
            try
            {
                return await _context.StageTemplates
                    .Include(st => st.TemplateSteps)
                        .ThenInclude(ts => ts.ProductionStage)
                    .FirstOrDefaultAsync(st => st.Id == templateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stage template {TemplateId}", templateId);
                return null;
            }
        }
        
        public async Task<StageTemplate> CreateTemplateAsync(StageTemplate template)
        {
            try
            {
                template.CreatedDate = DateTime.UtcNow;
                template.LastModifiedDate = DateTime.UtcNow;
                
                _context.StageTemplates.Add(template);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Stage template created: {TemplateName} (ID: {TemplateId})", 
                    template.Name, template.Id);
                
                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stage template: {TemplateName}", template.Name);
                throw;
            }
        }
        
        public async Task<StageTemplate> UpdateTemplateAsync(StageTemplate template)
        {
            try
            {
                var existingTemplate = await _context.StageTemplates.FindAsync(template.Id);
                if (existingTemplate == null)
                {
                    throw new InvalidOperationException($"Stage template {template.Id} not found");
                }
                
                existingTemplate.Name = template.Name;
                existingTemplate.Description = template.Description;
                existingTemplate.Industry = template.Industry;
                existingTemplate.MaterialType = template.MaterialType;
                existingTemplate.ComplexityLevel = template.ComplexityLevel;
                existingTemplate.IsActive = template.IsActive;
                existingTemplate.IsDefault = template.IsDefault;
                existingTemplate.SortOrder = template.SortOrder;
                existingTemplate.TemplateConfiguration = template.TemplateConfiguration;
                existingTemplate.EstimatedTotalHours = template.EstimatedTotalHours;
                existingTemplate.EstimatedTotalCost = template.EstimatedTotalCost;
                existingTemplate.LastModifiedBy = template.LastModifiedBy;
                existingTemplate.LastModifiedDate = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Stage template updated: {TemplateId}", template.Id);
                return existingTemplate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stage template {TemplateId}", template.Id);
                throw;
            }
        }
        
        public async Task<bool> DeleteTemplateAsync(int templateId)
        {
            try
            {
                var template = await _context.StageTemplates.FindAsync(templateId);
                if (template == null)
                {
                    return false;
                }
                
                // Soft delete - mark as inactive
                template.IsActive = false;
                template.LastModifiedDate = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Stage template deleted: {TemplateId}", templateId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting stage template {TemplateId}", templateId);
                return false;
            }
        }
        
        public async Task<List<StageTemplateStep>> GetTemplateStepsAsync(int templateId)
        {
            try
            {
                return await _context.StageTemplateSteps
                    .Where(sts => sts.StageTemplateId == templateId)
                    .Include(sts => sts.ProductionStage)
                    .OrderBy(sts => sts.ExecutionOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving template steps for template {TemplateId}", templateId);
                return new List<StageTemplateStep>();
            }
        }
        
        public async Task<StageTemplateStep> AddTemplateStepAsync(StageTemplateStep step)
        {
            try
            {
                _context.StageTemplateSteps.Add(step);
                await _context.SaveChangesAsync();
                
                // Update template totals
                await UpdateTemplateTotalsAsync(step.StageTemplateId);
                
                _logger.LogInformation("Template step added: Template {TemplateId}, Stage {StageId}", 
                    step.StageTemplateId, step.ProductionStageId);
                
                return step;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding template step");
                throw;
            }
        }
        
        public async Task<StageTemplateStep> UpdateTemplateStepAsync(StageTemplateStep step)
        {
            try
            {
                var existingStep = await _context.StageTemplateSteps.FindAsync(step.Id);
                if (existingStep == null)
                {
                    throw new InvalidOperationException($"Template step {step.Id} not found");
                }
                
                existingStep.ExecutionOrder = step.ExecutionOrder;
                existingStep.EstimatedHours = step.EstimatedHours;
                existingStep.HourlyRate = step.HourlyRate;
                existingStep.MaterialCost = step.MaterialCost;
                existingStep.SetupTimeMinutes = step.SetupTimeMinutes;
                existingStep.TeardownTimeMinutes = step.TeardownTimeMinutes;
                existingStep.IsRequired = step.IsRequired;
                existingStep.IsParallel = step.IsParallel;
                existingStep.StageConfiguration = step.StageConfiguration;
                existingStep.QualityRequirements = step.QualityRequirements;
                existingStep.SpecialInstructions = step.SpecialInstructions;
                
                await _context.SaveChangesAsync();
                
                // Update template totals
                await UpdateTemplateTotalsAsync(step.StageTemplateId);
                
                _logger.LogInformation("Template step updated: {StepId}", step.Id);
                return existingStep;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating template step {StepId}", step.Id);
                throw;
            }
        }
        
        public async Task<bool> RemoveTemplateStepAsync(int stepId)
        {
            try
            {
                var step = await _context.StageTemplateSteps.FindAsync(stepId);
                if (step == null)
                {
                    return false;
                }
                
                var templateId = step.StageTemplateId;
                
                _context.StageTemplateSteps.Remove(step);
                await _context.SaveChangesAsync();
                
                // Update template totals
                await UpdateTemplateTotalsAsync(templateId);
                
                _logger.LogInformation("Template step removed: {StepId}", stepId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing template step {StepId}", stepId);
                return false;
            }
        }
        
        public async Task<bool> ReorderTemplateStepsAsync(int templateId, List<int> stepIds)
        {
            try
            {
                for (int i = 0; i < stepIds.Count; i++)
                {
                    var step = await _context.StageTemplateSteps.FindAsync(stepIds[i]);
                    if (step != null && step.StageTemplateId == templateId)
                    {
                        step.ExecutionOrder = i + 1;
                    }
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Template steps reordered for template {TemplateId}", templateId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering template steps for template {TemplateId}", templateId);
                return false;
            }
        }
        
        public async Task<List<PartStageRequirement>> ApplyTemplateToPartAsync(int partId, int templateId, string appliedBy)
        {
            try
            {
                var template = await GetTemplateByIdAsync(templateId);
                if (template == null)
                {
                    throw new InvalidOperationException($"Template {templateId} not found");
                }
                
                // Remove existing stage requirements
                await _partStageService.RemoveAllPartStagesAsync(partId);
                
                var newStageRequirements = new List<PartStageRequirement>();
                
                foreach (var step in template.TemplateSteps.OrderBy(ts => ts.ExecutionOrder))
                {
                    var stageRequirement = new PartStageRequirement
                    {
                        PartId = partId,
                        ProductionStageId = step.ProductionStageId,
                        ExecutionOrder = step.ExecutionOrder,
                        EstimatedHours = step.EstimatedHours,
                        HourlyRateOverride = step.HourlyRate,
                        MaterialCost = step.MaterialCost,
                        SetupTimeMinutes = step.SetupTimeMinutes,
                        IsRequired = step.IsRequired,
                        IsActive = true,
                        CreatedBy = appliedBy,
                        LastModifiedBy = appliedBy,
                        // Copy template configuration
                        StageParameters = step.StageConfiguration,
                        QualityRequirements = step.QualityRequirements,
                        SpecialInstructions = step.SpecialInstructions
                    };
                    
                    var success = await _partStageService.AddPartStageAsync(stageRequirement);
                    if (success)
                    {
                        newStageRequirements.Add(stageRequirement);
                    }
                }
                
                // Update template usage statistics
                await UpdateTemplateUsageAsync(templateId);
                
                _logger.LogInformation("Template {TemplateId} applied to part {PartId} with {StageCount} stages", 
                    templateId, partId, newStageRequirements.Count);
                
                return newStageRequirements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying template {TemplateId} to part {PartId}", templateId, partId);
                throw;
            }
        }
        
        public async Task<bool> ValidateTemplateForPartAsync(int templateId, Part part)
        {
            try
            {
                var template = await GetTemplateByIdAsync(templateId);
                if (template == null)
                {
                    return false;
                }
                
                // Basic compatibility checks
                if (template.Industry != "General" && template.Industry != part.Industry)
                {
                    return false;
                }
                
                // Add more validation logic as needed
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating template {TemplateId} for part", templateId);
                return false;
            }
        }
        
        public async Task<decimal> CalculateTemplateCostAsync(int templateId, Part part)
        {
            try
            {
                var template = await GetTemplateByIdAsync(templateId);
                if (template == null)
                {
                    return 0;
                }
                
                decimal totalCost = 0;
                
                foreach (var step in template.TemplateSteps)
                {
                    var laborCost = (decimal)step.EstimatedHours * step.HourlyRate;
                    var setupCost = (decimal)(step.SetupTimeMinutes / 60.0) * step.HourlyRate;
                    var teardownCost = (decimal)(step.TeardownTimeMinutes / 60.0) * step.HourlyRate;
                    var materialCost = step.MaterialCost;
                    
                    totalCost += laborCost + setupCost + teardownCost + materialCost;
                }
                
                return totalCost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating template cost for template {TemplateId}", templateId);
                return 0;
            }
        }
        
        public async Task<List<StageTemplate>> SuggestTemplatesForPartAsync(Part part)
        {
            try
            {
                // Get templates that match part characteristics
                var suggestions = await GetTemplatesForPartAsync(
                    part.Industry ?? "General",
                    part.Material ?? "Metal",
                    GetComplexityLevel(part.EstimatedHours)
                );
                
                // Return top 5 suggestions
                return suggestions.Take(5).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template suggestions for part");
                return new List<StageTemplate>();
            }
        }
        
        public async Task<StageTemplate> CreateTemplateFromPartAsync(int partId, string templateName, string createdBy)
        {
            try
            {
                var part = await _context.Parts.FindAsync(partId);
                if (part == null)
                {
                    throw new InvalidOperationException($"Part {partId} not found");
                }
                
                var partStages = await _partStageService.GetPartStagesWithDetailsAsync(partId);
                
                var template = new StageTemplate
                {
                    Name = templateName,
                    Description = $"Template created from part {part.PartNumber}",
                    Industry = part.Industry ?? "General",
                    MaterialType = part.Material ?? "Metal",
                    ComplexityLevel = GetComplexityLevel(part.EstimatedHours),
                    EstimatedTotalHours = (decimal)partStages.Sum(ps => ps.EstimatedHours ?? 0),
                    EstimatedTotalCost = partStages.Sum(ps => ps.CalculateTotalEstimatedCost()),
                    CreatedBy = createdBy,
                    LastModifiedBy = createdBy
                };
                
                var createdTemplate = await CreateTemplateAsync(template);
                
                // Add template steps
                foreach (var partStage in partStages.OrderBy(ps => ps.ExecutionOrder))
                {
                    var step = new StageTemplateStep
                    {
                        StageTemplateId = createdTemplate.Id,
                        ProductionStageId = partStage.ProductionStageId,
                        ExecutionOrder = partStage.ExecutionOrder,
                        EstimatedHours = partStage.EstimatedHours ?? 1.0,
                        HourlyRate = partStage.HourlyRateOverride ?? 85.00m,
                        MaterialCost = partStage.MaterialCost,
                        SetupTimeMinutes = partStage.SetupTimeMinutes ?? 30,
                        TeardownTimeMinutes = 0,
                        IsRequired = partStage.IsRequired,
                        StageConfiguration = partStage.StageParameters ?? "{}",
                        QualityRequirements = partStage.QualityRequirements ?? "",
                        SpecialInstructions = partStage.SpecialInstructions ?? ""
                    };
                    
                    await AddTemplateStepAsync(step);
                }
                
                _logger.LogInformation("Template created from part {PartId}: {TemplateName}", 
                    partId, templateName);
                
                return createdTemplate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating template from part {PartId}", partId);
                throw;
            }
        }
        
        public async Task<List<StageTemplateCategory>> GetActiveCategoriesAsync()
        {
            try
            {
                return await _context.StageTemplateCategories
                    .Where(stc => stc.IsActive)
                    .OrderBy(stc => stc.SortOrder)
                    .ThenBy(stc => stc.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving template categories");
                return new List<StageTemplateCategory>();
            }
        }
        
        public async Task<StageTemplateCategory> CreateCategoryAsync(StageTemplateCategory category)
        {
            try
            {
                _context.StageTemplateCategories.Add(category);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Template category created: {CategoryName}", category.Name);
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating template category: {CategoryName}", category.Name);
                throw;
            }
        }
        
        public async Task<Dictionary<string, int>> GetTemplateUsageStatisticsAsync()
        {
            try
            {
                return await _context.StageTemplates
                    .Where(st => st.IsActive)
                    .GroupBy(st => st.Industry)
                    .ToDictionaryAsync(g => g.Key, g => g.Sum(st => st.UsageCount));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving template usage statistics");
                return new Dictionary<string, int>();
            }
        }
        
        public async Task<List<StageTemplate>> GetMostUsedTemplatesAsync(int count = 10)
        {
            try
            {
                return await _context.StageTemplates
                    .Where(st => st.IsActive)
                    .OrderByDescending(st => st.UsageCount)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving most used templates");
                return new List<StageTemplate>();
            }
        }
        
        public async Task<bool> UpdateTemplateUsageAsync(int templateId)
        {
            try
            {
                var template = await _context.StageTemplates.FindAsync(templateId);
                if (template == null)
                {
                    return false;
                }
                
                template.UsageCount++;
                template.LastUsedDate = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating template usage for template {TemplateId}", templateId);
                return false;
            }
        }
        
        private async Task UpdateTemplateTotalsAsync(int templateId)
        {
            try
            {
                var template = await _context.StageTemplates
                    .Include(st => st.TemplateSteps)
                    .FirstOrDefaultAsync(st => st.Id == templateId);
                    
                if (template != null)
                {
                    template.EstimatedTotalHours = (decimal)template.TemplateSteps.Sum(ts => ts.EstimatedHours);
                    template.EstimatedTotalCost = template.TemplateSteps.Sum(ts => 
                        (decimal)ts.EstimatedHours * ts.HourlyRate + 
                        ts.MaterialCost +
                        (decimal)(ts.SetupTimeMinutes / 60.0) * ts.HourlyRate +
                        (decimal)(ts.TeardownTimeMinutes / 60.0) * ts.HourlyRate);
                    
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating template totals for template {TemplateId}", templateId);
            }
        }
        
        private string GetComplexityLevel(double estimatedHours)
        {
            return estimatedHours switch
            {
                <= 4 => "Simple",
                <= 12 => "Medium", 
                <= 24 => "Complex",
                _ => "VeryComplex"
            };
        }
    }
}