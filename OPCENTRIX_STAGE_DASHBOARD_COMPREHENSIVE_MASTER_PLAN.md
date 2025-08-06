# OpCentrix Stage Manufacturing Dashboard - COMPREHENSIVE MASTER PLAN

**Date**: August 5, 2025  
**Research Status**: COMPLETED - Database analysis of 43 tables  
**System Status**: ADVANCED MES WITH EXTENSIVE STAGE INFRASTRUCTURE  
**Character Set**: ASCII ONLY - No Unicode characters allowed  

---

## CRITICAL AI ASSISTANT WARNINGS & INSTRUCTIONS

### [WARN] MANDATORY RESEARCH PROTOCOL - READ FIRST

#### BEFORE ANY CODE CHANGES:
```powershell
# 1. ALWAYS use text_search to understand existing implementations
# 2. ALWAYS use get_file to verify current file contents  
# 3. NEVER assume file structure or contents
# 4. ALWAYS check for existing services before creating new ones
```

#### CONTEXT UNDERSTANDING REQUIREMENTS:
- **Search for existing implementations** before creating new files
- **Understand workspace structure** using find_files and get_projects_in_solution
- **Verify current state** of files before editing
- **Check for conflicting implementations** that might cause confusion

### [WARN] COMMON MISTAKES TO AVOID

#### 1. [X] NEVER CREATE DUPLICATE SERVICES
**WRONG APPROACH**:
```csharp
// DON'T create new IStageSchedulingService if similar exists
public interface IStageSchedulingService { ... }
```

**CORRECT APPROACH**:
```csharp
// EXTEND existing StageProgressionService instead
public static class StageProgressionServiceExtensions
{
    public static async Task<List<StageAwareJob>> GetStageAwareJobsAsync(
        this IStageProgressionService service, DateTime start, DateTime end)
    {
        // Extension method approach
    }
}
```

#### 2. [X] NEVER MODIFY WORKING PAGES WITHOUT RESEARCH
**WRONG**: Directly editing `/Admin/Parts.cshtml` without understanding current state
**CORRECT**: Research existing implementation with `get_file` before any changes

#### 3. [X] NEVER CREATE CONFLICTING PAGE ROUTES
**WRONG**: Creating `/Operations/StageDashboard` if similar page exists
**CORRECT**: Search for existing Operations pages first

#### 4. [X] NEVER IGNORE EXISTING DATABASE SCHEMA
**WRONG**: Creating new tables without checking existing ones
**CORRECT**: Use database research commands to understand current schema

### [WARN] WORKSPACE-SPECIFIC WARNINGS

#### Files That WILL CONFUSE IMPLEMENTATION:
```
[X] IGNORE THESE FILES COMPLETELY:
- OPCENTRIX_STAGE_SCHEDULER_COMPLETE_PLAN.md (Unicode version - superseded)
- MODERN_SCHEDULER_UI_BUILD_PLAN.md (different approach)
- Documentation-Cleanup/ folder (all superseded content)
- Any files in AppData/Local/Temp/ (temporary files)
- Phase-4-Manufacturing-Operations-Integration.md (conflicting strategy)
```

#### Files That MUST BE CHECKED FIRST:
```
[CHECK] VERIFY THESE BEFORE PROCEEDING:
- OpCentrix/Services/StageProgressionService.cs (already exists!)
- OpCentrix/Services/Admin/ProductionStageService.cs (already exists!)
- OpCentrix/Pages/PrintTracking/Index.cshtml.cs (template to copy!)
- OpCentrix/Data/SchedulerContext.cs (database context)
```

#### Existing Services That MUST BE REUSED:
```
[CHECK] THESE SERVICES ALREADY EXIST - EXTEND THEM:
- StageProgressionService: Automatic stage advancement
- ProductionStageService: Stage configuration (7 stages configured)
- PrintTrackingService: Mobile operator interface patterns
- CohortManagementService: Stage-aware cohort tracking
- MasterScheduleService: Comprehensive production visibility
```

### [WARN] COMMON IMPLEMENTATION ERRORS

#### Error 1: Creating New Instead of Extending
```csharp
// [X] WRONG - Creates duplicate functionality
public class StageSchedulingService : IStageSchedulingService
{
    // This duplicates existing StageProgressionService
}

// [CHECK] CORRECT - Extends existing service
public static class StageProgressionServiceExtensions
{
    public static async Task<List<StageAwareJob>> GetStageViewJobsAsync(
        this IStageProgressionService service, DateTime start, DateTime end)
    {
        // Builds on existing infrastructure
    }
}
```

