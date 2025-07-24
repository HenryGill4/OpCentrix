using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using OpCentrix.Models;
using OpCentrix.Services;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;

namespace OpCentrix.UnitTests
{
    public class ComprehensiveSchedulerTests
    {
        private SchedulerContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<SchedulerContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            var context = new SchedulerContext(options);
            
            // Seed test data
            SeedTestData(context);
            
            return context;
        }
        
        private void SeedTestData(SchedulerContext context)
        {
            var parts = new List<Part>
            {
                new Part 
                { 
                    Id = 1, 
                    PartNumber = "TEST-001", 
                    Description = "Test Part 1", 
                    Material = "PLA",
                    EstimatedHours = 8.0,
                    IsActive = true,
                    MaterialCostPerUnit = 50.00m,
                    StandardLaborCostPerHour = 75.00m
                },
                new Part 
                { 
                    Id = 2, 
                    PartNumber = "TEST-002", 
                    Description = "Test Part 2", 
                    Material = "ABS",
                    EstimatedHours = 4.0,
                    IsActive = true,
                    MaterialCostPerUnit = 30.00m,
                    StandardLaborCostPerHour = 75.00m
                }
            };
            
            context.Parts.AddRange(parts);
            context.SaveChanges();
        }

        #region Job Model Tests
        
        [Fact]
        public void Job_OverlapsWith_DetectsOverlappingJobs()
        {
            // Arrange
            var job1 = new Job
            {
                MachineId = "TI1",
                ScheduledStart = new DateTime(2024, 1, 1, 8, 0, 0),
                ScheduledEnd = new DateTime(2024, 1, 1, 16, 0, 0)
            };
            
            var job2 = new Job
            {
                MachineId = "TI1",
                ScheduledStart = new DateTime(2024, 1, 1, 12, 0, 0),
                ScheduledEnd = new DateTime(2024, 1, 1, 20, 0, 0)
            };
            
            var job3 = new Job
            {
                MachineId = "TI2", // Different machine
                ScheduledStart = new DateTime(2024, 1, 1, 12, 0, 0),
                ScheduledEnd = new DateTime(2024, 1, 1, 20, 0, 0)
            };
            
            var job4 = new Job
            {
                MachineId = "TI1",
                ScheduledStart = new DateTime(2024, 1, 1, 17, 0, 0), // After job1 ends
                ScheduledEnd = new DateTime(2024, 1, 2, 8, 0, 0)
            };
            
            // Act & Assert
            Assert.True(job1.OverlapsWith(job2)); // Same machine, overlapping times
            Assert.False(job1.OverlapsWith(job3)); // Different machines
            Assert.False(job1.OverlapsWith(job4)); // Same machine, non-overlapping times
        }
        
        [Fact]
        public void Job_CalculatedProperties_WorkCorrectly()
        {
            // Arrange
            var job = new Job
            {
                ScheduledStart = new DateTime(2024, 1, 1, 8, 0, 0),
                ScheduledEnd = new DateTime(2024, 1, 1, 16, 0, 0),
                ActualStart = new DateTime(2024, 1, 1, 8, 30, 0),
                ActualEnd = new DateTime(2024, 1, 1, 17, 0, 0),
                Quantity = 10,
                ProducedQuantity = 10,
                DefectQuantity = 2,
                EstimatedHours = 8.0,
                LaborCostPerHour = 75.00m,
                MaterialCostPerUnit = 10.00m, // Reduced to make calculation easier
                OverheadCostPerHour = 1.00m    // Reduced to make calculation easier
            };
            
            // Act & Assert
            Assert.Equal(480, job.DurationMinutes); // 8 hours = 480 minutes
            Assert.Equal(8.0, job.DurationHours); // 8 hours
            Assert.Equal(80.0, job.QualityScore); // (10-2)/10 * 100 = 80%
            Assert.Equal(600.00m, job.EstimatedLaborCost); // 8 * 75
            Assert.Equal(100.00m, job.EstimatedMaterialCost); // 10 * 10
            Assert.Equal(708.00m, job.EstimatedTotalCost); // 600 + 100 + (8*1)
        }
        
        [Fact]
        public void Job_GetStatusColor_ReturnsCorrectColors()
        {
            // Arrange & Act & Assert
            var completedJob = new Job { Status = "completed" };
            Assert.Equal("#10B981", completedJob.GetStatusColor());
            
            var inProgressJob = new Job { Status = "in-progress" };
            Assert.Equal("#F59E0B", inProgressJob.GetStatusColor());
            
            var delayedJob = new Job { Status = "delayed" };
            Assert.Equal("#EF4444", delayedJob.GetStatusColor());
            
            var scheduledJob = new Job { Status = "scheduled" };
            Assert.Equal("#3B82F6", scheduledJob.GetStatusColor());
        }
        
