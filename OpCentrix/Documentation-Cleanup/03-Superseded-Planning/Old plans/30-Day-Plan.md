OpCentrix Database Refactoring - Complete 30-Day Execution Plan
===============================================================
Generated: 2024-12-20
Developer: Solo Developer Implementation Guide
AI Assistant: Claude Sonnet 3.5 Prompts Included

WEEK 1: CRITICAL DATABASE FIXES & STABILIZATION
==============================================

DAY 1: Project Setup & Database Backup
--------------------------------------
Morning (2-3 hours):
1. Create project backup structure
   - Create folders: backup/database, backup/code, backup/migrations
   - Full project backup to version control
   - Document current database schema

2. Database backup tasks:
   # Create backup folder with timestamp
   $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
   New-Item -ItemType Directory -Path "backup\database\$timestamp" -Force
   
   # Backup current database
   Copy-Item "opcentrix.db" "backup\database\$timestamp\opcentrix.db"
   Copy-Item "scheduler.db" "backup\database\$timestamp\scheduler.db" -ErrorAction SilentlyContinue
   
   # Export current schema
   sqlite3 opcentrix.db .schema > "backup\database\$timestamp\schema.sql"

3. Document current issues:
   - Run application and note all errors
   - Test each admin page and document failures
   - Create "current-issues.md" with screenshots

Afternoon (3-4 hours):
4. Analyze database structure
   - Open database in DB Browser for SQLite
   - Document actual table names and columns
   - Compare with expected model structure

5. Create migration baseline
   # Remove existing migrations if corrupted
   Remove-Item Migrations\* -Recurse -Force
   
   # Create fresh initial migration
   dotnet ef migrations add InitialBaseline
   
   # Generate SQL script for review
   dotnet ef migrations script -o "backup\database\initial_schema.sql"

CLAUDE PROMPT FOR DAY 1:
"I need to analyze my SQLite database schema and Entity Framework models for inconsistencies. Here's my current schema: [paste schema.sql]. Here are my model classes: [paste Machine.cs, Part.cs, Job.cs]. Please identify all mismatches between the database schema and models, and provide a prioritized list of fixes needed."

DAY 2: Fix Machine Table Naming Issues
--------------------------------------
Morning (3-4 hours):
1. Update all model classes for Machine entity
   - Rename SlsMachine to Machine everywhere
   - Update navigation properties
   - Fix property names (Name vs MachineName)

2. Update DbContext:
   // In SchedulerContext.cs
   public DbSet<Machine> Machines { get; set; }  // NOT SlsMachines

3. Create migration for table standardization:
   dotnet ef migrations add StandardizeMachineNaming

Afternoon (3-4 hours):
4. Update all service references:
   - Search entire solution for "SlsMachine"
   - Update AdminDashboardService.cs
   - Update SchedulerService.cs
   - Fix all LINQ queries

5. Test and verify:
   # Build and check for compilation errors
   dotnet build
   
   # Run specific service tests
   dotnet test --filter "FullyQualifiedName~Machine"

CLAUDE PROMPT FOR DAY 2:
"I need to refactor all references from 'SlsMachine' to 'Machine' in my ASP.NET Core application. Here's my current service code: [paste AdminDashboardService.cs]. Please show me the corrected version with all SlsMachine references updated to Machine, including LINQ queries and navigation properties."

DAY 3: Fix Foreign Key Type Mismatches
---------------------------------------
Morning (4 hours):
1. Update MachineCapability model:
   public class MachineCapability
   {
       public int Id { get; set; }
       public string MachineId { get; set; }  // Changed from int SlsMachineId
       public string CapabilityName { get; set; }
       public string CapabilityValue { get; set; }
       public virtual Machine Machine { get; set; }
   }

2. Create foreign key fix migration:
   dotnet ef migrations add FixForeignKeyTypes

3. Manually adjust migration if needed for data preservation

Afternoon (3 hours):
4. Update all related queries and services
5. Fix form bindings that reference old property names
6. Test machine capability CRUD operations

CLAUDE PROMPT FOR DAY 3:
"I need to change a foreign key from INT to STRING in Entity Framework Core with SQLite. Current: MachineCapabilities.SlsMachineId (INT). Target: MachineCapabilities.MachineId (STRING). Please provide a migration that preserves existing data and updates all foreign key constraints."

DAY 4: Add Missing Required Fields
-----------------------------------
Morning (3-4 hours):
1. Update Part model with Name field:
   [Required]
   [MaxLength(200)]
   public string Name { get; set; }

