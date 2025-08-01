using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Links Parts with required ProductionStages, replacing boolean stage flags in Parts table
    /// Provides a normalized approach to stage management with configurable parameters
    /// </summary>
    public class PartStageRequirement
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Foreign key to the Part that requires this stage
        /// </summary>
        [Required]
        public int PartId { get; set; }
        
        /// <summary>
        /// Foreign key to the ProductionStage that is required
        /// </summary>
        [Required]
        public int ProductionStageId { get; set; }
        
        /// <summary>
        /// Execution order for this stage (1 = first, 2 = second, etc.)
        /// </summary>
        [Range(1, 20)]
        [Display(Name = "Execution Order")]
        public int ExecutionOrder { get; set; } = 1;
        
        /// <summary>
        /// Whether this stage is mandatory or optional for this part
        /// </summary>
        [Display(Name = "Is Required")]
        public bool IsRequired { get; set; } = true;
        
        /// <summary>
        /// Estimated duration in hours for this stage on this specific part
        /// Overrides the ProductionStage default if specified
        /// </summary>
        [Range(0.1, 200.0)]
        [Display(Name = "Estimated Hours")]
        public double? EstimatedHours { get; set; }
        
        /// <summary>
        /// Stage-specific setup time in minutes for this part
        /// </summary>
        [Range(0, 600)]
        [Display(Name = "Setup Time (minutes)")]
        public int SetupTimeMinutes { get; set; } = 0;
        
        /// <summary>
        /// Stage-specific parameters as JSON for this part
        /// Examples: {"laserPower": 200, "scanSpeed": 1200, "temperature": 180}
        /// </summary>
        [StringLength(2000)]
        [Display(Name = "Stage Parameters")]
        public string StageParameters { get; set; } = "{}";
        
        /// <summary>
        /// Special notes or instructions for this stage on this part
        /// </summary>
        [StringLength(1000)]
        [Display(Name = "Special Instructions")]
        public string SpecialInstructions { get; set; } = "";
        
        /// <summary>
        /// Quality requirements specific to this part at this stage
        /// </summary>
        [StringLength(1000)]
        [Display(Name = "Quality Requirements")]
        public string QualityRequirements { get; set; } = "";
        
        /// <summary>
        /// Required materials for this stage on this part
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Required Materials")]
        public string RequiredMaterials { get; set; } = "";
        
        /// <summary>
        /// Required tooling for this stage on this part
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Required Tooling")]
        public string RequiredTooling { get; set; } = "";
        
        /// <summary>
        /// Estimated cost for this stage on this part
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Estimated Cost")]
        public decimal EstimatedCost { get; set; } = 0.00m;
        
        /// <summary>
        /// Whether this stage can run in parallel with others
        /// </summary>
        [Display(Name = "Allow Parallel Execution")]
        public bool AllowParallel { get; set; } = false;
        
        /// <summary>
        /// Whether this stage blocks subsequent stages until complete
        /// </summary>
        [Display(Name = "Is Blocking")]
        public bool IsBlocking { get; set; } = true;
        
        /// <summary>
        /// Active flag - allows disabling stages without deletion
        /// </summary>
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Notes about why this stage is required for this part
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Requirement Notes")]
        public string RequirementNotes { get; set; } = "";
        
        #region Audit Fields
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";
        
        [Required]
        [StringLength(100)]
        public string LastModifiedBy { get; set; } = "System";
        
        #endregion
        
        #region Navigation Properties
        
        /// <summary>
        /// The Part that requires this stage
        /// </summary>
        public virtual Part Part { get; set; } = null!;
        
        /// <summary>
        /// The ProductionStage that is required
        /// </summary>
        public virtual ProductionStage ProductionStage { get; set; } = null!;
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Get the effective estimated hours (override or stage default)
        /// </summary>
        /// <returns>Part-specific hours if set, otherwise stage default</returns>
        public double GetEffectiveEstimatedHours()
        {
            return EstimatedHours ?? (double)(ProductionStage?.DefaultSetupMinutes ?? 60) / 60.0;
        }
        
        /// <summary>
        /// Get the effective hourly rate (stage default)
        /// </summary>
        /// <returns>Stage default hourly rate</returns>
        public decimal GetEffectiveHourlyRate()
        {
            return ProductionStage?.DefaultHourlyRate ?? 85.00m;
        }
        
        /// <summary>
        /// Calculate total estimated cost for this stage
        /// </summary>
        /// <returns>Setup cost + (hours * rate) + material/tooling costs</returns>
        public decimal CalculateTotalEstimatedCost()
        {
            if (EstimatedCost > 0) return EstimatedCost;
            
            var hours = (decimal)GetEffectiveEstimatedHours();
            var rate = GetEffectiveHourlyRate();
            var setupCost = (decimal)(SetupTimeMinutes / 60.0) * rate;
            
            return setupCost + (hours * rate);
        }
        
        /// <summary>
        /// Validate stage parameters JSON
        /// </summary>
        /// <returns>True if valid JSON, false otherwise</returns>
        public bool ValidateStageParameters()
        {
            if (string.IsNullOrWhiteSpace(StageParameters)) return true;
            
            try
            {
                System.Text.Json.JsonSerializer.Deserialize<object>(StageParameters);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Get stage dependencies (stages that must complete before this one)
        /// </summary>
        /// <param name="allPartStages">All stages for this part</param>
        /// <returns>List of stages that must complete first</returns>
        public List<PartStageRequirement> GetDependencies(List<PartStageRequirement> allPartStages)
        {
            return allPartStages
                .Where(s => s.PartId == PartId && s.ExecutionOrder < ExecutionOrder && s.IsBlocking)
                .OrderBy(s => s.ExecutionOrder)
                .ToList();
        }
        
        /// <summary>
        /// Check if this stage can start based on dependencies
        /// </summary>
        /// <param name="completedStageIds">IDs of completed stages</param>
        /// <param name="allPartStages">All stages for this part</param>
        /// <returns>True if dependencies are met</returns>
        public bool CanStart(List<int> completedStageIds, List<PartStageRequirement> allPartStages)
        {
            var dependencies = GetDependencies(allPartStages);
            return dependencies.All(d => completedStageIds.Contains(d.Id));
        }
        
        #endregion
    }
}