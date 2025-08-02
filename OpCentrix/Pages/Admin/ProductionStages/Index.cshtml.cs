using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Pages.Admin.ProductionStages
{
    /// <summary>
    /// Production stage configuration and management page
    /// Manages the ProductionStages table which defines available manufacturing stages
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class IndexModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(SchedulerContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<ProductionStage> ProductionStages { get; set; } = new List<ProductionStage>();
        public Dictionary<int, StageUsageStats> StageUsageStatistics { get; set; } = new Dictionary<int, StageUsageStats>();

        [BindProperty]
        public ProductionStage NewStage { get; set; } = new ProductionStage();

        [BindProperty]
        public string ReorderStageIds { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading production stages and usage statistics");
                
                // Load all active production stages
                ProductionStages = await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .OrderBy(ps => ps.DisplayOrder)
                    .ToListAsync();

                // Load usage statistics for each stage
                await LoadStageUsageStatisticsAsync();

                _logger.LogInformation("Loaded {StageCount} production stages", ProductionStages.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading production stages");
                TempData["ErrorMessage"] = "Error loading production stages. Please try again.";
                ProductionStages = new List<ProductionStage>();
            }
        }

        public async Task<IActionResult> OnPostCreateStageAsync()
        {
            try
            {
                _logger.LogInformation("Creating new production stage: {StageName}", NewStage?.Name ?? "null");
                
                if (NewStage == null)
                {
                    TempData["ErrorMessage"] = "Invalid stage data received.";
                    await OnGetAsync();
                    return Page();
                }

                // Validate model state
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model validation failed for new stage");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogDebug("Validation error: {Error}", error.ErrorMessage);
                    }
                    TempData["ErrorMessage"] = "Please correct the validation errors and try again.";
                    await OnGetAsync();
                    return Page();
                }

                // Check for duplicate stage names
                var existingStage = await _context.ProductionStages
                    .FirstOrDefaultAsync(ps => ps.Name.ToLower() == NewStage.Name.ToLower() && ps.IsActive);
                
                if (existingStage != null)
                {
                    TempData["ErrorMessage"] = $"A stage with the name '{NewStage.Name}' already exists.";
                    await OnGetAsync();
                    return Page();
                }

                // Set display order to the next available position
                var maxOrder = await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .MaxAsync(ps => (int?)ps.DisplayOrder) ?? 0;

                NewStage.DisplayOrder = maxOrder + 1;
                NewStage.CreatedDate = DateTime.UtcNow;
                NewStage.IsActive = true;

                _context.ProductionStages.Add(NewStage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created production stage: {StageName} with ID: {StageId}", 
                    NewStage.Name, NewStage.Id);
                TempData["SuccessMessage"] = $"Production stage '{NewStage.Name}' created successfully.";
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating production stage: {StageName}", NewStage?.Name ?? "null");
                TempData["ErrorMessage"] = "Error creating production stage. Please try again.";
                await OnGetAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteStageAsync(int stageId)
        {
            try
            {
                _logger.LogInformation("Attempting to delete production stage with ID: {StageId}", stageId);
                
                var stage = await _context.ProductionStages.FindAsync(stageId);
                if (stage == null)
                {
                    TempData["ErrorMessage"] = "Stage not found.";
                    return RedirectToPage();
                }

                // Check if stage is being used in any part stage requirements
                var partStageUsage = await _context.PartStageRequirements
                    .CountAsync(psr => psr.ProductionStageId == stageId && psr.IsActive);

                // Check if stage is being used in any prototype executions
                var prototypeUsage = await _context.ProductionStageExecutions
                    .CountAsync(pse => pse.ProductionStageId == stageId);

                if (partStageUsage > 0 || prototypeUsage > 0)
                {
                    _logger.LogWarning("Cannot delete stage {StageId} - it's used by {PartUsage} parts and {PrototypeUsage} prototypes", 
                        stageId, partStageUsage, prototypeUsage);
                    TempData["ErrorMessage"] = $"Cannot delete '{stage.Name}' - it's currently used by {partStageUsage + prototypeUsage} items. Please remove those references first.";
                    return RedirectToPage();
                }

                // Soft delete
                stage.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted production stage: {StageName}", stage.Name);
                TempData["SuccessMessage"] = $"Production stage '{stage.Name}' deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting production stage {StageId}", stageId);
                TempData["ErrorMessage"] = "Error deleting production stage. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReorderStagesAsync()
        {
            try
            {
                _logger.LogInformation("Reordering stages with input: {ReorderStageIds}", ReorderStageIds);
                
                if (string.IsNullOrEmpty(ReorderStageIds))
                {
                    TempData["ErrorMessage"] = "No stage order provided.";
                    return RedirectToPage();
                }

                var stageIds = ReorderStageIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.TryParse(id.Trim(), out var result) ? result : 0)
                    .Where(id => id > 0)
                    .ToList();

                if (!stageIds.Any())
                {
                    TempData["ErrorMessage"] = "No valid stage IDs provided for reordering.";
                    return RedirectToPage();
                }

                var stages = await _context.ProductionStages
                    .Where(ps => stageIds.Contains(ps.Id))
                    .ToListAsync();

                // Update display order
                for (int i = 0; i < stageIds.Count; i++)
                {
                    var stage = stages.FirstOrDefault(s => s.Id == stageIds[i]);
                    if (stage != null)
                    {
                        stage.DisplayOrder = i + 1;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully reordered {Count} production stages", stageIds.Count);
                TempData["SuccessMessage"] = $"Successfully reordered {stageIds.Count} production stages.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering production stages");
                TempData["ErrorMessage"] = "Error reordering production stages. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateDefaultStagesAsync()
        {
            try
            {
                _logger.LogInformation("Creating default production stages");
                
                // Check if stages already exist
                var existingStagesCount = await _context.ProductionStages.CountAsync(ps => ps.IsActive);
                if (existingStagesCount > 0)
                {
                    TempData["WarningMessage"] = "Default stages not created - existing stages found. Clear existing stages first if you want to recreate defaults.";
                    return RedirectToPage();
                }

                var defaultStages = GetDefaultStageTemplates();
                
                foreach (var stage in defaultStages)
                {
                    _context.ProductionStages.Add(stage);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created {Count} default production stages", defaultStages.Count);
                TempData["SuccessMessage"] = $"Successfully created {defaultStages.Count} default production stages.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default production stages");
                TempData["ErrorMessage"] = "Error creating default production stages. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetStageDetailsAsync(int stageId)
        {
            try
            {
                var stage = await _context.ProductionStages
                    .FirstOrDefaultAsync(ps => ps.Id == stageId);
                
                if (stage == null)
                {
                    return NotFound("Stage not found");
                }

                return new JsonResult(new
                {
                    id = stage.Id,
                    name = stage.Name,
                    description = stage.Description ?? "",
                    defaultSetupMinutes = stage.DefaultSetupMinutes,
                    defaultHourlyRate = stage.DefaultHourlyRate,
                    requiresQualityCheck = stage.RequiresQualityCheck,
                    requiresApproval = stage.RequiresApproval,
                    allowSkip = stage.AllowSkip,
                    isOptional = stage.IsOptional,
                    requiredRole = stage.RequiredRole ?? ""
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stage details for {StageId}", stageId);
                return BadRequest("Error loading stage details");
            }
        }

        public async Task<IActionResult> OnPostUpdateStageAsync()
        {
            try
            {
                var contentType = Request.ContentType?.ToLower();
                
                if (contentType?.Contains("application/json") == true)
                {
                    // Handle JSON request from AJAX
                    using var reader = new StreamReader(Request.Body);
                    var body = await reader.ReadToEndAsync();
                    var stageData = System.Text.Json.JsonSerializer.Deserialize<UpdateStageRequest>(body, 
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (stageData?.Id <= 0)
                    {
                        return BadRequest("Invalid stage ID");
                    }

                    var existingStage = await _context.ProductionStages.FindAsync(stageData.Id);
                    if (existingStage == null)
                    {
                        return NotFound("Stage not found");
                    }

                    // Update properties
                    existingStage.Name = stageData.Name ?? existingStage.Name;
                    existingStage.Description = stageData.Description;
                    existingStage.DefaultSetupMinutes = stageData.DefaultSetupMinutes;
                    existingStage.DefaultHourlyRate = stageData.DefaultHourlyRate;
                    existingStage.RequiresQualityCheck = stageData.RequiresQualityCheck;
                    existingStage.RequiresApproval = stageData.RequiresApproval;
                    existingStage.AllowSkip = stageData.AllowSkip;
                    existingStage.IsOptional = stageData.IsOptional;
                    existingStage.RequiredRole = stageData.RequiredRole;

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Successfully updated stage {StageId}: {StageName}", existingStage.Id, existingStage.Name);
                    return new JsonResult(new { success = true, message = "Stage updated successfully" });
                }
                else
                {
                    // Handle form request from modal
                    var stageId = int.Parse(Request.Form["Id"]!);
                    var existingStage = await _context.ProductionStages.FindAsync(stageId);
                    
                    if (existingStage == null)
                    {
                        TempData["ErrorMessage"] = "Stage not found.";
                        return RedirectToPage();
                    }

                    // Update from form data
                    existingStage.Name = Request.Form["Name"]!;
                    existingStage.Description = Request.Form["Description"];
                    existingStage.DefaultSetupMinutes = int.Parse(Request.Form["DefaultSetupMinutes"]!);
                    existingStage.DefaultHourlyRate = decimal.Parse(Request.Form["DefaultHourlyRate"]!);
                    existingStage.RequiresQualityCheck = Request.Form["RequiresQualityCheck"].Contains("true");
                    existingStage.RequiresApproval = Request.Form["RequiresApproval"].Contains("true");
                    existingStage.AllowSkip = Request.Form["AllowSkip"].Contains("true");
                    existingStage.IsOptional = Request.Form["IsOptional"].Contains("true");
                    existingStage.RequiredRole = Request.Form["RequiredRole"];

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Successfully updated stage {StageId}: {StageName}", existingStage.Id, existingStage.Name);
                    TempData["SuccessMessage"] = $"Production stage '{existingStage.Name}' updated successfully.";
                    
                    return RedirectToPage();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating production stage");
                
                var contentType = Request.ContentType?.ToLower();
                if (contentType?.Contains("application/json") == true)
                {
                    return BadRequest("Error updating stage");
                }
                else
                {
                    TempData["ErrorMessage"] = "Error updating production stage. Please try again.";
                    return RedirectToPage();
                }
            }
        }

        private async Task LoadStageUsageStatisticsAsync()
        {
            try
            {
                foreach (var stage in ProductionStages)
                {
                    var partUsageCount = await _context.PartStageRequirements
                        .CountAsync(psr => psr.ProductionStageId == stage.Id && psr.IsActive);

                    var prototypeUsageCount = await _context.ProductionStageExecutions
                        .CountAsync(pse => pse.ProductionStageId == stage.Id);

                    var completedExecutions = await _context.ProductionStageExecutions
                        .Where(pse => pse.ProductionStageId == stage.Id && pse.Status == "Completed")
                        .ToListAsync();

                    var avgHours = completedExecutions.Any() 
                        ? completedExecutions.Average(e => e.ActualHours ?? 0) 
                        : 0;

                    var avgCost = completedExecutions.Any() 
                        ? completedExecutions.Average(e => e.ActualCost ?? 0) 
                        : 0;

                    StageUsageStatistics[stage.Id] = new StageUsageStats
                    {
                        PartUsageCount = partUsageCount,
                        PrototypeUsageCount = prototypeUsageCount,
                        TotalUsageCount = partUsageCount + prototypeUsageCount,
                        CompletedExecutions = completedExecutions.Count,
                        AverageActualHours = (decimal)avgHours,
                        AverageActualCost = (decimal)avgCost
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stage usage statistics");
                // Continue without statistics rather than failing completely
            }
        }

        private List<ProductionStage> GetDefaultStageTemplates()
        {
            return new List<ProductionStage>
            {
                new ProductionStage
                {
                    Name = "3D Printing (SLS)",
                    DisplayOrder = 1,
                    Description = "Selective Laser Sintering of metal powder",
                    DefaultSetupMinutes = 45,
                    DefaultHourlyRate = 85.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = false,
                    IsOptional = false,
                    RequiredRole = "Operator",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                },
                new ProductionStage
                {
                    Name = "CNC Machining",
                    DisplayOrder = 2,
                    Description = "Computer Numerical Control machining operations",
                    DefaultSetupMinutes = 30,
                    DefaultHourlyRate = 95.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = true,
                    IsOptional = true,
                    RequiredRole = "Machinist",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                },
                new ProductionStage
                {
                    Name = "EDM",
                    DisplayOrder = 3,
                    Description = "Electrical Discharge Machining for complex geometries",
                    DefaultSetupMinutes = 60,
                    DefaultHourlyRate = 120.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = true,
                    IsOptional = true,
                    RequiredRole = "EDM Specialist",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                },
                new ProductionStage
                {
                    Name = "Laser Engraving",
                    DisplayOrder = 4,
                    Description = "Laser engraving of serial numbers and markings",
                    DefaultSetupMinutes = 15,
                    DefaultHourlyRate = 75.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = false,
                    IsOptional = false,
                    RequiredRole = "Operator",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                },
                new ProductionStage
                {
                    Name = "Sandblasting",
                    DisplayOrder = 5,
                    Description = "Surface preparation and finish uniformity",
                    DefaultSetupMinutes = 20,
                    DefaultHourlyRate = 65.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = true,
                    IsOptional = true,
                    RequiredRole = "Finisher",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                },
                new ProductionStage
                {
                    Name = "Coating/Cerakote",
                    DisplayOrder = 6,
                    Description = "Surface treatment and corrosion protection",
                    DefaultSetupMinutes = 45,
                    DefaultHourlyRate = 70.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = true,
                    IsOptional = true,
                    RequiredRole = "Coater",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                },
                new ProductionStage
                {
                    Name = "Assembly",
                    DisplayOrder = 7,
                    Description = "Final assembly with end caps, springs, and hardware",
                    DefaultSetupMinutes = 30,
                    DefaultHourlyRate = 80.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = true,
                    AllowSkip = false,
                    IsOptional = false,
                    RequiredRole = "Assembler",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                },
                new ProductionStage
                {
                    Name = "Quality Inspection",
                    DisplayOrder = 8,
                    Description = "Final quality control and testing",
                    DefaultSetupMinutes = 10,
                    DefaultHourlyRate = 80.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = true,
                    AllowSkip = false,
                    IsOptional = false,
                    RequiredRole = "Quality Inspector",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                }
            };
        }
    }

    // Helper classes for the page model
    public class StageUsageStats
    {
        public int PartUsageCount { get; set; }
        public int PrototypeUsageCount { get; set; }
        public int TotalUsageCount { get; set; }
        public int CompletedExecutions { get; set; }
        public decimal AverageActualHours { get; set; }
        public decimal AverageActualCost { get; set; }
    }

    public class UpdateStageRequest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int DefaultSetupMinutes { get; set; }
        public decimal DefaultHourlyRate { get; set; }
        public bool RequiresQualityCheck { get; set; }
        public bool RequiresApproval { get; set; }
        public bool AllowSkip { get; set; }
        public bool IsOptional { get; set; }
        public string? RequiredRole { get; set; }
    }
}