2. Add missing Job fields:
   public int? EstimatedDuration { get; set; }  // In minutes

3. Add Machine.BuildVolumeM3:
   public decimal? BuildVolumeM3 { get; set; }

Afternoon (3-4 hours):
4. Create migration for missing fields:
   dotnet ef migrations add AddMissingRequiredFields

5. Update forms to include new fields
6. Test data entry for all new fields

CLAUDE PROMPT FOR DAY 4:
"I need to add missing required fields to my Entity Framework models and create a migration. Fields needed: Parts.Name (Required, MaxLength 200), Jobs.EstimatedDuration (nullable int), Machines.BuildVolumeM3 (nullable decimal). Please show me the updated models and the migration code that sets default values for existing records."

DAY 5: Fix JavaScript and Form Bindings
----------------------------------------
Morning (3-4 hours):
1. Fix Parts form material selection:
   - Update _PartForm.cshtml JavaScript
   - Fix "SslMaterial" typo to "SlsMaterial"
   - Test material auto-fill functionality

2. Fix Job modal duration calculation:
   - Update scheduler-ui.js
   - Change from AvgDurationDays to EstimatedHours
   - Test duration calculations

Afternoon (3-4 hours):
3. Fix form field bindings:
   - Update StartTime/EndTime to ScheduledStart/ScheduledEnd
   - Fix MaterialCost vs MaterialCostPerUnit
   - Test all form submissions

CLAUDE PROMPT FOR DAY 5:
"I have JavaScript code that references wrong field IDs. In _PartForm.cshtml, it looks for 'SslMaterial' but should be 'SlsMaterial'. In scheduler-ui.js, it uses 'AvgDurationDays' but should calculate from 'EstimatedHours'. Please help me fix these JavaScript references and ensure proper form field bindings."

WEEK 2: PERFORMANCE OPTIMIZATION & LOGIC FIXES
==============================================

DAY 6: Add Performance Indexes
-------------------------------
Morning (3 hours):
1. Create performance indexes migration:
   dotnet ef migrations add AddPerformanceIndexes

2. Add indexes for:
   - Jobs.PartNumber
   - Jobs.MachineId
   - Jobs.Status
   - Jobs.ScheduledStart
   - Composite: Jobs(MachineId, ScheduledStart)
   - Parts.IsActive
   - Parts.Material

Afternoon (3 hours):
3. Apply migration and test performance:
   dotnet ef database update
   
4. Run performance tests on:
   - Job listing queries
   - Part searches
   - Machine scheduling
   - Overlap detection

CLAUDE PROMPT FOR DAY 6:
"I need to add performance indexes to my SQLite database using Entity Framework Core. Tables needing indexes: Jobs (PartNumber, MachineId, Status, ScheduledStart), Parts (IsActive, Material). Please show me the migration code and explain which queries will benefit from each index."

DAY 7: Standardize Audit Fields
--------------------------------
Morning (3 hours):
1. Update all models with consistent audit fields:
   public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
   public string CreatedBy { get; set; } = "System";
   public DateTime? LastModifiedDate { get; set; }
   public string? LastModifiedBy { get; set; }

2. Create IAuditable interface:
   public interface IAuditable
   {
       DateTime CreatedDate { get; set; }
       string CreatedBy { get; set; }
       DateTime? LastModifiedDate { get; set; }
       string? LastModifiedBy { get; set; }
   }

Afternoon (3 hours):
3. Update DbContext to auto-populate audit fields
4. Create migration for audit standardization
5. Test audit trail functionality

CLAUDE PROMPT FOR DAY 7:
"I need to standardize audit fields across all my Entity Framework models. Some tables use CreatedDate/CreatedBy, others use CreationDate/CreatedByUser. Please help me create an IAuditable interface and update my DbContext to automatically populate these fields on SaveChanges."

DAY 8: Fix UTC Time Standardization
------------------------------------
Morning (3 hours):
1. Find all DateTime.Today usage:
   - Replace with DateTime.UtcNow.Date
   - Update time comparisons
   - Fix date range queries

2. Update SchedulerService.cs:
   - Use UTC for all calculations
   - Add timezone conversion for display

Afternoon (3 hours):
3. Update frontend date handling:
   - Convert UTC to local time for display
   - Send local time as UTC to backend
   - Test across different timezones

