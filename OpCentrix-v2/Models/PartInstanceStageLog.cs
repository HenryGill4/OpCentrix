using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

public class PartInstanceStageLog
{
    public int Id { get; set; }

    public int PartInstanceId { get; set; }

    public int ProductionStageId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [StringLength(100)]
    public string OperatorName { get; set; } = string.Empty;

    [Column(TypeName = "TEXT")]
    public string? CustomFieldValues { get; set; }

    [Column(TypeName = "TEXT")]
    public string? Notes { get; set; }

    // Navigation
    public virtual PartInstance PartInstance { get; set; } = null!;
    public virtual ProductionStage ProductionStage { get; set; } = null!;
}
