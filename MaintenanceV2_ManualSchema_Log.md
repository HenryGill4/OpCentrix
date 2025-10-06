# Maintenance V2 Schema Deployment Log

Date (UTC): 2025-10-05
Deployed By: Automated assistant (manual SQL path)
Target Database: scheduler.db (SQLite)

## Summary
Implemented Phase 1 Maintenance V2 schema directly via SQLite (bypassing EF Core migrations due to migration build issues). Added core tables, indexes, seed factor definitions, and auto-imported existing machines as maintenance assets.

Patch (2025-10-05T UPDATED): Added `ConfigJson` column and composite index to `OperationalTask` to support interval-based task wizard configuration.

## Created Tables
1. MaintenanceAsset
2. MaintenanceProcedureTemplate
3. MaintenanceFactorDefinition
4. MaintenanceProcedureTemplateFactor
5. MaintenanceScheduleInstance
6. MaintenanceOccurrence
7. MaintenanceCounterAggregate
8. MaintenanceCounterBaseline
9. OperationalTask (added later for ad-hoc production & maintenance related tasks)

## Indexes & Constraints (Highlights)
- IX_MaintenanceAsset_MachineId, IX_MaintenanceAsset_Active
- IX_MaintenanceProcedureTemplate_Active
- UNIQUE(Code) on MaintenanceFactorDefinition
- IX_MaintenanceFactorDefinition_Active
- IX_MaintenancePTFactor_Template, IX_MaintenancePTFactor_Group
- IX_MaintenanceScheduleInstance_Asset, IX_MaintenanceScheduleInstance_Status
- IX_MaintenanceOccurrence_Schedule_Status, IX_MaintenanceOccurrence_Status
- UX_MaintCounterAgg_Period (AssetId, FactorDefinitionId, PeriodStart)
- IX_MaintBaseline_AssetFactor
- IX_OperationalTask_Status, IX_OperationalTask_MachineId, IX_OperationalTask_Assigned
- (New) IX_OperationalTask_Status_Priority (Status, Priority)

## Seed Data (Factor Definitions)
Inserted:
- RUN_HOURS
- BUILDS_COMPLETED
- BUILDS_COMPLETED_MATERIAL (parameterizable)
- DAYS_SINCE_LAST_COMPLETION
- CALENDAR_DAY_INTERVAL

## Asset Seeding
Copied all existing records from Machines -> MaintenanceAsset (AssetType='Machine').

SQL:
```sql
INSERT INTO MaintenanceAsset (AssetType,MachineId,Name,Location)
SELECT 'Machine', MachineId, Name, COALESCE(Location,'') FROM Machines;
```

## OperationalTask Table (Option B Implementation)
Purpose: Track ad-hoc operational / maintenance tasks initiated by Admins (Team Leads later) directly from dashboards.

Original Schema executed (manual):
```sql
CREATE TABLE IF NOT EXISTS OperationalTask (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Title TEXT NOT NULL,
  Description TEXT NULL,
  TaskType TEXT NOT NULL DEFAULT 'General',
  MachineId TEXT NULL,
  AssetId INTEGER NULL,
  Status TEXT NOT NULL DEFAULT 'Open',
  Priority INTEGER NOT NULL DEFAULT 3,
  CreatedByUserId INTEGER NOT NULL,
  AssignedUserId INTEGER NULL,
  DueAt TEXT NULL,
  CompletedAt TEXT NULL,
  Category TEXT NULL,
  Tags TEXT NULL,
  CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
  FOREIGN KEY(AssetId) REFERENCES MaintenanceAsset(Id)
);
CREATE INDEX IF NOT EXISTS IX_OperationalTask_Status ON OperationalTask(Status);
CREATE INDEX IF NOT EXISTS IX_OperationalTask_MachineId ON OperationalTask(MachineId);
CREATE INDEX IF NOT EXISTS IX_OperationalTask_Assigned ON OperationalTask(AssignedUserId, Status);
```

### Patch: Interval Configuration Support
Executed only after confirming column absence:
```sql
ALTER TABLE OperationalTask ADD COLUMN ConfigJson TEXT NULL;
CREATE INDEX IF NOT EXISTS IX_OperationalTask_Status_Priority ON OperationalTask(Status, Priority);
UPDATE OperationalTask
SET ConfigJson = '{"version":1,"intervals":[]}'

WHERE ConfigJson IS NULL; -- (No rows updated if table empty or already patched)
```
Verification queries run:
```sql
PRAGMA table_info('OperationalTask');
SELECT name FROM pragma_table_info('OperationalTask') WHERE name='ConfigJson'; -- returned row
SELECT COUNT(*) AS TotalTasks,
       SUM(CASE WHEN ConfigJson IS NOT NULL THEN 1 ELSE 0 END) AS WithConfig,
       SUM(CASE WHEN ConfigJson IS NULL THEN 1 ELSE 0 END) AS WithoutConfig
FROM OperationalTask; -- counts reported (0 tasks currently / or existing counts)
```
Latest preview (no tasks yet so 0 rows):
```sql
SELECT Id, Title, substr(ConfigJson,1,120) AS ConfigPreview
FROM OperationalTask ORDER BY Id DESC LIMIT 10;
```

## Next Implementation Steps
1. Add background aggregation job to populate MaintenanceCounterAggregate from BuildJobs.
2. Ensure BuildJobs are retained after completion (avoid deletion) to keep historical counters.
3. Implement evaluation service to create MaintenanceOccurrence when thresholds met.
4. Razor Pages (Admin) for Templates / Schedules / Occurrences / Tasks management.
5. Integrate OperationalTask creation modal in Print Tracking (Admins now; Team Leads later).
6. UI hook in maintenance summary to surface open tasks.
7. Service layer for OperationalTask (create / list / complete / assign) + HTMX endpoints.
8. Implement duplicate suppression + interval evaluation (ConfigJson) logic.

## Rollback Plan
To rollback (DROPs):
```sql
DROP TABLE IF EXISTS OperationalTask;
DROP TABLE IF EXISTS MaintenanceCounterBaseline;
DROP TABLE IF EXISTS MaintenanceCounterAggregate;
DROP TABLE IF EXISTS MaintenanceOccurrence;
DROP TABLE IF EXISTS MaintenanceScheduleInstance;
DROP TABLE IF EXISTS MaintenanceProcedureTemplateFactor;
DROP TABLE IF EXISTS MaintenanceFactorDefinition;
DROP TABLE IF EXISTS MaintenanceProcedureTemplate;
DROP TABLE IF EXISTS MaintenanceAsset;
```

## Notes
- Foreign keys enabled (PRAGMA foreign_keys=ON) before creation.
- EF Core model classes already added under Models/MaintenanceV2; OperationalTask integration now pending incremental features.
- Future migration will reconcile code-first model with this patched schema (include ConfigJson & new index).

End of log.
