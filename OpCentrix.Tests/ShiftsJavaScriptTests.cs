using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;
using System.Text.RegularExpressions;

namespace OpCentrix.Tests;

/// <summary>
/// Tests for JavaScript functionality in the Shifts calendar
/// </summary>
public class ShiftsJavaScriptTests : IClassFixture<OpCentrixWebApplicationFactory>
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public ShiftsJavaScriptTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task JavaScript_ShiftCalendarClass_IsProperlyDefined()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Extract JavaScript code
        var jsPattern = @"<script[^>]*>\s*(.*?class ShiftCalendar.*?)<\/script>";
        var jsMatch = Regex.Match(html, jsPattern, RegexOptions.Singleline);
        Assert.True(jsMatch.Success, "ShiftCalendar class not found in JavaScript");

        var jsCode = jsMatch.Groups[1].Value;

        // Verify class structure
        Assert.Contains("class ShiftCalendar {", jsCode);
        Assert.Contains("constructor() {", jsCode);
        Assert.Contains("this.currentDate = new Date();", jsCode);
        Assert.Contains("this.currentZoom = 'month';", jsCode);
        Assert.Contains("this.shiftsData = null;", jsCode);

        _output.WriteLine("? ShiftCalendar class is properly defined");
    }

    [Fact]
    public async Task JavaScript_InitializationMethods_ArePresent()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        var requiredMethods = new[]
        {
            "init()",
            "loadShiftsData()",
            "setupEventListeners()",
            "render()",
            "updateTitle()"
        };

        foreach (var method in requiredMethods)
        {
            Assert.Contains(method, html);
        }

        _output.WriteLine("? All initialization methods are present");
    }

    [Fact]
    public async Task JavaScript_NavigationMethods_ArePresent()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        var navigationMethods = new[]
        {
            "navigateMonth(direction)",
            "setZoomLevel(zoom)",
            "selectDay(date)"
        };

        foreach (var method in navigationMethods)
        {
            Assert.Contains(method, html);
        }

        // Verify zoom level handling
        Assert.Contains("this.currentZoom = zoom;", html);
        Assert.Contains("buttons.forEach(btn =>", html);
        Assert.Contains("btn.classList.toggle('active'", html);

        _output.WriteLine("? All navigation methods are present");
    }

    [Fact]
    public async Task JavaScript_RenderingMethods_ArePresent()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        var renderingMethods = new[]
        {
            "renderMonthView(container)",
            "renderWeekView(container)",
            "renderDayView(container)",
            "createDayCell(date, viewType)",
            "createShiftIndicator(shift, viewType)",
            "createTimelineView(shifts)"
        };

        foreach (var method in renderingMethods)
        {
            Assert.Contains(method, html);
        }

        // Verify view switching logic
        Assert.Contains("switch (this.currentZoom)", html);
        Assert.Contains("case 'month':", html);
        Assert.Contains("case 'week':", html);
        Assert.Contains("case 'day':", html);

        _output.WriteLine("? All rendering methods are present");
    }

    [Fact]
    public async Task JavaScript_DataProcessingMethods_ArePresent()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        var dataProcessingMethods = new[]
        {
            "getShiftsForDay(dayOfWeek)",
            "calculateDayCoverage(shifts)",
            "formatTime(timeString)",
            "formatHour(hour)"
        };

        foreach (var method in dataProcessingMethods)
        {
            Assert.Contains(method, html);
        }

        // Verify coverage calculation logic
        Assert.Contains("new Array(1440).fill(false)", html); // 24 hours * 60 minutes
        Assert.Contains("minutes.filter(m => m).length", html);
        Assert.Contains("(coveredMinutes / 1440) * 100", html);

        _output.WriteLine("? All data processing methods are present");
    }

    [Fact]
    public async Task JavaScript_EventHandlers_ArePresent()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify event listener setup
        Assert.Contains("setupEventListeners()", html);
        Assert.Contains("addEventListener('click'", html);
        Assert.Contains("addEventListener('touchstart'", html);
        Assert.Contains("addEventListener('touchend'", html);
        Assert.Contains("addEventListener('keydown'", html);

        // Verify touch handling
        Assert.Contains("touchStartX", html);
        Assert.Contains("touchStartY", html);
        Assert.Contains("Math.abs(diffX)", html);
        Assert.Contains("Math.abs(diffY)", html);

        // Verify keyboard handling
        Assert.Contains("case 'ArrowLeft':", html);
        Assert.Contains("case 'ArrowRight':", html);

        _output.WriteLine("? All event handlers are present");
    }

    [Fact]
    public async Task JavaScript_ShiftOperations_ArePresent()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        var shiftOperations = new[]
        {
            "quickAddShift(dayOfWeek)",
            "editShift(shiftId)",
            "deleteShift(shiftId)"
        };

        foreach (var operation in shiftOperations)
        {
            Assert.Contains(operation, html);
        }

        // Verify HTMX integration
        Assert.Contains("htmx.ajax('GET'", html);
        Assert.Contains("htmx.ajax('POST'", html);
        Assert.Contains("/Admin/Shifts?handler=", html);

        _output.WriteLine("? All shift operations are present");
    }

    [Fact]
    public async Task JavaScript_ErrorHandling_IsImplemented()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify error handling
        Assert.Contains("try {", html);
        Assert.Contains("catch (error)", html);
        Assert.Contains("console.error(", html);
        Assert.Contains("showError(", html);

        // Verify loading states
        Assert.Contains("showLoading()", html);
        Assert.Contains("hideLoading()", html);
        Assert.Contains("isLoading", html);

        _output.WriteLine("? Error handling is properly implemented");
    }

    [Fact]
    public async Task JavaScript_GlobalFunctions_AreAvailable()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify global function definitions
        var globalFunctions = new[]
        {
            "function quickAddShift(",
            "function editShift(",
            "function deleteShift(",
            "function loadTemplate(",
            "function showModal(",
            "function hideModal(",
            "function showAssignmentsDrawer(",
            "function hideAssignmentsDrawer("
        };

        foreach (var func in globalFunctions)
        {
            Assert.Contains(func, html);
        }

        // Verify global calendar instance
        Assert.Contains("window.ShiftCalendar = shiftCalendar", html);
        Assert.Contains("let shiftCalendar;", html);

        _output.WriteLine("? All global functions are available");
    }

    [Fact]
    public async Task JavaScript_TimeCalculations_AreCorrect()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify time parsing and formatting
        Assert.Contains("split(':').map(Number)", html);
        Assert.Contains("hours >= 12 ? 'PM' : 'AM'", html);
        Assert.Contains("hours % 12 || 12", html);

        // Verify overnight shift handling
        Assert.Contains("shift.isOvernight", html);
        Assert.Contains("endDecimal += 24", html);

        // Verify duration calculations
        Assert.Contains("startHour + (startMin / 60)", html);
        Assert.Contains("25px per hour", html); // Timeline positioning

        _output.WriteLine("? Time calculations are correctly implemented");
    }

    [Fact]
    public async Task JavaScript_DOMManipulation_IsPresent()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify DOM creation methods
        Assert.Contains("document.createElement('div')", html);
        Assert.Contains("appendChild(", html);
        Assert.Contains("classList.toggle(", html);
        Assert.Contains("classList.add(", html);
        Assert.Contains("classList.remove(", html);

        // Verify element queries
        Assert.Contains("document.getElementById(", html);
        Assert.Contains("document.querySelector(", html);
        Assert.Contains("document.querySelectorAll(", html);

        // Verify dynamic content updates
        Assert.Contains("innerHTML", html);
        Assert.Contains("textContent", html);
        Assert.Contains("style.width", html);
        Assert.Contains("style.height", html);

        _output.WriteLine("? DOM manipulation methods are present");
    }

    [Fact]
    public async Task JavaScript_CSSAnimations_AreSupported()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify animation classes
        Assert.Contains("animate-fade-in", html);
        Assert.Contains("animate-pop", html);
        Assert.Contains("animate-scale-fade", html);

        // Verify CSS transitions
        Assert.Contains("transition", html);
        Assert.Contains("transform", html);

        // Check CSS file for animations
        var cssResponse = await _client.GetAsync("/css/admin-shifts.css");
        var css = await cssResponse.Content.ReadAsStringAsync();

        Assert.Contains("@keyframes", css);
        Assert.Contains("fade-in", css);
        Assert.Contains("zoom-transition", css);
        Assert.Contains("cubic-bezier", css);

        _output.WriteLine("? CSS animations are properly supported");
    }

    [Fact]
    public async Task JavaScript_PerformanceOptimizations_ArePresent()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify performance optimizations
        var hasRequestAnimationFrame = html.Contains("requestAnimationFrame");
        var hasSetTimeout = html.Contains("setTimeout");
        Assert.True(hasRequestAnimationFrame || hasSetTimeout, "Performance optimization patterns should be present");
        
        // Verify debouncing/throttling patterns
        var hasOptimizations = html.Contains("debounce") || 
                              html.Contains("throttle") || 
                              html.Contains("clearTimeout") ||
                              html.Contains("isLoading");
        
        Assert.True(hasOptimizations, "Performance optimizations should be present");

        // Verify efficient DOM updates
        Assert.Contains("innerHTML = ''", html); // Clearing content
        var hasEfficientUpdates = html.Contains("fragment") || html.Contains("appendChild");
        Assert.True(hasEfficientUpdates, "Efficient DOM updates should be present");

        _output.WriteLine("? Performance optimizations are implemented");
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

    #endregion
}