# Implementation Plan 9: Enhanced Stage-Specific Forms Validation

**Priority: MEDIUM**  
**Estimated Time: 2 days**  
**Dependencies: Plans 1-8 must be completed first**

## Overview

Enhance the stage-specific job forms (created in Plan 4) with intelligent validation, part compatibility checks, smart defaults, and cost estimation. This makes the forms more user-friendly and prevents common scheduling mistakes.

## Current State

**What We Have (from Plan 4):**
- ? SLS, CNC, EDM, and Generic job forms
- ? Machine type detection and routing
- ? Stage-specific DTOs and parameters
- ? Basic form validation

**What We Need:**
- ? Intelligent machine-specific validation
- ? Part compatibility checks (e.g., SLS parts on CNC machines)
- ? Smart defaults based on part configuration  
- ? Stage-specific cost estimation
- ? Advanced UX features (cascading dropdowns, real-time validation)

## Solution: Intelligent Form Enhancement

Add smart validation and UX features to make job scheduling more intuitive and error-free.

---

## Step-by-Step Implementation

### Step 1: Create Part Compatibility Service

#### 1A: Create PartCompatibilityService
**File: `Services/PartCompatibilityService.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services
{
    public class PartCompatibilityService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PartCompatibilityService> _logger;

        public PartCompatibilityService(SchedulerContext context, ILogger<PartCompatibilityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // === PART COMPATIBILITY CHECKS ===

        public async Task<PartCompatibilityResult> CheckPartMachineCompatibilityAsync(int partId, string machineId)
        {
            try
            {
                var part = await _context.MasterParts.FindAsync(partId);
                var machine = await _context.Machines.FindAsync(machineId);

                if (part == null || machine == null)
                {
                    return new PartCompatibilityResult
                    {
                        IsCompatible = false,
                        Message = "Part or machine not found"
                    };
                }

                var machineType = GetUnifiedMachineType(machine);
                return await CheckCompatibilityByType(part, machineType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking part compatibility for part {PartId} on machine {MachineId}", partId, machineId);
                return new PartCompatibilityResult
                {
                    IsCompatible = false,
                    Message = "Error checking compatibility"
                };
            }
        }

        private async Task<PartCompatibilityResult> CheckCompatibilityByType(MasterPart part, string machineType)
        {
            var compatibility = new PartCompatibilityResult { IsCompatible = true };

            switch (machineType.ToUpper())
            {
                case "SLS":
                    return CheckSLSCompatibility(part);

                case "CNC":
                    return CheckCNCCompatibility(part);

                case "EDM":
                    return CheckEDMCompatibility(part);

                default:
                    compatibility.Warnings.Add("Unknown machine type - compatibility cannot be verified");
                    return compatibility;
            }
        }

        private PartCompatibilityResult CheckSLSCompatibility(MasterPart part)
        {
            var result = new PartCompatibilityResult { IsCompatible = true };

            // Check material compatibility
            if (!string.IsNullOrEmpty(part.Material))
            {
                var slsMaterials = new[] { "Ti-6Al-4V", "Inconel 718", "316L", "AlSi10Mg", "Maraging Steel" };
                if (!slsMaterials.Any(m => part.Material.Contains(m, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Warnings.Add($"Material '{part.Material}' may not be suitable for SLS printing");
                }
            }

            // Check dimensions (SLS build volume constraints)
            if (part.LengthMm > 250 || part.WidthMm > 250 || part.HeightMm > 200)
            {
                result.Warnings.Add("Part dimensions may exceed SLS build volume (250x250x200mm)");
            }

            // Check for overhangs/supports needed
            if (part.ComplexityLevel?.ToLower().Contains("complex") == true)
            {
                result.Suggestions.Add("Complex geometry - consider orientation and support structures");
            }

            return result;
        }

        private PartCompatibilityResult CheckCNCCompatibility(MasterPart part)
        {
            var result = new PartCompatibilityResult { IsCompatible = true };

            // Check material machinability
            if (!string.IsNullOrEmpty(part.Material))
            {
                var difficultMaterials = new[] { "Inconel", "Titanium", "Hardened Steel" };
                if (difficultMaterials.Any(m => part.Material.Contains(m, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Warnings.Add($"Material '{part.Material}' requires special tooling and slower speeds");
                }
            }

            // Check for internal features
            if (part.HasInternalFeatures)
            {
                result.Suggestions.Add("Part has internal features - verify tool access and clearances");
            }

            // Check tolerance requirements
            if (!string.IsNullOrEmpty(part.ToleranceClass) && part.ToleranceClass.Contains("IT6"))
            {
                result.Suggestions.Add("Tight tolerances required - plan for precision tooling and multiple operations");
            }

            // Check for thin walls
            if (part.MinWallThicknessMm < 2.0m)
            {
                result.Warnings.Add("Thin walls detected - risk of vibration and deflection during machining");
            }

            return result;
        }

        private PartCompatibilityResult CheckEDMCompatibility(MasterPart part)
        {
            var result = new PartCompatibilityResult { IsCompatible = true };

            // Check material conductivity
            if (!string.IsNullOrEmpty(part.Material))
            {
                var nonConductiveMaterials = new[] { "Ceramic", "Plastic", "Composite" };
                if (nonConductiveMaterials.Any(m => part.Material.Contains(m, StringComparison.OrdinalIgnoreCase)))
                {
                    result.IsCompatible = false;
                    result.Message = "EDM requires electrically conductive materials";
                    return result;
                }
            }

            // Check feature size (wire EDM limitations)
            if (part.MinFeatureSizeMm < 0.1m)
            {
                result.Warnings.Add("Very small features may be challenging with wire EDM");
            }

            // Check for thick sections
            if (part.HeightMm > 100)
            {
                result.Suggestions.Add("Thick sections - plan for longer cut times and potential wire breaks");
            }

            return result;
        }

        // === SMART DEFAULTS ===

        public async Task<JobDefaultsResult> GetSmartDefaultsAsync(int partId, string machineId)
        {
            try
            {
                var part = await _context.MasterParts.FindAsync(partId);
                var machine = await _context.Machines.FindAsync(machineId);

                if (part == null || machine == null)
                {
                    return new JobDefaultsResult();
                }

                var machineType = GetUnifiedMachineType(machine);
                return await GenerateDefaultsByType(part, machine, machineType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating smart defaults for part {PartId} on machine {MachineId}", partId, machineId);
                return new JobDefaultsResult();
            }
        }

        private async Task<JobDefaultsResult> GenerateDefaultsByType(MasterPart part, Machine machine, string machineType)
        {
            return machineType.ToUpper() switch
            {
                "SLS" => await GenerateSLSDefaults(part, machine),
                "CNC" => await GenerateCNCDefaults(part, machine),
                "EDM" => await GenerateEDMDefaults(part, machine),
                _ => new JobDefaultsResult()
            };
        }

        private async Task<JobDefaultsResult> GenerateSLSDefaults(MasterPart part, Machine machine)
        {
            var defaults = new JobDefaultsResult();

            // Estimate build time based on part history
            var recentBuilds = await _context.BuildTimeHistory
                .Where(bth => bth.MasterPartId == part.Id && bth.AccuracyPercent.HasValue)
                .OrderByDescending(bth => bth.CompletedDate)
                .Take(5)
                .ToListAsync();

            if (recentBuilds.Any())
            {
                var avgDuration = recentBuilds.Average(b => b.ActualDurationMinutes ?? 0);
                defaults.EstimatedDurationMinutes = (int)avgDuration;
                defaults.Confidence = "Based on recent build history";
            }
            else
            {
                // Rough estimate based on volume and complexity
                var volume = part.LengthMm * part.WidthMm * part.HeightMm;
                var baseTime = Math.Max(60, volume / 1000 * 2); // 2 minutes per cm³
                defaults.EstimatedDurationMinutes = (int)baseTime;
                defaults.Confidence = "Estimated based on part volume";
            }

            // Material suggestions
            if (string.IsNullOrEmpty(part.Material))
            {
                defaults.SuggestedMaterial = "Ti-6Al-4V"; // Default aerospace material
            }
            else
            {
                defaults.SuggestedMaterial = part.Material;
            }

            // Stack level recommendation
            if (part.HeightMm <= 20)
            {
                defaults.RecommendedStackLevel = 3;
                defaults.Suggestions.Add("Part suitable for triple stacking");
            }
            else if (part.HeightMm <= 50)
            {
                defaults.RecommendedStackLevel = 2;
                defaults.Suggestions.Add("Part suitable for double stacking");
            }
            else
            {
                defaults.RecommendedStackLevel = 1;
                defaults.Suggestions.Add("Single stack recommended for tall parts");
            }

            // Powder usage estimate
            var partVolume = (part.LengthMm * part.WidthMm * part.HeightMm) / 1000000; // Convert to cm³
            defaults.EstimatedPowderUsage = Math.Max(0.1m, (decimal)partVolume * 0.008m); // ~8g per cm³

            return defaults;
        }

        private async Task<JobDefaultsResult> GenerateCNCDefaults(MasterPart part, Machine machine)
        {
            var defaults = new JobDefaultsResult();

            // Material-based defaults
            if (!string.IsNullOrEmpty(part.Material))
            {
                if (part.Material.Contains("Aluminum", StringComparison.OrdinalIgnoreCase))
                {
                    defaults.SuggestedSpindleSpeed = 3000;
                    defaults.SuggestedFeedRate = 800;
                    defaults.SuggestedCoolant = "Flood";
                }
                else if (part.Material.Contains("Steel", StringComparison.OrdinalIgnoreCase))
                {
                    defaults.SuggestedSpindleSpeed = 1500;
                    defaults.SuggestedFeedRate = 400;
                    defaults.SuggestedCoolant = "Flood";
                }
                else if (part.Material.Contains("Titanium", StringComparison.OrdinalIgnoreCase))
                {
                    defaults.SuggestedSpindleSpeed = 800;
                    defaults.SuggestedFeedRate = 200;
                    defaults.SuggestedCoolant = "Flood";
                    defaults.Suggestions.Add("Titanium requires sharp tools and consistent coolant flow");
                }
            }

            // Complexity-based estimates
            var complexity = part.ComplexityLevel?.ToLower() ?? "medium";
            var baseTime = complexity switch
            {
                "simple" => 30,
                "medium" => 90,
                "complex" => 240,
                _ => 90
            };

            defaults.EstimatedDurationMinutes = baseTime;
            defaults.Confidence = $"Estimated based on {complexity} complexity";

            // Work holding suggestions
            if (part.LengthMm > 100 || part.WidthMm > 100)
            {
                defaults.SuggestedWorkHolding = "Custom Fixture";
                defaults.Suggestions.Add("Large part - consider custom fixturing for stability");
            }
            else
            {
                defaults.SuggestedWorkHolding = "Vise";
            }

            return defaults;
        }

        private async Task<JobDefaultsResult> GenerateEDMDefaults(MasterPart part, Machine machine)
        {
            var defaults = new JobDefaultsResult();

            // Material-based wire selection
            if (!string.IsNullOrEmpty(part.Material))
            {
                if (part.Material.Contains("Steel", StringComparison.OrdinalIgnoreCase))
                {
                    defaults.SuggestedWireType = "Brass";
                    defaults.SuggestedWireDiameter = 0.25m;
                }
                else if (part.Material.Contains("Titanium", StringComparison.OrdinalIgnoreCase))
                {
                    defaults.SuggestedWireType = "Copper";
                    defaults.SuggestedWireDiameter = 0.2m;
                    defaults.Suggestions.Add("Copper wire recommended for titanium");
                }
            }

            // Cut speed based on thickness
            if (part.HeightMm <= 20)
            {
                defaults.SuggestedCutSpeed = 8.0m;
            }
            else if (part.HeightMm <= 50)
            {
                defaults.SuggestedCutSpeed = 5.0m;
            }
            else
            {
                defaults.SuggestedCutSpeed = 3.0m;
                defaults.Suggestions.Add("Thick section - slow cut speed for accuracy");
            }

            // Time estimate based on perimeter and thickness
            var perimeter = 2 * (part.LengthMm + part.WidthMm);
            var estimatedMinutes = (perimeter * part.HeightMm) / (defaults.SuggestedCutSpeed * 60);
            defaults.EstimatedDurationMinutes = Math.Max(30, (int)estimatedMinutes);
            defaults.Confidence = "Estimated based on cut length";

            return defaults;
        }

        // === HELPER METHODS ===

        private string GetUnifiedMachineType(Machine machine)
        {
            // Use existing machine type detection logic
            if (machine.Name.Contains("TI") || machine.Name.Contains("INC"))
                return "SLS";
            if (machine.Name.Contains("CNC") || machine.Name.Contains("Mill"))
                return "CNC";
            if (machine.Name.Contains("EDM") || machine.Name.Contains("Wire"))
                return "EDM";
            
            return "Unknown";
        }
    }

    // === RESULT CLASSES ===

    public class PartCompatibilityResult
    {
        public bool IsCompatible { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public List<string> Warnings { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
    }

    public class JobDefaultsResult
    {
        public int EstimatedDurationMinutes { get; set; }
        public string Confidence { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new();
        
        // SLS defaults
        public string? SuggestedMaterial { get; set; }
        public int RecommendedStackLevel { get; set; } = 1;
        public decimal EstimatedPowderUsage { get; set; }
        
        // CNC defaults  
        public int SuggestedSpindleSpeed { get; set; }
        public int SuggestedFeedRate { get; set; }
        public string? SuggestedCoolant { get; set; }
        public string? SuggestedWorkHolding { get; set; }
        
        // EDM defaults
        public string? SuggestedWireType { get; set; }
        public decimal SuggestedWireDiameter { get; set; }
        public decimal SuggestedCutSpeed { get; set; }
    }
}
```

