-- ===============================================
-- OpCentrix Production Stages Seeding Script
-- Adds default production stages for enhanced manufacturing workflow
-- ===============================================

-- Check if ProductionStages table exists, if not create it
CREATE TABLE IF NOT EXISTS "ProductionStages" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductionStages" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "DisplayOrder" INTEGER NOT NULL,
    "Description" TEXT NULL,
    "DefaultSetupMinutes" INTEGER NOT NULL DEFAULT 30,
    "DefaultHourlyRate" decimal(8,2) NOT NULL DEFAULT 85.0,
    "RequiresQualityCheck" INTEGER NOT NULL DEFAULT 1,
    "RequiresApproval" INTEGER NOT NULL DEFAULT 0,
    "AllowSkip" INTEGER NOT NULL DEFAULT 0,
    "IsOptional" INTEGER NOT NULL DEFAULT 0,
    "RequiredRole" TEXT NULL,
    "CustomFieldsConfig" TEXT NOT NULL DEFAULT '[]',
    "AssignedMachineIds" TEXT NULL,
    "RequiresMachineAssignment" INTEGER NOT NULL DEFAULT 0,
    "DefaultMachineId" TEXT NULL,
    "StageColor" TEXT NOT NULL DEFAULT '#007bff',
    "StageIcon" TEXT NOT NULL DEFAULT 'fas fa-cogs',
    "Department" TEXT NULL,
    "AllowParallelExecution" INTEGER NOT NULL DEFAULT 0,
    "DefaultMaterialCost" decimal(10,2) NOT NULL DEFAULT 0.00,
    "DefaultDurationHours" REAL NOT NULL DEFAULT 1.0,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "IsActive" INTEGER NOT NULL DEFAULT 1
);

-- Seed default production stages if none exist
INSERT OR IGNORE INTO "ProductionStages" (
    "Name", "DisplayOrder", "Description", "DefaultSetupMinutes", "DefaultHourlyRate", 
    "DefaultDurationHours", "DefaultMaterialCost", "RequiresQualityCheck", "RequiresApproval",
    "AllowSkip", "IsOptional", "RequiredRole", "Department", "StageColor", "StageIcon",
    "RequiresMachineAssignment", "AllowParallelExecution", "CreatedDate", "CreatedBy", 
    "LastModifiedDate", "LastModifiedBy", "IsActive"
) 
SELECT 
    "SLS Printing", 1, "Selective Laser Sintering metal printing process", 45, 85.00,
    8.0, 25.00, 1, 0, 0, 0, "Operator", "3D Printing", "#007bff", "fas fa-print",
    1, 0, datetime('now'), "System", datetime('now'), "System", 1
WHERE NOT EXISTS (SELECT 1 FROM "ProductionStages" WHERE "Name" = "SLS Printing");

INSERT OR IGNORE INTO "ProductionStages" (
    "Name", "DisplayOrder", "Description", "DefaultSetupMinutes", "DefaultHourlyRate", 
    "DefaultDurationHours", "DefaultMaterialCost", "RequiresQualityCheck", "RequiresApproval",
    "AllowSkip", "IsOptional", "RequiredRole", "Department", "StageColor", "StageIcon",
    "RequiresMachineAssignment", "AllowParallelExecution", "CreatedDate", "CreatedBy", 
    "LastModifiedDate", "LastModifiedBy", "IsActive"
) 
SELECT 
    "CNC Machining", 2, "Computer Numerical Control precision machining", 30, 95.00,
    4.0, 10.00, 1, 0, 1, 1, "Machinist", "CNC Machining", "#28a745", "fas fa-cogs",
    1, 0, datetime('now'), "System", datetime('now'), "System", 1
WHERE NOT EXISTS (SELECT 1 FROM "ProductionStages" WHERE "Name" = "CNC Machining");

INSERT OR IGNORE INTO "ProductionStages" (
    "Name", "DisplayOrder", "Description", "DefaultSetupMinutes", "DefaultHourlyRate", 
    "DefaultDurationHours", "DefaultMaterialCost", "RequiresQualityCheck", "RequiresApproval",
    "AllowSkip", "IsOptional", "RequiredRole", "Department", "StageColor", "StageIcon",
    "RequiresMachineAssignment", "AllowParallelExecution", "CreatedDate", "CreatedBy", 
    "LastModifiedDate", "LastModifiedBy", "IsActive"
) 
SELECT 
    "EDM Operations", 3, "Electrical Discharge Machining for complex geometries", 60, 120.00,
    6.0, 15.00, 1, 1, 1, 1, "EDM Specialist", "EDM", "#ffc107", "fas fa-bolt",
    1, 0, datetime('now'), "System", datetime('now'), "System", 1
WHERE NOT EXISTS (SELECT 1 FROM "ProductionStages" WHERE "Name" = "EDM Operations");

INSERT OR IGNORE INTO "ProductionStages" (
    "Name", "DisplayOrder", "Description", "DefaultSetupMinutes", "DefaultHourlyRate", 
    "DefaultDurationHours", "DefaultMaterialCost", "RequiresQualityCheck", "RequiresApproval",
    "AllowSkip", "IsOptional", "RequiredRole", "Department", "StageColor", "StageIcon",
    "RequiresMachineAssignment", "AllowParallelExecution", "CreatedDate", "CreatedBy", 
    "LastModifiedDate", "LastModifiedBy", "IsActive"
) 
SELECT 
    "Assembly", 4, "Multi-component assembly operations", 15, 75.00,
    2.0, 5.00, 1, 0, 0, 1, "Assembler", "Assembly", "#17a2b8", "fas fa-puzzle-piece",
    0, 1, datetime('now'), "System", datetime('now'), "System", 1
