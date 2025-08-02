using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.ViewModels.PrintTracking;

namespace OpCentrix.Services
{
    public interface IPrintTrackingService
    {
        Task<PrintTrackingDashboardViewModel> GetDashboardDataAsync(int userId);
        Task<int> StartPrintJobAsync(PrintStartViewModel model, int userId);
        Task<bool> CompletePrintJobAsync(PostPrintViewModel model, int userId);
        Task<List<Job>> GetAvailableScheduledJobsAsync(string printerName);
        Task<List<JobStage>> GetAvailableJobStagesAsync(string printerName);
        Task<List<PrototypeJob>> GetAvailablePrototypeJobsAsync();
        Task<BuildJob?> GetActiveBuildJobAsync(string printerName);
        Task<bool> HasActiveBuildAsync(string printerName);
        Task<List<BuildJob>> GetRecentBuildsAsync(int count = 20);
        Task CreateCooldownAndChangeoverBlocksAsync(BuildJob completedJob);
        Task<MultiStageWorkflowViewModel> GetWorkflowStatusAsync(int jobId);
        Task<bool> AdvanceJobStageAsync(int jobStageId, int userId);
        Task<bool> UpdateStageProgressAsync(int jobStageId, double progressPercent, string? statusUpdate = null);
        Task<List<Part>> GetAvailablePartsAsync();
        Task<bool> ValidatePartCompatibilityAsync(string partNumber, string machineId);
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

                // Get active builds with enhanced data
                var activeBuilds = await _context.BuildJobs
                    .Include(b => b.User)
                    .Include(b => b.Part)
                    .Where(b => b.Status == "In Progress")
                    .OrderBy(b => b.ActualStartTime)
                    .ToListAsync();

                // Get recent completed builds (last 24 hours)
                var recentCompleted = await _context.BuildJobs
                    .Include(b => b.User)
                    .Include(b => b.Part)
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

                // Get active job stages
                var activeJobStages = await GetActiveJobStagesAsync();

                // Get active prototype jobs
                var activePrototypeJobs = await GetActivePrototypeJobsAsync();

                // Get production stages info
                var productionStages = await GetProductionStagesInfoAsync();

                // Get upcoming jobs from master schedule
                var upcomingJobs = await GetUpcomingJobsAsync();

                // Get delayed jobs
                var delayedJobs = await GetDelayedJobsAsync();

                // Get active alerts  
                var activeAlerts = await GetActiveAlertsAsync();

                // Calculate stats safely
                var activeByPrinter = activeBuilds
                    .GroupBy(b => b.PrinterName)
                    .ToDictionary(g => g.Key, g => g.Count());

                var hoursToday = await CalculateHoursTodayAsync();
                var jobsByStage = await CalculateJobsByStageAsync();
                var utilizationByMachine = await CalculateUtilizationByMachineAsync();
                var capacityUtilization = await CalculateCapacityUtilizationAsync();
                var queueDepth = await CalculateQueueDepthAsync();
                var maintenanceAlerts = await GetMaintenanceAlertsAsync();

                // Calculate performance metrics
                var metrics = await CalculatePerformanceMetricsAsync();

