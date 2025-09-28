# Implementation Plan 5: Service Method Implementation

**Priority: HIGH**  
**Estimated Time: 2-3 days**  
**Dependencies: Plans 1-4 must be completed first**

## Overview

Implement the missing service methods that are currently throwing `NotImplementedException`. This will make the ProductionBuild system fully functional and integrate it properly with the enhanced PrintTracking system.

## Current State

**Problems to Solve:**
- ? 50+ methods in ProductionBuildService throw NotImplementedException
- ? No PowderInventoryService exists
- ? Dashboard data loading broken due to missing service methods
- ? Integration between services incomplete

**What We'll Build:**
- ? Complete ProductionBuildService implementation
- ? New PowderInventoryService for powder management
- ? Service integration with PrintTracking system
- ? Proper error handling and logging

---

## Step-by-Step Implementation

### Step 1: Complete ProductionBuildService Implementation

#### 1A: Implement Core CRUD Methods
**File: `Services/ProductionBuildService.cs`**

Replace the NotImplementedException methods with full implementations:

```csharp
public class ProductionBuildService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<ProductionBuildService> _logger;
    private readonly UserManager<IdentityUser> _userManager;

    public ProductionBuildService(
        SchedulerContext context,
        ILogger<ProductionBuildService> logger,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    // === CREATE METHODS ===
    
    public async Task<(bool Success, int BuildId, string Message)> CreateProductionBuildAsync(CreateProductionBuildDto dto)
    {
        try
        {
            // Validate required data
            var masterPart = await _context.MasterParts.FindAsync(dto.MasterPartId);
            if (masterPart == null)
            {
                return (false, 0, "Master part not found");
            }

            var machine = await _context.Machines.FindAsync(dto.MachineId);
            if (machine == null)
            {
                return (false, 0, "Machine not found");
            }

            // Check if machine is available
            if (machine.Status == MachineStatus.Running)
            {
                return (false, 0, "Machine is currently running another job");
            }

            var productionBuild = new ProductionBuild
            {
                MasterPartId = dto.MasterPartId,
                MachineId = dto.MachineId,
                Quantity = dto.Quantity,
                StackLevel = dto.StackLevel,
                ScheduledStartTime = dto.ScheduledStartTime,
                PrinterEstimatedEndTime = dto.PrinterEstimatedEndTime,
                Status = ProductionBuildStatus.Scheduled,
                Priority = dto.Priority,
                CreatedByUserId = dto.CreatedByUserId,
                CreatedDate = DateTime.UtcNow,
                Notes = dto.Notes,
                CustomerOrderNumber = dto.CustomerOrderNumber,
                MaterialType = dto.MaterialType
            };

            _context.ProductionBuilds.Add(productionBuild);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created production build {BuildId} for part {PartId} on machine {MachineId}",
                productionBuild.Id, dto.MasterPartId, dto.MachineId);

            return (true, productionBuild.Id, "Production build created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating production build for part {PartId}", dto.MasterPartId);
            return (false, 0, $"Error creating production build: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> StartProductionBuildAsync(int buildId, string userId)
    {
        try
        {
            var build = await _context.ProductionBuilds
                .Include(pb => pb.Machine)
                .FirstOrDefaultAsync(pb => pb.Id == buildId);

            if (build == null)
            {
                return (false, "Production build not found");
            }

            if (build.Status != ProductionBuildStatus.Scheduled)
            {
                return (false, $"Cannot start build in status: {build.Status}");
            }

            // Update build status
            build.Status = ProductionBuildStatus.InProgress;
            build.ActualStartTime = DateTime.UtcNow;
            build.StartedByUserId = userId;

            // Update machine status
            if (build.Machine != null)
            {
                build.Machine.Status = MachineStatus.Running;
                build.Machine.CurrentJobId = buildId;
                build.Machine.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Started production build {BuildId} by user {UserId}", buildId, userId);
            return (true, "Production build started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting production build {BuildId}", buildId);
            return (false, $"Error starting production build: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> CompleteProductionBuildAsync(int buildId, string userId, CompleteProductionBuildDto? completionData = null)
    {
        try
        {
            var build = await _context.ProductionBuilds
                .Include(pb => pb.Machine)
                .Include(pb => pb.MasterPart)
                .FirstOrDefaultAsync(pb => pb.Id == buildId);

            if (build == null)
            {
                return (false, "Production build not found");
            }

            if (build.Status != ProductionBuildStatus.InProgress)
            {
                return (false, $"Cannot complete build in status: {build.Status}");
            }

            // Update build status
            build.Status = ProductionBuildStatus.Completed;
            build.ActualEndTime = DateTime.UtcNow;
            build.CompletedByUserId = userId;

            // Add completion data if provided
            if (completionData != null)
            {
                build.ActualQuantityProduced = completionData.ActualQuantityProduced ?? build.Quantity;
                build.QualityNotes = completionData.QualityNotes;
                build.DefectiveQuantity = completionData.DefectiveQuantity ?? 0;
            }

            // Update machine accuracy tracking
            await UpdateMachineAccuracyTrackingAsync(build);

            // Update machine status
            if (build.Machine != null)
            {
                build.Machine.Status = MachineStatus.Idle;
                build.Machine.CurrentJobId = null;
                build.Machine.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Completed production build {BuildId} by user {UserId}", buildId, userId);
            return (true, "Production build completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing production build {BuildId}", buildId);
            return (false, $"Error completing production build: {ex.Message}");
        }
    }

    // === READ METHODS ===

    public async Task<ProductionBuildViewModel?> GetProductionBuildAsync(int buildId)
    {
        try
        {
            var build = await _context.ProductionBuilds
                .Include(pb => pb.MasterPart)
                .Include(pb => pb.Machine)
                .Include(pb => pb.PowderConsumptions)
                .FirstOrDefaultAsync(pb => pb.Id == buildId);

            if (build == null) return null;

            return new ProductionBuildViewModel
            {
                Id = build.Id,
                PartName = build.MasterPart?.PartName ?? "Unknown",
                PartNumber = build.MasterPart?.PartNumber ?? "",
                MachineName = build.Machine?.Name ?? "Unknown",
                Quantity = build.Quantity,
                StackLevel = build.StackLevel,
                Status = build.Status.ToString(),
                ScheduledStart = build.ScheduledStartTime,
                ActualStart = build.ActualStartTime,
                PrinterEstimatedEnd = build.PrinterEstimatedEndTime,
                ActualEnd = build.ActualEndTime,
                MaterialType = build.MaterialType ?? "",
                Priority = build.Priority,
                CustomerOrderNumber = build.CustomerOrderNumber,
                Notes = build.Notes,
                QualityNotes = build.QualityNotes,
                ActualQuantityProduced = build.ActualQuantityProduced,
                DefectiveQuantity = build.DefectiveQuantity ?? 0,
                TotalPowderUsed = build.PowderConsumptions.Sum(pc => pc.AmountUsed),
                TotalPowderCost = build.PowderConsumptions.Sum(pc => pc.CostAllocated),
                CreatedDate = build.CreatedDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving production build {BuildId}", buildId);
            return null;
        }
    }

    public async Task<List<ProductionBuildSummaryViewModel>> GetActiveProductionBuildsAsync()
    {
        try
        {
            return await _context.ProductionBuilds
                .Where(pb => pb.Status == ProductionBuildStatus.InProgress || 
                            pb.Status == ProductionBuildStatus.Scheduled)
                .Include(pb => pb.MasterPart)
                .Include(pb => pb.Machine)
                .Select(pb => new ProductionBuildSummaryViewModel
                {
                    Id = pb.Id,
                    PartName = pb.MasterPart!.PartName,
                    PartNumber = pb.MasterPart.PartNumber,
                    MachineName = pb.Machine!.Name,
                    Quantity = pb.Quantity,
                    Status = pb.Status.ToString(),
                    ScheduledStart = pb.ScheduledStartTime,
                    ActualStart = pb.ActualStartTime,
                    PrinterEstimatedEnd = pb.PrinterEstimatedEndTime,
                    Progress = CalculateProgress(pb)
                })
                .OrderBy(pb => pb.ScheduledStart)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active production builds");
            return new List<ProductionBuildSummaryViewModel>();
        }
    }

    public async Task<List<ProductionBuildSummaryViewModel>> GetRecentProductionBuildsAsync(int days = 7)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            return await _context.ProductionBuilds
                .Where(pb => pb.CreatedDate >= cutoffDate)
                .Include(pb => pb.MasterPart)
                .Include(pb => pb.Machine)
                .Select(pb => new ProductionBuildSummaryViewModel
                {
                    Id = pb.Id,
                    PartName = pb.MasterPart!.PartName,
                    PartNumber = pb.MasterPart.PartNumber,
                    MachineName = pb.Machine!.Name,
                    Quantity = pb.Quantity,
                    Status = pb.Status.ToString(),
                    ScheduledStart = pb.ScheduledStartTime,
                    ActualStart = pb.ActualStartTime,
                    PrinterEstimatedEnd = pb.PrinterEstimatedEndTime,
                    ActualEnd = pb.ActualEndTime,
                    CreatedDate = pb.CreatedDate
                })
                .OrderByDescending(pb => pb.CreatedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent production builds");
            return new List<ProductionBuildSummaryViewModel>();
        }
    }

    public async Task<ProductionMetricsViewModel> GetProductionMetricsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var builds = await _context.ProductionBuilds
                .Where(pb => pb.CreatedDate >= startDate && pb.CreatedDate <= endDate)
                .Include(pb => pb.MasterPart)
                .ToListAsync();

            var completedBuilds = builds.Where(pb => pb.Status == ProductionBuildStatus.Completed).ToList();

            return new ProductionMetricsViewModel
            {
                TotalBuilds = builds.Count,
                CompletedBuilds = completedBuilds.Count,
                InProgressBuilds = builds.Count(pb => pb.Status == ProductionBuildStatus.InProgress),
                ScheduledBuilds = builds.Count(pb => pb.Status == ProductionBuildStatus.Scheduled),
                TotalPartsProduced = completedBuilds.Sum(pb => pb.ActualQuantityProduced ?? pb.Quantity),
                AverageAccuracy = CalculateAverageAccuracy(completedBuilds),
                TotalPowderUsed = await GetTotalPowderUsedAsync(startDate.Value, endDate.Value),
                StartDate = startDate.Value,
                EndDate = endDate.Value
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating production metrics");
            return new ProductionMetricsViewModel();
        }
    }

    // === UPDATE METHODS ===

    public async Task<(bool Success, string Message)> UpdateProductionBuildAsync(int buildId, UpdateProductionBuildDto dto)
    {
        try
        {
            var build = await _context.ProductionBuilds.FindAsync(buildId);
            if (build == null)
            {
                return (false, "Production build not found");
            }

            // Only allow updates for scheduled builds
            if (build.Status != ProductionBuildStatus.Scheduled)
            {
                return (false, $"Cannot update build in status: {build.Status}");
            }

            build.Quantity = dto.Quantity;
            build.StackLevel = dto.StackLevel;
            build.ScheduledStartTime = dto.ScheduledStartTime;
            build.PrinterEstimatedEndTime = dto.PrinterEstimatedEndTime;
            build.Priority = dto.Priority;
            build.Notes = dto.Notes;
            build.CustomerOrderNumber = dto.CustomerOrderNumber;
            build.MaterialType = dto.MaterialType;
            build.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated production build {BuildId}", buildId);
            return (true, "Production build updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating production build {BuildId}", buildId);
            return (false, $"Error updating production build: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> CancelProductionBuildAsync(int buildId, string userId, string reason)
    {
        try
        {
            var build = await _context.ProductionBuilds
                .Include(pb => pb.Machine)
                .FirstOrDefaultAsync(pb => pb.Id == buildId);

            if (build == null)
            {
                return (false, "Production build not found");
            }

            if (build.Status == ProductionBuildStatus.Completed || build.Status == ProductionBuildStatus.Cancelled)
            {
                return (false, $"Cannot cancel build in status: {build.Status}");
            }

            build.Status = ProductionBuildStatus.Cancelled;
            build.CancelledDate = DateTime.UtcNow;
            build.CancelledByUserId = userId;
            build.CancellationReason = reason;

            // Free up machine if it was running
            if (build.Machine != null && build.Machine.CurrentJobId == buildId)
            {
                build.Machine.Status = MachineStatus.Idle;
                build.Machine.CurrentJobId = null;
                build.Machine.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cancelled production build {BuildId} by user {UserId}: {Reason}", 
                buildId, userId, reason);
            return (true, "Production build cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling production build {BuildId}", buildId);
            return (false, $"Error cancelling production build: {ex.Message}");
        }
    }

    // === DELETE METHODS ===

    public async Task<(bool Success, string Message)> DeleteProductionBuildAsync(int buildId)
    {
        try
        {
            var build = await _context.ProductionBuilds
                .Include(pb => pb.PowderConsumptions)
                .FirstOrDefaultAsync(pb => pb.Id == buildId);

            if (build == null)
            {
                return (false, "Production build not found");
            }

            // Only allow deletion of scheduled or cancelled builds
            if (build.Status == ProductionBuildStatus.InProgress || build.Status == ProductionBuildStatus.Completed)
            {
                return (false, $"Cannot delete build in status: {build.Status}");
            }

            // Remove related records
            _context.PowderConsumption.RemoveRange(build.PowderConsumptions);
            _context.ProductionBuilds.Remove(build);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted production build {BuildId}", buildId);
            return (true, "Production build deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting production build {BuildId}", buildId);
            return (false, $"Error deleting production build: {ex.Message}");
        }
    }

    // === HELPER METHODS ===

    private async Task UpdateMachineAccuracyTrackingAsync(ProductionBuild build)
    {
        if (build.PrinterEstimatedEndTime.HasValue && 
            build.ActualStartTime.HasValue && 
            build.ActualEndTime.HasValue &&
            build.MasterPart != null)
        {
            var estimatedDuration = build.PrinterEstimatedEndTime.Value - build.ActualStartTime.Value;
            var actualDuration = build.ActualEndTime.Value - build.ActualStartTime.Value;

            var estimatedMinutes = (int)estimatedDuration.TotalMinutes;
            var actualMinutes = (int)actualDuration.TotalMinutes;

            build.MasterPart.LastPrinterEstimateMinutes = estimatedMinutes;
            build.MasterPart.LastActualDurationMinutes = actualMinutes;

            if (estimatedMinutes > 0)
            {
                var accuracyPercent = (decimal)actualMinutes / estimatedMinutes * 100;

                if (build.MasterPart.AveragePrinterAccuracyPercent == null)
                {
                    build.MasterPart.AveragePrinterAccuracyPercent = accuracyPercent;
                }
                else
                {
                    // Weighted average favoring recent builds
                    build.MasterPart.AveragePrinterAccuracyPercent = 
                        (build.MasterPart.AveragePrinterAccuracyPercent * 0.7m) + (accuracyPercent * 0.3m);
                }

                build.MasterPart.TotalBuildsTracked++;
            }
        }
    }

    private int CalculateProgress(ProductionBuild build)
    {
        if (!build.ActualStartTime.HasValue) return 0;
        if (build.Status == ProductionBuildStatus.Completed) return 100;
        if (!build.PrinterEstimatedEndTime.HasValue) return 50;

        var totalDuration = build.PrinterEstimatedEndTime.Value - build.ActualStartTime.Value;
        var elapsed = DateTime.UtcNow - build.ActualStartTime.Value;

        if (totalDuration.TotalMinutes <= 0) return 50;

        var progress = (int)Math.Min(100, (elapsed.TotalMinutes / totalDuration.TotalMinutes) * 100);
        return Math.Max(0, progress);
    }

    private decimal CalculateAverageAccuracy(List<ProductionBuild> completedBuilds)
    {
        var buildsWithEstimates = completedBuilds.Where(b => 
            b.PrinterEstimatedEndTime.HasValue && 
            b.ActualStartTime.HasValue && 
            b.ActualEndTime.HasValue).ToList();

        if (!buildsWithEstimates.Any()) return 0;

        var accuracies = buildsWithEstimates.Select(b =>
        {
            var estimated = (b.PrinterEstimatedEndTime!.Value - b.ActualStartTime!.Value).TotalMinutes;
            var actual = (b.ActualEndTime!.Value - b.ActualStartTime!.Value).TotalMinutes;
            return estimated > 0 ? (decimal)(actual / estimated * 100) : 100m;
        });

        return accuracies.Average();
    }

    private async Task<decimal> GetTotalPowderUsedAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.PowderConsumption
            .Where(pc => pc.UsageDate >= startDate && pc.UsageDate <= endDate)
            .SumAsync(pc => pc.AmountUsed);
    }
}
```

