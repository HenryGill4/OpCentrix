using OpCentrix.Data;
using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace OpCentrix.Services
{
    /// <summary>
    /// Service for managing part manufacturing stages using the new normalized structure
    /// Replaces the boolean flags with flexible ProductionStage relationships
    /// </summary>
    public interface IPartStageService
    {
        Task<List<PartStageRequirement>> GetPartStagesAsync(int partId);
        Task<List<PartStageRequirement>> GetPartStagesWithDetailsAsync(int partId);
        Task<PartStageRequirement?> GetPartStageAsync(int partId, int stageId);
        Task<bool> AddPartStageAsync(PartStageRequirement partStage);
        Task<bool> UpdatePartStageAsync(PartStageRequirement partStage);
        Task<bool> RemovePartStageAsync(int partId, int stageId);
        Task<bool> RemoveAllPartStagesAsync(int partId);
        Task<decimal> CalculateTotalEstimatedCostAsync(int partId);
        Task<double> CalculateTotalEstimatedTimeAsync(int partId);
        Task<string> GetComplexityLevelAsync(int partId);
        Task<List<StageIndicator>> GetStageIndicatorsAsync(int partId);
        Task<Dictionary<string, int>> GetStageUsageStatisticsAsync();
        Task<List<PartStageRequirement>> GetStageExecutionOrderAsync(int partId);
        Task<bool> ReorderStagesAsync(int partId, List<int> stageOrder);
        Task<List<ProductionStage>> GetAvailableStagesAsync();
        Task<List<PartStageRequirement>> GetPartsUsingStageAsync(int stageId);
    }

    public class PartStageService : IPartStageService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PartStageService> _logger;

        public PartStageService(SchedulerContext context, ILogger<PartStageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PartStageRequirement>> GetPartStagesAsync(int partId)
        {
            try
            {
                return await _context.PartStageRequirements
                    .Where(psr => psr.PartId == partId && psr.IsActive)
                    .OrderBy(psr => psr.ExecutionOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part stages for part ID: {PartId}", partId);
                return new List<PartStageRequirement>();
            }
        }

        public async Task<List<PartStageRequirement>> GetPartStagesWithDetailsAsync(int partId)
        {
            try
            {
                return await _context.PartStageRequirements
                    .Include(psr => psr.ProductionStage)
                    .Include(psr => psr.Part)
                    .Where(psr => psr.PartId == partId && psr.IsActive)
                    .OrderBy(psr => psr.ExecutionOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part stages with details for part ID: {PartId}", partId);
                return new List<PartStageRequirement>();
            }
        }

        public async Task<PartStageRequirement?> GetPartStageAsync(int partId, int stageId)
        {
            try
            {
                return await _context.PartStageRequirements
                    .Include(psr => psr.ProductionStage)
                    .FirstOrDefaultAsync(psr => psr.PartId == partId && 
                                               psr.ProductionStageId == stageId && 
                                               psr.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part stage for part ID: {PartId}, stage ID: {StageId}", partId, stageId);
                return null;
            }
        }

        public async Task<bool> AddPartStageAsync(PartStageRequirement partStage)
        {
            try
            {
                // Check if this combination already exists
                var existing = await _context.PartStageRequirements
                    .FirstOrDefaultAsync(psr => psr.PartId == partStage.PartId && 
                                               psr.ProductionStageId == partStage.ProductionStageId);

                if (existing != null)
                {
                    if (!existing.IsActive)
                    {
                        // Reactivate existing record
                        existing.IsActive = true;
                        existing.LastModifiedDate = DateTime.UtcNow;
                        existing.LastModifiedBy = partStage.LastModifiedBy;
                        return await _context.SaveChangesAsync() > 0;
                    }
                    return false; // Already exists and active
                }

                // Set execution order if not specified
                if (partStage.ExecutionOrder <= 0)
                {
                    var maxOrder = await _context.PartStageRequirements
                        .Where(psr => psr.PartId == partStage.PartId && psr.IsActive)
                        .MaxAsync(psr => (int?)psr.ExecutionOrder) ?? 0;
                    partStage.ExecutionOrder = maxOrder + 1;
                }

                _context.PartStageRequirements.Add(partStage);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding part stage for part ID: {PartId}, stage ID: {StageId}", 
                    partStage.PartId, partStage.ProductionStageId);
                return false;
            }
        }

        public async Task<bool> UpdatePartStageAsync(PartStageRequirement partStage)
        {
            try
            {
                var existing = await _context.PartStageRequirements
                    .FirstOrDefaultAsync(psr => psr.Id == partStage.Id);

                if (existing == null) return false;

                // Update fields
                existing.ExecutionOrder = partStage.ExecutionOrder;
                existing.IsRequired = partStage.IsRequired;
                existing.EstimatedHours = partStage.EstimatedHours;
                existing.SetupTimeMinutes = partStage.SetupTimeMinutes;
                existing.StageParameters = partStage.StageParameters;
                existing.SpecialInstructions = partStage.SpecialInstructions;
                existing.QualityRequirements = partStage.QualityRequirements;
                existing.RequiredMaterials = partStage.RequiredMaterials;
                existing.RequiredTooling = partStage.RequiredTooling;
                existing.EstimatedCost = partStage.EstimatedCost;
                existing.AllowParallelExecution = partStage.AllowParallelExecution;
                existing.IsBlocking = partStage.IsBlocking;
                existing.RequirementNotes = partStage.RequirementNotes;
                existing.LastModifiedDate = DateTime.UtcNow;
                existing.LastModifiedBy = partStage.LastModifiedBy;

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part stage ID: {Id}", partStage.Id);
                return false;
            }
        }

        public async Task<bool> RemovePartStageAsync(int partId, int stageId)
        {
            try
            {
                var partStage = await _context.PartStageRequirements
                    .FirstOrDefaultAsync(psr => psr.PartId == partId && 
                                               psr.ProductionStageId == stageId && 
                                               psr.IsActive);

                if (partStage != null)
                {
                    partStage.IsActive = false;
                    partStage.LastModifiedDate = DateTime.UtcNow;
                    return await _context.SaveChangesAsync() > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing part stage for part ID: {PartId}, stage ID: {StageId}", partId, stageId);
                return false;
            }
        }

        public async Task<bool> RemoveAllPartStagesAsync(int partId)
        {
            try
            {
                var partStages = await _context.PartStageRequirements
                    .Where(psr => psr.PartId == partId && psr.IsActive)
                    .ToListAsync();

                foreach (var stage in partStages)
                {
                    stage.IsActive = false;
                    stage.LastModifiedDate = DateTime.UtcNow;
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing all part stages for part ID: {PartId}", partId);
                return false;
            }
        }

        public async Task<decimal> CalculateTotalEstimatedCostAsync(int partId)
        {
            try
            {
                var partStages = await GetPartStagesWithDetailsAsync(partId);
                return partStages.Sum(ps => ps.CalculateTotalEstimatedCost());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total cost for part ID: {PartId}", partId);
                return 0;
            }
        }

        public async Task<double> CalculateTotalEstimatedTimeAsync(int partId)
        {
            try
            {
                var partStages = await GetPartStagesWithDetailsAsync(partId);
                
                // Calculate sequential time (non-parallel stages)
                var sequentialStages = partStages.Where(ps => !ps.AllowParallelExecution);
                var sequentialTime = sequentialStages.Sum(ps => ps.GetEffectiveEstimatedHours() + (ps.SetupTimeMinutes / 60.0));
                
                // Calculate parallel time (max of parallel stages)
                var parallelStages = partStages.Where(ps => ps.AllowParallelExecution);
                var parallelTime = parallelStages.Any() 
                    ? parallelStages.Max(ps => ps.GetEffectiveEstimatedHours() + (ps.SetupTimeMinutes / 60.0))
                    : 0;
                
                return (sequentialTime ?? 0.0) + (parallelTime ?? 0.0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total time for part ID: {PartId}", partId);
                return 0;
            }
        }

        public async Task<string> GetComplexityLevelAsync(int partId)
        {
            try
            {
                var partStages = await GetPartStagesAsync(partId);
                var totalTime = await CalculateTotalEstimatedTimeAsync(partId);
                
                var score = 0;
                
                // Time complexity
                if (totalTime > 48) score += 4;
                else if (totalTime > 24) score += 3;
                else if (totalTime > 12) score += 2;
                else if (totalTime > 4) score += 1;
                
                // Stage complexity
                score += partStages.Count;
                
                // Special requirements complexity
                score += partStages.Count(ps => ps.IsRequired && !ps.AllowParallelExecution);
                
                return score switch
                {
                    <= 3 => "Simple",
                    <= 6 => "Medium",
                    <= 10 => "Complex",
                    _ => "Very Complex"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating complexity for part ID: {PartId}", partId);
                return "Unknown";
            }
        }

        public async Task<List<StageIndicator>> GetStageIndicatorsAsync(int partId)
        {
            try
            {
                var partStages = await GetPartStagesWithDetailsAsync(partId);
                
                return partStages.Select(ps => new StageIndicator
                {
                    Name = ps.ProductionStage.Name,
                    Class = GetStageColorClass(ps.ProductionStage.Name),
                    Icon = GetStageIcon(ps.ProductionStage.Name),
                    Title = $"{ps.ProductionStage.Name} - {ps.GetEffectiveEstimatedHours():F1}h",
                    IsRequired = ps.IsRequired,
                    IsComplete = false, // This would come from job execution data
                    Order = ps.ExecutionOrder
                }).OrderBy(si => si.Order).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stage indicators for part ID: {PartId}", partId);
                return new List<StageIndicator>();
            }
        }

        public async Task<Dictionary<string, int>> GetStageUsageStatisticsAsync()
        {
            try
            {
                var stats = await _context.PartStageRequirements
                    .Include(psr => psr.ProductionStage)
                    .Where(psr => psr.IsActive)
                    .GroupBy(psr => psr.ProductionStage.Name)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
                
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stage usage statistics");
                return new Dictionary<string, int>();
            }
        }

        public async Task<List<PartStageRequirement>> GetStageExecutionOrderAsync(int partId)
        {
            return await GetPartStagesWithDetailsAsync(partId);
        }

        public async Task<bool> ReorderStagesAsync(int partId, List<int> stageOrder)
        {
            try
            {
                var partStages = await _context.PartStageRequirements
                    .Where(psr => psr.PartId == partId && psr.IsActive)
                    .ToListAsync();

                for (int i = 0; i < stageOrder.Count; i++)
                {
                    var stage = partStages.FirstOrDefault(ps => ps.Id == stageOrder[i]);
                    if (stage != null)
                    {
                        stage.ExecutionOrder = i + 1;
                        stage.LastModifiedDate = DateTime.UtcNow;
                    }
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering stages for part ID: {PartId}", partId);
                return false;
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
                _logger.LogError(ex, "Error getting available stages");
                return new List<ProductionStage>();
            }
        }

        public async Task<List<PartStageRequirement>> GetPartsUsingStageAsync(int stageId)
        {
            try
            {
                return await _context.PartStageRequirements
                    .Include(psr => psr.Part)
                    .Where(psr => psr.ProductionStageId == stageId && psr.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts using stage ID: {StageId}", stageId);
                return new List<PartStageRequirement>();
            }
        }

        #region Private Helper Methods

        private static string GetStageColorClass(string stageName)
        {
            return stageName.ToLowerInvariant() switch
            {
                var name when name.Contains("sls") || name.Contains("print") => "bg-primary",
                var name when name.Contains("cnc") || name.Contains("machin") => "bg-success",
                var name when name.Contains("edm") => "bg-warning",
                var name when name.Contains("assembly") => "bg-info",
                var name when name.Contains("finish") || name.Contains("coat") => "bg-secondary",
                var name when name.Contains("quality") || name.Contains("inspect") => "bg-danger",
                _ => "bg-dark"
            };
        }

        private static string GetStageIcon(string stageName)
        {
            return stageName.ToLowerInvariant() switch
            {
                var name when name.Contains("sls") || name.Contains("print") => "fas fa-print",
                var name when name.Contains("cnc") || name.Contains("machin") => "fas fa-cogs",
                var name when name.Contains("edm") => "fas fa-bolt",
                var name when name.Contains("assembly") => "fas fa-puzzle-piece",
                var name when name.Contains("finish") || name.Contains("coat") => "fas fa-brush",
                var name when name.Contains("quality") || name.Contains("inspect") => "fas fa-search",
                _ => "fas fa-tasks"
            };
        }

        #endregion
    }
}