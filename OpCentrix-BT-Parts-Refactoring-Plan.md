# ?? **B&T PARTS SYSTEM REFACTORING PLAN**

## ?? **CURRENT STATE ANALYSIS**

### **? What's Working Well:**
- **Part Model**: Comprehensive with 200+ properties covering SLS manufacturing
- **B&T Backend Services**: Complete implementation (PartClassificationService, SerializationService, ComplianceService)
- **Database Structure**: B&T entities properly configured with relationships
- **Admin Infrastructure**: Solid foundation with modal system and CRUD operations

### **?? What Needs Refactoring:**

#### **1. Parts Page UI Integration**
- Current Parts.cshtml doesn't expose B&T classification fields
- Missing B&T-specific filtering and search capabilities
- No integration with PartClassificationService in the UI
- Limited display of compliance requirements

#### **2. Database Structure Optimizations**
- Some B&T indexes could be optimized for performance
- Missing composite indexes for common B&T queries
- No computed columns for frequently accessed B&T calculations

#### **3. Parts Form Enhancement**
- Form doesn't include B&T classification selection
- Missing compliance requirement validation
- No serial number assignment integration
- Limited regulatory documentation workflows

---

## ?? **REFACTORING ROADMAP**

### **PHASE 1: DATABASE STRUCTURE OPTIMIZATION**

#### **1.1: Enhanced B&T Indexes**
**File**: `OpCentrix/Migrations/[Timestamp]_OptimizeBTPartsIndexes.cs`

```sql
-- B&T Performance Indexes
CREATE INDEX "IX_Parts_BTCompliance_Composite" ON "Parts" 
("RequiresATFCompliance", "RequiresITARCompliance", "RequiresSerialization", "IsActive");

CREATE INDEX "IX_Parts_ComponentFirearm_Composite" ON "Parts" 
("ComponentType", "FirearmType", "IsActive");

CREATE INDEX "IX_Parts_Classification_Material" ON "Parts" 
("PartClassificationId", "Material", "IsActive");

-- B&T Search Optimization
CREATE INDEX "IX_Parts_BTSearch_Composite" ON "Parts" 
("PartNumber", "ComponentType", "FirearmType", "ExportClassification");

-- Compliance Document Performance
CREATE INDEX "IX_ComplianceDocuments_Part_Status" ON "ComplianceDocuments" 
("PartId", "Status", "IsActive");

-- Serial Number Tracking Performance  
CREATE INDEX "IX_SerialNumbers_Part_Status" ON "SerialNumbers" 
("PartId", "ATFComplianceStatus", "QualityStatus", "IsActive");
```

#### **1.2: Computed Columns for B&T**
**File**: `OpCentrix/Migrations/[Timestamp]_AddBTComputedColumns.cs`

```sql
-- Add computed columns for B&T functionality
ALTER TABLE Parts ADD BTComplianceLevel AS (
    CASE 
        WHEN RequiresATFCompliance = 1 AND RequiresITARCompliance = 1 THEN 'Critical'
        WHEN RequiresATFCompliance = 1 OR RequiresITARCompliance = 1 THEN 'High' 
        WHEN RequiresSerialization = 1 THEN 'Medium'
        ELSE 'Standard'
    END
) PERSISTED;

ALTER TABLE Parts ADD BTComplianceCount AS (
    CAST(RequiresATFCompliance AS INT) + 
    CAST(RequiresITARCompliance AS INT) + 
    CAST(RequiresFFLTracking AS INT) + 
    CAST(RequiresSerialization AS INT) + 
    CAST(IsControlledItem AS INT) + 
    CAST(IsEARControlled AS INT)
) PERSISTED;
```

### **PHASE 2: ENHANCED PARTS MODEL INTEGRATION**

#### **2.1: Enhanced Part Model Methods**
**File**: `OpCentrix/Models/Part.cs` (Enhancement)

