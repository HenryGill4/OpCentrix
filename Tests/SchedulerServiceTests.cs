// Create Tests/SchedulerServiceTests.cs for unit testing
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OpCentrix.Data;
using OpCentrix.Models.ViewModels;
using OpCentrix.Services;
using Xunit;

namespace OpCentrix.Tests
{
    public class SchedulerServiceTests
    {
        private SchedulerContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<SchedulerContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            return new SchedulerContext(options);
        }

        [Fact]
        public async Task CreateJobAsync_ValidJob_CreatesSuccessfully()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var logger = Mock.Of<ILogger<SchedulerService>>();
            var service = new SchedulerService(context, logger);

            var model = new AddEditJobViewModel
            {
                PartId = 1,
                MachineId = "TI1",
                ScheduledStart = DateTime.UtcNow.AddHours(1),
                ScheduledEnd = DateTime.UtcNow.AddHours(9),
                Operator = "testuser"
            };

            // Act
            var result = await service.CreateJobAsync(model, "admin");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TI1", result.MachineId);
            Assert.Equal("admin", result.CreatedBy);
        }

        [Fact]
        public async Task GetConflictingJobsAsync_OverlappingJobs_ReturnsConflicts()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var logger = Mock.Of<ILogger<SchedulerService>>();
            var service = new SchedulerService(context, logger);

            // Add existing job
            var existingJob = new Job
            {
                MachineId = "TI1",
                ScheduledStart = DateTime.UtcNow,
                ScheduledEnd = DateTime.UtcNow.AddHours(8),
                PartId = 1
            };
            context.Jobs.Add(existingJob);
            await context.SaveChangesAsync();

            // Create overlapping job
            var overlappingJob = new Job
            {
                MachineId = "TI1",
                ScheduledStart = DateTime.UtcNow.AddHours(4),
                ScheduledEnd = DateTime.UtcNow.AddHours(12),
                PartId = 2
            };

            // Act
            var conflicts = await service.GetConflictingJobsAsync(overlappingJob);

            // Assert
            Assert.Single(conflicts);
            Assert.Equal(existingJob.Id, conflicts.First().Id);
        }
    }
}