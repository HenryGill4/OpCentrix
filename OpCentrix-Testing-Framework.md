# ?? **OpCentrix Comprehensive Testing Framework & Bug Debugging Guide**

**Date**: January 2025  
**Status**: ?? **COMPLETE TESTING PROTOCOL** - Systematic approach to validate all functionality  
**Goal**: Provide a comprehensive testing framework to identify and resolve bugs efficiently  

---

## ?? **CRITICAL TESTING INSTRUCTIONS FOR AI ASSISTANT**

### **?? MANDATORY RESEARCH PROTOCOL**
**?? READ THESE INSTRUCTIONS EVERY TIME WE DEBUG TOGETHER**

#### **1. ALWAYS Start with Context Gathering**
```powershell
# Before debugging ANY issue, ALWAYS run these commands:
dotnet clean
dotnet restore
dotnet build OpCentrix/OpCentrix.csproj

# If build fails, stop immediately and fix compilation errors first
# Only proceed to testing if build is 100% successful
```
- **REQUIRED**: Use `text_search` to understand current system state
- **REQUIRED**: Check error logs in `OpCentrix/logs/` directory
- **REQUIRED**: Verify which user role is experiencing the issue
- **REQUIRED**: Reproduce the exact steps that led to the problem

#### **2. PowerShell-Only Commands (CRITICAL)**
```powershell
# ? CORRECT PowerShell syntax for testing
dotnet clean
dotnet restore
dotnet build OpCentrix/OpCentrix.csproj
dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --verbosity normal

# For application testing (ONLY when specifically requested):
cd OpCentrix
dotnet run
# Note: This starts the web server - NEVER use for automated testing

# ? WRONG - Never use && in PowerShell
dotnet clean && dotnet restore  # This will FAIL in PowerShell
```

#### **3. Error Analysis Requirements**
- **ALWAYS** capture the full error message with stack trace
- **NEVER** assume the cause - investigate systematically
- **REQUIRED**: Check browser console (F12) for JavaScript errors
- **VERIFY**: Authentication status and user role for access issues
- **CHECK**: Network tab in browser for failed API calls

#### **4. Bug Investigation Protocol**
```powershell
# Systematic bug investigation process:

# Step 1: Verify build status
dotnet build OpCentrix/OpCentrix.csproj

# Step 2: Check recent logs
Get-Content "OpCentrix/logs/opcentrix-*.log" | Select-Object -Last 50

# Step 3: Run relevant tests
dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --filter "CategoryName" --verbosity normal

# Step 4: Check database state (if needed)
# Only suggest SQL queries if database issue is suspected
```

#### **5. Testing Environment Setup**
- **VERIFY** application is running on `http://localhost:5090`
- **CONFIRM** database exists at `OpCentrix/scheduler.db`
- **CHECK** all static files are properly served
- **ENSURE** user has appropriate role for the feature being tested

---

## ?? **COMPREHENSIVE TESTING CHECKLIST**

### **?? Phase 1: Authentication & Authorization Testing**

#### **? Basic Authentication Flow**
**Test Users Available:**
```
admin/admin123       (Full Access)
manager/manager123   (Management Access)  
scheduler/scheduler123 (Scheduling Access)
operator/operator123 (Operations Access)
printer/printer123   (Print Tracking Access)
coating/coating123   (Coating Operations Access)
edm/edm123          (EDM Operations Access)
qc/qc123            (Quality Control Access)
```

**Testing Steps:**
1. **Start Application**
   ```powershell
   cd OpCentrix
   dotnet run
   ```
   - Navigate to: `http://localhost:5090/Account/Login`
   - **Expected**: Login page displays properly

2. **Test Each User Role**
   - Login with `admin/admin123`
   - **Expected**: Redirected to `/Admin` dashboard
   - **Verify**: Can access all navigation items
   
   - Login with `scheduler/scheduler123`
   - **Expected**: Redirected to `/Scheduler`
   - **Verify**: Cannot access `/Admin` (should redirect to login)

3. **Test Session Management**
   - Login successfully
   - Wait for timeout warning (appears after 115 minutes by default)
   - **Expected**: Warning displays with countdown
   - **Verify**: Can extend session or auto-logout occurs

**?? Common Issues & Solutions:**
- **"Invalid username or password"**: Check exact case and spelling
- **"Access denied"**: Verify user role has permission for the page
- **"Session expired"**: Normal behavior after 2 hours, re-login required

---

### **?? Phase 2: Navigation & Layout Testing**

