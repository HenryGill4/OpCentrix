# ?? **NAVIGATION REFACTORING PLAN - B&T MANUFACTURING MES**

## ?? **CURRENT NAVIGATION STATUS**

? **Main Layout**: Enhanced with role-based navigation  
? **Admin Layout**: Professional sidebar with categorized sections  
? **Mobile Support**: Responsive hamburger menu implemented  
? **Session Management**: Advanced timeout with visual warnings  
?? **B&T Features**: Backend complete, UI integration needed  

---

## ??? **NAVIGATION ENHANCEMENT ROADMAP**

### **?? PHASE 1: MAIN LAYOUT B&T INTEGRATION**
**File**: `OpCentrix/Pages/Shared/_Layout.cshtml`  
**Priority**: **IMMEDIATE**  
**Estimated Time**: 4-6 hours

#### **Current Navigation Sections:**
```
? Print Tracking (Complete)
? Scheduling (Scheduler Grid + Master Schedule) 
? Administration (Admin-only access)
```

#### **NEW B&T Sections to Add:**
```html
<!-- B&T Manufacturing Section (NEW) -->
@if (userRole == "Admin" || userRole == "Manager" || userRole == "BTSpecialist")
{
    <div class="space-y-1">
        <h3 class="px-3 text-xs font-semibold text-amber-600 uppercase tracking-wider">
            B&T Manufacturing
        </h3>
        <a href="/BT/Dashboard" 
           class="@(ViewContext.RouteData.Values["page"]?.ToString().Contains("BT/Dashboard") == true ? "bg-amber-100 border-amber-500 text-amber-700" : "border-transparent text-amber-600 hover:bg-amber-50 hover:text-amber-700") group flex items-center px-3 py-2 text-sm font-medium border-l-4">
            <svg class="@(ViewContext.RouteData.Values["page"]?.ToString().Contains("BT/Dashboard") == true ? "text-amber-500" : "text-amber-400 group-hover:text-amber-500") flex-shrink-0 -ml-1 mr-3 h-6 w-6" 
                 fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 11.172V5l-1-1z"></path>
            </svg>
            B&T Dashboard
        </a>
        <a href="/BT/SerialNumbers" 
           class="@(ViewContext.RouteData.Values["page"]?.ToString().Contains("BT/SerialNumbers") == true ? "bg-amber-100 border-amber-500 text-amber-700" : "border-transparent text-amber-600 hover:bg-amber-50 hover:text-amber-700") group flex items-center px-3 py-2 text-sm font-medium border-l-4">
            <svg class="@(ViewContext.RouteData.Values["page"]?.ToString().Contains("BT/SerialNumbers") == true ? "text-amber-500" : "text-amber-400 group-hover:text-amber-500") flex-shrink-0 -ml-1 mr-3 h-6 w-6" 
                 fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a1.994 1.994 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z"></path>
            </svg>
            Serial Numbers
        </a>
        <a href="/BT/Compliance" 
           class="@(ViewContext.RouteData.Values["page"]?.ToString().Contains("BT/Compliance") == true ? "bg-amber-100 border-amber-500 text-amber-700" : "border-transparent text-amber-600 hover:bg-amber-50 hover:text-amber-700") group flex items-center px-3 py-2 text-sm font-medium border-l-4">
            <svg class="@(ViewContext.RouteData.Values["page"]?.ToString().Contains("BT/Compliance") == true ? "text-amber-500" : "text-amber-400 group-hover:text-amber-500") flex-shrink-0 -ml-1 mr-3 h-6 w-6" 
                 fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"></path>
            </svg>
            Compliance
        </a>
    </div>
}

<!-- Advanced Workflows Section (NEW) -->
@if (userRole == "Admin" || userRole == "Manager" || userRole == "WorkflowSpecialist")
{
    <div class="space-y-1">
        <h3 class="px-3 text-xs font-semibold text-purple-600 uppercase tracking-wider">
            Advanced Workflows
        </h3>
        <a href="/Workflows/MultiStage" 
           class="@(ViewContext.RouteData.Values["page"]?.ToString().Contains("Workflows/MultiStage") == true ? "bg-purple-100 border-purple-500 text-purple-700" : "border-transparent text-purple-600 hover:bg-purple-50 hover:text-purple-700") group flex items-center px-3 py-2 text-sm font-medium border-l-4">
            <svg class="@(ViewContext.RouteData.Values["page"]?.ToString().Contains("Workflows/MultiStage") == true ? "text-purple-500" : "text-purple-400 group-hover:text-purple-500") flex-shrink-0 -ml-1 mr-3 h-6 w-6" 
                 fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 4h6m-6 4h6m-7-7h.01M9 16h.01"></path>
            </svg>
            Multi-Stage Jobs
        </a>
        <a href="/Workflows/Resources" 
           class="@(ViewContext.RouteData.Values["page"]?.ToString().Contains("Workflows/Resources") == true ? "bg-purple-100 border-purple-500 text-purple-700" : "border-transparent text-purple-600 hover:bg-purple-50 hover:text-purple-700") group flex items-center px-3 py-2 text-sm font-medium border-l-4">
            <svg class="@(ViewContext.RouteData.Values["page"]?.ToString().Contains("Workflows/Resources") == true ? "text-purple-500" : "text-purple-400 group-hover:text-purple-500") flex-shrink-0 -ml-1 mr-3 h-6 w-6" 
                 fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 11.172V5l-1-1z"></path>
            </svg>
            Resource Scheduling
        </a>
    </div>
}
```

