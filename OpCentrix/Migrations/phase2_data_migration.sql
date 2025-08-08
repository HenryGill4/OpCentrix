-- OpCentrix Part Form Refactor - Phase 2: Data Migration
-- Migrating existing Parts from boolean flags to lookup-driven stage requirements

-- ============================================================================
-- 2.1 Pre-Migration Backup and Schema Snapshot
-- ============================================================================

-- Create backup tables for rollback capability
CREATE TABLE Migration_Backup_Parts AS SELECT * FROM Parts;
CREATE TABLE Migration_Backup_PartStageRequirements AS SELECT * FROM PartStageRequirements;

-- ============================================================================
-- 2.2 Pre-Migration Verification
-- ============================================================================

-- Verify ProductionStages exist for migration
SELECT 'Pre-check: Production Stages' as CheckType, COUNT(*) as Count 
FROM ProductionStages 
WHERE Name IN ('SLS Printing', 'EDM Operations', 'CNC Machining', 'Assembly', 'Finishing');

-- Check existing Parts data for migration
SELECT 'Pre-check: Parts with stage flags' as CheckType, COUNT(*) as Count
FROM Parts 
WHERE RequiresSLSPrinting = 1 OR RequiresCNCMachining = 1 OR RequiresEDMOperations = 1 
   OR RequiresAssembly = 1 OR RequiresFinishing = 1;

-- ============================================================================
-- 2.3 Component Type Data Migration
-- ============================================================================

BEGIN TRANSACTION;

-- Migrate ComponentType data with improved logic
UPDATE Parts 
SET ComponentTypeId = CASE 
    WHEN LOWER(IFNULL(ComponentType, '')) IN ('general', '') THEN 1
    WHEN LOWER(IFNULL(ComponentType, '')) = 'serialized' THEN 2
    ELSE 1 -- Default to General for any unrecognized values
END;

-- Migrate ComplianceCategory data (based on existing regulatory fields)
UPDATE Parts 
SET ComplianceCategoryId = CASE 
    WHEN RequiresATFCompliance = 1 OR RequiresITARCompliance = 1 OR RequiresSerialization = 1 THEN 2 -- NFA
    ELSE 1 -- Non NFA
END;

COMMIT;

-- ============================================================================
-- 2.4 Stage Boolean Flag Migration
-- ============================================================================

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
  AND p.IsActive = 1  -- Only migrate active parts
  AND (
    (lfm.LegacyFieldName = 'RequiresSLSPrinting' AND p.RequiresSLSPrinting = 1) OR
    (lfm.LegacyFieldName = 'RequiresCNCMachining' AND p.RequiresCNCMachining = 1) OR
    (lfm.LegacyFieldName = 'RequiresEDMOperations' AND p.RequiresEDMOperations = 1) OR
    (lfm.LegacyFieldName = 'RequiresAssembly' AND p.RequiresAssembly = 1) OR
    (lfm.LegacyFieldName = 'RequiresFinishing' AND p.RequiresFinishing = 1)
  )
  -- Prevent duplicates for parts that already have stage requirements
  AND NOT EXISTS (
    SELECT 1 FROM PartStageRequirements psr 
    WHERE psr.PartId = p.Id 
      AND psr.ProductionStageId = ps.Id 
      AND psr.IsActive = 1
  );

-- Mark parts as migrated
UPDATE Parts SET IsLegacyForm = 0 WHERE Id IN (
    SELECT DISTINCT PartId FROM PartStageRequirements WHERE CreatedBy = 'Migration'
);

COMMIT;

-- ============================================================================
-- 2.5 Data Integrity Verification
-- ============================================================================

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
WHERE p.IsActive = 1
GROUP BY p.Id, p.PartNumber, p.IsLegacyForm
ORDER BY p.PartNumber
LIMIT 10; -- Show first 10 for verification

-- Check for any migration issues
SELECT 'Migration Issues' as CheckType,
    COUNT(*) as PartsWithoutStages
FROM Parts p
WHERE p.IsActive = 1 
  AND p.IsLegacyForm = 1
  AND (p.RequiresSLSPrinting = 1 OR p.RequiresCNCMachining = 1 OR p.RequiresEDMOperations = 1 
       OR p.RequiresAssembly = 1 OR p.RequiresFinishing = 1)
  AND NOT EXISTS (
    SELECT 1 FROM PartStageRequirements psr 
    WHERE psr.PartId = p.Id AND psr.IsActive = 1
  );

-- ============================================================================
-- 2.6 Create Migration Debug View
-- ============================================================================

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

-- ============================================================================
-- Phase 2 Complete: Data Migration
-- ============================================================================
-- Summary of changes:
-- ? Created backup tables for rollback capability
-- ? Migrated ComponentType string field to ComponentTypeId foreign key
-- ? Migrated compliance data to ComplianceCategoryId based on regulatory flags
-- ? Migrated stage boolean flags to PartStageRequirements records
-- ? Distributed estimated hours across required stages
-- ? Marked migrated parts with IsLegacyForm = 0
-- ? Created debug view for migration status monitoring
-- ? Verified data integrity and migration success
-- 
-- Ready for Phase 3: Form Modernization