CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

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

PRAGMA foreign_keys = 0;

DROP TABLE "Jobs";

ALTER TABLE "ef_temp_Jobs" RENAME TO "Jobs";

DROP TABLE "Parts";

ALTER TABLE "ef_temp_Parts" RENAME TO "Parts";

PRAGMA foreign_keys = 1;

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

PRAGMA foreign_keys = 0;

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

PRAGMA foreign_keys = 1;

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

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250726141952_MultiStageSchedulingEntities', '8.0.11');

ALTER TABLE "UserSettings" ADD "SchedulerOrientation" TEXT NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250726143534_AddSchedulerOrientationToUserSettings', '8.0.11');

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

PRAGMA foreign_keys = 0;

DROP TABLE "JobStages";

ALTER TABLE "ef_temp_JobStages" RENAME TO "JobStages";

DROP TABLE "MachineCapabilities";

ALTER TABLE "ef_temp_MachineCapabilities" RENAME TO "MachineCapabilities";

DROP TABLE "Machines";

ALTER TABLE "ef_temp_Machines" RENAME TO "Machines";

PRAGMA foreign_keys = 1;

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

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250727134953_MakeAdminOverrideReasonNullable', '8.0.11');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250727141323_FixAdminOverrideFieldsNullability', '8.0.11');


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

PRAGMA foreign_keys = 0;

DROP TABLE "Parts";

ALTER TABLE "ef_temp_Parts" RENAME TO "Parts";

PRAGMA foreign_keys = 1;

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

PRAGMA foreign_keys = 0;

DROP TABLE "Parts";

ALTER TABLE "ef_temp_Parts" RENAME TO "Parts";

PRAGMA foreign_keys = 1;

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

PRAGMA foreign_keys = 0;

DROP TABLE "BuildJobs";

ALTER TABLE "ef_temp_BuildJobs" RENAME TO "BuildJobs";

PRAGMA foreign_keys = 1;

CREATE INDEX "IX_BuildJobs_PartId" ON "BuildJobs" ("PartId");

CREATE INDEX "IX_BuildJobs_UserId" ON "BuildJobs" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250731160157_AddEDMLogEntity', '8.0.11');

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

PRAGMA foreign_keys = 0;

DROP TABLE "EDMLogs";

ALTER TABLE "ef_temp_EDMLogs" RENAME TO "EDMLogs";

PRAGMA foreign_keys = 1;

CREATE INDEX "IX_EDMLogs_PartId" ON "EDMLogs" ("PartId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250731181242_AddBugReportingSystem', '8.0.11');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250801163504_DatabaseRefactoringComplete', '8.0.11');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250801164030_FixEFRelationshipConflicts', '8.0.11');

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

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250802122703_AddCustomFieldsToProductionStages', '8.0.11');

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

PRAGMA foreign_keys = 0;

DROP TABLE "PartStageRequirements";

ALTER TABLE "ef_temp_PartStageRequirements" RENAME TO "PartStageRequirements";

DROP TABLE "PartClassifications";

ALTER TABLE "ef_temp_PartClassifications" RENAME TO "PartClassifications";

DROP TABLE "Jobs";

ALTER TABLE "ef_temp_Jobs" RENAME TO "Jobs";

PRAGMA foreign_keys = 1;

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

PRAGMA foreign_keys = 0;

DROP TABLE "StageDependencies";

ALTER TABLE "ef_temp_StageDependencies" RENAME TO "StageDependencies";

DROP TABLE "PartStageRequirements";

ALTER TABLE "ef_temp_PartStageRequirements" RENAME TO "PartStageRequirements";

DROP TABLE "ProductionStageExecutions";

ALTER TABLE "ef_temp_ProductionStageExecutions" RENAME TO "ProductionStageExecutions";

PRAGMA foreign_keys = 1;

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

PRAGMA foreign_keys = 0;

DROP TABLE "Jobs";

ALTER TABLE "ef_temp_Jobs" RENAME TO "Jobs";

PRAGMA foreign_keys = 1;

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

CREATE TABLE "BuildTimeLearningData" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BuildTimeLearningData" PRIMARY KEY AUTOINCREMENT,
    "BuildJobId" INTEGER NOT NULL,
    "MachineId" TEXT NOT NULL,
    "BuildFileHash" TEXT NULL,
    "OperatorEstimatedHours" TEXT NOT NULL,
    "ActualHours" TEXT NOT NULL,
    "VariancePercent" TEXT NOT NULL,
    "SupportComplexity" TEXT NULL,
    "TimeFactors" TEXT NULL,
    "QualityScore" TEXT NOT NULL,
    "DefectCount" INTEGER NOT NULL DEFAULT 0,
    "BuildHeight" TEXT NULL,
    "LayerCount" INTEGER NULL,
    "TotalParts" INTEGER NOT NULL DEFAULT 1,
    "PartOrientations" TEXT NULL,
    "RecordedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_BuildTimeLearningData_BuildJobs_BuildJobId" FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
);

CREATE TABLE "OperatorEstimateLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_OperatorEstimateLogs" PRIMARY KEY AUTOINCREMENT,
    "BuildJobId" INTEGER NOT NULL,
    "EstimatedHours" TEXT NOT NULL,
    "TimeFactors" TEXT NULL,
    "OperatorNotes" TEXT NULL,
    "LoggedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_OperatorEstimateLogs_BuildJobs_BuildJobId" FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
);

CREATE TABLE "PartCompletionLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartCompletionLogs" PRIMARY KEY AUTOINCREMENT,
    "BuildJobId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "GoodParts" INTEGER NOT NULL,
    "DefectiveParts" INTEGER NOT NULL,
    "ReworkParts" INTEGER NOT NULL,
    "QualityRate" TEXT NOT NULL,
    "IsPrimary" INTEGER NOT NULL DEFAULT 0,
    "InspectionNotes" TEXT NULL,
    "CompletedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_PartCompletionLogs_BuildJobs_BuildJobId" FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
);

CREATE INDEX "IX_BuildTimeLearningData_BuildFileHash" ON "BuildTimeLearningData" ("BuildFileHash");

CREATE INDEX "IX_BuildTimeLearningData_BuildFileHash_SupportComplexity" ON "BuildTimeLearningData" ("BuildFileHash", "SupportComplexity");

CREATE INDEX "IX_BuildTimeLearningData_BuildJobId" ON "BuildTimeLearningData" ("BuildJobId");

CREATE INDEX "IX_BuildTimeLearningData_MachineId" ON "BuildTimeLearningData" ("MachineId");

CREATE INDEX "IX_BuildTimeLearningData_MachineId_BuildFileHash" ON "BuildTimeLearningData" ("MachineId", "BuildFileHash");

CREATE INDEX "IX_BuildTimeLearningData_MachineId_SupportComplexity" ON "BuildTimeLearningData" ("MachineId", "SupportComplexity");

CREATE INDEX "IX_BuildTimeLearningData_MachineId_TotalParts_SupportComplexity" ON "BuildTimeLearningData" ("MachineId", "TotalParts", "SupportComplexity");

CREATE INDEX "IX_BuildTimeLearningData_QualityScore" ON "BuildTimeLearningData" ("QualityScore");

CREATE INDEX "IX_BuildTimeLearningData_RecordedAt" ON "BuildTimeLearningData" ("RecordedAt");

CREATE INDEX "IX_BuildTimeLearningData_SupportComplexity" ON "BuildTimeLearningData" ("SupportComplexity");

CREATE INDEX "IX_OperatorEstimateLogs_BuildJobId" ON "OperatorEstimateLogs" ("BuildJobId");

CREATE INDEX "IX_OperatorEstimateLogs_EstimatedHours" ON "OperatorEstimateLogs" ("EstimatedHours");

CREATE INDEX "IX_OperatorEstimateLogs_LoggedAt" ON "OperatorEstimateLogs" ("LoggedAt");

CREATE INDEX "IX_PartCompletionLogs_BuildJobId" ON "PartCompletionLogs" ("BuildJobId");

CREATE INDEX "IX_PartCompletionLogs_BuildJobId_PartNumber" ON "PartCompletionLogs" ("BuildJobId", "PartNumber");

CREATE INDEX "IX_PartCompletionLogs_CompletedAt" ON "PartCompletionLogs" ("CompletedAt");

CREATE INDEX "IX_PartCompletionLogs_IsPrimary" ON "PartCompletionLogs" ("IsPrimary");

CREATE INDEX "IX_PartCompletionLogs_PartNumber" ON "PartCompletionLogs" ("PartNumber");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250803190558_AddPhase4LearningModelTables', '8.0.11');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250924164205_AddMachineColorHexMinimal', '8.0.11');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250924164751_AddMachineColorHexProper', '8.0.11');

CREATE TABLE IF NOT EXISTS MachineOperatorAssignments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MachineId TEXT NOT NULL,
                UserId INTEGER NOT NULL,
                IsPrimary INTEGER NOT NULL,
                EffectiveFrom TEXT NULL,
                EffectiveTo TEXT NULL,
                IsActive INTEGER NOT NULL,
                CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
                LastModifiedDate TEXT NOT NULL DEFAULT (datetime('now')),
                CreatedBy TEXT NOT NULL DEFAULT 'System',
                LastModifiedBy TEXT NOT NULL DEFAULT 'System',
                CONSTRAINT CK_Assignment_DateRange CHECK ((EffectiveTo IS NULL) OR (EffectiveFrom IS NULL) OR (EffectiveTo >= EffectiveFrom)),
                FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS IX_MachineOperatorAssignments_IsActive ON MachineOperatorAssignments (IsActive);
            CREATE INDEX IF NOT EXISTS IX_MachineOperatorAssignments_MachineId ON MachineOperatorAssignments (MachineId);
            CREATE INDEX IF NOT EXISTS IX_MachineOperatorAssignments_MachineId_IsPrimary ON MachineOperatorAssignments (MachineId, IsPrimary);
            CREATE INDEX IF NOT EXISTS IX_MachineOperatorAssignments_UserId ON MachineOperatorAssignments (UserId);
            CREATE INDEX IF NOT EXISTS IX_MachineOperatorAssignments_UserId_IsActive ON MachineOperatorAssignments (UserId, IsActive);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250927145033_FixActualEndTimeColumn', '8.0.11');

ALTER TABLE "MasterParts" ADD "PartsPerBuildSingle" INTEGER NOT NULL DEFAULT 1;

ALTER TABLE "MasterParts" ADD "PartsPerBuildDouble" INTEGER NULL;

ALTER TABLE "MasterParts" ADD "PartsPerBuildTriple" INTEGER NULL;

ALTER TABLE "MasterParts" ADD "EnableDoubleStack" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "MasterParts" ADD "EnableTripleStack" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "MasterParts" ADD "StageEstimateSingle" REAL NULL;


                UPDATE MasterParts
                SET PartsPerBuildSingle = 1
                WHERE PartsPerBuildSingle IS NULL OR PartsPerBuildSingle < 1;
            


                UPDATE MasterParts
                SET EnableDoubleStack = 1,
                    PartsPerBuildDouble = COALESCE(PartsPerBuildDouble, 1)
                WHERE DoubleStackDurationHours IS NOT NULL;

                UPDATE MasterParts
                SET EnableTripleStack = 1,
                    PartsPerBuildTriple = COALESCE(PartsPerBuildTriple, 1)
                WHERE TripleStackDurationHours IS NOT NULL;
            

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250928171551_Add_SLSBuildModeFields_To_MasterPart', '8.0.11');

ALTER TABLE "Jobs" ADD "ActualUnitsPlanned" INTEGER NULL;

ALTER TABLE "Jobs" ADD "LastStatusChangeUtc" TEXT NULL;

ALTER TABLE "Jobs" ADD "MasterPartId" INTEGER NULL;

ALTER TABLE "Jobs" ADD "OperatorUserId" INTEGER NULL;

ALTER TABLE "Jobs" ADD "PartsPerBuild" INTEGER NULL;

ALTER TABLE "Jobs" ADD "PlannedEndUtc" TEXT NULL;

ALTER TABLE "Jobs" ADD "PlannedStackDurationHours" REAL NULL;

ALTER TABLE "Jobs" ADD "PowderAddedKg" decimal(8,2) NULL;

ALTER TABLE "Jobs" ADD "PowderMaterial" TEXT NULL;

ALTER TABLE "Jobs" ADD "PredecessorJobId" INTEGER NULL;

ALTER TABLE "Jobs" ADD "PrototypeUnitsPlanned" INTEGER NULL;

ALTER TABLE "Jobs" ADD "StackLevel" INTEGER NULL;

CREATE INDEX "IX_Jobs_MasterPartId" ON "Jobs" ("MasterPartId");

CREATE INDEX "IX_Jobs_OperatorUserId" ON "Jobs" ("OperatorUserId");

CREATE INDEX "IX_Jobs_PredecessorJobId" ON "Jobs" ("PredecessorJobId");

