namespace OpCentrix.Services
{
    /// <summary>
    /// Interface for the SLS data seeding service
    /// Provides methods for seeding the database with test data
    /// </summary>
    public interface ISlsDataSeedingService
    {
        /// <summary>
        /// Add example parts specifically designed for testing the scheduler UI
        /// These parts have varied durations, materials, and complexities
        /// </summary>
        Task AddExamplePartsForSchedulerTestingAsync();

        /// <summary>
        /// Remove all example parts created for testing
        /// </summary>
        Task RemoveExamplePartsAsync();

        /// <summary>
        /// Get count of example parts for testing
        /// </summary>
        Task<int> GetExamplePartsCountAsync();

        /// <summary>
        /// DEPRECATED: Use admin pages to add data instead
        /// This method is commented out to prevent automatic seeding
        /// </summary>
        Task SeedDatabaseAsync();

        /// <summary>
        /// DEPRECATED: Use admin pages to add data instead
        /// </summary>
        Task SeedDataAsync();

        /// <summary>
        /// DEPRECATED: Machines should be added via /Admin/Machines page
        /// Keeping method signature for compatibility but implementation is disabled
        /// </summary>
        Task SeedSlsMachinesAsync();

        /// <summary>
        /// DEPRECATED: Parts should be added via /Admin/Parts page
        /// </summary>
        Task SeedPartsAsync();

        /// <summary>
        /// DEPRECATED: Jobs should be created via the Scheduler interface
        /// </summary>
        Task SeedJobsAsync();
    }
}