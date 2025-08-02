using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using OpCentrix.Services;
using System.Text.Json;

namespace OpCentrix.Pages.Admin.ProductionStages
{
    /// <summary>
    /// Stage template management page for viewing and applying custom field templates
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class TemplatesModel : PageModel
    {
        private readonly IStageTemplateService _stageTemplateService;
        private readonly ILogger<TemplatesModel> _logger;

        public TemplatesModel(IStageTemplateService stageTemplateService, ILogger<TemplatesModel> logger)
        {
            _stageTemplateService = stageTemplateService;
            _logger = logger;
        }

        public List<ProductionStageTemplate> StageTemplates { get; set; } = new List<ProductionStageTemplate>();
        public Dictionary<string, string> TemplateJson { get; set; } = new Dictionary<string, string>();

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading production stage templates");
                
                StageTemplates = await _stageTemplateService.GetAllStageTemplatesAsync();
                
                // Generate JSON representations for display
                foreach (var template in StageTemplates)
                {
                    var jsonOptions = new JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    TemplateJson[template.Name] = JsonSerializer.Serialize(template.CustomFields, jsonOptions);
                }

                _logger.LogInformation("Loaded {Count} production stage templates", StageTemplates.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading production stage templates");
                TempData["ErrorMessage"] = "Error loading stage templates. Please try again.";
                StageTemplates = new List<ProductionStageTemplate>();
            }
        }

