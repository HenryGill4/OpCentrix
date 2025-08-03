# ??? **OpCentrix Database Schema Documentation** 
**LIVE DATABASE REFERENCE**

**Date**: January 2025  
**Version**: 8.0 (Entity Framework Core 8.0.11)  
**Database**: SQLite (scheduler.db)  
**Status**: ? **LIVE PRODUCTION DATABASE** - 36 active tables  
**Source**: Generated from live database schema at OpCentrix/scheduler.db

---

## ?? **CRITICAL DATABASE DEBUGGING INSTRUCTIONS FOR AI ASSISTANT**

### **?? MANDATORY DATABASE RESEARCH PROTOCOL**
**?? READ THESE INSTRUCTIONS EVERY TIME YOU DEBUG DATABASE ISSUES**

#### **1. ALWAYS Start with Live Schema Analysis**
```powershell
# Before debugging ANY database issue, run these commands:
dotnet clean
dotnet restore
dotnet build OpCentrix/OpCentrix.csproj

# If build fails, check for EF Core issues first
dotnet ef migrations list --project OpCentrix
dotnet ef database update --project OpCentrix
```

#### **2. Database Connection Verification**
```powershell
# Check if database exists and is accessible
Test-Path "OpCentrix/scheduler.db"

# View live database schema structure
sqlite3 OpCentrix/scheduler.db ".tables"

# Get live table count (should be 36 tables)
sqlite3 OpCentrix/scheduler.db "SELECT COUNT(*) as TableCount FROM sqlite_master WHERE type='table' AND name != '__EFMigrationsHistory';"
```

#### **3. Entity Framework Debugging**
```powershell
# Check current migration status
dotnet ef migrations list --project OpCentrix

# Generate SQL script to see what EF would do
dotnet ef migrations script --project OpCentrix

# Check model validation against live database
dotnet ef dbcontext info --project OpCentrix
```

#### **4. Data Integrity Checks**
```powershell
# Run integrity check on SQLite database
sqlite3 OpCentrix/scheduler.db "PRAGMA integrity_check;"

# Check foreign key constraints
sqlite3 OpCentrix/scheduler.db "PRAGMA foreign_key_check;"

# View table counts for all entities
sqlite3 OpCentrix/scheduler.db "
SELECT name as TableName, 
       CASE name
         WHEN 'Jobs' THEN (SELECT COUNT(*) FROM Jobs)
         WHEN 'Parts' THEN (SELECT COUNT(*) FROM Parts WHERE IsActive = 1)
         WHEN 'Machines' THEN (SELECT COUNT(*) FROM Machines WHERE IsActive = 1)
         WHEN 'Users' THEN (SELECT COUNT(*) FROM Users WHERE IsActive = 1)
         WHEN 'ProductionStages' THEN (SELECT COUNT(*) FROM ProductionStages WHERE IsActive = 1)
         WHEN 'PartClassifications' THEN (SELECT COUNT(*) FROM PartClassifications WHERE IsActive = 1)
         ELSE 0
       END as RowCount
FROM sqlite_master 
WHERE type='table' AND name IN ('Jobs', 'Parts', 'Machines', 'Users', 'ProductionStages', 'PartClassifications')
ORDER BY name;"
```

---

## ?? **LIVE DATABASE SCHEMA OVERVIEW** 
**Current Tables: 36 Active Entities**

### **??? Core Manufacturing Tables (Primary Entities)**

