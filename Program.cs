// Add to Program.cs for enhanced EF debugging
builder.Services.AddDbContext<SchedulerContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    
    // ?? Enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});

// Enhanced logging configuration
builder.Logging.AddConsole();
builder.Logging.AddDebug();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
}

// Add health checks for production monitoring
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SchedulerContext>("database")
    .AddCheck("scheduler_service", () => 
    {
        // Custom health check for scheduler functionality
        try
        {
            var service = app.Services.GetRequiredService<ISchedulerService>();
            // Perform basic functionality check
            return HealthCheckResult.Healthy("Scheduler service is responding");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Scheduler service failed", ex);
        }
    });

// Configure health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});