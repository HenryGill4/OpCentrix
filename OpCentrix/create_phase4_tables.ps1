Write-Host "Starting Phase 4 table creation..."

# Try to execute SQL commands individually
$commands = @(
    'PRAGMA foreign_keys = ON;',
    'CREATE TABLE IF NOT EXISTS "PartCompletionLogs" (
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
        "CompletedAt" TEXT NOT NULL DEFAULT (datetime(''now'')),
        CONSTRAINT "FK_PartCompletionLogs_BuildJobs_BuildJobId" 
            FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
    );',
    'CREATE TABLE IF NOT EXISTS "OperatorEstimateLogs" (
        "Id" INTEGER NOT NULL CONSTRAINT "PK_OperatorEstimateLogs" PRIMARY KEY AUTOINCREMENT,
        "BuildJobId" INTEGER NOT NULL,
        "EstimatedHours" DECIMAL(5,2) NOT NULL,
        "TimeFactors" TEXT,
        "OperatorNotes" TEXT,
        "LoggedAt" TEXT NOT NULL DEFAULT (datetime(''now'')),
        CONSTRAINT "FK_OperatorEstimateLogs_BuildJobs_BuildJobId" 
            FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
    );',
    'CREATE TABLE IF NOT EXISTS "BuildTimeLearningData" (
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
        "RecordedAt" TEXT NOT NULL DEFAULT (datetime(''now'')),
        CONSTRAINT "FK_BuildTimeLearningData_BuildJobs_BuildJobId" 
            FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
    );'
)

# Create indexes
$indexes = @(
    'CREATE INDEX IF NOT EXISTS "IX_PartCompletionLogs_BuildJobId" ON "PartCompletionLogs" ("BuildJobId");',
    'CREATE INDEX IF NOT EXISTS "IX_PartCompletionLogs_PartNumber" ON "PartCompletionLogs" ("PartNumber");',
    'CREATE INDEX IF NOT EXISTS "IX_OperatorEstimateLogs_BuildJobId" ON "OperatorEstimateLogs" ("BuildJobId");',
    'CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_BuildJobId" ON "BuildTimeLearningData" ("BuildJobId");',
    'CREATE INDEX IF NOT EXISTS "IX_BuildTimeLearningData_MachineId" ON "BuildTimeLearningData" ("MachineId");'
)

$allCommands = $commands + $indexes

foreach ($cmd in $allCommands) {
    Write-Host "Executing: $($cmd.Substring(0, [Math]::Min(50, $cmd.Length)))..."
    try {
        sqlite3 scheduler.db $cmd
        Write-Host "  ? Success"
    }
    catch {
        Write-Host "  ? Error: $_"
    }
}

Write-Host "Phase 4 table creation completed."