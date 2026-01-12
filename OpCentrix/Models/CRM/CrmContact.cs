using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.CRM;

public class CrmContact
{
    public int Id { get; set; }

    [Required]
    public int AccountId { get; set; }

    public CrmAccount? Account { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(200)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(50)]
    public string? Phone { get; set; }

    [StringLength(100)]
    public string? Title { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    public List<CrmTask> Tasks { get; set; } = new();
}
