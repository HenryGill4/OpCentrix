using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin
{
    /// <summary>
    /// Service for managing B&T Part Classifications
    /// Segment 7.1: B&T-specific part categorization and classification management
    /// </summary>
    public interface IPartClassificationService
    {
        Task<List<PartClassification>> GetAllClassificationsAsync();
        Task<List<PartClassification>> GetActiveClassificationsAsync();
        Task<List<PartClassification>> GetClassificationsByIndustryAsync(string industry);
        Task<List<PartClassification>> GetClassificationsByComponentTypeAsync(string componentType);
        Task<PartClassification?> GetClassificationByIdAsync(int id);
        Task<PartClassification?> GetClassificationByCodeAsync(string code);
        Task<PartClassification> CreateClassificationAsync(PartClassification classification, string createdBy);
        Task<PartClassification> UpdateClassificationAsync(PartClassification classification, string modifiedBy);
        Task<bool> DeleteClassificationAsync(int id, string deletedBy);
        Task<bool> ActivateClassificationAsync(int id, string modifiedBy);
        Task<bool> DeactivateClassificationAsync(int id, string modifiedBy);
        Task<List<PartClassification>> SearchClassificationsAsync(string searchTerm);
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);
        Task<List<string>> GetIndustryTypesAsync();
        Task<List<string>> GetComponentCategoriesAsync();
        Task<List<Part>> GetPartsUsingClassificationAsync(int classificationId);
        Task<int> GetPartsCountForClassificationAsync(int classificationId);
        Task<Dictionary<string, int>> GetClassificationStatisticsAsync();
        Task<List<PartClassification>> GetRecommendedClassificationsForPartAsync(string partType, string industry, string material);
        Task SeedDefaultClassificationsAsync();
    }

    public class PartClassificationService : IPartClassificationService
    {
        private readonly SchedulerContext _context;

        public PartClassificationService(SchedulerContext context)
        {
            _context = context;
        }

        public async Task<List<PartClassification>> GetAllClassificationsAsync()
        {
            return await _context.PartClassifications
                .Include(pc => pc.ComplianceRequirements)
                .OrderBy(pc => pc.IndustryType)
                .ThenBy(pc => pc.ComponentCategory)
                .ThenBy(pc => pc.ClassificationName)
                .ToListAsync();
        }

        public async Task<List<PartClassification>> GetActiveClassificationsAsync()
        {
            return await _context.PartClassifications
                .Where(pc => pc.IsActive)
                .Include(pc => pc.ComplianceRequirements.Where(cr => cr.IsActive))
                .OrderBy(pc => pc.IndustryType)
                .ThenBy(pc => pc.ComponentCategory)
                .ThenBy(pc => pc.ClassificationName)
                .ToListAsync();
        }

        public async Task<List<PartClassification>> GetClassificationsByIndustryAsync(string industry)
        {
            return await _context.PartClassifications
                .Where(pc => pc.IsActive && pc.IndustryType == industry)
                .Include(pc => pc.ComplianceRequirements.Where(cr => cr.IsActive))
                .OrderBy(pc => pc.ComponentCategory)
                .ThenBy(pc => pc.ClassificationName)
                .ToListAsync();
        }

        public async Task<List<PartClassification>> GetClassificationsByComponentTypeAsync(string componentType)
        {
            return await _context.PartClassifications
                .Where(pc => pc.IsActive && pc.ComponentCategory == componentType)
                .Include(pc => pc.ComplianceRequirements.Where(cr => cr.IsActive))
                .OrderBy(pc => pc.IndustryType)
                .ThenBy(pc => pc.ClassificationName)
                .ToListAsync();
        }

        public async Task<PartClassification?> GetClassificationByIdAsync(int id)
        {
            return await _context.PartClassifications
                .Include(pc => pc.ComplianceRequirements)
                .Include(pc => pc.Parts)
                .FirstOrDefaultAsync(pc => pc.Id == id);
        }

        public async Task<PartClassification?> GetClassificationByCodeAsync(string code)
        {
            return await _context.PartClassifications
                .Include(pc => pc.ComplianceRequirements)
                .FirstOrDefaultAsync(pc => pc.ClassificationCode == code);
        }

        public async Task<PartClassification> CreateClassificationAsync(PartClassification classification, string createdBy)
        {
            classification.CreatedBy = createdBy;
            classification.LastModifiedBy = createdBy;
            classification.CreatedDate = DateTime.UtcNow;
            classification.LastModifiedDate = DateTime.UtcNow;

            _context.PartClassifications.Add(classification);
            await _context.SaveChangesAsync();

            return classification;
        }

        public async Task<PartClassification> UpdateClassificationAsync(PartClassification classification, string modifiedBy)
        {
            var existing = await _context.PartClassifications.FindAsync(classification.Id);
            if (existing == null)
                throw new ArgumentException($"Part classification with ID {classification.Id} not found");

            // Update properties
            existing.ClassificationCode = classification.ClassificationCode;
            existing.ClassificationName = classification.ClassificationName;
            existing.Description = classification.Description;
            existing.IndustryType = classification.IndustryType;
            existing.ComponentCategory = classification.ComponentCategory;
            existing.SuppressorType = classification.SuppressorType;
            existing.BafflePosition = classification.BafflePosition;
            existing.FirearmType = classification.FirearmType;
            existing.IsEndCap = classification.IsEndCap;
            existing.IsThreadMount = classification.IsThreadMount;
            existing.IsTubeHousing = classification.IsTubeHousing;
            existing.IsInternalComponent = classification.IsInternalComponent;
            existing.IsMountingHardware = classification.IsMountingHardware;
            existing.IsReceiver = classification.IsReceiver;
            existing.IsBarrelComponent = classification.IsBarrelComponent;
            existing.IsOperatingSystem = classification.IsOperatingSystem;
            existing.IsSafetyComponent = classification.IsSafetyComponent;
            existing.IsTriggerComponent = classification.IsTriggerComponent;
            existing.IsFurniture = classification.IsFurniture;
            existing.RecommendedMaterial = classification.RecommendedMaterial;
            existing.AlternativeMaterials = classification.AlternativeMaterials;
            existing.MaterialGrade = classification.MaterialGrade;
            existing.RequiresSpecialHandling = classification.RequiresSpecialHandling;
            existing.RequiredProcess = classification.RequiredProcess;
            existing.PostProcessingRequired = classification.PostProcessingRequired;
            existing.ComplexityLevel = classification.ComplexityLevel;
            existing.SpecialInstructions = classification.SpecialInstructions;
            existing.RequiresPressureTesting = classification.RequiresPressureTesting;
            existing.RequiresProofTesting = classification.RequiresProofTesting;
            existing.RequiresDimensionalVerification = classification.RequiresDimensionalVerification;
            existing.RequiresSurfaceFinishVerification = classification.RequiresSurfaceFinishVerification;
            existing.RequiresMaterialCertification = classification.RequiresMaterialCertification;
            existing.TestingRequirements = classification.TestingRequirements;
            existing.QualityStandards = classification.QualityStandards;
            existing.RequiresATFCompliance = classification.RequiresATFCompliance;
            existing.RequiresITARCompliance = classification.RequiresITARCompliance;
            existing.RequiresFFLTracking = classification.RequiresFFLTracking;
            existing.RequiresSerialization = classification.RequiresSerialization;
            existing.IsControlledItem = classification.IsControlledItem;
            existing.IsEARControlled = classification.IsEARControlled;
            existing.ExportClassification = classification.ExportClassification;
            existing.RegulatoryNotes = classification.RegulatoryNotes;
            existing.LastModifiedBy = modifiedBy;
            existing.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteClassificationAsync(int id, string deletedBy)
        {
            var classification = await _context.PartClassifications
                .Include(pc => pc.Parts)
                .FirstOrDefaultAsync(pc => pc.Id == id);

            if (classification == null)
                return false;

            // Check if any parts are using this classification
            if (classification.Parts.Any())
            {
                throw new InvalidOperationException($"Cannot delete classification '{classification.ClassificationName}' because it is being used by {classification.Parts.Count} part(s)");
            }

            _context.PartClassifications.Remove(classification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateClassificationAsync(int id, string modifiedBy)
        {
            var classification = await _context.PartClassifications.FindAsync(id);
            if (classification == null)
                return false;

            classification.IsActive = true;
            classification.LastModifiedBy = modifiedBy;
            classification.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateClassificationAsync(int id, string modifiedBy)
        {
            var classification = await _context.PartClassifications.FindAsync(id);
            if (classification == null)
                return false;

            classification.IsActive = false;
            classification.LastModifiedBy = modifiedBy;
            classification.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PartClassification>> SearchClassificationsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveClassificationsAsync();

            var term = searchTerm.ToLower();

            return await _context.PartClassifications
                .Where(pc => pc.IsActive && (
                    pc.ClassificationCode.ToLower().Contains(term) ||
                    pc.ClassificationName.ToLower().Contains(term) ||
                    pc.Description.ToLower().Contains(term) ||
                    pc.IndustryType.ToLower().Contains(term) ||
                    pc.ComponentCategory.ToLower().Contains(term) ||
                    pc.SuppressorType!.ToLower().Contains(term) ||
                    pc.FirearmType!.ToLower().Contains(term)
                ))
                .Include(pc => pc.ComplianceRequirements.Where(cr => cr.IsActive))
                .OrderBy(pc => pc.IndustryType)
                .ThenBy(pc => pc.ClassificationName)
                .ToListAsync();
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            var query = _context.PartClassifications.Where(pc => pc.ClassificationCode == code);
            
            if (excludeId.HasValue)
                query = query.Where(pc => pc.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<List<string>> GetIndustryTypesAsync()
        {
            return await _context.PartClassifications
                .Where(pc => pc.IsActive)
                .Select(pc => pc.IndustryType)
                .Distinct()
                .OrderBy(it => it)
                .ToListAsync();
        }

        public async Task<List<string>> GetComponentCategoriesAsync()
        {
            return await _context.PartClassifications
                .Where(pc => pc.IsActive)
                .Select(pc => pc.ComponentCategory)
                .Distinct()
                .OrderBy(cc => cc)
                .ToListAsync();
        }

        public async Task<List<Part>> GetPartsUsingClassificationAsync(int classificationId)
        {
            return await _context.Parts
                .Where(p => p.PartClassificationId == classificationId)
                .OrderBy(p => p.PartNumber)
                .ToListAsync();
        }

        public async Task<int> GetPartsCountForClassificationAsync(int classificationId)
        {
            return await _context.Parts
                .CountAsync(p => p.PartClassificationId == classificationId);
        }

        public async Task<Dictionary<string, int>> GetClassificationStatisticsAsync()
        {
            var stats = new Dictionary<string, int>();

            // Total classifications
            stats["Total"] = await _context.PartClassifications.CountAsync();
            stats["Active"] = await _context.PartClassifications.CountAsync(pc => pc.IsActive);
            stats["Inactive"] = await _context.PartClassifications.CountAsync(pc => !pc.IsActive);

            // By industry
            stats["Firearms"] = await _context.PartClassifications.CountAsync(pc => pc.IsActive && pc.IndustryType == "Firearms");
            stats["Suppressor"] = await _context.PartClassifications.CountAsync(pc => pc.IsActive && pc.IndustryType == "Suppressor");
            stats["General"] = await _context.PartClassifications.CountAsync(pc => pc.IsActive && pc.IndustryType == "General");

            // Compliance requirements
            stats["ATF Required"] = await _context.PartClassifications.CountAsync(pc => pc.IsActive && pc.RequiresATFCompliance);
            stats["ITAR Required"] = await _context.PartClassifications.CountAsync(pc => pc.IsActive && pc.RequiresITARCompliance);
            stats["Serialization Required"] = await _context.PartClassifications.CountAsync(pc => pc.IsActive && pc.RequiresSerialization);

            // Quality requirements
            stats["Pressure Testing"] = await _context.PartClassifications.CountAsync(pc => pc.IsActive && pc.RequiresPressureTesting);
            stats["Proof Testing"] = await _context.PartClassifications.CountAsync(pc => pc.IsActive && pc.RequiresProofTesting);

            return stats;
        }

        public async Task<List<PartClassification>> GetRecommendedClassificationsForPartAsync(string partType, string industry, string material)
        {
            var query = _context.PartClassifications.Where(pc => pc.IsActive);

            // Match by industry first
            if (!string.IsNullOrEmpty(industry))
            {
                query = query.Where(pc => pc.IndustryType == industry || pc.IndustryType == "General");
            }

            // Match by component category
            if (!string.IsNullOrEmpty(partType))
            {
                query = query.Where(pc => 
                    pc.ComponentCategory.Contains(partType, StringComparison.OrdinalIgnoreCase) ||
                    pc.ClassificationName.Contains(partType, StringComparison.OrdinalIgnoreCase) ||
                    pc.Description.Contains(partType, StringComparison.OrdinalIgnoreCase));
            }

            // Match by recommended material
            if (!string.IsNullOrEmpty(material))
            {
                query = query.Where(pc => 
                    pc.RecommendedMaterial.Contains(material, StringComparison.OrdinalIgnoreCase) ||
                    pc.AlternativeMaterials.Contains(material, StringComparison.OrdinalIgnoreCase));
            }

            return await query
                .Include(pc => pc.ComplianceRequirements.Where(cr => cr.IsActive))
                .OrderBy(pc => pc.ComplexityLevel)
                .ThenBy(pc => pc.ClassificationName)
                .Take(10)
                .ToListAsync();
        }

        /// <summary>
        /// Seed default B&T part classifications
        /// </summary>
        public async Task SeedDefaultClassificationsAsync()
        {
            if (await _context.PartClassifications.AnyAsync())
                return; // Already seeded

            var defaultClassifications = new List<PartClassification>
            {
                // Suppressor Classifications
                new PartClassification
                {
                    ClassificationCode = "SUP-BAFFLE-FRONT",
                    ClassificationName = "Suppressor Front Baffle",
                    Description = "Front baffle component for rifle suppressors",
                    IndustryType = "Suppressor",
                    ComponentCategory = "Baffle",
                    SuppressorType = "Rifle",
                    BafflePosition = "Front",
                    RecommendedMaterial = "Ti-6Al-4V Grade 5",
                    AlternativeMaterials = "Inconel 718, 17-4 PH Stainless Steel",
                    RequiredProcess = "SLS Metal Printing",
                    ComplexityLevel = 6,
                    RequiresPressureTesting = true,
                    RequiresDimensionalVerification = true,
                    RequiresSurfaceFinishVerification = true,
                    RequiresMaterialCertification = true,
                    RequiresATFCompliance = true,
                    RequiresITARCompliance = true,
                    RequiresSerialization = true,
                    TestingRequirements = "Pressure testing to 150% operating pressure, dimensional verification per drawing",
                    QualityStandards = "ASTM F3001, B&T internal standards",
                    CreatedBy = "System",
                    LastModifiedBy = "System"
                },
                new PartClassification
                {
                    ClassificationCode = "SUP-ENDCAP",
                    ClassificationName = "Suppressor End Cap",
                    Description = "Threaded end cap for suppressor mounting",
                    IndustryType = "Suppressor",
                    ComponentCategory = "End Cap",
                    IsEndCap = true,
                    IsThreadMount = true,
                    RecommendedMaterial = "Ti-6Al-4V Grade 5",
                    AlternativeMaterials = "17-4 PH Stainless Steel",
                    RequiredProcess = "SLS Metal Printing + CNC Threading",
                    PostProcessingRequired = "CNC threading, pressure testing",
                    ComplexityLevel = 7,
                    RequiresPressureTesting = true,
                    RequiresProofTesting = true,
                    RequiresDimensionalVerification = true,
                    RequiresATFCompliance = true,
                    RequiresITARCompliance = true,
                    RequiresSerialization = true,
                    TestingRequirements = "Thread verification, pressure testing, proof testing",
                    QualityStandards = "SAAMI specifications, B&T threading standards",
                    CreatedBy = "System",
                    LastModifiedBy = "System"
                },
                new PartClassification
                {
                    ClassificationCode = "SUP-TUBE",
                    ClassificationName = "Suppressor Tube Housing",
                    Description = "Main tube housing for suppressor assembly",
                    IndustryType = "Suppressor",
                    ComponentCategory = "Housing",
                    IsTubeHousing = true,
                    RecommendedMaterial = "Ti-6Al-4V Grade 5",
                    AlternativeMaterials = "Inconel 718",
                    RequiredProcess = "SLS Metal Printing",
                    ComplexityLevel = 5,
                    RequiresPressureTesting = true,
                    RequiresDimensionalVerification = true,
                    RequiresATFCompliance = true,
                    RequiresITARCompliance = true,
                    RequiresSerialization = true,
                    CreatedBy = "System",
                    LastModifiedBy = "System"
                },
                // Firearm Classifications
                new PartClassification
                {
                    ClassificationCode = "FIR-RECEIVER",
                    ClassificationName = "Firearm Receiver",
                    Description = "Main receiver component for firearms",
                    IndustryType = "Firearms",
                    ComponentCategory = "Receiver",
                    FirearmType = "Rifle",
                    IsReceiver = true,
                    RecommendedMaterial = "7075-T6 Aluminum",
                    AlternativeMaterials = "Ti-6Al-4V Grade 5, 17-4 PH Stainless Steel",
                    RequiredProcess = "CNC Machining",
                    ComplexityLevel = 9,
                    RequiresDimensionalVerification = true,
                    RequiresMaterialCertification = true,
                    RequiresATFCompliance = true,
                    RequiresITARCompliance = true,
                    RequiresFFLTracking = true,
                    RequiresSerialization = true,
                    IsControlledItem = true,
                    TestingRequirements = "Dimensional verification, material certification, function testing",
                    QualityStandards = "SAAMI specifications, military standards",
                    CreatedBy = "System",
                    LastModifiedBy = "System"
                },
                new PartClassification
                {
                    ClassificationCode = "FIR-BARREL",
                    ClassificationName = "Firearm Barrel",
                    Description = "Barrel component for firearms",
                    IndustryType = "Firearms",
                    ComponentCategory = "Barrel",
                    FirearmType = "Rifle",
                    IsBarrelComponent = true,
                    RecommendedMaterial = "4150 Chrome Moly Steel",
                    AlternativeMaterials = "416R Stainless Steel",
                    RequiredProcess = "Gun Drilling + CNC Machining",
                    ComplexityLevel = 8,
                    RequiresProofTesting = true,
                    RequiresDimensionalVerification = true,
                    RequiresSurfaceFinishVerification = true,
                    RequiresATFCompliance = true,
                    RequiresITARCompliance = true,
                    TestingRequirements = "Proof testing, chamber verification, bore measurement",
                    QualityStandards = "SAAMI chamber specifications",
                    CreatedBy = "System",
                    LastModifiedBy = "System"
                },
                // General Manufacturing
                new PartClassification
                {
                    ClassificationCode = "GEN-PROTOTYPE",
                    ClassificationName = "General Prototype Component",
                    Description = "General prototype parts for testing and development",
                    IndustryType = "General",
                    ComponentCategory = "Prototype",
                    RecommendedMaterial = "Ti-6Al-4V Grade 5",
                    AlternativeMaterials = "AlSi10Mg, 316L Stainless Steel",
                    RequiredProcess = "SLS Metal Printing",
                    ComplexityLevel = 3,
                    RequiresDimensionalVerification = true,
                    CreatedBy = "System",
                    LastModifiedBy = "System"
                }
            };

            _context.PartClassifications.AddRange(defaultClassifications);
            await _context.SaveChangesAsync();
        }
    }
}