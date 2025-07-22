using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    public class Part
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Material { get; set; } = string.Empty;
        
        // Duration estimates
        public string AvgDuration { get; set; } = "1h 0m";
        public int AvgDurationDays { get; set; } = 1;
        public double EstimatedHours { get; set; } = 8.0;
        
        // Enhanced part data for analytics
        
        #region Physical Properties
        
        public double WeightKg { get; set; } = 0;
        public string Dimensions { get; set; } = string.Empty; // "L x W x H"
        public double VolumeM3 { get; set; } = 0;
        
        #endregion
        
        #region Cost Data
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal MaterialCostPerUnit { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal StandardLaborCostPerHour { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal SetupCost { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal ToolingCost { get; set; } = 0;
        
        #endregion
        
        #region Manufacturing Data
        
        // Process information
        public string ProcessType { get; set; } = string.Empty; // SLS, SLA, FDM, etc.
        public string RequiredMachineType { get; set; } = string.Empty;
        public string PreferredMachines { get; set; } = string.Empty; // Comma-separated list
        
        // Setup and changeover
        public double SetupTimeMinutes { get; set; } = 30;
        public double ChangeoverTimeMinutes { get; set; } = 15;
        
        // Quality requirements
        public string QualityStandards { get; set; } = string.Empty;
        public string ToleranceRequirements { get; set; } = string.Empty;
        public bool RequiresInspection { get; set; } = false;
        
        #endregion
        
        #region Resource Requirements
        
        public string RequiredSkills { get; set; } = string.Empty;
        public string RequiredCertifications { get; set; } = string.Empty;
        public string RequiredTooling { get; set; } = string.Empty;
        public string ConsumableMaterials { get; set; } = string.Empty;
        
        #endregion
        
        #region Customer and Classification
        
        public string CustomerPartNumber { get; set; } = string.Empty;
        public string PartCategory { get; set; } = string.Empty; // Prototype, Production, etc.
        public string PartClass { get; set; } = string.Empty; // A, B, C classification
        public bool IsActive { get; set; } = true;
        
        #endregion
        
        #region Historical Performance Data
        
        // Historical averages (calculated from completed jobs)
        public double AverageActualHours { get; set; } = 0;
        public double AverageEfficiencyPercent { get; set; } = 100;
        public double AverageQualityScore { get; set; } = 100;
        public double AverageDefectRate { get; set; } = 0;
        
        // Volume data
        public int TotalJobsCompleted { get; set; } = 0;
        public int TotalUnitsProduced { get; set; } = 0;
        public DateTime? LastProduced { get; set; }
        
        #endregion
        
        #region Process Parameters
        
        // Process-specific data (stored as JSON for flexibility)
        public string ProcessParameters { get; set; } = "{}";
        public string QualityCheckpoints { get; set; } = "{}";
        
        #endregion
        
        #region Audit Trail
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public string LastModifiedBy { get; set; } = string.Empty;
        
        #endregion
        
        #region Computed Properties (NotMapped)
        
        // Cost calculations
        [NotMapped]
        public decimal EstimatedTotalCostPerUnit => 
            MaterialCostPerUnit + 
            (StandardLaborCostPerHour * (decimal)EstimatedHours) + 
            (TotalUnitsProduced > 0 ? SetupCost / Math.Max(1, TotalUnitsProduced) : 0) + // Amortize setup cost
            ToolingCost;
        
        // Performance indicators
        [NotMapped]
        public double EstimateAccuracy => AverageActualHours > 0 
            ? Math.Round((EstimatedHours / AverageActualHours) * 100, 2)
            : 100;
        
        [NotMapped]
        public string ComplexityLevel
        {
            get
            {
                if (EstimatedHours <= 2) return "Simple";
                if (EstimatedHours <= 8) return "Medium";
                if (EstimatedHours <= 24) return "Complex";
                return "Very Complex";
            }
        }
        
        #endregion
        
        #region Navigation Properties
        
        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
        
        #endregion
    }
}
