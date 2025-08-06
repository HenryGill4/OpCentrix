# OpCentrix Stage-Based Manufacturing Dashboard Implementation Plan

**Date**: August 5, 2025  
**Research Status**: COMPLETED - Database analysis of 43 tables  
**System Status**: ADVANCED MES WITH EXTENSIVE STAGE INFRASTRUCTURE  

---

## CRITICAL: NO UNICODE CHARACTERS ALLOWED

**MANDATORY RULE**: Never use Unicode characters, emojis, or special symbols in any code, comments, or documentation.

### Approved ASCII Alternatives:
```
Instead of: ✅ ❌ ⚠️ 📊 🎯 💡 ➡️ ⭐ 
Use:        [OK] [NO] [WARN] [INFO] [TARGET] [IDEA] -> [STAR]

Instead of: 🔧 🔍 📋 🚀 💻 📁 
Use:        [TOOL] [SEARCH] [LIST] [LAUNCH] [PC] [FOLDER]

Instead of: ✓ • ★ ◆ ► ◄ ▲ ▼
Use:        [CHECK] * [STAR] [DIAMOND] > < ^ v
```

### Why Unicode Causes Problems:
1. **PowerShell Compatibility**: Encoding issues in commands
2. **Database Issues**: SQLite encoding problems
3. **Git/Source Control**: Commit and merge failures
4. **Cross-Platform**: Display issues on different systems
5. **Terminal/Console**: Breaks in various console environments

---

## CRITICAL: POWERSHELL & DATABASE COMMAND INSTRUCTIONS

### MANDATORY COMMAND PROTOCOLS - READ FIRST

#### PowerShell-Only Commands (NEVER use && operators)
```powershell
# [CHECK] CORRECT: Individual PowerShell commands
dotnet clean
dotnet restore  
dotnet build OpCentrix/OpCentrix.csproj

# [X] WRONG: Never use && in PowerShell (WILL FAIL)
# dotnet clean && dotnet restore  # This WILL FAIL

# [CHECK] CORRECT: If you need conditional execution 
if ($LASTEXITCODE -eq 0) {
    dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --verbosity normal
}

# [X] NEVER USE: dotnet run (freezes AI assistant)
```

#### SQLite Database Commands (Proper PowerShell Syntax)
```powershell
# [CHECK] CORRECT: Direct SQLite commands
sqlite3 scheduler.db "PRAGMA integrity_check;"
sqlite3 scheduler.db "SELECT COUNT(*) FROM ProductionStages WHERE IsActive = 1;"
sqlite3 scheduler.db ".tables"

# [CHECK] CORRECT: Execute SQL script files
sqlite3 scheduler.db ".read Database/script.sql"
Get-Content "Database/script.sql" | sqlite3 scheduler.db

# [X] WRONG: Never use < redirection in PowerShell
# sqlite3 scheduler.db < script.sql  # This WILL FAIL in PowerShell

# [CHECK] CORRECT: Multi-line SQL in PowerShell
sqlite3 scheduler.db @"
PRAGMA foreign_keys = ON;
SELECT * FROM ProductionStages ORDER BY DisplayOrder;
"@
```

#### Mandatory Pre-Work Protocol
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

#### Database Research Commands for Implementation
```powershell
# Check existing production stages (should show 7 stages)
sqlite3 scheduler.db "SELECT Name, DisplayOrder, StageColor, Department FROM ProductionStages WHERE IsActive = 1 ORDER BY DisplayOrder;"

# Check stage-related tables structure
sqlite3 scheduler.db "PRAGMA table_info(ProductionStages);"
sqlite3 scheduler.db "PRAGMA table_info(JobStages);"
sqlite3 scheduler.db "PRAGMA table_info(ProductionStageExecutions);"

# Check if jobs have workflow fields
sqlite3 scheduler.db "PRAGMA table_info(Jobs);" | Select-String "WorkflowStage|BuildCohortId|StageOrder"

# Count existing data for planning
sqlite3 scheduler.db "SELECT COUNT(*) as ActiveJobs FROM Jobs WHERE Status IN ('Scheduled', 'InProgress');"
sqlite3 scheduler.db "SELECT COUNT(*) as ActiveParts FROM Parts WHERE IsActive = 1;"
```

