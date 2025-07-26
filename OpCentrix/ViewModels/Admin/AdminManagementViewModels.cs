using OpCentrix.Models;

namespace OpCentrix.ViewModels.Admin;

/// <summary>
/// View model for managing jobs in admin interface
/// </summary>
public class AdminJobsViewModel : AdminBaseViewModel
{
    public List<Job> Jobs { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = string.Empty;
    public string MachineFilter { get; set; } = string.Empty;
    public DateTime? StartDateFilter { get; set; }
    public DateTime? EndDateFilter { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalItems { get; set; }
    public List<Machine> AvailableMachines { get; set; } = new();
    public List<string> AvailableStatuses { get; set; } = new() { "Scheduled", "InProgress", "Completed", "Cancelled" };
}

/// <summary>
/// View model for managing parts in admin interface
/// </summary>
public class AdminPartsViewModel : AdminBaseViewModel
{
    public List<Part> Parts { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public string MaterialFilter { get; set; } = string.Empty;
    public bool? IsActiveFilter { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalItems { get; set; }
    public List<string> AvailableMaterials { get; set; } = new();
}

/// <summary>
/// View model for admin audit logs
/// </summary>
public class AdminLogsViewModel : AdminBaseViewModel
{
    public List<AuditLogEntry> LogEntries { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public string ActionFilter { get; set; } = string.Empty;
    public string UserFilter { get; set; } = string.Empty;
    public DateTime? StartDateFilter { get; set; }
    public DateTime? EndDateFilter { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalItems { get; set; }
}

/// <summary>
/// Simple audit log entry for admin interface
/// </summary>
public class AuditLogEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}