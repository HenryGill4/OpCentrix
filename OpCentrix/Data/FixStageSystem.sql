-- Stage System Database Fix Script
-- Ensures all required tables and data exist for the part form stage system

-- Check if ProductionStages table exists and has required data
INSERT OR IGNORE INTO ProductionStages (
    Id, Name, DisplayOrder, Description, 
    DefaultSetupMinutes, DefaultHourlyRate, DefaultDurationHours,
    RequiresQualityCheck, RequiresApproval, AllowSkip, IsOptional,
    IsActive, CreatedDate, CreatedBy, LastModifiedBy, LastModifiedDate,
    StageColor, StageIcon, Department
) VALUES 
(1, 'SLS Printing', 1, 'Selective Laser Sintering metal printing', 45, 85.00, 4.0, 1, 0, 0, 0, 1, datetime('now'), 'System', 'System', datetime('now'), '#007bff', 'fas fa-print', '3D Printing'),
(2, 'Post-Processing', 2, 'Support removal and finishing', 15, 75.00, 1.0, 1, 0, 0, 1, 1, datetime('now'), 'System', 'System', datetime('now'), '#28a745', 'fas fa-brush', 'Finishing'),
(3, 'CNC Machining', 3, 'Computer Numerical Control machining', 30, 95.00, 2.0, 1, 0, 0, 1, 1, datetime('now'), 'System', 'System', datetime('now'), '#28a745', 'fas fa-cogs', 'CNC Machining'),
(4, 'EDM Operations', 4, 'Electrical Discharge Machining', 60, 110.00, 3.0, 1, 1, 0, 1, 1, datetime('now'), 'System', 'System', datetime('now'), '#ffc107', 'fas fa-bolt', 'EDM'),
(5, 'Assembly', 5, 'Multi-component assembly operations', 15, 75.00, 1.5, 1, 0, 0, 1, 1, datetime('now'), 'System', 'System', datetime('now'), '#17a2b8', 'fas fa-puzzle-piece', 'Assembly'),
(6, 'Quality Inspection', 6, 'Final quality control and testing', 10, 80.00, 0.5, 1, 1, 0, 0, 1, datetime('now'), 'System', 'System', datetime('now'), '#dc3545', 'fas fa-search', 'Quality Control');

-- Verify the tables exist and show their structure
SELECT 'ProductionStages table verification:' as info;
SELECT COUNT(*) as active_stages FROM ProductionStages WHERE IsActive = 1;

SELECT 'PartStageRequirements table verification:' as info;
SELECT COUNT(*) as total_requirements FROM PartStageRequirements WHERE IsActive = 1;

-- Display some sample data
SELECT 'Sample ProductionStages:' as info;
SELECT Id, Name, StageColor, StageIcon, Department FROM ProductionStages WHERE IsActive = 1 LIMIT 3;

SELECT 'Sample PartStageRequirements:' as info;
SELECT psr.Id, psr.PartId, ps.Name as StageName, psr.ExecutionOrder, psr.EstimatedHours 
FROM PartStageRequirements psr 
LEFT JOIN ProductionStages ps ON psr.ProductionStageId = ps.Id 
WHERE psr.IsActive = 1 
LIMIT 3;