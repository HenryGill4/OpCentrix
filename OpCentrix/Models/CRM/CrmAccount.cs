using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.CRM;

public class CrmAccount
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string Status { get; set; } = "Active";

    [StringLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    public List<CrmContact> Contacts { get; set; } = new();
    public List<CrmTask> Tasks { get; set; } = new();
}
