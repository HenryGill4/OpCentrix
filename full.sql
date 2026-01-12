ALTER TABLE "Materials" ADD "QuantityOnHandKg" TEXT NOT NULL DEFAULT '0.0';

CREATE TABLE "ef_temp_MasterParts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MasterParts" PRIMARY KEY AUTOINCREMENT,
    "AllowStacking" INTEGER NOT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "Description" TEXT NOT NULL,
    "DoubleStackDurationHours" REAL NULL,
    "EnableDoubleStack" INTEGER NOT NULL,
    "EnableTripleStack" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "ManufacturingApproach" TEXT NOT NULL DEFAULT 'SLS-Based',
    "Material" TEXT NOT NULL,
    "MaxStackCount" INTEGER NOT NULL,
    "Name" TEXT NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "PartsPerBuildDouble" INTEGER NULL,
    "PartsPerBuildSingle" INTEGER NOT NULL,
    "PartsPerBuildTriple" INTEGER NULL,
    "RequiredStages" TEXT NOT NULL DEFAULT '[]',
    "SingleStackDurationHours" REAL NULL,
    "StageEstimateSingle" REAL NULL,
    "TripleStackDurationHours" REAL NULL
);

INSERT INTO "ef_temp_MasterParts" ("Id", "AllowStacking", "CreatedBy", "CreatedDate", "Description", "DoubleStackDurationHours", "EnableDoubleStack", "EnableTripleStack", "IsActive", "LastModifiedBy", "LastModifiedDate", "ManufacturingApproach", "Material", "MaxStackCount", "Name", "PartNumber", "PartsPerBuildDouble", "PartsPerBuildSingle", "PartsPerBuildTriple", "RequiredStages", "SingleStackDurationHours", "StageEstimateSingle", "TripleStackDurationHours")
SELECT "Id", "AllowStacking", "CreatedBy", "CreatedDate", "Description", "DoubleStackDurationHours", "EnableDoubleStack", "EnableTripleStack", "IsActive", "LastModifiedBy", "LastModifiedDate", "ManufacturingApproach", "Material", IFNULL("MaxStackCount", 0), "Name", "PartNumber", "PartsPerBuildDouble", "PartsPerBuildSingle", "PartsPerBuildTriple", "RequiredStages", "SingleStackDurationHours", "StageEstimateSingle", "TripleStackDurationHours"
FROM "MasterParts";

PRAGMA foreign_keys = 0;

DROP TABLE "MasterParts";

ALTER TABLE "ef_temp_MasterParts" RENAME TO "MasterParts";

PRAGMA foreign_keys = 1;

CREATE INDEX "IX_MasterParts_IsActive" ON "MasterParts" ("IsActive");

CREATE INDEX "IX_MasterParts_ManufacturingApproach" ON "MasterParts" ("ManufacturingApproach");

CREATE UNIQUE INDEX "IX_MasterParts_PartNumber" ON "MasterParts" ("PartNumber");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251002175517_TempVerify', '8.0.11');

ALTER TABLE "Jobs" ADD "UpstreamGapHours" REAL NULL;

ALTER TABLE "BuildJobs" ADD "Material" TEXT NULL;

CREATE TABLE "CrmAccounts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CrmAccounts" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Active',
    "Notes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE "MachineComponents" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MachineComponents" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "DisplayOrder" INTEGER NOT NULL,
    "Category" TEXT NOT NULL DEFAULT 'General',
    "Icon" TEXT NULL DEFAULT 'cog',
    "ColorCode" TEXT NULL DEFAULT '#6B7280',
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL
);

CREATE TABLE "MaintenanceActionLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceActionLogs" PRIMARY KEY AUTOINCREMENT,
    "RuleId" INTEGER NOT NULL,
    "MachineId" TEXT NOT NULL,
    "PerformedByUserId" INTEGER NOT NULL,
    "PerformedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "Notes" TEXT NULL,
    "ResetPerformed" INTEGER NOT NULL
);

CREATE TABLE "MaintenanceAssets" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceAssets" PRIMARY KEY AUTOINCREMENT,
    "AssetType" TEXT NOT NULL,
    "MachineId" TEXT NULL,
    "Name" TEXT NOT NULL,
    "Location" TEXT NULL,
    "Department" TEXT NULL,
    "Active" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "MetadataJson" TEXT NULL
);

