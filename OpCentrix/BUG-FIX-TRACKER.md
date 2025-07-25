# OpCentrix Scheduler - Bug Fix Tracking System

## BUG FIX PROGRESS TRACKER

**Last Updated**: January 2025  
**Current Status**: Phase 5 - Critical Bug Fixes IN PROGRESS  
**AI Instruction**: Always check Documentation/AI-INSTRUCTIONS-NO-UNICODE.md before making changes  

---

## CURRENT FIX SESSION - STARTED

### Session Start: January 2025
- **Current Bug**: BUG #1 - HTMX Form Submission Fix
- **Files Organized**: Project structure verified
- **Status**: ACTIVE FIXING - Issue identified

---

## CRITICAL BUGS IDENTIFIED AND STATUS

### 🚨 HIGH PRIORITY BUGS (Must Fix First)

#### 🔧 BUG #1: HTMX Form Submission Fix - IN PROGRESS
**Status**: 🟡 FIXING IN PROGRESS  
**Priority**: CRITICAL  
**Issue**: Modal doesn't close after successful job submission  
**Impact**: User experience severely degraded  

**ROOT CAUSE IDENTIFIED**:
- Form returns JavaScript script content instead of proper HTMX response
- Modal close is handled by JavaScript timeout instead of HTMX swap
- No proper HTMX targeting for modal closure

**Files to Fix**:
- [x] `OpCentrix/Pages/Scheduler/_AddEditJobModal.cshtml` - Examined
- [x] `OpCentrix/Pages/Scheduler/Index.cshtml.cs` (OnPostAddOrUpdateJobAsync) - Examined
- [ ] `OpCentrix/Pages/Scheduler/_FullMachineRow.cshtml` - Need to examine

**SPECIFIC ISSUE FOUND**:
```csharp
// PROBLEM: In Index.cshtml.cs OnPostAddOrUpdateJobAsync
// Returns JavaScript script instead of HTMX partial update
var script = $@"<script>setTimeout(() => {{ ... }}, 500);</script>";
return Content(script, "text/html");

// SOLUTION: Return proper HTMX response to close modal and update machine row
```

**Expected Behavior**: Modal closes, grid updates, no page refresh

**Next Action**: Implement proper HTMX response in OnPostAddOrUpdateJobAsync

---

#### ❌ BUG #2: Job End Time Calculation Fix
**Status**: 🔴 NOT FIXED  
**Priority**: CRITICAL  
**Issue**: Uses `AvgDurationDays` instead of `EstimatedHours`  
**Impact**: Incorrect job duration calculations  

**Files to Fix**:
- [ ] JavaScript in scheduler page (look for `updateEndTime` function)

**Fix Details**:
```javascript
// WRONG (current code):
const estimatedDays = parseFloat(selected.getAttribute('data-avg-duration-days'));
const end = new Date(start.getTime() + estimatedDays * 24 * 60 * 60 * 1000);

// CORRECT (fixed code):
const estimatedHours = parseFloat(selected.getAttribute('data-estimated-hours')) || 8;
const end = new Date(start.getTime() + estimatedHours * 60 * 60 * 1000);
```

**Testing Steps**:
1. Open "Add Job" modal
2. Select a part from dropdown
3. Verify end time auto-calculates correctly based on estimated hours
4. Test with different parts having different estimated hours

---

#### ✅ BUG #3: Weekend Operations Logic Fix
**Status**: 🟢 FIXED  
**Priority**: HIGH  
**Issue**: Jobs incorrectly moved from weekends to Monday  
**Impact**: Weekend scheduling not working  

**Fix Applied**: SchedulerSettingsService implementation complete  
**Files Modified**: 
- ✅ `OpCentrix/Services/SchedulerSettingsService.cs`
- ✅ `OpCentrix/Documentation/WEEKEND-OPERATIONS-FIX-COMPLETE.md`

**Verification Steps**:
1. Enable weekend operations in admin settings
2. Try to schedule job for Saturday - should stay on Saturday
3. Disable weekend operations 
4. Try to schedule job for Saturday - should move to Monday

---

#### ❌ BUG #4: Performance Optimization Fix
**Status**: 🔴 NOT FIXED  
**Priority**: HIGH  
**Issue**: Loading ALL jobs instead of date range  
**Impact**: Poor performance with large datasets  

