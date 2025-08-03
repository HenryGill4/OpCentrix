using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using OpCentrix.Data;
using OpCentrix.Services.Admin;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.Admin;

/// <summary>
/// Modern machine management page using best practices
/// - Separate DTOs for different operations
/// - Clean validation without conflicts
/// - Proper separation of concerns
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class MachinesModel : PageModel
{
    private readonly SchedulerContext _context;
    private readonly IMaterialService _materialService;
    private readonly ILogger<MachinesModel> _logger;

    public MachinesModel(SchedulerContext context, IMaterialService materialService, ILogger<MachinesModel> logger)
    {
        _context = context;
        _materialService = materialService;
        _logger = logger;
    }

    // Page display properties - NO [BindProperty] to avoid validation conflicts
    public List<Machine> Machines { get; set; } = new();
    public List<MachineCapability> MachineCapabilities { get; set; } = new();
    public List<Material> AvailableMaterials { get; set; } = new();
    public List<string> MaterialTypes { get; set; } = new();
    public Dictionary<string, int> StatusStatistics { get; set; } = new();
    public int TotalMachines { get; set; }
    public int ActiveMachines { get; set; }
    public int OfflineMachines { get; set; }
    public double AverageUtilization { get; set; }

    // Search/Filter properties - Manual binding only
    public string SearchTerm { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = string.Empty;
    public string MaterialFilter { get; set; } = string.Empty;
    public string SortBy { get; set; } = "MachineId";
    public string SortDirection { get; set; } = "asc";

    // MODERN APPROACH: Separate DTOs for different operations
    [BindProperty]
    public CreateMachineDto CreateMachineRequest { get; set; } = new();

    [BindProperty]
    public EditMachineDto EditMachineRequest { get; set; } = new();

    [BindProperty]
    public int? EditingMachineId { get; set; }

    [BindProperty]
    public CreateCapabilityDto CreateCapabilityRequest { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("💻 Loading machine management page");

            // Manual parameter binding for search/filter
            SearchTerm = Request.Query["searchTerm"].FirstOrDefault() ?? string.Empty;
            StatusFilter = Request.Query["statusFilter"].FirstOrDefault() ?? string.Empty;
            MaterialFilter = Request.Query["materialFilter"].FirstOrDefault() ?? string.Empty;
            SortBy = Request.Query["sortBy"].FirstOrDefault() ?? "MachineId";
            SortDirection = Request.Query["sortDirection"].FirstOrDefault() ?? "asc";

            await LoadPageDataAsync();

            _logger.LogInformation("✅ Machine management page loaded - {MachineCount} machines", Machines.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error loading machine management page");
            TempData["Error"] = "Error loading machine data. Please try again.";
            await InitializeEmptyDataAsync();
        }
    }

    public async Task<IActionResult> OnPostCreateMachineAsync()
    {
        try
        {
            _logger.LogInformation("🔧 Creating machine: {MachineId}", CreateMachineRequest.MachineId);

            // MODERN VALIDATION: Manual validation with proper error messages
            var validationResult = await ValidateCreateRequestAsync(CreateMachineRequest);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                
                await LoadPageDataAsync();
                TempData["Error"] = "Please correct the validation errors and try again.";
                return Page();
            }

            // Create machine from DTO
            var machine = CreateMachineFromDto(CreateMachineRequest);
            
            _context.Machines.Add(machine);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Machine '{CreateMachineRequest.MachineId}' created successfully.";
            _logger.LogInformation("✅ Machine created: {MachineId}", CreateMachineRequest.MachineId);

            // Clear form
            CreateMachineRequest = new CreateMachineDto();
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating machine");
            TempData["Error"] = "An error occurred while creating the machine.";
            await LoadPageDataAsync();
            return Page();
        }
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

            _logger.LogInformation("🔧 Editing machine ID: {MachineId}", EditingMachineId.Value);

            var validationResult = await ValidateEditRequestAsync(EditMachineRequest, EditingMachineId.Value);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                
                await LoadPageDataAsync();
                TempData["Error"] = "Please correct the validation errors and try again.";
                return Page();
            }

            var machine = await _context.Machines.FindAsync(EditingMachineId.Value);
            if (machine == null)
            {
                TempData["Error"] = "Machine not found.";
                return RedirectToPage();
            }

            UpdateMachineFromDto(machine, EditMachineRequest);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Machine '{machine.MachineId}' updated successfully.";
            _logger.LogInformation("✅ Machine updated: {MachineId}", machine.MachineId);

            EditingMachineId = null;
            EditMachineRequest = new EditMachineDto();
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating machine");
            TempData["Error"] = "An error occurred while updating the machine.";
            await LoadPageDataAsync();
            return Page();
        }
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
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error toggling machine status");
            TempData["Error"] = "An error occurred while updating machine status.";
            return RedirectToPage();
        }
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

            var hasActiveJobs = await _context.Jobs
                .AnyAsync(j => j.MachineId == machine.MachineId && 
                              j.Status != "Completed" && j.Status != "Cancelled");

            if (hasActiveJobs)
            {
                TempData["Error"] = $"Cannot delete machine '{machine.MachineId}' because it has active jobs.";
                return RedirectToPage();
            }

            var capabilities = await _context.MachineCapabilities
                .Where(mc => mc.MachineId == machine.Id)
                .ToListAsync();

            if (capabilities.Any())
            {
                _context.MachineCapabilities.RemoveRange(capabilities);
            }

            _context.Machines.Remove(machine);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Machine '{machine.MachineId}' deleted successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error deleting machine");
            TempData["Error"] = "An error occurred while deleting the machine.";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostAddCapabilityAsync()
    {
        try
        {
            var validationResult = ValidateCapabilityRequest(CreateCapabilityRequest);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                
                await LoadPageDataAsync();
                TempData["Error"] = "Please correct the validation errors and try again.";
                return Page();
            }

            var machine = await _context.Machines
                .FirstOrDefaultAsync(m => m.MachineId == CreateCapabilityRequest.MachineId);

            if (machine == null)
            {
                TempData["Error"] = "Machine not found.";
                return RedirectToPage();
            }

            var capability = new MachineCapability
            {
                MachineId = machine.Id,
                CapabilityType = CreateCapabilityRequest.CapabilityType,
                CapabilityName = CreateCapabilityRequest.CapabilityName,
                CapabilityValue = CreateCapabilityRequest.Description,
                IsAvailable = !CreateCapabilityRequest.IsRequired,
                Priority = 1,
                MinValue = CreateCapabilityRequest.MinValue,
                MaxValue = CreateCapabilityRequest.MaxValue,
                Unit = CreateCapabilityRequest.Unit,
                Notes = CreateCapabilityRequest.Description,
                RequiredCertification = "",
                CreatedBy = User.Identity?.Name ?? "Admin",
                LastModifiedBy = User.Identity?.Name ?? "Admin",
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            _context.MachineCapabilities.Add(capability);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Capability '{CreateCapabilityRequest.CapabilityName}' added successfully.";
            CreateCapabilityRequest = new CreateCapabilityDto();
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error adding capability");
            TempData["Error"] = "An error occurred while adding the capability.";
            await LoadPageDataAsync();
            return Page();
        }
    }

    // Helper methods
    private async Task LoadPageDataAsync()
    {
        await LoadMachinesAsync();
        await LoadMachineCapabilitiesAsync();
        await LoadMaterialsAsync();
        await LoadStatisticsAsync();
    }

    private async Task InitializeEmptyDataAsync()
    {
        Machines = new List<Machine>();
        MachineCapabilities = new List<MachineCapability>();
        AvailableMaterials = new List<Material>();
        MaterialTypes = new List<string>();
        StatusStatistics = new Dictionary<string, int>();
    }

    private async Task LoadMachinesAsync()
    {
        var query = _context.Machines.AsQueryable();

        if (!string.IsNullOrEmpty(SearchTerm))
        {
            query = query.Where(m => 
                m.MachineId.Contains(SearchTerm) ||
                m.Name.Contains(SearchTerm) ||
                m.MachineType.Contains(SearchTerm) ||
                m.Location.Contains(SearchTerm) ||
                m.SerialNumber.Contains(SearchTerm));
        }

        if (!string.IsNullOrEmpty(StatusFilter))
        {
            query = query.Where(m => m.Status == StatusFilter);
        }

        if (!string.IsNullOrEmpty(MaterialFilter))
        {
            query = query.Where(m => m.SupportedMaterials.Contains(MaterialFilter) || 
                                   m.CurrentMaterial.Contains(MaterialFilter));
        }

        query = SortDirection.ToLower() == "desc"
            ? SortBy switch
            {
                "Name" => query.OrderByDescending(m => m.Name),
                "MachineType" => query.OrderByDescending(m => m.MachineType),
                "Location" => query.OrderByDescending(m => m.Location),
                "Status" => query.OrderByDescending(m => m.Status),
                "Priority" => query.OrderByDescending(m => m.Priority),
                _ => query.OrderByDescending(m => m.MachineId)
            }
            : SortBy switch
            {
                "Name" => query.OrderBy(m => m.Name),
                "MachineType" => query.OrderBy(m => m.MachineType),
                "Location" => query.OrderBy(m => m.Location),
                "Status" => query.OrderBy(m => m.Status),
                "Priority" => query.OrderBy(m => m.Priority),
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

    private async Task LoadMaterialsAsync()
    {
        try
        {
            AvailableMaterials = await _materialService.GetActiveMaterialsAsync();
            MaterialTypes = await _materialService.GetMaterialTypesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error loading materials");
            AvailableMaterials = new List<Material>();
            MaterialTypes = new List<string>();
        }
    }

    private async Task LoadStatisticsAsync()
    {
        TotalMachines = await _context.Machines.CountAsync();
        ActiveMachines = await _context.Machines.CountAsync(m => m.IsActive);
        OfflineMachines = await _context.Machines.CountAsync(m => m.Status == "Offline");

        var machines = await _context.Machines.Select(m => m.Status).ToListAsync();
        StatusStatistics = machines
            .Where(status => !string.IsNullOrEmpty(status))
            .GroupBy(status => status)
            .ToDictionary(g => g.Key, g => g.Count());

        var utilizationValues = await _context.Machines
            .Where(m => m.IsActive)
            .Select(m => (double?)m.AverageUtilizationPercent)
            .ToListAsync();
            
        AverageUtilization = utilizationValues.Any() ? utilizationValues.Average() ?? 0 : 0;
    }

    private async Task<ValidationResult> ValidateCreateRequestAsync(CreateMachineDto request)
    {
        var result = new ValidationResult();

        // Required field validation
        if (string.IsNullOrWhiteSpace(request.MachineId))
            result.AddError(nameof(request.MachineId), "Machine ID is required.");
        
        if (string.IsNullOrWhiteSpace(request.MachineName))
            result.AddError(nameof(request.MachineName), "Machine Name is required.");
        
        if (string.IsNullOrWhiteSpace(request.MachineModel))
            result.AddError(nameof(request.MachineModel), "Machine Model is required.");
        
        if (string.IsNullOrWhiteSpace(request.SupportedMaterials))
            result.AddError(nameof(request.SupportedMaterials), "Supported Materials is required.");
        
        if (string.IsNullOrWhiteSpace(request.Status))
            result.AddError(nameof(request.Status), "Status is required.");

        // Business logic validation
        if (!string.IsNullOrWhiteSpace(request.MachineId))
        {
            var existingMachine = await _context.Machines
                .FirstOrDefaultAsync(m => m.MachineId.ToLower() == request.MachineId.ToLower());
            
            if (existingMachine != null)
                result.AddError(nameof(request.MachineId), $"Machine ID '{request.MachineId}' already exists.");
        }

        return result;
    }

    private async Task<ValidationResult> ValidateEditRequestAsync(EditMachineDto request, int machineId)
    {
        var result = new ValidationResult();

        // Required field validation
        if (string.IsNullOrWhiteSpace(request.MachineId))
            result.AddError(nameof(request.MachineId), "Machine ID is required.");
        
        if (string.IsNullOrWhiteSpace(request.MachineName))
            result.AddError(nameof(request.MachineName), "Machine Name is required.");

        // Business logic validation
        if (!string.IsNullOrWhiteSpace(request.MachineId))
        {
            var existingMachine = await _context.Machines
                .FirstOrDefaultAsync(m => m.MachineId.ToLower() == request.MachineId.ToLower() && m.Id != machineId);
            
            if (existingMachine != null)
                result.AddError(nameof(request.MachineId), $"Machine ID '{request.MachineId}' already exists.");
        }

        return result;
    }

    private ValidationResult ValidateCapabilityRequest(CreateCapabilityDto request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.MachineId))
            result.AddError(nameof(request.MachineId), "Machine ID is required.");
        
        if (string.IsNullOrWhiteSpace(request.CapabilityName))
            result.AddError(nameof(request.CapabilityName), "Capability Name is required.");
        
        if (string.IsNullOrWhiteSpace(request.CapabilityType))
            result.AddError(nameof(request.CapabilityType), "Capability Type is required.");

        return result;
    }

    private Machine CreateMachineFromDto(CreateMachineDto dto)
    {
        return new Machine
        {
            MachineId = dto.MachineId,
            Name = dto.MachineName,
            MachineName = dto.MachineName,
            MachineType = dto.MachineModel?.Contains("TruPrint") == true ? "SLS" : "SLS",
            MachineModel = dto.MachineModel,
            SerialNumber = dto.SerialNumber ?? string.Empty,
            Location = dto.Location ?? string.Empty,
            Department = "Manufacturing",
            Status = dto.Status,
            IsActive = dto.IsActive,
            IsAvailableForScheduling = dto.IsAvailableForScheduling,
            Priority = dto.Priority,
            LastStatusUpdate = DateTime.UtcNow,
            TechnicalSpecifications = "{}",
            SupportedMaterials = dto.SupportedMaterials,
            CurrentMaterial = dto.CurrentMaterial ?? string.Empty,
            MaintenanceIntervalHours = dto.MaintenanceIntervalHours,
            HoursSinceLastMaintenance = 0,
            AverageUtilizationPercent = 0,
            MaintenanceNotes = string.Empty,
            OperatorNotes = string.Empty,
            OpcUaEndpointUrl = dto.OpcUaEndpointUrl ?? string.Empty,
            OpcUaEnabled = dto.OpcUaEnabled,
            CommunicationSettings = "{}",
            BuildLengthMm = dto.BuildLengthMm,
            BuildWidthMm = dto.BuildWidthMm,
            BuildHeightMm = dto.BuildHeightMm,
            MaxLaserPowerWatts = dto.MaxLaserPowerWatts,
            MaxScanSpeedMmPerSec = dto.MaxScanSpeedMmPerSec,
            MinLayerThicknessMicrons = dto.MinLayerThicknessMicrons,
            MaxLayerThicknessMicrons = dto.MaxLayerThicknessMicrons,
            TotalOperatingHours = 0,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            CreatedBy = User.Identity?.Name ?? "Admin",
            LastModifiedBy = User.Identity?.Name ?? "Admin"
        };
    }

    private void UpdateMachineFromDto(Machine machine, EditMachineDto dto)
    {
        machine.MachineId = dto.MachineId;
        machine.Name = dto.MachineName;
        machine.MachineName = dto.MachineName;
        machine.MachineModel = dto.MachineModel;
        machine.SerialNumber = dto.SerialNumber ?? string.Empty;
        machine.Location = dto.Location ?? string.Empty;
        machine.SupportedMaterials = dto.SupportedMaterials ?? string.Empty;
        machine.CurrentMaterial = dto.CurrentMaterial ?? string.Empty;
        machine.Status = dto.Status;
        machine.IsActive = dto.IsActive;
        machine.IsAvailableForScheduling = dto.IsAvailableForScheduling;
        machine.Priority = dto.Priority;
        machine.BuildLengthMm = dto.BuildLengthMm;
        machine.BuildWidthMm = dto.BuildWidthMm;
        machine.BuildHeightMm = dto.BuildHeightMm;
        machine.MaxLaserPowerWatts = dto.MaxLaserPowerWatts;
        machine.MaxScanSpeedMmPerSec = dto.MaxScanSpeedMmPerSec;
        machine.MinLayerThicknessMicrons = dto.MinLayerThicknessMicrons;
        machine.MaxLayerThicknessMicrons = dto.MaxLayerThicknessMicrons;
        machine.MaintenanceIntervalHours = dto.MaintenanceIntervalHours;
        machine.OpcUaEndpointUrl = dto.OpcUaEndpointUrl ?? string.Empty;
        machine.OpcUaEnabled = dto.OpcUaEnabled;
        machine.LastModifiedBy = User.Identity?.Name ?? "Admin";
        machine.LastModifiedDate = DateTime.UtcNow;
    }

    public void LoadMachineForEditing(Machine machine)
    {
        EditingMachineId = machine.Id;
        EditMachineRequest = new EditMachineDto
        {
            MachineId = machine.MachineId,
            MachineName = machine.MachineName,
            MachineModel = machine.MachineModel,
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
}

// MODERN DTOs - Clean separation of concerns without validation conflicts
public class CreateMachineDto
{
    public string MachineId { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string MachineModel { get; set; } = "TruPrint 3000";
    public string? SerialNumber { get; set; }
    public string? Location { get; set; }
    public string SupportedMaterials { get; set; } = string.Empty;
    public string? CurrentMaterial { get; set; }
    public string Status { get; set; } = "Idle";
    public bool IsActive { get; set; } = true;
    public bool IsAvailableForScheduling { get; set; } = true;
    public int Priority { get; set; } = 3;
    public double BuildLengthMm { get; set; } = 250;
    public double BuildWidthMm { get; set; } = 250;
    public double BuildHeightMm { get; set; } = 300;
    public double MaxLaserPowerWatts { get; set; } = 400;
    public double MaxScanSpeedMmPerSec { get; set; } = 7000;
    public double MinLayerThicknessMicrons { get; set; } = 20;
    public double MaxLayerThicknessMicrons { get; set; } = 60;
    public double MaintenanceIntervalHours { get; set; } = 500;
    public string? OpcUaEndpointUrl { get; set; }
    public bool OpcUaEnabled { get; set; } = false;
}

public class EditMachineDto
{
    public string MachineId { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string MachineModel { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? Location { get; set; }
    public string? SupportedMaterials { get; set; }
    public string? CurrentMaterial { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsAvailableForScheduling { get; set; }
    public int Priority { get; set; }
    public double BuildLengthMm { get; set; }
    public double BuildWidthMm { get; set; }
    public double BuildHeightMm { get; set; }
    public double MaxLaserPowerWatts { get; set; }
    public double MaxScanSpeedMmPerSec { get; set; }
    public double MinLayerThicknessMicrons { get; set; }
    public double MaxLayerThicknessMicrons { get; set; }
    public double MaintenanceIntervalHours { get; set; }
    public string? OpcUaEndpointUrl { get; set; }
    public bool OpcUaEnabled { get; set; }
}

public class CreateCapabilityDto
{
    public string MachineId { get; set; } = string.Empty;
    public string CapabilityName { get; set; } = string.Empty;
    public string CapabilityType { get; set; } = string.Empty;
    public string ValueType { get; set; } = "Number";
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public double? DefaultValue { get; set; }
    public string? Unit { get; set; }
    public string? Description { get; set; }
    public bool IsRequired { get; set; } = false;
}

// Simple validation result class
public class ValidationResult
{
    public List<ValidationError> Errors { get; } = new();
    public bool IsValid => Errors.Count == 0;

    public void AddError(string propertyName, string errorMessage)
    {
        Errors.Add(new ValidationError { PropertyName = propertyName, ErrorMessage = errorMessage });
    }
}

public class ValidationError
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}