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

        // Example part list for TI printers
        private static readonly List<Part> ExampleParts = new()
        {
            new Part { PartNumber = "14-5397 version 2 of 22's", AvgDurationDays = 5, Description = "Example Data" },
            new Part { PartNumber = "MPS 14-5301 stacked with 14-5321", AvgDurationDays = 4, Description = "Example Data" },
            new Part { PartNumber = "14-5557 SPC-45 STACKED", AvgDurationDays = 4, Description = "Example Data" },
            new Part { PartNumber = "14-5299", AvgDurationDays = 3, Description = "Example Data" },
            new Part { PartNumber = "Z#001.5 ENG 56 14-5011", AvgDurationDays = 3, Description = "Example Data" },
            new Part { PartNumber = "APCP50 14-5330", AvgDurationDays = 2, Description = "Example Data" },
            new Part { PartNumber = "APCP50 14-5288", AvgDurationDays = 2, Description = "Example Data" },
            new Part { PartNumber = "SPC 45 and 14-5557", AvgDurationDays = 4, Description = "Example Data" }
        };

        // Example canceled jobs (optional archive)
        private static readonly List<Part> CanceledParts = new()
        {
            new Part { PartNumber = "14-5397 version 2 of 22's", Description = "Canceled" },
            new Part { PartNumber = "14-5290", Description = "Canceled" },
            new Part { PartNumber = "14-5289", Description = "Canceled" },
            new Part { PartNumber = "14-5288", Description = "Canceled" }
        };

        // This method runs when the /Scheduler page is first loaded
        public void OnGet()
        {
            var today = DateTime.Today;
            int diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            var start = today.AddDays(-diff);

            ViewModel.StartOfWeek = start;
            ViewModel.Dates = Enumerable.Range(0, 7)
                .Select(i => start.AddDays(i).ToString("yyyy-MM-dd"))
                .ToList();

            ViewModel.Machines = new List<MachineViewModel>
            {
                new MachineViewModel
                {
                    Id = "TI1",
                    Name = "TI1",
                    Jobs = new List<ScheduledJob>
                    {
                        new ScheduledJob
                        {
                            JobId = 1,
                            PartNumber = "PN-1001",
                            MachineName = "TI1",
                            StartDate = start.AddDays(1),
                            DurationDays = 2,
                            Status = "Scheduled",
                            OperatorName = "Alice",
                            Notes = "Sample job"
                        }
                    }
                },
                new MachineViewModel { Id = "TI2", Name = "TI2", Jobs = new List<ScheduledJob>() },
                new MachineViewModel { Id = "INC", Name = "INC", Jobs = new List<ScheduledJob>() }
            };
        }

        // This handles HTMX GET request when the user clicks "+ Add Job"
        // It returns a blank form inside the _AddEditJobModal partial view
        public PartialViewResult OnGetShowAddModal(string machine, DateTime date)
        {
            var job = new ScheduledJob
            {
                MachineName = machine,
                StartDate = date,
                DurationDays = 1
            };

            var vm = new AddEditJobModalViewModel
            {
                Job = job,
                Parts = ExampleParts
            };

            return Partial("_AddEditJobModal", vm);
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
