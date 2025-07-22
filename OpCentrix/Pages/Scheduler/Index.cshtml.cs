//Index.cshtml.cs:
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using OpCentrix.Models;
using OpCentrix.Models.ViewModels;
using OpCentrix.Data;

namespace OpCentrix.Pages.Scheduler
{
    public class IndexModel : PageModel
    {
        private readonly SchedulerContext _context;

        // Main scheduler data
        public SchedulerPageViewModel ViewModel { get; set; }
        public FooterSummaryViewModel Summary { get; set; } = new();

        public static bool IsShift(DateTime date)
        {
            var day = date.DayOfWeek;
            return day >= DayOfWeek.Monday && day <= DayOfWeek.Friday;
        }

        public IndexModel(SchedulerContext context)
        {
            _context = context;
        }


        // This method runs when the /Scheduler page is first loaded
        public void OnGet()
        {
            var startDate = DateTime.Today;
            var dates = Enumerable.Range(0, 7).Select(i => startDate.AddDays(i)).ToList();
            var machines = new List<string> { "TI1", "TI2", "INC" };

            var jobs = _context.Jobs
                .Include(j => j.Part)
                .ToList();

            ViewModel = new SchedulerPageViewModel
            {
                Dates = dates,
                Machines = machines,
                Jobs = jobs
            };

            Summary.MachineHours = machines.ToDictionary(
                m => m,
                m => jobs.Where(j => j.MachineId == m)
                          .Sum(j => (j.ScheduledEnd - j.ScheduledStart).TotalHours));
        }

        // This handles HTMX GET request when the user clicks "+ Add Job"
        // It returns a blank form inside the _AddEditJobModal partial view
        public PartialViewResult OnGetShowAddModal(string machineId, DateTime date, int? id)
        {
            Job job;
            if (id.HasValue)
            {
                job = _context.Jobs.First(j => j.Id == id.Value);
            }
            else
            {
                job = new Job { MachineId = machineId, ScheduledStart = date };
            }
            var parts = _context.Parts.ToList();

            return Partial("_AddEditJobModal", new AddEditJobViewModel
            {
                Job = job,
                Parts = parts
            });
        }

        public ContentResult OnGetCloseModal()
        {
            return Content(string.Empty);
        }

        // This handles HTMX POST request when the user submits the modal form
        // It saves the job then re-renders the machine row
        public IActionResult OnPostAddOrUpdateJob([FromForm] Job job)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(job.MachineId)) errors.Add("Machine is required");
            if (job.PartId == 0) errors.Add("Part is required");
            if (job.ScheduledStart == default) errors.Add("Start time is required");

            var part = _context.Parts.FirstOrDefault(p => p.Id == job.PartId);
            if (part != null)
            {
                job.PartNumber = part.PartNumber;
                job.ScheduledEnd = job.ScheduledStart.AddDays(part.AvgDurationDays);
            }

            if (errors.Any())
            {
                var parts = _context.Parts.ToList();
                return Partial("_AddEditJobModal", new AddEditJobViewModel { Job = job, Parts = parts, Errors = errors });
            }

            if (job.Id == 0)
            {
                _context.Jobs.Add(job);
            }
            else
            {
                _context.Jobs.Update(job);
            }

            _context.SaveChanges();

            var startDate = DateTime.Today;
            var dates = Enumerable.Range(0, 7).Select(i => startDate.AddDays(i)).ToList();

            var jobs = _context.Jobs
                .Where(j => j.MachineId == job.MachineId)
                .ToList();

            var vm = new MachineRowViewModel
            {
                MachineId = job.MachineId ?? string.Empty,
                Dates = dates,
                Jobs = jobs
            };

            Summary.MachineHours[job.MachineId!] = jobs.Sum(j => (j.ScheduledEnd - j.ScheduledStart).TotalHours);
            ViewData["Summary"] = Summary;
            return Partial("_MachineRow", vm);
        }

        public IActionResult OnDeleteJob(int id)
        {
            var job = _context.Jobs.FirstOrDefault(j => j.Id == id);
            if (job != null)
            {
                _context.Jobs.Remove(job);
                _context.SaveChanges();
            }

            var startDate = DateTime.Today;
            var dates = Enumerable.Range(0, 7).Select(i => startDate.AddDays(i)).ToList();
            var jobs = _context.Jobs.Where(j => j.MachineId == job!.MachineId).ToList();
            var vm = new MachineRowViewModel
            {
                MachineId = job!.MachineId ?? string.Empty,
                Dates = dates,
                Jobs = jobs
            };

            Summary.MachineHours[job.MachineId!] = jobs.Sum(j => (j.ScheduledEnd - j.ScheduledStart).TotalHours);
            ViewData["Summary"] = Summary;
            return Partial("_MachineRow", vm);
        }

        public PartialViewResult OnGetDayCell(string machineId, DateTime date)
        {
            var startDate = DateTime.Today;
            var dates = Enumerable.Range(0, 7).Select(i => startDate.AddDays(i)).ToList();
            var jobs = _context.Jobs.Where(j => j.MachineId == machineId).ToList();
            var vm = new MachineRowViewModel { MachineId = machineId, Dates = dates, Jobs = jobs };
            ViewData["Summary"] = Summary;
            return Partial("_MachineRow", vm);
        }
    }
}
