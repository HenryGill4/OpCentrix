using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.ViewModels;

namespace OpCentrix.Services
{
    public interface IPrintTrackingService
    {
        Task<PrintTrackingDashboardViewModel> GetDashboardDataAsync(int userId);
        Task<int> StartPrintJobAsync(PrintStartViewModel model, int userId);
        Task<bool> CompletePrintJobAsync(PostPrintViewModel model, int userId);
        Task<List<Job>> GetAvailableScheduledJobsAsync(string printerName);
        Task<BuildJob?> GetActiveBuildJobAsync(string printerName);
        Task<bool> HasActiveBuildAsync(string printerName);
        Task<List<BuildJob>> GetRecentBuildsAsync(int count = 20);
        Task CreateCooldownAndChangeoverBlocksAsync(BuildJob completedJob);
    }

    public class PrintTrackingService : IPrintTrackingService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PrintTrackingService> _logger;

        public PrintTrackingService(SchedulerContext context, ILogger<PrintTrackingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PrintTrackingDashboardViewModel> GetDashboardDataAsync(int userId)
        {
            try
            {
                var today = DateTime.Today;
                var user = await _context.Users.FindAsync(userId);

                // Get active builds with proper error handling
                var activeBuilds = await _context.BuildJobs
                    .Include(b => b.User)
                    .Where(b => b.Status == "In Progress")
                    .OrderBy(b => b.ActualStartTime)
                    .ToListAsync();

                // Get recent completed builds (last 24 hours)
                var recentCompleted = await _context.BuildJobs
                    .Include(b => b.User)
                    .Where(b => b.Status == "Completed" && b.CompletedAt >= today)
                    .OrderByDescending(b => b.CompletedAt)
                    .Take(10)
                    .ToListAsync();

                // Get recent delays
                var recentDelays = await _context.DelayLogs
                    .Include(d => d.BuildJob)
                    .Where(d => d.CreatedAt >= today)
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                // Calculate stats safely
                var activeByPrinter = activeBuilds
                    .GroupBy(b => b.PrinterName)
                    .ToDictionary(g => g.Key, g => g.Count());

                var hoursToday = new Dictionary<string, double>();
                try
                {
                    // FIX: Use client-side evaluation to avoid complex LINQ translation
                    var todayBuilds = await _context.BuildJobs
                        .Where(b => b.ActualStartTime >= today && b.Status == "Completed" && b.ActualEndTime.HasValue)
                        .Select(b => new { b.PrinterName, b.ActualStartTime, b.ActualEndTime })
                        .ToListAsync(); // Bring to client side first

                    // Calculate hours on client side
                    hoursToday = todayBuilds
                        .GroupBy(b => b.PrinterName)
                        .ToDictionary(
                            g => g.Key, 
                            g => g.Sum(b => (b.ActualEndTime!.Value - b.ActualStartTime).TotalHours)
                        );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error calculating today's hours, using empty data");
                    hoursToday = new Dictionary<string, double>();
                }

                return new PrintTrackingDashboardViewModel
                {
                    ActiveBuilds = activeBuilds,
                    RecentCompletedBuilds = recentCompleted,
                    RecentDelays = recentDelays,
                    ActiveJobsByPrinter = activeByPrinter,
                    HoursToday = hoursToday,
                    TotalDelaysToday = recentDelays.Count,
                    AverageDelayMinutes = recentDelays.Any() ? recentDelays.Average(d => d.DelayDuration) : 0,
                    OperatorName = user?.FullName ?? "Unknown",
                    UserId = userId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data for user {UserId}", userId);
                
                // Return safe fallback data
                return new PrintTrackingDashboardViewModel
                {
                    ActiveBuilds = new List<BuildJob>(),
                    RecentCompletedBuilds = new List<BuildJob>(),
                    RecentDelays = new List<DelayLog>(),
                    ActiveJobsByPrinter = new Dictionary<string, int>(),
                    HoursToday = new Dictionary<string, double>(),
                    TotalDelaysToday = 0,
                    AverageDelayMinutes = 0,
                    OperatorName = "Unknown",
                    UserId = userId
                };
            }
        }

        public async Task<int> StartPrintJobAsync(PrintStartViewModel model, int userId)
        {
            try
            {
                var buildJob = new BuildJob
                {
                    PrinterName = model.PrinterName,
                    ActualStartTime = model.ActualStartTime,
                    UserId = userId,
                    SetupNotes = model.SetupNotes,
                    AssociatedScheduledJobId = model.AssociatedScheduledJobId,
                    Status = "In Progress",
                    CreatedAt = DateTime.UtcNow
                };

                // If associated with scheduled job, update the scheduled job
                if (model.AssociatedScheduledJobId.HasValue)
                {
                    var scheduledJob = await _context.Jobs.FindAsync(model.AssociatedScheduledJobId.Value);
                    if (scheduledJob != null)
                    {
                        scheduledJob.Status = "In Progress";
                        scheduledJob.ActualStart = model.ActualStartTime;
                        buildJob.ScheduledStartTime = scheduledJob.ScheduledStart;
                        buildJob.ScheduledEndTime = scheduledJob.ScheduledEnd;
                    }
                }

                _context.BuildJobs.Add(buildJob);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Started print job {BuildId} on printer {PrinterName} by user {UserId}", 
                    buildJob.BuildId, buildJob.PrinterName, userId);

                return buildJob.BuildId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting print job for printer {PrinterName}", model.PrinterName);
                throw;
            }
        }

        public async Task<bool> CompletePrintJobAsync(PostPrintViewModel model, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                BuildJob buildJob;

                // Find or create build job
                if (model.BuildId.HasValue)
                {
                    buildJob = await _context.BuildJobs.FindAsync(model.BuildId.Value);
                    if (buildJob == null)
                    {
                        _logger.LogWarning("Build job {BuildId} not found, creating new one", model.BuildId);
                        buildJob = CreateBuildJobFromPostPrint(model, userId);
                        _context.BuildJobs.Add(buildJob);
                    }
                }
                else
                {
                    // Create new build job from post-print data
                    buildJob = CreateBuildJobFromPostPrint(model, userId);
                    _context.BuildJobs.Add(buildJob);
                }

                // Update build job completion data
                buildJob.ActualEndTime = model.ActualEndTime;
                buildJob.ReasonForEnd = model.ReasonForEnd;
                buildJob.LaserRunTime = model.LaserRunTime;
                buildJob.GasUsed_L = model.GasUsed_L;
                buildJob.PowderUsed_L = model.PowderUsed_L;
                buildJob.Notes = model.Notes;

                // Determine final status
                buildJob.Status = model.ReasonForEnd?.ToLower() switch
                {
                    "completed" => "Completed",
                    "aborted" => "Aborted", 
                    "error" => "Error",
                    _ => "Completed"
                };

                buildJob.CompletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Add parts
                await AddBuildJobPartsAsync(buildJob.BuildId, model.Parts, userId);

                // Handle delays if detected
                if (model.HasDelay && model.DelayInfo != null)
                {
                    await CreateDelayLogAsync(buildJob.BuildId, model.DelayInfo, userId);
                }

                // Create cooldown and changeover blocks
                await CreateCooldownAndChangeoverBlocksAsync(buildJob);

                // Update associated scheduled job if exists
                if (buildJob.AssociatedScheduledJobId.HasValue)
                {
                    await UpdateAssociatedScheduledJobAsync(buildJob.AssociatedScheduledJobId.Value, buildJob);
                }

                await transaction.CommitAsync();

                _logger.LogInformation("Completed print job {BuildId} on printer {PrinterName}", 
                    buildJob.BuildId, buildJob.PrinterName);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error completing print job for printer {PrinterName}", model.PrinterName);
                throw;
            }
        }

