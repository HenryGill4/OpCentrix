using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

public class Part
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string PartNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Material { get; set; } = "Ti-6Al-4V Grade 5";

    [Required]
    [StringLength(100)]
    public string ManufacturingApproach { get; set; } = "SLS-Based";

    #region SLS Stacking Configuration

    public bool AllowStacking { get; set; }

    [Range(0.1, 500.0)]
    public double? SingleStackDurationHours { get; set; }

    [Range(0.1, 500.0)]
    public double? DoubleStackDurationHours { get; set; }

    [Range(0.1, 500.0)]
    public double? TripleStackDurationHours { get; set; }

    [Range(1, 10)]
    public int MaxStackCount { get; set; } = 1;

    [Required]
    [Range(1, 100)]
    public int PartsPerBuildSingle { get; set; } = 1;

    [Range(1, 100)]
    public int? PartsPerBuildDouble { get; set; }

    [Range(1, 100)]
    public int? PartsPerBuildTriple { get; set; }

    public bool EnableDoubleStack { get; set; }

    public bool EnableTripleStack { get; set; }

    [Range(0.1, 500.0)]
    public double? StageEstimateSingle { get; set; }

    #endregion

    #region Batch Stage Durations

    [Range(0.1, 500.0)]
    public double? SlsBuildDurationHours { get; set; }

    [Range(1, 100)]
    public int? SlsPartsPerBuild { get; set; }

    [Range(0.1, 100.0)]
    public double? DepowderingDurationHours { get; set; }

    [Range(1, 100)]
    public int? DepowderingPartsPerBatch { get; set; }

    [Range(0.1, 100.0)]
    public double? HeatTreatmentDurationHours { get; set; }

    [Range(1, 100)]
    public int? HeatTreatmentPartsPerBatch { get; set; }

    [Range(0.1, 100.0)]
    public double? WireEdmDurationHours { get; set; }

    [Range(1, 100)]
    public int? WireEdmPartsPerSession { get; set; }

    #endregion

    #region Stage Config

    [Required]
    [StringLength(1000)]
    public string RequiredStages { get; set; } = "[]";

    #endregion

    #region Status + Audit

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastModifiedBy { get; set; } = string.Empty;

    #endregion

    #region Navigation Properties

    public virtual ICollection<PartStageRequirement> StageRequirements { get; set; } = new List<PartStageRequirement>();
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    #endregion

    #region Computed Properties

    [NotMapped]
    public double? SlsPerPartHours => SlsBuildDurationHours.HasValue && SlsPartsPerBuild is > 0
        ? SlsBuildDurationHours.Value / SlsPartsPerBuild.Value
        : null;

    [NotMapped]
    public double? DepowderingPerPartHours => DepowderingDurationHours.HasValue && DepowderingPartsPerBatch is > 0
        ? DepowderingDurationHours.Value / DepowderingPartsPerBatch.Value
        : null;

    [NotMapped]
    public double? HeatTreatmentPerPartHours => HeatTreatmentDurationHours.HasValue && HeatTreatmentPartsPerBatch is > 0
        ? HeatTreatmentDurationHours.Value / HeatTreatmentPartsPerBatch.Value
        : null;

    [NotMapped]
    public double? WireEdmPerPartHours => WireEdmDurationHours.HasValue && WireEdmPartsPerSession is > 0
        ? WireEdmDurationHours.Value / WireEdmPartsPerSession.Value
        : null;

    [NotMapped]
    public bool HasStackingConfiguration => AllowStacking && SingleStackDurationHours.HasValue;

    [NotMapped]
    public double EffectiveSingleDuration => SingleStackDurationHours ?? StageEstimateSingle ?? 8.0;

    [NotMapped]
    public bool HasValidDoubleStack => EnableDoubleStack && DoubleStackDurationHours.HasValue && PartsPerBuildDouble.HasValue;

    [NotMapped]
    public bool HasValidTripleStack => EnableTripleStack && TripleStackDurationHours.HasValue && PartsPerBuildTriple.HasValue;

    [NotMapped]
    public List<int> AvailableStackLevels
    {
        get
        {
            var levels = new List<int> { 1 };
            if (HasValidDoubleStack) levels.Add(2);
            if (HasValidTripleStack) levels.Add(3);
            return levels;
        }
    }

    #endregion

    #region Helper Methods

    public double? GetStackDuration(int level)
    {
        return level switch
        {
            1 => SingleStackDurationHours ?? StageEstimateSingle,
            2 => HasValidDoubleStack ? DoubleStackDurationHours : null,
            3 => HasValidTripleStack ? TripleStackDurationHours : null,
            _ => null
        };
    }

    public int? GetPartsPerBuild(int level)
    {
        return level switch
        {
            1 => PartsPerBuildSingle,
            2 => HasValidDoubleStack ? PartsPerBuildDouble : null,
            3 => HasValidTripleStack ? PartsPerBuildTriple : null,
            _ => null
        };
    }

    public List<string> ValidateStackingConfiguration()
    {
        var issues = new List<string>();

        if (AllowStacking)
        {
            if (!SingleStackDurationHours.HasValue)
                issues.Add("Single stack duration must be specified when stacking is allowed");

            if (EnableDoubleStack && !DoubleStackDurationHours.HasValue)
                issues.Add("Double stack duration must be specified when double stack is enabled");

            if (EnableDoubleStack && !PartsPerBuildDouble.HasValue)
                issues.Add("Parts per build (double) must be specified when double stack is enabled");

            if (EnableTripleStack && !TripleStackDurationHours.HasValue)
                issues.Add("Triple stack duration must be specified when triple stack is enabled");

            if (EnableTripleStack && !PartsPerBuildTriple.HasValue)
                issues.Add("Parts per build (triple) must be specified when triple stack is enabled");

            if (EnableTripleStack && !EnableDoubleStack)
                issues.Add("Double stack must be enabled before triple stack can be enabled");
        }

        return issues;
    }

    public int GetRecommendedStackLevel(int quantity)
    {
        if (!AllowStacking || !HasStackingConfiguration)
            return 1;

        if (HasValidTripleStack && quantity >= PartsPerBuildTriple)
            return 3;
        if (HasValidDoubleStack && quantity >= PartsPerBuildDouble)
            return 2;
        return 1;
    }

    public double CalculateStackEfficiency(int stackLevel, int quantity)
    {
        var partsPerBuild = GetPartsPerBuild(stackLevel);
        var duration = GetStackDuration(stackLevel);

        if (!partsPerBuild.HasValue || !duration.HasValue || partsPerBuild <= 0 || duration <= 0)
            return 0;

        var buildsNeeded = Math.Ceiling((double)quantity / partsPerBuild.Value);
        var totalHours = buildsNeeded * duration.Value;
        var partsPerHour = quantity / totalHours;

        return partsPerHour;
    }

    #endregion
}
