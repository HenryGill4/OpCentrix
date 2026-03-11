using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

public class UserSettings
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [StringLength(20)]
    public string Theme { get; set; } = "dark";

    [Column(TypeName = "TEXT")]
    public string? DashboardLayout { get; set; }

    [StringLength(50)]
    public string? DefaultView { get; set; }

    public bool NotificationsEnabled { get; set; } = true;

    public virtual User User { get; set; } = null!;
}
