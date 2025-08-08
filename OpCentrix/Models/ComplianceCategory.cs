using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models
{
    /// <summary>
    /// Compliance Category lookup table for Parts
    /// Handles regulatory compliance requirements (NFA, Non-NFA, etc.)
    /// </summary>
    public class ComplianceCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string RegulatoryLevel { get; set; } = "Standard"; // 'Standard', 'Regulated', 'Restricted'

        public bool RequiresSpecialHandling { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [Range(1, 1000)]
        public int SortOrder { get; set; } = 100;

        public string CreatedDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";

        public string LastModifiedDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        [Required]
        [StringLength(100)]
        public string LastModifiedBy { get; set; } = "System";

        // Navigation properties
        public virtual ICollection<Part> Parts { get; set; } = new List<Part>();

        /// <summary>
        /// Get display text for the compliance category
        /// </summary>
        public string DisplayText => $"{Name} ({RegulatoryLevel})";

        /// <summary>
        /// Get CSS class for regulatory level styling
        /// </summary>
        public string RegulatoryLevelCssClass => RegulatoryLevel switch
        {
            "Standard" => "badge bg-success",
            "Regulated" => "badge bg-warning",
            "Restricted" => "badge bg-danger",
            _ => "badge bg-secondary"
        };

        /// <summary>
        /// Get icon for regulatory level
        /// </summary>
        public string RegulatoryLevelIcon => RegulatoryLevel switch
        {
            "Standard" => "fas fa-check-circle",
            "Regulated" => "fas fa-exclamation-triangle",
            "Restricted" => "fas fa-ban",
            _ => "fas fa-question-circle"
        };

        /// <summary>
        /// Check if this compliance category is being used by any parts
        /// </summary>
        public bool IsInUse => Parts?.Any(p => p.IsActive) ?? false;

        /// <summary>
        /// Get special handling requirements text
        /// </summary>
        public string SpecialHandlingText => RequiresSpecialHandling 
            ? "Requires special handling procedures" 
            : "Standard handling procedures";
    }
}