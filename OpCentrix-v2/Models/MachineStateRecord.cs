using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

public class MachineStateRecord
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string MachineId { get; set; } = string.Empty;

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [StringLength(50)]
    public string? Status { get; set; }

    public double? BuildProgress { get; set; }

    public int? CurrentLayer { get; set; }

    public int? TotalLayers { get; set; }

    public double? BedTemperature { get; set; }

    public double? ChamberTemperature { get; set; }

    public double? LaserPower { get; set; }

    public double? GasFlow { get; set; }

    public double? OxygenLevel { get; set; }

    public double? HumidityPercent { get; set; }

    public bool IsConnected { get; set; }

    [Column(TypeName = "TEXT")]
    public string? RawDataJson { get; set; }
}
