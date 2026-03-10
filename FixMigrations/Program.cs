// Simple tool to run SQL migrations fix
// Run with: dotnet run --project FixMigrations

using Microsoft.Data.Sqlite;

var dbPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "OpCentrix", "scheduler.db");
if (!File.Exists(dbPath))
{
    dbPath = "OpCentrix/scheduler.db";
}

Console.WriteLine($"Database path: {Path.GetFullPath(dbPath)}");

var migrations = new[]
{
    "20250722031833_AddJobLogEntry",
    "20250725221757_AdminControlSystemModels",
    "20250726023548_FixJobStageMachineRelationship",
    "20250726135340_AddMaterialsTable",
    "20250726141952_MultiStageSchedulingEntities",
    "20250726143534_AddSchedulerOrientationToUserSettings",
    "20250727124109_FixPartsTableSchema",
    "20250727134953_MakeAdminOverrideReasonNullable",
    "20250727141323_FixAdminOverrideFieldsNullability",
    "20250729205335_AddCorePerformanceIndexes",
    "20250729235317_FixRolePermissionKeys",
    "20250730015507_AddBTIndustrySpecialization",
    "20250730024931_OptimizeBTPartsIndexes",
    "20250730145024_BTPartsSystemEnhancement",
    "20250730162329_AddPrototypeTrackingSystem",
    "20250731160157_AddEDMLogEntity",
    "20250731181242_AddBugReportingSystem",
    "20250801163504_DatabaseRefactoringComplete",
    "20250801164030_FixEFRelationshipConflicts",
    "20250801170930_AddPartStageRequirementTable",
    "20250802122703_AddCustomFieldsToProductionStages",
    "20250803003059_AddWorkflowFields",
    "20250803124455_AddAdvancedStageManagementTables",
    "20250803173130_EnhancedBuildJobTimeTracking",
    "20250803190558_AddPhase4LearningModelTables",
    "20250924164205_AddMachineColorHexMinimal",
    "20250924164751_AddMachineColorHexProper",
    "20250927145033_FixActualEndTimeColumn",
    "20250928171551_Add_SLSBuildModeFields_To_MasterPart",
    "20250930160505_Add_JobStackFields",
    "20250930160805_Add_JobStatusHistory_And_StackDurationLearning",
    "20251002175517_TempVerify",
    "20260112154500_AddCrmV1",
    "20260112161000_AddCrmTaskUserRelationships",
    // Mark our new migrations as applied too (we'll create tables manually)
    "20260310200600_AddMachineProviderTables",
    "20260310200624_AddStageLearningFields"
};

