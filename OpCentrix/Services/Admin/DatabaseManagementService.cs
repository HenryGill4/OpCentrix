using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using System.Text.Json;
using System.IO.Compression;
using Microsoft.Data.Sqlite;
using System.Text;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for database export, import, and diagnostics
/// Task 16: Database Export and Diagnostics
/// </summary>
public interface IDatabaseManagementService
{
    Task<DatabaseStatusResult> GetDatabaseStatusAsync();
    Task<DatabaseExportResult> ExportDatabaseAsync(DatabaseExportOptions options);
    Task<DatabaseImportResult> ImportDatabaseAsync(Stream fileStream, DatabaseImportOptions options);
    Task<SchemaValidationResult> ValidateSchemaAsync();
    Task<IntegrityCheckResult> CheckDatabaseIntegrityAsync();
    Task<DatabaseBackupResult> BackupDatabaseAsync(string backupPath);
    Task<DatabaseRestoreResult> RestoreDatabaseAsync(string backupPath, bool overwriteExisting);
    Task<List<string>> GetMigrationHistoryAsync();
    Task<DatabaseOptimizationResult> OptimizeDatabaseAsync();
}

public class DatabaseManagementService : IDatabaseManagementService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<DatabaseManagementService> _logger;
    private readonly IConfiguration _configuration;

    public DatabaseManagementService(
        SchedulerContext context,
        ILogger<DatabaseManagementService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<DatabaseStatusResult> GetDatabaseStatusAsync()
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("?? [DB-{OperationId}] Getting database status", operationId);

        try
        {
            var result = new DatabaseStatusResult
            {
                IsConnected = await _context.Database.CanConnectAsync(),
                ConnectionString = _context.Database.GetConnectionString() ?? "",
                DatabaseName = GetDatabaseName(),
                CreatedDate = await GetDatabaseCreatedDateAsync(),
                LastModified = await GetDatabaseLastModifiedAsync(),
                FileSize = await GetDatabaseFileSizeAsync(),
                TableCount = await GetTableCountAsync(),
                RecordCounts = await GetRecordCountsAsync(),
                IndexCount = await GetIndexCountAsync(),
                DatabaseProvider = _context.Database.ProviderName ?? "Unknown",
                MigrationHistory = await GetMigrationHistoryAsync()
            };

            _logger.LogInformation("? [DB-{OperationId}] Database status retrieved - Connected: {IsConnected}, Size: {FileSize} MB", 
                operationId, result.IsConnected, result.FileSize);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [DB-{OperationId}] Error getting database status: {ErrorMessage}", 
                operationId, ex.Message);
            
            return new DatabaseStatusResult
            {
                IsConnected = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<DatabaseExportResult> ExportDatabaseAsync(DatabaseExportOptions options)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("?? [DB-{OperationId}] Starting database export - Format: {Format}, Tables: {TableCount}", 
            operationId, options.ExportFormat, options.TablesToExport?.Count ?? 0);

        try
        {
            var exportData = new Dictionary<string, object>();
            var exportStats = new Dictionary<string, int>();

            // Export metadata
            exportData["ExportMetadata"] = new
            {
                ExportDate = DateTime.UtcNow,
                DatabaseVersion = await GetDatabaseVersionAsync(),
                ApplicationVersion = "OpCentrix v3.0",
                Options = options
            };

            // Export tables based on options
            if (options.IncludeUsers)
            {
                var users = await _context.Users.Include(u => u.Settings).ToListAsync();
                exportData["Users"] = users;
                exportStats["Users"] = users.Count;
            }

            if (options.IncludeMachines)
            {
                var machines = await _context.Machines.Include(m => m.Capabilities).ToListAsync();
                exportData["Machines"] = machines;
                exportStats["Machines"] = machines.Count;
            }

            if (options.IncludeParts)
            {
                var parts = await _context.Parts.ToListAsync();
                exportData["Parts"] = parts;
                exportStats["Parts"] = parts.Count;
            }

            if (options.IncludeJobs)
            {
                var jobs = await _context.Jobs.Include(j => j.Part).ToListAsync();
                exportData["Jobs"] = jobs;
                exportStats["Jobs"] = jobs.Count;
            }

            if (options.IncludeSystemSettings)
            {
                var settings = await _context.SystemSettings.ToListAsync();
                exportData["SystemSettings"] = settings;
                exportStats["SystemSettings"] = settings.Count;
            }

            if (options.IncludeOperatingShifts)
            {
                var shifts = await _context.OperatingShifts.ToListAsync();
                exportData["OperatingShifts"] = shifts;
                exportStats["OperatingShifts"] = shifts.Count;
            }

            if (options.IncludeRolePermissions)
            {
                var permissions = await _context.RolePermissions.ToListAsync();
                exportData["RolePermissions"] = permissions;
                exportStats["RolePermissions"] = permissions.Count;
            }

            // Serialize to JSON
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            var jsonData = JsonSerializer.Serialize(exportData, jsonOptions);
            var fileData = Encoding.UTF8.GetBytes(jsonData);

            // Compress if requested
            if (options.CompressOutput)
            {
                fileData = await CompressDataAsync(fileData);
            }

            var result = new DatabaseExportResult
            {
                Success = true,
                FileData = fileData,
                FileName = $"opcentrix-export-{DateTime.Now:yyyyMMdd-HHmmss}.{(options.CompressOutput ? "zip" : "json")}",
                ExportStats = exportStats,
                FileSizeBytes = fileData.Length,
                ExportDate = DateTime.UtcNow
            };

            _logger.LogInformation("? [DB-{OperationId}] Database export completed - File size: {FileSize} KB, Tables: {TableCount}", 
                operationId, result.FileSizeBytes / 1024, exportStats.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [DB-{OperationId}] Error exporting database: {ErrorMessage}", 
                operationId, ex.Message);
            
            return new DatabaseExportResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<DatabaseImportResult> ImportDatabaseAsync(Stream fileStream, DatabaseImportOptions options)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("?? [DB-{OperationId}] Starting database import - Overwrite: {OverwriteExisting}", 
            operationId, options.OverwriteExisting);

        try
        {
            // Read and decompress if needed
            var fileData = await ReadStreamAsync(fileStream);
            
            if (options.IsCompressed)
            {
                fileData = await DecompressDataAsync(fileData);
            }

            var jsonData = Encoding.UTF8.GetString(fileData);
            var importData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);

            if (importData == null)
            {
                throw new InvalidOperationException("Invalid import file format");
            }

            var importStats = new Dictionary<string, int>();
            var errors = new List<string>();

            // Validate metadata
            if (importData.ContainsKey("ExportMetadata"))
            {
                _logger.LogInformation("?? [DB-{OperationId}] Import file validated - contains metadata", operationId);
            }

            // Import tables based on options
            if (options.ImportUsers && importData.ContainsKey("Users"))
            {
                var count = await ImportUsersAsync(importData["Users"], options.OverwriteExisting);
                importStats["Users"] = count;
            }

            if (options.ImportMachines && importData.ContainsKey("Machines"))
            {
                var count = await ImportMachinesAsync(importData["Machines"], options.OverwriteExisting);
                importStats["Machines"] = count;
            }

            if (options.ImportParts && importData.ContainsKey("Parts"))
            {
                var count = await ImportPartsAsync(importData["Parts"], options.OverwriteExisting);
                importStats["Parts"] = count;
            }

            if (options.ImportJobs && importData.ContainsKey("Jobs"))
            {
                var count = await ImportJobsAsync(importData["Jobs"], options.OverwriteExisting);
                importStats["Jobs"] = count;
            }

            if (options.ImportSystemSettings && importData.ContainsKey("SystemSettings"))
            {
                var count = await ImportSystemSettingsAsync(importData["SystemSettings"], options.OverwriteExisting);
                importStats["SystemSettings"] = count;
            }

            await _context.SaveChangesAsync();

            var result = new DatabaseImportResult
            {
                Success = true,
                ImportStats = importStats,
                Errors = errors,
                ImportDate = DateTime.UtcNow
            };

            _logger.LogInformation("? [DB-{OperationId}] Database import completed - Tables imported: {TableCount}, Records: {RecordCount}", 
                operationId, importStats.Count, importStats.Values.Sum());

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [DB-{OperationId}] Error importing database: {ErrorMessage}", 
                operationId, ex.Message);
            
            return new DatabaseImportResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<SchemaValidationResult> ValidateSchemaAsync()
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("?? [DB-{OperationId}] Starting schema validation", operationId);

        try
        {
            var issues = new List<string>();
            var warnings = new List<string>();

            // Check if database exists and can connect
            if (!await _context.Database.CanConnectAsync())
            {
                issues.Add("Cannot connect to database");
                return new SchemaValidationResult
                {
                    IsValid = false,
                    Issues = issues,
                    Warnings = warnings
                };
            }

            // Check for pending migrations
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                warnings.Add($"Pending migrations: {string.Join(", ", pendingMigrations)}");
            }

            // Check essential tables exist
            var requiredTables = new[]
            {
                "Users", "Machines", "Parts", "Jobs", "SystemSettings",
                "OperatingShifts", "RolePermissions", "MachineCapabilities"
            };

            foreach (var table in requiredTables)
            {
                var exists = await TableExistsAsync(table);
                if (!exists)
                {
                    issues.Add($"Required table '{table}' does not exist");
                }
            }

            // Check foreign key constraints
            var constraintIssues = await ValidateForeignKeyConstraintsAsync();
            issues.AddRange(constraintIssues);

            // Check for orphaned records
            var orphanedRecords = await FindOrphanedRecordsAsync();
            warnings.AddRange(orphanedRecords);

            var result = new SchemaValidationResult
            {
                IsValid = issues.Count == 0,
                Issues = issues,
                Warnings = warnings,
                ValidationDate = DateTime.UtcNow
            };

            _logger.LogInformation("? [DB-{OperationId}] Schema validation completed - Valid: {IsValid}, Issues: {IssueCount}, Warnings: {WarningCount}", 
                operationId, result.IsValid, issues.Count, warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [DB-{OperationId}] Error validating schema: {ErrorMessage}", 
                operationId, ex.Message);
            
            return new SchemaValidationResult
            {
                IsValid = false,
                Issues = new List<string> { ex.Message },
                Warnings = new List<string>(),
                ValidationDate = DateTime.UtcNow
            };
        }
    }

    public async Task<IntegrityCheckResult> CheckDatabaseIntegrityAsync()
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("?? [DB-{OperationId}] Starting database integrity check", operationId);

        try
        {
            var issues = new List<string>();
            var statistics = new Dictionary<string, object>();

            // SQLite PRAGMA integrity_check
            var connectionString = _context.Database.GetConnectionString();
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA integrity_check";
            var integrityResult = await command.ExecuteScalarAsync() as string;

            if (integrityResult != "ok")
            {
                issues.Add($"SQLite integrity check failed: {integrityResult}");
            }

            // Check for data consistency issues
            var dataIssues = await CheckDataConsistencyAsync();
            issues.AddRange(dataIssues);

            // Gather statistics
            statistics["TotalRecords"] = await GetTotalRecordCountAsync();
            statistics["DatabaseSizeMB"] = await GetDatabaseFileSizeAsync();
            statistics["TableCount"] = await GetTableCountAsync();
            statistics["IndexCount"] = await GetIndexCountAsync();

            var result = new IntegrityCheckResult
            {
                IsHealthy = issues.Count == 0,
                Issues = issues,
                Statistics = statistics,
                CheckDate = DateTime.UtcNow
            };

            _logger.LogInformation("? [DB-{OperationId}] Database integrity check completed - Healthy: {IsHealthy}, Issues: {IssueCount}", 
                operationId, result.IsHealthy, issues.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [DB-{OperationId}] Error checking database integrity: {ErrorMessage}", 
                operationId, ex.Message);
            
            return new IntegrityCheckResult
            {
                IsHealthy = false,
                Issues = new List<string> { ex.Message },
                Statistics = new Dictionary<string, object>(),
                CheckDate = DateTime.UtcNow
            };
        }
    }

    public async Task<DatabaseBackupResult> BackupDatabaseAsync(string backupPath)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("?? [DB-{OperationId}] Starting database backup to: {BackupPath}", 
            operationId, backupPath);

        try
        {
            var connectionString = _context.Database.GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Database connection string is null or empty");
            }

            // For SQLite, we can copy the database file
            var sourceDb = GetDatabaseFilePath(connectionString);
            if (!File.Exists(sourceDb))
            {
                throw new FileNotFoundException($"Database file not found: {sourceDb}");
            }

            // Ensure backup directory exists
            var backupDir = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrEmpty(backupDir) && !Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            // Copy database file
            File.Copy(sourceDb, backupPath, overwrite: true);

            // Verify backup
            var backupSize = new FileInfo(backupPath).Length;
            var originalSize = new FileInfo(sourceDb).Length;

            if (backupSize != originalSize)
            {
                throw new InvalidOperationException("Backup verification failed: file sizes do not match");
            }

            var result = new DatabaseBackupResult
            {
                Success = true,
                BackupPath = backupPath,
                BackupSizeBytes = backupSize,
                BackupDate = DateTime.UtcNow
            };

            _logger.LogInformation("? [DB-{OperationId}] Database backup completed - Size: {BackupSize} KB", 
                operationId, backupSize / 1024);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [DB-{OperationId}] Error backing up database: {ErrorMessage}", 
                operationId, ex.Message);
            
            return new DatabaseBackupResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<DatabaseRestoreResult> RestoreDatabaseAsync(string backupPath, bool overwriteExisting)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("?? [DB-{OperationId}] Starting database restore from: {BackupPath}, Overwrite: {OverwriteExisting}", 
            operationId, backupPath, overwriteExisting);

        try
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException($"Backup file not found: {backupPath}");
            }

            var connectionString = _context.Database.GetConnectionString();
            var targetDb = GetDatabaseFilePath(connectionString!);

            if (File.Exists(targetDb) && !overwriteExisting)
            {
                throw new InvalidOperationException("Target database exists and overwrite is not allowed");
            }

            // Close any existing connections
            await _context.Database.CloseConnectionAsync();

            // Copy backup file to database location
            File.Copy(backupPath, targetDb, overwrite: true);

            // Verify restore
            var restoredSize = new FileInfo(targetDb).Length;
            var backupSize = new FileInfo(backupPath).Length;

            if (restoredSize != backupSize)
            {
                throw new InvalidOperationException("Restore verification failed: file sizes do not match");
            }

            // Test connection to restored database
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                throw new InvalidOperationException("Cannot connect to restored database");
            }

            var result = new DatabaseRestoreResult
            {
                Success = true,
                RestoredSizeBytes = restoredSize,
                RestoreDate = DateTime.UtcNow
            };

            _logger.LogInformation("? [DB-{OperationId}] Database restore completed - Size: {RestoredSize} KB", 
                operationId, restoredSize / 1024);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [DB-{OperationId}] Error restoring database: {ErrorMessage}", 
                operationId, ex.Message);
            
            return new DatabaseRestoreResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<string>> GetMigrationHistoryAsync()
    {
        try
        {
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
            return appliedMigrations.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting migration history");
            return new List<string>();
        }
    }

    public async Task<DatabaseOptimizationResult> OptimizeDatabaseAsync()
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("?? [DB-{OperationId}] Starting database optimization", operationId);

        try
        {
            var beforeSize = await GetDatabaseFileSizeAsync();
            var optimizationSteps = new List<string>();

            var connectionString = _context.Database.GetConnectionString();
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            // Run VACUUM to reclaim space
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "VACUUM";
                await command.ExecuteNonQueryAsync();
                optimizationSteps.Add("VACUUM completed");
            }

            // Analyze tables for query optimization
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "ANALYZE";
                await command.ExecuteNonQueryAsync();
                optimizationSteps.Add("ANALYZE completed");
            }

            // Update statistics
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA optimize";
                await command.ExecuteNonQueryAsync();
                optimizationSteps.Add("PRAGMA optimize completed");
            }

            var afterSize = await GetDatabaseFileSizeAsync();
            var spaceSaved = beforeSize - afterSize;

            var result = new DatabaseOptimizationResult
            {
                Success = true,
                BeforeSizeMB = beforeSize,
                AfterSizeMB = afterSize,
                SpaceSavedMB = spaceSaved,
                OptimizationSteps = optimizationSteps,
                OptimizationDate = DateTime.UtcNow
            };

            _logger.LogInformation("? [DB-{OperationId}] Database optimization completed - Space saved: {SpaceSaved} MB", 
                operationId, spaceSaved);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [DB-{OperationId}] Error optimizing database: {ErrorMessage}", 
                operationId, ex.Message);
            
            return new DatabaseOptimizationResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    #region Private Helper Methods

    private string GetDatabaseName()
    {
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            return Path.GetFileNameWithoutExtension(GetDatabaseFilePath(connectionString!));
        }
        catch
        {
            return "Unknown";
        }
    }

    private string GetDatabaseFilePath(string connectionString)
    {
        // Extract file path from SQLite connection string
        var builder = new SqliteConnectionStringBuilder(connectionString);
        return builder.DataSource;
    }

    private async Task<DateTime?> GetDatabaseCreatedDateAsync()
    {
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            var dbPath = GetDatabaseFilePath(connectionString!);
            if (File.Exists(dbPath))
            {
                return File.GetCreationTime(dbPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get database creation date");
        }
        return null;
    }

    private async Task<DateTime?> GetDatabaseLastModifiedAsync()
    {
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            var dbPath = GetDatabaseFilePath(connectionString!);
            if (File.Exists(dbPath))
            {
                return File.GetLastWriteTime(dbPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get database last modified date");
        }
        return null;
    }

    private async Task<double> GetDatabaseFileSizeAsync()
    {
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            var dbPath = GetDatabaseFilePath(connectionString!);
            if (File.Exists(dbPath))
            {
                var fileInfo = new FileInfo(dbPath);
                return Math.Round(fileInfo.Length / (1024.0 * 1024.0), 2); // MB
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get database file size");
        }
        return 0;
    }

    private async Task<int> GetTableCountAsync()
    {
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get table count");
            return 0;
        }
    }

    private async Task<Dictionary<string, int>> GetRecordCountsAsync()
    {
        var counts = new Dictionary<string, int>();
        
        try
        {
            counts["Users"] = await _context.Users.CountAsync();
            counts["Machines"] = await _context.Machines.CountAsync();
            counts["Parts"] = await _context.Parts.CountAsync();
            counts["Jobs"] = await _context.Jobs.CountAsync();
            counts["SystemSettings"] = await _context.SystemSettings.CountAsync();
            counts["OperatingShifts"] = await _context.OperatingShifts.CountAsync();
            counts["RolePermissions"] = await _context.RolePermissions.CountAsync();
            counts["MachineCapabilities"] = await _context.MachineCapabilities.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get complete record counts");
        }

        return counts;
    }

    private async Task<int> GetIndexCountAsync()
    {
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name NOT LIKE 'sqlite_%'";
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get index count");
            return 0;
        }
    }

    private async Task<string> GetDatabaseVersionAsync()
    {
        try
        {
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
            return appliedMigrations.LastOrDefault() ?? "Unknown";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get database version");
            return "Unknown";
        }
    }

    private async Task<byte[]> CompressDataAsync(byte[] data)
    {
        using var memoryStream = new MemoryStream();
        using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var entry = zipArchive.CreateEntry("data.json");
            using var entryStream = entry.Open();
            await entryStream.WriteAsync(data, 0, data.Length);
        }
        return memoryStream.ToArray();
    }

    private async Task<byte[]> DecompressDataAsync(byte[] compressedData)
    {
        using var memoryStream = new MemoryStream(compressedData);
        using var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
        var entry = zipArchive.Entries.First();
        using var entryStream = entry.Open();
        using var resultStream = new MemoryStream();
        await entryStream.CopyToAsync(resultStream);
        return resultStream.ToArray();
    }

    private async Task<byte[]> ReadStreamAsync(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    private async Task<int> ImportUsersAsync(JsonElement usersElement, bool overwriteExisting)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var users = JsonSerializer.Deserialize<List<User>>(usersElement.GetRawText(), options) ?? new List<User>();
        
        int count = 0;
        foreach (var user in users)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existing == null)
            {
                _context.Users.Add(user);
                count++;
            }
            else if (overwriteExisting)
            {
                existing.FullName = user.FullName;
                existing.Email = user.Email;
                existing.Role = user.Role;
                existing.Department = user.Department;
                existing.IsActive = user.IsActive;
                existing.LastModifiedDate = DateTime.UtcNow;
                count++;
            }
        }
        
        return count;
    }

    private async Task<int> ImportMachinesAsync(JsonElement machinesElement, bool overwriteExisting)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var machines = JsonSerializer.Deserialize<List<Machine>>(machinesElement.GetRawText(), options) ?? new List<Machine>();
        
        int count = 0;
        foreach (var machine in machines)
        {
            var existing = await _context.Machines.FirstOrDefaultAsync(m => m.MachineId == machine.MachineId);
            if (existing == null)
            {
                _context.Machines.Add(machine);
                count++;
            }
            else if (overwriteExisting)
            {
                existing.Name = machine.Name;
                existing.MachineType = machine.MachineType;
                existing.Status = machine.Status;
                existing.IsActive = machine.IsActive;
                existing.LastModifiedDate = DateTime.UtcNow;
                count++;
            }
        }
        
        return count;
    }

    private async Task<int> ImportPartsAsync(JsonElement partsElement, bool overwriteExisting)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var parts = JsonSerializer.Deserialize<List<Part>>(partsElement.GetRawText(), options) ?? new List<Part>();
        
        int count = 0;
        foreach (var part in parts)
        {
            var existing = await _context.Parts.FirstOrDefaultAsync(p => p.PartNumber == part.PartNumber);
            if (existing == null)
            {
                _context.Parts.Add(part);
                count++;
            }
            else if (overwriteExisting)
            {
                existing.Name = part.Name;
                existing.Description = part.Description;
                existing.Material = part.Material;
                existing.EstimatedHours = part.EstimatedHours;
                existing.LastModifiedDate = DateTime.UtcNow;
                count++;
            }
        }
        
        return count;
    }

    private async Task<int> ImportJobsAsync(JsonElement jobsElement, bool overwriteExisting)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var jobs = JsonSerializer.Deserialize<List<Job>>(jobsElement.GetRawText(), options) ?? new List<Job>();
        
        int count = 0;
        foreach (var job in jobs)
        {
            var existing = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == job.Id);
            if (existing == null)
            {
                _context.Jobs.Add(job);
                count++;
            }
            else if (overwriteExisting)
            {
                existing.PartNumber = job.PartNumber;
                existing.Quantity = job.Quantity;
                existing.Status = job.Status;
                existing.ScheduledStart = job.ScheduledStart;
                existing.ScheduledEnd = job.ScheduledEnd;
                existing.LastModifiedDate = DateTime.UtcNow;
                count++;
            }
        }
        
        return count;
    }

    private async Task<int> ImportSystemSettingsAsync(JsonElement settingsElement, bool overwriteExisting)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var settings = JsonSerializer.Deserialize<List<SystemSetting>>(settingsElement.GetRawText(), options) ?? new List<SystemSetting>();
        
        int count = 0;
        foreach (var setting in settings)
        {
            var existing = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == setting.SettingKey);
            if (existing == null)
            {
                _context.SystemSettings.Add(setting);
                count++;
            }
            else if (overwriteExisting)
            {
                existing.SettingValue = setting.SettingValue;
                existing.LastModifiedDate = DateTime.UtcNow;
                count++;
            }
        }
        
        return count;
    }

    private async Task<bool> TableExistsAsync(string tableName)
    {
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@tableName";
            command.Parameters.AddWithValue("@tableName", tableName);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking if table {TableName} exists", tableName);
            return false;
        }
    }

    private async Task<List<string>> ValidateForeignKeyConstraintsAsync()
    {
        var issues = new List<string>();
        
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_key_check";
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var table = reader.GetString(0);
                var rowid = reader.GetInt64(1);
                var parent = reader.GetString(2);
                var fkid = reader.GetInt32(3);
                
                issues.Add($"Foreign key violation in {table} (rowid: {rowid}) referencing {parent} (fkid: {fkid})");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating foreign key constraints");
            issues.Add($"Error validating foreign keys: {ex.Message}");
        }
        
        return issues;
    }

    private async Task<List<string>> FindOrphanedRecordsAsync()
    {
        var warnings = new List<string>();
        
        try
        {
            // Check for jobs without corresponding parts
            var orphanedJobs = await _context.Jobs
                .Where(j => !_context.Parts.Any(p => p.PartNumber == j.PartNumber))
                .CountAsync();
            
            if (orphanedJobs > 0)
            {
                warnings.Add($"{orphanedJobs} jobs reference non-existent parts");
            }

            // Check for machine capabilities without corresponding machines
            var orphanedCapabilities = await _context.MachineCapabilities
                .Where(mc => !_context.Machines.Any(m => m.Id == mc.MachineId))
                .CountAsync();
            
            if (orphanedCapabilities > 0)
            {
                warnings.Add($"{orphanedCapabilities} machine capabilities reference non-existent machines");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error finding orphaned records");
            warnings.Add($"Error finding orphaned records: {ex.Message}");
        }
        
        return warnings;
    }

    private async Task<List<string>> CheckDataConsistencyAsync()
    {
        var issues = new List<string>();
        
        try
        {
            // Check for users without required roles
            var usersWithoutRoles = await _context.Users
                .Where(u => string.IsNullOrEmpty(u.Role))
                .CountAsync();
            
            if (usersWithoutRoles > 0)
            {
                issues.Add($"{usersWithoutRoles} users have empty or null roles");
            }

            // Check for inactive machines with active jobs
            var activeJobsOnInactiveMachines = await _context.Jobs
                .Where(j => j.Status == "Scheduled" || j.Status == "InProgress")
                .Where(j => _context.Machines.Any(m => m.MachineId == j.MachineId && !m.IsActive))
                .CountAsync();
            
            if (activeJobsOnInactiveMachines > 0)
            {
                issues.Add($"{activeJobsOnInactiveMachines} active jobs scheduled on inactive machines");
            }

            // Check for overlapping jobs on same machine
            // First get all scheduled jobs grouped by machine, then check overlaps in memory
            var scheduledJobs = await _context.Jobs
                .Where(j => j.Status == "Scheduled")
                .Select(j => new { j.Id, j.MachineId, j.ScheduledStart, j.ScheduledEnd })
                .ToListAsync();
            
            var overlappingJobsCount = 0;
            var processedPairs = new HashSet<string>();
            
            foreach (var job1 in scheduledJobs)
            {
                var overlappingJobs = scheduledJobs
                    .Where(job2 => job2.MachineId == job1.MachineId && 
                                   job2.Id != job1.Id &&
                                   job1.ScheduledStart < job2.ScheduledEnd && 
                                   job2.ScheduledStart < job1.ScheduledEnd)
                    .ToList();
                
                foreach (var job2 in overlappingJobs)
                {
                    // Create a unique pair identifier to avoid counting the same overlap twice
                    var pairKey = job1.Id < job2.Id ? $"{job1.Id}-{job2.Id}" : $"{job2.Id}-{job1.Id}";
                    if (!processedPairs.Contains(pairKey))
                    {
                        processedPairs.Add(pairKey);
                        overlappingJobsCount++;
                    }
                }
            }
            
            if (overlappingJobsCount > 0)
            {
                issues.Add($"{overlappingJobsCount} overlapping job conflicts detected");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking data consistency");
            issues.Add($"Error checking data consistency: {ex.Message}");
        }
        
        return issues;
    }

    private async Task<int> GetTotalRecordCountAsync()
    {
        try
        {
            var counts = await GetRecordCountsAsync();
            return counts.Values.Sum();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get total record count");
            return 0;
        }
    }

    #endregion
}

#region Result Classes

public class DatabaseStatusResult
{
    public bool IsConnected { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModified { get; set; }
    public double FileSize { get; set; } // MB
    public int TableCount { get; set; }
    public Dictionary<string, int> RecordCounts { get; set; } = new();
    public int IndexCount { get; set; }
    public string DatabaseProvider { get; set; } = string.Empty;
    public List<string> MigrationHistory { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class DatabaseExportOptions
{
    public string ExportFormat { get; set; } = "JSON";
    public bool CompressOutput { get; set; } = false;
    public List<string>? TablesToExport { get; set; }
    public bool IncludeUsers { get; set; } = true;
    public bool IncludeMachines { get; set; } = true;
    public bool IncludeParts { get; set; } = true;
    public bool IncludeJobs { get; set; } = true;
    public bool IncludeSystemSettings { get; set; } = true;
    public bool IncludeOperatingShifts { get; set; } = true;
    public bool IncludeRolePermissions { get; set; } = true;
    public bool IncludeHistoricalData { get; set; } = false;
}

public class DatabaseExportResult
{
    public bool Success { get; set; }
    public byte[]? FileData { get; set; }
    public string? FileName { get; set; }
    public Dictionary<string, int> ExportStats { get; set; } = new();
    public long FileSizeBytes { get; set; }
    public DateTime ExportDate { get; set; }
    public string? ErrorMessage { get; set; }
}

public class DatabaseImportOptions
{
    public bool OverwriteExisting { get; set; } = false;
    public bool IsCompressed { get; set; } = false;
    public bool ImportUsers { get; set; } = true;
    public bool ImportMachines { get; set; } = true;
    public bool ImportParts { get; set; } = true;
    public bool ImportJobs { get; set; } = false; // Default false for safety
    public bool ImportSystemSettings { get; set; } = true;
    public bool ValidateBeforeImport { get; set; } = true;
    public bool BackupBeforeImport { get; set; } = true;
}

public class DatabaseImportResult
{
    public bool Success { get; set; }
    public Dictionary<string, int> ImportStats { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public DateTime ImportDate { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SchemaValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public DateTime ValidationDate { get; set; }
}

public class IntegrityCheckResult
{
    public bool IsHealthy { get; set; }
    public List<string> Issues { get; set; } = new();
    public Dictionary<string, object> Statistics { get; set; } = new();
    public DateTime CheckDate { get; set; }
}

public class DatabaseBackupResult
{
    public bool Success { get; set; }
    public string? BackupPath { get; set; }
    public long BackupSizeBytes { get; set; }
    public DateTime BackupDate { get; set; }
    public string? ErrorMessage { get; set; }
}

public class DatabaseRestoreResult
{
    public bool Success { get; set; }
    public long RestoredSizeBytes { get; set; }
    public DateTime RestoreDate { get; set; }
    public string? ErrorMessage { get; set; }
}

public class DatabaseOptimizationResult
{
    public bool Success { get; set; }
    public double BeforeSizeMB { get; set; }
    public double AfterSizeMB { get; set; }
    public double SpaceSavedMB { get; set; }
    public List<string> OptimizationSteps { get; set; } = new();
    public DateTime OptimizationDate { get; set; }
    public string? ErrorMessage { get; set; }
}

#endregion