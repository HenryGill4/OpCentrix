using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services
{
    /// <summary>
    /// Service for seeding default production stages data
    /// </summary>
    public interface IProductionStageSeederService
    {
        Task<int> SeedDefaultStagesAsync();
        Task<List<ProductionStage>> GetAllStagesAsync();
        Task<bool> HasDefaultStagesAsync();
    }

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
                // Check if stages already exist
                var existingCount = await _context.ProductionStages.CountAsync(ps => ps.IsActive);
                if (existingCount > 0)
                {
                    _logger.LogInformation("Default stages already exist ({Count} stages found)", existingCount);
                    return existingCount;
                }

                // Create default production stages
                var defaultStages = new List<ProductionStage>
                {
                    new ProductionStage
                    {
                        Name = "SLS Printing",
                        DisplayOrder = 1,
                        Description = "Selective Laser Sintering metal printing process",
                        DefaultSetupMinutes = 45,
                        DefaultHourlyRate = 85.00m,
                        RequiresQualityCheck = true,
                        RequiresApproval = false,
                        AllowSkip = false,
                        IsOptional = false,
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
                        AllowSkip = false,
                        IsOptional = true,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    },
                    new ProductionStage
                    {
                        Name = "EDM Operations",
                        DisplayOrder = 3,
                        Description = "Electrical Discharge Machining for complex geometries",
                        DefaultSetupMinutes = 60,
                        DefaultHourlyRate = 110.00m,
                        RequiresQualityCheck = true,
                        RequiresApproval = true,
                        AllowSkip = false,
                        IsOptional = true,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    },
                    new ProductionStage
                    {
                        Name = "Assembly",
                        DisplayOrder = 4,
                        Description = "Assembly of multiple components",
                        DefaultSetupMinutes = 15,
                        DefaultHourlyRate = 75.00m,
                        RequiresQualityCheck = true,
                        RequiresApproval = false,
                        AllowSkip = false,
                        IsOptional = true,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    },
                    new ProductionStage
                    {
                        Name = "Finishing",
                        DisplayOrder = 5,
                        Description = "Surface finishing, coating, and final processing",
                        DefaultSetupMinutes = 20,
                        DefaultHourlyRate = 70.00m,
                        RequiresQualityCheck = true,
                        RequiresApproval = false,
                        AllowSkip = false,
                        IsOptional = true,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    },
                    new ProductionStage
                    {
                        Name = "Quality Inspection",
                        DisplayOrder = 6,
                        Description = "Final quality control and inspection",
                        DefaultSetupMinutes = 10,
                        DefaultHourlyRate = 80.00m,
                        RequiresQualityCheck = true,
                        RequiresApproval = true,
                        AllowSkip = false,
                        IsOptional = false,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    }
                };

                // Add stages to context
                await _context.ProductionStages.AddRangeAsync(defaultStages);
                
                // Save changes
                var savedCount = await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully seeded {Count} default production stages", savedCount);
                
                return savedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding default production stages");
                throw;
            }
        }

        public async Task<List<ProductionStage>> GetAllStagesAsync()
        {
            return await _context.ProductionStages
                .Where(ps => ps.IsActive)
                .OrderBy(ps => ps.DisplayOrder)
                .ToListAsync();
        }

        public async Task<bool> HasDefaultStagesAsync()
        {
            var count = await _context.ProductionStages.CountAsync(ps => ps.IsActive);
            return count > 0;
        }
    }
}