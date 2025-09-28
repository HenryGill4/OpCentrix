# Phase 1: Emergency Database Fixes
**Priority: CRITICAL** | **Duration: 2-3 days** | **Risk: HIGH if not done first**

## Overview
Fix the fundamental database issues that are causing crashes and preventing the system from functioning. These are the showstopper issues that must be resolved before any other work can begin.

## Current Broken State
- ? PowderStock/PowderConsumption tables missing from database despite being in code models
- ? Foreign key constraints missing causing crashes on user lookups  
- ? PrinterEstimatedEndTime field missing from ProductionBuilds table
- ? Calculated duration columns still exist in MasterParts (should be removed)

## Success Criteria
- ? System doesn't crash when trying to track powder usage
- ? User lookups work without crashing
- ? PrinterEstimatedEndTime can be stored in ProductionBuilds
- ? No calculated time fields remain in MasterParts table
- ? All foreign key relationships work properly

---

## Step-by-Step Implementation

### Step 1: Add Missing Powder Tables to SchedulerContext
**Duration: 30 minutes**

#### 1.1 Update SchedulerContext.cs
Add these DbSet properties to `OpCentrix/Data/SchedulerContext.cs`:

```csharp
// Add to the existing DbSet properties section
public DbSet<PowderStock> PowderStock { get; set; }
public DbSet<PowderConsumption> PowderConsumption { get; set; }
```

#### 1.2 Add Model Configuration
Add to the `OnModelCreating` method in SchedulerContext.cs:

```csharp
private void ConfigurePowderInventoryEntities(ModelBuilder modelBuilder)
{
    // PowderStock configuration
    modelBuilder.Entity<PowderStock>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.MaterialType).HasMaxLength(100).IsRequired();
        entity.Property(e => e.CurrentStock).HasColumnType("decimal(8,2)");
        entity.Property(e => e.ReorderPoint).HasColumnType("decimal(8,2)");
        entity.Property(e => e.AlertThreshold).HasColumnType("decimal(8,2)");
        entity.Property(e => e.CostPerKg).HasColumnType("decimal(8,2)");
        entity.HasIndex(e => e.MaterialType).IsUnique();
    });

    // PowderConsumption configuration  
    modelBuilder.Entity<PowderConsumption>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.MaterialType).HasMaxLength(100).IsRequired();
        entity.Property(e => e.AmountUsed).HasColumnType("decimal(6,2)");
        entity.Property(e => e.CostAllocated).HasColumnType("decimal(8,2)");
        entity.HasOne<ProductionBuild>()
              .WithMany()
              .HasForeignKey(e => e.ProductionBuildId)
              .OnDelete(DeleteBehavior.Cascade);
    });
}
```

And call it in OnModelCreating:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... existing calls ...
    ConfigurePowderInventoryEntities(modelBuilder);
}
```

### Step 2: Create Database Migration Script
**Duration: 45 minutes**

#### 2.1 Create Migration File
Create `OpCentrix/Data/Migrations/Phase1_EmergencyFixes.sql`:

```sql
-- OpCentrix Emergency Database Fixes - Phase 1
-- Purpose: Add missing tables and fix critical constraints
-- Created: January 2025

PRAGMA foreign_keys = OFF;

-- =============================================================================
-- ADD MISSING POWDER INVENTORY TABLES
-- =============================================================================

-- Create PowderStock table
CREATE TABLE IF NOT EXISTS "PowderStock" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PowderStock" PRIMARY KEY AUTOINCREMENT,
    "MaterialType" TEXT NOT NULL,
    "CurrentStock" decimal(8,2) NOT NULL DEFAULT 0.00,
    "ReorderPoint" decimal(8,2) NOT NULL DEFAULT 10.00,
    "AlertThreshold" decimal(8,2) NOT NULL DEFAULT 0.25,
    "CostPerKg" decimal(8,2) NOT NULL DEFAULT 0.00,
    "LastRestocked" TEXT NULL,
    "LastUpdated" TEXT NOT NULL DEFAULT (datetime('now')),
    "LowStockAlert" INTEGER NOT NULL DEFAULT 0
);

