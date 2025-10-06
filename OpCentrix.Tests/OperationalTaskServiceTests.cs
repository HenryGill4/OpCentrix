using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OpCentrix.Data;
using OpCentrix.Models.MaintenanceV2;
using OpCentrix.Services;
using DuplicateOperationalTaskException = OpCentrix.Services.DuplicateOperationalTaskException;

// Ensure namespace matches tests assembly
namespace OpCentrix.Tests;

public class OperationalTaskServiceTests
{
    private SchedulerContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SchedulerContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SchedulerContext(options);
    }

    [Fact]
    public async Task Create_Persists_ConfigJson()
    {
        using var ctx = CreateContext();
        var svc = new OperationalTaskService(ctx, new NullLogger<OperationalTaskService>());
        var task = new OperationalTask
        {
            Title = "Test Task",
            CreatedByUserId = 1,
            ConfigJson = "{\"options\":{\"suppressIfOpen\":false},\"intervals\":[]}"
        };
        var created = await svc.CreateAsync(task);
        Assert.True(created.Id > 0);
        var db = await ctx.OperationalTasks.FirstAsync();
        Assert.Equal(task.ConfigJson, db.ConfigJson);
    }

    [Fact]
    public async Task Duplicate_Suppression_Throws()
    {
        using var ctx = CreateContext();
        var svc = new OperationalTaskService(ctx, new NullLogger<OperationalTaskService>());
        var config = "{\"options\":{\"suppressIfOpen\":true},\"intervals\":[{\"type\":\"BuildCount\",\"material\":\"Any\",\"threshold\":100}]}";
        await svc.CreateAsync(new OperationalTask { Title = "Clean Filter", MachineId = "TI1", CreatedByUserId = 1, ConfigJson = config });
        await Assert.ThrowsAsync<DuplicateOperationalTaskException>(async () =>
        {
            await svc.CreateAsync(new OperationalTask { Title = "Clean Filter", MachineId = "TI1", CreatedByUserId = 1, ConfigJson = config });
        });
    }
}
