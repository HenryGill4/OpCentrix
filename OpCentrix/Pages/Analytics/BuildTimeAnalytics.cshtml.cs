using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Services;
using OpCentrix.ViewModels.Analytics;
using OpCentrix.Authorization;

namespace OpCentrix.Pages.Analytics
{
    /// <summary>
    /// Phase 5 Build Time Analytics Dashboard
    /// Comprehensive build time analytics with machine learning capabilities
    /// </summary>
    [AnalyticsAccess]
    public class BuildTimeAnalyticsModel : PageModel
    {
        private readonly IBuildTimeAnalyticsService _analyticsService;
        private readonly ILogger<BuildTimeAnalyticsModel> _logger;

        public BuildTimeAnalyticsModel(IBuildTimeAnalyticsService analyticsService, ILogger<BuildTimeAnalyticsModel> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        #region Properties

        [BindProperty(SupportsGet = true)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow.AddDays(-30);

        [BindProperty(SupportsGet = true)]
        public DateTime EndDate { get; set; } = DateTime.UtcNow;

        [BindProperty(SupportsGet = true)]
        public string SelectedOperator { get; set; } = "All";

        [BindProperty(SupportsGet = true)]
        public string SelectedMachine { get; set; } = "All";

        public BuildTimeAnalyticsViewModel Analytics { get; set; } = new();
        public MachinePerformanceComparisonViewModel MachineComparison { get; set; } = new();
        public OperatorPerformanceViewModel OperatorPerformance { get; set; } = new();
        public List<BuildTimeTrend> TimeTrends { get; set; } = new();
        public List<QualityTrend> QualityTrends { get; set; } = new();
        public List<MachineUtilizationTrend> UtilizationTrends { get; set; } = new();
        public LearningModelInsights ModelInsights { get; set; } = new();
        public List<string> AvailableOperators { get; set; } = new();
        public List<string> AvailableMachines { get; set; } = new();

        #endregion

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading build time analytics dashboard for date range {StartDate} to {EndDate}", StartDate, EndDate);

                await LoadAnalyticsDataAsync();
                await LoadAvailableFiltersAsync();

                _logger.LogInformation("Build time analytics dashboard loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading build time analytics dashboard");
                Analytics.ErrorMessage = "Unable to load analytics data. Please try again.";
            }
        }

        public async Task<IActionResult> OnPostRefreshAsync()
        {
            try
            {
                _logger.LogInformation("Refreshing analytics data for date range {StartDate} to {EndDate}", StartDate, EndDate);
                
                await LoadAnalyticsDataAsync();
                await LoadAvailableFiltersAsync();

                TempData["SuccessMessage"] = "Analytics data refreshed successfully!";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing analytics data");
                TempData["ErrorMessage"] = "Failed to refresh analytics data. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdateLearningModelAsync()
        {
            try
            {
                _logger.LogInformation("Updating learning model");
                
                var success = await _analyticsService.UpdateLearningModelAsync();
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Learning model updated successfully!";
                }
                else
                {
                    TempData["WarningMessage"] = "Learning model update completed with warnings. Check logs for details.";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating learning model");
                TempData["ErrorMessage"] = "Failed to update learning model. Please try again.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetPredictBuildTimeAsync(string partNumber, string machineId, string? buildFileHash = null)
        {
            try
            {
                if (string.IsNullOrEmpty(partNumber) || string.IsNullOrEmpty(machineId))
                {
                    return BadRequest("Part number and machine ID are required.");
                }

                var prediction = await _analyticsService.PredictBuildTimeAsync(partNumber, machineId, buildFileHash);
                return new JsonResult(prediction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting build time for part {PartNumber} on machine {MachineId}", partNumber, machineId);
                return StatusCode(500, "Error generating prediction");
            }
        }

        public async Task<IActionResult> OnGetQualityPredictionAsync([FromQuery] BuildParameters parameters)
        {
            try
            {
                if (string.IsNullOrEmpty(parameters.MachineId) || string.IsNullOrEmpty(parameters.PartNumber))
                {
                    return BadRequest("Machine ID and part number are required.");
                }

                var prediction = await _analyticsService.PredictQualityOutcomeAsync(parameters);
                return new JsonResult(prediction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting quality for part {PartNumber}", parameters.PartNumber);
                return StatusCode(500, "Error generating quality prediction");
            }
        }

        public async Task<IActionResult> OnGetOptimizationSuggestionsAsync()
        {
            try
            {
                var suggestions = await _analyticsService.GetOptimizationSuggestionsAsync();
                return new JsonResult(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading optimization suggestions");
                return StatusCode(500, "Error loading suggestions");
            }
        }

        #region Private Methods

        private async Task LoadAnalyticsDataAsync()
        {
            // Load main analytics data
            Analytics = await _analyticsService.GetBuildTimeAnalyticsAsync(StartDate, EndDate);

            // Load machine performance comparison
            MachineComparison = await _analyticsService.GetMachinePerformanceComparisonAsync();

            // Load operator performance if specific operator selected
            if (SelectedOperator != "All" && !string.IsNullOrEmpty(SelectedOperator))
            {
                OperatorPerformance = await _analyticsService.GetOperatorPerformanceAsync(SelectedOperator);
            }

            // Load trend data
            TimeTrends = await _analyticsService.GetBuildTimeTrendsAsync(30);
            QualityTrends = await _analyticsService.GetQualityTrendsAsync(30);
            UtilizationTrends = await _analyticsService.GetMachineUtilizationTrendsAsync();

            // Load learning model insights
            ModelInsights = await _analyticsService.GetLearningModelInsightsAsync();
        }

        private async Task LoadAvailableFiltersAsync()
        {
            try
            {
                // This would typically come from the database
                AvailableOperators = new List<string> { "All", "Operator1", "Operator2", "Operator3" };
                AvailableMachines = new List<string> { "All", "TI1", "TI2", "TI3" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading available filters");
                AvailableOperators = new List<string> { "All" };
                AvailableMachines = new List<string> { "All" };
            }
        }

        #endregion

        #region Helper Methods for View

        public string GetAccuracyBadgeClass(decimal accuracy)
        {
            return accuracy switch
            {
                >= 90 => "badge-success",
                >= 80 => "badge-primary",
                >= 70 => "badge-warning",
                _ => "badge-danger"
            };
        }

        public string GetQualityBadgeClass(decimal quality)
        {
            return quality switch
            {
                >= 95 => "badge-success",
                >= 85 => "badge-warning",
                _ => "badge-danger"
            };
        }

        public string GetTrendIcon(string trend)
        {
            return trend switch
            {
                "Improving" => "??",
                "Declining" => "??",
                "Stable" => "??",
                _ => "?"
            };
        }

        public string GetPriorityBadgeClass(string priority)
        {
            return priority switch
            {
                "High" => "badge-danger",
                "Medium" => "badge-warning",
                "Low" => "badge-info",
                _ => "badge-secondary"
            };
        }

        public string GetCategoryIcon(string category)
        {
            return category switch
            {
                "Machine Utilization" => "??",
                "Quality Improvement" => "??",
                "Time Estimation" => "?",
                "Cost Optimization" => "??",
                _ => "??"
            };
        }

        #endregion
    }
}