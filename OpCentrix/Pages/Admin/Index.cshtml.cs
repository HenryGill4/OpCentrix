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

        public async Task<IActionResult> OnPostCreatePrintTrackingTestJobsAsync()
        {
            try
            {
                _logger.LogInformation("Creating comprehensive print tracking test jobs...");

                // First ensure we have basic SLS machines in the database
                await EnsureSlsMachinesExistAsync();

                // Clear existing jobs and build jobs to start fresh
                var existingJobs = await _context.Jobs.ToListAsync();
                var existingBuildJobs = await _context.BuildJobs.ToListAsync();
                var existingJobLogs = await _context.JobLogEntries.ToListAsync();

                _context.Jobs.RemoveRange(existingJobs);
                _context.BuildJobs.RemoveRange(existingBuildJobs);
                _context.JobLogEntries.RemoveRange(existingJobLogs);

                await _context.SaveChangesAsync();

                // Create test parts if they don't exist
                await EnsureTestPartsExistAsync();

                var now = DateTime.Now;
                var testJobs = new List<Job>();

                // Get available machines and parts
                var machines = await _context.Machines
                    .Where(m => m.IsActive && m.MachineType.ToUpper() == "SLS")
                    .ToListAsync();

                var parts = await _context.Parts.Take(10).ToListAsync();

                if (!machines.Any())
                {
                    throw new InvalidOperationException("No SLS machines found in database");
                }

                if (!parts.Any())
                {
                    throw new InvalidOperationException("No parts found in database");
                }

                // Create a variety of jobs for testing different scenarios
                var jobScenarios = new[]
                {
                    // Jobs ready to start printing (Scheduled -> In Progress)
                    new { Status = "Scheduled", Hours = 2.5, StartOffset = -0.5, Description = "Ready to start - slightly delayed" },
                    new { Status = "Scheduled", Hours = 4.0, StartOffset = 0.0, Description = "Ready to start - on time" },
                    new { Status = "Scheduled", Hours = 6.25, StartOffset = 0.5, Description = "Ready to start - early" },

                    // Jobs currently in progress (can be completed)
                    new { Status = "In Progress", Hours = 3.5, StartOffset = -3.0, Description = "Currently printing - almost done" },
                    new { Status = "In Progress", Hours = 8.0, StartOffset = -4.0, Description = "Long job - halfway through" },
                    new { Status = "Building", Hours = 2.75, StartOffset = -1.5, Description = "Quick job - nearly finished" },

                    // Future jobs (scheduled for later)
                    new { Status = "Scheduled", Hours = 5.0, StartOffset = 4.0, Description = "Future job - tomorrow morning" },
                    new { Status = "Scheduled", Hours = 12.0, StartOffset = 8.0, Description = "Big job - scheduled for later" },

                    // Recently completed jobs (for testing historical data)
                    new { Status = "Completed", Hours = 3.0, StartOffset = -8.0, Description = "Recently completed job" },
                    new { Status = "Completed", Hours = 6.5, StartOffset = -12.0, Description = "Yesterday's job" },
                };

                foreach (var scenario in jobScenarios)
                {
                    var machine = machines[(testJobs.Count) % machines.Count];
                    var part = parts[(testJobs.Count) % parts.Count];
                    
                    var scheduledStart = now.AddHours(scenario.StartOffset);
                    var scheduledEnd = scheduledStart.AddHours(scenario.Hours);

                    var job = new Job
                    {
                        PartId = part.Id,
                        PartNumber = part.PartNumber,
                        MachineId = machine.MachineId,
                        Quantity = new Random().Next(1, 10),
                        Priority = testJobs.Count <= 3 ? 2 : 3, // 2 = High, 3 = Normal
                        Status = scenario.Status,
                        ScheduledStart = scheduledStart,
                        ScheduledEnd = scheduledEnd,
                        EstimatedHours = scenario.Hours,
                        CustomerOrderNumber = $"TEST-{1000 + testJobs.Count + 1}",
                        Notes = $"Test job {testJobs.Count + 1}: {scenario.Description}",
                        Operator = "Test Operator",
                        CreatedDate = now.AddDays(-1),
                        LastModifiedDate = now,
                        CreatedBy = "Admin Test Setup",
                        LastModifiedBy = "Admin Test Setup",
                        SlsMaterial = part.SlsMaterial ?? "SS316L"
                    };

                    // Set actual start/end times for jobs that are in progress or completed
                    if (scenario.Status == "In Progress" || scenario.Status == "Building")
                    {
                        job.ActualStart = scheduledStart;
                    }
                    else if (scenario.Status == "Completed")
                    {
                        job.ActualStart = scheduledStart;
                        job.ActualEnd = scheduledEnd.AddMinutes(new Random().Next(-30, 30)); // Some variance
                    }

                    testJobs.Add(job);
                }

                _context.Jobs.AddRange(testJobs);
                await _context.SaveChangesAsync();

                // Create corresponding build jobs for jobs that are in progress or completed
                var buildJobs = new List<BuildJob>();

                foreach (var job in testJobs.Where(j => j.Status == "In Progress" || j.Status == "Building" || j.Status == "Completed"))
                {
                    var buildJob = new BuildJob
                    {
                        // The Job.Id will be auto-assigned by the database after SaveChanges
                        // We'll need to link this after the jobs are saved
                        PrinterName = job.MachineId,
                        PartId = job.PartId,
                        Status = job.Status == "Completed" ? "Completed" : "In Progress",
                        ActualStartTime = job.ActualStart ?? DateTime.Now.AddHours(-2),
                        ActualEndTime = job.Status == "Completed" ? job.ActualEnd : null,
                        OperatorEstimatedHours = (decimal)job.EstimatedHours,
                        OperatorActualHours = job.Status == "Completed" ? (decimal)(job.EstimatedHours + (new Random().NextDouble() - 0.5)) : null,
                        TotalPartsInBuild = job.Quantity,
                        Notes = $"Build job for test job",
                        UserId = 1, // Default test user
                        CreatedAt = job.CreatedDate
                    };

                    buildJobs.Add(buildJob);
                }

                _context.BuildJobs.AddRange(buildJobs);
                await _context.SaveChangesAsync();

                // Now link the BuildJobs to the saved Jobs using their generated IDs
                var savedJobs = await _context.Jobs.OrderBy(j => j.Id).ToListAsync();
                var savedBuildJobs = await _context.BuildJobs.OrderBy(bj => bj.BuildId).ToListAsync();
                
                for (int i = 0; i < Math.Min(savedJobs.Count, savedBuildJobs.Count); i++)
                {
                    var job = savedJobs.Where(j => j.Status == "In Progress" || j.Status == "Building" || j.Status == "Completed").Skip(i).FirstOrDefault();
                    if (job != null && i < savedBuildJobs.Count)
                    {
                        savedBuildJobs[i].AssociatedScheduledJobId = job.Id;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created {JobCount} test jobs and {BuildJobCount} build jobs for print tracking", 
                    testJobs.Count, buildJobs.Count);

                return Content($@"
                    <div class='bg-green-50 border border-green-200 rounded-lg p-4'>
                        <div class='flex items-center mb-3'>
                            <svg class='w-5 h-5 text-green-500 mr-2' fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M5 13l4 4L19 7'></path>
                            </svg>
                            <span class='text-green-800 font-medium'>? Print Tracking Test Jobs Created!</span>
                        </div>
                        <div class='text-green-700 space-y-1'>
                            <p><strong>Created {testJobs.Count} test jobs:</strong></p>
                            <ul class='list-disc list-inside text-sm ml-4'>
                                <li>?? 3 jobs ready to start (use Start Print button)</li>
                                <li>?? 3 jobs currently in progress (use Complete Print button)</li>
                                <li>? 2 jobs scheduled for future</li>
                                <li>? 2 jobs already completed (for historical data)</li>
                            </ul>
                            <p class='mt-3 font-medium'>?? <strong>Testing Instructions:</strong></p>
                            <ol class='list-decimal list-inside text-sm ml-4 space-y-1'>
                                <li>Go to <a href='/PrintTracking' class='underline font-medium'>Print Tracking</a> page</li>
                                <li>For yellow jobs: Click ""Start Print"" to begin printing</li>
                                <li>For blue jobs: Click ""Complete Print"" to finish and test the decimal input</li>
                                <li>Test the modal forms and validation</li>
                            </ol>
                            <div class='mt-3 p-2 bg-green-100 rounded border-l-4 border-green-400'>
                                <p class='text-sm'>?? <strong>Perfect for testing:</strong> Decimal validation, modal functionality, job state transitions, and print tracking workflow!</p>
                            </div>
                        </div>
                    </div>
                ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating print tracking test jobs");
                return Content($@"
                    <div class='bg-red-50 border border-red-200 rounded-lg p-4'>
                        <div class='flex items-center'>
                            <svg class='w-5 h-5 text-red-500 mr-2' fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M6 18L18 6M6 6l12 12'></path>
                            </svg>
                            <span class='text-red-800 font-medium'>Error Creating Test Jobs</span>
                        </div>
                        <p class='text-red-700 mt-1'>Failed to create test jobs: {ex.Message}</p>
                        <details class='mt-2'>
                            <summary class='text-red-600 cursor-pointer'>Show details</summary>
                            <pre class='text-xs text-red-600 mt-1 whitespace-pre-wrap'>{ex.StackTrace}</pre>
                        </details>
                    </div>
                ");
            }
        }

        public async Task<IActionResult> OnPostCreateStageDashboardTestDataAsync()
        {
            try
            {
                _logger.LogInformation("?? [ADMIN] Creating comprehensive stage dashboard test data...");

                // Create stage dashboard test data using our new seeding service
                var stageDashboardSeeder = HttpContext.RequestServices.GetRequiredService<StageDashboardSeedingService>();
                await stageDashboardSeeder.CreateStageDashboardTestDataAsync();

                _logger.LogInformation("? [ADMIN] Stage dashboard test data created successfully");

                return Content($@"
                    <div class='bg-green-50 border border-green-200 rounded-lg p-4'>
                        <div class='flex items-center mb-3'>
                            <svg class='w-5 h-5 text-green-500 mr-2' fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M5 13l4 4L19 7'></path>
                            </svg>
                            <span class='text-green-800 font-medium'>?? Stage Dashboard Test Data Created!</span>
                        </div>
                        <div class='text-green-700 space-y-2'>
                            <p><strong>Created comprehensive stage-aware test data:</strong></p>
                            <ul class='list-disc list-inside text-sm ml-4 space-y-1'>
                                <li>? 6 stage-aware test parts (STAGE-001 to STAGE-006)</li>
                                <li>?? 9 stage-specific machines (SLS, CNC, EDM, Laser, etc.)</li>
                                <li>?? 10 jobs in various stages of completion</li>
                                <li>?? Production stage executions showing progression</li>
                                <li>?? Build cohorts for testing cohort progression</li>
                                <li>?? Stage requirements linking parts to manufacturing stages</li>
                            </ul>
                            
                            <div class='mt-4 p-3 bg-blue-50 border border-blue-200 rounded'>
                                <p class='text-blue-800 font-medium'>?? Ready to test Stage Dashboards:</p>
                                <div class='mt-2 space-y-1 text-sm'>
                                    <div>
                                        <a href='/Operations/StageDashboard' class='text-blue-600 underline font-medium'>
                                            ?? Master Stage Dashboard
                                        </a>
                                        <span class='text-blue-600 ml-2'>- View all 7 stages with progress</span>
                                    </div>
                                    <div>
                                        <a href='/Operations/Dashboard' class='text-blue-600 underline font-medium'>
                                            ?? Operator Dashboard
                                        </a>
                                        <span class='text-blue-600 ml-2'>- Mobile-optimized punch in/out</span>
                                    </div>
                                    <div>
                                        <a href='/Operations/Stages/SLS' class='text-blue-600 underline font-medium'>
                                            ??? SLS Operations Dashboard
                                        </a>
                                        <span class='text-blue-600 ml-2'>- SLS-specific operations</span>
                                    </div>
                                    <div>
                                        <a href='/Operations/Stages/CNC' class='text-blue-600 underline font-medium'>
                                            ?? CNC Operations Dashboard
                                        </a>
                                        <span class='text-blue-600 ml-2'>- CNC-specific operations</span>
                                    </div>
                                </div>
                            </div>

                            <div class='mt-3 p-2 bg-green-100 rounded border-l-4 border-green-400'>
                                <p class='text-sm text-green-800'>
                                    <strong>?? Perfect for testing:</strong> Stage progression, operator workflows, 
                                    multi-stage manufacturing, approval processes, and real-time dashboard updates!
                                </p>
                            </div>
                        </div>
                    </div>
                ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [ADMIN] Error creating stage dashboard test data");
                return Content($@"
                    <div class='bg-red-50 border border-red-200 rounded-lg p-4'>
                        <div class='flex items-center'>
                            <svg class='w-5 h-5 text-red-500 mr-2' fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M6 18L18 6M6 6l12 12'></path>
                            </svg>
                            <span class='text-red-800 font-medium'>Error Creating Stage Dashboard Test Data</span>
                        </div>
                        <p class='text-red-700 mt-1'>Failed to create stage dashboard test data: {ex.Message}</p>
                        <details class='mt-2'>
                            <summary class='text-red-600 cursor-pointer'>Show details</summary>
                            <pre class='text-xs text-red-600 mt-1 whitespace-pre-wrap'>{ex.StackTrace}</pre>
                        </details>
                    </div>
                ");
            }
        }

        private async Task EnsureSlsMachinesExistAsync()
        {
            var existingMachines = await _context.Machines
                .Where(m => m.MachineType.ToUpper() == "SLS")
                .CountAsync();

            if (existingMachines == 0)
            {
                var machines = new[]
                {
                    new Machine 
                    { 
                        MachineId = "TI1", 
                        MachineName = "TruPrint 3000 #1", 
                        MachineType = "SLS", 
                        Status = "Idle", 
                        IsActive = true, 
                        IsAvailableForScheduling = true, 
                        Priority = 1,
                        Location = "Print Floor",
                        Department = "Printing",
                        CurrentMaterial = "SS316L",
                        SupportedMaterials = "SS316L,Inconel 625,AlSi10Mg",
                        BuildLengthMm = 250,
                        BuildWidthMm = 250,
                        BuildHeightMm = 325
                    },
                    new Machine 
                    { 
                        MachineId = "TI2", 
                        MachineName = "TruPrint 3000 #2", 
                        MachineType = "SLS", 
                        Status = "Idle", 
                        IsActive = true, 
                        IsAvailableForScheduling = true, 
                        Priority = 2,
                        Location = "Print Floor",
                        Department = "Printing",
                        CurrentMaterial = "SS316L",
                        SupportedMaterials = "SS316L,Inconel 625,AlSi10Mg",
                        BuildLengthMm = 250,
                        BuildWidthMm = 250,
                        BuildHeightMm = 325
                    },
                    new Machine 
                    { 
                        MachineId = "INC", 
                        MachineName = "Inconel Printer", 
                        MachineType = "SLS", 
                        Status = "Idle", 
                        IsActive = true, 
                        IsAvailableForScheduling = true, 
                        Priority = 3,
                        Location = "Print Floor",
                        Department = "Printing",
                        CurrentMaterial = "Inconel 625",
                        SupportedMaterials = "Inconel 625,SS316L",
                        BuildLengthMm = 200,
                        BuildWidthMm = 200,
                        BuildHeightMm = 280
                    }
                };

                _context.Machines.AddRange(machines);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created {MachineCount} SLS machines for testing", machines.Length);
            }
        }

        private async Task EnsureTestPartsExistAsync()
        {
            var existingParts = await _context.Parts.CountAsync();

            if (existingParts < 5)
            {
                var testParts = new[]
                {
                    new Part 
                    { 
                        PartNumber = "TEST-001", 
                        Description = "Test Bracket - Small", 
                        SlsMaterial = "SS316L", 
                        IsActive = true,
                        Industry = "Testing",
                        CreatedDate = DateTime.Now,
                        CreatedBy = "Admin Test Setup"
                    },
                    new Part 
                    { 
                        PartNumber = "TEST-002", 
                        Description = "Test Housing - Medium", 
                        SlsMaterial = "SS316L", 
                        IsActive = true,
                        Industry = "Testing",
                        CreatedDate = DateTime.Now,
                        CreatedBy = "Admin Test Setup"
                    },
                    new Part 
                    { 
                        PartNumber = "TEST-003", 
                        Description = "Test Component - Large", 
                        SlsMaterial = "Inconel 625", 
                        IsActive = true,
                        Industry = "Testing",
                        CreatedDate = DateTime.Now,
                        CreatedBy = "Admin Test Setup"
                    },
                    new Part 
                    { 
                        PartNumber = "TEST-004", 
                        Description = "Test Fixture - Complex", 
                        SlsMaterial = "AlSi10Mg", 
                        IsActive = true,
                        Industry = "Testing",
                        CreatedDate = DateTime.Now,
                        CreatedBy = "Admin Test Setup"
                    },
                    new Part 
                    { 
                        PartNumber = "TEST-005", 
                        Description = "Test Prototype - Experimental", 
                        SlsMaterial = "SS316L", 
                        IsActive = true,
                        Industry = "Testing",
                        CreatedDate = DateTime.Now,
                        CreatedBy = "Admin Test Setup"
                    }
                };

                _context.Parts.AddRange(testParts);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created {PartCount} test parts for testing", testParts.Length);
            }
        }
    }
}