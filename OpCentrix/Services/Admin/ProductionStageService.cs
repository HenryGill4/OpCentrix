using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin
{
    /// <summary>
    /// Service for managing production stages that define the manufacturing workflow
    /// Allows admins to configure stages like 3D Printing, CNC, EDM, etc.
    /// </summary>
    public class ProductionStageService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<ProductionStageService> _logger;

        public ProductionStageService(SchedulerContext context, ILogger<ProductionStageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Stage Configuration

        /// <summary>
        /// Gets all production stages ordered by display order
        /// </summary>
        public async Task<List<ProductionStage>> GetAllStagesAsync()
        {
            return await _context.ProductionStages
                .Where(ps => ps.IsActive)
                .OrderBy(ps => ps.DisplayOrder)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a specific production stage by ID
        /// </summary>
        public async Task<ProductionStage?> GetStageAsync(int stageId)
        {
            return await _context.ProductionStages
                .Include(ps => ps.StageExecutions)
                .FirstOrDefaultAsync(ps => ps.Id == stageId);
        }

        /// <summary>
        /// Creates a new production stage
        /// </summary>
        public async Task<bool> CreateStageAsync(ProductionStage stage)
        {
            try
            {
                // Set display order to the next available position
                var maxOrder = await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .MaxAsync(ps => (int?)ps.DisplayOrder) ?? 0;

                stage.DisplayOrder = maxOrder + 1;
                stage.CreatedDate = DateTime.UtcNow;
                stage.IsActive = true;

                _context.ProductionStages.Add(stage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created production stage: {StageName}", stage.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating production stage: {StageName}", stage.Name);
                return false;
            }
        }

        /// <summary>
        /// Updates an existing production stage
        /// </summary>
        public async Task<bool> UpdateStageAsync(ProductionStage stage)
        {
            try
            {
                _context.ProductionStages.Update(stage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated production stage: {StageName}", stage.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating production stage {StageId}", stage.Id);
                return false;
            }
        }

        /// <summary>
        /// Soft deletes a production stage (marks as inactive)
        /// </summary>
        public async Task<bool> DeleteStageAsync(int stageId)
        {
            try
            {
                var stage = await _context.ProductionStages.FindAsync(stageId);
                if (stage == null)
                {
                    _logger.LogWarning("Production stage not found: {StageId}", stageId);
                    return false;
                }

                // Check if stage is being used in any prototype jobs
                var isInUse = await _context.ProductionStageExecutions
                    .AnyAsync(se => se.ProductionStageId == stageId);

                if (isInUse)
                {
                    _logger.LogWarning("Cannot delete production stage {StageId} - it is being used by prototype jobs", stageId);
                    return false;
                }

                stage.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted production stage: {StageName}", stage.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting production stage {StageId}", stageId);
                return false;
            }
        }

        /// <summary>
        /// Reorders production stages
        /// </summary>
        public async Task<bool> ReorderStagesAsync(List<int> stageIds)
        {
            try
            {
                var stages = await _context.ProductionStages
                    .Where(ps => stageIds.Contains(ps.Id))
                    .ToListAsync();

                for (int i = 0; i < stageIds.Count; i++)
                {
                    var stage = stages.FirstOrDefault(s => s.Id == stageIds[i]);
                    if (stage != null)
                    {
                        stage.DisplayOrder = i + 1;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Reordered {Count} production stages", stageIds.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering production stages");
                return false;
            }
        }

        #endregion

        #region Default Stage Setup

        /// <summary>
        /// Creates the default set of production stages for B&T manufacturing
        /// </summary>
        public async Task<bool> CreateDefaultStagesAsync()
        {
            try
            {
                // Check if stages already exist
                var existingStagesCount = await _context.ProductionStages.CountAsync(ps => ps.IsActive);
                if (existingStagesCount > 0)
                {
                    _logger.LogWarning("Production stages already exist, skipping default creation");
                    return false;
                }

                var defaultStages = GetDefaultStageTemplates();

                foreach (var stage in defaultStages)
                {
                    _context.ProductionStages.Add(stage);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Created {Count} default production stages", defaultStages.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default production stages");
                return false;
            }
        }

        /// <summary>
        /// Gets the template stages for B&T manufacturing
        /// </summary>
        public async Task<List<ProductionStage>> GetStageTemplatesAsync()
        {
            // Return the default templates without saving them
            return GetDefaultStageTemplates();
        }

        #endregion

        #region Stage Analytics

        /// <summary>
        /// Gets performance metrics for a specific stage
        /// </summary>
        public async Task<StagePerformanceMetrics> GetStagePerformanceAsync(int stageId)
        {
            var executions = await _context.ProductionStageExecutions
                .Where(se => se.ProductionStageId == stageId && se.Status == "Completed")
                .ToListAsync();

            var metrics = new StagePerformanceMetrics
            {
                StageId = stageId,
                TotalExecutions = executions.Count,
                AverageActualHours = executions.Any() ? executions.Average(e => e.ActualHours ?? 0) : 0,
                AverageActualCost = executions.Any() ? executions.Average(e => e.ActualCost ?? 0) : 0,
                SuccessRate = executions.Count > 0 ? (decimal)executions.Count(e => e.QualityCheckPassed != false) / executions.Count * 100 : 0
            };

            if (executions.Any(e => e.EstimatedHours.HasValue))
            {
                var estimatedVsActual = executions
                    .Where(e => e.EstimatedHours.HasValue && e.ActualHours.HasValue)
                    .Select(e => new { Estimated = e.EstimatedHours!.Value, Actual = e.ActualHours!.Value })
                    .ToList();

                metrics.AverageTimeVariance = estimatedVsActual.Any()
                    ? estimatedVsActual.Average(e => ((e.Actual - e.Estimated) / e.Estimated) * 100)
                    : 0;
            }

            return metrics;
        }

        /// <summary>
        /// Gets benchmarks for all stages
        /// </summary>
        public async Task<List<StageBenchmark>> GetStageBenchmarksAsync()
        {
            var stages = await _context.ProductionStages
                .Where(ps => ps.IsActive)
                .OrderBy(ps => ps.DisplayOrder)
                .ToListAsync();

            var benchmarks = new List<StageBenchmark>();

            foreach (var stage in stages)
            {
                var executions = await _context.ProductionStageExecutions
                    .Where(se => se.ProductionStageId == stage.Id && se.Status == "Completed")
                    .ToListAsync();

                var benchmark = new StageBenchmark
                {
                    StageId = stage.Id,
                    StageName = stage.Name,
                    TotalJobs = executions.Count,
                    AverageHours = executions.Any() ? executions.Average(e => e.ActualHours ?? 0) : 0,
                    StandardDeviation = CalculateStandardDeviation(executions.Select(e => (double)(e.ActualHours ?? 0)).ToList()),
                    MinHours = executions.Any() ? executions.Min(e => e.ActualHours ?? 0) : 0,
                    MaxHours = executions.Any() ? executions.Max(e => e.ActualHours ?? 0) : 0,
                    MedianHours = CalculateMedian(executions.Select(e => (double)(e.ActualHours ?? 0)).ToList())
                };

                benchmarks.Add(benchmark);
            }

            return benchmarks;
        }

        #endregion

        #region Private Helper Methods

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
                }
            };
        }

        private double CalculateStandardDeviation(List<double> values)
        {
            if (!values.Any()) return 0;

            var average = values.Average();
            var sumOfSquares = values.Sum(x => Math.Pow(x - average, 2));
            return Math.Sqrt(sumOfSquares / values.Count);
        }

        private decimal CalculateMedian(List<double> values)
        {
            if (!values.Any()) return 0;

            var sorted = values.OrderBy(x => x).ToList();
            var count = sorted.Count;

            if (count % 2 == 0)
            {
                return (decimal)((sorted[count / 2 - 1] + sorted[count / 2]) / 2);
            }
            else
            {
                return (decimal)sorted[count / 2];
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Performance metrics for a production stage
    /// </summary>
    public class StagePerformanceMetrics
    {
        public int StageId { get; set; }
        public int TotalExecutions { get; set; }
        public decimal AverageActualHours { get; set; }
        public decimal AverageActualCost { get; set; }
        public decimal AverageTimeVariance { get; set; }
        public decimal SuccessRate { get; set; }
    }

    /// <summary>
    /// Benchmark data for a production stage
    /// </summary>
    public class StageBenchmark
    {
        public int StageId { get; set; }
        public string StageName { get; set; } = string.Empty;
        public int TotalJobs { get; set; }
        public decimal AverageHours { get; set; }
        public double StandardDeviation { get; set; }
        public decimal MinHours { get; set; }
        public decimal MaxHours { get; set; }
        public decimal MedianHours { get; set; }
    }

    #endregion
}