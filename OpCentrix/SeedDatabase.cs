using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;

namespace OpCentrix
{
    /// <summary>
    /// DEPRECATED: Comprehensive database seeding for OpCentrix Parts system
    /// This service has been DISABLED per user request - use Admin pages to add data instead
    /// Keeping file for reference but all methods are now disabled
    /// </summary>
    public static class SeedDatabase
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            
            try
            {
                await context.Database.EnsureCreatedAsync();
                
                logger.LogInformation("?? [SEED] Part and machine seeding is DISABLED");
                logger.LogInformation("?? [SEED] Use /Admin/Parts and /Admin/Machines to add data manually");
                logger.LogInformation("? [SEED] Database seeding skipped successfully!");
                
                // DISABLED: All seeding operations removed per user request
                // Users should use the admin pages to add parts and machines instead
                
                // Still available via admin pages:
                // - Production stages (via ProductionStageSeederService)
                // - Materials (via MaterialService) 
                // - Users (via admin seeding service)
                // - System settings and configurations
                
                return; // Exit early without seeding
                
                // COMMENTED OUT: All seeding logic below
                /*
                // Check if data already exists
                if (await context.Parts.AnyAsync())
                {
                    Console.WriteLine("?? [SEED] Database already contains data, skipping seed");
                    return;
                }
                
                Console.WriteLine("?? [SEED] Starting comprehensive database seeding...");
                
                // Create production stages first
                await SeedProductionStagesAsync(context);
                
                // Create sample parts with all field types
                await SeedPartsAsync(context);
                
                // Create machines and capabilities
                await SeedMachinesAsync(context);
                
                // Create users for testing
                await SeedUsersAsync(context);
                
                // Create part-stage relationships
                if (partStageService != null)
                {
                    await SeedPartStageRequirementsAsync(context, partStageService);
                }
                
                // Create some sample jobs
                await SeedJobsAsync(context);
                
                await context.SaveChangesAsync();
                
                Console.WriteLine("? [SEED] Database seeding completed successfully!");
                */
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "? [SEED] Error in disabled seeding method: {Message}", ex.Message);
                // Don't throw since seeding is disabled anyway
            }
        }
        
        // All seeding methods below are DISABLED and kept for reference only
        
        private static async Task SeedProductionStagesAsync(SchedulerContext context)
        {
            // DISABLED: Use Admin pages instead
            return;
        }
        
        private static async Task SeedPartsAsync(SchedulerContext context)
        {
            // DISABLED: Use /Admin/Parts page instead  
            return;
        }
        
        private static async Task SeedMachinesAsync(SchedulerContext context)
        {
            // DISABLED: Use /Admin/Machines page instead
            return;
        }
        
        private static async Task SeedUsersAsync(SchedulerContext context)
        {
            // DISABLED: Use Admin seeding service instead
            return;
        }
        
        private static async Task SeedPartStageRequirementsAsync(SchedulerContext context, IPartStageService partStageService)
        {
            // DISABLED: Use Admin pages to configure part-stage relationships
            return;
        }
        
        private static async Task SeedJobsAsync(SchedulerContext context)
        {
            // DISABLED: Use Scheduler interface to create jobs
            return;
        }
    }
}