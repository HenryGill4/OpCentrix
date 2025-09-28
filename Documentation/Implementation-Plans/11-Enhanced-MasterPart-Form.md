# Implementation Plan 11: Enhanced MasterPart Form

**Priority: LOW**  
**Estimated Time: 2 days**  
**Dependencies: Plans 1-10 must be completed first**

## Overview

Create a calculation-free MasterPart form that displays historical machine accuracy, provides intelligent stacking recommendations, and integrates version control. This replaces the old calculated approach with machine-driven intelligence.

## Current State

**What We Have:**
- ? MasterPart forms with machine accuracy display (from Plan 6)
- ? Version control integration (from Plan 10)
- ? Basic MasterPart CRUD operations
- ? Machine accuracy tracking system

**What We Need:**
- ? Intelligent stacking recommendations based on success rates
- ? Historical performance analytics display
- ? Smart form validation and assistance
- ? Enhanced user experience with predictive features

## Solution: Intelligent MasterPart Management

Transform the MasterPart form into an intelligent interface that provides insights without making predictions.

---

## Step-by-Step Implementation

### Step 1: Create Part Performance Analytics Service

#### 1A: Create PartPerformanceService
**File: `Services/PartPerformanceService.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.ViewModels.PartPerformance;

namespace OpCentrix.Services
{
    public class PartPerformanceService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PartPerformanceService> _logger;

        public PartPerformanceService(SchedulerContext context, ILogger<PartPerformanceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // === STACKING ANALYSIS ===

        public async Task<StackingRecommendationsViewModel> GetStackingRecommendationsAsync(int masterPartId)
        {
            try
            {
                var masterPart = await _context.MasterParts.FindAsync(masterPartId);
                if (masterPart == null)
                {
                    return new StackingRecommendationsViewModel();
                }

                // Get historical stacking data
                var stackingHistory = await _context.BuildTimeHistory
                    .Where(bth => bth.MasterPartId == masterPartId && bth.AccuracyPercent.HasValue)
                    .GroupBy(bth => bth.StackLevel)
                    .Select(g => new StackingPerformanceViewModel
                    {
                        StackLevel = g.Key,
                        TotalBuilds = g.Count(),
                        SuccessfulBuilds = g.Count(b => b.AccuracyPercent >= 80 && b.AccuracyPercent <= 120),
                        AverageAccuracy = g.Average(b => b.AccuracyPercent!.Value),
                        BestAccuracy = g.Max(b => b.AccuracyPercent!.Value),
                        WorstAccuracy = g.Min(b => b.AccuracyPercent!.Value),
                        AverageDurationMinutes = g.Average(b => b.ActualDurationMinutes ?? 0),
                        TotalParts = g.Sum(b => b.Quantity)
                    })
                    .OrderBy(s => s.StackLevel)
                    .ToListAsync();

                var recommendations = new StackingRecommendationsViewModel
                {
                    PartId = masterPartId,
                    PartNumber = masterPart.PartNumber,
                    PartName = masterPart.PartName,
                    StackingPerformance = stackingHistory,
                    Recommendations = GenerateStackingRecommendations(masterPart, stackingHistory)
                };

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating stacking recommendations for part {PartId}", masterPartId);
                return new StackingRecommendationsViewModel();
            }
        }

        private List<string> GenerateStackingRecommendations(MasterPart part, List<StackingPerformanceViewModel> history)
        {
            var recommendations = new List<string>();

            if (!history.Any())
            {
                // No history - provide basic guidance
                if (part.HeightMm <= 20)
                {
                    recommendations.Add("Part height suitable for triple stacking - consider starting with 3x stack");
                }
                else if (part.HeightMm <= 50)
                {
                    recommendations.Add("Part height suitable for double stacking - recommend 2x stack");
                }
                else
                {
                    recommendations.Add("Tall part - single stack recommended to avoid build failures");
                }

                if (part.ComplexityLevel?.ToLower().Contains("complex") == true)
                {
                    recommendations.Add("Complex geometry detected - consider single stack for first builds");
                }

                return recommendations;
            }

            // Analyze historical performance
            var bestPerforming = history.OrderByDescending(h => h.SuccessRate).First();
            var mostUsed = history.OrderByDescending(h => h.TotalBuilds).First();

            if (bestPerforming.StackLevel == mostUsed.StackLevel)
            {
                recommendations.Add($"Optimal stacking: {bestPerforming.StackLevel}x stack ({bestPerforming.SuccessRate:F0}% success rate from {bestPerforming.TotalBuilds} builds)");
            }
            else
            {
                recommendations.Add($"Best performance: {bestPerforming.StackLevel}x stack ({bestPerforming.SuccessRate:F0}% success rate)");
                recommendations.Add($"Most used: {mostUsed.StackLevel}x stack ({mostUsed.TotalBuilds} builds, {mostUsed.SuccessRate:F0}% success rate)");
            }

            // Check for problematic stack levels
            var poorPerforming = history.Where(h => h.SuccessRate < 70).ToList();
            if (poorPerforming.Any())
            {
                foreach (var poor in poorPerforming)
                {
                    recommendations.Add($"?? Avoid {poor.StackLevel}x stack - only {poor.SuccessRate:F0}% success rate");
                }
            }

            // Time efficiency analysis
            var timeEfficient = history.OrderBy(h => h.AverageDurationMinutes / h.StackLevel).FirstOrDefault();
            if (timeEfficient != null && timeEfficient.SuccessRate >= 80)
            {
                recommendations.Add($"Most time-efficient: {timeEfficient.StackLevel}x stack ({timeEfficient.AverageDurationMinutes / timeEfficient.StackLevel:F0} min/part)");
            }

            return recommendations;
        }

        // === MACHINE PERFORMANCE ANALYSIS ===

        public async Task<MachinePerformanceAnalysisViewModel> GetMachinePerformanceAnalysisAsync(int masterPartId)
        {
            try
            {
                var buildHistory = await _context.BuildTimeHistory
                    .Where(bth => bth.MasterPartId == masterPartId && bth.AccuracyPercent.HasValue)
                    .Include(bth => bth.Machine)
                    .GroupBy(bth => new { bth.MachineId, bth.Machine!.Name })
                    .Select(g => new MachinePartPerformanceViewModel
                    {
                        MachineId = g.Key.MachineId,
                        MachineName = g.Key.Name,
                        TotalBuilds = g.Count(),
                        AverageAccuracy = g.Average(b => b.AccuracyPercent!.Value),
                        BestAccuracy = g.Max(b => b.AccuracyPercent!.Value),
                        WorstAccuracy = g.Min(b => b.AccuracyPercent!.Value),
                        AverageDurationMinutes = g.Average(b => b.ActualDurationMinutes ?? 0),
                        SuccessfulBuilds = g.Count(b => b.AccuracyPercent >= 80 && b.AccuracyPercent <= 120),
                        LastBuildDate = g.Max(b => b.CompletedDate)
                    })
                    .OrderByDescending(m => m.SuccessRate)
                    .ToListAsync();

                var analysis = new MachinePerformanceAnalysisViewModel
                {
                    PartId = masterPartId,
                    MachinePerformance = buildHistory,
                    TotalBuilds = buildHistory.Sum(m => m.TotalBuilds),
                    OverallSuccessRate = buildHistory.Any() ? 
                        buildHistory.Sum(m => m.SuccessfulBuilds) / (double)buildHistory.Sum(m => m.TotalBuilds) * 100 : 0
                };

                analysis.Insights = GenerateMachineInsights(buildHistory);

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing machine performance for part {PartId}", masterPartId);
                return new MachinePerformanceAnalysisViewModel();
            }
        }

        private List<string> GenerateMachineInsights(List<MachinePartPerformanceViewModel> machinePerformance)
        {
            var insights = new List<string>();

            if (!machinePerformance.Any())
            {
                insights.Add("No build history available - performance insights will appear after completing builds");
                return insights;
            }

            // Best performing machine
            var bestMachine = machinePerformance.OrderByDescending(m => m.SuccessRate).First();
            insights.Add($"Best performance on {bestMachine.MachineName}: {bestMachine.SuccessRate:F0}% success rate from {bestMachine.TotalBuilds} builds");

            // Most experienced machine
            var mostExperienced = machinePerformance.OrderByDescending(m => m.TotalBuilds).First();
            if (mostExperienced.MachineId != bestMachine.MachineId)
            {
                insights.Add($"Most builds on {mostExperienced.MachineName}: {mostExperienced.TotalBuilds} builds with {mostExperienced.SuccessRate:F0}% success rate");
            }

            // Problematic machines
            var problematic = machinePerformance.Where(m => m.SuccessRate < 70 && m.TotalBuilds >= 3).ToList();
            if (problematic.Any())
            {
                foreach (var machine in problematic)
                {
                    insights.Add($"?? {machine.MachineName} showing poor performance: {machine.SuccessRate:F0}% success rate - may need calibration");
                }
            }

            // Time consistency
            var timeVariation = machinePerformance.Select(m => m.AverageDurationMinutes).ToList();
            if (timeVariation.Count > 1)
            {
                var maxTime = timeVariation.Max();
                var minTime = timeVariation.Min();
                if ((maxTime - minTime) / minTime > 0.3) // More than 30% variation
                {
                    insights.Add($"Build times vary significantly between machines ({minTime:F0}-{maxTime:F0} min) - consider process standardization");
                }
            }

            return insights;
        }

        // === QUALITY TRENDS ===

        public async Task<QualityTrendsViewModel> GetQualityTrendsAsync(int masterPartId, int days = 90)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days);

                var qualityData = await _context.ProductionBuilds
                    .Where(pb => pb.MasterPartId == masterPartId && 
                                pb.Status == ProductionBuildStatus.Completed &&
                                pb.ActualEndTime >= cutoffDate)
                    .OrderBy(pb => pb.ActualEndTime)
                    .Select(pb => new QualityDataPointViewModel
                    {
                        BuildId = pb.Id,
                        CompletedDate = pb.ActualEndTime!.Value,
                        Quantity = pb.Quantity,
                        ActualQuantityProduced = pb.ActualQuantityProduced ?? pb.Quantity,
                        DefectiveQuantity = pb.DefectiveQuantity ?? 0,
                        QualityNotes = pb.QualityNotes
                    })
                    .ToListAsync();

                var trends = new QualityTrendsViewModel
                {
                    PartId = masterPartId,
                    AnalysisPeriodDays = days,
                    QualityData = qualityData,
                    TotalBuilds = qualityData.Count,
                    TotalPartsProduced = qualityData.Sum(q => q.ActualQuantityProduced),
                    TotalDefectiveParts = qualityData.Sum(q => q.DefectiveQuantity),
                    OverallYieldRate = qualityData.Any() ? 
                        (qualityData.Sum(q => q.ActualQuantityProduced - q.DefectiveQuantity) / (double)qualityData.Sum(q => q.ActualQuantityProduced)) * 100 : 100
                };

                trends.QualityInsights = GenerateQualityInsights(qualityData);

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing quality trends for part {PartId}", masterPartId);
                return new QualityTrendsViewModel();
            }
        }

        private List<string> GenerateQualityInsights(List<QualityDataPointViewModel> qualityData)
        {
            var insights = new List<string>();

            if (!qualityData.Any())
            {
                insights.Add("No quality data available - insights will appear after completing builds");
                return insights;
            }

            // Overall yield rate
            var yieldRate = (qualityData.Sum(q => q.ActualQuantityProduced - q.DefectiveQuantity) / (double)qualityData.Sum(q => q.ActualQuantityProduced)) * 100;
            
            if (yieldRate >= 98)
            {
                insights.Add($"Excellent quality: {yieldRate:F1}% yield rate from {qualityData.Count} builds");
            }
            else if (yieldRate >= 95)
            {
                insights.Add($"Good quality: {yieldRate:F1}% yield rate - room for minor improvements");
            }
            else if (yieldRate >= 90)
            {
                insights.Add($"Acceptable quality: {yieldRate:F1}% yield rate - consider process improvements");
            }
            else
            {
                insights.Add($"?? Quality concerns: {yieldRate:F1}% yield rate - requires immediate attention");
            }

            // Trend analysis (if enough data points)
            if (qualityData.Count >= 5)
            {
                var recentBuilds = qualityData.TakeLast(3);
                var earlierBuilds = qualityData.Take(qualityData.Count - 3);

                var recentYield = (recentBuilds.Sum(q => q.ActualQuantityProduced - q.DefectiveQuantity) / (double)recentBuilds.Sum(q => q.ActualQuantityProduced)) * 100;
                var earlierYield = (earlierBuilds.Sum(q => q.ActualQuantityProduced - q.DefectiveQuantity) / (double)earlierBuilds.Sum(q => q.ActualQuantityProduced)) * 100;

                if (recentYield > earlierYield + 2)
                {
                    insights.Add("?? Quality improving - recent builds show better yield rates");
                }
                else if (recentYield < earlierYield - 2)
                {
                    insights.Add("?? Quality declining - recent builds show lower yield rates");
                }
                else
                {
                    insights.Add("Quality stable - consistent performance over time");
                }
            }

            // Common quality issues
            var buildsWithDefects = qualityData.Where(q => q.DefectiveQuantity > 0).ToList();
            if (buildsWithDefects.Any())
            {
                var defectRate = buildsWithDefects.Count / (double)qualityData.Count * 100;
                insights.Add($"{defectRate:F0}% of builds had defective parts - review quality notes for patterns");
            }

            return insights;
        }
    }
}
```