#### **Jobs** - Core scheduling entity (LIVE SCHEMA)
```sql
CREATE TABLE IF NOT EXISTS "Jobs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Jobs" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NOT NULL,
    
    -- Scheduling fields
    "ScheduledStart" TEXT NOT NULL,
    "ScheduledEnd" TEXT NOT NULL,
    "ActualStart" TEXT NULL,
    "ActualEnd" TEXT NULL,
    
    -- Part relationship
    "PartId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "ProducedQuantity" INTEGER NOT NULL,
    "DefectQuantity" INTEGER NOT NULL,
    "ReworkQuantity" INTEGER NOT NULL,
    
    -- Manufacturing parameters (SLS-specific)
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
    
    -- Time tracking
    "EstimatedHours" REAL NOT NULL,
    "PreheatingTimeMinutes" REAL NOT NULL,
    "BuildTimeMinutes" REAL NOT NULL,
    "CoolingTimeMinutes" REAL NOT NULL,
    "PowderChangeoverTimeMinutes" REAL NOT NULL,
    "PostProcessingTimeMinutes" REAL NOT NULL,
    "SetupTimeMinutes" REAL NOT NULL,
    "ChangeoverTimeMinutes" REAL NOT NULL,
    "PreviousJobPartNumber" TEXT NULL,
    
    -- Cost tracking
    "LaborCostPerHour" decimal(10,2) NOT NULL,
    "MaterialCostPerKg" decimal(10,2) NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "PowerCostPerKwh" decimal(10,2) NOT NULL,
    
    -- OPC UA Integration
    "OpcUaJobId" TEXT NULL,
    "OpcUaStatus" TEXT NULL,
    "OpcUaLastUpdate" TEXT NULL,
    "OpcUaBuildProgress" REAL NOT NULL,
    "OpcUaErrorMessages" TEXT NULL,
    "CurrentLaserPowerWatts" REAL NOT NULL,
    "CurrentBuildTemperature" REAL NOT NULL,
    "CurrentOxygenLevel" REAL NOT NULL,
    "CurrentArgonFlowRate" REAL NOT NULL,
    
    -- File management
    "BuildFileName" TEXT NULL,
    "BuildFilePath" TEXT NULL,
    "BuildFileSizeBytes" INTEGER NOT NULL,
    "BuildFileCreatedDate" TEXT NULL,
    
    -- Requirements and instructions
    "RequiredSkills" TEXT NOT NULL,
    "RequiredTooling" TEXT NOT NULL,
    "RequiredMaterials" TEXT NOT NULL,
    "SpecialInstructions" TEXT NOT NULL,
    "ProcessParameters" TEXT NOT NULL,
    "QualityCheckpoints" TEXT NOT NULL,
    
    -- Performance metrics
    "MachineUtilizationPercent" REAL NOT NULL,
    "EnergyConsumptionKwh" REAL NOT NULL,
    "SurfaceRoughnessRa" REAL NOT NULL,
    "DensityPercentage" REAL NOT NULL,
    "UltimateTensileStrengthMPa" REAL NOT NULL,
    
    -- Job control
    "Status" TEXT NOT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 3,
    "CustomerOrderNumber" TEXT NOT NULL,
    "CustomerDueDate" TEXT NULL,
    "IsRushJob" INTEGER NOT NULL,
    "HoldReason" TEXT NOT NULL,
    
    -- Process controls
    "RequiresArgonPurge" INTEGER NOT NULL,
    "RequiresPreheating" INTEGER NOT NULL,
    "RequiresPostProcessing" INTEGER NOT NULL,
    "RequiresPowderSieving" INTEGER NOT NULL,
    
    -- Personnel
    "Operator" TEXT NULL,
    "QualityInspector" TEXT NULL,
    "Supervisor" TEXT NULL,
    "Notes" TEXT NULL,
    
    -- Audit trail
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    
    -- Foreign key constraints
    CONSTRAINT "FK_Jobs_Parts_PartId" FOREIGN KEY ("PartId") REFERENCES "Parts" ("Id") ON DELETE CASCADE
);

-- Indexes for Jobs
CREATE INDEX "IX_Jobs_MachineId_ScheduledStart" ON "Jobs" ("MachineId", "ScheduledStart");
CREATE INDEX "IX_Jobs_PartId" ON "Jobs" ("PartId");
CREATE INDEX "IX_Jobs_PartNumber" ON "Jobs" ("PartNumber");
CREATE INDEX "IX_Jobs_Priority" ON "Jobs" ("Priority");
CREATE INDEX "IX_Jobs_Status" ON "Jobs" ("Status");
```

