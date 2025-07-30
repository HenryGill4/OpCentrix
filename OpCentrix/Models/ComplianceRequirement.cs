using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Compliance Requirements for B&T Manufacturing
    /// Segment 7.1: Regulatory tracking and compliance management
    /// </summary>
    public class ComplianceRequirement
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Requirement Code")]
        public string RequirementCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Requirement Name")]
        public string RequirementName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Compliance Type")]
        public string ComplianceType { get; set; } = string.Empty; // ATF, ITAR, EAR, ISO, ASTM, etc.

        [Required]
        [StringLength(50)]
        [Display(Name = "Regulatory Authority")]
        public string RegulatoryAuthority { get; set; } = string.Empty; // ATF, State Department, Commerce, etc.

        #region Requirement Details

        [Required]
        [StringLength(1000)]
        [Display(Name = "Requirement Details")]
        public string RequirementDetails { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Documentation Required")]
        public string DocumentationRequired { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Forms Required")]
        public string FormsRequired { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Record Keeping Requirements")]
        public string RecordKeepingRequirements { get; set; } = string.Empty;

        #endregion

        #region Applicability

        [StringLength(100)]
        [Display(Name = "Applicable Industries")]
        public string ApplicableIndustries { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Applicable Part Types")]
        public string ApplicablePartTypes { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Applicable Processes")]
        public string ApplicableProcesses { get; set; } = string.Empty;

        public bool AppliesToManufacturing { get; set; } = true;
        public bool AppliesToDistribution { get; set; } = false;
        public bool AppliesToExport { get; set; } = false;
        public bool AppliesToImport { get; set; } = false;

        #endregion

        #region Enforcement Details

        [StringLength(20)]
        [Display(Name = "Enforcement Level")]
        public string EnforcementLevel { get; set; } = "Mandatory"; // Mandatory, Recommended, Optional

        [StringLength(50)]
        [Display(Name = "Penalty Type")]
        public string PenaltyType { get; set; } = string.Empty; // Civil, Criminal, Administrative

        [StringLength(500)]
        [Display(Name = "Penalty Description")]
        public string PenaltyDescription { get; set; } = string.Empty;

        public int MaxPenaltyDays { get; set; } = 0;

        [Column(TypeName = "decimal(12,2)")]
        public decimal MaxPenaltyAmount { get; set; } = 0;

        #endregion

        #region Timeline and Renewal

        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? NextReviewDate { get; set; }

        public int RenewalIntervalMonths { get; set; } = 0;
        public bool RequiresRenewal { get; set; } = false;
        public bool RequiresInspection { get; set; } = false;

        [StringLength(200)]
        [Display(Name = "Renewal Process")]
        public string RenewalProcess { get; set; } = string.Empty;

        #endregion

        #region Implementation Requirements

        [StringLength(500)]
        [Display(Name = "Implementation Steps")]
        public string ImplementationSteps { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Required Training")]
        public string RequiredTraining { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Required Certifications")]
        public string RequiredCertifications { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "System Requirements")]
        public string SystemRequirements { get; set; } = string.Empty;

        [Range(0, 1000)]
        [Display(Name = "Estimated Implementation Hours")]
        public double EstimatedImplementationHours { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Estimated Implementation Cost")]
        public decimal EstimatedImplementationCost { get; set; } = 0;

        #endregion

        #region References and Resources

        [StringLength(500)]
        [Display(Name = "Reference Documents")]
        public string ReferenceDocuments { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Web Resources")]
        public string WebResources { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Contact Information")]
        public string ContactInformation { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Additional Notes")]
        public string AdditionalNotes { get; set; } = string.Empty;

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
        public bool IsCurrentVersion { get; set; } = true;

        #endregion

        #region Foreign Keys

        /// <summary>
        /// Optional link to specific part classification
        /// </summary>
        public int? PartClassificationId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Associated part classification (if specific to a classification)
        /// </summary>
        public virtual PartClassification? PartClassification { get; set; }

        /// <summary>
        /// Serial numbers that must comply with this requirement
        /// </summary>
        public virtual ICollection<SerialNumber> SerialNumbers { get; set; } = new List<SerialNumber>();

        /// <summary>
        /// Compliance documents associated with this requirement
        /// </summary>
        public virtual ICollection<ComplianceDocument> ComplianceDocuments { get; set; } = new List<ComplianceDocument>();

        #endregion

        #region Helper Methods

        /// <summary>
        /// Check if requirement is currently active
        /// </summary>
        public bool IsCurrentlyActive()
        {
            if (!IsActive) return false;
            
            var now = DateTime.UtcNow;
            
            if (EffectiveDate.HasValue && now < EffectiveDate.Value) return false;
            if (ExpirationDate.HasValue && now > ExpirationDate.Value) return false;
            
            return true;
        }

        /// <summary>
        /// Check if requirement needs renewal soon
        /// </summary>
        public bool NeedsRenewalSoon(int daysAhead = 30)
        {
            if (!RequiresRenewal || !NextReviewDate.HasValue) return false;
            
            return NextReviewDate.Value <= DateTime.UtcNow.AddDays(daysAhead);
        }

        /// <summary>
        /// Get severity level based on enforcement and penalties
        /// </summary>
        public string GetSeverityLevel()
        {
            if (EnforcementLevel != "Mandatory") return "Low";
            if (PenaltyType == "Criminal") return "Critical";
            if (MaxPenaltyAmount > 100000 || MaxPenaltyDays > 365) return "High";
            if (MaxPenaltyAmount > 10000 || MaxPenaltyDays > 30) return "Medium";
            return "Low";
        }

        /// <summary>
        /// Get display summary
        /// </summary>
        public string GetDisplaySummary()
        {
            return $"{RequirementCode} - {RequirementName} ({ComplianceType})";
        }

        /// <summary>
        /// Check if applies to specific part type
        /// </summary>
        public bool AppliesTo(string partType, string industry, string process)
        {
            if (!IsCurrentlyActive()) return false;
            
            if (!string.IsNullOrEmpty(ApplicableIndustries) && 
                !ApplicableIndustries.Contains(industry, StringComparison.OrdinalIgnoreCase))
                return false;
                
            if (!string.IsNullOrEmpty(ApplicablePartTypes) && 
                !ApplicablePartTypes.Contains(partType, StringComparison.OrdinalIgnoreCase))
                return false;
                
            if (!string.IsNullOrEmpty(ApplicableProcesses) && 
                !ApplicableProcesses.Contains(process, StringComparison.OrdinalIgnoreCase))
                return false;
                
            return true;
        }

        #endregion
    }
}