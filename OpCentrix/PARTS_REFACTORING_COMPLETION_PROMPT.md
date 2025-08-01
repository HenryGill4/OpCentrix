# ?? OpCentrix Parts Page Refactoring Prompt

## ?? **CONTEXT AND BACKGROUND**

You are working with the OpCentrix manufacturing management system that has been partially refactored to use a normalized database structure for parts and manufacturing stages. The system now uses:

- **Parts table**: Simplified, focused on essential part information
- **ProductionStages table**: Defines available manufacturing stages
- **PartStageRequirement table**: Links parts with their required stages (NEW)
- **PartStageService**: Service layer for managing part-stage relationships

## ?? **CURRENT STATE**

The following components have been implemented:

### ? **Completed Components**
1. **Models**:
   - `PartStageRequirement.cs` - Links parts to required stages
   - Enhanced `Part.cs` with stage-related helper methods
   - `StageIndicator.cs` - Helper for UI display

2. **Services**:
   - `PartStageService.cs` - Complete service for stage management
   - `IPartStageService` interface with full CRUD operations

3. **Database Changes**:
   - New `PartStageRequirements` table added to SchedulerContext
   - Migration script: `Migrate-PartsTableRefactoring.ps1`

4. **UI Components**:
   - `_PartStagesManager.cstml` - Stage management interface
   - Updated `_PartForm.cshtml` with new Manufacturing Stages tab
   - Enhanced `Parts.cshtml.cs` with stage service integration

5. **Configuration**:
   - Service registration in `Program.cs`
   - Database context updated

## ?? **REFACTORING TASKS NEEDED**

### **Priority 1: Complete Database Integration**

```powershell
# Run the migration to create PartStageRequirement table
.\Migrate-PartsTableRefactoring.ps1

# Verify database schema
dotnet ef migrations list --project OpCentrix
```

**Tasks:**
1. Execute the migration script to create the new table structure
2. Create default ProductionStages if they don't exist
3. Test database connectivity and relationships

### **Priority 2: Update Parts List Display**

**File**: `OpCentrix/Pages/Admin/Parts.cshtml`

**Current Issue**: The parts list still shows the old structure without stage indicators.

**Required Changes**:
1. **Add Stage Indicators Column**:
   ```html
   <th>
       <i class="fas fa-tasks me-1"></i>
       Required Stages
   </th>
   ```

2. **Display Stage Badges**:
   ```html
   <td>
       <div class="stage-indicators d-flex flex-wrap gap-1">
           @foreach (var indicator in await ViewBag.PartStageService.GetStageIndicatorsAsync(part.Id))
           {
               <span class="badge @indicator.Class" title="@indicator.Name">
                   <i class="@indicator.Icon me-1"></i>@indicator.Name
               </span>
           }
       </div>
   </td>
   ```

3. **Add Complexity and Time Columns**:
   ```html
   <th>Total Time</th>
   <th>Complexity</th>
   ```

### **Priority 3: Enhance Filtering and Search**

**File**: `OpCentrix/Pages/Admin/Parts.cshtml.cs`

**Add New Filter Properties**:
```csharp
[BindProperty(SupportsGet = true)]
public string? StageFilter { get; set; }

[BindProperty(SupportsGet = true)]
public string? ComplexityFilter { get; set; }

public Dictionary<string, int> StageUsageStats { get; set; } = new();
```

**Update LoadPartsDataAsync Method**:
```csharp
// Add stage filtering
if (!string.IsNullOrEmpty(StageFilter))
{
    var partsWithStage = await _context.PartStageRequirements
        .Include(psr => psr.ProductionStage)
        .Where(psr => psr.ProductionStage.Name.Contains(StageFilter) && psr.IsActive)
        .Select(psr => psr.PartId)
        .ToListAsync();
    
    query = query.Where(p => partsWithStage.Contains(p.Id));
}
```

### **Priority 4: Fix Stage Manager Integration**