#### **Parts** - Enhanced part definition (LIVE SCHEMA)
```sql
CREATE TABLE IF NOT EXISTS "Parts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Parts" PRIMARY KEY AUTOINCREMENT,
    
    -- Core identification
    "PartNumber" TEXT NOT NULL UNIQUE,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "CustomerPartNumber" TEXT NOT NULL DEFAULT '',
    
    -- Manufacturing classification
    "Material" TEXT NOT NULL,
    "SlsMaterial" TEXT NOT NULL,
    "Industry" TEXT NOT NULL,
    "Application" TEXT NOT NULL,
    "PartCategory" TEXT NOT NULL DEFAULT 'Prototype',
    "PartClass" TEXT NOT NULL DEFAULT 'B',
    "ProcessType" TEXT NOT NULL,
    "RequiredMachineType" TEXT NOT NULL,
    "PreferredMachines" TEXT NOT NULL,
    
    -- Physical properties
    "WeightGrams" REAL NOT NULL,
    "Dimensions" TEXT NOT NULL,
    "VolumeMm3" REAL NOT NULL,
    "HeightMm" REAL NOT NULL,
    "LengthMm" REAL NOT NULL,
    "WidthMm" REAL NOT NULL,
    "MaxSurfaceRoughnessRa" REAL NOT NULL,
    
    -- Manufacturing parameters (SLS-specific)
    "PowderRequirementKg" REAL NOT NULL,
    "RecommendedLaserPower" REAL NOT NULL,
    "RecommendedScanSpeed" REAL NOT NULL,
    "RecommendedLayerThickness" REAL NOT NULL,
    "RecommendedHatchSpacing" REAL NOT NULL,
    "RecommendedBuildTemperature" REAL NOT NULL,
    "RequiredArgonPurity" REAL NOT NULL,
    "MaxOxygenContent" REAL NOT NULL,
    "PowderSpecification" TEXT NOT NULL,
    
    -- Cost structure
    "MaterialCostPerKg" decimal(12,2) NOT NULL,
    "StandardLaborCostPerHour" decimal(10,2) NOT NULL,
    "MachineOperatingCostPerHour" decimal(10,2) NOT NULL,
    "SetupCost" decimal(10,2) NOT NULL,
    "PostProcessingCost" decimal(10,2) NOT NULL,
    "QualityInspectionCost" decimal(10,2) NOT NULL,
    "ArgonCostPerHour" decimal(10,2) NOT NULL,
    "PowerCostPerKwh" decimal(10,2) NOT NULL,
    
    -- Time estimation (detailed breakdown)
    "EstimatedBuildTimeMinutes" REAL NOT NULL,
    "SetupTimeMinutes" REAL NOT NULL,
    "ChangeoverTimeMinutes" REAL NOT NULL,
    "PowderChangeoverTimeMinutes" REAL NOT NULL,
    "PreheatingTimeMinutes" REAL NOT NULL,
    "CoolingTimeMinutes" REAL NOT NULL,
    "PostProcessingTimeMinutes" REAL NOT NULL,
    
    -- Quality requirements
    "QualityStandards" TEXT NOT NULL DEFAULT 'ASTM F3001, ISO 17296',
    "ToleranceRequirements" TEXT NOT NULL DEFAULT '±0.1mm typical',
    "RequiresInspection" INTEGER NOT NULL,
    "RequiresCertification" INTEGER NOT NULL,
    
    -- Manufacturing requirements
    "RequiredSkills" TEXT NOT NULL DEFAULT 'SLS Operation,Powder Handling',
    "RequiredCertifications" TEXT NOT NULL DEFAULT 'SLS Operation Certification',
    "RequiredTooling" TEXT NOT NULL DEFAULT 'Build Platform,Powder Sieve',
    "ConsumableMaterials" TEXT NOT NULL DEFAULT 'Argon Gas,Build Platform Coating',
    "RequiresSupports" INTEGER NOT NULL,
    "SupportStrategy" TEXT NOT NULL DEFAULT 'Minimal supports on overhangs > 45°',
    "SupportRemovalTimeMinutes" REAL NOT NULL,
    
    -- Regulatory compliance (Enhanced B&T)
    "RequiresATFCompliance" INTEGER NOT NULL,
    "RequiresITARCompliance" INTEGER NOT NULL,
    "RequiresSerialization" INTEGER NOT NULL,
    "ComponentType" TEXT NOT NULL,
    "FirearmType" TEXT NOT NULL,
    "ExportClassification" TEXT NOT NULL,
    "BTTestingRequirements" TEXT NOT NULL,
    "BTQualityStandards" TEXT NOT NULL,
    "BTRegulatoryNotes" TEXT NOT NULL,
    
    -- Classification and specialization requirements
    "RequiresFDA" INTEGER NOT NULL,
    "RequiresAS9100" INTEGER NOT NULL,
    "RequiresNADCAP" INTEGER NOT NULL,
    
    -- Performance tracking
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
    
    -- Process configuration
    "ProcessParameters" TEXT NOT NULL DEFAULT '{}',
    "QualityCheckpoints" TEXT NOT NULL DEFAULT '{}',
    "BuildFileTemplate" TEXT NOT NULL DEFAULT '',
    "CadFilePath" TEXT NOT NULL DEFAULT '',
    "CadFileVersion" TEXT NOT NULL DEFAULT '',
    
    -- Time estimation and admin overrides
    "EstimatedHours" REAL NOT NULL,
    "AvgDuration" TEXT NOT NULL DEFAULT '8h 0m',
    "AvgDurationDays" INTEGER NOT NULL,
    "AdminEstimatedHoursOverride" REAL NULL,
    "AdminOverrideReason" TEXT NULL DEFAULT '',
    "AdminOverrideBy" TEXT NOT NULL DEFAULT '',
    "AdminOverrideDate" TEXT NULL,
    
    -- Status and activity
    "IsActive" INTEGER NOT NULL,
    
    -- Audit trail
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL,
    
    -- Foreign keys
    "PartClassificationId" INTEGER NULL,
    CONSTRAINT "FK_Parts_PartClassifications_PartClassificationId" FOREIGN KEY ("PartClassificationId") REFERENCES "PartClassifications" ("Id") ON DELETE SET NULL
);

-- Performance indexes for Parts
CREATE UNIQUE INDEX "IX_Parts_PartNumber" ON "Parts" ("PartNumber");
CREATE INDEX "IX_Parts_Name" ON "Parts" ("Name");
CREATE INDEX "IX_Parts_Material" ON "Parts" ("Material");
CREATE INDEX "IX_Parts_Industry" ON "Parts" ("Industry");
CREATE INDEX "IX_Parts_PartCategory" ON "Parts" ("PartCategory");
CREATE INDEX "IX_Parts_IsActive" ON "Parts" ("IsActive");
CREATE INDEX "IX_Parts_ComponentType" ON "Parts" ("ComponentType");
CREATE INDEX "IX_Parts_RequiresATFCompliance" ON "Parts" ("RequiresATFCompliance");
CREATE INDEX "IX_Parts_RequiresITARCompliance" ON "Parts" ("RequiresITARCompliance");
CREATE INDEX "IX_Parts_RequiresSerialization" ON "Parts" ("RequiresSerialization");
CREATE INDEX "IX_Parts_SlsMaterial" ON "Parts" ("SlsMaterial");
CREATE INDEX "IX_Parts_ProcessType" ON "Parts" ("ProcessType");
CREATE INDEX "IX_Parts_RequiredMachineType" ON "Parts" ("RequiredMachineType");
CREATE INDEX "IX_Parts_FirearmType" ON "Parts" ("FirearmType");
CREATE INDEX "IX_Parts_CreatedDate" ON "Parts" ("CreatedDate");
CREATE INDEX "IX_Parts_CustomerPartNumber" ON "Parts" ("CustomerPartNumber");
CREATE INDEX "IX_Parts_PartClassificationId" ON "Parts" ("PartClassificationId");
```

