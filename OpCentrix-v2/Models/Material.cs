using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

public class Material
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = "Metal Powder";

    public double? Density { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal CostPerKg { get; set; }

    [StringLength(200)]
    public string? Supplier { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string? CompatibleMaterials { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [StringLength(100)]
    public string LastModifiedBy { get; set; } = string.Empty;
}
