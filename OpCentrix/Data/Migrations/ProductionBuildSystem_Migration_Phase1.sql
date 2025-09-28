-- OpCentrix Production Build System Migration
-- Phase 1: Database Cleanup and New Table Creation
-- Created: 2025-01-15
-- Purpose: Migrate from old BuildJob system to new Production Build architecture

PRAGMA foreign_keys = OFF;

-- =============================================================================
-- PHASE 1A: CLEANUP DUPLICATE PRODUCTION STAGES
-- =============================================================================

-- First, let's identify and remove duplicate production stages
-- Keep the ones with the lowest ID for each name
DELETE FROM ProductionStages 
WHERE Id NOT IN (
    SELECT MIN(Id) 
    FROM ProductionStages 
    GROUP BY Name, Department
);

-- Update display orders to be sequential and unique
UPDATE ProductionStages SET DisplayOrder = 1 WHERE Name = 'SLS Printing' OR Name = 'SLS 3D Printing';
UPDATE ProductionStages SET DisplayOrder = 2 WHERE Name = 'Heat Treatment';  
UPDATE ProductionStages SET DisplayOrder = 3 WHERE Name = 'EDM Operations';
UPDATE ProductionStages SET DisplayOrder = 4 WHERE Name = 'CNC Machining';
UPDATE ProductionStages SET DisplayOrder = 5 WHERE Name = 'Assembly';
UPDATE ProductionStages SET DisplayOrder = 6 WHERE Name = 'Finishing';
UPDATE ProductionStages SET DisplayOrder = 7 WHERE Name = 'Quality Inspection';

-- Consolidate SLS stages to just one
UPDATE ProductionStages 
SET Name = 'SLS Printing', 
    Description = 'Selective Laser Sintering - Metal 3D Printing',
    Department = '3D Printing'
WHERE Name = 'SLS 3D Printing' OR Name = 'SLS Printing';

-- Remove any remaining duplicates after consolidation
DELETE FROM ProductionStages 
WHERE Id NOT IN (
    SELECT MIN(Id) 
    FROM ProductionStages 
    GROUP BY Name
);

-- =============================================================================
-- PHASE 1B: CREATE NEW PRODUCTION BUILD TABLES  
-- =============================================================================

-- Create MasterParts table
CREATE TABLE IF NOT EXISTS "MasterParts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MasterParts" PRIMARY KEY AUTOINCREMENT,
    "PartNumber" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Material" TEXT NOT NULL DEFAULT 'Ti-6Al-4V Grade 5',
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "ManufacturingApproach" TEXT NOT NULL DEFAULT 'SLS-Based',
    "AllowStacking" INTEGER NOT NULL DEFAULT 0,
    "SingleStackDurationHours" REAL NULL,
    "DoubleStackDurationHours" REAL NULL, 
    "TripleStackDurationHours" REAL NULL,
    "MaxStackCount" INTEGER DEFAULT 1,
    "RequiredStages" TEXT NOT NULL DEFAULT '[]',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

-- Create indexes for MasterParts
CREATE UNIQUE INDEX "IX_MasterParts_PartNumber" ON "MasterParts" ("PartNumber");
CREATE INDEX "IX_MasterParts_IsActive" ON "MasterParts" ("IsActive");
CREATE INDEX "IX_MasterParts_ManufacturingApproach" ON "MasterParts" ("ManufacturingApproach");
CREATE INDEX "IX_MasterParts_Material" ON "MasterParts" ("Material");

-- Create ProductionBuilds table
CREATE TABLE IF NOT EXISTS "ProductionBuilds" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductionBuilds" PRIMARY KEY AUTOINCREMENT,
    "BuildNumber" TEXT NOT NULL,
    "MasterPartId" INTEGER NOT NULL,
    "PrinterName" TEXT NOT NULL,
    "BuildQuantity" INTEGER NOT NULL DEFAULT 1,
    "StackLevel" INTEGER NOT NULL DEFAULT 1,
    "MaterialBatch" TEXT NOT NULL,
    "PowderLot" TEXT NULL,
    "AddedPowder" INTEGER NOT NULL DEFAULT 0,
    "PowderAmountKg" decimal(5,2) NULL,
    "ActualStartTime" TEXT NULL,
    "ActualEndTime" TEXT NULL,
    "ScheduledStartTime" TEXT NULL,
    "ScheduledEndTime" TEXT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Planned',
    "CreatedByUserId" INTEGER NOT NULL,
    "SetupNotes" TEXT NULL,
    "CompletionNotes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CompletedDate" TEXT NULL,
    CONSTRAINT "FK_ProductionBuilds_MasterParts_MasterPartId" FOREIGN KEY ("MasterPartId") REFERENCES "MasterParts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ProductionBuilds_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

