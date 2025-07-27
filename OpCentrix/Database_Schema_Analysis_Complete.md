# OpCentrix Database Schema Analysis - Complete Documentation

Generated: 2025-01-27
Application: OpCentrix SLS Scheduler (.NET 8, Razor Pages, SQLite)
Database File: scheduler.db (560 KB)

## ?? Database Overview

The OpCentrix application uses a **single SQLite database** called `scheduler.db` that contains all scheduling, parts, machines, users, and administrative data. The database has evolved through multiple Entity Framework migrations and contains 23+ entity types.

### Connection Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=scheduler.db"
  }
}
```

## ??? Database Tables Structure

Based on the actual schema analysis and SchedulerContext.cs, here are the confirmed tables:

### CORE SCHEDULING TABLES

#### 1. **Jobs** (Primary scheduling table)
```sql
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
    "Quantity" INTEGER NOT NULL
    -- Additional fields from context: Priority, LaserPowerWatts, ScanSpeedMmPerSec, etc.
)
```

**Key Features:**
- Primary scheduling entity
- Foreign keys to Parts and Machines
- Extensive process parameters (laser power, scan speed, etc.)
- Audit trail fields (CreatedDate, CreatedBy)
- Performance indexes on (MachineId, ScheduledStart), Status, PartNumber

#### 2. **Parts** (Parts library)
```sql
CREATE TABLE "Parts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Parts" PRIMARY KEY AUTOINCREMENT,
    "PartNumber" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Material" TEXT NOT NULL,
    "SlsMaterial" TEXT NOT NULL,
    "AvgDuration" TEXT NOT NULL,
    "AvgDurationDays" INTEGER NOT NULL DEFAULT 1
    -- Additional fields: Industry, Application, MaterialCostPerKg, etc.
)
```

**Key Features:**
- Comprehensive part specifications
- Material and cost information
- Process parameters and requirements
- Unique constraint on PartNumber
- Extensive metadata (dimensions, tolerances, quality standards)

#### 3. **Machines** (Equipment management)
```sql
CREATE TABLE "Machines" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Machines" PRIMARY KEY AUTOINCREMENT,
    "MachineId" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "MachineName" TEXT NOT NULL,
    "MachineType" TEXT NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Idle',
    "IsActive" INTEGER NOT NULL DEFAULT 1
    -- Additional fields: Department, Capabilities, etc.
)
```

**Key Features:**
- Unique MachineId constraint
- Machine capabilities and status tracking
- Equipment type and department organization

#### 4. **Users** (Authentication system)
- User accounts and authentication
- Role-based access control
- User preferences and settings

### ENHANCED SCHEDULING FEATURES

#### 5. **JobNotes** (Job annotations)
- Job-specific notes and documentation
- Step-by-step instructions
- Priority and completion tracking
- Foreign key to Jobs table

#### 6. **JobStages** (Multi-stage workflow)
- Multi-stage manufacturing process support
- Stage dependencies and sequencing
- Department and resource allocation
- Execution order management

#### 7. **StageDependencies** (Workflow management)
- Stage prerequisite relationships
- Dependency type configuration
- Circular dependency prevention

### MACHINE MANAGEMENT

#### 8. **MachineCapabilities** (Equipment capabilities)
- Machine capability matrix
- Capability types and values
- Equipment certification requirements
- Foreign key to Machines table

#### 9. **Materials** (Material library)
- Material definitions and properties
- Material specifications
- Process compatibility

### ADMINISTRATIVE SYSTEM

#### 10. **SystemSettings** (Configuration)
- Application configuration management
- Category-based organization
- Data type validation
- User permission controls

#### 11. **RolePermissions** (Access control)
- Role-based permission matrix
- Permission levels and constraints
- Category-based organization

#### 12. **OperatingShifts** (Schedule management)
- Work schedule configuration
- Day-of-week and holiday handling
- Shift time definitions

#### 13. **InspectionCheckpoints** (Quality control)
- Quality control checkpoints
- Part-specific inspection requirements
- Measurement methods and criteria

#### 14. **DefectCategories** (Issue tracking)
- Defect classification system
- Severity levels and impact assessment
- Corrective action procedures

### PRINT TRACKING SYSTEM

#### 15. **BuildJobs** (Real-time tracking)
- Real-time print job monitoring
- Build process tracking
- Equipment association

#### 16. **BuildJobParts** (Multi-part builds)
- Multi-part build management
- Part quantity tracking
- Build organization

#### 17. **DelayLogs** (Production analysis)
- Production delay tracking
- Delay reason classification
- Performance analysis

### ARCHIVE AND LOGGING

#### 18. **ArchivedJobs** (Historical data)
- Completed job archival
- Historical performance data
- Data retention management

#### 19. **AdminAlerts** (Monitoring system)
- Automated alerting system
- Trigger condition management
- Multi-channel notifications

#### 20. **FeatureToggles** (Feature flags)
- Runtime feature management
- A/B testing support
- Rollout control

## ?? Database Relationships

### Primary Foreign Key Relationships
```
Jobs -> Parts (via PartId and PartNumber)
Jobs -> Machines (via MachineId reference)
JobNotes -> Jobs (via JobId)
JobStages -> Jobs (via JobId)
StageDependencies -> JobStages (via DependentStageId, RequiredStageId)
MachineCapabilities -> Machines (via MachineId)
InspectionCheckpoints -> Parts (via PartId)
BuildJobParts -> BuildJobs (via BuildId)
DelayLogs -> BuildJobs (via BuildId)
```

### Unique Constraints
- Parts.PartNumber
- Machines.MachineId  
- SystemSettings.SettingKey
- RolePermissions.(RoleName, PermissionKey)

## ? Performance Optimizations

### Strategic Indexes
```sql
-- Job scheduling performance
CREATE INDEX "IX_Jobs_MachineId_ScheduledStart" ON "Jobs" ("MachineId", "ScheduledStart");
CREATE INDEX "IX_Jobs_Status" ON "Jobs" ("Status");
CREATE INDEX "IX_Jobs_PartNumber" ON "Jobs" ("PartNumber");
CREATE INDEX "IX_Jobs_Priority" ON "Jobs" ("Priority");