        public async Task<IActionResult> OnGetTemplateCustomFieldsAsync(string stageName)
        {
            try
            {
                var customFields = await _stageTemplateService.GetCustomFieldsForStageAsync(stageName);
                
                return new JsonResult(new
                {
                    success = true,
                    stageName = stageName,
                    customFields = customFields.Select(cf => new
                    {
                        name = cf.Name,
                        type = cf.Type,
                        label = cf.Label,
                        description = cf.Description,
                        required = cf.Required,
                        defaultValue = cf.DefaultValue,
                        options = cf.Options,
                        minValue = cf.MinValue,
                        maxValue = cf.MaxValue,
                        unit = cf.Unit,
                        displayOrder = cf.DisplayOrder,
                        placeholderText = cf.PlaceholderText,
                        isReadOnly = cf.IsReadOnly
                    }).OrderBy(cf => cf.displayOrder).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom fields for stage: {StageName}", stageName);
                return new JsonResult(new
                {
                    success = false,
                    error = "Error loading custom fields for stage"
                });
            }
        }

        public async Task<IActionResult> OnGetAvailableStagesAsync()
        {
            try
            {
                var stageNames = await _stageTemplateService.GetAvailableStageNamesAsync();
                
                return new JsonResult(new
                {
                    success = true,
                    stages = stageNames
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available stage names");
                return new JsonResult(new
                {
                    success = false,
                    error = "Error loading available stages"
                });
            }
        }

        public async Task<IActionResult> OnPostCreateStageFromTemplateAsync(string stageName)
        {
            try
            {
                _logger.LogInformation("Creating production stage from template: {StageName}", stageName);
                
                var stage = await _stageTemplateService.CreateStageFromTemplateAsync(stageName);
                
                // Here you would typically save the stage to the database
                // For now, we'll just return the created stage data
                
                return new JsonResult(new
                {
                    success = true,
                    message = $"Stage '{stageName}' created successfully from template",
                    stage = new
                    {
                        name = stage.Name,
                        description = stage.Description,
                        defaultSetupMinutes = stage.DefaultSetupMinutes,
                        defaultHourlyRate = stage.DefaultHourlyRate,
                        defaultDurationHours = stage.DefaultDurationHours,
                        stageColor = stage.StageColor,
                        stageIcon = stage.StageIcon,
                        department = stage.Department,
                        customFieldsCount = stage.GetCustomFields().Count
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stage from template: {StageName}", stageName);
                return new JsonResult(new
                {
                    success = false,
                    error = $"Error creating stage from template: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Demonstrate how to apply template custom fields to a part stage requirement
        /// </summary>
        public async Task<IActionResult> OnPostApplyTemplateToPartAsync(int partId, string stageName)
        {
            try
            {
                _logger.LogInformation("Applying template {StageName} to part {PartId}", stageName, partId);
                
                // Get the template custom fields
                var customFields = await _stageTemplateService.GetCustomFieldsForStageAsync(stageName);
                
                if (!customFields.Any())
                {
                    return new JsonResult(new
                    {
                        success = false,
                        error = "No custom fields found for the specified stage template"
                    });
                }

                // Create example custom field values based on template defaults
                var customFieldValues = new Dictionary<string, object>();
                
                foreach (var field in customFields)
                {
                    switch (field.Type.ToLower())
                    {
                        case "text":
                            customFieldValues[field.Name] = field.DefaultValue ?? "";
                            break;
                        case "number":
                            if (double.TryParse(field.DefaultValue, out var numValue))
                                customFieldValues[field.Name] = numValue;
                            else
                                customFieldValues[field.Name] = field.MinValue ?? 0;
                            break;
                        case "dropdown":
                            customFieldValues[field.Name] = field.DefaultValue ?? field.Options?.FirstOrDefault() ?? "";
                            break;
                        case "checkbox":
                            customFieldValues[field.Name] = bool.TryParse(field.DefaultValue, out var boolValue) ? boolValue : false;
                            break;
                        case "textarea":
                            customFieldValues[field.Name] = field.DefaultValue ?? "";
                            break;
                        default:
                            customFieldValues[field.Name] = field.DefaultValue ?? "";
                            break;
                    }
                }

                return new JsonResult(new
                {
                    success = true,
                    message = $"Template '{stageName}' applied successfully",
                    partId = partId,
                    stageName = stageName,
                    customFieldsApplied = customFields.Count,
                    sampleCustomFieldValues = customFieldValues,
                    customFieldDefinitions = customFields.Select(cf => new
                    {
                        name = cf.Name,
                        type = cf.Type,
                        label = cf.Label,
                        required = cf.Required,
                        options = cf.Options,
                        unit = cf.Unit
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying template {StageName} to part {PartId}", stageName, partId);
                return new JsonResult(new
                {
                    success = false,
                    error = $"Error applying template: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get a JSON preview of all templates for API consumption
        /// </summary>
        public async Task<IActionResult> OnGetTemplatePreviewAsync()
        {
            try
            {
                var templates = await _stageTemplateService.GetAllStageTemplatesAsync();
                
                var templateData = templates.Select(template => new
                {
                    name = template.Name,
                    displayOrder = template.DisplayOrder,
                    description = template.Description,
                    stageColor = template.StageColor,
                    stageIcon = template.StageIcon,
                    department = template.Department,
                    defaultSetupMinutes = template.DefaultSetupMinutes,
                    defaultHourlyRate = template.DefaultHourlyRate,
                    defaultDurationHours = template.DefaultDurationHours,
                    defaultMaterialCost = template.DefaultMaterialCost,
                    requiresQualityCheck = template.RequiresQualityCheck,
                    requiresApproval = template.RequiresApproval,
                    requiresMachineAssignment = template.RequiresMachineAssignment,
                    allowParallelExecution = template.AllowParallelExecution,
                    customFields = template.CustomFields.Select(cf => new
                    {
                        name = cf.Name,
                        type = cf.Type,
                        label = cf.Label,
                        description = cf.Description,
                        required = cf.Required,
                        defaultValue = cf.DefaultValue,
                        options = cf.Options,
                        minValue = cf.MinValue,
                        maxValue = cf.MaxValue,
                        unit = cf.Unit,
                        displayOrder = cf.DisplayOrder,
                        placeholderText = cf.PlaceholderText
                    }).OrderBy(cf => cf.displayOrder).ToList()
                }).OrderBy(t => t.displayOrder).ToList();

                return new JsonResult(new
                {
                    success = true,
                    totalTemplates = templates.Count,
                    totalCustomFields = templates.Sum(t => t.CustomFields.Count),
                    templates = templateData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template preview");
                return new JsonResult(new
                {
                    success = false,
                    error = "Error loading template preview"
                });
            }
        }
    }
}