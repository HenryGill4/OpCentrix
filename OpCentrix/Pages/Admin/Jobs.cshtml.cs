using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Data;
using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class JobsModel : PageModel
    {
        private readonly SchedulerContext _context;

        public JobsModel(SchedulerContext context)
        {
            _context = context;
        }

        public List<Job> Jobs { get; set; } = new();
        public List<Part> Parts { get; set; } = new();
        public string SearchTerm { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public string MachineFilter { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        [BindProperty]
        public Job JobToEdit { get; set; } = new();

        public async Task OnGetAsync(string? search, string? status, string? machine, int? page)
        {
            SearchTerm = search ?? string.Empty;
            StatusFilter = status ?? string.Empty;
            MachineFilter = machine ?? string.Empty;
            PageNumber = page ?? 1;

            var query = _context.Jobs.Include(j => j.Part).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(j => j.PartNumber.Contains(SearchTerm) || 
                                        j.Operator.Contains(SearchTerm) ||
                                        j.Notes.Contains(SearchTerm));
            }

            if (!string.IsNullOrEmpty(StatusFilter))
            {
                query = query.Where(j => j.Status == StatusFilter);
            }

            if (!string.IsNullOrEmpty(MachineFilter))
            {
                query = query.Where(j => j.MachineId == MachineFilter);
            }

            TotalCount = await query.CountAsync();
            
            Jobs = await query
                .OrderByDescending(j => j.CreatedDate)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            Parts = await _context.Parts.Where(p => p.IsActive).ToListAsync();
        }

        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            var job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == id);
            if (job == null)
                return NotFound();

            var parts = await _context.Parts.Where(p => p.IsActive).ToListAsync();
            
            var editForm = $@"
                <div class='bg-white rounded-lg p-6 max-w-2xl w-full mx-4'>
                    <h3 class='text-lg font-semibold mb-4'>Edit Job #{job.Id}</h3>
                    <form hx-post='/Admin/Jobs?handler=Update' hx-include='this' class='space-y-4'>
                        <input type='hidden' name='JobToEdit.Id' value='{job.Id}' />
                        
                        <div class='grid grid-cols-2 gap-4'>
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Machine</label>
                                <select name='JobToEdit.MachineId' class='w-full border border-gray-300 rounded-lg p-2' required>
                                    <option value='TI1' {(job.MachineId == "TI1" ? "selected" : "")}>TI1</option>
                                    <option value='TI2' {(job.MachineId == "TI2" ? "selected" : "")}>TI2</option>
                                    <option value='INC' {(job.MachineId == "INC" ? "selected" : "")}>INC</option>
                                </select>
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Part</label>
                                <select name='JobToEdit.PartId' class='w-full border border-gray-300 rounded-lg p-2' required>
                                    {string.Join("", parts.Select(p => $"<option value='{p.Id}' {(job.PartId == p.Id ? "selected" : "")}>{p.PartNumber} - {p.Description}</option>"))}
                                </select>
                            </div>
                        </div>
                        
                        <div class='grid grid-cols-2 gap-4'>
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Start Time</label>
                                <input type='datetime-local' name='JobToEdit.ScheduledStart' value='{job.ScheduledStart:yyyy-MM-ddTHH:mm}' class='w-full border border-gray-300 rounded-lg p-2' required />
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>End Time</label>
                                <input type='datetime-local' name='JobToEdit.ScheduledEnd' value='{job.ScheduledEnd:yyyy-MM-ddTHH:mm}' class='w-full border border-gray-300 rounded-lg p-2' required />
                            </div>
                        </div>
                        
                        <div class='grid grid-cols-2 gap-4'>
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Status</label>
                                <select name='JobToEdit.Status' class='w-full border border-gray-300 rounded-lg p-2' required>
                                    <option value='Scheduled' {(job.Status == "Scheduled" ? "selected" : "")}>Scheduled</option>
                                    <option value='Active' {(job.Status == "Active" ? "selected" : "")}>Active</option>
                                    <option value='Complete' {(job.Status == "Complete" ? "selected" : "")}>Complete</option>
                                    <option value='Delayed' {(job.Status == "Delayed" ? "selected" : "")}>Delayed</option>
                                    <option value='Cancelled' {(job.Status == "Cancelled" ? "selected" : "")}>Cancelled</option>
                                </select>
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Quantity</label>
                                <input type='number' name='JobToEdit.Quantity' value='{job.Quantity}' class='w-full border border-gray-300 rounded-lg p-2' required min='1' />
                            </div>
                        </div>
                        
                        <div>
                            <label class='block text-sm font-medium text-gray-700 mb-1'>Operator</label>
                            <input type='text' name='JobToEdit.Operator' value='{job.Operator}' class='w-full border border-gray-300 rounded-lg p-2' />
                        </div>
                        
                        <div>
                            <label class='block text-sm font-medium text-gray-700 mb-1'>Notes</label>
                            <textarea name='JobToEdit.Notes' class='w-full border border-gray-300 rounded-lg p-2' rows='3'>{job.Notes}</textarea>
                        </div>
                        
                        <div class='flex justify-end space-x-3 pt-4'>
                            <button type='button' onclick='hideModal()' class='px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50'>
                                Cancel
                            </button>
                            <button type='submit' class='px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700'>
                                Update Job
                            </button>
                        </div>
                    </form>
                </div>";

            return Content(editForm, "text/html");
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            try
            {
                var existingJob = await _context.Jobs.FindAsync(JobToEdit.Id);
                if (existingJob == null)
                    return NotFound();

                // Update properties
                existingJob.MachineId = JobToEdit.MachineId;
                existingJob.PartId = JobToEdit.PartId;
                existingJob.ScheduledStart = JobToEdit.ScheduledStart;
                existingJob.ScheduledEnd = JobToEdit.ScheduledEnd;
                existingJob.Status = JobToEdit.Status;
                existingJob.Quantity = JobToEdit.Quantity;
                existingJob.Operator = JobToEdit.Operator;
                existingJob.Notes = JobToEdit.Notes;
                existingJob.LastModifiedDate = DateTime.UtcNow;
                existingJob.LastModifiedBy = "Admin";

                // Update part number
                var part = await _context.Parts.FindAsync(JobToEdit.PartId);
                if (part != null)
                {
                    existingJob.PartNumber = part.PartNumber;
                }

                await _context.SaveChangesAsync();

                // Add log entry
                _context.JobLogEntries.Add(new JobLogEntry
                {
                    MachineId = existingJob.MachineId,
                    PartNumber = existingJob.PartNumber,
                    Action = "Updated",
                    Operator = "Admin",
                    Notes = "Job updated via admin panel",
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Content("<script>hideModal(); showNotification('Job updated successfully!', 'success'); setTimeout(() => window.location.reload(), 1000);</script>", "text/html");
            }
            catch (Exception ex)
            {
                return Content($"<script>showNotification('Error updating job: {ex.Message}', 'error');</script>", "text/html");
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(id);
                if (job == null)
                    return NotFound();

                _context.Jobs.Remove(job);

                // Add log entry
                _context.JobLogEntries.Add(new JobLogEntry
                {
                    MachineId = job.MachineId,
                    PartNumber = job.PartNumber,
                    Action = "Deleted",
                    Operator = "Admin",
                    Notes = "Job deleted via admin panel",
                    Timestamp = DateTime.Now
                });

                await _context.SaveChangesAsync();

                return Content("<script>showNotification('Job deleted successfully!', 'success'); setTimeout(() => window.location.reload(), 1000);</script>", "text/html");
            }
            catch (Exception ex)
            {
                return Content($"<script>showNotification('Error deleting job: {ex.Message}', 'error');</script>", "text/html");
            }
        }
    }
}