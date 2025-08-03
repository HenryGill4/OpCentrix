# ?? **OpCentrix Comprehensive Testing Framework & Bug Debugging Guide**

**Date**: January 2025  
**Status**: ?? **COMPLETE TESTING PROTOCOL** - Systematic approach to validate all functionality  
**Goal**: Provide a comprehensive testing framework to identify and resolve bugs efficiently  

---

## ?? **CRITICAL TESTING INSTRUCTIONS FOR AI ASSISTANT**

### **?? MANDATORY RESEARCH PROTOCOL**
**?? READ THESE INSTRUCTIONS EVERY TIME WE DEBUG TOGETHER**

#### **1. ALWAYS Start with Context Gathering**
```powershell
# Before debugging ANY issue, ALWAYS run these commands:
dotnet clean
dotnet restore
dotnet build OpCentrix/OpCentrix.csproj

# If build fails, stop immediately and fix compilation errors first
# Only proceed to testing if build is 100% successful
```
- **REQUIRED**: Use `text_search` to understand current system state
- **REQUIRED**: Check error logs in `OpCentrix/logs/` directory
- **REQUIRED**: Verify which user role is experiencing the issue
- **REQUIRED**: Reproduce the exact steps that led to the problem

#### **2. PowerShell-Only Commands (CRITICAL)**
```powershell
# ? CORRECT PowerShell syntax for testing
dotnet clean
dotnet restore
dotnet build OpCentrix/OpCentrix.csproj
dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --verbosity normal

# For application testing (ONLY when specifically requested):
cd OpCentrix
dotnet run
# Note: This starts the web server - NEVER use for automated testing

# ? WRONG - Never use && in PowerShell
dotnet clean && dotnet restore  # This will FAIL in PowerShell
```

#### **3. Error Analysis Requirements**
- **ALWAYS** capture the full error message with stack trace
- **NEVER** assume the cause - investigate systematically
- **REQUIRED**: Check browser console (F12) for JavaScript errors
- **VERIFY**: Authentication status and user role for access issues
- **CHECK**: Network tab in browser for failed API calls

#### **4. Bug Investigation Protocol**
```powershell
# Systematic bug investigation process:

# Step 1: Verify build status
dotnet build OpCentrix/OpCentrix.csproj

# Step 2: Check recent logs
Get-Content "OpCentrix/logs/opcentrix-*.log" | Select-Object -Last 50

# Step 3: Run relevant tests
dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --filter "CategoryName" --verbosity normal

# Step 4: Check database state (if needed)
# Only suggest SQL queries if database issue is suspected
```

#### **5. Database Investigation Protocol**
```powershell
# Database-specific debugging commands:

# Step 1: Verify database exists and is accessible
Test-Path "OpCentrix/scheduler.db"
if (-not (Test-Path "OpCentrix/scheduler.db")) {
    Write-Host "? Database missing - creating new database" -ForegroundColor Red
    cd OpCentrix
    dotnet ef database update
}

# Step 2: Check database integrity
sqlite3 OpCentrix/scheduler.db "PRAGMA integrity_check;"

# Step 3: Verify Entity Framework model matches database
dotnet ef dbcontext validate --project OpCentrix

# Step 4: Check critical table row counts (UPDATED - now includes workflow tables)
sqlite3 OpCentrix/scheduler.db "
SELECT 'Parts' as TableName, COUNT(*) as RowCount FROM Parts WHERE IsActive = 1
UNION ALL
SELECT 'Jobs', COUNT(*) FROM Jobs WHERE Status IN ('Scheduled', 'Running')  
UNION ALL
SELECT 'Machines', COUNT(*) FROM Machines WHERE IsActive = 1
UNION ALL
SELECT 'Users', COUNT(*) FROM Users WHERE IsActive = 1
UNION ALL
SELECT 'ProductionStages', COUNT(*) FROM ProductionStages WHERE IsActive = 1
UNION ALL
SELECT 'BuildCohorts ?', COUNT(*) FROM BuildCohorts
UNION ALL
SELECT 'JobStageHistories ?', COUNT(*) FROM JobStageHistories;"
```

#### **6. Database Schema Reference**
**?? Complete Schema Documentation**: See `OpCentrix/Database-Schema-Documentation.md`

**? LIVE DATABASE STRUCTURE** (Updated with Option A Workflow Fields):
- **Jobs** (87+ columns) - ? **NOW INCLUDES** workflow fields: BuildCohortId, WorkflowStage, StageOrder, TotalStages
- **Parts** (150+ columns) - Enhanced with B&T specialization
- **Machines** - Manufacturing equipment management  
- **? BuildCohorts** - ? **NOW AVAILABLE** - SLS build grouping for cohort tracking
- **? JobStageHistories** - ? **NOW AVAILABLE** - Enhanced traceability for stage transitions
- **ProductionStages** - Configurable workflow stages
- **SerialNumbers** - ATF compliance tracking (B&T)
- **Users** - Authentication and role management

**? UPDATED Critical Relationships:**
```
Jobs ?? Parts (via PartNumber)
Jobs ?? BuildCohorts (FK: BuildCohortId) ? NOW ACTIVE
Jobs ?? JobStageHistories (1:Many) ? NOW ACTIVE
Parts ?? PartStageRequirements (1:Many)
ProductionStages ?? PartStageRequirements (1:Many)
BuildCohorts ?? Jobs (1:Many) ? NOW ACTIVE
```

**?? OPTION A WORKFLOW FEATURES NOW AVAILABLE:**
- ? **BuildCohort Management** - Track 20-130 parts per SLS build
- ? **Workflow Stage Tracking** - SLS ? EDM ? Coating ? QC ? Shipping
- ? **Stage History Audit** - Complete traceability of stage transitions
- ? **Cohort-Based Scheduling** - Group jobs by build cohorts
- ? **Multi-Stage Job Progression** - Track jobs through manufacturing stages

### **?? Database Schema Quick Facts**
- **Total Tables**: ? **38+ entities** with full relationships (UPDATED)
- **Key Entities**: Jobs (? enhanced), Parts, Machines, Users, ? BuildCohorts, ? JobStageHistories, ProductionStages
- **Database Engine**: SQLite with Entity Framework Core 8.0.11
- **File Location**: `OpCentrix/scheduler.db`
- **Backup Location**: Auto-generated with timestamp in same directory
- **? WORKFLOW STATUS**: Option A implementation complete - all tables and fields active