using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.CRM;

namespace OpCentrix.Services.CRM;

public class CrmTaskSeedingService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<CrmTaskSeedingService> _logger;

    public CrmTaskSeedingService(SchedulerContext context, ILogger<CrmTaskSeedingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedSampleTasksAsync()
    {
        try
        {
            // Check if CRM tables exist and have any data
            var existingTasks = await _context.CrmTasks.AnyAsync();
            if (existingTasks)
            {
                _logger.LogInformation("CRM tasks already exist, skipping seeding");
                return;
            }

            _logger.LogInformation("Seeding sample CRM tasks...");

            // Get available users (exclude Admin to avoid admin-only data)
            var users = await _context.Users
                .Where(u => u.Role != "Admin") // Exclude admin from getting tasks
                .ToListAsync();

            if (!users.Any())
            {
                _logger.LogWarning("No non-admin users found for task assignment");
                return;
            }

            // Create sample accounts first
            var accounts = new List<CrmAccount>
            {
                new CrmAccount
                {
                    Name = "TechCorp Industries",
                    Status = "Active",
                    Notes = "Key client for advanced manufacturing components - Technology industry, $2.5M revenue"
                },
                new CrmAccount
                {
                    Name = "Precision Parts Ltd",
                    Status = "Active",
                    Notes = "Aerospace components manufacturer - $1.2M revenue"
                },
                new CrmAccount
                {
                    Name = "Innovation Labs",
                    Status = "Active", 
                    Notes = "Research and development focused - $800K revenue"
                }
            };

            _context.CrmAccounts.AddRange(accounts);
            await _context.SaveChangesAsync();

            // Create sample tasks for different users
            var tasks = new List<CrmTask>();
            var random = new Random();

            foreach (var user in users.Take(5)) // Limit to first 5 users
            {
                var userTasks = new List<CrmTask>
                {
                    new CrmTask
                    {
                        Title = $"Quality Review - Part #{random.Next(1000, 9999)}",
                        Description = "Review manufacturing quality and provide feedback on recent production batch. Check tolerances and surface finish requirements.",
                        Status = "Open",
                        Priority = 3,
                        AssignedToUserId = user.Id,
                        AccountId = accounts[random.Next(accounts.Count)].Id,
                        DueAt = DateTime.UtcNow.AddDays(random.Next(1, 7)),
                        CreatedByUserId = 1, // Assume admin user ID = 1
                        CreatedDate = DateTime.UtcNow.AddDays(-random.Next(1, 3))
                    },
                    new CrmTask
                    {
                        Title = $"Process Documentation Update",
                        Description = "Update process documentation for SLS printing procedures. Include new material specifications and quality checkpoints.",
                        Status = "InProgress",
                        Priority = 2,
                        AssignedToUserId = user.Id,
                        AccountId = accounts[random.Next(accounts.Count)].Id,
                        DueAt = DateTime.UtcNow.AddDays(random.Next(3, 10)),
                        CreatedByUserId = 1,
                        CreatedDate = DateTime.UtcNow.AddDays(-random.Next(2, 5))
                    },
                    new CrmTask
                    {
                        Title = $"Client Follow-up Meeting",
                        Description = "Schedule and conduct follow-up meeting with client regarding recent order delivery and future requirements.",
                        Status = "Open",
                        Priority = 4, // High priority
                        AssignedToUserId = user.Id,
                        AccountId = accounts[random.Next(accounts.Count)].Id,
                        DueAt = DateTime.UtcNow.AddDays(1), // Due tomorrow (urgent)
                        CreatedByUserId = 1,
                        CreatedDate = DateTime.UtcNow.AddHours(-random.Next(2, 12))
                    }
                };

                tasks.AddRange(userTasks);
            }

            // Add some overdue tasks for testing
            if (users.Any())
            {
                var overdueTask = new CrmTask
                {
                    Title = "Overdue Equipment Calibration",
                    Description = "Critical: Complete calibration of SLS printer. This task is overdue and requires immediate attention.",
                    Status = "InProgress",
                    Priority = 5, // Critical
                    AssignedToUserId = users.First().Id,
                    AccountId = accounts.First().Id,
                    DueAt = DateTime.UtcNow.AddDays(-2), // Overdue
                    CreatedByUserId = 1,
                    CreatedDate = DateTime.UtcNow.AddDays(-5)
                };

                tasks.Add(overdueTask);
            }

            _context.CrmTasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            // Add some progress entries to show activity
            var progressEntries = new List<CrmTaskProgress>();
            
            foreach (var task in tasks.Where(t => t.Status == "InProgress").Take(3))
            {
                var progressEntry = new CrmTaskProgress
                {
                    TaskId = task.Id,
                    ProgressNote = "Initial work started. Gathering requirements and setting up workspace.",
                    PercentComplete = random.Next(15, 45),
                    Status = "InProgress",
                    CreatedByUserId = task.AssignedToUserId ?? 1, // Handle nullable
                    CreatedDate = DateTime.UtcNow.AddDays(-1),
                    ProgressType = "Update"
                };

                progressEntries.Add(progressEntry);
            }

            if (progressEntries.Any())
            {
                _context.CrmTaskProgress.AddRange(progressEntries);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Successfully seeded {tasks.Count} CRM tasks for {users.Count} users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding CRM tasks");
        }
    }
}