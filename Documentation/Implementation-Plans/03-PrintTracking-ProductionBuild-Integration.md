# Phase 3: PrintTracking & ProductionBuild Integration
**Priority: HIGH** | **Duration: 3-4 days** | **Depends On: Phase 1 & 2 Complete**

## Overview
Integrate the ProductionBuild data model and business logic with the existing PrintTracking system. This is the core integration that makes the machine-first approach work by enhancing existing workflows instead of replacing them.

## Current State After Phase 2
- ? Database fixes complete (Phase 1)
- ? Duplicate ProductionBuild pages removed (Phase 2)
- ? PrintTracking system still fully functional
- ? ProductionBuildService and models available for integration
- ? PrintTracking modals still use old BuildJob system
- ? No "Printer Estimated End Time" field in start print modal
- ? No powder tracking integration

## Success Criteria
- ? PrintTracking start modal creates ProductionBuild records (not BuildJob)
- ? "Printer Estimated End Time" field added to start print modal  
- ? Powder tracking integrated into start print modal
- ? PrintTracking dashboard shows ProductionBuild data
- ? All existing PrintTracking functionality preserved
- ? Machine-first time management implemented
- ? Real-time schedule updates when builds start

---

## Step-by-Step Implementation

### Step 1: Enhance PrintTracking Start Modal
**Duration: 2 hours**

#### 1.1 Add Master Part Selection to Start Modal
**File: `OpCentrix/Pages/PrintTracking/_StartPrintModal.cshtml`**

Replace the current part selection with MasterPart integration:

```razor
<!-- REPLACE the existing part selection section with: -->
<div class="form-group mb-3">
    <label for="masterPartSelect" class="form-label">
        <i class="fas fa-cube me-1"></i>Master Part <span class="text-danger">*</span>
    </label>
    <select asp-for="MasterPartId" class="form-select" id="masterPartSelect" required>
        <option value="">Select a master part...</option>
        @foreach (var part in Model.AvailableMasterParts)
        {
            <option value="@part.Id" 
                    data-name="@part.Name"
                    data-material="@part.Material" 
                    data-stages="@part.RequiredStages"
                    data-allow-stacking="@part.AllowStacking"
                    data-max-stack="@part.MaxStackCount">
                @part.PartNumber - @part.Name
            </option>
        }
    </select>
    <small class="text-muted">Select the master part for this production build</small>
</div>

<!-- ADD Stack Level Selection (if part allows stacking) -->
<div id="stackLevelGroup" class="form-group mb-3" style="display: none;">
    <label for="stackLevel" class="form-label">
        <i class="fas fa-layer-group me-1"></i>Stack Level
    </label>
    <select asp-for="StackLevel" class="form-select" id="stackLevel">
        <option value="1">1x Stack (Standard)</option>
        <option value="2">2x Stack (Double Height)</option>
        <option value="3">3x Stack (Triple Height)</option>
    </select>
    <small class="text-muted">Higher stack levels increase build time</small>
</div>
```

#### 1.2 Add Printer Estimated End Time Field
**File: `OpCentrix/Pages/PrintTracking/_StartPrintModal.cshtml`**

Add the critical machine-first field:

```razor
<!-- ADD this new field after the existing start time field -->
<div class="form-group mb-3">
    <label for="printerEstimatedEndTime" class="form-label">
        <i class="fas fa-clock me-1 text-primary"></i>Printer Estimated End Time <span class="text-danger">*</span>
    </label>
    <input asp-for="PrinterEstimatedEndTime" 
           type="datetime-local" 
           class="form-control" 
           id="printerEstimatedEndTime"
           required />
    <div class="form-text">
        <i class="fas fa-info-circle me-1"></i>
        <strong>Enter the completion time shown on the printer display</strong>
    </div>
</div>
```

#### 1.3 Add Powder Tracking Fields
**File: `OpCentrix/Pages/PrintTracking/_StartPrintModal.cshtml`**

Add powder tracking integration:

