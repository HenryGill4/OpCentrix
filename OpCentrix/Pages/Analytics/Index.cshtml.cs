using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Authorization;
using OpCentrix.Data;
using OpCentrix.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpCentrix.Pages.Analytics
{
    [AnalyticsAccess]
    public class IndexModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(SchedulerContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Production metrics
        public int TotalJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int DelayedJobs { get; set; }

        // Department metrics - Temporarily using placeholder values
        public int TotalCoatingOps { get; set; }
        public int TotalEDMOps { get; set; }
        public int TotalMachiningOps { get; set; }
        public int TotalQualityInspections { get; set; }
        public int TotalShipments { get; set; }

        // Quality metrics - Simplified without department operations
        public double OverallQualityRate { get; set; }
        public int QualityFailures { get; set; }
        public int QualityHolds { get; set; }

        // Performance metrics
        public double AverageJobDuration { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public double MachineUtilization { get; set; }

        // Cost metrics
        public decimal TotalProductionCost { get; set; }
        public decimal AverageCostPerJob { get; set; }

        // Recent activity
        public List<Job> RecentJobs { get; set; } = new();
        public List<object> RecentInspections { get; set; } = new(); // Placeholder
        public List<object> RecentShipments { get; set; } = new(); // Placeholder

        // Chart data for frontend
        public Dictionary<string, object> ChartData { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                await LoadProductionMetricsAsync();
                await LoadDepartmentMetricsAsync();
                await LoadQualityMetricsAsync();
                await LoadPerformanceMetricsAsync();
                await LoadCostMetricsAsync();
                await LoadRecentActivityAsync();
                await LoadChartDataAsync();

                _logger.LogInformation("Analytics dashboard loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analytics dashboard");
                // Initialize with safe defaults to prevent page crashes
                await InitializeWithDefaults();
            }
        }

        private async Task LoadProductionMetricsAsync()
        {
            TotalJobs = await _context.Jobs.CountAsync();
            CompletedJobs = await _context.Jobs.CountAsync(j => j.Status == "Completed");
            ActiveJobs = await _context.Jobs.CountAsync(j => j.Status == "Active" || j.Status == "Building");
            
            // Calculate delayed jobs (started late or taking longer than estimated)
            DelayedJobs = await _context.Jobs.CountAsync(j => 
                j.ActualStart.HasValue && j.ScheduledStart < j.ActualStart);
        }

        private async Task LoadDepartmentMetricsAsync()
        {
            // Placeholder values since department operations tables are not yet implemented
            TotalCoatingOps = 0;
            TotalEDMOps = 0;
            TotalMachiningOps = 0;
            TotalQualityInspections = 0;
            TotalShipments = 0;
        }

        private async Task LoadQualityMetricsAsync()
        {
            // Simplified quality metrics based on job completion rates
            var totalCompletedJobs = await _context.Jobs.CountAsync(j => j.Status == "Completed");
            var defectiveJobs = await _context.Jobs.CountAsync(j => j.DefectQuantity > 0);
            
            OverallQualityRate = totalCompletedJobs > 0 ? 
                (double)(totalCompletedJobs - defectiveJobs) / totalCompletedJobs * 100 : 100;
            
            QualityFailures = defectiveJobs;
            QualityHolds = await _context.Jobs.CountAsync(j => j.Status == "On Hold");
        }

        private async Task LoadPerformanceMetricsAsync()
        {
            // Calculate average job duration (simplified calculation)
            var completedJobsWithDuration = await _context.Jobs
                .Where(j => j.ActualStart.HasValue && j.ActualEnd.HasValue)
                .ToListAsync();

            AverageJobDuration = completedJobsWithDuration.Any() 
                ? completedJobsWithDuration.Average(j => (j.ActualEnd!.Value - j.ActualStart!.Value).TotalHours) 
                : 0;

            // Calculate on-time delivery rate
            var onTimeJobs = await _context.Jobs.CountAsync(j => 
                j.ActualEnd.HasValue && j.ActualEnd <= j.ScheduledEnd);
            var totalCompletedJobs = await _context.Jobs.CountAsync(j => j.ActualEnd.HasValue);
            OnTimeDeliveryRate = totalCompletedJobs > 0 ? (double)onTimeJobs / totalCompletedJobs * 100 : 100;

            // Calculate machine utilization (simplified - sum of actual hours vs available hours)
            var totalMachineHours = completedJobsWithDuration.Sum(j => (j.ActualEnd!.Value - j.ActualStart!.Value).TotalHours);

            var availableHours = DateTime.Now.Day * 24 * 3; // 3 machines, simplified
            MachineUtilization = availableHours > 0 ? totalMachineHours / availableHours * 100 : 0;
            MachineUtilization = Math.Min(MachineUtilization, 100); // Cap at 100%
        }

        private async Task LoadCostMetricsAsync()
        {
            // Sum costs from jobs only (department operation costs not available yet)
            var jobCosts = await _context.Jobs
                .Where(j => j.EstimatedLaborCost > 0)
                .SumAsync(j => j.EstimatedLaborCost + j.EstimatedMaterialCost + j.EstimatedMachineCost);

            TotalProductionCost = jobCosts;
            AverageCostPerJob = TotalJobs > 0 ? TotalProductionCost / TotalJobs : 0;
        }

        private async Task LoadRecentActivityAsync()
        {
            RecentJobs = await _context.Jobs
                .Include(j => j.Part)
                .OrderByDescending(j => j.LastModifiedDate)
                .Take(5)
                .ToListAsync();

            // Placeholder for department operations (will be implemented later)
            RecentInspections = new List<object>();
            RecentShipments = new List<object>();
        }

        private async Task LoadChartDataAsync()
        {
            // Job status distribution
            var jobStatusData = await _context.Jobs
                .GroupBy(j => j.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Daily production over last 30 days
            var thirtyDaysAgo = DateTime.Today.AddDays(-30);
            var dailyProduction = await _context.Jobs
                .Where(j => j.CreatedDate >= thirtyDaysAgo)
                .GroupBy(j => j.CreatedDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Department workload (simplified without department operations)
            var departmentWorkload = new Dictionary<string, int>
            {
                ["SLS Printing"] = TotalJobs,
                ["Coating"] = TotalCoatingOps,
                ["EDM"] = TotalEDMOps,
                ["Machining"] = TotalMachiningOps,
                ["Quality"] = TotalQualityInspections,
                ["Shipping"] = TotalShipments
            };

            ChartData = new Dictionary<string, object>
            {
                ["jobStatus"] = jobStatusData.ToDictionary(x => x.Status, x => x.Count),
                ["dailyProduction"] = dailyProduction.ToDictionary(x => x.Date.ToString("MM/dd"), x => x.Count),
                ["departmentWorkload"] = departmentWorkload
            };
        }

        private async Task InitializeWithDefaults()
        {
            // Initialize all properties with safe defaults
            TotalJobs = 0;
            CompletedJobs = 0;
            ActiveJobs = 0;
            DelayedJobs = 0;
            TotalCoatingOps = 0;
            TotalEDMOps = 0;
            TotalMachiningOps = 0;
            TotalQualityInspections = 0;
            TotalShipments = 0;
            OverallQualityRate = 100;
            QualityFailures = 0;
            QualityHolds = 0;
            AverageJobDuration = 0;
            OnTimeDeliveryRate = 100;
            MachineUtilization = 0;
            TotalProductionCost = 0;
            AverageCostPerJob = 0;
            RecentJobs = new List<Job>();
            RecentInspections = new List<object>();
            RecentShipments = new List<object>();
            ChartData = new Dictionary<string, object>();
        }
    }
}