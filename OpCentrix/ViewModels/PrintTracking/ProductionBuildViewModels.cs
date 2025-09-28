using System.ComponentModel.DataAnnotations;

namespace OpCentrix.ViewModels.PrintTracking
{
    /// <summary>
    /// Streamlined print start view model focused on operator inputs only
    /// All other data auto-populated from master part and machine configuration
    /// </summary>
    public class StreamlinedPrintStartViewModel
    {
        #region Core Selection
        
        [Required(ErrorMessage = "Master part selection is required")]
        [Display(Name = "Master Part")]
        public int MasterPartId { get; set; }
        
        [Required(ErrorMessage = "Printer selection is required")]
        [Display(Name = "Printer")]
        public string PrinterName { get; set; } = string.Empty;
        
        #endregion
        
        #region Operator-Determined Batch Configuration
        
        [Required(ErrorMessage = "Build quantity is required")]
        [Range(1, 500, ErrorMessage = "Build quantity must be between 1 and 500")]
        [Display(Name = "Build Quantity")]
        public int BuildQuantity { get; set; } = 1;
        
        [Range(1, 5, ErrorMessage = "Stack level must be between 1 and 5")]
        [Display(Name = "Stack Level")]
        public int StackLevel { get; set; } = 1;
        
        #endregion
        
        #region Time Management
        
        [Required]
        [Display(Name = "Actual Start Time")]
        public DateTime ActualStartTime { get; set; } = DateTime.Now;
        
        [Display(Name = "Scheduled Start Time")]
        public DateTime? ScheduledStartTime { get; set; }
        
        #endregion
        
        #region Powder Management (Operator Input Only)
        
        [Display(Name = "Added Powder Today")]
        public bool AddedPowder { get; set; }
        
        [Range(0.1, 50.0, ErrorMessage = "Powder amount must be between 0.1 and 50.0 kg")]
        [Display(Name = "Powder Amount (kg)")]
        public decimal? PowderAmountKg { get; set; }
        
        #endregion
        
        #region Optional Notes
        
        [StringLength(1000, ErrorMessage = "Setup notes cannot exceed 1000 characters")]
        [Display(Name = "Setup Notes")]
        public string? SetupNotes { get; set; }
        
        #endregion
        
        #region Auto-Populated Display Data (Read-Only)
        
        [Display(Name = "Part Number")]
        public string PartNumber { get; set; } = string.Empty;
        
        [Display(Name = "Part Name")]
        public string PartName { get; set; } = string.Empty;
        
        [Display(Name = "Material")]
        public string Material { get; set; } = string.Empty;
        
        [Display(Name = "Material Batch")]
        public string MaterialBatch { get; set; } = string.Empty;
        
        [Display(Name = "Powder Lot")]
        public string PowderLot { get; set; } = string.Empty;
        
        [Display(Name = "Required Stages")]
        public List<string> RequiredStages { get; set; } = new();
        
        [Display(Name = "Estimated Duration")]
        public double EstimatedHours { get; set; }
        
        [Display(Name = "Build Number")]
        public string BuildNumber { get; set; } = string.Empty;
        
        #endregion
        
        #region Stacking Information
        
        public bool AllowStacking { get; set; }
        public double? SingleStackDuration { get; set; }
        public double? DoubleStackDuration { get; set; }
        public double? TripleStackDuration { get; set; }
        public int MaxStackCount { get; set; } = 1;
        
        #endregion
        
        #region Available Selections (Populated by Controller)
        
        public List<MasterPartOption> AvailableMasterParts { get; set; } = new();
        public List<string> AvailablePrinters { get; set; } = new();
        
        #endregion
        
        #region Validation and Helper Properties
        
        [Display(Name = "Effective Duration")]
        public double EffectiveDurationHours
        {
            get
            {
                if (!AllowStacking) return EstimatedHours;
                
                return StackLevel switch
                {
                    1 => SingleStackDuration ?? EstimatedHours,
                    2 => DoubleStackDuration ?? EstimatedHours * 1.5,
                    3 => TripleStackDuration ?? EstimatedHours * 2.0,
                    _ => EstimatedHours * StackLevel
                };
            }
        }
        
        [Display(Name = "Estimated End Time")]
        public DateTime EstimatedEndTime => ActualStartTime.AddHours(EffectiveDurationHours);
        
        public bool IsDelayed => ScheduledStartTime.HasValue && ActualStartTime > ScheduledStartTime.Value;
        
        public int DelayMinutes => IsDelayed 
            ? (int)(ActualStartTime - ScheduledStartTime!.Value).TotalMinutes 
            : 0;
        
        #endregion
    }
    
