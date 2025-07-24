# ??? OpCentrix Database Structure Guide

## ?? Overview

This document provides a comprehensive guide to the OpCentrix database structure, including entity relationships, data models, indexing strategies, and migration management. The database is designed to support SLS manufacturing operations with role-based access control and comprehensive audit trails.

## ??? Database Architecture

### **Technology Stack**
- **ORM**: Entity Framework Core 9.0.7
- **Database Engine**: SQLite (development), SQL Server (production ready)
- **Migration System**: EF Core Code-First migrations
- **Connection Management**: Connection pooling with 30-second timeout
- **Optimization**: Strategic indexing and query optimization

### **Design Principles**
- **Normalization**: Third normal form with performance considerations
- **Referential Integrity**: Proper foreign key constraints
- **Audit Trail**: Complete change tracking for compliance
- **Performance**: Indexed queries for frequently accessed data
- **Scalability**: Designed for growth and horizontal scaling

## ?? Entity Relationship Diagram

```
Users 1:1 UserSettings
  ? 1:?
Jobs ?:1 Parts
  ? 1:?
JobLogEntries

SlsMachines 1:0..1 Jobs (CurrentJob)
     ? 1:?
MachineDataSnapshots

BuildJobs 1:? BuildJobParts
    ? 1:?
DelayLogs

[Future Department Operations]
Jobs ? CoatingOperations
Jobs ? EDMOperations
Jobs ? MachiningOperations
Jobs ? QualityInspections
Jobs ? ShippingOperations
```

## ?? Core Tables

### **1. Jobs Table** - `Jobs`
*The heart of the scheduling system*

```sql
CREATE TABLE Jobs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PartId INTEGER NOT NULL,
    MachineId NVARCHAR(50) NOT NULL DEFAULT '',
    PartNumber NVARCHAR(50) NOT NULL DEFAULT '',
    ScheduledStart DATETIME NOT NULL,
    ScheduledEnd DATETIME NOT NULL,
    ActualStart DATETIME NULL,
    ActualEnd DATETIME NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Scheduled',
    Operator NVARCHAR(100) NOT NULL DEFAULT '',
    Priority INTEGER NOT NULL DEFAULT 3,
    -- SLS-Specific Fields
    SlsMaterial NVARCHAR(100) NOT NULL DEFAULT 'Ti-6Al-4V Grade 5',
    PowderLotNumber NVARCHAR(50) NOT NULL DEFAULT '',
    BuildPlatformId NVARCHAR(50) NOT NULL DEFAULT '',
    -- Process Parameters
    ArgonPurityPercent REAL NOT NULL DEFAULT 99.9,
    OxygenContentPpm REAL NOT NULL DEFAULT 50,
    BuildTemperatureCelsius REAL NOT NULL DEFAULT 180,
    LaserPowerWatts REAL NOT NULL DEFAULT 200,
    ScanSpeedMmPerSec REAL NOT NULL DEFAULT 1200,
    LayerThicknessMicrons REAL NOT NULL DEFAULT 30,
    HatchSpacingMicrons REAL NOT NULL DEFAULT 120,
    -- Quality & Analytics
    DefectQuantity INTEGER NOT NULL DEFAULT 0,
    ProducedQuantity INTEGER NOT NULL DEFAULT 0,
    YieldPercentage REAL NOT NULL DEFAULT 100.0,
    DensityPercentage REAL NOT NULL DEFAULT 99.5,
    -- Cost Tracking
    LaborCostPerHour DECIMAL(10,2) NOT NULL DEFAULT 50.00,
    MaterialCostPerKg DECIMAL(10,2) NOT NULL DEFAULT 500.00,
    MachineOperatingCostPerHour DECIMAL(10,2) NOT NULL DEFAULT 75.00,
    ArgonCostPerHour DECIMAL(10,2) NOT NULL DEFAULT 15.00,
    PowerCostPerKwh DECIMAL(10,2) NOT NULL DEFAULT 0.12,
    -- Timestamps & Audit
    CreatedDate DATETIME NOT NULL DEFAULT (datetime('now')),
    LastModifiedDate DATETIME NOT NULL DEFAULT (datetime('now')),
    CreatedBy NVARCHAR(100) NOT NULL DEFAULT 'System',
    LastModifiedBy NVARCHAR(100) NOT NULL DEFAULT 'System',
    -- Foreign Keys
    FOREIGN KEY (PartId) REFERENCES Parts(Id) ON DELETE RESTRICT
);
```