CREATE TABLE "MaintenanceFactorDefinitions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceFactorDefinitions" PRIMARY KEY AUTOINCREMENT,
    "Code" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "FactorType" TEXT NOT NULL DEFAULT 'Counter',
    "SourceType" TEXT NOT NULL DEFAULT 'BuildData',
    "ParametersSchemaJson" TEXT NULL,
    "Unit" TEXT NOT NULL DEFAULT '',
    "SupportsParameterization" INTEGER NOT NULL,
    "IsSystem" INTEGER NOT NULL,
    "Active" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE "MaintenanceProcedureTemplates" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceProcedureTemplates" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "AppliesToAssetType" TEXT NULL,
    "DefaultInstructions" TEXT NULL,
    "SafetyNotes" TEXT NULL,
    "EstimatedDurationMinutes" INTEGER NULL,
    "DefaultPriority" INTEGER NOT NULL,
    "RecurrenceStrategy" TEXT NOT NULL DEFAULT 'FactorBased',
    "GroupCombinationOperator" TEXT NOT NULL DEFAULT 'OR',
    "Active" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt" TEXT NULL,
    "Version" INTEGER NOT NULL,
    "Tags" TEXT NULL,
    "MetadataJson" TEXT NULL
);

CREATE TABLE "MaintenanceRules" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceRules" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NULL,
    "MachineComponentId" INTEGER NULL,
    "ProductionStageId" INTEGER NULL,
    "Title" TEXT NOT NULL,
    "Description" TEXT NULL,
    "TriggerType" INTEGER NOT NULL,
    "Severity" INTEGER NOT NULL,
    "ThresholdValue" REAL NOT NULL,
    "IntervalDays" INTEGER NULL,
    "IsActive" INTEGER NOT NULL,
    "EarlyWarningPercent" INTEGER NULL,
    "EarlyWarningDays" INTEGER NULL,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL
);

CREATE TABLE "MaintenanceStates" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceStates" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NOT NULL,
    "RuleId" INTEGER NOT NULL,
    "CurrentValue" REAL NOT NULL,
    "LastServiceDate" TEXT NULL,
    "NextDueDate" TEXT NULL,
    "IsDue" INTEGER NOT NULL,
    "IsOverdue" INTEGER NOT NULL,
    "CalculatedAt" TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE "ScheduleAdjustments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ScheduleAdjustments" PRIMARY KEY AUTOINCREMENT,
    "TriggerJobId" INTEGER NOT NULL,
    "AffectedJobId" INTEGER NOT NULL,
    "OriginalStart" TEXT NOT NULL,
    "OriginalEnd" TEXT NOT NULL,
    "NewStart" TEXT NOT NULL,
    "NewEnd" TEXT NOT NULL,
    "ShiftMinutes" REAL NOT NULL,
    "Reason" TEXT NOT NULL,
    "Notes" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL
);

