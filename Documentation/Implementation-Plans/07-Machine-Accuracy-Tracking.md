# Implementation Plan 7: Machine Accuracy Tracking

**Priority: MEDIUM**  
**Estimated Time: 2 days**  
**Dependencies: Plans 1-6 must be completed first**

## Overview

Implement comprehensive machine accuracy tracking that analyzes printer estimates vs actual build times. This provides valuable intelligence to operators without making predictions - just showing how accurate the printers are.

## Current State

**What We Have:**
- ? Basic accuracy tracking in ProductionBuild completion
- ? Simple accuracy fields in MasterPart model
- ? Basic display in MasterPart forms

**What We Need:**
- ? Detailed accuracy analysis by machine, part, and stack level
- ? Historical accuracy trends over time  
- ? Machine performance comparison reports
- ? Accuracy alerts for consistently inaccurate estimates

## Solution: Comprehensive Machine Intelligence

Build a system that tracks and analyzes machine accuracy without making predictions - just providing intelligence.

---

## Step-by-Step Implementation

### Step 1: Create BuildTimeHistory Table

#### 1A: Add New Database Table
**File: `Data/Migrations/AddBuildTimeHistoryTable.sql`**

```sql
-- Add comprehensive build time tracking table
CREATE TABLE IF NOT EXISTS "BuildTimeHistory" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BuildTimeHistory" PRIMARY KEY AUTOINCREMENT,
    "ProductionBuildId" INTEGER NOT NULL,
    "MasterPartId" INTEGER NOT NULL,
    "MachineId" TEXT NOT NULL,
    "StackLevel" INTEGER NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "MaterialType" TEXT NULL,
    "PrinterEstimatedMinutes" INTEGER NULL,
    "ActualDurationMinutes" INTEGER NULL,
    "AccuracyPercent" REAL NULL,
    "CompletedDate" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    
    CONSTRAINT "FK_BuildTimeHistory_ProductionBuild" 
        FOREIGN KEY ("ProductionBuildId") REFERENCES "ProductionBuilds" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_BuildTimeHistory_MasterPart" 
        FOREIGN KEY ("MasterPartId") REFERENCES "MasterParts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_BuildTimeHistory_Machine" 
        FOREIGN KEY ("MachineId") REFERENCES "Machines" ("Id") ON DELETE CASCADE
);

-- Create indexes for performance
CREATE INDEX "IX_BuildTimeHistory_ProductionBuildId" ON "BuildTimeHistory" ("ProductionBuildId");
CREATE INDEX "IX_BuildTimeHistory_MasterPartId" ON "BuildTimeHistory" ("MasterPartId");
CREATE INDEX "IX_BuildTimeHistory_MachineId" ON "BuildTimeHistory" ("MachineId");
CREATE INDEX "IX_BuildTimeHistory_CompletedDate" ON "BuildTimeHistory" ("CompletedDate");
CREATE INDEX "IX_BuildTimeHistory_StackLevel" ON "BuildTimeHistory" ("StackLevel");
CREATE INDEX "IX_BuildTimeHistory_AccuracyPercent" ON "BuildTimeHistory" ("AccuracyPercent");

-- Verify table creation
SELECT 'BuildTimeHistory table created successfully' as Status;
```

#### 1B: Add BuildTimeHistory Model
**File: `Models/BuildTimeHistory.cs`**

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    public class BuildTimeHistory
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProductionBuildId { get; set; }
        
        [Required]
        public int MasterPartId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string MachineId { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 10)]
        public int StackLevel { get; set; }
        
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }
        
        [StringLength(100)]
        public string? MaterialType { get; set; }
        
        public int? PrinterEstimatedMinutes { get; set; }
        
        public int? ActualDurationMinutes { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? AccuracyPercent { get; set; }
        
        [Required]
        public DateTime CompletedDate { get; set; }
        
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ProductionBuild? ProductionBuild { get; set; }
        public virtual MasterPart? MasterPart { get; set; }
        public virtual Machine? Machine { get; set; }
        
        // Computed properties
        [NotMapped]
        public string AccuracyStatus
        {
            get
            {
                if (!AccuracyPercent.HasValue) return "No Data";
                var accuracy = AccuracyPercent.Value;
                return accuracy >= 90 && accuracy <= 110 ? "Excellent" :
                       accuracy >= 80 && accuracy <= 120 ? "Good" : "Poor";
            }
        }
        
        [NotMapped]
        public string AccuracyClass
        {
            get
            {
                if (!AccuracyPercent.HasValue) return "text-muted";
                var accuracy = AccuracyPercent.Value;
                return accuracy >= 90 && accuracy <= 110 ? "text-success" :
                       accuracy >= 80 && accuracy <= 120 ? "text-warning" : "text-danger";
            }
        }
        
        [NotMapped]
        public string PrinterEstimateDisplay => PrinterEstimatedMinutes.HasValue 
            ? $"{PrinterEstimatedMinutes / 60}h {PrinterEstimatedMinutes % 60}m" 
            : "N/A";
            
        [NotMapped]
        public string ActualDurationDisplay => ActualDurationMinutes.HasValue 
            ? $"{ActualDurationMinutes / 60}h {ActualDurationMinutes % 60}m" 
            : "N/A";
    }
}
```

#### 1C: Add to SchedulerContext
**File: `Data/SchedulerContext.cs`**

```csharp
public class SchedulerContext : DbContext
{
    // ... existing DbSets ...
    
