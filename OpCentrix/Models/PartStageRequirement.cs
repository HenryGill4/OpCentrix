using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OpCentrix.Models
{
    /// <summary>
    /// Links a Part to a ProductionStage, indicating that this part requires this stage
    /// Enhanced to support custom field values and machine assignments
    /// </summary>
    public class PartStageRequirement
    {
        [Key]
        public int Id { get; set; }

        #region Core Relationships
        
        /// <summary>
        /// The Part that requires this stage
        /// </summary>
        [Required]
        public int PartId { get; set; }
        
        /// <summary>
        /// The ProductionStage that is required
        /// </summary>
        [Required]
        public int ProductionStageId { get; set; }

        /// <summary>
        /// Optional workflow template this requirement belongs to
        /// </summary>
        public int? WorkflowTemplateId { get; set; }
        
        #endregion
        
        #region Stage Configuration
        
        /// <summary>
        /// Order in which this stage should be executed (1 = first, 2 = second, etc.)
        /// </summary>
        [Required]
        [Range(1, 100)]
        public int ExecutionOrder { get; set; } = 1;
        
        /// <summary>
        /// Whether this stage is required (true) or optional (false) for this part
        /// </summary>
        public bool IsRequired { get; set; } = true;
        
        /// <summary>
        /// Whether this stage requirement is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Whether this stage can be executed in parallel with other stages
        /// </summary>
        public bool AllowParallelExecution { get; set; } = false;
        
        /// <summary>
        /// Whether this stage blocks subsequent stages until completion
        /// </summary>
        public bool IsBlocking { get; set; } = true;
        
        #endregion
        
        #region Timing and Cost Overrides
        
        /// <summary>
        /// Part-specific estimated hours (overrides stage default if set)
        /// </summary>
        public double? EstimatedHours { get; set; }
        
        /// <summary>
        /// Part-specific setup time in minutes (overrides stage default if set)
        /// </summary>
        public int? SetupTimeMinutes { get; set; }
        
        /// <summary>
        /// Part-specific hourly rate (overrides stage default if set)
        /// </summary>
        [Column(TypeName = "decimal(8,2)")]
        public decimal? HourlyRateOverride { get; set; }
        
        /// <summary>
        /// Part-specific total estimated cost (calculated if not provided)
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal EstimatedCost { get; set; } = 0.00m;
        
        /// <summary>
        /// Part-specific material cost for this stage
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal MaterialCost { get; set; } = 0.00m;
        
        #endregion
        
        #region NEW: Machine Assignment
        
        /// <summary>
        /// Specific machine ID assigned to execute this stage for this part
        /// </summary>
        [StringLength(50)]
        public string? AssignedMachineId { get; set; }
        
        /// <summary>
        /// Whether a specific machine assignment is required for this part-stage combination
        /// </summary>
        public bool RequiresSpecificMachine { get; set; } = false;
        
        /// <summary>
        /// Preferred machine IDs (comma-separated) for this stage, in order of preference
        /// </summary>
        [StringLength(200)]
        public string? PreferredMachineIds { get; set; }
        
        #endregion
        
        #region NEW: Custom Field Values
        
        /// <summary>
        /// JSON object containing values for custom fields defined in the ProductionStage
        /// Format: {"fieldName": "value", "anotherField": 123}
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string CustomFieldValues { get; set; } = "{}";
        
        #endregion
        
        #region Process Configuration
        
        /// <summary>
        /// JSON object for stage-specific process parameters
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string StageParameters { get; set; } = "{}";
        
        /// <summary>
        /// Required materials for this stage (JSON array)
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string RequiredMaterials { get; set; } = "[]";
        
        /// <summary>
        /// Required tooling for this stage (comma-separated)
        /// </summary>
        [StringLength(500)]
        public string RequiredTooling { get; set; } = "";
        
        /// <summary>
        /// Quality requirements for this stage (JSON object)
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string QualityRequirements { get; set; } = "{}";
        
        #endregion
        
        #region Notes and Instructions
        
        /// <summary>
        /// Special instructions for this part-stage combination
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string SpecialInstructions { get; set; } = "";
        
        /// <summary>
        /// General notes about this stage requirement
        /// </summary>
        [Column(TypeName = "TEXT")]
        [Display(Name = "Requirement Notes")]
        public string RequirementNotes { get; set; } = "";
        
        #endregion
        
        #region Audit Fields
        
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
        
        /// <summary>
        /// The Part that requires this stage
        /// </summary>
        public virtual Part Part { get; set; } = null!;
        
        /// <summary>
        /// The ProductionStage that is required
        /// </summary>
        public virtual ProductionStage ProductionStage { get; set; } = null!;
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Get the effective estimated hours (override or stage default)
        /// </summary>
        /// <returns>Part-specific hours if set, otherwise stage default</returns>
        public double GetEffectiveEstimatedHours()
        {
            return EstimatedHours ?? ProductionStage?.DefaultDurationHours ?? 1.0;
        }
        
        /// <summary>
        /// Get the effective hourly rate (override or stage default)
        /// </summary>
        /// <returns>Part-specific rate if set, otherwise stage default</returns>
        public decimal GetEffectiveHourlyRate()
        {
            return HourlyRateOverride ?? ProductionStage?.DefaultHourlyRate ?? 85.00m;
        }
        
        /// <summary>
        /// Calculate total estimated cost for this stage
        /// </summary>
        /// <returns>Setup cost + (hours * rate) + material costs</returns>
        public decimal CalculateTotalEstimatedCost()
        {
            if (EstimatedCost > 0) return EstimatedCost;
            
            var hours = (decimal)GetEffectiveEstimatedHours();
            var rate = GetEffectiveHourlyRate();
            var setupMinutes = SetupTimeMinutes ?? ProductionStage?.DefaultSetupMinutes ?? 30;
            var setupCost = (decimal)(setupMinutes / 60.0) * rate;
            
            return setupCost + (hours * rate) + MaterialCost;
        }

        /// <summary>
        /// Get custom field values as a dictionary
        /// </summary>
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

        /// <summary>
        /// Set custom field values from a dictionary
        /// </summary>
        public void SetCustomFieldValues(Dictionary<string, object> values)
        {
            try
            {
                CustomFieldValues = JsonSerializer.Serialize(values, new JsonSerializerOptions 
                { 
                    WriteIndented = false 
                });
            }
            catch (JsonException)
            {
                CustomFieldValues = "{}";
            }
        }

        /// <summary>
        /// Get a specific custom field value
        /// </summary>
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

        /// <summary>
        /// Set a specific custom field value
        /// </summary>
        public void SetCustomFieldValue(string fieldName, object value)
        {
            var values = GetCustomFieldValues();
            values[fieldName] = value;
            SetCustomFieldValues(values);
        }
        
        /// <summary>
        /// Get the list of preferred machine IDs
        /// </summary>
        public List<string> GetPreferredMachineIds()
        {
            if (string.IsNullOrWhiteSpace(PreferredMachineIds))
                return new List<string>();

            return PreferredMachineIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();
        }

        /// <summary>
        /// Set the preferred machine IDs from a list
        /// </summary>
        public void SetPreferredMachineIds(List<string> machineIds)
        {
            PreferredMachineIds = string.Join(",", machineIds.Where(id => !string.IsNullOrWhiteSpace(id)));
        }

        /// <summary>
        /// Check if a machine can execute this stage requirement
        /// </summary>
        public bool CanMachineExecute(string machineId)
        {
            if (string.IsNullOrWhiteSpace(machineId))
                return false;

            // If specific machine required, check assignment
            if (RequiresSpecificMachine)
                return AssignedMachineId == machineId;

            // Otherwise check if stage allows this machine
            return ProductionStage?.CanMachineExecuteStage(machineId) ?? true;
        }

        /// <summary>
        /// Get the best machine for this stage requirement
        /// </summary>
        public string? GetBestMachineId(List<string> availableMachineIds)
        {
            // First try assigned machine if required
            if (RequiresSpecificMachine && !string.IsNullOrWhiteSpace(AssignedMachineId))
            {
                if (availableMachineIds.Contains(AssignedMachineId))
                    return AssignedMachineId;
            }

            // Try preferred machines in order
            var preferredMachines = GetPreferredMachineIds();
            foreach (var machineId in preferredMachines)
            {
                if (availableMachineIds.Contains(machineId) && CanMachineExecute(machineId))
                    return machineId;
            }

            // Fall back to any compatible machine
            return availableMachineIds.FirstOrDefault(id => CanMachineExecute(id));
        }
        
        /// <summary>
        /// Validate stage parameters JSON
        /// </summary>
        /// <returns>True if valid JSON, false otherwise</returns>
        public bool ValidateStageParameters()
        {
            if (string.IsNullOrWhiteSpace(StageParameters)) return true;
            
            try
            {
                JsonSerializer.Deserialize<object>(StageParameters);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Get stage dependencies (stages that must complete before this one)
        /// </summary>
        /// <param name="allPartStages">All stages for this part</param>
        /// <returns>List of stages that must complete first</returns>
        public List<PartStageRequirement> GetDependencies(List<PartStageRequirement> allPartStages)
        {
            return allPartStages
                .Where(s => s.PartId == PartId && s.ExecutionOrder < ExecutionOrder && s.IsBlocking)
                .OrderBy(s => s.ExecutionOrder)
                .ToList();
        }

        #endregion
    }
}