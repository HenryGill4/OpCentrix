using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for admin job management functionality
/// </summary>
public interface IAdminJobService
{
    Task<AdminJobsViewModel> GetJobsAsync(AdminJobsViewModel filters, string currentUserName, string currentUserRole);
    Task<Job?> GetJobByIdAsync(int id);
    Task<bool> DeleteJobAsync(int id, string currentUserName);
    Task<List<SlsMachine>> GetAvailableMachinesAsync();
}

public class AdminJobService : IAdminJobService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<AdminJobService> _logger;
    private const int PageSize = 20;

    public AdminJobService(SchedulerContext context, ILogger<AdminJobService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AdminJobsViewModel> GetJobsAsync(AdminJobsViewModel filters, string currentUserName, string currentUserRole)
    {
        try
        {
            var query = _context.Jobs
                .Include(j => j.Part)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                query = query.Where(j => 
                    j.Part!.PartNumber.Contains(filters.SearchTerm) ||
                    j.Part.Description.Contains(filters.SearchTerm) ||
                    j.Notes!.Contains(filters.SearchTerm));
            }

            if (!string.IsNullOrEmpty(filters.StatusFilter))
            {
                query = query.Where(j => j.Status == filters.StatusFilter);
            }

            if (!string.IsNullOrEmpty(filters.MachineFilter))
            {
                query = query.Where(j => j.MachineId == filters.MachineFilter);
            }

            if (filters.StartDateFilter.HasValue)
            {
                query = query.Where(j => j.ScheduledStart >= filters.StartDateFilter.Value);
            }

            if (filters.EndDateFilter.HasValue)
            {
                query = query.Where(j => j.ScheduledEnd <= filters.EndDateFilter.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var jobs = await query
                .OrderByDescending(j => j.ScheduledStart)
                .Skip((filters.CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Build view model
            var viewModel = new AdminJobsViewModel
            {
                PageTitle = "Manage Jobs",
                PageDescription = "View and manage scheduled jobs",
                CurrentUserName = currentUserName,
                CurrentUserRole = currentUserRole,
                IsAdminRole = currentUserRole == "Admin",
                IsManagerRole = currentUserRole == "Manager" || currentUserRole == "Admin",
                Jobs = jobs,
                SearchTerm = filters.SearchTerm,
                StatusFilter = filters.StatusFilter,
                MachineFilter = filters.MachineFilter,
                StartDateFilter = filters.StartDateFilter,
                EndDateFilter = filters.EndDateFilter,
                CurrentPage = filters.CurrentPage,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / PageSize),
                AvailableMachines = await GetAvailableMachinesAsync()
            };

            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting jobs for admin interface");
            throw;
        }
    }

    public async Task<Job?> GetJobByIdAsync(int id)
    {
        return await _context.Jobs
            .Include(j => j.Part)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<bool> DeleteJobAsync(int id, string currentUserName)
    {
        try
        {
            var job = await GetJobByIdAsync(id);
            if (job == null)
                return false;

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {UserName} deleted job {JobId}", currentUserName, id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job {JobId}", id);
            return false;
        }
    }

    public async Task<List<SlsMachine>> GetAvailableMachinesAsync()
    {
        return await _context.SlsMachines
            .Where(m => m.IsActive)
            .OrderBy(m => m.MachineId)
            .ToListAsync();
    }
}