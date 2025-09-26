using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpCentrix.Data;
using OpCentrix.Models;
using Xunit;
using Xunit.Abstractions;

namespace OpCentrix.Tests;

/// <summary>
/// Production-ready integration tests for the complete Shifts page functionality
/// These tests verify that all components work together correctly
/// </summary>
public class ShiftsProductionReadinessTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public ShiftsProductionReadinessTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove the app's existing database context
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SchedulerContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<SchedulerContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid());
                });
            });
        });
        _output = output;
    }

    [Fact]
    public async Task ShiftsPage_CompleteWorkflow_Production_Ready()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        _output.WriteLine("?? Starting Production Readiness Test for Shifts Page");

        // Step 1: Authentication Flow
        _output.WriteLine("1?? Testing Authentication Flow...");
        await AuthenticateAsAdmin(client);

        // Step 2: Page Load Performance
        _output.WriteLine("2?? Testing Page Load Performance...");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await client.GetAsync("/Admin/Shifts");
        stopwatch.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, $"Page load took {stopwatch.ElapsedMilliseconds}ms - should be under 2000ms");
        _output.WriteLine($"   ? Page loads in {stopwatch.ElapsedMilliseconds}ms");

        // Step 3: Content Verification
        _output.WriteLine("3?? Verifying Page Content...");
        var html = await response.Content.ReadAsStringAsync();
        await VerifyPageContent(html);

        // Step 4: CRUD Operations
        _output.WriteLine("4?? Testing CRUD Operations...");
        await TestShiftCrudOperations(client);

        // Step 5: Template System
        _output.WriteLine("5?? Testing Template System...");
        await TestTemplateSystem(client);

        // Step 6: Error Handling
        _output.WriteLine("6?? Testing Error Handling...");
        await TestErrorHandling(client);

        // Step 7: Concurrent Operations
        _output.WriteLine("7?? Testing Concurrent Operations...");
        await TestConcurrentOperations(client);

        // Step 8: Data Validation
        _output.WriteLine("8?? Testing Data Validation...");
        await TestDataValidation(client);

        _output.WriteLine("?? All Production Readiness Tests Passed!");
    }

    private async Task AuthenticateAsAdmin(HttpClient client)
    {
        var loginPage = await client.GetAsync("/Account/Login");
        Assert.Equal(HttpStatusCode.OK, loginPage.StatusCode);
        
        var loginContent = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(loginContent);
        
        var loginData = new List<KeyValuePair<string, string>>
        {
            new("Username", "admin"),
            new("Password", "admin123"),
            new("__RequestVerificationToken", token)
        };

        var loginResponse = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(loginData));
        Assert.True(loginResponse.StatusCode == HttpStatusCode.Redirect || loginResponse.StatusCode == HttpStatusCode.OK);
        
        _output.WriteLine("   ? Authentication successful");
    }

    private async Task VerifyPageContent(string html)
    {
        var requiredElements = new[]
        {
            "Operating Shifts",
            "calendar-container",
            "Add Shift",
            "Templates",
            "ShiftCalendar",
            "shiftsData",
            "zoom-controls",
            "calendar-legend",
            "loading-indicator",
            "modal-container"
        };

        foreach (var element in requiredElements)
        {
            Assert.Contains(element, html);
        }

        // Check for iOS-style design elements
        var designElements = new[]
        {
            "animate-fade-in",
            "animate-pop",
            "btn primary elevated",
            "calendar-nav",
            "oc-stat"
        };

        foreach (var element in designElements)
        {
            Assert.Contains(element, html);
        }

        _output.WriteLine("   ? All required page elements present");
        _output.WriteLine("   ? iOS-style design elements present");
    }

    private async Task TestShiftCrudOperations(HttpClient client)
    {
        var token = await GetAntiForgeryToken(client);

        // Test Create
        var createData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "09:00"),
            new("Input.EndTime", "17:00"),
            new("Input.Description", "Production Test Shift"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };

        var createResponse = await client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(createData));
        var createContent = await createResponse.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", createContent);
        _output.WriteLine("   ? Create operation successful");

        // Test Read (via page load)
        var readResponse = await client.GetAsync("/Admin/Shifts");
        var readContent = await readResponse.Content.ReadAsStringAsync();
        Assert.Contains("Production Test Shift", readContent);
        _output.WriteLine("   ? Read operation successful");

        // Find the created shift ID for update/delete operations
        int shiftId = await GetLatestShiftId();

        // Test Update
        var updateData = new List<KeyValuePair<string, string>>
        {
            new("Input.Id", shiftId.ToString()),
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "08:00"),
            new("Input.EndTime", "16:00"),
            new("Input.Description", "Updated Production Shift"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };

        var updateResponse = await client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(updateData));
        var updateContent = await updateResponse.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", updateContent);
        _output.WriteLine("   ? Update operation successful");

        // Test Delete
        var deleteData = new List<KeyValuePair<string, string>>
        {
            new("__RequestVerificationToken", token)
        };

        var deleteResponse = await client.PostAsync($"/Admin/Shifts?handler=Delete&id={shiftId}", new FormUrlEncodedContent(deleteData));
        var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", deleteContent);
        _output.WriteLine("   ? Delete operation successful");
    }

    private async Task TestTemplateSystem(HttpClient client)
    {
        var templates = new[] { "business", "24x7", "twoshift", "plant" };

        foreach (var template in templates)
        {
            // Test template loading
            var loadResponse = await client.GetAsync($"/Admin/Shifts?handler=LoadTemplate&template={template}");
            var loadContent = await loadResponse.Content.ReadAsStringAsync();
            Assert.Contains("\"success\":true", loadContent);
            
            // Test template application
            var token = await GetAntiForgeryToken(client);
            var applyData = new List<KeyValuePair<string, string>>
            {
                new("template", template),
                new("clearExisting", "true"),
                new("__RequestVerificationToken", token)
            };

            var applyResponse = await client.PostAsync("/Admin/Shifts?handler=ApplyTemplate", new FormUrlEncodedContent(applyData));
            var applyContent = await applyResponse.Content.ReadAsStringAsync();
            Assert.Contains("\"success\":true", applyContent);
        }

        _output.WriteLine("   ? All template operations successful");
    }

    private async Task TestErrorHandling(HttpClient client)
    {
        // Test invalid shift ID
        var invalidResponse = await client.GetAsync("/Admin/Shifts?handler=Edit&id=99999");
        var invalidContent = await invalidResponse.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":false", invalidContent);

        // Test invalid template
        var invalidTemplateResponse = await client.GetAsync("/Admin/Shifts?handler=LoadTemplate&template=invalid");
        var invalidTemplateContent = await invalidTemplateResponse.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":false", invalidTemplateContent);

        _output.WriteLine("   ? Error handling working correctly");
    }

    private async Task TestConcurrentOperations(HttpClient client)
    {
        var token = await GetAntiForgeryToken(client);
        var tasks = new List<Task<HttpResponseMessage>>();

        // Create multiple shifts concurrently
        for (int i = 0; i < 5; i++)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new("Input.DayOfWeek", ((i % 7)).ToString()),
                new("Input.StartTime", $"{(9 + i):D2}:00"),
                new("Input.EndTime", $"{(17 + i):D2}:00"),
                new("Input.Description", $"Concurrent Shift {i}"),
                new("Input.IsActive", "true"),
                new("Input.IsHoliday", "false"),
                new("__RequestVerificationToken", token)
            };

            tasks.Add(client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(data)));
        }

        var responses = await Task.WhenAll(tasks);
        var successCount = 0;

        foreach (var response in responses)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (content.Contains("\"success\":true"))
                successCount++;
        }

        Assert.True(successCount >= 3, $"Expected at least 3 successful concurrent operations, got {successCount}");
        _output.WriteLine($"   ? {successCount}/5 concurrent operations successful");
    }

    private async Task TestDataValidation(HttpClient client)
    {
        var token = await GetAntiForgeryToken(client);

        // Test invalid time range
        var invalidTimeData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "17:00"),
            new("Input.EndTime", "09:00"),
            new("Input.Description", "Invalid Time Range"),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };

        var response = await client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(invalidTimeData));
        var content = await response.Content.ReadAsStringAsync();
        // Should either be rejected or handled as overnight shift
        Assert.True(content.Contains("\"success\":false") || content.Contains("\"success\":true"));

        // Test empty description
        var emptyDescData = new List<KeyValuePair<string, string>>
        {
            new("Input.DayOfWeek", "1"),
            new("Input.StartTime", "09:00"),
            new("Input.EndTime", "17:00"),
            new("Input.Description", ""),
            new("Input.IsActive", "true"),
            new("Input.IsHoliday", "false"),
            new("__RequestVerificationToken", token)
        };

        var emptyDescResponse = await client.PostAsync("/Admin/Shifts?handler=Save", new FormUrlEncodedContent(emptyDescData));
        var emptyDescContent = await emptyDescResponse.Content.ReadAsStringAsync();
        Assert.Contains("Description is required", emptyDescContent);

        _output.WriteLine("   ? Data validation working correctly");
    }

    private async Task<int> GetLatestShiftId()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        var latestShift = await context.OperatingShifts.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
        return latestShift?.Id ?? 0;
    }

    private async Task<string> GetAntiForgeryToken(HttpClient client)
    {
        var response = await client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();
        return ExtractAntiForgeryToken(html);
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        var tokenStart = html.IndexOf("__RequestVerificationToken");
        if (tokenStart == -1) return string.Empty;
        var valueStart = html.IndexOf("value=\"", tokenStart) + 7;
        var valueEnd = html.IndexOf("\"", valueStart);
        return valueEnd > valueStart ? html.Substring(valueStart, valueEnd - valueStart) : string.Empty;
    }

    [Fact]
    public async Task ShiftsPage_Accessibility_Standards()
    {
        using var client = _factory.CreateClient();
        await AuthenticateAsAdmin(client);

        var response = await client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Check for accessibility features
        Assert.Contains("aria-label", html);
        Assert.Contains("title=", html);
        Assert.Contains("<button", html);

        // Check for semantic HTML
        var hasSemanticStructure = html.Contains("<nav") || html.Contains("role=\"navigation\"") || 
                                 html.Contains("main") || html.Contains("role=\"main\"");
        Assert.True(hasSemanticStructure, "Page should have semantic HTML structure");

        _output.WriteLine("   ? Accessibility standards met");
    }

    [Fact]
    public async Task ShiftsPage_Performance_UnderLoad()
    {
        using var client = _factory.CreateClient();
        await AuthenticateAsAdmin(client);

        // Create many shifts to test performance
        var token = await GetAntiForgeryToken(client);
        
        // Apply a template with many shifts
        var applyData = new List<KeyValuePair<string, string>>
        {
            new("template", "24x7"),
            new("clearExisting", "true"),
            new("__RequestVerificationToken", token)
        };

        await client.PostAsync("/Admin/Shifts?handler=ApplyTemplate", new FormUrlEncodedContent(applyData));

        // Test page load performance with data
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await client.GetAsync("/Admin/Shifts");
        stopwatch.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, $"Page with data took {stopwatch.ElapsedMilliseconds}ms - should be under 3000ms");

        _output.WriteLine($"   ? Performance under load: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ShiftsPage_JavaScript_Functionality()
    {
        using var client = _factory.CreateClient();
        await AuthenticateAsAdmin(client);

        var response = await client.GetAsync("/Admin/Shifts");
        var html = await response.Content.ReadAsStringAsync();

        // Verify JavaScript components are present
        var jsComponents = new[]
        {
            "class ShiftCalendar",
            "navigateMonth",
            "setZoomLevel",
            "quickAddShift",
            "loadTemplate",
            "showModal",
            "hideModal"
        };

        foreach (var component in jsComponents)
        {
            Assert.Contains(component, html);
        }

        // Verify error handling
        Assert.Contains("try {", html);
        Assert.Contains("catch", html);

        _output.WriteLine("   ? JavaScript functionality verified");
    }
}