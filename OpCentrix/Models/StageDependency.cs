using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Stage dependency for workflow ordering and relationships
    /// Defines which stages must be completed before others can begin
    /// </summary>
    public class StageDependency
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DependentStageId { get; set; }

        [Required]
        public int PrerequisiteStageId { get; set; }

        [Required]
        [StringLength(50)]
        public string DependencyType { get; set; } = "FinishToStart"; // FinishToStart, StartToStart, FinishToFinish, StartToFinish

        public int DelayHours { get; set; } = 0;

        public bool IsOptional { get; set; } = false;

        /// <summary>
        /// JSON condition for conditional dependencies
        /// Format: {"field": "quality_result", "operator": "equals", "value": "Pass"}
        /// </summary>
        [StringLength(1000)]
        public string? Condition { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

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

        // Navigation properties
        public virtual ProductionStage DependentStage { get; set; } = null!;
        public virtual ProductionStage PrerequisiteStage { get; set; } = null!;

        #region Helper Methods

        /// <summary>
        /// Get the dependency condition as a structured object
        /// </summary>
        public DependencyCondition? GetCondition()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Condition))
                    return null;

                return System.Text.Json.JsonSerializer.Deserialize<DependencyCondition>(Condition);
            }
            catch (System.Text.Json.JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Set the dependency condition from a structured object
        /// </summary>
        public void SetCondition(DependencyCondition condition)
        {
            try
            {
                Condition = System.Text.Json.JsonSerializer.Serialize(condition, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = false 
                });
            }
            catch (System.Text.Json.JsonException)
            {
                Condition = null;
            }
        }

        /// <summary>
        /// Check if this dependency type allows stages to run in parallel
        /// </summary>
        public bool AllowsParallelExecution()
        {
            return DependencyType switch
            {
                "StartToStart" => true,
                "FinishToFinish" => true,
                "FinishToStart" => false,
                "StartToFinish" => false,
                _ => false
            };
        }

        /// <summary>
        /// Get a human-readable description of this dependency
        /// </summary>
        public string GetDescription()
        {
            var delayText = DelayHours > 0 ? $" with {DelayHours}h delay" : "";
            var optionalText = IsOptional ? " (optional)" : "";
            
            return DependencyType switch
            {
                "FinishToStart" => $"{PrerequisiteStage?.Name} must finish before {DependentStage?.Name} starts{delayText}{optionalText}",
                "StartToStart" => $"{DependentStage?.Name} can start when {PrerequisiteStage?.Name} starts{delayText}{optionalText}",
                "FinishToFinish" => $"{DependentStage?.Name} must finish when {PrerequisiteStage?.Name} finishes{delayText}{optionalText}",
                "StartToFinish" => $"{PrerequisiteStage?.Name} must start before {DependentStage?.Name} finishes{delayText}{optionalText}",
                _ => $"Unknown dependency type{optionalText}"
            };
        }

        /// <summary>
        /// Calculate the minimum start time for the dependent stage based on this dependency
        /// </summary>
        public DateTime CalculateMinimumStartTime(DateTime prerequisiteStart, DateTime prerequisiteEnd)
        {
            var baseTime = DependencyType switch
            {
                "FinishToStart" => prerequisiteEnd,
                "StartToStart" => prerequisiteStart,
                "FinishToFinish" => prerequisiteEnd, // This would need duration calculation
                "StartToFinish" => prerequisiteStart,
                _ => prerequisiteEnd
            };

            return baseTime.AddHours(DelayHours);
        }

        #endregion
    }

    /// <summary>
    /// Represents a conditional dependency requirement
    /// </summary>
    public class DependencyCondition
    {
        public string Field { get; set; } = string.Empty;
        public string Operator { get; set; } = "equals"; // equals, not_equals, greater_than, less_than, contains
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    /// <summary>
    /// Dependency types enum for validation
    /// </summary>
    public enum DependencyType
    {
        FinishToStart,  // Prerequisite must finish before dependent can start
        StartToStart,   // Dependent can start when prerequisite starts
        FinishToFinish, // Dependent must finish when prerequisite finishes
        StartToFinish   // Prerequisite must start before dependent can finish
    }
}