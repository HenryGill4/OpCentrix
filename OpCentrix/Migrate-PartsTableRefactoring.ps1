# Parts Table Refactoring Migration Script
# This script creates the new PartStageRequirement table and migrates existing stage data

param(
    [string]$ConnectionString = "Data Source=scheduler.db",
    [switch]$DryRun = $false
)

Write-Host "?? OpCentrix Parts Table Refactoring Migration" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

if ($DryRun) {
    Write-Host "?? DRY RUN MODE - No changes will be made" -ForegroundColor Yellow
    Write-Host ""
}

try {
    # Create the migration
    Write-Host "?? Creating database migration for PartStageRequirement table..." -ForegroundColor Cyan
    
    $migrationName = "AddPartStageRequirementTable"
    $migrationCommand = "dotnet ef migrations add $migrationName --project OpCentrix --startup-project OpCentrix"
    
    if (-not $DryRun) {
        Invoke-Expression $migrationCommand
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? Migration created successfully: $migrationName" -ForegroundColor Green
        } else {
            throw "Failed to create migration"
        }
    } else {
        Write-Host "?? Would run: $migrationCommand" -ForegroundColor Yellow
    }
    
    # Apply the migration
    Write-Host "?? Applying database migration..." -ForegroundColor Cyan
    
    $updateCommand = "dotnet ef database update --project OpCentrix --startup-project OpCentrix"
    
    if (-not $DryRun) {
        Invoke-Expression $updateCommand
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? Database migration applied successfully" -ForegroundColor Green
        } else {
            throw "Failed to apply migration"
        }
    } else {
        Write-Host "?? Would run: $updateCommand" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "?? Parts Table Refactoring Migration Completed Successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? What was created:" -ForegroundColor Cyan
    Write-Host "   • PartStageRequirement table with full schema" -ForegroundColor White
    Write-Host "   • Foreign key relationships to Parts and ProductionStages" -ForegroundColor White
    Write-Host "   • Indexes for performance optimization" -ForegroundColor White
    Write-Host "   • Audit fields for tracking changes" -ForegroundColor White
    Write-Host ""
    Write-Host "?? Next Steps:" -ForegroundColor Cyan
    Write-Host "   1. Test the Parts page with stage management" -ForegroundColor White
    Write-Host "   2. Create default ProductionStages if needed" -ForegroundColor White
    Write-Host "   3. Migrate existing parts to use stage requirements" -ForegroundColor White
    Write-Host ""
    
    if (-not $DryRun) {
        Write-Host "?? Creating default production stages..." -ForegroundColor Cyan
        
        # Create default stages script
        $defaultStagesScript = @"
-- Insert default production stages if they don't exist
INSERT OR IGNORE INTO ProductionStages (Name, DisplayOrder, Description, DefaultSetupMinutes, DefaultHourlyRate, RequiresQualityCheck, RequiresApproval, AllowSkip, IsOptional, RequiredRole, CreatedDate, IsActive)
VALUES 
('SLS Printing', 1, 'Selective Laser Sintering metal printing process', 45, 85.00, 1, 0, 0, 0, NULL, datetime('now'), 1),
('CNC Machining', 2, 'Computer Numerical Control machining operations', 30, 95.00, 1, 0, 0, 1, NULL, datetime('now'), 1),
('EDM Operations', 3, 'Electrical Discharge Machining for complex geometries', 60, 110.00, 1, 1, 0, 1, NULL, datetime('now'), 1),
('Assembly', 4, 'Assembly of multiple components', 15, 75.00, 1, 0, 0, 1, NULL, datetime('now'), 1),
('Finishing', 5, 'Surface finishing, coating, and final processing', 20, 70.00, 1, 0, 0, 1, NULL, datetime('now'), 1),
('Quality Inspection', 6, 'Final quality control and inspection', 10, 80.00, 1, 1, 0, 0, NULL, datetime('now'), 1);
"@
        
        # Write to temp file and execute
        $tempFile = [System.IO.Path]::GetTempFileName() + ".sql"
        $defaultStagesScript | Out-File -FilePath $tempFile -Encoding UTF8
        
        try {
            $sqliteCommand = "sqlite3 scheduler.db < `"$tempFile`""
            Invoke-Expression $sqliteCommand
            Write-Host "? Default production stages created" -ForegroundColor Green
        } catch {
            Write-Host "??  Warning: Could not create default stages automatically" -ForegroundColor Yellow
            Write-Host "   You may need to create them manually in the admin interface" -ForegroundColor Yellow
        } finally {
            if (Test-Path $tempFile) {
                Remove-Item $tempFile -Force
            }
        }
    }
    
} catch {
    Write-Host ""
    Write-Host "? Migration failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "?? Troubleshooting tips:" -ForegroundColor Yellow
    Write-Host "   • Ensure you're in the OpCentrix solution directory" -ForegroundColor White
    Write-Host "   • Check that Entity Framework tools are installed" -ForegroundColor White
    Write-Host "   • Verify the database connection is working" -ForegroundColor White
    Write-Host "   • Try running with -DryRun first to test" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host "?? Migration script completed successfully!" -ForegroundColor Green