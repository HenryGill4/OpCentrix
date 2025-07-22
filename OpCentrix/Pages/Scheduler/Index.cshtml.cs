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
                // Add detailed logging for debugging
                Console.WriteLine($"Received job data: Id={job.Id}, MachineId={job.MachineId}, PartId={job.PartId}, Status={job.Status}");
                
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

                // Get part information first to set required fields
                var part = await _context.Parts.FindAsync(job.PartId);
                if (part == null)
                {
                    var parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
                    return Partial("_AddEditJobModal", new AddEditJobViewModel 
                    { 
                        Job = job, 
                        Parts = parts, 
                        Errors = new List<string> { "Selected part not found." } 
                    });
                }

                // Set part information and other required fields
                job.PartNumber = part.PartNumber;
                job.EstimatedHours = part.EstimatedHours;
                
                // Ensure all required string fields have values (prevent null/empty validation errors)
                job.PartNumber = job.PartNumber ?? string.Empty;
                job.MachineId = job.MachineId ?? string.Empty;
                job.Status = job.Status ?? "Scheduled";
                job.CreatedBy = job.CreatedBy ?? (User.Identity?.Name ?? "System");
                job.LastModifiedBy = User.Identity?.Name ?? "System";
                job.RequiredSkills = job.RequiredSkills ?? string.Empty;
                job.RequiredTooling = job.RequiredTooling ?? string.Empty;
                job.RequiredMaterials = job.RequiredMaterials ?? string.Empty;
                job.SpecialInstructions = job.SpecialInstructions ?? string.Empty;
                job.ProcessParameters = job.ProcessParameters ?? "{}";
                job.QualityCheckpoints = job.QualityCheckpoints ?? "{}";
                job.CustomerOrderNumber = job.CustomerOrderNumber ?? string.Empty;
                job.HoldReason = job.HoldReason ?? string.Empty;
                job.Operator = job.Operator ?? string.Empty;
                job.Notes = job.Notes ?? string.Empty;

                // Set audit fields and timestamps
                job.LastModifiedDate = DateTime.UtcNow;
                
                string logAction;
                if (job.Id == 0)
                {
                    // New job - set all required fields
                    job.CreatedDate = DateTime.UtcNow;
                    job.CreatedBy = User.Identity?.Name ?? "System";
                    
                    // Set default values for new enhanced fields
                    job.Priority = job.Priority == 0 ? 3 : job.Priority;
                    job.ProducedQuantity = 0;
                    job.DefectQuantity = 0;
                    job.ReworkQuantity = 0;
                    job.SetupTimeMinutes = 0;
                    job.ChangeoverTimeMinutes = 0;
                    job.LaborCostPerHour = 0;
                    job.MaterialCostPerUnit = 0;
                    job.OverheadCostPerHour = 0;
                    job.MachineUtilizationPercent = 0;
                    job.EnergyConsumptionKwh = 0;
                    job.IsRushJob = false;
                    
                    _context.Jobs.Add(job);
                    logAction = "Created";
                    Console.WriteLine($"Creating new job with PartNumber: {job.PartNumber}");
                }
                else
                {
                    // Update existing job - preserve existing data
                    var existingJob = await _context.Jobs.FindAsync(job.Id);
                    if (existingJob == null)
                    {
                        var parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
                        return Partial("_AddEditJobModal", new AddEditJobViewModel 
                        { 
                            Job = job, 
                            Parts = parts, 
                            Errors = new List<string> { "Job not found for update." } 
                        });
                    }
                    
                    // Update only the fields that come from the form
                    existingJob.MachineId = job.MachineId;
                    existingJob.PartId = job.PartId;
                    existingJob.PartNumber = job.PartNumber;
                    existingJob.EstimatedHours = job.EstimatedHours;
                    existingJob.ScheduledStart = job.ScheduledStart;
                    existingJob.ScheduledEnd = job.ScheduledEnd;
                    existingJob.Status = job.Status;
                    existingJob.Quantity = job.Quantity;
                    existingJob.Operator = job.Operator ?? string.Empty;
                    existingJob.Notes = job.Notes ?? string.Empty;
                    existingJob.LastModifiedDate = DateTime.UtcNow;
                    existingJob.LastModifiedBy = User.Identity?.Name ?? "System";
                    
                    job = existingJob; // Use the existing job for further processing
                    logAction = "Updated";
                    Console.WriteLine($"Updating existing job {job.Id} with PartNumber: {job.PartNumber}");
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"Job {logAction.ToLower()} successfully in database");

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
                Console.WriteLine($"Audit log entry created");

                // HTMX FIX: Return updated machine row for seamless update
                var result = await GetMachineRowPartialAsync(job.MachineId);
                Console.WriteLine($"Returning machine row partial for {job.MachineId}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddOrUpdateJob: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                try
                {
                    var parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
                    return Partial("_AddEditJobModal", new AddEditJobViewModel
                    {
                        Job = job,
                        Parts = parts,
                        Errors = new List<string> { $"Error saving job: {ex.Message}" }
                    });
                }
                catch (Exception innerEx)
                {
                    Console.WriteLine($"Error in AddOrUpdateJob fallback: {innerEx.Message}");
                    // Return empty content to prevent page duplication
                    return Content("");
                }
            }
        }

        public async Task<IActionResult> OnPostDeleteJobAsync(int id)
        {
            try
            {
                Console.WriteLine($"Attempting to delete job with ID: {id}");
                
                var job = await _context.Jobs.FindAsync(id);
                if (job == null)
                {
                    Console.WriteLine($"Job with ID {id} not found");
                    // Return an empty partial response instead of NotFound()
                    // This prevents HTMX from inserting a full error page
                    return Content(""); // Empty response - no change to the page
                }

                var machineId = job.MachineId; // Store before deletion
                Console.WriteLine($"Deleting job {id} from machine {machineId}");
                
                _context.Jobs.Remove(job);
                
                // Create audit log entry
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
                Console.WriteLine($"Job {id} deleted successfully from database");

                // HTMX FIX: Return updated machine row
                var result = await GetMachineRowPartialAsync(machineId);
                Console.WriteLine($"Returning updated machine row for {machineId}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteJob: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // Return empty content instead of StatusCode to prevent full error page insertion
                return Content(""); // Empty response - maintains current state
            }
        }

        private async Task<PartialViewResult> GetMachineRowPartialAsync(string machineId)
        {
            try
            {
                Console.WriteLine($"Generating machine row partial for {machineId}");
                
                // Get current zoom from query params
                var zoom = Request.Query["zoom"].FirstOrDefault() ?? "day";
                Console.WriteLine($"Using zoom level: {zoom}");
                
                var viewModel = _schedulerService.GetSchedulerData(zoom);
                
                // Load jobs for this specific machine within visible range
                var startDate = viewModel.StartDate;
                var endDate = startDate.AddDays(viewModel.Dates.Count);
                Console.WriteLine($"Loading jobs for {machineId} from {startDate} to {endDate}");
                
                var machineJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.MachineId == machineId && 
                               j.ScheduledStart < endDate && 
                               j.ScheduledEnd > startDate)
                    .OrderBy(j => j.ScheduledStart)
                    .ToListAsync();

                Console.WriteLine($"Found {machineJobs.Count} jobs for machine {machineId}");

                var (_, rowHeight) = _schedulerService.CalculateMachineRowLayout(machineId, machineJobs);
                var jobCount = machineJobs.Count;
                var totalHours = machineJobs.Sum(j => j.DurationHours);

                Console.WriteLine($"Machine {machineId}: {jobCount} jobs, {totalHours:F1} hours, row height {rowHeight}px");

                // Create a view model that includes the full machine row structure
                var fullMachineRowViewModel = new
                {
                    MachineId = machineId,
                    Dates = viewModel.Dates,
                    Jobs = machineJobs,
                    RowHeight = rowHeight,
                    SlotsPerDay = viewModel.SlotsPerDay,
                    SlotMinutes = viewModel.SlotMinutes,
                    Zoom = zoom,
                    JobCount = jobCount,
                    TotalHours = totalHours
                };

                Console.WriteLine($"Successfully created view model for machine {machineId}");
                return Partial("_FullMachineRow", fullMachineRowViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating machine row partial for {machineId}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Return empty machine row on error
                var emptyViewModel = new
                {
                    MachineId = machineId,
                    Dates = new List<DateTime>(),
                    Jobs = new List<Job>(),
                    RowHeight = 160,
                    SlotsPerDay = 1,
                    SlotMinutes = 1440,
                    Zoom = "day",
                    JobCount = 0,
                    TotalHours = 0.0
                };
                
                Console.WriteLine($"Returning empty machine row for {machineId} due to error");
                return Partial("_FullMachineRow", emptyViewModel);
            }
        }
    }
}
