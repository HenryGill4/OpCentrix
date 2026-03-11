using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services
{
    public interface IBuildPlanningService
    {
        // Build Package CRUD
        Task<List<BuildPackage>> GetBuildPackagesAsync(string? status = null, CancellationToken ct = default);
        Task<BuildPackage?> GetBuildPackageAsync(int id, CancellationToken ct = default);
        Task<BuildPackage> CreateBuildPackageAsync(BuildPackage package, string createdBy, CancellationToken ct = default);
        Task<BuildPackage> UpdateBuildPackageAsync(BuildPackage package, string modifiedBy, CancellationToken ct = default);
        Task<bool> DeleteBuildPackageAsync(int id, CancellationToken ct = default);

        // Part Management
        Task<BuildPackagePart> AddPartToPackageAsync(int packageId, int partId, int quantity, string? orientation = null, CancellationToken ct = default);
        Task<bool> RemovePartFromPackageAsync(int packagePartId, CancellationToken ct = default);
        Task<bool> UpdatePartQuantityAsync(int packagePartId, int quantity, CancellationToken ct = default);

        // Build File Management
        Task<bool> AssignBuildFileAsync(int packageId, string fileName, string filePath, string? fileHash = null, CancellationToken ct = default);
        Task<bool> RemoveBuildFileAsync(int packageId, CancellationToken ct = default);

        // Status Management
        Task<bool> MarkAsReadyAsync(int packageId, string modifiedBy, CancellationToken ct = default);
        Task<int?> ScheduleBuildAsync(int packageId, string machineId, DateTime startDate, string createdBy, CancellationToken ct = default);

        // Analytics
        Task<BuildPackageStatistics> GetStatisticsAsync(CancellationToken ct = default);
        Task<List<BuildPackage>> GetRecentPackagesAsync(int count = 10, CancellationToken ct = default);
    }

    public class BuildPlanningService : IBuildPlanningService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<BuildPlanningService> _logger;

        public BuildPlanningService(SchedulerContext context, ILogger<BuildPlanningService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<BuildPackage>> GetBuildPackagesAsync(string? status = null, CancellationToken ct = default)
        {
            var query = _context.BuildPackages
                .Include(bp => bp.Parts)
                    .ThenInclude(p => p.Part)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(bp => bp.Status == status);
            }

            return await query
                .OrderByDescending(bp => bp.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<BuildPackage?> GetBuildPackageAsync(int id, CancellationToken ct = default)
        {
            return await _context.BuildPackages
                .Include(bp => bp.Parts)
                    .ThenInclude(p => p.Part)
                .Include(bp => bp.ScheduledJob)
                .FirstOrDefaultAsync(bp => bp.Id == id, ct);
        }

        public async Task<BuildPackage> CreateBuildPackageAsync(BuildPackage package, string createdBy, CancellationToken ct = default)
        {
            // Generate package number
            var count = await _context.BuildPackages.CountAsync(ct) + 1;
            package.PackageNumber = $"BP-{DateTime.UtcNow:yyyy}-{count:D4}";
            package.CreatedBy = createdBy;
            package.CreatedAt = DateTime.UtcNow;
            package.Status = "Draft";

            _context.BuildPackages.Add(package);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Created build package {PackageNumber}", package.PackageNumber);
            return package;
        }

        public async Task<BuildPackage> UpdateBuildPackageAsync(BuildPackage package, string modifiedBy, CancellationToken ct = default)
        {
            package.LastModifiedAt = DateTime.UtcNow;
            package.LastModifiedBy = modifiedBy;

            _context.BuildPackages.Update(package);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Updated build package {PackageNumber}", package.PackageNumber);
            return package;
        }

        public async Task<bool> DeleteBuildPackageAsync(int id, CancellationToken ct = default)
        {
            var package = await _context.BuildPackages.FindAsync(new object[] { id }, ct);
            if (package == null) return false;

            if (package.Status == "InProgress" || package.Status == "Completed")
            {
                _logger.LogWarning("Cannot delete build package {PackageNumber} with status {Status}", 
                    package.PackageNumber, package.Status);
                return false;
            }

            _context.BuildPackages.Remove(package);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Deleted build package {PackageNumber}", package.PackageNumber);
            return true;
        }

        public async Task<BuildPackagePart> AddPartToPackageAsync(int packageId, int partId, int quantity, string? orientation = null, CancellationToken ct = default)
        {
            var package = await _context.BuildPackages
                .Include(bp => bp.Parts)
                .FirstOrDefaultAsync(bp => bp.Id == packageId, ct);

            if (package == null)
                throw new InvalidOperationException($"Build package {packageId} not found");

            var part = await _context.Parts.FindAsync(new object[] { partId }, ct);
            if (part == null)
                throw new InvalidOperationException($"Part {partId} not found");

            // Check if part already exists in package
            var existingPart = package.Parts.FirstOrDefault(p => p.PartId == partId);
            if (existingPart != null)
            {
                existingPart.Quantity += quantity;
                await _context.SaveChangesAsync(ct);
                return existingPart;
            }

            var packagePart = new BuildPackagePart
            {
                BuildPackageId = packageId,
                PartId = partId,
                Quantity = quantity,
                Orientation = orientation ?? "Flat",
                EstimatedHours = part.EstimatedHours * quantity,
                PositionIndex = package.Parts.Count + 1
            };

            _context.BuildPackageParts.Add(packagePart);

            // Update package totals
            package.EstimatedBuildHours = package.Parts.Sum(p => p.EstimatedHours) + packagePart.EstimatedHours;
            
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Added part {PartNumber} x{Quantity} to package {PackageNumber}", 
                part.PartNumber, quantity, package.PackageNumber);

            return packagePart;
        }

        public async Task<bool> RemovePartFromPackageAsync(int packagePartId, CancellationToken ct = default)
        {
            var packagePart = await _context.BuildPackageParts
                .Include(pp => pp.BuildPackage)
                .FirstOrDefaultAsync(pp => pp.Id == packagePartId, ct);

            if (packagePart == null) return false;

            _context.BuildPackageParts.Remove(packagePart);

            // Update package totals
            var package = packagePart.BuildPackage;
            package.EstimatedBuildHours = package.Parts.Where(p => p.Id != packagePartId).Sum(p => p.EstimatedHours);

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> UpdatePartQuantityAsync(int packagePartId, int quantity, CancellationToken ct = default)
        {
            var packagePart = await _context.BuildPackageParts
                .Include(pp => pp.Part)
                .Include(pp => pp.BuildPackage)
                .FirstOrDefaultAsync(pp => pp.Id == packagePartId, ct);

            if (packagePart == null) return false;

            packagePart.Quantity = quantity;
            packagePart.EstimatedHours = (packagePart.Part?.EstimatedHours ?? 1) * quantity;

            // Update package totals
            var package = packagePart.BuildPackage;
            package.EstimatedBuildHours = package.Parts.Sum(p => p.EstimatedHours);

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> AssignBuildFileAsync(int packageId, string fileName, string filePath, string? fileHash = null, CancellationToken ct = default)
        {
            var package = await _context.BuildPackages.FindAsync(new object[] { packageId }, ct);
            if (package == null) return false;

            package.BuildFileName = fileName;
            package.BuildFilePath = filePath;
            package.BuildFileHash = fileHash;
            package.BuildFileUploadedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Assigned build file {FileName} to package {PackageNumber}", 
                fileName, package.PackageNumber);
            return true;
        }

        public async Task<bool> RemoveBuildFileAsync(int packageId, CancellationToken ct = default)
        {
            var package = await _context.BuildPackages.FindAsync(new object[] { packageId }, ct);
            if (package == null) return false;

            package.BuildFileName = null;
            package.BuildFilePath = null;
            package.BuildFileHash = null;
            package.BuildFileSizeBytes = null;
            package.BuildFileUploadedAt = null;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> MarkAsReadyAsync(int packageId, string modifiedBy, CancellationToken ct = default)
        {
            var package = await _context.BuildPackages
                .Include(bp => bp.Parts)
                .FirstOrDefaultAsync(bp => bp.Id == packageId, ct);

            if (package == null) return false;

            if (!package.Parts.Any())
            {
                _logger.LogWarning("Cannot mark package {PackageNumber} as ready - no parts", package.PackageNumber);
                return false;
            }

            if (string.IsNullOrEmpty(package.BuildFileName))
            {
                _logger.LogWarning("Cannot mark package {PackageNumber} as ready - no build file", package.PackageNumber);
                return false;
            }

            package.Status = "Ready";
            package.LastModifiedAt = DateTime.UtcNow;
            package.LastModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Marked package {PackageNumber} as ready", package.PackageNumber);
            return true;
        }

        public async Task<int?> ScheduleBuildAsync(int packageId, string machineId, DateTime startDate, string createdBy, CancellationToken ct = default)
        {
            var package = await _context.BuildPackages
                .Include(bp => bp.Parts)
                    .ThenInclude(p => p.Part)
                .FirstOrDefaultAsync(bp => bp.Id == packageId, ct);

            if (package == null || package.Status != "Ready")
            {
                _logger.LogWarning("Cannot schedule package {PackageId} - not found or not ready", packageId);
                return null;
            }

            // Create a Job from the build package
            var primaryPart = package.Parts.FirstOrDefault()?.Part;
            if (primaryPart == null)
            {
                _logger.LogWarning("Cannot schedule package {PackageNumber} - no parts", package.PackageNumber);
                return null;
            }

            var job = new Job
            {
                MachineId = machineId,
                PartId = primaryPart.Id,
                PartNumber = primaryPart.PartNumber,
                Quantity = package.TotalPartCount,
                ScheduledStart = startDate,
                ScheduledEnd = startDate.AddHours(package.EstimatedBuildHours),
                EstimatedHours = package.EstimatedBuildHours,
                SlsMaterial = package.Material,
                BuildFileName = package.BuildFileName,
                BuildFilePath = package.BuildFilePath,
                Status = "Scheduled",
                Priority = package.Priority,
                IsRushJob = package.IsRushJob,
                CustomerDueDate = package.DueDate,
                Notes = $"Created from Build Package: {package.PackageNumber}",
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                LastModifiedBy = createdBy,
                LastModifiedDate = DateTime.UtcNow
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync(ct);

            // Update package with job reference
            package.ScheduledJobId = job.Id;
            package.TargetMachineId = machineId;
            package.Status = "Scheduled";
            package.LastModifiedAt = DateTime.UtcNow;
            package.LastModifiedBy = createdBy;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Scheduled package {PackageNumber} as job {JobId} on machine {MachineId}", 
                package.PackageNumber, job.Id, machineId);

            return job.Id;
        }

        public async Task<BuildPackageStatistics> GetStatisticsAsync(CancellationToken ct = default)
        {
            var packages = await _context.BuildPackages.ToListAsync(ct);

            return new BuildPackageStatistics
            {
                TotalPackages = packages.Count,
                DraftCount = packages.Count(p => p.Status == "Draft"),
                ReadyCount = packages.Count(p => p.Status == "Ready"),
                ScheduledCount = packages.Count(p => p.Status == "Scheduled"),
                InProgressCount = packages.Count(p => p.Status == "InProgress"),
                CompletedCount = packages.Count(p => p.Status == "Completed"),
                TotalEstimatedHours = packages.Sum(p => p.EstimatedBuildHours),
                AveragePartsPerBuild = packages.Any() ? packages.Average(p => p.TotalPartCount) : 0
            };
        }

        public async Task<List<BuildPackage>> GetRecentPackagesAsync(int count = 10, CancellationToken ct = default)
        {
            return await _context.BuildPackages
                .Include(bp => bp.Parts)
                .OrderByDescending(bp => bp.CreatedAt)
                .Take(count)
                .ToListAsync(ct);
        }
    }

    public class BuildPackageStatistics
    {
        public int TotalPackages { get; set; }
        public int DraftCount { get; set; }
        public int ReadyCount { get; set; }
        public int ScheduledCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public double TotalEstimatedHours { get; set; }
        public double AveragePartsPerBuild { get; set; }
    }
}
