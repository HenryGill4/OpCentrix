using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

public class DelayLog
{
    public int Id { get; set; }

    public int? BuildJobId { get; set; }

    public int? JobId { get; set; }

    public int? StageExecutionId { get; set; }

    [Required]
    [StringLength(200)]
    public string Reason { get; set; } = string.Empty;

    [StringLength(50)]
    public string? ReasonCode { get; set; }

    [Required]
    public int DelayMinutes { get; set; }

    [Required]
    [StringLength(100)]
    public string LoggedBy { get; set; } = string.Empty;

    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public virtual BuildJob? BuildJob { get; set; }
    public virtual Job? Job { get; set; }
    public virtual StageExecution? StageExecution { get; set; }
}