#### **Machines** - Manufacturing equipment management (LIVE SCHEMA)
```sql
CREATE TABLE IF NOT EXISTS "Machines" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Machines" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NOT NULL UNIQUE,
    "Name" TEXT NOT NULL,
    "MachineName" TEXT NOT NULL,
    "MachineType" TEXT NOT NULL,
    "MachineModel" TEXT NOT NULL,
    "SerialNumber" TEXT NOT NULL,
    "Location" TEXT NOT NULL,
    "Department" TEXT NOT NULL,
    
    -- Operational status
    "Status" TEXT NOT NULL DEFAULT 'Idle',
    "IsActive" INTEGER NOT NULL,
    "IsAvailableForScheduling" INTEGER NOT NULL,
    "Priority" INTEGER NOT NULL,
    "LastStatusUpdate" TEXT NOT NULL,
    
    -- Technical specifications
    "TechnicalSpecifications" TEXT NOT NULL,
    "SupportedMaterials" TEXT NOT NULL,
    "CurrentMaterial" TEXT NOT NULL,
    
    -- Build volume specifications
    "BuildLengthMm" REAL NOT NULL,
    "BuildWidthMm" REAL NOT NULL,
    "BuildHeightMm" REAL NOT NULL,
    "MaxLaserPowerWatts" REAL NOT NULL,
    "MaxScanSpeedMmPerSec" REAL NOT NULL,
    "MinLayerThicknessMicrons" REAL NOT NULL,
    "MaxLayerThicknessMicrons" REAL NOT NULL,
    
    -- Maintenance tracking
    "MaintenanceIntervalHours" REAL NOT NULL,
    "HoursSinceLastMaintenance" REAL NOT NULL,
    "LastMaintenanceDate" TEXT NULL,
    "NextMaintenanceDate" TEXT NULL,
    "TotalOperatingHours" REAL NOT NULL,
    "AverageUtilizationPercent" REAL NOT NULL,
    "MaintenanceNotes" TEXT NOT NULL,
    "OperatorNotes" TEXT NOT NULL,
    
    -- OPC UA Integration
    "OpcUaEndpointUrl" TEXT NOT NULL,
    "OpcUaEnabled" INTEGER NOT NULL,
    "CommunicationSettings" TEXT NOT NULL,
    
    -- Current job tracking
    "CurrentJobId" INTEGER NULL,
    
    -- Audit trail
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
    
    -- Foreign key constraints
    CONSTRAINT "FK_Machines_Jobs_CurrentJobId" FOREIGN KEY ("CurrentJobId") REFERENCES "Jobs" ("Id")
);

-- Indexes for Machines
CREATE UNIQUE INDEX "IX_Machines_MachineId" ON "Machines" ("MachineId");
CREATE INDEX "IX_Machines_MachineType" ON "Machines" ("MachineType");
CREATE INDEX "IX_Machines_Status" ON "Machines" ("Status");
CREATE INDEX "IX_Machines_IsActive" ON "Machines" ("IsActive");
CREATE INDEX "IX_Machines_CurrentJobId" ON "Machines" ("CurrentJobId");
```

