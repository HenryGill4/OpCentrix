# OpCentrix Database Quick Reference

## ?? Database Overview
- **File**: scheduler.db (560 KB)
- **Type**: SQLite
- **Tables**: 23 entities
- **Connection**: `"Data Source=scheduler.db"`

## ??? Core Tables
| Table | Purpose | Key Fields |
|-------|---------|------------|
| **Jobs** | Job scheduling | Id, MachineId, PartNumber, ScheduledStart, Status |
| **Parts** | Part library | Id, PartNumber, Name, Material, SlsMaterial |
| **Machines** | Equipment | Id, MachineId, MachineName, MachineType, Status |
| **Users** | Authentication | Id, Username, Email, Role |

## ?? Key Relationships
```
Jobs -> Parts (PartId, PartNumber)
Jobs -> Machines (MachineId)
JobStages -> Jobs (JobId)
MachineCapabilities -> Machines (MachineId)
```

## ? Performance Indexes
- `(MachineId, ScheduledStart)` - Job scheduling queries
- `PartNumber` (unique) - Part lookups
- `Status, Priority` - Job filtering
- `MachineId` (unique) - Machine operations

## ?? Critical Issues
1. **SlsMachine vs Machine** naming confusion
2. **Foreign key type mismatches** in MachineCapabilities
3. **Missing fields**: EstimatedDuration, BuildVolumeM3, Parts.Name
4. **JavaScript references**: "SslMaterial" vs "SlsMaterial"
5. **Audit field inconsistencies**: CreatedDate vs CreationDate

## ??? Quick Fixes
```powershell
# Navigate to project
cd "C:\Users\Henry\source\repos\OpCentrix\OpCentrix"

# Create backup
Copy-Item "scheduler.db" "backup/scheduler_backup.db"

# Apply migrations
dotnet ef database update

# Export schema
.\database_schema_export.ps1
```

## ?? Admin Commands
- **Database Management**: Admin > Database
- **Schema Export**: `dotnet ef migrations script`
- **Backup**: Admin > Database > Backup
- **Optimization**: VACUUM, ANALYZE via admin interface

## ?? Priority Actions
1. Fix Machine table naming (Week 1)
2. Add missing fields migration (Week 1)
3. Fix foreign key types (Week 1)
4. Add performance indexes (Week 2)
5. Standardize audit fields (Week 2)