**Key Features:**
- **Comprehensive SLS Parameters**: All critical process parameters tracked
- **Cost Analytics**: Real-time cost calculation capability
- **Quality Metrics**: Defect tracking and yield calculations
- **Audit Trail**: Complete change tracking with user attribution
- **Performance Indexes**: Optimized for scheduling queries

**Indexes:**
```sql
CREATE INDEX IX_Jobs_MachineId ON Jobs(MachineId);
CREATE INDEX IX_Jobs_ScheduledStart ON Jobs(ScheduledStart);
CREATE INDEX IX_Jobs_Status ON Jobs(Status);
CREATE INDEX IX_Jobs_PartNumber ON Jobs(PartNumber);
CREATE INDEX IX_Jobs_MachineId_ScheduledStart ON Jobs(MachineId, ScheduledStart);
CREATE INDEX IX_Jobs_SlsMaterial ON Jobs(SlsMaterial);
CREATE INDEX IX_Jobs_Priority ON Jobs(Priority);
CREATE INDEX IX_Jobs_CustomerOrderNumber ON Jobs(CustomerOrderNumber);
CREATE INDEX IX_Jobs_OpcUaJobId ON Jobs(OpcUaJobId);
```

### **2. Parts Table** - `Parts`
*Manufacturing part definitions and specifications*

```sql
CREATE TABLE Parts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PartNumber NVARCHAR(50) UNIQUE NOT NULL DEFAULT '',
    Description NVARCHAR(500) NOT NULL DEFAULT '',
    Material NVARCHAR(100) NOT NULL DEFAULT 'Ti-6Al-4V Grade 5',
    SlsMaterial NVARCHAR(100) NOT NULL DEFAULT 'Ti-6Al-4V Grade 5',
    PowderSpecification NVARCHAR(100) NOT NULL DEFAULT '15-45 ?m particle size',
    AvgDuration NVARCHAR(50) NOT NULL DEFAULT '8h',
    EstimatedHours REAL NOT NULL DEFAULT 8.0,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    -- Manufacturing Details
    ProcessType NVARCHAR(100) NOT NULL DEFAULT 'SLS Metal',
    RequiredMachineType NVARCHAR(100) NOT NULL DEFAULT 'TruPrint 3000',
    PreferredMachines NVARCHAR(200) NOT NULL DEFAULT 'TI1,TI2',
    ProcessParameters TEXT NOT NULL DEFAULT '{}',
    QualityCheckpoints TEXT NOT NULL DEFAULT '{}',
    -- Classification
    PartCategory NVARCHAR(100) NOT NULL DEFAULT 'Component',
    PartClass NVARCHAR(100) NOT NULL DEFAULT 'Standard',
    Industry NVARCHAR(100) NOT NULL DEFAULT 'Aerospace',
    Application NVARCHAR(200) NOT NULL DEFAULT 'General Manufacturing',
    -- Dimensions & Weight
    LengthMm REAL NOT NULL DEFAULT 0,
    WidthMm REAL NOT NULL DEFAULT 0,
    HeightMm REAL NOT NULL DEFAULT 0,
    WeightGrams REAL NOT NULL DEFAULT 0,
    VolumeFromCad REAL NOT NULL DEFAULT 0,
    VolumeFromMeasurement REAL NOT NULL DEFAULT 0,
    SurfaceAreaSqMm REAL NOT NULL DEFAULT 0,
    -- Cost Analysis
    MaterialCostPerKg DECIMAL(12,2) NOT NULL DEFAULT 500.00,
    MaterialUsageKg REAL NOT NULL DEFAULT 0.5,
    StandardLaborCostPerHour DECIMAL(10,2) NOT NULL DEFAULT 50.00,
    EstimatedLaborHours REAL NOT NULL DEFAULT 1.0,
    SetupCost DECIMAL(10,2) NOT NULL DEFAULT 100.00,
    PostProcessingCost DECIMAL(10,2) NOT NULL DEFAULT 50.00,
    QualityControlCost DECIMAL(10,2) NOT NULL DEFAULT 25.00,
    PackagingCost DECIMAL(10,2) NOT NULL DEFAULT 10.00,
    -- Complexity Metrics
    ComplexityScore INTEGER NOT NULL DEFAULT 3,
    GeometricComplexity INTEGER NOT NULL DEFAULT 3,
    ManufacturingComplexity INTEGER NOT NULL DEFAULT 3,
    AssemblyComplexity INTEGER NOT NULL DEFAULT 1,
    -- Timestamps & Audit
    CreatedDate DATETIME NOT NULL DEFAULT (datetime('now')),
    LastModifiedDate DATETIME NOT NULL DEFAULT (datetime('now')),
    CreatedBy NVARCHAR(100) NOT NULL DEFAULT 'System',
    LastModifiedBy NVARCHAR(100) NOT NULL DEFAULT 'System'
);
```