#### **? Main Navigation System**
**Test Steps:**
1. **Login as admin** (`admin/admin123`)
2. **Test Main Navigation**
   - Click "Scheduler" ? Should load `/Scheduler`
   - Click "Admin" ? Should load `/Admin`
   - Verify stage-aware navigation shows:
     - SLS ? EDM ? Coating ? QC ? Shipping workflow
     - B&T Manufacturing dropdown (if BTSpecialist role)
     - Advanced Workflows dropdown (if WorkflowSpecialist role)

3. **Test Mobile Navigation**
   - Resize browser to mobile width (< 768px)
   - **Expected**: Hamburger menu appears
   - **Verify**: All navigation items accessible in mobile menu

4. **Test Admin Navigation**
   - Navigate to `/Admin`
   - **Verify**: Comprehensive admin sidebar shows all sections:
     - System Administration (red)
     - Resource Management (blue)
     - Manufacturing Operations (indigo)
     - Quality Management (green)
     - B&T Administration (amber)
     - Advanced Workflows (purple)
     - System Tools (orange)
     - Data Management (gray)

**?? Common Issues & Solutions:**
- **Navigation not responsive**: Check CSS compilation and TailwindCSS loading
- **Dropdown menus not working**: Verify JavaScript is loading properly
- **Missing navigation items**: Check user role permissions

---

### **?? Phase 3: Manufacturing Operations Testing**

#### **? Scheduler Core Functionality**
**Test Steps:**
1. **Login as scheduler** (`scheduler/scheduler123`)
2. **Navigate to** `/Scheduler`
3. **Test Basic Scheduler Features**
   - **Verify**: Calendar displays current month
   - **Test**: Click on different date ranges
   - **Check**: Machine rows display (TI1, TI2, INC expected)
   - **Verify**: Job blocks display with proper styling

4. **Test Job Management**
   - **Click** any empty time slot
   - **Expected**: Add Job modal opens
   - **Fill out** basic job information
   - **Submit**: Job should be created and displayed
   - **Edit**: Click existing job block
   - **Expected**: Edit modal with populated data

5. **Test Enhanced Stage Features**
   - **Create jobs** with different workflow stages
   - **Verify**: Stage indicators show on job blocks
   - **Check**: Cohort grouping displays properly
   - **Test**: Progress bars for multi-stage jobs

**?? Common Issues & Solutions:**
- **Modal not opening**: Check JavaScript errors in browser console
- **Jobs not saving**: Check server logs and database connection
- **Stage indicators missing**: Verify CSS files are loading properly

#### **? Print Tracking System**
**Test Steps:**
1. **Login as printer** (`printer/printer123`)
2. **Navigate to** `/PrintTracking`
3. **Test Print Operations**
   - **Click** "Start New Print" button
   - **Expected**: Start Print modal opens
   - **Fill out** print details
   - **Submit**: Print should be tracked in system

4. **Test Print Status Updates**
   - **View** active prints
   - **Update** print status
   - **Verify**: Status changes reflect in dashboard

**?? Common Issues & Solutions:**
- **Print modal issues**: Check modal JavaScript and HTMX integration
- **Status not updating**: Verify HTMX requests are working properly

#### **? EDM Operations**
**Test Steps:**
1. **Login as edm** (`edm/edm123`)
2. **Navigate to** `/EDM`
3. **Test EDM Logging**
   - **Click** "New EDM Log" or similar
   - **Fill out** EDM operation details
   - **Save**: Entry should be recorded
   - **Verify**: Log appears in recent logs

**?? Common Issues & Solutions:**
- **EDM form errors**: Check form validation and server-side processing
- **Data not saving**: Verify database connection and model binding

#### **? Coating Operations**
**Test Steps:**
1. **Login as coating** (`coating/coating123`)
2. **Navigate to** `/Coating`
3. **Test Coating Workflow**
   - **Create** new coating entry
   - **Update** coating status
   - **Verify**: Progress tracking works

---

### **?? Phase 4: Admin System Testing**

#### **? Admin Dashboard**
**Test Steps:**
1. **Login as admin** (`admin/admin123`)
2. **Navigate to** `/Admin`
3. **Verify Dashboard Elements**
   - **Check**: KPI cards display properly
   - **Verify**: Recent activity shows
   - **Test**: Quick action buttons work

#### **? User Management**
**Test Steps:**
1. **Navigate to** `/Admin/Users`
2. **Test User Operations**
   - **View**: User list displays
   - **Add**: Create new user
   - **Edit**: Modify existing user
   - **Roles**: Assign/modify user roles