-- Create indexes for ProductionBuilds
CREATE UNIQUE INDEX "IX_ProductionBuilds_BuildNumber" ON "ProductionBuilds" ("BuildNumber");
CREATE INDEX "IX_ProductionBuilds_Status" ON "ProductionBuilds" ("Status");
CREATE INDEX "IX_ProductionBuilds_PrinterName" ON "ProductionBuilds" ("PrinterName");
CREATE INDEX "IX_ProductionBuilds_CreatedDate" ON "ProductionBuilds" ("CreatedDate");
CREATE INDEX "IX_ProductionBuilds_MasterPartId" ON "ProductionBuilds" ("MasterPartId");
CREATE INDEX "IX_ProductionBuilds_CreatedByUserId" ON "ProductionBuilds" ("CreatedByUserId");

-- Create StageDefinitions table
CREATE TABLE IF NOT EXISTS "StageDefinitions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StageDefinitions" PRIMARY KEY AUTOINCREMENT,
    "MasterPartId" INTEGER NOT NULL,
    "StageName" TEXT NOT NULL,
    "ExecutionOrder" INTEGER NOT NULL,
    "IsRequired" INTEGER NOT NULL DEFAULT 1,
    "CanSkip" INTEGER NOT NULL DEFAULT 0,
    "EstimatedHoursPerPart" REAL NOT NULL DEFAULT 1.0,
    "SetupMinutes" INTEGER NOT NULL DEFAULT 30,
    "TeardownMinutes" INTEGER NOT NULL DEFAULT 15,
    "RequiredMachineType" TEXT NULL,
    "PreferredMachines" TEXT NULL,
    "StageConfiguration" TEXT NOT NULL DEFAULT '{}',
    "QualityRequirements" TEXT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_StageDefinitions_MasterParts_MasterPartId" FOREIGN KEY ("MasterPartId") REFERENCES "MasterParts" ("Id") ON DELETE CASCADE
);

-- Create indexes for StageDefinitions
CREATE INDEX "IX_StageDefinitions_MasterPartId" ON "StageDefinitions" ("MasterPartId");
CREATE INDEX "IX_StageDefinitions_StageName" ON "StageDefinitions" ("StageName");
CREATE INDEX "IX_StageDefinitions_ExecutionOrder" ON "StageDefinitions" ("ExecutionOrder");
CREATE INDEX "IX_StageDefinitions_IsActive" ON "StageDefinitions" ("IsActive");
CREATE INDEX "IX_StageDefinitions_MasterPartId_ExecutionOrder" ON "StageDefinitions" ("MasterPartId", "ExecutionOrder");

