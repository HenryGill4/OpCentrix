using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// B&T Manufacturing Part Classification System
    /// Segment 7.1: Firearms/Suppressor-Specific Part Categories and Classifications
    /// </summary>
    public class PartClassification
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Classification Code")]
        public string ClassificationCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Classification Name")]
        public string ClassificationName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Industry Type")]
        public string IndustryType { get; set; } = "Firearms"; // Firearms, Suppressor, General

        [Required]
        [StringLength(50)]
        [Display(Name = "Component Category")]
        public string ComponentCategory { get; set; } = string.Empty;

        #region Suppressor Component Categories
        
        [StringLength(50)]
        [Display(Name = "Suppressor Type")]
        public string? SuppressorType { get; set; } // Rifle, Pistol, Multi-Caliber, etc.

        [StringLength(50)]
        [Display(Name = "Baffle Position")]
        public string? BafflePosition { get; set; } // Front, Middle, Rear, N/A

        public bool IsEndCap { get; set; } = false;
        public bool IsThreadMount { get; set; } = false;
        public bool IsTubeHousing { get; set; } = false;
        public bool IsInternalComponent { get; set; } = false;
        public bool IsMountingHardware { get; set; } = false;

        #endregion

        #region Firearm Part Categories

        [StringLength(50)]
        [Display(Name = "Firearm Type")]
        public string? FirearmType { get; set; } // Rifle, Pistol, SBR, etc.

        public bool IsReceiver { get; set; } = false;
        public bool IsBarrelComponent { get; set; } = false;
        public bool IsOperatingSystem { get; set; } = false;
        public bool IsSafetyComponent { get; set; } = false;
        public bool IsTriggerComponent { get; set; } = false;
        public bool IsFurniture { get; set; } = false;

        #endregion

        #region Material Classifications

        [Required]
        [StringLength(100)]
        [Display(Name = "Recommended Material")]
        public string RecommendedMaterial { get; set; } = "Ti-6Al-4V Grade 5";

        [StringLength(200)]
        [Display(Name = "Alternative Materials")]
        public string AlternativeMaterials { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Material Grade")]
        public string MaterialGrade { get; set; } = "Aerospace";

        public bool RequiresSpecialHandling { get; set; } = false;

        #endregion

        #region Manufacturing Requirements

        [StringLength(100)]
        [Display(Name = "Required Process")]
        public string RequiredProcess { get; set; } = "SLS Metal Printing";

        [StringLength(200)]
        [Display(Name = "Post-Processing Required")]
        public string PostProcessingRequired { get; set; } = string.Empty;

        [Range(1, 10)]
        [Display(Name = "Complexity Level")]
        public int ComplexityLevel { get; set; } = 3; // 1=Simple, 10=Very Complex

        [StringLength(500)]
        [Display(Name = "Special Instructions")]
        public string SpecialInstructions { get; set; } = string.Empty;

        #endregion

        #region Quality and Compliance Requirements

        public bool RequiresPressureTesting { get; set; } = false;
        public bool RequiresProofTesting { get; set; } = false;
        public bool RequiresDimensionalVerification { get; set; } = true;
        public bool RequiresSurfaceFinishVerification { get; set; } = true;
        public bool RequiresMaterialCertification { get; set; } = true;

        [StringLength(500)]
        [Display(Name = "Testing Requirements")]
        public string TestingRequirements { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Quality Standards")]
        public string QualityStandards { get; set; } = string.Empty;

        #endregion

        #region Regulatory Compliance

        public bool RequiresATFCompliance { get; set; } = false;
        public bool RequiresITARCompliance { get; set; } = false;
        public bool RequiresFFLTracking { get; set; } = false;
        public bool RequiresSerialization { get; set; } = false;
        public bool IsControlledItem { get; set; } = false;
        public bool IsEARControlled { get; set; } = false;

        [StringLength(50)]
        [Display(Name = "Export Classification")]
        public string ExportClassification { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Regulatory Notes")]
        public string RegulatoryNotes { get; set; } = string.Empty;

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

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Parts that use this classification
        /// </summary>
        public virtual ICollection<Part> Parts { get; set; } = new List<Part>();

        /// <summary>
        /// Compliance requirements associated with this classification
        /// </summary>
        public virtual ICollection<ComplianceRequirement> ComplianceRequirements { get; set; } = new List<ComplianceRequirement>();

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get display name for the classification
        /// </summary>
        public string GetDisplayName()
        {
            return $"{ClassificationCode} - {ClassificationName}";
        }

        /// <summary>
        /// Check if this classification is for suppressor components
        /// </summary>
        public bool IsSuppressorClassification()
        {
            return IndustryType == "Suppressor" || ComponentCategory.Contains("Suppressor") ||
                   IsEndCap || IsThreadMount || IsTubeHousing || !string.IsNullOrEmpty(BafflePosition);
        }

        /// <summary>
        /// Check if this classification is for firearm components
        /// </summary>
        public bool IsFirearmClassification()
        {
            return IndustryType == "Firearms" || ComponentCategory.Contains("Firearm") ||
                   IsReceiver || IsBarrelComponent || IsOperatingSystem || IsSafetyComponent;
        }

        /// <summary>
        /// Get all required compliance types
        /// </summary>
        public List<string> GetRequiredCompliance()
        {
            var compliance = new List<string>();
            
            if (RequiresATFCompliance) compliance.Add("ATF");
            if (RequiresITARCompliance) compliance.Add("ITAR");
            if (RequiresFFLTracking) compliance.Add("FFL");
            if (RequiresSerialization) compliance.Add("Serialization");
            if (IsControlledItem) compliance.Add("Controlled Item");
            if (IsEARControlled) compliance.Add("EAR");
            
            return compliance;
        }

        /// <summary>
        /// Get all required testing types
        /// </summary>
        public List<string> GetRequiredTesting()
        {
            var testing = new List<string>();
            
            if (RequiresPressureTesting) testing.Add("Pressure Testing");
            if (RequiresProofTesting) testing.Add("Proof Testing");
            if (RequiresDimensionalVerification) testing.Add("Dimensional Verification");
            if (RequiresSurfaceFinishVerification) testing.Add("Surface Finish");
            if (RequiresMaterialCertification) testing.Add("Material Certification");
            
            return testing;
        }

        /// <summary>
        /// Get complexity description
        /// </summary>
        public string GetComplexityDescription()
        {
            return ComplexityLevel switch
            {
                1 or 2 => "Simple",
                3 or 4 => "Standard",
                5 or 6 => "Moderate",
                7 or 8 => "Complex",
                9 or 10 => "Very Complex",
                _ => "Unknown"
            };
        }

        #endregion
    }
}