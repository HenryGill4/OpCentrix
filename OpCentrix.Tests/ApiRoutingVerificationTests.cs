using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace OpCentrix.Tests
{
    /// <summary>
    /// Quick API routing verification test
    /// </summary>
    public class ApiRoutingVerificationTests : IClassFixture<OpCentrixWebApplicationFactory>
    {
        private readonly OpCentrixWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public ApiRoutingVerificationTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _output = output;
        }

        [Fact]
        public async Task API_Test_Endpoint_Should_Work()
        {
            _output.WriteLine("?? Testing API routing with test endpoint");

            var response = await _client.GetAsync("/api/production-stages/test");
            var content = await response.Content.ReadAsStringAsync();

            _output.WriteLine($"Status: {response.StatusCode}");
            _output.WriteLine($"Content: {content}");

            if (response.IsSuccessStatusCode)
            {
                _output.WriteLine("? API routing is working!");
            }
            else
            {
                _output.WriteLine($"? API routing failed: {response.StatusCode}");
                if (content.Contains("Error"))
                {
                    _output.WriteLine("Error page returned instead of API response");
                }
            }

            Assert.True(response.IsSuccessStatusCode, $"API test endpoint failed: {response.StatusCode}");
        }
    }
}