using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin
{
    /// <summary>
    /// Service for managing B&T Compliance Requirements and Documentation
    /// Segment 7.2: Regulatory compliance automation and audit trail management
    /// </summary>
    public interface IComplianceService
    {
        // Compliance Requirements Management
        Task<List<ComplianceRequirement>> GetAllComplianceRequirementsAsync();
        Task<List<ComplianceRequirement>> GetActiveComplianceRequirementsAsync();
        Task<List<ComplianceRequirement>> GetComplianceRequirementsByTypeAsync(string complianceType);
        Task<ComplianceRequirement?> GetComplianceRequirementByIdAsync(int id);
        Task<ComplianceRequirement?> GetComplianceRequirementByCodeAsync(string code);
        Task<ComplianceRequirement> CreateComplianceRequirementAsync(ComplianceRequirement requirement, string createdBy);
        Task<ComplianceRequirement> UpdateComplianceRequirementAsync(ComplianceRequirement requirement, string modifiedBy);
        Task<bool> DeleteComplianceRequirementAsync(int id, string deletedBy);
        Task<List<ComplianceRequirement>> GetApplicableRequirementsAsync(string partType, string industry, string process);
        
        // Compliance Documents Management
        Task<List<ComplianceDocument>> GetAllComplianceDocumentsAsync();
        Task<List<ComplianceDocument>> GetActiveComplianceDocumentsAsync();
        Task<List<ComplianceDocument>> GetComplianceDocumentsByTypeAsync(string documentType);
        Task<List<ComplianceDocument>> GetComplianceDocumentsByStatusAsync(string status);
        Task<ComplianceDocument?> GetComplianceDocumentByIdAsync(int id);
        Task<ComplianceDocument?> GetComplianceDocumentByNumberAsync(string documentNumber);
        Task<ComplianceDocument> CreateComplianceDocumentAsync(ComplianceDocument document, string createdBy);
        Task<ComplianceDocument> UpdateComplianceDocumentAsync(ComplianceDocument document, string modifiedBy);
        Task<bool> DeleteComplianceDocumentAsync(int id, string deletedBy);
        Task<ComplianceDocument> ApproveDocumentAsync(int documentId, string approvedBy, string comments);
        Task<ComplianceDocument> RejectDocumentAsync(int documentId, string rejectedBy, string comments);
        Task<ComplianceDocument> ArchiveDocumentAsync(int documentId, string archivedBy);
        
        // Compliance Monitoring and Alerts
        Task<List<ComplianceDocument>> GetExpiringDocumentsAsync(int daysAhead = 30);
        Task<List<ComplianceDocument>> GetDocumentsRequiringRenewalAsync();
        Task<List<ComplianceRequirement>> GetRequirementsNeedingReviewAsync();
        Task<Dictionary<string, int>> GetComplianceStatisticsAsync();
        Task<List<string>> GetComplianceAlertsAsync();
        
        // Compliance Validation and Checking
        Task<bool> ValidatePartComplianceAsync(int partId);
        Task<List<string>> GetComplianceIssuesForPartAsync(int partId);
        Task<bool> ValidateSerialNumberComplianceAsync(int serialNumberId);
        Task<List<string>> GetComplianceIssuesForSerialNumberAsync(int serialNumberId);
        
        // Search and Reporting
        Task<List<ComplianceRequirement>> SearchComplianceRequirementsAsync(string searchTerm);
        Task<List<ComplianceDocument>> SearchComplianceDocumentsAsync(string searchTerm);
        Task<List<ComplianceDocument>> GetDocumentsForSerialNumberAsync(int serialNumberId);
        Task<List<ComplianceDocument>> GetDocumentsForPartAsync(int partId);
        Task<List<ComplianceDocument>> GetDocumentsForJobAsync(int jobId);
        
        // Document Access Tracking
        Task<ComplianceDocument> RecordDocumentAccessAsync(int documentId, string accessedBy);
        Task<List<ComplianceDocument>> GetRecentlyAccessedDocumentsAsync(string userId, int count = 10);
    }

    public class ComplianceService : IComplianceService
    {
        private readonly SchedulerContext _context;

        public ComplianceService(SchedulerContext context)
        {
            _context = context;
        }

        #region Compliance Requirements Management

        public async Task<List<ComplianceRequirement>> GetAllComplianceRequirementsAsync()
        {
            return await _context.ComplianceRequirements
                .Include(cr => cr.PartClassification)
                .Include(cr => cr.SerialNumbers)
                .Include(cr => cr.ComplianceDocuments)
                .OrderBy(cr => cr.ComplianceType)
                .ThenBy(cr => cr.RequirementName)
                .ToListAsync();
        }

        public async Task<List<ComplianceRequirement>> GetActiveComplianceRequirementsAsync()
        {
            return await _context.ComplianceRequirements
                .Where(cr => cr.IsActive && cr.IsCurrentVersion)
                .Include(cr => cr.PartClassification)
                .OrderBy(cr => cr.ComplianceType)
                .ThenBy(cr => cr.RequirementName)
                .ToListAsync();
        }

        public async Task<List<ComplianceRequirement>> GetComplianceRequirementsByTypeAsync(string complianceType)
        {
            return await _context.ComplianceRequirements
                .Where(cr => cr.IsActive && cr.ComplianceType == complianceType)
                .Include(cr => cr.PartClassification)
                .OrderBy(cr => cr.RequirementName)
                .ToListAsync();
        }

        public async Task<ComplianceRequirement?> GetComplianceRequirementByIdAsync(int id)
        {
            return await _context.ComplianceRequirements
                .Include(cr => cr.PartClassification)
                .Include(cr => cr.SerialNumbers)
                .Include(cr => cr.ComplianceDocuments)
                .FirstOrDefaultAsync(cr => cr.Id == id);
        }

        public async Task<ComplianceRequirement?> GetComplianceRequirementByCodeAsync(string code)
        {
            return await _context.ComplianceRequirements
                .Include(cr => cr.PartClassification)
                .FirstOrDefaultAsync(cr => cr.RequirementCode == code);
        }

        public async Task<ComplianceRequirement> CreateComplianceRequirementAsync(ComplianceRequirement requirement, string createdBy)
        {
            requirement.CreatedBy = createdBy;
            requirement.LastModifiedBy = createdBy;
            requirement.CreatedDate = DateTime.UtcNow;
            requirement.LastModifiedDate = DateTime.UtcNow;

            _context.ComplianceRequirements.Add(requirement);
            await _context.SaveChangesAsync();

            return requirement;
        }

        public async Task<ComplianceRequirement> UpdateComplianceRequirementAsync(ComplianceRequirement requirement, string modifiedBy)
        {
            var existing = await _context.ComplianceRequirements.FindAsync(requirement.Id);
            if (existing == null)
                throw new ArgumentException($"Compliance requirement with ID {requirement.Id} not found");

            // Update properties
            existing.RequirementCode = requirement.RequirementCode;
            existing.RequirementName = requirement.RequirementName;
            existing.Description = requirement.Description;
            existing.ComplianceType = requirement.ComplianceType;
            existing.RegulatoryAuthority = requirement.RegulatoryAuthority;
            existing.RequirementDetails = requirement.RequirementDetails;
            existing.DocumentationRequired = requirement.DocumentationRequired;
            existing.FormsRequired = requirement.FormsRequired;
            existing.RecordKeepingRequirements = requirement.RecordKeepingRequirements;
            existing.ApplicableIndustries = requirement.ApplicableIndustries;
            existing.ApplicablePartTypes = requirement.ApplicablePartTypes;
            existing.ApplicableProcesses = requirement.ApplicableProcesses;
            existing.AppliesToManufacturing = requirement.AppliesToManufacturing;
            existing.AppliesToDistribution = requirement.AppliesToDistribution;
            existing.AppliesToExport = requirement.AppliesToExport;
            existing.AppliesToImport = requirement.AppliesToImport;
            existing.EnforcementLevel = requirement.EnforcementLevel;
            existing.PenaltyType = requirement.PenaltyType;
            existing.PenaltyDescription = requirement.PenaltyDescription;
            existing.MaxPenaltyDays = requirement.MaxPenaltyDays;
            existing.MaxPenaltyAmount = requirement.MaxPenaltyAmount;
            existing.EffectiveDate = requirement.EffectiveDate;
            existing.ExpirationDate = requirement.ExpirationDate;
            existing.NextReviewDate = requirement.NextReviewDate;
            existing.RenewalIntervalMonths = requirement.RenewalIntervalMonths;
            existing.RequiresRenewal = requirement.RequiresRenewal;
            existing.RequiresInspection = requirement.RequiresInspection;
            existing.RenewalProcess = requirement.RenewalProcess;
            existing.ImplementationSteps = requirement.ImplementationSteps;
            existing.RequiredTraining = requirement.RequiredTraining;
            existing.RequiredCertifications = requirement.RequiredCertifications;
            existing.SystemRequirements = requirement.SystemRequirements;
            existing.EstimatedImplementationHours = requirement.EstimatedImplementationHours;
            existing.EstimatedImplementationCost = requirement.EstimatedImplementationCost;
            existing.ReferenceDocuments = requirement.ReferenceDocuments;
            existing.WebResources = requirement.WebResources;
            existing.ContactInformation = requirement.ContactInformation;
            existing.AdditionalNotes = requirement.AdditionalNotes;
            existing.LastModifiedBy = modifiedBy;
            existing.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteComplianceRequirementAsync(int id, string deletedBy)
        {
            var requirement = await _context.ComplianceRequirements
                .Include(cr => cr.SerialNumbers)
                .Include(cr => cr.ComplianceDocuments)
                .FirstOrDefaultAsync(cr => cr.Id == id);

            if (requirement == null)
                return false;

            // Check if any entities are using this requirement
            if (requirement.SerialNumbers.Any() || requirement.ComplianceDocuments.Any())
            {
                throw new InvalidOperationException($"Cannot delete compliance requirement '{requirement.RequirementName}' because it is being used");
            }

            _context.ComplianceRequirements.Remove(requirement);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ComplianceRequirement>> GetApplicableRequirementsAsync(string partType, string industry, string process)
        {
            return await _context.ComplianceRequirements
                .Where(cr => cr.IsActive && cr.IsCurrentVersion)
                .Where(cr => cr.AppliesTo(partType, industry, process))
                .Include(cr => cr.PartClassification)
                .OrderBy(cr => cr.ComplianceType)
                .ThenBy(cr => cr.RequirementName)
                .ToListAsync();
        }

        #endregion

        #region Compliance Documents Management

        public async Task<List<ComplianceDocument>> GetAllComplianceDocumentsAsync()
        {
            return await _context.ComplianceDocuments
                .Include(cd => cd.SerialNumber)
                .Include(cd => cd.ComplianceRequirement)
                .Include(cd => cd.Part)
                .Include(cd => cd.Job)
                .OrderByDescending(cd => cd.DocumentDate)
                .ToListAsync();
        }

        public async Task<List<ComplianceDocument>> GetActiveComplianceDocumentsAsync()
        {
            return await _context.ComplianceDocuments
                .Where(cd => cd.IsActive && !cd.IsArchived && !cd.IsDisposed)
                .Include(cd => cd.SerialNumber)
                .Include(cd => cd.ComplianceRequirement)
                .OrderByDescending(cd => cd.DocumentDate)
                .ToListAsync();
        }

        public async Task<List<ComplianceDocument>> GetComplianceDocumentsByTypeAsync(string documentType)
        {
            return await _context.ComplianceDocuments
                .Where(cd => cd.IsActive && cd.DocumentType == documentType)
                .Include(cd => cd.SerialNumber)
                .Include(cd => cd.ComplianceRequirement)
                .OrderByDescending(cd => cd.DocumentDate)
                .ToListAsync();
        }

        public async Task<List<ComplianceDocument>> GetComplianceDocumentsByStatusAsync(string status)
        {
            return await _context.ComplianceDocuments
                .Where(cd => cd.Status == status)
                .Include(cd => cd.SerialNumber)
                .Include(cd => cd.ComplianceRequirement)
                .OrderByDescending(cd => cd.DocumentDate)
                .ToListAsync();
        }

        public async Task<ComplianceDocument?> GetComplianceDocumentByIdAsync(int id)
        {
            return await _context.ComplianceDocuments
                .Include(cd => cd.SerialNumber)
                .Include(cd => cd.ComplianceRequirement)
                .Include(cd => cd.Part)
                .Include(cd => cd.Job)
                .FirstOrDefaultAsync(cd => cd.Id == id);
        }

        public async Task<ComplianceDocument?> GetComplianceDocumentByNumberAsync(string documentNumber)
        {
            return await _context.ComplianceDocuments
                .Include(cd => cd.SerialNumber)
                .Include(cd => cd.ComplianceRequirement)
                .FirstOrDefaultAsync(cd => cd.DocumentNumber == documentNumber);
        }

        public async Task<ComplianceDocument> CreateComplianceDocumentAsync(ComplianceDocument document, string createdBy)
        {
            document.CreatedBy = createdBy;
            document.LastModifiedBy = createdBy;
            document.PreparedBy = createdBy;
            document.CreatedDate = DateTime.UtcNow;
            document.LastModifiedDate = DateTime.UtcNow;
            document.LastAccessedDate = DateTime.UtcNow;
            document.LastAccessedBy = createdBy;

            // Calculate retention end date
            document.CalculateRetentionEndDate();

            _context.ComplianceDocuments.Add(document);
            await _context.SaveChangesAsync();

            return document;
        }

        public async Task<ComplianceDocument> UpdateComplianceDocumentAsync(ComplianceDocument document, string modifiedBy)
        {
            var existing = await _context.ComplianceDocuments.FindAsync(document.Id);
            if (existing == null)
                throw new ArgumentException($"Compliance document with ID {document.Id} not found");

            if (existing.IsArchived)
                throw new InvalidOperationException($"Document '{existing.DocumentNumber}' is archived and cannot be modified");

            // Update properties
            existing.DocumentTitle = document.DocumentTitle;
            existing.DocumentType = document.DocumentType;
            existing.Description = document.Description;
            existing.ComplianceCategory = document.ComplianceCategory;
            existing.DocumentClassification = document.DocumentClassification;
            existing.RegulatoryAuthority = document.RegulatoryAuthority;
            existing.FormNumber = document.FormNumber;
            existing.DocumentDate = document.DocumentDate;
            existing.EffectiveDate = document.EffectiveDate;
            existing.ExpirationDate = document.ExpirationDate;
            existing.SubmissionDate = document.SubmissionDate;
            existing.ReferenceNumber = document.ReferenceNumber;
            existing.FilePath = document.FilePath;
            existing.FileName = document.FileName;
            existing.FileType = document.FileType;
            existing.FileSizeMB = document.FileSizeMB;
            existing.FileHash = document.FileHash;
            existing.DocumentContent = document.DocumentContent;
            existing.AssociatedSerialNumbers = document.AssociatedSerialNumbers;
            existing.AssociatedPartNumbers = document.AssociatedPartNumbers;
            existing.AssociatedJobNumbers = document.AssociatedJobNumbers;
            existing.Customer = document.Customer;
            existing.Vendor = document.Vendor;
            existing.RetentionPeriod = document.RetentionPeriod;
            existing.RequiresRenewal = document.RequiresRenewal;
            existing.RenewalReminderDays = document.RenewalReminderDays;
            existing.NotificationRecipients = document.NotificationRecipients;
            existing.LastModifiedBy = modifiedBy;
            existing.LastModifiedDate = DateTime.UtcNow;

            // Recalculate retention end date
            existing.CalculateRetentionEndDate();

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteComplianceDocumentAsync(int id, string deletedBy)
        {
            var document = await _context.ComplianceDocuments.FindAsync(id);
            if (document == null)
                return false;

            if (document.IsArchived)
                throw new InvalidOperationException($"Document '{document.DocumentNumber}' is archived and cannot be deleted");

            _context.ComplianceDocuments.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ComplianceDocument> ApproveDocumentAsync(int documentId, string approvedBy, string comments)
        {
            var document = await _context.ComplianceDocuments.FindAsync(documentId);
            if (document == null)
                throw new ArgumentException($"Compliance document with ID {documentId} not found");

            document.Status = "Approved";
            document.ApprovedBy = approvedBy;
            document.ApprovalDateInternal = DateTime.UtcNow;
            document.ApprovalComments = comments;
            document.LastModifiedBy = approvedBy;
            document.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<ComplianceDocument> RejectDocumentAsync(int documentId, string rejectedBy, string comments)
        {
            var document = await _context.ComplianceDocuments.FindAsync(documentId);
            if (document == null)
                throw new ArgumentException($"Compliance document with ID {documentId} not found");

            document.Status = "Rejected";
            document.ReviewedBy = rejectedBy;
            document.ReviewDate = DateTime.UtcNow;
            document.ReviewComments = comments;
            document.LastModifiedBy = rejectedBy;
            document.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<ComplianceDocument> ArchiveDocumentAsync(int documentId, string archivedBy)
        {
            var document = await _context.ComplianceDocuments.FindAsync(documentId);
            if (document == null)
                throw new ArgumentException($"Compliance document with ID {documentId} not found");

            document.IsArchived = true;
            document.ArchiveDate = DateTime.UtcNow;
            document.LastModifiedBy = archivedBy;
            document.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return document;
        }

        #endregion

        #region Compliance Monitoring and Alerts

        public async Task<List<ComplianceDocument>> GetExpiringDocumentsAsync(int daysAhead = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);

            return await _context.ComplianceDocuments
                .Where(cd => cd.IsActive && !cd.IsArchived && 
                            cd.ExpirationDate.HasValue && 
                            cd.ExpirationDate.Value <= cutoffDate)
                .Include(cd => cd.SerialNumber)
                .Include(cd => cd.ComplianceRequirement)
                .OrderBy(cd => cd.ExpirationDate)
                .ToListAsync();
        }

        public async Task<List<ComplianceDocument>> GetDocumentsRequiringRenewalAsync()
        {
            return await _context.ComplianceDocuments
                .Where(cd => cd.IsActive && cd.RequiresRenewal && cd.NeedsRenewalReminder())
                .Include(cd => cd.SerialNumber)
                .Include(cd => cd.ComplianceRequirement)
                .OrderBy(cd => cd.ExpirationDate)
                .ToListAsync();
        }

        public async Task<List<ComplianceRequirement>> GetRequirementsNeedingReviewAsync()
        {
            var cutoffDate = DateTime.UtcNow.AddDays(30);

            return await _context.ComplianceRequirements
                .Where(cr => cr.IsActive && 
                            cr.NextReviewDate.HasValue && 
                            cr.NextReviewDate.Value <= cutoffDate)
                .OrderBy(cr => cr.NextReviewDate)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetComplianceStatisticsAsync()
        {
            var stats = new Dictionary<string, int>();

            // Requirements statistics
            stats["Total Requirements"] = await _context.ComplianceRequirements.CountAsync();
            stats["Active Requirements"] = await _context.ComplianceRequirements.CountAsync(cr => cr.IsActive);
            stats["ATF Requirements"] = await _context.ComplianceRequirements.CountAsync(cr => cr.IsActive && cr.ComplianceType == "ATF");
            stats["ITAR Requirements"] = await _context.ComplianceRequirements.CountAsync(cr => cr.IsActive && cr.ComplianceType == "ITAR");
            stats["EAR Requirements"] = await _context.ComplianceRequirements.CountAsync(cr => cr.IsActive && cr.ComplianceType == "EAR");

            // Documents statistics
            stats["Total Documents"] = await _context.ComplianceDocuments.CountAsync();
            stats["Active Documents"] = await _context.ComplianceDocuments.CountAsync(cd => cd.IsActive && !cd.IsArchived);
            stats["Pending Approval"] = await _context.ComplianceDocuments.CountAsync(cd => cd.Status == "Submitted");
            stats["Approved Documents"] = await _context.ComplianceDocuments.CountAsync(cd => cd.Status == "Approved");
            stats["Expired Documents"] = await _context.ComplianceDocuments.CountAsync(cd => cd.ExpirationDate < DateTime.UtcNow);
            stats["Expiring Soon"] = await _context.ComplianceDocuments.CountAsync(cd => 
                cd.IsActive && cd.ExpirationDate.HasValue && 
                cd.ExpirationDate.Value <= DateTime.UtcNow.AddDays(30));

            return stats;
        }

        public async Task<List<string>> GetComplianceAlertsAsync()
        {
            var alerts = new List<string>();

            // Expiring documents
            var expiringDocs = await GetExpiringDocumentsAsync();
            if (expiringDocs.Count > 0)
                alerts.Add($"{expiringDocs.Count} compliance document(s) expiring in the next 30 days");

            // Documents requiring renewal
            var renewalDocs = await GetDocumentsRequiringRenewalAsync();
            if (renewalDocs.Count > 0)
                alerts.Add($"{renewalDocs.Count} document(s) require renewal notification");

            // Requirements needing review
            var reviewReqs = await GetRequirementsNeedingReviewAsync();
            if (reviewReqs.Count > 0)
                alerts.Add($"{reviewReqs.Count} compliance requirement(s) need review");

            // Pending approvals
            var pendingDocs = await GetComplianceDocumentsByStatusAsync("Submitted");
            if (pendingDocs.Count > 0)
                alerts.Add($"{pendingDocs.Count} document(s) pending approval");

            return alerts;
        }

        #endregion

        #region Compliance Validation and Checking

        public async Task<bool> ValidatePartComplianceAsync(int partId)
        {
            var part = await _context.Parts
                .Include(p => p.PartClassification)
                .Include(p => p.SerialNumbers)
                .FirstOrDefaultAsync(p => p.Id == partId);

            if (part == null)
                return false;

            var issues = await GetComplianceIssuesForPartAsync(partId);
            return issues.Count == 0;
        }

        public async Task<List<string>> GetComplianceIssuesForPartAsync(int partId)
        {
            var issues = new List<string>();
            
            var part = await _context.Parts
                .Include(p => p.PartClassification)
                .Include(p => p.SerialNumbers)
                .FirstOrDefaultAsync(p => p.Id == partId);

            if (part == null)
            {
                issues.Add("Part not found");
                return issues;
            }

            // Check ATF compliance
            if (part.RequiresATFCompliance)
            {
                if (part.SerialNumbers.Any(sn => sn.ATFComplianceStatus != "Compliant"))
                    issues.Add("ATF compliance not achieved for all serial numbers");
            }

            // Check ITAR compliance
            if (part.RequiresITARCompliance)
            {
                if (part.SerialNumbers.Any(sn => sn.IsITARControlled && string.IsNullOrEmpty(sn.ExportLicense)))
                    issues.Add("ITAR controlled items missing export license");
            }

            // Check serialization
            if (part.RequiresSerialization && !part.SerialNumbers.Any())
                issues.Add("Part requires serialization but no serial numbers assigned");

            return issues;
        }

        public async Task<bool> ValidateSerialNumberComplianceAsync(int serialNumberId)
        {
            var issues = await GetComplianceIssuesForSerialNumberAsync(serialNumberId);
            return issues.Count == 0;
        }

        public async Task<List<string>> GetComplianceIssuesForSerialNumberAsync(int serialNumberId)
        {
            var issues = new List<string>();
            
            var serialNumber = await _context.SerialNumbers
                .Include(sn => sn.Part)
                .Include(sn => sn.ComplianceDocuments)
                .FirstOrDefaultAsync(sn => sn.Id == serialNumberId);

            if (serialNumber == null)
            {
                issues.Add("Serial number not found");
                return issues;
            }

            // Check ATF compliance
            if (serialNumber.RequiresATFApproval() && serialNumber.ATFComplianceStatus != "Compliant")
                issues.Add("ATF compliance status is not compliant");

            // Check quality status
            if (serialNumber.QualityStatus != "Pass")
                issues.Add($"Quality status is {serialNumber.QualityStatus}");

            // Check export control
            if (serialNumber.IsITARControlled && string.IsNullOrEmpty(serialNumber.ExportLicense))
                issues.Add("ITAR controlled item missing export license");

            if (serialNumber.IsEARControlled && !serialNumber.ExportPermitObtained)
                issues.Add("EAR controlled item missing export permit");

            // Check expiring licenses
            if (serialNumber.ExportLicenseExpiration.HasValue && 
                serialNumber.ExportLicenseExpiration.Value <= DateTime.UtcNow.AddDays(30))
                issues.Add("Export license expires within 30 days");

            return issues;
        }

        #endregion

        #region Search and Reporting

        public async Task<List<ComplianceRequirement>> SearchComplianceRequirementsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveComplianceRequirementsAsync();

            var term = searchTerm.ToLower();

            return await _context.ComplianceRequirements
                .Where(cr => cr.IsActive && (
                    cr.RequirementCode.ToLower().Contains(term) ||
                    cr.RequirementName.ToLower().Contains(term) ||
                    cr.Description.ToLower().Contains(term) ||
                    cr.ComplianceType.ToLower().Contains(term) ||
                    cr.RegulatoryAuthority.ToLower().Contains(term)
                ))
                .Include(cr => cr.PartClassification)
                .OrderBy(cr => cr.ComplianceType)
                .ThenBy(cr => cr.RequirementName)
                .ToListAsync();
        }

        public async Task<List<ComplianceDocument>> SearchComplianceDocumentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveComplianceDocumentsAsync();

            var term = searchTerm.ToLower();

            return await _context.ComplianceDocuments
                .Where(cd => cd.IsActive && (
                    cd.DocumentNumber.ToLower().Contains(term) ||
                    cd.DocumentTitle.ToLower().Contains(term) ||
                    cd.Description.ToLower().Contains(term) ||
                    cd.DocumentType.ToLower().Contains(term) ||
                    cd.ComplianceCategory.ToLower().Contains(term) ||
                    cd.DocumentContent.ToLower().Contains(term)
                ))
                .Include(cd => cd.SerialNumber)
                .Include(cd => cd.ComplianceRequirement)
                .OrderByDescending(cd => cd.DocumentDate)
                .ToListAsync();
        }

        public async Task<List<ComplianceDocument>> GetDocumentsForSerialNumberAsync(int serialNumberId)
        {
            return await _context.ComplianceDocuments
                .Where(cd => cd.SerialNumberId == serialNumberId)
                .Include(cd => cd.ComplianceRequirement)
                .OrderByDescending(cd => cd.DocumentDate)
                .ToListAsync();
        }

        public async Task<List<ComplianceDocument>> GetDocumentsForPartAsync(int partId)
        {
            return await _context.ComplianceDocuments
                .Where(cd => cd.PartId == partId)
                .Include(cd => cd.SerialNumber)
                .Include(cd => cd.ComplianceRequirement)
                .OrderByDescending(cd => cd.DocumentDate)
                .ToListAsync();
        }

        public async Task<List<ComplianceDocument>> GetDocumentsForJobAsync(int jobId)
        {
            return await _context.ComplianceDocuments
                .Where(cd => cd.JobId == jobId)
                .Include(cd => cd.SerialNumber)
                .Include(cd => cd.ComplianceRequirement)
                .OrderByDescending(cd => cd.DocumentDate)
                .ToListAsync();
        }

        #endregion

        #region Document Access Tracking

        public async Task<ComplianceDocument> RecordDocumentAccessAsync(int documentId, string accessedBy)
        {
            var document = await _context.ComplianceDocuments.FindAsync(documentId);
            if (document == null)
                throw new ArgumentException($"Compliance document with ID {documentId} not found");

            document.RecordAccess(accessedBy);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<List<ComplianceDocument>> GetRecentlyAccessedDocumentsAsync(string userId, int count = 10)
        {
            return await _context.ComplianceDocuments
                .Where(cd => cd.IsActive && cd.LastAccessedBy == userId)
                .Include(cd => cd.SerialNumber)
                .Include(cd => cd.ComplianceRequirement)
                .OrderByDescending(cd => cd.LastAccessedDate)
                .Take(count)
                .ToListAsync();
        }

        #endregion
    }
}