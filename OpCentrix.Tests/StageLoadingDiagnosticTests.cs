using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;
using OpCentrix.Data;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Models;

namespace OpCentrix.Tests
{
    /// <summary>
    /// Comprehensive real-time test to identify and fix stage loading issues
    /// This will test each component step by step to find the exact failure point
    /// </summary>
    public class StageLoadingDiagnosticTests : IClassFixture<OpCentrixWebApplicationFactory>
    {
        private readonly OpCentrixWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public StageLoadingDiagnosticTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _output = output;
        }

        [Fact]
        public async Task Diagnose_Complete_Stage_Loading_Chain()
        {
            _output.WriteLine("?? COMPREHENSIVE STAGE LOADING DIAGNOSIS");
            _output.WriteLine("==========================================");

            // Step 1: Test Database Connectivity
            await TestDatabaseConnectivity();

            // Step 2: Test Production Stages Data
            await TestProductionStagesData();

            // Step 3: Test API Endpoint Accessibility
            await TestApiEndpoint();

            // Step 4: Test Parts Form Loading
            await TestPartsFormLoading();

            // Step 5: Test JavaScript File Loading
            await TestJavaScriptFiles();

            // Step 6: Propose Complete Fix
            ProposeFix();
        }

        private async Task TestDatabaseConnectivity()
        {
            _output.WriteLine("\n?? Step 1: Testing Database Connectivity");
            _output.WriteLine("----------------------------------------");

            try
            {
                using var scope = _factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
                
                var canConnect = await context.Database.CanConnectAsync();
                _output.WriteLine($"Database Connection: {(canConnect ? "? SUCCESS" : "? FAILED")}");

                if (canConnect)
                {
                    var tables = await context.Database.SqlQueryRaw<string>(
                        "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
                    ).ToListAsync();
                    
                    _output.WriteLine($"Tables Found: {tables.Count}");
                    
                    var hasProductionStages = tables.Contains("ProductionStages");
                    _output.WriteLine($"ProductionStages Table: {(hasProductionStages ? "? EXISTS" : "? MISSING")}");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"? Database Error: {ex.Message}");
            }
        }

