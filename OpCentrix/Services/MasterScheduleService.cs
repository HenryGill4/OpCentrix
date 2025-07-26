using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.ViewModels;
using OpCentrix.Services.Admin;

namespace OpCentrix.Services
{
    /// <summary>
    /// Master Schedule Service for comprehensive production visibility
    /// Task 12: Master Schedule View with real-time analytics and reporting
    /// </summary>
    public interface IMasterScheduleService
    {
        Task<MasterScheduleViewModel> GetMasterScheduleAsync(MasterScheduleFilters filters);
        Task<MasterScheduleMetrics> CalculateMetricsAsync(DateTime startDate, DateTime endDate);
        Task<List<ScheduleAlert>> GetActiveAlertsAsync();
        Task<List<ResourceUtilization>> GetResourceUtilizationAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> ExportScheduleAsync(MasterScheduleExportOptions options);
        Task<bool> ResolveAlertAsync(int alertId, string resolvedBy);
        Task<List<MasterScheduleTimeSlot>> GetTimelineDataAsync(DateTime startDate, DateTime endDate);
        Task RefreshRealTimeDataAsync();
    }

    public class MasterScheduleService : IMasterScheduleService
    {
        private readonly SchedulerContext _context;
        private readonly IMachineManagementService _machineService;
        private readonly ILogger<MasterScheduleService> _logger;

        public MasterScheduleService(
            SchedulerContext context,
            IMachineManagementService machineService,
            ILogger<MasterScheduleService> logger)
        {
            _context = context;
            _machineService = machineService;
            _logger = logger;
        }

        public async Task<MasterScheduleViewModel> GetMasterScheduleAsync(MasterScheduleFilters filters)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [MASTER-{OperationId}] Loading master schedule view with filters", operationId);

