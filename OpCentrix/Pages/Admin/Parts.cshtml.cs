using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;
using System.Text.Json;

namespace OpCentrix.Pages.Admin
{
    /// <summary>
    /// REFACTORED: Parts management page now using MasterParts + StageDefinitions architecture
    /// Replaces old boolean flag system with dynamic stage-based approach
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class PartsModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PartsModel> _logger;

        public PartsModel(SchedulerContext context, ILogger<PartsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // REFACTORED: Main properties now use MasterParts
        public IList<MasterPart> MasterParts { get; set; } = new List<MasterPart>();

        [BindProperty]
        public MasterPart MasterPart { get; set; } = new MasterPart();

        // Stage management for forms
        [BindProperty]
        public string SelectedStageIds { get; set; } = "";
        
        [BindProperty]
        public string StageEstimatedHours { get; set; } = "";

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

        // REFACTORED: Statistics now based on MasterParts
        public List<string> AvailableMaterials { get; set; } = new();
        public List<string> AvailableApproaches { get; set; } = new();
        public int ActivePartsCount { get; set; } = 0;
        public int InactivePartsCount { get; set; } = 0;
        public string MostUsedMaterial { get; set; } = "N/A";
        public double AverageEstimatedHours { get; set; } = 0;

        // Form data
        public List<ProductionStage> AvailableStages { get; set; } = new();
        public Dictionary<int, List<StageDefinition>> PartStages { get; set; } = new();
        
        /// <summary>
        /// Learning status data for stage requirements keyed by ProductionStageId
        /// Contains: EstimateSource, ActualSampleCount, ActualAverageDurationHours
        /// </summary>
        public Dictionary<int, StageLearningInfo> StageLearningData { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("?? [PARTS] Loading refactored parts page with MasterParts");

                await LoadFormDataAsync();
                await LoadStatisticsAsync();
                await LoadPartsDataAsync();

                _logger.LogInformation("? [PARTS] Page loaded - {Count} master parts", MasterParts.Count);
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
            _logger.LogInformation("? [PARTS-{OperationId}] Loading add form", operationId);

            try
            {
                MasterPart = CreateDefaultMasterPart();
                await LoadFormDataAsync();

                _logger.LogInformation("? [PARTS-{OperationId}] Add form loaded", operationId);
                return Partial("Shared/_MasterPartForm", this);
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

                MasterPart = await _context.MasterParts
                    .Include(mp => mp.StageDefinitions.Where(sd => sd.IsActive))
                    .FirstOrDefaultAsync(mp => mp.Id == id);

                if (MasterPart == null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Master part not found: {PartId}", operationId, id);
                    return NotFound("Master part not found");
                }

                await LoadFormDataAsync();

                // Convert stage definitions to form data
                if (MasterPart.StageDefinitions.Any())
                {
                    // Get the actual ProductionStage IDs, not StageDefinition IDs
                    var stageNames = MasterPart.StageDefinitions.OrderBy(sd => sd.ExecutionOrder).Select(sd => sd.StageName).ToList();
                    var hours = MasterPart.StageDefinitions.OrderBy(sd => sd.ExecutionOrder).Select(sd => sd.EstimatedHoursPerPart.ToString("F1")).ToList();
                    
                    // Convert stage names to ProductionStage IDs
                    var productionStages = await _context.ProductionStages.Where(ps => stageNames.Contains(ps.Name)).ToListAsync();
                    var stageIds = stageNames.Select(stageName => 
                        productionStages.FirstOrDefault(ps => ps.Name == stageName)?.Id.ToString() ?? "0"
                    ).Where(id => id != "0").ToList();
                    
                    SelectedStageIds = string.Join(",", stageIds);
                    StageEstimatedHours = string.Join(",", hours);
                }

                // Load learning data for this part's stages
                var part = await _context.Parts.FirstOrDefaultAsync(p => p.PartNumber == MasterPart.PartNumber);
                if (part != null)
                {
                    var learningData = await _context.PartStageRequirements
                        .Where(psr => psr.PartId == part.Id && psr.IsActive)
                        .Select(psr => new StageLearningInfo
                        {
                            ProductionStageId = psr.ProductionStageId,
                            EstimateSource = psr.EstimateSource,
                            ActualSampleCount = psr.ActualSampleCount,
                            ActualAverageDurationHours = psr.ActualAverageDurationHours,
                            LastActualDurationHours = psr.LastActualDurationHours,
                            EstimatedHours = psr.EstimatedHours
                        })
                        .ToListAsync();

                    StageLearningData = learningData.ToDictionary(l => l.ProductionStageId, l => l);
                    _logger.LogInformation("?? [PARTS-{OperationId}] Loaded learning data for {Count} stages", operationId, StageLearningData.Count);
                }

                _logger.LogInformation("? [PARTS-{OperationId}] Edit form loaded with {StageCount} stages", operationId, MasterPart.StageDefinitions.Count);
                return Partial("Shared/_MasterPartForm", this);
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
            _logger.LogInformation("? [PARTS-{OperationId}] Creating master part: {PartNumber}", operationId, MasterPart.PartNumber);
            
            // DEBUG: Log received stage data
            _logger.LogInformation("?? [PARTS-{OperationId}] Stage data received - SelectedStageIds: '{SelectedStageIds}', StageEstimatedHours: '{StageEstimatedHours}'", 
                operationId, SelectedStageIds ?? "NULL", StageEstimatedHours ?? "NULL");

            try
            {
                // Check if this is an AJAX request
                var isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                _logger.LogInformation("?? [PARTS-{OperationId}] Request type - AJAX: {IsAjax}", operationId, isAjaxRequest);

                // CRITICAL FIX: Extract stage data from form if not bound properly
                if (string.IsNullOrEmpty(SelectedStageIds) && Request.HasFormContentType)
                {
                    SelectedStageIds = Request.Form["SelectedStageIds"].FirstOrDefault() ?? "";
                    StageEstimatedHours = Request.Form["StageEstimatedHours"].FirstOrDefault() ?? "";
                    _logger.LogWarning("?? [PARTS-{OperationId}] Stage data extracted from form - SelectedStageIds: '{SelectedStageIds}', StageEstimatedHours: '{StageEstimatedHours}'", 
                        operationId, SelectedStageIds, StageEstimatedHours);
                }

                // Validate part number uniqueness
                var existingPart = await _context.MasterParts.FirstOrDefaultAsync(mp => mp.PartNumber == MasterPart.PartNumber);
                if (existingPart != null)
                {
                    ModelState.AddModelError("MasterPart.PartNumber", $"Part number '{MasterPart.PartNumber}' already exists");
                    await LoadFormDataAsync();
                    
                    if (isAjaxRequest)
                    {
                        return new JsonResult(new { success = false, message = "Validation failed", errors = ModelState });
                    }
                    return Partial("Shared/_MasterPartForm", this);
                }

                // Set creation defaults
                MasterPart.CreatedDate = DateTime.UtcNow;
                MasterPart.LastModifiedDate = DateTime.UtcNow;
                MasterPart.CreatedBy = User.Identity?.Name ?? "System";
                MasterPart.LastModifiedBy = User.Identity?.Name ?? "System";

                // Ensure required fields have defaults
                if (string.IsNullOrEmpty(MasterPart.RequiredStages))
                    MasterPart.RequiredStages = "[]";

                _context.MasterParts.Add(MasterPart);
                await _context.SaveChangesAsync();

                // Create stage definitions
                await CreateStageDefinitionsAsync(MasterPart.Id, operationId);

                _logger.LogInformation("? [PARTS-{OperationId}] Master part created with ID: {Id}", operationId, MasterPart.Id);
                
                // Return appropriate response based on request type
                if (isAjaxRequest)
                {
                    return new JsonResult(new { success = true, message = $"Master part '{MasterPart.PartNumber}' created successfully!" });
                }
                else
                {
                    TempData["SuccessMessage"] = $"Master part '{MasterPart.PartNumber}' created successfully!";
                    return RedirectToPage();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error creating master part", operationId);
                ModelState.AddModelError("", $"Error creating part: {ex.Message}");
                await LoadFormDataAsync();
                
                var isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                if (isAjaxRequest)
                {
                    return new JsonResult(new { success = false, message = ex.Message, errors = ModelState });
                }
                return Partial("Shared/_MasterPartForm", this);
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PARTS-{OperationId}] Updating master part: {PartNumber} (ID: {PartId})", operationId, MasterPart.PartNumber, MasterPart.Id);
            
            // DEBUG: Log received stage data
            _logger.LogInformation("?? [PARTS-{OperationId}] Stage data received - SelectedStageIds: '{SelectedStageIds}', StageEstimatedHours: '{StageEstimatedHours}'", 
                operationId, SelectedStageIds ?? "NULL", StageEstimatedHours ?? "NULL");

            try
            {
                // Check if this is an AJAX request
                var isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                _logger.LogInformation("?? [PARTS-{OperationId}] Request type - AJAX: {IsAjax}", operationId, isAjaxRequest);

                // CRITICAL FIX: Extract stage data from form if not bound properly
                if (string.IsNullOrEmpty(SelectedStageIds) && Request.HasFormContentType)
                {
                    SelectedStageIds = Request.Form["SelectedStageIds"].FirstOrDefault() ?? "";
                    StageEstimatedHours = Request.Form["StageEstimatedHours"].FirstOrDefault() ?? "";
                    _logger.LogWarning("?? [PARTS-{OperationId}] Stage data extracted from form - SelectedStageIds: '{SelectedStageIds}', StageEstimatedHours: '{StageEstimatedHours}'", 
                        operationId, SelectedStageIds, StageEstimatedHours);
                }

                if (MasterPart.Id <= 0)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Invalid part ID for update: {PartId}", operationId, MasterPart.Id);
                    ModelState.AddModelError("", "Invalid part ID. Please try again.");
                    await LoadFormDataAsync();
                    
                    if (isAjaxRequest)
                    {
                        return new JsonResult(new { success = false, message = "Invalid part ID", errors = ModelState });
                    }
                    return Partial("Shared/_MasterPartForm", this);
                }

                var existingPart = await _context.MasterParts.FindAsync(MasterPart.Id);
                if (existingPart == null)
                {
                    ModelState.AddModelError("", "Master part not found. It may have been deleted by another user.");
                    await LoadFormDataAsync();
                    
                    if (isAjaxRequest)
                    {
                        return new JsonResult(new { success = false, message = "Master part not found", errors = ModelState });
                    }
                    return Partial("Shared/_MasterPartForm", this);
                }

                // Check for duplicate part number
                var duplicatePart = await _context.MasterParts
                    .Where(mp => mp.PartNumber == MasterPart.PartNumber && mp.Id != MasterPart.Id)
                    .FirstOrDefaultAsync();
                if (duplicatePart != null)
                {
                    ModelState.AddModelError("MasterPart.PartNumber", $"Part number '{MasterPart.PartNumber}' already exists");
                    await LoadFormDataAsync();
                    
                    if (isAjaxRequest)
                    {
                        return new JsonResult(new { success = false, message = "Part number already exists", errors = ModelState });
                    }
                    return Partial("Shared/_MasterPartForm", this);
                }

                // Update existing part
                existingPart.PartNumber = MasterPart.PartNumber;
                existingPart.Name = MasterPart.Name;
                existingPart.Description = MasterPart.Description;
                existingPart.Material = MasterPart.Material;
                existingPart.ManufacturingApproach = MasterPart.ManufacturingApproach;
                // legacy stacking fields retained but no longer primary logic
                existingPart.AllowStacking = MasterPart.AllowStacking;
                existingPart.SingleStackDurationHours = MasterPart.SingleStackDurationHours;
                existingPart.DoubleStackDurationHours = MasterPart.DoubleStackDurationHours;
                existingPart.TripleStackDurationHours = MasterPart.TripleStackDurationHours;
                existingPart.MaxStackCount = MasterPart.MaxStackCount;
                // SLS Build Configuration fields
                existingPart.PartsPerBuildSingle = MasterPart.PartsPerBuildSingle == 0 ? 1 : MasterPart.PartsPerBuildSingle;
                existingPart.PartsPerBuildDouble = MasterPart.EnableDoubleStack ? MasterPart.PartsPerBuildDouble : null;
                existingPart.PartsPerBuildTriple = MasterPart.EnableTripleStack ? MasterPart.PartsPerBuildTriple : null;
                existingPart.EnableDoubleStack = MasterPart.EnableDoubleStack;
                existingPart.EnableTripleStack = MasterPart.EnableTripleStack;
                existingPart.StageEstimateSingle = MasterPart.StageEstimateSingle;
                // Batch Stage Build Configuration fields
                existingPart.SlsBuildDurationHours = MasterPart.SlsBuildDurationHours;
                existingPart.SlsPartsPerBuild = MasterPart.SlsPartsPerBuild;
                existingPart.DepowderingDurationHours = MasterPart.DepowderingDurationHours;
                existingPart.DepowderingPartsPerBatch = MasterPart.DepowderingPartsPerBatch;
                existingPart.HeatTreatmentDurationHours = MasterPart.HeatTreatmentDurationHours;
                existingPart.HeatTreatmentPartsPerBatch = MasterPart.HeatTreatmentPartsPerBatch;
                existingPart.WireEdmDurationHours = MasterPart.WireEdmDurationHours;
                existingPart.WireEdmPartsPerSession = MasterPart.WireEdmPartsPerSession;
                existingPart.IsActive = MasterPart.IsActive;
                existingPart.LastModifiedDate = DateTime.UtcNow;
                existingPart.LastModifiedBy = User.Identity?.Name ?? "System";

                // Update stage definitions
                await UpdateStageDefinitionsAsync(existingPart.Id, operationId);

                await _context.SaveChangesAsync();

                _logger.LogInformation("? [PARTS-{OperationId}] Master part updated successfully", operationId);
                
                // Return appropriate response based on request type
                if (isAjaxRequest)
                {
                    return new JsonResult(new { success = true, message = $"Master part '{MasterPart.PartNumber}' updated successfully!" });
                }
                else
                {
                    TempData["SuccessMessage"] = $"Master part '{MasterPart.PartNumber}' updated successfully!";
                    return RedirectToPage();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error updating master part", operationId);
                ModelState.AddModelError("", $"Error updating part: {ex.Message}");
                await LoadFormDataAsync();
                
                var isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                if (isAjaxRequest)
                {
                    return new JsonResult(new { success = false, message = ex.Message, errors = ModelState });
                }
                return Partial("Shared/_MasterPartForm", this);
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("??? [PARTS-{OperationId}] Deleting master part ID: {PartId}", operationId, id);

            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Invalid part ID for deletion: {PartId}", operationId, id);
                    TempData["ErrorMessage"] = "Invalid part ID";
                    return RedirectToPage();
                }

                var masterPart = await _context.MasterParts.FindAsync(id);
                if (masterPart == null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Master part not found for deletion: ID {PartId}", operationId, id);
                    TempData["ErrorMessage"] = "Master part not found. It may have already been deleted.";
                    return RedirectToPage();
                }

                _context.MasterParts.Remove(masterPart);
                await _context.SaveChangesAsync();

                _logger.LogInformation("? [PARTS-{OperationId}] Master part deleted: {PartNumber} (ID: {PartId})", operationId, masterPart.PartNumber, masterPart.Id);
                TempData["SuccessMessage"] = $"Master part '{masterPart.PartNumber}' deleted successfully";

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error deleting master part ID: {PartId}", operationId, id);
                TempData["ErrorMessage"] = $"Error deleting part: {ex.Message}";
                return RedirectToPage();
            }
        }

        #region Helper Methods

        private async Task LoadFormDataAsync()
        {
            try
            {
                // Check if any production stages exist, if not seed defaults
                if (!await _context.ProductionStages.AnyAsync())
                {
                    _logger.LogInformation("[PARTS] No production stages found, seeding defaults");
                    await SeedDefaultProductionStagesAsync();
                }

                AvailableStages = await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .OrderBy(ps => ps.DisplayOrder)
                    .ThenBy(ps => ps.Name)
                    .ToListAsync();

                _logger.LogInformation("[PARTS] Form data loaded - {StageCount} available stages", AvailableStages.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PARTS] Error loading form data");
                AvailableStages = new List<ProductionStage>();
            }
        }

        private async Task SeedDefaultProductionStagesAsync()
        {
            var stages = new List<ProductionStage>
            {
                new() { Name = "SLS Printing", Description = "Selective Laser Sintering build", Department = "SLS", DisplayOrder = 1, DefaultDurationHours = 8.0, DefaultHourlyRate = 125m, DefaultSetupMinutes = 30, IsBatchStage = true, IsActive = true },
                new() { Name = "Powder Removal", Description = "Remove excess powder", Department = "SLS", DisplayOrder = 2, DefaultDurationHours = 1.0, DefaultHourlyRate = 85m, DefaultSetupMinutes = 15, IsBatchStage = true, IsActive = true },
                new() { Name = "Heat Treatment", Description = "Stress relief heat treatment", Department = "Post-Process", DisplayOrder = 3, DefaultDurationHours = 4.0, DefaultHourlyRate = 95m, DefaultSetupMinutes = 30, IsBatchStage = true, IsActive = true },
                new() { Name = "Wire EDM", Description = "Wire EDM cutting from plate", Department = "EDM", DisplayOrder = 4, DefaultDurationHours = 2.0, DefaultHourlyRate = 110m, DefaultSetupMinutes = 20, IsBatchStage = true, IsActive = true },
                new() { Name = "CNC Machining", Description = "CNC milling and turning", Department = "Machining", DisplayOrder = 5, DefaultDurationHours = 3.0, DefaultHourlyRate = 95m, DefaultSetupMinutes = 30, IsActive = true },
                new() { Name = "Surface Finishing", Description = "Surface treatment", Department = "Finishing", DisplayOrder = 6, DefaultDurationHours = 1.5, DefaultHourlyRate = 75m, DefaultSetupMinutes = 15, IsActive = true },
                new() { Name = "Quality Inspection", Description = "Dimensional inspection", Department = "Quality", DisplayOrder = 7, DefaultDurationHours = 0.5, DefaultHourlyRate = 85m, DefaultSetupMinutes = 10, IsActive = true },
                new() { Name = "Assembly", Description = "Component assembly", Department = "Assembly", DisplayOrder = 8, DefaultDurationHours = 1.0, DefaultHourlyRate = 75m, DefaultSetupMinutes = 15, IsActive = true }
            };
            _context.ProductionStages.AddRange(stages);
            await _context.SaveChangesAsync();
            _logger.LogInformation("[PARTS] Seeded {Count} default production stages", stages.Count);
        }

        private async Task LoadPartsDataAsync()
        {
            try
            {
                var query = _context.MasterParts
                    .Include(mp => mp.StageDefinitions.Where(sd => sd.IsActive))
                    .AsQueryable();

                if (ActiveOnly)
                    query = query.Where(mp => mp.IsActive);

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    var searchLower = SearchTerm.ToLower();
                    query = query.Where(mp =>
                        mp.PartNumber.ToLower().Contains(searchLower) ||
                        mp.Name.ToLower().Contains(searchLower) ||
                        mp.Description.ToLower().Contains(searchLower));
                }

                if (!string.IsNullOrEmpty(MaterialFilter))
                    query = query.Where(mp => mp.Material == MaterialFilter);

                if (!string.IsNullOrEmpty(StageFilter))
                {
                    query = query.Where(mp => mp.StageDefinitions.Any(sd => sd.StageName == StageFilter && sd.IsActive));
                }

                query = ApplySorting(query);

                TotalCount = await query.CountAsync();

                MasterParts = await query
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .AsNoTracking()
                    .ToListAsync();

                // Load stage definitions for display
                var partIds = MasterParts.Select(mp => mp.Id).ToList();
                var stageDefinitions = await _context.StageDefinitions
                    .Where(sd => partIds.Contains(sd.MasterPartId) && sd.IsActive)
                    .OrderBy(sd => sd.ExecutionOrder)
                    .ToListAsync();

                PartStages = stageDefinitions
                    .GroupBy(sd => sd.MasterPartId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                _logger.LogInformation("?? [PARTS] Loaded {PartsCount} master parts with stage definitions (Total: {TotalCount})", MasterParts.Count, TotalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS] Error loading parts data");
                TotalCount = 0;
                MasterParts = new List<MasterPart>();
                PartStages = new Dictionary<int, List<StageDefinition>>();
                throw;
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                var allParts = await _context.MasterParts.AsNoTracking().ToListAsync();

                ActivePartsCount = allParts.Count(mp => mp.IsActive);
                InactivePartsCount = allParts.Count(mp => !mp.IsActive);

                var materialGroups = allParts
                    .Where(mp => !string.IsNullOrEmpty(mp.Material))
                    .GroupBy(mp => mp.Material)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                MostUsedMaterial = materialGroups?.Key ?? "N/A";

                var partsWithHours = allParts.Where(mp => mp.SingleStackDurationHours.HasValue && mp.SingleStackDurationHours > 0);
                AverageEstimatedHours = partsWithHours.Any() ? partsWithHours.Average(mp => mp.SingleStackDurationHours!.Value) : 0;

                AvailableMaterials = allParts
                    .Where(mp => !string.IsNullOrEmpty(mp.Material))
                    .Select(mp => mp.Material)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                AvailableApproaches = allParts
                    .Where(mp => !string.IsNullOrEmpty(mp.ManufacturingApproach))
                    .Select(mp => mp.ManufacturingApproach)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToList();

                _logger.LogInformation("?? [PARTS] Statistics loaded - Active: {ActiveCount}, Inactive: {InactiveCount}",
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
                AvailableApproaches = new List<string>();
            }
        }

        private async Task CreateStageDefinitionsAsync(int masterPartId, string operationId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SelectedStageIds))
                {
                    _logger.LogInformation("?? [PARTS-{OperationId}] No stages selected for master part", operationId);
                    return;
                }

                var stageIds = SelectedStageIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s.Trim(), out var v) ? v : 0)
                    .Where(id => id > 0)
                    .ToList();

