using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for managing defect categories in the quality management system
/// Provides comprehensive CRUD operations and business logic for defect classification
/// </summary>
public class DefectCategoryService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<DefectCategoryService> _logger;

    public DefectCategoryService(SchedulerContext context, ILogger<DefectCategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all defect categories with optional filtering
    /// </summary>
    public async Task<List<DefectCategory>> GetAllDefectCategoriesAsync(
        string searchTerm = "",
        string categoryGroup = "",
        int? severityLevel = null,
        bool? isActive = null)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("Getting all defect categories - Operation: {OperationId}", operationId);

        try
        {
            var query = _context.DefectCategories
                .Include(dc => dc.InspectionCheckpoints)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(dc => 
                    dc.Name.Contains(searchTerm) ||
                    dc.Description.Contains(searchTerm) ||
                    dc.Code.Contains(searchTerm));
            }

            // Apply category group filter
            if (!string.IsNullOrEmpty(categoryGroup))
            {
                query = query.Where(dc => dc.CategoryGroup == categoryGroup);
            }

            // Apply severity level filter
            if (severityLevel.HasValue)
            {
                query = query.Where(dc => dc.SeverityLevel == severityLevel.Value);
            }

            // Apply active status filter
            if (isActive.HasValue)
            {
                query = query.Where(dc => dc.IsActive == isActive.Value);
            }

            var categories = await query
                .OrderBy(dc => dc.SortOrder)
                .ThenBy(dc => dc.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} defect categories - Operation: {OperationId}", 
                categories.Count, operationId);

            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving defect categories - Operation: {OperationId}", operationId);
            throw;
        }
    }

    /// <summary>
    /// Get defect category by ID
    /// </summary>
    public async Task<DefectCategory?> GetDefectCategoryByIdAsync(int id)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("Getting defect category by ID: {Id} - Operation: {OperationId}", id, operationId);

        try
        {
            var category = await _context.DefectCategories
                .Include(dc => dc.InspectionCheckpoints)
                .AsNoTracking()
                .FirstOrDefaultAsync(dc => dc.Id == id);

            if (category != null)
            {
                _logger.LogInformation("Found defect category: {Name} - Operation: {OperationId}", 
                    category.Name, operationId);
            }
            else
            {
                _logger.LogWarning("Defect category not found for ID: {Id} - Operation: {OperationId}", 
                    id, operationId);
            }

            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving defect category by ID: {Id} - Operation: {OperationId}", 
                id, operationId);
            throw;
        }
    }

    /// <summary>
    /// Create new defect category
    /// </summary>
    public async Task<DefectCategory> CreateDefectCategoryAsync(DefectCategory category)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("Creating defect category: {Name} - Operation: {OperationId}", 
            category.Name, operationId);

        try
        {
            // Validate unique code if provided
            if (!string.IsNullOrEmpty(category.Code))
            {
                var existingWithCode = await _context.DefectCategories
                    .AnyAsync(dc => dc.Code == category.Code);

                if (existingWithCode)
                {
                    throw new InvalidOperationException($"Defect category code '{category.Code}' already exists");
                }
            }

            // Set audit fields
            category.CreatedDate = DateTime.UtcNow;
            category.LastModifiedDate = DateTime.UtcNow;

            // Ensure sort order
            if (category.SortOrder == 0)
            {
                var maxSortOrder = await _context.DefectCategories
                    .MaxAsync(dc => (int?)dc.SortOrder) ?? 0;
                category.SortOrder = maxSortOrder + 10;
            }

            _context.DefectCategories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created defect category: {Name} (ID: {Id}) - Operation: {OperationId}", 
                category.Name, category.Id, operationId);

            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating defect category: {Name} - Operation: {OperationId}", 
                category.Name, operationId);
            throw;
        }
    }

    /// <summary>
    /// Update existing defect category
    /// </summary>
    public async Task<bool> UpdateDefectCategoryAsync(DefectCategory category)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("Updating defect category: {Id} - Operation: {OperationId}", 
            category.Id, operationId);

        try
        {
            var existingCategory = await _context.DefectCategories
                .FirstOrDefaultAsync(dc => dc.Id == category.Id);

            if (existingCategory == null)
            {
                _logger.LogWarning("Defect category not found for update: {Id} - Operation: {OperationId}", 
                    category.Id, operationId);
                return false;
            }

            // Validate unique code if changed
            if (!string.IsNullOrEmpty(category.Code) && 
                category.Code != existingCategory.Code)
            {
                var existingWithCode = await _context.DefectCategories
                    .AnyAsync(dc => dc.Code == category.Code && dc.Id != category.Id);

                if (existingWithCode)
                {
                    throw new InvalidOperationException($"Defect category code '{category.Code}' already exists");
                }
            }

            // Update properties
            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.Code = category.Code;
            existingCategory.SeverityLevel = category.SeverityLevel;
            existingCategory.IsActive = category.IsActive;
            existingCategory.CategoryGroup = category.CategoryGroup;
            existingCategory.ApplicableProcesses = category.ApplicableProcesses;
            existingCategory.StandardCorrectiveActions = category.StandardCorrectiveActions;
            existingCategory.PreventionMethods = category.PreventionMethods;
            existingCategory.RequiresImmediateNotification = category.RequiresImmediateNotification;
            existingCategory.CostImpact = category.CostImpact;
            existingCategory.AverageResolutionTimeMinutes = category.AverageResolutionTimeMinutes;
            existingCategory.SortOrder = category.SortOrder;
            existingCategory.ColorCode = category.ColorCode;
            existingCategory.Icon = category.Icon;
            existingCategory.LastModifiedDate = DateTime.UtcNow;
            existingCategory.LastModifiedBy = category.LastModifiedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated defect category: {Name} (ID: {Id}) - Operation: {OperationId}", 
                existingCategory.Name, existingCategory.Id, operationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating defect category: {Id} - Operation: {OperationId}", 
                category.Id, operationId);
            throw;
        }
    }

    /// <summary>
    /// Delete defect category
    /// </summary>
    public async Task<bool> DeleteDefectCategoryAsync(int id)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("Deleting defect category: {Id} - Operation: {OperationId}", id, operationId);

        try
        {
            var category = await _context.DefectCategories
                .Include(dc => dc.InspectionCheckpoints)
                .FirstOrDefaultAsync(dc => dc.Id == id);

            if (category == null)
            {
                _logger.LogWarning("Defect category not found for deletion: {Id} - Operation: {OperationId}", 
                    id, operationId);
                return false;
            }

            // Check if category can be safely deleted
            if (!category.CanBeDeleted())
            {
                throw new InvalidOperationException(
                    $"Cannot delete defect category '{category.Name}' because it is referenced by {category.InspectionCheckpoints.Count} inspection checkpoints");
            }

            _context.DefectCategories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted defect category: {Name} (ID: {Id}) - Operation: {OperationId}", 
                category.Name, id, operationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting defect category: {Id} - Operation: {OperationId}", id, operationId);
            throw;
        }
    }

    /// <summary>
    /// Get available category groups
    /// </summary>
    public async Task<List<string>> GetCategoryGroupsAsync()
    {
        try
        {
            var groups = await _context.DefectCategories
                .Where(dc => dc.IsActive && !string.IsNullOrEmpty(dc.CategoryGroup))
                .Select(dc => dc.CategoryGroup)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            // Add standard groups that might not be in use yet
            var standardGroups = new[]
            {
                DefectCategoryGroups.Surface,
                DefectCategoryGroups.Dimensional,
                DefectCategoryGroups.Material,
                DefectCategoryGroups.Functional,
                DefectCategoryGroups.Cosmetic,
                DefectCategoryGroups.Assembly,
                DefectCategoryGroups.Process,
                DefectCategoryGroups.Handling,
                DefectCategoryGroups.Documentation,
                DefectCategoryGroups.Other
            };

            return groups.Union(standardGroups).Distinct().OrderBy(g => g).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category groups");
            throw;
        }
    }

    /// <summary>
    /// Get defect category statistics
    /// </summary>
    public async Task<DefectCategoryStatistics> GetStatisticsAsync()
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("Getting defect category statistics - Operation: {OperationId}", operationId);

        try
        {
            var totalCategories = await _context.DefectCategories.CountAsync();
            var activeCategories = await _context.DefectCategories.CountAsync(dc => dc.IsActive);
            var categoriesWithCheckpoints = await _context.DefectCategories
                .CountAsync(dc => dc.InspectionCheckpoints.Any());

            var severityDistribution = await _context.DefectCategories
                .Where(dc => dc.IsActive)
                .GroupBy(dc => dc.SeverityLevel)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .ToListAsync();

            var groupDistribution = await _context.DefectCategories
                .Where(dc => dc.IsActive)
                .GroupBy(dc => dc.CategoryGroup)
                .Select(g => new { Group = g.Key, Count = g.Count() })
                .ToListAsync();

            var statistics = new DefectCategoryStatistics
            {
                TotalCategories = totalCategories,
                ActiveCategories = activeCategories,
                InactiveCategories = totalCategories - activeCategories,
                CategoriesWithCheckpoints = categoriesWithCheckpoints,
                SeverityDistribution = severityDistribution.ToDictionary(x => x.Severity, x => x.Count),
                GroupDistribution = groupDistribution.ToDictionary(x => x.Group ?? "Unknown", x => x.Count)
            };

            _logger.LogInformation("Retrieved defect category statistics: {Total} total, {Active} active - Operation: {OperationId}", 
                totalCategories, activeCategories, operationId);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving defect category statistics - Operation: {OperationId}", operationId);
            throw;
        }
    }

    /// <summary>
    /// Reorder defect categories within a group
    /// </summary>
    public async Task<bool> ReorderCategoriesAsync(List<int> categoryIds)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("Reordering {Count} defect categories - Operation: {OperationId}", 
            categoryIds.Count, operationId);

        try
        {
            var categories = await _context.DefectCategories
                .Where(dc => categoryIds.Contains(dc.Id))
                .ToListAsync();

            for (int i = 0; i < categoryIds.Count; i++)
            {
                var category = categories.FirstOrDefault(dc => dc.Id == categoryIds[i]);
                if (category != null)
                {
                    category.SortOrder = (i + 1) * 10;
                    category.LastModifiedDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reordered {Count} defect categories - Operation: {OperationId}", 
                categories.Count, operationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering defect categories - Operation: {OperationId}", operationId);
            throw;
        }
    }

    /// <summary>
    /// Toggle active status of a defect category
    /// </summary>
    public async Task<bool> ToggleActiveStatusAsync(int id)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("Toggling active status for defect category: {Id} - Operation: {OperationId}", 
            id, operationId);

        try
        {
            var category = await _context.DefectCategories
                .FirstOrDefaultAsync(dc => dc.Id == id);

            if (category == null)
            {
                _logger.LogWarning("Defect category not found for status toggle: {Id} - Operation: {OperationId}", 
                    id, operationId);
                return false;
            }

            category.IsActive = !category.IsActive;
            category.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Toggled active status for defect category: {Name} to {Status} - Operation: {OperationId}", 
                category.Name, category.IsActive ? "Active" : "Inactive", operationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling active status for defect category: {Id} - Operation: {OperationId}", 
                id, operationId);
            throw;
        }
    }
}

/// <summary>
/// Statistics model for defect categories
/// </summary>
public class DefectCategoryStatistics
{
    public int TotalCategories { get; set; }
    public int ActiveCategories { get; set; }
    public int InactiveCategories { get; set; }
    public int CategoriesWithCheckpoints { get; set; }
    public Dictionary<int, int> SeverityDistribution { get; set; } = new();
    public Dictionary<string, int> GroupDistribution { get; set; } = new();
}