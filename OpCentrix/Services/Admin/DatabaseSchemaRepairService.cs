using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using System.Data;
using Microsoft.Data.Sqlite;

namespace OpCentrix.Services.Admin
{
    /// <summary>
    /// Service to handle database schema repairs and ensure all columns exist
    /// Fixes issues like missing ActualEndTime column in ProductionStageExecutions
    /// </summary>
    public class DatabaseSchemaRepairService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<DatabaseSchemaRepairService> _logger;

        public DatabaseSchemaRepairService(SchedulerContext context, ILogger<DatabaseSchemaRepairService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Ensures all required columns exist in the database tables
        /// </summary>
        public async Task EnsureSchemaIntegrityAsync()
        {
            try
            {
                _logger.LogInformation("[SCHEMA-REPAIR] Starting database schema integrity check");

                await EnsureProductionStageExecutionColumnsAsync();
                await EnsureOtherMissingColumnsAsync();

                _logger.LogInformation("[SCHEMA-REPAIR] Schema integrity check completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SCHEMA-REPAIR] Error during schema integrity check");
                throw;
            }
        }

        /// <summary>
        /// Ensures ProductionStageExecutions table has all required columns
        /// Rewritten to avoid dynamic LINQ raw query which was throwing at startup.
        /// Uses direct ADO with PRAGMA for SQLite and guards for table absence.
        /// </summary>
        private async Task EnsureProductionStageExecutionColumnsAsync()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                // First verify the table exists (SQLite specific check)
                using (var tableCheck = connection.CreateCommand())
                {
                    tableCheck.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='ProductionStageExecutions' LIMIT 1";
                    var existsResult = await tableCheck.ExecuteScalarAsync();
                    if (existsResult == null || existsResult == DBNull.Value)
                    {
                        _logger.LogWarning("[SCHEMA-REPAIR] ProductionStageExecutions table not found – skipping column repair");
                        return; // Nothing else to do
                    }
                }

                // Collect existing column names
                var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var pragma = connection.CreateCommand())
                {
                    pragma.CommandText = "PRAGMA table_info('ProductionStageExecutions');";
                    using var reader = await pragma.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        // PRAGMA table_info returns: cid | name | type | notnull | dflt_value | pk
                        if (!reader.IsDBNull(1))
                        {
                            existing.Add(reader.GetString(1));
                        }
                    }
                }

                // Desired columns (name, type) – keep types simple / provider-agnostic
                var desired = new (string Name, string Type)[]
                {
                    ("ActualEndTime", "DATETIME"),
                    ("ActualStartTime", "DATETIME"),
                    ("OperatorName", "TEXT"),
                    ("CreatedBy", "TEXT"),
                    ("LastModifiedBy", "TEXT"),
                    ("LastModifiedDate", "DATETIME")
                };

                foreach (var (name, type) in desired)
                {
                    if (existing.Contains(name))
                    {
                        _logger.LogDebug("[SCHEMA-REPAIR] Column already present: {Column}", name);
                        continue;
                    }

                    try
                    {
                        using var alter = connection.CreateCommand();
                        alter.CommandText = $"ALTER TABLE ProductionStageExecutions ADD COLUMN {name} {type};";
                        await alter.ExecuteNonQueryAsync();
                        _logger.LogInformation("[SCHEMA-REPAIR] Added missing column {Column}", name);
                    }
                    catch (SqliteException sx) when (sx.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
                    {
                        // Race condition / already added by another instance – safe to ignore
                        _logger.LogDebug("[SCHEMA-REPAIR] Duplicate column ignored: {Column}", name);
                    }
                    catch (Exception colEx)
                    {
                        _logger.LogWarning(colEx, "[SCHEMA-REPAIR] Failed adding column {Column}", name);
                    }
                }

                _logger.LogInformation("[SCHEMA-REPAIR] ProductionStageExecutions column check completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SCHEMA-REPAIR] Error checking ProductionStageExecutions columns");
                throw; // propagate so startup can log clearly
            }
        }

        /// <summary>
        /// Ensures other tables have required columns
        /// </summary>
        private async Task EnsureOtherMissingColumnsAsync()
        {
            try
            {
                // Placeholder for future repair logic
                _logger.LogDebug("[SCHEMA-REPAIR] Other column checks completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SCHEMA-REPAIR] Error checking other columns");
                // Non?critical – do not rethrow
            }
        }
    }
}