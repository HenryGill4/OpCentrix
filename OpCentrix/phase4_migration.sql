CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;

CREATE TABLE "Jobs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Jobs" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "PartId" INTEGER NOT NULL,
    "ScheduledStart" TEXT NOT NULL,
    "ScheduledEnd" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "Notes" TEXT NULL,
    "Operator" TEXT NULL,
    "Quantity" INTEGER NOT NULL,
    CONSTRAINT "FK_Jobs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Parts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Parts" PRIMARY KEY AUTOINCREMENT,
    "PartNumber" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Material" TEXT NOT NULL,
    "AvgDuration" TEXT NOT NULL,
    "AvgDurationDays" INTEGER NOT NULL DEFAULT 1
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250721164727_InitialCreate', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

CREATE TABLE "JobLogEntries" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_JobLogEntries" PRIMARY KEY AUTOINCREMENT,
    "Timestamp" TEXT NOT NULL,
    "MachineId" TEXT NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "Action" TEXT NOT NULL,
    "Notes" TEXT NULL,
    "Operator" TEXT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250722031833_AddJobLogEntry', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

ALTER TABLE "Parts" ADD "AverageActualHours" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "AverageDefectRate" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "AverageEfficiencyPercent" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "AverageQualityScore" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "ChangeoverTimeMinutes" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "ConsumableMaterials" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "CreatedBy" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now'));

ALTER TABLE "Parts" ADD "CustomerPartNumber" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "Dimensions" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "EstimatedHours" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "IsActive" INTEGER NOT NULL DEFAULT 1;

ALTER TABLE "Parts" ADD "LastModifiedBy" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now'));

ALTER TABLE "Parts" ADD "LastProduced" TEXT NULL;

ALTER TABLE "Parts" ADD "MaterialCostPerUnit" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Parts" ADD "PartCategory" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "PartClass" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "PreferredMachines" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "ProcessParameters" TEXT NOT NULL DEFAULT '{}';

ALTER TABLE "Parts" ADD "ProcessType" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "QualityCheckpoints" TEXT NOT NULL DEFAULT '{}';

ALTER TABLE "Parts" ADD "QualityStandards" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "RequiredCertifications" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "RequiredMachineType" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "RequiredSkills" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "RequiredTooling" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "RequiresInspection" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "SetupCost" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Parts" ADD "SetupTimeMinutes" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "StandardLaborCostPerHour" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Parts" ADD "ToleranceRequirements" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "ToolingCost" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Parts" ADD "TotalJobsCompleted" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "TotalUnitsProduced" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "VolumeM3" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "WeightKg" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "ActualEnd" TEXT NULL;

ALTER TABLE "Jobs" ADD "ActualStart" TEXT NULL;

ALTER TABLE "Jobs" ADD "ChangeoverTimeMinutes" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "CreatedBy" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now'));

ALTER TABLE "Jobs" ADD "CustomerDueDate" TEXT NULL;

ALTER TABLE "Jobs" ADD "CustomerOrderNumber" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "DefectQuantity" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Jobs" ADD "EnergyConsumptionKwh" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "EstimatedHours" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "HoldReason" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "IsRushJob" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Jobs" ADD "LaborCostPerHour" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Jobs" ADD "LastModifiedBy" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now'));

ALTER TABLE "Jobs" ADD "MachineUtilizationPercent" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "MaterialCostPerUnit" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Jobs" ADD "OverheadCostPerHour" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Jobs" ADD "PreviousJobPartNumber" TEXT NULL;

ALTER TABLE "Jobs" ADD "Priority" INTEGER NOT NULL DEFAULT 3;

ALTER TABLE "Jobs" ADD "ProcessParameters" TEXT NOT NULL DEFAULT '{}';

ALTER TABLE "Jobs" ADD "ProducedQuantity" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Jobs" ADD "QualityCheckpoints" TEXT NOT NULL DEFAULT '{}';

ALTER TABLE "Jobs" ADD "QualityInspector" TEXT NULL;

ALTER TABLE "Jobs" ADD "RequiredMaterials" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "RequiredSkills" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "RequiredTooling" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "ReworkQuantity" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Jobs" ADD "SetupTimeMinutes" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "SpecialInstructions" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "Supervisor" TEXT NULL;

CREATE INDEX "IX_Parts_IsActive" ON "Parts" ("IsActive");

CREATE INDEX "IX_Parts_Material" ON "Parts" ("Material");

CREATE UNIQUE INDEX "IX_Parts_PartNumber" ON "Parts" ("PartNumber");

CREATE INDEX "IX_Jobs_MachineId" ON "Jobs" ("MachineId");

CREATE INDEX "IX_Jobs_MachineId_ScheduledStart" ON "Jobs" ("MachineId", "ScheduledStart");

CREATE INDEX "IX_Jobs_PartNumber" ON "Jobs" ("PartNumber");

CREATE INDEX "IX_Jobs_ScheduledStart" ON "Jobs" ("ScheduledStart");

CREATE INDEX "IX_Jobs_Status" ON "Jobs" ("Status");

CREATE INDEX "IX_JobLogEntries_Action" ON "JobLogEntries" ("Action");

CREATE INDEX "IX_JobLogEntries_MachineId" ON "JobLogEntries" ("MachineId");

CREATE INDEX "IX_JobLogEntries_Timestamp" ON "JobLogEntries" ("Timestamp");

CREATE TABLE "ef_temp_Jobs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Jobs" PRIMARY KEY AUTOINCREMENT,
    "ActualEnd" TEXT NULL,
    "ActualStart" TEXT NULL,
    "ChangeoverTimeMinutes" REAL NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CustomerDueDate" TEXT NULL,
    "CustomerOrderNumber" TEXT NOT NULL,
    "DefectQuantity" INTEGER NOT NULL,
    "EnergyConsumptionKwh" REAL NOT NULL,
    "EstimatedHours" REAL NOT NULL,
    "HoldReason" TEXT NOT NULL,
    "IsRushJob" INTEGER NOT NULL,
    "LaborCostPerHour" decimal(10,2) NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "MachineId" TEXT NOT NULL,
    "MachineUtilizationPercent" REAL NOT NULL,
    "MaterialCostPerUnit" decimal(10,2) NOT NULL,
    "Notes" TEXT NULL,
    "Operator" TEXT NULL,
    "OverheadCostPerHour" decimal(10,2) NOT NULL,
    "PartId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "PreviousJobPartNumber" TEXT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "ProcessParameters" TEXT NOT NULL DEFAULT '{}',
    "ProducedQuantity" INTEGER NOT NULL,
    "QualityCheckpoints" TEXT NOT NULL DEFAULT '{}',
    "QualityInspector" TEXT NULL,
    "Quantity" INTEGER NOT NULL,
    "RequiredMaterials" TEXT NOT NULL,
    "RequiredSkills" TEXT NOT NULL,
    "RequiredTooling" TEXT NOT NULL,
    "ReworkQuantity" INTEGER NOT NULL,
    "ScheduledEnd" TEXT NOT NULL,
    "ScheduledStart" TEXT NOT NULL,
    "SetupTimeMinutes" REAL NOT NULL,
    "SpecialInstructions" TEXT NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Scheduled',
    "Supervisor" TEXT NULL,
    CONSTRAINT "FK_Jobs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_Jobs" ("Id", "ActualEnd", "ActualStart", "ChangeoverTimeMinutes", "CreatedBy", "CreatedDate", "CustomerDueDate", "CustomerOrderNumber", "DefectQuantity", "EnergyConsumptionKwh", "EstimatedHours", "HoldReason", "IsRushJob", "LaborCostPerHour", "LastModifiedBy", "LastModifiedDate", "MachineId", "MachineUtilizationPercent", "MaterialCostPerUnit", "Notes", "Operator", "OverheadCostPerHour", "PartId", "PartNumber", "PreviousJobPartNumber", "Priority", "ProcessParameters", "ProducedQuantity", "QualityCheckpoints", "QualityInspector", "Quantity", "RequiredMaterials", "RequiredSkills", "RequiredTooling", "ReworkQuantity", "ScheduledEnd", "ScheduledStart", "SetupTimeMinutes", "SpecialInstructions", "Status", "Supervisor")
SELECT "Id", "ActualEnd", "ActualStart", "ChangeoverTimeMinutes", "CreatedBy", "CreatedDate", "CustomerDueDate", "CustomerOrderNumber", "DefectQuantity", "EnergyConsumptionKwh", "EstimatedHours", "HoldReason", "IsRushJob", "LaborCostPerHour", "LastModifiedBy", "LastModifiedDate", "MachineId", "MachineUtilizationPercent", "MaterialCostPerUnit", "Notes", "Operator", "OverheadCostPerHour", "PartId", "PartNumber", "PreviousJobPartNumber", "Priority", "ProcessParameters", "ProducedQuantity", "QualityCheckpoints", "QualityInspector", "Quantity", "RequiredMaterials", "RequiredSkills", "RequiredTooling", "ReworkQuantity", "ScheduledEnd", "ScheduledStart", "SetupTimeMinutes", "SpecialInstructions", "Status", "Supervisor"
FROM "Jobs";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "Jobs";

ALTER TABLE "ef_temp_Jobs" RENAME TO "Jobs";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

CREATE INDEX "IX_Jobs_MachineId" ON "Jobs" ("MachineId");

CREATE INDEX "IX_Jobs_MachineId_ScheduledStart" ON "Jobs" ("MachineId", "ScheduledStart");

CREATE INDEX "IX_Jobs_PartId" ON "Jobs" ("PartId");

CREATE INDEX "IX_Jobs_PartNumber" ON "Jobs" ("PartNumber");

CREATE INDEX "IX_Jobs_ScheduledStart" ON "Jobs" ("ScheduledStart");

CREATE INDEX "IX_Jobs_Status" ON "Jobs" ("Status");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250722211248_EnhancedJobAnalytics', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

ALTER TABLE "Parts" RENAME COLUMN "WeightKg" TO "WidthMm";

ALTER TABLE "Parts" RENAME COLUMN "VolumeM3" TO "WeightGrams";

ALTER TABLE "Parts" RENAME COLUMN "ToolingCost" TO "StandardSellingPrice";

ALTER TABLE "Parts" RENAME COLUMN "MaterialCostPerUnit" TO "QualityInspectionCost";

ALTER TABLE "Parts" RENAME COLUMN "ChangeoverTimeMinutes" TO "VolumeMm3";

ALTER TABLE "Parts" ADD "Application" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "ArgonCostPerHour" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Parts" ADD "AverageCostPerUnit" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Parts" ADD "AveragePowderUtilization" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "BuildFileTemplate" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "CadFilePath" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "CadFileVersion" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "CoolingTimeMinutes" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "HeightMm" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "Industry" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "LengthMm" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "MachineOperatingCostPerHour" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Parts" ADD "MaterialCostPerKg" decimal(12,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Parts" ADD "MaxOxygenContent" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "MaxSurfaceRoughnessRa" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "PostProcessingCost" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Parts" ADD "PostProcessingTimeMinutes" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "PowderChangeoverTimeMinutes" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "PowderRequirementKg" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "PowderSpecification" TEXT NOT NULL DEFAULT '15-45 μm particle size';

ALTER TABLE "Parts" ADD "PreheatingTimeMinutes" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "RecommendedBuildTemperature" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "RecommendedHatchSpacing" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "RecommendedLaserPower" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "RecommendedLayerThickness" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "RecommendedScanSpeed" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "RequiredArgonPurity" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "RequiresAS9100" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresCertification" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresFDA" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresNADCAP" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresSupports" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "SlsMaterial" TEXT NOT NULL DEFAULT 'Ti-6Al-4V Grade 5';

ALTER TABLE "Parts" ADD "SupportRemovalTimeMinutes" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Parts" ADD "SupportStrategy" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "SurfaceFinishRequirement" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "ActualPowderUsageKg" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "ArgonCostPerHour" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Jobs" ADD "ArgonPurityPercent" REAL NOT NULL DEFAULT 99.900000000000006;

ALTER TABLE "Jobs" ADD "BuildFileCreatedDate" TEXT NULL;

ALTER TABLE "Jobs" ADD "BuildFileName" TEXT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "BuildFilePath" TEXT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "BuildFileSizeBytes" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Jobs" ADD "BuildLayerNumber" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Jobs" ADD "BuildPlatformId" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "BuildTemperatureCelsius" REAL NOT NULL DEFAULT 180.0;

ALTER TABLE "Jobs" ADD "BuildTimeMinutes" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "CoolingTimeMinutes" REAL NOT NULL DEFAULT 240.0;

ALTER TABLE "Jobs" ADD "CurrentArgonFlowRate" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "CurrentBuildTemperature" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "CurrentLaserPowerWatts" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "CurrentOxygenLevel" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "DensityPercentage" REAL NOT NULL DEFAULT 99.5;

ALTER TABLE "Jobs" ADD "EstimatedPowderUsageKg" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "HatchSpacingMicrons" REAL NOT NULL DEFAULT 120.0;

ALTER TABLE "Jobs" ADD "LaserPowerWatts" REAL NOT NULL DEFAULT 200.0;

ALTER TABLE "Jobs" ADD "LayerThicknessMicrons" REAL NOT NULL DEFAULT 30.0;

ALTER TABLE "Jobs" ADD "MachineOperatingCostPerHour" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Jobs" ADD "MaterialCostPerKg" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Jobs" ADD "OpcUaBuildProgress" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "OpcUaErrorMessages" TEXT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "OpcUaJobId" TEXT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "OpcUaLastUpdate" TEXT NULL;

ALTER TABLE "Jobs" ADD "OpcUaStatus" TEXT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "OxygenContentPpm" REAL NOT NULL DEFAULT 50.0;

ALTER TABLE "Jobs" ADD "PostProcessingTimeMinutes" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "PowderChangeoverTimeMinutes" REAL NOT NULL DEFAULT 30.0;

ALTER TABLE "Jobs" ADD "PowderExpirationDate" TEXT NULL;

ALTER TABLE "Jobs" ADD "PowderLotNumber" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Jobs" ADD "PowderRecyclePercentage" REAL NOT NULL DEFAULT 85.0;

ALTER TABLE "Jobs" ADD "PowerCostPerKwh" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Jobs" ADD "PreheatingTimeMinutes" REAL NOT NULL DEFAULT 60.0;

ALTER TABLE "Jobs" ADD "RequiresArgonPurge" INTEGER NOT NULL DEFAULT 1;

ALTER TABLE "Jobs" ADD "RequiresPostProcessing" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Jobs" ADD "RequiresPowderSieving" INTEGER NOT NULL DEFAULT 1;

ALTER TABLE "Jobs" ADD "RequiresPreheating" INTEGER NOT NULL DEFAULT 1;

ALTER TABLE "Jobs" ADD "ScanSpeedMmPerSec" REAL NOT NULL DEFAULT 1200.0;

ALTER TABLE "Jobs" ADD "SlsMaterial" TEXT NOT NULL DEFAULT 'Ti-6Al-4V Grade 5';

ALTER TABLE "Jobs" ADD "SurfaceRoughnessRa" REAL NOT NULL DEFAULT 0.0;

ALTER TABLE "Jobs" ADD "UltimateTensileStrengthMPa" REAL NOT NULL DEFAULT 0.0;

CREATE TABLE "AdminAlerts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AdminAlerts" PRIMARY KEY AUTOINCREMENT,
    "AlertName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Category" TEXT NOT NULL DEFAULT 'System',
    "TriggerType" TEXT NOT NULL,
    "TriggerConditions" TEXT NOT NULL DEFAULT '{}',
    "SeverityLevel" INTEGER NOT NULL DEFAULT 3,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "EmailRecipients" TEXT NOT NULL,
    "EmailSubject" TEXT NOT NULL DEFAULT 'OpCentrix Alert: {AlertName}',
    "EmailTemplate" TEXT NOT NULL,
    "SendSms" INTEGER NOT NULL DEFAULT 0,
    "SmsRecipients" TEXT NOT NULL,
    "SmsTemplate" TEXT NOT NULL,
    "SendBrowserNotification" INTEGER NOT NULL DEFAULT 1,
    "CooldownMinutes" INTEGER NOT NULL DEFAULT 15,
    "LastTriggered" TEXT NULL,
    "TriggerCount" INTEGER NOT NULL DEFAULT 0,
    "EscalationRules" TEXT NOT NULL DEFAULT '{}',
    "BusinessHoursOnly" INTEGER NOT NULL DEFAULT 0,
    "BusinessHoursStart" TEXT NOT NULL,
    "BusinessHoursEnd" TEXT NOT NULL,
    "BusinessDays" TEXT NOT NULL DEFAULT '1,2,3,4,5',
    "MaxAlertsPerDay" INTEGER NOT NULL DEFAULT 10,
    "TriggersToday" INTEGER NOT NULL DEFAULT 0,
    "LastDailyReset" TEXT NOT NULL DEFAULT (date('now')),
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

CREATE TABLE "ArchivedJobs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ArchivedJobs" PRIMARY KEY AUTOINCREMENT,
    "OriginalJobId" INTEGER NOT NULL,
    "MachineId" TEXT NOT NULL,
    "PartId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "PartDescription" TEXT NOT NULL,
    "ScheduledStart" TEXT NOT NULL,
    "ScheduledEnd" TEXT NOT NULL,
    "ActualStart" TEXT NULL,
    "ActualEnd" TEXT NULL,
    "Quantity" INTEGER NOT NULL DEFAULT 1,
    "ProducedQuantity" INTEGER NOT NULL,
    "DefectQuantity" INTEGER NOT NULL,
    "ReworkQuantity" INTEGER NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Completed',
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "EstimatedHours" REAL NOT NULL,
    "SlsMaterial" TEXT NOT NULL,
    "PowderLotNumber" TEXT NOT NULL,
    "LaserPowerWatts" REAL NOT NULL,
    "ScanSpeedMmPerSec" REAL NOT NULL,
    "LayerThicknessMicrons" REAL NOT NULL,
    "EstimatedPowderUsageKg" REAL NOT NULL,
    "ActualPowderUsageKg" REAL NOT NULL,
    "LaborCostPerHour" decimal(10,2) NOT NULL,
    "MaterialCostPerKg" decimal(10,2) NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "DensityPercentage" REAL NOT NULL,
    "SurfaceRoughnessRa" REAL NOT NULL,
    "Operator" TEXT NOT NULL,
    "QualityInspector" TEXT NOT NULL,
    "Supervisor" TEXT NOT NULL,
    "CustomerOrderNumber" TEXT NOT NULL,
    "CustomerDueDate" TEXT NULL,
    "Notes" TEXT NOT NULL,
    "HoldReason" TEXT NOT NULL,
    "ProcessParameters" TEXT NOT NULL DEFAULT '{}',
    "QualityCheckpoints" TEXT NOT NULL DEFAULT '{}',
    "ArchivedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "ArchivedBy" TEXT NOT NULL DEFAULT 'System',
    "ArchiveReason" TEXT NOT NULL DEFAULT 'Cleanup',
    "OriginalCreatedDate" TEXT NOT NULL,
    "OriginalLastModifiedDate" TEXT NOT NULL,
    "OriginalCreatedBy" TEXT NOT NULL,
    "OriginalLastModifiedBy" TEXT NOT NULL
);

CREATE TABLE "DefectCategories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_DefectCategories" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Code" TEXT NOT NULL,
    "SeverityLevel" INTEGER NOT NULL DEFAULT 3,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "CategoryGroup" TEXT NOT NULL DEFAULT 'General',
    "ApplicableProcesses" TEXT NOT NULL,
    "StandardCorrectiveActions" TEXT NOT NULL,
    "PreventionMethods" TEXT NOT NULL,
    "RequiresImmediateNotification" INTEGER NOT NULL DEFAULT 0,
    "CostImpact" TEXT NOT NULL DEFAULT 'Medium',
    "AverageResolutionTimeMinutes" INTEGER NOT NULL DEFAULT 30,
    "SortOrder" INTEGER NOT NULL DEFAULT 100,
    "ColorCode" TEXT NOT NULL DEFAULT '#6B7280',
    "Icon" TEXT NOT NULL DEFAULT 'exclamation-triangle',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

CREATE TABLE "FeatureToggles" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_FeatureToggles" PRIMARY KEY AUTOINCREMENT,
    "FeatureName" TEXT NOT NULL,
    "DisplayName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "IsEnabled" INTEGER NOT NULL DEFAULT 0,
    "Category" TEXT NOT NULL DEFAULT 'General',
    "Environment" TEXT NOT NULL DEFAULT 'All',
    "RequiredRole" TEXT NOT NULL DEFAULT 'User',
    "RolloutPercentage" INTEGER NOT NULL DEFAULT 100,
    "StartDate" TEXT NULL,
    "EndDate" TEXT NULL,
    "RequiresRestart" INTEGER NOT NULL DEFAULT 0,
    "Dependencies" TEXT NOT NULL,
    "Conflicts" TEXT NOT NULL,
    "Configuration" TEXT NOT NULL DEFAULT '{}',
    "UsageCount" INTEGER NOT NULL DEFAULT 0,
    "LastUsed" TEXT NULL,
    "PerformanceNotes" TEXT NOT NULL,
    "SecurityNotes" TEXT NOT NULL,
    "IntroducedInVersion" TEXT NOT NULL,
    "PlannedRemovalVersion" TEXT NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Experimental',
    "SortOrder" INTEGER NOT NULL DEFAULT 100,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

CREATE TABLE "OperatingShifts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_OperatingShifts" PRIMARY KEY AUTOINCREMENT,
    "DayOfWeek" INTEGER NOT NULL,
    "StartTime" TEXT NOT NULL,
    "EndTime" TEXT NOT NULL,
    "IsHoliday" INTEGER NOT NULL DEFAULT 0,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "Description" TEXT NOT NULL,
    "SpecificDate" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

CREATE TABLE "RolePermissions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_RolePermissions" PRIMARY KEY AUTOINCREMENT,
    "RoleName" TEXT NOT NULL,
    "PermissionKey" TEXT NOT NULL,
    "HasPermission" INTEGER NOT NULL DEFAULT 0,
    "PermissionLevel" TEXT NOT NULL DEFAULT 'Read',
    "Category" TEXT NOT NULL DEFAULT 'General',
    "Description" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "Priority" INTEGER NOT NULL DEFAULT 100,
    "Constraints" TEXT NOT NULL DEFAULT '{}',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

CREATE TABLE "SlsMachines" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SlsMachines" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NOT NULL,
    "MachineName" TEXT NOT NULL,
    "MachineModel" TEXT NOT NULL DEFAULT 'TruPrint 3000',
    "SerialNumber" TEXT NOT NULL,
    "Location" TEXT NOT NULL,
    "BuildLengthMm" REAL NOT NULL DEFAULT 250.0,
    "BuildWidthMm" REAL NOT NULL DEFAULT 250.0,
    "BuildHeightMm" REAL NOT NULL DEFAULT 300.0,
    "SupportedMaterials" TEXT NOT NULL DEFAULT 'Ti-6Al-4V Grade 5,Ti-6Al-4V ELI Grade 23',
    "CurrentMaterial" TEXT NOT NULL,
    "MaxLaserPowerWatts" REAL NOT NULL DEFAULT 400.0,
    "MaxScanSpeedMmPerSec" REAL NOT NULL DEFAULT 7000.0,
    "MinLayerThicknessMicrons" REAL NOT NULL DEFAULT 20.0,
    "MaxLayerThicknessMicrons" REAL NOT NULL DEFAULT 60.0,
    "OpcUaEndpointUrl" TEXT NOT NULL,
    "OpcUaUsername" TEXT NOT NULL,
    "OpcUaPasswordHash" TEXT NOT NULL,
    "OpcUaNamespace" TEXT NOT NULL,
    "OpcUaEnabled" INTEGER NOT NULL,
    "OpcUaLastConnection" TEXT NULL,
    "OpcUaConnectionStatus" TEXT NOT NULL DEFAULT 'Disconnected',
    "Status" TEXT NOT NULL DEFAULT 'Offline',
    "StatusMessage" TEXT NOT NULL,
    "LastStatusUpdate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CurrentJobId" INTEGER NULL,
    "CurrentBuildProgress" REAL NOT NULL,
    "CurrentJobStartTime" TEXT NULL,
    "EstimatedCompletionTime" TEXT NULL,
    "CurrentBuildTemperature" REAL NOT NULL,
    "TargetBuildTemperature" REAL NOT NULL,
    "AmbientTemperature" REAL NOT NULL,
    "CurrentOxygenLevel" REAL NOT NULL,
    "ArgonFlowRate" REAL NOT NULL,
    "ArgonPressure" REAL NOT NULL,
    "CurrentLaserPower" REAL NOT NULL,
    "LaserOnTime" REAL NOT NULL,
    "LaserStatus" INTEGER NOT NULL,
    "PowderLevelPercent" REAL NOT NULL,
    "PowderRemainingKg" REAL NOT NULL,
    "LastPowderRefill" TEXT NULL,
    "CurrentBuildHeight" REAL NOT NULL,
    "TotalLayersCompleted" INTEGER NOT NULL,
    "TotalLayersPlanned" INTEGER NOT NULL,
    "TotalOperatingHours" REAL NOT NULL,
    "HoursSinceLastMaintenance" REAL NOT NULL,
    "MaintenanceIntervalHours" REAL NOT NULL DEFAULT 500.0,
    "LastMaintenanceDate" TEXT NULL,
    "NextMaintenanceDate" TEXT NULL,
    "TotalJobsCompleted" INTEGER NOT NULL,
    "TotalPartsPrinted" INTEGER NOT NULL,
    "AverageUtilizationPercent" REAL NOT NULL,
    "QualityScorePercent" REAL NOT NULL DEFAULT 100.0,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "IsAvailableForScheduling" INTEGER NOT NULL DEFAULT 1,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "MaintenanceNotes" TEXT NOT NULL,
    "OperatorNotes" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    CONSTRAINT "FK_SlsMachines_Jobs_CurrentJobId" FOREIGN KEY ("CurrentJobId") REFERENCES "Jobs" ("Id") ON DELETE SET NULL
);

CREATE TABLE "SystemSettings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SystemSettings" PRIMARY KEY AUTOINCREMENT,
    "SettingKey" TEXT NOT NULL,
    "SettingValue" TEXT NOT NULL,
    "DataType" TEXT NOT NULL DEFAULT 'String',
    "Category" TEXT NOT NULL DEFAULT 'General',
    "Description" TEXT NOT NULL,
    "DefaultValue" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "IsReadOnly" INTEGER NOT NULL DEFAULT 0,
    "RequiresRestart" INTEGER NOT NULL DEFAULT 0,
    "ValidationRules" TEXT NOT NULL,
    "DisplayOrder" INTEGER NOT NULL DEFAULT 100,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

CREATE TABLE "Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "Username" TEXT NOT NULL,
    "FullName" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Role" TEXT NOT NULL,
    "Department" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastLoginDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL
);

CREATE TABLE "InspectionCheckpoints" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_InspectionCheckpoints" PRIMARY KEY AUTOINCREMENT,
    "PartId" INTEGER NOT NULL,
    "CheckpointName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "InspectionType" TEXT NOT NULL DEFAULT 'Visual',
    "SortOrder" INTEGER NOT NULL DEFAULT 100,
    "IsRequired" INTEGER NOT NULL DEFAULT 1,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "EstimatedMinutes" INTEGER NOT NULL DEFAULT 5,
    "AcceptanceCriteria" TEXT NOT NULL,
    "MeasurementMethod" TEXT NOT NULL,
    "RequiredEquipment" TEXT NOT NULL,
    "RequiredSkills" TEXT NOT NULL,
    "ReferenceDocuments" TEXT NOT NULL,
    "TargetValue" REAL NULL,
    "UpperTolerance" REAL NULL,
    "LowerTolerance" REAL NULL,
    "Unit" TEXT NOT NULL,
    "FailureAction" TEXT NOT NULL DEFAULT 'Hold for review',
    "SampleSize" INTEGER NOT NULL DEFAULT 1,
    "SamplingMethod" TEXT NOT NULL DEFAULT 'All',
    "Category" TEXT NOT NULL DEFAULT 'Quality',
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "Notes" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "DefectCategoryId" INTEGER NULL,
    CONSTRAINT "FK_InspectionCheckpoints_DefectCategories_DefectCategoryId" FOREIGN KEY ("DefectCategoryId") REFERENCES "DefectCategories" ("Id"),
    CONSTRAINT "FK_InspectionCheckpoints_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "MachineCapabilities" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MachineCapabilities" PRIMARY KEY AUTOINCREMENT,
    "SlsMachineId" INTEGER NOT NULL,
    "CapabilityType" TEXT NOT NULL,
    "CapabilityName" TEXT NOT NULL,
    "CapabilityValue" TEXT NOT NULL,
    "IsAvailable" INTEGER NOT NULL DEFAULT 1,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "MinValue" REAL NULL,
    "MaxValue" REAL NULL,
    "Unit" TEXT NOT NULL,
    "Notes" TEXT NOT NULL,
    "RequiredCertification" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    CONSTRAINT "FK_MachineCapabilities_SlsMachines_SlsMachineId" FOREIGN KEY ("SlsMachineId") REFERENCES "SlsMachines" ("Id") ON DELETE CASCADE
);

CREATE TABLE "MachineDataSnapshot" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MachineDataSnapshot" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NOT NULL,
    "SlsMachineId" INTEGER NOT NULL,
    "Timestamp" TEXT NOT NULL DEFAULT (datetime('now')),
    "ProcessDataJson" TEXT NOT NULL DEFAULT '{}',
    "QualityDataJson" TEXT NOT NULL DEFAULT '{}',
    "AlarmDataJson" TEXT NOT NULL DEFAULT '{}',
    "UtilizationPercent" REAL NOT NULL,
    "EnergyConsumptionKwh" REAL NOT NULL,
    "PowderConsumedKg" REAL NOT NULL,
    "ArgonConsumedM3" REAL NOT NULL,
    CONSTRAINT "FK_MachineDataSnapshot_SlsMachines_SlsMachineId" FOREIGN KEY ("SlsMachineId") REFERENCES "SlsMachines" ("Id") ON DELETE CASCADE
);

