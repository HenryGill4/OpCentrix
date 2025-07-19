using Microsoft.EntityFrameworkCore;
using OpCentrix.Models;

namespace OpCentrix.Data
{
    public class SchedulerContext : DbContext
    {
        public SchedulerContext(DbContextOptions<SchedulerContext> options)
            : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<Part> Parts { get; set; }
    }
}