CREATE TABLE "ef_temp_Jobs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Jobs" PRIMARY KEY AUTOINCREMENT,
    "ActualEnd" TEXT NULL,
    "ActualPowderUsageKg" REAL NOT NULL,
    "ActualStart" TEXT NULL,
    "ActualUnitsPlanned" INTEGER NULL,
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
    "LastStatusChangeUtc" TEXT NULL,
    "LayerThicknessMicrons" REAL NOT NULL DEFAULT 30.0,
    "MachineId" TEXT NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "MachineUtilizationPercent" REAL NOT NULL,
    "MasterPartId" INTEGER NULL,
    "MaterialCostPerKg" decimal(10,2) NOT NULL,
    "Notes" TEXT NULL,
    "OpcUaBuildProgress" REAL NOT NULL,
    "OpcUaErrorMessages" TEXT NULL,
    "OpcUaJobId" TEXT NULL,
    "OpcUaLastUpdate" TEXT NULL,
    "OpcUaStatus" TEXT NULL,
    "Operator" TEXT NULL,
    "OperatorUserId" INTEGER NULL,
    "OxygenContentPpm" REAL NOT NULL DEFAULT 50.0,
    "PartId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "PartsPerBuild" INTEGER NULL,
    "PlannedEndUtc" TEXT NULL,
    "PlannedStackDurationHours" REAL NULL,
    "PostProcessingTimeMinutes" REAL NOT NULL,
    "PowderAddedKg" decimal(8,2) NULL,
    "PowderChangeoverTimeMinutes" REAL NOT NULL,
    "PowderExpirationDate" TEXT NULL,
    "PowderLotNumber" TEXT NOT NULL,
    "PowderMaterial" TEXT NULL,
    "PowderRecyclePercentage" REAL NOT NULL,
    "PowerCostPerKwh" decimal(10,2) NOT NULL,
    "PredecessorJobId" INTEGER NULL,
    "PreheatingTimeMinutes" REAL NOT NULL,
    "PreviousJobPartNumber" TEXT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "ProcessParameters" TEXT NOT NULL,
    "ProducedQuantity" INTEGER NOT NULL,
    "PrototypeUnitsPlanned" INTEGER NULL,
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
    "StackLevel" INTEGER NULL,
    "StageOrder" INTEGER NULL,
    "Status" TEXT NOT NULL,
    "Supervisor" TEXT NULL,
    "SurfaceRoughnessRa" REAL NOT NULL,
    "TotalStages" INTEGER NULL,
    "UltimateTensileStrengthMPa" REAL NOT NULL,
    "WorkflowStage" TEXT NULL,
    CONSTRAINT "FK_Jobs_BuildCohorts_BuildCohortId" FOREIGN KEY ("BuildCohortId") REFERENCES "BuildCohorts" ("Id"),
    CONSTRAINT "FK_Jobs_Jobs_PredecessorJobId" FOREIGN KEY ("PredecessorJobId") REFERENCES "Jobs" ("Id"),
    CONSTRAINT "FK_Jobs_MasterParts_MasterPartId" FOREIGN KEY ("MasterPartId") REFERENCES "MasterParts" ("Id"),
    CONSTRAINT "FK_Jobs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Jobs_Users_OperatorUserId" FOREIGN KEY ("OperatorUserId") REFERENCES "Users" ("Id")
);

INSERT INTO "ef_temp_Jobs" ("Id", "ActualEnd", "ActualPowderUsageKg", "ActualStart", "ActualUnitsPlanned", "ArgonCostPerHour", "ArgonPurityPercent", "BuildCohortId", "BuildFileCreatedDate", "BuildFileName", "BuildFilePath", "BuildFileSizeBytes", "BuildLayerNumber", "BuildPlatformId", "BuildTemperatureCelsius", "BuildTimeMinutes", "ChangeoverTimeMinutes", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CurrentArgonFlowRate", "CurrentBuildTemperature", "CurrentLaserPowerWatts", "CurrentOxygenLevel", "CustomerDueDate", "CustomerOrderNumber", "DefectQuantity", "DensityPercentage", "EnergyConsumptionKwh", "EstimatedDuration", "EstimatedHours", "EstimatedPowderUsageKg", "HatchSpacingMicrons", "HoldReason", "IsRushJob", "LaborCostPerHour", "LaserPowerWatts", "LastModifiedBy", "LastModifiedDate", "LastStatusChangeUtc", "LayerThicknessMicrons", "MachineId", "MachineOperatingCostPerHour", "MachineUtilizationPercent", "MasterPartId", "MaterialCostPerKg", "Notes", "OpcUaBuildProgress", "OpcUaErrorMessages", "OpcUaJobId", "OpcUaLastUpdate", "OpcUaStatus", "Operator", "OperatorUserId", "OxygenContentPpm", "PartId", "PartNumber", "PartsPerBuild", "PlannedEndUtc", "PlannedStackDurationHours", "PostProcessingTimeMinutes", "PowderAddedKg", "PowderChangeoverTimeMinutes", "PowderExpirationDate", "PowderLotNumber", "PowderMaterial", "PowderRecyclePercentage", "PowerCostPerKwh", "PredecessorJobId", "PreheatingTimeMinutes", "PreviousJobPartNumber", "Priority", "ProcessParameters", "ProducedQuantity", "PrototypeUnitsPlanned", "QualityCheckpoints", "QualityInspector", "Quantity", "RequiredMaterials", "RequiredSkills", "RequiredTooling", "RequiresArgonPurge", "RequiresPostProcessing", "RequiresPowderSieving", "RequiresPreheating", "ReworkQuantity", "ScanSpeedMmPerSec", "ScheduledEnd", "ScheduledStart", "SetupTimeMinutes", "SlsMaterial", "SpecialInstructions", "StackLevel", "StageOrder", "Status", "Supervisor", "SurfaceRoughnessRa", "TotalStages", "UltimateTensileStrengthMPa", "WorkflowStage")
SELECT "Id", "ActualEnd", "ActualPowderUsageKg", "ActualStart", "ActualUnitsPlanned", "ArgonCostPerHour", "ArgonPurityPercent", "BuildCohortId", "BuildFileCreatedDate", "BuildFileName", "BuildFilePath", "BuildFileSizeBytes", "BuildLayerNumber", "BuildPlatformId", "BuildTemperatureCelsius", "BuildTimeMinutes", "ChangeoverTimeMinutes", "CoolingTimeMinutes", "CreatedBy", "CreatedDate", "CurrentArgonFlowRate", "CurrentBuildTemperature", "CurrentLaserPowerWatts", "CurrentOxygenLevel", "CustomerDueDate", "CustomerOrderNumber", "DefectQuantity", "DensityPercentage", "EnergyConsumptionKwh", "EstimatedDuration", "EstimatedHours", "EstimatedPowderUsageKg", "HatchSpacingMicrons", "HoldReason", "IsRushJob", "LaborCostPerHour", "LaserPowerWatts", "LastModifiedBy", "LastModifiedDate", "LastStatusChangeUtc", "LayerThicknessMicrons", "MachineId", "MachineOperatingCostPerHour", "MachineUtilizationPercent", "MasterPartId", "MaterialCostPerKg", "Notes", "OpcUaBuildProgress", "OpcUaErrorMessages", "OpcUaJobId", "OpcUaLastUpdate", "OpcUaStatus", "Operator", "OperatorUserId", "OxygenContentPpm", "PartId", "PartNumber", "PartsPerBuild", "PlannedEndUtc", "PlannedStackDurationHours", "PostProcessingTimeMinutes", "PowderAddedKg", "PowderChangeoverTimeMinutes", "PowderExpirationDate", "PowderLotNumber", "PowderMaterial", "PowderRecyclePercentage", "PowerCostPerKwh", "PredecessorJobId", "PreheatingTimeMinutes", "PreviousJobPartNumber", "Priority", "ProcessParameters", "ProducedQuantity", "PrototypeUnitsPlanned", "QualityCheckpoints", "QualityInspector", "Quantity", "RequiredMaterials", "RequiredSkills", "RequiredTooling", "RequiresArgonPurge", "RequiresPostProcessing", "RequiresPowderSieving", "RequiresPreheating", "ReworkQuantity", "ScanSpeedMmPerSec", "ScheduledEnd", "ScheduledStart", "SetupTimeMinutes", "SlsMaterial", "SpecialInstructions", "StackLevel", "StageOrder", "Status", "Supervisor", "SurfaceRoughnessRa", "TotalStages", "UltimateTensileStrengthMPa", "WorkflowStage"
FROM "Jobs";

PRAGMA foreign_keys = 0;

DROP TABLE "Jobs";

ALTER TABLE "ef_temp_Jobs" RENAME TO "Jobs";

PRAGMA foreign_keys = 1;

CREATE INDEX "IX_Jobs_BuildCohortId" ON "Jobs" ("BuildCohortId");

CREATE INDEX "IX_Jobs_MachineId_ScheduledStart" ON "Jobs" ("MachineId", "ScheduledStart");

CREATE INDEX "IX_Jobs_MasterPartId" ON "Jobs" ("MasterPartId");

CREATE INDEX "IX_Jobs_OperatorUserId" ON "Jobs" ("OperatorUserId");

CREATE INDEX "IX_Jobs_PartId" ON "Jobs" ("PartId");

CREATE INDEX "IX_Jobs_PartNumber" ON "Jobs" ("PartNumber");

CREATE INDEX "IX_Jobs_PredecessorJobId" ON "Jobs" ("PredecessorJobId");

CREATE INDEX "IX_Jobs_Priority" ON "Jobs" ("Priority");

CREATE INDEX "IX_Jobs_Status" ON "Jobs" ("Status");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250930160505_Add_JobStackFields', '8.0.11');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250930160805_Add_JobStatusHistory_And_StackDurationLearning', '8.0.11');

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

CREATE TABLE "AdminAlerts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AdminAlerts" PRIMARY KEY AUTOINCREMENT,
    "AlertName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Category" TEXT NOT NULL,
    "TriggerType" TEXT NOT NULL,
    "TriggerConditions" TEXT NOT NULL,
    "SeverityLevel" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "EmailRecipients" TEXT NOT NULL,
    "EmailSubject" TEXT NOT NULL,
    "EmailTemplate" TEXT NOT NULL,
    "SendSms" INTEGER NOT NULL,
    "SmsRecipients" TEXT NOT NULL,
    "SmsTemplate" TEXT NOT NULL,
    "SendBrowserNotification" INTEGER NOT NULL,
    "CooldownMinutes" INTEGER NOT NULL,
    "LastTriggered" TEXT NULL,
    "TriggerCount" INTEGER NOT NULL,
    "EscalationRules" TEXT NOT NULL,
    "BusinessHoursOnly" INTEGER NOT NULL,
    "BusinessHoursStart" TEXT NOT NULL,
    "BusinessHoursEnd" TEXT NOT NULL,
    "BusinessDays" TEXT NOT NULL,
    "MaxAlertsPerDay" INTEGER NOT NULL,
    "TriggersToday" INTEGER NOT NULL,
    "LastDailyReset" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL
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
    "Quantity" INTEGER NOT NULL,
    "ProducedQuantity" INTEGER NOT NULL,
    "DefectQuantity" INTEGER NOT NULL,
    "ReworkQuantity" INTEGER NOT NULL,
    "Status" TEXT NOT NULL,
    "Priority" INTEGER NOT NULL,
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
    "ProcessParameters" TEXT NOT NULL,
    "QualityCheckpoints" TEXT NOT NULL,
    "ArchivedDate" TEXT NOT NULL,
    "ArchivedBy" TEXT NOT NULL,
    "ArchiveReason" TEXT NOT NULL,
    "OriginalCreatedDate" TEXT NOT NULL,
    "OriginalLastModifiedDate" TEXT NOT NULL,
    "OriginalCreatedBy" TEXT NOT NULL,
    "OriginalLastModifiedBy" TEXT NOT NULL
);

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

CREATE TABLE "ComplianceCategories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ComplianceCategories" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "RegulatoryLevel" TEXT NOT NULL,
    "RequiresSpecialHandling" INTEGER NOT NULL DEFAULT 0,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "SortOrder" INTEGER NOT NULL DEFAULT 100,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

CREATE TABLE "ComponentTypes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ComponentTypes" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "SortOrder" INTEGER NOT NULL DEFAULT 100,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

CREATE TABLE "CrmAccounts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CrmAccounts" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Active',
    "Notes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE "DefectCategories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_DefectCategories" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Code" TEXT NOT NULL,
    "SeverityLevel" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "CategoryGroup" TEXT NOT NULL DEFAULT 'General',
    "ApplicableProcesses" TEXT NOT NULL,
    "StandardCorrectiveActions" TEXT NOT NULL,
    "PreventionMethods" TEXT NOT NULL,
    "RequiresImmediateNotification" INTEGER NOT NULL,
    "CostImpact" TEXT NOT NULL DEFAULT 'Medium',
    "AverageResolutionTimeMinutes" INTEGER NOT NULL,
    "SortOrder" INTEGER NOT NULL,
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
    "IsEnabled" INTEGER NOT NULL,
    "Category" TEXT NOT NULL,
    "Environment" TEXT NOT NULL,
    "RequiredRole" TEXT NOT NULL,
    "RolloutPercentage" INTEGER NOT NULL,
    "StartDate" TEXT NULL,
    "EndDate" TEXT NULL,
    "RequiresRestart" INTEGER NOT NULL,
    "Dependencies" TEXT NOT NULL,
    "Conflicts" TEXT NOT NULL,
    "Configuration" TEXT NOT NULL,
    "UsageCount" INTEGER NOT NULL,
    "LastUsed" TEXT NULL,
    "PerformanceNotes" TEXT NOT NULL,
    "SecurityNotes" TEXT NOT NULL,
    "IntroducedInVersion" TEXT NOT NULL,
    "PlannedRemovalVersion" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "SortOrder" INTEGER NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL
);

