using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpCentrix.Services
{
    /// <summary>
    /// Service for managing print job logs and analytics
    /// </summary>
    public interface IPrintJobLogService
    {
        Task<PrintJobLogViewModel> GetPrintJobLogAsync(
            string? searchTerm = null,
            string? statusFilter = null,
            string? machineFilter = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 50);
            
        Task<PrintJobLogEntry?> GetJobLogEntryAsync(int jobId);
        Task<List<PrintJobLogEntry>> GetLogEntriesForPartAsync(int partId);
        Task<PrintJobStatistics> GetPrintJobStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<RecentActivity>> GetRecentActivitiesAsync(int count = 10);
    }

    /// <summary>
    /// Implementation of print job log service
    /// </summary>
    public class PrintJobLogService : IPrintJobLogService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PrintJobLogService> _logger;

        public PrintJobLogService(SchedulerContext context, ILogger<PrintJobLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PrintJobLogViewModel> GetPrintJobLogAsync(
            string? searchTerm = null,
            string? statusFilter = null,
            string? machineFilter = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 50)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PRINT-LOG-{OperationId}] Loading print job log with filters", operationId);

            try
            {
                // Build query for print job entries
                var query = await BuildPrintJobLogQueryAsync();

                // Apply filters
                query = ApplyFilters(query, searchTerm, statusFilter, machineFilter, startDate, endDate);

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination and get results
                var logEntries = await query
                    .OrderByDescending(e => e.ActualStart ?? e.ScheduledStart ?? e.JobCreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Get filter options
                var availableStatuses = await GetAvailableStatusesAsync();
                var availableMachines = await GetAvailableMachinesAsync();

                // Get statistics
                var statistics = await GetPrintJobStatisticsAsync(startDate, endDate);

                var viewModel = new PrintJobLogViewModel
                {
                    LogEntries = logEntries,
                    Statistics = statistics,
                    SearchTerm = searchTerm,
                    StatusFilter = statusFilter,
                    MachineFilter = machineFilter,
                    StartDate = startDate,
                    EndDate = endDate,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    AvailableStatuses = availableStatuses,
                    AvailableMachines = availableMachines
                };

                _logger.LogInformation("? [PRINT-LOG-{OperationId}] Print job log loaded: {EntryCount} entries", 
                    operationId, logEntries.Count);

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PRINT-LOG-{OperationId}] Error loading print job log", operationId);
                throw;
            }
        }

        public async Task<PrintJobLogEntry?> GetJobLogEntryAsync(int jobId)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [PRINT-LOG-{OperationId}] Loading log entry for job {JobId}", operationId, jobId);

            try
            {
                var query = await BuildPrintJobLogQueryAsync();
                var entry = await query.FirstOrDefaultAsync(e => e.ScheduledJobId == jobId);

                _logger.LogDebug("? [PRINT-LOG-{OperationId}] Log entry loaded for job {JobId}", operationId, jobId);
                return entry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PRINT-LOG-{OperationId}] Error loading log entry for job {JobId}", operationId, jobId);
                throw;
            }
        }

        public async Task<List<PrintJobLogEntry>> GetLogEntriesForPartAsync(int partId)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [PRINT-LOG-{OperationId}] Loading log entries for part {PartId}", operationId, partId);

            try
            {
                var query = await BuildPrintJobLogQueryAsync();
                var entries = await query
                    .Where(e => e.PartId == partId)
                    .OrderByDescending(e => e.ActualStart ?? e.ScheduledStart ?? e.JobCreatedDate)
                    .ToListAsync();

                _logger.LogDebug("? [PRINT-LOG-{OperationId}] Found {EntryCount} log entries for part {PartId}", 
                    operationId, entries.Count, partId);

                return entries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PRINT-LOG-{OperationId}] Error loading log entries for part {PartId}", operationId, partId);
                throw;
            }
        }

        public async Task<PrintJobStatistics> GetPrintJobStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [PRINT-LOG-{OperationId}] Calculating statistics", operationId);

            try
            {
                // Set default date range if not provided
                endDate ??= DateTime.UtcNow;
                startDate ??= endDate.Value.AddDays(-30);

                var query = await BuildPrintJobLogQueryAsync();
                
                // Filter by date range
                query = query.Where(e => 
                    (e.ActualStart ?? e.ScheduledStart ?? e.JobCreatedDate) >= startDate &&
                    (e.ActualStart ?? e.ScheduledStart ?? e.JobCreatedDate) <= endDate);

                var allEntries = await query.ToListAsync();

                var statistics = new PrintJobStatistics
                {
                    TotalJobs = allEntries.Count,
                    CompletedJobs = allEntries.Count(e => e.BuildJobStatus == "Completed"),
                    InProgressJobs = allEntries.Count(e => e.BuildJobStatus == "In Progress"),
                    ScheduledJobs = allEntries.Count(e => string.IsNullOrEmpty(e.BuildJobStatus) && e.JobStatus == "Scheduled"),
                    CancelledJobs = allEntries.Count(e => e.JobStatus == "Cancelled" || e.BuildJobStatus == "Aborted"),
                    
                    TotalEstimatedHours = allEntries.Sum(e => e.EstimatedHours),
                    TotalActualHours = allEntries.Sum(e => e.ActualHours),
                    AverageJobDuration = allEntries.Where(e => e.ActualHours > 0).Average(e => e.ActualHours),
                    
                    JobsByMachine = allEntries
                        .Where(e => !string.IsNullOrEmpty(e.MachineId))
                        .GroupBy(e => e.MachineId)
                        .ToDictionary(g => g.Key, g => g.Count()),
                        
                    JobsByMaterial = allEntries
                        .Where(e => !string.IsNullOrEmpty(e.SlsMaterial))
                        .GroupBy(e => e.SlsMaterial)
                        .ToDictionary(g => g.Key, g => g.Count()),
                        
                    EfficiencyByMachine = allEntries
                        .Where(e => !string.IsNullOrEmpty(e.MachineId) && e.EstimatedHours > 0 && e.ActualHours > 0)
                        .GroupBy(e => e.MachineId)
                        .ToDictionary(g => g.Key, g => g.Average(e => e.EfficiencyPercentage)),
                        
                    PartsProduced = allEntries.Count(e => e.BuildJobStatus == "Completed"),
                    RecentActivities = await GetRecentActivitiesAsync(10)
                };

                // Calculate overall efficiency
                if (statistics.TotalEstimatedHours > 0 && statistics.TotalActualHours > 0)
                {
                    statistics.OverallEfficiency = Math.Round((statistics.TotalEstimatedHours / statistics.TotalActualHours) * 100, 1);
                }

                _logger.LogDebug("? [PRINT-LOG-{OperationId}] Statistics calculated: {TotalJobs} jobs, {CompletedJobs} completed", 
                    operationId, statistics.TotalJobs, statistics.CompletedJobs);

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PRINT-LOG-{OperationId}] Error calculating statistics", operationId);
                throw;
            }
        }

        public async Task<List<RecentActivity>> GetRecentActivitiesAsync(int count = 10)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [PRINT-LOG-{OperationId}] Loading recent activities", operationId);

            try
            {
                var activities = new List<RecentActivity>();

                // Get recent job log entries
                var recentJobLogs = await _context.JobLogEntries
                    .OrderByDescending(jle => jle.Timestamp)
                    .Take(count)
                    .ToListAsync();

                foreach (var log in recentJobLogs)
                {
                    activities.Add(new RecentActivity
                    {
                        Timestamp = log.Timestamp,
                        Activity = log.Action,
                        PartNumber = log.PartNumber ?? "Unknown",
                        MachineId = log.MachineId,
                        Operator = log.Operator ?? "Unknown",
                        ActivityType = log.Action
                    });
                }

                _logger.LogDebug("? [PRINT-LOG-{OperationId}] Loaded {ActivityCount} recent activities", 
                    operationId, activities.Count);

                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PRINT-LOG-{OperationId}] Error loading recent activities", operationId);
                return new List<RecentActivity>();
            }
        }

        private async Task<IQueryable<PrintJobLogEntry>> BuildPrintJobLogQueryAsync()
        {
            // Build a comprehensive query that joins Parts, Jobs, and BuildJobs
            var query = from part in _context.Parts
                       join job in _context.Jobs on part.Id equals job.PartId into jobGroup
                       from job in jobGroup.DefaultIfEmpty()
                       join buildJob in _context.BuildJobs on job!.Id equals buildJob.AssociatedScheduledJobId into buildJobGroup
                       from buildJob in buildJobGroup.DefaultIfEmpty()
                       select new PrintJobLogEntry
                       {
                           // Part Information
                           PartId = part.Id,
                           PartNumber = part.PartNumber,
                           PartName = part.Name,
                           Material = part.Material,
                           SlsMaterial = part.SlsMaterial,
                           
                           // Job Information
                           ScheduledJobId = job != null ? job.Id : (int?)null,
                           ScheduledStart = job != null ? job.ScheduledStart : (DateTime?)null,
                           ScheduledEnd = job != null ? job.ScheduledEnd : (DateTime?)null,
                           EstimatedHours = job != null ? job.EstimatedHours : part.EstimatedHours,
                           Priority = job != null ? job.Priority : 3,
                           JobStatus = job != null ? job.Status : "",
                           JobCreatedBy = job != null ? job.CreatedBy : "",
                           JobCreatedDate = job != null ? job.CreatedDate : (DateTime?)null,
                           
                           // BuildJob Information (using correct property names)
                           BuildJobId = buildJob != null ? buildJob.BuildId : (int?)null,
                           ActualStart = buildJob != null ? buildJob.ActualStartTime : (DateTime?)null,
                           ActualEnd = buildJob != null ? buildJob.ActualEndTime : (DateTime?)null,
                           BuildJobStatus = buildJob != null ? buildJob.Status : "",
                           Operator = buildJob != null ? (buildJob.User != null ? buildJob.User.Username : "Unknown") : "",
                           MachineId = buildJob != null ? buildJob.PrinterName : (job != null ? job.MachineId : "")
                       };

            return query;
        }

        private IQueryable<PrintJobLogEntry> ApplyFilters(
            IQueryable<PrintJobLogEntry> query,
            string? searchTerm,
            string? statusFilter,
            string? machineFilter,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(e => 
                    e.PartNumber.ToLower().Contains(searchLower) ||
                    e.PartName.ToLower().Contains(searchLower) ||
                    e.Operator.ToLower().Contains(searchLower));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(e => e.BuildJobStatus == statusFilter || e.JobStatus == statusFilter);
            }

            if (!string.IsNullOrEmpty(machineFilter))
            {
                query = query.Where(e => e.MachineId == machineFilter);
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => 
                    (e.ActualStart ?? e.ScheduledStart ?? e.JobCreatedDate) >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => 
                    (e.ActualStart ?? e.ScheduledStart ?? e.JobCreatedDate) <= endDate.Value);
            }

            return query;
        }

        private async Task<List<string>> GetAvailableStatusesAsync()
        {
            var statuses = new List<string>();

            var jobStatuses = await _context.Jobs
                .Where(j => !string.IsNullOrEmpty(j.Status))
                .Select(j => j.Status)
                .Distinct()
                .ToListAsync();

            var buildJobStatuses = await _context.BuildJobs
                .Where(bj => !string.IsNullOrEmpty(bj.Status))
                .Select(bj => bj.Status)
                .Distinct()
                .ToListAsync();

            statuses.AddRange(jobStatuses);
            statuses.AddRange(buildJobStatuses);

            return statuses.Distinct().OrderBy(s => s).ToList();
        }

        private async Task<List<string>> GetAvailableMachinesAsync()
        {
            var machines = new List<string>();

            var jobMachines = await _context.Jobs
                .Where(j => !string.IsNullOrEmpty(j.MachineId))
                .Select(j => j.MachineId)
                .Distinct()
                .ToListAsync();

            var buildJobMachines = await _context.BuildJobs
                .Where(bj => !string.IsNullOrEmpty(bj.PrinterName))
                .Select(bj => bj.PrinterName)
                .Distinct()
                .ToListAsync();

            machines.AddRange(jobMachines);
            machines.AddRange(buildJobMachines);

            return machines.Distinct().OrderBy(m => m).ToList();
        }
    }
}