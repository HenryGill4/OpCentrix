using OpCentrix.Data;
using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Services
{
    /// <summary>
    /// Service for managing scheduler configuration settings
    /// </summary>
    public interface ISchedulerSettingsService
    {
        Task<SchedulerSettings> GetSettingsAsync();
        Task<SchedulerSettings> UpdateSettingsAsync(SchedulerSettings settings, string modifiedBy);
        Task<SchedulerSettings> ResetToDefaultsAsync(string modifiedBy);
        Task<int> GetChangeoverTimeAsync(string fromMaterial, string toMaterial);
        Task<int> CalculateChangeoverTimeAsync(string fromMaterial, string toMaterial);
        Task<bool> IsOperatorAvailableAsync(DateTime startTime, DateTime endTime);
        Task<int> GetMachinePriorityAsync(string machineId);
    }

    public class SchedulerSettingsService : ISchedulerSettingsService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<SchedulerSettingsService> _logger;
        private SchedulerSettings? _cachedSettings;
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

        public SchedulerSettingsService(SchedulerContext context, ILogger<SchedulerSettingsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get current scheduler settings (cached for performance)
        /// </summary>
        public async Task<SchedulerSettings> GetSettingsAsync()
        {
            try
            {
                // Check cache first
                if (_cachedSettings != null && DateTime.UtcNow - _lastCacheUpdate < _cacheTimeout)
                {
                    return _cachedSettings;
                }

                // Try to load from database
                SchedulerSettings? settings = null;
                try
                {
                    settings = await _context.SchedulerSettings.FirstOrDefaultAsync();
                }
                catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("no such table"))
                {
                    _logger.LogWarning("SchedulerSettings table does not exist. Creating table and using default settings.");
                    
                    // Ensure database is created and migrations are applied
                    try
                    {
                        await _context.Database.EnsureCreatedAsync();
                        
                        // Try to apply pending migrations
                        var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                        if (pendingMigrations.Any())
                        {
                            _logger.LogInformation("Applying {MigrationCount} pending migrations", pendingMigrations.Count());
                            await _context.Database.MigrateAsync();
                        }
                        
                        // Try to load settings again after migration
                        settings = await _context.SchedulerSettings.FirstOrDefaultAsync();
                    }
                    catch (Exception migrationEx)
                    {
                        _logger.LogError(migrationEx, "Failed to apply migrations. Using default settings.");
                    }
                }

                if (settings == null)
                {
                    // Create default settings
                    settings = CreateDefaultSettings();
                    
                    try
                    {
                        _context.SchedulerSettings.Add(settings);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Created default scheduler settings");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save default settings to database. Using in-memory defaults.");
                        // Continue with in-memory settings if database save fails
                    }
                }

                // Update cache
                _cachedSettings = settings;
                _lastCacheUpdate = DateTime.UtcNow;

                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading scheduler settings");
                
                // Return default settings as fallback
                var defaultSettings = CreateDefaultSettings();
                _cachedSettings = defaultSettings;
                _lastCacheUpdate = DateTime.UtcNow;
                return defaultSettings;
            }
        }

        /// <summary>
        /// Update scheduler settings
        /// </summary>
        public async Task<SchedulerSettings> UpdateSettingsAsync(SchedulerSettings settings, string modifiedBy)
        {
            try
            {
                var existingSettings = await _context.SchedulerSettings.FirstOrDefaultAsync();

                if (existingSettings == null)
                {
                    // Create new settings
                    settings.CreatedBy = modifiedBy;
                    settings.CreatedDate = DateTime.UtcNow;
                    settings.LastModifiedBy = modifiedBy;
                    settings.LastModifiedDate = DateTime.UtcNow;
                    
                    _context.SchedulerSettings.Add(settings);
                }
                else
                {
                    // Update existing settings
                    UpdateSettingsProperties(existingSettings, settings);
                    existingSettings.LastModifiedBy = modifiedBy;
                    existingSettings.LastModifiedDate = DateTime.UtcNow;
                    existingSettings.ChangeNotes = settings.ChangeNotes;
                }

                await _context.SaveChangesAsync();

                // Clear cache to force reload
                _cachedSettings = null;

                _logger.LogInformation("Scheduler settings updated by {ModifiedBy}", modifiedBy);

                return await GetSettingsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating scheduler settings");
                throw;
            }
        }

        /// <summary>
        /// Reset settings to defaults
        /// </summary>
        public async Task<SchedulerSettings> ResetToDefaultsAsync(string modifiedBy)
        {
            try
            {
                var existingSettings = await _context.SchedulerSettings.FirstOrDefaultAsync();

                if (existingSettings != null)
                {
                    _context.SchedulerSettings.Remove(existingSettings);
                }

                var defaultSettings = CreateDefaultSettings();
                defaultSettings.CreatedBy = modifiedBy;
                defaultSettings.LastModifiedBy = modifiedBy;
                defaultSettings.ChangeNotes = "Reset to default values";

                _context.SchedulerSettings.Add(defaultSettings);
                await _context.SaveChangesAsync();

                // Clear cache
                _cachedSettings = null;

                _logger.LogInformation("Scheduler settings reset to defaults by {ModifiedBy}", modifiedBy);

                return defaultSettings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting scheduler settings");
                throw;
            }
        }

        /// <summary>
        /// Get changeover time between materials
        /// </summary>
        public async Task<int> GetChangeoverTimeAsync(string fromMaterial, string toMaterial)
        {
            try
            {
                var settings = await GetSettingsAsync();
                return settings.GetChangeoverTime(fromMaterial, toMaterial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting changeover time for materials {FromMaterial} to {ToMaterial}", 
                    fromMaterial, toMaterial);
                return 60; // Default fallback
            }
        }

        /// <summary>
        /// Calculate changeover time between materials (alias for GetChangeoverTimeAsync)
        /// </summary>
        public async Task<int> CalculateChangeoverTimeAsync(string fromMaterial, string toMaterial)
        {
            return await GetChangeoverTimeAsync(fromMaterial, toMaterial);
        }

        /// <summary>
        /// Check if operator is available during specified time
        /// </summary>
        public async Task<bool> IsOperatorAvailableAsync(DateTime startTime, DateTime endTime)
        {
            try
            {
                var settings = await GetSettingsAsync();
                return settings.IsOperatorAvailable(startTime, endTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking operator availability for time {StartTime} to {EndTime}", 
                    startTime, endTime);
                return true; // Default to available
            }
        }

        /// <summary>
        /// Get machine priority
        /// </summary>
        public async Task<int> GetMachinePriorityAsync(string machineId)
        {
            try
            {
                var settings = await GetSettingsAsync();
                return settings.GetMachinePriority(machineId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting machine priority for {MachineId}", machineId);
                return 5; // Default priority
            }
        }

        #region Private Methods

        /// <summary>
        /// Create default scheduler settings
        /// </summary>
        private SchedulerSettings CreateDefaultSettings()
        {
            return new SchedulerSettings
            {
                // Material changeover defaults
                TitaniumToTitaniumChangeoverMinutes = 30,
                InconelToInconelChangeoverMinutes = 45,
                CrossMaterialChangeoverMinutes = 120,
                DefaultMaterialChangeoverMinutes = 60,

                // Timing defaults
                DefaultPreheatingTimeMinutes = 60,
                DefaultCoolingTimeMinutes = 240,
                DefaultPostProcessingTimeMinutes = 90,
                SetupTimeBufferMinutes = 30,

                // Operator schedule defaults (24/7 operations)
                StandardShiftStart = new TimeSpan(7, 0, 0),
                StandardShiftEnd = new TimeSpan(15, 0, 0),
                EveningShiftStart = new TimeSpan(15, 0, 0),
                EveningShiftEnd = new TimeSpan(23, 0, 0),
                NightShiftStart = new TimeSpan(23, 0, 0),
                NightShiftEnd = new TimeSpan(7, 0, 0),
                EnableWeekendOperations = false,
                SaturdayOperations = false,
                SundayOperations = false,

                // Machine defaults
                TI1MachinePriority = 5,
                TI2MachinePriority = 5,
                INCMachinePriority = 5,
                AllowConcurrentJobs = true,
                MaxJobsPerMachinePerDay = 8,

                // Quality and safety defaults
                RequiredOperatorCertification = "SLS Basic",
                QualityCheckRequired = true,
                MinimumTimeBetweenJobsMinutes = 15,
                EmergencyOverrideEnabled = true,

                // Notification defaults
                NotifyOnScheduleConflicts = true,
                NotifyOnMaterialChanges = true,
                AdvanceWarningTimeMinutes = 60,

                // Audit fields
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = "System",
                LastModifiedBy = "System",
                ChangeNotes = "Default settings initialization"
            };
        }

        /// <summary>
        /// Update settings properties from source to target
        /// </summary>
        private void UpdateSettingsProperties(SchedulerSettings target, SchedulerSettings source)
        {
            // Material changeover settings
            target.TitaniumToTitaniumChangeoverMinutes = source.TitaniumToTitaniumChangeoverMinutes;
            target.InconelToInconelChangeoverMinutes = source.InconelToInconelChangeoverMinutes;
            target.CrossMaterialChangeoverMinutes = source.CrossMaterialChangeoverMinutes;
            target.DefaultMaterialChangeoverMinutes = source.DefaultMaterialChangeoverMinutes;

            // Timing settings
            target.DefaultPreheatingTimeMinutes = source.DefaultPreheatingTimeMinutes;
            target.DefaultCoolingTimeMinutes = source.DefaultCoolingTimeMinutes;
            target.DefaultPostProcessingTimeMinutes = source.DefaultPostProcessingTimeMinutes;
            target.SetupTimeBufferMinutes = source.SetupTimeBufferMinutes;

            // Operator schedule settings
            target.StandardShiftStart = source.StandardShiftStart;
            target.StandardShiftEnd = source.StandardShiftEnd;
            target.EveningShiftStart = source.EveningShiftStart;
            target.EveningShiftEnd = source.EveningShiftEnd;
            target.NightShiftStart = source.NightShiftStart;
            target.NightShiftEnd = source.NightShiftEnd;
            target.EnableWeekendOperations = source.EnableWeekendOperations;
            target.SaturdayOperations = source.SaturdayOperations;
            target.SundayOperations = source.SundayOperations;

            // Machine settings
            target.TI1MachinePriority = source.TI1MachinePriority;
            target.TI2MachinePriority = source.TI2MachinePriority;
            target.INCMachinePriority = source.INCMachinePriority;
            target.AllowConcurrentJobs = source.AllowConcurrentJobs;
            target.MaxJobsPerMachinePerDay = source.MaxJobsPerMachinePerDay;

            // Quality and safety settings
            target.RequiredOperatorCertification = source.RequiredOperatorCertification;
            target.QualityCheckRequired = source.QualityCheckRequired;
            target.MinimumTimeBetweenJobsMinutes = source.MinimumTimeBetweenJobsMinutes;
            target.EmergencyOverrideEnabled = source.EmergencyOverrideEnabled;

            // Notification settings
            target.NotifyOnScheduleConflicts = source.NotifyOnScheduleConflicts;
            target.NotifyOnMaterialChanges = source.NotifyOnMaterialChanges;
            target.AdvanceWarningTimeMinutes = source.AdvanceWarningTimeMinutes;
        }

        #endregion
    }
}