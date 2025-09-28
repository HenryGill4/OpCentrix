# Implementation Plan 4: Stage-Specific Job Forms

**Priority: HIGH**  
**Estimated Time: 2-3 days**  
**Dependencies: Plans 1-3 must be completed first**

## Overview

Create machine-type-specific job creation forms to replace the current universal form that shows SLS parameters for all machine types. This will eliminate confusion for CNC and EDM operators who currently see irrelevant laser power and powder usage fields.

## Current Problem

The existing `/Scheduler/_AddEditJobModal.cshtml` shows:
- **SLS Process Parameters** for ALL machine types
- Laser power, scan speed, layer thickness for CNC jobs (irrelevant)
- Powder usage estimates for EDM jobs (meaningless) 
- Missing CNC-specific parameters (spindle speed, tooling, etc.)
- Missing EDM-specific parameters (wire type, cut speed, etc.)

## Solution: Machine-Type-Specific Forms

### Files to Create

```
?? OpCentrix/Pages/Scheduler/
   ?? _AddEditSLSJobModal.cshtml (refine existing form)
   ?? _AddEditCNCJobModal.cshtml (NEW - CNC-specific)
   ?? _AddEditEDMJobModal.cshtml (NEW - EDM-specific)
   ?? _AddEditGenericJobModal.cshtml (NEW - fallback)

?? OpCentrix/ViewModels/Scheduler/
   ?? CreateCNCJobDto.cs (NEW)
   ?? CreateEDMJobDto.cs (NEW)
   ?? CreateGenericJobDto.cs (NEW)
```

---

## Step-by-Step Implementation

### Step 1: Create Enhanced DTO Structure

First, create the base and stage-specific DTOs:

#### 1A: Create Base DTO Class
**File: `ViewModels/Scheduler/CreateJobDto.cs`**

```csharp
namespace OpCentrix.ViewModels.Scheduler
{
    // Base class for common properties
    public class CreateJobDto
    {
        public int Id { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public int PartId { get; set; }
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }
        public int Quantity { get; set; } = 1;
        public int Priority { get; set; } = 3;
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public string? CustomerOrderNumber { get; set; }
        public string? Operator { get; set; }
        public bool IsRushJob { get; set; }
    }
}
```

#### 1B: Create SLS-Specific DTO
**File: `ViewModels/Scheduler/CreateSLSJobDto.cs`**

```csharp
namespace OpCentrix.ViewModels.Scheduler
{
    // SLS-specific extensions
    public class CreateSLSJobDto : CreateJobDto
    {
        public string? SlsMaterial { get; set; }
        public double LaserPowerWatts { get; set; } = 200;
        public double ScanSpeedMmPerSec { get; set; } = 1200;
        public double LayerThicknessMicrons { get; set; } = 30;
        public double HatchSpacingMicrons { get; set; } = 120;
        public double BuildTemperatureCelsius { get; set; } = 180;
        public double EstimatedPowderUsageKg { get; set; } = 0.5;
        public int StackLevel { get; set; } = 1; // SLS-specific
    }
}
```

#### 1C: Create CNC-Specific DTO
**File: `ViewModels/Scheduler/CreateCNCJobDto.cs`**

```csharp
namespace OpCentrix.ViewModels.Scheduler
{
    // CNC-specific extensions
    public class CreateCNCJobDto : CreateJobDto
    {
        public string WorkHoldingMethod { get; set; } = "Vise";
        public double SpindleSpeedRPM { get; set; } = 2500;
        public double FeedRateMmPerMin { get; set; } = 500;
        public string CoolantType { get; set; } = "Flood";
        public string RequiredTooling { get; set; } = string.Empty;
        public int EstimatedToolChanges { get; set; } = 3;
        public string SurfaceFinishReq { get; set; } = "Ra 3.2";
        public string DimensionalTolerances { get; set; } = "+/- 0.1mm";
        public string ProgramFile { get; set; } = string.Empty;
        
        // Setup Requirements
        public string FixtureRequirements { get; set; } = string.Empty;
        public string SpecialToolingNotes { get; set; } = string.Empty;
        public string WorkpieceDimensions { get; set; } = string.Empty;
    }
}
```

#### 1D: Create EDM-Specific DTO
**File: `ViewModels/Scheduler/CreateEDMJobDto.cs`**

```csharp
namespace OpCentrix.ViewModels.Scheduler
{
    // EDM-specific extensions  
    public class CreateEDMJobDto : CreateJobDto
    {
        public string WireType { get; set; } = "Brass";
        public double WireDiameterMm { get; set; } = 0.25;
        public double CutSpeedMmPerMin { get; set; } = 5.0;
        public double FlushPressureBar { get; set; } = 0.5;
        public string SurfaceFinishTarget { get; set; } = "Ra 1.6";
        public double CutOffHeightMm { get; set; } = 2.0;
        public string DielectricType { get; set; } = "Deionized Water";
        
        // Setup Requirements
        public string FixturingMethod { get; set; } = "Standard Clamp";
        public string ReferenceSurfaces { get; set; } = string.Empty;
        public string ElectrodeRequirements { get; set; } = string.Empty;
        public bool TaperRequired { get; set; } = false;
        public double CornerRadiiMm { get; set; } = 0.1;
    }
}
```

