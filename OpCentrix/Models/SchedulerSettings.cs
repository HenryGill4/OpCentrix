using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Scheduler configuration settings for administrators
    /// Controls changeover times, cooldown periods, and operator schedules
    /// </summary>
    public class SchedulerSettings
    {
        public int Id { get; set; }

        #region Material Changeover Settings

        [Display(Name = "Titanium to Titanium Changeover (minutes)")]
        [Range(15, 180)]
        public int TitaniumToTitaniumChangeoverMinutes { get; set; } = 30;

        [Display(Name = "Inconel to Inconel Changeover (minutes)")]
        [Range(30, 180)]
        public int InconelToInconelChangeoverMinutes { get; set; } = 45;

        [Display(Name = "Cross-Material Changeover (minutes)")]
        [Range(60, 240)]
        public int CrossMaterialChangeoverMinutes { get; set; } = 120;

        [Display(Name = "Default Material Changeover (minutes)")]
        [Range(30, 120)]
        public int DefaultMaterialChangeoverMinutes { get; set; } = 60;

        #endregion

        #region Cooling and Timing Settings

        [Display(Name = "Default Preheating Time (minutes)")]
        [Range(30, 120)]
        public int DefaultPreheatingTimeMinutes { get; set; } = 60;

        [Display(Name = "Default Cooling Time (minutes)")]
        [Range(120, 600)]
        public int DefaultCoolingTimeMinutes { get; set; } = 240;

        [Display(Name = "Post-Processing Time (minutes)")]
        [Range(30, 180)]
        public int DefaultPostProcessingTimeMinutes { get; set; } = 90;

        [Display(Name = "Setup Time Buffer (minutes)")]
        [Range(15, 60)]
        public int SetupTimeBufferMinutes { get; set; } = 30;

        #endregion

        #region Operator Schedule Settings

        [Display(Name = "Standard Shift Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan StandardShiftStart { get; set; } = new TimeSpan(7, 0, 0); // 7:00 AM

        [Display(Name = "Standard Shift End Time")]
        [DataType(DataType.Time)]
        public TimeSpan StandardShiftEnd { get; set; } = new TimeSpan(15, 0, 0); // 3:00 PM

        [Display(Name = "Evening Shift Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan EveningShiftStart { get; set; } = new TimeSpan(15, 0, 0); // 3:00 PM

        [Display(Name = "Evening Shift End Time")]
        [DataType(DataType.Time)]
        public TimeSpan EveningShiftEnd { get; set; } = new TimeSpan(23, 0, 0); // 11:00 PM

        [Display(Name = "Night Shift Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan NightShiftStart { get; set; } = new TimeSpan(23, 0, 0); // 11:00 PM

        [Display(Name = "Night Shift End Time")]
        [DataType(DataType.Time)]
        public TimeSpan NightShiftEnd { get; set; } = new TimeSpan(7, 0, 0); // 7:00 AM

        [Display(Name = "Enable Weekend Operations")]
        public bool EnableWeekendOperations { get; set; } = false;

        [Display(Name = "Saturday Operating Hours")]
        public bool SaturdayOperations { get; set; } = false;

        [Display(Name = "Sunday Operating Hours")]
        public bool SundayOperations { get; set; } = false;

        #endregion

        #region Machine-Specific Settings

        [Display(Name = "TI1 Machine Priority")]
        [Range(1, 10)]
        public int TI1MachinePriority { get; set; } = 5;

        [Display(Name = "TI2 Machine Priority")]
        [Range(1, 10)]
        public int TI2MachinePriority { get; set; } = 5;

        [Display(Name = "INC Machine Priority")]
        [Range(1, 10)]
        public int INCMachinePriority { get; set; } = 5;

        [Display(Name = "Allow Concurrent Jobs on Same Machine")]
        public bool AllowConcurrentJobs { get; set; } = true;

        [Display(Name = "Maximum Jobs Per Machine Per Day")]
        [Range(1, 20)]
        public int MaxJobsPerMachinePerDay { get; set; } = 8;

        #endregion

        #region Quality and Safety Settings

        [Display(Name = "Required Operator Certification Level")]
        [StringLength(50)]
        public string RequiredOperatorCertification { get; set; } = "SLS Basic";

        [Display(Name = "Quality Check Required")]
        public bool QualityCheckRequired { get; set; } = true;

        [Display(Name = "Minimum Time Between Jobs (minutes)")]
        [Range(5, 60)]
        public int MinimumTimeBetweenJobsMinutes { get; set; } = 15;

        [Display(Name = "Emergency Override Enabled")]
        public bool EmergencyOverrideEnabled { get; set; } = true;

        #endregion

        #region Notification Settings

        [Display(Name = "Notify on Schedule Conflicts")]
        public bool NotifyOnScheduleConflicts { get; set; } = true;

        [Display(Name = "Notify on Material Changes")]
        public bool NotifyOnMaterialChanges { get; set; } = true;

        [Display(Name = "Advance Warning Time (minutes)")]
        [Range(30, 240)]
        public int AdvanceWarningTimeMinutes { get; set; } = 60;

        #endregion

        #region Audit Trail

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";

        [StringLength(100)]
        public string LastModifiedBy { get; set; } = "System";

        [StringLength(1000)]
        public string ChangeNotes { get; set; } = string.Empty;

        #endregion

        #region Computed Properties

        [NotMapped]
        public bool IsStandardHours => StandardShiftStart != default && StandardShiftEnd != default;

        [NotMapped]
        public double StandardShiftDurationHours => 
            (StandardShiftEnd - StandardShiftStart).TotalHours;

        [NotMapped]
        public bool IsValid => ValidateSettings().Count == 0;

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get changeover time based on material types
        /// </summary>
        public int GetChangeoverTime(string fromMaterial, string toMaterial)
        {
            if (string.IsNullOrEmpty(fromMaterial) || string.IsNullOrEmpty(toMaterial))
                return 0;

            if (fromMaterial == toMaterial)
                return 0;

            // Titanium family materials
            var titaniumMaterials = new[] { "Ti-6Al-4V Grade 5", "Ti-6Al-4V ELI Grade 23" };
            var inconelMaterials = new[] { "Inconel 718", "Inconel 625" };

            bool fromTitanium = titaniumMaterials.Any(m => fromMaterial.Contains(m, StringComparison.OrdinalIgnoreCase));
            bool toTitanium = titaniumMaterials.Any(m => toMaterial.Contains(m, StringComparison.OrdinalIgnoreCase));
            bool fromInconel = inconelMaterials.Any(m => fromMaterial.Contains(m, StringComparison.OrdinalIgnoreCase));
            bool toInconel = inconelMaterials.Any(m => toMaterial.Contains(m, StringComparison.OrdinalIgnoreCase));

            // Same material family
            if (fromTitanium && toTitanium)
                return TitaniumToTitaniumChangeoverMinutes;
            
            if (fromInconel && toInconel)
                return InconelToInconelChangeoverMinutes;

            // Cross-material family
            if ((fromTitanium && toInconel) || (fromInconel && toTitanium))
                return CrossMaterialChangeoverMinutes;

            // Default case
            return DefaultMaterialChangeoverMinutes;
        }

        /// <summary>
        /// Check if operator is available during specified time
        /// </summary>
        public bool IsOperatorAvailable(DateTime startTime, DateTime endTime)
        {
            var dayOfWeek = startTime.DayOfWeek;
            
            // Check weekend operations
            if ((dayOfWeek == DayOfWeek.Saturday && !SaturdayOperations) ||
                (dayOfWeek == DayOfWeek.Sunday && !SundayOperations))
            {
                return false;
            }

            var startTimeOfDay = startTime.TimeOfDay;
            var endTimeOfDay = endTime.TimeOfDay;

            // Check if within standard operating hours
            if (IsWithinShift(startTimeOfDay, endTimeOfDay, StandardShiftStart, StandardShiftEnd) ||
                IsWithinShift(startTimeOfDay, endTimeOfDay, EveningShiftStart, EveningShiftEnd) ||
                IsWithinShift(startTimeOfDay, endTimeOfDay, NightShiftStart, NightShiftEnd))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get machine priority
        /// </summary>
        public int GetMachinePriority(string machineId)
        {
            return machineId?.ToUpper() switch
            {
                "TI1" => TI1MachinePriority,
                "TI2" => TI2MachinePriority,
                "INC" => INCMachinePriority,
                _ => 5 // Default priority
            };
        }

        /// <summary>
        /// Validate all settings
        /// </summary>
        public List<string> ValidateSettings()
        {
            var errors = new List<string>();

            // Validate shift times
            if (StandardShiftStart >= StandardShiftEnd)
                errors.Add("Standard shift start time must be before end time");

            if (EveningShiftStart >= EveningShiftEnd)
                errors.Add("Evening shift start time must be before end time");

            // Night shift can span midnight, so different validation
            if (NightShiftStart == NightShiftEnd)
                errors.Add("Night shift start and end times cannot be the same");

            // Validate changeover times are logical
            if (TitaniumToTitaniumChangeoverMinutes > CrossMaterialChangeoverMinutes)
                errors.Add("Same-material changeover should be faster than cross-material changeover");

            if (InconelToInconelChangeoverMinutes > CrossMaterialChangeoverMinutes)
                errors.Add("Same-material changeover should be faster than cross-material changeover");

            return errors;
        }

        /// <summary>
        /// Check if time range is within shift hours
        /// </summary>
        private bool IsWithinShift(TimeSpan startTime, TimeSpan endTime, TimeSpan shiftStart, TimeSpan shiftEnd)
        {
            // Handle overnight shifts (like night shift)
            if (shiftStart > shiftEnd)
            {
                return (startTime >= shiftStart || startTime <= shiftEnd) && 
                       (endTime >= shiftStart || endTime <= shiftEnd);
            }
            else
            {
                return startTime >= shiftStart && endTime <= shiftEnd;
            }
        }

        #endregion
    }
}