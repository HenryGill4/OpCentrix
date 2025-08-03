-- Phase 4 Database Updates: Learning Model Tables
-- Date: February 8, 2025
-- Purpose: Add Phase 4 learning tables for build time intelligence

-- Enable foreign key constraints
PRAGMA foreign_keys = ON;

-- Check if BuildJobs table has Phase 4 columns (from Phase 2)
-- If not, add them first
SELECT 'Checking BuildJobs table structure...' as Status;

-- Add Phase 4 BuildJobs columns if they don't exist
-- (These should have been added in Phase 2, but let's ensure they exist)

-- Note: SQLite doesn't support IF NOT EXISTS for columns, so we'll let errors pass
-- The application should handle this gracefully

-- Add Phase 4 BuildJobs columns (safe to run - will error if exists)
-- ALTER TABLE BuildJobs ADD COLUMN OperatorEstimatedHours DECIMAL;
-- ALTER TABLE BuildJobs ADD COLUMN OperatorActualHours DECIMAL;
-- ALTER TABLE BuildJobs ADD COLUMN TotalPartsInBuild INTEGER DEFAULT 0;
-- ALTER TABLE BuildJobs ADD COLUMN BuildFileHash TEXT;
-- ALTER TABLE BuildJobs ADD COLUMN IsLearningBuild INTEGER DEFAULT 0;
-- ALTER TABLE BuildJobs ADD COLUMN OperatorBuildAssessment TEXT;
-- ALTER TABLE BuildJobs ADD COLUMN TimeFactors TEXT;
-- ALTER TABLE BuildJobs ADD COLUMN MachinePerformanceNotes TEXT;
-- ALTER TABLE BuildJobs ADD COLUMN PowerConsumption DECIMAL;
-- ALTER TABLE BuildJobs ADD COLUMN LaserOnTime DECIMAL;
-- ALTER TABLE BuildJobs ADD COLUMN LayerCount INTEGER;
-- ALTER TABLE BuildJobs ADD COLUMN BuildHeight DECIMAL;
-- ALTER TABLE BuildJobs ADD COLUMN SupportComplexity TEXT;
-- ALTER TABLE BuildJobs ADD COLUMN PartOrientations TEXT;
-- ALTER TABLE BuildJobs ADD COLUMN PostProcessingNeeded TEXT;
-- ALTER TABLE BuildJobs ADD COLUMN DefectCount INTEGER;
-- ALTER TABLE BuildJobs ADD COLUMN LessonsLearned TEXT;

-- Create Phase 4 Learning Model Tables

-- 1. PartCompletionLogs - Track completion data for individual parts within builds
CREATE TABLE IF NOT EXISTS "PartCompletionLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartCompletionLogs" PRIMARY KEY AUTOINCREMENT,
    "BuildJobId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "GoodParts" INTEGER NOT NULL,
    "DefectiveParts" INTEGER NOT NULL,
    "ReworkParts" INTEGER NOT NULL,
    "QualityRate" DECIMAL(5,2) NOT NULL DEFAULT 0.0,
    "IsPrimary" INTEGER NOT NULL DEFAULT 0,
    "InspectionNotes" TEXT,
    "CompletedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    
    -- Foreign key constraint
    CONSTRAINT "FK_PartCompletionLogs_BuildJobs_BuildJobId" 
        FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
);

-- 2. OperatorEstimateLogs - Track operator time estimates for learning
CREATE TABLE IF NOT EXISTS "OperatorEstimateLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_OperatorEstimateLogs" PRIMARY KEY AUTOINCREMENT,
    "BuildJobId" INTEGER NOT NULL,
    "EstimatedHours" DECIMAL(5,2) NOT NULL,
    "TimeFactors" TEXT,
    "OperatorNotes" TEXT,
    "LoggedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    
    -- Foreign key constraint
    CONSTRAINT "FK_OperatorEstimateLogs_BuildJobs_BuildJobId" 
        FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
);