#### 1E: Create Generic Fallback DTO
**File: `ViewModels/Scheduler/CreateGenericJobDto.cs`**

```csharp
namespace OpCentrix.ViewModels.Scheduler
{
    // Generic fallback for unknown machine types
    public class CreateGenericJobDto : CreateJobDto
    {
        public string ProcessParameters { get; set; } = string.Empty;
        public string SetupRequirements { get; set; } = string.Empty;
        public string SpecialInstructions { get; set; } = string.Empty;
    }
}
```

### Step 2: Implement Smart Form Routing

#### 2A: Add Machine Type Detection to Scheduler IndexModel
**File: `Pages/Scheduler/Index.cshtml.cs`**

Add this method to determine machine type and route to appropriate form:

```csharp
public async Task<IActionResult> OnGetShowAddModalAsync(string machineId, string date, int? id)
{
    try
    {
        // Get machine details
        var machine = await _schedulerService.GetMachineByIdAsync(machineId);
        if (machine == null)
        {
            return BadRequest("Machine not found");
        }

        // Determine machine type using existing logic
        var machineType = GetUnifiedMachineType(machine);
        
        // Prepare base view model
        var baseViewModel = new CreateJobDto
        {
            MachineId = machineId,
            ScheduledStart = DateTime.Parse(date),
            ScheduledEnd = DateTime.Parse(date).AddHours(2) // Default 2 hours
        };

        // Route to appropriate partial based on machine type
        return machineType.ToUpper() switch
        {
            "SLS" => await LoadSLSJobModal(baseViewModel, id),
            "CNC" => await LoadCNCJobModal(baseViewModel, id),
            "EDM" => await LoadEDMJobModal(baseViewModel, id),
            _ => await LoadGenericJobModal(baseViewModel, id) // fallback
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error showing add/edit modal for machine {MachineId}", machineId);
        return BadRequest("Error loading job form");
    }
}

private async Task<IActionResult> LoadSLSJobModal(CreateJobDto baseDto, int? id)
{
    var viewModel = new CreateSLSJobDto
    {
        Id = baseDto.Id,
        MachineId = baseDto.MachineId,
        ScheduledStart = baseDto.ScheduledStart,
        ScheduledEnd = baseDto.ScheduledEnd,
        // Set SLS defaults
        LaserPowerWatts = 200,
        ScanSpeedMmPerSec = 1200,
        LayerThicknessMicrons = 30,
        HatchSpacingMicrons = 120,
        BuildTemperatureCelsius = 180,
        EstimatedPowderUsageKg = 0.5
    };

    if (id.HasValue)
    {
        // Load existing job data if editing
        // TODO: Implement job loading logic
    }

    ViewData["AvailableParts"] = await _schedulerService.GetAvailablePartsAsync();
    return Partial("_AddEditSLSJobModal", viewModel);
}

private async Task<IActionResult> LoadCNCJobModal(CreateJobDto baseDto, int? id)
{
    var viewModel = new CreateCNCJobDto
    {
        Id = baseDto.Id,
        MachineId = baseDto.MachineId,
        ScheduledStart = baseDto.ScheduledStart,
        ScheduledEnd = baseDto.ScheduledEnd,
        // Set CNC defaults
        WorkHoldingMethod = "Vise",
        SpindleSpeedRPM = 2500,
        FeedRateMmPerMin = 500,
        CoolantType = "Flood",
        SurfaceFinishReq = "Ra 3.2",
        DimensionalTolerances = "+/- 0.1mm"
    };

    if (id.HasValue)
    {
        // Load existing job data if editing
        // TODO: Implement job loading logic
    }

    ViewData["AvailableParts"] = await _schedulerService.GetAvailablePartsAsync();
    return Partial("_AddEditCNCJobModal", viewModel);
}

private async Task<IActionResult> LoadEDMJobModal(CreateJobDto baseDto, int? id)
{
    var viewModel = new CreateEDMJobDto
    {
        Id = baseDto.Id,
        MachineId = baseDto.MachineId,
        ScheduledStart = baseDto.ScheduledStart,
        ScheduledEnd = baseDto.ScheduledEnd,
        // Set EDM defaults
        WireType = "Brass",
        WireDiameterMm = 0.25,
        CutSpeedMmPerMin = 5.0,
        FlushPressureBar = 0.5,
        SurfaceFinishTarget = "Ra 1.6",
        CutOffHeightMm = 2.0,
        DielectricType = "Deionized Water",
        FixturingMethod = "Standard Clamp"
    };

    if (id.HasValue)
    {
        // Load existing job data if editing
        // TODO: Implement job loading logic
    }

    ViewData["AvailableParts"] = await _schedulerService.GetAvailablePartsAsync();
    return Partial("_AddEditEDMJobModal", viewModel);
}

private async Task<IActionResult> LoadGenericJobModal(CreateJobDto baseDto, int? id)
{
    var viewModel = new CreateGenericJobDto
    {
        Id = baseDto.Id,
        MachineId = baseDto.MachineId,
        ScheduledStart = baseDto.ScheduledStart,
        ScheduledEnd = baseDto.ScheduledEnd
    };

    if (id.HasValue)
    {
        // Load existing job data if editing
        // TODO: Implement job loading logic
    }

    ViewData["AvailableParts"] = await _schedulerService.GetAvailablePartsAsync();
    return Partial("_AddEditGenericJobModal", viewModel);
}
```

