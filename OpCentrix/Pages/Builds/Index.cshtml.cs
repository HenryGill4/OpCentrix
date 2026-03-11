using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Authorization;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;

namespace OpCentrix.Pages.Builds
{
    [SchedulerAccess]
    public class IndexModel : PageModel
    {
        private readonly IBuildPlanningService _buildPlanningService;
        private readonly SchedulerContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IBuildPlanningService buildPlanningService,
            SchedulerContext context,
            ILogger<IndexModel> logger)
        {
            _buildPlanningService = buildPlanningService;
            _context = context;
            _logger = logger;
        }

        // Data
        public List<BuildPackage> BuildPackages { get; set; } = new();
        public List<Part> AvailableParts { get; set; } = new();
        public List<Machine> AvailableMachines { get; set; } = new();
        public BuildPackageStatistics Statistics { get; set; } = new();

        // Filters
        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        // Form data
        [BindProperty]
        public BuildPackage NewPackage { get; set; } = new();

        [BindProperty]
        public int SelectedPartId { get; set; }

        [BindProperty]
        public int PartQuantity { get; set; } = 1;

        public async Task OnGetAsync()
        {
            try
            {
                BuildPackages = await _buildPlanningService.GetBuildPackagesAsync(StatusFilter);
                
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    var search = SearchTerm.ToLower();
                    BuildPackages = BuildPackages.Where(p => 
                        p.PackageNumber.ToLower().Contains(search) ||
                        (p.Name?.ToLower().Contains(search) ?? false) ||
                        p.Material.ToLower().Contains(search)
                    ).ToList();
                }

                AvailableParts = await _context.Parts
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.PartNumber)
                    .ToListAsync();

                AvailableMachines = await _context.Machines
                    .Where(m => m.IsActive && (m.MachineType == "SLS" || m.MachineType == "Printer"))
                    .OrderBy(m => m.Name)
                    .ToListAsync();

                Statistics = await _buildPlanningService.GetStatisticsAsync();

                _logger.LogInformation("Build Planning page loaded with {Count} packages", BuildPackages.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Build Planning page");
                BuildPackages = new List<BuildPackage>();
            }
        }

        public async Task<IActionResult> OnPostCreatePackageAsync()
        {
            try
            {
                var userName = User.Identity?.Name ?? "System";
                NewPackage.CreatedBy = userName;
                
                var created = await _buildPlanningService.CreateBuildPackageAsync(NewPackage, userName);
                TempData["Success"] = $"Build package {created.PackageNumber} created successfully";
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating build package");
                TempData["Error"] = "Failed to create build package";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostAddPartAsync(int packageId)
        {
            try
            {
                await _buildPlanningService.AddPartToPackageAsync(packageId, SelectedPartId, PartQuantity);
                TempData["Success"] = "Part added to build package";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding part to package {PackageId}", packageId);
                TempData["Error"] = ex.Message;
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostRemovePartAsync(int packagePartId)
        {
            try
            {
                await _buildPlanningService.RemovePartFromPackageAsync(packagePartId);
                TempData["Success"] = "Part removed from build package";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing part {PackagePartId}", packagePartId);
                TempData["Error"] = "Failed to remove part";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostAssignBuildFileAsync(int packageId, string fileName, string filePath)
        {
            try
            {
                await _buildPlanningService.AssignBuildFileAsync(packageId, fileName, filePath);
                TempData["Success"] = "Build file assigned successfully";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning build file to package {PackageId}", packageId);
                TempData["Error"] = "Failed to assign build file";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostMarkReadyAsync(int packageId)
        {
            try
            {
                var userName = User.Identity?.Name ?? "System";
                var success = await _buildPlanningService.MarkAsReadyAsync(packageId, userName);
                
                if (success)
                    TempData["Success"] = "Build package marked as ready for scheduling";
                else
                    TempData["Warning"] = "Cannot mark as ready - ensure parts and build file are assigned";
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking package {PackageId} as ready", packageId);
                TempData["Error"] = "Failed to update package status";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostScheduleBuildAsync(int packageId, string machineId, DateTime startDate)
        {
            try
            {
                var userName = User.Identity?.Name ?? "System";
                var jobId = await _buildPlanningService.ScheduleBuildAsync(packageId, machineId, startDate, userName);
                
                if (jobId.HasValue)
                    TempData["Success"] = $"Build package scheduled as Job #{jobId.Value}";
                else
                    TempData["Error"] = "Failed to schedule build - ensure package is ready";
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling package {PackageId}", packageId);
                TempData["Error"] = "Failed to schedule build";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeletePackageAsync(int packageId)
        {
            try
            {
                var success = await _buildPlanningService.DeleteBuildPackageAsync(packageId);
                
                if (success)
                    TempData["Success"] = "Build package deleted";
                else
                    TempData["Warning"] = "Cannot delete in-progress or completed packages";
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting package {PackageId}", packageId);
                TempData["Error"] = "Failed to delete build package";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetPackageDetailsAsync(int id)
        {
            var package = await _buildPlanningService.GetBuildPackageAsync(id);
            if (package == null)
                return NotFound();
            
            return Partial("_PackageDetailsPartial", package);
        }

        // Helper methods
        public string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "Draft" => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
                "Ready" => "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200",
                "Scheduled" => "bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200",
                "InProgress" => "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200",
                "Completed" => "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200",
                "Cancelled" => "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200",
                _ => "bg-gray-100 text-gray-800"
            };
        }

        public string GetPriorityBadgeClass(int priority)
        {
            return priority switch
            {
                1 => "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200",
                2 => "bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200",
                3 => "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200",
                4 => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
                5 => "bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-400",
                _ => "bg-gray-100 text-gray-800"
            };
        }
    }
}
