-- Fix for SQLite Error 1: 'no such column: p.StageTemplateId'
-- This addresses the database schema mismatch

-- Add the missing StageTemplateId column to PartStageRequirements table
ALTER TABLE PartStageRequirements ADD COLUMN StageTemplateId INTEGER REFERENCES StageTemplates(Id) ON DELETE SET NULL;

-- Add the missing WorkflowTemplateId column if it doesn't exist
ALTER TABLE PartStageRequirements ADD COLUMN WorkflowTemplateId INTEGER REFERENCES WorkflowTemplates(Id) ON DELETE SET NULL;

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS IX_PartStageRequirements_StageTemplateId ON PartStageRequirements(StageTemplateId);
CREATE INDEX IF NOT EXISTS IX_PartStageRequirements_WorkflowTemplateId ON PartStageRequirements(WorkflowTemplateId);

-- Verify the columns were added successfully
PRAGMA table_info(PartStageRequirements);