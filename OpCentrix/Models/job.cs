//Job.cs:
namespace OpCentrix.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string? MachineId { get; set; } // Made nullable
        public DateTime ScheduledStart { get; set; }

        // Add ScheduledEnd property
        public DateTime ScheduledEnd { get; set; } // New property

        // Add PartNumber property if it doesn't exist
        public string? PartNumber { get; set; } // New property, made nullable

        public int PartId { get; set; } // Foreign key to Part
        public virtual Part? Part { get; set; } // Made nullable
    }
}
