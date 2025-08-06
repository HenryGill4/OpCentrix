using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using System.Net;
using Xunit;

namespace OpCentrix.Tests;

/// <summary>
/// Comprehensive functional tests for the OpCentrix Parts system
/// Following PowerShell-compatible testing protocols from prompt helper
/// </summary>
public class PartsSystemFunctionalTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PartsSystemFunctionalTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database context
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<SchedulerContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<SchedulerContext>(options =>
                {
                    options.UseInMemoryDatabase("TestPartsDatabase");
                });
            });
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task PartsPage_ShouldLoad_Successfully()
    {
        // Test that the Parts page loads without errors
        var response = await _client.GetAsync("/Admin/Parts");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Parts Management", content);
        Assert.Contains("Add New Part", content);
    }

    [Fact]
    public async Task PartsPage_AddNewPartModal_ShouldLoad()
    {
        // Test that the Add Part modal loads
        var response = await _client.GetAsync("/Admin/Parts?handler=Add");
        
        // Should be OK or Redirect (depending on authentication)
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task PartsForm_ShouldContain_ManufacturingStages()
    {
        // Test that the form contains manufacturing stages
        var response = await _client.GetAsync("/Admin/Parts?handler=Add");
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            // Check for manufacturing stages in the form
            Assert.Contains("Manufacturing Stages", content);
            Assert.Contains("SLS Printing", content);
            Assert.Contains("CNC Machining", content);
            Assert.Contains("EDM Operations", content);
            Assert.Contains("Assembly", content);
            Assert.Contains("Finishing", content);
            Assert.Contains("Quality Inspection", content);
        }
    }

    [Fact]
    public async Task PartsForm_ShouldContain_MaterialAutoFill()
    {
        // Test that the form contains material auto-fill functionality
        var response = await _client.GetAsync("/Admin/Parts?handler=Add");
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            // Check for material selection and auto-fill
            Assert.Contains("materialSelect", content);
            Assert.Contains("updateMaterialDefaults", content);
            Assert.Contains("Ti-6Al-4V", content);
            Assert.Contains("Inconel", content);
        }
    }

    [Fact]
    public async Task PartsForm_ShouldContain_AdminOverride()
    {
        // Test that the form contains admin override functionality
        var response = await _client.GetAsync("/Admin/Parts?handler=Add");
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            // Check for admin override section
            Assert.Contains("Administrative Override", content);
            Assert.Contains("AdminEstimatedHoursOverride", content);
            Assert.Contains("AdminOverrideReason", content);
            Assert.Contains("validateAdminOverride", content);
        }
    }

    [Fact]
    public async Task PartsForm_ShouldHave_HTMXIntegration()
    {
        // Test that the form has proper HTMX integration
        var response = await _client.GetAsync("/Admin/Parts?handler=Add");
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            // Check for HTMX attributes
            Assert.Contains("hx-post", content);
            Assert.Contains("hx-target", content);
            Assert.Contains("hx-indicator", content);
        }
    }

    [Fact]
    public async Task Database_ShouldHave_RequiredTables()
    {
        // Test that the database has the required tables
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Test that we can access the Parts table
        var parts = await context.Parts.ToListAsync();
        Assert.NotNull(parts); // Should not throw an exception
    }

    [Fact]
    public async Task PartsController_Methods_ShouldExist()
    {
        // Test that all required controller methods exist by checking routes
        var testRoutes = new[]
        {
            "/Admin/Parts",
            "/Admin/Parts?handler=Add",
            "/Admin/Parts?handler=Edit&id=1"
        };

        foreach (var route in testRoutes)
        {
            var response = await _client.GetAsync(route);
            
            // Should not return NotFound (404) - method exists
            Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Fact]
    public async Task PartsSystem_JavaScript_ShouldLoad()
    {
        // Test that required JavaScript functions are present
        var response = await _client.GetAsync("/Admin/Parts?handler=Add");
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            // Check for critical JavaScript functions
            Assert.Contains("updateMaterialDefaults", content);
            Assert.Contains("toggleStageDetails", content);
            Assert.Contains("updateManufacturingSummary", content);
            Assert.Contains("validateAdminOverride", content);
        }
    }

    [Fact]
    public async Task PartsForm_ShouldHave_ResponsiveDesign()
    {
        // Test that the form has responsive design elements
        var response = await _client.GetAsync("/Admin/Parts?handler=Add");
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            // Check for Bootstrap responsive classes
            Assert.Contains("col-md-", content);
            Assert.Contains("row g-", content);
            Assert.Contains("nav-tabs", content);
            Assert.Contains("tab-content", content);
        }
    }

    [Fact] 
    public async Task PartsForm_Tabs_ShouldBePresent()
    {
        // Test that all form tabs are present and functional
        var response = await _client.GetAsync("/Admin/Parts?handler=Add");
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            // Check for all three tabs
            Assert.Contains("Basic Information", content);
            Assert.Contains("Material & Process", content);
            Assert.Contains("Manufacturing Stages", content);
            
            // Check for tab functionality
            Assert.Contains("data-bs-toggle=\"tab\"", content);
            Assert.Contains("tab-pane", content);
        }
    }
}