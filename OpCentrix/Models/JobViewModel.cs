//JobViewModel.cs:
namespace OpCentrix.Models
{
    public class JobViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;

        public DateTime ScheduledStart { get; set; } = DateTime.Now;
        public DateTime ScheduledEnd { get; set; } = DateTime.Now.AddHours(1);

        // Additional fields used for persistence
        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }
        public string? EndReason { get; set; }

        public string Status { get; set; } = "Scheduled";
        public string Notes { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;

        public int DurationHrs => (int)(ScheduledEnd - ScheduledStart).TotalHours;

        public int DurationMinutes => (int)(ScheduledEnd - ScheduledStart).TotalMinutes;

        public string StatusClass =>
            Status switch
            {
                "Active" => "bg-green-500",
                "Delayed" => "bg-orange-500",
                "Complete" => "bg-gray-400 border border-green-500",
                _ => "bg-blue-500"
            };
    }
}
