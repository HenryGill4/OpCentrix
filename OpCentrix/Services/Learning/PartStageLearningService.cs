using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.JobStaging;

namespace OpCentrix.Services.Learning;

/// <summary>
/// Service that learns and refines stage duration estimates from actual completions.
/// Uses Exponential Moving Average (EMA) for stable, responsive estimates.
/// </summary>
public class PartStageLearningService : IPartStageLearningService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<PartStageLearningService> _logger;

    /// <summary>
    /// EMA smoothing factor (? = 0.3 means 30% weight to new observation)
    /// Higher values = more responsive to recent data, but more volatile
    /// Lower values = more stable, but slower to adapt
    /// </summary>
    private const double EmaAlpha = 0.3;

    /// <summary>
    /// Minimum sample count before we trust auto-estimates enough to update EstimatedHours
    /// </summary>
    private const int MinSampleCountForAutoUpdate = 3;

    public PartStageLearningService(SchedulerContext context, ILogger<PartStageLearningService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task RecordCompletionAsync(int jobStageId, CancellationToken ct = default)
    {
        try
        {
            // Load the JobStage with its Job and Job's Part
            var jobStage = await _context.JobStages
                .Include(js => js.Job)
                    .ThenInclude(j => j.Part)
                .FirstOrDefaultAsync(js => js.Id == jobStageId, ct);

            if (jobStage == null)
            {
                _logger.LogWarning("[LEARNING] JobStage {JobStageId} not found for learning", jobStageId);
                return;
            }

            if (jobStage.Job?.Part == null)
            {
                _logger.LogWarning("[LEARNING] JobStage {JobStageId} has no associated Part", jobStageId);
                return;
            }

            // Calculate actual duration
            if (!jobStage.ActualStart.HasValue || !jobStage.ActualEnd.HasValue)
            {
                _logger.LogWarning("[LEARNING] JobStage {JobStageId} missing ActualStart or ActualEnd", jobStageId);
                return;
            }

            var actualDuration = jobStage.ActualEnd.Value - jobStage.ActualStart.Value;
            var actualHours = actualDuration.TotalHours;

            if (actualHours <= 0)
            {
                _logger.LogWarning("[LEARNING] JobStage {JobStageId} has invalid duration: {Hours}h", jobStageId, actualHours);
                return;
            }

            // Record in JobStageHistory if not already recorded
            var existingHistory = await _context.JobStageHistories
                .FirstOrDefaultAsync(h => h.JobId == jobStage.JobId && 
                                          h.StageName == jobStage.StageType &&
                                          h.Action == "StageCompleted" &&
                                          h.Timestamp > DateTime.UtcNow.AddMinutes(-5), ct);

            if (existingHistory == null)
            {
                var history = new JobStageHistory
                {
                    JobId = jobStage.JobId,
                    ProductionStageId = null, // We'll try to resolve this
                    Action = "StageCompleted",
                    StageName = jobStage.StageType,
                    Operator = jobStage.AssignedOperator ?? "System",
                    Timestamp = jobStage.ActualEnd.Value,
                    StageHours = actualHours,
                    MachineId = jobStage.MachineId,
                    QualityResult = "Pass", // Default to pass unless specified
                    Notes = $"Auto-recorded by learning service. Duration: {actualHours:F2}h"
                };

                // Try to find the ProductionStage by name match
                var productionStage = await _context.ProductionStages
                    .FirstOrDefaultAsync(ps => ps.Name.Contains(jobStage.StageType) || 
                                               jobStage.StageType.Contains(ps.Name), ct);
                if (productionStage != null)
                {
                    history.ProductionStageId = productionStage.Id;
                }

                _context.JobStageHistories.Add(history);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("[LEARNING] Recorded completion for JobStage {JobStageId}: {Hours:F2}h", 
                    jobStageId, actualHours);
            }

            // Trigger estimate refinement
            await RefineEstimateAsync(jobStage.Job.PartId, jobStage.StageType, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LEARNING] Error recording completion for JobStage {JobStageId}", jobStageId);
            // Don't throw - learning failure shouldn't break stage completion
        }
    }

    /// <inheritdoc />
    public async Task RefineEstimateAsync(int partId, string stageType, CancellationToken ct = default)
    {
        try
        {
            // Find the PartStageRequirement for this part and stage type
            var requirement = await _context.PartStageRequirements
                .Include(psr => psr.ProductionStage)
                .FirstOrDefaultAsync(psr => psr.PartId == partId && 
                                            (psr.ProductionStage.Name.Contains(stageType) || 
                                             stageType.Contains(psr.ProductionStage.Name)), ct);

            if (requirement == null)
            {
                _logger.LogDebug("[LEARNING] No PartStageRequirement found for Part {PartId}, Stage {StageType}", 
                    partId, stageType);
                return;
            }

            // Get all completed stage history for this part and stage type
            var historyRecords = await _context.JobStageHistories
                .Where(h => h.Job.PartId == partId && 
                           h.Action == "StageCompleted" &&
                           h.StageName == stageType &&
                           h.StageHours.HasValue &&
                           h.StageHours.Value > 0)
                .OrderByDescending(h => h.Timestamp)
                .Take(50) // Limit to recent history
                .Select(h => h.StageHours!.Value)
                .ToListAsync(ct);

            if (!historyRecords.Any())
            {
                _logger.LogDebug("[LEARNING] No history records for Part {PartId}, Stage {StageType}", 
                    partId, stageType);
                return;
            }

            // Get the most recent actual duration
            var lastActual = historyRecords.First();

            // Calculate EMA
            double newAverage;
            if (requirement.ActualAverageDurationHours.HasValue && requirement.ActualSampleCount > 0)
            {
                // Apply EMA: newAvg = ? * newActual + (1-?) * oldAvg
                newAverage = (EmaAlpha * lastActual) + ((1 - EmaAlpha) * requirement.ActualAverageDurationHours.Value);
            }
            else
            {
                // First observation - use it directly
                newAverage = lastActual;
            }

            // Update the learning fields
            requirement.ActualAverageDurationHours = Math.Round(newAverage, 3);
            requirement.ActualSampleCount = historyRecords.Count;
            requirement.LastActualDurationHours = Math.Round(lastActual, 3);
            requirement.EstimateLastUpdated = DateTime.UtcNow;
            requirement.LastModifiedDate = DateTime.UtcNow;
            requirement.LastModifiedBy = "LearningService";

            // Only auto-update EstimatedHours if we have sufficient data and source is Auto
            if (requirement.ActualSampleCount >= MinSampleCountForAutoUpdate)
            {
                // If source is Manual, we don't override unless admin explicitly enables auto-update
                // For now, we'll set source to Auto on first sufficient data and update from then on
                if (requirement.EstimateSource == "Default" || requirement.EstimateSource == "Auto")
                {
                    requirement.EstimatedHours = Math.Round(newAverage, 2);
                    requirement.EstimateSource = "Auto";
                }
            }

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "[LEARNING] Refined estimate for Part {PartId}, Stage {StageType}: " +
                "Avg={Average:F2}h, Last={Last:F2}h, Samples={Count}, Source={Source}",
                partId, stageType, newAverage, lastActual, historyRecords.Count, requirement.EstimateSource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LEARNING] Error refining estimate for Part {PartId}, Stage {StageType}", 
                partId, stageType);
            // Don't throw - learning failure shouldn't break the application
        }
    }

    /// <inheritdoc />
    public async Task<StagePerformanceReport> GetStagePerformanceAsync(int partId, CancellationToken ct = default)
    {
        var part = await _context.Parts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == partId, ct);

        if (part == null)
        {
            return new StagePerformanceReport
            {
                PartId = partId,
                PartNumber = "Unknown",
                PartName = "Part not found"
            };
        }

        var requirements = await _context.PartStageRequirements
            .Include(psr => psr.ProductionStage)
            .Where(psr => psr.PartId == partId && psr.IsActive)
            .OrderBy(psr => psr.ExecutionOrder)
            .AsNoTracking()
            .ToListAsync(ct);

        var report = new StagePerformanceReport
        {
            PartId = partId,
            PartNumber = part.PartNumber,
            PartName = part.Name,
            Stages = requirements.Select(r => new StagePerformanceItem
            {
                PartStageRequirementId = r.Id,
                StageName = r.ProductionStage?.Name ?? "Unknown",
                StageType = r.ProductionStage?.Name ?? "Unknown",
                ExecutionOrder = r.ExecutionOrder,
                EstimatedHours = r.GetEffectiveEstimatedHours(),
                ActualAverageHours = r.ActualAverageDurationHours,
                LastActualHours = r.LastActualDurationHours,
                SampleCount = r.ActualSampleCount,
                EstimateSource = r.EstimateSource,
                LastUpdated = r.EstimateLastUpdated
            }).ToList()
        };

        return report;
    }

    /// <inheritdoc />
    public async Task ResetToManualAsync(int partStageRequirementId, double? manualHours, CancellationToken ct = default)
    {
        var requirement = await _context.PartStageRequirements
            .FirstOrDefaultAsync(psr => psr.Id == partStageRequirementId, ct);

        if (requirement == null)
        {
            _logger.LogWarning("[LEARNING] PartStageRequirement {Id} not found for reset", partStageRequirementId);
            return;
        }

        // Reset to manual mode
        requirement.EstimateSource = "Manual";
        requirement.EstimateLastUpdated = DateTime.UtcNow;
        requirement.LastModifiedDate = DateTime.UtcNow;
        requirement.LastModifiedBy = "Admin";

        if (manualHours.HasValue && manualHours.Value > 0)
        {
            requirement.EstimatedHours = manualHours.Value;
        }

        // Note: We preserve the learning data (ActualAverageDurationHours, etc.) 
        // so it can be viewed even though we're in Manual mode

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("[LEARNING] Reset PartStageRequirement {Id} to Manual mode. Hours={Hours}", 
            partStageRequirementId, requirement.EstimatedHours);
    }
}
