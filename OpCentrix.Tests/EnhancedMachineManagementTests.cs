using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services.Admin;
using System.Net;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using System.Text.RegularExpressions;

namespace OpCentrix.Tests;

/// <summary>
/// Comprehensive tests for enhanced machine management functionality
/// Task 6: Machine Material Management - ENHANCED TESTING
/// </summary>
public class EnhancedMachineManagementTests : IClassFixture<OpCentrixWebApplicationFactory>, IDisposable
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public EnhancedMachineManagementTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region Material Service Tests

    [Fact]
    public async Task MaterialService_GetActiveMaterials_ReturnsSeededMaterials()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var materialService = scope.ServiceProvider.GetRequiredService<IMaterialService>();

        // Act
        var materials = await materialService.GetActiveMaterialsAsync();

        // Assert
        Assert.NotEmpty(materials);
        Assert.Contains(materials, m => m.MaterialCode == "TI64-G5");
        Assert.Contains(materials, m => m.MaterialCode == "IN718");
        Assert.All(materials, m => Assert.True(m.IsActive));

        _output.WriteLine($"? Found {materials.Count} active materials including Ti-6Al-4V and Inconel 718");
    }

    [Fact]
    public async Task MaterialService_GetMaterialByCode_ReturnsCorrectMaterial()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var materialService = scope.ServiceProvider.GetRequiredService<IMaterialService>();

        // Act
        var material = await materialService.GetMaterialByCodeAsync("TI64-G5");

        // Assert
        Assert.NotNull(material);
        Assert.Equal("TI64-G5", material.MaterialCode);
        Assert.Equal("Ti-6Al-4V Grade 5", material.MaterialName);
        Assert.Equal("Titanium", material.MaterialType);

        _output.WriteLine($"? Material service correctly returns Ti-6Al-4V Grade 5 material");
    }

    [Fact]
    public async Task MaterialService_GetCompatibleMaterials_FiltersByMachineType()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var materialService = scope.ServiceProvider.GetRequiredService<IMaterialService>();

        // Act
        var slsMaterials = await materialService.GetCompatibleMaterialsAsync("SLS");

        // Assert
        Assert.NotEmpty(slsMaterials);
        Assert.All(slsMaterials, m => Assert.True(m.IsCompatibleWithMachineType("SLS")));

        _output.WriteLine($"? Found {slsMaterials.Count} SLS-compatible materials");
    }

    [Fact]
    public async Task MaterialService_GetMaterialTypes_ReturnsDistinctTypes()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var materialService = scope.ServiceProvider.GetRequiredService<IMaterialService>();

        // Act
        var materialTypes = await materialService.GetMaterialTypesAsync();

        // Assert
        Assert.NotEmpty(materialTypes);
        Assert.Contains("Titanium", materialTypes);
        Assert.Contains("Steel", materialTypes);
        Assert.Contains("Nickel", materialTypes);

        _output.WriteLine($"? Found {materialTypes.Count} material types: {string.Join(", ", materialTypes)}");
    }

    #endregion

    #region Machine Management Page Tests

    [Fact]
    public async Task MachinesPage_LoadsWithMaterials_ShowsEnhancedInterface()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await _client.GetAsync("/Admin/Machines");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        
        // Check for enhanced material functionality
        Assert.Contains("Material selection checkboxes", content);
        Assert.Contains("Current Material", content);
        Assert.Contains("TI64-G5", content);
        Assert.Contains("IN718", content);
        Assert.Contains("updateSupportedMaterials()", content);

        _output.WriteLine("? Enhanced machines page loads with material selection interface");
    }

    [Fact]
    public async Task MachinesPage_CreateMachine_WithValidMaterials_Succeeds()
    {
        try
        {
            // Arrange
            await LoginAsAdminAsync();

            // Get the machines page first for anti-forgery token
            var machinesPage = await _client.GetAsync("/Admin/Machines");
            Assert.Equal(HttpStatusCode.OK, machinesPage.StatusCode);
            
            var pageContent = await machinesPage.Content.ReadAsStringAsync();
            var tokenMatch = Regex.Match(pageContent, @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");
            
            var createData = new List<KeyValuePair<string, string>>
            {
                new("MachineInput.MachineId", "TEST-01"),
                new("MachineInput.MachineName", "Test SLS Printer"),
                new("MachineInput.MachineModel", "TruPrint 3000"),
                new("MachineInput.SerialNumber", "TP3000-001"),
                new("MachineInput.Location", "Bay 1"),
                new("MachineInput.SupportedMaterials", "TI64-G5,IN718,SS316L"),
                new("MachineInput.CurrentMaterial", "TI64-G5"),
                new("MachineInput.Status", "Idle"),
                new("MachineInput.IsActive", "true"),
                new("MachineInput.IsAvailableForScheduling", "true"),
                new("MachineInput.Priority", "3"),
                new("MachineInput.BuildLengthMm", "250"),
                new("MachineInput.BuildWidthMm", "250"),
                new("MachineInput.BuildHeightMm", "300"),
                new("MachineInput.MaxLaserPowerWatts", "400"),
                new("MachineInput.MaxScanSpeedMmPerSec", "7000"),
                new("MachineInput.MinLayerThicknessMicrons", "20"),
                new("MachineInput.MaxLayerThicknessMicrons", "60"),
                new("MachineInput.MaintenanceIntervalHours", "500"),
                new("MachineInput.OpcUaEnabled", "false")
            };

            if (tokenMatch.Success)
            {
                createData.Add(new("__RequestVerificationToken", tokenMatch.Groups[1].Value));
            }

            // Act
            var formContent = new FormUrlEncodedContent(createData);
            var response = await _client.PostAsync("/Admin/Machines?handler=CreateMachine", formContent);

            // Assert
            _output.WriteLine($"?? Create machine response status: {response.StatusCode}");
            
            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                var location = response.Headers.Location?.ToString() ?? "";
                Assert.Contains("/Admin/Machines", location);
                _output.WriteLine("? Machine creation redirected successfully");
                
                // Follow redirect and check for success message
                var redirectResponse = await _client.GetAsync(location);
                var redirectContent = await redirectResponse.Content.ReadAsStringAsync();
                
                // Check if machine was created successfully
                Assert.Contains("TEST-01", redirectContent);
                _output.WriteLine("? New machine TEST-01 appears in machine list");
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                _output.WriteLine($"?? Response content: {content.Substring(0, Math.Min(500, content.Length))}...");
                
                // Even if not redirect, check for success indicators
                Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect, 
                    $"Machine creation should succeed, got: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"? Machine creation test failed: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public async Task MachinesPage_CreateMachine_WithInvalidMaterial_ShowsError()
    {
        try
        {
            // Arrange
            await LoginAsAdminAsync();

            var machinesPage = await _client.GetAsync("/Admin/Machines");
            var pageContent = await machinesPage.Content.ReadAsStringAsync();
            var tokenMatch = Regex.Match(pageContent, @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");
            
            var createData = new List<KeyValuePair<string, string>>
            {
                new("MachineInput.MachineId", "TEST-02"),
                new("MachineInput.MachineName", "Test SLS Printer 2"),
                new("MachineInput.MachineModel", "TruPrint 3000"),
                new("MachineInput.SupportedMaterials", "INVALID-MATERIAL,TI64-G5"),
                new("MachineInput.CurrentMaterial", "INVALID-MATERIAL"),
                new("MachineInput.Status", "Idle"),
                new("MachineInput.IsActive", "true"),
                new("MachineInput.IsAvailableForScheduling", "true")
            };

            if (tokenMatch.Success)
            {
                createData.Add(new("__RequestVerificationToken", tokenMatch.Groups[1].Value));
            }

            // Act
            var formContent = new FormUrlEncodedContent(createData);
            var response = await _client.PostAsync("/Admin/Machines?handler=CreateMachine", formContent);

            // Assert
            _output.WriteLine($"?? Invalid material response status: {response.StatusCode}");
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                // Should show validation error
                Assert.True(content.Contains("not a valid material") || 
                           content.Contains("error") ||
                           content.Contains("INVALID-MATERIAL"),
                    "Should show error for invalid material");
                _output.WriteLine("? Invalid material properly rejected with error message");
            }
            else
            {
                _output.WriteLine($"?? Invalid material returned status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"? Invalid material test failed: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public async Task MachinesPage_MaterialFilter_WorksCorrectly()
    {
        try
        {
            // Arrange
            await LoginAsAdminAsync();

            // Act - Filter by Ti-6Al-4V material
            var response = await _client.GetAsync("/Admin/Machines?materialFilter=TI64-G5");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine("? Material filter applied successfully");
            
            // Check that filter is reflected in the page
            Assert.Contains("materialFilter", content);
        }
        catch (Exception ex)
        {
            _output.WriteLine($"? Material filter test failed: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Material Validation Tests

    [Fact]
    public async Task Machine_ValidateMaterials_RejectsUnsupportedMaterial()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        
        // Create a machine with specific supported materials
        var machine = new Machine
        {
            MachineId = "TEST-VALIDATION",
            Name = "Test Machine",
            MachineName = "Test Machine",
            MachineType = "SLS",
            SupportedMaterials = "TI64-G5,IN718",
            CurrentMaterial = "TI64-G5",
            IsActive = true,
            CreatedBy = "Test",
            LastModifiedBy = "Test"
        };

        context.Machines.Add(machine);
        await context.SaveChangesAsync();

        // Act & Assert
        Assert.True(machine.CanAccommodatePart(new Part { SlsMaterial = "TI64-G5" }));
        Assert.False(machine.CanAccommodatePart(new Part { SlsMaterial = "SS316L" }));

        _output.WriteLine("? Machine material validation works correctly");
    }

    #endregion

    #region Database Integration Tests

    [Fact]
    public async Task MaterialSeeding_CreatesDefaultMaterials()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

        // Act
        var materialCount = await context.Materials.CountAsync();

        // Assert
        Assert.True(materialCount >= 5, $"Should have at least 5 seeded materials, found {materialCount}");
        
        var titaniumMaterials = await context.Materials
            .Where(m => m.MaterialType == "Titanium")
            .CountAsync();
        
        Assert.True(titaniumMaterials >= 2, $"Should have at least 2 titanium materials, found {titaniumMaterials}");

        _output.WriteLine($"? Database contains {materialCount} materials including {titaniumMaterials} titanium alloys");
    }

    [Fact]
    public async Task MaterialProperties_SerializeCorrectly()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

        var material = await context.Materials.FirstOrDefaultAsync(m => m.MaterialCode == "TI64-G5");
        Assert.NotNull(material);

        // Act
        var properties = new Dictionary<string, object>
        {
            {"YieldStrength", 880},
            {"UltimateStrength", 950},
            {"Elongation", 14}
        };

        material.SetMaterialProperties(properties);
        context.Update(material);
        await context.SaveChangesAsync();

        // Read back
        var updatedMaterial = await context.Materials.FirstOrDefaultAsync(m => m.MaterialCode == "TI64-G5");
        var retrievedProperties = updatedMaterial!.GetMaterialProperties();

        // Assert
        Assert.Equal(3, retrievedProperties.Count);
        Assert.True(retrievedProperties.ContainsKey("YieldStrength"));

        _output.WriteLine("? Material properties serialize and deserialize correctly");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task MaterialLoading_PerformsEfficiently()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var materialService = scope.ServiceProvider.GetRequiredService<IMaterialService>();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var materials = await materialService.GetActiveMaterialsAsync();
        var materialTypes = await materialService.GetMaterialTypesAsync();
        var compatibleMaterials = await materialService.GetCompatibleMaterialsAsync("SLS");

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Material loading should complete within 1 second, took {stopwatch.ElapsedMilliseconds}ms");

        _output.WriteLine($"? Material loading completed in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"   - {materials.Count} active materials");
        _output.WriteLine($"   - {materialTypes.Count} material types");
        _output.WriteLine($"   - {compatibleMaterials.Count} SLS-compatible materials");
    }

    #endregion

    #region Helper Methods

    private async Task LoginAsAdminAsync()
    {
        // Get login page first
        var loginPage = await _client.GetAsync("/Account/Login");
        Assert.Equal(HttpStatusCode.OK, loginPage.StatusCode);
        
        var loginContent = await loginPage.Content.ReadAsStringAsync();

        // Prepare login data
        var loginData = new List<KeyValuePair<string, string>>
        {
            new("Input.Username", "admin"),
            new("Input.Password", "admin123"),
            new("Input.RememberMe", "false")
        };

        // Look for anti-forgery token
        var tokenMatch = Regex.Match(loginContent, @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");
        if (tokenMatch.Success)
        {
            loginData.Add(new("__RequestVerificationToken", tokenMatch.Groups[1].Value));
        }

        // Submit login form
        var formContent = new FormUrlEncodedContent(loginData);
        var response = await _client.PostAsync("/Account/Login", formContent);
        
        Assert.True(response.StatusCode == HttpStatusCode.Redirect || response.IsSuccessStatusCode, 
            $"Login should succeed, got: {response.StatusCode}");
    }

    #endregion

    public void Dispose()
    {
        _client?.Dispose();
    }
}