CREATE TABLE "JobLogEntries" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_JobLogEntries" PRIMARY KEY AUTOINCREMENT,
    "Timestamp" TEXT NOT NULL,
    "MachineId" TEXT NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "Action" TEXT NOT NULL,
    "Notes" TEXT NULL,
    "Operator" TEXT NULL
);

CREATE TABLE "LegacyFlagToStageMaps" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_LegacyFlagToStageMaps" PRIMARY KEY AUTOINCREMENT,
    "LegacyFieldName" TEXT NOT NULL,
    "ProductionStageName" TEXT NOT NULL,
    "ExecutionOrder" INTEGER NOT NULL DEFAULT 1,
    "DefaultSetupMinutes" INTEGER NOT NULL DEFAULT 30,
    "DefaultTeardownMinutes" INTEGER NOT NULL DEFAULT 0,
    "IsActive" INTEGER NOT NULL DEFAULT 1
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

CREATE TABLE "MasterParts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MasterParts" PRIMARY KEY AUTOINCREMENT,
    "PartNumber" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Material" TEXT NOT NULL,
    "ManufacturingApproach" TEXT NOT NULL DEFAULT 'SLS-Based',
    "AllowStacking" INTEGER NOT NULL,
    "SingleStackDurationHours" REAL NULL,
    "DoubleStackDurationHours" REAL NULL,
    "TripleStackDurationHours" REAL NULL,
    "MaxStackCount" INTEGER NOT NULL,
    "PartsPerBuildSingle" INTEGER NOT NULL,
    "PartsPerBuildDouble" INTEGER NULL,
    "PartsPerBuildTriple" INTEGER NULL,
    "EnableDoubleStack" INTEGER NOT NULL,
    "EnableTripleStack" INTEGER NOT NULL,
    "StageEstimateSingle" REAL NULL,
    "RequiredStages" TEXT NOT NULL DEFAULT '[]',
    "IsActive" INTEGER NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

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
    "QuantityOnHandKg" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL
);

CREATE TABLE "OperatingShifts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_OperatingShifts" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NULL,
    "DayOfWeek" INTEGER NOT NULL,
    "StartTime" TEXT NOT NULL,
    "EndTime" TEXT NOT NULL,
    "IsHoliday" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "Description" TEXT NOT NULL,
    "SpecificDate" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

CREATE TABLE "PartClassifications" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartClassifications" PRIMARY KEY AUTOINCREMENT,
    "ClassificationCode" TEXT NOT NULL,
    "ClassificationName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "IndustryType" TEXT NOT NULL,
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
    "RecommendedMaterial" TEXT NOT NULL,
    "AlternativeMaterials" TEXT NOT NULL,
    "MaterialGrade" TEXT NOT NULL,
    "RequiresSpecialHandling" INTEGER NOT NULL,
    "RequiredProcess" TEXT NOT NULL,
    "PostProcessingRequired" TEXT NOT NULL,
    "ComplexityLevel" INTEGER NOT NULL,
    "SpecialInstructions" TEXT NOT NULL,
    "RequiresPressureTesting" INTEGER NOT NULL,
    "RequiresProofTesting" INTEGER NOT NULL,
    "RequiresDimensionalVerification" INTEGER NOT NULL,
    "RequiresSurfaceFinishVerification" INTEGER NOT NULL,
    "RequiresMaterialCertification" INTEGER NOT NULL,
    "TestingRequirements" TEXT NOT NULL,
    "QualityStandards" TEXT NOT NULL,
    "RequiresATFCompliance" INTEGER NOT NULL,
    "RequiresITARCompliance" INTEGER NOT NULL,
    "RequiresFFLTracking" INTEGER NOT NULL,
    "RequiresSerialization" INTEGER NOT NULL,
    "IsControlledItem" INTEGER NOT NULL,
    "IsEARControlled" INTEGER NOT NULL,
    "ExportClassification" TEXT NOT NULL,
    "RegulatoryNotes" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL
);

CREATE TABLE "ProductionStages" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductionStages" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "DisplayOrder" INTEGER NOT NULL,
    "Description" TEXT NULL,
    "DefaultSetupMinutes" INTEGER NOT NULL,
    "DefaultHourlyRate" decimal(8,2) NOT NULL,
    "RequiresQualityCheck" INTEGER NOT NULL,
    "RequiresApproval" INTEGER NOT NULL,
    "AllowSkip" INTEGER NOT NULL,
    "IsOptional" INTEGER NOT NULL,
    "RequiredRole" TEXT NULL,
    "CustomFieldsConfig" TEXT NOT NULL,
    "AssignedMachineIds" TEXT NULL,
    "RequiresMachineAssignment" INTEGER NOT NULL,
    "DefaultMachineId" TEXT NULL,
    "StageColor" TEXT NOT NULL,
    "StageIcon" TEXT NOT NULL,
    "Department" TEXT NULL,
    "AllowParallelExecution" INTEGER NOT NULL,
    "DefaultMaterialCost" decimal(10,2) NOT NULL,
    "DefaultDurationHours" REAL NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL
);

CREATE TABLE "ResourcePools" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ResourcePools" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "ResourceType" TEXT NOT NULL,
    "ResourceConfiguration" TEXT NOT NULL,
    "MaxConcurrentAllocations" INTEGER NOT NULL,
    "AutoAssign" INTEGER NOT NULL,
    "AssignmentCriteria" TEXT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL
);

CREATE TABLE "RolePermissions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_RolePermissions" PRIMARY KEY AUTOINCREMENT,
    "RoleName" TEXT NOT NULL,
    "PermissionKey" TEXT NOT NULL,
    "HasPermission" INTEGER NOT NULL,
    "PermissionLevel" TEXT NOT NULL DEFAULT 'Read',
    "Category" TEXT NOT NULL DEFAULT 'General',
    "Description" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "Priority" INTEGER NOT NULL,
    "Constraints" TEXT NOT NULL DEFAULT '{}',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
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

CREATE TABLE "StageTemplateCategories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StageTemplateCategories" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL DEFAULT '',
    "Icon" TEXT NOT NULL DEFAULT 'fas fa-cogs',
    "ColorCode" TEXT NOT NULL DEFAULT '#007bff',
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "SortOrder" INTEGER NOT NULL DEFAULT 100
);

CREATE TABLE "SystemSettings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SystemSettings" PRIMARY KEY AUTOINCREMENT,
    "SettingKey" TEXT NOT NULL,
    "SettingValue" TEXT NOT NULL,
    "DataType" TEXT NOT NULL DEFAULT 'String',
    "Category" TEXT NOT NULL DEFAULT 'General',
    "Description" TEXT NOT NULL,
    "DefaultValue" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "IsReadOnly" INTEGER NOT NULL,
    "RequiresRestart" INTEGER NOT NULL,
    "ValidationRules" TEXT NOT NULL DEFAULT '',
    "DisplayOrder" INTEGER NOT NULL,
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
    "IsActive" INTEGER NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastLoginDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL
);

CREATE TABLE "WorkflowTemplates" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_WorkflowTemplates" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Category" TEXT NULL,
    "Complexity" TEXT NOT NULL,
    "EstimatedDurationHours" REAL NOT NULL,
    "EstimatedCost" decimal(12,2) NOT NULL,
    "StageConfiguration" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL
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

CREATE TABLE "StageDefinitions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StageDefinitions" PRIMARY KEY AUTOINCREMENT,
    "MasterPartId" INTEGER NOT NULL,
    "StageName" TEXT NOT NULL,
    "ExecutionOrder" INTEGER NOT NULL,
    "IsRequired" INTEGER NOT NULL,
    "CanSkip" INTEGER NOT NULL,
    "EstimatedHoursPerPart" REAL NOT NULL,
    "SetupMinutes" INTEGER NOT NULL,
    "TeardownMinutes" INTEGER NOT NULL,
    "RequiredMachineType" TEXT NULL,
    "PreferredMachines" TEXT NULL,
    "StageConfiguration" TEXT NOT NULL DEFAULT '{}',
    "QualityRequirements" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    CONSTRAINT "FK_StageDefinitions_MasterParts_MasterPartId" FOREIGN KEY ("MasterPartId") REFERENCES "MasterParts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ComplianceRequirements" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ComplianceRequirements" PRIMARY KEY AUTOINCREMENT,
    "RequirementCode" TEXT NOT NULL,
    "RequirementName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "ComplianceType" TEXT NOT NULL,
    "RegulatoryAuthority" TEXT NOT NULL,
    "RequirementDetails" TEXT NOT NULL,
    "DocumentationRequired" TEXT NOT NULL,
    "FormsRequired" TEXT NOT NULL,
    "RecordKeepingRequirements" TEXT NOT NULL,
    "ApplicableIndustries" TEXT NOT NULL,
    "ApplicablePartTypes" TEXT NOT NULL,
    "ApplicableProcesses" TEXT NOT NULL,
    "AppliesToManufacturing" INTEGER NOT NULL,
    "AppliesToDistribution" INTEGER NOT NULL,
    "AppliesToExport" INTEGER NOT NULL,
    "AppliesToImport" INTEGER NOT NULL,
    "EnforcementLevel" TEXT NOT NULL,
    "PenaltyType" TEXT NOT NULL,
    "PenaltyDescription" TEXT NOT NULL,
    "MaxPenaltyDays" INTEGER NOT NULL,
    "MaxPenaltyAmount" decimal(12,2) NOT NULL,
    "EffectiveDate" TEXT NULL,
    "ExpirationDate" TEXT NULL,
    "NextReviewDate" TEXT NULL,
    "RenewalIntervalMonths" INTEGER NOT NULL,
    "RequiresRenewal" INTEGER NOT NULL,
    "RequiresInspection" INTEGER NOT NULL,
    "RenewalProcess" TEXT NOT NULL,
    "ImplementationSteps" TEXT NOT NULL,
    "RequiredTraining" TEXT NOT NULL,
    "RequiredCertifications" TEXT NOT NULL,
    "SystemRequirements" TEXT NOT NULL,
    "EstimatedImplementationHours" REAL NOT NULL,
    "EstimatedImplementationCost" decimal(10,2) NOT NULL,
    "ReferenceDocuments" TEXT NOT NULL,
    "WebResources" TEXT NOT NULL,
    "ContactInformation" TEXT NOT NULL,
    "AdditionalNotes" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "IsCurrentVersion" INTEGER NOT NULL,
    "PartClassificationId" INTEGER NULL,
    CONSTRAINT "FK_ComplianceRequirements_PartClassifications_PartClassificationId" FOREIGN KEY ("PartClassificationId") REFERENCES "PartClassifications" ("Id")
);

CREATE TABLE "ProductionStageDependencies" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductionStageDependencies" PRIMARY KEY AUTOINCREMENT,
    "DependentStageId" INTEGER NOT NULL,
    "PrerequisiteStageId" INTEGER NOT NULL,
    "DependencyType" TEXT NOT NULL,
    "DelayHours" INTEGER NOT NULL,
    "IsOptional" INTEGER NOT NULL,
    "Condition" TEXT NULL,
    "Notes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    CONSTRAINT "FK_ProductionStageDependencies_ProductionStages_DependentStageId" FOREIGN KEY ("DependentStageId") REFERENCES "ProductionStages" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ProductionStageDependencies_ProductionStages_PrerequisiteStageId" FOREIGN KEY ("PrerequisiteStageId") REFERENCES "ProductionStages" ("Id") ON DELETE CASCADE
);

CREATE TABLE "StageTemplates" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StageTemplates" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Industry" TEXT NOT NULL DEFAULT 'General',
    "MaterialType" TEXT NOT NULL DEFAULT 'Metal',
    "ComplexityLevel" TEXT NOT NULL DEFAULT 'Medium',
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "IsDefault" INTEGER NOT NULL DEFAULT 0,
    "SortOrder" INTEGER NOT NULL DEFAULT 100,
    "TemplateConfiguration" TEXT NOT NULL DEFAULT '{}',
    "EstimatedTotalHours" TEXT NOT NULL,
    "EstimatedTotalCost" TEXT NOT NULL,
    "UsageCount" INTEGER NOT NULL DEFAULT 0,
    "LastUsedDate" TEXT NULL,
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "StageTemplateCategoryId" INTEGER NULL,
    CONSTRAINT "FK_StageTemplates_StageTemplateCategories_StageTemplateCategoryId" FOREIGN KEY ("StageTemplateCategoryId") REFERENCES "StageTemplateCategories" ("Id")
);