```razor
<!-- ADD powder tracking section -->
<div class="form-section mb-4">
    <h6 class="form-section-title">
        <i class="fas fa-tint text-warning me-2"></i>Powder Management
    </h6>
    
    <div class="form-check mb-3">
        <input asp-for="AddedPowder" class="form-check-input" type="checkbox" id="addedPowderCheck" />
        <label asp-for="AddedPowder" class="form-check-label">
            Added fresh powder today
        </label>
    </div>
    
    <div id="powderAmountGroup" class="form-group mb-3" style="display: none;">
        <label for="powderAmount" class="form-label">
            <i class="fas fa-weight me-1"></i>Powder Amount (kg)
        </label>
        <input asp-for="PowderAmountKg" 
               type="number" 
               step="0.1" 
               min="0.1" 
               max="50" 
               class="form-control" 
               id="powderAmount" />
        <small class="text-muted">Amount of fresh powder added to the machine</small>
    </div>
</div>
```

### Step 2: Update PrintTracking ViewModel  
**Duration: 1 hour**

#### 2.1 Enhance PrintStartViewModel
**File: `OpCentrix/ViewModels/PrintTracking/PrintTrackingViewModels.cs`**

Add properties for ProductionBuild integration:

```csharp
public class PrintStartViewModel
{
    // Existing properties...
    
    // ADD these new properties for ProductionBuild integration
    [Required(ErrorMessage = "Master part selection is required")]
    public int MasterPartId { get; set; }
    
    [Range(1, 5, ErrorMessage = "Stack level must be between 1 and 5")]
    public int StackLevel { get; set; } = 1;
    
    [Required(ErrorMessage = "Printer estimated end time is required")]
    public DateTime PrinterEstimatedEndTime { get; set; } = DateTime.Now.AddHours(8);
    
    public bool AddedPowder { get; set; }
    
    [Range(0.1, 50.0, ErrorMessage = "Powder amount must be between 0.1 and 50.0 kg")]
    public decimal? PowderAmountKg { get; set; }
    
    // Available options for dropdowns
    public List<MasterPartOption> AvailableMasterParts { get; set; } = new();
    
    // Computed properties
    public bool IsPrinterEstimateAccurate => 
        PrinterEstimatedEndTime > ActualStartTime && 
        (PrinterEstimatedEndTime - ActualStartTime).TotalHours >= 1;
}

// ADD this helper class if not already present
public class MasterPartOption
{
    public int Id { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string RequiredStages { get; set; } = string.Empty;
    public bool AllowStacking { get; set; }
    public int MaxStackCount { get; set; } = 1;
    
    public string DisplayText => $"{PartNumber} - {Name}";
}
```

### Step 3: Update PrintTracking Service Integration
**Duration: 2 hours**

#### 3.1 Enhance PrintTrackingService
**File: `OpCentrix/Services/PrintTrackingService.cs`**

Add ProductionBuildService integration:

```csharp
public class PrintTrackingService : IPrintTrackingService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<PrintTrackingService> _logger;
    private readonly IProductionBuildService _productionBuildService; // ADD this
    
    public PrintTrackingService(
        SchedulerContext context, 
        ILogger<PrintTrackingService> logger,
        IProductionBuildService productionBuildService) // ADD this parameter
    {
        _context = context;
        _logger = logger;
        _productionBuildService = productionBuildService;
    }
    
    // REPLACE the existing StartPrintJobAsync method with:
    public async Task<int> StartPrintJobAsync(PrintStartViewModel model, int userId)
    {
        try
        {
            _logger.LogInformation("Starting print job using ProductionBuild system for MasterPart {MasterPartId} on printer {PrinterName}", 
                model.MasterPartId, model.PrinterName);

            // Create ProductionBuild using the ProductionBuildService
            var productionBuildData = new ProductionBuildStartData
            {
                MasterPartId = model.MasterPartId,
                PrinterName = model.PrinterName,
                BuildQuantity = model.Quantity,
                StackLevel = model.StackLevel,
                MaterialBatch = await GetCurrentMaterialBatchAsync(model.PrinterName),
                PowderLot = await GetCurrentPowderLotAsync(),
                AddedPowder = model.AddedPowder,
                PowderAmountKg = model.PowderAmountKg,
                ActualStartTime = model.ActualStartTime,
                PrinterEstimatedEndTime = model.PrinterEstimatedEndTime,
                SetupNotes = model.SetupNotes
            };

            var productionBuildId = await _productionBuildService.StartProductionBuildAsync(productionBuildData, userId);
            
            _logger.LogInformation("Successfully created ProductionBuild {ProductionBuildId} for print job", productionBuildId);
            
            return productionBuildId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting print job with ProductionBuild system");
            throw;
        }
    }
    
    // ADD helper methods
    private async Task<string> GetCurrentMaterialBatchAsync(string printerName)
    {
        // Simple implementation - in real system this would track per machine
        return $"TI64-G5-{DateTime.Now:yyyyMM}-001";
    }
    
    private async Task<string> GetCurrentPowderLotAsync()
    {
        return $"PWD-{DateTime.Now:yyyy}-{DateTime.Now.DayOfYear:000}";
    }
}
```

