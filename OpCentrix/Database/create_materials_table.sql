-- Enhanced Materials Table Fix Script for OpCentrix MES
-- Execute individual commands to avoid multi-line SQL issues

-- Enable foreign key constraints
PRAGMA foreign_keys = ON;
 
-- Create Materials table with all required columns
CREATE TABLE IF NOT EXISTS "Materials" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Materials" PRIMARY KEY AUTOINCREMENT,
    "MaterialCode" TEXT NOT NULL,
    "MaterialName" TEXT NOT NULL,
    "MaterialType" TEXT NOT NULL,
    "Description" TEXT NOT NULL DEFAULT '',
    "Density" REAL NOT NULL DEFAULT 4.43,
    "MeltingPointC" REAL NOT NULL DEFAULT 1660.0,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "CostPerGram" TEXT NOT NULL DEFAULT '0.50',
    "DefaultLayerThicknessMicrons" REAL NOT NULL DEFAULT 30.0,
    "DefaultLaserPowerPercent" REAL NOT NULL DEFAULT 85.0,
    "DefaultScanSpeedMmPerSec" REAL NOT NULL DEFAULT 1200.0,
    "MaterialProperties" TEXT NOT NULL DEFAULT '{}',
    "CompatibleMachineTypes" TEXT NOT NULL DEFAULT 'SLS',
    "SafetyNotes" TEXT NOT NULL DEFAULT '',
    "CreatedDate" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastModifiedDate" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

-- Create indexes for performance
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Materials_MaterialCode" ON "Materials" ("MaterialCode");
CREATE INDEX IF NOT EXISTS "IX_Materials_MaterialType" ON "Materials" ("MaterialType");  
CREATE INDEX IF NOT EXISTS "IX_Materials_IsActive" ON "Materials" ("IsActive");
CREATE INDEX IF NOT EXISTS "IX_Materials_CompatibleMachineTypes" ON "Materials" ("CompatibleMachineTypes");