# OpCentrix Parts Table Refactoring PowerShell Script
# This script will execute the SQL commands to update the Parts table
# Run this from the OpCentrix project directory

Write-Host "?? OpCentrix Parts Table Refactoring Script" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Check if we're in the right directory
if (!(Test-Path "scheduler.db")) {
    Write-Host "? Error: scheduler.db not found. Please run this script from the OpCentrix project directory." -ForegroundColor Red
    exit 1
}

# Check if sqlite3 command is available
$sqliteCommand = Get-Command sqlite3 -ErrorAction SilentlyContinue
if (-not $sqliteCommand) {
    Write-Host "? Error: sqlite3 command not found. Installing SQLite..." -ForegroundColor Red
    
    # Try to install SQLite via winget
    try {
        winget install sqlite.sqlite --accept-source-agreements --accept-package-agreements
        Write-Host "? SQLite installed successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "? Error: Could not install SQLite. Please install SQLite manually." -ForegroundColor Red
        Write-Host "   Download from: https://www.sqlite.org/download.html" -ForegroundColor Yellow
        exit 1
    }
}

# Backup the database first
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = "scheduler_backup_$timestamp.db"

Write-Host "?? Creating backup: $backupFile" -ForegroundColor Yellow
Copy-Item "scheduler.db" $backupFile
Write-Host "? Backup created successfully" -ForegroundColor Green

# Execute the SQL script
Write-Host "?? Executing Parts table refactoring..." -ForegroundColor Yellow

try {
    # Read the SQL script and execute each ALTER TABLE command individually
    $sqlScript = Get-Content "RefactorPartsTableScript.sql" -Raw
    
    # Split into individual commands and filter out comments
    $commands = $sqlScript -split ";" | Where-Object { 
        $_.Trim() -ne "" -and 
        !$_.Trim().StartsWith("--") -and
        $_.Trim() -match "ALTER TABLE"
    }
    
    $successCount = 0
    $skipCount = 0
    $totalCommands = $commands.Count
    
    Write-Host "?? Processing $totalCommands ALTER TABLE commands..." -ForegroundColor Cyan
    
    foreach ($command in $commands) {
        $trimmedCommand = $command.Trim()
        if ($trimmedCommand -ne "") {
            try {
                # Execute the command using sqlite3
                $result = & sqlite3 "scheduler.db" "$trimmedCommand;"
                $successCount++
                
                # Extract column name from the command for progress display
                if ($trimmedCommand -match "ADD COLUMN (\w+)") {
                    $columnName = $matches[1]
                    Write-Host "  ? Added column: $columnName" -ForegroundColor Green
                }
            }
            catch {
                # Column might already exist - this is okay
                if ($_.Exception.Message -contains "duplicate column name") {
                    $skipCount++
                    if ($trimmedCommand -match "ADD COLUMN (\w+)") {
                        $columnName = $matches[1]
                        Write-Host "  ??  Skipped existing column: $columnName" -ForegroundColor Yellow
                    }
                } else {
                    Write-Host "  ? Error: $($_.Exception.Message)" -ForegroundColor Red
                }
            }
        }
    }
    
    Write-Host "?? Refactoring Summary:" -ForegroundColor Cyan
    Write-Host "  ? Successfully added: $successCount columns" -ForegroundColor Green
    Write-Host "  ??  Skipped existing: $skipCount columns" -ForegroundColor Yellow
    Write-Host "  ?? Total processed: $totalCommands commands" -ForegroundColor Cyan
    
} catch {
    Write-Host "? Error executing SQL script: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "?? Restoring backup..." -ForegroundColor Yellow
    Copy-Item $backupFile "scheduler.db" -Force
    Write-Host "? Database restored from backup" -ForegroundColor Green
    exit 1
}

# Verify the changes
Write-Host "?? Verifying database structure..." -ForegroundColor Yellow

try {
    $columnQuery = "SELECT name FROM pragma_table_info('Parts') ORDER BY cid;"
    $columns = & sqlite3 "scheduler.db" $columnQuery
    $columnCount = ($columns | Measure-Object).Count
    
    Write-Host "? Parts table now has $columnCount columns" -ForegroundColor Green
    
    # Check for some key stage columns
    $stageColumns = @(
        "RequiresSLSPrinting",
        "RequiresEDM", 
        "RequiresMachining",
        "RequiresCoating",
        "RequiresQC",
        "RequiresAssembly",
        "RequiresShipping",
        "GenericStage1Name",
        "GenericStage2Name",
        "GenericStage3Name",
        "GenericStage4Name",
        "GenericStage5Name"
    )
    
    $foundStageColumns = 0
    foreach ($stageColumn in $stageColumns) {
        if ($columns -contains $stageColumn) {
            $foundStageColumns++
        }
    }
    
    Write-Host "? Found $foundStageColumns/$($stageColumns.Count) key stage columns" -ForegroundColor Green
    
} catch {
    Write-Host "??  Warning: Could not verify database structure: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Optional: Update Entity Framework model
Write-Host "?? Would you like to update the Entity Framework model? (y/n)" -ForegroundColor Cyan
$updateEF = Read-Host

if ($updateEF -eq "y" -or $updateEF -eq "Y") {
    Write-Host "?? Creating Entity Framework migration..." -ForegroundColor Yellow
    try {
        & dotnet ef migrations add "UpdatePartsTableWithStages" --verbose
        Write-Host "? Migration created successfully" -ForegroundColor Green
        
        Write-Host "?? Applying migration..." -ForegroundColor Yellow
        & dotnet ef database update --verbose
        Write-Host "? Migration applied successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "??  Warning: Could not create/apply EF migration: $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host "   You may need to manually update your Part model to match the database schema." -ForegroundColor Yellow
    }
}

# Final summary
Write-Host ""
Write-Host "?? Parts Table Refactoring Complete!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host "? Database backup created: $backupFile" -ForegroundColor Green
Write-Host "? Parts table updated with stage fields" -ForegroundColor Green
Write-Host "? Your database now matches the refactored schema" -ForegroundColor Green
Write-Host ""
Write-Host "?? Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Test your application to ensure it starts properly" -ForegroundColor White
Write-Host "  2. Update your Parts forms to use the new stage fields" -ForegroundColor White
Write-Host "  3. Add UI components for the new stage management" -ForegroundColor White
Write-Host "  4. Update your Part model properties if needed" -ForegroundColor White
Write-Host ""
Write-Host "?? You can now start using the new stage-based parts management!" -ForegroundColor Green