### Step 3: Create Stage-Specific Modal Forms

#### 3A: Refine Existing SLS Form
**File: `Pages/Scheduler/_AddEditSLSJobModal.cshtml`**

Rename the existing `_AddEditJobModal.cshtml` to `_AddEditSLSJobModal.cshtml` and refine it:

```html
@model OpCentrix.ViewModels.Scheduler.CreateSLSJobDto

<div class="modal fade" id="addEditJobModal" tabindex="-1" aria-labelledby="addEditJobModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <form id="jobForm" method="post">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title" id="addEditJobModalLabel">
                        <i class="fas fa-print me-2"></i>
                        @(Model.Id == 0 ? "Schedule New SLS Job" : "Edit SLS Job")
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                
                <div class="modal-body">
                    <!-- Common Fields -->
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="PartId" class="form-label">Part</label>
                            <select asp-for="PartId" class="form-select" required>
                                <option value="">Select Part</option>
                                @if (ViewData["AvailableParts"] is List<SelectListItem> parts)
                                {
                                    @foreach (var part in parts)
                                    {
                                        <option value="@part.Value">@part.Text</option>
                                    }
                                }
                            </select>
                        </div>
                        <div class="col-md-3">
                            <label for="Quantity" class="form-label">Quantity</label>
                            <input asp-for="Quantity" type="number" class="form-control" min="1" required />
                        </div>
                        <div class="col-md-3">
                            <label for="StackLevel" class="form-label">Stack Level</label>
                            <select asp-for="StackLevel" class="form-select">
                                <option value="1">1x Stack</option>
                                <option value="2">2x Stack</option>
                                <option value="3">3x Stack</option>
                            </select>
                        </div>
                    </div>

                    <!-- Timing -->
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="ScheduledStart" class="form-label">Start Time</label>
                            <input asp-for="ScheduledStart" type="datetime-local" class="form-control" required />
                        </div>
                        <div class="col-md-6">
                            <label for="ScheduledEnd" class="form-label">Estimated End Time</label>
                            <input asp-for="ScheduledEnd" type="datetime-local" class="form-control" required />
                        </div>
                    </div>

                    <!-- SLS Process Parameters -->
                    <div class="card mb-3">
                        <div class="card-header bg-light">
                            <h6 class="mb-0"><i class="fas fa-cog me-2"></i>SLS Process Parameters</h6>
                        </div>
                        <div class="card-body">
                            <div class="row mb-3">
                                <div class="col-md-6">
                                    <label for="SlsMaterial" class="form-label">Material</label>
                                    <select asp-for="SlsMaterial" class="form-select">
                                        <option value="">Select Material</option>
                                        <option value="Ti-6Al-4V">Ti-6Al-4V</option>
                                        <option value="Inconel 718">Inconel 718</option>
                                        <option value="316L Stainless">316L Stainless Steel</option>
                                        <option value="AlSi10Mg">AlSi10Mg</option>
                                    </select>
                                </div>
                                <div class="col-md-3">
                                    <label for="LaserPowerWatts" class="form-label">Laser Power (W)</label>
                                    <input asp-for="LaserPowerWatts" type="number" class="form-control" step="0.1" />
                                </div>
                                <div class="col-md-3">
                                    <label for="ScanSpeedMmPerSec" class="form-label">Scan Speed (mm/s)</label>
                                    <input asp-for="ScanSpeedMmPerSec" type="number" class="form-control" step="0.1" />
                                </div>
                            </div>
                            
                            <div class="row mb-3">
                                <div class="col-md-3">
                                    <label for="LayerThicknessMicrons" class="form-label">Layer Thickness (?m)</label>
                                    <input asp-for="LayerThicknessMicrons" type="number" class="form-control" step="0.1" />
                                </div>
                                <div class="col-md-3">
                                    <label for="HatchSpacingMicrons" class="form-label">Hatch Spacing (?m)</label>
                                    <input asp-for="HatchSpacingMicrons" type="number" class="form-control" step="0.1" />
                                </div>
                                <div class="col-md-3">
                                    <label for="BuildTemperatureCelsius" class="form-label">Build Temp (°C)</label>
                                    <input asp-for="BuildTemperatureCelsius" type="number" class="form-control" step="0.1" />
                                </div>
                                <div class="col-md-3">
                                    <label for="EstimatedPowderUsageKg" class="form-label">Powder Usage (kg)</label>
                                    <input asp-for="EstimatedPowderUsageKg" type="number" class="form-control" step="0.1" />
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Additional Info -->
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="CustomerOrderNumber" class="form-label">Customer Order</label>
                            <input asp-for="CustomerOrderNumber" type="text" class="form-control" />
                        </div>
                        <div class="col-md-3">
                            <label for="Priority" class="form-label">Priority</label>
                            <select asp-for="Priority" class="form-select">
                                <option value="1">High</option>
                                <option value="2">Medium-High</option>
                                <option value="3">Normal</option>
                                <option value="4">Low</option>
                            </select>
                        </div>
                        <div class="col-md-3">
                            <div class="form-check mt-4">
                                <input asp-for="IsRushJob" class="form-check-input" type="checkbox" />
                                <label class="form-check-label" for="IsRushJob">
                                    Rush Job
                                </label>
                            </div>
                        </div>
                    </div>

                    <div class="mb-3">
                        <label for="Notes" class="form-label">Notes</label>
                        <textarea asp-for="Notes" class="form-control" rows="3"></textarea>
                    </div>
                </div>
                
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="submit" class="btn btn-primary">
                        <i class="fas fa-save me-2"></i>
                        @(Model.Id == 0 ? "Schedule Job" : "Update Job")
                    </button>
                </div>

                <!-- Hidden Fields -->
                <input asp-for="Id" type="hidden" />
                <input asp-for="MachineId" type="hidden" />
            </form>
        </div>
    </div>
</div>
```

