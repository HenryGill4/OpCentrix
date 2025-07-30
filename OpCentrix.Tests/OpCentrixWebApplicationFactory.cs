using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Services;
using OpCentrix.Models;

namespace OpCentrix.Tests;

/// <summary>
/// Test application factory for OpCentrix integration tests
/// Provides a test environment with in-memory database and proper service configuration
/// </summary>
public class OpCentrixWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SchedulerContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a test database context with in-memory database
            services.AddDbContext<SchedulerContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
                options.EnableSensitiveDataLogging();
            });

            // Build the service provider to initialize test data
            var serviceProvider = services.BuildServiceProvider();

            // Create the database and seed test data
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<OpCentrixWebApplicationFactory>>();

                try
                {
                    // Ensure the database is created
                    context.Database.EnsureCreated();

                    // Seed test users if they don't exist
                    SeedTestUsers(context, authService, logger);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the test database");
                    throw;
                }
            }
        });

        // FIXED: Use Testing environment to ensure proper test behavior
        builder.UseEnvironment("Testing");
    }

    private void SeedTestUsers(SchedulerContext context, IAuthenticationService authService, ILogger logger)
    {
        try
        {
            // Check if users already exist
            if (context.Users.Any())
            {
                logger.LogInformation("Test users already exist, skipping seeding");
                return;
            }

            // Create test users
            var testUsers = new[]
            {
                new { Username = "admin", Password = "admin123", Role = "Admin", FullName = "Admin User" },
                new { Username = "manager", Password = "manager123", Role = "Manager", FullName = "Manager User" },
                new { Username = "scheduler", Password = "scheduler123", Role = "Scheduler", FullName = "Scheduler User" },
                new { Username = "operator", Password = "operator123", Role = "Operator", FullName = "Operator User" },
                new { Username = "printer", Password = "printer123", Role = "PrintingSpecialist", FullName = "Printing Specialist" },
                new { Username = "coating", Password = "coating123", Role = "CoatingSpecialist", FullName = "Coating Specialist" }
            };

            foreach (var testUser in testUsers)
            {
                var user = new User
                {
                    Username = testUser.Username,
                    PasswordHash = authService.HashPassword(testUser.Password),
                    Role = testUser.Role,
                    FullName = testUser.FullName,
                    Email = $"{testUser.Username}@opcentrix.com",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    LastLoginDate = DateTime.UtcNow
                };

                context.Users.Add(user);
                logger.LogInformation("Created test user: {Username} with role {Role}", testUser.Username, testUser.Role);
            }

            // Create some test parts and machines for integration tests
            SeedTestData(context, logger);

            context.SaveChanges();
            logger.LogInformation("Test database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while seeding test users");
            throw;
        }
    }

    private void SeedTestData(SchedulerContext context, ILogger logger)
    {
        try
        {
            // Add test machines - using correct property names
            if (!context.Machines.Any())
            {
                var testMachines = new[]
                {
                    new Machine 
                    { 
                        Name = "TI1", 
                        MachineName = "TI1", 
                        MachineId = "TI1",
                        MachineType = "SLS", 
                        MachineModel = "TruPrint 3000",
                        IsActive = true, 
                        Department = "Printing",
                        Status = "Idle",
                        SerialNumber = "TP3000-001"
                    },
                    new Machine 
                    { 
                        Name = "TI2", 
                        MachineName = "TI2", 
                        MachineId = "TI2",
                        MachineType = "SLS", 
                        MachineModel = "TruPrint 3000",
                        IsActive = true, 
                        Department = "Printing",
                        Status = "Idle",
                        SerialNumber = "TP3000-002"
                    },
                    new Machine 
                    { 
                        Name = "INC", 
                        MachineName = "INC", 
                        MachineId = "INC",
                        MachineType = "PostProcessing", 
                        MachineModel = "Incubator",
                        IsActive = true, 
                        Department = "PostProcessing",
                        Status = "Idle",
                        SerialNumber = "INC-001"
                    }
                };

                context.Machines.AddRange(testMachines);
                logger.LogInformation("Added {Count} test machines", testMachines.Length);
            }

            // Add test parts - using correct property names  
            if (!context.Parts.Any())
            {
                var testParts = new[]
                {
                    new Part
                    {
                        PartNumber = "TEST001",
                        Name = "Test Part 1",
                        Description = "Test Part 1",
                        Material = "Ti-6Al-4V Grade 5",
                        SlsMaterial = "Ti-6Al-4V Grade 5",
                        EstimatedHours = 4.5,
                        PostProcessingTimeMinutes = 60,
                        IsActive = true,
                        PartCategory = "Prototype",
                        ProcessType = "SLS Metal"
                    },
                    new Part
                    {
                        PartNumber = "TEST002", 
                        Name = "Test Part 2",
                        Description = "Test Part 2",
                        Material = "Ti-6Al-4V Grade 5",
                        SlsMaterial = "Ti-6Al-4V Grade 5",
                        EstimatedHours = 6.0,
                        PostProcessingTimeMinutes = 90,
                        IsActive = true,
                        PartCategory = "Prototype", 
                        ProcessType = "SLS Metal"
                    }
                };

                context.Parts.AddRange(testParts);
                logger.LogInformation("Added {Count} test parts", testParts.Length);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while seeding test data");
            throw;
        }
    }
}

/// <summary>
/// Extension methods for HTTP response testing - shared across all test classes
/// </summary>
public static class HttpResponseExtensions
{
    public static bool IsRedirectionResult(this HttpResponseMessage response)
    {
        return (int)response.StatusCode >= 300 && (int)response.StatusCode < 400;
    }
}