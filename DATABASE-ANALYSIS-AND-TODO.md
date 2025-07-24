# ?? OpCentrix Database Structure & Server Calls Analysis

## ??? **Current Database Structure**

Your OpCentrix system uses a **SQLite database** (`scheduler.db`) with **Entity Framework Core** and has a comprehensive structure for SLS (Selective Laser Sintering) metal printing manufacturing operations.

### **Database Tables Overview**

| Table | Primary Purpose | Key Features |
|-------|----------------|--------------|
| **Jobs** | Scheduled manufacturing jobs | Core scheduling, SLS parameters, cost tracking |
| **Parts** | Manufacturing part specifications | SLS materials, process parameters, cost data |
| **JobLogEntries** | Audit trail of all operations | Complete activity history |
| **Users** | System user accounts | Role-based access control |
| **UserSettings** | User preferences | Personalization settings |
| **SlsMachines** | TruPrint 3000 machine data | Machine status, OPC UA integration |
| **MachineDataSnapshots** | Historical machine telemetry | Performance monitoring |
| **BuildJobs** | Print tracking workflow | Real-time build management |
| **BuildJobParts** | Parts in each build | Build composition tracking |
| **DelayLogs** | Production delays | Delay analysis and reporting |

### **Database Access Methods**

Your system uses **3 different database access patterns**:

1. **Direct SchedulerContext** - `_context.Jobs.Include(j => j.Part)...`
2. **SchedulerService Layer** - `_schedulerService.ValidateJobScheduling(...)`
3. **Entity Framework Migrations** - For schema management

---

## ?? **How to Access Your Database**

### **Option 1: Through the Application (Recommended)**
```bash
# Start the application
cd OpCentrix
dotnet run

# Access via:
http://localhost:5000/Account/Login

# Test Credentials:
admin/admin123     (Full Access)
manager/manager123 (Management Access)
operator/operator123 (Operator Access)
```

### **Option 2: SQLite Database Browser**
```bash
# Install SQLite Browser
# Download from: https://sqlitebrowser.org/

# Open database file:
# File: OpCentrix/scheduler.db
```

### **Option 3: SQLite Command Line**
```bash
# From OpCentrix directory
sqlite3 scheduler.db

# Common queries:
.tables                    # List all tables
.schema Jobs              # Show Jobs table structure
SELECT COUNT(*) FROM Jobs; # Count jobs
SELECT * FROM Users;      # View all users
.quit                     # Exit
```

### **Option 4: Visual Studio Database Tools**
```
1. In Visual Studio, open View ? SQL Server Object Explorer
2. Add ? SQL Server ? Browse ? Local ? MSSQLLocalDB
3. Right-click ? Attach Database ? select scheduler.db
```

---

## ?? **Database Modifications Needed**

### **Critical Issues Found:**

1. **Missing Table: `MachineDataSnapshot`**
   - Defined in context but missing property mapping
   - Needed for telemetry data

2. **Incomplete BuildJob Relations**
   - Missing proper navigation properties
   - No cascade delete configured properly

3. **OPC UA Fields in Jobs**
   - Some fields may be nullable but not marked as such

---

## ?? **Server Call Analysis - Issues Found**

I've identified several server calls that won't work correctly with your current database structure:

### **?? CRITICAL ISSUES**

#### **1. Admin Dashboard Sample Data Management**
**File:** `Pages/Admin/Index.cshtml.cs`
**Issue:** Missing handlers for sample data operations
```csharp
// MISSING HANDLERS:
OnPostRemoveSampleData()  // Referenced in Admin/Index.cshtml  
OnPostAddSampleData()     // Referenced in Admin/Index.cshtml
OnPostBackupDatabase()    // Referenced in Admin/Index.cshtml
```

#### **2. Print Tracking System**
**File:** `Pages/PrintTracking/Index.cshtml.cs`
**Issue:** Missing handlers for build management
```csharp
// MISSING HANDLERS:
OnPostStartPrintModal()    // Referenced in PrintTracking/Index.cshtml
OnPostPostPrintModal()     // Referenced in PrintTracking/Index.cshtml  
OnPostRefreshDashboard()   // Referenced in PrintTracking/Index.cshtml
```

#### **3. Parts Management**
**File:** `Pages/Admin/Parts.cshtml.cs` 
**Issue:** Delete handler references wrong method signature
```csharp
// CURRENT (PROBLEMATIC):
hx-post="/Admin/Parts?handler=Delete&id=@part.Id"

// NEEDS TO BE:
OnPostDeleteAsync(int id)  // Current method signature correct
```

