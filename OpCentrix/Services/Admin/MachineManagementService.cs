using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for managing SLS machines and their capabilities
/// Task 6: Machine Status and Dynamic Machine Management
/// </summary>
public interface IMachineManagementService
{
    // Machine CRUD operations
    Task<List<SlsMachine>> GetAllMachinesAsync();
    Task<List<SlsMachine>> GetActiveMachinesAsync();
    Task<SlsMachine?> GetMachineByIdAsync(int id);
    Task<SlsMachine?> GetMachineByMachineIdAsync(string machineId);
    Task<bool> CreateMachineAsync(SlsMachine machine);
    Task<bool> UpdateMachineAsync(SlsMachine machine);
    Task<bool> DeleteMachineAsync(int id);

    // Machine status operations
    Task<bool> UpdateMachineStatusAsync(string machineId, string status);
    Task<bool> SetMachineAvailabilityAsync(string machineId, bool isAvailable);
    Task<Dictionary<string, string>> GetMachineStatusesAsync();

    // Machine capability operations
    Task<List<MachineCapability>> GetMachineCapabilitiesAsync(string machineId);
    Task<bool> AddMachineCapabilityAsync(MachineCapability capability);
    Task<bool> UpdateMachineCapabilityAsync(MachineCapability capability);
    Task<bool> RemoveMachineCapabilityAsync(int capabilityId);

    // Machine validation and compatibility
    Task<bool> CanMachineProcessPartAsync(string machineId, int partId);
    Task<List<SlsMachine>> GetCompatibleMachinesForPartAsync(int partId);
    Task<bool> IsMachineIdUniqueAsync(string machineId, int? excludeId = null);

    // Machine statistics and utilization
    Task<Dictionary<string, object>> GetMachineStatisticsAsync();
    Task<double> GetMachineUtilizationAsync(string machineId, DateTime fromDate, DateTime toDate);
    Task<List<SlsMachine>> GetMachinesRequiringMaintenanceAsync();
}

