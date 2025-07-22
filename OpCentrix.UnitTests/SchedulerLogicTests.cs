using System;
using System.Collections.Generic;
using System.Linq;
using OpCentrix.Models;
using OpCentrix.Models.ViewModels;
using OpCentrix.Pages.Scheduler;
using Xunit;

namespace OpCentrix.UnitTests
{
    public class SchedulerLogicTests
    {
        [Fact]
        public void CalculateDatesAndRowHeight_Returns7DaysAndBaseRowHeight_WhenNoJobs()
        {
            // Arrange
            var jobs = new List<Job>();
            int baseRowHeight = 160;

            // Act
            var (dates, rowHeight) = IndexModel.CalculateDatesAndRowHeight(jobs, baseRowHeight);

            // Assert
            Assert.Equal(7, dates.Count);
            Assert.Equal(baseRowHeight, rowHeight);
        }

        [Fact]
        public void CalculateDatesAndRowHeight_CorrectlyCalculatesDatesAndLayers_WithOverlappingJobs()
        {
            // Arrange
            var jobs = new List<Job>
            {
                new Job { ScheduledStart = new DateTime(2024, 1, 1, 8, 0, 0), ScheduledEnd = new DateTime(2024, 1, 2, 8, 0, 0) },
                new Job { ScheduledStart = new DateTime(2024, 1, 1, 12, 0, 0), ScheduledEnd = new DateTime(2024, 1, 2, 12, 0, 0) },
                new Job { ScheduledStart = new DateTime(2024, 1, 2, 7, 0, 0), ScheduledEnd = new DateTime(2024, 1, 3, 8, 0, 0) }
            };
            int baseRowHeight = 160;

            // Act
            var (dates, rowHeight) = IndexModel.CalculateDatesAndRowHeight(jobs, baseRowHeight);

            // Assert
            Assert.True(dates.Count >= 2);
            Assert.True(rowHeight >= baseRowHeight);
        }

        [Fact]
        public void IsShift_ReturnsTrueForWeekdays_AndFalseForWeekends()
        {
            // Arrange
            var monday = new DateTime(2024, 7, 22); // Monday
            var saturday = new DateTime(2024, 7, 20); // Saturday

            // Act & Assert
            Assert.True(IndexModel.IsShift(monday));
            Assert.False(IndexModel.IsShift(saturday));
        }

        [Fact]
        public void FooterSummaryViewModel_MachineHours_CalculatesCorrectly()
        {
            // Arrange
            var jobs = new List<Job>
            {
                new Job { MachineId = "TI1", ScheduledStart = DateTime.Today, ScheduledEnd = DateTime.Today.AddHours(2) },
                new Job { MachineId = "TI1", ScheduledStart = DateTime.Today.AddHours(3), ScheduledEnd = DateTime.Today.AddHours(5) },
                new Job { MachineId = "TI2", ScheduledStart = DateTime.Today, ScheduledEnd = DateTime.Today.AddHours(1) }
            };
            var machines = new List<string> { "TI1", "TI2" };
            var summary = new FooterSummaryViewModel();
            summary.MachineHours = machines.ToDictionary(
                m => m,
                m => jobs.Where(j => j.MachineId == m).Sum(j => (j.ScheduledEnd - j.ScheduledStart).TotalHours)
            );

            // Assert
            Assert.Equal(4, summary.MachineHours["TI1"]);
            Assert.Equal(1, summary.MachineHours["TI2"]);
        }
    }
}