### Step 2: Create Part Performance ViewModels

#### 2A: Create Part Performance ViewModels
**File: `ViewModels/PartPerformance/PartPerformanceViewModels.cs`**

```csharp
namespace OpCentrix.ViewModels.PartPerformance
{
    public class StackingRecommendationsViewModel
    {
        public int PartId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public List<StackingPerformanceViewModel> StackingPerformance { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        
        public bool HasHistory => StackingPerformance.Any();
    }

    public class StackingPerformanceViewModel
    {
        public int StackLevel { get; set; }
        public int TotalBuilds { get; set; }
        public int SuccessfulBuilds { get; set; }
        public decimal AverageAccuracy { get; set; }
        public decimal BestAccuracy { get; set; }
        public decimal WorstAccuracy { get; set; }
        public double AverageDurationMinutes { get; set; }
        public int TotalParts { get; set; }
        
        public double SuccessRate => TotalBuilds > 0 ? (SuccessfulBuilds / (double)TotalBuilds) * 100 : 0;
        public string SuccessRateClass => SuccessRate >= 90 ? "text-success" : 
                                         SuccessRate >= 80 ? "text-warning" : "text-danger";
        public double TimePerPart => StackLevel > 0 ? AverageDurationMinutes / StackLevel : 0;
    }

    public class MachinePerformanceAnalysisViewModel
    {
        public int PartId { get; set; }
        public List<MachinePartPerformanceViewModel> MachinePerformance { get; set; } = new();
        public int TotalBuilds { get; set; }
        public double OverallSuccessRate { get; set; }
        public List<string> Insights { get; set; } = new();
        
        public bool HasHistory => MachinePerformance.Any();
    }

    public class MachinePartPerformanceViewModel
    {
        public string MachineId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public int TotalBuilds { get; set; }
        public decimal AverageAccuracy { get; set; }
        public decimal BestAccuracy { get; set; }
        public decimal WorstAccuracy { get; set; }
        public double AverageDurationMinutes { get; set; }
        public int SuccessfulBuilds { get; set; }
        public DateTime LastBuildDate { get; set; }
        
        public double SuccessRate => TotalBuilds > 0 ? (SuccessfulBuilds / (double)TotalBuilds) * 100 : 0;
        public string SuccessRateClass => SuccessRate >= 90 ? "text-success" : 
                                         SuccessRate >= 80 ? "text-warning" : "text-danger";
    }

    public class QualityTrendsViewModel
    {
        public int PartId { get; set; }
        public int AnalysisPeriodDays { get; set; }
        public List<QualityDataPointViewModel> QualityData { get; set; } = new();
        public int TotalBuilds { get; set; }
        public int TotalPartsProduced { get; set; }
        public int TotalDefectiveParts { get; set; }
        public double OverallYieldRate { get; set; }
        public List<string> QualityInsights { get; set; } = new();
        
        public bool HasData => QualityData.Any();
        public string YieldRateClass => OverallYieldRate >= 98 ? "text-success" : 
                                       OverallYieldRate >= 95 ? "text-info" : 
                                       OverallYieldRate >= 90 ? "text-warning" : "text-danger";
    }

    public class QualityDataPointViewModel
    {
        public int BuildId { get; set; }
        public DateTime CompletedDate { get; set; }
        public int Quantity { get; set; }
        public int ActualQuantityProduced { get; set; }
        public int DefectiveQuantity { get; set; }
        public string? QualityNotes { get; set; }
        
        public double YieldRate => ActualQuantityProduced > 0 ? 
            ((ActualQuantityProduced - DefectiveQuantity) / (double)ActualQuantityProduced) * 100 : 100;
    }
}
```