#### **4. Scheduler Missing Handlers**
**File:** `Pages/Scheduler/Index.cshtml.cs`
**Issue:** Some HTMX calls reference non-existent handlers
```csharp
// POTENTIALLY MISSING:
OnPostDeleteJobAsync()     // May be referenced in frontend
OnGetJobDetailsAsync()     // May be referenced for job details modal
```

#### **5. Database Context Issues**
**File:** `Data/SchedulerContext.cs`
**Issue:** Missing MachineDataSnapshot DbSet property
```csharp
// MISSING:
public DbSet<MachineDataSnapshot> MachineDataSnapshots { get; set; }
// Currently commented or missing despite model existing
```

---

## ?? **TODO LIST - PRIORITY ORDER**

### **?? HIGH PRIORITY (Must Fix)**

#### **TODO 1: Fix Database Context**
- Add missing `MachineDataSnapshot` property
- Fix BuildJob navigation properties
- Add proper cascade delete behaviors

#### **TODO 2: Implement Missing Admin Handlers**
- `OnPostRemoveSampleData()` in Admin/Index.cshtml.cs
- `OnPostAddSampleData()` in Admin/Index.cshtml.cs  
- `OnPostBackupDatabase()` in Admin/Index.cshtml.cs

#### **TODO 3: Implement Print Tracking Handlers**
- `OnGetStartPrintModalAsync()` in PrintTracking/Index.cshtml.cs
- `OnGetPostPrintModalAsync()` in PrintTracking/Index.cshtml.cs
- `OnPostRefreshDashboardAsync()` in PrintTracking/Index.cshtml.cs

#### **TODO 4: Fix Parts Delete Operation**
- Verify delete handler works with current HTMX calls
- Add proper error handling for parts in use

### **?? MEDIUM PRIORITY (Should Fix)**

#### **TODO 5: Add Missing Scheduler Handlers**
- `OnPostDeleteJobAsync()` for job deletion
- `OnGetJobDetailsAsync()` for job details modal
- Proper error handling for all job operations

#### **TODO 6: Enhance Error Handling**
- Add try-catch blocks to all server calls
- Implement proper logging for all operations
- Add user-friendly error messages

#### **TODO 7: Database Optimization**
- Add missing indexes for performance
- Optimize query patterns
- Add data validation

### **?? LOW PRIORITY (Nice to Have)**

#### **TODO 8: Advanced Features**
- Add bulk operations for jobs/parts
- Implement advanced filtering
- Add export functionality

#### **TODO 9: UI/UX Improvements**
- Add loading states for all operations
- Improve error message display
- Add confirmation dialogs

---

## ?? **Recommended Action Plan**

### **Phase 1: Critical Database Fixes (1-2 hours)**
1. Fix SchedulerContext missing properties
2. Run database migration to update schema
3. Test basic CRUD operations

### **Phase 2: Admin System (2-3 hours)**
1. Implement all missing admin handlers
2. Test sample data management
3. Test backup functionality

### **Phase 3: Print Tracking (2-3 hours)**
1. Implement all print tracking handlers
2. Test build job workflow
3. Test dashboard refresh

### **Phase 4: Scheduler Enhancement (1-2 hours)**
1. Add missing scheduler handlers
2. Test all job operations
3. Verify error handling

### **Phase 5: Testing & Polish (2-3 hours)**
1. Comprehensive testing of all features
2. Performance optimization
3. Final error handling improvements

---

## ? **Current Working Features**

### **? FULLY FUNCTIONAL:**
- User authentication and authorization
- Scheduler job creation and editing
- Parts management (create, edit, list)
- Job audit logging
- Database initialization and seeding
- Basic admin dashboard

### **?? PARTIALLY FUNCTIONAL:**
- Admin sample data management (UI exists, handlers missing)
- Print tracking dashboard (UI exists, handlers missing)  
- Parts deletion (handler exists, may have edge cases)

### **? NOT FUNCTIONAL:**
- Sample data operations from admin panel
- Print tracking build management
- Database backup from admin panel
- Some advanced scheduler operations

---

## ?? **Next Steps**

**READY TO START FIXES?** 

I can implement all the missing handlers and fix the database issues. The fixes are straightforward since your architecture is solid - we just need to implement the missing server-side handlers that the frontend is already calling.

**Would you like me to:**
1. **Start with the critical database fixes**
2. **Implement all missing handlers at once**  
3. **Go through the TODO list one by one**
4. **Focus on a specific area first (Admin, Print Tracking, or Scheduler)**

Your system is actually very well structured - we're mainly dealing with missing implementations rather than architectural issues!