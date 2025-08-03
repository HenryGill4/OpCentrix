using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Workflow template for reusable stage configurations
    /// Used in advanced stage management for creating standardized manufacturing workflows
    /// </summary>
    public class WorkflowTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [Required]
        [StringLength(50)]
        public string Complexity { get; set; } = "Medium"; // Simple, Medium, Complex, Very Complex

        public double EstimatedDurationHours { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal EstimatedCost { get; set; }

        /// <summary>
        /// JSON configuration of stages and their parameters
        /// Format: [{"stageId": 1, "order": 1, "duration": 8.0, "cost": 680.00, "parameters": {...}}]
        /// </summary>
        [Required]
        [Column(TypeName = "TEXT")]
        public string StageConfiguration { get; set; } = "[]";

        // Audit fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";

        [Required]
        [StringLength(100)]
        public string LastModifiedBy { get; set; } = "System";

        public bool IsActive { get; set; } = true;

        #region Helper Methods

        /// <summary>
        /// Parse the stage configuration JSON into a list of stage definitions
        /// </summary>
        public List<WorkflowStageDefinition> GetStageDefinitions()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(StageConfiguration))
                    return new List<WorkflowStageDefinition>();

                var definitions = System.Text.Json.JsonSerializer.Deserialize<List<WorkflowStageDefinition>>(StageConfiguration);
                return definitions ?? new List<WorkflowStageDefinition>();
            }
            catch (System.Text.Json.JsonException)
            {
                return new List<WorkflowStageDefinition>();
            }
        }

        /// <summary>
        /// Set the stage configuration from a list of stage definitions
        /// </summary>
        public void SetStageDefinitions(List<WorkflowStageDefinition> definitions)
        {
            try
            {
                StageConfiguration = System.Text.Json.JsonSerializer.Serialize(definitions, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = false 
                });
            }
            catch (System.Text.Json.JsonException)
            {
                StageConfiguration = "[]";
            }
        }

        /// <summary>
        /// Get the complexity level as an integer for sorting
        /// </summary>
        public int GetComplexityScore()
        {
            return Complexity switch
            {
                "Simple" => 1,
                "Medium" => 2,
                "Complex" => 3,
                "Very Complex" => 4,
                _ => 2
            };
        }

        #endregion
    }

    /// <summary>
    /// Represents a stage definition within a workflow template
    /// </summary>
    public class WorkflowStageDefinition
    {
        public int StageId { get; set; }
        public int Order { get; set; }
        public double Duration { get; set; }
        public decimal Cost { get; set; }
        public bool IsRequired { get; set; } = true;
        public bool IsOptional { get; set; } = false;
        public string? MachineAssignment { get; set; }
        public string? Parameters { get; set; } // JSON string for stage-specific parameters
        public string? Notes { get; set; }
    }
}