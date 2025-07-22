using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using OpCentrix.Data;
using OpCentrix.Models;
using System.Net.Http;
using System.Text;

namespace OpCentrix.IntegrationTests
{
    public class SchedulerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public SchedulerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing context registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SchedulerContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Use in-memory database for testing
                    services.AddDbContext<SchedulerContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString());
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task SchedulerPage_LoadsSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/Scheduler");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("Production Scheduler", content);
            Assert.Contains("TI1", content);
            Assert.Contains("TI2", content);
            Assert.Contains("INC", content);
        }

        [Fact]
        public async Task AdminPage_LoadsSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/Admin");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("Admin Dashboard", content);
        }

        [Fact]
        public async Task AdminJobs_LoadsSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/Admin/Jobs");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("Job Management", content);
        }

        [Fact]
        public async Task AdminParts_LoadsSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/Admin/Parts");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("Parts Management", content);
        }

        [Fact]
        public async Task AdminLogs_LoadsSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/Admin/Logs");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("Job Logs", content);
        }

        [Fact]
        public async Task SchedulerModal_OpensSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/Scheduler?handler=ShowAddModal&machineId=TI1&date=2024-01-01T08:00");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.Contains("Machine", content);
            Assert.Contains("Part", content);
        }

        [Fact]
        public async Task SchedulerZoom_WorksCorrectly()
        {
            // Test different zoom levels
            var zoomLevels = new[] { "day", "hour", "30min", "15min" };

            foreach (var zoom in zoomLevels)
            {
                // Act
                var response = await _client.GetAsync($"/Scheduler?zoom={zoom}");

                // Assert
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                
                Assert.Contains("Production Scheduler", content);
                Assert.Contains(zoom.ToUpper(), content);
            }
        }

        [Fact]
        public async Task HomePage_LoadsSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            // Should load successfully regardless of content
            Assert.True(content.Length > 0);
        }
    }
}