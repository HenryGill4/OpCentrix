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
    /// Bulletproof Parts management page with simplified, reliable CRUD operations
    /// REDESIGNED: Focuses on simplicity, reliability, and maintainability
    /// ENHANCED: Integrated with PartStageService for stage management
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class PartsModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PartsModel> _logger;
        private readonly IPartStageService _partStageService;

        public PartsModel(SchedulerContext context, ILogger<PartsModel> logger, IPartStageService partStageService)
        {
            _context = context;
            _logger = logger;
            _partStageService = partStageService;
        }

        // Main data properties
        public IList<Part> Parts { get; set; } = new List<Part>();

        // Form property for modal
        [BindProperty]
        public Part Part { get; set; } = new Part();

        // Stage management properties
        [BindProperty]
        public List<int> SelectedStageIds { get; set; } = new List<int>();

        [BindProperty]
        public List<int> StageExecutionOrders { get; set; } = new List<int>();

        [BindProperty]
        public List<double> StageEstimatedHours { get; set; } = new List<double>();

        [BindProperty]
        public List<decimal> StageHourlyRates { get; set; } = new List<decimal>();

        [BindProperty]
        public List<decimal> StageMaterialCosts { get; set; } = new List<decimal>();

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
        public string? StageFilter { get; set; }

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

        // Statistics
        public int ActivePartsCount { get; set; }
        public int InactivePartsCount { get; set; }
        public string MostUsedMaterial { get; set; } = string.Empty;
        public double AverageEstimatedHours { get; set; }

        // Simplified ViewModels for bulletproof data passing
        public PartFormViewModel PartFormData { get; set; } = new PartFormViewModel();

        // Stage-related properties
        public Dictionary<string, int> StageUsageStats { get; set; } = new();
        public List<ProductionStage> AvailableStages { get; set; } = new List<ProductionStage>();

        public async Task<IActionResult> OnGetAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🔧 [PARTS-{OperationId}] Loading parts page", operationId);

            try
            {
                await LoadPartsDataAsync();
                await LoadFilterOptionsAsync();
                await LoadStatisticsAsync();
                await LoadStageDataAsync();

                _logger.LogInformation("✅ [PARTS-{OperationId}] Parts page loaded successfully", operationId);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [PARTS-{OperationId}] Error loading parts page", operationId);
                TempData["ErrorMessage"] = "Error loading parts data. Please try again.";
                return Page();
            }
        }

        /// <summary>
        /// Get add part modal form - SIMPLIFIED for bulletproof reliability
        /// </summary>
        public async Task<IActionResult> OnGetAddAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🔧 [PARTS-{OperationId}] Loading add part form", operationId);

            try
            {
                // Create new part with essential defaults only
                Part = CreateDefaultPart();
                
                // Load form data in simple, predictable way
                PartFormData = await CreatePartFormViewModelAsync(Part);

                _logger.LogInformation("✅ [PARTS-{OperationId}] Add part form loaded successfully", operationId);
                return Partial("Shared/_PartForm", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [PARTS-{OperationId}] Error loading add part form", operationId);
                return StatusCode(500, "Error loading form");
            }
        }

        /// <summary>
        /// Get edit part modal form - SIMPLIFIED for bulletproof reliability
        /// </summary>
        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🔧 [PARTS-{OperationId}] Loading edit part form for ID: {PartId}", operationId, id);

            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("⚠️ [PARTS-{OperationId}] Invalid part ID: {PartId}", operationId, id);
                    return BadRequest("Invalid part ID");
                }

                Part = await _context.Parts.FindAsync(id);
                if (Part == null)
                {
                    _logger.LogWarning("⚠️ [PARTS-{OperationId}] Part not found: {PartId}", operationId, id);
                    return NotFound("Part not found");
                }

                // Load existing stage assignments
                await LoadExistingStageAssignments(id);

                // Load form data in simple, predictable way
                PartFormData = await CreatePartFormViewModelAsync(Part);

                _logger.LogInformation("✅ [PARTS-{OperationId}] Edit part form loaded successfully", operationId);
                return Partial("Shared/_PartForm", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [PARTS-{OperationId}] Error loading edit part form for ID: {PartId}", operationId, id);
                return StatusCode(500, "Error loading form");
            }
        }

        /// <summary>
        /// Create new part - ENHANCED with stage management
        /// </summary>
        public async Task<IActionResult> OnPostCreateAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🔧 [PARTS-{OperationId}] Creating new part with stages", operationId);

            try
            {
                // Basic validation first
                if (Part == null)
                {
                    _logger.LogWarning("⚠️ [PARTS-{OperationId}] Part data is null", operationId);
                    return await HandleValidationError("Part data is required", CreateDefaultPart());
                }

                // Essential field validation
                var validationErrors = ValidateEssentialFields(Part);
                if (validationErrors.Any())
                {
                    _logger.LogWarning("⚠️ [PARTS-{OperationId}] Validation failed: {Errors}", operationId, string.Join(", ", validationErrors));
                    foreach (var error in validationErrors)
                        ModelState.AddModelError("", error);
                    return await HandleValidationError("Please fix validation errors", Part);
                }

                // Check for duplicate part number
                var duplicateExists = await _context.Parts
                    .AnyAsync(p => p.PartNumber == Part.PartNumber);

                if (duplicateExists)
                {
                    _logger.LogWarning("⚠️ [PARTS-{OperationId}] Duplicate part number: {PartNumber}", operationId, Part.PartNumber);
                    ModelState.AddModelError("Part.PartNumber", $"Part number '{Part.PartNumber}' already exists");
                    return await HandleValidationError("Duplicate part number", Part);
                }

                // Set audit fields and defaults
                SetPartDefaults(Part, isNew: true);

                // Save to database with transaction
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Parts.Add(Part);
                    var result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {
                        // Handle stage assignments
                        await ProcessStageAssignments(Part.Id, operationId);

                        await transaction.CommitAsync();
                        _logger.LogInformation("✅ [PARTS-{OperationId}] Part created successfully: {PartNumber} (ID: {PartId}) with {StageCount} stages",
                            operationId, Part.PartNumber, Part.Id, SelectedStageIds.Count);

                        // Return success response that works for both HTMX and standard forms
                        return await HandleFormSuccess($"Part '{Part.PartNumber}' created successfully with {SelectedStageIds.Count} manufacturing stages");
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return await HandleValidationError("Failed to save part to database", Part);
                    }
                }
                catch (Exception dbEx)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(dbEx, "❌ [PARTS-{OperationId}] Database error creating part", operationId);
                    return await HandleValidationError($"Database error: {GetFriendlyErrorMessage(dbEx)}", Part);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [PARTS-{OperationId}] Unexpected error creating part", operationId);
                return await HandleValidationError($"Unexpected error: {ex.Message}", Part ?? CreateDefaultPart());
            }
        }

        /// <summary>
        /// Update existing part - ENHANCED with stage management
        /// </summary>
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🔧 [PARTS-{OperationId}] Updating part: {PartNumber} (ID: {PartId}) with stages",
                operationId, Part?.PartNumber, Part?.Id);

            try
            {
                // Basic validation
                if (Part == null || Part.Id <= 0)
                {
                    _logger.LogWarning("⚠️ [PARTS-{OperationId}] Invalid part data for update", operationId);
                    return await HandleValidationError("Invalid part data", CreateDefaultPart());
                }

                // Essential field validation
                var validationErrors = ValidateEssentialFields(Part);
                if (validationErrors.Any())
                {
                    _logger.LogWarning("⚠️ [PARTS-{OperationId}] Validation failed: {Errors}", operationId, string.Join(", ", validationErrors));
                    foreach (var error in validationErrors)
                        ModelState.AddModelError("", error);
                    return await HandleValidationError("Please fix validation errors", Part);
                }

                // Get existing part
                var existingPart = await _context.Parts.FindAsync(Part.Id);
                if (existingPart == null)
                {
                    _logger.LogWarning("⚠️ [PARTS-{OperationId}] Part not found for update: ID {PartId}", operationId, Part.Id);
                    return await HandleValidationError("Part not found. It may have been deleted.", Part);
                }

                // Check for duplicate part number (excluding current part)
                var duplicateExists = await _context.Parts
                    .AnyAsync(p => p.PartNumber == Part.PartNumber && p.Id != Part.Id);

                if (duplicateExists)
                {
                    _logger.LogWarning("⚠️ [PARTS-{OperationId}] Duplicate part number: {PartNumber}", operationId, Part.PartNumber);
                    ModelState.AddModelError("Part.PartNumber", $"Part number '{Part.PartNumber}' already exists");
                    return await HandleValidationError("Duplicate part number", Part);
                }

                // Preserve audit fields and set defaults
                SetPartDefaults(Part, isNew: false, existingPart: existingPart);

                // Save to database with transaction
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Entry(existingPart).CurrentValues.SetValues(Part);
                    
                    // Handle stage assignments
                    await ProcessStageAssignments(Part.Id, operationId);
                    
                    var result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {
                        await transaction.CommitAsync();
                        _logger.LogInformation("✅ [PARTS-{OperationId}] Part updated successfully: {PartNumber} (ID: {PartId}) with {StageCount} stages",
                            operationId, Part.PartNumber, Part.Id, SelectedStageIds.Count);

                        // Return success response that works for both HTMX and standard forms
                        return await HandleFormSuccess($"Part '{Part.PartNumber}' updated successfully with {SelectedStageIds.Count} manufacturing stages");
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return await HandleValidationError("No changes were detected", Part);
                    }
                }
                catch (Exception dbEx)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(dbEx, "❌ [PARTS-{OperationId}] Database error updating part", operationId);
                    return await HandleValidationError($"Database error: {GetFriendlyErrorMessage(dbEx)}", Part);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [PARTS-{OperationId}] Unexpected error updating part", operationId);
                return await HandleValidationError($"Unexpected error: {ex.Message}", Part ?? CreateDefaultPart());
            }
        }

        /// <summary>
        /// Delete part with comprehensive dependency checking
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🗑️ [PARTS-{OperationId}] Deleting part ID: {PartId}", operationId, id);

            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("⚠️ [PARTS-{OperationId}] Invalid part ID for deletion: {PartId}", operationId, id);
                    TempData["ErrorMessage"] = "Invalid part ID";
                    return RedirectToPage();
                }

                var part = await _context.Parts.FindAsync(id);
                if (part == null)
                {
                    _logger.LogWarning("⚠️ [PARTS-{OperationId}] Part not found for deletion: ID {PartId}", operationId, id);
                    TempData["ErrorMessage"] = "Part not found. It may have already been deleted.";
                    return RedirectToPage();
                }

                // Check dependencies
                var dependencies = await CheckPartDependencies(part);
                if (dependencies.Any())
                {
                    var dependencyList = string.Join(", ", dependencies);
                    _logger.LogWarning("⚠️ [PARTS-{OperationId}] Cannot delete part with dependencies: {PartNumber} - Dependencies: {Dependencies}",
                        operationId, part.PartNumber, dependencyList);
                    TempData["ErrorMessage"] = $"Cannot delete part '{part.PartNumber}' because it is referenced by {dependencyList}.";
                    return RedirectToPage();
                }

                // Delete with transaction
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Remove stage assignments first
                    await _partStageService.RemoveAllPartStagesAsync(part.Id);
                    
                    _context.Parts.Remove(part);
                    var result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {
                        await transaction.CommitAsync();
                        _logger.LogInformation("✅ [PARTS-{OperationId}] Part deleted successfully: {PartNumber} (ID: {PartId})",
                            operationId, part.PartNumber, part.Id);

                        TempData["SuccessMessage"] = $"Part '{part.PartNumber}' deleted successfully";
                        return RedirectToPage();
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = "Failed to delete part. No changes were made.";
                        return RedirectToPage();
                    }
                }
                catch (Exception dbEx)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(dbEx, "❌ [PARTS-{OperationId}] Database error deleting part", operationId);
                    TempData["ErrorMessage"] = $"Database error: {GetFriendlyErrorMessage(dbEx)}";
                    return RedirectToPage();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [PARTS-{OperationId}] Unexpected error deleting part ID: {PartId}", operationId, id);
                TempData["ErrorMessage"] = $"Error deleting part: {ex.Message}";
                return RedirectToPage();
            }
        }

        #region Private Helper Methods - ENHANCED WITH STAGE MANAGEMENT

        private async Task LoadStageDataAsync()
        {
            try
            {
                StageUsageStats = await _partStageService.GetStageUsageStatisticsAsync();
                AvailableStages = await _partStageService.GetAvailableStagesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stage data");
            }
        }

        private async Task LoadExistingStageAssignments(int partId)
        {
            try
            {
                var partStages = await _partStageService.GetPartStagesWithDetailsAsync(partId);
                
                SelectedStageIds = partStages.Select(ps => ps.ProductionStageId).ToList();
                StageExecutionOrders = partStages.Select(ps => ps.ExecutionOrder).ToList();
                StageEstimatedHours = partStages.Select(ps => ps.EstimatedHours ?? (ps.ProductionStage?.DefaultDurationHours ?? 1.0)).ToList();
                StageHourlyRates = partStages.Select(ps => ps.HourlyRateOverride ?? (ps.ProductionStage?.DefaultHourlyRate ?? 85.00m)).ToList();
                StageMaterialCosts = partStages.Select(ps => ps.MaterialCost).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading existing stage assignments for part {PartId}", partId);
            }
        }

        private async Task ProcessStageAssignments(int partId, string operationId)
        {
            try
            {
                if (SelectedStageIds?.Any() != true)
                {
                    _logger.LogInformation("📋 [PARTS-{OperationId}] No stages selected for part {PartId}", operationId, partId);
                    return;
                }

                // Remove existing stage assignments
                await _partStageService.RemoveAllPartStagesAsync(partId);

                // Add new stage assignments
                for (int i = 0; i < SelectedStageIds.Count; i++)
                {
                    var stageRequirement = new PartStageRequirement
                    {
                        PartId = partId,
                        ProductionStageId = SelectedStageIds[i],
                        ExecutionOrder = i < StageExecutionOrders.Count ? StageExecutionOrders[i] : i + 1,
                        EstimatedHours = i < StageEstimatedHours.Count ? StageEstimatedHours[i] : null,
                        HourlyRateOverride = i < StageHourlyRates.Count ? StageHourlyRates[i] : null,
                        MaterialCost = i < StageMaterialCosts.Count ? StageMaterialCosts[i] : 0,
                        IsRequired = true,
                        IsActive = true,
                        CreatedBy = User.Identity?.Name ?? "System",
                        LastModifiedBy = User.Identity?.Name ?? "System"
                    };

                    await _partStageService.AddPartStageAsync(stageRequirement);
                }

                _logger.LogInformation("✅ [PARTS-{OperationId}] Processed {StageCount} stage assignments for part {PartId}", 
                    operationId, SelectedStageIds.Count, partId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [PARTS-{OperationId}] Error processing stage assignments for part {PartId}", operationId, partId);
                throw;
            }
        }

        #endregion

        #region Private Helper Methods - SIMPLIFIED

        private Part CreateDefaultPart()
        {
            return new Part
            {
                Id = 0,
                PartNumber = "",
                Name = "",
                Description = "",
                Industry = "Firearms", // Updated to match B&T focus
                Application = "B&T Manufacturing", // Updated to match B&T focus
                Material = "Ti-6Al-4V Grade 5",
                SlsMaterial = "Ti-6Al-4V Grade 5", // Ensure SLS material matches
                EstimatedHours = 8.0,
                MaterialCostPerKg = 450.00m,
                StandardLaborCostPerHour = 85.00m,
                PartCategory = "Production",
                PartClass = "B",
                ProcessType = "SLS Metal",
                RequiredMachineType = "TruPrint 3000",
                IsActive = true,
                RequiresSLSPrinting = true,
                RequiresInspection = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "System",
                
                // B&T Manufacturing defaults
                BTComponentType = "General",
                BTFirearmCategory = "Component",
                ManufacturingStage = "Design",
                StageDetails = "{}",
                StageOrder = 1,
                
                // Ensure all required string fields have defaults
                CustomerPartNumber = "",
                Dimensions = "",
                SurfaceFinishRequirement = "As-built",
                PowderSpecification = "15-45 micron particle size",
                PreferredMachines = "TI1,TI2",
                RequiredSkills = "SLS Operation,Powder Handling,Inert Gas Safety,Post-Processing",
                RequiredCertifications = "SLS Operation Certification,Powder Safety Training",
                RequiredTooling = "Build Platform,Powder Sieve,Support Removal Tools",
                ConsumableMaterials = "Argon Gas,Build Platform Coating",
                SupportStrategy = "Minimal supports on overhangs > 45°",
                QualityStandards = "ASTM F3001, ISO 17296",
                ToleranceRequirements = "±0.1mm typical, ±0.05mm critical dimensions",
                ProcessParameters = "{}",
                QualityCheckpoints = "{}",
                BuildFileTemplate = "",
                CadFilePath = "",
                CadFileVersion = "",
                AvgDuration = "8h 0m",
                AvgDurationDays = 1,
                AdminOverrideReason = "",
                AdminOverrideBy = "",
                
                // B&T specific defaults
                BTSuppressorType = "",
                BTBafflePosition = "",
                BTCaliberCompatibility = "",
                BTThreadPitch = "",
                SerialNumberFormat = "BT-{YYYY}-{####}",
                BatchControlMethod = "Standard",
                MaxBatchSize = 1,
                ParentComponents = "[]",
                ChildComponents = "[]",
                WorkflowTemplate = "BT_Standard_Workflow",
                ApprovalWorkflow = "Standard",
                ATFClassification = "",
                FFLRequirements = "",
                ITARCategory = "",
                EARClassification = "",
                ExportControlNotes = "",
                ExportClassification = "",
                ComponentType = "",
                FirearmType = "",
                BTTestingProtocol = "",
                BTQualitySpecification = "",
                BTTestingRequirements = "",
                BTQualityStandards = "",
                BTRegulatoryNotes = "",
                
                // Initialize numeric fields with safe defaults
                PowderRequirementKg = 0.5,
                RecommendedLaserPower = 200,
                RecommendedScanSpeed = 1200,
                RecommendedLayerThickness = 30,
                RecommendedHatchSpacing = 120,
                RecommendedBuildTemperature = 180,
                RequiredArgonPurity = 99.9,
                MaxOxygenContent = 50,
                WeightGrams = 0,
                VolumeMm3 = 0,
                HeightMm = 0,
                LengthMm = 0,
                WidthMm = 0,
                MaxSurfaceRoughnessRa = 25,
                SetupCost = 150.00m,
                PostProcessingCost = 75.00m,
                QualityInspectionCost = 50.00m,
                MachineOperatingCostPerHour = 125.00m,
                ArgonCostPerHour = 15.00m,
                SetupTimeMinutes = 45,
                PowderChangeoverTimeMinutes = 30,
                PreheatingTimeMinutes = 60,
                CoolingTimeMinutes = 240,
                PostProcessingTimeMinutes = 45,
                SupportRemovalTimeMinutes = 0,
                AverageActualHours = 0,
                AverageEfficiencyPercent = 100,
                AverageQualityScore = 100,
                AverageDefectRate = 0,
                AveragePowderUtilization = 85,
                TotalJobsCompleted = 0,
                TotalUnitsProduced = 0,
                AverageCostPerUnit = 0,
                StandardSellingPrice = 0,
                
                // B&T cost defaults
                BTLicensingCost = 0.00m,
                ComplianceCost = 0.00m,
                TestingCost = 0.00m,
                DocumentationCost = 0.00m
            };
        }

        private async Task<PartFormViewModel> CreatePartFormViewModelAsync(Part part)
        {
            return new PartFormViewModel
            {
                Part = part,
                AvailableMaterials = await _context.Parts
                    .Where(p => !string.IsNullOrEmpty(p.Material))
                    .Select(p => p.Material)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToListAsync(),
                AvailableIndustries = await _context.Parts
                    .Where(p => !string.IsNullOrEmpty(p.Industry))
                    .Select(p => p.Industry)
                    .Distinct()
                    .OrderBy(i => i)
                    .ToListAsync(),
                AvailableApplications = await _context.Parts
                    .Where(p => !string.IsNullOrEmpty(p.Application))
                    .Select(p => p.Application)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToListAsync(),
                AvailableStages = await _partStageService.GetAvailableStagesAsync()
            };
        }

        private List<string> ValidateEssentialFields(Part part)
        {
            var errors = new List<string>();

            // Required field validation
            if (string.IsNullOrWhiteSpace(part.PartNumber))
                errors.Add("Part Number is required");
            else if (part.PartNumber.Length > 50)
                errors.Add("Part Number cannot exceed 50 characters");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(part.PartNumber, @"^[A-Z0-9][A-Z0-9\-_]{2,49}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                errors.Add("Part Number must be 3-50 characters, alphanumeric with hyphens/underscores only");

            if (string.IsNullOrWhiteSpace(part.Name))
                errors.Add("Part Name is required");
            else if (part.Name.Length > 200)
                errors.Add("Part Name cannot exceed 200 characters");

            if (string.IsNullOrWhiteSpace(part.Description))
                errors.Add("Description is required");
            else if (part.Description.Length > 500)
                errors.Add("Description cannot exceed 500 characters");

            if (string.IsNullOrWhiteSpace(part.Industry))
                errors.Add("Industry is required");
            else if (part.Industry.Length > 100)
                errors.Add("Industry cannot exceed 100 characters");

            if (string.IsNullOrWhiteSpace(part.Application))
                errors.Add("Application is required");
            else if (part.Application.Length > 100)
                errors.Add("Application cannot exceed 100 characters");

            if (string.IsNullOrWhiteSpace(part.Material))
                errors.Add("Material is required");
            else if (part.Material.Length > 100)
                errors.Add("Material cannot exceed 100 characters");

            // Numeric field validation
            if (part.EstimatedHours <= 0)
                errors.Add("Estimated Hours must be greater than 0");
            else if (part.EstimatedHours > 200)
                errors.Add("Estimated Hours cannot exceed 200 hours");

            if (part.MaterialCostPerKg < 0)
                errors.Add("Material Cost cannot be negative");
            else if (part.MaterialCostPerKg > 10000)
                errors.Add("Material Cost seems unreasonably high (over $10,000/kg)");

            if (part.StandardLaborCostPerHour < 0)
                errors.Add("Labor Cost cannot be negative");
            else if (part.StandardLaborCostPerHour > 500)
                errors.Add("Labor Cost seems unreasonably high (over $500/hour)");

            // Physical properties validation
            if (part.WeightGrams < 0)
                errors.Add("Weight cannot be negative");

            if (part.LengthMm < 0 || part.WidthMm < 0 || part.HeightMm < 0)
                errors.Add("Dimensions cannot be negative");

            if (part.LengthMm > 1000 || part.WidthMm > 1000 || part.HeightMm > 1000)
                errors.Add("Dimensions seem unreasonably large (over 1000mm)");

            // Manufacturing stages validation
            var stageCount = 0;
            if (part.RequiresSLSPrinting) stageCount++;
            if (part.RequiresCNCMachining) stageCount++;
            if (part.RequiresEDMOperations) stageCount++;
            if (part.RequiresAssembly) stageCount++;
            if (part.RequiresFinishing) stageCount++;

            if (stageCount == 0)
                errors.Add("At least one manufacturing stage must be selected");

            // B&T specific validation
            if (part.RequiresATFForm1 || part.RequiresATFForm4)
            {
                if (string.IsNullOrWhiteSpace(part.ATFClassification))
                    errors.Add("ATF Classification is required when ATF forms are needed");
            }

            if (part.RequiresTaxStamp && !part.TaxStampAmount.HasValue)
                errors.Add("Tax Stamp Amount is required when tax stamp is needed");

            if (part.RequiresExportLicense && string.IsNullOrWhiteSpace(part.ITARCategory))
                errors.Add("ITAR Category is required for export controlled items");

            // Admin override validation
            if (part.AdminEstimatedHoursOverride.HasValue)
            {
                if (part.AdminEstimatedHoursOverride.Value <= 0)
                    errors.Add("Admin override hours must be greater than 0");
                else if (part.AdminEstimatedHoursOverride.Value > 200)
                    errors.Add("Admin override hours cannot exceed 200 hours");

                if (string.IsNullOrWhiteSpace(part.AdminOverrideReason))
                    errors.Add("Admin override reason is required when override hours are specified");
                else if (part.AdminOverrideReason.Length > 500)
                    errors.Add("Admin override reason cannot exceed 500 characters");
            }

            // Business logic validation
            if (part.BTComponentType == "Suppressor" && !part.RequiresTaxStamp)
                errors.Add("Suppressor components typically require a tax stamp");

            if (part.BTFirearmCategory == "Firearm" && !part.RequiresUniqueSerialNumber)
                errors.Add("Firearm components require unique serial numbers");

            if (!string.IsNullOrEmpty(part.BTSuppressorType) && !part.RequiresSoundTesting)
                errors.Add("Suppressor components should include sound testing");

            return errors;
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

            // Set essential defaults for NOT NULL fields
            part.CustomerPartNumber ??= "";
            part.AdminOverrideReason ??= "";
            part.AdminOverrideBy ??= "";

            // Ensure numeric fields have valid values
            if (part.MaterialCostPerKg <= 0) part.MaterialCostPerKg = 450.00m;
            if (part.StandardLaborCostPerHour <= 0) part.StandardLaborCostPerHour = 85.00m;
        }

        private async Task<IActionResult> HandleValidationError(string message, Part part)
        {
            ModelState.AddModelError("", message);
            PartFormData = await CreatePartFormViewModelAsync(part);
            return Partial("Shared/_PartForm", this);
        }

        private async Task<IActionResult> HandleFormSuccess(string message)
        {
            // Check if this is an HTMX request
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                // For HTMX requests, return JavaScript that closes modal and refreshes list
                var successScript = $@"
                    <script>
                        console.log('[PARTS] HTMX Success handler executed');
                        
                        // Close modal with multiple fallback methods
                        const modal = document.getElementById('partModal');
                        if (modal) {{
                            try {{
                                if (typeof bootstrap !== 'undefined') {{
                                    const bsModal = bootstrap.Modal.getInstance(modal);
                                    if (bsModal) {{
                                        bsModal.hide();
                                        console.log('[PARTS] Modal closed via Bootstrap');
                                    }}
                                }}
                                
                                // Fallback modal close
                                modal.style.display = 'none';
                                modal.classList.remove('show');
                                document.body.classList.remove('modal-open');
                                
                                // Remove backdrop if exists
                                const backdrop = document.querySelector('.modal-backdrop');
                                if (backdrop) {{
                                    backdrop.remove();
                                }}
                            }} catch (e) {{
                                console.error('Modal close error:', e);
                            }}
                        }}
                        
                        // Show success message
                        try {{
                            if (typeof window.showToast === 'function') {{
                                window.showToast('success', '{message}');
                                console.log('[PARTS] Success toast displayed');
                            }} else if (typeof window.showPartToast === 'function') {{
                                window.showPartToast('success', '{message}');
                            }} else {{
                                alert('SUCCESS: {message}');
                            }}
                        }} catch (e) {{
                            console.error('Toast error:', e);
                            alert('SUCCESS: {message}');
                        }}
                        
                        // Clear any validation messages
                        const validationSummary = document.querySelector('.validation-summary');
                        if (validationSummary) {{
                            validationSummary.remove();
                        }}
                        
                        // Clear form validation states
                        const validationElements = document.querySelectorAll('.is-valid, .is-invalid');
                        validationElements.forEach(el => {{
                            el.classList.remove('is-valid', 'is-invalid');
                        }});
                        
                        // Reload page to refresh parts list
                        setTimeout(() => {{
                            console.log('[PARTS] Reloading page to refresh parts list');
                            window.location.reload();
                        }}, 1500);
                    </script>";

                return Content(successScript, "text/html");
            }
            else
            {
                // For standard form requests, redirect with success message
                TempData["SuccessMessage"] = message;
                return RedirectToPage();
            }
        }

        private async Task<IActionResult> HandleHtmxSuccess(string message)
        {
            // Return JavaScript that closes modal and shows success message
            var successScript = $@"
                <script>
                    console.log('[PARTS] HTMX Success handler executed');
                    
                    // Close modal
                    const modal = document.getElementById('partModal');
                    if (modal) {{
                        if (typeof bootstrap !== 'undefined') {{
                            const bsModal = bootstrap.Modal.getInstance(modal);
                            if (bsModal) {{
                                bsModal.hide();
                                console.log('[PARTS] Modal closed via Bootstrap');
                            }}
                        }}

                        // Fallback modal close
                        modal.style.display = 'none';
                        modal.classList.remove('show');
                        document.body.classList.remove('modal-open');
                        
                        // Remove backdrop if exists
                        const backdrop = document.querySelector('.modal-backdrop');
                        if (backdrop) {{
                            backdrop.remove();
                        }}
                    }}
                    
                    // Show success message
                    if (typeof window.showToast === 'function') {{
                        window.showToast('success', '{message}');
                        console.log('[PARTS] Success toast displayed');
                    }} else {{
                        alert('SUCCESS: {message}');
                    }}
                    
                    // Reload page to refresh data
                    setTimeout(() => {{
                        console.log('[PARTS] Reloading page to refresh data');
                        window.location.reload();
                    }}, 1500);
                </script>";

            return Content(successScript, "text/html");
        }
        private string GetFriendlyErrorMessage(Exception ex)
        {
            if (ex.Message.Contains("UNIQUE") || ex.Message.Contains("duplicate"))
                return "A part with this information already exists";
            
            if (ex.InnerException?.Message?.Contains("NOT NULL") == true)
                return "Required field validation failed";
            
            return "Database operation failed";
        }

        private async Task<List<string>> CheckPartDependencies(Part part)
        {
            var dependencies = new List<string>();

            try
            {
                // Check Jobs
                var jobCount = await _context.Jobs.CountAsync(j => j.PartNumber == part.PartNumber);
                if (jobCount > 0) dependencies.Add($"{jobCount} job(s)");

                // Check other dependencies if needed in the future
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking dependencies for part {PartId}", part.Id);
            }

            return dependencies;
        }

        private async Task LoadPartsDataAsync()
        {
            var query = _context.Parts.AsQueryable();

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

            // Enhanced: Add stage filtering
            if (!string.IsNullOrEmpty(StageFilter))
            {
                var partsWithStage = await _context.PartStageRequirements
                    .Include(psr => psr.ProductionStage)
                    .Where(psr => psr.ProductionStage.Name.Contains(StageFilter) && psr.IsActive)
                    .Select(psr => psr.PartId)
                    .ToListAsync();
                
                query = query.Where(p => partsWithStage.Contains(p.Id));
            }

            // Enhanced: Add complexity filtering
            if (!string.IsNullOrEmpty(ComplexityFilter))
            {
                // Note: This is a simplified complexity filter - in practice you'd calculate complexity
                switch (ComplexityFilter.ToLower())
                {
                    case "simple":
                        query = query.Where(p => p.EstimatedHours <= 4);
                        break;
                    case "medium":
                        query = query.Where(p => p.EstimatedHours > 4 && p.EstimatedHours <= 12);
                        break;
                    case "complex":
                        query = query.Where(p => p.EstimatedHours > 12 && p.EstimatedHours <= 24);
                        break;
                    case "very complex":
                        query = query.Where(p => p.EstimatedHours > 24);
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

                AverageEstimatedHours = allParts.Average(p => p.EstimatedHours);
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
                return SortDirection?.ToLower() == "desc" ? "↓" : "↑";
            }
            return "↕";
        }

        public string GetStatusBadgeClass(bool isActive)
        {
            return isActive ? "bg-success" : "bg-secondary";
        }

        #endregion

        /// <summary>
        /// Check if part number already exists (AJAX endpoint)
        /// </summary>
        public async Task<IActionResult> OnGetCheckDuplicateAsync(string partNumber, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(partNumber))
                {
                    return new JsonResult(new { isDuplicate = false, message = "Part number is empty" });
                }

                var query = _context.Parts.Where(p => p.PartNumber == partNumber);
                
                // Exclude current part when editing
                if (excludeId.HasValue && excludeId.Value > 0)
                {
                    query = query.Where(p => p.Id != excludeId.Value);
                }

                var existingPart = await query.FirstOrDefaultAsync();
                var isDuplicate = existingPart != null;

                return new JsonResult(new 
                { 
                    isDuplicate, 
                    message = isDuplicate ? $"Part number '{partNumber}' already exists" : "Part number is available",
                    existingPartId = existingPart?.Id,
                    existingPartName = existingPart?.Name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate part number: {PartNumber}", partNumber);
                return new JsonResult(new { isDuplicate = false, message = "Error checking duplicate" });
            }
        }
    }

    /// <summary>
    /// Enhanced ViewModel for bulletproof data passing to the form
    /// </summary>
    public class PartFormViewModel
    {
        public Part Part { get; set; } = new Part();
        public List<string> AvailableMaterials { get; set; } = new List<string>();
        public List<string> AvailableIndustries { get; set; } = new List<string>();
        public List<string> AvailableApplications { get; set; } = new List<string>();
        public List<ProductionStage> AvailableStages { get; set; } = new List<ProductionStage>();
        public List<PartStageRequirement> ExistingStages { get; set; } = new List<PartStageRequirement>();
    }
}