try
{
    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();
    
    // Create migrations history table if not exists
    using (var cmd = connection.CreateCommand())
    {
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                ""MigrationId"" TEXT NOT NULL CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY,
                ""ProductVersion"" TEXT NOT NULL
            );";
        cmd.ExecuteNonQuery();
    }
    
    Console.WriteLine("Created/verified __EFMigrationsHistory table");
    
    // Insert each migration
    int inserted = 0;
    foreach (var migration in migrations)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"INSERT OR IGNORE INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"") VALUES ($mid, '8.0.0')";
        cmd.Parameters.AddWithValue("$mid", migration);
        var result = cmd.ExecuteNonQuery();
        if (result > 0)
        {
            Console.WriteLine($"  + {migration}");
            inserted++;
        }
    }
    
    Console.WriteLine($"\nMarked {inserted} migrations as applied");
    
    // Now create the missing tables
    Console.WriteLine("\n--- Creating missing tables ---");
    
    // MachineConnectionSettings
    using (var cmd = connection.CreateCommand())
    {
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='MachineConnectionSettings'";
        if (cmd.ExecuteScalar() == null)
        {
            Console.WriteLine("Creating MachineConnectionSettings...");
            cmd.CommandText = @"
                CREATE TABLE ""MachineConnectionSettings"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_MachineConnectionSettings"" PRIMARY KEY AUTOINCREMENT,
                    ""MachineId"" INTEGER NOT NULL,
                    ""ProviderType"" TEXT NOT NULL DEFAULT 'Mock',
                    ""ConnectionConfigJson"" TEXT DEFAULT '{}',
                    ""IsEnabled"" INTEGER NOT NULL DEFAULT 1,
                    ""PollIntervalSeconds"" INTEGER NOT NULL DEFAULT 30,
                    ""LastConnectedAt"" TEXT NULL,
                    ""LastError"" TEXT NULL,
                    ""ConsecutiveFailures"" INTEGER NOT NULL DEFAULT 0,
                    ""MaxConsecutiveFailures"" INTEGER NOT NULL DEFAULT 10,
                    ""CreatedDate"" TEXT NOT NULL DEFAULT (datetime('now')),
                    ""LastModifiedDate"" TEXT NOT NULL DEFAULT (datetime('now')),
                    ""CreatedBy"" TEXT NOT NULL DEFAULT 'System',
                    ""LastModifiedBy"" TEXT NOT NULL DEFAULT 'System',
                    CONSTRAINT ""FK_MachineConnectionSettings_Machines_MachineId"" FOREIGN KEY (""MachineId"") REFERENCES ""Machines"" (""Id"") ON DELETE CASCADE
                )";
            cmd.ExecuteNonQuery();
            
            cmd.CommandText = @"CREATE UNIQUE INDEX ""IX_MachineConnectionSettings_MachineId"" ON ""MachineConnectionSettings"" (""MachineId"")";
            cmd.ExecuteNonQuery();
            Console.WriteLine("  Created MachineConnectionSettings");
        }
        else
        {
            Console.WriteLine("  MachineConnectionSettings exists");
        }
    }
    
    // MachineStateRecords
    using (var cmd = connection.CreateCommand())
    {
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='MachineStateRecords'";
        if (cmd.ExecuteScalar() == null)
        {
            Console.WriteLine("Creating MachineStateRecords...");
            cmd.CommandText = @"
                CREATE TABLE ""MachineStateRecords"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_MachineStateRecords"" PRIMARY KEY AUTOINCREMENT,
                    ""MachineId"" INTEGER NOT NULL,
                    ""Status"" TEXT NOT NULL DEFAULT 'Unknown',
                    ""BuildProgressPercent"" REAL NULL,
                    ""CurrentJobReference"" TEXT NULL,
                    ""IsConnected"" INTEGER NOT NULL DEFAULT 1,
                    ""AlarmsSnapshot"" TEXT NULL,
                    ""StateDataJson"" TEXT NULL,
                    ""ProviderType"" TEXT DEFAULT 'Unknown',
                    ""IsStateChange"" INTEGER NOT NULL DEFAULT 0,
                    ""PreviousStatus"" TEXT NULL,
                    ""Timestamp"" TEXT NOT NULL DEFAULT (datetime('now')),
                    CONSTRAINT ""FK_MachineStateRecords_Machines_MachineId"" FOREIGN KEY (""MachineId"") REFERENCES ""Machines"" (""Id"") ON DELETE CASCADE
                )";
            cmd.ExecuteNonQuery();
            
            cmd.CommandText = @"CREATE INDEX ""IX_MachineStateRecords_MachineId"" ON ""MachineStateRecords"" (""MachineId"")";
            cmd.ExecuteNonQuery();
            cmd.CommandText = @"CREATE INDEX ""IX_MachineStateRecords_Timestamp"" ON ""MachineStateRecords"" (""Timestamp"")";
            cmd.ExecuteNonQuery();
            Console.WriteLine("  Created MachineStateRecords");
        }
        else
        {
            Console.WriteLine("  MachineStateRecords exists");
        }
    }
    
    // Add learning fields to PartStageRequirements
    Console.WriteLine("\n--- Adding learning fields to PartStageRequirements ---");
    var learningColumns = new (string name, string def)[]
    {
        ("ActualAverageDurationHours", "REAL NULL"),
        ("ActualSampleCount", "INTEGER NOT NULL DEFAULT 0"),
        ("LastActualDurationHours", "REAL NULL"),
        ("EstimateSource", "TEXT DEFAULT 'Manual'"),
        ("EstimateLastUpdated", "TEXT NULL")
    };
    
    foreach (var (colName, colDef) in learningColumns)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('PartStageRequirements') WHERE name='{colName}'";
        var exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        
        if (!exists)
        {
            try
            {
                cmd.CommandText = $@"ALTER TABLE ""PartStageRequirements"" ADD COLUMN ""{colName}"" {colDef}";
                cmd.ExecuteNonQuery();
                Console.WriteLine($"  Added {colName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Skipped {colName}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"  {colName} exists");
        }
    }
    
    // Verify final state
    using (var cmd = connection.CreateCommand())
    {
        cmd.CommandText = @"SELECT COUNT(*) FROM ""__EFMigrationsHistory""";
        var count = cmd.ExecuteScalar();
        Console.WriteLine($"\n? Total migrations in history: {count}");
    }
    
    Console.WriteLine("\n? Database synchronized successfully!");
    Console.WriteLine("\nVerify with: dotnet ef migrations list -p OpCentrix -s OpCentrix --context SchedulerContext");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    return 1;
}

return 0;