**Key Features:**
- **Comprehensive Specifications**: Complete part definition with SLS parameters
- **Cost Management**: Detailed cost breakdown for accurate pricing
- **Classification System**: Industry and application categorization
- **Complexity Scoring**: Manufacturing difficulty assessment
- **CAD Integration**: Volume and surface area tracking

**Indexes:**
```sql
CREATE UNIQUE INDEX IX_Parts_PartNumber ON Parts(PartNumber);
CREATE INDEX IX_Parts_Material ON Parts(Material);
CREATE INDEX IX_Parts_SlsMaterial ON Parts(SlsMaterial);
CREATE INDEX IX_Parts_IsActive ON Parts(IsActive);
CREATE INDEX IX_Parts_PartCategory ON Parts(PartCategory);
CREATE INDEX IX_Parts_PartClass ON Parts(PartClass);
CREATE INDEX IX_Parts_Industry ON Parts(Industry);
CREATE INDEX IX_Parts_ProcessType ON Parts(ProcessType);
CREATE INDEX IX_Parts_RequiredMachineType ON Parts(RequiredMachineType);
```

### **3. Users Table** - `Users`
*User authentication and profile management*

```sql
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    Department NVARCHAR(100) NULL,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    LastLoginDate DATETIME NULL,
    FailedLoginAttempts INTEGER NOT NULL DEFAULT 0,
    AccountLockedUntil DATETIME NULL,
    -- Audit Trail
    CreatedDate DATETIME NOT NULL DEFAULT (datetime('now')),
    LastModifiedDate DATETIME NOT NULL DEFAULT (datetime('now')),
    CreatedBy NVARCHAR(50) NOT NULL DEFAULT 'System',
    LastModifiedBy NVARCHAR(50) NOT NULL DEFAULT 'System'
);
```

**Supported Roles:**
- **Admin**: Full system access
- **Manager**: Scheduling oversight and management
- **Scheduler**: Job scheduling and coordination
- **Operator**: Machine operation and job execution
- **PrintingSpecialist**: SLS machine operation
- **CoatingSpecialist**: Post-processing coating
- **EDMSpecialist**: Electrical discharge machining
- **MachiningSpecialist**: Traditional machining
- **QCSpecialist**: Quality control and inspection
- **ShippingSpecialist**: Order fulfillment
- **MediaSpecialist**: Digital media management
- **Analyst**: Performance analytics and reporting

