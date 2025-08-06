# Create Sample Data for Testing Stages
# This script adds sample parts and stage assignments to test the stage functionality

Write-Host "?? Creating sample parts and stage assignments for testing..." -ForegroundColor Green

# Sample part data SQL
$samplePartsSQL = @"
INSERT INTO Parts (
    PartNumber, Name, Description, Industry, Application, Material, SlsMaterial, 
    EstimatedHours, MaterialCostPerKg, StandardLaborCostPerHour, MachineOperatingCostPerHour,
    PartCategory, PartClass, ProcessType, IsActive, RequiresSLSPrinting, RequiresInspection,
    RecommendedLaserPower, RecommendedScanSpeed, RecommendedLayerThickness, 
    RecommendedHatchSpacing, RecommendedBuildTemperature, RequiredArgonPurity, MaxOxygenContent,
    SetupTimeMinutes, PreheatingTimeMinutes, CoolingTimeMinutes, PostProcessingTimeMinutes,
    SetupCost, PostProcessingCost, QualityInspectionCost, ArgonCostPerHour,
    CreatedDate, CreatedBy, LastModifiedDate, LastModifiedBy,
    PowderSpecification, Dimensions, SurfaceFinishRequirement, QualityStandards,
    ToleranceRequirements, RequiredSkills, RequiredCertifications, RequiredTooling,
    ConsumableMaterials, SupportStrategy, ProcessParameters, QualityCheckpoints,
    BuildFileTemplate, CadFilePath, CadFileVersion, AvgDuration, PreferredMachines,
    AdminOverrideBy, ManufacturingStage, StageDetails, StageOrder,
    BTComponentType, BTFirearmCategory, SerialNumberFormat, BatchControlMethod,
    MaxBatchSize, ParentComponents, ChildComponents, WorkflowTemplate, ApprovalWorkflow,
    RequiredMachineType
) VALUES 
('P-001', 'Test Part 1', 'A simple test part for SLS printing', 'Aerospace', 'Structural Component', 
    'Ti-6Al-4V Grade 5', 'Ti-6Al-4V Grade 5', 8.0, 450.00, 85.00, 125.00, 
    'Production', 'B', 'SLS Metal', 1, 1, 1,
    200, 1200, 30, 120, 180, 99.9, 50,
    45, 60, 240, 45, 150.00, 75.00, 50.00, 15.00,
    datetime('now'), 'System', datetime('now'), 'System',
    '15-45 micron particle size', '50x30x20mm', 'As-built', 'ASTM F3001',
    '±0.1mm typical', 'SLS Operation', 'SLS Certification', 'Build Platform',
    'Argon Gas', 'Minimal supports', '{}', '{}',
    'default-template.slm', '', '1.0', '8h 0m', 'TI1,TI2',
    '', 'Design', '{}', 1,
    'General', 'Component', 'BT-{YYYY}-{####}', 'Standard',
    1, '[]', '[]', 'BT_Standard_Workflow', 'Standard',
    'TruPrint 3000'),
    
('P-002', 'Test Part 2', 'A complex part requiring multiple stages', 'Medical', 'Implant Component', 
    'Ti-6Al-4V Grade 5', 'Ti-6Al-4V Grade 5', 12.0, 450.00, 85.00, 125.00, 
    'Production', 'A', 'SLS Metal', 1, 1, 1,
    200, 1200, 30, 120, 180, 99.9, 50,
    45, 60, 240, 45, 150.00, 75.00, 50.00, 15.00,
    datetime('now'), 'System', datetime('now'), 'System',
    '15-45 micron particle size', '80x60x40mm', 'Ra 3.2', 'ASTM F3001, ISO 13485',
    '±0.05mm critical', 'SLS Operation, CNC Machining', 'SLS + Machining Certification', 'Build Platform, CNC Tools',
    'Argon Gas, Cutting Fluid', 'Support structures required', '{}', '{}',
    'complex-template.slm', '', '1.0', '12h 0m', 'TI1,TI2,CNC-01',
    '', 'Design', '{}', 1,
    'Medical', 'Component', 'BT-{YYYY}-{####}', 'Standard',
    1, '[]', '[]', 'BT_Standard_Workflow', 'Standard',
    'TruPrint 3000'),
    
('P-003', 'Test Part 3', 'Simple bracket with finishing requirements', 'Automotive', 'Bracket', 
    'AlSi10Mg', 'AlSi10Mg', 6.0, 85.00, 85.00, 125.00, 
    'Production', 'C', 'SLS Metal', 1, 1, 1,
    180, 1100, 30, 120, 165, 99.9, 50,
    30, 45, 180, 30, 100.00, 50.00, 30.00, 15.00,
    datetime('now'), 'System', datetime('now'), 'System',
    '15-45 micron particle size', '40x25x15mm', 'Anodized', 'ASTM F3001',
    '±0.2mm typical', 'SLS Operation', 'SLS Certification', 'Build Platform',
    'Argon Gas', 'Minimal supports', '{}', '{}',
    'bracket-template.slm', '', '1.0', '6h 0m', 'TI1,TI2',
    '', 'Design', '{}', 1,
    'General', 'Component', 'BT-{YYYY}-{####}', 'Standard',
    1, '[]', '[]', 'BT_Standard_Workflow', 'Standard',
    'TruPrint 3000');
"@

