using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Represents a build package - a collection of parts nested on a build plate
    /// ready to be scheduled and printed on an SLS machine.
    /// </summary>
    public class BuildPackage
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Unique build package reference number (e.g., "BP-2024-001")
        /// </summary>
        [Required]
        [StringLength(50)]
        public string PackageNumber { get; set; } = string.Empty;

        /// <summary>
        /// Descriptive name for the build package
        /// </summary>
        [StringLength(200)]
        public string? Name { get; set; }

        /// <summary>
        /// Current status: Draft, Ready, Scheduled, InProgress, Completed, Cancelled
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Target machine for this build
        /// </summary>
        [StringLength(50)]
        public string? TargetMachineId { get; set; }

        /// <summary>
        /// Material for this build (all parts must use same material)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Material { get; set; } = "Ti-6Al-4V Grade 5";

        #region Build File Information

        /// <summary>
        /// Name of the .slm build file
        /// </summary>
        [StringLength(255)]
        public string? BuildFileName { get; set; }

        /// <summary>
        /// Path to the build file on network/local storage
        /// </summary>
        [StringLength(500)]
        public string? BuildFilePath { get; set; }

        /// <summary>
        /// Hash of build file for tracking unique builds
        /// </summary>
        [StringLength(64)]
        public string? BuildFileHash { get; set; }

        /// <summary>
        /// Size of build file in bytes
        /// </summary>
        public long? BuildFileSizeBytes { get; set; }

        /// <summary>
        /// When the build file was uploaded/assigned
        /// </summary>
        public DateTime? BuildFileUploadedAt { get; set; }

        #endregion

        #region Build Parameters

        /// <summary>
        /// Total estimated build time in hours
        /// </summary>
        public double EstimatedBuildHours { get; set; }

        /// <summary>
        /// Estimated powder usage in kg
        /// </summary>
        public double EstimatedPowderKg { get; set; }

        /// <summary>
        /// Build height in mm (tallest part + supports)
        /// </summary>
        public double? BuildHeightMm { get; set; }

        /// <summary>
        /// Total layer count
        /// </summary>
        public int? LayerCount { get; set; }

        /// <summary>
        /// Layer thickness in microns
        /// </summary>
        public double LayerThicknessMicrons { get; set; } = 30;

        /// <summary>
        /// Support complexity: None, Low, Medium, High
        /// </summary>
        [StringLength(20)]
        public string SupportComplexity { get; set; } = "Medium";

        #endregion

        #region Scheduling

        /// <summary>
        /// Preferred start date for scheduling
        /// </summary>
        public DateTime? PreferredStartDate { get; set; }

        /// <summary>
        /// Customer due date
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Priority: 1 (Critical) to 5 (Lowest)
        /// </summary>
        [Range(1, 5)]
        public int Priority { get; set; } = 3;

        /// <summary>
        /// Rush job flag
        /// </summary>
        public bool IsRushJob { get; set; }

        /// <summary>
        /// Associated Job ID once scheduled
        /// </summary>
        public int? ScheduledJobId { get; set; }

        #endregion

        #region Audit Trail

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? LastModifiedAt { get; set; }

        [StringLength(100)]
        public string? LastModifiedBy { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Parts included in this build package
        /// </summary>
        public virtual ICollection<BuildPackagePart> Parts { get; set; } = new List<BuildPackagePart>();

        /// <summary>
        /// Associated scheduled job
        /// </summary>
        public virtual Job? ScheduledJob { get; set; }

        #endregion

        #region Computed Properties

        [NotMapped]
        public int TotalPartCount => Parts?.Sum(p => p.Quantity) ?? 0;

        [NotMapped]
        public int UniquePartCount => Parts?.Count ?? 0;

        [NotMapped]
        public bool IsReadyToSchedule => 
            Status == "Ready" && 
            !string.IsNullOrEmpty(BuildFileName) && 
            Parts.Any();

        [NotMapped]
        public string StatusColor => Status switch
        {
            "Draft" => "#6B7280",
            "Ready" => "#3B82F6",
            "Scheduled" => "#8B5CF6",
            "InProgress" => "#F59E0B",
            "Completed" => "#10B981",
            "Cancelled" => "#EF4444",
            _ => "#6B7280"
        };

        [NotMapped]
        public string PriorityDisplay => Priority switch
        {
            1 => "Critical",
            2 => "High",
            3 => "Normal",
            4 => "Low",
            5 => "Lowest",
            _ => "Normal"
        };

        #endregion
    }

    /// <summary>
    /// Represents a part included in a build package with quantity and positioning info
    /// </summary>
    public class BuildPackagePart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BuildPackageId { get; set; }

        [Required]
        public int PartId { get; set; }

        /// <summary>
        /// Quantity of this part in the build
        /// </summary>
        [Required]
        [Range(1, 500)]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Part orientation: Flat, Vertical, Angled45, Custom
        /// </summary>
        [StringLength(50)]
        public string Orientation { get; set; } = "Flat";

        /// <summary>
        /// Support structure type for this part
        /// </summary>
        [StringLength(50)]
        public string? SupportType { get; set; }

        /// <summary>
        /// Estimated build time contribution in hours
        /// </summary>
        public double EstimatedHours { get; set; }

        /// <summary>
        /// Position index on build plate (for visualization)
        /// </summary>
        public int? PositionIndex { get; set; }

        /// <summary>
        /// Additional notes for this part in the build
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        #region Navigation Properties

        public virtual BuildPackage BuildPackage { get; set; } = null!;
        public virtual Part Part { get; set; } = null!;

        #endregion

        #region Computed Properties

        [NotMapped]
        public string PartNumber => Part?.PartNumber ?? "Unknown";

        [NotMapped]
        public string PartDescription => Part?.Description ?? string.Empty;

        #endregion
    }
}
