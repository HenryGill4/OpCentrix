using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Runtime
{
    public class SchedulerRuntimeService : ISchedulerRuntimeService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<SchedulerRuntimeService> _logger;

        public SchedulerRuntimeService(SchedulerContext context, ILogger<SchedulerRuntimeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string? Error, Job Job)> StartJobAsync(int jobId, StartJobRequest request, string userName, int? userId)
        {
            var opId = Guid.NewGuid().ToString("N").Substring(0, 8);
            try
            {
                var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
                if (job == null)
                    return (false, "Job not found", new Job());
                if (!string.Equals(job.Status, "Scheduled", StringComparison.OrdinalIgnoreCase))
                    return (false, $"Job not in Scheduled state (current: {job.Status})", job);
                if (request.ActualUnitsPlanned <= 0)
                    return (false, "Actual units must be > 0", job);
                // Basic start
                job.Status = "Building";
                job.ActualStart = DateTime.UtcNow;
                job.LastStatusChangeUtc = job.ActualStart;
                job.LastModifiedDate = DateTime.UtcNow;
                job.LastModifiedBy = userName;
                job.OperatorUserId = userId;
                job.ActualUnitsPlanned = request.ActualUnitsPlanned;
                job.PrototypeUnitsPlanned = request.PrototypeUnitsPlanned;
                if (request.StackLevel.HasValue)
                    job.StackLevel = request.StackLevel;
                if (request.PowderAddedKg.HasValue && request.PowderAddedKg.Value > 0)
                {
                    job.PowderAddedKg = request.PowderAddedKg.Value;
                    job.PowderMaterial = job.SlsMaterial;
                }
                // Snapshot planned stack duration if not set
                if (!job.PlannedStackDurationHours.HasValue)
                    job.PlannedStackDurationHours = job.EstimatedHours;
                // Plan end
                if (request.OverrideDurationHours.HasValue && request.OverrideDurationHours.Value > 0.25 && request.OverrideDurationHours.Value <= 500)
                {
                    job.PlannedStackDurationHours = request.OverrideDurationHours.Value;
                    job.PlannedEndUtc = job.ActualStart.Value.AddHours(request.OverrideDurationHours.Value);
                }
                else
                {
                    job.PlannedEndUtc = job.PlannedEndUtc ?? job.ScheduledEnd;
                }
                // Align scheduled end to planned end for UI coherence
                if (job.PlannedEndUtc.HasValue)
                    job.ScheduledEnd = job.PlannedEndUtc.Value;
                if (!string.IsNullOrWhiteSpace(request.Notes))
                {
                    job.Notes = string.IsNullOrWhiteSpace(job.Notes) ? request.Notes : job.Notes + "\n" + request.Notes;
                }
                await _context.SaveChangesAsync();
                // History row
                await AddHistoryAsync(job.Id, "Scheduled", job.Status, userId, "Job started");
                _logger.LogInformation("? [RUNTIME-{OpId}] Job {JobId} started by {User}", opId, jobId, userName);
                return (true, null, job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [RUNTIME-{OpId}] Error starting job {JobId}", opId, jobId);
                return (false, "Error starting job", new Job());
            }
        }

        public async Task<(bool Success, string? Error, Job Job)> CompleteJobAsync(int jobId, CompleteJobRequest request, string userName, int? userId)
        {
            var opId = Guid.NewGuid().ToString("N").Substring(0, 8);
            try
            {
                var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
                if (job == null)
                    return (false, "Job not found", new Job());
                if (!string.Equals(job.Status, "Building", StringComparison.OrdinalIgnoreCase))
                    return (false, $"Job not in Building state (current: {job.Status})", job);
                job.Status = "Completed";
                job.ActualEnd = DateTime.UtcNow;
                job.LastStatusChangeUtc = job.ActualEnd;
                job.LastModifiedDate = DateTime.UtcNow;
                job.LastModifiedBy = userName;
                if (request.ProducedQuantity.HasValue && request.ProducedQuantity.Value >= 0)
                    job.ProducedQuantity = request.ProducedQuantity.Value;
                if (request.DefectQuantity.HasValue && request.DefectQuantity.Value >= 0)
                    job.DefectQuantity = request.DefectQuantity.Value;
                if (!string.IsNullOrWhiteSpace(request.Notes))
                {
                    job.Notes = string.IsNullOrWhiteSpace(job.Notes) ? request.Notes : job.Notes + "\n" + request.Notes;
                }
                await _context.SaveChangesAsync();
                await AddHistoryAsync(job.Id, "Building", job.Status, userId, "Job completed");
                _logger.LogInformation("? [RUNTIME-{OpId}] Job {JobId} completed by {User}", opId, jobId, userName);
                return (true, null, job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [RUNTIME-{OpId}] Error completing job {JobId}", opId, jobId);
                return (false, "Error completing job", new Job());
            }
        }

        private async Task AddHistoryAsync(int jobId, string oldStatus, string newStatus, int? userId, string? notes)
        {
            try
            {
                var history = new JobStatusHistory
                {
                    JobId = jobId,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    ChangedByUserId = userId,
                    ChangedUtc = DateTime.UtcNow,
                    Notes = notes
                };
                _context.Set<JobStatusHistory>().Add(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "?? [RUNTIME] Failed to add history for Job {JobId}", jobId);
            }
        }
    }
}