        public async Task<List<Job>> GetAvailableScheduledJobsAsync(string printerName)
        {
            var today = DateTime.Today;
            var endOfWeek = today.AddDays(7);

            return await _context.Jobs
                .Where(j => j.MachineId == printerName && 
                           j.Status == "Scheduled" &&
                           j.ScheduledStart >= today &&
                           j.ScheduledStart <= endOfWeek)
                .OrderBy(j => j.ScheduledStart)
                .ToListAsync();
        }

        public async Task<BuildJob?> GetActiveBuildJobAsync(string printerName)
        {
            return await _context.BuildJobs
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.PrinterName == printerName && b.Status == "In Progress");
        }

        public async Task<bool> HasActiveBuildAsync(string printerName)
        {
            return await _context.BuildJobs
                .AnyAsync(b => b.PrinterName == printerName && b.Status == "In Progress");
        }

        public async Task<List<BuildJob>> GetRecentBuildsAsync(int count = 20)
        {
            return await _context.BuildJobs
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task CreateCooldownAndChangeoverBlocksAsync(BuildJob completedJob)
        {
            try
            {
                var endTime = completedJob.ActualEndTime ?? DateTime.UtcNow;

                // Create 1-hour cooldown block
                var cooldownJob = new Job
                {
                    MachineId = completedJob.PrinterName,
                    PartNumber = "COOLDOWN",
                    PartId = 1, // Assume system part ID
                    ScheduledStart = endTime,
                    ScheduledEnd = endTime.AddHours(1),
                    Status = "Cooldown",
                    Quantity = 1,
                    Priority = 1, // High priority to prevent scheduling over it
                    CreatedBy = "System",
                    LastModifiedBy = "System",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Notes = $"Auto-generated cooldown after job {completedJob.BuildId}",
                    // Make it visually distinct
                    EstimatedHours = 1
                };

                // Create 3-hour changeover block
                var changeoverJob = new Job
                {
                    MachineId = completedJob.PrinterName,
                    PartNumber = "CHANGEOVER",
                    PartId = 1, // Assume system part ID
                    ScheduledStart = endTime.AddHours(1),
                    ScheduledEnd = endTime.AddHours(4),
                    Status = "Changeover",
                    Quantity = 1,
                    Priority = 1, // High priority to prevent scheduling over it
                    CreatedBy = "System",
                    LastModifiedBy = "System",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Notes = $"Auto-generated changeover after job {completedJob.BuildId}",
                    EstimatedHours = 3
                };

                _context.Jobs.AddRange(cooldownJob, changeoverJob);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created cooldown and changeover blocks for printer {PrinterName} after job {BuildId}", 
                    completedJob.PrinterName, completedJob.BuildId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cooldown/changeover blocks for job {BuildId}", completedJob.BuildId);
                // Don't throw - this is not critical to the main workflow
            }
        }

        #region Private Helper Methods

        private BuildJob CreateBuildJobFromPostPrint(PostPrintViewModel model, int userId)
        {
            return new BuildJob
            {
                PrinterName = model.PrinterName,
                ActualStartTime = model.ActualStartTime,
                UserId = userId,
                ScheduledStartTime = model.ScheduledStartTime,
                Status = "In Progress", // Will be updated to final status
                CreatedAt = DateTime.UtcNow
            };
        }

        private async Task AddBuildJobPartsAsync(int buildId, List<PostPrintPartEntry> parts, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            var createdBy = user?.Username ?? "System";

            for (int i = 0; i < parts.Count; i++)
            {
                var partEntry = parts[i];
                var buildJobPart = new BuildJobPart
                {
                    BuildId = buildId,
                    PartNumber = partEntry.PartNumber,
                    Quantity = partEntry.Quantity,
                    IsPrimary = i == 0 || partEntry.IsPrimary, // First part is always primary
                    Description = partEntry.Description,
                    Material = partEntry.Material,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                // Try to get additional info from Parts library
                var existingPart = await _context.Parts
                    .FirstOrDefaultAsync(p => p.PartNumber == partEntry.PartNumber);
                
                if (existingPart != null)
                {
                    buildJobPart.Description = existingPart.Description;
                    buildJobPart.Material = existingPart.Material;
                    buildJobPart.EstimatedHours = existingPart.EstimatedHours;
                }

                _context.BuildJobParts.Add(buildJobPart);
            }

            await _context.SaveChangesAsync();
        }

        private async Task CreateDelayLogAsync(int buildId, DelayInfo delayInfo, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            var createdBy = user?.Username ?? "System";

            var delayLog = new DelayLog
            {
                BuildId = buildId,
                DelayReason = delayInfo.DelayReason,
                DelayDuration = delayInfo.DelayDuration,
                Description = delayInfo.DelayNotes,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.DelayLogs.Add(delayLog);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateAssociatedScheduledJobAsync(int scheduledJobId, BuildJob buildJob)
        {
            var scheduledJob = await _context.Jobs.FindAsync(scheduledJobId);
            if (scheduledJob != null)
            {
                scheduledJob.ActualStart = buildJob.ActualStartTime;
                scheduledJob.ActualEnd = buildJob.ActualEndTime;
                scheduledJob.Status = buildJob.Status;
                scheduledJob.LastModifiedDate = DateTime.UtcNow;
                scheduledJob.LastModifiedBy = "PrintTrackingSystem";
                
                await _context.SaveChangesAsync();
            }
        }

        #endregion
    }
}