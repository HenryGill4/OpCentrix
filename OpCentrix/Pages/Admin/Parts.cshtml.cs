using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;
using OpCentrix.Services.Admin; // FIXED: Add Admin namespace
using System.Text.Json;

namespace OpCentrix.Pages.Admin
{
    /// <summary>
    /// Parts management page with comprehensive CRUD operations and stage management
    /// Updated to work with the new PartStageRequirements and ProductionStages schema
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class PartsModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PartsModel> _logger;
        private readonly IPartStageService _partStageService;
        private readonly OpCentrix.Services.Admin.IProductionStageSeederService _productionStageSeeder; // FIXED: Use fully qualified name

        public PartsModel(
            SchedulerContext context,
            ILogger<PartsModel> logger,
            IPartStageService partStageService,
            OpCentrix.Services.Admin.IProductionStageSeederService productionStageSeeder) // FIXED: Use fully qualified name
        {
            _context = context;
            _logger = logger;
            _partStageService = partStageService;
            _productionStageSeeder = productionStageSeeder;
        }

        // Main data properties
        public IList<Part> Parts { get; set; } = new List<Part>();
        
        // Form property for modal
        [BindProperty]
        public Part Part { get; set; } = new Part();
        
        // Pagination and filtering properties
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        
        [BindProperty(SupportsGet = true)]
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
        public string? ComplexityFilter { get; set; }
        
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
        public List<string> AvailableComplexityLevels { get; set; } = new List<string> { "Simple", "Medium", "Complex", "Very Complex" };
        
        // Statistics
        public int ActivePartsCount { get; set; }
        public int InactivePartsCount { get; set; }
        public string MostUsedMaterial { get; set; } = string.Empty;
        public double AverageEstimatedHours { get; set; }
        
        // Stage-related properties
        public List<ProductionStage> AvailableStages { get; set; } = new List<ProductionStage>();
        public Dictionary<int, List<PartStageRequirement>> PartStages { get; set; } = new Dictionary<int, List<PartStageRequirement>>();
        public List<Machine> AvailableMachines { get; set; } = new List<Machine>();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Ensure default production stages exist
                await EnsureDefaultStagesExist();
                
