# Modal Stacking Logic Fix - Context & Solution

## Problem Summary
From console logs: System fails to resolve legacy part for master part ID 23 (part number "14-5397") when adding 2x stacked build to TI2 machine. Validation fails with 1 error.

## Root Cause Analysis
1. **Legacy Part Resolution Failure**: `TryResolveLegacyPartAsync` method cannot find matching Part record for MasterPart "14-5397"
2. **Part Number Format Mismatch**: System expects "XX-XXXX" format but may have format inconsistencies
3. **Missing StageDefinition Model**: Referenced in code but file doesn't exist
4. **Stacking Configuration Issues**: Modal JavaScript may not properly populate hidden fields for 2x stack

## Key Files & Issues

### 1. Database Schema Issue
- MasterPart exists with PartNumber "14-5397" 
- Legacy Part table may not have matching "14-5397" record
- Fallback logic tries formatting "145397" ? "14-5397" but still fails

### 2. Modal JavaScript (scheduler-addjob-modal.js)
- Stacking section properly configured
- Hidden fields: StackLevel, PartsPerBuild, PlannedStackDurationHours
- Need to verify 2x stack chip activation and data population

### 3. Backend Validation (Index.cshtml.cs)
- `TryResolveLegacyPartAsync` method needs improvement
- Better error handling for missing legacy parts
- PartId validation should allow MasterPart-only workflows

### 4. Missing Models
- StageDefinition model referenced but not found
- ProductionStage model likely missing

## Critical Fixes Needed

### 1. Create Missing StageDefinition Model
```csharp
public class StageDefinition
{
    public int Id { get; set; }
    public int MasterPartId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int ExecutionOrder { get; set; }
    public double EstimatedHoursPerPart { get; set; }
    public string RequiredMachineType { get; set; } = string.Empty;
    public string PreferredMachines { get; set; } = string.Empty;
    public string StageConfiguration { get; set; } = "{}";
    public bool IsRequired { get; set; } = true;
    public bool CanSkip { get; set; } = false;
    public int SetupMinutes { get; set; } = 30;
    public int TeardownMinutes { get; set; } = 15;
    public bool IsActive { get; set; } = true;
    public virtual MasterPart? MasterPart { get; set; }
}
```

### 2. Fix Legacy Part Resolution
- Allow jobs to proceed with MasterPart only (PartId = 0)
- Improve part number matching logic
- Add fallback creation of legacy parts if needed

### 3. Test Data Setup
- Ensure MasterPart ID 23 ("14-5397") has proper stacking configuration
- Verify EnableDoubleStack = true, DoubleStackDurationHours set
- Create matching legacy Part record if needed

### 4. Validation Logic Fix
```csharp
// In ValidateJobRequestAsync - allow MasterPart-only workflow
if (request.PartId <= 0 && !request.MasterPartId.HasValue) 
{
    result.AddError(nameof(request.PartId), "Either Part or Master Part must be selected");
}
```

## Test Scenario
- Machine: TI2
- MasterPart: ID 23, PartNumber "14-5397" 
- Stack Level: 2x (double stack)
- Expected: Successful job creation with 2x stack configuration

## Next Steps
1. Create StageDefinition model
2. Fix legacy part resolution logic
3. Update validation to allow MasterPart-only jobs
4. Create test for "14-5397" Tiger 22s part
5. Verify 2x stacking configuration in database
6. Test modal functionality end-to-end

## Files Modified
- Models/MasterPart.cs (created)
- Models/StageDefinition.cs (needs creation)
- Pages/Scheduler/Index.cshtml.cs (fix validation)
- Test setup for Tiger 22s part data