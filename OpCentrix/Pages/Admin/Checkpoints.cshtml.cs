using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services.Admin;

namespace OpCentrix.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class CheckpointsModel : PageModel
{
    private readonly IInspectionCheckpointService _checkpointService;
    private readonly SchedulerContext _context;
    private readonly ILogger<CheckpointsModel> _logger;

    public CheckpointsModel(
        IInspectionCheckpointService checkpointService,
        SchedulerContext context,
        ILogger<CheckpointsModel> logger)
    {
        _checkpointService = checkpointService;
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public InspectionCheckpoint Checkpoint { get; set; } = new();

    [BindProperty]
    public int SelectedPartId { get; set; }

    public IEnumerable<InspectionCheckpoint> Checkpoints { get; set; } = new List<InspectionCheckpoint>();
    public IEnumerable<Part> Parts { get; set; } = new List<Part>();
    public SelectList? PartsSelectList { get; set; }
    public SelectList? InspectionTypesSelectList { get; set; }
    public SelectList? CategoriesSelectList { get; set; }
    public SelectList? PrioritySelectList { get; set; }
    public SelectList? SamplingMethodsSelectList { get; set; }

    // Validation and Statistics
    public int TotalCheckpoints { get; set; }
    public int ActiveCheckpoints { get; set; }
    public int PartsWithCheckpoints { get; set; }
    public Dictionary<string, int> CheckpointsByType { get; set; } = new();
    public Dictionary<string, int> CheckpointsByCategory { get; set; } = new();

    // Filter and Sort Properties
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? FilterPartId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterType { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterCategory { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? FilterActiveOnly { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "Part";

    [BindProperty(SupportsGet = true)]
    public string SortDirection { get; set; } = "asc";

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            await LoadDataAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading inspection checkpoints page");
            TempData["ErrorMessage"] = "Error loading inspection checkpoints. Please try again.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            // Set audit fields
            Checkpoint.CreatedBy = User.Identity?.Name ?? "Admin";
            Checkpoint.LastModifiedBy = User.Identity?.Name ?? "Admin";

            var success = await _checkpointService.CreateCheckpointAsync(Checkpoint);

            if (success)
            {
                TempData["SuccessMessage"] = $"Checkpoint '{Checkpoint.CheckpointName}' created successfully.";
                return RedirectToPage(new { FilterPartId = Checkpoint.PartId });
            }

            ModelState.AddModelError("", "Failed to create checkpoint. Please try again.");
            await LoadDataAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inspection checkpoint");
            ModelState.AddModelError("", "An error occurred while creating the checkpoint.");
            await LoadDataAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            // Set audit fields
            Checkpoint.LastModifiedBy = User.Identity?.Name ?? "Admin";

            var success = await _checkpointService.UpdateCheckpointAsync(Checkpoint);

            if (success)
            {
                TempData["SuccessMessage"] = $"Checkpoint '{Checkpoint.CheckpointName}' updated successfully.";
                return RedirectToPage(new { FilterPartId = Checkpoint.PartId });
            }

            ModelState.AddModelError("", "Failed to update checkpoint. Please try again.");
            await LoadDataAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inspection checkpoint {CheckpointId}", Checkpoint.Id);
            ModelState.AddModelError("", "An error occurred while updating the checkpoint.");
            await LoadDataAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var success = await _checkpointService.DeleteCheckpointAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = "Checkpoint deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete checkpoint.";
            }

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inspection checkpoint {CheckpointId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the checkpoint.";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(int id)
    {
        try
        {
            var success = await _checkpointService.ToggleCheckpointStatusAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = "Checkpoint status updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update checkpoint status.";
            }

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling checkpoint status {CheckpointId}", id);
            TempData["ErrorMessage"] = "An error occurred while updating checkpoint status.";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostDuplicateAsync(int sourceId, int targetPartId)
    {
        try
        {
            var success = await _checkpointService.DuplicateCheckpointAsync(sourceId, targetPartId);

            if (success)
            {
                TempData["SuccessMessage"] = "Checkpoint duplicated successfully.";
                return RedirectToPage(new { FilterPartId = targetPartId });
            }

            TempData["ErrorMessage"] = "Failed to duplicate checkpoint.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating checkpoint {SourceId} to part {TargetPartId}", 
                sourceId, targetPartId);
            TempData["ErrorMessage"] = "An error occurred while duplicating the checkpoint.";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostReorderAsync(int partId, string checkpointIds)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(checkpointIds))
            {
                TempData["ErrorMessage"] = "Invalid checkpoint order data.";
                return RedirectToPage();
            }

            var ids = checkpointIds.Split(',')
                .Where(s => int.TryParse(s, out _))
                .Select(int.Parse)
                .ToList();

            if (!ids.Any())
            {
                TempData["ErrorMessage"] = "No valid checkpoint IDs provided.";
                return RedirectToPage();
            }

            var success = await _checkpointService.ReorderCheckpointsAsync(partId, ids);

            if (success)
            {
                TempData["SuccessMessage"] = "Checkpoints reordered successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reorder checkpoints.";
            }

            return RedirectToPage(new { FilterPartId = partId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering checkpoints for part {PartId}", partId);
            TempData["ErrorMessage"] = "An error occurred while reordering checkpoints.";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnGetEditAsync(int id)
    {
        try
        {
            var checkpoint = await _checkpointService.GetCheckpointByIdAsync(id);
            if (checkpoint == null)
            {
                TempData["ErrorMessage"] = "Checkpoint not found.";
                return RedirectToPage();
            }

            Checkpoint = checkpoint;
            await LoadDataAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading checkpoint {CheckpointId} for editing", id);
            TempData["ErrorMessage"] = "Error loading checkpoint for editing.";
            return RedirectToPage();
        }
    }

    private async Task LoadDataAsync()
    {
        // Load all checkpoints with filtering and sorting
        var checkpointsQuery = await _checkpointService.GetAllCheckpointsAsync();
        var checkpoints = checkpointsQuery.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            checkpoints = checkpoints.Where(c => 
                c.CheckpointName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.Part.PartNumber.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (FilterPartId.HasValue)
        {
            checkpoints = checkpoints.Where(c => c.PartId == FilterPartId.Value);
        }

        if (!string.IsNullOrWhiteSpace(FilterType))
        {
            checkpoints = checkpoints.Where(c => c.InspectionType == FilterType);
        }

        if (!string.IsNullOrWhiteSpace(FilterCategory))
        {
            checkpoints = checkpoints.Where(c => c.Category == FilterCategory);
        }

        if (FilterActiveOnly.HasValue && FilterActiveOnly.Value)
        {
            checkpoints = checkpoints.Where(c => c.IsActive);
        }

        // Apply sorting
        checkpoints = SortBy.ToLower() switch
        {
            "name" => SortDirection == "desc" 
                ? checkpoints.OrderByDescending(c => c.CheckpointName)
                : checkpoints.OrderBy(c => c.CheckpointName),
            "type" => SortDirection == "desc"
                ? checkpoints.OrderByDescending(c => c.InspectionType).ThenBy(c => c.Part.PartNumber)
                : checkpoints.OrderBy(c => c.InspectionType).ThenBy(c => c.Part.PartNumber),
            "priority" => SortDirection == "desc"
                ? checkpoints.OrderByDescending(c => c.Priority).ThenBy(c => c.Part.PartNumber)
                : checkpoints.OrderBy(c => c.Priority).ThenBy(c => c.Part.PartNumber),
            "order" => SortDirection == "desc"
                ? checkpoints.OrderByDescending(c => c.Part.PartNumber).ThenByDescending(c => c.SortOrder)
                : checkpoints.OrderBy(c => c.Part.PartNumber).ThenBy(c => c.SortOrder),
            _ => SortDirection == "desc"
                ? checkpoints.OrderByDescending(c => c.Part.PartNumber).ThenByDescending(c => c.SortOrder)
                : checkpoints.OrderBy(c => c.Part.PartNumber).ThenBy(c => c.SortOrder)
        };

        Checkpoints = checkpoints.ToList();

        // Load parts for dropdowns
        Parts = await _context.Parts
            .Where(p => p.IsActive)
            .OrderBy(p => p.PartNumber)
            .AsNoTracking()
            .ToListAsync();

        // Create select lists
        PartsSelectList = new SelectList(Parts, "Id", "PartNumber", SelectedPartId);

        var inspectionTypes = await _checkpointService.GetInspectionTypesAsync();
        InspectionTypesSelectList = new SelectList(inspectionTypes);

        var categories = await _checkpointService.GetCategoriesAsync();
        CategoriesSelectList = new SelectList(categories);

        PrioritySelectList = new SelectList(new[]
        {
            new { Value = 1, Text = "1 - Critical" },
            new { Value = 2, Text = "2 - High" },
            new { Value = 3, Text = "3 - Normal" },
            new { Value = 4, Text = "4 - Low" },
            new { Value = 5, Text = "5 - Minor" }
        }, "Value", "Text");

        SamplingMethodsSelectList = new SelectList(new[]
        {
            "All", "Random", "First/Last", "Statistical", "Skip-Lot", "Customer Specified"
        });

        // Calculate statistics
        await LoadStatisticsAsync();
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            var allCheckpoints = await _checkpointService.GetAllCheckpointsAsync();
            
            TotalCheckpoints = allCheckpoints.Count();
            ActiveCheckpoints = allCheckpoints.Count(c => c.IsActive);
            PartsWithCheckpoints = allCheckpoints.Select(c => c.PartId).Distinct().Count();

            CheckpointsByType = allCheckpoints
                .GroupBy(c => c.InspectionType)
                .ToDictionary(g => g.Key, g => g.Count());

            CheckpointsByCategory = allCheckpoints
                .GroupBy(c => c.Category)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading checkpoint statistics");
            TotalCheckpoints = 0;
            ActiveCheckpoints = 0;
            PartsWithCheckpoints = 0;
        }
    }
}