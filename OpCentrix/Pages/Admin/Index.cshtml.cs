using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Data;
using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly SchedulerContext _context;

        public IndexModel(SchedulerContext context)
        {
            _context = context;
        }

        public int TotalJobs { get; set; }
        public int TotalParts { get; set; }
        public int TotalLogEntries { get; set; }
        public int ActiveJobs { get; set; }
        public int ActiveParts { get; set; }
        public DateTime? LastJobUpdate { get; set; }
        public DateTime? LastPartUpdate { get; set; }
        public List<Job> RecentJobs { get; set; } = new();
        public List<Part> RecentParts { get; set; } = new();
        public List<JobLogEntry> RecentLogEntries { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                // Get database statistics
                TotalJobs = await _context.Jobs.CountAsync();
                TotalParts = await _context.Parts.CountAsync();
                TotalLogEntries = await _context.JobLogEntries.CountAsync();
                
                ActiveJobs = await _context.Jobs.CountAsync(j => j.Status == "Scheduled" || j.Status == "Active");
                ActiveParts = await _context.Parts.CountAsync(p => p.IsActive);
                
                LastJobUpdate = await _context.Jobs.MaxAsync(j => (DateTime?)j.LastModifiedDate);
                LastPartUpdate = await _context.Parts.MaxAsync(p => (DateTime?)p.LastModifiedDate);
                
                // Get recent data
                RecentJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .OrderByDescending(j => j.CreatedDate)
                    .Take(5)
                    .ToListAsync();
                
                RecentParts = await _context.Parts
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(5)
                    .ToListAsync();
                
                RecentLogEntries = await _context.JobLogEntries
                    .OrderByDescending(l => l.Timestamp)
                    .Take(10)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading admin dashboard: {ex.Message}");
                // Initialize with default values
                TotalJobs = 0;
                TotalParts = 0;
                TotalLogEntries = 0;
                ActiveJobs = 0;
                ActiveParts = 0;
            }
        }
    }
}