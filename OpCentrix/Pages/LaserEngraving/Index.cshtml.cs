using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Authorization;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.LaserEngraving
{
    [LaserEngravingAccess]
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
        public int TodayRequests { get; set; } = 5;
        public int PendingJobs { get; set; } = 8;
        public int CompletedToday { get; set; } = 3;
        public int ActiveForms { get; set; } = 47; // Units to mark

        // Demo data collections
        public List<SerialNumberRequest> RecentRequests { get; set; } = new();
        public List<LaserEquipment> Equipment { get; set; } = new();

        [BindProperty]
        public SerialNumberRequestForm RequestForm { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                await LoadDashboardMetricsAsync();
                await LoadRecentRequestsAsync();
                LoadEquipmentStatus();

                _logger.LogInformation("Laser Engraving department page loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Laser Engraving page");
                InitializeDefaults();
            }
        }

        public async Task<IActionResult> OnPostSubmitRequestAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please correct the form errors and try again.";
                    await LoadDashboardMetricsAsync();
                    return Page();
                }

                // Generate request ID
                var requestId = await GenerateRequestIdAsync();

                // TODO: Send email to compliance department or create database record
                // For now, we'll just log it
                _logger.LogInformation("Serial number request {RequestId} created for model {ModelInfo} with quantity {Quantity}", 
                    requestId, RequestForm.ModelInfo, RequestForm.Quantity);

                TempData["Success"] = $"Serial number request {requestId} has been submitted to compliance department.";

                // Reset form
                RequestForm = new SerialNumberRequestForm();
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting serial number request");
                TempData["Error"] = "Failed to submit request. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnGetRequestDetailsAsync(string requestId)
        {
            try
            {
                // TODO: Implement when database table is available
                // For demo, return sample data
                var sampleRequest = new
                {
                    RequestId = requestId,
                    ComponentType = "Suppressor",
                    PartNumber = "BT-SUP-300BLK-01",
                    Customer = "John Doe",
                    Status = "Pending",
                    ATFForm = "Form 4",
                    Created = DateTime.Now.AddHours(-2),
                    EstimatedCompletion = DateTime.Now.AddDays(1)
                };

                return new JsonResult(sampleRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving request details for {RequestId}", requestId);
                return new JsonResult(new { error = "Failed to retrieve request details" });
            }
        }

        public async Task<IActionResult> OnPostStartJobAsync(string requestId)
        {
            try
            {
                // TODO: Implement job start functionality
                _logger.LogInformation("Starting laser engraving job for request {RequestId}", requestId);
                
                TempData["Success"] = $"Laser engraving job started for request {requestId}";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting job for request {RequestId}", requestId);
                TempData["Error"] = "Failed to start laser engraving job";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostCompleteJobAsync(string requestId, string serialNumber)
        {
            try
            {
                // TODO: Implement job completion functionality
                _logger.LogInformation("Completing laser engraving job for request {RequestId} with serial {SerialNumber}", 
                    requestId, serialNumber);
                
                TempData["Success"] = $"Laser engraving job completed. Serial number {serialNumber} assigned and engraved.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing job for request {RequestId}", requestId);
                TempData["Error"] = "Failed to complete laser engraving job";
                return RedirectToPage();
            }
        }

        private async Task LoadDashboardMetricsAsync()
        {
            // TODO: Load actual metrics from database when tables are available
            // For demo, using hardcoded values that simulate realistic metrics
            TodayRequests = 5;
            PendingJobs = 8;
            CompletedToday = 3;
            ActiveForms = 47; // Units to mark
        }

        private async Task LoadRecentRequestsAsync()
        {
            // Demo data - replace with actual database queries when available
            RecentRequests = new List<SerialNumberRequest>
            {
                new SerialNumberRequest
                {
                    RequestId = "REQ-2024-001",
                    ComponentType = "Print-XH 5.56",
                    PartNumber = "93",
                    Customer = "Normal",
                    Status = "Pending",
                    ATFForm = "",
                    Created = DateTime.Now.AddHours(-2)
                },
                new SerialNumberRequest
                {
                    RequestId = "REQ-2024-002",
                    ComponentType = "Suppressor-300BLK",
                    PartNumber = "25",
                    Customer = "Rush",
                    Status = "In Progress",
                    ATFForm = "",
                    Created = DateTime.Now.AddHours(-6)
                },
                new SerialNumberRequest
                {
                    RequestId = "REQ-2024-003",
                    ComponentType = "Receiver-308",
                    PartNumber = "15",
                    Customer = "Normal",
                    Status = "Completed",
                    ATFForm = "",
                    Created = DateTime.Now.AddDays(-1)
                }
            };
        }

        private void LoadEquipmentStatus()
        {
            // Demo equipment data
            Equipment = new List<LaserEquipment>
            {
                new LaserEquipment
                {
                    Id = "LASER-01",
                    Name = "Fiber Laser",
                    Type = "20W IPG Fiber",
                    Status = "Operational",
                    Materials = "Titanium, Steel, Aluminum",
                    QueueCount = 3,
                    LastMaintenance = DateTime.Now.AddDays(-2)
                },
                new LaserEquipment
                {
                    Id = "LASER-02",
                    Name = "CO2 Laser",
                    Type = "40W CO2",
                    Status = "In Use",
                    Materials = "Polymer, Coated Metals",
                    CurrentJob = "Print-XH 5.56 (25 units)",
                    EstimatedCompletion = DateTime.Now.AddMinutes(45)
                },
                new LaserEquipment
                {
                    Id = "LASER-03",
                    Name = "UV Laser",
                    Type = "5W UV Diode",
                    Status = "Maintenance",
                    Materials = "Precision Marking",
                    AvailableTime = DateTime.Now.AddDays(1).Date.AddHours(8)
                }
            };
        }

        private void LoadSupportedMaterials()
        {
            // Removed - no longer needed for simplified form
        }

        private void InitializeDefaults()
        {
            TodayRequests = 0;
            PendingJobs = 0;
            CompletedToday = 0;
            ActiveForms = 0;
            RecentRequests = new List<SerialNumberRequest>();
            Equipment = new List<LaserEquipment>();
        }

        private async Task<string> GenerateRequestIdAsync()
        {
            // Generate a unique request ID
            var year = DateTime.Now.Year;
            var sequence = new Random().Next(1000, 9999); // In production, use a proper sequence
            return $"REQ-{year}-{sequence:D3}";
        }

        // Helper methods for view
        public string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "Completed" => "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300",
                "In Progress" => "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300",
                "Pending" => "bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-300",
                "On Hold" => "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300",
                _ => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300"
            };
        }

        public string GetEquipmentStatusBadgeClass(string status)
        {
            return status switch
            {
                "Operational" => "bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300",
                "In Use" => "bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300",
                "Maintenance" => "bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-300",
                "Offline" => "bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-300",
                _ => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300"
            };
        }
    }

    // Demo data models - would typically be in Models folder
    public class SerialNumberRequest
    {
        public string RequestId { get; set; } = string.Empty;
        public string ComponentType { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ATFForm { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime? EstimatedCompletion { get; set; }
    }

    public class LaserEquipment
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Materials { get; set; } = string.Empty;
        public int QueueCount { get; set; }
        public string? CurrentJob { get; set; }
        public DateTime? LastMaintenance { get; set; }
        public DateTime? EstimatedCompletion { get; set; }
        public DateTime? AvailableTime { get; set; }
    }

    public class SerialNumberRequestForm
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Model Information")]
        public string ModelInfo { get; set; } = string.Empty;

        [Required]
        [Range(1, 500)]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        [Required]
        [Display(Name = "Priority Level")]
        public string Priority { get; set; } = "Normal";

        [StringLength(500)]
        [Display(Name = "Additional Notes")]
        public string? Notes { get; set; }
    }
}