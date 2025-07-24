// Add to Program.cs for performance monitoring
builder.Services.AddApplicationInsightsTelemetry();

// Custom performance tracking in SchedulerService
public class SchedulerService : ISchedulerService
{
    private readonly ILogger<SchedulerService> _logger;
    private readonly SchedulerContext _context;
    
    public async Task<SchedulerPageViewModel> GetSchedulerDataAsync(string? zoom, DateTime? startDate)
    {
        using var activity = Activity.StartActivity("GetSchedulerData");
        activity?.SetTag("zoom", zoom);
        activity?.SetTag("startDate", startDate?.ToString());
        
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await LoadOptimizedSchedulerDataAsync(zoom, startDate);
            
            _logger.LogInformation("?? Scheduler data loaded in {ElapsedMs}ms for {JobCount} jobs", 
                stopwatch.ElapsedMilliseconds, result.Jobs.Count);
            
            return result;
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}