CREATE TABLE "MachineOperatorAssignments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MachineOperatorAssignments" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NOT NULL,
    "UserId" INTEGER NOT NULL,
    "IsPrimary" INTEGER NOT NULL,
    "EffectiveFrom" TEXT NULL,
    "EffectiveTo" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    CONSTRAINT "CK_Assignment_DateRange" CHECK ((EffectiveTo IS NULL) OR (EffectiveFrom IS NULL) OR (EffectiveTo >= EffectiveFrom)),
    CONSTRAINT "FK_MachineOperatorAssignments_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ProductionBuilds" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductionBuilds" PRIMARY KEY AUTOINCREMENT,
    "BuildNumber" TEXT NOT NULL,
    "MasterPartId" INTEGER NOT NULL,
    "PrinterName" TEXT NOT NULL,
    "BuildQuantity" INTEGER NOT NULL,
    "StackLevel" INTEGER NOT NULL,
    "MaterialBatch" TEXT NOT NULL,
    "PowderLot" TEXT NOT NULL DEFAULT '',
    "AddedPowder" INTEGER NOT NULL,
    "PowderAmountKg" TEXT NULL,
    "ActualStartTime" TEXT NULL,
    "ActualEndTime" TEXT NULL,
    "ScheduledStartTime" TEXT NULL,
    "ScheduledEndTime" TEXT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Planned',
    "CreatedByUserId" INTEGER NOT NULL,
    "SetupNotes" TEXT NULL,
    "CompletionNotes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CompletedDate" TEXT NULL,
    CONSTRAINT "FK_ProductionBuilds_MasterParts_MasterPartId" FOREIGN KEY ("MasterPartId") REFERENCES "MasterParts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ProductionBuilds_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "UserSettings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_UserSettings" PRIMARY KEY AUTOINCREMENT,
    "UserId" INTEGER NOT NULL,
    "SessionTimeoutMinutes" INTEGER NOT NULL,
    "Theme" TEXT NOT NULL,
    "SchedulerOrientation" TEXT NOT NULL,
    "EmailNotifications" INTEGER NOT NULL,
    "BrowserNotifications" INTEGER NOT NULL,
    "DefaultPage" TEXT NOT NULL,
    "ItemsPerPage" INTEGER NOT NULL,
    "TimeZone" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    CONSTRAINT "FK_UserSettings_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
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

CREATE TABLE "Parts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Parts" PRIMARY KEY AUTOINCREMENT,
    "PartNumber" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Material" TEXT NOT NULL,
    "ManufacturingStage" TEXT NOT NULL,
    "StageDetails" TEXT NOT NULL,
    "StageOrder" INTEGER NOT NULL,
    "RequiresSLSPrinting" INTEGER NOT NULL,
    "RequiresCNCMachining" INTEGER NOT NULL,
    "RequiresEDMOperations" INTEGER NOT NULL,
    "RequiresAssembly" INTEGER NOT NULL,
    "RequiresFinishing" INTEGER NOT NULL,
    "BTComponentType" TEXT NOT NULL DEFAULT 'General',
    "BTFirearmCategory" TEXT NOT NULL,
    "BTSuppressorType" TEXT NOT NULL,
    "BTBafflePosition" TEXT NOT NULL,
    "BTCaliberCompatibility" TEXT NOT NULL,
    "BTThreadPitch" TEXT NOT NULL,
    "BTSoundReductionDB" REAL NULL,
    "BTBackPressurePSI" REAL NULL,
    "RequiresATFCompliance" INTEGER NOT NULL,
    "RequiresITARCompliance" INTEGER NOT NULL,
    "RequiresFFLTracking" INTEGER NOT NULL,
    "RequiresSerialization" INTEGER NOT NULL,
    "IsControlledItem" INTEGER NOT NULL,
    "IsEARControlled" INTEGER NOT NULL,
    "RequiresATFForm1" INTEGER NOT NULL,
    "RequiresATFForm4" INTEGER NOT NULL,
    "ATFClassification" TEXT NOT NULL,
    "FFLRequirements" TEXT NOT NULL,
    "RequiresTaxStamp" INTEGER NOT NULL,
    "TaxStampAmount" decimal(10,2) NULL,
    "ITARCategory" TEXT NOT NULL,
    "EARClassification" TEXT NOT NULL,
    "RequiresExportLicense" INTEGER NOT NULL,
    "ExportControlNotes" TEXT NOT NULL,
    "ExportClassification" TEXT NOT NULL DEFAULT '',
    "FirearmType" TEXT NOT NULL DEFAULT '',
    "RequiresBTProofTesting" INTEGER NOT NULL,
    "ProofTestPressure" REAL NULL,
    "RequiresSoundTesting" INTEGER NOT NULL,
    "RequiresBackPressureTesting" INTEGER NOT NULL,
    "RequiresThreadVerification" INTEGER NOT NULL,
    "BTTestingProtocol" TEXT NOT NULL,
    "BTQualitySpecification" TEXT NOT NULL,
    "RequiresPressureTesting" INTEGER NOT NULL,
    "RequiresProofTesting" INTEGER NOT NULL,
    "RequiresDimensionalVerification" INTEGER NOT NULL,
    "RequiresSurfaceFinishVerification" INTEGER NOT NULL,
    "RequiresMaterialCertification" INTEGER NOT NULL,
    "BTTestingRequirements" TEXT NOT NULL DEFAULT '',
    "BTQualityStandards" TEXT NOT NULL DEFAULT '',
    "BTRegulatoryNotes" TEXT NOT NULL DEFAULT '',
    "SerialNumberFormat" TEXT NOT NULL,
    "RequiresUniqueSerialNumber" INTEGER NOT NULL,
    "BatchControlMethod" TEXT NOT NULL,
    "MaxBatchSize" INTEGER NOT NULL,
    "RequiresTraceabilityDocuments" INTEGER NOT NULL,
    "ParentComponents" TEXT NOT NULL,
    "ChildComponents" TEXT NOT NULL,
    "IsAssemblyComponent" INTEGER NOT NULL,
    "IsSubAssembly" INTEGER NOT NULL,
    "BTLicensingCost" decimal(10,2) NOT NULL,
    "ComplianceCost" decimal(10,2) NOT NULL,
    "TestingCost" decimal(10,2) NOT NULL,
    "DocumentationCost" decimal(10,2) NOT NULL,
    "WorkflowTemplate" TEXT NOT NULL,
    "ApprovalWorkflow" TEXT NOT NULL,
    "RequiresEngineeringApproval" INTEGER NOT NULL,
    "RequiresQualityApproval" INTEGER NOT NULL,
    "RequiresComplianceApproval" INTEGER NOT NULL,
    "SlsMaterial" TEXT NOT NULL,
    "PowderSpecification" TEXT NOT NULL DEFAULT '15-45 micron particle size',
    "PowderRequirementKg" REAL NOT NULL,
    "RecommendedLaserPower" REAL NOT NULL,
    "RecommendedScanSpeed" REAL NOT NULL,
    "RecommendedLayerThickness" REAL NOT NULL,
    "RecommendedHatchSpacing" REAL NOT NULL,
    "RecommendedBuildTemperature" REAL NOT NULL,
    "RequiredArgonPurity" REAL NOT NULL,
    "MaxOxygenContent" REAL NOT NULL,
    "WeightGrams" REAL NOT NULL,
    "Dimensions" TEXT NOT NULL DEFAULT '',
    "VolumeMm3" REAL NOT NULL,
    "HeightMm" REAL NOT NULL,
    "LengthMm" REAL NOT NULL,
    "WidthMm" REAL NOT NULL,
    "SurfaceFinishRequirement" TEXT NOT NULL DEFAULT 'As-built',
    "MaxSurfaceRoughnessRa" REAL NOT NULL,
    "MaterialCostPerKg" decimal(12,2) NOT NULL,
    "StandardLaborCostPerHour" decimal(10,2) NOT NULL,
    "SetupCost" decimal(10,2) NOT NULL,
    "PostProcessingCost" decimal(10,2) NOT NULL,
    "QualityInspectionCost" decimal(10,2) NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "ProcessType" TEXT NOT NULL DEFAULT 'SLS Metal',
    "RequiredMachineType" TEXT NOT NULL DEFAULT 'TruPrint 3000',
    "PreferredMachines" TEXT NOT NULL DEFAULT 'TI1,TI2',
    "SetupTimeMinutes" REAL NOT NULL,
    "PowderChangeoverTimeMinutes" REAL NOT NULL,
    "PreheatingTimeMinutes" REAL NOT NULL,
    "CoolingTimeMinutes" REAL NOT NULL,
    "PostProcessingTimeMinutes" REAL NOT NULL,
    "QualityStandards" TEXT NOT NULL DEFAULT 'ASTM F3001, ISO 17296',
    "ToleranceRequirements" TEXT NOT NULL DEFAULT '+/-0.1mm typical',
    "RequiresInspection" INTEGER NOT NULL,
    "RequiresCertification" INTEGER NOT NULL,
    "RequiredSkills" TEXT NOT NULL DEFAULT 'SLS Operation,Powder Handling',
    "RequiredCertifications" TEXT NOT NULL DEFAULT 'SLS Operation Certification',
    "RequiredTooling" TEXT NOT NULL DEFAULT 'Build Platform,Powder Sieve',
    "ConsumableMaterials" TEXT NOT NULL DEFAULT 'Argon Gas,Build Platform Coating',
    "RequiresSupports" INTEGER NOT NULL,
    "SupportStrategy" TEXT NOT NULL DEFAULT 'Minimal supports on overhangs > 45°',
    "SupportRemovalTimeMinutes" REAL NOT NULL,
    "CustomerPartNumber" TEXT NOT NULL DEFAULT '',
    "PartCategory" TEXT NOT NULL DEFAULT 'Prototype',
    "PartClass" TEXT NOT NULL DEFAULT 'B',
    "IsActive" INTEGER NOT NULL,
    "Industry" TEXT NOT NULL,
    "Application" TEXT NOT NULL,
    "RequiresFDA" INTEGER NOT NULL,
    "RequiresAS9100" INTEGER NOT NULL,
    "RequiresNADCAP" INTEGER NOT NULL,
    "AverageActualHours" REAL NOT NULL,
    "AverageEfficiencyPercent" REAL NOT NULL,
    "AverageQualityScore" REAL NOT NULL,
    "AverageDefectRate" REAL NOT NULL,
    "AveragePowderUtilization" REAL NOT NULL,
    "TotalJobsCompleted" INTEGER NOT NULL,
    "TotalUnitsProduced" INTEGER NOT NULL,
    "LastProduced" TEXT NULL,
    "AverageCostPerUnit" decimal(10,2) NOT NULL,
    "StandardSellingPrice" decimal(10,2) NOT NULL,
    "ProcessParameters" TEXT NOT NULL DEFAULT '{}',
    "QualityCheckpoints" TEXT NOT NULL DEFAULT '{}',
    "BuildFileTemplate" TEXT NOT NULL DEFAULT '',
    "CadFilePath" TEXT NOT NULL DEFAULT '',
    "CadFileVersion" TEXT NOT NULL DEFAULT '',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "AvgDuration" TEXT NOT NULL DEFAULT '8h 0m',
    "AvgDurationDays" INTEGER NOT NULL,
    "EstimatedHours" REAL NOT NULL,
    "AdminEstimatedHoursOverride" REAL NULL,
    "AdminOverrideReason" TEXT NULL DEFAULT '',
    "AdminOverrideBy" TEXT NOT NULL DEFAULT '',
    "AdminOverrideDate" TEXT NULL,
    "PartClassificationId" INTEGER NULL,
    "ComponentTypeId" INTEGER NULL,
    "ComplianceCategoryId" INTEGER NULL,
    "IsLegacyForm" INTEGER NOT NULL,
    "AppliedTemplateId" INTEGER NULL,
    "AllowStacking" INTEGER NOT NULL,
    "SingleStackDurationHours" REAL NULL,
    "DoubleStackDurationHours" REAL NULL,
    "TripleStackDurationHours" REAL NULL,
    CONSTRAINT "FK_Parts_ComplianceCategories_ComplianceCategoryId" FOREIGN KEY ("ComplianceCategoryId") REFERENCES "ComplianceCategories" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Parts_ComponentTypes_ComponentTypeId" FOREIGN KEY ("ComponentTypeId") REFERENCES "ComponentTypes" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Parts_PartClassifications_PartClassificationId" FOREIGN KEY ("PartClassificationId") REFERENCES "PartClassifications" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Parts_StageTemplates_AppliedTemplateId" FOREIGN KEY ("AppliedTemplateId") REFERENCES "StageTemplates" ("Id") ON DELETE SET NULL
);

CREATE TABLE "StageTemplateSteps" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StageTemplateSteps" PRIMARY KEY AUTOINCREMENT,
    "StageTemplateId" INTEGER NOT NULL,
    "ProductionStageId" INTEGER NOT NULL,
    "ExecutionOrder" INTEGER NOT NULL DEFAULT 1,
    "EstimatedHours" REAL NOT NULL DEFAULT 1.0,
    "HourlyRate" TEXT NOT NULL DEFAULT '85.0',
    "MaterialCost" TEXT NOT NULL DEFAULT '0.0',
    "SetupTimeMinutes" INTEGER NOT NULL DEFAULT 30,
    "TeardownTimeMinutes" INTEGER NOT NULL DEFAULT 0,
    "IsRequired" INTEGER NOT NULL DEFAULT 1,
    "IsParallel" INTEGER NOT NULL DEFAULT 0,
    "StageConfiguration" TEXT NOT NULL DEFAULT '{}',
    "QualityRequirements" TEXT NOT NULL DEFAULT '',
    "SpecialInstructions" TEXT NOT NULL DEFAULT '',
    CONSTRAINT "FK_StageTemplateSteps_ProductionStages_ProductionStageId" FOREIGN KEY ("ProductionStageId") REFERENCES "ProductionStages" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_StageTemplateSteps_StageTemplates_StageTemplateId" FOREIGN KEY ("StageTemplateId") REFERENCES "StageTemplates" ("Id") ON DELETE CASCADE
);