-- Create StageExecutions table
CREATE TABLE IF NOT EXISTS "StageExecutions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StageExecutions" PRIMARY KEY AUTOINCREMENT,
    "ProductionBuildId" INTEGER NOT NULL,
    "StageDefinitionId" INTEGER NOT NULL,
    "StageName" TEXT NOT NULL,
    "ExecutionOrder" INTEGER NOT NULL,
    "QuantityIn" INTEGER NOT NULL DEFAULT 0,
    "QuantityOut" INTEGER NOT NULL DEFAULT 0,
    "DefectCount" INTEGER NOT NULL DEFAULT 0,
    "ReworkCount" INTEGER NOT NULL DEFAULT 0,
    "StartTime" TEXT NULL,
    "EndTime" TEXT NULL,
    "ActualHours" REAL NULL,
    "Status" TEXT NOT NULL DEFAULT 'NotStarted',
    "MachineUsed" TEXT NULL,
    "OperatorUserId" INTEGER NULL,
    "StageData" TEXT NOT NULL DEFAULT '{}',
    "OperatorNotes" TEXT NULL,
    "QualityNotes" TEXT NULL,
    "PassedQuality" INTEGER NOT NULL DEFAULT 1,
    "ActualCost" decimal(10,2) NULL,
    "MaterialCost" decimal(10,2) NULL,
    "LaborCost" decimal(10,2) NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CompletedDate" TEXT NULL,
    CONSTRAINT "FK_StageExecutions_ProductionBuilds_ProductionBuildId" FOREIGN KEY ("ProductionBuildId") REFERENCES "ProductionBuilds" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_StageExecutions_StageDefinitions_StageDefinitionId" FOREIGN KEY ("StageDefinitionId") REFERENCES "StageDefinitions" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_StageExecutions_Users_OperatorUserId" FOREIGN KEY ("OperatorUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

-- Create indexes for StageExecutions
CREATE INDEX "IX_StageExecutions_ProductionBuildId" ON "StageExecutions" ("ProductionBuildId");
CREATE INDEX "IX_StageExecutions_Status" ON "StageExecutions" ("Status");
CREATE INDEX "IX_StageExecutions_StageName" ON "StageExecutions" ("StageName");
CREATE INDEX "IX_StageExecutions_StartTime" ON "StageExecutions" ("StartTime");
CREATE INDEX "IX_StageExecutions_OperatorUserId" ON "StageExecutions" ("OperatorUserId");
CREATE INDEX "IX_StageExecutions_StageDefinitionId" ON "StageExecutions" ("StageDefinitionId");
CREATE INDEX "IX_StageExecutions_ProductionBuildId_ExecutionOrder" ON "StageExecutions" ("ProductionBuildId", "ExecutionOrder");

-- Create PartBatches table
CREATE TABLE IF NOT EXISTS "PartBatches" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartBatches" PRIMARY KEY AUTOINCREMENT,
    "ProductionBuildId" INTEGER NOT NULL,
    "BatchNumber" TEXT NOT NULL,
    "CurrentStage" TEXT NOT NULL DEFAULT 'SLS',
    "Quantity" INTEGER NOT NULL,
    "QualityStatus" TEXT NOT NULL DEFAULT 'Good',
    "Location" TEXT NULL,
    "Notes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_PartBatches_ProductionBuilds_ProductionBuildId" FOREIGN KEY ("ProductionBuildId") REFERENCES "ProductionBuilds" ("Id") ON DELETE CASCADE
);

-- Create indexes for PartBatches
CREATE INDEX "IX_PartBatches_ProductionBuildId" ON "PartBatches" ("ProductionBuildId");
CREATE INDEX "IX_PartBatches_BatchNumber" ON "PartBatches" ("BatchNumber");
CREATE INDEX "IX_PartBatches_CurrentStage" ON "PartBatches" ("CurrentStage");
CREATE INDEX "IX_PartBatches_QualityStatus" ON "PartBatches" ("QualityStatus");
CREATE INDEX "IX_PartBatches_CreatedDate" ON "PartBatches" ("CreatedDate");

-- =============================================================================
-- PHASE 1C: SEED SAMPLE DATA FROM EXISTING PARTS
-- =============================================================================

-- Create master parts from existing parts that have SLS printing enabled
INSERT INTO MasterParts (
    PartNumber, 
    Name, 
    Description, 
    Material, 
    ManufacturingApproach,
    AllowStacking,
    SingleStackDurationHours,
    DoubleStackDurationHours,
    TripleStackDurationHours,
    MaxStackCount,
    RequiredStages,
    CreatedBy,
    LastModifiedBy
)
SELECT 
    PartNumber,
    Name,
    Description,
    Material,
    CASE 
        WHEN RequiresSLSPrinting = 1 THEN 'SLS-Based' 
        ELSE 'RawMaterial-Based' 
    END as ManufacturingApproach,
    COALESCE(AllowStacking, 0) as AllowStacking,
    CASE 
        WHEN SingleStackDurationHours IS NOT NULL AND SingleStackDurationHours > 0 
        THEN SingleStackDurationHours 
        ELSE EstimatedHours 
    END as SingleStackDurationHours,
    DoubleStackDurationHours,
    TripleStackDurationHours,
    CASE 
        WHEN AllowStacking = 1 THEN 3 
        ELSE 1 
    END as MaxStackCount,
    -- Build required stages JSON array based on boolean flags
    '[' ||
    CASE WHEN RequiresSLSPrinting = 1 THEN '"SLS",' ELSE '' END ||
    CASE WHEN RequiresEDMOperations = 1 THEN '"EDM",' ELSE '' END ||
    CASE WHEN RequiresCNCMachining = 1 THEN '"CNC",' ELSE '' END ||
    CASE WHEN RequiresAssembly = 1 THEN '"Assembly",' ELSE '' END ||
    CASE WHEN RequiresFinishing = 1 THEN '"Finishing",' ELSE '' END ||
    '"Quality"' ||  -- Always include quality
    ']' as RequiredStages,
    'Migration' as CreatedBy,
    'Migration' as LastModifiedBy
