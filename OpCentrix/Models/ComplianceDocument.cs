using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Compliance Documentation for B&T Manufacturing
    /// Segment 7.2: Regulatory documentation and audit trail management
    /// </summary>
    public class ComplianceDocument
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Document Number")]
        public string DocumentNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Document Title")]
        public string DocumentTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; } = string.Empty; // ATF Form, ITAR License, Test Certificate, etc.

        [Required]
        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        #region Document Classification

        [Required]
        [StringLength(50)]
        [Display(Name = "Compliance Category")]
        public string ComplianceCategory { get; set; } = string.Empty; // ATF, ITAR, EAR, Quality, Safety

        [StringLength(50)]
        [Display(Name = "Document Classification")]
        public string DocumentClassification { get; set; } = "Unclassified"; // Unclassified, Confidential, Restricted

        [StringLength(100)]
        [Display(Name = "Regulatory Authority")]
        public string RegulatoryAuthority { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Form Number")]
        public string? FormNumber { get; set; } // ATF Form 1, Form 4, etc.

        #endregion

        #region Document Details

        public DateTime DocumentDate { get; set; } = DateTime.UtcNow;
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public DateTime? ApprovalDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Rejected, Expired

        [StringLength(100)]
        [Display(Name = "Approval Number")]
        public string? ApprovalNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        #endregion

        #region Content and Storage

        [StringLength(500)]
        [Display(Name = "File Path")]
        public string? FilePath { get; set; }

        [StringLength(100)]
        [Display(Name = "File Name")]
        public string? FileName { get; set; }

        [StringLength(20)]
        [Display(Name = "File Type")]
        public string? FileType { get; set; } // PDF, DOCX, etc.

        [Column(TypeName = "decimal(8,2)")]
        [Display(Name = "File Size (MB)")]
        public decimal FileSizeMB { get; set; } = 0;

        [StringLength(100)]
        [Display(Name = "File Hash")]
        public string? FileHash { get; set; } // For integrity verification

        [StringLength(2000)]
        [Display(Name = "Document Content")]
        public string DocumentContent { get; set; } = string.Empty; // Text content for search

        #endregion

        #region Associated Entities

        [StringLength(200)]
        [Display(Name = "Associated Serial Numbers")]
        public string AssociatedSerialNumbers { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Associated Part Numbers")]
        public string AssociatedPartNumbers { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Associated Job Numbers")]
        public string AssociatedJobNumbers { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Customer")]
        public string? Customer { get; set; }

        [StringLength(100)]
        [Display(Name = "Vendor/Supplier")]
        public string? Vendor { get; set; }

        #endregion

        #region Signatures and Authorization

        [StringLength(100)]
        [Display(Name = "Prepared By")]
        public string PreparedBy { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Reviewed By")]
        public string? ReviewedBy { get; set; }

        [StringLength(100)]
        [Display(Name = "Approved By")]
        public string? ApprovedBy { get; set; }

        public DateTime? ReviewDate { get; set; }
        public DateTime? ApprovalDateInternal { get; set; }

        [StringLength(500)]
        [Display(Name = "Review Comments")]
        public string ReviewComments { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Approval Comments")]
        public string ApprovalComments { get; set; } = string.Empty;

        #endregion

        #region Retention and Archive

        [Required]
        [StringLength(20)]
        [Display(Name = "Retention Period")]
        public string RetentionPeriod { get; set; } = "Permanent"; // Years or "Permanent"

        public DateTime? RetentionEndDate { get; set; }
        public DateTime? ArchiveDate { get; set; }
        public DateTime? DisposalDate { get; set; }

        [StringLength(50)]
        [Display(Name = "Archive Location")]
        public string? ArchiveLocation { get; set; }

        [StringLength(500)]
        [Display(Name = "Disposal Method")]
        public string? DisposalMethod { get; set; }

        public bool IsArchived { get; set; } = false;
        public bool IsDisposed { get; set; } = false;

        #endregion

        #region Notifications and Reminders

        public bool RequiresRenewal { get; set; } = false;
        public int RenewalReminderDays { get; set; } = 30;
        public DateTime? NextReminderDate { get; set; }

        public bool EmailNotificationSent { get; set; } = false;
        public DateTime? LastNotificationDate { get; set; }

        [StringLength(200)]
        [Display(Name = "Notification Recipients")]
        public string NotificationRecipients { get; set; } = string.Empty;

        #endregion

        #region Audit Trail

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastAccessedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastModifiedBy { get; set; } = string.Empty;

        [StringLength(100)]
        public string LastAccessedBy { get; set; } = string.Empty;

        public int AccessCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        [StringLength(1000)]
        [Display(Name = "Audit Notes")]
        public string AuditNotes { get; set; } = string.Empty;

        #endregion

        #region Foreign Keys

        public int? SerialNumberId { get; set; }
        public int? ComplianceRequirementId { get; set; }
        public int? PartId { get; set; }
        public int? JobId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Associated serial number
        /// </summary>
        public virtual SerialNumber? SerialNumber { get; set; }

        /// <summary>
        /// Associated compliance requirement
        /// </summary>
        public virtual ComplianceRequirement? ComplianceRequirement { get; set; }

        /// <summary>
        /// Associated part
        /// </summary>
        public virtual Part? Part { get; set; }

        /// <summary>
        /// Associated job
        /// </summary>
        public virtual Job? Job { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Check if document is currently valid
        /// </summary>
        public bool IsCurrentlyValid()
        {
            if (!IsActive || IsArchived || IsDisposed) return false;
            if (Status == "Rejected" || Status == "Expired") return false;
            
            var now = DateTime.UtcNow;
            if (EffectiveDate.HasValue && now < EffectiveDate.Value) return false;
            if (ExpirationDate.HasValue && now > ExpirationDate.Value) return false;
            
            return true;
        }

        /// <summary>
        /// Check if document expires soon
        /// </summary>
        public bool ExpiresSoon(int daysAhead = 30)
        {
            if (!ExpirationDate.HasValue) return false;
            return ExpirationDate.Value <= DateTime.UtcNow.AddDays(daysAhead);
        }

        /// <summary>
        /// Check if document needs renewal reminder
        /// </summary>
        public bool NeedsRenewalReminder()
        {
            if (!RequiresRenewal || !ExpirationDate.HasValue) return false;
            if (EmailNotificationSent && LastNotificationDate.HasValue && 
                LastNotificationDate.Value > DateTime.UtcNow.AddDays(-RenewalReminderDays)) return false;
            
            return ExpirationDate.Value <= DateTime.UtcNow.AddDays(RenewalReminderDays);
        }

        /// <summary>
        /// Get display name for the document
        /// </summary>
        public string GetDisplayName()
        {
            return $"{DocumentNumber} - {DocumentTitle}";
        }

        /// <summary>
        /// Get status with expiration warning
        /// </summary>
        public string GetStatusWithWarning()
        {
            if (!IsCurrentlyValid()) return "Invalid";
            if (ExpiresSoon(7)) return $"{Status} (Expires in {(ExpirationDate!.Value - DateTime.UtcNow).Days} days)";
            if (ExpiresSoon(30)) return $"{Status} (Expires Soon)";
            return Status;
        }

        /// <summary>
        /// Update access tracking
        /// </summary>
        public void RecordAccess(string accessedBy)
        {
            LastAccessedDate = DateTime.UtcNow;
            LastAccessedBy = accessedBy;
            AccessCount++;
        }

        /// <summary>
        /// Calculate retention end date
        /// </summary>
        public void CalculateRetentionEndDate()
        {
            if (RetentionPeriod == "Permanent") return;
            
            if (int.TryParse(RetentionPeriod, out int years))
            {
                RetentionEndDate = DocumentDate.AddYears(years);
            }
        }

        /// <summary>
        /// Get file extension from file name
        /// </summary>
        public string GetFileExtension()
        {
            if (string.IsNullOrEmpty(FileName)) return string.Empty;
            return Path.GetExtension(FileName).ToLowerInvariant();
        }

        /// <summary>
        /// Check if document is searchable (has text content)
        /// </summary>
        public bool IsSearchable()
        {
            return !string.IsNullOrEmpty(DocumentContent) && DocumentContent.Length > 10;
        }

        #endregion
    }
}