using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;
using OpCentrix.Services.Admin;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Reflection;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.Memory;

// Configure Serilog for global logging (Task 2.5)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "OpCentrix")
    .Enrich.WithProperty("Version", Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: Path.Combine("logs", "opcentrix-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Configure culture for proper decimal parsing
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US");
    options.SupportedCultures = new List<System.Globalization.CultureInfo> { new System.Globalization.CultureInfo("en-US") };
    options.SupportedUICultures = new List<System.Globalization.CultureInfo> { new System.Globalization.CultureInfo("en-US") };
});

// Use Serilog as the logging provider
builder.Host.UseSerilog();

// Configure logging first
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

// FIXED: Add proper antiforgery configuration
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
    options.Cookie.Name = "__RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.SuppressXFrameOptionsHeader = false;
});

// Add services to the container
builder.Services.AddRazorPages(options =>
{
    // FIXED: Ensure proper authorization for Admin folder
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Scheduler", "SchedulerPolicy");

    // Operations Dashboard Authorization
    options.Conventions.AuthorizeFolder("/Operations", "OperatorAccess");

    // B&T MES Route Authorization (NEW)
    options.Conventions.AuthorizeFolder("/BT", "BTAccess");
    options.Conventions.AuthorizeFolder("/Workflows", "WorkflowAccess");
    options.Conventions.AuthorizeFolder("/Compliance", "ComplianceAccess");

    // FIXED: Configure antiforgery for all pages
    options.Conventions.ConfigureFilter(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

// FIXED: Add controllers support for API endpoints (BugReport API)
builder.Services.AddControllers();

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Fallback to default SQLite connection
    connectionString = "Data Source=scheduler.db";
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
}

builder.Services.AddDbContext<SchedulerContext>(options =>
    options.UseSqlite(connectionString, sqliteOptions =>
    {
        sqliteOptions.CommandTimeout(30);
    }));

builder.Services.AddHttpClient(); // Register HttpClient for external API calls

// Authentication configuration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SchedulerPolicy", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("Admin", "Manager"));  // FIXED: Add Manager role

    // FIXED: AdminOnly should also allow Manager role to match test expectations
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin", "Manager"));  // FIXED: Add Manager role

    // Add the missing SchedulerAccess policy that health endpoint uses
    options.AddPolicy("SchedulerAccess", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("SupervisorAccess", policy =>
        policy.RequireRole("Admin", "Supervisor"));

    options.AddPolicy("OperatorAccess", policy =>
        policy.RequireRole("Admin", "Supervisor", "Operator"));

    // B&T Manufacturing Policies (NEW)
    options.AddPolicy("BTAccess", policy =>
        policy.RequireRole("Admin", "Manager", "BTSpecialist"));

    options.AddPolicy("WorkflowAccess", policy =>
        policy.RequireRole("Admin", "Manager", "WorkflowSpecialist"));

    options.AddPolicy("ComplianceAccess", policy =>
        policy.RequireRole("Admin", "Manager", "ComplianceSpecialist", "BTSpecialist"));

    // Individual B&T Feature Policies
    options.AddPolicy("BTSpecialistAccess", policy =>
        policy.RequireRole("Admin", "Manager", "BTSpecialist"));

    options.AddPolicy("WorkflowSpecialistAccess", policy =>
        policy.RequireRole("Admin", "Manager", "WorkflowSpecialist"));

    options.AddPolicy("ComplianceSpecialistAccess", policy =>
        policy.RequireRole("Admin", "Manager", "ComplianceSpecialist"));

    options.AddPolicy("MaintenanceAdmin", policy => policy.RequireRole("Admin","Manager"));
});

// Register application services
builder.Services.AddScoped<ISchedulerService, SchedulerService>();
builder.Services.AddScoped<ITimeSlotService, TimeSlotService>(); // NEW: Time slot calculation service
builder.Services.AddScoped<IPrintJobLogService, PrintJobLogService>();
builder.Services.AddScoped<IPrintTrackingService, PrintTrackingService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IMasterScheduleService, MasterScheduleService>(); // Task 12: Master Schedule Service
builder.Services.AddScoped<SlsDataSeedingService>(); // SLS Data Seeding Service

// NEW: Schedule Compression Service (MVP)
builder.Services.AddScoped<IScheduleCompressionService, ScheduleCompressionService>();

// NEW: Operational Task service registration (Phase 1 Maintenance V2 tasks)
builder.Services.AddScoped<IOperationalTaskService, OperationalTaskService>();
builder.Services.AddScoped<IOperationalTaskEvaluationService, OperationalTaskEvaluationService>();
builder.Services.AddHostedService<OperationalTaskEvaluationHostedService>();

// Register database validation service
builder.Services.AddScoped<DatabaseValidationService>();

// NEW: Admin Control System services (Task 2)
builder.Services.AddScoped<OpCentrix.Services.Admin.IAdminDashboardService, OpCentrix.Services.Admin.AdminDashboardService>();
builder.Services.AddScoped<OpCentrix.Services.Admin.IAdminJobService, OpCentrix.Services.Admin.AdminJobService>();
builder.Services.AddScoped<OpCentrix.Services.Admin.ISystemSettingService, OpCentrix.Services.Admin.SystemSettingService>();
builder.Services.AddScoped<OpCentrix.Services.Admin.IRolePermissionService, OpCentrix.Services.Admin.RolePermissionService>();
builder.Services.AddScoped<OpCentrix.Services.Admin.IOperatingShiftService, OpCentrix.Services.Admin.OperatingShiftService>();
builder.Services.AddScoped<OpCentrix.Services.Admin.ILogViewerService, OpCentrix.Services.Admin.LogViewerService>();
builder.Services.AddScoped<OpCentrix.Services.Admin.IAdminDataSeedingService, OpCentrix.Services.Admin.AdminDataSeedingService>();
builder.Services.AddScoped<OpCentrix.Services.Admin.ISystemConfigurationService, OpCentrix.Services.Admin.SystemConfigurationService>();
builder.Services.AddScoped<OpCentrix.Services.Admin.IMachineManagementService, OpCentrix.Services.Admin.MachineManagementService>();
builder.Services.AddScoped<OpCentrix.Services.Admin.IOperatorAssignmentService, OpCentrix.Services.Admin.OperatorAssignmentService>();

// TASK 16: Database Management Service
builder.Services.AddScoped<OpCentrix.Services.Admin.IDatabaseManagementService, OpCentrix.Services.Admin.DatabaseManagementService>();

// TASK 13: Inspection Checkpoint Service
builder.Services.AddScoped<OpCentrix.Services.Admin.IInspectionCheckpointService, OpCentrix.Services.Admin.InspectionCheckpointService>();

// SEGMENT 6A: Defect Category Service
builder.Services.AddScoped<OpCentrix.Services.Admin.DefectCategoryService>();

// TASK 15: Job Archive Service
builder.Services.AddScoped<OpCentrix.Services.Admin.IJobArchiveService, OpCentrix.Services.Admin.JobArchiveService>();

// FIXED: Add missing OPC UA service
builder.Services.AddScoped<IOpcUaService, OpcUaService>();

// FIXED: Add missing multi-stage job service with correct namespace
builder.Services.AddScoped<IMultiStageJobService, MultiStageJobService>();
builder.Services.AddScoped<IStagePermissionService, StagePermissionService>();

// Task 6: Enhanced machine management services
builder.Services.AddScoped<IMaterialService, MaterialService>();

// Segment 7: B&T Industry Specialization Services
builder.Services.AddScoped<IPartClassificationService, PartClassificationService>();
builder.Services.AddScoped<ISerializationService, SerializationService>();
builder.Services.AddScoped<IComplianceService, ComplianceService>();

// SECTION 7C: B&T Parts Service Layer Enhancement (NEW)
builder.Services.AddScoped<PartClassificationService>();
builder.Services.AddScoped<BTManufacturingWorkflowService>();

// Phase 0.5: Prototype Tracking System Services
builder.Services.AddScoped<PrototypeTrackingService>();
builder.Services.AddScoped<ProductionStageService>();
builder.Services.AddScoped<AssemblyComponentService>();

// Bug Reporting System
builder.Services.AddScoped<IBugReportService, BugReportService>();

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure logging levels
if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

// Register the new PartStageService
builder.Services.AddScoped<IPartStageService, PartStageService>();

// PHASE 3: Part Form Refactor Services - Add missing services
builder.Services.AddScoped<ComponentTypeService>();
builder.Services.AddScoped<ComplianceCategoryService>();

// PHASE 5: Asset Management Service
builder.Services.AddScoped<IPartAssetService, PartAssetService>();

// PHASE 6: Stage Template Service
builder.Services.AddScoped<IStageTemplateService, StageTemplateService>();

// Create seeding service for Part Form Refactor data
builder.Services.AddScoped<PartFormRefactorSeedingService>();

// PHASE 6: Stage Template Seeding Service
builder.Services.AddScoped<StageTemplateSeedingService>();

// Option A: Multi-Stage Workflow Enhancement Service
builder.Services.AddScoped<ICohortManagementService, CohortManagementService>();

// Phase 3: Automated Stage Progression Service - REQUIRED FOR SLS DASHBOARD
builder.Services.AddScoped<IStageProgressionService, StageProgressionService>();

// Phase 5: Build Time Analytics Service (Machine Learning and Performance Analytics)
builder.Services.AddScoped<IBuildTimeAnalyticsService, BuildTimeAnalyticsService>();

// NEW: Stage Dashboard Seeding Service
builder.Services.AddScoped<StageDashboardSeedingService>();

// NEW: Core manufacturing data seeding (materials + machines + demo part)
builder.Services.AddScoped<CoreManufacturingDataSeedingService>();

// NEW: Database Schema Repair Service
builder.Services.AddScoped<DatabaseSchemaRepairService>();

// Update PrintTrackingService registration to include cohort service and stage progression service
builder.Services.AddScoped<IPrintTrackingService>(provider =>
{
    var context = provider.GetRequiredService<SchedulerContext>();
    var logger = provider.GetRequiredService<ILogger<PrintTrackingService>>();
    var cohortService = provider.GetService<ICohortManagementService>(); // Optional
    var stageProgressionService = provider.GetRequiredService<IStageProgressionService>(); // Required for Phase 3
    var maint = provider.GetService<OpCentrix.Services.Maintenance.IMaintenanceService>();
    return new PrintTrackingService(context, logger, cohortService, stageProgressionService, null, null, maint);
});

// NEW: Production Build System Service
builder.Services.AddScoped<IProductionBuildService, ProductionBuildService>();

// FIXED: Use only the Admin namespace ProductionStageSeederService to resolve ambiguity - COMPLETE FIX
builder.Services.AddScoped<OpCentrix.Services.Admin.IProductionStageSeederService, OpCentrix.Services.Admin.ProductionStageSeederService>();

// NEW: Runtime scheduler service (Phase 3)
builder.Services.AddScoped<OpCentrix.Services.Runtime.ISchedulerRuntimeService, OpCentrix.Services.Runtime.SchedulerRuntimeService>();
builder.Services.AddScoped<OpCentrix.Services.Maintenance.MaintenanceComponentSeedingService>();

// Register maintenance services with proper dependencies including memory cache
builder.Services.AddMemoryCache(); // Add memory cache service

builder.Services.AddScoped<OpCentrix.Services.Maintenance.IMaintenanceService>(serviceProvider =>
{
    var context = serviceProvider.GetRequiredService<SchedulerContext>();
    var logger = serviceProvider.GetRequiredService<ILogger<OpCentrix.Services.Maintenance.MaintenanceService>>();
    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
    return new OpCentrix.Services.Maintenance.MaintenanceService(context, logger, loggerFactory, memoryCache);
});

// NEW: Maintenance Background Service for automated processing
if (builder.Configuration.GetValue<bool>("EnableMaintenanceBackgroundService", false))
{
    builder.Services.AddHostedService<OpCentrix.Services.Background.MaintenanceBackgroundService>();
}

var app = builder.Build();

// Ensure core manufacturing data (idempotent lightweight seeding)
using (var scope = app.Services.CreateScope())
{
    try
    {
        // NEW: Run database schema repair first
        var schemaRepair = scope.ServiceProvider.GetRequiredService<DatabaseSchemaRepairService>();
        await schemaRepair.EnsureSchemaIntegrityAsync();
        
        var coreSeeder = scope.ServiceProvider.GetRequiredService<CoreManufacturingDataSeedingService>();
        await coreSeeder.EnsureCoreManufacturingDataAsync();
        // NEW: seed maintenance components
        var maintCompSeeder = scope.ServiceProvider.GetRequiredService<OpCentrix.Services.Maintenance.MaintenanceComponentSeedingService>();
        await maintCompSeeder.EnsureSeedAsync();
    }
    catch (Exception ex)
    {
        var log = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        log.LogError(ex, "? [STARTUP] Failed during startup initialization");
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    app.UseDeveloperExceptionPage();
    // Only use HTTPS redirection in development if explicitly configured
    if (builder.Configuration.GetValue<bool>("UseHttpsRedirection", false))
    {
        app.UseHttpsRedirection();
    }
}

// Add comprehensive error logging middleware for click-through testing
app.UseMiddleware<ErrorLoggingMiddleware>();

// CRITICAL FIX: Use request localization for consistent decimal parsing
app.UseRequestLocalization();

app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map Razor Pages
app.MapRazorPages();

// FIXED: Map API controllers for Production Stages and other API endpoints  
app.MapControllers();

app.Run();

public partial class Program { }