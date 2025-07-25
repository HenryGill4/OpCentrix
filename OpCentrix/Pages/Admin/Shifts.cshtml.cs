using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using OpCentrix.Services.Admin;
using OpCentrix.Services;

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

    public ShiftsModel(IOperatingShiftService shiftService, IAuthenticationService authService, ILogger<ShiftsModel> logger)
    {
        _shiftService = shiftService;
        _authService = authService;
        _logger = logger;
    }

    // Properties for the page
    public List<OperatingShift> Shifts { get; set; } = new();
    public List<OperatingShift> HolidayShifts { get; set; } = new();
    public Dictionary<int, List<OperatingShift>> ShiftsByDay { get; set; } = new();
    public List<string> ConflictErrors { get; set; } = new();
    
    // Form binding
    [BindProperty]
    public OperatingShift Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
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
                return Partial("_ShiftForm", this);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error saving shift");
            ModelState.AddModelError("", "An error occurred while saving the shift");
            ViewData["ValidationErrors"] = new List<string> { "An error occurred while saving the shift" };
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
}