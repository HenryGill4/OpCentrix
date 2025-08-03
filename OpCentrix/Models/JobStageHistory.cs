using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models
{
    /// <summary>
    /// Tracks stage transitions for complete job workflow traceability
    /// Option A: Enhanced audit trail for multi-stage manufacturing
    /// </summary>
    public class JobStageHistory
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Job being tracked
        /// </summary>
        [Required]
        public int JobId { get; set; }
        
        /// <summary>
        /// Optional link to production stage definition
        /// </summary>
        public int? ProductionStageId { get; set; }
        
        /// <summary>
        /// Action taken: "StageStarted", "StageCompleted", "StageSkipped", "StageHeld"
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;
        
        /// <summary>
        /// Stage name: "SLS", "CNC", "EDM", "Assembly", "QC"
        /// </summary>
        [Required]
        [StringLength(50)]
        public string StageName { get; set; } = string.Empty;
        
        /// <summary>
        /// Operator performing the action
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Operator { get; set; } = string.Empty;
        
        /// <summary>
        /// When the action occurred
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Optional notes about the stage transition
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        /// <summary>
        /// Machine used for this stage (if applicable)
        /// </summary>
        [StringLength(50)]
        public string? MachineId { get; set; }
        
        /// <summary>
        /// Time spent in this stage (hours)
        /// </summary>
        public double? StageHours { get; set; }
        
        /// <summary>
        /// Stage quality result: "Pass", "Fail", "Rework"
        /// </summary>
        [StringLength(20)]
        public string? QualityResult { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Link to the job
        /// </summary>
        public virtual Job Job { get; set; } = null!;
        
        /// <summary>
        /// Link to production stage definition (if applicable)
        /// </summary>
        public virtual ProductionStage? ProductionStage { get; set; }

        #endregion
    }
}