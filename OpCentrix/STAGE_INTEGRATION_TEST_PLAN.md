# ?? Stage Integration Testing Plan - Critical Fixes Verification

## ?? **TESTING OBJECTIVES**

Verify that the 5 critical fixes for stage functionality are working:

1. ? **API Authorization Fixed** - `/api/production-stages/available` now accessible
2. ? **Stage Manager Initialization Fixed** - No more race conditions  
3. ? **Form Data Population Fixed** - Hidden fields populated before submission
4. ? **HTMX Conflict Removed** - Single form submission path
5. ? **Compilation Issues Fixed** - Clean build

---

## ?? **PRE-TEST SETUP**

### **Start Application**
```powershell
cd OpCentrix
dotnet run --urls http://localhost:5091
```

### **Login as Admin**
- Navigate to: `http://localhost:5091/Account/Login`
- Username: `admin`
- Password: `admin123`

---

## ?? **TEST SEQUENCE**

### **TEST 1: API Endpoint Verification**

**Objective:** Verify the production stages API is accessible

**Steps:**
1. Open browser developer tools (F12)
2. Navigate to: `http://localhost:5091/Admin/Parts`
3. In the Console tab, run:
```javascript
fetch('/api/production-stages/available')
  .then(r => r.json())
  .then(data => {
    console.log('? API SUCCESS - Stages loaded:', data.length);
    console.table(data);
  })
  .catch(err => {
    console.error('? API FAILED:', err);
  });
```

**Expected Result:**
- ? Console shows: `API SUCCESS - Stages loaded: X` (where X > 0)
- ? Table shows stage data with proper fields (id, name, description, etc.)

**Failure Indicators:**
- ? `401 Unauthorized` error
- ? `404 Not Found` error
- ? Network error or timeout

---

### **TEST 2: Stage Manager Initialization**

**Objective:** Verify stage manager initializes when modal opens

**Steps:**
1. On the Parts page, click **"Add New Part"** button
2. Wait for modal to open
3. Click on the **"Manufacturing Stages"** tab
4. In browser console, check:
```javascript
console.log('Stage Manager Available:', !!window.stageManager);
console.log('Stage Manager Initialized:', window.stageManager?.initialized);
console.log('Available Stages:', window.stageManager?.availableStages?.length);
console.log('Container Exists:', !!document.getElementById('stage-requirements-container'));
```

**Expected Result:**
- ? `Stage Manager Available: true`
- ? `Stage Manager Initialized: true` 
- ? `Available Stages: X` (where X > 0)
- ? `Container Exists: true`
- ? Stages tab shows either "No Manufacturing Stages Selected" or available stages

**Failure Indicators:**
- ? `Stage Manager Available: false`
- ? Permanent loading spinner in stages tab
- ? `Container Exists: false`

---

### **TEST 3: Stage Selection Functionality**

**Objective:** Verify users can add and remove stages

**Steps:**
1. In the open modal, go to **Manufacturing Stages** tab
2. Click **"Add First Stage"** or **"Add Stage"** button
3. In the stage selection modal, click on **"SLS Printing"**
4. Verify stage appears in the selected stages list
5. Click the **Edit** button (pencil icon) on the added stage
6. Change **Estimated Hours** to `5.0`
7. Click **"Save Changes"**
8. Click the **Remove** button (trash icon) on the stage
9. Confirm removal

**Expected Result:**
- ? Stage selection modal opens with available stages
- ? Selected stage appears as a card with correct details
- ? Edit modal opens and saves changes
- ? Stage can be removed successfully
- ? Summary updates with correct totals

**Failure Indicators:**
- ? "Add Stage" button does nothing
- ? Stage selection modal is empty
- ? Selected stages don't appear
- ? Edit/remove buttons don't work

---

### **TEST 4: Form Data Submission**

**Objective:** Verify stage data is included when saving parts

**Steps:**
1. In the modal, fill out the **Basic Information** tab:
   - Part Number: `TEST-STAGE-001`
   - Name: `Stage Integration Test Part`
   - Description: `Testing stage functionality`
   - Component Type: Select any
   - Compliance Category: Select any
   - Material: `Ti-6Al-4V Grade 5`
   - Estimated Hours: `8.0`

2. Go to **Manufacturing Stages** tab
3. Add **SLS Printing** stage (8h, $85/hr)
4. Add **CNC Machining** stage (4h, $105/hr)
5. Before clicking save, check hidden fields in console:
```javascript
const hiddenFields = {
  selectedStageIds: document.getElementById('selectedStageIds')?.value,
  stageExecutionOrders: document.getElementById('stageExecutionOrders')?.value,
  stageEstimatedHours: document.getElementById('stageEstimatedHours')?.value,
  stageHourlyRates: document.getElementById('stageHourlyRates')?.value,
  stageMaterialCosts: document.getElementById('stageMaterialCosts')?.value
};
console.log('Hidden Field Values Before Submit:', hiddenFields);
```

6. Click **"Create Part"**
7. Wait for success notification
8. Check if part was created with stages

**Expected Result Before Submit:**
- ? `selectedStageIds: "1,3"` (or similar comma-separated IDs)
- ? `stageExecutionOrders: "1,2"` 
- ? `stageEstimatedHours: "8,4"` (or your entered values)
- ? All other fields have corresponding comma-separated values