CLAUDE PROMPT FOR DAY 8:
"I need to standardize all datetime handling to UTC in my ASP.NET Core application. Currently mixing DateTime.Today and DateTime.UtcNow. Please show me how to: 1) Update all backend code to use UTC, 2) Convert to local time for display in Razor views, 3) Handle timezone conversion in JavaScript."

DAY 9: Implement Concurrent Edit Protection
-------------------------------------------
Morning (4 hours):
1. Add RowVersion to all entities:
   [Timestamp]
   public byte[] RowVersion { get; set; }

2. Update DbContext configuration:
   modelBuilder.Entity<Job>()
       .Property(j => j.RowVersion)
       .IsRowVersion();

3. Create migration for concurrency tokens

Afternoon (3 hours):
4. Implement concurrency exception handling:
   - Add try-catch for DbUpdateConcurrencyException
   - Create conflict resolution UI
   - Test concurrent edits

CLAUDE PROMPT FOR DAY 9:
"I need to implement optimistic concurrency control in Entity Framework Core with SQLite. Please show me how to: 1) Add RowVersion to my models, 2) Configure it in DbContext, 3) Handle DbUpdateConcurrencyException with retry logic and user notification."

DAY 10: Fix Job Validation Performance
--------------------------------------
Morning (3 hours):
1. Optimize overlap validation query:
   - Filter by machine and date range
   - Use indexed columns
   - Cache validation results

2. Update ValidateJobOverlap method:
   var overlappingJobs = await _context.Jobs
       .Where(j => j.MachineId == machineId &&
                   j.Id != jobId &&
                   j.ScheduledStart < proposedEnd &&
                   j.ScheduledEnd > proposedStart)
       .AnyAsync();

Afternoon (3 hours):
3. Add validation caching:
   - Cache machine schedules per session
   - Invalidate cache on changes
   - Test performance improvements

CLAUDE PROMPT FOR DAY 10:
"I have a job overlap validation query that loads ALL jobs for validation. Please help me optimize it to only check relevant jobs by machine and time window. Current query: [paste current query]. Also show me how to implement simple caching for validation results."

WEEK 3: BUSINESS LOGIC & UI IMPROVEMENTS
========================================

DAY 11: Implement Duration Display Logic
----------------------------------------
Morning (4 hours):
1. Create BusinessDaysCalculator service:
   - Calculate business days between dates
   - Exclude weekends
   - Handle holidays

2. Add holiday configuration:
   - Create Holiday model
   - Add holiday management UI
   - Store company holidays

Afternoon (3 hours):
3. Standardize duration display:
   - Create DurationFormatter helper
   - Display as "X days, Y hours"
   - Update all duration displays

CLAUDE PROMPT FOR DAY 11:
"I need to create a BusinessDaysCalculator service for my ASP.NET Core app that calculates working days between two dates, excluding weekends and holidays. Please provide the service implementation with methods for: 1) Calculate business days, 2) Add business days to a date, 3) Format duration display."

DAY 12: Implement Delete Cascade Protection
--------------------------------------------
Morning (3 hours):
1. Add referential integrity checks:
   - Check for active jobs before deleting parts
   - Check for scheduled jobs before deleting machines
   - Implement soft delete pattern

2. Update delete methods:
   public async Task<bool> CanDeletePartAsync(string partNumber)
   {
       return !await _context.Jobs
           .Where(j => j.PartNumber == partNumber && 
                      j.Status != "Completed" && 
                      j.Status != "Cancelled")
           .AnyAsync();
   }

Afternoon (3 hours):
3. Update UI with delete warnings:
   - Show count of dependent records
   - Require confirmation
   - Suggest soft delete option

CLAUDE PROMPT FOR DAY 12:
"I need to implement cascade delete protection in my application. Before deleting a Part, I need to check if there are active Jobs using it. Please show me how to: 1) Create validation methods, 2) Implement soft delete with IsActive flag, 3) Add appropriate UI warnings."

DAY 13: Create Job Status State Machine
----------------------------------------
Morning (4 hours):
1. Define valid status transitions:
   Scheduled -> InProgress, Cancelled
   InProgress -> Completed, OnHold, Cancelled
   OnHold -> InProgress, Cancelled
   Completed -> (no transitions)
   Cancelled -> (no transitions)

2. Create JobStatusStateMachine class:
   public class JobStatusStateMachine
   {
       private static readonly Dictionary<string, List<string>> ValidTransitions = new()
       {
           ["Scheduled"] = new() { "InProgress", "Cancelled" },
           ["InProgress"] = new() { "Completed", "OnHold", "Cancelled" },
           ["OnHold"] = new() { "InProgress", "Cancelled" },
           ["Completed"] = new(),
           ["Cancelled"] = new()
       };
       
       public static bool CanTransition(string from, string to)
       {
           return ValidTransitions.ContainsKey(from) && 
                  ValidTransitions[from].Contains(to);
       }
   }

