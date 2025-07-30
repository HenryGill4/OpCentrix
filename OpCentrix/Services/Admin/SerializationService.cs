using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using System.Text.RegularExpressions;

namespace OpCentrix.Services.Admin
{
    /// <summary>
    /// Service for managing B&T Serial Numbers and ATF Compliance
    /// Segment 7.2: ATF Serialization Compliance and Component Traceability
    /// </summary>
    public interface ISerializationService
    {
        Task<List<SerialNumber>> GetAllSerialNumbersAsync();
        Task<List<SerialNumber>> GetActiveSerialNumbersAsync();
        Task<List<SerialNumber>> GetSerialNumbersByStatusAsync(string status);
        Task<List<SerialNumber>> GetSerialNumbersByComplianceStatusAsync(string complianceStatus);
        Task<SerialNumber?> GetSerialNumberByIdAsync(int id);
        Task<SerialNumber?> GetSerialNumberByValueAsync(string serialNumberValue);
        Task<SerialNumber> CreateSerialNumberAsync(SerialNumber serialNumber, string createdBy);
        Task<SerialNumber> UpdateSerialNumberAsync(SerialNumber serialNumber, string modifiedBy);
        Task<bool> DeleteSerialNumberAsync(int id, string deletedBy);
        Task<string> GenerateNextSerialNumberAsync(string format, string manufacturerCode);
        Task<bool> SerialNumberExistsAsync(string serialNumberValue, int? excludeId = null);
        Task<bool> ValidateSerialNumberFormatAsync(string serialNumber, string format);
        Task<List<SerialNumber>> SearchSerialNumbersAsync(string searchTerm);
        Task<List<SerialNumber>> GetSerialNumbersForPartAsync(int partId);
        Task<List<SerialNumber>> GetSerialNumbersForJobAsync(int jobId);
        Task<List<SerialNumber>> GetSerialNumbersRequiringActionAsync();
        Task<List<SerialNumber>> GetExpiringComplianceDocumentsAsync(int daysAhead = 30);
        Task<Dictionary<string, int>> GetSerializationStatisticsAsync();
        Task<SerialNumber> AssignSerialNumberToJobAsync(int serialNumberId, int jobId, string assignedBy);
        Task<SerialNumber> UpdateQualityStatusAsync(int serialNumberId, string qualityStatus, string notes, string updatedBy);
        Task<SerialNumber> UpdateATFComplianceAsync(int serialNumberId, string complianceStatus, string formNumbers, string updatedBy);
        Task<SerialNumber> RecordTransferAsync(int serialNumberId, string transferTo, string transferDocument, string notes, string transferredBy);
        Task<SerialNumber> LockSerialNumberAsync(int serialNumberId, string reason, string lockedBy);
        Task<List<SerialNumber>> GetSerialNumbersForDestructionAsync();
        Task<SerialNumber> ScheduleDestructionAsync(int serialNumberId, DateTime destructionDate, string method, string scheduledBy);
        Task<SerialNumber> RecordDestructionAsync(int serialNumberId, DateTime destructionDate, string method, string destroyedBy);
    }

    public class SerializationService : ISerializationService
    {
        private readonly SchedulerContext _context;

        public SerializationService(SchedulerContext context)
        {
            _context = context;
        }

        public async Task<List<SerialNumber>> GetAllSerialNumbersAsync()
        {
            return await _context.SerialNumbers
                .Include(sn => sn.Part)
                .Include(sn => sn.Job)
                .Include(sn => sn.ComplianceRequirement)
                .Include(sn => sn.ComplianceDocuments)
                .OrderByDescending(sn => sn.AssignedDate)
                .ToListAsync();
        }

        public async Task<List<SerialNumber>> GetActiveSerialNumbersAsync()
        {
            return await _context.SerialNumbers
                .Where(sn => sn.IsActive && !sn.IsLocked)
                .Include(sn => sn.Part)
                .Include(sn => sn.Job)
                .OrderByDescending(sn => sn.AssignedDate)
                .ToListAsync();
        }

        public async Task<List<SerialNumber>> GetSerialNumbersByStatusAsync(string status)
        {
            return await _context.SerialNumbers
                .Where(sn => sn.TransferStatus == status)
                .Include(sn => sn.Part)
                .Include(sn => sn.Job)
                .OrderByDescending(sn => sn.AssignedDate)
                .ToListAsync();
        }

