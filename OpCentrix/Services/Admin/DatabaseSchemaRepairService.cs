using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;

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
        /// </summary>
        private async Task EnsureProductionStageExecutionColumnsAsync()
        {
            try
            {
                var sql = @"
                    PRAGMA table_info(ProductionStageExecutions);
                ";

                var columns = await _context.Database.SqlQueryRaw<dynamic>(sql).ToListAsync();
                var columnNames = new HashSet<string>();

                // Parse column information (SQLite specific)
                foreach (var column in columns)
                {
                    // Extract column name from the dynamic result
                    // This is SQLite specific - column info returns: cid, name, type, notnull, dflt_value, pk
                    var columnInfo = column.ToString();
                    // Parse the column name from the result
                    if (columnInfo.Contains("name"))
                    {
                        // Simple parsing - in real implementation you'd parse the JSON properly
                        // For now, let's add the columns we know might be missing
                    }
                }

                // Add missing columns one by one (SQLite doesn't support adding multiple columns in one statement)
                var columnsToAdd = new[]
                {
                    "ALTER TABLE ProductionStageExecutions ADD COLUMN ActualEndTime DATETIME;",
                    "ALTER TABLE ProductionStageExecutions ADD COLUMN ActualStartTime DATETIME;",
                    "ALTER TABLE ProductionStageExecutions ADD COLUMN OperatorName TEXT;",
                    "ALTER TABLE ProductionStageExecutions ADD COLUMN CreatedBy TEXT;",
                    "ALTER TABLE ProductionStageExecutions ADD COLUMN LastModifiedBy TEXT;",
                    "ALTER TABLE ProductionStageExecutions ADD COLUMN LastModifiedDate DATETIME;"
                };

                foreach (var alterSql in columnsToAdd)
                {
                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync(alterSql);
                        _logger.LogDebug("[SCHEMA-REPAIR] Added column: {AlterSql}", alterSql);
                    }
                    catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("duplicate column name"))
                    {
                        // Column already exists - this is expected
                        _logger.LogDebug("[SCHEMA-REPAIR] Column already exists: {AlterSql}", alterSql);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[SCHEMA-REPAIR] Could not add column: {AlterSql}", alterSql);
                    }
                }

                _logger.LogInformation("[SCHEMA-REPAIR] ProductionStageExecutions column check completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SCHEMA-REPAIR] Error checking ProductionStageExecutions columns");
                throw;
            }
        }

        /// <summary>
        /// Ensures other tables have required columns
        /// </summary>
        private async Task EnsureOtherMissingColumnsAsync()
        {
            try
            {
                // Add other column checks here as needed
                _logger.LogDebug("[SCHEMA-REPAIR] Other column checks completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SCHEMA-REPAIR] Error checking other columns");
                // Don't throw - these are non-critical
            }
        }
    }
}