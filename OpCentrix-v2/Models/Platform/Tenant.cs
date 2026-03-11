using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.Platform;

public class Tenant
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? LogoUrl { get; set; }

    [StringLength(7)]
    public string? PrimaryColor { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [StringLength(50)]
    public string? SubscriptionTier { get; set; }
}
