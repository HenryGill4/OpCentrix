using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OpCentrix.Models
{
    // Represents a scheduled job on a specific machine with enhanced analytics data
    public class Job
    {
        public int Id { get; set; }

        #region Core Scheduling Data
        
        // Machine that will run the job (required)
        [Required]
        public string MachineId { get; set; } = string.Empty;

        // Scheduled time range
        [Required]
        public DateTime ScheduledStart { get; set; }
        
        [Required]
        public DateTime ScheduledEnd { get; set; }

        // Actual time tracking for performance analysis
        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }

        // Audit trail
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public string LastModifiedBy { get; set; } = string.Empty;

        #endregion

        #region Part and Production Data

        // Linked part information
        [Required]
        public int PartId { get; set; }
        
        [Required]
        public string PartNumber { get; set; } = string.Empty;
        
        public virtual Part? Part { get; set; }

        // Production details
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; } = 1;
        
        public int ProducedQuantity { get; set; } = 0;
        public int DefectQuantity { get; set; } = 0;
        public int ReworkQuantity { get; set; } = 0;

        #endregion

        #region Performance and Time Tracking

        // Time estimates and actuals (in hours)
        public double EstimatedHours { get; set; }
        
        [NotMapped]
        public double ActualHours => ActualStart.HasValue && ActualEnd.HasValue 
            ? (ActualEnd.Value - ActualStart.Value).TotalHours 
            : 0;

        // Setup and changeover tracking
        public double SetupTimeMinutes { get; set; } = 0;
        public double ChangeoverTimeMinutes { get; set; } = 0;
        public string? PreviousJobPartNumber { get; set; }

        // Performance metrics
        [NotMapped]
        public double EfficiencyPercent => EstimatedHours > 0 && ActualHours > 0 
            ? Math.Round((EstimatedHours / ActualHours) * 100, 2) 
            : 0;

        #endregion

        #region Cost Tracking

        // Cost per hour data
        [Column(TypeName = "decimal(10,2)")]
        public decimal LaborCostPerHour { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal MaterialCostPerUnit { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal OverheadCostPerHour { get; set; } = 0;

        // Calculated costs (NotMapped to avoid EF issues)
        [NotMapped]
        public decimal EstimatedLaborCost => (decimal)EstimatedHours * LaborCostPerHour;
        
        [NotMapped]
        public decimal EstimatedMaterialCost => Quantity * MaterialCostPerUnit;
        
        [NotMapped]
        public decimal EstimatedTotalCost => EstimatedLaborCost + EstimatedMaterialCost + ((decimal)EstimatedHours * OverheadCostPerHour);

        [NotMapped]
        public decimal ActualLaborCost => (decimal)ActualHours * LaborCostPerHour;
        
        [NotMapped]
        public decimal ActualMaterialCost => ProducedQuantity * MaterialCostPerUnit;

        #endregion

        #region Resource Requirements

        // Skills and certifications required
        public string RequiredSkills { get; set; } = string.Empty;
        
        // Tooling and equipment needed
        public string RequiredTooling { get; set; } = string.Empty;
        
        // Materials and consumables
        public string RequiredMaterials { get; set; } = string.Empty;
        
        // Special instructions or procedures
        public string SpecialInstructions { get; set; } = string.Empty;

        #endregion

        #region Process and Quality Data

        // Process parameters (stored as JSON)
        public string ProcessParameters { get; set; } = "{}";
        
        // Quality checkpoints and results (stored as JSON)
        public string QualityCheckpoints { get; set; } = "{}";
        
        // Machine utilization during job
        public double MachineUtilizationPercent { get; set; } = 0;
        
        // Energy consumption (kWh)
        public double EnergyConsumptionKwh { get; set; } = 0;

        #endregion

        #region Status and Workflow

        // Enhanced status tracking
        [Required]
        public string Status { get; set; } = "Scheduled";
        
        // Priority level (1 = highest, 5 = lowest)
        [Range(1, 5)]
        public int Priority { get; set; } = 3;
        
        // Customer or order information
        public string CustomerOrderNumber { get; set; } = string.Empty;
        public DateTime? CustomerDueDate { get; set; }
        
        // Rush job indicator
        public bool IsRushJob { get; set; } = false;
        
        // Hold reasons (if job is on hold)
        public string HoldReason { get; set; } = string.Empty;

        #endregion

        #region Personnel

        public string? Operator { get; set; }
        public string? QualityInspector { get; set; }
        public string? Supervisor { get; set; }
        public string? Notes { get; set; }

        #endregion

        #region Computed Properties for Analytics (NotMapped)

        // Duration calculations
        [NotMapped]
        public TimeSpan ScheduledDuration => ScheduledEnd - ScheduledStart;
        
        [NotMapped]
        public TimeSpan? ActualDuration => ActualStart.HasValue && ActualEnd.HasValue 
            ? ActualEnd.Value - ActualStart.Value 
            : null;

        [NotMapped]
        public double DurationHours => ScheduledDuration.TotalHours;
        
        [NotMapped]
        public int DurationMinutes => (int)ScheduledDuration.TotalMinutes;

        // Quality metrics
        [NotMapped]
        public double QualityScore => ProducedQuantity > 0 
            ? Math.Round(((double)(ProducedQuantity - DefectQuantity) / ProducedQuantity) * 100, 2) 
            : 100;
        
        [NotMapped]
        public double DefectRate => ProducedQuantity > 0 
            ? Math.Round(((double)DefectQuantity / ProducedQuantity) * 100, 2) 
            : 0;

        [NotMapped]
        public double ReworkRate => ProducedQuantity > 0 
            ? Math.Round(((double)ReworkQuantity / ProducedQuantity) * 100, 2) 
            : 0;

        // Schedule performance
        [NotMapped]
        public bool IsOnTime => ActualEnd.HasValue && ActualEnd.Value <= ScheduledEnd;
        
        [NotMapped]
        public bool IsStartedOnTime => ActualStart.HasValue && ActualStart.Value <= ScheduledStart.AddMinutes(15); // 15 min tolerance
        
        [NotMapped]
        public double ScheduleVarianceHours => ActualDuration.HasValue 
            ? (ActualDuration.Value - ScheduledDuration).TotalHours 
            : 0;

        // Cost performance
        [NotMapped]
        public decimal CostVariance => EstimatedTotalCost - (ActualLaborCost + ActualMaterialCost);
        
        [NotMapped]
        public double CostEfficiencyPercent => EstimatedTotalCost > 0 
            ? Math.Round((double)((ActualLaborCost + ActualMaterialCost) / EstimatedTotalCost) * 100, 2) 
            : 0;

        #endregion

        #region Helper Methods

        // Helper method to check if this job overlaps with another
        public bool OverlapsWith(Job other)
        {
            if (other == null || MachineId != other.MachineId)
                return false;

            // Two jobs overlap if neither ends before the other starts
            return !(ScheduledEnd <= other.ScheduledStart || ScheduledStart >= other.ScheduledEnd);
        }

        // Helper method to calculate position in grid
        public double CalculateGridPosition(DateTime gridStartDate, int slotMinutes)
        {
            var totalMinutes = (ScheduledStart - gridStartDate.Date).TotalMinutes;
            return Math.Max(0, totalMinutes / slotMinutes);
        }

        // Helper method to calculate width in grid
        public double CalculateGridWidth(int slotMinutes)
        {
            return Math.Max(0.5, DurationMinutes / (double)slotMinutes);
        }

        // Helper method to get status color
        public string GetStatusColor()
        {
            return Status?.ToLower() switch
            {
                "completed" => "#10B981", // green
                "in-progress" => "#F59E0B", // amber
                "delayed" => "#EF4444", // red
                "cancelled" => "#6B7280", // gray
                "on-hold" => "#8B5CF6", // purple
                _ => "#3B82F6" // blue (scheduled)
            };
        }

        // Get priority color
        public string GetPriorityColor()
        {
            return Priority switch
            {
                1 => "#DC2626", // red (critical)
                2 => "#F59E0B", // orange (high)
                3 => "#10B981", // green (normal)
                4 => "#6B7280", // gray (low)
                5 => "#9CA3AF", // light gray (lowest)
                _ => "#6B7280"
            };
        }

        // Parse process parameters from JSON
        public Dictionary<string, object> GetProcessParameters()
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(ProcessParameters) ?? new();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        // Set process parameters as JSON
        public void SetProcessParameters(Dictionary<string, object> parameters)
        {
            ProcessParameters = JsonSerializer.Serialize(parameters);
        }

        // Parse quality checkpoints from JSON
        public Dictionary<string, object> GetQualityCheckpoints()
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(QualityCheckpoints) ?? new();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        // Set quality checkpoints as JSON
        public void SetQualityCheckpoints(Dictionary<string, object> checkpoints)
        {
            QualityCheckpoints = JsonSerializer.Serialize(checkpoints);
        }

        #endregion
    }
}

// Enums for better type safety
public enum JobStatus
{
    Scheduled,
    InProgress,
    OnHold,
    Completed,
    Cancelled,
    Delayed
}

public enum JobPriority
{
    Critical = 1,
    High = 2,
    Normal = 3,
    Low = 4,
    Lowest = 5
}
