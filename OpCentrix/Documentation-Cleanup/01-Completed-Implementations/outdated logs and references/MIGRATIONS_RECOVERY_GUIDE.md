# OpCentrix Migrations Crisis - Step-by-Step Recovery Guide

## ?? SITUATION ANALYSIS
Your migrations folder has been severely corrupted during the cleanup process. All migration files now contain duplicate method definitions, making the project unable to compile.

## ?? MANUAL RECOVERY STEPS

### Step 1: Navigate to Migrations Folder
```powershell
cd "C:\Users\Henry\source\repos\OpCentrix\OpCentrix\Migrations"
```

### Step 2: Create Emergency Backup
```powershell
# Create timestamp for backup
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
New-Item -ItemType Directory -Path "../EmergencyBackup_$timestamp" -Force
Copy-Item *.cs "../EmergencyBackup_$timestamp/"
```

### Step 3: Remove ALL Corrupted Migration Files
```powershell
# Remove ALL .cs files in the Migrations folder
Remove-Item "*.cs" -Force
```

### Step 4: Verify Cleanup
```powershell
# Should show no .cs files
Get-ChildItem *.cs
```

### Step 5: Create Minimal Snapshot
Create a new file `SchedulerContextModelSnapshot.cs` with this content:

```csharp
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OpCentrix.Data;

#nullable disable

namespace OpCentrix.Migrations
{
    [DbContext(typeof(SchedulerContext))]
    partial class SchedulerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "8.0.11");
        }
    }
}
```

### Step 6: Test Compilation
```powershell
cd ..
dotnet build
```

### Step 7: If Build Succeeds, Recreate Database
```powershell
# Remove existing database
Remove-Item "scheduler.db" -ErrorAction SilentlyContinue
Remove-Item "scheduler.db-*" -ErrorAction SilentlyContinue

# Create new migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### Step 8: Verify Application
```powershell
dotnet run
```

## ?? EXPECTED OUTCOME
- ? Project builds successfully
- ? Database recreated with current model
- ? Application starts on http://localhost:5090
- ? Admin login works (admin/admin123)

## ?? IF PROBLEMS PERSIST
1. Check that ALL .cs files were removed from Migrations folder
2. Verify only SchedulerContextModelSnapshot.cs exists
3. Clear build cache: `dotnet clean`
4. Rebuild: `dotnet build`

## ?? BACKUP PLAN
If this doesn't work, we have these backups:
- `MigrationsBackup_20250801_171841` (from original cleanup)
- `EmergencyBackup_[timestamp]` (just created)

The nuclear option is to start completely fresh with a new migration history.