### Step 2: Enhance Stage-Specific Form Validation

#### 2A: Create Advanced Validation Attributes
**File: `Validation/StageSpecificValidation.cs`**

```csharp
using System.ComponentModel.DataAnnotations;
using OpCentrix.Services;

namespace OpCentrix.Validation
{
    public class SLSParameterValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is CreateSLSJobDto dto)
            {
                // Validate laser power ranges
                if (dto.LaserPowerWatts < 50 || dto.LaserPowerWatts > 400)
                {
                    ErrorMessage = "Laser power must be between 50-400 watts";
                    return false;
                }

                // Validate scan speed
                if (dto.ScanSpeedMmPerSec < 200 || dto.ScanSpeedMmPerSec > 2000)
                {
                    ErrorMessage = "Scan speed must be between 200-2000 mm/s";
                    return false;
                }

                // Validate layer thickness
                if (dto.LayerThicknessMicrons < 10 || dto.LayerThicknessMicrons > 100)
                {
                    ErrorMessage = "Layer thickness must be between 10-100 microns";
                    return false;
                }

                return true;
            }

            return false;
        }
    }

    public class CNCParameterValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is CreateCNCJobDto dto)
            {
                // Validate spindle speed
                if (dto.SpindleSpeedRPM < 100 || dto.SpindleSpeedRPM > 20000)
                {
                    ErrorMessage = "Spindle speed must be between 100-20,000 RPM";
                    return false;
                }

                // Validate feed rate
                if (dto.FeedRateMmPerMin < 10 || dto.FeedRateMmPerMin > 5000)
                {
                    ErrorMessage = "Feed rate must be between 10-5,000 mm/min";
                    return false;
                }

                return true;
            }

            return false;
        }
    }

    public class EDMParameterValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is CreateEDMJobDto dto)
            {
                // Validate wire diameter
                if (dto.WireDiameterMm < 0.05m || dto.WireDiameterMm > 0.5m)
                {
                    ErrorMessage = "Wire diameter must be between 0.05-0.5 mm";
                    return false;
                }

                // Validate cut speed
                if (dto.CutSpeedMmPerMin < 0.5m || dto.CutSpeedMmPerMin > 20m)
                {
                    ErrorMessage = "Cut speed must be between 0.5-20 mm/min";
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}
```

