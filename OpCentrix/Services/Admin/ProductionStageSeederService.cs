using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin
{
    /// <summary>
    /// Service for seeding default production stages
    /// </summary>
    public class ProductionStageSeederService : IProductionStageSeederService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<ProductionStageSeederService> _logger;

        public ProductionStageSeederService(SchedulerContext context, ILogger<ProductionStageSeederService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> SeedDefaultStagesAsync()
        {
            try
            {
                var existingCount = await _context.ProductionStages.CountAsync(ps => ps.IsActive);
                if (existingCount > 0)
                {
                    _logger.LogInformation("Production stages already exist ({Count}), skipping seeding", existingCount);
                    return existingCount;
                }

                var defaultStages = CreateDefaultStages();
                
                await _context.ProductionStages.AddRangeAsync(defaultStages);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully seeded {Count} default production stages", defaultStages.Count);
                return defaultStages.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding default production stages");
                return 0;
            }
        }

        public async Task<List<ProductionStage>> GetAvailableStagesAsync()
        {
            try
            {
                return await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .OrderBy(ps => ps.DisplayOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available production stages");
                return new List<ProductionStage>();
            }
        }

        public async Task<bool> DefaultStagesExistAsync()
        {
            try
            {
                return await _context.ProductionStages.AnyAsync(ps => ps.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if default stages exist");
                return false;
            }
        }

        private List<ProductionStage> CreateDefaultStages()
        {
            return new List<ProductionStage>
            {
                new ProductionStage
                {
                    Name = "SLS Printing",
                    DisplayOrder = 1,
                    Description = "Selective Laser Sintering metal printing process",
                    DefaultSetupMinutes = 45,
                    DefaultHourlyRate = 85.00m,
                    DefaultDurationHours = 8.0,
                    DefaultMaterialCost = 25.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = false,
                    IsOptional = false,
                    RequiredRole = "Operator",
                    Department = "3D Printing",
                    StageColor = "#007bff",
                    StageIcon = "fas fa-print",
                    RequiresMachineAssignment = true,
                    AllowParallelExecution = false,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = "System",
                    IsActive = true
                },
                new ProductionStage
                {
                    Name = "CNC Machining",
                    DisplayOrder = 2,
                    Description = "Computer Numerical Control precision machining",
                    DefaultSetupMinutes = 30,
                    DefaultHourlyRate = 95.00m,
                    DefaultDurationHours = 4.0,
                    DefaultMaterialCost = 10.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = true,
                    IsOptional = true,
                    RequiredRole = "Machinist",
                    Department = "CNC Machining",
                    StageColor = "#28a745",
                    StageIcon = "fas fa-cogs",
                    RequiresMachineAssignment = true,
                    AllowParallelExecution = false,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = "System",
                    IsActive = true
                },
                new ProductionStage
                {
                    Name = "EDM Operations",
                    DisplayOrder = 3,
                    Description = "Electrical Discharge Machining for complex geometries",
                    DefaultSetupMinutes = 60,
                    DefaultHourlyRate = 120.00m,
                    DefaultDurationHours = 6.0,
                    DefaultMaterialCost = 15.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = true,
                    AllowSkip = true,
                    IsOptional = true,
                    RequiredRole = "EDM Specialist",
                    Department = "EDM",
                    StageColor = "#ffc107",
                    StageIcon = "fas fa-bolt",
                    RequiresMachineAssignment = true,
                    AllowParallelExecution = false,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = "System",
                    IsActive = true
                },
                new ProductionStage
                {
                    Name = "Assembly",
                    DisplayOrder = 4,
                    Description = "Multi-component assembly operations",
                    DefaultSetupMinutes = 15,
                    DefaultHourlyRate = 75.00m,
                    DefaultDurationHours = 2.0,
                    DefaultMaterialCost = 5.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = false,
                    IsOptional = true,
                    RequiredRole = "Assembler",
                    Department = "Assembly",
                    StageColor = "#17a2b8",
                    StageIcon = "fas fa-puzzle-piece",
                    RequiresMachineAssignment = false,
                    AllowParallelExecution = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = "System",
                    IsActive = true
                },
                new ProductionStage
                {
                    Name = "Finishing",
                    DisplayOrder = 5,
                    Description = "Surface finishing and coating operations",
                    DefaultSetupMinutes = 20,
                    DefaultHourlyRate = 70.00m,
                    DefaultDurationHours = 3.0,
                    DefaultMaterialCost = 8.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = true,
                    IsOptional = true,
                    RequiredRole = "Finisher",
                    Department = "Finishing",
                    StageColor = "#6c757d",
                    StageIcon = "fas fa-brush",
                    RequiresMachineAssignment = false,
                    AllowParallelExecution = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = "System",
                    IsActive = true
                },
                new ProductionStage
                {
                    Name = "Quality Inspection",
                    DisplayOrder = 6,
                    Description = "Final quality control and inspection",
                    DefaultSetupMinutes = 10,
                    DefaultHourlyRate = 80.00m,
                    DefaultDurationHours = 1.0,
                    DefaultMaterialCost = 2.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = true,
                    AllowSkip = false,
                    IsOptional = false,
                    RequiredRole = "Quality Inspector",
                    Department = "Quality Control",
                    StageColor = "#dc3545",
                    StageIcon = "fas fa-search",
                    RequiresMachineAssignment = false,
                    AllowParallelExecution = false,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = "System",
                    IsActive = true
                }
            };
        }
    }
}