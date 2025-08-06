# ?? **OpCentrix Stage-Based Manufacturing Dashboard Implementation Plan**

**Date**: August 5, 2025  
**Research Status**: ? **COMPREHENSIVE DATABASE ANALYSIS COMPLETED**  
**System Status**: ?? **ADVANCED MES WITH 43 TABLES + SOPHISTICATED STAGE INFRASTRUCTURE**  

---

## ?? **CRITICAL: POWERSHELL & DATABASE COMMAND INSTRUCTIONS**

### **?? MANDATORY COMMAND PROTOCOLS - READ FIRST**

#### **?? PowerShell-Only Commands (NEVER use && operators)**
```powershell
# ? CORRECT: Individual PowerShell commands
dotnet clean
dotnet restore  
dotnet build OpCentrix/OpCentrix.csproj

# ? WRONG: Never use && in PowerShell (WILL FAIL)
# dotnet clean && dotnet restore  # This WILL FAIL

# ? CORRECT: If you need conditional execution
if ($LASTEXITCODE -eq 0) {
    dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --verbosity normal
}

# ? NEVER USE: dotnet run (freezes AI assistant)
```

#### **??? SQLite Database Commands (Proper PowerShell Syntax)**
```powershell
# ? CORRECT: Direct SQLite commands
sqlite3 scheduler.db "PRAGMA integrity_check;"
sqlite3 scheduler.db "SELECT COUNT(*) FROM ProductionStages WHERE IsActive = 1;"
sqlite3 scheduler.db ".tables"

# ? CORRECT: Execute SQL script files
sqlite3 scheduler.db ".read Database/script.sql"
Get-Content "Database/script.sql" | sqlite3 scheduler.db

# ? WRONG: Never use < redirection in PowerShell
# sqlite3 scheduler.db < script.sql  # This WILL FAIL in PowerShell

# ? CORRECT: Multi-line SQL in PowerShell
sqlite3 scheduler.db @"
PRAGMA foreign_keys = ON;
SELECT * FROM ProductionStages ORDER BY DisplayOrder;
"@
```

#### **?? Mandatory Pre-Work Protocol**
```powershell
# STEP 1: Navigate to correct directory
cd OpCentrix

# STEP 2: Verify environment
pwd  # Should show: .../OpCentrix-MES/OpCentrix
Test-Path "scheduler.db"  # Should return True

# STEP 3: Create backup (MANDATORY before ANY changes)
New-Item -ItemType Directory -Path "../backup/database" -Force
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
Copy-Item "scheduler.db" "../backup/database/scheduler_backup_$timestamp.db"

# STEP 4: Verify backup created
Test-Path "../backup/database/scheduler_backup_$timestamp.db"

# STEP 5: Check database integrity
sqlite3 scheduler.db "PRAGMA integrity_check;"
sqlite3 scheduler.db "PRAGMA foreign_key_check;"
```

#### **?? Database Research Commands for Implementation**
```powershell
# Check existing production stages (should show 7 stages)
sqlite3 scheduler.db "SELECT Name, DisplayOrder, StageColor, Department FROM ProductionStages WHERE IsActive = 1 ORDER BY DisplayOrder;"

# Check stage-related tables structure
sqlite3 scheduler.db "PRAGMA table_info(ProductionStages);"
sqlite3 scheduler.db "PRAGMA table_info(JobStages);"
sqlite3 scheduler.db "PRAGMA table_info(ProductionStageExecutions);"