FROM Parts 
WHERE IsActive = 1;

-- Create stage definitions for each master part based on their required stages
-- SLS Stage
INSERT INTO StageDefinitions (
    MasterPartId, StageName, ExecutionOrder, EstimatedHoursPerPart, 
    RequiredMachineType, PreferredMachines, StageConfiguration
)
SELECT 
    mp.Id,
    'SLS' as StageName,
    1 as ExecutionOrder,
    p.EstimatedHours as EstimatedHoursPerPart,
    'SLS' as RequiredMachineType,
    'TI1,TI2,INC' as PreferredMachines,
    '{"LaserPower":170,"ScanSpeed":1000,"LayerThickness":30,"HatchSpacing":120,"BuildTemperature":180}' as StageConfiguration
FROM MasterParts mp
JOIN Parts p ON p.PartNumber = mp.PartNumber
WHERE mp.RequiredStages LIKE '%"SLS"%';

-- EDM Stage (must follow SLS for cut-off)
INSERT INTO StageDefinitions (
    MasterPartId, StageName, ExecutionOrder, EstimatedHoursPerPart,
    RequiredMachineType, PreferredMachines, StageConfiguration
)
SELECT 
    mp.Id,
    'EDM' as StageName,
    2 as ExecutionOrder,
    ROUND(p.EstimatedHours * 0.3, 2) as EstimatedHoursPerPart, -- 30% of SLS time
    'EDM' as RequiredMachineType,
    'EDM' as PreferredMachines,
    '{"WireType":"Brass","WireDiameter":0.25,"CutSpeed":5.0,"CutOffHeight":2.0}' as StageConfiguration
FROM MasterParts mp
JOIN Parts p ON p.PartNumber = mp.PartNumber
WHERE mp.RequiredStages LIKE '%"SLS"%'; -- EDM follows all SLS parts for cut-off

-- CNC Stage
INSERT INTO StageDefinitions (
    MasterPartId, StageName, ExecutionOrder, EstimatedHoursPerPart,
    RequiredMachineType, PreferredMachines, StageConfiguration
)
SELECT 
    mp.Id,
    'CNC' as StageName,
    CASE WHEN mp.RequiredStages LIKE '%"EDM"%' THEN 3 ELSE 1 END as ExecutionOrder, -- After EDM or first if no SLS
    ROUND(p.EstimatedHours * 0.4, 2) as EstimatedHoursPerPart, -- 40% of SLS time
    'CNC' as RequiredMachineType,
    'CNC1,CNC2,CNC3,CNC4,CNC5' as PreferredMachines,
    '{"ToolsUsed":[],"SpindleSpeed":2500,"FeedRate":500,"CoolantType":"Flood"}' as StageConfiguration
FROM MasterParts mp
JOIN Parts p ON p.PartNumber = mp.PartNumber
WHERE mp.RequiredStages LIKE '%"CNC"%';

