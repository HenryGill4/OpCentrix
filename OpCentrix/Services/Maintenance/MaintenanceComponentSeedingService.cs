using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Models.Maintenance;

namespace OpCentrix.Services.Maintenance
{
    public class MaintenanceComponentSeedingService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<MaintenanceComponentSeedingService> _logger;
        public MaintenanceComponentSeedingService(SchedulerContext context, ILogger<MaintenanceComponentSeedingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task EnsureSeedAsync()
        {
            // Ensure migration table exists
            if (!await _context.Database.CanConnectAsync()) return;
            try
            {
                var slsMachines = await _context.Machines.Where(m=>m.IsActive && (m.MachineType.Contains("SLS") || m.MachineType.Contains("Print") || m.MachineType==""))
                    .Select(m=>m.MachineId).ToListAsync();
                if (!slsMachines.Any()) return;
                var defaultComponents = new []{"Recoater Arm","Laser Optics","Inert Gas Filter","Build Chamber","Powder Feed System"};
                foreach (var machineId in slsMachines)
                {
                    foreach (var name in defaultComponents)
                    {
                        if (!await _context.MachineComponents.AnyAsync(c=>c.MachineId==machineId && c.Name==name))
                        {
                            _context.MachineComponents.Add(new MachineComponent
                            {
                                MachineId = machineId,
                                Name = name,
                                Category = "Core",
                                DisplayOrder = 10,
                                CreatedBy = "Seeder"
                            });
                        }
                    }
                }
                // Global stage-like component for Sieve Station (shared)
                if (!await _context.MachineComponents.AnyAsync(c=>c.MachineId=="SIEVE-STATION"))
                {
                    _context.MachineComponents.Add(new MachineComponent
                    {
                        MachineId = "SIEVE-STATION",
                        Name = "Sieve Station",
                        Category = "Stage",
                        DisplayOrder = 5,
                        CreatedBy = "Seeder"
                    });
                }
                await _context.SaveChangesAsync();
            }
            catch(System.Exception ex)
            {
                _logger.LogError(ex, "Error seeding maintenance machine components");
            }
        }
    }
}
