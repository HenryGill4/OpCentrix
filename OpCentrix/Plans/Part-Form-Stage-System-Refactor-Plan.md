# ?? **OpCentrix Part Form & Stage System Refactor Plan**

**Project**: OpCentrix Manufacturing Execution System  
**Date**: January 2025  
**Status**: ?? **PLANNING PHASE** - Ready for Implementation  
**Developer**: Solo Implementation with AI Assistant  

---

## ?? **PROJECT OVERVIEW**

### **Refactor Goals**
Transform the current Part Form from a complex boolean-flag system to a clean, lookup-driven form with flexible stage management through the existing ProductionStage infrastructure.

### **Key Principles**
- ? **Keep Core Functionality**: Maintain existing stage system (ProductionStage, PartStageRequirement)
- ?? **Simplify Part Form**: Remove legacy fields, add lookup tables  
- ?? **Preserve Data**: Migrate existing part configurations seamlessly
- ?? **Enhance Scheduler**: Better integration with stage-based scheduling

---

## ? **CURRENT STATE ANALYSIS**

### **? What Already Works (Strong Foundation)**

#### **Database Schema - READY**
- ? **ProductionStage table** - Complete with DefaultHourlyRate, CustomFieldsConfig
- ? **PartStageRequirement table** - Has PlannedMinutes (EstimatedHours), SetupMinutes, TeardownMinutes, OverrideHourlyRate
- ? **Machine table** - Contains StdHourlyRate equivalent fields
- ? **Parts table** - Has all core fields (PartNumber, Name, ComponentType, CustomerPartNumber)
- ? **Audit infrastructure** - CreatedUtc/UpdatedUtc patterns established

#### **Models - PRODUCTION READY**
- ? **Part.cs** - Has PartNumber, ComponentType, Description (200+ fields total)
- ? **ProductionStage.cs** - Complete with machine assignments, custom fields, hourly rates
- ? **PartStageRequirement.cs** - Full CRUD with time tracking, cost overrides
- ? **PartStageService.cs** - Complete service layer with business logic

#### **Pages & Services - FUNCTIONAL**
- ? **Parts.cshtml/.cs** - Working CRUD operations 
- ? **_PartForm.cshtml** - Functional form structure
- ? **parts-form.js** - JavaScript foundation exists
- ? **PartStageService** - Complete with GetPartStagesAsync, AddPartStageAsync, etc.

### **?? Current Part Form Structure**
```
Part Form Sections:
??? Basic Info (PartNumber, Name, Description) ?
??? Material Selection ?  
??? ComponentType (string field) ? NEEDS LOOKUP
??? Industry/Application ? REMOVE
??? Part Class ? REMOVE  
??? Quality Standards ? REMOVE
??? Stage Boolean Flags (20+ fields) ? REMOVE
??? Duration Fields ? MOVE TO STAGE LEVEL
??? Assets ? ADD NEW SECTION
```

---

## ??? **DETAILED IMPLEMENTATION PLAN**

## **Phase 1: Foundation Setup** (No Breaking Changes)
*Timeline: 1-2 days*

### **1.1 Create Lookup Tables**
```sql
-- ComponentType lookup (General/Serialized)
CREATE TABLE ComponentTypes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Description TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    SortOrder INTEGER NOT NULL DEFAULT 100,
    CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
    CreatedBy TEXT NOT NULL DEFAULT 'System',
    LastModifiedDate TEXT NOT NULL DEFAULT (datetime('now')),
    LastModifiedBy TEXT NOT NULL DEFAULT 'System'
);

-- ComplianceCategory lookup (Non NFA/NFA)  
CREATE TABLE ComplianceCategories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Description TEXT NOT NULL,
    RegulatoryLevel TEXT NOT NULL, -- 'Standard', 'Regulated', 'Restricted'
    RequiresSpecialHandling INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1,
    SortOrder INTEGER NOT NULL DEFAULT 100,
    CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
    CreatedBy TEXT NOT NULL DEFAULT 'System',
    LastModifiedDate TEXT NOT NULL DEFAULT (datetime('now')),
    LastModifiedBy TEXT NOT NULL DEFAULT 'System'
);

-- PartAssetLink for 3D models/photos
CREATE TABLE PartAssetLinks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PartId INTEGER NOT NULL,
    Url TEXT NOT NULL,
    DisplayName TEXT NOT NULL,
    Source TEXT NOT NULL, -- 'Upload', 'External', 'Generated'
    AssetType TEXT NOT NULL, -- '3DModel', 'Photo', 'Drawing', 'Document'
    LastCheckedUtc TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
    CreatedBy TEXT NOT NULL DEFAULT 'System',
    FOREIGN KEY (PartId) REFERENCES Parts(Id) ON DELETE CASCADE
);

-- NEW: Legacy flag to stage mapping table for cleaner migration
CREATE TABLE LegacyFlagToStageMap (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    LegacyFieldName TEXT NOT NULL UNIQUE, -- 'RequiresSLSPrinting', 'RequiresCNCMachining', etc.
    ProductionStageName TEXT NOT NULL,     -- 'SLS Printing', 'CNC Machining', etc.
    ExecutionOrder INTEGER NOT NULL,       -- 1, 2, 3, etc.
    DefaultSetupMinutes INTEGER NOT NULL DEFAULT 30,
    DefaultTeardownMinutes INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1
);
```

### **1.2 Add Foreign Keys to Parts Table**
```sql
-- Add lookup foreign keys
ALTER TABLE Parts ADD COLUMN ComponentTypeId INTEGER;
ALTER TABLE Parts ADD COLUMN ComplianceCategoryId INTEGER;

-- Optional future enhancement fields
ALTER TABLE Parts ADD COLUMN IsLegacyForm INTEGER NOT NULL DEFAULT 1; -- Track migration status

-- Add foreign key constraints (if SQLite supports, otherwise enforce in code)
-- FK to ComponentTypes and ComplianceCategories
```

