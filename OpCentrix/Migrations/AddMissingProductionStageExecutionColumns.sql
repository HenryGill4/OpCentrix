-- Add missing ActualEndTime column to ProductionStageExecutions table
-- This fixes the SQLite Error 1: 'no such column: p1.ActualEndTime' issue

BEGIN TRANSACTION;

-- Check if column exists
PRAGMA table_info(ProductionStageExecutions);

-- Add ActualEndTime column if it doesn't exist
ALTER TABLE ProductionStageExecutions ADD COLUMN ActualEndTime DATETIME;

-- Add ActualStartTime column if it doesn't exist (might also be missing)
ALTER TABLE ProductionStageExecutions ADD COLUMN ActualStartTime DATETIME;

-- Add OperatorName column if it doesn't exist
ALTER TABLE ProductionStageExecutions ADD COLUMN OperatorName TEXT;

-- Add CreatedBy column if it doesn't exist
ALTER TABLE ProductionStageExecutions ADD COLUMN CreatedBy TEXT;

-- Add LastModifiedBy column if it doesn't exist
ALTER TABLE ProductionStageExecutions ADD COLUMN LastModifiedBy TEXT;

-- Add LastModifiedDate column if it doesn't exist
ALTER TABLE ProductionStageExecutions ADD COLUMN LastModifiedDate DATETIME;

COMMIT;