**Indexes:**
```sql
CREATE UNIQUE INDEX IX_Users_Username ON Users(Username);
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Role ON Users(Role);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
```

### **4. UserSettings Table** - `UserSettings`
*User preferences and customization*

```sql
CREATE TABLE UserSettings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER UNIQUE NOT NULL,
    SessionTimeoutMinutes INTEGER NOT NULL DEFAULT 120,
    Theme NVARCHAR(20) NOT NULL DEFAULT 'Light',
    EmailNotifications BOOLEAN NOT NULL DEFAULT 1,
    BrowserNotifications BOOLEAN NOT NULL DEFAULT 1,
    DefaultPage NVARCHAR(100) NOT NULL DEFAULT '/Scheduler',
    ItemsPerPage INTEGER NOT NULL DEFAULT 20,
    TimeZone NVARCHAR(50) NOT NULL DEFAULT 'UTC',
    CreatedDate DATETIME NOT NULL DEFAULT (datetime('now')),
    LastModifiedDate DATETIME NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

### **5. JobLogEntries Table** - `JobLogEntries`
*Comprehensive audit trail for all job operations*

```sql
CREATE TABLE JobLogEntries (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    JobId INTEGER NULL,
    MachineId NVARCHAR(50) NOT NULL,
    PartNumber NVARCHAR(100) NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    Details TEXT NOT NULL,
    Operator NVARCHAR(100) NOT NULL,
    Timestamp DATETIME NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (JobId) REFERENCES Jobs(Id) ON DELETE SET NULL
);
```

**Tracked Actions:**
- **Created**: Job creation with initial parameters
- **Updated**: Any job modification with change details
- **Started**: Job execution start with operator
- **Completed**: Job completion with results
- **Cancelled**: Job cancellation with reason
- **Delayed**: Production delays with cause
- **QualityCheck**: Quality control checkpoints

**Indexes:**
```sql
CREATE INDEX IX_JobLogEntries_MachineId ON JobLogEntries(MachineId);
CREATE INDEX IX_JobLogEntries_Timestamp ON JobLogEntries(Timestamp);
CREATE INDEX IX_JobLogEntries_Action ON JobLogEntries(Action);
```

## ?? SLS Machine Management

### **6. SlsMachines Table** - `SlsMachines`
*SLS machine configuration and status tracking*

```sql
CREATE TABLE SlsMachines (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MachineId NVARCHAR(50) UNIQUE NOT NULL,
    MachineName NVARCHAR(100) NOT NULL,
    MachineModel NVARCHAR(100) NOT NULL DEFAULT 'TruPrint 3000',
    SerialNumber NVARCHAR(50) NOT NULL,
    Location NVARCHAR(100) NOT NULL,
    -- Build Volume Specifications
    BuildLengthMm REAL NOT NULL DEFAULT 250,
    BuildWidthMm REAL NOT NULL DEFAULT 250,
    BuildHeightMm REAL NOT NULL DEFAULT 300,
    -- Material Capabilities
    SupportedMaterials NVARCHAR(500) NOT NULL DEFAULT 'Ti-6Al-4V Grade 5,Ti-6Al-4V ELI Grade 23',
    CurrentMaterial NVARCHAR(100) NOT NULL,
    -- Process Capabilities
    MaxLaserPowerWatts REAL NOT NULL DEFAULT 400,
    MaxScanSpeedMmPerSec REAL NOT NULL DEFAULT 7000,
    MinLayerThicknessMicrons REAL NOT NULL DEFAULT 20,
    MaxLayerThicknessMicrons REAL NOT NULL DEFAULT 60,
    -- Current Status
    Status NVARCHAR(50) NOT NULL DEFAULT 'Offline',
    StatusMessage NVARCHAR(500) NOT NULL,
    CurrentJobId INTEGER NULL,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    IsAvailableForScheduling BOOLEAN NOT NULL DEFAULT 1,
    LastStatusUpdate DATETIME NOT NULL DEFAULT (datetime('now')),
    -- OPC UA Integration
    OpcUaEndpointUrl NVARCHAR(200) NULL,
    OpcUaUsername NVARCHAR(100) NULL,
    OpcUaPasswordHash NVARCHAR(100) NULL,
    OpcUaNamespace NVARCHAR(100) NULL,
    OpcUaConnectionStatus NVARCHAR(500) NOT NULL DEFAULT 'Disconnected',
    -- Maintenance & Quality
    MaintenanceIntervalHours REAL NOT NULL DEFAULT 500,
    NextMaintenanceDate DATETIME NULL,
    MaintenanceNotes NVARCHAR(1000) NOT NULL,
    QualityScorePercent REAL NOT NULL DEFAULT 100,
    OperatorNotes NVARCHAR(1000) NOT NULL,
    -- Audit
    CreatedDate DATETIME NOT NULL DEFAULT (datetime('now')),
    LastModifiedDate DATETIME NOT NULL DEFAULT (datetime('now')),
    CreatedBy NVARCHAR(100) NOT NULL DEFAULT 'System',
    LastModifiedBy NVARCHAR(100) NOT NULL DEFAULT 'System',
    FOREIGN KEY (CurrentJobId) REFERENCES Jobs(Id) ON DELETE SET NULL
);
```

### **7. MachineDataSnapshots Table** - `MachineDataSnapshots`
*Real-time machine data collection*

```sql
CREATE TABLE MachineDataSnapshots (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SlsMachineId INTEGER NOT NULL,
    MachineId NVARCHAR(50) NOT NULL,
    Timestamp DATETIME NOT NULL DEFAULT (datetime('now')),
    -- Process Data (JSON format)
    ProcessDataJson NVARCHAR(2000) NOT NULL DEFAULT '{}',
    QualityDataJson NVARCHAR(2000) NOT NULL DEFAULT '{}',
    AlarmDataJson NVARCHAR(2000) NOT NULL DEFAULT '{}',
    FOREIGN KEY (SlsMachineId) REFERENCES SlsMachines(Id) ON DELETE CASCADE
);
```

**JSON Data Examples:**
```json
// ProcessDataJson
{
  "laserPower": 285.5,
  "scanSpeed": 1150,
  "buildTemperature": 182.3,
  "chamberPressure": 1.2,
  "argonFlow": 15.8,
  "oxygenLevel": 42
}

