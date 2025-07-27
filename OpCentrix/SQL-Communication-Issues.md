================================================================
Generated: 2024-12-20
Category: Database Schema vs Application Mismatch Analysis

CRITICAL DATABASE ISSUES FOUND
==============================

1. Part Number Field Naming Inconsistency
-----------------------------------------
Issue: Parts page form uses "SlsMaterial" but validation function references "SslMaterial"
Location: OpCentrix/Pages/Admin/Shared/_PartForm.cshtml
SQL Table: Parts.SlsMaterial (VARCHAR)
Form Field: asp-for="SlsMaterial" 
JavaScript: Incorrectly references "SslMaterial" in some places
Impact: Material selection auto-fill may fail

2. Machine Table Naming Discrepancy
-----------------------------------
Issue: Database uses "Machines" table but code references "SlsMachines"
SQL Table: Machines (not SlsMachines)
Code References: 
- AdminDashboardService.cs: _context.SlsMachines.CountAsync()
- Various pages expecting SlsMachine entity
Impact: Runtime errors when accessing machine data

3. Missing Database Fields Referenced in Code
---------------------------------------------
Issue: Code expects fields that don't exist in database
Missing Fields:
- Jobs.EstimatedDuration (code expects this, DB only has Part.EstimatedHours)
- Jobs.EffectiveDurationHours (calculated field not in DB)
- Machine.BuildVolumeM3 (expected but not in DB schema)
- Machine.Name vs Machine.MachineName (inconsistent usage)

4. Foreign Key Relationship Issues
----------------------------------
Issue: MachineCapability uses SlsMachineId (int) but code expects MachineId (string)
SQL: MachineCapabilities.SlsMachineId (INT)
Code: Expecting MachineCapability.MachineId (STRING)
Impact: Machine capability operations will fail

5. User Table Field Mismatches
-------------------------------
Issue: User authentication fields don't match schema
SQL Table: Users has Username, PasswordHash
Code Expects: Some pages expect Email field that doesn't exist
Impact: User management pages may have issues

6. Job Status Enum vs String
----------------------------
Issue: Job.Status stored as string but treated as enum in some code
SQL: Jobs.Status (NVARCHAR)
Code: Sometimes expects JobStatus enum type
Impact: Status filtering and updates may fail

7. DateTime vs DateTime2 Precision
----------------------------------
Issue: SQL uses DATETIME2 but some code assumes DATETIME precision
Affected Fields:
- Jobs.ScheduledStart/ScheduledEnd
- All CreatedDate/LastModifiedDate fields
Impact: Millisecond precision loss in some operations

8. Nullable Field Handling
--------------------------
Issue: Many nullable DB fields not properly handled in code
Examples:
- Jobs.ActualStart/ActualEnd (nullable in DB, not always checked)
- Parts.AdminOverrideDuration (nullable but accessed directly)
- Machine.Department (nullable but displayed without null check)

9. Missing Indexes for Foreign Keys
------------------------------------
Issue: Foreign key columns lack indexes for performance
Missing Indexes:
- Jobs.PartNumber (no index, frequent lookups)
- Jobs.MachineId (no index, frequent filtering)
- MachineCapabilities.SlsMachineId
Impact: Slow query performance

10. Audit Field Inconsistencies
--------------------------------
Issue: Audit fields have different names across tables
Patterns Found:
- Some use CreatedDate/CreatedBy
- Others use CreationDate/CreatedByUser
- Some missing audit fields entirely
Impact: Audit trail functionality inconsistent

DETAILED NAMING MISMATCHES
==========================

Parts Table vs Parts Form
-------------------------
DB Column          | Form Field         | Issue
-------------------|-------------------|------------------
PartNumber         | PartNumber        | ✓ OK
Name               | Name              | ✓ OK
SlsMaterial        | SlsMaterial       | ⚠️ Sometimes "SslMaterial"
EstimatedHours     | EstimatedHours    | ✓ OK
MaterialCostPerUnit| MaterialCost      | ❌ Name mismatch

Jobs Table vs Scheduler Forms
-----------------------------
DB Column          | Form Field         | Issue
-------------------|-------------------|------------------
Id                 | Id                | ✓ OK
PartNumber         | PartNumber        | ✓ OK
MachineId          | MachineId         | ✓ OK
ScheduledStart     | StartTime         | ❌ Name mismatch
ScheduledEnd       | EndTime           | ❌ Name mismatch
Status             | Status            | ⚠️ String vs Enum

