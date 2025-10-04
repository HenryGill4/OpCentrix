using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.JobStaging;
using OpCentrix.ViewModels.PrintTracking;
using OpCentrix.Services.Admin; // added for material service

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
        Task<bool> ValidatePartCompatibilityAsync(String partNumber, String machineId);

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

    public partial class PrintTrackingService : IPrintTrackingService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PrintTrackingService> _logger;
        private readonly ICohortManagementService? _cohortManagementService;
        private readonly IStageProgressionService _stageProgressionService;
        private readonly IMaterialService? _materialService; // NEW
        private readonly IOperatingShiftService? _shiftService; // NEW

        // Added: cached detection for ScheduleAdjustments table (SQLite dev env may not have migration applied)
        private static bool _adjTableChecked = false;
        private static bool _adjTableExists = false;
        private async Task EnsureScheduleAdjustmentsTableAsync()
        {
            if (_adjTableChecked && _adjTableExists) return;
            try
            {
                var conn = _context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open)
                    await conn.OpenAsync();
                using (var check = conn.CreateCommand())
                {
                    check.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='ScheduleAdjustments' LIMIT 1";
                    var result = await check.ExecuteScalarAsync();
                    _adjTableExists = result != null && result != DBNull.Value;
                }
                if (!_adjTableExists)
                {
                    _logger.LogInformation("Creating ScheduleAdjustments table (migration fallback)");
                    using var create = conn.CreateCommand();
                    // FIX: remove literal \n tokens (were causing 'unrecognized token: "\\"') and use proper newlines
                    create.CommandText = @"CREATE TABLE IF NOT EXISTS ScheduleAdjustments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TriggerJobId INTEGER NOT NULL,
    AffectedJobId INTEGER NOT NULL,
    OriginalStart TEXT NOT NULL,
    OriginalEnd TEXT NOT NULL,
    NewStart TEXT NOT NULL,
    NewEnd TEXT NOT NULL,
    ShiftMinutes REAL NOT NULL,
    Reason TEXT NOT NULL,
    Notes TEXT NULL,
    CreatedAt TEXT NOT NULL,
    CreatedBy TEXT NOT NULL
);";
                    await create.ExecuteNonQueryAsync();
                    _adjTableExists = true;
                    _logger.LogInformation("ScheduleAdjustments table created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to verify/create ScheduleAdjustments table; adjustments will be skipped");
                _adjTableExists = false;
            }
            finally { _adjTableChecked = true; }
        }

        public PrintTrackingService(
            SchedulerContext context,
            ILogger<PrintTrackingService> logger,
            ICohortManagementService? cohortManagementService = null,
            IStageProgressionService? stageProgressionService = null,
            IMaterialService? materialService = null,
            IOperatingShiftService? shiftService = null)
        {
            _context = context;
            _logger = logger;
            _cohortManagementService = cohortManagementService;
            _stageProgressionService = stageProgressionService ?? throw new ArgumentNullException(nameof(stageProgressionService));
            _materialService = materialService;
            _shiftService = shiftService;
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
                // Helper local function to normalize and resolve machine id
                async Task<Machine?> ResolveMachineAsync(string input)
                {
                    if (string.IsNullOrWhiteSpace(input)) return null;
                    var inputLower = input.Trim().ToLower();

                    // Use case-normalized comparison (EF translatable) instead of StringComparison overload
                    var mach = await _context.Machines.FirstOrDefaultAsync(m =>
                        m.MachineId.ToLower() == inputLower ||
                        (m.Name != null && m.Name.ToLower() == inputLower) ||
                        (m.MachineName != null && m.MachineName.ToLower() == inputLower));
                    if (mach != null) return mach;

                    // Fuzzy contains match on MachineId token (client-side)
                    var all = await _context.Machines.AsNoTracking().ToListAsync();
                    var normInput = new string(input.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
                    return all.FirstOrDefault(m =>
                        normInput.Contains(new string(m.MachineId.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant()));
                }

                var machine = await ResolveMachineAsync(model.PrinterName);

                // Canonical machine id used for scheduler Job linking
                var canonicalMachineId = machine?.MachineId ?? model.PrinterName;
                var machineMaterialCode = machine?.CurrentMaterial;

                // Ensure operator estimate derives from EstimatedEndTime if supplied
                if (model.EstimatedEndTime > model.ActualStartTime)
                {
                    model.OperatorEstimatedHours = (decimal)(model.EstimatedEndTime - model.ActualStartTime).TotalHours;
                }

                DateTime? scheduledStart = model.ScheduledStartTime;
                DateTime? scheduledEnd = model.ScheduledEndTime;
                Job? scheduledJob = null;

                if (!model.AssociatedScheduledJobId.HasValue)
                {
                    var actualStart = model.ActualStartTime;

                    // 1. PRIMARY: overlapping job (already running window)
                    scheduledJob = await _context.Jobs
                        .Where(j => j.MachineId == canonicalMachineId && j.Status == "Scheduled")
                        .OrderBy(j => j.ScheduledStart)
                        .FirstOrDefaultAsync(j => j.ScheduledStart <= actualStart && j.ScheduledEnd >= actualStart);

                    // 2. NEAR-FUTURE: within 4h (existing logic)
                    if (scheduledJob == null)
                    {
                        scheduledJob = await _context.Jobs
                            .Where(j => j.MachineId == canonicalMachineId && j.Status == "Scheduled" && j.ScheduledStart >= actualStart)
                            .OrderBy(j => j.ScheduledStart)
                            .FirstOrDefaultAsync(j => j.ScheduledStart <= actualStart.AddHours(4));
                    }

                    // 3. EARLY START: part-based match within next 36h (actual start earlier than scheduled block)
                    if (scheduledJob == null && model.PartId.HasValue)
                    {
                        var partId = model.PartId.Value;
                        scheduledJob = await _context.Jobs
                            .Where(j => j.MachineId == canonicalMachineId && j.Status == "Scheduled" && j.PartId == partId && j.ScheduledStart >= actualStart && j.ScheduledStart <= actualStart.AddHours(36))
                            .OrderBy(j => j.ScheduledStart)
                            .FirstOrDefaultAsync();
                    }

                    // 4. FALLBACK: earliest scheduled job on machine within next 48h
                    if (scheduledJob == null)
                    {
                        scheduledJob = await _context.Jobs
                            .Where(j => j.MachineId == canonicalMachineId && j.Status == "Scheduled" && j.ScheduledStart >= actualStart && j.ScheduledStart <= actualStart.AddHours(48))
                            .OrderBy(j => j.ScheduledStart)
                            .FirstOrDefaultAsync();
                    }

                    // 5. LAST RESORT: a job scheduled earlier today that hasn't been started yet (late logging scenario)
                    if (scheduledJob == null)
                    {
                        var dayStart = actualStart.Date;
                        scheduledJob = await _context.Jobs
                            .Where(j => j.MachineId == canonicalMachineId && j.Status == "Scheduled" && j.ScheduledStart >= dayStart && j.ScheduledStart <= actualStart)
                            .OrderByDescending(j => j.ScheduledStart)
                            .FirstOrDefaultAsync();
                    }

                    if (scheduledJob != null)
                    {
                        model.AssociatedScheduledJobId = scheduledJob.Id;
                        scheduledStart = scheduledJob.ScheduledStart;
                        scheduledEnd = scheduledJob.ScheduledEnd;
                        _logger.LogInformation("[AUTO-LINK] Build auto-associated to Job {JobId} on machine {Machine} (strategy)", scheduledJob.Id, canonicalMachineId);
                    }
                }
                else
                {
                    scheduledJob = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == model.AssociatedScheduledJobId.Value);
                    if (scheduledJob != null)
                    {
                        canonicalMachineId = scheduledJob.MachineId; // enforce machine alignment
                        scheduledStart = scheduledJob.ScheduledStart;
                        scheduledEnd = scheduledJob.ScheduledEnd;
                    }
                }

                if (!scheduledEnd.HasValue && model.EstimatedEndTime > model.ActualStartTime)
                    scheduledEnd = model.EstimatedEndTime;

                var friendlyName = model.PrinterName;

                var buildJob = new BuildJob
                {
                    BuildId = await GenerateBuildIdAsync(),
                    PrinterName = canonicalMachineId, // store canonical for unified queries
                    ActualStartTime = model.ActualStartTime,
                    Status = "In Progress",
                    PartId = model.PartId,
                    UserId = userId,
                    OperatorEstimatedHours = model.OperatorEstimatedHours,
                    TotalPartsInBuild = model.TotalPartsInBuild,
                    BuildFileHash = GenerateBuildFileHash(model.BuildFileName),
                    IsLearningBuild = true,
                    SupportComplexity = model.SupportComplexity,
                    PartOrientations = model.PartOrientations,
                    BuildHeight = model.BuildHeight,
                    LayerCount = model.LayerCount,
                    TimeFactors = string.Join(',', model.TimeFactors ?? new List<string>()),
                    MachinePerformanceNotes = (canonicalMachineId != friendlyName ? $"FriendlyName={friendlyName}; " : string.Empty) + model.MachinePerformanceNotes,
                    SetupNotes = model.SetupNotes,
                    ScheduledStartTime = scheduledStart,
                    ScheduledEndTime = scheduledEnd
                };

                _context.BuildJobs.Add(buildJob);

                TimeSpan netCascadeShift = TimeSpan.Zero;
                if (scheduledJob != null)
                {
                    await EnsureScheduleAdjustmentsTableAsync();
                    var originalStart = scheduledJob.ScheduledStart;
                    var originalEnd = scheduledJob.ScheduledEnd;
                    scheduledJob.Status = "In Progress"; // unified status
                    if (!scheduledJob.ActualStart.HasValue) scheduledJob.ActualStart = model.ActualStartTime;

                    var pendingAdjustments = new List<ScheduleAdjustment>();

                    var startShift = model.ActualStartTime - originalStart;
                    if (Math.Abs(startShift.TotalMinutes) >= 1)
                    {
                        scheduledJob.ScheduledStart = model.ActualStartTime;
                        scheduledJob.ScheduledEnd = scheduledJob.ScheduledEnd + startShift;
                        if (startShift.TotalMinutes > 0) netCascadeShift += startShift;
                        if (_adjTableExists)
                        {
                            pendingAdjustments.Add(new ScheduleAdjustment
                            {
                                TriggerJobId = scheduledJob.Id,
                                AffectedJobId = scheduledJob.Id,
                                OriginalStart = originalStart,
                                OriginalEnd = originalEnd,
                                NewStart = scheduledJob.ScheduledStart,
                                NewEnd = scheduledJob.ScheduledEnd,
                                ShiftMinutes = startShift.TotalMinutes,
                                Reason = startShift.TotalMinutes > 0 ? "StartDelay" : "EarlyStart",
                                Notes = "Anchor job start aligned to actual print start"
                            });
                        }
                    }

                    if (model.EstimatedEndTime > model.ActualStartTime)
                    {
                        var preAdjustEnd = scheduledJob.ScheduledEnd;
                        var newEnd = model.EstimatedEndTime;
                        var durationDelta = newEnd - preAdjustEnd;
                        if (Math.Abs(durationDelta.TotalMinutes) >= 1)
                        {
                            scheduledJob.ScheduledEnd = newEnd;
                            scheduledJob.EstimatedHours = (newEnd - scheduledJob.ScheduledStart).TotalHours;
                            if (durationDelta.TotalMinutes > 0) netCascadeShift += durationDelta;
                            if (_adjTableExists)
                            {
                                pendingAdjustments.Add(new ScheduleAdjustment
                                {
                                    TriggerJobId = scheduledJob.Id,
                                    AffectedJobId = scheduledJob.Id,
                                    OriginalStart = scheduledJob.ScheduledStart,
                                    OriginalEnd = preAdjustEnd,
                                    NewStart = scheduledJob.ScheduledStart,
                                    NewEnd = scheduledJob.ScheduledEnd,
                                    ShiftMinutes = durationDelta.TotalMinutes,
                                    Reason = durationDelta.TotalMinutes > 0 ? "DurationIncrease" : "DurationDecrease",
                                    Notes = "Anchor job duration adjusted from operator estimate"
                                });
                            }
                        }
                    }

                    if (scheduledJob.PartId != null && scheduledJob.Quantity > 0)
                    {
                        var part = await _context.Parts.FindAsync(scheduledJob.PartId);
                        if (part != null)
                        {
                            var timePerPart = scheduledJob.EstimatedHours / scheduledJob.Quantity;
                            part.EstimatedHours = timePerPart;
                            part.LastProduced = DateTime.UtcNow;
                        }
                    }

                    buildJob.AssociatedScheduledJobId = scheduledJob.Id;
                    buildJob.ScheduledStartTime = scheduledJob.ScheduledStart;
                    buildJob.ScheduledEndTime = scheduledJob.ScheduledEnd;

                    // First save core job + build changes so failure to log adjustments doesn't rollback them
                    await _context.SaveChangesAsync();

                    // Now attempt to write adjustments (ignore failure if table missing)
                    if (_adjTableExists && pendingAdjustments.Any())
                    {
                        try
                        {
                            _context.ScheduleAdjustments.AddRange(pendingAdjustments);
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception aex)
                        {
                            _logger.LogWarning(aex, "Failed to write schedule adjustments – proceeding without them");
                        }
                    }

                    if (netCascadeShift.TotalMinutes > 15)
                    {
                        await CascadeScheduleChangesAsync(canonicalMachineId, originalEnd, netCascadeShift);
                        await ReflowDownstreamJobsWithChangeoverAsync(scheduledJob, netCascadeShift);
                    }
                }
                else
                {
                    // No scheduled job link – just save build
                    await _context.SaveChangesAsync();
                }

                // Powder inventory deduction (if service available and input provided)
                if (model.AddPowder && model.PowderAddedKg.HasValue && model.PowderAddedKg.Value > 0 && !string.IsNullOrWhiteSpace(machineMaterialCode) && _materialService != null)
                {
                    try
                    {
                        var adjust = await _materialService.AdjustMaterialQuantityAsync(machineMaterialCode, -model.PowderAddedKg.Value, "PrintStart");
                        if (!adjust.Success)
                            _logger.LogWarning("Material adjustment failed for {Mat} qty {Qty}", machineMaterialCode, model.PowderAddedKg.Value);
                    }
                    catch (Exception mex)
                    {
                        _logger.LogWarning(mex, "Material service exception while deducting powder");
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Started build {BuildId} on {Printer} (JobId={JobId}) netShift={Shift}m End={End}", buildJob.BuildId, canonicalMachineId, buildJob.AssociatedScheduledJobId, netCascadeShift.TotalMinutes, buildJob.ScheduledEndTime);
                return buildJob.BuildId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting print job for {Printer}", model.PrinterName);
                throw;
            }
        }

        // NEW: Reflow downstream jobs applying 3h changeover + shift alignment similar to scheduling logic
        private async Task ReflowDownstreamJobsWithChangeoverAsync(Job anchorJob, TimeSpan initialShift)
        {
            try
            {
                const double changeoverHours = 3.0;
                var machineId = anchorJob.MachineId;
                var anchorEnd = anchorJob.ScheduledEnd; // already updated

                // Ensure table availability (safe no-op if already checked)
                await EnsureScheduleAdjustmentsTableAsync();

                var downstream = await _context.Jobs
                    .Where(j => j.MachineId == machineId && j.ScheduledStart >= anchorJob.ScheduledStart && j.Id != anchorJob.Id && j.Status == "Scheduled")
                    .OrderBy(j => j.ScheduledStart)
                    .ToListAsync();

                DateTime previousEnd = anchorEnd; // rolling end

                foreach (var job in downstream)
                {
                    var originalStart = job.ScheduledStart;
                    var originalEnd = job.ScheduledEnd;
                    var duration = originalEnd - originalStart;

                    // required earliest start after changeover buffer
                    DateTime candidate = previousEnd.AddHours(changeoverHours);

                    // Align to shift if service available
                    candidate = await AlignToShiftWithSetupAsync(candidate, machineId, changeoverHours);

                    job.ScheduledStart = candidate;
                    job.ScheduledEnd = candidate + duration;
                    previousEnd = job.ScheduledEnd;

                    // Record adjustment only if table exists
                    if (_adjTableExists)
                    {
                        _context.ScheduleAdjustments.Add(new ScheduleAdjustment
                        {
                            TriggerJobId = anchorJob.Id,
                            AffectedJobId = job.Id,
                            OriginalStart = originalStart,
                            OriginalEnd = originalEnd,
                            NewStart = job.ScheduledStart,
                            NewEnd = job.ScheduledEnd,
                            ShiftMinutes = (job.ScheduledStart - originalStart).TotalMinutes,
                            Reason = initialShift.TotalMinutes > 0 ? "DurationIncrease" : "DelayCascade",
                            Notes = "Auto reflow with 3h changeover"
                        });
                    }
                }

                if (downstream.Any())
                {
                    _logger.LogInformation("Reflowed {Count} downstream jobs on {Machine} after job {Anchor}", downstream.Count, machineId, anchorJob.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed cascading downstream jobs after job {JobId}", anchorJob.Id);
            }
        }

        // Shift alignment helper replicating simplified logic from scheduler
        private async Task<DateTime> AlignToShiftWithSetupAsync(DateTime candidate, string machineId, double setupHours)
        {
            if (_shiftService == null) return candidate; // no shift enforcement if service missing
            try
            {
                int guard = 0;
                while (guard < 96)
                {
                    guard++;
                    var shiftsToday = await _shiftService.GetShiftsForDayAsync(candidate.DayOfWeek, machineId) ?? new List<OperatingShift>();
                    var shiftsPrev = await _shiftService.GetShiftsForDayAsync(candidate.AddDays(-1).DayOfWeek, machineId) ?? new List<OperatingShift>();

                    foreach (var (start, end) in Enumerate(shiftsPrev, candidate.AddDays(-1).Date).Concat(Enumerate(shiftsToday, candidate.Date)).OrderBy(s => s.start))
                    {
                        if (candidate < start) candidate = start; // jump forward into shift
                        if (candidate >= start && candidate < end)
                        {
                            var minOp = start.AddHours(setupHours);
                            if (candidate < minOp) candidate = minOp;
                            if (candidate >= end)
                                break; // will loop again to find next shift
                            return candidate;
                        }
                    }
                    candidate = candidate.AddMinutes(30); // step forward and retry
                }
                return candidate;
            }
            catch { return candidate; }

            IEnumerable<(DateTime start, DateTime end)> Enumerate(IEnumerable<OperatingShift> shifts, DateTime day)
            {
                foreach (var s in shifts)
                {
                    var sStart = day + s.StartTime;
                    var sEnd = day + s.EndTime;
                    if (s.EndTime < s.StartTime) sEnd = sEnd.AddDays(1);
                    yield return (sStart, sEnd);
                }
            }
        }

        private string ResolveFriendlyMaterial(string code, string fallback)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["TI64-G5"] = "Ti-6Al-4V Grade 5",
                ["TI64-G23"] = "Ti-6Al-4V ELI Grade 23",
                ["IN718"] = "Inconel 718",
                ["IN625"] = "Inconel 625",
                ["SS316L"] = "316L Stainless Steel",
                ["ALSI10MG"] = "AlSi10Mg"
            };
            return map.TryGetValue(code, out var friendly) ? friendly : fallback;
        }

        // ... existing methods unchanged ...

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
            try
            {
                var activePrototypes = await _context.PrototypeJobs
                    .Include(pj => pj.Part)
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
                    CompletedStages = 0,
                    TotalStages = 0,
                    OverallProgress = 0
                }).ToList();
            }
            catch
            {
                return new List<PrototypeJobInfo>();
            }
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
                }
                if (affectedJobs.Any())
                    await _context.SaveChangesAsync();
            }
            catch { }
        }

        private async Task CascadeScheduleChangesAsync(string machineId, DateTime fromTime, TimeSpan timeDifference)
        {
            try
            {
                var subsequentJobs = await _context.Jobs
                    .Where(j => j.MachineId == machineId &&
                               j.ScheduledStart >= fromTime &&
                               j.Status == "Scheduled")
                    .OrderBy(j => j.ScheduledStart)
                    .ToListAsync();
                foreach (var job in subsequentJobs)
                {
                    job.ScheduledStart = job.ScheduledStart.Add(timeDifference);
                    job.ScheduledEnd = job.ScheduledEnd.Add(timeDifference);
                }
                if (subsequentJobs.Any())
                    await _context.SaveChangesAsync();
            }
            catch { }
        }

        // ===== Re-implemented interface methods (delegating to existing helpers) =====
        public async Task<bool> CompletePrintJobAsync(PostPrintViewModel model, int userId)
        {
            var buildJob = await _context.BuildJobs.FirstOrDefaultAsync(b => b.BuildId == model.BuildId);
            if (buildJob == null)
            {
                // Try infer build by associated job
                if (model.JobId.HasValue)
                {
                    buildJob = await _context.BuildJobs.FirstOrDefaultAsync(b => b.AssociatedScheduledJobId == model.JobId && b.Status == "In Progress");
                }
            }
            if (buildJob == null) return false;

            buildJob.ActualEndTime = model.ActualEndTime;
            buildJob.ReasonForEnd = model.ReasonForEnd;
            buildJob.Status = GetBuildJobStatus(model.ReasonForEnd);
            buildJob.CompletedAt = DateTime.UtcNow;
            buildJob.LaserRunTime = model.LaserRunTime;
            buildJob.GasUsed_L = model.GasUsed_L;
            buildJob.PowderUsed_L = model.PowderUsed_L;
            buildJob.Notes = model.Notes;
            buildJob.OperatorActualHours = model.OperatorActualHours;
            buildJob.OperatorBuildAssessment = model.OperatorBuildAssessment;
            buildJob.MachinePerformanceNotes = model.MachinePerformanceNotes;
            buildJob.DefectCount = model.DefectCount ?? 0;
            buildJob.LessonsLearned = model.LessonsLearned;
            if (model.TimeFactors?.Any() == true)
            {
                var existing = !string.IsNullOrEmpty(buildJob.TimeFactors) ? buildJob.TimeFactors.Split(',').ToList() : new List<string>();
                existing.AddRange(model.TimeFactors);
                buildJob.TimeFactors = string.Join(",", existing.Distinct());
            }

            // Update scheduled job linkage
            Job? job = null;
            if (buildJob.AssociatedScheduledJobId.HasValue)
                job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == buildJob.AssociatedScheduledJobId.Value);
            else if (model.JobId.HasValue)
                job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == model.JobId.Value);

            if (job != null)
            {
                if (!job.ActualStart.HasValue) job.ActualStart = model.ActualStartTime; // safety
                job.ActualEnd = model.ActualEndTime;
                job.Status = buildJob.Status == "Completed" ? "Completed" : job.Status switch
                {
                    _ when buildJob.Status == "Aborted" => "Aborted",
                    _ when buildJob.Status == "Rework" => "Rework",
                    _ => "Completed"
                };
                job.LastModifiedDate = DateTime.UtcNow;
                job.LastModifiedBy = "PrintTracking";
                // Adjust scheduled end if actual finished early/late > 10 min
                var diff = model.ActualEndTime - job.ScheduledEnd;
                if (Math.Abs(diff.TotalMinutes) > 10)
                {
                    var shift = model.ActualEndTime - job.ScheduledEnd;
                    job.ScheduledEnd = model.ActualEndTime;
                    if (job.EstimatedHours > 0 && job.Quantity > 0)
                    {
                        var newDuration = (job.ScheduledEnd - job.ScheduledStart).TotalHours;
                        await UpdatePartDurationFromScheduleAsync(job.Id, newDuration); // updates part estimate
                    }
                    if (shift.TotalMinutes != 0)
                        await CascadeScheduleChangesAsync(job.MachineId, job.ScheduledEnd, shift);
                }
            }

            // Capture machine id before removing build record
            var machineIdForStatus = buildJob.PrinterName;

            // NEW: Remove the build job record after completion so dashboard list does not accumulate
            _context.BuildJobs.Remove(buildJob);

            // Update machine status to Idle if no other active builds remain
            try
            {
                var machine = await _context.Machines.FirstOrDefaultAsync(m => m.MachineId == machineIdForStatus);
                if (machine != null)
                {
                    var stillHasActive = await _context.BuildJobs.AnyAsync(b => b.PrinterName == machine.MachineId && b.Status == "In Progress");
                    machine.Status = stillHasActive ? "Building" : "Idle";
                }
            }
            catch (Exception mex)
            {
                _logger.LogWarning(mex, "Failed to update machine status on completion for {Machine}", machineIdForStatus);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public Task<List<Job>> GetAvailableScheduledJobsAsync(string printerName) => _context.Jobs.Where(j => j.MachineId == printerName && j.Status == "Scheduled").OrderBy(j => j.ScheduledStart).ToListAsync();
        public Task<List<JobStage>> GetAvailableJobStagesAsync(string printerName) => Task.FromResult(new List<JobStage>());
        public Task<List<PrototypeJob>> GetAvailablePrototypeJobsAsync() => _context.PrototypeJobs.Where(p => p.Status == "Scheduled" && p.IsActive).OrderBy(p => p.Priority).ToListAsync();
        public Task<BuildJob?> GetActiveBuildJobAsync(string printerName) => _context.BuildJobs.Where(b => b.PrinterName == printerName && b.Status == "In Progress").FirstOrDefaultAsync();
        public Task<bool> HasActiveBuildAsync(string printerName) => _context.BuildJobs.AnyAsync(b => b.PrinterName == printerName && b.Status == "In Progress");
        public Task<List<BuildJob>> GetRecentBuildsAsync(int count = 20) => _context.BuildJobs.Include(b => b.User).Include(b => b.Part).OrderByDescending(b => b.CreatedAt).Take(count).ToListAsync();
        public Task CreateCooldownAndChangeoverBlocksAsync(BuildJob completedJob) => Task.CompletedTask;

        public async Task<MultiStageWorkflowViewModel> GetWorkflowStatusAsync(int jobId)
        {
            var job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId);
            if (job == null) return new MultiStageWorkflowViewModel();
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

        public Task<bool> AdvanceJobStageAsync(int jobStageId, int userId) => Task.FromResult(false);
        public Task<bool> UpdateStageProgressAsync(int jobStageId, double progressPercent, string? statusUpdate = null) => Task.FromResult(false);
        public Task<List<Part>> GetAvailablePartsAsync() => _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
        public async Task<bool> ValidatePartCompatibilityAsync(string partNumber, string machineId) => await _context.Parts.AnyAsync(p => p.PartNumber == partNumber && p.IsActive);
        public Task<bool> TryCreateCohortFromCompletedBuildAsync(BuildJob buildJob) => Task.FromResult(false);

        public async Task<BuildTimeEstimate> GetBuildTimeEstimateAsync(string buildFileHash, string machineType)
        {
            var historical = await _context.BuildJobs.Where(b => b.BuildFileHash == buildFileHash && b.Status == "Completed" && b.OperatorActualHours.HasValue)
                .OrderByDescending(b => b.CreatedAt). Take(10).ToListAsync();
            if (historical.Any())
            {
                var avg = historical.Average(b => b.OperatorActualHours!.Value);
                return new BuildTimeEstimate { EstimatedHours = avg, ConfidenceLevel = Math.Min(100, historical.Count * 10), BasedOnBuilds = historical.Count, LastBuildDate = historical.First().CreatedAt, MachineSpecific = true };
            }
            return new BuildTimeEstimate { EstimatedHours = 8.0m, ConfidenceLevel = 10, BasedOnBuilds = 0, MachineSpecific = false };
        }

        public async Task LogOperatorEstimateAsync(int buildId, decimal estimatedHours, string notes)
        {
            var build = await _context.BuildJobs.FindAsync(buildId);
            if (build == null) return;
            build.OperatorEstimatedHours = estimatedHours;
            if (!string.IsNullOrEmpty(notes))
                build.Notes = string.IsNullOrEmpty(build.Notes) ? notes : build.Notes + "\n" + notes;
            await _context.SaveChangesAsync();
        }

        public async Task RecordActualBuildTimeAsync(int buildId, decimal actualHours, string assessment)
        {
            var build = await _context.BuildJobs.FindAsync(buildId);
            if (build == null) return;
            build.OperatorActualHours = actualHours;
            build.OperatorBuildAssessment = assessment;
            await _context.SaveChangesAsync();
        }

        public async Task AnalyzeBuildPerformanceAsync(int buildId)
        {
            var build = await _context.BuildJobs.FindAsync(buildId);
            if (build?.OperatorEstimatedHours.HasValue == true && build.OperatorActualHours.HasValue)
            {
                var est = build.OperatorEstimatedHours.Value;
                var act = build.OperatorActualHours.Value;
                var variancePct = est > 0 ? Math.Abs(act - est) / est * 100 : 0;
                var perf = variancePct switch { <= 10 => "Excellent", <= 20 => "Good", <= 30 => "Fair", _ => "Needs Improvement" };
                build.MachinePerformanceNotes = string.IsNullOrEmpty(build.MachinePerformanceNotes) ? perf : build.MachinePerformanceNotes + "\n" + perf;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<BuildPerformanceData>> GetHistoricalBuildDataAsync(string partNumber)
        {
            var builds = await _context.BuildJobs.Where(b => b.Status == "Completed" && b.OperatorEstimatedHours.HasValue && b.OperatorActualHours.HasValue)
                .OrderByDescending(b => b.CreatedAt). Take(50).ToListAsync();
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

        public async Task UpdateBuildTimeLearningAsync(int buildId, BuildCompletionData data)
        {
            var build = await _context.BuildJobs.FindAsync(buildId);
            if (build == null) return;
            build.BuildFileHash = data.BuildFileHash;
            build.LayerCount = data.LayerCount;
            build.BuildHeight = data.BuildHeight;
            build.SupportComplexity = data.SupportComplexity;
            build.PartOrientations = data.PartOrientations;
            build.PostProcessingNeeded = data.PostProcessingNeeded;
            build.DefectCount = data.DefectCount;
            build.LessonsLearned = data.LessonsLearned;
            build.TimeFactors = data.TimeFactors;
            build.PowerConsumption = data.PowerConsumption;
            build.LaserOnTime = data.LaserOnTime;
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePartDurationFromScheduleAsync(int jobId, double newDurationHours)
        {
            var job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId);
            if (job?.Part == null || job.Quantity <= 0) return;
            var timePerPart = newDurationHours / job.Quantity;
            job.Part.EstimatedHours = timePerPart;
            job.Part.LastModifiedDate = DateTime.UtcNow;
            job.EstimatedHours = newDurationHours;
            await _context.SaveChangesAsync();
        }

        public async Task<int> CreateBuildJobFromScheduledJobAsync(int jobId, string operatorName)
        {
            var job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId);
            if (job == null) throw new InvalidOperationException($"Job {jobId} not found");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FullName == operatorName) ?? await _context.Users.FirstAsync();
            var build = new BuildJob
            {
                BuildId = await GenerateBuildIdAsync(),
                PrinterName = job.MachineId,
                ActualStartTime = job.ActualStart ?? DateTime.UtcNow,
                Status = "In Progress",
                PartId = job.PartId,
                UserId = user.Id,
                AssociatedScheduledJobId = jobId,
                OperatorEstimatedHours = (decimal)job.EstimatedHours,
                TotalPartsInBuild = job.Quantity,
                ScheduledStartTime = job.ScheduledStart,
                ScheduledEndTime = job.ScheduledEnd,
                BuildFileHash = GenerateBuildFileHash(job.PartNumber),
                IsLearningBuild = true,
                SetupNotes = $"Started from scheduler job {jobId} - {job.PartNumber} (Qty: {job.Quantity})"
            };
            _context.BuildJobs.Add(build);
            // Also update machine status immediately for scheduler-origin builds
            try
            {
                var machine = await _context.Machines.FirstOrDefaultAsync(m => m.MachineId == job.MachineId);
                if (machine != null)
                {
                    machine.Status = "Building";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set machine status to Building for scheduler started build {JobId}", jobId);
            }
            await _context.SaveChangesAsync();
            return build.BuildId;
        }
        // ===== end restored methods =====
    }
}