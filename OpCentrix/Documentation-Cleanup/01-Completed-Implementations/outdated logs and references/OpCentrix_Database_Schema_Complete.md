OpCentrix Database Schema Documentation
====================================
Generated: 2025-07-27 08:34:11
Database: scheduler.db (Primary Database)
Size: 560 KB

OVERVIEW
========
The OpCentrix application uses a single SQLite database (scheduler.db) that contains
all the scheduling, parts, machines, users, and administrative data.

Connection String: "Data Source=scheduler.db"

ENTITY FRAMEWORK MODELS
========================
Based on the SchedulerContext.cs file, the following entities are configured:

CORE SCHEDULING TABLES
====================
1. Jobs - Manufacturing job scheduling
   - Primary key: Id (int)
   - Properties: PartNumber, MachineId, ScheduledStart, ScheduledEnd, Status, Priority
   - Indexes: (MachineId, ScheduledStart), Status, PartNumber, Priority

2. Parts - Part definitions and specifications
   - Primary key: Id (int)
   - Properties: PartNumber (unique), Name, Description, Material, SlsMaterial
   - Indexes: PartNumber (unique), Material, IsActive, Industry

3. Machines - Manufacturing equipment
   - Primary key: Id (int)
   - Properties: MachineId (unique), Name, MachineName, MachineType, Status
   - Indexes: MachineId (unique), MachineType, Status, IsActive

4. Users - User accounts and authentication
   - Primary key: Id (int)
   - Properties: Username, Email, PasswordHash, Role

5. UserSettings - User preferences
   - Primary key: Id (int)
   - Foreign key: UserId -> Users.Id

6. JobLogEntries - Job activity logging
   - Primary key: Id (int)
   - Foreign key: JobId -> Jobs.Id

ENHANCED SCHEDULING FEATURES
============================
7. JobNotes - Job-specific notes and annotations
   - Primary key: Id (int)
   - Foreign key: JobId -> Jobs.Id
   - Properties: Step, Note, NoteType, Priority

8. JobStages - Multi-stage manufacturing workflow
   - Primary key: Id (int)
   - Foreign key: JobId -> Jobs.Id
   - Properties: StageType, StageName, Department, MachineId

9. StageDependencies - Stage prerequisite management
   - Primary key: Id (int)
   - Foreign keys: DependentStageId, RequiredStageId -> JobStages.Id

10. StageNotes - Stage-specific documentation
    - Primary key: Id (int)
    - Foreign key: StageId -> JobStages.Id

MACHINE MANAGEMENT
==================
11. MachineCapabilities - Equipment capabilities matrix
    - Primary key: Id (int)
    - Foreign key: MachineId -> Machines.Id
    - Properties: CapabilityType, CapabilityName, CapabilityValue

12. Materials - Material definitions and properties
    - Primary key: Id (int)
    - Properties: MaterialName, Properties, Specifications

ADMINISTRATIVE SYSTEM
=====================
13. OperatingShifts - Work schedule configuration
    - Primary key: Id (int)
    - Properties: DayOfWeek, StartTime, EndTime, IsActive

14. SystemSettings - Application configuration
    - Primary key: Id (int)
    - Properties: SettingKey (unique), SettingValue, Category

15. RolePermissions - Role-based access control
    - Primary key: Id (int)
    - Properties: RoleName, PermissionKey, HasPermission

16. InspectionCheckpoints - Quality control points
    - Primary key: Id (int)
    - Foreign key: PartId -> Parts.Id

17. DefectCategories - Issue classification
    - Primary key: Id (int)
    - Properties: Name, Code, CategoryGroup, SeverityLevel

18. ArchivedJobs - Historical job data
    - Primary key: Id (int)
    - Properties: OriginalJobId, archived job data

19. AdminAlerts - Automated alerting system
    - Primary key: Id (int)
    - Properties: AlertName, Category, TriggerType, IsActive

20. FeatureToggles - Runtime feature flags
    - Primary key: Id (int)
    - Properties: FeatureName (unique), IsEnabled, Category

PRINT TRACKING SYSTEM
=====================
21. BuildJobs - Real-time print job tracking
    - Primary key: Id (int)
    - Properties: BuildId, PrinterName, AssociatedScheduledJobId

22. BuildJobParts - Multi-part build management
    - Primary key: Id (int)
    - Foreign key: BuildId -> BuildJobs.Id

23. DelayLogs - Production delay tracking
    - Primary key: DelayId (int)
    - Foreign key: BuildId -> BuildJobs.Id

PERFORMANCE OPTIMIZATIONS
=========================
The database includes strategic indexing for:
- Job scheduling queries: (MachineId, ScheduledStart)
- Status filtering: Status, Priority
- Part lookups: PartNumber, Material, IsActive
- User operations: Username, Role
- Time-based queries: date ranges, shifts

CRITICAL ISSUES IDENTIFIED
==========================
Based on the SQL refactoring analysis:

1. Machine Table Naming: Code expects "Machine" but may reference "SlsMachine"
2. Foreign Key Types: MachineCapabilities foreign key type mismatch
3. Missing Fields: Some fields referenced in code may not exist in DB
4. Audit Fields: Inconsistent naming patterns across tables

FOREIGN KEY RELATIONSHIPS
=========================
- Jobs -> Parts (via PartNumber or PartId)
- Jobs -> Machines (via MachineId)
- JobNotes -> Jobs (via JobId)
- JobStages -> Jobs (via JobId)
- StageDependencies -> JobStages (via DependentStageId, RequiredStageId)
- StageNotes -> JobStages (via StageId)
- MachineCapabilities -> Machines (via MachineId)
- InspectionCheckpoints -> Parts (via PartId)
- BuildJobParts -> BuildJobs (via BuildId)
- DelayLogs -> BuildJobs (via BuildId)

DATABASE CONSTRAINTS
====================
- Unique constraints: Parts.PartNumber, Machines.MachineId, SystemSettings.SettingKey
- Check constraints: StageDependency (no self-reference)
- Default values: Status, Priority, CreatedDate, IsActive fields
- Cascade deletes: JobNotes, JobStages, StageNotes on job deletion

RECOMMENDED MAINTENANCE
=======================
1. Run VACUUM periodically to reclaim space
2. Run ANALYZE to update query optimizer statistics
3. Monitor database size and performance
4. Backup before major operations
5. Apply database refactoring plan from SQL-Refractor-Plan.md

NEXT STEPS
==========
1. Review and apply the database refactoring plan
2. Add missing performance indexes
3. Standardize audit field naming
4. Implement missing required fields
5. Set up automated database maintenance