### **1.3 Seed Legacy Flag Mapping Data**
```sql
-- Populate the mapping table for automated migration
INSERT INTO LegacyFlagToStageMap (LegacyFieldName, ProductionStageName, ExecutionOrder, DefaultSetupMinutes, DefaultTeardownMinutes) VALUES
('RequiresSLSPrinting', 'SLS Printing', 1, 45, 30),
('RequiresEDMOperations', 'EDM Operations', 2, 30, 15),
('RequiresCNCMachining', 'CNC Machining', 3, 60, 20),
('RequiresAssembly', 'Assembly', 4, 15, 10),
('RequiresFinishing', 'Finishing', 5, 30, 15);
```

### **1.4 Create Services**
- `ComponentTypeService.cs` - CRUD for component types
- `ComplianceCategoryService.cs` - CRUD for compliance categories  
- `PartAssetService.cs` - Asset management (upload, validation, cleanup)

### **1.5 Seed Lookup Data**
```csharp
// ComponentTypes
General, Serialized

// ComplianceCategories  
Non NFA, NFA
```

## **Phase 2: Data Migration** (Critical Phase)
*Timeline: 1 day*

### **2.1 Pre-Migration Backup and Schema Snapshot**
```sql
-- Create migration snapshots directory if not exists
-- mkdir -p Migrations/Snapshots

-- Export current schema before migration
-- .output Migrations/Snapshots/pre_migration_schema.sql
-- .schema
-- .output stdout

-- Store current state for rollback capability
CREATE TABLE Migration_Backup_Parts AS SELECT * FROM Parts;
CREATE TABLE Migration_Backup_PartStageRequirements AS SELECT * FROM PartStageRequirements;
```

### **2.2 Improved ComponentType Data Migration**
```sql
BEGIN TRANSACTION;

-- Pre-check: Verify ProductionStages exist
SELECT 'Pre-check: Production Stages' as CheckType, COUNT(*) as Count 
FROM ProductionStages 
WHERE Name IN ('SLS Printing', 'EDM Operations', 'CNC Machining', 'Assembly', 'Finishing');

-- Sanitize and migrate ComponentType data with improved logic
UPDATE Parts 
SET ComponentTypeId = CASE 
    WHEN LOWER(IFNULL(ComponentType, '')) IN ('general', '') THEN 1
    WHEN LOWER(IFNULL(ComponentType, '')) = 'serialized' THEN 2
    ELSE 1 -- Default to General for any unrecognized values
END;

-- Migrate ComplianceCategory data (add logic based on existing regulatory fields)
UPDATE Parts 
SET ComplianceCategoryId = CASE 
    WHEN RequiresATFCompliance = 1 OR RequiresITARCompliance = 1 OR RequiresSerialization = 1 THEN 2 -- NFA
    ELSE 1 -- Non NFA
END;

COMMIT;
```

### **2.3 Enhanced Stage Boolean Flag Migration**
```sql
BEGIN TRANSACTION;

-- Use the mapping table for automated, maintainable migration
INSERT INTO PartStageRequirements (
    PartId, 
    ProductionStageId, 
    ExecutionOrder, 
    EstimatedHours, 
    SetupTimeMinutes,
    TeardownTimeMinutes,
    IsRequired, 
    IsActive, 
    CreatedBy, 
    CreatedDate,
    LastModifiedBy,
    LastModifiedDate
)
SELECT 
    p.Id as PartId,
    ps.Id as ProductionStageId,
    lfm.ExecutionOrder,
    COALESCE(p.EstimatedHours / (SELECT COUNT(*) FROM LegacyFlagToStageMap WHERE IsActive = 1), 2.0) as EstimatedHours, -- Distribute estimated hours across stages
    lfm.DefaultSetupMinutes,
    lfm.DefaultTeardownMinutes,
    1 as IsRequired,
    1 as IsActive,
    'Migration' as CreatedBy,
    datetime('now') as CreatedDate,
    'Migration' as LastModifiedBy,
    datetime('now') as LastModifiedDate
FROM Parts p
CROSS JOIN LegacyFlagToStageMap lfm
INNER JOIN ProductionStages ps ON ps.Name = lfm.ProductionStageName
WHERE lfm.IsActive = 1
  AND (
    (lfm.LegacyFieldName = 'RequiresSLSPrinting' AND p.RequiresSLSPrinting = 1) OR
    (lfm.LegacyFieldName = 'RequiresCNCMachining' AND p.RequiresCNCMachining = 1) OR
    (lfm.LegacyFieldName = 'RequiresEDMOperations' AND p.RequiresEDMOperations = 1) OR
    (lfm.LegacyFieldName = 'RequiresAssembly' AND p.RequiresAssembly = 1) OR
    (lfm.LegacyFieldName = 'RequiresFinishing' AND p.RequiresFinishing = 1)
  );

-- Mark parts as migrated
UPDATE Parts SET IsLegacyForm = 0 WHERE Id IN (
    SELECT DISTINCT PartId FROM PartStageRequirements WHERE CreatedBy = 'Migration'
);

COMMIT;
```