CREATE TABLE "BuildJobs" (
    "BuildId" INTEGER NOT NULL CONSTRAINT "PK_BuildJobs" PRIMARY KEY AUTOINCREMENT,
    "PrinterName" TEXT NOT NULL,
    "ActualStartTime" TEXT NOT NULL,
    "ActualEndTime" TEXT NULL,
    "ScheduledStartTime" TEXT NULL,
    "ScheduledEndTime" TEXT NULL,
    "Status" TEXT NOT NULL DEFAULT 'In Progress',
    "UserId" INTEGER NOT NULL,
    "Notes" TEXT NULL,
    "LaserRunTime" TEXT NULL,
    "GasUsed_L" REAL NULL,
    "PowderUsed_L" REAL NULL,
    "ReasonForEnd" TEXT NULL,
    "SetupNotes" TEXT NULL,
    "AssociatedScheduledJobId" INTEGER NULL,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "CompletedAt" TEXT NULL,
    CONSTRAINT "FK_BuildJobs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "UserSettings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_UserSettings" PRIMARY KEY AUTOINCREMENT,
    "UserId" INTEGER NOT NULL,
    "SessionTimeoutMinutes" INTEGER NOT NULL DEFAULT 120,
    "Theme" TEXT NOT NULL DEFAULT 'Light',
    "EmailNotifications" INTEGER NOT NULL DEFAULT 1,
    "BrowserNotifications" INTEGER NOT NULL DEFAULT 1,
    "DefaultPage" TEXT NOT NULL DEFAULT '/Scheduler',
    "ItemsPerPage" INTEGER NOT NULL DEFAULT 20,
    "TimeZone" TEXT NOT NULL DEFAULT 'UTC',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_UserSettings_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "BuildJobParts" (
    "PartEntryId" INTEGER NOT NULL CONSTRAINT "PK_BuildJobParts" PRIMARY KEY AUTOINCREMENT,
    "BuildId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "IsPrimary" INTEGER NOT NULL DEFAULT 0,
    "Description" TEXT NULL,
    "Material" TEXT NULL,
    "EstimatedHours" REAL NOT NULL,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    CONSTRAINT "FK_BuildJobParts_BuildJobs_BuildId" FOREIGN KEY ("BuildId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
);

CREATE TABLE "DelayLogs" (
    "DelayId" INTEGER NOT NULL CONSTRAINT "PK_DelayLogs" PRIMARY KEY AUTOINCREMENT,
    "BuildId" INTEGER NOT NULL,
    "DelayReason" TEXT NOT NULL,
    "DelayDuration" INTEGER NOT NULL,
    "Description" TEXT NULL,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    CONSTRAINT "FK_DelayLogs_BuildJobs_BuildId" FOREIGN KEY ("BuildId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
);

CREATE INDEX "IX_Parts_Industry" ON "Parts" ("Industry");

CREATE INDEX "IX_Parts_PartCategory" ON "Parts" ("PartCategory");

CREATE INDEX "IX_Parts_PartClass" ON "Parts" ("PartClass");

CREATE INDEX "IX_Parts_ProcessType" ON "Parts" ("ProcessType");

CREATE INDEX "IX_Parts_RequiredMachineType" ON "Parts" ("RequiredMachineType");

CREATE INDEX "IX_Parts_SlsMaterial" ON "Parts" ("SlsMaterial");

CREATE INDEX "IX_Jobs_CustomerOrderNumber" ON "Jobs" ("CustomerOrderNumber");

CREATE INDEX "IX_Jobs_OpcUaJobId" ON "Jobs" ("OpcUaJobId");

CREATE INDEX "IX_Jobs_Priority" ON "Jobs" ("Priority");

CREATE INDEX "IX_Jobs_SlsMaterial" ON "Jobs" ("SlsMaterial");

CREATE INDEX "IX_AdminAlerts_AlertName" ON "AdminAlerts" ("AlertName");

CREATE INDEX "IX_AdminAlerts_Category" ON "AdminAlerts" ("Category");

CREATE INDEX "IX_AdminAlerts_IsActive" ON "AdminAlerts" ("IsActive");

CREATE INDEX "IX_AdminAlerts_LastTriggered" ON "AdminAlerts" ("LastTriggered");

CREATE INDEX "IX_AdminAlerts_SeverityLevel" ON "AdminAlerts" ("SeverityLevel");

CREATE INDEX "IX_AdminAlerts_TriggerType" ON "AdminAlerts" ("TriggerType");

CREATE INDEX "IX_ArchivedJobs_ArchivedBy" ON "ArchivedJobs" ("ArchivedBy");

CREATE INDEX "IX_ArchivedJobs_ArchivedDate" ON "ArchivedJobs" ("ArchivedDate");

CREATE INDEX "IX_ArchivedJobs_MachineId" ON "ArchivedJobs" ("MachineId");

CREATE INDEX "IX_ArchivedJobs_MachineId_ScheduledStart" ON "ArchivedJobs" ("MachineId", "ScheduledStart");

CREATE INDEX "IX_ArchivedJobs_OriginalJobId" ON "ArchivedJobs" ("OriginalJobId");

CREATE INDEX "IX_ArchivedJobs_PartNumber" ON "ArchivedJobs" ("PartNumber");

CREATE INDEX "IX_ArchivedJobs_Status" ON "ArchivedJobs" ("Status");

CREATE INDEX "IX_BuildJobParts_BuildId" ON "BuildJobParts" ("BuildId");

CREATE INDEX "IX_BuildJobParts_BuildId_IsPrimary" ON "BuildJobParts" ("BuildId", "IsPrimary");

CREATE INDEX "IX_BuildJobParts_IsPrimary" ON "BuildJobParts" ("IsPrimary");

CREATE INDEX "IX_BuildJobParts_PartNumber" ON "BuildJobParts" ("PartNumber");

CREATE INDEX "IX_BuildJobs_ActualStartTime" ON "BuildJobs" ("ActualStartTime");

CREATE INDEX "IX_BuildJobs_AssociatedScheduledJobId" ON "BuildJobs" ("AssociatedScheduledJobId");

CREATE INDEX "IX_BuildJobs_PrinterName" ON "BuildJobs" ("PrinterName");

CREATE INDEX "IX_BuildJobs_PrinterName_ActualStartTime" ON "BuildJobs" ("PrinterName", "ActualStartTime");

CREATE INDEX "IX_BuildJobs_Status" ON "BuildJobs" ("Status");

CREATE INDEX "IX_BuildJobs_UserId" ON "BuildJobs" ("UserId");

CREATE INDEX "IX_DefectCategories_CategoryGroup" ON "DefectCategories" ("CategoryGroup");

CREATE INDEX "IX_DefectCategories_Code" ON "DefectCategories" ("Code");

CREATE INDEX "IX_DefectCategories_IsActive" ON "DefectCategories" ("IsActive");

CREATE INDEX "IX_DefectCategories_Name" ON "DefectCategories" ("Name");

CREATE INDEX "IX_DefectCategories_SeverityLevel" ON "DefectCategories" ("SeverityLevel");

CREATE INDEX "IX_DelayLogs_BuildId" ON "DelayLogs" ("BuildId");

CREATE INDEX "IX_DelayLogs_CreatedAt" ON "DelayLogs" ("CreatedAt");

CREATE INDEX "IX_DelayLogs_DelayDuration" ON "DelayLogs" ("DelayDuration");

CREATE INDEX "IX_DelayLogs_DelayReason" ON "DelayLogs" ("DelayReason");

CREATE INDEX "IX_FeatureToggles_Category" ON "FeatureToggles" ("Category");

CREATE INDEX "IX_FeatureToggles_Environment" ON "FeatureToggles" ("Environment");

CREATE UNIQUE INDEX "IX_FeatureToggles_FeatureName" ON "FeatureToggles" ("FeatureName");

CREATE INDEX "IX_FeatureToggles_IsEnabled" ON "FeatureToggles" ("IsEnabled");

CREATE INDEX "IX_FeatureToggles_RequiredRole" ON "FeatureToggles" ("RequiredRole");

CREATE INDEX "IX_FeatureToggles_Status" ON "FeatureToggles" ("Status");

CREATE INDEX "IX_InspectionCheckpoints_DefectCategoryId" ON "InspectionCheckpoints" ("DefectCategoryId");

CREATE INDEX "IX_InspectionCheckpoints_InspectionType" ON "InspectionCheckpoints" ("InspectionType");

CREATE INDEX "IX_InspectionCheckpoints_IsActive" ON "InspectionCheckpoints" ("IsActive");

CREATE INDEX "IX_InspectionCheckpoints_IsRequired" ON "InspectionCheckpoints" ("IsRequired");

CREATE INDEX "IX_InspectionCheckpoints_PartId" ON "InspectionCheckpoints" ("PartId");

CREATE INDEX "IX_InspectionCheckpoints_PartId_SortOrder" ON "InspectionCheckpoints" ("PartId", "SortOrder");

CREATE INDEX "IX_MachineCapabilities_CapabilityType" ON "MachineCapabilities" ("CapabilityType");

CREATE INDEX "IX_MachineCapabilities_IsAvailable" ON "MachineCapabilities" ("IsAvailable");

CREATE INDEX "IX_MachineCapabilities_SlsMachineId" ON "MachineCapabilities" ("SlsMachineId");

CREATE INDEX "IX_MachineCapabilities_SlsMachineId_CapabilityType" ON "MachineCapabilities" ("SlsMachineId", "CapabilityType");

CREATE INDEX "IX_MachineDataSnapshot_MachineId" ON "MachineDataSnapshot" ("MachineId");

CREATE INDEX "IX_MachineDataSnapshot_MachineId_Timestamp" ON "MachineDataSnapshot" ("MachineId", "Timestamp");

CREATE INDEX "IX_MachineDataSnapshot_SlsMachineId" ON "MachineDataSnapshot" ("SlsMachineId");

CREATE INDEX "IX_MachineDataSnapshot_Timestamp" ON "MachineDataSnapshot" ("Timestamp");

CREATE INDEX "IX_OperatingShifts_DayOfWeek" ON "OperatingShifts" ("DayOfWeek");

CREATE INDEX "IX_OperatingShifts_DayOfWeek_IsActive" ON "OperatingShifts" ("DayOfWeek", "IsActive");

CREATE INDEX "IX_OperatingShifts_IsActive" ON "OperatingShifts" ("IsActive");

CREATE INDEX "IX_OperatingShifts_IsHoliday" ON "OperatingShifts" ("IsHoliday");

CREATE INDEX "IX_OperatingShifts_SpecificDate" ON "OperatingShifts" ("SpecificDate");

CREATE INDEX "IX_RolePermissions_Category" ON "RolePermissions" ("Category");

CREATE INDEX "IX_RolePermissions_IsActive" ON "RolePermissions" ("IsActive");

CREATE INDEX "IX_RolePermissions_PermissionKey" ON "RolePermissions" ("PermissionKey");

CREATE INDEX "IX_RolePermissions_RoleName" ON "RolePermissions" ("RoleName");

CREATE UNIQUE INDEX "IX_RolePermissions_RoleName_PermissionKey" ON "RolePermissions" ("RoleName", "PermissionKey");

CREATE INDEX "IX_SlsMachines_CurrentJobId" ON "SlsMachines" ("CurrentJobId");

CREATE INDEX "IX_SlsMachines_CurrentMaterial" ON "SlsMachines" ("CurrentMaterial");

CREATE INDEX "IX_SlsMachines_IsActive" ON "SlsMachines" ("IsActive");

CREATE INDEX "IX_SlsMachines_IsAvailableForScheduling" ON "SlsMachines" ("IsAvailableForScheduling");

CREATE INDEX "IX_SlsMachines_LastStatusUpdate" ON "SlsMachines" ("LastStatusUpdate");

CREATE UNIQUE INDEX "IX_SlsMachines_MachineId" ON "SlsMachines" ("MachineId");

CREATE INDEX "IX_SlsMachines_Status" ON "SlsMachines" ("Status");

CREATE INDEX "IX_SystemSettings_Category" ON "SystemSettings" ("Category");

CREATE INDEX "IX_SystemSettings_Category_DisplayOrder" ON "SystemSettings" ("Category", "DisplayOrder");

CREATE INDEX "IX_SystemSettings_IsActive" ON "SystemSettings" ("IsActive");

CREATE UNIQUE INDEX "IX_SystemSettings_SettingKey" ON "SystemSettings" ("SettingKey");

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

CREATE INDEX "IX_Users_IsActive" ON "Users" ("IsActive");

CREATE INDEX "IX_Users_Role" ON "Users" ("Role");

CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");

CREATE UNIQUE INDEX "IX_UserSettings_UserId" ON "UserSettings" ("UserId");

CREATE TABLE "ef_temp_Jobs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Jobs" PRIMARY KEY AUTOINCREMENT,
    "ActualEnd" TEXT NULL,
    "ActualPowderUsageKg" REAL NOT NULL,
    "ActualStart" TEXT NULL,
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "ArgonPurityPercent" REAL NOT NULL DEFAULT 99.900000000000006,
    "BuildFileCreatedDate" TEXT NULL,
    "BuildFileName" TEXT NULL DEFAULT '',
    "BuildFilePath" TEXT NULL DEFAULT '',
    "BuildFileSizeBytes" INTEGER NOT NULL,
    "BuildLayerNumber" INTEGER NOT NULL,
    "BuildPlatformId" TEXT NOT NULL DEFAULT '',
    "BuildTemperatureCelsius" REAL NOT NULL DEFAULT 180.0,
    "BuildTimeMinutes" REAL NOT NULL,
    "ChangeoverTimeMinutes" REAL NOT NULL,
    "CoolingTimeMinutes" REAL NOT NULL DEFAULT 240.0,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CurrentArgonFlowRate" REAL NOT NULL,
    "CurrentBuildTemperature" REAL NOT NULL,
    "CurrentLaserPowerWatts" REAL NOT NULL,
    "CurrentOxygenLevel" REAL NOT NULL,
    "CustomerDueDate" TEXT NULL,
    "CustomerOrderNumber" TEXT NOT NULL DEFAULT '',
    "DefectQuantity" INTEGER NOT NULL,
    "DensityPercentage" REAL NOT NULL DEFAULT 99.5,
    "EnergyConsumptionKwh" REAL NOT NULL,
    "EstimatedHours" REAL NOT NULL,
    "EstimatedPowderUsageKg" REAL NOT NULL,
    "HatchSpacingMicrons" REAL NOT NULL DEFAULT 120.0,
    "HoldReason" TEXT NOT NULL DEFAULT '',
    "IsRushJob" INTEGER NOT NULL,
    "LaborCostPerHour" decimal(10,2) NOT NULL,
    "LaserPowerWatts" REAL NOT NULL DEFAULT 200.0,
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LayerThicknessMicrons" REAL NOT NULL DEFAULT 30.0,
    "MachineId" TEXT NOT NULL DEFAULT '',
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "MachineUtilizationPercent" REAL NOT NULL,
    "MaterialCostPerKg" decimal(10,2) NOT NULL,
    "Notes" TEXT NULL DEFAULT '',
    "OpcUaBuildProgress" REAL NOT NULL,
    "OpcUaErrorMessages" TEXT NULL DEFAULT '',
    "OpcUaJobId" TEXT NULL DEFAULT '',
    "OpcUaLastUpdate" TEXT NULL,
    "OpcUaStatus" TEXT NULL DEFAULT '',
    "Operator" TEXT NULL DEFAULT '',
    "OxygenContentPpm" REAL NOT NULL DEFAULT 50.0,
    "PartId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL DEFAULT '',
    "PostProcessingTimeMinutes" REAL NOT NULL,
    "PowderChangeoverTimeMinutes" REAL NOT NULL DEFAULT 30.0,
    "PowderExpirationDate" TEXT NULL,
    "PowderLotNumber" TEXT NOT NULL DEFAULT '',
    "PowderRecyclePercentage" REAL NOT NULL DEFAULT 85.0,
    "PowerCostPerKwh" decimal(10,2) NOT NULL,
    "PreheatingTimeMinutes" REAL NOT NULL DEFAULT 60.0,
    "PreviousJobPartNumber" TEXT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "ProcessParameters" TEXT NOT NULL DEFAULT '{}',
    "ProducedQuantity" INTEGER NOT NULL,
    "QualityCheckpoints" TEXT NOT NULL DEFAULT '{}',
    "QualityInspector" TEXT NULL DEFAULT '',
    "Quantity" INTEGER NOT NULL,
    "RequiredMaterials" TEXT NOT NULL DEFAULT '',
    "RequiredSkills" TEXT NOT NULL DEFAULT 'SLS Operation,Powder Handling,Inert Gas Safety',
    "RequiredTooling" TEXT NOT NULL DEFAULT 'Build Platform,Powder Sieve,Inert Gas Setup',
    "RequiresArgonPurge" INTEGER NOT NULL DEFAULT 1,
    "RequiresPostProcessing" INTEGER NOT NULL,
    "RequiresPowderSieving" INTEGER NOT NULL DEFAULT 1,
    "RequiresPreheating" INTEGER NOT NULL DEFAULT 1,
    "ReworkQuantity" INTEGER NOT NULL,
    "ScanSpeedMmPerSec" REAL NOT NULL DEFAULT 1200.0,
    "ScheduledEnd" TEXT NOT NULL,
    "ScheduledStart" TEXT NOT NULL,
    "SetupTimeMinutes" REAL NOT NULL,
    "SlsMaterial" TEXT NOT NULL DEFAULT 'Ti-6Al-4V Grade 5',
    "SpecialInstructions" TEXT NOT NULL DEFAULT '',
    "Status" TEXT NOT NULL DEFAULT 'Scheduled',
    "Supervisor" TEXT NULL DEFAULT '',
    "SurfaceRoughnessRa" REAL NOT NULL,
    "UltimateTensileStrengthMPa" REAL NOT NULL,
    CONSTRAINT "FK_Jobs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_Jobs" ("Id", "ActualEnd", "ActualPowderUsageKg", "ActualStart", "ArgonCostPerHour", "ArgonPurityPercent", "BuildFileCreatedDate", "BuildFileName", "BuildFilePath", "BuildFileSizeBytes", "BuildLayerNumber", "BuildPlatformId", "BuildTemperatureCelsius", "BuildTimeMinutes", "ChangeoverTimeMinutes", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CurrentArgonFlowRate", "CurrentBuildTemperature", "CurrentLaserPowerWatts", "CurrentOxygenLevel", "CustomerDueDate", "CustomerOrderNumber", "DefectQuantity", "DensityPercentage", "EnergyConsumptionKwh", "EstimatedHours", "EstimatedPowderUsageKg", "HatchSpacingMicrons", "HoldReason", "IsRushJob", "LaborCostPerHour", "LaserPowerWatts", "LastModifiedBy", "LastModifiedDate", "LayerThicknessMicrons", "MachineId", "MachineOperatingCostPerHour", "MachineUtilizationPercent", "MaterialCostPerKg", "Notes", "OpcUaBuildProgress", "OpcUaErrorMessages", "OpcUaJobId", "OpcUaLastUpdate", "OpcUaStatus", "Operator", "OxygenContentPpm", "PartId", "PartNumber", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderExpirationDate", "PowderLotNumber", "PowderRecyclePercentage", "PowerCostPerKwh", "PreheatingTimeMinutes", "PreviousJobPartNumber", "Priority", "ProcessParameters", "ProducedQuantity", "QualityCheckpoints", "QualityInspector", "Quantity", "RequiredMaterials", "RequiredSkills", "RequiredTooling", "RequiresArgonPurge", "RequiresPostProcessing", "RequiresPowderSieving", "RequiresPreheating", "ReworkQuantity", "ScanSpeedMmPerSec", "ScheduledEnd", "ScheduledStart", "SetupTimeMinutes", "SlsMaterial", "SpecialInstructions", "Status", "Supervisor", "SurfaceRoughnessRa", "UltimateTensileStrengthMPa")
SELECT "Id", "ActualEnd", "ActualPowderUsageKg", "ActualStart", "ArgonCostPerHour", "ArgonPurityPercent", "BuildFileCreatedDate", "BuildFileName", "BuildFilePath", "BuildFileSizeBytes", "BuildLayerNumber", "BuildPlatformId", "BuildTemperatureCelsius", "BuildTimeMinutes", "ChangeoverTimeMinutes", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CurrentArgonFlowRate", "CurrentBuildTemperature", "CurrentLaserPowerWatts", "CurrentOxygenLevel", "CustomerDueDate", "CustomerOrderNumber", "DefectQuantity", "DensityPercentage", "EnergyConsumptionKwh", "EstimatedHours", "EstimatedPowderUsageKg", "HatchSpacingMicrons", "HoldReason", "IsRushJob", "LaborCostPerHour", "LaserPowerWatts", "LastModifiedBy", "LastModifiedDate", "LayerThicknessMicrons", "MachineId", "MachineOperatingCostPerHour", "MachineUtilizationPercent", "MaterialCostPerKg", "Notes", "OpcUaBuildProgress", "OpcUaErrorMessages", "OpcUaJobId", "OpcUaLastUpdate", "OpcUaStatus", "Operator", "OxygenContentPpm", "PartId", "PartNumber", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderExpirationDate", "PowderLotNumber", "PowderRecyclePercentage", "PowerCostPerKwh", "PreheatingTimeMinutes", "PreviousJobPartNumber", "Priority", "ProcessParameters", "ProducedQuantity", "QualityCheckpoints", "QualityInspector", "Quantity", "RequiredMaterials", "RequiredSkills", "RequiredTooling", "RequiresArgonPurge", "RequiresPostProcessing", "RequiresPowderSieving", "RequiresPreheating", "ReworkQuantity", "ScanSpeedMmPerSec", "ScheduledEnd", "ScheduledStart", "SetupTimeMinutes", "SlsMaterial", "SpecialInstructions", "Status", "Supervisor", "SurfaceRoughnessRa", "UltimateTensileStrengthMPa"
FROM "Jobs";

CREATE TABLE "ef_temp_Parts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Parts" PRIMARY KEY AUTOINCREMENT,
    "Application" TEXT NOT NULL,
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "AverageActualHours" REAL NOT NULL,
    "AverageCostPerUnit" decimal(10,2) NOT NULL,
    "AverageDefectRate" REAL NOT NULL,
    "AverageEfficiencyPercent" REAL NOT NULL,
    "AveragePowderUtilization" REAL NOT NULL,
    "AverageQualityScore" REAL NOT NULL,
    "AvgDuration" TEXT NOT NULL DEFAULT '8h',
    "AvgDurationDays" INTEGER NOT NULL,
    "BuildFileTemplate" TEXT NOT NULL,
    "CadFilePath" TEXT NOT NULL,
    "CadFileVersion" TEXT NOT NULL,
    "ConsumableMaterials" TEXT NOT NULL,
    "CoolingTimeMinutes" REAL NOT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CustomerPartNumber" TEXT NOT NULL,
    "Description" TEXT NOT NULL DEFAULT '',
    "Dimensions" TEXT NOT NULL,
    "EstimatedHours" REAL NOT NULL DEFAULT 8.0,
    "HeightMm" REAL NOT NULL,
    "Industry" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastProduced" TEXT NULL,
    "LengthMm" REAL NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "Material" TEXT NOT NULL DEFAULT 'Ti-6Al-4V Grade 5',
    "MaterialCostPerKg" decimal(12,2) NOT NULL,
    "MaxOxygenContent" REAL NOT NULL,
    "MaxSurfaceRoughnessRa" REAL NOT NULL,
    "PartCategory" TEXT NOT NULL,
    "PartClass" TEXT NOT NULL,
    "PartNumber" TEXT NOT NULL DEFAULT '',
    "PostProcessingCost" decimal(10,2) NOT NULL,
    "PostProcessingTimeMinutes" REAL NOT NULL,
    "PowderChangeoverTimeMinutes" REAL NOT NULL,
    "PowderRequirementKg" REAL NOT NULL,
    "PowderSpecification" TEXT NOT NULL DEFAULT '15-45 μm particle size',
    "PreferredMachines" TEXT NOT NULL DEFAULT 'TI1,TI2',
    "PreheatingTimeMinutes" REAL NOT NULL,
    "ProcessParameters" TEXT NOT NULL DEFAULT '{}',
    "ProcessType" TEXT NOT NULL DEFAULT 'SLS Metal',
    "QualityCheckpoints" TEXT NOT NULL DEFAULT '{}',
    "QualityInspectionCost" decimal(10,2) NOT NULL,
    "QualityStandards" TEXT NOT NULL,
    "RecommendedBuildTemperature" REAL NOT NULL,
    "RecommendedHatchSpacing" REAL NOT NULL,
    "RecommendedLaserPower" REAL NOT NULL,
    "RecommendedLayerThickness" REAL NOT NULL,
    "RecommendedScanSpeed" REAL NOT NULL,
    "RequiredArgonPurity" REAL NOT NULL,
    "RequiredCertifications" TEXT NOT NULL,
    "RequiredMachineType" TEXT NOT NULL DEFAULT 'TruPrint 3000',
    "RequiredSkills" TEXT NOT NULL,
    "RequiredTooling" TEXT NOT NULL,
    "RequiresAS9100" INTEGER NOT NULL,
    "RequiresCertification" INTEGER NOT NULL,
    "RequiresFDA" INTEGER NOT NULL,
    "RequiresInspection" INTEGER NOT NULL,
    "RequiresNADCAP" INTEGER NOT NULL,
    "RequiresSupports" INTEGER NOT NULL,
    "SetupCost" decimal(10,2) NOT NULL,
    "SetupTimeMinutes" REAL NOT NULL,
    "SlsMaterial" TEXT NOT NULL DEFAULT 'Ti-6Al-4V Grade 5',
    "StandardLaborCostPerHour" decimal(10,2) NOT NULL,
    "StandardSellingPrice" decimal(10,2) NOT NULL,
    "SupportRemovalTimeMinutes" REAL NOT NULL,
    "SupportStrategy" TEXT NOT NULL,
    "SurfaceFinishRequirement" TEXT NOT NULL,
    "ToleranceRequirements" TEXT NOT NULL,
    "TotalJobsCompleted" INTEGER NOT NULL,
    "TotalUnitsProduced" INTEGER NOT NULL,
    "VolumeMm3" REAL NOT NULL,
    "WeightGrams" REAL NOT NULL,
    "WidthMm" REAL NOT NULL
);

INSERT INTO "ef_temp_Parts" ("Id", "Application", "ArgonCostPerHour", "AverageActualHours", "AverageCostPerUnit", "AverageDefectRate", "AverageEfficiencyPercent", "AveragePowderUtilization", "AverageQualityScore", "AvgDuration", "AvgDurationDays", "BuildFileTemplate", "CadFilePath", "CadFileVersion", "ConsumableMaterials", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CustomerPartNumber", "Description", "Dimensions", "EstimatedHours", "HeightMm", "Industry", "IsActive", "LastModifiedBy", "LastModifiedDate", "LastProduced", "LengthMm", "MachineOperatingCostPerHour", "Material", "MaterialCostPerKg", "MaxOxygenContent", "MaxSurfaceRoughnessRa", "PartCategory", "PartClass", "PartNumber", "PostProcessingCost", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderRequirementKg", "PowderSpecification", "PreferredMachines", "PreheatingTimeMinutes", "ProcessParameters", "ProcessType", "QualityCheckpoints", "QualityInspectionCost", "QualityStandards", "RecommendedBuildTemperature", "RecommendedHatchSpacing", "RecommendedLaserPower", "RecommendedLayerThickness", "RecommendedScanSpeed", "RequiredArgonPurity", "RequiredCertifications", "RequiredMachineType", "RequiredSkills", "RequiredTooling", "RequiresAS9100", "RequiresCertification", "RequiresFDA", "RequiresInspection", "RequiresNADCAP", "RequiresSupports", "SetupCost", "SetupTimeMinutes", "SlsMaterial", "StandardLaborCostPerHour", "StandardSellingPrice", "SupportRemovalTimeMinutes", "SupportStrategy", "SurfaceFinishRequirement", "ToleranceRequirements", "TotalJobsCompleted", "TotalUnitsProduced", "VolumeMm3", "WeightGrams", "WidthMm")
SELECT "Id", "Application", "ArgonCostPerHour", "AverageActualHours", "AverageCostPerUnit", "AverageDefectRate", "AverageEfficiencyPercent", "AveragePowderUtilization", "AverageQualityScore", "AvgDuration", "AvgDurationDays", "BuildFileTemplate", "CadFilePath", "CadFileVersion", "ConsumableMaterials", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CustomerPartNumber", "Description", "Dimensions", "EstimatedHours", "HeightMm", "Industry", "IsActive", "LastModifiedBy", "LastModifiedDate", "LastProduced", "LengthMm", "MachineOperatingCostPerHour", "Material", "MaterialCostPerKg", "MaxOxygenContent", "MaxSurfaceRoughnessRa", "PartCategory", "PartClass", "PartNumber", "PostProcessingCost", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderRequirementKg", "PowderSpecification", "PreferredMachines", "PreheatingTimeMinutes", "ProcessParameters", "ProcessType", "QualityCheckpoints", "QualityInspectionCost", "QualityStandards", "RecommendedBuildTemperature", "RecommendedHatchSpacing", "RecommendedLaserPower", "RecommendedLayerThickness", "RecommendedScanSpeed", "RequiredArgonPurity", "RequiredCertifications", "RequiredMachineType", "RequiredSkills", "RequiredTooling", "RequiresAS9100", "RequiresCertification", "RequiresFDA", "RequiresInspection", "RequiresNADCAP", "RequiresSupports", "SetupCost", "SetupTimeMinutes", "SlsMaterial", "StandardLaborCostPerHour", "StandardSellingPrice", "SupportRemovalTimeMinutes", "SupportStrategy", "SurfaceFinishRequirement", "ToleranceRequirements", "TotalJobsCompleted", "TotalUnitsProduced", "VolumeMm3", "WeightGrams", "WidthMm"
FROM "Parts";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "Jobs";

ALTER TABLE "ef_temp_Jobs" RENAME TO "Jobs";

DROP TABLE "Parts";

ALTER TABLE "ef_temp_Parts" RENAME TO "Parts";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

CREATE INDEX "IX_Jobs_CustomerOrderNumber" ON "Jobs" ("CustomerOrderNumber");

CREATE INDEX "IX_Jobs_MachineId" ON "Jobs" ("MachineId");

CREATE INDEX "IX_Jobs_MachineId_ScheduledStart" ON "Jobs" ("MachineId", "ScheduledStart");

CREATE INDEX "IX_Jobs_OpcUaJobId" ON "Jobs" ("OpcUaJobId");

CREATE INDEX "IX_Jobs_PartId" ON "Jobs" ("PartId");

CREATE INDEX "IX_Jobs_PartNumber" ON "Jobs" ("PartNumber");

CREATE INDEX "IX_Jobs_Priority" ON "Jobs" ("Priority");

CREATE INDEX "IX_Jobs_ScheduledStart" ON "Jobs" ("ScheduledStart");

CREATE INDEX "IX_Jobs_SlsMaterial" ON "Jobs" ("SlsMaterial");

CREATE INDEX "IX_Jobs_Status" ON "Jobs" ("Status");

CREATE INDEX "IX_Parts_Industry" ON "Parts" ("Industry");

CREATE INDEX "IX_Parts_IsActive" ON "Parts" ("IsActive");

CREATE INDEX "IX_Parts_Material" ON "Parts" ("Material");

CREATE INDEX "IX_Parts_PartCategory" ON "Parts" ("PartCategory");

CREATE INDEX "IX_Parts_PartClass" ON "Parts" ("PartClass");

CREATE UNIQUE INDEX "IX_Parts_PartNumber" ON "Parts" ("PartNumber");

CREATE INDEX "IX_Parts_ProcessType" ON "Parts" ("ProcessType");

CREATE INDEX "IX_Parts_RequiredMachineType" ON "Parts" ("RequiredMachineType");

CREATE INDEX "IX_Parts_SlsMaterial" ON "Parts" ("SlsMaterial");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250725221757_AdminControlSystemModels', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

DROP TABLE "MachineDataSnapshot";

DROP TABLE "SlsMachines";

DROP INDEX "IX_Users_Email";

DROP INDEX "IX_Users_IsActive";

DROP INDEX "IX_Users_Role";

DROP INDEX "IX_Users_Username";

DROP INDEX "IX_Parts_Industry";

DROP INDEX "IX_Parts_IsActive";

DROP INDEX "IX_Parts_Material";

DROP INDEX "IX_Parts_PartCategory";

DROP INDEX "IX_Parts_PartClass";

DROP INDEX "IX_Parts_ProcessType";

DROP INDEX "IX_Parts_RequiredMachineType";

DROP INDEX "IX_Parts_SlsMaterial";

DROP INDEX "IX_Jobs_CustomerOrderNumber";

DROP INDEX "IX_Jobs_MachineId";

DROP INDEX "IX_Jobs_OpcUaJobId";

DROP INDEX "IX_Jobs_ScheduledStart";

DROP INDEX "IX_Jobs_SlsMaterial";

DROP INDEX "IX_JobLogEntries_Action";

DROP INDEX "IX_JobLogEntries_MachineId";

DROP INDEX "IX_JobLogEntries_Timestamp";

DROP INDEX "IX_DelayLogs_BuildId";

DROP INDEX "IX_DelayLogs_CreatedAt";

DROP INDEX "IX_DelayLogs_DelayDuration";

DROP INDEX "IX_DelayLogs_DelayReason";

DROP INDEX "IX_BuildJobs_ActualStartTime";

DROP INDEX "IX_BuildJobs_AssociatedScheduledJobId";

DROP INDEX "IX_BuildJobs_PrinterName";

DROP INDEX "IX_BuildJobs_PrinterName_ActualStartTime";

DROP INDEX "IX_BuildJobs_Status";

DROP INDEX "IX_BuildJobParts_BuildId";

DROP INDEX "IX_BuildJobParts_BuildId_IsPrimary";

DROP INDEX "IX_BuildJobParts_IsPrimary";

DROP INDEX "IX_BuildJobParts_PartNumber";

ALTER TABLE "MachineCapabilities" RENAME COLUMN "SlsMachineId" TO "MachineId";

DROP INDEX "IX_MachineCapabilities_SlsMachineId_CapabilityType";

CREATE INDEX "IX_MachineCapabilities_MachineId_CapabilityType" ON "MachineCapabilities" ("MachineId", "CapabilityType");

DROP INDEX "IX_MachineCapabilities_SlsMachineId";

CREATE INDEX "IX_MachineCapabilities_MachineId" ON "MachineCapabilities" ("MachineId");

ALTER TABLE "Parts" ADD "AdminEstimatedHoursOverride" REAL NULL;

ALTER TABLE "Parts" ADD "AdminOverrideBy" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "AdminOverrideDate" TEXT NULL;

ALTER TABLE "Parts" ADD "AdminOverrideReason" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "Name" TEXT NOT NULL DEFAULT '';

ALTER TABLE "MachineCapabilities" ADD "MachineId1" INTEGER NULL;

ALTER TABLE "Jobs" ADD "EstimatedDuration" TEXT NOT NULL DEFAULT '00:00:00';

ALTER TABLE "DelayLogs" ADD "BuildJobBuildId" INTEGER NULL;

ALTER TABLE "BuildJobParts" ADD "BuildJobBuildId" INTEGER NULL;

CREATE TABLE "JobNotes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_JobNotes" PRIMARY KEY AUTOINCREMENT,
    "JobId" INTEGER NOT NULL,
    "Step" TEXT NOT NULL,
    "Note" TEXT NOT NULL,
    "StepTime" TEXT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "NoteType" TEXT NOT NULL DEFAULT 'Info',
    "IsCompleted" INTEGER NOT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "PartId" INTEGER NULL,
    CONSTRAINT "FK_JobNotes_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES "Jobs" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_JobNotes_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id")
);

