-- Migration: Add Print Tracking Tables
-- This adds the new BuildJob, BuildJobPart, and DelayLog tables to the existing OpCentrix database
-- Run this script to add the print tracking functionality

-- Create BuildJobs table for tracking actual print operations
CREATE TABLE IF NOT EXISTS BuildJobs (
    BuildId INTEGER PRIMARY KEY AUTOINCREMENT,
    PrinterName TEXT(10) NOT NULL,
    ActualStartTime DATETIME NOT NULL,
    ActualEndTime DATETIME,
    ScheduledStartTime DATETIME,
    ScheduledEndTime DATETIME,
    Status TEXT(50) NOT NULL DEFAULT 'In Progress',
    UserId INTEGER NOT NULL,
    Notes TEXT(1000),
    LaserRunTime TEXT(50),
    GasUsed_L REAL,
    PowderUsed_L REAL,
    ReasonForEnd TEXT(50),
    SetupNotes TEXT(1000),
    AssociatedScheduledJobId INTEGER,
    CreatedAt DATETIME NOT NULL DEFAULT (datetime('now')),
    CompletedAt DATETIME,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE RESTRICT
);

-- Create BuildJobParts table for tracking parts in each build
CREATE TABLE IF NOT EXISTS BuildJobParts (
    PartEntryId INTEGER PRIMARY KEY AUTOINCREMENT,
    BuildId INTEGER NOT NULL,
    PartNumber TEXT(50) NOT NULL,
    Quantity INTEGER NOT NULL DEFAULT 1,
    IsPrimary INTEGER NOT NULL DEFAULT 0, -- SQLite uses INTEGER for BOOLEAN
    Description TEXT(500),
    Material TEXT(100),
    EstimatedHours REAL NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT (datetime('now')),
    CreatedBy TEXT(100) NOT NULL DEFAULT '',
    FOREIGN KEY (BuildId) REFERENCES BuildJobs(BuildId) ON DELETE CASCADE
);

-- Create DelayLogs table for tracking delays
CREATE TABLE IF NOT EXISTS DelayLogs (
    DelayId INTEGER PRIMARY KEY AUTOINCREMENT,
    BuildId INTEGER NOT NULL,
    DelayReason TEXT(100) NOT NULL,
    DelayDuration INTEGER NOT NULL DEFAULT 0, -- in minutes
    Description TEXT(1000),
    CreatedAt DATETIME NOT NULL DEFAULT (datetime('now')),
    CreatedBy TEXT(100) NOT NULL DEFAULT '',
    FOREIGN KEY (BuildId) REFERENCES BuildJobs(BuildId) ON DELETE CASCADE
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS IX_BuildJobs_PrinterName ON BuildJobs(PrinterName);
CREATE INDEX IF NOT EXISTS IX_BuildJobs_ActualStartTime ON BuildJobs(ActualStartTime);
CREATE INDEX IF NOT EXISTS IX_BuildJobs_Status ON BuildJobs(Status);
CREATE INDEX IF NOT EXISTS IX_BuildJobs_UserId ON BuildJobs(UserId);
CREATE INDEX IF NOT EXISTS IX_BuildJobs_PrinterName_ActualStartTime ON BuildJobs(PrinterName, ActualStartTime);
CREATE INDEX IF NOT EXISTS IX_BuildJobs_AssociatedScheduledJobId ON BuildJobs(AssociatedScheduledJobId);

CREATE INDEX IF NOT EXISTS IX_BuildJobParts_BuildId ON BuildJobParts(BuildId);
CREATE INDEX IF NOT EXISTS IX_BuildJobParts_PartNumber ON BuildJobParts(PartNumber);
CREATE INDEX IF NOT EXISTS IX_BuildJobParts_IsPrimary ON BuildJobParts(IsPrimary);
CREATE INDEX IF NOT EXISTS IX_BuildJobParts_BuildId_IsPrimary ON BuildJobParts(BuildId, IsPrimary);

CREATE INDEX IF NOT EXISTS IX_DelayLogs_BuildId ON DelayLogs(BuildId);
CREATE INDEX IF NOT EXISTS IX_DelayLogs_DelayReason ON DelayLogs(DelayReason);
CREATE INDEX IF NOT EXISTS IX_DelayLogs_CreatedAt ON DelayLogs(CreatedAt);
CREATE INDEX IF NOT EXISTS IX_DelayLogs_DelayDuration ON DelayLogs(DelayDuration);

-- Insert some sample data for testing (optional)
-- Uncomment the following if you want test data

/*
-- Sample BuildJob
INSERT INTO BuildJobs (PrinterName, ActualStartTime, ActualEndTime, Status, UserId, Notes, ReasonForEnd, CreatedAt, CompletedAt)
VALUES ('TI1', datetime('now', '-4 hours'), datetime('now'), 'Completed', 1, 'Sample completed build for testing', 'Completed', datetime('now', '-4 hours'), datetime('now'));

-- Sample BuildJobParts for the above build
INSERT INTO BuildJobParts (BuildId, PartNumber, Quantity, IsPrimary, Description, Material, CreatedBy)
VALUES 
    (1, '14-5396', 2, 1, 'Aerospace Turbine Blade', 'Ti-6Al-4V Grade 5', 'System'),
    (1, '14-5397', 1, 0, 'Support Bracket', 'Ti-6Al-4V Grade 5', 'System');

-- Sample Active BuildJob
INSERT INTO BuildJobs (PrinterName, ActualStartTime, Status, UserId, SetupNotes, CreatedAt)
VALUES ('TI2', datetime('now', '-2 hours'), 'In Progress', 1, 'Fresh powder loaded, argon purged', datetime('now', '-2 hours'));

-- Sample DelayLog
INSERT INTO DelayLogs (BuildId, DelayReason, DelayDuration, Description, CreatedBy)
VALUES (1, 'Argon Refill', 30, 'Had to refill argon tank before starting', 'System');
*/

-- Migration complete
SELECT 'Print tracking tables created successfully!' as Result;