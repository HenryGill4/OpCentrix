using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Defines a production stage that parts can go through (3D Printing, CNC, EDM, etc.)
    /// Configured by admins to set up the manufacturing workflow
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

        // Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<ProductionStageExecution> StageExecutions { get; set; } = new List<ProductionStageExecution>();
    }
}