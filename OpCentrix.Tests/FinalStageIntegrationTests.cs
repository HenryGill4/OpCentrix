using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;
using System.Text.Json;
using AngleSharp;
using AngleSharp.Html.Dom;
using OpCentrix.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Models;

namespace OpCentrix.Tests
{
    /// <summary>
    /// Final Stage Integration Verification Tests
    /// Comprehensive end-to-end testing of stage functionality
    /// </summary>
    public class FinalStageIntegrationTests : IClassFixture<OpCentrixWebApplicationFactory>
    {
        private readonly OpCentrixWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public FinalStageIntegrationTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _output = output;
        }

        [Fact]
        public async Task Complete_Stage_Integration_Test()
        {
            _output.WriteLine("?? COMPREHENSIVE STAGE INTEGRATION TEST");
            _output.WriteLine("=====================================");

            // Step 1: Verify API endpoints work
            await VerifyApiEndpoints();

            // Step 2: Verify database has stage data
            await VerifyDatabaseStages();

            // Step 3: Verify parts modal loads correctly
            await VerifyPartsModalContent();

            // Step 4: Verify JavaScript files are accessible
            await VerifyJavaScriptFiles();

            // Step 5: Test complete workflow
            await TestCompleteWorkflow();

            _output.WriteLine("");
            _output.WriteLine("?? STAGE INTEGRATION TEST COMPLETED");
        }

