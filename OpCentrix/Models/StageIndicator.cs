namespace OpCentrix.Models
{
    /// <summary>
    /// Helper class for displaying stage indicators in the UI
    /// Used to show visual indicators for required manufacturing stages
    /// </summary>
    public class StageIndicator
    {
        /// <summary>
        /// Short display name (e.g., "SLS", "EDM", "CNC")
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// CSS class for styling (e.g., "bg-primary", "bg-warning")
        /// </summary>
        public string Class { get; set; } = string.Empty;
        
        /// <summary>
        /// Font Awesome icon class (e.g., "fas fa-print", "fas fa-bolt")
        /// </summary>
        public string Icon { get; set; } = string.Empty;
        
        /// <summary>
        /// Full title for tooltips (e.g., "SLS Printing", "EDM Operations")
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether this stage is required for the part
        /// </summary>
        public bool IsRequired { get; set; } = true;
        
        /// <summary>
        /// Whether this stage has been completed
        /// </summary>
        public bool IsComplete { get; set; } = false;
        
        /// <summary>
        /// Display order for stages (1-10)
        /// </summary>
        public int Order { get; set; } = 0;
        
        /// <summary>
        /// Estimated duration for this stage in minutes
        /// </summary>
        public decimal EstimatedDurationMinutes { get; set; } = 0;
        
        /// <summary>
        /// Department responsible for this stage
        /// </summary>
        public string Department { get; set; } = string.Empty;
        
        /// <summary>
        /// Additional notes or requirements for this stage
        /// </summary>
        public string Notes { get; set; } = string.Empty;
    }
}