    public DbSet<BuildTimeHistory> BuildTimeHistory { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ... existing configurations ...
        
        // Configure BuildTimeHistory
        modelBuilder.Entity<BuildTimeHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccuracyPercent).HasPrecision(5, 2);
            
            entity.HasOne<ProductionBuild>()
                  .WithMany()
                  .HasForeignKey(e => e.ProductionBuildId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne<MasterPart>()
                  .WithMany()
                  .HasForeignKey(e => e.MasterPartId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne<Machine>()
                  .WithMany()
                  .HasForeignKey(e => e.MachineId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.CompletedDate);
            entity.HasIndex(e => e.AccuracyPercent);
            entity.HasIndex(e => new { e.MasterPartId, e.MachineId, e.StackLevel });
        });
    }
}
```

### Step 2: Create MachineAccuracyService

#### 2A: Create New Service
**File: `Services/MachineAccuracyService.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.ViewModels.MachineAccuracy;

namespace OpCentrix.Services
{
    public class MachineAccuracyService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<MachineAccuracyService> _logger;

        public MachineAccuracyService(SchedulerContext context, ILogger<MachineAccuracyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // === RECORD BUILD TIME HISTORY ===
        
        public async Task<(bool Success, string Message)> RecordBuildTimeHistoryAsync(ProductionBuild completedBuild)
        {
            try
            {
                // Only record if we have both printer estimate and actual times
                if (!completedBuild.PrinterEstimatedEndTime.HasValue || 
                    !completedBuild.ActualStartTime.HasValue || 
                    !completedBuild.ActualEndTime.HasValue)
                {
                    return (true, "No time history recorded - missing time data");
                }

                var estimatedMinutes = (int)(completedBuild.PrinterEstimatedEndTime.Value - completedBuild.ActualStartTime.Value).TotalMinutes;
                var actualMinutes = (int)(completedBuild.ActualEndTime.Value - completedBuild.ActualStartTime.Value).TotalMinutes;
                
                var accuracyPercent = estimatedMinutes > 0 ? (decimal)actualMinutes / estimatedMinutes * 100 : 100m;

                var history = new BuildTimeHistory
                {
                    ProductionBuildId = completedBuild.Id,
                    MasterPartId = completedBuild.MasterPartId,
                    MachineId = completedBuild.MachineId,
                    StackLevel = completedBuild.StackLevel,
                    Quantity = completedBuild.Quantity,
                    MaterialType = completedBuild.MaterialType,
                    PrinterEstimatedMinutes = estimatedMinutes,
                    ActualDurationMinutes = actualMinutes,
                    AccuracyPercent = accuracyPercent,
                    CompletedDate = completedBuild.ActualEndTime.Value
                };

                _context.BuildTimeHistory.Add(history);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Recorded build time history for build {BuildId}: {Estimated}min vs {Actual}min ({Accuracy:F1}%)",
                    completedBuild.Id, estimatedMinutes, actualMinutes, accuracyPercent);

                return (true, "Build time history recorded");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording build time history for build {BuildId}", completedBuild.Id);
                return (false, $"Error recording history: {ex.Message}");
            }
        }

        // === MACHINE ACCURACY ANALYSIS ===
        
        public async Task<List<MachineAccuracyViewModel>> GetMachineAccuracySummaryAsync(int days = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days);

                var machineData = await _context.BuildTimeHistory
                    .Where(bth => bth.CompletedDate >= cutoffDate && bth.AccuracyPercent.HasValue)
                    .Include(bth => bth.Machine)
                    .GroupBy(bth => new { bth.MachineId, bth.Machine!.Name })
                    .Select(g => new MachineAccuracyViewModel
                    {
                        MachineId = g.Key.MachineId,
                        MachineName = g.Key.Name,
                        TotalBuilds = g.Count(),
                        AverageAccuracy = g.Average(bth => bth.AccuracyPercent!.Value),
                        BestAccuracy = g.Max(bth => bth.AccuracyPercent!.Value),
                        WorstAccuracy = g.Min(bth => bth.AccuracyPercent!.Value),
                        ExcellentBuilds = g.Count(bth => bth.AccuracyPercent >= 90 && bth.AccuracyPercent <= 110),
                        GoodBuilds = g.Count(bth => bth.AccuracyPercent >= 80 && bth.AccuracyPercent <= 120),
                        PoorBuilds = g.Count(bth => bth.AccuracyPercent < 80 || bth.AccuracyPercent > 120),
                        TotalEstimatedMinutes = g.Sum(bth => bth.PrinterEstimatedMinutes ?? 0),
                        TotalActualMinutes = g.Sum(bth => bth.ActualDurationMinutes ?? 0)
                    })
                    .OrderByDescending(m => m.AverageAccuracy)
                    .ToListAsync();

