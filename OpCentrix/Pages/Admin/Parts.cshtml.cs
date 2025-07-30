using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.Admin
{
    /// <summary>
    /// Enhanced Parts management page with comprehensive CRUD operations
    /// PHASE 3 OPTIMIZED: Async/await patterns and database query optimization
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

        // Properties for the parts list
        public IList<Part> Parts { get; set; } = new List<Part>();
        
        // Search and filter properties
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
        
        [BindProperty(SupportsGet = true)]
        public string? SortOrder { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }

        // Properties for the part form
        [BindProperty]
        public Part Part { get; set; } = new Part();
        
        [BindProperty]
        public string? Action { get; set; }

        // Lists for dropdowns - Enhanced
        public List<string> AvailableMaterials { get; set; } = new List<string>();
        public List<string> AvailableIndustries { get; set; } = new List<string>();
        public List<string> AvailableCategories { get; set; } = new List<string>();
        public List<string> AvailableProcessTypes { get; set; } = new List<string>();
        public List<string> AvailableMachineTypes { get; set; } = new List<string>();
        
        // Statistics for dashboard
        public int ActivePartsCount { get; set; }
        public int InactivePartsCount { get; set; }
        public string MostUsedMaterial { get; set; } = string.Empty;
        public double AverageEstimatedHours { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("?? Loading Parts management page - Admin: {Admin}", User.Identity?.Name);
                
                await LoadPartsAsync().ConfigureAwait(false);
                await LoadDropdownDataAsync().ConfigureAwait(false);
                await LoadStatisticsAsync().ConfigureAwait(false);
                
                _logger.LogInformation("? Parts page loaded successfully - {TotalCount} parts found", TotalCount);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error loading parts page");
                TempData["ErrorMessage"] = "Error loading parts. Please try again.";
                
                // Initialize empty collections to prevent view errors
                Parts = new List<Part>();
                AvailableMaterials = new List<string>();
                AvailableIndustries = new List<string>();
                AvailableCategories = new List<string>();
                
                return Page();
            }
        }

        // FIXED: Add handler for HTMX modal requests
        public async Task<IActionResult> OnGetAddAsync()
        {
            try
            {
                _logger.LogInformation("?? Loading add part form");
                
                // Initialize a new part with intelligent defaults
                Part = CreateNewPartWithDefaults();
                
                await LoadDropdownDataAsync().ConfigureAwait(false);
                
                // FIXED: Check for both HTMX and XMLHttpRequest headers for modal requests
                bool isAjaxRequest = Request.Headers.ContainsKey("HX-Request") || 
                                   Request.Headers.ContainsKey("X-Requested-With");
                
                if (isAjaxRequest)
                {
                    _logger.LogDebug("? Returning partial view for modal request");
                    return Partial("Shared/_PartFormModal", Part);
                }
                
                _logger.LogDebug("?? Returning full page view");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error loading add part form");
                
                // For AJAX requests, return error partial
                bool isAjaxRequestForError = Request.Headers.ContainsKey("HX-Request") || 
                                           Request.Headers.ContainsKey("X-Requested-With");
                
                if (isAjaxRequestForError)
                {
                    return Content($@"
                        <div class='modal-header bg-danger text-white'>
                            <h5 class='modal-title'>Error Loading Form</h5>
                            <button type='button' class='btn-close btn-close-white' data-bs-dismiss='modal'></button>
                        </div>
                        <div class='modal-body'>
                            <div class='alert alert-danger'>
                                <i class='fas fa-exclamation-triangle me-2'></i>
                                <strong>Error:</strong> {ex.Message}
                                <br><small>Please try refreshing the page and attempting again.</small>
                            </div>
                        </div>
                        <div class='modal-footer'>
                            <button type='button' class='btn btn-secondary' data-bs-dismiss='modal'>Close</button>
                            <button type='button' class='btn btn-primary' onclick='window.location.reload()'>Refresh Page</button>
                        </div>
                    ", "text/html");
                }
                
                return BadRequest("Error loading form.");
            }
        }

        // FIXED: Add handler for HTMX edit modal requests
        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            try
            {
                _logger.LogInformation("?? Loading edit part form for ID: {PartId}", id);
                
                var part = await _context.Parts.FindAsync(id).ConfigureAwait(false);
                if (part == null)
                {
                    _logger.LogWarning("? Part not found for ID: {PartId}", id);
                    
                    // For AJAX requests, return error partial
                    bool isAjaxRequestForNotFound = Request.Headers.ContainsKey("HX-Request") || 
                                                  Request.Headers.ContainsKey("X-Requested-With");
                    
                    if (isAjaxRequestForNotFound)
                    {
                        return Content($@"
                            <div class='modal-header bg-warning text-dark'>
                                <h5 class='modal-title'>Part Not Found</h5>
                                <button type='button' class='btn-close' data-bs-dismiss='modal'></button>
                            </div>
                            <div class='modal-body'>
                                <div class='alert alert-warning'>
                                    <i class='fas fa-exclamation-triangle me-2'></i>
                                    <strong>Part not found:</strong> The part with ID {id} could not be located.
                                    <br><small>It may have been deleted or moved.</small>
                                </div>
                            </div>
                            <div class='modal-footer'>
                                <button type='button' class='btn btn-secondary' data-bs-dismiss='modal'>Close</button>
                                <button type='button' class='btn btn-primary' onclick='window.location.reload()'>Refresh Page</button>
                            </div>
                        ", "text/html");
                    }
                    
                    return NotFound();
                }

                Part = part;
                await LoadDropdownDataAsync().ConfigureAwait(false);
                
                // FIXED: Check for both HTMX and XMLHttpRequest headers for modal requests
                bool isAjaxRequest = Request.Headers.ContainsKey("HX-Request") || 
                                   Request.Headers.ContainsKey("X-Requested-With");
                
                if (isAjaxRequest)
                {
                    _logger.LogDebug("? Returning partial view for edit modal request");
                    return Partial("Shared/_PartFormModal", Part);
                }
                
                _logger.LogDebug("?? Returning full page view for edit");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error loading edit part form for ID {PartId}", id);
                
                // For AJAX requests, return error partial
                bool isAjaxRequestForError = Request.Headers.ContainsKey("HX-Request") || 
                                           Request.Headers.ContainsKey("X-Requested-With");
                
                if (isAjaxRequestForError)
                {
                    return Content($@"
                        <div class='modal-header bg-danger text-white'>
                            <h5 class='modal-title'>Error Loading Part</h5>
                            <button type='button' class='btn-close btn-close-white' data-bs-dismiss='modal'></button>
                        </div>
                        <div class='modal-body'>
                            <div class='alert alert-danger'>
                                <i class='fas fa-exclamation-triangle me-2'></i>
                                <strong>Error loading part {id}:</strong> {ex.Message}
                                <br><small>Please try refreshing the page and attempting again.</small>
                            </div>
                        </div>
                        <div class='modal-footer'>
                            <button type='button' class='btn btn-secondary' data-bs-dismiss='modal'>Close</button>
                            <button type='button' class='btn btn-primary' onclick='window.location.reload()'>Refresh Page</button>
                        </div>
                    ", "text/html");
                }
                
                return BadRequest("Error loading part data.");
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            try
            {
                _logger.LogInformation("?? Creating new part: {PartNumber}", Part.PartNumber);

                // ENHANCED DEBUGGING: Log ModelState validation details
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("? ModelState validation failed for part: {PartNumber}", Part.PartNumber);
                    
                    // Log each specific validation error
                    foreach (var modelError in ModelState)
                    {
                        if (modelError.Value?.Errors.Count > 0)
                        {
                            foreach (var error in modelError.Value.Errors)
                            {
                                _logger.LogWarning("?? VALIDATION ERROR - Field: {FieldName}, Error: {ErrorMessage}", 
                                    modelError.Key, error.ErrorMessage);
                            }
                        }
                    }
                    
                    // Count of validation errors for tracking
                    var errorCount = ModelState.Values.SelectMany(v => v.Errors).Count();
                    _logger.LogWarning("?? Total ModelState errors: {ErrorCount}", errorCount);
                    
                    await LoadDropdownDataAsync().ConfigureAwait(false);
                    return Partial("Shared/_PartFormModal", Part);
                }

                // Enhanced validation
                var validationErrors = await ValidatePartAsync(Part, isCreate: true).ConfigureAwait(false);
                if (validationErrors.Any())
                {
                    _logger.LogWarning("? Business validation failed for part: {PartNumber}", Part.PartNumber);
                    foreach (var error in validationErrors)
                    {
                        _logger.LogWarning("?? BUSINESS ERROR: {ErrorMessage}", error);
                        ModelState.AddModelError("", error);
                    }
                    await LoadDropdownDataAsync().ConfigureAwait(false);
                    return Partial("Shared/_PartFormModal", Part);
                }

                // Set audit fields and defaults
                Part.CreatedBy = User.Identity?.Name ?? "System";
                Part.LastModifiedBy = User.Identity?.Name ?? "System";
                Part.CreatedDate = DateTime.UtcNow;
                Part.LastModifiedDate = DateTime.UtcNow;
                
                // Ensure required fields have defaults
                EnsurePartDefaults(Part);

                _context.Parts.Add(Part);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                _logger.LogInformation("? Part {PartNumber} created successfully by {User}", Part.PartNumber, User.Identity?.Name);
                
                // FIXED: Better success response that preserves styling during reload
                return Content($@"
                    <script>
                        console.log('? Part saved successfully - starting completion sequence');
                        
                        // Create a completion promise chain to ensure everything finishes
                        async function completePartSaveSequence() {{
                            try {{
                                // Step 1: Close the modal with animation
                                console.log('?? Step 1: Closing modal...');
                                const modal = bootstrap.Modal.getInstance(document.getElementById('partModal'));
                                if (modal) {{
                                    modal.hide();
                                }}
                                
                                // Wait for modal close animation to complete
                                await new Promise(resolve => setTimeout(resolve, 350));
                                
                                // Step 2: Show success notification
                                console.log('?? Step 2: Showing success notification...');
                                const notification = document.createElement('div');
                                notification.className = 'alert alert-success alert-dismissible fade show position-fixed';
                                notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; max-width: 400px;';
                                notification.innerHTML = `
                                    <i class='fas fa-check-circle me-2'></i>
                                    <strong>Part '{Part.PartNumber}' ({Part.Name}) created successfully!</strong>
                                    <button type='button' class='btn-close' data-bs-dismiss='alert'></button>
                                `;
                                document.body.appendChild(notification);
                                
                                // Wait for notification to be visible
                                await new Promise(resolve => setTimeout(resolve, 100));
                                
                                // Step 3: Wait for user to see the notification
                                console.log('? Step 3: Displaying notification for user...');
                                await new Promise(resolve => setTimeout(resolve, 1500));
                                
                                // Step 4: Fade out notification before navigation
                                console.log('?? Step 4: Preparing for navigation...');
                                if (notification.parentNode) {{
                                    notification.style.transition = 'opacity 0.3s ease-out';
                                    notification.style.opacity = '0';
                                }}
                                
                                // Wait for fade out
                                await new Promise(resolve => setTimeout(resolve, 300));
                                
                                // Step 5: Now safe to navigate - all JS operations complete
                                console.log('?? Step 5: All operations complete, navigating to refresh parts list...');
                                window.location.href = '/Admin/Parts';
                                
                            }} catch (error) {{
                                console.error('? Error in part save sequence:', error);
                                // Fallback navigation if anything fails
                                window.location.href = '/Admin/Parts';
                            }}
                        }}
                        
                        // Start the completion sequence
                        completePartSaveSequence();
                    </script>
                ", "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error creating part {PartNumber}", Part.PartNumber);
                ModelState.AddModelError("", "Error creating part. Please try again.");
                await LoadDropdownDataAsync().ConfigureAwait(false);
                return Partial("Shared/_PartFormModal", Part);
            }
        }

        public async Task<IActionResult> OnPostEditAsync(int id)
        {
            try
            {
                _logger.LogInformation("?? Editing part ID: {PartId}", id);

                if (!ModelState.IsValid)
                {
                    await LoadDropdownDataAsync().ConfigureAwait(false);
                    return Partial("Shared/_PartFormModal", Part);
                }

                var existingPart = await _context.Parts.FindAsync(id).ConfigureAwait(false);
                if (existingPart == null)
                {
                    ModelState.AddModelError("", "Part not found.");
                    await LoadDropdownDataAsync().ConfigureAwait(false);
                    return Partial("Shared/_PartFormModal", Part);
                }

                // Enhanced validation
                var validationErrors = await ValidatePartAsync(Part, isCreate: false, existingId: id).ConfigureAwait(false);
                if (validationErrors.Any())
                {
                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }
                    await LoadDropdownDataAsync().ConfigureAwait(false);
                    return Partial("Shared/_PartFormModal", Part);
                }

                // Update all properties comprehensively
                UpdatePartFromForm(existingPart, Part);
                
                // Set audit fields
                existingPart.LastModifiedBy = User.Identity?.Name ?? "System";
                existingPart.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync().ConfigureAwait(false);

                _logger.LogInformation("? Part {PartNumber} updated successfully by {User}", existingPart.PartNumber, User.Identity?.Name);
                
                // FIXED: Better success response that preserves styling during reload
                return Content($@"
                    <script>
                        console.log('? Part updated successfully - starting completion sequence');
                        
                        // Create a completion promise chain to ensure everything finishes
                        async function completePartUpdateSequence() {{
                            try {{
                                // Step 1: Close the modal with animation
                                console.log('?? Step 1: Closing modal...');
                                const modal = bootstrap.Modal.getInstance(document.getElementById('partModal'));
                                if (modal) {{
                                    modal.hide();
                                }}
                                
                                // Wait for modal close animation to complete
                                await new Promise(resolve => setTimeout(resolve, 350));
                                
                                // Step 2: Show success notification
                                console.log('?? Step 2: Showing success notification...');
                                const notification = document.createElement('div');
                                notification.className = 'alert alert-success alert-dismissible fade show position-fixed';
                                notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; max-width: 400px;';
                                notification.innerHTML = `
                                    <i class='fas fa-check-circle me-2'></i>
                                    <strong>Part '{existingPart.PartNumber}' ({existingPart.Name}) updated successfully!</strong>
                                    <button type='button' class='btn-close' data-bs-dismiss='alert'></button>
                                `;
                                document.body.appendChild(notification);
                                
                                // Wait for notification to be visible
                                await new Promise(resolve => setTimeout(resolve, 100));
                                
                                // Step 3: Wait for user to see the notification
                                console.log('? Step 3: Displaying notification for user...');
                                await new Promise(resolve => setTimeout(resolve, 1500));
                                
                                // Step 4: Fade out notification before navigation
                                console.log('?? Step 4: Preparing for navigation...');
                                if (notification.parentNode) {{
                                    notification.style.transition = 'opacity 0.3s ease-out';
                                    notification.style.opacity = '0';
                                }}
                                
                                // Wait for fade out
                                await new Promise(resolve => setTimeout(resolve, 300));
                                
                                // Step 5: Now safe to navigate - all JS operations complete
                                console.log('?? Step 5: All operations complete, navigating to refresh parts list...');
                                window.location.href = '/Admin/Parts';
                                
                            }} catch (error) {{
                                console.error('? Error in part update sequence:', error);
                                // Fallback navigation if anything fails
                                window.location.href = '/Admin/Parts';
                            }}
                        }}
                        
                        // Start the completion sequence
                        completePartUpdateSequence();
                    </script>
                ", "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error updating part ID {PartId}", id);
                ModelState.AddModelError("", "Error updating part. Please try again.");
                await LoadDropdownDataAsync().ConfigureAwait(false);
                return Partial("Shared/_PartFormModal", Part);
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("??? Attempting to delete part ID: {PartId}", id);

                var part = await _context.Parts
                    .Include(p => p.Jobs)
                    .Include(p => p.JobNotes)
                    .FirstOrDefaultAsync(p => p.Id == id)
                    .ConfigureAwait(false);
                    
                if (part == null)
                {
                    TempData["ErrorMessage"] = "Part not found.";
                    return RedirectToPage();
                }

                // Enhanced safety checks
                var canDelete = await CanDeletePartAsync(part).ConfigureAwait(false);
                if (!canDelete.CanDelete)
                {
                    TempData["ErrorMessage"] = canDelete.Reason;
                    return RedirectToPage();
                }

                var partNumber = part.PartNumber;
                var partName = part.Name;

                // FIXED: Remove related inspection checkpoints first with error handling
                try
                {
                    var checkpoints = await _context.InspectionCheckpoints
                        .Where(ic => ic.PartId == id)
                        .ToListAsync()
                        .ConfigureAwait(false);
                        
                    if (checkpoints.Any())
                    {
                        _context.InspectionCheckpoints.RemoveRange(checkpoints);
                        _logger.LogInformation("?? Removed {Count} inspection checkpoints for part {PartNumber}", checkpoints.Count, partNumber);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "?? Error removing inspection checkpoints for part {PartNumber}, continuing with deletion", partNumber);
                    // Continue with part deletion even if checkpoint removal fails
                }

                _context.Parts.Remove(part);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                _logger.LogWarning("??? Part {PartNumber} ({PartName}) deleted by {User}", partNumber, partName, User.Identity?.Name);
                TempData["SuccessMessage"] = $"Part '{partNumber}' ({partName}) deleted successfully.";
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error deleting part with ID {PartId}", id);
                TempData["ErrorMessage"] = "Error deleting part. Please try again.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetPartDataAsync(int id)
        {
            try
            {
                var part = await _context.Parts.FindAsync(id).ConfigureAwait(false);
                if (part == null)
                {
                    return NotFound();
                }

                // Return only the data needed for editing
                return new JsonResult(new
                {
                    part.Id,
                    part.PartNumber,
                    part.Name,
                    part.Description,
                    part.Material,
                    part.SlsMaterial,
                    part.Industry,
                    part.Application,
                    part.PartCategory,
                    part.PartClass,
                    part.CustomerPartNumber,
                    part.IsActive,
                    part.EstimatedHours,
                    part.AdminEstimatedHoursOverride,
                    part.AdminOverrideReason,
                    part.MaterialCostPerKg,
                    part.WeightGrams,
                    part.LengthMm,
                    part.WidthMm,
                    part.HeightMm,
                    part.VolumeMm3,
                    part.ProcessType,
                    part.RequiredMachineType,
                    part.RequiresInspection,
                    part.RequiresCertification,
                    part.RequiresFDA,
                    part.RequiresAS9100,
                    part.RequiresNADCAP,
                    part.PowderRequirementKg
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error getting part data for ID {PartId}", id);
                return BadRequest("Error retrieving part data.");
            }
        }

        // LEGACY HANDLERS - Keep for backward compatibility
        public async Task<IActionResult> OnGetAddFormAsync()
        {
            return await OnGetAddAsync().ConfigureAwait(false);
        }

        public async Task<IActionResult> OnGetEditFormAsync(int id)
        {
            return await OnGetEditAsync(id).ConfigureAwait(false);
        }

        /// <summary>
        /// Enhanced part validation with comprehensive business rules
        /// PHASE 3 OPTIMIZED: Added ConfigureAwait(false) to all async calls
        /// </summary>
        private async Task<List<string>> ValidatePartAsync(Part part, bool isCreate, int? existingId = null)
        {
            var errors = new List<string>();

            try
            {
                // Check part number uniqueness with optimized query
                var query = _context.Parts.Where(p => p.PartNumber.ToLower() == part.PartNumber.ToLower());
                if (!isCreate && existingId.HasValue)
                {
                    query = query.Where(p => p.Id != existingId.Value);
                }

                if (await query.AnyAsync().ConfigureAwait(false))
                {
                    errors.Add($"Part number '{part.PartNumber}' already exists.");
                }

                // Business rule validations
                if (part.EstimatedHours <= 0)
                {
                    errors.Add("Estimated hours must be greater than 0.");
                }

                if (part.AdminEstimatedHoursOverride.HasValue)
                {
                    if (part.AdminEstimatedHoursOverride.Value <= 0)
                    {
                        errors.Add("Admin override hours must be greater than 0.");
                    }
                    
                    if (string.IsNullOrWhiteSpace(part.AdminOverrideReason))
                    {
                        errors.Add("Admin override reason is required when override hours are specified.");
                    }
                }

                // Physical dimensions validation
                if (part.LengthMm > 250 || part.WidthMm > 250)
                {
                    errors.Add("Part dimensions exceed TruPrint 3000 build envelope (250x250mm).");
                }

                if (part.HeightMm > 300)
                {
                    errors.Add("Part height exceeds TruPrint 3000 build height (300mm).");
                }

                // Material cost validation
                if (part.MaterialCostPerKg < 0)
                {
                    errors.Add("Material cost cannot be negative.");
                }

                // SLS-specific validations
                if (part.Material != part.SlsMaterial)
                {
                    var compatibleMaterials = GetCompatibleMaterials(part.Material);
                    if (!compatibleMaterials.Contains(part.SlsMaterial))
                    {
                        errors.Add($"SLS Material '{part.SlsMaterial}' is not compatible with base material '{part.Material}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error during part validation");
                errors.Add("An error occurred during validation. Please try again.");
            }

            return errors;
        }

        /// <summary>
        /// Check if a part can be safely deleted
        /// PHASE 3 OPTIMIZED: Added ConfigureAwait(false) to all async calls
        /// </summary>
        private async Task<(bool CanDelete, string Reason)> CanDeletePartAsync(Part part)
        {
            try
            {
                // Check for active jobs with optimized query
                var activeJobs = await _context.Jobs
                    .Where(j => j.PartId == part.Id && j.Status != "Completed" && j.Status != "Cancelled")
                    .CountAsync()
                    .ConfigureAwait(false);

                if (activeJobs > 0)
                {
                    return (false, $"Cannot delete part '{part.PartNumber}' because it has {activeJobs} active job(s). Complete or cancel the jobs first.");
                }

                // Check for any jobs in the last 30 days with optimized query
                var recentJobs = await _context.Jobs
                    .Where(j => j.PartId == part.Id && j.CreatedDate > DateTime.UtcNow.AddDays(-30))
                    .CountAsync()
                    .ConfigureAwait(false);

                if (recentJobs > 0)
                {
                    return (false, $"Cannot delete part '{part.PartNumber}' because it has {recentJobs} recent job(s). Consider marking it as inactive instead.");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error checking if part can be deleted");
                return (false, "Error checking part dependencies. Please try again.");
            }
        }

        /// <summary>
        /// Create a new part with intelligent defaults
        /// </summary>
        private Part CreateNewPartWithDefaults()
        {
            return new Part
            {
                Material = "Ti-6Al-4V Grade 5",
                SlsMaterial = "Ti-6Al-4V Grade 5",
                Industry = "General",
                Application = "General Component",
                PartCategory = "Prototype",
                PartClass = "B",
                ProcessType = "SLS Metal",
                RequiredMachineType = "TruPrint 3000",
                PreferredMachines = "TI1,TI2",
                IsActive = true,
                EstimatedHours = 8.0,
                MaterialCostPerKg = 450.00m,
                StandardLaborCostPerHour = 85.00m,
                SetupCost = 150.00m,
                PostProcessingCost = 75.00m,
                QualityInspectionCost = 50.00m,
                MachineOperatingCostPerHour = 125.00m,
                ArgonCostPerHour = 15.00m,
                RecommendedLaserPower = 200,
                RecommendedScanSpeed = 1200,
                RecommendedLayerThickness = 30,
                RecommendedHatchSpacing = 120,
                RecommendedBuildTemperature = 180,
                RequiredArgonPurity = 99.9,
                MaxOxygenContent = 50,
                PowderRequirementKg = 0.5,
                SetupTimeMinutes = 45,
                PowderChangeoverTimeMinutes = 30,
                PreheatingTimeMinutes = 60,
                CoolingTimeMinutes = 240,
                PostProcessingTimeMinutes = 45,
                RequiresInspection = true,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Ensure part has all required default values to prevent NULL errors
        /// ENHANCED: Comprehensive defaults for ALL database NOT NULL constraints
        /// SEGMENT 1 FIX 1.1.3: Fixed AdminOverrideBy required field issue
        /// </summary>
        private void EnsurePartDefaults(Part part)
        {
            // Basic Information - NOT NULL fields
            part.Description ??= "";
            part.CustomerPartNumber ??= "";
            part.Industry ??= "General";
            part.Application ??= "General Component";
            
            // Material and Process - NOT NULL fields
            part.Material ??= "Ti-6Al-4V Grade 5";
            part.SlsMaterial ??= "Ti-6Al-4V Grade 5";
            part.PowderSpecification ??= "15-45 micron particle size";
            part.ProcessType ??= "SLS Metal";
            part.RequiredMachineType ??= "TruPrint 3000";
            part.PreferredMachines ??= "TI1,TI2";
            
            // Physical Properties - NOT NULL fields
            part.Dimensions ??= "";
            
            // Quality and Surface - NOT NULL fields
            part.SurfaceFinishRequirement ??= "As-built";
            part.QualityStandards ??= "ASTM F3001, ISO 17296";
            part.ToleranceRequirements ??= "±0.1mm typical";
            
            // Skills and Requirements - NOT NULL fields
            part.RequiredSkills ??= "SLS Operation,Powder Handling";
            part.RequiredCertifications ??= "SLS Operation Certification";
            part.RequiredTooling ??= "Build Platform,Powder Sieve";
            part.ConsumableMaterials ??= "Argon Gas,Build Platform Coating";
            part.SupportStrategy ??= "Minimal supports on overhangs > 45°";
            
            // File and Template Information - NOT NULL fields
            part.ProcessParameters ??= "{}";
            part.QualityCheckpoints ??= "{}";
            part.BuildFileTemplate ??= "";
            part.CadFilePath ??= "";
            part.CadFileVersion ??= "";
            
            // Duration and Display - NOT NULL fields
            part.AvgDuration ??= "8h 0m";
            
            // CRITICAL FIX: Admin Override Fields - NOT NULL fields
            // AdminOverrideBy is [Required] but should have a valid default
            if (string.IsNullOrWhiteSpace(part.AdminOverrideBy))
            {
                part.AdminOverrideBy = User.Identity?.Name ?? "System";
                _logger.LogDebug("?? Set AdminOverrideBy to: {AdminOverrideBy}", part.AdminOverrideBy);
            }
            
            // AdminOverrideReason can be empty (nullable string)
            part.AdminOverrideReason ??= "";
            
            // NUMERIC DEFAULTS - Ensure all numeric NOT NULL fields have valid values
            if (part.PowderRequirementKg <= 0) part.PowderRequirementKg = 0.5;
            if (part.RecommendedLaserPower <= 0) part.RecommendedLaserPower = 200;
            if (part.RecommendedScanSpeed <= 0) part.RecommendedScanSpeed = 1200;
            if (part.RecommendedLayerThickness <= 0) part.RecommendedLayerThickness = 30;
            if (part.RecommendedHatchSpacing <= 0) part.RecommendedHatchSpacing = 120;
            if (part.RecommendedBuildTemperature <= 0) part.RecommendedBuildTemperature = 180;
            if (part.RequiredArgonPurity <= 0) part.RequiredArgonPurity = 99.9;
            if (part.MaxOxygenContent <= 0) part.MaxOxygenContent = 50;
            if (part.WeightGrams <= 0) part.WeightGrams = 0;
            if (part.VolumeMm3 <= 0) part.VolumeMm3 = 0;
            if (part.HeightMm <= 0) part.HeightMm = 0;
            if (part.LengthMm <= 0) part.LengthMm = 0;
            if (part.WidthMm <= 0) part.WidthMm = 0;
            if (part.MaxSurfaceRoughnessRa <= 0) part.MaxSurfaceRoughnessRa = 25;
            
            // Cost Fields - NOT NULL decimals
            if (part.MaterialCostPerKg <= 0) part.MaterialCostPerKg = 450.00m;
            if (part.StandardLaborCostPerHour <= 0) part.StandardLaborCostPerHour = 85.00m;
            if (part.SetupCost <= 0) part.SetupCost = 150.00m;
            if (part.PostProcessingCost <= 0) part.PostProcessingCost = 75.00m;
            if (part.QualityInspectionCost <= 0) part.QualityInspectionCost = 50.00m;
            if (part.MachineOperatingCostPerHour <= 0) part.MachineOperatingCostPerHour = 125.00m;
            if (part.ArgonCostPerHour <= 0) part.ArgonCostPerHour = 15.00m;
            if (part.AverageCostPerUnit < 0) part.AverageCostPerUnit = 0;
            if (part.StandardSellingPrice < 0) part.StandardSellingPrice = 0;
            
            // Time Fields - NOT NULL numerics
            if (part.SetupTimeMinutes <= 0) part.SetupTimeMinutes = 45;
            if (part.PowderChangeoverTimeMinutes <= 0) part.PowderChangeoverTimeMinutes = 30;
            if (part.PreheatingTimeMinutes <= 0) part.PreheatingTimeMinutes = 60;
            if (part.CoolingTimeMinutes <= 0) part.CoolingTimeMinutes = 240;
            if (part.PostProcessingTimeMinutes <= 0) part.PostProcessingTimeMinutes = 45;
            if (part.SupportRemovalTimeMinutes < 0) part.SupportRemovalTimeMinutes = 0;
            
            // Performance and Quality Metrics - NOT NULL fields
            if (part.AverageActualHours < 0) part.AverageActualHours = 0;
            if (part.AverageEfficiencyPercent <= 0) part.AverageEfficiencyPercent = 100;
            if (part.AverageQualityScore <= 0) part.AverageQualityScore = 100;
            if (part.AverageDefectRate < 0) part.AverageDefectRate = 0;
            if (part.AveragePowderUtilization <= 0) part.AveragePowderUtilization = 85;
            if (part.TotalJobsCompleted < 0) part.TotalJobsCompleted = 0;
            if (part.TotalUnitsProduced < 0) part.TotalUnitsProduced = 0;
            
            // Duration Days - NOT NULL integer
            if (part.AvgDurationDays <= 0) part.AvgDurationDays = 1;
            
            // Core Business Fields
            part.PartCategory ??= "Prototype";
            part.PartClass ??= "B";
            
            // Ensure EstimatedHours is valid (this is critical)
            if (part.EstimatedHours <= 0) part.EstimatedHours = 8.0;
            
            _logger.LogDebug("? Applied comprehensive defaults to part {PartNumber}", part.PartNumber);
        }

        /// <summary>
        /// Comprehensive part update method
        /// </summary>
        private void UpdatePartFromForm(Part existingPart, Part formPart)
        {
            // Basic information
            existingPart.PartNumber = formPart.PartNumber;
            existingPart.Name = formPart.Name;
            existingPart.Description = formPart.Description ?? "";
            existingPart.CustomerPartNumber = formPart.CustomerPartNumber ?? "";
            existingPart.Industry = formPart.Industry;
            existingPart.Application = formPart.Application;
            existingPart.PartCategory = formPart.PartCategory;
            existingPart.PartClass = formPart.PartClass;
            existingPart.IsActive = formPart.IsActive;

            // Material and process
            existingPart.Material = formPart.Material;
            existingPart.SlsMaterial = formPart.SlsMaterial;
            existingPart.ProcessType = formPart.ProcessType;
            existingPart.RequiredMachineType = formPart.RequiredMachineType;
            existingPart.PreferredMachines = formPart.PreferredMachines ?? "";

            // Physical properties
            existingPart.WeightGrams = formPart.WeightGrams;
            existingPart.LengthMm = formPart.LengthMm;
            existingPart.WidthMm = formPart.WidthMm;
            existingPart.HeightMm = formPart.HeightMm;
            existingPart.VolumeMm3 = formPart.VolumeMm3;
            existingPart.Dimensions = formPart.Dimensions ?? "";

            // Process parameters
            existingPart.RecommendedLaserPower = formPart.RecommendedLaserPower;
            existingPart.RecommendedScanSpeed = formPart.RecommendedScanSpeed;
            existingPart.RecommendedLayerThickness = formPart.RecommendedLayerThickness;
            existingPart.RecommendedHatchSpacing = formPart.RecommendedHatchSpacing;
            existingPart.RecommendedBuildTemperature = formPart.RecommendedBuildTemperature;
            existingPart.RequiredArgonPurity = formPart.RequiredArgonPurity;
            existingPart.MaxOxygenContent = formPart.MaxOxygenContent;

            // Material and powder
            existingPart.PowderSpecification = formPart.PowderSpecification ?? "15-45 micron particle size";
            existingPart.PowderRequirementKg = formPart.PowderRequirementKg;

            // Surface and quality
            existingPart.SurfaceFinishRequirement = formPart.SurfaceFinishRequirement ?? "As-built";
            existingPart.MaxSurfaceRoughnessRa = formPart.MaxSurfaceRoughnessRa;
            existingPart.RequiresInspection = formPart.RequiresInspection;
            existingPart.RequiresCertification = formPart.RequiresCertification;
            existingPart.RequiresFDA = formPart.RequiresFDA;
            existingPart.RequiresAS9100 = formPart.RequiresAS9100;
            existingPart.RequiresNADCAP = formPart.RequiresNADCAP;

            // Support requirements
            existingPart.RequiresSupports = formPart.RequiresSupports;
            existingPart.SupportStrategy = formPart.SupportStrategy ?? "Minimal supports on overhangs > 45°";
            existingPart.SupportRemovalTimeMinutes = formPart.SupportRemovalTimeMinutes;

            // Timing
            existingPart.EstimatedHours = formPart.EstimatedHours;
            existingPart.SetupTimeMinutes = formPart.SetupTimeMinutes;
            existingPart.PowderChangeoverTimeMinutes = formPart.PowderChangeoverTimeMinutes;
            existingPart.PreheatingTimeMinutes = formPart.PreheatingTimeMinutes;
            existingPart.CoolingTimeMinutes = formPart.CoolingTimeMinutes;
            existingPart.PostProcessingTimeMinutes = formPart.PostProcessingTimeMinutes;

            // Admin override fields
            existingPart.AdminEstimatedHoursOverride = formPart.AdminEstimatedHoursOverride;
            existingPart.AdminOverrideReason = formPart.AdminOverrideReason ?? "";
            if (formPart.AdminEstimatedHoursOverride.HasValue)
            {
                existingPart.AdminOverrideBy = User.Identity?.Name ?? "System";
                existingPart.AdminOverrideDate = DateTime.UtcNow;
            }

            // Cost data
            existingPart.MaterialCostPerKg = formPart.MaterialCostPerKg;
            existingPart.StandardLaborCostPerHour = formPart.StandardLaborCostPerHour;
            existingPart.SetupCost = formPart.SetupCost;
            existingPart.PostProcessingCost = formPart.PostProcessingCost;
            existingPart.QualityInspectionCost = formPart.QualityInspectionCost;
            existingPart.MachineOperatingCostPerHour = formPart.MachineOperatingCostPerHour;
            existingPart.ArgonCostPerHour = formPart.ArgonCostPerHour;
            existingPart.StandardSellingPrice = formPart.StandardSellingPrice;

            // Requirements and certifications
            existingPart.QualityStandards = formPart.QualityStandards ?? "ASTM F3001, ISO 17296";
            existingPart.ToleranceRequirements = formPart.ToleranceRequirements ?? "±0.1mm typical";
            existingPart.RequiredSkills = formPart.RequiredSkills ?? "SLS Operation,Powder Handling";
            existingPart.RequiredCertifications = formPart.RequiredCertifications ?? "SLS Operation Certification";
            existingPart.RequiredTooling = formPart.RequiredTooling ?? "Build Platform,Powder Sieve";
            existingPart.ConsumableMaterials = formPart.ConsumableMaterials ?? "Argon Gas,Build Platform Coating";

            // File information
            existingPart.BuildFileTemplate = formPart.BuildFileTemplate ?? "";
            existingPart.CadFilePath = formPart.CadFilePath ?? "";
            existingPart.CadFileVersion = formPart.CadFileVersion ?? "";

            // JSON fields
            existingPart.ProcessParameters = formPart.ProcessParameters ?? "{}";
            existingPart.QualityCheckpoints = formPart.QualityCheckpoints ?? "{}";
        }

        /// <summary>
        /// Get compatible materials for a base material
        /// </summary>
        private string[] GetCompatibleMaterials(string baseMaterial)
        {
            return baseMaterial switch
            {
                "Ti-6Al-4V Grade 5" => new[] { "Ti-6Al-4V Grade 5", "Ti-6Al-4V ELI Grade 23" },
                "Ti-6Al-4V ELI Grade 23" => new[] { "Ti-6Al-4V Grade 5", "Ti-6Al-4V ELI Grade 23" },
                "Inconel 718" => new[] { "Inconel 718", "Inconel 625" },
                "Inconel 625" => new[] { "Inconel 718", "Inconel 625" },
                "316L Stainless Steel" => new[] { "316L Stainless Steel", "17-4 PH Stainless Steel" },
                "17-4 PH Stainless Steel" => new[] { "316L Stainless Steel", "17-4 PH Stainless Steel" },
                "AlSi10Mg" => new[] { "AlSi10Mg" },
                "CoCrMo" => new[] { "CoCrMo" },
                _ => new[] { baseMaterial }
            };
        }

        /// <summary>
        /// Load all page data for error scenarios
        /// </summary>
        private async Task LoadPageDataAsync()
        {
            await LoadPartsAsync().ConfigureAwait(false);
            await LoadDropdownDataAsync().ConfigureAwait(false);
            await LoadStatisticsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Enhanced parts loading with optimized queries
        /// PHASE 3 OPTIMIZED: Added ConfigureAwait(false) and improved query performance
        /// </summary>
        private async Task LoadPartsAsync()
        {
            try
            {
                var query = _context.Parts.AsQueryable();

                // Apply filters
                if (ActiveOnly)
                {
                    query = query.Where(p => p.IsActive);
                }

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    var searchLower = SearchTerm.ToLower();
                    query = query.Where(p => 
                        p.PartNumber.ToLower().Contains(searchLower) ||
                        p.Name.ToLower().Contains(searchLower) ||
                        p.Description.ToLower().Contains(searchLower) ||
                        p.CustomerPartNumber.ToLower().Contains(searchLower));
                }

                if (!string.IsNullOrEmpty(MaterialFilter))
                {
                    query = query.Where(p => p.Material == MaterialFilter);
                }

                if (!string.IsNullOrEmpty(IndustryFilter))
                {
                    query = query.Where(p => p.Industry == IndustryFilter);
                }

                if (!string.IsNullOrEmpty(CategoryFilter))
                {
                    query = query.Where(p => p.PartCategory == CategoryFilter);
                }

                // Apply sorting
                query = SortOrder switch
                {
                    "partnumber_desc" => query.OrderByDescending(p => p.PartNumber),
                    "name" => query.OrderBy(p => p.Name),
                    "name_desc" => query.OrderByDescending(p => p.Name),
                    "material" => query.OrderBy(p => p.Material),
                    "material_desc" => query.OrderByDescending(p => p.Material),
                    "industry" => query.OrderBy(p => p.Industry),
                    "industry_desc" => query.OrderByDescending(p => p.Industry),
                    "created" => query.OrderBy(p => p.CreatedDate),
                    "created_desc" => query.OrderByDescending(p => p.CreatedDate),
                    "hours" => query.OrderBy(p => p.EstimatedHours),
                    "hours_desc" => query.OrderByDescending(p => p.EstimatedHours),
                    "cost" => query.OrderBy(p => p.MaterialCostPerKg),
                    "cost_desc" => query.OrderByDescending(p => p.MaterialCostPerKg),
                    _ => query.OrderBy(p => p.PartNumber)
                };

                // Calculate pagination with optimized count query
                TotalCount = await query.CountAsync().ConfigureAwait(false);
                TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

                // Apply pagination and load data with optimized query
                Parts = await query
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .AsNoTracking() // Read-only for performance
                    .ToListAsync()
                    .ConfigureAwait(false);

                _logger.LogInformation("?? Loaded {Count} parts (Page {Page} of {TotalPages})", Parts.Count, PageNumber, TotalPages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error loading parts data");
                Parts = new List<Part>();
                TotalCount = 0;
                TotalPages = 0;
            }
        }

        /// <summary>
        /// Enhanced dropdown data loading
        /// PHASE 3 OPTIMIZED: Added ConfigureAwait(false) and optimized with single query
        /// </summary>
        private async Task LoadDropdownDataAsync()
        {
            try
            {
                // PERFORMANCE OPTIMIZATION: Single query to load all active parts for dropdown data
                var allParts = await _context.Parts
                    .Where(p => p.IsActive)
                    .Select(p => new { p.Material, p.Industry, p.PartCategory, p.ProcessType, p.RequiredMachineType })
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);

                AvailableMaterials = allParts
                    .Where(p => !string.IsNullOrEmpty(p.Material))
                    .Select(p => p.Material)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                AvailableIndustries = allParts
                    .Where(p => !string.IsNullOrEmpty(p.Industry))
                    .Select(p => p.Industry)
                    .Distinct()
                    .OrderBy(i => i)
                    .ToList();

                AvailableCategories = allParts
                    .Where(p => !string.IsNullOrEmpty(p.PartCategory))
                    .Select(p => p.PartCategory)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                AvailableProcessTypes = allParts
                    .Where(p => !string.IsNullOrEmpty(p.ProcessType))
                    .Select(p => p.ProcessType)
                    .Distinct()
                    .OrderBy(pt => pt)
                    .ToList();

                AvailableMachineTypes = allParts
                    .Where(p => !string.IsNullOrEmpty(p.RequiredMachineType))
                    .Select(p => p.RequiredMachineType)
                    .Distinct()
                    .OrderBy(mt => mt)
                    .ToList();

                _logger.LogInformation("?? Loaded dropdown data - Materials: {Materials}, Industries: {Industries}, Categories: {Categories}",
                    AvailableMaterials.Count, AvailableIndustries.Count, AvailableCategories.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error loading dropdown data");
                // Initialize with empty lists to prevent view errors
                AvailableMaterials = new List<string>();
                AvailableIndustries = new List<string>();
                AvailableCategories = new List<string>();
                AvailableProcessTypes = new List<string>();
                AvailableMachineTypes = new List<string>();
            }
        }

        /// <summary>
        /// Load statistics for dashboard display
        /// PHASE 3 OPTIMIZED: Added ConfigureAwait(false) and optimized queries
        /// </summary>
        private async Task LoadStatisticsAsync()
        {
            try
            {
                // PERFORMANCE OPTIMIZATION: Use parallel queries for statistics
                var activePartsTask = _context.Parts.CountAsync(p => p.IsActive);
                var inactivePartsTask = _context.Parts.CountAsync(p => !p.IsActive);
                var materialUsageTask = _context.Parts
                    .Where(p => p.IsActive)
                    .GroupBy(p => p.Material)
                    .Select(g => new { Material = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefaultAsync();
                var averageHoursTask = _context.Parts
                    .Where(p => p.IsActive)
                    .AverageAsync(p => (double?)p.EstimatedHours);

                // Wait for all tasks to complete
                await Task.WhenAll(activePartsTask, inactivePartsTask, materialUsageTask, averageHoursTask).ConfigureAwait(false);

                ActivePartsCount = await activePartsTask.ConfigureAwait(false);
                InactivePartsCount = await inactivePartsTask.ConfigureAwait(false);
                var materialUsage = await materialUsageTask.ConfigureAwait(false);
                MostUsedMaterial = materialUsage?.Material ?? "None";
                AverageEstimatedHours = await averageHoursTask.ConfigureAwait(false) ?? 0.0;

                _logger.LogInformation("?? Statistics loaded - Active: {Active}, Inactive: {Inactive}, Avg Hours: {AvgHours:F1}",
                    ActivePartsCount, InactivePartsCount, AverageEstimatedHours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error loading statistics");
                ActivePartsCount = 0;
                InactivePartsCount = 0;
                MostUsedMaterial = "Error";
                AverageEstimatedHours = 0.0;
            }
        }

        // DEBUGGING: Add a simple test endpoint to check what data is being received
        [HttpPost]
        public async Task<IActionResult> OnPostDebugCreateAsync()
        {
            try
            {
                _logger.LogInformation("?? DEBUG: Starting part creation debug");
                
                // Log the actual form data received
                _logger.LogInformation("?? DEBUG: Form data received:");
                foreach (var formField in Request.Form)
                {
                    _logger.LogInformation("?? DEBUG: {Key} = {Value}", formField.Key, formField.Value);
                }
                
                // Log the Part object state after model binding
                _logger.LogInformation("?? DEBUG: Part object after model binding:");
                _logger.LogInformation("?? DEBUG: PartNumber = {PartNumber}", Part.PartNumber ?? "NULL");
                _logger.LogInformation("?? DEBUG: Name = {Name}", Part.Name ?? "NULL");
                _logger.LogInformation("?? DEBUG: Description = {Description}", Part.Description ?? "NULL");
                _logger.LogInformation("?? DEBUG: AdminOverrideBy = {AdminOverrideBy}", Part.AdminOverrideBy ?? "NULL");
                
                // Log ModelState validation
                _logger.LogInformation("?? DEBUG: ModelState.IsValid = {IsValid}", ModelState.IsValid);
                
                if (!ModelState.IsValid)
                {
                    foreach (var modelError in ModelState)
                    {
                        if (modelError.Value?.Errors.Count > 0)
                        {
                            foreach (var error in modelError.Value.Errors)
                            {
                                _logger.LogWarning("?? DEBUG VALIDATION ERROR - Field: {FieldName}, Error: {ErrorMessage}", 
                                    modelError.Key, error.ErrorMessage);
                            }
                        }
                    }
                }
                
                return Content("DEBUG: Check logs for detailed information", "text/plain");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? DEBUG: Error in debug endpoint");
                return Content($"DEBUG ERROR: {ex.Message}", "text/plain");
            }
        }
        // Helper methods for the view
        public string GetSortDirection(string column)
        {
            if (SortOrder == column) return "desc";
            if (SortOrder == $"{column}_desc") return "asc";
            return "asc";
        }

        public string GetSortIcon(string column)
        {
            if (SortOrder == column) return "?";
            if (SortOrder == $"{column}_desc") return "?";
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
    }
}