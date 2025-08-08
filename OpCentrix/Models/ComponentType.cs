using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models
{
    /// <summary>
    /// Component Type lookup table for Parts
    /// Replaces the string ComponentType field with a proper lookup
    /// </summary>
    public class ComponentType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

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
        /// Get display text for the component type
        /// </summary>
        public string DisplayText => $"{Name} - {Description}";

        /// <summary>
        /// Check if this component type is being used by any parts
        /// </summary>
        public bool IsInUse => Parts?.Any(p => p.IsActive) ?? false;
    }
}