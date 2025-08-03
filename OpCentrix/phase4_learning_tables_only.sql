-- Phase 4 Learning Tables Creation Script
-- Date: February 8, 2025
-- Purpose: Create ONLY the Phase 4 learning tables (not the Jobs table)
-- Status: Safe to run - uses IF NOT EXISTS

PRAGMA foreign_keys = ON;

-- 1. PartCompletionLogs - exactly as defined in migration
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
    
    CONSTRAINT "FK_PartCompletionLogs_BuildJobs_BuildJobId" 
        FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
);

-- 2. OperatorEstimateLogs - exactly as defined in migration
CREATE TABLE IF NOT EXISTS "OperatorEstimateLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_OperatorEstimateLogs" PRIMARY KEY AUTOINCREMENT,
    "BuildJobId" INTEGER NOT NULL,
    "EstimatedHours" DECIMAL(5,2) NOT NULL,
    "TimeFactors" TEXT,
    "OperatorNotes" TEXT,
    "LoggedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    
    CONSTRAINT "FK_OperatorEstimateLogs_BuildJobs_BuildJobId" 
        FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
);

-- 3. BuildTimeLearningData - exactly as defined in migration
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
    
    CONSTRAINT "FK_BuildTimeLearningData_BuildJobs_BuildJobId" 
        FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
);

-- Create all indexes from the migration
CREATE INDEX IF NOT EXISTS "IX_PartCompletionLogs_BuildJobId" ON "PartCompletionLogs" ("BuildJobId");
CREATE INDEX IF NOT EXISTS "IX_PartCompletionLogs_PartNumber" ON "PartCompletionLogs" ("PartNumber");
CREATE INDEX IF NOT EXISTS "IX_PartCompletionLogs_IsPrimary" ON "PartCompletionLogs" ("IsPrimary");
CREATE INDEX IF NOT EXISTS "IX_PartCompletionLogs_CompletedAt" ON "PartCompletionLogs" ("CompletedAt");
CREATE INDEX IF NOT EXISTS "IX_PartCompletionLogs_BuildJobId_PartNumber" ON "PartCompletionLogs" ("BuildJobId", "PartNumber");

CREATE INDEX IF NOT EXISTS "IX_OperatorEstimateLogs_BuildJobId" ON "OperatorEstimateLogs" ("BuildJobId");
CREATE INDEX IF NOT EXISTS "IX_OperatorEstimateLogs_LoggedAt" ON "OperatorEstimateLogs" ("LoggedAt");
CREATE INDEX IF NOT EXISTS "IX_OperatorEstimateLogs_EstimatedHours" ON "OperatorEstimateLogs" ("EstimatedHours");

CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_BuildJobId" ON "BuildTimeLearningData" ("BuildJobId");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_MachineId" ON "BuildTimeLearningData" ("MachineId");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_BuildFileHash" ON "BuildTimeLearningData" ("BuildFileHash");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_RecordedAt" ON "BuildTimeLearningData" ("RecordedAt");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_SupportComplexity" ON "BuildTimeLearningData" ("SupportComplexity");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_QualityScore" ON "BuildTimeLearningData" ("QualityScore");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_MachineId_BuildFileHash" ON "BuildTimeLearningData" ("MachineId", "BuildFileHash");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_MachineId_SupportComplexity" ON "BuildTimeLearningData" ("MachineId", "SupportComplexity");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_BuildFileHash_SupportComplexity" ON "BuildTimeLearningData" ("BuildFileHash", "SupportComplexity");
CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_MachineId_TotalParts_SupportComplexity" ON "BuildTimeLearningData" ("MachineId", "TotalParts", "SupportComplexity");

-- Verify tables were created
SELECT 'Phase 4 learning tables created successfully!' as Status;

-- Show the created tables
SELECT name as TableName 
FROM sqlite_master 
WHERE type='table' AND name IN ('PartCompletionLogs', 'OperatorEstimateLogs', 'BuildTimeLearningData')
ORDER BY name;

-- Run integrity check
PRAGMA integrity_check;