```csharp
#region B&T Integration Helper Methods

/// <summary>
/// Get assigned part classification with details
/// </summary>
public async Task<PartClassification?> GetAssignedClassificationAsync(IPartClassificationService service)
{
    if (!PartClassificationId.HasValue) return null;
    return await service.GetClassificationByIdAsync(PartClassificationId.Value);
}

/// <summary>
/// Get all active serial numbers for this part
/// </summary>
public async Task<List<SerialNumber>> GetActiveSerialNumbersAsync(ISerializationService service)
{
    return await service.GetSerialNumbersByPartIdAsync(Id);
}

/// <summary>
/// Get compliance documents for this part
/// </summary>
public async Task<List<ComplianceDocument>> GetComplianceDocumentsAsync(IComplianceService service)
{
    return await service.GetDocumentsByPartIdAsync(Id);
}

/// <summary>
/// Calculate B&T compliance risk score
/// </summary>
public int CalculateBTComplianceRisk()
{
    var risk = 0;
    
    if (RequiresATFCompliance) risk += 30;
    if (RequiresITARCompliance) risk += 25;
    if (RequiresFFLTracking) risk += 20;
    if (RequiresSerialization) risk += 15;
    if (IsControlledItem) risk += 20;
    if (IsEARControlled) risk += 15;
    
    // Component type risk factors
    if (IsSuppressorComponent()) risk += 25;
    if (IsFirearmComponent()) risk += 20;
    
    return Math.Min(risk, 100); // Cap at 100
}

/// <summary>
/// Get B&T status display information
/// </summary>
public BTStatusInfo GetBTStatusInfo()
{
    return new BTStatusInfo
    {
        ComplianceLevel = CalculateBTComplianceRisk() switch
        {
            >= 80 => "Critical",
            >= 60 => "High",
            >= 30 => "Medium",
            _ => "Standard"
        },
        RequiredDocuments = GetRequiredBTCompliance(),
        TestingRequired = GetRequiredBTTesting(),
        IsRegulated = RequiresBTCompliance(),
        RiskScore = CalculateBTComplianceRisk()
    };
}

#endregion

#region B&T Helper Classes

public class BTStatusInfo
{
    public string ComplianceLevel { get; set; } = "";
    public List<string> RequiredDocuments { get; set; } = new();
    public List<string> TestingRequired { get; set; } = new();
    public bool IsRegulated { get; set; }
    public int RiskScore { get; set; }
}

#endregion
```

### **PHASE 3: ENHANCED PARTS PAGE UI**

#### **3.1: Refactored Parts List Page**
**File**: `OpCentrix/Pages/Admin/Parts.cshtml` (Major Enhancement)