### **2.4 Enhanced Data Integrity Verification**
```sql
-- Comprehensive verification queries
SELECT 'Migration Summary' as CheckType,
    COUNT(DISTINCT p.Id) as TotalParts,
    COUNT(DISTINCT CASE WHEN p.IsLegacyForm = 0 THEN p.Id END) as MigratedParts,
    COUNT(DISTINCT psr.PartId) as PartsWithStages
FROM Parts p
LEFT JOIN PartStageRequirements psr ON p.Id = psr.PartId AND psr.IsActive = 1;

-- Detailed verification per part
SELECT 
    p.PartNumber,
    p.IsLegacyForm,
    COUNT(psr.Id) as StageCount,
    GROUP_CONCAT(ps.Name) as RequiredStages,
    SUM(psr.EstimatedHours) as TotalEstimatedHours
FROM Parts p
LEFT JOIN PartStageRequirements psr ON p.Id = psr.PartId AND psr.IsActive = 1
LEFT JOIN ProductionStages ps ON psr.ProductionStageId = ps.Id
GROUP BY p.Id, p.PartNumber, p.IsLegacyForm
ORDER BY p.PartNumber;
```

## **Phase 2.5: Migration Verification Tools** (NEW)
*Timeline: 0.5 day*

### **2.5.1 Create Migration Debug View**
```sql
-- Create a view for ongoing migration status monitoring
CREATE VIEW vw_MigratedPartSummary AS
SELECT 
    p.Id,
    p.PartNumber,
    p.Name,
    p.IsLegacyForm,
    p.ComponentTypeId,
    ct.Name as ComponentTypeName,
    p.ComplianceCategoryId,
    cc.Name as ComplianceCategoryName,
    COUNT(psr.Id) as StageCount,
    SUM(CASE WHEN psr.EstimatedHours IS NOT NULL THEN psr.EstimatedHours ELSE 0 END) as TotalStageHours,
    COUNT(pal.Id) as AssetCount,
    CASE 
        WHEN p.IsLegacyForm = 1 THEN 'Legacy Form'
        WHEN COUNT(psr.Id) = 0 THEN 'No Stages Assigned'
        WHEN p.ComponentTypeId IS NULL THEN 'Missing Component Type'
        WHEN p.ComplianceCategoryId IS NULL THEN 'Missing Compliance Category'
        ELSE 'Migrated Successfully'
    END as MigrationStatus
FROM Parts p
LEFT JOIN ComponentTypes ct ON p.ComponentTypeId = ct.Id
LEFT JOIN ComplianceCategories cc ON p.ComplianceCategoryId = cc.Id
LEFT JOIN PartStageRequirements psr ON p.Id = psr.PartId AND psr.IsActive = 1
LEFT JOIN PartAssetLinks pal ON p.Id = pal.PartId AND pal.IsActive = 1
GROUP BY p.Id;
```

### **2.5.2 Add Debug Admin Page (Suggestion)**
```csharp
// SUGGESTION: Create /Admin/Debug/RefactorStatus.cshtml page
// This page would display:
// - Parts missing stage assignments
// - Orphaned asset checks  
// - Part distribution by compliance/category
// - Migration progress metrics

public class RefactorStatusModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        var migrationStats = await _context.Database.SqlQueryRaw<MigrationSummary>(@"
            SELECT 
                COUNT(*) as TotalParts,
                SUM(CASE WHEN IsLegacyForm = 0 THEN 1 ELSE 0 END) as MigratedParts,
                COUNT(DISTINCT psr.PartId) as PartsWithStages,
                COUNT(DISTINCT pal.PartId) as PartsWithAssets
            FROM Parts p
            LEFT JOIN PartStageRequirements psr ON p.Id = psr.PartId AND psr.IsActive = 1
            LEFT JOIN PartAssetLinks pal ON p.Id = pal.PartId AND pal.IsActive = 1
        ").FirstOrDefaultAsync();

        ViewData["MigrationStats"] = migrationStats;
        return Page();
    }
}
```

## **Phase 3: Form Modernization** (User-Facing Changes)
*Timeline: 2-3 days*

### **3.1 Update Part Form UI**

#### **Remove Legacy Fields**
```html
<!-- REMOVE from _PartForm.cshtml -->
<div class="form-group">
    <label asp-for="Industry">Industry</label>
    <select asp-for="Industry" class="form-control">
        <!-- Remove entire dropdown -->
    </select>
</div>

<div class="form-group">
    <label asp-for="Application">Application</label>
    <input asp-for="Application" class="form-control" />
    <!-- Remove entire field -->
</div>

<div class="form-group">
    <label asp-for="PartClass">Part Class</label>
    <select asp-for="PartClass" class="form-control">
        <!-- Remove entire dropdown -->
    </select>
</div>

<!-- Remove all stage boolean checkboxes -->
<div class="form-check">
    <input asp-for="RequiresSLSPrinting" class="form-check-input" type="checkbox" />
    <label asp-for="RequiresSLSPrinting" class="form-check-label">Requires SLS Printing</label>
    <!-- Remove ~15 similar checkboxes -->
</div>
```

#### **Add New Lookup Fields**
```html
<!-- ADD to _PartForm.cshtml -->
<div class="form-group">
    <label asp-for="ComponentTypeId">Component Type</label>
    <select asp-for="ComponentTypeId" class="form-control" asp-items="ViewBag.ComponentTypes">
        <option value="">Select Component Type</option>
    </select>
    <span asp-validation-for="ComponentTypeId" class="text-danger"></span>
</div>

<div class="form-group">
    <label asp-for="ComplianceCategoryId">Compliance Category</label>
    <select asp-for="ComplianceCategoryId" class="form-control" asp-items="ViewBag.ComplianceCategories">
        <option value="">Select Compliance Category</option>
    </select>
    <span asp-validation-for="ComplianceCategoryId" class="text-danger"></span>
</div>
```

