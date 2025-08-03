# ?? OpCentrix Database Diagnostic Script - LIVE DATABASE VERSION
# Run this script for quick database health check and troubleshooting
# Updated to match live database with 36 active tables

param(
    [switch]$Detailed,
    [switch]$Repair,
    [switch]$Backup
)

Write-Host "?? OpCentrix Database Diagnostic Tool (Live DB)" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Step 1: Check if database exists
Write-Host "`n?? Checking database file..." -ForegroundColor Yellow
$dbPath = "OpCentrix/scheduler.db"
if (Test-Path $dbPath) {
    $dbSize = (Get-Item $dbPath).Length / 1MB
    Write-Host "? Database exists: $dbPath (Size: $($dbSize.ToString('F2')) MB)" -ForegroundColor Green
} else {
    Write-Host "? Database not found at: $dbPath" -ForegroundColor Red
    Write-Host "   Run: cd OpCentrix && dotnet ef database update" -ForegroundColor Yellow
    exit 1
}

# Step 2: Test SQLite accessibility and get table count
Write-Host "`n?? Testing database connectivity..." -ForegroundColor Yellow
try {
    $tableCount = sqlite3 $dbPath "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name != '__EFMigrationsHistory';"
    if ($tableCount -eq 36) {
        Write-Host "? Database is accessible with correct table count ($tableCount tables)" -ForegroundColor Green
    } elseif ($tableCount -gt 0) {
        Write-Host "??  Database accessible but table count is $tableCount (expected: 36)" -ForegroundColor Yellow
    } else {
        Write-Host "? Database accessible but no tables found" -ForegroundColor Red
    }
} catch {
    Write-Host "? Cannot access database: $($_.Exception.Message)" -ForegroundColor Red
    if ($Repair) {
        Write-Host "?? Attempting repair..." -ForegroundColor Yellow
        sqlite3 $dbPath "PRAGMA integrity_check;"
    }
    exit 1
}

# Step 3: Check critical tables from live database
Write-Host "`n?? Checking critical tables..." -ForegroundColor Yellow
$criticalTables = @('Jobs', 'Parts', 'Machines', 'Users', 'ProductionStages', 'PartClassifications', 'BuildJobs', 'SerialNumbers')
$missingTables = @()

foreach ($table in $criticalTables) {
    try {
        $exists = sqlite3 $dbPath "SELECT name FROM sqlite_master WHERE type='table' AND name='$table';"
        if ($exists) {
            $rowCount = sqlite3 $dbPath "SELECT COUNT(*) FROM $table;"
            Write-Host "? $table exists ($rowCount rows)" -ForegroundColor Green
        } else {
            Write-Host "? $table missing" -ForegroundColor Red
            $missingTables += $table
        }
    } catch {
        Write-Host "? Error checking $table : $($_.Exception.Message)" -ForegroundColor Red
        $missingTables += $table
    }
}

if ($missingTables.Count -gt 0) {
    Write-Host "`n?? Missing critical tables: $($missingTables -join ', ')" -ForegroundColor Red
    Write-Host "   Run: cd OpCentrix && dotnet ef database update" -ForegroundColor Yellow
}

# Step 4: Check Entity Framework status
Write-Host "`n??? Checking Entity Framework status..." -ForegroundColor Yellow
try {
    Push-Location "OpCentrix"
    $migrationsList = dotnet ef migrations list 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Entity Framework connection successful" -ForegroundColor Green
        if ($Detailed) {
            Write-Host "?? Applied migrations:" -ForegroundColor Cyan
            $migrationsList | Where-Object { $_ -notmatch "warn|info|debug" } | ForEach-Object { Write-Host "   $_" }
        }
    } else {
        Write-Host "? Entity Framework connection failed" -ForegroundColor Red
        Write-Host "   Error: $migrationsList" -ForegroundColor Red
    }
} catch {
    Write-Host "? Entity Framework error: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    Pop-Location
}

