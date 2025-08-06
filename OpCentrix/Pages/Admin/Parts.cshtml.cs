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

                        // For HTMX requests, return success response
                        if (Request.Headers.ContainsKey("HX-Request"))
                        {
                            return await HandleHtmxSuccess($"Part '{Part.PartNumber}' created successfully with {SelectedStageIds.Count} manufacturing stages");
                        }

                        // For standard requests, redirect with success message
                        TempData["SuccessMessage"] = $"Part '{Part.PartNumber}' created successfully";
                        return RedirectToPage();
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

                        // For HTMX requests, return success response
                        if (Request.Headers.ContainsKey("HX-Request"))
                        {
                            return await HandleHtmxSuccess($"Part '{Part.PartNumber}' updated successfully with {SelectedStageIds.Count} manufacturing stages");
                        }

                        // For standard requests, redirect with success message
                        TempData["SuccessMessage"] = $"Part '{Part.PartNumber}' updated successfully";
                        return RedirectToPage();
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
                Industry = "General Manufacturing",
                Application = "General Component",
                Material = "Ti-6Al-4V Grade 5",
                EstimatedHours = 8.0,
                MaterialCostPerKg = 450.00m,
                StandardLaborCostPerHour = 85.00m,
                PartCategory = "Production",
                PartClass = "B",
                ProcessType = "SLS Metal",
                IsActive = true,
                RequiresSLSPrinting = true,
                RequiresInspection = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "System"
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

            // Admin override validation
            if (part.AdminEstimatedHoursOverride.HasValue)
            {
                if (part.AdminEstimatedHoursOverride.Value <= 0)
                    errors.Add("Admin override hours must be greater than 0");

                if (string.IsNullOrWhiteSpace(part.AdminOverrideReason))
                    errors.Add("Admin override reason is required when override hours are specified");
            }

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

        private async Task<IActionResult> HandleHtmxSuccess(string message)
        {
            // Return JavaScript that closes modal and shows success message
            var successScript = $@"
                <script>
                    console.log('✅ [PARTS] HTMX Success handler executed');
                    
                    // Close modal
                    const modal = document.getElementById('partModal');
                    if (modal) {{
                        if (typeof bootstrap !== 'undefined') {{
                            const bsModal = bootstrap.Modal.getInstance(modal);
                            if (bsModal) {{
                                bsModal.hide();
                                console.log('✅ [PARTS] Modal closed via Bootstrap');
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
                        console.log('✅ [PARTS] Success toast displayed');
                    }} else {{
                        alert('SUCCESS: {message}');
                    }}
                    
                    // Reload page to refresh data
                    setTimeout(() => {{
                        console.log('🔄 [PARTS] Reloading page to refresh data');
                        window.location.reload();
                    }}, 1500);
                </script>";

            return Content(successScript, "text/html");
        }

        private Task<IActionResult> HandleSuccess(string message)
        {
            var successScript = $@"
                <script>
                    // Close modal
                    const modal = document.getElementById('partModal');
                    if (modal && typeof bootstrap !== 'undefined') {{
                        const bsModal = bootstrap.Modal.getInstance(modal);
                        if (bsModal) bsModal.hide();
                    }}
                    
                    // Show success message
                    if (typeof window.showToast === 'function') {{
                        window.showToast('success', '{message}');
                    }}
                    
                    // Redirect after delay
                    setTimeout(() => {{
                        window.location.href = '/Admin/Parts';
                    }}, 1500);
                </script>";

            return Task.FromResult<IActionResult>(Content(successScript, "text/html"));
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