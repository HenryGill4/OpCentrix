using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using OpCentrix.Services;
using OpCentrix.ViewModels.PrintTracking;

namespace OpCentrix.Pages.ProductionBuild
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly IProductionBuildService _productionBuildService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(IProductionBuildService productionBuildService, ILogger<DashboardModel> logger)
        {
            _productionBuildService = productionBuildService;
            _logger = logger;
        }

        public ProductionBuildDashboardViewModel DashboardData { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                // Get current user ID (simplified for demo)
                var userId = 1; // In a real app, get from claims

                DashboardData = await _productionBuildService.GetDashboardDataAsync(userId);

                _logger.LogDebug("Loaded production build dashboard with {ActiveBuilds} active builds and {CompletedToday} completed today",
                    DashboardData.ActiveBuilds.Count, DashboardData.RecentCompletedBuilds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading production build dashboard");
                DashboardData = new ProductionBuildDashboardViewModel(); // Fallback to empty data
            }
        }
    }
}