using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Represents a laser engraving operation for firearms and suppressor serialization
    /// Includes ATF compliance tracking and audit trail requirements
    /// </summary>
    public class LaserEngravingOperation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string RequestId { get; set; } = string.Empty; // REQ-YYYY-NNN format

        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ComponentType { get; set; } = string.Empty; // Firearm, Suppressor, Barrel, etc.

        [Required]
        [StringLength(100)]
        public string ATFClassification { get; set; } = string.Empty; // NFA Item, Title I Firearm, etc.

        [Required]
        [StringLength(50)]
        public string FormType { get; set; } = string.Empty; // Form 1, Form 4, etc.

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? FFLNumber { get; set; }

        [Required]
        [Range(1, 50)]
        public int Quantity { get; set; } = 1;

        [Required]
        [StringLength(100)]
        public string Material { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CaliberThreadPitch { get; set; }

        [Required]
        [StringLength(100)]
        public string EngravingPosition { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? SpecialInstructions { get; set; }

        // Engraving Details
        [StringLength(50)]
        public string? SerialNumber { get; set; }

        [StringLength(100)]
        public string? ManufacturerMark { get; set; }

        [StringLength(100)]
        public string? ModelDesignation { get; set; }

        [StringLength(50)]
        public string? CaliberMarking { get; set; }

        [Range(0, 10)]
        public double EngravingDepth { get; set; } = 0; // in thousandths of an inch

        [StringLength(50)]
        public string? FontSize { get; set; }

        [StringLength(50)]
        public string? FontStyle { get; set; }

        // Equipment and Process
        [StringLength(50)]
        public string? LaserEquipmentId { get; set; }

        [StringLength(100)]
        public string? LaserType { get; set; } // Fiber, CO2, UV, etc.

        [Range(0, 100)]
        public double LaserPowerPercent { get; set; } = 0;

        [Range(0, 1000)]
        public double LaserSpeedMmPerMin { get; set; } = 0;

        [Range(1, 10)]
        public int PassCount { get; set; } = 1;

        // Status and Timeline
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Requested"; // Requested, Approved, In Progress, QC Review, Completed, Rejected

        [Required]
        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedDate { get; set; }

        public DateTime? StartedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public DateTime? QCApprovedDate { get; set; }

        [Required]
        [StringLength(100)]
        public string RequestedBy { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ApprovedBy { get; set; }

        [StringLength(100)]
        public string? OperatorName { get; set; }

        [StringLength(100)]
        public string? QCInspector { get; set; }

        // ATF Compliance
        public bool ATFFormRequired { get; set; } = true;

        [StringLength(100)]
        public string? ATFFormNumber { get; set; }

        public DateTime? ATFApprovalDate { get; set; }

        public bool ITARControlled { get; set; } = false;

        public bool EARControlled { get; set; } = false;

        [StringLength(500)]
        public string? ComplianceNotes { get; set; }

        // Quality Control
        public bool RequiresQCInspection { get; set; } = true;

        [Range(0, 100)]
        public double QCScore { get; set; } = 0;

        [StringLength(1000)]
        public string? QCNotes { get; set; }

        public bool QCPassed { get; set; } = false;

        [StringLength(500)]
        public string? QCRejectionReason { get; set; }

        // Cost Tracking
        [Column(TypeName = "decimal(10,2)")]
        public decimal LaborCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal MaterialCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal EquipmentCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ComplianceCost { get; set; } = 0;

        // Audit Trail
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastModifiedBy { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? AuditLog { get; set; } // JSON log of all changes

        // Navigation properties
        public virtual Part? Part { get; set; }

        // Computed properties
        [NotMapped]
        public string StatusColor => Status switch
        {
            "Completed" => "#10B981", // green
            "In Progress" => "#F59E0B", // amber
            "QC Review" => "#8B5CF6", // purple
            "Approved" => "#06B6D4", // cyan
            "Rejected" => "#EF4444", // red
            "Requested" => "#6B7280", // gray
            _ => "#3B82F6" // blue
        };

        [NotMapped]
        public decimal TotalCost => LaborCost + MaterialCost + EquipmentCost + ComplianceCost;

        [NotMapped]
        public double CompletionRate => Status switch
        {
            "Completed" => 100,
            "QC Review" => 90,
            "In Progress" => 50,
            "Approved" => 25,
            "Requested" => 10,
            _ => 0
        };

        [NotMapped]
        public TimeSpan? ProcessingTime => StartedDate.HasValue && CompletedDate.HasValue 
            ? CompletedDate.Value - StartedDate.Value 
            : null;

        [NotMapped]
        public TimeSpan? TotalLeadTime => CompletedDate.HasValue 
            ? CompletedDate.Value - RequestedDate 
            : null;

        [NotMapped]
        public bool IsCompliant => ATFFormRequired ? !string.IsNullOrEmpty(ATFFormNumber) : true;

        [NotMapped]
        public bool IsExpedited => (DateTime.UtcNow - RequestedDate).TotalDays < 1;

        // Helper methods
        public bool RequiresATFApproval()
        {
            return FormType == "Form 1" || FormType == "Form 4" || ATFFormRequired;
        }

        public bool RequiresExportLicense()
        {
            return ITARControlled || EARControlled;
        }

        public string GetComplianceStatus()
        {
            if (!IsCompliant)
                return "Non-Compliant";
            
            if (RequiresATFApproval() && !ATFApprovalDate.HasValue)
                return "Pending ATF";
            
            if (RequiresExportLicense() && string.IsNullOrEmpty(ComplianceNotes))
                return "Pending Export Review";
                
            return "Compliant";
        }

        public void AddAuditLogEntry(string action, string details, string userId)
        {
            var entry = new
            {
                Timestamp = DateTime.UtcNow,
                Action = action,
                Details = details,
                UserId = userId
            };
            
            var currentLog = string.IsNullOrEmpty(AuditLog) ? "[]" : AuditLog;
            // In a real implementation, you would deserialize, add the entry, and reserialize
            // For demo purposes, this is simplified
        }
    }

    // Supporting enums for laser engraving operations
    public enum LaserEngravingStatus
    {
        Requested,
        ATFReview,
        Approved,
        InProgress,
        QCReview,
        QCFailed,
        Completed,
        Rejected,
        OnHold
    }

    public enum LaserType
    {
        Fiber,
        CO2,
        UV,
        Diode,
        YAG
    }

    public enum EngravingComponentType
    {
        FirearmReceiver,
        Suppressor,
        BarrelAssembly,
        Frame,
        TriggerGroup,
        Bolt,
        Other
    }

    public enum ATFFormType
    {
        None,
        Form1,
        Form4,
        Form2,
        Form3,
        Form6,
        Form10
    }
}