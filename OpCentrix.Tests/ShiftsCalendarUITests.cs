using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services.Admin;
using Xunit;
using Xunit.Abstractions;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OpCentrix.Tests;

/// <summary>
/// Tests for the iOS-style calendar UI components and interactions
/// </summary>
public class ShiftsCalendarUITests : IClassFixture<OpCentrixWebApplicationFactory>
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public ShiftsCalendarUITests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task CalendarUI_ContainsRequiredElements()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify main calendar container
        Assert.Contains("calendar-container", html);
        Assert.Contains("calendar-header", html);
        Assert.Contains("calendar-nav", html);
        Assert.Contains("zoom-controls", html);
        Assert.Contains("calendar-content", html);
        Assert.Contains("calendar-legend", html);

        // Verify zoom buttons
        Assert.Contains("data-zoom=\"month\"", html);
        Assert.Contains("data-zoom=\"week\"", html);
        Assert.Contains("data-zoom=\"day\"", html);

        // Verify navigation buttons
        Assert.Contains("ShiftCalendar.navigateMonth(-1)", html);
        Assert.Contains("ShiftCalendar.navigateMonth(1)", html);

        // Verify legend items
        Assert.Contains("legend-dot day-shift", html);
        Assert.Contains("legend-dot night-shift", html);
        Assert.Contains("legend-dot holiday", html);

        _output.WriteLine("? Calendar UI contains all required elements");
    }

    [Fact]
    public async Task CalendarUI_JavaScriptClassExists()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify ShiftCalendar class definition
        Assert.Contains("class ShiftCalendar", html);
        Assert.Contains("constructor()", html);
        Assert.Contains("init()", html);
        Assert.Contains("loadShiftsData()", html);
        Assert.Contains("render()", html);
        Assert.Contains("navigateMonth(direction)", html);
        Assert.Contains("setZoomLevel(zoom)", html);

        // Verify view rendering methods
        Assert.Contains("renderMonthView(container)", html);
        Assert.Contains("renderWeekView(container)", html);
        Assert.Contains("renderDayView(container)", html);

        // Verify helper methods
        Assert.Contains("createDayCell(date, viewType)", html);
        Assert.Contains("createShiftIndicator(shift, viewType)", html);
        Assert.Contains("calculateDayCoverage(shifts)", html);

        _output.WriteLine("? JavaScript ShiftCalendar class is properly defined");
    }

    [Fact]
    public async Task CalendarUI_ShiftsDataSerialization_IsValid()
    {
        await AuthenticateAsAdminAsync();
        await EnsureTestShiftsExist();
        
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Extract shifts data JSON
        var jsonMatch = Regex.Match(html, @"<script type=""application/json"" id=""shiftsData"">\s*(\{.*?\})\s*</script>", RegexOptions.Singleline);
        Assert.True(jsonMatch.Success, "Shifts data JSON not found in page");

        var jsonString = jsonMatch.Groups[1].Value;
        var shiftsData = JsonSerializer.Deserialize<JsonElement>(jsonString);

        // Verify JSON structure
        Assert.True(shiftsData.TryGetProperty("shifts", out var shiftsArray));
        Assert.True(shiftsData.TryGetProperty("holidays", out var holidaysArray));

        Assert.Equal(JsonValueKind.Array, shiftsArray.ValueKind);
        Assert.Equal(JsonValueKind.Array, holidaysArray.ValueKind);

        // Verify shift data structure if shifts exist
        if (shiftsArray.GetArrayLength() > 0)
        {
            var firstShift = shiftsArray[0];
            Assert.True(firstShift.TryGetProperty("id", out _));
            Assert.True(firstShift.TryGetProperty("dayOfWeek", out _));
            Assert.True(firstShift.TryGetProperty("startTime", out _));
            Assert.True(firstShift.TryGetProperty("endTime", out _));
            Assert.True(firstShift.TryGetProperty("description", out _));
            Assert.True(firstShift.TryGetProperty("isActive", out _));
            Assert.True(firstShift.TryGetProperty("isOvernight", out _));
            Assert.True(firstShift.TryGetProperty("duration", out _));

            // Verify time format
            var startTime = firstShift.GetProperty("startTime").GetString();
            Assert.Matches(@"^\d{2}:\d{2}$", startTime);
        }

        _output.WriteLine($"? Shifts data JSON is valid with {shiftsArray.GetArrayLength()} shifts and {holidaysArray.GetArrayLength()} holidays");
    }

    [Fact]
    public async Task CalendarUI_SummaryCards_DisplayCorrectData()
    {
        await AuthenticateAsAdminAsync();
        await EnsureTestShiftsExist();

        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Extract shift counts from summary cards
        var totalShiftsMatch = Regex.Match(html, @"<p class=""label"">Total Shifts</p>\s*<p class=""value"">(\d+)</p>");
        var activeShiftsMatch = Regex.Match(html, @"<p class=""label"">Active Shifts</p>\s*<p class=""value"">(\d+)</p>");
        var nightShiftsMatch = Regex.Match(html, @"<p class=""label"">Night Shifts</p>\s*<p class=""value"">(\d+)</p>");

        Assert.True(totalShiftsMatch.Success, "Total shifts count not found");
        Assert.True(activeShiftsMatch.Success, "Active shifts count not found");
        Assert.True(nightShiftsMatch.Success, "Night shifts count not found");

        var totalShifts = int.Parse(totalShiftsMatch.Groups[1].Value);
        var activeShifts = int.Parse(activeShiftsMatch.Groups[1].Value);
        var nightShifts = int.Parse(nightShiftsMatch.Groups[1].Value);

        // Verify counts make sense
        Assert.True(totalShifts > 0, "No shifts found in summary");
        Assert.True(activeShifts <= totalShifts, "Active shifts count exceeds total");
        Assert.True(nightShifts <= activeShifts, "Night shifts count exceeds active shifts");

        // Verify against database
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var dbTotalShifts = await context.OperatingShifts.CountAsync();
            var dbActiveShifts = await context.OperatingShifts.CountAsync(s => s.IsActive);

            Assert.Equal(dbTotalShifts, totalShifts);
            Assert.Equal(dbActiveShifts, activeShifts);
        }

        _output.WriteLine($"? Summary cards display correct data: {totalShifts} total, {activeShifts} active, {nightShifts} night shifts");
    }

    [Fact]
    public async Task CalendarUI_CSSClasses_ArePresent()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify CSS file is linked
        Assert.Contains("/css/admin-shifts.css", html);

        // Verify key CSS classes are referenced
        var cssClasses = new[]
        {
            "calendar-container",
            "calendar-header",
            "calendar-nav",
            "nav-btn",
            "calendar-title",
            "zoom-controls",
            "zoom-btn",
            "calendar-content",
            "calendar-legend",
            "legend-item",
            "legend-dot",
            "day-shift",
            "night-shift",
            "holiday",
            "shift-indicators",
            "coverage-bar",
            "coverage-fill"
        };

        foreach (var cssClass in cssClasses)
        {
            Assert.Contains(cssClass, html);
        }

        _output.WriteLine("? All required CSS classes are referenced in the HTML");
    }

    [Fact]
    public async Task CalendarUI_ResponsiveDesign_MediaQueries()
    {
        // Get the CSS file directly
        var cssResponse = await _client.GetAsync("/css/admin-shifts.css");
        Assert.Equal(HttpStatusCode.OK, cssResponse.StatusCode);
        
        var css = await cssResponse.Content.ReadAsStringAsync();

        // Verify responsive breakpoints exist
        Assert.Contains("@media (max-width:1200px)", css);
        Assert.Contains("@media (max-width:768px)", css);
        Assert.Contains("@media (max-width:480px)", css);

        // Verify mobile-specific styles
        Assert.Contains("flex-direction:column", css);
        Assert.Contains("touch-optimized", css.ToLower());

        _output.WriteLine("? Responsive design media queries are present");
    }

    [Fact]
    public async Task CalendarUI_LoadingAndErrorStates_Exist()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify loading indicator
        Assert.Contains("loading-indicator", html);
        Assert.Contains("loading-spinner", html);
        Assert.Contains("Loading shift calendar", html);

        // Verify error handling functions
        Assert.Contains("showLoading()", html);
        Assert.Contains("hideLoading()", html);
        Assert.Contains("showError(", html);

        // Verify error states in CSS
        var cssResponse = await _client.GetAsync("/css/admin-shifts.css");
        var css = await cssResponse.Content.ReadAsStringAsync();
        
        Assert.Contains("error-message", css);
        Assert.Contains("error-icon", css);

        _output.WriteLine("? Loading and error states are properly implemented");
    }

    [Fact]
    public async Task CalendarUI_Accessibility_Features()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify ARIA labels and accessibility features
        Assert.Contains("aria-label", html);
        Assert.Contains("title=", html);
        
        // Verify semantic HTML structure
        Assert.Contains("<button", html);
        var hasNavigation = html.Contains("<nav") || html.Contains("role=\"navigation\"");
        Assert.True(hasNavigation, "Navigation structure should be present");
        
        // Verify keyboard navigation support
        Assert.Contains("keydown", html);
        Assert.Contains("ArrowLeft", html);
        Assert.Contains("ArrowRight", html);

        _output.WriteLine("? Accessibility features are implemented");
    }

    [Fact]
    public async Task CalendarUI_TouchGesture_Support()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify touch event handlers
        Assert.Contains("touchstart", html);
        Assert.Contains("touchend", html);
        Assert.Contains("touchStartX", html);
        Assert.Contains("touchStartY", html);

        // Verify swipe detection
        Assert.Contains("Math.abs(diffX)", html);
        Assert.Contains("Math.abs(diffY)", html);

        _output.WriteLine("? Touch gesture support is implemented");
    }

    [Fact]
    public async Task CalendarUI_HolidayTable_RendersCorrectly()
    {
        await AuthenticateAsAdminAsync();
        await EnsureHolidayShiftsExist();

        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify holiday table exists
        Assert.Contains("Holiday Schedules", html);
        Assert.Contains("Special date overrides", html);
        
        // Verify table structure
        Assert.Contains("<table", html);
        Assert.Contains("thead", html);
        Assert.Contains("tbody", html);
        
        // Verify holiday-specific styling
        Assert.Contains("status-chip", html);

        _output.WriteLine("? Holiday table renders correctly");
    }

    [Fact]
    public async Task CalendarUI_TemplateDropdown_FunctionsCorrectly()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify template dropdown
        Assert.Contains("templateDropdown", html);
        Assert.Contains("templateMenu", html);
        Assert.Contains("templateChevron", html);

        // Verify template options
        Assert.Contains("Business Hours", html);
        Assert.Contains("Two-Shift", html);
        Assert.Contains("24/7 Operations", html);

        // Verify template functions
        Assert.Contains("loadTemplate(", html);
        Assert.Contains("onclick=\"loadTemplate('business')\"", html);

        _output.WriteLine("? Template dropdown functions correctly");
    }

    [Fact]
    public async Task CalendarUI_Modal_SystemWorks()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify modal container
        Assert.Contains("modal-container", html);
        Assert.Contains("oc-modal-container", html);

        // Verify modal functions
        Assert.Contains("showModal()", html);
        Assert.Contains("hideModal()", html);

        // Verify modal CSS classes
        var cssResponse = await _client.GetAsync("/css/admin-shifts.css");
        var css = await cssResponse.Content.ReadAsStringAsync();
        
        Assert.Contains("oc-modal-container", css);
        Assert.Contains("backdrop-filter:blur", css);
        Assert.Contains("z-index:9999", css);

        _output.WriteLine("? Modal system works correctly");
    }

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

    private async Task EnsureHolidayShiftsExist()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        
        if (!await context.OperatingShifts.AnyAsync(s => s.IsHoliday))
        {
            context.OperatingShifts.Add(new OperatingShift
            {
                DayOfWeek = 1,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(13, 0, 0),
                Description = "Holiday Hours",
                IsActive = true,
                IsHoliday = true,
                SpecificDate = new DateTime(2024, 12, 25),
                CreatedBy = "Test"
            });

            await context.SaveChangesAsync();
        }
    }

    #endregion
}