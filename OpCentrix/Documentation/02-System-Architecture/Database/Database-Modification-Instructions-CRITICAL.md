# 🗄️ **OpCentrix Database Modification Instructions - CRITICAL PROTOCOL**

**Date**: January 2025  
**Version**: 1.1  
**Purpose**: Prevent database schema issues and ensure proper EF Core integration  
**Context**: Razor Pages .NET 8 project with SQLite database  

---

## 🚨 **MANDATORY PRE-FLIGHT CHECKLIST**

**BEFORE making ANY database changes, ALWAYS follow this exact protocol:**

### **Step 1: Environment Setup**
```powershell
# Verify you're in the right place
pwd  # Should show: C:\Users\Henry\source\repos\OpCentrix-MES\OpCentrix

# CRITICAL: If not navigate to the correct directory
cd OpCentrix
```

### **Step 2: Current State Analysis**
```powershell
# Check current build status
dotnet build

# Check current database exists
Test-Path "scheduler.db"

# Check current migration status
dotnet ef migrations list --context SchedulerContext

# Check database integrity using SQLite3
sqlite3 scheduler.db "PRAGMA integrity_check;"
```

### **Step 3: Mandatory Backup**
```powershell
# Create backup directory if it doesn't exist
New-Item -ItemType Directory -Path "../backup/database" -Force

# Create timestamped backup
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
Copy-Item "scheduler.db" "../backup/database/scheduler_backup_$timestamp.db"

# Verify backup was created
Test-Path "../backup/database/scheduler_backup_$timestamp.db"
```

### **Step 4: Real-Time Database Access Setup**
```powershell
# Verify SQLite3 is available
sqlite3 --version

# Test database connection
sqlite3 scheduler.db "SELECT COUNT(*) as TableCount FROM sqlite_master WHERE type='table';"

# Enable foreign key constraints for safety
sqlite3 scheduler.db "PRAGMA foreign_keys = ON;"
```

---

## 🔍 **REAL-TIME DATABASE INSPECTION COMMANDS**

### **Database Structure Analysis**
```powershell
# View all tables in database
sqlite3 scheduler.db ".tables"

# Get detailed table schema
sqlite3 scheduler.db ".schema [TableName]"


# View all schemas at once
sqlite3 scheduler.db ".schema"

# Check table row counts
sqlite3 scheduler.db "
SELECT name as TableName, 
       CASE name
         WHEN 'Jobs' THEN (SELECT COUNT(*) FROM Jobs)
         WHEN 'Parts' THEN (SELECT COUNT(*) FROM Parts)
         WHEN 'Machines' THEN (SELECT COUNT(*) FROM Machines)
         WHEN 'Users' THEN (SELECT COUNT(*) FROM Users)
         ELSE 0
       END as RowCount
FROM sqlite_master 
WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name != '__EFMigrationsHistory'
ORDER BY name;"
```

### **Database Health Monitoring**
```powershell
# Check database integrity
sqlite3 scheduler.db "PRAGMA integrity_check;"

# Check foreign key constraints
sqlite3 scheduler.db "PRAGMA foreign_key_check;"

# View database size and performance metrics
sqlite3 scheduler.db "
PRAGMA page_count;
PRAGMA page_size; 
PRAGMA freelist_count;
PRAGMA cache_size;"

# Check index usage
sqlite3 scheduler.db "SELECT name, sql FROM sqlite_master WHERE type='index' ORDER BY name;"
```

### **Live Data Inspection**
```powershell
# View sample data from key tables
sqlite3 scheduler.db "SELECT * FROM Machines WHERE IsActive = 1 LIMIT 5;"
sqlite3 scheduler.db "SELECT * FROM Parts WHERE IsActive = 1 LIMIT 5;"
sqlite3 scheduler.db "SELECT * FROM Jobs WHERE Status = 'Scheduled' LIMIT 5;"

# Check for orphaned records
sqlite3 scheduler.db "
SELECT j.Id, j.PartNumber, p.PartNumber as ValidPart
FROM Jobs j 
LEFT JOIN Parts p ON j.PartId = p.Id 
WHERE p.Id IS NULL;"

# Verify foreign key relationships
sqlite3 scheduler.db "
SELECT 'Jobs->Parts' as Relationship, COUNT(*) as OrphanedRecords
FROM Jobs j LEFT JOIN Parts p ON j.PartId = p.Id WHERE p.Id IS NULL
UNION ALL
SELECT 'MachineCapabilities->Machines', COUNT(*)
FROM MachineCapabilities mc LEFT JOIN Machines m ON mc.MachineId = m.Id WHERE m.Id IS NULL;"
```

