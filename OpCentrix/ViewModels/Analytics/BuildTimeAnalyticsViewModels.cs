using System.ComponentModel.DataAnnotations;

namespace OpCentrix.ViewModels.Analytics
{
    /// <summary>
    /// Phase 5 Build Time Analytics View Models
    /// Supports comprehensive build time analytics, machine learning, and performance insights
    /// </summary>

    #region Core Analytics ViewModels

    public class BuildTimeAnalyticsViewModel
    {
        public DateRangeViewModel DateRange { get; set; } = new();
        public int TotalBuilds { get; set; }
        public decimal AverageEstimateAccuracy { get; set; }
        public List<MachinePerformanceData> MachinePerformanceData { get; set; } = new();
        public List<QualityMetrics> QualityTrends { get; set; } = new();
        public List<OperatorPerformanceData> OperatorPerformanceData { get; set; } = new();
        public List<BuildOptimizationSuggestion> OptimizationSuggestions { get; set; } = new();
        public List<BuildPerformanceData> TopPerformingBuilds { get; set; } = new();
        public List<BuildPerformanceData> ProblemBuilds { get; set; } = new();
        public LearningModelInsights LearningInsights { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class DateRangeViewModel
    {
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow.AddDays(-30);

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.UtcNow;
    }

    #endregion

    #region Machine Performance ViewModels

    public class MachinePerformanceComparisonViewModel
    {
        public List<MachinePerformanceData> MachinePerformanceData { get; set; } = new();
        public string ComparisonPeriod { get; set; } = string.Empty;
        public string TopPerformingMachine { get; set; } = string.Empty;
        public string MostUtilizedMachine { get; set; } = string.Empty;
        public string BestQualityMachine { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    public class MachinePerformanceData
    {
        public string MachineId { get; set; } = string.Empty;
        public int TotalBuilds { get; set; }
        public double AverageAccuracy { get; set; }
        public double AverageQualityScore { get; set; }
        public double AverageDefectRate { get; set; }
        public double AverageHours { get; set; }
        public double UtilizationRate { get; set; }

        // Display properties
        public string AccuracyDisplay => $"{AverageAccuracy:F1}%";
        public string QualityDisplay => $"{AverageQualityScore:F1}%";
        public string UtilizationDisplay => $"{UtilizationRate:F1}%";
        public string PerformanceRating => GetPerformanceRating();

        private string GetPerformanceRating()
        {
            var score = (AverageAccuracy + AverageQualityScore + (100 - AverageDefectRate)) / 3;
            return score switch
            {
                >= 90 => "Excellent",
                >= 80 => "Good",
                >= 70 => "Fair",
                _ => "Needs Improvement"
            };
        }
    }

    public class MachineUtilizationTrend
    {
        public string MachineId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal HoursUsed { get; set; }
        public int BuildCount { get; set; }
        public double UtilizationRate { get; set; }
    }

    #endregion

    #region Operator Performance ViewModels

    public class OperatorPerformanceViewModel
    {
        public string OperatorName { get; set; } = string.Empty;
        public int TotalBuilds { get; set; }
        public double AverageAccuracy { get; set; }
        public double AverageQualityScore { get; set; }
        public string AccuracyTrend { get; set; } = string.Empty;
        public List<OperatorAccuracyData> AccuracyData { get; set; } = new();
        public List<string> ImprovementSuggestions { get; set; } = new();
        public string PerformancePeriod { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }

        // Display properties
        public string AccuracyDisplay => $"{AverageAccuracy:F1}%";
        public string QualityDisplay => $"{AverageQualityScore:F1}%";
        public string TrendIcon => AccuracyTrend switch
        {
            "Improving" => "??",
            "Declining" => "??",
            "Stable" => "??",
            _ => "?"
        };
    }

    public class OperatorPerformanceData
    {
        public string OperatorName { get; set; } = string.Empty;
        public int TotalBuilds { get; set; }
        public double AverageAccuracy { get; set; }
        public double AverageQualityScore { get; set; }
    }

    public class OperatorAccuracyData
    {
        public DateTime Date { get; set; }
        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public double Accuracy { get; set; }
        public decimal QualityScore { get; set; }

        public string AccuracyDisplay => $"{Accuracy:F1}%";
        public string VarianceDisplay => $"{EstimatedHours - ActualHours:+0.0;-0.0;0.0}h";
    }

    #endregion

    #region Prediction and ML ViewModels

    public class BuildTimeEstimate
    {
        public decimal EstimatedHours { get; set; }
        public int ConfidenceLevel { get; set; }
        public List<string> FactorsConsidered { get; set; } = new();
        public string RecommendedMachine { get; set; } = string.Empty;
        public string DataQuality { get; set; } = string.Empty;
        public List<BuildOptimization> Optimizations { get; set; } = new();

        // Display properties
        public string EstimateDisplay => $"{EstimatedHours:F1} hours";
        public string ConfidenceDisplay => $"{ConfidenceLevel}%";
        public string ConfidenceBadgeClass => ConfidenceLevel switch
        {
            >= 80 => "badge-success",
            >= 60 => "badge-warning",
            _ => "badge-danger"
        };
    }

    public class BuildOptimization
    {
        public string Type { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;
        public string EstimatedImpact { get; set; } = string.Empty;
    }

    public class BuildParameters
    {
        public string MachineId { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string SupportComplexity { get; set; } = "Medium";
        public decimal BuildHeight { get; set; }
        public int LayerCount { get; set; }
        public int TotalParts { get; set; } = 1;
        public string BuildFileHash { get; set; } = string.Empty;
    }

    public class QualityPrediction
    {
        public decimal PredictedQualityRate { get; set; }
        public int ConfidenceLevel { get; set; }
        public List<QualityRiskFactor> RiskFactors { get; set; } = new();
        public List<QualityRecommendation> Recommendations { get; set; } = new();

        public string QualityDisplay => $"{PredictedQualityRate:F1}%";
        public string QualityBadgeClass => PredictedQualityRate switch
        {
            >= 95 => "badge-success",
            >= 85 => "badge-warning",
            _ => "badge-danger"
        };
    }

    public class QualityRiskFactor
    {
        public string Factor { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;

        public string ImpactBadgeClass => Impact switch
        {
            "High" => "badge-danger",
            "Medium" => "badge-warning",
            "Low" => "badge-info",
            _ => "badge-secondary"
        };
    }

    public class QualityRecommendation
    {
        public string Priority { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;

        public string PriorityBadgeClass => Priority switch
        {
            "High" => "badge-danger",
            "Medium" => "badge-warning",
            "Low" => "badge-info",
            _ => "badge-secondary"
        };
    }

    #endregion

    #region Optimization ViewModels

    public class BuildOptimizationSuggestion
    {
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;
        public string PotentialImpact { get; set; } = string.Empty;
        public string EstimatedSavings { get; set; } = string.Empty;

        public string PriorityBadgeClass => Priority switch
        {
            "High" => "badge-danger",
            "Medium" => "badge-warning",
            "Low" => "badge-info",
            _ => "badge-secondary"
        };

        public string CategoryIcon => Category switch
        {
            "Machine Utilization" => "??",
            "Quality Improvement" => "??",
            "Time Estimation" => "?",
            "Cost Optimization" => "??",
            _ => "??"
        };
    }

    #endregion

    #region Trend Analysis ViewModels

    public class BuildTimeTrend
    {
        public DateTime Date { get; set; }
        public decimal AverageHours { get; set; }
        public double AverageAccuracy { get; set; }
        public int BuildCount { get; set; }
        public decimal QualityScore { get; set; }

        public string DateDisplay => Date.ToString("MMM dd");
        public string AccuracyDisplay => $"{AverageAccuracy:F1}%";
    }

    public class QualityTrend
    {
        public DateTime Date { get; set; }
        public decimal QualityRate { get; set; }
        public decimal DefectRate { get; set; }
        public decimal ReworkRate { get; set; }
        public int PartsCompleted { get; set; }

        public string DateDisplay => Date.ToString("MMM dd");
        public string QualityDisplay => $"{QualityRate:F1}%";
        public string DefectDisplay => $"{DefectRate:F1}%";
    }

    public class QualityMetrics
    {
        public DateTime Date { get; set; }
        public decimal QualityRate { get; set; }
        public int DefectCount { get; set; }
        public int TotalParts { get; set; }

        public decimal DefectRate => TotalParts > 0 ? (decimal)DefectCount / TotalParts * 100 : 0;
        public string QualityDisplay => $"{QualityRate:F1}%";
    }

    #endregion

    #region Learning Model ViewModels

    public class LearningModelInsights
    {
        public int TotalDataPoints { get; set; }
        public int RecentDataPoints { get; set; }
        public decimal ModelAccuracy { get; set; }
        public DateTime LastUpdated { get; set; }
        public string DataQuality { get; set; } = string.Empty;
        public List<string> RecommendedActions { get; set; } = new();

        public string AccuracyDisplay => $"{ModelAccuracy:F1}%";
        public string DataQualityBadgeClass => DataQuality switch
        {
            "Excellent" => "badge-success",
            "Good" => "badge-primary",
            "Fair" => "badge-warning",
            "Poor" or "Building Dataset" => "badge-info",
            _ => "badge-secondary"
        };
        
        public string LastUpdatedDisplay => LastUpdated.ToString("MMM dd, yyyy 'at' HH:mm");
    }

    public class BuildPerformanceData
    {
        public int BuildJobId { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public decimal QualityScore { get; set; }
        public double AccuracyScore { get; set; }
        public DateTime Date { get; set; }

        public string QualityDisplay => $"{QualityScore:F1}%";
        public string AccuracyDisplay => $"{AccuracyScore:F1}%";
        public string DateDisplay => Date.ToString("MMM dd");
        public string OverallScore => $"{(QualityScore + (decimal)AccuracyScore) / 2:F1}%";
    }

    #endregion

    #region Chart Data ViewModels

    public class ChartDataViewModel
    {
        public Dictionary<string, object> TimeAccuracyChart { get; set; } = new();
        public Dictionary<string, object> QualityTrendChart { get; set; } = new();
        public Dictionary<string, object> MachineComparisonChart { get; set; } = new();
        public Dictionary<string, object> OperatorPerformanceChart { get; set; } = new();
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    #endregion
}

#region Extension Methods for Analytics

public static class AnalyticsExtensions
{
    public static double Variance(this IEnumerable<decimal> values)
    {
        var enumerable = values as decimal[] ?? values.ToArray();
        if (!enumerable.Any()) return 0;

        var average = enumerable.Average();
        var sum = enumerable.Sum(d => (double)Math.Pow((double)(d - average), 2));
        return sum / enumerable.Length;
    }

    public static double Variance(this IEnumerable<double> values)
    {
        var enumerable = values as double[] ?? values.ToArray();
        if (!enumerable.Any()) return 0;

        var average = enumerable.Average();
        var sum = enumerable.Sum(d => Math.Pow(d - average, 2));
        return sum / enumerable.Length;
    }

    public static string ToPercentageDisplay(this double value) => $"{value:F1}%";
    public static string ToPercentageDisplay(this decimal value) => $"{value:F1}%";
    
    public static string GetTrendIcon(this string trend) => trend switch
    {
        "Improving" => "??",
        "Declining" => "??", 
        "Stable" => "??",
        _ => "?"
    };

    public static string GetPerformanceBadgeClass(this double score) => score switch
    {
        >= 90 => "badge-success",
        >= 80 => "badge-primary", 
        >= 70 => "badge-warning",
        _ => "badge-danger"
    };
}

#endregion