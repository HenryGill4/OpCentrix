using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;
using OpCentrix.Services.Admin;

namespace OpCentrix.Pages.Admin
{
    /// <summary>
    /// PHASE 3: Parts management page with modernized lookup-driven form
    /// Successfully migrated all 4 parts to ComponentType/ComplianceCategory structure
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class PartsModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PartsModel> _logger;
        private readonly IPartStageService _partStageService;
        private readonly ComponentTypeService _componentTypeService;
        private readonly ComplianceCategoryService _complianceCategoryService;
        private readonly IPartAssetService _partAssetService;

        public PartsModel(
            SchedulerContext context,
            ILogger<PartsModel> logger,
            IPartStageService partStageService,
            ComponentTypeService componentTypeService,
            ComplianceCategoryService complianceCategoryService,
            IPartAssetService partAssetService)
        {
            _context = context;
            _logger = logger;
            _partStageService = partStageService;
            _componentTypeService = componentTypeService;
            _complianceCategoryService = complianceCategoryService;
            _partAssetService = partAssetService;
        }

        // Main properties
        public IList<Part> Parts { get; set; } = new List<Part>();

        [BindProperty]
        public Part Part { get; set; } = new Part();

        // Pagination
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 20;
        
        public int TotalCount { get; set; } = 0;
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        // Filters
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? MaterialFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? CategoryFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? StageFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? ComplexityFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public bool ActiveOnly { get; set; } = true;

