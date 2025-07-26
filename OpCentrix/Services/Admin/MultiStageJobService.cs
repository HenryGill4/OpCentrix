using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for managing multi-stage job workflows
/// Task 11: Modular Multi-Stage Scheduling
/// </summary>
public interface IMultiStageJobService
{
    // Job stage management
    Task<JobStage?> GetStageByIdAsync(int stageId);
    Task<List<JobStage>> GetStagesByJobIdAsync(int jobId);
    Task<List<JobStage>> GetStagesByDepartmentAsync(string department, DateTime startDate, DateTime endDate);
    Task<JobStage> CreateStageAsync(JobStage stage);
    Task<JobStage> UpdateStageAsync(JobStage stage);
    Task<bool> DeleteStageAsync(int stageId);
    
    // Stage dependency management
    Task<List<StageDependency>> GetStageDependenciesAsync(int stageId);
    Task<StageDependency> CreateDependencyAsync(StageDependency dependency);
    Task<bool> DeleteDependencyAsync(int dependencyId);
    Task<bool> ValidateDependencyAsync(int dependentStageId, int requiredStageId);
    
    // Workflow operations
    Task<List<JobStage>> CreateDefaultStagesForJobAsync(int jobId, string jobType);
    Task<bool> StartStageAsync(int stageId, string operatorName);
    Task<bool> CompleteStageAsync(int stageId, decimal? actualCost = null);
    Task<bool> UpdateStageProgressAsync(int stageId, double progressPercent);
    Task<List<JobStage>> GetReadyStagesAsync(string? department = null);
    
    // Scheduling and validation
    Task<bool> ValidateStageScheduleAsync(JobStage stage);
    Task<List<string>> GetSchedulingConflictsAsync(JobStage stage);
    Task<bool> RescheduleStageAsync(int stageId, DateTime newStart, DateTime newEnd);
    Task<TimeSpan> CalculateJobDurationAsync(int jobId);
    
    // Reporting and analytics
    Task<Dictionary<string, int>> GetStageStatusCountsAsync(int jobId);
    Task<Dictionary<string, double>> GetDepartmentUtilizationAsync(DateTime startDate, DateTime endDate);
    Task<List<JobStage>> GetOverdueStagesAsync();
    Task<double> CalculateJobCompletionPercentAsync(int jobId);
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

    #region Stage Management

    public async Task<JobStage?> GetStageByIdAsync(int stageId)
    {
        try
        {
            return await _context.JobStages
                .Include(s => s.Job)
                .Include(s => s.Machine)
                .Include(s => s.Dependencies).ThenInclude(d => d.RequiredStage)
                .Include(s => s.Dependents).ThenInclude(d => d.DependentStage)
                .Include(s => s.StageNotes)
                .FirstOrDefaultAsync(s => s.Id == stageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stage {StageId}", stageId);
            return null;
        }
    }

    public async Task<List<JobStage>> GetStagesByJobIdAsync(int jobId)
    {
        try
        {
            return await _context.JobStages
                .Include(s => s.Machine)
                .Include(s => s.Dependencies).ThenInclude(d => d.RequiredStage)
                .Include(s => s.StageNotes)
                .Where(s => s.JobId == jobId)
                .OrderBy(s => s.ExecutionOrder)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stages for job {JobId}", jobId);
            return new List<JobStage>();
        }
    }

    public async Task<List<JobStage>> GetStagesByDepartmentAsync(string department, DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _context.JobStages
                .Include(s => s.Job)
                .Include(s => s.Machine)
                .Where(s => s.Department == department &&
                           s.ScheduledStart >= startDate &&
                           s.ScheduledEnd <= endDate)
                .OrderBy(s => s.ScheduledStart)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stages for department {Department}", department);
            return new List<JobStage>();
        }
    }

