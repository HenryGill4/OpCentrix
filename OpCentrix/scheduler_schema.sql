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