### Step 3: Enhanced MasterPart Form

#### 3A: Update MasterPart Form with Intelligence Features
**File: `Pages/Admin/Shared/_MasterPartForm.cshtml`**

Replace the existing form with an enhanced version:

```html
@model OpCentrix.Models.MasterPart
@{
    var stackingRecommendations = ViewData["StackingRecommendations"] as OpCentrix.ViewModels.PartPerformance.StackingRecommendationsViewModel;
    var machineAnalysis = ViewData["MachineAnalysis"] as OpCentrix.ViewModels.PartPerformance.MachinePerformanceAnalysisViewModel;
    var qualityTrends = ViewData["QualityTrends"] as OpCentrix.ViewModels.PartPerformance.QualityTrendsViewModel;
}

<div class="container-fluid">
    <!-- Part Intelligence Dashboard -->
    @if (Model.Id > 0)
    {
        <div class="row mb-4">
            <div class="col-12">
                <div class="card border-info">
                    <div class="card-header bg-info text-white">
                        <h5 class="mb-0">
                            <i class="fas fa-brain me-2"></i>
                            Part Intelligence Dashboard
                            <small class="float-end">No Calculations - Pure Historical Analysis</small>
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <!-- Quick Stats -->
                            <div class="col-md-3">
                                <div class="text-center">
                                    <div class="h4 text-primary">@Model.TotalBuildsTracked</div>
                                    <small class="text-muted">Total Builds</small>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="text-center">
                                    <div class="h4 @(Model.AveragePrinterAccuracyPercent >= 90 && Model.AveragePrinterAccuracyPercent <= 110 ? "text-success" : "text-warning")">
                                        @(Model.AveragePrinterAccuracyPercent?.ToString("F1") ?? "N/A")%
                                    </div>
                                    <small class="text-muted">Printer Accuracy</small>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="text-center">
                                    <div class="h4 @(qualityTrends?.YieldRateClass ?? "text-muted")">
                                        @(qualityTrends?.OverallYieldRate.ToString("F1") ?? "N/A")%
                                    </div>
                                    <small class="text-muted">Quality Yield</small>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="text-center">
                                    <div class="h4 text-info">
                                        @(machineAnalysis?.MachinePerformance.Count ?? 0)
                                    </div>
                                    <small class="text-muted">Machines Used</small>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    }

    <form method="post" id="masterPartForm">
        <div class="row">
            <!-- Left Column: Part Details -->
            <div class="col-lg-8">
                <!-- Basic Information -->
                <div class="card mb-4">
                    <div class="card-header bg-primary text-white">
                        <h5 class="mb-0">
                            <i class="fas fa-info-circle me-2"></i>Basic Information
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row mb-3">
                            <div class="col-md-6">
                                <label for="PartNumber" class="form-label required">Part Number</label>
                                <input asp-for="PartNumber" class="form-control" required />
                                <span asp-validation-for="PartNumber" class="text-danger"></span>
                            </div>
                            <div class="col-md-6">
                                <label for="PartName" class="form-label required">Part Name</label>
                                <input asp-for="PartName" class="form-control" required />
                                <span asp-validation-for="PartName" class="text-danger"></span>
                            </div>
                        </div>

                        <div class="mb-3">
                            <label for="PartDescription" class="form-label">Description</label>
                            <textarea asp-for="PartDescription" class="form-control" rows="3"></textarea>
                            <span asp-validation-for="PartDescription" class="text-danger"></span>
                        </div>

                        <div class="row mb-3">
                            <div class="col-md-6">
                                <label for="Material" class="form-label">Material</label>
                                <input asp-for="Material" class="form-control" 
                                       list="materialSuggestions" placeholder="e.g., Ti-6Al-4V, Inconel 718" />
                                <datalist id="materialSuggestions">
                                    <option value="Ti-6Al-4V Grade 5">
                                    <option value="Ti-6Al-4V ELI Grade 23">
                                    <option value="Inconel 718">
                                    <option value="316L Stainless Steel">
                                    <option value="AlSi10Mg">
                                    <option value="Maraging Steel 300">
                                </datalist>
                                <span asp-validation-for="Material" class="text-danger"></span>
                            </div>
                            <div class="col-md-6">
                                <label for="ComplexityLevel" class="form-label">Complexity Level</label>
                                <select asp-for="ComplexityLevel" class="form-select">
                                    <option value="">Select Complexity</option>
                                    <option value="Simple">Simple</option>
                                    <option value="Medium">Medium</option>
                                    <option value="Complex">Complex</option>
                                    <option value="Very Complex">Very Complex</option>
                                </select>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Dimensions -->
                <div class="card mb-4">
                    <div class="card-header bg-secondary text-white">
                        <h5 class="mb-0">
                            <i class="fas fa-ruler me-2"></i>Dimensions & Specifications
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row mb-3">
                            <div class="col-md-3">
                                <label for="LengthMm" class="form-label">Length (mm)</label>
                                <input asp-for="LengthMm" type="number" step="0.1" class="form-control" 
                                       onchange="checkBuildVolume()" />
                                <span asp-validation-for="LengthMm" class="text-danger"></span>
                            </div>
                            <div class="col-md-3">
                                <label for="WidthMm" class="form-label">Width (mm)</label>
                                <input asp-for="WidthMm" type="number" step="0.1" class="form-control"
                                       onchange="checkBuildVolume()" />
                                <span asp-validation-for="WidthMm" class="text-danger"></span>
                            </div>
                            <div class="col-md-3">
                                <label for="HeightMm" class="form-label">Height (mm)</label>
                                <input asp-for="HeightMm" type="number" step="0.1" class="form-control"
                                       onchange="checkBuildVolume(); updateStackingGuidance()" />
                                <span asp-validation-for="HeightMm" class="text-danger"></span>
                            </div>
                            <div class="col-md-3">
                                <label for="WeightGrams" class="form-label">Weight (g)</label>
                                <input asp-for="WeightGrams" type="number" class="form-control" />
                                <span asp-validation-for="WeightGrams" class="text-danger"></span>
                            </div>
                        </div>

                        <div id="buildVolumeCheck" class="alert" style="display: none;"></div>

                        <div class="row mb-3">
                            <div class="col-md-4">
                                <label for="MinWallThicknessMm" class="form-label">Min Wall Thickness (mm)</label>
                                <input asp-for="MinWallThicknessMm" type="number" step="0.1" class="form-control" />
                            </div>
                            <div class="col-md-4">
                                <label for="MinFeatureSizeMm" class="form-label">Min Feature Size (mm)</label>
                                <input asp-for="MinFeatureSizeMm" type="number" step="0.1" class="form-control" />
                            </div>
                            <div class="col-md-4">
                                <div class="form-check mt-4">
                                    <input asp-for="HasInternalFeatures" class="form-check-input" type="checkbox" />
                                    <label class="form-check-label" for="HasInternalFeatures">
                                        Has Internal Features
                                    </label>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Quality Requirements -->
                <div class="card mb-4">
                    <div class="card-header bg-success text-white">
                        <h5 class="mb-0">
                            <i class="fas fa-award me-2"></i>Quality Requirements
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row mb-3">
                            <div class="col-md-6">
                                <label for="SurfaceFinishRequirement" class="form-label">Surface Finish</label>
                                <select asp-for="SurfaceFinishRequirement" class="form-select">
                                    <option value="">Select Finish</option>
                                    <option value="As-Built">As-Built</option>
                                    <option value="Ra 6.3">Ra 6.3 (Rough machining)</option>
                                    <option value="Ra 3.2">Ra 3.2 (Standard machining)</option>
                                    <option value="Ra 1.6">Ra 1.6 (Fine machining)</option>
                                    <option value="Ra 0.8">Ra 0.8 (Precision machining)</option>
                                </select>
                            </div>
                            <div class="col-md-6">
                                <label for="ToleranceClass" class="form-label">Tolerance Class</label>
                                <select asp-for="ToleranceClass" class="form-select">
                                    <option value="">Select Tolerance</option>
                                    <option value="IT9">IT9 (±0.2mm typical)</option>
                                    <option value="IT8">IT8 (±0.1mm typical)</option>
                                    <option value="IT7">IT7 (±0.05mm typical)</option>
                                    <option value="IT6">IT6 (±0.02mm typical)</option>
                                </select>
                            </div>
                        </div>

                        <div class="row mb-3">
                            <div class="col-md-6">
                                <label for="PostProcessingSteps" class="form-label">Post-Processing Steps</label>
                                <textarea asp-for="PostProcessingSteps" class="form-control" rows="2"
                                         placeholder="e.g., Support removal, heat treatment, machining"></textarea>
                            </div>
                            <div class="col-md-6">
                                <label for="QualityRequirements" class="form-label">Quality Requirements</label>
                                <textarea asp-for="QualityRequirements" class="form-control" rows="2"
                                         placeholder="e.g., NDT testing, dimensional inspection"></textarea>
                            </div>
                        </div>

                        <div class="mb-3">
                            <label for="CustomerSpecifications" class="form-label">Customer Specifications</label>
                            <textarea asp-for="CustomerSpecifications" class="form-control" rows="2"></textarea>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Right Column: Intelligence & Analytics -->
            <div class="col-lg-4">
                <!-- Stacking Recommendations -->
                @if (stackingRecommendations != null)
                {
                    <div class="card mb-4">
                        <div class="card-header bg-warning text-dark">
                            <h6 class="mb-0">
                                <i class="fas fa-layer-group me-2"></i>Stacking Intelligence
                            </h6>
                        </div>
                        <div class="card-body">
                            @if (stackingRecommendations.HasHistory)
                            {
                                <div class="table-responsive mb-3">
                                    <table class="table table-sm">
                                        <thead>
                                            <tr>
                                                <th>Stack</th>
                                                <th>Builds</th>
                                                <th>Success</th>
                                                <th>Min/Part</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            @foreach (var stack in stackingRecommendations.StackingPerformance)
                                            {
                                                <tr>
                                                    <td>@stack.StackLevel×</td>
                                                    <td>@stack.TotalBuilds</td>
                                                    <td><span class="@stack.SuccessRateClass">@stack.SuccessRate.ToString("F0")%</span></td>
                                                    <td>@stack.TimePerPart.ToString("F0")</td>
                                                </tr>
                                            }
                                        </tbody>
                                    </table>
                                </div>
                            }

                            <div class="recommendations">
                                @foreach (var recommendation in stackingRecommendations.Recommendations)
                                {
                                    <div class="alert alert-info py-2 px-3 mb-2">
                                        <small>@recommendation</small>
                                    </div>
                                }
                            </div>

                            <div id="liveStackingGuidance" class="mt-2"></div>
                        </div>
                    </div>
                }

                <!-- Machine Performance -->
                @if (machineAnalysis?.HasHistory == true)
                {
                    <div class="card mb-4">
                        <div class="card-header bg-info text-white">
                            <h6 class="mb-0">
                                <i class="fas fa-desktop me-2"></i>Machine Performance
                            </h6>
                        </div>
                        <div class="card-body">
                            @foreach (var machine in machineAnalysis.MachinePerformance.Take(3))
                            {
                                <div class="d-flex justify-content-between align-items-center mb-2">
                                    <div>
                                        <div class="fw-bold">@machine.MachineName</div>
                                        <small class="text-muted">@machine.TotalBuilds builds</small>
                                    </div>
                                    <div class="text-end">
                                        <div class="@machine.SuccessRateClass fw-bold">@machine.SuccessRate.ToString("F0")%</div>
                                        <small class="text-muted">@machine.AverageAccuracy.ToString("F0")% acc</small>
                                    </div>
                                </div>
                            }

                            <div class="mt-3">
                                @foreach (var insight in machineAnalysis.Insights.Take(2))
                                {
                                    <div class="alert alert-light py-2 px-3 mb-2">
                                        <small>@insight</small>
                                    </div>
                                }
                            </div>
                        </div>
                    </div>
                }

                <!-- Quality Trends -->
                @if (qualityTrends?.HasData == true)
                {
                    <div class="card mb-4">
                        <div class="card-header bg-success text-white">
                            <h6 class="mb-0">
                                <i class="fas fa-chart-line me-2"></i>Quality Intelligence
                            </h6>
                        </div>
                        <div class="card-body">
                            <div class="text-center mb-3">
                                <div class="h4 @qualityTrends.YieldRateClass">@qualityTrends.OverallYieldRate.ToString("F1")%</div>
                                <small class="text-muted">Overall Yield Rate</small>
                            </div>

                            <div class="row text-center mb-3">
                                <div class="col-4">
                                    <div class="fw-bold text-primary">@qualityTrends.TotalBuilds</div>
                                    <small class="text-muted">Builds</small>
                                </div>
                                <div class="col-4">
                                    <div class="fw-bold text-info">@qualityTrends.TotalPartsProduced</div>
                                    <small class="text-muted">Parts</small>
                                </div>
                                <div class="col-4">
                                    <div class="fw-bold text-warning">@qualityTrends.TotalDefectiveParts</div>
                                    <small class="text-muted">Defects</small>
                                </div>
                            </div>

                            @foreach (var insight in qualityTrends.QualityInsights.Take(2))
                            {
                                <div class="alert alert-light py-2 px-3 mb-2">
                                    <small>@insight</small>
                                </div>
                            }
                        </div>
                    </div>
                }

                <!-- Version Control -->
                @if (Model.Id > 0)
                {
                    <div class="card mb-4">
                        <div class="card-header bg-secondary text-white">
                            <h6 class="mb-0">
                                <i class="fas fa-history me-2"></i>Version Control
                            </h6>
                        </div>
                        <div class="card-body text-center">
                            <div class="mb-3">
                                <div class="h5 text-info">v@Model.VersionNumber</div>
                                @if (Model.IsLatestVersion)
                                {
                                    <span class="badge bg-success">Latest Version</span>
                                }
                                else
                                {
                                    <span class="badge bg-warning">Historical Version</span>
                                }
                            </div>

                            <div class="d-grid gap-2">
                                <button type="button" class="btn btn-outline-info btn-sm" onclick="showVersionHistory(@Model.Id)">
                                    <i class="fas fa-history me-1"></i>Version History
                                </button>
                                @if (Model.IsLatestVersion)
                                {
                                    <button type="button" class="btn btn-outline-warning btn-sm" onclick="createChangeRequest(@Model.Id)">
                                        <i class="fas fa-edit me-1"></i>Request Change
                                    </button>
                                }
                            </div>
                        </div>
                    </div>
                }
            </div>
        </div>

        <!-- Form Actions -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-body text-center">
                        <button type="submit" class="btn btn-primary btn-lg me-3">
                            <i class="fas fa-save me-2"></i>
                            @(Model.Id == 0 ? "Create Part" : "Update Part")
                        </button>
                        <a href="/Admin/Parts" class="btn btn-secondary btn-lg">
                            <i class="fas fa-times me-2"></i>Cancel
                        </a>
                    </div>
                </div>
            </div>
        </div>

        <!-- Hidden Fields -->
        <input asp-for="Id" type="hidden" />
        <input asp-for="VersionNumber" type="hidden" />
        <input asp-for="IsLatestVersion" type="hidden" />
        <input asp-for="PreviousVersionId" type="hidden" />
        <input asp-for="CreatedDate" type="hidden" />
    </form>
</div>

<script>
// Build volume checking
function checkBuildVolume() {
    const length = parseFloat($('#LengthMm').val()) || 0;
    const width = parseFloat($('#WidthMm').val()) || 0;
    const height = parseFloat($('#HeightMm').val()) || 0;
    
    const buildVolumeDiv = $('#buildVolumeCheck');
    
    // SLS build volume: 250x250x200mm
    if (length > 250 || width > 250 || height > 200) {
        buildVolumeDiv.removeClass().addClass('alert alert-warning').show();
        buildVolumeDiv.html(`
            <i class="fas fa-exclamation-triangle me-2"></i>
            <strong>Build Volume Warning:</strong> 
            Part dimensions (${length}×${width}×${height}mm) may exceed SLS build volume (250×250×200mm)
        `);
    } else {
        buildVolumeDiv.hide();
    }
}

// Live stacking guidance
function updateStackingGuidance() {
    const height = parseFloat($('#HeightMm').val()) || 0;
    const guidanceDiv = $('#liveStackingGuidance');
    
    if (height > 0) {
        let guidance = '';
        
        if (height <= 20) {
            guidance = '<div class="alert alert-success py-2"><small><i class="fas fa-check me-1"></i>Height suitable for 3× stacking</small></div>';
        } else if (height <= 50) {
            guidance = '<div class="alert alert-info py-2"><small><i class="fas fa-info me-1"></i>Height suitable for 2× stacking</small></div>';
        } else {
            guidance = '<div class="alert alert-warning py-2"><small><i class="fas fa-exclamation me-1"></i>Single stack recommended for tall parts</small></div>';
        }
        
        guidanceDiv.html(guidance);
    } else {
        guidanceDiv.html('');
    }
}

// Version control functions
function showVersionHistory(partId) {
    window.location.href = `/Admin/VersionHistory/${partId}`;
}

function createChangeRequest(partId) {
    window.location.href = `/Admin/CreateChangeRequest?partId=${partId}`;
}

// Initialize on page load
$(document).ready(function() {
    checkBuildVolume();
    updateStackingGuidance();
});
</script>
```

