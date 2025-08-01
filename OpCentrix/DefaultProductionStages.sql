-- Default Production Stages for OpCentrix Parts Refactoring
-- Insert default production stages if they don't exist

INSERT OR IGNORE INTO ProductionStages 
(Name, DisplayOrder, Description, DefaultSetupMinutes, DefaultHourlyRate, RequiresQualityCheck, RequiresApproval, AllowSkip, IsOptional, CreatedDate, IsActive, RequiredRole)
VALUES 
('SLS Printing', 1, 'Selective Laser Sintering metal printing process', 45, 85.00, 1, 0, 0, 0, datetime('now'), 1, NULL),
('CNC Machining', 2, 'Computer Numerical Control machining operations', 30, 95.00, 1, 0, 0, 1, datetime('now'), 1, NULL),
('EDM Operations', 3, 'Electrical Discharge Machining for complex geometries', 60, 110.00, 1, 1, 0, 1, datetime('now'), 1, NULL),
('Assembly', 4, 'Assembly of multiple components', 15, 75.00, 1, 0, 0, 1, datetime('now'), 1, NULL),
('Finishing', 5, 'Surface finishing, coating, and final processing', 20, 70.00, 1, 0, 0, 1, datetime('now'), 1, NULL),
('Quality Inspection', 6, 'Final quality control and inspection', 10, 80.00, 1, 1, 0, 0, datetime('now'), 1, NULL);

-- Verify the insert
SELECT COUNT(*) as 'Stages Created' FROM ProductionStages WHERE IsActive = 1;