Afternoon (2 hours):
3. Implement status change logging:
   - Create StatusChangeLog model
   - Log all transitions with timestamp
   - Add status history view

CLAUDE PROMPT FOR DAY 13:
"I need to implement a state machine for job status transitions in C#. Valid transitions: Scheduled->InProgress/Cancelled, InProgress->Completed/OnHold/Cancelled, etc. Please create a JobStatusStateMachine class with validation methods and status change logging."

DAY 14: Fix N+1 Query Problems
-------------------------------
Morning (3 hours):
1. Add Include statements to queries:
   var jobs = await _context.Jobs
       .Include(j => j.Part)
       .Include(j => j.Machine)
       .Where(j => j.ScheduledStart >= startDate)
       .ToListAsync();

2. Create query specifications:
   public class JobWithDetailsSpec : ISpecification<Job>
   {
       public IQueryable<Job> Apply(IQueryable<Job> query)
       {
           return query
               .Include(j => j.Part)
               .Include(j => j.Machine)
               .ThenInclude(m => m.Capabilities);
       }
   }

Afternoon (3 hours):
3. Implement projection for read operations:
   - Create DTOs for read-only views
   - Use Select() for projections
   - Test query performance

CLAUDE PROMPT FOR DAY 14:
"I have N+1 query problems in my Entity Framework application. Please show me how to: 1) Add proper Include statements for eager loading, 2) Create a specification pattern for reusable queries, 3) Use projections with Select() for read-only operations."

DAY 15: Implement Material Changeover Logic
--------------------------------------------
Morning (3 hours):
1. Add material compatibility matrix:
   - Create MaterialCompatibility model
   - Define changeover times between materials
   - Add configuration UI

2. Update scheduling logic:
   public int CalculateChangeoverTime(string fromMaterial, string toMaterial)
   {
       if (fromMaterial == toMaterial) return 0;
       
       var changeover = _context.MaterialCompatibilities
           .FirstOrDefault(mc => mc.FromMaterial == fromMaterial && 
                                mc.ToMaterial == toMaterial);
                                
       return changeover?.ChangeoverMinutes ?? DefaultChangeoverTime;
   }

Afternoon (3 hours):
3. Update cost calculations:
   - Include changeover time in costs
   - Add changeover to job duration
   - Test scheduling with changeovers

CLAUDE PROMPT FOR DAY 15:
"I need to implement material changeover logic for my scheduling system. When scheduling jobs with different materials consecutively, I need to add changeover time. Please help me create: 1) MaterialCompatibility model, 2) Changeover time calculation, 3) Updated scheduling logic."

WEEK 4: UI/UX IMPROVEMENTS & TESTING
====================================

DAY 16: Fix Modal State Management
----------------------------------
Morning (3 hours):
1. Update modal JavaScript:
   function resetJobModal() {
       $('#jobForm')[0].reset();
       $('.validation-message').remove();
       $('#jobId').val('0');
       updateEndTime(); // Recalculate
   }
   
   $('#addJobModal').on('show.bs.modal', function(e) {
       if ($(e.relatedTarget).data('job-id') === undefined) {
           resetJobModal();
       }
   });

2. Add unsaved changes warning:
   var formChanged = false;
   $('#jobForm :input').on('change', function() {
       formChanged = true;
   });
   
   window.onbeforeunload = function() {
       if (formChanged) {
           return "You have unsaved changes. Are you sure?";
       }
   };

Afternoon (3 hours):
3. Improve validation display:
   - Replace alert() with inline messages
   - Add field-level validation
   - Style error states

CLAUDE PROMPT FOR DAY 16:
"I need to fix modal state management in my application. The form data persists between operations. Please help me: 1) Reset form when opening for new record, 2) Preserve data during validation errors, 3) Add unsaved changes warning, 4) Replace alert() with better validation display."

DAY 17: Fix Grid Positioning at Zoom Levels
--------------------------------------------
Morning (4 hours):
1. Implement responsive grid calculations:
   function calculateJobPosition(startTime, endTime, dayStart, pixelsPerHour) {
       const startOffset = (startTime - dayStart) * pixelsPerHour;
       const duration = (endTime - startTime) * pixelsPerHour;
       const minWidth = 60; // Minimum job width
       
       return {
           left: Math.max(0, startOffset),
           width: Math.max(minWidth, duration)
       };
   }

