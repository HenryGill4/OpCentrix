using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Authorization;
using OpCentrix.ViewModels.Analytics;
using OpCentrix.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.Scheduler
{
    /// <summary>
    /// Master Schedule Page - Comprehensive production visibility dashboard
    /// Task 12: Master Schedule View with real-time analytics and reporting
    /// </summary>
    [SchedulerAccess]
    public class MasterScheduleModel : PageModel
    {
        private readonly IMasterScheduleService _masterScheduleService;
        private readonly ILogger<MasterScheduleModel> _logger;

        public MasterScheduleModel(
            IMasterScheduleService masterScheduleService,
            ILogger<MasterScheduleModel> logger)
        {
            _masterScheduleService = masterScheduleService;
            _logger = logger;
        }

        // Page properties
        public MasterScheduleViewModel ViewModel { get; set; } = new();
        public MasterScheduleFilters Filters { get; set; } = new();
        
        [BindProperty]
        public FilterInputModel Input { get; set; } = new();

        public async Task OnGetAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string viewMode = "weekly",
            string department = "all",
            string status = "all",
            string priority = "all",
            string search = "")
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [MASTER-SCHEDULE-{OperationId}] Loading master schedule page", operationId);

            try
            {
                // Set up filters from query parameters
                Filters = new MasterScheduleFilters
                {
                    StartDate = startDate ?? DateTime.Today,
                    EndDate = endDate ?? DateTime.Today.AddDays(30),
                    SearchTerm = search ?? string.Empty
                };

                // Add filter collections based on parameters
                if (department != "all" && !string.IsNullOrEmpty(department))
                    Filters.Departments.Add(department);
                
                if (status != "all" && !string.IsNullOrEmpty(status))
                    Filters.Statuses.Add(status);
                
                if (priority != "all" && !string.IsNullOrEmpty(priority))
                    Filters.Priorities.Add(priority);

                // Load master schedule data
                ViewModel = await _masterScheduleService.GetMasterScheduleAsync(Filters);
                ViewModel.ViewMode = viewMode;
                ViewModel.FilterDepartment = department;
                ViewModel.FilterStatus = status;
                ViewModel.FilterPriority = priority;

                // Set up input model for form binding
                Input = new FilterInputModel
                {
                    StartDate = Filters.StartDate ?? DateTime.Today,
                    EndDate = Filters.EndDate ?? DateTime.Today.AddDays(30),
                    ViewMode = viewMode,
                    Department = department,
                    Status = status,
                    Priority = priority,
                    SearchTerm = search ?? string.Empty
                };

                _logger.LogInformation("? [MASTER-SCHEDULE-{OperationId}] Master schedule loaded successfully: {JobCount} jobs, {MachineCount} machines, {AlertCount} alerts",
                    operationId, ViewModel.Jobs.Count, ViewModel.Machines.Count, ViewModel.Alerts.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-SCHEDULE-{OperationId}] Error loading master schedule: {ErrorMessage}",
                    operationId, ex.Message);
                
                // Provide fallback empty data
                ViewModel = new MasterScheduleViewModel();
                Input = new FilterInputModel();
            }
        }

        public async Task<IActionResult> OnPostApplyFiltersAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [MASTER-SCHEDULE-{OperationId}] Applying filters", operationId);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("?? [MASTER-SCHEDULE-{OperationId}] Invalid filter model state", operationId);
                    await OnGetAsync(); // Reload with default filters
                    return Page();
                }

                // Build query string for redirect
                var queryParams = new List<string>();
                
                if (Input.StartDate != DateTime.Today)
                    queryParams.Add($"startDate={Input.StartDate:yyyy-MM-dd}");
                
                if (Input.EndDate != DateTime.Today.AddDays(30))
                    queryParams.Add($"endDate={Input.EndDate:yyyy-MM-dd}");
                
                if (Input.ViewMode != "weekly")
                    queryParams.Add($"viewMode={Input.ViewMode}");
                
                if (Input.Department != "all")
                    queryParams.Add($"department={Input.Department}");
                
                if (Input.Status != "all")
                    queryParams.Add($"status={Input.Status}");
                
                if (Input.Priority != "all")
                    queryParams.Add($"priority={Input.Priority}");
                
                if (!string.IsNullOrEmpty(Input.SearchTerm))
                    queryParams.Add($"search={Uri.EscapeDataString(Input.SearchTerm)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                
                _logger.LogInformation("? [MASTER-SCHEDULE-{OperationId}] Redirecting with filters: {QueryString}",
                    operationId, queryString);

                return RedirectToPage("/Scheduler/MasterSchedule" + queryString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-SCHEDULE-{OperationId}] Error applying filters: {ErrorMessage}",
                    operationId, ex.Message);
                
                ModelState.AddModelError("", "Error applying filters. Please try again.");
                await OnGetAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnGetRefreshMetricsAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [MASTER-SCHEDULE-{OperationId}] Refreshing metrics data", operationId);

            try
            {
                var startDate = DateTime.Today;
                var endDate = DateTime.Today.AddDays(30);
                
                var metrics = await _masterScheduleService.CalculateMetricsAsync(startDate, endDate);
                
                _logger.LogDebug("? [MASTER-SCHEDULE-{OperationId}] Metrics refreshed successfully", operationId);
                
                return Partial("_MasterScheduleMetrics", metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-SCHEDULE-{OperationId}] Error refreshing metrics: {ErrorMessage}",
                    operationId, ex.Message);
                
                return Partial("_MasterScheduleMetrics", new MasterScheduleMetrics());
            }
        }

        public async Task<IActionResult> OnGetRefreshAlertsAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [MASTER-SCHEDULE-{OperationId}] Refreshing alerts data", operationId);

            try
            {
                var alerts = await _masterScheduleService.GetActiveAlertsAsync();
                
                _logger.LogDebug("? [MASTER-SCHEDULE-{OperationId}] Alerts refreshed: {AlertCount} active alerts",
                    operationId, alerts.Count);
                
                return Partial("_MasterScheduleAlerts", alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-SCHEDULE-{OperationId}] Error refreshing alerts: {ErrorMessage}",
                    operationId, ex.Message);
                
                return Partial("_MasterScheduleAlerts", new List<ScheduleAlert>());
            }
        }

        public async Task<IActionResult> OnGetTimelineDataAsync(DateTime? start = null, DateTime? end = null)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [MASTER-SCHEDULE-{OperationId}] Loading timeline data", operationId);

            try
            {
                var startDate = start ?? DateTime.Today;
                var endDate = end ?? DateTime.Today.AddDays(7);
                
                var timeline = await _masterScheduleService.GetTimelineDataAsync(startDate, endDate);
                
                _logger.LogDebug("? [MASTER-SCHEDULE-{OperationId}] Timeline data loaded: {SlotCount} time slots",
                    operationId, timeline.Count);
                
                return Partial("_MasterScheduleTimeline", timeline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-SCHEDULE-{OperationId}] Error loading timeline: {ErrorMessage}",
                    operationId, ex.Message);
                
                return Partial("_MasterScheduleTimeline", new List<MasterScheduleTimeSlot>());
            }
        }

        public async Task<IActionResult> OnGetUtilizationDataAsync(DateTime? start = null, DateTime? end = null)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [MASTER-SCHEDULE-{OperationId}] Loading utilization data", operationId);

            try
            {
                var startDate = start ?? DateTime.Today.AddDays(-30);
                var endDate = end ?? DateTime.Today;
                
                var utilization = await _masterScheduleService.GetResourceUtilizationAsync(startDate, endDate);
                
                _logger.LogDebug("? [MASTER-SCHEDULE-{OperationId}] Utilization data loaded: {ResourceCount} resources",
                    operationId, utilization.Count);
                
                return Partial("_MasterScheduleUtilization", utilization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-SCHEDULE-{OperationId}] Error loading utilization: {ErrorMessage}",
                    operationId, ex.Message);
                
                return Partial("_MasterScheduleUtilization", new List<ResourceUtilization>());
            }
        }

        public async Task<IActionResult> OnPostResolveAlertAsync(int alertId)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("? [MASTER-SCHEDULE-{OperationId}] Resolving alert {AlertId}", operationId, alertId);

            try
            {
                var username = User.Identity?.Name ?? "Unknown";
                var success = await _masterScheduleService.ResolveAlertAsync(alertId, username);
                
                if (success)
                {
                    _logger.LogInformation("? [MASTER-SCHEDULE-{OperationId}] Alert {AlertId} resolved by {User}",
                        operationId, alertId, username);
                    
                    return new JsonResult(new { success = true, message = "Alert resolved successfully" });
                }
                else
                {
                    _logger.LogWarning("?? [MASTER-SCHEDULE-{OperationId}] Failed to resolve alert {AlertId}",
                        operationId, alertId);
                    
                    return new JsonResult(new { success = false, message = "Failed to resolve alert" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-SCHEDULE-{OperationId}] Error resolving alert {AlertId}: {ErrorMessage}",
                    operationId, alertId, ex.Message);
                
                return new JsonResult(new { success = false, message = "Error resolving alert" });
            }
        }

        public async Task<IActionResult> OnGetExportScheduleAsync(
            string format = "excel",
            DateTime? start = null,
            DateTime? end = null,
            bool includeMetrics = true,
            bool includeCharts = true,
            bool includeAlerts = true)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [MASTER-SCHEDULE-{OperationId}] Exporting schedule as {Format}", operationId, format);

            try
            {
                var options = new MasterScheduleExportOptions
                {
                    Format = format,
                    StartDate = start ?? DateTime.Today,
                    EndDate = end ?? DateTime.Today.AddDays(30),
                    IncludeMetrics = includeMetrics,
                    IncludeCharts = includeCharts,
                    IncludeAlerts = includeAlerts,
                    TemplateType = "detailed"
                };

                var exportData = await _masterScheduleService.ExportScheduleAsync(options);
                
                var fileName = $"master-schedule-{DateTime.Today:yyyy-MM-dd}.{format}";
                var contentType = format.ToLower() switch
                {
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "pdf" => "application/pdf",
                    "csv" => "text/csv",
                    _ => "application/octet-stream"
                };

                _logger.LogInformation("? [MASTER-SCHEDULE-{OperationId}] Schedule exported successfully: {FileName}",
                    operationId, fileName);

                return File(exportData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-SCHEDULE-{OperationId}] Error exporting schedule: {ErrorMessage}",
                    operationId, ex.Message);
                
                return BadRequest("Error exporting schedule data");
            }
        }

        public async Task<IActionResult> OnPostRefreshRealTimeDataAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [MASTER-SCHEDULE-{OperationId}] Triggering real-time data refresh", operationId);

            try
            {
                await _masterScheduleService.RefreshRealTimeDataAsync();
                
                _logger.LogDebug("? [MASTER-SCHEDULE-{OperationId}] Real-time data refresh triggered", operationId);
                
                return new JsonResult(new { success = true, message = "Real-time data refresh triggered" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [MASTER-SCHEDULE-{OperationId}] Error refreshing real-time data: {ErrorMessage}",
                    operationId, ex.Message);
                
                return new JsonResult(new { success = false, message = "Error refreshing data" });
            }
        }
    }

    /// <summary>
    /// Input model for filter form binding
    /// </summary>
    public class FilterInputModel
    {
        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);

        [Display(Name = "View Mode")]
        public string ViewMode { get; set; } = "weekly";

        [Display(Name = "Department")]
        public string Department { get; set; } = "all";

        [Display(Name = "Status")]
        public string Status { get; set; } = "all";

        [Display(Name = "Priority")]
        public string Priority { get; set; } = "all";

        [Display(Name = "Search")]
        [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string SearchTerm { get; set; } = string.Empty;

        // Custom validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "End date must be after start date",
                    new[] { nameof(EndDate) });
            }

            if ((EndDate - StartDate).TotalDays > 365)
            {
                yield return new ValidationResult(
                    "Date range cannot exceed 365 days",
                    new[] { nameof(EndDate) });
            }
        }
    }
}