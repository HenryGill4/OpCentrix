using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.DatabaseFix
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("OpCentrix Database Fix Tool");
            Console.WriteLine("===========================");

            var connectionString = "Data Source=opcentrix.db";
            var optionsBuilder = new DbContextOptionsBuilder<SchedulerContext>();
            optionsBuilder.UseSqlite(connectionString);

            using var context = new SchedulerContext(optionsBuilder.Options);

            try
            {
                Console.WriteLine("1. Ensuring database is created...");
                await context.Database.EnsureCreatedAsync();

                Console.WriteLine("2. Checking for pending migrations...");
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                
                if (pendingMigrations.Any())
                {
                    Console.WriteLine($"Found {pendingMigrations.Count()} pending migrations:");
                    foreach (var migration in pendingMigrations)
                    {
                        Console.WriteLine($"  - {migration}");
                    }

                    Console.WriteLine("3. Applying migrations...");
                    await context.Database.MigrateAsync();
                    Console.WriteLine("? Migrations applied successfully!");
                }
                else
                {
                    Console.WriteLine("? No pending migrations found.");
                }

                Console.WriteLine("4. Checking SchedulerSettings table...");
                try
                {
                    var settingsCount = await context.SchedulerSettings.CountAsync();
                    Console.WriteLine($"? SchedulerSettings table exists with {settingsCount} records.");

                    if (settingsCount == 0)
                    {
                        Console.WriteLine("5. Creating default settings...");
                        var defaultSettings = new SchedulerSettings
                        {
                            TitaniumToTitaniumChangeoverMinutes = 30,
                            InconelToInconelChangeoverMinutes = 45,
                            CrossMaterialChangeoverMinutes = 120,
                            DefaultMaterialChangeoverMinutes = 60,
                            DefaultPreheatingTimeMinutes = 60,
                            DefaultCoolingTimeMinutes = 240,
                            DefaultPostProcessingTimeMinutes = 90,
                            SetupTimeBufferMinutes = 30,
                            StandardShiftStart = new TimeSpan(7, 0, 0),
                            StandardShiftEnd = new TimeSpan(15, 0, 0),
                            EveningShiftStart = new TimeSpan(15, 0, 0),
                            EveningShiftEnd = new TimeSpan(23, 0, 0),
                            NightShiftStart = new TimeSpan(23, 0, 0),
                            NightShiftEnd = new TimeSpan(7, 0, 0),
                            EnableWeekendOperations = false,
                            SaturdayOperations = false,
                            SundayOperations = false,
                            TI1MachinePriority = 5,
                            TI2MachinePriority = 5,
                            INCMachinePriority = 5,
                            AllowConcurrentJobs = true,
                            MaxJobsPerMachinePerDay = 8,
                            RequiredOperatorCertification = "SLS Basic",
                            QualityCheckRequired = true,
                            MinimumTimeBetweenJobsMinutes = 15,
                            EmergencyOverrideEnabled = true,
                            NotifyOnScheduleConflicts = true,
                            NotifyOnMaterialChanges = true,
                            AdvanceWarningTimeMinutes = 60,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedDate = DateTime.UtcNow,
                            CreatedBy = "DatabaseFix",
                            LastModifiedBy = "DatabaseFix",
                            ChangeNotes = "Initial settings created by database fix tool"
                        };

                        context.SchedulerSettings.Add(defaultSettings);
                        await context.SaveChangesAsync();
                        Console.WriteLine("? Default settings created successfully!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"? Error checking SchedulerSettings table: {ex.Message}");
                    
                    Console.WriteLine("6. Creating table manually...");
                    await CreateSchedulerSettingsTableManuallyAsync(context);
                }

                Console.WriteLine("\n?? Database fix completed successfully!");
                Console.WriteLine("The application should now work without SchedulerSettings errors.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Fatal error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
        }

        private static async Task CreateSchedulerSettingsTableManuallyAsync(SchedulerContext context)
        {
            try
            {
                var createTableSql = @"
                    CREATE TABLE IF NOT EXISTS ""SchedulerSettings"" (
                        ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_SchedulerSettings"" PRIMARY KEY AUTOINCREMENT,
                        ""TitaniumToTitaniumChangeoverMinutes"" INTEGER NOT NULL DEFAULT 30,
                        ""InconelToInconelChangeoverMinutes"" INTEGER NOT NULL DEFAULT 45,
                        ""CrossMaterialChangeoverMinutes"" INTEGER NOT NULL DEFAULT 120,
                        ""DefaultMaterialChangeoverMinutes"" INTEGER NOT NULL DEFAULT 60,
                        ""DefaultPreheatingTimeMinutes"" INTEGER NOT NULL DEFAULT 60,
                        ""DefaultCoolingTimeMinutes"" INTEGER NOT NULL DEFAULT 240,
                        ""DefaultPostProcessingTimeMinutes"" INTEGER NOT NULL DEFAULT 90,
                        ""SetupTimeBufferMinutes"" INTEGER NOT NULL DEFAULT 30,
                        ""StandardShiftStart"" TEXT NOT NULL DEFAULT '07:00:00',
                        ""StandardShiftEnd"" TEXT NOT NULL DEFAULT '15:00:00',
                        ""EveningShiftStart"" TEXT NOT NULL DEFAULT '15:00:00',
                        ""EveningShiftEnd"" TEXT NOT NULL DEFAULT '23:00:00',
                        ""NightShiftStart"" TEXT NOT NULL DEFAULT '23:00:00',
                        ""NightShiftEnd"" TEXT NOT NULL DEFAULT '07:00:00',
                        ""EnableWeekendOperations"" INTEGER NOT NULL DEFAULT 0,
                        ""SaturdayOperations"" INTEGER NOT NULL DEFAULT 0,
                        ""SundayOperations"" INTEGER NOT NULL DEFAULT 0,
                        ""TI1MachinePriority"" INTEGER NOT NULL DEFAULT 5,
                        ""TI2MachinePriority"" INTEGER NOT NULL DEFAULT 5,
                        ""INCMachinePriority"" INTEGER NOT NULL DEFAULT 5,
                        ""AllowConcurrentJobs"" INTEGER NOT NULL DEFAULT 1,
                        ""MaxJobsPerMachinePerDay"" INTEGER NOT NULL DEFAULT 8,
                        ""RequiredOperatorCertification"" TEXT NOT NULL DEFAULT 'SLS Basic',
                        ""QualityCheckRequired"" INTEGER NOT NULL DEFAULT 1,
                        ""MinimumTimeBetweenJobsMinutes"" INTEGER NOT NULL DEFAULT 15,
                        ""EmergencyOverrideEnabled"" INTEGER NOT NULL DEFAULT 1,
                        ""NotifyOnScheduleConflicts"" INTEGER NOT NULL DEFAULT 1,
                        ""NotifyOnMaterialChanges"" INTEGER NOT NULL DEFAULT 1,
                        ""AdvanceWarningTimeMinutes"" INTEGER NOT NULL DEFAULT 60,
                        ""CreatedDate"" TEXT NOT NULL DEFAULT (datetime('now')),
                        ""LastModifiedDate"" TEXT NOT NULL DEFAULT (datetime('now')),
                        ""CreatedBy"" TEXT NOT NULL DEFAULT 'System',
                        ""LastModifiedBy"" TEXT NOT NULL DEFAULT 'System',
                        ""ChangeNotes"" TEXT NOT NULL DEFAULT 'Default settings initialization'
                    );";

                await context.Database.ExecuteSqlRawAsync(createTableSql);

                var insertDefaultSql = @"
                    INSERT OR IGNORE INTO ""SchedulerSettings"" (
                        ""TitaniumToTitaniumChangeoverMinutes"", ""InconelToInconelChangeoverMinutes"", 
                        ""CrossMaterialChangeoverMinutes"", ""DefaultMaterialChangeoverMinutes"",
                        ""DefaultPreheatingTimeMinutes"", ""DefaultCoolingTimeMinutes"",
                        ""DefaultPostProcessingTimeMinutes"", ""SetupTimeBufferMinutes"",
                        ""StandardShiftStart"", ""StandardShiftEnd"", ""EveningShiftStart"", ""EveningShiftEnd"",
                        ""NightShiftStart"", ""NightShiftEnd"", ""EnableWeekendOperations"", ""SaturdayOperations"",
                        ""SundayOperations"", ""TI1MachinePriority"", ""TI2MachinePriority"", ""INCMachinePriority"",
                        ""AllowConcurrentJobs"", ""MaxJobsPerMachinePerDay"", ""RequiredOperatorCertification"",
                        ""QualityCheckRequired"", ""MinimumTimeBetweenJobsMinutes"", ""EmergencyOverrideEnabled"",
                        ""NotifyOnScheduleConflicts"", ""NotifyOnMaterialChanges"", ""AdvanceWarningTimeMinutes"",
                        ""CreatedBy"", ""LastModifiedBy"", ""ChangeNotes""
                    ) VALUES (
                        30, 45, 120, 60, 60, 240, 90, 30,
                        '07:00:00', '15:00:00', '15:00:00', '23:00:00', '23:00:00', '07:00:00',
                        0, 0, 0, 5, 5, 5, 1, 8, 'SLS Basic', 1, 15, 1, 1, 1, 60,
                        'DatabaseFix', 'DatabaseFix', 'Manual table creation with default settings'
                    );";

                await context.Database.ExecuteSqlRawAsync(insertDefaultSql);

                Console.WriteLine("? SchedulerSettings table created manually with default data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Failed to create table manually: {ex.Message}");
                throw;
            }
        }
    }
}