    /// <summary>
    /// Master part option for dropdown selection
    /// </summary>
    public class MasterPartOption
    {
        public int Id { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string RequiredStages { get; set; } = string.Empty;
        public bool AllowStacking { get; set; }
        public int MaxStackCount { get; set; } = 1;
        public double EstimatedHours { get; set; }
        
        public string DisplayText => $"{PartNumber} - {Name} ({RequiredStages})";
    }
    
    /// <summary>
    /// Production build dashboard view model
    /// Replaces the old print tracking dashboard with production build focus
    /// </summary>
    public class ProductionBuildDashboardViewModel
    {
        // Active production builds (in progress)
        public List<ProductionBuildSummary> ActiveBuilds { get; set; } = new();
        
        // Recent completed builds
        public List<ProductionBuildSummary> RecentCompletedBuilds { get; set; } = new();
        
        // Stage executions in progress
        public List<StageExecutionSummary> ActiveStageExecutions { get; set; } = new();
        
        // Parts waiting for next stage
        public List<PartBatchSummary> PendingBatches { get; set; } = new();
        
        // Quality alerts
        public List<QualityAlert> QualityAlerts { get; set; } = new();
        
        // Performance metrics
        public Dictionary<string, int> BuildsByPrinter { get; set; } = new();
        public Dictionary<string, int> PartsByStage { get; set; } = new();
        public Dictionary<string, double> UtilizationByMachine { get; set; } = new();
        
        // Analytics
        public int TotalPartsProducedToday { get; set; }
        public double OverallEfficiency { get; set; }
        public double AverageYield { get; set; }
        public decimal TotalCostToday { get; set; }
        
        // User info
        public string OperatorName { get; set; } = "Unknown";
        public int UserId { get; set; }
        public string UserRole { get; set; } = "User";
        public List<string> UserPermissions { get; set; } = new();
        
        // Dashboard config
        public DateTime RefreshTime { get; set; } = DateTime.Now;
        public int RefreshIntervalSeconds { get; set; } = 30;
        
        // DASHBOARD STATS PROPERTIES (for the cards at the top)
        public int ActiveBuildsCount => ActiveBuilds.Count;
        public int CompletedToday => RecentCompletedBuilds.Count;
        public int ActiveStages => ActiveStageExecutions.Count;
        public int TotalPartsInProgress => ActiveBuilds.Sum(b => b.BuildQuantity);
        
        // COMPATIBILITY: Dashboard stats properties for dashboard view compatibility
        public List<ProductionBuildSummary> ActiveProductionBuilds => ActiveBuilds;
    }
    
    public class ProductionBuildSummary
    {
        public int Id { get; set; }
        public string BuildNumber { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string PrinterName { get; set; } = string.Empty;
        public int BuildQuantity { get; set; }
        public int StackLevel { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public double ActualDurationHours { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public int CompletedStages { get; set; }
        public int TotalStages { get; set; }
        public double ProgressPercentage { get; set; }
        
        // NEW: Compatibility properties for dashboard view
        public string MasterPartNumber => PartNumber;
        public string MasterPartName => PartName;
        public string CurrentStage => Status == "InProgress" ? "SLS" : "";
        public double OverallProgress => ProgressPercentage;
        public DateTime? CompletedDate => ActualEndTime;
    }
    
    /// <summary>
    /// Data model for starting a new production build
    /// </summary>
    public class ProductionBuildStartData
    {
        public int MasterPartId { get; set; }
        public string PrinterName { get; set; } = string.Empty;
        public int BuildQuantity { get; set; }
        public int StackLevel { get; set; } = 1;
        public string MaterialBatch { get; set; } = string.Empty;
        public string? PowderLot { get; set; }
        public bool AddedPowder { get; set; }
        public decimal? PowderAmountKg { get; set; }
        public DateTime ActualStartTime { get; set; }
        public string? SetupNotes { get; set; }
    }
    
    public class StageExecutionSummary
    {
        public int Id { get; set; }
        public string BuildNumber { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string StageName { get; set; } = string.Empty;
        public string MachineUsed { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EstimatedEndTime { get; set; }
        public int QuantityIn { get; set; }
        public int QuantityOut { get; set; }
        public double YieldPercentage { get; set; }
        public double ProgressPercentage { get; set; }
    }
    
    public class PartBatchSummary
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string CurrentStage { get; set; } = string.Empty;
        public string NextStage { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string QualityStatus { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public bool CanStartNextStage { get; set; }
    }
    
    public class QualityAlert
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // "DefectRate", "Yield", "Rework"
        public string Severity { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string StageName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsAcknowledged { get; set; }
    }
}