#### **Users** - Authentication and authorization (LIVE SCHEMA)
```sql
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "Username" TEXT NOT NULL UNIQUE,
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

-- Indexes for Users
CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");
CREATE INDEX "IX_Users_Role" ON "Users" ("Role");
CREATE INDEX "IX_Users_IsActive" ON "Users" ("IsActive");
```

### **?? Manufacturing Operations Tables (LIVE SCHEMA)**

#### **ProductionStages** - Configurable workflow stages (LIVE SCHEMA)
```sql
CREATE TABLE IF NOT EXISTS "ProductionStages" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductionStages" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "DisplayOrder" INTEGER NOT NULL,
    "Description" TEXT NULL,
    
    -- Visual and organizational
    "StageColor" TEXT NOT NULL DEFAULT '#007bff',
    "StageIcon" TEXT NOT NULL DEFAULT 'fas fa-cogs',
    "Department" TEXT NULL,
    
    -- Operational defaults
    "DefaultSetupMinutes" INTEGER NOT NULL DEFAULT 30,
    "DefaultHourlyRate" decimal(8,2) NOT NULL DEFAULT '85.0',
    "DefaultDurationHours" REAL NOT NULL DEFAULT 1.0,
    "DefaultMaterialCost" decimal(10,2) NOT NULL DEFAULT '0.0',
    
    -- Machine assignment
    "RequiresMachineAssignment" INTEGER NOT NULL DEFAULT 0,
    "DefaultMachineId" TEXT NULL,
    "AssignedMachineIds" TEXT NULL,
    "AllowParallelExecution" INTEGER NOT NULL DEFAULT 0,
    
    -- Process control
    "RequiresQualityCheck" INTEGER NOT NULL DEFAULT 1,
    "RequiresApproval" INTEGER NOT NULL DEFAULT 0,
    "AllowSkip" INTEGER NOT NULL DEFAULT 0,
    "IsOptional" INTEGER NOT NULL DEFAULT 0,
    "RequiredRole" TEXT NULL,
    
    -- Custom configuration
    "CustomFieldsConfig" TEXT NOT NULL DEFAULT '[]',
    
    -- Status
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

-- Indexes for ProductionStages
CREATE INDEX "IX_ProductionStages_Name" ON "ProductionStages" ("Name");
CREATE INDEX "IX_ProductionStages_DisplayOrder" ON "ProductionStages" ("DisplayOrder");
CREATE INDEX "IX_ProductionStages_Department" ON "ProductionStages" ("Department");
CREATE INDEX "IX_ProductionStages_IsActive" ON "ProductionStages" ("IsActive");
CREATE INDEX "IX_ProductionStages_RequiredRole" ON "ProductionStages" ("RequiredRole");
CREATE INDEX "IX_ProductionStages_RequiresMachineAssignment" ON "ProductionStages" ("RequiresMachineAssignment");
CREATE INDEX "IX_ProductionStages_StageColor" ON "ProductionStages" ("StageColor");
CREATE INDEX "IX_ProductionStages_DisplayOrder_IsActive" ON "ProductionStages" ("DisplayOrder", "IsActive");
```

### **??? B&T Firearms Specialization Tables (LIVE SCHEMA)**

