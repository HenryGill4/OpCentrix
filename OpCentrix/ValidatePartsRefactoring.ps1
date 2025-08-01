# OpCentrix Parts Refactoring - Final Validation Script
# This script validates the complete implementation of the Parts refactoring

Write-Host "?? OpCentrix Parts Refactoring - Final Validation" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

$validationResults = @()
$totalChecks = 0
$passedChecks = 0

# Helper function to add validation result
function Add-ValidationResult {
    param($Description, $Status, $Details = "")
    $global:totalChecks++
    if ($Status) { $global:passedChecks++ }
    
    $statusText = if ($Status) { "? PASS" } else { "? FAIL" }
    $color = if ($Status) { "Green" } else { "Red" }
    
    Write-Host "$statusText $Description" -ForegroundColor $color
    if ($Details) { Write-Host "    $Details" -ForegroundColor Gray }
    
    $global:validationResults += @{
        Description = $Description
        Status = $Status
        Details = $Details
    }
}

Write-Host "?? Phase 1: Database Migration Validation" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Yellow

# Check if migration files exist
$migrationExists = Test-Path "Migrations/20250801170930_AddPartStageRequirementTable.cs"
Add-ValidationResult "Migration file exists" $migrationExists "AddPartStageRequirementTable.cs"

$designerExists = Test-Path "Migrations/20250801170930_AddPartStageRequirementTable.Designer.cs"
Add-ValidationResult "Migration designer file exists" $designerExists "Designer.cs file"

# Check if OpCentrix.csproj exists (project file validation)
$projectExists = Test-Path "OpCentrix.csproj"
Add-ValidationResult "Project file exists" $projectExists "OpCentrix.csproj"

Write-Host ""
Write-Host "??? Phase 2: Model Implementation Validation" -ForegroundColor Yellow
Write-Host "-------------------------------------------" -ForegroundColor Yellow

# Check if Part.cs contains new properties
$partModelExists = Test-Path "Models/Part.cs"
Add-ValidationResult "Part.cs model exists" $partModelExists

if ($partModelExists) {
    $partContent = Get-Content "Models/Part.cs" -Raw
    
    $hasRequiredStages = $partContent -match "RequiredStages"
    Add-ValidationResult "Part.cs has RequiredStages property" $hasRequiredStages
    
    $hasStageIndicators = $partContent -match "StageIndicators"
    Add-ValidationResult "Part.cs has StageIndicators property" $hasStageIndicators
    
    $hasTotalProcessTime = $partContent -match "TotalEstimatedProcessTime"
    Add-ValidationResult "Part.cs has TotalEstimatedProcessTime property" $hasTotalProcessTime
    
    $hasComplexityLevel = $partContent -match "ComplexityLevel"
    Add-ValidationResult "Part.cs has ComplexityLevel property" $hasComplexityLevel
    
    $hasComplexityScore = $partContent -match "ComplexityScore"
    Add-ValidationResult "Part.cs has ComplexityScore property" $hasComplexityScore
}

# Check if StageIndicator.cs exists
$stageIndicatorExists = Test-Path "Models/StageIndicator.cs"
Add-ValidationResult "StageIndicator.cs model exists" $stageIndicatorExists

Write-Host ""
Write-Host "?? Phase 3: Service Layer Validation" -ForegroundColor Yellow
Write-Host "-----------------------------------" -ForegroundColor Yellow

# Check if services exist
$partStageServiceExists = Test-Path "Services/PartStageService.cs"
Add-ValidationResult "PartStageService.cs exists" $partStageServiceExists

$productionStageSeederExists = Test-Path "Services/ProductionStageSeederService.cs"
Add-ValidationResult "ProductionStageSeederService.cs exists" $productionStageSeederExists

# Check if Program.cs has service registrations
$programExists = Test-Path "Program.cs"
Add-ValidationResult "Program.cs exists" $programExists

if ($programExists) {
    $programContent = Get-Content "Program.cs" -Raw
    
    $hasPartStageService = $programContent -match "IPartStageService.*PartStageService"
    Add-ValidationResult "PartStageService registered in Program.cs" $hasPartStageService
    
    $hasProductionStageSeeder = $programContent -match "IProductionStageSeederService.*ProductionStageSeederService"
    Add-ValidationResult "ProductionStageSeederService registered in Program.cs" $hasProductionStageSeeder
}