// QualityDataJson
{
  "layerThickness": 29.8,
  "powderSpread": "good",
  "beamProfile": "within_spec",
  "surfaceQuality": 95.2
}

// AlarmDataJson
{
  "activeAlarms": [],
  "warningCount": 1,
  "lastAlarmClear": "2024-12-20T14:30:00Z"
}
```

## ?? Print Tracking System

### **8. BuildJobs Table** - `BuildJobs`
*Actual print operation tracking*

```sql
CREATE TABLE BuildJobs (
    BuildId INTEGER PRIMARY KEY AUTOINCREMENT,
    PrinterName NVARCHAR(10) NOT NULL,
    UserId INTEGER NOT NULL,
    ActualStartTime DATETIME NOT NULL,
    ActualEndTime DATETIME NULL,
    LaserRunTime NVARCHAR(50) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'In Progress',
    ReasonForEnd NVARCHAR(50) NULL,
    Notes NVARCHAR(1000) NULL,
    SetupNotes NVARCHAR(1000) NULL,
    AssociatedScheduledJobId INTEGER NULL,
    CreatedAt DATETIME NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    FOREIGN KEY (AssociatedScheduledJobId) REFERENCES Jobs(Id) ON DELETE SET NULL
);
```

### **9. BuildJobParts Table** - `BuildJobParts`
*Parts included in actual build jobs*

```sql
CREATE TABLE BuildJobParts (
    PartEntryId INTEGER PRIMARY KEY AUTOINCREMENT,
    BuildId INTEGER NOT NULL,
    PartNumber NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NULL,
    Quantity INTEGER NOT NULL,
    Material NVARCHAR(100) NULL,
    IsPrimary BOOLEAN NOT NULL DEFAULT 0,
    CreatedBy NVARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (BuildId) REFERENCES BuildJobs(BuildId) ON DELETE CASCADE
);
```

### **10. DelayLogs Table** - `DelayLogs`
*Production delay tracking and analysis*

```sql
CREATE TABLE DelayLogs (
    DelayId INTEGER PRIMARY KEY AUTOINCREMENT,
    BuildId INTEGER NOT NULL,
    DelayReason NVARCHAR(100) NOT NULL,
    DelayDuration INTEGER NOT NULL,
    Description NVARCHAR(1000) NULL,
    CreatedBy NVARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (BuildId) REFERENCES BuildJobs(BuildId) ON DELETE CASCADE
);
```

**Common Delay Reasons:**
- **Material Issue**: Powder quality or availability
- **Machine Maintenance**: Unscheduled maintenance
- **Power Outage**: Electrical supply interruption
- **Operator Delay**: Staff availability issues
- **Quality Hold**: Quality control hold
- **Equipment Failure**: Machine malfunction
- **Setup Time**: Extended setup requirements

## ?? Department Operations (Future Tables)

### **Future Implementation - Department-Specific Operations**

#### **CoatingOperations Table**
```sql
-- Post-processing coating operations
CREATE TABLE CoatingOperations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PartNumber NVARCHAR(50) NOT NULL,
    CoatingType NVARCHAR(100) NOT NULL,
    CoatingSpecification NVARCHAR(50) NOT NULL,
    Color NVARCHAR(50) NOT NULL,
    Thickness NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    ScheduledDate DATETIME NOT NULL,
    -- Process parameters, costs, quality tracking
);
```

#### **EDMOperations Table**
```sql
-- Electrical discharge machining operations
CREATE TABLE EDMOperations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PartNumber NVARCHAR(50) NOT NULL,
    EDMType NVARCHAR(50) NOT NULL,
    MachineId NVARCHAR(50) NOT NULL,
    Material NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Setup',
    -- Wire type, dielectric fluid, process parameters
);
```

#### **MachiningOperations Table**
```sql
-- Traditional machining operations
CREATE TABLE MachiningOperations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PartNumber NVARCHAR(50) NOT NULL,
    WorkOrderNumber NVARCHAR(100) NOT NULL,
    MachineId NVARCHAR(50) NOT NULL,
    Operation NVARCHAR(100) NOT NULL,
    -- Tooling, programs, setup instructions
);
```

#### **QualityInspections Table**
```sql
-- Quality control and inspection operations
CREATE TABLE QualityInspections (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    InspectionNumber NVARCHAR(50) UNIQUE NOT NULL,
    PartNumber NVARCHAR(50) NOT NULL,
    InspectionType NVARCHAR(100) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    -- Dimensional, visual, functional requirements and results
);
```

#### **ShippingOperations Table**
```sql
-- Order fulfillment and shipping
CREATE TABLE ShippingOperations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ShipmentNumber NVARCHAR(50) UNIQUE NOT NULL,
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerOrderNumber NVARCHAR(100) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    -- Address, shipping method, tracking, packaging
);
```

## ??? Migration Management

### **Migration History**
```sql
-- EF Core Migration History
CREATE TABLE __EFMigrationsHistory (
    MigrationId NVARCHAR(150) NOT NULL PRIMARY KEY,
    ProductVersion NVARCHAR(32) NOT NULL
);
```

### **Current Migrations**
1. **InitialCreate** (`20250721164727_InitialCreate`)
   - Core tables: Jobs, Parts, Users, UserSettings
   - Basic SLS machine support
   - Initial audit logging

2. **AddJobLogEntry** (`20250722031833_AddJobLogEntry`)
   - Enhanced audit trail system
   - Job log entry relationships
   - Improved change tracking

3. **EnhancedJobAnalytics** (`20250722211248_EnhancedJobAnalytics`)
   - Advanced analytics fields
   - Quality metrics tracking
   - Cost calculation enhancements
   - Performance optimization indexes

### **Migration Commands**
```bash
# Create new migration
dotnet ef migrations add MigrationName --project OpCentrix

