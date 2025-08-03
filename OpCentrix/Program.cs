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
});

// Register application services
builder.Services.AddScoped<ISchedulerService, SchedulerService>();
builder.Services.AddScoped<ITimeSlotService, TimeSlotService>(); // NEW: Time slot calculation service
builder.Services.AddScoped<IPrintJobLogService, PrintJobLogService>();
builder.Services.AddScoped<IPrintTrackingService, PrintTrackingService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IMasterScheduleService, MasterScheduleService>(); // Task 12: Master Schedule Service
builder.Services.AddScoped<SlsDataSeedingService>(); // SLS Data Seeding Service

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

// Option A: Multi-Stage Workflow Enhancement Service
builder.Services.AddScoped<ICohortManagementService, CohortManagementService>();

// FIXED: Use only the Admin namespace ProductionStageSeederService to resolve ambiguity - COMPLETE FIX
builder.Services.AddScoped<OpCentrix.Services.Admin.IProductionStageSeederService, OpCentrix.Services.Admin.ProductionStageSeederService>();

// Register the new StageTemplateService for custom field templates - PLACEHOLDER
// builder.Services.AddScoped<IStageTemplateService, StageTemplateService>();

var app = builder.Build();

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

app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map Razor Pages
app.MapRazorPages();

// FIXED: Map API controllers for BugReport and other API endpoints
app.MapControllers();

// B&T MES Route Configuration (NEW)
app.MapGet("/BT", context =>
{
    context.Response.Redirect("/BT/Dashboard");
    return Task.CompletedTask;
});

app.MapGet("/BT/Dashboard", () => "B&T Dashboard - Coming Soon")
    .RequireAuthorization("BTAccess");

app.MapGet("/BT/SerialNumbers", () => "B&T Serial Numbers - Coming Soon")
    .RequireAuthorization("BTAccess");

app.MapGet("/BT/Compliance", () => "B&T Compliance - Coming Soon")
    .RequireAuthorization("ComplianceAccess");

app.MapGet("/Workflows", context =>
{
    context.Response.Redirect("/Workflows/MultiStage");
    return Task.CompletedTask;
});

app.MapGet("/Workflows/MultiStage", () => "Multi-Stage Workflows - Coming Soon")
    .RequireAuthorization("WorkflowAccess");

app.MapGet("/Workflows/Resources", () => "Resource Scheduling - Coming Soon")
    .RequireAuthorization("WorkflowAccess");

// Admin B&T Routes
app.MapGet("/Admin/BT", context =>
{
    context.Response.Redirect("/Admin/BT/PartClassifications");
    return Task.CompletedTask;
});

app.MapGet("/Admin/BT/PartClassifications", () => "B&T Part Classifications Admin - Coming Soon")
    .RequireAuthorization("AdminOnly");

app.MapGet("/Admin/BT/SerialNumbers", () => "B&T Serial Numbers Admin - Coming Soon")
    .RequireAuthorization("AdminOnly");

app.MapGet("/Admin/BT/ComplianceDocuments", () => "B&T Compliance Documents Admin - Coming Soon")
    .RequireAuthorization("AdminOnly");

// Add health check endpoint
app.MapGet("/health", () => "Healthy")
    .RequireAuthorization("SchedulerAccess");

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Initialize database with seeded data
        logger.LogInformation("Initializing database...");
        using (var initScope = app.Services.CreateScope())
        {
            var context = initScope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var seedingService = initScope.ServiceProvider.GetRequiredService<SlsDataSeedingService>();
            var adminSeedingService = initScope.ServiceProvider.GetRequiredService<OpCentrix.Services.Admin.IAdminDataSeedingService>();
            var partClassificationService = initScope.ServiceProvider.GetRequiredService<IPartClassificationService>();

            // Ensure database is created and up to date
            await context.Database.EnsureCreatedAsync();

            // Seed initial data
            await seedingService.SeedDataAsync();
            
            // Seed admin control system data (Task 2)
            await adminSeedingService.SeedAllDefaultDataAsync();

            // Seed B&T part classifications (Segment 7)
            await partClassificationService.SeedDefaultClassificationsAsync();
            
            // Initialize production stages for prototype tracking (Phase 0.5)
            var productionStageService = initScope.ServiceProvider.GetRequiredService<ProductionStageService>();
            await productionStageService.CreateDefaultStagesAsync();
            
            // FIXED: Use the Admin namespace service for production stage seeding - COMPLETE FIX
            var productionStageSeeder = initScope.ServiceProvider.GetRequiredService<OpCentrix.Services.Admin.IProductionStageSeederService>();
            var seededStageCount = await productionStageSeeder.SeedDefaultStagesAsync();
            logger.LogInformation("Production stages seeded: {Count} stages available", seededStageCount);
            
            // Load system settings into configuration (Task 3)
            var configurationService = initScope.ServiceProvider.GetRequiredService<OpCentrix.Services.Admin.ISystemConfigurationService>();
            await configurationService.LoadSettingsIntoConfigurationAsync();
            
            // Initialize static configuration helper
            OpCentrix.Services.Admin.SystemConfiguration.Initialize(app.Services);
        }
        logger.LogInformation("Database initialization completed successfully");
        logger.LogInformation("B&T Manufacturing Execution System ready!");
        logger.LogInformation("Test users available:");
        logger.LogInformation("   admin/admin123 (Admin)");
        logger.LogInformation("   manager/manager123 (Manager)");  
        logger.LogInformation("   scheduler/scheduler123 (Scheduler)");
        logger.LogInformation("   operator/operator123 (Operator)");
        logger.LogInformation("   printer/printer123 (PrintingSpecialist)");
        logger.LogInformation("   coating/coating123 (CoatingSpecialist)");
        logger.LogInformation("B&T Specialist Users:");
        logger.LogInformation("   btspecialist/btspecialist123 (BTSpecialist)");
        logger.LogInformation("   workflowspecialist/workflowspecialist123 (WorkflowSpecialist)");
        logger.LogInformation("   compliancespecialist/compliancespecialist123 (ComplianceSpecialist)");
        logger.LogInformation("Segment 7 Features:");
        logger.LogInformation("   - B&T Part Classification System");
        logger.LogInformation("   - ATF/ITAR Compliance Tracking");
        logger.LogInformation("   - Serial Number Management");
        logger.LogInformation("   - Regulatory Documentation");
        logger.LogInformation("B&T MES Navigation Features:");
        logger.LogInformation("   - B&T Manufacturing Section (Amber)");
        logger.LogInformation("   - Advanced Workflows Section (Purple)");
        logger.LogInformation("   - Enhanced Admin B&T Management");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "CRITICAL ERROR during database initialization");
        logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
        
        // In development, continue running even if seeding fails
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

// Log application startup with correct URL
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
var urls = builder.Configuration.GetValue<string>("Urls") ?? "http://localhost:5090";
startupLogger.LogInformation("OpCentrix B&T Manufacturing Execution System startedSuccessfully");
startupLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
startupLogger.LogInformation("URL: {Url}", urls);
startupLogger.LogInformation("Login Page: {LoginUrl}", $"{urls}/Account/Login");

app.Run();

// Make Program accessible for testing
public partial class Program { }