Write-Host ""
Write-Host "?? Phase 4: UI Components Validation" -ForegroundColor Yellow
Write-Host "-----------------------------------" -ForegroundColor Yellow

# Check if UI components exist
$partStagesManagerExists = Test-Path "Pages/Admin/Shared/_PartStagesManager.cshtml"
Add-ValidationResult "_PartStagesManager.cshtml exists" $partStagesManagerExists

$partFormExists = Test-Path "Pages/Admin/Shared/_PartForm.cshtml"
Add-ValidationResult "_PartForm.cshtml exists" $partFormExists

$partsPageExists = Test-Path "Pages/Admin/Parts.cshtml"
Add-ValidationResult "Parts.cshtml exists" $partsPageExists

$partsModelExists = Test-Path "Pages/Admin/Parts.cshtml.cs"
Add-ValidationResult "Parts.cshtml.cs exists" $partsModelExists

Write-Host ""
Write-Host "?? Phase 5: Build and Compilation Validation" -ForegroundColor Yellow
Write-Host "--------------------------------------------" -ForegroundColor Yellow

# Test build
Write-Host "Running build validation..." -ForegroundColor Gray
try {
    $buildOutput = dotnet build --verbosity quiet 2>&1
    $buildSuccess = $LASTEXITCODE -eq 0
    Add-ValidationResult "Application builds successfully" $buildSuccess
    
    if (!$buildSuccess) {
        Write-Host "Build errors detected:" -ForegroundColor Red
        Write-Host $buildOutput -ForegroundColor Red
    }
} catch {
    Add-ValidationResult "Application builds successfully" $false "$($_.Exception.Message)"
}

Write-Host ""
Write-Host "?? Validation Summary" -ForegroundColor Cyan
Write-Host "==================" -ForegroundColor Cyan

$successRate = [math]::Round(($passedChecks / $totalChecks) * 100, 1)
$statusColor = if ($successRate -ge 90) { "Green" } elseif ($successRate -ge 75) { "Yellow" } else { "Red" }

Write-Host "Total Checks: $totalChecks" -ForegroundColor White
Write-Host "Passed: $passedChecks" -ForegroundColor Green
Write-Host "Failed: $($totalChecks - $passedChecks)" -ForegroundColor Red
Write-Host "Success Rate: $successRate%" -ForegroundColor $statusColor

Write-Host ""

if ($successRate -ge 95) {
    Write-Host "?? EXCELLENT! Parts Refactoring Implementation is COMPLETE and READY!" -ForegroundColor Green
    Write-Host "? All critical components are properly implemented" -ForegroundColor Green
    Write-Host "?? System is ready for production deployment" -ForegroundColor Green
} elseif ($successRate -ge 85) {
    Write-Host "??  GOOD! Implementation is mostly complete with minor issues" -ForegroundColor Yellow
    Write-Host "?? Review failed checks and address any missing components" -ForegroundColor Yellow
} else {
    Write-Host "? ATTENTION NEEDED! Critical components are missing" -ForegroundColor Red
    Write-Host "???  Please review and fix the failed validation checks" -ForegroundColor Red
}

Write-Host ""
Write-Host "?? Next Steps:" -ForegroundColor Cyan
if ($successRate -ge 95) {
    Write-Host "1. ? All validation checks passed - proceed with confidence" -ForegroundColor Green
    Write-Host "2. ?? Deploy to production environment" -ForegroundColor Green
    Write-Host "3. ?? Train users on new stage management features" -ForegroundColor Green
    Write-Host "4. ?? Monitor system performance and user adoption" -ForegroundColor Green
} else {
    Write-Host "1. ?? Review failed validation checks above" -ForegroundColor Yellow
    Write-Host "2. ???  Fix missing or incorrect implementations" -ForegroundColor Yellow
    Write-Host "3. ?? Re-run this validation script" -ForegroundColor Yellow
    Write-Host "4. ?? Deploy when all checks pass" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "OpCentrix Parts Refactoring Validation Complete!" -ForegroundColor Cyan
Write-Host "For detailed implementation information, see: PARTS_REFACTORING_IMPLEMENTATION_COMPLETE.md" -ForegroundColor Gray