#### Error 2: Ignoring Existing Database Structure
```sql
-- [X] WRONG - Creates duplicate tables
CREATE TABLE OperatorPunches (...)

-- [CHECK] CORRECT - Use existing tables
-- ProductionStageExecutions already handles stage tracking
-- JobStages already handles individual stage execution
```

#### Error 3: Breaking Existing Functionality
```csharp
// [X] WRONG - Modifies existing working code
public async Task OnGetAsync()
{
    // Changed existing scheduler logic - DANGEROUS
}

// [CHECK] CORRECT - Creates new page with own logic
public class StageDashboardModel : PageModel
{
    // New page, no risk to existing functionality
}
```

### [WARN] FORM VALIDATION AND ERROR HANDLING

#### Based on OpCentrix Form Validation System Research:
```javascript
// [CHECK] FOLLOW EXISTING PATTERNS FROM form-validation.js:
- Use OpCentrixFormValidator for consistent validation
- Implement real-time field validation
- Add auto-save functionality 
- Include unsaved changes warnings
- Use OpCentrixConfirmationDialog for user confirmations
```

#### Critical Form Validation Rules:
```javascript
// [CHECK] REQUIRED VALIDATION PATTERNS:
validatePartNumber: function(field, value, isBlur) {
    // Part number format: ABC-123 or ABC123 (3-50 characters)
    const partNumberPattern = /^[A-Z0-9][A-Z0-9\-_]{2,49}$/i;
    // Include duplicate checking via AJAX
}

validateMaterial: function(field, value) {
    // Cross-reference with material limits
    // Check laser power and scan speed compatibility
}
```

### [WARN] HTMX AND JAVASCRIPT INTEGRATION

#### Based on Parts System Research:
```html
<!-- [CHECK] FOLLOW EXISTING HTMX PATTERNS: -->
<button hx-get="/Operations/StageDashboard?handler=RefreshStages" 
        hx-target="#stage-content"
        hx-indicator="#loading-spinner">
    Refresh Stages
</button>

<!-- [X] AVOID COMPLEX JAVASCRIPT - USE HTMX WHERE POSSIBLE -->
```

#### Error Handling Patterns:
```javascript
// [CHECK] USE EXISTING ERROR HANDLING PATTERNS:
try {
    const response = await fetch(url, {
        method: 'GET',
        headers: {
            'Accept': 'text/html,application/xhtml+xml',
            'X-Requested-With': 'XMLHttpRequest',
            'HX-Request': 'true'
        },
        signal: controller.signal,
        credentials: 'same-origin'
    });
     
    if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }
    
} catch (error) {
    if (error.name === 'AbortError') {
        console.log('Request timeout - expected behavior');
    } else {
        this.showErrorModal('Request Failed', this.getErrorMessage(error));
    }
}
```

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
- **MasterScheduleService** - Comprehensive production visibility

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
- **What to Add**: Extension methods for stage dashboard functionality

### BUILD NEW: Stage-Focused Components

#### 1. NEW PAGE: Stage-Based Dashboard
**Path**: `/Operations/StageDashboard` (separate from existing `/Scheduler`)
**Benefits**: No risk to existing scheduler, clean implementation

#### 2. NEW PAGE: Operator Dashboard  
**Path**: `/Operations/Dashboard` (operator-focused interface)
**Benefits**: Mobile-optimized for shop floor, no conflicts

#### 3. NEW EXTENSIONS: Stage-Specific Logic
**New**: Extension methods for existing services instead of new interfaces
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

