using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using OpCentrix.Services.Admin;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.Admin;

/// <summary>
/// Admin page for managing defect categories in the quality management system
/// Provides comprehensive CRUD operations and statistics for defect classification
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class DefectCategoriesModel : PageModel
{
    private readonly DefectCategoryService _defectCategoryService;
    private readonly ILogger<DefectCategoriesModel> _logger;

    public DefectCategoriesModel(DefectCategoryService defectCategoryService, ILogger<DefectCategoriesModel> logger)
    {
        _defectCategoryService = defectCategoryService;
        _logger = logger;
    }

    #region Properties

    public List<DefectCategory> DefectCategories { get; set; } = new();
    public DefectCategoryStatistics Statistics { get; set; } = new();
    public List<string> AvailableGroups { get; set; } = new();

    // Filtering and search
    [BindProperty(SupportsGet = true)]
    public string SearchTerm { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string CategoryGroup { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public int? SeverityLevel { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "SortOrder";

    [BindProperty(SupportsGet = true)]
    public string SortDirection { get; set; } = "asc";

    // Form models
    [BindProperty]
    public DefectCategoryFormModel CategoryInput { get; set; } = new();

    [BindProperty]
    public int? EditingCategoryId { get; set; }

    #endregion

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Loading defect categories page - Admin: {Admin}", User.Identity?.Name);

            await LoadDefectCategoriesAsync();
            await LoadStatisticsAsync();
            await LoadAvailableGroupsAsync();

            _logger.LogInformation("Defect categories page loaded - {CategoryCount} categories found", 
                DefectCategories.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading defect categories page");
            TempData["Error"] = "Error loading defect categories. Please try again.";
            
            // Initialize with empty data on error
            DefectCategories = new List<DefectCategory>();
            Statistics = new DefectCategoryStatistics();
            AvailableGroups = new List<string>();
        }
    }

    public async Task<IActionResult> OnPostCreateCategoryAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadDefectCategoriesAsync();
                await LoadStatisticsAsync();
                await LoadAvailableGroupsAsync();
                TempData["Error"] = "Please correct the validation errors and try again.";
                return Page();
            }

            var category = new DefectCategory
            {
                Name = CategoryInput.Name,
                Description = CategoryInput.Description,
                Code = CategoryInput.Code,
                SeverityLevel = CategoryInput.SeverityLevel,
                IsActive = CategoryInput.IsActive,
                CategoryGroup = CategoryInput.CategoryGroup,
                ApplicableProcesses = CategoryInput.ApplicableProcesses,
                StandardCorrectiveActions = CategoryInput.StandardCorrectiveActions,
                PreventionMethods = CategoryInput.PreventionMethods,
                RequiresImmediateNotification = CategoryInput.RequiresImmediateNotification,
                CostImpact = CategoryInput.CostImpact,
                AverageResolutionTimeMinutes = CategoryInput.AverageResolutionTimeMinutes,
                SortOrder = CategoryInput.SortOrder,
                ColorCode = CategoryInput.ColorCode,
                Icon = CategoryInput.Icon,
                CreatedBy = User.Identity?.Name ?? "Admin",
                LastModifiedBy = User.Identity?.Name ?? "Admin"
            };

            var createdCategory = await _defectCategoryService.CreateDefectCategoryAsync(category);

            TempData["Success"] = $"Defect category '{CategoryInput.Name}' created successfully.";
            _logger.LogInformation("Admin {Admin} created defect category: {Name}", 
                User.Identity?.Name, CategoryInput.Name);

            // Clear the form
            CategoryInput = new DefectCategoryFormModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating defect category: {Name}", CategoryInput.Name);
            TempData["Error"] = ex.Message.Contains("already exists") ? ex.Message : "An error occurred while creating the defect category.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditCategoryAsync()
    {
        try
        {
            if (!EditingCategoryId.HasValue)
            {
                TempData["Error"] = "Invalid category ID.";
                return RedirectToPage();
            }

            if (!ModelState.IsValid)
            {
                await LoadDefectCategoriesAsync();
                await LoadStatisticsAsync();
                await LoadAvailableGroupsAsync();
                TempData["Error"] = "Please correct the validation errors and try again.";
                return Page();
            }

            var category = new DefectCategory
            {
                Id = EditingCategoryId.Value,
                Name = CategoryInput.Name,
                Description = CategoryInput.Description,
                Code = CategoryInput.Code,
                SeverityLevel = CategoryInput.SeverityLevel,
                IsActive = CategoryInput.IsActive,
                CategoryGroup = CategoryInput.CategoryGroup,
                ApplicableProcesses = CategoryInput.ApplicableProcesses,
                StandardCorrectiveActions = CategoryInput.StandardCorrectiveActions,
                PreventionMethods = CategoryInput.PreventionMethods,
                RequiresImmediateNotification = CategoryInput.RequiresImmediateNotification,
                CostImpact = CategoryInput.CostImpact,
                AverageResolutionTimeMinutes = CategoryInput.AverageResolutionTimeMinutes,
                SortOrder = CategoryInput.SortOrder,
                ColorCode = CategoryInput.ColorCode,
                Icon = CategoryInput.Icon,
                LastModifiedBy = User.Identity?.Name ?? "Admin"
            };

            var success = await _defectCategoryService.UpdateDefectCategoryAsync(category);

            if (success)
            {
                TempData["Success"] = $"Defect category '{CategoryInput.Name}' updated successfully.";
                _logger.LogInformation("Admin {Admin} updated defect category: {Name}", 
                    User.Identity?.Name, CategoryInput.Name);

                // Clear editing state
                EditingCategoryId = null;
                CategoryInput = new DefectCategoryFormModel();
            }
            else
            {
                TempData["Error"] = "Defect category not found.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating defect category ID: {Id}", EditingCategoryId);
            TempData["Error"] = ex.Message.Contains("already exists") ? ex.Message : "An error occurred while updating the defect category.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteCategoryAsync(int id)
    {
        try
        {
            var category = await _defectCategoryService.GetDefectCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["Error"] = "Defect category not found.";
                return RedirectToPage();
            }

            var success = await _defectCategoryService.DeleteDefectCategoryAsync(id);

            if (success)
            {
                TempData["Success"] = $"Defect category '{category.Name}' deleted successfully.";
                _logger.LogInformation("Admin {Admin} deleted defect category: {Name}", 
                    User.Identity?.Name, category.Name);
            }
            else
            {
                TempData["Error"] = "Failed to delete defect category.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting defect category ID: {Id}", id);
            TempData["Error"] = ex.Message.Contains("referenced by") ? ex.Message : "An error occurred while deleting the defect category.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(int id)
    {
        try
        {
            var success = await _defectCategoryService.ToggleActiveStatusAsync(id);

            if (success)
            {
                TempData["Success"] = "Defect category status updated successfully.";
                _logger.LogInformation("Admin {Admin} toggled status for defect category ID: {Id}", 
                    User.Identity?.Name, id);
            }
            else
            {
                TempData["Error"] = "Defect category not found.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for defect category ID: {Id}", id);
            TempData["Error"] = "An error occurred while updating the defect category status.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReorderCategoriesAsync(string categoryIds)
    {
        try
        {
            if (string.IsNullOrEmpty(categoryIds))
            {
                TempData["Error"] = "No categories to reorder.";
                return RedirectToPage();
            }

            var ids = categoryIds.Split(',')
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .ToList();

            if (ids.Any())
            {
                var success = await _defectCategoryService.ReorderCategoriesAsync(ids);

                if (success)
                {
                    TempData["Success"] = "Defect categories reordered successfully.";
                    _logger.LogInformation("Admin {Admin} reordered {Count} defect categories", 
                        User.Identity?.Name, ids.Count);
                }
                else
                {
                    TempData["Error"] = "Failed to reorder defect categories.";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering defect categories");
            TempData["Error"] = "An error occurred while reordering the defect categories.";
        }

        return RedirectToPage();
    }

    #region Helper Methods

    private async Task LoadDefectCategoriesAsync()
    {
        DefectCategories = await _defectCategoryService.GetAllDefectCategoriesAsync(
            SearchTerm, CategoryGroup, SeverityLevel, IsActive);

        // Apply sorting
        DefectCategories = SortDirection.ToLower() == "desc"
            ? SortBy switch
            {
                "Name" => DefectCategories.OrderByDescending(dc => dc.Name).ToList(),
                "Code" => DefectCategories.OrderByDescending(dc => dc.Code).ToList(),
                "SeverityLevel" => DefectCategories.OrderByDescending(dc => dc.SeverityLevel).ToList(),
                "CategoryGroup" => DefectCategories.OrderByDescending(dc => dc.CategoryGroup).ToList(),
                "IsActive" => DefectCategories.OrderByDescending(dc => dc.IsActive).ToList(),
                "CreatedDate" => DefectCategories.OrderByDescending(dc => dc.CreatedDate).ToList(),
                _ => DefectCategories.OrderByDescending(dc => dc.SortOrder).ThenByDescending(dc => dc.Name).ToList()
            }
            : SortBy switch
            {
                "Name" => DefectCategories.OrderBy(dc => dc.Name).ToList(),
                "Code" => DefectCategories.OrderBy(dc => dc.Code).ToList(),
                "SeverityLevel" => DefectCategories.OrderBy(dc => dc.SeverityLevel).ToList(),
                "CategoryGroup" => DefectCategories.OrderBy(dc => dc.CategoryGroup).ToList(),
                "IsActive" => DefectCategories.OrderBy(dc => dc.IsActive).ToList(),
                "CreatedDate" => DefectCategories.OrderBy(dc => dc.CreatedDate).ToList(),
                _ => DefectCategories.OrderBy(dc => dc.SortOrder).ThenBy(dc => dc.Name).ToList()
            };
    }

    private async Task LoadStatisticsAsync()
    {
        Statistics = await _defectCategoryService.GetStatisticsAsync();
    }

    private async Task LoadAvailableGroupsAsync()
    {
        AvailableGroups = await _defectCategoryService.GetCategoryGroupsAsync();
    }

    public void LoadCategoryForEditing(DefectCategory category)
    {
        EditingCategoryId = category.Id;
        CategoryInput = new DefectCategoryFormModel
        {
            Name = category.Name,
            Description = category.Description,
            Code = category.Code,
            SeverityLevel = category.SeverityLevel,
            IsActive = category.IsActive,
            CategoryGroup = category.CategoryGroup,
            ApplicableProcesses = category.ApplicableProcesses,
            StandardCorrectiveActions = category.StandardCorrectiveActions,
            PreventionMethods = category.PreventionMethods,
            RequiresImmediateNotification = category.RequiresImmediateNotification,
            CostImpact = category.CostImpact,
            AverageResolutionTimeMinutes = category.AverageResolutionTimeMinutes,
            SortOrder = category.SortOrder,
            ColorCode = category.ColorCode,
            Icon = category.Icon
        };
    }

    public string GetNextSortDirection(string column)
    {
        if (SortBy == column)
            return SortDirection == "asc" ? "desc" : "asc";
        return "asc";
    }

    public string GetSortIcon(string column)
    {
        if (SortBy != column) return "?";
        return SortDirection == "asc" ? "?" : "?";
    }

    #endregion
}

/// <summary>
/// Form model for defect category creation and editing
/// </summary>
public class DefectCategoryFormModel
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Range(1, 5)]
    public int SeverityLevel { get; set; } = 3;

    public bool IsActive { get; set; } = true;

    [StringLength(50)]
    public string CategoryGroup { get; set; } = DefectCategoryGroups.Other;

    [StringLength(200)]
    public string ApplicableProcesses { get; set; } = string.Empty;

    [StringLength(1000)]
    public string StandardCorrectiveActions { get; set; } = string.Empty;

    [StringLength(1000)]
    public string PreventionMethods { get; set; } = string.Empty;

    public bool RequiresImmediateNotification { get; set; } = false;

    [StringLength(20)]
    public string CostImpact { get; set; } = CostImpactLevels.Medium;

    [Range(1, 9999)]
    public int AverageResolutionTimeMinutes { get; set; } = 30;

    [Range(1, 9999)]
    public int SortOrder { get; set; } = 100;

    [StringLength(7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color code must be a valid hex color (e.g., #FF0000)")]
    public string ColorCode { get; set; } = "#6B7280";

    [StringLength(50)]
    public string Icon { get; set; } = "exclamation-triangle";
}