using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OpCentrix.Models;

public class ProductionStage
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string StageSlug { get; set; } = string.Empty;

    public bool HasBuiltInPage { get; set; }

    public bool RequiresSerialNumber { get; set; }

    [Required]
    public int DisplayOrder { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public int DefaultSetupMinutes { get; set; } = 30;

    [Column(TypeName = "decimal(8,2)")]
    public decimal DefaultHourlyRate { get; set; } = 85.00m;

    public bool RequiresQualityCheck { get; set; } = true;

    public bool RequiresApproval { get; set; }

    public bool AllowSkip { get; set; }

    public bool IsOptional { get; set; }

    [StringLength(50)]
    public string? RequiredRole { get; set; }

    [Column(TypeName = "TEXT")]
    public string CustomFieldsConfig { get; set; } = "[]";

    [StringLength(500)]
    public string? AssignedMachineIds { get; set; }

    public bool RequiresMachineAssignment { get; set; }

    [StringLength(50)]
    public string? DefaultMachineId { get; set; }

    [StringLength(7)]
    public string StageColor { get; set; } = "#007bff";

    [StringLength(50)]
    public string StageIcon { get; set; } = "fas fa-cogs";

    [StringLength(100)]
    public string? Department { get; set; }

    public bool AllowParallelExecution { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal DefaultMaterialCost { get; set; }

    public double DefaultDurationHours { get; set; } = 1.0;

    public bool IsBatchStage { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    [StringLength(100)]
    public string LastModifiedBy { get; set; } = "System";

    // Navigation
    public virtual ICollection<PartStageRequirement> PartStageRequirements { get; set; } = new List<PartStageRequirement>();
    public virtual ICollection<StageExecution> StageExecutions { get; set; } = new List<StageExecution>();

    #region Helper Methods

    public List<CustomFieldDefinition> GetCustomFields()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CustomFieldsConfig))
                return new List<CustomFieldDefinition>();

            return JsonSerializer.Deserialize<List<CustomFieldDefinition>>(CustomFieldsConfig)
                   ?? new List<CustomFieldDefinition>();
        }
        catch (JsonException)
        {
            return new List<CustomFieldDefinition>();
        }
    }

    public void SetCustomFields(List<CustomFieldDefinition> fields)
    {
        try
        {
            CustomFieldsConfig = JsonSerializer.Serialize(fields, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (JsonException)
        {
            CustomFieldsConfig = "[]";
        }
    }

    public List<string> GetAssignedMachineIds()
    {
        if (string.IsNullOrWhiteSpace(AssignedMachineIds))
            return new List<string>();

        return AssignedMachineIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => id.Trim())
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();
    }

    public void SetAssignedMachineIds(List<string> machineIds)
    {
        AssignedMachineIds = string.Join(",", machineIds.Where(id => !string.IsNullOrWhiteSpace(id)));
    }

    public bool CanMachineExecuteStage(string machineId)
    {
        if (string.IsNullOrWhiteSpace(machineId))
            return false;

        if (!RequiresMachineAssignment)
            return true;

        var assignedMachines = GetAssignedMachineIds();
        return assignedMachines.Contains(machineId);
    }

    public decimal GetTotalEstimatedCost()
    {
        var laborCost = (decimal)DefaultDurationHours * DefaultHourlyRate;
        var setupCost = (decimal)(DefaultSetupMinutes / 60.0) * DefaultHourlyRate;
        return laborCost + setupCost + DefaultMaterialCost;
    }

    #endregion
}

public record CustomFieldDefinition
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "text";
    public string Label { get; set; } = "";
    public string? Description { get; set; }
    public bool Required { get; set; }
    public string? DefaultValue { get; set; }
    public List<string>? Options { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public string? ValidationPattern { get; set; }
    public string? Unit { get; set; }
    public int DisplayOrder { get; set; }
    public string? PlaceholderText { get; set; }
    public bool IsReadOnly { get; set; }
}
