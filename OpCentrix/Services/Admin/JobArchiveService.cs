using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for managing job archival and cleanup operations
/// Task 15: Job Archive & Cleanup - Complete data management system
/// </summary>
public interface IJobArchiveService
{
    Task<List<ArchivedJob>> GetArchivedJobsAsync();
    Task<List<ArchivedJob>> SearchArchivedJobsAsync(string searchTerm);
    Task<List<ArchivedJob>> GetArchivedJobsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<ArchivedJob>> GetArchivedJobsByMachineAsync(string machineId);
    Task<List<ArchivedJob>> GetArchivedJobsByStatusAsync(string status);
    Task<ArchivedJob?> GetArchivedJobByIdAsync(int id);
    Task<ArchivedJob?> GetArchivedJobByOriginalIdAsync(int originalJobId);
    Task<List<Job>> GetEligibleJobsForArchivalAsync(int olderThanDays = 30);
    Task<Dictionary<string, object>> GetArchiveStatisticsAsync();
    Task<int> ArchiveJobAsync(int jobId, string archivedBy, string reason = "Manual Archive");
    Task<int> BulkArchiveJobsAsync(List<int> jobIds, string archivedBy, string reason = "Bulk Archive");
    Task<int> AutoArchiveOldJobsAsync(int olderThanDays = 30, string reason = "Auto Cleanup");
    Task<bool> RestoreArchivedJobAsync(int archivedJobId, string restoredBy);
    Task<bool> DeleteArchivedJobAsync(int archivedJobId);
    Task<int> CleanupOldArchivedJobsAsync(int olderThanDays = 365);
    Task<Dictionary<string, int>> GetCleanupRecommendationsAsync();
    Task<List<string>> GetArchiveReasonsAsync();
    Task<List<string>> GetArchiveMachinesAsync();
    Task<bool> ValidateArchiveOperationAsync(int jobId);
}

