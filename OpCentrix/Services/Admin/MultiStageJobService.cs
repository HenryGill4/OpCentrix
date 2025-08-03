using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.JobStaging;
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
}

public class MultiStageJobService : IMultiStageJobService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<MultiStageJobService> _logger;

    public MultiStageJobService(SchedulerContext context, ILogger<MultiStageJobService> logger)
    {
        _context = context;
        _logger = logger;
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
            
            await _context.SaveChangesAsync();
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
}

public class JobStageValidationResult
{
    public int StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}