# Implementation Plan 10: Version Control System

**Priority: LOW**  
**Estimated Time: 3 days**  
**Dependencies: Plans 1-9 must be completed first**

## Overview

Implement a comprehensive version control system for MasterParts that tracks changes, maintains version history, and provides change approval workflows. This is a future enhancement that provides full audit trails and change management.

## Current State

**What We Have:**
- ? Version control fields in MasterPart model (from Plan 1)
- ? Basic version tracking (VersionNumber, IsLatestVersion, PreviousVersionId)
- ? Database fields for version control

**What We Need:**
- ? Full version control implementation
- ? Change tracking and comparison tools  
- ? Approval workflow system
- ? Version history display
- ? Rollback capabilities

## Solution: Complete Version Control System

Build a full-featured version control system that tracks all changes to MasterParts with approval workflows.

---

## Step-by-Step Implementation

### Step 1: Create Version History Tables

#### 1A: Add Change Tracking Tables
**File: `Data/Migrations/AddVersionControlTables.sql`**

```sql
-- Add comprehensive version control tables
CREATE TABLE IF NOT EXISTS "PartChangeRequests" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartChangeRequests" PRIMARY KEY AUTOINCREMENT,
    "MasterPartId" INTEGER NOT NULL,
    "RequestedByUserId" TEXT NOT NULL,
    "RequestDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "ChangeDescription" TEXT NOT NULL,
    "ChangeReason" TEXT NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Pending',
    "ReviewedByUserId" TEXT NULL,
    "ReviewDate" TEXT NULL,
    "ReviewComments" TEXT NULL,
    "ApprovedDate" TEXT NULL,
    "RejectedDate" TEXT NULL,
    "ImplementedDate" TEXT NULL,
    "NewVersionNumber" INTEGER NULL,
    
    CONSTRAINT "FK_PartChangeRequests_MasterPart" 
        FOREIGN KEY ("MasterPartId") REFERENCES "MasterParts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PartChangeRequests_RequestedByUser" 
        FOREIGN KEY ("RequestedByUserId") REFERENCES "AspNetUsers" ("Id"),
    CONSTRAINT "FK_PartChangeRequests_ReviewedByUser" 
        FOREIGN KEY ("ReviewedByUserId") REFERENCES "AspNetUsers" ("Id")
);

-- Add field-level change tracking
CREATE TABLE IF NOT EXISTS "PartFieldChanges" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartFieldChanges" PRIMARY KEY AUTOINCREMENT,
    "ChangeRequestId" INTEGER NOT NULL,
    "FieldName" TEXT NOT NULL,
    "OldValue" TEXT NULL,
    "NewValue" TEXT NULL,
    "ChangeType" TEXT NOT NULL, -- 'Added', 'Modified', 'Deleted'
    
    CONSTRAINT "FK_PartFieldChanges_ChangeRequest" 
        FOREIGN KEY ("ChangeRequestId") REFERENCES "PartChangeRequests" ("Id") ON DELETE CASCADE
);

-- Add version snapshots table
CREATE TABLE IF NOT EXISTS "PartVersionSnapshots" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PartVersionSnapshots" PRIMARY KEY AUTOINCREMENT,
    "MasterPartId" INTEGER NOT NULL,
    "VersionNumber" INTEGER NOT NULL,
    "SnapshotData" TEXT NOT NULL, -- JSON snapshot of part data
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedByUserId" TEXT NOT NULL,
    "ChangeDescription" TEXT NULL,
    
    CONSTRAINT "FK_PartVersionSnapshots_MasterPart" 
        FOREIGN KEY ("MasterPartId") REFERENCES "MasterParts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PartVersionSnapshots_CreatedByUser" 
        FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id")
);

-- Create indexes for performance
CREATE INDEX "IX_PartChangeRequests_MasterPartId" ON "PartChangeRequests" ("MasterPartId");
CREATE INDEX "IX_PartChangeRequests_Status" ON "PartChangeRequests" ("Status");
CREATE INDEX "IX_PartChangeRequests_RequestDate" ON "PartChangeRequests" ("RequestDate");
CREATE INDEX "IX_PartFieldChanges_ChangeRequestId" ON "PartFieldChanges" ("ChangeRequestId");
CREATE INDEX "IX_PartVersionSnapshots_MasterPartId" ON "PartVersionSnapshots" ("MasterPartId");
CREATE INDEX "IX_PartVersionSnapshots_VersionNumber" ON "PartVersionSnapshots" ("VersionNumber");

-- Verify tables creation
SELECT 'Version Control tables created successfully' as Status;
```

