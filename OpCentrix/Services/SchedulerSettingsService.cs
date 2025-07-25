using OpCentrix.Models;
using OpCentrix.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace OpCentrix.Services
{
    public interface ISchedulerSettingsService
    {
        Task<SchedulerSettings> GetSettingsAsync();
        Task<bool> UpdateSettingsAsync(SchedulerSettings settings);
        Task<bool> UpdateSettingsAsync(SchedulerSettings settings, string username);
        Task<bool> IsWeekendOperationAllowedAsync(DateTime date);
        Task<bool> IsOperatorAvailableAsync(DateTime start, DateTime end);
        Task<double> GetChangeoverTimeAsync(string fromMaterial, string toMaterial);
        Task<bool> IsSaturdayOperationAllowedAsync();
        Task<bool> IsSundayOperationAllowedAsync();
        Task<SchedulerSettings> GetOrCreateDefaultSettingsAsync();
        Task<SchedulerSettings> ResetToDefaultsAsync(string username);
    }

    public class SchedulerSettingsService : ISchedulerSettingsService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<SchedulerSettingsService> _logger;
        private SchedulerSettings? _cachedSettings;
        private DateTime _lastCacheTime = DateTime.MinValue;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public SchedulerSettingsService(SchedulerContext context, ILogger<SchedulerSettingsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SchedulerSettings> GetSettingsAsync()
        {
            try
            {
                // Check cache first
                if (_cachedSettings != null && 
                    DateTime.UtcNow - _lastCacheTime < _cacheExpiration)
                {
                    return _cachedSettings;
                }

                var settings = await _context.SchedulerSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (settings == null)
                {
                    _logger.LogWarning("No scheduler settings found in database, creating defaults");
                    settings = await GetOrCreateDefaultSettingsAsync();
                }

                // Update cache
                _cachedSettings = settings;
                _lastCacheTime = DateTime.UtcNow;

                _logger.LogDebug("Scheduler settings loaded: EnableWeekendOperations={EnableWeekend}, SaturdayOperations={Saturday}, SundayOperations={Sunday}",
                    settings.EnableWeekendOperations, settings.SaturdayOperations, settings.SundayOperations);

                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading scheduler settings, using fallback defaults");
                return CreateFallbackSettings();
            }
        }

        public async Task<bool> UpdateSettingsAsync(SchedulerSettings settings)
        {
            return await UpdateSettingsAsync(settings, "System");
        }

        public async Task<bool> UpdateSettingsAsync(SchedulerSettings settings, string username)
        {
            try
            {
                var existing = await _context.SchedulerSettings.FirstOrDefaultAsync();
                
                if (existing == null)
                {
                    // Create new settings record
                    settings.Id = 1;
                    settings.CreatedDate = DateTime.UtcNow;
                    settings.CreatedBy = username;
                    settings.LastModifiedBy = username;
                    settings.LastModifiedDate = DateTime.UtcNow;
                    _context.SchedulerSettings.Add(settings);
                }
                else
                {
                    // Update existing settings
                    UpdateSettingsEntity(existing, settings);
                    existing.LastModifiedBy = username;
                    existing.LastModifiedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Clear cache to force reload
                _cachedSettings = null;

                _logger.LogInformation("Scheduler settings updated successfully by {Username}: EnableWeekendOperations={EnableWeekend}, SaturdayOperations={Saturday}, SundayOperations={Sunday}",
                    username, settings.EnableWeekendOperations, settings.SaturdayOperations, settings.SundayOperations);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating scheduler settings");
                return false;
            }
        }

        public async Task<SchedulerSettings> ResetToDefaultsAsync(string username)
        {
            try
            {
                var defaultSettings = CreateDefaultSettings();
                defaultSettings.LastModifiedBy = username;
                defaultSettings.ChangeNotes = $"Reset to defaults by {username}";
                
                await UpdateSettingsAsync(defaultSettings, username);
                return defaultSettings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting scheduler settings to defaults");
                return CreateFallbackSettings();
            }
        }

        public async Task<bool> IsWeekendOperationAllowedAsync(DateTime date)
        {
            try
            {
                var settings = await GetSettingsAsync();
                
                // If weekend operations are disabled globally, no weekend work
                if (!settings.EnableWeekendOperations)
                {
                    var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                    _logger.LogDebug("Weekend operations disabled globally. Date {Date} is weekend: {IsWeekend}, allowed: {Allowed}",
                        date.ToString("yyyy-MM-dd"), isWeekend, !isWeekend);
                    return !isWeekend;
                }

                // Weekend operations are enabled, check specific days
                var dayOfWeek = date.DayOfWeek;
                bool allowed;

                switch (dayOfWeek)
                {
                    case DayOfWeek.Saturday:
                        allowed = settings.SaturdayOperations;
                        break;
                    case DayOfWeek.Sunday:
                        allowed = settings.SundayOperations;
                        break;
                    default:
                        allowed = true; // Weekdays are always allowed
                        break;
                }

                _logger.LogDebug("Weekend operation check for {Date} ({DayOfWeek}): allowed={Allowed}", 
                    date.ToString("yyyy-MM-dd"), dayOfWeek, allowed);

                return allowed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking weekend operations for date {Date}", date);
                // Fail safe: default to weekdays only
                return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
            }
        }

        public async Task<bool> IsSaturdayOperationAllowedAsync()
        {
            try
            {
                var settings = await GetSettingsAsync();
                return settings.EnableWeekendOperations && settings.SaturdayOperations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Saturday operations");
                return false;
            }
        }

        public async Task<bool> IsSundayOperationAllowedAsync()
        {
            try
            {
                var settings = await GetSettingsAsync();
                return settings.EnableWeekendOperations && settings.SundayOperations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Sunday operations");
                return false;
            }
        }

        public async Task<bool> IsOperatorAvailableAsync(DateTime start, DateTime end)
        {
            try
            {
                var settings = await GetSettingsAsync();
                
                // Check if start and end times fall within any shift
                var startTime = start.TimeOfDay;
                var endTime = end.TimeOfDay;

                // Check all three shifts
                bool isInStandardShift = IsTimeInShift(startTime, settings.StandardShiftStart, settings.StandardShiftEnd) &&
                                        IsTimeInShift(endTime, settings.StandardShiftStart, settings.StandardShiftEnd);

                bool isInEveningShift = IsTimeInShift(startTime, settings.EveningShiftStart, settings.EveningShiftEnd) &&
                                       IsTimeInShift(endTime, settings.EveningShiftStart, settings.EveningShiftEnd);

                bool isInNightShift = IsTimeInShift(startTime, settings.NightShiftStart, settings.NightShiftEnd) &&
                                     IsTimeInShift(endTime, settings.NightShiftStart, settings.NightShiftEnd);

                var available = isInStandardShift || isInEveningShift || isInNightShift;

                _logger.LogDebug("Operator availability check: {Start} - {End}, available: {Available}",
                    start.ToString("yyyy-MM-dd HH:mm"), end.ToString("yyyy-MM-dd HH:mm"), available);

                return available;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking operator availability for {Start} - {End}", start, end);
                return true; // Default to available if check fails
            }
        }

        public async Task<double> GetChangeoverTimeAsync(string fromMaterial, string toMaterial)
        {
            try
            {
                if (string.IsNullOrEmpty(fromMaterial) || string.IsNullOrEmpty(toMaterial))
                    return 0;

                if (fromMaterial == toMaterial)
                    return 0;

                var settings = await GetSettingsAsync();

                // Material-specific changeover times
                var titaniumMaterials = new[] { "Ti-6Al-4V Grade 5", "Ti-6Al-4V ELI Grade 23" };
                var inconelMaterials = new[] { "Inconel 718", "Inconel 625" };

                bool isFromTitanium = titaniumMaterials.Contains(fromMaterial);
                bool isToTitanium = titaniumMaterials.Contains(toMaterial);
                bool isFromInconel = inconelMaterials.Contains(fromMaterial);
                bool isToInconel = inconelMaterials.Contains(toMaterial);

                double changeoverTime;

                if (isFromTitanium && isToTitanium)
                {
                    // Titanium to Titanium
                    changeoverTime = settings.TitaniumToTitaniumChangeoverMinutes;
                }
                else if (isFromInconel && isToInconel)
                {
                    // Inconel to Inconel
                    changeoverTime = settings.InconelToInconelChangeoverMinutes;
                }
                else if ((isFromTitanium && isToInconel) || (isFromInconel && isToTitanium))
                {
                    // Cross-material changeover (Ti to In or In to Ti)
                    changeoverTime = settings.CrossMaterialChangeoverMinutes;
                }
                else
                {
                    // Default changeover for unknown materials
                    changeoverTime = settings.DefaultMaterialChangeoverMinutes;
                }

                _logger.LogDebug("Changeover time from {FromMaterial} to {ToMaterial}: {ChangeoverTime} minutes",
                    fromMaterial, toMaterial, changeoverTime);

                return changeoverTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating changeover time from {FromMaterial} to {ToMaterial}", fromMaterial, toMaterial);
                return 60; // Default 1 hour
            }
        }

        public async Task<SchedulerSettings> GetOrCreateDefaultSettingsAsync()
        {
            try
            {
                var existing = await _context.SchedulerSettings.FirstOrDefaultAsync();
                if (existing != null)
                    return existing;

                var defaultSettings = CreateDefaultSettings();
                
                _context.SchedulerSettings.Add(defaultSettings);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created default scheduler settings with weekend operations enabled");
                return defaultSettings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default scheduler settings");
                return CreateFallbackSettings();
            }
        }

        private static bool IsTimeInShift(TimeSpan time, TimeSpan shiftStart, TimeSpan shiftEnd)
        {
            if (shiftStart <= shiftEnd)
            {
                // Normal shift (e.g., 7:00 - 15:00)
                return time >= shiftStart && time <= shiftEnd;
            }
            else
            {
                // Night shift crossing midnight (e.g., 23:00 - 07:00)
                return time >= shiftStart || time <= shiftEnd;
            }
        }

        private static SchedulerSettings CreateDefaultSettings()
        {
            return new SchedulerSettings
            {
                Id = 1,
                // CRITICAL FIX: Enable weekend operations by default
                EnableWeekendOperations = true,
                SaturdayOperations = true,
                SundayOperations = true,
                
                // Shift times
                StandardShiftStart = TimeSpan.FromHours(7),   // 7:00 AM
                StandardShiftEnd = TimeSpan.FromHours(15),    // 3:00 PM
                EveningShiftStart = TimeSpan.FromHours(15),   // 3:00 PM
                EveningShiftEnd = TimeSpan.FromHours(23),     // 11:00 PM
                NightShiftStart = TimeSpan.FromHours(23),     // 11:00 PM
                NightShiftEnd = TimeSpan.FromHours(7),        // 7:00 AM (next day)
                
                // Changeover times
                TitaniumToTitaniumChangeoverMinutes = 30,
                InconelToInconelChangeoverMinutes = 45,
                CrossMaterialChangeoverMinutes = 120,
                DefaultMaterialChangeoverMinutes = 60,
                
                // Processing times
                DefaultPreheatingTimeMinutes = 60,
                DefaultCoolingTimeMinutes = 240,
                DefaultPostProcessingTimeMinutes = 90,
                SetupTimeBufferMinutes = 30,
                
                // Machine priorities
                TI1MachinePriority = 5,
                TI2MachinePriority = 5,
                INCMachinePriority = 5,
                
                // Job constraints
                MaxJobsPerMachinePerDay = 8,
                MinimumTimeBetweenJobsMinutes = 15,
                AdvanceWarningTimeMinutes = 60,
                
                // Quality and safety
                RequiredOperatorCertification = "SLS Basic",
                AllowConcurrentJobs = true,
                QualityCheckRequired = true,
                EmergencyOverrideEnabled = true,
                NotifyOnScheduleConflicts = true,
                NotifyOnMaterialChanges = true,
                
                // Audit fields
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = "System",
                LastModifiedBy = "System",
                ChangeNotes = "Default scheduler settings with weekend operations enabled"
            };
        }

        private static SchedulerSettings CreateFallbackSettings()
        {
            var fallback = CreateDefaultSettings();
            // For fallback, be more conservative
            fallback.EnableWeekendOperations = false;
            fallback.SaturdayOperations = false;
            fallback.SundayOperations = false;
            fallback.ChangeNotes = "Fallback settings due to database error";
            return fallback;
        }

        private static void UpdateSettingsEntity(SchedulerSettings existing, SchedulerSettings updated)
        {
            // Weekend operations
            existing.EnableWeekendOperations = updated.EnableWeekendOperations;
            existing.SaturdayOperations = updated.SaturdayOperations;
            existing.SundayOperations = updated.SundayOperations;
            
            // Shift times
            existing.StandardShiftStart = updated.StandardShiftStart;
            existing.StandardShiftEnd = updated.StandardShiftEnd;
            existing.EveningShiftStart = updated.EveningShiftStart;
            existing.EveningShiftEnd = updated.EveningShiftEnd;
            existing.NightShiftStart = updated.NightShiftStart;
            existing.NightShiftEnd = updated.NightShiftEnd;
            
            // Changeover times
            existing.TitaniumToTitaniumChangeoverMinutes = updated.TitaniumToTitaniumChangeoverMinutes;
            existing.InconelToInconelChangeoverMinutes = updated.InconelToInconelChangeoverMinutes;
            existing.CrossMaterialChangeoverMinutes = updated.CrossMaterialChangeoverMinutes;
            existing.DefaultMaterialChangeoverMinutes = updated.DefaultMaterialChangeoverMinutes;
            
            // Processing times
            existing.DefaultPreheatingTimeMinutes = updated.DefaultPreheatingTimeMinutes;
            existing.DefaultCoolingTimeMinutes = updated.DefaultCoolingTimeMinutes;
            existing.DefaultPostProcessingTimeMinutes = updated.DefaultPostProcessingTimeMinutes;
            existing.SetupTimeBufferMinutes = updated.SetupTimeBufferMinutes;
            
            // Machine priorities
            existing.TI1MachinePriority = updated.TI1MachinePriority;
            existing.TI2MachinePriority = updated.TI2MachinePriority;
            existing.INCMachinePriority = updated.INCMachinePriority;
            
            // Job constraints
            existing.MaxJobsPerMachinePerDay = updated.MaxJobsPerMachinePerDay;
            existing.MinimumTimeBetweenJobsMinutes = updated.MinimumTimeBetweenJobsMinutes;
            existing.AdvanceWarningTimeMinutes = updated.AdvanceWarningTimeMinutes;
            
            // Quality and safety
            existing.RequiredOperatorCertification = updated.RequiredOperatorCertification;
            existing.AllowConcurrentJobs = updated.AllowConcurrentJobs;
            existing.QualityCheckRequired = updated.QualityCheckRequired;
            existing.EmergencyOverrideEnabled = updated.EmergencyOverrideEnabled;
            existing.NotifyOnScheduleConflicts = updated.NotifyOnScheduleConflicts;
            existing.NotifyOnMaterialChanges = updated.NotifyOnMaterialChanges;
            
            // Notes
            existing.ChangeNotes = updated.ChangeNotes ?? "Settings updated";
        }
    }
}