using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OpCentrix.Models;
using OpCentrix.Models.JobStaging;

namespace OpCentrix.ViewModels.PrintTracking
{
    /// <summary>
    /// View model for the Print Start Form - Enhanced for Phase 4 with operator time estimation, prototype addition, and build file tracking
    /// </summary>
    public class PrintStartViewModel
    {
        [Required(ErrorMessage = "Printer selection is required")]
        [Display(Name = "Printer")]
        public string PrinterName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Actual Start Time")]
        public DateTime ActualStartTime { get; set; } = DateTime.Now;

        [Display(Name = "Associated Scheduled Job")]
        public int? AssociatedScheduledJobId { get; set; }

        [Display(Name = "Part")]
        public int? PartId { get; set; }

        [Display(Name = "Setup Notes")]
        [StringLength(1000, ErrorMessage = "Setup notes cannot exceed 1000 characters")]
        public string? SetupNotes { get; set; }

        // PHASE 4: Enhanced Build Time Estimation
        [Required(ErrorMessage = "Estimated end time is required")]
        [Display(Name = "Estimated End Time")]
        public DateTime EstimatedEndTime { get; set; } = DateTime.Now.AddHours(4);

        [Display(Name = "Calculated Duration (hours)")]
        public decimal OperatorEstimatedHours 
        { 
            get 
            {
                if (EstimatedEndTime > ActualStartTime)
                {
                    return (decimal)(EstimatedEndTime - ActualStartTime).TotalHours;
                }
                return 0;
            }
            set
            {
                if (value > 0)
                {
                    EstimatedEndTime = ActualStartTime.AddHours((double)value);
                }
            }
        }

        [Display(Name = "System Estimated Time (hours)")]
        public decimal? SystemEstimatedHours { get; set; }

        [Display(Name = "Expected Completion Time")]
        public DateTime? ExpectedCompletionTime { get; set; }

        [Display(Name = "Build File (.build)")]
        public string? BuildFileName { get; set; }

        [Display(Name = "Build File Hash")]
        public string? BuildFileHash { get; set; }

        [Display(Name = "Repeated Build")]
        public bool IsRepeatedBuild { get; set; }

        [Display(Name = "Last Build Time (hours)")]
        public decimal? LastBuildTime { get; set; }

        [Display(Name = "Average Build Time (hours)")]
        public decimal? AverageBuildTime { get; set; }

        [Display(Name = "Times Built Before")]
        public int TimesPreviouslyBuilt { get; set; }

        // PHASE 4: Prototype Addition Interface
        [Display(Name = "Add Prototypes")]
        public bool AddPrototypes { get; set; }

        [Display(Name = "Additional Prototypes")]
        public List<PrototypeAddition> PrototypeAdditions { get; set; } = new();

        [Display(Name = "Total Parts in Build")]
        public int TotalPartsInBuild { get; set; } = 1;

        [Display(Name = "Build Plate Utilization (%)")]
        public decimal BuildPlateUtilization { get; set; }

        [Display(Name = "Maximum Parts Capacity")]
        public int MaxPartsCapacity { get; set; } = 50;

        // Multi-stage job support
        [Display(Name = "Job Stage")]
        public int? JobStageId { get; set; }

        [Display(Name = "Prototype Job")]
        public int? PrototypeJobId { get; set; }

        // Available options for dropdowns
        public List<string> AvailablePrinters { get; set; } = new() { "TI1", "TI2", "INC" };
        public List<Job> AvailableScheduledJobs { get; set; } = new();
        public List<Part> AvailableParts { get; set; } = new();
        public List<JobStage> AvailableJobStages { get; set; } = new();
        public List<PrototypeJob> AvailablePrototypeJobs { get; set; } = new();

        // For displaying current operator info
        public string OperatorName { get; set; } = string.Empty;
        public int UserId { get; set; }

        // Master schedule integration
        public DateTime? ScheduledStartTime { get; set; }
        public DateTime? ScheduledEndTime { get; set; }
        public bool IsDelayed => ScheduledStartTime.HasValue && ActualStartTime > ScheduledStartTime.Value;
        public int DelayMinutes => IsDelayed ? (int)(ActualStartTime - ScheduledStartTime!.Value).TotalMinutes : 0;

