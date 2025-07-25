namespace OpCentrix.ViewModels.Admin;

/// <summary>
/// Base view model for all admin pages
/// </summary>
public class AdminBaseViewModel
{
    public string PageTitle { get; set; } = string.Empty;
    public string PageDescription { get; set; } = string.Empty;
    public string CurrentUserName { get; set; } = string.Empty;
    public string CurrentUserRole { get; set; } = string.Empty;
    public bool IsAdminRole { get; set; }
    public bool IsManagerRole { get; set; }
}

/// <summary>
/// Dashboard view model for admin home page
/// </summary>
public class AdminDashboardViewModel : AdminBaseViewModel
{
    public int TotalUsers { get; set; }
    public int TotalMachines { get; set; }
    public int TotalParts { get; set; }
    public int TotalJobs { get; set; }
    public int ActiveJobs { get; set; }
    public int CompletedJobsThisWeek { get; set; }
    public List<string> RecentActivities { get; set; } = new();
    public DateTime LastDatabaseUpdate { get; set; }
    public string DatabaseSize { get; set; } = string.Empty;
}