**Expected Result After Submit:**
- ? Success notification: "Part 'TEST-STAGE-001' created successfully!"
- ? Modal closes and redirects to parts list
- ? New part appears in parts list
- ? Part shows stage indicators in the "Manufacturing Stages" column

**Failure Indicators:**
- ? Hidden fields are empty (all empty strings)
- ? Form submission fails with validation errors
- ? Part saves but stages are lost
- ? No stage indicators in parts list

---

### **TEST 5: Edit Existing Part with Stages**

**Objective:** Verify stage data loads when editing existing parts

**Steps:**
1. Find the part created in Test 4 (`TEST-STAGE-001`)
2. Click the **Edit** button (pencil icon)
3. Wait for modal to open
4. Go to **Manufacturing Stages** tab
5. Verify existing stages are loaded
6. Add one more stage (**Assembly**)
7. Remove one existing stage
8. Click **"Update Part"**

**Expected Result:**
- ? Edit modal opens with populated data
- ? Manufacturing Stages tab shows previously selected stages
- ? Can add and remove stages
- ? Part updates successfully with new stage configuration
- ? Changes are reflected in parts list

**Failure Indicators:**
- ? Stages tab is empty when editing existing part
- ? Cannot modify existing stages
- ? Changes don't persist after update

---

## ?? **DEBUGGING TOOLS**

### **Console Commands for Troubleshooting**

**Check Stage Manager State:**
```javascript
window.debugStageManager = function() {
  console.log('=== STAGE MANAGER DEBUG ===');
  console.log('Available:', !!window.stageManager);
  console.log('Initialized:', window.stageManager?.initialized);
  console.log('Part ID:', window.stageManager?.partId);
  console.log('Available Stages:', window.stageManager?.availableStages?.length);
  console.log('Selected Stages:', window.stageManager?.selectedStages?.size);
  console.log('Container Element:', !!document.getElementById('stage-requirements-container'));
  return window.stageManager;
};
// Run: debugStageManager()
```

**Check Form Manager State:**
```javascript
window.debugPartForm = function() {
  console.log('=== FORM MANAGER DEBUG ===');
  console.log('Form Manager:', !!window.partFormManager);
  console.log('Form Element:', !!window.partFormManager?.formElement);
  console.log('Submit In Progress:', window.partFormManager?.submitInProgress);
  console.log('Validation Required:', window.partFormManager?.validationRequired);
  return window.partFormManager;
};
// Run: debugPartForm()
```

**Manual Stage Data Collection:**
```javascript
window.getStageDataManually = function() {
  const data = window.stageManager?.getStageDataForSubmission();
  console.log('Stage Data for Submission:', data);
  return data;
};
// Run: getStageDataManually()
```

---

## ? **SUCCESS CRITERIA**

### **All Tests Must Pass For:**

1. **API Connectivity** ?
   - Production stages API returns data
   - No authorization errors

2. **Stage Manager Functionality** ?  
   - Initializes when modal opens
   - Shows available stages
   - Allows stage selection/removal
   - Updates summaries correctly

3. **Form Integration** ?
   - Hidden fields populated before submission
   - Stage data persists when saving parts
   - Edit functionality works with existing stages

4. **User Experience** ?
   - No loading spinners that never complete
   - No JavaScript errors in console
   - Smooth transitions between form operations

---

## ?? **FAILURE SCENARIOS & FIXES**

### **If API Test Fails:**
- Check authorization settings in `ProductionStagesApiController.cs`
- Verify user is logged in
- Check network tab for exact error response

### **If Stage Manager Fails:**
- Check console for JavaScript errors
- Verify modal initialization timing
- Check DOM element existence

### **If Form Submission Fails:**
- Check hidden field population in console
- Verify form manager initialization
- Check backend logging for stage data receipt

### **If Edit Functionality Fails:**
- Check part loading endpoint
- Verify stage requirements are being retrieved
- Check database for existing stage assignments

---

## ?? **EXPECTED FINAL STATE**

After all tests pass successfully:

1. **Parts List Shows:**
   - ? Stage indicators in "Manufacturing Stages" column
   - ? Total duration reflects stage hours
   - ? Stage badges show correctly (SLS, CNC, etc.)

2. **Database Contains:**
   - ? Part record in `Parts` table
   - ? Stage assignments in `PartStageRequirements` table
   - ? Correct execution orders and costs

3. **User Experience:**
   - ? Smooth modal operations
   - ? Clear stage selection interface
   - ? Accurate cost/time calculations
   - ? Professional success notifications

---

## ?? **COMPLETION CHECKLIST**

- [ ] **TEST 1:** API endpoint returns stage data
- [ ] **TEST 2:** Stage manager initializes in modal
- [ ] **TEST 3:** Can add/edit/remove stages
- [ ] **TEST 4:** New parts save with stage data
- [ ] **TEST 5:** Existing parts load with stage data
- [ ] **VERIFY:** No console errors during any operation
- [ ] **VERIFY:** Database contains stage assignments
- [ ] **VERIFY:** Parts list shows stage indicators

**Test Status:** ?? **Ready for Testing**  
**Estimated Test Time:** 15-20 minutes  
**Required Role:** Admin user access