#### Step 1.1: Create New Stage Dashboard Controller (Hour 1)
```csharp
// NEW FILE: Pages/Operations/StageDashboard.cshtml.cs
public class StageDashboardModel : PageModel
{
    private readonly ISchedulerService _schedulerService; // REUSE existing
    private readonly ProductionStageService _stageService; // REUSE existing
    private readonly StageProgressionService _progressionService; // REUSE existing
    private readonly SchedulerContext _context; // REUSE existing
    
    public List<StageJob> StageJobs { get; set; } = new();
    public List<ProductionStage> ProductionStages { get; set; } = new();
    public string ViewMode { get; set; } = "stage"; // stage, machine, operator
    
    public async Task OnGetAsync(string? view = "stage")
    {
        ViewMode = view;
        
        // REUSE existing services
        ProductionStages = await _stageService.GetAllStagesAsync();
        var jobs = await LoadJobsFromExistingScheduler();
        
        // NEW stage breakdown logic using existing infrastructure
        StageJobs = await BuildStageJobsFromExistingData(jobs);
    }
    
    // NEW operator punch methods (extend existing functionality)
    public async Task<IActionResult> OnPostPunchInAsync(int jobId, int stageId, string operatorName)
    {
        // Use existing ProductionStageExecutions table
        // Follow existing PrintTracking patterns
        try
        {
            var execution = new ProductionStageExecution
            {
                JobId = jobId,
                ProductionStageId = stageId,
                OperatorName = operatorName,
                Status = "In Progress",
                ActualStartTime = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };
            
            _context.ProductionStageExecutions.Add(execution);
            await _context.SaveChangesAsync();
            
            return new JsonResult(new { success = true, message = "Punched in successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error punching in to stage {StageId} for job {JobId}", stageId, jobId);
            return new JsonResult(new { success = false, message = "Error punching in" });
        }
    }
}
```

