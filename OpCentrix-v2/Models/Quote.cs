using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpCentrix.Models.Enums;

namespace OpCentrix.Models;

public class Quote
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string QuoteNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? CustomerEmail { get; set; }

    [StringLength(50)]
    public string? CustomerPhone { get; set; }

    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpirationDate { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalEstimatedCost { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? QuotedPrice { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? Markup { get; set; }

    [Column(TypeName = "TEXT")]
    public string? Notes { get; set; }

    public int? ConvertedWorkOrderId { get; set; }

    // Navigation
    public virtual ICollection<QuoteLine> Lines { get; set; } = new List<QuoteLine>();
    public virtual WorkOrder? ConvertedWorkOrder { get; set; }
}

public class QuoteLine
{
    public int Id { get; set; }

    [Required]
    public int QuoteId { get; set; }

    [Required]
    public int PartId { get; set; }

    [Required]
    [Range(1, 10000)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal EstimatedCostPerPart { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal QuotedPricePerPart { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public virtual Quote Quote { get; set; } = null!;
    public virtual Part Part { get; set; } = null!;
}