CREATE TABLE "CrmContacts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CrmContacts" PRIMARY KEY AUTOINCREMENT,
    "AccountId" INTEGER NOT NULL,
    "Name" TEXT NOT NULL,
    "Email" TEXT NULL,
    "Phone" TEXT NULL,
    "Title" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_CrmContacts_CrmAccounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "CrmAccounts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "MaintenanceSchedules" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceSchedules" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NOT NULL,
    "MachineComponentId" INTEGER NULL,
    "ScheduleName" TEXT NOT NULL,
    "Description" TEXT NULL,
    "ScheduleType" INTEGER NOT NULL,
    "IntervalDays" INTEGER NULL,
    "IntervalWeeks" INTEGER NULL,
    "IntervalMonths" INTEGER NULL,
    "OperatingHoursInterval" REAL NULL,
    "CyclesInterval" INTEGER NULL,
    "UnitsProducedInterval" INTEGER NULL,
    "ConditionThreshold" REAL NULL,
    "ConditionMetric" TEXT NULL,
    "LastMaintenanceDate" TEXT NULL,
    "NextMaintenanceDate" TEXT NULL,
    "EstimatedDurationHours" REAL NOT NULL,
    "EstimatedCost" TEXT NOT NULL,
    "DefaultTechnician" TEXT NULL,
    "DefaultTechnicianUserId" INTEGER NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "AutoCreateWorkOrders" INTEGER NOT NULL,
    "LeadTimeDays" INTEGER NULL,
    "Instructions" TEXT NULL,
    "RequiredTools" TEXT NULL,
    "RequiredParts" TEXT NULL,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "MachineId1" INTEGER NULL,
    CONSTRAINT "FK_MaintenanceSchedules_MachineComponents_MachineComponentId" FOREIGN KEY ("MachineComponentId") REFERENCES "MachineComponents" ("Id"),
    CONSTRAINT "FK_MaintenanceSchedules_Machines_MachineId1" FOREIGN KEY ("MachineId1") REFERENCES "Machines" ("Id"),
    CONSTRAINT "FK_MaintenanceSchedules_Users_DefaultTechnicianUserId" FOREIGN KEY ("DefaultTechnicianUserId") REFERENCES "Users" ("Id")
);

CREATE TABLE "MaintenanceServices" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceServices" PRIMARY KEY AUTOINCREMENT,
    "ServiceName" TEXT NOT NULL,
    "Description" TEXT NULL,
    "ServiceType" INTEGER NOT NULL,
    "MachineId" TEXT NOT NULL,
    "MachineComponentId" INTEGER NULL,
    "IsEnabled" INTEGER NOT NULL DEFAULT 1,
    "UpdateIntervalMinutes" INTEGER NOT NULL,
    "DataSource" TEXT NULL,
    "Configuration" TEXT NULL,
    "CurrentValue" REAL NOT NULL,
    "LastUpdateTime" TEXT NOT NULL,
    "LastResetTime" TEXT NULL,
    "WarningThreshold" REAL NULL,
    "CriticalThreshold" REAL NULL,
    "MaxValue" REAL NULL,
    "Unit" TEXT NULL,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "MachineId1" INTEGER NULL,
    CONSTRAINT "FK_MaintenanceServices_MachineComponents_MachineComponentId" FOREIGN KEY ("MachineComponentId") REFERENCES "MachineComponents" ("Id"),
    CONSTRAINT "FK_MaintenanceServices_Machines_MachineId1" FOREIGN KEY ("MachineId1") REFERENCES "Machines" ("Id")
);

CREATE TABLE "OperationalTasks" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_OperationalTasks" PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NOT NULL,
    "Description" TEXT NULL,
    "TaskType" TEXT NOT NULL,
    "MachineId" TEXT NULL,
    "AssetId" INTEGER NULL,
    "Status" TEXT NOT NULL,
    "Priority" INTEGER NOT NULL,
    "CreatedByUserId" INTEGER NOT NULL,
    "AssignedUserId" INTEGER NULL,
    "DueAt" TEXT NULL,
    "CompletedAt" TEXT NULL,
    "Category" TEXT NULL,
    "Tags" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "ConfigJson" TEXT NULL,
    "OverdueFlag" INTEGER NULL,
    "LastResetAt" TEXT NULL,
    CONSTRAINT "FK_OperationalTasks_MaintenanceAssets_AssetId" FOREIGN KEY ("AssetId") REFERENCES "MaintenanceAssets" ("Id")
);