CREATE TABLE "PartBatches" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartBatches" PRIMARY KEY AUTOINCREMENT,
    "ProductionBuildId" INTEGER NOT NULL,
    "BatchNumber" TEXT NOT NULL,
    "CurrentStage" TEXT NOT NULL DEFAULT 'SLS',
    "Quantity" INTEGER NOT NULL,
    "QualityStatus" TEXT NOT NULL DEFAULT 'Good',
    "Location" TEXT NULL,
    "Notes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_PartBatches_ProductionBuilds_ProductionBuildId" FOREIGN KEY ("ProductionBuildId") REFERENCES "ProductionBuilds" ("Id") ON DELETE CASCADE
);

CREATE TABLE "StageExecutions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StageExecutions" PRIMARY KEY AUTOINCREMENT,
    "ProductionBuildId" INTEGER NOT NULL,
    "StageDefinitionId" INTEGER NOT NULL,
    "StageName" TEXT NOT NULL,
    "ExecutionOrder" INTEGER NOT NULL,
    "QuantityIn" INTEGER NOT NULL,
    "QuantityOut" INTEGER NOT NULL,
    "DefectCount" INTEGER NOT NULL,
    "ReworkCount" INTEGER NOT NULL,
    "StartTime" TEXT NULL,
    "EndTime" TEXT NULL,
    "ActualHours" REAL NULL,
    "Status" TEXT NOT NULL DEFAULT 'NotStarted',
    "MachineUsed" TEXT NULL,
    "OperatorUserId" INTEGER NULL,
    "StageData" TEXT NOT NULL DEFAULT '{}',
    "OperatorNotes" TEXT NULL,
    "QualityNotes" TEXT NULL,
    "PassedQuality" INTEGER NOT NULL,
    "ActualCost" TEXT NULL,
    "MaterialCost" TEXT NULL,
    "LaborCost" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CompletedDate" TEXT NULL,
    CONSTRAINT "FK_StageExecutions_ProductionBuilds_ProductionBuildId" FOREIGN KEY ("ProductionBuildId") REFERENCES "ProductionBuilds" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_StageExecutions_StageDefinitions_StageDefinitionId" FOREIGN KEY ("StageDefinitionId") REFERENCES "StageDefinitions" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_StageExecutions_Users_OperatorUserId" FOREIGN KEY ("OperatorUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
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

CREATE TABLE "BuildJobs" (
    "BuildId" INTEGER NOT NULL CONSTRAINT "PK_BuildJobs" PRIMARY KEY AUTOINCREMENT,
    "PrinterName" TEXT NOT NULL,
    "ActualStartTime" TEXT NOT NULL,
    "ActualEndTime" TEXT NULL,
    "ScheduledStartTime" TEXT NULL,
    "ScheduledEndTime" TEXT NULL,
    "Status" TEXT NOT NULL,
    "UserId" INTEGER NOT NULL,
    "Notes" TEXT NULL,
    "Material" TEXT NULL,
    "LaserRunTime" TEXT NULL,
    "GasUsed_L" REAL NULL,
    "PowderUsed_L" REAL NULL,
    "ReasonForEnd" TEXT NULL,
    "SetupNotes" TEXT NULL,
    "AssociatedScheduledJobId" INTEGER NULL,
    "PartId" INTEGER NULL,
    "CreatedAt" TEXT NOT NULL,
    "CompletedAt" TEXT NULL,
    "OperatorEstimatedHours" TEXT NULL,
    "OperatorActualHours" TEXT NULL,
    "TotalPartsInBuild" INTEGER NOT NULL,
    "BuildFileHash" TEXT NULL,
    "IsLearningBuild" INTEGER NOT NULL,
    "OperatorBuildAssessment" TEXT NULL,
    "TimeFactors" TEXT NULL,
    "MachinePerformanceNotes" TEXT NULL,
    "PowerConsumption" TEXT NULL,
    "LaserOnTime" TEXT NULL,
    "LayerCount" INTEGER NULL,
    "BuildHeight" TEXT NULL,
    "SupportComplexity" TEXT NULL,
    "PartOrientations" TEXT NULL,
    "PostProcessingNeeded" TEXT NULL,
    "DefectCount" INTEGER NULL,
    "LessonsLearned" TEXT NULL,
    CONSTRAINT "FK_BuildJobs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id"),
    CONSTRAINT "FK_BuildJobs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "EDMLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EDMLogs" PRIMARY KEY AUTOINCREMENT,
    "LogNumber" TEXT NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "LogDate" TEXT NOT NULL,
    "Shift" TEXT NOT NULL,
    "OperatorName" TEXT NOT NULL,
    "OperatorInitials" TEXT NOT NULL,
    "StartTime" TEXT NOT NULL,
    "EndTime" TEXT NOT NULL,
    "Measurement1" TEXT NOT NULL,
    "Measurement2" TEXT NOT NULL,
    "ToleranceStatus" TEXT NOT NULL,
    "ScrapIssues" TEXT NOT NULL,
    "Notes" TEXT NOT NULL,
    "TotalTime" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "MachineUsed" TEXT NOT NULL,
    "ProcessType" TEXT NOT NULL,
    "QualityNotes" TEXT NOT NULL,
    "IsCompleted" INTEGER NOT NULL,
    "RequiresReview" INTEGER NOT NULL,
    "ReviewedBy" TEXT NOT NULL,
    "ReviewedDate" TEXT NULL,
    "ReviewNotes" TEXT NOT NULL,
    "ProcessParameters" TEXT NOT NULL,
    "Measurements" TEXT NOT NULL,
    "PartId" INTEGER NULL,
    CONSTRAINT "FK_EDMLogs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id")
);

CREATE TABLE "InspectionCheckpoints" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_InspectionCheckpoints" PRIMARY KEY AUTOINCREMENT,
    "PartId" INTEGER NOT NULL,
    "DefectCategoryId" INTEGER NULL,
    "CheckpointName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "InspectionType" TEXT NOT NULL DEFAULT 'Visual',
    "SortOrder" INTEGER NOT NULL,
    "IsRequired" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "EstimatedMinutes" INTEGER NOT NULL,
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
    "SampleSize" INTEGER NOT NULL,
    "SamplingMethod" TEXT NOT NULL DEFAULT 'All',
    "Category" TEXT NOT NULL DEFAULT 'Quality',
    "Priority" INTEGER NOT NULL,
    "Notes" TEXT NOT NULL DEFAULT '',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "DefectCategoryId1" INTEGER NULL,
    CONSTRAINT "FK_InspectionCheckpoints_DefectCategories_DefectCategoryId" FOREIGN KEY ("DefectCategoryId") REFERENCES "DefectCategories" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_InspectionCheckpoints_DefectCategories_DefectCategoryId1" FOREIGN KEY ("DefectCategoryId1") REFERENCES "DefectCategories" ("Id"),
    CONSTRAINT "FK_InspectionCheckpoints_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "PartAssetLinks" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartAssetLinks" PRIMARY KEY AUTOINCREMENT,
    "PartId" INTEGER NOT NULL,
    "Url" TEXT NOT NULL,
    "DisplayName" TEXT NOT NULL,
    "Source" TEXT NOT NULL DEFAULT 'Upload',
    "AssetType" TEXT NOT NULL DEFAULT '3DModel',
    "LastCheckedUtc" TEXT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    CONSTRAINT "FK_PartAssetLinks_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "PartStageRequirements" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartStageRequirements" PRIMARY KEY AUTOINCREMENT,
    "PartId" INTEGER NOT NULL,
    "ProductionStageId" INTEGER NOT NULL,
    "WorkflowTemplateId" INTEGER NULL,
    "ExecutionOrder" INTEGER NOT NULL,
    "IsRequired" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "AllowParallelExecution" INTEGER NOT NULL,
    "IsBlocking" INTEGER NOT NULL,
    "EstimatedHours" REAL NULL,
    "SetupTimeMinutes" INTEGER NULL,
    "HourlyRateOverride" decimal(8,2) NULL,
    "EstimatedCost" decimal(10,2) NOT NULL,
    "MaterialCost" decimal(10,2) NOT NULL,
    "AssignedMachineId" TEXT NULL,
    "RequiresSpecificMachine" INTEGER NOT NULL,
    "PreferredMachineIds" TEXT NULL,
    "CustomFieldValues" TEXT NOT NULL,
    "StageParameters" TEXT NOT NULL,
    "RequiredMaterials" TEXT NOT NULL,
    "RequiredTooling" TEXT NOT NULL,
    "QualityRequirements" TEXT NOT NULL,
    "SpecialInstructions" TEXT NOT NULL,
    "RequirementNotes" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "StageTemplateId" INTEGER NULL,
    CONSTRAINT "FK_PartStageRequirements_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PartStageRequirements_ProductionStages_ProductionStageId" FOREIGN KEY ("ProductionStageId") REFERENCES "ProductionStages" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PartStageRequirements_StageTemplates_StageTemplateId" FOREIGN KEY ("StageTemplateId") REFERENCES "StageTemplates" ("Id")
);

CREATE TABLE "PrototypeJobs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PrototypeJobs" PRIMARY KEY AUTOINCREMENT,
    "PartId" INTEGER NOT NULL,
    "PrototypeNumber" TEXT NOT NULL,
    "CustomerOrderNumber" TEXT NULL,
    "RequestedBy" TEXT NOT NULL,
    "RequestDate" TEXT NOT NULL,
    "Priority" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "TotalActualCost" decimal(12,2) NOT NULL,
    "TotalEstimatedCost" decimal(12,2) NOT NULL,
    "CostVariancePercent" decimal(5,2) NOT NULL,
    "TotalActualHours" decimal(8,2) NOT NULL,
    "TotalEstimatedHours" decimal(8,2) NOT NULL,
    "TimeVariancePercent" decimal(5,2) NOT NULL,
    "StartDate" TEXT NULL,
    "CompletionDate" TEXT NULL,
    "LeadTimeDays" INTEGER NULL,
    "AdminReviewStatus" TEXT NOT NULL,
    "AdminReviewBy" TEXT NULL,
    "AdminReviewDate" TEXT NULL,
    "AdminReviewNotes" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "UpdatedBy" TEXT NULL,
    "UpdatedDate" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    CONSTRAINT "FK_PrototypeJobs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "BuildCohorts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BuildCohorts" PRIMARY KEY AUTOINCREMENT,
    "BuildJobId" INTEGER NULL,
    "BuildNumber" TEXT NOT NULL,
    "PartCount" INTEGER NOT NULL,
    "Material" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "CompletedDate" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "Notes" TEXT NULL,
    CONSTRAINT "FK_BuildCohorts_BuildJobs_BuildJobId" FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId")
);

CREATE TABLE "BuildJobParts" (
    "PartEntryId" INTEGER NOT NULL CONSTRAINT "PK_BuildJobParts" PRIMARY KEY AUTOINCREMENT,
    "BuildId" INTEGER NOT NULL,
    "BuildJobBuildId" INTEGER NULL,
    "PartNumber" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "IsPrimary" INTEGER NOT NULL,
    "Description" TEXT NULL,
    "Material" TEXT NULL,
    "EstimatedHours" REAL NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    CONSTRAINT "FK_BuildJobParts_BuildJobs_BuildJobBuildId" FOREIGN KEY ("BuildJobBuildId") REFERENCES "BuildJobs" ("BuildId")
);

CREATE TABLE "BuildTimeLearningData" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BuildTimeLearningData" PRIMARY KEY AUTOINCREMENT,
    "BuildJobId" INTEGER NOT NULL,
    "MachineId" TEXT NOT NULL,
    "BuildFileHash" TEXT NULL,
    "OperatorEstimatedHours" TEXT NOT NULL,
    "ActualHours" TEXT NOT NULL,
    "VariancePercent" TEXT NOT NULL,
    "SupportComplexity" TEXT NULL,
    "TimeFactors" TEXT NULL,
    "QualityScore" TEXT NOT NULL,
    "DefectCount" INTEGER NOT NULL,
    "BuildHeight" TEXT NULL,
    "LayerCount" INTEGER NULL,
    "TotalParts" INTEGER NOT NULL,
    "PartOrientations" TEXT NULL,
    "RecordedAt" TEXT NOT NULL,
    CONSTRAINT "FK_BuildTimeLearningData_BuildJobs_BuildJobId" FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
);

CREATE TABLE "DelayLogs" (
    "DelayId" INTEGER NOT NULL CONSTRAINT "PK_DelayLogs" PRIMARY KEY AUTOINCREMENT,
    "BuildId" INTEGER NOT NULL,
    "BuildJobBuildId" INTEGER NULL,
    "DelayReason" TEXT NOT NULL,
    "DelayDuration" INTEGER NOT NULL,
    "Description" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    CONSTRAINT "FK_DelayLogs_BuildJobs_BuildJobBuildId" FOREIGN KEY ("BuildJobBuildId") REFERENCES "BuildJobs" ("BuildId")
);

