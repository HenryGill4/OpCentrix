using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpCentrix.Data;
using OpCentrix.Models;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace OpCentrix.Tests
{
    /// <summary>
    /// Comprehensive Parts Page Testing Suite
    /// Tests all CRUD operations, HTMX handlers, validation, and modal functionality
    /// </summary>
    public class PartsPageTests : IClassFixture<OpCentrixWebApplicationFactory>
    {
        private readonly OpCentrixWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public PartsPageTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _output = output;
        }

        [Fact]
        public async Task PartsPage_LoadsSuccessfully()
        {
            // Arrange
            await AuthenticateAsAdminAsync();

            // Act
            var response = await _client.GetAsync("/Admin/Parts");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("Parts Management", content);
            Assert.Contains("Add New Part", content);
            Assert.Contains("Parts (", content); // Should show parts count
            
            _output.WriteLine("? Parts page loads successfully");
        }

        [Fact]
        public async Task PartsPage_RequiresAuthentication()
        {
            // Act
            var response = await _client.GetAsync("/Admin/Parts");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
            
            _output.WriteLine("? Parts page requires authentication");
        }

        [Fact]
        public async Task AddPartHandler_LoadsFormSuccessfully()
        {
            // Arrange
            await AuthenticateAsAdminAsync();

            // Act
            var response = await _client.GetAsync("/Admin/Parts?handler=Add");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("Add New Part", content);
            Assert.Contains("PartNumber", content);
            Assert.Contains("Name", content);
            Assert.Contains("Description", content);
            Assert.Contains("Material", content);
            
            _output.WriteLine("? Add part handler loads form successfully");
        }

        [Fact]
        public async Task EditPartHandler_LoadsFormWithData()
        {
            // Arrange
            await AuthenticateAsAdminAsync();
            var partId = await CreateTestPartAsync();

            // Act
            var response = await _client.GetAsync($"/Admin/Parts?handler=Edit&id={partId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("Edit Part", content);
            Assert.Contains("TEST-PART-001", content);
            Assert.Contains("Test Part Name", content);
            
            _output.WriteLine($"? Edit part handler loads form with data for part ID: {partId}");
        }

        [Fact]
        public async Task CreatePart_WithValidData_SucceedsAndRedirects()
        {
            // Arrange
            await AuthenticateAsAdminAsync();
            var formData = CreateValidPartFormData("CREATE-TEST-001");

            // Act
            var response = await _client.PostAsync("/Admin/Parts?handler=Create", new FormUrlEncodedContent(formData));

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            // Should return JavaScript for success handling
            Assert.Contains("Part saved successfully", content);
            Assert.Contains("CREATE-TEST-001", content);
            
            // Verify part was created in database
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var createdPart = await context.Parts.FirstOrDefaultAsync(p => p.PartNumber == "CREATE-TEST-001");
            
            Assert.NotNull(createdPart);
            Assert.Equal("Test Create Part", createdPart.Name);
            
            _output.WriteLine("? Create part with valid data succeeds");
        }

        [Fact]
        public async Task CreatePart_WithInvalidData_ReturnsValidationErrors()
        {
            // Arrange
            await AuthenticateAsAdminAsync();
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("PartNumber", ""), // Missing required field
                new KeyValuePair<string, string>("Name", ""),        // Missing required field
            });

            // Act
            var response = await _client.PostAsync("/Admin/Parts?handler=Create", formData);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            // Should return form with validation errors
            Assert.Contains("is required", content);
            
            _output.WriteLine("? Create part with invalid data returns validation errors");
        }

        [Fact]
        public async Task CreatePart_WithDuplicatePartNumber_ReturnsError()
        {
            // Arrange
            await AuthenticateAsAdminAsync();
            await CreateTestPartAsync(); // Create first part
            
            var formData = CreateValidPartFormData("TEST-PART-001"); // Duplicate part number

            // Act
            var response = await _client.PostAsync("/Admin/Parts?handler=Create", new FormUrlEncodedContent(formData));

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("already exists", content);
            
            _output.WriteLine("? Create part with duplicate part number returns error");
        }

        [Fact]
        public async Task EditPart_WithValidData_UpdatesSuccessfully()
        {
            // Arrange
            await AuthenticateAsAdminAsync();
            var partId = await CreateTestPartAsync();
            
            var formData = CreateValidPartFormData("TEST-PART-001");
            formData.Add(new KeyValuePair<string, string>("Id", partId.ToString()));
            formData.Add(new KeyValuePair<string, string>("Name", "Updated Test Part Name"));

            // Act
            var response = await _client.PostAsync($"/Admin/Parts?handler=Edit&id={partId}", 
                new FormUrlEncodedContent(formData));

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("Part saved successfully", content);
            
            // Verify part was updated in database
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var updatedPart = await context.Parts.FindAsync(partId);
            
            Assert.NotNull(updatedPart);
            Assert.Equal("Updated Test Part Name", updatedPart.Name);
            
            _output.WriteLine($"? Edit part with valid data updates successfully for part ID: {partId}");
        }

        [Fact]
        public async Task DeletePart_WithValidId_DeletesSuccessfully()
        {
            // Arrange
            await AuthenticateAsAdminAsync();
            var partId = await CreateTestPartAsync();
            
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("id", partId.ToString())
            });

            // Act
            var response = await _client.PostAsync("/Admin/Parts?handler=Delete", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Admin/Parts", response.Headers.Location?.ToString());
            
            // Verify part was deleted from database
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var deletedPart = await context.Parts.FindAsync(partId);
            
            Assert.Null(deletedPart);
            
            _output.WriteLine($"? Delete part with valid ID deletes successfully for part ID: {partId}");
        }

        [Fact]
        public async Task PartDataHandler_ReturnsJsonData()
        {
            // Arrange
            await AuthenticateAsAdminAsync();
            var partId = await CreateTestPartAsync();

            // Act
            var response = await _client.GetAsync($"/Admin/Parts?handler=PartData&id={partId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("TEST-PART-001", content);
            Assert.Contains("Test Part Name", content);
            Assert.Contains("Ti-6Al-4V Grade 5", content);
            
            _output.WriteLine($"? Part data handler returns JSON data for part ID: {partId}");
        }

        [Fact]
        public async Task PartsPage_HandlesHtmxRequests()
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
            Assert.Contains("Add New Part", content);
            
            _output.WriteLine("? Parts page handles HTMX requests correctly");
        }

        [Fact]
        public async Task PartsPage_SupportsFiltering()
        {
            // Arrange
            await AuthenticateAsAdminAsync();
            await CreateTestPartAsync(); // Creates a Ti-6Al-4V part

            // Act - Filter by material
            var response = await _client.GetAsync("/Admin/Parts?MaterialFilter=Ti-6Al-4V Grade 5");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("TEST-PART-001", content);
            Assert.Contains("Ti-6Al-4V Grade 5", content);
            
            _output.WriteLine("? Parts page supports filtering by material");
        }

        [Fact]
        public async Task PartsPage_SupportsPagination()
        {
            // Arrange
            await AuthenticateAsAdminAsync();

            // Act
            var response = await _client.GetAsync("/Admin/Parts?PageNumber=1&PageSize=10");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("per page", content);
            Assert.Contains("Showing", content);
            
            _output.WriteLine("? Parts page supports pagination");
        }

        [Fact]
        public async Task PartsPage_SupportsSorting()
        {
            // Arrange
            await AuthenticateAsAdminAsync();

            // Act
            var response = await _client.GetAsync("/Admin/Parts?SortOrder=name");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("Part Number", content);
            Assert.Contains("Name", content);
            
            _output.WriteLine("? Parts page supports sorting");
        }

        [Fact]
        public async Task CreatePart_WithAdminOverride_SavesOverrideData()
        {
            // Arrange
            await AuthenticateAsAdminAsync();
            var formData = CreateValidPartFormData("OVERRIDE-TEST-001");
            formData.Add(new KeyValuePair<string, string>("AdminEstimatedHoursOverride", "15.5"));
            formData.Add(new KeyValuePair<string, string>("AdminOverrideReason", "Complex geometry requires additional time"));

            // Act
            var response = await _client.PostAsync("/Admin/Parts?handler=Create", 
                new FormUrlEncodedContent(formData));

            // Assert
            response.EnsureSuccessStatusCode();
            
            // Verify override data was saved
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var part = await context.Parts.FirstOrDefaultAsync(p => p.PartNumber == "OVERRIDE-TEST-001");
            
            Assert.NotNull(part);
            Assert.Equal(15.5, part.AdminEstimatedHoursOverride);
            Assert.Equal("Complex geometry requires additional time", part.AdminOverrideReason);
            Assert.True(part.HasAdminOverride);
            
            _output.WriteLine("? Create part with admin override saves override data");
        }

        [Theory]
        [InlineData("Ti-6Al-4V Grade 5")]
        [InlineData("Inconel 718")]
        [InlineData("316L Stainless Steel")]
        [InlineData("AlSi10Mg")]
        public async Task CreatePart_WithDifferentMaterials_SavesCorrectly(string material)
        {
            // Arrange
            await AuthenticateAsAdminAsync();
            var formData = CreateValidPartFormData($"MAT-TEST-{material.Replace(" ", "").Replace("-", "")}");
            
            // Update material
            for (int i = 0; i < formData.Count; i++)
            {
                if (formData[i].Key == "Material")
                {
                    formData[i] = new KeyValuePair<string, string>("Material", material);
                    break;
                }
            }
            formData.Add(new KeyValuePair<string, string>("SlsMaterial", material));

            // Act
            var response = await _client.PostAsync("/Admin/Parts?handler=Create", 
                new FormUrlEncodedContent(formData));

            // Assert
            response.EnsureSuccessStatusCode();
            
            _output.WriteLine($"? Create part with {material} material saves correctly");
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

        private async Task<int> CreateTestPartAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            var part = new Part
            {
                PartNumber = "TEST-PART-001",
                Name = "Test Part Name",
                Description = "Test part description",
                Material = "Ti-6Al-4V Grade 5",
                SlsMaterial = "Ti-6Al-4V Grade 5",
                Industry = "Aerospace",
                Application = "Test Component",
                PartCategory = "Prototype",
                PartClass = "B",
                ProcessType = "SLS Metal",
                RequiredMachineType = "TruPrint 3000",
                IsActive = true,
                EstimatedHours = 8.0,
                MaterialCostPerKg = 450.00m,
                StandardLaborCostPerHour = 85.00m,
                SetupCost = 150.00m,
                PostProcessingCost = 75.00m,
                QualityInspectionCost = 50.00m,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = "test",
                LastModifiedBy = "test",
                // Ensure all required fields have values
                CustomerPartNumber = "",
                Dimensions = "",
                PowderSpecification = "15-45 micron particle size",
                SurfaceFinishRequirement = "As-built",
                QualityStandards = "ASTM F3001",
                ToleranceRequirements = "±0.1mm",
                RequiredSkills = "SLS Operation",
                RequiredCertifications = "SLS Certification",
                RequiredTooling = "Build Platform",
                ConsumableMaterials = "Argon Gas",
                SupportStrategy = "Minimal supports",
                ProcessParameters = "{}",
                QualityCheckpoints = "{}",
                BuildFileTemplate = "",
                CadFilePath = "",
                CadFileVersion = "",
                AvgDuration = "8h 0m",
                AdminOverrideReason = "",
                AdminOverrideBy = ""
            };

            context.Parts.Add(part);
            await context.SaveChangesAsync();

            return part.Id;
        }

        private List<KeyValuePair<string, string>> CreateValidPartFormData(string partNumber)
        {
            return new List<KeyValuePair<string, string>>
            {
                new("PartNumber", partNumber),
                new("Name", "Test Create Part"),
                new("Description", "Test part description for validation"),
                new("Material", "Ti-6Al-4V Grade 5"),
                new("SlsMaterial", "Ti-6Al-4V Grade 5"),
                new("Industry", "Aerospace"),
                new("Application", "Test Component"),
                new("PartCategory", "Prototype"),
                new("PartClass", "B"),
                new("ProcessType", "SLS Metal"),
                new("RequiredMachineType", "TruPrint 3000"),
                new("IsActive", "true"),
                new("EstimatedHours", "8.0"),
                new("MaterialCostPerKg", "450.00"),
                new("StandardLaborCostPerHour", "85.00"),
                new("SetupCost", "150.00"),
                new("PostProcessingCost", "75.00"),
                new("QualityInspectionCost", "50.00"),
                new("WeightGrams", "100"),
                new("LengthMm", "50"),
                new("WidthMm", "30"),
                new("HeightMm", "20"),
                new("VolumeMm3", "30000"),
                new("RecommendedLaserPower", "200"),
                new("RecommendedScanSpeed", "1200"),
                new("RecommendedLayerThickness", "30"),
                new("PowderRequirementKg", "0.5"),
                new("SetupTimeMinutes", "45"),
                new("PreheatingTimeMinutes", "60"),
                new("CoolingTimeMinutes", "240"),
                new("RequiresInspection", "true"),
                new("RequiresCertification", "false"),
                new("RequiresFDA", "false"),
                new("RequiresAS9100", "true"),
                new("RequiresNADCAP", "false"),
                new("RequiresSupports", "true"),
                new("SupportRemovalTimeMinutes", "30")
            };
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