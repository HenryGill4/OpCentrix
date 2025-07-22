using System;
using System.Collections.Generic;
using System.Linq;
using OpCentrix.Models;
using OpCentrix.Models.ViewModels;
using OpCentrix.Pages.Scheduler;
using Xunit;

namespace OpCentrix.Tests.Unit
{
    public class SchedulerLogicTests
    {
        [Fact]
        public void CalculateDatesAndRowHeight_NoJobs_DefaultsTo7DaysAndBaseRowHeight()
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
        public void CalculateDatesAndRowHeight_JobsWithOverlap_CalculatesLayersAndRowHeight()
        {
            // Arrange
            var jobs = new List<Job>
            {
                new Job { ScheduledStart = DateTime.Today, ScheduledEnd = DateTime.Today.AddDays(2) },
                new Job { ScheduledStart = DateTime.Today.AddDays(1), ScheduledEnd = DateTime.Today.AddDays(3) },
                new Job { ScheduledStart = DateTime.Today.AddDays(2), ScheduledEnd = DateTime.Today.AddDays(4) }
            };
            int baseRowHeight = 160;

            // Act
            var (dates, rowHeight) = IndexModel.CalculateDatesAndRowHeight(jobs, baseRowHeight);

            // Assert
            Assert.True(rowHeight > baseRowHeight); // Should be more than 1 layer
            Assert.True(dates.Count >= 4); // Should cover all job dates
        }

        [Fact]
        public void CalculateDatesAndRowHeight_JobsNoOverlap_OneLayerRowHeight()
        {
            // Arrange
            var jobs = new List<Job>
            {
                new Job { ScheduledStart = DateTime.Today, ScheduledEnd = DateTime.Today.AddDays(1) },
                new Job { ScheduledStart = DateTime.Today.AddDays(1), ScheduledEnd = DateTime.Today.AddDays(2) },
                new Job { ScheduledStart = DateTime.Today.AddDays(2), ScheduledEnd = DateTime.Today.AddDays(3) }
            };
            int baseRowHeight = 160;

            // Act
            var (dates, rowHeight) = IndexModel.CalculateDatesAndRowHeight(jobs, baseRowHeight);

            // Assert
            Assert.Equal(baseRowHeight, rowHeight); // Only one layer
            Assert.True(dates.Count >= 3);
        }

        [Fact]
        public void IsShift_Weekdays_ReturnsTrue()
        {
            // Arrange
            var monday = new DateTime(2024, 7, 22); // Monday
            var friday = new DateTime(2024, 7, 26); // Friday

            // Act & Assert
            Assert.True(IndexModel.IsShift(monday));
            Assert.True(IndexModel.IsShift(friday));
        }

        [Fact]
        public void IsShift_Weekend_ReturnsFalse()
        {
            // Arrange
            var saturday = new DateTime(2024, 7, 27); // Saturday
            var sunday = new DateTime(2024, 7, 28); // Sunday

            // Act & Assert
            Assert.False(IndexModel.IsShift(saturday));
            Assert.False(IndexModel.IsShift(sunday));
        }
    }
}
