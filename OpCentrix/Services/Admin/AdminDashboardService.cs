using OpCentrix.Data;
using OpCentrix.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for admin dashboard functionality
/// </summary>
public interface IAdminDashboardService
{
    Task<AdminDashboardViewModel> GetDashboardDataAsync(string currentUserName, string currentUserRole);
}

public class AdminDashboardService : IAdminDashboardService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<AdminDashboardService> _logger;

    public AdminDashboardService(SchedulerContext context, ILogger<AdminDashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AdminDashboardViewModel> GetDashboardDataAsync(string currentUserName, string currentUserRole)
    {
        try
        {
            var viewModel = new AdminDashboardViewModel
            {
                PageTitle = "Admin Dashboard",
                PageDescription = "System overview and management",
                CurrentUserName = currentUserName,
                CurrentUserRole = currentUserRole,
                IsAdminRole = currentUserRole == "Admin",
                IsManagerRole = currentUserRole == "Manager" || currentUserRole == "Admin"
            };

            // Get basic counts
            viewModel.TotalUsers = await _context.Users.CountAsync();
            // FIXED: Use Machines instead of SlsMachines
            viewModel.TotalMachines = await _context.Machines.CountAsync(m => m.IsActive);
            viewModel.TotalParts = await _context.Parts.CountAsync();
            viewModel.TotalJobs = await _context.Jobs.CountAsync();
            viewModel.ActiveJobs = await _context.Jobs.CountAsync(j => j.Status == "Scheduled" || j.Status == "InProgress");

            // Get completed jobs this week
            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            viewModel.CompletedJobsThisWeek = await _context.Jobs
                .CountAsync(j => j.Status == "Completed" && j.ScheduledEnd >= weekStart);

            // Get recent activities (simplified for now)
            viewModel.RecentActivities = new List<string>
            {
                $"System started at {DateTime.Now:yyyy-MM-dd HH:mm}",
                $"Total jobs scheduled: {viewModel.TotalJobs}",
                $"Active machines: {viewModel.TotalMachines}",
                $"Registered users: {viewModel.TotalUsers}"
            };

            viewModel.LastDatabaseUpdate = DateTime.Now;
            viewModel.DatabaseSize = "Calculating..."; // TODO: Implement actual database size calculation

            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin dashboard data");
            throw;
        }
    }
}