CREATE TABLE "MaintenanceCounterAggregates" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceCounterAggregates" PRIMARY KEY AUTOINCREMENT,
    "AssetId" INTEGER NOT NULL,
    "FactorDefinitionId" INTEGER NOT NULL,
    "PeriodStart" TEXT NOT NULL,
    "PeriodEnd" TEXT NOT NULL,
    "Value" REAL NOT NULL,
    "LastUpdatedAt" TEXT NOT NULL,
    CONSTRAINT "FK_MaintenanceCounterAggregates_MaintenanceAssets_AssetId" FOREIGN KEY ("AssetId") REFERENCES "MaintenanceAssets" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_MaintenanceCounterAggregates_MaintenanceFactorDefinitions_FactorDefinitionId" FOREIGN KEY ("FactorDefinitionId") REFERENCES "MaintenanceFactorDefinitions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "MaintenanceProcedureTemplateFactors" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceProcedureTemplateFactors" PRIMARY KEY AUTOINCREMENT,
    "ProcedureTemplateId" INTEGER NOT NULL,
    "FactorDefinitionId" INTEGER NOT NULL,
    "ParameterJson" TEXT NULL,
    "ThresholdValue" TEXT NOT NULL,
    "ComparisonOperator" TEXT NOT NULL DEFAULT 'GreaterOrEqual',
    "LogicalGroupKey" TEXT NOT NULL DEFAULT 'A',
    "GroupOperator" TEXT NOT NULL DEFAULT 'AND',
    "ResetCounterOnCompletion" INTEGER NOT NULL,
    "Optional" INTEGER NOT NULL,
    "SequenceOrder" INTEGER NOT NULL,
    "Weight" INTEGER NULL,
    "Notes" TEXT NULL,
    CONSTRAINT "FK_MaintenanceProcedureTemplateFactors_MaintenanceFactorDefinitions_FactorDefinitionId" FOREIGN KEY ("FactorDefinitionId") REFERENCES "MaintenanceFactorDefinitions" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MaintenanceProcedureTemplateFactors_MaintenanceProcedureTemplates_ProcedureTemplateId" FOREIGN KEY ("ProcedureTemplateId") REFERENCES "MaintenanceProcedureTemplates" ("Id") ON DELETE CASCADE
);

CREATE TABLE "MaintenanceScheduleInstances" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceScheduleInstances" PRIMARY KEY AUTOINCREMENT,
    "ProcedureTemplateId" INTEGER NOT NULL,
    "AssetId" INTEGER NOT NULL,
    "CustomName" TEXT NULL,
    "Enabled" INTEGER NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Active',
    "NextDueAt" TEXT NULL,
    "LastEvaluatedAt" TEXT NULL,
    "LastCompletionAt" TEXT NULL,
    "LastOccurrenceId" INTEGER NULL,
    "OverrideJson" TEXT NULL,
    "PriorityOverride" INTEGER NULL,
    "CalendarPattern" TEXT NULL,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_MaintenanceScheduleInstances_MaintenanceAssets_AssetId" FOREIGN KEY ("AssetId") REFERENCES "MaintenanceAssets" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_MaintenanceScheduleInstances_MaintenanceProcedureTemplates_ProcedureTemplateId" FOREIGN KEY ("ProcedureTemplateId") REFERENCES "MaintenanceProcedureTemplates" ("Id") ON DELETE CASCADE
);

CREATE TABLE "MaintenanceWorkOrders" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceWorkOrders" PRIMARY KEY AUTOINCREMENT,
    "WorkOrderNumber" TEXT NOT NULL,
    "MachineId" TEXT NOT NULL,
    "MachineComponentId" INTEGER NULL,
    "MaintenanceRuleId" INTEGER NULL,
    "Title" TEXT NOT NULL,
    "Description" TEXT NULL,
    "WorkOrderType" INTEGER NOT NULL,
    "Priority" INTEGER NOT NULL,
    "Status" INTEGER NOT NULL,
    "ScheduledStartDate" TEXT NULL,
    "ScheduledEndDate" TEXT NULL,
    "ActualStartDate" TEXT NULL,
    "ActualEndDate" TEXT NULL,
    "AssignedTechnician" TEXT NULL,
    "AssignedTechnicianUserId" INTEGER NULL,
    "EstimatedHours" REAL NOT NULL,
    "ActualHours" REAL NULL,
    "EstimatedCost" TEXT NOT NULL,
    "ActualCost" TEXT NULL,
    "WorkPerformed" TEXT NULL,
    "PartsUsed" TEXT NULL,
    "Notes" TEXT NULL,
    "CompletionNotes" TEXT NULL,
    "RequiresShutdown" INTEGER NOT NULL,
    "ShutdownDurationMinutes" INTEGER NULL,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "UpdatedAt" TEXT NULL,
    "UpdatedBy" TEXT NULL,
    "MachineId1" INTEGER NULL,
    CONSTRAINT "FK_MaintenanceWorkOrders_MachineComponents_MachineComponentId" FOREIGN KEY ("MachineComponentId") REFERENCES "MachineComponents" ("Id"),
    CONSTRAINT "FK_MaintenanceWorkOrders_Machines_MachineId1" FOREIGN KEY ("MachineId1") REFERENCES "Machines" ("Id"),
    CONSTRAINT "FK_MaintenanceWorkOrders_MaintenanceRules_MaintenanceRuleId" FOREIGN KEY ("MaintenanceRuleId") REFERENCES "MaintenanceRules" ("Id"),
    CONSTRAINT "FK_MaintenanceWorkOrders_Users_AssignedTechnicianUserId" FOREIGN KEY ("AssignedTechnicianUserId") REFERENCES "Users" ("Id")
);

