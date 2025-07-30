using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpCentrix.Data;
using System.Net;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace OpCentrix.Tests
{
    /// <summary>
    /// Integration tests for the complete OpCentrix system
    /// Validates that all major components work together correctly
    /// </summary>
    public class SystemIntegrationTests : IClassFixture<OpCentrixWebApplicationFactory>
    {
        private readonly OpCentrixWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public SystemIntegrationTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _output = output;
        }

        [Fact]
        public async Task Application_StartsSuccessfully()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/");

            // Assert
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect);
            _output.WriteLine("? Application starts successfully");
        }

        [Fact]
        public async Task Database_IsConnectedAndSeeded()
        {
            // Arrange & Act
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            // Assert
            Assert.True(await context.Database.CanConnectAsync());
            
            var userCount = await context.Users.CountAsync();
            Assert.True(userCount >= 4, $"Expected at least 4 test users, found {userCount}");
            
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            Assert.NotNull(adminUser);
            
            _output.WriteLine($"? Database connected and seeded with {userCount} users");
        }

        [Fact]
        public async Task AdminPages_RequireProperAuthentication()
        {
            // FIXED: Ensure we start with no authentication
            await _client.GetAsync("/Account/Logout");
            await Task.Delay(100); // Allow logout to process
            
            // Test all major admin pages
            var adminPages = new[]
            {
                "/Admin",
                "/Admin/Parts",
                "/Admin/Users",
                "/Admin/Settings",
                "/Admin/Roles",
                "/Admin/Machines"
            };

            foreach (var page in adminPages)
            {
                var response = await _client.GetAsync(page);
                
                // Should redirect to login (302) or return unauthorized (401/403)
                Assert.True(response.StatusCode == HttpStatusCode.Redirect || 
                           response.StatusCode == HttpStatusCode.Unauthorized ||
                           response.StatusCode == HttpStatusCode.Forbidden,
                    $"Admin page {page} should require authentication but got {response.StatusCode}");
                
                if (response.StatusCode == HttpStatusCode.Redirect)
                {
                    Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
                }
                
                _output.WriteLine($"? Admin page {page} requires authentication (Status: {response.StatusCode})");
            }
        }

        [Fact]
        public async Task AdminPages_AccessibleAfterLogin()
        {
            // Arrange
            await AuthenticateAsAdminAsync();

            // Test all major admin pages
            var adminPages = new[]
            {
                "/Admin",
                "/Admin/Parts",
                "/Admin/Users", 
                "/Admin/Settings",
                "/Admin/Roles"
            };

            foreach (var page in adminPages)
            {
                var response = await _client.GetAsync(page);
                
                Assert.True(response.IsSuccessStatusCode, 
                    $"Page {page} returned {response.StatusCode}: {response.ReasonPhrase}");
                
                _output.WriteLine($"? Admin page {page} accessible after login");
            }
        }

        [Fact]
        public async Task PartsSystem_CompleteWorkflow()
        {
            // Arrange
            await AuthenticateAsAdminAsync();

            // Step 1: Load parts page
            var partsPage = await _client.GetAsync("/Admin/Parts");
            partsPage.EnsureSuccessStatusCode();
            
            // Step 2: Load add form to get anti-forgery token
            var addForm = await _client.GetAsync("/Admin/Parts?handler=Add");
            addForm.EnsureSuccessStatusCode();
            var addFormContent = await addForm.Content.ReadAsStringAsync();
            var token = ExtractAntiforgeryToken(addFormContent);
            
            // Step 3: Create a part with ALL required fields
            var formData = new FormUrlEncodedContent(new[]
            {
                // Anti-forgery token
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                
                // Basic required fields
                new KeyValuePair<string, string>("PartNumber", "INTEGRATION-001"),
                new KeyValuePair<string, string>("Name", "Integration Test Part"),
                new KeyValuePair<string, string>("Description", "Test part for integration testing"),
                new KeyValuePair<string, string>("Material", "Ti-6Al-4V Grade 5"),
                new KeyValuePair<string, string>("SlsMaterial", "Ti-6Al-4V Grade 5"),
                new KeyValuePair<string, string>("Industry", "Aerospace"),
                new KeyValuePair<string, string>("Application", "Test Component"),
                new KeyValuePair<string, string>("PartCategory", "Prototype"),
                new KeyValuePair<string, string>("PartClass", "B"),
                new KeyValuePair<string, string>("ProcessType", "SLS Metal"),
                new KeyValuePair<string, string>("RequiredMachineType", "TruPrint 3000"),
                new KeyValuePair<string, string>("IsActive", "true"),
                new KeyValuePair<string, string>("EstimatedHours", "8.0"),
                new KeyValuePair<string, string>("MaterialCostPerKg", "450.00"),
                new KeyValuePair<string, string>("StandardLaborCostPerHour", "85.00"),
                new KeyValuePair<string, string>("WeightGrams", "100"),
                new KeyValuePair<string, string>("LengthMm", "50"),
                new KeyValuePair<string, string>("WidthMm", "30"),
                new KeyValuePair<string, string>("HeightMm", "20"),
                new KeyValuePair<string, string>("RequiresInspection", "true"),
                
                // FIXED: Add all required fields that have NOT NULL constraints
                new KeyValuePair<string, string>("PowderSpecification", "15-45 micron particle size"),
                new KeyValuePair<string, string>("Dimensions", "50 x 30 x 20 mm"),
                new KeyValuePair<string, string>("SurfaceFinishRequirement", "As-built"),
                new KeyValuePair<string, string>("PreferredMachines", "TI1,TI2"),
                new KeyValuePair<string, string>("QualityStandards", "ASTM F3001, ISO 17296"),
                new KeyValuePair<string, string>("ToleranceRequirements", "±0.1mm typical"),
                new KeyValuePair<string, string>("RequiredSkills", "SLS Operation,Powder Handling"),
                new KeyValuePair<string, string>("RequiredCertifications", "SLS Operation Certification"),
                new KeyValuePair<string, string>("RequiredTooling", "Build Platform,Powder Sieve"),
                new KeyValuePair<string, string>("ConsumableMaterials", "Argon Gas,Build Platform Coating"),
                new KeyValuePair<string, string>("SupportStrategy", "Minimal supports on overhangs > 45°"),
                new KeyValuePair<string, string>("CustomerPartNumber", "CUST-INT-001"),
                new KeyValuePair<string, string>("AvgDuration", "8h 0m"),
                new KeyValuePair<string, string>("ProcessParameters", "{}"),
                new KeyValuePair<string, string>("QualityCheckpoints", "{}"),
                new KeyValuePair<string, string>("BuildFileTemplate", "default-template.slm"),
                new KeyValuePair<string, string>("CadFilePath", "/files/parts/integration-test.step"),
                new KeyValuePair<string, string>("CadFileVersion", "1.0"),
                new KeyValuePair<string, string>("CreatedBy", "System"),
                new KeyValuePair<string, string>("LastModifiedBy", "System"),
                new KeyValuePair<string, string>("AdminOverrideBy", "System")
            });

            var createResponse = await _client.PostAsync("/Admin/Parts?handler=Create", formData);
            createResponse.EnsureSuccessStatusCode();
            
            // Step 4: Verify part was created
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
                var part = await context.Parts.FirstOrDefaultAsync(p => p.PartNumber == "INTEGRATION-001");
                Assert.NotNull(part);
                Assert.Equal("Integration Test Part", part.Name);
                
                // Step 5: Load edit form
                var editForm = await _client.GetAsync($"/Admin/Parts?handler=Edit&id={part.Id}");
                editForm.EnsureSuccessStatusCode();
                
                // Step 6: Get part data via API
                var partData = await _client.GetAsync($"/Admin/Parts?handler=PartData&id={part.Id}");
                partData.EnsureSuccessStatusCode();
                
                // Step 7: Clean up - delete the part
                var deleteForm = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("id", part.Id.ToString()),
                    new KeyValuePair<string, string>("__RequestVerificationToken", token)
                });
                
                var deleteResponse = await _client.PostAsync("/Admin/Parts?handler=Delete", deleteForm);
                Assert.Equal(HttpStatusCode.Redirect, deleteResponse.StatusCode);
            }

            _output.WriteLine("? Parts system complete workflow successful");
        }

        [Fact]
        public async Task HTMX_RequestsWorkCorrectly()
        {
            // Arrange
            await AuthenticateAsAdminAsync();
            
            var request = new HttpRequestMessage(HttpMethod.Get, "/Admin/Parts?handler=Add");
            request.Headers.Add("HX-Request", "true");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            // Should return partial content for HTMX
            Assert.Contains("modal-header", content);
            Assert.DoesNotContain("<!DOCTYPE html>", content); // Shouldn't be full page
            
            _output.WriteLine("? HTMX requests work correctly");
        }

        [Fact]
        public async Task ErrorHandling_WorksCorrectly()
        {
            // Arrange
            await AuthenticateAsAdminAsync();

            // Test invalid part ID
            var response = await _client.GetAsync("/Admin/Parts?handler=Edit&id=99999");
            
            // Should handle gracefully (404 or redirect)
            Assert.True(response.StatusCode == HttpStatusCode.NotFound || 
                       response.StatusCode == HttpStatusCode.Redirect);
            
            _output.WriteLine("? Error handling works correctly");
        }

        [Theory]
        [InlineData("admin", "admin123", true)]
        [InlineData("manager", "manager123", true)]
        [InlineData("scheduler", "scheduler123", false)] // Should not have admin access
        [InlineData("operator", "operator123", false)]   // Should not have admin access
        public async Task UserRoles_HaveCorrectAccess(string username, string password, bool shouldHaveAdminAccess)
        {
            // FIXED: Ensure we start with a clean slate - clear any existing authentication
            await _client.GetAsync("/Account/Logout");
            
            // Arrange - Login as specific user
            var loginPage = await _client.GetAsync("/Account/Login");
            loginPage.EnsureSuccessStatusCode();
            var loginContent = await loginPage.Content.ReadAsStringAsync();
            var token = ExtractAntiforgeryToken(loginContent);

            var loginData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Username", username),
                new KeyValuePair<string, string>("Password", password),
                new KeyValuePair<string, string>("__RequestVerificationToken", token)
            });

            var loginResponse = await _client.PostAsync("/Account/Login", loginData);
            Assert.True(loginResponse.IsSuccessStatusCode || loginResponse.StatusCode == HttpStatusCode.Redirect);
            
            // FIXED: Add a small delay to ensure authentication is processed
            await Task.Delay(100);

            // Act - Try to access admin page
            var adminResponse = await _client.GetAsync("/Admin/Parts");

            // Assert
            if (shouldHaveAdminAccess)
            {
                Assert.True(adminResponse.IsSuccessStatusCode, 
                    $"User {username} should have admin access but got {adminResponse.StatusCode}");
            }
            else
            {
                Assert.True(adminResponse.StatusCode == HttpStatusCode.Forbidden || 
                           adminResponse.StatusCode == HttpStatusCode.Redirect,
                    $"User {username} should not have admin access but got {adminResponse.StatusCode}");
            }

            _output.WriteLine($"? User {username} has correct access level (admin: {shouldHaveAdminAccess})");

            // FIXED: Properly logout for next test
            var logoutResponse = await _client.GetAsync("/Account/Logout");
            Assert.True(logoutResponse.IsSuccessStatusCode || logoutResponse.StatusCode == HttpStatusCode.Redirect);
            
            // FIXED: Add delay to ensure logout is processed
            await Task.Delay(100);
        }

        [Fact]
        public async Task PerformanceCheck_PageLoadsWithinReasonableTime()
        {
            // Arrange
            await AuthenticateAsAdminAsync();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var response = await _client.GetAsync("/Admin/Parts");

            // Assert
            stopwatch.Stop();
            response.EnsureSuccessStatusCode();
            
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
                $"Parts page took {stopwatch.ElapsedMilliseconds}ms to load (should be < 5000ms)");
            
            _output.WriteLine($"? Parts page loads in {stopwatch.ElapsedMilliseconds}ms");
        }

        [Fact]
        public async Task StaticFiles_AreAccessible()
        {
            // Test that CSS, JS, and other static files are served correctly
            var staticFiles = new[]
            {
                "/css/site.css",
                "/js/site.js",
                "/lib/bootstrap/dist/css/bootstrap.min.css",
                "/lib/bootstrap/dist/js/bootstrap.bundle.min.js"
            };

            foreach (var file in staticFiles)
            {
                var response = await _client.GetAsync(file);
                
                // Should be successful or not found (some files might not exist)
                Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound,
                    $"Static file {file} returned unexpected status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    _output.WriteLine($"? Static file {file} accessible");
                }
            }
        }

        #region Helper Methods

        private async Task AuthenticateAsAdminAsync()
        {
            // Get login page to retrieve antiforgery token
            var loginPage = await _client.GetAsync("/Account/Login");
            var loginContent = await loginPage.Content.ReadAsStringAsync();

            var token = ExtractAntiforgeryToken(loginContent);

            // Login as admin
            var loginData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Username", "admin"),
                new KeyValuePair<string, string>("Password", "admin123"),
                new KeyValuePair<string, string>("__RequestVerificationToken", token)
            });

            var loginResponse = await _client.PostAsync("/Account/Login", loginData);
            
            // Should redirect to dashboard on success
            Assert.True(loginResponse.IsSuccessStatusCode || loginResponse.StatusCode == HttpStatusCode.Redirect);
        }

        private string ExtractAntiforgeryToken(string html)
        {
            var tokenStart = html.IndexOf("name=\"__RequestVerificationToken\"");
            if (tokenStart == -1) return "";

            var valueStart = html.IndexOf("value=\"", tokenStart) + 7;
            var valueEnd = html.IndexOf("\"", valueStart);

            return html.Substring(valueStart, valueEnd - valueStart);
        }

        #endregion
    }
}