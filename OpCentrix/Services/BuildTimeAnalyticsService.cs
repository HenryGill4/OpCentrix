using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.ViewModels.Analytics;
using System.Text.Json;

namespace OpCentrix.Services
{
    /// <summary>
    /// Build Time Analytics Service for Phase 5 - Machine Learning and Performance Analytics
    /// Leverages Phase 4 learning tables: PartCompletionLogs, OperatorEstimateLogs, BuildTimeLearningData
    /// </summary>
    public interface IBuildTimeAnalyticsService
    {
        // Core Analytics
        Task<BuildTimeAnalyticsViewModel> GetBuildTimeAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<MachinePerformanceComparisonViewModel> GetMachinePerformanceComparisonAsync();
        Task<OperatorPerformanceViewModel> GetOperatorPerformanceAsync(string operatorName);
        
        // Machine Learning & Prediction
        Task<ViewModels.Analytics.BuildTimeEstimate> PredictBuildTimeAsync(string partNumber, string machineId, string? buildFileHash = null);
        Task<QualityPrediction> PredictQualityOutcomeAsync(BuildParameters parameters);
        Task<List<BuildOptimizationSuggestion>> GetOptimizationSuggestionsAsync();
        
        // Trend Analysis
        Task<List<BuildTimeTrend>> GetBuildTimeTrendsAsync(int daysBack = 30);
        Task<List<QualityTrend>> GetQualityTrendsAsync(int daysBack = 30);
        Task<List<MachineUtilizationTrend>> GetMachineUtilizationTrendsAsync();
        
        // Learning Engine
        Task<LearningModelInsights> GetLearningModelInsightsAsync();
        Task<bool> UpdateLearningModelAsync();
    }

    public class BuildTimeAnalyticsService : IBuildTimeAnalyticsService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<BuildTimeAnalyticsService> _logger;

