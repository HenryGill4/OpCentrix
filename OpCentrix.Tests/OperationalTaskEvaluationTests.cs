using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OpCentrix.Data;
using OpCentrix.Models.MaintenanceV2;
using OpCentrix.Services;
using System.Text.Json;

namespace OpCentrix.Tests;

public class OperationalTaskEvaluationTests
{
    private SchedulerContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SchedulerContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SchedulerContext(options);
    }

    private static OperationalTask MakeTask(string title, object config, DateTime? createdAt = null, string? machineId = "M1")
    {
        return new OperationalTask
        {
            Title = title,
            CreatedByUserId = 1,
            MachineId = machineId,
            ConfigJson = JsonSerializer.Serialize(config),
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
    }

    [Fact]
    public async Task BuildCount_Any_Marks_Due_For_New_Builds()
    {
        using var ctx = CreateContext();
        var taskCreated = DateTime.UtcNow.AddMinutes(-30);
        var task = MakeTask("BC Any", new
        {
            logic = "ANY",
            intervals = new[] { new { type = "BuildCount", material = "Any", threshold = 2 } }
        }, createdAt: taskCreated, machineId: "M1");
        ctx.OperationalTasks.Add(task);
        await ctx.SaveChangesAsync();

        // Builds BEFORE task creation should NOT count
        ctx.BuildJobs.Add(new OpCentrix.Models.BuildJob
        {
            PrinterName = "M1",
            ActualStartTime = taskCreated.AddHours(-2),
            ActualEndTime = taskCreated.AddHours(-1.5),
            Status = "Completed",
            UserId = 1
        });
        // Builds AFTER task creation should count
        for (int i = 0; i < 2; i++)
        {
            ctx.BuildJobs.Add(new OpCentrix.Models.BuildJob
            {
                PrinterName = "M1",
                ActualStartTime = taskCreated.AddMinutes(5 + (i * 10)),
                ActualEndTime = taskCreated.AddMinutes(15 + (i * 10)),
                Status = "Completed",
                UserId = 1
            });
        }
        await ctx.SaveChangesAsync();

        var eval = new OperationalTaskEvaluationService(ctx, new NullLogger<OperationalTaskEvaluationService>());
        await eval.EvaluateAsync();

        var reloaded = await ctx.OperationalTasks.FirstAsync();
        Assert.NotNull(reloaded.DueAt); // threshold met by 2 post-creation builds
    }

    [Fact]
    public async Task Historical_Builds_Do_Not_Trigger_Immediately()
    {
        using var ctx = CreateContext();
        // Add historical builds
        for (int i = 0; i < 5; i++)
        {
            ctx.BuildJobs.Add(new OpCentrix.Models.BuildJob
            {
                PrinterName = "MHist",
                ActualStartTime = DateTime.UtcNow.AddHours(-10 - i),
                ActualEndTime = DateTime.UtcNow.AddHours(-9.5 - i),
                Status = "Completed",
                UserId = 1
            });
        }
        await ctx.SaveChangesAsync();

        // Create task AFTER builds done
        var task = MakeTask("History Gap", new
        {
            logic = "ANY",
            intervals = new[] { new { type = "BuildCount", material = "Any", threshold = 1 } }
        }, createdAt: DateTime.UtcNow, machineId: "MHist");
        ctx.OperationalTasks.Add(task);
        await ctx.SaveChangesAsync();

        var eval = new OperationalTaskEvaluationService(ctx, new NullLogger<OperationalTaskEvaluationService>());
        await eval.EvaluateAsync();
        var reloaded = await ctx.OperationalTasks.FirstAsync();
        Assert.Null(reloaded.DueAt); // historical builds should not trigger

        // Add a new build now -> should trigger
        ctx.BuildJobs.Add(new OpCentrix.Models.BuildJob
        {
            PrinterName = "MHist",
            ActualStartTime = DateTime.UtcNow.AddMinutes(-5),
            ActualEndTime = DateTime.UtcNow.AddMinutes(-1),
            Status = "Completed",
            UserId = 1
        });
        await ctx.SaveChangesAsync();
        await eval.EvaluateAsync();
        reloaded = await ctx.OperationalTasks.FirstAsync();
        Assert.NotNull(reloaded.DueAt);
    }

    [Fact]
    public async Task BuildCount_Material_Specific_Only_Due_When_New_Material_Builds_Present()
    {
        using var ctx = CreateContext();
        var createdAt = DateTime.UtcNow.AddHours(-2);
        var taskTi = MakeTask("BC Ti", new
        {
            logic = "ANY",
            intervals = new[] { new { type = "BuildCount", material = "Titanium", threshold = 1 } }
        }, createdAt: createdAt, machineId: "M2");
        var taskInc = MakeTask("BC Inc Count 2", new
        {
            logic = "ANY",
            intervals = new[] { new { type = "BuildCount", material = "Inconel", threshold = 2 } }
        }, createdAt: createdAt, machineId: "M2");
        ctx.OperationalTasks.AddRange(taskTi, taskInc);
        await ctx.SaveChangesAsync();

        // One Titanium build AFTER creation
        var tiBuild = new OpCentrix.Models.BuildJob
        {
            PrinterName = "M2",
            ActualStartTime = createdAt.AddMinutes(10),
            ActualEndTime = createdAt.AddMinutes(40),
            Status = "Completed",
            UserId = 1
        };
        // One Inconel build AFTER creation (not enough to trigger threshold 2)
        var incBuild = new OpCentrix.Models.BuildJob
        {
            PrinterName = "M2",
            ActualStartTime = createdAt.AddMinutes(60),
            ActualEndTime = createdAt.AddMinutes(90),
            Status = "Completed",
            UserId = 1
        };
        ctx.BuildJobs.AddRange(tiBuild, incBuild);
        await ctx.SaveChangesAsync();
        ctx.BuildJobParts.AddRange(
            new OpCentrix.Models.BuildJobPart { BuildId = tiBuild.BuildId, PartNumber = "P1", Quantity = 1, IsPrimary = true, Material = "Titanium" },
            new OpCentrix.Models.BuildJobPart { BuildId = incBuild.BuildId, PartNumber = "P2", Quantity = 1, IsPrimary = true, Material = "Inconel" }
        );
        await ctx.SaveChangesAsync();

        var eval = new OperationalTaskEvaluationService(ctx, new NullLogger<OperationalTaskEvaluationService>());
        await eval.EvaluateAsync();

        var tasks = await ctx.OperationalTasks.OrderBy(t => t.Title).ToListAsync();
        Assert.NotNull(tasks.First(t => t.Title == "BC Ti").DueAt); // should be due
        Assert.Null(tasks.First(t => t.Title == "BC Inc Count 2").DueAt); // threshold not met
    }

    [Fact]
    public async Task MachineHours_Triggers_Due_Based_On_PostCreation_Hours()
    {
        using var ctx = CreateContext();
        var createdAt = DateTime.UtcNow.AddHours(-4);
        // Build spanning before and after task creation to test trimming
        var spanningBuild = new OpCentrix.Models.BuildJob
        {
            PrinterName = "M3",
            ActualStartTime = createdAt.AddHours(-2), // starts before task creation
            ActualEndTime = createdAt.AddHours(1),
            Status = "Completed",
            UserId = 1
        };
        ctx.BuildJobs.Add(spanningBuild);
        await ctx.SaveChangesAsync();

        var task = MakeTask("MH 2h", new
        {
            logic = "ANY",
            intervals = new[] { new { type = "MachineHours", hours = 2.0 } }
        }, createdAt: createdAt, machineId: "M3");
        ctx.OperationalTasks.Add(task);
        await ctx.SaveChangesAsync();

        var eval = new OperationalTaskEvaluationService(ctx, new NullLogger<OperationalTaskEvaluationService>());
        await eval.EvaluateAsync();
        var reloaded = await ctx.OperationalTasks.FirstAsync(t => t.Title == "MH 2h");
        Assert.NotNull(reloaded.DueAt); // 3 hours after creation (trimmed) >= 2
    }

    [Fact]
    public async Task ALL_Logic_Waits_Until_All_Satisfied_PostCreation()
    {
        using var ctx = CreateContext();
        var createdAt = DateTime.UtcNow.AddHours(-2);
        // First build after creation (only 1 so far)
        ctx.BuildJobs.Add(new OpCentrix.Models.BuildJob
        {
            PrinterName = "M4",
            ActualStartTime = createdAt.AddMinutes(10),
            ActualEndTime = createdAt.AddMinutes(40),
            Status = "Completed",
            UserId = 1
        });
        await ctx.SaveChangesAsync();

        var task = MakeTask("ALL combo", new
        {
            logic = "ALL",
            intervals = new object[] {
                new { type = "BuildCount", material = "Any", threshold = 2 },
                new { type = "MachineHours", hours = 1.0 }
            }
        }, createdAt: createdAt, machineId: "M4");
        ctx.OperationalTasks.Add(task);
        await ctx.SaveChangesAsync();

        var eval = new OperationalTaskEvaluationService(ctx, new NullLogger<OperationalTaskEvaluationService>());
        await eval.EvaluateAsync();
        var firstPass = await ctx.OperationalTasks.FirstAsync();
        Assert.Null(firstPass.DueAt); // build count not yet met (only 1)

        // Add second build
        ctx.BuildJobs.Add(new OpCentrix.Models.BuildJob
        {
            PrinterName = "M4",
            ActualStartTime = DateTime.UtcNow.AddMinutes(-50),
            ActualEndTime = DateTime.UtcNow.AddMinutes(-10),
            Status = "Completed",
            UserId = 1
        });
        await ctx.SaveChangesAsync();

        await eval.EvaluateAsync();
        var secondPass = await ctx.OperationalTasks.FirstAsync();
        Assert.NotNull(secondPass.DueAt);
    }
}
