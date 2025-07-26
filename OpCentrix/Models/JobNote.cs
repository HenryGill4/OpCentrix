using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

/// <summary>
/// Job step notes for tracking detailed job progress and instructions
/// Task 9: Enhanced scheduler with job step notes support
/// </summary>
public class JobNote
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Reference to the parent job
    /// </summary>
    [Required]
    public int JobId { get; set; }

    /// <summary>
    /// Navigation property to parent job
    /// </summary>
    public virtual Job Job { get; set; } = null!;

    /// <summary>
    /// Step or stage this note applies to (e.g., "Setup", "Printing", "Cooling", "Post-Processing")
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Step { get; set; } = string.Empty;

    /// <summary>
    /// Note content
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Optional timestamp for when this step occurs
    /// </summary>
    public DateTime? StepTime { get; set; }

    /// <summary>
    /// Priority level (1 = highest, 5 = lowest)
    /// </summary>
    [Range(1, 5)]
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Note type (Info, Warning, Critical, Instruction)
    /// </summary>
    [StringLength(20)]
    public string NoteType { get; set; } = "Info";

    /// <summary>
    /// Whether this note is completed/acknowledged
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// User who created the note
    /// </summary>
    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who last modified the note
    /// </summary>
    [StringLength(100)]
    public string LastModifiedBy { get; set; } = "System";

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Get color for note type
    /// </summary>
    [NotMapped]
    public string NoteTypeColor => NoteType.ToLower() switch
    {
        "info" => "#3B82F6",      // blue
        "warning" => "#F59E0B",   // amber
        "critical" => "#EF4444",  // red
        "instruction" => "#10B981", // green
        _ => "#6B7280"            // gray
    };

    /// <summary>
    /// Get priority color
    /// </summary>
    [NotMapped]
    public string PriorityColor => Priority switch
    {
        1 => "#DC2626", // red (critical)
        2 => "#F59E0B", // orange (high)
        3 => "#10B981", // green (normal)
        4 => "#6B7280", // gray (low)
        5 => "#9CA3AF", // light gray (lowest)
        _ => "#6B7280"
    };
}