#### 2B: Update DTOs with Enhanced Validation
**File: `ViewModels/Scheduler/CreateJobDto.cs`**

```csharp
using OpCentrix.Validation;

[SLSParameterValidation]
public class CreateSLSJobDto : CreateJobDto
{
    // ... existing properties ...

    [Display(Name = "Material Type")]
    public string? SlsMaterial { get; set; }

    [Display(Name = "Laser Power (W)")]
    [Range(50, 400, ErrorMessage = "Laser power must be between 50-400 watts")]
    public double LaserPowerWatts { get; set; } = 200;

    [Display(Name = "Scan Speed (mm/s)")]
    [Range(200, 2000, ErrorMessage = "Scan speed must be between 200-2000 mm/s")]
    public double ScanSpeedMmPerSec { get; set; } = 1200;

    [Display(Name = "Layer Thickness (?m)")]
    [Range(10, 100, ErrorMessage = "Layer thickness must be between 10-100 microns")]
    public double LayerThicknessMicrons { get; set; } = 30;

    // Additional validation logic
    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in base.Validate(validationContext))
            yield return result;

        // Custom SLS validation
        if (StackLevel > 3 && EstimatedPowderUsageKg > 5.0)
        {
            yield return new ValidationResult(
                "High stack level with high powder usage - verify build parameters",
                new[] { nameof(StackLevel), nameof(EstimatedPowderUsageKg) });
        }

        // Validate material-specific parameters
        if (!string.IsNullOrEmpty(SlsMaterial))
        {
            if (SlsMaterial.Contains("Titanium", StringComparison.OrdinalIgnoreCase) && BuildTemperatureCelsius < 150)
            {
                yield return new ValidationResult(
                    "Titanium materials typically require build temperatures above 150°C",
                    new[] { nameof(BuildTemperatureCelsius) });
            }
        }
    }
}

[CNCParameterValidation]
public class CreateCNCJobDto : CreateJobDto
{
    // ... existing properties ...

    [Display(Name = "Spindle Speed (RPM)")]
    [Range(100, 20000, ErrorMessage = "Spindle speed must be between 100-20,000 RPM")]
    public double SpindleSpeedRPM { get; set; } = 2500;

    [Display(Name = "Feed Rate (mm/min)")]
    [Range(10, 5000, ErrorMessage = "Feed rate must be between 10-5,000 mm/min")]
    public double FeedRateMmPerMin { get; set; } = 500;

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in base.Validate(validationContext))
            yield return result;

        // CNC-specific validation
        if (string.IsNullOrEmpty(RequiredTooling))
        {
            yield return new ValidationResult(
                "Please specify required tooling for CNC operations",
                new[] { nameof(RequiredTooling) });
        }

        // Surface finish vs tolerance validation
        if (SurfaceFinishReq.Contains("0.8") && !DimensionalTolerances.Contains("0.02"))
        {
            yield return new ValidationResult(
                "Fine surface finish typically requires tight tolerances",
                new[] { nameof(SurfaceFinishReq), nameof(DimensionalTolerances) });
        }
    }
}

[EDMParameterValidation]  
public class CreateEDMJobDto : CreateJobDto
{
    // ... existing properties ...

    [Display(Name = "Wire Diameter (mm)")]
    [Range(0.05, 0.5, ErrorMessage = "Wire diameter must be between 0.05-0.5 mm")]
    public double WireDiameterMm { get; set; } = 0.25;

    [Display(Name = "Cut Speed (mm/min)")]
    [Range(0.5, 20, ErrorMessage = "Cut speed must be between 0.5-20 mm/min")]
    public double CutSpeedMmPerMin { get; set; } = 5.0;

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in base.Validate(validationContext))
            yield return result;

        // EDM-specific validation
        if (TaperRequired && CornerRadiiMm < 0.05)
        {
            yield return new ValidationResult(
                "Taper cuts require minimum corner radius of 0.05mm",
                new[] { nameof(CornerRadiiMm) });
        }

        // Wire size vs feature size validation
        if (WireDiameterMm > 0.3 && SurfaceFinishTarget.Contains("0.4"))
        {
            yield return new ValidationResult(
                "Fine surface finish requires smaller wire diameter",
                new[] { nameof(WireDiameterMm), nameof(SurfaceFinishTarget) });
        }
    }
}
```