2. Add collision detection:
   function detectCollisions(jobs) {
       const rows = [];
       jobs.forEach(job => {
           let placed = false;
           for (let row of rows) {
               if (!hasOverlap(job, row)) {
                   row.push(job);
                   placed = true;
                   break;
               }
           }
           if (!placed) {
               rows.push([job]);
           }
       });
       return rows;
   }

Afternoon (2 hours):
3. Implement job stacking:
   - Stack overlapping jobs vertically
   - Add row height calculations
   - Test at different zoom levels

CLAUDE PROMPT FOR DAY 17:
"I need to fix job positioning in my scheduler grid that breaks at different zoom levels. Jobs overlap visually. Please help me: 1) Calculate positions based on zoom level, 2) Implement collision detection, 3) Stack overlapping jobs vertically with proper spacing."

DAY 18: Create Comprehensive Cost Calculation
----------------------------------------------
Morning (4 hours):
1. Create CostCalculationService:
   public class CostCalculationService
   {
       public decimal CalculateJobCost(Job job, Part part, Machine machine)
       {
           var materialCost = part.MaterialCostPerUnit * job.Quantity;
           var machineCost = machine.HourlyRate * (job.EstimatedDuration / 60m);
           var setupCost = machine.SetupCost;
           var laborCost = CalculateLaborCost(job);
           var overheadRate = 0.15m; // 15% overhead
           
           var totalDirectCost = materialCost + machineCost + setupCost + laborCost;
           var totalCost = totalDirectCost * (1 + overheadRate);
           
           return totalCost;
       }
   }

2. Add cost configuration:
   - Machine hourly rates
   - Setup costs
   - Labor rates by skill level
   - Overhead percentages

Afternoon (2 hours):
3. Create cost approval workflow:
   - Add cost thresholds
   - Require approval for high-cost jobs
   - Add cost reporting

CLAUDE PROMPT FOR DAY 18:
"I need to create a comprehensive cost calculation service for manufacturing jobs. Currently only calculating material costs. Please help me implement: 1) Full cost model (material, machine, labor, overhead), 2) Configuration system for rates, 3) Cost approval workflow based on thresholds."

DAY 19: Implement HTMX Response Handling
-----------------------------------------
Morning (3 hours):
1. Update controller actions for HTMX:
   public async Task<IActionResult> OnPostAddJobAsync(Job job)
   {
       if (!ModelState.IsValid)
       {
           if (Request.Headers["HX-Request"] == "true")
           {
               return Partial("_JobForm", job);
           }
           return Page();
       }
       
       await _context.Jobs.AddAsync(job);
       await _context.SaveChangesAsync();
       
       if (Request.Headers["HX-Request"] == "true")
       {
           return Partial("_MachineRow", await GetMachineRowData(job.MachineId));
       }
       
       return RedirectToPage();
   }

2. Update HTMX attributes:
   - Set proper hx-target
   - Use hx-swap strategies
   - Add hx-indicator for loading

Afternoon (3 hours):
3. Remove page reload JavaScript:
   - Replace location.reload() with HTMX swaps
   - Update success handlers
   - Test all CRUD operations

CLAUDE PROMPT FOR DAY 19:
"I need to properly implement HTMX response handling in my Razor Pages application. Currently doing full page refreshes after CRUD operations. Please show me how to: 1) Return partial views for HTMX requests, 2) Update hx-target attributes correctly, 3) Implement proper swap strategies."

DAY 20: Add Session Timeout Handling
-------------------------------------
Morning (3 hours):
1. Implement session timeout warning:
   let warningTimer;
   let timeoutTimer;
   const warningTime = 18 * 60 * 1000; // 18 minutes
   const timeoutTime = 20 * 60 * 1000; // 20 minutes
   
   function resetTimers() {
       clearTimeout(warningTimer);
       clearTimeout(timeoutTimer);
       
       warningTimer = setTimeout(showWarning, warningTime);
       timeoutTimer = setTimeout(forceLogout, timeoutTime);
   }
   
   function showWarning() {
       $('#sessionWarningModal').modal('show');
   }

2. Add auto-save functionality:
   function autoSaveDraft() {
       const formData = $('#jobForm').serialize();
       localStorage.setItem('jobDraft', formData);
   }
   
   setInterval(autoSaveDraft, 30000); // Every 30 seconds