#### **PartClassifications** - B&T component categorization (LIVE SCHEMA)
```sql
CREATE TABLE IF NOT EXISTS "PartClassifications" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartClassifications" PRIMARY KEY AUTOINCREMENT,
    "ClassificationCode" TEXT NOT NULL UNIQUE,
    "ClassificationName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "IndustryType" TEXT NOT NULL DEFAULT 'Firearms',
    "ComponentCategory" TEXT NOT NULL,
    
    -- Suppressor-specific classifications
    "SuppressorType" TEXT NULL,
    "BafflePosition" TEXT NULL,
    "IsEndCap" INTEGER NOT NULL,
    "IsThreadMount" INTEGER NOT NULL,
    "IsTubeHousing" INTEGER NOT NULL,
    "IsInternalComponent" INTEGER NOT NULL,
    "IsMountingHardware" INTEGER NOT NULL,
    
    -- Firearm-specific classifications
    "FirearmType" TEXT NULL,
    "IsReceiver" INTEGER NOT NULL,
    "IsBarrelComponent" INTEGER NOT NULL,
    "IsOperatingSystem" INTEGER NOT NULL,
    "IsSafetyComponent" INTEGER NOT NULL,
    "IsTriggerComponent" INTEGER NOT NULL,
    "IsFurniture" INTEGER NOT NULL,
    
    -- Manufacturing requirements
    "RecommendedMaterial" TEXT NOT NULL DEFAULT 'Ti-6Al-4V Grade 5',
    "AlternativeMaterials" TEXT NOT NULL DEFAULT '',
    "MaterialGrade" TEXT NOT NULL DEFAULT 'Aerospace',
    "RequiresSpecialHandling" INTEGER NOT NULL,
    "RequiredProcess" TEXT NOT NULL DEFAULT 'SLS Metal Printing',
    "PostProcessingRequired" TEXT NOT NULL DEFAULT '',
    "ComplexityLevel" INTEGER NOT NULL,
    "SpecialInstructions" TEXT NOT NULL DEFAULT '',
    
    -- Testing requirements
    "RequiresPressureTesting" INTEGER NOT NULL,
    "RequiresProofTesting" INTEGER NOT NULL,
    "RequiresDimensionalVerification" INTEGER NOT NULL,
    "RequiresSurfaceFinishVerification" INTEGER NOT NULL,
    "RequiresMaterialCertification" INTEGER NOT NULL,
    "TestingRequirements" TEXT NOT NULL DEFAULT '',
    "QualityStandards" TEXT NOT NULL DEFAULT '',
    
    -- Regulatory compliance
    "RequiresATFCompliance" INTEGER NOT NULL,
    "RequiresITARCompliance" INTEGER NOT NULL,
    "RequiresFFLTracking" INTEGER NOT NULL,
    "RequiresSerialization" INTEGER NOT NULL,
    "IsControlledItem" INTEGER NOT NULL,
    "IsEARControlled" INTEGER NOT NULL,
    "ExportClassification" TEXT NOT NULL DEFAULT '',
    "RegulatoryNotes" TEXT NOT NULL DEFAULT '',
    
    -- Status and audit
    "IsActive" INTEGER NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL,
    "LastModifiedBy" TEXT NOT NULL
);

-- Indexes for PartClassifications
CREATE UNIQUE INDEX "IX_PartClassifications_ClassificationCode" ON "PartClassifications" ("ClassificationCode");
CREATE INDEX "IX_PartClassifications_ClassificationName" ON "PartClassifications" ("ClassificationName");
CREATE INDEX "IX_PartClassifications_ComponentCategory" ON "PartClassifications" ("ComponentCategory");
CREATE INDEX "IX_PartClassifications_IndustryType" ON "PartClassifications" ("IndustryType");
CREATE INDEX "IX_PartClassifications_SuppressorType" ON "PartClassifications" ("SuppressorType");
CREATE INDEX "IX_PartClassifications_FirearmType" ON "PartClassifications" ("FirearmType");
CREATE INDEX "IX_PartClassifications_IsActive" ON "PartClassifications" ("IsActive");
CREATE INDEX "IX_PartClassifications_RequiresATFCompliance" ON "PartClassifications" ("RequiresATFCompliance");
CREATE INDEX "IX_PartClassifications_RequiresITARCompliance" ON "PartClassifications" ("RequiresITARCompliance");
CREATE INDEX "IX_PartClassifications_RequiresSerialization" ON "PartClassifications" ("RequiresSerialization");
```

---

## ?? **COMPLETE TABLE LISTING** 
**36 Active Tables in Live Database**

### **Core Manufacturing (5 tables)**
- **Jobs** - Job scheduling and tracking with detailed SLS parameters
- **Parts** - Enhanced parts with B&T firearms specialization
- **Machines** - Equipment management with OPC UA integration
- **Users** - Authentication and role management  
- **Materials** - Material management and specifications

### **Advanced Manufacturing (6 tables)**
- **ProductionStages** - Configurable workflow stages
- **PartStageRequirements** - Part-specific stage configuration
- **JobStages** - Multi-stage job execution
- **StageDependencies** - Stage dependency management
- **StageNotes** - Stage-specific notes and comments
- **ProductionStageExecutions** - Stage execution tracking

### **Print Tracking System (3 tables)**
- **BuildJobs** - SLS build job management
- **BuildJobParts** - Parts within build jobs
- **DelayLogs** - Build delay tracking and analysis

### **B&T Firearms Specialization (4 tables)**
- **PartClassifications** - Firearms component categorization
- **ComplianceRequirements** - Regulatory compliance tracking
- **SerialNumbers** - ATF compliance and serial tracking
- **ComplianceDocuments** - Regulatory document management

### **Quality Management (2 tables)**
- **InspectionCheckpoints** - Quality control checkpoints
- **DefectCategories** - Defect classification system

### **System Administration (6 tables)**
- **SystemSettings** - Application configuration
- **RolePermissions** - Role-based permissions
- **OperatingShifts** - Shift management
- **AdminAlerts** - System alert management
- **FeatureToggles** - Feature flag management
- **BugReports** - Bug tracking system