#### **? Parts Management**
**Test Steps:**
1. **Navigate to** `/Admin/Parts`
2. **Test Parts CRUD**
   - **Create**: Add new part
   - **Read**: View parts list
   - **Update**: Edit existing part
   - **Delete**: Remove part (with confirmation)

#### **? Machine Management**
**Test Steps:**
1. **Navigate to** `/Admin/Machines`
2. **Test Machine Operations**
   - **View**: Machine list (TI1, TI2, INC expected)
   - **Configure**: Machine settings
   - **Status**: Machine availability updates

---

### **?? Phase 5: Advanced Features Testing**

#### **? Stage-Aware Workflow Testing**
**Prerequisites**: Jobs with workflow stages assigned

**Test Steps:**
1. **Create Multi-Stage Job**
   - **Start**: SLS stage job
   - **Progress**: Move to EDM stage
   - **Complete**: Finish at Shipping stage
   - **Verify**: Stage progression tracked

2. **Test Build Cohorts**
   - **Create**: SLS build with multiple parts
   - **Complete**: Build to generate cohort
   - **Verify**: Cohort appears in system
   - **Track**: Cohort through subsequent stages

#### **? B&T Manufacturing Features**
**Test Steps** (if user has BTSpecialist role):
1. **Navigate to** B&T sections
2. **Test**: Part classifications
3. **Test**: Serial number tracking
4. **Test**: Compliance documentation

#### **? Analytics & Reporting**
**Test Steps:**
1. **Login as analyst** (`analyst/analyst123`)
2. **Navigate to** `/Analytics`
3. **Verify**: Charts and reports display
4. **Test**: Data filtering and exports

---

## ?? **SYSTEMATIC BUG DEBUGGING PROTOCOL**

### **Step 1: Immediate Error Analysis**
When a bug is reported, immediately collect:
```powershell
# 1. Build status
dotnet build OpCentrix/OpCentrix.csproj

# 2. Recent application logs
Get-Content "OpCentrix/logs/opcentrix-*.log" | Select-Object -Last 20

# 3. Test relevant functionality
dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --filter "RelevantTest" --verbosity normal
```

### **Step 2: Browser-Side Investigation**
For UI issues:
1. **Open Browser Developer Tools** (F12)
2. **Check Console Tab** for JavaScript errors
3. **Check Network Tab** for failed requests
4. **Check Elements Tab** for CSS issues

### **Step 3: Server-Side Investigation**
For backend issues:
1. **Check server logs** in `OpCentrix/logs/`
2. **Verify database state** if data-related
3. **Check service registrations** in `Program.cs`
4. **Verify model binding** for form issues

### **Step 4: Systematic Reproduction**
1. **Document exact steps** to reproduce
2. **Test with different user roles**
3. **Test in different browsers**
4. **Test with different data states**

### **Step 5: Root Cause Analysis**
Common issue categories:
- **Authentication**: Role/permission problems
- **Database**: Connection, migration, or data issues
- **Frontend**: JavaScript, CSS, or HTMX issues
- **Backend**: Service, model, or controller issues
- **Integration**: API calls or third-party services

---

## ?? **BUG REPORT TEMPLATE**

When reporting bugs, use this format:

```markdown
## Bug Report: [Brief Description]

### Environment
- **User Role**: admin/manager/scheduler/etc.
- **Browser**: Chrome/Firefox/Safari + version
- **Page/URL**: /Admin/Parts, /Scheduler, etc.
- **Build Status**: Success/Failed

### Steps to Reproduce
1. Login as [role]
2. Navigate to [page]
3. Click [button/link]
4. [Additional steps]

### Expected Behavior
[What should happen]

### Actual Behavior
[What actually happens]

### Error Messages
- **Browser Console**: [Any JavaScript errors]
- **Server Logs**: [Any server-side errors]
- **Visual Issues**: [Screenshots if helpful]

### Additional Context
- **Data State**: [Any specific data conditions]
- **Frequency**: Always/Sometimes/Rare
- **Workaround**: [If any exists]
```

---

## ? **AUTOMATED TESTING COMMANDS**

### **Run All Tests**
```powershell
# Comprehensive test suite
dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --verbosity normal

# Expected: All tests pass (100+ test cases)
# Categories: Authentication, Authorization, Services, Models, Integration
```

