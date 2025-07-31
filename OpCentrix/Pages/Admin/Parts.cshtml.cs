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
    /// Parts management page with comprehensive CRUD operations and best practices
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
                
                Part = await _context.Parts.FindAsync(id);
                if (Part == null)
                {
                    _logger.LogWarning("Part with ID {PartId} not found", id);
                    return NotFound();
                }

                return Partial("Shared/_PartForm", Part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit part form for ID: {PartId}", id);
                return StatusCode(500, "Error loading form");
            }
        }

        /// <summary>
        /// Create new part - ENHANCED with better error handling and validation
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
                    return Partial("Shared/_PartForm", Part);
                }

                // Check for duplicate part number
                var existingPart = await _context.Parts
                    .FirstOrDefaultAsync(p => p.PartNumber == Part.PartNumber);
                
                if (existingPart != null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Duplicate part number: {PartNumber}", operationId, Part.PartNumber);
                    ModelState.AddModelError("Part.PartNumber", $"Part number '{Part.PartNumber}' already exists");
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
                        await transaction.CommitAsync();
                        
                        _logger.LogInformation("? [PARTS-{OperationId}] Part created successfully: {PartNumber} (ID: {PartId})", 
                            operationId, Part.PartNumber, Part.Id);

                        // Return success response with JavaScript to close modal and refresh
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
                                
                                // Redirect after delay to show success message
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
                
                // Check for specific constraint violations
                var errorMessage = "A database error occurred while creating the part.";
                if (dbEx.Message.Contains("UNIQUE") || dbEx.Message.Contains("duplicate"))
                {
                    errorMessage = "A part with this number or unique identifier already exists.";
                }
                else if (dbEx.Message.Contains("FOREIGN KEY") || dbEx.Message.Contains("constraint"))
                {
                    errorMessage = "Invalid reference data. Please check your selections and try again.";
                }
                
                ModelState.AddModelError("", errorMessage);
                return Partial("Shared/_PartForm", Part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Unexpected error creating part: {PartNumber}", operationId, Part?.PartNumber);
                ModelState.AddModelError("", $"An unexpected error occurred while creating the part: {ex.Message}. Please try again.");
                return Partial("Shared/_PartForm", Part);
            }
        }

        /// <summary>
        /// Update existing part - ENHANCED with better error handling and validation
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
                    return Partial("Shared/_PartForm", Part);
                }

                // Get existing part
                var existingPart = await _context.Parts.FindAsync(Part.Id);
                if (existingPart == null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Part not found for update: ID {PartId}", operationId, Part.Id);
                    ModelState.AddModelError("", "Part not found. It may have been deleted by another user.");
                    return Partial("Shared/_PartForm", Part);
                }

                // Check for duplicate part number (excluding current part)
                var duplicatePart = await _context.Parts
                    .FirstOrDefaultAsync(p => p.PartNumber == Part.PartNumber && p.Id != Part.Id);
                
                if (duplicatePart != null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Duplicate part number: {PartNumber}", operationId, Part.PartNumber);
                    ModelState.AddModelError("Part.PartNumber", $"Part number '{Part.PartNumber}' already exists");
                    return Partial("Shared/_PartForm", Part);
                }

                // Preserve audit fields and other important data
                Part.CreatedDate = existingPart.CreatedDate;
                Part.CreatedBy = existingPart.CreatedBy;
                Part.LastModifiedDate = DateTime.UtcNow;
                Part.LastModifiedBy = User.Identity?.Name ?? "System";

                // Use transaction for data integrity
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Update part using explicit property mapping to avoid EF tracking issues
                    _context.Entry(existingPart).CurrentValues.SetValues(Part);
                    
                    var saveResult = await _context.SaveChangesAsync();
                    
                    if (saveResult > 0)
                    {
                        await transaction.CommitAsync();
                        
                        _logger.LogInformation("? [PARTS-{OperationId}] Part updated successfully: {PartNumber} (ID: {PartId})", 
                            operationId, Part.PartNumber, Part.Id);

                        // Return success response with JavaScript to close modal and refresh
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
                                
                                // Redirect after delay to show success message
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
                        ModelState.AddModelError("", "No changes were detected or saved. Please verify your changes and try again.");
                        return Partial("Shared/_PartForm", Part);
                    }
                }
                catch (Exception dbEx)
                {
                    await transaction.RollbackAsync();
                    throw; // Re-throw to be caught by outer catch
                }
            }
            catch (DbUpdateConcurrencyException concurrencyEx)
            {
                _logger.LogError(concurrencyEx, "? [PARTS-{OperationId}] Concurrency error updating part: {PartNumber} (ID: {PartId})", 
                    operationId, Part?.PartNumber, Part?.Id);
                ModelState.AddModelError("", "This part has been modified by another user. Please reload the form and try again.");
                return Partial("Shared/_PartForm", Part);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "? [PARTS-{OperationId}] Database error updating part: {PartNumber} (ID: {PartId})", 
                    operationId, Part?.PartNumber, Part?.Id);
                ModelState.AddModelError("", "A database error occurred while updating the part. Please check for data conflicts and try again.");
                return Partial("Shared/_PartForm", Part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Unexpected error updating part: {PartNumber} (ID: {PartId})", 
                    operationId, Part?.PartNumber, Part?.Id);
                ModelState.AddModelError("", $"An unexpected error occurred while updating the part: {ex.Message}. Please try again.");
                return Partial("Shared/_PartForm", Part);
            }
        }

        /// <summary>
        /// ENHANCED: Delete part with comprehensive dependency checking and error handling
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

                // ENHANCED: Check multiple dependency types with safe error handling
                var dependencies = new List<string>();
                
                try
                {
                    // Check Jobs (using PartNumber reference)
                    var jobCount = await _context.Jobs.CountAsync(j => j.PartNumber == part.PartNumber);
                    if (jobCount > 0)
                    {
                        dependencies.Add($"{jobCount} scheduled job(s)");
                    }
                }
                catch (Exception jobEx)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Error checking job dependencies: {Error}", operationId, jobEx.Message);
                }

                try
                {
                    // Check BuildJobs (using PartId reference)  
                    var buildJobCount = await _context.BuildJobs.CountAsync(bj => bj.PartId == part.Id);
                    if (buildJobCount > 0)
                    {
                        dependencies.Add($"{buildJobCount} build job(s)");
                    }
                }
                catch (Exception buildJobEx)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Error checking build job dependencies: {Error}", operationId, buildJobEx.Message);
                }

                try
                {
                    // Check SerialNumbers
                    var serialNumberCount = await _context.SerialNumbers.CountAsync(sn => sn.PartId == part.Id);
                    if (serialNumberCount > 0)
                    {
                        dependencies.Add($"{serialNumberCount} serial number(s)");
                    }
                }
                catch (Exception serialEx)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Error checking serial number dependencies: {Error}", operationId, serialEx.Message);
                }

                try
                {
                    // Check JobNotes
                    var jobNoteCount = await _context.JobNotes.CountAsync(jn => jn.PartId == part.Id);
                    if (jobNoteCount > 0)
                    {
                        dependencies.Add($"{jobNoteCount} job note(s)");
                    }
                }
                catch (Exception noteEx)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Error checking job note dependencies: {Error}", operationId, noteEx.Message);
                }

                try
                {
                    // Check InspectionCheckpoints
                    var checkpointCount = await _context.InspectionCheckpoints.CountAsync(ic => ic.PartId == part.Id);
                    if (checkpointCount > 0)
                    {
                        dependencies.Add($"{checkpointCount} inspection checkpoint(s)");
                    }
                }
                catch (Exception checkpointEx)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Error checking inspection checkpoint dependencies: {Error}", operationId, checkpointEx.Message);
                }

                try
                {
                    // Check ComplianceDocuments
                    var complianceDocCount = await _context.ComplianceDocuments.CountAsync(cd => cd.PartId == part.Id);
                    if (complianceDocCount > 0)
                    {
                        dependencies.Add($"{complianceDocCount} compliance document(s)");
                    }
                }
                catch (Exception complianceEx)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Error checking compliance document dependencies: {Error}", operationId, complianceEx.Message);
                }

                try
                {
                    // Check PrototypeJobs
                    var prototypeJobCount = await _context.PrototypeJobs.CountAsync(pj => pj.PartId == part.Id);
                    if (prototypeJobCount > 0)
                    {
                        dependencies.Add($"{prototypeJobCount} prototype job(s)");
                    }
                }
                catch (Exception prototypeEx)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Error checking prototype job dependencies: {Error}", operationId, prototypeEx.Message);
                }

                if (dependencies.Any())
                {
                    var dependencyList = string.Join(", ", dependencies);
                    _logger.LogWarning("?? [PARTS-{OperationId}] Cannot delete part with dependencies: {PartNumber} - Dependencies: {Dependencies}", 
                        operationId, part.PartNumber, dependencyList);
                    TempData["ErrorMessage"] = $"Cannot delete part '{part.PartNumber}' because it is referenced by {dependencyList}. Please remove or complete those items first.";
                    return RedirectToPage();
                }

                // Proceed with deletion using transaction
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    _context.Parts.Remove(part);
                    var saveResult = await _context.SaveChangesAsync();
                    
                    if (saveResult > 0)
                    {
                        await transaction.CommitAsync();
                        
                        _logger.LogInformation("? [PARTS-{OperationId}] Part deleted successfully: {PartNumber} (ID: {PartId})", 
                            operationId, part.PartNumber, part.Id);

                        TempData["SuccessMessage"] = $"Part '{part.PartNumber}' deleted successfully";
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
                    throw; // Re-throw to be caught by outer catch
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "? [PARTS-{OperationId}] Database error deleting part ID: {PartId}", operationId, id);
                TempData["ErrorMessage"] = "Database error occurred while deleting the part. The part may be referenced by other data that prevents deletion.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Unexpected error deleting part ID: {PartId}", operationId, id);
                TempData["ErrorMessage"] = $"An unexpected error occurred while deleting the part: {ex.Message}. Please try again or contact support if the issue persists.";
                return RedirectToPage();
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
            {
                errors.Add("Part Number is required");
            }
            else if (part.PartNumber.Length > 50)
            {
                errors.Add("Part Number cannot exceed 50 characters");
            }

            if (string.IsNullOrWhiteSpace(part.Name))
            {
                errors.Add("Part Name is required");
            }
            else if (part.Name.Length > 200)
            {
                errors.Add("Part Name cannot exceed 200 characters");
            }

            if (string.IsNullOrWhiteSpace(part.Description))
            {
                errors.Add("Description is required");
            }

            if (string.IsNullOrWhiteSpace(part.Industry))
            {
                errors.Add("Industry is required");
            }

            if (string.IsNullOrWhiteSpace(part.Application))
            {
                errors.Add("Application is required");
            }

            if (string.IsNullOrWhiteSpace(part.Material))
            {
                errors.Add("Material is required");
            }

            if (string.IsNullOrWhiteSpace(part.SlsMaterial))
            {
                errors.Add("SLS Material is required");
            }

            if (part.EstimatedHours <= 0)
            {
                errors.Add("Estimated Hours must be greater than 0");
            }

            // Numeric validation
            if (part.MaterialCostPerKg < 0)
            {
                errors.Add("Material cost cannot be negative");
            }

            if (part.AdminEstimatedHoursOverride.HasValue && part.AdminEstimatedHoursOverride.Value <= 0)
            {
                errors.Add("Admin override hours must be greater than 0 if specified");
            }

            if (part.AdminEstimatedHoursOverride.HasValue && string.IsNullOrWhiteSpace(part.AdminOverrideReason))
            {
                errors.Add("Admin override reason is required when override hours are specified");
            }

            return errors;
        }

        private async Task LoadPartsDataAsync()
        {
            // Parse pagination parameters
            if (Request.Query.ContainsKey("PageNumber") && int.TryParse(Request.Query["PageNumber"], out int pageNum))
            {
                PageNumber = Math.Max(1, pageNum);
            }
            
            if (Request.Query.ContainsKey("PageSize") && int.TryParse(Request.Query["PageSize"], out int pageSize))
            {
                PageSize = Math.Max(1, Math.Min(100, pageSize)); // Limit to reasonable range
            }

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