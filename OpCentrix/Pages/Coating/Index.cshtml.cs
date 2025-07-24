using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Authorization;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Pages.Coating
{
    [CoatingAccess]
    public class IndexModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(SchedulerContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Dashboard metrics
        public int TotalOperations { get; set; }
        public int PendingOperations { get; set; }
        public int InProgressOperations { get; set; }
        public int CompletedOperations { get; set; }
        public int RejectedOperations { get; set; }
        public double OverallQualityRate { get; set; }

        // Data collections - Using object lists until database tables are implemented
        public List<object> Operations { get; set; } = new();
        public List<object> TodaysOperations { get; set; } = new();
        public List<Part> AvailableParts { get; set; } = new();

        // Filtering and pagination
        public string SearchTerm { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public string CoatingTypeFilter { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Placeholder for new operation - commented out until database table exists
        // [BindProperty]
        // public CoatingOperation NewOperation { get; set; } = new();

        public async Task OnGetAsync(string? search, string? status, string? coatingType, int? page)
        {
            try
            {
                SearchTerm = search ?? string.Empty;
                StatusFilter = status ?? string.Empty;
                CoatingTypeFilter = coatingType ?? string.Empty;
                PageNumber = page ?? 1;

                await LoadDashboardMetricsAsync();
                await LoadOperationsAsync();
                await LoadTodaysOperationsAsync();
                await LoadAvailablePartsAsync();

                _logger.LogInformation("Coating operations page loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading coating operations page");
                await InitializeWithDefaults();
            }
        }

        public async Task<IActionResult> OnPostCreateOperationAsync()
        {
            // TODO: Implement when CoatingOperation table is available
            TempData["Info"] = "Coating operations feature is under development";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
        {
            // TODO: Implement when CoatingOperation table is available
            TempData["Info"] = "Coating operations feature is under development";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteOperationAsync(int id)
        {
            // TODO: Implement when CoatingOperation table is available
            TempData["Info"] = "Coating operations feature is under development";
            return RedirectToPage();
        }

        private async Task LoadDashboardMetricsAsync()
        {
            // Placeholder implementation without database
            TotalOperations = 0;
            PendingOperations = 0;
            InProgressOperations = 0;
            CompletedOperations = 0;
            RejectedOperations = 0;
            OverallQualityRate = 100;
        }

        private async Task LoadOperationsAsync()
        {
            // Placeholder implementation without database
            Operations = new List<object>();
            TotalCount = 0;
        }

        private async Task LoadTodaysOperationsAsync()
        {
            // Placeholder implementation without database
            TodaysOperations = new List<object>();
        }

        private async Task LoadAvailablePartsAsync()
        {
            // Load from actual Parts table (this still exists)
            AvailableParts = await _context.Parts
                .Where(p => p.IsActive)
                .OrderBy(p => p.PartNumber)
                .ToListAsync();
        }

        private async Task LoadPageDataAsync()
        {
            await LoadDashboardMetricsAsync();
            await LoadOperationsAsync();
            await LoadTodaysOperationsAsync();
            await LoadAvailablePartsAsync();
        }

        private async Task InitializeWithDefaults()
        {
            TotalOperations = 0;
            PendingOperations = 0;
            InProgressOperations = 0;
            CompletedOperations = 0;
            RejectedOperations = 0;
            OverallQualityRate = 100;
            Operations = new List<object>();
            TodaysOperations = new List<object>();
            AvailableParts = new List<Part>();
            TotalCount = 0;
        }

        // Helper methods for the view
        public string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "Completed" => "bg-green-100 text-green-800",
                "In Progress" => "bg-yellow-100 text-yellow-800",
                "Rejected" => "bg-red-100 text-red-800",
                "Pending" => "bg-gray-100 text-gray-800",
                _ => "bg-blue-100 text-blue-800"
            };
        }

        public string GetPriorityColor(string status)
        {
            return status switch
            {
                "Rush" => "text-red-600",
                "Expedited" => "text-orange-600",
                "Standard" => "text-green-600",
                _ => "text-gray-600"
            };
        }
    }
}