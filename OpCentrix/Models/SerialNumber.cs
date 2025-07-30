using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Serial Number Management for B&T Manufacturing
    /// Segment 7.2: ATF Serialization Compliance and Component Traceability
    /// </summary>
    public class SerialNumber
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Serial Number")]
        public string SerialNumberValue { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Serial Number Format")]
        public string SerialNumberFormat { get; set; } = string.Empty; // BT2024-XXXXX, etc.

        [Required]
        [StringLength(50)]
        [Display(Name = "Manufacturer Code")]
        public string ManufacturerCode { get; set; } = "BT"; // B&T manufacturer identifier

        #region Assignment Details

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ManufacturedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Assigned To Job")]
        public string? AssignedJobId { get; set; }

        [StringLength(50)]
        [Display(Name = "Part Number")]
        public string? PartNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Component Name")]
        public string ComponentName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Component Type")]
        public string ComponentType { get; set; } = string.Empty; // Suppressor, Receiver, Barrel, etc.

        #endregion

        #region Manufacturing Details

        [StringLength(50)]
        [Display(Name = "Manufacturing Method")]
        public string ManufacturingMethod { get; set; } = "SLS Metal Printing";

        [StringLength(100)]
        [Display(Name = "Material Used")]
        public string MaterialUsed { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Material Lot Number")]
        public string MaterialLotNumber { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Machine Used")]
        public string MachineUsed { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Operator")]
        public string Operator { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Quality Inspector")]
        public string QualityInspector { get; set; } = string.Empty;

        #endregion

        #region ATF Compliance

        [Required]
        [StringLength(20)]
        [Display(Name = "ATF Compliance Status")]
        public string ATFComplianceStatus { get; set; } = "Pending"; // Pending, Compliant, Non-Compliant

        [StringLength(50)]
        [Display(Name = "ATF Classification")]
        public string ATFClassification { get; set; } = string.Empty; // NFA Item, Title I, Title II, etc.

        [StringLength(100)]
        [Display(Name = "FFL Dealer")]
        public string? FFLDealer { get; set; }

        [StringLength(50)]
        [Display(Name = "FFL Number")]
        public string? FFLNumber { get; set; }

        public DateTime? ATFFormSubmissionDate { get; set; }
        public DateTime? ATFApprovalDate { get; set; }

        [StringLength(100)]
        [Display(Name = "ATF Form Numbers")]
        public string ATFFormNumbers { get; set; } = string.Empty; // Form 1, Form 4, etc.

        [StringLength(50)]
        [Display(Name = "Tax Stamp Number")]
        public string? TaxStampNumber { get; set; }

        #endregion

        #region Transfer Documentation

        [StringLength(20)]
        [Display(Name = "Transfer Status")]
        public string TransferStatus { get; set; } = "In Manufacturing"; // In Manufacturing, Available, Transferred, Destroyed

        public DateTime? TransferDate { get; set; }

        [StringLength(200)]
        [Display(Name = "Transfer To")]
        public string? TransferTo { get; set; }

        [StringLength(100)]
        [Display(Name = "Transfer Document")]
        public string? TransferDocument { get; set; }

        [StringLength(500)]
        [Display(Name = "Transfer Notes")]
        public string? TransferNotes { get; set; }

        public bool IsDestructionScheduled { get; set; } = false;
        public DateTime? ScheduledDestructionDate { get; set; }
        public DateTime? ActualDestructionDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Destruction Method")]
        public string? DestructionMethod { get; set; }

        #endregion

        #region Export Control (ITAR/EAR)

        public bool IsITARControlled { get; set; } = false;
        public bool IsEARControlled { get; set; } = false;

        [StringLength(50)]
        [Display(Name = "Export Classification")]
        public string ExportClassification { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Export License")]
        public string? ExportLicense { get; set; }

        public DateTime? ExportLicenseExpiration { get; set; }

        [StringLength(100)]
        [Display(Name = "Destination Country")]
        public string? DestinationCountry { get; set; }

        [StringLength(200)]
        [Display(Name = "End User")]
        public string? EndUser { get; set; }

        public bool RequiresExportPermit { get; set; } = false;
        public bool ExportPermitObtained { get; set; } = false;

        #endregion

        #region Quality and Testing Records

        [StringLength(20)]
        [Display(Name = "Quality Status")]
        public string QualityStatus { get; set; } = "Pending"; // Pending, Pass, Fail, Conditional

        public DateTime? QualityInspectionDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Quality Certificate Number")]
        public string? QualityCertificateNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "Test Results Summary")]
        public string TestResultsSummary { get; set; } = string.Empty;

        public bool DimensionalTestPassed { get; set; } = false;
        public bool MaterialTestPassed { get; set; } = false;
        public bool PressureTestPassed { get; set; } = false;
        public bool ProofTestPassed { get; set; } = false;

        [StringLength(1000)]
        [Display(Name = "Quality Notes")]
        public string QualityNotes { get; set; } = string.Empty;

        #endregion

        #region Traceability Information

        [StringLength(2000)]
        [Display(Name = "Manufacturing History")]
        public string ManufacturingHistory { get; set; } = "{}"; // JSON data

        [StringLength(1000)]
        [Display(Name = "Component Genealogy")]
        public string ComponentGenealogy { get; set; } = string.Empty; // Parent/child relationships

        [StringLength(500)]
        [Display(Name = "Assembly Components")]
        public string AssemblyComponents { get; set; } = string.Empty; // Related serial numbers

        [StringLength(200)]
        [Display(Name = "Batch Number")]
        public string? BatchNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Build Platform ID")]
        public string? BuildPlatformId { get; set; }

        #endregion

        #region Audit Trail

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastModifiedBy { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public bool IsLocked { get; set; } = false; // Prevent changes after transfer

        #endregion

        #region Foreign Keys

        public int? PartId { get; set; }
        public int? JobId { get; set; }
        public int? ComplianceRequirementId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Associated part
        /// </summary>
        public virtual Part? Part { get; set; }

        /// <summary>
        /// Associated job
        /// </summary>
        public virtual Job? Job { get; set; }

        /// <summary>
        /// Associated compliance requirement
        /// </summary>
        public virtual ComplianceRequirement? ComplianceRequirement { get; set; }

        /// <summary>
        /// Compliance documents for this serial number
        /// </summary>
        public virtual ICollection<ComplianceDocument> ComplianceDocuments { get; set; } = new List<ComplianceDocument>();

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generate next serial number in sequence
        /// </summary>
        public static string GenerateNextSerialNumber(string format, int sequence)
        {
            var year = DateTime.UtcNow.Year;
            return format.Replace("YYYY", year.ToString())
                        .Replace("XXXXX", sequence.ToString("D5"))
                        .Replace("XXXX", sequence.ToString("D4"))
                        .Replace("XXX", sequence.ToString("D3"));
        }

        /// <summary>
        /// Validate serial number format
        /// </summary>
        public bool IsValidFormat()
        {
            if (string.IsNullOrEmpty(SerialNumberValue) || string.IsNullOrEmpty(SerialNumberFormat))
                return false;

            // Basic validation - manufacturer code should be present
            if (!SerialNumberValue.StartsWith(ManufacturerCode))
                return false;

            // Check minimum length
            if (SerialNumberValue.Length < 6)
                return false;

            return true;
        }

        /// <summary>
        /// Check if ready for transfer
        /// </summary>
        public bool IsReadyForTransfer()
        {
            return QualityStatus == "Pass" && 
                   ATFComplianceStatus == "Compliant" && 
                   !IsDestructionScheduled &&
                   TransferStatus == "Available";
        }

        /// <summary>
        /// Check if requires ATF approval
        /// </summary>
        public bool RequiresATFApproval()
        {
            return ATFClassification == "NFA Item" || 
                   ATFClassification == "Title II" ||
                   ComponentType == "Suppressor" ||
                   ComponentType == "Receiver";
        }

        /// <summary>
        /// Get compliance status summary
        /// </summary>
        public string GetComplianceStatusSummary()
        {
            var statuses = new List<string>();
            
            if (ATFComplianceStatus != "Compliant") statuses.Add($"ATF: {ATFComplianceStatus}");
            if (IsITARControlled && string.IsNullOrEmpty(ExportLicense)) statuses.Add("ITAR: No License");
            if (IsEARControlled && !ExportPermitObtained) statuses.Add("EAR: No Permit");
            if (QualityStatus != "Pass") statuses.Add($"Quality: {QualityStatus}");
            
            return statuses.Count > 0 ? string.Join(", ", statuses) : "Compliant";
        }

        /// <summary>
        /// Get manufacturing summary
        /// </summary>
        public string GetManufacturingSummary()
        {
            return $"{ManufacturingMethod} - {MaterialUsed} (Lot: {MaterialLotNumber}) - {MachineUsed}";
        }

        /// <summary>
        /// Lock serial number to prevent changes
        /// </summary>
        public void Lock(string reason, string lockedBy)
        {
            IsLocked = true;
            LastModifiedBy = lockedBy;
            LastModifiedDate = DateTime.UtcNow;
            // Could add lock reason to notes or separate field
        }

        #endregion
    }
}