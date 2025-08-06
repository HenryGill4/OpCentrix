-- Materials Table Creation and Data Seeding Script
-- Execute this in DB Browser for SQLite or similar tool
-- OpCentrix MES - Material Management Fix

-- Enable foreign key constraints
PRAGMA foreign_keys = ON;
 
-- Create Materials table if it doesn't exist
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

-- Create unique index on MaterialCode if it doesn't exist
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Materials_MaterialCode" ON "Materials" ("MaterialCode");

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS "IX_Materials_MaterialType" ON "Materials" ("MaterialType");
CREATE INDEX IF NOT EXISTS "IX_Materials_IsActive" ON "Materials" ("IsActive");
CREATE INDEX IF NOT EXISTS "IX_Materials_CompatibleMachineTypes" ON "Materials" ("CompatibleMachineTypes");

-- Check if materials already exist (count should return 0 if no materials exist)
-- If materials exist, this script will skip the inserts

-- Insert default materials only if none exist
INSERT OR IGNORE INTO "Materials" (
    "MaterialCode", "MaterialName", "MaterialType", "Description",
    "Density", "MeltingPointC", "IsActive", "CostPerGram",
    "DefaultLayerThicknessMicrons", "DefaultLaserPowerPercent", "DefaultScanSpeedMmPerSec",
    "MaterialProperties", "CompatibleMachineTypes", "SafetyNotes",
    "CreatedDate", "LastModifiedDate", "CreatedBy", "LastModifiedBy"
) VALUES
-- Ti-6Al-4V Grade 5
('TI64-G5', 'Ti-6Al-4V Grade 5', 'Titanium', 
 'Aerospace grade titanium alloy with excellent strength-to-weight ratio',
 4.43, 1660.0, 1, '0.45',
 30.0, 85.0, 1200.0,
 '{}', 'SLS', 'Handle with care. May cause respiratory irritation if inhaled.',
 datetime('now'), datetime('now'), 'System', 'System'),

-- Ti-6Al-4V Grade 23 (ELI)
('TI64-G23', 'Ti-6Al-4V Grade 23 (ELI)', 'Titanium',
 'Extra Low Interstitial grade for medical implants',
 4.43, 1660.0, 1, '0.65',
 25.0, 80.0, 1100.0,
 '{}', 'SLS', 'Medical grade material. Handle with care.',
 datetime('now'), datetime('now'), 'System', 'System'),

-- Inconel 718
('IN718', 'Inconel 718', 'Nickel',
 'High-temperature nickel-chromium superalloy',
 8.19, 1336.0, 1, '0.75',
 40.0, 90.0, 1000.0,
 '{}', 'SLS', 'High-temperature alloy. Use appropriate safety equipment.',
 datetime('now'), datetime('now'), 'System', 'System'),

-- Stainless Steel 316L
('SS316L', 'Stainless Steel 316L', 'Steel',
 'Austenitic stainless steel with low carbon content',
 8.0, 1400.0, 1, '0.15',
 30.0, 75.0, 1500.0,
 '{}', 'SLS', 'General purpose stainless steel. Standard safety precautions apply.',
 datetime('now'), datetime('now'), 'System', 'System'),

-- AlSi10Mg
('ALSI10MG', 'AlSi10Mg', 'Aluminum',
 'Aluminum-silicon alloy for lightweight applications',
 2.67, 570.0, 1, '0.25',
 30.0, 70.0, 1800.0,
 '{}', 'SLS', 'Aluminum powder - fire hazard. Keep away from ignition sources.',
 datetime('now'), datetime('now'), 'System', 'System');

-- Verification queries
SELECT 'Materials table created successfully' as Status;
SELECT COUNT(*) as 'Total Materials' FROM Materials;
SELECT MaterialCode, MaterialName, MaterialType FROM Materials ORDER BY MaterialType, MaterialCode;

-- Check database integrity
PRAGMA integrity_check;
PRAGMA foreign_key_check;