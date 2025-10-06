using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.Maintenance
{
    public class MachineComponent
    {
        public int Id { get; set; }
        [Required, MaxLength(50)] public string MachineId { get; set; } = string.Empty; // parent machine logical id
        [Required, MaxLength(100)] public string Name { get; set; } = string.Empty; // e.g. Recoater Arm, Filter, Sieve Station
        [MaxLength(500)] public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 100;
        [MaxLength(50)] public string Category { get; set; } = "General"; // e.g. Core, Consumable, Stage, Peripheral
        [MaxLength(20)] public string? Icon { get; set; } = "cog";
        [MaxLength(7)] public string? ColorCode { get; set; } = "#6B7280";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)] public string CreatedBy { get; set; } = "System";
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
    }
}
