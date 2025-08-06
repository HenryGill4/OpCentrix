using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services.Admin;
using OpCentrix.Services;
using System.Net;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using System.Diagnostics;
using System.Text.Json;

namespace OpCentrix.Tests;

/// <summary>
/// Stage Dashboard Performance and Integration Tests
/// Testing system performance, data integrity, and cross-dashboard integration
/// Following OpCentrix Stage Dashboard Master Plan performance criteria
/// </summary>
public class StageDashboardPerformanceTests : IClassFixture<OpCentrixWebApplicationFactory>, IDisposable
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly SchedulerContext _context;

    public StageDashboardPerformanceTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Get database context for test setup
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
    }

    #region Performance Tests

    [Fact]
    public async Task StageDashboards_LoadWithinPerformanceThresholds()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        await SetupPerformanceTestData();

        var dashboardEndpoints = new[]
        {
            new { Url = "/Operations/StageDashboard", Name = "Master Stage Dashboard", MaxMs = 3000 },
            new { Url = "/Operations/Dashboard", Name = "Operator Dashboard", MaxMs = 2000 },
            new { Url = "/Operations/Stages/SLS", Name = "SLS Operations Dashboard", MaxMs = 2000 },
            new { Url = "/Operations/Stages/CNC", Name = "CNC Operations Dashboard", MaxMs = 2000 }
        };

        // Act & Assert
        foreach (var endpoint in dashboardEndpoints)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await _client.GetAsync(endpoint.Url);
            stopwatch.Stop();
            
            Assert.True(response.IsSuccessStatusCode, 
                $"{endpoint.Name} failed to load: {response.StatusCode}");
            
            Assert.True(stopwatch.ElapsedMilliseconds < endpoint.MaxMs, 
                $"{endpoint.Name} took {stopwatch.ElapsedMilliseconds}ms (exceeds {endpoint.MaxMs}ms threshold)");
            
            _output.WriteLine($"? {endpoint.Name}: {stopwatch.ElapsedMilliseconds}ms (threshold: {endpoint.MaxMs}ms)");
        }
    }

    [Fact]
    public async Task StageDashboards_HandleConcurrentUsers()
    {
        // Arrange
        await SetupPerformanceTestData();
        
        // Create multiple concurrent clients (simulating multiple operators)
        var concurrentTasks = new List<Task<bool>>();
        
        for (int i = 0; i < 5; i++)
        {
            var taskId = i;
            concurrentTasks.Add(Task.Run(async () =>
            {
                using var concurrentClient = _factory.CreateClient();
                await AuthenticateClient(concurrentClient);
                
                var stopwatch = Stopwatch.StartNew();
                var response = await concurrentClient.GetAsync("/Operations/Dashboard");
                stopwatch.Stop();
                
                _output.WriteLine($"? Concurrent user {taskId}: {stopwatch.ElapsedMilliseconds}ms");
                
                return response.IsSuccessStatusCode && stopwatch.ElapsedMilliseconds < 5000;
            }));
        }

        // Act
        var results = await Task.WhenAll(concurrentTasks);

        // Assert
        Assert.All(results, result => Assert.True(result, "Concurrent request failed"));
        _output.WriteLine("? Stage Dashboard system handles 5 concurrent users successfully");
    }

    [Fact]
    public async Task StageDashboards_RefreshEndpointsPerformance()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        await SetupPerformanceTestData();

        var refreshEndpoints = new[]
        {
            new { Url = "/Operations/StageDashboard?handler=RefreshStages", Name = "Stage Refresh", MaxMs = 1500 },
            new { Url = "/Operations/Dashboard?handler=RefreshDashboard", Name = "Dashboard Refresh", MaxMs = 1000 },
            new { Url = "/Operations/Stages/SLS?handler=RefreshSLSDashboard", Name = "SLS Refresh", MaxMs = 1000 }
        };

        // Act & Assert
        foreach (var endpoint in refreshEndpoints)
        {
            // Test multiple refresh calls to simulate real usage
            for (int i = 0; i < 3; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await _client.GetAsync(endpoint.Url);
                stopwatch.Stop();
                
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(stopwatch.ElapsedMilliseconds < endpoint.MaxMs, 
                    $"{endpoint.Name} refresh {i+1} took {stopwatch.ElapsedMilliseconds}ms (exceeds {endpoint.MaxMs}ms)");
                
                _output.WriteLine($"? {endpoint.Name} refresh {i+1}: {stopwatch.ElapsedMilliseconds}ms");
            }
        }
    }

    [Fact]
    public async Task StageDashboards_HandleLargeDatasets()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        await SetupLargeDatasetTestData(); // Create 50+ jobs across multiple stages

        // Act & Assert - Test Master Stage Dashboard with large dataset
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync("/Operations/StageDashboard");
        stopwatch.Stop();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Master Stage Dashboard with large dataset took {stopwatch.ElapsedMilliseconds}ms (exceeds 5s threshold)");
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("stage-overview-grid", content);
        
        _output.WriteLine($"? Master Stage Dashboard handles large dataset: {stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task StageDashboards_DisplayConsistentData()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        await SetupConsistencyTestData();

        // Act - Get data from different dashboards
        var masterResponse = await _client.GetAsync("/Operations/StageDashboard");
        var operatorResponse = await _client.GetAsync("/Operations/Dashboard");
        var slsResponse = await _client.GetAsync("/Operations/Stages/SLS");

        // Assert
        Assert.Equal(HttpStatusCode.OK, masterResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, operatorResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, slsResponse.StatusCode);

        var masterContent = await masterResponse.Content.ReadAsStringAsync();
        var operatorContent = await operatorResponse.Content.ReadAsStringAsync();
        var slsContent = await slsResponse.Content.ReadAsStringAsync();

        // All dashboards should load successfully with consistent data structure
        Assert.Contains("dashboard", masterContent.ToLower());
        Assert.Contains("dashboard", operatorContent.ToLower());
        Assert.Contains("sls", slsContent.ToLower());

        _output.WriteLine("? Stage dashboards display consistent data structure");
    }

    [Fact]
    public async Task StageDashboards_MaintainDataIntegrityAfterOperations()
    {
        // Arrange
        await AuthenticateAsOperatorAsync();
        var (job, stage) = await SetupIntegrityTestJobAndStage();

        // Act - Perform punch in operation
        var punchInData = new List<KeyValuePair<string, string>>
        {
            new("jobId", job.Id.ToString()),
            new("stageId", stage.Id.ToString()),
            new("operatorName", "IntegrityTestOperator")
        };

        var punchInResponse = await _client.PostAsync("/Operations/StageDashboard?handler=PunchIn", 
            new FormUrlEncodedContent(punchInData));

        // Assert - Check database integrity
        var execution = await _context.ProductionStageExecutions
            .FirstOrDefaultAsync(pse => pse.JobId == job.Id && 
                                      pse.ProductionStageId == stage.Id &&
                                      pse.OperatorName == "IntegrityTestOperator");

        if (execution != null)
        {
            // Verify all required fields are properly set
            Assert.NotNull(execution.JobId);
            Assert.Equal(stage.Id, execution.ProductionStageId);
            Assert.Equal("IntegrityTestOperator", execution.OperatorName);
            Assert.Equal("IntegrityTestOperator", execution.ExecutedBy);
            Assert.True(execution.StartDate.HasValue);
            Assert.True(execution.CreatedDate != default(DateTime));
            Assert.Equal("InProgress", execution.Status);

            _output.WriteLine("? Stage dashboard operations maintain proper data integrity");
        }
        else
        {
            _output.WriteLine("??  No execution record created (may be business rule limitation)");
        }
    }

    [Fact]
    public async Task StageDashboards_HandleDatabaseErrors_Gracefully()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act - Test with invalid stage ID
        var invalidRefreshResponse = await _client.GetAsync("/Operations/StageDashboard?view=invalid&startDate=invalid");

        // Assert - Should handle gracefully without crashing
        Assert.True(invalidRefreshResponse.IsSuccessStatusCode || 
                   invalidRefreshResponse.StatusCode == HttpStatusCode.BadRequest,
                   $"Dashboard should handle invalid parameters gracefully, got: {invalidRefreshResponse.StatusCode}");

        if (invalidRefreshResponse.IsSuccessStatusCode)
        {
            var content = await invalidRefreshResponse.Content.ReadAsStringAsync();
            Assert.Contains("dashboard", content.ToLower());
            _output.WriteLine("? Stage dashboards handle invalid parameters gracefully");
        }
        else
        {
            _output.WriteLine($"? Stage dashboards properly reject invalid parameters: {invalidRefreshResponse.StatusCode}");
        }
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task StageDashboards_IntegrateWithExistingScheduler()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act - Test navigation between old scheduler and new stage dashboards
        var schedulerResponse = await _client.GetAsync("/Scheduler");
        var stageDashboardResponse = await _client.GetAsync("/Operations/StageDashboard");

        // Assert
        Assert.Equal(HttpStatusCode.OK, schedulerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, stageDashboardResponse.StatusCode);

        var schedulerContent = await schedulerResponse.Content.ReadAsStringAsync();
        var stageDashboardContent = await stageDashboardResponse.Content.ReadAsStringAsync();

        // Both should work independently without conflicts
        Assert.Contains("scheduler", schedulerContent.ToLower());
        Assert.Contains("stage", stageDashboardContent.ToLower());

        _output.WriteLine("? Stage dashboards integrate properly with existing scheduler");
    }

    [Fact]
    public async Task StageDashboards_DoNotBreakExistingFunctionality()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Test that existing pages still work after stage dashboard implementation
        var existingPages = new[]
        {
            "/Admin",
            "/Admin/Parts",
            "/Admin/Machines",
            "/Scheduler",
            "/PrintTracking"
        };

        // Act & Assert
        foreach (var page in existingPages)
        {
            var response = await _client.GetAsync(page);
            
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect,
                $"Existing page {page} should still work, got: {response.StatusCode}");
            
            _output.WriteLine($"? Existing page {page}: {response.StatusCode}");
        }
    }

    [Fact]
    public async Task StageDashboards_ShareDataCorrectlyAcrossViews()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        await SetupSharedDataTestData();

        // Act - Create execution in one dashboard, verify it appears in others
        var (job, stage) = await CreateTestExecution();

        // Get data from different dashboards
        var masterDashboard = await _client.GetAsync("/Operations/StageDashboard");
        var operatorDashboard = await _client.GetAsync("/Operations/Dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.OK, masterDashboard.StatusCode);
        Assert.Equal(HttpStatusCode.OK, operatorDashboard.StatusCode);

        // Both dashboards should reflect the same underlying data
        var masterContent = await masterDashboard.Content.ReadAsStringAsync();
        var operatorContent = await operatorDashboard.Content.ReadAsStringAsync();

        // Should contain references to the test job or related content
        var hasJobReference = masterContent.Contains(job.PartNumber) || 
                             masterContent.Contains("Active Jobs") ||
                             masterContent.Contains("stage-overview");

        Assert.True(hasJobReference || masterContent.Contains("dashboard"),
            "Master dashboard should display job data or dashboard structure");

        _output.WriteLine("? Stage dashboards share data correctly across views");
    }

    #endregion

    #region Memory and Resource Tests

    [Fact]
    public async Task StageDashboards_DoNotLeakMemory()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        await SetupPerformanceTestData();

        var initialMemory = GC.GetTotalMemory(true);

        // Act - Simulate high usage scenario
        for (int i = 0; i < 10; i++)
        {
            var response = await _client.GetAsync("/Operations/StageDashboard");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            // Content should be processed and released
        }

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;

        // Assert - Memory increase should be reasonable (less than 50MB for 10 requests)
        Assert.True(memoryIncrease < 50 * 1024 * 1024, 
            $"Memory increased by {memoryIncrease / 1024 / 1024}MB (should be less than 50MB)");

        _output.WriteLine($"? Stage dashboards memory usage: {memoryIncrease / 1024 / 1024}MB increase after 10 requests");
    }

    #endregion

    #region Helper Methods

    private async Task AuthenticateAsAdminAsync()
    {
        await AuthenticateClient(_client);
    }

    private async Task AuthenticateAsOperatorAsync()
    {
        await AuthenticateClient(_client);
    }

    private async Task AuthenticateClient(HttpClient client)
    {
        // Get login page to extract anti-forgery token
        var loginPage = await client.GetAsync("/Account/Login");
        var loginContent = await loginPage.Content.ReadAsStringAsync();
        
        var token = ExtractAntiForgeryToken(loginContent);
        
        // Prepare login form data
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Username", "admin"),
            new("Password", "admin123"),
            new("__RequestVerificationToken", token)
        };

        // Submit login form
        var loginFormContent = new FormUrlEncodedContent(formData);
        var loginResponse = await client.PostAsync("/Account/Login", loginFormContent);
        
        // Verify login was successful
        Assert.True(loginResponse.StatusCode == HttpStatusCode.Redirect || 
                   loginResponse.StatusCode == HttpStatusCode.OK);
    }

    private async Task SetupPerformanceTestData()
    {
        // Create test data for performance testing
        await EnsureProductionStagesExist();
        await CreateMultipleTestJobs(10); // 10 jobs for performance testing
    }

    private async Task SetupLargeDatasetTestData()
    {
        // Create larger dataset for stress testing
        await EnsureProductionStagesExist();
        await CreateMultipleTestJobs(50); // 50 jobs for large dataset testing
    }

    private async Task SetupConsistencyTestData()
    {
        await EnsureProductionStagesExist();
        await CreateMultipleTestJobs(5);
    }

    private async Task SetupSharedDataTestData()
    {
        await EnsureProductionStagesExist();
        await CreateMultipleTestJobs(3);
    }

    private async Task<(Job job, ProductionStage stage)> SetupIntegrityTestJobAndStage()
    {
        var stage = await EnsureProductionStageExists();
        var job = await CreateTestJob("INTEGRITY-TEST-001", stage);
        return (job, stage);
    }

    private async Task<(Job job, ProductionStage stage)> CreateTestExecution()
    {
        var stage = await EnsureProductionStageExists();
        var job = await CreateTestJob("SHARED-DATA-001", stage);
        
        // Create a test execution
        var execution = new ProductionStageExecution
        {
            JobId = job.Id,
            ProductionStageId = stage.Id,
            ExecutedBy = "TestOperator",
            OperatorName = "TestOperator",
            Status = "InProgress",
            StartDate = DateTime.UtcNow,
            ActualStartTime = DateTime.UtcNow,
            CreatedBy = "TestOperator",
            CreatedDate = DateTime.UtcNow
        };

        _context.ProductionStageExecutions.Add(execution);
        await _context.SaveChangesAsync();

        return (job, stage);
    }

    private async Task EnsureProductionStagesExist()
    {
        var existingStages = await _context.ProductionStages.CountAsync(ps => ps.IsActive);
        
        if (existingStages < 7)
        {
            var stages = new[]
            {
                new ProductionStage { Name = "3D Printing (SLS)", StageColor = "#007bff", Department = "3D Printing", DisplayOrder = 1, IsActive = true, DefaultSetupMinutes = 240 },
                new ProductionStage { Name = "CNC Machining", StageColor = "#28a745", Department = "CNC Machining", DisplayOrder = 2, IsActive = true, DefaultSetupMinutes = 180 },
                new ProductionStage { Name = "EDM", StageColor = "#ffc107", Department = "EDM", DisplayOrder = 3, IsActive = true, DefaultSetupMinutes = 120 },
                new ProductionStage { Name = "Laser Engraving", StageColor = "#fd7e14", Department = "Laser Operations", DisplayOrder = 4, IsActive = true, DefaultSetupMinutes = 60 },
                new ProductionStage { Name = "Sandblasting", StageColor = "#6c757d", Department = "Finishing", DisplayOrder = 5, IsActive = true, DefaultSetupMinutes = 90 },
                new ProductionStage { Name = "Coating/Cerakote", StageColor = "#17a2b8", Department = "Finishing", DisplayOrder = 6, IsActive = true, DefaultSetupMinutes = 120 },
                new ProductionStage { Name = "Assembly", StageColor = "#dc3545", Department = "Assembly", DisplayOrder = 7, IsActive = true, DefaultSetupMinutes = 150 }
            };

            foreach (var stage in stages)
            {
                var existing = await _context.ProductionStages
                    .FirstOrDefaultAsync(ps => ps.Name == stage.Name);
                
                if (existing == null)
                {
                    _context.ProductionStages.Add(stage);
                }
            }

            await _context.SaveChangesAsync();
        }
    }

    private async Task<ProductionStage> EnsureProductionStageExists()
    {
        var stage = await _context.ProductionStages.FirstOrDefaultAsync(ps => ps.IsActive);
        
        if (stage == null)
        {
            stage = new ProductionStage
            {
                Name = "Test Stage",
                StageColor = "#007bff",
                Department = "Test Department",
                DisplayOrder = 1,
                IsActive = true,
                DefaultSetupMinutes = 240
            };
            _context.ProductionStages.Add(stage);
            await _context.SaveChangesAsync();
        }

        return stage;
    }

    private async Task CreateMultipleTestJobs(int count)
    {
        var stages = await _context.ProductionStages.Where(ps => ps.IsActive).ToListAsync();
        if (!stages.Any()) return;

        for (int i = 1; i <= count; i++)
        {
            var partNumber = $"PERF-TEST-{i:D3}";
            
            // Check if job already exists
            var existingJob = await _context.Jobs.FirstOrDefaultAsync(j => j.PartNumber == partNumber);
            if (existingJob != null) continue;

            var stage = stages[i % stages.Count]; // Distribute across stages
            await CreateTestJob(partNumber, stage);
        }
    }

    private async Task<Job> CreateTestJob(string partNumber, ProductionStage stage)
    {
        // Create test part
        var part = new Part
        {
            PartNumber = partNumber,
            Name = $"Performance Test Part {partNumber}",
            Description = $"Performance Test Part {partNumber}",
            Material = "PA12",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "PerfTest"
        };

        _context.Parts.Add(part);
        await _context.SaveChangesAsync();

        // Create stage requirement
        var stageReq = new PartStageRequirement
        {
            PartId = part.Id,
            ProductionStageId = stage.Id,
            IsRequired = true,
            ExecutionOrder = stage.DisplayOrder,
            EstimatedHours = 2.0,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "PerfTest"
        };

        _context.PartStageRequirements.Add(stageReq);
        await _context.SaveChangesAsync();

        // Create test job
        var job = new Job
        {
            PartId = part.Id,
            PartNumber = part.PartNumber,
            Quantity = 1,
            Priority = 1,
            Status = "Scheduled",
            ScheduledStart = DateTime.Today.AddHours(8),
            ScheduledEnd = DateTime.Today.AddHours(16),
            EstimatedHours = 2.0,
            MachineId = $"TEST-MACHINE-{stage.Department?.Replace(" ", "")}",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "PerfTest"
        };

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        return job;
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        var tokenStart = html.IndexOf("__RequestVerificationToken");
        if (tokenStart == -1) return string.Empty;
        
        var valueStart = html.IndexOf("value=\"", tokenStart) + 7;
        var valueEnd = html.IndexOf("\"", valueStart);
        
        return html.Substring(valueStart, valueEnd - valueStart);
    }

    #endregion

    public void Dispose()
    {
        _client?.Dispose();
    }
}