                return machineData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving machine accuracy summary");
                return new List<MachineAccuracyViewModel>();
            }
        }

        public async Task<List<PartAccuracyViewModel>> GetPartAccuracySummaryAsync(int days = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days);

                var partData = await _context.BuildTimeHistory
                    .Where(bth => bth.CompletedDate >= cutoffDate && bth.AccuracyPercent.HasValue)
                    .Include(bth => bth.MasterPart)
                    .GroupBy(bth => new { bth.MasterPartId, bth.MasterPart!.PartNumber, bth.MasterPart.PartName })
                    .Select(g => new PartAccuracyViewModel
                    {
                        PartId = g.Key.MasterPartId,
                        PartNumber = g.Key.PartNumber,
                        PartName = g.Key.PartName,
                        TotalBuilds = g.Count(),
                        AverageAccuracy = g.Average(bth => bth.AccuracyPercent!.Value),
                        BestAccuracy = g.Max(bth => bth.AccuracyPercent!.Value),
                        WorstAccuracy = g.Min(bth => bth.AccuracyPercent!.Value),
                        DifferentMachines = g.Select(bth => bth.MachineId).Distinct().Count(),
                        DifferentStackLevels = g.Select(bth => bth.StackLevel).Distinct().Count(),
                        MostCommonStackLevel = g.GroupBy(bth => bth.StackLevel)
                                                .OrderByDescending(sl => sl.Count())
                                                .First().Key
                    })
                    .OrderBy(p => p.PartNumber)
                    .ToListAsync();

                return partData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving part accuracy summary");
                return new List<PartAccuracyViewModel>();
            }
        }

        public async Task<List<AccuracyTrendViewModel>> GetAccuracyTrendsAsync(string machineId, int days = 90)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days);

                var trends = await _context.BuildTimeHistory
                    .Where(bth => bth.MachineId == machineId && 
                                 bth.CompletedDate >= cutoffDate && 
                                 bth.AccuracyPercent.HasValue)
                    .OrderBy(bth => bth.CompletedDate)
                    .Select(bth => new AccuracyTrendViewModel
                    {
                        CompletedDate = bth.CompletedDate,
                        AccuracyPercent = bth.AccuracyPercent!.Value,
                        PartNumber = bth.MasterPart!.PartNumber,
                        StackLevel = bth.StackLevel,
                        PrinterEstimatedMinutes = bth.PrinterEstimatedMinutes ?? 0,
                        ActualDurationMinutes = bth.ActualDurationMinutes ?? 0
                    })
                    .ToListAsync();

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accuracy trends for machine {MachineId}", machineId);
                return new List<AccuracyTrendViewModel>();
            }
        }

        // === ACCURACY ALERTS ===
        
        public async Task<List<AccuracyAlertViewModel>> GetAccuracyAlertsAsync()
        {
            try
            {
                var alerts = new List<AccuracyAlertViewModel>();
                var cutoffDate = DateTime.UtcNow.AddDays(-14); // Last 2 weeks

                // Find machines with consistently poor accuracy
                var machineAlerts = await _context.BuildTimeHistory
                    .Where(bth => bth.CompletedDate >= cutoffDate && bth.AccuracyPercent.HasValue)
                    .GroupBy(bth => new { bth.MachineId, bth.Machine!.Name })
                    .Where(g => g.Count() >= 3) // At least 3 builds
                    .Select(g => new
                    {
                        MachineId = g.Key.MachineId,
                        MachineName = g.Key.Name,
                        AverageAccuracy = g.Average(bth => bth.AccuracyPercent!.Value),
                        BuildCount = g.Count(),
                        PoorBuilds = g.Count(bth => bth.AccuracyPercent < 80 || bth.AccuracyPercent > 120)
                    })
                    .ToListAsync();

                foreach (var machine in machineAlerts)
                {
                    // Alert if average accuracy is poor OR if >50% of builds are poor
                    if (machine.AverageAccuracy < 80 || machine.AverageAccuracy > 120 || 
                        (machine.PoorBuilds / (double)machine.BuildCount) > 0.5)
                    {
                        alerts.Add(new AccuracyAlertViewModel
                        {
                            AlertType = "Machine",
                            Subject = machine.MachineName,
                            Message = $"Machine {machine.MachineName} has poor accuracy: {machine.AverageAccuracy:F1}% average over {machine.BuildCount} builds",
                            Severity = machine.AverageAccuracy < 70 || machine.AverageAccuracy > 130 ? "High" : "Medium",
                            AlertDate = DateTime.UtcNow
                        });
                    }
                }

                // Find parts that are consistently hard to estimate
                var partAlerts = await _context.BuildTimeHistory
                    .Where(bth => bth.CompletedDate >= cutoffDate && bth.AccuracyPercent.HasValue)
                    .GroupBy(bth => new { bth.MasterPartId, bth.MasterPart!.PartNumber })
                    .Where(g => g.Count() >= 3) // At least 3 builds
                    .Select(g => new
                    {
                        PartId = g.Key.MasterPartId,
                        PartNumber = g.Key.PartNumber,
                        AverageAccuracy = g.Average(bth => bth.AccuracyPercent!.Value),
                        AccuracyVariance = g.Max(bth => bth.AccuracyPercent!.Value) - g.Min(bth => bth.AccuracyPercent!.Value),
                        BuildCount = g.Count()
                    })
                    .ToListAsync();

                foreach (var part in partAlerts)
                {
                    // Alert if accuracy is highly variable (>50% variance) or consistently poor
                    if (part.AccuracyVariance > 50 || part.AverageAccuracy < 75 || part.AverageAccuracy > 125)
                    {
                        alerts.Add(new AccuracyAlertViewModel
                        {
                            AlertType = "Part",
                            Subject = part.PartNumber,
                            Message = $"Part {part.PartNumber} has inconsistent estimates: {part.AverageAccuracy:F1}% average with {part.AccuracyVariance:F1}% variance over {part.BuildCount} builds",
                            Severity = part.AccuracyVariance > 75 ? "High" : "Medium",
                            AlertDate = DateTime.UtcNow
                        });
                    }
                }

                return alerts.OrderByDescending(a => a.Severity).ThenByDescending(a => a.AlertDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accuracy alerts");
                return new List<AccuracyAlertViewModel>();
            }
        }

        // === DETAILED ANALYSIS ===
        
        public async Task<MachineDetailedAnalysisViewModel> GetMachineDetailedAnalysisAsync(string machineId, int days = 60)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days);
                
                var machine = await _context.Machines.FindAsync(machineId);
                if (machine == null)
                {
                    return new MachineDetailedAnalysisViewModel();
                }

                var buildHistory = await _context.BuildTimeHistory
                    .Where(bth => bth.MachineId == machineId && 
                                 bth.CompletedDate >= cutoffDate && 
                                 bth.AccuracyPercent.HasValue)
                    .Include(bth => bth.MasterPart)
                    .OrderByDescending(bth => bth.CompletedDate)
                    .ToListAsync();

                if (!buildHistory.Any())
                {
                    return new MachineDetailedAnalysisViewModel
                    {
                        MachineId = machineId,
                        MachineName = machine.Name,
                        AnalysisPeriodDays = days,
                        Message = "No build history available for analysis"
                    };
                }

                var analysis = new MachineDetailedAnalysisViewModel
                {
                    MachineId = machineId,
                    MachineName = machine.Name,
                    AnalysisPeriodDays = days,
                    TotalBuilds = buildHistory.Count,
                    AverageAccuracy = buildHistory.Average(bh => bh.AccuracyPercent!.Value),
                    BestAccuracy = buildHistory.Max(bh => bh.AccuracyPercent!.Value),
                    WorstAccuracy = buildHistory.Min(bh => bh.AccuracyPercent!.Value),
                    AccuracyStandardDeviation = CalculateStandardDeviation(buildHistory.Select(bh => (double)bh.AccuracyPercent!.Value)),
                    
                    // Breakdown by stack level
                    StackLevelAnalysis = buildHistory
                        .GroupBy(bh => bh.StackLevel)
                        .Select(g => new StackLevelAccuracyViewModel
                        {
                            StackLevel = g.Key,
                            BuildCount = g.Count(),
                            AverageAccuracy = g.Average(bh => bh.AccuracyPercent!.Value),
                            BestAccuracy = g.Max(bh => bh.AccuracyPercent!.Value),
                            WorstAccuracy = g.Min(bh => bh.AccuracyPercent!.Value)
                        })
                        .OrderBy(sla => sla.StackLevel)
                        .ToList(),

                    // Recent builds
                    RecentBuilds = buildHistory
                        .Take(10)
                        .Select(bh => new BuildAccuracyViewModel
                        {
                            BuildId = bh.ProductionBuildId,
                            CompletedDate = bh.CompletedDate,
                            PartNumber = bh.MasterPart?.PartNumber ?? "Unknown",
                            StackLevel = bh.StackLevel,
                            PrinterEstimatedMinutes = bh.PrinterEstimatedMinutes ?? 0,
                            ActualDurationMinutes = bh.ActualDurationMinutes ?? 0,
                            AccuracyPercent = bh.AccuracyPercent!.Value
                        })
                        .ToList(),

                    // Trends over time
                    WeeklyTrends = buildHistory
                        .GroupBy(bh => GetWeekStart(bh.CompletedDate))
                        .Select(g => new WeeklyAccuracyViewModel
                        {
                            WeekStart = g.Key,
                            BuildCount = g.Count(),
                            AverageAccuracy = g.Average(bh => bh.AccuracyPercent!.Value)
                        })
                        .OrderBy(wt => wt.WeekStart)
                        .ToList()
                };

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing detailed analysis for machine {MachineId}", machineId);
                return new MachineDetailedAnalysisViewModel { Message = "Error performing analysis" };
            }
        }

        // === HELPER METHODS ===
        
        private double CalculateStandardDeviation(IEnumerable<double> values)
        {
            var valuesList = values.ToList();
            if (valuesList.Count < 2) return 0;

            var mean = valuesList.Average();
            var sumOfSquares = valuesList.Sum(x => Math.Pow(x - mean, 2));
            return Math.Sqrt(sumOfSquares / (valuesList.Count - 1));
        }

        private DateTime GetWeekStart(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
    }
}
```

### Step 3: Create Machine Accuracy ViewModels

#### 3A: Expand MachineAccuracy ViewModels
**File: `ViewModels/MachineAccuracy/MachineAccuracyViewModels.cs`**

```csharp
namespace OpCentrix.ViewModels.MachineAccuracy
{
    public class MachineAccuracyViewModel
    {
        public string MachineId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public int TotalBuilds { get; set; }
        public decimal AverageAccuracy { get; set; }
        public decimal BestAccuracy { get; set; }
        public decimal WorstAccuracy { get; set; }
        public int ExcellentBuilds { get; set; }
        public int GoodBuilds { get; set; }
        public int PoorBuilds { get; set; }
        public int TotalEstimatedMinutes { get; set; }
        public int TotalActualMinutes { get; set; }
        
