using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using OpCentrix.Services.Admin;
using OpCentrix.Services;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;

namespace OpCentrix.Pages.Admin;

/// <summary>
/// Admin page for managing operating shifts with calendar-style interface
/// Task 8: Operating Shift Editor
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class ShiftsModel : PageModel
{
    private readonly IOperatingShiftService _shiftService;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<ShiftsModel> _logger;
    private readonly SchedulerContext _context;
    private readonly IOperatorAssignmentService _assignmentService;

    public ShiftsModel(IOperatingShiftService shiftService, IAuthenticationService authService, ILogger<ShiftsModel> logger, SchedulerContext context, IOperatorAssignmentService assignmentService)
    {
        _shiftService = shiftService;
        _authService = authService;
        _logger = logger;
        _context = context;
        _assignmentService = assignmentService;
    }

    // Properties for the page
    public List<OperatingShift> Shifts { get; set; } = new();
    public List<OperatingShift> HolidayShifts { get; set; } = new();
    public Dictionary<int, List<OperatingShift>> ShiftsByDay { get; set; } = new();
    public List<string> ConflictErrors { get; set; } = new();
    // Assignments panel data
    public List<Machine> Machines { get; set; } = new();
    public List<MachineOperatorAssignment> Assignments { get; set; } = new();
    public List<User> EligibleOperators { get; set; } = new();
    public string? SelectedMachineId { get; set; }

    // Form binding
    [BindProperty]
    public OperatingShift Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            await EnsureDefaultShiftsSeededAsync();
            await LoadShiftsAsync();
            _logger.LogInformation("?? [SHIFTS] Admin shifts page loaded - {ShiftCount} shifts, {HolidayCount} holidays", 
                Shifts.Count, HolidayShifts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error loading shifts page");
            ModelState.AddModelError("", "Error loading shifts. Please try again.");
        }
    }

    public async Task<IActionResult> OnGetAddAsync()
    {
        try
        {
            // Return empty form for adding new shift
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [SHIFTS-{OperationId}] Loading add shift form", operationId);

            Input = new OperatingShift
            {
                DayOfWeek = 1, // Monday default
                StartTime = new TimeSpan(8, 0, 0), // 8 AM default
                EndTime = new TimeSpan(17, 0, 0), // 5 PM default
                IsActive = true,
                Description = "Standard Shift"
            };

            await LoadMachinesAsync();
            return Partial("_ShiftForm", this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error loading add form");
            return new JsonResult(new { success = false, message = "Error loading form" });
        }
    }

    public async Task<IActionResult> OnGetEditAsync(int id)
    {
        try
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [SHIFTS-{OperationId}] Loading edit form for shift {ShiftId}", operationId, id);

            var shift = await _shiftService.GetShiftAsync(id);
            if (shift == null)
            {
                _logger.LogWarning("?? [SHIFTS-{OperationId}] Shift {ShiftId} not found for editing", operationId, id);
                return new JsonResult(new { success = false, message = "Shift not found" });
            }

            Input = shift;
            await LoadMachinesAsync();
            return Partial("_ShiftForm", this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error loading edit form for shift {ShiftId}", id);
            return new JsonResult(new { success = false, message = "Error loading shift" });
        }
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        try
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            var currentUser = await _authService.GetCurrentUserAsync(HttpContext);
            var userName = currentUser?.Username ?? "Unknown";

            _logger.LogInformation("?? [SHIFTS-{OperationId}] Saving shift: {DayName} {StartTime}-{EndTime} by {User}", 
                operationId, Input.DayName, Input.StartTime, Input.EndTime, userName);

            // Validate the shift
            var validationErrors = await ValidateShiftAsync(Input);
            if (validationErrors.Any())
            {
                _logger.LogWarning("?? [SHIFTS-{OperationId}] Validation failed: {ErrorCount} errors", operationId, validationErrors.Count);
                
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError("", error);
                }
                
                ViewData["ValidationErrors"] = validationErrors;
                await LoadMachinesAsync();
                return Partial("_ShiftForm", this);
            }

            // Set audit fields
            if (Input.Id == 0)
            {
                Input.CreatedBy = userName;
                Input.CreatedDate = DateTime.UtcNow;
            }
            
            Input.LastModifiedBy = userName;
            Input.LastModifiedDate = DateTime.UtcNow;

            // Save the shift
            bool success;
            if (Input.Id == 0)
            {
                success = await _shiftService.CreateShiftAsync(Input);
                _logger.LogInformation("? [SHIFTS-{OperationId}] Shift created successfully: {ShiftId}", operationId, Input.Id);
            }
            else
            {
                success = await _shiftService.UpdateShiftAsync(Input);
                _logger.LogInformation("? [SHIFTS-{OperationId}] Shift updated successfully: {ShiftId}", operationId, Input.Id);
            }

            if (success)
            {
                return new JsonResult(new { 
                    success = true, 
                    message = Input.Id == 0 ? "Shift created successfully" : "Shift updated successfully",
                    redirect = "/Admin/Shifts"
                });
            }
            else
            {
                _logger.LogWarning("?? [SHIFTS-{OperationId}] Failed to save shift - conflicts detected", operationId);
                ModelState.AddModelError("", "Failed to save shift. Check for conflicts with existing shifts.");
                ViewData["ValidationErrors"] = new List<string> { "Failed to save shift. Check for conflicts with existing shifts." };
                await LoadMachinesAsync();
                return Partial("_ShiftForm", this);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error saving shift");
            ModelState.AddModelError("", "An error occurred while saving the shift");
            ViewData["ValidationErrors"] = new List<string> { "An error occurred while saving the shift" };
            await LoadMachinesAsync();
            return Partial("_ShiftForm", this);
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            var currentUser = await _authService.GetCurrentUserAsync(HttpContext);
            var userName = currentUser?.Username ?? "Unknown";

            _logger.LogInformation("?? [SHIFTS-{OperationId}] Deleting shift {ShiftId} by {User}", operationId, id, userName);

            var success = await _shiftService.DeleteShiftAsync(id);
            
            if (success)
            {
                _logger.LogInformation("? [SHIFTS-{OperationId}] Shift {ShiftId} deleted successfully", operationId, id);
                return new JsonResult(new { success = true, message = "Shift deleted successfully" });
            }
            else
            {
                _logger.LogWarning("?? [SHIFTS-{OperationId}] Failed to delete shift {ShiftId}", operationId, id);
                return new JsonResult(new { success = false, message = "Failed to delete shift" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error deleting shift {ShiftId}", id);
            return new JsonResult(new { success = false, message = "Error deleting shift" });
        }
    }

    public async Task<IActionResult> OnGetToggleActiveAsync(int id)
    {
        try
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            var currentUser = await _authService.GetCurrentUserAsync(HttpContext);
            var userName = currentUser?.Username ?? "Unknown";

            _logger.LogInformation("?? [SHIFTS-{OperationId}] Toggling active status for shift {ShiftId} by {User}", operationId, id, userName);

            var shift = await _shiftService.GetShiftAsync(id);
            if (shift == null)
            {
                return new JsonResult(new { success = false, message = "Shift not found" });
            }

            shift.IsActive = !shift.IsActive;
            shift.LastModifiedBy = userName;
            shift.LastModifiedDate = DateTime.UtcNow;

            var success = await _shiftService.UpdateShiftAsync(shift);
            
            if (success)
            {
                _logger.LogInformation("? [SHIFTS-{OperationId}] Shift {ShiftId} status toggled to {Status}", 
                    operationId, id, shift.IsActive ? "Active" : "Inactive");
                return new JsonResult(new { 
                    success = true, 
                    message = $"Shift {(shift.IsActive ? "activated" : "deactivated")} successfully",
                    isActive = shift.IsActive
                });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Failed to update shift status" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error toggling shift {ShiftId} status", id);
            return new JsonResult(new { success = false, message = "Error updating shift status" });
        }
    }

    public async Task<IActionResult> OnGetLoadTemplateAsync(string template)
    {
        try
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [SHIFTS-{OperationId}] Loading template: {Template}", operationId, template);

            List<OperatingShift> templateShifts = template switch
            {
                "business" => DefaultOperatingShifts.GetStandardBusinessHours(),
                "24x7" => DefaultOperatingShifts.Get24x7Schedule(),
                "twoshift" => DefaultOperatingShifts.GetTwoShiftSchedule(),
                "plant" => DefaultOperatingShifts.GetPlantTwoShiftSchedule(),
                _ => new List<OperatingShift>()
            };

            if (!templateShifts.Any())
            {
                return new JsonResult(new { success = false, message = "Unknown template" });
            }

            // Apply template shifts
            var currentUser = await _authService.GetCurrentUserAsync(HttpContext);
            var userName = currentUser?.Username ?? "System";

            foreach (var shift in templateShifts)
            {
                shift.CreatedBy = userName;
                shift.LastModifiedBy = userName;
            }

            // First, clear existing active shifts (optional - ask user for confirmation)
            var existingShifts = await _shiftService.GetActiveShiftsAsync();
            
            return new JsonResult(new { 
                success = true, 
                message = $"Template '{template}' loaded. This will add {templateShifts.Count} shifts.",
                shifts = templateShifts.Select(s => new {
                    dayOfWeek = s.DayOfWeek,
                    dayName = s.DayName,
                    startTime = s.StartTime.ToString(@"hh\:mm"),
                    endTime = s.EndTime.ToString(@"hh\:mm"),
                    description = s.Description
                }),
                existingCount = existingShifts.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error loading template {Template}", template);
            return new JsonResult(new { success = false, message = "Error loading template" });
        }
    }

    public async Task<IActionResult> OnPostApplyTemplateAsync(string template, bool clearExisting = false)
    {
        try
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            var currentUser = await _authService.GetCurrentUserAsync(HttpContext);
            var userName = currentUser?.Username ?? "System";

            _logger.LogInformation("?? [SHIFTS-{OperationId}] Applying template {Template} by {User} (clearExisting: {ClearExisting})", 
                operationId, template, userName, clearExisting);

            List<OperatingShift> templateShifts = template switch
            {
                "business" => DefaultOperatingShifts.GetStandardBusinessHours(),
                "24x7" => DefaultOperatingShifts.Get24x7Schedule(),
                "twoshift" => DefaultOperatingShifts.GetTwoShiftSchedule(),
                "plant" => DefaultOperatingShifts.GetPlantTwoShiftSchedule(),
                _ => new List<OperatingShift>()
            };

            if (!templateShifts.Any())
            {
                return new JsonResult(new { success = false, message = "Unknown template" });
            }

            // Clear existing shifts if requested
            if (clearExisting)
            {
                var existingShifts = await _shiftService.GetActiveShiftsAsync();
                foreach (var shift in existingShifts)
                {
                    await _shiftService.DeleteShiftAsync(shift.Id);
                }
                _logger.LogInformation("?? [SHIFTS-{OperationId}] Cleared {Count} existing shifts", operationId, existingShifts.Count);
            }

            // Apply template shifts
            var successCount = 0;
            foreach (var shift in templateShifts)
            {
                shift.CreatedBy = userName;
                shift.LastModifiedBy = userName;
                
                var success = await _shiftService.CreateShiftAsync(shift);
                if (success) successCount++;
            }

            _logger.LogInformation("? [SHIFTS-{OperationId}] Applied template: {SuccessCount}/{TotalCount} shifts created", 
                operationId, successCount, templateShifts.Count);

            return new JsonResult(new { 
                success = true, 
                message = $"Template applied successfully. Created {successCount} out of {templateShifts.Count} shifts.",
                redirect = "/Admin/Shifts"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error applying template {Template}", template);
            return new JsonResult(new { success = false, message = "Error applying template" });
        }
    }

    private async Task LoadShiftsAsync()
    {
        Shifts = await _shiftService.GetAllShiftsAsync();
        HolidayShifts = await _shiftService.GetHolidayShiftsAsync();
        
        // Group shifts by day of week for calendar display
        ShiftsByDay = Shifts
            .Where(s => !s.IsHoliday && !s.SpecificDate.HasValue)
            .GroupBy(s => s.DayOfWeek)
            .ToDictionary(g => g.Key, g => g.OrderBy(s => s.StartTime).ToList());
    }

    private async Task<List<string>> ValidateShiftAsync(OperatingShift shift)
    {
        var errors = new List<string>();

        try
        {
            // Basic validation
            if (shift.StartTime >= shift.EndTime)
            {
                // Allow overnight shifts
                if (shift.EndTime.Add(TimeSpan.FromDays(1)) <= shift.StartTime)
                {
                    errors.Add("Invalid shift times. End time must be after start time (accounting for overnight shifts).");
                }
            }

            if (shift.DayOfWeek < 0 || shift.DayOfWeek > 6)
            {
                errors.Add("Day of week must be between 0 (Sunday) and 6 (Saturday)");
            }

            if (string.IsNullOrWhiteSpace(shift.Description))
            {
                errors.Add("Description is required");
            }

            if (shift.Description?.Length > 200)
            {
                errors.Add("Description cannot exceed 200 characters");
            }

            // Check for conflicts with existing shifts
            var conflicts = await _shiftService.GetConflictingShiftsAsync(shift);
            if (shift.Id > 0)
            {
                conflicts = conflicts.Where(c => c.Id != shift.Id).ToList();
            }

            if (conflicts.Any())
            {
                foreach (var conflict in conflicts)
                {
                    errors.Add($"Conflicts with existing shift: {conflict.Description} ({conflict.StartTime:hh\\:mm}-{conflict.EndTime:hh\\:mm})");
                }
            }

            // Validate specific date for holidays
            if (shift.IsHoliday && shift.SpecificDate.HasValue)
            {
                if (shift.SpecificDate.Value.Date < DateTime.Today)
                {
                    errors.Add("Holiday date cannot be in the past");
                }
            }

            return errors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error validating shift");
            errors.Add("Error validating shift");
            return errors;
        }
    }

    private async Task LoadMachinesAsync()
    {
        try
        {
            var machines = await _context.Machines.Where(m => m.IsActive).OrderBy(m => m.MachineId).ToListAsync();
            ViewData["Machines"] = machines;
        }
        catch
        {
            ViewData["Machines"] = new List<OpCentrix.Models.Machine>();
        }
    }

    private async Task EnsureDefaultShiftsSeededAsync()
    {
        try
        {
            // If there are no shifts at all, seed the plant schedule as global defaults
            var anyShifts = await _context.OperatingShifts.AnyAsync();
            if (!anyShifts)
            {
                var currentUser = await _authService.GetCurrentUserAsync(HttpContext);
                var userName = currentUser?.Username ?? "System";

                var defaults = DefaultOperatingShifts.GetPlantTwoShiftSchedule();
                foreach (var s in defaults)
                {
                    s.CreatedBy = userName;
                    s.LastModifiedBy = userName;
                }

                var created = 0;
                foreach (var shift in defaults)
                {
                    var ok = await _shiftService.CreateShiftAsync(shift);
                    if (ok) created++;
                }

                TempData["ShiftsSeeded"] = created;
                _logger.LogInformation("? [SHIFTS] Seeded {Count} default plant schedule shifts", created);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error seeding default shifts");
        }
    }

    // Assignments Panel Handlers
    public async Task<IActionResult> OnGetAssignmentsAsync(string? machineId)
    {
        try
        {
            SelectedMachineId = machineId;
            await LoadMachinesAndEligibleOperatorsAsync();
            if (string.IsNullOrEmpty(SelectedMachineId) && Machines.Any())
            {
                SelectedMachineId = Machines.First().MachineId;
            }
            Assignments = string.IsNullOrEmpty(SelectedMachineId)
                ? new List<MachineOperatorAssignment>()
                : await _assignmentService.GetAssignmentsByMachineAsync(SelectedMachineId);

            return Partial("~/Pages/Admin/Shared/_AssignmentsPanel.cshtml", this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ASSIGN] Error loading assignments panel for {MachineId}", machineId);
            return new JsonResult(new { success = false, message = "Error loading assignments" });
        }
    }

    public async Task<IActionResult> OnPostAssignAsync([FromBody] AssignRequest request)
    {
        try
        {
            var actor = (await _authService.GetCurrentUserAsync(HttpContext))?.Username ?? "System";

            if (!request.force)
            {
                var warn = await _assignmentService.CheckDoubleAssignmentWarningAsync(request.userId, request.machineId);
                if (!string.IsNullOrEmpty(warn))
                {
                    return new JsonResult(new { success = false, warning = warn });
                }
            }

            var (success, warning, assignment) = await _assignmentService.AssignOperatorAsync(
                request.machineId, request.userId, request.isPrimary, request.effectiveFrom, request.effectiveTo, actor);
            return new JsonResult(new
            {
                success,
                warning,
                assignment = assignment == null ? null : new
                {
                    assignment.Id,
                    assignment.MachineId,
                    assignment.UserId,
                    assignment.IsPrimary,
                    assignment.EffectiveFrom,
                    assignment.EffectiveTo,
                    assignment.IsActive
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ASSIGN] Error assigning operator");
            return new JsonResult(new { success = false, message = "Error assigning operator" });
        }
    }

    public async Task<IActionResult> OnPostUnassignAsync([FromBody] UnassignRequest request)
    {
        try
        {
            var actor = (await _authService.GetCurrentUserAsync(HttpContext))?.Username ?? "System";
            var ok = await _assignmentService.UnassignAsync(request.assignmentId, actor);
            return new JsonResult(new { success = ok });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ASSIGN] Error unassigning operator");
            return new JsonResult(new { success = false, message = "Error unassigning operator" });
        }
    }

    public async Task<IActionResult> OnPostSetPrimaryAsync([FromBody] SetPrimaryRequest request)
    {
        try
        {
            var actor = (await _authService.GetCurrentUserAsync(HttpContext))?.Username ?? "System";
            var ok = await _assignmentService.SetPrimaryAsync(request.assignmentId, actor);
            return new JsonResult(new { success = ok });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ASSIGN] Error setting primary assignment");
            return new JsonResult(new { success = false, message = "Error updating primary assignment" });
        }
    }

    private async Task LoadMachinesAndEligibleOperatorsAsync()
    {
        Machines = await _context.Machines.Where(m => m.IsActive).OrderBy(m => m.MachineId).ToListAsync();
        var eligibleRoles = new[]
        {
            UserRoles.Operator,
            UserRoles.PrintingSpecialist,
            UserRoles.EDMSpecialist,
            UserRoles.MachiningSpecialist,
            UserRoles.CoatingSpecialist,
            UserRoles.QCSpecialist
        };
        EligibleOperators = await _context.Users
            .Where(u => u.IsActive && eligibleRoles.Contains(u.Role))
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    // Request DTOs for JSON binding
    public class AssignRequest
    {
        public string machineId { get; set; } = string.Empty;
        public int userId { get; set; }
        public bool isPrimary { get; set; }
        public DateTime? effectiveFrom { get; set; }
        public DateTime? effectiveTo { get; set; }
        public bool force { get; set; }
    }

    public class UnassignRequest
    {
        public int assignmentId { get; set; }
    }

    public class SetPrimaryRequest
    {
        public int assignmentId { get; set; }
    }

    // Form-post friendly variants (for environments where JSON antiforgery header pairing fails)
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAssignFormAsync(string machineId, int userId, bool isPrimary, DateTime? effectiveFrom, DateTime? effectiveTo, bool force = false)
    {
        var req = new AssignRequest { machineId = machineId, userId = userId, isPrimary = isPrimary, effectiveFrom = effectiveFrom, effectiveTo = effectiveTo, force = force };
        return await OnPostAssignAsync(req);
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostUnassignFormAsync(int assignmentId)
    {
        var req = new UnassignRequest { assignmentId = assignmentId };
        return await OnPostUnassignAsync(req);
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostSetPrimaryFormAsync(int assignmentId)
    {
        var req = new SetPrimaryRequest { assignmentId = assignmentId };
        return await OnPostSetPrimaryAsync(req);
    }
}