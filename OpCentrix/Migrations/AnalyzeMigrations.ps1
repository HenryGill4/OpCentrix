# OpCentrix Migrations Cleanup Plan
# This script provides a comprehensive analysis and cleanup of Entity Framework migrations

Write-Host "?? OpCentrix Migrations Cleanup Utility" -ForegroundColor Cyan
Write-Host "=" * 60

$currentDir = Get-Location
Write-Host "?? Working Directory: $currentDir" -ForegroundColor Yellow

# First, let's analyze what we have
$allFiles = Get-ChildItem -Filter "*.cs" | Sort-Object Name
$migrationFiles = $allFiles | Where-Object { $_.Name -match "^\d{8,}" }
$nonMigrationFiles = $allFiles | Where-Object { $_.Name -notmatch "^\d{8,}" }

Write-Host "`n?? FILE INVENTORY:" -ForegroundColor Cyan
Write-Host "  Migration files: $($migrationFiles.Count)" -ForegroundColor White
Write-Host "  Other files: $($nonMigrationFiles.Count)" -ForegroundColor White
Write-Host "  Total files: $($allFiles.Count)" -ForegroundColor White

# Analyze problems
$emptyFiles = @()
$emptyMigrations = @()
$validMigrations = @()
$problemFiles = @()

Write-Host "`n?? ANALYZING MIGRATIONS..." -ForegroundColor Yellow

foreach ($file in $migrationFiles) {
    $analysis = [PSCustomObject]@{
        FileName = $file.Name
        FullPath = $file.FullName
        Size = $file.Length
        Timestamp = $file.Name.Substring(0, 17)
        MigrationName = ($file.Name -replace "^\d+_", "" -replace "\.cs$", "")
        IsEmpty = $false
        HasEmptyUp = $false
        HasDesignerFile = $false
        Issues = @()
    }
    
    # Check if file is completely empty
    if ($file.Length -eq 0) {
        $emptyFiles += $analysis
        $analysis.IsEmpty = $true
        $analysis.Issues += "Empty file (0 bytes)"
    } else {
        # Check file content for empty Up() method
        $content = Get-Content -Path $file.FullName -Raw
        if ($content -match "protected override void Up\(MigrationBuilder migrationBuilder\)\s*\{\s*\}") {
            $analysis.HasEmptyUp = $true
            $analysis.Issues += "Empty Up() method"
            $emptyMigrations += $analysis
        } else {
            $validMigrations += $analysis
        }
    }
    
    # Check for corresponding Designer file
    $designerFile = $file.FullName -replace "\.cs$", ".Designer.cs"
    if (Test-Path $designerFile) {
        $analysis.HasDesignerFile = $true
    } else {
        $analysis.Issues += "Missing Designer file"
    }
    
    if ($analysis.Issues.Count -gt 0) {
        $problemFiles += $analysis
    }
}

# Display findings
Write-Host "`n?? ANALYSIS RESULTS:" -ForegroundColor Cyan
Write-Host "  ? Valid migrations: $($validMigrations.Count)" -ForegroundColor Green
Write-Host "  ??? Empty files: $($emptyFiles.Count)" -ForegroundColor Red  
Write-Host "  ? Empty migrations: $($emptyMigrations.Count)" -ForegroundColor Red
Write-Host "  ?? Problem files: $($problemFiles.Count)" -ForegroundColor Yellow

# Show problematic files
if ($problemFiles.Count -gt 0) {
    Write-Host "`n?? PROBLEM FILES:" -ForegroundColor Yellow
    foreach ($problem in $problemFiles) {
        Write-Host "  ?? $($problem.FileName)" -ForegroundColor White
        foreach ($issue in $problem.Issues) {
            Write-Host "     • $issue" -ForegroundColor Gray
        }
    }
}

# Show valid migrations in chronological order
Write-Host "`n? VALID MIGRATIONS (chronological):" -ForegroundColor Green
foreach ($valid in $validMigrations | Sort-Object Timestamp) {
    Write-Host "  ?? $($valid.Timestamp) - $($valid.MigrationName)" -ForegroundColor Gray
}

