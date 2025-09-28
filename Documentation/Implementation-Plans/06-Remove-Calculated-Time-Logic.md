# Implementation Plan 6: Remove Calculated Time Logic

**Priority: HIGH**  
**Estimated Time: 1-2 days**  
**Dependencies: Plans 1-5 must be completed first**

## Overview

Remove all calculated time logic from the system and replace with machine-driven estimates. This is a critical architectural change that aligns with the principle of trusting printer estimates over calculations.

## Current Problem

**What's Wrong:**
- ? MasterPart forms still show calculated stack times (1.3x, 1.6x multipliers)
- ? Calculated duration fields exist in database (removed in Plan 1 but may have UI references)
- ? Time estimation logic still uses multipliers instead of machine data
- ? Users see confusing calculated times alongside printer estimates

**What Should Happen:**
- ? Remove all calculation logic from forms and services  
- ? Replace calculated displays with historical machine data
- ? Show printer accuracy trends instead of calculated predictions
- ? Forms capture only printer estimates, no calculations

---

## Step-by-Step Implementation

### Step 1: Remove Calculated Time Logic from MasterPart Forms

#### 1A: Update MasterPart Form (Admin Interface)
**File: `Pages/Admin/Shared/_MasterPartForm.cshtml`**

Remove calculated time fields and replace with machine accuracy display:

```html
@model OpCentrix.Models.MasterPart

<!-- REMOVE THESE SECTIONS (if they exist): -->
<!-- 
<div class="row mb-3">
    <div class="col-md-4">
        <label for="SingleStackDurationHours" class="form-label">Single Stack Duration (hrs)</label>
        <input asp-for="SingleStackDurationHours" class="form-control" type="number" step="0.1" />
    </div>
    <div class="col-md-4">
        <label for="DoubleStackDurationHours" class="form-label">Double Stack Duration (hrs)</label>
        <input asp-for="DoubleStackDurationHours" class="form-control" type="number" step="0.1" />
    </div>
    <div class="col-md-4">
        <label for="TripleStackDurationHours" class="form-label">Triple Stack Duration (hrs)</label>
        <input asp-for="TripleStackDurationHours" class="form-control" type="number" step="0.1" />
    </div>
</div>
-->

<!-- REPLACE WITH: Machine Performance History (Read-Only Display) -->
@if (Model.Id > 0) // Only show for existing parts
{
    <div class="card mb-4">
        <div class="card-header bg-info text-white">
            <h6 class="mb-0">
                <i class="fas fa-chart-line me-2"></i>
                Machine Performance History
            </h6>
            <small>Historical data from printer estimates vs actual build times</small>
        </div>
        <div class="card-body">
            @if (Model.TotalBuildsTracked > 0)
            {
                <div class="row">
                    <div class="col-md-3">
                        <div class="text-center">
                            <div class="h4 text-primary">@Model.TotalBuildsTracked</div>
                            <small class="text-muted">Total Builds Tracked</small>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="text-center">
                            <div class="h4 @(Model.AveragePrinterAccuracyPercent >= 90 && Model.AveragePrinterAccuracyPercent <= 110 ? "text-success" : "text-warning")">
                                @(Model.AveragePrinterAccuracyPercent?.ToString("F1") ?? "N/A")%
                            </div>
                            <small class="text-muted">Average Printer Accuracy</small>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="text-center">
                            <div class="h5 text-info">
                                @(Model.LastPrinterEstimateMinutes?.ToString() ?? "N/A") min
                            </div>
                            <small class="text-muted">Last Printer Estimate</small>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="text-center">
                            <div class="h5 text-secondary">
                                @(Model.LastActualDurationMinutes?.ToString() ?? "N/A") min
                            </div>
                            <small class="text-muted">Last Actual Duration</small>
                        </div>
                    </div>
                </div>

                @if (Model.AveragePrinterAccuracyPercent.HasValue)
                {
                    <div class="mt-3">
                        <div class="progress" style="height: 10px;">
                            @{
                                var accuracy = Model.AveragePrinterAccuracyPercent.Value;
                                var progressValue = Math.Min(100, Math.Max(0, accuracy));
                                var progressClass = accuracy >= 90 && accuracy <= 110 ? "bg-success" : 
                                                   accuracy >= 80 && accuracy <= 120 ? "bg-warning" : "bg-danger";
                            }
                            <div class="progress-bar @progressClass" 
                                 role="progressbar" 
                                 style="width: @progressValue%"
                                 aria-valuenow="@progressValue" 
                                 aria-valuemin="0" 
                                 aria-valuemax="100">
                            </div>
                        </div>
                        <small class="text-muted">
                            Accuracy Range: 90-110% = Excellent, 80-120% = Good, Outside = Needs Review
                        </small>
                    </div>
                }

                <div class="mt-3">
                    <div class="alert alert-info mb-0">
                        <i class="fas fa-info-circle me-2"></i>
                        <strong>No Time Calculations:</strong> 
                        This system uses actual printer estimates entered by operators. 
                        Historical data shown above helps assess printer accuracy but is not used for predictions.
                    </div>
                </div>
            }
            else
            {
                <div class="text-center py-4">
                    <i class="fas fa-chart-line fa-3x text-muted mb-3"></i>
                    <h5 class="text-muted">No Build History Yet</h5>
                    <p class="text-muted mb-0">
                        Machine performance data will appear here after operators complete builds 
                        using printer estimates from the machine display.
                    </p>
                </div>
            }
        </div>
    </div>
}

<!-- Rest of the form remains the same -->
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

<!-- Continue with rest of existing form fields... -->
```