#### **Add Assets Management Section**
```html
<!-- ADD new Assets section with modern approach -->
<div class="card mt-4" x-data="assetManager()">
    <div class="card-header">
        <h5>?? Part Assets</h5>
    </div>
    <div class="card-body">
        <div id="assets-container">
            <!-- NOTE: Consider migrating to Alpine.js for better state management -->
            <div class="asset-upload-area">
                <input type="file" id="asset-upload" accept=".step,.stp,.stl,.jpg,.jpeg,.png,.pdf" multiple>
                <label for="asset-upload" class="btn btn-outline-primary">
                    <i class="fas fa-upload"></i> Upload 3D Models/Photos
                </label>
            </div>
            <div id="existing-assets">
                <!-- Existing assets listed here -->
            </div>
        </div>
    </div>
</div>
```

### **3.2 Enhanced Stage Requirements Section**
```html
<!-- REPLACE stage checkboxes with dynamic stage manager -->
<!-- SUGGESTION: Consider HTMX for better server-side integration -->
<div class="card mt-4" hx-get="/api/parts/stage-requirements" hx-trigger="load">
    <div class="card-header d-flex justify-content-between">
        <h5>?? Manufacturing Stages</h5>
        <button type="button" class="btn btn-sm btn-success" id="add-stage-btn">
            <i class="fas fa-plus"></i> Add Stage
        </button>
    </div>
    <div class="card-body">
        <div id="stage-requirements-container">
            <!-- Dynamic stage requirements loaded via HTMX/AJAX -->
        </div>
    </div>
</div>
```

### **3.3 Update JavaScript (Enhanced)**

#### **Remove Legacy Stage Logic**
```javascript
// REMOVE from parts-form.js
function updateStageRequirements() {
    // Remove boolean-based stage logic
}

// REMOVE individual stage change handlers
$('#RequiresSLSPrinting').change(function() { ... });
$('#RequiresCNCMachining').change(function() { ... });
// etc.
```

#### **Add New Lookup and Stage Logic (Modern Approach)**
```javascript
// ENHANCED: Consider migrating from jQuery to Alpine.js for better reactivity
// ADD to parts-form.js
class PartFormManager {
    constructor() {
        this.initializeLookupHandlers();
        this.initializeStageManager();
        this.initializeAssetManager();
    }

    initializeLookupHandlers() {
        $('#ComponentTypeId').change(() => this.onComponentTypeChange());
        $('#ComplianceCategoryId').change(() => this.onComplianceCategoryChange());
    }

    initializeStageManager() {
        $('#add-stage-btn').click(() => this.showAddStageModal());
        this.loadExistingStageRequirements();
    }

    initializeAssetManager() {
        $('#asset-upload').change((e) => this.handleAssetUpload(e));
    }

    async loadExistingStageRequirements() {
        const partId = $('#Id').val();
        if (partId) {
            try {
                const response = await fetch(`/api/parts/${partId}/stages`);
                if (!response.ok) throw new Error(`HTTP ${response.status}`);
                const stages = await response.json();
                this.renderStageRequirements(stages);
            } catch (error) {
                console.error('Failed to load stage requirements:', error);
                this.showError('Failed to load stage requirements');
            }
        }
    }

    renderStageRequirements(stages) {
        const container = $('#stage-requirements-container');
        container.empty();
        
        stages.forEach(stage => {
            const stageHtml = this.createStageRequirementHtml(stage);
            container.append(stageHtml);
        });
    }

    // SUGGESTION: Consider Alpine.js component instead
    createStageRequirementHtml(stage) {
        return `
            <div class="stage-requirement-item" data-stage-id="${stage.id}">
                <div class="d-flex justify-content-between align-items-center">
                    <div>
                        <strong>${stage.productionStage.name}</strong>
                        <small class="text-muted">(${stage.estimatedHours}h + ${stage.setupTimeMinutes}min setup + ${stage.teardownTimeMinutes || 0}min teardown)</small>
                    </div>
                    <button type="button" class="btn btn-sm btn-outline-danger" onclick="this.removeStage(${stage.id})">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
        `;
    }

    showError(message) {
        // TODO: Implement proper toast/notification system
        alert(message);
    }
}

// Initialize on document ready
$(document).ready(() => {
    new PartFormManager();
});
```

### **3.4 Update Page Model**
```csharp
// UPDATE Parts.cshtml.cs
public async Task<IActionResult> OnGetAsync(int? id)
{
    // Load lookup data
    ViewData["ComponentTypes"] = new SelectList(
        await _componentTypeService.GetActiveComponentTypesAsync(),
        "Id", "Name");
        
    ViewData["ComplianceCategories"] = new SelectList(
        await _complianceCategoryService.GetActiveCategoriesAsync(),
        "Id", "Name");

    if (id.HasValue)
    {
        Part = await _partService.GetPartByIdAsync(id.Value);
        if (Part == null) return NotFound();
        
        // Load existing stage requirements
        var stages = await _partStageService.GetPartStagesWithDetailsAsync(Part.Id);
        ViewData["ExistingStages"] = stages;
    }
    else
    {
        Part = new Part();
    }

    return Page();
}
```

## **Phase 4: Database Cleanup** (Breaking Changes)
*Timeline: 1 day*

### **4.1 Pre-Cleanup Schema Snapshot**
```sql
-- Export schema after migration, before cleanup
-- .output Migrations/Snapshots/post_migration_pre_cleanup_schema.sql
-- .schema
-- .output stdout
```

