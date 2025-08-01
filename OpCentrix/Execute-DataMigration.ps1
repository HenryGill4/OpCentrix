# ===================================================================
# DATA MIGRATION SCRIPT
# Migrates existing data from old Parts table to new normalized structure
# ===================================================================

Write-Host "Starting Data Migration Process..." -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Yellow

# Step 1: Navigate to project directory
Write-Host "Step 1: Navigating to project directory..." -ForegroundColor Cyan
Set-Location -Path "OpCentrix"

# Step 2: Create the data migration SQL script
Write-Host "Step 2: Creating data migration SQL script..." -ForegroundColor Cyan

$dataMigrationSQL = @"
-- ===================================================================
-- DATA MIGRATION SCRIPT FOR REFACTORED PARTS STRUCTURE
-- ===================================================================

BEGIN TRANSACTION;

-- Migrate core part data to Parts_New
INSERT INTO Parts_New (
    Id, PartNumber, Name, Description, PartCategory, PartClass, 
    Industry, Application, Material, ProcessType, EstimatedHours,
    StatusId, CreatedDate, CreatedBy, LastModifiedDate, LastModifiedBy, IsActive
)
SELECT 
    Id, 
    PartNumber, 
    COALESCE(Name, PartNumber) as Name,
    COALESCE(Description, '') as Description, 
    COALESCE(PartCategory, 'Prototype') as PartCategory,
    COALESCE(PartClass, 'B') as PartClass,
    COALESCE(Industry, 'Manufacturing') as Industry,
    COALESCE(Application, 'General') as Application,
    COALESCE(Material, 'Ti-6Al-4V Grade 5') as Material,
    COALESCE(ProcessType, 'SLS Metal') as ProcessType,
    COALESCE(EstimatedHours, 0) as EstimatedHours,
    1 as StatusId, -- Default to ACTIVE status
    COALESCE(CreatedDate, datetime('now')) as CreatedDate,
    COALESCE(CreatedBy, 'System') as CreatedBy,
    COALESCE(LastModifiedDate, datetime('now')) as LastModifiedDate,
    COALESCE(LastModifiedBy, 'System') as LastModifiedBy,
    COALESCE(IsActive, 1) as IsActive
FROM Parts
WHERE IsActive = 1;

-- Migrate physical properties
INSERT INTO PartPhysicalProperties (
    PartId, LengthMm, WidthMm, HeightMm, WeightGrams, VolumeMm3,
    Dimensions, ToleranceRequirements, SurfaceFinishRequirement
)
SELECT 
    Id,
    COALESCE(LengthMm, 0),
    COALESCE(WidthMm, 0), 
    COALESCE(HeightMm, 0),
    COALESCE(WeightGrams, 0),
    COALESCE(VolumeMm3, 0),
    COALESCE(Dimensions, ''),
    COALESCE(ToleranceRequirements, '±0.1mm typical'),
    COALESCE(SurfaceFinishRequirement, 'As-built')
FROM Parts
WHERE IsActive = 1;

-- Migrate cost data
INSERT INTO PartCosts (
    PartId, MaterialCostPerKg, LaborCostPerHour, MachineOperatingCostPerHour,
    SetupCost, PostProcessingCost, QualityInspectionCost, StandardSellingPrice
)
SELECT 
    Id,
    COALESCE(MaterialCostPerKg, 0),
    COALESCE(StandardLaborCostPerHour, 0),
    COALESCE(MachineOperatingCostPerHour, 0),
    COALESCE(SetupCost, 0),
    COALESCE(PostProcessingCost, 0),
    COALESCE(QualityInspectionCost, 0),
    COALESCE(StandardSellingPrice, 0)
FROM Parts
WHERE IsActive = 1;

-- Migrate quality metrics
INSERT INTO PartQualityMetrics (
    PartId, AverageQualityScore, AverageDefectRate, AverageEfficiencyPercent,
    TotalJobsCompleted, TotalUnitsProduced
)
SELECT 
    Id,
    COALESCE(AverageQualityScore, 0),
    COALESCE(AverageDefectRate, 0),
    COALESCE(AverageEfficiencyPercent, 0),
    COALESCE(TotalJobsCompleted, 0),
    COALESCE(TotalUnitsProduced, 0)
