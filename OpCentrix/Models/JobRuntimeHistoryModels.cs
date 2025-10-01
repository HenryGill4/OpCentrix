using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Status transition history for a Job (Phase 2)
    /// </summary>
    public class JobStatusHistory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int JobId { get; set; }
        [StringLength(50)]
        public string OldStatus { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string NewStatus { get; set; } = string.Empty;
        public int? ChangedByUserId { get; set; }
        [Required]
        public DateTime ChangedUtc { get; set; } = DateTime.UtcNow;
        [StringLength(1000)]
        public string? Notes { get; set; }
        public virtual Job? Job { get; set; }
        public virtual User? ChangedByUser { get; set; }
    }

    /// <summary>
    /// Captures observed duration data for stack-level learning (Phase 2 schema only)
    /// </summary>
    public class StackDurationLearning
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int JobId { get; set; }
        public int? MasterPartId { get; set; }
        public byte? StackLevel { get; set; }
        public double ObservedHours { get; set; }
        public int? PrototypeUnits { get; set; }
        public bool IncludedInAverage { get; set; } = false;
        [StringLength(100)]
        public string? Reason { get; set; } // e.g. HasPrototypes, OutlierLow, OutlierHigh, Included
        [Required]
        public DateTime CompletedUtc { get; set; } = DateTime.UtcNow;
        public virtual Job? Job { get; set; }
        public virtual MasterPart? MasterPart { get; set; }
    }
}
