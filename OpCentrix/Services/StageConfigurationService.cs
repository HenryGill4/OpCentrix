using OpCentrix.Data;
using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Services
{
    /// <summary>
    /// Interface for managing part manufacturing stages
    /// </summary>
    public interface IStageConfigurationService
    {
        Task<List<StageIndicator>> GetPartStageIndicatorsAsync(int partId);
        Task<List<StageIndicator>> GetPartStageIndicatorsAsync(Part part);
        Task<decimal> CalculateTotalEstimatedTimeAsync(int partId);
        Task<string> GetComplexityLevelAsync(int partId);
        Task<Dictionary<string, int>> GetStageUsageStatisticsAsync();
        Task<List<Part>> GetPartsByStageAsync(string stageType);
        Task<List<Part>> GetPartsByComplexityAsync(string complexityLevel);
    }

    /// <summary>
    /// Service for managing part manufacturing stages
    /// </summary>
    public class StageConfigurationService : IStageConfigurationService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<StageConfigurationService> _logger;

        public StageConfigurationService(SchedulerContext context, ILogger<StageConfigurationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get stage indicators for a part by ID
        /// </summary>
        public async Task<List<StageIndicator>> GetPartStageIndicatorsAsync(int partId)
        {
            try
            {
                var part = await _context.Parts.FindAsync(partId);
                return part != null ? await GetPartStageIndicatorsAsync(part) : new List<StageIndicator>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stage indicators for part ID {PartId}", partId);
                return new List<StageIndicator>();
            }
        }

        /// <summary>
        /// Get stage indicators for a part object
        /// </summary>
        public async Task<List<StageIndicator>> GetPartStageIndicatorsAsync(Part part)
        {
            try
            {
                var indicators = new List<StageIndicator>();

                // Generate stage indicators based on part requirements
                if (part.RequiresSLSPrinting)
                {
                    indicators.Add(new StageIndicator
                    {
                        Name = "SLS",
                        Class = "bg-primary",
                        Icon = "fas fa-print",
                        Title = "SLS Printing",
                        IsRequired = true,
                        Order = 1,
                        EstimatedDurationMinutes = (int)(part.EstimatedHours * 60),
                        Department = "SLS Printing",
                        Notes = $"Material: {part.SlsMaterial}"
                    });
                }

                if (part.RequiresCNCMachining)
                {
                    indicators.Add(new StageIndicator
                    {
                        Name = "CNC",
                        Class = "bg-info",
                        Icon = "fas fa-cog",
                        Title = "CNC Machining",
                        IsRequired = true,
                        Order = 2,
                        EstimatedDurationMinutes = (int)(part.EstimatedHours * 60 * 0.3), // 30% of SLS time
                        Department = "CNC Machining",
                        Notes = "Secondary operations"
                    });
                }

                if (part.RequiresEDMOperations)
                {
                    indicators.Add(new StageIndicator
                    {
                        Name = "EDM",
                        Class = "bg-warning",
                        Icon = "fas fa-bolt",
                        Title = "EDM Operations",
                        IsRequired = true,
                        Order = 3,
                        EstimatedDurationMinutes = (int)(part.EstimatedHours * 60 * 0.5), // 50% of SLS time
                        Department = "EDM",
                        Notes = "Complex geometries"
                    });
                }

                if (part.RequiresAssembly)
                {
                    indicators.Add(new StageIndicator
                    {
                        Name = "ASM",
                        Class = "bg-success",
                        Icon = "fas fa-puzzle-piece",
                        Title = "Assembly",
                        IsRequired = true,
                        Order = 4,
                        EstimatedDurationMinutes = 120, // 2 hours
                        Department = "Assembly",
                        Notes = "Multi-component assembly"
                    });
                }

                if (part.RequiresFinishing)
                {
                    indicators.Add(new StageIndicator
                    {
                        Name = "FIN",
                        Class = "bg-secondary",
                        Icon = "fas fa-magic",
                        Title = "Finishing",
                        IsRequired = true,
                        Order = 5,
                        EstimatedDurationMinutes = 180, // 3 hours
                        Department = "Finishing",
                        Notes = part.SurfaceFinishRequirement
                    });
                }

                return await Task.FromResult(indicators.OrderBy(i => i.Order).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stage indicators for part {PartNumber}", part?.PartNumber);
                return new List<StageIndicator>();
            }
        }

        /// <summary>
        /// Calculate total estimated time for all stages of a part
        /// </summary>
        public async Task<decimal> CalculateTotalEstimatedTimeAsync(int partId)
        {
            try
            {
                var part = await _context.Parts.FindAsync(partId);
                if (part == null) return 0;

                return (decimal)part.CalculateTotalManufacturingTime();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total estimated time for part ID {PartId}", partId);
                return 0;
            }
        }

        /// <summary>
        /// Get complexity level for a part
        /// </summary>
        public async Task<string> GetComplexityLevelAsync(int partId)
        {
            try
            {
                var part = await _context.Parts.FindAsync(partId);
                return await Task.FromResult(part?.BTComplexityLevel ?? "Unknown");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complexity level for part ID {PartId}", partId);
                return "Unknown";
            }
        }

        /// <summary>
        /// Get usage statistics for all manufacturing stages
        /// </summary>
        public async Task<Dictionary<string, int>> GetStageUsageStatisticsAsync()
        {
            var stats = new Dictionary<string, int>();
            
            try
            {
                _logger.LogInformation("Calculating stage usage statistics");
                
                stats["SLS Printing"] = await _context.Parts.CountAsync(p => p.RequiresSLSPrinting);
                stats["EDM Operations"] = await _context.Parts.CountAsync(p => p.RequiresEDMOperations);
                stats["CNC Machining"] = await _context.Parts.CountAsync(p => p.RequiresCNCMachining);
                stats["Assembly"] = await _context.Parts.CountAsync(p => p.RequiresAssembly);
                stats["Finishing"] = await _context.Parts.CountAsync(p => p.RequiresFinishing);
                
                _logger.LogInformation("Stage usage statistics calculated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stage usage statistics");
            }
            
            return stats;
        }

        /// <summary>
        /// Get parts that require a specific manufacturing stage
        /// </summary>
        public async Task<List<Part>> GetPartsByStageAsync(string stageType)
        {
            try
            {
                _logger.LogInformation("Getting parts by stage: {StageType}", stageType);
                
                var query = _context.Parts.AsQueryable();
                
                query = stageType.ToLowerInvariant() switch
                {
                    "sls" or "sls printing" => query.Where(p => p.RequiresSLSPrinting),
                    "edm" or "edm operations" => query.Where(p => p.RequiresEDMOperations),
                    "cnc" or "cnc machining" => query.Where(p => p.RequiresCNCMachining),
                    "assembly" => query.Where(p => p.RequiresAssembly),
                    "finishing" => query.Where(p => p.RequiresFinishing),
                    _ => query.Where(p => false) // Return empty result for unknown stage types
                };
                
                var result = await query.OrderBy(p => p.PartNumber).ToListAsync();
                
                _logger.LogInformation("Found {Count} parts requiring {StageType}", result.Count, stageType);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts by stage {StageType}", stageType);
                return new List<Part>();
            }
        }

        /// <summary>
        /// Get parts by complexity level
        /// </summary>
        public async Task<List<Part>> GetPartsByComplexityAsync(string complexityLevel)
        {
            try
            {
                _logger.LogInformation("Getting parts by complexity: {ComplexityLevel}", complexityLevel);
                
                var allParts = await _context.Parts.ToListAsync();
                
                var filteredParts = allParts
                    .Where(p => p.BTComplexityLevel.Equals(complexityLevel, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.PartNumber)
                    .ToList();
                
                _logger.LogInformation("Found {Count} parts with {ComplexityLevel} complexity", filteredParts.Count, complexityLevel);
                
                return filteredParts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts by complexity {ComplexityLevel}", complexityLevel);
                return new List<Part>();
            }
        }
    }
}