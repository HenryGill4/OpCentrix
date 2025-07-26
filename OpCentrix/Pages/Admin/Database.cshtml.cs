using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Services.Admin;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.Admin;

/// <summary>
/// Admin page for database export, import, and diagnostics
/// Task 16: Database Export and Diagnostics
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class DatabaseModel : PageModel
{
    private readonly IDatabaseManagementService _databaseService;
    private readonly ILogger<DatabaseModel> _logger;

    public DatabaseModel(IDatabaseManagementService databaseService, ILogger<DatabaseModel> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    // Page properties
    public DatabaseStatusResult DatabaseStatus { get; set; } = new();
    public SchemaValidationResult? ValidationResult { get; set; }
    public IntegrityCheckResult? IntegrityResult { get; set; }
    public DatabaseOptimizationResult? OptimizationResult { get; set; }

    // Export/Import options
    [BindProperty]
    public ExportOptionsModel ExportOptions { get; set; } = new();

    [BindProperty]
    public ImportOptionsModel ImportOptions { get; set; } = new();

    // Backup/Restore
    [BindProperty]
    public BackupRestoreModel BackupRestore { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("?? Admin {Admin} accessing database management page", User.Identity?.Name);

            await LoadDatabaseStatusAsync();

            _logger.LogInformation("? Database management page loaded for {Admin}", User.Identity?.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error loading database management page for {Admin}", User.Identity?.Name);
            TempData["Error"] = "Error loading database information. Please try again.";
        }
    }

    public async Task<IActionResult> OnPostExportAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadDatabaseStatusAsync();
                TempData["Error"] = "Please correct the validation errors and try again.";
                return Page();
            }

            _logger.LogInformation("?? Admin {Admin} starting database export - Format: {Format}", 
                User.Identity?.Name, ExportOptions.ExportFormat);

            var options = new DatabaseExportOptions
            {
                ExportFormat = ExportOptions.ExportFormat,
                CompressOutput = ExportOptions.CompressOutput,
                IncludeUsers = ExportOptions.IncludeUsers,
                IncludeMachines = ExportOptions.IncludeMachines,
                IncludeParts = ExportOptions.IncludeParts,
                IncludeJobs = ExportOptions.IncludeJobs,
                IncludeSystemSettings = ExportOptions.IncludeSystemSettings,
                IncludeOperatingShifts = ExportOptions.IncludeOperatingShifts,
                IncludeRolePermissions = ExportOptions.IncludeRolePermissions,
                IncludeHistoricalData = ExportOptions.IncludeHistoricalData
            };

            var result = await _databaseService.ExportDatabaseAsync(options);

            if (result.Success && result.FileData != null)
            {
                var contentType = result.FileName!.EndsWith(".zip") ? "application/zip" : "application/json";
                
                _logger.LogInformation("? Database export completed by {Admin} - File: {FileName}, Size: {Size} KB", 
                    User.Identity?.Name, result.FileName, result.FileSizeBytes / 1024);

                return File(result.FileData, contentType, result.FileName);
            }
            else
            {
                TempData["Error"] = $"Export failed: {result.ErrorMessage}";
                await LoadDatabaseStatusAsync();
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error exporting database for {Admin}", User.Identity?.Name);
            TempData["Error"] = "An error occurred while exporting the database.";
            await LoadDatabaseStatusAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostImportAsync(IFormFile importFile)
    {
        try
        {
            if (importFile == null || importFile.Length == 0)
            {
                TempData["Error"] = "Please select a file to import.";
                await LoadDatabaseStatusAsync();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await LoadDatabaseStatusAsync();
                TempData["Error"] = "Please correct the validation errors and try again.";
                return Page();
            }

            _logger.LogInformation("?? Admin {Admin} starting database import - File: {FileName}, Size: {Size} KB", 
                User.Identity?.Name, importFile.FileName, importFile.Length / 1024);

            // Show confirmation dialog for dangerous operations
            if (ImportOptions.OverwriteExisting)
            {
                _logger.LogWarning("?? Admin {Admin} performing OVERWRITE import - File: {FileName}", 
                    User.Identity?.Name, importFile.FileName);
            }

            var options = new DatabaseImportOptions
            {
                OverwriteExisting = ImportOptions.OverwriteExisting,
                IsCompressed = importFile.FileName.EndsWith(".zip"),
                ImportUsers = ImportOptions.ImportUsers,
                ImportMachines = ImportOptions.ImportMachines,
                ImportParts = ImportOptions.ImportParts,
                ImportJobs = ImportOptions.ImportJobs,
                ImportSystemSettings = ImportOptions.ImportSystemSettings,
                ValidateBeforeImport = ImportOptions.ValidateBeforeImport,
                BackupBeforeImport = ImportOptions.BackupBeforeImport
            };

            // Create backup before import if requested
            if (options.BackupBeforeImport)
            {
                var backupPath = $"backups/pre-import-{DateTime.Now:yyyyMMdd-HHmmss}.db";
                var backupResult = await _databaseService.BackupDatabaseAsync(backupPath);
                if (!backupResult.Success)
                {
                    TempData["Error"] = $"Failed to create backup before import: {backupResult.ErrorMessage}";
                    await LoadDatabaseStatusAsync();
                    return Page();
                }
                _logger.LogInformation("?? Pre-import backup created: {BackupPath}", backupPath);
            }

            using var stream = importFile.OpenReadStream();
            var result = await _databaseService.ImportDatabaseAsync(stream, options);

            if (result.Success)
            {
                var recordsImported = result.ImportStats.Values.Sum();
                TempData["Success"] = $"Import completed successfully. {recordsImported} records imported across {result.ImportStats.Count} tables.";
                
                _logger.LogInformation("? Database import completed by {Admin} - Records imported: {RecordCount}, Tables: {TableCount}", 
                    User.Identity?.Name, recordsImported, result.ImportStats.Count);

                if (result.Errors.Any())
                {
                    TempData["Warning"] = $"Import completed with {result.Errors.Count} warnings. Check logs for details.";
                }
            }
            else
            {
                TempData["Error"] = $"Import failed: {result.ErrorMessage}";
            }

            await LoadDatabaseStatusAsync();
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error importing database for {Admin}", User.Identity?.Name);
            TempData["Error"] = "An error occurred while importing the database.";
            await LoadDatabaseStatusAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostValidateSchemaAsync()
    {
        try
        {
            _logger.LogInformation("?? Admin {Admin} validating database schema", User.Identity?.Name);

            ValidationResult = await _databaseService.ValidateSchemaAsync();

            if (ValidationResult.IsValid)
            {
                TempData["Success"] = "Schema validation passed successfully.";
            }
            else
            {
                TempData["Warning"] = $"Schema validation found {ValidationResult.Issues.Count} issues and {ValidationResult.Warnings.Count} warnings.";
            }

            _logger.LogInformation("? Schema validation completed by {Admin} - Valid: {IsValid}, Issues: {IssueCount}", 
                User.Identity?.Name, ValidationResult.IsValid, ValidationResult.Issues.Count);

            await LoadDatabaseStatusAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error validating schema for {Admin}", User.Identity?.Name);
            TempData["Error"] = "An error occurred while validating the database schema.";
            await LoadDatabaseStatusAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostCheckIntegrityAsync()
    {
        try
        {
            _logger.LogInformation("?? Admin {Admin} checking database integrity", User.Identity?.Name);

            IntegrityResult = await _databaseService.CheckDatabaseIntegrityAsync();

            if (IntegrityResult.IsHealthy)
            {
                TempData["Success"] = "Database integrity check passed successfully.";
            }
            else
            {
                TempData["Warning"] = $"Database integrity check found {IntegrityResult.Issues.Count} issues.";
            }

            _logger.LogInformation("? Integrity check completed by {Admin} - Healthy: {IsHealthy}, Issues: {IssueCount}", 
                User.Identity?.Name, IntegrityResult.IsHealthy, IntegrityResult.Issues.Count);

            await LoadDatabaseStatusAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error checking database integrity for {Admin}", User.Identity?.Name);
            TempData["Error"] = "An error occurred while checking database integrity.";
            await LoadDatabaseStatusAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostOptimizeDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("?? Admin {Admin} optimizing database", User.Identity?.Name);

            OptimizationResult = await _databaseService.OptimizeDatabaseAsync();

            if (OptimizationResult.Success)
            {
                TempData["Success"] = $"Database optimization completed. Space saved: {OptimizationResult.SpaceSavedMB:F2} MB";
            }
            else
            {
                TempData["Error"] = $"Database optimization failed: {OptimizationResult.ErrorMessage}";
            }

            _logger.LogInformation("? Database optimization completed by {Admin} - Space saved: {SpaceSaved} MB", 
                User.Identity?.Name, OptimizationResult.SpaceSavedMB);

            await LoadDatabaseStatusAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error optimizing database for {Admin}", User.Identity?.Name);
            TempData["Error"] = "An error occurred while optimizing the database.";
            await LoadDatabaseStatusAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostBackupAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(BackupRestore.BackupPath))
            {
                TempData["Error"] = "Please specify a backup path.";
                await LoadDatabaseStatusAsync();
                return Page();
            }

            _logger.LogInformation("?? Admin {Admin} creating database backup to: {BackupPath}", 
                User.Identity?.Name, BackupRestore.BackupPath);

            var result = await _databaseService.BackupDatabaseAsync(BackupRestore.BackupPath);

            if (result.Success)
            {
                TempData["Success"] = $"Database backup created successfully. Size: {result.BackupSizeBytes / 1024} KB";
            }
            else
            {
                TempData["Error"] = $"Backup failed: {result.ErrorMessage}";
            }

            _logger.LogInformation("? Database backup completed by {Admin} - Success: {Success}, Size: {Size} KB", 
                User.Identity?.Name, result.Success, result.BackupSizeBytes / 1024);

            await LoadDatabaseStatusAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error creating database backup for {Admin}", User.Identity?.Name);
            TempData["Error"] = "An error occurred while creating the database backup.";
            await LoadDatabaseStatusAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRestoreAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(BackupRestore.RestorePath))
            {
                TempData["Error"] = "Please specify a restore path.";
                await LoadDatabaseStatusAsync();
                return Page();
            }

            _logger.LogWarning("?? Admin {Admin} restoring database from: {RestorePath}, Overwrite: {OverwriteExisting}", 
                User.Identity?.Name, BackupRestore.RestorePath, BackupRestore.OverwriteExisting);

            var result = await _databaseService.RestoreDatabaseAsync(BackupRestore.RestorePath, BackupRestore.OverwriteExisting);

            if (result.Success)
            {
                TempData["Success"] = $"Database restored successfully. Size: {result.RestoredSizeBytes / 1024} KB";
            }
            else
            {
                TempData["Error"] = $"Restore failed: {result.ErrorMessage}";
            }

            _logger.LogInformation("? Database restore completed by {Admin} - Success: {Success}, Size: {Size} KB", 
                User.Identity?.Name, result.Success, result.RestoredSizeBytes / 1024);

            await LoadDatabaseStatusAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error restoring database for {Admin}", User.Identity?.Name);
            TempData["Error"] = "An error occurred while restoring the database.";
            await LoadDatabaseStatusAsync();
            return Page();
        }
    }

    private async Task LoadDatabaseStatusAsync()
    {
        try
        {
            DatabaseStatus = await _databaseService.GetDatabaseStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading database status");
            DatabaseStatus = new DatabaseStatusResult
            {
                IsConnected = false,
                ErrorMessage = ex.Message
            };
        }
    }
}

#region View Models

public class ExportOptionsModel
{
    [Required]
    [Display(Name = "Export Format")]
    public string ExportFormat { get; set; } = "JSON";

    [Display(Name = "Compress Output")]
    public bool CompressOutput { get; set; } = true;

    [Display(Name = "Include Users")]
    public bool IncludeUsers { get; set; } = true;

    [Display(Name = "Include Machines")]
    public bool IncludeMachines { get; set; } = true;

    [Display(Name = "Include Parts")]
    public bool IncludeParts { get; set; } = true;

    [Display(Name = "Include Jobs")]
    public bool IncludeJobs { get; set; } = false; // Default false for safety

    [Display(Name = "Include System Settings")]
    public bool IncludeSystemSettings { get; set; } = true;

    [Display(Name = "Include Operating Shifts")]
    public bool IncludeOperatingShifts { get; set; } = true;

    [Display(Name = "Include Role Permissions")]
    public bool IncludeRolePermissions { get; set; } = true;

    [Display(Name = "Include Historical Data")]
    public bool IncludeHistoricalData { get; set; } = false;
}

public class ImportOptionsModel
{
    [Display(Name = "Overwrite Existing Data")]
    public bool OverwriteExisting { get; set; } = false;

    [Display(Name = "Import Users")]
    public bool ImportUsers { get; set; } = true;

    [Display(Name = "Import Machines")]
    public bool ImportMachines { get; set; } = true;

    [Display(Name = "Import Parts")]
    public bool ImportParts { get; set; } = true;

    [Display(Name = "Import Jobs")]
    public bool ImportJobs { get; set; } = false; // Default false for safety

    [Display(Name = "Import System Settings")]
    public bool ImportSystemSettings { get; set; } = true;

    [Display(Name = "Validate Before Import")]
    public bool ValidateBeforeImport { get; set; } = true;

    [Display(Name = "Create Backup Before Import")]
    public bool BackupBeforeImport { get; set; } = true;
}

public class BackupRestoreModel
{
    [StringLength(500)]
    [Display(Name = "Backup Path")]
    public string BackupPath { get; set; } = $"backups/backup-{DateTime.Now:yyyyMMdd-HHmmss}.db";

    [StringLength(500)]
    [Display(Name = "Restore Path")]
    public string RestorePath { get; set; } = string.Empty;

    [Display(Name = "Overwrite Existing Database")]
    public bool OverwriteExisting { get; set; } = false;
}

#endregion