using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for managing machines and their capabilities (ALL machine types)
/// Task 6: Machine Status and Dynamic Machine Management - UPDATED
/// </summary>
public interface IMachineManagementService
{
    // Machine CRUD operations
    Task<List<Machine>> GetAllMachinesAsync();
    Task<List<Machine>> GetActiveMachinesAsync();
    Task<List<Machine>> GetMachinesByTypeAsync(string machineType);
    Task<Machine?> GetMachineByIdAsync(int id);
    Task<Machine?> GetMachineByMachineIdAsync(string machineId);
    Task<bool> CreateMachineAsync(Machine machine);
    Task<bool> UpdateMachineAsync(Machine machine);
    Task<bool> DeleteMachineAsync(int id);

    // Machine status operations
    Task<bool> UpdateMachineStatusAsync(string machineId, string status);
    Task<bool> SetMachineAvailabilityAsync(string machineId, bool isAvailable);
    Task<Dictionary<string, string>> GetMachineStatusesAsync();

    // Machine capability operations
    Task<List<MachineCapability>> GetMachineCapabilitiesAsync(int machineId);
    Task<bool> AddMachineCapabilityAsync(MachineCapability capability);
    Task<bool> UpdateMachineCapabilityAsync(MachineCapability capability);
    Task<bool> RemoveMachineCapabilityAsync(int capabilityId);

    // Machine validation and compatibility
    Task<bool> CanMachineProcessPartAsync(string machineId, int partId);
    Task<List<Machine>> GetCompatibleMachinesForPartAsync(int partId);
    Task<bool> IsMachineIdUniqueAsync(string machineId, int? excludeId = null);

    // Machine statistics and utilization
    Task<Dictionary<string, object>> GetMachineStatisticsAsync();
    Task<double> GetMachineUtilizationAsync(string machineId, DateTime fromDate, DateTime toDate);
    Task<List<Machine>> GetMachinesRequiringMaintenanceAsync();

    // Machine type operations
    Task<List<string>> GetSupportedMachineTypesAsync();
    Task<List<string>> GetAvailableMachineIdsAsync(string? machineType = null);
}