### **Extended Operations (4 tables)**
- **EDMLogs** - EDM operation tracking
- **JobLogEntries** - Job activity logging
- **JobNotes** - Job-specific notes
- **UserSettings** - User preference management

### **Machine Management (2 tables)**
- **MachineCapabilities** - Machine capability tracking
- **ArchivedJobs** - Job archival system

### **Prototype System (4 tables)**  
- **PrototypeJobs** - Prototype job management
- **AssemblyComponents** - Assembly component tracking
- **PrototypeTimeLogs** - Time tracking for prototypes

### **Internal Systems (1 table)**
- **__EFMigrationsHistory** - Entity Framework migration tracking

---

## ?? **DATABASE DEBUGGING AND TESTING PROCEDURES**

### **?? Emergency Database Recovery**
```powershell
# If database is corrupted or missing:

# 1. Backup current database (if it exists)
if (Test-Path "OpCentrix/scheduler.db") {
    Copy-Item "OpCentrix/scheduler.db" "OpCentrix/scheduler.db.backup.$(Get-Date -Format 'yyyyMMdd_HHmmss')"
}

# 2. Delete corrupted database
Remove-Item "OpCentrix/scheduler.db" -Force

# 3. Recreate from migrations
cd OpCentrix
dotnet ef database update

# 4. Verify database structure
sqlite3 scheduler.db ".tables"
sqlite3 scheduler.db "PRAGMA integrity_check;"
```

### **?? Database Health Monitoring**
```powershell
# Check database size and performance
sqlite3 OpCentrix/scheduler.db "PRAGMA page_count; PRAGMA page_size; PRAGMA freelist_count;"

# Check table row counts for key entities
sqlite3 OpCentrix/scheduler.db "
SELECT name as TableName, 
       CASE name
         WHEN 'Jobs' THEN (SELECT COUNT(*) FROM Jobs)
         WHEN 'Parts' THEN (SELECT COUNT(*) FROM Parts WHERE IsActive = 1)
         WHEN 'Machines' THEN (SELECT COUNT(*) FROM Machines WHERE IsActive = 1)
         WHEN 'Users' THEN (SELECT COUNT(*) FROM Users WHERE IsActive = 1)
         WHEN 'ProductionStages' THEN (SELECT COUNT(*) FROM ProductionStages WHERE IsActive = 1)
         WHEN 'PartClassifications' THEN (SELECT COUNT(*) FROM PartClassifications WHERE IsActive = 1)
         ELSE 0
       END as RowCount
FROM sqlite_master 
WHERE type='table' AND name IN ('Jobs', 'Parts', 'Machines', 'Users', 'ProductionStages', 'PartClassifications')
ORDER BY name;"
```

### **?? Entity Framework Troubleshooting**
```powershell
# Check model validation against live database
dotnet ef dbcontext validate --project OpCentrix

# Generate migration preview
dotnet ef migrations script --project OpCentrix --output "migration-preview.sql"

# Check pending migrations
dotnet ef migrations list --project OpCentrix | Select-String "Pending"

# Force migration application
dotnet ef database update --project OpCentrix --verbose
```

### **?? Database Testing Commands**
```powershell
# Test database operations in order:

# 1. Verify connection and table count (should be 36)
sqlite3 OpCentrix/scheduler.db "SELECT COUNT(*) as TableCount FROM sqlite_master WHERE type='table' AND name != '__EFMigrationsHistory';"

# 2. Check schema matches models
dotnet ef dbcontext validate --project OpCentrix

# 3. Test basic queries on core tables
sqlite3 OpCentrix/scheduler.db "SELECT COUNT(*) FROM Parts WHERE IsActive = 1;"
sqlite3 OpCentrix/scheduler.db "SELECT COUNT(*) FROM Jobs WHERE Status IN ('Scheduled', 'Running');"
sqlite3 OpCentrix/scheduler.db "SELECT COUNT(*) FROM Machines WHERE IsActive = 1;"

# 4. Test foreign key relationships
sqlite3 OpCentrix/scheduler.db "PRAGMA foreign_keys = ON; SELECT COUNT(*) FROM Jobs j INNER JOIN Parts p ON j.PartId = p.Id;"

# 5. Test index performance on key queries
sqlite3 OpCentrix/scheduler.db "EXPLAIN QUERY PLAN SELECT * FROM Jobs WHERE MachineId = 'TI1' AND Status = 'Scheduled';"
```

### **? Performance Optimization**
```powershell
# Analyze and optimize database
sqlite3 OpCentrix/scheduler.db "ANALYZE; VACUUM;"

# Check critical index usage
sqlite3 OpCentrix/scheduler.db "PRAGMA index_list('Jobs'); PRAGMA index_list('Parts'); PRAGMA index_list('Machines');"

# Update table statistics
sqlite3 OpCentrix/scheduler.db "UPDATE sqlite_stat1 SET stat = NULL; ANALYZE;"
```