#### **Mobile Menu Updates:**
```html
<!-- Add B&T sections to mobile menu -->
@if (isAuthenticated && (userRole == "Admin" || userRole == "Manager" || userRole == "BTSpecialist"))
{
    <div class="border-t border-gray-200 pt-2 mt-2">
        <h4 class="px-3 text-xs font-semibold text-amber-600 uppercase tracking-wider mb-2">B&T Manufacturing</h4>
        <a href="/BT/Dashboard" class="block px-3 py-2 text-amber-600 hover:text-amber-700 font-medium">B&T Dashboard</a>
        <a href="/BT/SerialNumbers" class="block px-3 py-2 text-amber-600 hover:text-amber-700 font-medium">Serial Numbers</a>
        <a href="/BT/Compliance" class="block px-3 py-2 text-amber-600 hover:text-amber-700 font-medium">Compliance</a>
    </div>
}
@if (isAuthenticated && (userRole == "Admin" || userRole == "Manager" || userRole == "WorkflowSpecialist"))
{
    <div class="border-t border-gray-200 pt-2 mt-2">
        <h4 class="px-3 text-xs font-semibold text-purple-600 uppercase tracking-wider mb-2">Advanced Workflows</h4>
        <a href="/Workflows/MultiStage" class="block px-3 py-2 text-purple-600 hover:text-purple-700 font-medium">Multi-Stage Jobs</a>
        <a href="/Workflows/Resources" class="block px-3 py-2 text-purple-600 hover:text-purple-700 font-medium">Resource Scheduling</a>
    </div>
}
```

---

### **?? PHASE 2: ADMIN LAYOUT B&T ENHANCEMENT**
**File**: `OpCentrix/Pages/Admin/Shared/_AdminLayout.cshtml`  
**Priority**: **HIGH**  
**Estimated Time**: 3-4 hours

#### **Current Admin Sections:**
```
? Overview (Dashboard, Scheduler, Stages, Print Tracking)
? Resources (Machines, Parts, Shifts, Multi-Stage Jobs)  
? Quality (Inspection Checkpoints, Defect Categories)
? Data (Job Archive, Database Management, System Logs)
? Administration (User Management, Roles & Permissions, System Settings)
```

