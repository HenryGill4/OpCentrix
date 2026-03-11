using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

public class SystemSetting
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "TEXT")]
    public string Value { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = "General";

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string LastModifiedBy { get; set; } = string.Empty;
}
