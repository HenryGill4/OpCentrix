using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OpCentrix.Models
{
    // Represents a part specification for SLS metal printing with comprehensive manufacturing data
    public class Part
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        [RegularExpression(@"^\d{2}-\d{4}$", ErrorMessage = "Part number must be in format XX-XXXX (e.g., 14-5396)")]
        public string PartNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Material { get; set; } = "Ti-6Al-4V Grade 5";
        
        #region SLS-Specific Manufacturing Properties
        
        // SLS Material Specifications
        [Required]
        [StringLength(100)]
        public string SlsMaterial { get; set; } = "Ti-6Al-4V Grade 5";
        
        [StringLength(100)]
        public string PowderSpecification { get; set; } = "15-45 μm particle size";
        
        [Range(0, 50)]
        public double PowderRequirementKg { get; set; } = 0.5;
        
        // Build Parameters
        [Range(0, 2000)]
        public double RecommendedLaserPower { get; set; } = 200;
        
        [Range(0, 5000)]
        public double RecommendedScanSpeed { get; set; } = 1200;
        
        [Range(10, 100)]
        public double RecommendedLayerThickness { get; set; } = 30;
        
        [Range(50, 200)]
        public double RecommendedHatchSpacing { get; set; } = 120;
        
        // Build Environment Requirements
        [Range(0, 500)]
        public double RecommendedBuildTemperature { get; set; } = 180;
        
        [Range(99.0, 100.0)]
        public double RequiredArgonPurity { get; set; } = 99.9;
        
        [Range(0, 100)]
        public double MaxOxygenContent { get; set; } = 50;
        
        #endregion
        
        #region Physical Properties
        
        [Range(0, 1000)]
        public double WeightGrams { get; set; } = 0;
        
        [StringLength(100)]
        public string Dimensions { get; set; } = string.Empty; // "L x W x H in mm"
        
        [Range(0, 1000000)]
        public double VolumeMm3 { get; set; } = 0;
        
        [Range(0, 300)]
        public double HeightMm { get; set; } = 0; // Critical for SLS build planning
        
        [Range(0, 250)]
        public double LengthMm { get; set; } = 0;
        
        [Range(0, 250)]
        public double WidthMm { get; set; } = 0;
        
        // Surface finish requirements
        [StringLength(100)]
        public string SurfaceFinishRequirement { get; set; } = "As-built";
        
        [Range(0, 100)]
        public double MaxSurfaceRoughnessRa { get; set; } = 25; // microns
        
        #endregion
        
        #region Cost Data Enhanced for SLS
        
        [Column(TypeName = "decimal(12,2)")]
        public decimal MaterialCostPerKg { get; set; } = 450.00m; // Titanium powder cost
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal StandardLaborCostPerHour { get; set; } = 85.00m; // SLS operator rate
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal SetupCost { get; set; } = 150.00m; // Platform preparation, etc.
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal PostProcessingCost { get; set; } = 75.00m; // Support removal, finishing
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal QualityInspectionCost { get; set; } = 50.00m;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal MachineOperatingCostPerHour { get; set; } = 125.00m; // TruPrint 3000 operating cost
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal ArgonCostPerHour { get; set; } = 15.00m;
        
        #endregion
        
        #region Manufacturing Data Enhanced for SLS
        
        // Process information
        [StringLength(50)]
        public string ProcessType { get; set; } = "SLS Metal"; // SLS, SLA, FDM, etc.
        
        [StringLength(100)]
        public string RequiredMachineType { get; set; } = "TruPrint 3000";
        
        [StringLength(200)]
        public string PreferredMachines { get; set; } = "TI1,TI2"; // Comma-separated list
        
        // SLS-specific setup and timing
        [Range(0, 300)]
        public double SetupTimeMinutes { get; set; } = 45;
        
        [Range(0, 180)]
        public double PowderChangeoverTimeMinutes { get; set; } = 30;
        
        [Range(0, 300)]
        public double PreheatingTimeMinutes { get; set; } = 60;
        
        [Range(0, 600)]
        public double CoolingTimeMinutes { get; set; } = 240; // 4 hours typical
        
        [Range(0, 180)]
        public double PostProcessingTimeMinutes { get; set; } = 45;
        
        // Quality requirements for SLS
        [StringLength(500)]
        public string QualityStandards { get; set; } = "ASTM F3001, ISO 17296";
        
        [StringLength(500)]
        public string ToleranceRequirements { get; set; } = "±0.1mm typical, ±0.05mm critical dimensions";
        
        public bool RequiresInspection { get; set; } = true;
        
        public bool RequiresCertification { get; set; } = false; // AS9100, ISO 13485, etc.
        
        #endregion
        
        #region SLS Resource Requirements
        
        [StringLength(500)]
        public string RequiredSkills { get; set; } = "SLS Operation,Powder Handling,Inert Gas Safety,Post-Processing";
        
        [StringLength(500)]
        public string RequiredCertifications { get; set; } = "SLS Operation Certification,Powder Safety Training";
        
        [StringLength(500)]
        public string RequiredTooling { get; set; } = "Build Platform,Powder Sieve,Support Removal Tools";
        
        [StringLength(500)]
        public string ConsumableMaterials { get; set; } = "Argon Gas,Build Platform Coating";
        
        // Support structure requirements
        public bool RequiresSupports { get; set; } = false;
        
        [StringLength(200)]
        public string SupportStrategy { get; set; } = "Minimal supports on overhangs > 45°";
        
        [Range(0, 60)]
        public double SupportRemovalTimeMinutes { get; set; } = 0;
        
        #endregion
        
        #region Customer and Classification Enhanced
        
        [StringLength(100)]
        public string CustomerPartNumber { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string PartCategory { get; set; } = "Prototype"; // Prototype, Production, Tooling, Repair
        
        [StringLength(10)]
        public string PartClass { get; set; } = "B"; // A=Critical, B=Important, C=Standard
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(100)]
        public string Industry { get; set; } = "General"; // Set default value to prevent NOT NULL errors
        
        [StringLength(100)]
        public string Application { get; set; } = "General Component"; // Set default value to prevent NOT NULL errors
        
        // Regulatory requirements
        public bool RequiresFDA { get; set; } = false;
        public bool RequiresAS9100 { get; set; } = false;
        public bool RequiresNADCAP { get; set; } = false;
        
        #endregion
        
        #region Historical Performance Data Enhanced
        
        // Historical averages (calculated from completed jobs)
        public double AverageActualHours { get; set; } = 0;
        
        [Range(0, 200)]
        public double AverageEfficiencyPercent { get; set; } = 100;
        
        [Range(0, 100)]
        public double AverageQualityScore { get; set; } = 100;
        
        [Range(0, 50)]
        public double AverageDefectRate { get; set; } = 0;
        
        [Range(0, 100)]
        public double AveragePowderUtilization { get; set; } = 85;
        
        // Volume data
        public int TotalJobsCompleted { get; set; } = 0;
        public int TotalUnitsProduced { get; set; } = 0;
        public DateTime? LastProduced { get; set; }
        
        // Financial performance
        [Column(TypeName = "decimal(10,2)")]
        public decimal AverageCostPerUnit { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal StandardSellingPrice { get; set; } = 0;
        
        #endregion
        
        #region Process Parameters Enhanced for SLS
        
        // Process-specific data (stored as JSON for flexibility)
        [StringLength(2000)]
        public string ProcessParameters { get; set; } = "{}";
        
        [StringLength(2000)]
        public string QualityCheckpoints { get; set; } = "{}";
        
        // Build file information
        [StringLength(255)]
        public string BuildFileTemplate { get; set; } = string.Empty; // Template .slm file
        
        [StringLength(500)]
        public string CadFilePath { get; set; } = string.Empty; // Original CAD file
        
        [StringLength(100)]
        public string CadFileVersion { get; set; } = string.Empty;
        
        #endregion
        
        #region Audit Trail
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string LastModifiedBy { get; set; } = string.Empty;
        
        #endregion
        
        #region Computed Properties Enhanced for SLS (NotMapped)
        
        // Cost calculations enhanced for SLS
        [NotMapped]
        public decimal EstimatedTotalCostPerUnit => 
            ((decimal)PowderRequirementKg * MaterialCostPerKg) + 
            (StandardLaborCostPerHour * (decimal)EstimatedHours) + 
            (MachineOperatingCostPerHour * (decimal)EstimatedHours) +
            (ArgonCostPerHour * (decimal)EstimatedHours) +
            SetupCost + 
            PostProcessingCost + 
            QualityInspectionCost;
        
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
                var score = 0;
                
                // Time complexity
                if (EstimatedHours > 24) score += 3;
                else if (EstimatedHours > 8) score += 2;
                else if (EstimatedHours > 2) score += 1;
                
                // Geometric complexity
                if (RequiresSupports) score += 2;
                if (HeightMm > 150) score += 2;
                if (VolumeMm3 > 100000) score += 1;
                
                // Quality complexity
                if (MaxSurfaceRoughnessRa < 10) score += 2;
                if (RequiresCertification) score += 2;
                if (RequiresFDA) score += 3;
                
                // Material complexity
                if (SlsMaterial.Contains("Inconel")) score += 2;
                if (RequiredArgonPurity > 99.95) score += 1;
                
                return score switch
                {
                    <= 2 => "Simple",
                    <= 5 => "Medium", 
                    <= 8 => "Complex",
                    _ => "Very Complex"
                };
            }
        }
        
        // Build efficiency metrics
        [NotMapped]
        public double BuildDensityScore => VolumeMm3 > 0 && EstimatedHours > 0
            ? Math.Round(VolumeMm3 / (EstimatedHours * 1000), 2) // mm³ per hour per 1000
            : 0;
        
        [NotMapped]
        public double PowderEfficiency => PowderRequirementKg > 0 && WeightGrams > 0
            ? Math.Round((WeightGrams / 1000.0) / PowderRequirementKg * 100, 2)
            : 0;
        
        // Total process time including all SLS phases
        [NotMapped]
        public double TotalProcessTimeHours => 
            (SetupTimeMinutes + PreheatingTimeMinutes + (EstimatedHours * 60) + 
             CoolingTimeMinutes + PostProcessingTimeMinutes) / 60.0;
        
        #endregion
        
        #region Duration and Time Management Enhanced
        
        // Duration estimates
        [StringLength(50)]
        public string AvgDuration { get; set; } = "8h 0m";
        public int AvgDurationDays { get; set; } = 1;
        
        [Range(0.1, 200.0)]
        public double EstimatedHours { get; set; } = 8.0;
        
        // Task 7: Admin Duration Override System
        [Range(0.1, 200.0)]
        public double? AdminEstimatedHoursOverride { get; set; }
        
        [StringLength(500)]
        public string AdminOverrideReason { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string AdminOverrideBy { get; set; } = string.Empty;
        
        public DateTime? AdminOverrideDate { get; set; }
        
        public bool HasAdminOverride => AdminEstimatedHoursOverride.HasValue;
        
        // Computed property to get the effective duration (override takes precedence)
        [NotMapped]
        public double EffectiveDurationHours => AdminEstimatedHoursOverride ?? EstimatedHours;
        
        [NotMapped]
        public string EffectiveDurationDisplay => HasAdminOverride 
            ? $"{EffectiveDurationHours:F1}h (Override)" 
            : $"{EstimatedHours:F1}h";
        
        #endregion
        
        #region Navigation Properties
        
        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
        
        #endregion
        
        #region Helper Methods for SLS Operations
        
        // Get recommended process parameters as dictionary
        public Dictionary<string, object> GetRecommendedProcessParameters()
        {
            return new Dictionary<string, object>
            {
                ["LaserPower"] = RecommendedLaserPower,
                ["ScanSpeed"] = RecommendedScanSpeed,
                ["LayerThickness"] = RecommendedLayerThickness,
                ["HatchSpacing"] = RecommendedHatchSpacing,
                ["BuildTemperature"] = RecommendedBuildTemperature,
                ["ArgonPurity"] = RequiredArgonPurity,
                ["MaxOxygenContent"] = MaxOxygenContent
            };
        }
        
        // Check if part can fit on specific build platform
        public bool CanFitOnBuildPlatform(double platformLengthMm, double platformWidthMm, double platformHeightMm)
        {
            return LengthMm <= platformLengthMm && 
                   WidthMm <= platformWidthMm && 
                   HeightMm <= platformHeightMm;
        }
        
        // Calculate build volume utilization
        public double CalculateBuildVolumeUtilization(double platformVolumeMm3)
        {
            return platformVolumeMm3 > 0 ? Math.Round((VolumeMm3 / platformVolumeMm3) * 100, 2) : 0;
        }
        
        // Get material compatibility list
        public string[] GetCompatibleMaterials()
        {
            return SlsMaterial switch
            {
                "Ti-6Al-4V Grade 5" => new[] { "Ti-6Al-4V Grade 5", "Ti-6Al-4V ELI Grade 23" },
                "Ti-6Al-4V ELI Grade 23" => new[] { "Ti-6Al-4V Grade 5", "Ti-6Al-4V ELI Grade 23" },
                "Inconel 718" => new[] { "Inconel 718", "Inconel 625" },
                "Inconel 625" => new[] { "Inconel 718", "Inconel 625" },
                _ => new[] { SlsMaterial }
            };
        }
        
        // Validate part design for SLS manufacturing
        public List<string> ValidateForSlsManufacturing()
        {
            var issues = new List<string>();
            
            if (LengthMm > 250 || WidthMm > 250)
                issues.Add("Part dimensions exceed TruPrint 3000 build envelope (250x250mm)");
            
            if (HeightMm > 300)
                issues.Add("Part height exceeds TruPrint 3000 build height (300mm)");
            
            if (MaxSurfaceRoughnessRa < 5)
                issues.Add("Surface roughness requirement may require post-processing");
            
            if (VolumeMm3 < 100)
                issues.Add("Very small parts may be difficult to locate and remove");
            
            if (RequiresSupports && SupportRemovalTimeMinutes == 0)
                issues.Add("Support removal time not specified for part requiring supports");
            
            return issues;
        }
        
        #endregion
    }
}