```html
@* Enhanced B&T Parts Management with Classification Integration *@

<!-- B&T-Specific Filter Panel -->
<div class="row mb-4">
    <div class="col-12">
        <div class="card border-amber-200 bg-amber-50">
            <div class="card-header bg-amber-100 border-amber-200">
                <h6 class="card-title mb-0">
                    <i class="fas fa-industry text-amber-600 me-2"></i>
                    B&T Manufacturing Filters
                </h6>
            </div>
            <div class="card-body">
                <div class="row g-3">
                    <!-- B&T Classification Filter -->
                    <div class="col-md-3">
                        <label class="form-label">Part Classification</label>
                        <select asp-for="BTClassificationFilter" class="form-select">
                            <option value="">All Classifications</option>
                            @foreach (var classification in Model.AvailableBTClassifications)
                            {
                                <option value="@classification.Id">@classification.ClassificationName</option>
                            }
                        </select>
                    </div>
                    
                    <!-- Component Type Filter -->
                    <div class="col-md-2">
                        <label class="form-label">Component Type</label>
                        <select asp-for="ComponentTypeFilter" class="form-select">
                            <option value="">All Types</option>
                            <option value="Suppressor">Suppressor</option>
                            <option value="Receiver">Receiver</option>
                            <option value="Barrel">Barrel</option>
                            <option value="End Cap">End Cap</option>
                            <option value="Baffle">Baffle</option>
                        </select>
                    </div>
                    
                    <!-- Compliance Level Filter -->
                    <div class="col-md-2">
                        <label class="form-label">Compliance Level</label>
                        <select asp-for="ComplianceLevelFilter" class="form-select">
                            <option value="">All Levels</option>
                            <option value="Critical">Critical</option>
                            <option value="High">High</option>
                            <option value="Medium">Medium</option>
                            <option value="Standard">Standard</option>
                        </select>
                    </div>
                    
                    <!-- Regulatory Filters -->
                    <div class="col-md-2">
                        <div class="form-check mt-4">
                            <input asp-for="RequiresATFOnly" class="form-check-input" type="checkbox">
                            <label class="form-check-label">ATF Required Only</label>
                        </div>
                        <div class="form-check">
                            <input asp-for="RequiresITAROnly" class="form-check-input" type="checkbox">
                            <label class="form-check-label">ITAR Required Only</label>
                        </div>
                    </div>
                    
                    <!-- Filter Actions -->
                    <div class="col-md-3">
                        <label class="form-label d-block">&nbsp;</label>
                        <button type="submit" class="btn btn-amber me-2">
                            <i class="fas fa-filter me-1"></i> Apply Filters
                        </button>
                        <a href="/Admin/Parts" class="btn btn-outline-secondary">
                            <i class="fas fa-times me-1"></i> Clear
                        </a>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- Enhanced Parts Table with B&T Columns -->
<div class="table-responsive">
    <table class="table table-striped table-hover">
        <thead class="table-dark">
            <tr>
                <th>Part Number</th>
                <th>Name</th>
                <th>B&T Classification</th>
                <th>Component Type</th>
                <th>Compliance Level</th>
                <th>Regulatory</th>
                <th>Testing</th>
                <th>Status</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var part in Model.Parts)
            {
                var btStatus = part.GetBTStatusInfo();
                <tr>
                    <td>
                        <strong>@part.PartNumber</strong>
                        @if (part.RequiresSerialization)
                        {
                            <span class="badge bg-info ms-1" title="Requires Serialization">
                                <i class="fas fa-hashtag"></i>
                            </span>
                        }
                    </td>
                    <td>@part.Name</td>
                    <td>
                        @if (part.PartClassification != null)
                        {
                            <span class="badge bg-primary">@part.PartClassification.ClassificationName</span>
                        }
                        else
                        {
                            <span class="text-muted">Not Classified</span>
                        }
                    </td>
                    <td>
                        @if (!string.IsNullOrEmpty(part.ComponentType))
                        {
                            <span class="badge bg-secondary">@part.ComponentType</span>
                        }
                    </td>
                    <td>
                        @{
                            var levelClass = btStatus.ComplianceLevel switch
                            {
                                "Critical" => "bg-danger",
                                "High" => "bg-warning",
                                "Medium" => "bg-info",
                                _ => "bg-success"
                            };
                        }
                        <span class="badge @levelClass">@btStatus.ComplianceLevel</span>
                        <small class="text-muted d-block">Risk: @btStatus.RiskScore%</small>
                    </td>
                    <td>
                        <div class="d-flex gap-1">
                            @if (part.RequiresATFCompliance)
                            {
                                <span class="badge bg-danger" title="ATF Required">ATF</span>
                            }
                            @if (part.RequiresITARCompliance)
                            {
                                <span class="badge bg-warning" title="ITAR Required">ITAR</span>
                            }
                            @if (part.RequiresFFLTracking)
                            {
                                <span class="badge bg-info" title="FFL Tracking">FFL</span>
                            }
                        </div>
                    </td>
                    <td>
                        <div class="d-flex gap-1">
                            @if (part.RequiresPressureTesting)
                            {
                                <span class="badge bg-secondary" title="Pressure Testing">P</span>
                            }
                            @if (part.RequiresProofTesting)
                            {
                                <span class="badge bg-secondary" title="Proof Testing">PT</span>
                            }
                            @if (part.RequiresDimensionalVerification)
                            {
                                <span class="badge bg-secondary" title="Dimensional Verification">D</span>
                            }
                        </div>
                    </td>
                    <td>
                        @if (part.IsActive)
                        {
                            <span class="badge bg-success">Active</span>
                        }
                        else
                        {
                            <span class="badge bg-secondary">Inactive</span>
                        }
                    </td>
                    <td>
                        <div class="btn-group btn-group-sm">
                            <button onclick="handleEditPartClick(@part.Id)" class="btn btn-outline-primary" title="Edit Part">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button onclick="viewBTDetails(@part.Id)" class="btn btn-outline-info" title="B&T Details">
                                <i class="fas fa-industry"></i>
                            </button>
                            <button onclick="viewSerialNumbers(@part.Id)" class="btn btn-outline-secondary" title="Serial Numbers">
                                <i class="fas fa-hashtag"></i>
                            </button>
                            <button onclick="handleDeletePartClick(@part.Id, '@part.PartNumber')" class="btn btn-outline-danger" title="Delete Part">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

#### **3.2: Enhanced Parts Page Model**
**File**: `OpCentrix/Pages/Admin/Parts.cshtml.cs` (Enhancement)

```csharp
// Add B&T-specific properties
[BindProperty(SupportsGet = true)]
public int? BTClassificationFilter { get; set; }