CREATE TABLE "CrmTasks" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CrmTasks" PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Open',
    "Priority" INTEGER NOT NULL,
    "DueAt" TEXT NULL,
    "CompletedAt" TEXT NULL,
    "AssignedToUserId" INTEGER NULL,
    "CreatedByUserId" INTEGER NOT NULL,
    "AccountId" INTEGER NULL,
    "ContactId" INTEGER NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_CrmTasks_CrmAccounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "CrmAccounts" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_CrmTasks_CrmContacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES "CrmContacts" ("Id") ON DELETE SET NULL
);

CREATE TABLE "MaintenanceServiceData" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceServiceData" PRIMARY KEY AUTOINCREMENT,
    "MaintenanceServiceId" INTEGER NOT NULL,
    "Value" REAL NOT NULL,
    "Timestamp" TEXT NOT NULL DEFAULT (datetime('now')),
    "Source" TEXT NULL,
    "Notes" TEXT NULL,
    "IsCalculated" INTEGER NOT NULL,
    "IsReset" INTEGER NOT NULL,
    "MaintenanceServiceId1" INTEGER NULL,
    CONSTRAINT "FK_MaintenanceServiceData_MaintenanceServices_MaintenanceServiceId" FOREIGN KEY ("MaintenanceServiceId") REFERENCES "MaintenanceServices" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_MaintenanceServiceData_MaintenanceServices_MaintenanceServiceId1" FOREIGN KEY ("MaintenanceServiceId1") REFERENCES "MaintenanceServices" ("Id")
);

CREATE TABLE "MaintenanceOccurrences" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceOccurrences" PRIMARY KEY AUTOINCREMENT,
    "ScheduleInstanceId" INTEGER NOT NULL,
    "OpenedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "DueAt" TEXT NULL,
    "CompletedAt" TEXT NULL,
    "CompletedByUserId" INTEGER NULL,
    "Status" TEXT NOT NULL DEFAULT 'Open',
    "EvaluationSnapshotJson" TEXT NOT NULL DEFAULT '{}',
    "CompletionNotes" TEXT NULL,
    "ActualDurationMinutes" INTEGER NULL,
    "AutoGenerated" INTEGER NOT NULL,
    "CreatedFromFactorGroup" TEXT NULL,
    CONSTRAINT "FK_MaintenanceOccurrences_MaintenanceScheduleInstances_ScheduleInstanceId" FOREIGN KEY ("ScheduleInstanceId") REFERENCES "MaintenanceScheduleInstances" ("Id") ON DELETE CASCADE
);

