using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models
{
    /// <summary>
    /// Part Asset Links for 3D models, photos, drawings, and documents
    /// </summary>
    public class PartAssetLink
    {
        public int Id { get; set; }

        [Required]
        public int PartId { get; set; }

        [Required]
        [StringLength(500)]
        public string Url { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Source { get; set; } = "Upload"; // 'Upload', 'External', 'Generated'

        [Required]
        [StringLength(50)]
        public string AssetType { get; set; } = "3DModel"; // '3DModel', 'Photo', 'Drawing', 'Document'

        public string? LastCheckedUtc { get; set; }

        public bool IsActive { get; set; } = true;

        public string CreatedDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";

        // Navigation properties
        public virtual Part Part { get; set; } = null!;

        /// <summary>
        /// Get file extension from URL
        /// </summary>
        public string FileExtension
        {
            get
            {
                try
                {
                    return Path.GetExtension(Url)?.ToLowerInvariant() ?? "";
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Get icon for asset type
        /// </summary>
        public string AssetTypeIcon => AssetType switch
        {
            "3DModel" => "fas fa-cube",
            "Photo" => "fas fa-image",
            "Drawing" => "fas fa-drafting-compass",
            "Document" => "fas fa-file-alt",
            _ => "fas fa-file"
        };

        /// <summary>
        /// Get CSS class for asset type
        /// </summary>
        public string AssetTypeCssClass => AssetType switch
        {
            "3DModel" => "badge bg-primary",
            "Photo" => "badge bg-success",
            "Drawing" => "badge bg-info",
            "Document" => "badge bg-warning",
            _ => "badge bg-secondary"
        };

        /// <summary>
        /// Check if the asset is an image file
        /// </summary>
        public bool IsImageFile
        {
            get
            {
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                return imageExtensions.Contains(FileExtension);
            }
        }

        /// <summary>
        /// Check if the asset is a 3D model file
        /// </summary>
        public bool Is3DModelFile
        {
            get
            {
                var modelExtensions = new[] { ".step", ".stp", ".stl", ".obj", ".ply", ".3mf" };
                return modelExtensions.Contains(FileExtension);
            }
        }

        /// <summary>
        /// Check if the asset is a CAD drawing file
        /// </summary>
        public bool IsDrawingFile
        {
            get
            {
                var drawingExtensions = new[] { ".dwg", ".dxf", ".pdf" };
                return drawingExtensions.Contains(FileExtension);
            }
        }

        /// <summary>
        /// Get display size text for UI
        /// </summary>
        public string DisplaySize
        {
            get
            {
                // This would be implemented with actual file size if available
                return "Size unknown";
            }
        }

        /// <summary>
        /// Check if the asset link is still valid
        /// </summary>
        public bool IsLinkValid
        {
            get
            {
                // Basic validation - can be enhanced with actual URL checking
                return !string.IsNullOrWhiteSpace(Url) && 
                       (Url.StartsWith("http://") || Url.StartsWith("https://") || Url.StartsWith("/") || Path.IsPathRooted(Url));
            }
        }
    }
}