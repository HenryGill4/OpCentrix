-- ============================================================================
-- CRM Database Migration Summary
-- Applied on: $(Get-Date)
-- ============================================================================

-- COMPLETED MIGRATION STEPS:
-- 1. Created CrmAlerts table with all required columns
-- 2. Added missing reminder and notification columns to CrmTasks table
-- 3. Created performance indexes for both tables

-- TABLES CREATED:
-- - CrmAlerts: Stores system alerts and notifications

-- COLUMNS ADDED TO CrmTasks:
-- - HasReminder: Boolean flag for reminder enabled/disabled
-- - ReminderDateTime: When to send the reminder
-- - ReminderMinutesBefore: How many minutes before due date to remind
-- - ReminderType: Email, Browser, etc.
-- - IsReminderSent: Boolean flag tracking if reminder was sent
-- - ReminderSentAt: Timestamp when reminder was sent
-- - NotifyOnStatusChange: Boolean for status change notifications
-- - NotifyAssignee: Boolean for assignee notifications
-- - NotifyCreator: Boolean for creator notifications

-- INDEXES CREATED:
-- CrmAlerts:
--   - IX_CrmAlerts_Severity
--   - IX_CrmAlerts_IsDismissed
--   - IX_CrmAlerts_CreatedDate

-- CrmTasks:
--   - IX_CrmTasks_HasReminder
--   - IX_CrmTasks_ReminderDateTime
--   - IX_CrmTasks_IsReminderSent

-- VERIFICATION:
-- - Sample alert inserted successfully
-- - CrmTasks table accessible with new columns
-- - System should now work correctly with CRM alerts and reminders

-- The database schema now matches the Entity Framework model definitions.