CREATE TABLE "MaintenanceNotifications" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceNotifications" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NOT NULL,
    "MaintenanceRuleId" INTEGER NULL,
    "MaintenanceServiceId" INTEGER NULL,
    "WorkOrderId" INTEGER NULL,
    "NotificationType" INTEGER NOT NULL,
    "Severity" INTEGER NOT NULL,
    "Title" TEXT NOT NULL,
    "Message" TEXT NULL,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "AcknowledgedAt" TEXT NULL,
    "AcknowledgedBy" TEXT NULL,
    "IsEmailSent" INTEGER NOT NULL,
    "IsSmsSent" INTEGER NOT NULL,
    "IsBrowserNotificationSent" INTEGER NOT NULL,
    "Recipients" TEXT NULL,
    "AutoDismissAt" TEXT NULL,
    "IsDismissed" INTEGER NOT NULL DEFAULT 0,
    "MachineId1" INTEGER NULL,
    CONSTRAINT "FK_MaintenanceNotifications_Machines_MachineId1" FOREIGN KEY ("MachineId1") REFERENCES "Machines" ("Id"),
    CONSTRAINT "FK_MaintenanceNotifications_MaintenanceRules_MaintenanceRuleId" FOREIGN KEY ("MaintenanceRuleId") REFERENCES "MaintenanceRules" ("Id"),
    CONSTRAINT "FK_MaintenanceNotifications_MaintenanceServices_MaintenanceServiceId" FOREIGN KEY ("MaintenanceServiceId") REFERENCES "MaintenanceServices" ("Id"),
    CONSTRAINT "FK_MaintenanceNotifications_MaintenanceWorkOrders_WorkOrderId" FOREIGN KEY ("WorkOrderId") REFERENCES "MaintenanceWorkOrders" ("Id")
);

CREATE TABLE "MaintenanceCounterBaselines" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MaintenanceCounterBaselines" PRIMARY KEY AUTOINCREMENT,
    "AssetId" INTEGER NOT NULL,
    "FactorDefinitionId" INTEGER NOT NULL,
    "OccurrenceId" INTEGER NOT NULL,
    "BaselineValue" REAL NOT NULL,
    "RecordedAt" TEXT NOT NULL,
    CONSTRAINT "FK_MaintenanceCounterBaselines_MaintenanceAssets_AssetId" FOREIGN KEY ("AssetId") REFERENCES "MaintenanceAssets" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_MaintenanceCounterBaselines_MaintenanceFactorDefinitions_FactorDefinitionId" FOREIGN KEY ("FactorDefinitionId") REFERENCES "MaintenanceFactorDefinitions" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_MaintenanceCounterBaselines_MaintenanceOccurrences_OccurrenceId" FOREIGN KEY ("OccurrenceId") REFERENCES "MaintenanceOccurrences" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_CrmAccounts_Name" ON "CrmAccounts" ("Name");

CREATE INDEX "IX_CrmContacts_AccountId" ON "CrmContacts" ("AccountId");

CREATE INDEX "IX_CrmContacts_Name" ON "CrmContacts" ("Name");

CREATE INDEX "IX_CrmTasks_AccountId" ON "CrmTasks" ("AccountId");

CREATE INDEX "IX_CrmTasks_AssignedToUserId" ON "CrmTasks" ("AssignedToUserId");

CREATE INDEX "IX_CrmTasks_ContactId" ON "CrmTasks" ("ContactId");

CREATE INDEX "IX_CrmTasks_DueAt" ON "CrmTasks" ("DueAt");

CREATE INDEX "IX_CrmTasks_Status" ON "CrmTasks" ("Status");

CREATE INDEX "IX_MachineComponents_MachineId" ON "MachineComponents" ("MachineId");

CREATE INDEX "IX_MachineComponents_MachineId_Category" ON "MachineComponents" ("MachineId", "Category");

CREATE INDEX "IX_MachineComponents_MachineId_IsActive" ON "MachineComponents" ("MachineId", "IsActive");

CREATE INDEX "IX_MaintenanceActionLogs_MachineId" ON "MaintenanceActionLogs" ("MachineId");

CREATE INDEX "IX_MaintenanceActionLogs_PerformedAt" ON "MaintenanceActionLogs" ("PerformedAt");

CREATE INDEX "IX_MaintenanceActionLogs_PerformedByUserId" ON "MaintenanceActionLogs" ("PerformedByUserId");

CREATE INDEX "IX_MaintenanceActionLogs_RuleId" ON "MaintenanceActionLogs" ("RuleId");

CREATE INDEX "IX_MaintenanceAssets_Active" ON "MaintenanceAssets" ("Active");

CREATE INDEX "IX_MaintenanceAssets_MachineId" ON "MaintenanceAssets" ("MachineId");

CREATE UNIQUE INDEX "IX_MaintenanceCounterAggregates_AssetId_FactorDefinitionId_PeriodStart" ON "MaintenanceCounterAggregates" ("AssetId", "FactorDefinitionId", "PeriodStart");