#### Step 1.2: Stage View Helpers (Hour 1 continued)
```csharp
// NEW FILE: Extensions/StageProgressionServiceExtensions.cs
public static class StageProgressionServiceExtensions
{
    public static async Task<List<StageJob>> GetStageViewJobsAsync(
        this StageProgressionService service, 
        SchedulerContext context,
        DateTime start, 
        DateTime end)
    {
        // Extension method that builds on existing service
        // Uses existing database tables and relationships
        var jobs = await context.Jobs
            .Where(j => j.ScheduledStart >= start && j.ScheduledStart <= end)
            .Include(j => j.Part)
            .ThenInclude(p => p.PartStageRequirements)
            .ThenInclude(psr => psr.ProductionStage)
            .ToListAsync();
            
        return jobs.Select(job => new StageJob
        {
            BaseJob = job,
            RequiredStages = job.Part?.PartStageRequirements
                .Where(psr => psr.IsRequired)
                .Select(psr => psr.ProductionStage)
                .OrderBy(ps => ps.DisplayOrder)
                .ToList() ?? new List<ProductionStage>()
        }).ToList();
    }
}

// NEW FILE: ViewModels/StageDashboardViewModels.cs
public class StageJob
{
    public Job BaseJob { get; set; }
    public List<ProductionStage> RequiredStages { get; set; } = new();
    public List<ProductionStageExecution> StageExecutions { get; set; } = new();
    
    public string GetStageStatus(ProductionStage stage)
    {
        var execution = StageExecutions.FirstOrDefault(se => se.ProductionStageId == stage.Id);
        return execution?.Status ?? "Pending";
    }
    
    public double GetStageProgress(ProductionStage stage)
    {
        var execution = StageExecutions.FirstOrDefault(se => se.ProductionStageId == stage.Id);
        if (execution?.Status == "Completed") return 100.0;
        if (execution?.Status == "In Progress") return 50.0;
        return 0.0;
    }
}

public class OperatorStageView
{
    public string OperatorName { get; set; }
    public List<ProductionStageExecution> ActiveStages { get; set; } = new();
    public List<JobStage> AvailableStages { get; set; } = new();
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
    
    <!-- Stage overview cards -->
    <div class="stage-overview-grid">
        @foreach(var stage in Model.ProductionStages)
        {
            <div class="stage-overview-card" style="border-left: 4px solid @stage.StageColor">
                <div class="stage-header">
                    <h5>@stage.Name</h5>
                    <span class="badge" style="background-color: @stage.StageColor">@stage.Department</span>
                </div>
                <div class="stage-metrics">
                    <div class="metric">
                        <span class="metric-value">@GetActiveJobsCount(stage)</span>
                        <span class="metric-label">Active Jobs</span>
                    </div>
                    <div class="metric">
                        <span class="metric-value">@GetQueuedJobsCount(stage)</span>
                        <span class="metric-label">Queued</span>
                    </div>
                </div>
            </div>
        }
    </div>
    
    <!-- Stage timeline visualization -->
    @foreach(var stageJob in Model.StageJobs)
    {
        <div class="stage-job-card">
            <div class="job-header">
                <h5>@stageJob.BaseJob.PartNumber</h5>
                <span class="job-machine">@stageJob.BaseJob.MachineId</span>
                <span class="job-status badge @GetJobStatusClass(stageJob.BaseJob.Status)">
                    @stageJob.BaseJob.Status
                </span>
            </div>
            
            <div class="stage-timeline">
                @foreach(var stage in stageJob.RequiredStages)
                {
                    var stageStatus = stageJob.GetStageStatus(stage);
                    var progress = stageJob.GetStageProgress(stage);
                    
                    <div class="stage-block @stageStatus.ToLower()" 
                         style="background-color: @stage.StageColor">
                        <div class="stage-name">@stage.Name</div>
                        <div class="stage-duration">@stage.DefaultSetupMinutes min</div>
                        <div class="stage-progress">
                            <div class="progress-bar" style="width: @(progress)%"></div>
                        </div>
                        
                        @if(CanOperatorPunch(stageJob.BaseJob, stage))
                        {
                            <button class="punch-btn btn btn-sm btn-success" 
                                    hx-post="/Operations/StageDashboard?handler=PunchIn"
                                    hx-vals='{"jobId": @stageJob.BaseJob.Id, "stageId": @stage.Id, "operatorName": "@GetCurrentOperator()"}'
                                    hx-target="#stage-@stageJob.BaseJob.Id-@stage.Id"
                                    hx-indicator="#loading-spinner">
                                <i class="fas fa-play"></i>Punch In
                            </button>
                        }
                        else if(stageStatus == "In Progress")
                        {
                            <button class="punch-btn btn btn-sm btn-warning" 
                                    hx-post="/Operations/StageDashboard?handler=PunchOut"
                                    hx-vals='{"jobId": @stageJob.BaseJob.Id, "stageId": @stage.Id}'
                                    hx-target="#stage-@stageJob.BaseJob.Id-@stage.Id"
                                    hx-confirm="Are you sure you want to punch out of this stage?"
                                    hx-indicator="#loading-spinner">
                                <i class="fas fa-stop"></i>Punch Out
                            </button>
                        }
                    </div>
                }
            </div>
        </div>
    }
</div>

<!-- Loading spinner -->
<div id="loading-spinner" class="htmx-indicator">
    <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
    </div>
</div>

<!-- Use existing OpCentrixFormValidator for any forms -->
<script>
    // Follow existing JavaScript patterns from form-validation.js
    document.addEventListener('DOMContentLoaded', function() {
        if (window.OpCentrixFormValidator) {
            // Initialize existing validation system
            console.log('Stage dashboard using existing form validation');
        }
        
        // Use existing confirmation dialog patterns
        if (window.OpCentrixConfirmationDialog) {
            console.log('Stage dashboard using existing confirmation dialogs');
        }
    });
</script>

<style>
/* Use existing CSS patterns and extend them */
.stage-scheduler-container {
    padding: 20px;
}

.stage-header-card {
    background: white;
    border-radius: 8px;
    padding: 20px;
    margin-bottom: 20px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.stage-overview-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 15px;
    margin-bottom: 30px;
}

.stage-overview-card {
    background: white;
    border-radius: 8px;
    padding: 15px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.stage-job-card {
    background: white;
    border-radius: 8px;
    padding: 15px;
    margin-bottom: 15px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.stage-timeline {
    display: flex;
    gap: 10px;
    margin-top: 15px;
    overflow-x: auto;
    padding: 10px 0;
}

.stage-block {
    min-width: 150px;
    padding: 10px;
    border-radius: 6px;
    color: white;
    text-align: center;
    position: relative;
}

.stage-progress {
    margin-top: 5px;
    background: rgba(255,255,255,0.3);
    height: 4px;
    border-radius: 2px;
    overflow: hidden;
}

.progress-bar {
    height: 100%;
    background: rgba(255,255,255,0.8);
    transition: width 0.3s ease;
}

.punch-btn {
    margin-top: 8px;
    font-size: 12px;
}

/* Mobile responsiveness */
@media (max-width: 768px) {
    .stage-timeline {
        flex-direction: column;
    }
    
    .stage-block {
        min-width: auto;
    }
    
    .view-controls {
        flex-direction: column;
        gap: 10px;
    }
}
</style>
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
            <div class="stage-card active" id="stage-card-@stage.JobId">
                <div class="stage-info">
                    <h4>@stage.JobPartNumber - @stage.StageName</h4>
                    <p>Started: @stage.StartTime.ToString("HH:mm")</p>
                    <p>Estimated: @stage.EstimatedHours hours</p>
                </div>
                <button class="btn btn-warning" 
                        hx-post="/Operations/Dashboard?handler=PunchOut"
                        hx-vals='{"jobId": @stage.JobId, "stageId": @stage.StageId}'
                        hx-confirm="Are you sure you want to punch out of this stage?"
                        hx-target="#stage-card-@stage.JobId"
                        hx-indicator="#loading-spinner">
                    <i class="fas fa-stop"></i>Punch Out
                </button>
            </div>
        }
        
        @if(!Model.MyActiveStages.Any())
        {
            <div class="empty-state">
                <i class="fas fa-clock"></i>
                <p>No active stages. Select a job below to get started.</p>
            </div>
        }
    </div>
    
    <!-- Available Stages -->
    <div class="available-stages-section">
        <h3>AVAILABLE STAGES</h3>
        @foreach(var stage in Model.AvailableStages)
        {
            <div class="stage-card available" id="stage-card-@stage.JobId">
                <div class="stage-info">
                    <h4>@stage.JobPartNumber - @stage.StageName</h4>
                    <p>Machine: @stage.MachineId | Est: @stage.EstimatedHours h</p>
                    <p>Priority: @stage.Priority | Status: @stage.Status</p>
                </div>
                <button class="btn btn-success" 
                        hx-post="/Operations/Dashboard?handler=PunchIn"
                        hx-vals='{"jobId": @stage.JobId, "stageId": @stage.StageId}'
                        hx-target="#stage-card-@stage.JobId"
                        hx-indicator="#loading-spinner">
                    <i class="fas fa-play"></i>Punch In
                </button>
            </div>
        }
        
        @if(!Model.AvailableStages.Any())
        {
            <div class="empty-state">
                <i class="fas fa-check-circle"></i>
                <p>No available stages at this time.</p>
            </div>
        }
    </div>
</div>

<!-- Include existing validation and error handling -->
<script>
    // Use existing OpCentrixConfirmationDialog for confirmations
    // Follow existing HTMX patterns from PrintTracking
    // Include existing error handling from form-validation.js
    
    document.addEventListener('DOMContentLoaded', function() {
        // Auto-refresh every 30 seconds
        setInterval(function() {
            htmx.trigger('#operator-dashboard', 'refresh');
        }, 30000);
    });
</script>

<style>
/* Copy mobile-optimized styles from PrintTracking */
.operator-dashboard {
    padding: 15px;
    max-width: 800px;
    margin: 0 auto;
}

.operator-header {
    background: linear-gradient(135deg, #007bff, #0056b3);
    color: white;
    padding: 20px;
    border-radius: 8px;
    margin-bottom: 20px;
    text-align: center;
}

.stage-card {
    background: white;
    border-radius: 8px;
    padding: 15px;
    margin-bottom: 15px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    border-left: 4px solid #ddd;
}

.stage-card.active {
    border-left-color: #28a745;
}

.stage-card.available {
    border-left-color: #007bff;
}

.empty-state {
    text-align: center;
    color: #6c757d;
    padding: 40px 20px;
}

.empty-state i {
    font-size: 48px;
    margin-bottom: 15px;
}

/* Touch-friendly buttons for tablets */
.btn {
    min-height: 44px;
    min-width: 120px;
    font-size: 16px;
}

/* Mobile responsive */
@media (max-width: 768px) {
    .operator-dashboard {
        padding: 10px;
    }
    
    .stage-card {
        padding: 12px;
    }
    
    .btn {
        width: 100%;
        margin-top: 10px;
    }
}
</style>
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
        <li><a class="dropdown-item" href="/Scheduler">
            <i class="fas fa-calendar"></i> Job Scheduler
        </a></li>
        <li><a class="dropdown-item" href="/Operations/StageDashboard">
            <i class="fas fa-industry"></i> Stage Dashboard
        </a></li>
        <li><a class="dropdown-item" href="/Operations/Dashboard">
            <i class="fas fa-user-hard-hat"></i> Operator Dashboard
        </a></li>
    </ul>
</li>
```

