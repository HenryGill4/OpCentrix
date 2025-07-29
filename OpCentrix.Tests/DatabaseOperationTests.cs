using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OpCentrix.Tests;

/// <summary>
/// Essential database operation tests for Phase 6: Testing & Quality Assurance
/// Tests database connectivity, CRUD operations, data integrity, and performance
/// </summary>
public class DatabaseOperationTests : IClassFixture<OpCentrixWebApplicationFactory>, IDisposable
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly ITestOutputHelper _output;

    public DatabaseOperationTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    #region Database Connectivity Tests

    [Fact]
    public async Task Database_ConnectionIsHealthy()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

        // Act
        var canConnect = await context.Database.CanConnectAsync();

        // Assert
        Assert.True(canConnect);
        _output.WriteLine("? Database connection is healthy");
    }

    [Fact]
    public async Task Database_CoreTablesExist()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

        try
        {
            // Act & Assert - Test core tables
            var userCount = await context.Users.CountAsync();
            var partCount = await context.Parts.CountAsync();
            var machineCount = await context.Machines.CountAsync();
            var jobCount = await context.Jobs.CountAsync();
            var settingsCount = await context.SystemSettings.CountAsync();
            var permissionsCount = await context.RolePermissions.CountAsync();

            // All counts should be >= 0
            Assert.True(userCount >= 0);
            Assert.True(partCount >= 0);
            Assert.True(machineCount >= 0);
            Assert.True(jobCount >= 0);
            Assert.True(settingsCount >= 0);
            Assert.True(permissionsCount >= 0);

            _output.WriteLine($"? Core tables verified:");
            _output.WriteLine($"   Users: {userCount}");
            _output.WriteLine($"   Parts: {partCount}");
            _output.WriteLine($"   Machines: {machineCount}");
            _output.WriteLine($"   Jobs: {jobCount}");
            _output.WriteLine($"   Settings: {settingsCount}");
            _output.WriteLine($"   Permissions: {permissionsCount}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"? Database table access error: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region CRUD Operation Tests

    [Fact]
    public async Task CRUD_UsersOperations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

        var testUser = new User
        {
            Username = $"testuser_{Guid.NewGuid():N}",
            PasswordHash = "testhash",
            Role = "Operator",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        try
        {
            // Act - Create
            context.Users.Add(testUser);
            await context.SaveChangesAsync();
            Assert.True(testUser.Id > 0);
            _output.WriteLine($"? User created with ID: {testUser.Id}");

            // Act - Read
            var readUser = await context.Users.FindAsync(testUser.Id);
            Assert.NotNull(readUser);
            Assert.Equal(testUser.Username, readUser!.Username);
            _output.WriteLine($"? User read successfully: {readUser.Username}");

            // Act - Update
            readUser.Role = "Scheduler";
            readUser.LastModifiedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();

            var updatedUser = await context.Users.FindAsync(testUser.Id);
            Assert.NotNull(updatedUser);
            Assert.Equal("Scheduler", updatedUser!.Role);
            _output.WriteLine($"? User updated: Role = {updatedUser.Role}");

            // Act - Delete
            context.Users.Remove(updatedUser);
            await context.SaveChangesAsync();

            var deletedUser = await context.Users.FindAsync(testUser.Id);
            Assert.Null(deletedUser);
            _output.WriteLine("? User deleted successfully");
        }
        catch (Exception ex)
        {
            // Cleanup in case of failure
            try
            {
                var cleanupUser = await context.Users.FindAsync(testUser.Id);
                if (cleanupUser != null)
                {
                    context.Users.Remove(cleanupUser);
                    await context.SaveChangesAsync();
                }
            }
            catch { }
            
            throw new Exception("User CRUD operations failed", ex);
        }
    }

    [Fact]
    public async Task CRUD_PartsOperations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

        var testPart = new Part
        {
            PartNumber = $"TEST-{Random.Shared.Next(1000, 9999)}",
            Name = "Test Part",
            Description = "Test Description",
            Material = "Ti-6Al-4V Grade 5",
            SlsMaterial = "Ti-6Al-4V Grade 5",
            Industry = "Aerospace",
            Application = "Test Component",
            IsActive = true,
            EstimatedHours = 8.0,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        try
        {
            // Act - Create
            context.Parts.Add(testPart);
            await context.SaveChangesAsync();
            Assert.True(testPart.Id > 0);
            _output.WriteLine($"? Part created: {testPart.PartNumber} (ID: {testPart.Id})");

            // Act - Read with filtering
            var readPart = await context.Parts
                .Where(p => p.PartNumber == testPart.PartNumber)
                .FirstOrDefaultAsync();
            Assert.NotNull(readPart);
            Assert.Equal(testPart.Name, readPart!.Name);
            _output.WriteLine($"? Part read with filter: {readPart.PartNumber}");

            // Act - Update
            readPart.EstimatedHours = 10.0;
            readPart.LastModifiedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();

            var updatedPart = await context.Parts.FindAsync(testPart.Id);
            Assert.NotNull(updatedPart);
            Assert.Equal(10.0, updatedPart!.EstimatedHours);
            _output.WriteLine($"? Part updated: EstimatedHours = {updatedPart.EstimatedHours}");

            // Act - Delete
            context.Parts.Remove(updatedPart);
            await context.SaveChangesAsync();

            var deletedPart = await context.Parts.FindAsync(testPart.Id);
            Assert.Null(deletedPart);
            _output.WriteLine("? Part deleted successfully");
        }
        catch (Exception ex)
        {
            // Cleanup
            try
            {
                var cleanupPart = await context.Parts.FindAsync(testPart.Id);
                if (cleanupPart != null)
                {
                    context.Parts.Remove(cleanupPart);
                    await context.SaveChangesAsync();
                }
            }
            catch { }
            
            throw new Exception("Part CRUD operations failed", ex);
        }
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Performance_LargeDataSetQuery()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var results = await context.Jobs
            .Include(j => j.Part)
            .Where(j => j.Status == "Scheduled" || j.Status == "InProgress")
            .OrderBy(j => j.ScheduledStart)
            .Take(100)
            .ToListAsync();
        
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Query took {stopwatch.ElapsedMilliseconds}ms (over 5s threshold)");
        _output.WriteLine($"? Large dataset query: {results.Count} jobs in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Performance_ConcurrentOperations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

        // Act - Multiple concurrent read operations
        var tasks = Enumerable.Range(0, 10).Select(async i =>
        {
            var parts = await context.Parts.Take(10).ToListAsync();
            return parts.Count;
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, count => Assert.True(count >= 0));
        _output.WriteLine($"? Concurrent operations completed: {results.Length} tasks");
    }

    #endregion

    #region Admin Service Integration Tests

    [Fact]
    public async Task AdminServices_RolePermissionsIntegration()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var roleService = scope.ServiceProvider.GetRequiredService<IRolePermissionService>();

        // Act
        var hasPermission = await roleService.HasPermissionAsync("Admin", "Admin.ManageUsers");
        var permissions = await roleService.GetPermissionsForRoleAsync("Admin");

        // Assert
        Assert.True(hasPermission);
        Assert.NotNull(permissions);
        Assert.NotEmpty(permissions);
        
        _output.WriteLine($"? Role permissions service: Admin has {permissions.Count()} permissions");
    }

    #endregion

    public void Dispose()
    {
        // Cleanup if needed
    }
}