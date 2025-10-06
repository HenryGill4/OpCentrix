using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models.MaintenanceV2
{
    // Core asset that can receive maintenance
    public class MaintenanceAsset
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(40)]
        public string AssetType { get; set; } = "Machine"; // Machine / Area / Tool / GeneralTask
        [MaxLength(50)]
        public string? MachineId { get; set; }
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(120)]
        public string? Location { get; set; }
        [MaxLength(120)]
        public string? Department { get; set; }
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)]
        public string CreatedBy { get; set; } = "System";
        public string? MetadataJson { get; set; }
        public ICollection<MaintenanceScheduleInstance> Schedules { get; set; } = new List<MaintenanceScheduleInstance>();
    }

    public class MaintenanceProcedureTemplate
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [MaxLength(40)]
        public string? AppliesToAssetType { get; set; }
        public string? DefaultInstructions { get; set; }
        public string? SafetyNotes { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public int DefaultPriority { get; set; } = 3;
        [MaxLength(30)]
        public string RecurrenceStrategy { get; set; } = "FactorBased"; // FactorBased|Calendar|Hybrid
        [MaxLength(5)]
        public string GroupCombinationOperator { get; set; } = "OR"; // OR / AND across groups
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int Version { get; set; } = 1;
        public string? Tags { get; set; }
        public string? MetadataJson { get; set; }
        public ICollection<MaintenanceProcedureTemplateFactor> Factors { get; set; } = new List<MaintenanceProcedureTemplateFactor>();
        public ICollection<MaintenanceScheduleInstance> ScheduleInstances { get; set; } = new List<MaintenanceScheduleInstance>();
    }

    public class MaintenanceFactorDefinition
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(80)]
        public string Code { get; set; } = string.Empty; // RUN_HOURS etc
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [MaxLength(30)]
        public string FactorType { get; set; } = "Counter";
        [MaxLength(30)]
        public string SourceType { get; set; } = "BuildData";
        public string? ParametersSchemaJson { get; set; }
        [MaxLength(30)]
        public string Unit { get; set; } = string.Empty;
        public bool SupportsParameterization { get; set; }
        public bool IsSystem { get; set; } = true;
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<MaintenanceProcedureTemplateFactor> TemplateFactors { get; set; } = new List<MaintenanceProcedureTemplateFactor>();
    }

    public class MaintenanceProcedureTemplateFactor
    {
        public int Id { get; set; }
        public int ProcedureTemplateId { get; set; }
        public MaintenanceProcedureTemplate ProcedureTemplate { get; set; } = null!;
        public int FactorDefinitionId { get; set; }
        public MaintenanceFactorDefinition FactorDefinition { get; set; } = null!;
        public string? ParameterJson { get; set; }
        public decimal ThresholdValue { get; set; }
        [MaxLength(30)]
        public string ComparisonOperator { get; set; } = "GreaterOrEqual";
        [MaxLength(10)]
        public string LogicalGroupKey { get; set; } = "A";
        [MaxLength(5)]
        public string GroupOperator { get; set; } = "AND"; // AND/OR within group
        public bool ResetCounterOnCompletion { get; set; }
        public bool Optional { get; set; }
        public int SequenceOrder { get; set; }
        public int? Weight { get; set; }
        public string? Notes { get; set; }
    }

    public class MaintenanceScheduleInstance
    {
        public int Id { get; set; }
        public int ProcedureTemplateId { get; set; }
        public MaintenanceProcedureTemplate ProcedureTemplate { get; set; } = null!;
        public int AssetId { get; set; }
        public MaintenanceAsset Asset { get; set; } = null!;
        [MaxLength(150)]
        public string? CustomName { get; set; }
        public bool Enabled { get; set; } = true;
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active|Paused|Archived
        public DateTime? NextDueAt { get; set; }
        public DateTime? LastEvaluatedAt { get; set; }
        public DateTime? LastCompletionAt { get; set; }
        public int? LastOccurrenceId { get; set; }
        public string? OverrideJson { get; set; }
        public int? PriorityOverride { get; set; }
        public string? CalendarPattern { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<MaintenanceOccurrence> Occurrences { get; set; } = new List<MaintenanceOccurrence>();
    }

    public class MaintenanceOccurrence
    {
        public int Id { get; set; }
        public int ScheduleInstanceId { get; set; }
        public MaintenanceScheduleInstance ScheduleInstance { get; set; } = null!;
        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CompletedByUserId { get; set; }
        [MaxLength(20)]
        public string Status { get; set; } = "Open"; // Open|Completed|Skipped|Cancelled|Overdue
        public string EvaluationSnapshotJson { get; set; } = "{}";
        public string? CompletionNotes { get; set; }
        public int? ActualDurationMinutes { get; set; }
        public bool AutoGenerated { get; set; } = true;
        [MaxLength(10)]
        public string? CreatedFromFactorGroup { get; set; }
    }

    public class MaintenanceCounterAggregate
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public MaintenanceAsset Asset { get; set; } = null!;
        public int FactorDefinitionId { get; set; }
        public MaintenanceFactorDefinition FactorDefinition { get; set; } = null!;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public double Value { get; set; }
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class MaintenanceCounterBaseline
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public MaintenanceAsset Asset { get; set; } = null!;
        public int FactorDefinitionId { get; set; }
        public MaintenanceFactorDefinition FactorDefinition { get; set; } = null!;
        public int OccurrenceId { get; set; }
        public MaintenanceOccurrence Occurrence { get; set; } = null!;
        public double BaselineValue { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }

    // OperationalTask entity (ad-hoc tasks)
    public class OperationalTask
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(160)]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [MaxLength(40)] public string TaskType { get; set; } = "General";
        [MaxLength(50)] public string? MachineId { get; set; }
        public int? AssetId { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Open"; // Open|InProgress|Completed|Cancelled
        public int Priority { get; set; } = 3; // 1 highest
        public int CreatedByUserId { get; set; }
        public int? AssignedUserId { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        [MaxLength(60)] public string? Category { get; set; }
        [MaxLength(120)] public string? Tags { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // NEW: Unified scheduling / trigger configuration JSON (wizard output)
        public string? ConfigJson { get; set; }
        // NEW: Overdue flag (evaluation sets when task considered overdue separately from Status)
        public int? OverdueFlag { get; set; } // null/0 = not overdue, 1 = overdue
        // NEW: Explicit baseline reset timestamp (when user manually resets/early completes)
        public DateTime? LastResetAt { get; set; }

        // Navigation
        public MaintenanceAsset? Asset { get; set; }

        // NOT MAPPED: Interval progress summaries (computed server-side)
        [NotMapped]
        public List<string> ProgressSummaries { get; set; } = new();
    }
}