# Update database
dotnet ef database update --project OpCentrix

# Rollback to specific migration
dotnet ef database update PreviousMigrationName --project OpCentrix

# Generate SQL script
dotnet ef migrations script --project OpCentrix
```

## ?? Query Optimization

### **Performance Indexes**

#### **High-Traffic Query Indexes**
```sql
-- Scheduler timeline queries
CREATE INDEX IX_Jobs_MachineId_ScheduledStart ON Jobs(MachineId, ScheduledStart);

-- Admin dashboard queries
CREATE INDEX IX_Jobs_Status ON Jobs(Status);
CREATE INDEX IX_Parts_IsActive ON Parts(IsActive);

-- Audit trail queries
CREATE INDEX IX_JobLogEntries_Timestamp ON JobLogEntries(Timestamp);
CREATE INDEX IX_JobLogEntries_Action ON JobLogEntries(Action);

-- User authentication
CREATE UNIQUE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Role ON Users(Role);

-- Machine monitoring
CREATE INDEX IX_SlsMachines_Status ON SlsMachines(Status);
CREATE INDEX IX_MachineDataSnapshots_MachineId_Timestamp ON MachineDataSnapshots(MachineId, Timestamp);
```

#### **Search and Filter Indexes**
```sql
-- Part search functionality
CREATE INDEX IX_Parts_PartNumber ON Parts(PartNumber);
CREATE INDEX IX_Parts_Material ON Parts(Material);
CREATE INDEX IX_Parts_SlsMaterial ON Parts(SlsMaterial);

