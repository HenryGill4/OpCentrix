using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

/// <summary>
/// Represents archived jobs for cleanup and historical data management
/// Mirrors the Job entity but for long-term storage of completed/cancelled jobs
/// </summary>
public class ArchivedJob
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Original job ID from the Jobs table
    /// </summary>
    [Required]
    public int OriginalJobId { get; set; }

    /// <summary>
    /// Machine that ran the job
    /// </summary>
    [Required]
    [StringLength(50)]
    public string MachineId { get; set; } = string.Empty;

    /// <summary>
    /// Part information
    /// </summary>
    [Required]
    public int PartId { get; set; }

    [Required]
    [StringLength(50)]
    public string PartNumber { get; set; } = string.Empty;

    [StringLength(500)]
    public string PartDescription { get; set; } = string.Empty;

    /// <summary>
    /// Scheduling information
    /// </summary>
    [Required]
    public DateTime ScheduledStart { get; set; }

    [Required]
    public DateTime ScheduledEnd { get; set; }

    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd { get; set; }

    /// <summary>
    /// Job details
    /// </summary>
    [Required]
    public int Quantity { get; set; } = 1;

    public int ProducedQuantity { get; set; } = 0;
    public int DefectQuantity { get; set; } = 0;
    public int ReworkQuantity { get; set; } = 0;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Completed";

    [Range(1, 5)]
    public int Priority { get; set; } = 3;

    public double EstimatedHours { get; set; } = 0;

    /// <summary>
    /// SLS-specific data
    /// </summary>
    [StringLength(100)]
    public string SlsMaterial { get; set; } = string.Empty;

    [StringLength(50)]
    public string PowderLotNumber { get; set; } = string.Empty;

    public double LaserPowerWatts { get; set; } = 0;
    public double ScanSpeedMmPerSec { get; set; } = 0;
    public double LayerThicknessMicrons { get; set; } = 0;
    public double EstimatedPowderUsageKg { get; set; } = 0;
    public double ActualPowderUsageKg { get; set; } = 0;

    /// <summary>
    /// Cost tracking
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal LaborCostPerHour { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    public decimal MaterialCostPerKg { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    public decimal MachineOperatingCostPerHour { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    public decimal ArgonCostPerHour { get; set; } = 0;

    /// <summary>
    /// Quality metrics
    /// </summary>
    public double DensityPercentage { get; set; } = 0;
    public double SurfaceRoughnessRa { get; set; } = 0;

    /// <summary>
    /// Personnel
    /// </summary>
    [StringLength(100)]
    public string Operator { get; set; } = string.Empty;

    [StringLength(100)]
    public string QualityInspector { get; set; } = string.Empty;

    [StringLength(100)]
    public string Supervisor { get; set; } = string.Empty;

    /// <summary>
    /// Customer information
    /// </summary>
    [StringLength(100)]
    public string CustomerOrderNumber { get; set; } = string.Empty;

    public DateTime? CustomerDueDate { get; set; }

    /// <summary>
    /// Notes and additional data
    /// </summary>
    [StringLength(1000)]
    public string Notes { get; set; } = string.Empty;

    [StringLength(500)]
    public string HoldReason { get; set; } = string.Empty;

    [StringLength(2000)]
    public string ProcessParameters { get; set; } = "{}";

    [StringLength(2000)]
    public string QualityCheckpoints { get; set; } = "{}";

    /// <summary>
    /// Archive information
    /// </summary>
    public DateTime ArchivedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string ArchivedBy { get; set; } = "System";

    [StringLength(500)]
    public string ArchiveReason { get; set; } = "Cleanup";

    /// <summary>
    /// Original job audit data
    /// </summary>
    public DateTime OriginalCreatedDate { get; set; }
    public DateTime OriginalLastModifiedDate { get; set; }

    [StringLength(100)]
    public string OriginalCreatedBy { get; set; } = string.Empty;

    [StringLength(100)]
    public string OriginalLastModifiedBy { get; set; } = string.Empty;

    /// <summary>
    /// Calculated properties for analytics
    /// </summary>
    [NotMapped]
    public TimeSpan ScheduledDuration => ScheduledEnd - ScheduledStart;

    [NotMapped]
    public TimeSpan? ActualDuration => ActualStart.HasValue && ActualEnd.HasValue 
        ? ActualEnd.Value - ActualStart.Value 
        : null;

    [NotMapped]
    public double ActualHours => ActualDuration?.TotalHours ?? 0;

    [NotMapped]
    public double EfficiencyPercent => EstimatedHours > 0 && ActualHours > 0 
        ? Math.Round((EstimatedHours / ActualHours) * 100, 2) 
        : 0;

    [NotMapped]
    public double QualityScore => ProducedQuantity > 0 
        ? Math.Round(((double)(ProducedQuantity - DefectQuantity) / ProducedQuantity) * 100, 2) 
        : 100;

    [NotMapped]
    public decimal EstimatedTotalCost
    {
        get
        {
            var laborCost = (decimal)EstimatedHours * LaborCostPerHour;
            var materialCost = (decimal)EstimatedPowderUsageKg * MaterialCostPerKg;
            var machineCost = (decimal)EstimatedHours * MachineOperatingCostPerHour;
            var argonCost = (decimal)EstimatedHours * ArgonCostPerHour;
            return laborCost + materialCost + machineCost + argonCost;
        }
    }

    [NotMapped]
    public decimal ActualTotalCost
    {
        get
        {
            var laborCost = (decimal)ActualHours * LaborCostPerHour;
            var materialCost = (decimal)ActualPowderUsageKg * MaterialCostPerKg;
            var machineCost = (decimal)ActualHours * MachineOperatingCostPerHour;
            var argonCost = (decimal)ActualHours * ArgonCostPerHour;
            return laborCost + materialCost + machineCost + argonCost;
        }
    }

    [NotMapped]
    public bool IsOnTime => ActualEnd.HasValue && ActualEnd.Value <= ScheduledEnd;

    [NotMapped]
    public double ScheduleVarianceHours => ActualDuration.HasValue 
        ? (ActualDuration.Value - ScheduledDuration).TotalHours 
        : 0;

    /// <summary>
    /// Create an ArchivedJob from a Job entity
    /// </summary>
    public static ArchivedJob FromJob(Job job, string archivedBy, string archiveReason = "Cleanup")
    {
        return new ArchivedJob
        {
            OriginalJobId = job.Id,
            MachineId = job.MachineId,
            PartId = job.PartId,
            PartNumber = job.PartNumber,
            PartDescription = job.Part?.Description ?? "",
            ScheduledStart = job.ScheduledStart,
            ScheduledEnd = job.ScheduledEnd,
            ActualStart = job.ActualStart,
            ActualEnd = job.ActualEnd,
            Quantity = job.Quantity,
            ProducedQuantity = job.ProducedQuantity,
            DefectQuantity = job.DefectQuantity,
            ReworkQuantity = job.ReworkQuantity,
            Status = job.Status,
            Priority = job.Priority,
            EstimatedHours = job.EstimatedHours,
            SlsMaterial = job.SlsMaterial,
            PowderLotNumber = job.PowderLotNumber,
            LaserPowerWatts = job.LaserPowerWatts,
            ScanSpeedMmPerSec = job.ScanSpeedMmPerSec,
            LayerThicknessMicrons = job.LayerThicknessMicrons,
            EstimatedPowderUsageKg = job.EstimatedPowderUsageKg,
            ActualPowderUsageKg = job.ActualPowderUsageKg,
            LaborCostPerHour = job.LaborCostPerHour,
            MaterialCostPerKg = job.MaterialCostPerKg,
            MachineOperatingCostPerHour = job.MachineOperatingCostPerHour,
            ArgonCostPerHour = job.ArgonCostPerHour,
            DensityPercentage = job.DensityPercentage,
            SurfaceRoughnessRa = job.SurfaceRoughnessRa,
            Operator = job.Operator ?? "",
            QualityInspector = job.QualityInspector ?? "",
            Supervisor = job.Supervisor ?? "",
            CustomerOrderNumber = job.CustomerOrderNumber,
            CustomerDueDate = job.CustomerDueDate,
            Notes = job.Notes ?? "",
            HoldReason = job.HoldReason,
            ProcessParameters = job.ProcessParameters,
            QualityCheckpoints = job.QualityCheckpoints,
            ArchivedDate = DateTime.UtcNow,
            ArchivedBy = archivedBy,
            ArchiveReason = archiveReason,
            OriginalCreatedDate = job.CreatedDate,
            OriginalLastModifiedDate = job.LastModifiedDate,
            OriginalCreatedBy = job.CreatedBy,
            OriginalLastModifiedBy = job.LastModifiedBy
        };
    }
}