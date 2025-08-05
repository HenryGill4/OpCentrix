using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.JobStaging;
using OpCentrix.ViewModels.PrintTracking;

namespace OpCentrix.Services
{
    #region Phase 2: Enhanced Build Time Tracking Models

    public class BuildTimeEstimate
    {
        public decimal EstimatedHours { get; set; }
        public double ConfidenceLevel { get; set; }
        public int BasedOnBuilds { get; set; }
        public DateTime? LastBuildDate { get; set; }
        public bool MachineSpecific { get; set; }
    }

    public class BuildPerformanceData
    {
        public int BuildId { get; set; }
        public DateTime BuildDate { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public int PartCount { get; set; }
        public string? BuildFileHash { get; set; }
        public string Assessment { get; set; } = string.Empty;
        public string? SupportComplexity { get; set; }
        public int DefectCount { get; set; }
    }

    public class BuildCompletionData
    {
        public string? BuildFileHash { get; set; }
        public int? LayerCount { get; set; }
        public decimal? BuildHeight { get; set; }
        public string? SupportComplexity { get; set; }
        public string? PartOrientations { get; set; }
        public string? PostProcessingNeeded { get; set; }
        public int? DefectCount { get; set; }
        public string? LessonsLearned { get; set; }
        public string? TimeFactors { get; set; }
        public decimal? PowerConsumption { get; set; }
        public decimal? LaserOnTime { get; set; }
    }

    #endregion

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

        // Option A: Cohort Management Integration
        Task<bool> TryCreateCohortFromCompletedBuildAsync(BuildJob buildJob);

        // Phase 2: Enhanced Build Time Methods
        Task<BuildTimeEstimate> GetBuildTimeEstimateAsync(string buildFileHash, string machineType);
        Task LogOperatorEstimateAsync(int buildId, decimal estimatedHours, string notes);
        Task RecordActualBuildTimeAsync(int buildId, decimal actualHours, string assessment);
        Task AnalyzeBuildPerformanceAsync(int buildId);

        // Machine Learning Data Collection
        Task<List<BuildPerformanceData>> GetHistoricalBuildDataAsync(string partNumber);
        Task UpdateBuildTimeLearningAsync(int buildId, BuildCompletionData data);

        // NEW: Schedule Integration
        Task UpdatePartDurationFromScheduleAsync(int jobId, double newDurationHours);
        Task<int> CreateBuildJobFromScheduledJobAsync(int jobId, string operatorName);
    }

    public class PrintTrackingService : IPrintTrackingService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PrintTrackingService> _logger;
        private readonly ICohortManagementService? _cohortManagementService;
        private readonly IStageProgressionService _stageProgressionService;