CREATE TABLE "Machines" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Machines" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "MachineName" TEXT NOT NULL,
    "MachineType" TEXT NOT NULL,
    "MachineModel" TEXT NOT NULL,
    "SerialNumber" TEXT NOT NULL,
    "Location" TEXT NOT NULL,
    "Department" TEXT NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Idle',
    "IsActive" INTEGER NOT NULL,
    "IsAvailableForScheduling" INTEGER NOT NULL,
    "Priority" INTEGER NOT NULL,
    "LastStatusUpdate" TEXT NOT NULL,
    "TechnicalSpecifications" TEXT NOT NULL,
    "SupportedMaterials" TEXT NOT NULL,
    "CurrentMaterial" TEXT NOT NULL,
    "MaintenanceIntervalHours" REAL NOT NULL,
    "HoursSinceLastMaintenance" REAL NOT NULL,
    "LastMaintenanceDate" TEXT NULL,
    "NextMaintenanceDate" TEXT NULL,
    "AverageUtilizationPercent" REAL NOT NULL,
    "MaintenanceNotes" TEXT NOT NULL,
    "OperatorNotes" TEXT NOT NULL,
    "OpcUaEndpointUrl" TEXT NOT NULL,
    "OpcUaEnabled" INTEGER NOT NULL,
    "CommunicationSettings" TEXT NOT NULL,
    "CurrentJobId" INTEGER NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "BuildLengthMm" REAL NOT NULL,
    "BuildWidthMm" REAL NOT NULL,
    "BuildHeightMm" REAL NOT NULL,
    "MaxLaserPowerWatts" REAL NOT NULL,
    "MaxScanSpeedMmPerSec" REAL NOT NULL,
    "MinLayerThicknessMicrons" REAL NOT NULL,
    "MaxLayerThicknessMicrons" REAL NOT NULL,
    "TotalOperatingHours" REAL NOT NULL,
    CONSTRAINT "AK_Machines_MachineId" UNIQUE ("MachineId"),
    CONSTRAINT "FK_Machines_Jobs_CurrentJobId" FOREIGN KEY ("CurrentJobId") REFERENCES "Jobs" ("Id")
);

CREATE TABLE "JobStages" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_JobStages" PRIMARY KEY AUTOINCREMENT,
    "JobId" INTEGER NOT NULL,
    "StageType" TEXT NOT NULL,
    "StageName" TEXT NOT NULL,
    "ExecutionOrder" INTEGER NOT NULL,
    "Department" TEXT NOT NULL,
    "MachineId" TEXT NULL,
    "ScheduledStart" TEXT NOT NULL,
    "ScheduledEnd" TEXT NOT NULL,
    "ActualStart" TEXT NULL,
    "ActualEnd" TEXT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Scheduled',
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "EstimatedDurationHours" REAL NOT NULL DEFAULT 1.0,
    "CanStart" INTEGER NOT NULL,
    "SetupTimeHours" REAL NOT NULL,
    "CooldownTimeHours" REAL NOT NULL,
    "AssignedOperator" TEXT NULL,
    "Notes" TEXT NULL,
    "QualityRequirements" TEXT NULL,
    "RequiredMaterials" TEXT NULL,
    "RequiredTooling" TEXT NULL,
    "EstimatedCost" TEXT NOT NULL,
    "ActualCost" TEXT NULL,
    "IsBlocking" INTEGER NOT NULL,
    "AllowParallel" INTEGER NOT NULL,
    "ProgressPercent" REAL NOT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_JobStages_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES "Jobs" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_JobStages_Machines_MachineId" FOREIGN KEY ("MachineId") REFERENCES "Machines" ("MachineId") ON DELETE SET NULL
);

CREATE TABLE "StageDependencies" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StageDependencies" PRIMARY KEY AUTOINCREMENT,
    "DependentStageId" INTEGER NOT NULL,
    "RequiredStageId" INTEGER NOT NULL,
    "DependencyType" TEXT NOT NULL DEFAULT 'FinishToStart',
    "LagTimeHours" REAL NOT NULL,
    "IsMandatory" INTEGER NOT NULL,
    "Notes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "CK_StageDependency_NoSelfReference" CHECK (DependentStageId != RequiredStageId),
    CONSTRAINT "FK_StageDependencies_JobStages_DependentStageId" FOREIGN KEY ("DependentStageId") REFERENCES "JobStages" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_StageDependencies_JobStages_RequiredStageId" FOREIGN KEY ("RequiredStageId") REFERENCES "JobStages" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "StageNotes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StageNotes" PRIMARY KEY AUTOINCREMENT,
    "StageId" INTEGER NOT NULL,
    "Note" TEXT NOT NULL,
    "NoteType" TEXT NOT NULL DEFAULT 'Info',
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "IsPublic" INTEGER NOT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_StageNotes_JobStages_StageId" FOREIGN KEY ("StageId") REFERENCES "JobStages" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_MachineCapabilities_MachineId1" ON "MachineCapabilities" ("MachineId1");

CREATE INDEX "IX_DelayLogs_BuildJobBuildId" ON "DelayLogs" ("BuildJobBuildId");

CREATE INDEX "IX_BuildJobParts_BuildJobBuildId" ON "BuildJobParts" ("BuildJobBuildId");

CREATE INDEX "IX_JobNotes_IsCompleted" ON "JobNotes" ("IsCompleted");

CREATE INDEX "IX_JobNotes_JobId" ON "JobNotes" ("JobId");

CREATE INDEX "IX_JobNotes_JobId_Step" ON "JobNotes" ("JobId", "Step");

CREATE INDEX "IX_JobNotes_PartId" ON "JobNotes" ("PartId");

CREATE INDEX "IX_JobNotes_Priority" ON "JobNotes" ("Priority");

CREATE INDEX "IX_JobStages_Department" ON "JobStages" ("Department");

CREATE INDEX "IX_JobStages_JobId" ON "JobStages" ("JobId");

CREATE INDEX "IX_JobStages_JobId_ExecutionOrder" ON "JobStages" ("JobId", "ExecutionOrder");

CREATE INDEX "IX_JobStages_MachineId" ON "JobStages" ("MachineId");

CREATE INDEX "IX_JobStages_ScheduledEnd" ON "JobStages" ("ScheduledEnd");

CREATE INDEX "IX_JobStages_ScheduledStart" ON "JobStages" ("ScheduledStart");

CREATE INDEX "IX_JobStages_StageType" ON "JobStages" ("StageType");

CREATE INDEX "IX_JobStages_Status" ON "JobStages" ("Status");

CREATE INDEX "IX_Machines_CurrentJobId" ON "Machines" ("CurrentJobId");

CREATE INDEX "IX_Machines_IsActive" ON "Machines" ("IsActive");

CREATE UNIQUE INDEX "IX_Machines_MachineId" ON "Machines" ("MachineId");

CREATE INDEX "IX_Machines_MachineType" ON "Machines" ("MachineType");

CREATE INDEX "IX_Machines_Status" ON "Machines" ("Status");

CREATE INDEX "IX_StageDependencies_DependencyType" ON "StageDependencies" ("DependencyType");

CREATE INDEX "IX_StageDependencies_DependentStageId" ON "StageDependencies" ("DependentStageId");

CREATE INDEX "IX_StageDependencies_RequiredStageId" ON "StageDependencies" ("RequiredStageId");

CREATE INDEX "IX_StageNotes_CreatedDate" ON "StageNotes" ("CreatedDate");

CREATE INDEX "IX_StageNotes_NoteType" ON "StageNotes" ("NoteType");

CREATE INDEX "IX_StageNotes_Priority" ON "StageNotes" ("Priority");

CREATE INDEX "IX_StageNotes_StageId" ON "StageNotes" ("StageId");

CREATE TABLE "ef_temp_BuildJobParts" (
    "PartEntryId" INTEGER NOT NULL CONSTRAINT "PK_BuildJobParts" PRIMARY KEY AUTOINCREMENT,
    "BuildId" INTEGER NOT NULL,
    "BuildJobBuildId" INTEGER NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "Description" TEXT NULL,
    "EstimatedHours" REAL NOT NULL,
    "IsPrimary" INTEGER NOT NULL,
    "Material" TEXT NULL,
    "PartNumber" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL,
    CONSTRAINT "FK_BuildJobParts_BuildJobs_BuildJobBuildId" FOREIGN KEY ("BuildJobBuildId") REFERENCES "BuildJobs" ("BuildId")
);

INSERT INTO "ef_temp_BuildJobParts" ("PartEntryId", "BuildId", "BuildJobBuildId", "CreatedAt", "CreatedBy", "Description", "EstimatedHours", "IsPrimary", "Material", "PartNumber", "Quantity")
SELECT "PartEntryId", "BuildId", "BuildJobBuildId", "CreatedAt", "CreatedBy", "Description", "EstimatedHours", "IsPrimary", "Material", "PartNumber", "Quantity"
FROM "BuildJobParts";

CREATE TABLE "ef_temp_BuildJobs" (
    "BuildId" INTEGER NOT NULL CONSTRAINT "PK_BuildJobs" PRIMARY KEY AUTOINCREMENT,
    "ActualEndTime" TEXT NULL,
    "ActualStartTime" TEXT NOT NULL,
    "AssociatedScheduledJobId" INTEGER NULL,
    "CompletedAt" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "GasUsed_L" REAL NULL,
    "LaserRunTime" TEXT NULL,
    "Notes" TEXT NULL,
    "PowderUsed_L" REAL NULL,
    "PrinterName" TEXT NOT NULL,
    "ReasonForEnd" TEXT NULL,
    "ScheduledEndTime" TEXT NULL,
    "ScheduledStartTime" TEXT NULL,
    "SetupNotes" TEXT NULL,
    "Status" TEXT NOT NULL,
    "UserId" INTEGER NOT NULL,
    CONSTRAINT "FK_BuildJobs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_BuildJobs" ("BuildId", "ActualEndTime", "ActualStartTime", "AssociatedScheduledJobId", "CompletedAt", "CreatedAt", "GasUsed_L", "LaserRunTime", "Notes", "PowderUsed_L", "PrinterName", "ReasonForEnd", "ScheduledEndTime", "ScheduledStartTime", "SetupNotes", "Status", "UserId")
SELECT "BuildId", "ActualEndTime", "ActualStartTime", "AssociatedScheduledJobId", "CompletedAt", "CreatedAt", "GasUsed_L", "LaserRunTime", "Notes", "PowderUsed_L", "PrinterName", "ReasonForEnd", "ScheduledEndTime", "ScheduledStartTime", "SetupNotes", "Status", "UserId"
FROM "BuildJobs";

CREATE TABLE "ef_temp_DelayLogs" (
    "DelayId" INTEGER NOT NULL CONSTRAINT "PK_DelayLogs" PRIMARY KEY AUTOINCREMENT,
    "BuildId" INTEGER NOT NULL,
    "BuildJobBuildId" INTEGER NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "DelayDuration" INTEGER NOT NULL,
    "DelayReason" TEXT NOT NULL,
    "Description" TEXT NULL,
    CONSTRAINT "FK_DelayLogs_BuildJobs_BuildJobBuildId" FOREIGN KEY ("BuildJobBuildId") REFERENCES "BuildJobs" ("BuildId")
);

INSERT INTO "ef_temp_DelayLogs" ("DelayId", "BuildId", "BuildJobBuildId", "CreatedAt", "CreatedBy", "DelayDuration", "DelayReason", "Description")
SELECT "DelayId", "BuildId", "BuildJobBuildId", "CreatedAt", "CreatedBy", "DelayDuration", "DelayReason", "Description"
FROM "DelayLogs";

CREATE TABLE "ef_temp_Jobs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Jobs" PRIMARY KEY AUTOINCREMENT,
    "ActualEnd" TEXT NULL,
    "ActualPowderUsageKg" REAL NOT NULL,
    "ActualStart" TEXT NULL,
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "ArgonPurityPercent" REAL NOT NULL DEFAULT 99.5,
    "BuildFileCreatedDate" TEXT NULL,
    "BuildFileName" TEXT NULL,
    "BuildFilePath" TEXT NULL,
    "BuildFileSizeBytes" INTEGER NOT NULL,
    "BuildLayerNumber" INTEGER NOT NULL,
    "BuildPlatformId" TEXT NOT NULL,
    "BuildTemperatureCelsius" REAL NOT NULL DEFAULT 180.0,
    "BuildTimeMinutes" REAL NOT NULL,
    "ChangeoverTimeMinutes" REAL NOT NULL,
    "CoolingTimeMinutes" REAL NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CurrentArgonFlowRate" REAL NOT NULL,
    "CurrentBuildTemperature" REAL NOT NULL,
    "CurrentLaserPowerWatts" REAL NOT NULL,
    "CurrentOxygenLevel" REAL NOT NULL,
    "CustomerDueDate" TEXT NULL,
    "CustomerOrderNumber" TEXT NOT NULL,
    "DefectQuantity" INTEGER NOT NULL,
    "DensityPercentage" REAL NOT NULL,
    "EnergyConsumptionKwh" REAL NOT NULL,
    "EstimatedDuration" TEXT NOT NULL,
    "EstimatedHours" REAL NOT NULL,
    "EstimatedPowderUsageKg" REAL NOT NULL DEFAULT 0.5,
    "HatchSpacingMicrons" REAL NOT NULL DEFAULT 120.0,
    "HoldReason" TEXT NOT NULL,
    "IsRushJob" INTEGER NOT NULL,
    "LaborCostPerHour" decimal(10,2) NOT NULL,
    "LaserPowerWatts" REAL NOT NULL DEFAULT 170.0,
    "LastModifiedBy" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LayerThicknessMicrons" REAL NOT NULL DEFAULT 30.0,
    "MachineId" TEXT NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "MachineUtilizationPercent" REAL NOT NULL,
    "MaterialCostPerKg" decimal(10,2) NOT NULL,
    "Notes" TEXT NULL,
    "OpcUaBuildProgress" REAL NOT NULL,
    "OpcUaErrorMessages" TEXT NULL,
    "OpcUaJobId" TEXT NULL,
    "OpcUaLastUpdate" TEXT NULL,
    "OpcUaStatus" TEXT NULL,
    "Operator" TEXT NULL,
    "OxygenContentPpm" REAL NOT NULL DEFAULT 50.0,
    "PartId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "PostProcessingTimeMinutes" REAL NOT NULL,
    "PowderChangeoverTimeMinutes" REAL NOT NULL,
    "PowderExpirationDate" TEXT NULL,
    "PowderLotNumber" TEXT NOT NULL,
    "PowderRecyclePercentage" REAL NOT NULL,
    "PowerCostPerKwh" decimal(10,2) NOT NULL,
    "PreheatingTimeMinutes" REAL NOT NULL,
    "PreviousJobPartNumber" TEXT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "ProcessParameters" TEXT NOT NULL,
    "ProducedQuantity" INTEGER NOT NULL,
    "QualityCheckpoints" TEXT NOT NULL,
    "QualityInspector" TEXT NULL,
    "Quantity" INTEGER NOT NULL,
    "RequiredMaterials" TEXT NOT NULL,
    "RequiredSkills" TEXT NOT NULL,
    "RequiredTooling" TEXT NOT NULL,
    "RequiresArgonPurge" INTEGER NOT NULL,
    "RequiresPostProcessing" INTEGER NOT NULL,
    "RequiresPowderSieving" INTEGER NOT NULL,
    "RequiresPreheating" INTEGER NOT NULL,
    "ReworkQuantity" INTEGER NOT NULL,
    "ScanSpeedMmPerSec" REAL NOT NULL DEFAULT 1000.0,
    "ScheduledEnd" TEXT NOT NULL,
    "ScheduledStart" TEXT NOT NULL,
    "SetupTimeMinutes" REAL NOT NULL,
    "SlsMaterial" TEXT NOT NULL,
    "SpecialInstructions" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "Supervisor" TEXT NULL,
    "SurfaceRoughnessRa" REAL NOT NULL,
    "UltimateTensileStrengthMPa" REAL NOT NULL,
    CONSTRAINT "FK_Jobs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Jobs" ("Id", "ActualEnd", "ActualPowderUsageKg", "ActualStart", "ArgonCostPerHour", "ArgonPurityPercent", "BuildFileCreatedDate", "BuildFileName", "BuildFilePath", "BuildFileSizeBytes", "BuildLayerNumber", "BuildPlatformId", "BuildTemperatureCelsius", "BuildTimeMinutes", "ChangeoverTimeMinutes", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CurrentArgonFlowRate", "CurrentBuildTemperature", "CurrentLaserPowerWatts", "CurrentOxygenLevel", "CustomerDueDate", "CustomerOrderNumber", "DefectQuantity", "DensityPercentage", "EnergyConsumptionKwh", "EstimatedDuration", "EstimatedHours", "EstimatedPowderUsageKg", "HatchSpacingMicrons", "HoldReason", "IsRushJob", "LaborCostPerHour", "LaserPowerWatts", "LastModifiedBy", "LastModifiedDate", "LayerThicknessMicrons", "MachineId", "MachineOperatingCostPerHour", "MachineUtilizationPercent", "MaterialCostPerKg", "Notes", "OpcUaBuildProgress", "OpcUaErrorMessages", "OpcUaJobId", "OpcUaLastUpdate", "OpcUaStatus", "Operator", "OxygenContentPpm", "PartId", "PartNumber", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderExpirationDate", "PowderLotNumber", "PowderRecyclePercentage", "PowerCostPerKwh", "PreheatingTimeMinutes", "PreviousJobPartNumber", "Priority", "ProcessParameters", "ProducedQuantity", "QualityCheckpoints", "QualityInspector", "Quantity", "RequiredMaterials", "RequiredSkills", "RequiredTooling", "RequiresArgonPurge", "RequiresPostProcessing", "RequiresPowderSieving", "RequiresPreheating", "ReworkQuantity", "ScanSpeedMmPerSec", "ScheduledEnd", "ScheduledStart", "SetupTimeMinutes", "SlsMaterial", "SpecialInstructions", "Status", "Supervisor", "SurfaceRoughnessRa", "UltimateTensileStrengthMPa")
SELECT "Id", "ActualEnd", "ActualPowderUsageKg", "ActualStart", "ArgonCostPerHour", "ArgonPurityPercent", "BuildFileCreatedDate", "BuildFileName", "BuildFilePath", "BuildFileSizeBytes", "BuildLayerNumber", "BuildPlatformId", "BuildTemperatureCelsius", "BuildTimeMinutes", "ChangeoverTimeMinutes", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CurrentArgonFlowRate", "CurrentBuildTemperature", "CurrentLaserPowerWatts", "CurrentOxygenLevel", "CustomerDueDate", "CustomerOrderNumber", "DefectQuantity", "DensityPercentage", "EnergyConsumptionKwh", "EstimatedDuration", "EstimatedHours", "EstimatedPowderUsageKg", "HatchSpacingMicrons", "HoldReason", "IsRushJob", "LaborCostPerHour", "LaserPowerWatts", "LastModifiedBy", "LastModifiedDate", "LayerThicknessMicrons", "MachineId", "MachineOperatingCostPerHour", "MachineUtilizationPercent", "MaterialCostPerKg", "Notes", "OpcUaBuildProgress", "OpcUaErrorMessages", "OpcUaJobId", "OpcUaLastUpdate", "OpcUaStatus", "Operator", "OxygenContentPpm", "PartId", "PartNumber", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderExpirationDate", "PowderLotNumber", "PowderRecyclePercentage", "PowerCostPerKwh", "PreheatingTimeMinutes", "PreviousJobPartNumber", "Priority", "ProcessParameters", "ProducedQuantity", "QualityCheckpoints", "QualityInspector", "Quantity", "RequiredMaterials", "RequiredSkills", "RequiredTooling", "RequiresArgonPurge", "RequiresPostProcessing", "RequiresPowderSieving", "RequiresPreheating", "ReworkQuantity", "ScanSpeedMmPerSec", "ScheduledEnd", "ScheduledStart", "SetupTimeMinutes", "SlsMaterial", "SpecialInstructions", "Status", "Supervisor", "SurfaceRoughnessRa", "UltimateTensileStrengthMPa"
FROM "Jobs";

CREATE TABLE "ef_temp_MachineCapabilities" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MachineCapabilities" PRIMARY KEY AUTOINCREMENT,
    "CapabilityName" TEXT NOT NULL,
    "CapabilityType" TEXT NOT NULL,
    "CapabilityValue" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "IsAvailable" INTEGER NOT NULL DEFAULT 1,
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "MachineId" INTEGER NOT NULL,
    "MachineId1" INTEGER NULL,
    "MaxValue" REAL NULL,
    "MinValue" REAL NULL,
    "Notes" TEXT NOT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "RequiredCertification" TEXT NOT NULL,
    "Unit" TEXT NOT NULL,
    CONSTRAINT "FK_MachineCapabilities_Machines_MachineId" FOREIGN KEY ("MachineId") REFERENCES "Machines" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_MachineCapabilities_Machines_MachineId1" FOREIGN KEY ("MachineId1") REFERENCES "Machines" ("Id")
);

INSERT INTO "ef_temp_MachineCapabilities" ("Id", "CapabilityName", "CapabilityType", "CapabilityValue", "CreatedBy", "CreatedDate", "IsAvailable", "LastModifiedBy", "LastModifiedDate", "MachineId", "MachineId1", "MaxValue", "MinValue", "Notes", "Priority", "RequiredCertification", "Unit")
SELECT "Id", "CapabilityName", "CapabilityType", "CapabilityValue", "CreatedBy", "CreatedDate", "IsAvailable", "LastModifiedBy", "LastModifiedDate", "MachineId", "MachineId1", "MaxValue", "MinValue", "Notes", "Priority", "RequiredCertification", "Unit"
FROM "MachineCapabilities";

CREATE TABLE "ef_temp_UserSettings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_UserSettings" PRIMARY KEY AUTOINCREMENT,
    "BrowserNotifications" INTEGER NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "DefaultPage" TEXT NOT NULL,
    "EmailNotifications" INTEGER NOT NULL,
    "ItemsPerPage" INTEGER NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "SessionTimeoutMinutes" INTEGER NOT NULL,
    "Theme" TEXT NOT NULL,
    "TimeZone" TEXT NOT NULL,
    "UserId" INTEGER NOT NULL,
    CONSTRAINT "FK_UserSettings_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_UserSettings" ("Id", "BrowserNotifications", "CreatedDate", "DefaultPage", "EmailNotifications", "ItemsPerPage", "LastModifiedDate", "SessionTimeoutMinutes", "Theme", "TimeZone", "UserId")
SELECT "Id", "BrowserNotifications", "CreatedDate", "DefaultPage", "EmailNotifications", "ItemsPerPage", "LastModifiedDate", "SessionTimeoutMinutes", "Theme", "TimeZone", "UserId"
FROM "UserSettings";

CREATE TABLE "ef_temp_Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "Department" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "FullName" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "LastLoginDate" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Role" TEXT NOT NULL,
    "Username" TEXT NOT NULL
);

INSERT INTO "ef_temp_Users" ("Id", "CreatedBy", "CreatedDate", "Department", "Email", "FullName", "IsActive", "LastLoginDate", "LastModifiedBy", "LastModifiedDate", "PasswordHash", "Role", "Username")
SELECT "Id", "CreatedBy", "CreatedDate", "Department", "Email", "FullName", "IsActive", "LastLoginDate", "LastModifiedBy", "LastModifiedDate", "PasswordHash", "Role", "Username"
FROM "Users";

CREATE TABLE "ef_temp_Parts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Parts" PRIMARY KEY AUTOINCREMENT,
    "AdminEstimatedHoursOverride" REAL NULL,
    "AdminOverrideBy" TEXT NOT NULL,
    "AdminOverrideDate" TEXT NULL,
    "AdminOverrideReason" TEXT NOT NULL,
    "Application" TEXT NOT NULL,
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "AverageActualHours" REAL NOT NULL,
    "AverageCostPerUnit" decimal(10,2) NOT NULL,
    "AverageDefectRate" REAL NOT NULL,
    "AverageEfficiencyPercent" REAL NOT NULL,
    "AveragePowderUtilization" REAL NOT NULL,
    "AverageQualityScore" REAL NOT NULL,
    "AvgDuration" TEXT NOT NULL,
    "AvgDurationDays" INTEGER NOT NULL,
    "BuildFileTemplate" TEXT NOT NULL,
    "CadFilePath" TEXT NOT NULL,
    "CadFileVersion" TEXT NOT NULL,
    "ConsumableMaterials" TEXT NOT NULL,
    "CoolingTimeMinutes" REAL NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CustomerPartNumber" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Dimensions" TEXT NOT NULL,
    "EstimatedHours" REAL NOT NULL,
    "HeightMm" REAL NOT NULL,
    "Industry" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastProduced" TEXT NULL,
    "LengthMm" REAL NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "Material" TEXT NOT NULL,
    "MaterialCostPerKg" decimal(12,2) NOT NULL,
    "MaxOxygenContent" REAL NOT NULL,
    "MaxSurfaceRoughnessRa" REAL NOT NULL,
    "Name" TEXT NOT NULL,
    "PartCategory" TEXT NOT NULL,
    "PartClass" TEXT NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "PostProcessingCost" decimal(10,2) NOT NULL,
    "PostProcessingTimeMinutes" REAL NOT NULL,
    "PowderChangeoverTimeMinutes" REAL NOT NULL,
    "PowderRequirementKg" REAL NOT NULL,
    "PowderSpecification" TEXT NOT NULL,
    "PreferredMachines" TEXT NOT NULL,
    "PreheatingTimeMinutes" REAL NOT NULL,
    "ProcessParameters" TEXT NOT NULL,
    "ProcessType" TEXT NOT NULL,
    "QualityCheckpoints" TEXT NOT NULL,
    "QualityInspectionCost" decimal(10,2) NOT NULL,
    "QualityStandards" TEXT NOT NULL,
    "RecommendedBuildTemperature" REAL NOT NULL,
    "RecommendedHatchSpacing" REAL NOT NULL,
    "RecommendedLaserPower" REAL NOT NULL,
    "RecommendedLayerThickness" REAL NOT NULL,
    "RecommendedScanSpeed" REAL NOT NULL,
    "RequiredArgonPurity" REAL NOT NULL,
    "RequiredCertifications" TEXT NOT NULL,
    "RequiredMachineType" TEXT NOT NULL,
    "RequiredSkills" TEXT NOT NULL,
    "RequiredTooling" TEXT NOT NULL,
    "RequiresAS9100" INTEGER NOT NULL,
    "RequiresCertification" INTEGER NOT NULL,
    "RequiresFDA" INTEGER NOT NULL,
    "RequiresInspection" INTEGER NOT NULL,
    "RequiresNADCAP" INTEGER NOT NULL,
    "RequiresSupports" INTEGER NOT NULL,
    "SetupCost" decimal(10,2) NOT NULL,
    "SetupTimeMinutes" REAL NOT NULL,
    "SlsMaterial" TEXT NOT NULL,
    "StandardLaborCostPerHour" decimal(10,2) NOT NULL,
    "StandardSellingPrice" decimal(10,2) NOT NULL,
    "SupportRemovalTimeMinutes" REAL NOT NULL,
    "SupportStrategy" TEXT NOT NULL,
    "SurfaceFinishRequirement" TEXT NOT NULL,
    "ToleranceRequirements" TEXT NOT NULL,
    "TotalJobsCompleted" INTEGER NOT NULL,
    "TotalUnitsProduced" INTEGER NOT NULL,
    "VolumeMm3" REAL NOT NULL,
    "WeightGrams" REAL NOT NULL,
    "WidthMm" REAL NOT NULL
);

