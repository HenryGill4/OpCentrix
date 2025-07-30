using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Services.Admin;
using OpCentrix.ViewModels.Admin.Prototypes;
using OpCentrix.Models;

namespace OpCentrix.Pages.Admin.Prototypes
{
    /// <summary>
    /// Detailed prototype job tracking page showing stage-by-stage progress, time/cost analysis, and component management
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class DetailsModel : PageModel
    {
        private readonly PrototypeTrackingService _prototypeService;
        private readonly ProductionStageService _stageService;
        private readonly AssemblyComponentService _componentService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(
            PrototypeTrackingService prototypeService,
            ProductionStageService stageService,
            AssemblyComponentService componentService,
            ILogger<DetailsModel> logger)
        {
            _prototypeService = prototypeService;
            _stageService = stageService;
            _componentService = componentService;
            _logger = logger;
        }

        public PrototypeDetailsViewModel ViewModel { get; set; } = new PrototypeDetailsViewModel();

        [BindProperty]
        public int PrototypeJobId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                PrototypeJobId = id;
                await LoadPrototypeDetailsAsync(id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading prototype details for ID: {PrototypeJobId}", id);
                TempData["ErrorMessage"] = "Error loading prototype details. Please try again.";
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostStartStageAsync(int prototypeJobId, int stageId)
        {
            try
            {
                var executor = User.Identity?.Name ?? "System";
                var success = await _prototypeService.StartStageAsync(prototypeJobId, stageId, executor);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Stage started successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to start stage. Please check prerequisites.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting stage {StageId} for prototype {PrototypeJobId}", stageId, prototypeJobId);
                TempData["ErrorMessage"] = "Error starting stage. Please try again.";
            }

            return RedirectToPage("Details", new { id = prototypeJobId });
        }

        public async Task<IActionResult> OnPostCompleteStageAsync(int stageExecutionId, decimal actualHours, decimal actualCost)
        {
            try
            {
                var success = await _prototypeService.CompleteStageAsync(stageExecutionId, actualHours, actualCost);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Stage completed successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to complete stage. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing stage execution {StageExecutionId}", stageExecutionId);
                TempData["ErrorMessage"] = "Error completing stage. Please try again.";
            }

            return RedirectToPage("Details", new { id = PrototypeJobId });
        }

        public async Task<IActionResult> OnPostLogTimeAsync(int stageExecutionId, DateTime startTime, DateTime? endTime, string activityType, string activityDescription)
        {
            try
            {
                var success = await _prototypeService.LogTimeAsync(stageExecutionId, startTime, endTime, activityType + ": " + activityDescription);
                
                if (success)
                {
                    return new JsonResult(new { success = true, message = "Time logged successfully" });
                }
                else
                {
                    return BadRequest("Failed to log time");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging time for stage execution {StageExecutionId}", stageExecutionId);
                return BadRequest("Error logging time");
            }
        }

        public async Task<IActionResult> OnPostAddComponentAsync([FromBody] AssemblyComponent component)
        {
            try
            {
                component.CreatedBy = User.Identity?.Name ?? "System";
                var success = await _componentService.AddComponentAsync(component);
                
                if (success)
                {
                    return new JsonResult(new { success = true, message = "Component added successfully" });
                }
                else
                {
                    return BadRequest("Failed to add component");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding assembly component");
                return BadRequest("Error adding component");
            }
        }

        private async Task LoadPrototypeDetailsAsync(int prototypeJobId)
        {
            // Load the prototype job
            ViewModel.PrototypeJob = await _prototypeService.GetPrototypeJobDetailsAsync(prototypeJobId);
            if (ViewModel.PrototypeJob == null)
            {
                throw new InvalidOperationException($"Prototype job {prototypeJobId} not found");
            }

            // Load stage executions
            var stageExecutions = await _prototypeService.GetStageExecutionsAsync(prototypeJobId);
            ViewModel.StageExecutions = stageExecutions.Select(se => new StageExecutionDetail
            {
                Execution = se,
                StageName = se.ProductionStage?.Name ?? "Unknown Stage",
                StageOrder = se.ProductionStage?.DisplayOrder ?? 0,
                StatusIcon = GetStatusIcon(se.Status),
                StatusColor = GetStatusColor(se.Status),
                TimeVarianceStatus = GetTimeVarianceStatus(se.EstimatedHours, se.ActualHours),
                TimeVarianceHours = (se.ActualHours ?? 0) - (se.EstimatedHours ?? 0),
                CanStart = se.Status == "NotStarted" && CanStartStage(se, stageExecutions),
                CanComplete = se.Status == "InProgress",
                CanSkip = se.ProductionStage?.AllowSkip == true && se.Status == "NotStarted"
            }).OrderBy(se => se.StageOrder).ToList();

            // Load assembly components
            ViewModel.AssemblyComponents = await _componentService.GetComponentsAsync(prototypeJobId);

            // Load cost analysis
            var costAnalysis = await _prototypeService.GetCostAnalysisAsync(prototypeJobId);
            ViewModel.CostAnalysis = new CostAnalysisDetail
            {
                TotalEstimatedCost = ViewModel.PrototypeJob.TotalEstimatedCost,
                TotalActualCost = ViewModel.PrototypeJob.TotalActualCost,
                CostVariance = ViewModel.PrototypeJob.TotalActualCost - ViewModel.PrototypeJob.TotalEstimatedCost,
                CostVariancePercent = ViewModel.PrototypeJob.CostVariancePercent,
                VarianceStatus = GetCostVarianceStatus(ViewModel.PrototypeJob.CostVariancePercent),
                StageCosts = stageExecutions.Select(se => new StageCostBreakdown
                {
                    StageName = se.ProductionStage?.Name ?? "Unknown",
                    EstimatedCost = se.EstimatedCost ?? 0,
                    ActualCost = se.ActualCost ?? 0,
                    Variance = (se.ActualCost ?? 0) - (se.EstimatedCost ?? 0),
                    VariancePercent = se.EstimatedCost > 0 ? ((se.ActualCost ?? 0) - (se.EstimatedCost ?? 0)) / (se.EstimatedCost ?? 0) * 100 : 0,
                    Status = se.Status
                }).ToList()
            };

            // Load time analysis
            ViewModel.TimeAnalysis = new TimeAnalysisDetail
            {
                TotalEstimatedHours = ViewModel.PrototypeJob.TotalEstimatedHours,
                TotalActualHours = ViewModel.PrototypeJob.TotalActualHours,
                TimeVariance = ViewModel.PrototypeJob.TotalActualHours - ViewModel.PrototypeJob.TotalEstimatedHours,
                TimeVariancePercent = ViewModel.PrototypeJob.TimeVariancePercent,
                VarianceStatus = GetTimeVarianceStatus(ViewModel.PrototypeJob.TimeVariancePercent),
                StageTimes = stageExecutions.Select(se => new StageTimeBreakdown
                {
                    StageName = se.ProductionStage?.Name ?? "Unknown",
                    EstimatedHours = se.EstimatedHours ?? 0,
                    ActualHours = se.ActualHours ?? 0,
                    Variance = (se.ActualHours ?? 0) - (se.EstimatedHours ?? 0),
                    VariancePercent = se.EstimatedHours > 0 ? ((se.ActualHours ?? 0) - (se.EstimatedHours ?? 0)) / (se.EstimatedHours ?? 0) * 100 : 0,
                    Status = se.Status
                }).ToList()
            };

            // Set flags for current state
            ViewModel.CurrentStageExecution = stageExecutions.FirstOrDefault(se => se.Status == "InProgress");
            ViewModel.CanStartNextStage = stageExecutions.Any(se => se.Status == "NotStarted");
            ViewModel.CanCompleteCurrentStage = ViewModel.CurrentStageExecution != null;
            ViewModel.IsReadyForAdminReview = stageExecutions.All(se => se.Status == "Completed" || se.Status == "Skipped");
        }

        private string GetStatusIcon(string status)
        {
            return status switch
            {
                "Completed" => "?",
                "InProgress" => "??",
                "NotStarted" => "?",
                "Skipped" => "??",
                "Failed" => "?",
                _ => "?"
            };
        }

        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Completed" => "success",
                "InProgress" => "primary",
                "NotStarted" => "secondary",
                "Skipped" => "warning",
                "Failed" => "danger",
                _ => "secondary"
            };
        }

        private string GetTimeVarianceStatus(decimal? estimated, decimal? actual)
        {
            if (!estimated.HasValue || !actual.HasValue) return "Unknown";
            
            var variance = actual.Value - estimated.Value;
            return variance switch
            {
                > 1 => "Behind",
                < -1 => "Ahead",
                _ => "On Schedule"
            };
        }

        private string GetTimeVarianceStatus(decimal variancePercent)
        {
            return variancePercent switch
            {
                > 10 => "Behind",
                < -10 => "Ahead",
                _ => "On Schedule"
            };
        }

        private string GetCostVarianceStatus(decimal variancePercent)
        {
            return variancePercent switch
            {
                > 10 => "Over Budget",
                < -10 => "Under Budget",
                _ => "On Budget"
            };
        }

        private bool CanStartStage(ProductionStageExecution stageExecution, List<ProductionStageExecution> allExecutions)
        {
            // Check if all previous stages are completed or skipped
            var previousStages = allExecutions
                .Where(se => (se.ProductionStage?.DisplayOrder ?? 0) < (stageExecution.ProductionStage?.DisplayOrder ?? 0))
                .ToList();

            return previousStages.All(se => se.Status == "Completed" || se.Status == "Skipped");
        }
    }
}