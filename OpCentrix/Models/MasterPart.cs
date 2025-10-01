using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// PHASE 4 Master Part Refactor: Master Part entity for part-based stacking and stage management
    /// Replaces legacy Part-based boolean flags with structured stacking configuration
    /// </summary>
    public class MasterPart
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Part Number")]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Material")]
        public string Material { get; set; } = "Ti-6Al-4V Grade 5";

        [Required]
        [StringLength(100)]
        [Display(Name = "Manufacturing Approach")]
        public string ManufacturingApproach { get; set; } = "SLS-Based";

        #region SLS Stacking Configuration
        
        [Display(Name = "Allow Stacking")]
        public bool AllowStacking { get; set; } = false;

        [Range(0.1, 500.0)]
        [Display(Name = "Single (1x) Duration (hours)")]
        public double? SingleStackDurationHours { get; set; }

        [Range(0.1, 500.0)]
        [Display(Name = "Double (2x) Duration (hours)")]
        public double? DoubleStackDurationHours { get; set; }

        [Range(0.1, 500.0)]
        [Display(Name = "Triple (3x) Duration (hours)")]
        public double? TripleStackDurationHours { get; set; }

        [Range(1, 10)]
        [Display(Name = "Max Stack Count")]
        public int MaxStackCount { get; set; } = 1;

        #endregion

        #region SLS Build Configuration - NEW FIELDS

        /// <summary>
        /// Parts per build for single stack (always required, default 1)
        /// </summary>
        [Required]
        [Range(1, 100)]
        [Display(Name = "Parts Per Build (Single)")]
        public int PartsPerBuildSingle { get; set; } = 1;

        /// <summary>
        /// Parts per build for double stack (nullable, only set if double stack enabled)
        /// </summary>
        [Range(1, 100)]
        [Display(Name = "Parts Per Build (Double)")]
        public int? PartsPerBuildDouble { get; set; }

        /// <summary>
        /// Parts per build for triple stack (nullable, only set if triple stack enabled)
        /// </summary>
        [Range(1, 100)]
        [Display(Name = "Parts Per Build (Triple)")]
        public int? PartsPerBuildTriple { get; set; }

        /// <summary>
        /// Enable double stack configuration
        /// </summary>
        [Display(Name = "Enable Double Stack")]
        public bool EnableDoubleStack { get; set; } = false;

        /// <summary>
        /// Enable triple stack configuration
        /// </summary>
        [Display(Name = "Enable Triple Stack")]
        public bool EnableTripleStack { get; set; } = false;

        /// <summary>
        /// Stage-derived single estimate (advisory, for comparison with observed durations)
        /// </summary>
        [Range(0.1, 500.0)]
        [Display(Name = "Stage Estimate (Single)")]
        public double? StageEstimateSingle { get; set; }

        #endregion

        #region Required Stages JSON

        /// <summary>
        /// JSON array of required production stages for this master part
        /// </summary>
        [Required]
        [StringLength(1000)]
        [Display(Name = "Required Stages")]
        public string RequiredStages { get; set; } = "[]";

        #endregion

        #region Status and Lifecycle

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

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

        #endregion

        #region Navigation Properties

        public virtual ICollection<StageDefinition> StageDefinitions { get; set; } = new List<StageDefinition>();
        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
        public virtual ICollection<ProductionBuild> ProductionBuilds { get; set; } = new List<ProductionBuild>(); // <-- Add this property

        #endregion

        #region Computed Properties

        /// <summary>
        /// Check if stacking is properly configured
        /// </summary>
        [NotMapped]
        public bool HasStackingConfiguration => AllowStacking && SingleStackDurationHours.HasValue;

        /// <summary>
        /// Get the effective single stack duration considering fallbacks
        /// </summary>
        [NotMapped]
        public double EffectiveSingleDuration => SingleStackDurationHours ?? StageEstimateSingle ?? 8.0;

        /// <summary>
        /// Check if double stack is fully configured
        /// </summary>
        [NotMapped]
        public bool HasValidDoubleStack => EnableDoubleStack && DoubleStackDurationHours.HasValue && PartsPerBuildDouble.HasValue;

        /// <summary>
        /// Check if triple stack is fully configured
        /// </summary>
        [NotMapped]
        public bool HasValidTripleStack => EnableTripleStack && TripleStackDurationHours.HasValue && PartsPerBuildTriple.HasValue;

        /// <summary>
        /// Get available stack levels
        /// </summary>
        [NotMapped]
        public List<int> AvailableStackLevels
        {
            get
            {
                var levels = new List<int> { 1 }; // Single always available
                if (HasValidDoubleStack) levels.Add(2);
                if (HasValidTripleStack) levels.Add(3);
                return levels;
            }
        }

        /// <summary>
        /// Get duration for a specific stack level
        /// </summary>
        public double? GetStackDuration(int level)
        {
            return level switch
            {
                1 => SingleStackDurationHours ?? StageEstimateSingle,
                2 => HasValidDoubleStack ? DoubleStackDurationHours : null,
                3 => HasValidTripleStack ? TripleStackDurationHours : null,
                _ => null
            };
        }

        /// <summary>
        /// Get parts per build for a specific stack level
        /// </summary>
        public int? GetPartsPerBuild(int level)
        {
            return level switch
            {
                1 => PartsPerBuildSingle,
                2 => HasValidDoubleStack ? PartsPerBuildDouble : null,
                3 => HasValidTripleStack ? PartsPerBuildTriple : null,
                _ => null
            };
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validate stacking configuration
        /// </summary>
        public List<string> ValidateStackingConfiguration()
        {
            var issues = new List<string>();

            if (AllowStacking)
            {
                if (!SingleStackDurationHours.HasValue)
                    issues.Add("Single stack duration must be specified when stacking is allowed");

                if (EnableDoubleStack && !DoubleStackDurationHours.HasValue)
                    issues.Add("Double stack duration must be specified when double stack is enabled");

                if (EnableDoubleStack && !PartsPerBuildDouble.HasValue)
                    issues.Add("Parts per build (double) must be specified when double stack is enabled");

                if (EnableTripleStack && !TripleStackDurationHours.HasValue)
                    issues.Add("Triple stack duration must be specified when triple stack is enabled");

                if (EnableTripleStack && !PartsPerBuildTriple.HasValue)
                    issues.Add("Parts per build (triple) must be specified when triple stack is enabled");

                if (EnableTripleStack && !EnableDoubleStack)
                    issues.Add("Double stack must be enabled before triple stack can be enabled");
            }

            return issues;
        }

        /// <summary>
        /// Get recommended stack level based on quantity and part configuration
        /// </summary>
        public int GetRecommendedStackLevel(int quantity)
        {
            if (!AllowStacking || !HasStackingConfiguration)
                return 1;

            // Simple logic: recommend highest stack level that doesn't waste capacity
            if (HasValidTripleStack && quantity >= PartsPerBuildTriple)
                return 3;
            if (HasValidDoubleStack && quantity >= PartsPerBuildDouble)
                return 2;
            return 1;
        }

        /// <summary>
        /// Calculate efficiency for a given stack level and quantity
        /// </summary>
        public double CalculateStackEfficiency(int stackLevel, int quantity)
        {
            var partsPerBuild = GetPartsPerBuild(stackLevel);
            var duration = GetStackDuration(stackLevel);

            if (!partsPerBuild.HasValue || !duration.HasValue || partsPerBuild <= 0 || duration <= 0)
                return 0;

            var buildsNeeded = Math.Ceiling((double)quantity / partsPerBuild.Value);
            var totalHours = buildsNeeded * duration.Value;
            var partsPerHour = quantity / totalHours;

            return partsPerHour;
        }

        #endregion
    }
}