#### Step 4.2: Service Registration
```csharp
// UPDATE: Program.cs - Register new services
builder.Services.AddScoped<ProductionStageService>(); // Already exists
builder.Services.AddScoped<StageProgressionService>(); // Already exists

// Add extension methods via static classes - no DI registration needed
```

---

## BENEFITS OF THIS APPROACH

### Risk Mitigation
- [CHECK] **Existing scheduler untouched** - No risk to working system
- [CHECK] **Existing parts system preserved** - Recent fixes maintained
- [CHECK] **Database models reused** - No schema changes needed
- [CHECK] **Service layer extended** - Build on proven foundation
- [CHECK] **Form validation patterns reused** - Consistent user experience

### Development Efficiency  
- [CHECK] **70% less code** - Reuse existing validation, business logic
- [CHECK] **Proven patterns** - Build on working components
- [CHECK] **Parallel development** - Can work on new pages without conflicts
- [CHECK] **Easy rollback** - New pages can be disabled independently
- [CHECK] **Mobile patterns copied** - PrintTracking interface proven

### User Benefits
- [CHECK] **Familiar navigation** - Existing users keep current workflow
- [CHECK] **New capabilities** - Stage-aware scheduling available
- [CHECK] **Progressive adoption** - Can switch gradually
- [CHECK] **Mobile-optimized** - Operators get dedicated interface
- [CHECK] **Real-time updates** - HTMX provides instant feedback

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
- **Create new services when existing ones can be extended**
- **Modify existing working pages without thorough research**
- **Ignore existing form validation patterns**