-- Job filtering
CREATE INDEX IX_Jobs_PartNumber ON Jobs(PartNumber);
CREATE INDEX IX_Jobs_Priority ON Jobs(Priority);
CREATE INDEX IX_Jobs_SlsMaterial ON Jobs(SlsMaterial);

-- Print tracking
CREATE INDEX IX_BuildJobs_PrinterName_ActualStartTime ON BuildJobs(PrinterName, ActualStartTime);
CREATE INDEX IX_BuildJobParts_BuildId ON BuildJobParts(BuildId);
```

### **Optimized Query Patterns**

#### **Scheduler Data Loading**
```sql
-- Load jobs for specific date range and machines
SELECT j.*, p.Description as PartDescription, p.Material
FROM Jobs j
INNER JOIN Parts p ON j.PartId = p.Id
WHERE j.MachineId IN ('TI1', 'TI2', 'INC')
  AND j.ScheduledStart >= @StartDate
  AND j.ScheduledStart <= @EndDate
ORDER BY j.MachineId, j.ScheduledStart;
```

#### **Conflict Detection Query**
```sql
-- Check for overlapping jobs on same machine
SELECT COUNT(*)
FROM Jobs
WHERE MachineId = @MachineId
  AND Id != @ExcludeJobId
  AND ((ScheduledStart < @JobEnd AND ScheduledEnd > @JobStart));
```

#### **Dashboard Analytics**
```sql
-- Real-time statistics for admin dashboard
SELECT 
    COUNT(*) as TotalJobs,
    COUNT(CASE WHEN Status = 'Active' THEN 1 END) as ActiveJobs,
    COUNT(CASE WHEN Status = 'Completed' THEN 1 END) as CompletedJobs,
    AVG(CASE WHEN ActualEnd IS NOT NULL 
        THEN (julianday(ActualEnd) - julianday(ActualStart)) * 24 
        END) as AvgJobHours