#### 1B: Create Version Control Models
**File: `Models/VersionControlModels.cs`**

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models.VersionControl
{
    public class PartChangeRequest
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int MasterPartId { get; set; }
        
        [Required]
        public string RequestedByUserId { get; set; } = string.Empty;
        
        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(500)]
        public string ChangeDescription { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string ChangeReason { get; set; } = string.Empty;
        
        [Required]
        public ChangeRequestStatus Status { get; set; } = ChangeRequestStatus.Pending;
        
        public string? ReviewedByUserId { get; set; }
        public DateTime? ReviewDate { get; set; }
        
        [StringLength(1000)]
        public string? ReviewComments { get; set; }
        
        public DateTime? ApprovedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public DateTime? ImplementedDate { get; set; }
        
        public int? NewVersionNumber { get; set; }
        
        // Navigation properties
        public virtual MasterPart? MasterPart { get; set; }
        public virtual ICollection<PartFieldChange> FieldChanges { get; set; } = new List<PartFieldChange>();
        
        // Computed properties
        [NotMapped]
        public string StatusDisplay => Status switch
        {
            ChangeRequestStatus.Pending => "Pending Review",
            ChangeRequestStatus.UnderReview => "Under Review",
            ChangeRequestStatus.Approved => "Approved",
            ChangeRequestStatus.Rejected => "Rejected",
            ChangeRequestStatus.Implemented => "Implemented",
            _ => "Unknown"
        };
        
        [NotMapped]
        public string StatusClass => Status switch
        {
            ChangeRequestStatus.Pending => "warning",
            ChangeRequestStatus.UnderReview => "info",
            ChangeRequestStatus.Approved => "success",
            ChangeRequestStatus.Rejected => "danger",
            ChangeRequestStatus.Implemented => "primary",
            _ => "secondary"
        };
    }

    public class PartFieldChange
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ChangeRequestId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FieldName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? OldValue { get; set; }
        
        [StringLength(500)]
        public string? NewValue { get; set; }
        
        [Required]
        public FieldChangeType ChangeType { get; set; }
        
        // Navigation property
        public virtual PartChangeRequest? ChangeRequest { get; set; }
        
        // Computed properties
        [NotMapped]
        public string ChangeTypeDisplay => ChangeType switch
        {
            FieldChangeType.Added => "Added",
            FieldChangeType.Modified => "Modified",
            FieldChangeType.Deleted => "Deleted",
            _ => "Unknown"
        };
        
        [NotMapped]
        public string ChangeTypeClass => ChangeType switch
        {
            FieldChangeType.Added => "success",
            FieldChangeType.Modified => "warning",
            FieldChangeType.Deleted => "danger",
            _ => "secondary"
        };
    }

    public class PartVersionSnapshot
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int MasterPartId { get; set; }
        
        [Required]
        public int VersionNumber { get; set; }
        
        [Required]
        public string SnapshotData { get; set; } = string.Empty; // JSON data
        
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? ChangeDescription { get; set; }
        
        // Navigation property
        public virtual MasterPart? MasterPart { get; set; }
    }

    public enum ChangeRequestStatus
    {
        Pending = 0,
        UnderReview = 1,
        Approved = 2,
        Rejected = 3,
        Implemented = 4
    }

    public enum FieldChangeType
    {
        Added = 0,
        Modified = 1,
        Deleted = 2
    }
}
```

#### 1C: Update SchedulerContext
**File: `Data/SchedulerContext.cs`**

```csharp
public class SchedulerContext : DbContext
{
    // ... existing DbSets ...
    
