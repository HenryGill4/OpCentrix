using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

public class MachineConnectionSettings
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string MachineId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ProviderType { get; set; } = "Mock";

    [StringLength(200)]
    public string? EndpointUrl { get; set; }

    public bool IsEnabled { get; set; }

    public int PollIntervalSeconds { get; set; } = 5;

    [Column(TypeName = "TEXT")]
    public string? ConfigJson { get; set; }

    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
}
