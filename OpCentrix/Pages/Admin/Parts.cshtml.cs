using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Authorization;
using System.Text.Json;

namespace OpCentrix.Pages.Admin
{
    [AdminOnly]
    public class PartsModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PartsModel> _logger;

        public PartsModel(SchedulerContext context, ILogger<PartsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Pagination and filtering properties
        public List<Part> Parts { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalParts { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public string MaterialFilter { get; set; } = string.Empty;
        public string ActiveFilter { get; set; } = string.Empty;

        public async Task OnGetAsync(int page = 1, string search = "", string material = "", string active = "")
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PARTS-{OperationId}] Loading parts page: page={Page}, search='{Search}', material='{Material}', active='{Active}'", 
                operationId, page, search, material, active);

            try
            {
                // DEFENSIVE VALIDATION: Sanitize inputs to prevent format exceptions
                PageNumber = Math.Max(1, Math.Min(1000, page)); // Clamp to reasonable range
                SearchTerm = search?.Trim() ?? string.Empty;
                MaterialFilter = material?.Trim() ?? string.Empty;
                ActiveFilter = active?.Trim() ?? string.Empty;

                // Validate search term length to prevent excessive queries
                if (SearchTerm.Length > 100)
                {
                    SearchTerm = SearchTerm.Substring(0, 100);
                    _logger.LogWarning("?? [PARTS-{OperationId}] Search term truncated to 100 characters", operationId);
                }

                // Validate material filter
                if (MaterialFilter.Length > 50)
                {
                    MaterialFilter = MaterialFilter.Substring(0, 50);
                    _logger.LogWarning("?? [PARTS-{OperationId}] Material filter truncated to 50 characters", operationId);
                }

                _logger.LogDebug("?? [PARTS-{OperationId}] Building query with validated filters", operationId);

                // Build query with filters
                var query = _context.Parts.AsQueryable();

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    query = query.Where(p => p.PartNumber.Contains(SearchTerm) || 
                                           p.Description.Contains(SearchTerm));
                    _logger.LogDebug("?? [PARTS-{OperationId}] Applied search filter: '{SearchTerm}'", operationId, SearchTerm);
                }

                if (!string.IsNullOrEmpty(MaterialFilter))
                {
                    query = query.Where(p => p.Material.Contains(MaterialFilter));
                    _logger.LogDebug("?? [PARTS-{OperationId}] Applied material filter: '{MaterialFilter}'", operationId, MaterialFilter);
                }

                if (!string.IsNullOrEmpty(ActiveFilter))
                {
                    // DEFENSIVE: Safer bool parsing to prevent ArgumentException
                    if (string.Equals(ActiveFilter, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(p => p.IsActive == true);
                        _logger.LogDebug("?? [PARTS-{OperationId}] Applied active filter: true", operationId);
                    }
                    else if (string.Equals(ActiveFilter, "false", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(p => p.IsActive == false);
                        _logger.LogDebug("?? [PARTS-{OperationId}] Applied active filter: false", operationId);
                    }
                    else
                    {
                        _logger.LogWarning("?? [PARTS-{OperationId}] Invalid active filter value: '{ActiveFilter}', ignoring", 
                            operationId, ActiveFilter);
                        ActiveFilter = string.Empty; // Clear invalid value
                    }
                }

                // Get total count for pagination
                TotalParts = await query.CountAsync();
                TotalPages = (int)Math.Ceiling(TotalParts / (double)PageSize);
                
                // Validate page number against total pages
                if (PageNumber > TotalPages && TotalPages > 0)
                {
                    PageNumber = TotalPages;
                    _logger.LogWarning("?? [PARTS-{OperationId}] Page number adjusted to {PageNumber} (max available)", 
                        operationId, PageNumber);
                }
                
                _logger.LogDebug("?? [PARTS-{OperationId}] Query statistics: {TotalParts} total parts, {TotalPages} pages", 
                    operationId, TotalParts, TotalPages);

                // Get parts for current page
                Parts = await query
                    .OrderBy(p => p.PartNumber)
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("? [PARTS-{OperationId}] Parts page loaded successfully: {PartsCount} parts on page {Page}/{TotalPages}", 
                    operationId, Parts.Count, PageNumber, TotalPages);
            }
            catch (FormatException fEx)
            {
                _logger.LogError(fEx, "?? [PARTS-{OperationId}] Format error loading parts page: {ErrorMessage}", 
                    operationId, fEx.Message);
                
                // Provide fallback data with safe defaults
                Parts = new List<Part>();
                TotalParts = 0;
                TotalPages = 0;
                PageNumber = 1;
                SearchTerm = string.Empty;
                MaterialFilter = string.Empty;
                ActiveFilter = string.Empty;
                
                TempData["ErrorMessage"] = $"Data format error (ID: {operationId}). Please check your search parameters.";
            }
            catch (ArgumentException aEx)
            {
                _logger.LogError(aEx, "?? [PARTS-{OperationId}] Argument error loading parts page: {ErrorMessage}", 
                    operationId, aEx.Message);
                
                // Provide fallback data with safe defaults
                Parts = new List<Part>();
                TotalParts = 0;
                TotalPages = 0;
                PageNumber = 1;
                SearchTerm = string.Empty;
                MaterialFilter = string.Empty;
                ActiveFilter = string.Empty;
                
                TempData["ErrorMessage"] = $"Invalid parameter error (ID: {operationId}). Parameters have been reset.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error loading parts page: {ErrorMessage}", 
                    operationId, ex.Message);
                
                // Provide fallback data
                Parts = new List<Part>();
                TotalParts = 0;
                TotalPages = 0;
                
                // Add error message for user
                TempData["ErrorMessage"] = $"Error loading parts (ID: {operationId}): {ex.Message}";
            }
        }