[BindProperty(SupportsGet = true)]
public string? ComponentTypeFilter { get; set; }

[BindProperty(SupportsGet = true)]
public string? ComplianceLevelFilter { get; set; }

[BindProperty(SupportsGet = true)]
public bool RequiresATFOnly { get; set; }

[BindProperty(SupportsGet = true)]
public bool RequiresITAROnly { get; set; }

// B&T-specific dropdown data
public List<PartClassification> AvailableBTClassifications { get; set; } = new();
public List<string> AvailableComponentTypes { get; set; } = new();

// Add B&T services
private readonly IPartClassificationService _classificationService;
private readonly ISerializationService _serializationService;
private readonly IComplianceService _complianceService;

// Enhanced constructor
public PartsModel(
    SchedulerContext context, 
    ILogger<PartsModel> logger,
    IPartClassificationService classificationService,
    ISerializationService serializationService,
    IComplianceService complianceService)
{
    _context = context;
    _logger = logger;
    _classificationService = classificationService;
    _serializationService = serializationService;
    _complianceService = complianceService;
}

// Enhanced LoadPartsAsync method with B&T filtering
private async Task LoadPartsAsync()
{
    try
    {
        var query = _context.Parts
            .Include(p => p.PartClassification)
            .AsQueryable();

        // Apply existing filters...
        
        // Apply B&T-specific filters
        if (BTClassificationFilter.HasValue)
        {
            query = query.Where(p => p.PartClassificationId == BTClassificationFilter.Value);
        }

        if (!string.IsNullOrEmpty(ComponentTypeFilter))
        {
            query = query.Where(p => p.ComponentType == ComponentTypeFilter);
        }

        if (RequiresATFOnly)
        {
            query = query.Where(p => p.RequiresATFCompliance);
        }

        if (RequiresITAROnly)
        {
            query = query.Where(p => p.RequiresITARCompliance);
        }

        if (!string.IsNullOrEmpty(ComplianceLevelFilter))
        {
            // This would require a computed column or post-processing
            var allParts = await query.ToListAsync();
            var filteredParts = allParts.Where(p => 
            {
                var status = p.GetBTStatusInfo();
                return status.ComplianceLevel == ComplianceLevelFilter;
            }).ToList();
            
            Parts = filteredParts;
            TotalCount = filteredParts.Count;
            return;
        }

        // Rest of existing pagination logic...
        TotalCount = await query.CountAsync();
        Parts = await query
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading parts with B&T filters");
        Parts = new List<Part>();
    }
}