#### Build & Validation Commands
```powershell
# Always run after any code changes (in sequence, never with &&)
dotnet clean
dotnet restore
dotnet build OpCentrix/OpCentrix.csproj

# Only run tests if build succeeds
if ($LASTEXITCODE -eq 0) {
    Write-Host "SUCCESS: Build successful"
    dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --verbosity normal
} else {
    Write-Host "ERROR: Build failed - fix errors before proceeding"
}

# Check database state after any schema changes
sqlite3 scheduler.db "PRAGMA integrity_check;"
dotnet ef dbcontext validate --context SchedulerContext
```

---

## COMPREHENSIVE SYSTEM RESEARCH FINDINGS

### Your Current OpCentrix MES Architecture:

#### Database Infrastructure (43 Tables)
Your system already has sophisticated stage management:
- **ProductionStages** (25 fields) - 7 configured stages with colors, departments
- **PartStageRequirements** (27 fields) - Advanced stage assignments per part  
- **JobStages** - Individual stage execution tracking
- **Jobs** (87 fields) - BuildCohortId, WorkflowStage, StageOrder, TotalStages READY
- **BuildCohorts** - Group tracking already implemented
- **JobStageHistories** - Complete audit trail exists
- **ProductionStageExecutions** - Stage completion tracking

#### Current Production Stages (CONFIGURED)
```
STAGE 1: 3D Printing (SLS) -> #007bff (Blue) -> 3D Printing Dept
STAGE 2: CNC Machining -> #28a745 (Green) -> CNC Machining Dept  
STAGE 3: EDM -> #ffc107 (Yellow) -> EDM Dept
STAGE 4: Laser Engraving -> #fd7e14 (Orange) -> Laser Operations
STAGE 5: Sandblasting -> #6c757d (Gray) -> Finishing Dept
STAGE 6: Coating/Cerakote -> #17a2b8 (Teal) -> Finishing Dept
STAGE 7: Assembly -> #dc3545 (Red) -> Assembly Dept
```

#### Existing Management Pages
- `/Admin/ProductionStages` - Full stage configuration UI (25 fields per stage)
- `/Admin/Stages` - Job stage management interface  
- `/PrintTracking` - **PERFECT OPERATOR DASHBOARD EXAMPLE** (mobile-optimized)
- Advanced approval workflow built into ProductionStages (`RequiresApproval` field)

#### Services Already Implemented
Based on research documentation:
- **CohortManagementService** - Stage-aware cohort tracking
- **StageProgressionService** - Automatic stage advancement  
- **PrototypeTrackingService** - Complete stage execution tracking
- **ProductionStageService** - Stage configuration management
- **PrintTrackingService** - Operator workflow management

---

## ANSWERS TO YOUR SPECIFIC QUESTIONS

### 1. Stage Management Assessment
**YOUR CURRENT SYSTEM**: You have **EXCELLENT** existing stage management:
- `/Admin/ProductionStages` - Full stage configuration (25 fields per stage)
- `/Admin/Stages` - Advanced job stage management 
- 7 Production stages CONFIGURED with colors, departments, approval settings
- Built-in approval workflow: `RequiresApproval` field per stage

### 2. Database Structure - What We'll Use
**EXISTING TABLES (Ready to use)**:
```sql
ProductionStages (25 fields) - Stage definitions with RequiresApproval
JobStages - Individual stage execution tracking
ProductionStageExecutions - Stage completion tracking  
Jobs (87 fields) - WorkflowStage, BuildCohortId, StageOrder ready
PartStageRequirements (27 fields) - Stage assignments per part
BuildCohorts - Group progression tracking
Users - Role-based stage access control
```

### 3. Operator Dashboard Pattern
**USE YOUR PRINTTRACKING PAGE**: It's the PERFECT template!
- Mobile-optimized [CHECK]
- Role-based (Admin vs Operator views) [CHECK]  
- Touch-friendly for tablets [CHECK]
- HTMX real-time updates [CHECK]
- Fixed action buttons [CHECK]