### **4.2 Remove Legacy Columns**
```sql
-- Remove stage boolean columns (after confirming migration success)
ALTER TABLE Parts DROP COLUMN RequiresSLSPrinting;
ALTER TABLE Parts DROP COLUMN RequiresCNCMachining;
ALTER TABLE Parts DROP COLUMN RequiresEDMOperations;
ALTER TABLE Parts DROP COLUMN RequiresAssembly;
ALTER TABLE Parts DROP COLUMN RequiresFinishing;
-- Remove ~15 more stage-related boolean columns

-- Remove other deprecated fields
ALTER TABLE Parts DROP COLUMN Industry;
ALTER TABLE Parts DROP COLUMN Application;
ALTER TABLE Parts DROP COLUMN PartClass;
ALTER TABLE Parts DROP COLUMN QualityStandards;

-- IMPORTANT: Keep EstimatedHours during transition to prevent scheduler breakage
-- Will be removed in Phase 5 once stage-based logic is verified and tested
-- ALTER TABLE Parts DROP COLUMN EstimatedHours; -- DEFER to Phase 5
```

### **4.3 Update Part.cs Model (Enhanced)**
```csharp
// REMOVE from Part.cs
public bool RequiresSLSPrinting { get; set; }
public bool RequiresCNCMachining { get; set; }
public bool RequiresEDMOperations { get; set; }
// Remove ~15 stage boolean properties

public string Industry { get; set; }
public string Application { get; set; }
public string PartClass { get; set; }
public string QualityStandards { get; set; }

// ADD new foreign key properties
public int? ComponentTypeId { get; set; }
public int? ComplianceCategoryId { get; set; }

// Optional future enhancement fields
public bool IsLegacyForm { get; set; } = true; // Track migration status

// ADD navigation properties
public virtual ComponentType? ComponentType { get; set; }
public virtual ComplianceCategory? ComplianceCategory { get; set; }
public virtual ICollection<PartAssetLink> AssetLinks { get; set; } = new List<PartAssetLink>();
```

### **4.4 Update DbContext (Enhanced)**
```csharp
// ADD to SchedulerContext.cs
public DbSet<ComponentType> ComponentTypes { get; set; }
public DbSet<ComplianceCategory> ComplianceCategories { get; set; }
public DbSet<PartAssetLink> PartAssetLinks { get; set; }
public DbSet<LegacyFlagToStageMap> LegacyFlagToStageMaps { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configure relationships
    modelBuilder.Entity<Part>()
        .HasOne(p => p.ComponentType)
        .WithMany()
        .HasForeignKey(p => p.ComponentTypeId);

    modelBuilder.Entity<Part>()
        .HasOne(p => p.ComplianceCategory)
        .WithMany()
        .HasForeignKey(p => p.ComplianceCategoryId);

    modelBuilder.Entity<PartAssetLink>()
        .HasOne(pal => pal.Part)
        .WithMany(p => p.AssetLinks)
        .HasForeignKey(pal => pal.PartId);

    // ENHANCED: Handle legacy field mapping during EF model cleanup
    modelBuilder.Entity<Part>()
        .Ignore(p => p.RequiresSLSPrinting)
        .Ignore(p => p.RequiresCNCMachining)
        .Ignore(p => p.RequiresEDMOperations)
        .Ignore(p => p.RequiresAssembly)
        .Ignore(p => p.RequiresFinishing)
        .Ignore(p => p.Industry)
        .Ignore(p => p.Application)
        .Ignore(p => p.PartClass)
        .Ignore(p => p.QualityStandards);

    // Use HasColumnName if database column names differ from property names
    modelBuilder.Entity<Part>()
        .Property(p => p.ComponentTypeId)
        .HasColumnName("ComponentTypeId");
}
```

### **4.5 Post-Cleanup Schema Snapshot**
```sql
-- Export final schema after cleanup
-- .output Migrations/Snapshots/post_cleanup_final_schema.sql
-- .schema
-- .output stdout
```

## **Phase 5: Enhanced Features** (Optional Improvements)
*Timeline: 2-3 days**

### **5.1 Enhanced TeardownMinutes Support**
```csharp
// ENHANCED: Add to PartStageRequirement.cs if not already present
public int? TeardownTimeMinutes { get; set; }

// OPTIONAL: Add material cost override capability
public decimal? OverrideMaterialCost { get; set; }

// ENHANCED: Update CalculateTotalEstimatedCost method
public decimal CalculateTotalEstimatedCost()
{
    var hours = (decimal)GetEffectiveEstimatedHours();
    var rate = GetEffectiveHourlyRate();
    var setupMinutes = SetupTimeMinutes ?? ProductionStage?.DefaultSetupMinutes ?? 30;
    var teardownMinutes = TeardownTimeMinutes ?? 0;
    
    var setupCost = (decimal)(setupMinutes / 60.0) * rate;
    var teardownCost = (decimal)(teardownMinutes / 60.0) * rate;
    var materialCost = OverrideMaterialCost ?? MaterialCost;
    
    return setupCost + (hours * rate) + teardownCost + materialCost;
}
```

### **5.2 PartStageSLS Specialized Table**
```sql
-- IF SLS-specific parameters are needed
CREATE TABLE PartStageSLS (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PartStageRequirementId INTEGER NOT NULL,
    LaserPowerWatts REAL NOT NULL DEFAULT 170.0,
    ScanSpeedMmPerSec REAL NOT NULL DEFAULT 1000.0,
    LayerThicknessMicrons REAL NOT NULL DEFAULT 30.0,
    HatchSpacingMicrons REAL NOT NULL DEFAULT 120.0,
    PowderType TEXT NOT NULL DEFAULT 'Ti-6Al-4V Grade 5',
    ArgonFlowRate REAL NOT NULL DEFAULT 15.0,
    BuildTemperatureCelsius REAL NOT NULL DEFAULT 180.0,
    CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
    CreatedBy TEXT NOT NULL DEFAULT 'System',
    FOREIGN KEY (PartStageRequirementId) REFERENCES PartStageRequirements(Id) ON DELETE CASCADE
);
```