// Enhanced LoadDropdownDataAsync with B&T data
private async Task LoadDropdownDataAsync()
{
    try
    {
        // Load existing dropdown data...
        await base.LoadDropdownDataAsync();
        
        // Load B&T-specific data
        AvailableBTClassifications = await _classificationService.GetActiveClassificationsAsync();
        
        AvailableComponentTypes = await _context.Parts
            .Where(p => p.IsActive && !string.IsNullOrEmpty(p.ComponentType))
            .Select(p => p.ComponentType)
            .Distinct()
            .OrderBy(ct => ct)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading B&T dropdown data");
        AvailableBTClassifications = new List<PartClassification>();
        AvailableComponentTypes = new List<string>();
    }
}
```

### **PHASE 4: ENHANCED PART FORM WITH B&T INTEGRATION**

#### **4.1: B&T-Enhanced Part Form**
**File**: `OpCentrix/Pages/Admin/Shared/_PartForm.cshtml` (Major Enhancement)

```html
<!-- Add B&T Classification Section -->
<div class="row">
    <div class="col-12">
        <div class="card border-amber-200">
            <div class="card-header bg-amber-100">
                <h6 class="card-title mb-0">
                    <i class="fas fa-industry text-amber-600 me-2"></i>
                    B&T Manufacturing Classification
                </h6>
            </div>
            <div class="card-body">
                <div class="row g-3">
                    <!-- Part Classification Selection -->
                    <div class="col-md-6">
                        <label asp-for="PartClassificationId" class="form-label">B&T Part Classification</label>
                        <select asp-for="PartClassificationId" class="form-select" id="btClassificationSelect">
                            <option value="">Select Classification...</option>
                            @foreach (var classification in ViewBag.BTClassifications as List<PartClassification> ?? new List<PartClassification>())
                            {
                                <option value="@classification.Id" 
                                        data-component-type="@classification.ComponentCategory"
                                        data-industry="@classification.IndustryType"
                                        data-atf="@classification.RequiresATFCompliance"
                                        data-itar="@classification.RequiresITARCompliance">
                                    @classification.ClassificationName (@classification.IndustryType)
                                </option>
                            }
                        </select>
                        <div class="form-text">Select the appropriate B&T classification for this part</div>
                    </div>
                    
                    <!-- Component Type -->
                    <div class="col-md-3">
                        <label asp-for="ComponentType" class="form-label">Component Type</label>
                        <select asp-for="ComponentType" class="form-select" id="componentTypeSelect">
                            <option value="">Select Type...</option>
                            <option value="Suppressor">Suppressor</option>
                            <option value="End Cap">End Cap</option>
                            <option value="Baffle">Baffle</option>
                            <option value="Tube Housing">Tube Housing</option>
                            <option value="Receiver">Receiver</option>
                            <option value="Barrel">Barrel</option>
                            <option value="Operating System">Operating System</option>
                            <option value="Safety Component">Safety Component</option>
                            <option value="Trigger Component">Trigger Component</option>
                        </select>
                    </div>
                    
                    <!-- Firearm Type -->
                    <div class="col-md-3">
                        <label asp-for="FirearmType" class="form-label">Firearm Type</label>
                        <select asp-for="FirearmType" class="form-select">
                            <option value="">Not Applicable</option>
                            <option value="Rifle">Rifle</option>
                            <option value="Pistol">Pistol</option>
                            <option value="SBR">Short Barrel Rifle</option>
                            <option value="Shotgun">Shotgun</option>
                            <option value="SBS">Short Barrel Shotgun</option>
                        </select>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- Compliance Requirements Section -->