#### 1B: Update MasterPart Model (Remove Calculated Fields)
**File: `Models/ProductionModels.cs`**

Ensure the MasterPart model only has machine accuracy fields:

```csharp
public class MasterPart
{
    // ... existing fields ...

    // REMOVE THESE (if they exist):
    // public decimal? SingleStackDurationHours { get; set; }
    // public decimal? DoubleStackDurationHours { get; set; }  
    // public decimal? TripleStackDurationHours { get; set; }
    // public decimal? SingleMultiplier { get; set; }
    // public decimal? StackMultiplier { get; set; }

    // KEEP THESE (machine accuracy tracking):
    [Display(Name = "Last Printer Estimate (minutes)")]
    public int? LastPrinterEstimateMinutes { get; set; }
    
    [Display(Name = "Last Actual Duration (minutes)")]
    public int? LastActualDurationMinutes { get; set; }
    
    [Display(Name = "Average Printer Accuracy (%)")]
    public decimal? AveragePrinterAccuracyPercent { get; set; }
    
    [Display(Name = "Total Builds Tracked")]
    public int TotalBuildsTracked { get; set; } = 0;

    // ... rest of existing fields ...
}
```

### Step 2: Remove Calculated Logic from Services

#### 2A: Update SchedulerService (Remove Time Calculations)
**File: `Services/SchedulerService.cs`**

Remove any methods that calculate build times:

```csharp
public class SchedulerService
{
    // ... existing methods ...

    // REMOVE METHODS LIKE THESE (if they exist):
    // public TimeSpan CalculateBuildTime(int masterPartId, int stackLevel) { }
    // public decimal GetStackMultiplier(int stackLevel) { }
    // public TimeSpan EstimateBuildDuration(MasterPart part, int quantity, int stack) { }

    // REPLACE WITH: Historical data retrieval only
    public async Task<MachineAccuracySummaryViewModel> GetMachineAccuracyAsync(int masterPartId)
    {
        try
        {
            var masterPart = await _context.MasterParts.FindAsync(masterPartId);
            if (masterPart == null) return new MachineAccuracySummaryViewModel();

            return new MachineAccuracySummaryViewModel
            {
                PartId = masterPartId,
                PartNumber = masterPart.PartNumber,
                PartName = masterPart.PartName,
                TotalBuildsTracked = masterPart.TotalBuildsTracked,
                AverageAccuracyPercent = masterPart.AveragePrinterAccuracyPercent,
                LastPrinterEstimate = masterPart.LastPrinterEstimateMinutes,
                LastActualDuration = masterPart.LastActualDurationMinutes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machine accuracy for part {PartId}", masterPartId);
            return new MachineAccuracySummaryViewModel();
        }
    }

    // Method to get historical performance without calculations
    public async Task<List<BuildPerformanceViewModel>> GetBuildPerformanceHistoryAsync(int masterPartId, int limit = 10)
    {
        try
        {
            return await _context.ProductionBuilds
                .Where(pb => pb.MasterPartId == masterPartId && 
                            pb.Status == ProductionBuildStatus.Completed &&
                            pb.PrinterEstimatedEndTime.HasValue &&
                            pb.ActualStartTime.HasValue &&
                            pb.ActualEndTime.HasValue)
                .OrderByDescending(pb => pb.ActualEndTime)
                .Take(limit)
                .Select(pb => new BuildPerformanceViewModel
                {
                    BuildId = pb.Id,
                    CompletedDate = pb.ActualEndTime!.Value,
                    PrinterEstimateMinutes = (int)(pb.PrinterEstimatedEndTime!.Value - pb.ActualStartTime!.Value).TotalMinutes,
                    ActualDurationMinutes = (int)(pb.ActualEndTime!.Value - pb.ActualStartTime!.Value).TotalMinutes,
                    AccuracyPercent = CalculateAccuracyPercent(
                        (pb.PrinterEstimatedEndTime!.Value - pb.ActualStartTime!.Value).TotalMinutes,
                        (pb.ActualEndTime!.Value - pb.ActualStartTime!.Value).TotalMinutes),
                    StackLevel = pb.StackLevel,
                    Quantity = pb.Quantity
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving build performance history for part {PartId}", masterPartId);
            return new List<BuildPerformanceViewModel>();
        }
    }

    private decimal CalculateAccuracyPercent(double estimatedMinutes, double actualMinutes)
    {
        if (estimatedMinutes <= 0) return 100m;
        return (decimal)(actualMinutes / estimatedMinutes * 100);
    }

    // ... rest of existing methods ...
}
```

### Step 3: Update Job Creation Forms to Remove Calculations

#### 3A: Update Scheduler Job Forms (Remove Duration Calculations)
**File: `Pages/Scheduler/_AddEditSLSJobModal.cshtml`** (and other job forms)

Remove any automatic duration calculation fields:

```html
<!-- REMOVE SECTIONS LIKE THIS (if they exist): -->
<!--
<div class="row mb-3">
    <div class="col-md-6">
        <label for="EstimatedDuration" class="form-label">Estimated Duration</label>
        <input asp-for="EstimatedDuration" class="form-control" readonly />
        <small class="text-muted">Calculated based on part configuration</small>
    </div>
</div>
-->

<!-- KEEP ONLY: Manual end time entry based on printer display -->
<div class="row mb-3">
    <div class="col-md-6">
        <label for="ScheduledStart" class="form-label">Start Time</label>
        <input asp-for="ScheduledStart" type="datetime-local" class="form-control" required />
    </div>
    <div class="col-md-6">
        <label for="ScheduledEnd" class="form-label required">Estimated End Time</label>
        <input asp-for="ScheduledEnd" type="datetime-local" class="form-control" required />
        <small class="text-info">
            <i class="fas fa-info-circle me-1"></i>
            Enter estimate from printer display or your experience
        </small>
    </div>
</div>

<!-- Add helpful context without calculations -->
@if (Model.Id == 0) // New job
{
    <div class="alert alert-info">
        <i class="fas fa-lightbulb me-2"></i>
        <strong>Time Estimation:</strong> 
        This system doesn't calculate build times. Enter your best estimate based on:
        <ul class="mb-0 mt-2">
            <li>Printer display estimate (if available)</li>
            <li>Your experience with similar parts</li>  
            <li>Historical performance data (if part has been built before)</li>
        </ul>
    </div>
}
```

#### 3B: Remove Calculation JavaScript
Remove any JavaScript that automatically calculates durations:

```html
<!-- REMOVE SCRIPTS LIKE THIS: -->
<!--
<script>
function calculateDuration() {
    var partId = $('#PartId').val();
    var stackLevel = $('#StackLevel').val();
    var quantity = $('#Quantity').val();
    
    if (partId && stackLevel && quantity) {
        // ... calculation logic ...
    }
}
</script>
-->

<!-- REPLACE WITH: Simple validation and helper scripts -->
<script>
// Helper to set reasonable minimum end time
function updateMinEndTime() {
    var startTime = $('#ScheduledStart').val();
    if (startTime) {
        var start = new Date(startTime);
        var minEnd = new Date(start.getTime() + (30 * 60 * 1000)); // 30 minutes minimum
        $('#ScheduledEnd').attr('min', minEnd.toISOString().slice(0, 16));
    }
}

$('#ScheduledStart').on('change', updateMinEndTime);

// Initialize on page load
$(document).ready(function() {
    updateMinEndTime();
});
</script>
```

### Step 4: Update Dashboard and Reports to Show Historical Data

#### 4A: Add Machine Accuracy Dashboard Component
**File: `Pages/Admin/Parts.cshtml`**

Add a machine accuracy summary section:

```html
<!-- Add this section to show machine accuracy trends -->
@if (Model.Parts.Any(p => p.TotalBuildsTracked > 0))
{
    <div class="row mb-4">
        <div class="col-12">
            <div class="card">
                <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                    <h5 class="mb-0">
                        <i class="fas fa-chart-bar me-2"></i>
                        Machine Accuracy Summary
                    </h5>
                    <small>Based on printer estimates vs actual build times</small>
                </div>
                <div class="card-body">
                    <div class="row">
                        @foreach (var part in Model.Parts.Where(p => p.TotalBuildsTracked > 0).Take(6))
                        {
                            <div class="col-md-2 mb-3">
                                <div class="text-center">
                                    <div class="h6 text-truncate" title="@part.PartNumber - @part.PartName">
                                        @part.PartNumber
                                    </div>
                                    @if (part.AveragePrinterAccuracyPercent.HasValue)
                                    {
                                        var accuracy = part.AveragePrinterAccuracyPercent.Value;
                                        var colorClass = accuracy >= 90 && accuracy <= 110 ? "text-success" : 
                                                        accuracy >= 80 && accuracy <= 120 ? "text-warning" : "text-danger";
                                        <div class="h5 @colorClass">
                                            @accuracy.ToString("F1")%
                                        </div>
                                        <small class="text-muted">@part.TotalBuildsTracked builds</small>
                                    }
                                    else
                                    {
                                        <div class="h5 text-muted">N/A</div>
                                        <small class="text-muted">No data</small>
                                    }
                                </div>
                            </div>
                        }
                    </div>
                    
                    <div class="text-center mt-3">
                        <small class="text-muted">
                            <i class="fas fa-info-circle me-1"></i>
                            Accuracy = (Actual Time ÷ Printer Estimate) × 100. Target: 90-110%
                        </small>
                    </div>
                </div>
            </div>
        </div>
    </div>
}
```

### Step 5: Create Required ViewModels for Machine Accuracy

#### 5A: Create Machine Accuracy ViewModels
**File: `ViewModels/MachineAccuracy/MachineAccuracyViewModels.cs`**

