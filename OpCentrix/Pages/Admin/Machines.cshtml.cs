using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using OpCentrix.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.Admin;

/// <summary>
/// Admin page for managing SLS machines
/// Task 6: Machine Status and Dynamic Machine Management
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class MachinesModel : PageModel
{
    private readonly SchedulerContext _context;
    private readonly ILogger<MachinesModel> _logger;

    public MachinesModel(SchedulerContext context, ILogger<MachinesModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Properties for the page
    public List<Machine> Machines { get; set; } = new();
    public List<MachineCapability> MachineCapabilities { get; set; } = new();
    public Dictionary<string, int> StatusStatistics { get; set; } = new();
    public int TotalMachines { get; set; }
    public int ActiveMachines { get; set; }
    public int OfflineMachines { get; set; }
    public double AverageUtilization { get; set; }

    // Filtering and search
    [BindProperty(SupportsGet = true)]
    public string SearchTerm { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string StatusFilter { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string MaterialFilter { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "MachineId";

    [BindProperty(SupportsGet = true)]
    public string SortDirection { get; set; } = "asc";

    // Machine creation/editing
    [BindProperty]
    public MachineCreateEditModel MachineInput { get; set; } = new();

    [BindProperty]
    public int? EditingMachineId { get; set; }

    // Machine capabilities management
    [BindProperty]
    public MachineCapabilityModel CapabilityInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Loading machine management page - Admin: {Admin}", User.Identity?.Name);

            await LoadMachinesAsync();
            await LoadMachineCapabilitiesAsync();
            await LoadStatisticsAsync();

            _logger.LogInformation("Machine management page loaded - {MachineCount} machines found", Machines.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading machine management page");
            TempData["Error"] = "Error loading machine data. Please try again.";
            
            // Initialize with empty data on error
            Machines = new List<Machine>();
            MachineCapabilities = new List<MachineCapability>();
            StatusStatistics = new Dictionary<string, int>();
        }
    }

    public async Task<IActionResult> OnPostCreateMachineAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadMachinesAsync();
                await LoadMachineCapabilitiesAsync();
                await LoadStatisticsAsync();
                TempData["Error"] = "Please correct the validation errors and try again.";
                return Page();
            }

            // Check if machine ID already exists
            var existingMachine = await _context.Machines
                .FirstOrDefaultAsync(m => m.MachineId.ToLower() == MachineInput.MachineId.ToLower());

            if (existingMachine != null)
            {
                ModelState.AddModelError("MachineInput.MachineId", "Machine ID already exists.");
                await LoadMachinesAsync();
                await LoadMachineCapabilitiesAsync();
                await LoadStatisticsAsync();
                TempData["Error"] = "Machine ID already exists. Please choose a different ID.";
                return Page();
            }

            var newMachine = CreateMachineFromForm();

            _context.Machines.Add(newMachine);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Machine '{MachineInput.MachineId}' created successfully.";
            _logger.LogInformation("Admin {Admin} created new machine: {MachineId} ({MachineName})", 
                User.Identity?.Name, MachineInput.MachineId, MachineInput.MachineName);
            
            // Clear the form
            MachineInput = new MachineCreateEditModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating machine {MachineId}", MachineInput.MachineId);
            TempData["Error"] = "An error occurred while creating the machine.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditMachineAsync()
    {
        try
        {
            if (!EditingMachineId.HasValue)
            {
                TempData["Error"] = "Invalid machine ID.";
                return RedirectToPage();
            }

            if (!ModelState.IsValid)
            {
                await LoadMachinesAsync();
                await LoadMachineCapabilitiesAsync();
                await LoadStatisticsAsync();
                TempData["Error"] = "Please correct the validation errors and try again.";
                return Page();
            }

            var machine = await _context.Machines.FindAsync(EditingMachineId.Value);
            if (machine == null)
            {
                TempData["Error"] = "Machine not found.";
                return RedirectToPage();
            }

            // Check if machine ID is being changed and if it conflicts
            if (machine.MachineId.ToLower() != MachineInput.MachineId.ToLower())
            {
                var existingMachine = await _context.Machines
                    .FirstOrDefaultAsync(m => m.MachineId.ToLower() == MachineInput.MachineId.ToLower() && m.Id != machine.Id);

                if (existingMachine != null)
                {
                    ModelState.AddModelError("MachineInput.MachineId", "Machine ID already exists.");
                    await LoadMachinesAsync();
                    await LoadMachineCapabilitiesAsync();
                    await LoadStatisticsAsync();
                    TempData["Error"] = "Machine ID already exists. Please choose a different ID.";
                    return Page();
                }
            }

            // Update machine properties
            machine.MachineId = MachineInput.MachineId;
            machine.Name = MachineInput.MachineName;
            machine.MachineType = MachineInput.MachineModel;
            machine.SerialNumber = MachineInput.SerialNumber;
            machine.Location = MachineInput.Location;
            machine.SupportedMaterials = MachineInput.SupportedMaterials;
            machine.CurrentMaterial = MachineInput.CurrentMaterial;
            machine.Status = MachineInput.Status;
            machine.IsActive = MachineInput.IsActive;
            machine.IsAvailableForScheduling = MachineInput.IsAvailableForScheduling;
            machine.Priority = MachineInput.Priority;
            machine.BuildLengthMm = MachineInput.BuildLengthMm;
            machine.BuildWidthMm = MachineInput.BuildWidthMm;
            machine.BuildHeightMm = MachineInput.BuildHeightMm;
            machine.MaxLaserPowerWatts = MachineInput.MaxLaserPowerWatts;
            machine.MaxScanSpeedMmPerSec = MachineInput.MaxScanSpeedMmPerSec;
            machine.MinLayerThicknessMicrons = MachineInput.MinLayerThicknessMicrons;
            machine.MaxLayerThicknessMicrons = MachineInput.MaxLayerThicknessMicrons;
            machine.MaintenanceIntervalHours = MachineInput.MaintenanceIntervalHours;
            machine.OpcUaEndpointUrl = MachineInput.OpcUaEndpointUrl;
            machine.OpcUaEnabled = MachineInput.OpcUaEnabled;
            machine.LastModifiedBy = User.Identity?.Name ?? "Admin";
            machine.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Machine '{machine.MachineId}' updated successfully.";
            _logger.LogInformation("Admin {Admin} updated machine: {MachineId} ({MachineName})", 
                User.Identity?.Name, machine.MachineId, machine.MachineName);

            // Clear editing state
            EditingMachineId = null;
            MachineInput = new MachineCreateEditModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating machine ID {MachineId}", EditingMachineId);
            TempData["Error"] = "An error occurred while updating the machine.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleMachineStatusAsync(int machineId)
    {
        try
        {
            var machine = await _context.Machines.FindAsync(machineId);
            if (machine == null)
            {
                TempData["Error"] = "Machine not found.";
                return RedirectToPage();
            }

            machine.IsActive = !machine.IsActive;
            machine.LastModifiedBy = User.Identity?.Name ?? "Admin";
            machine.LastModifiedDate = DateTime.UtcNow;

            // Update status based on active state
            if (!machine.IsActive)
            {
                machine.Status = "Offline";
                machine.IsAvailableForScheduling = false;
            }
            else
            {
                machine.Status = "Idle";
                machine.IsAvailableForScheduling = true;
            }

            await _context.SaveChangesAsync();

            var action = machine.IsActive ? "activated" : "deactivated";
            TempData["Success"] = $"Machine '{machine.MachineId}' {action} successfully.";
            _logger.LogInformation("Admin {Admin} {Action} machine: {MachineId}", 
                User.Identity?.Name, action, machine.MachineId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for machine ID {MachineId}", machineId);
            TempData["Error"] = "An error occurred while updating machine status.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteMachineAsync(int machineId)
    {
        try
        {
            var machine = await _context.Machines
                .Include(m => m.CurrentJob)
                .FirstOrDefaultAsync(m => m.Id == machineId);

            if (machine == null)
            {
                TempData["Error"] = "Machine not found.";
                return RedirectToPage();
            }

            // Check if machine has active jobs
            var hasActiveJobs = await _context.Jobs
                .AnyAsync(j => j.MachineId == machine.MachineId && 
                              j.Status != "Completed" && j.Status != "Cancelled");

            if (hasActiveJobs)
            {
                TempData["Error"] = $"Cannot delete machine '{machine.MachineId}' because it has active jobs.";
                return RedirectToPage();
            }

            // Remove related machine capabilities - FIXED: Use MachineId instead of MachineId
            var capabilities = await _context.MachineCapabilities
                .Where(mc => mc.MachineId == machine.Id) // FIXED: Use machine.Id (int) not machine.MachineId (string)
                .ToListAsync();

            if (capabilities.Any())
            {
                _context.MachineCapabilities.RemoveRange(capabilities);
            }

            _context.Machines.Remove(machine);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Machine '{machine.MachineId}' deleted successfully.";
            _logger.LogWarning("Admin {Admin} deleted machine: {MachineId} ({MachineName})", 
                User.Identity?.Name, machine.MachineId, machine.MachineName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting machine ID {MachineId}", machineId);
            TempData["Error"] = "An error occurred while deleting the machine.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddCapabilityAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadMachinesAsync();
                await LoadMachineCapabilitiesAsync();
                await LoadStatisticsAsync();
                TempData["Error"] = "Please correct the validation errors and try again.";
                return Page();
            }

            // Find the machine by MachineId string and get its integer Id
            var machine = await _context.Machines
                .FirstOrDefaultAsync(m => m.MachineId == CapabilityInput.MachineId);

            if (machine == null)
            {
                TempData["Error"] = "Machine not found.";
                return RedirectToPage();
            }

            var capability = new MachineCapability
            {
                MachineId = machine.Id, // FIXED: Use machine.Id (int) not CapabilityInput.MachineId (string)
                CapabilityType = CapabilityInput.CapabilityType,
                CapabilityName = CapabilityInput.CapabilityName,
                CapabilityValue = CapabilityInput.Description, // FIXED: Use Description instead of non-existent CapabilityValue
                IsAvailable = !CapabilityInput.IsRequired, // FIXED: Use logical mapping - available if not required
                Priority = 1, // FIXED: Use default priority since Priority doesn't exist in input model
                MinValue = CapabilityInput.MinValue,
                MaxValue = CapabilityInput.MaxValue,
                Unit = CapabilityInput.Unit,
                Notes = CapabilityInput.Description, // FIXED: Use Description instead of non-existent Notes
                RequiredCertification = "", // FIXED: Set empty string since RequiredCertification doesn't exist in input model
                CreatedBy = User.Identity?.Name ?? "Admin",
                LastModifiedBy = User.Identity?.Name ?? "Admin",
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            _context.MachineCapabilities.Add(capability);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Capability '{CapabilityInput.CapabilityName}' added successfully.";
            _logger.LogInformation("Admin {Admin} added capability: {CapabilityName} to machine: {MachineId}", 
                User.Identity?.Name, CapabilityInput.CapabilityName, CapabilityInput.MachineId);

            // Clear the form
            CapabilityInput = new MachineCapabilityModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding capability {CapabilityName}", CapabilityInput.CapabilityName);
            TempData["Error"] = "An error occurred while adding the capability.";
        }

        return RedirectToPage();
    }

    // Helper methods
    private async Task LoadMachinesAsync()
    {
        var query = _context.Machines.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(SearchTerm))
        {
            query = query.Where(m => 
                m.MachineId.Contains(SearchTerm) ||
                m.Name.Contains(SearchTerm) ||
                m.MachineType.Contains(SearchTerm) ||
                m.Location.Contains(SearchTerm) ||
                m.SerialNumber.Contains(SearchTerm));
        }

        // Apply status filter
        if (!string.IsNullOrEmpty(StatusFilter))
        {
            query = query.Where(m => m.Status == StatusFilter);
        }

        // Apply material filter
        if (!string.IsNullOrEmpty(MaterialFilter))
        {
            query = query.Where(m => m.SupportedMaterials.Contains(MaterialFilter));
        }

        // Apply sorting
        query = SortDirection.ToLower() == "desc"
            ? SortBy switch
            {
                "Name" => query.OrderByDescending(m => m.Name),
                "MachineType" => query.OrderByDescending(m => m.MachineType),
                "Location" => query.OrderByDescending(m => m.Location),
                "Status" => query.OrderByDescending(m => m.Status),
                "Priority" => query.OrderByDescending(m => m.Priority),
                "CreatedDate" => query.OrderByDescending(m => m.CreatedDate),
                "IsActive" => query.OrderByDescending(m => m.IsActive),
                _ => query.OrderByDescending(m => m.MachineId)
            }
            : SortBy switch
            {
                "Name" => query.OrderBy(m => m.Name),
                "MachineType" => query.OrderBy(m => m.MachineType),
                "Location" => query.OrderBy(m => m.Location),
                "Status" => query.OrderBy(m => m.Status),
                "Priority" => query.OrderBy(m => m.Priority),
                "CreatedDate" => query.OrderBy(m => m.CreatedDate),
                "IsActive" => query.OrderBy(m => m.IsActive),
                _ => query.OrderBy(m => m.MachineId)
            };

        Machines = await query.ToListAsync();
    }

    private async Task LoadMachineCapabilitiesAsync()
    {
        MachineCapabilities = await _context.MachineCapabilities
            .Where(mc => mc.IsAvailable)
            .Include(mc => mc.Machine)
            .OrderBy(mc => mc.Machine.MachineId)
            .ThenBy(mc => mc.CapabilityName)
            .ToListAsync();
    }

    private async Task LoadStatisticsAsync()
    {
        TotalMachines = await _context.Machines.CountAsync();
        ActiveMachines = await _context.Machines.CountAsync(m => m.IsActive);
        OfflineMachines = await _context.Machines.CountAsync(m => m.Status == "Offline");

        StatusStatistics = await _context.Machines
            .GroupBy(m => m.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        AverageUtilization = await _context.Machines
            .Where(m => m.IsActive)
            .AverageAsync(m => (double?)m.AverageUtilizationPercent) ?? 0;
    }

    public void LoadMachineForEditing(Machine machine)
    {
        EditingMachineId = machine.Id;
        MachineInput = new MachineCreateEditModel
        {
            MachineId = machine.MachineId,
            MachineName = machine.Name,
            MachineModel = machine.MachineType,
            SerialNumber = machine.SerialNumber,
            Location = machine.Location,
            SupportedMaterials = machine.SupportedMaterials,
            CurrentMaterial = machine.CurrentMaterial,
            Status = machine.Status,
            IsActive = machine.IsActive,
            IsAvailableForScheduling = machine.IsAvailableForScheduling,
            Priority = machine.Priority,
            BuildLengthMm = machine.BuildLengthMm,
            BuildWidthMm = machine.BuildWidthMm,
            BuildHeightMm = machine.BuildHeightMm,
            MaxLaserPowerWatts = machine.MaxLaserPowerWatts,
            MaxScanSpeedMmPerSec = machine.MaxScanSpeedMmPerSec,
            MinLayerThicknessMicrons = machine.MinLayerThicknessMicrons,
            MaxLayerThicknessMicrons = machine.MaxLayerThicknessMicrons,
            MaintenanceIntervalHours = machine.MaintenanceIntervalHours,
            OpcUaEndpointUrl = machine.OpcUaEndpointUrl,
            OpcUaEnabled = machine.OpcUaEnabled
        };
    }

    public string GetNextSortDirection(string column)
    {
        if (SortBy == column)
            return SortDirection == "asc" ? "desc" : "asc";
        return "asc";
    }

    public string GetSortIcon(string column)
    {
        if (SortBy != column) return "↕";
        return SortDirection == "asc" ? "▲" : "▼";
    }

        private Machine CreateMachineFromForm()
        {
            return new Machine
            {
                MachineId = MachineInput.MachineId,
                Name = MachineInput.MachineName,
                MachineName = MachineInput.MachineName,
                MachineType = MachineInput.MachineModel,
                SerialNumber = MachineInput.SerialNumber,
                Location = MachineInput.Location,
                Status = MachineInput.Status,
                IsActive = MachineInput.IsActive,
                IsAvailableForScheduling = MachineInput.IsAvailableForScheduling,
                BuildLengthMm = MachineInput.BuildLengthMm,
                BuildWidthMm = MachineInput.BuildWidthMm,
                BuildHeightMm = MachineInput.BuildHeightMm,
                SupportedMaterials = MachineInput.SupportedMaterials,
                CurrentMaterial = MachineInput.CurrentMaterial,
                MaxLaserPowerWatts = MachineInput.MaxLaserPowerWatts,
                MaxScanSpeedMmPerSec = MachineInput.MaxScanSpeedMmPerSec,
                MinLayerThicknessMicrons = MachineInput.MinLayerThicknessMicrons,
                MaxLayerThicknessMicrons = MachineInput.MaxLayerThicknessMicrons,
                MaintenanceIntervalHours = MachineInput.MaintenanceIntervalHours,
                OpcUaEndpointUrl = MachineInput.OpcUaEndpointUrl,
                OpcUaEnabled = MachineInput.OpcUaEnabled,
                Priority = MachineInput.Priority,
                CreatedBy = User.Identity?.Name ?? "Admin",
                LastModifiedBy = User.Identity?.Name ?? "Admin"
            };
        }
}

/// <summary>
/// Model for creating and editing machines
/// </summary>
public class MachineCreateEditModel
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    [Display(Name = "Machine ID")]
    public string MachineId { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    [Display(Name = "Machine Name")]
    public string MachineName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Machine Model")]
    public string MachineModel { get; set; } = "TruPrint 3000";

    [StringLength(50)]
    [Display(Name = "Serial Number")]
    public string SerialNumber { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Location")]
    public string Location { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    [Display(Name = "Supported Materials")]
    public string SupportedMaterials { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Current Material")]
    public string CurrentMaterial { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Idle";

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Available for Scheduling")]
    public bool IsAvailableForScheduling { get; set; } = true;

    [Range(1, 10)]
    [Display(Name = "Priority")]
    public int Priority { get; set; } = 3;

    [Range(50, 500)]
    [Display(Name = "Build Length (mm)")]
    public double BuildLengthMm { get; set; } = 250;

    [Range(50, 500)]
    [Display(Name = "Build Width (mm)")]
    public double BuildWidthMm { get; set; } = 250;

    [Range(50, 500)]
    [Display(Name = "Build Height (mm)")]
    public double BuildHeightMm { get; set; } = 300;

    [Range(100, 1000)]
    [Display(Name = "Max Laser Power (Watts)")]
    public double MaxLaserPowerWatts { get; set; } = 400;

    [Range(1000, 10000)]
    [Display(Name = "Max Scan Speed (mm/s)")]
    public double MaxScanSpeedMmPerSec { get; set; } = 7000;

    [Range(10, 100)]
    [Display(Name = "Min Layer Thickness (µm)")]
    public double MinLayerThicknessMicrons { get; set; } = 20;

    [Range(10, 100)]
    [Display(Name = "Max Layer Thickness (µm)")]
    public double MaxLayerThicknessMicrons { get; set; } = 60;

    [Range(100, 2000)]
    [Display(Name = "Maintenance Interval (hours)")]
    public double MaintenanceIntervalHours { get; set; } = 500;

    [StringLength(200)]
    [Display(Name = "OPC UA Endpoint URL")]
    public string OpcUaEndpointUrl { get; set; } = string.Empty;

    [Display(Name = "OPC UA Enabled")]
    public bool OpcUaEnabled { get; set; } = false;
}

/// <summary>
/// Model for managing machine capabilities
/// </summary>
public class MachineCapabilityModel
{
    [Required]
    [StringLength(50)]
    [Display(Name = "Machine ID")]
    public string MachineId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Capability Name")]
    public string CapabilityName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Capability Type")]
    public string CapabilityType { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Display(Name = "Value Type")]
    public string ValueType { get; set; } = "Number";

    [Display(Name = "Minimum Value")]
    public double? MinValue { get; set; }

    [Display(Name = "Maximum Value")]
    public double? MaxValue { get; set; }

    [Display(Name = "Default Value")]
    public double? DefaultValue { get; set; }

    [StringLength(20)]
    [Display(Name = "Unit")]
    public string Unit { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Required")]
    public bool IsRequired { get; set; } = false;
}