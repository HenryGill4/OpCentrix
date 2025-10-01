using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.ViewModels.PrintTracking;
using System.Text.Json;

namespace OpCentrix.Services
{
    public interface IProductionBuildService
    {
        // Production Build Management
        Task<int> StartProductionBuildAsync(StreamlinedPrintStartViewModel model, int userId);
        Task<int> StartProductionBuildAsync(ProductionBuildStartData data, int userId); // NEW: Overload for StartData
        Task<bool> CompleteSlsStageAsync(int productionBuildId, SlsCompletionData data, int userId);
        Task<ProductionBuildDashboardViewModel> GetDashboardDataAsync(int userId);
        Task<ProductionBuildDetailsViewModel> GetProductionBuildDetailsAsync(int productionBuildId);
        
        // Stage Execution Management
        Task<int> StartStageExecutionAsync(int productionBuildId, string stageName, int userId);
        Task<bool> CompleteStageExecutionAsync(int stageExecutionId, StageCompletionData data, int userId);
        Task<List<StageExecutionSummary>> GetPendingStageExecutionsAsync(string stageName);
        
        // Master Part Management
        Task<List<MasterPartOption>> GetAvailableMasterPartsAsync();
        Task<MasterPart?> GetMasterPartAsync(int masterPartId);
        Task<StreamlinedPrintStartViewModel> PrepareStartFormAsync(string printerName);
        
        // Analytics and Reporting
        Task<List<ProductionAnalytics>> GetProductionAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, object>> GetStagePerformanceMetricsAsync(string stageName, int days = 30);
        
        // Batch and Quality Management
        Task<List<PartBatchSummary>> GetPartBatchesAsync(string? currentStage = null);
        Task<bool> MovePartBatchToNextStageAsync(int partBatchId, int userId);
        Task<bool> RecordQualityIssueAsync(int stageExecutionId, QualityIssueData data, int userId);
    }

    public class ProductionBuildService : IProductionBuildService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<ProductionBuildService> _logger;

