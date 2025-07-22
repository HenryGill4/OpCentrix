//Index.cshtml.cs:
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Models;
using OpCentrix.Models.ViewModels;
using OpCentrix.Data;
using OpCentrix.Services;

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

        public void OnGet(string? zoom = null)
        {
            try
            {
                // Get basic scheduler data
                ViewModel = _schedulerService.GetSchedulerData(zoom) ?? new SchedulerPageViewModel();
                
                // Load jobs from database
                var jobs = _context.Jobs.Include(j => j.Part).ToList();
                ViewModel.Jobs = jobs;

                // Calculate date range based on jobs
                var (dates, _) = _schedulerService.CalculateMachineRowLayout("", jobs);
                ViewModel.Dates = dates;

                // Calculate machine row heights
                foreach (var machine in ViewModel.Machines)
                {
                    var machineJobs = jobs.Where(j => j.MachineId == machine).ToList();
                    var (_, rowHeight) = _schedulerService.CalculateMachineRowLayout(machine, machineJobs);
                    ViewModel.MachineRowHeights[machine] = rowHeight;
                }

                // Initialize Summary with defensive checks
                Summary = new FooterSummaryViewModel();

                // Calculate summary data safely
                Summary.MachineHours = ViewModel.Machines.ToDictionary(
                    m => m,
                    m => jobs.Where(j => j.MachineId == m).Sum(j => j.DurationHours));

                Summary.JobCounts = ViewModel.Machines.ToDictionary(
                    m => m,
                    m => jobs.Count(j => j.MachineId == m));
            }
            catch (Exception ex)
            {
                // Log the error and provide fallback data
                Console.WriteLine($"Error in OnGet: {ex.Message}");
                
                // Provide fallback data
                ViewModel = new SchedulerPageViewModel();
                Summary = new FooterSummaryViewModel();
            }
        }

        public PartialViewResult OnGetShowAddModal(string machineId, string date, int? id)
        {
            try
            {
                Job job;
                DateTime parsedDate;

                if (!DateTime.TryParse(date, out parsedDate))
                {
                    parsedDate = DateTime.Now;
                }

                if (id.HasValue)
                {
                    job = _context.Jobs.Include(j => j.Part).FirstOrDefault(j => j.Id == id.Value);
                    if (job == null)
                    {
                        return Partial("_AddEditJobModal", new AddEditJobViewModel
                        {
                            Job = new Job { MachineId = machineId ?? "TI1", ScheduledStart = parsedDate, ScheduledEnd = parsedDate.AddHours(8) },
                            Parts = _context.Parts.ToList(),
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

                var parts = _context.Parts.ToList();
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = parts
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ShowAddModal: {ex.Message}");
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = new Job { MachineId = machineId ?? "TI1", ScheduledStart = DateTime.Now, ScheduledEnd = DateTime.Now.AddHours(8) },
                    Parts = new List<Part>(),
                    Errors = new List<string> { "Error loading form. Please try again." }
                });
            }
        }

        public IActionResult OnPostAddOrUpdateJob([FromForm] Job job)
        {
            try
            {
                var errors = new List<string>();
                var existingJobs = _context.Jobs.Where(j => j.Id != job.Id).ToList();

                // Use service for validation
                if (!_schedulerService.ValidateJobScheduling(job, existingJobs, out errors))
                {
                    var parts = _context.Parts.ToList();
                    return Partial("_AddEditJobModal", new AddEditJobViewModel
                    {
                        Job = job,
                        Parts = parts,
                        Errors = errors
                    });
                }

                // Get part information
                var part = _context.Parts.FirstOrDefault(p => p.Id == job.PartId);
                if (part != null)
                {
                    job.PartNumber = part.PartNumber;
                    job.Part = part;

                    // Auto-calculate end time if not set properly
                    if (job.ScheduledEnd <= job.ScheduledStart)
                    {
                        job.ScheduledEnd = job.ScheduledStart.AddDays(part.AvgDurationDays);
                    }
                }

                if (string.IsNullOrEmpty(job.Status))
                {
                    job.Status = "Scheduled";
                }

                if (job.Id == 0)
                {
                    _context.Jobs.Add(job);
                    _context.JobLogEntries.Add(new JobLogEntry
                    {
                        MachineId = job.MachineId,
                        PartNumber = job.PartNumber,
                        Action = "Created",
                        Operator = job.Operator ?? "System",
                        Notes = $"Job created: {job.Notes}",
                        Timestamp = DateTime.Now
                    });
                }
                else
                {
                    _context.Jobs.Update(job);
                    _context.JobLogEntries.Add(new JobLogEntry
                    {
                        MachineId = job.MachineId,
                        PartNumber = job.PartNumber,
                        Action = "Updated",
                        Operator = job.Operator ?? "System",
                        Notes = $"Job updated: {job.Notes}",
                        Timestamp = DateTime.Now
                    });
                }
                _context.SaveChanges();

                // Return success indicator (empty content = success)
                return Content("SUCCESS");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddOrUpdateJob: {ex.Message}");
                var parts = _context.Parts.ToList();
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = parts,
                    Errors = new List<string> { $"Database error: {ex.Message}" }
                });
            }
        }

        public IActionResult OnDeleteJob(int id)
        {
            try
            {
                var job = _context.Jobs.FirstOrDefault(j => j.Id == id);
                
                if (job != null)
                {
                    _context.Jobs.Remove(job);
                    _context.JobLogEntries.Add(new JobLogEntry
                    {
                        MachineId = job.MachineId,
                        PartNumber = job.PartNumber,
                        Action = "Deleted",
                        Operator = job.Operator ?? "System",
                        Notes = $"Job deleted: {job.Notes}",
                        Timestamp = DateTime.Now
                    });
                    _context.SaveChanges();
                }

                return new OkResult(); // Return 200 OK
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteJob: {ex.Message}");
                return new BadRequestObjectResult($"Error deleting job: {ex.Message}");
            }
        }
    }
}
