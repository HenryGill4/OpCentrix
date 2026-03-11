using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.Maintenance;

public class MaintenanceActionLog
{
    public int Id { get; set; }

    public int RuleId { get; set; }

    [Required]
    [StringLength(50)]
    public string MachineId { get; set; } = string.Empty;

    public int? MachineComponentId { get; set; }

    public int PerformedByUserId { get; set; }

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool ResetPerformed { get; set; }

    // Navigation
    public virtual MaintenanceRule Rule { get; set; } = null!;
}