FROM Jobs
WHERE ScheduledStart >= date('now', '-30 days');
```

## ?? Data Relationships

### **Entity Relationships**

#### **Core Scheduling Relationships**
```
Users (1) ?? (1) UserSettings
Users (1) ?? (?) Jobs (CreatedBy/ModifiedBy)
Parts (1) ?? (?) Jobs
Jobs (1) ?? (?) JobLogEntries
```

#### **Machine Management Relationships**
```
SlsMachines (1) ?? (0..1) Jobs (CurrentJob)
SlsMachines (1) ?? (?) MachineDataSnapshots
```

#### **Print Tracking Relationships**
```
Users (1) ?? (?) BuildJobs
BuildJobs (1) ?? (?) BuildJobParts
BuildJobs (1) ?? (?) DelayLogs
Jobs (0..1) ?? (?) BuildJobs (AssociatedScheduledJobId)
```

### **Cascade Delete Policies**

#### **Restrict Deletes (Data Protection)**
```sql
-- Cannot delete Parts if referenced by Jobs
FOREIGN KEY (PartId) REFERENCES Parts(Id) ON DELETE RESTRICT

-- Cannot delete Users if they created/modified Jobs
FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE RESTRICT
```

#### **Cascade Deletes (Data Cleanup)**
```sql
-- Delete UserSettings when User is deleted
FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE

-- Delete BuildJobParts when BuildJob is deleted
FOREIGN KEY (BuildId) REFERENCES BuildJobs(BuildId) ON DELETE CASCADE

-- Delete DelayLogs when BuildJob is deleted
FOREIGN KEY (BuildId) REFERENCES BuildJobs(BuildId) ON DELETE CASCADE
```

#### **Set Null (Reference Cleanup)**
```sql
-- Clear machine's current job when job is deleted
FOREIGN KEY (CurrentJobId) REFERENCES Jobs(Id) ON DELETE SET NULL

-- Preserve log entries when job is deleted
FOREIGN KEY (JobId) REFERENCES Jobs(Id) ON DELETE SET NULL
```

## ?? Database Maintenance

### **Backup Strategy**
```sql
-- SQLite backup (development)
.backup main backup_filename.db

-- SQL Server backup (production)
BACKUP DATABASE OpCentrix 
TO DISK = 'C:\Backups\OpCentrix_backup.bak'
WITH COMPRESSION, CHECKSUM;
```

### **Performance Monitoring**
```sql
-- Analyze query performance
EXPLAIN QUERY PLAN 
SELECT * FROM Jobs 
WHERE MachineId = 'TI1' 
  AND ScheduledStart >= '2024-12-01';

-- Check index usage
.schema Jobs  -- Shows all indexes on Jobs table
```

### **Data Archival Strategy**
```sql
-- Archive completed jobs older than 1 year
INSERT INTO JobsArchive 
SELECT * FROM Jobs 
WHERE Status = 'Completed' 
  AND ScheduledEnd < date('now', '-1 year');

-- Archive machine data snapshots older than 6 months
DELETE FROM MachineDataSnapshots 
WHERE Timestamp < date('now', '-6 months');
```

## ?? Scaling Considerations

### **Horizontal Scaling**
- **Read Replicas**: Configure read-only replicas for reporting
- **Partitioning**: Partition by date ranges for large datasets
- **Caching**: Implement Redis for frequently accessed data
- **Connection Pooling**: Optimize connection management

### **Database Migration Path**
```
Development: SQLite
     ?
Testing: SQL Server Express
     ?
Production: SQL Server Standard/Enterprise
     ?
Enterprise: SQL Server with Always On
```

### **Performance Benchmarks**
- **Jobs Table**: Efficient up to 1M+ records with proper indexing
- **MachineDataSnapshots**: Archive strategy required for high-frequency data
- **Query Performance**: Sub-second response for scheduler queries
- **Concurrent Users**: 50+ concurrent users supported

---

This database structure provides a robust foundation for SLS manufacturing operations with room for growth and adaptation to changing business requirements.

---

*Last Updated: December 2024*  
*Version: 2.0.0*