using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Resource pool for managing machines, operators, and tools
    /// Used in advanced stage management for automatic resource allocation
    /// </summary>
    public class ResourcePool
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string ResourceType { get; set; } = "Machine"; // Machine, Operator, Tool, Equipment

        /// <summary>
        /// JSON configuration of resources in this pool
        /// Format: [{"resourceId": "TI1", "type": "Machine", "capacity": 1, "skills": ["SLS"], "shifts": ["Day", "Night"]}]
        /// </summary>
        [Required]
        [Column(TypeName = "TEXT")]
        public string ResourceConfiguration { get; set; } = "[]";

        public int MaxConcurrentAllocations { get; set; } = 1;

        public bool AutoAssign { get; set; } = false;

        /// <summary>
        /// JSON criteria for automatic assignment
        /// Format: {"priority": "FIFO", "skills": ["required"], "availability": "immediate"}
        /// </summary>
        [StringLength(2000)]
        public string? AssignmentCriteria { get; set; }

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
        /// Parse the resource configuration JSON into a list of resource definitions
        /// </summary>
        public List<ResourceDefinition> GetResourceDefinitions()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ResourceConfiguration))
                    return new List<ResourceDefinition>();

                var definitions = System.Text.Json.JsonSerializer.Deserialize<List<ResourceDefinition>>(ResourceConfiguration);
                return definitions ?? new List<ResourceDefinition>();
            }
            catch (System.Text.Json.JsonException)
            {
                return new List<ResourceDefinition>();
            }
        }

        /// <summary>
        /// Set the resource configuration from a list of resource definitions
        /// </summary>
        public void SetResourceDefinitions(List<ResourceDefinition> definitions)
        {
            try
            {
                ResourceConfiguration = System.Text.Json.JsonSerializer.Serialize(definitions, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = false 
                });
            }
            catch (System.Text.Json.JsonException)
            {
                ResourceConfiguration = "[]";
            }
        }

        /// <summary>
        /// Get the assignment criteria as a structured object
        /// </summary>
        public AssignmentCriteria? GetAssignmentCriteria()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AssignmentCriteria))
                    return null;

                return System.Text.Json.JsonSerializer.Deserialize<AssignmentCriteria>(AssignmentCriteria);
            }
            catch (System.Text.Json.JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Set the assignment criteria from a structured object
        /// </summary>
        public void SetAssignmentCriteria(AssignmentCriteria criteria)
        {
            try
            {
                AssignmentCriteria = System.Text.Json.JsonSerializer.Serialize(criteria, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = false 
                });
            }
            catch (System.Text.Json.JsonException)
            {
                AssignmentCriteria = null;
            }
        }

        /// <summary>
        /// Get the current utilization percentage of this resource pool
        /// </summary>
        public decimal GetUtilizationPercentage()
        {
            // This would be calculated based on current assignments
            // For now, return 0 as placeholder
            return 0m;
        }

        #endregion
    }

    /// <summary>
    /// Represents a resource definition within a resource pool
    /// </summary>
    public class ResourceDefinition
    {
        public string ResourceId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Capacity { get; set; } = 1;
        public List<string> Skills { get; set; } = new List<string>();
        public List<string> Shifts { get; set; } = new List<string>();
        public bool IsAvailable { get; set; } = true;
        public string? Notes { get; set; }
        public decimal? CostPerHour { get; set; }
        public int Priority { get; set; } = 3;
    }

    /// <summary>
    /// Represents assignment criteria for automatic resource allocation
    /// </summary>
    public class AssignmentCriteria
    {
        public string Priority { get; set; } = "FIFO"; // FIFO, Priority, Cost, Availability
        public List<string> RequiredSkills { get; set; } = new List<string>();
        public string Availability { get; set; } = "immediate"; // immediate, scheduled, flexible
        public decimal? MaxCostPerHour { get; set; }
        public List<string> PreferredShifts { get; set; } = new List<string>();
        public bool AllowOverallocation { get; set; } = false;
        public int MaxQueueLength { get; set; } = 10;
    }
}