#### 3.2 Update ProductionBuildStartData
**File: `OpCentrix/ViewModels/PrintTracking/ProductionBuildViewModels.cs`**

Add the missing printer estimate field:

```csharp
public class ProductionBuildStartData
{
    // Existing properties...
    
    // ADD this critical field
    public DateTime PrinterEstimatedEndTime { get; set; }
    
    // Ensure all required fields are present
    public int MasterPartId { get; set; }
    public string PrinterName { get; set; } = string.Empty;
    public int BuildQuantity { get; set; }
    public int StackLevel { get; set; } = 1;
    public string MaterialBatch { get; set; } = string.Empty;
    public string? PowderLot { get; set; }
    public bool AddedPowder { get; set; }
    public decimal? PowderAmountKg { get; set; }
    public DateTime ActualStartTime { get; set; }
    public string? SetupNotes { get; set; }
}
```

### Step 4: Update ProductionBuildService
**Duration: 1 hour**

#### 4.1 Enhance StartProductionBuildAsync Method
**File: `OpCentrix/Services/ProductionBuildService.cs`**

Update the method to use printer estimates:

```csharp
public async Task<int> StartProductionBuildAsync(ProductionBuildStartData data, int userId)
{
    var operationId = Guid.NewGuid().ToString("N")[..8];
    _logger.LogInformation("?? [PROD-BUILD-{OperationId}] Starting production build for MasterPart {MasterPartId} on printer {PrinterName}", 
        operationId, data.MasterPartId, data.PrinterName);

    try
    {
        // Get master part details
        var masterPart = await _context.MasterParts.FindAsync(data.MasterPartId);
        if (masterPart == null)
        {
            throw new InvalidOperationException($"Master part {data.MasterPartId} not found");
        }

        // Generate build number
        var buildNumber = await GenerateBuildNumberAsync();

        // Create production build with printer estimate
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
            PrinterEstimatedEndTime = data.PrinterEstimatedEndTime, // ADD this line
            ScheduledStartTime = data.ActualStartTime,
            ScheduledEndTime = data.PrinterEstimatedEndTime, // Use printer estimate, not calculation
            Status = "InProgress",
            CreatedByUserId = userId,
            SetupNotes = data.SetupNotes,
            CreatedDate = DateTime.UtcNow
        };

        _context.ProductionBuilds.Add(productionBuild);
        await _context.SaveChangesAsync();

        // Create stage executions based on master part requirements
        await CreateStageExecutionsAsync(productionBuild, masterPart);

        // Handle powder consumption if powder was added
        if (data.AddedPowder && data.PowderAmountKg.HasValue)
        {
            await RecordPowderConsumptionAsync(productionBuild.Id, masterPart.Material, data.PowderAmountKg.Value);
        }

        // Start first stage automatically
        await StartFirstStageAsync(productionBuild.Id, userId);

        _logger.LogInformation("? [PROD-BUILD-{OperationId}] Production build {BuildNumber} started successfully", 
            operationId, buildNumber);

        return productionBuild.Id;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "? [PROD-BUILD-{OperationId}] Error starting production build", operationId);
        throw;
    }
}

// ADD powder consumption tracking
private async Task RecordPowderConsumptionAsync(int productionBuildId, string materialType, decimal amountUsed)
{
    try
    {
        // Get powder stock and cost
        var powderStock = await _context.PowderStock.FirstOrDefaultAsync(ps => ps.MaterialType == materialType);
        var costPerKg = powderStock?.CostPerKg ?? 0m;

        // Record consumption
        var consumption = new PowderConsumption
        {
            ProductionBuildId = productionBuildId,
            MaterialType = materialType,
            AmountUsed = amountUsed,
            CostAllocated = amountUsed * costPerKg,
            UsageDate = DateTime.UtcNow
        };

        _context.PowderConsumption.Add(consumption);

        // Update stock levels if we have powder stock tracking
        if (powderStock != null && powderStock.CurrentStock >= amountUsed)
        {
            powderStock.CurrentStock -= amountUsed;
            powderStock.LastUpdated = DateTime.UtcNow;
            powderStock.LowStockAlert = powderStock.IsLowStock;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Recorded powder consumption: {AmountUsed} kg of {MaterialType} for build {ProductionBuildId}", 
            amountUsed, materialType, productionBuildId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error recording powder consumption for build {ProductionBuildId}", productionBuildId);
        // Don't throw - powder tracking failure shouldn't prevent build start
    }
}
```