/// <summary>
/// Implementation of machine management service
/// UPDATED: Now works with generic Machine model instead of SlsMachine
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

    public async Task<List<Machine>> GetAllMachinesAsync()
    {
        try
        {
            return await _context.Machines
                .Include(m => m.Capabilities)
                .OrderBy(m => m.MachineType)
                .ThenBy(m => m.MachineId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all machines");
            return new List<Machine>();
        }
    }

    public async Task<List<Machine>> GetActiveMachinesAsync()
    {
        try
        {
            return await _context.Machines
                .Include(m => m.Capabilities)
                .Where(m => m.IsActive && m.IsAvailableForScheduling)
                .OrderBy(m => m.Priority)
                .ThenBy(m => m.MachineType)
                .ThenBy(m => m.MachineId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active machines");
            return new List<Machine>();
        }
    }

    public async Task<List<Machine>> GetMachinesByTypeAsync(string machineType)
    {
        try
        {
            return await _context.Machines
                .Include(m => m.Capabilities)
                .Where(m => m.MachineType == machineType)
                .OrderBy(m => m.Priority)
                .ThenBy(m => m.MachineId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machines by type: {MachineType}", machineType);
            return new List<Machine>();
        }
    }

    public async Task<Machine?> GetMachineByIdAsync(int id)
    {
        try
        {
            return await _context.Machines
                .Include(m => m.Capabilities)
                .Include(m => m.CurrentJob)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machine by ID: {MachineId}", id);
            return null;
        }
    }

    public async Task<Machine?> GetMachineByMachineIdAsync(string machineId)
    {
        try
        {
            return await _context.Machines
                .Include(m => m.Capabilities)
                .Include(m => m.CurrentJob)
                .FirstOrDefaultAsync(m => m.MachineId == machineId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machine by machine ID: {MachineId}", machineId);
            return null;
        }
    }

    public async Task<bool> CreateMachineAsync(Machine machine)
    {
        try
        {
            // Validate machine ID uniqueness
            var existingMachine = await _context.Machines
                .FirstOrDefaultAsync(m => m.MachineId == machine.MachineId);

            if (existingMachine != null)
            {
                _logger.LogWarning("Cannot create machine - Machine ID {MachineId} already exists", machine.MachineId);
                return false;
            }

            machine.CreatedDate = DateTime.UtcNow;
            machine.LastModifiedDate = DateTime.UtcNow;
            machine.LastStatusUpdate = DateTime.UtcNow;

            _context.Machines.Add(machine);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new machine: {MachineId} - {MachineName} ({MachineType})", 
                machine.MachineId, machine.MachineName, machine.MachineType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating machine: {MachineId}", machine.MachineId);
            return false;
        }
    }

    public async Task<bool> UpdateMachineAsync(Machine machine)
    {
        try
        {
            var existingMachine = await _context.Machines.FindAsync(machine.Id);
            if (existingMachine == null)
            {
                _logger.LogWarning("Cannot update machine - Machine with ID {Id} not found", machine.Id);
                return false;
            }

            // Check if machine ID is unique (excluding current machine)
            if (existingMachine.MachineId != machine.MachineId)
            {
                var duplicateMachine = await _context.Machines
                    .FirstOrDefaultAsync(m => m.MachineId == machine.MachineId && m.Id != machine.Id);

                if (duplicateMachine != null)
                {
                    _logger.LogWarning("Cannot update machine - Machine ID {MachineId} already exists", machine.MachineId);
                    return false;
                }
            }

            // Update properties
            existingMachine.MachineId = machine.MachineId;
            existingMachine.MachineName = machine.MachineName;
            existingMachine.MachineType = machine.MachineType;
            existingMachine.MachineModel = machine.MachineModel;
            existingMachine.SerialNumber = machine.SerialNumber;
            existingMachine.Location = machine.Location;
            existingMachine.Status = machine.Status;
            existingMachine.IsActive = machine.IsActive;
            existingMachine.IsAvailableForScheduling = machine.IsAvailableForScheduling;
            existingMachine.Priority = machine.Priority;
            existingMachine.SupportedMaterials = machine.SupportedMaterials;
            existingMachine.CurrentMaterial = machine.CurrentMaterial;
            existingMachine.TechnicalSpecifications = machine.TechnicalSpecifications;
            existingMachine.MaintenanceIntervalHours = machine.MaintenanceIntervalHours;
            existingMachine.OpcUaEndpointUrl = machine.OpcUaEndpointUrl;
            existingMachine.OpcUaEnabled = machine.OpcUaEnabled;
            existingMachine.CommunicationSettings = machine.CommunicationSettings;
            existingMachine.LastModifiedDate = DateTime.UtcNow;
            existingMachine.LastModifiedBy = machine.LastModifiedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated machine: {MachineId} - {MachineName} ({MachineType})", 
                machine.MachineId, machine.MachineName, machine.MachineType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating machine: {MachineId}", machine.MachineId);
            return false;
        }
    }

    public async Task<bool> DeleteMachineAsync(int id)
    {
        try
        {
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null)
            {
                _logger.LogWarning("Cannot delete machine - Machine with ID {Id} not found", id);
                return false;
            }

            // Check if machine has associated capabilities
            var capabilityCount = await _context.MachineCapabilities
                .Where(mc => mc.MachineId == id)
                .CountAsync();

            if (capabilityCount > 0)
            {
                // Remove associated capabilities first
                var capabilities = await _context.MachineCapabilities
                    .Where(mc => mc.MachineId == id)
                    .ToListAsync();

                _context.MachineCapabilities.RemoveRange(capabilities);
            }

            // Check if machine has current jobs
            var hasCurrentJobs = await _context.Jobs
                .AnyAsync(j => j.MachineId == machine.MachineId && 
                              (j.Status == "Scheduled" || j.Status == "Building" || j.Status == "Running"));

            if (hasCurrentJobs)
            {
                _logger.LogWarning("Cannot delete machine {MachineId} - has active jobs", machine.MachineId);
                return false;
            }

            _context.Machines.Remove(machine);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted machine: {MachineId} - {MachineName} ({MachineType})", 
                machine.MachineId, machine.MachineName, machine.MachineType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting machine with ID: {Id}", id);
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
            return await _context.Machines
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

    public async Task<List<MachineCapability>> GetMachineCapabilitiesAsync(int machineId)
    {
        try
        {
            return await _context.MachineCapabilities
                .Where(c => c.MachineId == machineId && c.IsAvailable)
                .OrderBy(c => c.Priority)
                .ThenBy(c => c.CapabilityType)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machine capabilities for machine ID {MachineId}", machineId);
            return new List<MachineCapability>();
        }
    }

    public async Task<bool> AddMachineCapabilityAsync(MachineCapability capability)
    {
        try
        {
            capability.CreatedDate = DateTime.UtcNow;
            capability.LastModifiedDate = DateTime.UtcNow;

            _context.MachineCapabilities.Add(capability);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Machine capability added: MachineId {MachineId} - {CapabilityName}", 
                capability.MachineId, capability.CapabilityName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding machine capability for MachineId {MachineId}", 
                capability.MachineId);
            return false;
        }
    }

    public async Task<bool> UpdateMachineCapabilityAsync(MachineCapability capability)
    {
        try
        {
            var existing = await _context.MachineCapabilities.FindAsync(capability.Id);
            if (existing == null)
            {
                _logger.LogWarning("Cannot update capability - Capability with ID {Id} not found", capability.Id);
                return false;
            }

            // Update properties
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

            _logger.LogInformation("Updated machine capability: {CapabilityName} for MachineId {MachineId}", 
                capability.CapabilityName, capability.MachineId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating machine capability with ID: {Id}", capability.Id);
            return false;
        }
    }

    public async Task<bool> RemoveMachineCapabilityAsync(int capabilityId)
    {
        try
        {
            var capability = await _context.MachineCapabilities.FindAsync(capabilityId);
            if (capability == null)
            {
                _logger.LogWarning("Cannot remove capability - Capability with ID {Id} not found", capabilityId);
                return false;
            }

            _context.MachineCapabilities.Remove(capability);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed machine capability: {CapabilityName} for MachineId {MachineId}", 
                capability.CapabilityName, capability.MachineId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing machine capability with ID: {Id}", capabilityId);
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

            // Check basic compatibility
            return machine.CanAccommodatePart(part);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking machine-part compatibility: {MachineId} - {PartId}", machineId, partId);
            return false;
        }
    }

    public async Task<List<Machine>> GetCompatibleMachinesForPartAsync(int partId)
    {
        try
        {
            var part = await _context.Parts.FindAsync(partId);
            if (part == null)
                return new List<Machine>();

            var machines = await GetActiveMachinesAsync();
            return machines.Where(m => m.CanAccommodatePart(part)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding compatible machines for part: {PartId}", partId);
            return new List<Machine>();
        }
    }

    public async Task<bool> IsMachineIdUniqueAsync(string machineId, int? excludeId = null)
    {
        try
        {
            var query = _context.Machines.Where(m => m.MachineId == machineId);
            
            if (excludeId.HasValue)
            {
                query = query.Where(m => m.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking machine ID uniqueness: {MachineId}", machineId);
            return false;
        }
    }

    #endregion

    #region Machine Statistics and Utilization

    public async Task<Dictionary<string, object>> GetMachineStatisticsAsync()
    {
        try
        {
            var machines = await _context.Machines.ToListAsync();
            
            var stats = new Dictionary<string, object>
            {
                ["TotalMachines"] = machines.Count,
                ["ActiveMachines"] = machines.Count(m => m.IsActive),
                ["OfflineMachines"] = machines.Count(m => m.Status == "Offline"),
                ["AverageUtilization"] = machines.Any() ? machines.Average(m => m.AverageUtilizationPercent) : 0,
                ["StatusStatistics"] = machines.GroupBy(m => m.Status)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ["TypeStatistics"] = machines.GroupBy(m => m.MachineType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machine statistics");
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

            // Calculate utilization based on scheduled jobs
            var totalPeriodHours = (toDate - fromDate).TotalHours;
            if (totalPeriodHours <= 0)
                return 0;

            // FIXED: Calculate job hours using TimeSpan instead of EF.Functions.DateDiffHour
            var jobs = await _context.Jobs
                .Where(j => j.MachineId == machineId &&
                           j.ScheduledStart >= fromDate &&
                           j.ScheduledEnd <= toDate)
                .Select(j => new { j.ScheduledStart, j.ScheduledEnd })
                .ToListAsync();

            var jobHours = jobs.Sum(j => (j.ScheduledEnd - j.ScheduledStart).TotalHours);

            return Math.Round((jobHours / totalPeriodHours) * 100, 2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating machine utilization for {MachineId}", machineId);
            return 0;
        }
    }

    public async Task<List<Machine>> GetMachinesRequiringMaintenanceAsync()
    {
        try
        {
            return await _context.Machines
                .Where(m => m.IsActive && m.RequiresMaintenance)
                .OrderByDescending(m => m.HoursSinceLastMaintenance)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machines requiring maintenance");
            return new List<Machine>();
        }
    }

    #endregion

    #region Machine Type Operations

    public async Task<List<string>> GetSupportedMachineTypesAsync()
    {
        try
        {
            return await _context.Machines
                .Select(m => m.MachineType)
                .Distinct()
                .OrderBy(type => type)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported machine types");
            return new List<string> { "SLS", "EDM", "CNC", "Coating", "Assembly", "Inspection" };
        }
    }

    public async Task<List<string>> GetAvailableMachineIdsAsync(string? machineType = null)
    {
        try
        {
            var query = _context.Machines
                .Where(m => m.IsActive && m.IsAvailableForScheduling);

            if (!string.IsNullOrEmpty(machineType))
            {
                query = query.Where(m => m.MachineType == machineType);
            }

            return await query
                .Select(m => m.MachineId)
                .OrderBy(id => id)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available machine IDs for type: {MachineType}", machineType);
            return new List<string>();
        }
    }

    #endregion
}