### 4. Manufacturing Design Decision
**HORIZONTAL PROGRESSION with Progress Bars**:
```
SLS -> CNC -> EDM -> Laser -> Sandblasting -> Coating -> Assembly
[PROGRESS: 100%] -> [75%] -> [0%] -> [0%] -> [0%] -> [0%] -> [0%]
```

### 5. Views Structure
**MASTER + INDIVIDUAL DASHBOARDS**:
- **Master Dashboard**: All 7 stages with progress overview
- **Stage Dashboards**: Individual pages per department (copy PrintTracking)
- **Admin Oversight**: Cross-stage monitoring with approval workflow

---

## SMART REUSE STRATEGY

### REUSE: Stable & Working Components

#### 1. Existing SchedulerService.cs (KEEP & EXTEND)
**Status**: [CHECK] **Proven, stable, comprehensive**
- **What Works**: Job validation, overlap detection, machine compatibility
- **What to Reuse**: All business logic methods, validation rules, machine calculations
- **How to Extend**: Add new stage-aware methods alongside existing ones

```csharp
// EXISTING (Keep working):
public bool ValidateJobScheduling(Job job, List<Job> existingJobs, out List<string> errors)
public (int maxLayers, int rowHeight) CalculateMachineRowLayout(string machineId, List<Job> jobs)

// NEW (Add to same service):
public async Task<List<StageAwareJobBlock>> GetStageAwareJobsAsync(DateTime start, DateTime end)
public async Task<bool> ValidateStageTransitionAsync(int jobId, string fromStage, string toStage)
```

#### 2. Parts System (MOSTLY KEEP)
**Status**: [CHECK] **Recently fixed, working well**
- **What Works**: CRUD operations, modal system, stage management
- **What to Reuse**: Database models, service layer, form validation
- **What to Enhance**: Add stage-to-scheduler integration methods

#### 3. Database Models & Context (KEEP ALL)
**Status**: [CHECK] **Sophisticated, normalized, working**
- **What Works**: Part, PartStageRequirement, ProductionStage models
- **What to Reuse**: All existing tables and relationships
- **What to Add**: Operator punch tracking tables only

### BUILD NEW: Stage-Focused Components

#### 1. NEW PAGE: Stage-Based Scheduler
**Path**: `/Scheduler/Stages` (separate from existing `/Scheduler`)
**Benefits**: No risk to existing scheduler, clean implementation

#### 2. NEW PAGE: Operator Dashboard  
**Path**: `/Operations/Dashboard` (operator-focused interface)
**Benefits**: Mobile-optimized for shop floor, no conflicts

#### 3. NEW SERVICES: Stage-Specific Logic
**New**: `IStageSchedulingService`, `IOperatorPunchService`
**Benefits**: Clean separation, no impact on existing scheduler logic

---

## IMPLEMENTATION PLAN

### Phase 1: Master Stage Dashboard (3 hours)

#### Prerequisites Commands:
```powershell
# Research existing stage data
sqlite3 scheduler.db "SELECT Id, Name, StageColor, Department, RequiresApproval FROM ProductionStages WHERE IsActive = 1 ORDER BY DisplayOrder;"

# Check existing job workflow fields
sqlite3 scheduler.db "SELECT COUNT(*) FROM Jobs WHERE WorkflowStage IS NOT NULL;"

# Verify existing services
Get-ChildItem "Services" -Filter "*Stage*.cs" | Select-Object Name
```

**Create**: `/Operations/StageDashboard` page

**Features**:
- Visual progress flow across all 7 manufacturing stages
- Progress bars showing completion percentage per stage  
- Stage cards with active job counts and department info
- Click to enter individual stage dashboards
- Admin vs Operator role-based views
- Real-time updates using HTMX

**UI Pattern**: Copy PrintTracking header, layout, and mobile-responsive design

