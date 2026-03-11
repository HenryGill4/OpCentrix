using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services
{
    public interface IQCInspectionService
    {
        // Inspection CRUD
        Task<List<QCInspection>> GetInspectionsAsync(string? status = null, string? type = null, CancellationToken ct = default);
        Task<QCInspection?> GetInspectionAsync(int id, CancellationToken ct = default);
        Task<QCInspection> CreateInspectionAsync(QCInspection inspection, string createdBy, CancellationToken ct = default);
        Task<QCInspection> UpdateInspectionAsync(QCInspection inspection, string modifiedBy, CancellationToken ct = default);

        // Inspection Workflow
        Task<bool> StartInspectionAsync(int inspectionId, int inspectorUserId, string inspectorName, CancellationToken ct = default);
        Task<bool> RecordResultAsync(int inspectionId, int passed, int failed, int rework, int scrapped, string? notes = null, CancellationToken ct = default);
        Task<bool> CompleteInspectionAsync(int inspectionId, string modifiedBy, CancellationToken ct = default);

        // Checklist Management
        Task<QCChecklistItem> AddChecklistItemAsync(int inspectionId, QCChecklistItem item, CancellationToken ct = default);
        Task<bool> UpdateChecklistItemResultAsync(int itemId, string result, string? actualValue = null, string? notes = null, CancellationToken ct = default);

        // Auto-create inspections
        Task<QCInspection?> CreatePostPrintInspectionAsync(int buildJobId, string createdBy, CancellationToken ct = default);
        Task<QCInspection?> CreateStageInspectionAsync(int jobId, string inspectionType, string createdBy, CancellationToken ct = default);

        // Analytics
        Task<QCStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
        Task<List<QCInspection>> GetPendingInspectionsAsync(CancellationToken ct = default);
        Task<List<QCInspection>> GetRecentInspectionsAsync(int count = 20, CancellationToken ct = default);
    }

    public class QCInspectionService : IQCInspectionService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<QCInspectionService> _logger;

        public QCInspectionService(SchedulerContext context, ILogger<QCInspectionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<QCInspection>> GetInspectionsAsync(string? status = null, string? type = null, CancellationToken ct = default)
        {
            var query = _context.QCInspections
                .Include(i => i.Part)
                .Include(i => i.Job)
                .Include(i => i.ChecklistItems)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(i => i.Status == status);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(i => i.InspectionType == type);

            return await query
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<QCInspection?> GetInspectionAsync(int id, CancellationToken ct = default)
        {
            return await _context.QCInspections
                .Include(i => i.Part)
                .Include(i => i.Job)
                .Include(i => i.BuildJob)
                .Include(i => i.ChecklistItems.OrderBy(c => c.SortOrder))
                .FirstOrDefaultAsync(i => i.Id == id, ct);
        }

        public async Task<QCInspection> CreateInspectionAsync(QCInspection inspection, string createdBy, CancellationToken ct = default)
        {
            // Generate inspection number
            var count = await _context.QCInspections.CountAsync(ct) + 1;
            inspection.InspectionNumber = $"QC-{DateTime.UtcNow:yyyy}-{count:D4}";
            inspection.CreatedBy = createdBy;
            inspection.CreatedAt = DateTime.UtcNow;
            inspection.Status = "Pending";

            _context.QCInspections.Add(inspection);
            await _context.SaveChangesAsync(ct);

            // Add default checklist items based on inspection type
            await AddDefaultChecklistItemsAsync(inspection, ct);

            _logger.LogInformation("Created QC inspection {InspectionNumber} for part {PartId}", 
                inspection.InspectionNumber, inspection.PartId);

            return inspection;
        }

        public async Task<QCInspection> UpdateInspectionAsync(QCInspection inspection, string modifiedBy, CancellationToken ct = default)
        {
            inspection.LastModifiedAt = DateTime.UtcNow;
            inspection.LastModifiedBy = modifiedBy;

            _context.QCInspections.Update(inspection);
            await _context.SaveChangesAsync(ct);

            return inspection;
        }

        public async Task<bool> StartInspectionAsync(int inspectionId, int inspectorUserId, string inspectorName, CancellationToken ct = default)
        {
            var inspection = await _context.QCInspections.FindAsync(new object[] { inspectionId }, ct);
            if (inspection == null) return false;

            inspection.Status = "InProgress";
            inspection.InspectorUserId = inspectorUserId;
            inspection.InspectorName = inspectorName;
            inspection.StartedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Started inspection {InspectionNumber} by {Inspector}", 
                inspection.InspectionNumber, inspectorName);
            return true;
        }

        public async Task<bool> RecordResultAsync(int inspectionId, int passed, int failed, int rework, int scrapped, string? notes = null, CancellationToken ct = default)
        {
            var inspection = await _context.QCInspections.FindAsync(new object[] { inspectionId }, ct);
            if (inspection == null) return false;

            inspection.PassedQuantity = passed;
            inspection.FailedQuantity = failed;
            inspection.ReworkQuantity = rework;
            inspection.ScrappedQuantity = scrapped;

            if (!string.IsNullOrEmpty(notes))
            {
                inspection.Notes = string.IsNullOrEmpty(inspection.Notes) 
                    ? notes 
                    : $"{inspection.Notes}\n{notes}";
            }

            // Calculate quality score
            var total = passed + failed + rework + scrapped;
            if (total > 0)
            {
                inspection.QualityScore = Math.Round((double)passed / total * 100, 2);
            }

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> CompleteInspectionAsync(int inspectionId, string modifiedBy, CancellationToken ct = default)
        {
            var inspection = await _context.QCInspections
                .Include(i => i.ChecklistItems)
                .FirstOrDefaultAsync(i => i.Id == inspectionId, ct);

            if (inspection == null) return false;

            // Determine final status based on results
            var hasCriticalFail = inspection.ChecklistItems
                .Any(c => c.IsCritical && c.Result == "Fail");

            if (hasCriticalFail || inspection.FailedQuantity > 0)
            {
                inspection.Status = "Failed";
            }
            else if (inspection.ReworkQuantity > 0)
            {
                inspection.Status = "ConditionalPass";
            }
            else
            {
                inspection.Status = "Passed";
            }

            inspection.CompletedAt = DateTime.UtcNow;
            inspection.LastModifiedAt = DateTime.UtcNow;
            inspection.LastModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Completed inspection {InspectionNumber} with status {Status}", 
                inspection.InspectionNumber, inspection.Status);
            return true;
        }

        public async Task<QCChecklistItem> AddChecklistItemAsync(int inspectionId, QCChecklistItem item, CancellationToken ct = default)
        {
            item.QCInspectionId = inspectionId;
            
            var maxOrder = await _context.QCChecklistItems
                .Where(c => c.QCInspectionId == inspectionId)
                .MaxAsync(c => (int?)c.SortOrder, ct) ?? 0;
            
            item.SortOrder = maxOrder + 1;

            _context.QCChecklistItems.Add(item);
            await _context.SaveChangesAsync(ct);

            return item;
        }

        public async Task<bool> UpdateChecklistItemResultAsync(int itemId, string result, string? actualValue = null, string? notes = null, CancellationToken ct = default)
        {
            var item = await _context.QCChecklistItems.FindAsync(new object[] { itemId }, ct);
            if (item == null) return false;

            item.Result = result;
            if (actualValue != null) item.ActualValue = actualValue;
            if (notes != null) item.Notes = notes;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<QCInspection?> CreatePostPrintInspectionAsync(int buildJobId, string createdBy, CancellationToken ct = default)
        {
            var buildJob = await _context.BuildJobs
                .Include(b => b.Part)
                .Include(b => b.BuildJobParts)
                .FirstOrDefaultAsync(b => b.BuildId == buildJobId, ct);

            if (buildJob == null) return null;

            var primaryPart = buildJob.BuildJobParts.FirstOrDefault(p => p.IsPrimary);
            var partId = buildJob.PartId ?? primaryPart?.BuildJob?.PartId;

            if (!partId.HasValue)
            {
                _logger.LogWarning("Cannot create post-print inspection for build {BuildId} - no part found", buildJobId);
                return null;
            }

            var inspection = new QCInspection
            {
                BuildJobId = buildJobId,
                PartId = partId.Value,
                InspectionType = "PostPrint",
                TotalQuantity = buildJob.TotalPartsProduced
            };

            return await CreateInspectionAsync(inspection, createdBy, ct);
        }

        public async Task<QCInspection?> CreateStageInspectionAsync(int jobId, string inspectionType, string createdBy, CancellationToken ct = default)
        {
            var job = await _context.Jobs.FindAsync(new object[] { jobId }, ct);
            if (job == null) return null;

            var inspection = new QCInspection
            {
                JobId = jobId,
                PartId = job.PartId,
                InspectionType = inspectionType,
                TotalQuantity = job.Quantity
            };

            return await CreateInspectionAsync(inspection, createdBy, ct);
        }

        public async Task<QCStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
        {
            var query = _context.QCInspections.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(i => i.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.CreatedAt <= toDate.Value);

            var inspections = await query.ToListAsync(ct);

            return new QCStatistics
            {
                TotalInspections = inspections.Count,
                PendingCount = inspections.Count(i => i.Status == "Pending"),
                InProgressCount = inspections.Count(i => i.Status == "InProgress"),
                PassedCount = inspections.Count(i => i.Status == "Passed"),
                FailedCount = inspections.Count(i => i.Status == "Failed"),
                ConditionalPassCount = inspections.Count(i => i.Status == "ConditionalPass"),
                TotalPartsInspected = inspections.Sum(i => i.InspectedQuantity),
                TotalPassedParts = inspections.Sum(i => i.PassedQuantity),
                TotalFailedParts = inspections.Sum(i => i.FailedQuantity),
                TotalReworkParts = inspections.Sum(i => i.ReworkQuantity),
                TotalScrappedParts = inspections.Sum(i => i.ScrappedQuantity),
                AverageQualityScore = inspections.Any(i => i.QualityScore.HasValue) 
                    ? inspections.Where(i => i.QualityScore.HasValue).Average(i => i.QualityScore!.Value) 
                    : 0,
                FirstPassYield = inspections.Sum(i => i.TotalQuantity) > 0
                    ? Math.Round((double)inspections.Sum(i => i.PassedQuantity) / inspections.Sum(i => i.TotalQuantity) * 100, 2)
                    : 100
            };
        }

        public async Task<List<QCInspection>> GetPendingInspectionsAsync(CancellationToken ct = default)
        {
            return await _context.QCInspections
                .Include(i => i.Part)
                .Where(i => i.Status == "Pending" || i.Status == "InProgress")
                .OrderBy(i => i.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<List<QCInspection>> GetRecentInspectionsAsync(int count = 20, CancellationToken ct = default)
        {
            return await _context.QCInspections
                .Include(i => i.Part)
                .OrderByDescending(i => i.CreatedAt)
                .Take(count)
                .ToListAsync(ct);
        }

        private async Task AddDefaultChecklistItemsAsync(QCInspection inspection, CancellationToken ct)
        {
            var items = inspection.InspectionType switch
            {
                "PostPrint" => GetPostPrintChecklistItems(),
                "PostCNC" => GetPostCNCChecklistItems(),
                "PostEDM" => GetPostEDMChecklistItems(),
                "PostCoating" => GetPostCoatingChecklistItems(),
                "Final" => GetFinalChecklistItems(),
                _ => GetDefaultChecklistItems()
            };

            foreach (var item in items)
            {
                item.QCInspectionId = inspection.Id;
                _context.QCChecklistItems.Add(item);
            }

            await _context.SaveChangesAsync(ct);
        }

        private List<QCChecklistItem> GetPostPrintChecklistItems()
        {
            return new List<QCChecklistItem>
            {
                new() { ItemName = "Visual inspection - no obvious defects", Category = "Visual", IsCritical = true, SortOrder = 1 },
                new() { ItemName = "Build plate separation clean", Category = "Visual", SortOrder = 2 },
                new() { ItemName = "Support structures intact", Category = "Visual", SortOrder = 3 },
                new() { ItemName = "No warping or distortion", Category = "Dimensional", IsCritical = true, SortOrder = 4 },
                new() { ItemName = "Surface finish acceptable", Category = "Surface", SortOrder = 5 },
                new() { ItemName = "Part count matches build file", Category = "Documentation", IsCritical = true, SortOrder = 6 }
            };
        }

        private List<QCChecklistItem> GetPostCNCChecklistItems()
        {
            return new List<QCChecklistItem>
            {
                new() { ItemName = "Critical dimensions within tolerance", Category = "Dimensional", IsCritical = true, SortOrder = 1 },
                new() { ItemName = "Surface finish Ra meets spec", Category = "Surface", SortOrder = 2 },
                new() { ItemName = "No tool marks or gouges", Category = "Visual", SortOrder = 3 },
                new() { ItemName = "Threads/holes properly machined", Category = "Dimensional", SortOrder = 4 },
                new() { ItemName = "Deburring complete", Category = "Visual", SortOrder = 5 }
            };
        }

        private List<QCChecklistItem> GetPostEDMChecklistItems()
        {
            return new List<QCChecklistItem>
            {
                new() { ItemName = "EDM features within tolerance", Category = "Dimensional", IsCritical = true, SortOrder = 1 },
                new() { ItemName = "No recast layer issues", Category = "Material", SortOrder = 2 },
                new() { ItemName = "Surface finish acceptable", Category = "Surface", SortOrder = 3 },
                new() { ItemName = "No wire marks or burns", Category = "Visual", SortOrder = 4 }
            };
        }

        private List<QCChecklistItem> GetPostCoatingChecklistItems()
        {
            return new List<QCChecklistItem>
            {
                new() { ItemName = "Coating coverage complete", Category = "Visual", IsCritical = true, SortOrder = 1 },
                new() { ItemName = "Coating thickness within spec", Category = "Dimensional", SortOrder = 2 },
                new() { ItemName = "No coating defects (bubbles, runs)", Category = "Visual", SortOrder = 3 },
                new() { ItemName = "Adhesion test passed", Category = "Material", IsCritical = true, SortOrder = 4 }
            };
        }

        private List<QCChecklistItem> GetFinalChecklistItems()
        {
            return new List<QCChecklistItem>
            {
                new() { ItemName = "All dimensions verified", Category = "Dimensional", IsCritical = true, SortOrder = 1 },
                new() { ItemName = "Visual inspection passed", Category = "Visual", IsCritical = true, SortOrder = 2 },
                new() { ItemName = "Documentation complete", Category = "Documentation", IsCritical = true, SortOrder = 3 },
                new() { ItemName = "Packaging requirements met", Category = "Documentation", SortOrder = 4 },
                new() { ItemName = "Ready for shipment", Category = "Documentation", SortOrder = 5 }
            };
        }

        private List<QCChecklistItem> GetDefaultChecklistItems()
        {
            return new List<QCChecklistItem>
            {
                new() { ItemName = "Visual inspection", Category = "Visual", SortOrder = 1 },
                new() { ItemName = "Dimensional check", Category = "Dimensional", SortOrder = 2 },
                new() { ItemName = "Documentation review", Category = "Documentation", SortOrder = 3 }
            };
        }
    }

    public class QCStatistics
    {
        public int TotalInspections { get; set; }
        public int PendingCount { get; set; }
        public int InProgressCount { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public int ConditionalPassCount { get; set; }
        public int TotalPartsInspected { get; set; }
        public int TotalPassedParts { get; set; }
        public int TotalFailedParts { get; set; }
        public int TotalReworkParts { get; set; }
        public int TotalScrappedParts { get; set; }
        public double AverageQualityScore { get; set; }
        public double FirstPassYield { get; set; }
    }
}