        public ProductionBuildService(SchedulerContext context, ILogger<ProductionBuildService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> StartProductionBuildAsync(StreamlinedPrintStartViewModel model, int userId)
        {
            try
            {
                var masterPart = await _context.MasterParts.FindAsync(model.MasterPartId);
                if (masterPart == null)
                {
                    throw new InvalidOperationException($"Master part {model.MasterPartId} not found");
                }

                // Generate build number
                var buildNumber = await GenerateBuildNumberAsync();

                // Get machine material info
                var machine = await _context.Machines.FirstOrDefaultAsync(m => m.MachineId == model.PrinterName);
                var materialBatch = machine?.CurrentMaterial ?? "Unknown";
                var powderLot = await GeneratePowderLotAsync();

                // Create production build
                var productionBuild = new ProductionBuild
                {
                    BuildNumber = buildNumber,
                    MasterPartId = model.MasterPartId,
                    PrinterName = model.PrinterName,
                    BuildQuantity = model.BuildQuantity,
                    StackLevel = model.StackLevel,
                    MaterialBatch = materialBatch,
                    PowderLot = powderLot,
                    AddedPowder = model.AddedPowder,
                    PowderAmountKg = model.PowderAmountKg,
                    ActualStartTime = model.ActualStartTime,
                    ScheduledStartTime = model.ScheduledStartTime,
                    Status = "InProgress",
                    CreatedByUserId = userId,
                    SetupNotes = model.SetupNotes
                };

                _context.ProductionBuilds.Add(productionBuild);
                await _context.SaveChangesAsync();

                // Create stage executions based on master part requirements
                await CreateStageExecutionsAsync(productionBuild, masterPart);

                // Create initial part batch for SLS stage
                await CreateInitialPartBatchAsync(productionBuild);

                // Start the first stage (usually SLS)
                await StartFirstStageAsync(productionBuild.Id, userId);

                _logger.LogInformation("Started production build {BuildNumber} for {PartNumber} with {Quantity} parts",
                    buildNumber, masterPart.PartNumber, model.BuildQuantity);

                return productionBuild.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting production build for master part {MasterPartId}", model.MasterPartId);
                throw;
            }
        }

        public async Task<int> StartProductionBuildAsync(ProductionBuildStartData data, int userId)
        {
            try
            {
                var masterPart = await _context.MasterParts.FindAsync(data.MasterPartId);
                if (masterPart == null)
                {
                    throw new InvalidOperationException($"Master part {data.MasterPartId} not found");
                }

                // Generate build number
                var buildNumber = await GenerateBuildNumberAsync();

                // Create production build
                var productionBuild = new ProductionBuild
                {
                    BuildNumber = buildNumber,
                    MasterPartId = data.MasterPartId,
                    PrinterName = data.PrinterName,
                    BuildQuantity = data.BuildQuantity,
                    StackLevel = data.StackLevel,
                    MaterialBatch = data.MaterialBatch,
                    PowderLot = data.PowderLot,
                    AddedPowder = data.AddedPowder,
                    PowderAmountKg = data.PowderAmountKg,
                    ActualStartTime = data.ActualStartTime,
                    Status = "InProgress",
                    CreatedByUserId = userId,
                    SetupNotes = data.SetupNotes
                };

                _context.ProductionBuilds.Add(productionBuild);
                await _context.SaveChangesAsync();

                // Create stage executions based on master part requirements
                await CreateStageExecutionsAsync(productionBuild, masterPart);

                // Create initial part batch for SLS stage
                await CreateInitialPartBatchAsync(productionBuild);

                // Start the first stage (usually SLS)
                await StartFirstStageAsync(productionBuild.Id, userId);

                _logger.LogInformation("Started production build {BuildNumber} for {PartNumber} with {Quantity} parts",
                    buildNumber, masterPart.PartNumber, data.BuildQuantity);

                return productionBuild.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting production build for master part {MasterPartId}", data.MasterPartId);
                throw;
            }
        }

        public async Task<bool> CompleteSlsStageAsync(int productionBuildId, SlsCompletionData data, int userId)
        {
            try
            {
                var productionBuild = await _context.ProductionBuilds
                    .Include(pb => pb.StageExecutions)
                    .Include(pb => pb.MasterPart)
                    .FirstOrDefaultAsync(pb => pb.Id == productionBuildId);

                if (productionBuild == null) return false;

                // Find SLS stage execution
                var slsExecution = productionBuild.StageExecutions
                    .FirstOrDefault(se => se.StageName == "SLS" && se.Status == "InProgress");

                if (slsExecution == null) return false;

                // Complete SLS stage
                slsExecution.EndTime = data.ActualEndTime;
                slsExecution.ActualHours = data.ActualHours;
                slsExecution.QuantityOut = data.GoodParts;
                slsExecution.DefectCount = data.DefectiveParts;
                slsExecution.Status = "Completed";
                slsExecution.CompletedDate = DateTime.UtcNow;
                slsExecution.OperatorNotes = data.Notes;

                // Store SLS-specific data
                var slsData = new
                {
                    LaserRunTime = data.LaserRunTime,
                    GasUsedL = data.GasUsedL,
                    PowderUsedL = data.PowderUsedL,
                    LayerCount = data.LayerCount,
                    BuildHeight = data.BuildHeight,
                    SupportComplexity = data.SupportComplexity,
                    PowerConsumption = data.PowerConsumption,
                    LaserOnTime = data.LaserOnTime
                };
                slsExecution.StageData = JsonSerializer.Serialize(slsData);

                // Complete production build if this was the only stage
                var requiredStages = JsonSerializer.Deserialize<List<string>>(productionBuild.MasterPart.RequiredStages) ?? new List<string>();
                if (requiredStages.Count == 1 && requiredStages[0] == "SLS")
                {
                    productionBuild.Status = "Completed";
                    productionBuild.ActualEndTime = data.ActualEndTime;
                    productionBuild.CompletedDate = DateTime.UtcNow;
                }
                else
                {
                    // Start next stage (EDM for cut-off)
                    await StartNextStageAsync(productionBuildId, "SLS", userId);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Completed SLS stage for build {BuildNumber}, produced {GoodParts} good parts, {DefectiveParts} defective",
                    productionBuild.BuildNumber, data.GoodParts, data.DefectiveParts);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing SLS stage for production build {ProductionBuildId}", productionBuildId);
                return false;
            }
        }

        public async Task<ProductionBuildDashboardViewModel> GetDashboardDataAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                var today = DateTime.Today;

                // Get active builds - FIXED: Remove CreatedByUser Include since FK constraint doesn't exist
                var activeBuilds = await _context.ProductionBuilds
                    .Include(pb => pb.MasterPart)
                    .Where(pb => pb.Status == "InProgress")
                    .OrderBy(pb => pb.ActualStartTime)
                    .ToListAsync();

                // Get recent completed builds - FIXED: Remove CreatedByUser Include since FK constraint doesn't exist  
                var recentCompleted = await _context.ProductionBuilds
                    .Include(pb => pb.MasterPart)
                    .Where(pb => pb.Status == "Completed" && pb.CompletedDate >= today)
                    .OrderByDescending(pb => pb.CompletedDate)
                    .Take(10)
                    .ToListAsync();

                // Get active stage executions
                var activeStageExecutions = await _context.StageExecutions
                    .Include(se => se.ProductionBuild)
                        .ThenInclude(pb => pb.MasterPart)
                    .Include(se => se.OperatorUser)
                    .Where(se => se.Status == "InProgress")
                    .OrderBy(se => se.StartTime)
                    .ToListAsync();

                // Get pending batches
                var pendingBatches = await _context.PartBatches
                    .Include(pb => pb.ProductionBuild)
                        .ThenInclude(pb => pb.MasterPart)
                    .Where(pb => pb.QualityStatus == "Good" && pb.CurrentStage != "Completed")
                    .OrderBy(pb => pb.CreatedDate)
                    .ToListAsync();

                // Calculate metrics
                var buildsByPrinter = activeBuilds
                    .GroupBy(pb => pb.PrinterName)
                    .ToDictionary(g => g.Key, g => g.Count());

                var partsByStage = activeStageExecutions
                    .GroupBy(se => se.StageName)
                    .ToDictionary(g => g.Key, g => g.Sum(se => se.QuantityIn));

                var totalPartsToday = recentCompleted.Sum(pb => pb.BuildQuantity);
                var avgEfficiency = CalculateAverageEfficiency(recentCompleted);
                var avgYield = CalculateAverageYield(activeStageExecutions.Where(se => se.IsCompleted).ToList());

                return new ProductionBuildDashboardViewModel
                {
                    ActiveBuilds = activeBuilds.Select(MapToProductionBuildSummary).ToList(),
                    RecentCompletedBuilds = recentCompleted.Select(MapToProductionBuildSummary).ToList(),
                    ActiveStageExecutions = activeStageExecutions.Select(MapToStageExecutionSummary).ToList(),
                    PendingBatches = pendingBatches.Select(MapToPartBatchSummary).ToList(),
                    BuildsByPrinter = buildsByPrinter,
                    PartsByStage = partsByStage,
                    TotalPartsProducedToday = totalPartsToday,
                    OverallEfficiency = avgEfficiency,
                    AverageYield = avgYield,
                    OperatorName = user?.FullName ?? "Unknown",
                    UserId = userId,
                    UserRole = user?.Role ?? "User",
                    UserPermissions = await GetUserPermissionsAsync(userId)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data for user {UserId}", userId);
                return new ProductionBuildDashboardViewModel();
            }
        }