        // Job context information
        public string? PartNumber { get; set; }
        public string? PartDescription { get; set; }
        public string? Material { get; set; }
        public int? Quantity { get; set; }
        public double? EstimatedHours { get; set; }

        // Production stage information
        public string? StageName { get; set; }
        public string? StageType { get; set; }
        public string? Department { get; set; }
        public int? StageExecutionOrder { get; set; }

        // PHASE 4: Machine Performance Context
        public string? MachinePerformanceNotes { get; set; }
        public decimal? MachineEfficiencyFactor { get; set; } = 1.0m;
        public DateTime? LastMaintenanceDate { get; set; }

        // PHASE 4: Build Complexity Assessment
        [Display(Name = "Support Complexity")]
        public string SupportComplexity { get; set; } = "Medium";

        [Display(Name = "Part Orientations")]
        public string? PartOrientations { get; set; }

        [Display(Name = "Build Height (mm)")]
        [Range(1, 300, ErrorMessage = "Build height must be between 1 and 300mm")]
        public decimal? BuildHeight { get; set; }

        [Display(Name = "Layer Count")]
        [Range(10, 5000, ErrorMessage = "Layer count must be between 10 and 5000")]
        public int? LayerCount { get; set; }

        public List<string> SupportComplexityOptions { get; set; } = new() 
        { 
            "None", "Low", "Medium", "High", "Extreme" 
        };

        // PHASE 4: Time Factor Assessment
        [Display(Name = "Factors Affecting Time")]
        public List<string> TimeFactors { get; set; } = new();

        public List<string> AvailableTimeFactors { get; set; } = new()
        {
            "Complex geometry",
            "Fine details", 
            "Thick supports",
            "Multiple orientations",
            "New material",
            "Tight tolerances", 
            "First-time build",
            "Rush job",
            "Holiday/weekend",
            "Machine maintenance needed"
        };

        // Validation
        public bool CanAddMorePrototypes => TotalPartsInBuild < MaxPartsCapacity;
        public bool HasSpaceForPrototypes => BuildPlateUtilization < 90m;
        public bool IsWithinScheduleTolerance => !IsDelayed || DelayMinutes <= 15;
    }

    /// <summary>
    /// PHASE 4: Prototype addition during print start
    /// </summary>
    public class PrototypeAddition
    {
        [Required(ErrorMessage = "Part number is required")]
        [Display(Name = "Part Number")]
        public string PartNumber { get; set; } = string.Empty;