### Step 2: Create PowderInventoryService

#### 2A: Create New PowderInventoryService
**File: `Services/PowderInventoryService.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.ViewModels.PowderInventory;

namespace OpCentrix.Services
{
    public class PowderInventoryService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PowderInventoryService> _logger;

        public PowderInventoryService(SchedulerContext context, ILogger<PowderInventoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // === STOCK MANAGEMENT ===

        public async Task<List<PowderStockViewModel>> GetAllPowderStockAsync()
        {
            try
            {
                return await _context.PowderStock
                    .Where(ps => ps.IsActive)
                    .Select(ps => new PowderStockViewModel
                    {
                        Id = ps.Id,
                        MaterialType = ps.MaterialType,
                        CurrentStock = ps.CurrentStock,
                        ReorderPoint = ps.ReorderPoint,
                        AlertThreshold = ps.AlertThreshold,
                        CostPerKg = ps.CostPerKg,
                        LastRestocked = ps.LastRestocked,
                        IsLowStock = ps.IsLowStock,
                        StockValue = ps.StockValue,
                        DisplayName = ps.DisplayName
                    })
                    .OrderBy(ps => ps.MaterialType)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving powder stock");
                return new List<PowderStockViewModel>();
            }
        }

        public async Task<List<PowderStockSummaryViewModel>> GetLowStockAlertsAsync()
        {
            try
            {
                return await _context.PowderStock
                    .Where(ps => ps.IsActive && ps.IsLowStock)
                    .Select(ps => new PowderStockSummaryViewModel
                    {
                        MaterialType = ps.MaterialType,
                        CurrentStock = ps.CurrentStock,
                        ReorderPoint = ps.ReorderPoint,
                        IsLowStock = ps.IsLowStock
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low stock alerts");
                return new List<PowderStockSummaryViewModel>();
            }
        }

        public async Task<(bool Success, string Message)> UpdateStockAsync(int stockId, decimal newAmount, string reason)
        {
            try
            {
                var stock = await _context.PowderStock.FindAsync(stockId);
                if (stock == null)
                {
                    return (false, "Powder stock not found");
                }

                var oldAmount = stock.CurrentStock;
                stock.CurrentStock = newAmount;
                stock.LastUpdated = DateTime.UtcNow;

                if (newAmount > oldAmount)
                {
                    stock.LastRestocked = DateTime.UtcNow;
                }

                // Update low stock alert
                stock.LowStockAlert = stock.IsLowStock;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated powder stock {MaterialType}: {OldAmount} -> {NewAmount} kg. Reason: {Reason}",
                    stock.MaterialType, oldAmount, newAmount, reason);

                return (true, $"Stock updated for {stock.MaterialType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating powder stock {StockId}", stockId);
                return (false, $"Error updating stock: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> RestockAsync(int stockId, decimal addedAmount, decimal costPerKg)
        {
            try
            {
                var stock = await _context.PowderStock.FindAsync(stockId);
                if (stock == null)
                {
                    return (false, "Powder stock not found");
                }

                stock.CurrentStock += addedAmount;
                stock.CostPerKg = costPerKg; // Update to latest cost
                stock.LastRestocked = DateTime.UtcNow;
                stock.LastUpdated = DateTime.UtcNow;
                stock.LowStockAlert = false; // Clear alert after restocking

                await _context.SaveChangesAsync();

                _logger.LogInformation("Restocked {MaterialType}: +{Amount} kg at {Cost}/kg",
                    stock.MaterialType, addedAmount, costPerKg);

                return (true, $"Restocked {addedAmount:F1} kg of {stock.MaterialType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restocking powder {StockId}", stockId);
                return (false, $"Error restocking: {ex.Message}");
            }
        }

        // === CONSUMPTION TRACKING ===

        public async Task<(bool Success, string Message)> RecordConsumptionAsync(RecordConsumptionDto dto)
        {
            try
            {
                // Find powder stock
                var stock = await _context.PowderStock
                    .FirstOrDefaultAsync(ps => ps.MaterialType == dto.MaterialType && ps.IsActive);

                if (stock == null)
                {
                    return (false, $"Powder stock not found for material: {dto.MaterialType}");
                }

                // Check if enough stock
                if (stock.CurrentStock < dto.AmountUsed)
                {
                    return (false, $"Insufficient stock. Available: {stock.CurrentStock:F1} kg, Requested: {dto.AmountUsed:F1} kg");
                }

                // Create consumption record
                var consumption = new PowderConsumption
                {
                    ProductionBuildId = dto.ProductionBuildId,
                    MaterialType = dto.MaterialType,
                    AmountUsed = dto.AmountUsed,
                    CostAllocated = dto.AmountUsed * stock.CostPerKg,
                    UsageDate = DateTime.UtcNow
                };

                _context.PowderConsumption.Add(consumption);

                // Deduct from stock
                stock.CurrentStock -= dto.AmountUsed;
                stock.LastUpdated = DateTime.UtcNow;

                // Check if low stock alert should be triggered
                if (stock.IsLowStock && !stock.LowStockAlert)
                {
                    stock.LowStockAlert = true;
                    _logger.LogWarning("Low stock alert triggered for {MaterialType}: {CurrentStock} kg remaining",
                        stock.MaterialType, stock.CurrentStock);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Recorded powder consumption: {Amount} kg of {Material} for build {BuildId}",
                    dto.AmountUsed, dto.MaterialType, dto.ProductionBuildId);

                return (true, $"Recorded consumption of {dto.AmountUsed:F1} kg {dto.MaterialType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording powder consumption for build {BuildId}", dto.ProductionBuildId);
                return (false, $"Error recording consumption: {ex.Message}");
            }
        }

        public async Task<List<PowderConsumptionViewModel>> GetConsumptionHistoryAsync(int days = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days);

                return await _context.PowderConsumption
                    .Where(pc => pc.UsageDate >= cutoffDate)
                    .Include(pc => pc.ProductionBuild)
                        .ThenInclude(pb => pb!.MasterPart)
                    .Select(pc => new PowderConsumptionViewModel
                    {
                        Id = pc.Id,
                        ProductionBuildId = pc.ProductionBuildId,
                        PartName = pc.ProductionBuild!.MasterPart!.PartName,
                        PartNumber = pc.ProductionBuild.MasterPart.PartNumber,
                        MaterialType = pc.MaterialType,
                        AmountUsed = pc.AmountUsed,
                        CostAllocated = pc.CostAllocated,
                        UsageDate = pc.UsageDate
                    })
                    .OrderByDescending(pc => pc.UsageDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving powder consumption history");
                return new List<PowderConsumptionViewModel>();
            }
        }

        public async Task<PowderUsageReportViewModel> GetUsageReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var consumptions = await _context.PowderConsumption
                    .Where(pc => pc.UsageDate >= startDate && pc.UsageDate <= endDate)
                    .ToListAsync();

                var materialTotals = consumptions
                    .GroupBy(pc => pc.MaterialType)
                    .Select(g => new MaterialUsageViewModel
                    {
                        MaterialType = g.Key,
                        TotalUsed = g.Sum(pc => pc.AmountUsed),
                        TotalCost = g.Sum(pc => pc.CostAllocated),
                        UsageCount = g.Count()
                    })
                    .OrderByDescending(m => m.TotalUsed)
                    .ToList();

                return new PowderUsageReportViewModel
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalConsumptions = consumptions.Count,
                    TotalAmountUsed = consumptions.Sum(pc => pc.AmountUsed),
                    TotalCost = consumptions.Sum(pc => pc.CostAllocated),
                    MaterialTotals = materialTotals
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating powder usage report");
                return new PowderUsageReportViewModel();
            }
        }

        // === ADMIN METHODS ===

        public async Task<(bool Success, string Message)> AddNewMaterialAsync(AddMaterialDto dto)
        {
            try
            {
                // Check if material already exists
                var exists = await _context.PowderStock
                    .AnyAsync(ps => ps.MaterialType == dto.MaterialType);

                if (exists)
                {
                    return (false, "Material type already exists");
                }

                var stock = new PowderStock
                {
                    MaterialType = dto.MaterialType,
                    CurrentStock = dto.InitialStock,
                    ReorderPoint = dto.ReorderPoint,
                    AlertThreshold = dto.AlertThreshold,
                    CostPerKg = dto.CostPerKg,
                    LastRestocked = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                    IsActive = true
                };

                _context.PowderStock.Add(stock);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added new powder material: {MaterialType} with {InitialStock} kg",
                    dto.MaterialType, dto.InitialStock);

                return (true, $"Added new material: {dto.MaterialType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new powder material: {MaterialType}", dto.MaterialType);
                return (false, $"Error adding material: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeactivateMaterialAsync(int stockId)
        {
            try
            {
                var stock = await _context.PowderStock.FindAsync(stockId);
                if (stock == null)
                {
                    return (false, "Powder stock not found");
                }

                stock.IsActive = false;
                stock.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deactivated powder material: {MaterialType}", stock.MaterialType);
                return (true, $"Deactivated material: {stock.MaterialType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating powder material {StockId}", stockId);
                return (false, $"Error deactivating material: {ex.Message}");
            }
        }
    }
}
```

