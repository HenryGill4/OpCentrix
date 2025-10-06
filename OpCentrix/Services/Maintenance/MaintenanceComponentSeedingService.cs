using Microsoft.EntityFrameworkCore; // minimal refs retained for future re-enable
using Microsoft.Extensions.Logging;
using OpCentrix.Data;

namespace OpCentrix.Services.Maintenance
{
    /// <summary>
    /// LEGACY (disabled) maintenance component seeding.
    /// Table MachineComponents has been retired. This service now no-ops to avoid startup errors.
    /// </summary>
    public class MaintenanceComponentSeedingService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<MaintenanceComponentSeedingService> _logger;
        public MaintenanceComponentSeedingService(SchedulerContext context, ILogger<MaintenanceComponentSeedingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// No-op: MachineComponents seeding removed. Call left in place for backward compatibility.
        /// </summary>
        public Task EnsureSeedAsync()
        {
            _logger.LogInformation("[MAINT-SEED] Skipped legacy MachineComponents seeding (table removed)");
            return Task.CompletedTask;
        }
    }
}