### Step 4: Update Parts Controller to Load Analytics

#### 4A: Update Parts Page Model
**File: `Pages/Admin/Parts.cshtml.cs`**

```csharp
public class PartsModel : PageModel
{
    private readonly MasterPartService _partService;
    private readonly PartPerformanceService _performanceService;
    private readonly VersionControlService _versionControlService;

    // ... existing constructor and properties ...

    public async Task<IActionResult> OnGetEditAsync(int id)
    {
        MasterPart = await _partService.GetMasterPartAsync(id);
        if (MasterPart == null)
        {
            return NotFound();
        }

        // Load analytics data
        await LoadPartAnalyticsAsync(id);
        
        return Page();
    }

    private async Task LoadPartAnalyticsAsync(int partId)
    {
        try
        {
            // Load all analytics in parallel
            var stackingTask = _performanceService.GetStackingRecommendationsAsync(partId);
            var machineTask = _performanceService.GetMachinePerformanceAnalysisAsync(partId);
            var qualityTask = _performanceService.GetQualityTrendsAsync(partId);

            await Task.WhenAll(stackingTask, machineTask, qualityTask);

            ViewData["StackingRecommendations"] = await stackingTask;
            ViewData["MachineAnalysis"] = await machineTask;
            ViewData["QualityTrends"] = await qualityTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading part analytics for part {PartId}", partId);
            // Continue without analytics data
        }
    }
}
```

