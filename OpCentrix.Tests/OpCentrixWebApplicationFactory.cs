using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Tests;

/// <summary>
/// Custom WebApplicationFactory for OpCentrix testing
/// Configures in-memory database and test services
/// Following OpCentrix Stage Dashboard Master Plan testing protocols
/// </summary>
public class OpCentrixWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real database registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<SchedulerContext>));
            
            if (descriptor != null)
                services.Remove(descriptor);

            // Add in-memory database for testing
            services.AddDbContext<SchedulerContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
                options.EnableSensitiveDataLogging();
            });

            // Ensure the database is created and seeded
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<OpCentrixWebApplicationFactory>>();

            try
            {
                context.Database.EnsureCreated();
                SeedTestDatabase(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the test database");
            }
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Seed the test database with required data for Stage Dashboard testing
    /// </summary>
    private static void SeedTestDatabase(SchedulerContext context)
    {
        // Only seed if database is empty
        if (context.Users.Any())
            return;

        // Seed test users
        var users = new[]
        {
            new User 
            { 
                Username = "admin", 
                Email = "admin@opcentrix.com", 
                PasswordHash = "AQAAAAEAACcQAAAAEJ7u3m6mJ9Fj+dCFf3Qg4KH4Gv6tQgKp5Xs8Zc2Vh7Ql6Qw8Er9Ty1Ui0Op3As4Qg==", // admin123
                Role = "Admin", 
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new User 
            { 
                Username = "operator", 
                Email = "operator@opcentrix.com", 
                PasswordHash = "AQAAAAEAACcQAAAAEJ7u3m6mJ9Fj+dCFf3Qg4KH4Gv6tQgKp5Xs8Zc2Vh7Ql6Qw8Er9Ty1Ui0Op3As4Qg==", // admin123
                Role = "Operator", 
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new User 
            { 
                Username = "manager", 
                Email = "manager@opcentrix.com", 
                PasswordHash = "AQAAAAEAACcQAAAAEJ7u3m6mJ9Fj+dCFf3Qg4KH4Gv6tQgKp5Xs8Zc2Vh7Ql6Qw8Er9Ty1Ui0Op3As4Qg==", // admin123
                Role = "Manager", 
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new User 
            { 
                Username = "readonly", 
                Email = "readonly@opcentrix.com", 
                PasswordHash = "AQAAAAEAACcQAAAAEJ7u3m6mJ9Fj+dCFf3Qg4KH4Gv6tQgKp5Xs8Zc2Vh7Ql6Qw8Er9Ty1Ui0Op3As4Qg==", // admin123
                Role = "ReadOnly", 
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        context.Users.AddRange(users);

        // Seed production stages for Stage Dashboard testing
        var productionStages = new[]
        {
            new ProductionStage 
            { 
                Name = "3D Printing (SLS)", 
                StageColor = "#007bff", 
                Department = "3D Printing", 
                DisplayOrder = 1, 
                IsActive = true,
                DefaultSetupMinutes = 240,
                RequiresApproval = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new ProductionStage 
            { 
                Name = "CNC Machining", 
                StageColor = "#28a745", 
                Department = "CNC Machining", 
                DisplayOrder = 2, 
                IsActive = true,
                DefaultSetupMinutes = 180,
                RequiresApproval = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new ProductionStage 
            { 
                Name = "EDM", 
                StageColor = "#ffc107", 
                Department = "EDM", 
                DisplayOrder = 3, 
                IsActive = true,
                DefaultSetupMinutes = 120,
                RequiresApproval = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new ProductionStage 
            { 
                Name = "Laser Engraving", 
                StageColor = "#fd7e14", 
                Department = "Laser Operations", 
                DisplayOrder = 4, 
                IsActive = true,
                DefaultSetupMinutes = 60,
                RequiresApproval = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new ProductionStage 
            { 
                Name = "Sandblasting", 
                StageColor = "#6c757d", 
                Department = "Finishing", 
                DisplayOrder = 5, 
                IsActive = true,
                DefaultSetupMinutes = 90,
                RequiresApproval = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new ProductionStage 
            { 
                Name = "Coating/Cerakote", 
                StageColor = "#17a2b8", 
                Department = "Finishing", 
                DisplayOrder = 6, 
                IsActive = true,
                DefaultSetupMinutes = 120,
                RequiresApproval = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new ProductionStage 
            { 
                Name = "Assembly", 
                StageColor = "#dc3545", 
                Department = "Assembly", 
                DisplayOrder = 7, 
                IsActive = true,
                DefaultSetupMinutes = 150,
                RequiresApproval = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        context.ProductionStages.AddRange(productionStages);

        // Seed test machines for stage testing
        var machines = new[]
        {
            new Machine 
            { 
                MachineId = "SLS-001", 
                MachineName = "EOS P396", 
                MachineType = "SLS",
                Status = "Available",
                CurrentMaterial = "PA12",
                Location = "Print Floor A",
                BuildLengthMm = 340,
                BuildWidthMm = 340,
                BuildHeightMm = 600,
                IsActive = true,
                Priority = 1,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Machine 
            { 
                MachineId = "CNC-001", 
                MachineName = "Haas VF-2", 
                MachineType = "CNC",
                Status = "Available",
                Location = "CNC Floor",
                IsActive = true,
                Priority = 1,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Machine 
            { 
                MachineId = "EDM-001", 
                MachineName = "Sodick AQ327L", 
                MachineType = "EDM",
                Status = "Available",
                Location = "EDM Cell",
                IsActive = true,
                Priority = 1,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Machine 
            { 
                MachineId = "LASER-001", 
                MachineName = "Trumpf TruLaser", 
                MachineType = "Laser",
                Status = "Available",
                Location = "Laser Bay",
                IsActive = true,
                Priority = 1,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        context.Machines.AddRange(machines);

        // Seed test parts for Stage Dashboard testing
        var parts = new[]
        {
            new Part 
            { 
                PartNumber = "TEST-PART-001", 
                Name = "Test Part for Stage Dashboard",
                Description = "Test Part for Stage Dashboard",
                Material = "PA12",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Part 
            { 
                PartNumber = "TEST-PART-002", 
                Name = "Multi-Stage Test Part",
                Description = "Multi-Stage Test Part", 
                Material = "Aluminum",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        context.Parts.AddRange(parts);

        // Save changes to get IDs for relationships
        context.SaveChanges();

        // Seed stage requirements for testing stage progression
        var stageRequirements = new List<PartStageRequirement>();
        
        // Part 1 requires SLS and Assembly
        var part1 = context.Parts.First(p => p.PartNumber == "TEST-PART-001");
        var slsStage = context.ProductionStages.First(ps => ps.Name.Contains("SLS"));
        var assemblyStage = context.ProductionStages.First(ps => ps.Name.Contains("Assembly"));
        
        stageRequirements.Add(new PartStageRequirement
        {
            PartId = part1.Id,
            ProductionStageId = slsStage.Id,
            IsRequired = true,
            ExecutionOrder = 1,
            EstimatedHours = 4.0,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        });

        stageRequirements.Add(new PartStageRequirement
        {
            PartId = part1.Id,
            ProductionStageId = assemblyStage.Id,
            IsRequired = true,
            ExecutionOrder = 2,
            EstimatedHours = 2.0,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        });

        // Part 2 requires multiple stages
        var part2 = context.Parts.First(p => p.PartNumber == "TEST-PART-002");
        var cncStage = context.ProductionStages.First(ps => ps.Name.Contains("CNC"));
        var edmStage = context.ProductionStages.First(ps => ps.Name.Contains("EDM"));
        var laserStage = context.ProductionStages.First(ps => ps.Name.Contains("Laser"));

        stageRequirements.Add(new PartStageRequirement
        {
            PartId = part2.Id,
            ProductionStageId = cncStage.Id,
            IsRequired = true,
            ExecutionOrder = 1,
            EstimatedHours = 3.0,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        });

        stageRequirements.Add(new PartStageRequirement
        {
            PartId = part2.Id,
            ProductionStageId = edmStage.Id,
            IsRequired = true,
            ExecutionOrder = 2,
            EstimatedHours = 2.0,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        });

        stageRequirements.Add(new PartStageRequirement
        {
            PartId = part2.Id,
            ProductionStageId = laserStage.Id,
            IsRequired = true,
            ExecutionOrder = 3,
            EstimatedHours = 1.0,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        });

        context.PartStageRequirements.AddRange(stageRequirements);

        // Seed comprehensive test jobs for validation
        var testJobs = new[]
        {
            new Job
            {
                PartId = part1.Id,
                PartNumber = part1.PartNumber,
                Priority = 1,
                Status = "Scheduled",
                ScheduledStart = DateTime.Today.AddHours(8),
                ScheduledEnd = DateTime.Today.AddHours(16),
                EstimatedHours = 6.0,
                MachineId = "SLS-001",
                Quantity = 10,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System",
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = "System"
            },
            new Job
            {
                PartId = part2.Id,
                PartNumber = part2.PartNumber,
                Priority = 2,
                Status = "InProgress",
                ScheduledStart = DateTime.Today.AddHours(10),
                ScheduledEnd = DateTime.Today.AddHours(18),
                EstimatedHours = 8.0,
                MachineId = "CNC-001",
                Quantity = 5,
                ActualStart = DateTime.Today.AddHours(10),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System",
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = "System"
            }
        };

        context.Jobs.AddRange(testJobs);
        context.SaveChanges();
    }
}