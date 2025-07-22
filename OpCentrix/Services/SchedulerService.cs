using OpCentrix.Models;
using OpCentrix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpCentrix.Services
{
    public interface ISchedulerService
    {
        SchedulerPageViewModel GetSchedulerData(string? zoom = null, DateTime? startDate = null);
        (List<DateTime> Dates, int RowHeight) CalculateMachineRowLayout(string machineId, List<Job> jobs);
        List<List<Job>> CalculateJobLayers(List<Job> jobs);
        bool ValidateJobScheduling(Job job, List<Job> existingJobs, out List<string> errors);
        double GetSlotPosition(DateTime dateTime, DateTime startDate, int slotMinutes);
        int CalculateSlotMinutes(string zoom);
        int CalculateSlotsPerDay(string zoom);
    }

    public class SchedulerService : ISchedulerService
    {
        private const int BaseRowHeight = 160;
        private const int LayerHeight = 40;
        private const int MinRowHeight = 160;
        private const int MaxRowHeight = 400;

        public SchedulerPageViewModel GetSchedulerData(string? zoom = null, DateTime? startDate = null)
        {
            zoom ??= "day";
            var start = (startDate ?? DateTime.UtcNow).Date;
            var dates = Enumerable.Range(0, 14).Select(i => start.AddDays(i)).ToList();

            return new SchedulerPageViewModel
            {
                Zoom = zoom,
                StartDate = start,
                SlotsPerDay = CalculateSlotsPerDay(zoom),
                SlotMinutes = CalculateSlotMinutes(zoom),
                Dates = dates,
                Machines = new List<string> { "TI1", "TI2", "INC" },
                Jobs = new List<Job>()
            };
        }

        public (List<DateTime> Dates, int RowHeight) CalculateMachineRowLayout(string machineId, List<Job> jobs)
        {
            var dates = CalculateDateRange(jobs);
            var layers = CalculateJobLayers(jobs.Where(j => j.MachineId == machineId).ToList());
            var rowHeight = Math.Max(MinRowHeight, Math.Min(MaxRowHeight, BaseRowHeight + (layers.Count - 1) * LayerHeight));
            
            return (dates, rowHeight);
        }

        public List<List<Job>> CalculateJobLayers(List<Job> jobs)
        {
            var layers = new List<List<Job>>();
            var sortedJobs = jobs.OrderBy(j => j.ScheduledStart).ToList();

            foreach (var job in sortedJobs)
            {
                bool placed = false;
                for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
                {
                    if (!layers[layerIndex].Any(existingJob => job.OverlapsWith(existingJob)))
                    {
                        layers[layerIndex].Add(job);
                        placed = true;
                        break;
                    }
                }

                if (!placed)
                {
                    layers.Add(new List<Job> { job });
                }
            }

            return layers;
        }

        public bool ValidateJobScheduling(Job job, List<Job> existingJobs, out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(job.MachineId)) errors.Add("Machine is required.");
            if (job.PartId <= 0) errors.Add("Part is required.");
            if (job.ScheduledStart == default) errors.Add("Start time is required.");
            if (job.ScheduledEnd <= job.ScheduledStart) errors.Add("End time must be after start time.");
            if (job.Quantity <= 0) errors.Add("Quantity must be greater than 0.");

            if (existingJobs.Any(j => j.Id != job.Id && j.OverlapsWith(job)))
            {
                var overlappingPartNumbers = string.Join(", ", existingJobs.Select(j => j.PartNumber));
                errors.Add($"This job overlaps with existing jobs: {overlappingPartNumbers}.");
            }

            return !errors.Any();
        }

        public double GetSlotPosition(DateTime dateTime, DateTime startDate, int slotMinutes)
        {
            var totalMinutesFromStart = (dateTime - startDate.Date).TotalMinutes;
            return totalMinutesFromStart / slotMinutes;
        }

        public int CalculateSlotMinutes(string zoom)
        {
            return zoom switch
            {
                "hour" => 60,
                "30min" => 30,
                "15min" => 15,
                _ => 1440 // day
            };
        }

        public int CalculateSlotsPerDay(string zoom)
        {
            return 1440 / CalculateSlotMinutes(zoom);
        }

        private List<DateTime> CalculateDateRange(List<Job> jobs)
        {
            var startDate = DateTime.UtcNow.Date;
            int days = 14; // Default to 2 weeks

            if (jobs.Any())
            {
                var minDate = jobs.Min(j => j.ScheduledStart.Date);
                var maxDate = jobs.Max(j => j.ScheduledEnd.Date);
                startDate = minDate;
                days = Math.Max(14, (int)(maxDate - minDate).TotalDays + 3);
            }
            
            return Enumerable.Range(0, days).Select(i => startDate.AddDays(i)).ToList();
        }
    }
}