# Identify specific cleanup targets
$filesToDelete = @()
$filesToDelete += $emptyFiles | Select-Object -ExpandProperty FileName
$filesToDelete += $emptyMigrations | Select-Object -ExpandProperty FileName

# Add corresponding Designer files
foreach ($file in ($emptyFiles + $emptyMigrations)) {
    $designerFile = $file.FileName -replace "\.cs$", ".Designer.cs"
    if (Test-Path $designerFile) {
        $filesToDelete += $designerFile
    }
}

Write-Host "`n?? CLEANUP PLAN:" -ForegroundColor Cyan
Write-Host "  Files to delete: $($filesToDelete.Count)" -ForegroundColor Red

if ($filesToDelete.Count -gt 0) {
    Write-Host "`n??? FILES TO DELETE:" -ForegroundColor Red
    foreach ($file in $filesToDelete | Sort-Object) {
        Write-Host "  ? $file" -ForegroundColor Gray
    }
}

# Create backup commands
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = "MigrationsBackup_$timestamp"

Write-Host "`n?? BACKUP COMMANDS:" -ForegroundColor Cyan
Write-Host "  # Create backup directory" -ForegroundColor Gray
Write-Host "  New-Item -ItemType Directory -Path `"../$backupDir`" -Force" -ForegroundColor White
Write-Host "  # Copy all migrations to backup" -ForegroundColor Gray  
Write-Host "  Copy-Item -Path `"*`" -Destination `"../$backupDir`" -Recurse" -ForegroundColor White

Write-Host "`n?? CLEANUP COMMANDS:" -ForegroundColor Cyan
if ($filesToDelete.Count -gt 0) {
    foreach ($file in $filesToDelete | Sort-Object) {
        Write-Host "  Remove-Item `"$file`" -Force" -ForegroundColor White
    }
} else {
    Write-Host "  # No files need to be deleted" -ForegroundColor Green
}

# Interactive cleanup
Write-Host "`n?? INTERACTIVE CLEANUP:" -ForegroundColor Cyan
$choice = Read-Host "Proceed with automated cleanup? [y/N]"

if ($choice -eq 'y' -or $choice -eq 'Y') {
    # Create backup
    Write-Host "`n?? Creating backup..." -ForegroundColor Yellow
    $backupPath = Join-Path (Split-Path $currentDir -Parent) $backupDir
    New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
    Copy-Item -Path "*" -Destination $backupPath -Recurse
    Write-Host "? Backup created: $backupPath" -ForegroundColor Green
    
    # Delete problematic files
    if ($filesToDelete.Count -gt 0) {
        Write-Host "`n??? Deleting problematic files..." -ForegroundColor Yellow
        foreach ($file in $filesToDelete) {
            if (Test-Path $file) {
                Remove-Item $file -Force
                Write-Host "  ? Deleted: $file" -ForegroundColor Gray
            }
        }
    }
    
    # Final verification
    $remaining = Get-ChildItem -Filter "*.cs" | Where-Object { $_.Name -match "^\d{8,}" }
    Write-Host "`n?? CLEANUP COMPLETE!" -ForegroundColor Green
    Write-Host "  Remaining migrations: $($remaining.Count)" -ForegroundColor White
    
    Write-Host "`n?? NEXT STEPS:" -ForegroundColor Cyan
    Write-Host "  1. Review remaining migrations" -ForegroundColor White
    Write-Host "  2. Run: dotnet ef migrations list" -ForegroundColor White
    Write-Host "  3. Run: dotnet ef database update" -ForegroundColor White
    Write-Host "  4. Verify application still works" -ForegroundColor White

} else {
    Write-Host "?? Cleanup cancelled. Use the commands above to clean manually." -ForegroundColor Yellow
}

Write-Host "`n? Analysis complete!" -ForegroundColor Green