using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using System.Text.Json;

namespace OpCentrix.Pages.Admin
{
    /// <summary>
    /// Parts management page with CRUD operations
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class PartsModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PartsModel> _logger;

        public PartsModel(
            SchedulerContext context,
            ILogger<PartsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Main data properties
        public IList<Part> Parts { get; set; } = new List<Part>();
        
        // Form property for modal
        [BindProperty]
        public Part Part { get; set; } = new Part();
        
        // Pagination and filtering properties
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        
        // Filter properties
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? MaterialFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? IndustryFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? CategoryFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public bool ActiveOnly { get; set; } = true;
        
        // Sorting properties
        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; } = "PartNumber";
        
        [BindProperty(SupportsGet = true)]
        public string? SortDirection { get; set; } = "asc";
        
        // Filter options for dropdowns
        public List<string> AvailableMaterials { get; set; } = new List<string>();
        public List<string> AvailableIndustries { get; set; } = new List<string>();
        public List<string> AvailableCategories { get; set; } = new List<string>();
        
        // Statistics
        public int ActivePartsCount { get; set; }
        public int InactivePartsCount { get; set; }
        public string MostUsedMaterial { get; set; } = string.Empty;
        public double AverageEstimatedHours { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadPartsDataAsync();
                await LoadFilterOptionsAsync();
                await LoadStatisticsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading parts data");
                TempData["ErrorMessage"] = "Error loading parts data. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostScheduleJobAsync(int partId)
        {
            try
            {
                var part = await _context.Parts.FindAsync(partId);
                if (part == null) 
                {
                    _logger.LogWarning("Part with ID {PartId} not found for scheduling", partId);
                    return NotFound();
                }

                if (!part.IsActive)
                {
                    _logger.LogWarning("Attempt to schedule job for inactive part {PartNumber}", part.PartNumber);
                    TempData["ErrorMessage"] = $"Cannot schedule job for inactive part {part.PartNumber}";
                    return RedirectToPage();
                }

                _logger.LogInformation("Redirecting to scheduler for part {PartNumber} (ID: {PartId})", part.PartNumber, part.Id);

                // Redirect to scheduler with part data pre-population
                return RedirectToPage("/Scheduler/Index", new { 
                    partId = part.Id, 
                    partNumber = part.PartNumber,
                    partName = part.Name,
                    prePopulate = true,
                    source = "parts"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling job for part ID {PartId}", partId);
                TempData["ErrorMessage"] = "Error opening scheduler. Please try again.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetPartDataAsync(int id)
        {
            try
            {
                var part = await _context.Parts.FindAsync(id);
                if (part == null)
                {
                    return NotFound();
                }

                var partData = new
                {
                    id = part.Id,
                    partNumber = part.PartNumber,
                    name = part.Name,
                    description = part.Description,
                    industry = part.Industry,
                    application = part.Application,
                    partCategory = part.PartCategory,
                    partClass = part.PartClass,
                    customerPartNumber = part.CustomerPartNumber,
                    material = part.Material,
                    slsMaterial = part.SlsMaterial,
                    estimatedHours = part.EstimatedHours,
                    adminEstimatedHoursOverride = part.AdminEstimatedHoursOverride,
                    processType = part.ProcessType,
                    requiredMachineType = part.RequiredMachineType,
                    materialCostPerKg = part.MaterialCostPerKg,
                    lengthMm = part.LengthMm,
                    widthMm = part.WidthMm,
                    heightMm = part.HeightMm,
                    weightGrams = part.WeightGrams,
                    requiresInspection = part.RequiresInspection,
                    requiresCertification = part.RequiresCertification,
                    requiresFDA = part.RequiresFDA,
                    requiresAS9100 = part.RequiresAS9100,
                    requiresNADCAP = part.RequiresNADCAP
                };

                return new JsonResult(partData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part data for ID {PartId}", id);
                return StatusCode(500, "Error retrieving part data");
            }
        }

        #region Private Helper Methods

        private async Task LoadPartsDataAsync()
        {
            var query = _context.Parts.AsQueryable();

            if (ActiveOnly) query = query.Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var searchLower = SearchTerm.ToLower();
                query = query.Where(p => 
                    p.PartNumber.ToLower().Contains(searchLower) ||
                    p.Name.ToLower().Contains(searchLower) ||
                    p.Description.ToLower().Contains(searchLower));
            }

            if (!string.IsNullOrEmpty(MaterialFilter)) query = query.Where(p => p.Material == MaterialFilter);
            if (!string.IsNullOrEmpty(IndustryFilter)) query = query.Where(p => p.Industry == IndustryFilter);
            if (!string.IsNullOrEmpty(CategoryFilter)) query = query.Where(p => p.PartCategory == CategoryFilter);

            query = ApplySorting(query);

            TotalCount = await query.CountAsync();

            Parts = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        private IQueryable<Part> ApplySorting(IQueryable<Part> query)
        {
            var ascending = SortDirection?.ToLower() != "desc";

            return SortBy?.ToLower() switch
            {
                "name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "material" => ascending ? query.OrderBy(p => p.Material) : query.OrderByDescending(p => p.Material),
                "industry" => ascending ? query.OrderBy(p => p.Industry) : query.OrderByDescending(p => p.Industry),
                "category" => ascending ? query.OrderBy(p => p.PartCategory) : query.OrderByDescending(p => p.PartCategory),
                "hours" => ascending ? query.OrderBy(p => p.EstimatedHours) : query.OrderByDescending(p => p.EstimatedHours),
                _ => ascending ? query.OrderBy(p => p.PartNumber) : query.OrderByDescending(p => p.PartNumber)
            };
        }

        private async Task LoadFilterOptionsAsync()
        {
            AvailableMaterials = await _context.Parts
                .Where(p => !string.IsNullOrEmpty(p.Material))
                .Select(p => p.Material)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            AvailableIndustries = await _context.Parts
                .Where(p => !string.IsNullOrEmpty(p.Industry))
                .Select(p => p.Industry)
                .Distinct()
                .OrderBy(i => i)
                .ToListAsync();

            AvailableCategories = await _context.Parts
                .Where(p => !string.IsNullOrEmpty(p.PartCategory))
                .Select(p => p.PartCategory)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        private async Task LoadStatisticsAsync()
        {
            var allParts = await _context.Parts.ToListAsync();
            
            ActivePartsCount = allParts.Count(p => p.IsActive);
            InactivePartsCount = allParts.Count(p => !p.IsActive);

            if (allParts.Any())
            {
                MostUsedMaterial = allParts
                    .Where(p => !string.IsNullOrEmpty(p.Material))
                    .GroupBy(p => p.Material)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A";

                AverageEstimatedHours = allParts.Average(p => p.GetEffectiveEstimatedHours());
            }
        }

        #endregion

        #region Helper Methods for Views

        public string GetSortDirection(string column)
        {
            if (SortBy?.ToLower() == column.ToLower())
            {
                return SortDirection?.ToLower() == "desc" ? "asc" : "desc";
            }
            return "asc";
        }

        public string GetSortIcon(string column)
        {
            if (SortBy?.ToLower() == column.ToLower())
            {
                return SortDirection?.ToLower() == "desc" ? "?" : "?";
            }
            return "?";
        }

        public string GetStatusBadgeClass(bool isActive)
        {
            return isActive ? "bg-success" : "bg-secondary";
        }

        public string GetPriorityBadgeClass(string partClass)
        {
            return partClass switch
            {
                "A" => "bg-danger",
                "B" => "bg-warning",
                "C" => "bg-info",
                _ => "bg-secondary"
            };
        }

        #endregion
    }
}