### **5.3 Enhanced Scheduler Integration**
```csharp
// ENHANCED SchedulerService to use new stage structure with proper time calculation
public async Task<List<JobViewModel>> GetScheduledJobsAsync()
{
    var jobs = await _context.Jobs
        .Include(j => j.Part)
            .ThenInclude(p => p.PartStageRequirements)
                .ThenInclude(psr => psr.ProductionStage)
        .Where(j => j.IsActive)
        .ToListAsync();

    return jobs.Select(job => new JobViewModel
    {
        // ENHANCED: Use PlannedMinutes + Setup + Teardown calculation
        EstimatedDuration = job.Part.PartStageRequirements
            .Where(psr => psr.IsActive)
            .Sum(psr => {
                var plannedMinutes = psr.EstimatedHours * 60.0;
                var setupMinutes = psr.SetupTimeMinutes ?? psr.ProductionStage?.DefaultSetupMinutes ?? 30;
                var teardownMinutes = psr.TeardownTimeMinutes ?? 0;
                return (plannedMinutes + setupMinutes + teardownMinutes) / 60.0;
            }),
        
        // Use stage-based machine requirements
        PreferredMachines = job.Part.PartStageRequirements
            .Where(psr => psr.RequiresSpecificMachine)
            .Select(psr => psr.AssignedMachineId)
            .Where(mid => !string.IsNullOrEmpty(mid))
            .ToList()),

        // ENHANCED: Use hourly rate overrides if present
        EstimatedCost = job.Part.PartStageRequirements
            .Where(psr => psr.IsActive)
            .Sum(psr => psr.CalculateTotalEstimatedCost())
    }).ToList();
}

// IMPORTANT: Retain EstimatedHours field during transition to prevent scheduler breakage
// Remove Part.EstimatedHours only after stage-based logic is fully verified and tested
public async Task<bool> VerifyStageBasedCalculations()
{
    var partsWithStages = await _context.Parts
        .Where(p => p.PartStageRequirements.Any(psr => psr.IsActive))
        .Include(p => p.PartStageRequirements)
            .ThenInclude(psr => psr.ProductionStage)
        .ToListAsync();

    var discrepancies = new List<string>();
    
    foreach (var part in partsWithStages)
    {
        var legacyEstimate = part.EstimatedHours;
        var stageBasedEstimate = part.PartStageRequirements
            .Where(psr => psr.IsActive)
            .Sum(psr => psr.GetEffectiveEstimatedHours() + 
                   (psr.SetupTimeMinutes ?? 30) / 60.0 +
                   (psr.TeardownTimeMinutes ?? 0) / 60.0);

        var variance = Math.Abs(legacyEstimate - stageBasedEstimate);
        if (variance > 0.5) // More than 30 minutes difference
        {
            discrepancies.Add($"Part {part.PartNumber}: Legacy={legacyEstimate}h, Stage-based={stageBasedEstimate}h");
        }
    }
    
    if (discrepancies.Any())
    {
        _logger.LogWarning("Stage-based calculation discrepancies found: {Discrepancies}", 
            string.Join("; ", discrepancies));
        return false;
    }
    
    return true;
}
```

---

## ? **Phase 6: Advanced Operator Tools** (Optional Enhancements)
*Timeline: 3-5 days*  
*?? **IMPORTANT**: Implement only AFTER successful deployment of core refactor (Phases 1–5)*

### **6.1 — Operator Notes Per Stage**
Add stage-specific operator communication and documentation.

**Database Schema:**
```sql
CREATE TABLE PartStageNotes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PartId INTEGER NOT NULL,
    ProductionStageId INTEGER NOT NULL,
    JobId INTEGER, -- Optional link to specific job instance
    Note TEXT NOT NULL,
    NoteType TEXT NOT NULL DEFAULT 'General', -- 'General', 'QC', 'Issue', 'Setup'
    IsResolved INTEGER NOT NULL DEFAULT 0,
    CreatedBy TEXT NOT NULL,
    CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
    LastModifiedBy TEXT,
    LastModifiedDate TEXT,
    FOREIGN KEY (PartId) REFERENCES Parts(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductionStageId) REFERENCES ProductionStages(Id) ON DELETE CASCADE,
    FOREIGN KEY (JobId) REFERENCES Jobs(Id) ON DELETE SET NULL
);
```

**Use Cases:**
- Shift communication about part-specific issues
- QC failure documentation and resolution tracking
- Setup notes for complex parts
- Historical knowledge retention

---

### **6.2 — Material and Tooling Requirements**
Extend stage requirements with material and tooling specifications.

**Database Schema Enhancement:**
```sql
-- Add to existing PartStageRequirement table
ALTER TABLE PartStageRequirements ADD COLUMN RequiredMaterial TEXT NULL;
ALTER TABLE PartStageRequirements ADD COLUMN ToolingNotes TEXT NULL;
ALTER TABLE PartStageRequirements ADD COLUMN SpecialInstructions TEXT NULL;
```

**Use Cases:**
- Special powder requirements for SLS printing
- Custom jigs and fixturing specifications
- Material staging and preparation notes
- Safety requirements and handling instructions

---

### **6.3 — Checklist Templates per Stage**
Create reusable procedural checklists tied to production stages.

