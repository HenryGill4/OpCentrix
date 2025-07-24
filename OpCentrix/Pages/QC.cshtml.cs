using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Authorization;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Pages
{
    [QCAccess]
    public class QCModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<QCModel> _logger;

        public QCModel(SchedulerContext context, ILogger<QCModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Dashboard metrics
        public int TotalInspections { get; set; }
        public int PendingInspections { get; set; }
        public int InProgressInspections { get; set; }
        public int PassedInspections { get; set; }
        public int FailedInspections { get; set; }
        public int OnHoldInspections { get; set; }
        public double OverallPassRate { get; set; }
        public int OverdueInspections { get; set; }

        // Data collections - Using object lists until database tables are implemented
        public List<object> Inspections { get; set; } = new();
        public List<object> TodaysInspections { get; set; } = new();
        public List<object> UrgentInspections { get; set; } = new();
        public List<Part> AvailableParts { get; set; } = new();

        // Filtering and pagination
        public string SearchTerm { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public string InspectionTypeFilter { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Placeholder for new inspection - commented out until database table exists
        // [BindProperty]
        // public QualityInspection NewInspection { get; set; } = new();

        public async Task OnGetAsync(string? search, string? status, string? inspectionType, int? page)
        {
            try
            {
                SearchTerm = search ?? string.Empty;
                StatusFilter = status ?? string.Empty;
                InspectionTypeFilter = inspectionType ?? string.Empty;
                PageNumber = page ?? 1;

                await LoadDashboardMetricsAsync();
                await LoadInspectionsAsync();
                await LoadTodaysInspectionsAsync();
                await LoadUrgentInspectionsAsync();
                await LoadAvailablePartsAsync();

                _logger.LogInformation("Quality inspections page loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading quality inspections page");
                await InitializeWithDefaults();
            }
        }

        public async Task<IActionResult> OnPostCreateInspectionAsync()
        {
            // TODO: Implement when QualityInspection table is available
            TempData["Info"] = "Quality inspection feature is under development";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
        {
            // TODO: Implement when QualityInspection table is available
            TempData["Info"] = "Quality inspection feature is under development";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteInspectionAsync(int id)
        {
            // TODO: Implement when QualityInspection table is available
            TempData["Info"] = "Quality inspection feature is under development";
            return RedirectToPage();
        }

        private async Task LoadDashboardMetricsAsync()
        {
            // Placeholder implementation without database
            TotalInspections = 0;
            PendingInspections = 0;
            InProgressInspections = 0;
            PassedInspections = 0;
            FailedInspections = 0;
            OnHoldInspections = 0;
            OverallPassRate = 100;
            OverdueInspections = 0;
        }

        private async Task LoadInspectionsAsync()
        {
            // Placeholder implementation without database
            Inspections = new List<object>();
            TotalCount = 0;
        }

        private async Task LoadTodaysInspectionsAsync()
        {
            // Placeholder implementation without database
            TodaysInspections = new List<object>();
        }

        private async Task LoadUrgentInspectionsAsync()
        {
            // Placeholder implementation without database
            UrgentInspections = new List<object>();
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
            await LoadInspectionsAsync();
            await LoadTodaysInspectionsAsync();
            await LoadUrgentInspectionsAsync();
            await LoadAvailablePartsAsync();
        }

        private async Task InitializeWithDefaults()
        {
            TotalInspections = 0;
            PendingInspections = 0;
            InProgressInspections = 0;
            PassedInspections = 0;
            FailedInspections = 0;
            OnHoldInspections = 0;
            OverallPassRate = 100;
            OverdueInspections = 0;
            Inspections = new List<object>();
            TodaysInspections = new List<object>();
            UrgentInspections = new List<object>();
            AvailableParts = new List<Part>();
            TotalCount = 0;
        }

        // Helper methods for the view
        public string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "Pass" => "bg-green-100 text-green-800",
                "Fail" => "bg-red-100 text-red-800",
                "In Progress" => "bg-blue-100 text-blue-800",
                "Hold" => "bg-yellow-100 text-yellow-800",
                "Pending" => "bg-gray-100 text-gray-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }

        public string GetPriorityColor(string priority)
        {
            return priority switch
            {
                "Critical" => "text-red-600",
                "High" => "text-orange-600",
                "Medium" => "text-yellow-600",
                "Low" => "text-green-600",
                _ => "text-gray-600"
            };
        }
    }
}
