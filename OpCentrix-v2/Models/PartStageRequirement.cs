using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OpCentrix.Models;

public class PartStageRequirement
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PartId { get; set; }

    [Required]
    public int ProductionStageId { get; set; }

    [Required]
    [Range(1, 100)]
    public int ExecutionOrder { get; set; } = 1;

    public bool IsRequired { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public bool AllowParallelExecution { get; set; }

    public bool IsBlocking { get; set; } = true;

    #region Timing & Cost Overrides

    public double? EstimatedHours { get; set; }

    public int? SetupTimeMinutes { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal? HourlyRateOverride { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal EstimatedCost { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal MaterialCost { get; set; }

    #endregion

    #region Machine Assignment

    [StringLength(50)]
    public string? AssignedMachineId { get; set; }

    public bool RequiresSpecificMachine { get; set; }

    [StringLength(200)]
    public string? PreferredMachineIds { get; set; }

    #endregion

    #region Custom Field Values

    [Column(TypeName = "TEXT")]
    public string CustomFieldValues { get; set; } = "{}";

    #endregion

    #region Process Config

    [Column(TypeName = "TEXT")]
    public string StageParameters { get; set; } = "{}";

    [Column(TypeName = "TEXT")]
    public string RequiredMaterials { get; set; } = "[]";

    [StringLength(500)]
    public string RequiredTooling { get; set; } = "";

    [Column(TypeName = "TEXT")]
    public string QualityRequirements { get; set; } = "{}";

    #endregion

    #region Notes

    [Column(TypeName = "TEXT")]
    public string SpecialInstructions { get; set; } = "";

    [Column(TypeName = "TEXT")]
    public string RequirementNotes { get; set; } = "";

    #endregion

    #region Learning (EMA)

    public double? ActualAverageDurationHours { get; set; }

    public int ActualSampleCount { get; set; }

    public double? LastActualDurationHours { get; set; }

    [StringLength(20)]
    public string EstimateSource { get; set; } = "Manual";

    public DateTime? EstimateLastUpdated { get; set; }

    #endregion

    #region Audit

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    [Required]
    [StringLength(100)]
    public string LastModifiedBy { get; set; } = "System";

    #endregion

    #region Navigation Properties

    public virtual Part Part { get; set; } = null!;
    public virtual ProductionStage ProductionStage { get; set; } = null!;

    #endregion

    #region Helper Methods

    public double GetEffectiveEstimatedHours()
    {
        return EstimatedHours ?? ProductionStage?.DefaultDurationHours ?? 1.0;
    }

    public decimal GetEffectiveHourlyRate()
    {
        return HourlyRateOverride ?? ProductionStage?.DefaultHourlyRate ?? 85.00m;
    }

    public decimal CalculateTotalEstimatedCost()
    {
        if (EstimatedCost > 0) return EstimatedCost;

        var hours = (decimal)GetEffectiveEstimatedHours();
        var rate = GetEffectiveHourlyRate();
        var setupMinutes = SetupTimeMinutes ?? ProductionStage?.DefaultSetupMinutes ?? 30;
        var setupCost = (decimal)(setupMinutes / 60.0) * rate;

        return setupCost + (hours * rate) + MaterialCost;
    }

    public Dictionary<string, object> GetCustomFieldValues()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CustomFieldValues))
                return new Dictionary<string, object>();

            var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(CustomFieldValues);
            var result = new Dictionary<string, object>();

            foreach (var kvp in values ?? new Dictionary<string, JsonElement>())
            {
                object value = kvp.Value.ValueKind switch
                {
                    JsonValueKind.String => kvp.Value.GetString() ?? "",
                    JsonValueKind.Number => kvp.Value.GetDecimal(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => kvp.Value.ToString() ?? ""
                };
                result[kvp.Key] = value;
            }

            return result;
        }
        catch (JsonException)
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetCustomFieldValues(Dictionary<string, object> values)
    {
        try
        {
            CustomFieldValues = JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (JsonException)
        {
            CustomFieldValues = "{}";
        }
    }

    public T? GetCustomFieldValue<T>(string fieldName, T? defaultValue = default)
    {
        var values = GetCustomFieldValues();
        if (values.TryGetValue(fieldName, out var value))
        {
            try
            {
                return (T?)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    public void SetCustomFieldValue(string fieldName, object value)
    {
        var values = GetCustomFieldValues();
        values[fieldName] = value;
        SetCustomFieldValues(values);
    }

    public List<string> GetPreferredMachineIds()
    {
        if (string.IsNullOrWhiteSpace(PreferredMachineIds))
            return new List<string>();

        return PreferredMachineIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => id.Trim())
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();
    }

    public void SetPreferredMachineIds(List<string> machineIds)
    {
        PreferredMachineIds = string.Join(",", machineIds.Where(id => !string.IsNullOrWhiteSpace(id)));
    }

    public bool CanMachineExecute(string machineId)
    {
        if (string.IsNullOrWhiteSpace(machineId))
            return false;

        if (RequiresSpecificMachine)
            return AssignedMachineId == machineId;

        return ProductionStage?.CanMachineExecuteStage(machineId) ?? true;
    }

    public string? GetBestMachineId(List<string> availableMachineIds)
    {
        if (RequiresSpecificMachine && !string.IsNullOrWhiteSpace(AssignedMachineId))
        {
            if (availableMachineIds.Contains(AssignedMachineId))
                return AssignedMachineId;
        }

        var preferredMachines = GetPreferredMachineIds();
        foreach (var machineId in preferredMachines)
        {
            if (availableMachineIds.Contains(machineId) && CanMachineExecute(machineId))
                return machineId;
        }

        return availableMachineIds.FirstOrDefault(id => CanMachineExecute(id));
    }

    public List<PartStageRequirement> GetDependencies(List<PartStageRequirement> allPartStages)
    {
        return allPartStages
            .Where(s => s.PartId == PartId && s.ExecutionOrder < ExecutionOrder && s.IsBlocking)
            .OrderBy(s => s.ExecutionOrder)
            .ToList();
    }

    #endregion
}
