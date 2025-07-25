using OpCentrix.Data;
using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for managing operating shifts and working hours
/// Task 2: Admin Control System - OperatingShift management
/// </summary>
public interface IOperatingShiftService
{
    Task<List<OperatingShift>> GetAllShiftsAsync();
    Task<List<OperatingShift>> GetActiveShiftsAsync();
    Task<List<OperatingShift>> GetShiftsForDayAsync(DayOfWeek dayOfWeek);
    Task<OperatingShift?> GetShiftAsync(int id);
    Task<bool> CreateShiftAsync(OperatingShift shift);
    Task<bool> UpdateShiftAsync(OperatingShift shift);
    Task<bool> DeleteShiftAsync(int id);
    Task<bool> IsTimeWithinOperatingHoursAsync(DateTime dateTime);
    Task<List<OperatingShift>> GetConflictingShiftsAsync(OperatingShift shift);
    Task<TimeSpan> GetShiftDurationAsync(int shiftId);
    Task<List<OperatingShift>> GetHolidayShiftsAsync();
}

public class OperatingShiftService : IOperatingShiftService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<OperatingShiftService> _logger;

    public OperatingShiftService(SchedulerContext context, ILogger<OperatingShiftService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<OperatingShift>> GetAllShiftsAsync()
    {
        try
        {
            return await _context.OperatingShifts
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all operating shifts");
            return new List<OperatingShift>();
        }
    }

    public async Task<List<OperatingShift>> GetActiveShiftsAsync()
    {
        try
        {
            return await _context.OperatingShifts
                .Where(s => s.IsActive)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active operating shifts");
            return new List<OperatingShift>();
        }
    }

    public async Task<List<OperatingShift>> GetShiftsForDayAsync(DayOfWeek dayOfWeek)
    {
        try
        {
            var dayNumber = (int)dayOfWeek;
            return await _context.OperatingShifts
                .Where(s => s.DayOfWeek == dayNumber && s.IsActive && !s.IsHoliday)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shifts for day {DayOfWeek}", dayOfWeek);
            return new List<OperatingShift>();
        }
    }

    public async Task<OperatingShift?> GetShiftAsync(int id)
    {
        try
        {
            return await _context.OperatingShifts.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving operating shift {ShiftId}", id);
            return null;
        }
    }

    public async Task<bool> CreateShiftAsync(OperatingShift shift)
    {
        try
        {
            // Check for conflicts
            var conflicts = await GetConflictingShiftsAsync(shift);
            if (conflicts.Any())
            {
                _logger.LogWarning("Cannot create shift due to conflicts with existing shifts");
                return false;
            }

            _context.OperatingShifts.Add(shift);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Operating shift created for {DayName} {StartTime}-{EndTime} by {User}",
                shift.DayName, shift.StartTime, shift.EndTime, shift.CreatedBy);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating operating shift");
            return false;
        }
    }

    public async Task<bool> UpdateShiftAsync(OperatingShift shift)
    {
        try
        {
            var existing = await GetShiftAsync(shift.Id);
            if (existing == null)
                return false;

            // Check for conflicts (excluding the current shift)
            var conflicts = await GetConflictingShiftsAsync(shift);
            conflicts = conflicts.Where(c => c.Id != shift.Id).ToList();
            
            if (conflicts.Any())
            {
                _logger.LogWarning("Cannot update shift due to conflicts with existing shifts");
                return false;
            }

            existing.DayOfWeek = shift.DayOfWeek;
            existing.StartTime = shift.StartTime;
            existing.EndTime = shift.EndTime;
            existing.IsHoliday = shift.IsHoliday;
            existing.IsActive = shift.IsActive;
            existing.Description = shift.Description;
            existing.SpecificDate = shift.SpecificDate;
            existing.LastModifiedDate = DateTime.UtcNow;
            existing.LastModifiedBy = shift.LastModifiedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Operating shift {ShiftId} updated by {User}", shift.Id, shift.LastModifiedBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating operating shift {ShiftId}", shift.Id);
            return false;
        }
    }

    public async Task<bool> DeleteShiftAsync(int id)
    {
        try
        {
            var shift = await GetShiftAsync(id);
            if (shift == null)
                return false;

            _context.OperatingShifts.Remove(shift);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Operating shift {ShiftId} deleted", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting operating shift {ShiftId}", id);
            return false;
        }
    }

    public async Task<bool> IsTimeWithinOperatingHoursAsync(DateTime dateTime)
    {
        try
        {
            var dayOfWeek = (int)dateTime.DayOfWeek;
            
            // Check for specific date overrides first
            var specificDateShift = await _context.OperatingShifts
                .FirstOrDefaultAsync(s => s.SpecificDate.HasValue && 
                                         s.SpecificDate.Value.Date == dateTime.Date && 
                                         s.IsActive);

            if (specificDateShift != null)
            {
                return specificDateShift.IsTimeWithinShift(dateTime);
            }

            // Check regular day-of-week shifts
            var shifts = await _context.OperatingShifts
                .Where(s => s.DayOfWeek == dayOfWeek && 
                           s.IsActive && 
                           !s.IsHoliday && 
                           !s.SpecificDate.HasValue)
                .ToListAsync();

            return shifts.Any(s => s.IsTimeWithinShift(dateTime));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if time {DateTime} is within operating hours", dateTime);
            return false;
        }
    }

    public async Task<List<OperatingShift>> GetConflictingShiftsAsync(OperatingShift shift)
    {
        try
        {
            var conflictingShifts = new List<OperatingShift>();

            if (shift.SpecificDate.HasValue)
            {
                // Check conflicts with other specific date shifts
                conflictingShifts = await _context.OperatingShifts
                    .Where(s => s.SpecificDate.HasValue && 
                               s.SpecificDate.Value.Date == shift.SpecificDate.Value.Date &&
                               s.IsActive)
                    .ToListAsync();
            }
            else
            {
                // Check conflicts with same day of week shifts
                conflictingShifts = await _context.OperatingShifts
                    .Where(s => s.DayOfWeek == shift.DayOfWeek && 
                               !s.SpecificDate.HasValue &&
                               s.IsActive)
                    .ToListAsync();
            }

            // Filter for actual time overlaps
            return conflictingShifts.Where(s => ShiftsOverlap(shift, s)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for conflicting shifts");
            return new List<OperatingShift>();
        }
    }

    public async Task<TimeSpan> GetShiftDurationAsync(int shiftId)
    {
        try
        {
            var shift = await GetShiftAsync(shiftId);
            if (shift == null)
                return TimeSpan.Zero;

            return TimeSpan.FromHours(shift.DurationHours);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shift duration for {ShiftId}", shiftId);
            return TimeSpan.Zero;
        }
    }

    public async Task<List<OperatingShift>> GetHolidayShiftsAsync()
    {
        try
        {
            return await _context.OperatingShifts
                .Where(s => s.IsHoliday)
                .OrderBy(s => s.SpecificDate ?? DateTime.MaxValue)
                .ThenBy(s => s.DayOfWeek)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving holiday shifts");
            return new List<OperatingShift>();
        }
    }

    private bool ShiftsOverlap(OperatingShift shift1, OperatingShift shift2)
    {
        // Convert times to minutes for easier comparison
        var start1 = shift1.StartTime.TotalMinutes;
        var end1 = shift1.EndTime.TotalMinutes;
        var start2 = shift2.StartTime.TotalMinutes;
        var end2 = shift2.EndTime.TotalMinutes;

        // Handle shifts that cross midnight
        if (end1 < start1) end1 += 24 * 60; // Add 24 hours
        if (end2 < start2) end2 += 24 * 60; // Add 24 hours

        // Check for overlap
        return !(end1 <= start2 || end2 <= start1);
    }
}

