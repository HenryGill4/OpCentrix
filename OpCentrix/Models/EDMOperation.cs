using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Represents an EDM (Electrical Discharge Machining) operation
    /// </summary>
    public class EDMOperation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string WorkPieceDescription { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Material { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string EDMType { get; set; } = string.Empty; // Wire EDM, Sinker EDM, Hole Drilling EDM

        [Required]
        [StringLength(50)]
        public string MachineId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Setup"; // Setup, Cutting, Completed, On Hold

        [Required]
        public DateTime ScheduledDate { get; set; }

        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }

        [Required]
        [StringLength(100)]
        public string Operator { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;

        public int CompletedQuantity { get; set; } = 0;

        // EDM Process Parameters
        [StringLength(50)]
        public string WireType { get; set; } = string.Empty; // For Wire EDM

        [Range(0, 1000)]
        public double WireDiameter { get; set; } = 0; // microns

        [Range(0, 100)]
        public double Current { get; set; } = 0; // Amperes

        [Range(0, 1000)]
        public double Voltage { get; set; } = 0; // Volts

        [Range(0, 100)]
        public double PulseOnTime { get; set; } = 0; // microseconds

        [Range(0, 100)]
        public double PulseOffTime { get; set; } = 0; // microseconds

        [StringLength(50)]
        public string DielectricFluid { get; set; } = string.Empty;

        [Range(0, 100)]
        public double FluidTemperature { get; set; } = 20.0; // Celsius

        [Range(0, 1000)]
        public double FlowRate { get; set; } = 0; // L/min

        // Quality and measurements
        [Range(0, 100)]
        public double SurfaceFinishRa { get; set; } = 0; // Surface roughness in microns

        [Range(0, 1000)]
        public double ToleranceAchieved { get; set; } = 0; // microns

        [StringLength(1000)]
        public string QualityNotes { get; set; } = string.Empty;

        // Process tracking
        [Range(0, 1000)]
        public double EstimatedCuttingTime { get; set; } = 0; // hours

        [Range(0, 1000)]
        public double ActualCuttingTime { get; set; } = 0; // hours

        [Range(0, 100)]
        public double MachineUtilization { get; set; } = 0; // percentage

        [StringLength(1000)]
        public string ProcessNotes { get; set; } = string.Empty;

        [StringLength(500)]
        public string HoldReason { get; set; } = string.Empty;

        // Cost tracking
        [Column(TypeName = "decimal(10,2)")]
        public decimal WireCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ElectricityCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal LaborCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal MachineCost { get; set; } = 0;

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
            "Cutting" => "#F59E0B", // amber
            "On Hold" => "#EF4444", // red
            "Setup" => "#6B7280", // gray
            _ => "#3B82F6" // blue
        };

        [NotMapped]
        public decimal TotalCost => WireCost + ElectricityCost + LaborCost + MachineCost;

        [NotMapped]
        public double CompletionRate => Quantity > 0 ? (double)CompletedQuantity / Quantity * 100 : 0;

        [NotMapped]
        public double EfficiencyRate => EstimatedCuttingTime > 0 && ActualCuttingTime > 0 
            ? EstimatedCuttingTime / ActualCuttingTime * 100 
            : 0;

        [NotMapped]
        public TimeSpan? ProcessingTime => ActualStartDate.HasValue && ActualCompletionDate.HasValue 
            ? ActualCompletionDate.Value - ActualStartDate.Value 
            : null;
    }
}