**File**: `OpCentrix/Pages/Admin/Shared/_PartStagesManager.cstml`

**Current Issues**:
1. File extension should be `.cshtml` not `.cstml`
2. Needs proper integration with the PartStageService
3. JavaScript functions need API endpoints

**Required Changes**:
1. **Rename file**: `_PartStagesManager.cshtml`
2. **Update JavaScript functions**:
   ```javascript
   async function addPartStage() {
       const response = await fetch('/Admin/Parts?handler=AddStage', {
           method: 'POST',
           headers: {
               'Content-Type': 'application/json',
               'RequestVerificationToken': getAntiForgeryToken()
           },
           body: JSON.stringify({
               partId: getPartId(),
               stageId: getSelectedStageId(),
               executionOrder: getExecutionOrder()
           })
       });
       
       const result = await response.json();
       if (result.success) {
           refreshStageDisplay();
           showToast('success', result.message);
       }
   }
   ```

### **Priority 5: Create Default Data Setup**

**Create**: `OpCentrix/Data/DefaultProductionStages.sql`

```sql
-- Default Production Stages for OpCentrix
INSERT OR IGNORE INTO ProductionStages (Name, DisplayOrder, Description, DefaultSetupMinutes, DefaultHourlyRate, RequiresQualityCheck, RequiresApproval, AllowSkip, IsOptional, CreatedDate, IsActive)
VALUES 
('SLS Printing', 1, 'Selective Laser Sintering metal printing', 45, 85.00, 1, 0, 0, 0, datetime('now'), 1),
('CNC Machining', 2, 'Computer Numerical Control machining', 30, 95.00, 1, 0, 0, 1, datetime('now'), 1),
('EDM Operations', 3, 'Electrical Discharge Machining', 60, 110.00, 1, 1, 0, 1, datetime('now'), 1),
('Assembly', 4, 'Multi-component assembly operations', 15, 75.00, 1, 0, 0, 1, datetime('now'), 1),
('Finishing', 5, 'Surface finishing and coating', 20, 70.00, 1, 0, 0, 1, datetime('now'), 1),
('Quality Inspection', 6, 'Final quality control and testing', 10, 80.00, 1, 1, 0, 0, datetime('now'), 1);
```

### **Priority 6: Add API Endpoints**

**File**: `OpCentrix/Controllers/Api/PartsController.cs` (CREATE NEW)

```csharp
[ApiController]
[Route("api/[controller]")]
public class PartsController : ControllerBase
{
    private readonly IPartStageService _partStageService;
    
    [HttpGet("{partId}/stages")]
    public async Task<IActionResult> GetPartStages(int partId)
    {
        var stages = await _partStageService.GetPartStagesWithDetailsAsync(partId);
        return Ok(stages);
    }
    
    [HttpPost("{partId}/stages")]
    public async Task<IActionResult> AddPartStage(int partId, [FromBody] AddStageRequest request)
    {
        // Implementation
    }
}
```

### **Priority 7: Testing and Validation**

**Create Test Plan**:
1. **Database Tests**:
   ```powershell
   # Test migration
   dotnet ef database update --project OpCentrix
   
   # Verify tables exist
   sqlite3 scheduler.db ".tables" | grep -E "(PartStageRequirements|ProductionStages)"
   ```

2. **Functionality Tests**:
   - Create new part with stages
   - Edit existing part stages
   - Delete part stage requirements
   - Filter parts by stages
   - Test stage complexity calculations

3. **UI Tests**:
   - Manufacturing Stages tab functionality
   - Stage indicator display
   - Filter and search operations
   - Mobile responsiveness

## ?? **IMPLEMENTATION SEQUENCE**

### **Day 1: Database Setup**
```powershell
# Execute migration
.\Migrate-PartsTableRefactoring.ps1

# Verify schema
dotnet ef migrations list --project OpCentrix

# Test application startup
dotnet run --project OpCentrix
```