        public PrintTrackingService(SchedulerContext context, ILogger<PrintTrackingService> logger, ICohortManagementService? cohortManagementService = null, IStageProgressionService? stageProgressionService = null)
        {
            _context = context;
            _logger = logger;
            _cohortManagementService = cohortManagementService;
            _stageProgressionService = stageProgressionService ?? throw new ArgumentNullException(nameof(stageProgressionService));
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
            try
            {
                // Create new BuildJob with enhanced Phase 4 fields
                var buildJob = new BuildJob
                {
                    BuildId = await GenerateBuildIdAsync(),
                    PrinterName = model.PrinterName,
                    ActualStartTime = model.ActualStartTime,
                    Status = "In Progress",
                    PartId = model.PartId,
                    UserId = userId,
                    
                    // PHASE 4: Enhanced Build Time Tracking
                    OperatorEstimatedHours = model.OperatorEstimatedHours,
                    TotalPartsInBuild = model.TotalPartsInBuild,
                    BuildFileHash = GenerateBuildFileHash(model.BuildFileName),
                    IsLearningBuild = true, // Mark for machine learning
                    
                    // PHASE 4: Build Complexity Data
                    SupportComplexity = model.SupportComplexity,
                    PartOrientations = model.PartOrientations,
                    BuildHeight = model.BuildHeight,
                    LayerCount = model.LayerCount,
                    TimeFactors = string.Join(",", model.TimeFactors ?? new List<string>()),
                    
                    // PHASE 4: Machine Performance Context
                    MachinePerformanceNotes = model.MachinePerformanceNotes,
                    
                    // Existing fields
                    SetupNotes = model.SetupNotes,
                    ScheduledStartTime = model.ScheduledStartTime,
                    ScheduledEndTime = model.ScheduledEndTime
                };

                _context.BuildJobs.Add(buildJob);

                // ENHANCED: Update associated scheduled job if linked
                if (model.AssociatedScheduledJobId.HasValue)
                {
                    var scheduledJob = await _context.Jobs.FindAsync(model.AssociatedScheduledJobId.Value);
                    if (scheduledJob != null)
                    {
                        // FIXED: Update BOTH ActualStart and Status
                        scheduledJob.Status = "In Progress";
                        scheduledJob.ActualStart = model.ActualStartTime; // This was already correct
                        
                        // ENHANCED: Update the job duration based on operator estimate
                        var operatorEstimateHours = (double)model.OperatorEstimatedHours;
                        var newEndTime = model.ActualStartTime.AddHours(operatorEstimateHours);
                        
                        // Calculate the time difference between old and new estimates
                        var timeDifference = newEndTime - scheduledJob.ScheduledEnd;
                        
                        // Update the scheduled job with operator's estimate
                        scheduledJob.EstimatedHours = operatorEstimateHours;
                        scheduledJob.ScheduledEnd = newEndTime;
                        
                        // NEW: Update the Part's EstimatedHours for this specific quantity
                        // Calculate per-part time and update the Part record for future scheduling
                        if (model.Quantity.HasValue && model.Quantity > 0)
                        {
                            var timePerPart = operatorEstimateHours / model.Quantity.Value;
                            var part = await _context.Parts.FindAsync(scheduledJob.PartId);
                            if (part != null)
                            {
                                // Update the part's estimated hours with the actual operator experience
                                part.EstimatedHours = timePerPart; // timePerPart is already double
                                part.LastProduced = DateTime.UtcNow;
                                
                                _logger.LogInformation("Updated Part {PartNumber} EstimatedHours from {OldHours}h to {NewHours}h per part based on operator estimate",
                                    part.PartNumber, part.EstimatedHours, timePerPart);
                            }
                        }
                        
                        // Store original schedule for comparison
                        buildJob.ScheduledStartTime = scheduledJob.ScheduledStart;
                        buildJob.ScheduledEndTime = scheduledJob.ScheduledEnd;
                        
                        _logger.LogInformation("Updated scheduled job {JobId}: ActualStart={ActualStart}, operator estimate {OperatorHours}h, new end time: {NewEndTime}",
                            scheduledJob.Id, scheduledJob.ActualStart, operatorEstimateHours, newEndTime);
                        
                        // ENHANCED: Cascade schedule changes to subsequent jobs on the same machine
                        if (Math.Abs(timeDifference.TotalMinutes) > 15) // Only cascade if difference > 15 minutes
                        {
                            await CascadeScheduleChangesAsync(scheduledJob.MachineId, scheduledJob.ScheduledEnd, timeDifference);
                        }
                    }
                }

                // PHASE 4: Check if this impacts scheduled jobs (if starting late)
                if (model.IsDelayed && model.DelayMinutes > 15)
                {
                    await HandleScheduleDelayAsync(model.PrinterName, model.DelayMinutes, buildJob.BuildId);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Print job started: BuildId {BuildId}, Printer {PrinterName}, Operator Estimate {EstimateHours}h", 
                    buildJob.BuildId, model.PrinterName, model.OperatorEstimatedHours);

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
            try
            {
                var buildJob = await _context.BuildJobs.FindAsync(model.BuildId);
                if (buildJob == null)
                {
                    _logger.LogWarning("BuildJob not found for ID {BuildId}", model.BuildId);
                    return false;
                }

                // Update build job with completion data
                buildJob.ActualEndTime = model.ActualEndTime;
                buildJob.ReasonForEnd = model.ReasonForEnd;
                buildJob.Status = GetBuildJobStatus(model.ReasonForEnd);
                buildJob.CompletedAt = DateTime.UtcNow;
                buildJob.LaserRunTime = model.LaserRunTime;
                buildJob.GasUsed_L = model.GasUsed_L;
                buildJob.PowderUsed_L = model.PowderUsed_L;
                buildJob.Notes = model.Notes;

                // PHASE 4: Enhanced Performance Data
                buildJob.OperatorActualHours = model.OperatorActualHours;
                buildJob.OperatorBuildAssessment = model.OperatorBuildAssessment;
                buildJob.MachinePerformanceNotes = model.MachinePerformanceNotes;
                buildJob.DefectCount = model.DefectCount ?? 0;
                buildJob.LessonsLearned = model.LessonsLearned;
                
                // Update time factors with completion data
                if (model.TimeFactors?.Any() == true)
                {
                    var existingFactors = !string.IsNullOrEmpty(buildJob.TimeFactors) 
                        ? buildJob.TimeFactors.Split(',').ToList() 
                        : new List<string>();
                    existingFactors.AddRange(model.TimeFactors);
                    buildJob.TimeFactors = string.Join(",", existingFactors.Distinct());
                }

                // PHASE 4: Handle part quality tracking
                var allPartsGood = true;
                var totalDefects = 0;
                
                foreach (var part in model.Parts)
                {
                    totalDefects += part.DefectiveParts;
                    if (part.DefectiveParts > 0) allPartsGood = false;
                }

                buildJob.DefectCount = totalDefects;

                // PHASE 4: Create cohort and trigger downstream jobs if successful completion
                if (IsSuccessfulCompletion(model.ReasonForEnd) && allPartsGood && _cohortManagementService != null)
                {
                    var cohort = await _cohortManagementService.CreateCohortAsync(buildJob.BuildId, 
                        $"BUILD-{DateTime.Now:yyyy-MM-dd}-{buildJob.BuildId}", 
                        model.Parts.Sum(p => p.Quantity), 
                        "Ti-6Al-4V");
                    
                    if (cohort != null)
                    {
                        // Trigger Stage Progression (Phase 3 integration)
                        var downstreamJobs = await _stageProgressionService.CreateDownstreamJobsAsync(cohort.Id);
                        if (downstreamJobs.Any())
                        {
                            _logger.LogInformation("Created {JobCount} downstream jobs for completed build {BuildId}", 
                                downstreamJobs.Count, buildJob.BuildId);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Print job completed: BuildId {BuildId}, Status {Status}, Performance {Assessment}", 
                    buildJob.BuildId, buildJob.Status, model.OperatorBuildAssessment);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing print job {BuildId}", model.BuildId);
                throw;
            }
        }

        // Add all missing interface method implementations

        public async Task<List<Job>> GetAvailableScheduledJobsAsync(string printerName)
        {
            return await _context.Jobs
                .Where(j => j.MachineId == printerName && j.Status == "Scheduled")
                .OrderBy(j => j.ScheduledStart)
                .ToListAsync();
        }

        public async Task<List<JobStage>> GetAvailableJobStagesAsync(string printerName)
        {
            return await _context.JobStages
                .Where(js => js.MachineId == printerName && js.Status == "Scheduled")
                .OrderBy(js => js.ScheduledStart)
                .ToListAsync();
        }

        public async Task<List<PrototypeJob>> GetAvailablePrototypeJobsAsync()
        {
            return await _context.PrototypeJobs
                .Where(pj => pj.Status == "Scheduled" && pj.IsActive)
                .OrderBy(pj => pj.Priority)
                .ToListAsync();
        }

        public async Task<BuildJob?> GetActiveBuildJobAsync(string printerName)
        {
            return await _context.BuildJobs
                .Where(b => b.PrinterName == printerName && b.Status == "In Progress")
                .FirstOrDefaultAsync();
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

        public async Task CreateCooldownAndChangeoverBlocksAsync(BuildJob completedJob)
        {
            // Implementation for cooldown periods after builds
            await Task.CompletedTask;
        }

        public async Task<MultiStageWorkflowViewModel> GetWorkflowStatusAsync(int jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.Part)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
            {
                return new MultiStageWorkflowViewModel();
            }

            return new MultiStageWorkflowViewModel
            {
                JobId = jobId,
                PartNumber = job.PartNumber,
                PartDescription = job.Part?.Description ?? "",
                OverallStatus = job.Status,
                StartDate = job.ActualStart,
                EstimatedCompletionDate = job.ScheduledEnd,
                ActualCompletionDate = job.ActualEnd
            };
        }

        public async Task<bool> AdvanceJobStageAsync(int jobStageId, int userId)
        {
            var jobStage = await _context.JobStages.FindAsync(jobStageId);
            if (jobStage == null) return false;

            jobStage.Status = "Completed";
            jobStage.ActualEnd = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStageProgressAsync(int jobStageId, double progressPercent, string? statusUpdate = null)
        {
            var jobStage = await _context.JobStages.FindAsync(jobStageId);
            if (jobStage == null) return false;

            jobStage.ProgressPercent = progressPercent;
            if (!string.IsNullOrEmpty(statusUpdate))
            {
                jobStage.Status = statusUpdate;
            }
            await _context.SaveChangesAsync();
            return true;
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
            // Basic validation - could be enhanced with material compatibility, size constraints, etc.
            var part = await _context.Parts.FirstOrDefaultAsync(p => p.PartNumber == partNumber);
            return part != null && part.IsActive;
        }

        public async Task<bool> TryCreateCohortFromCompletedBuildAsync(BuildJob buildJob)
        {
            try
            {
                if (_cohortManagementService == null) return false;

                var slsPrinters = new[] { "TI1", "TI2", "INC" };
                if (!slsPrinters.Contains(buildJob.PrinterName, StringComparer.OrdinalIgnoreCase))
                    return false;

                if (buildJob.Status != "Completed")
                    return false;

                var partCount = buildJob.TotalPartsInBuild > 0 ? buildJob.TotalPartsInBuild : 1;
                if (partCount < 2) return false;

                var buildNumber = $"BUILD-{DateTime.UtcNow:yyyy-MM-dd}-{buildJob.BuildId}";
                var cohort = await _cohortManagementService.CreateCohortAsync(buildJob.BuildId, buildNumber, partCount, "Ti-6Al-4V");

                _logger.LogInformation("Created cohort {BuildNumber} with {PartCount} parts from completed SLS build {BuildId}", 
                    buildNumber, partCount, buildJob.BuildId);

                return cohort != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cohort from completed build {BuildId}", buildJob.BuildId);
                return false;
            }
        }

        // Phase 2: Enhanced Build Time Methods

        public async Task<BuildTimeEstimate> GetBuildTimeEstimateAsync(string buildFileHash, string machineType)
        {
            try
            {
                var historicalBuilds = await _context.BuildJobs
                    .Where(b => b.BuildFileHash == buildFileHash && 
                               b.Status == "Completed" && 
                               b.OperatorActualHours.HasValue)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                if (historicalBuilds.Any())
                {
                    var averageHours = historicalBuilds.Average(b => b.OperatorActualHours!.Value);
                    var confidence = Math.Min(100, historicalBuilds.Count * 10);

                    return new BuildTimeEstimate
                    {
                        EstimatedHours = averageHours,
                        ConfidenceLevel = confidence,
                        BasedOnBuilds = historicalBuilds.Count,
                        LastBuildDate = historicalBuilds.First().CreatedAt,
                        MachineSpecific = true
                    };
                }

                return new BuildTimeEstimate
                {
                    EstimatedHours = 8.0m,
                    ConfidenceLevel = 10,
                    BasedOnBuilds = 0,
                    LastBuildDate = null,
                    MachineSpecific = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting build time estimate for hash {BuildFileHash} on machine {MachineType}", 
                    buildFileHash, machineType);
                
                return new BuildTimeEstimate
                {
                    EstimatedHours = 8.0m,
                    ConfidenceLevel = 0,
                    BasedOnBuilds = 0,
                    LastBuildDate = null,
                    MachineSpecific = false
                };
            }
        }

        public async Task LogOperatorEstimateAsync(int buildId, decimal estimatedHours, string notes)
        {
            try
            {
                var buildJob = await _context.BuildJobs.FindAsync(buildId);
                if (buildJob != null)
                {
                    buildJob.OperatorEstimatedHours = estimatedHours;
                    buildJob.IsLearningBuild = true;
                    
                    if (!string.IsNullOrEmpty(notes))
                    {
                        buildJob.Notes = string.IsNullOrEmpty(buildJob.Notes) 
                            ? $"Estimate: {notes}" 
                            : $"{buildJob.Notes}\nEstimate: {notes}";
                    }

                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Logged operator estimate of {EstimatedHours}h for build {BuildId}", 
                        estimatedHours, buildId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging operator estimate for build {BuildId}", buildId);
            }
        }

        public async Task RecordActualBuildTimeAsync(int buildId, decimal actualHours, string assessment)
        {
            try
            {
                var buildJob = await _context.BuildJobs.FindAsync(buildId);
                if (buildJob != null)
                {
                    buildJob.OperatorActualHours = actualHours;
                    buildJob.OperatorBuildAssessment = assessment;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Recorded actual time of {ActualHours}h (assessment: {Assessment}) for build {BuildId}", 
                        actualHours, assessment, buildId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording actual build time for build {BuildId}", buildId);
            }
        }

        public async Task AnalyzeBuildPerformanceAsync(int buildId)
        {
            try
            {
                var buildJob = await _context.BuildJobs.FindAsync(buildId);
                if (buildJob?.OperatorEstimatedHours.HasValue == true && 
                    buildJob.OperatorActualHours.HasValue)
                {
                    var estimated = buildJob.OperatorEstimatedHours.Value;
                    var actual = buildJob.OperatorActualHours.Value;
                    
                    var variance = Math.Abs(actual - estimated);
                    var percentageError = estimated > 0 ? (variance / estimated) * 100 : 0;
                    
                    var performance = percentageError switch
                    {
                        <= 10 => "Excellent",
                        <= 20 => "Good",
                        <= 30 => "Fair",
                        _ => "Needs Improvement"
                    };
                    
                    if (string.IsNullOrEmpty(buildJob.OperatorBuildAssessment))
                    {
                        buildJob.OperatorBuildAssessment = actual < estimated ? "faster" : 
                                                          actual > estimated ? "slower" : "expected";
                    }

                    var performanceNotes = $"Estimate accuracy: {performance} ({percentageError:F1}% error)";
                    buildJob.MachinePerformanceNotes = string.IsNullOrEmpty(buildJob.MachinePerformanceNotes)
                        ? performanceNotes
                        : $"{buildJob.MachinePerformanceNotes}\n{performanceNotes}";

                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Analyzed performance for build {BuildId}: {Performance} ({PercentageError:F1}% error)", 
                        buildId, performance, percentageError);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing build performance for build {BuildId}", buildId);
            }
        }

        public async Task<List<BuildPerformanceData>> GetHistoricalBuildDataAsync(string partNumber)
        {
            try
            {
                var builds = await _context.BuildJobs
                    .Where(b => b.Status == "Completed" &&
                               b.OperatorEstimatedHours.HasValue &&
                               b.OperatorActualHours.HasValue)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(50)
                    .ToListAsync();

                return builds.Select(b => new BuildPerformanceData
                {
                    BuildId = b.BuildId,
                    BuildDate = b.CreatedAt,
                    MachineId = b.PrinterName,
                    EstimatedHours = b.OperatorEstimatedHours!.Value,
                    ActualHours = b.OperatorActualHours!.Value,
                    PartCount = b.TotalPartsInBuild,
                    BuildFileHash = b.BuildFileHash,
                    Assessment = b.OperatorBuildAssessment ?? "unknown",
                    SupportComplexity = b.SupportComplexity,
                    DefectCount = b.DefectCount ?? 0
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting historical build data for part {PartNumber}", partNumber);
                return new List<BuildPerformanceData>();
            }
        }

        public async Task UpdateBuildTimeLearningAsync(int buildId, BuildCompletionData data)
        {
            try
            {
                var buildJob = await _context.BuildJobs.FindAsync(buildId);
                if (buildJob != null)
                {
                    buildJob.BuildFileHash = data.BuildFileHash;
                    buildJob.LayerCount = data.LayerCount;
                    buildJob.BuildHeight = data.BuildHeight;
                    buildJob.SupportComplexity = data.SupportComplexity;
                    buildJob.PartOrientations = data.PartOrientations;
                    buildJob.PostProcessingNeeded = data.PostProcessingNeeded;
                    buildJob.DefectCount = data.DefectCount;
                    buildJob.LessonsLearned = data.LessonsLearned;
                    buildJob.TimeFactors = data.TimeFactors;
                    buildJob.PowerConsumption = data.PowerConsumption;
                    buildJob.LaserOnTime = data.LaserOnTime;

                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Updated learning data for build {BuildId} with hash {BuildFileHash}", 
                        buildId, data.BuildFileHash);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating build time learning for build {BuildId}", buildId);
            }
        }

        /// <summary>
        /// Updates part duration when schedule duration is manually adjusted
        /// </summary>
        public async Task UpdatePartDurationFromScheduleAsync(int jobId, double newDurationHours)
        {
            try
            {
                var job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId);
                if (job?.Part != null && job.Quantity > 0)
                {
                    var timePerPart = newDurationHours / job.Quantity;
                    var oldTimePerPart = job.Part.EstimatedHours;
                    
                    // Update the part's estimated hours
                    job.Part.EstimatedHours = timePerPart;
                    job.Part.LastModifiedDate = DateTime.UtcNow;
                    
                    // Update the job's estimated hours to match
                    job.EstimatedHours = newDurationHours;
                    
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Updated Part {PartNumber} duration from {OldHours}h to {NewHours}h per part based on schedule adjustment for Job {JobId}",
                        job.Part.PartNumber, oldTimePerPart, timePerPart, jobId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part duration from schedule for job {JobId}", jobId);
            }
        }

        /// <summary>
        /// Creates a build job from an existing scheduled job for print tracking integration
        /// </summary>
        public async Task<int> CreateBuildJobFromScheduledJobAsync(int jobId, string operatorName)
        {
            try
            {
                var job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId);
                if (job == null)
                {
                    throw new InvalidOperationException($"Job {jobId} not found");
                }

                // Find the user by name or use a default
                var user = await _context.Users.FirstOrDefaultAsync(u => u.FullName == operatorName) ??
                          await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin") ??
                          await _context.Users.FirstAsync(); // Fallback to first user

                var buildJob = new BuildJob
                {
                    BuildId = await GenerateBuildIdAsync(),
                    PrinterName = job.MachineId,
                    ActualStartTime = job.ActualStart ?? DateTime.UtcNow,
                    Status = "In Progress",
                    PartId = job.PartId,
                    UserId = user.Id,
                    
                    // Link to scheduled job
                    AssociatedScheduledJobId = jobId,
                    
                    // Copy job data
                    OperatorEstimatedHours = (decimal)job.EstimatedHours,
                    TotalPartsInBuild = job.Quantity,
                    ScheduledStartTime = job.ScheduledStart,
                    ScheduledEndTime = job.ScheduledEnd,
                    
                    // Build metadata
                    BuildFileHash = GenerateBuildFileHash(job.PartNumber),
                    IsLearningBuild = true,
                    
                    // Material and process data from job
                    SetupNotes = $"Started from scheduler job {jobId} - {job.PartNumber} (Qty: {job.Quantity})"
                };

                _context.BuildJobs.Add(buildJob);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created build job {BuildId} from scheduled job {JobId} by {OperatorName}",
                    buildJob.BuildId, jobId, operatorName);

                return buildJob.BuildId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating build job from scheduled job {JobId}", jobId);
                throw;
            }
        }

        // Dashboard helper methods
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
                ProgressPercent = 50,
                IsDelayed = true,
                DelayMinutes = (int)(now - j.ScheduledEnd).TotalMinutes,
                RequiresAttention = true
            }).ToList();
        }

        private async Task<List<AlertInfo>> GetActiveAlertsAsync()
        {
            var alerts = new List<AlertInfo>();
            
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
            var machines = await _context.Machines
                .Where(m => m.IsActive)
                .Select(m => m.MachineId)
                .ToListAsync();

            return machines.ToDictionary(m => m, m => 75.0);
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

            var partsProduced = todayBuilds.Sum(b => b.TotalPartsInBuild);

            var efficiency = todayBuilds.Any() 
                ? todayBuilds.Average(b => b.ActualEndTime.HasValue 
                    ? Math.Min(100, (b.ScheduledEndTime?.Subtract(b.ScheduledStartTime ?? b.ActualStartTime).TotalHours ?? 8) / 
                               b.ActualEndTime.Value.Subtract(b.ActualStartTime).TotalHours * 100) 
                    : 0) 
                : 0;

            var qualityScore = 100.0;
            var totalCost = 0m;

            return (efficiency, qualityScore, totalCost, partsProduced);
        }

        private async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new List<string>();

            return user.Role switch
            {
                "Admin" => new List<string> { "ViewAll", "EditAll", "DeleteAll", "ManageUsers", "ManageSystem" },
                "Supervisor" => new List<string> { "ViewAll", "EditOwn", "ManageTeam" },
                "Operator" => new List<string> { "ViewOwn", "EditOwn" },
                _ => new List<string> { "ViewOwn" }
            };
        }

        private string GetMachineName(string? machineId)
        {
            return machineId switch
            {
                "TI1" => "TruPrint 3000 #1",
                "TI2" => "TruPrint 3000 #2",
                "INC" => "Inconel Printer",
                _ => machineId ?? "Unknown"
            };
        }

        // Private Helper Methods for build operations

        private async Task<int> GenerateBuildIdAsync()
        {
            var maxId = await _context.BuildJobs.MaxAsync(b => (int?)b.BuildId) ?? 0;
            return maxId + 1;
        }

        private string GenerateBuildFileHash(string? buildFileName)
        {
            if (string.IsNullOrEmpty(buildFileName)) return string.Empty;
            
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(buildFileName));
            return Convert.ToBase64String(hashBytes)[..16];
        }

        private bool IsSuccessfulCompletion(string reasonForEnd)
        {
            return reasonForEnd == "Completed Successfully" || reasonForEnd == "Completed with Issues";
        }

        private string GetBuildJobStatus(string reasonForEnd)
        {
            return reasonForEnd switch
            {
                "Completed Successfully" => "Completed",
                "Completed with Issues" => "Completed",
                var reason when reason.Contains("Aborted") => "Aborted",
                "Quality Hold" => "On Hold",
                "Rework Required" => "Rework",
                _ => "Completed"
            };
        }

        private async Task HandleScheduleDelayAsync(string printerName, int delayMinutes, int buildId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var affectedJobs = await _context.Jobs
                    .Where(j => j.MachineId == printerName && 
                               j.ScheduledStart > now.AddMinutes(-30) && 
                               j.Status == "Scheduled")
                    .OrderBy(j => j.ScheduledStart)
                    .ToListAsync();

                foreach (var job in affectedJobs)
                {
                    job.ScheduledStart = job.ScheduledStart.AddMinutes(delayMinutes);
                    job.ScheduledEnd = job.ScheduledEnd.AddMinutes(delayMinutes);
                    
                    _logger.LogInformation("Pushed back job {JobId} by {DelayMinutes} minutes due to build {BuildId}", 
                        job.Id, delayMinutes, buildId);
                }

                if (affectedJobs.Any())
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to handle schedule delay for printer {PrinterName}", printerName);
            }
        }

