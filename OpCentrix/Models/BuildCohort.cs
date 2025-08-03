using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Represents a cohort of parts produced in a single SLS build
    /// Option A: Lightweight model for tracking 20-130 parts per build
    /// </summary>
    public class BuildCohort
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Link to existing BuildJob for SLS completion data
        /// </summary>
        public int? BuildJobId { get; set; }
        
        /// <summary>
        /// Human-readable build identifier: "BUILD-2025-001"
        /// </summary>
        [Required]
        [StringLength(50)]
        public string BuildNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Number of parts in this cohort (20-130 typical)
        /// </summary>
        [Range(1, 500)]
        public int PartCount { get; set; }
        
        /// <summary>
        /// Material used for entire cohort: Ti-6Al-4V, Inconel 718, etc.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Material { get; set; } = "Ti-6Al-4V Grade 5";
        
        /// <summary>
        /// Cohort status: "InProgress", "Complete", "QC", "Shipped"
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "InProgress";
        
        /// <summary>
        /// SLS build completion date
        /// </summary>
        public DateTime? CompletedDate { get; set; }
        
        /// <summary>
        /// Created by user/system
        /// </summary>
        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";
        
        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Last modification timestamp
        /// </summary>
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Notes about this build cohort
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Link to existing BuildJob (SLS print tracking)
        /// </summary>
        public virtual BuildJob? BuildJob { get; set; }
        
        /// <summary>
        /// Jobs associated with this cohort
        /// </summary>
        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

        #endregion

        #region Computed Properties

        /// <summary>
        /// Jobs in this cohort that are completed
        /// </summary>
        [NotMapped]
        public int CompletedJobsCount => Jobs?.Count(j => j.Status == "Completed") ?? 0;
        
        /// <summary>
        /// Progress percentage for this cohort
        /// </summary>
        [NotMapped]
        public double ProgressPercent => PartCount > 0 
            ? Math.Round((double)CompletedJobsCount / PartCount * 100, 1) 
            : 0;
        
        /// <summary>
        /// Current stage of most jobs in cohort
        /// </summary>
        [NotMapped]
        public string CurrentStage => Jobs?.GroupBy(j => j.WorkflowStage)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? "Unknown";

        #endregion
    }
}