                var hours = new List<double>();
                if (!string.IsNullOrWhiteSpace(StageEstimatedHours))
                {
                    hours = StageEstimatedHours.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => double.TryParse(s.Trim(), out var v) ? v : 1.0)
                        .ToList();
                }

                if (!stageIds.Any())
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] No valid stage IDs found after parsing", operationId);
                    return;
                }

                // Ensure hours list matches stage count
                while (hours.Count < stageIds.Count)
                {
                    hours.Add(1.0); // Default 1 hour
                }

                var stageDefinitions = new List<StageDefinition>();

                // Get MasterPart to find or create corresponding Part for learning system
                var masterPart = await _context.MasterParts.FindAsync(masterPartId);
                if (masterPart == null)
                {
                    _logger.LogError("? [PARTS-{OperationId}] MasterPart not found: {MasterPartId}", operationId, masterPartId);
                    return;
                }

                // Find or create corresponding Part for learning system integration
                var part = await _context.Parts.FirstOrDefaultAsync(p => p.PartNumber == masterPart.PartNumber);
                if (part == null)
                {
                    // Create a corresponding Part entry for the learning system
                    part = CreatePartFromMasterPart(masterPart);
                    _context.Parts.Add(part);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("? [PARTS-{OperationId}] Created corresponding Part entry with ID: {PartId}", operationId, part.Id);
                }

                for (int i = 0; i < stageIds.Count; i++)
                {
                    var stageId = stageIds[i];
                    var estimatedHours = hours[i];

                    // Find the production stage to get its details
                    var productionStage = await _context.ProductionStages.FindAsync(stageId);
                    if (productionStage == null)
                    {
                        _logger.LogWarning("?? [PARTS-{OperationId}] Production stage not found: {StageId}", operationId, stageId);
                        continue;
                    }

                    var stageDefinition = new StageDefinition
                    {
                        MasterPartId = masterPartId,
                        StageName = productionStage.Name,
                        ExecutionOrder = i + 1, // Sequential order based on selection
                        EstimatedHoursPerPart = estimatedHours,
                        RequiredMachineType = GetMachineTypeForStage(productionStage.Name),
                        PreferredMachines = GetPreferredMachinesForStage(productionStage.Name),
                        StageConfiguration = GetDefaultConfigForStage(productionStage.Name),
                        IsRequired = true,
                        CanSkip = false,
                        SetupMinutes = 30,
                        TeardownMinutes = 15,
                        IsActive = true
                    };

                    stageDefinitions.Add(stageDefinition);

                        // Create corresponding PartStageRequirement for learning system
                        var existingRequirement = await _context.PartStageRequirements
                            .FirstOrDefaultAsync(psr => psr.PartId == part.Id && psr.ProductionStageId == stageId);

                        if (existingRequirement == null)
                        {
                            var partStageRequirement = new PartStageRequirement
                            {
                                PartId = part.Id,
                                ProductionStageId = stageId,
                                ExecutionOrder = i + 1,
                                IsRequired = true,
                                IsActive = true,
                                EstimatedHours = estimatedHours,
                                SetupTimeMinutes = productionStage.DefaultSetupMinutes,
                                EstimateSource = "Manual", // Initial values are manual
                                CreatedBy = User.Identity?.Name ?? "System",
                                LastModifiedBy = User.Identity?.Name ?? "System",
                                CreatedDate = DateTime.UtcNow,
                                LastModifiedDate = DateTime.UtcNow
                            };
                            _context.PartStageRequirements.Add(partStageRequirement);
                            _logger.LogInformation("? [PARTS-{OperationId}] Created PartStageRequirement for Part {PartId}, Stage {StageId} with {Hours}h", operationId, part.Id, stageId, estimatedHours);
                        }
                        else
                        {
                            // Update existing requirement - only if not auto-learned
                            if (existingRequirement.EstimateSource == "Manual")
                            {
                                existingRequirement.ExecutionOrder = i + 1;
                                existingRequirement.EstimatedHours = estimatedHours;
                                existingRequirement.IsActive = true;
                                existingRequirement.LastModifiedBy = User.Identity?.Name ?? "System";
                                existingRequirement.LastModifiedDate = DateTime.UtcNow;
                                _logger.LogInformation("?? [PARTS-{OperationId}] Updated PartStageRequirement for Part {PartId}, Stage {StageId}", operationId, part.Id, stageId);
                            }
                            else
                            {
                                // Preserve auto-learned estimate, just update order
                                existingRequirement.ExecutionOrder = i + 1;
                                existingRequirement.IsActive = true;
                                existingRequirement.LastModifiedBy = User.Identity?.Name ?? "System";
                                existingRequirement.LastModifiedDate = DateTime.UtcNow;
                                _logger.LogInformation("?? [PARTS-{OperationId}] Preserved auto-learned estimate for Part {PartId}, Stage {StageId} ({Hours}h from {Samples} samples)", 
                                    operationId, part.Id, stageId, existingRequirement.ActualAverageDurationHours, existingRequirement.ActualSampleCount);
                            }
                        }
                    }

                if (stageDefinitions.Any())
                {
                    _context.StageDefinitions.AddRange(stageDefinitions);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("? [PARTS-{OperationId}] Created {StageCount} stage definitions", operationId, stageDefinitions.Count);
                }
                else
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] No valid stage definitions created", operationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error creating stage definitions", operationId);
                throw;
            }
        }

        private async Task UpdateStageDefinitionsAsync(int masterPartId, string operationId)
        {
            try
            {
                // Remove existing stage definitions
                var existingStages = await _context.StageDefinitions
                    .Where(sd => sd.MasterPartId == masterPartId)
                    .ToListAsync();

                _context.StageDefinitions.RemoveRange(existingStages);

                // Create new stage definitions
                await CreateStageDefinitionsAsync(masterPartId, operationId);

                _logger.LogInformation("? [PARTS-{OperationId}] Updated stage definitions for master part {MasterPartId}", operationId, masterPartId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error updating stage definitions", operationId);
                throw;
                    }
                }

                /// <summary>
                /// Create a Part entity from a MasterPart for learning system integration
                /// </summary>
                private Part CreatePartFromMasterPart(MasterPart masterPart)
                {
                    return new Part
                    {
                        PartNumber = masterPart.PartNumber,
                        Name = masterPart.Name,
                        Description = masterPart.Description ?? "",
                        Material = masterPart.Material,
                        IsActive = masterPart.IsActive,
                        SlsMaterial = masterPart.Material,
                        ManufacturingStage = "Initial",
                        StageDetails = "Auto-created from MasterPart",
                        BTComponentType = "Standard",
                        BTFirearmCategory = "N/A",
                        ProcessType = masterPart.ManufacturingApproach,
                        RequiredMachineType = "SLS",
                        PreferredMachines = "TI1,TI2",
                        CreatedBy = User.Identity?.Name ?? "System",
                        LastModifiedBy = User.Identity?.Name ?? "System",
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow,
                        PowderSpecification = "Ti-6Al-4V Grade 5",
                        QualityStandards = "Standard",
                        ToleranceRequirements = "±0.1mm",
                        RequiredSkills = "SLS Operation",
                        RequiredCertifications = "Basic",
                        RequiredTooling = "Standard",
                        ConsumableMaterials = "Standard",
                        SupportStrategy = "Auto",
                        CustomerPartNumber = masterPart.PartNumber,
                        PartCategory = "Production",
                        PartClass = "A",
                        Industry = "Manufacturing",
                        Application = "General",
                        ProcessParameters = "{}",
                        QualityCheckpoints = "{}",
                        BuildFileTemplate = "",
                        CadFilePath = "",
                        CadFileVersion = "1.0",
                        AvgDuration = "8h",
                        Dimensions = "Standard",
                        SurfaceFinishRequirement = "Standard",
                        AdminOverrideBy = ""
                    };
                }

                private string GetMachineTypeForStage(string stageName)
                {
                    return stageName switch
            {
                "SLS Printing" => "SLS",
                "Heat Treatment" => "Furnace",
                "CNC Machining" => "CNC",
                "EDM Operations" => "EDM",
                "Assembly" => "Assembly",
                "Finishing" => "Finishing",
                "Quality Inspection" => "Inspection",
                _ => "General"
            };
        }

        private string GetPreferredMachinesForStage(string stageName)
        {
            return stageName switch
            {
                "SLS Printing" => "TI1,TI2,INC,FARTU Ti 1,FARTU Ti 2,FARTU Ti 3",
                "Heat Treatment" => "Heat Treatment Furnace",
                "CNC Machining" => "CNC1,CNC2,CNC3,CNC4,CNC5",
                "EDM Operations" => "EDM",
                "Assembly" => "",
                "Finishing" => "",
                "Quality Inspection" => "",
                _ => ""
            };
        }

        private string GetDefaultConfigForStage(string stageName)
        {
            return stageName switch
            {
                "SLS Printing" => @"{""LaserPower"":170,""ScanSpeed"":1000,""LayerThickness"":30,""HatchSpacing"":120,""BuildTemperature"":180,""ArgonPurity"":99.9}",
                "Heat Treatment" => @"{""Temperature"":650,""HoldTime"":4.0,""CoolingRate"":""Furnace Cool"",""Atmosphere"":""Argon""}",
                "CNC Machining" => @"{""SpindleSpeed"":2500,""FeedRate"":500,""CoolantType"":""Flood"",""Tolerance"":""±0.1mm"",""SurfaceFinish"":""Ra 3.2""}",
                "EDM Operations" => @"{""WireType"":""Brass"",""WireDiameter"":0.25,""CutSpeed"":5.0,""FlushPressure"":0.5,""CutOffHeight"":2.0}",
                "Assembly" => @"{""Components"":[],""Hardware"":[],""TorqueSpecs"":{},""TestPressure"":0,""FunctionTest"":false}",
                "Finishing" => @"{""SurfaceFinish"":""Ra 3.2"",""CoatingType"":""Cerakote"",""Color"":""Black"",""MaskingRequired"":false}",
                "Quality Inspection" => @"{""DimensionalCheck"":true,""SurfaceFinish"":true,""MaterialCert"":true,""FunctionTest"":true,""Documentation"":true}",
                _ => "{}"
            };
        }

        private IQueryable<MasterPart> ApplySorting(IQueryable<MasterPart> query)
        {
            var ascending = SortDirection?.ToLower() != "desc";

            return SortBy?.ToLower() switch
            {
                "name" => ascending ? query.OrderBy(mp => mp.Name) : query.OrderByDescending(mp => mp.Name),
                "material" => ascending ? query.OrderBy(mp => mp.Material) : query.OrderByDescending(mp => mp.Material),
                "hours" => ascending ? query.OrderBy(mp => mp.SingleStackDurationHours) : query.OrderByDescending(mp => mp.SingleStackDurationHours),
                _ => ascending ? query.OrderBy(mp => mp.PartNumber) : query.OrderByDescending(mp => mp.PartNumber)
            };
        }

        #endregion

        // UI Helper Methods
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
                return SortDirection?.ToLower() == "desc" 
                    ? "<i class='fas fa-sort-down'></i>" 
                    : "<i class='fas fa-sort-up'></i>";
            }
            return "<i class='fas fa-sort'></i>";
        }

        public string GetStatusBadgeClass(bool isActive)
        {
            return isActive ? "bg-success" : "bg-secondary";
        }

        public List<StageDefinition> GetPartStages(int masterPartId)
        {
            var stages = PartStages.ContainsKey(masterPartId) ? PartStages[masterPartId] : new List<StageDefinition>();
            return stages;
        }

        private MasterPart CreateDefaultMasterPart()
        {
            return new MasterPart
            {
                PartNumber = "",
                Name = "",
                Description = "",
                Material = "Ti-6Al-4V Grade 5",
                ManufacturingApproach = "SLS-Based",
                AllowStacking = false,
                SingleStackDurationHours = null, // user will enter observed
                MaxStackCount = 1,
                PartsPerBuildSingle = 1,
                EnableDoubleStack = false,
                EnableTripleStack = false,
                RequiredStages = "[]",
                IsActive = true,
                CreatedBy = User.Identity?.Name ?? "System",
                LastModifiedBy = User.Identity?.Name ?? "System"
            };
        }

        public async Task<IActionResult> OnPostFixLegacyPartsAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PARTS-{OperationId}] Fixing legacy parts without stages", operationId);

            try
            {
                // Find all parts without stages
                var partsWithoutStages = await _context.MasterParts
                    .Where(mp => !_context.StageDefinitions.Any(sd => sd.MasterPartId == mp.Id && sd.IsActive))
                    .ToListAsync();

                _logger.LogInformation("?? [PARTS-{OperationId}] Found {Count} parts without stages", operationId, partsWithoutStages.Count);

                var stagesAdded = 0;

                foreach (var masterPart in partsWithoutStages)
                {
                    _logger.LogInformation("?? [PARTS-{OperationId}] Adding default stages to part {PartNumber}", operationId, masterPart.PartNumber);

                    // Default stages based on manufacturing approach
                    var defaultStages = masterPart.ManufacturingApproach switch
                    {
                        "SLS-Based" => new[] { 
                            ("SLS Printing", 1, 8.0, "SLS", "TI1,TI2,INC"),
                            ("EDM Operations", 2, 2.0, "EDM", "EDM"),
                            ("Quality Inspection", 3, 0.5, "Inspection", "") 
                        },
                        "CNC-Based" => new[] {
                            ("CNC Machining", 1, 4.0, "CNC", "CNC1,CNC2,CNC3"),
                            ("Quality Inspection", 2, 0.5, "Inspection", "")
                        },
                        "Hybrid-Approach" => new[] {
                            ("SLS Printing", 1, 8.0, "SLS", "TI1,TI2,INC"),
                            ("CNC Machining", 2, 2.0, "CNC", "CNC1,CNC2"),
                            ("Quality Inspection", 3, 0.5, "Inspection", "")
                        },
                        _ => new[] {
                            ("SLS Printing", 1, 8.0, "SLS", "TI1,TI2,INC"),
                            ("Quality Inspection", 2, 0.5, "Inspection", "")
                        }
                    };

                    foreach (var (stageName, order, hours, machineType, preferredMachines) in defaultStages)
                    {
                        var stageDefinition = new StageDefinition
                        {
                            MasterPartId = masterPart.Id,
                            StageName = stageName,
                            ExecutionOrder = order,
                            EstimatedHoursPerPart = hours,
                            RequiredMachineType = machineType,
                            PreferredMachines = preferredMachines,
                            StageConfiguration = GetDefaultConfigForStage(stageName),
                            IsRequired = true,
                            CanSkip = false,
                            SetupMinutes = 30,
                            TeardownMinutes = 15,
                            IsActive = true
                        };

                        _context.StageDefinitions.Add(stageDefinition);
                        stagesAdded++;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("? [PARTS-{OperationId}] Fixed {PartCount} legacy parts, added {StageCount} stage definitions", 
                    operationId, partsWithoutStages.Count, stagesAdded);

                return new JsonResult(new { 
                    success = true, 
                    message = $"Successfully added default stages to {partsWithoutStages.Count} legacy parts ({stagesAdded} stages total)" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error fixing legacy parts", operationId);
                return new JsonResult(new { 
                                    success = false, 
                                    message = $"Error fixing legacy parts: {ex.Message}" 
                                });
                            }
                        }
                    }

                    /// <summary>
                    /// DTO for stage learning status information
                    /// </summary>
                    public class StageLearningInfo
                    {
                        public int ProductionStageId { get; set; }
                        public string EstimateSource { get; set; } = "Manual";
                        public int ActualSampleCount { get; set; }
                        public double? ActualAverageDurationHours { get; set; }
                        public double? LastActualDurationHours { get; set; }
                        public double? EstimatedHours { get; set; }
        
                        public bool IsLearned => EstimateSource == "Auto" && ActualSampleCount >= 3;
                        public bool HasData => ActualSampleCount > 0;
                    }
                }