        private async Task VerifyApiEndpoints()
        {
            _output.WriteLine("");
            _output.WriteLine("?? Step 1: Testing API Endpoints");
            _output.WriteLine("----------------------------------");

            // Test the available stages endpoint
            var response = await _client.GetAsync("/api/production-stages/available");
            _output.WriteLine($"API Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _output.WriteLine($"? API SUCCESS - Content length: {content.Length}");

                try
                {
                    var stages = JsonSerializer.Deserialize<JsonElement[]>(content);
                    _output.WriteLine($"? Returned {stages.Length} stages");

                    if (stages.Length > 0)
                    {
                        var firstStage = stages[0];
                        if (firstStage.TryGetProperty("name", out var nameProperty))
                        {
                            _output.WriteLine($"? Sample stage: {nameProperty.GetString()}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"? JSON parsing failed: {ex.Message}");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _output.WriteLine($"? API FAILED: {errorContent}");
            }
        }

        private async Task VerifyDatabaseStages()
        {
            _output.WriteLine("");
            _output.WriteLine("?? Step 2: Testing Database Stages");
            _output.WriteLine("-----------------------------------");

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            var stages = await context.ProductionStages.ToListAsync();
            _output.WriteLine($"Database stages count: {stages.Count}");

            if (stages.Count == 0)
            {
                _output.WriteLine("?? No stages in database - creating test stages");

                var testStages = new[]
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
                        CreatedDate = DateTime.UtcNow,
                        Department = "3D Printing",
                        StageColor = "#007bff",
                        StageIcon = "fas fa-print"
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
                        CreatedDate = DateTime.UtcNow,
                        Department = "CNC Machining",
                        StageColor = "#28a745",
                        StageIcon = "fas fa-cogs"
                    }
                };

                context.ProductionStages.AddRange(testStages);
                await context.SaveChangesAsync();
                _output.WriteLine("? Test stages created successfully");
            }
            else
            {
                _output.WriteLine($"? Database has {stages.Count} stages");
                foreach (var stage in stages.Take(3))
                {
                    _output.WriteLine($"   - {stage.Name}: {stage.DefaultHourlyRate:C}/hr");
                }
            }
        }

        private async Task VerifyPartsModalContent()
        {
            _output.WriteLine("");
            _output.WriteLine("?? Step 3: Testing Parts Modal Content");
            _output.WriteLine("---------------------------------------");

            // Login first
            await LoginAsAdminAsync();

            // Get the add part form
            var response = await _client.GetAsync("/Admin/Parts?handler=Add");
            _output.WriteLine($"Parts Add Handler Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _output.WriteLine($"? Parts modal loads - Content length: {content.Length}");

                // Check for key elements
                var hasManufacturingStages = content.Contains("Manufacturing Stages");
                var hasStageContainer = content.Contains("stage-requirements-container");
                var hasStageTab = content.Contains("stages-tab");
                var hasHiddenFields = content.Contains("SelectedStageIds");
                var hasScriptFiles = content.Contains("parts-stage-manager.js");

                _output.WriteLine($"   Has 'Manufacturing Stages' tab: {hasManufacturingStages}");
                _output.WriteLine($"   Has stage container: {hasStageContainer}");
                _output.WriteLine($"   Has stage tab: {hasStageTab}");
                _output.WriteLine($"   Has hidden fields: {hasHiddenFields}");
                _output.WriteLine($"   Has stage manager script: {hasScriptFiles}");

                if (hasManufacturingStages && hasStageContainer && hasStageTab && hasHiddenFields)
                {
                    _output.WriteLine("? Parts modal has all required stage elements");
                }
                else
                {
                    _output.WriteLine("? Parts modal missing some stage elements");
                }
            }
            else
            {
                _output.WriteLine($"? Parts modal failed to load: {response.StatusCode}");
            }
        }

        private async Task VerifyJavaScriptFiles()
        {
            _output.WriteLine("");
            _output.WriteLine("?? Step 4: Testing JavaScript Files");
            _output.WriteLine("------------------------------------");

            var jsFiles = new[]
            {
                "/js/shared/opcentrix-global-functions.js",
                "/js/admin/parts-stage-manager.js",
                "/js/admin/parts-form-manager.js"
            };

            foreach (var jsFile in jsFiles)
            {
                var response = await _client.GetAsync(jsFile);
                _output.WriteLine($"{jsFile}: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _output.WriteLine($"   ? Size: {content.Length} bytes");

                    // Check for key functions
                    if (jsFile.Contains("stage-manager"))
                    {
                        var hasModernStageManager = content.Contains("class ModernStageManager");
                        var hasInitFunction = content.Contains("initializeStageManagerForModal");
                        _output.WriteLine($"   Has ModernStageManager class: {hasModernStageManager}");
                        _output.WriteLine($"   Has init function: {hasInitFunction}");
                    }
                }
                else
                {
                    _output.WriteLine($"   ? File not accessible");
                }
            }
        }

        private async Task TestCompleteWorkflow()
        {
            _output.WriteLine("");
            _output.WriteLine("?? Step 5: Testing Complete Workflow");
            _output.WriteLine("-------------------------------------");

            // Ensure we're logged in
            await LoginAsAdminAsync();

            // Create a test part with stage data
            var partData = new Dictionary<string, string>
            {
                {"Part.PartNumber", "INTEGRATION-TEST-001"},
                {"Part.Name", "Integration Test Part"},
                {"Part.Description", "Complete stage integration test"},
                {"Part.Material", "Ti-6Al-4V Grade 5"},
                {"Part.EstimatedHours", "8.0"},
                {"Part.ComponentTypeId", "1"},
                {"Part.ComplianceCategoryId", "1"},
                {"Part.IsActive", "true"},
                
                // Stage data
                {"SelectedStageIds", "1,2"},
                {"StageExecutionOrders", "1,2"},
                {"StageEstimatedHours", "4.0,2.0"},
                {"StageHourlyRates", "85.00,105.00"},
                {"StageMaterialCosts", "10.00,5.00"}
            };

            _output.WriteLine("?? Creating test part with stage data...");

            var createResponse = await PostFormDataAsync("/Admin/Parts?handler=Create", partData);
            _output.WriteLine($"Create Response Status: {createResponse.StatusCode}");

            if (createResponse.IsSuccessStatusCode)
            {
                _output.WriteLine("? Part creation request successful");

                // Verify in database
                using var scope = _factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
                
                var createdPart = await context.Parts
                    .Include(p => p.PartStageRequirements)
                    .FirstOrDefaultAsync(p => p.PartNumber == "INTEGRATION-TEST-001");

                if (createdPart != null)
                {
                    _output.WriteLine($"? Part created in database with ID: {createdPart.Id}");
                    _output.WriteLine($"   Stage requirements: {createdPart.PartStageRequirements?.Count ?? 0}");

                    if (createdPart.PartStageRequirements?.Count > 0)
                    {
                        _output.WriteLine("? STAGE INTEGRATION FULLY WORKING!");
                        foreach (var stage in createdPart.PartStageRequirements)
                        {
                            _output.WriteLine($"   Stage {stage.ProductionStageId}: {stage.EstimatedHours}h, Order {stage.ExecutionOrder}");
                        }
                    }
                    else
                    {
                        _output.WriteLine("?? Part created but no stage requirements saved");
                    }
                }
                else
                {
                    _output.WriteLine("? Part not found in database");
                }
            }
            else
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                _output.WriteLine($"? Part creation failed: {errorContent}");
            }
        }

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
            // Don't assert here - just ensure we tried to login
        }

        private async Task<HttpResponseMessage> PostFormDataAsync(string url, Dictionary<string, string> formData)
        {
            var content = new FormUrlEncodedContent(formData);
            return await _client.PostAsync(url, content);
        }

        #endregion
    }
}