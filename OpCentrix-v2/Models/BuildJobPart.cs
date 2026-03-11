using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models;

public class BuildJobPart
{
    public int Id { get; set; }

    [Required]
    public int BuildJobId { get; set; }

    [Required]
    public int PartId { get; set; }

    [StringLength(50)]
    public string PartNumber { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public virtual BuildJob BuildJob { get; set; } = null!;
    public virtual Part Part { get; set; } = null!;
}
