using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services
{
    public interface IAnalyticsDashboardService
    {
        Task<ManufacturingMetrics> GetManufacturingMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
        Task<MachineUtilizationReport> GetMachineUtilizationAsync(string? machineId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
        Task<List<BuildPerformanceTrend>> GetBuildPerformanceTrendsAsync(int days = 30, CancellationToken ct = default);
        Task<PartProductionSummary> GetPartProductionSummaryAsync(int? partId = null, CancellationToken ct = default);
        Task<OEEMetrics> CalculateOEEAsync(string machineId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
        Task<List<StagePerformanceMetric>> GetStagePerformanceAsync(CancellationToken ct = default);
    }

    public class AnalyticsDashboardService : IAnalyticsDashboardService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<AnalyticsDashboardService> _logger;

        public AnalyticsDashboardService(SchedulerContext context, ILogger<AnalyticsDashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ManufacturingMetrics> GetManufacturingMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
        {
            fromDate ??= DateTime.UtcNow.AddDays(-30);
            toDate ??= DateTime.UtcNow;

            var jobs = await _context.Jobs
                .Where(j => j.CreatedDate >= fromDate && j.CreatedDate <= toDate)
                .ToListAsync(ct);

            var buildJobs = await _context.BuildJobs
                .Where(b => b.CreatedAt >= fromDate && b.CreatedAt <= toDate)
                .ToListAsync(ct);

            var completedJobs = jobs.Where(j => j.Status == "Completed" || j.Status == "Complete").ToList();
            var completedBuilds = buildJobs.Where(b => b.Status == "Completed").ToList();

            // Calculate metrics
            var totalScheduledHours = jobs.Sum(j => j.EstimatedHours);
            var totalActualHours = completedBuilds.Sum(b => b.PrintHours);
            var totalPartsProduced = completedBuilds.Sum(b => b.TotalPartsProduced);

            return new ManufacturingMetrics
            {
                PeriodStart = fromDate.Value,
                PeriodEnd = toDate.Value,
                TotalJobsScheduled = jobs.Count,
                TotalJobsCompleted = completedJobs.Count,
                TotalBuildsCompleted = completedBuilds.Count,
                TotalPartsProduced = totalPartsProduced,
                TotalScheduledHours = totalScheduledHours,
                TotalActualHours = totalActualHours,
                ScheduleEfficiency = totalScheduledHours > 0 
                    ? Math.Round(totalActualHours / totalScheduledHours * 100, 2) 
                    : 0,
                OnTimeDeliveryRate = jobs.Any() 
                    ? Math.Round((double)completedJobs.Count(j => j.IsOnTime) / completedJobs.Count * 100, 2) 
                    : 100,
                AverageBuildTime = completedBuilds.Any() 
                    ? Math.Round(completedBuilds.Average(b => b.PrintHours), 2) 
                    : 0,
                AveragePartsPerBuild = completedBuilds.Any() 
                    ? Math.Round(completedBuilds.Average(b => (double)b.TotalPartsProduced), 1) 
                    : 0,
                JobsInProgress = jobs.Count(j => j.Status == "Active" || j.Status == "Building" || j.Status == "In Progress"),
                JobsDelayed = jobs.Count(j => j.Status == "Delayed"),
                MaterialUsageKg = completedBuilds.Sum(b => b.PowderUsed_L ?? 0) / 1000 // Convert L to kg approximation
            };
        }

        public async Task<MachineUtilizationReport> GetMachineUtilizationAsync(string? machineId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
        {
            fromDate ??= DateTime.UtcNow.AddDays(-7);
            toDate ??= DateTime.UtcNow;
            var totalHours = (toDate.Value - fromDate.Value).TotalHours;

            var query = _context.BuildJobs
                .Where(b => b.ActualStartTime >= fromDate && 
                           (b.ActualEndTime == null || b.ActualEndTime <= toDate));

            if (!string.IsNullOrEmpty(machineId))
            {
                query = query.Where(b => b.PrinterName == machineId);
            }

            var builds = await query.ToListAsync(ct);
            var machines = await _context.Machines
                .Where(m => m.MachineType == "SLS" || m.MachineType == "Printer")
                .ToListAsync(ct);

            var machineMetrics = new List<MachineUtilizationMetric>();

            foreach (var machine in machines)
            {
                if (!string.IsNullOrEmpty(machineId) && machine.MachineId != machineId)
                    continue;

                var machineBuilds = builds.Where(b => b.PrinterName == machine.MachineId).ToList();
                var runningHours = machineBuilds.Sum(b => b.PrintHours);

                machineMetrics.Add(new MachineUtilizationMetric
                {
                    MachineId = machine.MachineId,
                    MachineName = machine.Name,
                    TotalBuilds = machineBuilds.Count,
                    RunningHours = runningHours,
                    AvailableHours = totalHours,
                    UtilizationPercent = totalHours > 0 
                        ? Math.Round(runningHours / totalHours * 100, 2) 
                        : 0,
                    AverageBuildTime = machineBuilds.Any() 
                        ? Math.Round(machineBuilds.Average(b => b.PrintHours), 2) 
                        : 0,
                    TotalPartsProduced = machineBuilds.Sum(b => b.TotalPartsProduced)
                });
            }

            return new MachineUtilizationReport
            {
                PeriodStart = fromDate.Value,
                PeriodEnd = toDate.Value,
                TotalAvailableHours = totalHours * machines.Count,
                TotalRunningHours = machineMetrics.Sum(m => m.RunningHours),
                OverallUtilization = machineMetrics.Any() 
                    ? Math.Round(machineMetrics.Average(m => m.UtilizationPercent), 2) 
                    : 0,
                MachineMetrics = machineMetrics
            };
        }

        public async Task<List<BuildPerformanceTrend>> GetBuildPerformanceTrendsAsync(int days = 30, CancellationToken ct = default)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            
            var builds = await _context.BuildJobs
                .Where(b => b.CreatedAt >= fromDate && b.Status == "Completed")
                .OrderBy(b => b.CreatedAt)
                .ToListAsync(ct);

            var trends = builds
                .GroupBy(b => b.CreatedAt.Date)
                .Select(g => new BuildPerformanceTrend
                {
                    Date = g.Key,
                    BuildCount = g.Count(),
                    TotalHours = Math.Round(g.Sum(b => b.PrintHours), 2),
                    AverageHours = Math.Round(g.Average(b => b.PrintHours), 2),
                    TotalParts = g.Sum(b => b.TotalPartsProduced),
                    AveragePartsPerBuild = Math.Round(g.Average(b => (double)b.TotalPartsProduced), 1)
                })
                .OrderBy(t => t.Date)
                .ToList();

            return trends;
        }

        public async Task<PartProductionSummary> GetPartProductionSummaryAsync(int? partId = null, CancellationToken ct = default)
        {
            var query = _context.BuildJobParts
                .Include(bjp => bjp.BuildJob)
                .Where(bjp => bjp.BuildJob != null && bjp.BuildJob.Status == "Completed");

            if (partId.HasValue)
            {
                var part = await _context.Parts.FindAsync(new object[] { partId.Value }, ct);
                if (part != null)
                {
                    query = query.Where(bjp => bjp.PartNumber == part.PartNumber);
                }
            }

            var partBuilds = await query.ToListAsync(ct);

            var partMetrics = partBuilds
                .GroupBy(p => p.PartNumber)
                .Select(g => new PartProductionMetric
                {
                    PartNumber = g.Key,
                    TotalBuilds = g.Select(p => p.BuildId).Distinct().Count(),
                    TotalQuantityProduced = g.Sum(p => p.Quantity),
                    AverageQuantityPerBuild = Math.Round(g.Average(p => (double)p.Quantity), 1),
                    TotalBuildHours = Math.Round(g.Sum(p => p.EstimatedHours), 2)
                })
                .OrderByDescending(p => p.TotalQuantityProduced)
                .ToList();

            return new PartProductionSummary
            {
                TotalUniqueParts = partMetrics.Count,
                TotalPartsProduced = partMetrics.Sum(p => p.TotalQuantityProduced),
                TotalBuildHours = partMetrics.Sum(p => p.TotalBuildHours),
                PartMetrics = partMetrics.Take(50).ToList() // Top 50 parts
            };
        }

        public async Task<OEEMetrics> CalculateOEEAsync(string machineId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            var totalTime = (toDate - fromDate).TotalHours;
            
            // Get builds for this machine in the period
            var builds = await _context.BuildJobs
                .Where(b => b.PrinterName == machineId && 
                           b.ActualStartTime >= fromDate && 
                           b.ActualStartTime <= toDate)
                .ToListAsync(ct);

            var completedBuilds = builds.Where(b => b.Status == "Completed").ToList();

            // Availability = Running Time / Planned Production Time
            var runningTime = completedBuilds.Sum(b => b.PrintHours);
            var plannedTime = totalTime * 0.85; // Assume 85% planned production (15% planned maintenance)
            var availability = plannedTime > 0 ? runningTime / plannedTime : 0;

            // Performance = (Ideal Cycle Time * Total Count) / Running Time
            // For SLS, ideal cycle time is harder to define, so we use estimated vs actual
            var estimatedTime = completedBuilds.Sum(b => (double)(b.OperatorEstimatedHours ?? (decimal)b.PrintHours));
            var performance = runningTime > 0 ? estimatedTime / runningTime : 1;
            performance = Math.Min(performance, 1.0); // Cap at 100%

            // Quality = Good Parts / Total Parts
            var totalParts = completedBuilds.Sum(b => b.TotalPartsProduced);
            var defectParts = completedBuilds.Sum(b => b.DefectCount ?? 0);
            var quality = totalParts > 0 ? (double)(totalParts - defectParts) / totalParts : 1;

            var oee = availability * performance * quality;

            return new OEEMetrics
            {
                MachineId = machineId,
                PeriodStart = fromDate,
                PeriodEnd = toDate,
                Availability = Math.Round(availability * 100, 2),
                Performance = Math.Round(performance * 100, 2),
                Quality = Math.Round(quality * 100, 2),
                OEE = Math.Round(oee * 100, 2),
                TotalBuilds = completedBuilds.Count,
                TotalParts = totalParts,
                DefectParts = defectParts,
                RunningHours = Math.Round(runningTime, 2),
                PlannedHours = Math.Round(plannedTime, 2),
                DowntimeHours = Math.Round(plannedTime - runningTime, 2)
            };
        }

        public async Task<List<StagePerformanceMetric>> GetStagePerformanceAsync(CancellationToken ct = default)
        {
            var stageHistories = await _context.JobStageHistories
                .Where(h => h.StageHours.HasValue && h.StageHours > 0)
                .ToListAsync(ct);

            var stageMetrics = stageHistories
                .GroupBy(h => h.ProductionStageId)
                .Select(g =>
                {
                    var stage = _context.ProductionStages.Find(g.Key);
                    return new StagePerformanceMetric
                    {
                        StageId = g.Key ?? 0,
                        StageName = stage?.Name ?? "Unknown",
                        StageType = stage?.Department ?? "General",
                        TotalCompletions = g.Count(),
                        AverageHours = Math.Round(g.Average(h => h.StageHours ?? 0), 2),
                        MinHours = Math.Round(g.Min(h => h.StageHours ?? 0), 2),
                        MaxHours = Math.Round(g.Max(h => h.StageHours ?? 0), 2),
                        TotalHours = Math.Round(g.Sum(h => h.StageHours ?? 0), 2)
                    };
                })
                .OrderBy(s => s.StageName)
                .ToList();

            return stageMetrics;
        }
    }

    #region DTOs

    public class ManufacturingMetrics
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalJobsScheduled { get; set; }
        public int TotalJobsCompleted { get; set; }
        public int TotalBuildsCompleted { get; set; }
        public int TotalPartsProduced { get; set; }
        public double TotalScheduledHours { get; set; }
        public double TotalActualHours { get; set; }
        public double ScheduleEfficiency { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public double AverageBuildTime { get; set; }
        public double AveragePartsPerBuild { get; set; }
        public int JobsInProgress { get; set; }
        public int JobsDelayed { get; set; }
        public double MaterialUsageKg { get; set; }
    }

    public class MachineUtilizationReport
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public double TotalAvailableHours { get; set; }
        public double TotalRunningHours { get; set; }
        public double OverallUtilization { get; set; }
        public List<MachineUtilizationMetric> MachineMetrics { get; set; } = new();
    }

    public class MachineUtilizationMetric
    {
        public string MachineId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public int TotalBuilds { get; set; }
        public double RunningHours { get; set; }
        public double AvailableHours { get; set; }
        public double UtilizationPercent { get; set; }
        public double AverageBuildTime { get; set; }
        public int TotalPartsProduced { get; set; }
    }

    public class BuildPerformanceTrend
    {
        public DateTime Date { get; set; }
        public int BuildCount { get; set; }
        public double TotalHours { get; set; }
        public double AverageHours { get; set; }
        public int TotalParts { get; set; }
        public double AveragePartsPerBuild { get; set; }
    }

    public class PartProductionSummary
    {
        public int TotalUniqueParts { get; set; }
        public int TotalPartsProduced { get; set; }
        public double TotalBuildHours { get; set; }
        public List<PartProductionMetric> PartMetrics { get; set; } = new();
    }

    public class PartProductionMetric
    {
        public string PartNumber { get; set; } = string.Empty;
        public int TotalBuilds { get; set; }
        public int TotalQuantityProduced { get; set; }
        public double AverageQuantityPerBuild { get; set; }
        public double TotalBuildHours { get; set; }
    }

    public class OEEMetrics
    {
        public string MachineId { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public double Availability { get; set; }
        public double Performance { get; set; }
        public double Quality { get; set; }
        public double OEE { get; set; }
        public int TotalBuilds { get; set; }
        public int TotalParts { get; set; }
        public int DefectParts { get; set; }
        public double RunningHours { get; set; }
        public double PlannedHours { get; set; }
        public double DowntimeHours { get; set; }
    }

    public class StagePerformanceMetric
    {
        public int StageId { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string StageType { get; set; } = string.Empty;
        public int TotalCompletions { get; set; }
        public double AverageHours { get; set; }
        public double MinHours { get; set; }
        public double MaxHours { get; set; }
        public double TotalHours { get; set; }
    }

    #endregion
}
