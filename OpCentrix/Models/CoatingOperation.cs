using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Represents a coating operation for parts processing
    /// </summary>
    public class CoatingOperation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CoatingType { get; set; } = string.Empty; // Anodizing, Powder Coating, Painting, etc.

        [Required]
        [StringLength(50)]
        public string CoatingSpecification { get; set; } = string.Empty; // MIL-A-8625, etc.

        [StringLength(50)]
        public string Color { get; set; } = string.Empty;

        [StringLength(50)]
        public string Thickness { get; set; } = string.Empty; // in microns

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed, Rejected

        [Required]
        public DateTime ScheduledDate { get; set; }

        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }

        [Required]
        [StringLength(100)]
        public string Operator { get; set; } = string.Empty;

        [StringLength(100)]
        public string QualityInspector { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;

        public int ProcessedQuantity { get; set; } = 0;
        public int RejectedQuantity { get; set; } = 0;

        [StringLength(1000)]
        public string ProcessNotes { get; set; } = string.Empty;

        [StringLength(1000)]
        public string QualityNotes { get; set; } = string.Empty;

        [StringLength(500)]
        public string RejectionReason { get; set; } = string.Empty;

        // Temperature and environmental conditions
        [Range(0, 100)]
        public double ProcessTemperature { get; set; } = 20.0; // Celsius

        [Range(0, 100)]
        public double Humidity { get; set; } = 50.0; // Percentage

        // Cost tracking
        [Column(TypeName = "decimal(10,2)")]
        public decimal MaterialCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal LaborCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal OverheadCost { get; set; } = 0;

        // Audit trail
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastModifiedBy { get; set; } = string.Empty;

        // Navigation properties
        public virtual Part? Part { get; set; }

        // Computed properties
        [NotMapped]
        public string StatusColor => Status switch
        {
            "Completed" => "#10B981", // green
            "In Progress" => "#F59E0B", // amber
            "Rejected" => "#EF4444", // red
            "Pending" => "#6B7280", // gray
            _ => "#3B82F6" // blue
        };

        [NotMapped]
        public decimal TotalCost => MaterialCost + LaborCost + OverheadCost;

        [NotMapped]
        public double CompletionRate => Quantity > 0 ? (double)ProcessedQuantity / Quantity * 100 : 0;

        [NotMapped]
        public double QualityRate => ProcessedQuantity > 0 ? (double)(ProcessedQuantity - RejectedQuantity) / ProcessedQuantity * 100 : 100;

        [NotMapped]
        public TimeSpan? ProcessingTime => ActualStartDate.HasValue && ActualCompletionDate.HasValue 
            ? ActualCompletionDate.Value - ActualStartDate.Value 
            : null;
    }
}