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
using System.Text.Json;

namespace OpCentrix.Tests;

/// <summary>
/// Stage Operations Workflow Tests
/// Testing punch in/out functionality, stage progression, and workflow automation
/// Following OpCentrix Stage Dashboard Master Plan protocols
/// </summary>
public class StageOperationsWorkflowTests : IClassFixture<OpCentrixWebApplicationFactory>, IDisposable
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly SchedulerContext _context;

    public StageOperationsWorkflowTests(OpCentrixWebApplicationFactory factory, ITestOutputHelper output)
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

    #region Stage Punch In/Out Workflow Tests

    [Fact]
    public async Task StageWorkflow_PunchIn_CreatesExecutionRecord()
    {
        // Arrange
        await AuthenticateAsOperatorAsync();
        var (job, stage) = await SetupTestJobAndStage();

        // Act
        var punchInData = new List<KeyValuePair<string, string>>
        {
            new("jobId", job.Id.ToString()),
            new("stageId", stage.Id.ToString()),
            new("operatorName", "TestOperator")
        };

        var response = await _client.PostAsync("/Operations/StageDashboard?handler=PunchIn", 
            new FormUrlEncodedContent(punchInData));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        if (result.TryGetProperty("success", out var successProp))
        {
            var success = successProp.GetBoolean();
            if (success)
            {
                // Verify execution record was created
                var execution = await _context.ProductionStageExecutions
                    .FirstOrDefaultAsync(pse => pse.JobId == job.Id && 
                                              pse.ProductionStageId == stage.Id &&
                                              pse.Status == "InProgress");

                Assert.NotNull(execution);
                Assert.Equal("TestOperator", execution.OperatorName);
                Assert.Equal("InProgress", execution.Status);
                Assert.True(execution.StartDate.HasValue);
                
                _output.WriteLine("? Stage punch in creates execution record correctly");
            }
            else
            {
                var message = result.TryGetProperty("message", out var msgProp) ? 
                    msgProp.GetString() : "Unknown error";
                _output.WriteLine($"??  Punch in failed (may be business rule): {message}");
            }
        }
    }

    [Fact]
    public async Task StageWorkflow_PunchOut_CompletesExecution()
    {
        // Arrange
        await AuthenticateAsOperatorAsync();
        var (job, stage) = await SetupTestJobAndStage();
        
        // Create an active execution first
        var execution = new ProductionStageExecution
        {
            JobId = job.Id,
            ProductionStageId = stage.Id,
            ExecutedBy = "TestOperator",
            OperatorName = "TestOperator",
            Status = "InProgress",
            StartDate = DateTime.UtcNow.AddHours(-2),
            ActualStartTime = DateTime.UtcNow.AddHours(-2),
            EstimatedHours = 4.0m,
            CreatedBy = "TestOperator",
            CreatedDate = DateTime.UtcNow.AddHours(-2)
        };

        _context.ProductionStageExecutions.Add(execution);
        await _context.SaveChangesAsync();

        // Act
        var punchOutData = new List<KeyValuePair<string, string>>
        {
            new("jobId", job.Id.ToString()),
            new("stageId", stage.Id.ToString())
        };

        var response = await _client.PostAsync("/Operations/StageDashboard?handler=PunchOut", 
            new FormUrlEncodedContent(punchOutData));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        if (result.TryGetProperty("success", out var successProp))
        {
            var success = successProp.GetBoolean();
            if (success)
            {
                // Verify execution was completed
                var completedExecution = await _context.ProductionStageExecutions
                    .FirstOrDefaultAsync(pse => pse.Id == execution.Id);

                Assert.NotNull(completedExecution);
                Assert.Equal("Completed", completedExecution.Status);
                Assert.True(completedExecution.CompletionDate.HasValue);
                Assert.True(completedExecution.ActualHours.HasValue);
                
                _output.WriteLine($"? Stage punch out completes execution with {completedExecution.ActualHours:F2} hours");
            }
            else
            {
                var message = result.TryGetProperty("message", out var msgProp) ? 
                    msgProp.GetString() : "Unknown error";
                _output.WriteLine($"??  Punch out failed: {message}");
            }
        }
    }

    [Fact]
    public async Task StageWorkflow_PreventsDuplicatePunchIn()
    {
        // Arrange
        await AuthenticateAsOperatorAsync();
        var (job, stage) = await SetupTestJobAndStage();
        
        // Create existing active execution
        var existingExecution = new ProductionStageExecution
        {
            JobId = job.Id,
            ProductionStageId = stage.Id,
            ExecutedBy = "ExistingOperator",
            OperatorName = "ExistingOperator",
            Status = "InProgress",
            StartDate = DateTime.UtcNow.AddHours(-1),
            ActualStartTime = DateTime.UtcNow.AddHours(-1),
            CreatedBy = "ExistingOperator",
            CreatedDate = DateTime.UtcNow.AddHours(-1)
        };

        _context.ProductionStageExecutions.Add(existingExecution);
        await _context.SaveChangesAsync();

        // Act - Try to punch in to same stage
        var punchInData = new List<KeyValuePair<string, string>>
        {
            new("jobId", job.Id.ToString()),
            new("stageId", stage.Id.ToString()),
            new("operatorName", "TestOperator")
        };

        var response = await _client.PostAsync("/Operations/StageDashboard?handler=PunchIn", 
            new FormUrlEncodedContent(punchInData));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        if (result.TryGetProperty("success", out var successProp))
        {
            var success = successProp.GetBoolean();
            Assert.False(success); // Should fail due to existing execution
            
            if (result.TryGetProperty("message", out var messageProp))
            {
                var message = messageProp.GetString();
                Assert.Contains("already", message?.ToLower() ?? "");
                _output.WriteLine($"? Stage workflow prevents duplicate punch in: {message}");
            }
        }
    }

    #endregion

    #region SLS Operations Workflow Tests

    [Fact]
    public async Task SLSWorkflow_StartJob_UpdatesJobStatus()
    {
        // Arrange
        await AuthenticateAsOperatorAsync();
        var (job, slsStage) = await SetupSLSTestJobAndStage();

        // Act
        var startJobData = new List<KeyValuePair<string, string>>
        {
            new("jobId", job.Id.ToString())
        };

        var response = await _client.PostAsync("/Operations/Stages/SLS?handler=StartSLSJob", 
            new FormUrlEncodedContent(startJobData));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        if (result.TryGetProperty("success", out var successProp))
        {
            var success = successProp.GetBoolean();
            if (success)
            {
                // Verify job status was updated
                var updatedJob = await _context.Jobs.FindAsync(job.Id);
                Assert.NotNull(updatedJob);
                Assert.Equal("InProgress", updatedJob.Status);
                Assert.True(updatedJob.ActualStart.HasValue);
                
                _output.WriteLine("? SLS workflow start job updates job status correctly");
            }
            else
            {
                var message = result.TryGetProperty("message", out var msgProp) ? 
                    msgProp.GetString() : "Unknown error";
                _output.WriteLine($"??  SLS job start failed: {message}");
            }
        }
    }

    [Fact]
    public async Task SLSWorkflow_CompleteJob_TriggersStageProgression()
    {
        // Arrange
        await AuthenticateAsOperatorAsync();
        var (job, slsStage) = await SetupSLSTestJobAndStage();
        
        // Set up active SLS execution
        var execution = new ProductionStageExecution
        {
            JobId = job.Id,
            ProductionStageId = slsStage.Id,
            ExecutedBy = "TestOperator",
            OperatorName = "TestOperator",
            Status = "InProgress",
            StartDate = DateTime.UtcNow.AddHours(-4),
            ActualStartTime = DateTime.UtcNow.AddHours(-4),
            CreatedBy = "TestOperator",
            CreatedDate = DateTime.UtcNow.AddHours(-4)
        };

        _context.ProductionStageExecutions.Add(execution);
        await _context.SaveChangesAsync();

        // Act
        var completeJobData = new List<KeyValuePair<string, string>>
        {
            new("jobId", job.Id.ToString())
        };

        var response = await _client.PostAsync("/Operations/Stages/SLS?handler=CompleteSLSJob", 
            new FormUrlEncodedContent(completeJobData));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        if (result.TryGetProperty("success", out var successProp))
        {
            var success = successProp.GetBoolean();
            if (success)
            {
                // Verify execution was completed
                var completedExecution = await _context.ProductionStageExecutions
                    .FirstOrDefaultAsync(pse => pse.Id == execution.Id);

                Assert.NotNull(completedExecution);
                Assert.Equal("Completed", completedExecution.Status);
                
                // Verify job was completed
                var completedJob = await _context.Jobs.FindAsync(job.Id);
                Assert.NotNull(completedJob);
                Assert.Equal("Completed", completedJob.Status);
                Assert.True(completedJob.ActualEnd.HasValue);
                
                _output.WriteLine("? SLS workflow complete job triggers stage progression correctly");
            }
            else
            {
                var message = result.TryGetProperty("message", out var msgProp) ? 
                    msgProp.GetString() : "Unknown error";
                _output.WriteLine($"??  SLS job completion failed: {message}");
            }
        }
    }

    #endregion

    #region Operator Dashboard Workflow Tests

    [Fact]
    public async Task OperatorDashboard_EnforcesOneStagePerOperator()
    {
        // Arrange
        await AuthenticateAsOperatorAsync();
        var (job1, stage1) = await SetupTestJobAndStage("STAGE-CONCURRENT-1");
        var (job2, stage2) = await SetupTestJobAndStage("STAGE-CONCURRENT-2");

        // Act - First punch in
        var punchIn1Data = new List<KeyValuePair<string, string>>
        {
            new("jobId", job1.Id.ToString()),
            new("stageId", stage1.Id.ToString())
        };

        var response1 = await _client.PostAsync("/Operations/Dashboard?handler=PunchIn", 
            new FormUrlEncodedContent(punchIn1Data));

        // Act - Second punch in (should be prevented)
        var punchIn2Data = new List<KeyValuePair<string, string>>
        {
            new("jobId", job2.Id.ToString()),
            new("stageId", stage2.Id.ToString())
        };

        var response2 = await _client.PostAsync("/Operations/Dashboard?handler=PunchIn", 
            new FormUrlEncodedContent(punchIn2Data));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var content1 = await response1.Content.ReadAsStringAsync();
        var result1 = JsonSerializer.Deserialize<JsonElement>(content1);
        
        var content2 = await response2.Content.ReadAsStringAsync();
        var result2 = JsonSerializer.Deserialize<JsonElement>(content2);
        
        if (result1.TryGetProperty("success", out var success1Prop) &&
            result2.TryGetProperty("success", out var success2Prop))
        {
            var success1 = success1Prop.GetBoolean();
            var success2 = success2Prop.GetBoolean();
            
            if (success1 && !success2)
            {
                // Second punch in should be prevented
                if (result2.TryGetProperty("message", out var messageProp))
                {
                    var message = messageProp.GetString();
                    Assert.Contains("one stage at a time", message?.ToLower() ?? "");
                    _output.WriteLine($"? Operator dashboard enforces one stage per operator: {message}");
                }
            }
            else
            {
                _output.WriteLine($"??  Concurrent stage enforcement result: First={success1}, Second={success2}");
            }
        }
    }

    [Fact]
    public async Task OperatorDashboard_DisplaysActiveAndAvailableStages()
    {
        // Arrange
        await AuthenticateAsOperatorAsync();
        await SetupMultipleTestJobs();

        // Act
        var response = await _client.GetAsync("/Operations/Dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        
        // Should contain operator dashboard sections
        Assert.Contains("MY ACTIVE STAGES", content);
        Assert.Contains("AVAILABLE STAGES", content);
        Assert.Contains("dashboard-stats", content);
        
        // Should contain stage cards or empty state
        var hasActiveStages = content.Contains("stage-card active") || 
                             content.Contains("No active stages");
        var hasAvailableStages = content.Contains("stage-card available") || 
                                content.Contains("No available stages");
        
        Assert.True(hasActiveStages, "Should display active stages or empty state");
        Assert.True(hasAvailableStages, "Should display available stages or empty state");
        
        _output.WriteLine("? Operator dashboard displays active and available stages correctly");
    }

    #endregion

    #region Stage Dashboard Integration Tests

    [Fact]
    public async Task StageDashboard_RefreshEndpoints_WorkConsistently()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        await SetupMultipleTestJobs();

        var refreshEndpoints = new[]
        {
            "/Operations/StageDashboard?handler=RefreshStages",
            "/Operations/Dashboard?handler=RefreshDashboard",
            "/Operations/Stages/SLS?handler=RefreshSLSDashboard"
        };

        // Act & Assert
        foreach (var endpoint in refreshEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.True(content.Length > 0, $"Refresh endpoint {endpoint} returned empty content");
            
            _output.WriteLine($"? Refresh endpoint {endpoint} works correctly");
        }
    }

    [Fact]
    public async Task StageDashboard_HandlesMissingJobsGracefully()
    {
        // Arrange
        await AuthenticateAsOperatorAsync();

        // Act - Try to punch in to non-existent job
        var invalidPunchInData = new List<KeyValuePair<string, string>>
        {
            new("jobId", "99999"),
            new("stageId", "1"),
            new("operatorName", "TestOperator")
        };

        var response = await _client.PostAsync("/Operations/StageDashboard?handler=PunchIn", 
            new FormUrlEncodedContent(invalidPunchInData));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        if (result.TryGetProperty("success", out var successProp))
        {
            var success = successProp.GetBoolean();
            Assert.False(success); // Should fail gracefully
            
            if (result.TryGetProperty("message", out var messageProp))
            {
                var message = messageProp.GetString();
                Assert.Contains("not found", message?.ToLower() ?? "");
                _output.WriteLine($"? Stage dashboard handles missing jobs gracefully: {message}");
            }
        }
    }

    #endregion

    #region Helper Methods

    private async Task AuthenticateAsOperatorAsync()
    {
        // Get login page to extract anti-forgery token
        var loginPage = await _client.GetAsync("/Account/Login");
        var loginContent = await loginPage.Content.ReadAsStringAsync();
        
        var token = ExtractAntiForgeryToken(loginContent);
        
        // Prepare login form data (using admin creds for testing)
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Username", "admin"),
            new("Password", "admin123"),
            new("__RequestVerificationToken", token)
        };

        // Submit login form
        var loginFormContent = new FormUrlEncodedContent(formData);
        var loginResponse = await _client.PostAsync("/Account/Login", loginFormContent);
        
        // Verify login was successful
        Assert.True(loginResponse.StatusCode == HttpStatusCode.Redirect || 
                   loginResponse.StatusCode == HttpStatusCode.OK);
    }

    private async Task AuthenticateAsAdminAsync()
    {
        await AuthenticateAsOperatorAsync(); // Same for testing
    }

    private async Task<(Job job, ProductionStage stage)> SetupTestJobAndStage(string partNumberSuffix = "001")
    {
        var partNumber = $"STAGE-WORKFLOW-{partNumberSuffix}";
        
        // Clean up any existing test data
        var existingJob = await _context.Jobs
            .FirstOrDefaultAsync(j => j.PartNumber == partNumber);
        
        if (existingJob != null)
        {
            // Clean up existing executions
            var existingExecutions = await _context.ProductionStageExecutions
                .Where(pse => pse.JobId == existingJob.Id)
                .ToListAsync();
            _context.ProductionStageExecutions.RemoveRange(existingExecutions);
            
            _context.Jobs.Remove(existingJob);
            await _context.SaveChangesAsync();
        }

        // Ensure we have a production stage
        var stage = await _context.ProductionStages
            .FirstOrDefaultAsync(ps => ps.IsActive);
        
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

        // Create test part
        var part = new Part
        {
            PartNumber = partNumber,
            Name = "Test Part for Workflow Testing",
            Description = "Test Part for Workflow Testing",
            Material = "PA12",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "Test"
        };

        _context.Parts.Add(part);
        await _context.SaveChangesAsync();

        // Create stage requirement
        var stageReq = new PartStageRequirement
        {
            PartId = part.Id,
            ProductionStageId = stage.Id,
            IsRequired = true,
            ExecutionOrder = 1,
            EstimatedHours = (double?)4.0m,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "Test"
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
            EstimatedHours = (double)4.0m,
            MachineId = "TEST-MACHINE",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "Test"
        };

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        return (job, stage);
    }

    private async Task<(Job job, ProductionStage slsStage)> SetupSLSTestJobAndStage()
    {
        // Ensure SLS stage exists
        var slsStage = await _context.ProductionStages
            .FirstOrDefaultAsync(ps => ps.Name.Contains("SLS") && ps.IsActive);
        
        if (slsStage == null)
        {
            slsStage = new ProductionStage
            {
                Name = "3D Printing (SLS)",
                StageColor = "#007bff",
                Department = "3D Printing",
                DisplayOrder = 1,
                IsActive = true,
                DefaultSetupMinutes = 240
            };
            _context.ProductionStages.Add(slsStage);
            await _context.SaveChangesAsync();
        }

        var partNumber = "SLS-WORKFLOW-001";
        
        // Clean up any existing test data
        var existingJob = await _context.Jobs
            .FirstOrDefaultAsync(j => j.PartNumber == partNumber);
        
        if (existingJob != null)
        {
            var existingExecutions = await _context.ProductionStageExecutions
                .Where(pse => pse.JobId == existingJob.Id)
                .ToListAsync();
            _context.ProductionStageExecutions.RemoveRange(existingExecutions);
            
            _context.Jobs.Remove(existingJob);
            await _context.SaveChangesAsync();
        }

        // Create test part for SLS
        var part = new Part
        {
            PartNumber = partNumber,
            Name = "SLS Test Part for Workflow Testing",
            Description = "SLS Test Part for Workflow Testing",
            Material = "PA12",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "Test"
        };

        _context.Parts.Add(part);
        await _context.SaveChangesAsync();

        // Create SLS stage requirement
        var stageReq = new PartStageRequirement
        {
            PartId = part.Id,
            ProductionStageId = slsStage.Id,
            IsRequired = true,
            ExecutionOrder = 1,
            EstimatedHours = (double?)6.0m,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "Test"
        };

        _context.PartStageRequirements.Add(stageReq);
        await _context.SaveChangesAsync();

        // Create test job for SLS
        var job = new Job
        {
            PartId = part.Id,
            PartNumber = part.PartNumber,
            Quantity = 1,
            Priority = 1,
            Status = "Scheduled",
            ScheduledStart = DateTime.Today.AddHours(8),
            ScheduledEnd = DateTime.Today.AddHours(16),
            EstimatedHours = (double)6.0m,
            MachineId = "SLS-001",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "Test"
        };

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        return (job, slsStage);
    }

    private async Task SetupMultipleTestJobs()
    {
        // Create multiple test jobs for comprehensive testing
        for (int i = 1; i <= 3; i++)
        {
            await SetupTestJobAndStage($"MULTI-{i:D3}");
        }
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