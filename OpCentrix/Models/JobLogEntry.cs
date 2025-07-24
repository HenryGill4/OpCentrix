using System;

namespace OpCentrix.Models
{
    public class JobLogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string MachineId { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // e.g. Started, Completed, Issue
        public string? Notes { get; set; }
        public string? Operator { get; set; }
    }
}