    public async Task<JobStage> CreateStageAsync(JobStage stage)
    {
        try
        {
            stage.CreatedDate = DateTime.UtcNow;
            stage.LastModifiedDate = DateTime.UtcNow;

            _context.JobStages.Add(stage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created stage {StageId} for job {JobId}", stage.Id, stage.JobId);
            return stage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stage for job {JobId}", stage.JobId);
            throw;
        }
    }

    public async Task<JobStage> UpdateStageAsync(JobStage stage)
    {
        try
        {
            stage.LastModifiedDate = DateTime.UtcNow;
            
            _context.JobStages.Update(stage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated stage {StageId}", stage.Id);
            return stage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stage {StageId}", stage.Id);
            throw;
        }
    }

    public async Task<bool> DeleteStageAsync(int stageId)
    {
        try
        {
            var stage = await _context.JobStages.FindAsync(stageId);
            if (stage == null)
                return false;

            // Check if stage has dependents
            var hasDependents = await _context.StageDependencies
                .AnyAsync(d => d.RequiredStageId == stageId);

            if (hasDependents)
                throw new InvalidOperationException("Cannot delete stage with dependent stages");

            _context.JobStages.Remove(stage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted stage {StageId}", stageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stage {StageId}", stageId);
            throw;
        }
    }

    #endregion

    #region Dependency Management

    public async Task<List<StageDependency>> GetStageDependenciesAsync(int stageId)
    {
        try
        {
            return await _context.StageDependencies
                .Include(d => d.RequiredStage)
                .Include(d => d.DependentStage)
                .Where(d => d.DependentStageId == stageId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dependencies for stage {StageId}", stageId);
            return new List<StageDependency>();
        }
    }

    public async Task<StageDependency> CreateDependencyAsync(StageDependency dependency)
    {
        try
        {
            // Validate that dependency doesn't create a cycle
            if (!await ValidateDependencyAsync(dependency.DependentStageId, dependency.RequiredStageId))
                throw new InvalidOperationException("Dependency would create a circular reference");

            dependency.CreatedDate = DateTime.UtcNow;
            
            _context.StageDependencies.Add(dependency);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created dependency: Stage {DependentId} depends on Stage {RequiredId}", 
                dependency.DependentStageId, dependency.RequiredStageId);
            
            return dependency;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dependency");
            throw;
        }
    }

    public async Task<bool> DeleteDependencyAsync(int dependencyId)
    {
        try
        {
            var dependency = await _context.StageDependencies.FindAsync(dependencyId);
            if (dependency == null)
                return false;

            _context.StageDependencies.Remove(dependency);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted dependency {DependencyId}", dependencyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting dependency {DependencyId}", dependencyId);
            throw;
        }
    }

    public async Task<bool> ValidateDependencyAsync(int dependentStageId, int requiredStageId)
    {
        try
        {
            // Check for self-reference
            if (dependentStageId == requiredStageId)
                return false;

            // Check for circular dependencies using depth-first search
            var visited = new HashSet<int>();
            return !await HasCircularDependencyAsync(requiredStageId, dependentStageId, visited);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating dependency");
            return false;
        }
    }

    private async Task<bool> HasCircularDependencyAsync(int currentStageId, int targetStageId, HashSet<int> visited)
    {
        if (visited.Contains(currentStageId))
            return false;

        visited.Add(currentStageId);

        var dependencies = await _context.StageDependencies
            .Where(d => d.DependentStageId == currentStageId)
            .Select(d => d.RequiredStageId)
            .ToListAsync();

        foreach (var requiredStageId in dependencies)
        {
            if (requiredStageId == targetStageId)
                return true;

            if (await HasCircularDependencyAsync(requiredStageId, targetStageId, visited))
                return true;
        }

        return false;
    }

    #endregion

    #region Workflow Operations

    public async Task<List<JobStage>> CreateDefaultStagesForJobAsync(int jobId, string jobType)
    {
        try
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null)
                throw new ArgumentException("Job not found", nameof(jobId));

            var stages = new List<JobStage>();
            var baseDate = job.ScheduledStart;

            // Define default workflows based on job type
            var stageDefinitions = GetDefaultStageDefinitions(jobType);

            var currentDate = baseDate;
            foreach (var (stageName, department, duration, order) in stageDefinitions)
            {
                var stage = new JobStage
                {
                    JobId = jobId,
                    StageType = stageName,
                    StageName = stageName,
                    Department = department,
                    ExecutionOrder = order,
                    ScheduledStart = currentDate,
                    ScheduledEnd = currentDate.AddHours(duration),
                    EstimatedDurationHours = duration,
                    Status = "Scheduled",
                    Priority = job.Priority,
                    CreatedBy = "System"
                };

                stages.Add(stage);
                currentDate = stage.ScheduledEnd;
            }

            _context.JobStages.AddRange(stages);
            await _context.SaveChangesAsync();

            // Create dependencies between sequential stages
            await CreateSequentialDependenciesAsync(stages);

            _logger.LogInformation("Created {StageCount} default stages for job {JobId}", stages.Count, jobId);
            return stages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating default stages for job {JobId}", jobId);
            throw;
        }
    }

    private List<(string stageName, string department, double duration, int order)> GetDefaultStageDefinitions(string jobType)
    {
        return jobType.ToLower() switch
        {
            "printing" => new List<(string, string, double, int)>
            {
                ("Printing", "Printing", 8.0, 1),
                ("Cooling", "Printing", 2.0, 2),
                ("Powder Removal", "Printing", 1.0, 3),
                ("Support Removal", "Printing", 1.5, 4),
                ("Heat Treatment", "Printing", 4.0, 5),
                ("Inspection", "Inspection", 1.0, 6)
            },
            "machining" => new List<(string, string, double, int)>
            {
                ("Setup", "Machining", 1.0, 1),
                ("Rough Machining", "Machining", 4.0, 2),
                ("Finish Machining", "Machining", 2.0, 3),
                ("Deburring", "Machining", 0.5, 4),
                ("Inspection", "Inspection", 0.5, 5)
            },
            "assembly" => new List<(string, string, double, int)>
            {
                ("Component Prep", "Assembly", 1.0, 1),
                ("Assembly", "Assembly", 3.0, 2),
                ("Testing", "Assembly", 1.0, 3),
                ("Final Inspection", "Inspection", 0.5, 4),
                ("Packaging", "Shipping", 0.5, 5)
            },
            _ => new List<(string, string, double, int)>
            {
                ("Processing", "General", 2.0, 1),
                ("Inspection", "Inspection", 0.5, 2)
            }
        };
    }

    private async Task CreateSequentialDependenciesAsync(List<JobStage> stages)
    {
        for (int i = 1; i < stages.Count; i++)
        {
            var dependency = new StageDependency
            {
                DependentStageId = stages[i].Id,
                RequiredStageId = stages[i - 1].Id,
                DependencyType = "FinishToStart",
                IsMandatory = true
            };

            _context.StageDependencies.Add(dependency);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> StartStageAsync(int stageId, string operatorName)
    {
        try
        {
            var stage = await GetStageByIdAsync(stageId);
            if (stage == null)
                return false;

            stage.StartStage(operatorName);
            await UpdateStageAsync(stage);

            _logger.LogInformation("Started stage {StageId} by operator {Operator}", stageId, operatorName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting stage {StageId}", stageId);
            return false;
        }
    }

    public async Task<bool> CompleteStageAsync(int stageId, decimal? actualCost = null)
    {
        try
        {
            var stage = await GetStageByIdAsync(stageId);
            if (stage == null)
                return false;

            stage.CompleteStage(actualCost);
            await UpdateStageAsync(stage);

            // Update dependent stages to "Ready" if all dependencies are met
            await UpdateDependentStageStatusAsync(stageId);

            _logger.LogInformation("Completed stage {StageId}", stageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing stage {StageId}", stageId);
            return false;
        }
    }

    private async Task UpdateDependentStageStatusAsync(int completedStageId)
    {
        var dependentStages = await _context.StageDependencies
            .Include(d => d.DependentStage)
            .Where(d => d.RequiredStageId == completedStageId)
            .Select(d => d.DependentStage)
            .ToListAsync();

        foreach (var stage in dependentStages)
        {
            if (stage.IsReadyToStart && stage.Status == "Scheduled")
            {
                stage.Status = "Ready";
                stage.CanStart = true;
                stage.LastModifiedDate = DateTime.UtcNow;
            }
        }

        if (dependentStages.Any())
            await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateStageProgressAsync(int stageId, double progressPercent)
    {
        try
        {
            var stage = await _context.JobStages.FindAsync(stageId);
            if (stage == null)
                return false;

            stage.UpdateProgress(progressPercent);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Updated progress for stage {StageId}: {Progress}%", stageId, progressPercent);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress for stage {StageId}", stageId);
            return false;
        }
    }

    public async Task<List<JobStage>> GetReadyStagesAsync(string? department = null)
    {
        try
        {
            var query = _context.JobStages
                .Include(s => s.Job)
                .Include(s => s.Machine)
                .Where(s => s.Status == "Ready" || s.Status == "Scheduled");

            if (!string.IsNullOrEmpty(department))
                query = query.Where(s => s.Department == department);

            return await query
                .OrderBy(s => s.Priority)
                .ThenBy(s => s.ScheduledStart)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ready stages");
            return new List<JobStage>();
        }
    }

    #endregion

    #region Scheduling and Validation

    public async Task<bool> ValidateStageScheduleAsync(JobStage stage)
    {
        try
        {
            var conflicts = await GetSchedulingConflictsAsync(stage);
            return !conflicts.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating stage schedule");
            return false;
        }
    }

    public async Task<List<string>> GetSchedulingConflictsAsync(JobStage stage)
    {
        var conflicts = new List<string>();

        try
        {
            // Check machine conflicts
            if (!string.IsNullOrEmpty(stage.MachineId))
            {
                var machineConflicts = await _context.JobStages
                    .Where(s => s.Id != stage.Id &&
                               s.MachineId == stage.MachineId &&
                               s.Status != "Completed" &&
                               s.Status != "Cancelled" &&
                               s.ScheduledStart < stage.ScheduledEnd &&
                               s.ScheduledEnd > stage.ScheduledStart)
                    .Include(s => s.Job)
                    .ToListAsync();

                foreach (var conflict in machineConflicts)
                {
                    conflicts.Add($"Machine {stage.MachineId} is scheduled for Job {conflict.Job.PartNumber} from {conflict.ScheduledStart:MM/dd HH:mm} to {conflict.ScheduledEnd:MM/dd HH:mm}");
                }
            }

            // Check dependency constraints
            var dependencies = await GetStageDependenciesAsync(stage.Id);
            foreach (var dependency in dependencies)
            {
                if (dependency.RequiredStage.Status != "Completed" && 
                    dependency.RequiredStage.ScheduledEnd > stage.ScheduledStart)
                {
                    conflicts.Add($"Stage depends on '{dependency.RequiredStage.StageName}' which is not completed and scheduled to finish at {dependency.RequiredStage.ScheduledEnd:MM/dd HH:mm}");
                }
            }

            return conflicts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking scheduling conflicts");
            return new List<string> { "Error checking conflicts" };
        }
    }

    public async Task<bool> RescheduleStageAsync(int stageId, DateTime newStart, DateTime newEnd)
    {
        try
        {
            var stage = await _context.JobStages.FindAsync(stageId);
            if (stage == null)
                return false;

            stage.ScheduledStart = newStart;
            stage.ScheduledEnd = newEnd;
            stage.LastModifiedDate = DateTime.UtcNow;

            var conflicts = await GetSchedulingConflictsAsync(stage);
            if (conflicts.Any())
                throw new InvalidOperationException($"Rescheduling would create conflicts: {string.Join(", ", conflicts)}");

            await _context.SaveChangesAsync();

            _logger.LogInformation("Rescheduled stage {StageId} to {Start} - {End}", stageId, newStart, newEnd);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rescheduling stage {StageId}", stageId);
            throw;
        }
    }

    public async Task<TimeSpan> CalculateJobDurationAsync(int jobId)
    {
        try
        {
            var stages = await _context.JobStages
                .Where(s => s.JobId == jobId)
                .OrderBy(s => s.ExecutionOrder)
                .ToListAsync();

            if (!stages.Any())
                return TimeSpan.Zero;

            var earliestStart = stages.Min(s => s.ScheduledStart);
            var latestEnd = stages.Max(s => s.ScheduledEnd);

            return latestEnd - earliestStart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating job duration for job {JobId}", jobId);
            return TimeSpan.Zero;
        }
    }

    #endregion

    #region Reporting and Analytics

    public async Task<Dictionary<string, int>> GetStageStatusCountsAsync(int jobId)
    {
        try
        {
            return await _context.JobStages
                .Where(s => s.JobId == jobId)
                .GroupBy(s => s.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stage status counts for job {JobId}", jobId);
            return new Dictionary<string, int>();
        }
    }

    public async Task<Dictionary<string, double>> GetDepartmentUtilizationAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var utilizationData = await _context.JobStages
                .Where(s => s.ScheduledStart >= startDate && s.ScheduledEnd <= endDate)
                .GroupBy(s => s.Department)
                .Select(g => new { 
                    Department = g.Key, 
                    TotalHours = g.Sum(s => s.EstimatedDurationHours) 
                })
                .ToDictionaryAsync(x => x.Department, x => x.TotalHours);

            // Calculate utilization as percentage of available time
            var totalHours = (endDate - startDate).TotalHours;
            return utilizationData.ToDictionary(
                kvp => kvp.Key, 
                kvp => totalHours > 0 ? (kvp.Value / totalHours) * 100 : 0
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating department utilization");
            return new Dictionary<string, double>();
        }
    }

    public async Task<List<JobStage>> GetOverdueStagesAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            return await _context.JobStages
                .Include(s => s.Job)
                .Include(s => s.Machine)
                .Where(s => s.ScheduledEnd < now && 
                           s.Status != "Completed" && 
                           s.Status != "Cancelled")
                .OrderBy(s => s.ScheduledEnd)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue stages");
            return new List<JobStage>();
        }
    }

    public async Task<double> CalculateJobCompletionPercentAsync(int jobId)
    {
        try
        {
            var stages = await _context.JobStages
                .Where(s => s.JobId == jobId)
                .ToListAsync();

            if (!stages.Any())
                return 0;

            var totalWeight = stages.Sum(s => s.EstimatedDurationHours);
            var completedWeight = stages
                .Where(s => s.Status == "Completed")
                .Sum(s => s.EstimatedDurationHours);

            var inProgressWeight = stages
                .Where(s => s.Status == "In-Progress")
                .Sum(s => s.EstimatedDurationHours * (s.ProgressPercent / 100.0));

            return totalWeight > 0 ? ((completedWeight + inProgressWeight) / totalWeight) * 100 : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating job completion percent for job {JobId}", jobId);
            return 0;
        }
    }

    #endregion
}