### Step 3: Enhanced Form UI with Smart Features

#### 3A: Update SLS Job Form with Smart Defaults
**File: `Pages/Scheduler/_AddEditSLSJobModal.cshtml`**

Add smart defaults and compatibility checking:

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
                    <!-- Part Selection with Compatibility Check -->
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="PartId" class="form-label required">Part</label>
                            <select asp-for="PartId" class="form-select" required onchange="checkPartCompatibility()">
                                <option value="">Select Part</option>
                                @if (ViewData["AvailableParts"] is List<SelectListItem> parts)
                                {
                                    @foreach (var part in parts)
                                    {
                                        <option value="@part.Value">@part.Text</option>
                                    }
                                }
                            </select>
                            <span asp-validation-for="PartId" class="text-danger"></span>
                        </div>
                        <div class="col-md-3">
                            <label for="Quantity" class="form-label required">Quantity</label>
                            <input asp-for="Quantity" type="number" class="form-control" min="1" required 
                                   onchange="updatePowderEstimate()" />
                            <span asp-validation-for="Quantity" class="text-danger"></span>
                        </div>
                        <div class="col-md-3">
                            <label for="StackLevel" class="form-label">Stack Level</label>
                            <select asp-for="StackLevel" class="form-select" onchange="updateEstimates()">
                                <option value="1">1x Stack</option>
                                <option value="2">2x Stack</option>
                                <option value="3">3x Stack</option>
                            </select>
                        </div>
                    </div>

                    <!-- Compatibility Alert Area -->
                    <div id="compatibilityAlert" class="alert" style="display: none;"></div>

                    <!-- Smart Defaults Info -->
                    <div id="smartDefaults" class="alert alert-info" style="display: none;">
                        <i class="fas fa-lightbulb me-2"></i>
                        <strong>Smart Suggestions:</strong>
                        <ul id="suggestionsList" class="mb-0 mt-2"></ul>
                    </div>

                    <!-- Timing with Smart Estimates -->
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="ScheduledStart" class="form-label required">Start Time</label>
                            <input asp-for="ScheduledStart" type="datetime-local" class="form-control" required 
                                   onchange="updateEndTimeEstimate()" />
                            <span asp-validation-for="ScheduledStart" class="text-danger"></span>
                        </div>
                        <div class="col-md-6">
                            <label for="ScheduledEnd" class="form-label required">Estimated End Time</label>
                            <div class="input-group">
                                <input asp-for="ScheduledEnd" type="datetime-local" class="form-control" required />
                                <button type="button" class="btn btn-outline-secondary" onclick="useSmartEstimate()" title="Use smart estimate">
                                    <i class="fas fa-magic"></i>
                                </button>
                            </div>
                            <small class="text-info" id="estimateConfidence"></small>
                            <span asp-validation-for="ScheduledEnd" class="text-danger"></span>
                        </div>
                    </div>

                    <!-- Enhanced SLS Process Parameters -->
                    <div class="card mb-3">
                        <div class="card-header bg-light">
                            <h6 class="mb-0">
                                <i class="fas fa-cog me-2"></i>SLS Process Parameters
                                <button type="button" class="btn btn-sm btn-outline-primary float-end" onclick="loadOptimalParameters()">
                                    <i class="fas fa-magic me-1"></i>Load Optimal
                                </button>
                            </h6>
                        </div>
                        <div class="card-body">
                            <div class="row mb-3">
                                <div class="col-md-6">
                                    <label for="SlsMaterial" class="form-label">Material</label>
                                    <select asp-for="SlsMaterial" class="form-select" onchange="updateMaterialDefaults()">
                                        <option value="">Select Material</option>
                                        <option value="Ti-6Al-4V">Ti-6Al-4V</option>
                                        <option value="Inconel 718">Inconel 718</option>
                                        <option value="316L Stainless">316L Stainless Steel</option>
                                        <option value="AlSi10Mg">AlSi10Mg</option>
                                    </select>
                                </div>
                                <div class="col-md-3">
                                    <label for="LaserPowerWatts" class="form-label">Laser Power (W)</label>
                                    <input asp-for="LaserPowerWatts" type="number" class="form-control" step="1" 
                                           min="50" max="400" />
                                    <span asp-validation-for="LaserPowerWatts" class="text-danger"></span>
                                </div>
                                <div class="col-md-3">
                                    <label for="ScanSpeedMmPerSec" class="form-label">Scan Speed (mm/s)</label>
                                    <input asp-for="ScanSpeedMmPerSec" type="number" class="form-control" step="10"
                                           min="200" max="2000" />
                                    <span asp-validation-for="ScanSpeedMmPerSec" class="text-danger"></span>
                                </div>
                            </div>
                            
                            <div class="row mb-3">
                                <div class="col-md-3">
                                    <label for="LayerThicknessMicrons" class="form-label">Layer Thickness (?m)</label>
                                    <input asp-for="LayerThicknessMicrons" type="number" class="form-control" step="1"
                                           min="10" max="100" />
                                    <span asp-validation-for="LayerThicknessMicrons" class="text-danger"></span>
                                </div>
                                <div class="col-md-3">
                                    <label for="HatchSpacingMicrons" class="form-label">Hatch Spacing (?m)</label>
                                    <input asp-for="HatchSpacingMicrons" type="number" class="form-control" step="1" />
                                </div>
                                <div class="col-md-3">
                                    <label for="BuildTemperatureCelsius" class="form-label">Build Temp (°C)</label>
                                    <input asp-for="BuildTemperatureCelsius" type="number" class="form-control" step="1" />
                                </div>
                                <div class="col-md-3">
                                    <label for="EstimatedPowderUsageKg" class="form-label">Powder Usage (kg)</label>
                                    <div class="input-group">
                                        <input asp-for="EstimatedPowderUsageKg" type="number" class="form-control" step="0.1" />
                                        <button type="button" class="btn btn-outline-secondary" onclick="calculatePowderUsage()" title="Auto-calculate">
                                            <i class="fas fa-calculator"></i>
                                        </button>
                                    </div>
                                </div>
                            </div>

                            <!-- Parameter Validation Feedback -->
                            <div id="parameterValidation" class="mt-2"></div>
                        </div>
                    </div>

                    <!-- Cost Estimation -->
                    <div id="costEstimation" class="card mb-3" style="display: none;">
                        <div class="card-header bg-success text-white">
                            <h6 class="mb-0">
                                <i class="fas fa-dollar-sign me-2"></i>Cost Estimation
                            </h6>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-3">
                                    <div class="text-center">
                                        <div class="h5 text-success" id="powderCost">$0.00</div>
                                        <small class="text-muted">Powder Cost</small>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="text-center">
                                        <div class="h5 text-info" id="machineCost">$0.00</div>
                                        <small class="text-muted">Machine Time</small>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="text-center">
                                        <div class="h5 text-warning" id="laborCost">$0.00</div>
                                        <small class="text-muted">Labor</small>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="text-center">
                                        <div class="h4 text-primary" id="totalCost">$0.00</div>
                                        <small class="text-muted">Total Estimate</small>
                                    </div>
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
                                <label class="form-check-label" for="IsRushJob">Rush Job</label>
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