            try
            {
                var viewModel = new MasterScheduleViewModel
                {
                    StartDate = filters.StartDate ?? DateTime.Today,
                    EndDate = filters.EndDate ?? DateTime.Today.AddDays(30)
                };

                // Load jobs with comprehensive data
                viewModel.Jobs = await LoadMasterScheduleJobsAsync(filters, operationId);
                
                // Load machine status and utilization
                viewModel.Machines = await LoadMasterScheduleMachinesAsync(operationId);
                
                // Calculate real-time metrics
                viewModel.Metrics = await CalculateMetricsAsync(viewModel.StartDate, viewModel.EndDate);
                
                // Load timeline data for visualization
                viewModel.Timeline = await GetTimelineDataAsync(viewModel.StartDate, viewModel.EndDate);
                
                // Load resource utilization
                viewModel.ResourceUtilization = await GetResourceUtilizationAsync(viewModel.StartDate, viewModel.EndDate);
                
                // Load active alerts
                viewModel.Alerts = await GetActiveAlertsAsync();
                
                // Populate filter options
                await PopulateFilterOptionsAsync(viewModel);

                _logger.LogInformation("? [MASTER-{OperationId}] Master schedule loaded: {JobCount} jobs, {MachineCount} machines, {AlertCount} alerts",
                    operationId, viewModel.Jobs.Count, viewModel.Machines.Count, viewModel.Alerts.Count);

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-{OperationId}] Error loading master schedule: {ErrorMessage}",
                    operationId, ex.Message);
                throw;
            }
        }

        private async Task<List<MasterScheduleJob>> LoadMasterScheduleJobsAsync(MasterScheduleFilters filters, string operationId)
        {
            try
            {
                var query = _context.Jobs
                    .Include(j => j.Part)
                    .AsQueryable();

                // Apply date filters
                if (filters.StartDate.HasValue)
                    query = query.Where(j => j.ScheduledEnd >= filters.StartDate.Value);
                
                if (filters.EndDate.HasValue)
                    query = query.Where(j => j.ScheduledStart <= filters.EndDate.Value);

                // Apply department filters
                if (filters.Departments.Any())
                    query = query.Where(j => filters.Departments.Contains(j.MachineId.Substring(0, 2))); // Simplified department logic

                // Apply machine filters
                if (filters.Machines.Any())
                    query = query.Where(j => filters.Machines.Contains(j.MachineId));

                // Apply status filters
                if (filters.Statuses.Any())
                    query = query.Where(j => filters.Statuses.Contains(j.Status));

                // Apply priority filters
                if (filters.Priorities.Any())
                    query = query.Where(j => filters.Priorities.Contains(j.Priority.ToString()));

                // Apply search term
                if (!string.IsNullOrEmpty(filters.SearchTerm))
                {
                    var searchLower = filters.SearchTerm.ToLower();
                    query = query.Where(j => 
                        j.PartNumber.ToLower().Contains(searchLower) ||
                        j.MachineId.ToLower().Contains(searchLower) ||
                        (j.Part != null && j.Part.Description.ToLower().Contains(searchLower)));
                }

                var jobs = await query
                    .OrderBy(j => j.ScheduledStart)
                    .ThenBy(j => j.Priority)
                    .AsNoTracking()
                    .ToListAsync();

                var masterJobs = jobs.Select(job => new MasterScheduleJob
                {
                    Id = job.Id,
                    PartNumber = job.PartNumber,
                    MachineId = job.MachineId,
                    MachineName = GetMachineName(job.MachineId),
                    Department = GetDepartmentFromMachine(job.MachineId),
                    Status = job.Status,
                    Priority = job.Priority.ToString(),
                    ScheduledStart = job.ScheduledStart,
                    ScheduledEnd = job.ScheduledEnd,
                    ActualStart = job.ActualStart,
                    ActualEnd = job.ActualEnd,
                    EstimatedHours = job.EstimatedHours,
                    ActualHours = job.ActualHours,
                    ProgressPercent = CalculateJobProgress(job),
                    EstimatedCost = job.EstimatedTotalCost,
                    ActualCost = job.ActualLaborCost + job.ActualMaterialCost, // Use calculated cost
                    SlsMaterial = job.SlsMaterial ?? "Unknown",
                    Quantity = job.Quantity
                }).ToList();

                _logger.LogDebug("?? [MASTER-{OperationId}] Loaded {JobCount} jobs for master schedule", operationId, masterJobs.Count);
                return masterJobs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-{OperationId}] Error loading jobs: {ErrorMessage}", operationId, ex.Message);
                return new List<MasterScheduleJob>();
            }
        }

        private async Task<List<MasterScheduleMachine>> LoadMasterScheduleMachinesAsync(string operationId)
        {
            try
            {
                var machines = await _machineService.GetActiveMachinesAsync();
                var masterMachines = new List<MasterScheduleMachine>();

                foreach (var machine in machines)
                {
                    var utilization = await CalculateMachineUtilizationAsync(machine.MachineId);
                    var jobCounts = await GetMachineJobCountsAsync(machine.MachineId);
                    var currentJob = await GetCurrentJobAsync(machine.MachineId);

                    var masterMachine = new MasterScheduleMachine
                    {
                        MachineId = machine.MachineId,
                        MachineName = machine.Name ?? machine.MachineId,
                        Department = GetDepartmentFromMachine(machine.MachineId),
                        Status = await GetMachineStatusAsync(machine.MachineId),
                        UtilizationPercent = utilization.UtilizationPercent,
                        EfficiencyPercent = utilization.EfficiencyPercent,
                        ActiveJobs = jobCounts.Active,
                        QueuedJobs = jobCounts.Queued,
                        CompletedJobs = jobCounts.Completed,
                        TotalScheduledTime = utilization.ScheduledTime,
                        TotalActualTime = utilization.ActualTime,
                        LastMaintenanceDate = machine.LastMaintenanceDate,
                        NextMaintenanceDate = machine.NextMaintenanceDate,
                        CurrentJobPartNumber = currentJob?.PartNumber ?? string.Empty,
                        CurrentJobStartTime = currentJob?.ActualStart,
                        CurrentJobEndTime = currentJob?.ScheduledEnd,
                        CurrentJobProgress = currentJob != null ? CalculateJobProgress(currentJob) : null
                    };

                    masterMachines.Add(masterMachine);
                }

                _logger.LogDebug("?? [MASTER-{OperationId}] Loaded {MachineCount} machines for master schedule", operationId, masterMachines.Count);
                return masterMachines;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-{OperationId}] Error loading machines: {ErrorMessage}", operationId, ex.Message);
                return new List<MasterScheduleMachine>();
            }
        }

        public async Task<MasterScheduleMetrics> CalculateMetricsAsync(DateTime startDate, DateTime endDate)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [MASTER-{OperationId}] Calculating metrics from {StartDate} to {EndDate}", 
                operationId, startDate, endDate);

            try
            {
                var jobs = await _context.Jobs
                    .Where(j => j.ScheduledStart >= startDate && j.ScheduledEnd <= endDate)
                    .AsNoTracking()
                    .ToListAsync();

                var metrics = new MasterScheduleMetrics
                {
                    TotalJobs = jobs.Count,
                    CompletedJobs = jobs.Count(j => j.Status.Equals("completed", StringComparison.OrdinalIgnoreCase)),
                    InProgressJobs = jobs.Count(j => j.Status.Equals("running", StringComparison.OrdinalIgnoreCase) || 
                                                    j.Status.Equals("building", StringComparison.OrdinalIgnoreCase)),
                    DelayedJobs = jobs.Count(j => j.ActualStart.HasValue && j.ActualStart > j.ScheduledStart),
                    CriticalJobs = jobs.Count(j => j.Priority <= 2),
                    
                    TotalEstimatedCost = jobs.Sum(j => j.EstimatedTotalCost),
                    TotalActualCost = jobs.Sum(j => j.ActualLaborCost + j.ActualMaterialCost),
                    
                    AverageJobDurationHours = jobs.Any() ? jobs.Average(j => j.EstimatedHours) : 0,
                    AverageDelayHours = CalculateAverageDelay(jobs)
                };

                // Calculate derived metrics with proper type casting
                metrics.CostVariancePercent = metrics.TotalEstimatedCost > 0 
                    ? (decimal)((double)((metrics.TotalActualCost - metrics.TotalEstimatedCost) / metrics.TotalEstimatedCost * 100))
                    : 0;

                metrics.OnTimeDeliveryPercent = metrics.TotalJobs > 0 
                    ? (double)(metrics.TotalJobs - metrics.DelayedJobs) / metrics.TotalJobs * 100 
                    : 100;

                metrics.OverallEfficiency = CalculateOverallEfficiency(jobs);
                metrics.UtilizationPercent = await CalculateOverallUtilizationAsync(startDate, endDate);
                metrics.QualityScorePercent = await CalculateQualityScoreAsync(startDate, endDate);

                // Calculate machine metrics
                var machines = await _machineService.GetActiveMachinesAsync();
                metrics.OperationalMachines = machines.Count(m => m.Status.Equals("operational", StringComparison.OrdinalIgnoreCase));
                metrics.MaintenanceMachines = machines.Count(m => m.Status.Equals("maintenance", StringComparison.OrdinalIgnoreCase));
                metrics.IdleMachines = machines.Count(m => m.Status.Equals("idle", StringComparison.OrdinalIgnoreCase));

                // Calculate trends (simplified - compare to previous period)
                var previousPeriod = startDate.AddDays(-(endDate - startDate).Days);
                var previousMetrics = await CalculatePreviousPeriodMetricsAsync(previousPeriod, startDate);
                
                metrics.EfficiencyTrend = CalculateTrend(metrics.OverallEfficiency, previousMetrics.OverallEfficiency);
                metrics.UtilizationTrend = CalculateTrend(metrics.UtilizationPercent, previousMetrics.UtilizationPercent);
                metrics.CostTrend = CalculateTrend((double)metrics.TotalActualCost, (double)previousMetrics.TotalActualCost);
                metrics.QualityTrend = CalculateTrend(metrics.QualityScorePercent, previousMetrics.QualityScorePercent);

                _logger.LogDebug("? [MASTER-{OperationId}] Metrics calculated: {CompletionRate:F1}% completion, {OnTimeRate:F1}% on-time",
                    operationId, metrics.CompletionRate, metrics.OnTimeDeliveryPercent);

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-{OperationId}] Error calculating metrics: {ErrorMessage}", operationId, ex.Message);
                return new MasterScheduleMetrics();
            }
        }

        public async Task<List<ScheduleAlert>> GetActiveAlertsAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [MASTER-{OperationId}] Loading active schedule alerts", operationId);

            try
            {
                var alerts = new List<ScheduleAlert>();

                // Check for delayed jobs
                var delayedJobs = await _context.Jobs
                    .Where(j => j.ActualStart.HasValue && j.ActualStart > j.ScheduledStart && 
                               !j.Status.Equals("completed", StringComparison.OrdinalIgnoreCase))
                    .AsNoTracking()
                    .ToListAsync();

                foreach (var job in delayedJobs)
                {
                    var delayHours = (job.ActualStart!.Value - job.ScheduledStart).TotalHours;
                    alerts.Add(new ScheduleAlert
                    {
                        Id = alerts.Count + 1,
                        Type = "delay",
                        Severity = delayHours > 24 ? "critical" : delayHours > 8 ? "high" : "medium",
                        Title = $"Job Delayed: {job.PartNumber}",
                        Description = $"Job on {job.MachineId} delayed by {delayHours:F1} hours",
                        MachineId = job.MachineId,
                        JobId = job.Id.ToString(),
                        CreatedDate = DateTime.UtcNow.AddHours(-delayHours)
                    });
                }

                // Check for machine conflicts
                var conflictingJobs = await DetectScheduleConflictsAsync();
                foreach (var conflict in conflictingJobs)
                {
                    alerts.Add(new ScheduleAlert
                    {
                        Id = alerts.Count + 1,
                        Type = "conflict",
                        Severity = "high",
                        Title = $"Schedule Conflict on {conflict.MachineId}",
                        Description = $"Overlapping jobs detected: {conflict.Description}",
                        MachineId = conflict.MachineId,
                        CreatedDate = DateTime.UtcNow
                    });
                }

                // Check for upcoming maintenance
                var machines = await _machineService.GetActiveMachinesAsync();
                foreach (var machine in machines.Where(m => m.NextMaintenanceDate.HasValue && 
                                                           m.NextMaintenanceDate <= DateTime.Today.AddDays(7)))
                {
                    alerts.Add(new ScheduleAlert
                    {
                        Id = alerts.Count + 1,
                        Type = "maintenance",
                        Severity = machine.NextMaintenanceDate <= DateTime.Today.AddDays(1) ? "critical" : "medium",
                        Title = $"Maintenance Due: {machine.MachineId}",
                        Description = $"Scheduled maintenance on {machine.NextMaintenanceDate:yyyy-MM-dd}",
                        MachineId = machine.MachineId,
                        CreatedDate = DateTime.UtcNow.AddDays(-1)
                    });
                }

                _logger.LogDebug("? [MASTER-{OperationId}] Found {AlertCount} active alerts", operationId, alerts.Count);
                return alerts.OrderByDescending(a => a.Severity).ThenByDescending(a => a.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-{OperationId}] Error loading alerts: {ErrorMessage}", operationId, ex.Message);
                return new List<ScheduleAlert>();
            }
        }

        public async Task<List<ResourceUtilization>> GetResourceUtilizationAsync(DateTime startDate, DateTime endDate)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [MASTER-{OperationId}] Calculating resource utilization", operationId);

            try
            {
                var utilization = new List<ResourceUtilization>();
                var machines = await _machineService.GetActiveMachinesAsync();

                foreach (var machine in machines)
                {
                    var machineUtilization = await CalculateMachineUtilizationAsync(machine.MachineId, startDate, endDate);
                    var dailyData = await GetDailyUtilizationAsync(machine.MachineId, startDate, endDate);

                    // Calculate capacity and used hours
                    var totalDays = (endDate - startDate).TotalDays;
                    var capacityHours = totalDays * 16.0; // 16 hours per day capacity
                    var usedHours = machineUtilization.ScheduledTime.TotalHours;

                    utilization.Add(new ResourceUtilization
                    {
                        ResourceId = machine.MachineId,
                        ResourceName = machine.Name ?? machine.MachineId,
                        ResourceType = "machine",
                        UtilizationPercent = machineUtilization.UtilizationPercent,
                        CapacityHours = capacityHours,
                        UsedHours = usedHours,
                        AvailableHours = capacityHours - usedHours,
                        DailyUtilization = dailyData
                    });
                }

                _logger.LogDebug("? [MASTER-{OperationId}] Calculated utilization for {ResourceCount} resources", 
                    operationId, utilization.Count);
                return utilization;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-{OperationId}] Error calculating utilization: {ErrorMessage}", 
                    operationId, ex.Message);
                return new List<ResourceUtilization>();
            }
        }

        public async Task<List<MasterScheduleTimeSlot>> GetTimelineDataAsync(DateTime startDate, DateTime endDate)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [MASTER-{OperationId}] Loading timeline data", operationId);

            try
            {
                var jobs = await _context.Jobs
                    .Where(j => j.ScheduledStart < endDate && j.ScheduledEnd > startDate)
                    .AsNoTracking()
                    .ToListAsync();

                var timeSlots = jobs.Select(job => new MasterScheduleTimeSlot
                {
                    StartTime = job.ScheduledStart,
                    EndTime = job.ScheduledEnd,
                    MachineId = job.MachineId,
                    JobId = job.Id.ToString(),
                    PartNumber = job.PartNumber,
                    Status = job.Status,
                    Priority = job.Priority.ToString(),
                    ProgressPercent = CalculateJobProgress(job)
                }).ToList();

                // Detect conflicts
                DetectTimeSlotConflicts(timeSlots);

                _logger.LogDebug("? [MASTER-{OperationId}] Loaded {SlotCount} timeline slots", operationId, timeSlots.Count);
                return timeSlots.OrderBy(t => t.StartTime).ThenBy(t => t.MachineId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-{OperationId}] Error loading timeline: {ErrorMessage}", operationId, ex.Message);
                return new List<MasterScheduleTimeSlot>();
            }
        }

        public async Task<byte[]> ExportScheduleAsync(MasterScheduleExportOptions options)
        {
            // Implementation for exporting schedule data
            // This would generate Excel, PDF, or CSV based on options
            await Task.CompletedTask;
            return Array.Empty<byte>();
        }

        public async Task<bool> ResolveAlertAsync(int alertId, string resolvedBy)
        {
            // Implementation for resolving alerts
            // This would mark alerts as resolved in a persistence layer
            await Task.CompletedTask;
            return true;
        }

        public async Task RefreshRealTimeDataAsync()
        {
            // Implementation for real-time data refresh
            // This could use SignalR to push updates to connected clients
            await Task.CompletedTask;
        }

        #region Helper Methods

        private async Task PopulateFilterOptionsAsync(MasterScheduleViewModel viewModel)
        {
            viewModel.Departments = viewModel.Jobs.Select(j => j.Department).Distinct().OrderBy(d => d).ToList();
            viewModel.Statuses = viewModel.Jobs.Select(j => j.Status).Distinct().OrderBy(s => s).ToList();
            viewModel.Priorities = viewModel.Jobs.Select(j => j.Priority).Distinct().OrderBy(p => p).ToList();
        }

        private string GetMachineName(string machineId)
        {
            // Simple mapping - in real implementation, this would query machine data
            return machineId switch
            {
                "TI1" => "Titanium SLS Machine 1",
                "TI2" => "Titanium SLS Machine 2", 
                "INC" => "Inconel SLS Machine",
                _ => machineId
            };
        }

        private string GetDepartmentFromMachine(string machineId)
        {
            return machineId.Substring(0, Math.Min(2, machineId.Length));
        }

        private double CalculateJobProgress(Job job)
        {
            if (job.Status.Equals("completed", StringComparison.OrdinalIgnoreCase)) return 100.0;
            if (!job.ActualStart.HasValue) return 0.0;
            
            var totalDuration = job.ScheduledEnd - job.ScheduledStart;
            var elapsed = DateTime.UtcNow - job.ActualStart.Value;
            
            return Math.Min(100.0, Math.Max(0.0, elapsed.TotalMinutes / totalDuration.TotalMinutes * 100));
        }

        private double CalculateOverallEfficiency(List<Job> jobs)
        {
            if (!jobs.Any()) return 0;
            
            var completedJobs = jobs.Where(j => j.ActualHours > 0).ToList();
            if (!completedJobs.Any()) return 0;
            
            var totalEstimated = completedJobs.Sum(j => j.EstimatedHours);
            var totalActual = completedJobs.Sum(j => j.ActualHours);
            
            return totalActual > 0 ? totalEstimated / totalActual * 100 : 0;
        }

        private async Task<double> CalculateOverallUtilizationAsync(DateTime startDate, DateTime endDate)
        {
            var machines = await _machineService.GetActiveMachinesAsync();
            if (!machines.Any()) return 0;
            
            var totalUtilization = 0.0;
            foreach (var machine in machines)
            {
                var utilization = await CalculateMachineUtilizationAsync(machine.MachineId, startDate, endDate);
                totalUtilization += utilization.UtilizationPercent;
            }
            
            return totalUtilization / machines.Count;
        }

        private async Task<double> CalculateQualityScoreAsync(DateTime startDate, DateTime endDate)
        {
            // Simplified quality score calculation
            var jobs = await _context.Jobs
                .Where(j => j.ScheduledStart >= startDate && j.ScheduledEnd <= endDate)
                .AsNoTracking()
                .ToListAsync();
            
            if (!jobs.Any()) return 100;
            
            var qualityJobs = jobs.Count(j => j.DensityPercentage >= 99.0); // High quality threshold
            return (double)qualityJobs / jobs.Count * 100;
        }

        private double CalculateAverageDelay(List<Job> jobs)
        {
            var delayedJobs = jobs.Where(j => j.ActualStart.HasValue && j.ActualStart > j.ScheduledStart).ToList();
            if (!delayedJobs.Any()) return 0;
            
            return delayedJobs.Average(j => (j.ActualStart!.Value - j.ScheduledStart).TotalHours);
        }

        private async Task<MasterScheduleMetrics> CalculatePreviousPeriodMetricsAsync(DateTime startDate, DateTime endDate)
        {
            // Simplified implementation - reuse existing calculation
            return await CalculateMetricsAsync(startDate, endDate);
        }

        private double CalculateTrend(double current, double previous)
        {
            if (previous == 0) return 0;
            return (current - previous) / previous * 100;
        }

        private async Task<(double UtilizationPercent, double EfficiencyPercent, TimeSpan ScheduledTime, TimeSpan ActualTime)> 
            CalculateMachineUtilizationAsync(string machineId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;
            
            var jobs = await _context.Jobs
                .Where(j => j.MachineId == machineId && j.ScheduledStart >= start && j.ScheduledEnd <= end)
                .AsNoTracking()
                .ToListAsync();
            
            var scheduledHours = jobs.Sum(j => j.EstimatedHours);
            var actualHours = jobs.Sum(j => j.ActualHours);
            
            var availableHours = (end - start).TotalHours * 0.8; // 80% available time (20% for maintenance/breaks)
            
            var utilizationPercent = availableHours > 0 ? scheduledHours / availableHours * 100 : 0;
            var efficiencyPercent = scheduledHours > 0 && actualHours > 0 ? scheduledHours / actualHours * 100 : 0;
            
            return (utilizationPercent, efficiencyPercent, TimeSpan.FromHours(scheduledHours), TimeSpan.FromHours(actualHours));
        }

        private async Task<(int Active, int Queued, int Completed)> GetMachineJobCountsAsync(string machineId)
        {
            var active = await _context.Jobs.CountAsync(j => j.MachineId == machineId && 
                (j.Status == "running" || j.Status == "building"));
            var queued = await _context.Jobs.CountAsync(j => j.MachineId == machineId && j.Status == "scheduled");
            var completed = await _context.Jobs.CountAsync(j => j.MachineId == machineId && j.Status == "completed");
            
            return (active, queued, completed);
        }

        private async Task<Job?> GetCurrentJobAsync(string machineId)
        {
            return await _context.Jobs
                .Where(j => j.MachineId == machineId && 
                           (j.Status == "running" || j.Status == "building") &&
                           j.ActualStart.HasValue && !j.ActualEnd.HasValue)
                .OrderByDescending(j => j.ActualStart)
                .FirstOrDefaultAsync();
        }

        private async Task<string> GetMachineStatusAsync(string machineId)
        {
            var currentJob = await GetCurrentJobAsync(machineId);
            if (currentJob != null) return "operational";
            
            var machines = await _machineService.GetActiveMachinesAsync();
            var machine = machines.FirstOrDefault(m => m.MachineId == machineId);
            
            return machine?.Status ?? "idle";
        }

        private async Task<List<UtilizationDataPoint>> GetDailyUtilizationAsync(string machineId, DateTime startDate, DateTime endDate)
        {
            var dataPoints = new List<UtilizationDataPoint>();
            
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var dayJobs = await _context.Jobs
                    .Where(j => j.MachineId == machineId && 
                               j.ScheduledStart.Date == date.Date)
                    .AsNoTracking()
                    .ToListAsync();
                
                var totalHours = dayJobs.Sum(j => j.EstimatedHours);
                var availableHours = 16.0; // 16-hour operating day
                
                dataPoints.Add(new UtilizationDataPoint
                {
                    Date = date,
                    UtilizationPercent = availableHours > 0 ? totalHours / availableHours * 100 : 0,
                    Hours = totalHours,
                    JobCount = dayJobs.Count
                });
            }
            
            return dataPoints;
        }

        private async Task<List<(string MachineId, string Description)>> DetectScheduleConflictsAsync()
        {
            var conflicts = new List<(string MachineId, string Description)>();
            var machines = await _machineService.GetActiveMachinesAsync();
            
            foreach (var machine in machines)
            {
                var jobs = await _context.Jobs
                    .Where(j => j.MachineId == machine.MachineId && 
                               j.Status != "completed" && j.Status != "cancelled")
                    .OrderBy(j => j.ScheduledStart)
                    .AsNoTracking()
                    .ToListAsync();
                
                for (int i = 0; i < jobs.Count - 1; i++)
                {
                    var currentJob = jobs[i];
                    var nextJob = jobs[i + 1];
                    
                    if (currentJob.ScheduledEnd > nextJob.ScheduledStart)
                    {
                        conflicts.Add((machine.MachineId, 
                            $"Jobs {currentJob.PartNumber} and {nextJob.PartNumber} overlap"));
                    }
                }
            }
            
            return conflicts;
        }

        private void DetectTimeSlotConflicts(List<MasterScheduleTimeSlot> timeSlots)
        {
            var machineGroups = timeSlots.GroupBy(t => t.MachineId);
            
            foreach (var group in machineGroups)
            {
                var slots = group.OrderBy(t => t.StartTime).ToList();
                
                for (int i = 0; i < slots.Count - 1; i++)
                {
                    var current = slots[i];
                    var next = slots[i + 1];
                    
                    if (current.EndTime > next.StartTime)
                    {
                        current.IsConflict = true;
                        next.IsConflict = true;
                    }
                }
            }
        }

        #endregion
    }
}