using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.Maintenance;

public class MachineComponent
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string MachineId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; } = 100;

    [StringLength(50)]
    public string Category { get; set; } = "General";

    [StringLength(20)]
    public string? Icon { get; set; } = "cog";

    [StringLength(7)]
    public string? ColorCode { get; set; } = "#6B7280";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    public DateTime? UpdatedAt { get; set; }

    [StringLength(100)]
    public string? UpdatedBy { get; set; }
}