#### Step 1.1: Create New Stage Scheduler Controller (Hour 1)
```csharp
// NEW FILE: Pages/Operations/StageDashboard.cshtml.cs
public class StageDashboardModel : PageModel
{
    private readonly ISchedulerService _schedulerService; // REUSE existing
    private readonly IStageSchedulingService _stageService; // NEW
    private readonly SchedulerContext _context; // REUSE existing
    
    public List<StageAwareJob> StageJobs { get; set; } = new();
    public string ViewMode { get; set; } = "stage"; // stage, machine, operator
    
    public async Task OnGetAsync(string? view = "stage")
    {
        ViewMode = view;
        
        // REUSE existing data loading
        var jobs = await LoadJobsFromExistingScheduler();
        
        // NEW stage breakdown logic
        StageJobs = await _stageService.ConvertJobsToStageAwareAsync(jobs);
    }
    
    // NEW operator punch methods
    public async Task<IActionResult> OnPostPunchInAsync(int jobId, int stageId, string operatorName)
    {
        // Stage-specific punch logic
    }
}
```

#### Step 1.2: Stage Scheduling Service (Hour 1 continued)
```csharp
// NEW FILE: Services/StageSchedulingService.cs
public interface IStageSchedulingService
{
    Task<List<StageAwareJob>> ConvertJobsToStageAwareAsync(List<Job> jobs);
    Task<List<StageBlock>> GetStageBlocksForJobAsync(int jobId);
    Task<bool> CanOperatorPunchAsync(int jobId, int stageId, string operatorName);
}

public class StageSchedulingService : IStageSchedulingService
{
    private readonly SchedulerContext _context;
    private readonly ISchedulerService _existingScheduler; // REUSE!
    
    // Build on existing scheduler logic, don't replace it
    public async Task<List<StageAwareJob>> ConvertJobsToStageAwareAsync(List<Job> jobs)
    {
        var stageAwareJobs = new List<StageAwareJob>();
        
        foreach(var job in jobs)
        {
            // REUSE existing job validation
            var isValidJob = _existingScheduler.ValidateJobScheduling(job, jobs, out var errors);
            
            // NEW stage breakdown
            var stageBlocks = await BuildStageBlocksFromPartRequirements(job);
            
            stageAwareJobs.Add(new StageAwareJob 
            {
                BaseJob = job, // Keep original job intact
                StageBlocks = stageBlocks,
                IsValid = isValidJob
            });
        }
        
        return stageAwareJobs;
    }
}
```

#### Step 1.3: Beautiful Stage View (Hour 2)
```html
<!-- NEW FILE: Pages/Operations/StageDashboard.cshtml -->
@page "/Operations/StageDashboard"
@model OpCentrix.Pages.Operations.StageDashboardModel

<div class="stage-scheduler-container">
    <!-- Header with view toggles -->
    <div class="stage-header-card">
        <h1><i class="fas fa-industry me-2"></i>Stage-Based Production Scheduler</h1>
        <div class="view-controls">
            <a href="/Scheduler" class="btn btn-outline-secondary">
                <i class="fas fa-calendar me-1"></i>Job View
            </a>
            <a href="/Operations/StageDashboard" class="btn btn-primary">
                <i class="fas fa-tasks me-1"></i>Stage View
            </a>
        </div>
    </div>
    
    <!-- Stage timeline visualization -->
    @foreach(var stageJob in Model.StageJobs)
    {
        <div class="stage-job-card">
            <div class="job-header">
                <h5>@stageJob.BaseJob.PartNumber</h5>
                <span class="job-machine">@stageJob.BaseJob.MachineId</span>
            </div>
            
            <div class="stage-timeline">
                @foreach(var stage in stageJob.StageBlocks)
                {
                    <div class="stage-block @stage.Status.ToLower()" 
                         style="background-color: @stage.StageColor">
                        <div class="stage-name">@stage.StageName</div>
                        <div class="stage-duration">@stage.EstimatedHours h</div>
                        
                        @if(stage.CanOperatorPunch)
                        {
                            <button class="punch-btn" onclick="punchInStage(@stageJob.BaseJob.Id, @stage.StageId)">
                                <i class="fas fa-play"></i>Punch In
                            </button>
                        }
                    </div>
                }
            </div>
        </div>
    }
</div>
```

