using System;
using System.Collections.Generic;
using System.Linq;
using OpCentrix.Models;
using OpCentrix.Models.ViewModels;
using OpCentrix.Pages.Scheduler;
using Xunit;

namespace OpCentrix.Tests.Unit
{
    public class JobLogAndSchedulerTests
    {
        [Fact]
        public void JobLogEntry_Creation_And_Properties()
        {
            var now = DateTime.Now;
            var entry = new JobLogEntry
            {
                MachineId = "TI1",
                PartNumber = "TI-1001",
                Action = "Add",
                Operator = "John",
                Notes = "Initial job",
                Timestamp = now
            };
            Assert.Equal("TI1", entry.MachineId);
            Assert.Equal("TI-1001", entry.PartNumber);
            Assert.Equal("Add", entry.Action);
            Assert.Equal("John", entry.Operator);
            Assert.Equal("Initial job", entry.Notes);
            Assert.Equal(now, entry.Timestamp);
        }

        [Fact]
        public void JobLogEntry_Defaults_Are_Set()
        {
            var entry = new JobLogEntry();
            Assert.NotNull(entry.Timestamp);
            Assert.Equal(string.Empty, entry.MachineId);
            Assert.Equal(string.Empty, entry.PartNumber);
            Assert.Equal(string.Empty, entry.Action);
        }

        [Fact]
        public void MachineRowViewModel_Assigns_Jobs_And_Dates()
        {
            var jobs = new List<Job>
            {
                new Job { Id = 1, MachineId = "TI1", ScheduledStart = DateTime.Today, ScheduledEnd = DateTime.Today.AddDays(1) },
                new Job { Id = 2, MachineId = "TI1", ScheduledStart = DateTime.Today.AddDays(1), ScheduledEnd = DateTime.Today.AddDays(2) }
            };
            var dates = new List<DateTime> { DateTime.Today, DateTime.Today.AddDays(1), DateTime.Today.AddDays(2) };
            var vm = new MachineRowViewModel
            {
                MachineId = "TI1",
                Dates = dates,
                Jobs = jobs,
                RowHeight = 160
            };
            Assert.Equal("TI1", vm.MachineId);
            Assert.Equal(3, vm.Dates.Count);
            Assert.Equal(2, vm.Jobs.Count);
            Assert.Equal(160, vm.RowHeight);
        }

        [Fact]
        public void FooterSummaryViewModel_MachineHours_EmptyAndPopulated()
        {
            var summary = new FooterSummaryViewModel();
            Assert.Empty(summary.MachineHours);
            summary.MachineHours["TI1"] = 5.5;
            summary.MachineHours["INC"] = 2.0;
            Assert.Equal(5.5, summary.MachineHours["TI1"]);
            Assert.Equal(2.0, summary.MachineHours["INC"]);
        }

        [Fact]
        public void AddEditJobViewModel_Handles_Errors()
        {
            var job = new Job { MachineId = "TI1", PartId = 1, ScheduledStart = DateTime.Today, ScheduledEnd = DateTime.Today.AddDays(1) };
            var parts = new List<Part> { new Part { Id = 1, PartNumber = "TI-1001", Description = "desc", Material = "titanium", AvgDuration = "1d", AvgDurationDays = 1 } };
            var errors = new List<string> { "Machine required", "Part required" };
            var vm = new AddEditJobViewModel { Job = job, Parts = parts, Errors = errors };
            Assert.Equal(job, vm.Job);
            Assert.Single(vm.Parts);
            Assert.Equal(2, vm.Errors.Count);
        }

        [Fact]
        public void DayCellViewModel_Defaults_And_Properties()
        {
            var job = new Job { Id = 1, MachineId = "TI1", ScheduledStart = DateTime.Today, ScheduledEnd = DateTime.Today.AddDays(1) };
            var vm = new DayCellViewModel
            {
                Date = DateTime.Today,
                MachineId = "TI1",
                Job = job,
                ColSpan = 2,
                ShowAdd = true,
                IsChangeover = false
            };
            Assert.Equal(DateTime.Today, vm.Date);
            Assert.Equal("TI1", vm.MachineId);
            Assert.Equal(job, vm.Job);
            Assert.Equal(2, vm.ColSpan);
            Assert.True(vm.ShowAdd);
            Assert.False(vm.IsChangeover);
        }
    }
}