INSERT INTO "ef_temp_Parts" ("Id", "AdminEstimatedHoursOverride", "AdminOverrideBy", "AdminOverrideDate", "AdminOverrideReason", "Application", "ArgonCostPerHour", "AverageActualHours", "AverageCostPerUnit", "AverageDefectRate", "AverageEfficiencyPercent", "AveragePowderUtilization", "AverageQualityScore", "AvgDuration", "AvgDurationDays", "BuildFileTemplate", "CadFilePath", "CadFileVersion", "ConsumableMaterials", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CustomerPartNumber", "Description", "Dimensions", "EstimatedHours", "HeightMm", "Industry", "IsActive", "LastModifiedBy", "LastModifiedDate", "LastProduced", "LengthMm", "MachineOperatingCostPerHour", "Material", "MaterialCostPerKg", "MaxOxygenContent", "MaxSurfaceRoughnessRa", "Name", "PartCategory", "PartClass", "PartNumber", "PostProcessingCost", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderRequirementKg", "PowderSpecification", "PreferredMachines", "PreheatingTimeMinutes", "ProcessParameters", "ProcessType", "QualityCheckpoints", "QualityInspectionCost", "QualityStandards", "RecommendedBuildTemperature", "RecommendedHatchSpacing", "RecommendedLaserPower", "RecommendedLayerThickness", "RecommendedScanSpeed", "RequiredArgonPurity", "RequiredCertifications", "RequiredMachineType", "RequiredSkills", "RequiredTooling", "RequiresAS9100", "RequiresCertification", "RequiresFDA", "RequiresInspection", "RequiresNADCAP", "RequiresSupports", "SetupCost", "SetupTimeMinutes", "SlsMaterial", "StandardLaborCostPerHour", "StandardSellingPrice", "SupportRemovalTimeMinutes", "SupportStrategy", "SurfaceFinishRequirement", "ToleranceRequirements", "TotalJobsCompleted", "TotalUnitsProduced", "VolumeMm3", "WeightGrams", "WidthMm")
SELECT "Id", "AdminEstimatedHoursOverride", "AdminOverrideBy", "AdminOverrideDate", "AdminOverrideReason", "Application", "ArgonCostPerHour", "AverageActualHours", "AverageCostPerUnit", "AverageDefectRate", "AverageEfficiencyPercent", "AveragePowderUtilization", "AverageQualityScore", "AvgDuration", "AvgDurationDays", "BuildFileTemplate", "CadFilePath", "CadFileVersion", "ConsumableMaterials", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CustomerPartNumber", "Description", "Dimensions", "EstimatedHours", "HeightMm", "Industry", "IsActive", "LastModifiedBy", "LastModifiedDate", "LastProduced", "LengthMm", "MachineOperatingCostPerHour", "Material", "MaterialCostPerKg", "MaxOxygenContent", "MaxSurfaceRoughnessRa", "Name", "PartCategory", "PartClass", "PartNumber", "PostProcessingCost", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderRequirementKg", "PowderSpecification", "PreferredMachines", "PreheatingTimeMinutes", "ProcessParameters", "ProcessType", "QualityCheckpoints", "QualityInspectionCost", "QualityStandards", "RecommendedBuildTemperature", "RecommendedHatchSpacing", "RecommendedLaserPower", "RecommendedLayerThickness", "RecommendedScanSpeed", "RequiredArgonPurity", "RequiredCertifications", "RequiredMachineType", "RequiredSkills", "RequiredTooling", "RequiresAS9100", "RequiresCertification", "RequiresFDA", "RequiresInspection", "RequiresNADCAP", "RequiresSupports", "SetupCost", "SetupTimeMinutes", "SlsMaterial", "StandardLaborCostPerHour", "StandardSellingPrice", "SupportRemovalTimeMinutes", "SupportStrategy", "SurfaceFinishRequirement", "ToleranceRequirements", "TotalJobsCompleted", "TotalUnitsProduced", "VolumeMm3", "WeightGrams", "WidthMm"
FROM "Parts";

CREATE TABLE "ef_temp_InspectionCheckpoints" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_InspectionCheckpoints" PRIMARY KEY AUTOINCREMENT,
    "AcceptanceCriteria" TEXT NOT NULL,
    "Category" TEXT NOT NULL DEFAULT 'Quality',
    "CheckpointName" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "DefectCategoryId" INTEGER NULL,
    "Description" TEXT NOT NULL,
    "EstimatedMinutes" INTEGER NOT NULL DEFAULT 5,
    "FailureAction" TEXT NOT NULL DEFAULT 'Hold for review',
    "InspectionType" TEXT NOT NULL DEFAULT 'Visual',
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "IsRequired" INTEGER NOT NULL DEFAULT 1,
    "LastModifiedBy" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LowerTolerance" REAL NULL,
    "MeasurementMethod" TEXT NOT NULL,
    "Notes" TEXT NOT NULL DEFAULT '',
    "PartId" INTEGER NOT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "ReferenceDocuments" TEXT NOT NULL,
    "RequiredEquipment" TEXT NOT NULL,
    "RequiredSkills" TEXT NOT NULL,
    "SampleSize" INTEGER NOT NULL DEFAULT 1,
    "SamplingMethod" TEXT NOT NULL DEFAULT 'All',
    "SortOrder" INTEGER NOT NULL DEFAULT 100,
    "TargetValue" REAL NULL,
    "Unit" TEXT NOT NULL,
    "UpperTolerance" REAL NULL,
    CONSTRAINT "FK_InspectionCheckpoints_DefectCategories_DefectCategoryId" FOREIGN KEY ("DefectCategoryId") REFERENCES "DefectCategories" ("Id"),
    CONSTRAINT "FK_InspectionCheckpoints_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_InspectionCheckpoints" ("Id", "AcceptanceCriteria", "Category", "CheckpointName", "CreatedBy", "CreatedDate", "DefectCategoryId", "Description", "EstimatedMinutes", "FailureAction", "InspectionType", "IsActive", "IsRequired", "LastModifiedBy", "LastModifiedDate", "LowerTolerance", "MeasurementMethod", "Notes", "PartId", "Priority", "ReferenceDocuments", "RequiredEquipment", "RequiredSkills", "SampleSize", "SamplingMethod", "SortOrder", "TargetValue", "Unit", "UpperTolerance")
SELECT "Id", "AcceptanceCriteria", "Category", "CheckpointName", "CreatedBy", "CreatedDate", "DefectCategoryId", "Description", "EstimatedMinutes", "FailureAction", "InspectionType", "IsActive", "IsRequired", "LastModifiedBy", "LastModifiedDate", "LowerTolerance", "MeasurementMethod", "Notes", "PartId", "Priority", "ReferenceDocuments", "RequiredEquipment", "RequiredSkills", "SampleSize", "SamplingMethod", "SortOrder", "TargetValue", "Unit", "UpperTolerance"
FROM "InspectionCheckpoints";

CREATE TABLE "ef_temp_ArchivedJobs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ArchivedJobs" PRIMARY KEY AUTOINCREMENT,
    "ActualEnd" TEXT NULL,
    "ActualPowderUsageKg" REAL NOT NULL,
    "ActualStart" TEXT NULL,
    "ArchiveReason" TEXT NOT NULL DEFAULT 'Cleanup',
    "ArchivedBy" TEXT NOT NULL DEFAULT 'System',
    "ArchivedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "CustomerDueDate" TEXT NULL,
    "CustomerOrderNumber" TEXT NOT NULL,
    "DefectQuantity" INTEGER NOT NULL,
    "DensityPercentage" REAL NOT NULL,
    "EstimatedHours" REAL NOT NULL,
    "EstimatedPowderUsageKg" REAL NOT NULL,
    "HoldReason" TEXT NOT NULL DEFAULT '',
    "LaborCostPerHour" decimal(10,2) NOT NULL,
    "LaserPowerWatts" REAL NOT NULL,
    "LayerThicknessMicrons" REAL NOT NULL,
    "MachineId" TEXT NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "MaterialCostPerKg" decimal(10,2) NOT NULL,
    "Notes" TEXT NOT NULL DEFAULT '',
    "Operator" TEXT NOT NULL,
    "OriginalCreatedBy" TEXT NOT NULL,
    "OriginalCreatedDate" TEXT NOT NULL,
    "OriginalJobId" INTEGER NOT NULL,
    "OriginalLastModifiedBy" TEXT NOT NULL,
    "OriginalLastModifiedDate" TEXT NOT NULL,
    "PartDescription" TEXT NOT NULL,
    "PartId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "PowderLotNumber" TEXT NOT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "ProcessParameters" TEXT NOT NULL DEFAULT '{}',
    "ProducedQuantity" INTEGER NOT NULL,
    "QualityCheckpoints" TEXT NOT NULL DEFAULT '{}',
    "QualityInspector" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL DEFAULT 1,
    "ReworkQuantity" INTEGER NOT NULL,
    "ScanSpeedMmPerSec" REAL NOT NULL,
    "ScheduledEnd" TEXT NOT NULL,
    "ScheduledStart" TEXT NOT NULL,
    "SlsMaterial" TEXT NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Completed',
    "Supervisor" TEXT NOT NULL,
    "SurfaceRoughnessRa" REAL NOT NULL
);

INSERT INTO "ef_temp_ArchivedJobs" ("Id", "ActualEnd", "ActualPowderUsageKg", "ActualStart", "ArchiveReason", "ArchivedBy", "ArchivedDate", "ArgonCostPerHour", "CustomerDueDate", "CustomerOrderNumber", "DefectQuantity", "DensityPercentage", "EstimatedHours", "EstimatedPowderUsageKg", "HoldReason", "LaborCostPerHour", "LaserPowerWatts", "LayerThicknessMicrons", "MachineId", "MachineOperatingCostPerHour", "MaterialCostPerKg", "Notes", "Operator", "OriginalCreatedBy", "OriginalCreatedDate", "OriginalJobId", "OriginalLastModifiedBy", "OriginalLastModifiedDate", "PartDescription", "PartId", "PartNumber", "PowderLotNumber", "Priority", "ProcessParameters", "ProducedQuantity", "QualityCheckpoints", "QualityInspector", "Quantity", "ReworkQuantity", "ScanSpeedMmPerSec", "ScheduledEnd", "ScheduledStart", "SlsMaterial", "Status", "Supervisor", "SurfaceRoughnessRa")
SELECT "Id", "ActualEnd", "ActualPowderUsageKg", "ActualStart", "ArchiveReason", "ArchivedBy", "ArchivedDate", "ArgonCostPerHour", "CustomerDueDate", "CustomerOrderNumber", "DefectQuantity", "DensityPercentage", "EstimatedHours", "EstimatedPowderUsageKg", "HoldReason", "LaborCostPerHour", "LaserPowerWatts", "LayerThicknessMicrons", "MachineId", "MachineOperatingCostPerHour", "MaterialCostPerKg", "Notes", "Operator", "OriginalCreatedBy", "OriginalCreatedDate", "OriginalJobId", "OriginalLastModifiedBy", "OriginalLastModifiedDate", "PartDescription", "PartId", "PartNumber", "PowderLotNumber", "Priority", "ProcessParameters", "ProducedQuantity", "QualityCheckpoints", "QualityInspector", "Quantity", "ReworkQuantity", "ScanSpeedMmPerSec", "ScheduledEnd", "ScheduledStart", "SlsMaterial", "Status", "Supervisor", "SurfaceRoughnessRa"
FROM "ArchivedJobs";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "BuildJobParts";

ALTER TABLE "ef_temp_BuildJobParts" RENAME TO "BuildJobParts";

DROP TABLE "BuildJobs";

ALTER TABLE "ef_temp_BuildJobs" RENAME TO "BuildJobs";

DROP TABLE "DelayLogs";

ALTER TABLE "ef_temp_DelayLogs" RENAME TO "DelayLogs";

DROP TABLE "Jobs";

ALTER TABLE "ef_temp_Jobs" RENAME TO "Jobs";

DROP TABLE "MachineCapabilities";

ALTER TABLE "ef_temp_MachineCapabilities" RENAME TO "MachineCapabilities";

DROP TABLE "UserSettings";

ALTER TABLE "ef_temp_UserSettings" RENAME TO "UserSettings";

DROP TABLE "Users";

ALTER TABLE "ef_temp_Users" RENAME TO "Users";

DROP TABLE "Parts";

ALTER TABLE "ef_temp_Parts" RENAME TO "Parts";

DROP TABLE "InspectionCheckpoints";

ALTER TABLE "ef_temp_InspectionCheckpoints" RENAME TO "InspectionCheckpoints";

DROP TABLE "ArchivedJobs";

ALTER TABLE "ef_temp_ArchivedJobs" RENAME TO "ArchivedJobs";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

CREATE INDEX "IX_BuildJobParts_BuildJobBuildId" ON "BuildJobParts" ("BuildJobBuildId");

CREATE INDEX "IX_BuildJobs_UserId" ON "BuildJobs" ("UserId");

CREATE INDEX "IX_DelayLogs_BuildJobBuildId" ON "DelayLogs" ("BuildJobBuildId");

CREATE INDEX "IX_Jobs_MachineId_ScheduledStart" ON "Jobs" ("MachineId", "ScheduledStart");

CREATE INDEX "IX_Jobs_PartId" ON "Jobs" ("PartId");

CREATE INDEX "IX_Jobs_PartNumber" ON "Jobs" ("PartNumber");

CREATE INDEX "IX_Jobs_Priority" ON "Jobs" ("Priority");

CREATE INDEX "IX_Jobs_Status" ON "Jobs" ("Status");

CREATE INDEX "IX_MachineCapabilities_CapabilityType" ON "MachineCapabilities" ("CapabilityType");

CREATE INDEX "IX_MachineCapabilities_IsAvailable" ON "MachineCapabilities" ("IsAvailable");

CREATE INDEX "IX_MachineCapabilities_MachineId" ON "MachineCapabilities" ("MachineId");

CREATE INDEX "IX_MachineCapabilities_MachineId_CapabilityType" ON "MachineCapabilities" ("MachineId", "CapabilityType");

CREATE INDEX "IX_MachineCapabilities_MachineId1" ON "MachineCapabilities" ("MachineId1");

CREATE UNIQUE INDEX "IX_UserSettings_UserId" ON "UserSettings" ("UserId");

CREATE UNIQUE INDEX "IX_Parts_PartNumber" ON "Parts" ("PartNumber");

CREATE INDEX "IX_InspectionCheckpoints_DefectCategoryId" ON "InspectionCheckpoints" ("DefectCategoryId");

CREATE INDEX "IX_InspectionCheckpoints_InspectionType" ON "InspectionCheckpoints" ("InspectionType");

CREATE INDEX "IX_InspectionCheckpoints_IsActive" ON "InspectionCheckpoints" ("IsActive");

CREATE INDEX "IX_InspectionCheckpoints_IsRequired" ON "InspectionCheckpoints" ("IsRequired");

CREATE INDEX "IX_InspectionCheckpoints_PartId" ON "InspectionCheckpoints" ("PartId");

CREATE INDEX "IX_InspectionCheckpoints_PartId_SortOrder" ON "InspectionCheckpoints" ("PartId", "SortOrder");

CREATE INDEX "IX_ArchivedJobs_ArchivedBy" ON "ArchivedJobs" ("ArchivedBy");

CREATE INDEX "IX_ArchivedJobs_ArchivedDate" ON "ArchivedJobs" ("ArchivedDate");

CREATE INDEX "IX_ArchivedJobs_MachineId" ON "ArchivedJobs" ("MachineId");

CREATE INDEX "IX_ArchivedJobs_MachineId_ScheduledStart" ON "ArchivedJobs" ("MachineId", "ScheduledStart");

CREATE INDEX "IX_ArchivedJobs_OriginalJobId" ON "ArchivedJobs" ("OriginalJobId");

CREATE INDEX "IX_ArchivedJobs_PartNumber" ON "ArchivedJobs" ("PartNumber");

CREATE INDEX "IX_ArchivedJobs_Status" ON "ArchivedJobs" ("Status");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250726023548_FixJobStageMachineRelationship', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

CREATE TABLE "Materials" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Materials" PRIMARY KEY AUTOINCREMENT,
    "MaterialCode" TEXT NOT NULL,
    "MaterialName" TEXT NOT NULL,
    "MaterialType" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Density" REAL NOT NULL,
    "MeltingPointC" REAL NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "CostPerGram" TEXT NOT NULL,
    "DefaultLayerThicknessMicrons" REAL NOT NULL,
    "DefaultLaserPowerPercent" REAL NOT NULL,
    "DefaultScanSpeedMmPerSec" REAL NOT NULL,
    "MaterialProperties" TEXT NOT NULL,
    "CompatibleMachineTypes" TEXT NOT NULL,
    "SafetyNotes" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250726135340_AddMaterialsTable', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250726141952_MultiStageSchedulingEntities', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

ALTER TABLE "UserSettings" ADD "SchedulerOrientation" TEXT NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250726143534_AddSchedulerOrientationToUserSettings', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

DROP INDEX "IX_MachineCapabilities_MachineId1";

ALTER TABLE "JobStages" ADD "MachineId1" INTEGER NULL;

CREATE INDEX "IX_Parts_CreatedDate" ON "Parts" ("CreatedDate");

CREATE INDEX "IX_Parts_Industry" ON "Parts" ("Industry");

CREATE INDEX "IX_Parts_IsActive" ON "Parts" ("IsActive");

CREATE INDEX "IX_Parts_Material" ON "Parts" ("Material");

CREATE INDEX "IX_Parts_PartCategory" ON "Parts" ("PartCategory");

CREATE INDEX "IX_JobStages_MachineId1" ON "JobStages" ("MachineId1");

CREATE TABLE "ef_temp_JobStages" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_JobStages" PRIMARY KEY AUTOINCREMENT,
    "ActualCost" TEXT NULL,
    "ActualEnd" TEXT NULL,
    "ActualStart" TEXT NULL,
    "AllowParallel" INTEGER NOT NULL,
    "AssignedOperator" TEXT NULL,
    "CanStart" INTEGER NOT NULL,
    "CooldownTimeHours" REAL NOT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "Department" TEXT NOT NULL,
    "EstimatedCost" TEXT NOT NULL,
    "EstimatedDurationHours" REAL NOT NULL DEFAULT 1.0,
    "ExecutionOrder" INTEGER NOT NULL,
    "IsBlocking" INTEGER NOT NULL,
    "JobId" INTEGER NOT NULL,
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "MachineId" TEXT NULL,
    "MachineId1" INTEGER NULL,
    "Notes" TEXT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "ProgressPercent" REAL NOT NULL,
    "QualityRequirements" TEXT NULL,
    "RequiredMaterials" TEXT NULL,
    "RequiredTooling" TEXT NULL,
    "ScheduledEnd" TEXT NOT NULL,
    "ScheduledStart" TEXT NOT NULL,
    "SetupTimeHours" REAL NOT NULL,
    "StageName" TEXT NOT NULL,
    "StageType" TEXT NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Scheduled',
    CONSTRAINT "FK_JobStages_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES "Jobs" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_JobStages_Machines_MachineId1" FOREIGN KEY ("MachineId1") REFERENCES "Machines" ("Id")
);

INSERT INTO "ef_temp_JobStages" ("Id", "ActualCost", "ActualEnd", "ActualStart", "AllowParallel", "AssignedOperator", "CanStart", "CooldownTimeHours", "CreatedBy", "CreatedDate", "Department", "EstimatedCost", "EstimatedDurationHours", "ExecutionOrder", "IsBlocking", "JobId", "LastModifiedBy", "LastModifiedDate", "MachineId", "MachineId1", "Notes", "Priority", "ProgressPercent", "QualityRequirements", "RequiredMaterials", "RequiredTooling", "ScheduledEnd", "ScheduledStart", "SetupTimeHours", "StageName", "StageType", "Status")
SELECT "Id", "ActualCost", "ActualEnd", "ActualStart", "AllowParallel", "AssignedOperator", "CanStart", "CooldownTimeHours", "CreatedBy", "CreatedDate", "Department", "EstimatedCost", "EstimatedDurationHours", "ExecutionOrder", "IsBlocking", "JobId", "LastModifiedBy", "LastModifiedDate", "MachineId", "MachineId1", "Notes", "Priority", "ProgressPercent", "QualityRequirements", "RequiredMaterials", "RequiredTooling", "ScheduledEnd", "ScheduledStart", "SetupTimeHours", "StageName", "StageType", "Status"
FROM "JobStages";

CREATE TABLE "ef_temp_MachineCapabilities" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MachineCapabilities" PRIMARY KEY AUTOINCREMENT,
    "CapabilityName" TEXT NOT NULL,
    "CapabilityType" TEXT NOT NULL,
    "CapabilityValue" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "IsAvailable" INTEGER NOT NULL DEFAULT 1,
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "MachineId" INTEGER NOT NULL,
    "MaxValue" REAL NULL,
    "MinValue" REAL NULL,
    "Notes" TEXT NOT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "RequiredCertification" TEXT NOT NULL,
    "Unit" TEXT NOT NULL,
    CONSTRAINT "FK_MachineCapabilities_Machines_MachineId" FOREIGN KEY ("MachineId") REFERENCES "Machines" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_MachineCapabilities" ("Id", "CapabilityName", "CapabilityType", "CapabilityValue", "CreatedBy", "CreatedDate", "IsAvailable", "LastModifiedBy", "LastModifiedDate", "MachineId", "MaxValue", "MinValue", "Notes", "Priority", "RequiredCertification", "Unit")
SELECT "Id", "CapabilityName", "CapabilityType", "CapabilityValue", "CreatedBy", "CreatedDate", "IsAvailable", "LastModifiedBy", "LastModifiedDate", "MachineId", "MaxValue", "MinValue", "Notes", "Priority", "RequiredCertification", "Unit"
FROM "MachineCapabilities";

CREATE TABLE "ef_temp_Machines" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Machines" PRIMARY KEY AUTOINCREMENT,
    "AverageUtilizationPercent" REAL NOT NULL,
    "BuildHeightMm" REAL NOT NULL,
    "BuildLengthMm" REAL NOT NULL,
    "BuildWidthMm" REAL NOT NULL,
    "CommunicationSettings" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CurrentJobId" INTEGER NULL,
    "CurrentMaterial" TEXT NOT NULL,
    "Department" TEXT NOT NULL,
    "HoursSinceLastMaintenance" REAL NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "IsAvailableForScheduling" INTEGER NOT NULL,
    "LastMaintenanceDate" TEXT NULL,
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastStatusUpdate" TEXT NOT NULL,
    "Location" TEXT NOT NULL,
    "MachineId" TEXT NOT NULL,
    "MachineModel" TEXT NOT NULL,
    "MachineName" TEXT NOT NULL,
    "MachineType" TEXT NOT NULL,
    "MaintenanceIntervalHours" REAL NOT NULL,
    "MaintenanceNotes" TEXT NOT NULL,
    "MaxLaserPowerWatts" REAL NOT NULL,
    "MaxLayerThicknessMicrons" REAL NOT NULL,
    "MaxScanSpeedMmPerSec" REAL NOT NULL,
    "MinLayerThicknessMicrons" REAL NOT NULL,
    "Name" TEXT NOT NULL,
    "NextMaintenanceDate" TEXT NULL,
    "OpcUaEnabled" INTEGER NOT NULL,
    "OpcUaEndpointUrl" TEXT NOT NULL,
    "OperatorNotes" TEXT NOT NULL,
    "Priority" INTEGER NOT NULL,
    "SerialNumber" TEXT NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Idle',
    "SupportedMaterials" TEXT NOT NULL,
    "TechnicalSpecifications" TEXT NOT NULL,
    "TotalOperatingHours" REAL NOT NULL,
    CONSTRAINT "FK_Machines_Jobs_CurrentJobId" FOREIGN KEY ("CurrentJobId") REFERENCES "Jobs" ("Id")
);

INSERT INTO "ef_temp_Machines" ("Id", "AverageUtilizationPercent", "BuildHeightMm", "BuildLengthMm", "BuildWidthMm", "CommunicationSettings", "CreatedBy", "CreatedDate", "CurrentJobId", "CurrentMaterial", "Department", "HoursSinceLastMaintenance", "IsActive", "IsAvailableForScheduling", "LastMaintenanceDate", "LastModifiedBy", "LastModifiedDate", "LastStatusUpdate", "Location", "MachineId", "MachineModel", "MachineName", "MachineType", "MaintenanceIntervalHours", "MaintenanceNotes", "MaxLaserPowerWatts", "MaxLayerThicknessMicrons", "MaxScanSpeedMmPerSec", "MinLayerThicknessMicrons", "Name", "NextMaintenanceDate", "OpcUaEnabled", "OpcUaEndpointUrl", "OperatorNotes", "Priority", "SerialNumber", "Status", "SupportedMaterials", "TechnicalSpecifications", "TotalOperatingHours")
SELECT "Id", "AverageUtilizationPercent", "BuildHeightMm", "BuildLengthMm", "BuildWidthMm", "CommunicationSettings", "CreatedBy", "CreatedDate", "CurrentJobId", "CurrentMaterial", "Department", "HoursSinceLastMaintenance", "IsActive", "IsAvailableForScheduling", "LastMaintenanceDate", "LastModifiedBy", "LastModifiedDate", "LastStatusUpdate", "Location", "MachineId", "MachineModel", "MachineName", "MachineType", "MaintenanceIntervalHours", "MaintenanceNotes", "MaxLaserPowerWatts", "MaxLayerThicknessMicrons", "MaxScanSpeedMmPerSec", "MinLayerThicknessMicrons", "Name", "NextMaintenanceDate", "OpcUaEnabled", "OpcUaEndpointUrl", "OperatorNotes", "Priority", "SerialNumber", "Status", "SupportedMaterials", "TechnicalSpecifications", "TotalOperatingHours"
FROM "Machines";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "JobStages";

ALTER TABLE "ef_temp_JobStages" RENAME TO "JobStages";

DROP TABLE "MachineCapabilities";

ALTER TABLE "ef_temp_MachineCapabilities" RENAME TO "MachineCapabilities";

DROP TABLE "Machines";

ALTER TABLE "ef_temp_Machines" RENAME TO "Machines";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

CREATE INDEX "IX_JobStages_Department" ON "JobStages" ("Department");

CREATE INDEX "IX_JobStages_JobId" ON "JobStages" ("JobId");

CREATE INDEX "IX_JobStages_JobId_ExecutionOrder" ON "JobStages" ("JobId", "ExecutionOrder");

CREATE INDEX "IX_JobStages_MachineId" ON "JobStages" ("MachineId");

CREATE INDEX "IX_JobStages_MachineId1" ON "JobStages" ("MachineId1");

CREATE INDEX "IX_JobStages_ScheduledEnd" ON "JobStages" ("ScheduledEnd");

CREATE INDEX "IX_JobStages_ScheduledStart" ON "JobStages" ("ScheduledStart");

CREATE INDEX "IX_JobStages_StageType" ON "JobStages" ("StageType");

CREATE INDEX "IX_JobStages_Status" ON "JobStages" ("Status");

CREATE INDEX "IX_MachineCapabilities_CapabilityType" ON "MachineCapabilities" ("CapabilityType");

CREATE INDEX "IX_MachineCapabilities_IsAvailable" ON "MachineCapabilities" ("IsAvailable");

CREATE INDEX "IX_MachineCapabilities_MachineId" ON "MachineCapabilities" ("MachineId");

CREATE INDEX "IX_MachineCapabilities_MachineId_CapabilityType" ON "MachineCapabilities" ("MachineId", "CapabilityType");

CREATE INDEX "IX_Machines_CurrentJobId" ON "Machines" ("CurrentJobId");

CREATE INDEX "IX_Machines_IsActive" ON "Machines" ("IsActive");

CREATE UNIQUE INDEX "IX_Machines_MachineId" ON "Machines" ("MachineId");

CREATE INDEX "IX_Machines_MachineType" ON "Machines" ("MachineType");

CREATE INDEX "IX_Machines_Status" ON "Machines" ("Status");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250727124109_FixPartsTableSchema', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250727134953_MakeAdminOverrideReasonNullable', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250727141323_FixAdminOverrideFieldsNullability', '8.0.11');

COMMIT;

BEGIN TRANSACTION;


                CREATE INDEX IF NOT EXISTS IX_Jobs_ScheduledStart_ScheduledEnd 
                ON Jobs (ScheduledStart, ScheduledEnd);
            


                CREATE INDEX IF NOT EXISTS IX_Jobs_Status_Priority 
                ON Jobs (Status, Priority DESC);
            


                CREATE INDEX IF NOT EXISTS IX_Parts_Material_Industry 
                ON Parts (Material, Industry);
            


                CREATE INDEX IF NOT EXISTS IX_Parts_IsActive_EstimatedHours 
                ON Parts (IsActive DESC, EstimatedHours);
            


                CREATE INDEX IF NOT EXISTS IX_Parts_LastModifiedDate 
                ON Parts (LastModifiedDate DESC);
            


                CREATE INDEX IF NOT EXISTS IX_Users_Role_IsActive 
                ON Users (Role, IsActive DESC);
            


                CREATE INDEX IF NOT EXISTS IX_Machines_Status_IsActive 
                ON Machines (Status, IsActive DESC);
            

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250729205335_AddCorePerformanceIndexes', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

