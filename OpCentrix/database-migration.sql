-- ============================================================================
-- CRM Database Schema Migration Script
-- This script adds missing tables and columns for the CRM task system
-- ============================================================================

-- Create CrmAlerts table
CREATE TABLE IF NOT EXISTS "CrmAlerts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CrmAlerts" PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NOT NULL,
    "Message" TEXT NULL,
    "Severity" TEXT NOT NULL DEFAULT 'Info',
    "IsDismissed" INTEGER NOT NULL DEFAULT 0,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Create indexes for CrmAlerts
CREATE INDEX IF NOT EXISTS "IX_CrmAlerts_Severity" ON "CrmAlerts" ("Severity");
CREATE INDEX IF NOT EXISTS "IX_CrmAlerts_IsDismissed" ON "CrmAlerts" ("IsDismissed");
CREATE INDEX IF NOT EXISTS "IX_CrmAlerts_CreatedDate" ON "CrmAlerts" ("CreatedDate");
CREATE INDEX IF NOT EXISTS "IX_CrmAlerts_IsDismissed_CreatedDate" ON "CrmAlerts" ("IsDismissed", "CreatedDate");

-- Add missing columns to CrmTasks table
-- Reminder and notification settings
ALTER TABLE "CrmTasks" ADD COLUMN "HasReminder" INTEGER NOT NULL DEFAULT 0;
ALTER TABLE "CrmTasks" ADD COLUMN "ReminderDateTime" TEXT NULL;
ALTER TABLE "CrmTasks" ADD COLUMN "ReminderMinutesBefore" INTEGER NOT NULL DEFAULT 15;
ALTER TABLE "CrmTasks" ADD COLUMN "ReminderType" TEXT NOT NULL DEFAULT 'Email';
ALTER TABLE "CrmTasks" ADD COLUMN "IsReminderSent" INTEGER NOT NULL DEFAULT 0;
ALTER TABLE "CrmTasks" ADD COLUMN "ReminderSentAt" TEXT NULL;

-- Notification preferences
ALTER TABLE "CrmTasks" ADD COLUMN "NotifyOnStatusChange" INTEGER NOT NULL DEFAULT 1;
ALTER TABLE "CrmTasks" ADD COLUMN "NotifyAssignee" INTEGER NOT NULL DEFAULT 1;
ALTER TABLE "CrmTasks" ADD COLUMN "NotifyCreator" INTEGER NOT NULL DEFAULT 1;

-- Create indexes for new CrmTasks columns
CREATE INDEX IF NOT EXISTS "IX_CrmTasks_HasReminder" ON "CrmTasks" ("HasReminder");
CREATE INDEX IF NOT EXISTS "IX_CrmTasks_ReminderDateTime" ON "CrmTasks" ("ReminderDateTime");
CREATE INDEX IF NOT EXISTS "IX_CrmTasks_IsReminderSent" ON "CrmTasks" ("IsReminderSent");

-- Verify the tables exist and have correct structure
.schema CrmAlerts
.schema CrmTasks