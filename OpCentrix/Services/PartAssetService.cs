using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services
{
    /// <summary>
    /// Service for managing part assets (3D models, photos, documents)
    /// Phase 5: Asset Management Implementation
    /// </summary>
    public interface IPartAssetService
    {
        Task<List<PartAssetLink>> GetPartAssetsAsync(int partId);
        Task<List<PartAssetLink>> GetPartAssetsByTypeAsync(int partId, string assetType);
        Task<PartAssetLink?> GetAssetByIdAsync(int assetId);
        Task<PartAssetLink> AddAssetAsync(PartAssetLink asset);
        Task<PartAssetLink> UpdateAssetAsync(PartAssetLink asset);
        Task<bool> DeleteAssetAsync(int assetId);
        Task<bool> ValidateAssetAsync(IFormFile file, string assetType);
        Task<string> UploadAssetAsync(IFormFile file, int partId, string assetType, string displayName);
        Task<List<string>> GetSupportedFileTypesAsync(string assetType);
        Task<long> GetMaxFileSizeAsync(string assetType);
        Task<Dictionary<string, int>> GetAssetUsageStatisticsAsync();
        Task<bool> CheckAssetAvailabilityAsync(string url);
    }

    /// <summary>
    /// Implementation of Part Asset Management Service
    /// Handles file uploads, 3D models, photos, and technical documents
    /// </summary>
    public class PartAssetService : IPartAssetService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PartAssetService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadBasePath;

        // Supported file types by asset type
        private readonly Dictionary<string, string[]> _supportedFileTypes = new()
        {
            ["3DModel"] = new[] { ".step", ".stp", ".stl", ".obj", ".3mf", ".ply" },
            ["Photo"] = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".webp" },
            ["Drawing"] = new[] { ".pdf", ".dwg", ".dxf", ".svg" },
            ["Document"] = new[] { ".pdf", ".docx", ".xlsx", ".txt", ".md" }
        };

        // Maximum file sizes (in bytes)
        private readonly Dictionary<string, long> _maxFileSizes = new()
        {
            ["3DModel"] = 100 * 1024 * 1024, // 100MB for 3D models
            ["Photo"] = 10 * 1024 * 1024,    // 10MB for photos
            ["Drawing"] = 25 * 1024 * 1024,  // 25MB for drawings
            ["Document"] = 5 * 1024 * 1024   // 5MB for documents
        };

        public PartAssetService(SchedulerContext context, ILogger<PartAssetService> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
            _uploadBasePath = Path.Combine(_environment.WebRootPath, "uploads", "parts");
            
            // Ensure upload directory exists
            Directory.CreateDirectory(_uploadBasePath);
        }

        public async Task<List<PartAssetLink>> GetPartAssetsAsync(int partId)
        {
            try
            {
                return await _context.PartAssetLinks
                    .Where(pal => pal.PartId == partId && pal.IsActive)
                    .OrderBy(pal => pal.AssetType)
                    .ThenBy(pal => pal.DisplayName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assets for part {PartId}", partId);
                return new List<PartAssetLink>();
            }
        }

        public async Task<List<PartAssetLink>> GetPartAssetsByTypeAsync(int partId, string assetType)
        {
            try
            {
                return await _context.PartAssetLinks
                    .Where(pal => pal.PartId == partId && pal.AssetType == assetType && pal.IsActive)
                    .OrderBy(pal => pal.DisplayName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving {AssetType} assets for part {PartId}", assetType, partId);
                return new List<PartAssetLink>();
            }
        }

        public async Task<PartAssetLink?> GetAssetByIdAsync(int assetId)
        {
            try
            {
                return await _context.PartAssetLinks
                    .Include(pal => pal.Part)
                    .FirstOrDefaultAsync(pal => pal.Id == assetId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving asset {AssetId}", assetId);
                return null;
            }
        }

        public async Task<PartAssetLink> AddAssetAsync(PartAssetLink asset)
        {
            try
            {
                asset.CreatedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                asset.IsActive = true;

                _context.PartAssetLinks.Add(asset);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Asset added successfully: {DisplayName} for part {PartId}", 
                    asset.DisplayName, asset.PartId);

                return asset;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding asset {DisplayName} for part {PartId}", 
                    asset.DisplayName, asset.PartId);
                throw;
            }
        }

        public async Task<PartAssetLink> UpdateAssetAsync(PartAssetLink asset)
        {
            try
            {
                var existingAsset = await _context.PartAssetLinks.FindAsync(asset.Id);
                if (existingAsset == null)
                {
                    throw new InvalidOperationException($"Asset {asset.Id} not found");
                }

                existingAsset.DisplayName = asset.DisplayName;
                existingAsset.Source = asset.Source;
                existingAsset.AssetType = asset.AssetType;
                existingAsset.LastCheckedUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                await _context.SaveChangesAsync();

                _logger.LogInformation("Asset updated successfully: {AssetId}", asset.Id);
                return existingAsset;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating asset {AssetId}", asset.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAssetAsync(int assetId)
        {
            try
            {
                var asset = await _context.PartAssetLinks.FindAsync(assetId);
                if (asset == null)
                {
                    return false;
                }

                // Soft delete - mark as inactive
                asset.IsActive = false;
                await _context.SaveChangesAsync();

                // Optionally delete physical file if it's an uploaded file
                if (asset.Source == "Upload" && !string.IsNullOrEmpty(asset.Url))
                {
                    await DeletePhysicalFileAsync(asset.Url);
                }

                _logger.LogInformation("Asset deleted successfully: {AssetId}", assetId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting asset {AssetId}", assetId);
                return false;
            }
        }

        public async Task<bool> ValidateAssetAsync(IFormFile file, string assetType)
        {
            try
            {
                // Check if asset type is supported
                if (!_supportedFileTypes.ContainsKey(assetType))
                {
                    _logger.LogWarning("Unsupported asset type: {AssetType}", assetType);
                    return false;
                }

                // Check file extension
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_supportedFileTypes[assetType].Contains(fileExtension))
                {
                    _logger.LogWarning("Unsupported file extension {Extension} for asset type {AssetType}", 
                        fileExtension, assetType);
                    return false;
                }

                // Check file size
                if (file.Length > _maxFileSizes[assetType])
                {
                    _logger.LogWarning("File size {Size} exceeds maximum {MaxSize} for asset type {AssetType}", 
                        file.Length, _maxFileSizes[assetType], assetType);
                    return false;
                }

                // Additional validation for specific file types
                return await ValidateFileContentAsync(file, assetType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating asset file {FileName}", file.FileName);
                return false;
            }
        }

        public async Task<string> UploadAssetAsync(IFormFile file, int partId, string assetType, string displayName)
        {
            try
            {
                if (!await ValidateAssetAsync(file, assetType))
                {
                    throw new InvalidOperationException("File validation failed");
                }

                // Create directory structure: uploads/parts/{partId}/{assetType}/
                var partDirectory = Path.Combine(_uploadBasePath, partId.ToString(), assetType);
                Directory.CreateDirectory(partDirectory);

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(partDirectory, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative URL for storage in database
                var relativeUrl = $"/uploads/parts/{partId}/{assetType}/{fileName}";

                _logger.LogInformation("Asset uploaded successfully: {FileName} -> {RelativeUrl}", 
                    file.FileName, relativeUrl);

                return relativeUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading asset {FileName} for part {PartId}", 
                    file.FileName, partId);
                throw;
            }
        }

        public async Task<List<string>> GetSupportedFileTypesAsync(string assetType)
        {
            await Task.CompletedTask; // Async for future expansion
            
            if (_supportedFileTypes.ContainsKey(assetType))
            {
                return _supportedFileTypes[assetType].ToList();
            }
            
            return new List<string>();
        }

        public async Task<long> GetMaxFileSizeAsync(string assetType)
        {
            await Task.CompletedTask; // Async for future expansion
            
            if (_maxFileSizes.ContainsKey(assetType))
            {
                return _maxFileSizes[assetType];
            }
            
            return 5 * 1024 * 1024; // Default 5MB
        }

        public async Task<Dictionary<string, int>> GetAssetUsageStatisticsAsync()
        {
            try
            {
                return await _context.PartAssetLinks
                    .Where(pal => pal.IsActive)
                    .GroupBy(pal => pal.AssetType)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving asset usage statistics");
                return new Dictionary<string, int>();
            }
        }

        public async Task<bool> CheckAssetAvailabilityAsync(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return false;

                // For uploaded files, check if physical file exists
                if (url.StartsWith("/uploads/"))
                {
                    var physicalPath = Path.Combine(_environment.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    return File.Exists(physicalPath);
                }

                // For external URLs, you could implement HTTP HEAD request checking
                // For now, assume external URLs are available
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking asset availability for URL: {Url}", url);
                return false;
            }
        }

        private async Task<bool> ValidateFileContentAsync(IFormFile file, string assetType)
        {
            try
            {
                // Basic content validation - can be expanded for specific file types
                using var stream = file.OpenReadStream();
                var buffer = new byte[1024];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                // Very basic file signature validation
                switch (assetType)
                {
                    case "Photo":
                        return ValidateImageFile(buffer, bytesRead);
                    case "Document":
                        return ValidateDocumentFile(buffer, bytesRead, file.FileName);
                    default:
                        return true; // Allow other types for now
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file content for {FileName}", file.FileName);
                return false;
            }
        }

        private bool ValidateImageFile(byte[] buffer, int bytesRead)
        {
            if (bytesRead < 8) return false;

            // Check for common image file signatures
            // JPEG: FF D8 FF
            if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF) return true;
            
            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47) return true;
            
            // BMP: 42 4D
            if (buffer[0] == 0x42 && buffer[1] == 0x4D) return true;

            return false;
        }

        private bool ValidateDocumentFile(byte[] buffer, int bytesRead, string fileName)
        {
            if (bytesRead < 4) return false;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            switch (extension)
            {
                case ".pdf":
                    // PDF signature: %PDF
                    return buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46;
                default:
                    return true; // Allow other document types for now
            }
        }

        private async Task DeletePhysicalFileAsync(string relativeUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(relativeUrl) || !relativeUrl.StartsWith("/uploads/"))
                    return;

                var physicalPath = Path.Combine(_environment.WebRootPath, relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    _logger.LogInformation("Physical file deleted: {PhysicalPath}", physicalPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting physical file: {RelativeUrl}", relativeUrl);
                // Don't throw - this is cleanup, not critical
            }
        }
    }
}