Afternoon (3 hours):
3. Implement session refresh:
   - Add keep-alive endpoint
   - Refresh session on user activity
   - Test timeout scenarios

CLAUDE PROMPT FOR DAY 20:
"I need to implement session timeout handling with warning for my web application. Requirements: 1) Show warning before timeout, 2) Auto-save draft data, 3) Allow session refresh, 4) Graceful redirect to login on timeout. Please provide JavaScript implementation."

WEEK 5: TESTING & DEPLOYMENT PREPARATION
========================================

DAY 21: Create Unit Tests for Business Logic
---------------------------------------------
Morning (4 hours):
1. Set up test project:
   dotnet new xunit -n OpCentrix.Tests
   dotnet add reference ../OpCentrix/OpCentrix.csproj
   dotnet add package Moq
   dotnet add package FluentAssertions

2. Create tests for critical services:
   [Fact]
   public async Task ValidateJobOverlap_ShouldReturnTrue_WhenJobsOverlap()
   {
       // Arrange
       var context = GetInMemoryContext();
       var service = new SchedulerService(context);
       
       // Add test data
       await context.Jobs.AddAsync(new Job 
       { 
           MachineId = "M1",
           ScheduledStart = new DateTime(2024, 1, 1, 10, 0, 0),
           ScheduledEnd = new DateTime(2024, 1, 1, 12, 0, 0)
       });
       await context.SaveChangesAsync();
       
       // Act
       var hasOverlap = await service.ValidateJobOverlap(
           "M1", 
           new DateTime(2024, 1, 1, 11, 0, 0),
           new DateTime(2024, 1, 1, 13, 0, 0),
           0);
       
       // Assert
       hasOverlap.Should().BeTrue();
   }

Afternoon (3 hours):
3. Test business rules:
   - Job status transitions
   - Cost calculations
   - Duration calculations
   - Material changeover logic

CLAUDE PROMPT FOR DAY 21:
"I need to create unit tests for my SchedulerService using xUnit, Moq, and FluentAssertions. Please show me how to: 1) Set up in-memory database for testing, 2) Test job overlap validation, 3) Test status transitions, 4) Mock dependencies properly."

DAY 22: Create Integration Tests
---------------------------------
Morning (4 hours):
1. Set up integration test base:
   public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
   {
       protected readonly WebApplicationFactory<Program> _factory;
       protected readonly HttpClient _client;
       
       public IntegrationTestBase(WebApplicationFactory<Program> factory)
       {
           _factory = factory;
           _client = _factory.WithWebHostBuilder(builder =>
           {
               builder.ConfigureServices(services =>
               {
                   // Use in-memory database
                   var descriptor = services.SingleOrDefault(
                       d => d.ServiceType == typeof(DbContextOptions<SchedulerContext>));
                   services.Remove(descriptor);
                   
                   services.AddDbContext<SchedulerContext>(options =>
                   {
                       options.UseInMemoryDatabase("TestDb");
                   });
               });
           }).CreateClient();
       }
   }

2. Test API endpoints:
   [Fact]
   public async Task CreateJob_ShouldReturnSuccess_WithValidData()
   {
       // Arrange
       var job = new Job { /* valid data */ };
       
       // Act
       var response = await _client.PostAsJsonAsync("/api/jobs", job);
       
       // Assert
       response.StatusCode.Should().Be(HttpStatusCode.OK);
   }

Afternoon (3 hours):
3. Test complete workflows:
   - User login -> Create job -> Update status -> Complete
   - Create part -> Schedule jobs -> Validate conflicts
   - Run reports -> Export data

CLAUDE PROMPT FOR DAY 22:
"I need to create integration tests for my ASP.NET Core Razor Pages application. Please show me how to: 1) Set up WebApplicationFactory with in-memory database, 2) Test complete user workflows, 3) Handle authentication in tests, 4) Verify database state after operations."

DAY 23: Performance Testing
---------------------------
Morning (4 hours):
1. Set up performance benchmarks:
   dotnet add package BenchmarkDotNet
   
   [MemoryDiagnoser]
   public class SchedulerBenchmarks
   {
       private SchedulerService _service;
       private SchedulerContext _context;
       
       [GlobalSetup]
       public void Setup()
       {
           var options = new DbContextOptionsBuilder<SchedulerContext>()
               .UseInMemoryDatabase("PerfTest")
               .Options;
           _context = new SchedulerContext(options);
           _service = new SchedulerService(_context);
           
           // Add test data
           SeedTestData();
       }
       
       [Benchmark]
       public async Task GetMachineSchedule()
       {
           await _service.GetMachineSchedulesAsync(
               DateTime.Today, 
               DateTime.Today.AddDays(7));
       }
   }

