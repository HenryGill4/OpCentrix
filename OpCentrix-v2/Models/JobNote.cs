using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

public class JobNote
{
    public int Id { get; set; }

    [Required]
    public int JobId { get; set; }

    [Required]
    [Column(TypeName = "TEXT")]
    public string NoteText { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation
    public virtual Job Job { get; set; } = null!;
}