#### **NEW B&T Admin Sections:**
```html
<!-- B&T Administration Section (NEW) -->
<div class="mb-6">
    <h3 class="px-3 text-xs font-semibold text-amber-600 uppercase tracking-wider mb-2">B&T Administration</h3>
    <a href="/Admin/BT/PartClassifications" class="nav-item group flex items-center px-3 py-2 text-sm font-medium rounded-lg @(ViewContext.RouteData.Values["Page"]?.ToString() == "/Admin/BT/PartClassifications" ? "nav-item-active" : "text-gray-700 hover:text-gray-900 hover:bg-gray-100")">
        <svg class="mr-3 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14-4H9m4 8H9m-5-8h.01M4 15h.01M4 19h.01"></path>
        </svg>
        Part Classifications
    </a>
    <a href="/Admin/BT/SerialNumbers" class="nav-item group flex items-center px-3 py-2 text-sm font-medium rounded-lg @(ViewContext.RouteData.Values["Page"]?.ToString() == "/Admin/BT/SerialNumbers" ? "nav-item-active" : "text-gray-700 hover:text-gray-900 hover:bg-gray-100")">
        <svg class="mr-3 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a1.994 1.994 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z"></path>
        </svg>
        Serial Number Management
    </a>
    <a href="/Admin/BT/ComplianceRequirements" class="nav-item group flex items-center px-3 py-2 text-sm font-medium rounded-lg @(ViewContext.RouteData.Values["Page"]?.ToString() == "/Admin/BT/ComplianceRequirements" ? "nav-item-active" : "text-gray-700 hover:text-gray-900 hover:bg-gray-100")">
        <svg class="mr-3 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"></path>
        </svg>
        Compliance Requirements
    </a>
    <a href="/Admin/BT/ComplianceDocuments" class="nav-item group flex items-center px-3 py-2 text-sm font-medium rounded-lg @(ViewContext.RouteData.Values["Page"]?.ToString() == "/Admin/BT/ComplianceDocuments" ? "nav-item-active" : "text-gray-700 hover:text-gray-900 hover:bg-gray-100")">
        <svg class="mr-3 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path>
        </svg>
        Compliance Documents
    </a>
</div>

<!-- B&T Analytics Section (NEW) -->
<div class="mb-6">
    <h3 class="px-3 text-xs font-semibold text-amber-600 uppercase tracking-wider mb-2">B&T Analytics</h3>
    <a href="/Admin/BT/Dashboard" class="nav-item group flex items-center px-3 py-2 text-sm font-medium rounded-lg @(ViewContext.RouteData.Values["Page"]?.ToString() == "/Admin/BT/Dashboard" ? "nav-item-active" : "text-gray-700 hover:text-gray-900 hover:bg-gray-100")">
        <svg class="mr-3 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>
        </svg>
        Compliance Dashboard
    </a>
    <a href="/Admin/BT/Reports" class="nav-item group flex items-center px-3 py-2 text-sm font-medium rounded-lg @(ViewContext.RouteData.Values["Page"]?.ToString() == "/Admin/BT/Reports" ? "nav-item-active" : "text-gray-700 hover:text-gray-900 hover:bg-gray-100")">
        <svg class="mr-3 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path>
        </svg>
        B&T Reports
    </a>
    <a href="/Admin/BT/QualityMetrics" class="nav-item group flex items-center px-3 py-2 text-sm font-medium rounded-lg @(ViewContext.RouteData.Values["Page"]?.ToString() == "/Admin/BT/QualityMetrics" ? "nav-item-active" : "text-gray-700 hover:text-gray-900 hover:bg-gray-100")">
        <svg class="mr-3 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01"></path>
        </svg>
        Quality Metrics
    </a>
</div>

<!-- Advanced Workflows Section (NEW) -->  
<div class="mb-6">
    <h3 class="px-3 text-xs font-semibold text-purple-600 uppercase tracking-wider mb-2">Advanced Workflows</h3>
    <a href="/Admin/Workflows/Templates" class="nav-item group flex items-center px-3 py-2 text-sm font-medium rounded-lg @(ViewContext.RouteData.Values["Page"]?.ToString() == "/Admin/Workflows/Templates" ? "nav-item-active" : "text-gray-700 hover:text-gray-900 hover:bg-gray-100")">
        <svg class="mr-3 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 5a1 1 0 011-1h14a1 1 0 011 1v2a1 1 0 01-1 1H5a1 1 0 01-1-1V5zM4 13a1 1 0 011-1h6a1 1 0 011 1v6a1 1 0 01-1 1H5a1 1 0 01-1-1v-6zM16 13a1 1 0 011-1h2a1 1 0 011 1v6a1 1 0 01-1 1h-2a1 1 0 01-1-1v-6z"></path>
        </svg>
        Workflow Templates
    </a>
    <a href="/Admin/Workflows/Resources" class="nav-item group flex items-center px-3 py-2 text-sm font-medium rounded-lg @(ViewContext.RouteData.Values["Page"]?.ToString() == "/Admin/Workflows/Resources" ? "nav-item-active" : "text-gray-700 hover:text-gray-900 hover:bg-gray-100")">
        <svg class="mr-3 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 11.172V5l-1-1z"></path>
        </svg>
        Resource Management
    </a>
</div>
```