2. Run benchmarks:
   dotnet run -c Release

Afternoon (3 hours):
3. Load testing:
   - Use NBomber or k6 for load testing
   - Test concurrent user scenarios
   - Identify bottlenecks

CLAUDE PROMPT FOR DAY 23:
"I need to set up performance testing for my scheduling application using BenchmarkDotNet. Please show me how to: 1) Create benchmarks for key operations, 2) Test with realistic data volumes, 3) Measure memory allocation, 4) Set up load testing for concurrent users."

DAY 24: Security Testing
------------------------
Morning (4 hours):
1. Security audit checklist:
   - SQL injection prevention
   - XSS protection
   - CSRF tokens
   - Authentication/Authorization
   - Input validation
   - Error handling

2. Test authorization:
   [Fact]
   public async Task AdminEndpoint_ShouldReturn401_ForUnauthenticatedUser()
   {
       // Act
       var response = await _client.GetAsync("/Admin/Users");
       
       // Assert
       response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
   }

Afternoon (3 hours):
3. Penetration testing basics:
   - Test input validation
   - Try SQL injection patterns
   - Test file upload security
   - Verify sensitive data protection

CLAUDE PROMPT FOR DAY 24:
"I need to perform security testing on my ASP.NET Core application. Please provide: 1) Security testing checklist, 2) Tests for common vulnerabilities (SQL injection, XSS, CSRF), 3) Authorization testing patterns, 4) Input validation test cases."

DAY 25: Create User Documentation
----------------------------------
Morning (4 hours):
1. Create user guide structure:
   - Getting Started
   - Dashboard Overview
   - Managing Parts
   - Scheduling Jobs
   - Reports and Analytics
   - Troubleshooting

2. Document key workflows:
   ## Scheduling a New Job
   1. Navigate to Scheduler page
   2. Click "Add Job" button
   3. Select Part from dropdown
   4. Choose Machine and time slot
   5. Enter quantity
   6. Click "Save"
   
   ### Tips:
   - Jobs automatically calculate end time
   - Red blocks indicate conflicts
   - Use drag-and-drop to reschedule

Afternoon (3 hours):
3. Create administrator guide:
   - System configuration
   - User management
   - Database maintenance
   - Backup procedures

CLAUDE PROMPT FOR DAY 25:
"I need to create user documentation for my manufacturing scheduler application. Please help me create: 1) Table of contents structure, 2) Step-by-step guide for scheduling jobs, 3) Screenshots annotations guide, 4) Common troubleshooting scenarios."

WEEK 6: DEPLOYMENT & OPTIMIZATION
=================================

DAY 26: Deployment Preparation
------------------------------
Morning (4 hours):
1. Update appsettings.Production.json:
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=opcentrix.db"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Warning",
         "Microsoft": "Warning"
       }
     },
     "AllowedHosts": "*"
   }

