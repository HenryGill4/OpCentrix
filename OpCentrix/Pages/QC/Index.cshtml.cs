using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Authorization;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;

namespace OpCentrix.Pages.QC
{
    [QCAccess]
    public class IndexModel : PageModel
    {
        private readonly IQCInspectionService _qcService;
        private readonly SchedulerContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IQCInspectionService qcService,
            SchedulerContext context,
            ILogger<IndexModel> logger)
        {
            _qcService = qcService;
            _context = context;
            _logger = logger;
        }

        // Data
        public List<QCInspection> Inspections { get; set; } = new();
        public List<QCInspection> PendingInspections { get; set; } = new();
        public QCStatistics Statistics { get; set; } = new();
        public QCInspection? SelectedInspection { get; set; }
        public List<Part> AvailableParts { get; set; } = new();

        // Filters
        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TypeFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedInspectionId { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Inspections = await _qcService.GetInspectionsAsync(StatusFilter, TypeFilter);
                PendingInspections = await _qcService.GetPendingInspectionsAsync();
                Statistics = await _qcService.GetStatisticsAsync(DateTime.UtcNow.AddDays(-30));

                if (SelectedInspectionId.HasValue)
                {
                    SelectedInspection = await _qcService.GetInspectionAsync(SelectedInspectionId.Value);
                }

                AvailableParts = await _context.Parts
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.PartNumber)
                    .ToListAsync();

                _logger.LogInformation("QC page loaded with {Count} inspections", Inspections.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading QC page");
                Inspections = new List<QCInspection>();
                PendingInspections = new List<QCInspection>();
            }
        }

        public async Task<IActionResult> OnPostStartInspectionAsync(int inspectionId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
                var userId = user?.Id ?? 1;
                var userName = user?.FullName ?? User.Identity?.Name ?? "Inspector";

                var success = await _qcService.StartInspectionAsync(inspectionId, userId, userName);
                
                if (success)
                    TempData["Success"] = "Inspection started";
                else
                    TempData["Error"] = "Failed to start inspection";
                
                return RedirectToPage(new { selectedInspectionId = inspectionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting inspection {InspectionId}", inspectionId);
                TempData["Error"] = "Error starting inspection";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostRecordResultAsync(int inspectionId, int passed, int failed, int rework, int scrapped, string? notes)
        {
            try
            {
                await _qcService.RecordResultAsync(inspectionId, passed, failed, rework, scrapped, notes);
                TempData["Success"] = "Results recorded";
                return RedirectToPage(new { selectedInspectionId = inspectionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording results for inspection {InspectionId}", inspectionId);
                TempData["Error"] = "Error recording results";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostUpdateChecklistItemAsync(int itemId, string result, string? actualValue, string? notes)
        {
            try
            {
                await _qcService.UpdateChecklistItemResultAsync(itemId, result, actualValue, notes);
                
                // Get the inspection ID for redirect
                var item = await _context.QCChecklistItems.FindAsync(itemId);
                return RedirectToPage(new { selectedInspectionId = item?.QCInspectionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating checklist item {ItemId}", itemId);
                TempData["Error"] = "Error updating checklist item";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostCompleteInspectionAsync(int inspectionId)
        {
            try
            {
                var userName = User.Identity?.Name ?? "Inspector";
                var success = await _qcService.CompleteInspectionAsync(inspectionId, userName);
                
                if (success)
                    TempData["Success"] = "Inspection completed";
                else
                    TempData["Error"] = "Failed to complete inspection";
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing inspection {InspectionId}", inspectionId);
                TempData["Error"] = "Error completing inspection";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostCreateInspectionAsync(int partId, string inspectionType, int quantity)
        {
            try
            {
                var userName = User.Identity?.Name ?? "System";
                var inspection = new QCInspection
                {
                    PartId = partId,
                    InspectionType = inspectionType,
                    TotalQuantity = quantity
                };

                var created = await _qcService.CreateInspectionAsync(inspection, userName);
                TempData["Success"] = $"Inspection {created.InspectionNumber} created";
                
                return RedirectToPage(new { selectedInspectionId = created.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inspection");
                TempData["Error"] = "Error creating inspection";
                return RedirectToPage();
            }
        }

        // Helper methods
        public string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "Pending" => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
                "InProgress" => "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200",
                "Passed" => "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200",
                "Failed" => "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200",
                "ConditionalPass" => "bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200",
                _ => "bg-gray-100 text-gray-800"
            };
        }

        public string GetResultBadgeClass(string result)
        {
            return result switch
            {
                "Pass" => "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200",
                "Fail" => "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200",
                "NA" => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
                _ => "bg-gray-100 text-gray-800"
            };
        }

        public string GetInspectionTypeIcon(string type)
        {
            return type switch
            {
                "PostPrint" => "fa-print",
                "PostCNC" => "fa-gears",
                "PostEDM" => "fa-bolt",
                "PostCoating" => "fa-paintbrush",
                "Final" => "fa-check-double",
                _ => "fa-clipboard-check"
            };
        }
    }
}