# Step 5: Database integrity check
Write-Host "`n?? Running database integrity check..." -ForegroundColor Yellow
try {
    $integrityResult = sqlite3 $dbPath "PRAGMA integrity_check;"
    if ($integrityResult -eq "ok") {
        Write-Host "? Database integrity: OK" -ForegroundColor Green
    } else {
        Write-Host "? Database integrity issues found:" -ForegroundColor Red
        Write-Host "   $integrityResult" -ForegroundColor Red
    }
} catch {
    Write-Host "? Integrity check failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 6: Check foreign key constraints (if detailed)
if ($Detailed) {
    Write-Host "`n?? Checking foreign key constraints..." -ForegroundColor Yellow
    try {
        $foreignKeyCheck = sqlite3 $dbPath "PRAGMA foreign_key_check;"
        if (-not $foreignKeyCheck) {
            Write-Host "? Foreign key constraints: OK" -ForegroundColor Green
        } else {
            Write-Host "? Foreign key constraint violations:" -ForegroundColor Red
            Write-Host "   $foreignKeyCheck" -ForegroundColor Red
        }
    } catch {
        Write-Host "? Foreign key check failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Step 7: Check live database critical indexes
if ($Detailed) {
    Write-Host "`n?? Checking critical indexes..." -ForegroundColor Yellow
    $criticalIndexes = @(
        'IX_Jobs_MachineId_ScheduledStart',
        'IX_Parts_PartNumber',
        'IX_Machines_MachineId',
        'IX_ProductionStages_DisplayOrder'
    )
    
    foreach ($index in $criticalIndexes) {
        try {
            $exists = sqlite3 $dbPath "SELECT name FROM sqlite_master WHERE type='index' AND name='$index';"
            if ($exists) {
                Write-Host "? Index $index exists" -ForegroundColor Green
            } else {
                Write-Host "? Index $index missing" -ForegroundColor Red
            }
        } catch {
            Write-Host "? Error checking index $index : $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

# Step 8: Performance analysis (if detailed)
if ($Detailed) {
    Write-Host "`n? Database performance analysis..." -ForegroundColor Yellow
    try {
        $pageCount = sqlite3 $dbPath "PRAGMA page_count;"
        $pageSize = sqlite3 $dbPath "PRAGMA page_size;"
        $freeListCount = sqlite3 $dbPath "PRAGMA freelist_count;"
        $totalSize = ($pageCount * $pageSize) / 1MB
        $freeSpace = ($freeListCount * $pageSize) / 1MB
        $usedSpace = $totalSize - $freeSpace
        
        Write-Host "?? Database Statistics:" -ForegroundColor Cyan
        Write-Host "   Total Size: $($totalSize.ToString('F2')) MB" 
        Write-Host "   Used Space: $($usedSpace.ToString('F2')) MB"
        Write-Host "   Free Space: $($freeSpace.ToString('F2')) MB"
        Write-Host "   Pages: $pageCount (Size: $pageSize bytes each)"
        Write-Host "   Tables: $tableCount (Expected: 36)"
        
        if ($freeSpace -gt ($totalSize * 0.25)) {
            Write-Host "?? Consider running VACUUM to reclaim space" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "? Performance analysis failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Step 9: Live database row counts (if detailed)
if ($Detailed) {
    Write-Host "`n?? Live database row counts..." -ForegroundColor Yellow
    try {
        $rowCounts = sqlite3 $dbPath "
        SELECT name as TableName, 
               CASE name
                 WHEN 'Jobs' THEN (SELECT COUNT(*) FROM Jobs)
                 WHEN 'Parts' THEN (SELECT COUNT(*) FROM Parts WHERE IsActive = 1)
                 WHEN 'Machines' THEN (SELECT COUNT(*) FROM Machines WHERE IsActive = 1)
                 WHEN 'Users' THEN (SELECT COUNT(*) FROM Users WHERE IsActive = 1)
                 WHEN 'ProductionStages' THEN (SELECT COUNT(*) FROM ProductionStages WHERE IsActive = 1)
                 WHEN 'PartClassifications' THEN (SELECT COUNT(*) FROM PartClassifications WHERE IsActive = 1)
                 ELSE 0
               END as RowCount
        FROM sqlite_master 
        WHERE type='table' AND name IN ('Jobs', 'Parts', 'Machines', 'Users', 'ProductionStages', 'PartClassifications')
        ORDER BY name;"
        
        Write-Host "?? Key Table Row Counts:" -ForegroundColor Cyan
        $rowCounts -split "`n" | ForEach-Object {
            if ($_.Trim()) {
                $parts = $_.Split('|')
                if ($parts.Length -eq 2) {
                    $tableName = $parts[0].Trim()
                    $count = $parts[1].Trim()
                    Write-Host "   $tableName : $count rows"
                }
            }
        }
    } catch {
        Write-Host "? Row count analysis failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Step 10: Create backup (if requested)
if ($Backup) {
    Write-Host "`n?? Creating database backup..." -ForegroundColor Yellow
    $backupPath = "OpCentrix/scheduler.db.backup.$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    try {
        Copy-Item $dbPath $backupPath
        $backupSize = (Get-Item $backupPath).Length / 1MB
        Write-Host "? Backup created: $backupPath ($($backupSize.ToString('F2')) MB)" -ForegroundColor Green
    } catch {
        Write-Host "? Backup failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Step 11: Summary and recommendations
Write-Host "`n?? DIAGNOSTIC SUMMARY" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan

if ($tableCount -eq 36) {
    Write-Host "? Database structure correct (36 tables)" -ForegroundColor Green
} else {
    Write-Host "? Database structure issue ($tableCount tables, expected 36)" -ForegroundColor Red
}

if ($missingTables.Count -eq 0) {
    Write-Host "? All critical tables present" -ForegroundColor Green
} else {
    Write-Host "? $($missingTables.Count) critical tables missing - run database update" -ForegroundColor Red
}

Write-Host "`n?? TROUBLESHOOTING COMMANDS:" -ForegroundColor Yellow
Write-Host "   Database Update:  cd OpCentrix && dotnet ef database update" -ForegroundColor White
Write-Host "   Build Check:      dotnet build OpCentrix/OpCentrix.csproj" -ForegroundColor White
Write-Host "   EF Validation:    dotnet ef dbcontext validate --project OpCentrix" -ForegroundColor White
Write-Host "   Test Suite:       dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj" -ForegroundColor White
Write-Host "   Full Repair:      PowerShell .\Database-Diagnostics.ps1 -Repair -Detailed -Backup" -ForegroundColor White

Write-Host "`n?? Documentation:" -ForegroundColor Cyan
Write-Host "   Schema Reference: OpCentrix/Database-Schema-Documentation.md" -ForegroundColor White
Write-Host "   Testing Guide:    OpCentrix-Testing-Framework.md" -ForegroundColor White

Write-Host "`n?? Diagnostic completed at $(Get-Date)" -ForegroundColor Cyan
Write-Host "   Database: $dbPath" -ForegroundColor Gray
Write-Host "   Tables: $tableCount (Expected: 36)" -ForegroundColor Gray
Write-Host "   Size: $($dbSize.ToString('F2')) MB" -ForegroundColor Gray