```csharp
namespace OpCentrix.ViewModels.MachineAccuracy
{
    public class MachineAccuracySummaryViewModel
    {
        public int PartId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public int TotalBuildsTracked { get; set; }
        public decimal? AverageAccuracyPercent { get; set; }
        public int? LastPrinterEstimate { get; set; }
        public int? LastActualDuration { get; set; }
        
        public string AccuracyStatus
        {
            get
            {
                if (!AverageAccuracyPercent.HasValue) return "No Data";
                var accuracy = AverageAccuracyPercent.Value;
                return accuracy >= 90 && accuracy <= 110 ? "Excellent" :
                       accuracy >= 80 && accuracy <= 120 ? "Good" : "Needs Review";
            }
        }
        
        public string AccuracyClass
        {
            get
            {
                if (!AverageAccuracyPercent.HasValue) return "text-muted";
                var accuracy = AverageAccuracyPercent.Value;
                return accuracy >= 90 && accuracy <= 110 ? "text-success" :
                       accuracy >= 80 && accuracy <= 120 ? "text-warning" : "text-danger";
            }
        }
    }

    public class BuildPerformanceViewModel
    {
        public int BuildId { get; set; }
        public DateTime CompletedDate { get; set; }
        public int PrinterEstimateMinutes { get; set; }
        public int ActualDurationMinutes { get; set; }
        public decimal AccuracyPercent { get; set; }
        public int StackLevel { get; set; }
        public int Quantity { get; set; }
        
        public string AccuracyDisplay => $"{AccuracyPercent:F1}%";
        public string AccuracyClass => AccuracyPercent >= 90 && AccuracyPercent <= 110 ? "text-success" :
                                      AccuracyPercent >= 80 && AccuracyPercent <= 120 ? "text-warning" : "text-danger";
        
        public string PrinterEstimateDisplay => $"{PrinterEstimateMinutes / 60}h {PrinterEstimateMinutes % 60}m";
        public string ActualDurationDisplay => $"{ActualDurationMinutes / 60}h {ActualDurationMinutes % 60}m";
    }
}
```

### Step 6: Update Form Validation to Prevent Calculation Dependencies

#### 6A: Remove Calculation-Based Validation
**File: `ViewModels/Scheduler/CreateJobDto.cs`** (and related DTOs)

```csharp
public class CreateJobDto : IValidatableObject
{
    // ... existing properties ...

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // REMOVE validation that depends on calculations
        // KEEP only basic validation

        if (ScheduledEnd <= ScheduledStart)
        {
            yield return new ValidationResult(
                "End time must be after start time",
                new[] { nameof(ScheduledEnd) });
        }

        // Reasonable minimum duration (30 minutes)
        if ((ScheduledEnd - ScheduledStart).TotalMinutes < 30)
        {
            yield return new ValidationResult(
                "Build duration should be at least 30 minutes",
                new[] { nameof(ScheduledEnd) });
        }

        // Reasonable maximum duration (48 hours)
        if ((ScheduledEnd - ScheduledStart).TotalHours > 48)
        {
            yield return new ValidationResult(
                "Build duration should not exceed 48 hours",
                new[] { nameof(ScheduledEnd) });
        }

        // Don't validate against calculated times - let operators use their judgment
    }
}
```

---

## Step 7: Testing & Validation

### 7A: Test MasterPart Forms
1. **Open existing parts** - verify no calculated time fields shown
2. **Check historical data** - confirm machine accuracy displays correctly
3. **Create new parts** - ensure no calculation prompts
4. **Verify form submission** - no calculated fields saved

### 7B: Test Job Creation
1. **Schedule jobs** - verify no automatic time calculations
2. **Check validation** - confirm reasonable time validation works
3. **Test all machine types** - SLS, CNC, EDM forms work without calculations
4. **Verify user guidance** - helpful text guides operators

### 7C: Test Dashboard Displays
1. **Check part accuracy** - historical data displays correctly
2. **Verify no calculation references** - no calculated times shown
3. **Test performance metrics** - accuracy calculations work properly

---

## Success Criteria

- ? **No calculated time fields** in any forms or displays
- ? **No automatic duration calculations** - operators enter estimates manually  
- ? **Machine accuracy tracking works** - historical data displays properly
- ? **User guidance clear** - operators understand machine-driven approach
- ? **Form validation reasonable** - prevents obviously wrong times without calculations
- ? **Dashboard shows historical trends** - machine accuracy data visible
- ? **No calculation-dependent code** - all calculation logic removed

## Next Steps

After completing this plan:
1. Test that no calculated times appear anywhere in the UI
2. Verify operators can successfully create jobs without calculations
3. Check that machine accuracy tracking works properly
4. Move to **Plan 7: Machine Accuracy Tracking**

---

**Status: READY FOR IMPLEMENTATION**  
**Next Plan: 07-Machine-Accuracy-Tracking.md**