WHERE NOT EXISTS (SELECT 1 FROM "ProductionStages" WHERE "Name" = "Assembly");

INSERT OR IGNORE INTO "ProductionStages" (
    "Name", "DisplayOrder", "Description", "DefaultSetupMinutes", "DefaultHourlyRate", 
    "DefaultDurationHours", "DefaultMaterialCost", "RequiresQualityCheck", "RequiresApproval",
    "AllowSkip", "IsOptional", "RequiredRole", "Department", "StageColor", "StageIcon",
    "RequiresMachineAssignment", "AllowParallelExecution", "CreatedDate", "CreatedBy", 
    "LastModifiedDate", "LastModifiedBy", "IsActive"
) 
SELECT 
    "Finishing", 5, "Surface finishing and coating operations", 20, 70.00,
    3.0, 8.00, 1, 0, 1, 1, "Finisher", "Finishing", "#6c757d", "fas fa-brush",
    0, 1, datetime('now'), "System", datetime('now'), "System", 1
WHERE NOT EXISTS (SELECT 1 FROM "ProductionStages" WHERE "Name" = "Finishing");

INSERT OR IGNORE INTO "ProductionStages" (
    "Name", "DisplayOrder", "Description", "DefaultSetupMinutes", "DefaultHourlyRate", 
    "DefaultDurationHours", "DefaultMaterialCost", "RequiresQualityCheck", "RequiresApproval",
    "AllowSkip", "IsOptional", "RequiredRole", "Department", "StageColor", "StageIcon",
    "RequiresMachineAssignment", "AllowParallelExecution", "CreatedDate", "CreatedBy", 
    "LastModifiedDate", "LastModifiedBy", "IsActive"
) 
SELECT 
    "Quality Inspection", 6, "Final quality control and inspection", 10, 80.00,
    1.0, 2.00, 1, 1, 0, 0, "Quality Inspector", "Quality Control", "#dc3545", "fas fa-search",
    0, 0, datetime('now'), "System", datetime('now'), "System", 1
WHERE NOT EXISTS (SELECT 1 FROM "ProductionStages" WHERE "Name" = "Quality Inspection");

-- Create indexes if they don't exist
CREATE INDEX IF NOT EXISTS "IX_ProductionStages_DisplayOrder" ON "ProductionStages" ("DisplayOrder");
CREATE INDEX IF NOT EXISTS "IX_ProductionStages_IsActive" ON "ProductionStages" ("IsActive");
CREATE INDEX IF NOT EXISTS "IX_ProductionStages_Name" ON "ProductionStages" ("Name");
CREATE INDEX IF NOT EXISTS "IX_ProductionStages_RequiredRole" ON "ProductionStages" ("RequiredRole");

-- Also create the PartStageRequirements table if it doesn't exist
CREATE TABLE IF NOT EXISTS "PartStageRequirements" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartStageRequirements" PRIMARY KEY AUTOINCREMENT,
    "PartId" INTEGER NOT NULL,
    "ProductionStageId" INTEGER NOT NULL,
    "ExecutionOrder" INTEGER NOT NULL,
    "IsRequired" INTEGER NOT NULL DEFAULT 1,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "EstimatedHours" REAL NULL,
    "SetupTimeMinutes" INTEGER NOT NULL DEFAULT 30,
    "HourlyRateOverride" decimal(8,2) NULL,
    "EstimatedCost" decimal(10,2) NOT NULL DEFAULT 0.00,
    "MaterialCost" decimal(10,2) NOT NULL DEFAULT 0.00,
    "AssignedMachineId" TEXT NULL,
    "RequiresSpecificMachine" INTEGER NOT NULL DEFAULT 0,
    "PreferredMachineIds" TEXT NULL,
    "AllowParallelExecution" INTEGER NOT NULL DEFAULT 0,
    "IsBlocking" INTEGER NOT NULL DEFAULT 1,
    "CustomFieldValues" TEXT NOT NULL DEFAULT '{}',
    "StageParameters" TEXT NOT NULL DEFAULT '{}',
    "RequiredMaterials" TEXT NOT NULL DEFAULT '[]',
    "RequiredTooling" TEXT NOT NULL DEFAULT '',
    "QualityRequirements" TEXT NOT NULL DEFAULT '{}',
    "SpecialInstructions" TEXT NOT NULL DEFAULT '',
    "RequirementNotes" TEXT NOT NULL DEFAULT '',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    CONSTRAINT "FK_PartStageRequirements_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PartStageRequirements_ProductionStages_ProductionStageId" FOREIGN KEY ("ProductionStageId") REFERENCES "ProductionStages" ("Id") ON DELETE CASCADE
);

-- Create indexes for PartStageRequirements if they don't exist
CREATE INDEX IF NOT EXISTS "IX_PartStageRequirements_PartId" ON "PartStageRequirements" ("PartId");
CREATE INDEX IF NOT EXISTS "IX_PartStageRequirements_ProductionStageId" ON "PartStageRequirements" ("ProductionStageId");
CREATE INDEX IF NOT EXISTS "IX_PartStageRequirements_ExecutionOrder" ON "PartStageRequirements" ("ExecutionOrder");
CREATE INDEX IF NOT EXISTS "IX_PartStageRequirements_IsActive" ON "PartStageRequirements" ("IsActive");

-- Query to show what was inserted
SELECT 'Production Stages seeded:' as Result;
SELECT Id, Name, DisplayOrder, Department, DefaultDurationHours, DefaultHourlyRate, IsActive 
FROM ProductionStages 
WHERE IsActive = 1 
ORDER BY DisplayOrder;