CREATE INDEX "IX_MaintenanceCounterAggregates_FactorDefinitionId" ON "MaintenanceCounterAggregates" ("FactorDefinitionId");

CREATE INDEX "IX_MaintenanceCounterBaselines_AssetId_FactorDefinitionId" ON "MaintenanceCounterBaselines" ("AssetId", "FactorDefinitionId");

CREATE INDEX "IX_MaintenanceCounterBaselines_FactorDefinitionId" ON "MaintenanceCounterBaselines" ("FactorDefinitionId");

CREATE INDEX "IX_MaintenanceCounterBaselines_OccurrenceId" ON "MaintenanceCounterBaselines" ("OccurrenceId");

CREATE INDEX "IX_MaintenanceFactorDefinitions_Active" ON "MaintenanceFactorDefinitions" ("Active");

CREATE UNIQUE INDEX "IX_MaintenanceFactorDefinitions_Code" ON "MaintenanceFactorDefinitions" ("Code");

CREATE INDEX "IX_MaintenanceNotifications_CreatedAt" ON "MaintenanceNotifications" ("CreatedAt");

CREATE INDEX "IX_MaintenanceNotifications_IsDismissed" ON "MaintenanceNotifications" ("IsDismissed");

CREATE INDEX "IX_MaintenanceNotifications_MachineId" ON "MaintenanceNotifications" ("MachineId");

CREATE INDEX "IX_MaintenanceNotifications_MachineId1" ON "MaintenanceNotifications" ("MachineId1");

CREATE INDEX "IX_MaintenanceNotifications_MaintenanceRuleId" ON "MaintenanceNotifications" ("MaintenanceRuleId");

CREATE INDEX "IX_MaintenanceNotifications_MaintenanceServiceId" ON "MaintenanceNotifications" ("MaintenanceServiceId");

CREATE INDEX "IX_MaintenanceNotifications_NotificationType" ON "MaintenanceNotifications" ("NotificationType");

CREATE INDEX "IX_MaintenanceNotifications_Severity" ON "MaintenanceNotifications" ("Severity");

CREATE INDEX "IX_MaintenanceNotifications_WorkOrderId" ON "MaintenanceNotifications" ("WorkOrderId");

CREATE INDEX "IX_MaintenanceOccurrences_ScheduleInstanceId_Status" ON "MaintenanceOccurrences" ("ScheduleInstanceId", "Status");

CREATE INDEX "IX_MaintenanceProcedureTemplateFactors_FactorDefinitionId" ON "MaintenanceProcedureTemplateFactors" ("FactorDefinitionId");

CREATE INDEX "IX_MaintenanceProcedureTemplateFactors_ProcedureTemplateId_LogicalGroupKey" ON "MaintenanceProcedureTemplateFactors" ("ProcedureTemplateId", "LogicalGroupKey");

CREATE INDEX "IX_MaintenanceProcedureTemplates_Active" ON "MaintenanceProcedureTemplates" ("Active");

CREATE INDEX "IX_MaintenanceRules_IsActive" ON "MaintenanceRules" ("IsActive");

CREATE INDEX "IX_MaintenanceRules_MachineComponentId" ON "MaintenanceRules" ("MachineComponentId");

CREATE INDEX "IX_MaintenanceRules_MachineComponentId_IsActive" ON "MaintenanceRules" ("MachineComponentId", "IsActive");

CREATE INDEX "IX_MaintenanceRules_MachineId" ON "MaintenanceRules" ("MachineId");

CREATE INDEX "IX_MaintenanceRules_MachineId_IsActive" ON "MaintenanceRules" ("MachineId", "IsActive");

CREATE INDEX "IX_MaintenanceRules_ProductionStageId" ON "MaintenanceRules" ("ProductionStageId");

CREATE INDEX "IX_MaintenanceScheduleInstances_AssetId" ON "MaintenanceScheduleInstances" ("AssetId");

CREATE INDEX "IX_MaintenanceScheduleInstances_ProcedureTemplateId" ON "MaintenanceScheduleInstances" ("ProcedureTemplateId");