### Phase 2: Individual Stage Dashboards (4 hours)

#### Prerequisites Commands:
```powershell
# Check existing operator layout
Get-Content "Pages/Shared/_OperatorLayout.cshtml" | Select-String "class" -Context 2

# Research PrintTracking structure for copying
Get-Content "Pages/PrintTracking/Index.cshtml" | Select-String "operator-dashboard|mobile-optimized" -Context 3
```

**Create stage-specific pages** (copy PrintTracking pattern):

#### 2.1 SLS Operations Dashboard 
- `/Operations/Stages/SLS` 
- Active builds, queued jobs, operator assignment
- Start/complete build functionality like PrintTracking

#### 2.2 CNC Operations Dashboard
- `/Operations/Stages/CNC`
- CNC job queue, machine assignment, progress tracking

#### 2.3 EDM Operations Dashboard 
- `/Operations/Stages/EDM`
- EDM job management with approval workflow (RequiresApproval=true)

#### 2.4 Assembly Operations Dashboard
- `/Operations/Stages/Assembly` 
- Final assembly workflow, quality checkpoints

#### 2.5 Finishing Dashboards
- `/Operations/Stages/Sandblasting`
- `/Operations/Stages/Coating`
- Surface treatment workflows

#### Step 2.1: Operator Punch Models (Hour 3)
```csharp
// NEW FILE: Models/OperatorPunch.cs
public class OperatorPunch
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public int StageId { get; set; }
    public string OperatorName { get; set; }
    public DateTime PunchTime { get; set; }
    public string PunchType { get; set; } // IN, OUT, PAUSE
    
    // Navigation properties
    public Job Job { get; set; } // REUSE existing Job model
    public ProductionStage Stage { get; set; } // REUSE existing ProductionStage
}

// NEW FILE: Models/StageProgress.cs  
public class StageProgress
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public int StageId { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public string Status { get; set; } // Pending, Active, Complete
    public string CurrentOperator { get; set; }
}
```

#### Step 2.2: Punch Service (Hour 3 continued)
```csharp
// NEW FILE: Services/OperatorPunchService.cs
public class OperatorPunchService
{
    private readonly SchedulerContext _context; // REUSE existing context
    private readonly ISchedulerService _schedulerService; // REUSE for validation
    
    public async Task<PunchResult> PunchInAsync(int jobId, int stageId, string operatorName)
    {
        // REUSE existing job validation
        var job = await _context.Jobs.FindAsync(jobId);
        if (job == null) return new PunchResult { Success = false, Message = "Job not found" };
        
        // NEW punch logic
        var punch = new OperatorPunch
        {
            JobId = jobId,
            StageId = stageId,
            OperatorName = operatorName,
            PunchTime = DateTime.UtcNow,
            PunchType = "IN"
        };
        
        _context.OperatorPunches.Add(punch);
        await _context.SaveChangesAsync();
        
        return new PunchResult { Success = true, Message = "Punched in successfully" };
    }
}
```

### Phase 3: Admin Approval Workflow (2 hours)

#### Prerequisites Commands:
```powershell
# Check existing approval infrastructure
sqlite3 scheduler.db "SELECT Name, RequiresApproval, RequiredRole FROM ProductionStages WHERE RequiresApproval = 1;"

# Check existing admin authorization
Get-Content "Pages/Admin" -Recurse | Select-String "Authorize.*Admin" | Select-Object -First 5
```

**Create**: `/Admin/StageApprovals` page

**Features**:
- List all stages requiring admin approval
- Approve/reject stage transitions
- Override operator restrictions
- Cross-stage progress monitoring

