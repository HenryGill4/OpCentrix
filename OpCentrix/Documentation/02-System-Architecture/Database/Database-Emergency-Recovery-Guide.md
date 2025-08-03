# ?? **DATABASE EMERGENCY RECOVERY GUIDE**

**CRITICAL: Use this guide when database changes go wrong**

---

## ?? **IMMEDIATE ACTIONS (Do This First)**

### **Step 1: Stop Any Running Processes**
```powershell
# If web server is running, stop it with Ctrl+C
# If processes are hung, close terminal and open new one
```

### **Step 2: Navigate to Correct Directory**
```powershell
# CRITICAL: Always start here
cd C:\Users\Henry\source\repos\OpCentrix-MES\OpCentrix
pwd  # Verify location
```

### **Step 3: Assess Current State**
```powershell
# Check if database file exists
Test-Path "scheduler.db"

# Check if database is corrupted
sqlite3 scheduler.db "PRAGMA integrity_check;"

# Check build status
dotnet build
```

---

## ?? **RECOVERY SCENARIOS**

### **Scenario A: Database File Corrupted/Missing**
```powershell
# Find latest backup
Get-ChildItem "../backup/database/" | Sort-Object LastWriteTime -Descending | Select-Object -First 5

# Restore from latest backup
$latestBackup = Get-ChildItem "../backup/database/" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Copy-Item $latestBackup.FullName "scheduler.db" -Force

# Verify restoration
sqlite3 scheduler.db "PRAGMA integrity_check;"
sqlite3 scheduler.db "SELECT COUNT(*) FROM sqlite_master WHERE type='table';"
```

### **Scenario B: Build Failures After Migration**
```powershell
# Clean everything
dotnet clean

# Restore database from backup
$latestBackup = Get-ChildItem "../backup/database/" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Copy-Item $latestBackup.FullName "scheduler.db" -Force

# Restore packages
dotnet restore

# Test build
dotnet build
```

### **Scenario C: Migration Applied But Errors Exist**
```powershell
# Check migration history
dotnet ef migrations list --context SchedulerContext

# Rollback to previous migration (if safe)
dotnet ef database update [PreviousMigrationName] --context SchedulerContext

# Or restore from backup (safer)
$latestBackup = Get-ChildItem "../backup/database/" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Copy-Item $latestBackup.FullName "scheduler.db" -Force
```

### **Scenario D: "No Such Table" Errors**
```powershell
# Check what tables actually exist
sqlite3 scheduler.db ".tables"

# Compare with expected tables from models
Get-Content "Data/SchedulerContext.cs" | Select-String "DbSet"

# Manual fix - create missing tables
Get-Content "fix_missing_tables.sql" | sqlite3 scheduler.db

# Or restore and start over
$latestBackup = Get-ChildItem "../backup/database/" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Copy-Item $latestBackup.FullName "scheduler.db" -Force
```

### **Scenario E: Foreign Key Constraint Failures**
```powershell
# Check which foreign keys are broken
sqlite3 scheduler.db "PRAGMA foreign_key_check;"

# Temporarily disable foreign keys and fix
sqlite3 scheduler.db "PRAGMA foreign_keys = OFF;"
# ... make fixes ...
sqlite3 scheduler.db "PRAGMA foreign_keys = ON;"

# Or restore from backup (safer)
$latestBackup = Get-ChildItem "../backup/database/" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Copy-Item $latestBackup.FullName "scheduler.db" -Force
```

---

## ?? **COMPLETE RESET (Nuclear Option)**

**Use only when all else fails:**

### **Step 1: Backup Current State**
```powershell
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
Copy-Item "scheduler.db" "../backup/database/emergency_backup_$timestamp.db" -ErrorAction SilentlyContinue
```

### **Step 2: Remove Corrupted Database**
```powershell
Remove-Item "scheduler.db" -Force -ErrorAction SilentlyContinue
Remove-Item "scheduler.db-*" -Force -ErrorAction SilentlyContinue
```

### **Step 3: Clean EF Migrations (Extreme)**
```powershell
# Only if migrations are completely broken
# Remove-Item "Migrations/*.cs" -Force
# Keep only the snapshot file
```

### **Step 4: Recreate Database**
```powershell
# This will create a fresh database from models
dotnet ef database update --context SchedulerContext
```

### **Step 5: Verify Everything Works**
```powershell
dotnet build
sqlite3 scheduler.db "PRAGMA integrity_check;"
sqlite3 scheduler.db ".tables"
```

---

## ?? **POST-RECOVERY VERIFICATION**

**After any recovery, verify these items:**

```powershell
# 1. Database file exists and is not corrupted
Test-Path "scheduler.db"
sqlite3 scheduler.db "PRAGMA integrity_check;"

# 2. Expected tables exist
sqlite3 scheduler.db "SELECT COUNT(*) FROM sqlite_master WHERE type='table';"
# Should return around 36 tables

# 3. Foreign keys are valid
sqlite3 scheduler.db "PRAGMA foreign_key_check;"

# 4. Build succeeds
dotnet clean
dotnet restore
dotnet build

# 5. Context validates
dotnet ef dbcontext validate --context SchedulerContext

# 6. Migration history is clean
dotnet ef migrations list --context SchedulerContext
```

---

## ?? **PREVENTION FOR NEXT TIME**

### **Always Before Changes:**
1. **Navigate to correct directory**: `cd OpCentrix`
2. **Create backup**: `Copy-Item "scheduler.db" "../backup/database/scheduler_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').db"`
3. **Verify build**: `dotnet build`
4. **One change at a time**: Don't batch multiple changes

### **During Changes:**
1. **Test build after each change**: `dotnet build`
2. **Use descriptive migration names**: `dotnet ef migrations add DescriptiveName --context SchedulerContext`
3. **Review generated migrations**: Check Up() and Down() methods
4. **Test migration immediately**: `dotnet ef database update --context SchedulerContext`

### **After Changes:**
1. **Verify integrity**: `sqlite3 scheduler.db "PRAGMA integrity_check;"`
2. **Test build**: `dotnet build`
3. **Document changes**: Update change log

---

## ?? **EMERGENCY CONTACTS**

**If you need to restore to a known good state:**

1. **Latest known good backup**: Check `../backup/database/` for recent backups
2. **Git repository**: `git checkout HEAD -- OpCentrix/scheduler.db` (if tracked)
3. **Fresh start**: Delete database and recreate from migrations

**Key commands for emergency:**
```powershell
# Quick restore from latest backup
$latest = Get-ChildItem "../backup/database/" | Sort-Object LastWriteTime -Descending | Select-Object -First 1; Copy-Item $latest.FullName "scheduler.db" -Force

# Quick build test
dotnet build

# Quick integrity check
sqlite3 scheduler.db "PRAGMA integrity_check;"
```

---

**Remember: It's better to restore from backup and lose recent changes than to have a corrupted database in production.**

**Last Updated**: January 2025  
**Emergency Protocol Version**: 1.0