/// <summary>
/// Default operating shifts for common work schedules
/// </summary>
public static class DefaultOperatingShifts
{
    public static List<OperatingShift> GetStandardBusinessHours()
    {
        var shifts = new List<OperatingShift>();

        // Monday through Friday, 8 AM to 5 PM
        for (int day = 1; day <= 5; day++) // Monday = 1, Friday = 5
        {
            shifts.Add(new OperatingShift
            {
                DayOfWeek = day,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(17, 0, 0),
                IsActive = true,
                IsHoliday = false,
                Description = "Standard Business Hours",
                CreatedBy = "System"
            });
        }

        return shifts;
    }

    public static List<OperatingShift> Get24x7Schedule()
    {
        var shifts = new List<OperatingShift>();

        // 24/7 operation, all days
        for (int day = 0; day <= 6; day++) // Sunday = 0, Saturday = 6
        {
            shifts.Add(new OperatingShift
            {
                DayOfWeek = day,
                StartTime = new TimeSpan(0, 0, 0),
                EndTime = new TimeSpan(23, 59, 59),
                IsActive = true,
                IsHoliday = false,
                Description = "24/7 Operations",
                CreatedBy = "System"
            });
        }

        return shifts;
    }

    public static List<OperatingShift> GetTwoShiftSchedule()
    {
        var shifts = new List<OperatingShift>();

        // Monday through Friday, two shifts
        for (int day = 1; day <= 5; day++)
        {
            // First shift: 6 AM to 2 PM
            shifts.Add(new OperatingShift
            {
                DayOfWeek = day,
                StartTime = new TimeSpan(6, 0, 0),
                EndTime = new TimeSpan(14, 0, 0),
                IsActive = true,
                IsHoliday = false,
                Description = "First Shift",
                CreatedBy = "System"
            });

            // Second shift: 2 PM to 10 PM
            shifts.Add(new OperatingShift
            {
                DayOfWeek = day,
                StartTime = new TimeSpan(14, 0, 0),
                EndTime = new TimeSpan(22, 0, 0),
                IsActive = true,
                IsHoliday = false,
                Description = "Second Shift",
                CreatedBy = "System"
            });
        }

        return shifts;
    }
}