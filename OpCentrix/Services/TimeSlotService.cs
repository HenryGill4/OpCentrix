using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;

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

        public TimeSlotService(SchedulerContext context, ILogger<TimeSlotService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DateTime> GetNextAvailableTimeAsync(string machineId, DateTime? preferredStart = null, double durationHours = 8.0)
        {
            try
            {
                var searchStart = preferredStart ?? DateTime.UtcNow;
                
                // Ensure we start from a reasonable time (business hours)
                searchStart = RoundToNextBusinessHour(searchStart);
                
                // Look up to 30 days ahead for an available slot
                var maxSearchDate = searchStart.AddDays(30);
                
                _logger.LogDebug("?? [TIME-SLOT] Searching for {Duration}h slot on {Machine} starting from {StartTime}", 
                    durationHours, machineId, searchStart);

                while (searchStart < maxSearchDate)
                {
                    var proposedEndTime = searchStart.AddHours(durationHours);
                    
                    // Check if this time slot is available
                    if (await IsTimeSlotAvailableAsync(machineId, searchStart, proposedEndTime))
                    {
                        _logger.LogInformation("? [TIME-SLOT] Found available slot for {Machine}: {StartTime} to {EndTime}", 
                            machineId, searchStart, proposedEndTime);
                        return searchStart;
                    }
                    
                    // Move to next hour and try again
                    searchStart = searchStart.AddHours(1);
                    
                    // Skip non-business hours
                    searchStart = RoundToNextBusinessHour(searchStart);
                }
                
                _logger.LogWarning("?? [TIME-SLOT] No available slot found for {Machine} within 30 days", machineId);
                
                // Fallback: return next business hour even if there might be conflicts
                return RoundToNextBusinessHour(preferredStart ?? DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [TIME-SLOT] Error finding next available time for {Machine}", machineId);
                
                // Fallback to preferred start or now
                return RoundToNextBusinessHour(preferredStart ?? DateTime.UtcNow);
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

                var currentTime = RoundToNextBusinessHour(startDate);
                
                while (currentTime.Date <= endDate.Date)
                {
                    var proposedEndTime = currentTime.AddHours(durationHours);
                    
                    // Check if this slot conflicts with any existing job
                    var hasConflict = existingJobs.Any(job => 
                        job.ScheduledStart < proposedEndTime && job.ScheduledEnd > currentTime);
                    
                    if (!hasConflict && IsBusinessHours(currentTime))
                    {
                        availableSlots.Add(new TimeSlot
                        {
                            StartTime = currentTime,
                            EndTime = proposedEndTime,
                            DurationHours = durationHours,
                            IsBusinessHours = true
                        });
                    }
                    
                    // Move to next hour
                    currentTime = currentTime.AddHours(1);
                    
                    // Skip to next business day if past business hours
                    if (!IsBusinessHours(currentTime))
                    {
                        currentTime = GetNextBusinessDay(currentTime);
                    }
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

        private DateTime RoundToNextBusinessHour(DateTime dateTime)
        {
            // Round up to next hour
            var rounded = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0);
            if (dateTime.Minute > 0 || dateTime.Second > 0)
            {
                rounded = rounded.AddHours(1);
            }
            
            // Ensure we're in business hours (6 AM to 6 PM)
            if (rounded.Hour < 6)
            {
                rounded = new DateTime(rounded.Year, rounded.Month, rounded.Day, 6, 0, 0);
            }
            else if (rounded.Hour >= 18)
            {
                // Move to next business day at 6 AM
                rounded = GetNextBusinessDay(rounded);
            }
            
            // Skip weekends
            while (rounded.DayOfWeek == DayOfWeek.Saturday || rounded.DayOfWeek == DayOfWeek.Sunday)
            {
                rounded = rounded.AddDays(1);
                rounded = new DateTime(rounded.Year, rounded.Month, rounded.Day, 6, 0, 0);
            }
            
            return rounded;
        }

        private DateTime GetNextBusinessDay(DateTime dateTime)
        {
            var nextDay = dateTime.Date.AddDays(1);
            
            // Skip weekends
            while (nextDay.DayOfWeek == DayOfWeek.Saturday || nextDay.DayOfWeek == DayOfWeek.Sunday)
            {
                nextDay = nextDay.AddDays(1);
            }
            
            return new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 6, 0, 0);
        }

        private bool IsBusinessHours(DateTime dateTime)
        {
            return dateTime.DayOfWeek != DayOfWeek.Saturday &&
                   dateTime.DayOfWeek != DayOfWeek.Sunday &&
                   dateTime.Hour >= 6 &&
                   dateTime.Hour < 18;
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