-- Assembly Stage
INSERT INTO StageDefinitions (
    MasterPartId, StageName, ExecutionOrder, EstimatedHoursPerPart,
    RequiredMachineType, PreferredMachines, StageConfiguration
)
SELECT 
    mp.Id,
    'Assembly' as StageName,
    -- Calculate execution order based on previous stages
    CASE 
        WHEN mp.RequiredStages LIKE '%"CNC"%' AND mp.RequiredStages LIKE '%"EDM"%' THEN 4
        WHEN mp.RequiredStages LIKE '%"CNC"%' OR mp.RequiredStages LIKE '%"EDM"%' THEN 3
        ELSE 2
    END as ExecutionOrder,
    2.0 as EstimatedHoursPerPart, -- Fixed 2 hours for assembly
    'Assembly' as RequiredMachineType,
    '' as PreferredMachines,
    '{"Components":[],"Hardware":[],"TorqueSpecs":{},"TestPressure":0}' as StageConfiguration
FROM MasterParts mp
JOIN Parts p ON p.PartNumber = mp.PartNumber
WHERE mp.RequiredStages LIKE '%"Assembly"%';

-- Finishing Stage
INSERT INTO StageDefinitions (
    MasterPartId, StageName, ExecutionOrder, EstimatedHoursPerPart,
    RequiredMachineType, PreferredMachines, StageConfiguration
)
SELECT 
    mp.Id,
    'Finishing' as StageName,
    -- Calculate execution order (always near the end)
    CASE 
        WHEN mp.RequiredStages LIKE '%"Assembly"%' THEN 5
        WHEN mp.RequiredStages LIKE '%"CNC"%' AND mp.RequiredStages LIKE '%"EDM"%' THEN 4
        WHEN mp.RequiredStages LIKE '%"CNC"%' OR mp.RequiredStages LIKE '%"EDM"%' THEN 3
        ELSE 2
    END as ExecutionOrder,
    3.0 as EstimatedHoursPerPart, -- Fixed 3 hours for finishing
    'Finishing' as RequiredMachineType,
    '' as PreferredMachines,
    '{"SurfaceFinish":"Ra 3.2","CoatingType":"Cerakote","Color":"Black"}' as StageConfiguration
FROM MasterParts mp
JOIN Parts p ON p.PartNumber = mp.PartNumber
WHERE mp.RequiredStages LIKE '%"Finishing"%';

-- Quality Stage (always last)
INSERT INTO StageDefinitions (
    MasterPartId, StageName, ExecutionOrder, EstimatedHoursPerPart,
    RequiredMachineType, PreferredMachines, StageConfiguration
)
SELECT 
    mp.Id,
    'Quality' as StageName,
    -- Always last stage
    CASE 
        WHEN mp.RequiredStages LIKE '%"Finishing"%' THEN 6
        WHEN mp.RequiredStages LIKE '%"Assembly"%' THEN 5
        WHEN mp.RequiredStages LIKE '%"CNC"%' AND mp.RequiredStages LIKE '%"EDM"%' THEN 4
        WHEN mp.RequiredStages LIKE '%"CNC"%' OR mp.RequiredStages LIKE '%"EDM"%' THEN 3
        ELSE 2
    END as ExecutionOrder,
    0.5 as EstimatedHoursPerPart, -- Fixed 30 minutes for quality inspection
    'Inspection' as RequiredMachineType,
    '' as PreferredMachines,
    '{"DimensionalCheck":true,"SurfaceFinish":true,"MaterialCert":true}' as StageConfiguration
FROM MasterParts mp;

-- =============================================================================
-- PHASE 1D: VERIFICATION AND CLEANUP
-- =============================================================================

-- Verify our data migration
SELECT 'Master Parts Created:' as Description, COUNT(*) as Count FROM MasterParts
UNION ALL
SELECT 'Stage Definitions Created:', COUNT(*) FROM StageDefinitions
UNION ALL
SELECT 'Production Stages Cleaned:', COUNT(*) FROM ProductionStages;

-- Update any materials that might be missing
UPDATE Machines 
SET CurrentMaterial = CASE 
    WHEN MachineId = 'TI1' THEN 'TI64-G5'
    WHEN MachineId = 'TI2' THEN 'TI64-G5' 
    WHEN MachineId = 'INC' THEN 'IN718'
    WHEN MachineId LIKE 'CNC%' THEN 'Various'
    WHEN MachineId = 'EDM' THEN 'N/A'
    ELSE CurrentMaterial
END
WHERE CurrentMaterial IS NULL OR CurrentMaterial = '';

PRAGMA foreign_keys = ON;

-- Migration complete!
SELECT 'Production Build System Migration Complete!' as Status;