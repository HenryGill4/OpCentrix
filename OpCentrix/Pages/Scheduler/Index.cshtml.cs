//Index.cshtml.cs:
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using System.Text.Json;
using System;

namespace OpCentrix.Pages.Scheduler
{
    public class IndexModel : PageModel
    {
        // The main view model passed to the Razor view to render the scheduler
        public SchedulerPageViewModel ViewModel { get; set; } = new();

        private const string ScheduleFile = "schedule.json";

        // Load schedule.json and map to machines
        public void OnGet()
        {
            var jobs = LoadJobs();
            ViewModel.Machines = BuildMachines(jobs);
            if (jobs.Count > 0)
            {
                var start = jobs.Min(j => j.ScheduledStart);
                var end = jobs.Max(j => j.ScheduledEnd);
                ViewData["TotalHours"] = (int)Math.Ceiling((end - start).TotalHours);
            }
            else
            {
                ViewData["TotalHours"] = 24;
            }
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

        // HTMX handler for editing an existing job
        public PartialViewResult OnGetShowEditModal(string id)
        {
            var jobs = LoadJobs();
            var job = jobs.FirstOrDefault(j => j.Id == id);
            if (job == null)
            {
                job = new JobViewModel
                {
                    ScheduledStart = DateTime.Now,
                    ScheduledEnd = DateTime.Now.AddHours(2)
                };
            }
            return Partial("_AddEditJobModal", job);
        }

        // Add or update a job and persist to schedule.json
        public IActionResult OnPostAddOrUpdateJob(JobViewModel model)
        {
            var jobs = LoadJobs();
            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = $"job_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                jobs.Add(model);
            }
            else
            {
                var existing = jobs.FirstOrDefault(j => j.Id == model.Id);
                if (existing != null)
                {
                    existing.MachineId = model.MachineId;
                    existing.PartNumber = model.PartNumber;
                    existing.ScheduledStart = model.ScheduledStart;
                    existing.ScheduledEnd = model.ScheduledEnd;
                    existing.Notes = model.Notes;
                }
                else
                {
                    jobs.Add(model);
                }
            }

            SaveJobs(jobs);
            Response.Headers.Add("HX-Redirect", "/Scheduler");
            return new EmptyResult();
        }

        public IActionResult OnPostDeleteJob(JobViewModel model)
        {
            var jobs = LoadJobs();
            jobs.RemoveAll(j => j.Id == model.Id);
            SaveJobs(jobs);
            Response.Headers.Add("HX-Redirect", "/Scheduler");
            return new EmptyResult();
        }

        // Helper to load jobs from JSON
        private List<JobViewModel> LoadJobs()
        {
            if (!System.IO.File.Exists(ScheduleFile))
            {
                return new List<JobViewModel>();
            }
            var json = System.IO.File.ReadAllText(ScheduleFile);
            return JsonSerializer.Deserialize<List<JobViewModel>>(json) ?? new List<JobViewModel>();
        }

        private void SaveJobs(List<JobViewModel> jobs)
        {
            var json = JsonSerializer.Serialize(jobs, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(ScheduleFile, json);
        }

        private List<MachineViewModel> BuildMachines(List<JobViewModel> jobs)
        {
            var machines = new List<MachineViewModel>
            {
                new() { Id = "machine_1", Name = "TI1" },
                new() { Id = "machine_2", Name = "TI2" },
                new() { Id = "machine_3", Name = "INC" }
            };

            foreach (var job in jobs.OrderBy(j => j.ScheduledStart))
            {
                var machine = machines.FirstOrDefault(m => m.Id == job.MachineId);
                if (machine != null)
                {
                    machine.Jobs.Add(job);
                }
            }
            return machines;
        }
    }
}
