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
    "Notes" TEXT NOT NULL DEFAULT '',
    "HoldReason" TEXT NOT NULL DEFAULT '',
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


CREATE TABLE "JobLogEntries" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_JobLogEntries" PRIMARY KEY AUTOINCREMENT,
    "Timestamp" TEXT NOT NULL,
    "MachineId" TEXT NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "Action" TEXT NOT NULL,
    "Notes" TEXT NULL,
    "Operator" TEXT NULL
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
    "CreatedDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL
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


CREATE TABLE "Parts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Parts" PRIMARY KEY AUTOINCREMENT,
    "PartNumber" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Material" TEXT NOT NULL,
    "SlsMaterial" TEXT NOT NULL,
    "PowderSpecification" TEXT NOT NULL,
    "PowderRequirementKg" REAL NOT NULL,
    "RecommendedLaserPower" REAL NOT NULL,
    "RecommendedScanSpeed" REAL NOT NULL,
    "RecommendedLayerThickness" REAL NOT NULL,
    "RecommendedHatchSpacing" REAL NOT NULL,
    "RecommendedBuildTemperature" REAL NOT NULL,
    "RequiredArgonPurity" REAL NOT NULL,
    "MaxOxygenContent" REAL NOT NULL,
    "WeightGrams" REAL NOT NULL,
    "Dimensions" TEXT NOT NULL,
    "VolumeMm3" REAL NOT NULL,
    "HeightMm" REAL NOT NULL,
    "LengthMm" REAL NOT NULL,
    "WidthMm" REAL NOT NULL,
    "SurfaceFinishRequirement" TEXT NOT NULL,
    "MaxSurfaceRoughnessRa" REAL NOT NULL,
    "MaterialCostPerKg" decimal(12,2) NOT NULL,
    "StandardLaborCostPerHour" decimal(10,2) NOT NULL,
    "SetupCost" decimal(10,2) NOT NULL,
    "PostProcessingCost" decimal(10,2) NOT NULL,
    "QualityInspectionCost" decimal(10,2) NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "ProcessType" TEXT NOT NULL,
    "RequiredMachineType" TEXT NOT NULL,
    "PreferredMachines" TEXT NOT NULL,
    "SetupTimeMinutes" REAL NOT NULL,
    "PowderChangeoverTimeMinutes" REAL NOT NULL,
    "PreheatingTimeMinutes" REAL NOT NULL,
    "CoolingTimeMinutes" REAL NOT NULL,
    "PostProcessingTimeMinutes" REAL NOT NULL,
    "QualityStandards" TEXT NOT NULL,
    "ToleranceRequirements" TEXT NOT NULL,
    "RequiresInspection" INTEGER NOT NULL,
    "RequiresCertification" INTEGER NOT NULL,
    "RequiredSkills" TEXT NOT NULL,
    "RequiredCertifications" TEXT NOT NULL,
    "RequiredTooling" TEXT NOT NULL,
    "ConsumableMaterials" TEXT NOT NULL,
    "RequiresSupports" INTEGER NOT NULL,
    "SupportStrategy" TEXT NOT NULL,
    "SupportRemovalTimeMinutes" REAL NOT NULL,
    "CustomerPartNumber" TEXT NOT NULL,
    "PartCategory" TEXT NOT NULL,
    "PartClass" TEXT NOT NULL,
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
    "ProcessParameters" TEXT NOT NULL,
    "QualityCheckpoints" TEXT NOT NULL,
    "BuildFileTemplate" TEXT NOT NULL,
    "CadFilePath" TEXT NOT NULL,
    "CadFileVersion" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "AvgDuration" TEXT NOT NULL,
    "AvgDurationDays" INTEGER NOT NULL,
    "EstimatedHours" REAL NOT NULL,
    "AdminEstimatedHoursOverride" REAL NULL,
    "AdminOverrideReason" TEXT NULL,
    "AdminOverrideBy" TEXT NOT NULL,
    "AdminOverrideDate" TEXT NULL
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
    "IsActive" INTEGER NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "LastLoginDate" TEXT NOT NULL,
    "LastModifiedDate" TEXT NOT NULL,
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
    "Notes" TEXT NOT NULL DEFAULT '',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    "DefectCategoryId" INTEGER NULL,
    CONSTRAINT "FK_InspectionCheckpoints_DefectCategories_DefectCategoryId" FOREIGN KEY ("DefectCategoryId") REFERENCES "DefectCategories" ("Id"),
    CONSTRAINT "FK_InspectionCheckpoints_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE
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
    "CustomerOrderNumber" TEXT NOT NULL,
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
    CONSTRAINT "FK_Jobs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE
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
    "LaserRunTime" TEXT NULL,
    "GasUsed_L" REAL NULL,
    "PowderUsed_L" REAL NULL,
    "ReasonForEnd" TEXT NULL,
    "SetupNotes" TEXT NULL,
    "AssociatedScheduledJobId" INTEGER NULL,
    "CreatedAt" TEXT NOT NULL,
    "CompletedAt" TEXT NULL,
    CONSTRAINT "FK_BuildJobs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
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
    CONSTRAINT "FK_Machines_Jobs_CurrentJobId" FOREIGN KEY ("CurrentJobId") REFERENCES "Jobs" ("Id")
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
    "Unit" TEXT NOT NULL,
    "Notes" TEXT NOT NULL,
    "RequiredCertification" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    CONSTRAINT "FK_MachineCapabilities_Machines_MachineId" FOREIGN KEY ("MachineId") REFERENCES "Machines" ("Id") ON DELETE CASCADE
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