---

### **?? PHASE 3: NEW PAGE ROUTING SETUP**
**Files to Create/Modify:**  
**Priority**: **MEDIUM**  
**Estimated Time**: 2-3 hours

#### **Route Configuration Updates:**
```csharp
// Add to Program.cs
app.MapRazorPages();

// B&T Manufacturing Routes
app.MapRazorPagesConventions(options =>
{
    options.AddAreaPageRoute("BT", "/Dashboard", "/BT/Dashboard");
    options.AddAreaPageRoute("BT", "/SerialNumbers", "/BT/SerialNumbers");
    options.AddAreaPageRoute("BT", "/Compliance", "/BT/Compliance");
    options.AddAreaPageRoute("BT", "/PartClassifications", "/BT/PartClassifications");
});

// Advanced Workflow Routes
app.MapRazorPagesConventions(options =>
{
    options.AddAreaPageRoute("Workflows", "/MultiStage", "/Workflows/MultiStage");
    options.AddAreaPageRoute("Workflows", "/Resources", "/Workflows/Resources");
});

// Admin B&T Routes
app.MapRazorPagesConventions(options =>
{
    options.AddAreaPageRoute("Admin", "/BT/PartClassifications", "/Admin/BT/PartClassifications");
    options.AddAreaPageRoute("Admin", "/BT/SerialNumbers", "/Admin/BT/SerialNumbers");
    options.AddAreaPageRoute("Admin", "/BT/ComplianceRequirements", "/Admin/BT/ComplianceRequirements");
    options.AddAreaPageRoute("Admin", "/BT/ComplianceDocuments", "/Admin/BT/ComplianceDocuments");
});
```

#### **Directory Structure to Create:**
```
OpCentrix/Pages/
??? BT/                          (NEW - B&T Manufacturing Pages)
?   ??? Dashboard.cshtml
?   ??? Dashboard.cshtml.cs
?   ??? SerialNumbers.cshtml
?   ??? SerialNumbers.cshtml.cs
?   ??? Compliance.cshtml
?   ??? Compliance.cshtml.cs
?   ??? Shared/
?       ??? _BTLayout.cshtml
?       ??? _SerialNumberForm.cshtml
?       ??? _ComplianceForm.cshtml
??? Workflows/                   (NEW - Advanced Workflow Pages)
?   ??? MultiStage.cshtml
?   ??? MultiStage.cshtml.cs
?   ??? Resources.cshtml
?   ??? Resources.cshtml.cs
?   ??? Shared/
?       ??? _WorkflowLayout.cshtml
?       ??? _StageForm.cshtml
??? Admin/
    ??? BT/                      (NEW - B&T Admin Pages)
        ??? PartClassifications.cshtml
        ??? PartClassifications.cshtml.cs
        ??? SerialNumbers.cshtml
        ??? SerialNumbers.cshtml.cs
        ??? ComplianceRequirements.cshtml
        ??? ComplianceRequirements.cshtml.cs
        ??? ComplianceDocuments.cshtml
        ??? ComplianceDocuments.cshtml.cs
        ??? Shared/
            ??? _BTAdminLayout.cshtml
            ??? _PartClassificationForm.cshtml
            ??? _SerialNumberForm.cshtml
            ??? _ComplianceForm.cshtml
```