CREATE TABLE "ef_temp_Parts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Parts" PRIMARY KEY AUTOINCREMENT,
    "AdminEstimatedHoursOverride" REAL NULL,
    "AdminOverrideBy" TEXT NOT NULL DEFAULT '',
    "AdminOverrideDate" TEXT NULL,
    "AdminOverrideReason" TEXT NULL DEFAULT '',
    "Application" TEXT NOT NULL,
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "AverageActualHours" REAL NOT NULL,
    "AverageCostPerUnit" decimal(10,2) NOT NULL,
    "AverageDefectRate" REAL NOT NULL,
    "AverageEfficiencyPercent" REAL NOT NULL,
    "AveragePowderUtilization" REAL NOT NULL,
    "AverageQualityScore" REAL NOT NULL,
    "AvgDuration" TEXT NOT NULL DEFAULT '8h 0m',
    "AvgDurationDays" INTEGER NOT NULL,
    "BuildFileTemplate" TEXT NOT NULL DEFAULT '',
    "CadFilePath" TEXT NOT NULL DEFAULT '',
    "CadFileVersion" TEXT NOT NULL DEFAULT '',
    "ConsumableMaterials" TEXT NOT NULL DEFAULT 'Argon Gas,Build Platform Coating',
    "CoolingTimeMinutes" REAL NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CustomerPartNumber" TEXT NOT NULL DEFAULT '',
    "Description" TEXT NOT NULL,
    "Dimensions" TEXT NOT NULL DEFAULT '',
    "EstimatedHours" REAL NOT NULL,
    "HeightMm" REAL NOT NULL,
    "Industry" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastProduced" TEXT NULL,
    "LengthMm" REAL NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "Material" TEXT NOT NULL,
    "MaterialCostPerKg" decimal(12,2) NOT NULL,
    "MaxOxygenContent" REAL NOT NULL,
    "MaxSurfaceRoughnessRa" REAL NOT NULL,
    "Name" TEXT NOT NULL,
    "PartCategory" TEXT NOT NULL DEFAULT 'Prototype',
    "PartClass" TEXT NOT NULL DEFAULT 'B',
    "PartNumber" TEXT NOT NULL,
    "PostProcessingCost" decimal(10,2) NOT NULL,
    "PostProcessingTimeMinutes" REAL NOT NULL,
    "PowderChangeoverTimeMinutes" REAL NOT NULL,
    "PowderRequirementKg" REAL NOT NULL,
    "PowderSpecification" TEXT NOT NULL DEFAULT '15-45 micron particle size',
    "PreferredMachines" TEXT NOT NULL DEFAULT 'TI1,TI2',
    "PreheatingTimeMinutes" REAL NOT NULL,
    "ProcessParameters" TEXT NOT NULL DEFAULT '{}',
    "ProcessType" TEXT NOT NULL DEFAULT 'SLS Metal',
    "QualityCheckpoints" TEXT NOT NULL DEFAULT '{}',
    "QualityInspectionCost" decimal(10,2) NOT NULL,
    "QualityStandards" TEXT NOT NULL DEFAULT 'ASTM F3001, ISO 17296',
    "RecommendedBuildTemperature" REAL NOT NULL,
    "RecommendedHatchSpacing" REAL NOT NULL,
    "RecommendedLaserPower" REAL NOT NULL,
    "RecommendedLayerThickness" REAL NOT NULL,
    "RecommendedScanSpeed" REAL NOT NULL,
    "RequiredArgonPurity" REAL NOT NULL,
    "RequiredCertifications" TEXT NOT NULL DEFAULT 'SLS Operation Certification',
    "RequiredMachineType" TEXT NOT NULL DEFAULT 'TruPrint 3000',
    "RequiredSkills" TEXT NOT NULL DEFAULT 'SLS Operation,Powder Handling',
    "RequiredTooling" TEXT NOT NULL DEFAULT 'Build Platform,Powder Sieve',
    "RequiresAS9100" INTEGER NOT NULL,
    "RequiresCertification" INTEGER NOT NULL,
    "RequiresFDA" INTEGER NOT NULL,
    "RequiresInspection" INTEGER NOT NULL,
    "RequiresNADCAP" INTEGER NOT NULL,
    "RequiresSupports" INTEGER NOT NULL,
    "SetupCost" decimal(10,2) NOT NULL,
    "SetupTimeMinutes" REAL NOT NULL,
    "SlsMaterial" TEXT NOT NULL,
    "StandardLaborCostPerHour" decimal(10,2) NOT NULL,
    "StandardSellingPrice" decimal(10,2) NOT NULL,
    "SupportRemovalTimeMinutes" REAL NOT NULL,
    "SupportStrategy" TEXT NOT NULL DEFAULT 'Minimal supports on overhangs > 45°',
    "SurfaceFinishRequirement" TEXT NOT NULL DEFAULT 'As-built',
    "ToleranceRequirements" TEXT NOT NULL DEFAULT '±0.1mm typical',
    "TotalJobsCompleted" INTEGER NOT NULL,
    "TotalUnitsProduced" INTEGER NOT NULL,
    "VolumeMm3" REAL NOT NULL,
    "WeightGrams" REAL NOT NULL,
    "WidthMm" REAL NOT NULL
);

INSERT INTO "ef_temp_Parts" ("Id", "AdminEstimatedHoursOverride", "AdminOverrideBy", "AdminOverrideDate", "AdminOverrideReason", "Application", "ArgonCostPerHour", "AverageActualHours", "AverageCostPerUnit", "AverageDefectRate", "AverageEfficiencyPercent", "AveragePowderUtilization", "AverageQualityScore", "AvgDuration", "AvgDurationDays", "BuildFileTemplate", "CadFilePath", "CadFileVersion", "ConsumableMaterials", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CustomerPartNumber", "Description", "Dimensions", "EstimatedHours", "HeightMm", "Industry", "IsActive", "LastModifiedBy", "LastModifiedDate", "LastProduced", "LengthMm", "MachineOperatingCostPerHour", "Material", "MaterialCostPerKg", "MaxOxygenContent", "MaxSurfaceRoughnessRa", "Name", "PartCategory", "PartClass", "PartNumber", "PostProcessingCost", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderRequirementKg", "PowderSpecification", "PreferredMachines", "PreheatingTimeMinutes", "ProcessParameters", "ProcessType", "QualityCheckpoints", "QualityInspectionCost", "QualityStandards", "RecommendedBuildTemperature", "RecommendedHatchSpacing", "RecommendedLaserPower", "RecommendedLayerThickness", "RecommendedScanSpeed", "RequiredArgonPurity", "RequiredCertifications", "RequiredMachineType", "RequiredSkills", "RequiredTooling", "RequiresAS9100", "RequiresCertification", "RequiresFDA", "RequiresInspection", "RequiresNADCAP", "RequiresSupports", "SetupCost", "SetupTimeMinutes", "SlsMaterial", "StandardLaborCostPerHour", "StandardSellingPrice", "SupportRemovalTimeMinutes", "SupportStrategy", "SurfaceFinishRequirement", "ToleranceRequirements", "TotalJobsCompleted", "TotalUnitsProduced", "VolumeMm3", "WeightGrams", "WidthMm")
SELECT "Id", "AdminEstimatedHoursOverride", "AdminOverrideBy", "AdminOverrideDate", "AdminOverrideReason", "Application", "ArgonCostPerHour", "AverageActualHours", "AverageCostPerUnit", "AverageDefectRate", "AverageEfficiencyPercent", "AveragePowderUtilization", "AverageQualityScore", "AvgDuration", "AvgDurationDays", "BuildFileTemplate", "CadFilePath", "CadFileVersion", "ConsumableMaterials", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CustomerPartNumber", "Description", "Dimensions", "EstimatedHours", "HeightMm", "Industry", "IsActive", "LastModifiedBy", "LastModifiedDate", "LastProduced", "LengthMm", "MachineOperatingCostPerHour", "Material", "MaterialCostPerKg", "MaxOxygenContent", "MaxSurfaceRoughnessRa", "Name", "PartCategory", "PartClass", "PartNumber", "PostProcessingCost", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderRequirementKg", "PowderSpecification", "PreferredMachines", "PreheatingTimeMinutes", "ProcessParameters", "ProcessType", "QualityCheckpoints", "QualityInspectionCost", "QualityStandards", "RecommendedBuildTemperature", "RecommendedHatchSpacing", "RecommendedLaserPower", "RecommendedLayerThickness", "RecommendedScanSpeed", "RequiredArgonPurity", "RequiredCertifications", "RequiredMachineType", "RequiredSkills", "RequiredTooling", "RequiresAS9100", "RequiresCertification", "RequiresFDA", "RequiresInspection", "RequiresNADCAP", "RequiresSupports", "SetupCost", "SetupTimeMinutes", "SlsMaterial", "StandardLaborCostPerHour", "StandardSellingPrice", "SupportRemovalTimeMinutes", "SupportStrategy", "SurfaceFinishRequirement", "ToleranceRequirements", "TotalJobsCompleted", "TotalUnitsProduced", "VolumeMm3", "WeightGrams", "WidthMm"
FROM "Parts";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "Parts";

ALTER TABLE "ef_temp_Parts" RENAME TO "Parts";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

CREATE INDEX "IX_Parts_CreatedDate" ON "Parts" ("CreatedDate");

CREATE INDEX "IX_Parts_CustomerPartNumber" ON "Parts" ("CustomerPartNumber");

CREATE INDEX "IX_Parts_Industry" ON "Parts" ("Industry");

CREATE INDEX "IX_Parts_IsActive" ON "Parts" ("IsActive");

CREATE INDEX "IX_Parts_Material" ON "Parts" ("Material");

CREATE INDEX "IX_Parts_Name" ON "Parts" ("Name");

CREATE INDEX "IX_Parts_PartCategory" ON "Parts" ("PartCategory");

CREATE UNIQUE INDEX "IX_Parts_PartNumber" ON "Parts" ("PartNumber");

CREATE INDEX "IX_Parts_ProcessType" ON "Parts" ("ProcessType");

CREATE INDEX "IX_Parts_RequiredMachineType" ON "Parts" ("RequiredMachineType");

CREATE INDEX "IX_Parts_SlsMaterial" ON "Parts" ("SlsMaterial");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250729235317_FixRolePermissionKeys', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

ALTER TABLE "Parts" ADD "BTQualityStandards" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "BTRegulatoryNotes" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "BTTestingRequirements" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "ComponentType" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "ExportClassification" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "FirearmType" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "IsControlledItem" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "IsEARControlled" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "PartClassificationId" INTEGER NULL;

ALTER TABLE "Parts" ADD "RequiresATFCompliance" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresDimensionalVerification" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresFFLTracking" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresITARCompliance" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresMaterialCertification" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresPressureTesting" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresProofTesting" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresSerialization" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresSurfaceFinishVerification" INTEGER NOT NULL DEFAULT 0;

CREATE TABLE "PartClassifications" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartClassifications" PRIMARY KEY AUTOINCREMENT,
    "ClassificationCode" TEXT NOT NULL,
    "ClassificationName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "IndustryType" TEXT NOT NULL DEFAULT 'Firearms',
    "ComponentCategory" TEXT NOT NULL,
    "SuppressorType" TEXT NULL,
    "BafflePosition" TEXT NULL,
    "IsEndCap" INTEGER NOT NULL,
    "IsThreadMount" INTEGER NOT NULL,
    "IsTubeHousing" INTEGER NOT NULL,
    "IsInternalComponent" INTEGER NOT NULL,
    "IsMountingHardware" INTEGER NOT NULL,
    "FirearmType" TEXT NULL,
    "IsReceiver" INTEGER NOT NULL,
    "IsBarrelComponent" INTEGER NOT NULL,
    "IsOperatingSystem" INTEGER NOT NULL,
    "IsSafetyComponent" INTEGER NOT NULL,
    "IsTriggerComponent" INTEGER NOT NULL,
    "IsFurniture" INTEGER NOT NULL,
    "RecommendedMaterial" TEXT NOT NULL DEFAULT 'Ti-6Al-4V Grade 5',
    "AlternativeMaterials" TEXT NOT NULL DEFAULT '',
    "MaterialGrade" TEXT NOT NULL DEFAULT 'Aerospace',
    "RequiresSpecialHandling" INTEGER NOT NULL,
    "RequiredProcess" TEXT NOT NULL DEFAULT 'SLS Metal Printing',
    "PostProcessingRequired" TEXT NOT NULL DEFAULT '',
    "ComplexityLevel" INTEGER NOT NULL,
    "SpecialInstructions" TEXT NOT NULL DEFAULT '',
    "RequiresPressureTesting" INTEGER NOT NULL,
    "RequiresProofTesting" INTEGER NOT NULL,
    "RequiresDimensionalVerification" INTEGER NOT NULL,
    "RequiresSurfaceFinishVerification" INTEGER NOT NULL,
    "RequiresMaterialCertification" INTEGER NOT NULL,
    "TestingRequirements" TEXT NOT NULL DEFAULT '',
    "QualityStandards" TEXT NOT NULL DEFAULT '',
    "RequiresATFCompliance" INTEGER NOT NULL,
    "RequiresITARCompliance" INTEGER NOT NULL,
    "RequiresFFLTracking" INTEGER NOT NULL,
    "RequiresSerialization" INTEGER NOT NULL,
    "IsControlledItem" INTEGER NOT NULL,
    "IsEARControlled" INTEGER NOT NULL,
    "ExportClassification" TEXT NOT NULL DEFAULT '',
    "RegulatoryNotes" TEXT NOT NULL DEFAULT '',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL
);

CREATE TABLE "ComplianceRequirements" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ComplianceRequirements" PRIMARY KEY AUTOINCREMENT,
    "RequirementCode" TEXT NOT NULL,
    "RequirementName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "ComplianceType" TEXT NOT NULL,
    "RegulatoryAuthority" TEXT NOT NULL,
    "RequirementDetails" TEXT NOT NULL,
    "DocumentationRequired" TEXT NOT NULL DEFAULT '',
    "FormsRequired" TEXT NOT NULL DEFAULT '',
    "RecordKeepingRequirements" TEXT NOT NULL DEFAULT '',
    "ApplicableIndustries" TEXT NOT NULL DEFAULT '',
    "ApplicablePartTypes" TEXT NOT NULL DEFAULT '',
    "ApplicableProcesses" TEXT NOT NULL DEFAULT '',
    "AppliesToManufacturing" INTEGER NOT NULL,
    "AppliesToDistribution" INTEGER NOT NULL,
    "AppliesToExport" INTEGER NOT NULL,
    "AppliesToImport" INTEGER NOT NULL,
    "EnforcementLevel" TEXT NOT NULL DEFAULT 'Mandatory',
    "PenaltyType" TEXT NOT NULL DEFAULT '',
    "PenaltyDescription" TEXT NOT NULL DEFAULT '',
    "MaxPenaltyDays" INTEGER NOT NULL,
    "MaxPenaltyAmount" decimal(12,2) NOT NULL,
    "EffectiveDate" TEXT NULL,
    "ExpirationDate" TEXT NULL,
    "NextReviewDate" TEXT NULL,
    "RenewalIntervalMonths" INTEGER NOT NULL,
    "RequiresRenewal" INTEGER NOT NULL,
    "RequiresInspection" INTEGER NOT NULL,
    "RenewalProcess" TEXT NOT NULL DEFAULT '',
    "ImplementationSteps" TEXT NOT NULL DEFAULT '',
    "RequiredTraining" TEXT NOT NULL DEFAULT '',
    "RequiredCertifications" TEXT NOT NULL DEFAULT '',
    "SystemRequirements" TEXT NOT NULL DEFAULT '',
    "EstimatedImplementationHours" REAL NOT NULL,
    "EstimatedImplementationCost" decimal(10,2) NOT NULL,
    "ReferenceDocuments" TEXT NOT NULL DEFAULT '',
    "WebResources" TEXT NOT NULL DEFAULT '',
    "ContactInformation" TEXT NOT NULL DEFAULT '',
    "AdditionalNotes" TEXT NOT NULL DEFAULT '',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "IsCurrentVersion" INTEGER NOT NULL,
    "PartClassificationId" INTEGER NULL,
    CONSTRAINT "FK_ComplianceRequirements_PartClassifications_PartClassificationId" FOREIGN KEY ("PartClassificationId") REFERENCES "PartClassifications" ("Id") ON DELETE SET NULL
);

CREATE TABLE "SerialNumbers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SerialNumbers" PRIMARY KEY AUTOINCREMENT,
    "SerialNumberValue" TEXT NOT NULL,
    "SerialNumberFormat" TEXT NOT NULL,
    "ManufacturerCode" TEXT NOT NULL DEFAULT 'BT',
    "AssignedDate" TEXT NOT NULL,
    "ManufacturedDate" TEXT NULL,
    "CompletedDate" TEXT NULL,
    "AssignedJobId" TEXT NULL,
    "PartNumber" TEXT NULL,
    "ComponentName" TEXT NOT NULL DEFAULT '',
    "ComponentType" TEXT NOT NULL DEFAULT '',
    "ManufacturingMethod" TEXT NOT NULL DEFAULT 'SLS Metal Printing',
    "MaterialUsed" TEXT NOT NULL DEFAULT '',
    "MaterialLotNumber" TEXT NOT NULL DEFAULT '',
    "MachineUsed" TEXT NOT NULL DEFAULT '',
    "Operator" TEXT NOT NULL DEFAULT '',
    "QualityInspector" TEXT NOT NULL DEFAULT '',
    "ATFComplianceStatus" TEXT NOT NULL DEFAULT 'Pending',
    "ATFClassification" TEXT NOT NULL DEFAULT '',
    "FFLDealer" TEXT NULL,
    "FFLNumber" TEXT NULL,
    "ATFFormSubmissionDate" TEXT NULL,
    "ATFApprovalDate" TEXT NULL,
    "ATFFormNumbers" TEXT NOT NULL DEFAULT '',
    "TaxStampNumber" TEXT NULL,
    "TransferStatus" TEXT NOT NULL DEFAULT 'In Manufacturing',
    "TransferDate" TEXT NULL,
    "TransferTo" TEXT NULL,
    "TransferDocument" TEXT NULL,
    "TransferNotes" TEXT NULL,
    "IsDestructionScheduled" INTEGER NOT NULL,
    "ScheduledDestructionDate" TEXT NULL,
    "ActualDestructionDate" TEXT NULL,
    "DestructionMethod" TEXT NULL,
    "IsITARControlled" INTEGER NOT NULL,
    "IsEARControlled" INTEGER NOT NULL,
    "ExportClassification" TEXT NOT NULL DEFAULT '',
    "ExportLicense" TEXT NULL,
    "ExportLicenseExpiration" TEXT NULL,
    "DestinationCountry" TEXT NULL,
    "EndUser" TEXT NULL,
    "RequiresExportPermit" INTEGER NOT NULL,
    "ExportPermitObtained" INTEGER NOT NULL,
    "QualityStatus" TEXT NOT NULL DEFAULT 'Pending',
    "QualityInspectionDate" TEXT NULL,
    "QualityCertificateNumber" TEXT NULL,
    "TestResultsSummary" TEXT NOT NULL DEFAULT '',
    "DimensionalTestPassed" INTEGER NOT NULL,
    "MaterialTestPassed" INTEGER NOT NULL,
    "PressureTestPassed" INTEGER NOT NULL,
    "ProofTestPassed" INTEGER NOT NULL,
    "QualityNotes" TEXT NOT NULL DEFAULT '',
    "ManufacturingHistory" TEXT NOT NULL DEFAULT '{}',
    "ComponentGenealogy" TEXT NOT NULL DEFAULT '',
    "AssemblyComponents" TEXT NOT NULL DEFAULT '',
    "BatchNumber" TEXT NULL,
    "BuildPlatformId" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "IsLocked" INTEGER NOT NULL,
    "PartId" INTEGER NULL,
    "JobId" INTEGER NULL,
    "ComplianceRequirementId" INTEGER NULL,
    CONSTRAINT "FK_SerialNumbers_ComplianceRequirements_ComplianceRequirementId" FOREIGN KEY ("ComplianceRequirementId") REFERENCES "ComplianceRequirements" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_SerialNumbers_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES "Jobs" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_SerialNumbers_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE SET NULL
);

CREATE TABLE "ComplianceDocuments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ComplianceDocuments" PRIMARY KEY AUTOINCREMENT,
    "DocumentNumber" TEXT NOT NULL,
    "DocumentTitle" TEXT NOT NULL,
    "DocumentType" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "ComplianceCategory" TEXT NOT NULL,
    "DocumentClassification" TEXT NOT NULL DEFAULT 'Unclassified',
    "RegulatoryAuthority" TEXT NOT NULL DEFAULT '',
    "FormNumber" TEXT NULL,
    "DocumentDate" TEXT NOT NULL,
    "EffectiveDate" TEXT NULL,
    "ExpirationDate" TEXT NULL,
    "SubmissionDate" TEXT NULL,
    "ApprovalDate" TEXT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Draft',
    "ApprovalNumber" TEXT NULL,
    "ReferenceNumber" TEXT NULL,
    "FilePath" TEXT NULL,
    "FileName" TEXT NULL,
    "FileType" TEXT NULL,
    "FileSizeMB" decimal(8,2) NOT NULL,
    "FileHash" TEXT NULL,
    "DocumentContent" TEXT NOT NULL DEFAULT '',
    "AssociatedSerialNumbers" TEXT NOT NULL DEFAULT '',
    "AssociatedPartNumbers" TEXT NOT NULL DEFAULT '',
    "AssociatedJobNumbers" TEXT NOT NULL DEFAULT '',
    "Customer" TEXT NULL,
    "Vendor" TEXT NULL,
    "PreparedBy" TEXT NOT NULL DEFAULT '',
    "ReviewedBy" TEXT NULL,
    "ApprovedBy" TEXT NULL,
    "ReviewDate" TEXT NULL,
    "ApprovalDateInternal" TEXT NULL,
    "ReviewComments" TEXT NOT NULL DEFAULT '',
    "ApprovalComments" TEXT NOT NULL DEFAULT '',
    "RetentionPeriod" TEXT NOT NULL DEFAULT 'Permanent',
    "RetentionEndDate" TEXT NULL,
    "ArchiveDate" TEXT NULL,
    "DisposalDate" TEXT NULL,
    "ArchiveLocation" TEXT NULL,
    "DisposalMethod" TEXT NULL,
    "IsArchived" INTEGER NOT NULL,
    "IsDisposed" INTEGER NOT NULL,
    "RequiresRenewal" INTEGER NOT NULL,
    "RenewalReminderDays" INTEGER NOT NULL,
    "NextReminderDate" TEXT NULL,
    "EmailNotificationSent" INTEGER NOT NULL,
    "LastNotificationDate" TEXT NULL,
    "NotificationRecipients" TEXT NOT NULL DEFAULT '',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastAccessedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "LastAccessedBy" TEXT NOT NULL DEFAULT '',
    "AccessCount" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "AuditNotes" TEXT NOT NULL DEFAULT '',
    "SerialNumberId" INTEGER NULL,
    "ComplianceRequirementId" INTEGER NULL,
    "PartId" INTEGER NULL,
    "JobId" INTEGER NULL,
    CONSTRAINT "FK_ComplianceDocuments_ComplianceRequirements_ComplianceRequirementId" FOREIGN KEY ("ComplianceRequirementId") REFERENCES "ComplianceRequirements" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_ComplianceDocuments_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES "Jobs" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_ComplianceDocuments_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_ComplianceDocuments_SerialNumbers_SerialNumberId" FOREIGN KEY ("SerialNumberId") REFERENCES "SerialNumbers" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_Parts_ComponentType" ON "Parts" ("ComponentType");

CREATE INDEX "IX_Parts_FirearmType" ON "Parts" ("FirearmType");

CREATE INDEX "IX_Parts_PartClassificationId" ON "Parts" ("PartClassificationId");

CREATE INDEX "IX_Parts_RequiresATFCompliance" ON "Parts" ("RequiresATFCompliance");

CREATE INDEX "IX_Parts_RequiresITARCompliance" ON "Parts" ("RequiresITARCompliance");

CREATE INDEX "IX_Parts_RequiresSerialization" ON "Parts" ("RequiresSerialization");

CREATE INDEX "IX_ComplianceDocuments_ComplianceCategory" ON "ComplianceDocuments" ("ComplianceCategory");

CREATE INDEX "IX_ComplianceDocuments_ComplianceRequirementId" ON "ComplianceDocuments" ("ComplianceRequirementId");

CREATE INDEX "IX_ComplianceDocuments_DocumentDate" ON "ComplianceDocuments" ("DocumentDate");

CREATE UNIQUE INDEX "IX_ComplianceDocuments_DocumentNumber" ON "ComplianceDocuments" ("DocumentNumber");

CREATE INDEX "IX_ComplianceDocuments_DocumentType" ON "ComplianceDocuments" ("DocumentType");

CREATE INDEX "IX_ComplianceDocuments_EffectiveDate" ON "ComplianceDocuments" ("EffectiveDate");

CREATE INDEX "IX_ComplianceDocuments_ExpirationDate" ON "ComplianceDocuments" ("ExpirationDate");

CREATE INDEX "IX_ComplianceDocuments_IsActive" ON "ComplianceDocuments" ("IsActive");

CREATE INDEX "IX_ComplianceDocuments_IsArchived" ON "ComplianceDocuments" ("IsArchived");

CREATE INDEX "IX_ComplianceDocuments_JobId" ON "ComplianceDocuments" ("JobId");

CREATE INDEX "IX_ComplianceDocuments_PartId" ON "ComplianceDocuments" ("PartId");

CREATE INDEX "IX_ComplianceDocuments_SerialNumberId" ON "ComplianceDocuments" ("SerialNumberId");

CREATE INDEX "IX_ComplianceDocuments_Status" ON "ComplianceDocuments" ("Status");

CREATE INDEX "IX_ComplianceRequirements_ComplianceType" ON "ComplianceRequirements" ("ComplianceType");

CREATE INDEX "IX_ComplianceRequirements_EffectiveDate" ON "ComplianceRequirements" ("EffectiveDate");

CREATE INDEX "IX_ComplianceRequirements_EnforcementLevel" ON "ComplianceRequirements" ("EnforcementLevel");

CREATE INDEX "IX_ComplianceRequirements_ExpirationDate" ON "ComplianceRequirements" ("ExpirationDate");

CREATE INDEX "IX_ComplianceRequirements_IsActive" ON "ComplianceRequirements" ("IsActive");

CREATE INDEX "IX_ComplianceRequirements_IsCurrentVersion" ON "ComplianceRequirements" ("IsCurrentVersion");

CREATE INDEX "IX_ComplianceRequirements_PartClassificationId" ON "ComplianceRequirements" ("PartClassificationId");

CREATE INDEX "IX_ComplianceRequirements_RegulatoryAuthority" ON "ComplianceRequirements" ("RegulatoryAuthority");

CREATE UNIQUE INDEX "IX_ComplianceRequirements_RequirementCode" ON "ComplianceRequirements" ("RequirementCode");

CREATE UNIQUE INDEX "IX_PartClassifications_ClassificationCode" ON "PartClassifications" ("ClassificationCode");

CREATE INDEX "IX_PartClassifications_ClassificationName" ON "PartClassifications" ("ClassificationName");

CREATE INDEX "IX_PartClassifications_ComponentCategory" ON "PartClassifications" ("ComponentCategory");

CREATE INDEX "IX_PartClassifications_FirearmType" ON "PartClassifications" ("FirearmType");

CREATE INDEX "IX_PartClassifications_IndustryType" ON "PartClassifications" ("IndustryType");

CREATE INDEX "IX_PartClassifications_IsActive" ON "PartClassifications" ("IsActive");

CREATE INDEX "IX_PartClassifications_RequiresATFCompliance" ON "PartClassifications" ("RequiresATFCompliance");

CREATE INDEX "IX_PartClassifications_RequiresITARCompliance" ON "PartClassifications" ("RequiresITARCompliance");

CREATE INDEX "IX_PartClassifications_RequiresSerialization" ON "PartClassifications" ("RequiresSerialization");

CREATE INDEX "IX_PartClassifications_SuppressorType" ON "PartClassifications" ("SuppressorType");

CREATE INDEX "IX_SerialNumbers_AssignedDate" ON "SerialNumbers" ("AssignedDate");

CREATE INDEX "IX_SerialNumbers_ATFComplianceStatus" ON "SerialNumbers" ("ATFComplianceStatus");

CREATE INDEX "IX_SerialNumbers_ComplianceRequirementId" ON "SerialNumbers" ("ComplianceRequirementId");

CREATE INDEX "IX_SerialNumbers_ComponentType" ON "SerialNumbers" ("ComponentType");

CREATE INDEX "IX_SerialNumbers_IsActive" ON "SerialNumbers" ("IsActive");

CREATE INDEX "IX_SerialNumbers_IsLocked" ON "SerialNumbers" ("IsLocked");

CREATE INDEX "IX_SerialNumbers_JobId" ON "SerialNumbers" ("JobId");

CREATE INDEX "IX_SerialNumbers_ManufacturedDate" ON "SerialNumbers" ("ManufacturedDate");

CREATE INDEX "IX_SerialNumbers_ManufacturerCode" ON "SerialNumbers" ("ManufacturerCode");

CREATE INDEX "IX_SerialNumbers_PartId" ON "SerialNumbers" ("PartId");

CREATE INDEX "IX_SerialNumbers_QualityStatus" ON "SerialNumbers" ("QualityStatus");

CREATE UNIQUE INDEX "IX_SerialNumbers_SerialNumberValue" ON "SerialNumbers" ("SerialNumberValue");

CREATE INDEX "IX_SerialNumbers_TransferStatus" ON "SerialNumbers" ("TransferStatus");

