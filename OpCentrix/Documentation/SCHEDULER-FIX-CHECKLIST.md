# OpCentrix Scheduler - Fix Checklist

## CRITICAL SYSTEM FIX CHECKLIST

### PHASE 1: SYSTEM DIAGNOSTICS AND VERIFICATION

#### [ ] 1.1 System Health Check 
```cmd
# Run comprehensive system diagnostic
C:\Users\Henry\Source\Repos\OpCentrix\diagnose-system.bat

# Expected Output:
# [OK] Project file found
# [OK] Database connection working
# [OK] jQuery libraries present
# [OK] All critical files verified
```

#### [ ] 1.2 Database Verification
```cmd
# Clean and setup database
C:\Users\Henry\Source\Repos\OpCentrix\setup-clean-database.bat

# Expected Result:
# - Fresh database created
# - All tables initialized
# - Default settings populated
# - Sample data loaded (if enabled)
```

#### [ ] 1.3 Application Startup Test
```cmd
# Start application
C:\Users\Henry\Source\Repos\OpCentrix\start-application.bat

# Expected Result:
# - Application starts without errors
# - Accessible at https://localhost:5001
# - Login page loads correctly
```

---

### PHASE 2: CORE FUNCTIONALITY VERIFICATION

#### [ ] 2.1 Authentication System
- [ ] Login page loads without JavaScript errors
- [ ] Admin login works: `admin / admin123`
- [ ] User roles are properly assigned
- [ ] Session management working

#### [ ] 2.2 Scheduler Settings System
```cmd
# Test URL: https://localhost:5001/Admin/SchedulerSettings
```
- [ ] Admin settings page loads
- [ ] Default values are populated
- [ ] Settings can be modified and saved
- [ ] Changes persist after page refresh
- [ ] Weekend operations toggle works

#### [ ] 2.3 jQuery Validation System
```cmd
# Run jQuery validation fix if needed
C:\Users\Henry\Source\Repos\OpCentrix\fix-jquery-validation.bat
```
- [ ] Open browser developer tools (F12)
- [ ] Navigate to any form page
- [ ] Console shows: "[OK] jQuery loaded successfully"
- [ ] Console shows: "[OK] jQuery Validation loaded successfully"
- [ ] Form validation works on empty required fields

---

### PHASE 3: SCHEDULER GRID FUNCTIONALITY

#### [ ] 3.1 Grid Display Issues
- [ ] Scheduler page loads: `https://localhost:5001/Scheduler`
- [ ] All three machines visible: TI1, TI2, INC
- [ ] Date headers display correctly
- [ ] Time slots are properly aligned
- [ ] Grid is responsive on different screen sizes

#### [ ] 3.2 Zoom Level Testing
Test each zoom level systematically:
- [ ] Day view (8 weeks) - slots should be 120px wide
- [ ] 12h view (4 weeks) - slots should be 100px wide
- [ ] Hour view (2 weeks) - slots should be 65px wide
- [ ] 30min view (1 week) - slots should be 50px wide
- [ ] 15min view (3 days) - slots should be 30px wide

#### [ ] 3.3 Job Block Positioning
- [ ] Job blocks appear in correct time slots
- [ ] Job blocks scale properly across zoom levels
- [ ] No overlap with properly scheduled jobs
- [ ] Job block colors match status (scheduled, building, etc.)

---

### PHASE 4: JOB MANAGEMENT FUNCTIONALITY

#### [ ] 4.1 Add Job Modal
- [ ] Click "Add Job" button opens modal
- [ ] Modal displays without JavaScript errors
- [ ] All form fields are present and functional
- [ ] Part number dropdown populates
- [ ] Material dropdown works
- [ ] Date/time pickers function correctly

#### [ ] 4.2 Job Creation Process
- [ ] Fill out job form with valid data
- [ ] Click "Save Job" 
- [ ] Modal closes automatically
- [ ] Job appears on grid immediately (HTMX update)
- [ ] No full page refresh occurs
- [ ] Job displays in correct position

#### [ ] 4.3 Job Editing Process
- [ ] Click existing job block opens edit modal
- [ ] Form pre-populates with job data
- [ ] Changes can be saved successfully
- [ ] Updated job reflects changes immediately
- [ ] Position updates if time changed

#### [ ] 4.4 Job Deletion Process
- [ ] Click job block to open edit modal
- [ ] Click "Delete Job" button
- [ ] Confirmation dialog appears
- [ ] Confirm deletion
- [ ] Job disappears from grid immediately
- [ ] No full page refresh occurs

---

### PHASE 5: CRITICAL BUG FIXES

#### [ ] 5.1 HTMX Form Submission Fix
**Issue**: Modal doesn't close after successful submission
**Fix Required**: Update form submission handlers

Check these files need fixing:
- [ ] `OpCentrix\Pages\Scheduler\_AddEditJobModal.cshtml`
- [ ] `OpCentrix\Pages\Scheduler\Index.cshtml.cs` (OnPostAddOrUpdateJobAsync)
- [ ] `OpCentrix\Pages\Scheduler\_FullMachineRow.cshtml`

**Expected Behavior**: Modal closes, grid updates, no page refresh

#### [ ] 5.2 Job End Time Calculation Fix
**Issue**: Uses `AvgDurationDays` instead of `EstimatedHours`
**File**: JavaScript in scheduler page

**Fix Required**:
```javascript
// WRONG:
const estimatedDays = parseFloat(selected.getAttribute('data-avg-duration-days'));
const end = new Date(start.getTime() + estimatedDays * 24 * 60 * 60 * 1000);

// CORRECT:
const estimatedHours = parseFloat(selected.getAttribute('data-estimated-hours')) || 8;
const end = new Date(start.getTime() + estimatedHours * 60 * 60 * 1000);
```