-- Create unique index on MaterialType
CREATE UNIQUE INDEX "IX_PowderStock_MaterialType" ON "PowderStock" ("MaterialType");

-- Create PowderConsumption table
CREATE TABLE IF NOT EXISTS "PowderConsumption" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PowderConsumption" PRIMARY KEY AUTOINCREMENT,
    "ProductionBuildId" INTEGER NOT NULL,
    "MaterialType" TEXT NOT NULL,
    "AmountUsed" decimal(6,2) NOT NULL,
    "CostAllocated" decimal(8,2) NOT NULL,
    "UsageDate" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_PowderConsumption_ProductionBuilds_ProductionBuildId" 
        FOREIGN KEY ("ProductionBuildId") REFERENCES "ProductionBuilds" ("Id") ON DELETE CASCADE
);

-- Create indexes for PowderConsumption
CREATE INDEX "IX_PowderConsumption_ProductionBuildId" ON "PowderConsumption" ("ProductionBuildId");
CREATE INDEX "IX_PowderConsumption_MaterialType" ON "PowderConsumption" ("MaterialType");
CREATE INDEX "IX_PowderConsumption_UsageDate" ON "PowderConsumption" ("UsageDate");

-- =============================================================================
-- ADD MISSING FIELD TO PRODUCTIONBUILDS
-- =============================================================================

-- Add PrinterEstimatedEndTime field if it doesn't exist
ALTER TABLE ProductionBuilds ADD COLUMN PrinterEstimatedEndTime TEXT NULL;

-- =============================================================================
-- FIX FOREIGN KEY CONSTRAINTS  
-- =============================================================================

-- Fix missing foreign key constraint for CreatedByUserId
-- Note: SQLite doesn't support adding constraints to existing tables
-- So we'll verify the relationship exists and log if there are orphaned records

-- Check for orphaned ProductionBuilds (builds without valid users)
SELECT 'WARNING: Orphaned ProductionBuilds found' as Status, COUNT(*) as Count
FROM ProductionBuilds pb
LEFT JOIN Users u ON pb.CreatedByUserId = u.Id
WHERE u.Id IS NULL AND pb.CreatedByUserId > 0;

-- =============================================================================
-- REMOVE CALCULATED DURATION COLUMNS FROM MASTERPARTS
-- =============================================================================