<script>
// Global variables for smart features
let currentPartId = 0;
let smartDefaults = {};
let compatibilityResult = {};

// Check part compatibility when part is selected
async function checkPartCompatibility() {
    const partId = $('#PartId').val();
    const machineId = $('#MachineId').val();
    
    if (!partId || !machineId) return;
    
    currentPartId = partId;
    
    try {
        const response = await fetch(`/api/scheduler/check-compatibility?partId=${partId}&machineId=${machineId}`);
        compatibilityResult = await response.json();
        
        const alertDiv = $('#compatibilityAlert');
        
        if (!compatibilityResult.isCompatible) {
            alertDiv.removeClass().addClass('alert alert-danger').show();
            alertDiv.html(`
                <i class="fas fa-exclamation-triangle me-2"></i>
                <strong>Compatibility Issue:</strong> ${compatibilityResult.message}
            `);
            return;
        }
        
        if (compatibilityResult.warnings?.length > 0) {
            alertDiv.removeClass().addClass('alert alert-warning').show();
            const warnings = compatibilityResult.warnings.map(w => `<li>${w}</li>`).join('');
            alertDiv.html(`
                <i class="fas fa-exclamation-triangle me-2"></i>
                <strong>Warnings:</strong>
                <ul class="mb-0 mt-1">${warnings}</ul>
            `);
        } else {
            alertDiv.hide();
        }
        
        // Load smart defaults
        await loadSmartDefaults();
        
    } catch (error) {
        console.error('Error checking compatibility:', error);
    }
}