        public decimal ExcellentPercentage => TotalBuilds > 0 ? (decimal)ExcellentBuilds / TotalBuilds * 100 : 0;
        public decimal GoodPercentage => TotalBuilds > 0 ? (decimal)GoodBuilds / TotalBuilds * 100 : 0;
        public decimal PoorPercentage => TotalBuilds > 0 ? (decimal)PoorBuilds / TotalBuilds * 100 : 0;
        
        public string AccuracyClass => AverageAccuracy >= 90 && AverageAccuracy <= 110 ? "text-success" :
                                      AverageAccuracy >= 80 && AverageAccuracy <= 120 ? "text-warning" : "text-danger";
                                      
        public string AccuracyStatus => AverageAccuracy >= 90 && AverageAccuracy <= 110 ? "Excellent" :
                                       AverageAccuracy >= 80 && AverageAccuracy <= 120 ? "Good" : "Poor";
    }

    public class PartAccuracyViewModel
    {
        public int PartId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public int TotalBuilds { get; set; }
        public decimal AverageAccuracy { get; set; }
        public decimal BestAccuracy { get; set; }
        public decimal WorstAccuracy { get; set; }
        public int DifferentMachines { get; set; }
        public int DifferentStackLevels { get; set; }
        public int MostCommonStackLevel { get; set; }
        
