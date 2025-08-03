using System;
using System.Collections.Generic;

namespace OpCentrix.TempModels;

public partial class ProductionStage
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public string? Description { get; set; }

    public int DefaultSetupMinutes { get; set; }

    public int DefaultHourlyRate { get; set; }

    public int RequiresQualityCheck { get; set; }

    public int RequiresApproval { get; set; }

    public int AllowSkip { get; set; }

    public int IsOptional { get; set; }

    public string? RequiredRole { get; set; }

    public string CustomFieldsConfig { get; set; } = null!;

    public string? AssignedMachineIds { get; set; }

    public int RequiresMachineAssignment { get; set; }

    public string? DefaultMachineId { get; set; }

    public string StageColor { get; set; } = null!;

    public string StageIcon { get; set; } = null!;

    public string? Department { get; set; }

    public int AllowParallelExecution { get; set; }

    public int DefaultMaterialCost { get; set; }

    public double DefaultDurationHours { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastModifiedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string LastModifiedBy { get; set; } = null!;

    public int IsActive { get; set; }
}