        public async Task<IActionResult> OnGetAddAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [PARTS-{OperationId}] Opening add part modal", operationId);

            try
            {
                var newPart = new Part
                {
                    // CRITICAL FIX: Set comprehensive defaults to prevent NOT NULL and format errors
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System",
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = User.Identity?.Name ?? "System",
                    
                    // Basic required fields with safe defaults
                    PartNumber = "", // Will be filled by user
                    Description = "", // Will be filled by user
                    Industry = "General",
                    Application = "General Component",
                    
                    // Manufacturing defaults
                    EstimatedHours = 8.0,
                    AvgDuration = "8.0h",
                    AvgDurationDays = 1,
                    
                    // Cost defaults to prevent decimal conversion errors
                    MaterialCostPerKg = 450.00m, // Default for titanium, will adjust for Inconel
                    StandardLaborCostPerHour = 85.00m,
                    SetupCost = 125.00m,
                    
                    // SLS parameter defaults to prevent conversion errors
                    RecommendedLaserPower = 200,
                    RecommendedScanSpeed = 1200,
                    RecommendedBuildTemperature = 180,
                    RecommendedLayerThickness = 30,
                    RecommendedHatchSpacing = 120,
                    RequiredArgonPurity = 99.9,
                    MaxOxygenContent = 50,
                    
                    // Time parameter defaults
                    PreheatingTimeMinutes = 60,
                    CoolingTimeMinutes = 120,
                    PostProcessingTimeMinutes = 90,
                    
                    // Dimensional defaults (set to 0 for non-nullable fields)
                    WeightGrams = 0,
                    LengthMm = 0,
                    WidthMm = 0,
                    HeightMm = 0,
                    VolumeMm3 = 0,
                    PowderRequirementKg = 1.5,
                    
                    // Material defaults - start with titanium
                    Material = "Ti-6Al-4V Grade 5",
                    SlsMaterial = "Ti-6Al-4V Grade 5",
                    
                    // Classification defaults
                    PartCategory = "Production",
                    PartClass = "B",
                    
                    // Audit fields
                    TotalJobsCompleted = 0,
                    TotalUnitsProduced = 0
                };

                _logger.LogDebug("? [PARTS-{OperationId}] Add part modal prepared with comprehensive defaults", operationId);
                return Partial("Shared/_PartForm", newPart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error opening add part modal: {ErrorMessage}", 
                    operationId, ex.Message);
                
                return Content($@"
                    <script>
                        console.error('Error opening add part modal (ID: {operationId}): {ex.Message}');
                        if (typeof showErrorNotification === 'function') {{
                            showErrorNotification('Error opening add part form (ID: {operationId}): {ex.Message}', 8000);
                        }} else {{
                            alert('Error opening add part form (ID: {operationId}): {ex.Message}');
                        }}
                        if (typeof hideModal === 'function') {{ hideModal(); }}
                    </script>
                ", "text/html");
            }
        }

        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [PARTS-{OperationId}] Opening edit part modal for ID: {PartId}", operationId, id);

            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Invalid part ID: {PartId}", operationId, id);
                    throw new ArgumentException($"Invalid part ID: {id}");
                }

                var part = await _context.Parts.FindAsync(id);
                
                if (part == null)
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Part not found with ID: {PartId}", operationId, id);
                    
                    return Content($@"
                        <script>
                            console.warn('Part not found (ID: {operationId}): Part ID {id} does not exist');
                            if (typeof showErrorNotification === 'function') {{
                                showErrorNotification('Part not found (ID: {operationId})', 6000);
                            }} else {{
                                alert('Part not found (ID: {operationId})');
                            }}
                            if (typeof hideModal === 'function') {{ hideModal(); }}
                        </script>
                    ", "text/html");
                }

                _logger.LogDebug("? [PARTS-{OperationId}] Edit part modal prepared for part: {PartNumber}", 
                    operationId, part.PartNumber);
                
                return Partial("Shared/_PartForm", part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Error opening edit part modal for ID {PartId}: {ErrorMessage}", 
                    operationId, id, ex.Message);
                
                return Content($@"
                    <script>
                        console.error('Error opening edit part modal (ID: {operationId}): {ex.Message}');
                        if (typeof showErrorNotification === 'function') {{
                            showErrorNotification('Error opening edit form (ID: {operationId}): {ex.Message}', 8000);
                        }} else {{
                            alert('Error opening edit form (ID: {operationId}): {ex.Message}');
                        }}
                        if (typeof hideModal === 'function') {{ hideModal(); }}
                    </script>
                ", "text/html");
            }
        }

        public async Task<IActionResult> OnPostSaveAsync([FromForm] Part part)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PARTS-{OperationId}] Processing part save: Id={PartId}, PartNumber='{PartNumber}', Description='{Description}'", 
                operationId, part.Id, part.PartNumber, part.Description);

            try
            {
                // CRITICAL FIX: Defensive input sanitization to prevent FormatException/ArgumentException
                if (part == null)
                {
                    _logger.LogError("? [PARTS-{OperationId}] Part object is null", operationId);
                    throw new ArgumentNullException(nameof(part), "Part data is required");
                }

                // Sanitize and validate string inputs to prevent format exceptions
                part.PartNumber = part.PartNumber?.Trim()?.ToUpperInvariant() ?? string.Empty;
                part.Description = part.Description?.Trim() ?? string.Empty;
                part.Material = part.Material?.Trim() ?? string.Empty;
                part.SlsMaterial = part.SlsMaterial?.Trim() ?? string.Empty;
                part.Industry = part.Industry?.Trim() ?? "General";
                part.Application = part.Application?.Trim() ?? "General Component";
                part.PartCategory = part.PartCategory?.Trim() ?? string.Empty;
                part.PartClass = part.PartClass?.Trim() ?? string.Empty;

                // CRITICAL FIX: Validate and sanitize numeric inputs to prevent TypeConverter errors
                if (part.EstimatedHours <= 0 || part.EstimatedHours > 1000 || double.IsNaN(part.EstimatedHours) || double.IsInfinity(part.EstimatedHours))
                {
                    _logger.LogWarning("?? [PARTS-{OperationId}] Invalid EstimatedHours value: {EstimatedHours}, setting to default", 
                        operationId, part.EstimatedHours);
                    part.EstimatedHours = 8.0; // Safe default
                }

                // Sanitize dimensional inputs
                part.LengthMm = SanitizeDouble(part.LengthMm, "LengthMm", operationId);
                part.WidthMm = SanitizeDouble(part.WidthMm, "WidthMm", operationId);
                part.HeightMm = SanitizeDouble(part.HeightMm, "HeightMm", operationId);
                part.WeightGrams = SanitizeDouble(part.WeightGrams, "WeightGrams", operationId);
                part.VolumeMm3 = SanitizeDouble(part.VolumeMm3, "VolumeMm3", operationId);
                part.PowderRequirementKg = SanitizeDouble(part.PowderRequirementKg, "PowderRequirementKg", operationId);

                // Sanitize process parameters
                part.RecommendedLaserPower = SanitizeDouble(part.RecommendedLaserPower, "RecommendedLaserPower", operationId);
                part.RecommendedScanSpeed = SanitizeDouble(part.RecommendedScanSpeed, "RecommendedScanSpeed", operationId);
                part.RecommendedBuildTemperature = SanitizeDouble(part.RecommendedBuildTemperature, "RecommendedBuildTemperature", operationId);
                part.RecommendedLayerThickness = SanitizeDouble(part.RecommendedLayerThickness, "RecommendedLayerThickness", operationId);
                part.RecommendedHatchSpacing = SanitizeDouble(part.RecommendedHatchSpacing, "RecommendedHatchSpacing", operationId);

                // Sanitize process quality parameters
                part.RequiredArgonPurity = SanitizeDouble(part.RequiredArgonPurity, "RequiredArgonPurity", operationId);
                part.MaxOxygenContent = SanitizeDouble(part.MaxOxygenContent, "MaxOxygenContent", operationId);

                // Sanitize time parameters
                part.PreheatingTimeMinutes = SanitizeDouble(part.PreheatingTimeMinutes, "PreheatingTimeMinutes", operationId);
                part.CoolingTimeMinutes = SanitizeDouble(part.CoolingTimeMinutes, "CoolingTimeMinutes", operationId);
                part.PostProcessingTimeMinutes = SanitizeDouble(part.PostProcessingTimeMinutes, "PostProcessingTimeMinutes", operationId);

                // Sanitize cost parameters with safe decimal conversion
                part.MaterialCostPerKg = SanitizeDecimal(part.MaterialCostPerKg, "MaterialCostPerKg", operationId);
                part.StandardLaborCostPerHour = SanitizeDecimal(part.StandardLaborCostPerHour, "StandardLaborCostPerHour", operationId);
                part.SetupCost = SanitizeDecimal(part.SetupCost, "SetupCost", operationId);
                _logger.LogDebug("? [PARTS-{OperationId}] Input sanitization completed successfully", operationId);

                // Enhanced validation with detailed logging
                var validationErrors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(part.PartNumber))
                {
                    validationErrors.Add("Part Number is required");
                    _logger.LogWarning("?? [PARTS-{OperationId}] Validation failed: Missing part number", operationId);
                }
                else if (part.PartNumber.Length > 50)
                {
                    validationErrors.Add("Part Number cannot exceed 50 characters");
                    _logger.LogWarning("?? [PARTS-{OperationId}] Validation failed: Part number too long: {Length}", 
                        operationId, part.PartNumber.Length);
                }

                if (string.IsNullOrWhiteSpace(part.Description))
                {
                    validationErrors.Add("Description is required");
                    _logger.LogWarning("?? [PARTS-{OperationId}] Validation failed: Missing description", operationId);
                }
                else if (part.Description.Length > 500)
                {
                    validationErrors.Add("Description cannot exceed 500 characters");
                    _logger.LogWarning("?? [PARTS-{OperationId}] Validation failed: Description too long: {Length}", 
                        operationId, part.Description.Length);
                }

                // CRITICAL FIX: Check for duplicate part numbers with proper error handling
                try
                {
                    var duplicateExists = await _context.Parts
                        .AnyAsync(p => p.PartNumber == part.PartNumber && p.Id != part.Id);
                    
                    if (duplicateExists)
                    {
                        validationErrors.Add($"Part Number '{part.PartNumber}' already exists. Please use a different part number.");
                        _logger.LogWarning("?? [PARTS-{OperationId}] Validation failed: Duplicate part number '{PartNumber}'", 
                            operationId, part.PartNumber);
                    }
                }
                catch (Exception dupCheckEx)
                {
                    _logger.LogError(dupCheckEx, "? [PARTS-{OperationId}] Error checking for duplicate part number: {ErrorMessage}", 
                        operationId, dupCheckEx.Message);
                    validationErrors.Add("Unable to verify part number uniqueness. Please try again.");
                }

                if (validationErrors.Any())
                {
                    _logger.LogWarning("? [PARTS-{OperationId}] Part save failed validation: {ErrorCount} errors", 
                        operationId, validationErrors.Count);
                    
                    ViewData["ValidationErrors"] = validationErrors;
                    return Partial("Shared/_PartForm", part);
                }

                var currentUser = User.Identity?.Name ?? "System";
                var now = DateTime.UtcNow;

                if (part.Id == 0)
                {
                    // Create new part
                    _logger.LogDebug("? [PARTS-{OperationId}] Creating new part: {PartNumber}", operationId, part.PartNumber);
                    
                    part.CreatedDate = now;
                    part.CreatedBy = currentUser;
                    part.LastModifiedDate = now;
                    part.LastModifiedBy = currentUser;
                    
                    // Set calculated fields with safe conversions
                    part.AvgDuration = $"{part.EstimatedHours:F1}h";
                    part.AvgDurationDays = (int)Math.Ceiling(Math.Max(part.EstimatedHours / 8, 1));
                    
                    // CRITICAL FIX: Ensure all required fields have values
                    part.IsActive = part.IsActive; // Keep user selection
                    part.TotalJobsCompleted = 0;
                    part.TotalUnitsProduced = 0;
                    
                    await _context.Parts.AddAsync(part);
                    _logger.LogDebug("? [PARTS-{OperationId}] New part added to context", operationId);
                }
                else
                {
                    // Update existing part
                    _logger.LogDebug("?? [PARTS-{OperationId}] Updating existing part: {PartNumber} (ID: {PartId})", 
                        operationId, part.PartNumber, part.Id);
                    
                    var existingPart = await _context.Parts.FindAsync(part.Id);
                    if (existingPart == null)
                    {
                        _logger.LogWarning("?? [PARTS-{OperationId}] Part not found for update: ID {PartId}", operationId, part.Id);
                        throw new InvalidOperationException($"Part with ID {part.Id} not found for update");
                    }

                    // Log changes for audit trail
                    var changes = new List<string>();
                    if (existingPart.PartNumber != part.PartNumber)
                        changes.Add($"PartNumber: {existingPart.PartNumber} -> {part.PartNumber}");
                    if (existingPart.Description != part.Description)
                        changes.Add($"Description: {existingPart.Description} -> {part.Description}");
                    if (Math.Abs(existingPart.EstimatedHours - part.EstimatedHours) > 0.01)
                        changes.Add($"EstimatedHours: {existingPart.EstimatedHours} -> {part.EstimatedHours}");
                    
                    if (changes.Any())
                    {
                        _logger.LogDebug("?? [PARTS-{OperationId}] Part changes detected: {Changes}", 
                            operationId, string.Join("; ", changes));
                    }

                    // Apply all updates with safe assignments
                    existingPart.PartNumber = part.PartNumber;
                    existingPart.Description = part.Description;
                    existingPart.Material = part.Material;
                    existingPart.SlsMaterial = part.SlsMaterial;
                    existingPart.EstimatedHours = part.EstimatedHours;
                    existingPart.AvgDuration = $"{part.EstimatedHours:F1}h";
                    existingPart.AvgDurationDays = (int)Math.Ceiling(Math.Max(part.EstimatedHours / 8, 1));
                    
                    // Update dimensional properties with safe assignments
                    existingPart.WeightGrams = part.WeightGrams;
                    existingPart.LengthMm = part.LengthMm;
                    existingPart.WidthMm = part.WidthMm;
                    existingPart.HeightMm = part.HeightMm;
                    existingPart.VolumeMm3 = part.VolumeMm3;
                    existingPart.PowderRequirementKg = part.PowderRequirementKg;
                    
                    // Update process parameters
                    existingPart.RecommendedLaserPower = part.RecommendedLaserPower;
                    existingPart.RecommendedScanSpeed = part.RecommendedScanSpeed;
                    existingPart.RecommendedLayerThickness = part.RecommendedLayerThickness;
                    existingPart.RecommendedHatchSpacing = part.RecommendedHatchSpacing;
                    existingPart.RecommendedBuildTemperature = part.RecommendedBuildTemperature;
                    existingPart.RequiredArgonPurity = part.RequiredArgonPurity;
                    existingPart.MaxOxygenContent = part.MaxOxygenContent;
                    
                    // Update time parameters
                    existingPart.PreheatingTimeMinutes = part.PreheatingTimeMinutes;
                    existingPart.CoolingTimeMinutes = part.CoolingTimeMinutes;
                    existingPart.PostProcessingTimeMinutes = part.PostProcessingTimeMinutes;
                    
                    // Update cost parameters
                    existingPart.MaterialCostPerKg = part.MaterialCostPerKg;
                    existingPart.StandardLaborCostPerHour = part.StandardLaborCostPerHour;
                    existingPart.SetupCost = part.SetupCost;

                    existingPart.LastModifiedDate = now;
                    existingPart.LastModifiedBy = currentUser;
                    
                    _logger.LogDebug("? [PARTS-{OperationId}] Part updated successfully", operationId);
                }

                // CRITICAL FIX: Ensure navigation properties are not loaded by default
                // This prevents loading related entities unintentionally, keeping the context clean
                _context.Entry(part).State = EntityState.Detached;
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("? [PARTS-{OperationId}] Part saved successfully: Id={PartId}, PartNumber='{PartNumber}'", 
                    operationId, part.Id, part.PartNumber);
                
                return Content($@"
                    <script>
                        console.log('Part saved successfully (ID: {operationId})');
                        if (typeof hideModal === 'function') {{ hideModal(); }}
                        if (typeof refreshPartsGrid === 'function') {{ refreshPartsGrid(); }}
                    </script>
                ", "text/html");
            }
            catch (FormatException fEx)
            {
                _logger.LogError(fEx, "?? [PARTS-{OperationId}] Format error saving part: {ErrorMessage}", 
                    operationId, fEx.Message);
                
                return Content($@"
                    <script>
                        console.error('Format error saving part (ID: {operationId}): {fEx.Message}');
                        if (typeof showErrorNotification === 'function') {{
                            showErrorNotification('Format error saving part (ID: {operationId}): {fEx.Message}', 8000);
                        }} else {{
                            alert('Format error saving part (ID: {operationId}): {fEx.Message}');
                        }}
                    </script>
                ", "text/html");
            }
            catch (ArgumentException aEx)
            {
                _logger.LogError(aEx, "?? [PARTS-{OperationId}] Argument error saving part: {ErrorMessage}", 
                    operationId, aEx.Message);
                
                return Content($@"
                    <script>
                        console.error('Argument error saving part (ID: {operationId}): {aEx.Message}');
                        if (typeof showErrorNotification === 'function') {{
                            showErrorNotification('Argument error saving part (ID: {operationId}): {aEx.Message}', 8000);
                        }} else {{
                            alert('Argument error saving part (ID: {operationId}): {aEx.Message}');
                        }}
                    </script>
                ", "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PARTS-{OperationId}] Unexpected error saving part: {ErrorMessage}", 
                    operationId, ex.Message);
                
                return Content($@"
                    <script>
                        console.error('Unexpected error saving part (ID: {operationId}): {ex.Message}');
                        if (typeof showErrorNotification === 'function') {{
                            showErrorNotification('Unexpected error saving part (ID: {operationId}): {ex.Message}', 8000);
                        }} else {{
                            alert('Unexpected error saving part (ID: {operationId}): {ex.Message}');
                        }}
                    </script>
                ", "text/html");
            }
        }

        // CRITICAL FIX: Helper methods for safe data sanitization
        private double SanitizeDouble(double value, string fieldName, string operationId)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                _logger.LogWarning("?? [PARTS-{OperationId}] Invalid double value for {FieldName}: {Value}, setting to 0", 
                    operationId, fieldName, value);
                return 0;
            }
            
            if (value < 0 || value > 1000000) // Reasonable bounds
            {
                _logger.LogWarning("?? [PARTS-{OperationId}] Out of range double value for {FieldName}: {Value}, setting to 0", 
                    operationId, fieldName, value);
                return 0;
            }
            
            return Math.Round(value, 3); // Round to prevent precision issues
        }

        private decimal SanitizeDecimal(decimal value, string fieldName, string operationId)
        {
            if (value < 0 || value > 1000000) // Reasonable bounds
            {
                _logger.LogWarning("?? [PARTS-{OperationId}] Out of range decimal value for {FieldName}: {Value}, setting to 0", 
                    operationId, fieldName, value);
                return 0;
            }
            
            return Math.Round(value, 2); // Round to prevent precision issues
        }
    }
}
