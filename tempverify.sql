ALTER TABLE "Materials" ADD "QuantityOnHandKg" TEXT NOT NULL DEFAULT '0.0';

CREATE TABLE "ef_temp_MasterParts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MasterParts" PRIMARY KEY AUTOINCREMENT,
    "AllowStacking" INTEGER NOT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "Description" TEXT NOT NULL,
    "DoubleStackDurationHours" REAL NULL,
    "EnableDoubleStack" INTEGER NOT NULL,
    "EnableTripleStack" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "ManufacturingApproach" TEXT NOT NULL DEFAULT 'SLS-Based',
    "Material" TEXT NOT NULL,
    "MaxStackCount" INTEGER NOT NULL,
    "Name" TEXT NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "PartsPerBuildDouble" INTEGER NULL,
    "PartsPerBuildSingle" INTEGER NOT NULL,
    "PartsPerBuildTriple" INTEGER NULL,
    "RequiredStages" TEXT NOT NULL DEFAULT '[]',
    "SingleStackDurationHours" REAL NULL,
    "StageEstimateSingle" REAL NULL,
    "TripleStackDurationHours" REAL NULL
);

INSERT INTO "ef_temp_MasterParts" ("Id", "AllowStacking", "CreatedBy", "CreatedDate", "Description", "DoubleStackDurationHours", "EnableDoubleStack", "EnableTripleStack", "IsActive", "LastModifiedBy", "LastModifiedDate", "ManufacturingApproach", "Material", "MaxStackCount", "Name", "PartNumber", "PartsPerBuildDouble", "PartsPerBuildSingle", "PartsPerBuildTriple", "RequiredStages", "SingleStackDurationHours", "StageEstimateSingle", "TripleStackDurationHours")
SELECT "Id", "AllowStacking", "CreatedBy", "CreatedDate", "Description", "DoubleStackDurationHours", "EnableDoubleStack", "EnableTripleStack", "IsActive", "LastModifiedBy", "LastModifiedDate", "ManufacturingApproach", "Material", IFNULL("MaxStackCount", 0), "Name", "PartNumber", "PartsPerBuildDouble", "PartsPerBuildSingle", "PartsPerBuildTriple", "RequiredStages", "SingleStackDurationHours", "StageEstimateSingle", "TripleStackDurationHours"
FROM "MasterParts";

PRAGMA foreign_keys = 0;

DROP TABLE "MasterParts";

ALTER TABLE "ef_temp_MasterParts" RENAME TO "MasterParts";

PRAGMA foreign_keys = 1;

CREATE INDEX "IX_MasterParts_IsActive" ON "MasterParts" ("IsActive");

CREATE INDEX "IX_MasterParts_ManufacturingApproach" ON "MasterParts" ("ManufacturingApproach");

CREATE UNIQUE INDEX "IX_MasterParts_PartNumber" ON "MasterParts" ("PartNumber");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251002175517_TempVerify', '8.0.11');

