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
    Task<List<OperatingShift>> GetShiftsForDayAsync(DayOfWeek dayOfWeek, string? machineId);
    Task<OperatingShift?> GetShiftAsync(int id);
    Task<bool> CreateShiftAsync(OperatingShift shift);
    Task<bool> UpdateShiftAsync(OperatingShift shift);
    Task<bool> DeleteShiftAsync(int id);
    Task<bool> IsTimeWithinOperatingHoursAsync(DateTime dateTime);
    Task<bool> IsTimeWithinOperatingHoursAsync(DateTime dateTime, string? machineId);
    Task<List<OperatingShift>> GetConflictingShiftsAsync(OperatingShift shift);
    Task<TimeSpan> GetShiftDurationAsync(int shiftId);
    Task<List<OperatingShift>> GetHolidayShiftsAsync();
}

public class OperatingShiftService : IOperatingShiftService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<OperatingShiftService> _logger;
    private bool? _hasMachineIdColumn;

    public OperatingShiftService(SchedulerContext context, ILogger<OperatingShiftService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private async Task<bool> HasMachineIdColumnAsync()
    {
        if (_hasMachineIdColumn.HasValue) return _hasMachineIdColumn.Value;
        try
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "PRAGMA table_info('OperatingShifts');";
            using var reader = await cmd.ExecuteReaderAsync();
            var found = false;
            while (await reader.ReadAsync())
            {
                // PRAGMA table_info columns: cid, name, type, notnull, dflt_value, pk
                var colName = reader.GetString(1);
                if (string.Equals(colName, "MachineId", StringComparison.OrdinalIgnoreCase))
                {
                    found = true; break;
                }
            }
            _hasMachineIdColumn = found;
            return found;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check OperatingShifts schema; assuming no MachineId column");
            _hasMachineIdColumn = false;
            return false;
        }
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

    public async Task<List<OperatingShift>> GetShiftsForDayAsync(DayOfWeek dayOfWeek, string? machineId)
    {
        try
        {
            var dayNumber = (int)dayOfWeek;
            var hasMachineCol = await HasMachineIdColumnAsync();
            var query = _context.OperatingShifts.AsQueryable();
            query = query.Where(s => s.DayOfWeek == dayNumber && s.IsActive && !s.IsHoliday);
            if (hasMachineCol)
            {
                query = query.Where(s => s.MachineId == null || s.MachineId == "" || s.MachineId == machineId);
            }
            return await query.OrderBy(s => s.StartTime).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shifts for day {DayOfWeek} and machine {MachineId}", dayOfWeek, machineId);
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
            existing.MachineId = shift.MachineId; // ensure machine scope updates are saved
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

    public async Task<bool> IsTimeWithinOperatingHoursAsync(DateTime dateTime, string? machineId)
    {
        try
        {
            var dayOfWeek = (int)dateTime.DayOfWeek;
            var hasMachineCol = await HasMachineIdColumnAsync();
 
             // Machine-specific override for specific date
            var specificQuery = _context.OperatingShifts
                .Where(s => s.SpecificDate.HasValue && s.SpecificDate.Value.Date == dateTime.Date && s.IsActive);
            if (hasMachineCol)
            {
                specificQuery = specificQuery.Where(s => s.MachineId == null || s.MachineId == "" || s.MachineId == machineId);
            }
            var specificDateShift = await specificQuery.FirstOrDefaultAsync();

            if (specificDateShift != null)
            {
                return specificDateShift.IsTimeWithinShift(dateTime);
            }

            var query = _context.OperatingShifts
                .Where(s => s.DayOfWeek == dayOfWeek && s.IsActive && !s.IsHoliday && !s.SpecificDate.HasValue);
            if (hasMachineCol)
            {
                query = query.Where(s => s.MachineId == null || s.MachineId == "" || s.MachineId == machineId);
            }
            var shifts = await query.ToListAsync();

            return shifts.Any(s => s.IsTimeWithinShift(dateTime));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if time {DateTime} is within operating hours for machine {MachineId}", dateTime, machineId);
            return false;
        }
    }

    public async Task<List<OperatingShift>> GetConflictingShiftsAsync(OperatingShift shift)
    {
        try
        {
            var conflictingShifts = new List<OperatingShift>();
            var hasMachineCol = await HasMachineIdColumnAsync();
 
            

            if (shift.SpecificDate.HasValue)
            {
                // Check conflicts with other specific date shifts
                var q = _context.OperatingShifts
                    .Where(s => s.SpecificDate.HasValue && s.SpecificDate.Value.Date == shift.SpecificDate.Value.Date && s.IsActive);
                if (hasMachineCol)
                    q = q.Where(s => s.MachineId == shift.MachineId);
                conflictingShifts = await q.ToListAsync();
            }
            else
            {
                // Check conflicts with same day of week shifts
                var q = _context.OperatingShifts
                    .Where(s => s.DayOfWeek == shift.DayOfWeek && !s.SpecificDate.HasValue && s.IsActive);
                if (hasMachineCol)
                    q = q.Where(s => s.MachineId == shift.MachineId);
                conflictingShifts = await q.ToListAsync();
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
    public static List<OperatingShift> GetStandardBusinessHours(string? machineId = null)
    {
        var shifts = new List<OperatingShift>();

        // Monday through Friday, 8 AM to 5 PM
        for (int day = 1; day <= 5; day++) // Monday = 1, Friday = 5
        {
            shifts.Add(new OperatingShift
            {
                MachineId = machineId,
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

    public static List<OperatingShift> Get24x7Schedule(string? machineId = null)
    {
        var shifts = new List<OperatingShift>();

        // 24/7 operation, all days
        for (int day = 0; day <= 6; day++) // Sunday = 0, Saturday = 6
        {
            shifts.Add(new OperatingShift
            {
                MachineId = machineId,
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

    public static List<OperatingShift> GetPlantTwoShiftSchedule(string? machineId = null)
    {
        var shifts = new List<OperatingShift>();

        // Monday through Friday: 06:00–15:30 and 15:30–00:00 (cross-midnight)
        for (int day = 1; day <= 5; day++)
        {
            shifts.Add(new OperatingShift
            {
                MachineId = machineId,
                DayOfWeek = day,
                StartTime = new TimeSpan(6, 0, 0),
                EndTime = new TimeSpan(15, 30, 0),
                IsActive = true,
                IsHoliday = false,
                Description = "Day Shift",
                CreatedBy = "System"
            });

            shifts.Add(new OperatingShift
            {
                MachineId = machineId,
                DayOfWeek = day,
                StartTime = new TimeSpan(15, 30, 0),
                EndTime = new TimeSpan(0, 0, 0), // midnight
                IsActive = true,
                IsHoliday = false,
                Description = "Night Shift",
                CreatedBy = "System"
            });
        }

        // Saturday and Sunday: 06:00–18:00
        foreach (var day in new[] { 0, 6 }) // Sunday=0, Saturday=6
        {
            shifts.Add(new OperatingShift
            {
                MachineId = machineId,
                DayOfWeek = day,
                StartTime = new TimeSpan(6, 0, 0),
                EndTime = new TimeSpan(18, 0, 0),
                IsActive = true,
                IsHoliday = false,
                Description = "Weekend Shift",
                CreatedBy = "System"
            });
        }

        return shifts;
    }

    public static List<OperatingShift> GetTwoShiftSchedule(string? machineId = null)
    {
        // Keep legacy schedule for template compatibility; prefer GetPlantTwoShiftSchedule for current plant hours
        var shifts = new List<OperatingShift>();
        for (int day = 1; day <= 5; day++)
        {
            shifts.Add(new OperatingShift
            {
                MachineId = machineId,
                DayOfWeek = day,
                StartTime = new TimeSpan(6, 0, 0),
                EndTime = new TimeSpan(14, 0, 0),
                IsActive = true,
                IsHoliday = false,
                Description = "First Shift",
                CreatedBy = "System"
            });
            shifts.Add(new OperatingShift
            {
                MachineId = machineId,
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