    public DbSet<PartChangeRequest> PartChangeRequests { get; set; }
    public DbSet<PartFieldChange> PartFieldChanges { get; set; }
    public DbSet<PartVersionSnapshot> PartVersionSnapshots { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ... existing configurations ...
        
        // Configure version control entities
        modelBuilder.Entity<PartChangeRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            
            entity.HasOne<MasterPart>()
                  .WithMany()
                  .HasForeignKey(e => e.MasterPartId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.MasterPartId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RequestDate);
        });

        modelBuilder.Entity<PartFieldChange>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangeType).HasConversion<string>();
            
            entity.HasOne<PartChangeRequest>()
                  .WithMany(pcr => pcr.FieldChanges)
                  .HasForeignKey(e => e.ChangeRequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PartVersionSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne<MasterPart>()
                  .WithMany()
                  .HasForeignKey(e => e.MasterPartId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.MasterPartId, e.VersionNumber });
        });
    }
}
```

### Step 2: Create Version Control Service

#### 2A: Create VersionControlService
**File: `Services/VersionControlService.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.VersionControl;
using OpCentrix.ViewModels.VersionControl;
using System.Text.Json;

namespace OpCentrix.Services
{
    public class VersionControlService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<VersionControlService> _logger;

        public VersionControlService(SchedulerContext context, ILogger<VersionControlService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // === CHANGE REQUEST MANAGEMENT ===

        public async Task<(bool Success, int RequestId, string Message)> CreateChangeRequestAsync(CreateChangeRequestDto dto)
        {
            try
            {
                var masterPart = await _context.MasterParts.FindAsync(dto.MasterPartId);
                if (masterPart == null)
                {
                    return (false, 0, "Master part not found");
                }

                var changeRequest = new PartChangeRequest
                {
                    MasterPartId = dto.MasterPartId,
                    RequestedByUserId = dto.RequestedByUserId,
                    ChangeDescription = dto.ChangeDescription,
                    ChangeReason = dto.ChangeReason,
                    Status = ChangeRequestStatus.Pending
                };

                _context.PartChangeRequests.Add(changeRequest);
                await _context.SaveChangesAsync();

                // Add field changes
                foreach (var fieldChange in dto.FieldChanges)
                {
                    var change = new PartFieldChange
                    {
                        ChangeRequestId = changeRequest.Id,
                        FieldName = fieldChange.FieldName,
                        OldValue = fieldChange.OldValue,
                        NewValue = fieldChange.NewValue,
                        ChangeType = fieldChange.ChangeType
                    };

                    _context.PartFieldChanges.Add(change);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Created change request {RequestId} for part {PartId} by user {UserId}",
                    changeRequest.Id, dto.MasterPartId, dto.RequestedByUserId);

                return (true, changeRequest.Id, "Change request created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating change request for part {PartId}", dto.MasterPartId);
                return (false, 0, $"Error creating change request: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ReviewChangeRequestAsync(int requestId, string reviewerId, bool approved, string comments)
        {
            try
            {
                var request = await _context.PartChangeRequests.FindAsync(requestId);
                if (request == null)
                {
                    return (false, "Change request not found");
                }

                if (request.Status != ChangeRequestStatus.Pending)
                {
                    return (false, $"Cannot review request in status: {request.Status}");
                }

                request.Status = approved ? ChangeRequestStatus.Approved : ChangeRequestStatus.Rejected;
                request.ReviewedByUserId = reviewerId;
                request.ReviewDate = DateTime.UtcNow;
                request.ReviewComments = comments;

                if (approved)
                {
                    request.ApprovedDate = DateTime.UtcNow;
                }
                else
                {
                    request.RejectedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Change request {RequestId} {Action} by user {ReviewerId}",
                    requestId, approved ? "approved" : "rejected", reviewerId);

                return (true, $"Change request {(approved ? "approved" : "rejected")} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing change request {RequestId}", requestId);
                return (false, $"Error reviewing change request: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ImplementChangeRequestAsync(int requestId, string implementerId)
        {
            try
            {
                var request = await _context.PartChangeRequests
                    .Include(r => r.FieldChanges)
                    .Include(r => r.MasterPart)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null || request.MasterPart == null)
                {
                    return (false, "Change request or master part not found");
                }

                if (request.Status != ChangeRequestStatus.Approved)
                {
                    return (false, $"Cannot implement request in status: {request.Status}");
                }

                // Create snapshot of current version
                await CreateVersionSnapshotAsync(request.MasterPart, request.ChangeDescription ?? "Change request implementation");

                // Create new version
                var newVersion = await CreateNewVersionAsync(request.MasterPart);

                // Apply field changes
                await ApplyFieldChangesAsync(newVersion, request.FieldChanges);

                // Update request status
                request.Status = ChangeRequestStatus.Implemented;
                request.ImplementedDate = DateTime.UtcNow;
                request.NewVersionNumber = newVersion.VersionNumber;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Implemented change request {RequestId} creating version {VersionNumber} by user {ImplementerId}",
                    requestId, newVersion.VersionNumber, implementerId);

                return (true, $"Change request implemented successfully. New version: {newVersion.VersionNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error implementing change request {RequestId}", requestId);
                return (false, $"Error implementing change request: {ex.Message}");
            }
        }

        // === VERSION MANAGEMENT ===

        public async Task<MasterPart> CreateNewVersionAsync(MasterPart currentPart)
        {
            // Mark current version as not latest
            currentPart.IsLatestVersion = false;

            // Create new version
            var newPart = new MasterPart
            {
                // Copy all properties from current part
                PartNumber = currentPart.PartNumber,
                PartName = currentPart.PartName,
                PartDescription = currentPart.PartDescription,
                Material = currentPart.Material,
                LengthMm = currentPart.LengthMm,
                WidthMm = currentPart.WidthMm,
                HeightMm = currentPart.HeightMm,
                WeightGrams = currentPart.WeightGrams,
                ComplexityLevel = currentPart.ComplexityLevel,
                HasInternalFeatures = currentPart.HasInternalFeatures,
                MinWallThicknessMm = currentPart.MinWallThicknessMm,
                MinFeatureSizeMm = currentPart.MinFeatureSizeMm,
                SurfaceFinishRequirement = currentPart.SurfaceFinishRequirement,
                ToleranceClass = currentPart.ToleranceClass,
                PostProcessingSteps = currentPart.PostProcessingSteps,
                QualityRequirements = currentPart.QualityRequirements,
                CustomerSpecifications = currentPart.CustomerSpecifications,
                
                // Version control fields
                VersionNumber = currentPart.VersionNumber + 1,
                IsLatestVersion = true,
                PreviousVersionId = currentPart.Id,
                
                // Timestamps
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                IsActive = true
            };

            _context.MasterParts.Add(newPart);
            await _context.SaveChangesAsync();

            return newPart;
        }

        public async Task CreateVersionSnapshotAsync(MasterPart part, string changeDescription)
        {
            var snapshotData = new
            {
                part.PartNumber,
                part.PartName,
                part.PartDescription,
                part.Material,
                part.LengthMm,
                part.WidthMm,
                part.HeightMm,
                part.WeightGrams,
                part.ComplexityLevel,
                part.HasInternalFeatures,
                part.MinWallThicknessMm,
                part.MinFeatureSizeMm,
                part.SurfaceFinishRequirement,
                part.ToleranceClass,
                part.PostProcessingSteps,
                part.QualityRequirements,
                part.CustomerSpecifications
            };

            var snapshot = new PartVersionSnapshot
            {
                MasterPartId = part.Id,
                VersionNumber = part.VersionNumber,
                SnapshotData = JsonSerializer.Serialize(snapshotData),
                CreatedByUserId = "system", // Could be passed as parameter
                ChangeDescription = changeDescription
            };

            _context.PartVersionSnapshots.Add(snapshot);
            await _context.SaveChangesAsync();
        }

        private async Task ApplyFieldChangesAsync(MasterPart part, ICollection<PartFieldChange> fieldChanges)
        {
            foreach (var change in fieldChanges)
            {
                var property = typeof(MasterPart).GetProperty(change.FieldName);
                if (property != null && property.CanWrite)
                {
                    object? newValue = null;

                    if (!string.IsNullOrEmpty(change.NewValue))
                    {
                        // Convert string value to appropriate type
                        newValue = change.FieldName switch
                        {
                            nameof(MasterPart.LengthMm) or nameof(MasterPart.WidthMm) or nameof(MasterPart.HeightMm) or
                            nameof(MasterPart.MinWallThicknessMm) or nameof(MasterPart.MinFeatureSizeMm) => 
                                decimal.Parse(change.NewValue),
                            nameof(MasterPart.WeightGrams) => int.Parse(change.NewValue),
                            nameof(MasterPart.HasInternalFeatures) => bool.Parse(change.NewValue),
                            _ => change.NewValue
                        };
                    }

                    property.SetValue(part, newValue);
                }
            }

            part.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // === QUERY METHODS ===

        public async Task<List<ChangeRequestSummaryViewModel>> GetPendingChangeRequestsAsync()
        {
            return await _context.PartChangeRequests
                .Where(r => r.Status == ChangeRequestStatus.Pending || r.Status == ChangeRequestStatus.UnderReview)
                .Include(r => r.MasterPart)
                .Select(r => new ChangeRequestSummaryViewModel
                {
                    Id = r.Id,
                    PartNumber = r.MasterPart!.PartNumber,
                    PartName = r.MasterPart.PartName,
                    ChangeDescription = r.ChangeDescription,
                    RequestedDate = r.RequestDate,
                    Status = r.StatusDisplay,
                    StatusClass = r.StatusClass
                })
                .OrderByDescending(r => r.RequestedDate)
                .ToListAsync();
        }

        public async Task<List<PartVersionHistoryViewModel>> GetPartVersionHistoryAsync(int masterPartId)
        {
            // Get all versions of this part
            var allVersions = await _context.MasterParts
                .Where(mp => mp.Id == masterPartId || mp.PreviousVersionId == masterPartId)
                .OrderByDescending(mp => mp.VersionNumber)
                .ToListAsync();

            // Get snapshots
            var snapshots = await _context.PartVersionSnapshots
                .Where(s => s.MasterPartId == masterPartId)
                .OrderByDescending(s => s.VersionNumber)
                .ToListAsync();

            var versionHistory = allVersions.Select(v => new PartVersionHistoryViewModel
            {
                Id = v.Id,
                VersionNumber = v.VersionNumber,
                CreatedDate = v.CreatedDate,
                IsLatestVersion = v.IsLatestVersion,
                ChangeDescription = snapshots.FirstOrDefault(s => s.VersionNumber == v.VersionNumber)?.ChangeDescription ?? "Version created"
            }).ToList();

            return versionHistory;
        }

        public async Task<PartComparisonViewModel?> CompareVersionsAsync(int version1Id, int version2Id)
        {
            var version1 = await _context.MasterParts.FindAsync(version1Id);
            var version2 = await _context.MasterParts.FindAsync(version2Id);

            if (version1 == null || version2 == null)
                return null;

            var comparison = new PartComparisonViewModel
            {
                Version1 = new PartVersionDetailViewModel
                {
                    Id = version1.Id,
                    VersionNumber = version1.VersionNumber,
                    PartNumber = version1.PartNumber,
                    PartName = version1.PartName,
                    CreatedDate = version1.CreatedDate
                },
                Version2 = new PartVersionDetailViewModel
                {
                    Id = version2.Id,
                    VersionNumber = version2.VersionNumber,
                    PartNumber = version2.PartNumber,
                    PartName = version2.PartName,
                    CreatedDate = version2.CreatedDate
                },
                Differences = FindDifferences(version1, version2)
            };

            return comparison;
        }

        private List<FieldDifferenceViewModel> FindDifferences(MasterPart part1, MasterPart part2)
        {
            var differences = new List<FieldDifferenceViewModel>();

            // Compare all relevant properties
            CompareField(differences, nameof(MasterPart.PartName), part1.PartName, part2.PartName);
            CompareField(differences, nameof(MasterPart.PartDescription), part1.PartDescription, part2.PartDescription);
            CompareField(differences, nameof(MasterPart.Material), part1.Material, part2.Material);
            CompareField(differences, nameof(MasterPart.LengthMm), part1.LengthMm?.ToString(), part2.LengthMm?.ToString());
            CompareField(differences, nameof(MasterPart.WidthMm), part1.WidthMm?.ToString(), part2.WidthMm?.ToString());
            CompareField(differences, nameof(MasterPart.HeightMm), part1.HeightMm?.ToString(), part2.HeightMm?.ToString());
            // ... add more field comparisons as needed

            return differences;
        }

        private void CompareField(List<FieldDifferenceViewModel> differences, string fieldName, string? value1, string? value2)
        {
            if (value1 != value2)
            {
                differences.Add(new FieldDifferenceViewModel
                {
                    FieldName = fieldName,
                    Version1Value = value1 ?? "",
                    Version2Value = value2 ?? ""
                });
            }
        }
    }
}
```

### Step 3: Create Version Control ViewModels

#### 3A: Create Version Control ViewModels
**File: `ViewModels/VersionControl/VersionControlViewModels.cs`**

```csharp
using OpCentrix.Models.VersionControl;

namespace OpCentrix.ViewModels.VersionControl
{
    public class CreateChangeRequestDto
    {
        public int MasterPartId { get; set; }
        public string RequestedByUserId { get; set; } = string.Empty;
        public string ChangeDescription { get; set; } = string.Empty;
        public string ChangeReason { get; set; } = string.Empty;
        public List<FieldChangeDto> FieldChanges { get; set; } = new();
    }

    public class FieldChangeDto
    {
        public string FieldName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public FieldChangeType ChangeType { get; set; }
    }

    public class ChangeRequestSummaryViewModel
    {
        public int Id { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string ChangeDescription { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
    }

    public class PartVersionHistoryViewModel
    {
        public int Id { get; set; }
        public int VersionNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsLatestVersion { get; set; }
        public string ChangeDescription { get; set; } = string.Empty;
    }

    public class PartComparisonViewModel
    {
        public PartVersionDetailViewModel Version1 { get; set; } = new();
        public PartVersionDetailViewModel Version2 { get; set; } = new();
        public List<FieldDifferenceViewModel> Differences { get; set; } = new();
    }

    public class PartVersionDetailViewModel
    {
        public int Id { get; set; }
        public int VersionNumber { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    public class FieldDifferenceViewModel
    {
        public string FieldName { get; set; } = string.Empty;
        public string Version1Value { get; set; } = string.Empty;
        public string Version2Value { get; set; } = string.Empty;
    }
}
```

### Step 4: Create Version Control UI

#### 4A: Create Change Request Page
**File: `Pages/Admin/ChangeRequests.cshtml`**

```html
@page "/Admin/ChangeRequests"
@model OpCentrix.Pages.Admin.ChangeRequestsModel
@{
    ViewData["Title"] = "Change Requests";
    Layout = "~/Pages/Shared/_Layout.cshtml";
}

<div class="container-fluid">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h2 class="mb-0">
                <i class="fas fa-edit me-2 text-primary"></i>
                Change Requests
            </h2>
            <p class="text-muted mb-0">Review and approve part changes</p>
        </div>
    </div>

    <!-- Pending Change Requests -->
    <div class="row">
        <div class="col-12">
            <div class="card">
                <div class="card-header bg-primary text-white">
                    <h5 class="mb-0">
                        <i class="fas fa-clock me-2"></i>
                        Pending Change Requests (@Model.PendingRequests.Count)
                    </h5>
                </div>
                <div class="card-body">
                    @if (Model.PendingRequests.Any())
                    {
                        <div class="table-responsive">
                            <table class="table table-hover">
                                <thead>
                                    <tr>
                                        <th>Part Number</th>
                                        <th>Part Name</th>
                                        <th>Change Description</th>
                                        <th>Requested Date</th>
                                        <th>Status</th>
                                        <th class="text-center">Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @foreach (var request in Model.PendingRequests)
                                    {
                                        <tr>
                                            <td class="fw-bold">@request.PartNumber</td>
                                            <td>@request.PartName</td>
                                            <td>@request.ChangeDescription</td>
                                            <td>@request.RequestedDate.ToString("MMM dd, yyyy HH:mm")</td>
                                            <td>
                                                <span class="badge bg-@request.StatusClass">@request.Status</span>
                                            </td>
                                            <td class="text-center">
                                                <div class="btn-group">
                                                    <button type="button" class="btn btn-sm btn-outline-info"
                                                            onclick="viewRequest(@request.Id)">
                                                        <i class="fas fa-eye me-1"></i>View
                                                    </button>
                                                    <button type="button" class="btn btn-sm btn-success"
                                                            onclick="reviewRequest(@request.Id, true)">
                                                        <i class="fas fa-check me-1"></i>Approve
                                                    </button>
                                                    <button type="button" class="btn btn-sm btn-danger"
                                                            onclick="reviewRequest(@request.Id, false)">
                                                        <i class="fas fa-times me-1"></i>Reject
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    }
                                </tbody>
                            </table>
                        </div>
                    }
                    else
                    {
                        <div class="text-center py-5">
                            <i class="fas fa-check-circle fa-3x text-success mb-3"></i>
                            <h5 class="text-muted">No Pending Change Requests</h5>
                            <p class="text-muted">All change requests have been reviewed.</p>
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>
</div>
```

#### 4B: Add Version History to MasterPart Form
**File: `Pages/Admin/Shared/_MasterPartForm.cshtml`**

Add version history section:

```html
<!-- Add this after the machine performance history section -->
@if (Model.Id > 0 && Model.VersionNumber > 1)
{
    <div class="card mb-4">
        <div class="card-header bg-secondary text-white d-flex justify-content-between align-items-center">
            <h6 class="mb-0">
                <i class="fas fa-history me-2"></i>
                Version History
            </h6>
            <small>Current Version: @Model.VersionNumber</small>
        </div>
        <div class="card-body">
            <div class="row mb-3">
                <div class="col-md-4">
                    <div class="text-center">
                        <div class="h5 text-info">@Model.VersionNumber</div>
                        <small class="text-muted">Current Version</small>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="text-center">
                        <div class="h5 text-secondary">@Model.CreatedDate.ToString("MMM dd, yyyy")</div>
                        <small class="text-muted">Version Created</small>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="text-center">
                        @if (Model.IsLatestVersion)
                        {
                            <span class="badge bg-success">Latest Version</span>
                        }
                        else
                        {
                            <span class="badge bg-warning">Historical Version</span>
                        }
                    </div>
                </div>
            </div>
            
            <div class="d-grid gap-2 d-md-flex justify-content-md-center">
                <button type="button" class="btn btn-outline-info" onclick="showVersionHistory(@Model.Id)">
                    <i class="fas fa-history me-1"></i>View Version History
                </button>
                @if (Model.PreviousVersionId.HasValue)
                {
                    <button type="button" class="btn btn-outline-secondary" onclick="compareVersions(@Model.Id, @Model.PreviousVersionId)">
                        <i class="fas fa-exchange-alt me-1"></i>Compare with Previous
                    </button>
                }
                <button type="button" class="btn btn-outline-warning" onclick="createChangeRequest(@Model.Id)">
                    <i class="fas fa-edit me-1"></i>Request Change
                </button>
            </div>
        </div>
    </div>
}

<script>
function showVersionHistory(partId) {
    // Load version history in modal
    fetch(`/api/version-control/history/${partId}`)
        .then(response => response.json())
        .then(data => {
            // Display version history modal
            showVersionHistoryModal(data);
        });
}

function compareVersions(version1Id, version2Id) {
    // Load version comparison
    window.location.href = `/Admin/CompareVersions?v1=${version1Id}&v2=${version2Id}`;
}

function createChangeRequest(partId) {
    // Show change request modal
    window.location.href = `/Admin/CreateChangeRequest?partId=${partId}`;
}
</script>
```

---

## Step 5: Register Services and Run Migration

#### 5A: Register Service
**File: `Program.cs`**

```csharp
// Add version control service
builder.Services.AddScoped<VersionControlService>();
```

#### 5B: Run Migration
```bash
sqlite3 scheduler.db < "Data/Migrations/AddVersionControlTables.sql"
```

---

## Step 6: Testing & Validation

### 6A: Test Change Request Workflow
1. **Create change request** - verify change tracking works
2. **Review requests** - test approval/rejection workflow  
3. **Implement changes** - verify new version creation
4. **Check version history** - confirm version tracking works

### 6B: Test Version Management
1. **Version snapshots** - verify data capture works
2. **Version comparison** - test difference detection
3. **Version rollback** - verify rollback functionality
4. **History display** - check version timeline

### 6C: Test Integration
1. **UI integration** - version controls appear in forms
2. **Permissions** - only authorized users can approve
3. **Audit trail** - all changes properly logged
4. **Data integrity** - version relationships maintained

---

## Success Criteria

- ? **Complete change request workflow** - create, review, approve, implement
- ? **Version history tracking** - full audit trail of changes
- ? **Version comparison tools** - difference detection and display
- ? **Approval workflow** - proper authorization and review process
- ? **Data integrity maintained** - version relationships preserved
- ? **UI integration complete** - version controls in master part forms
- ? **Audit logging comprehensive** - all changes tracked and logged

## Next Steps

After completing this plan:
1. Test version control workflow thoroughly
2. Verify all approval processes work correctly
3. Check audit trails and history tracking
4. Move to **Plan 11: Enhanced MasterPart Form**

---

**Status: READY FOR IMPLEMENTATION**  
**Next Plan: 11-Enhanced-MasterPart-Form.md**