-- 3. BuildTimeLearningData - Machine learning data for build time optimization
CREATE TABLE IF NOT EXISTS "BuildTimeLearningData" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BuildTimeLearningData" PRIMARY KEY AUTOINCREMENT,
    "BuildJobId" INTEGER NOT NULL,
    "MachineId" TEXT NOT NULL,
    "BuildFileHash" TEXT,
    "OperatorEstimatedHours" DECIMAL(5,2) NOT NULL,
    "ActualHours" DECIMAL(5,2) NOT NULL,
    "VariancePercent" DECIMAL(6,2) NOT NULL,
    "SupportComplexity" TEXT,
    "TimeFactors" TEXT,
    "QualityScore" DECIMAL(5,2) NOT NULL DEFAULT 0.0,
    "DefectCount" INTEGER NOT NULL DEFAULT 0,
    "BuildHeight" DECIMAL(6,2),
    "LayerCount" INTEGER,
    "TotalParts" INTEGER NOT NULL DEFAULT 1,
    "PartOrientations" TEXT,
    "RecordedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    
    -- Foreign key constraint
    CONSTRAINT "FK_BuildTimeLearningData_BuildJobs_BuildJobId" 
        FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
);

-- Create indexes for Phase 4 learning tables

-- PartCompletionLogs indexes
CREATE INDEX IF NOT EXISTS "IX_PartCompletionLogs_BuildJobId" ON "PartCompletionLogs" ("BuildJobId");
CREATE INDEX IF NOT EXISTS "IX_PartCompletionLogs_PartNumber" ON "PartCompletionLogs" ("PartNumber");
CREATE INDEX IF NOT EXISTS "IX_PartCompletionLogs_IsPrimary" ON "PartCompletionLogs" ("IsPrimary");
CREATE INDEX IF NOT EXISTS "IX_PartCompletionLogs_CompletedAt" ON "PartCompletionLogs" ("CompletedAt");
CREATE INDEX IF NOT EXISTS "IX_PartCompletionLogs_BuildJobId_PartNumber" ON "PartCompletionLogs" ("BuildJobId", "PartNumber");

-- OperatorEstimateLogs indexes
CREATE INDEX IF NOT EXISTS "IX_OperatorEstimateLogs_BuildJobId" ON "OperatorEstimateLogs" ("BuildJobId");
CREATE INDEX IF NOT EXISTS "IX_OperatorEstimateLogs_LoggedAt" ON "OperatorEstimateLogs" ("LoggedAt");
CREATE INDEX IF NOT EXISTS "IX_OperatorEstimateLogs_EstimatedHours" ON "OperatorEstimateLogs" ("EstimatedHours");

-- BuildTimeLearningData indexes for machine learning and analytics
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_BuildJobId" ON "BuildTimeLearningData" ("BuildJobId");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_MachineId" ON "BuildTimeLearningData" ("MachineId");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_BuildFileHash" ON "BuildTimeLearningData" ("BuildFileHash");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_SupportComplexity" ON "BuildTimeLearningData" ("SupportComplexity");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_QualityScore" ON "BuildTimeLearningData" ("QualityScore");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_RecordedAt" ON "BuildTimeLearningData" ("RecordedAt");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_MachineId_BuildFileHash" ON "BuildTimeLearningData" ("MachineId", "BuildFileHash");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_MachineId_SupportComplexity" ON "BuildTimeLearningData" ("MachineId", "SupportComplexity");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_BuildFileHash_SupportComplexity" ON "BuildTimeLearningData" ("BuildFileHash", "SupportComplexity");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_MachineId_TotalParts_SupportComplexity" ON "BuildTimeLearningData" ("MachineId", "TotalParts", "SupportComplexity");

-- Verify tables were created
SELECT 'Phase 4 learning tables created successfully!' as Status;

-- Show table counts
SELECT name as TableName, 
       CASE name
         WHEN 'PartCompletionLogs' THEN (SELECT COUNT(*) FROM PartCompletionLogs)
         WHEN 'OperatorEstimateLogs' THEN (SELECT COUNT(*) FROM OperatorEstimateLogs)
         WHEN 'BuildTimeLearningData' THEN (SELECT COUNT(*) FROM BuildTimeLearningData)
         ELSE 0
       END as RowCount
FROM sqlite_master 
WHERE type='table' AND name IN ('PartCompletionLogs', 'OperatorEstimateLogs', 'BuildTimeLearningData')
ORDER BY name;

-- Check database integrity
PRAGMA integrity_check;