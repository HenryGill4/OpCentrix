using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
/// Comprehensive production-ready tests for the Shifts page
/// Tests all functionality including edge cases, error handling, and UI interactions
/// </summary>
public class ShiftsPageComprehensiveTests : IClassFixture<OpCentrixWebApplicationFactory>
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public ShiftsPageComprehensiveTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region Page Load and Authentication Tests

    [Fact]
    public async Task ShiftsPage_RequiresAdminAuthentication()
    {
        // Test unauthorized access
        var response = await _client.GetAsync("/Admin/Shifts");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task ShiftsPage_LoadsWithAuthentication()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        
        // Verify key page elements
        Assert.Contains("Shift Calendar", html);
        Assert.Contains("iOS-style calendar", html);
        Assert.Contains("calendar-container", html);
        Assert.Contains("zoom-controls", html);
        Assert.Contains("shiftsData", html);
        
        _output.WriteLine("? Shifts page loads successfully with admin authentication");
    }

    [Fact]
    public async Task ShiftsPage_ContainsRequiredJavaScript()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();
        
        // Verify JavaScript components
        Assert.Contains("class ShiftCalendar", html);
        Assert.Contains("navigateMonth", html);
        Assert.Contains("setZoomLevel", html);
        Assert.Contains("quickAddShift", html);
        Assert.Contains("editShift", html);
        Assert.Contains("deleteShift", html);
        
        _output.WriteLine("? All required JavaScript functions are present");
    }

    #endregion

    #region Data Seeding and Loading Tests

    [Fact]
    public async Task ShiftsPage_SeedsDefaultShiftsWhenEmpty()
    {
        // Clear existing shifts
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            context.OperatingShifts.RemoveRange(context.OperatingShifts);
            await context.SaveChangesAsync();
        }

        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verify seeding occurred
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var shiftsCount = await context.OperatingShifts.CountAsync();
            Assert.True(shiftsCount >= 12, $"Expected at least 12 default shifts, got {shiftsCount}");
        }
        
        _output.WriteLine("? Default shifts seeded correctly when database is empty");
    }

    [Fact]
    public async Task ShiftsPage_LoadsExistingShiftsCorrectly()
    {
        // Ensure we have test data
        await EnsureTestShiftsExist();
        
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();
        
        // Verify shifts data is serialized correctly
        var jsonStartIndex = html.IndexOf("\"shifts\":[");
        Assert.True(jsonStartIndex > 0, "Shifts JSON data not found");
        
        // Extract and validate JSON
        var jsonStart = html.IndexOf("{", html.IndexOf("id=\"shiftsData\""));
        var jsonEnd = html.IndexOf("</script>", jsonStart);
        var jsonString = html.Substring(jsonStart, jsonEnd - jsonStart).Trim();
        
        var shiftsData = JsonSerializer.Deserialize<ShiftsDataDto>(jsonString, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        Assert.NotNull(shiftsData);
        Assert.True(shiftsData.shifts.Count > 0, "No shifts found in JSON data");
        
        _output.WriteLine($"? Successfully loaded {shiftsData.shifts.Count} shifts and {shiftsData.holidays.Count} holidays");
    }

    #endregion

    #region CRUD Operations Tests

    [Fact]
    public async Task ShiftsPage_AddShift_Success()
    {
        await AuthenticateAsAdminAsync();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");
        
        // Get add form
        var addResponse = await _client.GetAsync("/Admin/Shifts?handler=Add&dayOfWeek=1");
        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);
        var formHtml = await addResponse.Content.ReadAsStringAsync();
        Assert.Contains("Add Operating Shift", formHtml);
        
        // Submit new shift
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "08:00"),
            new("Input.EndTime", "17:00"),
            new("Input.Description", "Test Business Hours"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };
        
        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();
        
        // Verify success response
        Assert.Contains("\"success\":true", responseContent);
        
        // Verify shift was created in database
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var createdShift = await context.OperatingShifts
                .FirstOrDefaultAsync(s => s.Description == "Test Business Hours");
            
            Assert.NotNull(createdShift);
            Assert.Equal(1, createdShift.DayOfWeek);
            Assert.Equal(new TimeSpan(8, 0, 0), createdShift.StartTime);
            Assert.Equal(new TimeSpan(17, 0, 0), createdShift.EndTime);
        }
        
        _output.WriteLine("? Shift creation works correctly");
    }

    [Fact]
    public async Task ShiftsPage_EditShift_Success()
    {
        await AuthenticateAsAdminAsync();
        await EnsureTestShiftsExist();
        
        // Get existing shift ID
        int shiftId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var shift = await context.OperatingShifts.FirstAsync();
            shiftId = shift.Id;
        }
        
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");
        
        // Get edit form
        var editResponse = await _client.GetAsync($"/Admin/Shifts?handler=Edit&id={shiftId}");
        Assert.Equal(HttpStatusCode.OK, editResponse.StatusCode);
        var formHtml = await editResponse.Content.ReadAsStringAsync();
        Assert.Contains("Edit Shift", formHtml);
        
        // Submit updated shift
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Input.Id", shiftId.ToString()),
            new("Input.DayOfWeek", "2"),
            new("Input.StartTime", "09:00"),
            new("Input.EndTime", "18:00"),
            new("Input.Description", "Updated Test Shift"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };
        
        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("\"success\":true", responseContent);
        
        // Verify changes in database
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var updatedShift = await context.OperatingShifts.FindAsync(shiftId);
            
            Assert.NotNull(updatedShift);
            Assert.Equal("Updated Test Shift", updatedShift.Description);
            Assert.Equal(2, updatedShift.DayOfWeek);
            Assert.Equal(new TimeSpan(9, 0, 0), updatedShift.StartTime);
            Assert.Equal(new TimeSpan(18, 0, 0), updatedShift.EndTime);
        }
        
        _output.WriteLine("? Shift editing works correctly");
    }

    [Fact]
    public async Task ShiftsPage_DeleteShift_Success()
    {
        await AuthenticateAsAdminAsync();
        await EnsureTestShiftsExist();
        
        // Get existing shift ID
        int shiftId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var shift = await context.OperatingShifts.FirstAsync();
            shiftId = shift.Id;
        }
        
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");
        
        // Delete shift
        var formData = new List<KeyValuePair<string, string>>
        {
            new("__RequestVerificationToken", token)
        };
        
        var response = await _client.PostAsync($"/Admin/Shifts?handler=Delete&id={shiftId}", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("\"success\":true", responseContent);
        
        // Verify deletion
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var deletedShift = await context.OperatingShifts.FindAsync(shiftId);
            Assert.Null(deletedShift);
        }
        
        _output.WriteLine("? Shift deletion works correctly");
    }

    [Fact]
    public async Task ShiftsPage_ToggleActive_Success()
    {
        await AuthenticateAsAdminAsync();
        await EnsureTestShiftsExist();
        
        // Get existing shift
        int shiftId;
        bool originalStatus;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var shift = await context.OperatingShifts.FirstAsync();
            shiftId = shift.Id;
            originalStatus = shift.IsActive;
        }
        
        // Toggle active status
        var response = await _client.GetAsync($"/Admin/Shifts?handler=ToggleActive&id={shiftId}");
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("\"success\":true", responseContent);
        
        // Verify status changed
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var shift = await context.OperatingShifts.FindAsync(shiftId);
            Assert.NotNull(shift);
            Assert.Equal(!originalStatus, shift.IsActive);
        }
        
        _output.WriteLine("? Shift status toggle works correctly");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ShiftsPage_RejectsInvalidShiftData()
    {
        await AuthenticateAsAdminAsync();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");
        
        // Test invalid time range (end before start)
        var invalidFormData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "17:00"),
            new("Input.EndTime", "08:00"),
            new("Input.Description", "Invalid Shift"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };
        
        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(invalidFormData));
        var responseContent = await response.Content.ReadAsStringAsync();
        
        // Should return form with validation errors (not success JSON)
        Assert.DoesNotContain("\"success\":true", responseContent);
        
        _output.WriteLine("? Invalid shift data validation works correctly");
    }

    [Fact]
    public async Task ShiftsPage_DetectsShiftConflicts()
    {
        await AuthenticateAsAdminAsync();
        await EnsureTestShiftsExist();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");
        
        // Create overlapping shift
        var conflictingFormData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "07:00"),
            new("Input.EndTime", "12:00"),
            new("Input.Description", "Conflicting Shift"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };
        
        var response = await _client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(conflictingFormData));
        var responseContent = await response.Content.ReadAsStringAsync();
        
        // Should detect conflicts and return error
        Assert.Contains("Conflicts with existing shift", responseContent);
        
        _output.WriteLine("? Shift conflict detection works correctly");
    }

    #endregion

    #region Template System Tests

    [Theory]
    [InlineData("business")]
    [InlineData("24x7")]
    [InlineData("twoshift")]
    [InlineData("plant")]
    public async Task ShiftsPage_LoadTemplate_Success(string template)
    {
        await AuthenticateAsAdminAsync();
        
        var response = await _client.GetAsync($"/Admin/Shifts?handler=LoadTemplate&template={template}");
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("\"success\":true", responseContent);
        Assert.Contains("\"count\":", responseContent);
        
        _output.WriteLine($"? Template '{template}' loads successfully");
    }

    [Fact]
    public async Task ShiftsPage_ApplyTemplate_Success()
    {
        await AuthenticateAsAdminAsync();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");
        
        var formData = new List<KeyValuePair<string, string>>
        {
            new("template", "business"),
            new("clearExisting", "true"),
            new("__RequestVerificationToken", token)
        };
        
        var response = await _client.PostAsync("/Admin/Shifts?handler=ApplyTemplate", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("\"success\":true", responseContent);
        Assert.Contains("Template applied", responseContent);
        
        // Verify shifts were created
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var businessShifts = await context.OperatingShifts
                .Where(s => s.Description == "Standard Business Hours")
                .CountAsync();
            
            Assert.Equal(5, businessShifts); // Monday through Friday
        }
        
        _output.WriteLine("? Template application works correctly");
    }

    #endregion

    #region Assignment Panel Tests

    [Fact]
    public async Task ShiftsPage_LoadAssignmentsPanel_Success()
    {
        await AuthenticateAsAdminAsync();
        await EnsureTestMachinesExist();
        
        var response = await _client.GetAsync("/Admin/Shifts?handler=Assignments");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Assignments", html);
        Assert.Contains("assignMachineSelect", html);
        
        _output.WriteLine("? Assignments panel loads correctly");
    }

    [Fact]
    public async Task ShiftsPage_AssignOperator_Success()
    {
        await AuthenticateAsAdminAsync();
        await EnsureTestMachinesAndOperatorsExist();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");
        
        string machineId;
        int userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            machineId = (await context.Machines.FirstAsync()).MachineId;
            userId = (await context.Users.FirstAsync(u => u.Role == UserRoles.Operator)).Id;
        }
        
        var formData = new List<KeyValuePair<string, string>>
        {
            new("machineId", machineId),
            new("userId", userId.ToString()),
            new("isPrimary", "true"),
            new("force", "true"),
            new("__RequestVerificationToken", token)
        };
        
        var response = await _client.PostAsync("/Admin/Shifts?handler=AssignForm", new FormUrlEncodedContent(formData));
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("\"success\":true", responseContent);
        
        // Verify assignment was created
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var assignment = await context.MachineOperatorAssignments
                .FirstOrDefaultAsync(a => a.MachineId == machineId && a.UserId == userId);
            
            Assert.NotNull(assignment);
            Assert.True(assignment.IsPrimary);
        }
        
        _output.WriteLine("? Operator assignment works correctly");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ShiftsPage_HandlesNonExistentShift_Gracefully()
    {
        await AuthenticateAsAdminAsync();
        
        // Try to edit non-existent shift
        var response = await _client.GetAsync("/Admin/Shifts?handler=Edit&id=99999");
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("\"success\":false", responseContent);
        Assert.Contains("not found", responseContent.ToLower());
        
        _output.WriteLine("? Non-existent shift handling works correctly");
    }

    [Fact]
    public async Task ShiftsPage_HandlesInvalidTemplateGracefully()
    {
        await AuthenticateAsAdminAsync();
        
        var response = await _client.GetAsync("/Admin/Shifts?handler=LoadTemplate&template=invalid");
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("\"success\":false", responseContent);
        
        _output.WriteLine("? Invalid template handling works correctly");
    }

    #endregion

    #region Service Layer Tests

    [Fact]
    public async Task OperatingShiftService_CreateShift_ValidatesCorrectly()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IOperatingShiftService>();
        
        var validShift = new OperatingShift
        {
            DayOfWeek = 1,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            Description = "Test Shift",
            IsActive = true,
            CreatedBy = "Test"
        };
        
        var result = await service.CreateShiftAsync(validShift);
        Assert.True(result);
        
        // Verify it was created
        var allShifts = await service.GetAllShiftsAsync();
        Assert.Contains(allShifts, s => s.Description == "Test Shift");
        
        _output.WriteLine("? Service layer validation works correctly");
    }

    [Fact]
    public async Task OperatingShiftService_ConflictDetection_WorksCorrectly()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IOperatingShiftService>();
        
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
        var conflictingShift = new OperatingShift
        {
            DayOfWeek = 1,
            StartTime = new TimeSpan(16, 0, 0),
            EndTime = new TimeSpan(20, 0, 0),
            Description = "Conflicting Shift",
            IsActive = true,
            CreatedBy = "Test"
        };
        
        var conflicts = await service.GetConflictingShiftsAsync(conflictingShift);
        Assert.Single(conflicts);
        Assert.Equal("First Shift", conflicts.First().Description);
        
        _output.WriteLine("? Conflict detection service works correctly");
    }

    [Fact]
    public async Task OperatingShiftService_OvernightShifts_HandledCorrectly()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IOperatingShiftService>();
        
        var overnightShift = new OperatingShift
        {
            DayOfWeek = 1,
            StartTime = new TimeSpan(22, 0, 0),
            EndTime = new TimeSpan(6, 0, 0), // Next day
            Description = "Night Shift",
            IsActive = true,
            CreatedBy = "Test"
        };
        
        var result = await service.CreateShiftAsync(overnightShift);
        Assert.True(result);
        
        // Test time within shift
        var testTime = new DateTime(2024, 1, 1, 23, 0, 0); // Monday 11 PM
        var isWithin = await service.IsTimeWithinOperatingHoursAsync(testTime);
        Assert.True(isWithin);
        
        // Test early morning of next day
        var earlyMorning = new DateTime(2024, 1, 2, 5, 0, 0); // Tuesday 5 AM
        var isWithinEarly = await service.IsTimeWithinOperatingHoursAsync(earlyMorning);
        Assert.True(isWithinEarly);
        
        _output.WriteLine("? Overnight shift handling works correctly");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task ShiftsPage_LoadsWithinReasonableTime()
    {
        await AuthenticateAsAdminAsync();
        await EnsureTestShiftsExist();
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync("/Admin/Shifts");
        stopwatch.Stop();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Page load took {stopwatch.ElapsedMilliseconds}ms, expected under 5000ms");
        
        _output.WriteLine($"? Page loads in {stopwatch.ElapsedMilliseconds}ms");
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

    private async Task EnsureTestShiftsExist()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        
        if (!await context.OperatingShifts.AnyAsync())
        {
            var service = scope.ServiceProvider.GetRequiredService<IOperatingShiftService>();
            var defaultShifts = DefaultOperatingShifts.GetPlantTwoShiftSchedule();
            
            foreach (var shift in defaultShifts)
            {
                shift.CreatedBy = "Test";
                await service.CreateShiftAsync(shift);
            }
        }
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

    private async Task EnsureTestMachinesAndOperatorsExist()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        
        await EnsureTestMachinesExist();
        
        if (!await context.Users.AnyAsync(u => u.Role == UserRoles.Operator))
        {
            context.Users.Add(new User
            {
                Username = "testop",
                Email = "testop@example.com",
                PasswordHash = "test",
                Role = UserRoles.Operator,
                IsActive = true
            });
            
            await context.SaveChangesAsync();
        }
    }

    #endregion

    #region DTOs for Testing

    private class ShiftsDataDto
    {
        public List<ShiftDto> shifts { get; set; } = new();
        public List<ShiftDto> holidays { get; set; } = new();
    }

    private class ShiftDto
    {
        public int id { get; set; }
        public int dayOfWeek { get; set; }
        public string startTime { get; set; } = "";
        public string endTime { get; set; } = "";
        public string description { get; set; } = "";
        public bool isActive { get; set; }
        public bool isHoliday { get; set; }
        public bool isOvernight { get; set; }
        public double duration { get; set; }
        public string machineId { get; set; } = "";
    }

    #endregion
}