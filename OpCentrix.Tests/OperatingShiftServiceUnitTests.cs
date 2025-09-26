using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services.Admin;
using Microsoft.Extensions.Logging;

namespace OpCentrix.Tests;

/// <summary>
/// Unit tests for the OperatingShiftService to verify core functionality
/// </summary>
public class OperatingShiftServiceUnitTests
{
    private SchedulerContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<SchedulerContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new SchedulerContext(options);
    }

    private IOperatingShiftService GetShiftService(SchedulerContext context)
    {
        var logger = new TestLogger<OperatingShiftService>();
        return new OperatingShiftService(context, logger);
    }

    private class TestLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            // Test logger - does nothing
        }
    }

    [Fact]
    public async Task CreateShift_ValidShift_ReturnsTrue()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = GetShiftService(context);
        
        var shift = new OperatingShift
        {
            DayOfWeek = 1, // Monday
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            Description = "Business Hours",
            IsActive = true,
            CreatedBy = "Test"
        };

        // Act
        var result = await service.CreateShiftAsync(shift);

        // Assert
        Assert.True(result);
        
        var savedShifts = await service.GetAllShiftsAsync();
        Assert.Single(savedShifts);
        Assert.Equal("Business Hours", savedShifts.First().Description);
    }

    [Fact]
    public async Task CreateShift_ConflictingShift_ReturnsFalse()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = GetShiftService(context);
        
        // Create first shift
        var shift1 = new OperatingShift
        {
            DayOfWeek = 1,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            Description = "First Shift",
            IsActive = true,
            CreatedBy = "Test"
        };
        await service.CreateShiftAsync(shift1);

        // Create conflicting shift
        var shift2 = new OperatingShift
        {
            DayOfWeek = 1,
            StartTime = new TimeSpan(16, 0, 0), // Overlaps with first shift
            EndTime = new TimeSpan(20, 0, 0),
            Description = "Conflicting Shift",
            IsActive = true,
            CreatedBy = "Test"
        };

        // Act
        var result = await service.CreateShiftAsync(shift2);

        // Assert
        Assert.False(result);
        
        var savedShifts = await service.GetAllShiftsAsync();
        Assert.Single(savedShifts); // Only first shift should exist
    }

    [Fact]
    public async Task GetConflictingShifts_OverlappingShifts_ReturnsConflicts()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = GetShiftService(context);
        
        // Create existing shift
        var existingShift = new OperatingShift
        {
            DayOfWeek = 1,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            Description = "Existing Shift",
            IsActive = true,
            CreatedBy = "Test"
        };
        await service.CreateShiftAsync(existingShift);

        // Test overlapping shift
        var testShift = new OperatingShift
        {
            DayOfWeek = 1,
            StartTime = new TimeSpan(16, 0, 0),
            EndTime = new TimeSpan(20, 0, 0),
            Description = "Test Shift",
            IsActive = true,
            CreatedBy = "Test"
        };

        // Act
        var conflicts = await service.GetConflictingShiftsAsync(testShift);

        // Assert
        Assert.Single(conflicts);
        Assert.Equal("Existing Shift", conflicts.First().Description);
    }

    [Fact]
    public async Task GetShiftsForDay_ValidDay_ReturnsShifts()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = GetShiftService(context);
        
        // Create shifts for different days
        var mondayShift = new OperatingShift
        {
            DayOfWeek = 1,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            Description = "Monday Shift",
            IsActive = true,
            CreatedBy = "Test"
        };
        
        var tuesdayShift = new OperatingShift
        {
            DayOfWeek = 2,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            Description = "Tuesday Shift",
            IsActive = true,
            CreatedBy = "Test"
        };

        await service.CreateShiftAsync(mondayShift);
        await service.CreateShiftAsync(tuesdayShift);

        // Act
        var mondayShifts = await service.GetShiftsForDayAsync(DayOfWeek.Monday);
        var tuesdayShifts = await service.GetShiftsForDayAsync(DayOfWeek.Tuesday);

        // Assert
        Assert.Single(mondayShifts);
        Assert.Single(tuesdayShifts);
        Assert.Equal("Monday Shift", mondayShifts.First().Description);
        Assert.Equal("Tuesday Shift", tuesdayShifts.First().Description);
    }

    [Fact]
    public async Task IsTimeWithinOperatingHours_WithinActiveShift_ReturnsTrue()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = GetShiftService(context);
        
        var shift = new OperatingShift
        {
            DayOfWeek = 1, // Monday
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            Description = "Business Hours",
            IsActive = true,
            CreatedBy = "Test"
        };
        await service.CreateShiftAsync(shift);

        // Test time within shift (Monday 2PM)
        var testTime = new DateTime(2024, 1, 1, 14, 0, 0); // Monday 2PM
        while (testTime.DayOfWeek != DayOfWeek.Monday)
        {
            testTime = testTime.AddDays(1);
        }

        // Act
        var result = await service.IsTimeWithinOperatingHoursAsync(testTime);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsTimeWithinOperatingHours_OutsideActiveShift_ReturnsFalse()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = GetShiftService(context);
        
        var shift = new OperatingShift
        {
            DayOfWeek = 1, // Monday
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            Description = "Business Hours",
            IsActive = true,
            CreatedBy = "Test"
        };
        await service.CreateShiftAsync(shift);

        // Test time outside shift (Monday 8PM)
        var testTime = new DateTime(2024, 1, 1, 20, 0, 0); // Monday 8PM
        while (testTime.DayOfWeek != DayOfWeek.Monday)
        {
            testTime = testTime.AddDays(1);
        }

        // Act
        var result = await service.IsTimeWithinOperatingHoursAsync(testTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task OvernightShift_HandledCorrectly()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = GetShiftService(context);
        
        var overnightShift = new OperatingShift
        {
            DayOfWeek = 1, // Monday
            StartTime = new TimeSpan(22, 0, 0), // 10 PM
            EndTime = new TimeSpan(6, 0, 0),   // 6 AM (next day)
            Description = "Night Shift",
            IsActive = true,
            CreatedBy = "Test"
        };

        // Act
        var result = await service.CreateShiftAsync(overnightShift);

        // Assert
        Assert.True(result);
        
        var savedShift = (await service.GetAllShiftsAsync()).First();
        Assert.True(savedShift.EndTime < savedShift.StartTime); // Indicates overnight
        Assert.True(savedShift.DurationHours > 7 && savedShift.DurationHours < 9); // ~8 hours
    }

    [Fact]
    public async Task UpdateShift_ValidShift_ReturnsTrue()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = GetShiftService(context);
        
        var shift = new OperatingShift
        {
            DayOfWeek = 1,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            Description = "Original Description",
            IsActive = true,
            CreatedBy = "Test"
        };
        await service.CreateShiftAsync(shift);
        
        var savedShifts = await service.GetAllShiftsAsync();
        var savedShift = savedShifts.First();
        
        // Update the shift
        savedShift.Description = "Updated Description";
        savedShift.LastModifiedBy = "TestUpdater";

        // Act
        var result = await service.UpdateShiftAsync(savedShift);

        // Assert
        Assert.True(result);
        
        var updatedShifts = await service.GetAllShiftsAsync();
        Assert.Single(updatedShifts);
        Assert.Equal("Updated Description", updatedShifts.First().Description);
        Assert.Equal("TestUpdater", updatedShifts.First().LastModifiedBy);
    }

    [Fact]
    public async Task DeleteShift_ExistingShift_ReturnsTrue()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = GetShiftService(context);
        
        var shift = new OperatingShift
        {
            DayOfWeek = 1,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            Description = "Test Shift",
            IsActive = true,
            CreatedBy = "Test"
        };
        await service.CreateShiftAsync(shift);
        
        var savedShifts = await service.GetAllShiftsAsync();
        var savedShift = savedShifts.First();

        // Act
        var result = await service.DeleteShiftAsync(savedShift.Id);

        // Assert
        Assert.True(result);
        
        var remainingShifts = await service.GetAllShiftsAsync();
        Assert.Empty(remainingShifts);
    }

    [Fact]
    public async Task DeleteShift_NonExistentShift_ReturnsFalse()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = GetShiftService(context);

        // Act
        var result = await service.DeleteShiftAsync(999);

        // Assert
        Assert.False(result);
    }
}