**Database Schema:**
```sql
CREATE TABLE StageChecklistTemplates (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductionStageId INTEGER NOT NULL,
    Title TEXT NOT NULL,
    Description TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedBy TEXT NOT NULL,
    CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (ProductionStageId) REFERENCES ProductionStages(Id) ON DELETE CASCADE
);

CREATE TABLE ChecklistItems (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TemplateId INTEGER NOT NULL,
    StepDescription TEXT NOT NULL,
    IsRequired INTEGER NOT NULL DEFAULT 1,
    SortOrder INTEGER NOT NULL DEFAULT 100,
    EstimatedMinutes INTEGER,
    SafetyNote TEXT,
    CreatedBy TEXT NOT NULL,
    CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (TemplateId) REFERENCES StageChecklistTemplates(Id) ON DELETE CASCADE
);

-- Link checklists to specific part stage requirements
CREATE TABLE PartStageChecklists (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PartStageRequirementId INTEGER NOT NULL,
    ChecklistTemplateId INTEGER NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (PartStageRequirementId) REFERENCES PartStageRequirements(Id) ON DELETE CASCADE,
    FOREIGN KEY (ChecklistTemplateId) REFERENCES StageChecklistTemplates(Id) ON DELETE CASCADE
);
```

**Use Cases:**
- Standardized QC procedures
- Safety verification steps
- Machine setup and calibration checklists
- Training and consistency enforcement

---

### **6.4 — Audit Log for Metadata Changes**
Comprehensive change tracking for critical manufacturing data.

**Database Schema:**
```sql
CREATE TABLE AuditLog (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EntityName TEXT NOT NULL, -- 'Part', 'PartStageRequirement', 'ProductionStage'
    EntityId INTEGER NOT NULL,
    FieldName TEXT NOT NULL,
    OldValue TEXT,
    NewValue TEXT,
    ChangeType TEXT NOT NULL, -- 'Create', 'Update', 'Delete'
    ChangedBy TEXT NOT NULL,
    ChangedUtc TEXT NOT NULL DEFAULT (datetime('now')),
    IPAddress TEXT,
    UserAgent TEXT,
    Reason TEXT -- Optional justification for change
);

-- Index for performance
CREATE INDEX IX_AuditLog_Entity ON AuditLog(EntityName, EntityId);
CREATE INDEX IX_AuditLog_Date ON AuditLog(ChangedUtc);
```

**Use Cases:**
- Track changes to part setup times and costs
- Monitor stage requirement modifications
- Compliance and traceability requirements
- Root cause analysis for production issues

---

### **6.5 — Export Part/Stage Requirements as CSV**
Data export functionality for external systems and reporting.

**API Endpoint:**
```csharp
[HttpGet("api/parts/export")]
public async Task<IActionResult> ExportPartStageRequirements(
    [FromQuery] string format = "csv",
    [FromQuery] int[] partIds = null,
    [FromQuery] bool includeInactive = false)
{
    var query = _context.Parts
        .Include(p => p.PartStageRequirements)
            .ThenInclude(psr => psr.ProductionStage)
        .AsQueryable();

    if (partIds?.Any() == true)
        query = query.Where(p => partIds.Contains(p.Id));

    if (!includeInactive)
        query = query.Where(p => p.PartStageRequirements.Any(psr => psr.IsActive));

    var data = await query.SelectMany(p => p.PartStageRequirements
        .Where(psr => includeInactive || psr.IsActive)
        .Select(psr => new PartStageExportDto
        {
            PartNumber = p.PartNumber,
            PartName = p.Name,
            StageName = psr.ProductionStage.Name,
            ExecutionOrder = psr.ExecutionOrder,
            EstimatedHours = psr.EstimatedHours,
            SetupMinutes = psr.SetupTimeMinutes,
            TeardownMinutes = psr.TeardownTimeMinutes,
            RequiredMaterial = psr.RequiredMaterial,
            ToolingNotes = psr.ToolingNotes,
            HourlyRate = psr.HourlyRateOverride ?? psr.ProductionStage.DefaultHourlyRate
        }))
        .ToListAsync();

    return File(GenerateCsv(data), "text/csv", $"part-stage-requirements-{DateTime.Now:yyyyMMdd}.csv");
}
```

**Use Cases:**
- Job traveler creation
- External backup and archival
- ERP system integration
- Offline analysis and reporting

---

### **6.6 — Flag for Admin Review**
Allow users to flag parts or stages that need administrative attention.

**Database Schema Enhancement:**
```sql
-- Add to existing Parts table
ALTER TABLE Parts ADD COLUMN FlagForReview INTEGER NOT NULL DEFAULT 0;
ALTER TABLE Parts ADD COLUMN ReviewComment TEXT NULL;
ALTER TABLE Parts ADD COLUMN FlaggedBy TEXT NULL;
ALTER TABLE Parts ADD COLUMN FlaggedDate TEXT NULL;

-- Add to existing PartStageRequirements table
ALTER TABLE PartStageRequirements ADD COLUMN FlagForReview INTEGER NOT NULL DEFAULT 0;
ALTER TABLE PartStageRequirements ADD COLUMN ReviewComment TEXT NULL;
ALTER TABLE PartStageRequirements ADD COLUMN FlaggedBy TEXT NULL;
ALTER TABLE PartStageRequirements ADD COLUMN FlaggedDate TEXT NULL;
```

**UI Enhancement:**
```html
<!-- Add to Parts dashboard -->
<div class="alert alert-warning" v-if="flaggedPartsCount > 0">
    <i class="fas fa-flag"></i>
    <strong>{{ flaggedPartsCount }}</strong> parts flagged for review
    <a href="/Admin/Parts?filter=flagged" class="btn btn-sm btn-outline-warning ms-2">
        Review Flagged Items
    </a>
</div>
```

