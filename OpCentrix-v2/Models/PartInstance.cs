using System.ComponentModel.DataAnnotations;
using OpCentrix.Models.Enums;

namespace OpCentrix.Models;

public class PartInstance
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string SerialNumber { get; set; } = string.Empty;

    public int WorkOrderLineId { get; set; }

    public int PartId { get; set; }

    public int? CurrentStageId { get; set; }

    public PartInstanceStatus Status { get; set; } = PartInstanceStatus.InProcess;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual Part Part { get; set; } = null!;
    public virtual WorkOrderLine WorkOrderLine { get; set; } = null!;
    public virtual ProductionStage? CurrentStage { get; set; }
    public virtual ICollection<PartInstanceStageLog> StageLogs { get; set; } = new List<PartInstanceStageLog>();
    public virtual ICollection<QCInspection> Inspections { get; set; } = new List<QCInspection>();
}
