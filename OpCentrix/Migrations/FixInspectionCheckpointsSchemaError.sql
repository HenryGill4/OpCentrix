-- Fix InspectionCheckpoints schema issues
-- Remove the problematic DefectCategoryId1 column if it exists

-- First check if the column exists
SELECT name FROM pragma_table_info('InspectionCheckpoints') WHERE name = 'DefectCategoryId1';

-- If it exists, we need to recreate the table without it
-- Since SQLite doesn't support dropping columns directly, we'll use a migration

-- Create backup table
CREATE TABLE IF NOT EXISTS "InspectionCheckpoints_backup" AS 
SELECT "Id", "PartId", "DefectCategoryId", "CheckpointName", "Description", "InspectionType", 
       "SortOrder", "IsRequired", "IsActive", "EstimatedMinutes", "AcceptanceCriteria", 
       "MeasurementMethod", "RequiredEquipment", "RequiredSkills", "ReferenceDocuments", 
       "TargetValue", "UpperTolerance", "LowerTolerance", "Unit", "FailureAction", 
       "SampleSize", "SamplingMethod", "Category", "Priority", "Notes", 
       "CreatedDate", "LastModifiedDate", "CreatedBy", "LastModifiedBy"
FROM "InspectionCheckpoints";

-- Drop the original table
DROP TABLE IF EXISTS "InspectionCheckpoints";

-- Recreate the table with correct schema
CREATE TABLE "InspectionCheckpoints" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_InspectionCheckpoints" PRIMARY KEY AUTOINCREMENT,
    "PartId" INTEGER NOT NULL,
    "DefectCategoryId" INTEGER NULL,
    "CheckpointName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "InspectionType" TEXT NOT NULL DEFAULT 'Visual',
    "SortOrder" INTEGER NOT NULL DEFAULT 100,
    "IsRequired" INTEGER NOT NULL DEFAULT 1,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "EstimatedMinutes" INTEGER NOT NULL DEFAULT 5,
    "AcceptanceCriteria" TEXT NOT NULL,
    "MeasurementMethod" TEXT NOT NULL,
    "RequiredEquipment" TEXT NOT NULL,
    "RequiredSkills" TEXT NOT NULL,
    "ReferenceDocuments" TEXT NOT NULL,
    "TargetValue" REAL NULL,
    "UpperTolerance" REAL NULL,
    "LowerTolerance" REAL NULL,
    "Unit" TEXT NOT NULL,
    "FailureAction" TEXT NOT NULL DEFAULT 'Hold for review',
    "SampleSize" INTEGER NOT NULL DEFAULT 1,
    "SamplingMethod" TEXT NOT NULL DEFAULT 'All',
    "Category" TEXT NOT NULL DEFAULT 'Quality',
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "Notes" TEXT NOT NULL DEFAULT '',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    CONSTRAINT "FK_InspectionCheckpoints_DefectCategories_DefectCategoryId" FOREIGN KEY ("DefectCategoryId") REFERENCES "DefectCategories" ("Id"),
    CONSTRAINT "FK_InspectionCheckpoints_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE
);

-- Restore data from backup (only if backup exists)
INSERT OR IGNORE INTO "InspectionCheckpoints" 
SELECT * FROM "InspectionCheckpoints_backup" 
WHERE EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='InspectionCheckpoints_backup');

-- Drop backup table
DROP TABLE IF EXISTS "InspectionCheckpoints_backup";

-- Recreate indexes
CREATE INDEX IF NOT EXISTS "IX_InspectionCheckpoints_DefectCategoryId" ON "InspectionCheckpoints" ("DefectCategoryId");
CREATE INDEX IF NOT EXISTS "IX_InspectionCheckpoints_InspectionType" ON "InspectionCheckpoints" ("InspectionType");
CREATE INDEX IF NOT EXISTS "IX_InspectionCheckpoints_IsActive" ON "InspectionCheckpoints" ("IsActive");
CREATE INDEX IF NOT EXISTS "IX_InspectionCheckpoints_IsRequired" ON "InspectionCheckpoints" ("IsRequired");
CREATE INDEX IF NOT EXISTS "IX_InspectionCheckpoints_PartId" ON "InspectionCheckpoints" ("PartId");
CREATE INDEX IF NOT EXISTS "IX_InspectionCheckpoints_PartId_SortOrder" ON "InspectionCheckpoints" ("PartId", "SortOrder");

-- Verify the schema is correct
SELECT sql FROM sqlite_master WHERE type='table' AND name='InspectionCheckpoints';