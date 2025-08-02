using OpCentrix.Models;

namespace OpCentrix.Services.Admin
{
    /// <summary>
    /// Service interface for seeding default production stages
    /// </summary>
    public interface IProductionStageSeederService
    {
        /// <summary>
        /// Seeds default production stages if none exist
        /// </summary>
        /// <returns>Number of stages seeded</returns>
        Task<int> SeedDefaultStagesAsync();
        
        /// <summary>
        /// Gets all available production stages
        /// </summary>
        /// <returns>List of active production stages</returns>
        Task<List<ProductionStage>> GetAvailableStagesAsync();
        
        /// <summary>
        /// Checks if default stages exist
        /// </summary>
        /// <returns>True if stages exist, false otherwise</returns>
        Task<bool> DefaultStagesExistAsync();
    }
}