#### Step 3.1: Operator-Focused Interface (Hour 4)
```html
<!-- NEW FILE: Pages/Operations/Dashboard.cshtml -->
@page "/Operations/Dashboard"  
@model OpCentrix.Pages.Operations.DashboardModel

<!-- Mobile-optimized operator interface -->
<div class="operator-dashboard">
    <div class="operator-header">
        <h2>OPERATOR DASHBOARD</h2>
        <div class="current-operator">
            <strong>Current: </strong>
            <span id="current-operator">@Model.CurrentOperator</span>
        </div>
    </div>
    
    <!-- My Active Stages -->
    <div class="my-stages-section">
        <h3>MY ACTIVE STAGES</h3>
        @foreach(var stage in Model.MyActiveStages)
        {
            <div class="stage-card active">
                <div class="stage-info">
                    <h4>@stage.JobPartNumber - @stage.StageName</h4>
                    <p>Started: @stage.StartTime.ToString("HH:mm")</p>
                </div>
                <button class="btn btn-warning" onclick="punchOut(@stage.JobId, @stage.StageId)">
                    <i class="fas fa-stop"></i>Punch Out
                </button>
            </div>
        }
    </div>
    
    <!-- Available Stages -->
    <div class="available-stages-section">
        <h3>AVAILABLE STAGES</h3>
        @foreach(var stage in Model.AvailableStages)
        {
            <div class="stage-card available">
                <div class="stage-info">
                    <h4>@stage.JobPartNumber - @stage.StageName</h4>
                    <p>Machine: @stage.MachineId | Est: @stage.EstimatedHours h</p>
                </div>
                <button class="btn btn-success" onclick="punchIn(@stage.JobId, @stage.StageId)">
                    <i class="fas fa-play"></i>Punch In
                </button>
            </div>
        }
    </div>
</div>
```

### Phase 4: Navigation & Integration (1 hour)

#### Validation Commands:
```powershell
# Test final implementation
dotnet clean
dotnet restore
dotnet build OpCentrix/OpCentrix.csproj

# Verify database integrity after changes
sqlite3 scheduler.db "PRAGMA integrity_check;"
sqlite3 scheduler.db "PRAGMA foreign_key_check;"

# Check no compilation errors
if ($LASTEXITCODE -eq 0) {
    Write-Host "SUCCESS: Implementation successful"
} else {
    Write-Host "ERROR: Fix compilation errors"
}
```

#### Step 4.1: Navigation Integration (Hour 5)
```html
<!-- UPDATE: Pages/Shared/_Layout.cshtml - Add navigation links -->
<li class="nav-item dropdown">
    <a class="nav-link dropdown-toggle" href="#" id="schedulerDropdown" role="button" data-bs-toggle="dropdown">
        <i class="fas fa-calendar-alt"></i> Scheduling
    </a>
    <ul class="dropdown-menu">
        <li><a class="dropdown-item" href="/Scheduler">CALENDAR Job Scheduler</a></li>
        <li><a class="dropdown-item" href="/Operations/StageDashboard">FACTORY Stage Scheduler</a></li>
        <li><a class="dropdown-item" href="/Operations/Dashboard">PERSON Operator Dashboard</a></li>
    </ul>
</li>
```

---

## BENEFITS OF THIS APPROACH

### Risk Mitigation
- [CHECK] **Existing scheduler untouched** - No risk to working system
- [CHECK] **Existing parts system preserved** - Recent fixes maintained
- [CHECK] **Database models reused** - No schema changes needed
- [CHECK] **Service layer extended** - Build on proven foundation

### Development Efficiency  
- [CHECK] **50% less code** - Reuse existing validation, business logic
- [CHECK] **Proven patterns** - Build on working components
- [CHECK] **Parallel development** - Can work on new pages without conflicts
- [CHECK] **Easy rollback** - New pages can be disabled independently

### User Benefits
- [CHECK] **Familiar navigation** - Existing users keep current workflow
- [CHECK] **New capabilities** - Stage-aware scheduling available
- [CHECK] **Progressive adoption** - Can switch gradually
- [CHECK] **Mobile-optimized** - Operators get dedicated interface

---

## CRITICAL DO'S AND DON'TS

### [X] NEVER DO:
- Use `dotnet run` (freezes AI assistant)
- Use `&&` operators in PowerShell commands
- Use `<` redirection: `sqlite3 db < script.sql` (fails in PowerShell)
- Make database changes without backup
- Work outside OpCentrix directory
- Skip build verification after changes
- Batch multiple complex database changes
- **Use Unicode characters or emojis in any code or documentation**