CREATE TABLE "ef_temp_Parts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Parts" PRIMARY KEY AUTOINCREMENT,
    "AdminEstimatedHoursOverride" REAL NULL,
    "AdminOverrideBy" TEXT NOT NULL DEFAULT '',
    "AdminOverrideDate" TEXT NULL,
    "AdminOverrideReason" TEXT NULL DEFAULT '',
    "Application" TEXT NOT NULL,
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "AverageActualHours" REAL NOT NULL,
    "AverageCostPerUnit" decimal(10,2) NOT NULL,
    "AverageDefectRate" REAL NOT NULL,
    "AverageEfficiencyPercent" REAL NOT NULL,
    "AveragePowderUtilization" REAL NOT NULL,
    "AverageQualityScore" REAL NOT NULL,
    "AvgDuration" TEXT NOT NULL DEFAULT '8h 0m',
    "AvgDurationDays" INTEGER NOT NULL,
    "BTQualityStandards" TEXT NOT NULL DEFAULT '',
    "BTRegulatoryNotes" TEXT NOT NULL DEFAULT '',
    "BTTestingRequirements" TEXT NOT NULL DEFAULT '',
    "BuildFileTemplate" TEXT NOT NULL DEFAULT '',
    "CadFilePath" TEXT NOT NULL DEFAULT '',
    "CadFileVersion" TEXT NOT NULL DEFAULT '',
    "ComponentType" TEXT NOT NULL DEFAULT '',
    "ConsumableMaterials" TEXT NOT NULL DEFAULT 'Argon Gas,Build Platform Coating',
    "CoolingTimeMinutes" REAL NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CustomerPartNumber" TEXT NOT NULL DEFAULT '',
    "Description" TEXT NOT NULL,
    "Dimensions" TEXT NOT NULL DEFAULT '',
    "EstimatedHours" REAL NOT NULL,
    "ExportClassification" TEXT NOT NULL DEFAULT '',
    "FirearmType" TEXT NOT NULL DEFAULT '',
    "HeightMm" REAL NOT NULL,
    "Industry" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "IsControlledItem" INTEGER NOT NULL,
    "IsEARControlled" INTEGER NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastProduced" TEXT NULL,
    "LengthMm" REAL NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "Material" TEXT NOT NULL,
    "MaterialCostPerKg" decimal(12,2) NOT NULL,
    "MaxOxygenContent" REAL NOT NULL,
    "MaxSurfaceRoughnessRa" REAL NOT NULL,
    "Name" TEXT NOT NULL,
    "PartCategory" TEXT NOT NULL DEFAULT 'Prototype',
    "PartClass" TEXT NOT NULL DEFAULT 'B',
    "PartClassificationId" INTEGER NULL,
    "PartNumber" TEXT NOT NULL,
    "PostProcessingCost" decimal(10,2) NOT NULL,
    "PostProcessingTimeMinutes" REAL NOT NULL,
    "PowderChangeoverTimeMinutes" REAL NOT NULL,
    "PowderRequirementKg" REAL NOT NULL,
    "PowderSpecification" TEXT NOT NULL DEFAULT '15-45 micron particle size',
    "PreferredMachines" TEXT NOT NULL DEFAULT 'TI1,TI2',
    "PreheatingTimeMinutes" REAL NOT NULL,
    "ProcessParameters" TEXT NOT NULL DEFAULT '{}',
    "ProcessType" TEXT NOT NULL DEFAULT 'SLS Metal',
    "QualityCheckpoints" TEXT NOT NULL DEFAULT '{}',
    "QualityInspectionCost" decimal(10,2) NOT NULL,
    "QualityStandards" TEXT NOT NULL DEFAULT 'ASTM F3001, ISO 17296',
    "RecommendedBuildTemperature" REAL NOT NULL,
    "RecommendedHatchSpacing" REAL NOT NULL,
    "RecommendedLaserPower" REAL NOT NULL,
    "RecommendedLayerThickness" REAL NOT NULL,
    "RecommendedScanSpeed" REAL NOT NULL,
    "RequiredArgonPurity" REAL NOT NULL,
    "RequiredCertifications" TEXT NOT NULL DEFAULT 'SLS Operation Certification',
    "RequiredMachineType" TEXT NOT NULL DEFAULT 'TruPrint 3000',
    "RequiredSkills" TEXT NOT NULL DEFAULT 'SLS Operation,Powder Handling',
    "RequiredTooling" TEXT NOT NULL DEFAULT 'Build Platform,Powder Sieve',
    "RequiresAS9100" INTEGER NOT NULL,
    "RequiresATFCompliance" INTEGER NOT NULL,
    "RequiresCertification" INTEGER NOT NULL,
    "RequiresDimensionalVerification" INTEGER NOT NULL,
    "RequiresFDA" INTEGER NOT NULL,
    "RequiresFFLTracking" INTEGER NOT NULL,
    "RequiresITARCompliance" INTEGER NOT NULL,
    "RequiresInspection" INTEGER NOT NULL,
    "RequiresMaterialCertification" INTEGER NOT NULL,
    "RequiresNADCAP" INTEGER NOT NULL,
    "RequiresPressureTesting" INTEGER NOT NULL,
    "RequiresProofTesting" INTEGER NOT NULL,
    "RequiresSerialization" INTEGER NOT NULL,
    "RequiresSupports" INTEGER NOT NULL,
    "RequiresSurfaceFinishVerification" INTEGER NOT NULL,
    "SetupCost" decimal(10,2) NOT NULL,
    "SetupTimeMinutes" REAL NOT NULL,
    "SlsMaterial" TEXT NOT NULL,
    "StandardLaborCostPerHour" decimal(10,2) NOT NULL,
    "StandardSellingPrice" decimal(10,2) NOT NULL,
    "SupportRemovalTimeMinutes" REAL NOT NULL,
    "SupportStrategy" TEXT NOT NULL DEFAULT 'Minimal supports on overhangs > 45°',
    "SurfaceFinishRequirement" TEXT NOT NULL DEFAULT 'As-built',
    "ToleranceRequirements" TEXT NOT NULL DEFAULT '±0.1mm typical',
    "TotalJobsCompleted" INTEGER NOT NULL,
    "TotalUnitsProduced" INTEGER NOT NULL,
    "VolumeMm3" REAL NOT NULL,
    "WeightGrams" REAL NOT NULL,
    "WidthMm" REAL NOT NULL,
    CONSTRAINT "FK_Parts_PartClassifications_PartClassificationId" FOREIGN KEY ("PartClassificationId") REFERENCES "PartClassifications" ("Id") ON DELETE SET NULL
);

INSERT INTO "ef_temp_Parts" ("Id", "AdminEstimatedHoursOverride", "AdminOverrideBy", "AdminOverrideDate", "AdminOverrideReason", "Application", "ArgonCostPerHour", "AverageActualHours", "AverageCostPerUnit", "AverageDefectRate", "AverageEfficiencyPercent", "AveragePowderUtilization", "AverageQualityScore", "AvgDuration", "AvgDurationDays", "BTQualityStandards", "BTRegulatoryNotes", "BTTestingRequirements", "BuildFileTemplate", "CadFilePath", "CadFileVersion", "ComponentType", "ConsumableMaterials", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CustomerPartNumber", "Description", "Dimensions", "EstimatedHours", "ExportClassification", "FirearmType", "HeightMm", "Industry", "IsActive", "IsControlledItem", "IsEARControlled", "LastModifiedBy", "LastModifiedDate", "LastProduced", "LengthMm", "MachineOperatingCostPerHour", "Material", "MaterialCostPerKg", "MaxOxygenContent", "MaxSurfaceRoughnessRa", "Name", "PartCategory", "PartClass", "PartClassificationId", "PartNumber", "PostProcessingCost", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderRequirementKg", "PowderSpecification", "PreferredMachines", "PreheatingTimeMinutes", "ProcessParameters", "ProcessType", "QualityCheckpoints", "QualityInspectionCost", "QualityStandards", "RecommendedBuildTemperature", "RecommendedHatchSpacing", "RecommendedLaserPower", "RecommendedLayerThickness", "RecommendedScanSpeed", "RequiredArgonPurity", "RequiredCertifications", "RequiredMachineType", "RequiredSkills", "RequiredTooling", "RequiresAS9100", "RequiresATFCompliance", "RequiresCertification", "RequiresDimensionalVerification", "RequiresFDA", "RequiresFFLTracking", "RequiresITARCompliance", "RequiresInspection", "RequiresMaterialCertification", "RequiresNADCAP", "RequiresPressureTesting", "RequiresProofTesting", "RequiresSerialization", "RequiresSupports", "RequiresSurfaceFinishVerification", "SetupCost", "SetupTimeMinutes", "SlsMaterial", "StandardLaborCostPerHour", "StandardSellingPrice", "SupportRemovalTimeMinutes", "SupportStrategy", "SurfaceFinishRequirement", "ToleranceRequirements", "TotalJobsCompleted", "TotalUnitsProduced", "VolumeMm3", "WeightGrams", "WidthMm")
SELECT "Id", "AdminEstimatedHoursOverride", "AdminOverrideBy", "AdminOverrideDate", "AdminOverrideReason", "Application", "ArgonCostPerHour", "AverageActualHours", "AverageCostPerUnit", "AverageDefectRate", "AverageEfficiencyPercent", "AveragePowderUtilization", "AverageQualityScore", "AvgDuration", "AvgDurationDays", "BTQualityStandards", "BTRegulatoryNotes", "BTTestingRequirements", "BuildFileTemplate", "CadFilePath", "CadFileVersion", "ComponentType", "ConsumableMaterials", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CustomerPartNumber", "Description", "Dimensions", "EstimatedHours", "ExportClassification", "FirearmType", "HeightMm", "Industry", "IsActive", "IsControlledItem", "IsEARControlled", "LastModifiedBy", "LastModifiedDate", "LastProduced", "LengthMm", "MachineOperatingCostPerHour", "Material", "MaterialCostPerKg", "MaxOxygenContent", "MaxSurfaceRoughnessRa", "Name", "PartCategory", "PartClass", "PartClassificationId", "PartNumber", "PostProcessingCost", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderRequirementKg", "PowderSpecification", "PreferredMachines", "PreheatingTimeMinutes", "ProcessParameters", "ProcessType", "QualityCheckpoints", "QualityInspectionCost", "QualityStandards", "RecommendedBuildTemperature", "RecommendedHatchSpacing", "RecommendedLaserPower", "RecommendedLayerThickness", "RecommendedScanSpeed", "RequiredArgonPurity", "RequiredCertifications", "RequiredMachineType", "RequiredSkills", "RequiredTooling", "RequiresAS9100", "RequiresATFCompliance", "RequiresCertification", "RequiresDimensionalVerification", "RequiresFDA", "RequiresFFLTracking", "RequiresITARCompliance", "RequiresInspection", "RequiresMaterialCertification", "RequiresNADCAP", "RequiresPressureTesting", "RequiresProofTesting", "RequiresSerialization", "RequiresSupports", "RequiresSurfaceFinishVerification", "SetupCost", "SetupTimeMinutes", "SlsMaterial", "StandardLaborCostPerHour", "StandardSellingPrice", "SupportRemovalTimeMinutes", "SupportStrategy", "SurfaceFinishRequirement", "ToleranceRequirements", "TotalJobsCompleted", "TotalUnitsProduced", "VolumeMm3", "WeightGrams", "WidthMm"
FROM "Parts";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "Parts";

ALTER TABLE "ef_temp_Parts" RENAME TO "Parts";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

CREATE INDEX "IX_Parts_ComponentType" ON "Parts" ("ComponentType");

CREATE INDEX "IX_Parts_CreatedDate" ON "Parts" ("CreatedDate");

CREATE INDEX "IX_Parts_CustomerPartNumber" ON "Parts" ("CustomerPartNumber");

CREATE INDEX "IX_Parts_FirearmType" ON "Parts" ("FirearmType");

CREATE INDEX "IX_Parts_Industry" ON "Parts" ("Industry");

CREATE INDEX "IX_Parts_IsActive" ON "Parts" ("IsActive");

CREATE INDEX "IX_Parts_Material" ON "Parts" ("Material");

CREATE INDEX "IX_Parts_Name" ON "Parts" ("Name");

CREATE INDEX "IX_Parts_PartCategory" ON "Parts" ("PartCategory");

CREATE INDEX "IX_Parts_PartClassificationId" ON "Parts" ("PartClassificationId");

CREATE UNIQUE INDEX "IX_Parts_PartNumber" ON "Parts" ("PartNumber");

CREATE INDEX "IX_Parts_ProcessType" ON "Parts" ("ProcessType");

CREATE INDEX "IX_Parts_RequiredMachineType" ON "Parts" ("RequiredMachineType");

CREATE INDEX "IX_Parts_RequiresATFCompliance" ON "Parts" ("RequiresATFCompliance");

CREATE INDEX "IX_Parts_RequiresITARCompliance" ON "Parts" ("RequiresITARCompliance");

CREATE INDEX "IX_Parts_RequiresSerialization" ON "Parts" ("RequiresSerialization");

CREATE INDEX "IX_Parts_SlsMaterial" ON "Parts" ("SlsMaterial");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250730015507_AddBTIndustrySpecialization', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

CREATE INDEX "IX_Parts_BTCompliance_Composite" ON "Parts" ("RequiresATFCompliance", "RequiresITARCompliance", "RequiresSerialization", "IsActive");

CREATE INDEX "IX_Parts_ComponentFirearm_Composite" ON "Parts" ("ComponentType", "FirearmType", "IsActive");

CREATE INDEX "IX_Parts_Classification_Material" ON "Parts" ("PartClassificationId", "Material", "IsActive");

CREATE INDEX "IX_Parts_BTSearch_Composite" ON "Parts" ("PartNumber", "ComponentType", "FirearmType", "ExportClassification");

CREATE INDEX "IX_SerialNumbers_Part_Status" ON "SerialNumbers" ("PartId", "ATFComplianceStatus", "QualityStatus", "IsActive");

CREATE INDEX "IX_ComplianceDocuments_Part_Status" ON "ComplianceDocuments" ("PartId", "Status", "IsActive");

CREATE INDEX "IX_PartClassifications_Industry_Category" ON "PartClassifications" ("IndustryType", "ComponentCategory", "IsActive");

CREATE INDEX "IX_Parts_PartClassification_Enhanced" ON "Parts" ("PartClassificationId", "ComponentType", "IsActive");

CREATE INDEX "IX_SerialNumbers_Tracking_Enhanced" ON "SerialNumbers" ("PartId", "AssignedDate", "ATFComplianceStatus", "IsActive");

CREATE INDEX "IX_ComplianceDocuments_Relationships" ON "ComplianceDocuments" ("PartId", "SerialNumberId", "ComplianceRequirementId", "Status", "IsActive");

CREATE INDEX "IX_Parts_CrossReference" ON "Parts" ("PartNumber", "CustomerPartNumber", "PartClassificationId", "IsActive");

CREATE INDEX "IX_Parts_BTTesting_Composite" ON "Parts" ("RequiresPressureTesting", "RequiresProofTesting", "RequiresDimensionalVerification", "IsActive");

CREATE INDEX "IX_Parts_ExportControl_Composite" ON "Parts" ("IsEARControlled", "IsControlledItem", "ExportClassification", "IsActive");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250730024931_OptimizeBTPartsIndexes', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

ALTER TABLE "Parts" ADD "ATFClassification" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "ApprovalWorkflow" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "BTBackPressurePSI" REAL NULL;

ALTER TABLE "Parts" ADD "BTBafflePosition" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "BTCaliberCompatibility" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "BTComponentType" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "BTFirearmCategory" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "BTLicensingCost" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Parts" ADD "BTQualitySpecification" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "BTSoundReductionDB" REAL NULL;

ALTER TABLE "Parts" ADD "BTSuppressorType" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "BTTestingProtocol" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "BTThreadPitch" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "BatchControlMethod" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "ChildComponents" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "ComplianceCost" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Parts" ADD "DocumentationCost" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Parts" ADD "EARClassification" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "ExportControlNotes" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "FFLRequirements" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "ITARCategory" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "IsAssemblyComponent" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "IsSubAssembly" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "ManufacturingStage" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "MaxBatchSize" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "ParentComponents" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "ProofTestPressure" REAL NULL;

ALTER TABLE "Parts" ADD "RequiresATFForm1" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresATFForm4" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresAssembly" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresBTProofTesting" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresBackPressureTesting" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresCNCMachining" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresComplianceApproval" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresEDMOperations" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresEngineeringApproval" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresExportLicense" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresFinishing" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresQualityApproval" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresSLSPrinting" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresSoundTesting" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresTaxStamp" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresThreadVerification" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresTraceabilityDocuments" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "RequiresUniqueSerialNumber" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "SerialNumberFormat" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "StageDetails" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Parts" ADD "StageOrder" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Parts" ADD "TaxStampAmount" decimal(10,2) NULL;

ALTER TABLE "Parts" ADD "TestingCost" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "Parts" ADD "WorkflowTemplate" TEXT NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250730145024_BTPartsSystemEnhancement', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

CREATE TABLE "ProductionStages" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductionStages" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "DisplayOrder" INTEGER NOT NULL,
    "Description" TEXT NULL,
    "DefaultSetupMinutes" INTEGER NOT NULL DEFAULT 30,
    "DefaultHourlyRate" decimal(8,2) NOT NULL DEFAULT '85.0',
    "RequiresQualityCheck" INTEGER NOT NULL DEFAULT 1,
    "RequiresApproval" INTEGER NOT NULL DEFAULT 0,
    "AllowSkip" INTEGER NOT NULL DEFAULT 0,
    "IsOptional" INTEGER NOT NULL DEFAULT 0,
    "RequiredRole" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "IsActive" INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE "PrototypeJobs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PrototypeJobs" PRIMARY KEY AUTOINCREMENT,
    "PartId" INTEGER NOT NULL,
    "PrototypeNumber" TEXT NOT NULL,
    "CustomerOrderNumber" TEXT NULL,
    "RequestedBy" TEXT NOT NULL,
    "RequestDate" TEXT NOT NULL,
    "Priority" TEXT NOT NULL DEFAULT 'Standard',
    "Status" TEXT NOT NULL DEFAULT 'InProgress',
    "TotalActualCost" decimal(12,2) NOT NULL,
    "TotalEstimatedCost" decimal(12,2) NOT NULL,
    "CostVariancePercent" decimal(5,2) NOT NULL,
    "TotalActualHours" decimal(8,2) NOT NULL,
    "TotalEstimatedHours" decimal(8,2) NOT NULL,
    "TimeVariancePercent" decimal(5,2) NOT NULL,
    "StartDate" TEXT NULL,
    "CompletionDate" TEXT NULL,
    "LeadTimeDays" INTEGER NULL,
    "AdminReviewStatus" TEXT NOT NULL DEFAULT 'Pending',
    "AdminReviewBy" TEXT NULL,
    "AdminReviewDate" TEXT NULL,
    "AdminReviewNotes" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "UpdatedBy" TEXT NULL,
    "UpdatedDate" TEXT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_PrototypeJobs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "AssemblyComponents" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AssemblyComponents" PRIMARY KEY AUTOINCREMENT,
    "PrototypeJobId" INTEGER NOT NULL,
    "ComponentType" TEXT NOT NULL,
    "ComponentPartNumber" TEXT NULL,
    "ComponentDescription" TEXT NOT NULL,
    "QuantityRequired" INTEGER NOT NULL DEFAULT 1,
    "QuantityUsed" INTEGER NOT NULL DEFAULT 0,
    "UnitCost" decimal(8,2) NULL,
    "TotalCost" decimal(10,2) NULL,
    "Supplier" TEXT NULL,
    "SupplierPartNumber" TEXT NULL,
    "LeadTimeDays" INTEGER NULL,
    "Status" TEXT NOT NULL DEFAULT 'Needed',
    "OrderDate" TEXT NULL,
    "ReceivedDate" TEXT NULL,
    "UsedDate" TEXT NULL,
    "InspectionRequired" INTEGER NOT NULL DEFAULT 0,
    "InspectionPassed" INTEGER NULL,
    "InspectionNotes" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_AssemblyComponents_PrototypeJobs_PrototypeJobId" FOREIGN KEY ("PrototypeJobId") REFERENCES "PrototypeJobs" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ProductionStageExecutions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductionStageExecutions" PRIMARY KEY AUTOINCREMENT,
    "PrototypeJobId" INTEGER NOT NULL,
    "ProductionStageId" INTEGER NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'NotStarted',
    "StartDate" TEXT NULL,
    "CompletionDate" TEXT NULL,
    "EstimatedHours" decimal(8,2) NULL,
    "ActualHours" decimal(8,2) NULL,
    "SetupHours" decimal(8,2) NULL,
    "RunHours" decimal(8,2) NULL,
    "EstimatedCost" decimal(10,2) NULL,
    "ActualCost" decimal(10,2) NULL,
    "MaterialCost" decimal(10,2) NULL,
    "LaborCost" decimal(10,2) NULL,
    "OverheadCost" decimal(10,2) NULL,
    "QualityCheckRequired" INTEGER NOT NULL DEFAULT 1,
    "QualityCheckPassed" INTEGER NULL,
    "QualityCheckBy" TEXT NULL,
    "QualityCheckDate" TEXT NULL,
    "QualityNotes" TEXT NULL,
    "ProcessParameters" TEXT NOT NULL DEFAULT '{}',
    "Issues" TEXT NULL,
    "Improvements" TEXT NULL,
    "ExecutedBy" TEXT NOT NULL,
    "ReviewedBy" TEXT NULL,
    "ApprovedBy" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "UpdatedDate" TEXT NULL,
    CONSTRAINT "FK_ProductionStageExecutions_ProductionStages_ProductionStageId" FOREIGN KEY ("ProductionStageId") REFERENCES "ProductionStages" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ProductionStageExecutions_PrototypeJobs_PrototypeJobId" FOREIGN KEY ("PrototypeJobId") REFERENCES "PrototypeJobs" ("Id") ON DELETE CASCADE
);

CREATE TABLE "PrototypeTimeLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PrototypeTimeLogs" PRIMARY KEY AUTOINCREMENT,
    "ProductionStageExecutionId" INTEGER NOT NULL,
    "LogDate" TEXT NOT NULL,
    "StartTime" TEXT NOT NULL,
    "EndTime" TEXT NULL,
    "ElapsedMinutes" INTEGER NULL,
    "ActivityType" TEXT NOT NULL,
    "ActivityDescription" TEXT NOT NULL,
    "Employee" TEXT NOT NULL,
    "IssuesEncountered" TEXT NULL,
    "ResolutionNotes" TEXT NULL,
    "ImprovementSuggestions" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_PrototypeTimeLogs_ProductionStageExecutions_ProductionStageExecutionId" FOREIGN KEY ("ProductionStageExecutionId") REFERENCES "ProductionStageExecutions" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AssemblyComponents_ComponentType" ON "AssemblyComponents" ("ComponentType");

CREATE INDEX "IX_AssemblyComponents_IsActive" ON "AssemblyComponents" ("IsActive");

CREATE INDEX "IX_AssemblyComponents_PrototypeJobId" ON "AssemblyComponents" ("PrototypeJobId");

CREATE INDEX "IX_AssemblyComponents_Status" ON "AssemblyComponents" ("Status");

CREATE INDEX "IX_AssemblyComponents_Supplier" ON "AssemblyComponents" ("Supplier");

CREATE INDEX "IX_ProductionStageExecutions_CompletionDate" ON "ProductionStageExecutions" ("CompletionDate");

CREATE INDEX "IX_ProductionStageExecutions_ExecutedBy" ON "ProductionStageExecutions" ("ExecutedBy");

CREATE INDEX "IX_ProductionStageExecutions_ProductionStageId" ON "ProductionStageExecutions" ("ProductionStageId");

CREATE INDEX "IX_ProductionStageExecutions_PrototypeJobId" ON "ProductionStageExecutions" ("PrototypeJobId");

CREATE UNIQUE INDEX "IX_ProductionStageExecutions_PrototypeJobId_ProductionStageId" ON "ProductionStageExecutions" ("PrototypeJobId", "ProductionStageId");

CREATE INDEX "IX_ProductionStageExecutions_StartDate" ON "ProductionStageExecutions" ("StartDate");

CREATE INDEX "IX_ProductionStageExecutions_Status" ON "ProductionStageExecutions" ("Status");

CREATE INDEX "IX_ProductionStages_DisplayOrder" ON "ProductionStages" ("DisplayOrder");

CREATE INDEX "IX_ProductionStages_IsActive" ON "ProductionStages" ("IsActive");

CREATE INDEX "IX_ProductionStages_Name" ON "ProductionStages" ("Name");

CREATE INDEX "IX_ProductionStages_RequiredRole" ON "ProductionStages" ("RequiredRole");

CREATE INDEX "IX_PrototypeJobs_AdminReviewStatus" ON "PrototypeJobs" ("AdminReviewStatus");

CREATE INDEX "IX_PrototypeJobs_IsActive" ON "PrototypeJobs" ("IsActive");

CREATE INDEX "IX_PrototypeJobs_PartId" ON "PrototypeJobs" ("PartId");

CREATE INDEX "IX_PrototypeJobs_Priority" ON "PrototypeJobs" ("Priority");

CREATE UNIQUE INDEX "IX_PrototypeJobs_PrototypeNumber" ON "PrototypeJobs" ("PrototypeNumber");

CREATE INDEX "IX_PrototypeJobs_RequestDate" ON "PrototypeJobs" ("RequestDate");

CREATE INDEX "IX_PrototypeJobs_RequestedBy" ON "PrototypeJobs" ("RequestedBy");

CREATE INDEX "IX_PrototypeJobs_Status" ON "PrototypeJobs" ("Status");

CREATE INDEX "IX_PrototypeTimeLogs_ActivityType" ON "PrototypeTimeLogs" ("ActivityType");

CREATE INDEX "IX_PrototypeTimeLogs_Employee" ON "PrototypeTimeLogs" ("Employee");

CREATE INDEX "IX_PrototypeTimeLogs_LogDate" ON "PrototypeTimeLogs" ("LogDate");

CREATE INDEX "IX_PrototypeTimeLogs_ProductionStageExecutionId" ON "PrototypeTimeLogs" ("ProductionStageExecutionId");

CREATE INDEX "IX_PrototypeTimeLogs_StartTime" ON "PrototypeTimeLogs" ("StartTime");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250730162329_AddPrototypeTrackingSystem', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

ALTER TABLE "BuildJobs" ADD "PartId" INTEGER NULL;

CREATE TABLE "EDMLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EDMLogs" PRIMARY KEY AUTOINCREMENT,
    "LogNumber" TEXT NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "LogDate" TEXT NOT NULL,
    "Shift" TEXT NOT NULL DEFAULT '',
    "OperatorName" TEXT NOT NULL,
    "OperatorInitials" TEXT NOT NULL,
    "StartTime" TEXT NOT NULL DEFAULT '',
    "EndTime" TEXT NOT NULL DEFAULT '',
    "Measurement1" TEXT NOT NULL DEFAULT '',
    "Measurement2" TEXT NOT NULL DEFAULT '',
    "ToleranceStatus" TEXT NOT NULL DEFAULT '',
    "ScrapIssues" TEXT NOT NULL DEFAULT '',
    "Notes" TEXT NOT NULL DEFAULT '',
    "TotalTime" TEXT NOT NULL DEFAULT '',
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedBy" TEXT NOT NULL DEFAULT '',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "MachineUsed" TEXT NOT NULL DEFAULT '',
    "ProcessType" TEXT NOT NULL DEFAULT 'EDM',
    "QualityNotes" TEXT NOT NULL DEFAULT '',
    "IsCompleted" INTEGER NOT NULL DEFAULT 0,
    "RequiresReview" INTEGER NOT NULL DEFAULT 0,
    "ReviewedBy" TEXT NOT NULL DEFAULT '',
    "ReviewedDate" TEXT NULL,
    "ReviewNotes" TEXT NOT NULL DEFAULT '',
    "ProcessParameters" TEXT NOT NULL DEFAULT '{}',
    "Measurements" TEXT NOT NULL DEFAULT '{}',
    "PartId" INTEGER NULL,
    CONSTRAINT "FK_EDMLogs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_BuildJobs_PartId" ON "BuildJobs" ("PartId");

CREATE INDEX "IX_EDMLogs_CreatedDate" ON "EDMLogs" ("CreatedDate");

CREATE INDEX "IX_EDMLogs_IsActive" ON "EDMLogs" ("IsActive");

CREATE INDEX "IX_EDMLogs_IsCompleted" ON "EDMLogs" ("IsCompleted");

CREATE INDEX "IX_EDMLogs_LogDate" ON "EDMLogs" ("LogDate");

CREATE INDEX "IX_EDMLogs_LogDate_OperatorName" ON "EDMLogs" ("LogDate", "OperatorName");

CREATE UNIQUE INDEX "IX_EDMLogs_LogNumber" ON "EDMLogs" ("LogNumber");

CREATE INDEX "IX_EDMLogs_OperatorName" ON "EDMLogs" ("OperatorName");

CREATE INDEX "IX_EDMLogs_PartId" ON "EDMLogs" ("PartId");

CREATE INDEX "IX_EDMLogs_PartNumber" ON "EDMLogs" ("PartNumber");

CREATE INDEX "IX_EDMLogs_PartNumber_LogDate" ON "EDMLogs" ("PartNumber", "LogDate");

CREATE INDEX "IX_EDMLogs_RequiresReview" ON "EDMLogs" ("RequiresReview");

CREATE TABLE "ef_temp_BuildJobs" (
    "BuildId" INTEGER NOT NULL CONSTRAINT "PK_BuildJobs" PRIMARY KEY AUTOINCREMENT,
    "ActualEndTime" TEXT NULL,
    "ActualStartTime" TEXT NOT NULL,
    "AssociatedScheduledJobId" INTEGER NULL,
    "CompletedAt" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "GasUsed_L" REAL NULL,
    "LaserRunTime" TEXT NULL,
    "Notes" TEXT NULL,
    "PartId" INTEGER NULL,
    "PowderUsed_L" REAL NULL,
    "PrinterName" TEXT NOT NULL,
    "ReasonForEnd" TEXT NULL,
    "ScheduledEndTime" TEXT NULL,
    "ScheduledStartTime" TEXT NULL,
    "SetupNotes" TEXT NULL,
    "Status" TEXT NOT NULL,
    "UserId" INTEGER NOT NULL,
    CONSTRAINT "FK_BuildJobs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id"),
    CONSTRAINT "FK_BuildJobs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_BuildJobs" ("BuildId", "ActualEndTime", "ActualStartTime", "AssociatedScheduledJobId", "CompletedAt", "CreatedAt", "GasUsed_L", "LaserRunTime", "Notes", "PartId", "PowderUsed_L", "PrinterName", "ReasonForEnd", "ScheduledEndTime", "ScheduledStartTime", "SetupNotes", "Status", "UserId")
SELECT "BuildId", "ActualEndTime", "ActualStartTime", "AssociatedScheduledJobId", "CompletedAt", "CreatedAt", "GasUsed_L", "LaserRunTime", "Notes", "PartId", "PowderUsed_L", "PrinterName", "ReasonForEnd", "ScheduledEndTime", "ScheduledStartTime", "SetupNotes", "Status", "UserId"
FROM "BuildJobs";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "BuildJobs";

ALTER TABLE "ef_temp_BuildJobs" RENAME TO "BuildJobs";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

CREATE INDEX "IX_BuildJobs_PartId" ON "BuildJobs" ("PartId");

CREATE INDEX "IX_BuildJobs_UserId" ON "BuildJobs" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250731160157_AddEDMLogEntity', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

DROP INDEX "IX_EDMLogs_CreatedDate";

DROP INDEX "IX_EDMLogs_IsActive";

DROP INDEX "IX_EDMLogs_IsCompleted";

DROP INDEX "IX_EDMLogs_LogDate";

DROP INDEX "IX_EDMLogs_LogDate_OperatorName";

DROP INDEX "IX_EDMLogs_LogNumber";

DROP INDEX "IX_EDMLogs_OperatorName";

DROP INDEX "IX_EDMLogs_PartNumber";

DROP INDEX "IX_EDMLogs_PartNumber_LogDate";

DROP INDEX "IX_EDMLogs_RequiresReview";