        public async Task<List<SerialNumber>> GetSerialNumbersByComplianceStatusAsync(string complianceStatus)
        {
            return await _context.SerialNumbers
                .Where(sn => sn.ATFComplianceStatus == complianceStatus)
                .Include(sn => sn.Part)
                .Include(sn => sn.Job)
                .OrderByDescending(sn => sn.AssignedDate)
                .ToListAsync();
        }

        public async Task<SerialNumber?> GetSerialNumberByIdAsync(int id)
        {
            return await _context.SerialNumbers
                .Include(sn => sn.Part)
                .Include(sn => sn.Job)
                .Include(sn => sn.ComplianceRequirement)
                .Include(sn => sn.ComplianceDocuments)
                .FirstOrDefaultAsync(sn => sn.Id == id);
        }

        public async Task<SerialNumber?> GetSerialNumberByValueAsync(string serialNumberValue)
        {
            return await _context.SerialNumbers
                .Include(sn => sn.Part)
                .Include(sn => sn.Job)
                .Include(sn => sn.ComplianceDocuments)
                .FirstOrDefaultAsync(sn => sn.SerialNumberValue == serialNumberValue);
        }

        public async Task<SerialNumber> CreateSerialNumberAsync(SerialNumber serialNumber, string createdBy)
        {
            // Validate serial number format
            if (!await ValidateSerialNumberFormatAsync(serialNumber.SerialNumberValue, serialNumber.SerialNumberFormat))
            {
                throw new ArgumentException($"Serial number '{serialNumber.SerialNumberValue}' does not match format '{serialNumber.SerialNumberFormat}'");
            }

            // Check for duplicates
            if (await SerialNumberExistsAsync(serialNumber.SerialNumberValue))
            {
                throw new ArgumentException($"Serial number '{serialNumber.SerialNumberValue}' already exists");
            }

            serialNumber.CreatedBy = createdBy;
            serialNumber.LastModifiedBy = createdBy;
            serialNumber.CreatedDate = DateTime.UtcNow;
            serialNumber.LastModifiedDate = DateTime.UtcNow;
            serialNumber.AssignedDate = DateTime.UtcNow;

            _context.SerialNumbers.Add(serialNumber);
            await _context.SaveChangesAsync();

            return serialNumber;
        }

