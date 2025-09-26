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

    public async Task<IActionResult> OnGetAddAsync(int? dayOfWeek)
    {
        try
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [SHIFTS-{OperationId}] Loading add shift form (dayOfWeek={Day})", operationId, dayOfWeek);

            var targetDay = dayOfWeek.HasValue && dayOfWeek.Value is >= 0 and <= 6 ? dayOfWeek.Value : 1;

            // Pull raw list first (SQLite cannot order by TimeSpan directly in translation reliably)
            var existingForDay = await _context.OperatingShifts
                .Where(s => !s.IsHoliday && !s.SpecificDate.HasValue && s.DayOfWeek == targetDay && (s.MachineId == null || s.MachineId == ""))
                .ToListAsync();
            existingForDay = existingForDay.OrderBy(s => s.StartTime).ToList();

            (TimeSpan start, TimeSpan end)? gap = FindFirstGap(existingForDay);
            var suggestedStart = gap?.start ?? new TimeSpan(0, 0, 0);
            var suggestedEnd = gap?.end ?? new TimeSpan(6, 0, 0);
            if (suggestedEnd <= suggestedStart)
            {
                suggestedEnd = suggestedStart.Add(TimeSpan.FromHours(1));
                if (suggestedEnd.Days > 0) suggestedEnd = new TimeSpan(23, 59, 0);
            }

            Input = new OperatingShift
            {
                DayOfWeek = targetDay,
                StartTime = suggestedStart,
                EndTime = suggestedEnd,
                IsActive = true,
                Description = "New Shift"
            };

            ViewData["ExistingShiftsForDay"] = existingForDay
                .Select(s => $"{s.Description} {s.StartTime:hh\\:mm}-{s.EndTime:hh\\:mm}")
                .ToList();
            ViewData["SuggestedGap"] = $"Suggested free slot: {Input.StartTime:hh\\:mm}-{Input.EndTime:hh\\:mm}";

            await LoadMachinesAsync();
            return Partial("~/Pages/Admin/Shared/_ShiftForm.cshtml", this);
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
            return Partial("~/Pages/Admin/Shared/_ShiftForm.cshtml", this);
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

            // First pass validation
            var validationErrors = await ValidateShiftAsync(Input);

            // If ONLY conflicts and they are exact duplicate of an existing shift, promote to update instead of blocking
            if (Input.Id == 0 && validationErrors.Any() && validationErrors.All(e => e.StartsWith("Conflicts with existing shift")))
            {
                var duplicateId = await FindExactDuplicateShiftIdAsync(Input);
                if (duplicateId.HasValue)
                {
                    _logger.LogInformation("?? [SHIFTS-{OperationId}] Detected exact duplicate, promoting create to update of Id {Id}", operationId, duplicateId.Value);
                    Input.Id = duplicateId.Value; // convert to update
                    validationErrors.Clear();
                }
            }

            if (validationErrors.Any())
            {
                _logger.LogWarning("?? [SHIFTS-{OperationId}] Validation failed: {ErrorCount} errors", operationId, validationErrors.Count);

                // If conflicts present, compute and show a suggested next free gap
                if (validationErrors.Any(e => e.StartsWith("Conflicts with existing shift")))
                {
                    var suggestion = await SuggestNextFreeGapAsync(Input);
                    if (suggestion.HasValue)
                    {
                        ViewData["SuggestedGap"] = $"Suggested free slot: {suggestion.Value.start:hh\\:mm}-{suggestion.Value.end:hh\\:mm}";
                    }
                }

                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError("", error);
                }
                ViewData["ValidationErrors"] = validationErrors;
                await LoadMachinesAsync();
                return Partial("~/Pages/Admin/Shared/_ShiftForm.cshtml", this);
            }

            // Set audit fields
            if (Input.Id == 0)
            {
                Input.CreatedBy = userName;
                Input.CreatedDate = DateTime.UtcNow;
            }
            Input.LastModifiedBy = userName;
            Input.LastModifiedDate = DateTime.UtcNow;

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
                _logger.LogWarning("?? [SHIFTS-{OperationId}] Failed to save shift - conflicts detected after service call", operationId);
                ModelState.AddModelError("", "Failed to save shift. Check for conflicts with existing shifts.");
                ViewData["ValidationErrors"] = new List<string> { "Failed to save shift. Check for conflicts with existing shifts." };
                await LoadMachinesAsync();
                return Partial("~/Pages/Admin/Shared/_ShiftForm.cshtml", this);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error saving shift");
            ModelState.AddModelError("", "An error occurred while saving the shift");
            ViewData["ValidationErrors"] = new List<string> { "An error occurred while saving the shift" };
            await LoadMachinesAsync();
            return Partial("~/Pages/Admin/Shared/_ShiftForm.cshtml", this);
        }
    }

    private async Task<int?> FindExactDuplicateShiftIdAsync(OperatingShift candidate)
    {
        try
        {
            var dup = await _context.OperatingShifts
                .Where(s => s.DayOfWeek == candidate.DayOfWeek
                            && s.StartTime == candidate.StartTime
                            && s.EndTime == candidate.EndTime
                            && s.IsHoliday == candidate.IsHoliday
                            && (s.MachineId ?? "") == (candidate.MachineId ?? "") )
                .OrderBy(s => s.Id)
                .FirstOrDefaultAsync();
            return dup?.Id;
        }
        catch
        {
            return null;
        }
    }

    private async Task<(TimeSpan start, TimeSpan end)?> SuggestNextFreeGapAsync(OperatingShift baseShift)
    {
        try
        {
            var list = await _context.OperatingShifts
                .Where(s => s.DayOfWeek == baseShift.DayOfWeek && !s.IsHoliday && !s.SpecificDate.HasValue && (s.MachineId == baseShift.MachineId || (s.MachineId == null && baseShift.MachineId == null)))
                .ToListAsync();
            var normalized = list.OrderBy(s => s.StartTime).ToList();
            // find first gap of at least 30 minutes that does not overlap
            TimeSpan cursor = TimeSpan.Zero;
            foreach (var s in normalized)
            {
                var st = s.StartTime;
                var en = s.EndTime;
                if (en < st) en = en.Add(TimeSpan.FromDays(1));
                if (st > cursor)
                {
                    var gap = st - cursor;
                    if (gap >= TimeSpan.FromMinutes(30))
                    {
                        var length = TimeSpan.FromHours( (baseShift.EndTime - baseShift.StartTime).TotalHours < 0 ? 1 : (baseShift.EndTime - baseShift.StartTime).TotalHours );
                        if (length <= TimeSpan.Zero) length = TimeSpan.FromHours(1);
                        var proposedEnd = cursor + (length > gap ? gap : length);
                        return (cursor, proposedEnd);
                    }
                }
                cursor = TimeSpan.FromMinutes(Math.Max(cursor.TotalMinutes, en.TotalMinutes % (24*60)));
            }
            // after last
            if (cursor < TimeSpan.FromHours(24) - TimeSpan.FromMinutes(30))
            {
                var end = cursor + TimeSpan.FromHours(1);
                if (end > TimeSpan.FromHours(24)) end = TimeSpan.FromHours(24) - TimeSpan.FromMinutes(1);
                return (cursor, end);
            }
            return null;
        }
        catch
        {
            return null;
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

    [ValidateAntiForgeryToken]
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
                return new JsonResult(new { success = true, message = "Shift deleted" });
            }
            _logger.LogWarning("?? [SHIFTS-{OperationId}] Failed to delete shift {ShiftId}", operationId, id);
            return new JsonResult(new { success = false, message = "Delete failed" });
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
                return new JsonResult(new { success = false, message = "Shift not found" });
            shift.IsActive = !shift.IsActive;
            shift.LastModifiedBy = userName;
            shift.LastModifiedDate = DateTime.UtcNow;
            var success = await _shiftService.UpdateShiftAsync(shift);
            if (success)
            {
                return new JsonResult(new { success = true, isActive = shift.IsActive });
            }
            return new JsonResult(new { success = false, message = "Failed to toggle shift" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error toggling shift {ShiftId}", id);
            return new JsonResult(new { success = false, message = "Error toggling shift" });
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
                return new JsonResult(new { success = false, message = "Unknown template" });
            var existing = await _shiftService.GetAllShiftsAsync();
            return new JsonResult(new { success = true, count = templateShifts.Count, existingCount = existing.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error loading template {Template}", template);
            return new JsonResult(new { success = false, message = "Error loading template" });
        }
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostApplyTemplateAsync(string template, bool clearExisting = false)
    {
        try
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            var currentUser = await _authService.GetCurrentUserAsync(HttpContext);
            var userName = currentUser?.Username ?? "System";
            _logger.LogInformation("?? [SHIFTS-{OperationId}] Applying template {Template} (clearExisting={Clear})", operationId, template, clearExisting);
            List<OperatingShift> templateShifts = template switch
            {
                "business" => DefaultOperatingShifts.GetStandardBusinessHours(),
                "24x7" => DefaultOperatingShifts.Get24x7Schedule(),
                "twoshift" => DefaultOperatingShifts.GetTwoShiftSchedule(),
                "plant" => DefaultOperatingShifts.GetPlantTwoShiftSchedule(),
                _ => new List<OperatingShift>()
            };
            if (!templateShifts.Any())
                return new JsonResult(new { success = false, message = "Unknown template" });
            if (clearExisting)
            {
                var existing = await _shiftService.GetAllShiftsAsync();
                foreach (var s in existing)
                    await _shiftService.DeleteShiftAsync(s.Id);
            }
            var created = 0;
            foreach (var s in templateShifts)
            {
                s.CreatedBy = userName;
                s.LastModifiedBy = userName;
                if (await _shiftService.CreateShiftAsync(s)) created++;
            }
            return new JsonResult(new { success = true, message = $"Template applied: {created}/{templateShifts.Count} shifts", redirect = "/Admin/Shifts" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SHIFTS] Error applying template {Template}", template);
            return new JsonResult(new { success = false, message = "Error applying template" });
        }
    }

    private static (TimeSpan start, TimeSpan end)? FindFirstGap(List<OperatingShift> shifts)
    {
        // Normalize list with adjusted overnight shifts (end < start treated as crossing midnight)
        var normalized = new List<(TimeSpan start, TimeSpan end)>();
        foreach (var s in shifts)
        {
            var st = s.StartTime;
            var en = s.EndTime;
            if (en < st) en = en.Add(TimeSpan.FromDays(1)); // treat as >24h timeline
            normalized.Add((st, en));
        }
        normalized = normalized.OrderBy(t => t.start).ToList();

        // Start-of-day gap (midnight to first shift)
        TimeSpan dayStart = TimeSpan.Zero;
        if (!normalized.Any())
        {
            return (TimeSpan.FromHours(6), TimeSpan.FromHours(12)); // empty day, arbitrary 6-12 block
        }
        var first = normalized.First();
        if (first.start > dayStart)
        {
            return (dayStart, first.start);
        }
        // Gaps between shifts
        for (int i = 0; i < normalized.Count - 1; i++)
        {
            var a = normalized[i];
            var b = normalized[i + 1];
            if (b.start > a.end)
            {
                // found a gap
                var length = b.start - a.end;
                if (length >= TimeSpan.FromMinutes(30))
                {
                    // choose up to 6h or entire gap if smaller
                    var proposedEnd = a.end + (length > TimeSpan.FromHours(6) ? TimeSpan.FromHours(6) : length);
                    return (a.end, proposedEnd);
                }
            }
        }
        // End-of-day gap (after last shift until midnight)
        var last = normalized.Last();
        var dayEnd = TimeSpan.FromDays(1); // 24:00 represented as 1 day
        if (last.end < dayEnd)
        {
            var remaining = dayEnd - last.end;
            var block = remaining > TimeSpan.FromHours(6) ? TimeSpan.FromHours(6) : remaining;
            return (last.end >= TimeSpan.FromHours(24) ? TimeSpan.FromHours(23) : last.end, last.end + block);
        }
        return null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Fallback router: if a specific handler wasn't matched (framework fell back to implicit handler)
        var rawHandler = Request.Query["handler"].ToString();
        if (!string.IsNullOrWhiteSpace(rawHandler))
        {
            switch (rawHandler.ToLowerInvariant())
            {
                case "delete":
                    if (int.TryParse(Request.Query["id"], out var delId))
                    {
                        return await OnPostDeleteAsync(delId);
                    }
                    return new JsonResult(new { success = false, message = "Invalid shift id" });
            }
        }
        // Default: reload page data and show full page (implicit behavior)
        await OnGetAsync();
        return Page();
    }
}