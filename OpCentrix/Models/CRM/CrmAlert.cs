using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.CRM;

public class CrmAlert
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Message { get; set; }

    [StringLength(30)]
    public string Severity { get; set; } = "Info"; // Info, Warning, Critical

    public bool IsDismissed { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
}
