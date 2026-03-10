using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.MachineProviders;

namespace OpCentrix.Services.MachineProviders;

/// <summary>
/// Configuration options for the machine sync service
/// </summary>
public class MachineSyncOptions
{
    public const string SectionName = "MachineSync";

    /// <summary>
    /// Whether machine sync is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Default poll interval in seconds
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum concurrent machine polls
    /// </summary>
    public int MaxConcurrentPolls { get; set; } = 5;

    /// <summary>
    /// Whether to save state records to database
    /// </summary>
    public bool SaveStateRecords { get; set; } = true;

    /// <summary>
    /// How long to retain state records (days, 0 = forever)
    /// </summary>
    public int StateRecordRetentionDays { get; set; } = 30;
}

/// <summary>
/// Background service that periodically polls machines and syncs their state.
/// Updates Machine.Status, creates MachineStateRecords, and detects state changes.
/// </summary>
public class MachineSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MachineSyncService> _logger;
    private readonly MachineSyncOptions _options;

    // Track last known state for change detection
    private readonly Dictionary<int, MachineStateRecord> _lastKnownStates = new();

    public MachineSyncService(
        IServiceProvider serviceProvider,
        ILogger<MachineSyncService> logger,
        IOptions<MachineSyncOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("[SYNC] Machine sync service is disabled");
            return;
        }

        _logger.LogInformation("[SYNC] Machine sync service starting with {Interval}s interval", 
            _options.PollIntervalSeconds);

        // Wait a bit for application startup
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncAllMachinesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Expected on shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SYNC] Error in machine sync cycle");
            }

            // Clean up old state records periodically (every hour)
            if (DateTime.UtcNow.Minute == 0 && _options.StateRecordRetentionDays > 0)
            {
                await CleanupOldStateRecordsAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("[SYNC] Machine sync service stopped");
    }

    private async Task SyncAllMachinesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        var factory = scope.ServiceProvider.GetRequiredService<IMachineProviderFactory>();

        // Get all active machines that should be synced
        var machines = await context.Machines
            .Where(m => m.IsActive)
            .Select(m => new { m.Id, m.MachineId, m.Name })
            .ToListAsync(ct);

        if (!machines.Any())
        {
            _logger.LogDebug("[SYNC] No active machines to sync");
            return;
        }

        _logger.LogDebug("[SYNC] Syncing {Count} machines", machines.Count);

        // Use semaphore for concurrency control
        using var semaphore = new SemaphoreSlim(_options.MaxConcurrentPolls);
        var tasks = machines.Select(async machine =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                await SyncMachineAsync(machine.Id, factory, context, ct);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task SyncMachineAsync(
        int machineId, 
        IMachineProviderFactory factory,
        SchedulerContext context,
        CancellationToken ct)
    {
        try
        {
            var provider = await factory.GetProviderForMachineAsync(machineId, ct);
            var machineData = await provider.GetMachineDataAsync(machineId, ct);

            // Get previous state for change detection
            _lastKnownStates.TryGetValue(machineId, out var previousState);

            // Create state record
            var stateRecord = MachineStateRecord.FromMachineData(machineData, previousState);

            // Update Machine entity
            var machine = await context.Machines.FindAsync(new object[] { machineId }, ct);
            if (machine != null)
            {
                var statusChanged = machine.Status != stateRecord.Status;
                
                machine.Status = stateRecord.Status;
                machine.LastStatusUpdate = DateTime.UtcNow;
                machine.LastModifiedDate = DateTime.UtcNow;
                machine.LastModifiedBy = "MachineSyncService";

                if (statusChanged)
                {
                    _logger.LogInformation("[SYNC] Machine {MachineId} ({Name}) status changed: {OldStatus} ? {NewStatus}",
                        machine.MachineId, machine.Name, previousState?.Status ?? "Unknown", stateRecord.Status);
                }
            }

            // Save state record if enabled
            if (_options.SaveStateRecords)
            {
                // Check if MachineStateRecords table exists (might need migration)
                try
                {
                    context.Set<MachineStateRecord>().Add(stateRecord);
                }
                catch (InvalidOperationException)
                {
                    // Table doesn't exist yet, skip
                    _logger.LogDebug("[SYNC] MachineStateRecords table not available, skipping record");
                }
            }

            await context.SaveChangesAsync(ct);

            // Update last known state
            _lastKnownStates[machineId] = stateRecord;

            // Update connection settings success
            var settings = await context.Set<MachineConnectionSettings>()
                .FirstOrDefaultAsync(s => s.MachineId == machineId, ct);
            if (settings != null)
            {
                settings.RecordSuccess();
                await context.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[SYNC] Failed to sync machine {MachineId}", machineId);

            // Record failure in connection settings
            try
            {
                var settings = await context.Set<MachineConnectionSettings>()
                    .FirstOrDefaultAsync(s => s.MachineId == machineId, ct);
                if (settings != null)
                {
                    settings.RecordFailure(ex.Message);
                    await context.SaveChangesAsync(ct);
                }
            }
            catch
            {
                // Ignore errors updating settings
            }

            // Update machine status to indicate connection issue
            try
            {
                var machine = await context.Machines.FindAsync(new object[] { machineId }, ct);
                if (machine != null)
                {
                    machine.Status = "Offline";
                    machine.LastStatusUpdate = DateTime.UtcNow;
                    await context.SaveChangesAsync(ct);
                }
            }
            catch
            {
                // Ignore errors updating machine
            }
        }
    }

    private async Task CleanupOldStateRecordsAsync(CancellationToken ct)
    {
        if (_options.StateRecordRetentionDays <= 0) return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            var cutoffDate = DateTime.UtcNow.AddDays(-_options.StateRecordRetentionDays);

            // Delete old records in batches
            var deleted = await context.Set<MachineStateRecord>()
                .Where(r => r.Timestamp < cutoffDate)
                .Take(1000)
                .ExecuteDeleteAsync(ct);

            if (deleted > 0)
            {
                _logger.LogInformation("[SYNC] Cleaned up {Count} old state records", deleted);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[SYNC] Error cleaning up old state records");
        }
    }
}