### [CHECK] ALWAYS DO:
- Navigate to OpCentrix directory first: `cd OpCentrix`
- Create timestamped backup before ANY changes
- Use individual PowerShell commands (not chained)
- Use proper SQLite syntax: `sqlite3 db ".read script.sql"`
- Run `dotnet build` after each change
- Check database integrity: `sqlite3 db "PRAGMA integrity_check;"`
- Verify in correct directory with `pwd` command
- **Use only ASCII characters and standard symbols**

---

## SUCCESS CRITERIA

### Operator Experience
- [ ] Master dashboard shows visual progress across all 7 stages
- [ ] Each department has dedicated mobile-optimized dashboard
- [ ] Touch-friendly interface works on shop floor tablets
- [ ] Real-time job status updates without page refresh
- [ ] Clear indication of jobs requiring attention

### Admin Experience  
- [ ] Cross-stage oversight and bottleneck identification
- [ ] Approval workflow for stages requiring manager sign-off
- [ ] Progress tracking and performance metrics
- [ ] Ability to reassign jobs and override restrictions

### Manufacturing Workflow
- [ ] Clear 7-stage progression: SLS -> CNC -> EDM -> Laser -> Sandblasting -> Coating -> Assembly
- [ ] Jobs advance automatically when stages complete
- [ ] Admin approval gates where configured (EDM stage has RequiresApproval=true)
- [ ] Operator assignment and time tracking per stage
- [ ] Integration with existing job scheduling system

### New Functionality
- [ ] Stage-based scheduler shows individual manufacturing steps
- [ ] Operators can punch in/out of specific stages  
- [ ] Real-time stage progress tracking
- [ ] Mobile-optimized operator dashboard

### Existing System Protection
- [ ] Current `/Scheduler` page works exactly as before
- [ ] Parts management system unchanged
- [ ] All existing job creation/editing preserved
- [ ] Database integrity maintained

### Smart Integration
- [ ] Navigation between job view and stage view
- [ ] Shared data models and business logic
- [ ] Consistent design with existing OpCentrix style
- [ ] Performance optimized with proven patterns

---

## IMPLEMENTATION TIMELINE

| Phase | Duration | New Components | Reused Components |
|-------|----------|----------------|-------------------|
| **Phase 1** | 3 hours | `/Operations/StageDashboard` page, `StageSchedulingService` | `SchedulerService`, `Job` model, database context |
| **Phase 2** | 2 hours | `OperatorPunchService`, punch models | `SchedulerContext`, `ProductionStage` model |
| **Phase 3** | 2 hours | `/Operations/Dashboard` page | All existing validation and business logic |
| **Phase 4** | 1 hour | Navigation updates, integration | Existing layout and admin systems |

**Total: 8 hours with 70% reuse of existing stable components**

---

## READY TO IMPLEMENT

**Command Checklist Before Starting:**
```powershell
# 1. Verify environment
cd OpCentrix
pwd
Test-Path "scheduler.db"

# 2. Create backup
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
Copy-Item "scheduler.db" "../backup/database/scheduler_backup_$timestamp.db"

# 3. Verify current system
sqlite3 scheduler.db "SELECT COUNT(*) FROM ProductionStages WHERE IsActive = 1;"
dotnet build OpCentrix/OpCentrix.csproj

# 4. Start implementation if all checks pass
if ($LASTEXITCODE -eq 0) {
    Write-Host "SUCCESS: Ready to start Phase 1 - Master Stage Dashboard"
} else {
    Write-Host "ERROR: Fix build errors before starting"
}
```

**Ready to start with Phase 1 - Master Stage Dashboard?**

---

*Stage-Based Manufacturing Dashboard Plan*  
*Created: August 5, 2025*  
*Research: Complete database analysis + PowerShell protocols*  
*Risk Level: LOW (building on proven infrastructure)*  
*Character Set: ASCII ONLY - No Unicode allowed*