                return new PrintTrackingDashboardViewModel
                {
                    ActiveBuilds = activeBuilds,
                    RecentCompletedBuilds = recentCompleted,
                    RecentDelays = recentDelays,
                    ActiveJobStages = activeJobStages,
                    ActivePrototypeJobs = activePrototypeJobs,
                    ProductionStages = productionStages,
                    UpcomingJobs = upcomingJobs,
                    DelayedJobs = delayedJobs,
                    ActiveAlerts = activeAlerts,
                    
                    // Stats
                    ActiveJobsByPrinter = activeByPrinter,
                    HoursToday = hoursToday,
                    JobsByStage = jobsByStage,
                    UtilizationByMachine = utilizationByMachine,
                    
                    // Performance metrics
                    TotalDelaysToday = recentDelays.Count,
                    AverageDelayMinutes = recentDelays.Any() ? recentDelays.Average(d => d.DelayDuration) : 0,
                    OverallEfficiency = metrics.Efficiency,
                    QualityScore = metrics.QualityScore,
                    TotalCostToday = metrics.TotalCost,
                    PartsProducedToday = metrics.PartsProduced,
                    
                    // Capacity and resources
                    CapacityUtilization = capacityUtilization,
                    QueueDepth = queueDepth,
                    MaintenanceAlerts = maintenanceAlerts,
                    
                    // User info
                    OperatorName = user?.FullName ?? "Unknown",
                    UserId = userId,
                    UserRole = user?.Role ?? "User",
                    UserPermissions = await GetUserPermissionsAsync(userId),
                    
                    // Dashboard config
                    RefreshTime = DateTime.Now,
                    RefreshIntervalSeconds = 30,
                    ShowAlerts = true,
                    ShowMetrics = true,
                    ShowQueues = true
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
                    ActiveJobStages = new List<JobStageInfo>(),
                    ActivePrototypeJobs = new List<PrototypeJobInfo>(),
                    ProductionStages = new List<ProductionStageInfo>(),
                    UpcomingJobs = new List<MasterScheduleJobInfo>(),
                    DelayedJobs = new List<MasterScheduleJobInfo>(),
                    ActiveAlerts = new List<AlertInfo>(),
                    ActiveJobsByPrinter = new Dictionary<string, int>(),
                    HoursToday = new Dictionary<string, double>(),
                    JobsByStage = new Dictionary<string, int>(),
                    UtilizationByMachine = new Dictionary<string, double>(),
                    CapacityUtilization = new Dictionary<string, double>(),
                    QueueDepth = new Dictionary<string, int>(),
                    MaintenanceAlerts = new List<MaintenanceAlert>(),
                    TotalDelaysToday = 0,
                    AverageDelayMinutes = 0,
                    OverallEfficiency = 0,
                    QualityScore = 100,
                    TotalCostToday = 0,
                    PartsProducedToday = 0,
                    OperatorName = "Unknown",
                    UserId = userId,
                    UserRole = "User",
                    UserPermissions = new List<string>()
                };
            }
        }

        public async Task<int> StartPrintJobAsync(PrintStartViewModel model, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var buildJob = new BuildJob
                {
                    PrinterName = model.PrinterName,
                    ActualStartTime = model.ActualStartTime,
                    UserId = userId,
                    SetupNotes = model.SetupNotes,
                    AssociatedScheduledJobId = model.AssociatedScheduledJobId,
                    PartId = model.PartId,
                    Status = "In Progress",
                    CreatedAt = DateTime.UtcNow
                };

                // Handle scheduled job association
                if (model.AssociatedScheduledJobId.HasValue)
                {
                    await UpdateScheduledJobForStartAsync(model.AssociatedScheduledJobId.Value, model.ActualStartTime, buildJob);
                }

                // Handle job stage association
                if (model.JobStageId.HasValue)
                {
                    await UpdateJobStageForStartAsync(model.JobStageId.Value, model.ActualStartTime);
                }

                // Handle prototype job association
                if (model.PrototypeJobId.HasValue)
                {
                    await UpdatePrototypeJobForStartAsync(model.PrototypeJobId.Value, model.ActualStartTime);
                }

                _context.BuildJobs.Add(buildJob);
                await _context.SaveChangesAsync();

                // Check for delays and create delay log if needed
                if (model.IsDelayed)
                {
                    await CreateDelayLogAsync(buildJob.BuildId, new DelayInfo
                    {
                        DelayReason = DetermineDelayReason(model),
                        DelayDuration = model.DelayMinutes,
                        DelayNotes = $"Build started {model.DelayMinutes} minutes late"
                    }, userId);
                }

                await transaction.CommitAsync();

                _logger.LogInformation("Started print job {BuildId} on printer {PrinterName} by user {UserId}", 
                    buildJob.BuildId, buildJob.PrinterName, userId);

                return buildJob.BuildId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
                if (model.BuildId > 0)
                {
                    buildJob = await _context.BuildJobs
                        .Include(b => b.Part)
                        .FirstOrDefaultAsync(b => b.BuildId == model.BuildId);
                    
                    if (buildJob == null)
                    {
                        _logger.LogWarning("Build job {BuildId} not found, creating new one", model.BuildId);
                        buildJob = CreateBuildJobFromPostPrint(model, userId);
                        _context.BuildJobs.Add(buildJob);
                    }
                }
                else
                {
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

                // Determine final status based on reason
                buildJob.Status = DetermineFinalStatus(model.ReasonForEnd);
                buildJob.CompletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Add parts with enhanced tracking
                await AddBuildJobPartsAsync(buildJob.BuildId, model.Parts, userId);

                // Handle delays if detected
                if (model.HasDelay && model.DelayInfo != null)
                {
                    await CreateDelayLogAsync(buildJob.BuildId, model.DelayInfo, userId);
                }

                // Update associated job stage
                if (model.JobStageId.HasValue)
                {
                    await UpdateJobStageForCompletionAsync(model.JobStageId.Value, model);
                }

                // Update production stage execution
                if (model.ProductionStageExecutionId.HasValue)
                {
                    await UpdateProductionStageExecutionAsync(model.ProductionStageExecutionId.Value, model);
                }

                // Update associated scheduled job
                if (buildJob.AssociatedScheduledJobId.HasValue)
                {
                    await UpdateAssociatedScheduledJobAsync(buildJob.AssociatedScheduledJobId.Value, buildJob);
                }

                // Create cooldown and changeover blocks if completed successfully
                if (buildJob.Status == "Completed")
                {
                    await CreateCooldownAndChangeoverBlocksAsync(buildJob);
                }

                // Advance to next stage if applicable
                if (model.NextStageReady && model.JobStageId.HasValue)
                {
                    await TryAdvanceToNextStageAsync(model.JobStageId.Value, userId);
                }

                await transaction.CommitAsync();

                _logger.LogInformation("Completed print job {BuildId} on printer {PrinterName} with status {Status}", 
                    buildJob.BuildId, buildJob.PrinterName, buildJob.Status);

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
                .Include(j => j.Part)
                .Where(j => j.MachineId == printerName && 
                           j.Status == "Scheduled" &&
                           j.ScheduledStart >= today &&
                           j.ScheduledStart <= endOfWeek)
                .OrderBy(j => j.ScheduledStart)
                .ToListAsync();
        }

        public async Task<List<JobStage>> GetAvailableJobStagesAsync(string printerName)
        {
            var today = DateTime.Today;
            var endOfWeek = today.AddDays(7);

            return await _context.JobStages
                .Include(js => js.Job)
                    .ThenInclude(j => j.Part)
                .Where(js => js.MachineId == printerName &&
                            js.Status == "Scheduled" &&
                            js.ScheduledStart >= today &&
                            js.ScheduledStart <= endOfWeek &&
                            js.CanStart)
                .OrderBy(js => js.ScheduledStart)
                .ToListAsync();
        }

        public async Task<List<PrototypeJob>> GetAvailablePrototypeJobsAsync()
        {
            return await _context.PrototypeJobs
                .Include(pj => pj.Part)
                .Where(pj => pj.Status == "InProgress" && 
                            pj.IsActive)
                .OrderBy(pj => pj.Priority)
                .ThenBy(pj => pj.RequestDate)
                .ToListAsync();
        }

        public async Task<BuildJob?> GetActiveBuildJobAsync(string printerName)
        {
            return await _context.BuildJobs
                .Include(b => b.User)
                .Include(b => b.Part)
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
                .Include(b => b.Part)
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<MultiStageWorkflowViewModel> GetWorkflowStatusAsync(int jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.Part)
                .Include(j => j.JobStages)
                    .ThenInclude(js => js.StageNotes)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
                throw new ArgumentException($"Job {jobId} not found");

            var stages = job.JobStages
                .OrderBy(js => js.ExecutionOrder)
                .Select(js => new WorkflowStageInfo
                {
                    StageId = js.Id,
                    Name = js.StageName,
                    Type = js.StageType,
                    Department = js.Department,
                    Status = js.Status,
                    ExecutionOrder = js.ExecutionOrder,
                    ScheduledStart = js.ScheduledStart,
                    ScheduledEnd = js.ScheduledEnd,
                    ActualStart = js.ActualStart,
                    ActualEnd = js.ActualEnd,
                    ProgressPercent = js.ProgressPercent,
                    AssignedOperator = js.AssignedOperator ?? string.Empty,
                    MachineId = js.MachineId ?? string.Empty,
                    IsCompleted = js.Status == "Completed",
                    IsActive = js.Status == "In Progress",
                    CanStart = js.CanStart,
                    RequiresApproval = false // Would need to be determined from business rules
                }).ToList();

            var currentStageIndex = stages.FindIndex(s => s.IsActive);
            if (currentStageIndex == -1)
                currentStageIndex = stages.FindIndex(s => !s.IsCompleted);

            var overallProgress = stages.Any() 
                ? stages.Average(s => s.ProgressPercent) 
                : 0;

            return new MultiStageWorkflowViewModel
            {
                JobId = jobId,
                PartNumber = job.PartNumber,
                PartDescription = job.Part?.Description ?? "Unknown",
                Stages = stages,
                CurrentStageIndex = Math.Max(0, currentStageIndex),
                OverallProgress = overallProgress,
                OverallStatus = job.Status,
                StartDate = job.ActualStart,
                EstimatedCompletionDate = job.ScheduledEnd,
                ActualCompletionDate = job.ActualEnd,
                Alerts = new List<WorkflowAlert>(), // Would be populated based on business rules
                CanAdvanceToNextStage = CanAdvanceToNextStage(stages, currentStageIndex),
                NextStageBlockedReason = GetNextStageBlockedReason(stages, currentStageIndex)
            };
        }

        public async Task<bool> AdvanceJobStageAsync(int jobStageId, int userId)
        {
            try
            {
                var jobStage = await _context.JobStages
                    .Include(js => js.Job)
                    .FirstOrDefaultAsync(js => js.Id == jobStageId);

                if (jobStage == null)
                    return false;

                // Start the stage
                jobStage.Status = "In Progress";
                jobStage.ActualStart = DateTime.UtcNow;
                jobStage.CanStart = true;

                // Add stage note
                var stageNote = new StageNote
                {
                    StageId = jobStageId,
                    Note = $"Stage started by user {userId}",
                    NoteType = "System",
                    Priority = 3,
                    IsPublic = true,
                    CreatedBy = $"User-{userId}",
                    CreatedDate = DateTime.UtcNow
                };

                _context.StageNotes.Add(stageNote);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Advanced job stage {JobStageId} for job {JobId} by user {UserId}", 
                    jobStageId, jobStage.JobId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error advancing job stage {JobStageId}", jobStageId);
                return false;
            }
        }

        public async Task<bool> UpdateStageProgressAsync(int jobStageId, double progressPercent, string? statusUpdate = null)
        {
            try
            {
                var jobStage = await _context.JobStages.FindAsync(jobStageId);
                if (jobStage == null)
                    return false;

                jobStage.ProgressPercent = Math.Clamp(progressPercent, 0, 100);
                
                if (!string.IsNullOrEmpty(statusUpdate))
                {
                    var stageNote = new StageNote
                    {
                        StageId = jobStageId,
                        Note = statusUpdate,
                        NoteType = "Progress",
                        Priority = 3,
                        IsPublic = true,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow
                    };
                    
                    _context.StageNotes.Add(stageNote);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stage progress for {JobStageId}", jobStageId);
                return false;
            }
        }

        public async Task<List<Part>> GetAvailablePartsAsync()
        {
            return await _context.Parts
                .Where(p => p.IsActive)
                .OrderBy(p => p.PartNumber)
                .ToListAsync();
        }

        public async Task<bool> ValidatePartCompatibilityAsync(string partNumber, string machineId)
        {
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.PartNumber == partNumber);

            if (part == null)
                return false;

            // Check if machine is in preferred machines list
            var preferredMachines = part.PreferredMachines?.Split(',') ?? Array.Empty<string>();
            if (preferredMachines.Contains(machineId))
                return true;

            // Check if machine type is compatible
            var machine = await _context.Machines
                .FirstOrDefaultAsync(m => m.MachineId == machineId);

            if (machine == null)
                return false;

            // Add business logic for compatibility checking
            return machine.MachineType == part.RequiredMachineType;
        }

        public async Task CreateCooldownAndChangeoverBlocksAsync(BuildJob completedJob)
        {
            try
            {
                var endTime = completedJob.ActualEndTime ?? DateTime.UtcNow;

                // Find a system part for cooldown/changeover blocks
                var systemPart = await _context.Parts
                    .FirstOrDefaultAsync(p => p.PartNumber == "SYSTEM-BLOCK");

                var partId = systemPart?.Id ?? 1; // Fallback to ID 1 if no system part exists

                // Create 1-hour cooldown block
                var cooldownJob = new Job
                {
                    MachineId = completedJob.PrinterName,
                    PartNumber = "COOLDOWN",
                    PartId = partId,
                    ScheduledStart = endTime,
                    ScheduledEnd = endTime.AddHours(1),
                    Status = "System Block",
                    Quantity = 1,
                    Priority = 1,
                    CreatedBy = "System",
                    LastModifiedBy = "System",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Notes = $"Auto-generated cooldown after build {completedJob.BuildId}",
                    EstimatedHours = 1,
                    EstimatedDuration = TimeSpan.FromHours(1)
                };

                // Create 3-hour changeover block  
                var changeoverJob = new Job
                {
                    MachineId = completedJob.PrinterName,
                    PartNumber = "CHANGEOVER",
                    PartId = partId,
                    ScheduledStart = endTime.AddHours(1),
                    ScheduledEnd = endTime.AddHours(4),
                    Status = "System Block",
                    Quantity = 1,
                    Priority = 1,
                    CreatedBy = "System",
                    LastModifiedBy = "System",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Notes = $"Auto-generated changeover after build {completedJob.BuildId}",
                    EstimatedHours = 3,
                    EstimatedDuration = TimeSpan.FromHours(3)
                };

                _context.Jobs.AddRange(cooldownJob, changeoverJob);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created cooldown and changeover blocks for printer {PrinterName} after build {BuildId}", 
                    completedJob.PrinterName, completedJob.BuildId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cooldown/changeover blocks for build {BuildId}", completedJob.BuildId);
                // Don't throw - this is not critical to the main workflow
            }
        }

        #region Private Helper Methods

        private async Task<List<JobStageInfo>> GetActiveJobStagesAsync()
        {
            var activeStages = await _context.JobStages
                .Include(js => js.Job)
                    .ThenInclude(j => j.Part)
                .Where(js => js.Status == "In Progress" || js.Status == "Scheduled")
                .OrderBy(js => js.ScheduledStart)
                .Take(20)
                .ToListAsync();

            return activeStages.Select(js => new JobStageInfo
            {
                JobStageId = js.Id,
                JobId = js.JobId,
                StageName = js.StageName,
                StageType = js.StageType,
                Department = js.Department,
                Status = js.Status,
                MachineId = js.MachineId ?? "",
                MachineName = GetMachineName(js.MachineId ?? ""),
                ScheduledStart = js.ScheduledStart,
                ScheduledEnd = js.ScheduledEnd,
                ActualStart = js.ActualStart,
                ProgressPercent = js.ProgressPercent,
                PartNumber = js.Job.PartNumber,
                AssignedOperator = js.AssignedOperator ?? "",
                Priority = js.Priority,
                CanStart = js.CanStart
            }).ToList();
        }

        private async Task<List<PrototypeJobInfo>> GetActivePrototypeJobsAsync()
        {
            var activePrototypes = await _context.PrototypeJobs
                .Include(pj => pj.Part)
                .Include(pj => pj.StageExecutions)
                .Where(pj => pj.Status == "InProgress" && pj.IsActive)
                .OrderBy(pj => pj.Priority)
                .Take(10)
                .ToListAsync();

            return activePrototypes.Select(pj => new PrototypeJobInfo
            {
                PrototypeJobId = pj.Id,
                PrototypeNumber = pj.PrototypeNumber,
                PartNumber = pj.Part?.PartNumber ?? "",
                Status = pj.Status,
                Priority = pj.Priority,
                RequestedBy = pj.RequestedBy,
                RequestDate = pj.RequestDate,
                StartDate = pj.StartDate,
                CompletionDate = pj.CompletionDate,
                TotalEstimatedCost = pj.TotalEstimatedCost,
                TotalActualCost = pj.TotalActualCost,
                TotalEstimatedHours = (double)pj.TotalEstimatedHours,
                TotalActualHours = (double)pj.TotalActualHours,
                CompletedStages = pj.StageExecutions.Count(se => se.Status == "Completed"),
                TotalStages = pj.StageExecutions.Count,
                OverallProgress = pj.StageExecutions.Any() 
                    ? pj.StageExecutions.Average(se => se.Status == "Completed" ? 100 : 0) 
                    : 0
            }).ToList();
        }

        private async Task<List<ProductionStageInfo>> GetProductionStagesInfoAsync()
        {
            var stages = await _context.ProductionStages
                .Include(ps => ps.StageExecutions)
                .Where(ps => ps.IsActive)
                .OrderBy(ps => ps.DisplayOrder)
                .ToListAsync();

            return stages.Select(ps => new ProductionStageInfo
            {
                ProductionStageId = ps.Id,
                Name = ps.Name,
                Description = ps.Description ?? "",
                DisplayOrder = ps.DisplayOrder,
                IsActive = ps.IsActive,
                ActiveExecutions = ps.StageExecutions.Count(se => se.Status == "InProgress"),
                QueuedExecutions = ps.StageExecutions.Count(se => se.Status == "NotStarted"),
                AverageCompletionTime = (double)ps.StageExecutions
                    .Where(se => se.ActualHours.HasValue)
                    .Select(se => se.ActualHours!.Value)
                    .DefaultIfEmpty(0)
                    .Average(),
                AverageCost = ps.StageExecutions
                    .Where(se => se.ActualCost.HasValue)
                    .Select(se => se.ActualCost!.Value)
                    .DefaultIfEmpty(0)
                    .Average(),
                RequiresQualityCheck = ps.RequiresQualityCheck,
                RequiresApproval = ps.RequiresApproval,
                RequiredRole = ps.RequiredRole ?? ""
            }).ToList();
        }

        private async Task<List<MasterScheduleJobInfo>> GetUpcomingJobsAsync()
        {
            var upcomingJobs = await _context.Jobs
                .Where(j => j.Status == "Scheduled" && 
                           j.ScheduledStart >= DateTime.Now &&
                           j.ScheduledStart <= DateTime.Now.AddDays(7))
                .OrderBy(j => j.ScheduledStart)
                .Take(10)
                .ToListAsync();

            return upcomingJobs.Select(j => new MasterScheduleJobInfo
            {
                JobId = j.Id,
                PartNumber = j.PartNumber,
                MachineId = j.MachineId,
                MachineName = GetMachineName(j.MachineId),
                Status = j.Status,
                ScheduledStart = j.ScheduledStart,
                ScheduledEnd = j.ScheduledEnd,
                ActualStart = j.ActualStart,
                Priority = j.Priority,
                ProgressPercent = 0,
                IsDelayed = false,
                DelayMinutes = 0,
                RequiresAttention = false
            }).ToList();
        }

        private async Task<List<MasterScheduleJobInfo>> GetDelayedJobsAsync()
        {
            var now = DateTime.Now;
            var delayedJobs = await _context.Jobs
                .Where(j => j.Status == "In Progress" && 
                           j.ScheduledEnd < now)
                .OrderBy(j => j.ScheduledEnd)
                .Take(10)
                .ToListAsync();

            return delayedJobs.Select(j => new MasterScheduleJobInfo
            {
                JobId = j.Id,
                PartNumber = j.PartNumber,
                MachineId = j.MachineId,
                MachineName = GetMachineName(j.MachineId),
                Status = j.Status,
                ScheduledStart = j.ScheduledStart,
                ScheduledEnd = j.ScheduledEnd,
                ActualStart = j.ActualStart,
                Priority = j.Priority,
                ProgressPercent = 50, // Estimate - would need better tracking
                IsDelayed = true,
                DelayMinutes = (int)(now - j.ScheduledEnd).TotalMinutes,
                RequiresAttention = true
            }).ToList();
        }

        private async Task<List<AlertInfo>> GetActiveAlertsAsync()
        {
            // This would integrate with an alerts system
            var alerts = new List<AlertInfo>();
            
            // Check for overdue jobs
            var overdueJobs = await _context.Jobs
                .Where(j => j.Status == "In Progress" && j.ScheduledEnd < DateTime.Now)
                .ToListAsync();

            foreach (var job in overdueJobs)
            {
                alerts.Add(new AlertInfo
                {
                    AlertId = job.Id,
                    Type = "delay",
                    Severity = "high",
                    Title = $"Job Overdue: {job.PartNumber}",
                    Description = $"Job on {job.MachineId} is {(DateTime.Now - job.ScheduledEnd).TotalHours:F1} hours overdue",
                    MachineId = job.MachineId,
                    JobId = job.Id.ToString(),
                    CreatedDate = DateTime.Now,
                    IsAcknowledged = false,
                    CreatedBy = "System"
                });
            }

            return alerts;
        }

        private async Task<Dictionary<string, double>> CalculateHoursTodayAsync()
        {
            var today = DateTime.Today;
            var todayBuilds = await _context.BuildJobs
                .Where(b => b.ActualStartTime >= today && 
                           b.Status == "Completed" && 
                           b.ActualEndTime.HasValue)
                .Select(b => new { b.PrinterName, b.ActualStartTime, b.ActualEndTime })
                .ToListAsync();

            return todayBuilds
                .GroupBy(b => b.PrinterName)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Sum(b => (b.ActualEndTime!.Value - b.ActualStartTime).TotalHours)
                );
        }

        private async Task<Dictionary<string, int>> CalculateJobsByStageAsync()
        {
            var jobsByStage = await _context.JobStages
                .GroupBy(js => js.StageType)
                .Select(g => new { StageType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StageType, x => x.Count);

            return jobsByStage;
        }

        private async Task<Dictionary<string, double>> CalculateUtilizationByMachineAsync()
        {
            var utilization = new Dictionary<string, double>();
            
            var machines = await _context.Machines
                .Where(m => m.IsActive)
                .Select(m => m.MachineId)
                .ToListAsync();

            foreach (var machineId in machines)
            {
                var todayStart = DateTime.Today;
                var todayEnd = DateTime.Today.AddDays(1);
                
                // Get builds and calculate time on client side to avoid EF limitations
                var builds = await _context.BuildJobs
                    .Where(b => b.PrinterName == machineId &&
                               b.ActualStartTime >= todayStart &&
                               b.ActualEndTime < todayEnd &&
                               b.ActualEndTime.HasValue)
                    .Select(b => new { b.ActualStartTime, b.ActualEndTime })
                    .ToListAsync();

                var activeTimeSeconds = builds
                    .Sum(b => (b.ActualEndTime!.Value - b.ActualStartTime).TotalSeconds);

                var totalSeconds = (todayEnd - todayStart).TotalSeconds;
                utilization[machineId] = totalSeconds > 0 ? (activeTimeSeconds / totalSeconds) * 100 : 0;
            }

            return utilization;
        }

        private async Task<Dictionary<string, double>> CalculateCapacityUtilizationAsync()
        {
            // Placeholder for capacity utilization calculation
            var machines = await _context.Machines
                .Where(m => m.IsActive)
                .Select(m => m.MachineId)
                .ToListAsync();

            return machines.ToDictionary(m => m, m => 75.0); // Placeholder values
        }

        private async Task<Dictionary<string, int>> CalculateQueueDepthAsync()
        {
            var queueDepth = await _context.Jobs
                .Where(j => j.Status == "Scheduled")
                .GroupBy(j => j.MachineId)
                .Select(g => new { MachineId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.MachineId, x => x.Count);

            return queueDepth;
        }

        private async Task<List<MaintenanceAlert>> GetMaintenanceAlertsAsync()
        {
            var alerts = new List<MaintenanceAlert>();
            var machines = await _context.Machines
                .Where(m => m.IsActive && m.NextMaintenanceDate.HasValue)
                .ToListAsync();

            foreach (var machine in machines)
            {
                if (machine.NextMaintenanceDate <= DateTime.Today.AddDays(7))
                {
                    var daysOverdue = (DateTime.Today - machine.NextMaintenanceDate.Value).Days;
                    alerts.Add(new MaintenanceAlert
                    {
                        MachineId = machine.MachineId,
                        MachineName = machine.Name,
                        AlertType = "Scheduled Maintenance",
                        Description = $"Maintenance due for {machine.Name}",
                        DueDate = machine.NextMaintenanceDate.Value,
                        DaysOverdue = Math.Max(0, daysOverdue),
                        Severity = daysOverdue > 7 ? "Critical" : daysOverdue > 0 ? "High" : "Medium"
                    });
                }
            }

            return alerts;
        }

        private async Task<(double Efficiency, double QualityScore, decimal TotalCost, int PartsProduced)> CalculatePerformanceMetricsAsync()
        {
            var today = DateTime.Today;
            
            var todayBuilds = await _context.BuildJobs
                .Where(b => b.ActualStartTime >= today && b.Status == "Completed")
                .ToListAsync();

            // Get parts produced from BuildJobParts table
            var buildIds = todayBuilds.Select(b => b.BuildId).ToList();
            var partsProduced = await _context.BuildJobParts
                .Where(p => buildIds.Contains(p.BuildId))
                .SumAsync(p => p.Quantity);

            var efficiency = todayBuilds.Any() 
                ? todayBuilds.Average(b => b.ActualEndTime.HasValue 
                    ? Math.Min(100, (b.ScheduledEndTime?.Subtract(b.ScheduledStartTime ?? b.ActualStartTime).TotalHours ?? 8) / 
                               b.ActualEndTime.Value.Subtract(b.ActualStartTime).TotalHours * 100) 
                    : 0) 
                : 0;

            var qualityScore = 100.0; // Placeholder - would calculate from quality data
            var totalCost = 0m; // Placeholder - would calculate from cost tracking

            return (efficiency, qualityScore, totalCost, partsProduced);
        }

        private async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new List<string>();

            // This would integrate with the role permissions system
            return user.Role switch
            {
                "Admin" => new List<string> { "ViewAll", "EditAll", "DeleteAll", "ManageUsers", "ManageSystem" },
                "Supervisor" => new List<string> { "ViewAll", "EditOwn", "ManageTeam" },
                "Operator" => new List<string> { "ViewOwn", "EditOwn" },
                _ => new List<string> { "ViewOwn" }
            };
        }

        private async Task UpdateScheduledJobForStartAsync(int jobId, DateTime actualStartTime, BuildJob buildJob)
        {
            var scheduledJob = await _context.Jobs.FindAsync(jobId);
            if (scheduledJob != null)
            {
                scheduledJob.Status = "In Progress";
                scheduledJob.ActualStart = actualStartTime;
                buildJob.ScheduledStartTime = scheduledJob.ScheduledStart;
                buildJob.ScheduledEndTime = scheduledJob.ScheduledEnd;
            }
        }

        private async Task UpdateJobStageForStartAsync(int jobStageId, DateTime actualStartTime)
        {
            var jobStage = await _context.JobStages.FindAsync(jobStageId);
            if (jobStage != null)
            {
                jobStage.Status = "In Progress";
                jobStage.ActualStart = actualStartTime;
            }
        }

        private async Task UpdatePrototypeJobForStartAsync(int prototypeJobId, DateTime actualStartTime)
        {
            var prototypeJob = await _context.PrototypeJobs.FindAsync(prototypeJobId);
            if (prototypeJob != null && prototypeJob.StartDate == null)
            {
                prototypeJob.StartDate = actualStartTime;
                prototypeJob.Status = "InProgress";
            }
        }

        private async Task UpdateJobStageForCompletionAsync(int jobStageId, PostPrintViewModel model)
        {
            var jobStage = await _context.JobStages.FindAsync(jobStageId);
            if (jobStage != null)
            {
                jobStage.Status = model.StageStatus;
                jobStage.ActualEnd = model.ActualEndTime;
                jobStage.ProgressPercent = 100;
                
                if (!string.IsNullOrEmpty(model.QualityNotes))
                {
                    var note = new StageNote
                    {
                        StageId = jobStageId,
                        Note = model.QualityNotes,
                        NoteType = "Quality",
                        Priority = 3,
                        IsPublic = true,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.StageNotes.Add(note);
                }
            }
        }

        private async Task UpdateProductionStageExecutionAsync(int executionId, PostPrintViewModel model)
        {
            var execution = await _context.ProductionStageExecutions.FindAsync(executionId);
            if (execution != null)
            {
                execution.Status = model.StageStatus;
                execution.CompletionDate = model.ActualEndTime;
                execution.ActualHours = (decimal)model.ActualHours;
                execution.QualityCheckPassed = model.QualityCheckPassed;
                execution.QualityNotes = model.QualityNotes;
            }
        }

        private async Task TryAdvanceToNextStageAsync(int currentStageId, int userId)
        {
            var currentStage = await _context.JobStages
                .Include(js => js.Job)
                .FirstOrDefaultAsync(js => js.Id == currentStageId);

            if (currentStage == null) return;

            var nextStage = await _context.JobStages
                .Where(js => js.JobId == currentStage.JobId && 
                            js.ExecutionOrder > currentStage.ExecutionOrder)
                .OrderBy(js => js.ExecutionOrder)
                .FirstOrDefaultAsync();

            if (nextStage != null && nextStage.CanStart)
            {
                nextStage.Status = "Ready";
                await _context.SaveChangesAsync();
            }
        }

        private BuildJob CreateBuildJobFromPostPrint(PostPrintViewModel model, int userId)
        {
            return new BuildJob
            {
                PrinterName = model.PrinterName,
                ActualStartTime = model.ActualStartTime,
                UserId = userId,
                PartId = model.PartId,
                ScheduledStartTime = model.ScheduledStartTime,
                Status = "In Progress",
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
                    IsPrimary = i == 0 || partEntry.IsPrimary,
                    Description = partEntry.Description,
                    Material = partEntry.Material,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                    EstimatedHours = 0 // Will be populated from Parts library
                };

                // Get additional info from Parts library
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
                scheduledJob.Status = MapBuildJobStatusToJobStatus(buildJob.Status);
                scheduledJob.LastModifiedDate = DateTime.UtcNow;
                scheduledJob.LastModifiedBy = "PrintTrackingSystem";
                
                // Update notes if available
                if (!string.IsNullOrEmpty(buildJob.Notes))
                {
                    scheduledJob.Notes = buildJob.Notes;
                }
                
                await _context.SaveChangesAsync();
            }
        }

        private string DetermineDelayReason(PrintStartViewModel model)
        {
            // This would analyze the delay and determine the most likely reason
            // For now, return a generic reason
            return "Late Start";
        }

        private string DetermineFinalStatus(string? reasonForEnd)
        {
            return reasonForEnd?.ToLower() switch
            {
                "completed successfully" => "Completed",
                "completed with issues" => "Completed with Issues",
                "aborted - operator" => "Aborted",
                "aborted - machine error" => "Machine Error",
                "aborted - material issue" => "Material Issue",
                "aborted - power failure" => "Power Failure",
                "aborted - emergency stop" => "Emergency Stop",
                "quality hold" => "Quality Hold",
                "rework required" => "Rework Required",
                _ => "Completed"
            };
        }

        private string MapBuildJobStatusToJobStatus(string buildJobStatus)
        {
            return buildJobStatus switch
            {
                "Completed" => "Completed",
                "Completed with Issues" => "Completed",
                "Aborted" => "Cancelled",
                "Machine Error" => "On Hold",
                "Material Issue" => "On Hold",
                "Quality Hold" => "On Hold",
                "Rework Required" => "Rework",
                _ => "Completed"
            };
        }

        private string GetMachineName(string? machineId)
        {
            if (string.IsNullOrEmpty(machineId)) return "Unknown";
            
            return machineId switch
            {
                "TI1" => "TruPrint 3000 #1",
                "TI2" => "TruPrint 3000 #2", 
                "INC" => "Inconel Printer",
                _ => machineId
            };
        }

        private bool CanAdvanceToNextStage(List<WorkflowStageInfo> stages, int currentStageIndex)
        {
            if (currentStageIndex < 0 || currentStageIndex >= stages.Count - 1)
                return false;

            var currentStage = stages[currentStageIndex];
            if (!currentStage.IsCompleted)
                return false;

            var nextStage = stages[currentStageIndex + 1];
            return nextStage.CanStart;
        }

        private string GetNextStageBlockedReason(List<WorkflowStageInfo> stages, int currentStageIndex)
        {
            if (currentStageIndex < 0 || currentStageIndex >= stages.Count - 1)
                return "No more stages";

            var currentStage = stages[currentStageIndex];
            if (!currentStage.IsCompleted)
                return "Current stage not completed";

            var nextStage = stages[currentStageIndex + 1];
            if (!nextStage.CanStart)
                return "Dependencies not satisfied";

            return string.Empty;
        }

        #endregion
    }
}