public class JobArchiveService : IJobArchiveService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<JobArchiveService> _logger;

    public JobArchiveService(SchedulerContext context, ILogger<JobArchiveService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ArchivedJob>> GetArchivedJobsAsync()
    {
        try
        {
            return await _context.ArchivedJobs
                .OrderByDescending(aj => aj.ArchivedDate)
                .Take(100) // Limit for performance
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving archived jobs");
            return new List<ArchivedJob>();
        }
    }

    public async Task<List<ArchivedJob>> SearchArchivedJobsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetArchivedJobsAsync();
            }

            var lowerSearchTerm = searchTerm.ToLower();

            return await _context.ArchivedJobs
                .Where(aj => 
                    aj.PartNumber.ToLower().Contains(lowerSearchTerm) ||
                    aj.PartDescription.ToLower().Contains(lowerSearchTerm) ||
                    aj.MachineId.ToLower().Contains(lowerSearchTerm) ||
                    aj.Operator.ToLower().Contains(lowerSearchTerm) ||
                    aj.CustomerOrderNumber.ToLower().Contains(lowerSearchTerm) ||
                    aj.Notes.ToLower().Contains(lowerSearchTerm) ||
                    aj.ArchiveReason.ToLower().Contains(lowerSearchTerm))
                .OrderByDescending(aj => aj.ArchivedDate)
                .Take(100)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching archived jobs with term: {SearchTerm}", searchTerm);
            return new List<ArchivedJob>();
        }
    }

    public async Task<List<ArchivedJob>> GetArchivedJobsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _context.ArchivedJobs
                .Where(aj => aj.ArchivedDate >= startDate && aj.ArchivedDate <= endDate)
                .OrderByDescending(aj => aj.ArchivedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving archived jobs by date range: {StartDate} - {EndDate}", startDate, endDate);
            return new List<ArchivedJob>();
        }
    }

    public async Task<List<ArchivedJob>> GetArchivedJobsByMachineAsync(string machineId)
    {
        try
        {
            return await _context.ArchivedJobs
                .Where(aj => aj.MachineId == machineId)
                .OrderByDescending(aj => aj.ArchivedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving archived jobs for machine: {MachineId}", machineId);
            return new List<ArchivedJob>();
        }
    }

    public async Task<List<ArchivedJob>> GetArchivedJobsByStatusAsync(string status)
    {
        try
        {
            return await _context.ArchivedJobs
                .Where(aj => aj.Status == status)
                .OrderByDescending(aj => aj.ArchivedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving archived jobs by status: {Status}", status);
            return new List<ArchivedJob>();
        }
    }

    public async Task<ArchivedJob?> GetArchivedJobByIdAsync(int id)
    {
        try
        {
            return await _context.ArchivedJobs
                .FirstOrDefaultAsync(aj => aj.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving archived job with ID: {Id}", id);
            return null;
        }
    }

    public async Task<ArchivedJob?> GetArchivedJobByOriginalIdAsync(int originalJobId)
    {
        try
        {
            return await _context.ArchivedJobs
                .FirstOrDefaultAsync(aj => aj.OriginalJobId == originalJobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving archived job with original ID: {OriginalJobId}", originalJobId);
            return null;
        }
    }

    public async Task<List<Job>> GetEligibleJobsForArchivalAsync(int olderThanDays = 30)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
            
            return await _context.Jobs
                .Where(j => 
                    (j.Status == "Completed" || j.Status == "Cancelled") &&
                    j.LastModifiedDate < cutoffDate)
                .Include(j => j.Part)
                .OrderBy(j => j.LastModifiedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving eligible jobs for archival (older than {Days} days)", olderThanDays);
            return new List<Job>();
        }
    }

    public async Task<Dictionary<string, object>> GetArchiveStatisticsAsync()
    {
        try
        {
            var stats = new Dictionary<string, object>();

            // Basic counts
            stats["TotalArchivedJobs"] = await _context.ArchivedJobs.CountAsync();
            stats["EligibleForArchival"] = await _context.Jobs
                .CountAsync(j => (j.Status == "Completed" || j.Status == "Cancelled") && 
                                j.LastModifiedDate < DateTime.UtcNow.AddDays(-30));

            // Archive by month (last 12 months)
            var monthlyData = await _context.ArchivedJobs
                .Where(aj => aj.ArchivedDate >= DateTime.UtcNow.AddMonths(-12))
                .GroupBy(aj => new { aj.ArchivedDate.Year, aj.ArchivedDate.Month })
                .Select(g => new { 
                    Year = g.Key.Year, 
                    Month = g.Key.Month, 
                    Count = g.Count() 
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToListAsync();

            stats["MonthlyArchiveData"] = monthlyData;

            // Archive by machine
            var machineData = await _context.ArchivedJobs
                .GroupBy(aj => aj.MachineId)
                .Select(g => new { 
                    MachineId = g.Key, 
                    Count = g.Count(),
                    LatestArchive = g.Max(aj => aj.ArchivedDate)
                })
                .OrderByDescending(g => g.Count)
                .Take(10)
                .ToListAsync();

            stats["MachineArchiveData"] = machineData;

            // Archive by status
            var statusData = await _context.ArchivedJobs
                .GroupBy(aj => aj.Status)
                .Select(g => new { 
                    Status = g.Key, 
                    Count = g.Count() 
                })
                .OrderByDescending(g => g.Count)
                .ToListAsync();

            stats["StatusArchiveData"] = statusData;

            // Archive by reason
            var reasonData = await _context.ArchivedJobs
                .GroupBy(aj => aj.ArchiveReason)
                .Select(g => new { 
                    Reason = g.Key, 
                    Count = g.Count() 
                })
                .OrderByDescending(g => g.Count)
                .ToListAsync();

            stats["ReasonArchiveData"] = reasonData;

            // Database size estimates
            var activeJobCount = await _context.Jobs.CountAsync();
            var archivedJobCount = await _context.ArchivedJobs.CountAsync();
            
            stats["ActiveJobCount"] = activeJobCount;
            stats["ArchivedJobCount"] = archivedJobCount;
            stats["ArchiveRatio"] = activeJobCount > 0 ? Math.Round((double)archivedJobCount / activeJobCount, 2) : 0;

            // Recent activity
            var recentArchives = await _context.ArchivedJobs
                .Where(aj => aj.ArchivedDate >= DateTime.UtcNow.AddDays(-7))
                .CountAsync();

            stats["RecentArchivesCount"] = recentArchives;

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving archive statistics");
            return new Dictionary<string, object>();
        }
    }

    public async Task<int> ArchiveJobAsync(int jobId, string archivedBy, string reason = "Manual Archive")
    {
        try
        {
            var job = await _context.Jobs
                .Include(j => j.Part)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
            {
                _logger.LogWarning("Cannot archive job: Job with ID {JobId} not found", jobId);
                return 0;
            }

            // Check if job can be archived
            if (!await ValidateArchiveOperationAsync(jobId))
            {
                _logger.LogWarning("Job {JobId} is not eligible for archival", jobId);
                return 0;
            }

            // Check if already archived
            var existingArchive = await GetArchivedJobByOriginalIdAsync(jobId);
            if (existingArchive != null)
            {
                _logger.LogWarning("Job {JobId} is already archived", jobId);
                return 0;
            }

            // Create archived job
            var archivedJob = ArchivedJob.FromJob(job, archivedBy, reason);
            _context.ArchivedJobs.Add(archivedJob);

            // Remove original job
            _context.Jobs.Remove(job);

            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                _logger.LogInformation("Job {JobId} archived successfully by {ArchivedBy} - Reason: {Reason}", 
                    jobId, archivedBy, reason);
                return 1;
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving job {JobId}", jobId);
            return 0;
        }
    }

    public async Task<int> BulkArchiveJobsAsync(List<int> jobIds, string archivedBy, string reason = "Bulk Archive")
    {
        try
        {
            var archivedCount = 0;

            foreach (var jobId in jobIds)
            {
                var result = await ArchiveJobAsync(jobId, archivedBy, reason);
                if (result > 0)
                {
                    archivedCount++;
                }
            }

            _logger.LogInformation("Bulk archive completed: {ArchivedCount}/{TotalCount} jobs archived by {ArchivedBy}", 
                archivedCount, jobIds.Count, archivedBy);

            return archivedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk archive operation");
            return 0;
        }
    }

    public async Task<int> AutoArchiveOldJobsAsync(int olderThanDays = 30, string reason = "Auto Cleanup")
    {
        try
        {
            var eligibleJobs = await GetEligibleJobsForArchivalAsync(olderThanDays);
            var jobIds = eligibleJobs.Select(j => j.Id).ToList();

            if (!jobIds.Any())
            {
                _logger.LogInformation("No jobs eligible for auto-archival (older than {Days} days)", olderThanDays);
                return 0;
            }

            var archivedCount = await BulkArchiveJobsAsync(jobIds, "System", reason);

            _logger.LogInformation("Auto-archive completed: {ArchivedCount} jobs archived (older than {Days} days)", 
                archivedCount, olderThanDays);

            return archivedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during auto-archive operation");
            return 0;
        }
    }

    public async Task<bool> RestoreArchivedJobAsync(int archivedJobId, string restoredBy)
    {
        try
        {
            var archivedJob = await GetArchivedJobByIdAsync(archivedJobId);
            if (archivedJob == null)
            {
                _logger.LogWarning("Cannot restore: Archived job with ID {ArchivedJobId} not found", archivedJobId);
                return false;
            }

            // Check if original job already exists
            var existingJob = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == archivedJob.OriginalJobId);
            if (existingJob != null)
            {
                _logger.LogWarning("Cannot restore: Job with original ID {OriginalJobId} already exists", archivedJob.OriginalJobId);
                return false;
            }

            // Create new job from archived data
            var restoredJob = new Job
            {
                MachineId = archivedJob.MachineId,
                PartId = archivedJob.PartId,
                PartNumber = archivedJob.PartNumber,
                ScheduledStart = archivedJob.ScheduledStart,
                ScheduledEnd = archivedJob.ScheduledEnd,
                ActualStart = archivedJob.ActualStart,
                ActualEnd = archivedJob.ActualEnd,
                Quantity = archivedJob.Quantity,
                ProducedQuantity = archivedJob.ProducedQuantity,
                DefectQuantity = archivedJob.DefectQuantity,
                ReworkQuantity = archivedJob.ReworkQuantity,
                Status = "Restored",
                Priority = archivedJob.Priority,
                EstimatedHours = archivedJob.EstimatedHours,
                SlsMaterial = archivedJob.SlsMaterial,
                PowderLotNumber = archivedJob.PowderLotNumber,
                LaserPowerWatts = archivedJob.LaserPowerWatts,
                ScanSpeedMmPerSec = archivedJob.ScanSpeedMmPerSec,
                LayerThicknessMicrons = archivedJob.LayerThicknessMicrons,
                EstimatedPowderUsageKg = archivedJob.EstimatedPowderUsageKg,
                ActualPowderUsageKg = archivedJob.ActualPowderUsageKg,
                LaborCostPerHour = archivedJob.LaborCostPerHour,
                MaterialCostPerKg = archivedJob.MaterialCostPerKg,
                MachineOperatingCostPerHour = archivedJob.MachineOperatingCostPerHour,
                ArgonCostPerHour = archivedJob.ArgonCostPerHour,
                DensityPercentage = archivedJob.DensityPercentage,
                SurfaceRoughnessRa = archivedJob.SurfaceRoughnessRa,
                Operator = archivedJob.Operator,
                QualityInspector = archivedJob.QualityInspector,
                Supervisor = archivedJob.Supervisor,
                CustomerOrderNumber = archivedJob.CustomerOrderNumber,
                CustomerDueDate = archivedJob.CustomerDueDate,
                Notes = $"{archivedJob.Notes}\n\nRestored from archive on {DateTime.UtcNow:yyyy-MM-dd HH:mm} by {restoredBy}",
                HoldReason = archivedJob.HoldReason,
                ProcessParameters = archivedJob.ProcessParameters,
                QualityCheckpoints = archivedJob.QualityCheckpoints,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = restoredBy,
                LastModifiedBy = restoredBy
            };

            _context.Jobs.Add(restoredJob);

            // Remove archived job
            _context.ArchivedJobs.Remove(archivedJob);

            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                _logger.LogInformation("Archived job {ArchivedJobId} restored successfully by {RestoredBy}", 
                    archivedJobId, restoredBy);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring archived job {ArchivedJobId}", archivedJobId);
            return false;
        }
    }

    public async Task<bool> DeleteArchivedJobAsync(int archivedJobId)
    {
        try
        {
            var archivedJob = await GetArchivedJobByIdAsync(archivedJobId);
            if (archivedJob == null)
            {
                _logger.LogWarning("Cannot delete: Archived job with ID {ArchivedJobId} not found", archivedJobId);
                return false;
            }

            _context.ArchivedJobs.Remove(archivedJob);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                _logger.LogInformation("Archived job {ArchivedJobId} deleted successfully", archivedJobId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting archived job {ArchivedJobId}", archivedJobId);
            return false;
        }
    }

    public async Task<int> CleanupOldArchivedJobsAsync(int olderThanDays = 365)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
            
            var oldArchivedJobs = await _context.ArchivedJobs
                .Where(aj => aj.ArchivedDate < cutoffDate)
                .ToListAsync();

            if (!oldArchivedJobs.Any())
            {
                _logger.LogInformation("No archived jobs older than {Days} days found for cleanup", olderThanDays);
                return 0;
            }

            _context.ArchivedJobs.RemoveRange(oldArchivedJobs);
            var result = await _context.SaveChangesAsync();

            _logger.LogInformation("Cleanup completed: {Count} archived jobs older than {Days} days deleted", 
                oldArchivedJobs.Count, olderThanDays);

            return oldArchivedJobs.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during archived jobs cleanup");
            return 0;
        }
    }

    public async Task<Dictionary<string, int>> GetCleanupRecommendationsAsync()
    {
        try
        {
            var recommendations = new Dictionary<string, int>();

            // Jobs ready for archival
            var eligibleFor30Days = await _context.Jobs
                .CountAsync(j => (j.Status == "Completed" || j.Status == "Cancelled") && 
                                j.LastModifiedDate < DateTime.UtcNow.AddDays(-30));

            var eligibleFor60Days = await _context.Jobs
                .CountAsync(j => (j.Status == "Completed" || j.Status == "Cancelled") && 
                                j.LastModifiedDate < DateTime.UtcNow.AddDays(-60));

            var eligibleFor90Days = await _context.Jobs
                .CountAsync(j => (j.Status == "Completed" || j.Status == "Cancelled") && 
                                j.LastModifiedDate < DateTime.UtcNow.AddDays(-90));

            // Archived jobs ready for cleanup
            var archiveCleanup1Year = await _context.ArchivedJobs
                .CountAsync(aj => aj.ArchivedDate < DateTime.UtcNow.AddDays(-365));

            var archiveCleanup2Years = await _context.ArchivedJobs
                .CountAsync(aj => aj.ArchivedDate < DateTime.UtcNow.AddDays(-730));

            recommendations["EligibleForArchive30Days"] = eligibleFor30Days;
            recommendations["EligibleForArchive60Days"] = eligibleFor60Days;
            recommendations["EligibleForArchive90Days"] = eligibleFor90Days;
            recommendations["ArchiveCleanup1Year"] = archiveCleanup1Year;
            recommendations["ArchiveCleanup2Years"] = archiveCleanup2Years;

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cleanup recommendations");
            return new Dictionary<string, int>();
        }
    }

    public async Task<List<string>> GetArchiveReasonsAsync()
    {
        try
        {
            return await _context.ArchivedJobs
                .Select(aj => aj.ArchiveReason)
                .Distinct()
                .OrderBy(reason => reason)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving archive reasons");
            return new List<string>();
        }
    }

    public async Task<List<string>> GetArchiveMachinesAsync()
    {
        try
        {
            return await _context.ArchivedJobs
                .Select(aj => aj.MachineId)
                .Distinct()
                .OrderBy(machineId => machineId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving archive machines");
            return new List<string>();
        }
    }

    public async Task<bool> ValidateArchiveOperationAsync(int jobId)
    {
        try
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job == null)
            {
                return false;
            }

            // Only allow archiving of completed or cancelled jobs
            if (job.Status != "Completed" && job.Status != "Cancelled")
            {
                return false;
            }

            // Ensure job is not too recent (safety check)
            if (job.LastModifiedDate > DateTime.UtcNow.AddDays(-1))
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating archive operation for job {JobId}", jobId);
            return false;
        }
    }
}