### Step 3: Create Required ViewModels and DTOs

#### 3A: Create ProductionBuild ViewModels
**File: `ViewModels/ProductionBuild/ProductionBuildViewModels.cs`**

```csharp
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.ViewModels.ProductionBuild
{
    public class CreateProductionBuildDto
    {
        [Required]
        public int MasterPartId { get; set; }
        
        [Required]
        public string MachineId { get; set; } = string.Empty;
        
        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;
        
        [Range(1, 10)]
        public int StackLevel { get; set; } = 1;
        
        [Required]
        public DateTime ScheduledStartTime { get; set; }
        
        public DateTime? PrinterEstimatedEndTime { get; set; }
        
        [Range(1, 5)]
        public int Priority { get; set; } = 3;
        
        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
        public string? CustomerOrderNumber { get; set; }
        public string? MaterialType { get; set; }
    }

    public class UpdateProductionBuildDto
    {
        [Range(1, 1000)]
        public int Quantity { get; set; }
        
        [Range(1, 10)]
        public int StackLevel { get; set; }
        
        [Required]
        public DateTime ScheduledStartTime { get; set; }
        
        public DateTime? PrinterEstimatedEndTime { get; set; }
        
        [Range(1, 5)]
        public int Priority { get; set; }
        
        public string? Notes { get; set; }
        public string? CustomerOrderNumber { get; set; }
        public string? MaterialType { get; set; }
    }

    public class CompleteProductionBuildDto
    {
        public int? ActualQuantityProduced { get; set; }
        public int? DefectiveQuantity { get; set; }
        public string? QualityNotes { get; set; }
    }

    public class ProductionBuildViewModel
    {
        public int Id { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int StackLevel { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ScheduledStart { get; set; }
        public DateTime? ActualStart { get; set; }
        public DateTime? PrinterEstimatedEnd { get; set; }
        public DateTime? ActualEnd { get; set; }
        public string MaterialType { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string? CustomerOrderNumber { get; set; }
        public string? Notes { get; set; }
        public string? QualityNotes { get; set; }
        public int? ActualQuantityProduced { get; set; }
        public int DefectiveQuantity { get; set; }
        public decimal TotalPowderUsed { get; set; }
        public decimal TotalPowderCost { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Display helpers
        public string StatusClass => Status switch
        {
            "Completed" => "success",
            "InProgress" => "primary",
            "Scheduled" => "info",
            "Cancelled" => "danger",
            _ => "secondary"
        };
        
        public string StatusIcon => Status switch
        {
            "Completed" => "fa-check-circle",
            "InProgress" => "fa-spinner fa-spin",
            "Scheduled" => "fa-clock",
            "Cancelled" => "fa-times-circle",
            _ => "fa-question-circle"
        };
    }

    public class ProductionBuildSummaryViewModel
    {
        public int Id { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ScheduledStart { get; set; }
        public DateTime? ActualStart { get; set; }
        public DateTime? PrinterEstimatedEnd { get; set; }
        public DateTime? ActualEnd { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Progress { get; set; }
    }

    public class ProductionMetricsViewModel
    {
        public int TotalBuilds { get; set; }
        public int CompletedBuilds { get; set; }
        public int InProgressBuilds { get; set; }
        public int ScheduledBuilds { get; set; }
        public int TotalPartsProduced { get; set; }
        public decimal AverageAccuracy { get; set; }
        public decimal TotalPowderUsed { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public decimal CompletionRate => TotalBuilds > 0 ? (decimal)CompletedBuilds / TotalBuilds * 100 : 0;
    }
}
```

