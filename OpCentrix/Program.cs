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

// Configure Serilog for global logging (Task 2.5)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/opcentrix-.log", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
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

// Add services to the container
builder.Services.AddRazorPages();

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
    options.AddPolicy("SchedulerAccess", policy =>
        policy.RequireAuthenticatedUser());
    
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    
    options.AddPolicy("SupervisorAccess", policy =>
        policy.RequireRole("Admin", "Supervisor"));
    
    options.AddPolicy("OperatorAccess", policy =>
        policy.RequireRole("Admin", "Supervisor", "Operator"));
});

// Register application services
builder.Services.AddScoped<ISchedulerService, SchedulerService>();
builder.Services.AddScoped<SlsDataSeedingService>();
builder.Services.AddScoped<PrintTrackingService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

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

// FIXED: Add missing OPC UA service
builder.Services.AddScoped<IOpcUaService, OpcUaService>();

// FIXED: Add missing multi-stage job service with correct namespace
builder.Services.AddScoped<IMultiStageJobService, MultiStageJobService>();

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

app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map Razor Pages
app.MapRazorPages();

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

            // Ensure database is created and up to date
            await context.Database.EnsureCreatedAsync();

            // Seed initial data
            await seedingService.SeedDataAsync();
            
            // Seed admin control system data (Task 2)
            await adminSeedingService.SeedAllDefaultDataAsync();
            
            // Load system settings into configuration (Task 3)
            var configurationService = initScope.ServiceProvider.GetRequiredService<OpCentrix.Services.Admin.ISystemConfigurationService>();
            await configurationService.LoadSettingsIntoConfigurationAsync();
            
            // Initialize static configuration helper
            OpCentrix.Services.Admin.SystemConfiguration.Initialize(app.Services);
        }
        logger.LogInformation("Database initialization completed successfully");
        logger.LogInformation("?? TIP: Set environment variable RECREATE_DATABASE=true to recreate database on next startup");
        logger.LogInformation("?? Test users available:");
        logger.LogInformation("   admin/admin123 (Admin)");
        logger.LogInformation("   manager/manager123 (Manager)");  
        logger.LogInformation("   scheduler/scheduler123 (Scheduler)");
        logger.LogInformation("   operator/operator123 (Operator)");
        logger.LogInformation("   printer/printer123 (PrintingSpecialist)");
        logger.LogInformation("   coating/coating123 (CoatingSpecialist)");
        logger.LogInformation("   And more... see TEST_USERS.txt for complete list");
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
startupLogger.LogInformation("?? OpCentrix SLS Scheduler started successfully");
startupLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
startupLogger.LogInformation("URL: {Url}", urls);
startupLogger.LogInformation("Login Page: {LoginUrl}", $"{urls}/Account/Login");

app.Run();

// Make Program accessible for testing
public partial class Program { }
