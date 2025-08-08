using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models
{
    /// <summary>
    /// Stage Template for pre-configured manufacturing workflows
    /// Phase 6: Advanced Stage Management
    /// </summary>
    public class StageTemplate
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Industry { get; set; } = "General";
        
        [MaxLength(50)]
        public string MaterialType { get; set; } = "Metal";
        
        [MaxLength(50)]
        public string ComplexityLevel { get; set; } = "Medium"; // Simple, Medium, Complex, VeryComplex
        
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public int SortOrder { get; set; } = 100;
        
        // Template configuration
        public string TemplateConfiguration { get; set; } = "{}"; // JSON configuration
        public decimal EstimatedTotalHours { get; set; }
        public decimal EstimatedTotalCost { get; set; }
        
        // Usage statistics
        public int UsageCount { get; set; } = 0;
        public DateTime? LastUsedDate { get; set; }
        
        // Audit fields
        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [MaxLength(100)]
        public string LastModifiedBy { get; set; } = "System";
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<StageTemplateStep> TemplateSteps { get; set; } = new List<StageTemplateStep>();
        public virtual ICollection<PartStageRequirement> PartStageRequirements { get; set; } = new List<PartStageRequirement>();
    }
    
    /// <summary>
    /// Individual step within a stage template
    /// Defines the specific stages and their parameters
    /// </summary>
    public class StageTemplateStep
    {
        public int Id { get; set; }
        
        public int StageTemplateId { get; set; }
        public int ProductionStageId { get; set; }
        
        public int ExecutionOrder { get; set; }
        public double EstimatedHours { get; set; } = 1.0;
        public decimal HourlyRate { get; set; } = 85.00m;
        public decimal MaterialCost { get; set; } = 0.00m;
        public int SetupTimeMinutes { get; set; } = 30;
        public int TeardownTimeMinutes { get; set; } = 0;
        
        public bool IsRequired { get; set; } = true;
        public bool IsParallel { get; set; } = false; // Can run in parallel with other stages
        
        // Stage-specific configuration
        public string StageConfiguration { get; set; } = "{}"; // JSON for stage-specific parameters
        public string QualityRequirements { get; set; } = string.Empty;
        public string SpecialInstructions { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual StageTemplate StageTemplate { get; set; } = null!;
        public virtual ProductionStage ProductionStage { get; set; } = null!;
    }
    
    /// <summary>
    /// Pre-configured stage combinations for different part types
    /// Phase 6: Smart template suggestions based on part characteristics
    /// </summary>
    public class StageTemplateCategory
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Icon { get; set; } = "fas fa-cogs";
        
        [MaxLength(7)]
        public string ColorCode { get; set; } = "#007bff";
        
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 100;
        
        // Navigation properties
        public virtual ICollection<StageTemplate> StageTemplates { get; set; } = new List<StageTemplate>();
    }
}