        // Sorting
        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; } = "PartNumber";
        
        [BindProperty(SupportsGet = true)]
        public string? SortDirection { get; set; } = "asc";

        // Statistics
        public List<string> AvailableMaterials { get; set; } = new();
        public List<string> AvailableCategories { get; set; } = new();
        public int ActivePartsCount { get; set; } = 0;
        public int InactivePartsCount { get; set; } = 0;
        public string MostUsedMaterial { get; set; } = "N/A";
        public double AverageEstimatedHours { get; set; } = 0;

        // PHASE 3: Form data with lookup support
        public PartFormViewModel PartFormData { get; set; } = new PartFormViewModel();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("?? [PARTS] Loading parts page");

                await LoadFormDataAsync();
                await LoadStatisticsAsync();
                await LoadPartsDataAsync();

                _logger.LogInformation("? [PARTS] Page loaded - {Count} parts", Parts.Count);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS] Error loading page");
                TempData["ErrorMessage"] = "Failed to load parts. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnGetAddAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PARTS-{OperationId}] Loading add form", operationId);

            try
            {
                Part = CreateDefaultPart();
                PartFormData = await CreatePartFormViewModelAsync(Part);

                _logger.LogInformation("? [PARTS-{OperationId}] Add form loaded", operationId);
                return Partial("Shared/_PartForm", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error loading add form", operationId);
                return StatusCode(500, "Error loading form");
            }
        }

        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PARTS-{OperationId}] Loading edit form for ID: {PartId}", operationId, id);

            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Invalid part ID: {PartId}", operationId, id);
                    return BadRequest("Invalid part ID");
                }

                Part = await _context.Parts
                    .Include(p => p.ComponentType)
                    .Include(p => p.ComplianceCategory)
                    .Include(p => p.AssetLinks)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (Part == null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Part not found: {PartId}", operationId, id);
                    return NotFound("Part not found");
                }

                PartFormData = await CreatePartFormViewModelAsync(Part);

                _logger.LogInformation("? [PARTS-{OperationId}] Edit form loaded", operationId);
                return Partial("Shared/_PartForm", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error loading edit form for ID: {PartId}", operationId, id);
                return StatusCode(500, "Error loading form");
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("? [PARTS-{OperationId}] Creating part: {PartNumber}", operationId, Part.PartNumber);

            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    _logger.LogWarning("?? [PARTS-{OperationId}] Validation failed: {Errors}", operationId, string.Join("; ", errors));
                    return await HandleValidationError("Please fix the validation errors and try again.");
                }

                // Check for duplicate part number
                var existingPart = await _context.Parts
                    .Where(p => p.PartNumber == Part.PartNumber)
                    .FirstOrDefaultAsync();

                if (existingPart != null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Duplicate part number: {PartNumber}", operationId, Part.PartNumber);
                    ModelState.AddModelError("Part.PartNumber", $"Part number '{Part.PartNumber}' already exists");
                    return await HandleValidationError("Part number already exists. Please choose a different part number.");
                }

                // Set defaults
                SetPartDefaults(Part, isNew: true);
                Part.IsLegacyForm = false; // Mark as modern form

                _context.Parts.Add(Part);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("? [PARTS-{OperationId}] Part created: {PartNumber} (ID: {PartId})", operationId, Part.PartNumber, Part.Id);
                    return await HandleFormSuccess($"Part '{Part.PartNumber}' created successfully!");
                }
                else
                {
                    _logger.LogError("? [PARTS-{OperationId}] No rows affected during creation", operationId);
                    return await HandleValidationError("Failed to create part. No changes were made to the database.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error creating part", operationId);
                return await HandleValidationError($"Unexpected error: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PARTS-{OperationId}] Updating part: {PartNumber} (ID: {PartId})", operationId, Part.PartNumber, Part.Id);

            try
            {
                if (Part.Id <= 0)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Invalid part ID for update: {PartId}", operationId, Part.Id);
                    return await HandleValidationError("Invalid part ID. Please try again.");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    _logger.LogWarning("?? [PARTS-{OperationId}] Validation failed: {Errors}", operationId, string.Join("; ", errors));
                    return await HandleValidationError("Please fix the validation errors and try again.");
                }

                var existingPart = await _context.Parts.FindAsync(Part.Id);
                if (existingPart == null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Part not found for update: ID {PartId}", operationId, Part.Id);
                    return await HandleValidationError("Part not found. It may have been deleted by another user.");
                }

                // Check for duplicate part number (excluding current part)
                var duplicatePart = await _context.Parts
                    .Where(p => p.PartNumber == Part.PartNumber && p.Id != Part.Id)
                    .FirstOrDefaultAsync();

                if (duplicatePart != null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Duplicate part number: {PartNumber}", operationId, Part.PartNumber);
                    ModelState.AddModelError("Part.PartNumber", $"Part number '{Part.PartNumber}' already exists");
                    return await HandleValidationError("Part number already exists. Please choose a different part number.");
                }

                // Update fields while preserving creation data
                SetPartDefaults(Part, isNew: false, existingPart);
                Part.IsLegacyForm = false; // Mark as modern form

                _context.Entry(existingPart).CurrentValues.SetValues(Part);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("? [PARTS-{OperationId}] Part updated: {PartNumber} (ID: {PartId})", operationId, Part.PartNumber, Part.Id);
                    return await HandleFormSuccess($"Part '{Part.PartNumber}' updated successfully!");
                }
                else
                {
                    _logger.LogError("? [PARTS-{OperationId}] No rows affected during update", operationId);
                    return await HandleValidationError("Failed to update part. No changes were detected.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error updating part", operationId);
                return await HandleValidationError($"Unexpected error: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("??? [PARTS-{OperationId}] Deleting part ID: {PartId}", operationId, id);

            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Invalid part ID for deletion: {PartId}", operationId, id);
                    TempData["ErrorMessage"] = "Invalid part ID";
                    return RedirectToPage();
                }

                var part = await _context.Parts.FindAsync(id);
                if (part == null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Part not found for deletion: ID {PartId}", operationId, id);
                    TempData["ErrorMessage"] = "Part not found. It may have already been deleted.";
                    return RedirectToPage();
                }

                _context.Parts.Remove(part);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("? [PARTS-{OperationId}] Part deleted: {PartNumber} (ID: {PartId})", operationId, part.PartNumber, part.Id);
                    TempData["SuccessMessage"] = $"Part '{part.PartNumber}' deleted successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete part. No changes were made.";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error deleting part ID: {PartId}", operationId, id);
                TempData["ErrorMessage"] = $"Error deleting part: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetPartStagesAsync(int partId)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PARTS-{OperationId}] Loading stages for part ID: {PartId}", operationId, partId);

            try
            {
                if (partId <= 0)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Invalid part ID: {PartId}", operationId, partId);
                    return BadRequest("Invalid part ID");
                }

                var stages = await _partStageService.GetPartStagesWithDetailsAsync(partId);

                _logger.LogInformation("? [PARTS-{OperationId}] Loaded {StageCount} stages for part {PartId}", operationId, stages.Count, partId);
                return new JsonResult(stages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error loading stages for part ID: {PartId}", operationId, partId);
                return StatusCode(500, "Error loading stages");
            }
        }

        public async Task<IActionResult> OnPostAddStageAsync([FromBody] AddStageRequest request)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("? [PARTS-{OperationId}] Adding stage {StageId} to part {PartId}", operationId, request.StageId, request.PartId);

            try
            {
                if (request.PartId <= 0 || request.StageId <= 0)
                {
                    return BadRequest("Invalid part or stage ID");
                }

                var partStageRequirement = new PartStageRequirement
                {
                    PartId = request.PartId,
                    ProductionStageId = request.StageId,
                    ExecutionOrder = request.ExecutionOrder,
                    EstimatedHours = request.EstimatedHours,
                    SetupTimeMinutes = request.SetupTimeMinutes,
                    HourlyRateOverride = request.HourlyRateOverride,
                    MaterialCost = request.MaterialCost,
                    IsRequired = request.IsRequired,
                    RequirementNotes = request.RequirementNotes ?? "",
                    SpecialInstructions = request.SpecialInstructions ?? "",
                    IsActive = true,
                    CreatedBy = User.Identity?.Name ?? "System",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = User.Identity?.Name ?? "System",
                    LastModifiedDate = DateTime.UtcNow
                };

                var success = await _partStageService.AddPartStageAsync(partStageRequirement);

                if (success)
                {
                    _logger.LogInformation("? [PARTS-{OperationId}] Stage added successfully", operationId);
                    return new JsonResult(new { success = true, message = "Stage added successfully" });
                }
                else
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Failed to add stage", operationId);
                    return new JsonResult(new { success = false, message = "Failed to add stage" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error adding stage", operationId);
                return StatusCode(500, "Error adding stage");
            }
        }

        public async Task<IActionResult> OnPostRemoveStageAsync([FromBody] RemoveStageRequest request)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("??? [PARTS-{OperationId}] Removing stage {StageId} from part {PartId}", operationId, request.StageId, request.PartId);

            try
            {
                if (request.PartId <= 0 || request.StageId <= 0)
                {
                    return BadRequest("Invalid part or stage ID");
                }

                var success = await _partStageService.RemovePartStageAsync(request.PartId, request.StageId);

                if (success)
                {
                    _logger.LogInformation("? [PARTS-{OperationId}] Stage removed successfully", operationId);
                    return new JsonResult(new { success = true, message = "Stage removed successfully" });
                }
                else
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Failed to remove stage", operationId);
                    return new JsonResult(new { success = false, message = "Failed to remove stage" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error removing stage", operationId);
                return StatusCode(500, "Error removing stage");
            }
        }

        #region Helper Methods

        private async Task LoadFormDataAsync()
        {
            try
            {
                var componentTypes = await _componentTypeService.GetActiveComponentTypesAsync();
                var complianceCategories = await _complianceCategoryService.GetActiveCategoriesAsync();
                var availableStages = await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .OrderBy(ps => ps.Name)
                    .AsNoTracking()
                    .ToListAsync();

                PartFormData = new PartFormViewModel
                {
                    Part = Part ?? CreateDefaultPart(),
                    ComponentTypes = componentTypes,
                    ComplianceCategories = complianceCategories,
                    AvailableStages = availableStages,
                    ExistingStages = new List<PartStageRequirement>(),
                    ExistingAssets = new List<PartAssetLink>()
                };

                _logger.LogInformation("? [PARTS] Form data loaded - {ComponentTypesCount} component types, {ComplianceCategoriesCount} compliance categories",
                    componentTypes.Count, complianceCategories.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS] Error loading form data");
                PartFormData = new PartFormViewModel
                {
                    Part = Part ?? CreateDefaultPart(),
                    ComponentTypes = new List<ComponentType>(),
                    ComplianceCategories = new List<ComplianceCategory>(),
                    AvailableStages = new List<ProductionStage>(),
                    ExistingStages = new List<PartStageRequirement>(),
                    ExistingAssets = new List<PartAssetLink>()
                };
            }
        }

        private async Task<PartFormViewModel> CreatePartFormViewModelAsync(Part part)
        {
            var viewModel = new PartFormViewModel
            {
                Part = part,
                ComponentTypes = await _componentTypeService.GetActiveComponentTypesAsync(),
                ComplianceCategories = await _complianceCategoryService.GetActiveCategoriesAsync(),
                AvailableStages = await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .OrderBy(ps => ps.Name)
                    .ToListAsync(),
                AvailableMaterials = await _context.Parts
                    .Where(p => !string.IsNullOrEmpty(p.Material))
                    .Select(p => p.Material)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToListAsync()
            };

            if (part.Id > 0)
            {
                try
                {
                    // Load existing assets
                    viewModel.ExistingAssets = await _partAssetService.GetPartAssetsAsync(part.Id);
                    
                    // Load existing stages using the correct service method
                    viewModel.ExistingStages = await _partStageService.GetPartStagesWithDetailsAsync(part.Id);
                    
                    _logger.LogInformation("? [PARTS] Loaded {AssetCount} assets and {StageCount} stages for part {PartId}", 
                        viewModel.ExistingAssets.Count, viewModel.ExistingStages.Count, part.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "? [PARTS] Error loading part assets/stages for part {PartId}", part.Id);
                    viewModel.ExistingAssets = new List<PartAssetLink>();
                    viewModel.ExistingStages = new List<PartStageRequirement>();
                }
            }
            else
            {
                viewModel.ExistingAssets = new List<PartAssetLink>();
                viewModel.ExistingStages = new List<PartStageRequirement>();
            }

            return viewModel;
        }

        private Part CreateDefaultPart()
        {
            return new Part
            {
                Id = 0,
                PartNumber = "",
                Name = "",
                Description = "",
                Material = "Ti-6Al-4V Grade 5",
                SlsMaterial = "Ti-6Al-4V Grade 5",
                EstimatedHours = 8.0,
                MaterialCostPerKg = 450.00m,
                StandardLaborCostPerHour = 85.00m,
                PartCategory = "Production",
                ProcessType = "SLS Metal",
                SurfaceFinishRequirement = "As-built",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "System",
                ComponentTypeId = 1, // Default to "General"
                ComplianceCategoryId = 1, // Default to "Non NFA"
                IsLegacyForm = false,
                CustomerPartNumber = "",
                Dimensions = "",
                AdminOverrideReason = "",
                AdminOverrideBy = "",
                WeightGrams = 0,
                VolumeMm3 = 0,
                HeightMm = 0,
                LengthMm = 0,
                WidthMm = 0,
                MaxSurfaceRoughnessRa = 25
            };
        }

        private void SetPartDefaults(Part part, bool isNew, Part? existingPart = null)
        {
            if (isNew)
            {
                part.Id = 0;
                part.CreatedDate = DateTime.UtcNow;
                part.CreatedBy = User.Identity?.Name ?? "System";
            }
            else if (existingPart != null)
            {
                part.CreatedDate = existingPart.CreatedDate;
                part.CreatedBy = existingPart.CreatedBy;
            }

            part.LastModifiedDate = DateTime.UtcNow;
            part.LastModifiedBy = User.Identity?.Name ?? "System";

            part.CustomerPartNumber ??= "";
            part.AdminOverrideReason ??= "";
            part.AdminOverrideBy ??= "";

            if (part.MaterialCostPerKg <= 0) part.MaterialCostPerKg = 450.00m;
            if (part.StandardLaborCostPerHour <= 0) part.StandardLaborCostPerHour = 85.00m;
        }

        private async Task<IActionResult> HandleValidationError(string message)
        {
            ModelState.AddModelError("", message);
            PartFormData = await CreatePartFormViewModelAsync(Part);
            return Partial("Shared/_PartForm", this);
        }

        private async Task<IActionResult> HandleFormSuccess(string message)
        {
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return Content($@"
                    <script>
                        console.log('? [PARTS] Part saved successfully');
                        
                        if (typeof hideModal === 'function') {{ hideModal(); }}
                        if (typeof showNotification === 'function') {{ showNotification('{message}', 'success'); }}
                        
                        setTimeout(() => {{ window.location.reload(); }}, 100);
                    </script>
                ", "text/html");
            }
            else
            {
                TempData["SuccessMessage"] = message;
                return RedirectToPage();
            }
        }

        private async Task LoadPartsDataAsync()
        {
            try
            {
                var query = _context.Parts
                    .Include(p => p.ComponentType)
                    .Include(p => p.ComplianceCategory)
                    .AsQueryable();

                if (ActiveOnly)
                    query = query.Where(p => p.IsActive);

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    var searchLower = SearchTerm.ToLower();
                    query = query.Where(p =>
                        p.PartNumber.ToLower().Contains(searchLower) ||
                        p.Name.ToLower().Contains(searchLower) ||
                        (p.Description != null && p.Description.ToLower().Contains(searchLower)));
                }

                if (!string.IsNullOrEmpty(MaterialFilter))
                    query = query.Where(p => p.Material == MaterialFilter);

                if (!string.IsNullOrEmpty(CategoryFilter))
                    query = query.Where(p => p.PartCategory == CategoryFilter);

                query = ApplySorting(query);

                TotalCount = await query.CountAsync();

                Parts = await query
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("? [PARTS] Loaded {PartsCount} parts (Total: {TotalCount})", Parts.Count, TotalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS] Error loading parts data");
                TotalCount = 0;
                Parts = new List<Part>();
                throw;
            }
        }

        private IQueryable<Part> ApplySorting(IQueryable<Part> query)
        {
            var ascending = SortDirection?.ToLower() != "desc";

            return SortBy?.ToLower() switch
            {
                "name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "material" => ascending ? query.OrderBy(p => p.Material) : query.OrderByDescending(p => p.Material),
                "category" => ascending ? query.OrderBy(p => p.PartCategory) : query.OrderByDescending(p => p.PartCategory),
                "hours" => ascending ? query.OrderBy(p => p.EstimatedHours) : query.OrderByDescending(p => p.EstimatedHours),
                _ => ascending ? query.OrderBy(p => p.PartNumber) : query.OrderByDescending(p => p.PartNumber)
            };
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                var allParts = await _context.Parts.AsNoTracking().ToListAsync();

                ActivePartsCount = allParts.Count(p => p.IsActive);
                InactivePartsCount = allParts.Count(p => !p.IsActive);

                var materialGroups = allParts
                    .Where(p => !string.IsNullOrEmpty(p.Material))
                    .GroupBy(p => p.Material)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                MostUsedMaterial = materialGroups?.Key ?? "N/A";

                var partsWithHours = allParts.Where(p => p.EstimatedHours > 0);
                AverageEstimatedHours = partsWithHours.Any() ? partsWithHours.Average(p => p.EstimatedHours) : 0;

                AvailableMaterials = allParts
                    .Where(p => !string.IsNullOrEmpty(p.Material))
                    .Select(p => p.Material)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                AvailableCategories = allParts
                    .Where(p => !string.IsNullOrEmpty(p.PartCategory))
                    .Select(p => p.PartCategory)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                _logger.LogInformation("? [PARTS] Statistics loaded - Active: {ActiveCount}, Inactive: {InactiveCount}",
                    ActivePartsCount, InactivePartsCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS] Error loading statistics");
                ActivePartsCount = 0;
                InactivePartsCount = 0;
                MostUsedMaterial = "N/A";
                AverageEstimatedHours = 0;
                AvailableMaterials = new List<string>();
                AvailableCategories = new List<string>();
            }
        }

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

        #endregion
    }

    /// <summary>
    /// PHASE 3: Enhanced ViewModel for modernized form with lookup support
    /// </summary>
    public class PartFormViewModel
    {
        public Part Part { get; set; } = new Part();
        public List<string> AvailableMaterials { get; set; } = new List<string>();
        public List<ProductionStage> AvailableStages { get; set; } = new List<ProductionStage>();
        public List<ComponentType> ComponentTypes { get; set; } = new List<ComponentType>();
        public List<ComplianceCategory> ComplianceCategories { get; set; } = new List<ComplianceCategory>();
        public List<PartStageRequirement> ExistingStages { get; set; } = new List<PartStageRequirement>();
        public List<PartAssetLink> ExistingAssets { get; set; } = new List<PartAssetLink>();
        public Dictionary<string, int> AssetStatistics { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// Request model for adding a stage to a part
    /// </summary>
    public class AddStageRequest
    {
        public int PartId { get; set; }
        public int StageId { get; set; }
        public int ExecutionOrder { get; set; } = 1;
        public double EstimatedHours { get; set; } = 1.0;
        public int SetupTimeMinutes { get; set; } = 30;
        public decimal? HourlyRateOverride { get; set; }
        public decimal MaterialCost { get; set; } = 0.00m;
        public bool IsRequired { get; set; } = true;
        public string? RequirementNotes { get; set; }
        public string? SpecialInstructions { get; set; }
    }

    /// <summary>
    /// Request model for removing a stage from a part
    /// </summary>
    public class RemoveStageRequest
    {
        public int PartId { get; set; }
        public int StageId { get; set; }
    }
}