### Step 5: Register New Service

#### 5A: Update Program.cs
**File: `Program.cs`**

```csharp
// Add part performance service
builder.Services.AddScoped<PartPerformanceService>();
```

---

## Step 6: Testing & Validation

### 6A: Test Intelligence Features
1. **Open existing parts** - verify analytics load correctly
2. **Test stacking recommendations** - check historical analysis
3. **Verify machine insights** - confirm performance data displays
4. **Check quality trends** - ensure yield calculations are accurate

### 6B: Test Form Enhancements
1. **Build volume checking** - warnings appear for oversized parts
2. **Live stacking guidance** - updates based on height
3. **Material suggestions** - datalist provides common materials
4. **Version control integration** - version info displays correctly

### 6C: Test Performance
1. **Page load times** - analytics don't slow down forms
2. **Data accuracy** - calculations match expected results
3. **Error handling** - graceful degradation when analytics fail

---

## Success Criteria

- ? **Intelligence dashboard functional** - comprehensive part analytics display
- ? **Stacking recommendations accurate** - based on historical success rates
- ? **Machine performance insights helpful** - identifies best/problematic machines
- ? **Quality trend analysis working** - tracks yield rates and issues
- ? **Form enhancements improve UX** - smart validation and guidance
- ? **No calculation dependencies** - pure historical analysis approach
- ? **Performance acceptable** - analytics don't impact form responsiveness

## Next Steps

After completing this plan:
1. Test all intelligence features thoroughly
2. Verify analytics accuracy and usefulness
3. Check that forms remain responsive
4. Move to **Plan 12: Advanced Stage Intelligence**

---

**Status: READY FOR IMPLEMENTATION**  
**Next Plan: 12-Advanced-Stage-Intelligence.md**