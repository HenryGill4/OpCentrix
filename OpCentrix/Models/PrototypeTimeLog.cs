using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Detailed time logging for each production stage execution
    /// Tracks setup, production, quality check, and rework activities
    /// </summary>
    public class PrototypeTimeLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductionStageExecutionId { get; set; }

        // Time Entry
        [Required]
        public DateTime LogDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? ElapsedMinutes { get; set; }

        // Activity Details
        [Required]
        [StringLength(50)]
        public string ActivityType { get; set; } = string.Empty; // Setup, Production, QualityCheck, Rework

        [Required]
        [StringLength(500)]
        public string ActivityDescription { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Employee { get; set; } = string.Empty;

        // Issues & Notes
        [StringLength(1000)]
        public string? IssuesEncountered { get; set; }

        [StringLength(1000)]
        public string? ResolutionNotes { get; set; }

        [StringLength(1000)]
        public string? ImprovementSuggestions { get; set; }

        // Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(ProductionStageExecutionId))]
        public virtual ProductionStageExecution? ProductionStageExecution { get; set; }
    }
}