        public decimal AccuracyVariance => BestAccuracy - WorstAccuracy;
        public string AccuracyClass => AverageAccuracy >= 90 && AverageAccuracy <= 110 ? "text-success" :
                                      AverageAccuracy >= 80 && AverageAccuracy <= 120 ? "text-warning" : "text-danger";
    }

    public class AccuracyTrendViewModel
    {
        public DateTime CompletedDate { get; set; }
        public decimal AccuracyPercent { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public int StackLevel { get; set; }
        public int PrinterEstimatedMinutes { get; set; }
        public int ActualDurationMinutes { get; set; }
    }

    public class AccuracyAlertViewModel
    {
        public string AlertType { get; set; } = string.Empty; // "Machine" or "Part"
        public string Subject { get; set; } = string.Empty; // Machine name or Part number
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // "High", "Medium", "Low"
        public DateTime AlertDate { get; set; }
        
        public string SeverityClass => Severity switch
        {
            "High" => "text-danger",
            "Medium" => "text-warning",
            "Low" => "text-info",
            _ => "text-muted"
        };
        
        public string SeverityIcon => Severity switch
        {
            "High" => "fa-exclamation-circle",
            "Medium" => "fa-exclamation-triangle",
            "Low" => "fa-info-circle",
            _ => "fa-question-circle"
        };
    }