        public async Task<List<MasterPartOption>> GetAvailableMasterPartsAsync()
        {
            try
            {
                var masterParts = await _context.MasterParts
                    .Where(mp => mp.IsActive)
                    .OrderBy(mp => mp.PartNumber)
                    .ToListAsync();

                return masterParts.Select(mp => new MasterPartOption
                {
                    Id = mp.Id,
                    PartNumber = mp.PartNumber,
                    Name = mp.Name,
                    Material = mp.Material,
                    RequiredStages = GetStagesSummary(mp.RequiredStages),
                    AllowStacking = mp.AllowStacking,
                    MaxStackCount = mp.MaxStackCount != 0 ? mp.MaxStackCount : 1, // FIXED: Use ternary instead of null-coalescing
                    EstimatedHours = mp.SingleStackDurationHours ?? 8.0
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available master parts");
                return new List<MasterPartOption>();
            }
        }

        public async Task<StreamlinedPrintStartViewModel> PrepareStartFormAsync(string printerName)
        {
            try
            {
                var availablePrinters = await _context.Machines
                    .Where(m => m.IsActive && m.MachineType == "SLS")
                    .Select(m => m.MachineId)
                    .ToListAsync();

                var availableMasterParts = await GetAvailableMasterPartsAsync();

                var machine = await _context.Machines.FirstOrDefaultAsync(m => m.MachineId == printerName);

                return new StreamlinedPrintStartViewModel
                {
                    PrinterName = printerName,
                    AvailablePrinters = availablePrinters,
                    AvailableMasterParts = availableMasterParts,
                    MaterialBatch = machine?.CurrentMaterial ?? "Unknown",
                    PowderLot = "LOT-2025-001",
                    BuildNumber = await GenerateBuildNumberAsync()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing start form for printer {PrinterName}", printerName);
                return new StreamlinedPrintStartViewModel();
            }
        }

        // Additional methods would go here...
        
        #region Private Helper Methods

        private async Task<string> GenerateBuildNumberAsync()
        {
            var today = DateTime.Today;
            var todayBuilds = await _context.ProductionBuilds
                .Where(pb => pb.CreatedDate.Date == today)
                .CountAsync();

            return $"BUILD-{today:yyyy-MM-dd}-{todayBuilds + 1:D3}";
        }

        private async Task<string> GeneratePowderLotAsync()
        {
            return $"LOT-{DateTime.Now:yyyy-MM}-{Random.Shared.Next(1, 999):D3}";
        }

        private async Task CreateStageExecutionsAsync(ProductionBuild productionBuild, MasterPart masterPart)
        {
            try
            {
                var requiredStages = JsonSerializer.Deserialize<List<string>>(masterPart.RequiredStages) ?? new List<string>();
                var stageDefinitions = await _context.StageDefinitions
                    .Where(sd => sd.MasterPartId == masterPart.Id && sd.IsActive)
                    .OrderBy(sd => sd.ExecutionOrder)
                    .ToListAsync();

                for (int i = 0; i < stageDefinitions.Count; i++)
                {
                    var stageDef = stageDefinitions[i];
                    var stageExecution = new StageExecution
                    {
                        ProductionBuildId = productionBuild.Id,
                        StageDefinitionId = stageDef.Id,
                        StageName = stageDef.StageName,
                        ExecutionOrder = stageDef.ExecutionOrder,
                        QuantityIn = i == 0 ? productionBuild.BuildQuantity : 0, // First stage gets full quantity
                        Status = i == 0 ? "InProgress" : "NotStarted", // Only first stage starts
                        StageData = GetDefaultStageData(stageDef.StageName)
                    };

                    _context.StageExecutions.Add(stageExecution);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stage executions for production build {BuildId}", productionBuild.Id);
                throw;
            }
        }

        private async Task CreateInitialPartBatchAsync(ProductionBuild productionBuild)
        {
            var initialBatch = new PartBatch
            {
                ProductionBuildId = productionBuild.Id,
                BatchNumber = $"{productionBuild.BuildNumber}-SLS",
                CurrentStage = "SLS",
                Quantity = productionBuild.BuildQuantity,
                QualityStatus = "InProgress",
                Location = productionBuild.PrinterName
            };

            _context.PartBatches.Add(initialBatch);
            await _context.SaveChangesAsync();
        }

        private async Task StartFirstStageAsync(int productionBuildId, int userId)
        {
            var firstStage = await _context.StageExecutions
                .FirstOrDefaultAsync(se => se.ProductionBuildId == productionBuildId && se.ExecutionOrder == 1);

            if (firstStage != null)
            {
                firstStage.StartTime = DateTime.UtcNow;
                firstStage.OperatorUserId = userId;
                firstStage.Status = "InProgress";
                await _context.SaveChangesAsync();
            }
        }

        private async Task StartNextStageAsync(int productionBuildId, string completedStage, int userId)
        {
            var nextStage = await _context.StageExecutions
                .Where(se => se.ProductionBuildId == productionBuildId && se.Status == "NotStarted")
                .OrderBy(se => se.ExecutionOrder)
                .FirstOrDefaultAsync();

            if (nextStage != null)
            {
                // Transfer quantity from completed stage
                var completedStageExecution = await _context.StageExecutions
                    .FirstOrDefaultAsync(se => se.ProductionBuildId == productionBuildId && se.StageName == completedStage);

                if (completedStageExecution != null)
                {
                    nextStage.QuantityIn = completedStageExecution.QuantityOut;
                    nextStage.Status = "NotStarted"; // Ready to start, but not auto-started
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Prepared next stage {StageName} with {Quantity} parts for build {ProductionBuildId}",
                        nextStage.StageName, nextStage.QuantityIn, productionBuildId);
                }
            }
        }

        private string GetDefaultStageData(string stageName)
        {
            return stageName switch
            {
                "SLS" => StageDataTemplates.SLS.Template,
                "EDM" => StageDataTemplates.EDM.Template,
                "CNC" => StageDataTemplates.CNC.Template,
                "Assembly" => StageDataTemplates.Assembly.Template,
                _ => "{}"
            };
        }

        private string GetStagesSummary(string requiredStagesJson)
        {
            try
            {
                var stages = JsonSerializer.Deserialize<List<string>>(requiredStagesJson) ?? new List<string>();
                return string.Join(" ? ", stages);
            }
            catch
            {
                return "Unknown";
            }
        }

        private ProductionBuildSummary MapToProductionBuildSummary(ProductionBuild pb)
        {
            return new ProductionBuildSummary
            {
                Id = pb.Id,
                BuildNumber = pb.BuildNumber,
                PartNumber = pb.MasterPart?.PartNumber ?? "Unknown",
                PartName = pb.MasterPart?.Name ?? "Unknown",
                PrinterName = pb.PrinterName,
                BuildQuantity = pb.BuildQuantity,
                StackLevel = pb.StackLevel,
                Status = pb.Status,
                ActualStartTime = pb.ActualStartTime,
                ActualEndTime = pb.ActualEndTime,
                ActualDurationHours = pb.ActualDurationHours,
                OperatorName = "Unknown" // FIXED: CreatedByUser navigation property has no FK constraint
            };
        }

        private StageExecutionSummary MapToStageExecutionSummary(StageExecution se)
        {
            return new StageExecutionSummary
            {
                Id = se.Id,
                BuildNumber = se.ProductionBuild?.BuildNumber ?? "Unknown",
                PartNumber = se.ProductionBuild?.MasterPart?.PartNumber ?? "Unknown",
                StageName = se.StageName,
                MachineUsed = se.MachineUsed ?? "Unknown",
                OperatorName = se.OperatorUser?.FullName ?? "Unknown",
                Status = se.Status,
                StartTime = se.StartTime,
                QuantityIn = se.QuantityIn,
                QuantityOut = se.QuantityOut,
                YieldPercentage = se.YieldPercentage
            };
        }

        private PartBatchSummary MapToPartBatchSummary(PartBatch pb)
        {
            return new PartBatchSummary
            {
                Id = pb.Id,
                BatchNumber = pb.BatchNumber,
                PartNumber = pb.ProductionBuild?.MasterPart?.PartNumber ?? "Unknown",
                CurrentStage = pb.CurrentStage,
                Quantity = pb.Quantity,
                QualityStatus = pb.QualityStatus,
                Location = pb.Location ?? "Unknown",
                LastUpdated = pb.LastModifiedDate
            };
        }

        private double CalculateAverageEfficiency(List<ProductionBuild> builds)
        {
            if (!builds.Any()) return 0;

            // Calculate efficiency based on actual vs estimated duration
            // This is a simplified calculation - you can enhance it based on your needs
            return 85.0; // Placeholder
        }

        private double CalculateAverageYield(List<StageExecution> completedExecutions)
        {
            if (!completedExecutions.Any()) return 100;

            return completedExecutions.Average(se => se.YieldPercentage);
        }

        private async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new List<string>();

            return user.Role switch
            {
                "Admin" => new List<string> { "ViewAll", "EditAll", "DeleteAll", "ManageUsers", "ManageSystem" },
                "Supervisor" => new List<string> { "ViewAll", "EditOwn", "ManageTeam" },
                "Operator" => new List<string> { "ViewOwn", "EditOwn" },
                _ => new List<string> { "ViewOwn" }
            };
        }

        #endregion

        // Placeholder implementations for interface methods
        public Task<ProductionBuildDetailsViewModel> GetProductionBuildDetailsAsync(int productionBuildId) => throw new NotImplementedException();
        public Task<int> StartStageExecutionAsync(int productionBuildId, string stageName, int userId) => throw new NotImplementedException();
        public Task<bool> CompleteStageExecutionAsync(int stageExecutionId, StageCompletionData data, int userId) => throw new NotImplementedException();
        public Task<List<StageExecutionSummary>> GetPendingStageExecutionsAsync(string stageName) => throw new NotImplementedException();
        public Task<MasterPart?> GetMasterPartAsync(int masterPartId) => throw new NotImplementedException();
        public Task<List<ProductionAnalytics>> GetProductionAnalyticsAsync(DateTime startDate, DateTime endDate) => throw new NotImplementedException();
        public Task<Dictionary<string, object>> GetStagePerformanceMetricsAsync(string stageName, int days = 30) => throw new NotImplementedException();
        public Task<List<PartBatchSummary>> GetPartBatchesAsync(string? currentStage = null) => throw new NotImplementedException();
        public Task<bool> MovePartBatchToNextStageAsync(int partBatchId, int userId) => throw new NotImplementedException();
        public Task<bool> RecordQualityIssueAsync(int stageExecutionId, QualityIssueData data, int userId) => throw new NotImplementedException();
    }

    // Supporting data classes
    public class SlsCompletionData
    {
        public DateTime ActualEndTime { get; set; }
        public double ActualHours { get; set; }
        public int GoodParts { get; set; }
        public int DefectiveParts { get; set; }
        public string? LaserRunTime { get; set; }
        public float? GasUsedL { get; set; }
        public float? PowderUsedL { get; set; }
        public int? LayerCount { get; set; }
        public decimal? BuildHeight { get; set; }
        public string? SupportComplexity { get; set; }
        public decimal? PowerConsumption { get; set; }
        public decimal? LaserOnTime { get; set; }
        public string? Notes { get; set; }
    }

    public class StageCompletionData
    {
        public DateTime EndTime { get; set; }
        public double ActualHours { get; set; }
        public int QuantityOut { get; set; }
        public int DefectCount { get; set; }
        public int ReworkCount { get; set; }
        public string? MachineUsed { get; set; }
        public string? OperatorNotes { get; set; }
        public string? QualityNotes { get; set; }
        public bool PassedQuality { get; set; } = true;
        public Dictionary<string, object>? StageSpecificData { get; set; }
    }

    public class QualityIssueData
    {
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium";
        public int AffectedQuantity { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? RootCause { get; set; }
    }

    public class ProductionAnalytics
    {
        public string PartNumber { get; set; } = string.Empty;
        public int TotalBuilds { get; set; }
        public int TotalParts { get; set; }
        public double AverageYield { get; set; }
        public double AverageEfficiency { get; set; }
        public decimal TotalCost { get; set; }
        public Dictionary<string, double> StagePerformance { get; set; } = new();
    }

    public class ProductionBuildDetailsViewModel
    {
        // Placeholder for detailed production build view
    }
}