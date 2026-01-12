using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.CRM;

public class CrmTaskProgress
{
    public int Id { get; set; }

    [Required]
    public int TaskId { get; set; }

    [Required]
    [StringLength(2000)]
    public string ProgressNote { get; set; } = string.Empty;

    [Range(0, 100)]
    public int? PercentComplete { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [Required]
    public int CreatedByUserId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? AttachmentPath { get; set; }

    [StringLength(50)]
    public string ProgressType { get; set; } = "Update"; // Update, Milestone, Issue, Resolution

    public bool IsVisibleToClient { get; set; } = true;

    // Navigation properties
    public virtual CrmTask? Task { get; set; }
    public virtual User? CreatedBy { get; set; }
}