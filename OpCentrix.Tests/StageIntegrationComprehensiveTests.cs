using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using AngleSharp;
using AngleSharp.Html.Dom;
using OpCentrix.Data;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Models;

namespace OpCentrix.Tests
{
    /// <summary>
    /// Comprehensive Stage Integration Tests
    /// Tests all stage functionality across Parts management, Production Stages, and API endpoints
    /// </summary>
    public class StageIntegrationTests : IClassFixture<OpCentrixWebApplicationFactory>
    {
        private readonly OpCentrixWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public StageIntegrationTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _output = output;
        }

        #region API Endpoint Tests

        [Fact]
        public async Task ProductionStagesApi_ShouldReturnAvailableStages()
        {
            _output.WriteLine("?? Testing: Production Stages API endpoint accessibility");

            // Arrange: Login first
            await LoginAsAdminAsync();

            // Act: Call the API endpoint
            var response = await _client.GetAsync("/api/production-stages/available");

            // Assert: Should return success with stage data
            Assert.True(response.IsSuccessStatusCode, 
                $"API endpoint failed with status: {response.StatusCode}. Content: {await response.Content.ReadAsStringAsync()}");

            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);

            // Parse and validate JSON structure
            var stages = JsonSerializer.Deserialize<JsonElement[]>(content);
            Assert.NotNull(stages);
            Assert.NotEmpty(stages);

            _output.WriteLine($"? API returned {stages.Length} stages");

            // Validate stage structure
            var firstStage = stages[0];
            Assert.True(firstStage.TryGetProperty("id", out _), "Stage should have 'id' property");
            Assert.True(firstStage.TryGetProperty("name", out _), "Stage should have 'name' property");
            Assert.True(firstStage.TryGetProperty("defaultHourlyRate", out _), "Stage should have 'defaultHourlyRate' property");

