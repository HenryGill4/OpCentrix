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
        public List<int> ReorderStageIds { get; set; } = new List<int>();

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
                if (!ModelState.IsValid)
                {
                    await OnGetAsync();
                    return Page();
                }

                var success = await _stageService.CreateStageAsync(NewStage);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Production stage '{NewStage.Name}' created successfully.";
                    return RedirectToPage();
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create production stage. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating production stage");
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
                if (ReorderStageIds?.Any() == true)
                {
                    var success = await _stageService.ReorderStagesAsync(ReorderStageIds);
                    
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Production stages reordered successfully.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to reorder production stages. Please try again.";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering production stages");
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

        public async Task<IActionResult> OnPostUpdateStageAsync([FromBody] ProductionStage stage)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating production stage {StageId}", stage?.Id);
                return BadRequest("Error updating stage");
            }
        }
    }
}