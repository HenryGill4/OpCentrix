using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Represents a prototype job that tracks a part through all production stages
    /// to capture real time and cost data for future production planning
    /// </summary>
    public class PrototypeJob
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PartId { get; set; }

        [Required]
        [StringLength(50)]
        public string PrototypeNumber { get; set; } = string.Empty; // PROTO-001, PROTO-002

        [StringLength(100)]
        public string? CustomerOrderNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string RequestedBy { get; set; } = string.Empty;

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Standard"; // Rush, Standard, Low

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "InProgress"; // InProgress, UnderReview, Approved, Production

        // Cost Tracking
        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalActualCost { get; set; } = 0;

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalEstimatedCost { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal CostVariancePercent { get; set; } = 0;

        // Time Tracking  
        [Column(TypeName = "decimal(8,2)")]
        public decimal TotalActualHours { get; set; } = 0;

        [Column(TypeName = "decimal(8,2)")]
        public decimal TotalEstimatedHours { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal TimeVariancePercent { get; set; } = 0;

        // Completion Tracking
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public int? LeadTimeDays { get; set; }

        // Admin Review
        [StringLength(50)]
        public string AdminReviewStatus { get; set; } = "Pending"; // Pending, UnderReview, Approved, Rejected

        [StringLength(100)]
        public string? AdminReviewBy { get; set; }

        public DateTime? AdminReviewDate { get; set; }

        [StringLength(2000)]
        public string? AdminReviewNotes { get; set; }

        // Audit Fields
        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey(nameof(PartId))]
        public virtual Part? Part { get; set; }

        public virtual ICollection<ProductionStageExecution> StageExecutions { get; set; } = new List<ProductionStageExecution>();
        public virtual ICollection<AssemblyComponent> AssemblyComponents { get; set; } = new List<AssemblyComponent>();
    }
}