CREATE TABLE "OperatorEstimateLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_OperatorEstimateLogs" PRIMARY KEY AUTOINCREMENT,
    "BuildJobId" INTEGER NOT NULL,
    "EstimatedHours" TEXT NOT NULL,
    "TimeFactors" TEXT NULL,
    "OperatorNotes" TEXT NULL,
    "LoggedAt" TEXT NOT NULL,
    CONSTRAINT "FK_OperatorEstimateLogs_BuildJobs_BuildJobId" FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
);

CREATE TABLE "PartCompletionLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartCompletionLogs" PRIMARY KEY AUTOINCREMENT,
    "BuildJobId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "GoodParts" INTEGER NOT NULL,
    "DefectiveParts" INTEGER NOT NULL,
    "ReworkParts" INTEGER NOT NULL,
    "QualityRate" TEXT NOT NULL,
    "IsPrimary" INTEGER NOT NULL,
    "InspectionNotes" TEXT NULL,
    "CompletedAt" TEXT NOT NULL,
    CONSTRAINT "FK_PartCompletionLogs_BuildJobs_BuildJobId" FOREIGN KEY ("BuildJobId") REFERENCES "BuildJobs" ("BuildId") ON DELETE CASCADE
);

CREATE TABLE "AssemblyComponents" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AssemblyComponents" PRIMARY KEY AUTOINCREMENT,
    "PrototypeJobId" INTEGER NOT NULL,
    "ComponentType" TEXT NOT NULL,
    "ComponentPartNumber" TEXT NULL,
    "ComponentDescription" TEXT NOT NULL,
    "QuantityRequired" INTEGER NOT NULL,
    "QuantityUsed" INTEGER NOT NULL,
    "UnitCost" decimal(8,2) NULL,
    "TotalCost" decimal(10,2) NULL,
    "Supplier" TEXT NULL,
    "SupplierPartNumber" TEXT NULL,
    "LeadTimeDays" INTEGER NULL,
    "Status" TEXT NOT NULL,
    "OrderDate" TEXT NULL,
    "ReceivedDate" TEXT NULL,
    "UsedDate" TEXT NULL,
    "InspectionRequired" INTEGER NOT NULL,
    "InspectionPassed" INTEGER NULL,
    "InspectionNotes" TEXT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    CONSTRAINT "FK_AssemblyComponents_PrototypeJobs_PrototypeJobId" FOREIGN KEY ("PrototypeJobId") REFERENCES "PrototypeJobs" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Jobs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Jobs" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NOT NULL,
    "ScheduledStart" TEXT NOT NULL,
    "ScheduledEnd" TEXT NOT NULL,
    "ActualStart" TEXT NULL,
    "ActualEnd" TEXT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "BuildCohortId" INTEGER NULL,
    "WorkflowStage" TEXT NULL,
    "StageOrder" INTEGER NULL,
    "TotalStages" INTEGER NULL,
    "PartId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "ProducedQuantity" INTEGER NOT NULL,
    "DefectQuantity" INTEGER NOT NULL,
    "ReworkQuantity" INTEGER NOT NULL,
    "EstimatedDuration" TEXT NOT NULL,
    "SlsMaterial" TEXT NOT NULL,
    "PowderLotNumber" TEXT NOT NULL,
    "PowderExpirationDate" TEXT NULL,
    "BuildPlatformId" TEXT NOT NULL,
    "BuildLayerNumber" INTEGER NOT NULL,
    "LaserPowerWatts" REAL NOT NULL DEFAULT 170.0,
    "ScanSpeedMmPerSec" REAL NOT NULL DEFAULT 1000.0,
    "LayerThicknessMicrons" REAL NOT NULL DEFAULT 30.0,
    "HatchSpacingMicrons" REAL NOT NULL DEFAULT 120.0,
    "ArgonPurityPercent" REAL NOT NULL DEFAULT 99.5,
    "OxygenContentPpm" REAL NOT NULL DEFAULT 50.0,
    "BuildTemperatureCelsius" REAL NOT NULL DEFAULT 180.0,
    "EstimatedPowderUsageKg" REAL NOT NULL DEFAULT 0.5,
    "ActualPowderUsageKg" REAL NOT NULL,
    "PowderRecyclePercentage" REAL NOT NULL,
    "EstimatedHours" REAL NOT NULL,
    "PreheatingTimeMinutes" REAL NOT NULL,
    "BuildTimeMinutes" REAL NOT NULL,
    "CoolingTimeMinutes" REAL NOT NULL,
    "PowderChangeoverTimeMinutes" REAL NOT NULL,
    "PostProcessingTimeMinutes" REAL NOT NULL,
    "SetupTimeMinutes" REAL NOT NULL,
    "ChangeoverTimeMinutes" REAL NOT NULL,
    "PreviousJobPartNumber" TEXT NULL,
    "LaborCostPerHour" decimal(10,2) NOT NULL,
    "MaterialCostPerKg" decimal(10,2) NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "PowerCostPerKwh" decimal(10,2) NOT NULL,
    "OpcUaJobId" TEXT NULL,
    "OpcUaStatus" TEXT NULL,
    "OpcUaLastUpdate" TEXT NULL,
    "OpcUaBuildProgress" REAL NOT NULL,
    "OpcUaErrorMessages" TEXT NULL,
    "CurrentLaserPowerWatts" REAL NOT NULL,
    "CurrentBuildTemperature" REAL NOT NULL,
    "CurrentOxygenLevel" REAL NOT NULL,
    "CurrentArgonFlowRate" REAL NOT NULL,
    "BuildFileName" TEXT NULL,
    "BuildFilePath" TEXT NULL,
    "BuildFileSizeBytes" INTEGER NOT NULL,
    "BuildFileCreatedDate" TEXT NULL,
    "RequiredSkills" TEXT NOT NULL,
    "RequiredTooling" TEXT NOT NULL,
    "RequiredMaterials" TEXT NOT NULL,
    "SpecialInstructions" TEXT NOT NULL,
    "ProcessParameters" TEXT NOT NULL,
    "QualityCheckpoints" TEXT NOT NULL,
    "MachineUtilizationPercent" REAL NOT NULL,
    "EnergyConsumptionKwh" REAL NOT NULL,
    "SurfaceRoughnessRa" REAL NOT NULL,
    "DensityPercentage" REAL NOT NULL,
    "UltimateTensileStrengthMPa" REAL NOT NULL,
    "Status" TEXT NOT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "CustomerOrderNumber" TEXT NOT NULL DEFAULT '',
    "CustomerDueDate" TEXT NULL,
    "IsRushJob" INTEGER NOT NULL,
    "HoldReason" TEXT NOT NULL,
    "RequiresArgonPurge" INTEGER NOT NULL,
    "RequiresPreheating" INTEGER NOT NULL,
    "RequiresPostProcessing" INTEGER NOT NULL,
    "RequiresPowderSieving" INTEGER NOT NULL,
    "Operator" TEXT NULL,
    "QualityInspector" TEXT NULL,
    "Supervisor" TEXT NULL,
    "Notes" TEXT NULL,
    "MasterPartId" INTEGER NULL,
    "StackLevel" INTEGER NULL,
    "PartsPerBuild" INTEGER NULL,
    "PlannedStackDurationHours" REAL NULL,
    "PlannedEndUtc" TEXT NULL,
    "ActualUnitsPlanned" INTEGER NULL,
    "PrototypeUnitsPlanned" INTEGER NULL,
    "PowderAddedKg" decimal(8,2) NULL,
    "PowderMaterial" TEXT NULL,
    "PredecessorJobId" INTEGER NULL,
    "UpstreamGapHours" REAL NULL,
    "OperatorUserId" INTEGER NULL,
    "LastStatusChangeUtc" TEXT NULL,
    CONSTRAINT "FK_Jobs_BuildCohorts_BuildCohortId" FOREIGN KEY ("BuildCohortId") REFERENCES "BuildCohorts" ("Id"),
    CONSTRAINT "FK_Jobs_Jobs_PredecessorJobId" FOREIGN KEY ("PredecessorJobId") REFERENCES "Jobs" ("Id"),
    CONSTRAINT "FK_Jobs_MasterParts_MasterPartId" FOREIGN KEY ("MasterPartId") REFERENCES "MasterParts" ("Id"),
    CONSTRAINT "FK_Jobs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Jobs_Users_OperatorUserId" FOREIGN KEY ("OperatorUserId") REFERENCES "Users" ("Id")
);

CREATE TABLE "JobNotes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_JobNotes" PRIMARY KEY AUTOINCREMENT,
    "JobId" INTEGER NOT NULL,
    "PartId" INTEGER NULL,
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
    CONSTRAINT "FK_JobNotes_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES "Jobs" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_JobNotes_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE SET NULL
);

CREATE TABLE "JobStageHistories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_JobStageHistories" PRIMARY KEY AUTOINCREMENT,
    "JobId" INTEGER NOT NULL,
    "ProductionStageId" INTEGER NULL,
    "Action" TEXT NOT NULL,
    "StageName" TEXT NOT NULL,
    "Operator" TEXT NOT NULL,
    "Timestamp" TEXT NOT NULL,
    "Notes" TEXT NULL,
    "MachineId" TEXT NULL,
    "StageHours" REAL NULL,
    "QualityResult" TEXT NULL,
    CONSTRAINT "FK_JobStageHistories_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES "Jobs" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_JobStageHistories_ProductionStages_ProductionStageId" FOREIGN KEY ("ProductionStageId") REFERENCES "ProductionStages" ("Id")
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
    "ColorHex" TEXT NULL,
    CONSTRAINT "FK_Machines_Jobs_CurrentJobId" FOREIGN KEY ("CurrentJobId") REFERENCES "Jobs" ("Id")
);

CREATE TABLE "ProductionStageExecutions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductionStageExecutions" PRIMARY KEY AUTOINCREMENT,
    "JobId" INTEGER NULL,
    "PrototypeJobId" INTEGER NULL,
    "ProductionStageId" INTEGER NOT NULL,
    "WorkflowTemplateId" INTEGER NULL,
    "Status" TEXT NOT NULL,
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
    "QualityCheckRequired" INTEGER NOT NULL,
    "QualityCheckPassed" INTEGER NULL,
    "QualityCheckBy" TEXT NULL,
    "QualityCheckDate" TEXT NULL,
    "QualityNotes" TEXT NULL,
    "ProcessParameters" TEXT NOT NULL,
    "Issues" TEXT NULL,
    "Improvements" TEXT NULL,
    "ExecutedBy" TEXT NOT NULL,
    "ReviewedBy" TEXT NULL,
    "ApprovedBy" TEXT NULL,
    "OperatorName" TEXT NULL,
    "ActualStartTime" TEXT NULL,
    "ActualEndTime" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    "LastModifiedDate" TEXT NULL,
    "CreatedDate" TEXT NOT NULL,
    "UpdatedDate" TEXT NULL,
    CONSTRAINT "FK_ProductionStageExecutions_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES "Jobs" ("Id"),
    CONSTRAINT "FK_ProductionStageExecutions_ProductionStages_ProductionStageId" FOREIGN KEY ("ProductionStageId") REFERENCES "ProductionStages" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ProductionStageExecutions_PrototypeJobs_PrototypeJobId" FOREIGN KEY ("PrototypeJobId") REFERENCES "PrototypeJobs" ("Id")
);

CREATE TABLE "SerialNumbers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SerialNumbers" PRIMARY KEY AUTOINCREMENT,
    "SerialNumberValue" TEXT NOT NULL,
    "SerialNumberFormat" TEXT NOT NULL,
    "ManufacturerCode" TEXT NOT NULL,
    "AssignedDate" TEXT NOT NULL,
    "ManufacturedDate" TEXT NULL,
    "CompletedDate" TEXT NULL,
    "AssignedJobId" TEXT NULL,
    "PartNumber" TEXT NULL,
    "ComponentName" TEXT NOT NULL,
    "ComponentType" TEXT NOT NULL,
    "ManufacturingMethod" TEXT NOT NULL,
    "MaterialUsed" TEXT NOT NULL,
    "MaterialLotNumber" TEXT NOT NULL,
    "MachineUsed" TEXT NOT NULL,
    "Operator" TEXT NOT NULL,
    "QualityInspector" TEXT NOT NULL,
    "ATFComplianceStatus" TEXT NOT NULL,
    "ATFClassification" TEXT NOT NULL,
    "FFLDealer" TEXT NULL,
    "FFLNumber" TEXT NULL,
    "ATFFormSubmissionDate" TEXT NULL,
    "ATFApprovalDate" TEXT NULL,
    "ATFFormNumbers" TEXT NOT NULL,
    "TaxStampNumber" TEXT NULL,
    "TransferStatus" TEXT NOT NULL,
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
    "ExportClassification" TEXT NOT NULL,
    "ExportLicense" TEXT NULL,
    "ExportLicenseExpiration" TEXT NULL,
    "DestinationCountry" TEXT NULL,
    "EndUser" TEXT NULL,
    "RequiresExportPermit" INTEGER NOT NULL,
    "ExportPermitObtained" INTEGER NOT NULL,
    "QualityStatus" TEXT NOT NULL,
    "QualityInspectionDate" TEXT NULL,
    "QualityCertificateNumber" TEXT NULL,
    "TestResultsSummary" TEXT NOT NULL,
    "DimensionalTestPassed" INTEGER NOT NULL,
    "MaterialTestPassed" INTEGER NOT NULL,
    "PressureTestPassed" INTEGER NOT NULL,
    "ProofTestPassed" INTEGER NOT NULL,
    "QualityNotes" TEXT NOT NULL,
    "ManufacturingHistory" TEXT NOT NULL,
    "ComponentGenealogy" TEXT NOT NULL,
    "AssemblyComponents" TEXT NOT NULL,
    "BatchNumber" TEXT NULL,
    "BuildPlatformId" TEXT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "IsLocked" INTEGER NOT NULL,
    "PartId" INTEGER NULL,
    "JobId" INTEGER NULL,
    "ComplianceRequirementId" INTEGER NULL,
    CONSTRAINT "FK_SerialNumbers_ComplianceRequirements_ComplianceRequirementId" FOREIGN KEY ("ComplianceRequirementId") REFERENCES "ComplianceRequirements" ("Id"),
    CONSTRAINT "FK_SerialNumbers_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES "Jobs" ("Id"),
    CONSTRAINT "FK_SerialNumbers_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id")
);