        public async Task<SerialNumber> UpdateSerialNumberAsync(SerialNumber serialNumber, string modifiedBy)
        {
            var existing = await _context.SerialNumbers.FindAsync(serialNumber.Id);
            if (existing == null)
                throw new ArgumentException($"Serial number with ID {serialNumber.Id} not found");

            if (existing.IsLocked)
                throw new InvalidOperationException($"Serial number '{existing.SerialNumberValue}' is locked and cannot be modified");

            // Update properties (excluding sensitive fields if locked)
            existing.ComponentName = serialNumber.ComponentName;
            existing.ComponentType = serialNumber.ComponentType;
            existing.ManufacturedDate = serialNumber.ManufacturedDate;
            existing.CompletedDate = serialNumber.CompletedDate;
            existing.ManufacturingMethod = serialNumber.ManufacturingMethod;
            existing.MaterialUsed = serialNumber.MaterialUsed;
            existing.MaterialLotNumber = serialNumber.MaterialLotNumber;
            existing.MachineUsed = serialNumber.MachineUsed;
            existing.Operator = serialNumber.Operator;
            existing.QualityInspector = serialNumber.QualityInspector;
            existing.ATFClassification = serialNumber.ATFClassification;
            existing.FFLDealer = serialNumber.FFLDealer;
            existing.FFLNumber = serialNumber.FFLNumber;
            existing.IsITARControlled = serialNumber.IsITARControlled;
            existing.IsEARControlled = serialNumber.IsEARControlled;
            existing.ExportClassification = serialNumber.ExportClassification;
            existing.ExportLicense = serialNumber.ExportLicense;
            existing.ExportLicenseExpiration = serialNumber.ExportLicenseExpiration;
            existing.DestinationCountry = serialNumber.DestinationCountry;
            existing.EndUser = serialNumber.EndUser;
            existing.RequiresExportPermit = serialNumber.RequiresExportPermit;
            existing.ExportPermitObtained = serialNumber.ExportPermitObtained;
            existing.ManufacturingHistory = serialNumber.ManufacturingHistory;
            existing.ComponentGenealogy = serialNumber.ComponentGenealogy;
            existing.AssemblyComponents = serialNumber.AssemblyComponents;
            existing.BatchNumber = serialNumber.BatchNumber;
            existing.BuildPlatformId = serialNumber.BuildPlatformId;
            existing.LastModifiedBy = modifiedBy;
            existing.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteSerialNumberAsync(int id, string deletedBy)
        {
            var serialNumber = await _context.SerialNumbers
                .Include(sn => sn.ComplianceDocuments)
                .FirstOrDefaultAsync(sn => sn.Id == id);

            if (serialNumber == null)
                return false;

            if (serialNumber.IsLocked)
                throw new InvalidOperationException($"Serial number '{serialNumber.SerialNumberValue}' is locked and cannot be deleted");

            if (serialNumber.TransferStatus != "In Manufacturing")
                throw new InvalidOperationException($"Serial number '{serialNumber.SerialNumberValue}' has been transferred and cannot be deleted");

            // Remove associated compliance documents
            _context.ComplianceDocuments.RemoveRange(serialNumber.ComplianceDocuments);
            
            _context.SerialNumbers.Remove(serialNumber);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateNextSerialNumberAsync(string format, string manufacturerCode)
        {
            // Get the highest sequence number for this format
            var pattern = format.Replace("YYYY", DateTime.UtcNow.Year.ToString())
                               .Replace("XXXXX", "[0-9]{5}")
                               .Replace("XXXX", "[0-9]{4}")
                               .Replace("XXX", "[0-9]{3}");

            var lastSerial = await _context.SerialNumbers
                .Where(sn => Regex.IsMatch(sn.SerialNumberValue, $"^{pattern}$"))
                .OrderByDescending(sn => sn.SerialNumberValue)
                .FirstOrDefaultAsync();

            int nextSequence = 1;
            if (lastSerial != null)
            {
                // Extract sequence number from last serial
                var regex = new Regex(@"(\d+)$");
                var match = regex.Match(lastSerial.SerialNumberValue);
                if (match.Success && int.TryParse(match.Value, out int lastSequence))
                {
                    nextSequence = lastSequence + 1;
                }
            }

            return SerialNumber.GenerateNextSerialNumber(format, nextSequence);
        }

        public async Task<bool> SerialNumberExistsAsync(string serialNumberValue, int? excludeId = null)
        {
            var query = _context.SerialNumbers.Where(sn => sn.SerialNumberValue == serialNumberValue);
            
            if (excludeId.HasValue)
                query = query.Where(sn => sn.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> ValidateSerialNumberFormatAsync(string serialNumber, string format)
        {
            if (string.IsNullOrEmpty(serialNumber) || string.IsNullOrEmpty(format))
                return false;

            // Convert format to regex pattern
            var pattern = format.Replace("YYYY", @"\d{4}")
                               .Replace("XXXXX", @"\d{5}")
                               .Replace("XXXX", @"\d{4}")
                               .Replace("XXX", @"\d{3}")
                               .Replace("-", @"\-");

            var regex = new Regex($"^{pattern}$");
            return regex.IsMatch(serialNumber);
        }

        public async Task<List<SerialNumber>> SearchSerialNumbersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveSerialNumbersAsync();

            var term = searchTerm.ToLower();

            return await _context.SerialNumbers
                .Where(sn => sn.IsActive && (
                    sn.SerialNumberValue.ToLower().Contains(term) ||
                    sn.ComponentName.ToLower().Contains(term) ||
                    sn.ComponentType.ToLower().Contains(term) ||
                    sn.PartNumber!.ToLower().Contains(term) ||
                    sn.MaterialUsed.ToLower().Contains(term) ||
                    sn.MachineUsed.ToLower().Contains(term) ||
                    sn.Operator.ToLower().Contains(term)
                ))
                .Include(sn => sn.Part)
                .Include(sn => sn.Job)
                .OrderByDescending(sn => sn.AssignedDate)
                .ToListAsync();
        }

        public async Task<List<SerialNumber>> GetSerialNumbersForPartAsync(int partId)
        {
            return await _context.SerialNumbers
                .Where(sn => sn.PartId == partId)
                .Include(sn => sn.Job)
                .OrderByDescending(sn => sn.AssignedDate)
                .ToListAsync();
        }

        public async Task<List<SerialNumber>> GetSerialNumbersForJobAsync(int jobId)
        {
            return await _context.SerialNumbers
                .Where(sn => sn.JobId == jobId)
                .Include(sn => sn.Part)
                .OrderByDescending(sn => sn.AssignedDate)
                .ToListAsync();
        }

        public async Task<List<SerialNumber>> GetSerialNumbersRequiringActionAsync()
        {
            var cutoffDate = DateTime.UtcNow.AddDays(30);

            return await _context.SerialNumbers
                .Where(sn => sn.IsActive && (
                    (sn.ATFComplianceStatus == "Pending" && sn.ATFFormSubmissionDate < DateTime.UtcNow.AddDays(-30)) ||
                    (sn.QualityStatus == "Pending" && sn.AssignedDate < DateTime.UtcNow.AddDays(-7)) ||
                    (sn.ExportLicenseExpiration.HasValue && sn.ExportLicenseExpiration.Value <= cutoffDate) ||
                    (sn.IsDestructionScheduled && sn.ScheduledDestructionDate <= cutoffDate)
                ))
                .Include(sn => sn.Part)
                .Include(sn => sn.Job)
                .OrderBy(sn => sn.AssignedDate)
                .ToListAsync();
        }

        public async Task<List<SerialNumber>> GetExpiringComplianceDocumentsAsync(int daysAhead = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);

            return await _context.SerialNumbers
                .Where(sn => sn.IsActive && 
                    sn.ExportLicenseExpiration.HasValue && 
                    sn.ExportLicenseExpiration.Value <= cutoffDate)
                .Include(sn => sn.Part)
                .OrderBy(sn => sn.ExportLicenseExpiration)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetSerializationStatisticsAsync()
        {
            var stats = new Dictionary<string, int>();

            // Total counts
            stats["Total"] = await _context.SerialNumbers.CountAsync();
            stats["Active"] = await _context.SerialNumbers.CountAsync(sn => sn.IsActive);
            stats["Locked"] = await _context.SerialNumbers.CountAsync(sn => sn.IsLocked);

            // Status breakdown
            stats["In Manufacturing"] = await _context.SerialNumbers.CountAsync(sn => sn.TransferStatus == "In Manufacturing");
            stats["Available"] = await _context.SerialNumbers.CountAsync(sn => sn.TransferStatus == "Available");
            stats["Transferred"] = await _context.SerialNumbers.CountAsync(sn => sn.TransferStatus == "Transferred");
            stats["Destroyed"] = await _context.SerialNumbers.CountAsync(sn => sn.TransferStatus == "Destroyed");

            // Compliance status
            stats["ATF Compliant"] = await _context.SerialNumbers.CountAsync(sn => sn.ATFComplianceStatus == "Compliant");
            stats["ATF Pending"] = await _context.SerialNumbers.CountAsync(sn => sn.ATFComplianceStatus == "Pending");
            stats["ATF Non-Compliant"] = await _context.SerialNumbers.CountAsync(sn => sn.ATFComplianceStatus == "Non-Compliant");

            // Quality status
            stats["Quality Pass"] = await _context.SerialNumbers.CountAsync(sn => sn.QualityStatus == "Pass");
            stats["Quality Pending"] = await _context.SerialNumbers.CountAsync(sn => sn.QualityStatus == "Pending");
            stats["Quality Fail"] = await _context.SerialNumbers.CountAsync(sn => sn.QualityStatus == "Fail");

            // Component types
            stats["Suppressors"] = await _context.SerialNumbers.CountAsync(sn => sn.ComponentType == "Suppressor");
            stats["Receivers"] = await _context.SerialNumbers.CountAsync(sn => sn.ComponentType == "Receiver");
            stats["Barrels"] = await _context.SerialNumbers.CountAsync(sn => sn.ComponentType == "Barrel");

            // Export control
            stats["ITAR Controlled"] = await _context.SerialNumbers.CountAsync(sn => sn.IsITARControlled);
            stats["EAR Controlled"] = await _context.SerialNumbers.CountAsync(sn => sn.IsEARControlled);

            return stats;
        }

        public async Task<SerialNumber> AssignSerialNumberToJobAsync(int serialNumberId, int jobId, string assignedBy)
        {
            var serialNumber = await _context.SerialNumbers.FindAsync(serialNumberId);
            if (serialNumber == null)
                throw new ArgumentException($"Serial number with ID {serialNumberId} not found");

            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null)
                throw new ArgumentException($"Job with ID {jobId} not found");

            serialNumber.JobId = jobId;
            serialNumber.AssignedJobId = job.Id.ToString();
            serialNumber.PartNumber = job.PartNumber;
            serialNumber.LastModifiedBy = assignedBy;
            serialNumber.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return serialNumber;
        }

        public async Task<SerialNumber> UpdateQualityStatusAsync(int serialNumberId, string qualityStatus, string notes, string updatedBy)
        {
            var serialNumber = await _context.SerialNumbers.FindAsync(serialNumberId);
            if (serialNumber == null)
                throw new ArgumentException($"Serial number with ID {serialNumberId} not found");

            serialNumber.QualityStatus = qualityStatus;
            serialNumber.QualityInspectionDate = DateTime.UtcNow;
            serialNumber.QualityNotes = notes;
            serialNumber.QualityInspector = updatedBy;
            
            // Update individual test results based on status
            if (qualityStatus == "Pass")
            {
                serialNumber.DimensionalTestPassed = true;
                serialNumber.MaterialTestPassed = true;
                // Set other test results as needed
            }

            serialNumber.LastModifiedBy = updatedBy;
            serialNumber.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return serialNumber;
        }

        public async Task<SerialNumber> UpdateATFComplianceAsync(int serialNumberId, string complianceStatus, string formNumbers, string updatedBy)
        {
            var serialNumber = await _context.SerialNumbers.FindAsync(serialNumberId);
            if (serialNumber == null)
                throw new ArgumentException($"Serial number with ID {serialNumberId} not found");

            serialNumber.ATFComplianceStatus = complianceStatus;
            serialNumber.ATFFormNumbers = formNumbers;
            
            if (complianceStatus == "Compliant")
            {
                serialNumber.ATFApprovalDate = DateTime.UtcNow;
            }

            serialNumber.LastModifiedBy = updatedBy;
            serialNumber.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return serialNumber;
        }

        public async Task<SerialNumber> RecordTransferAsync(int serialNumberId, string transferTo, string transferDocument, string notes, string transferredBy)
        {
            var serialNumber = await _context.SerialNumbers.FindAsync(serialNumberId);
            if (serialNumber == null)
                throw new ArgumentException($"Serial number with ID {serialNumberId} not found");

            if (!serialNumber.IsReadyForTransfer())
                throw new InvalidOperationException($"Serial number '{serialNumber.SerialNumberValue}' is not ready for transfer");

            serialNumber.TransferStatus = "Transferred";
            serialNumber.TransferDate = DateTime.UtcNow;
            serialNumber.TransferTo = transferTo;
            serialNumber.TransferDocument = transferDocument;
            serialNumber.TransferNotes = notes;
            serialNumber.LastModifiedBy = transferredBy;
            serialNumber.LastModifiedDate = DateTime.UtcNow;

            // Lock the serial number after transfer
            serialNumber.IsLocked = true;

            await _context.SaveChangesAsync();
            return serialNumber;
        }

        public async Task<SerialNumber> LockSerialNumberAsync(int serialNumberId, string reason, string lockedBy)
        {
            var serialNumber = await _context.SerialNumbers.FindAsync(serialNumberId);
            if (serialNumber == null)
                throw new ArgumentException($"Serial number with ID {serialNumberId} not found");

            serialNumber.Lock(reason, lockedBy);
            await _context.SaveChangesAsync();
            return serialNumber;
        }

        public async Task<List<SerialNumber>> GetSerialNumbersForDestructionAsync()
        {
            return await _context.SerialNumbers
                .Where(sn => sn.IsDestructionScheduled && !sn.ActualDestructionDate.HasValue)
                .Include(sn => sn.Part)
                .OrderBy(sn => sn.ScheduledDestructionDate)
                .ToListAsync();
        }

        public async Task<SerialNumber> ScheduleDestructionAsync(int serialNumberId, DateTime destructionDate, string method, string scheduledBy)
        {
            var serialNumber = await _context.SerialNumbers.FindAsync(serialNumberId);
            if (serialNumber == null)
                throw new ArgumentException($"Serial number with ID {serialNumberId} not found");

            serialNumber.IsDestructionScheduled = true;
            serialNumber.ScheduledDestructionDate = destructionDate;
            serialNumber.DestructionMethod = method;
            serialNumber.LastModifiedBy = scheduledBy;
            serialNumber.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return serialNumber;
        }

        public async Task<SerialNumber> RecordDestructionAsync(int serialNumberId, DateTime destructionDate, string method, string destroyedBy)
        {
            var serialNumber = await _context.SerialNumbers.FindAsync(serialNumberId);
            if (serialNumber == null)
                throw new ArgumentException($"Serial number with ID {serialNumberId} not found");

            serialNumber.ActualDestructionDate = destructionDate;
            serialNumber.DestructionMethod = method;
            serialNumber.TransferStatus = "Destroyed";
            serialNumber.IsActive = false;
            serialNumber.IsLocked = true;
            serialNumber.LastModifiedBy = destroyedBy;
            serialNumber.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return serialNumber;
        }
    }
}