/// <summary>
/// Implementation of machine management service
/// </summary>
public class MachineManagementService : IMachineManagementService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<MachineManagementService> _logger;

    public MachineManagementService(SchedulerContext context, ILogger<MachineManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Machine CRUD Operations

    public async Task<List<SlsMachine>> GetAllMachinesAsync()
    {
        try
        {
            return await _context.SlsMachines
                .OrderBy(m => m.MachineId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all machines");
            return new List<SlsMachine>();
        }
    }

    public async Task<List<SlsMachine>> GetActiveMachinesAsync()
    {
        try
        {
            return await _context.SlsMachines
                .Where(m => m.IsActive && m.IsAvailableForScheduling)
                .OrderBy(m => m.Priority)
                .ThenBy(m => m.MachineId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active machines");
            return new List<SlsMachine>();
        }
    }

    public async Task<SlsMachine?> GetMachineByIdAsync(int id)
    {
        try
        {
            return await _context.SlsMachines
                .Include(m => m.CurrentJob)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machine by ID {MachineId}", id);
            return null;
        }
    }

    public async Task<SlsMachine?> GetMachineByMachineIdAsync(string machineId)
    {
        try
        {
            return await _context.SlsMachines
                .Include(m => m.CurrentJob)
                .FirstOrDefaultAsync(m => m.MachineId == machineId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machine by machine ID {MachineId}", machineId);
            return null;
        }
    }

    public async Task<bool> CreateMachineAsync(SlsMachine machine)
    {
        try
        {
            // Validate machine ID uniqueness
            if (!await IsMachineIdUniqueAsync(machine.MachineId))
            {
                _logger.LogWarning("Cannot create machine - Machine ID {MachineId} already exists", machine.MachineId);
                return false;
            }

            machine.CreatedDate = DateTime.UtcNow;
            machine.LastModifiedDate = DateTime.UtcNow;

            _context.SlsMachines.Add(machine);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Machine created successfully: {MachineId} ({MachineName})", 
                machine.MachineId, machine.MachineName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating machine {MachineId}", machine.MachineId);
            return false;
        }
    }

    public async Task<bool> UpdateMachineAsync(SlsMachine machine)
    {
        try
        {
            var existingMachine = await _context.SlsMachines.FindAsync(machine.Id);
            if (existingMachine == null)
            {
                _logger.LogWarning("Cannot update machine - Machine ID {Id} not found", machine.Id);
                return false;
            }

            // Validate machine ID uniqueness if it's being changed
            if (existingMachine.MachineId != machine.MachineId && 
                !await IsMachineIdUniqueAsync(machine.MachineId, machine.Id))
            {
                _logger.LogWarning("Cannot update machine - Machine ID {MachineId} already exists", machine.MachineId);
                return false;
            }

            // Update properties
            existingMachine.MachineId = machine.MachineId;
            existingMachine.MachineName = machine.MachineName;
            existingMachine.MachineModel = machine.MachineModel;
            existingMachine.SerialNumber = machine.SerialNumber;
            existingMachine.Location = machine.Location;
            existingMachine.SupportedMaterials = machine.SupportedMaterials;
            existingMachine.CurrentMaterial = machine.CurrentMaterial;
            existingMachine.Status = machine.Status;
            existingMachine.IsActive = machine.IsActive;
            existingMachine.IsAvailableForScheduling = machine.IsAvailableForScheduling;
            existingMachine.Priority = machine.Priority;
            existingMachine.BuildLengthMm = machine.BuildLengthMm;
            existingMachine.BuildWidthMm = machine.BuildWidthMm;
            existingMachine.BuildHeightMm = machine.BuildHeightMm;
            existingMachine.MaxLaserPowerWatts = machine.MaxLaserPowerWatts;
            existingMachine.MaxScanSpeedMmPerSec = machine.MaxScanSpeedMmPerSec;
            existingMachine.MinLayerThicknessMicrons = machine.MinLayerThicknessMicrons;
            existingMachine.MaxLayerThicknessMicrons = machine.MaxLayerThicknessMicrons;
            existingMachine.MaintenanceIntervalHours = machine.MaintenanceIntervalHours;
            existingMachine.OpcUaEndpointUrl = machine.OpcUaEndpointUrl;
            existingMachine.OpcUaEnabled = machine.OpcUaEnabled;
            existingMachine.LastModifiedBy = machine.LastModifiedBy;
            existingMachine.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Machine updated successfully: {MachineId} ({MachineName})", 
                machine.MachineId, machine.MachineName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating machine {MachineId}", machine.MachineId);
            return false;
        }
    }

    public async Task<bool> DeleteMachineAsync(int id)
    {
        try
        {
            var machine = await _context.SlsMachines
                .Include(m => m.CurrentJob)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (machine == null)
            {
                _logger.LogWarning("Cannot delete machine - Machine ID {Id} not found", id);
                return false;
            }

            // Check for active jobs
            var hasActiveJobs = await _context.Jobs
                .AnyAsync(j => j.MachineId == machine.MachineId && 
                              j.Status != "Completed" && j.Status != "Cancelled");

            if (hasActiveJobs)
            {
                _logger.LogWarning("Cannot delete machine {MachineId} - has active jobs", machine.MachineId);
                return false;
            }

            // Remove related capabilities
            var capabilities = await _context.MachineCapabilities
                .Where(mc => mc.MachineId == machine.MachineId)
                .ToListAsync();

            if (capabilities.Any())
            {
                _context.MachineCapabilities.RemoveRange(capabilities);
            }

            _context.SlsMachines.Remove(machine);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Machine deleted successfully: {MachineId} ({MachineName})", 
                machine.MachineId, machine.MachineName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting machine ID {MachineId}", id);
            return false;
        }
    }

    #endregion

    #region Machine Status Operations

    public async Task<bool> UpdateMachineStatusAsync(string machineId, string status)
    {
        try
        {
            var machine = await GetMachineByMachineIdAsync(machineId);
            if (machine == null)
            {
                _logger.LogWarning("Cannot update status - Machine {MachineId} not found", machineId);
                return false;
            }

            machine.Status = status;
            machine.LastStatusUpdate = DateTime.UtcNow;
            machine.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Machine status updated: {MachineId} -> {Status}", machineId, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating machine status for {MachineId}", machineId);
            return false;
        }
    }

    public async Task<bool> SetMachineAvailabilityAsync(string machineId, bool isAvailable)
    {
        try
        {
            var machine = await GetMachineByMachineIdAsync(machineId);
            if (machine == null)
            {
                _logger.LogWarning("Cannot set availability - Machine {MachineId} not found", machineId);
                return false;
            }

            machine.IsAvailableForScheduling = isAvailable;
            machine.LastModifiedDate = DateTime.UtcNow;

            // Update status if being taken offline
            if (!isAvailable && machine.Status != "Maintenance")
            {
                machine.Status = "Offline";
                machine.LastStatusUpdate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Machine availability updated: {MachineId} -> {IsAvailable}", 
                machineId, isAvailable);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting machine availability for {MachineId}", machineId);
            return false;
        }
    }

    public async Task<Dictionary<string, string>> GetMachineStatusesAsync()
    {
        try
        {
            return await _context.SlsMachines
                .ToDictionaryAsync(m => m.MachineId, m => m.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machine statuses");
            return new Dictionary<string, string>();
        }
    }

    #endregion

    #region Machine Capability Operations

    public async Task<List<MachineCapability>> GetMachineCapabilitiesAsync(string machineId)
    {
        try
        {
            return await _context.MachineCapabilities
                .Where(c => c.SlsMachineId == machineId && c.IsAvailable) // Fixed property name
                .OrderBy(c => c.Priority)
                .ThenBy(c => c.CapabilityType)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machine capabilities for machine {MachineId}", machineId);
            return new List<MachineCapability>();
        }
    }

    public async Task<bool> AddMachineCapabilityAsync(MachineCapability capability)
    {
        try
        {
            capability.CreatedDate = DateTime.UtcNow;
            capability.LastModifiedDate = DateTime.UtcNow;
            capability.IsActive = true;

            _context.MachineCapabilities.Add(capability);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Machine capability added: {MachineId} - {CapabilityName}", 
                capability.MachineId, capability.CapabilityName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding capability {CapabilityName} to machine {MachineId}", 
                capability.CapabilityName, capability.MachineId);
            return false;
        }
    }

    public async Task<bool> UpdateMachineCapabilityAsync(MachineCapability capability)
    {
        try
        {
            var existing = await _context.MachineCapabilities.FindAsync(capability.Id);
            if (existing == null)
                return false;

            // Update with correct property names
            existing.CapabilityType = capability.CapabilityType;
            existing.CapabilityName = capability.CapabilityName;
            existing.CapabilityValue = capability.CapabilityValue;
            existing.IsAvailable = capability.IsAvailable;
            existing.Priority = capability.Priority;
            existing.MinValue = capability.MinValue;
            existing.MaxValue = capability.MaxValue;
            existing.Unit = capability.Unit;
            existing.Notes = capability.Notes;
            existing.RequiredCertification = capability.RequiredCertification;
            existing.LastModifiedDate = DateTime.UtcNow;
            existing.LastModifiedBy = capability.LastModifiedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Machine capability {CapabilityId} updated for machine {MachineId}", 
                capability.Id, capability.SlsMachineId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating machine capability {CapabilityId}", capability.Id);
            return false;
        }
    }

    public async Task<bool> RemoveMachineCapabilityAsync(int capabilityId)
    {
        try
        {
            var capability = await _context.MachineCapabilities.FindAsync(capabilityId);
            if (capability == null)
                return false;

            // Soft delete by marking as unavailable
            capability.IsAvailable = false;
            capability.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Machine capability {CapabilityId} deleted from machine {MachineId}", 
                capabilityId, capability.SlsMachineId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting machine capability {CapabilityId}", capabilityId);
            return false;
        }
    }

    #endregion

    #region Machine Validation and Compatibility

    public async Task<bool> CanMachineProcessPartAsync(string machineId, int partId)
    {
        try
        {
            var machine = await GetMachineByMachineIdAsync(machineId);
            var part = await _context.Parts.FindAsync(partId);

            if (machine == null || part == null)
                return false;

            return machine.CanAccommodatePart(part);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking compatibility for machine {MachineId} and part {PartId}", 
                machineId, partId);
            return false;
        }
    }

    public async Task<List<SlsMachine>> GetCompatibleMachinesForPartAsync(int partId)
    {
        try
        {
            var part = await _context.Parts.FindAsync(partId);
            if (part == null)
                return new List<SlsMachine>();

            var machines = await GetActiveMachinesAsync();
            return machines.Where(m => m.CanAccommodatePart(part)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding compatible machines for part {PartId}", partId);
            return new List<SlsMachine>();
        }
    }

    public async Task<bool> IsMachineIdUniqueAsync(string machineId, int? excludeId = null)
    {
        try
        {
            var query = _context.SlsMachines.Where(m => m.MachineId.ToLower() == machineId.ToLower());
            
            if (excludeId.HasValue)
                query = query.Where(m => m.Id != excludeId.Value);

            return !await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking machine ID uniqueness for {MachineId}", machineId);
            return false;
        }
    }

    #endregion

    #region Machine Statistics and Utilization

    public async Task<Dictionary<string, object>> GetMachineStatisticsAsync()
    {
        try
        {
            var totalMachines = await _context.SlsMachines.CountAsync();
            var activeMachines = await _context.SlsMachines.CountAsync(m => m.IsActive);
            var availableMachines = await _context.SlsMachines.CountAsync(m => m.IsAvailableForScheduling);
            var buildingMachines = await _context.SlsMachines.CountAsync(m => m.Status == "Building");
            var maintenanceMachines = await _context.SlsMachines.CountAsync(m => m.Status == "Maintenance");
            var offlineMachines = await _context.SlsMachines.CountAsync(m => m.Status == "Offline");

            var avgUtilization = await _context.SlsMachines
                .Where(m => m.IsActive)
                .AverageAsync(m => (double?)m.AverageUtilizationPercent) ?? 0;

            var statusBreakdown = await _context.SlsMachines
                .GroupBy(m => m.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            return new Dictionary<string, object>
            {
                ["TotalMachines"] = totalMachines,
                ["ActiveMachines"] = activeMachines,
                ["AvailableMachines"] = availableMachines,
                ["BuildingMachines"] = buildingMachines,
                ["MaintenanceMachines"] = maintenanceMachines,
                ["OfflineMachines"] = offlineMachines,
                ["AverageUtilization"] = Math.Round(avgUtilization, 1),
                ["StatusBreakdown"] = statusBreakdown
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating machine statistics");
            return new Dictionary<string, object>();
        }
    }

    public async Task<double> GetMachineUtilizationAsync(string machineId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var machine = await GetMachineByMachineIdAsync(machineId);
            if (machine == null)
                return 0;

            return machine.CalculateUtilizationPercent(fromDate, toDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating utilization for machine {MachineId}", machineId);
            return 0;
        }
    }

    public async Task<List<SlsMachine>> GetMachinesRequiringMaintenanceAsync()
    {
        try
        {
            return await _context.SlsMachines
                .Where(m => m.IsActive && m.HoursSinceLastMaintenance >= m.MaintenanceIntervalHours)
                .OrderBy(m => m.Priority)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machines requiring maintenance");
            return new List<SlsMachine>();
        }
    }

    #endregion
}