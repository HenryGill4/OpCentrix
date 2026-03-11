using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpCentrix.Models.Enums;

namespace OpCentrix.Models;

public class WorkOrder
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? CustomerPO { get; set; }

    [StringLength(200)]
    public string? CustomerEmail { get; set; }

    [StringLength(50)]
    public string? CustomerPhone { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime DueDate { get; set; }

    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;

    public JobPriority Priority { get; set; } = JobPriority.Normal;

    public int? QuoteId { get; set; }

    [Column(TypeName = "TEXT")]
    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [StringLength(100)]
    public string LastModifiedBy { get; set; } = string.Empty;

    // Navigation
    public virtual ICollection<WorkOrderLine> Lines { get; set; } = new List<WorkOrderLine>();
    public virtual Quote? Quote { get; set; }
}

public class WorkOrderLine
{
    public int Id { get; set; }

    [Required]
    public int WorkOrderId { get; set; }

    [Required]
    public int PartId { get; set; }

    [Required]
    [Range(1, 10000)]
    public int Quantity { get; set; }

    public int ProducedQuantity { get; set; }

    public int ShippedQuantity { get; set; }

    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public virtual WorkOrder WorkOrder { get; set; } = null!;
    public virtual Part Part { get; set; } = null!;
    // Job and PartInstance nav added later
}
