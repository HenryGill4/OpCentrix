using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Services.Admin;

namespace OpCentrix.Services
{
    public interface ITimeSlotService
    {
        Task<DateTime> GetNextAvailableTimeAsync(string machineId, DateTime? preferredStart = null, double durationHours = 8.0);
        Task<List<TimeSlot>> GetAvailableTimeSlotsAsync(string machineId, DateTime startDate, DateTime endDate, double durationHours = 8.0);
        Task<bool> IsTimeSlotAvailableAsync(string machineId, DateTime startTime, DateTime endTime, int? excludeJobId = null);
        Task<List<Job>> GetConflictingJobsAsync(string machineId, DateTime startTime, DateTime endTime, int? excludeJobId = null);
    }

    public class TimeSlotService : ITimeSlotService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<TimeSlotService> _logger;
        private readonly IOperatingShiftService _shiftService;
        // TEMP hard bypass flag to avoid shift lookups while DB is missing columns
        private const bool BYPASS_SHIFT_CHECKS = true;

        public TimeSlotService(SchedulerContext context, ILogger<TimeSlotService> logger, IOperatingShiftService shiftService)
        {
            _context = context;
            _logger = logger;
            _shiftService = shiftService;
        }

        public async Task<DateTime> GetNextAvailableTimeAsync(string machineId, DateTime? preferredStart = null, double durationHours = 8.0)
        {
            try
            {
                var searchStart = preferredStart ?? DateTime.UtcNow;
                searchStart = BYPASS_SHIFT_CHECKS ? RoundToNextHour(searchStart) : await RoundToNextOperatingHourAsync(machineId, searchStart);

                var maxSearchDate = searchStart.AddDays(30);
                var slotDuration = TimeSpan.FromHours(durationHours <= 0 ? 1 : durationHours);

                _logger.LogDebug("[TIME-SLOT] Scan for {Duration}h on {Machine} from {Start}", durationHours, machineId, searchStart);

                // Preload all jobs for the machine in the horizon once
                var jobs = await _context.Jobs
                    .Where(j => j.MachineId == machineId && j.ScheduledStart < maxSearchDate && j.ScheduledEnd > searchStart)
                    .OrderBy(j => j.ScheduledStart)
                    .AsNoTracking()
                    .ToListAsync();

                var idx = 0;
                var guard = 0;
                var candidate = searchStart;

                while (candidate < maxSearchDate && guard < 2000)
                {
                    var candidateEnd = candidate + slotDuration;

                    // Ensure within operating hours for start and end
                    if (!BYPASS_SHIFT_CHECKS && !await _shiftService.IsTimeWithinOperatingHoursAsync(candidate, machineId))
                    {
                        candidate = await RoundToNextOperatingHourAsync(machineId, candidate.AddHours(1));
                        guard++;
                        continue;
                    }
                    if (!BYPASS_SHIFT_CHECKS && !await _shiftService.IsTimeWithinOperatingHoursAsync(candidateEnd, machineId))
                    {
                        // Push to next valid time after end
                        candidate = await RoundToNextOperatingHourAsync(machineId, candidate.AddHours(1));
                        guard++;
                        continue;
                    }

                    // Advance idx to first potentially conflicting job
                    while (idx < jobs.Count && jobs[idx].ScheduledEnd <= candidate)
                    {
                        idx++;
                    }

                    // Check conflict with current or next job
                    var conflict = false;
                    if (idx < jobs.Count)
                    {
                        var j = jobs[idx];
                        if (j.ScheduledStart < candidateEnd && j.ScheduledEnd > candidate)
                        {
                            // Conflict: jump to end of this job and re-round
                            candidate = await RoundToNextOperatingHourAsync(machineId, j.ScheduledEnd);
                            guard++;
                            conflict = true;
                        }
                    }

                    if (!conflict)
                    {
                        _logger.LogDebug("[TIME-SLOT] Available slot {Start} - {End} on {Machine}", candidate, candidateEnd, machineId);
                        return candidate;
                    }
                }

                _logger.LogWarning("[TIME-SLOT] No slot found for {Machine} within horizon; returning next operating hour", machineId);
                return BYPASS_SHIFT_CHECKS ? RoundToNextHour(preferredStart ?? DateTime.UtcNow) : await RoundToNextOperatingHourAsync(machineId, preferredStart ?? DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TIME-SLOT] Error finding next available time for {Machine}", machineId);
                return BYPASS_SHIFT_CHECKS ? RoundToNextHour(preferredStart ?? DateTime.UtcNow) : await RoundToNextOperatingHourAsync(machineId, preferredStart ?? DateTime.UtcNow);
            }
        }

