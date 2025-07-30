using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using System.Text.RegularExpressions;

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
        private readonly ILogger<PartClassificationService> _logger;

        public PartClassificationService(SchedulerContext context, ILogger<PartClassificationService> logger)
        {
            _context = context;
            _logger = logger;
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

        #region B&T Classification Methods

        /// <summary>
        /// Automatically classify B&T component based on name and description
        /// </summary>
        public BTComponentClassification ClassifyBTComponent(string name, string description)
        {
            _logger.LogInformation("?? Classifying B&T component: {Name}", name);

            var classification = new BTComponentClassification();
            var nameUpper = name.ToUpperInvariant();
            var descUpper = description.ToUpperInvariant();

            // Suppressor classification
            if (IsSupressorComponent(nameUpper, descUpper))
            {
                classification.ComponentType = "Suppressor";
                classification.FirearmCategory = "NFA_Item";
                classification.RequiresATFForm1 = true;
                classification.RequiresTaxStamp = true;
                classification.TaxStampAmount = 200.00m;
                classification.RequiresSoundTesting = true;
                classification.WorkflowTemplate = "BT_Suppressor_Workflow";

                // Determine suppressor type
                if (nameUpper.Contains("BAFFLE"))
                {
                    classification.SuppressorType = "Baffle";
                    if (nameUpper.Contains("FRONT")) classification.BafflePosition = "Front";
                    else if (nameUpper.Contains("REAR")) classification.BafflePosition = "Rear";
                    else if (nameUpper.Contains("END")) classification.BafflePosition = "End";
                    else classification.BafflePosition = "Middle";
                }
                else if (nameUpper.Contains("END") && nameUpper.Contains("CAP"))
                {
                    classification.SuppressorType = "EndCap";
                }
                else if (nameUpper.Contains("TUBE"))
                {
                    classification.SuppressorType = "Tube";
                }
                else if (nameUpper.Contains("MOUNT"))
                {
                    classification.SuppressorType = "Mount";
                }
                else
                {
                    classification.SuppressorType = "Internal";
                }
            }
            // Receiver classification
            else if (IsReceiverComponent(nameUpper, descUpper))
            {
                classification.ComponentType = "Receiver";
                classification.FirearmCategory = "Firearm";
                classification.RequiresATFCompliance = true;
                classification.RequiresUniqueSerialNumber = true;
                classification.RequiresFFLTracking = true;
                classification.WorkflowTemplate = "BT_Firearm_Workflow";
            }
            // Barrel classification
            else if (IsBarrelComponent(nameUpper, descUpper))
            {
                classification.ComponentType = "Barrel";
                classification.FirearmCategory = "Component";
                classification.RequiresProofTesting = true;
                classification.WorkflowTemplate = "BT_Standard_Workflow";
                
                // Extract thread pitch if mentioned
                classification.ThreadPitch = ExtractThreadPitch(nameUpper + " " + descUpper);
            }
            // Other component types
            else if (IsTriggerComponent(nameUpper, descUpper))
            {
                classification.ComponentType = "Trigger";
                classification.FirearmCategory = "Component";
                classification.WorkflowTemplate = "BT_Standard_Workflow";
            }
            else if (IsSafetyComponent(nameUpper, descUpper))
            {
                classification.ComponentType = "Safety";
                classification.FirearmCategory = "Component";
                classification.RequiresQualityApproval = true;
                classification.WorkflowTemplate = "BT_Standard_Workflow";
            }
            else
            {
                classification.ComponentType = "General";
                classification.FirearmCategory = "Component";
                classification.WorkflowTemplate = "BT_Standard_Workflow";
            }

            // Extract caliber compatibility
            classification.CaliberCompatibility = ExtractCaliberCompatibility(nameUpper + " " + descUpper);

            _logger.LogInformation("? Classified as: {ComponentType} - {FirearmCategory}", 
                classification.ComponentType, classification.FirearmCategory);

            return classification;
        }

        /// <summary>
        /// Validate B&T manufacturing requirements and identify potential issues
        /// </summary>
        public async Task<List<string>> ValidateBTManufacturingRequirements(Part part)
        {
            _logger.LogInformation("?? Validating B&T manufacturing requirements for part: {PartNumber}", part.PartNumber);

            var issues = new List<string>();

            // Suppressor validation
            if (part.IsSuppressorComponent())
            {
                if (!part.RequiresATFForm1 && !part.RequiresATFForm4)
                    issues.Add("Suppressor components typically require ATF Form 1 or Form 4");

                if (!part.RequiresTaxStamp)
                    issues.Add("Suppressor components typically require tax stamp ($200)");

                if (!part.RequiresSoundTesting)
                    issues.Add("Suppressor components should include sound reduction testing");

                if (!part.RequiresBackPressureTesting)
                    issues.Add("Suppressor components should include back pressure testing");

                if (string.IsNullOrEmpty(part.BTSuppressorType))
                    issues.Add("Suppressor type must be specified (Baffle, EndCap, Tube, etc.)");

                if (part.BTSuppressorType == "Baffle" && string.IsNullOrEmpty(part.BTBafflePosition))
                    issues.Add("Baffle position must be specified (Front, Middle, Rear, End)");
            }

            // Firearm validation
            if (part.IsFirearmComponent())
            {
                if (!part.RequiresUniqueSerialNumber)
                    issues.Add("Firearm components require unique serial numbers");

                if (!part.RequiresATFCompliance)
                    issues.Add("Firearm components require ATF compliance tracking");

                if (!part.RequiresFFLTracking)
                    issues.Add("Firearm components require FFL tracking");

                if (string.IsNullOrEmpty(part.SerialNumberFormat))
                    issues.Add("Serial number format must be specified for firearm components");
            }

            // Material validation
            if (part.Material.Contains("Inconel", StringComparison.OrdinalIgnoreCase))
            {
                if (!part.RequiresBTProofTesting)
                    issues.Add("Inconel parts should include B&T proof testing protocol");

                if (part.RecommendedBuildTemperature < 200)
                    issues.Add("Inconel typically requires higher build temperatures (>200°C)");
            }

            // Regulatory validation
            if (part.RequiresTaxStamp && !part.TaxStampAmount.HasValue)
                issues.Add("Tax stamp amount must be specified when tax stamp is required");

            if (part.RequiresExportLicense && string.IsNullOrEmpty(part.ITARCategory))
                issues.Add("ITAR category must be specified for export controlled items");

            if (part.RequiresATFForm1 && string.IsNullOrEmpty(part.ATFClassification))
                issues.Add("ATF classification must be specified when ATF Form 1 is required");

            // Manufacturing validation
            if (part.EstimatedHours <= 0)
                issues.Add("Estimated manufacturing hours must be greater than 0");

            if (part.EstimatedHours > 100)
                issues.Add("Estimated hours seems unusually high (>100 hours) - please verify");

            // Quality validation
            if (part.IsBTRegulatedComponent() && !part.RequiresTraceabilityDocuments)
                issues.Add("Regulated components typically require complete traceability documentation");

            // Thread compatibility validation
            if (!string.IsNullOrEmpty(part.BTThreadPitch) && !string.IsNullOrEmpty(part.BTCaliberCompatibility))
            {
                var threadIssues = await ValidateThreadCaliberCompatibility(part.BTThreadPitch, part.BTCaliberCompatibility);
                issues.AddRange(threadIssues);
            }

            _logger.LogInformation("? B&T validation complete. Found {IssueCount} potential issues", issues.Count);

            return issues;
        }

        /// <summary>
        /// Get recommended B&T workflow based on component type and complexity
        /// </summary>
        public string GetRecommendedBTWorkflow(Part part)
        {
            if (part.IsSuppressorComponent()) return "BT_Suppressor_Workflow";
            if (part.IsFirearmComponent()) return "BT_Firearm_Workflow";
            if (part.IsBTRegulatedComponent()) return "BT_Regulated_Workflow";
            
            // Check complexity level
            var complexity = CalculateBTComplexityScore(part);
            if (complexity >= 15) return "BT_Complex_Workflow";
            if (complexity >= 10) return "BT_Standard_Workflow";
            
            return "BT_Simple_Workflow";
        }

        /// <summary>
        /// Calculate B&T manufacturing complexity score
        /// </summary>
        public int CalculateBTComplexityScore(Part part)
        {
            var score = 0;

            // Time complexity
            if (part.EstimatedHours > 24) score += 3;
            else if (part.EstimatedHours > 8) score += 2;
            else if (part.EstimatedHours > 2) score += 1;

            // Manufacturing stage complexity
            if (part.RequiresSLSPrinting) score += 1;
            if (part.RequiresCNCMachining) score += 2;
            if (part.RequiresEDMOperations) score += 3;
            if (part.RequiresAssembly) score += 2;
            if (part.RequiresFinishing) score += 1;

            // B&T compliance complexity
            if (part.RequiresATFForm1 || part.RequiresATFForm4) score += 3;
            if (part.RequiresTaxStamp) score += 2;
            if (part.RequiresExportLicense) score += 4;
            if (part.RequiresUniqueSerialNumber) score += 2;

            // Testing complexity
            if (part.RequiresBTProofTesting) score += 2;
            if (part.RequiresSoundTesting) score += 2;
            if (part.RequiresBackPressureTesting) score += 2;
            if (part.RequiresThreadVerification) score += 1;

            // Material complexity
            if (part.Material.Contains("Inconel")) score += 2;
            if (part.Material.Contains("Maraging")) score += 2;

            return score;
        }

        #endregion

        #region Helper Methods

        private bool IsSupressorComponent(string name, string description)
        {
            var suppressorKeywords = new[] {
                "SUPPRESSOR", "SILENCER", "BAFFLE", "END CAP", "SUPPRESSOR TUBE",
                "SOUND REDUCTION", "NOISE REDUCTION", "MUZZLE DEVICE"
            };

            return suppressorKeywords.Any(keyword => 
                name.Contains(keyword) || description.Contains(keyword));
        }

        private bool IsReceiverComponent(string name, string description)
        {
            var receiverKeywords = new[] {
                "RECEIVER", "LOWER", "UPPER", "FRAME", "ACTION"
            };

            return receiverKeywords.Any(keyword => 
                name.Contains(keyword) || description.Contains(keyword));
        }

        private bool IsBarrelComponent(string name, string description)
        {
            var barrelKeywords = new[] {
                "BARREL", "BORE", "RIFLING", "CHAMBER", "MUZZLE"
            };

            return barrelKeywords.Any(keyword => 
                name.Contains(keyword) || description.Contains(keyword));
        }

        private bool IsTriggerComponent(string name, string description)
        {
            var triggerKeywords = new[] {
                "TRIGGER", "SEAR", "HAMMER", "FIRING PIN"
            };

            return triggerKeywords.Any(keyword => 
                name.Contains(keyword) || description.Contains(keyword));
        }

        private bool IsSafetyComponent(string name, string description)
        {
            var safetyKeywords = new[] {
                "SAFETY", "SELECTOR", "SAFETY SWITCH"
            };

            return safetyKeywords.Any(keyword => 
                name.Contains(keyword) || description.Contains(keyword));
        }

        private string ExtractCaliberCompatibility(string text)
        {
            var calibers = new List<string>();
            var caliberPatterns = new[]
            {
                @"\.223", @"\.308", @"\.22", @"\.17", @"\.204", @"\.243", @"\.270", @"\.30-06",
                @"9MM", @"45ACP", @".45", @"40S&W", @".40", @"357MAG", @".357",
                @"5\.56", @"7\.62", @"6\.5", @"300BLK", @"300AAC"
            };

            foreach (var pattern in caliberPatterns)
            {
                var matches = System.Text.RegularExpressions.Regex.Matches(text, pattern);
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (!calibers.Contains(match.Value))
                        calibers.Add(match.Value);
                }
            }

            return string.Join(", ", calibers);
        }

        private string ExtractThreadPitch(string text)
        {
            var threadPatterns = new[]
            {
                @"1/2-28", @"5/8-24", @"3/4-24", @"M14X1", @"M15X1", @"M16X1",
                @"1/2X28", @"5/8X24", @"3/4X24"
            };

            foreach (var pattern in threadPatterns)
            {
                if (text.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    return pattern;
            }

            return string.Empty;
        }

        private async Task<List<string>> ValidateThreadCaliberCompatibility(string threadPitch, string calibers)
        {
            var issues = new List<string>();

            // Common thread/caliber compatibility rules
            var compatibilityRules = new Dictionary<string, string[]>
            {
                ["1/2-28"] = new[] { ".223", ".22", "5.56" },
                ["5/8-24"] = new[] { ".308", "7.62", ".300", ".30" },
                ["3/4-24"] = new[] { ".308", "7.62", ".300", ".338" },
                ["M14x1"] = new[] { "7.62", ".308" },
                ["M15x1"] = new[] { "7.62", ".308", "6.5" }
            };

            if (compatibilityRules.ContainsKey(threadPitch))
            {
                var compatibleCalibers = compatibilityRules[threadPitch];
                var partCalibers = calibers.Split(',').Select(c => c.Trim()).ToArray();

                var hasCompatible = partCalibers.Any(pc => 
                    compatibleCalibers.Any(cc => pc.Contains(cc, StringComparison.OrdinalIgnoreCase)));

                if (!hasCompatible)
                {
                    issues.Add($"Thread pitch {threadPitch} may not be compatible with specified calibers: {calibers}");
                }
            }

            return issues;
        }

        #endregion
    }

    /// <summary>
    /// B&T Component Classification Result
    /// </summary>
    public class BTComponentClassification
    {
        public string ComponentType { get; set; } = "General";
        public string FirearmCategory { get; set; } = "Component";
        public string? SuppressorType { get; set; }
        public string? BafflePosition { get; set; }
        public string CaliberCompatibility { get; set; } = string.Empty;
        public string ThreadPitch { get; set; } = string.Empty;
        public string WorkflowTemplate { get; set; } = "BT_Standard_Workflow";
        
        // Regulatory flags
        public bool RequiresATFForm1 { get; set; }
        public bool RequiresATFForm4 { get; set; }
        public bool RequiresTaxStamp { get; set; }
        public decimal? TaxStampAmount { get; set; }
        public bool RequiresATFCompliance { get; set; }
        public bool RequiresFFLTracking { get; set; }
        public bool RequiresUniqueSerialNumber { get; set; }
        
        // Testing flags
        public bool RequiresSoundTesting { get; set; }
        public bool RequiresBackPressureTesting { get; set; }
        public bool RequiresProofTesting { get; set; }
        public bool RequiresQualityApproval { get; set; }
    }
}