#### 3B: Create CNC Job Form
**File: `Pages/Scheduler/_AddEditCNCJobModal.cshtml`**

```html
@model OpCentrix.ViewModels.Scheduler.CreateCNCJobDto

<div class="modal fade" id="addEditJobModal" tabindex="-1" aria-labelledby="addEditJobModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <form id="jobForm" method="post">
                <div class="modal-header bg-success text-white">
                    <h5 class="modal-title" id="addEditJobModalLabel">
                        <i class="fas fa-cog me-2"></i>
                        @(Model.Id == 0 ? "Schedule New CNC Job" : "Edit CNC Job")
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                
                <div class="modal-body">
                    <!-- Common Fields -->
                    <div class="row mb-3">
                        <div class="col-md-8">
                            <label for="PartId" class="form-label">Part</label>
                            <select asp-for="PartId" class="form-select" required>
                                <option value="">Select Part</option>
                                @if (ViewData["AvailableParts"] is List<SelectListItem> parts)
                                {
                                    @foreach (var part in parts)
                                    {
                                        <option value="@part.Value">@part.Text</option>
                                    }
                                }
                            </select>
                        </div>
                        <div class="col-md-4">
                            <label for="Quantity" class="form-label">Quantity</label>
                            <input asp-for="Quantity" type="number" class="form-control" min="1" required />
                        </div>
                    </div>

                    <!-- Timing -->
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="ScheduledStart" class="form-label">Start Time</label>
                            <input asp-for="ScheduledStart" type="datetime-local" class="form-control" required />
                        </div>
                        <div class="col-md-6">
                            <label for="ScheduledEnd" class="form-label">Estimated End Time</label>
                            <input asp-for="ScheduledEnd" type="datetime-local" class="form-control" required />
                        </div>
                    </div>

                    <!-- CNC Process Parameters -->
                    <div class="card mb-3">
                        <div class="card-header bg-light">
                            <h6 class="mb-0"><i class="fas fa-wrench me-2"></i>CNC Process Parameters</h6>
                        </div>
                        <div class="card-body">
                            <div class="row mb-3">
                                <div class="col-md-4">
                                    <label for="WorkHoldingMethod" class="form-label">Work Holding</label>
                                    <select asp-for="WorkHoldingMethod" class="form-select">
                                        <option value="Vise">Vise</option>
                                        <option value="Chuck">Chuck</option>
                                        <option value="Fixture">Custom Fixture</option>
                                        <option value="Clamps">Clamps</option>
                                    </select>
                                </div>
                                <div class="col-md-4">
                                    <label for="SpindleSpeedRPM" class="form-label">Spindle Speed (RPM)</label>
                                    <input asp-for="SpindleSpeedRPM" type="number" class="form-control" step="1" />
                                </div>
                                <div class="col-md-4">
                                    <label for="FeedRateMmPerMin" class="form-label">Feed Rate (mm/min)</label>
                                    <input asp-for="FeedRateMmPerMin" type="number" class="form-control" step="0.1" />
                                </div>
                            </div>
                            
                            <div class="row mb-3">
                                <div class="col-md-3">
                                    <label for="CoolantType" class="form-label">Coolant</label>
                                    <select asp-for="CoolantType" class="form-select">
                                        <option value="Flood">Flood</option>
                                        <option value="Mist">Mist</option>
                                        <option value="None">None</option>
                                        <option value="Air">Air Blast</option>
                                    </select>
                                </div>
                                <div class="col-md-3">
                                    <label for="EstimatedToolChanges" class="form-label">Tool Changes</label>
                                    <input asp-for="EstimatedToolChanges" type="number" class="form-control" min="0" />
                                </div>
                                <div class="col-md-3">
                                    <label for="SurfaceFinishReq" class="form-label">Surface Finish</label>
                                    <select asp-for="SurfaceFinishReq" class="form-select">
                                        <option value="Ra 6.3">Ra 6.3 (Rough)</option>
                                        <option value="Ra 3.2">Ra 3.2 (Standard)</option>
                                        <option value="Ra 1.6">Ra 1.6 (Fine)</option>
                                        <option value="Ra 0.8">Ra 0.8 (Very Fine)</option>
                                    </select>
                                </div>
                                <div class="col-md-3">
                                    <label for="DimensionalTolerances" class="form-label">Tolerances</label>
                                    <select asp-for="DimensionalTolerances" class="form-select">
                                        <option value="+/- 0.1mm">±0.1mm (Standard)</option>
                                        <option value="+/- 0.05mm">±0.05mm (Precision)</option>
                                        <option value="+/- 0.02mm">±0.02mm (High Precision)</option>
                                        <option value="+/- 0.01mm">±0.01mm (Very High)</option>
                                    </select>
                                </div>
                            </div>

                            <div class="row mb-3">
                                <div class="col-md-6">
                                    <label for="RequiredTooling" class="form-label">Required Tooling</label>
                                    <textarea asp-for="RequiredTooling" class="form-control" rows="2" placeholder="List required tools, sizes, etc."></textarea>
                                </div>
                                <div class="col-md-6">
                                    <label for="ProgramFile" class="form-label">Program/G-Code File</label>
                                    <input asp-for="ProgramFile" type="text" class="form-control" placeholder="Program filename or path" />
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Setup Requirements -->
                    <div class="card mb-3">
                        <div class="card-header bg-light">
                            <h6 class="mb-0"><i class="fas fa-tools me-2"></i>Setup Requirements</h6>
                        </div>
                        <div class="card-body">
                            <div class="row mb-3">
                                <div class="col-md-4">
                                    <label for="FixtureRequirements" class="form-label">Fixture Requirements</label>
                                    <textarea asp-for="FixtureRequirements" class="form-control" rows="2"></textarea>
                                </div>
                                <div class="col-md-4">
                                    <label for="SpecialToolingNotes" class="form-label">Special Tooling Notes</label>
                                    <textarea asp-for="SpecialToolingNotes" class="form-control" rows="2"></textarea>
                                </div>
                                <div class="col-md-4">
                                    <label for="WorkpieceDimensions" class="form-label">Workpiece Dimensions</label>
                                    <textarea asp-for="WorkpieceDimensions" class="form-control" rows="2" placeholder="L x W x H"></textarea>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Additional Info -->
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="CustomerOrderNumber" class="form-label">Customer Order</label>
                            <input asp-for="CustomerOrderNumber" type="text" class="form-control" />
                        </div>
                        <div class="col-md-3">
                            <label for="Priority" class="form-label">Priority</label>
                            <select asp-for="Priority" class="form-select">
                                <option value="1">High</option>
                                <option value="2">Medium-High</option>
                                <option value="3">Normal</option>
                                <option value="4">Low</option>
                            </select>
                        </div>
                        <div class="col-md-3">
                            <div class="form-check mt-4">
                                <input asp-for="IsRushJob" class="form-check-input" type="checkbox" />
                                <label class="form-check-label" for="IsRushJob">
                                    Rush Job
                                </label>
                            </div>
                        </div>
                    </div>

                    <div class="mb-3">
                        <label for="Notes" class="form-label">Notes</label>
                        <textarea asp-for="Notes" class="form-control" rows="3"></textarea>
                    </div>
                </div>
                
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="submit" class="btn btn-success">
                        <i class="fas fa-save me-2"></i>
                        @(Model.Id == 0 ? "Schedule Job" : "Update Job")
                    </button>
                </div>

                <!-- Hidden Fields -->
                <input asp-for="Id" type="hidden" />
                <input asp-for="MachineId" type="hidden" />
            </form>
        </div>
    </div>
</div>
```

