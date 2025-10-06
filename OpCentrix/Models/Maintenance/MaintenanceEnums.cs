namespace OpCentrix.Models.Maintenance
{
    /// <summary>
    /// Defines the metric or trigger that causes a maintenance rule to become due.
    /// </summary>
    public enum MaintenanceTriggerType
    {
        HoursRun = 0,
        BuildsCompleted = 1,
        DateInterval = 2,
        CustomMeter = 3
    }

    /// <summary>
    /// Severity level used for visual prioritization of maintenance.
    /// </summary>
    public enum MaintenanceSeverity
    {
        Info = 0,
        Warning = 1,
        Critical = 2
    }
}
