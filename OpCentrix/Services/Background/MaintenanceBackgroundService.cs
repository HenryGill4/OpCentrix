using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpCentrix.Services.Maintenance;

namespace OpCentrix.Services.Background
{
    public class MaintenanceBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MaintenanceBackgroundService> _logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(5); // Run every 5 minutes

        public MaintenanceBackgroundService(IServiceProvider serviceProvider, ILogger<MaintenanceBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Maintenance Background Service started");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessMaintenanceServicesAsync();
                    await Task.Delay(_updateInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Service is stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in maintenance background service");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait 1 minute before retrying
                }
            }
            
            _logger.LogInformation("Maintenance Background Service stopped");
        }

        private async Task ProcessMaintenanceServicesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var maintenanceService = scope.ServiceProvider.GetService<IMaintenanceService>();
            
            if (maintenanceService == null)
            {
                _logger.LogWarning("MaintenanceService not available for background processing");
                return;
            }

            try
            {
                _logger.LogDebug("Processing maintenance services...");
                
                // Update all maintenance service values
                await maintenanceService.ProcessMaintenanceServicesAsync();
                
                // Check thresholds and create alerts
                await maintenanceService.ProcessMaintenanceAlertsAsync();
                
                // Auto-create work orders from schedules
                await maintenanceService.AutoCreateWorkOrdersAsync();
                
                // Recalculate maintenance states
                await maintenanceService.RecalculateAsync();
                
                _logger.LogDebug("Maintenance services processing completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing maintenance services in background task");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Maintenance Background Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}