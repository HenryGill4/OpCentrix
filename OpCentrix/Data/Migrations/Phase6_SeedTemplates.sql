-- PHASE 6: Default Stage Templates Seeding
-- Insert default stage templates for B&T manufacturing

-- Template 1: B&T Suppressor Component
INSERT OR IGNORE INTO StageTemplates (Name, Description, Industry, MaterialType, ComplexityLevel, IsDefault, EstimatedTotalHours, EstimatedTotalCost, CreatedBy)
VALUES ('B&T Suppressor Component', 'Complete manufacturing workflow for B&T suppressor components with serialization', 'Firearms', 'Titanium', 'Complex', 1, 16.5, 1485.00, 'System');

-- Template 2: Simple Prototype Part  
INSERT OR IGNORE INTO StageTemplates (Name, Description, Industry, MaterialType, ComplexityLevel, IsDefault, EstimatedTotalHours, EstimatedTotalCost, CreatedBy)
VALUES ('Simple Prototype', 'Basic workflow for prototype development parts', 'General', 'Metal', 'Simple', 1, 4.0, 340.00, 'System');

-- Template 3: Standard Titanium Manufacturing
INSERT OR IGNORE INTO StageTemplates (Name, Description, Industry, MaterialType, ComplexityLevel, IsDefault, EstimatedTotalHours, EstimatedTotalCost, CreatedBy)
VALUES ('Standard Titanium Manufacturing', 'Standard workflow for titanium parts with quality inspection', 'Aerospace', 'Titanium', 'Medium', 1, 8.5, 765.00, 'System');

-- Template 4: High Volume Production
INSERT OR IGNORE INTO StageTemplates (Name, Description, Industry, MaterialType, ComplexityLevel, IsDefault, EstimatedTotalHours, EstimatedTotalCost, CreatedBy)
VALUES ('High Volume Production', 'Optimized workflow for high volume manufacturing', 'Automotive', 'Steel', 'Simple', 0, 6.0, 450.00, 'System');

-- Now get the template IDs for creating steps
-- We'll need to insert template steps manually since SQLite doesn't support LAST_INSERT_ID() like MySQL

-- First, let's check if our templates were created and get their IDs
SELECT 'Templates created:', Id, Name FROM StageTemplates WHERE CreatedBy = 'System' AND Name IN (
    'B&T Suppressor Component', 
    'Simple Prototype', 
    'Standard Titanium Manufacturing', 
    'High Volume Production'
);