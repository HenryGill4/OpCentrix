using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models
{
    /// <summary>
    /// Legacy flag to stage mapping table for automated migration
    /// Maps old boolean fields to ProductionStage requirements
    /// </summary>
    public class LegacyFlagToStageMap
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string LegacyFieldName { get; set; } = string.Empty; // 'RequiresSLSPrinting', 'RequiresCNCMachining', etc.

        [Required]
        [StringLength(100)]
        public string ProductionStageName { get; set; } = string.Empty; // 'SLS Printing', 'CNC Machining', etc.

        [Range(1, 100)]
        public int ExecutionOrder { get; set; } = 1; // 1, 2, 3, etc.

        [Range(0, 1440)] // Max 24 hours in minutes
        public int DefaultSetupMinutes { get; set; } = 30;

        [Range(0, 1440)] // Max 24 hours in minutes
        public int DefaultTeardownMinutes { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Get display text for the mapping
        /// </summary>
        public string DisplayText => $"{LegacyFieldName} ? {ProductionStageName} (Order: {ExecutionOrder})";

        /// <summary>
        /// Get total time in minutes including setup and teardown
        /// </summary>
        public int TotalTimeMinutes => DefaultSetupMinutes + DefaultTeardownMinutes;

        /// <summary>
        /// Get setup time in hours (decimal)
        /// </summary>
        public decimal SetupTimeHours => (decimal)DefaultSetupMinutes / 60.0m;

        /// <summary>
        /// Get teardown time in hours (decimal)
        /// </summary>
        public decimal TeardownTimeHours => (decimal)DefaultTeardownMinutes / 60.0m;

        /// <summary>
        /// Get stage type from production stage name
        /// </summary>
        public string StageType
        {
            get
            {
                return ProductionStageName.ToLowerInvariant() switch
                {
                    var name when name.Contains("sls") || name.Contains("printing") => "Primary Manufacturing",
                    var name when name.Contains("cnc") || name.Contains("machining") => "Secondary Operations",
                    var name when name.Contains("edm") => "Precision Operations",
                    var name when name.Contains("assembly") => "Assembly",
                    var name when name.Contains("finishing") => "Finishing",
                    var name when name.Contains("quality") || name.Contains("inspection") => "Quality Control",
                    _ => "General"
                };
            }
        }

        /// <summary>
        /// Get CSS class for stage type
        /// </summary>
        public string StageTypeCssClass => StageType switch
        {
            "Primary Manufacturing" => "badge bg-primary",
            "Secondary Operations" => "badge bg-info",
            "Precision Operations" => "badge bg-warning",
            "Assembly" => "badge bg-success",
            "Finishing" => "badge bg-secondary",
            "Quality Control" => "badge bg-danger",
            _ => "badge bg-light"
        };

        /// <summary>
        /// Check if this is a critical manufacturing stage
        /// </summary>
        public bool IsCriticalStage => StageType == "Primary Manufacturing" || StageType == "Quality Control";
    }
}