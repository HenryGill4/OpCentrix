-- Complete fix for stage management testing
-- This script creates a minimal test part and assigns it to production stages

-- First, create a simple test part with only the absolutely required fields
INSERT OR IGNORE INTO Parts (
    Id, PartNumber, Name, Description, Industry, Application, Material, SlsMaterial, 
    EstimatedHours, MaterialCostPerKg, StandardLaborCostPerHour, MachineOperatingCostPerHour,
    PartCategory, PartClass, ProcessType, IsActive, RequiresSLSPrinting, RequiresInspection,
    CreatedDate, CreatedBy, LastModifiedDate, LastModifiedBy,
    -- All the required numeric fields with defaults
    PowderRequirementKg, WeightGrams, VolumeMm3, HeightMm, LengthMm, WidthMm,
    MaxSurfaceRoughnessRa, SetupTimeMinutes, PowderChangeoverTimeMinutes,
    PreheatingTimeMinutes, CoolingTimeMinutes, PostProcessingTimeMinutes,
    SupportRemovalTimeMinutes, AverageEfficiencyPercent, AverageQualityScore,
    AveragePowderUtilization, AvgDurationDays, MaxBatchSize, StageOrder,
    AverageActualHours, RequiredMachineType, RecommendedLaserPower, RecommendedScanSpeed,
    RecommendedLayerThickness, RecommendedHatchSpacing, RecommendedBuildTemperature,
    RequiredArgonPurity, MaxOxygenContent, SetupCost, PostProcessingCost,
    QualityInspectionCost, ArgonCostPerHour, StandardSellingPrice, TotalJobsCompleted,
    TotalUnitsProduced, AverageCostPerUnit,
    -- All the text fields with defaults
    PowderSpecification, Dimensions, SurfaceFinishRequirement, QualityStandards,
    ToleranceRequirements, RequiredSkills, RequiredCertifications, RequiredTooling,
    ConsumableMaterials, SupportStrategy, ProcessParameters, QualityCheckpoints,
    BuildFileTemplate, CadFilePath, CadFileVersion, AvgDuration, PreferredMachines,
    AdminOverrideBy, ManufacturingStage, StageDetails, SerialNumberFormat,
    BatchControlMethod, ParentComponents, ChildComponents, WorkflowTemplate,
    ApprovalWorkflow, CustomerPartNumber, AdminOverrideReason
) VALUES (
    1, 'TEST-001', 'Test Part 1', 'Simple SLS test part for stage management testing', 
    'Aerospace', 'Component', 'Ti-6Al-4V Grade 5', 'Ti-6Al-4V Grade 5',
    8.0, 450.00, 85.00, 125.00, 'Production', 'B', 'SLS Metal', 1, 1, 1,
    datetime('now'), 'System', datetime('now'), 'System',
    -- Numeric defaults
    0.5, 100, 30000, 20, 50, 30, 25, 45, 30, 60, 240, 45, 0, 100, 100, 85, 1, 1, 1, 8.0,
    'TruPrint 3000', 200, 1200, 30, 120, 180, 99.9, 50, 150.00, 75.00, 50.00, 15.00, 1200.00, 0, 0, 150.00,
    -- Text defaults
    '15-45 micron particle size', '50x30x20mm', 'As-built', 'ASTM F3001',
    '±0.1mm typical', 'SLS Operation', 'SLS Certification', 'Build Platform',
    'Argon Gas', 'Minimal supports', '{}', '{}', 'default-template.slm', '', '1.0', 
    '8h 0m', 'TI1,TI2', '', 'Design', '{}', 'BT-{YYYY}-{####}', 'Standard', 
    '[]', '[]', 'BT_Standard_Workflow', 'Standard', '', ''
);

