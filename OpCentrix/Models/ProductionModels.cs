using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Master part definition - the "what we're making"
    /// Single part number that flows through all stages
    /// </summary>
    public class MasterPart
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; } = string.Empty; // e.g., "PN-12345"
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Material { get; set; } = "Ti-6Al-4V Grade 5";
        
        public bool IsActive { get; set; } = true;
        
        // Manufacturing approach
        [Required]
        [StringLength(50)]
        public string ManufacturingApproach { get; set; } = "SLS-Based"; // "SLS-Based" or "RawMaterial-Based"
        
        // SLS stacking configuration
        public bool AllowStacking { get; set; } = false;
        public double? SingleStackDurationHours { get; set; }
        public double? DoubleStackDurationHours { get; set; }
        public double? TripleStackDurationHours { get; set; }
        public int? MaxStackCount { get; set; } = 1;
        
        // Required stages (JSON array)
        [Required]
        [StringLength(1000)]
        public string RequiredStages { get; set; } = "[]"; // ["SLS", "EDM", "CNC"]
        
        // Audit
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";
        [StringLength(100)]
        public string LastModifiedBy { get; set; } = "System";
        
        // Navigation
        public virtual ICollection<ProductionBuild> ProductionBuilds { get; set; } = new List<ProductionBuild>();
        public virtual ICollection<StageDefinition> StageDefinitions { get; set; } = new List<StageDefinition>();
    }

    /// <summary>
    /// Production build - represents a single print job that produces multiple parts
    /// The "print job" that creates a batch of parts
    /// </summary>
    public class ProductionBuild
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string BuildNumber { get; set; } = string.Empty; // Auto-generated: BUILD-2025-001
        
        [Required]
        public int MasterPartId { get; set; }
        public virtual MasterPart MasterPart { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string PrinterName { get; set; } = string.Empty; // TI1, TI2, INC
        
        [Required]
        public int BuildQuantity { get; set; } = 1; // Operator-determined quantity
        
        public int StackLevel { get; set; } = 1; // 1x, 2x, 3x stacking
        
        [Required]
        [StringLength(100)]
        public string MaterialBatch { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string PowderLot { get; set; } = string.Empty;
        
        public bool AddedPowder { get; set; }
        public decimal? PowderAmountKg { get; set; }
        
        // Time tracking
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public DateTime? ScheduledStartTime { get; set; }
        public DateTime? ScheduledEndTime { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Planned"; // Planned, InProgress, Completed, Aborted
        
        [Required]
        public int CreatedByUserId { get; set; }
        public virtual User CreatedByUser { get; set; } = null!;
        
        [StringLength(1000)]
        public string? SetupNotes { get; set; }
        
        [StringLength(1000)]
        public string? CompletionNotes { get; set; }
        
        // Audit
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }
        
        // Navigation
        public virtual ICollection<StageExecution> StageExecutions { get; set; } = new List<StageExecution>();
        public virtual ICollection<PartBatch> PartBatches { get; set; } = new List<PartBatch>();
        
        // Computed properties
        [NotMapped]
        public string DisplayName => $"{BuildNumber} - {MasterPart?.PartNumber} (×{BuildQuantity})";
        
        [NotMapped]
        public double ActualDurationHours => ActualEndTime.HasValue && ActualStartTime.HasValue 
            ? (ActualEndTime.Value - ActualStartTime.Value).TotalHours 
            : 0;
    }

    /// <summary>
    /// Stage definition - defines what stages a part requires and their parameters
    /// Links master parts to their required stages with stage-specific configuration
    /// </summary>
    public class StageDefinition
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int MasterPartId { get; set; }
        public virtual MasterPart MasterPart { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string StageName { get; set; } = string.Empty; // SLS, EDM, CNC, Assembly
        
        [Required]
        public int ExecutionOrder { get; set; } // 1, 2, 3, 4...
        
        public bool IsRequired { get; set; } = true;
        public bool CanSkip { get; set; } = false;
        
        // Time estimates
        public double EstimatedHoursPerPart { get; set; } = 1.0;
        public int SetupMinutes { get; set; } = 30;
        public int TeardownMinutes { get; set; } = 15;
        
        // Machine requirements
        [StringLength(100)]
        public string? RequiredMachineType { get; set; } // "SLS", "EDM", "CNC", "Assembly"
        
        [StringLength(100)]
        public string? PreferredMachines { get; set; } // "TI1,TI2" or "CNC-1,CNC-2"
        
        // Stage-specific configuration (JSON)
        [StringLength(2000)]
        public string StageConfiguration { get; set; } = "{}";
        
        // Quality requirements
        [StringLength(1000)]
        public string? QualityRequirements { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Stage execution - tracks actual execution of a stage for a production build
    /// The "work order" for a specific stage
    /// </summary>
    public class StageExecution
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProductionBuildId { get; set; }
        public virtual ProductionBuild ProductionBuild { get; set; } = null!;
        
        [Required]
        public int StageDefinitionId { get; set; }
        public virtual StageDefinition StageDefinition { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string StageName { get; set; } = string.Empty; // SLS, EDM, CNC, Assembly
        
        [Required]
        public int ExecutionOrder { get; set; }
        
        // Quantity tracking
        public int QuantityIn { get; set; } // Parts received into this stage
        public int QuantityOut { get; set; } // Parts successfully completed
        public int DefectCount { get; set; } = 0;
        public int ReworkCount { get; set; } = 0;
        
        // Time tracking
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? ActualHours { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed, Failed, Skipped
        
        // Machine and operator
        [StringLength(50)]
        public string? MachineUsed { get; set; }
        
        public int? OperatorUserId { get; set; }
        public virtual User? OperatorUser { get; set; }
        
        // Stage-specific data (JSON for flexibility)
        [StringLength(5000)]
        public string StageData { get; set; } = "{}";
        
        // Notes and quality
        [StringLength(2000)]
        public string? OperatorNotes { get; set; }
        
        [StringLength(2000)]
        public string? QualityNotes { get; set; }
        
        public bool PassedQuality { get; set; } = true;
        
        // Cost tracking
        public decimal? ActualCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public decimal? LaborCost { get; set; }
        
        // Audit
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }
        
        // Computed properties
        [NotMapped]
        public double YieldPercentage => QuantityIn > 0 ? (double)QuantityOut / QuantityIn * 100 : 0;
        
        [NotMapped]
        public bool IsCompleted => Status == "Completed";
        
        [NotMapped]
        public bool CanStart => Status == "NotStarted" && QuantityIn > 0;
    }

    /// <summary>
    /// Part batch - tracks parts as they move through the system
    /// Represents a "batch" of parts at a specific stage
    /// </summary>
    public class PartBatch
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProductionBuildId { get; set; }
        public virtual ProductionBuild ProductionBuild { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string BatchNumber { get; set; } = string.Empty; // BUILD-001-SLS, BUILD-001-CNC
        
        [Required]
        [StringLength(50)]
        public string CurrentStage { get; set; } = "SLS"; // Current stage of this batch
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        [StringLength(50)]
        public string QualityStatus { get; set; } = "Good"; // Good, Defective, Rework, Scrap
        
        [StringLength(100)]
        public string? Location { get; set; } // Physical location/bin
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        // REMOVED: Navigation to stage executions - this relationship doesn't make sense
        // PartBatch and StageExecution are parallel entities under ProductionBuild
        // If you need to find related stage executions, do it through ProductionBuildId
    }

    /// <summary>
    /// Stage data templates for common stage types
    /// Defines the JSON structure for StageExecution.StageData
    /// </summary>
    public static class StageDataTemplates
    {
        public static class SLS
        {
            public const string Template = @"{
                ""LaserPower"": 170,
                ""ScanSpeed"": 1000,
                ""LayerThickness"": 30,
                ""HatchSpacing"": 120,
                ""BuildTemperature"": 180,
                ""ArgonPurity"": 99.9,
                ""OxygenContent"": 50,
                ""SupportComplexity"": ""Medium"",
                ""BuildHeight"": 0,
                ""LayerCount"": 0,
                ""LaserRunTime"": """",
                ""GasUsedL"": 0,
                ""PowderUsedL"": 0
            }";
        }

        public static class EDM
        {
            public const string Template = @"{
                ""WireType"": ""Brass"",
                ""WireDiameter"": 0.25,
                ""CutSpeed"": 5.0,
                ""FlushPressure"": 0.5,
                ""CutOffHeight"": 2.0,
                ""SurfaceFinish"": ""Ra 3.2"",
                ""DielectricType"": ""Deionized Water"",
                ""ElectrodeWear"": ""Low"",
                ""CutTime"": 0,
                ""WireLength"": 0
            }";
        }

        public static class CNC
        {
            public const string Template = @"{
                ""Program"": """",
                ""ToolsUsed"": [],
                ""SpindleSpeed"": 2500,
                ""FeedRate"": 500,
                ""CoolantType"": ""Flood"",
                ""WorkHolding"": ""Vise"",
                ""Operations"": [],
                ""CycleTime"": 0,
                ""ToolChanges"": 0,
                ""MeasuredDimensions"": {}
            }";
        }

        public static class Assembly
        {
            public const string Template = @"{
                ""Components"": [],
                ""Hardware"": [],
                ""TorqueSpecs"": {},
                ""TestPressure"": 0,
                ""LeakTest"": false,
                ""FunctionTest"": false,
                ""SerialNumbers"": [],
                ""PackagingType"": ""Standard""
            }";
        }
    }
}