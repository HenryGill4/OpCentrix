using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for managing materials
/// Enhanced for Task 6: Machine Material Management
/// </summary>
public interface IMaterialService
{
    Task<List<Material>> GetAllMaterialsAsync();
    Task<List<Material>> GetActiveMaterialsAsync();
    Task<List<Material>> GetMaterialsByTypeAsync(string materialType);
    Task<List<Material>> GetCompatibleMaterialsAsync(string machineType);
    Task<Material?> GetMaterialByCodeAsync(string materialCode);
    Task<Material?> GetMaterialByIdAsync(int id);
    Task<Material> CreateMaterialAsync(Material material, string createdBy);
    Task<Material> UpdateMaterialAsync(Material material, string modifiedBy);
    Task<bool> DeleteMaterialAsync(int id);
    Task SeedDefaultMaterialsAsync();
    Task<List<string>> GetMaterialTypesAsync();
    Task<List<string>> GetMaterialCodesAsync();
}

public class MaterialService : IMaterialService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<MaterialService> _logger;

    public MaterialService(SchedulerContext context, ILogger<MaterialService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Material>> GetAllMaterialsAsync()
    {
        try
        {
            return await _context.Materials
                .OrderBy(m => m.MaterialType)
                .ThenBy(m => m.MaterialCode)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all materials");
            return new List<Material>();
        }
    }

    public async Task<List<Material>> GetActiveMaterialsAsync()
    {
        try
        {
            return await _context.Materials
                .Where(m => m.IsActive)
                .OrderBy(m => m.MaterialType)
                .ThenBy(m => m.MaterialCode)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active materials");
            return new List<Material>();
        }
    }

    public async Task<List<Material>> GetMaterialsByTypeAsync(string materialType)
    {
        try
        {
            return await _context.Materials
                .Where(m => m.IsActive && m.MaterialType.ToLower() == materialType.ToLower())
                .OrderBy(m => m.MaterialCode)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting materials by type {MaterialType}", materialType);
            return new List<Material>();
        }
    }

    public async Task<List<Material>> GetCompatibleMaterialsAsync(string machineType)
    {
        try
        {
            return await _context.Materials
                .Where(m => m.IsActive && 
                           (string.IsNullOrEmpty(m.CompatibleMachineTypes) || 
                            m.CompatibleMachineTypes.Contains(machineType)))
                .OrderBy(m => m.MaterialType)
                .ThenBy(m => m.MaterialCode)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compatible materials for machine type {MachineType}", machineType);
            return new List<Material>();
        }
    }

    public async Task<Material?> GetMaterialByCodeAsync(string materialCode)
    {
        try
        {
            return await _context.Materials
                .FirstOrDefaultAsync(m => m.MaterialCode.ToLower() == materialCode.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting material by code {MaterialCode}", materialCode);
            return null;
        }
    }

    public async Task<Material?> GetMaterialByIdAsync(int id)
    {
        try
        {
            return await _context.Materials.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting material by ID {MaterialId}", id);
            return null;
        }
    }

    public async Task<Material> CreateMaterialAsync(Material material, string createdBy)
    {
        try
        {
            // Check if material code already exists
            var existingMaterial = await GetMaterialByCodeAsync(material.MaterialCode);
            if (existingMaterial != null)
            {
                throw new InvalidOperationException($"Material with code '{material.MaterialCode}' already exists.");
            }

            material.CreatedBy = createdBy;
            material.LastModifiedBy = createdBy;
            material.CreatedDate = DateTime.UtcNow;
            material.LastModifiedDate = DateTime.UtcNow;

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Material created: {MaterialCode} by {CreatedBy}", material.MaterialCode, createdBy);
            return material;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating material {MaterialCode}", material.MaterialCode);
            throw;
        }
    }

    public async Task<Material> UpdateMaterialAsync(Material material, string modifiedBy)
    {
        try
        {
            var existingMaterial = await GetMaterialByIdAsync(material.Id);
            if (existingMaterial == null)
            {
                throw new InvalidOperationException($"Material with ID {material.Id} not found.");
            }

            // Check if material code is being changed and if it conflicts
            if (existingMaterial.MaterialCode.ToLower() != material.MaterialCode.ToLower())
            {
                var conflictingMaterial = await _context.Materials
                    .FirstOrDefaultAsync(m => m.MaterialCode.ToLower() == material.MaterialCode.ToLower() && m.Id != material.Id);

                if (conflictingMaterial != null)
                {
                    throw new InvalidOperationException($"Material with code '{material.MaterialCode}' already exists.");
                }
            }

            // Update properties
            existingMaterial.MaterialCode = material.MaterialCode;
            existingMaterial.MaterialName = material.MaterialName;
            existingMaterial.MaterialType = material.MaterialType;
            existingMaterial.Description = material.Description;
            existingMaterial.Density = material.Density;
            existingMaterial.MeltingPointC = material.MeltingPointC;
            existingMaterial.IsActive = material.IsActive;
            existingMaterial.CostPerGram = material.CostPerGram;
            existingMaterial.DefaultLayerThicknessMicrons = material.DefaultLayerThicknessMicrons;
            existingMaterial.DefaultLaserPowerPercent = material.DefaultLaserPowerPercent;
            existingMaterial.DefaultScanSpeedMmPerSec = material.DefaultScanSpeedMmPerSec;
            existingMaterial.MaterialProperties = material.MaterialProperties;
            existingMaterial.CompatibleMachineTypes = material.CompatibleMachineTypes;
            existingMaterial.SafetyNotes = material.SafetyNotes;
            existingMaterial.LastModifiedBy = modifiedBy;
            existingMaterial.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Material updated: {MaterialCode} by {ModifiedBy}", material.MaterialCode, modifiedBy);
            return existingMaterial;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating material {MaterialCode}", material.MaterialCode);
            throw;
        }
    }

    public async Task<bool> DeleteMaterialAsync(int id)
    {
        try
        {
            var material = await GetMaterialByIdAsync(id);
            if (material == null)
            {
                return false;
            }

            // Check if material is in use by any machines
            var machinesUsingMaterial = await _context.Machines
                .Where(m => m.CurrentMaterial == material.MaterialCode || 
                           m.SupportedMaterials.Contains(material.MaterialCode))
                .CountAsync();

            if (machinesUsingMaterial > 0)
            {
                throw new InvalidOperationException($"Cannot delete material '{material.MaterialCode}' because it is in use by {machinesUsingMaterial} machine(s).");
            }

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Material deleted: {MaterialCode}", material.MaterialCode);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting material ID {MaterialId}", id);
            throw;
        }
    }

    public async Task SeedDefaultMaterialsAsync()
    {
        try
        {
            // Check if materials already exist
            var existingMaterialCount = await _context.Materials.CountAsync();
            if (existingMaterialCount > 0)
            {
                _logger.LogInformation("Materials already seeded, skipping default material creation");
                return;
            }

            var defaultMaterials = Material.GetCommonSlsMaterials();
            
            foreach (var material in defaultMaterials)
            {
                material.CreatedBy = "System";
                material.LastModifiedBy = "System";
                material.CreatedDate = DateTime.UtcNow;
                material.LastModifiedDate = DateTime.UtcNow;
            }

            _context.Materials.AddRange(defaultMaterials);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} default materials", defaultMaterials.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding default materials");
            throw;
        }
    }

    public async Task<List<string>> GetMaterialTypesAsync()
    {
        try
        {
            return await _context.Materials
                .Where(m => m.IsActive)
                .Select(m => m.MaterialType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting material types");
            return new List<string>();
        }
    }

    public async Task<List<string>> GetMaterialCodesAsync()
    {
        try
        {
            return await _context.Materials
                .Where(m => m.IsActive)
                .Select(m => m.MaterialCode)
                .OrderBy(c => c)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting material codes");
            return new List<string>();
        }
    }
}