# Sample stage assignments SQL
$stageAssignmentsSQL = @"
INSERT INTO PartStageRequirements (
    PartId, ProductionStageId, ExecutionOrder, IsRequired, IsActive,
    EstimatedHours, SetupTimeMinutes, EstimatedCost, MaterialCost,
    RequiresSpecificMachine, AllowParallelExecution, IsBlocking,
    CustomFieldValues, StageParameters, RequiredMaterials, RequiredTooling,
    QualityRequirements, SpecialInstructions, RequirementNotes,
    CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy
) VALUES 
-- Part 1: Simple SLS + Inspection
(1, 1, 1, 1, 1, 8.0, 45, 680.00, 25.00, 0, 0, 1, '{}', '{}', 'Ti-6Al-4V Powder', 'Build Platform', 'Standard SLS quality', 'Standard SLS parameters', 'Primary manufacturing stage', datetime('now'), datetime('now'), 'System', 'System'),
(1, 6, 2, 1, 1, 1.0, 10, 80.00, 0.00, 0, 0, 1, '{}', '{}', '', 'Measuring tools', 'Dimensional inspection', 'Check all critical dimensions', 'Final quality check', datetime('now'), datetime('now'), 'System', 'System'),

-- Part 2: Complex multi-stage (SLS + CNC + EDM + Assembly + Finishing + Inspection)
(2, 1, 1, 1, 1, 12.0, 45, 1020.00, 37.50, 0, 0, 1, '{}', '{}', 'Ti-6Al-4V Powder', 'Build Platform', 'High precision SLS', 'Optimized parameters for machining stock', 'Primary manufacturing stage', datetime('now'), datetime('now'), 'System', 'System'),
(2, 2, 2, 1, 1, 4.0, 30, 380.00, 5.00, 1, 0, 1, '{}', '{}', 'Cutting fluid', 'CNC tools, fixtures', 'Precision machining', 'Machine critical features to ±0.05mm', 'Secondary machining operations', datetime('now'), datetime('now'), 'System', 'System'),
(2, 3, 3, 1, 1, 2.0, 60, 220.00, 10.00, 1, 0, 1, '{}', '{}', 'EDM wire/electrodes', 'EDM electrodes', 'Complex geometry EDM', 'EDM internal channels and complex features', 'Specialized EDM work', datetime('now'), datetime('now'), 'System', 'System'),
(2, 4, 4, 1, 1, 1.5, 15, 112.50, 2.00, 0, 1, 0, '{}', '{}', 'Assembly hardware', 'Assembly tools', 'Component assembly', 'Assemble sub-components if required', 'Assembly operations', datetime('now'), datetime('now'), 'System', 'System'),
(2, 5, 5, 1, 1, 2.0, 20, 140.00, 15.00, 0, 1, 0, '{}', '{}', 'Finishing compounds', 'Finishing tools', 'Surface finishing', 'Achieve required surface finish', 'Final finishing', datetime('now'), datetime('now'), 'System', 'System'),
(2, 6, 6, 1, 1, 1.0, 10, 80.00, 0.00, 0, 0, 1, '{}', '{}', '', 'Measuring tools', 'Complete inspection', 'Full dimensional and surface inspection', 'Final quality verification', datetime('now'), datetime('now'), 'System', 'System'),

-- Part 3: Simple with finishing (SLS + Finishing + Inspection)
(3, 1, 1, 1, 1, 6.0, 30, 510.00, 15.00, 0, 0, 1, '{}', '{}', 'AlSi10Mg Powder', 'Build Platform', 'Standard AlSi10Mg quality', 'Standard aluminum parameters', 'Primary manufacturing stage', datetime('now'), datetime('now'), 'System', 'System'),
(3, 5, 2, 1, 1, 1.5, 20, 105.00, 8.00, 0, 1, 0, '{}', '{}', 'Anodizing chemicals', 'Finishing equipment', 'Anodizing finish', 'Apply anodized coating for corrosion resistance', 'Cosmetic finishing', datetime('now'), datetime('now'), 'System', 'System'),
(3, 6, 3, 1, 1, 0.5, 10, 40.00, 0.00, 0, 0, 1, '{}', '{}', '', 'Measuring tools', 'Final inspection', 'Verify finish quality and dimensions', 'Quality verification', datetime('now'), datetime('now'), 'System', 'System');
"@

try {
    Write-Host "?? Adding sample parts..." -ForegroundColor Yellow
    
    # Execute parts SQL
    sqlite3 "scheduler.db" $samplePartsSQL
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Sample parts added successfully" -ForegroundColor Green
    } else {
        Write-Host "? Failed to add sample parts" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "?? Adding sample stage assignments..." -ForegroundColor Yellow
    
    # Execute stage assignments SQL
    sqlite3 "scheduler.db" $stageAssignmentsSQL
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Sample stage assignments added successfully" -ForegroundColor Green
    } else {
        Write-Host "? Failed to add sample stage assignments" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
    Write-Host "?? Sample data created successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Summary:" -ForegroundColor Cyan
    
    # Get counts
    $partCount = sqlite3 "scheduler.db" "SELECT COUNT(*) FROM Parts;"
    $stageCount = sqlite3 "scheduler.db" "SELECT COUNT(*) FROM PartStageRequirements WHERE IsActive = 1;"
    $prodStageCount = sqlite3 "scheduler.db" "SELECT COUNT(*) FROM ProductionStages WHERE IsActive = 1;"
    
    Write-Host "   • Parts: $partCount" -ForegroundColor White
    Write-Host "   • Active Stage Assignments: $stageCount" -ForegroundColor White  
    Write-Host "   • Available Production Stages: $prodStageCount" -ForegroundColor White
    Write-Host ""
    Write-Host "?? You can now test the stage functionality in the Parts page!" -ForegroundColor Green
    Write-Host "   1. Navigate to /Admin/Parts" -ForegroundColor White
    Write-Host "   2. You should see stage indicators in the 'Manufacturing Stages' column" -ForegroundColor White
    Write-Host "   3. Try editing a part to see the stage management interface" -ForegroundColor White
    
} catch {
    Write-Host "? Error creating sample data: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}