CREATE TABLE "BugReports" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BugReports" PRIMARY KEY AUTOINCREMENT,
    "BugId" TEXT NOT NULL,
    "Title" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Severity" TEXT NOT NULL DEFAULT 'Medium',
    "Priority" TEXT NOT NULL DEFAULT 'Medium',
    "Status" TEXT NOT NULL DEFAULT 'New',
    "Category" TEXT NOT NULL DEFAULT 'General',
    "PageUrl" TEXT NOT NULL,
    "PageName" TEXT NOT NULL,
    "PageArea" TEXT NOT NULL DEFAULT '',
    "PageController" TEXT NOT NULL DEFAULT '',
    "PageAction" TEXT NOT NULL DEFAULT '',
    "ReportedBy" TEXT NOT NULL,
    "UserRole" TEXT NOT NULL DEFAULT '',
    "UserEmail" TEXT NOT NULL DEFAULT '',
    "ReportedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "UserAgent" TEXT NOT NULL DEFAULT '',
    "BrowserName" TEXT NOT NULL DEFAULT '',
    "BrowserVersion" TEXT NOT NULL DEFAULT '',
    "OperatingSystem" TEXT NOT NULL DEFAULT '',
    "ScreenResolution" TEXT NOT NULL DEFAULT '',
    "IpAddress" TEXT NOT NULL DEFAULT '',
    "ErrorType" TEXT NOT NULL DEFAULT '',
    "ErrorMessage" TEXT NOT NULL DEFAULT '',
    "StackTrace" TEXT NOT NULL DEFAULT '',
    "OperationId" TEXT NOT NULL DEFAULT '',
    "StepsToReproduce" TEXT NOT NULL DEFAULT '',
    "ExpectedBehavior" TEXT NOT NULL DEFAULT '',
    "ActualBehavior" TEXT NOT NULL DEFAULT '',
    "AdditionalNotes" TEXT NOT NULL DEFAULT '',
    "AttachedFiles" TEXT NOT NULL DEFAULT '',
    "FormData" TEXT NOT NULL DEFAULT '',
    "NetworkRequests" TEXT NOT NULL DEFAULT '',
    "ConsoleErrors" TEXT NOT NULL DEFAULT '',
    "AssignedTo" TEXT NOT NULL DEFAULT '',
    "AssignedDate" TEXT NULL,
    "ResolvedBy" TEXT NOT NULL DEFAULT '',
    "ResolvedDate" TEXT NULL,
    "ResolutionNotes" TEXT NOT NULL DEFAULT '',
    "ResolutionType" TEXT NOT NULL DEFAULT '',
    "ViewCount" INTEGER NOT NULL DEFAULT 0,
    "VoteCount" INTEGER NOT NULL DEFAULT 0,
    "LastViewedDate" TEXT NULL,
    "LastViewedBy" TEXT NOT NULL DEFAULT '',
    "IsReproduced" INTEGER NOT NULL DEFAULT 0,
    "ReproducedBy" TEXT NOT NULL DEFAULT '',
    "ReproducedDate" TEXT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "IsPublic" INTEGER NOT NULL DEFAULT 0,
    "NotifyReporter" INTEGER NOT NULL DEFAULT 1,
    "CreatedBy" TEXT NOT NULL DEFAULT '',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedBy" TEXT NOT NULL DEFAULT '',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "RelatedBugIds" TEXT NOT NULL DEFAULT '',
    "DuplicateOf" TEXT NOT NULL DEFAULT '',
    "PerformanceImpact" TEXT NOT NULL DEFAULT 'None',
    "PageLoadTime" TEXT NULL,
    "MemoryUsage" TEXT NULL,
    "CpuUsage" TEXT NULL,
    "Tags" TEXT NOT NULL DEFAULT '',
    "CustomMetadata" TEXT NOT NULL DEFAULT '{}'
);

CREATE INDEX "IX_BugReports_AssignedTo" ON "BugReports" ("AssignedTo");

CREATE UNIQUE INDEX "IX_BugReports_BugId" ON "BugReports" ("BugId");

CREATE INDEX "IX_BugReports_Category" ON "BugReports" ("Category");

CREATE INDEX "IX_BugReports_IsActive" ON "BugReports" ("IsActive");

CREATE INDEX "IX_BugReports_IsPublic" ON "BugReports" ("IsPublic");

CREATE INDEX "IX_BugReports_OperationId" ON "BugReports" ("OperationId");

CREATE INDEX "IX_BugReports_PageArea" ON "BugReports" ("PageArea");

CREATE INDEX "IX_BugReports_PageArea_Status" ON "BugReports" ("PageArea", "Status");

CREATE INDEX "IX_BugReports_Priority" ON "BugReports" ("Priority");

CREATE INDEX "IX_BugReports_ReportedBy" ON "BugReports" ("ReportedBy");

CREATE INDEX "IX_BugReports_ReportedDate" ON "BugReports" ("ReportedDate");

CREATE INDEX "IX_BugReports_ReportedDate_Status" ON "BugReports" ("ReportedDate", "Status");

CREATE INDEX "IX_BugReports_ResolvedDate" ON "BugReports" ("ResolvedDate");

CREATE INDEX "IX_BugReports_Severity" ON "BugReports" ("Severity");

CREATE INDEX "IX_BugReports_Severity_Priority" ON "BugReports" ("Severity", "Priority");

CREATE INDEX "IX_BugReports_Status" ON "BugReports" ("Status");

CREATE TABLE "ef_temp_EDMLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EDMLogs" PRIMARY KEY AUTOINCREMENT,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "EndTime" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "IsCompleted" INTEGER NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "LogDate" TEXT NOT NULL,
    "LogNumber" TEXT NOT NULL,
    "MachineUsed" TEXT NOT NULL,
    "Measurement1" TEXT NOT NULL,
    "Measurement2" TEXT NOT NULL,
    "Measurements" TEXT NOT NULL,
    "Notes" TEXT NOT NULL,
    "OperatorInitials" TEXT NOT NULL,
    "OperatorName" TEXT NOT NULL,
    "PartId" INTEGER NULL,
    "PartNumber" TEXT NOT NULL,
    "ProcessParameters" TEXT NOT NULL,
    "ProcessType" TEXT NOT NULL,
    "QualityNotes" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "RequiresReview" INTEGER NOT NULL,
    "ReviewNotes" TEXT NOT NULL,
    "ReviewedBy" TEXT NOT NULL,
    "ReviewedDate" TEXT NULL,
    "ScrapIssues" TEXT NOT NULL,
    "Shift" TEXT NOT NULL,
    "StartTime" TEXT NOT NULL,
    "ToleranceStatus" TEXT NOT NULL,
    "TotalTime" TEXT NOT NULL,
    CONSTRAINT "FK_EDMLogs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id")
);

INSERT INTO "ef_temp_EDMLogs" ("Id", "CreatedBy", "CreatedDate", "EndTime", "IsActive", "IsCompleted", "LastModifiedBy", "LastModifiedDate", "LogDate", "LogNumber", "MachineUsed", "Measurement1", "Measurement2", "Measurements", "Notes", "OperatorInitials", "OperatorName", "PartId", "PartNumber", "ProcessParameters", "ProcessType", "QualityNotes", "Quantity", "RequiresReview", "ReviewNotes", "ReviewedBy", "ReviewedDate", "ScrapIssues", "Shift", "StartTime", "ToleranceStatus", "TotalTime")
SELECT "Id", "CreatedBy", "CreatedDate", "EndTime", "IsActive", "IsCompleted", "LastModifiedBy", "LastModifiedDate", "LogDate", "LogNumber", "MachineUsed", "Measurement1", "Measurement2", "Measurements", "Notes", "OperatorInitials", "OperatorName", "PartId", "PartNumber", "ProcessParameters", "ProcessType", "QualityNotes", "Quantity", "RequiresReview", "ReviewNotes", "ReviewedBy", "ReviewedDate", "ScrapIssues", "Shift", "StartTime", "ToleranceStatus", "TotalTime"
FROM "EDMLogs";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "EDMLogs";

ALTER TABLE "ef_temp_EDMLogs" RENAME TO "EDMLogs";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

CREATE INDEX "IX_EDMLogs_PartId" ON "EDMLogs" ("PartId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250731181242_AddBugReportingSystem', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250801163504_DatabaseRefactoringComplete', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250801164030_FixEFRelationshipConflicts', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

CREATE TABLE "PartStageRequirements" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartStageRequirements" PRIMARY KEY AUTOINCREMENT,
    "PartId" INTEGER NOT NULL,
    "ProductionStageId" INTEGER NOT NULL,
    "ExecutionOrder" INTEGER NOT NULL,
    "IsRequired" INTEGER NOT NULL,
    "EstimatedHours" REAL NULL,
    "SetupTimeMinutes" INTEGER NOT NULL,
    "StageParameters" TEXT NOT NULL,
    "SpecialInstructions" TEXT NOT NULL,
    "QualityRequirements" TEXT NOT NULL,
    "RequiredMaterials" TEXT NOT NULL,
    "RequiredTooling" TEXT NOT NULL,
    "EstimatedCost" decimal(10,2) NOT NULL,
    "AllowParallel" INTEGER NOT NULL,
    "IsBlocking" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "RequirementNotes" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    CONSTRAINT "FK_PartStageRequirements_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PartStageRequirements_ProductionStages_ProductionStageId" FOREIGN KEY ("ProductionStageId") REFERENCES "ProductionStages" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_PartStageRequirements_PartId" ON "PartStageRequirements" ("PartId");

CREATE INDEX "IX_PartStageRequirements_ProductionStageId" ON "PartStageRequirements" ("ProductionStageId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250801170930_AddPartStageRequirementTable', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250802122703_AddCustomFieldsToProductionStages', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

ALTER TABLE "PartStageRequirements" RENAME COLUMN "AllowParallel" TO "RequiresSpecificMachine";

ALTER TABLE "ProductionStages" ADD "AllowParallelExecution" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "ProductionStages" ADD "AssignedMachineIds" TEXT NULL;

ALTER TABLE "ProductionStages" ADD "CreatedBy" TEXT NOT NULL DEFAULT 'System';

ALTER TABLE "ProductionStages" ADD "CustomFieldsConfig" TEXT NOT NULL DEFAULT '[]';

ALTER TABLE "ProductionStages" ADD "DefaultDurationHours" REAL NOT NULL DEFAULT 1.0;

ALTER TABLE "ProductionStages" ADD "DefaultMachineId" TEXT NULL;

ALTER TABLE "ProductionStages" ADD "DefaultMaterialCost" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "ProductionStages" ADD "Department" TEXT NULL;

ALTER TABLE "ProductionStages" ADD "LastModifiedBy" TEXT NOT NULL DEFAULT 'System';

ALTER TABLE "ProductionStages" ADD "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now'));

ALTER TABLE "ProductionStages" ADD "RequiresMachineAssignment" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "ProductionStages" ADD "StageColor" TEXT NOT NULL DEFAULT '#007bff';

ALTER TABLE "ProductionStages" ADD "StageIcon" TEXT NOT NULL DEFAULT 'fas fa-cogs';

ALTER TABLE "PartStageRequirements" ADD "AllowParallelExecution" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "PartStageRequirements" ADD "AssignedMachineId" TEXT NULL;

ALTER TABLE "PartStageRequirements" ADD "CustomFieldValues" TEXT NOT NULL DEFAULT '';

ALTER TABLE "PartStageRequirements" ADD "HourlyRateOverride" decimal(8,2) NULL;

ALTER TABLE "PartStageRequirements" ADD "MaterialCost" decimal(10,2) NOT NULL DEFAULT '0.0';

ALTER TABLE "PartStageRequirements" ADD "PreferredMachineIds" TEXT NULL;

ALTER TABLE "Jobs" ADD "BuildCohortId" INTEGER NULL;

ALTER TABLE "Jobs" ADD "StageOrder" INTEGER NULL;

ALTER TABLE "Jobs" ADD "TotalStages" INTEGER NULL;

ALTER TABLE "Jobs" ADD "WorkflowStage" TEXT NULL;

CREATE TABLE "BuildCohorts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BuildCohorts" PRIMARY KEY AUTOINCREMENT,
    "BuildJobId" INTEGER NULL,
    "BuildNumber" TEXT NOT NULL,
    "PartCount" INTEGER NOT NULL,
    "Material" TEXT NOT NULL DEFAULT 'Ti-6Al-4V Grade 5',
    "Status" TEXT NOT NULL DEFAULT 'InProgress',
    "CompletedDate" TEXT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "Notes" TEXT NULL,
    CONSTRAINT "FK_BuildCohorts_BuildJobs_BuildJobId" FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE SET NULL
);

CREATE TABLE "JobStageHistories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_JobStageHistories" PRIMARY KEY AUTOINCREMENT,
    "JobId" INTEGER NOT NULL,
    "ProductionStageId" INTEGER NULL,
    "Action" TEXT NOT NULL,
    "StageName" TEXT NOT NULL,
    "Operator" TEXT NOT NULL,
    "Timestamp" TEXT NOT NULL DEFAULT (datetime('now')),
    "Notes" TEXT NULL,
    "MachineId" TEXT NULL,
    "StageHours" REAL NULL,
    "QualityResult" TEXT NULL,
    CONSTRAINT "FK_JobStageHistories_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES "Jobs" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_JobStageHistories_ProductionStages_ProductionStageId" FOREIGN KEY ("ProductionStageId") REFERENCES "ProductionStages" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_ProductionStages_Department" ON "ProductionStages" ("Department");

CREATE INDEX "IX_ProductionStages_DisplayOrder_IsActive" ON "ProductionStages" ("DisplayOrder", "IsActive");

CREATE INDEX "IX_ProductionStages_RequiresMachineAssignment" ON "ProductionStages" ("RequiresMachineAssignment");

CREATE INDEX "IX_ProductionStages_StageColor" ON "ProductionStages" ("StageColor");

CREATE INDEX "IX_Jobs_BuildCohortId" ON "Jobs" ("BuildCohortId");

CREATE INDEX "IX_Jobs_BuildCohortId_StageOrder" ON "Jobs" ("BuildCohortId", "StageOrder");

CREATE INDEX "IX_Jobs_StageOrder" ON "Jobs" ("StageOrder");

CREATE INDEX "IX_Jobs_WorkflowStage" ON "Jobs" ("WorkflowStage");

CREATE INDEX "IX_Jobs_WorkflowStage_Status" ON "Jobs" ("WorkflowStage", "Status");

CREATE INDEX "IX_BuildCohorts_BuildJobId" ON "BuildCohorts" ("BuildJobId");

CREATE UNIQUE INDEX "IX_BuildCohorts_BuildNumber" ON "BuildCohorts" ("BuildNumber");

CREATE INDEX "IX_BuildCohorts_CompletedDate" ON "BuildCohorts" ("CompletedDate");

CREATE INDEX "IX_BuildCohorts_CreatedDate" ON "BuildCohorts" ("CreatedDate");

CREATE INDEX "IX_BuildCohorts_Material" ON "BuildCohorts" ("Material");

CREATE INDEX "IX_BuildCohorts_Status" ON "BuildCohorts" ("Status");

CREATE INDEX "IX_JobStageHistories_Action" ON "JobStageHistories" ("Action");

CREATE INDEX "IX_JobStageHistories_JobId" ON "JobStageHistories" ("JobId");

CREATE INDEX "IX_JobStageHistories_JobId_Timestamp" ON "JobStageHistories" ("JobId", "Timestamp");

CREATE INDEX "IX_JobStageHistories_MachineId" ON "JobStageHistories" ("MachineId");

CREATE INDEX "IX_JobStageHistories_Operator" ON "JobStageHistories" ("Operator");

CREATE INDEX "IX_JobStageHistories_ProductionStageId" ON "JobStageHistories" ("ProductionStageId");

CREATE INDEX "IX_JobStageHistories_StageName" ON "JobStageHistories" ("StageName");

CREATE INDEX "IX_JobStageHistories_Timestamp" ON "JobStageHistories" ("Timestamp");

CREATE TABLE "ef_temp_PartStageRequirements" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartStageRequirements" PRIMARY KEY AUTOINCREMENT,
    "AllowParallelExecution" INTEGER NOT NULL,
    "AssignedMachineId" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "CustomFieldValues" TEXT NOT NULL,
    "EstimatedCost" decimal(10,2) NOT NULL,
    "EstimatedHours" REAL NULL,
    "ExecutionOrder" INTEGER NOT NULL,
    "HourlyRateOverride" decimal(8,2) NULL,
    "IsActive" INTEGER NOT NULL,
    "IsBlocking" INTEGER NOT NULL,
    "IsRequired" INTEGER NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "MaterialCost" decimal(10,2) NOT NULL,
    "PartId" INTEGER NOT NULL,
    "PreferredMachineIds" TEXT NULL,
    "ProductionStageId" INTEGER NOT NULL,
    "QualityRequirements" TEXT NOT NULL,
    "RequiredMaterials" TEXT NOT NULL,
    "RequiredTooling" TEXT NOT NULL,
    "RequirementNotes" TEXT NOT NULL,
    "RequiresSpecificMachine" INTEGER NOT NULL,
    "SetupTimeMinutes" INTEGER NULL,
    "SpecialInstructions" TEXT NOT NULL,
    "StageParameters" TEXT NOT NULL,
    CONSTRAINT "FK_PartStageRequirements_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PartStageRequirements_ProductionStages_ProductionStageId" FOREIGN KEY ("ProductionStageId") REFERENCES "ProductionStages" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_PartStageRequirements" ("Id", "AllowParallelExecution", "AssignedMachineId", "CreatedBy", "CreatedDate", "CustomFieldValues", "EstimatedCost", "EstimatedHours", "ExecutionOrder", "HourlyRateOverride", "IsActive", "IsBlocking", "IsRequired", "LastModifiedBy", "LastModifiedDate", "MaterialCost", "PartId", "PreferredMachineIds", "ProductionStageId", "QualityRequirements", "RequiredMaterials", "RequiredTooling", "RequirementNotes", "RequiresSpecificMachine", "SetupTimeMinutes", "SpecialInstructions", "StageParameters")
SELECT "Id", "AllowParallelExecution", "AssignedMachineId", "CreatedBy", "CreatedDate", "CustomFieldValues", "EstimatedCost", "EstimatedHours", "ExecutionOrder", "HourlyRateOverride", "IsActive", "IsBlocking", "IsRequired", "LastModifiedBy", "LastModifiedDate", "MaterialCost", "PartId", "PreferredMachineIds", "ProductionStageId", "QualityRequirements", "RequiredMaterials", "RequiredTooling", "RequirementNotes", "RequiresSpecificMachine", "SetupTimeMinutes", "SpecialInstructions", "StageParameters"
FROM "PartStageRequirements";

CREATE TABLE "ef_temp_PartClassifications" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartClassifications" PRIMARY KEY AUTOINCREMENT,
    "AlternativeMaterials" TEXT NOT NULL DEFAULT '',
    "BafflePosition" TEXT NULL,
    "ClassificationCode" TEXT NOT NULL,
    "ClassificationName" TEXT NOT NULL,
    "ComplexityLevel" INTEGER NOT NULL,
    "ComponentCategory" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "Description" TEXT NOT NULL,
    "ExportClassification" TEXT NOT NULL DEFAULT '',
    "FirearmType" TEXT NULL DEFAULT '',
    "IndustryType" TEXT NOT NULL DEFAULT 'Firearms',
    "IsActive" INTEGER NOT NULL,
    "IsBarrelComponent" INTEGER NOT NULL,
    "IsControlledItem" INTEGER NOT NULL,
    "IsEARControlled" INTEGER NOT NULL,
    "IsEndCap" INTEGER NOT NULL,
    "IsFurniture" INTEGER NOT NULL,
    "IsInternalComponent" INTEGER NOT NULL,
    "IsMountingHardware" INTEGER NOT NULL,
    "IsOperatingSystem" INTEGER NOT NULL,
    "IsReceiver" INTEGER NOT NULL,
    "IsSafetyComponent" INTEGER NOT NULL,
    "IsThreadMount" INTEGER NOT NULL,
    "IsTriggerComponent" INTEGER NOT NULL,
    "IsTubeHousing" INTEGER NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "MaterialGrade" TEXT NOT NULL DEFAULT 'Aerospace',
    "PostProcessingRequired" TEXT NOT NULL DEFAULT '',
    "QualityStandards" TEXT NOT NULL DEFAULT '',
    "RecommendedMaterial" TEXT NOT NULL DEFAULT 'Ti-6Al-4V Grade 5',
    "RegulatoryNotes" TEXT NOT NULL DEFAULT '',
    "RequiredProcess" TEXT NOT NULL DEFAULT 'SLS Metal Printing',
    "RequiresATFCompliance" INTEGER NOT NULL,
    "RequiresDimensionalVerification" INTEGER NOT NULL,
    "RequiresFFLTracking" INTEGER NOT NULL,
    "RequiresITARCompliance" INTEGER NOT NULL,
    "RequiresMaterialCertification" INTEGER NOT NULL,
    "RequiresPressureTesting" INTEGER NOT NULL,
    "RequiresProofTesting" INTEGER NOT NULL,
    "RequiresSerialization" INTEGER NOT NULL,
    "RequiresSpecialHandling" INTEGER NOT NULL,
    "RequiresSurfaceFinishVerification" INTEGER NOT NULL,
    "SpecialInstructions" TEXT NOT NULL DEFAULT '',
    "SuppressorType" TEXT NULL,
    "TestingRequirements" TEXT NOT NULL DEFAULT ''
);

INSERT INTO "ef_temp_PartClassifications" ("Id", "AlternativeMaterials", "BafflePosition", "ClassificationCode", "ClassificationName", "ComplexityLevel", "ComponentCategory", "CreatedBy", "CreatedDate", "Description", "ExportClassification", "FirearmType", "IndustryType", "IsActive", "IsBarrelComponent", "IsControlledItem", "IsEARControlled", "IsEndCap", "IsFurniture", "IsInternalComponent", "IsMountingHardware", "IsOperatingSystem", "IsReceiver", "IsSafetyComponent", "IsThreadMount", "IsTriggerComponent", "IsTubeHousing", "LastModifiedBy", "LastModifiedDate", "MaterialGrade", "PostProcessingRequired", "QualityStandards", "RecommendedMaterial", "RegulatoryNotes", "RequiredProcess", "RequiresATFCompliance", "RequiresDimensionalVerification", "RequiresFFLTracking", "RequiresITARCompliance", "RequiresMaterialCertification", "RequiresPressureTesting", "RequiresProofTesting", "RequiresSerialization", "RequiresSpecialHandling", "RequiresSurfaceFinishVerification", "SpecialInstructions", "SuppressorType", "TestingRequirements")
SELECT "Id", "AlternativeMaterials", "BafflePosition", "ClassificationCode", "ClassificationName", "ComplexityLevel", "ComponentCategory", "CreatedBy", "CreatedDate", "Description", "ExportClassification", "FirearmType", "IndustryType", "IsActive", "IsBarrelComponent", "IsControlledItem", "IsEARControlled", "IsEndCap", "IsFurniture", "IsInternalComponent", "IsMountingHardware", "IsOperatingSystem", "IsReceiver", "IsSafetyComponent", "IsThreadMount", "IsTriggerComponent", "IsTubeHousing", "LastModifiedBy", "LastModifiedDate", "MaterialGrade", "PostProcessingRequired", "QualityStandards", "RecommendedMaterial", "RegulatoryNotes", "RequiredProcess", "RequiresATFCompliance", "RequiresDimensionalVerification", "RequiresFFLTracking", "RequiresITARCompliance", "RequiresMaterialCertification", "RequiresPressureTesting", "RequiresProofTesting", "RequiresSerialization", "RequiresSpecialHandling", "RequiresSurfaceFinishVerification", "SpecialInstructions", "SuppressorType", "TestingRequirements"
FROM "PartClassifications";

CREATE TABLE "ef_temp_Jobs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Jobs" PRIMARY KEY AUTOINCREMENT,
    "ActualEnd" TEXT NULL,
    "ActualPowderUsageKg" REAL NOT NULL,
    "ActualStart" TEXT NULL,
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "ArgonPurityPercent" REAL NOT NULL DEFAULT 99.5,
    "BuildCohortId" INTEGER NULL,
    "BuildFileCreatedDate" TEXT NULL,
    "BuildFileName" TEXT NULL,
    "BuildFilePath" TEXT NULL,
    "BuildFileSizeBytes" INTEGER NOT NULL,
    "BuildLayerNumber" INTEGER NOT NULL,
    "BuildPlatformId" TEXT NOT NULL,
    "BuildTemperatureCelsius" REAL NOT NULL DEFAULT 180.0,
    "BuildTimeMinutes" REAL NOT NULL,
    "ChangeoverTimeMinutes" REAL NOT NULL,
    "CoolingTimeMinutes" REAL NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CurrentArgonFlowRate" REAL NOT NULL,
    "CurrentBuildTemperature" REAL NOT NULL,
    "CurrentLaserPowerWatts" REAL NOT NULL,
    "CurrentOxygenLevel" REAL NOT NULL,
    "CustomerDueDate" TEXT NULL,
    "CustomerOrderNumber" TEXT NOT NULL,
    "DefectQuantity" INTEGER NOT NULL,
    "DensityPercentage" REAL NOT NULL,
    "EnergyConsumptionKwh" REAL NOT NULL,
    "EstimatedDuration" TEXT NOT NULL,
    "EstimatedHours" REAL NOT NULL,
    "EstimatedPowderUsageKg" REAL NOT NULL DEFAULT 0.5,
    "HatchSpacingMicrons" REAL NOT NULL DEFAULT 120.0,
    "HoldReason" TEXT NOT NULL,
    "IsRushJob" INTEGER NOT NULL,
    "LaborCostPerHour" decimal(10,2) NOT NULL,
    "LaserPowerWatts" REAL NOT NULL DEFAULT 170.0,
    "LastModifiedBy" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LayerThicknessMicrons" REAL NOT NULL DEFAULT 30.0,
    "MachineId" TEXT NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "MachineUtilizationPercent" REAL NOT NULL,
    "MaterialCostPerKg" decimal(10,2) NOT NULL,
    "Notes" TEXT NULL,
    "OpcUaBuildProgress" REAL NOT NULL,
    "OpcUaErrorMessages" TEXT NULL,
    "OpcUaJobId" TEXT NULL,
    "OpcUaLastUpdate" TEXT NULL,
    "OpcUaStatus" TEXT NULL,
    "Operator" TEXT NULL,
    "OxygenContentPpm" REAL NOT NULL DEFAULT 50.0,
    "PartId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "PostProcessingTimeMinutes" REAL NOT NULL,
    "PowderChangeoverTimeMinutes" REAL NOT NULL,
    "PowderExpirationDate" TEXT NULL,
    "PowderLotNumber" TEXT NOT NULL,
    "PowderRecyclePercentage" REAL NOT NULL,
    "PowerCostPerKwh" decimal(10,2) NOT NULL,
    "PreheatingTimeMinutes" REAL NOT NULL,
    "PreviousJobPartNumber" TEXT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "ProcessParameters" TEXT NOT NULL,
    "ProducedQuantity" INTEGER NOT NULL,
    "QualityCheckpoints" TEXT NOT NULL,
    "QualityInspector" TEXT NULL,
    "Quantity" INTEGER NOT NULL,
    "RequiredMaterials" TEXT NOT NULL,
    "RequiredSkills" TEXT NOT NULL,
    "RequiredTooling" TEXT NOT NULL,
    "RequiresArgonPurge" INTEGER NOT NULL,
    "RequiresPostProcessing" INTEGER NOT NULL,
    "RequiresPowderSieving" INTEGER NOT NULL,
    "RequiresPreheating" INTEGER NOT NULL,
    "ReworkQuantity" INTEGER NOT NULL,
    "ScanSpeedMmPerSec" REAL NOT NULL DEFAULT 1000.0,
    "ScheduledEnd" TEXT NOT NULL,
    "ScheduledStart" TEXT NOT NULL,
    "SetupTimeMinutes" REAL NOT NULL,
    "SlsMaterial" TEXT NOT NULL,
    "SpecialInstructions" TEXT NOT NULL,
    "StageOrder" INTEGER NULL,
    "Status" TEXT NOT NULL,
    "Supervisor" TEXT NULL,
    "SurfaceRoughnessRa" REAL NOT NULL,
    "TotalStages" INTEGER NULL,
    "UltimateTensileStrengthMPa" REAL NOT NULL,
    "WorkflowStage" TEXT NULL,
    CONSTRAINT "FK_Jobs_BuildCohorts_BuildCohortId" FOREIGN KEY ("BuildCohortId") REFERENCES "BuildCohorts" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Jobs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Jobs" ("Id", "ActualEnd", "ActualPowderUsageKg", "ActualStart", "ArgonCostPerHour", "ArgonPurityPercent", "BuildCohortId", "BuildFileCreatedDate", "BuildFileName", "BuildFilePath", "BuildFileSizeBytes", "BuildLayerNumber", "BuildPlatformId", "BuildTemperatureCelsius", "BuildTimeMinutes", "ChangeoverTimeMinutes", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CurrentArgonFlowRate", "CurrentBuildTemperature", "CurrentLaserPowerWatts", "CurrentOxygenLevel", "CustomerDueDate", "CustomerOrderNumber", "DefectQuantity", "DensityPercentage", "EnergyConsumptionKwh", "EstimatedDuration", "EstimatedHours", "EstimatedPowderUsageKg", "HatchSpacingMicrons", "HoldReason", "IsRushJob", "LaborCostPerHour", "LaserPowerWatts", "LastModifiedBy", "LastModifiedDate", "LayerThicknessMicrons", "MachineId", "MachineOperatingCostPerHour", "MachineUtilizationPercent", "MaterialCostPerKg", "Notes", "OpcUaBuildProgress", "OpcUaErrorMessages", "OpcUaJobId", "OpcUaLastUpdate", "OpcUaStatus", "Operator", "OxygenContentPpm", "PartId", "PartNumber", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderExpirationDate", "PowderLotNumber", "PowderRecyclePercentage", "PowerCostPerKwh", "PreheatingTimeMinutes", "PreviousJobPartNumber", "Priority", "ProcessParameters", "ProducedQuantity", "QualityCheckpoints", "QualityInspector", "Quantity", "RequiredMaterials", "RequiredSkills", "RequiredTooling", "RequiresArgonPurge", "RequiresPostProcessing", "RequiresPowderSieving", "RequiresPreheating", "ReworkQuantity", "ScanSpeedMmPerSec", "ScheduledEnd", "ScheduledStart", "SetupTimeMinutes", "SlsMaterial", "SpecialInstructions", "StageOrder", "Status", "Supervisor", "SurfaceRoughnessRa", "TotalStages", "UltimateTensileStrengthMPa", "WorkflowStage")