-- Now create stage requirements for this part
-- Assign SLS Printing stage (ID=1)
INSERT OR IGNORE INTO PartStageRequirements (
    PartId, ProductionStageId, ExecutionOrder, IsRequired, IsActive,
    EstimatedHours, SetupTimeMinutes, EstimatedCost, MaterialCost,
    RequiresSpecificMachine, AllowParallelExecution, IsBlocking,
    CustomFieldValues, StageParameters, RequiredMaterials, RequiredTooling,
    QualityRequirements, SpecialInstructions, RequirementNotes,
    CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy
) VALUES (
    1, 1, 1, 1, 1, 8.0, 45, 680.00, 25.00, 0, 0, 1, '{}', '{}', 
    'Ti-6Al-4V Powder', 'Build Platform', 'Standard SLS quality', 
    'Standard SLS parameters', 'Primary manufacturing stage', 
    datetime('now'), datetime('now'), 'System', 'System'
);

-- Assign Quality Inspection stage (ID=7 if it exists, or use available stage)
INSERT OR IGNORE INTO PartStageRequirements (
    PartId, ProductionStageId, ExecutionOrder, IsRequired, IsActive,
    EstimatedHours, SetupTimeMinutes, EstimatedCost, MaterialCost,
    RequiresSpecificMachine, AllowParallelExecution, IsBlocking,
    CustomFieldValues, StageParameters, RequiredMaterials, RequiredTooling,
    QualityRequirements, SpecialInstructions, RequirementNotes,
    CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy
) VALUES (
    1, 7, 2, 1, 1, 1.0, 10, 80.00, 0.00, 0, 0, 1, '{}', '{}', 
    '', 'Measuring tools', 'Dimensional inspection', 
    'Check all critical dimensions', 'Final quality check', 
    datetime('now'), datetime('now'), 'System', 'System'
);

-- Create a second test part for multi-stage demonstration
INSERT OR IGNORE INTO Parts (
    Id, PartNumber, Name, Description, Industry, Application, Material, SlsMaterial, 
    EstimatedHours, MaterialCostPerKg, StandardLaborCostPerHour, MachineOperatingCostPerHour,
    PartCategory, PartClass, ProcessType, IsActive, RequiresSLSPrinting, RequiresInspection,
    CreatedDate, CreatedBy, LastModifiedDate, LastModifiedBy,
    PowderRequirementKg, WeightGrams, VolumeMm3, HeightMm, LengthMm, WidthMm,
    MaxSurfaceRoughnessRa, SetupTimeMinutes, PowderChangeoverTimeMinutes,
    PreheatingTimeMinutes, CoolingTimeMinutes, PostProcessingTimeMinutes,
    SupportRemovalTimeMinutes, AverageEfficiencyPercent, AverageQualityScore,
    AveragePowderUtilization, AvgDurationDays, MaxBatchSize, StageOrder,
    AverageActualHours, RequiredMachineType, RecommendedLaserPower, RecommendedScanSpeed,
    RecommendedLayerThickness, RecommendedHatchSpacing, RecommendedBuildTemperature,
    RequiredArgonPurity, MaxOxygenContent, SetupCost, PostProcessingCost,
    QualityInspectionCost, ArgonCostPerHour, StandardSellingPrice, TotalJobsCompleted,
    TotalUnitsProduced, AverageCostPerUnit, RequiresCNCMachining,
    PowderSpecification, Dimensions, SurfaceFinishRequirement, QualityStandards,
    ToleranceRequirements, RequiredSkills, RequiredCertifications, RequiredTooling,
    ConsumableMaterials, SupportStrategy, ProcessParameters, QualityCheckpoints,
    BuildFileTemplate, CadFilePath, CadFileVersion, AvgDuration, PreferredMachines,
    AdminOverrideBy, ManufacturingStage, StageDetails, SerialNumberFormat,
    BatchControlMethod, ParentComponents, ChildComponents, WorkflowTemplate,
    ApprovalWorkflow, CustomerPartNumber, AdminOverrideReason
) VALUES (
    2, 'TEST-002', 'Complex Test Part', 'Multi-stage part requiring SLS + CNC + Assembly', 
    'Medical', 'Implant', 'Ti-6Al-4V Grade 5', 'Ti-6Al-4V Grade 5',
    15.0, 450.00, 85.00, 125.00, 'Production', 'A', 'SLS Metal', 1, 1, 1,
    datetime('now'), 'System', datetime('now'), 'System',
    0.8, 150, 45000, 30, 70, 40, 25, 45, 30, 60, 240, 45, 0, 100, 100, 85, 1, 1, 1, 15.0,
    'TruPrint 3000', 200, 1200, 30, 120, 180, 99.9, 50, 150.00, 75.00, 50.00, 15.00, 1800.00, 0, 0, 200.00, 1,
    '15-45 micron particle size', '80x60x40mm', 'Ra 3.2', 'ASTM F3001, ISO 13485',
    '±0.05mm critical', 'SLS Operation, CNC Machining', 'SLS + Machining Certification', 'Build Platform, CNC Tools',
    'Argon Gas, Cutting Fluid', 'Support structures required', '{}', '{}', 'complex-template.slm', '', '1.0', 
    '15h 0m', 'TI1,TI2,CNC-01', '', 'Design', '{}', 'BT-{YYYY}-{####}', 'Standard', 
    '[]', '[]', 'BT_Standard_Workflow', 'Standard', '', ''
);