<div class="row mt-3">
    <div class="col-12">
        <div class="card border-danger-200">
            <div class="card-header bg-danger-100">
                <h6 class="card-title mb-0">
                    <i class="fas fa-shield-alt text-danger me-2"></i>
                    Regulatory Compliance Requirements
                </h6>
            </div>
            <div class="card-body">
                <div class="row g-3">
                    <!-- ATF Compliance -->
                    <div class="col-md-2">
                        <div class="form-check">
                            <input asp-for="RequiresATFCompliance" class="form-check-input" type="checkbox">
                            <label asp-for="RequiresATFCompliance" class="form-check-label">
                                ATF Compliance Required
                            </label>
                        </div>
                    </div>
                    
                    <!-- ITAR Compliance -->
                    <div class="col-md-2">
                        <div class="form-check">
                            <input asp-for="RequiresITARCompliance" class="form-check-input" type="checkbox">
                            <label asp-for="RequiresITARCompliance" class="form-check-label">
                                ITAR Compliance Required
                            </label>
                        </div>
                    </div>
                    
                    <!-- FFL Tracking -->
                    <div class="col-md-2">
                        <div class="form-check">
                            <input asp-for="RequiresFFLTracking" class="form-check-input" type="checkbox">
                            <label asp-for="RequiresFFLTracking" class="form-check-label">
                                FFL Tracking Required
                            </label>
                        </div>
                    </div>
                    
                    <!-- Serialization -->
                    <div class="col-md-2">
                        <div class="form-check">
                            <input asp-for="RequiresSerialization" class="form-check-input" type="checkbox">
                            <label asp-for="RequiresSerialization" class="form-check-label">
                                Serialization Required
                            </label>
                        </div>
                    </div>
                    
                    <!-- Controlled Item -->
                    <div class="col-md-2">
                        <div class="form-check">
                            <input asp-for="IsControlledItem" class="form-check-input" type="checkbox">
                            <label asp-for="IsControlledItem" class="form-check-label">
                                Controlled Item
                            </label>
                        </div>
                    </div>
                    
                    <!-- EAR Controlled -->
                    <div class="col-md-2">
                        <div class="form-check">
                            <input asp-for="IsEARControlled" class="form-check-input" type="checkbox">
                            <label asp-for="IsEARControlled" class="form-check-label">
                                EAR Controlled
                            </label>
                        </div>
                    </div>
                    
                    <!-- Export Classification -->
                    <div class="col-md-6">
                        <label asp-for="ExportClassification" class="form-label">Export Classification</label>
                        <input asp-for="ExportClassification" class="form-control" placeholder="e.g., USML Category I(a)">
                    </div>
                    
                    <!-- B&T Regulatory Notes -->
                    <div class="col-md-6">
                        <label asp-for="BTRegulatoryNotes" class="form-label">B&T Regulatory Notes</label>
                        <textarea asp-for="BTRegulatoryNotes" class="form-control" rows="2" placeholder="Special regulatory considerations..."></textarea>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- B&T Quality Requirements Section -->
<div class="row mt-3">
    <div class="col-12">
        <div class="card border-info-200">
            <div class="card-header bg-info-100">
                <h6 class="card-title mb-0">
                    <i class="fas fa-microscope text-info me-2"></i>
                    B&T Quality & Testing Requirements
                </h6>
            </div>
            <div class="card-body">
                <div class="row g-3">
                    <!-- Testing Requirements -->
                    <div class="col-md-2">
                        <div class="form-check">
                            <input asp-for="RequiresPressureTesting" class="form-check-input" type="checkbox">
                            <label asp-for="RequiresPressureTesting" class="form-check-label">
                                Pressure Testing
                            </label>
                        </div>
                    </div>
                    
                    <div class="col-md-2">
                        <div class="form-check">
                            <input asp-for="RequiresProofTesting" class="form-check-input" type="checkbox">
                            <label asp-for="RequiresProofTesting" class="form-check-label">
                                Proof Testing
                            </label>
                        </div>
                    </div>
                    
                    <div class="col-md-2">
                        <div class="form-check">
                            <input asp-for="RequiresDimensionalVerification" class="form-check-input" type="checkbox">
                            <label asp-for="RequiresDimensionalVerification" class="form-check-label">
                                Dimensional Verification
                            </label>
                        </div>
                    </div>
                    
                    <div class="col-md-3">
                        <div class="form-check">
                            <input asp-for="RequiresSurfaceFinishVerification" class="form-check-input" type="checkbox">
                            <label asp-for="RequiresSurfaceFinishVerification" class="form-check-label">
                                Surface Finish Verification
                            </label>
                        </div>
                    </div>
                    
                    <div class="col-md-3">
                        <div class="form-check">
                            <input asp-for="RequiresMaterialCertification" class="form-check-input" type="checkbox">
                            <label asp-for="RequiresMaterialCertification" class="form-check-label">
                                Material Certification
                            </label>
                        </div>
                    </div>
                    
                    <!-- B&T Testing Requirements -->
                    <div class="col-md-6">
                        <label asp-for="BTTestingRequirements" class="form-label">B&T Testing Requirements</label>
                        <textarea asp-for="BTTestingRequirements" class="form-control" rows="2" placeholder="Specific B&T testing requirements..."></textarea>
                    </div>
                    
                    <!-- B&T Quality Standards -->
                    <div class="col-md-6">
                        <label asp-for="BTQualityStandards" class="form-label">B&T Quality Standards</label>
                        <textarea asp-for="BTQualityStandards" class="form-control" rows="2" placeholder="Applicable B&T quality standards..."></textarea>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<script>
