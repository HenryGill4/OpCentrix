-- This script creates sample CRM tasks for testing the employee task management system
-- Run this against your OpCentrix SQLite database

-- First, ensure we have some sample accounts
INSERT OR IGNORE INTO CrmAccounts (Name, Status, Notes, CreatedDate, LastModifiedDate)
VALUES 
('TechCorp Industries', 'Active', 'Key client for advanced manufacturing components', datetime('now'), datetime('now')),
('Precision Parts Ltd', 'Active', 'Aerospace components manufacturer', datetime('now'), datetime('now')),
('Innovation Labs', 'Active', 'Research and development focused company', datetime('now'), datetime('now'));

-- Create sample tasks for non-admin users (adjust user IDs based on your actual user table)
INSERT OR IGNORE INTO CrmTasks (Title, Description, Status, Priority, AssignedToUserId, AccountId, DueAt, CreatedByUserId, CreatedDate, LastModifiedDate)
VALUES 

-- Tasks for user ID 2 (replace with actual user IDs from your Users table)
('Quality Review - Part #1001', 'Review manufacturing quality and provide feedback on recent production batch. Check tolerances and surface finish requirements.', 'Open', 3, 2, 1, datetime('now', '+3 days'), 1, datetime('now', '-1 days'), datetime('now')),
('Process Documentation Update', 'Update process documentation for SLS printing procedures. Include new material specifications and quality checkpoints.', 'InProgress', 2, 2, 2, datetime('now', '+7 days'), 1, datetime('now', '-2 days'), datetime('now')),
('Client Follow-up Meeting', 'Schedule and conduct follow-up meeting with client regarding recent order delivery and future requirements.', 'Open', 4, 2, 3, datetime('now', '+1 days'), 1, datetime('now', '-3 hours'), datetime('now')),

-- Tasks for user ID 3 
('Equipment Calibration Review', 'Perform calibration review for SLS printer TI-1. Document any deviations and recommend corrective actions.', 'Open', 3, 3, 1, datetime('now', '+2 days'), 1, datetime('now', '-1 days'), datetime('now')),
('Material Inspection Report', 'Complete inspection report for incoming material batch. Verify compliance with specifications.', 'InProgress', 2, 3, 2, datetime('now', '+5 days'), 1, datetime('now', '-2 days'), datetime('now')),

-- Tasks for user ID 4
('Production Schedule Optimization', 'Review and optimize production schedule for upcoming week. Consider machine availability and order priorities.', 'Open', 4, 4, 1, datetime('now', '+1 days'), 1, datetime('now', '-4 hours'), datetime('now')),
('Customer Communication', 'Reach out to customer regarding delivery timeline updates. Provide detailed status report.', 'Open', 3, 4, 3, datetime('now', '+4 days'), 1, datetime('now', '-1 days'), datetime('now')),

-- An overdue task to test the overdue functionality
('OVERDUE: Machine Maintenance Log', 'Critical: Complete maintenance log for printer TI-2. This task is overdue and requires immediate attention.', 'InProgress', 5, 2, 1, datetime('now', '-2 days'), 1, datetime('now', '-5 days'), datetime('now'));

-- Add some progress entries for tasks that are in progress
INSERT OR IGNORE INTO CrmTaskProgress (TaskId, ProgressNote, PercentComplete, Status, CreatedByUserId, CreatedDate, ProgressType, IsVisibleToClient)
VALUES 
(2, 'Started reviewing current documentation. Identified areas that need updating based on new material specifications.', 25, 'InProgress', 2, datetime('now', '-1 days'), 'Update', 1),
(5, 'Completed initial material inspection. Found minor deviations in particle size distribution. Documenting findings.', 40, 'InProgress', 3, datetime('now', '-6 hours'), 'Update', 1),
(8, 'Started maintenance procedures. Completed pre-maintenance checks and safety protocols.', 30, 'InProgress', 2, datetime('now', '-1 days'), 'Update', 1);

-- Verify the data was inserted
SELECT 'Sample CRM Tasks Created:' as Message;
SELECT COUNT(*) as TaskCount FROM CrmTasks;
SELECT COUNT(*) as ProgressEntriesCount FROM CrmTaskProgress;
SELECT COUNT(*) as AccountsCount FROM CrmAccounts;

-- Show some sample tasks
SELECT 
    t.Id,
    t.Title,
    t.Status,
    t.Priority,
    t.AssignedToUserId,
    a.Name as AccountName,
    t.DueAt
FROM CrmTasks t
LEFT JOIN CrmAccounts a ON t.AccountId = a.Id
LIMIT 5;