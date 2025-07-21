namespace OpCentrix.Models
{
    public class ScheduledJob
    {
        public int JobId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public int DurationDays { get; set; } = 1;
        public string Status { get; set; } = "Scheduled";
        public string OperatorName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public string StatusClass => Status switch
        {
            "In Progress" => "bg-yellow-500",
            "Complete" => "bg-green-500",
            _ => "bg-blue-500"
        };
    }
}

