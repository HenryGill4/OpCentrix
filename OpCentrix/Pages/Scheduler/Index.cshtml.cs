//Index.cshtml.cs:
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;

namespace OpCentrix.Pages.Scheduler
{
    public class IndexModel : PageModel
    {
        // The main view model passed to the Razor view to render the scheduler
        public SchedulerPageViewModel ViewModel { get; set; } = new();

        // This method runs when the /Scheduler page is first loaded
        public void OnGet()
        {
            // For now, we use a hardcoded machine and job (demo mode)
            ViewModel.Machines = new List<MachineViewModel>
            {
                new MachineViewModel
                {
                    Id = "machine_1",
                    Name = "TI1",
                    Jobs = new List<JobViewModel>
                    {
                        new JobViewModel
                        {
                            Id = "job1",
                            MachineId = "machine_1",
                            PartNumber = "PN-1234",
                            ScheduledStart = DateTime.Today.AddHours(6),
                            ScheduledEnd = DateTime.Today.AddHours(10),
                            Operator = "Henry",
                            Status = "Scheduled",
                            Notes = "Initial build"
                        }
                    }
                }
            };
        }

        // This handles HTMX GET request when the user clicks "+ Add Job"
        // It returns a blank form inside the _AddEditJobModal partial view
        public PartialViewResult OnGetShowAddModal()
        {
            // Pre-populate with default time range
            var emptyModel = new JobViewModel
            {
                ScheduledStart = DateTime.Now,
                ScheduledEnd = DateTime.Now.AddHours(2)
            };

            // Return modal view with empty job form
            return Partial("_AddEditJobModal", emptyModel);
        }

        // This handles HTMX POST request when the user submits the modal form
        // It re-renders the scheduler row with the new job included
        public PartialViewResult OnPostAddOrUpdateJob(JobViewModel model)
        {
            // For now, we're just injecting the form input into a fake "database"
            // Later, you’ll replace this with EF Core to persist real job records
            ViewModel.Machines = new List<MachineViewModel>
            {
                new MachineViewModel
                {
                    Id = model.MachineId,
                    Name = model.MachineId.ToUpper(),
                    Jobs = new List<JobViewModel>
                    {
                        model
                    }
                }
            };

            // Return the updated machine row to HTMX to swap into #schedulerBody
            return Partial("_MachineRow", ViewModel.Machines.First());
        }
    }
}