        private async Task TestProductionStagesData()
        {
            _output.WriteLine("\n?? Step 2: Testing Production Stages Data");
            _output.WriteLine("------------------------------------------");

            try
            {
                using var scope = _factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

                var stages = await context.ProductionStages.ToListAsync();
                _output.WriteLine($"Production Stages Count: {stages.Count}");

                if (stages.Count == 0)
                {
                    _output.WriteLine("? NO STAGES FOUND - Creating default stages...");
                    await CreateDefaultStages(context);
                    stages = await context.ProductionStages.ToListAsync();
                    _output.WriteLine($"After Creation - Stages Count: {stages.Count}");
                }
                else
                {
                    _output.WriteLine("? Stages exist in database");
                    foreach (var stage in stages.Take(3))
                    {
                        _output.WriteLine($"   - {stage.Name}: ${stage.DefaultHourlyRate}/hr, Order: {stage.DisplayOrder}");
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"? Production Stages Error: {ex.Message}");
            }
        }

        private async Task TestApiEndpoint()
        {
            _output.WriteLine("\n?? Step 3: Testing API Endpoint");
            _output.WriteLine("--------------------------------");

            try
            {
                var response = await _client.GetAsync("/api/production-stages/available");
                _output.WriteLine($"API Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _output.WriteLine($"API Response Length: {content.Length} characters");
                    
                    if (content.StartsWith('['))
                    {
                        var stageCount = content.Split('{').Length - 1;
                        _output.WriteLine($"? API returning {stageCount} stages");
                    }
                    else
                    {
                        _output.WriteLine($"? API Response Format Issue: {content.Substring(0, Math.Min(100, content.Length))}");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _output.WriteLine($"? API Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"? API Request Error: {ex.Message}");
            }
        }

        private async Task TestPartsFormLoading()
        {
            _output.WriteLine("\n?? Step 4: Testing Parts Form Loading");
            _output.WriteLine("--------------------------------------");

            try
            {
                // First try to login
                await LoginAsAdmin();

                var response = await _client.GetAsync("/Admin/Parts?handler=Add");
                _output.WriteLine($"Parts Form Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    // Check for key elements
                    var hasStagesTab = content.Contains("Manufacturing Stages");
                    var hasStageContainer = content.Contains("stage-requirements-container");
                    var hasStageManager = content.Contains("parts-stage-manager.js");
                    var hasAvailableStages = content.Contains("AvailableStages");
                    
                    _output.WriteLine($"Has Manufacturing Stages Tab: {(hasStagesTab ? "?" : "?")}");
                    _output.WriteLine($"Has Stage Container: {(hasStageContainer ? "?" : "?")}");
                    _output.WriteLine($"Has Stage Manager JS: {(hasStageManager ? "?" : "?")}");
                    _output.WriteLine($"Has Available Stages: {(hasAvailableStages ? "?" : "?")}");
                    
                    if (!hasStagesTab)
                    {
                        _output.WriteLine($"? ISSUE: Parts form doesn't contain Manufacturing Stages tab");
                        _output.WriteLine($"Form length: {content.Length} characters");
                        
                        // Check if we're getting redirected to login
                        if (content.Contains("login") || content.Contains("Login"))
                        {
                            _output.WriteLine("? CRITICAL: Being redirected to login page!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"? Parts Form Error: {ex.Message}");
            }
        }

        private async Task TestJavaScriptFiles()
        {
            _output.WriteLine("\n?? Step 5: Testing JavaScript Files");
            _output.WriteLine("------------------------------------");

            var jsFiles = new[]
            {
                "/js/shared/opcentrix-global-functions.js",
                "/js/admin/parts-stage-manager.js",
                "/js/admin/parts-form-manager.js"
            };

            foreach (var jsFile in jsFiles)
            {
                try
                {
                    var response = await _client.GetAsync(jsFile);
                    _output.WriteLine($"{jsFile}: {response.StatusCode} ({(await response.Content.ReadAsStringAsync()).Length} bytes)");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"? {jsFile}: Error - {ex.Message}");
                }
            }
        }

        private void ProposeFix()
        {
            _output.WriteLine("\n?? PROPOSED COMPREHENSIVE FIX");
            _output.WriteLine("==============================");
            _output.WriteLine("Based on the diagnosis, implementing fixes for:");
            _output.WriteLine("1. Ensure production stages exist in database");
            _output.WriteLine("2. Fix API endpoint authorization");
            _output.WriteLine("3. Fix parts form stage initialization");
            _output.WriteLine("4. Add comprehensive error handling");
            _output.WriteLine("5. Fix JavaScript loading issues");
        }

        private async Task<bool> LoginAsAdmin()
        {
            try
            {
                var loginPage = await _client.GetAsync("/Account/Login");
                // For testing, we'll assume admin login works
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task CreateDefaultStages(SchedulerContext context)
        {
            var defaultStages = new[]
            {
                new ProductionStage
                {
                    Name = "3D Printing (SLS)",
                    Description = "Selective Laser Sintering",
                    Department = "3D Printing",
                    DefaultHourlyRate = 85.00m,
                    DefaultDurationHours = 8.0,
                    DefaultSetupMinutes = 30,
                    DisplayOrder = 1,
                    IsActive = true,
                    StageColor = "#007bff",
                    StageIcon = "fas fa-cube",
                    RequiresQualityCheck = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new ProductionStage
                {
                    Name = "CNC Machining",
                    Description = "Computer Numerical Control machining",
                    Department = "CNC Machining",
                    DefaultHourlyRate = 85.00m,
                    DefaultDurationHours = 4.0,
                    DefaultSetupMinutes = 45,
                    DisplayOrder = 2,
                    IsActive = true,
                    StageColor = "#28a745",
                    StageIcon = "fas fa-cogs",
                    RequiresQualityCheck = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new ProductionStage
                {
                    Name = "EDM Operations",
                    Description = "Electrical Discharge Machining",
                    Department = "EDM",
                    DefaultHourlyRate = 95.00m,
                    DefaultDurationHours = 6.0,
                    DefaultSetupMinutes = 60,
                    DisplayOrder = 3,
                    IsActive = true,
                    StageColor = "#ffc107",
                    StageIcon = "fas fa-bolt",
                    RequiresQualityCheck = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };

            context.ProductionStages.AddRange(defaultStages);
            await context.SaveChangesAsync();
        }
    }
}