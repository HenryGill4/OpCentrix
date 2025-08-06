-- Sample data for testing stages functionality
-- Create simple parts and stage assignments

-- Insert sample parts with minimal required fields
INSERT INTO Parts (
    PartNumber, Name, Description, Industry, Application, Material, SlsMaterial, 
    EstimatedHours, MaterialCostPerKg, StandardLaborCostPerHour, 
    PartCategory, PartClass, ProcessType, IsActive, RequiresSLSPrinting,
    CreatedDate, CreatedBy, LastModifiedDate, LastModifiedBy,
    PowderRequirementKg, WeightGrams, VolumeMm3, HeightMm, LengthMm, WidthMm,
    MaxSurfaceRoughnessRa, SetupTimeMinutes, PowderChangeoverTimeMinutes,
    PreheatingTimeMinutes, CoolingTimeMinutes, PostProcessingTimeMinutes,
    SupportRemovalTimeMinutes, AverageEfficiencyPercent, AverageQualityScore,
    AveragePowderUtilization, AvgDurationDays, MaxBatchSize, StageOrder,
    AverageActualHours, RequiredMachineType
) VALUES 
('P-001', 'Test Part 1', 'Simple SLS test part', 'Aerospace', 'Component', 
    'Ti-6Al-4V Grade 5', 'Ti-6Al-4V Grade 5', 8.0, 450.00, 85.00,
    'Production', 'B', 'SLS Metal', 1, 1,
    datetime('now'), 'System', datetime('now'), 'System',
    0.5, 100, 30000, 20, 50, 30, 25, 45, 30, 60, 240, 45, 0, 100, 100, 85, 1, 1, 1, 8.0, 'TruPrint 3000'),

('P-002', 'Test Part 2', 'Complex multi-stage part', 'Medical', 'Component', 
    'Ti-6Al-4V Grade 5', 'Ti-6Al-4V Grade 5', 12.0, 450.00, 85.00,
    'Production', 'A', 'SLS Metal', 1, 1,
    datetime('now'), 'System', datetime('now'), 'System',
    0.8, 150, 45000, 30, 70, 40, 25, 45, 30, 60, 240, 45, 0, 100, 100, 85, 1, 1, 1, 12.0, 'TruPrint 3000'),

('P-003', 'Test Part 3', 'Simple bracket', 'Automotive', 'Component', 
    'AlSi10Mg', 'AlSi10Mg', 6.0, 85.00, 85.00,
    'Production', 'C', 'SLS Metal', 1, 1,
    datetime('now'), 'System', datetime('now'), 'System',
    0.3, 75, 20000, 15, 40, 25, 25, 30, 30, 45, 180, 30, 0, 100, 100, 85, 1, 1, 1, 6.0, 'TruPrint 3000');

-- Insert stage assignments
INSERT INTO PartStageRequirements (
    PartId, ProductionStageId, ExecutionOrder, IsRequired, IsActive,
    EstimatedHours, SetupTimeMinutes, EstimatedCost, MaterialCost,
    RequiresSpecificMachine, AllowParallelExecution, IsBlocking,
    CustomFieldValues, StageParameters, RequiredMaterials, RequiredTooling,
    QualityRequirements, SpecialInstructions, RequirementNotes,
    CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy
) VALUES 
-- Part 1: SLS + Inspection
(1, 1, 1, 1, 1, 8.0, 45, 680.00, 25.00, 0, 0, 1, '{}', '{}', 'Ti-6Al-4V Powder', 'Build Platform', 'Standard SLS quality', 'Standard parameters', 'Primary stage', datetime('now'), datetime('now'), 'System', 'System'),
(1, 6, 2, 1, 1, 1.0, 10, 80.00, 0.00, 0, 0, 1, '{}', '{}', '', 'Measuring tools', 'Dimensional inspection', 'Check dimensions', 'Final inspection', datetime('now'), datetime('now'), 'System', 'System'),

-- Part 2: Multi-stage (SLS + CNC + Inspection)
(2, 1, 1, 1, 1, 12.0, 45, 1020.00, 37.50, 0, 0, 1, '{}', '{}', 'Ti-6Al-4V Powder', 'Build Platform', 'High precision SLS', 'Optimized parameters', 'Primary stage', datetime('now'), datetime('now'), 'System', 'System'),
(2, 2, 2, 1, 1, 4.0, 30, 380.00, 5.00, 1, 0, 1, '{}', '{}', 'Cutting fluid', 'CNC tools', 'Precision machining', 'Machine to tolerance', 'Secondary operations', datetime('now'), datetime('now'), 'System', 'System'),
(2, 6, 3, 1, 1, 1.0, 10, 80.00, 0.00, 0, 0, 1, '{}', '{}', '', 'Measuring tools', 'Complete inspection', 'Full inspection', 'Final verification', datetime('now'), datetime('now'), 'System', 'System'),

-- Part 3: SLS + Finishing + Inspection
(3, 1, 1, 1, 1, 6.0, 30, 510.00, 15.00, 0, 0, 1, '{}', '{}', 'AlSi10Mg Powder', 'Build Platform', 'Standard quality', 'Standard parameters', 'Primary stage', datetime('now'), datetime('now'), 'System', 'System'),
(3, 5, 2, 1, 1, 1.5, 20, 105.00, 8.00, 0, 1, 0, '{}', '{}', 'Finishing compounds', 'Finishing tools', 'Surface finishing', 'Achieve surface finish', 'Finishing', datetime('now'), datetime('now'), 'System', 'System'),
(3, 6, 3, 1, 1, 0.5, 10, 40.00, 0.00, 0, 0, 1, '{}', '{}', '', 'Measuring tools', 'Final inspection', 'Quality verification', 'Final check', datetime('now'), datetime('now'), 'System', 'System');