        [Fact]
        public void Job_GridPositionCalculation_WorksCorrectly()
        {
            // Arrange
            var job = new Job
            {
                ScheduledStart = new DateTime(2024, 1, 1, 10, 0, 0),
                ScheduledEnd = new DateTime(2024, 1, 1, 14, 0, 0)
            };
            var gridStartDate = new DateTime(2024, 1, 1, 0, 0, 0);
            var slotMinutes = 60; // 1 hour slots
            
            // Act
            var position = job.CalculateGridPosition(gridStartDate, slotMinutes);
            var width = job.CalculateGridWidth(slotMinutes);
            
            // Assert
            Assert.Equal(10.0, position); // 10 hours from midnight
            Assert.Equal(4.0, width); // 4 hours duration
        }
        
        #endregion
        
        #region Part Model Tests
        
        [Fact]
        public void Part_CalculatedProperties_WorkCorrectly()
        {
            // Arrange
            var part = new Part
            {
                EstimatedHours = 8.0,
                AverageActualHours = 9.0,
                MaterialCostPerUnit = 50.00m,
                StandardLaborCostPerHour = 75.00m,
                SetupCost = 150.00m,
                ToolingCost = 25.00m,
                TotalUnitsProduced = 10
            };
            
            // Act & Assert
            Assert.Equal(88.89, part.EstimateAccuracy, 2); // (8/9)*100 rounded to 2 decimals
            Assert.Equal("Medium", part.ComplexityLevel); // 8 hours = Medium complexity
            Assert.Equal(690.00m, part.EstimatedTotalCostPerUnit); // 50 + (75*8) + (150/10) + 25
        }
        
        [Fact]
        public void Part_ComplexityLevel_ClassifiesCorrectly()
        {
            // Arrange & Act & Assert
            var simplePart = new Part { EstimatedHours = 1.5 };
            Assert.Equal("Simple", simplePart.ComplexityLevel);
            
            var mediumPart = new Part { EstimatedHours = 6.0 };
            Assert.Equal("Medium", mediumPart.ComplexityLevel);
            
            var complexPart = new Part { EstimatedHours = 16.0 };
            Assert.Equal("Complex", complexPart.ComplexityLevel);
            
            var veryComplexPart = new Part { EstimatedHours = 30.0 };
            Assert.Equal("Very Complex", veryComplexPart.ComplexityLevel);
        }
        
        #endregion
        
        #region Scheduler Service Tests
        
        [Fact]
        public void SchedulerService_ValidateJobScheduling_PreventsOverlaps()
        {
            // Arrange
            var service = new SchedulerService();
            var existingJob = new Job
            {
                Id = 1,
                MachineId = "TI1",
                PartId = 1,
                PartNumber = "TEST-001",
                ScheduledStart = new DateTime(2024, 1, 1, 8, 0, 0),
                ScheduledEnd = new DateTime(2024, 1, 1, 16, 0, 0),
                Quantity = 1
            };
            
            var newOverlappingJob = new Job
            {
                Id = 0,
                MachineId = "TI1",
                PartId = 1,
                PartNumber = "TEST-001",
                ScheduledStart = new DateTime(2024, 1, 1, 12, 0, 0),
                ScheduledEnd = new DateTime(2024, 1, 1, 20, 0, 0),
                Quantity = 1
            };
            
            var newNonOverlappingJob = new Job
            {
                Id = 0,
                MachineId = "TI1",
                PartId = 1,
                PartNumber = "TEST-001",
                ScheduledStart = new DateTime(2024, 1, 1, 17, 0, 0),
                ScheduledEnd = new DateTime(2024, 1, 2, 8, 0, 0),
                Quantity = 1
            };
            
            // Act
            var overlapResult = service.ValidateJobScheduling(newOverlappingJob, new List<Job> { existingJob }, out var overlapErrors);
            var nonOverlapResult = service.ValidateJobScheduling(newNonOverlappingJob, new List<Job> { existingJob }, out var nonOverlapErrors);
            
            // Assert
            Assert.False(overlapResult);
            Assert.Contains("overlaps", overlapErrors.FirstOrDefault()?.ToLower() ?? "");
            
            Assert.True(nonOverlapResult);
            Assert.Empty(nonOverlapErrors);
        }
        
