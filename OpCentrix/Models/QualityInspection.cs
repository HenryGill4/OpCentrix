using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Represents a quality control inspection
    /// </summary>
    public class QualityInspection
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string InspectionNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string InspectionType { get; set; } = string.Empty; // Incoming, In-Process, Final, First Article

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Pass, Fail, Hold

        [Required]
        public DateTime ScheduledDate { get; set; }

        public DateTime? ActualInspectionDate { get; set; }

        [Required]
        [StringLength(100)]
        public string Inspector { get; set; } = string.Empty;

        [StringLength(100)]
        public string CustomerOrderNumber { get; set; } = string.Empty;

        [Range(1, 10000)]
        public int QuantityToInspect { get; set; } = 1;

        public int QuantityInspected { get; set; } = 0;
        public int QuantityPassed { get; set; } = 0;
        public int QuantityFailed { get; set; } = 0;
        public int QuantityOnHold { get; set; } = 0;

        // Inspection criteria and results
        [StringLength(1000)]
        public string DimensionalRequirements { get; set; } = string.Empty;

        [StringLength(1000)]
        public string DimensionalResults { get; set; } = string.Empty;

        [StringLength(1000)]
        public string VisualRequirements { get; set; } = string.Empty;

        [StringLength(1000)]
        public string VisualResults { get; set; } = string.Empty;

        [StringLength(1000)]
        public string FunctionalRequirements { get; set; } = string.Empty;

        [StringLength(1000)]
        public string FunctionalResults { get; set; } = string.Empty;

        // Surface finish and material properties
        [Range(0, 100)]
        public double SurfaceFinishRequired { get; set; } = 0; // Ra in microns

        [Range(0, 100)]
        public double SurfaceFinishMeasured { get; set; } = 0; // Ra in microns

        [Range(0, 1000)]
        public double HardnessRequired { get; set; } = 0; // HRC, HRB, etc.

        [Range(0, 1000)]
        public double HardnessMeasured { get; set; } = 0;

        [StringLength(50)]
        public string HardnessScale { get; set; } = string.Empty; // HRC, HRB, HV, etc.

        // Dimensional measurements
        [StringLength(2000)]
        public string CriticalDimensions { get; set; } = string.Empty; // JSON format

        [StringLength(2000)]
        public string MeasuredDimensions { get; set; } = string.Empty; // JSON format

        [StringLength(1000)]
        public string ToleranceAnalysis { get; set; } = string.Empty;

        // Testing and certification
        [StringLength(100)]
        public string TestProcedure { get; set; } = string.Empty;

        [StringLength(100)]
        public string TestEquipment { get; set; } = string.Empty;

        [StringLength(100)]
        public string CalibrationDate { get; set; } = string.Empty;

        [StringLength(1000)]
        public string TestResults { get; set; } = string.Empty;

        [StringLength(1000)]
        public string CertificationRequirements { get; set; } = string.Empty;

        public bool RequiresCertificate { get; set; } = false;
        public bool CertificateGenerated { get; set; } = false;

        [StringLength(100)]
        public string CertificateNumber { get; set; } = string.Empty;

        // Non-conformance and corrective action
        [StringLength(1000)]
        public string NonConformanceDescription { get; set; } = string.Empty;

        [StringLength(1000)]
        public string CorrectiveAction { get; set; } = string.Empty;

        [StringLength(100)]
        public string DispositionCode { get; set; } = string.Empty; // Accept, Reject, Rework, Use As Is

        [StringLength(1000)]
        public string DispositionNotes { get; set; } = string.Empty;

        // Documentation and traceability
        [StringLength(1000)]
        public string InspectionNotes { get; set; } = string.Empty;

        [StringLength(1000)]
        public string CustomerRequirements { get; set; } = string.Empty;

        [StringLength(500)]
        public string DrawingRevision { get; set; } = string.Empty;

        [StringLength(500)]
        public string SpecificationRevision { get; set; } = string.Empty;

        [StringLength(1000)]
        public string TraceabilityInfo { get; set; } = string.Empty;

        // Environmental conditions
        [Range(0, 50)]
        public double Temperature { get; set; } = 20.0; // Celsius

        [Range(0, 100)]
        public double Humidity { get; set; } = 50.0; // Percentage

        [Range(900, 1100)]
        public double Pressure { get; set; } = 1013.25; // hPa

        // Time tracking
        [Range(0, 100)]
        public double EstimatedInspectionTime { get; set; } = 0; // hours

        [Range(0, 100)]
        public double ActualInspectionTime { get; set; } = 0; // hours

        // Cost tracking
        [Column(TypeName = "decimal(10,2)")]
        public decimal InspectionCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TestingCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal CertificationCost { get; set; } = 0;

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
            "Pass" => "#10B981", // green
            "In Progress" => "#F59E0B", // amber
            "Fail" => "#EF4444", // red
            "Hold" => "#F97316", // orange
            "Pending" => "#6B7280", // gray
            _ => "#3B82F6" // blue
        };

        [NotMapped]
        public decimal TotalCost => InspectionCost + TestingCost + CertificationCost;

        [NotMapped]
        public double PassRate => QuantityInspected > 0 ? (double)QuantityPassed / QuantityInspected * 100 : 0;

        [NotMapped]
        public double FailRate => QuantityInspected > 0 ? (double)QuantityFailed / QuantityInspected * 100 : 0;

        [NotMapped]
        public double CompletionRate => QuantityToInspect > 0 ? (double)QuantityInspected / QuantityToInspect * 100 : 0;

        [NotMapped]
        public double InspectionEfficiency => EstimatedInspectionTime > 0 && ActualInspectionTime > 0 
            ? EstimatedInspectionTime / ActualInspectionTime * 100 
            : 0;

        [NotMapped]
        public bool IsOverdue => ScheduledDate < DateTime.Today && Status == "Pending";

        [NotMapped]
        public int DaysOverdue => IsOverdue ? (DateTime.Today - ScheduledDate.Date).Days : 0;

        [NotMapped]
        public bool RequiresAction => Status == "Fail" || Status == "Hold" || IsOverdue;

        [NotMapped]
        public string OverallResult => QuantityFailed > 0 ? "FAIL" : QuantityOnHold > 0 ? "HOLD" : QuantityPassed > 0 ? "PASS" : "PENDING";
    }
}