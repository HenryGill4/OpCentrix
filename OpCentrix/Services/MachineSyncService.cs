using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.MachineProviders;
using OpCentrix.Services.MachineProviders;
using System.Text.Json;

namespace OpCentrix.Services
{
    /// <summary>
    /// Background service that polls every connected machine on its configured interval,
    /// persists the latest state to MachineStateRecords, and (Phase 2) will broadcast
    /// updates to the SignalR hub for real-time UI refresh.
    ///
    /// One provider instance is created per machine and held for the service lifetime.
    /// Providers are reconnected automatically after consecutive failures.
    /// </summary>
    public sealed class MachineSyncService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<MachineSyncService> _logger;

        // machineId → (provider, lastPollTime, consecutiveFailures)
        private readonly Dictionary<string, MachineProviderState> _providers = new();

        private const int MaxConsecutiveFailures = 5;
        private const int ReconnectAfterFailures = 3;

        public MachineSyncService(
            IServiceProvider sp,
            ILogger<MachineSyncService> logger)
        {
            _sp = sp;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MachineSyncService starting");

            // Initial load of all enabled machine connections
            await InitialiseProvidersAsync(stoppingToken);

            // Main polling loop — runs until app shutdown
            while (!stoppingToken.IsCancellationRequested)
            {
                await PollDueMachinesAsync(stoppingToken);

                // Check every 5 seconds; individual machines honour their own intervals
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            _logger.LogInformation("MachineSyncService stopping");
        }

        public override async Task StopAsync(CancellationToken ct)
        {
            foreach (var entry in _providers.Values)
            {
                try { await entry.Provider.DisconnectAsync(ct); }
                catch { /* best-effort disconnect */ }
                await entry.Provider.DisposeAsync();
            }
            _providers.Clear();
            await base.StopAsync(ct);
        }

        // ── Initialisation ────────────────────────────────────────────────────

        private async Task InitialiseProvidersAsync(CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var factory = scope.ServiceProvider.GetRequiredService<MachineProviderFactory>();

            var configs = await db.MachineConnectionSettings
                .Where(c => c.IsEnabled)
                .ToListAsync(ct);

            foreach (var config in configs)
            {
                await RegisterProviderAsync(config, factory, ct);
            }

            _logger.LogInformation(
                "MachineSyncService initialised {Count} machine provider(s)", _providers.Count);
        }

        private async Task RegisterProviderAsync(
            MachineConnectionSettings config,
            MachineProviderFactory factory,
            CancellationToken ct)
        {
            try
            {
                var provider = await factory.CreateAndConnectAsync(config, ct);
                if (provider == null) return;

                _providers[config.MachineId] = new MachineProviderState(
                    provider: provider,
                    config: config,
                    nextPollAt: DateTime.UtcNow);

                _logger.LogInformation(
                    "Registered {ProviderType} provider for machine {MachineId}",
                    config.ProviderType, config.MachineId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to register provider for machine {MachineId}", config.MachineId);
            }
        }

        // ── Polling ───────────────────────────────────────────────────────────

        private async Task PollDueMachinesAsync(CancellationToken ct)
        {
            var due = _providers.Values
                .Where(p => DateTime.UtcNow >= p.NextPollAt)
                .ToList();

            if (due.Count == 0) return;

            // Poll all due machines concurrently
            var tasks = due.Select(p => PollMachineAsync(p, ct));
            await Task.WhenAll(tasks);
        }

        private async Task PollMachineAsync(MachineProviderState state, CancellationToken ct)
        {
            var machineId = state.Config.MachineId;

            try
            {
                var status = await state.Provider.GetStatusAsync(ct);
                var alerts = await state.Provider.GetAlertsAsync(ct);

                await PersistStateAsync(status, alerts, ct);

                state.ConsecutiveFailures = 0;
                state.NextPollAt = DateTime.UtcNow.AddSeconds(state.Config.PollIntervalSeconds);

                await UpdateConnectionHealthAsync(machineId, success: true, error: null, ct);

                _logger.LogDebug(
                    "Polled {MachineId}: {State} {Progress:F1}%",
                    machineId, status.State, status.BuildProgressPercent);

                // TODO (Phase 2): Broadcast to SignalR hub
                // await _hub.Clients.All.SendAsync("MachineStateUpdated", status, ct);
            }
            catch (Exception ex)
            {
                state.ConsecutiveFailures++;
                state.NextPollAt = DateTime.UtcNow.AddSeconds(
                    state.Config.PollIntervalSeconds * state.ConsecutiveFailures);

                _logger.LogWarning(ex,
                    "Poll failed for {MachineId} (failure {Count}/{Max})",
                    machineId, state.ConsecutiveFailures, MaxConsecutiveFailures);

                await UpdateConnectionHealthAsync(machineId, success: false, error: ex.Message, ct);

                if (state.ConsecutiveFailures >= ReconnectAfterFailures)
                    await TryReconnectAsync(state, ct);
            }
        }

        private async Task TryReconnectAsync(MachineProviderState state, CancellationToken ct)
        {
            _logger.LogInformation(
                "Attempting reconnect for {MachineId}", state.Config.MachineId);
            try
            {
                await state.Provider.DisconnectAsync(ct);
                var connected = await state.Provider.ConnectAsync(state.Config, ct);
                if (connected)
                {
                    state.ConsecutiveFailures = 0;
                    _logger.LogInformation("Reconnected {MachineId}", state.Config.MachineId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reconnect failed for {MachineId}", state.Config.MachineId);
            }
        }

        // ── Persistence ───────────────────────────────────────────────────────

        private async Task PersistStateAsync(
            MachineStatus status,
            IReadOnlyList<MachineAlert> alerts,
            CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            var existing = await db.MachineStateRecords
                .FirstOrDefaultAsync(r => r.MachineId == status.MachineId, ct);

            if (existing == null)
            {
                existing = MachineStateRecord.FromStatus(status);
                db.MachineStateRecords.Add(existing);
            }
            else
            {
                existing.State = status.State;
                existing.BuildProgressPercent = status.BuildProgressPercent;
                existing.ActiveJobId = status.ActiveJobId;
                existing.ActiveJobName = status.ActiveJobName;
                existing.EstimatedCompletion = status.EstimatedCompletion;
                existing.TelemetryJson = JsonSerializer.Serialize(status.Telemetry);
                existing.AlertsJson = JsonSerializer.Serialize(alerts);
                existing.ProviderType = status.ProviderType;
                existing.RecordedAt = status.Timestamp;
            }

            await db.SaveChangesAsync(ct);
        }

        private async Task UpdateConnectionHealthAsync(
            string machineId, bool success, string? error, CancellationToken ct)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

                var config = await db.MachineConnectionSettings
                    .FirstOrDefaultAsync(c => c.MachineId == machineId, ct);

                if (config == null) return;

                if (success)
                {
                    config.LastSuccessfulSync = DateTime.UtcNow;
                    config.ConsecutiveFailures = 0;
                    config.LastSyncError = null;
                }
                else
                {
                    config.ConsecutiveFailures++;
                    config.LastSyncError = error;
                }

                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update connection health for {MachineId}", machineId);
            }
        }

        // ── Inner types ───────────────────────────────────────────────────────

        private sealed class MachineProviderState
        {
            public IMachineProvider Provider { get; }
            public MachineConnectionSettings Config { get; }
            public DateTime NextPollAt { get; set; }
            public int ConsecutiveFailures { get; set; } = 0;

            public MachineProviderState(
                IMachineProvider provider,
                MachineConnectionSettings config,
                DateTime nextPollAt)
            {
                Provider = provider;
                Config = config;
                NextPollAt = nextPollAt;
            }
        }
    }
}
