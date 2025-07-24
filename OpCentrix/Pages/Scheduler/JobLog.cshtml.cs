using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Data;
using OpCentrix.Models;
using System.Collections.Generic;
using System.Linq;

namespace OpCentrix.Pages.Scheduler
{
    public class JobLogModel : PageModel
    {
        private readonly SchedulerContext _context;
        public List<JobLogEntry> Logs { get; set; } = new();

        public JobLogModel(SchedulerContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            Logs = _context.JobLogEntries.OrderByDescending(l => l.Timestamp).Take(100).ToList();
        }

        public IActionResult OnPost()
        {
            var entry = new JobLogEntry
            {
                MachineId = Request.Form["MachineId"],
                PartNumber = Request.Form["PartNumber"],
                Action = Request.Form["Action"],
                Operator = Request.Form["Operator"],
                Notes = Request.Form["Notes"],
                Timestamp = DateTime.Now
            };
            _context.JobLogEntries.Add(entry);
            _context.SaveChanges();

            // If the action is "AddToSchedule", also add a Job to the schedule
            if (entry.Action == "AddToSchedule")
            {
                // Try to find the part by part number
                var part = _context.Parts.FirstOrDefault(p => p.PartNumber == entry.PartNumber);
                if (part != null)
                {
                    var job = new Job
                    {
                        MachineId = entry.MachineId,
                        PartId = part.Id,
                        PartNumber = part.PartNumber,
                        ScheduledStart = DateTime.Now,
                        ScheduledEnd = DateTime.Now.AddDays(part.AvgDurationDays),
                        Status = "Scheduled",
                        Operator = entry.Operator,
                        Notes = entry.Notes,
                        Quantity = 1
                    };
                    _context.Jobs.Add(job);
                    _context.SaveChanges();
                }
            }

            return RedirectToPage();
        }
    }
}
