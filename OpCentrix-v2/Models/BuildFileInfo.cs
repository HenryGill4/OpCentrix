using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

public class BuildFileInfo
{
    public int Id { get; set; }

    public int BuildPackageId { get; set; }

    [StringLength(255)]
    public string? FileName { get; set; }

    public int? LayerCount { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? BuildHeightMm { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? EstimatedPrintTimeHours { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? EstimatedPowderKg { get; set; }

    [Column(TypeName = "TEXT")]
    public string? PartPositionsJson { get; set; }

    [StringLength(100)]
    public string? SlicerSoftware { get; set; }

    [StringLength(50)]
    public string? SlicerVersion { get; set; }

    public DateTime ImportedDate { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string ImportedBy { get; set; } = string.Empty;

    // Navigation
    public virtual BuildPackage BuildPackage { get; set; } = null!;
}
