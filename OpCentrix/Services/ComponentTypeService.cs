using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services
{
    /// <summary>
    /// Service for managing ComponentTypes lookup data
    /// </summary>
    public class ComponentTypeService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<ComponentTypeService> _logger;

        public ComponentTypeService(SchedulerContext context, ILogger<ComponentTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all active component types ordered by sort order
        /// </summary>
        public async Task<List<ComponentType>> GetActiveComponentTypesAsync()
        {
            try
            {
                return await _context.ComponentTypes
                    .Where(ct => ct.IsActive)
                    .OrderBy(ct => ct.SortOrder)
                    .ThenBy(ct => ct.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active component types");
                return new List<ComponentType>();
            }
        }

        /// <summary>
        /// Get all component types (including inactive)
        /// </summary>
        public async Task<List<ComponentType>> GetAllComponentTypesAsync()
        {
            try
            {
                return await _context.ComponentTypes
                    .OrderBy(ct => ct.SortOrder)
                    .ThenBy(ct => ct.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all component types");
                return new List<ComponentType>();
            }
        }

        /// <summary>
        /// Get component type by ID
        /// </summary>
        public async Task<ComponentType?> GetComponentTypeByIdAsync(int id)
        {
            try
            {
                return await _context.ComponentTypes.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving component type with ID: {ComponentTypeId}", id);
                return null;
            }
        }

        /// <summary>
        /// Create a new component type
        /// </summary>
        public async Task<ComponentType> CreateComponentTypeAsync(ComponentType componentType)
        {
            try
            {
                componentType.CreatedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                componentType.LastModifiedDate = componentType.CreatedDate;

                _context.ComponentTypes.Add(componentType);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new component type: {ComponentTypeName} (ID: {ComponentTypeId})", 
                    componentType.Name, componentType.Id);

                return componentType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating component type: {ComponentTypeName}", componentType.Name);
                throw;
            }
        }

        /// <summary>
        /// Update an existing component type
        /// </summary>
        public async Task<ComponentType> UpdateComponentTypeAsync(ComponentType componentType)
        {
            try
            {
                componentType.LastModifiedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                _context.ComponentTypes.Update(componentType);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated component type: {ComponentTypeName} (ID: {ComponentTypeId})", 
                    componentType.Name, componentType.Id);

                return componentType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating component type: {ComponentTypeName} (ID: {ComponentTypeId})", 
                    componentType.Name, componentType.Id);
                throw;
            }
        }

        /// <summary>
        /// Soft delete a component type (mark as inactive)
        /// </summary>
        public async Task<bool> DeactivateComponentTypeAsync(int id)
        {
            try
            {
                var componentType = await _context.ComponentTypes.FindAsync(id);
                if (componentType == null)
                {
                    _logger.LogWarning("Component type not found for deactivation: {ComponentTypeId}", id);
                    return false;
                }

                componentType.IsActive = false;
                componentType.LastModifiedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deactivated component type: {ComponentTypeName} (ID: {ComponentTypeId})", 
                    componentType.Name, componentType.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating component type with ID: {ComponentTypeId}", id);
                return false;
            }
        }

        /// <summary>
        /// Check if component type name already exists (for validation)
        /// </summary>
        public async Task<bool> ComponentTypeNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                var query = _context.ComponentTypes.Where(ct => ct.Name.ToLower() == name.ToLower());
                
                if (excludeId.HasValue)
                {
                    query = query.Where(ct => ct.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking component type name existence: {ComponentTypeName}", name);
                return false;
            }
        }

        /// <summary>
        /// Get component type usage statistics
        /// </summary>
        public async Task<Dictionary<int, int>> GetComponentTypeUsageAsync()
        {
            try
            {
                return await _context.Parts
                    .Where(p => p.ComponentTypeId.HasValue && p.IsActive)
                    .GroupBy(p => p.ComponentTypeId.Value)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving component type usage statistics");
                return new Dictionary<int, int>();
            }
        }
    }
}