        [Fact]
        public void SchedulerService_CalculateJobLayers_LayersOverlappingJobs()
        {
            // Arrange
            var service = new SchedulerService();
            var jobs = new List<Job>
            {
                new Job 
                { 
                    Id = 1, 
                    MachineId = "TI1",
                    ScheduledStart = new DateTime(2024, 1, 1, 8, 0, 0),
                    ScheduledEnd = new DateTime(2024, 1, 1, 12, 0, 0)
                },
                new Job 
                { 
                    Id = 2, 
                    MachineId = "TI1",
                    ScheduledStart = new DateTime(2024, 1, 1, 10, 0, 0),
                    ScheduledEnd = new DateTime(2024, 1, 1, 14, 0, 0)
                },
                new Job 
                { 
                    Id = 3, 
                    MachineId = "TI1",
                    ScheduledStart = new DateTime(2024, 1, 1, 15, 0, 0),
                    ScheduledEnd = new DateTime(2024, 1, 1, 18, 0, 0)
                }
            };
            
            // Act
            var layers = service.CalculateJobLayers(jobs);
            
            // Assert
            Assert.Equal(2, layers.Count); // Jobs 1 & 2 overlap, so need 2 layers
            Assert.Single(layers, layer => layer.Any(j => j.Id == 1) && layer.Any(j => j.Id == 3)); // Job 1 and 3 can share layer
            Assert.Single(layers, layer => layer.Any(j => j.Id == 2) && layer.Count == 1); // Job 2 needs its own layer
        }
        
        #endregion
        
        #region Integration Tests
        
        [Fact]
        public void DatabaseContext_JobPartRelationship_WorksCorrectly()
        {
            // Arrange
            using var context = GetInMemoryContext();
            
            var job = new Job
            {
                MachineId = "TI1",
                PartId = 1,
                PartNumber = "TEST-001",
                ScheduledStart = DateTime.UtcNow,
                ScheduledEnd = DateTime.UtcNow.AddHours(8),
                Quantity = 5,
                Status = "Scheduled"
            };
            
            // Act
            context.Jobs.Add(job);
            context.SaveChanges();
            
            var retrievedJob = context.Jobs.Include(j => j.Part).FirstOrDefault(j => j.Id == job.Id);
            
            // Assert
            Assert.NotNull(retrievedJob);
            Assert.NotNull(retrievedJob.Part);
            Assert.Equal("TEST-001", retrievedJob.Part.PartNumber);
            Assert.Equal("Test Part 1", retrievedJob.Part.Description);
        }
        
        [Fact]
        public void JobLogEntry_AuditTrail_CreatesCorrectly()
        {
            // Arrange
            using var context = GetInMemoryContext();
            
            var logEntry = new JobLogEntry
            {
                MachineId = "TI1",
                PartNumber = "TEST-001",
                Action = "Created",
                Operator = "TestUser",
                Notes = "Test job creation",
                Timestamp = DateTime.UtcNow
            };
            
            // Act
            context.JobLogEntries.Add(logEntry);
            context.SaveChanges();
            
            var retrievedEntry = context.JobLogEntries.FirstOrDefault(e => e.Id == logEntry.Id);
            
            // Assert
            Assert.NotNull(retrievedEntry);
            Assert.Equal("Created", retrievedEntry.Action);
            Assert.Equal("TestUser", retrievedEntry.Operator);
        }
        
        #endregion
        
        #region Performance Tests
        
        [Fact]
        public void SchedulerService_GetSchedulerData_PerformsEfficiently()
        {
            // Arrange
            var service = new SchedulerService();
            var startTime = DateTime.UtcNow;
            
            // Act
            var result = service.GetSchedulerData("day", DateTime.UtcNow);
            var executionTime = DateTime.UtcNow - startTime;
            
            // Assert
            Assert.NotNull(result);
            Assert.True(executionTime.TotalMilliseconds < 100); // Should complete in under 100ms
            Assert.Equal("day", result.Zoom);
            Assert.Equal(3, result.Machines.Count);
        }
        
        #endregion
        
        #region Edge Case Tests
        
        [Fact]
        public void Job_HandlesNullValues_Gracefully()
        {
            // Arrange
            var job = new Job();
            
            // Act & Assert (should not throw exceptions)
            Assert.Equal(0, job.DurationMinutes);
            Assert.Equal(0, job.DurationHours);
            Assert.Equal(100, job.QualityScore); // Default when no defects
            Assert.Equal("#3B82F6", job.GetStatusColor()); // Default color
            Assert.Equal("#10B981", job.GetPriorityColor()); // Priority 3 = Normal = Green
        }
        
        [Fact]
        public void SchedulerService_ValidatesJobWithMissingData()
        {
            // Arrange
            var service = new SchedulerService();
            var invalidJob = new Job(); // Missing required fields
            
            // Act
            var isValid = service.ValidateJobScheduling(invalidJob, new List<Job>(), out var errors);
            
            // Assert
            Assert.False(isValid);
            Assert.True(errors.Count > 0);
            Assert.Contains(errors, e => e.Contains("Machine"));
            Assert.Contains(errors, e => e.Contains("Part"));
        }
        
        #endregion
    }
}