CREATE INDEX "IX_MaintenanceScheduleInstances_Status" ON "MaintenanceScheduleInstances" ("Status");

CREATE INDEX "IX_MaintenanceSchedules_DefaultTechnicianUserId" ON "MaintenanceSchedules" ("DefaultTechnicianUserId");

CREATE INDEX "IX_MaintenanceSchedules_IsActive" ON "MaintenanceSchedules" ("IsActive");

CREATE INDEX "IX_MaintenanceSchedules_MachineComponentId" ON "MaintenanceSchedules" ("MachineComponentId");

CREATE INDEX "IX_MaintenanceSchedules_MachineId" ON "MaintenanceSchedules" ("MachineId");

CREATE INDEX "IX_MaintenanceSchedules_MachineId1" ON "MaintenanceSchedules" ("MachineId1");

CREATE INDEX "IX_MaintenanceSchedules_ScheduleType" ON "MaintenanceSchedules" ("ScheduleType");

CREATE INDEX "IX_MaintenanceServiceData_MaintenanceServiceId" ON "MaintenanceServiceData" ("MaintenanceServiceId");

CREATE INDEX "IX_MaintenanceServiceData_MaintenanceServiceId1" ON "MaintenanceServiceData" ("MaintenanceServiceId1");

CREATE INDEX "IX_MaintenanceServiceData_Timestamp" ON "MaintenanceServiceData" ("Timestamp");

CREATE INDEX "IX_MaintenanceServices_IsEnabled" ON "MaintenanceServices" ("IsEnabled");

CREATE INDEX "IX_MaintenanceServices_MachineComponentId" ON "MaintenanceServices" ("MachineComponentId");

CREATE INDEX "IX_MaintenanceServices_MachineId" ON "MaintenanceServices" ("MachineId");

CREATE INDEX "IX_MaintenanceServices_MachineId1" ON "MaintenanceServices" ("MachineId1");

CREATE INDEX "IX_MaintenanceServices_ServiceType" ON "MaintenanceServices" ("ServiceType");

CREATE INDEX "IX_MaintenanceStates_IsDue" ON "MaintenanceStates" ("IsDue");

CREATE INDEX "IX_MaintenanceStates_IsOverdue" ON "MaintenanceStates" ("IsOverdue");

CREATE INDEX "IX_MaintenanceStates_MachineId" ON "MaintenanceStates" ("MachineId");

CREATE UNIQUE INDEX "IX_MaintenanceStates_MachineId_RuleId" ON "MaintenanceStates" ("MachineId", "RuleId");

CREATE INDEX "IX_MaintenanceStates_RuleId" ON "MaintenanceStates" ("RuleId");

CREATE INDEX "IX_MaintenanceWorkOrders_AssignedTechnicianUserId" ON "MaintenanceWorkOrders" ("AssignedTechnicianUserId");

CREATE INDEX "IX_MaintenanceWorkOrders_CreatedAt" ON "MaintenanceWorkOrders" ("CreatedAt");

CREATE INDEX "IX_MaintenanceWorkOrders_MachineComponentId" ON "MaintenanceWorkOrders" ("MachineComponentId");

CREATE INDEX "IX_MaintenanceWorkOrders_MachineId" ON "MaintenanceWorkOrders" ("MachineId");

CREATE INDEX "IX_MaintenanceWorkOrders_MachineId1" ON "MaintenanceWorkOrders" ("MachineId1");

CREATE INDEX "IX_MaintenanceWorkOrders_MaintenanceRuleId" ON "MaintenanceWorkOrders" ("MaintenanceRuleId");

CREATE INDEX "IX_MaintenanceWorkOrders_Priority" ON "MaintenanceWorkOrders" ("Priority");

CREATE INDEX "IX_MaintenanceWorkOrders_Status" ON "MaintenanceWorkOrders" ("Status");

CREATE UNIQUE INDEX "IX_MaintenanceWorkOrders_WorkOrderNumber" ON "MaintenanceWorkOrders" ("WorkOrderNumber");

CREATE INDEX "IX_OperationalTasks_AssetId" ON "OperationalTasks" ("AssetId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260112151554_AddCrmV1', '8.0.11');

