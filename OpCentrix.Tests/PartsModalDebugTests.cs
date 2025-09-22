using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;
using AngleSharp;
using AngleSharp.Html.Dom;

namespace OpCentrix.Tests
{
    /// <summary>
    /// Debug test to examine exactly what the Parts Add handler returns
    /// </summary>
    public class PartsModalDebugTests : IClassFixture<OpCentrixWebApplicationFactory>
    {
        private readonly OpCentrixWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public PartsModalDebugTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _output = output;
        }

        [Fact]
        public async Task Debug_Parts_Add_Response_Content()
        {
            _output.WriteLine("?? DEBUGGING: Parts Add Handler Response");
            _output.WriteLine("==========================================");

            // Login first
            await LoginAsAdminAsync();

            // Get the add part form
            var response = await _client.GetAsync("/Admin/Parts?handler=Add");
            var content = await response.Content.ReadAsStringAsync();

            _output.WriteLine($"Response Status: {response.StatusCode}");
            _output.WriteLine($"Content Length: {content.Length}");
            _output.WriteLine($"Content Type: {response.Content.Headers.ContentType}");

            // Save content to file for examination
            var fileName = "debug_parts_add_response.html";
            await File.WriteAllTextAsync(fileName, content);
            _output.WriteLine($"?? Full response saved to: {fileName}");

            // Check key content strings
            var keyStrings = new[]
            {
                "Manufacturing Stages",
                "stage-requirements-container", 
                "stages-tab",
                "SelectedStageIds",
                "parts-stage-manager.js",
                "Add New Part",
                "Basic Information",
                "_PartForm"
            };

            _output.WriteLine("");
            _output.WriteLine("?? Content Analysis:");
            foreach (var str in keyStrings)
            {
                var contains = content.Contains(str);
                _output.WriteLine($"   {str}: {(contains ? "? Found" : "? Missing")}");
            }

            // Look for HTML structure
            if (content.StartsWith("<!DOCTYPE html>"))
            {
                _output.WriteLine("? ISSUE: Receiving full HTML page instead of partial view!");
                
                // Parse the HTML to see what page we're getting
                var config = Configuration.Default;
                var context = BrowsingContext.New(config);
                var document = await context.OpenAsync(req => req.Content(content));
                
                var title = document.QuerySelector("title")?.TextContent;
                var h1 = document.QuerySelector("h1")?.TextContent;
                var bodyClass = document.QuerySelector("body")?.GetAttribute("class");
                
                _output.WriteLine($"   Page Title: {title}");
                _output.WriteLine($"   H1 Content: {h1}");
                _output.WriteLine($"   Body Class: {bodyClass}");
                
                // Check for login page
                if (content.Contains("login") || content.Contains("Login"))
                {
                    _output.WriteLine("? CRITICAL: Getting redirected to login page!");
                    _output.WriteLine("   The OnGetAddAsync handler requires authentication that's not working in tests");
                }
            }
            else
            {
                _output.WriteLine("? Receiving partial content (not full page)");
                
                // Show first 500 characters
                _output.WriteLine("");
                _output.WriteLine("?? First 500 characters of response:");
                _output.WriteLine($"'{content.Substring(0, Math.Min(500, content.Length))}'");
                
                // Show last 200 characters
                if (content.Length > 500)
                {
                    _output.WriteLine("");
                    _output.WriteLine("?? Last 200 characters of response:");
                    var start = Math.Max(0, content.Length - 200);
                    _output.WriteLine($"'{content.Substring(start)}'");
                }
            }

            // Always pass the test, this is just for debugging
            Assert.True(true, "Debug test - check output for details");
        }

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
            // Don't assert here - login may fail but we still want to see the debug output
        }

        private async Task<HttpResponseMessage> PostFormDataAsync(string url, Dictionary<string, string> formData)
        {
            var content = new FormUrlEncodedContent(formData);
            return await _client.PostAsync(url, content);
        }
    }
}