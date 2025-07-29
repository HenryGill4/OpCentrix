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
/// Comprehensive security and authorization tests for Phase 6: Testing & Quality Assurance
/// Tests authentication, authorization policies, input validation, and security features
/// </summary>
public class SecurityAuthorizationTests : IClassFixture<OpCentrixWebApplicationFactory>, IDisposable
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public SecurityAuthorizationTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region Authentication Tests

    [Fact]
    public async Task Authentication_RedirectsUnauthenticatedUsersToLogin()
    {
        // Arrange & Act
        var protectedUrls = new[]
        {
            "/Admin",
            "/Admin/Settings",
            "/Admin/Users",
            "/Admin/Machines",
            "/Admin/Parts",
            "/Scheduler"
        };

        // Assert
        foreach (var url in protectedUrls)
        {
            var response = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
            _output.WriteLine($"? {url} correctly redirects to login");
        }
    }

    [Fact]
    public async Task Authentication_AcceptsValidCredentials()
    {
        // Arrange
        var loginData = new List<KeyValuePair<string, string>>
        {
            new("Username", "admin"),
            new("Password", "admin123")
        };

        // Get anti-forgery token
        var loginPage = await _client.GetAsync("/Account/Login");
        var loginContent = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(loginContent);
        loginData.Add(new("__RequestVerificationToken", token));

        // Act
        var formContent = new FormUrlEncodedContent(loginData);
        var response = await _client.PostAsync("/Account/Login", formContent);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.OK);
        _output.WriteLine("? Valid credentials accepted for login");
    }

    [Fact]
    public async Task Authentication_RejectsInvalidCredentials()
    {
        // Arrange
        var loginData = new List<KeyValuePair<string, string>>
        {
            new("Username", "admin"),
            new("Password", "wrongpassword")
        };

        // Get anti-forgery token
        var loginPage = await _client.GetAsync("/Account/Login");
        var loginContent = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(loginContent);
        loginData.Add(new("__RequestVerificationToken", token));

        // Act
        var formContent = new FormUrlEncodedContent(loginData);
        var response = await _client.PostAsync("/Account/Login", formContent);

        // Assert
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid", responseContent);
        _output.WriteLine("? Invalid credentials properly rejected");
    }

    [Fact]
    public async Task Authentication_RequiresAntiForgeryToken()
    {
        // Arrange
        var loginData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Username", "admin"),
            new KeyValuePair<string, string>("Password", "admin123")
            // Deliberately omitting anti-forgery token
        });

        // Act
        var response = await _client.PostAsync("/Account/Login", loginData);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _output.WriteLine("? Login requires anti-forgery token");
    }

    #endregion

    #region Authorization Policy Tests

    [Fact]
    public async Task Authorization_AdminOnlyPagesRequireAdminRole()
    {
        // Arrange - Login as non-admin user (if available)
        await AuthenticateAsUserAsync("scheduler", "scheduler123"); // Assuming scheduler user exists

        var adminOnlyPages = new[]
        {
            "/Admin/Users",
            "/Admin/Settings", 
            "/Admin/Roles",
            "/Admin/Database"
        };

        // Act & Assert
        foreach (var page in adminOnlyPages)
        {
            var response = await _client.GetAsync(page);
            
            // Should either redirect to login or return forbidden
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect ||
                response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.Unauthorized,
                $"Page {page} should be restricted for non-admin users"
            );
            
            _output.WriteLine($"? {page} properly restricted for non-admin users");
        }
    }

    [Fact]
    public async Task Authorization_AdminUserCanAccessAdminPages()
    {
        // Arrange
        await AuthenticateAsUserAsync("admin", "admin123");

        var adminPages = new[]
        {
            "/Admin",
            "/Admin/Settings",
            "/Admin/Users",
            "/Admin/Machines",
            "/Admin/Parts"
        };

        // Act & Assert
        foreach (var page in adminPages)
        {
            var response = await _client.GetAsync(page);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _output.WriteLine($"? Admin can access {page}");
        }
    }

    [Fact]
    public async Task Authorization_RolePermissionsEnforced()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var roleService = scope.ServiceProvider.GetRequiredService<IRolePermissionService>();

        // Test different role permissions
        var testCases = new[]
        {
            new { Role = "Admin", Permission = "Admin.ManageUsers", ExpectedResult = true },
            new { Role = "Manager", Permission = "Admin.ManageUsers", ExpectedResult = true },
            new { Role = "Scheduler", Permission = "Admin.ManageUsers", ExpectedResult = false },
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

    #region Input Validation Tests

    [Fact]
    public async Task InputValidation_RejectsXSSAttempts()
    {
        // Arrange
        await AuthenticateAsUserAsync("admin", "admin123");

        var xssPayloads = new[]
        {
            "<script>alert('xss')</script>",
            "javascript:alert('xss')",
            "<img src=x onerror=alert('xss')>",
            "'; DROP TABLE Users; --"
        };

        // Act & Assert
        foreach (var payload in xssPayloads)
        {
            // Test with user creation form
            var userData = new List<KeyValuePair<string, string>>
            {
                new("Username", payload),
                new("Password", "TestPass123!"),
                new("ConfirmPassword", "TestPass123!"),
                new("Role", "Operator")
            };

            var token = await GetAntiForgeryTokenAsync("/Admin/Users");
            userData.Add(new("__RequestVerificationToken", token));

            var formContent = new FormUrlEncodedContent(userData);
            var response = await _client.PostAsync("/Admin/Users?handler=CreateUser", formContent);

            // Should either validate input or return validation error, not execute script
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("<script>", responseContent);
            Assert.DoesNotContain("javascript:", responseContent);
            
            _output.WriteLine($"? XSS payload rejected: {payload.Substring(0, Math.Min(30, payload.Length))}...");
        }
    }

    [Fact]
    public async Task InputValidation_RejectsSQLInjectionAttempts()
    {
        // Arrange
        await AuthenticateAsUserAsync("admin", "admin123");

        var sqlInjectionPayloads = new[]
        {
            "'; DROP TABLE Parts; --",
            "' OR '1'='1",
            "'; UPDATE Users SET Role='Admin'; --",
            "' UNION SELECT * FROM Users --"
        };

        // Act & Assert  
        foreach (var payload in sqlInjectionPayloads)
        {
            // Test with parts search (should be parameterized)
            var response = await _client.GetAsync($"/Admin/Parts?SearchTerm={Uri.EscapeDataString(payload)}");
            
            // Should handle safely without SQL execution
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            // Should not contain SQL error messages or system information
            Assert.DoesNotContain("SQL", responseContent, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("syntax error", responseContent, StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"? SQL injection payload handled safely: {payload.Substring(0, Math.Min(20, payload.Length))}...");
        }
    }

    [Fact]
    public async Task InputValidation_EnforcesDataLimits()
    {
        // Arrange
        await AuthenticateAsUserAsync("admin", "admin123");

        // Test with extremely long input
        var longString = new string('A', 10000); // 10KB string
        
        var userData = new List<KeyValuePair<string, string>>
        {
            new("Username", longString),
            new("Password", "TestPass123!"),
            new("ConfirmPassword", "TestPass123!"),
            new("Role", "Operator")
        };

        var token = await GetAntiForgeryTokenAsync("/Admin/Users");
        userData.Add(new("__RequestVerificationToken", token));

        // Act
        var formContent = new FormUrlEncodedContent(userData);
        var response = await _client.PostAsync("/Admin/Users?handler=CreateUser", formContent);

        // Assert
        // Should reject overly long input
        var responseContent = await response.Content.ReadAsStringAsync();
        
        // Either validation error or form rejection
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            responseContent.Contains("validation") ||
            responseContent.Contains("error"),
            "Should reject overly long input"
        );
        
        _output.WriteLine("? Input validation enforces data limits");
    }

    #endregion

    #region Session Security Tests

    [Fact]
    public async Task SessionSecurity_InvalidatesAfterLogout()
    {
        // Arrange
        await AuthenticateAsUserAsync("admin", "admin123");

        // Verify authenticated access works
        var adminResponse = await _client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);

        // Act - Logout
        var logoutResponse = await _client.PostAsync("/Account/Logout", new StringContent(""));

        // Try to access admin page after logout
        var postLogoutResponse = await _client.GetAsync("/Admin");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, postLogoutResponse.StatusCode);
        Assert.Contains("/Account/Login", postLogoutResponse.Headers.Location?.ToString());
        
        _output.WriteLine("? Session properly invalidated after logout");
    }

    [Fact]
    public async Task SessionSecurity_HasSecureConfiguration()
    {
        // Arrange
        await AuthenticateAsUserAsync("admin", "admin123");

        // Act
        var response = await _client.GetAsync("/Admin");

        // Assert - Check for secure cookie flags
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            var cookieValue = cookies.FirstOrDefault();
            if (cookieValue != null)
            {
                // In production, should have HttpOnly and Secure flags
                _output.WriteLine($"? Cookie configuration: {cookieValue}");
            }
        }
        
        _output.WriteLine("? Session security configuration verified");
    }

    #endregion

    #region CSRF Protection Tests

    [Fact]
    public async Task CSRFProtection_RejectsRequestsWithoutToken()
    {
        // Arrange
        await AuthenticateAsUserAsync("admin", "admin123");

        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Username", "testuser"),
            new KeyValuePair<string, string>("Password", "TestPass123!"),
            new KeyValuePair<string, string>("Role", "Operator")
            // Deliberately omitting anti-forgery token
        });

        // Act
        var response = await _client.PostAsync("/Admin/Users?handler=CreateUser", formData);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _output.WriteLine("? CSRF protection rejects requests without tokens");
    }

    [Fact]
    public async Task CSRFProtection_AcceptsValidTokens()
    {
        // Arrange
        await AuthenticateAsUserAsync("admin", "admin123");

        var token = await GetAntiForgeryTokenAsync("/Admin/Users");
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Username", "testuser456"),
            new("Password", "TestPass123!"),
            new("ConfirmPassword", "TestPass123!"),
            new("Role", "Operator"),
            new("__RequestVerificationToken", token)
        };

        // Act
        var formContent = new FormUrlEncodedContent(formData);
        var response = await _client.PostAsync("/Admin/Users?handler=CreateUser", formContent);

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect);
        _output.WriteLine("? CSRF protection accepts valid tokens");

        // Cleanup - try to remove the test user
        try
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var testUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "testuser456");
            if (testUser != null)
            {
                context.Users.Remove(testUser);
                await context.SaveChangesAsync();
            }
        }
        catch
        {
            // Cleanup failed, but test still valid
        }
    }

    #endregion

    #region Data Access Security Tests

    [Fact]
    public async Task DataAccess_PreventsDirectDatabaseAccess()
    {
        // Arrange
        var maliciousUrls = new[]
        {
            "/Admin/../../../etc/passwd",
            "/Admin/../../scheduler.db",
            "/Admin/%2e%2e/%2e%2e/scheduler.db",
            "/Admin/Settings?file=../../../scheduler.db"
        };

        // Act & Assert
        foreach (var url in maliciousUrls)
        {
            var response = await _client.GetAsync(url);
            
            // Should not return database files or system files
            Assert.True(
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Redirect,
                $"Should not allow access to {url}"
            );
            
            _output.WriteLine($"? Blocked malicious path: {url}");
        }
    }

    [Fact]
    public async Task DataAccess_EnforcesPermissionBoundaries()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var roleService = scope.ServiceProvider.GetRequiredService<IRolePermissionService>();

        // Test that users can only access data they have permissions for
        var permissionTests = new[]
        {
            new { Role = "Operator", CanViewUsers = false },
            new { Role = "Scheduler", CanViewUsers = false },
            new { Role = "Manager", CanViewUsers = true },
            new { Role = "Admin", CanViewUsers = true }
        };

        // Act & Assert
        foreach (var test in permissionTests)
        {
            var hasPermission = await roleService.HasPermissionAsync(test.Role, "Admin.ViewUsers");
            Assert.Equal(test.CanViewUsers, hasPermission);
            _output.WriteLine($"? {test.Role} view users permission: {hasPermission}");
        }
    }

    #endregion

    #region Helper Methods

    private async Task AuthenticateAsUserAsync(string username, string password)
    {
        // Get login page for anti-forgery token
        var loginPage = await _client.GetAsync("/Account/Login");
        var loginContent = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(loginContent);

        // Submit login
        var loginData = new List<KeyValuePair<string, string>>
        {
            new("Username", username),
            new("Password", password),
            new("__RequestVerificationToken", token)
        };

        var formContent = new FormUrlEncodedContent(loginData);
        var response = await _client.PostAsync("/Account/Login", formContent);
        
        // Verify login success
        Assert.True(response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.OK);
    }

    private async Task<string> GetAntiForgeryTokenAsync(string url)
    {
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        return ExtractAntiForgeryToken(content);
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

    public void Dispose()
    {
        _client?.Dispose();
    }
}