using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Represents individual parts/components produced in a build job
    /// Supports multi-part plate builds with one primary part
    /// </summary>
    public class BuildJobPart
    {
        [Key] // FIXED: Add explicit Key attribute to resolve primary key issue
        public int PartEntryId { get; set; }

        [Required]
        public int BuildId { get; set; }
        public virtual BuildJob? BuildJob { get; set; }

        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// The first part added is marked as primary (displayed in schedule grid)
        /// </summary>
        [Required]
        public bool IsPrimary { get; set; } = false;

        #region Part Details (from Part library if exists)
        
        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Material { get; set; }

        // Estimated time contribution to total build
        public double EstimatedHours { get; set; } = 0;

        #endregion

        #region Audit Trail

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        #endregion

        #region Computed Properties

        [NotMapped]
        public string DisplayText => IsPrimary 
            ? $"{PartNumber} (Primary) x{Quantity}"
            : $"{PartNumber} x{Quantity}";

        [NotMapped]
        public bool IsFromPartLibrary => !string.IsNullOrEmpty(Description);

        #endregion

        #region Helper Methods

        /// <summary>
        /// Create from existing Part in library
        /// </summary>
        public static BuildJobPart FromPart(Part part, int quantity, bool isPrimary = false)
        {
            return new BuildJobPart
            {
                PartNumber = part.PartNumber,
                Quantity = quantity,
                IsPrimary = isPrimary,
                Description = part.Description,
                Material = part.Material,
                EstimatedHours = part.EstimatedHours,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create new part not in library
        /// </summary>
        public static BuildJobPart CreateNew(string partNumber, int quantity, bool isPrimary = false)
        {
            return new BuildJobPart
            {
                PartNumber = partNumber,
                Quantity = quantity,
                IsPrimary = isPrimary,
                CreatedAt = DateTime.UtcNow
            };
        }

        #endregion
    }
}