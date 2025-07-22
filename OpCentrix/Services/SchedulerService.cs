using OpCentrix.Models;
using OpCentrix.Models.ViewModels;

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
            startDate ??= DateTime.Today;

            // This will be injected with the actual data context in the controller
            return new SchedulerPageViewModel
            {
                Zoom = zoom,
                StartDate = startDate.Value,
                SlotsPerDay = CalculateSlotsPerDay(zoom),
                SlotMinutes = CalculateSlotMinutes(zoom),
                Dates = new List<DateTime>(),
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
                    var layer = layers[layerIndex];
                    if (!layer.Any(existingJob => job.OverlapsWith(existingJob)))
                    {
                        layer.Add(job);
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

            // Basic validation
            if (string.IsNullOrWhiteSpace(job.MachineId))
                errors.Add("Machine is required");

            if (job.PartId <= 0)
                errors.Add("Part is required");

            if (job.ScheduledStart == default)
                errors.Add("Start time is required");

            if (job.ScheduledEnd == default || job.ScheduledEnd <= job.ScheduledStart)
                errors.Add("End time must be after start time");

            if (job.Quantity <= 0)
                errors.Add("Quantity must be greater than 0");

            // Check for overlaps
            var overlappingJobs = existingJobs.Where(j =>
                j.MachineId == job.MachineId &&
                j.Id != job.Id &&
                job.OverlapsWith(j)
            ).ToList();

            if (overlappingJobs.Any())
            {
                var overlappingPartNumbers = string.Join(", ", overlappingJobs.Select(j => j.PartNumber));
                errors.Add($"This job overlaps with existing jobs: {overlappingPartNumbers}");
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
            int days = 14; // Default to 2 weeks

            if (jobs.Any())
            {
                var minDate = jobs.Min(j => j.ScheduledStart.Date);
                var maxDate = jobs.Max(j => j.ScheduledEnd.Date);
                var calculatedDays = (int)(maxDate - minDate).TotalDays + 3; // Add buffer
                days = Math.Max(7, Math.Min(30, calculatedDays)); // Between 1 week and 1 month
                return Enumerable.Range(0, days).Select(i => minDate.AddDays(i)).ToList();
            }

            var startDate = DateTime.Today;
            return Enumerable.Range(0, days).Select(i => startDate.AddDays(i)).ToList();
        }
    }
}