// Load smart defaults for selected part
async function loadSmartDefaults() {
    if (!currentPartId) return;
    
    try {
        const machineId = $('#MachineId').val();
        const response = await fetch(`/api/scheduler/smart-defaults?partId=${currentPartId}&machineId=${machineId}`);
        smartDefaults = await response.json();
        
        if (smartDefaults.suggestions?.length > 0 || smartDefaults.confidence) {
            const defaultsDiv = $('#smartDefaults');
            const suggestionsList = $('#suggestionsList');
            
            let content = [];
            if (smartDefaults.confidence) {
                content.push(`<li><strong>Time Estimate:</strong> ${smartDefaults.estimatedDurationMinutes} minutes (${smartDefaults.confidence})</li>`);
            }
            
            smartDefaults.suggestions?.forEach(suggestion => {
                content.push(`<li>${suggestion}</li>`);
            });
            
            suggestionsList.html(content.join(''));
            defaultsDiv.show();
        }
        
        // Update cost estimation
        updateCostEstimation();
        
    } catch (error) {
        console.error('Error loading smart defaults:', error);
    }
}

// Use smart time estimate
function useSmartEstimate() {
    if (smartDefaults.estimatedDurationMinutes) {
        const startTime = new Date($('#ScheduledStart').val());
        const endTime = new Date(startTime.getTime() + smartDefaults.estimatedDurationMinutes * 60000);
        
        $('#ScheduledEnd').val(endTime.toISOString().slice(0, 16));
        $('#estimateConfidence').text(`Estimate: ${smartDefaults.confidence || 'Based on historical data'}`);
        
        updateCostEstimation();
    }
}

