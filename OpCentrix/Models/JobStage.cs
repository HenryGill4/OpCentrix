using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models.JobStaging;

/// <summary>
/// Represents a manufacturing stage in the multi-stage production process
/// Task 11: Modular Multi-Stage Scheduling
/// </summary>
public class JobStage
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Reference to the parent job
    /// </summary>
    [Required]
    public int JobId { get; set; }

    /// <summary>
    /// Navigation property to parent job
    /// </summary>
    public virtual Job Job { get; set; } = null!;

    /// <summary>
    /// Stage type (Printing, EDM, Cerakoting, Assembly)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string StageType { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the stage
    /// </summary>
    [Required]
    [StringLength(100)]
    public string StageName { get; set; } = string.Empty;

    /// <summary>
    /// Order of execution in the job workflow
    /// </summary>
    [Range(1, 100)]
    public int ExecutionOrder { get; set; } = 1;

    /// <summary>
    /// Department responsible for this stage
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Machine or workstation for this stage (references Machine.MachineId)
    /// </summary>
    [StringLength(50)]
    public string? MachineId { get; set; }

    /// <summary>
    /// Navigation property to machine (configured via fluent API)
    /// Note: This uses MachineId (string) to match Machine.MachineId, not Machine.Id (int)
    /// </summary>
    public virtual Machine? Machine { get; set; }

    /// <summary>
    /// Scheduled start time for this stage
    /// </summary>
    public DateTime ScheduledStart { get; set; }

    /// <summary>
    /// Scheduled end time for this stage
    /// </summary>
    public DateTime ScheduledEnd { get; set; }

    /// <summary>
    /// Actual start time (when stage was actually started)
    /// </summary>
    public DateTime? ActualStart { get; set; }

    /// <summary>
    /// Actual completion time
    /// </summary>
    public DateTime? ActualEnd { get; set; }

    /// <summary>
    /// Current status of this stage
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Scheduled";

    /// <summary>
    /// Priority level (1 = highest, 5 = lowest)
    /// </summary>
    [Range(1, 5)]
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Estimated duration in hours
    /// </summary>
    [Range(0, 168)] // Max 1 week
    public double EstimatedDurationHours { get; set; } = 1.0;

    /// <summary>
    /// Actual duration in hours (calculated from ActualStart/End)
    /// </summary>
    [NotMapped]
    public double? ActualDurationHours => ActualStart.HasValue && ActualEnd.HasValue 
        ? (ActualEnd.Value - ActualStart.Value).TotalHours 
        : null;

    /// <summary>
    /// Whether this stage can be started (dependencies met)
    /// </summary>
    public bool CanStart { get; set; } = true;

    /// <summary>
    /// Required setup time before stage can begin
    /// </summary>
    [Range(0, 24)]
    public double SetupTimeHours { get; set; } = 0;

    /// <summary>
    /// Required cooldown/cleanup time after stage completion
    /// </summary>
    [Range(0, 24)]
    public double CooldownTimeHours { get; set; } = 0;

    /// <summary>
    /// Operator assigned to this stage
    /// </summary>
    [StringLength(100)]
    public string? AssignedOperator { get; set; }

    /// <summary>
    /// Stage-specific notes and instructions
    /// </summary>
    [StringLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Quality control requirements for this stage
    /// </summary>
    [StringLength(1000)]
    public string? QualityRequirements { get; set; }

    /// <summary>
    /// Materials or consumables required for this stage
    /// </summary>
    [StringLength(1000)]
    public string? RequiredMaterials { get; set; }

    /// <summary>
    /// Tools or fixtures required for this stage
    /// </summary>
    [StringLength(1000)]
    public string? RequiredTooling { get; set; }

    /// <summary>
    /// Estimated cost for this stage
    /// </summary>
    [Range(0, 100000)]
    public decimal EstimatedCost { get; set; } = 0;

    /// <summary>
    /// Actual cost incurred for this stage
    /// </summary>
    [Range(0, 100000)]
    public decimal? ActualCost { get; set; }

    /// <summary>
    /// Whether this stage blocks subsequent stages
    /// </summary>
    public bool IsBlocking { get; set; } = true;

    /// <summary>
    /// Whether this stage can run in parallel with others
    /// </summary>
    public bool AllowParallel { get; set; } = false;

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    [Range(0, 100)]
    public double ProgressPercent { get; set; } = 0;

    /// <summary>
    /// User who created this stage
    /// </summary>
    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who last modified this stage
    /// </summary>
    [StringLength(100)]
    public string LastModifiedBy { get; set; } = "System";

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Collection of dependencies for this stage
    /// </summary>
    public virtual ICollection<JobStageDependency> Dependencies { get; set; } = new List<JobStageDependency>();

    /// <summary>
    /// Collection of stages that depend on this one
    /// </summary>
    public virtual ICollection<JobStageDependency> Dependents { get; set; } = new List<JobStageDependency>();

    /// <summary>
    /// Collection of stage notes and updates
    /// </summary>
    public virtual ICollection<StageNote> StageNotes { get; set; } = new List<StageNote>();

    #region Computed Properties

    /// <summary>
    /// Get stage status color for UI
    /// </summary>
    [NotMapped]
    public string StatusColor => Status.ToLower() switch
    {
        "scheduled" => "#3B82F6",     // blue
        "ready" => "#10B981",         // green
        "in-progress" => "#F59E0B",   // amber
        "completed" => "#059669",     // dark green
        "on-hold" => "#F97316",       // orange
        "cancelled" => "#6B7280",     // gray
        "delayed" => "#EF4444",       // red
        "blocked" => "#DC2626",       // dark red
        _ => "#9CA3AF"                // light gray
    };

    /// <summary>
    /// Get department color for UI
    /// </summary>
    [NotMapped]
    public string DepartmentColor => Department.ToLower() switch
    {
        "printing" => "#8B5CF6",      // purple
        "edm" => "#06B6D4",           // cyan
        "cerakoting" => "#F59E0B",    // amber
        "assembly" => "#10B981",      // green
        "inspection" => "#3B82F6",    // blue
        "shipping" => "#6B7280",      // gray
        _ => "#9CA3AF"                // light gray
    };

    /// <summary>
    /// Check if stage is overdue
    /// </summary>
    [NotMapped]
    public bool IsOverdue => Status != "Completed" && ScheduledEnd < DateTime.UtcNow;

    /// <summary>
    /// Check if stage is ready to start
    /// </summary>
    [NotMapped]
    public bool IsReadyToStart => CanStart && Status == "Scheduled" && 
        Dependencies.All(d => d.RequiredStage.Status == "Completed");

    /// <summary>
    /// Get variance from scheduled duration
    /// </summary>
    [NotMapped]
    public double? DurationVarianceHours => ActualDurationHours.HasValue 
        ? ActualDurationHours.Value - EstimatedDurationHours 
        : null;

    #endregion

    #region Helper Methods

    /// <summary>
    /// Calculate total time including setup and cooldown
    /// </summary>
    public double GetTotalTimeRequired()
    {
        return SetupTimeHours + EstimatedDurationHours + CooldownTimeHours;
    }

    /// <summary>
    /// Check if stage conflicts with another stage on the same machine
    /// </summary>
    public bool ConflictsWith(JobStage other)
    {
        if (string.IsNullOrEmpty(MachineId) || MachineId != other.MachineId)
            return false;

        return ScheduledStart < other.ScheduledEnd && ScheduledEnd > other.ScheduledStart;
    }

    /// <summary>
    /// Update stage progress and status
    /// </summary>
    public void UpdateProgress(double progressPercent, string? statusUpdate = null)
    {
        ProgressPercent = Math.Max(0, Math.Min(100, progressPercent));
        
        if (!string.IsNullOrEmpty(statusUpdate))
        {
            Status = statusUpdate;
        }
        else if (ProgressPercent >= 100)
        {
            Status = "Completed";
            ActualEnd = DateTime.UtcNow;
        }
        else if (ProgressPercent > 0 && Status == "Scheduled")
        {
            Status = "In-Progress";
            ActualStart = DateTime.UtcNow;
        }

        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Start the stage
    /// </summary>
    public void StartStage(string? operatorName = null)
    {
        if (!IsReadyToStart)
            throw new InvalidOperationException("Stage is not ready to start");

        Status = "In-Progress";
        ActualStart = DateTime.UtcNow;
        ProgressPercent = 0;
        
        if (!string.IsNullOrEmpty(operatorName))
            AssignedOperator = operatorName;

        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Complete the stage
    /// </summary>
    public void CompleteStage(decimal? actualCost = null)
    {
        Status = "Completed";
        ActualEnd = DateTime.UtcNow;
        ProgressPercent = 100;
        
        if (actualCost.HasValue)
            ActualCost = actualCost.Value;

        LastModifiedDate = DateTime.UtcNow;
    }

    #endregion
}

/// <summary>
/// Represents dependencies between job stages (JobStage-based, different from ProductionStage-based)
/// </summary>
public class JobStageDependency
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Stage that has the dependency
    /// </summary>
    [Required]
    public int DependentStageId { get; set; }

    /// <summary>
    /// Navigation property to dependent stage
    /// </summary>
    public virtual JobStage DependentStage { get; set; } = null!;

    /// <summary>
    /// Stage that must be completed first
    /// </summary>
    [Required]
    public int RequiredStageId { get; set; }

    /// <summary>
    /// Navigation property to required stage
    /// </summary>
    public virtual JobStage RequiredStage { get; set; } = null!;

    /// <summary>
    /// Type of dependency (FinishToStart, StartToStart, etc.)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string DependencyType { get; set; } = "FinishToStart";

    /// <summary>
    /// Lag time (in hours) between dependency completion and stage start
    /// </summary>
    [Range(-24, 168)] // Allow negative for overlap
    public double LagTimeHours { get; set; } = 0;

    /// <summary>
    /// Whether this dependency is mandatory or optional
    /// </summary>
    public bool IsMandatory { get; set; } = true;

    /// <summary>
    /// Notes about this dependency
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents notes and updates for job stages
/// </summary>
public class StageNote
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Reference to the job stage
    /// </summary>
    [Required]
    public int StageId { get; set; }

    /// <summary>
    /// Navigation property to stage
    /// </summary>
    public virtual JobStage Stage { get; set; } = null!;

    /// <summary>
    /// Note content
    /// </summary>
    [Required]
    [StringLength(2000)]
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Note type (Info, Warning, Issue, Update)
    /// </summary>
    [StringLength(20)]
    public string NoteType { get; set; } = "Info";

    /// <summary>
    /// Priority level (1 = highest, 5 = lowest)
    /// </summary>
    [Range(1, 5)]
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Whether this note is visible to all users
    /// </summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// User who created the note
    /// </summary>
    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Get color for note type
    /// </summary>
    [NotMapped]
    public string NoteTypeColor => NoteType.ToLower() switch
    {
        "info" => "#3B82F6",      // blue
        "warning" => "#F59E0B",   // amber
        "issue" => "#EF4444",     // red
        "update" => "#10B981",    // green
        _ => "#6B7280"            // gray
    };
}