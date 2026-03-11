using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Authorization;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;

namespace OpCentrix.Pages.Analytics
{
    [SchedulerAccess]
    public class DashboardModel : PageModel
    {
        private readonly IAnalyticsDashboardService _analyticsService;
        private readonly SchedulerContext _context;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            IAnalyticsDashboardService analyticsService,
            SchedulerContext context,
            ILogger<DashboardModel> logger)
        {
            _analyticsService = analyticsService;
            _context = context;
            _logger = logger;
        }

        // Data
        public ManufacturingMetrics Metrics { get; set; } = new();
        public MachineUtilizationReport UtilizationReport { get; set; } = new();
        public List<BuildPerformanceTrend> BuildTrends { get; set; } = new();
        public PartProductionSummary ProductionSummary { get; set; } = new();
        public List<StagePerformanceMetric> StagePerformance { get; set; } = new();
        public List<OEEMetrics> MachineOEE { get; set; } = new();
        public List<Machine> Machines { get; set; } = new();

        // Filters
        [BindProperty(SupportsGet = true)]
        public int Days { get; set; } = 30;

        [BindProperty(SupportsGet = true)]
        public string? MachineFilter { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-Days);
                var toDate = DateTime.UtcNow;

                Machines = await _context.Machines
                    .Where(m => m.IsActive && (m.MachineType == "SLS" || m.MachineType == "Printer"))
                    .OrderBy(m => m.Name)
                    .ToListAsync();

                Metrics = await _analyticsService.GetManufacturingMetricsAsync(fromDate, toDate);
                UtilizationReport = await _analyticsService.GetMachineUtilizationAsync(MachineFilter, fromDate, toDate);
                BuildTrends = await _analyticsService.GetBuildPerformanceTrendsAsync(Days);
                ProductionSummary = await _analyticsService.GetPartProductionSummaryAsync();
                StagePerformance = await _analyticsService.GetStagePerformanceAsync();

                // Calculate OEE for each machine
                MachineOEE = new List<OEEMetrics>();
                foreach (var machine in Machines)
                {
                    var oee = await _analyticsService.CalculateOEEAsync(machine.MachineId, fromDate, toDate);
                    MachineOEE.Add(oee);
                }

                _logger.LogInformation("Analytics dashboard loaded for {Days} days", Days);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analytics dashboard");
            }
        }

        public string GetOEEColor(double oee)
        {
            return oee switch
            {
                >= 85 => "text-green-600",
                >= 70 => "text-yellow-600",
                >= 55 => "text-orange-600",
                _ => "text-red-600"
            };
        }

        public string GetOEEBgColor(double oee)
        {
            return oee switch
            {
                >= 85 => "bg-green-500",
                >= 70 => "bg-yellow-500",
                >= 55 => "bg-orange-500",
                _ => "bg-red-500"
            };
        }

        public string GetUtilizationColor(double util)
        {
            return util switch
            {
                >= 80 => "bg-green-500",
                >= 60 => "bg-blue-500",
                >= 40 => "bg-yellow-500",
                _ => "bg-red-500"
            };
        }
    }
}
