using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.JobStaging;
using OpCentrix.Services.Learning;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Services.Admin;

public interface IMultiStageJobService
{
    Task<List<JobStage>> GetJobStagesAsync(int jobId);
    Task<List<JobStageDependency>> GetStageDependenciesAsync(int jobId);
    Task<JobStage?> GetJobStageAsync(int stageId);
    Task<bool> CreateJobStageAsync(JobStage stage);
    Task<bool> UpdateJobStageAsync(JobStage stage);
    Task<bool> DeleteJobStageAsync(int stageId);
    Task<bool> StartStageAsync(int stageId, string operatorName);
    Task<bool> CompleteStageAsync(int stageId, string operatorName, string? notes = null);
    Task<bool> CreateStageDependencyAsync(JobStageDependency dependency);
    Task<bool> DeleteStageDependencyAsync(int dependencyId);
    Task<List<JobStageValidationResult>> ValidateJobStagesAsync(int jobId);
    Task<bool> CanStartStageAsync(int stageId);
    Task<bool> UpdateStageProgressAsync(int stageId, double progressPercent);

    /// <summary>
    /// Creates JobStage records for a job from its MasterPart stage definitions.
    /// Reads StageDefinitions, sequences them by ExecutionOrder, and creates
    /// linked JobStage rows with FinishToStart dependencies.
    /// </summary>
    Task<List<JobStage>> CreateJobStagesFromMasterPartAsync(int jobId, CancellationToken ct = default);
}