            _output.WriteLine("? API endpoint test passed");
        }

        [Fact]
        public async Task ProductionStagesApi_ShouldHandleUnauthorizedAccess()
        {
            _output.WriteLine("?? Testing: API authorization requirements");

            // Act: Call API without login
            var response = await _client.GetAsync("/api/production-stages/available");

            // Assert: Should redirect to login or return unauthorized
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.Redirect || 
                       response.StatusCode == System.Net.HttpStatusCode.Unauthorized,
                       $"Expected redirect or unauthorized, got: {response.StatusCode}");

            _output.WriteLine("? API authorization test passed");
        }

        #endregion

        #region Production Stages Page Tests

        [Fact]
        public async Task ProductionStagesPage_ShouldLoadWithoutErrors()
        {
            _output.WriteLine("?? Testing: Production Stages page loading");

            // Arrange
            await LoginAsAdminAsync();

            // Act
            var response = await _client.GetAsync("/Admin/ProductionStages");

            // Assert
            Assert.True(response.IsSuccessStatusCode, 
                $"Production Stages page failed to load: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            
            // Check for critical elements
            Assert.Contains("Production Stage Configuration", content);
            Assert.Contains("Add New Stage", content);
            Assert.DoesNotContain("error", content.ToLower());
            Assert.DoesNotContain("exception", content.ToLower());

            _output.WriteLine("? Production Stages page loads successfully");
        }

        [Fact]
        public async Task ProductionStagesPage_JavaScriptShouldNotHaveErrors()
        {
            _output.WriteLine("?? Testing: Production Stages page JavaScript execution");

            // Arrange
            await LoginAsAdminAsync();

            // Act
            var response = await _client.GetAsync("/Admin/ProductionStages");
            var content = await response.Content.ReadAsStringAsync();

            // Assert: Check for JavaScript error patterns
            Assert.DoesNotContain("Uncaught", content);
            Assert.DoesNotContain("TypeError", content);
            Assert.DoesNotContain("ReferenceError", content);
            Assert.DoesNotContain("SyntaxError", content);

            // Check for required JavaScript functions
            Assert.Contains("loadAvailableMachines", content);
            Assert.Contains("addCustomField", content);
            Assert.Contains("editStage", content);

            _output.WriteLine("? Production Stages JavaScript validation passed");
        }

        #endregion

        #region Parts Page Stage Integration Tests

        [Fact]
        public async Task PartsPage_ShouldLoadAddPartModal()
        {
            _output.WriteLine("?? Testing: Parts page Add Part modal loading");

            // Arrange
            await LoginAsAdminAsync();

            // Act: Get the parts page
            var response = await _client.GetAsync("/Admin/Parts");
            Assert.True(response.IsSuccessStatusCode);

            // Act: Get the add part form
            var addResponse = await _client.GetAsync("/Admin/Parts?handler=Add");
            
            // Assert
            Assert.True(addResponse.IsSuccessStatusCode, 
                $"Add part form failed to load: {addResponse.StatusCode}");

            var content = await addResponse.Content.ReadAsStringAsync();
            
            // Check for stage-related elements
            Assert.Contains("Manufacturing Stages", content);
            Assert.Contains("stage-requirements-container", content);
            Assert.Contains("Add Stage", content);

            _output.WriteLine("? Add Part modal loads with stage components");
        }

        [Fact]
        public async Task PartsModal_StageTabShouldHaveRequiredElements()
        {
            _output.WriteLine("?? Testing: Parts modal stage tab elements");

            // Arrange
            await LoginAsAdminAsync();

            // Act
            var response = await _client.GetAsync("/Admin/Parts?handler=Add");
            var content = await response.Content.ReadAsStringAsync();

            // Parse HTML
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(content));

            // Assert: Check for critical stage elements
            var stageTab = document.QuerySelector("#stages-tab");
            Assert.NotNull(stageTab);

            var stageContainer = document.QuerySelector("#stage-requirements-container");
            Assert.NotNull(stageContainer);

            var addStageBtn = document.QuerySelector("#add-stage-btn");
            Assert.NotNull(addStageBtn);

            var hiddenFields = document.QuerySelectorAll("input[type='hidden'][name*='Stage']");
            Assert.True(hiddenFields.Length >= 5, $"Expected at least 5 stage hidden fields, found {hiddenFields.Length}");

            _output.WriteLine("? Stage tab has all required elements");
        }

        [Fact]
        public async Task PartsModal_ShouldIncludeStageManagerScript()
        {
            _output.WriteLine("?? Testing: Parts modal includes stage manager JavaScript");

            // Arrange
            await LoginAsAdminAsync();

            // Act
            var response = await _client.GetAsync("/Admin/Parts?handler=Add");
            var content = await response.Content.ReadAsStringAsync();

            // Assert: Check for script inclusions
            Assert.Contains("parts-stage-manager.js", content);
            Assert.Contains("parts-form-manager.js", content);
            Assert.Contains("opcentrix-global-functions.js", content);

            // Check for stage manager initialization
            Assert.Contains("initializeStageManagerForModal", content);
            Assert.Contains("shown.bs.modal", content);

            _output.WriteLine("? Stage manager scripts are included");
        }

        #endregion

        #region End-to-End Stage Workflow Tests

        [Fact]
        public async Task CreatePartWithStages_ShouldPersistStageData()
        {
            _output.WriteLine("?? Testing: Complete part creation with stages workflow");

            // Arrange
            await LoginAsAdminAsync();
            await EnsureProductionStagesExist();

            // Act: Create a part with stages
            var partData = new Dictionary<string, string>
            {
                {"Part.PartNumber", "TEST-STAGE-INTEGRATION-001"},
                {"Part.Name", "Stage Integration Test Part"},
                {"Part.Description", "Testing stage integration functionality"},
                {"Part.Material", "Ti-6Al-4V Grade 5"},
                {"Part.EstimatedHours", "8.0"},
                {"Part.ComponentTypeId", "1"},
                {"Part.ComplianceCategoryId", "1"},
                {"Part.IsActive", "true"},
                
                // Stage data
                {"SelectedStageIds", "1,2"},
                {"StageExecutionOrders", "1,2"},
                {"StageEstimatedHours", "4.0,2.0"},
                {"StageHourlyRates", "85.00,95.00"},
                {"StageMaterialCosts", "10.00,5.00"}
            };

            var createResponse = await PostFormDataAsync("/Admin/Parts?handler=Create", partData);
            
            // Assert: Part creation should succeed
            Assert.True(createResponse.IsSuccessStatusCode, 
                $"Part creation failed: {createResponse.StatusCode}. Content: {await createResponse.Content.ReadAsStringAsync()}");

            // Verify part was created in database
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            
            var createdPart = await context.Parts
                .Include(p => p.PartStageRequirements)
                .FirstOrDefaultAsync(p => p.PartNumber == "TEST-STAGE-INTEGRATION-001");

            Assert.NotNull(createdPart);
            Assert.Equal("Stage Integration Test Part", createdPart.Name);
            
            // Verify stages were created
            Assert.NotNull(createdPart.PartStageRequirements);
            Assert.Equal(2, createdPart.PartStageRequirements.Count);

            var stages = createdPart.PartStageRequirements.OrderBy(s => s.ExecutionOrder).ToList();
            Assert.Equal(1, stages[0].ExecutionOrder);
            Assert.Equal(2, stages[1].ExecutionOrder);
            Assert.Equal(4.0, stages[0].EstimatedHours);
            Assert.Equal(2.0, stages[1].EstimatedHours);

            _output.WriteLine("? Part created successfully with stage data persisted");
        }

        [Fact]
        public async Task EditPartWithStages_ShouldLoadExistingStages()
        {
            _output.WriteLine("?? Testing: Edit part with existing stages");

            // Arrange
            await LoginAsAdminAsync();
            var partId = await CreateTestPartWithStagesAsync();

            // Act: Load edit form
            var response = await _client.GetAsync($"/Admin/Parts?handler=Edit&id={partId}");
            
            // Assert
            Assert.True(response.IsSuccessStatusCode, 
                $"Edit form failed to load: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            
            // Check that form includes stage data loading
            Assert.Contains("Manufacturing Stages", content);
            Assert.Contains("stage-requirements-container", content);

            _output.WriteLine("? Edit form loads for part with stages");
        }

        #endregion

        #region Database Integration Tests

        [Fact]
        public async Task Database_ShouldHaveProductionStagesTable()
        {
            _output.WriteLine("?? Testing: Database production stages table");

            // Arrange & Act
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            var stages = await context.ProductionStages.ToListAsync();

            // Assert
            Assert.NotNull(stages);
            _output.WriteLine($"Found {stages.Count} production stages in database");

            if (stages.Count == 0)
            {
                _output.WriteLine("?? No production stages found - this may indicate seeding issues");
            }
            else
            {
                var firstStage = stages.First();
                Assert.NotNull(firstStage.Name);
                Assert.True(firstStage.DefaultHourlyRate > 0);
                _output.WriteLine($"? Sample stage: {firstStage.Name} - ${firstStage.DefaultHourlyRate}/hr");
            }
        }

        [Fact]
        public async Task Database_ShouldSupportPartStageRequirements()
        {
            _output.WriteLine("?? Testing: Database part stage requirements table");

            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            // Verify table structure by creating a test record
            var testPart = new Part
            {
                PartNumber = "DB-TEST-001",
                Name = "Database Test Part",
                Description = "Testing database structure",
                Material = "Test Material",
                EstimatedHours = 1.0,
                MaterialCostPerKg = 100m,
                StandardLaborCostPerHour = 85m,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Test",
                ComponentTypeId = 1,
                ComplianceCategoryId = 1
            };

            context.Parts.Add(testPart);
            await context.SaveChangesAsync();

            // Create stage requirement
            var stageRequirement = new PartStageRequirement
            {
                PartId = testPart.Id,
                ProductionStageId = 1, // Assuming at least one stage exists
                ExecutionOrder = 1,
                EstimatedHours = 2.0,
                SetupTimeMinutes = 30,
                HourlyRateOverride = 90m,
                MaterialCost = 15m,
                IsRequired = true,
                IsActive = true,
                CreatedBy = "Test",
                CreatedDate = DateTime.UtcNow
            };

            context.PartStageRequirements.Add(stageRequirement);
            await context.SaveChangesAsync();

            // Assert: Verify relationship works
            var loadedPart = await context.Parts
                .Include(p => p.PartStageRequirements)
                .FirstOrDefaultAsync(p => p.Id == testPart.Id);

            Assert.NotNull(loadedPart);
            Assert.Single(loadedPart.PartStageRequirements);
            Assert.Equal(2.0, loadedPart.PartStageRequirements.First().EstimatedHours);

            // Cleanup
            context.PartStageRequirements.Remove(stageRequirement);
            context.Parts.Remove(testPart);
            await context.SaveChangesAsync();

            _output.WriteLine("? Database part stage requirements working correctly");
        }

        #endregion

        #region Helper Methods

        private async Task LoginAsAdminAsync()
        {
            var loginPage = await _client.GetAsync("/Account/Login");
            var loginContent = await loginPage.Content.ReadAsStringAsync();

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(loginContent));

            var form = document.QuerySelector("form") as IHtmlFormElement;
            var tokenInput = document.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement;

            var formData = new Dictionary<string, string>
            {
                {"Username", "admin"},
                {"Password", "admin123"},
                {"__RequestVerificationToken", tokenInput?.Value ?? ""}
            };

            var response = await PostFormDataAsync("/Account/Login", formData);
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Redirect);
        }

        private async Task<HttpResponseMessage> PostFormDataAsync(string url, Dictionary<string, string> formData)
        {
            var content = new FormUrlEncodedContent(formData);
            return await _client.PostAsync(url, content);
        }

        private async Task EnsureProductionStagesExist()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            var existingStages = await context.ProductionStages.CountAsync();
            if (existingStages == 0)
            {
                var defaultStages = new[]
                {
                    new ProductionStage
                    {
                        Name = "SLS Printing",
                        Description = "Selective Laser Sintering",
                        DefaultHourlyRate = 85m,
                        DefaultSetupMinutes = 45,
                        DefaultDurationHours = 8.0,
                        DefaultMaterialCost = 25m,
                        DisplayOrder = 1,
                        IsActive = true,
                        CreatedBy = "Test",
                        CreatedDate = DateTime.UtcNow
                    },
                    new ProductionStage
                    {
                        Name = "CNC Machining",
                        Description = "Computer Numerical Control Machining",
                        DefaultHourlyRate = 105m,
                        DefaultSetupMinutes = 60,
                        DefaultDurationHours = 4.0,
                        DefaultMaterialCost = 10m,
                        DisplayOrder = 2,
                        IsActive = true,
                        CreatedBy = "Test",
                        CreatedDate = DateTime.UtcNow
                    }
                };

                context.ProductionStages.AddRange(defaultStages);
                await context.SaveChangesAsync();
            }
        }

        private async Task<int> CreateTestPartWithStagesAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            var part = new Part
            {
                PartNumber = "TEST-EDIT-STAGES-001",
                Name = "Edit Stages Test Part",
                Description = "Testing stage editing functionality",
                Material = "Ti-6Al-4V Grade 5",
                EstimatedHours = 6.0,
                MaterialCostPerKg = 450m,
                StandardLaborCostPerHour = 85m,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Test",
                ComponentTypeId = 1,
                ComplianceCategoryId = 1
            };

            context.Parts.Add(part);
            await context.SaveChangesAsync();

            // Add stage requirements
            var stages = await context.ProductionStages.Take(2).ToListAsync();
            if (stages.Count >= 2)
            {
                var stageRequirements = new[]
                {
                    new PartStageRequirement
                    {
                        PartId = part.Id,
                        ProductionStageId = stages[0].Id,
                        ExecutionOrder = 1,
                        EstimatedHours = 4.0,
                        SetupTimeMinutes = 30,
                        HourlyRateOverride = 85m,
                        MaterialCost = 10m,
                        IsRequired = true,
                        IsActive = true,
                        CreatedBy = "Test",
                        CreatedDate = DateTime.UtcNow
                    },
                    new PartStageRequirement
                    {
                        PartId = part.Id,
                        ProductionStageId = stages[1].Id,
                        ExecutionOrder = 2,
                        EstimatedHours = 2.0,
                        SetupTimeMinutes = 15,
                        HourlyRateOverride = 105m,
                        MaterialCost = 5m,
                        IsRequired = true,
                        IsActive = true,
                        CreatedBy = "Test",
                        CreatedDate = DateTime.UtcNow
                    }
                };

                context.PartStageRequirements.AddRange(stageRequirements);
                await context.SaveChangesAsync();
            }

            return part.Id;
        }

        #endregion
    }
}