**Use Cases:**
- User-initiated escalation for complex parts
- Quality issue reporting
- Setup time dispute resolution
- Process improvement suggestions

---

### **6.7 — Operator Tags**
Visual labeling system for parts with special handling requirements.

**Database Schema:**
```sql
CREATE TABLE PartTags (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Color TEXT NOT NULL DEFAULT '#6c757d', -- Bootstrap color codes
    Icon TEXT, -- Font Awesome icon class
    Description TEXT,
    Priority INTEGER NOT NULL DEFAULT 0, -- Higher = more prominent
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedBy TEXT NOT NULL,
    CreatedDate TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE PartTagAssignments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PartId INTEGER NOT NULL,
    TagId INTEGER NOT NULL,
    AssignedBy TEXT NOT NULL,
    AssignedDate TEXT NOT NULL DEFAULT (datetime('now')),
    ExpiresDate TEXT, -- Optional expiration
    FOREIGN KEY (PartId) REFERENCES Parts(Id) ON DELETE CASCADE,
    FOREIGN KEY (TagId) REFERENCES PartTags(Id) ON DELETE CASCADE,
    UNIQUE(PartId, TagId)
);

-- Seed common tags
INSERT INTO PartTags (Name, Color, Icon, Description, Priority) VALUES
('Titanium', '#dc3545', 'fas fa-atom', 'Titanium alloy parts requiring special handling', 3),
('Time Sensitive', '#fd7e14', 'fas fa-clock', 'Rush order - prioritize scheduling', 5),
('Do Not Stack', '#198754', 'fas fa-layer-group', 'Parts that cannot be stacked in builds', 2),
('Fragile', '#ffc107', 'fas fa-fragile', 'Handle with extra care', 4),
('High Value', '#6f42c1', 'fas fa-gem', 'Expensive parts requiring special tracking', 3);
```

**UI Enhancement:**
```html
<!-- Display tags as badges in parts list and scheduler -->
<div class="part-tags">
    <span v-for="tag in part.tags" 
          :key="tag.id" 
          :class="'badge me-1'" 
          :style="{ backgroundColor: tag.color }">
        <i :class="tag.icon" v-if="tag.icon"></i>
        {{ tag.name }}
    </span>
</div>
```

**Use Cases:**
- Visual indicators in scheduler interface
- Special handling requirements communication
- Priority and risk level identification
- Quality and material categorization

---

### **Phase 6 Implementation Notes**

#### **Prerequisites:**
- ? Core refactor (Phases 1-5) successfully deployed
- ? System stability verified
- ? User adoption of new part form interface
- ? No critical issues from core implementation

#### **Implementation Priority:**
1. **High Priority**: Operator Notes (6.1), Material Requirements (6.2)
2. **Medium Priority**: Audit Log (6.4), Flag for Review (6.6)
3. **Low Priority**: Checklist Templates (6.3), CSV Export (6.5), Operator Tags (6.7)

#### **Integration Points:**
- **Scheduler**: Tags and flags should be visible in job scheduling interface
- **Mobile**: Operator notes and checklists need mobile-optimized interfaces
- **Reporting**: Audit logs and export functions integrate with existing analytics
- **API**: All features should have corresponding REST endpoints for future integrations

#### **Success Metrics for Phase 6:**
- **Operator Notes**: Reduced miscommunication incidents by 40%
- **Material Requirements**: Decreased setup time by 15%
- **Checklists**: Improved QC consistency score by 25%
- **Audit Log**: 100% traceability for critical data changes
- **Export**: Successful integration with at least 1 external system
- **Flags**: Average flag resolution time < 48 hours
- **Tags**: 90% of applicable parts properly tagged within 30 days

---

## ?? **SUPPORT & ESCALATION**

### **Technical Issues**
- **Database**: Check migration logs, verify data integrity queries, use debug views
- **Form Issues**: Browser developer tools, JavaScript console errors  
- **Performance**: SQL query analysis, index optimization
- **Scheduler**: Compare legacy vs stage-based calculations

### **User Issues**
- **Training**: Provide new form walkthrough and field mapping guide
- **Data**: Help users understand stage requirements vs old boolean flags
- **Workflow**: Assist with new part creation and stage assignment process
- **Migration Status**: Use /Admin/Debug/RefactorStatus page for progress tracking

### **Rollback Procedures**
If critical issues arise:
1. **Stop Migration**: Immediate halt of Phase 2+ if data corruption detected
2. **Restore Database**: Use Phase 1 backup to restore to pre-migration state
3. **Revert Code**: Roll back to previous Part.cs and form versions
4. **Use Schema Snapshots**: Reference pre-migration schema dumps for restoration
5. **User Communication**: Notify users of temporary reversion and timeline

---

## ? **READY FOR IMPLEMENTATION**

This enhanced plan provides:
- **Robust Migration Logic**: Transaction-wrapped, verified migration with mapping tables
- **Comprehensive Verification**: Debug views and admin pages for progress tracking
- **Enhanced Models**: Proper audit fields and future-ready enhancements
- **Modern Frontend**: Migration path to Alpine.js/HTMX for better UX
- **Schema Management**: Complete snapshots before/after each phase
- **Scheduler Safety**: Retain EstimatedHours until stage-based logic is verified
- **Clear Phase Structure**: Step-by-step implementation with minimal risk
- **Comprehensive Testing**: Unit, integration, and user acceptance testing
- **Risk Mitigation**: Detailed backup and rollback procedures  
- **Success Metrics**: Measurable outcomes for technical and business value
- **Future Roadmap**: Clear path for continued enhancement

**The OpCentrix Part Form & Stage System refactor is fully planned and ready for execution!** ??

---

*This document will be updated as implementation progresses and requirements evolve.*