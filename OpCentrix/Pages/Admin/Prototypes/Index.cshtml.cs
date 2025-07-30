using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Services.Admin;
using OpCentrix.ViewModels.Admin.Prototypes;

namespace OpCentrix.Pages.Admin.Prototypes
{
    /// <summary>
    /// Prototype tracking dashboard showing active prototypes, pipeline metrics, and bottlenecks
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class IndexModel : PageModel
    {
        private readonly PrototypeTrackingService _prototypeService;
        private readonly ProductionStageService _stageService;
        private readonly AssemblyComponentService _componentService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            PrototypeTrackingService prototypeService,
            ProductionStageService stageService,
            AssemblyComponentService componentService,
            ILogger<IndexModel> logger)
        {
            _prototypeService = prototypeService;
            _stageService = stageService;
            _componentService = componentService;
            _logger = logger;
        }

        public PrototypeDashboardViewModel ViewModel { get; set; } = new PrototypeDashboardViewModel();

        public async Task OnGetAsync()
        {
            try
            {
                await LoadDashboardDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading prototype dashboard");
                TempData["ErrorMessage"] = "Error loading dashboard data. Please try again.";
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            // Load active prototypes
            var activePrototypes = await _prototypeService.GetActivePrototypeJobsAsync();
            
            // Convert to summary format
            ViewModel.ActivePrototypes = activePrototypes.Select(p => new PrototypeJobSummary
            {
                Id = p.Id,
                PrototypeNumber = p.PrototypeNumber,
                PartName = p.Part?.Name ?? "Unknown Part",
                Status = p.Status,
                Priority = p.Priority,
                RequestDate = p.RequestDate,
                RequestedBy = p.RequestedBy,
                DaysInProcess = p.StartDate.HasValue 
                    ? (int)(DateTime.UtcNow - p.StartDate.Value).TotalDays 
                    : (int)(DateTime.UtcNow - p.RequestDate).TotalDays,
                CurrentStage = GetCurrentStageName(p),
                CurrentStageOrder = GetCurrentStageOrder(p),
                TotalStages = p.StageExecutions.Count,
                ProgressPercentage = CalculateProgressPercentage(p)
            }).ToList();

            // Load pipeline metrics
            await LoadPipelineMetricsAsync();

            // Load bottlenecks
            await LoadBottlenecksAsync();
        }

        private string GetCurrentStageName(OpCentrix.Models.PrototypeJob prototype)
        {
            var currentStage = prototype.StageExecutions
                .Where(se => se.Status == "InProgress")
                .OrderBy(se => se.ProductionStage?.DisplayOrder)
                .FirstOrDefault();

            if (currentStage != null)
                return currentStage.ProductionStage?.Name ?? "Unknown";

            // If no stage is in progress, find the next pending stage
            var nextStage = prototype.StageExecutions
                .Where(se => se.Status == "NotStarted")
                .OrderBy(se => se.ProductionStage?.DisplayOrder)
                .FirstOrDefault();

            return nextStage?.ProductionStage?.Name ?? "Completed";
        }

        private int GetCurrentStageOrder(OpCentrix.Models.PrototypeJob prototype)
        {
            var currentStage = prototype.StageExecutions
                .Where(se => se.Status == "InProgress")
                .OrderBy(se => se.ProductionStage?.DisplayOrder)
                .FirstOrDefault();

            if (currentStage != null)
                return currentStage.ProductionStage?.DisplayOrder ?? 0;

            // Count completed stages + 1
            var completedCount = prototype.StageExecutions.Count(se => se.Status == "Completed");
            return completedCount + 1;
        }

        private decimal CalculateProgressPercentage(OpCentrix.Models.PrototypeJob prototype)
        {
            if (prototype.StageExecutions.Count == 0) return 0;

            var completedStages = prototype.StageExecutions.Count(se => se.Status == "Completed");
            var inProgressStages = prototype.StageExecutions.Count(se => se.Status == "InProgress");
            
            // Completed stages count as 100%, in-progress as 50%
            var weightedProgress = completedStages + (inProgressStages * 0.5m);
            
            return Math.Round((weightedProgress / prototype.StageExecutions.Count) * 100, 1);
        }

        private async Task LoadPipelineMetricsAsync()
        {
            // Calculate metrics for the last 30 days
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            
            // For now, we'll use placeholder data since we need more complex queries
            ViewModel.Metrics = new PipelineMetrics
            {
                PrototypesStarted = ViewModel.ActivePrototypes.Count(p => p.RequestDate >= thirtyDaysAgo),
                PrototypesCompleted = 0, // Would need to query completed prototypes
                AverageLeadTimeDays = 14.2m,
                AverageCostVariancePercent = 12.5m,
                AverageTimeVariancePercent = 8.3m,
                TotalCostSavings = 2500.00m,
                QualityIssuesCount = 2
            };
        }

        private async Task LoadBottlenecksAsync()
        {
            // Analyze stage bottlenecks
            var stageBottlenecks = new List<BottleneckAlert>();

            // Find stages with multiple jobs waiting
            var stageGroups = ViewModel.ActivePrototypes
                .GroupBy(p => p.CurrentStage)
                .Where(g => g.Count() > 2)
                .ToList();

            foreach (var group in stageGroups)
            {
                var avgWaitTime = group.Average(p => p.DaysInProcess);
                
                stageBottlenecks.Add(new BottleneckAlert
                {
                    Type = "Stage",
                    StageName = group.Key,
                    JobsWaiting = group.Count(),
                    AverageWaitTimeDays = (decimal)avgWaitTime,
                    Description = $"{group.Key}: {group.Count()} jobs waiting (avg wait: {avgWaitTime:F1} days)",
                    Severity = group.Count() > 3 ? "High" : "Medium"
                });
            }

            // Component bottlenecks (placeholder)
            if (ViewModel.ActivePrototypes.Any(p => p.CurrentStage == "Assembly"))
            {
                stageBottlenecks.Add(new BottleneckAlert
                {
                    Type = "Component",
                    StageName = "Assembly",
                    JobsWaiting = 2,
                    AverageWaitTimeDays = 3.5m,
                    Description = "Assembly Components: 2 jobs waiting for end caps",
                    Severity = "Medium"
                });
            }

            ViewModel.Bottlenecks = stageBottlenecks;
        }
    }
}