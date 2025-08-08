using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using OpCentrix.Services;
using OpCentrix.Models;
using Microsoft.AspNetCore.Authorization;

namespace OpCentrix.Pages.Admin.ProductionStages
{
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

        // Display properties
        public List<StageTemplate> StageTemplates { get; set; } = new();
        public List<StageTemplateCategory> Categories { get; set; } = new();
        public Dictionary<string, int> UsageStatistics { get; set; } = new();

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public string? CategoryFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? IndustryFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? ComplexityFilter { get; set; }

        // Form binding properties
        [BindProperty]
        public StageTemplate NewTemplate { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadTemplatesAsync();
                await LoadCategoriesAsync();
                await LoadStatisticsAsync();
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stage templates");
                TempData["ErrorMessage"] = "Error loading templates. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCreateTemplateAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadTemplatesAsync();
                    await LoadCategoriesAsync();
                    await LoadStatisticsAsync();
                    return Page();
                }

                NewTemplate.CreatedBy = User.Identity?.Name ?? "System";
                NewTemplate.LastModifiedBy = NewTemplate.CreatedBy;

                var createdTemplate = await _stageTemplateService.CreateTemplateAsync(NewTemplate);
                
                TempData["SuccessMessage"] = $"Template '{createdTemplate.Name}' created successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stage template");
                TempData["ErrorMessage"] = "Error creating template. Please try again.";
                
                await LoadTemplatesAsync();
                await LoadCategoriesAsync();
                await LoadStatisticsAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteTemplateAsync(int templateId)
        {
            try
            {
                var success = await _stageTemplateService.DeleteTemplateAsync(templateId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Template deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Template not found or could not be deleted.";
                }
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting template {TemplateId}", templateId);
                TempData["ErrorMessage"] = "Error deleting template. Please try again.";
                return RedirectToPage();
            }
        }

        private async Task LoadTemplatesAsync()
        {
            StageTemplates = await _stageTemplateService.GetActiveTemplatesAsync();
            
            // Apply filters
            if (!string.IsNullOrEmpty(IndustryFilter))
            {
                StageTemplates = StageTemplates.Where(t => t.Industry == IndustryFilter).ToList();
            }
            
            if (!string.IsNullOrEmpty(ComplexityFilter))
            {
                StageTemplates = StageTemplates.Where(t => t.ComplexityLevel == ComplexityFilter).ToList();
            }
        }

        private async Task LoadCategoriesAsync()
        {
            Categories = await _stageTemplateService.GetActiveCategoriesAsync();
        }

        private async Task LoadStatisticsAsync()
        {
            UsageStatistics = await _stageTemplateService.GetTemplateUsageStatisticsAsync();
        }

        // Helper methods for display
        public string GetComplexityBadgeClass(string complexityLevel)
        {
            return complexityLevel switch
            {
                "Simple" => "bg-success",
                "Medium" => "bg-info",
                "Complex" => "bg-warning",
                "VeryComplex" => "bg-danger",
                _ => "bg-secondary"
            };
        }

        public string GetIndustryBadgeClass(string industry)
        {
            return industry switch
            {
                "Firearms" => "bg-danger",
                "Aerospace" => "bg-primary",
                "Automotive" => "bg-warning",
                "Medical" => "bg-success",
                _ => "bg-secondary"
            };
        }
    }
}