-- Remove calculated duration columns (we'll rely on machine estimates instead)
ALTER TABLE MasterParts DROP COLUMN IF EXISTS SingleStackDurationHours;
ALTER TABLE MasterParts DROP COLUMN IF EXISTS DoubleStackDurationHours;
ALTER TABLE MasterParts DROP COLUMN IF EXISTS TripleStackDurationHours;

-- Note: If columns don't exist, SQLite will ignore the DROP statements

-- =============================================================================
-- SEED INITIAL POWDER INVENTORY DATA
-- =============================================================================

-- Insert default powder materials with zero stock (will be updated manually)
INSERT OR IGNORE INTO PowderStock (MaterialType, CurrentStock, ReorderPoint, CostPerKg, AlertThreshold)
VALUES 
    ('Ti-6Al-4V Grade 5', 0.00, 25.00, 285.00, 0.25),
    ('Inconel 718', 0.00, 15.00, 320.00, 0.25),
    ('Ti-6Al-4V ELI Grade 23', 0.00, 10.00, 295.00, 0.25),
    ('AlSi10Mg', 0.00, 20.00, 45.00, 0.25);

-- =============================================================================
-- VERIFICATION QUERIES
-- =============================================================================

-- Verify tables were created
SELECT name FROM sqlite_master WHERE type='table' AND name IN ('PowderStock', 'PowderConsumption');

-- Verify ProductionBuilds has new field
PRAGMA table_info(ProductionBuilds);

-- Verify powder materials were inserted
SELECT MaterialType, CurrentStock, CostPerKg FROM PowderStock;

-- Check MasterParts columns (calculated duration columns should be gone)
PRAGMA table_info(MasterParts);

PRAGMA foreign_keys = ON;

-- Migration complete
SELECT 'Phase 1 Emergency Database Fixes Complete!' as Status,
       datetime('now') as CompletedAt;
```

### Step 3: Create Powder Inventory Models
**Duration: 30 minutes**

#### 3.1 Create PowderInventoryModels.cs
Create `OpCentrix/Models/PowderInventoryModels.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Powder stock tracking - simple global inventory
    /// </summary>
    public class PowderStock
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string MaterialType { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal CurrentStock { get; set; } = 0.00m;
        
        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal ReorderPoint { get; set; } = 10.00m;
        
        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal AlertThreshold { get; set; } = 0.25m; // 25%
        
        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal CostPerKg { get; set; } = 0.00m;
        
        public DateTime? LastRestocked { get; set; }
        
        [Required]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        public bool LowStockAlert { get; set; } = false;
        
        // Computed properties
        [NotMapped]
        public bool IsLowStock => CurrentStock <= (ReorderPoint * AlertThreshold);
        
        [NotMapped]
        public decimal StockValue => CurrentStock * CostPerKg;
        
        [NotMapped]
        public string DisplayName => $"{MaterialType} ({CurrentStock:F1} kg)";
    }

    /// <summary>
    /// Powder consumption tracking per production build
    /// </summary>
    public class PowderConsumption
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProductionBuildId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string MaterialType { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(6,2)")]
        public decimal AmountUsed { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal CostAllocated { get; set; }
        
        [Required]
        public DateTime UsageDate { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual ProductionBuild? ProductionBuild { get; set; }
        
        // Computed properties
        [NotMapped]
        public decimal CostPerKg => AmountUsed > 0 ? CostAllocated / AmountUsed : 0;
    }
}
```

### Step 4: Update ProductionBuild Model
**Duration: 15 minutes**

#### 4.1 Add Missing Field to ProductionBuild
Update `OpCentrix/Models/ProductionModels.cs`, add to ProductionBuild class:

```csharp
// Add this field to the ProductionBuild class
public DateTime? PrinterEstimatedEndTime { get; set; }

// Add navigation property for powder consumption
public virtual ICollection<PowderConsumption> PowderConsumptions { get; set; } = new List<PowderConsumption>();
```

### Step 5: Execute Migration and Verify
**Duration: 30 minutes**

#### 5.1 Run Migration Script
```bash
# Navigate to project directory
cd OpCentrix

# Run the migration script
sqlite3 scheduler.db < "Data/Migrations/Phase1_EmergencyFixes.sql"
```

#### 5.2 Verify Database Changes
```bash
# Check that tables were created
sqlite3 scheduler.db ".tables" | grep -E "PowderStock|PowderConsumption"

# Check ProductionBuilds has new field
sqlite3 scheduler.db "PRAGMA table_info(ProductionBuilds);" | grep PrinterEstimated

# Check MasterParts columns (calculated durations should be gone)  
sqlite3 scheduler.db "PRAGMA table_info(MasterParts);" | grep -E "Stack.*Duration"

# Check powder materials were seeded
sqlite3 scheduler.db "SELECT MaterialType, CurrentStock FROM PowderStock;"
```

#### 5.3 Test Application Startup
```bash
# Build and run to verify no crashes
dotnet build
dotnet run
```

**Expected Results:**
- ? Application starts without database errors
- ? No crashes when accessing ProductionBuild pages
- ? Powder tables exist and are accessible
- ? PrinterEstimatedEndTime field is available

---

## Rollback Plan (If Something Goes Wrong)

### Emergency Rollback Script
Create `OpCentrix/Data/Migrations/Phase1_Rollback.sql`:

```sql
-- Emergency rollback for Phase 1 changes
PRAGMA foreign_keys = OFF;

-- Drop powder tables if they cause issues
DROP TABLE IF EXISTS PowderConsumption;
DROP TABLE IF EXISTS PowderStock;

-- Remove PrinterEstimatedEndTime field if needed
-- Note: SQLite doesn't support DROP COLUMN easily, so we'll rename the table
-- and recreate without the field if absolutely necessary

SELECT 'Phase 1 Rollback Complete - Powder tables removed' as Status;
PRAGMA foreign_keys = ON;
```

### How to Execute Rollback
```bash
sqlite3 scheduler.db < "Data/Migrations/Phase1_Rollback.sql"
```

---

## Testing Checklist

### Before Implementation
- [ ] Database backup created
- [ ] Application currently runs without errors
- [ ] Current ProductionBuild functionality documented

### After Implementation  
- [ ] Application starts successfully
- [ ] No console errors or exceptions on startup
- [ ] Can navigate to ProductionBuild pages without crashes
- [ ] PowderStock table accessible via Entity Framework
- [ ] PowderConsumption table accessible via Entity Framework
- [ ] PrinterEstimatedEndTime field can be set/retrieved
- [ ] MasterParts no longer has calculated duration columns

### Integration Tests
- [ ] Can create a ProductionBuild record with PrinterEstimatedEndTime
- [ ] Can create PowderConsumption records linked to ProductionBuild
- [ ] Can query PowderStock for inventory levels
- [ ] User lookups work without foreign key errors

---

## Known Risks & Mitigation

### Risk 1: SQLite Column Drop Issues
**Risk**: SQLite doesn't support DROP COLUMN directly
**Mitigation**: Use ALTER TABLE DROP COLUMN IF EXISTS (newer SQLite versions support this)
**Fallback**: If fails, leave columns but don't use them

### Risk 2: Data Loss
**Risk**: Migration could corrupt existing data
**Mitigation**: 
- Always backup database first
- Use IF NOT EXISTS for table creation
- Use INSERT OR IGNORE for data insertion

### Risk 3: Foreign Key Violations
**Risk**: Existing data might violate new constraints
**Mitigation**: 
- Check for orphaned records before adding constraints
- Use LEFT JOIN queries to identify issues
- Fix data problems manually if needed

---

## Next Phase Dependencies

This phase must be **100% complete and verified** before moving to:
- **Phase 2**: Delete Duplicate ProductionBuild Pages
- **Phase 3**: PrintTracking Integration  
- **Phase 4**: Stage-Specific Job Forms

**Phase 1 Success Criteria Must Be Met:**
- ? No database-related crashes
- ? All foreign key relationships working
- ? Powder inventory tables functional
- ? PrinterEstimatedEndTime field available
- ? Calculated time columns removed

---

## Completion Verification

### Final Verification Commands
```bash
# Verify all changes
sqlite3 scheduler.db "
SELECT 'Tables Created:' as Check, 
       COUNT(*) as Count 
FROM sqlite_master 
WHERE type='table' AND name IN ('PowderStock', 'PowderConsumption');

SELECT 'Powder Materials Seeded:' as Check, 
       COUNT(*) as Count 
FROM PowderStock;

SELECT 'ProductionBuilds Schema:' as Check,
       COUNT(*) as HasPrinterEstimatedField
FROM pragma_table_info('ProductionBuilds') 
WHERE name = 'PrinterEstimatedEndTime';
"
```

**Success Output:**
```
Check                    | Count
------------------------|-------
Tables Created:          | 2
Powder Materials Seeded: | 4  
ProductionBuilds Schema: | 1
```

---

*Phase 1 Status: Ready for Implementation*  
*Next Phase: 02-Delete-Duplicate-ProductionBuild-Pages.md*