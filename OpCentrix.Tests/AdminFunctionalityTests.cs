using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services.Admin;
using System.Net;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace OpCentrix.Tests;

/// <summary>
/// Essential admin functionality tests for Phase 6: Testing & Quality Assurance
/// Tests core admin control system features
/// </summary>
public class AdminFunctionalityTests : IClassFixture<OpCentrixWebApplicationFactory>, IDisposable
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public AdminFunctionalityTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region Admin Dashboard Tests

    [Fact]
    public async Task AdminDashboard_RequiresAuthentication()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/Admin");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
        _output.WriteLine("? Admin dashboard properly requires authentication");
    }

    [Fact]
    public async Task AdminDashboard_LoadsSuccessfullyForAdminUser()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await _client.GetAsync("/Admin");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Admin Dashboard", content);
        
        _output.WriteLine("? Admin dashboard loads successfully for admin user");
    }

    #endregion

    #region Role Permissions Tests

    [Fact]
    public async Task RolePermissions_LoadsPermissionMatrix()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await _client.GetAsync("/Admin/Roles");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Role-Based Permission Grid", content);
        Assert.Contains("Admin", content);
        
        _output.WriteLine("? Role permissions page loads permission matrix");
    }

    [Fact]
    public async Task RolePermissions_ValidatesPermissions()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var roleService = scope.ServiceProvider.GetRequiredService<IRolePermissionService>();

        // Test different role permissions
        var testCases = new[]
        {
            new { Role = "Admin", Permission = "Admin.ManageUsers", ExpectedResult = true },
            new { Role = "Operator", Permission = "Admin.ManageUsers", ExpectedResult = false },
            new { Role = "Operator", Permission = "Scheduler.ViewJobs", ExpectedResult = true }
        };

        // Act & Assert
        foreach (var testCase in testCases)
        {
            var hasPermission = await roleService.HasPermissionAsync(testCase.Role, testCase.Permission);
            Assert.Equal(testCase.ExpectedResult, hasPermission);
            _output.WriteLine($"? {testCase.Role} {testCase.Permission}: {hasPermission}");
        }
    }

    #endregion

    #region User Management Tests

    [Fact]
    public async Task UserManagement_LoadsUserList()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await _client.GetAsync("/Admin/Users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("User Management", content);
        Assert.Contains("admin", content); // Should show admin user
        
        _output.WriteLine("? User management page loads user list");
    }

    #endregion

    #region Machine Management Tests

    [Fact]
    public async Task MachineManagement_LoadsMachineList()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await _client.GetAsync("/Admin/Machines");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Machine Management", content);
        
        _output.WriteLine("? Machine management page loads machine list");
    }

    [Fact]
    public async Task MachineManagement_ValidatesMachineCapabilities()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var machineService = scope.ServiceProvider.GetRequiredService<IMachineManagementService>();
        
        // Act - Get a machine and its capabilities
        var machines = await machineService.GetActiveMachinesAsync();
        
        if (machines.Any())
        {
            var machine = machines.First();
            var capabilities = await machineService.GetMachineCapabilitiesAsync(machine.Id);
            
            // Assert
            Assert.NotNull(capabilities);
            _output.WriteLine($"? Machine {machine.MachineId} has {capabilities.Count()} capabilities");
        }
        else
        {
            _output.WriteLine("?? No machines found for capability testing");
        }
    }

    #endregion

    #region Performance and Integration Tests

    [Fact]
    public async Task AdminPages_LoadWithinPerformanceThresholds()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var adminPages = new[]
        {
            "/Admin",
            "/Admin/Users",
            "/Admin/Machines",
            "/Admin/Parts"
        };

        // Act & Assert
        foreach (var page in adminPages)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _client.GetAsync(page);
            stopwatch.Stop();
            
            Assert.True(response.IsSuccessStatusCode, $"Page {page} failed to load");
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Page {page} took {stopwatch.ElapsedMilliseconds}ms (over 5s threshold)");
            
            _output.WriteLine($"? {page}: {stopwatch.ElapsedMilliseconds}ms");
        }
    }

    [Fact]
    public async Task AdminSystem_HandlesConcurrentRequests()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        
        // Act - Make multiple concurrent requests
        var tasks = Enumerable.Range(0, 5).Select(async i =>
        {
            var response = await _client.GetAsync("/Admin");
            return response.IsSuccessStatusCode;
        });
        
        var results = await Task.WhenAll(tasks);
        
        // Assert
        Assert.All(results, result => Assert.True(result));
        _output.WriteLine("? Admin system handles concurrent requests successfully");
    }

    #endregion

    #region Helper Methods

    private async Task AuthenticateAsAdminAsync()
    {
        // Get login page to extract anti-forgery token
        var loginPage = await _client.GetAsync("/Account/Login");
        var loginContent = await loginPage.Content.ReadAsStringAsync();
        
        var token = ExtractAntiForgeryToken(loginContent);
        
        // Prepare login form data
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Username", "admin"),
            new("Password", "admin123"),
            new("__RequestVerificationToken", token)
        };

        // Submit login form
        var loginFormContent = new FormUrlEncodedContent(formData);
        var loginResponse = await _client.PostAsync("/Account/Login", loginFormContent);
        
        // Verify login was successful (should redirect)
        Assert.True(loginResponse.StatusCode == HttpStatusCode.Redirect || loginResponse.StatusCode == HttpStatusCode.OK);
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        var tokenStart = html.IndexOf("__RequestVerificationToken");
        if (tokenStart == -1) return string.Empty;
        
        var valueStart = html.IndexOf("value=\"", tokenStart) + 7;
        var valueEnd = html.IndexOf("\"", valueStart);
        
        return html.Substring(valueStart, valueEnd - valueStart);
    }

    #endregion

    public void Dispose()
    {
        _client?.Dispose();
    }
}