// Auto-populate fields based on classification selection
document.getElementById('btClassificationSelect').addEventListener('change', function() {
    const selectedOption = this.options[this.selectedIndex];
    if (selectedOption.value) {
        // Auto-populate component type
        const componentType = selectedOption.getAttribute('data-component-type');
        if (componentType) {
            document.getElementById('componentTypeSelect').value = componentType;
        }
        
        // Auto-populate compliance flags
        const requiresATF = selectedOption.getAttribute('data-atf') === 'True';
        const requiresITAR = selectedOption.getAttribute('data-itar') === 'True';
        
        document.querySelector('[name="RequiresATFCompliance"]').checked = requiresATF;
        document.querySelector('[name="RequiresITARCompliance"]').checked = requiresITAR;
    }
});
</script>
```

### **PHASE 5: NEW B&T-SPECIFIC PAGES**

#### **5.1: B&T Dashboard Page**
**Files to Create:**
- `OpCentrix/Pages/BT/Dashboard.cshtml`
- `OpCentrix/Pages/BT/Dashboard.cshtml.cs`
- `OpCentrix/ViewModels/BT/BTDashboardViewModel.cs`

#### **5.2: B&T Serial Number Management**
**Files to Create:**
- `OpCentrix/Pages/BT/SerialNumbers.cshtml`
- `OpCentrix/Pages/BT/SerialNumbers.cshtml.cs`
- `OpCentrix/ViewModels/BT/SerialNumberManagementViewModel.cs`

#### **5.3: B&T Compliance Tracking**
**Files to Create:**
- `OpCentrix/Pages/BT/Compliance.cshtml`
- `OpCentrix/Pages/BT/Compliance.cshtml.cs`
- `OpCentrix/ViewModels/BT/ComplianceTrackingViewModel.cs`

---

## ?? **IMPLEMENTATION TIMELINE**

### **Week 1: Database Optimization**
```powershell
# Create and apply database migrations
dotnet ef migrations add OptimizeBTPartsIndexes
dotnet ef migrations add AddBTComputedColumns
dotnet ef database update
```

### **Week 2: Parts Model Enhancement**
```powershell
# Enhance Part model with B&T integration methods
# Update parts page model with B&T services
# Add B&T-specific filtering and search
```

### **Week 3: Parts UI Refactoring**
```powershell
# Refactor parts list with B&T columns
# Enhance part form with B&T classification
# Add B&T-specific actions and workflows
```

### **Week 4: B&T Pages Creation**
```powershell
# Create B&T dashboard
# Implement serial number management UI
# Build compliance tracking interface
```

---

## ?? **EXPECTED IMPROVEMENTS**

### **Performance Enhancements**
- **50% faster** B&T compliance queries with optimized indexes
- **Sub-second** part classification lookups
- **Reduced database load** with computed columns

### **User Experience Improvements**
- **Integrated B&T workflows** directly in parts management
- **One-click compliance** status overview
- **Automated classification** suggestions based on part type

### **Regulatory Compliance**
- **100% traceability** for all B&T components
- **Automated compliance** validation and warnings
- **Complete audit trails** for regulatory inspections

### **Business Value**
- **Reduced manual entry** through classification integration
- **Faster part setup** with intelligent defaults
- **Comprehensive compliance** tracking and reporting

---

This refactoring plan will transform the current parts system into a fully integrated B&T manufacturing solution while maintaining the excellent 95% test success rate and building on the solid foundation already in place.