CREATE INDEX "IX_BuildJobParts_BuildJobBuildId" ON "BuildJobParts" ("BuildJobBuildId");


CREATE INDEX "IX_BuildJobs_UserId" ON "BuildJobs" ("UserId");


CREATE INDEX "IX_DefectCategories_CategoryGroup" ON "DefectCategories" ("CategoryGroup");


CREATE INDEX "IX_DefectCategories_Code" ON "DefectCategories" ("Code");


CREATE INDEX "IX_DefectCategories_IsActive" ON "DefectCategories" ("IsActive");


CREATE INDEX "IX_DefectCategories_Name" ON "DefectCategories" ("Name");


CREATE INDEX "IX_DefectCategories_SeverityLevel" ON "DefectCategories" ("SeverityLevel");


CREATE INDEX "IX_DelayLogs_BuildJobBuildId" ON "DelayLogs" ("BuildJobBuildId");


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


CREATE INDEX "IX_JobNotes_IsCompleted" ON "JobNotes" ("IsCompleted");


CREATE INDEX "IX_JobNotes_JobId" ON "JobNotes" ("JobId");


CREATE INDEX "IX_JobNotes_JobId_Step" ON "JobNotes" ("JobId", "Step");


CREATE INDEX "IX_JobNotes_PartId" ON "JobNotes" ("PartId");


CREATE INDEX "IX_JobNotes_Priority" ON "JobNotes" ("Priority");


CREATE INDEX "IX_Jobs_MachineId_ScheduledStart" ON "Jobs" ("MachineId", "ScheduledStart");


CREATE INDEX "IX_Jobs_PartId" ON "Jobs" ("PartId");


CREATE INDEX "IX_Jobs_PartNumber" ON "Jobs" ("PartNumber");


CREATE INDEX "IX_Jobs_Priority" ON "Jobs" ("Priority");


CREATE INDEX "IX_Jobs_Status" ON "Jobs" ("Status");


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


CREATE INDEX "IX_OperatingShifts_DayOfWeek" ON "OperatingShifts" ("DayOfWeek");


CREATE INDEX "IX_OperatingShifts_DayOfWeek_IsActive" ON "OperatingShifts" ("DayOfWeek", "IsActive");


CREATE INDEX "IX_OperatingShifts_IsActive" ON "OperatingShifts" ("IsActive");


CREATE INDEX "IX_OperatingShifts_IsHoliday" ON "OperatingShifts" ("IsHoliday");


CREATE INDEX "IX_OperatingShifts_SpecificDate" ON "OperatingShifts" ("SpecificDate");


CREATE INDEX "IX_Parts_CreatedDate" ON "Parts" ("CreatedDate");


CREATE INDEX "IX_Parts_Industry" ON "Parts" ("Industry");


CREATE INDEX "IX_Parts_IsActive" ON "Parts" ("IsActive");


CREATE INDEX "IX_Parts_Material" ON "Parts" ("Material");


CREATE INDEX "IX_Parts_PartCategory" ON "Parts" ("PartCategory");


CREATE UNIQUE INDEX "IX_Parts_PartNumber" ON "Parts" ("PartNumber");


CREATE INDEX "IX_RolePermissions_Category" ON "RolePermissions" ("Category");


CREATE INDEX "IX_RolePermissions_IsActive" ON "RolePermissions" ("IsActive");


CREATE INDEX "IX_RolePermissions_PermissionKey" ON "RolePermissions" ("PermissionKey");


CREATE INDEX "IX_RolePermissions_RoleName" ON "RolePermissions" ("RoleName");


CREATE UNIQUE INDEX "IX_RolePermissions_RoleName_PermissionKey" ON "RolePermissions" ("RoleName", "PermissionKey");


CREATE INDEX "IX_StageDependencies_DependencyType" ON "StageDependencies" ("DependencyType");


CREATE INDEX "IX_StageDependencies_DependentStageId" ON "StageDependencies" ("DependentStageId");


CREATE INDEX "IX_StageDependencies_RequiredStageId" ON "StageDependencies" ("RequiredStageId");


CREATE INDEX "IX_StageNotes_CreatedDate" ON "StageNotes" ("CreatedDate");


CREATE INDEX "IX_StageNotes_NoteType" ON "StageNotes" ("NoteType");


CREATE INDEX "IX_StageNotes_Priority" ON "StageNotes" ("Priority");


CREATE INDEX "IX_StageNotes_StageId" ON "StageNotes" ("StageId");


CREATE INDEX "IX_SystemSettings_Category" ON "SystemSettings" ("Category");


CREATE INDEX "IX_SystemSettings_Category_DisplayOrder" ON "SystemSettings" ("Category", "DisplayOrder");


CREATE INDEX "IX_SystemSettings_IsActive" ON "SystemSettings" ("IsActive");


CREATE UNIQUE INDEX "IX_SystemSettings_SettingKey" ON "SystemSettings" ("SettingKey");


CREATE UNIQUE INDEX "IX_UserSettings_UserId" ON "UserSettings" ("UserId");


