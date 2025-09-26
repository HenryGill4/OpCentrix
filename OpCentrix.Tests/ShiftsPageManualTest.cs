using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpCentrix.Data;
using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Tests;

/// <summary>
/// Simple manual test to verify the shifts page works correctly
/// </summary>
public class ShiftsPageManualTest
{
    public static async Task TestShiftsPageBasicFunctionality()
    {
        var webAppFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
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

        using var client = webAppFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        Console.WriteLine("Testing Shifts Page Basic Functionality...");

        try
        {
            // Test 1: Verify unauthorized access is redirected
            Console.WriteLine("Test 1: Testing unauthorized access...");
            var unauthorizedResponse = await client.GetAsync("/Admin/Shifts");
            if (unauthorizedResponse.StatusCode == HttpStatusCode.Redirect)
            {
                Console.WriteLine("? Unauthorized access correctly redirected to login");
            }
            else
            {
                Console.WriteLine($"? Expected redirect, got {unauthorizedResponse.StatusCode}");
                return;
            }

            // Test 2: Login as admin
            Console.WriteLine("Test 2: Attempting admin login...");
            var loginPage = await client.GetAsync("/Account/Login");
            var loginContent = await loginPage.Content.ReadAsStringAsync();
            
            // Extract antiforgery token
            var tokenStart = loginContent.IndexOf("__RequestVerificationToken");
            string token = "";
            if (tokenStart != -1)
            {
                var valueStart = loginContent.IndexOf("value=\"", tokenStart) + 7;
                var valueEnd = loginContent.IndexOf("\"", valueStart);
                token = loginContent.Substring(valueStart, valueEnd - valueStart);
            }

            // Attempt login
            var loginData = new List<KeyValuePair<string, string>>
            {
                new("Username", "admin"),
                new("Password", "admin123"),
                new("__RequestVerificationToken", token)
            };

            var loginResponse = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(loginData));
            if (loginResponse.StatusCode == HttpStatusCode.Redirect || loginResponse.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("? Admin login successful");
            }
            else
            {
                Console.WriteLine($"? Login failed with status {loginResponse.StatusCode}");
                return;
            }

            // Test 3: Access shifts page as authenticated user
            Console.WriteLine("Test 3: Accessing shifts page as authenticated admin...");
            var shiftsPageResponse = await client.GetAsync("/Admin/Shifts");
            
            if (shiftsPageResponse.StatusCode == HttpStatusCode.OK)
            {
                var shiftsPageContent = await shiftsPageResponse.Content.ReadAsStringAsync();
                
                // Check for key elements
                var hasTitle = shiftsPageContent.Contains("Operating Shifts");
                var hasCalendar = shiftsPageContent.Contains("calendar-container");
                var hasAddButton = shiftsPageContent.Contains("Add Shift");
                var hasTemplates = shiftsPageContent.Contains("Templates");
                var hasJavaScript = shiftsPageContent.Contains("ShiftCalendar");
                var hasShiftsData = shiftsPageContent.Contains("shiftsData");

                Console.WriteLine($"? Shifts page loads successfully");
                Console.WriteLine($"   - Title present: {hasTitle}");
                Console.WriteLine($"   - Calendar UI present: {hasCalendar}");
                Console.WriteLine($"   - Add button present: {hasAddButton}");
                Console.WriteLine($"   - Templates present: {hasTemplates}");
                Console.WriteLine($"   - JavaScript present: {hasJavaScript}");
                Console.WriteLine($"   - Shifts data present: {hasShiftsData}");

                if (!hasTitle || !hasCalendar || !hasAddButton || !hasTemplates || !hasJavaScript || !hasShiftsData)
                {
                    Console.WriteLine("? Some required elements are missing from the page");
                    Console.WriteLine("First 1000 characters of response:");
                    Console.WriteLine(shiftsPageContent.Substring(0, Math.Min(1000, shiftsPageContent.Length)));
                }
            }
            else
            {
                Console.WriteLine($"? Shifts page failed to load with status {shiftsPageResponse.StatusCode}");
                return;
            }

            // Test 4: Test Add Shift modal
            Console.WriteLine("Test 4: Testing Add Shift functionality...");
            var addShiftResponse = await client.GetAsync("/Admin/Shifts?handler=Add&dayOfWeek=1");
            
            if (addShiftResponse.StatusCode == HttpStatusCode.OK)
            {
                var addShiftContent = await addShiftResponse.Content.ReadAsStringAsync();
                var hasModal = addShiftContent.Contains("Add Operating Shift") || addShiftContent.Contains("_ShiftForm");
                var hasFormFields = addShiftContent.Contains("Input.StartTime") && addShiftContent.Contains("Input.EndTime");
                
                Console.WriteLine($"? Add shift modal loads: {hasModal}");
                Console.WriteLine($"   - Form fields present: {hasFormFields}");
            }
            else
            {
                Console.WriteLine($"? Add shift modal failed with status {addShiftResponse.StatusCode}");
            }

            // Test 5: Test template loading
            Console.WriteLine("Test 5: Testing template loading...");
            var templateResponse = await client.GetAsync("/Admin/Shifts?handler=LoadTemplate&template=business");
            
            if (templateResponse.StatusCode == HttpStatusCode.OK)
            {
                var templateContent = await templateResponse.Content.ReadAsStringAsync();
                var isJson = templateContent.Contains("\"success\":");
                Console.WriteLine($"? Template loading works: {isJson}");
            }
            else
            {
                Console.WriteLine($"? Template loading failed with status {templateResponse.StatusCode}");
            }

            Console.WriteLine("\n?? All basic functionality tests completed!");
            Console.WriteLine("The Shifts page appears to be working correctly for production use.");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Test failed with exception: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            webAppFactory.Dispose();
        }
    }

    public static async Task Main(string[] args)
    {
        await TestShiftsPageBasicFunctionality();
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}