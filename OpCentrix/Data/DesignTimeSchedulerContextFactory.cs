using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OpCentrix.Data
{
    /// <summary>
    /// Design-time factory so EF CLI can create the SchedulerContext without needing full Program startup.
    /// Falls back to the same default SQLite file used at runtime if no configuration is loaded.
    /// </summary>
    public class DesignTimeSchedulerContextFactory : IDesignTimeDbContextFactory<SchedulerContext>
    {
        public SchedulerContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SchedulerContext>();
            // Simple fallback connection (matches Program.cs fallback)
            var connectionString = "Data Source=scheduler.db";
            optionsBuilder.UseSqlite(connectionString);
            return new SchedulerContext(optionsBuilder.Options);
        }
    }
}
