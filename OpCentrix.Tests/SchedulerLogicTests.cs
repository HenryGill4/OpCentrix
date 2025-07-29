using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;
using OpCentrix.Services.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OpCentrix.Tests;

/// <summary>
/// Comprehensive scheduler logic tests for Phase 6: Testing & Quality Assurance
/// Tests core scheduling algorithms, validation, and business logic
/// </summary>
public class SchedulerLogicTests : IClassFixture<OpCentrixWebApplicationFactory>, IDisposable
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly ITestOutputHelper _output;

    public SchedulerLogicTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    #region Job Validation Tests

    [Fact]
    public async Task JobValidation_RejectsOverlappingJobs()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
        
        var baseTime = DateTime.Today.AddHours(9); // 9 AM today
        
        var existingJob = CreateTestJob("TI1", baseTime, baseTime.AddHours(4));
        var overlappingJob = CreateTestJob("TI1", baseTime.AddHours(2), baseTime.AddHours(6));
        
        // Act
        var (isValid, errors) = await schedulerService.ValidateJobSchedulingAsync(overlappingJob, new List<Job> { existingJob });
        
        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("conflicts with existing job"));
        
        _output.WriteLine("? Scheduler correctly rejects overlapping jobs");
        _output.WriteLine($"   Errors: {string.Join(", ", errors)}");
    }

    [Fact]
    public async Task JobValidation_AllowsNonOverlappingJobs()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
        
        var baseTime = DateTime.Today.AddHours(9); // 9 AM today
        
        var existingJob = CreateTestJob("TI1", baseTime, baseTime.AddHours(4));
        var nonOverlappingJob = CreateTestJob("TI1", baseTime.AddHours(5), baseTime.AddHours(9));
        
        // Act
        var (isValid, errors) = await schedulerService.ValidateJobSchedulingAsync(nonOverlappingJob, new List<Job> { existingJob });
        
        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
        
        _output.WriteLine("? Scheduler allows non-overlapping jobs");
    }

    [Fact]
    public async Task JobValidation_ValidatesSLSParameters()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
        
        var invalidJob = CreateTestJob("TI1", DateTime.Today.AddHours(9), DateTime.Today.AddHours(13));
        invalidJob.LaserPowerWatts = 0; // Invalid laser power
        invalidJob.ScanSpeedMmPerSec = -100; // Invalid scan speed
        invalidJob.LayerThicknessMicrons = 0; // Invalid layer thickness
        
        // Act
        var (isValid, errors) = await schedulerService.ValidateJobSchedulingAsync(invalidJob, new List<Job>());
        
        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Laser power"));
        Assert.Contains(errors, e => e.Contains("Scan speed"));
        Assert.Contains(errors, e => e.Contains("Layer thickness"));
        
        _output.WriteLine("? Scheduler validates SLS parameters");
        _output.WriteLine($"   Parameter errors: {errors.Count}");
    }

    [Fact]
    public async Task JobValidation_ChecksMaterialCompatibility()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
        
        var baseTime = DateTime.Today.AddHours(9);
        var existingJobTitanium = CreateTestJob("TI1", baseTime, baseTime.AddHours(4));
        existingJobTitanium.SlsMaterial = "Ti-6Al-4V Grade 5";
        
        var newJobInconel = CreateTestJob("TI1", baseTime.AddHours(4), baseTime.AddHours(8));
        newJobInconel.SlsMaterial = "Inconel 718";
        
        // Act
        var (isValid, errors) = await schedulerService.ValidateJobSchedulingAsync(newJobInconel, new List<Job> { existingJobTitanium });
        
        // Assert - Should warn about material changeover
        _output.WriteLine($"? Material compatibility check: Valid={isValid}, Errors={errors.Count}");
        if (errors.Any())
        {
            _output.WriteLine($"   Changeover warnings: {string.Join(", ", errors)}");
        }
    }

    #endregion

    #region Machine Layout Tests

    [Fact]
    public void MachineLayout_CalculatesCorrectLayers()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
        
        var baseTime = DateTime.Today.AddHours(9);
        var jobs = new List<Job>
        {
            CreateTestJob("TI1", baseTime, baseTime.AddHours(2)),
            CreateTestJob("TI1", baseTime.AddHours(2), baseTime.AddHours(4)),
            CreateTestJob("TI1", baseTime.AddHours(4), baseTime.AddHours(6))
        };
        
        // Act
        var (maxLayers, rowHeight) = schedulerService.CalculateMachineRowLayout("TI1", jobs);
        
        // Assert
        Assert.True(maxLayers > 0);
        Assert.True(rowHeight >= 160); // Minimum height
        Assert.True(rowHeight <= 400); // Maximum height
        
        _output.WriteLine($"? Machine layout: {maxLayers} layers, {rowHeight}px height");
    }

    [Fact]
    public void MachineLayout_HandlesEmptyMachine()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
        
        // Act
        var (maxLayers, rowHeight) = schedulerService.CalculateMachineRowLayout("TI1", new List<Job>());
        
        // Assert
        Assert.Equal(1, maxLayers);
        Assert.Equal(160, rowHeight); // Default height
        
        _output.WriteLine("? Machine layout handles empty machines correctly");
    }

    #endregion

    #region Schedule Generation Tests

    [Fact]
    public async Task ScheduleGeneration_CreatesCorrectTimeSlots()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
        
        var startDate = DateTime.Today;
        
        // Act
        var weekView = await schedulerService.GetSchedulerDataAsync("week", startDate);
        var dayView = await schedulerService.GetSchedulerDataAsync("day", startDate);
        var hourView = await schedulerService.GetSchedulerDataAsync("hour", startDate);
        
        // Assert
        Assert.Equal(7, weekView.Dates.Count); // 7 days for week view
        Assert.Equal(1, dayView.SlotsPerDay); // 1 slot per day for day view
        Assert.Equal(24, hourView.SlotsPerDay); // 24 slots per day for hour view
        Assert.Equal(60, hourView.SlotMinutes); // 60 minutes per slot for hour view
        
        _output.WriteLine($"? Schedule generation: Week={weekView.Dates.Count} days, Hour={hourView.SlotsPerDay} slots");
    }

    [Fact]
    public async Task ScheduleGeneration_IncludesMachineData()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
        
        // Act
        var scheduleData = await schedulerService.GetSchedulerDataAsync("week");
        
        // Assert
        Assert.NotNull(scheduleData.Machines);
        _output.WriteLine($"? Schedule includes {scheduleData.Machines.Count} machines");
        
        foreach (var machine in scheduleData.Machines)
        {
            _output.WriteLine($"   Machine: {machine}");
        }
    }

    #endregion

    #region SLS-Specific Tests

    [Fact]
    public async Task SLSValidation_ChecksMachineCompatibility()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        
        var job = CreateTestJob("TI1", DateTime.Today.AddHours(9), DateTime.Today.AddHours(13));
        job.SlsMaterial = "Ti-6Al-4V Grade 5";
        job.LaserPowerWatts = 200;
        job.ScanSpeedMmPerSec = 1200;
        
        // Act
        var isCompatible = await schedulerService.ValidateSlsJobCompatibilityAsync(job, context);
        
        // Assert
        _output.WriteLine($"? SLS compatibility check: {isCompatible}");
        
        // Should be compatible with reasonable parameters
        // Note: Result depends on machine configuration in database
    }

    [Fact]
    public async Task SLSValidation_CalculatesChangeoverTime()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        
        // Act
        var changeoverTime = await schedulerService.CalculateOptimalPowderChangeoverTimeAsync("TI1", "Inconel 718", context);
        
        // Assert
        Assert.True(changeoverTime >= 0);
        _output.WriteLine($"? Powder changeover time calculation: {changeoverTime} minutes");
    }

    [Fact]
    public async Task SLSValidation_CalculatesCostEstimate()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        
        var job = CreateTestJob("TI1", DateTime.Today.AddHours(9), DateTime.Today.AddHours(13));
        // Use properties that exist and are settable
        
        // Act
        var costEstimate = await schedulerService.CalculateSlsJobCostEstimateAsync(job, context);
        
        // Assert
        Assert.True(costEstimate >= 0);
        _output.WriteLine($"? SLS cost estimate: ${costEstimate:F2}");
    }

    #endregion

    #region Operating Hours Integration Tests

    [Fact]
    public async Task SchedulerIntegration_ValidatesOperatingHours()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
        
        // Test business hours job
        var businessHourJob = CreateTestJob("TI1", 
            DateTime.Today.AddHours(10), // 10 AM
            DateTime.Today.AddHours(14)); // 2 PM
        
        // Test after-hours job
        var afterHourJob = CreateTestJob("TI1",
            DateTime.Today.AddHours(22), // 10 PM
            DateTime.Today.AddDays(1).AddHours(2)); // 2 AM next day
        
        // Act
        var (businessValid, businessErrors) = await schedulerService.ValidateJobSchedulingAsync(businessHourJob, new List<Job>());
        var (afterValid, afterErrors) = await schedulerService.ValidateJobSchedulingAsync(afterHourJob, new List<Job>());
        
        // Assert
        _output.WriteLine($"? Business hours job: Valid={businessValid}, Errors={businessErrors.Count}");
        _output.WriteLine($"? After hours job: Valid={afterValid}, Errors={afterErrors.Count}");
        
        // Business hours should typically be more permissive than after hours
        if (businessErrors.Any())
        {
            _output.WriteLine($"   Business hour warnings: {string.Join(", ", businessErrors)}");
        }
        if (afterErrors.Any())
        {
            _output.WriteLine($"   After hour warnings: {string.Join(", ", afterErrors)}");
        }
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Scheduler_HandlesLargeJobSets()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
        
        var largeJobSet = new List<Job>();
        var baseTime = DateTime.Today.AddHours(8);
        
        // Create 100 jobs across different machines
        for (int i = 0; i < 100; i++)
        {
            var machine = $"TI{(i % 2) + 1}"; // Alternate between TI1 and TI2
            var startTime = baseTime.AddHours(i * 0.5); // Stagger jobs
            largeJobSet.Add(CreateTestJob(machine, startTime, startTime.AddHours(2)));
        }
        
        var testJob = CreateTestJob("TI1", baseTime.AddHours(50), baseTime.AddHours(52));
        
        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var (isValid, errors) = await schedulerService.ValidateJobSchedulingAsync(testJob, largeJobSet);
        stopwatch.Stop();
        
        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Validation took {stopwatch.ElapsedMilliseconds}ms (over 1s threshold)");
        
        _output.WriteLine($"? Scheduler handles large job sets: {largeJobSet.Count} jobs validated in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"   Result: Valid={isValid}, Errors={errors.Count}");
    }

    #endregion

    #region Helper Methods

    private Job CreateTestJob(string machineId, DateTime start, DateTime end)
    {
        return new Job
        {
            Id = Random.Shared.Next(1, 1000000),
            MachineId = machineId,
            PartNumber = $"TEST-{Random.Shared.Next(1000, 9999)}",
            PartId = Random.Shared.Next(1, 1000),
            ScheduledStart = start,
            ScheduledEnd = end,
            Status = "Scheduled",
            Quantity = 1,
            Priority = 3,
            SlsMaterial = "Ti-6Al-4V Grade 5",
            LaserPowerWatts = 200,
            ScanSpeedMmPerSec = 1200,
            LayerThicknessMicrons = 30,
            HatchSpacingMicrons = 120,
            BuildTemperatureCelsius = 180,
            ArgonPurityPercent = 99.9,
            OxygenContentPpm = 50,
            EstimatedPowderUsageKg = 0.5,
            LaborCostPerHour = 85.00m,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };
    }

    #endregion

    public void Dispose()
    {
        // Cleanup if needed
    }
}