    public class MachineDetailedAnalysisViewModel
    {
        public string MachineId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public int AnalysisPeriodDays { get; set; }
        public int TotalBuilds { get; set; }
        public decimal AverageAccuracy { get; set; }
        public decimal BestAccuracy { get; set; }
        public decimal WorstAccuracy { get; set; }
        public double AccuracyStandardDeviation { get; set; }
        public string? Message { get; set; }
        
        public List<StackLevelAccuracyViewModel> StackLevelAnalysis { get; set; } = new();
        public List<BuildAccuracyViewModel> RecentBuilds { get; set; } = new();
        public List<WeeklyAccuracyViewModel> WeeklyTrends { get; set; } = new();
        
        public bool HasData => TotalBuilds > 0 && string.IsNullOrEmpty(Message);
    }

    public class StackLevelAccuracyViewModel
    {
        public int StackLevel { get; set; }
        public int BuildCount { get; set; }
        public decimal AverageAccuracy { get; set; }
        public decimal BestAccuracy { get; set; }
        public decimal WorstAccuracy { get; set; }
        
        public string AccuracyClass => AverageAccuracy >= 90 && AverageAccuracy <= 110 ? "text-success" :
                                      AverageAccuracy >= 80 && AverageAccuracy <= 120 ? "text-warning" : "text-danger";
    }

    public class BuildAccuracyViewModel
    {
        public int BuildId { get; set; }
        public DateTime CompletedDate { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public int StackLevel { get; set; }
        public int PrinterEstimatedMinutes { get; set; }
        public int ActualDurationMinutes { get; set; }
        public decimal AccuracyPercent { get; set; }
        
        public string AccuracyClass => AccuracyPercent >= 90 && AccuracyPercent <= 110 ? "text-success" :
                                      AccuracyPercent >= 80 && AccuracyPercent <= 120 ? "text-warning" : "text-danger";
    }

    public class WeeklyAccuracyViewModel
    {
        public DateTime WeekStart { get; set; }
        public int BuildCount { get; set; }
        public decimal AverageAccuracy { get; set; }
        
        public string WeekDisplay => WeekStart.ToString("MMM dd");
    }
}
```

### Step 4: Update ProductionBuildService to Record History

#### 4A: Integrate with Build Completion
**File: `Services/ProductionBuildService.cs`**

Update the completion method to record build time history:

```csharp
public class ProductionBuildService
{
    private readonly MachineAccuracyService _machineAccuracyService;
    
    // Add to constructor
    public ProductionBuildService(
        SchedulerContext context,
        ILogger<ProductionBuildService> logger,
        UserManager<IdentityUser> userManager,
        MachineAccuracyService machineAccuracyService)
    {
        // ... existing assignments ...
        _machineAccuracyService = machineAccuracyService;
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

            // *** NEW: Record detailed build time history ***
            var historyResult = await _machineAccuracyService.RecordBuildTimeHistoryAsync(build);
            if (!historyResult.Success)
            {
                _logger.LogWarning("Failed to record build time history for build {BuildId}: {Message}",
                    buildId, historyResult.Message);
            }

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
    
    // ... rest of existing methods ...
}
```

### Step 5: Create Machine Accuracy Dashboard Page

#### 5A: Create Accuracy Dashboard Page
**File: `Pages/Admin/MachineAccuracy.cshtml`**

```html
@page "/Admin/MachineAccuracy"
@model OpCentrix.Pages.Admin.MachineAccuracyModel
@{
    ViewData["Title"] = "Machine Accuracy Analysis";
    Layout = "~/Pages/Shared/_Layout.cshtml";
}

<div class="container-fluid">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h2 class="mb-0">
                <i class="fas fa-chart-line me-2 text-primary"></i>
                Machine Accuracy Analysis
            </h2>
            <p class="text-muted mb-0">Printer estimate vs actual build time analysis</p>
        </div>
        <div>
            <div class="btn-group" role="group">
                <button type="button" class="btn btn-outline-secondary" onclick="setDays(7)">7 Days</button>
                <button type="button" class="btn btn-outline-secondary active" onclick="setDays(30)">30 Days</button>
                <button type="button" class="btn btn-outline-secondary" onclick="setDays(90)">90 Days</button>
            </div>
        </div>
    </div>