        [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50")]
        [Display(Name = "Quantity")]  
        public int Quantity { get; set; } = 1;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Requested By")]
        public string? RequestedBy { get; set; }

        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Normal";

        [Display(Name = "Fits on Build Plate")]
        public bool FitsOnBuildPlate { get; set; } = true;

        [Display(Name = "Estimated Volume (cm³)")]
        public decimal? EstimatedVolume { get; set; }

        [Display(Name = "Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        public List<string> PriorityOptions { get; set; } = new() 
        { 
            "Low", "Normal", "High", "Urgent" 
        };
    }

    /// <summary>
    /// View model for embedded scheduler view in Print Tracking
    /// </summary>
    public class EmbeddedSchedulerViewModel
    {
        public List<Job> Jobs { get; set; } = new();
        public List<string> Machines { get; set; } = new();
        public List<DateTime> Dates { get; set; } = new();
        public DateTime StartDate { get; set; } = DateTime.Today;
    }

    /// <summary>
    /// View model for the Post-Print Log Form - Enhanced for PHASE 4 with operator time logging and performance assessment
    /// </summary>
    public class PostPrintViewModel
    {
        [Required(ErrorMessage = "Build ID is required")]
        public int BuildId { get; set; }

        [Required(ErrorMessage = "Printer selection is required")]
        [Display(Name = "Printer")]
        public string PrinterName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Actual start time is required")]
        [Display(Name = "Actual Start Time")]
        public DateTime ActualStartTime { get; set; }

        [Required(ErrorMessage = "Actual end time is required")]
        [Display(Name = "Actual End Time")]
        public DateTime ActualEndTime { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Reason for end is required")]
        [Display(Name = "Reason for End")]
        public string ReasonForEnd { get; set; } = string.Empty;

        // PHASE 4: Enhanced Operator Time Logging
        [Required(ErrorMessage = "Actual build time is required")]
        [Display(Name = "Actual Build Time (hours)")]
        [Range(0.1, 72.0, ErrorMessage = "Actual hours must be between 0.1 and 72")]
        public decimal OperatorActualHours { get; set; }

        [Display(Name = "Original Estimate (hours)")]
        public decimal? OperatorEstimatedHours { get; set; }

        [Display(Name = "System Estimate (hours)")]
        public decimal? SystemEstimatedHours { get; set; }

        [Required(ErrorMessage = "Performance assessment is required")]
        [Display(Name = "How did the build perform?")]
        public string OperatorBuildAssessment { get; set; } = string.Empty;

        [Display(Name = "Machine Performance Notes")]
        [StringLength(500, ErrorMessage = "Machine performance notes cannot exceed 500 characters")]
        public string? MachinePerformanceNotes { get; set; }

        // PHASE 4: Performance Assessment Options  
        public List<string> BuildAssessmentOptions { get; set; } = new()
        {
            "Much faster than expected (25%+ time savings)",
            "Faster than expected (10-25% time savings)", 
            "As expected (within 10% of estimate)",
            "Slower than expected (10-25% longer)",
            "Much slower than expected (25%+ longer)",
            "Failed/Aborted - estimate not applicable"
        };

        // PHASE 4: Quality Assessment
        [Required(ErrorMessage = "Quality assessment is required")]
        [Display(Name = "Overall Quality")]
        public string QualityAssessment { get; set; } = string.Empty;

        [Display(Name = "Defect Count")]
        [Range(0, 1000, ErrorMessage = "Defect count must be between 0 and 1000")]
        public int? DefectCount { get; set; } = 0;

        [Display(Name = "Quality Notes")]
        [StringLength(500, ErrorMessage = "Quality notes cannot exceed 500 characters")]
        public string? QualityNotes { get; set; }

        public List<string> QualityAssessmentOptions { get; set; } = new()
        {
            "Excellent - exceeds requirements",
            "Good - meets all requirements", 
            "Acceptable - minor issues",
            "Poor - significant issues",
            "Failed - does not meet requirements"
        };

        // PHASE 4: Factors Affecting Time Assessment
        [Display(Name = "What factors affected build time?")]
        public List<string> TimeFactors { get; set; } = new();

        [Display(Name = "Lessons Learned")]
        [StringLength(1000, ErrorMessage = "Lessons learned cannot exceed 1000 characters")]
        public string? LessonsLearned { get; set; }

        public List<string> AvailableTimeFactors { get; set; } = new()
        {
            "Machine ran faster than usual",
            "Machine ran slower than usual", 
            "Supports more complex than expected",
            "Supports simpler than expected",
            "Material issues/inconsistency",
            "Power fluctuations/outages",
            "Operator interrupted build",
            "Emergency stop required",
            "Software/file issues",
            "First time building this file", 
            "Familiar/repeated build",
            "Post-processing requirements changed"
        };

        // Part information
        public int? PartId { get; set; }
        public string? PartNumber { get; set; }
        public string? PartDescription { get; set; }

        // Job/Stage context
        public int? JobId { get; set; }
        public int? JobStageId { get; set; }
        public int? PrototypeJobId { get; set; }
        public int? ProductionStageExecutionId { get; set; }

        // Part number list (repeatable group)
        [Required(ErrorMessage = "At least one part must be specified")]
        public List<PostPrintPartEntry> Parts { get; set; } = new();

        // Enhanced print tracking fields
        [Display(Name = "Laser Run Time")]
        public string? LaserRunTime { get; set; }

        [Display(Name = "Gas Used (L)")]
        [Range(0, 10000, ErrorMessage = "Gas usage must be between 0 and 10000 liters")]
        public float? GasUsed_L { get; set; }

        [Display(Name = "Powder Used (L)")]
        [Range(0, 100, ErrorMessage = "Powder usage must be between 0 and 100 liters")]
        public float? PowderUsed_L { get; set; }

        [Display(Name = "Notes")]
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        // Process parameters and quality data
        [Display(Name = "Build Temperature (°C)")]
        [Range(0, 500, ErrorMessage = "Build temperature must be between 0 and 500°C")]
        public double? BuildTemperature { get; set; }

        [Display(Name = "Oxygen Level (ppm)")]
        [Range(0, 1000, ErrorMessage = "Oxygen level must be between 0 and 1000 ppm")]
        public double? OxygenLevel { get; set; }

        [Display(Name = "Argon Purity (%)")]
        [Range(90, 100, ErrorMessage = "Argon purity must be between 90 and 100%")]
        public double? ArgonPurity { get; set; }

        [Display(Name = "Layer Thickness (?m)")]
        [Range(10, 100, ErrorMessage = "Layer thickness must be between 10 and 100 ?m")]
        public double? LayerThickness { get; set; }

        [Display(Name = "Laser Power (W)")]
        [Range(50, 500, ErrorMessage = "Laser power must be between 50 and 500 W")]
        public double? LaserPower { get; set; }

        [Display(Name = "Scan Speed (mm/s)")]
        [Range(100, 3000, ErrorMessage = "Scan speed must be between 100 and 3000 mm/s")]
        public double? ScanSpeed { get; set; }

        // Quality metrics
        [Display(Name = "Surface Roughness Ra (?m)")]
        [Range(0, 50, ErrorMessage = "Surface roughness must be between 0 and 50 ?m")]
        public double? SurfaceRoughness { get; set; }

        [Display(Name = "Density (%)")]
        [Range(90, 100, ErrorMessage = "Density must be between 90 and 100%")]
        public double? Density { get; set; }

        // Multi-stage workflow fields
        [Display(Name = "Stage Completion Status")]
        public string StageStatus { get; set; } = "Completed";

        [Display(Name = "Quality Check Required")]
        public bool RequiresQualityCheck { get; set; }

        [Display(Name = "Quality Check Passed")]
        public bool? QualityCheckPassed { get; set; }

        [Display(Name = "Next Stage Ready")]
        public bool NextStageReady { get; set; } = true;

        // Delay tracking (auto-triggered)
        public bool HasDelay { get; set; }
        public DelayInfo? DelayInfo { get; set; }

        // Available options
        public List<string> AvailablePrinters { get; set; } = new() { "TI1", "TI2", "INC" };
        public List<string> EndReasons { get; set; } = new() 
        { 
            "Completed Successfully", 
            "Completed with Issues", 
            "Aborted - Operator", 
            "Aborted - Machine Error", 
            "Aborted - Material Issue", 
            "Aborted - Power Failure", 
            "Aborted - Emergency Stop",
            "Quality Hold",
            "Rework Required"
        };
        public List<Part> AvailableParts { get; set; } = new();
        
        // ENHANCED: Available running jobs for dropdown selection
        public List<Job> AvailableRunningJobs { get; set; } = new();

        // For operator info
        public string OperatorName { get; set; } = string.Empty;
        public int UserId { get; set; }

        // Build job context
        public DateTime? ScheduledStartTime { get; set; }
        public DateTime? ScheduledEndTime { get; set; }
        public string? AssociatedJobNumber { get; set; }
        public string? CustomerOrderNumber { get; set; }

        // Cost and time tracking
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public double? EstimatedHours { get; set; }
        public double ActualHours => (ActualEndTime - ActualStartTime).TotalHours;

        // B&T compliance tracking (if applicable)
        public bool RequiresSerialNumber { get; set; }
        public string? SerialNumberAssigned { get; set; }
        public bool RequiresATFCompliance { get; set; }
        public bool RequiresITARCompliance { get; set; }
        public string? ComplianceNotes { get; set; }

        // PHASE 4: Schedule Impact Assessment
        [Display(Name = "Will this delay affect other jobs?")]
        public bool WillDelayAffectSchedule { get; set; }

        [Display(Name = "Schedule Impact Notes")]
        [StringLength(500, ErrorMessage = "Schedule impact notes cannot exceed 500 characters")]
        public string? ScheduleImpactNotes { get; set; }

        [Display(Name = "Notify Affected Departments")]
        public bool NotifyAffectedDepartments { get; set; } = true;

        // Performance calculations
        public decimal TimeVariancePercent => OperatorEstimatedHours.HasValue && OperatorEstimatedHours > 0 
            ? ((OperatorActualHours - OperatorEstimatedHours.Value) / OperatorEstimatedHours.Value) * 100 
            : 0;

        public bool IsSignificantVariance => Math.Abs(TimeVariancePercent) > 15;
        public string PerformanceColor => TimeVariancePercent switch
        {
            < -15 => "text-green-600", // Much faster
            < -5 => "text-green-500",  // Faster  
            <= 5 => "text-blue-600",   // As expected
            <= 15 => "text-yellow-600", // Slower
            _ => "text-red-600"        // Much slower
        };
    }

    /// <summary>
    /// Enhanced part entry with compliance and traceability features
    /// </summary>
    public class PostPrintPartEntry
    {
        [Required(ErrorMessage = "Part number is required")]
        [Display(Name = "Part Number")]
        public string PartNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Good Parts")]
        [Range(0, 1000, ErrorMessage = "Good parts must be between 0 and 1000")]
        public int GoodParts { get; set; } = 1;

        [Display(Name = "Defective Parts")]
        [Range(0, 1000, ErrorMessage = "Defective parts cannot exceed total quantity")]
        public int DefectiveParts { get; set; } = 0;

        [Display(Name = "Rework Parts")]
        [Range(0, 1000, ErrorMessage = "Rework parts cannot exceed total quantity")]
        public int ReworkParts { get; set; } = 0;

        [Display(Name = "Is Primary Part")]
        public bool IsPrimary { get; set; }

        // Enhanced part information
        public int? PartId { get; set; }
        public string? Description { get; set; }
        public string? Material { get; set; }
        public string? CustomerPartNumber { get; set; }

        // Build platform and location tracking
        [Display(Name = "Build Platform Position")]
        public string? BuildPlatformPosition { get; set; }

        [Display(Name = "Layer Number")]
        [Range(1, 10, ErrorMessage = "Layer number must be between 1 and 10")]
        public int? LayerNumber { get; set; }

        // Quality and compliance
        [Display(Name = "Quality Status")]
        public string QualityStatus { get; set; } = "Pending";

        [Display(Name = "Requires Inspection")]
        public bool RequiresInspection { get; set; }

        [Display(Name = "Inspection Completed")]
        public bool InspectionCompleted { get; set; }

        [Display(Name = "Inspector")]
        public string? Inspector { get; set; }

        [Display(Name = "Inspection Notes")]
        [StringLength(500, ErrorMessage = "Inspection notes cannot exceed 500 characters")]
        public string? InspectionNotes { get; set; }

        // Serial number tracking for B&T compliance
        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        [Display(Name = "Requires Serialization")]
        public bool RequiresSerialization { get; set; }

        // Cost tracking
        public decimal? MaterialCost { get; set; }
        public decimal? ProcessingCost { get; set; }
        public decimal? TotalCost { get; set; }

        // For "Add New" functionality
        public bool IsNewPart { get; set; }

        // Validation
        public bool IsValid => GoodParts + DefectiveParts + ReworkParts <= Quantity;
        public double QualityRate => Quantity > 0 ? (double)GoodParts / Quantity * 100 : 0;
        public double DefectRate => Quantity > 0 ? (double)DefectiveParts / Quantity * 100 : 0;
    }

    /// <summary>
    /// Enhanced delay information with root cause analysis
    /// </summary>
    public class DelayInfo
    {
        [Required(ErrorMessage = "Delay reason is required")]
        [Display(Name = "Delay Reason")]
        public string DelayReason { get; set; } = string.Empty;

        [Display(Name = "Root Cause Category")]
        public string RootCauseCategory { get; set; } = string.Empty;

        [Display(Name = "Delay Notes")]
        [StringLength(1000, ErrorMessage = "Delay notes cannot exceed 1000 characters")]
        public string? DelayNotes { get; set; }

        [Display(Name = "Calculated Delay (minutes)")]
        public int DelayDuration { get; set; }

        [Display(Name = "Cost Impact")]
        public decimal? CostImpact { get; set; }

        [Display(Name = "Corrective Actions")]
        [StringLength(500, ErrorMessage = "Corrective actions cannot exceed 500 characters")]
        public string? CorrectiveActions { get; set; }

        [Display(Name = "Prevention Measures")]
        [StringLength(500, ErrorMessage = "Prevention measures cannot exceed 500 characters")]
        public string? PreventionMeasures { get; set; }

        // Impact on subsequent stages
        [Display(Name = "Affects Downstream")]
        public bool AffectsDownstream { get; set; }

        [Display(Name = "Customer Impact")]
        public string CustomerImpact { get; set; } = "None";

        public List<string> AvailableReasons { get; set; } = DelayLog.DelayReasons.ToList();
        
        public List<string> RootCauseCategories { get; set; } = new()
        {
            "Machine/Equipment",
            "Material/Supply",
            "Personnel/Operator",
            "Process/Method",
            "Environment/Facility",
            "Management/Planning",
            "External/Vendor"
        };

        public List<string> CustomerImpactLevels { get; set; } = new()
        {
            "None",
            "Low - Minor Schedule Impact",
            "Medium - Delivery Date at Risk", 
            "High - Customer Notification Required",
            "Critical - Immediate Escalation Required"
        };
    }

    /// <summary>
    /// Enhanced dashboard view model with multi-stage job support and master schedule integration
    /// </summary>
    public class PrintTrackingDashboardViewModel
    {
        // Active builds and jobs
        public List<BuildJob> ActiveBuilds { get; set; } = new();
        public List<BuildJob> RecentCompletedBuilds { get; set; } = new();
        public List<DelayLog> RecentDelays { get; set; } = new();

        // Multi-stage job tracking
        public List<JobStageInfo> ActiveJobStages { get; set; } = new();
        public List<PrototypeJobInfo> ActivePrototypeJobs { get; set; } = new();
        public List<ProductionStageInfo> ProductionStages { get; set; } = new();

        // Master schedule integration
        public List<MasterScheduleJobInfo> UpcomingJobs { get; set; } = new();
        public List<MasterScheduleJobInfo> DelayedJobs { get; set; } = new();
        public List<AlertInfo> ActiveAlerts { get; set; } = new();

        // Quick stats
        public Dictionary<string, int> ActiveJobsByPrinter { get; set; } = new();
        public Dictionary<string, double> HoursToday { get; set; } = new();
        public Dictionary<string, int> JobsByStage { get; set; } = new();
        public Dictionary<string, double> UtilizationByMachine { get; set; } = new();

        // Performance metrics
        public int TotalDelaysToday { get; set; }
        public double AverageDelayMinutes { get; set; }
        public double OverallEfficiency { get; set; }
        public double QualityScore { get; set; }
        public decimal TotalCostToday { get; set; }
        public int PartsProducedToday { get; set; }

        // Capacity and resource planning
        public Dictionary<string, double> CapacityUtilization { get; set; } = new();
        public Dictionary<string, int> QueueDepth { get; set; } = new();
        public List<MaintenanceAlert> MaintenanceAlerts { get; set; } = new();

        // Current user info
        public string OperatorName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserRole { get; set; } = string.Empty;
        public List<string> UserPermissions { get; set; } = new();

        // Dashboard configuration
        public DateTime RefreshTime { get; set; } = DateTime.Now;
        public int RefreshIntervalSeconds { get; set; } = 30;
        public bool ShowAlerts { get; set; } = true;
        public bool ShowMetrics { get; set; } = true;
        public bool ShowQueues { get; set; } = true;
    }

    /// <summary>
    /// Job stage information for dashboard display
    /// </summary>
    public class JobStageInfo
    {
        public int JobStageId { get; set; }
        public int JobId { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string StageType { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public DateTime? ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }
        public DateTime? ActualStart { get; set; }
        public double ProgressPercent { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string AssignedOperator { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool CanStart { get; set; }
        public List<string> BlockingDependencies { get; set; } = new();
    }

    /// <summary>
    /// Prototype job information for dashboard display
    /// </summary>
    public class PrototypeJobInfo
    {
        public int PrototypeJobId { get; set; }
        public string PrototypeNumber { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public decimal TotalActualCost { get; set; }
        public double TotalEstimatedHours { get; set; }
        public double TotalActualHours { get; set; }
        public string CurrentStage { get; set; } = string.Empty;
        public double OverallProgress { get; set; }
        public int CompletedStages { get; set; }
        public int TotalStages { get; set; }
    }

    /// <summary>
    /// Production stage information for process management
    /// </summary>
    public class ProductionStageInfo
    {
        public int ProductionStageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public int ActiveExecutions { get; set; }
        public int QueuedExecutions { get; set; }
        public double AverageCompletionTime { get; set; }
        public decimal AverageCost { get; set; }
        public bool RequiresQualityCheck { get; set; }
        public bool RequiresApproval { get; set; }
        public string RequiredRole { get; set; } = string.Empty;
    }

    /// <summary>
    /// Master schedule job information for integration
    /// </summary>
    public class MasterScheduleJobInfo
    {
        public int JobId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }
        public DateTime? ActualStart { get; set; }
        public int Priority { get; set; }
        public double ProgressPercent { get; set; }
        public bool IsDelayed { get; set; }
        public int DelayMinutes { get; set; }
        public string DelayReason { get; set; } = string.Empty;
        public bool RequiresAttention { get; set; }
    }

    /// <summary>
    /// Alert information for dashboard notifications
    /// </summary>
    public class AlertInfo
    {
        public int AlertId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public string JobId { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsAcknowledged { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string SeverityClass => Severity.ToLower() switch
        {
            "critical" => "bg-red-100 text-red-800 border-red-200",
            "high" => "bg-orange-100 text-orange-800 border-orange-200",
            "medium" => "bg-yellow-100 text-yellow-800 border-yellow-200",
            "low" => "bg-blue-100 text-blue-800 border-blue-200",
            _ => "bg-gray-100 text-gray-800 border-gray-200"
        };
    }

    /// <summary>
    /// Maintenance alert information
    /// </summary>
    public class MaintenanceAlert
    {
        public string MachineId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public string Severity { get; set; } = string.Empty;
        public bool IsOverdue => DaysOverdue > 0;
        public bool IsCritical => DaysOverdue > 7 || Severity == "Critical";
    }

    /// <summary>
    /// Multi-stage manufacturing workflow view model
    /// </summary>
    public class MultiStageWorkflowViewModel
    {
        public int JobId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string PartDescription { get; set; } = string.Empty;
        public List<WorkflowStageInfo> Stages { get; set; } = new();
        public int CurrentStageIndex { get; set; }
        public double OverallProgress { get; set; }
        public string OverallStatus { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public List<WorkflowAlert> Alerts { get; set; } = new();
        public bool CanAdvanceToNextStage { get; set; }
        public string NextStageBlockedReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Workflow stage information
    /// </summary>
    public class WorkflowStageInfo
    {
        public int StageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ExecutionOrder { get; set; }
        public DateTime? ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }
        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }
        public double ProgressPercent { get; set; }
        public string AssignedOperator { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; }
        public bool CanStart { get; set; }
        public List<string> Dependencies { get; set; } = new();
        public List<string> QualityCheckpoints { get; set; } = new();
        public bool RequiresApproval { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

    /// <summary>
    /// Workflow alert information
    /// </summary>
    public class WorkflowAlert
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int StageId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}