---

## 📋 **DATABASE MODIFICATION PROTOCOLS**

### **Protocol A: Adding New Models/Tables**

#### **Step A1: Research Existing Patterns**
```powershell
# Check existing models in the project
Get-ChildItem "Models" -Filter "*.cs" | Select-Object Name

# Search for similar patterns in database
sqlite3 scheduler.db "
SELECT name, sql 
FROM sqlite_master 
WHERE type='table' AND name LIKE '%Stage%' OR name LIKE '%Workflow%';"

# Check existing model configurations
Get-Content "Data/SchedulerContext.cs" | Select-String "modelBuilder.Entity"
```

#### **Step A2: Create Model File**
1. **Create model file** in `Models/` directory
2. **Follow existing naming conventions** (e.g., ProductionStage.cs)
3. **Include proper attributes** ([Key], [Required], [StringLength], etc.)
4. **Add navigation properties** if needed
5. **Include audit fields** (CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy)

#### **Step A3: Update SchedulerContext**
1. **Add DbSet property** to SchedulerContext.cs
2. **Add model configuration** in OnModelCreating method
3. **Follow existing patterns** for configuration
4. **Add proper indexes** for performance
5. **Configure relationships** and foreign keys

#### **Step A4: Verification Before Migration**
```powershell
# Build to check for compilation errors
dotnet build

# Only proceed if build succeeds
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful - ready for migration"
} else {
    Write-Host "❌ Build failed - fix errors before proceeding"
    exit 1
}
```

### **Protocol B: Modifying Existing Models**

#### **Step B1: Document Current State**
```powershell
# Check current database schema
sqlite3 scheduler.db ".schema [TableName]"

# Get current column information
sqlite3 scheduler.db "PRAGMA table_info([TableName]);"

# Check current data in table
sqlite3 scheduler.db "SELECT COUNT(*) FROM [TableName];"

# Check current model definition
Get-Content "Models/[ModelName].cs"

# Check current context configuration
Get-Content "Data/SchedulerContext.cs" | Select-String "[ModelName]" -Context 5
```