Machines Table vs Machine Management
------------------------------------
DB Column          | Code Expects      | Issue
-------------------|-------------------|------------------
MachineId          | MachineId         | ✓ OK
MachineName        | Name              | ❌ Property mismatch
MachineType        | MachineType       | ✓ OK
IsActive           | IsActive          | ✓ OK
BuildVolume        | BuildVolumeM3     | ❌ Property missing

DATABASE CONNECTION ISSUES
==========================

1. DbContext Configuration
--------------------------
Issue: SchedulerContext expects DbSet<SlsMachine> but table is "Machines"
Fix Needed: Change to DbSet<Machine> Machines

2. Connection String Issues
---------------------------
Issue: SQLite connection string path not consistent
Current: Various paths used (scheduler.db, opcentrix.db)
Impact: May connect to wrong database file

3. Transaction Scope Problems
-----------------------------
Issue: No transaction wrapping for multi-table operations
Example: Creating job + audit log should be atomic
Impact: Data integrity issues on failures

4. Lazy Loading Not Configured
------------------------------
Issue: Navigation properties not configured for lazy loading
Impact: N+1 query problems throughout application

5. Migration Out of Sync
------------------------
Issue: Code expects fields that migrations haven't created
Examples:
- Job.EstimatedDuration
- Machine.BuildVolumeM3
- Part.Name (might be missing)

FORM BINDING ISSUES
===================

1. Parts Form Material Selection
--------------------------------
Issue: updateSlsMaterial() expects different field IDs than rendered
Expected: materialSelect, slsMaterialInput
Actual: Material, SlsMaterial
Impact: Auto-fill doesn't work

2. Job Modal Duration Calculation
---------------------------------
Issue: Form uses Part.AvgDurationDays but should use EstimatedHours
Impact: Incorrect job duration calculations

3. DateTime Format Binding
--------------------------
Issue: Forms expect ISO format but DB returns different format
Impact: Date pickers may not populate correctly

4. Decimal vs Double Precision
------------------------------
Issue: Forms use double but DB stores decimal for costs
Impact: Precision loss in cost calculations

RECOMMENDED FIXES PRIORITY
==========================

CRITICAL (Fix Immediately)
--------------------------
1. Fix SlsMachine vs Machine references
2. Fix MachineCapability foreign key type
3. Add missing Job.EstimatedDuration migration
4. Fix Parts form material field references
5. Standardize DateTime handling to UTC

HIGH (Fix This Week)
--------------------
6. Add missing database indexes
7. Fix audit field naming consistency
8. Handle all nullable fields properly
9. Fix form field name bindings
10. Add transaction support

MEDIUM (Fix This Month)
-----------------------
11. Configure lazy loading properly
12. Standardize status enum handling
13. Fix decimal/double conversions
14. Add data validation attributes
15. Implement proper error handling

LOW (Future Improvements)
-------------------------
16. Add database views for complex queries
17. Implement stored procedures for reports
18. Add database triggers for audit
19. Optimize query performance
20. Add database documentation

MIGRATION SCRIPTS NEEDED
========================

1. Rename SlsMachines to Machines
---------------------------------
ALTER TABLE SlsMachines RENAME TO Machines;

2. Add Missing Fields
---------------------
ALTER TABLE Jobs ADD EstimatedDuration TIME;
ALTER TABLE Machines ADD BuildVolumeM3 DECIMAL(10,4);
ALTER TABLE Parts ADD Name NVARCHAR(100);

3. Fix Foreign Key Types
------------------------
-- Need to recreate MachineCapabilities table
-- with correct foreign key type

4. Add Missing Indexes
----------------------
CREATE INDEX IX_Jobs_PartNumber ON Jobs(PartNumber);
CREATE INDEX IX_Jobs_MachineId ON Jobs(MachineId);
CREATE INDEX IX_Jobs_Status ON Jobs(Status);

5. Standardize Audit Fields
---------------------------
-- Add CreatedDate/By where missing
-- Rename to consistent pattern

VALIDATION QUERIES
==================

-- Check for orphaned jobs
SELECT COUNT(*) FROM Jobs j
LEFT JOIN Parts p ON j.PartNumber = p.PartNumber
WHERE p.PartNumber IS NULL;

-- Check for invalid machine references
SELECT COUNT(*) FROM Jobs j
LEFT JOIN Machines m ON j.MachineId = m.MachineId
WHERE m.MachineId IS NULL;

-- Check for data type mismatches
SELECT * FROM PRAGMA_TABLE_INFO('Jobs');