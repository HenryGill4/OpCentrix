using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Models;
using OpCentrix.Models.ViewModels;
using OpCentrix.Data;
using OpCentrix.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OpCentrix.Pages.Scheduler
{
    public class IndexModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ISchedulerService _schedulerService;

        public SchedulerPageViewModel ViewModel { get; set; } = new();
        public FooterSummaryViewModel Summary { get; set; } = new();

        public IndexModel(SchedulerContext context, ISchedulerService schedulerService)
        {
            _context = context;
            _schedulerService = schedulerService;
        }

        public async Task OnGetAsync(string? zoom = null, DateTime? startDate = null)
        {
            try
            {
                // Get scheduler configuration
                ViewModel = _schedulerService.GetSchedulerData(zoom, startDate);
                
                // Calculate optimal date range for loading
                var viewStartDate = ViewModel.StartDate;
                var viewEndDate = viewStartDate.AddDays(ViewModel.Dates.Count);
                
                // PERFORMANCE FIX: Load only jobs within visible date range + buffer
                var bufferDays = 1; // Add small buffer for edge cases
                var queryStartDate = viewStartDate.AddDays(-bufferDays);
                var queryEndDate = viewEndDate.AddDays(bufferDays);

                var jobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.ScheduledStart < queryEndDate && j.ScheduledEnd > queryStartDate)
                    .OrderBy(j => j.ScheduledStart)
                    .ToListAsync();
                
                ViewModel.Jobs = jobs;

                // Calculate machine row heights based on filtered jobs
                foreach (var machine in ViewModel.Machines)
                {
                    var machineJobs = jobs.Where(j => j.MachineId == machine).ToList();
                    var (_, rowHeight) = _schedulerService.CalculateMachineRowLayout(machine, machineJobs);
                    ViewModel.MachineRowHeights[machine] = rowHeight;
                }

                // Calculate summary data efficiently
                Summary = new FooterSummaryViewModel
                {
                    MachineHours = ViewModel.Machines.ToDictionary(
                        m => m,
                        m => jobs.Where(j => j.MachineId == m).Sum(j => j.DurationHours)
                    ),
                    JobCounts = ViewModel.Machines.ToDictionary(
                        m => m,
                        m => jobs.Count(j => j.MachineId == m)
                    )
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnGetAsync: {ex.Message}");
                // Provide fallback data to prevent crashes
                ViewModel = _schedulerService.GetSchedulerData(zoom, startDate);
                Summary = new FooterSummaryViewModel();
            }
        }

        public async Task<PartialViewResult> OnGetShowAddModalAsync(string machineId, string date, int? id)
        {
            try
            {
                Job job;
                if (!DateTime.TryParse(date, out var parsedDate))
                {
                    parsedDate = DateTime.UtcNow;
                }

                if (id.HasValue)
                {
                    job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == id.Value);
                    if (job == null)
                    {
                        return Partial("_AddEditJobModal", new AddEditJobViewModel
                        {
                            Job = new Job { MachineId = machineId ?? "TI1", ScheduledStart = parsedDate, ScheduledEnd = parsedDate.AddHours(8) },
                            Parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync(),
                            Errors = new List<string> { "Job not found." }
                        });
                    }
                }
                else
                {
                    job = new Job
                    {
                        MachineId = machineId ?? "TI1",
                        ScheduledStart = parsedDate,
                        ScheduledEnd = parsedDate.AddHours(8),
                        Status = "Scheduled",
                        Quantity = 1
                    };
                }

                var parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
                return Partial("_AddEditJobModal", new AddEditJobViewModel { Job = job, Parts = parts });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ShowAddModal: {ex.Message}");
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = new Job { MachineId = machineId ?? "TI1", ScheduledStart = DateTime.UtcNow, ScheduledEnd = DateTime.UtcNow.AddHours(8) },
                    Parts = new List<Part>(),
                    Errors = new List<string> { "Error loading form. Please try again." }
                });
            }
        }

        public async Task<IActionResult> OnPostAddOrUpdateJobAsync([FromForm] Job job)
        {
            try
            {
                // PERFORMANCE FIX: Only check for conflicts with relevant jobs
                var conflictStartTime = job.ScheduledStart.AddDays(-1); // Buffer for long jobs
                var conflictEndTime = job.ScheduledEnd.AddDays(1);
                
                var existingJobs = await _context.Jobs
                    .Where(j => j.Id != job.Id && 
                               j.MachineId == job.MachineId && 
                               j.ScheduledStart < conflictEndTime && 
                               j.ScheduledEnd > conflictStartTime)
                    .ToListAsync();

                if (!_schedulerService.ValidateJobScheduling(job, existingJobs, out var errors))
                {
                    var parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
                    return Partial("_AddEditJobModal", new AddEditJobViewModel { Job = job, Parts = parts, Errors = errors });
                }

                // Set part information
                var part = await _context.Parts.FindAsync(job.PartId);
                if (part != null)
                {
                    job.PartNumber = part.PartNumber;
                    job.EstimatedHours = part.EstimatedHours;
                }

                // Set audit fields
                job.LastModifiedDate = DateTime.UtcNow;
                job.LastModifiedBy = User.Identity?.Name ?? "System";
                
                string logAction;
                if (job.Id == 0)
                {
                    job.CreatedDate = DateTime.UtcNow;
                    job.CreatedBy = User.Identity?.Name ?? "System";
                    _context.Jobs.Add(job);
                    logAction = "Created";
                }
                else
                {
                    _context.Jobs.Update(job);
                    logAction = "Updated";
                }

                await _context.SaveChangesAsync();

                // Create audit log entry
                _context.JobLogEntries.Add(new JobLogEntry
                {
                    MachineId = job.MachineId,
                    PartNumber = job.PartNumber ?? string.Empty,
                    Action = logAction,
                    Operator = User.Identity?.Name ?? "System",
                    Notes = $"Job {logAction.ToLower()} via scheduler",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                // HTMX FIX: Return updated machine row for seamless update
                return await GetMachineRowPartialAsync(job.MachineId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddOrUpdateJob: {ex.Message}");
                var parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = parts,
                    Errors = new List<string> { $"Database error: {ex.Message}" }
                });
            }
        }

        public async Task<IActionResult> OnPostDeleteJobAsync(int id)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(id);
                if (job != null)
                {
                    var machineId = job.MachineId; // Store before deletion
                    
                    _context.Jobs.Remove(job);
                    _context.JobLogEntries.Add(new JobLogEntry
                    {
                        MachineId = job.MachineId,
                        PartNumber = job.PartNumber ?? string.Empty,
                        Action = "Deleted",
                        Operator = User.Identity?.Name ?? "System",
                        Notes = "Job deleted via scheduler",
                        Timestamp = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();

                    // HTMX FIX: Return updated machine row
                    return await GetMachineRowPartialAsync(machineId);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteJob: {ex.Message}");
                return StatusCode(500, $"Error deleting job: {ex.Message}");
            }
        }

        private async Task<PartialViewResult> GetMachineRowPartialAsync(string machineId)
        {
            try
            {
                // Get current zoom from query params
                var zoom = Request.Query["zoom"].FirstOrDefault() ?? "day";
                var viewModel = _schedulerService.GetSchedulerData(zoom);
                
                // Load jobs for this specific machine within visible range
                var startDate = viewModel.StartDate;
                var endDate = startDate.AddDays(viewModel.Dates.Count);
                
                var machineJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.MachineId == machineId && 
                               j.ScheduledStart < endDate && 
                               j.ScheduledEnd > startDate)
                    .OrderBy(j => j.ScheduledStart)
                    .ToListAsync();

                var (_, rowHeight) = _schedulerService.CalculateMachineRowLayout(machineId, machineJobs);

                var machineRowViewModel = new MachineRowViewModel
                {
                    MachineId = machineId,
                    Dates = viewModel.Dates,
                    Jobs = machineJobs,
                    RowHeight = rowHeight,
                    SlotsPerDay = viewModel.SlotsPerDay,
                    SlotMinutes = viewModel.SlotMinutes,
                    Zoom = zoom
                };

                return Partial("_MachineRow", machineRowViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating machine row partial: {ex.Message}");
                // Return empty machine row on error
                var emptyViewModel = new MachineRowViewModel
                {
                    MachineId = machineId,
                    Dates = new List<DateTime>(),
                    Jobs = new List<Job>(),
                    RowHeight = 160,
                    SlotsPerDay = 1,
                    SlotMinutes = 1440,
                    Zoom = "day"
                };
                return Partial("_MachineRow", emptyViewModel);
            }
        }
    }
}
