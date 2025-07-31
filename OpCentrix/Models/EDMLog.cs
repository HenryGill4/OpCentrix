using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models
{
    /// <summary>
    /// EDM (Electrical Discharge Machining) Operation Log Entry
    /// Stores detailed information about EDM operations for tracking and compliance
    /// </summary>
    public class EDMLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string LogNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime LogDate { get; set; }

        [StringLength(50)]
        public string Shift { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string OperatorName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string OperatorInitials { get; set; } = string.Empty;

        [StringLength(10)]
        public string StartTime { get; set; } = string.Empty;

        [StringLength(10)]
        public string EndTime { get; set; } = string.Empty;

        [StringLength(50)]
        public string Measurement1 { get; set; } = string.Empty;

        [StringLength(50)]
        public string Measurement2 { get; set; } = string.Empty;

        [StringLength(50)]
        public string ToleranceStatus { get; set; } = string.Empty;

        [StringLength(200)]
        public string ScrapIssues { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;

        [StringLength(50)]
        public string TotalTime { get; set; } = string.Empty;

        // Audit fields
        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string LastModifiedBy { get; set; } = string.Empty;

        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Additional tracking fields
        [StringLength(100)]
        public string MachineUsed { get; set; } = string.Empty;

        [StringLength(100)]
        public string ProcessType { get; set; } = "EDM";

        [StringLength(500)]
        public string QualityNotes { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;

        public bool RequiresReview { get; set; } = false;

        [StringLength(100)]
        public string ReviewedBy { get; set; } = string.Empty;

        public DateTime? ReviewedDate { get; set; }

        [StringLength(500)]
        public string ReviewNotes { get; set; } = string.Empty;

        // JSON fields for additional data
        [StringLength(2000)]
        public string ProcessParameters { get; set; } = "{}";

        [StringLength(1000)]
        public string Measurements { get; set; } = "{}";

        // Foreign key relationship to Part (optional)
        public int? PartId { get; set; }
        public virtual Part? Part { get; set; }
    }
}