-- Assign multiple stages to the complex part
-- SLS Printing
INSERT OR IGNORE INTO PartStageRequirements (
    PartId, ProductionStageId, ExecutionOrder, IsRequired, IsActive,
    EstimatedHours, SetupTimeMinutes, EstimatedCost, MaterialCost,
    RequiresSpecificMachine, AllowParallelExecution, IsBlocking,
    CustomFieldValues, StageParameters, RequiredMaterials, RequiredTooling,
    QualityRequirements, SpecialInstructions, RequirementNotes,
    CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy
) VALUES (
    2, 1, 1, 1, 1, 12.0, 45, 1020.00, 37.50, 0, 0, 1, '{}', '{}', 
    'Ti-6Al-4V Powder', 'Build Platform', 'High precision SLS', 
    'Optimized parameters for machining stock', 'Primary manufacturing stage', 
    datetime('now'), datetime('now'), 'System', 'System'
);

-- CNC Machining
INSERT OR IGNORE INTO PartStageRequirements (
    PartId, ProductionStageId, ExecutionOrder, IsRequired, IsActive,
    EstimatedHours, SetupTimeMinutes, EstimatedCost, MaterialCost,
    RequiresSpecificMachine, AllowParallelExecution, IsBlocking,
    CustomFieldValues, StageParameters, RequiredMaterials, RequiredTooling,
    QualityRequirements, SpecialInstructions, RequirementNotes,
    CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy
) VALUES (
    2, 2, 2, 1, 1, 4.0, 30, 380.00, 5.00, 1, 0, 1, '{}', '{}', 
    'Cutting fluid', 'CNC tools, fixtures', 'Precision machining', 
    'Machine critical features to ±0.05mm', 'Secondary machining operations', 
    datetime('now'), datetime('now'), 'System', 'System'
);

-- Assembly
INSERT OR IGNORE INTO PartStageRequirements (
    PartId, ProductionStageId, ExecutionOrder, IsRequired, IsActive,
    EstimatedHours, SetupTimeMinutes, EstimatedCost, MaterialCost,
    RequiresSpecificMachine, AllowParallelExecution, IsBlocking,
    CustomFieldValues, StageParameters, RequiredMaterials, RequiredTooling,
    QualityRequirements, SpecialInstructions, RequirementNotes,
    CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy
) VALUES (
    2, 7, 3, 1, 1, 1.0, 10, 80.00, 0.00, 0, 0, 1, '{}', '{}', 
    '', 'Measuring tools', 'Complete inspection', 
    'Full dimensional and surface inspection', 'Final quality verification', 
    datetime('now'), datetime('now'), 'System', 'System'
);

-- Verify the data was inserted
SELECT 'Parts created:' as Result;
SELECT Id, PartNumber, Name, Description FROM Parts;

SELECT 'Stage assignments created:' as Result;
SELECT psr.PartId, p.PartNumber, ps.Name as StageName, psr.ExecutionOrder 
FROM PartStageRequirements psr 
JOIN Parts p ON psr.PartId = p.Id 
JOIN ProductionStages ps ON psr.ProductionStageId = ps.Id 
ORDER BY psr.PartId, psr.ExecutionOrder;