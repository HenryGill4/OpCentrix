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
    /// Focused Stage Integration Diagnostic Tests
    /// Diagnoses specific stage functionality issues
    /// </summary>
    public class StageDiagnosticTests : IClassFixture<OpCentrixWebApplicationFactory>
    {
        private readonly OpCentrixWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public StageDiagnosticTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _output = output;
        }

        [Fact]
        public async Task Diagnose_API_Authorization_Issue()
        {
            _output.WriteLine("?? DIAGNOSING: API Authorization Issue");

            // Test 1: API without authentication
            var unauthorizedResponse = await _client.GetAsync("/api/production-stages/available");
            _output.WriteLine($"API Response (No Auth): {unauthorizedResponse.StatusCode}");
            
            if (unauthorizedResponse.StatusCode == System.Net.HttpStatusCode.Redirect)
            {
                var location = unauthorizedResponse.Headers.Location?.ToString();
                _output.WriteLine($"? ISSUE: API redirects to: {location}");
                _output.WriteLine("?? FIX: API requires authentication but stage manager calls it without auth");
            }

            // Test 2: API with authentication
            await LoginAsAdminAsync();
            var authorizedResponse = await _client.GetAsync("/api/production-stages/available");
            _output.WriteLine($"API Response (With Auth): {authorizedResponse.StatusCode}");

            if (authorizedResponse.IsSuccessStatusCode)
            {
                var content = await authorizedResponse.Content.ReadAsStringAsync();
                _output.WriteLine($"? API SUCCESS: Content length: {content.Length}");
                
                try
                {
                    var stages = JsonSerializer.Deserialize<JsonElement[]>(content);
                    _output.WriteLine($"? API returns {stages.Length} stages");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"? API content is not valid JSON: {ex.Message}");
                }
            }
            else
            {
                var errorContent = await authorizedResponse.Content.ReadAsStringAsync();
                _output.WriteLine($"? API FAILED even with auth: {errorContent}");
            }
        }

        [Fact]
        public async Task Diagnose_Parts_Modal_Content_Issue()
        {
            _output.WriteLine("?? DIAGNOSING: Parts Modal Content Issue");

            await LoginAsAdminAsync();

            // Test the Add handler
            var response = await _client.GetAsync("/Admin/Parts?handler=Add");
            var content = await response.Content.ReadAsStringAsync();

            _output.WriteLine($"Add Handler Status: {response.StatusCode}");
            _output.WriteLine($"Content Length: {content.Length}");

            // Check what we're actually getting
            if (content.Contains("<!DOCTYPE html>"))
            {
                _output.WriteLine("? ISSUE: Getting full HTML page instead of partial view");
                _output.WriteLine("?? FIX: OnGetAddAsync should return Partial, not full page");
                
                // Check if it's a login redirect
                if (content.Contains("Login") || content.Contains("login"))
                {
                    _output.WriteLine("? ADDITIONAL ISSUE: Login required for parts modal");
                }
            }
            else
            {
                _output.WriteLine("? Receiving partial content");
                
                // Check for stage-related content
                var hasManufacturingStages = content.Contains("Manufacturing Stages");
                var hasStageContainer = content.Contains("stage-requirements-container");
                var hasStageTab = content.Contains("stages-tab");
                var hasHiddenFields = content.Contains("SelectedStageIds");
                
                _output.WriteLine($"Has 'Manufacturing Stages': {hasManufacturingStages}");
                _output.WriteLine($"Has stage container: {hasStageContainer}");
                _output.WriteLine($"Has stage tab: {hasStageTab}");
                _output.WriteLine($"Has hidden fields: {hasHiddenFields}");
                
                if (!hasManufacturingStages || !hasStageContainer)
                {
                    _output.WriteLine("? ISSUE: Stage components missing from partial view");
                    _output.WriteLine("?? FIX: _PartForm.cshtml may be incorrect or not loading properly");
                }
            }

            // Save content for analysis
            await File.WriteAllTextAsync("debug_add_response.html", content);
            _output.WriteLine("?? Response saved to debug_add_response.html for analysis");
        }

        [Fact]
        public async Task Diagnose_JavaScript_File_Loading()
        {
            _output.WriteLine("?? DIAGNOSING: JavaScript File Loading");

            await LoginAsAdminAsync();

            // Test each JavaScript file individually
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
                    _output.WriteLine($"  Size: {content.Length} bytes");

                    // Check for key functions
                    if (jsFile.Contains("stage-manager"))
                    {
                        var hasModernStageManager = content.Contains("ModernStageManager");
                        var hasInitFunction = content.Contains("initializeStageManagerForModal");
                        _output.WriteLine($"  Has ModernStageManager: {hasModernStageManager}");
                        _output.WriteLine($"  Has init function: {hasInitFunction}");
                    }
                }
                else
                {
                    _output.WriteLine($"  ? ISSUE: {jsFile} not accessible");
                }
            }
        }

        [Fact]
        public async Task Diagnose_Database_Stage_Data()
        {
            _output.WriteLine("?? DIAGNOSING: Database Stage Data");

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            var stages = await context.ProductionStages.ToListAsync();
            _output.WriteLine($"Database has {stages.Count} production stages");

            if (stages.Count == 0)
            {
                _output.WriteLine("? ISSUE: No production stages in database");
                _output.WriteLine("?? FIX: Need to seed production stages");
                
                // Create test stages
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

                context.ProductionStages.AddRange(defaultStages);
                await context.SaveChangesAsync();
                _output.WriteLine("? Added 2 test production stages");
            }
            else
            {
                foreach (var stage in stages.Take(3))
                {
                    _output.WriteLine($"  Stage: {stage.Name} - {stage.DefaultHourlyRate:C}/hr - Active: {stage.IsActive}");
                }
            }
        }

        [Fact]
        public async Task Diagnose_Production_Stages_Page_JavaScript()
        {
            _output.WriteLine("?? DIAGNOSING: Production Stages Page JavaScript Issues");

            await LoginAsAdminAsync();

            var response = await _client.GetAsync("/Admin/ProductionStages");
            var content = await response.Content.ReadAsStringAsync();

            _output.WriteLine($"Production Stages Page Status: {response.StatusCode}");

            // Check for JavaScript errors or issues
            var hasJSErrors = content.Contains("Uncaught") || 
                              content.Contains("TypeError") || 
                              content.Contains("ReferenceError") ||
                              content.Contains("SyntaxError");

            _output.WriteLine($"Has JavaScript Errors: {hasJSErrors}");

            // Check for required functions
            var hasLoadMachines = content.Contains("loadAvailableMachines");
            var hasAddField = content.Contains("addCustomField");
            var hasEditStage = content.Contains("editStage");

            _output.WriteLine($"Has loadAvailableMachines: {hasLoadMachines}");
            _output.WriteLine($"Has addCustomField: {hasAddField}");
            _output.WriteLine($"Has editStage: {hasEditStage}");

            if (!hasLoadMachines || !hasAddField || !hasEditStage)
            {
                _output.WriteLine("? ISSUE: Missing required JavaScript functions");
                _output.WriteLine("?? FIX: Check ProductionStages/Index.cshtml script section");
            }

            // Save for analysis
            await File.WriteAllTextAsync("debug_production_stages.html", content);
            _output.WriteLine("?? Production stages page saved to debug_production_stages.html");
        }

        [Fact]
        public async Task Create_Test_Part_With_Detailed_Logging()
        {
            _output.WriteLine("?? DIAGNOSING: Complete Part Creation Flow");

            await LoginAsAdminAsync();
            await EnsureProductionStagesExist();

            // Create comprehensive test data
            var partData = new Dictionary<string, string>
            {
                {"Part.PartNumber", "DIAGNOSTIC-TEST-001"},
                {"Part.Name", "Diagnostic Test Part"},
                {"Part.Description", "Testing complete stage integration"},
                {"Part.Material", "Ti-6Al-4V Grade 5"},
                {"Part.EstimatedHours", "8.0"},
                {"Part.ComponentTypeId", "1"},
                {"Part.ComplianceCategoryId", "1"},
                {"Part.IsActive", "true"},
                
                // Stage data - test if this gets processed
                {"SelectedStageIds", "1,2"},
                {"StageExecutionOrders", "1,2"},
                {"StageEstimatedHours", "4.0,2.0"},
                {"StageHourlyRates", "85.00,95.00"},
                {"StageMaterialCosts", "10.00,5.00"}
            };

            _output.WriteLine("?? Creating part with stage data:");
            foreach (var item in partData)
            {
                _output.WriteLine($"  {item.Key}: {item.Value}");
            }

            var response = await PostFormDataAsync("/Admin/Parts?handler=Create", partData);
            
            _output.WriteLine($"Create Response Status: {response.StatusCode}");
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response Length: {responseContent.Length}");

            if (responseContent.Contains("error") || responseContent.Contains("Error"))
            {
                _output.WriteLine("? ISSUE: Create response contains errors");
                await File.WriteAllTextAsync("debug_create_error.html", responseContent);
                _output.WriteLine("?? Error response saved to debug_create_error.html");
            }

            // Check if part was created in database
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            
            var createdPart = await context.Parts
                .Include(p => p.PartStageRequirements)
                .FirstOrDefaultAsync(p => p.PartNumber == "DIAGNOSTIC-TEST-001");

            if (createdPart != null)
            {
                _output.WriteLine($"? Part created successfully with ID: {createdPart.Id}");
                _output.WriteLine($"   Stage requirements count: {createdPart.PartStageRequirements?.Count ?? 0}");
                
                if (createdPart.PartStageRequirements?.Count > 0)
                {
                    _output.WriteLine("? Stage data was persisted correctly");
                    foreach (var stage in createdPart.PartStageRequirements)
                    {
                        _output.WriteLine($"   Stage {stage.ProductionStageId}: {stage.EstimatedHours}h, Order {stage.ExecutionOrder}");
                    }
                }
                else
                {
                    _output.WriteLine("? ISSUE: No stage requirements were saved");
                    _output.WriteLine("?? FIX: SyncStagesFromFormAsync is not working properly");
                }
            }
            else
            {
                _output.WriteLine("? ISSUE: Part was not created in database");
                _output.WriteLine("?? FIX: Check Parts.cshtml.cs OnPostCreateAsync method");
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

                context.ProductionStages.AddRange(defaultStages);
                await context.SaveChangesAsync();
            }
        }

        #endregion
    }
}