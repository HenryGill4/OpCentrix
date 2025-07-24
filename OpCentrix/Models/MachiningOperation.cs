using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Represents a machining operation for parts processing
    /// </summary>
    public class MachiningOperation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string WorkOrderNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string MachineId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string MachineType { get; set; } = string.Empty; // CNC Mill, CNC Lathe, Drill Press, etc.

        [Required]
        [StringLength(100)]
        public string Operation { get; set; } = string.Empty; // Milling, Turning, Drilling, Tapping, etc.

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Setup"; // Setup, Running, Completed, On Hold, Quality Check

        [Required]
        public DateTime ScheduledDate { get; set; }

        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }

        [Required]
        [StringLength(100)]
        public string Operator { get; set; } = string.Empty;

        [StringLength(100)]
        public string Programmer { get; set; } = string.Empty; // For CNC operations

        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;

        public int CompletedQuantity { get; set; } = 0;
        public int RejectedQuantity { get; set; } = 0;

        // Machining parameters
        [StringLength(100)]
        public string Material { get; set; } = string.Empty;

        [StringLength(100)]
        public string CuttingTool { get; set; } = string.Empty;

        [Range(0, 50000)]
        public double SpindleSpeed { get; set; } = 0; // RPM

        [Range(0, 5000)]
        public double FeedRate { get; set; } = 0; // mm/min

        [Range(0, 50)]
        public double DepthOfCut { get; set; } = 0; // mm

        [Range(0, 1000)]
        public double CuttingSpeed { get; set; } = 0; // m/min

        [StringLength(100)]
        public string CoolantType { get; set; } = string.Empty;

        [Range(0, 1000)]
        public double CoolantFlowRate { get; set; } = 0; // L/min

        // Program and tooling
        [StringLength(100)]
        public string ProgramNumber { get; set; } = string.Empty;

        [StringLength(1000)]
        public string ToolList { get; set; } = string.Empty; // JSON or comma-separated

        [StringLength(1000)]
        public string SetupNotes { get; set; } = string.Empty;

        [StringLength(1000)]
        public string ProcessNotes { get; set; } = string.Empty;

        // Quality and measurements
        [Range(0, 1000)]
        public double ToleranceRequired { get; set; } = 0; // microns

        [Range(0, 1000)]
        public double ToleranceAchieved { get; set; } = 0; // microns

        [Range(0, 100)]
        public double SurfaceFinishRequired { get; set; } = 0; // Ra in microns

        [Range(0, 100)]
        public double SurfaceFinishAchieved { get; set; } = 0; // Ra in microns

        [StringLength(1000)]
        public string QualityNotes { get; set; } = string.Empty;

        [StringLength(100)]
        public string InspectedBy { get; set; } = string.Empty;

        public DateTime? InspectionDate { get; set; }

        // Time tracking
        [Range(0, 1000)]
        public double EstimatedSetupTime { get; set; } = 0; // hours

        [Range(0, 1000)]
        public double ActualSetupTime { get; set; } = 0; // hours

        [Range(0, 1000)]
        public double EstimatedCycleTime { get; set; } = 0; // hours per part

        [Range(0, 1000)]
        public double ActualCycleTime { get; set; } = 0; // hours per part

        [Range(0, 100)]
        public double MachineUtilization { get; set; } = 0; // percentage

        [StringLength(500)]
        public string HoldReason { get; set; } = string.Empty;

        // Cost tracking
        [Column(TypeName = "decimal(10,2)")]
        public decimal ToolingCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal LaborCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal MachineCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal MaterialCost { get; set; } = 0;

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
            "Running" => "#F59E0B", // amber
            "Quality Check" => "#3B82F6", // blue
            "On Hold" => "#EF4444", // red
            "Setup" => "#6B7280", // gray
            _ => "#8B5CF6" // purple
        };

        [NotMapped]
        public decimal TotalCost => ToolingCost + LaborCost + MachineCost + MaterialCost;

        [NotMapped]
        public double CompletionRate => Quantity > 0 ? (double)CompletedQuantity / Quantity * 100 : 0;

        [NotMapped]
        public double QualityRate => CompletedQuantity > 0 ? (double)(CompletedQuantity - RejectedQuantity) / CompletedQuantity * 100 : 100;

        [NotMapped]
        public double SetupEfficiency => EstimatedSetupTime > 0 && ActualSetupTime > 0 
            ? EstimatedSetupTime / ActualSetupTime * 100 
            : 0;

        [NotMapped]
        public double CycleEfficiency => EstimatedCycleTime > 0 && ActualCycleTime > 0 
            ? EstimatedCycleTime / ActualCycleTime * 100 
            : 0;

        [NotMapped]
        public TimeSpan? ProcessingTime => ActualStartDate.HasValue && ActualCompletionDate.HasValue 
            ? ActualCompletionDate.Value - ActualStartDate.Value 
            : null;

        [NotMapped]
        public double TotalEstimatedTime => EstimatedSetupTime + (EstimatedCycleTime * Quantity);

        [NotMapped]
        public double TotalActualTime => ActualSetupTime + (ActualCycleTime * CompletedQuantity);
    }
}