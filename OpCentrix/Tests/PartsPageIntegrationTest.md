# ?? Parts Page Integration Test Plan

## ?? Comprehensive Testing Protocol

### **Database Status Check**
```powershell
# Verify database structure
sqlite3 scheduler.db "SELECT name FROM sqlite_master WHERE type='table' AND name IN ('Parts', 'ComponentTypes', 'ComplianceCategories', 'PartStageRequirements', 'ProductionStages');"

# Check data integrity
sqlite3 scheduler.db "SELECT COUNT(*) as PartsCount FROM Parts WHERE IsActive = 1;"
sqlite3 scheduler.db "SELECT COUNT(*) as ComponentTypesCount FROM ComponentTypes;"
sqlite3 scheduler.db "SELECT COUNT(*) as ComplianceCategoriesCount FROM ComplianceCategories;"
```

### **Application Startup Test**
```powershell
# Start application
dotnet run --urls http://localhost:5090 --no-launch-profile

# Expected output should include:
# ? Database creation/update completed
# ? Admin data seeding completed
# ? Material seeding completed
# ? Production stage seeding completed
# ? Part form refactor seeding completed
# Now listening on: http://localhost:5090
```

### **Parts Page Functionality Tests**

#### **1. Page Load Test**
- **URL**: `http://localhost:5090/Admin/Parts`
- **Expected**: Page loads without errors, shows parts list
- **JavaScript Console**: Should show initialization messages

#### **2. Add New Part Button Test**
- **Action**: Click "Add New Part" button
- **Expected**: Modal opens with form
- **JavaScript Console**: Should show operation ID and successful modal load

#### **3. Form Submission Test (Create)**
- **Action**: Fill out basic form fields and submit
- **Required Fields**: PartNumber, Name, Description, Industry, Application, ComponentTypeId, ComplianceCategoryId, Material, EstimatedHours
- **Expected**: Successful creation with redirect and toast notification

#### **4. Form Submission Test (Edit)**
- **Action**: Click edit button on existing part
- **Expected**: Modal opens with populated form data
- **Action**: Modify data and submit
- **Expected**: Successful update with redirect and toast notification

#### **5. Delete Test**
- **Action**: Click delete button
- **Expected**: Confirmation dialog, successful deletion on confirm

### **Antiforgery Token Verification**
```javascript
// In browser console:
console.log('Token available:', document.querySelector('input[name="__RequestVerificationToken"]')?.value);

// Should show token value
```

### **HTMX Integration Test**
```javascript
// In browser console after form submission:
// Should see HTMX request/response logs
```

### **Error Scenarios to Test**

#### **1. Duplicate Part Number**
- **Action**: Try to create part with existing part number
- **Expected**: Validation error with clear message

#### **2. Missing Required Fields**
- **Action**: Submit form with empty required fields
- **Expected**: Client-side and server-side validation errors

#### **3. Network Error Simulation**
- **Action**: Disconnect network and try form submission
- **Expected**: Graceful error handling with user-friendly message

### **Performance Benchmarks**

#### **Acceptable Performance Thresholds**
- **Page Load**: < 3 seconds
- **Modal Open**: < 1 second
- **Form Submission**: < 5 seconds
- **Search/Filter**: < 2 seconds

### **Browser Compatibility Test**
- **Chrome** (latest)
- **Firefox** (latest)
- **Edge** (latest)
- **Safari** (if available)

### **Success Criteria**

? **All CRUD operations work correctly**
? **Antiforgery token is properly handled**
? **JavaScript functions execute without errors**
? **Modal operations are smooth and reliable**
? **Form validation works on client and server side**
? **Error handling provides useful feedback**
? **Performance meets or exceeds thresholds**
? **No console errors or warnings**

### **Common Issues and Solutions**

#### **Issue**: "Add New Part" button doesn't work
**Debug**: Check browser console for JavaScript errors
**Solution**: Verify antiforgery token and HTMX integration

#### **Issue**: Form submission fails
**Debug**: Check network tab for failed requests
**Solution**: Verify server-side handler methods exist

#### **Issue**: Modal doesn't close after submission
**Debug**: Check HTMX event handlers
**Solution**: Verify HTMX success event handling

### **Debugging Commands**

```javascript
// Check Parts page functions
window.debugPartsPage();

// Test antiforgery token
console.log('Token:', window.getAntiforgeryToken?.() || 'Function not available');

// Test modal functions
window.handleAddPartClick();
```

## ?? **READY FOR PRODUCTION**

When all tests pass:
- ? Parts page is fully functional
- ? All CRUD operations work correctly  
- ? JavaScript integration is bulletproof
- ? Antiforgery tokens are properly handled
- ? User experience is smooth and reliable