FROM Parts
WHERE IsActive = 1;

-- Migrate manufacturing parameters
INSERT INTO PartManufacturingParameters (
    PartId, RequiredMachineType, PreferredMachines, SlsMaterial,
    PowderRequirementKg, RecommendedLaserPower, RecommendedScanSpeed,
    RecommendedLayerThickness, RecommendedBuildTemperature,
    RequiredArgonPurity, MaxOxygenContent, RequiresSupports, SupportStrategy
)
SELECT 
    Id,
    COALESCE(RequiredMachineType, 'TruPrint 3000'),
    COALESCE(PreferredMachines, 'TI1,TI2'),
    COALESCE(SlsMaterial, 'Ti-6Al-4V Grade 5'),
    COALESCE(PowderRequirementKg, 0),
    COALESCE(RecommendedLaserPower, 0),
    COALESCE(RecommendedScanSpeed, 0),
    COALESCE(RecommendedLayerThickness, 0),
    COALESCE(RecommendedBuildTemperature, 0),
    COALESCE(RequiredArgonPurity, 0),
    COALESCE(MaxOxygenContent, 0),
    COALESCE(RequiresSupports, 0),
    COALESCE(SupportStrategy, '')
FROM Parts
WHERE IsActive = 1;

-- Create default stages for existing parts
INSERT INTO PartStages (PartId, ProcessId, StageOrder, IsRequired)
SELECT p.Id, mp.Id, 1, 1
FROM Parts_New p
CROSS JOIN ManufacturingProcesses mp
WHERE mp.ProcessCode = 'SLS_PRINTING';

-- Log the migration in audit trail
INSERT INTO AuditLog (TableName, RecordId, Action, ChangedBy, Reason)
VALUES ('Parts', 0, 'MIGRATION', 'System', 'Database refactoring migration from old Parts structure to normalized structure');

COMMIT;

-- Verification queries
SELECT 'Parts Migration' as Check_Type, 
       (SELECT COUNT(*) FROM Parts WHERE IsActive = 1) as Original_Count,
       (SELECT COUNT(*) FROM Parts_New) as New_Count;

SELECT 'Physical Properties' as Check_Type,
       COUNT(*) as Migrated_Count
FROM PartPhysicalProperties;

SELECT 'Cost Data' as Check_Type,
       COUNT(*) as Migrated_Count  
FROM PartCosts;

SELECT 'Quality Metrics' as Check_Type,
       COUNT(*) as Migrated_Count
FROM PartQualityMetrics;

SELECT 'Manufacturing Parameters' as Check_Type,
       COUNT(*) as Migrated_Count
FROM PartManufacturingParameters;

SELECT 'Part Stages' as Check_Type,
       COUNT(*) as Created_Count
FROM PartStages;
"@

# Write the SQL script to a file
$dataMigrationSQL | Out-File -FilePath "DataMigration.sql" -Encoding UTF8
Write-Host "Data migration SQL script created: DataMigration.sql" -ForegroundColor Green

# Step 3: Execute the data migration
Write-Host "Step 3: Executing data migration..." -ForegroundColor Cyan
Write-Host "WARNING: This will migrate data from old structure to new structure!" -ForegroundColor Yellow
$confirmation = Read-Host "Do you want to execute the data migration? (y/N)"

if ($confirmation -eq 'y' -or $confirmation -eq 'Y') {
    # Execute the SQL script using sqlite3 command line (if available)
    if (Get-Command sqlite3 -ErrorAction SilentlyContinue) {
        Write-Host "Executing data migration using sqlite3..." -ForegroundColor Cyan
        sqlite3 "Data/OpCentrix.db" ".read DataMigration.sql"
        Write-Host "Data migration completed!" -ForegroundColor Green
    } else {
        Write-Host "sqlite3 command not found. Please execute the DataMigration.sql script manually." -ForegroundColor Yellow
        Write-Host "You can use DB Browser for SQLite or similar tool to run the script." -ForegroundColor Cyan
    }
} else {
    Write-Host "Data migration cancelled by user." -ForegroundColor Yellow
    Write-Host "SQL script saved as DataMigration.sql for manual execution." -ForegroundColor Cyan
}