#### 3B: Create PowderInventory ViewModels
**File: `ViewModels/PowderInventory/PowderInventoryViewModels.cs`**

```csharp
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.ViewModels.PowderInventory
{
    public class PowderStockViewModel
    {
        public int Id { get; set; }
        public string MaterialType { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal ReorderPoint { get; set; }
        public decimal AlertThreshold { get; set; }
        public decimal CostPerKg { get; set; }
        public DateTime? LastRestocked { get; set; }
        public bool IsLowStock { get; set; }
        public decimal StockValue { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }

    public class PowderStockSummaryViewModel
    {
        public string MaterialType { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal ReorderPoint { get; set; }
        public bool IsLowStock { get; set; }
        public string StatusClass => IsLowStock ? "text-danger" : "text-success";
        public string StatusIcon => IsLowStock ? "fa-exclamation-triangle" : "fa-check-circle";
    }

    public class RecordConsumptionDto
    {
        [Required]
        public int ProductionBuildId { get; set; }
        
        [Required]
        public string MaterialType { get; set; } = string.Empty;
        
        [Range(0.1, 100)]
        public decimal AmountUsed { get; set; }
    }

    public class PowderConsumptionViewModel
    {
        public int Id { get; set; }
        public int ProductionBuildId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string MaterialType { get; set; } = string.Empty;
        public decimal AmountUsed { get; set; }
        public decimal CostAllocated { get; set; }
        public DateTime UsageDate { get; set; }
    }

    public class PowderUsageReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalConsumptions { get; set; }
        public decimal TotalAmountUsed { get; set; }
        public decimal TotalCost { get; set; }
        public List<MaterialUsageViewModel> MaterialTotals { get; set; } = new();
    }

    public class MaterialUsageViewModel
    {
        public string MaterialType { get; set; } = string.Empty;
        public decimal TotalUsed { get; set; }
        public decimal TotalCost { get; set; }
        public int UsageCount { get; set; }
    }

    public class AddMaterialDto
    {
        [Required]
        [StringLength(100)]
        public string MaterialType { get; set; } = string.Empty;
        
        [Range(0, 1000)]
        public decimal InitialStock { get; set; } = 0;
        
        [Range(0, 100)]
        public decimal ReorderPoint { get; set; } = 10;
        
        [Range(0.1, 1)]
        public decimal AlertThreshold { get; set; } = 0.25m;
        
        [Range(0, 1000)]
        public decimal CostPerKg { get; set; } = 0;
    }
}
```

