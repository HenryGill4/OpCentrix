using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Authorization;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Pages
{
    [MachiningAccess]
    public class MachiningModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<MachiningModel> _logger;

        public MachiningModel(SchedulerContext context, ILogger<MachiningModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Dashboard metrics
        public int TotalOperations { get; set; }
        public int SetupOperations { get; set; }
        public int RunningOperations { get; set; }
        public int CompletedOperations { get; set; }
        public int OnHoldOperations { get; set; }
        public double AverageEfficiency { get; set; }
        public double TotalMachiningHours { get; set; }

        // Data collections - Using object lists until database tables are implemented
        public List<object> Operations { get; set; } = new();
        public List<object> ActiveOperations { get; set; } = new();
        public List<Part> AvailableParts { get; set; } = new();

        // Machine status tracking
        public Dictionary<string, string> MachineStatuses { get; set; } = new();

        // Filtering and pagination
        public string SearchTerm { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public string MachineFilter { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Placeholder for new operation - commented out until database table exists
        // [BindProperty]
        // public MachiningOperation NewOperation { get; set; } = new();

        public async Task OnGetAsync(string? search, string? status, string? machine, int? page)
        {
            try
            {
                SearchTerm = search ?? string.Empty;
                StatusFilter = status ?? string.Empty;
                MachineFilter = machine ?? string.Empty;
                PageNumber = page ?? 1;

                await LoadDashboardMetricsAsync();
                await LoadOperationsAsync();
                await LoadActiveOperationsAsync();
                await LoadAvailablePartsAsync();
                await LoadMachineStatusesAsync();

                _logger.LogInformation("Machining operations page loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading machining operations page");
                await InitializeWithDefaults();
            }
        }

        public async Task<IActionResult> OnPostCreateOperationAsync()
        {
            // TODO: Implement when MachiningOperation table is available
            TempData["Info"] = "Machining operations feature is under development";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
        {
            // TODO: Implement when MachiningOperation table is available
            TempData["Info"] = "Machining operations feature is under development";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteOperationAsync(int id)
        {
            // TODO: Implement when MachiningOperation table is available
            TempData["Info"] = "Machining operations feature is under development";
            return RedirectToPage();
        }

        private async Task LoadDashboardMetricsAsync()
        {
            // Placeholder implementation without database
            TotalOperations = 0;
            SetupOperations = 0;
            RunningOperations = 0;
            CompletedOperations = 0;
            OnHoldOperations = 0;
            AverageEfficiency = 0;
            TotalMachiningHours = 0;
        }

        private async Task LoadOperationsAsync()
        {
            // Placeholder implementation without database
            Operations = new List<object>();
            TotalCount = 0;
        }

        private async Task LoadActiveOperationsAsync()
        {
            // Placeholder implementation without database
            ActiveOperations = new List<object>();
        }

        private async Task LoadAvailablePartsAsync()
        {
            // Load from actual Parts table (this still exists)
            AvailableParts = await _context.Parts
                .Where(p => p.IsActive)
                .OrderBy(p => p.PartNumber)
                .ToListAsync();
        }

        private async Task LoadMachineStatusesAsync()
        {
            // Placeholder implementation without database
            MachineStatuses = new Dictionary<string, string>
            {
                { "CNC-MILL-1", "Idle" },
                { "CNC-MILL-2", "Idle" },
                { "CNC-MILL-3", "Idle" },
                { "CNC-LATHE-1", "Idle" },
                { "CNC-LATHE-2", "Idle" },
                { "DRILL-PRESS-1", "Idle" }
            };
        }

        private async Task LoadPageDataAsync()
        {
            await LoadDashboardMetricsAsync();
            await LoadOperationsAsync();
            await LoadActiveOperationsAsync();
            await LoadAvailablePartsAsync();
            await LoadMachineStatusesAsync();
        }

        private async Task InitializeWithDefaults()
        {
            TotalOperations = 0;
            SetupOperations = 0;
            RunningOperations = 0;
            CompletedOperations = 0;
            OnHoldOperations = 0;
            AverageEfficiency = 0;
            TotalMachiningHours = 0;
            Operations = new List<object>();
            ActiveOperations = new List<object>();
            AvailableParts = new List<Part>();
            MachineStatuses = new Dictionary<string, string>();
            TotalCount = 0;
        }

        // Helper methods for the view
        public string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "Completed" => "bg-green-100 text-green-800",
                "Running" => "bg-blue-100 text-blue-800",
                "Setup" => "bg-yellow-100 text-yellow-800",
                "On Hold" => "bg-red-100 text-red-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }

        public string GetMachineStatusColor(string status)
        {
            return status switch
            {
                "Running" => "text-green-600",
                "Setup" => "text-yellow-600",
                "Maintenance" => "text-red-600",
                "Idle" => "text-gray-600",
                _ => "text-gray-600"
            };
        }
    }
}