SELECT "Id", "ActualEnd", "ActualPowderUsageKg", "ActualStart", "ArgonCostPerHour", "ArgonPurityPercent", "BuildCohortId", "BuildFileCreatedDate", "BuildFileName", "BuildFilePath", "BuildFileSizeBytes", "BuildLayerNumber", "BuildPlatformId", "BuildTemperatureCelsius", "BuildTimeMinutes", "ChangeoverTimeMinutes", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CurrentArgonFlowRate", "CurrentBuildTemperature", "CurrentLaserPowerWatts", "CurrentOxygenLevel", "CustomerDueDate", "CustomerOrderNumber", "DefectQuantity", "DensityPercentage", "EnergyConsumptionKwh", "EstimatedDuration", "EstimatedHours", "EstimatedPowderUsageKg", "HatchSpacingMicrons", "HoldReason", "IsRushJob", "LaborCostPerHour", "LaserPowerWatts", "LastModifiedBy", "LastModifiedDate", "LayerThicknessMicrons", "MachineId", "MachineOperatingCostPerHour", "MachineUtilizationPercent", "MaterialCostPerKg", "Notes", "OpcUaBuildProgress", "OpcUaErrorMessages", "OpcUaJobId", "OpcUaLastUpdate", "OpcUaStatus", "Operator", "OxygenContentPpm", "PartId", "PartNumber", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderExpirationDate", "PowderLotNumber", "PowderRecyclePercentage", "PowerCostPerKwh", "PreheatingTimeMinutes", "PreviousJobPartNumber", "Priority", "ProcessParameters", "ProducedQuantity", "QualityCheckpoints", "QualityInspector", "Quantity", "RequiredMaterials", "RequiredSkills", "RequiredTooling", "RequiresArgonPurge", "RequiresPostProcessing", "RequiresPowderSieving", "RequiresPreheating", "ReworkQuantity", "ScanSpeedMmPerSec", "ScheduledEnd", "ScheduledStart", "SetupTimeMinutes", "SlsMaterial", "SpecialInstructions", "StageOrder", "Status", "Supervisor", "SurfaceRoughnessRa", "TotalStages", "UltimateTensileStrengthMPa", "WorkflowStage"
FROM "Jobs";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "PartStageRequirements";

ALTER TABLE "ef_temp_PartStageRequirements" RENAME TO "PartStageRequirements";

DROP TABLE "PartClassifications";

ALTER TABLE "ef_temp_PartClassifications" RENAME TO "PartClassifications";

DROP TABLE "Jobs";

ALTER TABLE "ef_temp_Jobs" RENAME TO "Jobs";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

CREATE INDEX "IX_PartStageRequirements_PartId" ON "PartStageRequirements" ("PartId");

CREATE INDEX "IX_PartStageRequirements_ProductionStageId" ON "PartStageRequirements" ("ProductionStageId");

CREATE UNIQUE INDEX "IX_PartClassifications_ClassificationCode" ON "PartClassifications" ("ClassificationCode");

CREATE INDEX "IX_PartClassifications_ClassificationName" ON "PartClassifications" ("ClassificationName");

CREATE INDEX "IX_PartClassifications_ComponentCategory" ON "PartClassifications" ("ComponentCategory");

CREATE INDEX "IX_PartClassifications_FirearmType" ON "PartClassifications" ("FirearmType");

CREATE INDEX "IX_PartClassifications_IndustryType" ON "PartClassifications" ("IndustryType");

CREATE INDEX "IX_PartClassifications_IsActive" ON "PartClassifications" ("IsActive");

CREATE INDEX "IX_PartClassifications_RequiresATFCompliance" ON "PartClassifications" ("RequiresATFCompliance");

CREATE INDEX "IX_PartClassifications_RequiresITARCompliance" ON "PartClassifications" ("RequiresITARCompliance");

CREATE INDEX "IX_PartClassifications_RequiresSerialization" ON "PartClassifications" ("RequiresSerialization");

CREATE INDEX "IX_PartClassifications_SuppressorType" ON "PartClassifications" ("SuppressorType");

CREATE INDEX "IX_Jobs_BuildCohortId" ON "Jobs" ("BuildCohortId");

CREATE INDEX "IX_Jobs_BuildCohortId_StageOrder" ON "Jobs" ("BuildCohortId", "StageOrder");

CREATE INDEX "IX_Jobs_MachineId_ScheduledStart" ON "Jobs" ("MachineId", "ScheduledStart");

CREATE INDEX "IX_Jobs_PartId" ON "Jobs" ("PartId");

CREATE INDEX "IX_Jobs_PartNumber" ON "Jobs" ("PartNumber");

CREATE INDEX "IX_Jobs_Priority" ON "Jobs" ("Priority");

CREATE INDEX "IX_Jobs_StageOrder" ON "Jobs" ("StageOrder");

CREATE INDEX "IX_Jobs_Status" ON "Jobs" ("Status");

CREATE INDEX "IX_Jobs_WorkflowStage" ON "Jobs" ("WorkflowStage");

CREATE INDEX "IX_Jobs_WorkflowStage_Status" ON "Jobs" ("WorkflowStage", "Status");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250803003059_AddWorkflowFields', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

ALTER TABLE "ProductionStageExecutions" ADD "WorkflowTemplateId" INTEGER NULL;

ALTER TABLE "PartStageRequirements" ADD "WorkflowTemplateId" INTEGER NULL;

CREATE TABLE "ProductionStageDependencies" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductionStageDependencies" PRIMARY KEY AUTOINCREMENT,
    "DependentStageId" INTEGER NOT NULL,
    "PrerequisiteStageId" INTEGER NOT NULL,
    "DependencyType" TEXT NOT NULL DEFAULT 'FinishToStart',
    "DelayHours" INTEGER NOT NULL DEFAULT 0,
    "IsOptional" INTEGER NOT NULL DEFAULT 0,
    "Condition" TEXT NULL,
    "Notes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "CK_ProductionStageDependency_NoSelfReference" CHECK (DependentStageId != PrerequisiteStageId),
    CONSTRAINT "FK_ProductionStageDependencies_ProductionStages_DependentStageId" FOREIGN KEY ("DependentStageId") REFERENCES "ProductionStages" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ProductionStageDependencies_ProductionStages_PrerequisiteStageId" FOREIGN KEY ("PrerequisiteStageId") REFERENCES "ProductionStages" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ResourcePools" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ResourcePools" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "ResourceType" TEXT NOT NULL DEFAULT 'Machine',
    "ResourceConfiguration" TEXT NOT NULL DEFAULT '[]',
    "MaxConcurrentAllocations" INTEGER NOT NULL DEFAULT 1,
    "AutoAssign" INTEGER NOT NULL DEFAULT 0,
    "AssignmentCriteria" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "IsActive" INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE "WorkflowTemplates" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_WorkflowTemplates" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Category" TEXT NULL,
    "Complexity" TEXT NOT NULL DEFAULT 'Medium',
    "EstimatedDurationHours" REAL NOT NULL DEFAULT 8.0,
    "EstimatedCost" decimal(12,2) NOT NULL DEFAULT '0.0',
    "StageConfiguration" TEXT NOT NULL DEFAULT '[]',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "IsActive" INTEGER NOT NULL DEFAULT 1
);

CREATE INDEX "IX_ProductionStageExecutions_WorkflowTemplateId" ON "ProductionStageExecutions" ("WorkflowTemplateId");

CREATE INDEX "IX_PartStageRequirements_WorkflowTemplateId" ON "PartStageRequirements" ("WorkflowTemplateId");

CREATE INDEX "IX_ProductionStageDependencies_DependencyType" ON "ProductionStageDependencies" ("DependencyType");

CREATE INDEX "IX_ProductionStageDependencies_DependentStageId" ON "ProductionStageDependencies" ("DependentStageId");

CREATE UNIQUE INDEX "IX_ProductionStageDependencies_DependentStageId_PrerequisiteStageId" ON "ProductionStageDependencies" ("DependentStageId", "PrerequisiteStageId");

CREATE INDEX "IX_ProductionStageDependencies_IsActive" ON "ProductionStageDependencies" ("IsActive");

CREATE INDEX "IX_ProductionStageDependencies_PrerequisiteStageId" ON "ProductionStageDependencies" ("PrerequisiteStageId");

CREATE INDEX "IX_ResourcePools_AutoAssign" ON "ResourcePools" ("AutoAssign");

CREATE INDEX "IX_ResourcePools_IsActive" ON "ResourcePools" ("IsActive");

CREATE INDEX "IX_ResourcePools_Name" ON "ResourcePools" ("Name");

CREATE INDEX "IX_ResourcePools_ResourceType" ON "ResourcePools" ("ResourceType");

CREATE INDEX "IX_ResourcePools_ResourceType_IsActive" ON "ResourcePools" ("ResourceType", "IsActive");

CREATE INDEX "IX_WorkflowTemplates_Category" ON "WorkflowTemplates" ("Category");

CREATE INDEX "IX_WorkflowTemplates_Category_IsActive" ON "WorkflowTemplates" ("Category", "IsActive");

CREATE INDEX "IX_WorkflowTemplates_Complexity" ON "WorkflowTemplates" ("Complexity");

CREATE INDEX "IX_WorkflowTemplates_IsActive" ON "WorkflowTemplates" ("IsActive");

CREATE INDEX "IX_WorkflowTemplates_Name" ON "WorkflowTemplates" ("Name");

CREATE TABLE "ef_temp_StageDependencies" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StageDependencies" PRIMARY KEY AUTOINCREMENT,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "DependencyType" TEXT NOT NULL DEFAULT 'FinishToStart',
    "DependentStageId" INTEGER NOT NULL,
    "IsMandatory" INTEGER NOT NULL,
    "LagTimeHours" REAL NOT NULL,
    "Notes" TEXT NULL,
    "RequiredStageId" INTEGER NOT NULL,
    CONSTRAINT "CK_JobStageDependency_NoSelfReference" CHECK (DependentStageId != RequiredStageId),
    CONSTRAINT "FK_StageDependencies_JobStages_DependentStageId" FOREIGN KEY ("DependentStageId") REFERENCES "JobStages" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_StageDependencies_JobStages_RequiredStageId" FOREIGN KEY ("RequiredStageId") REFERENCES "JobStages" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_StageDependencies" ("Id", "CreatedDate", "DependencyType", "DependentStageId", "IsMandatory", "LagTimeHours", "Notes", "RequiredStageId")
SELECT "Id", "CreatedDate", "DependencyType", "DependentStageId", "IsMandatory", "LagTimeHours", "Notes", "RequiredStageId"
FROM "StageDependencies";

CREATE TABLE "ef_temp_PartStageRequirements" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartStageRequirements" PRIMARY KEY AUTOINCREMENT,
    "AllowParallelExecution" INTEGER NOT NULL,
    "AssignedMachineId" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "CustomFieldValues" TEXT NOT NULL,
    "EstimatedCost" decimal(10,2) NOT NULL,
    "EstimatedHours" REAL NULL,
    "ExecutionOrder" INTEGER NOT NULL,
    "HourlyRateOverride" decimal(8,2) NULL,
    "IsActive" INTEGER NOT NULL,
    "IsBlocking" INTEGER NOT NULL,
    "IsRequired" INTEGER NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "MaterialCost" decimal(10,2) NOT NULL,
    "PartId" INTEGER NOT NULL,
    "PreferredMachineIds" TEXT NULL,
    "ProductionStageId" INTEGER NOT NULL,
    "QualityRequirements" TEXT NOT NULL,
    "RequiredMaterials" TEXT NOT NULL,
    "RequiredTooling" TEXT NOT NULL,
    "RequirementNotes" TEXT NOT NULL,
    "RequiresSpecificMachine" INTEGER NOT NULL,
    "SetupTimeMinutes" INTEGER NULL,
    "SpecialInstructions" TEXT NOT NULL,
    "StageParameters" TEXT NOT NULL,
    "WorkflowTemplateId" INTEGER NULL,
    CONSTRAINT "FK_PartStageRequirements_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PartStageRequirements_ProductionStages_ProductionStageId" FOREIGN KEY ("ProductionStageId") REFERENCES "ProductionStages" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PartStageRequirements_WorkflowTemplates_WorkflowTemplateId" FOREIGN KEY ("WorkflowTemplateId") REFERENCES "WorkflowTemplates" ("Id") ON DELETE SET NULL
);

INSERT INTO "ef_temp_PartStageRequirements" ("Id", "AllowParallelExecution", "AssignedMachineId", "CreatedBy", "CreatedDate", "CustomFieldValues", "EstimatedCost", "EstimatedHours", "ExecutionOrder", "HourlyRateOverride", "IsActive", "IsBlocking", "IsRequired", "LastModifiedBy", "LastModifiedDate", "MaterialCost", "PartId", "PreferredMachineIds", "ProductionStageId", "QualityRequirements", "RequiredMaterials", "RequiredTooling", "RequirementNotes", "RequiresSpecificMachine", "SetupTimeMinutes", "SpecialInstructions", "StageParameters", "WorkflowTemplateId")
SELECT "Id", "AllowParallelExecution", "AssignedMachineId", "CreatedBy", "CreatedDate", "CustomFieldValues", "EstimatedCost", "EstimatedHours", "ExecutionOrder", "HourlyRateOverride", "IsActive", "IsBlocking", "IsRequired", "LastModifiedBy", "LastModifiedDate", "MaterialCost", "PartId", "PreferredMachineIds", "ProductionStageId", "QualityRequirements", "RequiredMaterials", "RequiredTooling", "RequirementNotes", "RequiresSpecificMachine", "SetupTimeMinutes", "SpecialInstructions", "StageParameters", "WorkflowTemplateId"
FROM "PartStageRequirements";

CREATE TABLE "ef_temp_ProductionStageExecutions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductionStageExecutions" PRIMARY KEY AUTOINCREMENT,
    "ActualCost" decimal(10,2) NULL,
    "ActualHours" decimal(8,2) NULL,
    "ApprovedBy" TEXT NULL,
    "CompletionDate" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "EstimatedCost" decimal(10,2) NULL,
    "EstimatedHours" decimal(8,2) NULL,
    "ExecutedBy" TEXT NOT NULL,
    "Improvements" TEXT NULL,
    "Issues" TEXT NULL,
    "LaborCost" decimal(10,2) NULL,
    "MaterialCost" decimal(10,2) NULL,
    "OverheadCost" decimal(10,2) NULL,
    "ProcessParameters" TEXT NOT NULL DEFAULT '{}',
    "ProductionStageId" INTEGER NOT NULL,
    "PrototypeJobId" INTEGER NOT NULL,
    "QualityCheckBy" TEXT NULL,
    "QualityCheckDate" TEXT NULL,
    "QualityCheckPassed" INTEGER NULL,
    "QualityCheckRequired" INTEGER NOT NULL DEFAULT 1,
    "QualityNotes" TEXT NULL,
    "ReviewedBy" TEXT NULL,
    "RunHours" decimal(8,2) NULL,
    "SetupHours" decimal(8,2) NULL,
    "StartDate" TEXT NULL,
    "Status" TEXT NOT NULL DEFAULT 'NotStarted',
    "UpdatedDate" TEXT NULL,
    "WorkflowTemplateId" INTEGER NULL,
    CONSTRAINT "FK_ProductionStageExecutions_ProductionStages_ProductionStageId" FOREIGN KEY ("ProductionStageId") REFERENCES "ProductionStages" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ProductionStageExecutions_PrototypeJobs_PrototypeJobId" FOREIGN KEY ("PrototypeJobId") REFERENCES "PrototypeJobs" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ProductionStageExecutions_WorkflowTemplates_WorkflowTemplateId" FOREIGN KEY ("WorkflowTemplateId") REFERENCES "WorkflowTemplates" ("Id") ON DELETE SET NULL
);

INSERT INTO "ef_temp_ProductionStageExecutions" ("Id", "ActualCost", "ActualHours", "ApprovedBy", "CompletionDate", "CreatedDate", "EstimatedCost", "EstimatedHours", "ExecutedBy", "Improvements", "Issues", "LaborCost", "MaterialCost", "OverheadCost", "ProcessParameters", "ProductionStageId", "PrototypeJobId", "QualityCheckBy", "QualityCheckDate", "QualityCheckPassed", "QualityCheckRequired", "QualityNotes", "ReviewedBy", "RunHours", "SetupHours", "StartDate", "Status", "UpdatedDate", "WorkflowTemplateId")
SELECT "Id", "ActualCost", "ActualHours", "ApprovedBy", "CompletionDate", "CreatedDate", "EstimatedCost", "EstimatedHours", "ExecutedBy", "Improvements", "Issues", "LaborCost", "MaterialCost", "OverheadCost", "ProcessParameters", "ProductionStageId", "PrototypeJobId", "QualityCheckBy", "QualityCheckDate", "QualityCheckPassed", "QualityCheckRequired", "QualityNotes", "ReviewedBy", "RunHours", "SetupHours", "StartDate", "Status", "UpdatedDate", "WorkflowTemplateId"
FROM "ProductionStageExecutions";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "StageDependencies";

ALTER TABLE "ef_temp_StageDependencies" RENAME TO "StageDependencies";

DROP TABLE "PartStageRequirements";

ALTER TABLE "ef_temp_PartStageRequirements" RENAME TO "PartStageRequirements";

DROP TABLE "ProductionStageExecutions";

ALTER TABLE "ef_temp_ProductionStageExecutions" RENAME TO "ProductionStageExecutions";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

CREATE INDEX "IX_StageDependencies_DependencyType" ON "StageDependencies" ("DependencyType");

CREATE INDEX "IX_StageDependencies_DependentStageId" ON "StageDependencies" ("DependentStageId");

CREATE INDEX "IX_StageDependencies_RequiredStageId" ON "StageDependencies" ("RequiredStageId");

CREATE INDEX "IX_PartStageRequirements_PartId" ON "PartStageRequirements" ("PartId");

CREATE INDEX "IX_PartStageRequirements_ProductionStageId" ON "PartStageRequirements" ("ProductionStageId");

CREATE INDEX "IX_PartStageRequirements_WorkflowTemplateId" ON "PartStageRequirements" ("WorkflowTemplateId");

CREATE INDEX "IX_ProductionStageExecutions_CompletionDate" ON "ProductionStageExecutions" ("CompletionDate");

CREATE INDEX "IX_ProductionStageExecutions_ExecutedBy" ON "ProductionStageExecutions" ("ExecutedBy");

CREATE INDEX "IX_ProductionStageExecutions_ProductionStageId" ON "ProductionStageExecutions" ("ProductionStageId");

CREATE INDEX "IX_ProductionStageExecutions_PrototypeJobId" ON "ProductionStageExecutions" ("PrototypeJobId");

CREATE UNIQUE INDEX "IX_ProductionStageExecutions_PrototypeJobId_ProductionStageId" ON "ProductionStageExecutions" ("PrototypeJobId", "ProductionStageId");

CREATE INDEX "IX_ProductionStageExecutions_StartDate" ON "ProductionStageExecutions" ("StartDate");

CREATE INDEX "IX_ProductionStageExecutions_Status" ON "ProductionStageExecutions" ("Status");

CREATE INDEX "IX_ProductionStageExecutions_WorkflowTemplateId" ON "ProductionStageExecutions" ("WorkflowTemplateId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250803124455_AddAdvancedStageManagementTables', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

ALTER TABLE "BuildJobs" ADD "BuildFileHash" TEXT NULL;

ALTER TABLE "BuildJobs" ADD "BuildHeight" TEXT NULL;

ALTER TABLE "BuildJobs" ADD "DefectCount" INTEGER NULL;

ALTER TABLE "BuildJobs" ADD "IsLearningBuild" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "BuildJobs" ADD "LaserOnTime" TEXT NULL;

ALTER TABLE "BuildJobs" ADD "LayerCount" INTEGER NULL;

ALTER TABLE "BuildJobs" ADD "LessonsLearned" TEXT NULL;

ALTER TABLE "BuildJobs" ADD "MachinePerformanceNotes" TEXT NULL;

ALTER TABLE "BuildJobs" ADD "OperatorActualHours" TEXT NULL;

ALTER TABLE "BuildJobs" ADD "OperatorBuildAssessment" TEXT NULL;

ALTER TABLE "BuildJobs" ADD "OperatorEstimatedHours" TEXT NULL;

ALTER TABLE "BuildJobs" ADD "PartOrientations" TEXT NULL;

ALTER TABLE "BuildJobs" ADD "PostProcessingNeeded" TEXT NULL;

ALTER TABLE "BuildJobs" ADD "PowerConsumption" TEXT NULL;

ALTER TABLE "BuildJobs" ADD "SupportComplexity" TEXT NULL;

ALTER TABLE "BuildJobs" ADD "TimeFactors" TEXT NULL;

ALTER TABLE "BuildJobs" ADD "TotalPartsInBuild" INTEGER NOT NULL DEFAULT 0;

CREATE TABLE "ef_temp_Jobs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Jobs" PRIMARY KEY AUTOINCREMENT,
    "ActualEnd" TEXT NULL,
    "ActualPowderUsageKg" REAL NOT NULL,
    "ActualStart" TEXT NULL,
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "ArgonPurityPercent" REAL NOT NULL DEFAULT 99.5,
    "BuildCohortId" INTEGER NULL,
    "BuildFileCreatedDate" TEXT NULL,
    "BuildFileName" TEXT NULL,
    "BuildFilePath" TEXT NULL,
    "BuildFileSizeBytes" INTEGER NOT NULL,
    "BuildLayerNumber" INTEGER NOT NULL,
    "BuildPlatformId" TEXT NOT NULL,
    "BuildTemperatureCelsius" REAL NOT NULL DEFAULT 180.0,
    "BuildTimeMinutes" REAL NOT NULL,
    "ChangeoverTimeMinutes" REAL NOT NULL,
    "CoolingTimeMinutes" REAL NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CurrentArgonFlowRate" REAL NOT NULL,
    "CurrentBuildTemperature" REAL NOT NULL,
    "CurrentLaserPowerWatts" REAL NOT NULL,
    "CurrentOxygenLevel" REAL NOT NULL,
    "CustomerDueDate" TEXT NULL,
    "CustomerOrderNumber" TEXT NOT NULL DEFAULT '',
    "DefectQuantity" INTEGER NOT NULL,
    "DensityPercentage" REAL NOT NULL,
    "EnergyConsumptionKwh" REAL NOT NULL,
    "EstimatedDuration" TEXT NOT NULL,
    "EstimatedHours" REAL NOT NULL,
    "EstimatedPowderUsageKg" REAL NOT NULL DEFAULT 0.5,
    "HatchSpacingMicrons" REAL NOT NULL DEFAULT 120.0,
    "HoldReason" TEXT NOT NULL,
    "IsRushJob" INTEGER NOT NULL,
    "LaborCostPerHour" decimal(10,2) NOT NULL,
    "LaserPowerWatts" REAL NOT NULL DEFAULT 170.0,
    "LastModifiedBy" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LayerThicknessMicrons" REAL NOT NULL DEFAULT 30.0,
    "MachineId" TEXT NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "MachineUtilizationPercent" REAL NOT NULL,
    "MaterialCostPerKg" decimal(10,2) NOT NULL,
    "Notes" TEXT NULL,
    "OpcUaBuildProgress" REAL NOT NULL,
    "OpcUaErrorMessages" TEXT NULL,
    "OpcUaJobId" TEXT NULL,
    "OpcUaLastUpdate" TEXT NULL,
    "OpcUaStatus" TEXT NULL,
    "Operator" TEXT NULL,
    "OxygenContentPpm" REAL NOT NULL DEFAULT 50.0,
    "PartId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "PostProcessingTimeMinutes" REAL NOT NULL,
    "PowderChangeoverTimeMinutes" REAL NOT NULL,
    "PowderExpirationDate" TEXT NULL,
    "PowderLotNumber" TEXT NOT NULL,
    "PowderRecyclePercentage" REAL NOT NULL,
    "PowerCostPerKwh" decimal(10,2) NOT NULL,
    "PreheatingTimeMinutes" REAL NOT NULL,
    "PreviousJobPartNumber" TEXT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "ProcessParameters" TEXT NOT NULL,
    "ProducedQuantity" INTEGER NOT NULL,
    "QualityCheckpoints" TEXT NOT NULL,
    "QualityInspector" TEXT NULL,
    "Quantity" INTEGER NOT NULL,
    "RequiredMaterials" TEXT NOT NULL,
    "RequiredSkills" TEXT NOT NULL,
    "RequiredTooling" TEXT NOT NULL,
    "RequiresArgonPurge" INTEGER NOT NULL,
    "RequiresPostProcessing" INTEGER NOT NULL,
    "RequiresPowderSieving" INTEGER NOT NULL,
    "RequiresPreheating" INTEGER NOT NULL,
    "ReworkQuantity" INTEGER NOT NULL,
    "ScanSpeedMmPerSec" REAL NOT NULL DEFAULT 1000.0,
    "ScheduledEnd" TEXT NOT NULL,
    "ScheduledStart" TEXT NOT NULL,
    "SetupTimeMinutes" REAL NOT NULL,
    "SlsMaterial" TEXT NOT NULL,
    "SpecialInstructions" TEXT NOT NULL,
    "StageOrder" INTEGER NULL,
    "Status" TEXT NOT NULL,
    "Supervisor" TEXT NULL,
    "SurfaceRoughnessRa" REAL NOT NULL,
    "TotalStages" INTEGER NULL,
    "UltimateTensileStrengthMPa" REAL NOT NULL,
    "WorkflowStage" TEXT NULL,
    CONSTRAINT "FK_Jobs_BuildCohorts_BuildCohortId" FOREIGN KEY ("BuildCohortId") REFERENCES "BuildCohorts" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Jobs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Jobs" ("Id", "ActualEnd", "ActualPowderUsageKg", "ActualStart", "ArgonCostPerHour", "ArgonPurityPercent", "BuildCohortId", "BuildFileCreatedDate", "BuildFileName", "BuildFilePath", "BuildFileSizeBytes", "BuildLayerNumber", "BuildPlatformId", "BuildTemperatureCelsius", "BuildTimeMinutes", "ChangeoverTimeMinutes", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CurrentArgonFlowRate", "CurrentBuildTemperature", "CurrentLaserPowerWatts", "CurrentOxygenLevel", "CustomerDueDate", "CustomerOrderNumber", "DefectQuantity", "DensityPercentage", "EnergyConsumptionKwh", "EstimatedDuration", "EstimatedHours", "EstimatedPowderUsageKg", "HatchSpacingMicrons", "HoldReason", "IsRushJob", "LaborCostPerHour", "LaserPowerWatts", "LastModifiedBy", "LastModifiedDate", "LayerThicknessMicrons", "MachineId", "MachineOperatingCostPerHour", "MachineUtilizationPercent", "MaterialCostPerKg", "Notes", "OpcUaBuildProgress", "OpcUaErrorMessages", "OpcUaJobId", "OpcUaLastUpdate", "OpcUaStatus", "Operator", "OxygenContentPpm", "PartId", "PartNumber", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderExpirationDate", "PowderLotNumber", "PowderRecyclePercentage", "PowerCostPerKwh", "PreheatingTimeMinutes", "PreviousJobPartNumber", "Priority", "ProcessParameters", "ProducedQuantity", "QualityCheckpoints", "QualityInspector", "Quantity", "RequiredMaterials", "RequiredSkills", "RequiredTooling", "RequiresArgonPurge", "RequiresPostProcessing", "RequiresPowderSieving", "RequiresPreheating", "ReworkQuantity", "ScanSpeedMmPerSec", "ScheduledEnd", "ScheduledStart", "SetupTimeMinutes", "SlsMaterial", "SpecialInstructions", "StageOrder", "Status", "Supervisor", "SurfaceRoughnessRa", "TotalStages", "UltimateTensileStrengthMPa", "WorkflowStage")
SELECT "Id", "ActualEnd", "ActualPowderUsageKg", "ActualStart", "ArgonCostPerHour", "ArgonPurityPercent", "BuildCohortId", "BuildFileCreatedDate", "BuildFileName", "BuildFilePath", "BuildFileSizeBytes", "BuildLayerNumber", "BuildPlatformId", "BuildTemperatureCelsius", "BuildTimeMinutes", "ChangeoverTimeMinutes", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CurrentArgonFlowRate", "CurrentBuildTemperature", "CurrentLaserPowerWatts", "CurrentOxygenLevel", "CustomerDueDate", "CustomerOrderNumber", "DefectQuantity", "DensityPercentage", "EnergyConsumptionKwh", "EstimatedDuration", "EstimatedHours", "EstimatedPowderUsageKg", "HatchSpacingMicrons", "HoldReason", "IsRushJob", "LaborCostPerHour", "LaserPowerWatts", "LastModifiedBy", "LastModifiedDate", "LayerThicknessMicrons", "MachineId", "MachineOperatingCostPerHour", "MachineUtilizationPercent", "MaterialCostPerKg", "Notes", "OpcUaBuildProgress", "OpcUaErrorMessages", "OpcUaJobId", "OpcUaLastUpdate", "OpcUaStatus", "Operator", "OxygenContentPpm", "PartId", "PartNumber", "PostProcessingTimeMinutes", "PowderChangeoverTimeMinutes", "PowderExpirationDate", "PowderLotNumber", "PowderRecyclePercentage", "PowerCostPerKwh", "PreheatingTimeMinutes", "PreviousJobPartNumber", "Priority", "ProcessParameters", "ProducedQuantity", "QualityCheckpoints", "QualityInspector", "Quantity", "RequiredMaterials", "RequiredSkills", "RequiredTooling", "RequiresArgonPurge", "RequiresPostProcessing", "RequiresPowderSieving", "RequiresPreheating", "ReworkQuantity", "ScanSpeedMmPerSec", "ScheduledEnd", "ScheduledStart", "SetupTimeMinutes", "SlsMaterial", "SpecialInstructions", "StageOrder", "Status", "Supervisor", "SurfaceRoughnessRa", "TotalStages", "UltimateTensileStrengthMPa", "WorkflowStage"
FROM "Jobs";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "Jobs";

ALTER TABLE "ef_temp_Jobs" RENAME TO "Jobs";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

CREATE INDEX "IX_Jobs_BuildCohortId" ON "Jobs" ("BuildCohortId");

CREATE INDEX "IX_Jobs_BuildCohortId_StageOrder" ON "Jobs" ("BuildCohortId", "StageOrder");

CREATE INDEX "IX_Jobs_MachineId_ScheduledStart" ON "Jobs" ("MachineId", "ScheduledStart");

CREATE INDEX "IX_Jobs_PartId" ON "Jobs" ("PartId");

CREATE INDEX "IX_Jobs_PartNumber" ON "Jobs" ("PartNumber");

CREATE INDEX "IX_Jobs_Priority" ON "Jobs" ("Priority");

CREATE INDEX "IX_Jobs_StageOrder" ON "Jobs" ("StageOrder");

CREATE INDEX "IX_Jobs_Status" ON "Jobs" ("Status");

CREATE INDEX "IX_Jobs_WorkflowStage" ON "Jobs" ("WorkflowStage");

CREATE INDEX "IX_Jobs_WorkflowStage_Status" ON "Jobs" ("WorkflowStage", "Status");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250803173130_EnhancedBuildJobTimeTracking', '8.0.11');

COMMIT;