### Step 5: Update PrintTracking Page Model
**Duration: 1 hour**

#### 5.1 Enhance PrintTracking Index Model
**File: `OpCentrix/Pages/PrintTracking/Index.cshtml.cs`**

Add MasterPart integration:

```csharp
public class IndexModel : PageModel
{
    private readonly IPrintTrackingService _printTrackingService;
    private readonly IProductionBuildService _productionBuildService; // ADD this
    
    public IndexModel(IPrintTrackingService printTrackingService, IProductionBuildService productionBuildService)
    {
        _printTrackingService = printTrackingService;
        _productionBuildService = productionBuildService;
    }
    
    // Update the CreateStartPrintViewModelAsync method
    private async Task<PrintStartViewModel> CreateStartPrintViewModelAsync(string? printerName, int? jobId)
    {
        var viewModel = new PrintStartViewModel
        {
            PrinterName = printerName ?? "",
            ActualStartTime = DateTime.Now,
            PrinterEstimatedEndTime = DateTime.Now.AddHours(8), // Default 8 hours
            OperatorName = User.Identity?.Name ?? "Unknown",
            UserId = GetCurrentUserId(),
            AvailablePrinters = new List<string> { "TI1", "TI2", "INC" }
        };

        // Load available master parts
        viewModel.AvailableMasterParts = await _productionBuildService.GetAvailableMasterPartsAsync();

        if (jobId.HasValue)
        {
            // Load job details if provided (existing job scheduling integration)
            var job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId.Value);
            if (job != null)
            {
                viewModel.AssociatedScheduledJobId = job.Id;
                // Try to find matching master part
                var masterPart = viewModel.AvailableMasterParts.FirstOrDefault(mp => mp.PartNumber == job.PartNumber);
                if (masterPart != null)
                {
                    viewModel.MasterPartId = masterPart.Id;
                }
                viewModel.Quantity = job.Quantity;
            }
        }

        return viewModel;
    }
}
```

### Step 6: Add JavaScript Integration
**Duration: 30 minutes**

#### 6.1 Enhance Print Start Modal JavaScript
**File: `OpCentrix/Pages/PrintTracking/_StartPrintModal.cshtml`**

Add JavaScript for the new fields:

```javascript
<script>
document.addEventListener('DOMContentLoaded', function() {
    // Handle master part selection
    const masterPartSelect = document.getElementById('masterPartSelect');
    const stackLevelGroup = document.getElementById('stackLevelGroup');
    const stackLevelSelect = document.getElementById('stackLevel');
    
    if (masterPartSelect) {
        masterPartSelect.addEventListener('change', function() {
            const selectedOption = this.options[this.selectedIndex];
            const allowStacking = selectedOption.dataset.allowStacking === 'true';
            const maxStack = parseInt(selectedOption.dataset.maxStack) || 1;
            
            if (allowStacking && maxStack > 1) {
                // Show stacking options
                stackLevelGroup.style.display = 'block';
                
                // Update stack level options
                stackLevelSelect.innerHTML = '';
                for (let i = 1; i <= Math.min(maxStack, 3); i++) {
                    const option = document.createElement('option');
                    option.value = i;
                    option.textContent = `${i}x Stack`;
                    stackLevelSelect.appendChild(option);
                }
            } else {
                stackLevelGroup.style.display = 'none';
            }
        });
    }
    
    // Handle powder checkbox
    const addedPowderCheck = document.getElementById('addedPowderCheck');
    const powderAmountGroup = document.getElementById('powderAmountGroup');
    const powderAmountInput = document.getElementById('powderAmount');
    
    if (addedPowderCheck) {
        addedPowderCheck.addEventListener('change', function() {
            if (this.checked) {
                powderAmountGroup.style.display = 'block';
                powderAmountInput.required = true;
            } else {
                powderAmountGroup.style.display = 'none';
                powderAmountInput.required = false;
            }
        });
    }
    
    // Set default printer estimated end time
    const printerEstimatedEndTime = document.getElementById('printerEstimatedEndTime');
    if (printerEstimatedEndTime && !printerEstimatedEndTime.value) {
        const now = new Date();
        now.setHours(now.getHours() + 8); // Default 8 hours from now
        now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
        printerEstimatedEndTime.value = now.toISOString().slice(0, 16);
    }
});
</script>
```

### Step 7: Update Service Registration
**Duration: 15 minutes**

#### 7.1 Ensure Service Dependencies
**File: `OpCentrix/Program.cs`**

Ensure all required services are registered:

```csharp
// Ensure these services are registered
builder.Services.AddScoped<IProductionBuildService, ProductionBuildService>();
builder.Services.AddScoped<IPrintTrackingService, PrintTrackingService>();

// Add powder inventory service (basic implementation)
builder.Services.AddScoped<IPowderInventoryService, PowderInventoryService>();
```

---

## Testing Protocol

### Step-by-Step Testing
**Duration: 1 hour**

#### Test 1: Master Part Selection
1. Navigate to `/PrintTracking`
2. Click "Start Build" button  
3. Verify master part dropdown populates
4. Select a master part with stacking enabled
5. Verify stack level options appear
6. Select a master part without stacking
7. Verify stack level options are hidden

#### Test 2: Printer Estimated End Time
1. In start print modal, verify "Printer Estimated End Time" field exists
2. Verify default value is ~8 hours from now
3. Change the value and verify it accepts the input
4. Verify field is marked as required

#### Test 3: Powder Tracking
1. Check "Added fresh powder today" checkbox
2. Verify powder amount field appears
3. Enter powder amount and verify validation
4. Uncheck powder checkbox
5. Verify powder amount field disappears

#### Test 4: Production Build Creation
1. Fill out complete form with all new fields
2. Submit form
3. Verify ProductionBuild record created (not BuildJob)
4. Verify printer estimated end time stored correctly
5. Verify powder consumption recorded if powder added
6. Verify stage executions created

#### Test 5: Dashboard Integration  
1. Navigate back to PrintTracking dashboard
2. Verify active builds show ProductionBuild data
3. Verify printer estimates displayed correctly
4. Verify no errors in browser console

---

## Rollback Plan

### Quick Rollback Commands
```bash
# Restore original PrintTracking files
git checkout HEAD~1 -- OpCentrix/Pages/PrintTracking/
git checkout HEAD~1 -- OpCentrix/ViewModels/PrintTracking/PrintTrackingViewModels.cs  
git checkout HEAD~1 -- OpCentrix/Services/PrintTrackingService.cs
```

---

## Success Verification Checklist

### Integration Complete When:
- [ ] PrintTracking start modal has all 5 new fields
- [ ] Master part selection works with stacking logic
- [ ] Printer estimated end time field functional
- [ ] Powder tracking integrated and working
- [ ] ProductionBuild records created instead of BuildJob
- [ ] Dashboard shows ProductionBuild data
- [ ] No existing PrintTracking functionality broken
- [ ] All tests pass successfully

### Ready for Next Phase When:
- [ ] Machine-first time management working
- [ ] ProductionBuild system fully integrated with PrintTracking
- [ ] Powder tracking functional
- [ ] No crashes or errors
- [ ] User experience smooth and intuitive

---

*Phase 3 Status: Ready for Implementation*  
*Prerequisites: Phase 1 & 2 Complete*  
*Next Phase: 04-Stage-Specific-Job-Forms.md*