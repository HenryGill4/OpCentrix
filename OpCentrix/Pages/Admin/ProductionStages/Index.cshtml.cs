using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using OpCentrix.Services.Admin;

namespace OpCentrix.Pages.Admin.ProductionStages
{
    /// <summary>
    /// Production stage configuration and management page
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class IndexModel : PageModel
    {
        private readonly ProductionStageService _stageService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ProductionStageService stageService, ILogger<IndexModel> logger)
        {
            _stageService = stageService;
            _logger = logger;
        }

        public List<ProductionStage> ProductionStages { get; set; } = new List<ProductionStage>();

        [BindProperty]
        public ProductionStage NewStage { get; set; } = new ProductionStage();

        [BindProperty]
        public string ReorderStageIds { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            try
            {
                ProductionStages = await _stageService.GetAllStagesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading production stages");
                TempData["ErrorMessage"] = "Error loading production stages. Please try again.";
            }
        }

        public async Task<IActionResult> OnPostCreateStageAsync()
        {
            try
            {
                _logger.LogInformation("Create stage request received. Stage name: {StageName}", NewStage?.Name ?? "null");
                
                if (NewStage == null)
                {
                    _logger.LogWarning("NewStage is null");
                    TempData["ErrorMessage"] = "Invalid stage data received.";
                    await OnGetAsync();
                    return Page();
                }

                // Log all the stage properties for debugging
                _logger.LogInformation("Stage properties - Name: {Name}, Description: {Description}, SetupMinutes: {SetupMinutes}, HourlyRate: {HourlyRate}, RequiresQualityCheck: {RequiresQualityCheck}, RequiresApproval: {RequiresApproval}, AllowSkip: {AllowSkip}, IsOptional: {IsOptional}, RequiredRole: {RequiredRole}", 
                    NewStage.Name, NewStage.Description, NewStage.DefaultSetupMinutes, NewStage.DefaultHourlyRate, 
                    NewStage.RequiresQualityCheck, NewStage.RequiresApproval, NewStage.AllowSkip, NewStage.IsOptional, NewStage.RequiredRole);

                // Check model state
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model state is invalid:");
                    foreach (var modelError in ModelState)
                    {
                        foreach (var error in modelError.Value.Errors)
                        {
                            _logger.LogWarning("Model error - Key: {Key}, Error: {Error}", modelError.Key, error.ErrorMessage);
                        }
                    }
                    
                    TempData["ErrorMessage"] = "Please correct the validation errors and try again.";
                    await OnGetAsync();
                    return Page();
                }

                var success = await _stageService.CreateStageAsync(NewStage);
                
                if (success)
                {
                    _logger.LogInformation("Stage created successfully: {StageName}", NewStage.Name);
                    TempData["SuccessMessage"] = $"Production stage '{NewStage.Name}' created successfully.";
                    return RedirectToPage();
                }
                else
                {
                    _logger.LogWarning("Stage creation failed for: {StageName}", NewStage.Name);
                    TempData["ErrorMessage"] = "Failed to create production stage. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating production stage. NewStage: {NewStage}", NewStage?.Name ?? "null");
                TempData["ErrorMessage"] = "Error creating production stage. Please try again.";
            }

            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteStageAsync(int stageId)
        {
            try
            {
                var success = await _stageService.DeleteStageAsync(stageId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Production stage deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete production stage. It may be in use by active prototypes.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting production stage {StageId}", stageId);
                TempData["ErrorMessage"] = "Error deleting production stage. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReorderStagesAsync()
        {
            try
            {
                _logger.LogInformation("Received reorder request with: {ReorderStageIds}", ReorderStageIds);
                
                if (!string.IsNullOrEmpty(ReorderStageIds))
                {
                    // Parse the comma-separated string into a list of integers
                    var stageIds = ReorderStageIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => int.TryParse(id.Trim(), out var result) ? result : 0)
                        .Where(id => id > 0)
                        .ToList();

                    _logger.LogInformation("Parsed stage IDs: {StageIds}", string.Join(", ", stageIds));

                    if (stageIds.Any())
                    {
                        var success = await _stageService.ReorderStagesAsync(stageIds);
                        
                        if (success)
                        {
                            TempData["SuccessMessage"] = $"Production stages reordered successfully. New order: {stageIds.Count} stages updated.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Failed to reorder production stages. Please try again.";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "No valid stage IDs provided for reordering.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "No stage order provided.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering production stages. Input: {ReorderStageIds}", ReorderStageIds);
                TempData["ErrorMessage"] = "Error reordering production stages. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateDefaultStagesAsync()
        {
            try
            {
                var success = await _stageService.CreateDefaultStagesAsync();
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Default production stages created successfully.";
                }
                else
                {
                    TempData["WarningMessage"] = "Default stages already exist or could not be created.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default production stages");
                TempData["ErrorMessage"] = "Error creating default production stages. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetStageDetailsAsync(int stageId)
        {
            try
            {
                var stage = await _stageService.GetStageAsync(stageId);
                if (stage == null)
                {
                    return NotFound();
                }

                return new JsonResult(new
                {
                    id = stage.Id,
                    name = stage.Name,
                    description = stage.Description,
                    defaultSetupMinutes = stage.DefaultSetupMinutes,
                    defaultHourlyRate = stage.DefaultHourlyRate,
                    requiresQualityCheck = stage.RequiresQualityCheck,
                    requiresApproval = stage.RequiresApproval,
                    allowSkip = stage.AllowSkip,
                    isOptional = stage.IsOptional,
                    requiredRole = stage.RequiredRole
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stage details for {StageId}", stageId);
                return BadRequest("Error loading stage details");
            }
        }

        public async Task<IActionResult> OnPostUpdateStageAsync()
        {
            try
            {
                // Check if this is a JSON request (from JavaScript) or form request
                var contentType = Request.ContentType?.ToLower();
                
                if (contentType?.Contains("application/json") == true)
                {
                    // Handle JSON request (from JavaScript)
                    using var reader = new StreamReader(Request.Body);
                    var body = await reader.ReadToEndAsync();
                    var stage = System.Text.Json.JsonSerializer.Deserialize<ProductionStage>(body, new System.Text.Json.JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });

                    if (stage?.Id <= 0)
                    {
                        return BadRequest("Invalid stage ID");
                    }

                    var success = await _stageService.UpdateStageAsync(stage);
                    
                    if (success)
                    {
                        return new JsonResult(new { success = true, message = "Stage updated successfully" });
                    }
                    else
                    {
                        return BadRequest("Failed to update stage");
                    }
                }
                else
                {
                    // Handle form request (from modal)
                    var updatedStage = new ProductionStage
                    {
                        Id = int.Parse(Request.Form["Id"]),
                        Name = Request.Form["Name"],
                        Description = Request.Form["Description"],
                        DefaultSetupMinutes = int.Parse(Request.Form["DefaultSetupMinutes"]),
                        DefaultHourlyRate = decimal.Parse(Request.Form["DefaultHourlyRate"]),
                        RequiresQualityCheck = Request.Form["RequiresQualityCheck"].Contains("true"),
                        RequiresApproval = Request.Form["RequiresApproval"].Contains("true"),
                        AllowSkip = Request.Form["AllowSkip"].Contains("true"),
                        IsOptional = Request.Form["IsOptional"].Contains("true"),
                        RequiredRole = Request.Form["RequiredRole"]
                    };

                    _logger.LogInformation("Update stage request received. Stage ID: {StageId}, Name: {StageName}", updatedStage.Id, updatedStage.Name);
                    
                    if (updatedStage.Id <= 0)
                    {
                        _logger.LogWarning("Invalid stage ID received: {StageId}", updatedStage.Id);
                        TempData["ErrorMessage"] = "Invalid stage ID.";
                        return RedirectToPage();
                    }

                    var success = await _stageService.UpdateStageAsync(updatedStage);
                    
                    if (success)
                    {
                        _logger.LogInformation("Stage updated successfully: {StageName}", updatedStage.Name);
                        TempData["SuccessMessage"] = $"Production stage '{updatedStage.Name}' updated successfully.";
                    }
                    else
                    {
                        _logger.LogWarning("Stage update failed for: {StageName}", updatedStage.Name);
                        TempData["ErrorMessage"] = "Failed to update production stage. Please try again.";
                    }

                    return RedirectToPage();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating production stage");
                
                var contentType = Request.ContentType?.ToLower();
                if (contentType?.Contains("application/json") == true)
                {
                    return BadRequest("Error updating stage");
                }
                else
                {
                    TempData["ErrorMessage"] = "Error updating production stage. Please try again.";
                    return RedirectToPage();
                }
            }
        }
    }
}