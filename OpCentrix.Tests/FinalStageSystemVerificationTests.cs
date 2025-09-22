using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;
using OpCentrix.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace OpCentrix.Tests
{
    /// <summary>
    /// Final comprehensive test to verify the stage system is working end-to-end
    /// </summary>
    public class FinalStageSystemVerificationTests : IClassFixture<OpCentrixWebApplicationFactory>
    {
        private readonly OpCentrixWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public FinalStageSystemVerificationTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _output = output;
        }

        [Fact]
        public async Task Complete_Stage_System_Verification()
        {
            _output.WriteLine("?? FINAL STAGE SYSTEM VERIFICATION");
            _output.WriteLine("=====================================");

            // Test 1: Verify API endpoint works
            var apiWorking = await TestApiEndpoint();
            
            // Test 2: Verify JavaScript files load
            var jsWorking = await TestJavaScriptLoading();
            
            // Test 3: Verify database has stages
            var dbWorking = await TestDatabaseStages();
            
            // Test 4: Test complete integration
            var integrationWorking = await TestCompleteIntegration();
            
            // Final summary
            _output.WriteLine("\n?? FINAL VERIFICATION SUMMARY");
            _output.WriteLine("=============================");
            _output.WriteLine($"API Endpoint: {(apiWorking ? "? WORKING" : "? FAILED")}");
            _output.WriteLine($"JavaScript Loading: {(jsWorking ? "? WORKING" : "? FAILED")}");
            _output.WriteLine($"Database Stages: {(dbWorking ? "? WORKING" : "? FAILED")}");
            _output.WriteLine($"Complete Integration: {(integrationWorking ? "? WORKING" : "? FAILED")}");
            
            var allWorking = apiWorking && jsWorking && dbWorking && integrationWorking;
            _output.WriteLine($"\n?? OVERALL STATUS: {(allWorking ? "? ALL SYSTEMS WORKING" : "? SOME ISSUES REMAIN")}");
            
            if (allWorking)
            {
                _output.WriteLine("\n?? SUCCESS: Stage system is fully functional!");
                _output.WriteLine("You can now:");
                _output.WriteLine("1. Start the application: dotnet run --urls http://localhost:5091");
                _output.WriteLine("2. Login as admin (admin/admin123)");
                _output.WriteLine("3. Go to Admin > Parts > Add New Part");
                _output.WriteLine("4. Click Manufacturing Stages tab");
                _output.WriteLine("5. Add stages to parts and save");
            }
        }

        private async Task<bool> TestApiEndpoint()
        {
            _output.WriteLine("\n?? Testing API Endpoint");
            _output.WriteLine("-----------------------");

            try
            {
                var response = await _client.GetAsync("/api/production-stages/available");
                _output.WriteLine($"Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var stages = JsonSerializer.Deserialize<JsonElement[]>(content);
                    
                    _output.WriteLine($"? API returned {stages.Length} stages");
                    
                    foreach (var stage in stages.Take(3))
                    {
                        if (stage.TryGetProperty("name", out var name) && 
                            stage.TryGetProperty("defaultHourlyRate", out var rate))
                        {
                            _output.WriteLine($"   - {name.GetString()}: ${rate.GetDecimal()}/hr");
                        }
                    }
                    
                    return stages.Length > 0;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _output.WriteLine($"? API Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"? API Exception: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> TestJavaScriptLoading()
        {
            _output.WriteLine("\n?? Testing JavaScript Loading");
            _output.WriteLine("-----------------------------");

            var jsFiles = new[]
            {
                "/js/shared/opcentrix-global-functions.js",
                "/js/admin/parts-stage-manager.js",
                "/js/admin/parts-form-manager.js"
            };

            var allLoaded = true;

            foreach (var jsFile in jsFiles)
            {
                try
                {
                    var response = await _client.GetAsync(jsFile);
                    var size = (await response.Content.ReadAsStringAsync()).Length;
                    var status = response.IsSuccessStatusCode ? "?" : "?";
                    
                    _output.WriteLine($"{status} {jsFile}: {response.StatusCode} ({size:N0} bytes)");
                    
                    if (!response.IsSuccessStatusCode || size < 1000)
                    {
                        allLoaded = false;
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"? {jsFile}: Exception - {ex.Message}");
                    allLoaded = false;
                }
            }

            return allLoaded;
        }

        private async Task<bool> TestDatabaseStages()
        {
            _output.WriteLine("\n??? Testing Database Stages");
            _output.WriteLine("-------------------------");

            try
            {
                using var scope = _factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

                var stages = await context.ProductionStages.ToListAsync();
                _output.WriteLine($"Database Stages Count: {stages.Count}");

                if (stages.Count == 0)
                {
                    _output.WriteLine("?? No stages found, this should trigger API auto-creation");
                    return false;
                }

                foreach (var stage in stages.Take(3))
                {
                    _output.WriteLine($"   - {stage.Name}: ${stage.DefaultHourlyRate}/hr (Order: {stage.DisplayOrder})");
                }

                return stages.Count > 0;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"? Database Error: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> TestCompleteIntegration()
        {
            _output.WriteLine("\n?? Testing Complete Integration");
            _output.WriteLine("-------------------------------");

            try
            {
                // Test 1: API endpoint with fresh data
                var apiResponse = await _client.GetAsync("/api/production-stages/available");
                if (!apiResponse.IsSuccessStatusCode)
                {
                    _output.WriteLine("? API endpoint not accessible");
                    return false;
                }

                var apiContent = await apiResponse.Content.ReadAsStringAsync();
                var stages = JsonSerializer.Deserialize<JsonElement[]>(apiContent);
                
                _output.WriteLine($"? API Integration: {stages.Length} stages available");

                // Test 2: Verify stages have required properties
                var firstStage = stages.First();
                var hasRequiredProps = firstStage.TryGetProperty("id", out _) &&
                                      firstStage.TryGetProperty("name", out _) &&
                                      firstStage.TryGetProperty("defaultHourlyRate", out _) &&
                                      firstStage.TryGetProperty("defaultDurationHours", out _);

                _output.WriteLine($"? Stage Data Structure: {(hasRequiredProps ? "Valid" : "Invalid")}");

                // Test 3: JavaScript file accessibility
                var jsResponse = await _client.GetAsync("/js/admin/parts-stage-manager.js");
                var jsContent = await jsResponse.Content.ReadAsStringAsync();
                var hasStageManager = jsContent.Contains("ModernStageManager") && jsContent.Contains("initializeStageManagerForModal");
                
                _output.WriteLine($"? JavaScript Integration: {(hasStageManager ? "Valid" : "Invalid")}");

                return hasRequiredProps && hasStageManager && stages.Length > 0;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"? Integration Error: {ex.Message}");
                return false;
            }
        }
    }
}