public class MultiStageJobService : IMultiStageJobService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<MultiStageJobService> _logger;
    private readonly IPartStageLearningService? _learningService;

    public MultiStageJobService(
        SchedulerContext context, 
        ILogger<MultiStageJobService> logger,
        IPartStageLearningService? learningService = null)
    {
        _context = context;
        _logger = logger;
        _learningService = learningService;
    }

    public async Task<List<JobStage>> GetJobStagesAsync(int jobId)
    {
        return await _context.JobStages
            .Include(js => js.Job)
            .Include(js => js.Dependencies)
                .ThenInclude(d => d.RequiredStage)
            .Include(js => js.Dependents)
                .ThenInclude(d => d.DependentStage)
            .Include(js => js.StageNotes)
            .Where(js => js.JobId == jobId)
            .OrderBy(js => js.ExecutionOrder)
            .ToListAsync();
    }

    public async Task<List<JobStageDependency>> GetStageDependenciesAsync(int jobId)
    {
        return await _context.StageDependencies
            .Include(sd => sd.DependentStage)
            .Include(sd => sd.RequiredStage)
            .Where(sd => sd.DependentStage.JobId == jobId)
            .ToListAsync();
    }

    public async Task<JobStage?> GetJobStageAsync(int stageId)
    {
        return await _context.JobStages
            .Include(js => js.Job)
            .Include(js => js.Dependencies)
                .ThenInclude(d => d.RequiredStage)
            .Include(js => js.Dependents)
                .ThenInclude(d => d.DependentStage)
            .Include(js => js.StageNotes)
            .FirstOrDefaultAsync(js => js.Id == stageId);
    }

    public async Task<bool> CreateJobStageAsync(JobStage stage)
    {
        try
        {
            stage.CreatedDate = DateTime.UtcNow;
            stage.LastModifiedDate = DateTime.UtcNow;
            stage.Status = "Scheduled";
            stage.CanStart = stage.ExecutionOrder == 1;

            _context.JobStages.Add(stage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created job stage {StageName} with ID {StageId}", stage.StageName, stage.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job stage {StageName}", stage.StageName);
            return false;
        }
    }

    public async Task<bool> UpdateJobStageAsync(JobStage stage)
    {
        try
        {
            var existingStage = await _context.JobStages.FindAsync(stage.Id);
            if (existingStage == null)
            {
                return false;
            }

            existingStage.StageName = stage.StageName;
            existingStage.StageType = stage.StageType;
            existingStage.Department = stage.Department;
            existingStage.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job stage {StageId}", stage.Id);
            return false;
        }
    }

    public async Task<bool> DeleteJobStageAsync(int stageId)
    {
        try
        {
            var stage = await _context.JobStages.FindAsync(stageId);
            if (stage == null)
            {
                return false;
            }

            _context.JobStages.Remove(stage);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job stage {StageId}", stageId);
            return false;
        }
    }

    public async Task<bool> StartStageAsync(int stageId, string operatorName)
    {
        try
        {
            var stage = await _context.JobStages.FindAsync(stageId);
            if (stage == null || !stage.CanStart)
            {
                return false;
            }

            stage.Status = "In-Progress";
            stage.ActualStart = DateTime.UtcNow;
            stage.AssignedOperator = operatorName;
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting job stage {StageId}", stageId);
            return false;
        }
    }

    public async Task<bool> CompleteStageAsync(int stageId, string operatorName, string? notes = null)
    {
        try
        {
            var stage = await _context.JobStages.FindAsync(stageId);
            if (stage == null)
            {
                return false;
            }

            stage.Status = "Completed";
            stage.ActualEnd = DateTime.UtcNow;
            stage.ProgressPercent = 100;
            
            if (!string.IsNullOrWhiteSpace(notes))
            {
                stage.Notes = string.IsNullOrWhiteSpace(stage.Notes) 
                    ? notes 
                    : $"{stage.Notes}\n{notes}";
            }
            
            await _context.SaveChangesAsync();

            // Trigger learning service to record completion and refine estimates
            if (_learningService != null)
            {
                try
                {
                    await _learningService.RecordCompletionAsync(stageId);
                    _logger.LogInformation("[LEARNING] Triggered learning for completed stage {StageId}", stageId);
                }
                catch (Exception learningEx)
                {
                    // Learning failure should not fail stage completion
                    _logger.LogWarning(learningEx, "[LEARNING] Failed to record learning for stage {StageId}", stageId);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing job stage {StageId}", stageId);
            return false;
        }
    }

    public async Task<bool> CreateStageDependencyAsync(JobStageDependency dependency)
    {
        try
        {
            dependency.CreatedDate = DateTime.UtcNow;
            _context.StageDependencies.Add(dependency);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stage dependency");
            return false;
        }
    }

    public async Task<bool> DeleteStageDependencyAsync(int dependencyId)
    {
        try
        {
            var dependency = await _context.StageDependencies.FindAsync(dependencyId);
            if (dependency == null)
            {
                return false;
            }

            _context.StageDependencies.Remove(dependency);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stage dependency {DependencyId}", dependencyId);
            return false;
        }
    }

    public async Task<List<JobStageValidationResult>> ValidateJobStagesAsync(int jobId)
    {
        var results = new List<JobStageValidationResult>();
        var stages = await GetJobStagesAsync(jobId);

        foreach (var stage in stages)
        {
            results.Add(new JobStageValidationResult
            {
                StageId = stage.Id,
                StageName = stage.StageName,
                IsValid = !string.IsNullOrEmpty(stage.StageName),
                ErrorMessage = string.IsNullOrEmpty(stage.StageName) ? "Stage name is required" : string.Empty
            });
        }

        return results;
    }

    public async Task<bool> CanStartStageAsync(int stageId)
    {
        try
        {
            var stage = await _context.JobStages.FindAsync(stageId);
            return stage?.CanStart ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if stage {StageId} can start", stageId);
            return false;
        }
    }

    public async Task<bool> UpdateStageProgressAsync(int stageId, double progressPercent)
    {
        try
        {
            var stage = await _context.JobStages.FindAsync(stageId);
            if (stage == null)
            {
                return false;
            }

            stage.ProgressPercent = Math.Max(0, Math.Min(100, progressPercent));
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress for stage {StageId}", stageId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<List<JobStage>> CreateJobStagesFromMasterPartAsync(int jobId, CancellationToken ct = default)
    {
        var job = await _context.Jobs
            .Include(j => j.MasterPart)
            .FirstOrDefaultAsync(j => j.Id == jobId, ct);

        if (job?.MasterPartId == null)
        {
            _logger.LogWarning("Cannot create stages: Job {JobId} not found or has no MasterPart", jobId);
            return [];
        }

        var stageDefinitions = await _context.StageDefinitions
            .Where(sd => sd.MasterPartId == job.MasterPartId && sd.IsActive)
            .OrderBy(sd => sd.ExecutionOrder)
            .ToListAsync(ct);

        if (stageDefinitions.Count == 0)
        {
            _logger.LogWarning("MasterPart {MasterPartId} has no active stage definitions", job.MasterPartId);
            return [];
        }

        var createdStages = new List<JobStage>();
        var cursor = job.ScheduledStart;

        foreach (var sd in stageDefinitions)
        {
            var durationHours = sd.EstimatedHoursPerPart;
            var setupHours = sd.SetupMinutes / 60.0;
            var teardownHours = sd.TeardownMinutes / 60.0;
            var totalHours = setupHours + durationHours + teardownHours;

            var stage = new JobStage
            {
                JobId = jobId,
                StageType = sd.RequiredMachineType ?? sd.StageName,
                StageName = sd.StageName,
                ExecutionOrder = sd.ExecutionOrder,
                Department = sd.RequiredMachineType ?? "General",
                ScheduledStart = cursor,
                ScheduledEnd = cursor.AddHours(totalHours),
                EstimatedDurationHours = durationHours,
                SetupTimeHours = setupHours,
                CooldownTimeHours = teardownHours,
                Status = "Scheduled",
                CanStart = sd.ExecutionOrder == 1,
                IsBlocking = true,
                Priority = job.Priority,
                CreatedBy = job.CreatedBy,
                CreatedDate = DateTime.UtcNow,
                LastModifiedBy = job.CreatedBy,
                LastModifiedDate = DateTime.UtcNow
            };

            _context.JobStages.Add(stage);
            createdStages.Add(stage);

            cursor = stage.ScheduledEnd;
        }

        await _context.SaveChangesAsync(ct);

        // Create FinishToStart dependencies between sequential stages
        for (int i = 1; i < createdStages.Count; i++)
        {
            var dep = new JobStageDependency
            {
                DependentStageId = createdStages[i].Id,
                RequiredStageId = createdStages[i - 1].Id,
                DependencyType = "FinishToStart",
                IsMandatory = true,
                CreatedDate = DateTime.UtcNow
            };
            _context.StageDependencies.Add(dep);
        }

        await _context.SaveChangesAsync(ct);

        // Update job metadata
        job.TotalStages = createdStages.Count;
        job.WorkflowStage = createdStages[0].StageName;
        job.StageOrder = 1;
        job.ScheduledEnd = createdStages.Last().ScheduledEnd;
        job.EstimatedDuration = job.ScheduledEnd - job.ScheduledStart;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created {Count} job stages for Job {JobId} from MasterPart {MasterPartId}",
            createdStages.Count, jobId, job.MasterPartId);

        return createdStages;
    }
}

public class JobStageValidationResult
{
    public int StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}