using System;
using System.Collections.Generic;
using OpCentrix.Models;
using Xunit;

namespace OpCentrix.UnitTests
{
    public class ModelLogicTests
    {
        [Fact]
        public void JobViewModel_StatusClass_MapsCorrectly()
        {
            var jobActive = new JobViewModel { Status = "Active" };
            var jobDelayed = new JobViewModel { Status = "Delayed" };
            var jobComplete = new JobViewModel { Status = "Complete" };
            var jobOther = new JobViewModel { Status = "Other" };
            Assert.Equal("bg-green-500", jobActive.StatusClass);
            Assert.Equal("bg-orange-500", jobDelayed.StatusClass);
            Assert.Equal("bg-gray-400 border border-green-500", jobComplete.StatusClass);
            Assert.Equal("bg-blue-500", jobOther.StatusClass);
        }

        [Fact]
        public void JobViewModel_DurationMinutes_IsCorrect()
        {
            var start = DateTime.Today;
            var end = start.AddHours(2);
            var job = new JobViewModel { ScheduledStart = start, ScheduledEnd = end };
            Assert.Equal(120, job.DurationMinutes);
        }

        [Fact]
        public void ScheduledJob_StatusClass_MapsCorrectly()
        {
            var inProgress = new ScheduledJob { Status = "In Progress" };
            var complete = new ScheduledJob { Status = "Complete" };
            var other = new ScheduledJob { Status = "Other" };
            Assert.Equal("bg-yellow-500", inProgress.StatusClass);
            Assert.Equal("bg-green-500", complete.StatusClass);
            Assert.Equal("bg-blue-500", other.StatusClass);
        }

        [Fact]
        public void ScheduledJob_Defaults_Are_Set()
        {
            var job = new ScheduledJob();
            Assert.Equal(1, job.DurationDays);
            Assert.Equal("Scheduled", job.Status);
            Assert.Equal(string.Empty, job.PartNumber);
            Assert.Equal(string.Empty, job.MachineName);
            Assert.Equal(string.Empty, job.OperatorName);
            Assert.Equal(string.Empty, job.Notes);
        }

        [Fact]
        public void MachineViewModel_Initializes_Jobs_List()
        {
            var machine = new MachineViewModel { Id = "TI1", Name = "Titanium 1" };
            Assert.NotNull(machine.Jobs);
            Assert.Empty(machine.Jobs);
            machine.Jobs.Add(new ScheduledJob { JobId = 1 });
            Assert.Single(machine.Jobs);
        }
    }
}
