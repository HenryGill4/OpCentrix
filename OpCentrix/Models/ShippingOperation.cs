using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Represents a shipping operation for part dispatch
    /// </summary>
    public class ShippingOperation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ShipmentNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CustomerOrderNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Packed, Shipped, Delivered

        [Required]
        public DateTime ScheduledShipDate { get; set; }

        public DateTime? ActualShipDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        [Required]
        [StringLength(100)]
        public string ShippingMethod { get; set; } = string.Empty; // UPS, FedEx, DHL, etc.

        [StringLength(100)]
        public string TrackingNumber { get; set; } = string.Empty;

        [StringLength(50)]
        public string Priority { get; set; } = "Standard"; // Rush, Standard, Expedited

        // Customer information
        [Required]
        [StringLength(200)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Country { get; set; } = "USA";

        [StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [StringLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        [StringLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        // Package information
        [Range(0, 1000)]
        public double WeightKg { get; set; } = 0;

        [Range(0, 1000)]
        public double LengthCm { get; set; } = 0;

        [Range(0, 1000)]
        public double WidthCm { get; set; } = 0;

        [Range(0, 1000)]
        public double HeightCm { get; set; } = 0;

        [StringLength(50)]
        public string PackagingType { get; set; } = string.Empty; // Box, Crate, Envelope, etc.

        [StringLength(1000)]
        public string PackagingNotes { get; set; } = string.Empty;

        // Cost and insurance
        [Column(TypeName = "decimal(10,2)")]
        public decimal ShippingCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal InsuranceValue { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal CustomsDeclaredValue { get; set; } = 0;

        public bool RequiresSignature { get; set; } = false;
        public bool RequiresInsurance { get; set; } = false;
        public bool IsInternational { get; set; } = false;

        // Documentation
        [StringLength(1000)]
        public string SpecialInstructions { get; set; } = string.Empty;

        [StringLength(1000)]
        public string PackingSlipNotes { get; set; } = string.Empty;

        [StringLength(500)]
        public string CustomsDocumentation { get; set; } = string.Empty;

        // Quality and inspection
        [StringLength(100)]
        public string PackedBy { get; set; } = string.Empty;

        [StringLength(100)]
        public string InspectedBy { get; set; } = string.Empty;

        public DateTime? PackingDate { get; set; }
        public DateTime? InspectionDate { get; set; }

        [StringLength(1000)]
        public string QualityCheckNotes { get; set; } = string.Empty;

        // Audit trail
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastModifiedBy { get; set; } = string.Empty;

        // Navigation properties - Collection of parts being shipped
        public virtual ICollection<ShippingItem> ShippingItems { get; set; } = new List<ShippingItem>();

        // Computed properties
        [NotMapped]
        public string StatusColor => Status switch
        {
            "Delivered" => "#10B981", // green
            "Shipped" => "#3B82F6", // blue
            "Packed" => "#F59E0B", // amber
            "Pending" => "#6B7280", // gray
            _ => "#EF4444" // red
        };

        [NotMapped]
        public string PriorityColor => Priority switch
        {
            "Rush" => "#EF4444", // red
            "Expedited" => "#F59E0B", // amber
            "Standard" => "#10B981", // green
            _ => "#6B7280" // gray
        };

        [NotMapped]
        public bool IsOverdue => ScheduledShipDate < DateTime.Today && Status != "Shipped" && Status != "Delivered";

        [NotMapped]
        public int DaysToShip => (ScheduledShipDate.Date - DateTime.Today).Days;

        [NotMapped]
        public double VolumetricWeight => (LengthCm * WidthCm * HeightCm) / 5000; // Kg

        [NotMapped]
        public int TotalItems => ShippingItems.Sum(si => si.Quantity);

        [NotMapped]
        public decimal TotalValue => ShippingItems.Sum(si => si.UnitValue * si.Quantity);
    }

    /// <summary>
    /// Represents individual items within a shipment
    /// </summary>
    public class ShippingItem
    {
        public int Id { get; set; }

        [Required]
        public int ShippingOperationId { get; set; }

        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 10000)]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitValue { get; set; } = 0;

        [StringLength(100)]
        public string SerialNumbers { get; set; } = string.Empty; // Comma-separated if multiple

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual ShippingOperation ShippingOperation { get; set; } = null!;
        public virtual Part? Part { get; set; }

        // Computed properties
        [NotMapped]
        public decimal TotalValue => UnitValue * Quantity;
    }
}