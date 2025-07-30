using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Tracks assembly components needed for prototype jobs
    /// Manages end caps, springs, baffles, mounting hardware, etc.
    /// </summary>
    public class AssemblyComponent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PrototypeJobId { get; set; }

        // Component Details
        [Required]
        [StringLength(50)]
        public string ComponentType { get; set; } = string.Empty; // EndCap, Spring, Baffle, Hardware

        [StringLength(100)]
        public string? ComponentPartNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string ComponentDescription { get; set; } = string.Empty;

        // Quantity & Cost
        [Required]  
        public int QuantityRequired { get; set; } = 1;

        public int QuantityUsed { get; set; } = 0;

        [Column(TypeName = "decimal(8,2)")]
        public decimal? UnitCost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalCost { get; set; }

        // Sourcing
        [StringLength(100)]
        public string? Supplier { get; set; }

        [StringLength(100)]
        public string? SupplierPartNumber { get; set; }

        public int? LeadTimeDays { get; set; }

        // Status
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Needed"; // Needed, Ordered, Received, Used

        public DateTime? OrderDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public DateTime? UsedDate { get; set; }

        // Quality
        public bool InspectionRequired { get; set; } = false;
        public bool? InspectionPassed { get; set; }

        [StringLength(500)]
        public string? InspectionNotes { get; set; }

        // Audit Fields
        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey(nameof(PrototypeJobId))]
        public virtual PrototypeJob? PrototypeJob { get; set; }
    }
}