2. Set up deployment scripts:
   # deploy.ps1
   dotnet publish -c Release -o ./publish
   
   # Create deployment package
   Compress-Archive -Path ./publish/* -DestinationPath "OpCentrix_$(Get-Date -Format 'yyyyMMdd').zip"

Afternoon (3 hours):
3. Configure IIS/Linux deployment:
   - Install hosting bundle
   - Configure application pool
   - Set up reverse proxy (if Linux)
   - Configure SSL

CLAUDE PROMPT FOR DAY 26:
"I need to prepare my ASP.NET Core application for production deployment. Please provide: 1) Production configuration checklist, 2) Deployment script for Windows/Linux, 3) IIS configuration steps, 4) Environment-specific settings management."

DAY 27: Implement Logging & Monitoring
--------------------------------------
Morning (4 hours):
1. Enhance Serilog configuration:
   Log.Logger = new LoggerConfiguration()
       .MinimumLevel.Debug()
       .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
       .Enrich.FromLogContext()
       .Enrich.WithMachineName()
       .Enrich.WithEnvironmentName()
       .WriteTo.File(
           path: "logs/opcentrix-.log",
           rollingInterval: RollingInterval.Day,
           retainedFileCountLimit: 30,
           fileSizeLimitBytes: 10_000_000,
           rollOnFileSizeLimit: true)
       .WriteTo.Seq("http://localhost:5341") // If using Seq
       .CreateLogger();

2. Add performance logging:
   public class PerformanceLoggingMiddleware
   {
       public async Task InvokeAsync(HttpContext context, RequestDelegate next)
       {
           var sw = Stopwatch.StartNew();
           
           await next(context);
           
           sw.Stop();
           if (sw.ElapsedMilliseconds > 500)
           {
               _logger.LogWarning("Slow request: {Method} {Path} took {ElapsedMs}ms",
                   context.Request.Method,
                   context.Request.Path,
                   sw.ElapsedMilliseconds);
           }
       }
   }

Afternoon (3 hours):
3. Set up monitoring:
   - Configure Application Insights (if Azure)
   - Set up health checks
   - Create monitoring dashboard
   - Configure alerts

CLAUDE PROMPT FOR DAY 27:
"I need to implement comprehensive logging and monitoring for my production application. Please show me: 1) Enhanced Serilog configuration with file rotation, 2) Performance logging middleware, 3) Health check implementation, 4) Monitoring dashboard setup."

DAY 28: Performance Optimization
---------------------------------
Morning (4 hours):
1. Implement caching:
   public class CachedSchedulerService
   {
       private readonly IMemoryCache _cache;
       private readonly SchedulerService _service;
       
       public async Task<List<Part>> GetActivePartsAsync()
       {
           return await _cache.GetOrCreateAsync("active_parts", 
               async entry =>
               {
                   entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                   return await _service.GetActivePartsAsync();
               });
       }
   }

2. Add response compression:
   builder.Services.AddResponseCompression(options =>
   {
       options.EnableForHttps = true;
       options.Providers.Add<BrotliCompressionProvider>();
       options.Providers.Add<GzipCompressionProvider>();
   });

Afternoon (3 hours):
3. Optimize database queries:
   - Add missing indexes identified in testing
   - Optimize slow queries
   - Implement query result caching

CLAUDE PROMPT FOR DAY 28:
"I need to optimize my application performance for production. Please help me: 1) Implement memory caching for frequently accessed data, 2) Add response compression, 3) Optimize Entity Framework queries, 4) Set up CDN for static assets."

DAY 29: Final Testing & Bug Fixes
----------------------------------
Morning (4 hours):
1. Complete test checklist:
   [ ] All unit tests pass
   [ ] All integration tests pass
   [ ] Performance benchmarks meet targets
   [ ] Security scan completed
   [ ] No critical bugs in issue tracker
   [ ] Documentation complete

2. User acceptance testing:
   - Test with real users
   - Gather feedback
   - Fix critical issues

Afternoon (3 hours):
3. Final deployment preparation:
   - Create release notes
   - Update version numbers
   - Tag release in git
   - Create deployment package

CLAUDE PROMPT FOR DAY 29:
"I'm doing final testing before production deployment. Please provide: 1) Comprehensive testing checklist, 2) Release notes template, 3) Final deployment verification steps, 4) Rollback plan if issues occur in production."

DAY 30: Production Deployment & Monitoring
------------------------------------------
Morning (4 hours):
1. Deploy to production:
   - Backup production database
   - Deploy application files
   - Run database migrations
   - Verify application starts
   - Test critical functionality

2. Monitor initial hours:
   - Watch error logs
   - Monitor performance
   - Check for memory leaks
   - Verify all features work

Afternoon (3 hours):
3. Post-deployment tasks:
   - Document any issues found
   - Create hotfix plan if needed
   - Schedule follow-up review
   - Plan next iteration

CLAUDE PROMPT FOR DAY 30:
"I'm deploying to production today. Please provide: 1) Step-by-step deployment checklist, 2) Monitoring checklist for first 24 hours, 3) Common production issues and solutions, 4) Post-deployment review template."

SUMMARY CLAUDE PROMPTS BY WEEK
==============================

Week 1 - Database Fixes:
"Help me analyze and fix database schema mismatches between my SQLite database and Entity Framework models"

Week 2 - Performance & Logic:
"Help me optimize Entity Framework queries and implement business logic patterns like state machines"

Week 3 - Business Logic & UI:
"Help me implement business rules, improve UI/UX, and fix JavaScript form handling"

Week 4 - UI/UX & Testing:
"Help me implement HTMX properly, improve modal state management, and create comprehensive cost calculations"

Week 5 - Testing:
"Help me create unit tests, integration tests, and performance benchmarks for my application"

Week 6 - Deployment:
"Help me prepare, deploy, and monitor my ASP.NET Core application in production"

This plan provides a structured approach to fixing all identified issues while building a robust, production-ready application.