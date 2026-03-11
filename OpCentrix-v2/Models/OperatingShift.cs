using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models;

public class OperatingShift
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    [StringLength(50)]
    public string DaysOfWeek { get; set; } = "Mon,Tue,Wed,Thu,Fri";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