CREATE TABLE "JobStages" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_JobStages" PRIMARY KEY AUTOINCREMENT,
    "JobId" INTEGER NOT NULL,
    "StageType" TEXT NOT NULL,
    "StageName" TEXT NOT NULL,
    "ExecutionOrder" INTEGER NOT NULL,
    "Department" TEXT NOT NULL,
    "MachineId" TEXT NULL,
    "MachineId1" INTEGER NULL,
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
    CONSTRAINT "FK_JobStages_Machines_MachineId1" FOREIGN KEY ("MachineId1") REFERENCES "Machines" ("Id")
);

CREATE TABLE "MachineCapabilities" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MachineCapabilities" PRIMARY KEY AUTOINCREMENT,
    "MachineId" INTEGER NOT NULL,
    "CapabilityType" TEXT NOT NULL,
    "CapabilityName" TEXT NOT NULL,
    "CapabilityValue" TEXT NOT NULL,
    "IsAvailable" INTEGER NOT NULL DEFAULT 1,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "MinValue" REAL NULL,
    "MaxValue" REAL NULL,
    "Unit" TEXT NOT NULL DEFAULT '',
    "Notes" TEXT NOT NULL DEFAULT '',
    "RequiredCertification" TEXT NOT NULL DEFAULT '',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    CONSTRAINT "FK_MachineCapabilities_Machines_MachineId" FOREIGN KEY ("MachineId") REFERENCES "Machines" ("Id") ON DELETE CASCADE
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
    "CreatedDate" TEXT NOT NULL,
    CONSTRAINT "FK_PrototypeTimeLogs_ProductionStageExecutions_ProductionStageExecutionId" FOREIGN KEY ("ProductionStageExecutionId") REFERENCES "ProductionStageExecutions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ComplianceDocuments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ComplianceDocuments" PRIMARY KEY AUTOINCREMENT,
    "DocumentNumber" TEXT NOT NULL,
    "DocumentTitle" TEXT NOT NULL,
    "DocumentType" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "ComplianceCategory" TEXT NOT NULL,
    "DocumentClassification" TEXT NOT NULL,
    "RegulatoryAuthority" TEXT NOT NULL,
    "FormNumber" TEXT NULL,
    "DocumentDate" TEXT NOT NULL,
    "EffectiveDate" TEXT NULL,
    "ExpirationDate" TEXT NULL,
    "SubmissionDate" TEXT NULL,
    "ApprovalDate" TEXT NULL,
    "Status" TEXT NOT NULL,
    "ApprovalNumber" TEXT NULL,
    "ReferenceNumber" TEXT NULL,
    "FilePath" TEXT NULL,
    "FileName" TEXT NULL,
    "FileType" TEXT NULL,
    "FileSizeMB" decimal(8,2) NOT NULL,
    "FileHash" TEXT NULL,
    "DocumentContent" TEXT NOT NULL,
    "AssociatedSerialNumbers" TEXT NOT NULL,
    "AssociatedPartNumbers" TEXT NOT NULL,
    "AssociatedJobNumbers" TEXT NOT NULL,
    "Customer" TEXT NULL,
    "Vendor" TEXT NULL,
    "PreparedBy" TEXT NOT NULL,
    "ReviewedBy" TEXT NULL,
    "ApprovedBy" TEXT NULL,
    "ReviewDate" TEXT NULL,
    "ApprovalDateInternal" TEXT NULL,
    "ReviewComments" TEXT NOT NULL,
    "ApprovalComments" TEXT NOT NULL,
    "RetentionPeriod" TEXT NOT NULL,
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
    "NotificationRecipients" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "LastAccessedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "LastAccessedBy" TEXT NOT NULL,
    "AccessCount" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "AuditNotes" TEXT NOT NULL,
    "SerialNumberId" INTEGER NULL,
    "ComplianceRequirementId" INTEGER NULL,
    "PartId" INTEGER NULL,
    "JobId" INTEGER NULL,
    CONSTRAINT "FK_ComplianceDocuments_ComplianceRequirements_ComplianceRequirementId" FOREIGN KEY ("ComplianceRequirementId") REFERENCES "ComplianceRequirements" ("Id"),
    CONSTRAINT "FK_ComplianceDocuments_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES "Jobs" ("Id"),
    CONSTRAINT "FK_ComplianceDocuments_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id"),
    CONSTRAINT "FK_ComplianceDocuments_SerialNumbers_SerialNumberId" FOREIGN KEY ("SerialNumberId") REFERENCES "SerialNumbers" ("Id")
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
    CONSTRAINT "CK_JobStageDependency_NoSelfReference" CHECK (DependentStageId != RequiredStageId),
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

CREATE INDEX "IX_AssemblyComponents_PrototypeJobId" ON "AssemblyComponents" ("PrototypeJobId");

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

CREATE INDEX "IX_BuildCohorts_BuildJobId" ON "BuildCohorts" ("BuildJobId");

CREATE INDEX "IX_BuildJobParts_BuildJobBuildId" ON "BuildJobParts" ("BuildJobBuildId");

CREATE INDEX "IX_BuildJobs_PartId" ON "BuildJobs" ("PartId");

CREATE INDEX "IX_BuildJobs_UserId" ON "BuildJobs" ("UserId");

CREATE INDEX "IX_BuildTimeLearningData_BuildJobId" ON "BuildTimeLearningData" ("BuildJobId");

CREATE INDEX "IX_ComplianceCategories_IsActive" ON "ComplianceCategories" ("IsActive");

CREATE UNIQUE INDEX "IX_ComplianceCategories_Name" ON "ComplianceCategories" ("Name");

CREATE INDEX "IX_ComplianceCategories_RegulatoryLevel" ON "ComplianceCategories" ("RegulatoryLevel");

CREATE INDEX "IX_ComplianceCategories_RegulatoryLevel_SortOrder" ON "ComplianceCategories" ("RegulatoryLevel", "SortOrder");

CREATE INDEX "IX_ComplianceDocuments_ComplianceRequirementId" ON "ComplianceDocuments" ("ComplianceRequirementId");

CREATE INDEX "IX_ComplianceDocuments_JobId" ON "ComplianceDocuments" ("JobId");

CREATE INDEX "IX_ComplianceDocuments_PartId" ON "ComplianceDocuments" ("PartId");

CREATE INDEX "IX_ComplianceDocuments_SerialNumberId" ON "ComplianceDocuments" ("SerialNumberId");

CREATE INDEX "IX_ComplianceRequirements_PartClassificationId" ON "ComplianceRequirements" ("PartClassificationId");

CREATE INDEX "IX_ComponentTypes_IsActive" ON "ComponentTypes" ("IsActive");

CREATE UNIQUE INDEX "IX_ComponentTypes_Name" ON "ComponentTypes" ("Name");

CREATE INDEX "IX_ComponentTypes_SortOrder" ON "ComponentTypes" ("SortOrder");

CREATE INDEX "IX_CrmAccounts_Name" ON "CrmAccounts" ("Name");

CREATE INDEX "IX_CrmContacts_AccountId" ON "CrmContacts" ("AccountId");

CREATE INDEX "IX_CrmContacts_Name" ON "CrmContacts" ("Name");

CREATE INDEX "IX_CrmTasks_AccountId" ON "CrmTasks" ("AccountId");

CREATE INDEX "IX_CrmTasks_AssignedToUserId" ON "CrmTasks" ("AssignedToUserId");

CREATE INDEX "IX_CrmTasks_ContactId" ON "CrmTasks" ("ContactId");

CREATE INDEX "IX_CrmTasks_DueAt" ON "CrmTasks" ("DueAt");

CREATE INDEX "IX_CrmTasks_Status" ON "CrmTasks" ("Status");

CREATE INDEX "IX_DefectCategories_CategoryGroup" ON "DefectCategories" ("CategoryGroup");

CREATE INDEX "IX_DefectCategories_Code" ON "DefectCategories" ("Code");

CREATE INDEX "IX_DefectCategories_IsActive" ON "DefectCategories" ("IsActive");

CREATE INDEX "IX_DefectCategories_Name" ON "DefectCategories" ("Name");

CREATE INDEX "IX_DefectCategories_SeverityLevel" ON "DefectCategories" ("SeverityLevel");

CREATE INDEX "IX_DelayLogs_BuildJobBuildId" ON "DelayLogs" ("BuildJobBuildId");

CREATE INDEX "IX_EDMLogs_PartId" ON "EDMLogs" ("PartId");

CREATE INDEX "IX_InspectionCheckpoints_DefectCategoryId" ON "InspectionCheckpoints" ("DefectCategoryId");

CREATE INDEX "IX_InspectionCheckpoints_DefectCategoryId1" ON "InspectionCheckpoints" ("DefectCategoryId1");

CREATE INDEX "IX_InspectionCheckpoints_InspectionType" ON "InspectionCheckpoints" ("InspectionType");

CREATE INDEX "IX_InspectionCheckpoints_IsActive" ON "InspectionCheckpoints" ("IsActive");

CREATE INDEX "IX_InspectionCheckpoints_IsRequired" ON "InspectionCheckpoints" ("IsRequired");

CREATE INDEX "IX_InspectionCheckpoints_PartId" ON "InspectionCheckpoints" ("PartId");

CREATE INDEX "IX_InspectionCheckpoints_PartId_SortOrder" ON "InspectionCheckpoints" ("PartId", "SortOrder");

CREATE INDEX "IX_JobNotes_IsCompleted" ON "JobNotes" ("IsCompleted");

CREATE INDEX "IX_JobNotes_JobId" ON "JobNotes" ("JobId");

CREATE INDEX "IX_JobNotes_JobId_Step" ON "JobNotes" ("JobId", "Step");

CREATE INDEX "IX_JobNotes_PartId" ON "JobNotes" ("PartId");

CREATE INDEX "IX_JobNotes_Priority" ON "JobNotes" ("Priority");

CREATE INDEX "IX_Jobs_BuildCohortId" ON "Jobs" ("BuildCohortId");

CREATE INDEX "IX_Jobs_MachineId_ScheduledStart" ON "Jobs" ("MachineId", "ScheduledStart");

CREATE INDEX "IX_Jobs_MasterPartId" ON "Jobs" ("MasterPartId");

CREATE INDEX "IX_Jobs_OperatorUserId" ON "Jobs" ("OperatorUserId");

CREATE INDEX "IX_Jobs_PartId" ON "Jobs" ("PartId");

CREATE INDEX "IX_Jobs_PartNumber" ON "Jobs" ("PartNumber");

CREATE INDEX "IX_Jobs_PredecessorJobId" ON "Jobs" ("PredecessorJobId");

CREATE INDEX "IX_Jobs_Priority" ON "Jobs" ("Priority");

CREATE INDEX "IX_Jobs_Status" ON "Jobs" ("Status");

CREATE INDEX "IX_JobStageHistories_JobId" ON "JobStageHistories" ("JobId");

CREATE INDEX "IX_JobStageHistories_ProductionStageId" ON "JobStageHistories" ("ProductionStageId");

CREATE INDEX "IX_JobStages_Department" ON "JobStages" ("Department");

CREATE INDEX "IX_JobStages_JobId" ON "JobStages" ("JobId");

CREATE INDEX "IX_JobStages_JobId_ExecutionOrder" ON "JobStages" ("JobId", "ExecutionOrder");

CREATE INDEX "IX_JobStages_MachineId" ON "JobStages" ("MachineId");

CREATE INDEX "IX_JobStages_MachineId1" ON "JobStages" ("MachineId1");

CREATE INDEX "IX_JobStages_ScheduledEnd" ON "JobStages" ("ScheduledEnd");

CREATE INDEX "IX_JobStages_ScheduledStart" ON "JobStages" ("ScheduledStart");

CREATE INDEX "IX_JobStages_StageType" ON "JobStages" ("StageType");

CREATE INDEX "IX_JobStages_Status" ON "JobStages" ("Status");

CREATE INDEX "IX_LegacyFlagToStageMaps_ExecutionOrder" ON "LegacyFlagToStageMaps" ("ExecutionOrder");

CREATE INDEX "IX_LegacyFlagToStageMaps_IsActive" ON "LegacyFlagToStageMaps" ("IsActive");

CREATE UNIQUE INDEX "IX_LegacyFlagToStageMaps_LegacyFieldName" ON "LegacyFlagToStageMaps" ("LegacyFieldName");

