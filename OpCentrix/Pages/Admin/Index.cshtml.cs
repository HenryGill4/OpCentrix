using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace OpCentrix.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class IndexModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly SlsDataSeedingService _seedingService;
        private readonly DatabaseValidationService _databaseValidationService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(SchedulerContext context, SlsDataSeedingService seedingService, DatabaseValidationService databaseValidationService, ILogger<IndexModel> logger)
        {
            _context = context;
            _seedingService = seedingService;
            _databaseValidationService = databaseValidationService;
            _logger = logger;
        }

        // Existing properties
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalParts { get; set; }
        public int ActiveParts { get; set; }
        public int TotalLogEntries { get; set; }
        public DateTime? LastJobUpdate { get; set; }
        public DateTime? LastPartUpdate { get; set; }
        public List<Job> RecentJobs { get; set; } = new();
        public List<Part> RecentParts { get; set; } = new();
        public List<JobLogEntry> RecentLogEntries { get; set; } = new();

        // New database management properties
        public bool HasSampleData { get; set; }
        public int SamplePartsCount { get; set; }
        public int SampleJobsCount { get; set; }
        public int RealDataCount { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Load existing statistics with error handling
                TotalJobs = await _context.Jobs.CountAsync();
                ActiveJobs = await _context.Jobs.CountAsync(j => j.Status != "Completed" && j.Status != "Cancelled");
                TotalParts = await _context.Parts.CountAsync();
                ActiveParts = await _context.Parts.CountAsync(p => p.IsActive);
                TotalLogEntries = await _context.JobLogEntries.CountAsync();

                LastJobUpdate = await _context.Jobs
                    .OrderByDescending(j => j.LastModifiedDate)
                    .Select(j => j.LastModifiedDate)
                    .FirstOrDefaultAsync();
                    
                LastPartUpdate = await _context.Parts
                    .OrderByDescending(p => p.LastModifiedDate)
                    .Select(p => p.LastModifiedDate)
                    .FirstOrDefaultAsync();

                // Load recent data with safe queries
                RecentJobs = await _context.Jobs
                    .OrderByDescending(j => j.CreatedDate)
                    .Take(5)
                    .ToListAsync() ?? new List<Job>();
                    
                RecentParts = await _context.Parts
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(5)
                    .ToListAsync() ?? new List<Part>();
                    
                RecentLogEntries = await _context.JobLogEntries
                    .OrderByDescending(l => l.Timestamp)
                    .Take(8)
                    .ToListAsync() ?? new List<JobLogEntry>();

                // Safe database validation with error handling
                try
                {
                    var validationResult = await _databaseValidationService.ValidateDatabaseAsync();
                    HasSampleData = validationResult.HasSampleData;
                    RealDataCount = validationResult.RealPartsCount + validationResult.RealJobsCount;
                }
                catch (Exception validationEx)
                {
                    _logger.LogWarning(validationEx, "Database validation service unavailable");
                    HasSampleData = false;
                    RealDataCount = TotalParts + TotalJobs; // Fallback calculation
                }

                // Safe sample counting with error handling
                try
                {
                    SamplePartsCount = await CountSamplePartsAsync();
                    SampleJobsCount = await CountSampleJobsAsync();
                }
                catch (Exception countEx)
                {
                    _logger.LogWarning(countEx, "Error counting sample data");
                    SamplePartsCount = 0;
                    SampleJobsCount = 0;
                }

                _logger.LogInformation("Admin dashboard loaded successfully: {TotalJobs} jobs, {TotalParts} parts", TotalJobs, TotalParts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error loading admin dashboard data");
                
                // Initialize with safe defaults to prevent page crashes
                TotalJobs = 0;
                ActiveJobs = 0;
                TotalParts = 0;
                ActiveParts = 0;
                TotalLogEntries = 0;
                LastJobUpdate = null;
                LastPartUpdate = null;
                RecentJobs = new List<Job>();
                RecentParts = new List<Part>();
                RecentLogEntries = new List<JobLogEntry>();
                HasSampleData = false;
                SamplePartsCount = 0;
                SampleJobsCount = 0;
                RealDataCount = 0;
                
                // Log specific error details for debugging
                _logger.LogError("Database connection: {ConnectionString}", 
                    _context.Database.GetConnectionString());
                _logger.LogError("Exception details: {Message}", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
            }
        }

        private async Task<int> CountSamplePartsAsync()
        {
            var samplePartNumbers = new[] { "14-5396", "14-5397", "14-5398", "14-5399", "14-5400", "14-5401", "14-5402", "14-5403", "10-0001", "10-0002" };
            
            return await _context.Parts
                .Where(p => samplePartNumbers.Contains(p.PartNumber) || 
                           p.Description.Contains("Aerospace") || 
                           p.Description.Contains("Medical") || 
                           p.Description.Contains("Automotive") ||
                           p.Description.Contains("Standard") ||
                           p.CreatedBy == "Engineer" ||
                           p.CreatedBy == "System" ||
                           p.CreatedBy == "Design Engineer" ||
                           p.CreatedBy == "Biomedical Engineer" ||
                           p.CreatedBy == "Medical Device Engineer" ||
                           p.CreatedBy == "Aerospace Engineer" ||
                           p.Industry == "General")
                .CountAsync();
        }

        private async Task<int> CountSampleJobsAsync()
        {
            var samplePartNumbers = new[] { "14-5396", "14-5397", "14-5398", "14-5399", "14-5400", "14-5401", "14-5402", "14-5403", "10-0001", "10-0002" };

            return await _context.Jobs
                .Where(j => samplePartNumbers.Contains(j.PartNumber) || 
                           j.CreatedBy == "Scheduler" ||
                           j.CreatedBy == "System" ||
                           j.CustomerOrderNumber.StartsWith("CO-") ||
                           j.Operator == "John Smith" ||
                           j.Notes.Contains("Sample") ||
                           j.Notes.Contains("Demo"))
                .CountAsync();
        }

        public async Task<IActionResult> OnPostRemoveSampleDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting complete removal of all sample/example data...");

                var samplePartNumbers = new[] { "14-5396", "14-5397", "14-5398", "14-5399", "14-5400", "14-5401", "14-5402", "14-5403", "10-0001", "10-0002" };

                // Remove ALL sample jobs first (due to foreign key constraints)
                var sampleJobs = await _context.Jobs
                    .Where(j => samplePartNumbers.Contains(j.PartNumber) || 
                               j.CreatedBy == "Scheduler" ||
                               j.CreatedBy == "System" ||
                               j.CustomerOrderNumber.StartsWith("CO-") ||
                               j.Operator == "John Smith" ||
                               j.Notes.Contains("Sample") ||
                               j.Notes.Contains("Demo"))
                    .ToListAsync();

                _context.Jobs.RemoveRange(sampleJobs);

                // Remove ALL sample parts (including basic test parts)
                var sampleParts = await _context.Parts
                    .Where(p => samplePartNumbers.Contains(p.PartNumber) || 
                               p.Description.Contains("Aerospace") || 
                               p.Description.Contains("Medical") || 
                               p.Description.Contains("Automotive") ||
                               p.Description.Contains("Standard") ||
                               p.CreatedBy == "Engineer" ||
                               p.CreatedBy == "System" ||
                               p.CreatedBy == "Design Engineer" ||
                               p.CreatedBy == "Biomedical Engineer" ||
                               p.CreatedBy == "Medical Device Engineer" ||
                               p.CreatedBy == "Aerospace Engineer" ||
                               p.Industry == "General")
                    .ToListAsync();

                _context.Parts.RemoveRange(sampleParts);

                // Remove sample build jobs from print tracking
                var sampleBuildJobs = await _context.BuildJobs
                    .Where(bj => samplePartNumbers.Any(spn => bj.Notes != null && bj.Notes.Contains(spn)) ||
                                bj.User != null && bj.User.Username == "operator" ||
                                bj.Status == "Sample")
                    .ToListAsync();

                _context.BuildJobs.RemoveRange(sampleBuildJobs);

                // Remove sample log entries
                var sampleLogEntries = await _context.JobLogEntries
                    .Where(jle => samplePartNumbers.Contains(jle.PartNumber) ||
                                 jle.Operator == "John Smith" ||
                                 jle.Notes != null && jle.Notes.Contains("Sample"))
                    .ToListAsync();

                _context.JobLogEntries.RemoveRange(sampleLogEntries);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully removed {JobCount} sample jobs, {PartCount} sample parts, {BuildJobCount} build jobs, and {LogCount} log entries", 
                    sampleJobs.Count, sampleParts.Count, sampleBuildJobs.Count, sampleLogEntries.Count);

                return Content($@"
                    <div class='bg-green-50 border border-green-200 rounded-lg p-4'>
                        <div class='flex items-center'>
                            <svg class='w-5 h-5 text-green-500 mr-2' fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M5 13l4 4L19 7'></path>
                            </svg>
                            <span class='text-green-800 font-medium'>? Complete Cleanup Successful!</span>
                        </div>
                        <div class='text-green-700 mt-2'>
                            <p><strong>Removed all example data:</strong></p>
                            <ul class='list-disc list-inside mt-1 text-sm'>
                                <li>{sampleJobs.Count} sample jobs</li>
                                <li>{sampleParts.Count} sample parts</li>
                                <li>{sampleBuildJobs.Count} sample build jobs</li>
                                <li>{sampleLogEntries.Count} sample log entries</li>
                            </ul>
                            <p class='mt-2 font-medium'>?? Your database is now production-ready with only essential data!</p>
                            <a href='/Admin' class='underline text-green-800 font-medium'>Refresh page</a> to see updated statistics.
                        </div>
                    </div>
                ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing all sample data");
                return Content($@"
                    <div class='bg-red-50 border border-red-200 rounded-lg p-4'>
                        <div class='flex items-center'>
                            <svg class='w-5 h-5 text-red-500 mr-2' fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M6 18L18 6M6 6l12 12'></path>
                            </svg>
                            <span class='text-red-800 font-medium'>Error</span>
                        </div>
                        <p class='text-red-700 mt-1'>Failed to remove sample data: {ex.Message}</p>
                    </div>
                ");
            }
        }

        public async Task<IActionResult> OnPostAddSampleDataAsync()
        {
            try
            {
                // Re-seed sample data
                await _seedingService.SeedPartsAsync();
                await _seedingService.SeedJobsAsync();

                _logger.LogInformation("Sample data added successfully");

                return Content($@"
                    <div class='bg-green-50 border border-green-200 rounded-lg p-4'>
                        <div class='flex items-center'>
                            <svg class='w-5 h-5 text-green-500 mr-2' fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M5 13l4 4L19 7'></path>
                            </svg>
                            <span class='text-green-800 font-medium'>Success!</span>
                        </div>
                        <p class='text-green-700 mt-1'>
                            Sample data added for testing and demonstration. 
                            <a href='/Admin' class='underline'>Refresh page</a> to see updated statistics.
                        </p>
                    </div>
                ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding sample data");
                return Content($@"
                    <div class='bg-red-50 border border-red-200 rounded-lg p-4'>
                        <div class='flex items-center'>
                            <svg class='w-5 h-5 text-red-500 mr-2' fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M6 18L18 6M6 6l12 12'></path>
                            </svg>
                            <span class='text-red-800 font-medium'>Error</span>
                        </div>
                        <p class='text-red-700 mt-1'>Failed to add sample data: {ex.Message}</p>
                    </div>
                ");
            }
        }

        public async Task<IActionResult> OnPostBackupDatabaseAsync()
        {
            try
            {
                var backupFileName = $"scheduler_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                var sourcePath = "scheduler.db";
                
                // Check if source database exists
                if (!System.IO.File.Exists(sourcePath))
                {
                    _logger.LogWarning("Source database file not found: {SourcePath}", sourcePath);
                    return Content($@"
                        <div class='bg-yellow-50 border border-yellow-200 rounded-lg p-4'>
                            <div class='flex items-center'>
                                <svg class='w-5 h-5 text-yellow-500 mr-2' fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                    <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16c-.77.833.192 2.5 1.732 2.5z'></path>
                                </svg>
                                <span class='text-yellow-800 font-medium'>Database Not Found</span>
                            </div>
                            <p class='text-yellow-700 mt-1'>No database file found to backup. The database will be created automatically when you start the application.</p>
                        </div>
                    ");
                }

                var backupPath = Path.Combine("Backups", backupFileName);

                // Create backups directory if it doesn't exist
                Directory.CreateDirectory("Backups");

                // Get file size for feedback
                var sourceInfo = new FileInfo(sourcePath);
                var fileSizeKB = sourceInfo.Length / 1024;

                // Copy database file with verification
                System.IO.File.Copy(sourcePath, backupPath, true);
                
                // Verify backup was created successfully
                if (!System.IO.File.Exists(backupPath))
                {
                    throw new IOException("Backup file was not created successfully");
                }

                var backupInfo = new FileInfo(backupPath);
                if (backupInfo.Length != sourceInfo.Length)
                {
                    throw new IOException("Backup file size doesn't match source file");
                }

                _logger.LogInformation("Database backed up successfully to {BackupPath} ({FileSizeKB} KB)", backupPath, fileSizeKB);

                return Content($@"
                    <div class='bg-green-50 border border-green-200 rounded-lg p-4'>
                        <div class='flex items-center'>
                            <svg class='w-5 h-5 text-green-500 mr-2' fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M5 13l4 4L19 7'></path>
                            </svg>
                            <span class='text-green-800 font-medium'>Backup Created Successfully!</span>
                        </div>
                        <div class='text-green-700 mt-1'>
                            <p><strong>Database backed up to:</strong> {backupFileName}</p>
                            <p class='text-sm mt-1'>?? Location: /Backups/ directory</p>
                            <p class='text-sm'>?? Size: {fileSizeKB:N0} KB</p>
                            <p class='text-sm mt-2'>?? <strong>Tip:</strong> Download this file for safekeeping before making major changes.</p>
                        </div>
                    </div>
                ");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied creating database backup");
                return Content($@"
                    <div class='bg-red-50 border border-red-200 rounded-lg p-4'>
                        <div class='flex items-center'>
                            <svg class='w-5 h-5 text-red-500 mr-2' fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16c-.77.833.192 2.5 1.732 2.5z'></path>
                            </svg>
                            <span class='text-red-800 font-medium'>Access Denied</span>
                        </div>
                        <p class='text-red-700 mt-1'>Permission denied creating backup. Please check file permissions or run as administrator.</p>
                    </div>
                ");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "IO error creating database backup");
                return Content($@"
                    <div class='bg-red-50 border border-red-200 rounded-lg p-4'>
                        <div class='flex items-center'>
                            <svg class='w-5 h-5 text-red-500 mr-2' fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16c-.77.833.192 2.5 1.732 2.5z'></path>
                            </svg>
                            <span class='text-red-800 font-medium'>Backup Failed</span>
                        </div>
                        <p class='text-red-700 mt-1'>File system error: {ex.Message}</p>
                        <p class='text-red-600 text-sm mt-1'>Check disk space and file permissions.</p>
                    </div>
                ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating database backup");
                return Content($@"
                    <div class='bg-red-50 border border-red-200 rounded-lg p-4'>
                        <div class='flex items-center'>
                            <svg class='w-5 h-5 text-red-500 mr-2' fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M6 18L18 6M6 6l12 12'></path>
                            </svg>
                            <span class='text-red-800 font-medium'>Backup Failed</span>
                        </div>
                        <p class='text-red-700 mt-1'>Unexpected error: {ex.Message}</p>
                        <p class='text-red-600 text-sm mt-1'>Check the console output for more details.</p>
                    </div>
                ");
            }
        }
    }
}