// Load optimal parameters for selected material
function updateMaterialDefaults() {
    const material = $('#SlsMaterial').val();
    
    const materialDefaults = {
        'Ti-6Al-4V': { power: 200, speed: 1200, layer: 30, temp: 180 },
        'Inconel 718': { power: 220, speed: 1000, layer: 30, temp: 190 },
        '316L Stainless': { power: 180, speed: 1400, layer: 30, temp: 170 },
        'AlSi10Mg': { power: 160, speed: 1600, layer: 30, temp: 160 }
    };
    
    if (materialDefaults[material]) {
        const defaults = materialDefaults[material];
        $('#LaserPowerWatts').val(defaults.power);
        $('#ScanSpeedMmPerSec').val(defaults.speed);
        $('#LayerThicknessMicrons').val(defaults.layer);
        $('#BuildTemperatureCelsius').val(defaults.temp);
    }
    
    updatePowderEstimate();
    updateCostEstimation();
}

// Calculate powder usage based on part volume and quantity
function calculatePowderUsage() {
    if (smartDefaults.estimatedPowderUsage) {
        const quantity = parseInt($('#Quantity').val()) || 1;
        const stackLevel = parseInt($('#StackLevel').val()) || 1;
        const totalUsage = smartDefaults.estimatedPowderUsage * quantity / stackLevel;
        
        $('#EstimatedPowderUsageKg').val(totalUsage.toFixed(1));
        updateCostEstimation();
    }
}