**File to Fix**:
- [ ] `OpCentrix/Services/SchedulerService.cs` (GetSchedulerData method)

**Fix Details**:
```csharp
// Add date filtering to job query
var jobs = await _context.Jobs
    .Include(j => j.Part)
    .Where(j => j.ScheduledStart < queryEndDate && j.ScheduledEnd > queryStartDate)
    .OrderBy(j => j.ScheduledStart)
    .ToListAsync();
```

**Expected Impact**: 50-80% performance improvement for large datasets

---

### ⚠️ MEDIUM PRIORITY BUGS

#### ❌ BUG #5: Modal State Management
**Status**: 🔴 NOT FIXED  
**Priority**: MEDIUM  
**Issue**: Modal state management conflicts  

**Fixes Needed**:
- [ ] Modal opens reliably
- [ ] Background click closes modal
- [ ] Escape key closes modal
- [ ] No background scrolling during modal
- [ ] Multiple modals don't conflict

#### ❌ BUG #6: Loading States Missing
**Status**: 🔴 NOT FIXED  
**Priority**: MEDIUM  
**Issue**: No visual feedback during operations  

**Fixes Needed**:
- [ ] Loading indicators during job operations
- [ ] Disabled buttons during submission
- [ ] Visual feedback for successful operations
- [ ] Error messages display properly

---

## CURRENT FIX IMPLEMENTATION

###  FIXING BUG #1: HTMX Form Submission

**Problem Analysis**:
The `OnPostAddOrUpdateJobAsync` method in `Index.cshtml.cs` returns a JavaScript script that:
1. Uses setTimeout to close modal after 500ms
2. Reloads entire page instead of partial update
3. Doesn't use proper HTMX response handling

**Solution Strategy**:
1. Return proper HTMX response instead of JavaScript
2. Use HTMX `hx-target` and `hx-swap` correctly
3. Close modal via HTMX response mechanism
4. Update only the affected machine row

**Implementation Plan**:
1. Modify `OnPostAddOrUpdateJobAsync` to return proper partial view
2. Update modal form HTMX attributes
3. Test modal close and grid update behavior

---

## SYSTEMATIC FIX PROCESS

### Step 1: Pre-Fix Checklist
- [x] Read `Documentation/AI-INSTRUCTIONS-NO-UNICODE.md`
- [x] Project files are in correct structure
- [x] Backup current working state (git status clean)
- [x] Identify specific files to modify

### Step 2: Implementation Process
1. **Fix one bug at a time** ✅ Currently on Bug #1
2. **Test immediately after each fix**
3. **Update this tracking file**
4. **Move files to proper project structure as needed**

### Step 3: Post-Fix Verification
- [ ] Run `Scripts/diagnose-system.bat`
- [ ] Test specific functionality
- [ ] Update status in this file
- [ ] Document any new issues found

---

## PROGRESS LOG

### Session Start: January 2025
- **Current Bug**: BUG #1 - HTMX Form Submission Fix
- **Files Organized**: Yes - Project structure verified
- **Status**: IN PROGRESS - Root cause identified in OnPostAddOrUpdateJobAsync

### Next Steps:
1. ✅ Examine current HTMX implementation - DONE
2. ✅ Identify root cause - DONE: JavaScript script return instead of HTMX partial
3.  Implement proper HTMX response - IN PROGRESS
4. Test modal close behavior
5. Move to Bug #2

--- 

## NOTES FOR AI ASSISTANTS

### Critical Instructions:
1. **Always run systematic fixes - one bug at a time**
2. **Files must be in proper project structure: `OpCentrix/` subdirectory**
3. **Check `Documentation/AI-INSTRUCTIONS-NO-UNICODE.md` before changes**
4. **No Unicode characters in output or code**
5. **Test each fix immediately**
6. **Update this tracking file after each fix**

### File Location Rules:
- **Code files**: Must be in `OpCentrix/` project directory
- **Documentation**: Move to `OpCentrix/Documentation/`
- **Scripts**: Move to `OpCentrix/Scripts/`
- **Keep tracking files updated**

### Continuation Protocol:
If session ends before completion:
1. **Update this file with current status**
2. **Note which bug was being worked on**
3. **List next steps for resumption**
4. **Ensure all changes are in proper project structure**

---