        public async Task<List<TimeSlot>> GetAvailableTimeSlotsAsync(string machineId, DateTime startDate, DateTime endDate, double durationHours = 8.0)
        {
            var availableSlots = new List<TimeSlot>();
            
            try
            {
                // Get all existing jobs for the machine in the date range
                var existingJobs = await _context.Jobs
                    .Where(j => j.MachineId == machineId &&
                               j.ScheduledStart < endDate &&
                               j.ScheduledEnd > startDate)
                    .OrderBy(j => j.ScheduledStart)
                    .AsNoTracking()
                    .ToListAsync();

                var currentTime = BYPASS_SHIFT_CHECKS ? RoundToNextHour(startDate) : await RoundToNextOperatingHourAsync(machineId, startDate);
                
                while (currentTime.Date <= endDate.Date)
                {
                    var proposedEndTime = currentTime.AddHours(durationHours);
                    
                    // Check if this slot conflicts with any existing job
                    var hasConflict = existingJobs.Any(job => 
                        job.ScheduledStart < proposedEndTime && job.ScheduledEnd > currentTime);
                    
                    if (!hasConflict && (BYPASS_SHIFT_CHECKS || await _shiftService.IsTimeWithinOperatingHoursAsync(currentTime, machineId)))
                    {
                        availableSlots.Add(new TimeSlot
                        {
                            StartTime = currentTime,
                            EndTime = proposedEndTime,
                            DurationHours = durationHours,
                            IsBusinessHours = true
                        });
                    }
                    
                    // Move to next hour, then coerce to next valid operating hour
                    currentTime = BYPASS_SHIFT_CHECKS ? RoundToNextHour(currentTime.AddHours(1)) : await RoundToNextOperatingHourAsync(machineId, currentTime.AddHours(1));
                 }
                
                _logger.LogDebug("?? [TIME-SLOT] Found {SlotCount} available slots for {Machine} from {Start} to {End}", 
                    availableSlots.Count, machineId, startDate.Date, endDate.Date);
                
                return availableSlots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [TIME-SLOT] Error getting available time slots for {Machine}", machineId);
                return availableSlots;
            }
        }

        public async Task<bool> IsTimeSlotAvailableAsync(string machineId, DateTime startTime, DateTime endTime, int? excludeJobId = null)
        {
            try
            {
                var conflictingJobs = await GetConflictingJobsAsync(machineId, startTime, endTime, excludeJobId);
                var isAvailable = !conflictingJobs.Any();
                
                _logger.LogDebug("?? [TIME-SLOT] Slot {StartTime}-{EndTime} on {Machine}: {Status} ({ConflictCount} conflicts)", 
                    startTime, endTime, machineId, isAvailable ? "AVAILABLE" : "BLOCKED", conflictingJobs.Count);
                
                return isAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [TIME-SLOT] Error checking time slot availability for {Machine}", machineId);
                return false; // Fail safe - assume not available if we can't check
            }
        }

        public async Task<List<Job>> GetConflictingJobsAsync(string machineId, DateTime startTime, DateTime endTime, int? excludeJobId = null)
        {
            try
            {
                var query = _context.Jobs
                    .Where(j => j.MachineId == machineId &&
                               j.ScheduledStart < endTime &&
                               j.ScheduledEnd > startTime);
                
                if (excludeJobId.HasValue)
                {
                    query = query.Where(j => j.Id != excludeJobId.Value);
                }
                
                var conflicts = await query
                    .Include(j => j.Part)
                    .AsNoTracking()
                    .ToListAsync();
                
                return conflicts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [TIME-SLOT] Error getting conflicting jobs for {Machine}", machineId);
                return new List<Job>();
            }
        }

        private async Task<DateTime> RoundToNextOperatingHourAsync(string machineId, DateTime dateTime)
        {
            // Round up to next hour
            var rounded = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0);
            if (dateTime.Minute > 0 || dateTime.Second > 0)
            {
                rounded = rounded.AddHours(1);
            }
 
            // Short-circuit: if no active shifts exist at all, treat all hours as valid
            if (!await _context.OperatingShifts.AnyAsync(s => s.IsActive))
            {
                return rounded;
            }
 
            // If not within operating hours for this machine, advance until it is
            int guard = 0;
            // Reduce loop work by advancing an hour at a time (safer for performance)
            while (!await _shiftService.IsTimeWithinOperatingHoursAsync(rounded, machineId) && guard < 96)
            {
                rounded = rounded.AddHours(1);
                guard++;
            }
 
             return rounded;
         }
        
        private static DateTime RoundToNextHour(DateTime dateTime)
        {
            var rounded = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0);
            if (dateTime.Minute > 0 || dateTime.Second > 0) rounded = rounded.AddHours(1);
            if (rounded.Hour < 6) rounded = rounded.Date.AddHours(8);
            return rounded;
        }
     }

    public class TimeSlot
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double DurationHours { get; set; }
        public bool IsBusinessHours { get; set; }
        public string DisplayText => $"{StartTime:MMM d, h:mm tt} - {EndTime:h:mm tt}";
    }
}