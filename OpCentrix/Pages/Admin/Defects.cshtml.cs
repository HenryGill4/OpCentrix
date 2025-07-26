using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using OpCentrix.Services.Admin;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.Admin;

/// <summary>
/// Admin page for managing defect categories in the quality control system
/// Task 14: Defect Category Manager - Complete CRUD interface
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class DefectsModel : PageModel
{
    private readonly IDefectCategoryService _defectCategoryService;
    private readonly ILogger<DefectsModel> _logger;

    public DefectsModel(IDefectCategoryService defectCategoryService, ILogger<DefectsModel> logger)
    {
        _defectCategoryService = defectCategoryService;
        _logger = logger;
    }

    // Page Properties
    public List<DefectCategory> DefectCategories { get; set; } = new();
    public List<string> CategoryGroups { get; set; } = new();
    public List<string> ApplicableProcesses { get; set; } = new();
    public Dictionary<string, int> DefectStatistics { get; set; } = new();

    // Filtering and Search
    [BindProperty(SupportsGet = true)]
    public string SearchTerm { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string CategoryGroupFilter { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public int? SeverityFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string StatusFilter { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "CategoryGroup";

    [BindProperty(SupportsGet = true)]
    public string SortDirection { get; set; } = "asc";

    // Form Input Models
    [BindProperty]
    public DefectCategoryInputModel DefectCategoryInput { get; set; } = new();

    [BindProperty]
    public int? EditingDefectCategoryId { get; set; }

    [BindProperty]
    public List<SortOrderUpdate> SortOrderUpdates { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Loading defect categories management page - Admin: {Admin}", User.Identity?.Name);

            // Clear any existing model state issues for GET requests
            ModelState.Clear();

            await LoadDefectCategoriesAsync();
            await LoadPageDataAsync();

            _logger.LogInformation("Defect categories page loaded - {CategoryCount} categories found", DefectCategories.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading defect categories management page");
            TempData["Error"] = "Error loading defect categories. Please try again.";
            
            // Initialize with empty data on error
            DefectCategories = new List<DefectCategory>();
            CategoryGroups = new List<string>();
            ApplicableProcesses = new List<string>();
            DefectStatistics = new Dictionary<string, int>();
        }
    }

    public async Task<IActionResult> OnPostCreateDefectCategoryAsync()
    {
        try
        {
            _logger.LogInformation("Creating new defect category: {Name}", DefectCategoryInput.Name);

            if (!ModelState.IsValid)
            {
                await LoadPageDataAsync();
                TempData["Error"] = "Please correct the validation errors and try again.";
                return Page();
            }

            // Additional validation
            var validationErrors = await ValidateDefectCategoryAsync(DefectCategoryInput, null);
            if (validationErrors.Count > 0)
            {
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError("", error);
                }
                await LoadPageDataAsync();
                TempData["Error"] = string.Join(" ", validationErrors);
                return Page();
            }

            var newDefectCategory = CreateDefectCategoryFromInput();
            var success = await _defectCategoryService.CreateDefectCategoryAsync(newDefectCategory);

            if (success)
            {
                TempData["Success"] = $"Defect category '{DefectCategoryInput.Name}' created successfully.";
                _logger.LogInformation("Admin {Admin} created defect category: {Name} ({Code})", 
                    User.Identity?.Name, DefectCategoryInput.Name, DefectCategoryInput.Code);
                
                // Clear the form
                DefectCategoryInput = new DefectCategoryInputModel();
            }
            else
            {
                TempData["Error"] = "Failed to create defect category. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating defect category {Name}", DefectCategoryInput.Name);
            TempData["Error"] = "An error occurred while creating the defect category.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditDefectCategoryAsync()
    {
        try
        {
            if (!EditingDefectCategoryId.HasValue)
            {
                TempData["Error"] = "Invalid defect category ID.";
                return RedirectToPage();
            }

            _logger.LogInformation("Editing defect category ID: {Id}", EditingDefectCategoryId.Value);

            if (!ModelState.IsValid)
            {
                await LoadPageDataAsync();
                TempData["Error"] = "Please correct the validation errors and try again.";
                return Page();
            }

            var existingCategory = await _defectCategoryService.GetDefectCategoryByIdAsync(EditingDefectCategoryId.Value);
            if (existingCategory == null)
            {
                TempData["Error"] = "Defect category not found.";
                return RedirectToPage();
            }

            // Additional validation
            var validationErrors = await ValidateDefectCategoryAsync(DefectCategoryInput, EditingDefectCategoryId.Value);
            if (validationErrors.Count > 0)
            {
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError("", error);
                }
                await LoadPageDataAsync();
                TempData["Error"] = string.Join(" ", validationErrors);
                return Page();
            }

            UpdateDefectCategoryFromInput(existingCategory);
            var success = await _defectCategoryService.UpdateDefectCategoryAsync(existingCategory);

            if (success)
            {
                TempData["Success"] = $"Defect category '{existingCategory.Name}' updated successfully.";
                _logger.LogInformation("Admin {Admin} updated defect category: {Name} ({Code})", 
                    User.Identity?.Name, existingCategory.Name, existingCategory.Code);

                // Clear editing state
                EditingDefectCategoryId = null;
                DefectCategoryInput = new DefectCategoryInputModel();
            }
            else
            {
                TempData["Error"] = "Failed to update defect category. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating defect category ID {Id}", EditingDefectCategoryId);
            TempData["Error"] = "An error occurred while updating the defect category.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(int defectCategoryId)
    {
        try
        {
            var defectCategory = await _defectCategoryService.GetDefectCategoryByIdAsync(defectCategoryId);
            if (defectCategory == null)
            {
                TempData["Error"] = "Defect category not found.";
                return RedirectToPage();
            }

            var success = await _defectCategoryService.ToggleDefectCategoryStatusAsync(defectCategoryId);
            
            if (success)
            {
                var action = !defectCategory.IsActive ? "activated" : "deactivated";
                TempData["Success"] = $"Defect category '{defectCategory.Name}' {action} successfully.";
                _logger.LogInformation("Admin {Admin} {Action} defect category: {Name}", 
                    User.Identity?.Name, action, defectCategory.Name);
            }
            else
            {
                TempData["Error"] = "Failed to update defect category status.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for defect category ID {Id}", defectCategoryId);
            TempData["Error"] = "An error occurred while updating defect category status.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteDefectCategoryAsync(int defectCategoryId)
    {
        try
        {
            var defectCategory = await _defectCategoryService.GetDefectCategoryByIdAsync(defectCategoryId);
            if (defectCategory == null)
            {
                TempData["Error"] = "Defect category not found.";
                return RedirectToPage();
            }

            // Check if it can be deleted
            if (!defectCategory.CanBeDeleted())
            {
                TempData["Error"] = $"Cannot delete defect category '{defectCategory.Name}' because it is referenced by inspection checkpoints or other records.";
                return RedirectToPage();
            }

            var success = await _defectCategoryService.DeleteDefectCategoryAsync(defectCategoryId);
            
            if (success)
            {
                TempData["Success"] = $"Defect category '{defectCategory.Name}' deleted successfully.";
                _logger.LogWarning("Admin {Admin} deleted defect category: {Name} ({Code})", 
                    User.Identity?.Name, defectCategory.Name, defectCategory.Code);
            }
            else
            {
                TempData["Error"] = "Failed to delete defect category.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting defect category ID {Id}", defectCategoryId);
            TempData["Error"] = "An error occurred while deleting the defect category.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateSortOrderAsync()
    {
        try
        {
            if (!SortOrderUpdates.Any())
            {
                TempData["Error"] = "No sort order changes to save.";
                return RedirectToPage();
            }

            var sortOrders = SortOrderUpdates.ToDictionary(s => s.Id, s => s.SortOrder);
            var success = await _defectCategoryService.BulkUpdateSortOrderAsync(sortOrders);

            if (success)
            {
                TempData["Success"] = "Sort order updated successfully.";
                _logger.LogInformation("Admin {Admin} updated sort order for {Count} defect categories", 
                    User.Identity?.Name, sortOrders.Count);
            }
            else
            {
                TempData["Error"] = "Failed to update sort order.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sort order for defect categories");
            TempData["Error"] = "An error occurred while updating sort order.";
        }

        return RedirectToPage();
    }

    // Helper Methods
    private async Task LoadDefectCategoriesAsync()
    {
        if (!string.IsNullOrEmpty(SearchTerm))
        {
            DefectCategories = await _defectCategoryService.SearchDefectCategoriesAsync(SearchTerm);
        }
        else
        {
            DefectCategories = await _defectCategoryService.GetAllDefectCategoriesAsync();
        }

        // Apply filters
        if (!string.IsNullOrEmpty(CategoryGroupFilter))
        {
            DefectCategories = DefectCategories.Where(dc => dc.CategoryGroup == CategoryGroupFilter).ToList();
        }

        if (SeverityFilter.HasValue)
        {
            DefectCategories = DefectCategories.Where(dc => dc.SeverityLevel == SeverityFilter.Value).ToList();
        }

        if (!string.IsNullOrEmpty(StatusFilter))
        {
            var isActive = StatusFilter.ToLower() == "active";
            DefectCategories = DefectCategories.Where(dc => dc.IsActive == isActive).ToList();
        }

        // Apply sorting
        DefectCategories = SortDirection.ToLower() == "desc"
            ? SortBy switch
            {
                "Name" => DefectCategories.OrderByDescending(dc => dc.Name).ToList(),
                "Code" => DefectCategories.OrderByDescending(dc => dc.Code).ToList(),
                "SeverityLevel" => DefectCategories.OrderByDescending(dc => dc.SeverityLevel).ToList(),
                "CategoryGroup" => DefectCategories.OrderByDescending(dc => dc.CategoryGroup).ToList(),
                "CreatedDate" => DefectCategories.OrderByDescending(dc => dc.CreatedDate).ToList(),
                "IsActive" => DefectCategories.OrderByDescending(dc => dc.IsActive).ToList(),
                _ => DefectCategories.OrderByDescending(dc => dc.CategoryGroup).ThenByDescending(dc => dc.SortOrder).ToList()
            }
            : SortBy switch
            {
                "Name" => DefectCategories.OrderBy(dc => dc.Name).ToList(),
                "Code" => DefectCategories.OrderBy(dc => dc.Code).ToList(),
                "SeverityLevel" => DefectCategories.OrderBy(dc => dc.SeverityLevel).ToList(),
                "CategoryGroup" => DefectCategories.OrderBy(dc => dc.CategoryGroup).ToList(),
                "CreatedDate" => DefectCategories.OrderBy(dc => dc.CreatedDate).ToList(),
                "IsActive" => DefectCategories.OrderBy(dc => dc.IsActive).ToList(),
                _ => DefectCategories.OrderBy(dc => dc.CategoryGroup).ThenBy(dc => dc.SortOrder).ToList()
            };
    }

    private async Task LoadPageDataAsync()
    {
        CategoryGroups = await _defectCategoryService.GetCategoryGroupsAsync();
        ApplicableProcesses = await _defectCategoryService.GetApplicableProcessesAsync();
        DefectStatistics = await _defectCategoryService.GetDefectStatisticsAsync();
    }

    private async Task<List<string>> ValidateDefectCategoryAsync(DefectCategoryInputModel input, int? excludeId)
    {
        var errors = new List<string>();

        // Check if code already exists
        if (!string.IsNullOrEmpty(input.Code))
        {
            var codeExists = await _defectCategoryService.DefectCategoryExistsAsync(input.Code, excludeId);
            if (codeExists)
            {
                errors.Add($"Defect category code '{input.Code}' already exists.");
            }
        }

        // Validate color code format
        if (!string.IsNullOrEmpty(input.ColorCode) && !input.ColorCode.StartsWith("#"))
        {
            errors.Add("Color code must start with '#' (e.g., #FF0000).");
        }

        return errors;
    }

    private DefectCategory CreateDefectCategoryFromInput()
    {
        return new DefectCategory
        {
            Name = DefectCategoryInput.Name,
            Description = DefectCategoryInput.Description,
            Code = DefectCategoryInput.Code,
            SeverityLevel = DefectCategoryInput.SeverityLevel,
            IsActive = DefectCategoryInput.IsActive,
            CategoryGroup = DefectCategoryInput.CategoryGroup,
            ApplicableProcesses = string.Join(", ", DefectCategoryInput.ApplicableProcessesList),
            StandardCorrectiveActions = DefectCategoryInput.StandardCorrectiveActions,
            PreventionMethods = DefectCategoryInput.PreventionMethods,
            RequiresImmediateNotification = DefectCategoryInput.RequiresImmediateNotification,
            CostImpact = DefectCategoryInput.CostImpact,
            AverageResolutionTimeMinutes = DefectCategoryInput.AverageResolutionTimeMinutes,
            SortOrder = DefectCategoryInput.SortOrder,
            ColorCode = string.IsNullOrEmpty(DefectCategoryInput.ColorCode) ? "#6B7280" : DefectCategoryInput.ColorCode,
            Icon = string.IsNullOrEmpty(DefectCategoryInput.Icon) ? "exclamation-triangle" : DefectCategoryInput.Icon,
            CreatedBy = User.Identity?.Name ?? "Admin",
            LastModifiedBy = User.Identity?.Name ?? "Admin"
        };
    }

    private void UpdateDefectCategoryFromInput(DefectCategory defectCategory)
    {
        defectCategory.Name = DefectCategoryInput.Name;
        defectCategory.Description = DefectCategoryInput.Description;
        defectCategory.Code = DefectCategoryInput.Code;
        defectCategory.SeverityLevel = DefectCategoryInput.SeverityLevel;
        defectCategory.IsActive = DefectCategoryInput.IsActive;
        defectCategory.CategoryGroup = DefectCategoryInput.CategoryGroup;
        defectCategory.ApplicableProcesses = string.Join(", ", DefectCategoryInput.ApplicableProcessesList);
        defectCategory.StandardCorrectiveActions = DefectCategoryInput.StandardCorrectiveActions;
        defectCategory.PreventionMethods = DefectCategoryInput.PreventionMethods;
        defectCategory.RequiresImmediateNotification = DefectCategoryInput.RequiresImmediateNotification;
        defectCategory.CostImpact = DefectCategoryInput.CostImpact;
        defectCategory.AverageResolutionTimeMinutes = DefectCategoryInput.AverageResolutionTimeMinutes;
        defectCategory.SortOrder = DefectCategoryInput.SortOrder;
        defectCategory.ColorCode = string.IsNullOrEmpty(DefectCategoryInput.ColorCode) ? "#6B7280" : DefectCategoryInput.ColorCode;
        defectCategory.Icon = string.IsNullOrEmpty(DefectCategoryInput.Icon) ? "exclamation-triangle" : DefectCategoryInput.Icon;
        defectCategory.LastModifiedBy = User.Identity?.Name ?? "Admin";
    }

    public void LoadDefectCategoryForEditing(DefectCategory defectCategory)
    {
        EditingDefectCategoryId = defectCategory.Id;
        DefectCategoryInput = new DefectCategoryInputModel
        {
            Name = defectCategory.Name,
            Description = defectCategory.Description,
            Code = defectCategory.Code,
            SeverityLevel = defectCategory.SeverityLevel,
            IsActive = defectCategory.IsActive,
            CategoryGroup = defectCategory.CategoryGroup,
            ApplicableProcessesList = defectCategory.GetApplicableProcessesList(),
            StandardCorrectiveActions = defectCategory.StandardCorrectiveActions,
            PreventionMethods = defectCategory.PreventionMethods,
            RequiresImmediateNotification = defectCategory.RequiresImmediateNotification,
            CostImpact = defectCategory.CostImpact,
            AverageResolutionTimeMinutes = defectCategory.AverageResolutionTimeMinutes,
            SortOrder = defectCategory.SortOrder,
            ColorCode = defectCategory.ColorCode,
            Icon = defectCategory.Icon
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
}

/// <summary>
/// Input model for creating and editing defect categories
/// </summary>
public class DefectCategoryInputModel
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "Category Code")]
    public string Code { get; set; } = string.Empty;

    [Range(1, 5)]
    [Display(Name = "Severity Level")]
    public int SeverityLevel { get; set; } = 3;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Required]
    [StringLength(50)]
    [Display(Name = "Category Group")]
    public string CategoryGroup { get; set; } = "General";

    [Display(Name = "Applicable Processes")]
    public List<string> ApplicableProcessesList { get; set; } = new();

    [StringLength(1000)]
    [Display(Name = "Standard Corrective Actions")]
    public string StandardCorrectiveActions { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Prevention Methods")]
    public string PreventionMethods { get; set; } = string.Empty;

    [Display(Name = "Requires Immediate Notification")]
    public bool RequiresImmediateNotification { get; set; } = false;

    [Required]
    [StringLength(20)]
    [Display(Name = "Cost Impact")]
    public string CostImpact { get; set; } = "Medium";

    [Range(1, 1440)]
    [Display(Name = "Average Resolution Time (minutes)")]
    public int AverageResolutionTimeMinutes { get; set; } = 30;

    [Range(1, 999)]
    [Display(Name = "Sort Order")]
    public int SortOrder { get; set; } = 100;

    [StringLength(7)]
    [Display(Name = "Color Code")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color code must be in hex format (e.g., #FF0000)")]
    public string ColorCode { get; set; } = "#6B7280";

    [StringLength(50)]
    [Display(Name = "Icon")]
    public string Icon { get; set; } = "exclamation-triangle";
}

/// <summary>
/// Model for sort order updates
/// </summary>
public class SortOrderUpdate
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
}