CREATE INDEX "IX_LegacyFlagToStageMaps_ProductionStageName" ON "LegacyFlagToStageMaps" ("ProductionStageName");

CREATE INDEX "IX_MachineCapabilities_CapabilityType" ON "MachineCapabilities" ("CapabilityType");

CREATE INDEX "IX_MachineCapabilities_IsAvailable" ON "MachineCapabilities" ("IsAvailable");

CREATE INDEX "IX_MachineCapabilities_MachineId" ON "MachineCapabilities" ("MachineId");

CREATE INDEX "IX_MachineCapabilities_MachineId_CapabilityType" ON "MachineCapabilities" ("MachineId", "CapabilityType");

CREATE INDEX "IX_MachineComponents_MachineId" ON "MachineComponents" ("MachineId");

CREATE INDEX "IX_MachineComponents_MachineId_Category" ON "MachineComponents" ("MachineId", "Category");

CREATE INDEX "IX_MachineComponents_MachineId_IsActive" ON "MachineComponents" ("MachineId", "IsActive");

CREATE INDEX "IX_MachineOperatorAssignments_IsActive" ON "MachineOperatorAssignments" ("IsActive");

CREATE INDEX "IX_MachineOperatorAssignments_MachineId" ON "MachineOperatorAssignments" ("MachineId");

CREATE INDEX "IX_MachineOperatorAssignments_MachineId_IsPrimary" ON "MachineOperatorAssignments" ("MachineId", "IsPrimary");

CREATE INDEX "IX_MachineOperatorAssignments_UserId" ON "MachineOperatorAssignments" ("UserId");

CREATE INDEX "IX_MachineOperatorAssignments_UserId_IsActive" ON "MachineOperatorAssignments" ("UserId", "IsActive");

CREATE INDEX "IX_Machines_CurrentJobId" ON "Machines" ("CurrentJobId");

CREATE INDEX "IX_Machines_IsActive" ON "Machines" ("IsActive");

CREATE UNIQUE INDEX "IX_Machines_MachineId" ON "Machines" ("MachineId");

CREATE INDEX "IX_Machines_MachineType" ON "Machines" ("MachineType");

CREATE INDEX "IX_Machines_Status" ON "Machines" ("Status");

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

CREATE INDEX "IX_MasterParts_IsActive" ON "MasterParts" ("IsActive");

CREATE INDEX "IX_MasterParts_ManufacturingApproach" ON "MasterParts" ("ManufacturingApproach");

CREATE UNIQUE INDEX "IX_MasterParts_PartNumber" ON "MasterParts" ("PartNumber");

CREATE INDEX "IX_OperatingShifts_DayOfWeek" ON "OperatingShifts" ("DayOfWeek");

CREATE INDEX "IX_OperatingShifts_DayOfWeek_IsActive" ON "OperatingShifts" ("DayOfWeek", "IsActive");

CREATE INDEX "IX_OperatingShifts_IsActive" ON "OperatingShifts" ("IsActive");

CREATE INDEX "IX_OperatingShifts_IsHoliday" ON "OperatingShifts" ("IsHoliday");

CREATE INDEX "IX_OperatingShifts_MachineId_DayOfWeek" ON "OperatingShifts" ("MachineId", "DayOfWeek");

CREATE INDEX "IX_OperatingShifts_SpecificDate" ON "OperatingShifts" ("SpecificDate");

CREATE INDEX "IX_OperationalTasks_AssetId" ON "OperationalTasks" ("AssetId");

CREATE INDEX "IX_OperatorEstimateLogs_BuildJobId" ON "OperatorEstimateLogs" ("BuildJobId");

CREATE INDEX "IX_PartAssetLinks_AssetType" ON "PartAssetLinks" ("AssetType");

CREATE INDEX "IX_PartAssetLinks_IsActive" ON "PartAssetLinks" ("IsActive");

CREATE INDEX "IX_PartAssetLinks_PartId" ON "PartAssetLinks" ("PartId");

CREATE INDEX "IX_PartBatches_BatchNumber" ON "PartBatches" ("BatchNumber");

CREATE INDEX "IX_PartBatches_CreatedDate" ON "PartBatches" ("CreatedDate");

CREATE INDEX "IX_PartBatches_CurrentStage" ON "PartBatches" ("CurrentStage");

CREATE INDEX "IX_PartBatches_ProductionBuildId" ON "PartBatches" ("ProductionBuildId");

CREATE INDEX "IX_PartBatches_QualityStatus" ON "PartBatches" ("QualityStatus");

CREATE INDEX "IX_PartCompletionLogs_BuildJobId" ON "PartCompletionLogs" ("BuildJobId");

CREATE INDEX "IX_Parts_AppliedTemplateId" ON "Parts" ("AppliedTemplateId");

CREATE INDEX "IX_Parts_BTComponentType" ON "Parts" ("BTComponentType");

CREATE INDEX "IX_Parts_ComplianceCategoryId" ON "Parts" ("ComplianceCategoryId");

CREATE INDEX "IX_Parts_ComponentTypeId" ON "Parts" ("ComponentTypeId");

CREATE INDEX "IX_Parts_CreatedDate" ON "Parts" ("CreatedDate");

CREATE INDEX "IX_Parts_CustomerPartNumber" ON "Parts" ("CustomerPartNumber");

CREATE INDEX "IX_Parts_FirearmType" ON "Parts" ("FirearmType");

CREATE INDEX "IX_Parts_Industry" ON "Parts" ("Industry");

CREATE INDEX "IX_Parts_IsActive" ON "Parts" ("IsActive");

CREATE INDEX "IX_Parts_IsLegacyForm" ON "Parts" ("IsLegacyForm");

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

CREATE INDEX "IX_PartStageRequirements_PartId" ON "PartStageRequirements" ("PartId");

CREATE INDEX "IX_PartStageRequirements_ProductionStageId" ON "PartStageRequirements" ("ProductionStageId");

CREATE INDEX "IX_PartStageRequirements_StageTemplateId" ON "PartStageRequirements" ("StageTemplateId");

CREATE UNIQUE INDEX "IX_ProductionBuilds_BuildNumber" ON "ProductionBuilds" ("BuildNumber");

CREATE INDEX "IX_ProductionBuilds_CreatedByUserId" ON "ProductionBuilds" ("CreatedByUserId");

CREATE INDEX "IX_ProductionBuilds_CreatedDate" ON "ProductionBuilds" ("CreatedDate");

CREATE INDEX "IX_ProductionBuilds_MasterPartId" ON "ProductionBuilds" ("MasterPartId");

CREATE INDEX "IX_ProductionBuilds_PrinterName" ON "ProductionBuilds" ("PrinterName");

CREATE INDEX "IX_ProductionBuilds_Status" ON "ProductionBuilds" ("Status");

CREATE INDEX "IX_ProductionStageDependencies_DependentStageId" ON "ProductionStageDependencies" ("DependentStageId");

CREATE INDEX "IX_ProductionStageDependencies_PrerequisiteStageId" ON "ProductionStageDependencies" ("PrerequisiteStageId");

CREATE INDEX "IX_ProductionStageExecutions_JobId" ON "ProductionStageExecutions" ("JobId");

CREATE INDEX "IX_ProductionStageExecutions_ProductionStageId" ON "ProductionStageExecutions" ("ProductionStageId");

CREATE INDEX "IX_ProductionStageExecutions_PrototypeJobId" ON "ProductionStageExecutions" ("PrototypeJobId");

CREATE INDEX "IX_PrototypeJobs_PartId" ON "PrototypeJobs" ("PartId");

CREATE INDEX "IX_PrototypeTimeLogs_ProductionStageExecutionId" ON "PrototypeTimeLogs" ("ProductionStageExecutionId");

CREATE INDEX "IX_RolePermissions_Category" ON "RolePermissions" ("Category");

CREATE INDEX "IX_RolePermissions_IsActive" ON "RolePermissions" ("IsActive");

CREATE INDEX "IX_RolePermissions_PermissionKey" ON "RolePermissions" ("PermissionKey");

CREATE INDEX "IX_RolePermissions_RoleName" ON "RolePermissions" ("RoleName");

CREATE UNIQUE INDEX "IX_RolePermissions_RoleName_PermissionKey" ON "RolePermissions" ("RoleName", "PermissionKey");

CREATE INDEX "IX_SerialNumbers_ComplianceRequirementId" ON "SerialNumbers" ("ComplianceRequirementId");

CREATE INDEX "IX_SerialNumbers_JobId" ON "SerialNumbers" ("JobId");

CREATE INDEX "IX_SerialNumbers_PartId" ON "SerialNumbers" ("PartId");

CREATE INDEX "IX_StageDefinitions_ExecutionOrder" ON "StageDefinitions" ("ExecutionOrder");

CREATE INDEX "IX_StageDefinitions_IsActive" ON "StageDefinitions" ("IsActive");

CREATE INDEX "IX_StageDefinitions_MasterPartId" ON "StageDefinitions" ("MasterPartId");

CREATE INDEX "IX_StageDefinitions_MasterPartId_ExecutionOrder" ON "StageDefinitions" ("MasterPartId", "ExecutionOrder");

CREATE INDEX "IX_StageDefinitions_StageName" ON "StageDefinitions" ("StageName");

CREATE INDEX "IX_StageDependencies_DependencyType" ON "StageDependencies" ("DependencyType");

CREATE INDEX "IX_StageDependencies_DependentStageId" ON "StageDependencies" ("DependentStageId");

CREATE INDEX "IX_StageDependencies_RequiredStageId" ON "StageDependencies" ("RequiredStageId");

CREATE INDEX "IX_StageExecutions_OperatorUserId" ON "StageExecutions" ("OperatorUserId");

CREATE INDEX "IX_StageExecutions_ProductionBuildId" ON "StageExecutions" ("ProductionBuildId");

CREATE INDEX "IX_StageExecutions_ProductionBuildId_ExecutionOrder" ON "StageExecutions" ("ProductionBuildId", "ExecutionOrder");

CREATE INDEX "IX_StageExecutions_StageDefinitionId" ON "StageExecutions" ("StageDefinitionId");

CREATE INDEX "IX_StageExecutions_StageName" ON "StageExecutions" ("StageName");

CREATE INDEX "IX_StageExecutions_StartTime" ON "StageExecutions" ("StartTime");

CREATE INDEX "IX_StageExecutions_Status" ON "StageExecutions" ("Status");

CREATE INDEX "IX_StageNotes_CreatedDate" ON "StageNotes" ("CreatedDate");

CREATE INDEX "IX_StageNotes_NoteType" ON "StageNotes" ("NoteType");

CREATE INDEX "IX_StageNotes_Priority" ON "StageNotes" ("Priority");

CREATE INDEX "IX_StageNotes_StageId" ON "StageNotes" ("StageId");

CREATE INDEX "IX_StageTemplateCategories_IsActive" ON "StageTemplateCategories" ("IsActive");

CREATE UNIQUE INDEX "IX_StageTemplateCategories_Name" ON "StageTemplateCategories" ("Name");

CREATE INDEX "IX_StageTemplateCategories_SortOrder" ON "StageTemplateCategories" ("SortOrder");

CREATE INDEX "IX_StageTemplates_ComplexityLevel" ON "StageTemplates" ("ComplexityLevel");

CREATE INDEX "IX_StageTemplates_Industry" ON "StageTemplates" ("Industry");

CREATE INDEX "IX_StageTemplates_Industry_MaterialType_ComplexityLevel" ON "StageTemplates" ("Industry", "MaterialType", "ComplexityLevel");

CREATE INDEX "IX_StageTemplates_IsActive" ON "StageTemplates" ("IsActive");

CREATE INDEX "IX_StageTemplates_MaterialType" ON "StageTemplates" ("MaterialType");

CREATE INDEX "IX_StageTemplates_Name" ON "StageTemplates" ("Name");

CREATE INDEX "IX_StageTemplates_StageTemplateCategoryId" ON "StageTemplates" ("StageTemplateCategoryId");

CREATE INDEX "IX_StageTemplates_UsageCount" ON "StageTemplates" ("UsageCount");

CREATE INDEX "IX_StageTemplateSteps_ProductionStageId" ON "StageTemplateSteps" ("ProductionStageId");

CREATE INDEX "IX_StageTemplateSteps_StageTemplateId" ON "StageTemplateSteps" ("StageTemplateId");

CREATE INDEX "IX_StageTemplateSteps_StageTemplateId_ExecutionOrder" ON "StageTemplateSteps" ("StageTemplateId", "ExecutionOrder");

CREATE UNIQUE INDEX "IX_StageTemplateSteps_StageTemplateId_ProductionStageId" ON "StageTemplateSteps" ("StageTemplateId", "ProductionStageId");

CREATE INDEX "IX_SystemSettings_Category" ON "SystemSettings" ("Category");

CREATE INDEX "IX_SystemSettings_Category_DisplayOrder" ON "SystemSettings" ("Category", "DisplayOrder");

CREATE INDEX "IX_SystemSettings_IsActive" ON "SystemSettings" ("IsActive");

CREATE UNIQUE INDEX "IX_SystemSettings_SettingKey" ON "SystemSettings" ("SettingKey");

CREATE UNIQUE INDEX "IX_UserSettings_UserId" ON "UserSettings" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260112140147_AddCrmV1', '8.0.11');

