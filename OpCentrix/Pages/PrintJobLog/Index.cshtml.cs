using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Services;
using OpCentrix.ViewModels;
using System;
using System.Threading.Tasks;

namespace OpCentrix.Pages.PrintJobLog
{
    /// <summary>
    /// Print Job Log page showing comprehensive Parts ? Jobs ? BuildJobs tracking
    /// </summary>
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IPrintJobLogService _printJobLogService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IPrintJobLogService printJobLogService, ILogger<IndexModel> logger)
        {
            _printJobLogService = printJobLogService;
            _logger = logger;
        }

        public PrintJobLogViewModel ViewModel { get; set; } = new PrintJobLogViewModel();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? MachineFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 50;

        public async Task<IActionResult> OnGetAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PRINT-LOG-PAGE-{OperationId}] Loading Print Job Log page", operationId);

            try
            {
                // Set default date range if not specified
                if (!StartDate.HasValue && !EndDate.HasValue)
                {
                    EndDate = DateTime.Today;
                    StartDate = EndDate.Value.AddDays(-30);
                }

                ViewModel = await _printJobLogService.GetPrintJobLogAsync(
                    searchTerm: SearchTerm,
                    statusFilter: StatusFilter,
                    machineFilter: MachineFilter,
                    startDate: StartDate,
                    endDate: EndDate,
                    pageNumber: PageNumber,
                    pageSize: PageSize);

                _logger.LogInformation("? [PRINT-LOG-PAGE-{OperationId}] Print Job Log loaded: {EntryCount} entries, {TotalCount} total", 
                    operationId, ViewModel.LogEntries.Count, ViewModel.TotalCount);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PRINT-LOG-PAGE-{OperationId}] Error loading Print Job Log page", operationId);
                TempData["ErrorMessage"] = "Error loading print job log. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnGetPartHistoryAsync(int partId)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PRINT-LOG-PAGE-{OperationId}] Loading part history for part {PartId}", operationId, partId);

            try
            {
                var entries = await _printJobLogService.GetLogEntriesForPartAsync(partId);
                
                _logger.LogInformation("? [PRINT-LOG-PAGE-{OperationId}] Part history loaded: {EntryCount} entries for part {PartId}", 
                    operationId, entries.Count, partId);

                return Partial("_PartHistoryModal", entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PRINT-LOG-PAGE-{OperationId}] Error loading part history for part {PartId}", operationId, partId);
                return StatusCode(500, "Error loading part history");
            }
        }

        public async Task<IActionResult> OnGetJobDetailsAsync(int jobId)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PRINT-LOG-PAGE-{OperationId}] Loading job details for job {JobId}", operationId, jobId);

            try
            {
                var entry = await _printJobLogService.GetJobLogEntryAsync(jobId);
                
                if (entry == null)
                {
                    _logger.LogWarning("?? [PRINT-LOG-PAGE-{OperationId}] Job not found: {JobId}", operationId, jobId);
                    return NotFound();
                }

                _logger.LogInformation("? [PRINT-LOG-PAGE-{OperationId}] Job details loaded for job {JobId}", operationId, jobId);

                return Partial("_JobDetailsModal", entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PRINT-LOG-PAGE-{OperationId}] Error loading job details for job {JobId}", operationId, jobId);
                return StatusCode(500, "Error loading job details");
            }
        }

        public async Task<IActionResult> OnGetExportDataAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [PRINT-LOG-PAGE-{OperationId}] Exporting print job log data", operationId);

            try
            {
                var allData = await _printJobLogService.GetPrintJobLogAsync(
                    searchTerm: SearchTerm,
                    statusFilter: StatusFilter,
                    machineFilter: MachineFilter,
                    startDate: StartDate,
                    endDate: EndDate,
                    pageNumber: 1,
                    pageSize: 10000); // Large page size to get all data

                // Create CSV content
                var csv = CreateCsvExport(allData.LogEntries);
                var fileName = $"PrintJobLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                _logger.LogInformation("? [PRINT-LOG-PAGE-{OperationId}] Export created: {FileName} with {RecordCount} records", 
                    operationId, fileName, allData.LogEntries.Count);

                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [PRINT-LOG-PAGE-{OperationId}] Error exporting data", operationId);
                return StatusCode(500, "Error exporting data");
            }
        }

        private string CreateCsvExport(List<PrintJobLogEntry> entries)
        {
            var csv = new System.Text.StringBuilder();
            
            // Header
            csv.AppendLine("PartNumber,PartName,Material,SlsMaterial,ScheduledStart,ScheduledEnd,ActualStart,ActualEnd,EstimatedHours,ActualHours,EfficiencyPercentage,MachineId,Operator,JobStatus,BuildJobStatus,OverallStatus,LifecycleStage");

            // Data rows
            foreach (var entry in entries)
            {
                csv.AppendLine($@"""{entry.PartNumber}"",""{entry.PartName}"",""{entry.Material}"",""{entry.SlsMaterial}"",""{entry.ScheduledStart:yyyy-MM-dd HH:mm}"",""{entry.ScheduledEnd:yyyy-MM-dd HH:mm}"",""{entry.ActualStart:yyyy-MM-dd HH:mm}"",""{entry.ActualEnd:yyyy-MM-dd HH:mm}"",{entry.EstimatedHours},{entry.ActualHours},{entry.EfficiencyPercentage},""{entry.MachineId}"",""{entry.Operator}"",""{entry.JobStatus}"",""{entry.BuildJobStatus}"",""{entry.OverallStatus}"",""{entry.LifecycleStage}""");
            }

            return csv.ToString();
        }
    }
}