#### 3C: Create EDM Job Form
**File: `Pages/Scheduler/_AddEditEDMJobModal.cshtml`**

```html
@model OpCentrix.ViewModels.Scheduler.CreateEDMJobDto

<div class="modal fade" id="addEditJobModal" tabindex="-1" aria-labelledby="addEditJobModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <form id="jobForm" method="post">
                <div class="modal-header bg-warning text-dark">
                    <h5 class="modal-title" id="addEditJobModalLabel">
                        <i class="fas fa-bolt me-2"></i>
                        @(Model.Id == 0 ? "Schedule New EDM Job" : "Edit EDM Job")
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                
                <div class="modal-body">
                    <!-- Common Fields -->
                    <div class="row mb-3">
                        <div class="col-md-8">
                            <label for="PartId" class="form-label">Part</label>
                            <select asp-for="PartId" class="form-select" required>
                                <option value="">Select Part</option>
                                @if (ViewData["AvailableParts"] is List<SelectListItem> parts)
                                {
                                    @foreach (var part in parts)
                                    {
                                        <option value="@part.Value">@part.Text</option>
                                    }
                                }
                            </select>
                        </div>
                        <div class="col-md-4">
                            <label for="Quantity" class="form-label">Quantity</label>
                            <input asp-for="Quantity" type="number" class="form-control" min="1" required />
                        </div>
                    </div>

                    <!-- Timing -->
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="ScheduledStart" class="form-label">Start Time</label>
                            <input asp-for="ScheduledStart" type="datetime-local" class="form-control" required />
                        </div>
                        <div class="col-md-6">
                            <label for="ScheduledEnd" class="form-label">Estimated End Time</label>
                            <input asp-for="ScheduledEnd" type="datetime-local" class="form-control" required />
                        </div>
                    </div>

                    <!-- EDM Process Parameters -->
                    <div class="card mb-3">
                        <div class="card-header bg-light">
                            <h6 class="mb-0"><i class="fas fa-zap me-2"></i>EDM Process Parameters</h6>
                        </div>
                        <div class="card-body">
                            <div class="row mb-3">
                                <div class="col-md-3">
                                    <label for="WireType" class="form-label">Wire Type</label>
                                    <select asp-for="WireType" class="form-select">
                                        <option value="Brass">Brass</option>
                                        <option value="Copper">Copper</option>
                                        <option value="Zinc Coated">Zinc Coated</option>
                                        <option value="Molybdenum">Molybdenum</option>
                                    </select>
                                </div>
                                <div class="col-md-3">
                                    <label for="WireDiameterMm" class="form-label">Wire Diameter (mm)</label>
                                    <select asp-for="WireDiameterMm" class="form-select">
                                        <option value="0.1">0.1mm</option>
                                        <option value="0.15">0.15mm</option>
                                        <option value="0.2">0.2mm</option>
                                        <option value="0.25">0.25mm</option>
                                        <option value="0.3">0.3mm</option>
                                        <option value="0.33">0.33mm</option>
                                    </select>
                                </div>
                                <div class="col-md-3">
                                    <label for="CutSpeedMmPerMin" class="form-label">Cut Speed (mm/min)</label>
                                    <input asp-for="CutSpeedMmPerMin" type="number" class="form-control" step="0.1" />
                                </div>
                                <div class="col-md-3">
                                    <label for="FlushPressureBar" class="form-label">Flush Pressure (bar)</label>
                                    <input asp-for="FlushPressureBar" type="number" class="form-control" step="0.1" />
                                </div>
                            </div>
                            
                            <div class="row mb-3">
                                <div class="col-md-3">
                                    <label for="SurfaceFinishTarget" class="form-label">Surface Finish</label>
                                    <select asp-for="SurfaceFinishTarget" class="form-select">
                                        <option value="Ra 3.2">Ra 3.2 (Rough)</option>
                                        <option value="Ra 1.6">Ra 1.6 (Standard)</option>
                                        <option value="Ra 0.8">Ra 0.8 (Fine)</option>
                                        <option value="Ra 0.4">Ra 0.4 (Very Fine)</option>
                                    </select>
                                </div>
                                <div class="col-md-3">
                                    <label for="CutOffHeightMm" class="form-label">Cut-off Height (mm)</label>
                                    <input asp-for="CutOffHeightMm" type="number" class="form-control" step="0.1" />
                                </div>
                                <div class="col-md-3">
                                    <label for="CornerRadiiMm" class="form-label">Corner Radii (mm)</label>
                                    <input asp-for="CornerRadiiMm" type="number" class="form-control" step="0.01" />
                                </div>
                                <div class="col-md-3">
                                    <label for="DielectricType" class="form-label">Dielectric</label>
                                    <select asp-for="DielectricType" class="form-select">
                                        <option value="Deionized Water">Deionized Water</option>
                                        <option value="EDM Oil">EDM Oil</option>
                                        <option value="Kerosene">Kerosene</option>
                                    </select>
                                </div>
                            </div>

                            <div class="row mb-3">
                                <div class="col-md-12">
                                    <div class="form-check">
                                        <input asp-for="TaperRequired" class="form-check-input" type="checkbox" />
                                        <label class="form-check-label" for="TaperRequired">
                                            Taper Required
                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Setup Requirements -->
                    <div class="card mb-3">
                        <div class="card-header bg-light">
                            <h6 class="mb-0"><i class="fas fa-tools me-2"></i>Setup Requirements</h6>
                        </div>
                        <div class="card-body">
                            <div class="row mb-3">
                                <div class="col-md-4">
                                    <label for="FixturingMethod" class="form-label">Fixturing Method</label>
                                    <select asp-for="FixturingMethod" class="form-select">
                                        <option value="Standard Clamp">Standard Clamp</option>
                                        <option value="Custom Fixture">Custom Fixture</option>
                                        <option value="Magnetic Chuck">Magnetic Chuck</option>
                                        <option value="Vise">Vise</option>
                                    </select>
                                </div>
                                <div class="col-md-4">
                                    <label for="ReferenceSurfaces" class="form-label">Reference Surfaces</label>
                                    <textarea asp-for="ReferenceSurfaces" class="form-control" rows="2"></textarea>
                                </div>
                                <div class="col-md-4">
                                    <label for="ElectrodeRequirements" class="form-label">Electrode Requirements</label>
                                    <textarea asp-for="ElectrodeRequirements" class="form-control" rows="2"></textarea>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Additional Info -->
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="CustomerOrderNumber" class="form-label">Customer Order</label>
                            <input asp-for="CustomerOrderNumber" type="text" class="form-control" />
                        </div>
                        <div class="col-md-3">
                            <label for="Priority" class="form-label">Priority</label>
                            <select asp-for="Priority" class="form-select">
                                <option value="1">High</option>
                                <option value="2">Medium-High</option>
                                <option value="3">Normal</option>
                                <option value="4">Low</option>
                            </select>
                        </div>
                        <div class="col-md-3">
                            <div class="form-check mt-4">
                                <input asp-for="IsRushJob" class="form-check-input" type="checkbox" />
                                <label class="form-check-label" for="IsRushJob">
                                    Rush Job
                                </label>
                            </div>
                        </div>
                    </div>

                    <div class="mb-3">
                        <label for="Notes" class="form-label">Notes</label>
                        <textarea asp-for="Notes" class="form-control" rows="3"></textarea>
                    </div>
                </div>
                
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="submit" class="btn btn-warning">
                        <i class="fas fa-save me-2"></i>
                        @(Model.Id == 0 ? "Schedule Job" : "Update Job")
                    </button>
                </div>

                <!-- Hidden Fields -->
                <input asp-for="Id" type="hidden" />
                <input asp-for="MachineId" type="hidden" />
            </form>
        </div>
    </div>
</div>
```

