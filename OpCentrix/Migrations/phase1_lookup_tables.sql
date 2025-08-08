-- OpCentrix Part Form Refactor - Phase 1: Foundation Setup
-- Creating Lookup Tables for Component Types, Compliance Categories, Asset Links, and Legacy Mapping

-- ============================================================================
-- 1.1 Create ComponentTypes Table
-- ============================================================================
CREATE TABLE ComponentTypes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Description TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    SortOrder INTEGER NOT NULL DEFAULT 100,
    CreatedDate TEXT NOT NULL DEFAULT '2025-01-30 12:00:00',
    CreatedBy TEXT NOT NULL DEFAULT 'System',
    LastModifiedDate TEXT NOT NULL DEFAULT '2025-01-30 12:00:00',
    LastModifiedBy TEXT NOT NULL DEFAULT 'System'
);

-- ============================================================================
-- 1.2 Create ComplianceCategories Table  
-- ============================================================================
CREATE TABLE ComplianceCategories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Description TEXT NOT NULL,
    RegulatoryLevel TEXT NOT NULL DEFAULT 'Standard', -- 'Standard', 'Regulated', 'Restricted'
    RequiresSpecialHandling INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1,
    SortOrder INTEGER NOT NULL DEFAULT 100,
    CreatedDate TEXT NOT NULL DEFAULT '2025-01-30 12:00:00',
    CreatedBy TEXT NOT NULL DEFAULT 'System',
    LastModifiedDate TEXT NOT NULL DEFAULT '2025-01-30 12:00:00',
    LastModifiedBy TEXT NOT NULL DEFAULT 'System'
);

-- ============================================================================
-- 1.3 Create PartAssetLinks Table
-- ============================================================================
CREATE TABLE PartAssetLinks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PartId INTEGER NOT NULL,
    Url TEXT NOT NULL,
    DisplayName TEXT NOT NULL,
    Source TEXT NOT NULL DEFAULT 'Upload', -- 'Upload', 'External', 'Generated'
    AssetType TEXT NOT NULL DEFAULT '3DModel', -- '3DModel', 'Photo', 'Drawing', 'Document'
    LastCheckedUtc TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedDate TEXT NOT NULL DEFAULT '2025-01-30 12:00:00',
    CreatedBy TEXT NOT NULL DEFAULT 'System',
    FOREIGN KEY (PartId) REFERENCES Parts(Id) ON DELETE CASCADE
);

-- ============================================================================
-- 1.4 Create Legacy Flag to Stage Mapping Table
-- ============================================================================
CREATE TABLE LegacyFlagToStageMap (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    LegacyFieldName TEXT NOT NULL UNIQUE, -- 'RequiresSLSPrinting', 'RequiresCNCMachining', etc.
    ProductionStageName TEXT NOT NULL,     -- 'SLS Printing', 'CNC Machining', etc.
    ExecutionOrder INTEGER NOT NULL,       -- 1, 2, 3, etc.
    DefaultSetupMinutes INTEGER NOT NULL DEFAULT 30,
    DefaultTeardownMinutes INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1
);

-- ============================================================================
-- 1.5 Add Foreign Key Columns to Parts Table
-- ============================================================================
ALTER TABLE Parts ADD COLUMN ComponentTypeId INTEGER;
ALTER TABLE Parts ADD COLUMN ComplianceCategoryId INTEGER;
ALTER TABLE Parts ADD COLUMN IsLegacyForm INTEGER NOT NULL DEFAULT 1;

-- ============================================================================
-- 1.6 Seed Component Types Data
-- ============================================================================
INSERT INTO ComponentTypes (Name, Description, SortOrder) VALUES
('General', 'General manufacturing components - standard parts', 1),
('Serialized', 'Serialized components requiring tracking and documentation', 2);

-- ============================================================================
-- 1.7 Seed Compliance Categories Data
-- ============================================================================
INSERT INTO ComplianceCategories (Name, Description, RegulatoryLevel, RequiresSpecialHandling, SortOrder) VALUES
('Non NFA', 'Non-NFA items - standard regulatory requirements', 'Standard', 0, 1),
('NFA', 'NFA regulated items - requires ATF compliance and special handling', 'Regulated', 1, 2);

-- ============================================================================
-- 1.8 Seed Legacy Flag to Stage Mapping Data
-- ============================================================================
INSERT INTO LegacyFlagToStageMap (LegacyFieldName, ProductionStageName, ExecutionOrder, DefaultSetupMinutes, DefaultTeardownMinutes) VALUES
('RequiresSLSPrinting', 'SLS Printing', 1, 45, 30),
('RequiresEDMOperations', 'EDM Operations', 2, 30, 15),
('RequiresCNCMachining', 'CNC Machining', 3, 60, 20),
('RequiresAssembly', 'Assembly', 4, 15, 10),
('RequiresFinishing', 'Finishing', 5, 30, 15);

-- ============================================================================
-- 1.9 Create Indexes for Performance
-- ============================================================================
CREATE INDEX IX_ComponentTypes_IsActive ON ComponentTypes(IsActive);
CREATE INDEX IX_ComponentTypes_SortOrder ON ComponentTypes(SortOrder);

CREATE INDEX IX_ComplianceCategories_IsActive ON ComplianceCategories(IsActive);
CREATE INDEX IX_ComplianceCategories_RegulatoryLevel ON ComplianceCategories(RegulatoryLevel);
CREATE INDEX IX_ComplianceCategories_SortOrder ON ComplianceCategories(SortOrder);

CREATE INDEX IX_PartAssetLinks_PartId ON PartAssetLinks(PartId);
CREATE INDEX IX_PartAssetLinks_AssetType ON PartAssetLinks(AssetType);
CREATE INDEX IX_PartAssetLinks_IsActive ON PartAssetLinks(IsActive);

CREATE INDEX IX_LegacyFlagToStageMap_LegacyFieldName ON LegacyFlagToStageMap(LegacyFieldName);
CREATE INDEX IX_LegacyFlagToStageMap_ProductionStageName ON LegacyFlagToStageMap(ProductionStageName);
CREATE INDEX IX_LegacyFlagToStageMap_ExecutionOrder ON LegacyFlagToStageMap(ExecutionOrder);

CREATE INDEX IX_Parts_ComponentTypeId ON Parts(ComponentTypeId);
CREATE INDEX IX_Parts_ComplianceCategoryId ON Parts(ComplianceCategoryId);
CREATE INDEX IX_Parts_IsLegacyForm ON Parts(IsLegacyForm);

-- ============================================================================
-- Phase 1 Complete: Foundation Setup
-- ============================================================================
-- Summary of changes:
-- ? Created ComponentTypes lookup table with 'General' and 'Serialized' options
-- ? Created ComplianceCategories lookup table with 'Non NFA' and 'NFA' options  
-- ? Created PartAssetLinks table for 3D models, photos, and documents
-- ? Created LegacyFlagToStageMap table for automated migration mapping
-- ? Added foreign key columns to Parts table (ComponentTypeId, ComplianceCategoryId, IsLegacyForm)
-- ? Seeded lookup data with initial values
-- ? Created performance indexes
-- 
-- Ready for Phase 2: Data Migration