        public BuildTimeAnalyticsService(SchedulerContext context, ILogger<BuildTimeAnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BuildTimeAnalyticsViewModel> GetBuildTimeAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Loading build time analytics from {StartDate} to {EndDate}", startDate, endDate);

                // Get build time learning data for the period
                var learningData = await _context.BuildTimeLearningData
                    .Where(btl => btl.RecordedAt >= startDate && btl.RecordedAt <= endDate)
                    .Include(btl => btl.BuildJob)
                    .ToListAsync();

                // Get operator estimate data
                var operatorEstimates = await _context.OperatorEstimateLogs
                    .Where(oel => oel.LoggedAt >= startDate && oel.LoggedAt <= endDate)
                    .Include(oel => oel.BuildJob)
                    .ToListAsync();

                // Get part completion data
                var partCompletions = await _context.PartCompletionLogs
                    .Where(pcl => pcl.CompletedAt >= startDate && pcl.CompletedAt <= endDate)
                    .Include(pcl => pcl.BuildJob)
                    .ToListAsync();

                // Calculate analytics
                var viewModel = new BuildTimeAnalyticsViewModel
                {
                    DateRange = new DateRangeViewModel { StartDate = startDate, EndDate = endDate },
                    TotalBuilds = learningData.Count,
                    AverageEstimateAccuracy = CalculateAverageAccuracy(learningData),
                    MachinePerformanceData = CalculateMachinePerformance(learningData),
                    QualityTrends = CalculateQualityTrends(partCompletions),
                    OperatorPerformanceData = CalculateOperatorPerformance(operatorEstimates, learningData),
                    OptimizationSuggestions = await GenerateOptimizationSuggestions(learningData),
                    TopPerformingBuilds = GetTopPerformingBuilds(learningData),
                    ProblemBuilds = GetProblemBuilds(learningData),
                    LearningInsights = await GetLearningModelInsightsAsync()
                };

                _logger.LogInformation("Build time analytics loaded successfully: {TotalBuilds} builds analyzed", viewModel.TotalBuilds);
                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading build time analytics");
                return new BuildTimeAnalyticsViewModel
                {
                    DateRange = new DateRangeViewModel { StartDate = startDate, EndDate = endDate },
                    ErrorMessage = "Unable to load analytics data. Please try again."
                };
            }
        }

        public async Task<MachinePerformanceComparisonViewModel> GetMachinePerformanceComparisonAsync()
        {
            try
            {
                _logger.LogInformation("Loading machine performance comparison data");

                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var machineData = await _context.BuildTimeLearningData
                    .Where(btl => btl.RecordedAt >= thirtyDaysAgo)
                    .GroupBy(btl => btl.MachineId)
                    .Select(g => new MachinePerformanceData
                    {
                        MachineId = g.Key,
                        TotalBuilds = g.Count(),
                        AverageAccuracy = g.Average(x => 100 - Math.Abs((double)x.VariancePercent)),
                        AverageQualityScore = (double)g.Average(x => x.QualityScore),
                        AverageDefectRate = (double)g.Average(x => x.DefectCount),
                        AverageHours = (double)g.Average(x => x.ActualHours),
                        UtilizationRate = CalculateUtilizationRate(g.Key, thirtyDaysAgo)
                    })
                    .ToListAsync();

                return new MachinePerformanceComparisonViewModel
                {
                    MachinePerformanceData = machineData,
                    ComparisonPeriod = "Last 30 Days",
                    TopPerformingMachine = machineData.OrderByDescending(m => m.AverageAccuracy).FirstOrDefault()?.MachineId ?? "N/A",
                    MostUtilizedMachine = machineData.OrderByDescending(m => m.UtilizationRate).FirstOrDefault()?.MachineId ?? "N/A",
                    BestQualityMachine = machineData.OrderByDescending(m => m.AverageQualityScore).FirstOrDefault()?.MachineId ?? "N/A"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading machine performance comparison");
                return new MachinePerformanceComparisonViewModel { ErrorMessage = "Unable to load machine performance data." };
            }
        }

        public async Task<OperatorPerformanceViewModel> GetOperatorPerformanceAsync(string operatorName)
        {
            try
            {
                _logger.LogInformation("Loading operator performance for {OperatorName}", operatorName);

                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                
                // Get operator estimates based on BuildJob User
                var estimates = await _context.OperatorEstimateLogs
                    .Where(oel => oel.BuildJob.User != null && oel.BuildJob.User.Username == operatorName && oel.LoggedAt >= thirtyDaysAgo)
                    .Include(oel => oel.BuildJob)
                        .ThenInclude(bj => bj.User)
                    .ToListAsync();

                // Get corresponding learning data
                var buildIds = estimates.Select(e => e.BuildJobId).ToList();
                var learningData = await _context.BuildTimeLearningData
                    .Where(btl => buildIds.Contains(btl.BuildJobId))
                    .ToListAsync();

                // Calculate performance metrics
                var accuracyData = learningData
                    .Select(ld => new OperatorAccuracyData
                    {
                        Date = ld.RecordedAt,
                        EstimatedHours = ld.OperatorEstimatedHours,
                        ActualHours = ld.ActualHours,
                        Accuracy = 100 - Math.Abs((double)ld.VariancePercent),
                        QualityScore = ld.QualityScore
                    })
                    .OrderBy(ad => ad.Date)
                    .ToList();

                return new OperatorPerformanceViewModel
                {
                    OperatorName = operatorName,
                    TotalBuilds = estimates.Count,
                    AverageAccuracy = accuracyData.Any() ? accuracyData.Average(ad => ad.Accuracy) : 0,
                    AverageQualityScore = accuracyData.Any() ? (double)accuracyData.Average(ad => ad.QualityScore) : 0,
                    AccuracyTrend = CalculateAccuracyTrend(accuracyData),
                    AccuracyData = accuracyData,
                    ImprovementSuggestions = GenerateOperatorImprovementSuggestions(accuracyData),
                    PerformancePeriod = "Last 30 Days"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading operator performance for {OperatorName}", operatorName);
                return new OperatorPerformanceViewModel 
                { 
                    OperatorName = operatorName,
                    ErrorMessage = "Unable to load operator performance data."
                };
            }
        }

        public async Task<ViewModels.Analytics.BuildTimeEstimate> PredictBuildTimeAsync(string partNumber, string machineId, string? buildFileHash = null)
        {
            try
            {
                _logger.LogInformation("Predicting build time for part {PartNumber} on machine {MachineId}", partNumber, machineId);

                // Get historical data for similar builds
                var historicalData = await _context.BuildTimeLearningData
                    .Where(btl => btl.MachineId == machineId)
                    .Where(btl => buildFileHash == null || btl.BuildFileHash == buildFileHash)
                    .Include(btl => btl.BuildJob)
                    .OrderByDescending(btl => btl.RecordedAt)
                    .Take(20) // Use last 20 similar builds
                    .ToListAsync();

                if (!historicalData.Any())
                {
                    // Fallback to general part data from Jobs table
                    var partEstimate = await _context.Jobs
                        .Where(j => j.PartNumber == partNumber)
                        .AverageAsync(j => j.EstimatedHours);

                    return new ViewModels.Analytics.BuildTimeEstimate
                    {
                        EstimatedHours = (decimal)partEstimate,
                        ConfidenceLevel = 30, // Low confidence without historical data
                        FactorsConsidered = new List<string> { "General part estimate (limited data)" },
                        RecommendedMachine = machineId,
                        DataQuality = "Limited",
                        Optimizations = new List<BuildOptimization>()
                    };
                }

                // Calculate weighted average based on recency and similarity
                var weights = historicalData.Select((data, index) => new
                {
                    Data = data,
                    Weight = 1.0 / (index + 1), // More recent data has higher weight
                    Similarity = CalculateSimilarity(data, buildFileHash)
                }).ToList();

                var totalWeight = weights.Sum(w => w.Weight * w.Similarity);
                var weightedEstimate = weights.Sum(w => (double)w.Data.ActualHours * w.Weight * w.Similarity) / totalWeight;

                // Calculate confidence based on data consistency
                var variance = historicalData.Select(hd => hd.ActualHours).Variance();
                var confidenceLevel = Math.Max(50, 100 - (int)(variance * 10));

                // Generate factors considered
                var factors = new List<string>
                {
                    $"Historical data from {historicalData.Count} similar builds",
                    $"Machine-specific performance ({machineId})",
                    buildFileHash != null ? "Exact build file match" : "Similar build patterns"
                };

                return new ViewModels.Analytics.BuildTimeEstimate
                {
                    EstimatedHours = (decimal)weightedEstimate,
                    ConfidenceLevel = confidenceLevel,
                    FactorsConsidered = factors,
                    RecommendedMachine = await GetBestMachineForBuild(partNumber),
                    DataQuality = GetDataQualityAssessment(historicalData.Count, variance),
                    Optimizations = await GenerateBuildOptimizations(historicalData)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting build time for part {PartNumber}", partNumber);
                return new ViewModels.Analytics.BuildTimeEstimate
                {
                    EstimatedHours = 8, // Safe default
                    ConfidenceLevel = 0,
                    FactorsConsidered = new List<string> { "Error occurred - using safe default" },
                    RecommendedMachine = machineId,
                    DataQuality = "Error",
                    Optimizations = new List<BuildOptimization>()
                };
            }
        }

        public async Task<QualityPrediction> PredictQualityOutcomeAsync(BuildParameters parameters)
        {
            try
            {
                // Analyze historical quality data for similar parameters
                var historicalData = await _context.BuildTimeLearningData
                    .Where(btl => btl.MachineId == parameters.MachineId)
                    .Where(btl => btl.SupportComplexity == parameters.SupportComplexity)
                    .Include(btl => btl.BuildJob)
                    .ToListAsync();

                if (!historicalData.Any())
                {
                    return new QualityPrediction
                    {
                        PredictedQualityRate = 95, // Default optimistic prediction
                        ConfidenceLevel = 30,
                        RiskFactors = new List<QualityRiskFactor>(),
                        Recommendations = new List<QualityRecommendation>
                        {
                            new QualityRecommendation { Priority = "Medium", Suggestion = "Limited historical data - monitor build closely" }
                        }
                    };
                }

                var averageQuality = (double)historicalData.Average(hd => hd.QualityScore);
                var averageDefects = (double)historicalData.Average(hd => hd.DefectCount);

                // Identify risk factors
                var riskFactors = new List<QualityRiskFactor>();
                
                if (averageDefects > 2)
                    riskFactors.Add(new QualityRiskFactor { Factor = "High defect rate for similar builds", Impact = "High" });
                
                if (parameters.SupportComplexity == "High")
                    riskFactors.Add(new QualityRiskFactor { Factor = "High support complexity", Impact = "Medium" });

                // Generate recommendations
                var recommendations = GenerateQualityRecommendations(averageQuality, averageDefects, parameters);

                return new QualityPrediction
                {
                    PredictedQualityRate = (decimal)averageQuality,
                    ConfidenceLevel = CalculateQualityConfidence(historicalData.Count),
                    RiskFactors = riskFactors,
                    Recommendations = recommendations
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting quality outcome");
                return new QualityPrediction
                {
                    PredictedQualityRate = 90,
                    ConfidenceLevel = 0,
                    RiskFactors = new List<QualityRiskFactor>(),
                    Recommendations = new List<QualityRecommendation>()
                };
            }
        }

        public async Task<List<BuildOptimizationSuggestion>> GetOptimizationSuggestionsAsync()
        {
            try
            {
                var suggestions = new List<BuildOptimizationSuggestion>();

                // Analyze recent builds for patterns
                var recentBuilds = await _context.BuildTimeLearningData
                    .Where(btl => btl.RecordedAt >= DateTime.UtcNow.AddDays(-14))
                    .Include(btl => btl.BuildJob)
                    .ToListAsync();

                // Machine utilization optimization
                var machineUtilization = recentBuilds
                    .GroupBy(rb => rb.MachineId)
                    .Select(g => new { MachineId = g.Key, AvgHours = (double)g.Average(x => x.ActualHours), Count = g.Count() })
                    .ToList();

                var underutilizedMachines = machineUtilization.Where(mu => mu.Count < 5).ToList();
                foreach (var machine in underutilizedMachines)
                {
                    suggestions.Add(new BuildOptimizationSuggestion
                    {
                        Category = "Machine Utilization",
                        Priority = "Medium",
                        Suggestion = $"Machine {machine.MachineId} is underutilized - consider scheduling more builds",
                        PotentialImpact = "Increased throughput",
                        EstimatedSavings = "15-25% capacity increase"
                    });
                }

                // Quality optimization
                var lowQualityBuilds = recentBuilds.Where(rb => rb.QualityScore < 90).ToList();
                if (lowQualityBuilds.Any())
                {
                    var commonFactors = AnalyzeCommonQualityFactors(lowQualityBuilds);
                    foreach (var factor in commonFactors)
                    {
                        suggestions.Add(new BuildOptimizationSuggestion
                        {
                            Category = "Quality Improvement",
                            Priority = "High",
                            Suggestion = $"Address {factor} to improve quality outcomes",
                            PotentialImpact = "Reduced defects and rework",
                            EstimatedSavings = "10-20% cost reduction"
                        });
                    }
                }

                // Time estimation optimization
                var inaccurateEstimates = recentBuilds.Where(rb => Math.Abs((double)rb.VariancePercent) > 25).ToList();
                if (inaccurateEstimates.Any())
                {
                    suggestions.Add(new BuildOptimizationSuggestion
                    {
                        Category = "Time Estimation",
                        Priority = "Medium",
                        Suggestion = "Improve operator training on time estimation - high variance detected",
                        PotentialImpact = "Better schedule reliability",
                        EstimatedSavings = "5-10% schedule optimization"
                    });
                }

                return suggestions.OrderByDescending(s => GetPriorityWeight(s.Priority)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating optimization suggestions");
                return new List<BuildOptimizationSuggestion>();
            }
        }

        public async Task<List<BuildTimeTrend>> GetBuildTimeTrendsAsync(int daysBack = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-daysBack);
                var trends = await _context.BuildTimeLearningData
                    .Where(btl => btl.RecordedAt >= startDate)
                    .GroupBy(btl => btl.RecordedAt.Date)
                    .Select(g => new BuildTimeTrend
                    {
                        Date = g.Key,
                        AverageHours = g.Average(x => x.ActualHours),
                        AverageAccuracy = g.Average(x => 100 - Math.Abs((double)x.VariancePercent)),
                        BuildCount = g.Count(),
                        QualityScore = g.Average(x => x.QualityScore)
                    })
                    .OrderBy(bt => bt.Date)
                    .ToListAsync();

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading build time trends");
                return new List<BuildTimeTrend>();
            }
        }

        public async Task<List<QualityTrend>> GetQualityTrendsAsync(int daysBack = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-30);
                var trends = await _context.PartCompletionLogs
                    .Where(pcl => pcl.CompletedAt >= startDate)
                    .GroupBy(pcl => pcl.CompletedAt.Date)
                    .Select(g => new QualityTrend
                    {
                        Date = g.Key,
                        QualityRate = g.Average(x => x.QualityRate),
                        DefectRate = g.Average(x => (decimal)x.DefectiveParts / x.Quantity * 100),
                        ReworkRate = g.Average(x => (decimal)x.ReworkParts / x.Quantity * 100),
                        PartsCompleted = g.Sum(x => x.Quantity)
                    })
                    .OrderBy(qt => qt.Date)
                    .ToListAsync();

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading quality trends");
                return new List<QualityTrend>();
            }
        }

        public async Task<List<MachineUtilizationTrend>> GetMachineUtilizationTrendsAsync()
        {
            try
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var utilizationData = await _context.BuildTimeLearningData
                    .Where(btl => btl.RecordedAt >= thirtyDaysAgo)
                    .GroupBy(btl => new { btl.MachineId, Date = btl.RecordedAt.Date })
                    .Select(g => new MachineUtilizationTrend
                    {
                        MachineId = g.Key.MachineId,
                        Date = g.Key.Date,
                        HoursUsed = g.Sum(x => x.ActualHours),
                        BuildCount = g.Count(),
                        UtilizationRate = (double)g.Sum(x => x.ActualHours) / 24 * 100 // Assuming 24-hour availability
                    })
                    .OrderBy(mut => mut.Date)
                    .ThenBy(mut => mut.MachineId)
                    .ToListAsync();

                return utilizationData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading machine utilization trends");
                return new List<MachineUtilizationTrend>();
            }
        }

        public async Task<LearningModelInsights> GetLearningModelInsightsAsync()
        {
            try
            {
                var totalBuilds = await _context.BuildTimeLearningData.CountAsync();
                var recentBuilds = await _context.BuildTimeLearningData
                    .Where(btl => btl.RecordedAt >= DateTime.UtcNow.AddDays(-30))
                    .CountAsync();

                var averageAccuracy = await _context.BuildTimeLearningData
                    .AverageAsync(btl => 100 - Math.Abs((double)btl.VariancePercent));

                return new LearningModelInsights
                {
                    TotalDataPoints = totalBuilds,
                    RecentDataPoints = recentBuilds,
                    ModelAccuracy = (decimal)averageAccuracy,
                    LastUpdated = DateTime.UtcNow,
                    DataQuality = GetOverallDataQuality(totalBuilds, averageAccuracy),
                    RecommendedActions = GenerateModelRecommendations(totalBuilds, averageAccuracy)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading learning model insights");
                return new LearningModelInsights
                {
                    TotalDataPoints = 0,
                    ModelAccuracy = 0,
                    DataQuality = "Error",
                    RecommendedActions = new List<string> { "Unable to load model insights" }
                };
            }
        }

        public async Task<bool> UpdateLearningModelAsync()
        {
            try
            {
                _logger.LogInformation("Updating learning model with latest data");
                
                // This would typically involve more sophisticated ML model training
                // For now, we'll just log that the model is being updated
                
                var recentData = await _context.BuildTimeLearningData
                    .Where(btl => btl.RecordedAt >= DateTime.UtcNow.AddDays(-7))
                    .CountAsync();

                if (recentData < 5)
                {
                    _logger.LogWarning("Insufficient recent data for model update: {DataPoints} data points", recentData);
                    return false;
                }

                // Placeholder for actual ML model update logic
                await Task.Delay(1000); // Simulate processing time

                _logger.LogInformation("Learning model updated successfully with {DataPoints} recent data points", recentData);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating learning model");
                return false;
            }
        }

        #region Private Helper Methods

        private decimal CalculateAverageAccuracy(List<BuildTimeLearningData> learningData)
        {
            if (!learningData.Any()) return 0;
            return (decimal)learningData.Average(ld => 100 - Math.Abs((double)ld.VariancePercent));
        }

        private List<MachinePerformanceData> CalculateMachinePerformance(List<BuildTimeLearningData> learningData)
        {
            return learningData
                .GroupBy(ld => ld.MachineId)
                .Select(g => new MachinePerformanceData
                {
                    MachineId = g.Key,
                    TotalBuilds = g.Count(),
                    AverageAccuracy = g.Average(x => 100 - Math.Abs((double)x.VariancePercent)),
                    AverageQualityScore = (double)g.Average(x => x.QualityScore),
                    AverageDefectRate = (double)g.Average(x => x.DefectCount),
                    AverageHours = (double)g.Average(x => x.ActualHours)
                })
                .ToList();
        }

        private List<QualityMetrics> CalculateQualityTrends(List<PartCompletionLog> partCompletions)
        {
            return partCompletions
                .GroupBy(pc => pc.CompletedAt.Date)
                .Select(g => new QualityMetrics
                {
                    Date = g.Key,
                    QualityRate = g.Average(x => x.QualityRate),
                    DefectCount = g.Sum(x => x.DefectiveParts),
                    TotalParts = g.Sum(x => x.Quantity)
                })
                .OrderBy(qm => qm.Date)
                .ToList();
        }

        private List<OperatorPerformanceData> CalculateOperatorPerformance(
            List<OperatorEstimateLog> estimates, 
            List<BuildTimeLearningData> learningData)
        {
            var operatorData = estimates
                .Join(learningData, e => e.BuildJobId, ld => ld.BuildJobId, (e, ld) => new { Estimate = e, Learning = ld })
                .Where(x => x.Estimate.BuildJob.User != null)
                .GroupBy(x => x.Estimate.BuildJob.User!.Username ?? "Unknown")
                .Select(g => new OperatorPerformanceData
                {
                    OperatorName = g.Key,
                    TotalBuilds = g.Count(),
                    AverageAccuracy = g.Average(x => 100 - Math.Abs((double)x.Learning.VariancePercent)),
                    AverageQualityScore = (double)g.Average(x => x.Learning.QualityScore)
                })
                .ToList();

            return operatorData;
        }

        private double CalculateUtilizationRate(string machineId, DateTime startDate)
        {
            // Simplified calculation - would need more sophisticated logic in production
            var totalHours = _context.BuildTimeLearningData
                .Where(btl => btl.MachineId == machineId && btl.RecordedAt >= startDate)
                .Sum(btl => btl.ActualHours);

            var availableHours = (DateTime.UtcNow - startDate).TotalHours;
            return availableHours > 0 ? (double)totalHours / availableHours * 100 : 0;
        }

        private double CalculateSimilarity(BuildTimeLearningData data, string? buildFileHash)
        {
            // Simple similarity calculation - could be enhanced with more sophisticated algorithms
            if (buildFileHash != null && data.BuildFileHash == buildFileHash)
                return 1.0; // Perfect match
            
            return 0.7; // Default similarity for same machine type
        }

        private string GetDataQualityAssessment(int dataPoints, double variance)
        {
            if (dataPoints < 5) return "Poor";
            if (dataPoints < 10) return "Fair";
            if (variance > 30) return "Fair";
            if (dataPoints >= 20 && variance < 15) return "Excellent";
            return "Good";
        }

        private async Task<string> GetBestMachineForBuild(string partNumber)
        {
            try
            {
                // Check for part-specific machine preferences
                var partMachinePreference = await _context.Jobs
                    .Where(j => j.PartNumber == partNumber)
                    .GroupBy(j => j.MachineId)
                    .Select(g => new { MachineId = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefaultAsync();

                return partMachinePreference?.MachineId ?? "TI1"; // Default fallback
            }
            catch
            {
                return "TI1"; // Safe default
            }
        }

        private async Task<List<BuildOptimization>> GenerateBuildOptimizations(List<BuildTimeLearningData> historicalData)
        {
            var optimizations = new List<BuildOptimization>();

            // Analyze patterns in the data
            var avgDefects = (double)historicalData.Average(hd => hd.DefectCount);
            if (avgDefects > 1)
            {
                optimizations.Add(new BuildOptimization
                {
                    Type = "Quality",
                    Suggestion = "Consider adjusting support parameters to reduce defects",
                    EstimatedImpact = "Reduce defects by 20-30%"
                });
            }

            var avgVariance = historicalData.Average(hd => Math.Abs((double)hd.VariancePercent));
            if (avgVariance > 20)
            {
                optimizations.Add(new BuildOptimization
                {
                    Type = "Time",
                    Suggestion = "Time estimates could be improved - consider additional factors",
                    EstimatedImpact = "Improve estimate accuracy by 15-25%"
                });
            }

            return optimizations;
        }

        private string CalculateAccuracyTrend(List<OperatorAccuracyData> accuracyData)
        {
            if (accuracyData.Count < 5) return "Insufficient Data";

            var recent = accuracyData.TakeLast(5).Average(ad => ad.Accuracy);
            var earlier = accuracyData.Take(5).Average(ad => ad.Accuracy);

            if (recent > earlier + 5) return "Improving";
            if (recent < earlier - 5) return "Declining";
            return "Stable";
        }

        private List<string> GenerateOperatorImprovementSuggestions(List<OperatorAccuracyData> accuracyData)
        {
            var suggestions = new List<string>();

            var avgAccuracy = accuracyData.Average(ad => ad.Accuracy);
            if (avgAccuracy < 80)
            {
                suggestions.Add("Focus on time estimation training - accuracy below target");
            }

            var variance = accuracyData.Select(ad => ad.Accuracy).Variance();
            if (variance > 400) // High variance
            {
                suggestions.Add("Work on consistency - high variation in estimates");
            }

            if (!suggestions.Any())
            {
                suggestions.Add("Performance is good - continue current practices");
            }

            return suggestions;
        }

        private List<QualityRecommendation> GenerateQualityRecommendations(double averageQuality, double averageDefects, BuildParameters parameters)
        {
            var recommendations = new List<QualityRecommendation>();

            if (averageQuality < 90)
            {
                recommendations.Add(new QualityRecommendation
                {
                    Priority = "High",
                    Suggestion = "Review build parameters - quality below target"
                });
            }

            if (averageDefects > 2)
            {
                recommendations.Add(new QualityRecommendation
                {
                    Priority = "High",
                    Suggestion = "Investigate common defect causes for similar builds"
                });
            }

            if (parameters.SupportComplexity == "High")
            {
                recommendations.Add(new QualityRecommendation
                {
                    Priority = "Medium",
                    Suggestion = "Extra attention needed for high-complexity support structures"
                });
            }

            return recommendations;
        }

        private int CalculateQualityConfidence(int dataPoints)
        {
            if (dataPoints < 5) return 30;
            if (dataPoints < 10) return 60;
            if (dataPoints < 20) return 80;
            return 95;
        }

        private List<string> AnalyzeCommonQualityFactors(List<BuildTimeLearningData> lowQualityBuilds)
        {
            var factors = new List<string>();

            var commonSupport = lowQualityBuilds.GroupBy(lqb => lqb.SupportComplexity)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            if (commonSupport != null && commonSupport.Count() > lowQualityBuilds.Count * 0.6)
            {
                factors.Add($"{commonSupport.Key} support complexity");
            }

            var commonMachine = lowQualityBuilds.GroupBy(lqb => lqb.MachineId)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            if (commonMachine != null && commonMachine.Count() > lowQualityBuilds.Count * 0.6)
            {
                factors.Add($"Machine {commonMachine.Key} performance");
            }

            return factors;
        }

        private int GetPriorityWeight(string priority)
        {
            return priority switch
            {
                "High" => 3,
                "Medium" => 2,
                "Low" => 1,
                _ => 0
            };
        }

        private string GetOverallDataQuality(int totalBuilds, double averageAccuracy)
        {
            if (totalBuilds < 50) return "Building Dataset";
            if (totalBuilds < 100) return "Fair";
            if (averageAccuracy > 85) return "Excellent";
            if (averageAccuracy > 75) return "Good";
            return "Needs Improvement";
        }

        private List<string> GenerateModelRecommendations(int totalBuilds, double averageAccuracy)
        {
            var recommendations = new List<string>();

            if (totalBuilds < 50)
                recommendations.Add("Collect more build data to improve model accuracy");

            if (averageAccuracy < 75)
                recommendations.Add("Review data quality and operator training");

            if (totalBuilds >= 100 && averageAccuracy > 85)
                recommendations.Add("Model is performing well - continue current data collection");

            return recommendations;
        }

        private async Task<List<BuildOptimizationSuggestion>> GenerateOptimizationSuggestions(List<BuildTimeLearningData> learningData)
        {
            // This would normally call the full optimization method, but for brevity in analytics
            // we'll just return a summary
            var suggestions = new List<BuildOptimizationSuggestion>();

            if (learningData.Any(ld => Math.Abs((double)ld.VariancePercent) > 30))
            {
                suggestions.Add(new BuildOptimizationSuggestion
                {
                    Category = "Time Estimation",
                    Priority = "Medium",
                    Suggestion = "High variance in time estimates detected",
                    PotentialImpact = "Improved schedule reliability"
                });
            }

            return suggestions;
        }

        private List<ViewModels.Analytics.BuildPerformanceData> GetTopPerformingBuilds(List<BuildTimeLearningData> learningData)
        {
            return learningData
                .Where(ld => ld.QualityScore > 95 && Math.Abs((double)ld.VariancePercent) < 10)
                .OrderByDescending(ld => ld.QualityScore)
                .Take(5)
                .Select(ld => new ViewModels.Analytics.BuildPerformanceData
                {
                    BuildJobId = ld.BuildJobId,
                    MachineId = ld.MachineId,
                    QualityScore = ld.QualityScore,
                    AccuracyScore = 100 - Math.Abs((double)ld.VariancePercent),
                    Date = ld.RecordedAt
                })
                .ToList();
        }

        private List<ViewModels.Analytics.BuildPerformanceData> GetProblemBuilds(List<BuildTimeLearningData> learningData)
        {
            return learningData
                .Where(ld => ld.QualityScore < 80 || Math.Abs((double)ld.VariancePercent) > 50)
                .OrderBy(ld => ld.QualityScore)
                .Take(5)
                .Select(ld => new ViewModels.Analytics.BuildPerformanceData
                {
                    BuildJobId = ld.BuildJobId,
                    MachineId = ld.MachineId,
                    QualityScore = ld.QualityScore,
                    AccuracyScore = 100 - Math.Abs((double)ld.VariancePercent),
                    Date = ld.RecordedAt
                })
                .ToList();
        }

        #endregion
    }
}