#### 3D: Create Generic Fallback Form
**File: `Pages/Scheduler/_AddEditGenericJobModal.cshtml`**

```html
@model OpCentrix.ViewModels.Scheduler.CreateGenericJobDto

<div class="modal fade" id="addEditJobModal" tabindex="-1" aria-labelledby="addEditJobModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <form id="jobForm" method="post">
                <div class="modal-header bg-secondary text-white">
                    <h5 class="modal-title" id="addEditJobModalLabel">
                        <i class="fas fa-cogs me-2"></i>
                        @(Model.Id == 0 ? "Schedule New Job" : "Edit Job")
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                
                <div class="modal-body">
                    <!-- Common Fields -->
                    <div class="row mb-3">
                        <div class="col-md-8">
                            <label for="PartId" class="form-label">Part</label>
                            <select asp-for="PartId" class="form-select" required>
                                <option value="">Select Part</option>
                                @if (ViewData["AvailableParts"] is List<SelectListItem> parts)
                                {
                                    @foreach (var part in parts)
                                    {
                                        <option value="@part.Value">@part.Text</option>
                                    }
                                }
                            </select>
                        </div>
                        <div class="col-md-4">
                            <label for="Quantity" class="form-label">Quantity</label>
                            <input asp-for="Quantity" type="number" class="form-control" min="1" required />
                        </div>
                    </div>

                    <!-- Timing -->
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="ScheduledStart" class="form-label">Start Time</label>
                            <input asp-for="ScheduledStart" type="datetime-local" class="form-control" required />
                        </div>
                        <div class="col-md-6">
                            <label for="ScheduledEnd" class="form-label">Estimated End Time</label>
                            <input asp-for="ScheduledEnd" type="datetime-local" class="form-control" required />
                        </div>
                    </div>

                    <!-- Generic Process Parameters -->
                    <div class="card mb-3">
                        <div class="card-header bg-light">
                            <h6 class="mb-0"><i class="fas fa-list me-2"></i>Process Parameters</h6>
                        </div>
                        <div class="card-body">
                            <div class="mb-3">
                                <label for="ProcessParameters" class="form-label">Process Parameters</label>
                                <textarea asp-for="ProcessParameters" class="form-control" rows="4" placeholder="Enter machine-specific process parameters..."></textarea>
                            </div>
                        </div>
                    </div>

                    <!-- Setup Requirements -->
                    <div class="card mb-3">
                        <div class="card-header bg-light">
                            <h6 class="mb-0"><i class="fas fa-tools me-2"></i>Setup Requirements</h6>
                        </div>
                        <div class="card-body">
                            <div class="mb-3">
                                <label for="SetupRequirements" class="form-label">Setup Requirements</label>
                                <textarea asp-for="SetupRequirements" class="form-control" rows="3" placeholder="Enter setup and fixturing requirements..."></textarea>
                            </div>
                        </div>
                    </div>

                    <!-- Special Instructions -->
                    <div class="card mb-3">
                        <div class="card-header bg-light">
                            <h6 class="mb-0"><i class="fas fa-exclamation-triangle me-2"></i>Special Instructions</h6>
                        </div>
                        <div class="card-body">
                            <div class="mb-3">
                                <label for="SpecialInstructions" class="form-label">Special Instructions</label>
                                <textarea asp-for="SpecialInstructions" class="form-control" rows="3" placeholder="Enter any special instructions or requirements..."></textarea>
                            </div>
                        </div>
                    </div>

                    <!-- Additional Info -->
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="CustomerOrderNumber" class="form-label">Customer Order</label>
                            <input asp-for="CustomerOrderNumber" type="text" class="form-control" />
                        </div>
                        <div class="col-md-3">
                            <label for="Priority" class="form-label">Priority</label>
                            <select asp-for="Priority" class="form-select">
                                <option value="1">High</option>
                                <option value="2">Medium-High</option>
                                <option value="3">Normal</option>
                                <option value="4">Low</option>
                            </select>
                        </div>
                        <div class="col-md-3">
                            <div class="form-check mt-4">
                                <input asp-for="IsRushJob" class="form-check-input" type="checkbox" />
                                <label class="form-check-label" for="IsRushJob">
                                    Rush Job
                                </label>
                            </div>
                        </div>
                    </div>

                    <div class="mb-3">
                        <label for="Notes" class="form-label">Notes</label>
                        <textarea asp-for="Notes" class="form-control" rows="3"></textarea>
                    </div>
                </div>
                
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="submit" class="btn btn-secondary">
                        <i class="fas fa-save me-2"></i>
                        @(Model.Id == 0 ? "Schedule Job" : "Update Job")
                    </button>
                </div>

                <!-- Hidden Fields -->
                <input asp-for="Id" type="hidden" />
                <input asp-for="MachineId" type="hidden" />
            </form>
        </div>
    </div>
</div>
```

