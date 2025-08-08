using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services
{
    /// <summary>
    /// Service for managing ComplianceCategories lookup data
    /// </summary>
    public class ComplianceCategoryService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<ComplianceCategoryService> _logger;

        public ComplianceCategoryService(SchedulerContext context, ILogger<ComplianceCategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all active compliance categories ordered by sort order
        /// </summary>
        public async Task<List<ComplianceCategory>> GetActiveCategoriesAsync()
        {
            try
            {
                return await _context.ComplianceCategories
                    .Where(cc => cc.IsActive)
                    .OrderBy(cc => cc.SortOrder)
                    .ThenBy(cc => cc.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active compliance categories");
                return new List<ComplianceCategory>();
            }
        }

        /// <summary>
        /// Get all compliance categories (including inactive)
        /// </summary>
        public async Task<List<ComplianceCategory>> GetAllCategoriesAsync()
        {
            try
            {
                return await _context.ComplianceCategories
                    .OrderBy(cc => cc.SortOrder)
                    .ThenBy(cc => cc.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all compliance categories");
                return new List<ComplianceCategory>();
            }
        }

        /// <summary>
        /// Get compliance category by ID
        /// </summary>
        public async Task<ComplianceCategory?> GetCategoryByIdAsync(int id)
        {
            try
            {
                return await _context.ComplianceCategories.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance category with ID: {CategoryId}", id);
                return null;
            }
        }

        /// <summary>
        /// Get compliance categories by regulatory level
        /// </summary>
        public async Task<List<ComplianceCategory>> GetCategoriesByRegulatoryLevelAsync(string regulatoryLevel)
        {
            try
            {
                return await _context.ComplianceCategories
                    .Where(cc => cc.IsActive && cc.RegulatoryLevel == regulatoryLevel)
                    .OrderBy(cc => cc.SortOrder)
                    .ThenBy(cc => cc.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance categories for regulatory level: {RegulatoryLevel}", regulatoryLevel);
                return new List<ComplianceCategory>();
            }
        }

        /// <summary>
        /// Get categories that require special handling
        /// </summary>
        public async Task<List<ComplianceCategory>> GetSpecialHandlingCategoriesAsync()
        {
            try
            {
                return await _context.ComplianceCategories
                    .Where(cc => cc.IsActive && cc.RequiresSpecialHandling)
                    .OrderBy(cc => cc.SortOrder)
                    .ThenBy(cc => cc.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving special handling compliance categories");
                return new List<ComplianceCategory>();
            }
        }

        /// <summary>
        /// Create a new compliance category
        /// </summary>
        public async Task<ComplianceCategory> CreateCategoryAsync(ComplianceCategory category)
        {
            try
            {
                category.CreatedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                category.LastModifiedDate = category.CreatedDate;

                _context.ComplianceCategories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new compliance category: {CategoryName} (ID: {CategoryId})", 
                    category.Name, category.Id);

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating compliance category: {CategoryName}", category.Name);
                throw;
            }
        }

        /// <summary>
        /// Update an existing compliance category
        /// </summary>
        public async Task<ComplianceCategory> UpdateCategoryAsync(ComplianceCategory category)
        {
            try
            {
                category.LastModifiedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                _context.ComplianceCategories.Update(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated compliance category: {CategoryName} (ID: {CategoryId})", 
                    category.Name, category.Id);

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating compliance category: {CategoryName} (ID: {CategoryId})", 
                    category.Name, category.Id);
                throw;
            }
        }

        /// <summary>
        /// Soft delete a compliance category (mark as inactive)
        /// </summary>
        public async Task<bool> DeactivateCategoryAsync(int id)
        {
            try
            {
                var category = await _context.ComplianceCategories.FindAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("Compliance category not found for deactivation: {CategoryId}", id);
                    return false;
                }

                category.IsActive = false;
                category.LastModifiedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deactivated compliance category: {CategoryName} (ID: {CategoryId})", 
                    category.Name, category.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating compliance category with ID: {CategoryId}", id);
                return false;
            }
        }

        /// <summary>
        /// Check if compliance category name already exists (for validation)
        /// </summary>
        public async Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                var query = _context.ComplianceCategories.Where(cc => cc.Name.ToLower() == name.ToLower());
                
                if (excludeId.HasValue)
                {
                    query = query.Where(cc => cc.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking compliance category name existence: {CategoryName}", name);
                return false;
            }
        }

        /// <summary>
        /// Get compliance category usage statistics
        /// </summary>
        public async Task<Dictionary<int, int>> GetCategoryUsageAsync()
        {
            try
            {
                return await _context.Parts
                    .Where(p => p.ComplianceCategoryId.HasValue && p.IsActive)
                    .GroupBy(p => p.ComplianceCategoryId.Value)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance category usage statistics");
                return new Dictionary<int, int>();
            }
        }

        /// <summary>
        /// Get regulatory level summary
        /// </summary>
        public async Task<Dictionary<string, int>> GetRegulatoryLevelSummaryAsync()
        {
            try
            {
                return await _context.ComplianceCategories
                    .Where(cc => cc.IsActive)
                    .GroupBy(cc => cc.RegulatoryLevel)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving regulatory level summary");
                return new Dictionary<string, int>();
            }
        }
    }
}