### [CHECK] ALWAYS DO:
- Navigate to OpCentrix directory first: `cd OpCentrix`
- Create timestamped backup before ANY changes
- Use individual PowerShell commands (not chained)
- Use proper SQLite syntax: `sqlite3 db ".read script.sql"`
- Run `dotnet build` after each change
- Check database integrity: `sqlite3 db "PRAGMA integrity_check;"`
- Verify in correct directory with `pwd` command
- **Use only ASCII characters and standard symbols**
- **Research existing implementations before creating new ones**
- **Follow existing form validation and error handling patterns**
- **Use existing HTMX patterns for real-time updates**
- **Extend existing services rather than creating duplicates**
- **Copy mobile patterns from PrintTracking for operator interfaces**

---

## SUCCESS CRITERIA

### Operator Experience
- [ ] Master dashboard shows visual progress across all 7 stages
- [ ] Each department has dedicated mobile-optimized dashboard
- [ ] Touch-friendly interface works on shop floor tablets
- [ ] Real-time job status updates without page refresh
- [ ] Clear indication of jobs requiring attention
- [ ] Punch in/out functionality for stage tracking

### Admin Experience  
- [ ] Cross-stage oversight and bottleneck identification
- [ ] Approval workflow for stages requiring manager sign-off
- [ ] Progress tracking and performance metrics
- [ ] Ability to reassign jobs and override restrictions
- [ ] Integration with existing admin tools

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
- [ ] Visual progress indicators for each job

### Existing System Protection
- [ ] Current `/Scheduler` page works exactly as before
- [ ] Parts management system unchanged
- [ ] All existing job creation/editing preserved
- [ ] Database integrity maintained
- [ ] Form validation patterns preserved

### Smart Integration
- [ ] Navigation between job view and stage view
- [ ] Shared data models and business logic
- [ ] Consistent design with existing OpCentrix style
- [ ] Performance optimized with proven patterns
- [ ] HTMX integration for real-time updates

---

## IMPLEMENTATION TIMELINE

| Phase | Duration | Deliverable | Commands Required |
|-------|----------|-------------|-------------------|
| **Phase 1** | 3 hours | Master Stage Dashboard | Research + Build + Test |
| **Phase 2** | 4 hours | Individual Stage Dashboards | Copy PrintTracking + Build |
| **Phase 3** | 2 hours | Admin Approval Workflow | Extend existing + Test |
| **Phase 4** | 1 hour | Navigation & Integration | Final build + validation |

**Total: 10 hours for complete stage-based manufacturing execution system**

### Component Breakdown:
| Phase | Duration | New Components | Reused Components |
|-------|----------|----------------|-------------------|
| **Phase 1** | 3 hours | `/Operations/StageDashboard` page, Extension methods | `ProductionStageService`, `Job` model, database context |
| **Phase 2** | 4 hours | Individual stage dashboards | PrintTracking patterns, existing layouts |
| **Phase 3** | 2 hours | `/Operations/Dashboard` page | All existing validation and business logic |
| **Phase 4** | 1 hour | Navigation updates, integration | Existing layout and admin systems |

**Total: 10 hours with 70% reuse of existing stable components**

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

*OpCentrix Stage Manufacturing Dashboard - Comprehensive Master Plan*  
*Created: August 5, 2025*  
*Research: Complete database analysis + PowerShell protocols + AI warnings*  
*Risk Level: LOW (building on proven infrastructure)*  
*Character Set: ASCII ONLY - No Unicode allowed*  
*AI Instructions: Comprehensive warnings and error prevention included*