---

## Step 4: Testing & Validation

### 4A: Test Each Form Type

1. **Test SLS Form (TI1, TI2, INC machines)**:
   - Should show laser power, scan speed, powder usage
   - Should include stack level options
   - Should have SLS-specific materials dropdown

2. **Test CNC Form (CNC1, CNC2, CNC3 machines)**:
   - Should show spindle speed, feed rate, tooling
   - Should NOT show powder usage or laser parameters
   - Should include work holding and surface finish options

3. **Test EDM Form (EDM machines)**:
   - Should show wire type, cut speed, flush pressure
   - Should NOT show SLS or CNC parameters
   - Should include electrode requirements

4. **Test Generic Form (unknown machine types)**:
   - Should show basic fields with generic text areas
   - Should work as fallback for unrecognized machines

### 4B: Verify Machine Type Detection

Test the routing logic by accessing different machine types and confirming the correct form loads.

---

## Success Criteria

- ? **CNC operators** see only CNC-relevant parameters (spindle speed, tooling, etc.)
- ? **EDM operators** see only EDM-relevant parameters (wire type, cut speed, etc.) 
- ? **SLS operators** see only SLS-relevant parameters (laser power, powder usage, etc.)
- ? **No confusion** - operators don't see irrelevant parameters for their machine type
- ? **Proper routing** - machine type detection works and loads correct form
- ? **Fallback handling** - unknown machine types get generic form
- ? **Better UX** - cleaner, focused forms improve operator efficiency

## Next Steps

After completing this plan:
1. Test each machine type thoroughly
2. Gather operator feedback on form usability
3. Move to **Plan 5: Service Method Implementation**
4. Implement form submission handlers for each DTO type

---

**Status: READY FOR IMPLEMENTATION**
**Next Plan: 05-Service-Method-Implementation.md**