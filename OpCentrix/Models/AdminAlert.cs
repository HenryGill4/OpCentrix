using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

/// <summary>
/// Represents automated alert configurations for system monitoring
/// Allows admins to define triggers and notifications for important events
/// </summary>
public class AdminAlert
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Alert name or title
    /// </summary>
    [Required]
    [StringLength(200)]
    public string AlertName { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the alert
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Alert category for grouping (System, Job, Machine, Quality, etc.)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = "System";

    /// <summary>
    /// Event type that triggers this alert
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TriggerType { get; set; } = string.Empty;

    /// <summary>
    /// Trigger conditions in JSON format
    /// </summary>
    [StringLength(2000)]
    public string TriggerConditions { get; set; } = "{}";

    /// <summary>
    /// Alert severity level (1 = Critical, 5 = Info)
    /// </summary>
    [Range(1, 5)]
    public int SeverityLevel { get; set; } = 3;

    /// <summary>
    /// Whether this alert is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Email recipients (comma-separated list)
    /// </summary>
    [StringLength(1000)]
    public string EmailRecipients { get; set; } = string.Empty;

    /// <summary>
    /// Email subject template
    /// </summary>
    [StringLength(200)]
    public string EmailSubject { get; set; } = "OpCentrix Alert: {AlertName}";

    /// <summary>
    /// Email body template
    /// </summary>
    [StringLength(2000)]
    public string EmailTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Whether to send SMS notifications
    /// </summary>
    public bool SendSms { get; set; } = false;

    /// <summary>
    /// SMS recipients (comma-separated phone numbers)
    /// </summary>
    [StringLength(500)]
    public string SmsRecipients { get; set; } = string.Empty;

    /// <summary>
    /// SMS message template
    /// </summary>
    [StringLength(160)]
    public string SmsTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Whether to send browser notifications
    /// </summary>
    public bool SendBrowserNotification { get; set; } = true;

    /// <summary>
    /// Cooldown period in minutes to prevent spam
    /// </summary>
    public int CooldownMinutes { get; set; } = 15;

    /// <summary>
    /// Last time this alert was triggered
    /// </summary>
    public DateTime? LastTriggered { get; set; }

    /// <summary>
    /// Number of times this alert has been triggered
    /// </summary>
    public int TriggerCount { get; set; } = 0;

    /// <summary>
    /// Escalation rules in JSON format
    /// </summary>
    [StringLength(1000)]
    public string EscalationRules { get; set; } = "{}";

    /// <summary>
    /// Business hours restriction (only send during work hours)
    /// </summary>
    public bool BusinessHoursOnly { get; set; } = false;

    /// <summary>
    /// Start time for business hours
    /// </summary>
    public TimeSpan BusinessHoursStart { get; set; } = new TimeSpan(8, 0, 0);

    /// <summary>
    /// End time for business hours
    /// </summary>
    public TimeSpan BusinessHoursEnd { get; set; } = new TimeSpan(17, 0, 0);

    /// <summary>
    /// Days of week for business hours (1 = Monday, 7 = Sunday)
    /// </summary>
    [StringLength(20)]
    public string BusinessDays { get; set; } = "1,2,3,4,5"; // Monday to Friday

    /// <summary>
    /// Maximum number of alerts per day
    /// </summary>
    public int MaxAlertsPerDay { get; set; } = 10;

    /// <summary>
    /// Alert triggers today count
    /// </summary>
    public int TriggersToday { get; set; } = 0;

    /// <summary>
    /// Last reset date for daily counters
    /// </summary>
    public DateTime LastDailyReset { get; set; } = DateTime.UtcNow.Date;

    /// <summary>
    /// Audit fields
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    [StringLength(100)]
    public string LastModifiedBy { get; set; } = "System";

    /// <summary>
    /// Helper property for display
    /// </summary>
    [NotMapped]
    public string DisplayName => $"{Category} - {AlertName}";

    /// <summary>
    /// Helper property to get severity text
    /// </summary>
    [NotMapped]
    public string SeverityText => SeverityLevel switch
    {
        1 => "Critical",
        2 => "High",
        3 => "Medium",
        4 => "Low",
        5 => "Info",
        _ => "Unknown"
    };

    /// <summary>
    /// Helper property to get severity color
    /// </summary>
    [NotMapped]
    public string SeverityColor => SeverityLevel switch
    {
        1 => "#DC2626", // red (critical)
        2 => "#F59E0B", // orange (high)
        3 => "#FBBF24", // yellow (medium)
        4 => "#10B981", // green (low)
        5 => "#6B7280", // gray (info)
        _ => "#6B7280"
    };

    /// <summary>
    /// Check if alert is in cooldown period
    /// </summary>
    public bool IsInCooldown()
    {
        if (!LastTriggered.HasValue)
            return false;

        return DateTime.UtcNow < LastTriggered.Value.AddMinutes(CooldownMinutes);
    }

    /// <summary>
    /// Check if we're within business hours
    /// </summary>
    public bool IsWithinBusinessHours()
    {
        if (!BusinessHoursOnly)
            return true;

        var now = DateTime.Now;
        var currentTime = now.TimeOfDay;
        var currentDayOfWeek = (int)now.DayOfWeek;
        if (currentDayOfWeek == 0) currentDayOfWeek = 7; // Sunday = 7

        // Check if current day is a business day
        var businessDaysList = BusinessDays.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                          .Select(d => int.TryParse(d, out var day) ? day : 0)
                                          .Where(d => d > 0)
                                          .ToList();

        if (!businessDaysList.Contains(currentDayOfWeek))
            return false;

        // Check if current time is within business hours
        return currentTime >= BusinessHoursStart && currentTime <= BusinessHoursEnd;
    }

    /// <summary>
    /// Check if daily limit has been reached
    /// </summary>
    public bool HasReachedDailyLimit()
    {
        // Reset counter if it's a new day
        if (LastDailyReset.Date < DateTime.UtcNow.Date)
        {
            TriggersToday = 0;
            LastDailyReset = DateTime.UtcNow.Date;
        }

        return TriggersToday >= MaxAlertsPerDay;
    }

    /// <summary>
    /// Get trigger conditions as a dictionary
    /// </summary>
    public Dictionary<string, object> GetTriggerConditions()
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(TriggerConditions) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Set trigger conditions from a dictionary
    /// </summary>
    public void SetTriggerConditions(Dictionary<string, object> conditions)
    {
        try
        {
            TriggerConditions = System.Text.Json.JsonSerializer.Serialize(conditions);
        }
        catch
        {
            TriggerConditions = "{}";
        }
    }

    /// <summary>
    /// Get escalation rules as a dictionary
    /// </summary>
    public Dictionary<string, object> GetEscalationRules()
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(EscalationRules) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Record that this alert was triggered
    /// </summary>
    public void RecordTrigger()
    {
        LastTriggered = DateTime.UtcNow;
        TriggerCount++;
        
        // Reset daily counter if it's a new day
        if (LastDailyReset.Date < DateTime.UtcNow.Date)
        {
            TriggersToday = 0;
            LastDailyReset = DateTime.UtcNow.Date;
        }
        
        TriggersToday++;
    }

    /// <summary>
    /// Check if this alert should be triggered based on conditions
    /// </summary>
    public bool ShouldTrigger()
    {
        if (!IsActive)
            return false;

        if (IsInCooldown())
            return false;

        if (!IsWithinBusinessHours())
            return false;

        if (HasReachedDailyLimit())
            return false;

        return true;
    }
}

/// <summary>
/// Common alert trigger types
/// </summary>
public static class AlertTriggerTypes
{
    public const string JobCompleted = "job.completed";
    public const string JobDelayed = "job.delayed";
    public const string JobFailed = "job.failed";
    public const string MachineError = "machine.error";
    public const string MachineOffline = "machine.offline";
    public const string QualityIssue = "quality.issue";
    public const string SystemError = "system.error";
    public const string DiskSpaceLow = "system.disk_space_low";
    public const string DatabaseError = "database.error";
    public const string UserLoginFailed = "user.login_failed";
    public const string ScheduleConflict = "schedule.conflict";
    public const string MaterialLow = "material.low";
    public const string MaintenanceDue = "maintenance.due";
}

/// <summary>
/// Alert categories for organization
/// </summary>
public static class AlertCategories
{
    public const string System = "System";
    public const string Job = "Job";
    public const string Machine = "Machine";
    public const string Quality = "Quality";
    public const string Schedule = "Schedule";
    public const string Maintenance = "Maintenance";
    public const string User = "User";
    public const string Database = "Database";
    public const string Security = "Security";
}