    <!-- Accuracy Alerts -->
    @if (Model.AccuracyAlerts.Any())
    {
        <div class="row mb-4">
            <div class="col-12">
                <div class="card border-warning">
                    <div class="card-header bg-warning text-dark">
                        <h5 class="mb-0">
                            <i class="fas fa-exclamation-triangle me-2"></i>
                            Accuracy Alerts (@Model.AccuracyAlerts.Count)
                        </h5>
                    </div>
                    <div class="card-body">
                        @foreach (var alert in Model.AccuracyAlerts.Take(5))
                        {
                            <div class="alert alert-@(alert.Severity == "High" ? "danger" : "warning") alert-dismissible">
                                <i class="fas @alert.SeverityIcon me-2 @alert.SeverityClass"></i>
                                <strong>@alert.AlertType Alert:</strong> @alert.Message
                                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                            </div>
                        }
                    </div>
                </div>
            </div>
        </div>
    }

    <!-- Machine Accuracy Summary -->
    <div class="row mb-4">
        <div class="col-12">
            <div class="card">
                <div class="card-header bg-primary text-white">
                    <h5 class="mb-0">
                        <i class="fas fa-desktop me-2"></i>
                        Machine Accuracy Summary
                    </h5>
                </div>
                <div class="card-body">
                    @if (Model.MachineAccuracy.Any())
                    {
                        <div class="row">
                            @foreach (var machine in Model.MachineAccuracy)
                            {
                                <div class="col-lg-4 col-md-6 mb-4">
                                    <div class="card h-100 border-@(machine.AccuracyClass.Replace("text-", ""))">
                                        <div class="card-header">
                                            <h6 class="mb-0">
                                                <i class="fas fa-desktop me-2"></i>
                                                @machine.MachineName
                                            </h6>
                                            <span class="badge bg-@(machine.AccuracyClass.Replace("text-", ""))">
                                                @machine.AccuracyStatus
                                            </span>
                                        </div>
                                        <div class="card-body">
                                            <div class="row text-center mb-3">
                                                <div class="col-4">
                                                    <div class="h4 @machine.AccuracyClass">@machine.AverageAccuracy.ToString("F1")%</div>
                                                    <small class="text-muted">Average</small>
                                                </div>
                                                <div class="col-4">
                                                    <div class="h4 text-info">@machine.TotalBuilds</div>
                                                    <small class="text-muted">Builds</small>
                                                </div>
                                                <div class="col-4">
                                                    <div class="h4 text-success">@machine.ExcellentPercentage.ToString("F0")%</div>
                                                    <small class="text-muted">Excellent</small>
                                                </div>
                                            </div>
                                            
                                            <div class="progress mb-2" style="height: 10px;">
                                                <div class="progress-bar bg-success" style="width: @machine.ExcellentPercentage%"></div>
                                                <div class="progress-bar bg-warning" style="width: @machine.GoodPercentage%"></div>
                                                <div class="progress-bar bg-danger" style="width: @machine.PoorPercentage%"></div>
                                            </div>
                                            
                                            <div class="d-flex justify-content-between">
                                                <small class="text-success">@machine.ExcellentBuilds excellent</small>
                                                <small class="text-warning">@machine.GoodBuilds good</small>
                                                <small class="text-danger">@machine.PoorBuilds poor</small>
                                            </div>
                                            
                                            <div class="row mt-3 text-center">
                                                <div class="col-6">
                                                    <small class="text-muted">Best: @machine.BestAccuracy.ToString("F1")%</small>
                                                </div>
                                                <div class="col-6">
                                                    <small class="text-muted">Worst: @machine.WorstAccuracy.ToString("F1")%</small>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="card-footer">
                                            <a href="/Admin/MachineAccuracy/@machine.MachineId" class="btn btn-sm btn-outline-primary">
                                                <i class="fas fa-chart-area me-1"></i>Detailed Analysis
                                            </a>
                                        </div>
                                    </div>
                                </div>
                            }
                        </div>
                    }
                    else
                    {
                        <div class="text-center py-5">
                            <i class="fas fa-chart-line fa-3x text-muted mb-3"></i>
                            <h5 class="text-muted">No Machine Data Available</h5>
                            <p class="text-muted">Machine accuracy data will appear here after operators complete builds using printer estimates.</p>
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>

    <!-- Part Accuracy Summary -->
    <div class="row">
        <div class="col-12">
            <div class="card">
                <div class="card-header bg-info text-white">
                    <h5 class="mb-0">
                        <i class="fas fa-cog me-2"></i>
                        Part Accuracy Summary
                    </h5>
                </div>
                <div class="card-body">
                    @if (Model.PartAccuracy.Any())
                    {
                        <div class="table-responsive">
                            <table class="table table-hover">
                                <thead>
                                    <tr>
                                        <th>Part Number</th>
                                        <th>Part Name</th>
                                        <th class="text-center">Builds</th>
                                        <th class="text-center">Avg Accuracy</th>
                                        <th class="text-center">Variance</th>
                                        <th class="text-center">Machines</th>
                                        <th class="text-center">Common Stack</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @foreach (var part in Model.PartAccuracy)
                                    {
                                        <tr>
                                            <td class="fw-bold">@part.PartNumber</td>
                                            <td>@part.PartName</td>
                                            <td class="text-center">@part.TotalBuilds</td>
                                            <td class="text-center">
                                                <span class="@part.AccuracyClass fw-bold">@part.AverageAccuracy.ToString("F1")%</span>
                                            </td>
                                            <td class="text-center">
                                                <span class="@(part.AccuracyVariance > 30 ? "text-warning" : "text-muted")">
                                                    @part.AccuracyVariance.ToString("F1")%
                                                </span>
                                            </td>
                                            <td class="text-center">@part.DifferentMachines</td>
                                            <td class="text-center">@part.MostCommonStackLevel×</td>
                                        </tr>
                                    }
                                </tbody>
                            </table>
                        </div>
                    }
                    else
                    {
                        <div class="text-center py-4">
                            <i class="fas fa-cogs fa-2x text-muted mb-3"></i>
                            <p class="text-muted mb-0">No part accuracy data available yet.</p>
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>
</div>

<script>
function setDays(days) {
    // Update active button
    document.querySelectorAll('.btn-group .btn').forEach(btn => btn.classList.remove('active'));
    event.target.classList.add('active');
    
    // Reload page with new days parameter
    const url = new URL(window.location);
    url.searchParams.set('days', days);
    window.location.href = url.toString();
}
</script>
```

#### 5B: Create Page Model
**File: `Pages/Admin/MachineAccuracy.cshtml.cs`**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Services;
using OpCentrix.ViewModels.MachineAccuracy;

namespace OpCentrix.Pages.Admin
{
    [Authorize(Roles = "Admin,Manager")]
    public class MachineAccuracyModel : PageModel
    {
        private readonly MachineAccuracyService _machineAccuracyService;

        public MachineAccuracyModel(MachineAccuracyService machineAccuracyService)
        {
            _machineAccuracyService = machineAccuracyService;
        }

        public List<MachineAccuracyViewModel> MachineAccuracy { get; set; } = new();
        public List<PartAccuracyViewModel> PartAccuracy { get; set; } = new();
        public List<AccuracyAlertViewModel> AccuracyAlerts { get; set; } = new();
        public int Days { get; set; } = 30;

        public async Task OnGetAsync(int days = 30)
        {
            Days = days;
            
            MachineAccuracy = await _machineAccuracyService.GetMachineAccuracySummaryAsync(days);
            PartAccuracy = await _machineAccuracyService.GetPartAccuracySummaryAsync(days);
            AccuracyAlerts = await _machineAccuracyService.GetAccuracyAlertsAsync();
        }
    }
}
```

### Step 6: Register New Service and Run Migration

#### 6A: Register Service
**File: `Program.cs`**

```csharp
// Add this service registration
builder.Services.AddScoped<MachineAccuracyService>();
```

#### 6B: Run Migration
```bash
sqlite3 scheduler.db < "Data/Migrations/AddBuildTimeHistoryTable.sql"
```

---

## Step 7: Testing & Validation

### 7A: Test Build Time History Recording
1. **Complete a production build** with printer estimate
2. **Verify BuildTimeHistory record created** 
3. **Check accuracy calculation** is correct
4. **Test with different stack levels** and parts

### 7B: Test Machine Accuracy Dashboard
1. **Navigate to /Admin/MachineAccuracy**
2. **Verify machine summaries** display correctly
3. **Test different time periods** (7, 30, 90 days)
4. **Check accuracy alerts** appear for poor performers

### 7C: Test Integration with Existing System
1. **Complete builds through PrintTracking** 
2. **Verify history automatically recorded**
3. **Check MasterPart forms** show updated accuracy data
4. **Test alerts generation** for poor accuracy

---

## Success Criteria

- ? **BuildTimeHistory table created** and populated on build completion
- ? **Machine accuracy tracking works** - detailed analysis available
- ? **Dashboard shows machine intelligence** - accuracy trends and alerts
- ? **Part-specific accuracy analysis** - identifies hard-to-estimate parts
- ? **Integration seamless** - works with existing PrintTracking workflow
- ? **No prediction logic** - pure analysis and intelligence
- ? **Accuracy alerts functional** - identifies problematic machines/parts

## Next Steps

After completing this plan:
1. Test machine accuracy analysis thoroughly
2. Verify build time history recording works
3. Check that accuracy alerts are helpful
4. Move to **Plan 8: Powder Management UI**

---

**Status: READY FOR IMPLEMENTATION**  
**Next Plan: 08-Powder-Management-UI.md**