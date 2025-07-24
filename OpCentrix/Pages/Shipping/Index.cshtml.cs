using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Authorization;
using OpCentrix.Data;
using OpCentrix.Models;
using System.Diagnostics;

namespace OpCentrix.Pages.Shipping
{
    [ShippingAccess]
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
        public int TotalShipments { get; set; }
        public int PendingShipments { get; set; }
        public int InTransitShipments { get; set; }
        public int DeliveredShipments { get; set; }
        public int OnHoldShipments { get; set; }
        public double AverageShippingTime { get; set; }
        public int OverdueShipments { get; set; }

        // Data collections - Using object lists until database tables are implemented
        public List<object> Shipments { get; set; } = new();
        public List<object> TodaysShipments { get; set; } = new();
        public List<object> UrgentShipments { get; set; } = new();
        public List<Part> AvailableParts { get; set; } = new();

        // Filtering and pagination
        public string SearchTerm { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public string PriorityFilter { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Placeholder properties - commented out until database table exists
        // [BindProperty]
        // public ShippingOperation NewShipment { get; set; } = new();

        // [BindProperty]
        // public List<ShippingItem> ShippingItems { get; set; } = new();

        public async Task OnGetAsync(string? search, string? status, string? priority, int? page)
        {
            try
            {
                SearchTerm = search ?? string.Empty;
                StatusFilter = status ?? string.Empty;
                PriorityFilter = priority ?? string.Empty;
                PageNumber = page ?? 1;

                await LoadDashboardMetricsAsync();
                await LoadShipmentsAsync();
                await LoadTodaysShipmentsAsync();
                await LoadUrgentShipmentsAsync();
                await LoadAvailablePartsAsync();

                _logger.LogInformation("Shipping operations page loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shipping operations page");
                await InitializeWithDefaults();
            }
        }

        public async Task<IActionResult> OnPostCreateShipmentAsync()
        {
            // TODO: Implement when ShippingOperation table is available
            TempData["Info"] = "Shipping operations feature is under development";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
        {
            // TODO: Implement when ShippingOperation table is available
            TempData["Info"] = "Shipping operations feature is under development";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddTrackingNumberAsync(int id, string trackingNumber)
        {
            // TODO: Implement when ShippingOperation table is available
            TempData["Info"] = "Shipping operations feature is under development";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteShipmentAsync(int id)
        {
            // TODO: Implement when ShippingOperation table is available
            TempData["Info"] = "Shipping operations feature is under development";
            return RedirectToPage();
        }

        private async Task LoadDashboardMetricsAsync()
        {
            // Placeholder implementation without database
            TotalShipments = 0;
            PendingShipments = 0;
            InTransitShipments = 0;
            DeliveredShipments = 0;
            OnHoldShipments = 0;
            AverageShippingTime = 0;
            OverdueShipments = 0;
        }

        private async Task LoadShipmentsAsync()
        {
            // Placeholder implementation without database
            Shipments = new List<object>();
            TotalCount = 0;
        }

        private async Task LoadTodaysShipmentsAsync()
        {
            // Placeholder implementation without database
            TodaysShipments = new List<object>();
        }

        private async Task LoadUrgentShipmentsAsync()
        {
            // Placeholder implementation without database
            UrgentShipments = new List<object>();
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
            await LoadShipmentsAsync();
            await LoadTodaysShipmentsAsync();
            await LoadUrgentShipmentsAsync();
            await LoadAvailablePartsAsync();
        }

        private async Task InitializeWithDefaults()
        {
            TotalShipments = 0;
            PendingShipments = 0;
            InTransitShipments = 0;
            DeliveredShipments = 0;
            OnHoldShipments = 0;
            AverageShippingTime = 0;
            OverdueShipments = 0;
            Shipments = new List<object>();
            TodaysShipments = new List<object>();
            UrgentShipments = new List<object>();
            AvailableParts = new List<Part>();
            TotalCount = 0;
        }

        // Helper methods for the view
        public string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "Delivered" => "bg-green-100 text-green-800",
                "Shipped" => "bg-blue-100 text-blue-800",
                "Packed" => "bg-yellow-100 text-yellow-800",
                "Pending" => "bg-gray-100 text-gray-800",
                _ => "bg-red-100 text-red-800"
            };
        }

        public string GetPriorityBadgeClass(string priority)
        {
            return priority switch
            {
                "Rush" => "bg-red-100 text-red-800",
                "Expedited" => "bg-orange-100 text-orange-800",
                "Standard" => "bg-green-100 text-green-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }

        // Placeholder helper method
        public string GetOverdueClass(object shipment)
        {
            return ""; // No overdue logic without database model
        }
    }
}