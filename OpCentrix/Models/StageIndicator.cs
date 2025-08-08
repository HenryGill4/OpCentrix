namespace OpCentrix.Models
{
    /// <summary>
    /// Helper class for displaying stage indicators in the UI
    /// Used by Part model for stage visualization and service layer
    /// </summary>
    public class StageIndicator
    {
        public string Name { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        
        // Additional properties expected by services
        public bool IsRequired { get; set; } = true;
        public bool IsComplete { get; set; } = false;
        public int Order { get; set; } = 1;
        public int EstimatedDurationMinutes { get; set; } = 0;
        public string Department { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}