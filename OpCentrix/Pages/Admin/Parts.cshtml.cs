using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.Admin
{
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
        public bool? ActiveOnly { get; set; } = true;
        
        [BindProperty(SupportsGet = true)]
        public string? SortOrder { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 25;

        // Properties for the part form
        [BindProperty]
        public Part Part { get; set; } = new Part();
        
        [BindProperty]
        public string? Action { get; set; }

        // Lists for dropdowns
        public List<string> AvailableMaterials { get; set; } = new List<string>();
        public List<string> AvailableIndustries { get; set; } = new List<string>();
        public List<string> AvailableCategories { get; set; } = new List<string>();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadPartsAsync();
                await LoadDropdownDataAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading parts page");
                TempData["ErrorMessage"] = "Error loading parts. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadPartsAsync();
                    await LoadDropdownDataAsync();
                    TempData["ErrorMessage"] = "Please correct the validation errors.";
                    return Page();
                }

                // Set audit fields
                Part.CreatedBy = User.Identity?.Name ?? "System";
                Part.LastModifiedBy = User.Identity?.Name ?? "System";
                Part.CreatedDate = DateTime.UtcNow;
                Part.LastModifiedDate = DateTime.UtcNow;

                // Validate part number uniqueness
                if (await _context.Parts.AnyAsync(p => p.PartNumber == Part.PartNumber))
                {
                    ModelState.AddModelError("Part.PartNumber", "Part number already exists.");
                    await LoadPartsAsync();
                    await LoadDropdownDataAsync();
                    return Page();
                }

                _context.Parts.Add(Part);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Part {PartNumber} created by {User}", Part.PartNumber, User.Identity?.Name);
                TempData["SuccessMessage"] = $"Part '{Part.PartNumber}' created successfully.";
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating part {PartNumber}", Part.PartNumber);
                TempData["ErrorMessage"] = "Error creating part. Please try again.";
                await LoadPartsAsync();
                await LoadDropdownDataAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEditAsync(int id)
        {
            try
            {
                var existingPart = await _context.Parts.FindAsync(id);
                if (existingPart == null)
                {
                    TempData["ErrorMessage"] = "Part not found.";
                    return RedirectToPage();
                }

                // Update properties manually to avoid overriding navigation properties
                existingPart.PartNumber = Part.PartNumber;
                existingPart.Name = Part.Name;
                existingPart.Description = Part.Description;
                existingPart.Material = Part.Material;
                existingPart.SlsMaterial = Part.SlsMaterial;
                existingPart.Industry = Part.Industry;
                existingPart.Application = Part.Application;
                existingPart.PartCategory = Part.PartCategory;
                existingPart.PartClass = Part.PartClass;
                existingPart.CustomerPartNumber = Part.CustomerPartNumber;
                existingPart.IsActive = Part.IsActive;
                
                // Physical properties
                existingPart.WeightGrams = Part.WeightGrams;
                existingPart.LengthMm = Part.LengthMm;
                existingPart.WidthMm = Part.WidthMm;
                existingPart.HeightMm = Part.HeightMm;
                existingPart.VolumeMm3 = Part.VolumeMm3;
                existingPart.Dimensions = Part.Dimensions;
                
                // Process parameters
                existingPart.RecommendedLaserPower = Part.RecommendedLaserPower;
                existingPart.RecommendedScanSpeed = Part.RecommendedScanSpeed;
                existingPart.RecommendedLayerThickness = Part.RecommendedLayerThickness;
                existingPart.RecommendedHatchSpacing = Part.RecommendedHatchSpacing;
                existingPart.RecommendedBuildTemperature = Part.RecommendedBuildTemperature;
                existingPart.RequiredArgonPurity = Part.RequiredArgonPurity;
                existingPart.MaxOxygenContent = Part.MaxOxygenContent;
                
                // Material and powder
                existingPart.PowderSpecification = Part.PowderSpecification;
                existingPart.PowderRequirementKg = Part.PowderRequirementKg;
                
                // Surface and quality
                existingPart.SurfaceFinishRequirement = Part.SurfaceFinishRequirement;
                existingPart.MaxSurfaceRoughnessRa = Part.MaxSurfaceRoughnessRa;
                existingPart.RequiresInspection = Part.RequiresInspection;
                existingPart.RequiresCertification = Part.RequiresCertification;
                existingPart.RequiresFDA = Part.RequiresFDA;
                existingPart.RequiresAS9100 = Part.RequiresAS9100;
                existingPart.RequiresNADCAP = Part.RequiresNADCAP;
                
                // Support requirements
                existingPart.RequiresSupports = Part.RequiresSupports;
                existingPart.SupportStrategy = Part.SupportStrategy;
                existingPart.SupportRemovalTimeMinutes = Part.SupportRemovalTimeMinutes;
                
                // Timing
                existingPart.EstimatedHours = Part.EstimatedHours;
                existingPart.SetupTimeMinutes = Part.SetupTimeMinutes;
                existingPart.PowderChangeoverTimeMinutes = Part.PowderChangeoverTimeMinutes;
                existingPart.PreheatingTimeMinutes = Part.PreheatingTimeMinutes;
                existingPart.CoolingTimeMinutes = Part.CoolingTimeMinutes;
                existingPart.PostProcessingTimeMinutes = Part.PostProcessingTimeMinutes;
                
                // Admin override fields
                existingPart.AdminEstimatedHoursOverride = Part.AdminEstimatedHoursOverride;
                existingPart.AdminOverrideReason = Part.AdminOverrideReason;
                if (Part.AdminEstimatedHoursOverride.HasValue)
                {
                    existingPart.AdminOverrideBy = User.Identity?.Name ?? "System";
                    existingPart.AdminOverrideDate = DateTime.UtcNow;
                }
                
                // Cost data
                existingPart.MaterialCostPerKg = Part.MaterialCostPerKg;
                existingPart.StandardLaborCostPerHour = Part.StandardLaborCostPerHour;
                existingPart.SetupCost = Part.SetupCost;
                existingPart.PostProcessingCost = Part.PostProcessingCost;
                existingPart.QualityInspectionCost = Part.QualityInspectionCost;
                existingPart.MachineOperatingCostPerHour = Part.MachineOperatingCostPerHour;
                existingPart.ArgonCostPerHour = Part.ArgonCostPerHour;
                existingPart.StandardSellingPrice = Part.StandardSellingPrice;
                
                // Requirements and certifications
                existingPart.ProcessType = Part.ProcessType;
                existingPart.RequiredMachineType = Part.RequiredMachineType;
                existingPart.PreferredMachines = Part.PreferredMachines;
                existingPart.QualityStandards = Part.QualityStandards;
                existingPart.ToleranceRequirements = Part.ToleranceRequirements;
                existingPart.RequiredSkills = Part.RequiredSkills;
                existingPart.RequiredCertifications = Part.RequiredCertifications;
                existingPart.RequiredTooling = Part.RequiredTooling;
                existingPart.ConsumableMaterials = Part.ConsumableMaterials;
                
                // File information
                existingPart.BuildFileTemplate = Part.BuildFileTemplate;
                existingPart.CadFilePath = Part.CadFilePath;
                existingPart.CadFileVersion = Part.CadFileVersion;
                
                // Set audit fields
                existingPart.LastModifiedBy = User.Identity?.Name ?? "System";
                existingPart.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Part {PartNumber} updated by {User}", Part.PartNumber, User.Identity?.Name);
                TempData["SuccessMessage"] = $"Part '{Part.PartNumber}' updated successfully.";
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part {PartNumber}", Part.PartNumber);
                TempData["ErrorMessage"] = "Error updating part. Please try again.";
                await LoadPartsAsync();
                await LoadDropdownDataAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var part = await _context.Parts
                    .Include(p => p.Jobs)
                    .FirstOrDefaultAsync(p => p.Id == id);
                    
                if (part == null)
                {
                    TempData["ErrorMessage"] = "Part not found.";
                    return RedirectToPage();
                }

                // Check if part has associated jobs
                if (part.Jobs.Any())
                {
                    TempData["ErrorMessage"] = $"Cannot delete part '{part.PartNumber}' because it has associated jobs. Set it to inactive instead.";
                    return RedirectToPage();
                }

                var partNumber = part.PartNumber;
                _context.Parts.Remove(part);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Part {PartNumber} deleted by {User}", partNumber, User.Identity?.Name);
                TempData["SuccessMessage"] = $"Part '{partNumber}' deleted successfully.";
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting part with ID {PartId}", id);
                TempData["ErrorMessage"] = "Error deleting part. Please try again.";
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

                return new JsonResult(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part data for ID {PartId}", id);
                return BadRequest("Error retrieving part data.");
            }
        }

        public async Task<IActionResult> OnGetAddFormAsync()
        {
            try
            {
                // Initialize a new part with default values
                Part = new Part
                {
                    Material = "Ti-6Al-4V Grade 5",
                    SlsMaterial = "Ti-6Al-4V Grade 5",
                    Industry = "General",
                    Application = "General Component",
                    PartCategory = "Prototype",
                    PartClass = "B",
                    ProcessType = "SLS Metal",
                    RequiredMachineType = "TruPrint 3000",
                    IsActive = true,
                    EstimatedHours = 8.0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                return Partial("Shared/_PartFormModal", Part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading add part form");
                return BadRequest("Error loading form.");
            }
        }

        public async Task<IActionResult> OnGetEditFormAsync(int id)
        {
            try
            {
                var part = await _context.Parts.FindAsync(id);
                if (part == null)
                {
                    return NotFound();
                }

                Part = part;
                return Partial("Shared/_PartFormModal", Part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit part form for ID {PartId}", id);
                return BadRequest("Error loading part data.");
            }
        }

        private async Task LoadPartsAsync()
        {
            try
            {
                var query = _context.Parts.AsQueryable();

                // Apply filters
                if (ActiveOnly.HasValue && ActiveOnly.Value)
                {
                    query = query.Where(p => p.IsActive);
                }

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    query = query.Where(p => 
                        p.PartNumber.Contains(SearchTerm) ||
                        p.Name.Contains(SearchTerm) ||
                        (p.Description != null && p.Description.Contains(SearchTerm)) ||
                        (p.CustomerPartNumber != null && p.CustomerPartNumber.Contains(SearchTerm)));
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
                    _ => query.OrderBy(p => p.PartNumber)
                };

                // Calculate pagination
                var totalCount = await query.CountAsync();
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

                // Apply pagination - using ToListAsync directly to let EF handle the NULL values
                Parts = await query
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                // Post-process to ensure no NULL values in critical fields
                foreach (var part in Parts)
                {
                    part.Description ??= string.Empty;
                    part.CustomerPartNumber ??= string.Empty;
                    part.Industry ??= "General";
                    part.Application ??= "General Component";
                    part.PartClass ??= "B";
                    part.CreatedBy ??= "System";
                    part.LastModifiedBy ??= "System";
                    part.AdminOverrideReason ??= string.Empty;
                    part.AdminOverrideBy ??= string.Empty;
                    part.AvgDuration ??= "8h 0m";
                    part.Dimensions ??= string.Empty;
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("The data is NULL at ordinal"))
            {
                // Handle the specific NULL value error by fixing the database and retrying
                _logger.LogWarning("Detected NULL values in database causing data reader error. Attempting to fix...");
                
                await FixNullValuesInDatabaseAsync();
                
                // Retry the query after fixing NULL values
                var query = _context.Parts.AsQueryable();

                // Reapply all filters and sorting (simplified retry)
                if (ActiveOnly.HasValue && ActiveOnly.Value)
                {
                    query = query.Where(p => p.IsActive);
                }

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    query = query.Where(p => 
                        p.PartNumber.Contains(SearchTerm) ||
                        p.Name.Contains(SearchTerm));
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
                    _ => query.OrderBy(p => p.PartNumber)
                };

                var totalCount = await query.CountAsync();
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

                Parts = await query
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                _logger.LogInformation("Successfully loaded parts after fixing NULL values");
            }
        }

        private async Task LoadDropdownDataAsync()
        {
            AvailableMaterials = await _context.Parts
                .Where(p => !string.IsNullOrEmpty(p.Material))
                .Select(p => p.Material)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            AvailableIndustries = await _context.Parts
                .Where(p => !string.IsNullOrEmpty(p.Industry))
                .Select(p => p.Industry ?? "General")
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

        /// <summary>
        /// Fixes NULL values in the database that may cause InvalidOperationException
        /// during data loading. Called automatically when NULL value errors are detected.
        /// </summary>
        private async Task FixNullValuesInDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Fixing NULL values in Parts table to prevent data reader errors");
                
                var sql = """
                    UPDATE Parts 
                    SET 
                        AdminOverrideReason = COALESCE(AdminOverrideReason, ''),
                        AdminOverrideBy = COALESCE(AdminOverrideBy, ''),
                        Description = COALESCE(Description, ''),
                        CustomerPartNumber = COALESCE(CustomerPartNumber, ''),
                        Industry = COALESCE(Industry, 'General'),
                        Application = COALESCE(Application, 'General Component'),
                        PartClass = COALESCE(PartClass, 'B'),
                        CreatedBy = COALESCE(CreatedBy, 'System'),
                        LastModifiedBy = COALESCE(LastModifiedBy, 'System'),
                        AvgDuration = COALESCE(AvgDuration, '8h 0m'),
                        Dimensions = COALESCE(Dimensions, '')
                    WHERE 
                        AdminOverrideReason IS NULL 
                        OR AdminOverrideBy IS NULL
                        OR Description IS NULL
                        OR CustomerPartNumber IS NULL
                        OR Industry IS NULL
                        OR Application IS NULL
                        OR PartClass IS NULL
                        OR CreatedBy IS NULL
                        OR LastModifiedBy IS NULL
                        OR AvgDuration IS NULL
                        OR Dimensions IS NULL
                    """;
                
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql);
                _logger.LogInformation("Fixed {RowsAffected} rows with NULL values in Parts table", rowsAffected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fix NULL values in database");
            }
        }
    }
}