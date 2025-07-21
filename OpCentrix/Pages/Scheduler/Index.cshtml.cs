//Index.cshtml.cs:
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Models;
using OpCentrix.Models.ViewModels;
using OpCentrix.Data;

namespace OpCentrix.Pages.Scheduler
{
    public class IndexModel : PageModel
    {
        private readonly SchedulerContext _context;

        // The main view model passed to the Razor view to render the scheduler
        public SchedulerPageViewModel ViewModel { get; set; }

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
        }

        // This handles HTMX GET request when the user clicks "+ Add Job"
        // It returns a blank form inside the _AddEditJobModal partial view
        public PartialViewResult OnGetShowAddModal(string machineId, DateTime date)
        {
            var job = new Job { MachineId = machineId, ScheduledStart = date };
            var parts = _context.Parts.ToList();

            return Partial("_AddEditJobModal", new AddEditJobViewModel
            {
                Job = job,
                Parts = parts
            });
        }

        // This handles HTMX POST request when the user submits the modal form
        // It re-renders the scheduler row with the new job included
        public PartialViewResult OnPostAddOrUpdateJob(ScheduledJob job)
        {
            var start = job.StartDate.AddDays(-(int)job.StartDate.DayOfWeek + (int)DayOfWeek.Monday);
            ViewModel.StartOfWeek = start;
            ViewModel.Dates = Enumerable.Range(0, 7)
                .Select(i => start.AddDays(i).ToString("yyyy-MM-dd"))
                .ToList();

            ViewModel.Machines = new List<MachineViewModel>
            {
                new MachineViewModel
                {
                    Id = job.MachineName,
                    Name = job.MachineName,
                    Jobs = new List<ScheduledJob> { job }
                }
            };

            ViewData["StartOfWeek"] = ViewModel.StartOfWeek;
            return Partial("_MachineRow", ViewModel.Machines.First());
        }
    }
}
