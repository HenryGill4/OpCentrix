//Job.cs:
namespace OpCentrix.Models
{
    // Represents a scheduled job on a specific machine.
    // This model mirrors the DB schema defined in the initial migration.
    public class Job
    {
        public int Id { get; set; }

        // Machine that will run the job (required)
        public string MachineId { get; set; } = string.Empty;

        // Date/time range for the job
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }

        // Linked part information
        public int PartId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public virtual Part? Part { get; set; }

        // Additional fields expected by migrations and used in the modal
        public string Status { get; set; } = "Scheduled";
        public string? Notes { get; set; }
        public string? Operator { get; set; }

        // Quantity scheduled for this job
        public int Quantity { get; set; } = 1;
    }
}
