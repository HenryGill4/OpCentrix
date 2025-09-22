using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;
using System.Text.Json;

namespace OpCentrix.Tests
{
    public class ApiRoutingTests : IClassFixture<OpCentrixWebApplicationFactory>
    {
        private readonly OpCentrixWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public ApiRoutingTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _output = output;
        }

        [Fact]
        public async Task Test_Production_Stages_API_Routing()
        {
            _output.WriteLine("?? Testing Production Stages API Routing");
            
            // Test the simple test controller first
            _output.WriteLine("Testing simple test controller...");
            var simpleResponse = await _client.GetAsync("/api/test");
            _output.WriteLine($"Simple test endpoint status: {simpleResponse.StatusCode}");
            
            if (simpleResponse.IsSuccessStatusCode)
            {
                var simpleContent = await simpleResponse.Content.ReadAsStringAsync();
                _output.WriteLine($"Simple test response: {simpleContent}");
            }
            else
            {
                var errorContent = await simpleResponse.Content.ReadAsStringAsync();
                _output.WriteLine($"Simple test error: {errorContent}");
            }
            
            // Test the stages via test controller
            _output.WriteLine("Testing stages via test controller...");
            var testStagesResponse = await _client.GetAsync("/api/test/stages");
            _output.WriteLine($"Test stages endpoint status: {testStagesResponse.StatusCode}");
            
            if (testStagesResponse.IsSuccessStatusCode)
            {
                var testStagesContent = await testStagesResponse.Content.ReadAsStringAsync();
                _output.WriteLine($"Test stages response: {testStagesContent}");
            }
            
            // Test the custom route added to Program.cs
            _output.WriteLine("Testing custom route...");
            var customResponse = await _client.GetAsync("/api/production-stages/test");
            _output.WriteLine($"Custom route status: {customResponse.StatusCode}");
            
            if (customResponse.IsSuccessStatusCode)
            {
                var customContent = await customResponse.Content.ReadAsStringAsync();
                _output.WriteLine($"Custom route response: {customContent}");
            }
            
            // Test the main problematic endpoint
            _output.WriteLine("Testing main endpoint...");
            var response = await _client.GetAsync("/api/production-stages/available");
            _output.WriteLine($"Main endpoint status: {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response length: {content.Length}");
            _output.WriteLine($"First 500 chars: {content.Substring(0, Math.Min(500, content.Length))}");
            
            if (response.IsSuccessStatusCode && content.StartsWith('['))
            {
                try
                {
                    var stages = JsonSerializer.Deserialize<JsonElement[]>(content);
                    _output.WriteLine($"? Successfully parsed {stages.Length} stages");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"? JSON parsing failed: {ex.Message}");
                }
            }
        }
    }
}