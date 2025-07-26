using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service interface for managing inspection checkpoints
/// </summary>
public interface IInspectionCheckpointService
{
    Task<IEnumerable<InspectionCheckpoint>> GetAllCheckpointsAsync();
    Task<IEnumerable<InspectionCheckpoint>> GetCheckpointsByPartIdAsync(int partId);
    Task<InspectionCheckpoint?> GetCheckpointByIdAsync(int id);
    Task<bool> CreateCheckpointAsync(InspectionCheckpoint checkpoint);
    Task<bool> UpdateCheckpointAsync(InspectionCheckpoint checkpoint);
    Task<bool> DeleteCheckpointAsync(int id);
    Task<bool> ReorderCheckpointsAsync(int partId, List<int> checkpointIds);
    Task<bool> DuplicateCheckpointAsync(int sourceId, int targetPartId);
    Task<IEnumerable<string>> GetInspectionTypesAsync();
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<bool> ValidateCheckpointConfigurationAsync(int partId);
    Task<int> GetNextSortOrderAsync(int partId);
    Task<IEnumerable<InspectionCheckpoint>> GetActiveCheckpointsForJobAsync(int partId);
    Task<bool> ToggleCheckpointStatusAsync(int id);
}

/// <summary>
/// Service for managing inspection checkpoints for quality control
/// </summary>
public class InspectionCheckpointService : IInspectionCheckpointService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<InspectionCheckpointService> _logger;

    public InspectionCheckpointService(
        SchedulerContext context,
        ILogger<InspectionCheckpointService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all inspection checkpoints with part information
    /// </summary>
    public async Task<IEnumerable<InspectionCheckpoint>> GetAllCheckpointsAsync()
    {
        try
        {
            return await _context.InspectionCheckpoints
                .Include(c => c.Part)
                .OrderBy(c => c.Part.PartNumber)
                .ThenBy(c => c.SortOrder)
                .ThenBy(c => c.CheckpointName)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all inspection checkpoints");
            return Enumerable.Empty<InspectionCheckpoint>();
        }
    }

    /// <summary>
    /// Get all checkpoints for a specific part
    /// </summary>
    public async Task<IEnumerable<InspectionCheckpoint>> GetCheckpointsByPartIdAsync(int partId)
    {
        try
        {
            return await _context.InspectionCheckpoints
                .Include(c => c.Part)
                .Where(c => c.PartId == partId)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.CheckpointName)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving checkpoints for part {PartId}", partId);
            return Enumerable.Empty<InspectionCheckpoint>();
        }
    }

    /// <summary>
    /// Get a specific checkpoint by ID
    /// </summary>
    public async Task<InspectionCheckpoint?> GetCheckpointByIdAsync(int id)
    {
        try
        {
            return await _context.InspectionCheckpoints
                .Include(c => c.Part)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving checkpoint {CheckpointId}", id);
            return null;
        }
    }

    /// <summary>
    /// Create a new inspection checkpoint
    /// </summary>
    public async Task<bool> CreateCheckpointAsync(InspectionCheckpoint checkpoint)
    {
        try
        {
            // Validate the part exists
            var partExists = await _context.Parts.AnyAsync(p => p.Id == checkpoint.PartId);
            if (!partExists)
            {
                _logger.LogWarning("Cannot create checkpoint: Part {PartId} does not exist", checkpoint.PartId);
                return false;
            }

            // Set audit fields
            checkpoint.CreatedDate = DateTime.UtcNow;
            checkpoint.LastModifiedDate = DateTime.UtcNow;

            // Ensure sort order is set
            if (checkpoint.SortOrder <= 0)
            {
                checkpoint.SortOrder = await GetNextSortOrderAsync(checkpoint.PartId);
            }

            _context.InspectionCheckpoints.Add(checkpoint);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created inspection checkpoint {CheckpointName} for part {PartId}", 
                checkpoint.CheckpointName, checkpoint.PartId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inspection checkpoint {CheckpointName}", 
                checkpoint.CheckpointName);
            return false;
        }
    }

    /// <summary>
    /// Update an existing inspection checkpoint
    /// </summary>
    public async Task<bool> UpdateCheckpointAsync(InspectionCheckpoint checkpoint)
    {
        try
        {
            var existingCheckpoint = await _context.InspectionCheckpoints
                .FirstOrDefaultAsync(c => c.Id == checkpoint.Id);

            if (existingCheckpoint == null)
            {
                _logger.LogWarning("Cannot update checkpoint: Checkpoint {CheckpointId} not found", 
                    checkpoint.Id);
                return false;
            }

            // Update properties
            existingCheckpoint.CheckpointName = checkpoint.CheckpointName;
            existingCheckpoint.Description = checkpoint.Description;
            existingCheckpoint.InspectionType = checkpoint.InspectionType;
            existingCheckpoint.SortOrder = checkpoint.SortOrder;
            existingCheckpoint.IsRequired = checkpoint.IsRequired;
            existingCheckpoint.IsActive = checkpoint.IsActive;
            existingCheckpoint.EstimatedMinutes = checkpoint.EstimatedMinutes;
            existingCheckpoint.AcceptanceCriteria = checkpoint.AcceptanceCriteria;
            existingCheckpoint.MeasurementMethod = checkpoint.MeasurementMethod;
            existingCheckpoint.RequiredEquipment = checkpoint.RequiredEquipment;
            existingCheckpoint.RequiredSkills = checkpoint.RequiredSkills;
            existingCheckpoint.ReferenceDocuments = checkpoint.ReferenceDocuments;
            existingCheckpoint.TargetValue = checkpoint.TargetValue;
            existingCheckpoint.UpperTolerance = checkpoint.UpperTolerance;
            existingCheckpoint.LowerTolerance = checkpoint.LowerTolerance;
            existingCheckpoint.Unit = checkpoint.Unit;
            existingCheckpoint.FailureAction = checkpoint.FailureAction;
            existingCheckpoint.SampleSize = checkpoint.SampleSize;
            existingCheckpoint.SamplingMethod = checkpoint.SamplingMethod;
            existingCheckpoint.Category = checkpoint.Category;
            existingCheckpoint.Priority = checkpoint.Priority;
            existingCheckpoint.Notes = checkpoint.Notes;
            existingCheckpoint.LastModifiedDate = DateTime.UtcNow;
            existingCheckpoint.LastModifiedBy = checkpoint.LastModifiedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated inspection checkpoint {CheckpointName} (ID: {CheckpointId})", 
                checkpoint.CheckpointName, checkpoint.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inspection checkpoint {CheckpointId}", checkpoint.Id);
            return false;
        }
    }

    /// <summary>
    /// Delete an inspection checkpoint
    /// </summary>
    public async Task<bool> DeleteCheckpointAsync(int id)
    {
        try
        {
            var checkpoint = await _context.InspectionCheckpoints
                .FirstOrDefaultAsync(c => c.Id == id);

            if (checkpoint == null)
            {
                _logger.LogWarning("Cannot delete checkpoint: Checkpoint {CheckpointId} not found", id);
                return false;
            }

            _context.InspectionCheckpoints.Remove(checkpoint);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted inspection checkpoint {CheckpointName} (ID: {CheckpointId})", 
                checkpoint.CheckpointName, id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inspection checkpoint {CheckpointId}", id);
            return false;
        }
    }

    /// <summary>
    /// Reorder checkpoints for a specific part
    /// </summary>
    public async Task<bool> ReorderCheckpointsAsync(int partId, List<int> checkpointIds)
    {
        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Get all checkpoints for this part
            var checkpoints = await _context.InspectionCheckpoints
                .Where(c => c.PartId == partId && checkpointIds.Contains(c.Id))
                .ToListAsync();

            // Update sort order based on the provided order
            for (int i = 0; i < checkpointIds.Count; i++)
            {
                var checkpoint = checkpoints.FirstOrDefault(c => c.Id == checkpointIds[i]);
                if (checkpoint != null)
                {
                    checkpoint.SortOrder = (i + 1) * 10; // Allow space for insertion
                    checkpoint.LastModifiedDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Reordered {Count} checkpoints for part {PartId}", 
                checkpoints.Count, partId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering checkpoints for part {PartId}", partId);
            return false;
        }
    }

    /// <summary>
    /// Duplicate a checkpoint to another part
    /// </summary>
    public async Task<bool> DuplicateCheckpointAsync(int sourceId, int targetPartId)
    {
        try
        {
            var sourceCheckpoint = await GetCheckpointByIdAsync(sourceId);
            if (sourceCheckpoint == null)
            {
                _logger.LogWarning("Cannot duplicate checkpoint: Source checkpoint {SourceId} not found", 
                    sourceId);
                return false;
            }

            // Validate target part exists
            var targetPartExists = await _context.Parts.AnyAsync(p => p.Id == targetPartId);
            if (!targetPartExists)
            {
                _logger.LogWarning("Cannot duplicate checkpoint: Target part {PartId} does not exist", 
                    targetPartId);
                return false;
            }

            // Create new checkpoint based on source
            var newCheckpoint = new InspectionCheckpoint
            {
                PartId = targetPartId,
                CheckpointName = sourceCheckpoint.CheckpointName,
                Description = sourceCheckpoint.Description,
                InspectionType = sourceCheckpoint.InspectionType,
                SortOrder = await GetNextSortOrderAsync(targetPartId),
                IsRequired = sourceCheckpoint.IsRequired,
                IsActive = sourceCheckpoint.IsActive,
                EstimatedMinutes = sourceCheckpoint.EstimatedMinutes,
                AcceptanceCriteria = sourceCheckpoint.AcceptanceCriteria,
                MeasurementMethod = sourceCheckpoint.MeasurementMethod,
                RequiredEquipment = sourceCheckpoint.RequiredEquipment,
                RequiredSkills = sourceCheckpoint.RequiredSkills,
                ReferenceDocuments = sourceCheckpoint.ReferenceDocuments,
                TargetValue = sourceCheckpoint.TargetValue,
                UpperTolerance = sourceCheckpoint.UpperTolerance,
                LowerTolerance = sourceCheckpoint.LowerTolerance,
                Unit = sourceCheckpoint.Unit,
                FailureAction = sourceCheckpoint.FailureAction,
                SampleSize = sourceCheckpoint.SampleSize,
                SamplingMethod = sourceCheckpoint.SamplingMethod,
                Category = sourceCheckpoint.Category,
                Priority = sourceCheckpoint.Priority,
                Notes = sourceCheckpoint.Notes,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = "System",
                LastModifiedBy = "System"
            };

            return await CreateCheckpointAsync(newCheckpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating checkpoint {SourceId} to part {TargetPartId}", 
                sourceId, targetPartId);
            return false;
        }
    }

    /// <summary>
    /// Get available inspection types
    /// </summary>
    public async Task<IEnumerable<string>> GetInspectionTypesAsync()
    {
        try
        {
            var types = await _context.InspectionCheckpoints
                .Where(c => !string.IsNullOrEmpty(c.InspectionType))
                .Select(c => c.InspectionType)
                .Distinct()
                .OrderBy(t => t)
                .AsNoTracking()
                .ToListAsync();

            // Add standard types if not present
            var standardTypes = new[] 
            { 
                "Visual", "Dimensional", "Functional", "Destructive", 
                "Non-Destructive", "Surface", "Material", "Documentation" 
            };

            return standardTypes.Union(types).Distinct().OrderBy(t => t);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inspection types");
            return new[] { "Visual", "Dimensional", "Functional" };
        }
    }

    /// <summary>
    /// Get available categories
    /// </summary>
    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        try
        {
            var categories = await _context.InspectionCheckpoints
                .Where(c => !string.IsNullOrEmpty(c.Category))
                .Select(c => c.Category)
                .Distinct()
                .OrderBy(c => c)
                .AsNoTracking()
                .ToListAsync();

            // Add standard categories if not present
            var standardCategories = new[] 
            { 
                "Quality", "Safety", "Compliance", "Process", 
                "Material", "Dimensional", "Surface", "Assembly" 
            };

            return standardCategories.Union(categories).Distinct().OrderBy(c => c);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving checkpoint categories");
            return new[] { "Quality", "Safety", "Compliance" };
        }
    }

    /// <summary>
    /// Validate checkpoint configuration for a part
    /// </summary>
    public async Task<bool> ValidateCheckpointConfigurationAsync(int partId)
    {
        try
        {
            var checkpoints = await GetCheckpointsByPartIdAsync(partId);
            
            // Check for duplicate sort orders
            var sortOrders = checkpoints.Select(c => c.SortOrder).ToList();
            if (sortOrders.Count != sortOrders.Distinct().Count())
            {
                _logger.LogWarning("Part {PartId} has duplicate checkpoint sort orders", partId);
                return false;
            }

            // Check for missing required information
            var invalidCheckpoints = checkpoints.Where(c => 
                string.IsNullOrWhiteSpace(c.CheckpointName) ||
                string.IsNullOrWhiteSpace(c.InspectionType) ||
                (c.IsDimensional && !c.TargetValue.HasValue));

            if (invalidCheckpoints.Any())
            {
                _logger.LogWarning("Part {PartId} has {Count} invalid checkpoints", 
                    partId, invalidCheckpoints.Count());
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating checkpoint configuration for part {PartId}", partId);
            return false;
        }
    }

    /// <summary>
    /// Get the next available sort order for a part
    /// </summary>
    public async Task<int> GetNextSortOrderAsync(int partId)
    {
        try
        {
            var maxSortOrder = await _context.InspectionCheckpoints
                .Where(c => c.PartId == partId)
                .MaxAsync(c => (int?)c.SortOrder) ?? 0;

            return maxSortOrder + 10;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next sort order for part {PartId}", partId);
            return 100;
        }
    }

    /// <summary>
    /// Get active checkpoints for job execution
    /// </summary>
    public async Task<IEnumerable<InspectionCheckpoint>> GetActiveCheckpointsForJobAsync(int partId)
    {
        try
        {
            return await _context.InspectionCheckpoints
                .Include(c => c.Part)
                .Where(c => c.PartId == partId && c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.CheckpointName)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active checkpoints for job with part {PartId}", partId);
            return Enumerable.Empty<InspectionCheckpoint>();
        }
    }

    /// <summary>
    /// Toggle checkpoint active status
    /// </summary>
    public async Task<bool> ToggleCheckpointStatusAsync(int id)
    {
        try
        {
            var checkpoint = await _context.InspectionCheckpoints
                .FirstOrDefaultAsync(c => c.Id == id);

            if (checkpoint == null)
            {
                _logger.LogWarning("Cannot toggle status: Checkpoint {CheckpointId} not found", id);
                return false;
            }

            checkpoint.IsActive = !checkpoint.IsActive;
            checkpoint.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Toggled checkpoint {CheckpointName} status to {Status}", 
                checkpoint.CheckpointName, checkpoint.IsActive ? "Active" : "Inactive");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling checkpoint status for {CheckpointId}", id);
            return false;
        }
    }
}