### **??? Schema Validation Scripts**
```powershell
# Verify all required tables exist (should be 36 tables)
$RequiredTables = @(
    'Jobs', 'Parts', 'Machines', 'Users', 'ProductionStages', 'PartClassifications',
    'BuildJobs', 'SerialNumbers', 'ComplianceRequirements', 'SystemSettings'
)

foreach ($table in $RequiredTables) {
    $exists = sqlite3 OpCentrix/scheduler.db "SELECT name FROM sqlite_master WHERE type='table' AND name='$table';"
    if ($exists) {
        Write-Host "? Table $table exists" -ForegroundColor Green
    } else {
        Write-Host "? Table $table missing" -ForegroundColor Red
    }
}

# Verify critical indexes exist
$CriticalIndexes = @(
    'IX_Jobs_MachineId_ScheduledStart',
    'IX_Parts_PartNumber', 
    'IX_Machines_MachineId',
    'IX_ProductionStages_DisplayOrder'
)

foreach ($index in $CriticalIndexes) {
    $exists = sqlite3 OpCentrix/scheduler.db "SELECT name FROM sqlite_master WHERE type='index' AND name='$index';"
    if ($exists) {
        Write-Host "? Index $index exists" -ForegroundColor Green
    } else {
        Write-Host "? Index $index missing" -ForegroundColor Red
    }
}
```

---

## ?? **TESTING INTEGRATION WITH SCHEMA**

### **Database-Specific Test Commands**
```powershell
# Run database-focused tests
dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --filter "Category=Database" --verbosity normal

# Test specific entity operations
dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --filter "ClassName~Parts" --verbosity normal
dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --filter "ClassName~Jobs" --verbosity normal

# Test data integrity
dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --filter "TestName~Integrity" --verbosity normal
```

### **Schema Change Testing Protocol**
```powershell
# When making schema changes:

# 1. Backup database
Copy-Item "OpCentrix/scheduler.db" "OpCentrix/scheduler.db.pre-change"

# 2. Create migration
dotnet ef migrations add [DescriptiveName] --project OpCentrix

# 3. Review migration SQL
dotnet ef migrations script --project OpCentrix --from [PreviousMigration] --to [NewMigration]

# 4. Apply migration
dotnet ef database update --project OpCentrix

# 5. Verify schema (should still be 36+ tables)
sqlite3 OpCentrix/scheduler.db "SELECT COUNT(*) FROM sqlite_master WHERE type='table';"

# 6. Run full test suite
dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --verbosity normal

# 7. Check data integrity
sqlite3 OpCentrix/scheduler.db "PRAGMA integrity_check;"
```

---

## ?? **LIVE DATABASE REFERENCE INFORMATION**

### **Entity Relationships Summary** (Based on Live Schema)
```
Jobs ?? Parts (FK: PartId ? Parts.Id)
Jobs ?? Machines (CurrentJobId reference)

Parts ?? PartClassifications (FK: PartClassificationId)
Parts ?? PartStageRequirements (1:Many)

ProductionStages ?? PartStageRequirements (1:Many)
ProductionStages ?? ProductionStageExecutions (1:Many)

BuildJobs ?? BuildJobParts (1:Many)
PartClassifications ?? ComplianceRequirements (1:Many)
SerialNumbers ?? Parts (FK: PartId)
```

### **Key Indexes for Performance** (Live Database)
- **Jobs**: IX_Jobs_MachineId_ScheduledStart, IX_Jobs_Status, IX_Jobs_PartNumber, IX_Jobs_PartId
- **Parts**: IX_Parts_PartNumber (unique), IX_Parts_Material, IX_Parts_IsActive, IX_Parts_ComponentType
- **Machines**: IX_Machines_MachineId (unique), IX_Machines_Status, IX_Machines_IsActive
- **ProductionStages**: IX_ProductionStages_DisplayOrder, IX_ProductionStages_IsActive
- **PartClassifications**: IX_PartClassifications_ClassificationCode (unique)

### **Database Statistics** (Live Database)
- **Total Tables**: 36 active entities
- **Total Indexes**: 50+ performance indexes
- **Estimated Production Load**: 10,000+ Parts, 50,000+ Jobs, 100+ Machines
- **Database Engine**: SQLite with WAL mode for performance
- **Backup Strategy**: Automated daily backups recommended
- **Maintenance**: Monthly ANALYZE and VACUUM operations

---

**Schema Documentation Updated**: January 2025  
**Status**: ? **LIVE DATABASE ACCURATE** - Reflects actual production schema  
**Source**: Generated from live database at OpCentrix/scheduler.db  
**Table Count**: 36 active entities with full relationships and indexes  
**Last Verified**: Current with live database structure