using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpCentrix.Models.Enums;

namespace OpCentrix.Models;

public class Job
{
    public int Id { get; set; }

    public int PartId { get; set; }

    [StringLength(50)]
    public string MachineId { get; set; } = string.Empty;

    public int? WorkOrderLineId { get; set; }

    // Scheduling
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd { get; set; }

    // Production
    [StringLength(50)]
    public string PartNumber { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public int ProducedQuantity { get; set; }
    public int DefectQuantity { get; set; }
    public double EstimatedHours { get; set; }

    [StringLength(100)]
    public string? SlsMaterial { get; set; }

    // Stacking
    public byte? StackLevel { get; set; }
    public int? PartsPerBuild { get; set; }
    public double? PlannedStackDurationHours { get; set; }

    // Workflow
    public JobStatus Status { get; set; } = JobStatus.Draft;
    public JobPriority Priority { get; set; } = JobPriority.Normal;

    [Column(TypeName = "TEXT")]
    public string? Notes { get; set; }

    // Predecessor chain
    public int? PredecessorJobId { get; set; }
    public double? UpstreamGapHours { get; set; }

    // Operator
    public int? OperatorUserId { get; set; }
    public DateTime? LastStatusChangeUtc { get; set; }

    // Audit
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [StringLength(100)]
    public string LastModifiedBy { get; set; } = string.Empty;

    // Navigation
    public virtual Part Part { get; set; } = null!;
    public virtual Job? PredecessorJob { get; set; }
    public virtual User? OperatorUser { get; set; }
    public virtual WorkOrderLine? WorkOrderLine { get; set; }
    public virtual ICollection<StageExecution> Stages { get; set; } = new List<StageExecution>();
    public virtual ICollection<JobNote> JobNotes { get; set; } = new List<JobNote>();

    // NotMapped
    [NotMapped]
    public TimeSpan ScheduledDuration => ScheduledEnd - ScheduledStart;

    [NotMapped]
    public double DurationHours => ScheduledDuration.TotalHours;

    [NotMapped]
    public bool IsOverdue => Status != JobStatus.Completed && Status != JobStatus.Cancelled && ScheduledEnd < DateTime.UtcNow;
}
