using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OpCentrix.Models
{
    /// <summary>
    /// Defines a production stage that parts can go through (3D Printing, CNC, EDM, etc.)
    /// Configured by admins to set up the manufacturing workflow with custom fields and machine assignments
    /// </summary>
    public class ProductionStage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // "3D Printing", "CNC Machining", etc.

        [Required]
        public int DisplayOrder { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        // Default Parameters
        public int DefaultSetupMinutes { get; set; } = 30;

        [Column(TypeName = "decimal(8,2)")]
        public decimal DefaultHourlyRate { get; set; } = 85.00m;

        public bool RequiresQualityCheck { get; set; } = true;
        public bool RequiresApproval { get; set; } = false;

        // Stage Configuration
        public bool AllowSkip { get; set; } = false;
        public bool IsOptional { get; set; } = false;

        [StringLength(50)]
        public string? RequiredRole { get; set; } // Which roles can execute this stage

        // NEW: Custom Fields Configuration
        /// <summary>
        /// JSON configuration for custom fields specific to this stage
        /// Format: [{"name": "fieldName", "type": "text|number|dropdown|checkbox", "label": "Display Label", "required": true, "options": ["opt1", "opt2"]}]
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string CustomFieldsConfig { get; set; } = "[]";

        // NEW: Machine Assignment Support
        /// <summary>
        /// Comma-separated list of machine IDs that can execute this stage
        /// If empty, any machine can be used
        /// </summary>
        [StringLength(500)]
        public string? AssignedMachineIds { get; set; }

        /// <summary>
        /// Indicates if this stage requires a specific machine assignment
        /// </summary>
        public bool RequiresMachineAssignment { get; set; } = false;

        /// <summary>
        /// Default machine ID to use for this stage if available
        /// </summary>
        [StringLength(50)]
        public string? DefaultMachineId { get; set; }

        // NEW: Enhanced Stage Properties
        /// <summary>
        /// Stage color for UI display (hex color code)
        /// </summary>
        [StringLength(7)]
        public string StageColor { get; set; } = "#007bff";

        /// <summary>
        /// Font Awesome icon class for this stage
        /// </summary>
        [StringLength(50)]
        public string StageIcon { get; set; } = "fas fa-cogs";

        /// <summary>
        /// Department that typically handles this stage
        /// </summary>
        [StringLength(100)]
        public string? Department { get; set; }

        /// <summary>
        /// Whether this stage can run in parallel with other stages
        /// </summary>
        public bool AllowParallelExecution { get; set; } = false;

        /// <summary>
        /// Estimated material cost for this stage (per part)
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultMaterialCost { get; set; } = 0.00m;

        /// <summary>
        /// Default estimated duration in hours for this stage
        /// </summary>
        public double DefaultDurationHours { get; set; } = 1.0;

        // Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";
        
        [StringLength(100)]
        public string LastModifiedBy { get; set; } = "System";
        
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<ProductionStageExecution> StageExecutions { get; set; } = new List<ProductionStageExecution>();
        public virtual ICollection<PartStageRequirement> PartStageRequirements { get; set; } = new List<PartStageRequirement>();

        #region Helper Methods

        /// <summary>
        /// Get the custom fields configuration as a list of field definitions
        /// </summary>
        public List<CustomFieldDefinition> GetCustomFields()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CustomFieldsConfig))
                    return new List<CustomFieldDefinition>();

                var fields = JsonSerializer.Deserialize<List<CustomFieldDefinition>>(CustomFieldsConfig);
                return fields ?? new List<CustomFieldDefinition>();
            }
            catch (JsonException)
            {
                return new List<CustomFieldDefinition>();
            }
        }

        /// <summary>
        /// Set the custom fields configuration from a list of field definitions
        /// </summary>
        public void SetCustomFields(List<CustomFieldDefinition> fields)
        {
            try
            {
                CustomFieldsConfig = JsonSerializer.Serialize(fields, new JsonSerializerOptions 
                { 
                    WriteIndented = false 
                });
            }
            catch (JsonException)
            {
                CustomFieldsConfig = "[]";
            }
        }

        /// <summary>
        /// Get the list of assigned machine IDs
        /// </summary>
        public List<string> GetAssignedMachineIds()
        {
            if (string.IsNullOrWhiteSpace(AssignedMachineIds))
                return new List<string>();

            return AssignedMachineIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();
        }

        /// <summary>
        /// Set the assigned machine IDs from a list
        /// </summary>
        public void SetAssignedMachineIds(List<string> machineIds)
        {
            AssignedMachineIds = string.Join(",", machineIds.Where(id => !string.IsNullOrWhiteSpace(id)));
        }

        /// <summary>
        /// Check if a specific machine can execute this stage
        /// </summary>
        public bool CanMachineExecuteStage(string machineId)
        {
            if (string.IsNullOrWhiteSpace(machineId))
                return false;

            if (!RequiresMachineAssignment)
                return true; // Any machine can execute if no specific assignment required

            var assignedMachines = GetAssignedMachineIds();
            return assignedMachines.Contains(machineId);
        }

        /// <summary>
        /// Get the total estimated cost for this stage
        /// </summary>
        public decimal GetTotalEstimatedCost()
        {
            var laborCost = (decimal)DefaultDurationHours * DefaultHourlyRate;
            var setupCost = (decimal)(DefaultSetupMinutes / 60.0) * DefaultHourlyRate;
            return laborCost + setupCost + DefaultMaterialCost;
        }

        #endregion
    }

    /// <summary>
    /// Represents a custom field definition for a production stage
    /// </summary>
    public class CustomFieldDefinition
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "text"; // text, number, dropdown, checkbox, textarea, date
        public string Label { get; set; } = "";
        public string? Description { get; set; }
        public bool Required { get; set; } = false;
        public string? DefaultValue { get; set; }
        public List<string>? Options { get; set; } // For dropdown fields
        public double? MinValue { get; set; } // For number fields
        public double? MaxValue { get; set; } // For number fields
        public string? ValidationPattern { get; set; } // For text fields
        public string? Unit { get; set; } // For number/measurement fields
        public int DisplayOrder { get; set; } = 0;
        public string? PlaceholderText { get; set; }
        public bool IsReadOnly { get; set; } = false;
    }
}