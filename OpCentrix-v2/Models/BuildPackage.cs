using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

public class BuildPackage
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string PackageNumber { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Name { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Draft";

    [StringLength(50)]
    public string? TargetMachineId { get; set; }

    [Required]
    [StringLength(100)]
    public string Material { get; set; } = "Ti-6Al-4V Grade 5";

    // Build File
    [StringLength(255)]
    public string? BuildFileName { get; set; }

    [StringLength(500)]
    public string? BuildFilePath { get; set; }

    [StringLength(64)]
    public string? BuildFileHash { get; set; }

    public long? BuildFileSizeBytes { get; set; }

    public DateTime? BuildFileUploadedAt { get; set; }

    // Build Parameters
    public double EstimatedBuildHours { get; set; }
    public double EstimatedPowderKg { get; set; }
    public double? BuildHeightMm { get; set; }
    public int? LayerCount { get; set; }
    public double LayerThicknessMicrons { get; set; } = 30;

    [StringLength(20)]
    public string SupportComplexity { get; set; } = "Medium";

    // Scheduling
    public DateTime? PreferredStartDate { get; set; }
    public DateTime? DueDate { get; set; }

    [Range(1, 5)]
    public int Priority { get; set; } = 3;

    public bool IsRushJob { get; set; }
    public int? ScheduledJobId { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? LastModifiedAt { get; set; }

    [StringLength(100)]
    public string? LastModifiedBy { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    // Navigation
    public virtual ICollection<BuildPackagePart> Parts { get; set; } = new List<BuildPackagePart>();
    public virtual Job? ScheduledJob { get; set; }

    // Computed (business logic)
    [NotMapped]
    public int TotalPartCount => Parts?.Sum(p => p.Quantity) ?? 0;

    [NotMapped]
    public int UniquePartCount => Parts?.Count ?? 0;

    [NotMapped]
    public bool IsReadyToSchedule =>
        Status == "Ready" &&
        !string.IsNullOrEmpty(BuildFileName) &&
        Parts.Any();
}

public class BuildPackagePart
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int BuildPackageId { get; set; }

    [Required]
    public int PartId { get; set; }

    [Required]
    [Range(1, 500)]
    public int Quantity { get; set; } = 1;

    [StringLength(50)]
    public string Orientation { get; set; } = "Flat";

    [StringLength(50)]
    public string? SupportType { get; set; }

    public double EstimatedHours { get; set; }

    public int? PositionIndex { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public virtual BuildPackage BuildPackage { get; set; } = null!;
    public virtual Part Part { get; set; } = null!;
}