---

### **?? PHASE 4: ROLE-BASED ACCESS CONTROL**
**Files to Modify:**  
**Priority**: **HIGH**  
**Estimated Time**: 2-3 hours

#### **New User Roles to Add:**
```csharp
// Add to AuthenticationService.cs
public static class BTRoles
{
    public const string BTSpecialist = "BTSpecialist";
    public const string WorkflowSpecialist = "WorkflowSpecialist";
    public const string ComplianceSpecialist = "ComplianceSpecialist";
    public const string QualitySpecialist = "QualitySpecialist";
}

// Role hierarchy for B&T features
public static bool HasBTAccess(string userRole)
{
    return userRole == "Admin" || 
           userRole == "Manager" || 
           userRole == "BTSpecialist" ||
           userRole == "ComplianceSpecialist";
}

public static bool HasWorkflowAccess(string userRole)  
{
    return userRole == "Admin" || 
           userRole == "Manager" || 
           userRole == "WorkflowSpecialist" ||
           userRole == "BTSpecialist";
}
```

#### **Authorization Policies:**
```csharp
// Add to Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BTAccess", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Admin", "Manager", "BTSpecialist", "ComplianceSpecialist"));
        
    options.AddPolicy("WorkflowAccess", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Admin", "Manager", "WorkflowSpecialist", "BTSpecialist"));
        
    options.AddPolicy("ComplianceAccess", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Admin", "Manager", "ComplianceSpecialist"));
});
```

---

### **?? PHASE 5: ENHANCED USER EXPERIENCE**
**Files to Create/Modify:**  
**Priority**: **MEDIUM**  
**Estimated Time**: 3-4 hours

#### **Navigation Breadcrumbs Component:**
```html
<!-- Create: OpCentrix/Pages/Shared/_Breadcrumbs.cshtml -->
@{
    var currentPage = ViewContext.RouteData.Values["page"]?.ToString();
    var pageSegments = currentPage?.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>();
}

<nav class="flex mb-4" aria-label="Breadcrumb">
    <ol class="inline-flex items-center space-x-1 md:space-x-3">
        <li class="inline-flex items-center">
            <a href="/" class="inline-flex items-center text-sm font-medium text-gray-700 hover:text-indigo-600">
                <svg class="w-4 h-4 mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path d="M10.707 2.293a1 1 0 00-1.414 0l-7 7a1 1 0 001.414 1.414L4 10.414V17a1 1 0 001 1h2a1 1 0 001-1v-2a1 1 0 011-1h2a1 1 0 011 1v2a1 1 0 001 1h2a1 1 0 001-1v-6.586l.293.293a1 1 0 001.414-1.414l-7-7z"></path>
                </svg>
                Home
            </a>
        </li>
        @if (pageSegments.Any())
        {
            @for (int i = 0; i < pageSegments.Count; i++)
            {
                var segment = pageSegments[i];
                var isLast = i == pageSegments.Count - 1;
                var url = "/" + string.Join("/", pageSegments.Take(i + 1));
                
                <li>
                    <div class="flex items-center">
                        <svg class="w-6 h-6 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                            <path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd"></path>
                        </svg>
                        @if (isLast)
                        {
                            <span class="ml-1 text-sm font-medium text-gray-500 md:ml-2">@segment.Replace("BT", "B&T").Replace("Admin", "Administration")</span>
                        }
                        else
                        {
                            <a href="@url" class="ml-1 text-sm font-medium text-gray-700 hover:text-indigo-600 md:ml-2">@segment.Replace("BT", "B&T").Replace("Admin", "Administration")</a>
                        }
                    </div>
                </li>
            }
        }
    </ol>
</nav>
```

