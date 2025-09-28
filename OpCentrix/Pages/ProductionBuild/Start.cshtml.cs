using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using OpCentrix.Services;
using OpCentrix.ViewModels.PrintTracking;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.ProductionBuild
{
    [Authorize]
    public class StartModel : PageModel
    {
        private readonly IProductionBuildService _productionBuildService;
        private readonly ILogger<StartModel> _logger;

        public StartModel(IProductionBuildService productionBuildService, ILogger<StartModel> logger)
        {
            _productionBuildService = productionBuildService;
            _logger = logger;
        }

        [BindProperty]
        [Required(ErrorMessage = "Please select a master part")]
        [Display(Name = "Master Part")]
        public int MasterPartId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please select a printer")]
        [Display(Name = "Printer")]
        public string PrinterName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Build quantity is required")]
        [Range(1, 500, ErrorMessage = "Build quantity must be between 1 and 500")]
        [Display(Name = "Build Quantity")]
        public int BuildQuantity { get; set; } = 1;

        [BindProperty]
        [Range(1, 3, ErrorMessage = "Stack level must be between 1 and 3")]
        [Display(Name = "Stack Level")]
        public int StackLevel { get; set; } = 1;

        [BindProperty]
        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Actual Start Time")]
        public DateTime ActualStartTime { get; set; } = DateTime.Now;

        [BindProperty]
        [Display(Name = "Added Powder")]
        public bool AddedPowder { get; set; }

        [BindProperty]
        [Range(0.1, 50.0, ErrorMessage = "Powder amount must be between 0.1 and 50.0 kg")]
        [Display(Name = "Powder Amount (kg)")]
        public decimal? PowderAmountKg { get; set; }

        [BindProperty]
        [StringLength(1000, ErrorMessage = "Setup notes cannot exceed 1000 characters")]
        [Display(Name = "Setup Notes")]
        public string? SetupNotes { get; set; }

        // Display properties
        public List<MasterPartInfo> AvailableMasterParts { get; set; } = new();
        public List<string> AvailablePrinters { get; set; } = new();
        public string MaterialBatch { get; set; } = "TI64-G5-LOT-001";
        public string PowderLot { get; set; } = "PWD-2025-001";
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadPageDataAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Production Build start page");
                ErrorMessage = "Error loading page data. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Validate powder amount if powder was added
                if (AddedPowder && !PowderAmountKg.HasValue)
                {
                    ModelState.AddModelError(nameof(PowderAmountKg), "Powder amount is required when 'Added powder today' is checked");
                }

                if (!ModelState.IsValid)
                {
                    await LoadPageDataAsync();
                    return Page();
                }

                // Get current user ID (simplified for demo)
                var userId = 1; // In a real app, get from claims

                // Create production build start data
                var startData = new ProductionBuildStartData
                {
                    MasterPartId = MasterPartId,
                    PrinterName = PrinterName,
                    BuildQuantity = BuildQuantity,
                    StackLevel = StackLevel,
                    MaterialBatch = MaterialBatch,
                    PowderLot = PowderLot,
                    AddedPowder = AddedPowder,
                    PowderAmountKg = PowderAmountKg,
                    ActualStartTime = ActualStartTime,
                    SetupNotes = SetupNotes
                };

                var productionBuildId = await _productionBuildService.StartProductionBuildAsync(startData, userId);

                if (productionBuildId > 0)
                {
                    _logger.LogInformation("Successfully started production build {ProductionBuildId} for master part {MasterPartId} on printer {PrinterName}",
                        productionBuildId, MasterPartId, PrinterName);

                    TempData["SuccessMessage"] = $"Production build started successfully! Build ID: {productionBuildId}";
                    return RedirectToPage("/ProductionBuild/Dashboard");
                }
                else
                {
                    ErrorMessage = "Failed to start production build. Please check your settings and try again.";
                    await LoadPageDataAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting production build");
                ErrorMessage = $"Error starting production build: {ex.Message}";
                await LoadPageDataAsync();
                return Page();
            }
        }

        private async Task LoadPageDataAsync()
        {
            try
            {
                // Load available master parts and convert to our local type
                var masterParts = await _productionBuildService.GetAvailableMasterPartsAsync();
                AvailableMasterParts = masterParts.Select(mp => new MasterPartInfo
                {
                    Id = mp.Id,
                    PartNumber = mp.PartNumber,
                    Name = mp.Name,
                    Material = mp.Material,
                    AllowStacking = mp.AllowStacking,
                    MaxStackCount = mp.MaxStackCount,
                    EstimatedHours = mp.EstimatedHours,
                    RequiredStages = mp.RequiredStages
                }).ToList();

                // Load available printers (SLS machines)
                AvailablePrinters = new List<string> { "TI1", "TI2", "INC", "FARTU Ti 1", "FARTU Ti 2", "FARTU Ti 3" };

                // Set current material batch and powder lot (in real app, these would come from inventory system)
                MaterialBatch = "TI64-G5-LOT-001";
                PowderLot = "PWD-2025-001";

                _logger.LogDebug("Loaded {MasterPartCount} master parts and {PrinterCount} printers for Production Build start page",
                    AvailableMasterParts.Count, AvailablePrinters.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading page data for Production Build start");
                throw;
            }
        }
    }

    // Helper class for master part display
    public class MasterPartInfo
    {
        public int Id { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string ManufacturingApproach { get; set; } = string.Empty;
        public bool AllowStacking { get; set; }
        public int MaxStackCount { get; set; }
        public double EstimatedHours { get; set; }
        public string RequiredStages { get; set; } = string.Empty;
        public string DisplayText => $"{PartNumber} - {Name} ({ManufacturingApproach})";
    }
}