# Step 4: Create backup views for compatibility
Write-Host "Step 4: Creating compatibility views..." -ForegroundColor Cyan

$compatibilityViewsSQL = @"
-- ===================================================================
-- COMPATIBILITY VIEWS FOR BACKWARD COMPATIBILITY
-- ===================================================================

-- Parts compatibility view
CREATE VIEW IF NOT EXISTS vw_Parts_Legacy AS
SELECT 
    p.Id,
    p.PartNumber,
    p.Name,
    p.Description,
    p.PartCategory,
    p.PartClass,
    p.Industry,
    p.Application,
    p.Material,
    p.ProcessType,
    p.EstimatedHours,
    pp.LengthMm,
    pp.WidthMm,
    pp.HeightMm,
    pp.WeightGrams,
    pp.VolumeMm3,
    pp.Dimensions,
    pc.MaterialCostPerKg,
    pc.StandardSellingPrice,
    pq.AverageQualityScore,
    pq.TotalJobsCompleted,
    pm.SlsMaterial,
    pm.RequiredMachineType,
    p.CreatedDate,
    p.CreatedBy,
    p.LastModifiedDate,
    p.LastModifiedBy,
    p.IsActive
FROM Parts_New p
LEFT JOIN PartPhysicalProperties pp ON p.Id = pp.PartId
LEFT JOIN PartCosts pc ON p.Id = pc.PartId
LEFT JOIN PartQualityMetrics pq ON p.Id = pq.PartId
LEFT JOIN PartManufacturingParameters pm ON p.Id = pm.PartId;

-- Manufacturing stages view
CREATE VIEW IF NOT EXISTS vw_PartStages_Summary AS
SELECT 
    ps.PartId,
    p.PartNumber,
    ps.StageOrder,
    mp.ProcessName,
    mp.Category,
    ps.EstimatedDurationMinutes,
    ps.IsRequired,
    ps.IsActive
FROM PartStages ps
JOIN Parts_New p ON ps.PartId = p.Id
JOIN ManufacturingProcesses mp ON ps.ProcessId = mp.Id
WHERE ps.IsActive = 1
ORDER BY ps.PartId, ps.StageOrder;
"@

$compatibilityViewsSQL | Out-File -FilePath "CompatibilityViews.sql" -Encoding UTF8
Write-Host "Compatibility views SQL script created: CompatibilityViews.sql" -ForegroundColor Green

# Step 5: Display summary
Write-Host "=================================================" -ForegroundColor Yellow
Write-Host "DATA MIGRATION PROCESS COMPLETE!" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "FILES CREATED:" -ForegroundColor White
Write-Host "? DataMigration.sql - Main data migration script" -ForegroundColor Green
Write-Host "? CompatibilityViews.sql - Backward compatibility views" -ForegroundColor Green
Write-Host ""
Write-Host "MIGRATION STEPS:" -ForegroundColor White
Write-Host "? Core part data ? Parts_New table" -ForegroundColor Green
Write-Host "? Physical properties ? PartPhysicalProperties table" -ForegroundColor Green
Write-Host "? Cost information ? PartCosts table" -ForegroundColor Green
Write-Host "? Quality metrics ? PartQualityMetrics table" -ForegroundColor Green
Write-Host "? Manufacturing parameters ? PartManufacturingParameters table" -ForegroundColor Green
Write-Host "? Default stages ? PartStages table" -ForegroundColor Green
Write-Host "? Audit trail entry created" -ForegroundColor Green
Write-Host ""
Write-Host "TO EXECUTE MANUALLY:" -ForegroundColor White
Write-Host "1. Open DB Browser for SQLite" -ForegroundColor Cyan
Write-Host "2. Open your OpCentrix.db database" -ForegroundColor Cyan
Write-Host "3. Execute DataMigration.sql script" -ForegroundColor Cyan
Write-Host "4. Execute CompatibilityViews.sql script" -ForegroundColor Cyan
Write-Host "5. Verify migration results using the verification queries" -ForegroundColor Cyan
Write-Host ""
Write-Host "Data migration scripts ready for execution!" -ForegroundColor Green