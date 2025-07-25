using OpCentrix.Data;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Services
{
    /// <summary>
    /// Service to validate database integrity and production readiness
    /// </summary>
    public class DatabaseValidationService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<DatabaseValidationService> _logger;

        public DatabaseValidationService(SchedulerContext context, ILogger<DatabaseValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DatabaseValidationResult> ValidateDatabaseAsync()
        {
            var result = new DatabaseValidationResult();

            try
            {
                // Check for sample data
                result.HasSampleData = await HasSampleDataAsync();
                
                // Validate essential data
                result.HasUsers = await _context.Users.AnyAsync();
                result.HasMachines = await _context.SlsMachines.AnyAsync();
                
                // Count real data
                result.RealPartsCount = await CountRealPartsAsync();
                result.RealJobsCount = await CountRealJobsAsync();
                
                // Check database connectivity
                result.DatabaseConnected = await TestDatabaseConnectionAsync();
                
                // Production readiness assessment
                result.IsProductionReady = !result.HasSampleData && 
                                          result.HasUsers && 
                                          result.HasMachines && 
                                          result.DatabaseConnected;

                _logger.LogInformation("Database validation completed: ProductionReady={ProductionReady}, SampleData={SampleData}, RealParts={RealParts}, RealJobs={RealJobs}", 
                    result.IsProductionReady, result.HasSampleData, result.RealPartsCount, result.RealJobsCount);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database validation");
                result.ValidationError = ex.Message;
                return result;
            }
        }

        private async Task<bool> HasSampleDataAsync()
        {
            var samplePartNumbers = new[] { "14-5396", "14-5397", "14-5398", "14-5399", "14-5400", "14-5401", "14-5402", "14-5403", "10-0001", "10-0002" };

            var hasSampleParts = await _context.Parts
                .AnyAsync(p => samplePartNumbers.Contains(p.PartNumber) || 
                              p.CreatedBy == "Engineer" || 
                              p.CreatedBy == "System" ||
                              p.Industry == "General");

            var hasSampleJobs = await _context.Jobs
                .AnyAsync(j => samplePartNumbers.Contains(j.PartNumber) || 
                              j.CreatedBy == "Scheduler" ||
                              j.Operator == "John Smith");

            return hasSampleParts || hasSampleJobs;
        }

        private async Task<int> CountRealPartsAsync()
        {
            var samplePartNumbers = new[] { "14-5396", "14-5397", "14-5398", "14-5399", "14-5400", "14-5401", "14-5402", "14-5403", "10-0001", "10-0002" };

            return await _context.Parts
                .Where(p => !samplePartNumbers.Contains(p.PartNumber) && 
                           p.CreatedBy != "Engineer" && 
                           p.CreatedBy != "System" &&
                           p.Industry != "General")
                .CountAsync();
        }

        private async Task<int> CountRealJobsAsync()
        {
            var samplePartNumbers = new[] { "14-5396", "14-5397", "14-5398", "14-5399", "14-5400", "14-5401", "14-5402", "14-5403", "10-0001", "10-0002" };

            return await _context.Jobs
                .Where(j => !samplePartNumbers.Contains(j.PartNumber) && 
                           j.CreatedBy != "Scheduler" &&
                           j.Operator != "John Smith")
                .CountAsync();
        }

        private async Task<bool> TestDatabaseConnectionAsync()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class DatabaseValidationResult
    {
        public bool IsProductionReady { get; set; }
        public bool HasSampleData { get; set; }
        public bool HasUsers { get; set; }
        public bool HasMachines { get; set; }
        public bool DatabaseConnected { get; set; }
        public int RealPartsCount { get; set; }
        public int RealJobsCount { get; set; }
        public string? ValidationError { get; set; }

        public string GetStatusMessage()
        {
            if (!string.IsNullOrEmpty(ValidationError))
                return $"ERROR: Database Error: {ValidationError}";

            if (!DatabaseConnected)
                return "ERROR: Database Connection Failed";

            if (!HasUsers || !HasMachines)
                return "WARNING: Missing Essential Data (Users/Machines)";

            if (HasSampleData)
                return "WARNING: Sample Data Detected - Remove for Production";

            if (RealPartsCount == 0)
                return "INFO: Ready for Your Parts - Add Real Manufacturing Data";

            return $"SUCCESS: Production Ready - {RealPartsCount} parts, {RealJobsCount} jobs";
        }
    }
}