#### **Step B2: Make Incremental Changes**
1. **Make ONE change at a time** (don't batch multiple changes)
2. **Test build after each change**
3. **Consider data migration needs** for existing data
4. **Update context configuration** if needed

#### **Step B3: Handle Breaking Changes**
- **For nullable changes**: Add nullable annotations properly
- **For column renames**: Create migration scripts to preserve data
- **For data type changes**: Test with sample data first
- **For foreign key changes**: Update relationships in context

---

## 🛠️ **REAL-TIME DATABASE MODIFICATIONS**

### **Direct SQLite Commands for Quick Fixes**

#### **Adding Columns (Emergency Use Only)**
```powershell
# Add a new column to existing table
sqlite3 scheduler.db "ALTER TABLE [TableName] ADD COLUMN [ColumnName] [DataType] DEFAULT '[DefaultValue]';"

# Verify column was added
sqlite3 scheduler.db "PRAGMA table_info([TableName]);"

# Update existing records if needed
sqlite3 scheduler.db "UPDATE [TableName] SET [ColumnName] = '[Value]' WHERE [Condition];"
```

#### **Executing SQL Scripts (CRITICAL: PowerShell Redirection Fix)**
```powershell
# WRONG: PowerShell doesn't support < redirection
# sqlite3 scheduler.db < script.sql

# CORRECT: Use .read command in SQLite
sqlite3 scheduler.db ".read script.sql"

# ALTERNATIVE: Use Get-Content with pipe
Get-Content "script.sql" | sqlite3 scheduler.db

# ALTERNATIVE: Multi-line command execution
sqlite3 scheduler.db @"
PRAGMA foreign_keys = ON;
CREATE TABLE IF NOT EXISTS TestTable (Id INTEGER PRIMARY KEY);
"@

# FOR COMPLEX SCRIPTS: Use interactive mode
sqlite3 scheduler.db
# Then in SQLite shell: .read script.sql
# Exit with: .quit
```

#### **Fixing Data Issues**
```powershell
# Fix NULL values that cause errors
sqlite3 scheduler.db "UPDATE Parts SET Name = 'Unnamed Part' WHERE Name IS NULL OR Name = '';"

# Fix foreign key mismatches
sqlite3 scheduler.db "
UPDATE MachineCapabilities 
SET MachineId = (SELECT Id FROM Machines WHERE MachineId = 'TI1' LIMIT 1)
WHERE MachineId NOT IN (SELECT Id FROM Machines);"

# Clean up orphaned records
sqlite3 scheduler.db "DELETE FROM Jobs WHERE PartId NOT IN (SELECT Id FROM Parts);"
```

#### **Creating Indexes for Performance**
```powershell
# Create performance indexes
sqlite3 scheduler.db "CREATE INDEX IF NOT EXISTS IX_Jobs_MachineId_Status ON Jobs(MachineId, Status);"
sqlite3 scheduler.db "CREATE INDEX IF NOT EXISTS IX_Parts_IsActive ON Parts(IsActive);"
sqlite3 scheduler.db "CREATE INDEX IF NOT EXISTS IX_Machines_MachineType ON Machines(MachineType);"

# Verify indexes were created
sqlite3 scheduler.db "SELECT name FROM sqlite_master WHERE type='index' AND name LIKE 'IX_%';"
```

#### **Database Maintenance Commands**
```powershell
# Optimize database performance
sqlite3 scheduler.db "VACUUM;"
sqlite3 scheduler.db "ANALYZE;"

# Update table statistics
sqlite3 scheduler.db "UPDATE sqlite_stat1 SET stat = NULL; ANALYZE;"

# Check database after optimization
sqlite3 scheduler.db "PRAGMA integrity_check;"
```

---

## 🔧 **ENTITY FRAMEWORK MIGRATION PROTOCOLS**

### **Protocol C: Creating Migrations**

#### **Step C1: Pre-Migration Checks**
```powershell
# Ensure clean build
dotnet clean
dotnet restore
dotnet build

# Check EF Core tools version
dotnet ef --version

# Verify context can be found
dotnet ef dbcontext list

# Check current database state
sqlite3 scheduler.db "PRAGMA foreign_key_check;"
```

#### **Step C2: Create Migration**
```powershell
# Use descriptive migration names
dotnet ef migrations add [DescriptiveName] --context SchedulerContext

# Example names:
# AddWorkflowTemplateTable
# UpdateProductionStageModel
# FixStageManagementSchema
# AddAdvancedStageFeatures
```

#### **Step C3: Review Generated Migration**
1. **Open generated migration file** in Migrations/ folder
2. **Review Up() method** for correctness
3. **Review Down() method** for proper rollback
4. **Check for data loss warnings**
5. **Verify foreign key constraints**

#### **Step C4: Test Migration**
```powershell
# Generate SQL script to preview changes
dotnet ef migrations script [FromMigration] [ToMigration] --context SchedulerContext

# Apply migration to database
dotnet ef database update --context SchedulerContext

# Verify migration applied successfully
dotnet ef migrations list --context SchedulerContext

# Test database integrity
sqlite3 scheduler.db "PRAGMA integrity_check;"
sqlite3 scheduler.db "PRAGMA foreign_key_check;"

# Test application build
dotnet build
```

### **Protocol D: Handling Migration Failures**

#### **Step D1: Immediate Recovery**
```powershell
# If migration fails, restore from backup immediately
$latestBackup = Get-ChildItem "../backup/database/" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Copy-Item $latestBackup.FullName "scheduler.db" -Force

# Verify database is restored
sqlite3 scheduler.db "PRAGMA integrity_check;"
sqlite3 scheduler.db "SELECT COUNT(*) FROM sqlite_master WHERE type='table';"
```

#### **Step D2: Analyze and Fix**
1. **Read error messages carefully**
2. **Check for schema conflicts**
3. **Verify foreign key constraints**  
4. **Look for missing columns/tables**
5. **Check data type mismatches**

#### **Step D3: Manual Schema Updates (Last Resort)**
**Only use this when EF migrations completely fail:**

```powershell
# Create SQL script for manual fixes
# Example: fix_schema_issues.sql

# Test SQL script first
sqlite3 scheduler.db ".read fix_schema_issues.sql"

# Verify changes
sqlite3 scheduler.db "PRAGMA integrity_check;"

# Create a catch-up migration
dotnet ef migrations add CatchUpManualChanges --context SchedulerContext

# Verify everything is in sync
dotnet build
```

---

## 🔍 **ADVANCED SQLite DEBUGGING TECHNIQUES**

### **Interactive Database Sessions**
```powershell
# Start interactive SQLite session
sqlite3 scheduler.db

# Inside SQLite shell, useful commands:
# .help                    # Show all commands
# .tables                  # List all tables
# .schema tablename        # Show table schema
# .indices tablename       # Show indexes for table
# .dump tablename          # Export table data
# .mode column             # Better formatting
# .headers on              # Show column names
# .quit                    # Exit SQLite shell
```

### **Query Performance Analysis**
```powershell
# Analyze query performance
sqlite3 scheduler.db "EXPLAIN QUERY PLAN SELECT * FROM Jobs WHERE MachineId = 'TI1' AND Status = 'Scheduled';"

# Check slow queries by enabling query logging
sqlite3 scheduler.db "PRAGMA optimize;"

# View database statistics
sqlite3 scheduler.db "
SELECT name, sql FROM sqlite_master WHERE type='table' 
UNION ALL 
SELECT name, sql FROM sqlite_master WHERE type='index';"
```

### **Data Validation and Cleanup**
```powershell
# Find duplicate records
sqlite3 scheduler.db "
SELECT PartNumber, COUNT(*) as DuplicateCount 
FROM Parts 
GROUP BY PartNumber 
HAVING COUNT(*) > 1;"

# Find inconsistent data
sqlite3 scheduler.db "
SELECT m.MachineId, m.Status, j.Status as JobStatus
FROM Machines m 
LEFT JOIN Jobs j ON m.CurrentJobId = j.Id
WHERE (m.Status = 'Building' AND j.Status != 'InProgress');"

# Check constraint violations
sqlite3 scheduler.db "
SELECT * FROM Jobs 
WHERE ScheduledEnd <= ScheduledStart;"
```

---

## ⚠️ **CRITICAL ERROR PREVENTION**

### **Never Do This:**
- ❌ **Never use `dotnet run`** for testing database changes (freezes AI)
- ❌ **Never make changes without backup**
- ❌ **Never batch multiple complex changes**
- ❌ **Never skip build verification**
- ❌ **Never work outside OpCentrix directory**
- ❌ **Never use `&&` operators in PowerShell**
- ❌ **Never use `<` redirection in PowerShell** (`sqlite3 db < script.sql` fails)
- ❌ **Never assume existing schema matches models**
- ❌ **Never disable foreign keys without good reason**
- ❌ **Never run VACUUM on a corrupted database**

### **Always Do This:**
- ✅ **Always backup before changes**
- ✅ **Always verify build success**
- ✅ **Always test one change at a time**
- ✅ **Always use descriptive migration names**
- ✅ **Always verify in correct directory**
- ✅ **Always use PowerShell syntax correctly**
- ✅ **Always check database integrity**
- ✅ **Always enable foreign keys: `PRAGMA foreign_keys = ON;`**
- ✅ **Always test with real data samples**

---

## 🔍 **TROUBLESHOOTING COMMON ISSUES**

### **Issue 1: "No such table" Error**
**Cause**: Model defined in context but table doesn't exist in database
**Solution**:
```powershell
# Check what tables actually exist
sqlite3 scheduler.db ".tables"

# Compare with expected tables from context
Get-Content "Data/SchedulerContext.cs" | Select-String "DbSet"

# Create migration to add missing table
dotnet ef migrations add AddMissingTable --context SchedulerContext
dotnet ef database update --context SchedulerContext

# Or create table manually (emergency only)
sqlite3 scheduler.db "CREATE TABLE [TableName] ([Column] [Type]);"
```

### **Issue 2: "No such column" Error**
**Cause**: Model property exists but column missing in database
**Solution**:
```powershell
# Check actual table schema
sqlite3 scheduler.db ".schema [TableName]"
sqlite3 scheduler.db "PRAGMA table_info([TableName]);"

# Add column manually (emergency fix)
sqlite3 scheduler.db "ALTER TABLE [TableName] ADD COLUMN [ColumnName] [DataType] DEFAULT '[DefaultValue]';"

# Or create migration (preferred)
dotnet ef migrations add AddMissingColumn --context SchedulerContext
dotnet ef database update --context SchedulerContext

# Verify column was added
sqlite3 scheduler.db "PRAGMA table_info([TableName]);"
```

### **Issue 3: Foreign Key Constraint Failures**
**Cause**: Referenced table/column doesn't exist or wrong data type
**Solution**:
```powershell
# Check foreign key constraints
sqlite3 scheduler.db "PRAGMA foreign_key_check;"

# Check referenced table exists
sqlite3 scheduler.db ".schema [ReferencedTable]" 