### **Run Specific Test Categories**
```powershell
# Authentication tests only
dotnet test --filter "AuthenticationValidationTests" --verbosity normal

# Service layer tests only  
dotnet test --filter "ServiceTests" --verbosity normal

# UI integration tests only
dotnet test --filter "IntegrationTests" --verbosity normal
```

### **Performance Testing**
```powershell
# Load testing (if implemented)
dotnet test --filter "PerformanceTests" --verbosity normal

# Memory leak detection
dotnet test --filter "MemoryTests" --verbosity normal
```

---

## ?? **SUCCESS CRITERIA CHECKLIST**

### **? Core Functionality**
- [ ] Users can login with correct credentials
- [ ] Navigation works across all user roles
- [ ] Scheduler displays and functions properly
- [ ] Jobs can be created, edited, and deleted
- [ ] Admin functions work for admin users
- [ ] Department operations function correctly

### **? Enhanced Features**  
- [ ] Stage indicators display on job blocks
- [ ] Cohort grouping works properly
- [ ] Progress bars show for multi-stage jobs
- [ ] B&T Manufacturing features accessible
- [ ] Advanced Workflows function correctly

### **? User Experience**
- [ ] Pages load quickly (< 2 seconds)
- [ ] Mobile responsiveness works
- [ ] Error messages are clear and helpful
- [ ] Session management works properly
- [ ] No JavaScript console errors

### **? Data Integrity**
- [ ] Jobs save correctly to database
- [ ] User data persists properly
- [ ] Stage transitions tracked accurately
- [ ] Cohort data maintained correctly
- [ ] Audit logs capture changes

---

## ?? **TESTING WORKFLOW FOR COLLABORATION**

### **When You Encounter a Bug:**
1. **Document** using the bug report template above
2. **Provide** exact steps to reproduce
3. **Share** any error messages or screenshots
4. **Specify** your user role and what you were trying to do

### **When I Debug With You:**
1. **Research** current system state using tools
2. **Analyze** the specific error and context
3. **Reproduce** the issue systematically  
4. **Implement** fix with proper testing
5. **Verify** fix resolves the issue completely

### **Quality Assurance Process:**
1. **Fix Applied** ? Build verification
2. **Build Success** ? Targeted testing
3. **Tests Pass** ? Integration verification
4. **Integration Success** ? User acceptance testing
5. **User Verified** ? Issue marked resolved

---

## ? **QUICK REFERENCE COMMANDS**

### **Start Testing Session**
```powershell
# Clean build and verify
dotnet clean
dotnet restore  
dotnet build OpCentrix/OpCentrix.csproj

# If successful, start application
cd OpCentrix
dotnet run

# Application available at: http://localhost:5090
```

### **Emergency Recovery**
```powershell
# If database issues occur
Remove-Item "OpCentrix/scheduler.db" -Force
# Restart application - database will be recreated

# If authentication issues occur  
# Clear browser cache and cookies, then restart application
```

### **Log Investigation**
```powershell
# View recent logs
Get-Content "OpCentrix/logs/opcentrix-*.log" | Select-Object -Last 50

# View error logs only
Get-Content "OpCentrix/logs/opcentrix-*.log" | Select-String "ERROR" | Select-Object -Last 20
```

---

## ?? **TESTING STATUS TRACKING**

### **Completed Testing Areas**
- ? **Authentication System**: All user roles tested and working
- ? **Navigation Enhancement**: Admin and main navigation tested
- ? **Build System**: Clean compilation achieved
- ? **Basic Scheduler**: Core functionality verified

### **In Progress Testing**
- ? **Manufacturing Operations**: Department-specific features
- ? **Stage-Aware Features**: Workflow progression testing
- ? **Admin System**: Comprehensive admin testing

### **Pending Testing**
- ? **Performance Testing**: Load and stress testing
- ? **Security Testing**: Authorization edge cases
- ? **Integration Testing**: End-to-end workflows

---

## ?? **READY FOR COLLABORATIVE DEBUGGING**

**This testing framework provides:**
- ? **Systematic approach** to identify issues
- ? **Clear documentation** for bug reporting
- ? **Efficient debugging** workflow for collaboration
- ? **Comprehensive coverage** of all system areas
- ? **Automated testing** integration
- ? **Quality assurance** process

**Use this guide to:**
1. **Test systematically** through all functionality
2. **Report bugs effectively** with detailed information
3. **Collaborate efficiently** on debugging and fixes
4. **Verify solutions** comprehensively
5. **Maintain quality** throughout development

---

*Testing Framework Created: January 2025*  
*Status: ? READY FOR COMPREHENSIVE TESTING & DEBUGGING*