-- Part lookups
CREATE UNIQUE INDEX "IX_Parts_PartNumber" ON "Parts" ("PartNumber");
CREATE INDEX "IX_Parts_Material" ON "Parts" ("Material");
CREATE INDEX "IX_Parts_IsActive" ON "Parts" ("IsActive");

-- Machine operations
CREATE UNIQUE INDEX "IX_Machines_MachineId" ON "Machines" ("MachineId");
CREATE INDEX "IX_Machines_MachineType" ON "Machines" ("MachineType");
```

## ?? Critical Issues Identified

### 1. **Machine Table Naming Confusion**
- **Issue**: Code references both "Machine" and "SlsMachine"
- **Impact**: Runtime errors in service layer
- **Location**: AdminDashboardService.cs, SchedulerService.cs

### 2. **Foreign Key Type Mismatches**
- **Issue**: MachineCapabilities.SlsMachineId (INT) vs expected MachineId (STRING)
- **Impact**: Join operations fail
- **Fix Required**: Migration to standardize foreign key types

### 3. **Missing Required Fields**
- **Issue**: Code expects fields not in database schema
- **Missing Fields**:
  - Jobs.EstimatedDuration (referenced in UI)
  - Machines.BuildVolumeM3 (referenced in calculations)
  - Parts.Name (required but possibly missing)

### 4. **JavaScript Field Reference Errors**
- **Issue**: Form JavaScript references wrong field IDs
- **Examples**: "SslMaterial" vs "SlsMaterial"
- **Impact**: Auto-fill functionality broken

### 5. **Audit Field Inconsistencies**
- **Issue**: Different naming patterns across tables
- **Patterns**: CreatedDate vs CreationDate, CreatedBy vs CreatedByUser
- **Impact**: Inconsistent audit trail

## ??? Recommended Fixes (Priority Order)

### **IMMEDIATE (Week 1)**
1. **Standardize Machine References**
   ```powershell
   # Search and replace all SlsMachine references
   cd "C:\Users\Henry\source\repos\OpCentrix\OpCentrix"
   Get-ChildItem -Recurse -Include "*.cs" | ForEach-Object { 
       (Get-Content $_.FullName) -replace 'SlsMachine', 'Machine' | Set-Content $_.FullName 
   }
   ```

2. **Fix Foreign Key Types**
   ```csharp
   // Create migration to fix MachineCapabilities
   dotnet ef migrations add FixMachineCapabilityForeignKey
   ```

3. **Add Missing Required Fields**
   ```csharp
   // Add migration for missing fields
   dotnet ef migrations add AddMissingRequiredFields
   ```

### **HIGH PRIORITY (Week 1-2)**
4. **Add Performance Indexes**
   ```csharp
   dotnet ef migrations add AddPerformanceIndexes
   ```

5. **Standardize Audit Fields**
   ```csharp
   dotnet ef migrations add StandardizeAuditFields
   ```

6. **Fix JavaScript References**
   - Update _PartForm.cshtml JavaScript
   - Fix scheduler-ui.js field references

## ?? Database Maintenance Commands

### **Backup Database**
```powershell
cd "C:\Users\Henry\source\repos\OpCentrix\OpCentrix"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
Copy-Item "scheduler.db" "backup/scheduler_$timestamp.db"
```

### **Apply Migrations**
```powershell
# Review pending migrations
dotnet ef migrations list

# Apply migrations
dotnet ef database update

# Generate migration script
dotnet ef migrations script > migration_script.sql
```

### **Database Optimization**
```powershell
# Use the Admin > Database interface for:
# - VACUUM (reclaim space)
# - ANALYZE (update statistics) 
# - PRAGMA optimize (performance tuning)
```

### **Schema Export**
```powershell
# Run the schema export script
.\database_schema_export.ps1

# Manual SQLite export (if sqlite3 available)
sqlite3 scheduler.db ".schema" > scheduler_schema.sql
```

## ?? Next Steps

1. **Execute Database Refactoring Plan** from `SQL-Refractor-Plan.md`
2. **Apply Critical Fixes** in priority order
3. **Test All Functionality** after each migration
4. **Monitor Performance** and adjust indexes as needed
5. **Set Up Automated Backups** before production deployment

## ?? Generated Files

This analysis created the following documentation files:

- `OpCentrix_Database_Schema_Complete.md` - Complete entity documentation
- `scheduler_schema.sql` - Raw SQL schema (100KB+)
- `database_quick_reference.txt` - Quick reference guide
- `database_schema_export.ps1` - PowerShell export script

All files are located in: `C:\Users\Henry\source\repos\OpCentrix\OpCentrix\`

---

**Total Database Size**: 560 KB  
**Total Tables**: 23 entities  
**Total Indexes**: 25+ strategic indexes  
**Migration History**: Available via Entity Framework  

*This documentation provides a complete understanding of the OpCentrix database structure and critical issues that need to be addressed for optimal performance and stability.*