using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Data;
using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Pages.Admin
{
    public class LogsModel : PageModel
    {
        private readonly SchedulerContext _context;

        public LogsModel(SchedulerContext context)
        {
            _context = context;
        }

        public List<JobLogEntry> LogEntries { get; set; } = new();
        public string SearchTerm { get; set; } = string.Empty;
        public string ActionFilter { get; set; } = string.Empty;
        public string MachineFilter { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public async Task OnGetAsync(string? search, string? action, string? machine, DateTime? from, DateTime? to, int? page)
        {
            SearchTerm = search ?? string.Empty;
            ActionFilter = action ?? string.Empty;
            MachineFilter = machine ?? string.Empty;
            FromDate = from;
            ToDate = to;
            PageNumber = page ?? 1;

            var query = _context.JobLogEntries.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(l => l.PartNumber.Contains(SearchTerm) || 
                                        l.Operator.Contains(SearchTerm) ||
                                        l.Notes.Contains(SearchTerm));
            }

            if (!string.IsNullOrEmpty(ActionFilter))
            {
                query = query.Where(l => l.Action == ActionFilter);
            }

            if (!string.IsNullOrEmpty(MachineFilter))
            {
                query = query.Where(l => l.MachineId == MachineFilter);
            }

            if (FromDate.HasValue)
            {
                query = query.Where(l => l.Timestamp >= FromDate.Value);
            }

            if (ToDate.HasValue)
            {
                query = query.Where(l => l.Timestamp <= ToDate.Value.AddDays(1));
            }

            TotalCount = await query.CountAsync();
            
            LogEntries = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}