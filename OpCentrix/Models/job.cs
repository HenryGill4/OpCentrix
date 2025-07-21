//Job.cs:
namespace OpCentrix.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }
        public string Status { get; set; } = "Scheduled";
        public string? Notes { get; set; }
        public string? Operator { get; set; }
    }
}