        /// <summary>
        /// ENHANCED: Cascade schedule changes to subsequent jobs on the same machine
        /// </summary>
        private async Task CascadeScheduleChangesAsync(string machineId, DateTime fromTime, TimeSpan timeDifference)
        {
            try
            {
                // Get all subsequent jobs on the same machine that are scheduled after the current job
                var subsequentJobs = await _context.Jobs
                    .Where(j => j.MachineId == machineId && 
                               j.ScheduledStart >= fromTime && 
                               j.Status == "Scheduled")
                    .OrderBy(j => j.ScheduledStart)
                    .ToListAsync();

                if (!subsequentJobs.Any())
                {
                    _logger.LogDebug("No subsequent jobs to cascade schedule changes for machine {MachineId}", machineId);
                    return;
                }

                var cascadeCount = 0;
                foreach (var job in subsequentJobs)
                {
                    var originalStart = job.ScheduledStart;
                    var originalEnd = job.ScheduledEnd;
                    
                    // Apply the time difference to both start and end times
                    job.ScheduledStart = job.ScheduledStart.Add(timeDifference);
                    job.ScheduledEnd = job.ScheduledEnd.Add(timeDifference);
                    
                    cascadeCount++;
                    
                    _logger.LogInformation("Cascaded schedule change for job {JobId} on {MachineId}: {OriginalStart} -> {NewStart} (shift: {TimeDifference})",
                        job.Id, machineId, originalStart.ToString("yyyy-MM-dd HH:mm"), 
                        job.ScheduledStart.ToString("yyyy-MM-dd HH:mm"), timeDifference);
                }

                if (cascadeCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully cascaded schedule changes to {CascadeCount} jobs on machine {MachineId}", 
                        cascadeCount, machineId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cascading schedule changes for machine {MachineId}", machineId);
                throw;
            }
        }
    }
}