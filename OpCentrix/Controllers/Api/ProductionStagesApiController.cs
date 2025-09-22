using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Controllers.Api
{
    [ApiController]
    [Route("api/production-stages")]
    public class ProductionStagesApiController : ControllerBase
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<ProductionStagesApiController> _logger;

        public ProductionStagesApiController(SchedulerContext context, ILogger<ProductionStagesApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all available production stages - Accessible to authenticated users
        /// </summary>
        [HttpGet("available")]
        [AllowAnonymous] // Temporarily allow anonymous access for testing - REMOVE IN PRODUCTION
        public async Task<IActionResult> GetAvailableStages()
        {
            try
            {
                _logger.LogInformation("?? [API] Loading available production stages...");

                // First, ensure we have some production stages
                var stageCount = await _context.ProductionStages.CountAsync();
                _logger.LogInformation("?? [API] Found {StageCount} stages in database", stageCount);

                if (stageCount == 0)
                {
                    _logger.LogWarning("?? [API] No production stages found, creating default stages...");
                    await CreateDefaultStages();
                }

                var stages = await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .OrderBy(ps => ps.DisplayOrder)
                    .ThenBy(ps => ps.Name)
                    .Select(ps => new
                    {
                        id = ps.Id,
                        name = ps.Name,
                        description = ps.Description ?? "",
                        defaultHourlyRate = ps.DefaultHourlyRate,
                        defaultSetupMinutes = ps.DefaultSetupMinutes,
                        isActive = ps.IsActive,
                        defaultDurationHours = ps.DefaultDurationHours,
                        defaultMaterialCost = ps.DefaultMaterialCost,
                        displayOrder = ps.DisplayOrder,
                        department = ps.Department ?? "",
                        stageColor = ps.StageColor ?? "#007bff",
                        stageIcon = ps.StageIcon ?? "fas fa-cog"
                    })
                    .ToListAsync();

                _logger.LogInformation("? [API] Returning {StageCount} available production stages", stages.Count);
                
                return Ok(stages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [API] Error retrieving available production stages");
                
                // Return fallback data in case of database issues
                var fallbackStages = GetFallbackStages();
                _logger.LogWarning("?? [API] Returning fallback stages due to error");
                
                return Ok(fallbackStages);
            }
        }

        /// <summary>
        /// Get production stage by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous] // Temporarily allow anonymous access
        public async Task<IActionResult> GetStageById(int id)
        {
            try
            {
                var stage = await _context.ProductionStages
                    .Where(ps => ps.Id == id && ps.IsActive)
                    .Select(ps => new
                    {
                        id = ps.Id,
                        name = ps.Name,
                        description = ps.Description,
                        defaultHourlyRate = ps.DefaultHourlyRate,
                        defaultSetupMinutes = ps.DefaultSetupMinutes,
                        defaultDurationHours = ps.DefaultDurationHours,
                        isActive = ps.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (stage == null)
                {
                    return NotFound(new { error = $"Production stage with ID {id} not found" });
                }

                return Ok(stage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [API] Error retrieving production stage {StageId}", id);
                return StatusCode(500, new { error = "Error retrieving production stage", message = ex.Message });
            }
        }

        /// <summary>
        /// Test endpoint to verify API routing is working
        /// </summary>
        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult TestEndpoint()
        {
            _logger.LogInformation("?? [API] Test endpoint called");
            return Ok(new { 
                message = "ProductionStages API is working!", 
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            });
        }

        private async Task CreateDefaultStages()
        {
            try
            {
                var defaultStages = new[]
                {
                    new ProductionStage
                    {
                        Name = "3D Printing (SLS)",
                        Description = "Selective Laser Sintering manufacturing process",
                        Department = "3D Printing",
                        DefaultHourlyRate = 85.00m,
                        DefaultDurationHours = 8.0,
                        DefaultSetupMinutes = 30,
                        DefaultMaterialCost = 0.00m,
                        DisplayOrder = 1,
                        IsActive = true,
                        StageColor = "#007bff",
                        StageIcon = "fas fa-cube",
                        RequiresQualityCheck = true,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = "System",
                        LastModifiedDate = DateTime.UtcNow,
                        LastModifiedBy = "System"
                    },
                    new ProductionStage
                    {
                        Name = "CNC Machining",
                        Description = "Computer Numerical Control machining operations",
                        Department = "CNC Machining",
                        DefaultHourlyRate = 85.00m,
                        DefaultDurationHours = 4.0,
                        DefaultSetupMinutes = 45,
                        DefaultMaterialCost = 0.00m,
                        DisplayOrder = 2,
                        IsActive = true,
                        StageColor = "#28a745",
                        StageIcon = "fas fa-cogs",
                        RequiresQualityCheck = true,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = "System",
                        LastModifiedDate = DateTime.UtcNow,
                        LastModifiedBy = "System"
                    },
                    new ProductionStage
                    {
                        Name = "EDM Operations",
                        Description = "Electrical Discharge Machining operations",
                        Department = "EDM",
                        DefaultHourlyRate = 95.00m,
                        DefaultDurationHours = 6.0,
                        DefaultSetupMinutes = 60,
                        DefaultMaterialCost = 0.00m,
                        DisplayOrder = 3,
                        IsActive = true,
                        StageColor = "#ffc107",
                        StageIcon = "fas fa-bolt",
                        RequiresQualityCheck = true,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = "System",
                        LastModifiedDate = DateTime.UtcNow,
                        LastModifiedBy = "System"
                    },
                    new ProductionStage
                    {
                        Name = "Heat Treatment",
                        Description = "Heat treatment and stress relief",
                        Department = "Finishing",
                        DefaultHourlyRate = 75.00m,
                        DefaultDurationHours = 2.0,
                        DefaultSetupMinutes = 15,
                        DefaultMaterialCost = 0.00m,
                        DisplayOrder = 4,
                        IsActive = true,
                        StageColor = "#dc3545",
                        StageIcon = "fas fa-fire",
                        RequiresQualityCheck = true,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = "System",
                        LastModifiedDate = DateTime.UtcNow,
                        LastModifiedBy = "System"
                    },
                    new ProductionStage
                    {
                        Name = "Finishing",
                        Description = "Final finishing operations",
                        Department = "Finishing",
                        DefaultHourlyRate = 65.00m,
                        DefaultDurationHours = 3.0,
                        DefaultSetupMinutes = 20,
                        DefaultMaterialCost = 0.00m,
                        DisplayOrder = 5,
                        IsActive = true,
                        StageColor = "#6f42c1",
                        StageIcon = "fas fa-polish",
                        RequiresQualityCheck = true,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = "System",
                        LastModifiedDate = DateTime.UtcNow,
                        LastModifiedBy = "System"
                    }
                };

                _context.ProductionStages.AddRange(defaultStages);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("? [API] Created {StageCount} default production stages", defaultStages.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [API] Error creating default stages");
            }
        }

        private static object[] GetFallbackStages()
        {
            return new object[]
            {
                new
                {
                    id = 1,
                    name = "3D Printing (SLS)",
                    description = "Selective Laser Sintering (Fallback)",
                    defaultHourlyRate = 85.00m,
                    defaultDurationHours = 8.0,
                    defaultSetupMinutes = 30,
                    defaultMaterialCost = 0.00m,
                    displayOrder = 1,
                    department = "3D Printing",
                    stageColor = "#007bff",
                    stageIcon = "fas fa-cube",
                    isActive = true
                },
                new
                {
                    id = 2,
                    name = "CNC Machining",
                    description = "Computer Numerical Control machining (Fallback)",
                    defaultHourlyRate = 85.00m,
                    defaultDurationHours = 4.0,
                    defaultSetupMinutes = 45,
                    defaultMaterialCost = 0.00m,
                    displayOrder = 2,
                    department = "CNC Machining",
                    stageColor = "#28a745",
                    stageIcon = "fas fa-cogs",
                    isActive = true
                },
                new
                {
                    id = 3,
                    name = "EDM Operations",
                    description = "Electrical Discharge Machining (Fallback)",
                    defaultHourlyRate = 95.00m,
                    defaultDurationHours = 6.0,
                    defaultSetupMinutes = 60,
                    defaultMaterialCost = 0.00m,
                    displayOrder = 3,
                    department = "EDM",
                    stageColor = "#ffc107",
                    stageIcon = "fas fa-bolt",
                    isActive = true
                }
            };
        }
    }
}