// Update powder estimate when quantity or stack changes
function updatePowderEstimate() {
    calculatePowderUsage();
}

function updateEstimates() {
    updatePowderEstimate();
    updateCostEstimation();
}

// Update cost estimation
function updateCostEstimation() {
    const powderUsage = parseFloat($('#EstimatedPowderUsageKg').val()) || 0;
    const material = $('#SlsMaterial').val();
    
    // Material costs (example rates)
    const materialCosts = {
        'Ti-6Al-4V': 285,
        'Inconel 718': 320,
        '316L Stainless': 85,
        'AlSi10Mg': 45
    };
    
    const powderCost = powderUsage * (materialCosts[material] || 100);
    
    // Machine time cost (example: $150/hour)
    const duration = smartDefaults.estimatedDurationMinutes || 120;
    const machineCost = (duration / 60) * 150;
    
    // Labor cost (example: $75/hour setup + monitoring)
    const laborCost = (duration / 60) * 25 + 50; // Setup fee
    
    const totalCost = powderCost + machineCost + laborCost;
    
    $('#powderCost').text(`$${powderCost.toFixed(2)}`);
    $('#machineCost').text(`$${machineCost.toFixed(2)}`);
    $('#laborCost').text(`$${laborCost.toFixed(2)}`);
    $('#totalCost').text(`$${totalCost.toFixed(2)}`);
    
    $('#costEstimation').show();
}

// Initialize when modal opens
$('#addEditJobModal').on('show.bs.modal', function() {
    // Reset form state
    $('#compatibilityAlert').hide();
    $('#smartDefaults').hide();
    $('#costEstimation').hide();
    
    // Set minimum datetime to current time
    const now = new Date();
    const minDateTime = now.toISOString().slice(0, 16);
    $('#ScheduledStart').attr('min', minDateTime);
});
</script>
```

### Step 4: Create API Endpoints for Smart Features

#### 4A: Create Scheduler API Controller
**File: `Controllers/Api/SchedulerController.cs`**

```csharp
using Microsoft.AspNetCore.Mvc;
using OpCentrix.Services;

namespace OpCentrix.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulerController : ControllerBase
    {
        private readonly PartCompatibilityService _compatibilityService;

        public SchedulerController(PartCompatibilityService compatibilityService)
        {
            _compatibilityService = compatibilityService;
        }

        [HttpGet("check-compatibility")]
        public async Task<IActionResult> CheckCompatibility(int partId, string machineId)
        {
            try
            {
                var result = await _compatibilityService.CheckPartMachineCompatibilityAsync(partId, machineId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to check compatibility" });
            }
        }

        [HttpGet("smart-defaults")]
        public async Task<IActionResult> GetSmartDefaults(int partId, string machineId)
        {
            try
            {
                var result = await _compatibilityService.GetSmartDefaultsAsync(partId, machineId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get smart defaults" });
            }
        }
    }
}
```

### Step 5: Register New Services

#### 5A: Update Program.cs
**File: `Program.cs`**

```csharp
// Add new service registrations
builder.Services.AddScoped<PartCompatibilityService>();
```

---

## Step 6: Testing & Validation

### 6A: Test Smart Defaults
1. **Select different parts** - verify smart defaults load correctly
2. **Test material defaults** - parameters update based on material
3. **Check cost estimation** - costs calculate properly
4. **Verify compatibility checks** - warnings appear for incompatible combinations

### 6B: Test Enhanced Validation
1. **Test parameter ranges** - validation catches out-of-range values
2. **Test cross-field validation** - related fields validate together
3. **Test stage-specific rules** - each form type has appropriate validation
4. **Check error messages** - user-friendly validation messages

### 6C: Test API Integration
1. **Test compatibility API** - returns correct compatibility results
2. **Test smart defaults API** - provides useful suggestions
3. **Check error handling** - graceful handling of API failures

---

## Success Criteria

- ? **Intelligent compatibility checking** - warns about incompatible part/machine combinations
- ? **Smart defaults working** - forms populate with intelligent suggestions
- ? **Enhanced validation functional** - stage-specific parameter validation
- ? **Cost estimation accurate** - realistic cost calculations
- ? **User experience improved** - forms are intuitive and helpful
- ? **API integration complete** - smart features work seamlessly
- ? **Error handling robust** - graceful degradation when services fail

## Next Steps

After completing this plan:
1. Test all smart features thoroughly
2. Verify validation works for all machine types
3. Check cost estimation accuracy
4. Move to **Plan 10: Version Control System**

---

**Status: READY FOR IMPLEMENTATION**  
**Next Plan: 10-Version-Control-System.md**