#### **Quick Actions Component:**
```html
<!-- Create: OpCentrix/Pages/Shared/_QuickActions.cshtml -->
@{
    var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
}

<div class="bg-white shadow rounded-lg p-4 mb-6">
    <h3 class="text-lg font-medium text-gray-900 mb-4">Quick Actions</h3>
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        
        @if (userRole == "Admin" || userRole == "Manager" || userRole == "BTSpecialist")
        {
            <a href="/BT/SerialNumbers/Create" class="flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-amber-600 hover:bg-amber-700">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path>
                </svg>
                New Serial Number
            </a>
            
            <a href="/BT/Compliance/Documents/Create" class="flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-green-600 hover:bg-green-700">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path>
                </svg>
                New Document
            </a>
        }
        
        @if (userRole == "Admin" || userRole == "Manager")
        {
            <a href="/Admin/BT/PartClassifications/Create" class="flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14-4H9m4 8H9m-5-8h.01M4 15h.01M4 19h.01"></path>
                </svg>
                New Classification
            </a>
            
            <a href="/Workflows/MultiStage/Create" class="flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-purple-600 hover:bg-purple-700">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"></path>
                </svg>
                New Workflow
            </a>
        }
    </div>
</div>
```

---

## ?? **IMPLEMENTATION TIMELINE**

### **Week 1: Navigation Foundation**
```powershell
# Day 1-2: Main Layout Enhancement
# - Add B&T Manufacturing section
# - Add Advanced Workflows section  
# - Update mobile navigation

# Day 3-4: Admin Layout Enhancement
# - Add B&T Administration section
# - Add B&T Analytics section
# - Add Advanced Workflows section

# Day 5: Route Setup & Testing
# - Configure new page routes
# - Create directory structure
# - Test navigation functionality
```

### **Week 2: Role-Based Access**
```powershell
# Day 1-2: Role Definition
# - Add new B&T user roles
# - Configure authorization policies
# - Update role hierarchy

# Day 3-4: Access Control Testing
# - Test role-based menu visibility
# - Verify page access permissions
# - Update user management

# Day 5: Documentation & Training
# - Document new navigation structure
# - Create role permission matrix
# - Prepare user training materials
```

### **Week 3: Enhanced UX Components**
```powershell
# Day 1-2: Breadcrumbs & Quick Actions
# - Implement breadcrumb navigation
# - Create quick actions component
# - Add contextual help

# Day 3-4: Search & Filtering
# - Add global search functionality
# - Implement category filtering
# - Create smart navigation suggestions

# Day 5: Polish & Optimization
# - Performance optimization
# - Mobile UX refinement
# - Accessibility improvements
```

---

## ?? **SUCCESS METRICS**

### **Navigation Performance**
- ? **Page Load Speed**: < 2 seconds for navigation
- ? **Mobile Responsiveness**: Perfect on all devices
- ? **Accessibility**: WCAG 2.1 compliance
- ? **User Experience**: Intuitive, context-aware navigation

### **B&T Integration**
- ? **Role-Based Access**: Appropriate menus for each role
- ? **Feature Discovery**: Easy access to B&T features
- ? **Workflow Integration**: Seamless navigation between stages
- ? **Compliance Support**: Quick access to regulatory features

### **Admin Experience**
- ? **Organized Layout**: Logical categorization of admin functions
- ? **Quick Access**: Fast navigation to common tasks
- ? **Status Visibility**: Clear system status indicators
- ? **Efficient Workflow**: Minimal clicks to complete tasks

---

## ?? **IMMEDIATE NEXT STEPS**

```powershell
# Verify current foundation
Set-Location "C:\Users\Henry\source\repos\OpCentrix"
dotnet build
dotnet test --verbosity minimal

# Start Phase 1: Main Layout Enhancement
Write-Host "?? Starting navigation refactoring..." -ForegroundColor Green
Write-Host "   Phase 1: Main Layout B&T Integration" -ForegroundColor Cyan
Write-Host "   Foundation: 95% test success maintained" -ForegroundColor Cyan
```

**?? This navigation refactoring will provide the foundation for the complete B&T Manufacturing Execution System, ensuring users can easily discover and access all the powerful new B&T features while maintaining the excellent 95% test success rate!**