### Step 4: Register Services in DI Container

#### 4A: Update Program.cs
**File: `Program.cs`**

```csharp
// Add these service registrations in Program.cs

// Register the new services
builder.Services.AddScoped<ProductionBuildService>();
builder.Services.AddScoped<PowderInventoryService>();

// Ensure PrintTrackingService is registered with the new dependency
builder.Services.AddScoped<PrintTrackingService>();
```

---

## Step 5: Testing & Validation

### 5A: Test ProductionBuildService Methods

1. **Test Create**: Create new production build
2. **Test Read**: Retrieve production build data  
3. **Test Update**: Modify scheduled build
4. **Test Start**: Start production build
5. **Test Complete**: Complete with accuracy tracking
6. **Test Cancel**: Cancel build and free machine
7. **Test Metrics**: Generate production metrics

### 5B: Test PowderInventoryService

1. **Test Stock Management**: Add/update powder stock
2. **Test Consumption**: Record powder usage
3. **Test Alerts**: Verify low stock alerts
4. **Test Reporting**: Generate usage reports

### 5C: Integration Testing

1. **Test PrintTracking Integration**: Verify services work together
2. **Test Dashboard Data**: Confirm metrics display correctly
3. **Test Machine Accuracy**: Verify accuracy calculations

---

## Success Criteria

- ? **No NotImplementedException errors** - all service methods implemented
- ? **ProductionBuild CRUD works** - full create, read, update, delete functionality
- ? **Powder inventory functional** - stock tracking and consumption recording
- ? **Machine accuracy tracking** - printer vs actual time analysis
- ? **Dashboard data loads** - all metrics and summaries display correctly
- ? **Service integration complete** - PrintTracking uses ProductionBuild services
- ? **Error handling robust** - proper logging and user-friendly error messages

## Next Steps

After completing this plan:
1. Test all service methods thoroughly
2. Verify dashboard displays correct data
3. Check that powder inventory tracking works
4. Move to **Plan 6: Remove Calculated Time Logic**

---

**Status: READY FOR IMPLEMENTATION**  
**Next Plan: 06-Remove-Calculated-Time-Logic.md**