                await LoadPartsDataAsync();
                await LoadFilterOptionsAsync();
                await LoadStatisticsAsync();
                await LoadStageDataAsync();
                await LoadMachinesDataAsync();
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading parts data");
                TempData["ErrorMessage"] = "Error loading parts data. Please try again.";
                return Page();
            }
        }

        /// <summary>
        /// Get add part modal form
        /// </summary>
        public async Task<IActionResult> OnGetAddAsync()
        {
            try
            {
                _logger.LogInformation("Loading add part modal");
                
                // Initialize new part with defaults
                Part = new Part
                {
                    Id = 0,
                    PartNumber = "",
                    Name = "",
                    Description = "",
                    Industry = "General Manufacturing",
                    Application = "General Component",
                    Material = "Ti-6Al-4V Grade 5",
                    SlsMaterial = "Ti-6Al-4V Grade 5",
                    EstimatedHours = 8.0,
                    MaterialCostPerKg = 450.00m,
                    StandardLaborCostPerHour = 85.00m,
                    MachineOperatingCostPerHour = 125.00m,
                    PartCategory = "Production",
                    PartClass = "B",
                    ProcessType = "SLS Metal",
                    IsActive = true,
                    RequiresSLSPrinting = true,
                    RequiresInspection = true,
                    RecommendedLaserPower = 200,
                    RecommendedScanSpeed = 1200,
                    RecommendedLayerThickness = 30,
                    RecommendedHatchSpacing = 120,
                    RecommendedBuildTemperature = 180,
                    RequiredArgonPurity = 99.9,
                    MaxOxygenContent = 50,
                    SetupTimeMinutes = 45,
                    PreheatingTimeMinutes = 60,
                    CoolingTimeMinutes = 240,
                    PostProcessingTimeMinutes = 45,
                    SetupCost = 150.00m,
                    PostProcessingCost = 75.00m,
                    QualityInspectionCost = 50.00m,
                    ArgonCostPerHour = 15.00m,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                // Load stage data for new part
                await LoadStageDataForPart(Part);

                return Partial("Shared/_PartForm", Part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading add part form");
                return StatusCode(500, "Error loading form");
            }
        }

        /// <summary>
        /// Get edit part modal form
        /// </summary>
        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            try
            {
                _logger.LogInformation("Loading edit part modal for ID: {PartId}", id);
                
                Part = await _context.Parts
                    .Include(p => p.PartClassification)
                    .FirstOrDefaultAsync(p => p.Id == id);
                    
                if (Part == null)
                {
                    _logger.LogWarning("Part with ID {PartId} not found", id);
                    return NotFound();
                }

                // Load stage data for existing part
                await LoadStageDataForPart(Part);

                return Partial("Shared/_PartForm", Part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit part form for ID: {PartId}", id);
                return StatusCode(500, "Error loading form");
            }
        }

        /// <summary>
        /// Create new part - Enhanced with stage management
        /// </summary>
        public async Task<IActionResult> OnPostCreateAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PARTS-{OperationId}] Creating new part: {PartNumber}", operationId, Part?.PartNumber);

            try
            {
                if (Part == null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Part data is null", operationId);
                    ModelState.AddModelError("", "Part data is required");
                    return Partial("Shared/_PartForm", new Part());
                }

                // Validate required fields
                var validationErrors = ValidatePartData(Part);
                if (validationErrors.Any())
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Validation failed: {Errors}", operationId, string.Join(", ", validationErrors));
                    
                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }
                    await LoadStageDataForPart(Part);
                    return Partial("Shared/_PartForm", Part);
                }

                // Check for duplicate part number
                var existingPart = await _context.Parts
                    .FirstOrDefaultAsync(p => p.PartNumber == Part.PartNumber);
                
                if (existingPart != null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Duplicate part number: {PartNumber}", operationId, Part.PartNumber);
                    ModelState.AddModelError("Part.PartNumber", $"Part number '{Part.PartNumber}' already exists");
                    await LoadStageDataForPart(Part);
                    return Partial("Shared/_PartForm", Part);
                }

                // Set audit fields
                Part.Id = 0; // Ensure new record
                Part.CreatedDate = DateTime.UtcNow;
                Part.CreatedBy = User.Identity?.Name ?? "System";
                Part.LastModifiedDate = DateTime.UtcNow;
                Part.LastModifiedBy = User.Identity?.Name ?? "System";

                // Use transaction for data integrity
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Add to database
                    _context.Parts.Add(Part);
                    var saveResult = await _context.SaveChangesAsync();
                    
                    if (saveResult > 0)
                    {
                        // Create default stage requirements based on boolean flags
                        await CreateDefaultStageRequirements(Part);
                        
                        await transaction.CommitAsync();
                        
                        _logger.LogInformation("? [PARTS-{OperationId}] Part created successfully: {PartNumber} (ID: {PartId})", 
                            operationId, Part.PartNumber, Part.Id);

                        // Return success response
                        var successScript = $@"
                            <script>
                                console.log('? Part created successfully: {Part.PartNumber}');
                                
                                // Close modal
                                const modal = document.getElementById('partModal');
                                if (modal && typeof bootstrap !== 'undefined') {{
                                    const bsModal = bootstrap.Modal.getInstance(modal);
                                    if (bsModal) bsModal.hide();
                                }}
                                
                                // Show success message
                                if (typeof window.showToast === 'function') {{
                                    window.showToast('success', 'Part ""{Part.PartNumber}"" created successfully!');
                                }}
                                
                                // Redirect after delay
                                setTimeout(() => {{
                                    console.log('?? Redirecting to refresh parts list');
                                    window.location.href = '/Admin/Parts';
                                }}, 1500);
                            </script>";

                        return Content(successScript, "text/html");
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning("?? [PARTS-{OperationId}] No rows affected during part creation", operationId);
                        ModelState.AddModelError("", "Failed to create part. No changes were made to the database.");
                        await LoadStageDataForPart(Part);
                        return Partial("Shared/_PartForm", Part);
                    }
                }
                catch (Exception dbEx)
                {
                    await transaction.RollbackAsync();
                    throw; // Re-throw to be caught by outer catch
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "? [PARTS-{OperationId}] Database error creating part: {PartNumber}", operationId, Part?.PartNumber);
                
                var errorMessage = "A database error occurred while creating the part.";
                if (dbEx.Message.Contains("UNIQUE") || dbEx.Message.Contains("duplicate"))
                {
                    errorMessage = "A part with this number or unique identifier already exists.";
                }
                
                ModelState.AddModelError("", errorMessage);
                await LoadStageDataForPart(Part);
                return Partial("Shared/_PartForm", Part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Unexpected error creating part: {PartNumber}", operationId, Part?.PartNumber);
                ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                await LoadStageDataForPart(Part);
                return Partial("Shared/_PartForm", Part);
            }
        }

        /// <summary>
        /// Update existing part - Enhanced with stage management
        /// </summary>
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PARTS-{OperationId}] Updating part: {PartNumber} (ID: {PartId})", 
                operationId, Part?.PartNumber, Part?.Id);

            try
            {
                if (Part == null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Part data is null", operationId);
                    ModelState.AddModelError("", "Part data is required");
                    return Partial("Shared/_PartForm", new Part());
                }

                // Validate required fields
                var validationErrors = ValidatePartData(Part);
                if (validationErrors.Any())
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Validation failed: {Errors}", operationId, string.Join(", ", validationErrors));
                    
                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }
                    await LoadStageDataForPart(Part);
                    return Partial("Shared/_PartForm", Part);
                }

                // Get existing part
                var existingPart = await _context.Parts.FindAsync(Part.Id);
                if (existingPart == null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Part not found for update: ID {PartId}", operationId, Part.Id);
                    ModelState.AddModelError("", "Part not found. It may have been deleted.");
                    await LoadStageDataForPart(Part);
                    return Partial("Shared/_PartForm", Part);
                }

                // Check for duplicate part number (excluding current part)
                var duplicatePart = await _context.Parts
                    .FirstOrDefaultAsync(p => p.PartNumber == Part.PartNumber && p.Id != Part.Id);
                
                if (duplicatePart != null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Duplicate part number: {PartNumber}", operationId, Part.PartNumber);
                    ModelState.AddModelError("Part.PartNumber", $"Part number '{Part.PartNumber}' already exists");
                    await LoadStageDataForPart(Part);
                    return Partial("Shared/_PartForm", Part);
                }

                // Preserve audit fields
                Part.CreatedDate = existingPart.CreatedDate;
                Part.CreatedBy = existingPart.CreatedBy;
                Part.LastModifiedDate = DateTime.UtcNow;
                Part.LastModifiedBy = User.Identity?.Name ?? "System";

                // Use transaction for data integrity
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Update part
                    _context.Entry(existingPart).CurrentValues.SetValues(Part);
                    
                    // Update stage requirements based on boolean flags
                    await UpdateStageRequirements(Part);
                    
                    var saveResult = await _context.SaveChangesAsync();
                    
                    if (saveResult > 0)
                    {
                        await transaction.CommitAsync();
                        
                        _logger.LogInformation("? [PARTS-{OperationId}] Part updated successfully: {PartNumber} (ID: {PartId})", 
                            operationId, Part.PartNumber, Part.Id);

                        // Return success response
                        var successScript = $@"
                            <script>
                                console.log('? Part updated successfully: {Part.PartNumber}');
                                
                                // Close modal
                                const modal = document.getElementById('partModal');
                                if (modal && typeof bootstrap !== 'undefined') {{
                                    const bsModal = bootstrap.Modal.getInstance(modal);
                                    if (bsModal) bsModal.hide();
                                }}
                                
                                // Show success message
                                if (typeof window.showToast === 'function') {{
                                    window.showToast('success', 'Part ""{Part.PartNumber}"" updated successfully!');
                                }}
                                
                                // Redirect after delay
                                setTimeout(() => {{
                                    console.log('?? Redirecting to refresh parts list');
                                    window.location.href = '/Admin/Parts';
                                }}, 1500);
                            </script>";

                        return Content(successScript, "text/html");
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning("?? [PARTS-{OperationId}] No rows affected during part update", operationId);
                        ModelState.AddModelError("", "No changes were detected or saved.");
                        await LoadStageDataForPart(Part);
                        return Partial("Shared/_PartForm", Part);
                    }
                }
                catch (Exception dbEx)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error updating part: {PartNumber} (ID: {PartId})", 
                    operationId, Part?.PartNumber, Part?.Id);
                ModelState.AddModelError("", $"Error updating part: {ex.Message}");
                await LoadStageDataForPart(Part);
                return Partial("Shared/_PartForm", Part);
            }
        }

        /// <summary>
        /// Delete part with comprehensive dependency checking
        /// </summary>
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

                // Check dependencies
                var dependencies = await CheckPartDependencies(part);
                
                if (dependencies.Any())
                {
                    var dependencyList = string.Join(", ", dependencies);
                    _logger.LogWarning("?? [PARTS-{OperationId}] Cannot delete part with dependencies: {PartNumber} - Dependencies: {Dependencies}", 
                        operationId, part.PartNumber, dependencyList);
                    TempData["ErrorMessage"] = $"Cannot delete part '{part.PartNumber}' because it is referenced by {dependencyList}.";
                    return RedirectToPage();
                }

                // Use transaction for deletion
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Delete stage requirements first
                    var stageRequirements = await _context.PartStageRequirements
                        .Where(psr => psr.PartId == id)
                        .ToListAsync();
                    
                    if (stageRequirements.Any())
                    {
                        _context.PartStageRequirements.RemoveRange(stageRequirements);
                    }
                    
                    // Delete the part
                    _context.Parts.Remove(part);
                    var saveResult = await _context.SaveChangesAsync();
                    
                    if (saveResult > 0)
                    {
                        await transaction.CommitAsync();
                        
                        _logger.LogInformation("? [PARTS-{OperationId}] Part deleted successfully: {PartNumber} (ID: {PartId})", 
                            operationId, part.PartNumber, part.Id);

                        TempData["SuccessMessage"] = $"Part '{part.PartNumber}' and its stage requirements deleted successfully";
                        return RedirectToPage();
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning("?? [PARTS-{OperationId}] No rows affected during part deletion", operationId);
                        TempData["ErrorMessage"] = "Failed to delete part. No changes were made.";
                        return RedirectToPage();
                    }
                }
                catch (Exception dbEx)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error deleting part ID: {PartId}", operationId, id);
                TempData["ErrorMessage"] = $"Error deleting part: {ex.Message}";
                return RedirectToPage();
            }
        }

        #region Stage Management Methods

        /// <summary>
        /// Add a stage requirement to a part
        /// </summary>
        public async Task<IActionResult> OnPostAddStageAsync(int partId, int stageId, int executionOrder = 1)
        {
            try
            {
                var partStage = new PartStageRequirement
                {
                    PartId = partId,
                    ProductionStageId = stageId,
                    ExecutionOrder = executionOrder,
                    IsRequired = true,
                    IsActive = true,
                    CreatedBy = User.Identity?.Name ?? "System",
                    LastModifiedBy = User.Identity?.Name ?? "System",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                var success = await _partStageService.AddPartStageAsync(partStage);
                
                if (success)
                {
                    return new JsonResult(new { success = true, message = "Stage added successfully" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to add stage" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding stage {StageId} to part {PartId}", stageId, partId);
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Remove a stage requirement from a part
        /// </summary>
        public async Task<IActionResult> OnPostRemoveStageAsync(int partId, int stageId)
        {
            try
            {
                var success = await _partStageService.RemovePartStageAsync(partId, stageId);
                
                if (success)
                {
                    return new JsonResult(new { success = true, message = "Stage removed successfully" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to remove stage" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing stage {StageId} from part {PartId}", stageId, partId);
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get part stage requirements
        /// </summary>
        public async Task<IActionResult> OnGetPartStagesAsync(int partId)
        {
            try
            {
                var stages = await _partStageService.GetPartStagesWithDetailsAsync(partId);
                return new JsonResult(stages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stages for part {PartId}", partId);
                return new JsonResult(new List<PartStageRequirement>());
            }
        }

        #endregion

        #region API Endpoints

        /// <summary>
        /// Get part data as JSON for AJAX requests
        /// </summary>
        public async Task<IActionResult> OnGetPartDataAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting part data for ID: {PartId}", id);
                
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid part ID requested: {PartId}", id);
                    return BadRequest("Invalid part ID");
                }

                var part = await _context.Parts
                    .Include(p => p.PartClassification)
                    .FirstOrDefaultAsync(p => p.Id == id);
                    
                if (part == null)
                {
                    _logger.LogWarning("Part not found for ID: {PartId}", id);
                    return NotFound("Part not found");
                }

                // Get stage requirements for this part
                var stageRequirements = await _context.PartStageRequirements
                    .Where(req => req.PartId == id && req.IsActive)
                    .Include(req => req.ProductionStage)
                    .OrderBy(req => req.ExecutionOrder)
                    .ToListAsync();

                // Create response object
                var partData = new
                {
                    id = part.Id,
                    partNumber = part.PartNumber,
                    name = part.Name,
                    description = part.Description,
                    industry = part.Industry,
                    application = part.Application,
                    material = part.Material,
                    slsMaterial = part.SlsMaterial,
                    estimatedHours = part.EstimatedHours,
                    adminEstimatedHoursOverride = part.AdminEstimatedHoursOverride,
                    adminOverrideReason = part.AdminOverrideReason,
                    hasAdminOverride = part.HasAdminOverride,
                    processType = part.ProcessType,
                    requiredMachineType = part.RequiredMachineType,
                    materialCostPerKg = part.MaterialCostPerKg,
                    standardLaborCostPerHour = part.StandardLaborCostPerHour,
                    partCategory = part.PartCategory,
                    partClass = part.PartClass,
                    customerPartNumber = part.CustomerPartNumber,
                    dimensions = part.Dimensions,
                    weightGrams = part.WeightGrams,
                    isActive = part.IsActive,
                    requiresFDA = part.RequiresFDA,
                    requiresAS9100 = part.RequiresAS9100,
                    requiresNADCAP = part.RequiresNADCAP,
                    complexityLevel = part.ComplexityLevel,
                    complexityScore = part.ComplexityScore,
                    createdDate = part.CreatedDate,
                    createdBy = part.CreatedBy,
                    lastModifiedDate = part.LastModifiedDate,
                    lastModifiedBy = part.LastModifiedBy,
                    // Manufacturing requirements (legacy boolean flags)
                    requiresSLSPrinting = part.RequiresSLSPrinting,
                    requiresCNCMachining = part.RequiresCNCMachining,
                    requiresEDMOperations = part.RequiresEDMOperations,
                    requiresAssembly = part.RequiresAssembly,
                    requiresFinishing = part.RequiresFinishing,
                    requiresInspection = part.RequiresInspection,
                    // Process parameters
                    recommendedLaserPower = part.RecommendedLaserPower,
                    recommendedScanSpeed = part.RecommendedScanSpeed,
                    recommendedLayerThickness = part.RecommendedLayerThickness,
                    recommendedBuildTemperature = part.RecommendedBuildTemperature,
                    // Stage requirements
                    stageRequirements = stageRequirements.Select(sr => new
                    {
                        id = sr.Id,
                        productionStageId = sr.ProductionStageId,
                        stageName = sr.ProductionStage?.Name,
                        executionOrder = sr.ExecutionOrder,
                        estimatedHours = sr.EstimatedHours,
                        isRequired = sr.IsRequired,
                        notes = sr.ProductionStage?.Description // Use description instead of Notes
                    }).ToList()
                };

                _logger.LogInformation("Successfully retrieved part data for ID: {PartId}", id);
                return new JsonResult(partData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving part data for ID: {PartId}", id);
                return StatusCode(500, "Error retrieving part data");
            }
        }

        /// <summary>
        /// Schedule a job for a specific part
        /// </summary>
        public async Task<IActionResult> OnPostScheduleJobAsync(int partId)
        {
            try
            {
                _logger.LogInformation("Scheduling job for part ID: {PartId}", partId);
                
                var part = await _context.Parts.FindAsync(partId);
                if (part == null)
                {
                    TempData["ErrorMessage"] = "Part not found";
                    return RedirectToPage();
                }

                // Redirect to scheduler with part pre-selected
                TempData["SuccessMessage"] = $"Redirecting to scheduler for part '{part.PartNumber}'";
                return RedirectToPage("/Scheduler/Index", new { partNumber = part.PartNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling job for part ID: {PartId}", partId);
                TempData["ErrorMessage"] = "Error scheduling job. Please try again.";
                return RedirectToPage();
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task EnsureDefaultStagesExist()
        {
            try
            {
                var stageCount = await _productionStageSeeder.SeedDefaultStagesAsync();
                _logger.LogInformation("Production stages seeded: {Count} stages available", stageCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding default production stages");
            }
        }

        private async Task CreateDefaultStageRequirements(Part part)
        {
            try
            {
                var stageRequirements = new List<PartStageRequirement>();
                var availableStages = await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .ToListAsync();

                int executionOrder = 1;

                // Map boolean flags to stage requirements
                if (part.RequiresSLSPrinting)
                {
                    var slsStage = availableStages.FirstOrDefault(s => s.Name.Contains("SLS") || s.Name.Contains("Printing"));
                    if (slsStage != null)
                    {
                        stageRequirements.Add(new PartStageRequirement
                        {
                            PartId = part.Id,
                            ProductionStageId = slsStage.Id,
                            ExecutionOrder = executionOrder++,
                            IsRequired = true,
                            EstimatedHours = part.EstimatedHours,
                            CreatedBy = part.CreatedBy,
                            LastModifiedBy = part.LastModifiedBy,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedDate = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }

                if (part.RequiresCNCMachining)
                {
                    var cncStage = availableStages.FirstOrDefault(s => s.Name.Contains("CNC") || s.Name.Contains("Machining"));
                    if (cncStage != null)
                    {
                        stageRequirements.Add(new PartStageRequirement
                        {
                            PartId = part.Id,
                            ProductionStageId = cncStage.Id,
                            ExecutionOrder = executionOrder++,
                            IsRequired = true,
                            EstimatedHours = part.EstimatedHours * 0.3, // 30% of SLS time
                            CreatedBy = part.CreatedBy,
                            LastModifiedBy = part.LastModifiedBy,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedDate = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }

                if (part.RequiresEDMOperations)
                {
                    var edmStage = availableStages.FirstOrDefault(s => s.Name.Contains("EDM"));
                    if (edmStage != null)
                    {
                        stageRequirements.Add(new PartStageRequirement
                        {
                            PartId = part.Id,
                            ProductionStageId = edmStage.Id,
                            ExecutionOrder = executionOrder++,
                            IsRequired = true,
                            EstimatedHours = part.EstimatedHours * 0.5, // 50% of SLS time
                            CreatedBy = part.CreatedBy,
                            LastModifiedBy = part.LastModifiedBy,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedDate = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }

                if (part.RequiresAssembly)
                {
                    var assemblyStage = availableStages.FirstOrDefault(s => s.Name.Contains("Assembly"));
                    if (assemblyStage != null)
                    {
                        stageRequirements.Add(new PartStageRequirement
                        {
                            PartId = part.Id,
                            ProductionStageId = assemblyStage.Id,
                            ExecutionOrder = executionOrder++,
                            IsRequired = true,
                            EstimatedHours = 2.0, // Fixed 2 hours
                            CreatedBy = part.CreatedBy,
                            LastModifiedBy = part.LastModifiedBy,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedDate = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }

                if (part.RequiresFinishing)
                {
                    var finishingStage = availableStages.FirstOrDefault(s => s.Name.Contains("Finishing"));
                    if (finishingStage != null)
                    {
                        stageRequirements.Add(new PartStageRequirement
                        {
                            PartId = part.Id,
                            ProductionStageId = finishingStage.Id,
                            ExecutionOrder = executionOrder++,
                            IsRequired = true,
                            EstimatedHours = 3.0, // Fixed 3 hours
                            CreatedBy = part.CreatedBy,
                            LastModifiedBy = part.LastModifiedBy,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedDate = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }

                if (part.RequiresInspection)
                {
                    var qualityStage = availableStages.FirstOrDefault(s => s.Name.Contains("Quality") || s.Name.Contains("Inspection"));
                    if (qualityStage != null)
                    {
                        stageRequirements.Add(new PartStageRequirement
                        {
                            PartId = part.Id,
                            ProductionStageId = qualityStage.Id,
                            ExecutionOrder = executionOrder++,
                            IsRequired = true,
                            EstimatedHours = 1.0, // Fixed 1 hour
                            CreatedBy = part.CreatedBy,
                            LastModifiedBy = part.LastModifiedBy,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedDate = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }

                if (stageRequirements.Any())
                {
                    await _context.PartStageRequirements.AddRangeAsync(stageRequirements);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Created {Count} stage requirements for part {PartNumber}", 
                        stageRequirements.Count, part.PartNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default stage requirements for part {PartId}", part.Id);
            }
        }

        private async Task UpdateStageRequirements(Part part)
        {
            try
            {
                // Remove existing stage requirements
                var existingStages = await _context.PartStageRequirements
                    .Where(psr => psr.PartId == part.Id)
                    .ToListAsync();

                _context.PartStageRequirements.RemoveRange(existingStages);

                // Create new stage requirements based on current boolean flags
                await CreateDefaultStageRequirements(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stage requirements for part {PartId}", part.Id);
            }
        }

        private async Task<List<string>> CheckPartDependencies(Part part)
        {
            var dependencies = new List<string>();

            try
            {
                // Check Jobs
                var jobCount = await _context.Jobs.CountAsync(j => j.PartNumber == part.PartNumber);
                if (jobCount > 0) dependencies.Add($"{jobCount} job(s)");

                // Check BuildJobs
                var buildJobCount = await _context.BuildJobs.CountAsync(bj => bj.PartId == part.Id);
                if (buildJobCount > 0) dependencies.Add($"{buildJobCount} build job(s)");

                // Check SerialNumbers
                var serialNumberCount = await _context.SerialNumbers.CountAsync(sn => sn.PartId == part.Id);
                if (serialNumberCount > 0) dependencies.Add($"{serialNumberCount} serial number(s)");

                // Check PrototypeJobs
                var prototypeJobCount = await _context.PrototypeJobs.CountAsync(pj => pj.PartId == part.Id);
                if (prototypeJobCount > 0) dependencies.Add($"{prototypeJobCount} prototype job(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking dependencies for part {PartId}", part.Id);
            }

            return dependencies;
        }

        private List<string> ValidatePartData(Part part)
        {
            var errors = new List<string>();

            if (part == null)
            {
                errors.Add("Part data is required");
                return errors;
            }

            // Required field validation
            if (string.IsNullOrWhiteSpace(part.PartNumber))
                errors.Add("Part Number is required");
            else if (part.PartNumber.Length > 50)
                errors.Add("Part Number cannot exceed 50 characters");

            if (string.IsNullOrWhiteSpace(part.Name))
                errors.Add("Part Name is required");
            else if (part.Name.Length > 200)
                errors.Add("Part Name cannot exceed 200 characters");

            if (string.IsNullOrWhiteSpace(part.Description))
                errors.Add("Description is required");

            if (string.IsNullOrWhiteSpace(part.Industry))
                errors.Add("Industry is required");

            if (string.IsNullOrWhiteSpace(part.Application))
                errors.Add("Application is required");

            if (string.IsNullOrWhiteSpace(part.Material))
                errors.Add("Material is required");

            if (part.EstimatedHours <= 0)
                errors.Add("Estimated Hours must be greater than 0");

            // Numeric validation
            if (part.MaterialCostPerKg < 0)
                errors.Add("Material cost cannot be negative");

            if (part.AdminEstimatedHoursOverride.HasValue)
            {
                if (part.AdminEstimatedHoursOverride.Value <= 0)
                    errors.Add("Admin override hours must be greater than 0");
                
                if (string.IsNullOrWhiteSpace(part.AdminOverrideReason))
                    errors.Add("Admin override reason is required when override hours are specified");
            }

            return errors;
        }

        private async Task LoadPartsDataAsync()
        {
            var query = _context.Parts
                .Include(p => p.PartClassification)
                .AsQueryable();

            // Apply filters
            if (ActiveOnly) 
                query = query.Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var searchLower = SearchTerm.ToLower();
                query = query.Where(p => 
                    p.PartNumber.ToLower().Contains(searchLower) ||
                    p.Name.ToLower().Contains(searchLower) ||
                    p.Description.ToLower().Contains(searchLower));
            }

            if (!string.IsNullOrEmpty(MaterialFilter))
                query = query.Where(p => p.Material == MaterialFilter);
            
            if (!string.IsNullOrEmpty(IndustryFilter))
                query = query.Where(p => p.Industry == IndustryFilter);
            
            if (!string.IsNullOrEmpty(CategoryFilter))
                query = query.Where(p => p.PartCategory == CategoryFilter);

            if (!string.IsNullOrEmpty(ComplexityFilter))
            {
                // Apply complexity filter based on calculated complexity
                switch (ComplexityFilter)
                {
                    case "Simple":
                        query = query.Where(p => p.EstimatedHours <= 6 && p.RequiredStageCount <= 2);
                        break;
                    case "Medium":
                        query = query.Where(p => p.EstimatedHours > 6 && p.EstimatedHours <= 12 && p.RequiredStageCount <= 4);
                        break;
                    case "Complex":
                        query = query.Where(p => p.EstimatedHours > 12 && p.EstimatedHours <= 24 && p.RequiredStageCount <= 6);
                        break;
                    case "Very Complex":
                        query = query.Where(p => p.EstimatedHours > 24 || p.RequiredStageCount > 6);
                        break;
                }
            }

            // Apply sorting
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
                "complexity" => ascending ? query.OrderBy(p => p.ComplexityScore) : query.OrderByDescending(p => p.ComplexityScore),
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

        private async Task LoadStageDataAsync()
        {
            try
            {
                AvailableStages = await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .OrderBy(ps => ps.DisplayOrder)
                    .ToListAsync();

                // Load stage requirements for all parts in current page
                var partIds = Parts.Select(p => p.Id).ToList();
                var allPartStages = await _context.PartStageRequirements
                    .Where(psr => partIds.Contains(psr.PartId) && psr.IsActive)
                    .Include(psr => psr.ProductionStage)
                    .ToListAsync();

                PartStages = allPartStages
                    .GroupBy(partStageReq => partStageReq.PartId)
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stage data");
                AvailableStages = new List<ProductionStage>();
                PartStages = new Dictionary<int, List<PartStageRequirement>>();
            }
        }

        private async Task LoadMachinesDataAsync()
        {
            try
            {
                AvailableMachines = await _context.Machines
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.MachineId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading machines data");
                AvailableMachines = new List<Machine>();
            }
        }

        private async Task LoadStageDataForPart(Part part)
        {
            try
            {
                ViewData["AvailableStages"] = await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .OrderBy(ps => ps.DisplayOrder)
                    .ToListAsync();

                ViewData["AvailableMachines"] = await _context.Machines
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.MachineId)
                    .ToListAsync();

                if (part.Id > 0)
                {
                    ViewData["PartStages"] = await _context.PartStageRequirements
                        .Where(req => req.PartId == part.Id && req.IsActive)
                        .Include(req => req.ProductionStage)
                        .OrderBy(req => req.ExecutionOrder)
                        .ToListAsync();
                }
                else
                {
                    ViewData["PartStages"] = new List<PartStageRequirement>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stage data for part {PartId}", part.Id);
                ViewData["AvailableStages"] = new List<ProductionStage>();
                ViewData["AvailableMachines"] = new List<Machine>();
                ViewData["PartStages"] = new List<PartStageRequirement>();
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

        public string GetComplexityBadgeClass(string complexityLevel)
        {
            return complexityLevel switch
            {
                "Simple" => "bg-success",
                "Medium" => "bg-info",
                "Complex" => "bg-warning",
                "Very Complex" => "bg-danger",
                _ => "bg-secondary"
            };
        }

        public List<PartStageRequirement> GetPartStages(int partId)
        {
            return PartStages.TryGetValue(partId, out var stages) ? stages : new List<PartStageRequirement>();
        }

        #endregion
    }
}