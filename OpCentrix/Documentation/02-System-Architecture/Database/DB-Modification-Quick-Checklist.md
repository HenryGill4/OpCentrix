# ?? **DATABASE MODIFICATION QUICK CHECKLIST**

**CRITICAL: Follow this checklist EVERY TIME before database changes**

---

## ? **PRE-FLIGHT (Required)**
```powershell
# 1. Navigate to correct directory
cd OpCentrix

# 2. Verify current location
pwd  # Must show: C:\Users\Henry\source\repos\OpCentrix-MES\OpCentrix

# 3. Create backup
New-Item -ItemType Directory -Path "../backup/database" -Force
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
Copy-Item "scheduler.db" "../backup/database/scheduler_backup_$timestamp.db"

# 4. Verify build works
dotnet build
```

---

## ?? **MAKING CHANGES**

### **For New Models:**
- [ ] Create model file in `Models/` directory
- [ ] Add DbSet to `SchedulerContext.cs`
- [ ] Configure in `OnModelCreating()` method
- [ ] Add proper indexes and relationships
- [ ] Build test: `dotnet build`

### **For Model Updates:**
- [ ] Document current state first
- [ ] Make ONE change at a time
- [ ] Test build after each change
- [ ] Update context configuration if needed

---

## ?? **MIGRATION PROCESS**
```powershell
# 1. Clean build first
dotnet clean
dotnet restore
dotnet build

# 2. Create migration (use descriptive name)
dotnet ef migrations add [DescriptiveName] --context SchedulerContext

# 3. Review generated migration file

# 4. Apply migration
dotnet ef database update --context SchedulerContext

# 5. Verify success
dotnet build
sqlite3 scheduler.db "PRAGMA integrity_check;"
```

---

## ?? **IF ANYTHING FAILS**
```powershell
# Immediate recovery
$latestBackup = Get-ChildItem "../backup/database/" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Copy-Item $latestBackup.FullName "scheduler.db" -Force

# Verify restoration
sqlite3 scheduler.db "PRAGMA integrity_check;"
dotnet build
```

---

## ? **NEVER DO**
- ? Use `dotnet run` for testing (freezes AI)
- ? Work outside OpCentrix directory
- ? Make changes without backup
- ? Use `&&` operators in PowerShell
- ? Skip build verification

## ? **ALWAYS DO**
- ? Start with `cd OpCentrix`
- ? Create backup before changes
- ? Test build after each change
- ? Use descriptive migration names
- ? Verify database integrity

---

**?? SUCCESS = All steps complete + `dotnet build` succeeds + integrity check passes**