using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpCentrix.Authorization;
using Microsoft.AspNetCore.Authorization;
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

// Add services to the container
builder.Services.AddRazorPages();

// Database configuration
builder.Services.AddDbContext<SchedulerContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient(); // Register HttpClient for external API calls

// Authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2); // Default 2 hours
        options.SlidingExpiration = true; // Extend session on activity
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        
        // Handle AJAX/HTMX authentication challenges
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                if (context.Request.Headers["HX-Request"] == "true")
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

// Authorization services with custom requirements
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
    
    // Department-specific policies
    options.AddPolicy("SchedulerAccess", policy => 
        policy.RequireRole("Admin", "Manager", "Scheduler", "Operator"));
    options.AddPolicy("PrintingAccess", policy => 
        policy.RequireRole("Admin", "Manager", "PrintingSpecialist"));
    options.AddPolicy("CoatingAccess", policy => 
        policy.RequireRole("Admin", "Manager", "CoatingSpecialist"));
    options.AddPolicy("ShippingAccess", policy => 
        policy.RequireRole("Admin", "Manager", "ShippingSpecialist"));
    options.AddPolicy("EDMAccess", policy => 
        policy.RequireRole("Admin", "Manager", "EDMSpecialist"));
    options.AddPolicy("MachiningAccess", policy => 
        policy.RequireRole("Admin", "Manager", "MachiningSpecialist"));
    options.AddPolicy("QCAccess", policy => 
        policy.RequireRole("Admin", "Manager", "QCSpecialist"));
    options.AddPolicy("MediaAccess", policy => 
        policy.RequireRole("Admin", "Manager", "MediaSpecialist"));
    options.AddPolicy("AnalyticsAccess", policy => 
        policy.RequireRole("Admin", "Manager", "Analyst"));
});

// Core business services
builder.Services.AddScoped<ISchedulerService, SchedulerService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// SLS-specific services
builder.Services.AddScoped<IOpcUaService, OpcUaService>();
builder.Services.AddScoped<SlsDataSeedingService>();

// Register the print tracking service
builder.Services.AddScoped<IPrintTrackingService, PrintTrackingService>();

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

// Background services for OPC UA monitoring (if needed in future)
// builder.Services.AddHostedService<OpcUaMonitoringService>();

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
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var seedingService = scope.ServiceProvider.GetRequiredService<SlsDataSeedingService>();
            var adminSeedingService = scope.ServiceProvider.GetRequiredService<OpCentrix.Services.Admin.IAdminDataSeedingService>();

            // Ensure database is created and up to date
            await context.Database.EnsureCreatedAsync();

            // Seed initial data
            await seedingService.SeedDataAsync();
            
            // Seed admin control system data (Task 2)
            await adminSeedingService.SeedAllDefaultDataAsync();
            
            // Load system settings into configuration (Task 3)
            var configurationService = scope.ServiceProvider.GetRequiredService<OpCentrix.Services.Admin.ISystemConfigurationService>();
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
