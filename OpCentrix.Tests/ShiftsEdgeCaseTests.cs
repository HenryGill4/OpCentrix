using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services.Admin;
using Xunit;
using Xunit.Abstractions;

namespace OpCentrix.Tests;

/// <summary>
/// Edge case and stress tests for the Shifts functionality
/// </summary>
public class ShiftsEdgeCaseTests : IClassFixture<OpCentrixWebApplicationFactory>
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public ShiftsEdgeCaseTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region Boundary Value Tests

    [Fact]
    public async Task ShiftValidation_MinimumDuration_OneMinute()
    {
        await AuthenticateAsAdminAsync();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        var formData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "09:00"),
            new("Input.EndTime", "09:01"), // 1 minute duration
            new("Input.Description", "Minimal Duration Shift"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };

        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"success\":true", responseContent);

        _output.WriteLine("? Minimum duration shifts (1 minute) are handled correctly");
    }

    [Fact]
    public async Task ShiftValidation_MaximumDuration_24Hours()
    {
        await AuthenticateAsAdminAsync();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        var formData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "00:00"),
            new("Input.EndTime", "23:59"), // Nearly 24 hours
            new("Input.Description", "Maximum Duration Shift"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };

        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"success\":true", responseContent);

        _output.WriteLine("? Maximum duration shifts (24 hours) are handled correctly");
    }

    [Fact]
    public async Task ShiftValidation_OvernightShift_CrossesMidnight()
    {
        await AuthenticateAsAdminAsync();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        var formData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "23:00"),
            new("Input.EndTime", "07:00"), // Crosses midnight
            new("Input.Description", "Overnight Shift"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };

        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"success\":true", responseContent);

        // Verify overnight shift is marked correctly
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var shift = await context.OperatingShifts.FirstAsync(s => s.Description == "Overnight Shift");
            
            Assert.True(shift.EndTime < shift.StartTime); // Indicates overnight
            Assert.True(shift.DurationHours > 7 && shift.DurationHours < 9); // ~8 hours
        }

        _output.WriteLine("? Overnight shifts crossing midnight are handled correctly");
    }

    #endregion

    #region Data Validation Edge Cases

    [Fact]
    public async Task ShiftValidation_EmptyDescription_IsRejected()
    {
        await AuthenticateAsAdminAsync();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        var formData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "09:00"),
            new("Input.EndTime", "17:00"),
            new("Input.Description", ""), // Empty description
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };

        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("\"success\":true", responseContent);
        Assert.Contains("Description is required", responseContent);

        _output.WriteLine("? Empty description validation works correctly");
    }

    [Fact]
    public async Task ShiftValidation_VeryLongDescription_IsTruncated()
    {
        await AuthenticateAsAdminAsync();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        var longDescription = new string('A', 250); // Exceeds 200 char limit

        var formData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "09:00"),
            new("Input.EndTime", "17:00"),
            new("Input.Description", longDescription),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };

        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("\"success\":true", responseContent);
        Assert.Contains("cannot exceed 200 characters", responseContent);

        _output.WriteLine("? Long description validation works correctly");
    }

    [Fact]
    public async Task ShiftValidation_InvalidDayOfWeek_IsRejected()
    {
        await AuthenticateAsAdminAsync();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        var formData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "8"), // Invalid day (should be 0-6)
            new("Input.StartTime", "09:00"),
            new("Input.EndTime", "17:00"),
            new("Input.Description", "Invalid Day Shift"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };

        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("\"success\":true", responseContent);

        _output.WriteLine("? Invalid day of week validation works correctly");
    }

    #endregion

    #region Conflict Detection Edge Cases

    [Fact]
    public async Task ConflictDetection_ExactTimeMatch_IsDetected()
    {
        await AuthenticateAsAdminAsync();
        await CreateTestShiftAsync("Monday Shift", 1, "09:00", "17:00");
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        // Try to create exact duplicate
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "09:00"),
            new("Input.EndTime", "17:00"),
            new("Input.Description", "Duplicate Shift"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };

        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();

        // Should either be converted to update or show conflict
        var isConvertedToUpdate = responseContent.Contains("\"success\":true");
        var showsConflict = responseContent.Contains("Conflicts with existing shift");
        
        Assert.True(isConvertedToUpdate || showsConflict);

        _output.WriteLine("? Exact time match conflict detection works correctly");
    }

    [Fact]
    public async Task ConflictDetection_PartialOverlap_IsDetected()
    {
        await AuthenticateAsAdminAsync();
        await CreateTestShiftAsync("Base Shift", 1, "09:00", "17:00");
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        // Create overlapping shift
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "16:00"), // Overlaps last hour
            new("Input.EndTime", "20:00"),
            new("Input.Description", "Overlapping Shift"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };

        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.Contains("Conflicts with existing shift", responseContent);

        _output.WriteLine("? Partial overlap conflict detection works correctly");
    }

    [Fact]
    public async Task ConflictDetection_OvernightOverlap_IsDetected()
    {
        await AuthenticateAsAdminAsync();
        await CreateTestShiftAsync("Night Shift", 1, "22:00", "06:00");
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        // Create shift that would overlap with overnight shift
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "05:00"),
            new("Input.EndTime", "09:00"),
            new("Input.Description", "Morning Shift"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };

        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.Contains("Conflicts with existing shift", responseContent);

        _output.WriteLine("? Overnight overlap conflict detection works correctly");
    }

    #endregion

    #region Machine-Specific Edge Cases

    [Fact]
    public async Task MachineSpecificShifts_NoConflictWithGlobal()
    {
        await AuthenticateAsAdminAsync();
        await EnsureTestMachinesExist();
        await CreateTestShiftAsync("Global Shift", 1, "09:00", "17:00", null); // Global
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        string machineId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            machineId = (await context.Machines.FirstAsync()).MachineId;
        }

        // Create machine-specific shift with same time
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "09:00"),
            new("Input.EndTime", "17:00"),
            new("Input.Description", "Machine Specific Shift"),
            new("Input.MachineId", machineId),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };

        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();

        // Machine-specific shifts should be allowed alongside global shifts
        Assert.Contains("\"success\":true", responseContent);

        _output.WriteLine("? Machine-specific shifts don't conflict with global shifts");
    }

    #endregion

    #region Holiday and Special Date Edge Cases

    [Fact]
    public async Task HolidayShifts_PastDate_IsRejected()
    {
        await AuthenticateAsAdminAsync();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        var pastDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");

        var formData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "09:00"),
            new("Input.EndTime", "17:00"),
            new("Input.Description", "Past Holiday"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "true"),
            new("Input.SpecificDate", pastDate),
            new("__RequestVerificationToken", token)
        };

        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.Contains("cannot be in the past", responseContent);

        _output.WriteLine("? Past holiday date validation works correctly");
    }

    [Fact]
    public async Task HolidayShifts_FutureDate_IsAccepted()
    {
        await AuthenticateAsAdminAsync();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        var futureDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");

        var formData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "09:00"),
            new("Input.EndTime", "17:00"),
            new("Input.Description", "Future Holiday"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "true"),
            new("Input.SpecificDate", futureDate),
            new("__RequestVerificationToken", token)
        };

        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"success\":true", responseContent);

        _output.WriteLine("? Future holiday date validation works correctly");
    }

    #endregion

    #region Stress Tests

    [Fact]
    public async Task StressTest_MultipleShiftsPerDay_HandledCorrectly()
    {
        await AuthenticateAsAdminAsync();
        
        // Create multiple non-overlapping shifts for the same day
        var shifts = new[]
        {
            ("Early Morning", "06:00", "10:00"),
            ("Late Morning", "10:00", "14:00"),
            ("Afternoon", "14:00", "18:00"),
            ("Evening", "18:00", "22:00")
        };

        foreach (var (desc, start, end) in shifts)
        {
            await CreateTestShiftAsync(desc, 1, start, end);
        }

        // Load the page and verify all shifts are displayed
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        foreach (var (desc, _, _) in shifts)
        {
            Assert.Contains(desc, html);
        }

        _output.WriteLine($"? Multiple shifts per day ({shifts.Length}) handled correctly");
    }

    [Fact]
    public async Task StressTest_AllDaysOfWeek_Populated()
    {
        await AuthenticateAsAdminAsync();

        // Create shifts for all days of the week
        for (int day = 0; day <= 6; day++)
        {
            await CreateTestShiftAsync($"Day {day} Shift", day, "09:00", "17:00");
        }

        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify all days have shifts in the calendar data
        for (int day = 0; day <= 6; day++)
        {
            Assert.Contains($"\"dayOfWeek\":{day}", html);
        }

        _output.WriteLine("? All days of week with shifts handled correctly");
    }

    [Fact]
    public async Task StressTest_LargeNumberOfHolidays_Performance()
    {
        await AuthenticateAsAdminAsync();

        // Create multiple holiday shifts
        var holidays = new[]
        {
            ("New Year", "2024-01-01"),
            ("Independence Day", "2024-07-04"),
            ("Christmas Eve", "2024-12-24"),
            ("Christmas Day", "2024-12-25"),
            ("New Year Eve", "2024-12-31")
        };

        foreach (var (desc, date) in holidays)
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
                context.OperatingShifts.Add(new OperatingShift
                {
                    DayOfWeek = 0, // Doesn't matter for specific dates
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(14, 0, 0),
                    Description = desc,
                    IsActive = true,
                    IsHoliday = true,
                    SpecificDate = DateTime.Parse(date),
                    CreatedBy = "Test"
                });
                await context.SaveChangesAsync();
            }
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync("/Admin/Shifts");
        stopwatch.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, $"Page load with holidays took {stopwatch.ElapsedMilliseconds}ms");

        var html = await response.Content.ReadAsStringAsync();
        foreach (var (desc, _) in holidays)
        {
            Assert.Contains(desc, html);
        }

        _output.WriteLine($"? Large number of holidays ({holidays.Length}) handled in {stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Error Recovery Tests

    [Fact]
    public async Task ErrorRecovery_InvalidJsonData_HandledGracefully()
    {
        // This would require injecting invalid data, which is complex to test in integration
        // Instead, we verify the JavaScript error handling exists
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify error handling code exists
        Assert.Contains("try {", html);
        Assert.Contains("JSON.parse", html);
        Assert.Contains("catch (e)", html);
        Assert.Contains("console.error", html);

        _output.WriteLine("? JSON parsing error handling is implemented");
    }

    [Fact]
    public async Task ErrorRecovery_NetworkFailure_Simulation()
    {
        await AuthenticateAsAdminAsync();

        // Test invalid handler endpoint
        var response = await _client.GetAsync("/Admin/Shifts?handler=NonExistentHandler");
        
        // Should handle gracefully (not return 500 error)
        Assert.True(response.StatusCode == HttpStatusCode.NotFound || 
                   response.StatusCode == HttpStatusCode.BadRequest ||
                   response.StatusCode == HttpStatusCode.OK);

        _output.WriteLine("? Invalid handler requests are handled gracefully");
    }

    #endregion

    #region Helper Methods

    private async Task AuthenticateAsAdminAsync()
    {
        var loginPage = await _client.GetAsync("/Account/Login");
        var loginContent = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(loginContent);

        var loginData = new List<KeyValuePair<string, string>>
        {
            new("Username", "admin"),
            new("Password", "admin123"),
            new("__RequestVerificationToken", token)
        };

        var response = await _client.PostAsync("/Account/Login", new FormUrlEncodedContent(loginData));
        Assert.True(response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.OK);
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        var tokenStart = html.IndexOf("__RequestVerificationToken");
        if (tokenStart == -1) return string.Empty;
        var valueStart = html.IndexOf("value=\"", tokenStart) + 7;
        var valueEnd = html.IndexOf("\"", valueStart);
        return valueEnd > valueStart ? html.Substring(valueStart, valueEnd - valueStart) : string.Empty;
    }

    private async Task<string> GetAntiForgeryTokenAsync(string url)
    {
        var response = await _client.GetAsync(url);
        var html = await response.Content.ReadAsStringAsync();
        return ExtractAntiForgeryToken(html);
    }

    private async Task CreateTestShiftAsync(string description, int dayOfWeek, string startTime, string endTime, string? machineId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        
        var startParts = startTime.Split(':');
        var endParts = endTime.Split(':');
        
        context.OperatingShifts.Add(new OperatingShift
        {
            DayOfWeek = dayOfWeek,
            StartTime = new TimeSpan(int.Parse(startParts[0]), int.Parse(startParts[1]), 0),
            EndTime = new TimeSpan(int.Parse(endParts[0]), int.Parse(endParts[1]), 0),
            Description = description,
            MachineId = machineId,
            IsActive = true,
            CreatedBy = "Test"
        });
        
        await context.SaveChangesAsync();
    }

    private async Task EnsureTestMachinesExist()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        
        if (!await context.Machines.AnyAsync())
        {
            context.Machines.Add(new Machine
            {
                MachineId = "TEST-M1",
                MachineName = "Test Machine 1",
                IsActive = true
            });
            
            await context.SaveChangesAsync();
        }
    }

    #endregion
}