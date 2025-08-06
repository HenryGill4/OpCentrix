using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using OpCentrix.Services.Admin;

namespace OpCentrix.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class MaterialsModel : PageModel
{
    private readonly IMaterialService _materialService;
    private readonly ILogger<MaterialsModel> _logger;

    public MaterialsModel(IMaterialService materialService, ILogger<MaterialsModel> logger)
    {
        _materialService = materialService;
        _logger = logger;
    }

    #region Properties
    public List<Material> Materials { get; set; } = new();
    public List<string> MaterialTypes { get; set; } = new();
    public Dictionary<string, int> MaterialTypeStatistics { get; set; } = new();
    public int TotalMaterials { get; set; }
    public int ActiveMaterials { get; set; }
    public decimal AverageCostPerGram { get; set; }

    // Search and filter properties
    public string SearchTerm { get; set; } = string.Empty;
    public string TypeFilter { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = string.Empty;
    public string SortBy { get; set; } = "MaterialCode";
    public string SortDirection { get; set; } = "asc";

    [BindProperty]
    public CreateMaterialRequest CreateMaterialRequest { get; set; } = new();

    [BindProperty]
    public int EditingMaterialId { get; set; }
    #endregion

    public async Task<IActionResult> OnGetAsync(string searchTerm = "", string typeFilter = "", string statusFilter = "", string sortBy = "MaterialCode", string sortDirection = "asc")
    {
        try
        {
            SearchTerm = searchTerm?.Trim() ?? "";
            TypeFilter = typeFilter?.Trim() ?? "";
            StatusFilter = statusFilter?.Trim() ?? "";
            SortBy = sortBy;
            SortDirection = sortDirection;

            await LoadMaterialsAsync();
            await LoadStatisticsAsync();

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading materials page");
            TempData["ErrorMessage"] = "Error loading materials. Please try again.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostCreateMaterialAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadMaterialsAsync();
                await LoadStatisticsAsync();
                TempData["ErrorMessage"] = "Please check the form for errors.";
                return Page();
            }

            var material = new Material
            {
                MaterialCode = CreateMaterialRequest.MaterialCode.Trim(),
                MaterialName = CreateMaterialRequest.MaterialName.Trim(),
                MaterialType = CreateMaterialRequest.MaterialType,
                Description = CreateMaterialRequest.Description?.Trim() ?? "",
                Density = CreateMaterialRequest.Density,
                MeltingPointC = CreateMaterialRequest.MeltingPointC,
                IsActive = CreateMaterialRequest.IsActive,
                CostPerGram = CreateMaterialRequest.CostPerGram,
                DefaultLayerThicknessMicrons = CreateMaterialRequest.DefaultLayerThicknessMicrons,
                DefaultLaserPowerPercent = CreateMaterialRequest.DefaultLaserPowerPercent,
                DefaultScanSpeedMmPerSec = CreateMaterialRequest.DefaultScanSpeedMmPerSec,
                CompatibleMachineTypes = CreateMaterialRequest.CompatibleMachineTypes?.Trim() ?? "SLS",
                SafetyNotes = CreateMaterialRequest.SafetyNotes?.Trim() ?? ""
            };

            var createdMaterial = await _materialService.CreateMaterialAsync(material, User.Identity?.Name ?? "Admin");

            _logger.LogInformation("Material created: {MaterialCode} by {User}", createdMaterial.MaterialCode, User.Identity?.Name);
            TempData["SuccessMessage"] = $"Material '{createdMaterial.MaterialCode}' created successfully.";

            return RedirectToPage();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Material creation validation error");
            TempData["ErrorMessage"] = ex.Message;
            await LoadMaterialsAsync();
            await LoadStatisticsAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating material {MaterialCode}", CreateMaterialRequest.MaterialCode);
            TempData["ErrorMessage"] = "Error creating material. Please try again.";
            await LoadMaterialsAsync();
            await LoadStatisticsAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostEditMaterialAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadMaterialsAsync();
                await LoadStatisticsAsync();
                TempData["ErrorMessage"] = "Please check the form for errors.";
                return Page();
            }

            var material = new Material
            {
                Id = EditingMaterialId,
                MaterialCode = CreateMaterialRequest.MaterialCode.Trim(),
                MaterialName = CreateMaterialRequest.MaterialName.Trim(),
                MaterialType = CreateMaterialRequest.MaterialType,
                Description = CreateMaterialRequest.Description?.Trim() ?? "",
                Density = CreateMaterialRequest.Density,
                MeltingPointC = CreateMaterialRequest.MeltingPointC,
                IsActive = CreateMaterialRequest.IsActive,
                CostPerGram = CreateMaterialRequest.CostPerGram,
                DefaultLayerThicknessMicrons = CreateMaterialRequest.DefaultLayerThicknessMicrons,
                DefaultLaserPowerPercent = CreateMaterialRequest.DefaultLaserPowerPercent,
                DefaultScanSpeedMmPerSec = CreateMaterialRequest.DefaultScanSpeedMmPerSec,
                CompatibleMachineTypes = CreateMaterialRequest.CompatibleMachineTypes?.Trim() ?? "SLS",
                SafetyNotes = CreateMaterialRequest.SafetyNotes?.Trim() ?? ""
            };

            var updatedMaterial = await _materialService.UpdateMaterialAsync(material, User.Identity?.Name ?? "Admin");

            _logger.LogInformation("Material updated: {MaterialCode} by {User}", updatedMaterial.MaterialCode, User.Identity?.Name);
            TempData["SuccessMessage"] = $"Material '{updatedMaterial.MaterialCode}' updated successfully.";

            return RedirectToPage();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Material update validation error");
            TempData["ErrorMessage"] = ex.Message;
            await LoadMaterialsAsync();
            await LoadStatisticsAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating material {MaterialCode}", CreateMaterialRequest.MaterialCode);
            TempData["ErrorMessage"] = "Error updating material. Please try again.";
            await LoadMaterialsAsync();
            await LoadStatisticsAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostToggleMaterialStatusAsync(int materialId)
    {
        try
        {
            var material = await _materialService.GetMaterialByIdAsync(materialId);
            if (material == null)
            {
                TempData["ErrorMessage"] = "Material not found.";
                return RedirectToPage();
            }

            material.IsActive = !material.IsActive;
            await _materialService.UpdateMaterialAsync(material, User.Identity?.Name ?? "Admin");

            var statusText = material.IsActive ? "activated" : "deactivated";
            _logger.LogInformation("Material {MaterialCode} {Status} by {User}", material.MaterialCode, statusText, User.Identity?.Name);
            TempData["SuccessMessage"] = $"Material '{material.MaterialCode}' {statusText} successfully.";

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling material status for ID {MaterialId}", materialId);
            TempData["ErrorMessage"] = "Error updating material status. Please try again.";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostDeleteMaterialAsync(int materialId)
    {
        try
        {
            var material = await _materialService.GetMaterialByIdAsync(materialId);
            if (material == null)
            {
                TempData["ErrorMessage"] = "Material not found.";
                return RedirectToPage();
            }

            var materialCode = material.MaterialCode;
            var success = await _materialService.DeleteMaterialAsync(materialId);

            if (success)
            {
                _logger.LogInformation("Material {MaterialCode} deleted by {User}", materialCode, User.Identity?.Name);
                TempData["SuccessMessage"] = $"Material '{materialCode}' deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Material not found.";
            }

            return RedirectToPage();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Material deletion validation error for ID {MaterialId}", materialId);
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting material ID {MaterialId}", materialId);
            TempData["ErrorMessage"] = "Error deleting material. Please try again.";
            return RedirectToPage();
        }
    }

    #region Helper Methods
    private async Task LoadMaterialsAsync()
    {
        var allMaterials = await _materialService.GetAllMaterialsAsync();

        // Apply filters
        var filteredMaterials = allMaterials.AsEnumerable();

        if (!string.IsNullOrEmpty(SearchTerm))
        {
            filteredMaterials = filteredMaterials.Where(m =>
                m.MaterialCode.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                m.MaterialName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                m.MaterialType.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                m.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(TypeFilter))
        {
            filteredMaterials = filteredMaterials.Where(m => m.MaterialType.Equals(TypeFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(StatusFilter))
        {
            var isActive = StatusFilter.Equals("active", StringComparison.OrdinalIgnoreCase);
            filteredMaterials = filteredMaterials.Where(m => m.IsActive == isActive);
        }

        // Apply sorting
        filteredMaterials = SortBy.ToLower() switch
        {
            "materialcode" => SortDirection == "desc"
                ? filteredMaterials.OrderByDescending(m => m.MaterialCode)
                : filteredMaterials.OrderBy(m => m.MaterialCode),
            "materialname" => SortDirection == "desc"
                ? filteredMaterials.OrderByDescending(m => m.MaterialName)
                : filteredMaterials.OrderBy(m => m.MaterialName),
            "materialtype" => SortDirection == "desc"
                ? filteredMaterials.OrderByDescending(m => m.MaterialType)
                : filteredMaterials.OrderBy(m => m.MaterialType),
            "costpergram" => SortDirection == "desc"
                ? filteredMaterials.OrderByDescending(m => m.CostPerGram)
                : filteredMaterials.OrderBy(m => m.CostPerGram),
            _ => filteredMaterials.OrderBy(m => m.MaterialCode)
        };

        Materials = filteredMaterials.ToList();
        MaterialTypes = await _materialService.GetMaterialTypesAsync();
    }

    private async Task LoadStatisticsAsync()
    {
        var allMaterials = await _materialService.GetAllMaterialsAsync();

        TotalMaterials = allMaterials.Count;
        ActiveMaterials = allMaterials.Count(m => m.IsActive);
        AverageCostPerGram = allMaterials.Any() ? allMaterials.Average(m => m.CostPerGram) : 0;

        MaterialTypeStatistics = allMaterials
            .GroupBy(m => m.MaterialType)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public string GetNextSortDirection(string column)
    {
        if (SortBy.Equals(column, StringComparison.OrdinalIgnoreCase))
        {
            return SortDirection == "asc" ? "desc" : "asc";
        }
        return "asc";
    }

    public string GetSortIcon(string column)
    {
        if (!SortBy.Equals(column, StringComparison.OrdinalIgnoreCase))
        {
            return "↕️";
        }
        return SortDirection == "asc" ? "⬆️" : "⬇️";
    }
    #endregion
}

#region Request Models
public class CreateMaterialRequest
{
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string MaterialType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Density { get; set; } = 4.43;
    public double MeltingPointC { get; set; } = 1660;
    public bool IsActive { get; set; } = true;
    public decimal CostPerGram { get; set; } = 0.50m;
    public double DefaultLayerThicknessMicrons { get; set; } = 30;
    public double DefaultLaserPowerPercent { get; set; } = 85;
    public double DefaultScanSpeedMmPerSec { get; set; } = 1200;
    public string CompatibleMachineTypes { get; set; } = "SLS";
    public string SafetyNotes { get; set; } = string.Empty;
}
#endregion