#### [ ] 5.3 Weekend Operations Logic Fix
**Issue**: Jobs incorrectly moved from weekends to Monday

**Verification Steps**:
- [ ] Enable weekend operations in admin settings
- [ ] Try to schedule job for Saturday
- [ ] Job should stay on Saturday (not move to Monday)
- [ ] Disable weekend operations
- [ ] Try to schedule job for Saturday
- [ ] Job should move to next Monday

#### [ ] 5.4 Performance Optimization Fix
**Issue**: Loading ALL jobs instead of date range

**File**: `OpCentrix\Services\SchedulerService.cs`
**Method**: `GetSchedulerData`

**Fix Required**: Implement date range filtering:
```csharp
// Add date filtering to job query
var jobs = await _context.Jobs
    .Include(j => j.Part)
    .Where(j => j.ScheduledStart < queryEndDate && j.ScheduledEnd > queryStartDate)
    .OrderBy(j => j.ScheduledStart)
    .ToListAsync();
```

---

### PHASE 6: UI/UX IMPROVEMENTS

#### [ ] 6.1 Modal State Management
- [ ] Modal opens reliably
- [ ] Background click closes modal
- [ ] Escape key closes modal
- [ ] No background scrolling during modal
- [ ] Multiple modals don't conflict

#### [ ] 6.2 Loading States
- [ ] Loading indicators during job operations
- [ ] Disabled buttons during submission
- [ ] Visual feedback for successful operations
- [ ] Error messages display properly

#### [ ] 6.3 Responsive Design
- [ ] Works on desktop (1920x1080)
- [ ] Works on laptop (1366x768)
- [ ] Works on tablet (768px width)
- [ ] Works on mobile (375px width)
- [ ] Machine labels remain visible while scrolling

---

### PHASE 7: VALIDATION AND ERROR HANDLING

#### [ ] 7.1 Client-Side Validation
- [ ] Required field validation works
- [ ] Date validation prevents past dates
- [ ] Time validation ensures end > start
- [ ] Part number format validation
- [ ] Material selection validation

#### [ ] 7.2 Server-Side Validation
- [ ] Conflict detection works
- [ ] Material changeover time validation
- [ ] Weekend operation validation
- [ ] Machine capacity validation
- [ ] Operator availability validation

#### [ ] 7.3 Error Display
- [ ] Validation errors show in modal
- [ ] Server errors display to user
- [ ] Network errors are handled gracefully
- [ ] Error messages are clear and actionable

---

### PHASE 8: ADVANCED FEATURES

#### [ ] 8.1 Material Changeover Calculation
- [ ] Ti-Ti changeover: 30 minutes
- [ ] Inconel-Inconel changeover: 45 minutes
- [ ] Cross-material changeover: 120 minutes
- [ ] Automatic time adjustment in scheduler

#### [ ] 8.2 Machine Priority System
- [ ] TI1/TI2/INC priorities respected
- [ ] Job placement considers priority
- [ ] Admin can modify priorities
- [ ] Priority affects cost calculations

#### [ ] 8.3 Shift Management
- [ ] Standard shift: 7:00 AM - 3:00 PM
- [ ] Evening shift: 3:00 PM - 11:00 PM
- [ ] Night shift: 11:00 PM - 7:00 AM
- [ ] Jobs scheduled within shift hours

---

### PHASE 9: PRODUCTION READINESS

#### [ ] 9.1 Performance Testing
- [ ] Page load times under 2 seconds
- [ ] Database queries under 500ms
- [ ] Memory usage under 200MB
- [ ] No memory leaks during extended use

#### [ ] 9.2 Security Verification
- [ ] Authentication required for admin functions
- [ ] Authorization prevents unauthorized access
- [ ] SQL injection protection verified
- [ ] XSS protection in place

#### [ ] 9.3 Data Integrity
- [ ] Database constraints prevent invalid data
- [ ] Concurrent access handled properly
- [ ] Transaction rollback on errors
- [ ] Audit trail maintains data history

---

## QUICK FIX COMMANDS

### If System Won't Start:
```cmd
# Reset everything
C:\Users\Henry\Source\Repos\OpCentrix\setup-clean-database.bat
C:\Users\Henry\Source\Repos\OpCentrix\start-application.bat
```

### If jQuery Issues:
```cmd
C:\Users\Henry\Source\Repos\OpCentrix\fix-jquery-validation.bat
```

### If Modal Problems:
1. Clear browser cache (Ctrl+F5)
2. Check browser console for JavaScript errors
3. Verify HTMX responses in Network tab

### If Database Issues:
```cmd
# Complete database reset
del C:\Users\Henry\Source\Repos\OpCentrix\OpCentrix\Data\*.db*
cd C:\Users\Henry\Source\Repos\OpCentrix\OpCentrix
dotnet run
```

---

## SUCCESS CRITERIA

### System is Working When:
- [ ] Application starts without errors
- [ ] Scheduler page loads with proper grid
- [ ] Jobs can be created, edited, and deleted
- [ ] Modal operations work smoothly
- [ ] No JavaScript errors in browser console
- [ ] Weekend operations respect settings
- [ ] Performance is acceptable (< 2 second page loads)
- [ ] All validation rules work correctly

### System is Production Ready When:
- [ ] All checklist items completed
- [ ] Comprehensive testing performed
- [ ] Performance optimized
- [ ] Security verified
- [ ] Documentation updated
- [ ] Error handling comprehensive

---

**RECOMMENDATION**: Work through this checklist systematically, completing each phase before moving to the next. Most issues can be resolved by following the verification steps and applying the specific fixes outlined above.