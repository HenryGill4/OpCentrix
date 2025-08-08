-- PHASE 6: Stage Template System Database Migration
-- Create tables for stage templates and categories

-- Create StageTemplateCategories table
CREATE TABLE IF NOT EXISTS StageTemplateCategories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name VARCHAR(50) NOT NULL,
    Description VARCHAR(200) DEFAULT '',
    Icon VARCHAR(50) DEFAULT 'fas fa-cogs',
    ColorCode VARCHAR(7) DEFAULT '#007bff',
    IsActive BOOLEAN DEFAULT 1,
    SortOrder INTEGER DEFAULT 100,
    UNIQUE(Name)
);

-- Create StageTemplates table
CREATE TABLE IF NOT EXISTS StageTemplates (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500) NOT NULL,
    Industry VARCHAR(50) DEFAULT 'General',
    MaterialType VARCHAR(50) DEFAULT 'Metal',
    ComplexityLevel VARCHAR(50) DEFAULT 'Medium',
    IsActive BOOLEAN DEFAULT 1,
    IsDefault BOOLEAN DEFAULT 0,
    SortOrder INTEGER DEFAULT 100,
    TemplateConfiguration TEXT DEFAULT '{}',
    EstimatedTotalHours DECIMAL(8,2) DEFAULT 0,
    EstimatedTotalCost DECIMAL(12,2) DEFAULT 0,
    UsageCount INTEGER DEFAULT 0,
    LastUsedDate DATETIME NULL,
    CreatedBy VARCHAR(100) DEFAULT 'System',
    CreatedDate DATETIME DEFAULT (datetime('now')),
    LastModifiedBy VARCHAR(100) DEFAULT 'System',
    LastModifiedDate DATETIME DEFAULT (datetime('now'))
);

-- Create StageTemplateSteps table
CREATE TABLE IF NOT EXISTS StageTemplateSteps (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    StageTemplateId INTEGER NOT NULL,
    ProductionStageId INTEGER NOT NULL,
    ExecutionOrder INTEGER DEFAULT 1,
    EstimatedHours DECIMAL(8,2) DEFAULT 1.0,
    HourlyRate DECIMAL(8,2) DEFAULT 85.00,
    MaterialCost DECIMAL(10,2) DEFAULT 0.00,
    SetupTimeMinutes INTEGER DEFAULT 30,
    TeardownTimeMinutes INTEGER DEFAULT 0,
    IsRequired BOOLEAN DEFAULT 1,
    IsParallel BOOLEAN DEFAULT 0,
    StageConfiguration TEXT DEFAULT '{}',
    QualityRequirements VARCHAR(1000) DEFAULT '',
    SpecialInstructions VARCHAR(1000) DEFAULT '',
    FOREIGN KEY (StageTemplateId) REFERENCES StageTemplates(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductionStageId) REFERENCES ProductionStages(Id) ON DELETE RESTRICT,
    UNIQUE(StageTemplateId, ProductionStageId)
);

-- Add AppliedTemplateId column to Parts table
ALTER TABLE Parts ADD COLUMN AppliedTemplateId INTEGER REFERENCES StageTemplates(Id) ON DELETE SET NULL;

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS IX_StageTemplates_Name ON StageTemplates(Name);
CREATE INDEX IF NOT EXISTS IX_StageTemplates_Industry ON StageTemplates(Industry);
CREATE INDEX IF NOT EXISTS IX_StageTemplates_MaterialType ON StageTemplates(MaterialType);
CREATE INDEX IF NOT EXISTS IX_StageTemplates_ComplexityLevel ON StageTemplates(ComplexityLevel);
CREATE INDEX IF NOT EXISTS IX_StageTemplates_IsActive ON StageTemplates(IsActive);
CREATE INDEX IF NOT EXISTS IX_StageTemplates_UsageCount ON StageTemplates(UsageCount);
CREATE INDEX IF NOT EXISTS IX_StageTemplates_Industry_Material_Complexity ON StageTemplates(Industry, MaterialType, ComplexityLevel);

CREATE INDEX IF NOT EXISTS IX_StageTemplateSteps_StageTemplateId ON StageTemplateSteps(StageTemplateId);
CREATE INDEX IF NOT EXISTS IX_StageTemplateSteps_ProductionStageId ON StageTemplateSteps(ProductionStageId);
CREATE INDEX IF NOT EXISTS IX_StageTemplateSteps_Template_Order ON StageTemplateSteps(StageTemplateId, ExecutionOrder);

CREATE INDEX IF NOT EXISTS IX_StageTemplateCategories_Name ON StageTemplateCategories(Name);
CREATE INDEX IF NOT EXISTS IX_StageTemplateCategories_IsActive ON StageTemplateCategories(IsActive);
CREATE INDEX IF NOT EXISTS IX_StageTemplateCategories_SortOrder ON StageTemplateCategories(SortOrder);

CREATE INDEX IF NOT EXISTS IX_Parts_AppliedTemplateId ON Parts(AppliedTemplateId);