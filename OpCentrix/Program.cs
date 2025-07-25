//Program.cs:
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpCentrix.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();

// Database configuration
builder.Services.AddDbContext<SchedulerContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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
app.MapGet("/health", () => "Healthy");

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Initializing database...");
        
        var context = services.GetRequiredService<SchedulerContext>();
        
        // In development, create database but don't recreate unless explicitly requested
        if (app.Environment.IsDevelopment())
        {
            logger.LogInformation("Development environment - ensuring database exists...");
            
            // Check if user wants to recreate the database
            var recreateDatabase = Environment.GetEnvironmentVariable("RECREATE_DATABASE");
            if (recreateDatabase?.ToLower() == "true")
            {
                logger.LogInformation("RECREATE_DATABASE=true - recreating database...");
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database recreated successfully.");
            }
            else
            {
                // Just ensure it exists, don't delete existing data
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database ensured to exist, preserving existing data.");
            }
        }
        else
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            
            // Apply any pending migrations
            if (context.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Applying database migrations...");
                await context.Database.MigrateAsync();
            }
        }
        
        // Seed the database with SLS data
        var seedingService = services.GetRequiredService<SlsDataSeedingService>();
        await seedingService.SeedDatabaseAsync();
        
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