### **Day 2: UI Integration**
1. Fix `_PartStagesManager.cstml` ? `.cshtml`
2. Update Parts.cshtml with stage columns
3. Test stage management interface

### **Day 3: Service Integration**
1. Complete Parts.cshtml.cs stage methods
2. Add API endpoints for AJAX operations
3. Test stage CRUD operations

### **Day 4: Enhanced Features**
1. Add stage filtering to parts list
2. Implement complexity calculations
3. Add usage statistics

### **Day 5: Testing and Polish**
1. Comprehensive testing
2. Performance optimization
3. Documentation updates
4. User acceptance testing

## ?? **VALIDATION CHECKLIST**

### ? **Database Validation**
- [ ] PartStageRequirements table exists
- [ ] Foreign key relationships work
- [ ] ProductionStages have default data
- [ ] Migration runs without errors

### ? **Parts Form Validation**
- [ ] Manufacturing Stages tab appears
- [ ] Can add stages to new parts
- [ ] Can edit existing part stages
- [ ] Stage order and parameters save correctly

### ? **Parts List Validation**
- [ ] Stage indicators display correctly
- [ ] Complexity levels show properly
- [ ] Total time calculations work
- [ ] Stage filtering functions

### ? **Integration Validation**
- [ ] Service injection works
- [ ] API endpoints respond
- [ ] JavaScript functions execute
- [ ] Error handling works properly

## ?? **SUCCESS CRITERIA**

**The refactoring is complete when:**
1. ? Database schema updated successfully
2. ? Parts can be created with stage requirements
3. ? Parts list shows stage indicators and complexity
4. ? Stage management interface fully functional
5. ? Filtering and search include stage options
6. ? All existing functionality preserved
7. ? Performance meets or exceeds current system
8. ? User documentation updated

## ?? **POTENTIAL ISSUES AND SOLUTIONS**

### **Issue**: Migration fails with foreign key constraints
**Solution**: 
```sql
PRAGMA foreign_keys = OFF;
-- Run migration
PRAGMA foreign_keys = ON;
```

### **Issue**: Service not injected properly
**Solution**: Verify `Program.cs` registration:
```csharp
builder.Services.AddScoped<IPartStageService, PartStageService>();
```

### **Issue**: JavaScript functions not working
**Solution**: Check antiforgery token and API endpoints are correctly configured

### **Issue**: Stage indicators not displaying
**Solution**: Ensure ViewBag data is loaded in controller methods

## ?? **EXAMPLE PROMPTS FOR IMPLEMENTATION**

### **For Database Work**:
"Execute the Parts table refactoring migration. Create the PartStageRequirement table with proper foreign key relationships to Parts and ProductionStages. Include audit fields and indexes for performance. Verify the migration runs successfully and create default ProductionStages data."

### **For UI Updates**:
"Update the Parts list page to display stage indicators as colored badges for each part. Add columns for total estimated time and complexity level. Implement filtering by required stages and complexity. Ensure the display is responsive and user-friendly."

### **For Service Integration**:
"Complete the integration of PartStageService into the Parts controller. Implement AJAX endpoints for adding, editing, and removing part stage requirements. Ensure proper error handling and user feedback. Test all CRUD operations work correctly."

## ?? **COMPLETION VERIFICATION**

Run this final test sequence to verify the refactoring is complete:

```powershell
# Build and test
dotnet build OpCentrix
dotnet test OpCentrix.Tests

# Database verification
sqlite3 scheduler.db "SELECT name FROM sqlite_master WHERE type='table' AND name LIKE '%Stage%';"

# Functional test
dotnet run --project OpCentrix
# Navigate to /Admin/Parts
# Create new part with stages
# Verify stage indicators display
# Test filtering and search
```

**? Success**: All tests pass, UI works smoothly, no performance regressions
**? Issues**: Document any problems and continue iteration

---

**This prompt provides a complete roadmap for finishing the Parts table refactoring with normalized database structure and user-friendly stage management!**