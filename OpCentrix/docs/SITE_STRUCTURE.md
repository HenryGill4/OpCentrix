# ??? OpCentrix Site Structure Guide

## ?? Overview

This document provides a comprehensive guide to the OpCentrix application architecture, explaining the site structure, file organization, and how components work together to create a complete SLS Print Job Scheduler.

## ?? Application Architecture

### **Architecture Pattern**
OpCentrix follows the **ASP.NET Core Razor Pages** pattern with:
- **Page-Based Routing**: Each page is self-contained with its model
- **Service Layer**: Business logic separated into services
- **Repository Pattern**: Data access through Entity Framework Core
- **Dependency Injection**: Services registered and injected throughout

### **Design Principles**
- **Separation of Concerns**: Clear boundaries between UI, business logic, and data
- **Responsive Design**: Mobile-first approach with progressive enhancement
- **Role-Based Security**: Granular access control throughout the application
- **Real-Time Updates**: HTMX for seamless partial page updates
- **Error Handling**: Comprehensive logging and user-friendly error messages

## ?? Directory Structure

```
OpCentrix/
??? ?? Authorization/              # Security and authorization
?   ??? RoleRequirements.cs       # Custom authorization requirements
??? ?? Data/                       # Database context and configuration
?   ??? SchedulerContext.cs       # Main EF Core context
??? ?? Migrations/                 # Database migrations
?   ??? *_InitialCreate.cs        # Initial database schema
?   ??? *_EnhancedJobAnalytics.cs # Analytics enhancements
??? ?? Models/                     # Data models and entities
?   ??? ?? ViewModels/            # Page-specific view models
?   ??? Job.cs                    # Core scheduling entity
?   ??? Part.cs                   # Manufacturing part entity
?   ??? User.cs                   # User and authentication
?   ??? SlsMachine.cs             # SLS machine configuration
?   ??? [Department Models]       # Coating, EDM, Machining, etc.
??? ?? Pages/                      # Razor Pages (UI Layer)
?   ??? ?? Account/               # Authentication and user management
?   ??? ?? Admin/                 # Administrative interface
?   ??? ?? Api/                   # API endpoints
?   ??? ?? PrintTracking/         # Print operation tracking
?   ??? ?? Scheduler/             # Main scheduling interface
?   ??? ?? Shared/                # Shared layouts and components
?   ??? [Department Pages]        # Printing, Coating, EDM, etc.
??? ?? ProjectNotes/              # Documentation and guides
??? ?? Services/                  # Business logic layer
?   ??? ISchedulerService.cs      # Scheduling interface
?   ??? SchedulerService.cs       # Core scheduling logic
?   ??? AuthenticationService.cs  # User authentication
?   ??? PrintTrackingService.cs   # Print operation management
?   ??? SlsDataSeedingService.cs  # Database seeding
??? ?? wwwroot/                   # Static files and assets
?   ??? ?? css/                   # Stylesheets
?   ??? ?? js/                    # JavaScript files
?   ??? ?? lib/                   # Third-party libraries
?   ??? ?? images/                # Static images
??? Program.cs                    # Application startup and configuration
??? appsettings.json             # Configuration settings
??? OpCentrix.csproj             # Project file and dependencies
```

## ?? User Interface Architecture

### **Layout System**

#### **Main Layout (`_Layout.cshtml`)**
- **Role-Based Navigation**: Dynamic menu based on user permissions
- **Professional Header**: OpCentrix branding with gradient styling
- **User Profile Display**: Avatar, name, role, and quick actions
- **Responsive Design**: Collapses to hamburger menu on mobile
- **Session Management**: Visual timeout warnings and extension

#### **Admin Layout (`_AdminLayout.cshtml`)**
- **Sidebar Navigation**: Fixed 288px width with organized sections
- **Dashboard Focus**: Optimized for administrative tasks
- **Quick Actions**: Direct access to management functions
- **Return Navigation**: Easy access back to main application

### **Page Organization**

#### **1. ?? Scheduler Module (`/Pages/Scheduler/`)**
```
Scheduler/
??? Index.cshtml                 # Main scheduler interface
??? Index.cshtml.cs              # Scheduler page model
??? JobLog.cshtml                # Job history and logs
??? _MachineRow.cshtml           # Machine row partial view
??? _JobBlock.cshtml             # Individual job display
??? _AddEditJobModal.cshtml      # Job creation/editing modal
??? _FooterSummary.cshtml        # Summary statistics
??? _EmbeddedScheduler.cshtml    # Embedded scheduler component
```

**Features:**
- **Visual Timeline**: CSS Grid-based scheduling interface
- **Machine-Specific Rows**: Color-coded for TI1, TI2, INC machines
- **Interactive Job Blocks**: Drag-and-drop with conflict detection
- **Real-Time Updates**: HTMX partial updates for seamless UX
- **Multiple Zoom Levels**: Day, Hour, 30min, 15min views

#### **2. ??? Admin Module (`/Pages/Admin/`)**
```
Admin/
??? Index.cshtml                 # Admin dashboard
??? Index.cshtml.cs              # Dashboard page model
??? Jobs.cshtml                  # Job management interface
??? Jobs.cshtml.cs               # Job CRUD operations
??? Parts.cshtml                 # Parts management interface
??? Parts.cshtml.cs              # Parts CRUD operations
??? Logs.cshtml                  # Audit log viewer
??? Logs.cshtml.cs               # Log filtering and display
??? _PartForm.cshtml             # Part creation/editing form
```

**Features:**
- **Real-Time Dashboard**: Live KPIs and system status
- **Advanced Filtering**: Multi-dimensional search and filters
- **Inline Editing**: Modal-based CRUD operations
- **Audit Trail**: Complete change tracking and history
- **Data Validation**: Client and server-side validation

#### **3. ?? Authentication Module (`/Pages/Account/`)**
```
Account/
??? Login.cshtml                 # User login page
??? Login.cshtml.cs              # Authentication logic
??? Logout.cshtml.cs             # Session termination
??? Settings.cshtml              # User preferences
??? Settings.cshtml.cs           # Settings management
??? AccessDenied.cshtml          # Access denied page
??? ExtendSession.cshtml.cs      # Session extension API
```

**Features:**
- **Secure Authentication**: Cookie-based with role management
- **Session Management**: Automatic timeout with extension
- **User Preferences**: Customizable settings and themes
- **Access Control**: Role-based page access restrictions

#### **4. ?? Department Modules**
```
Printing/Index.cshtml            # SLS machine operations
Coating/Index.cshtml             # Post-processing coating
EDM/Index.cshtml                 # Electrical discharge machining
Machining.cshtml                 # Traditional machining
QC.cshtml                        # Quality control inspections
Shipping/Index.cshtml            # Order fulfillment
Media.cshtml                     # Digital media management
Analytics.cshtml                 # Performance analytics
```

**Features:**
- **Department-Specific UIs**: Tailored for each operation type
- **Embedded Schedulers**: Department-specific scheduling views
- **Process Tracking**: Operation-specific workflow management
- **Integration Points**: Connect with main scheduler system

## ?? Service Layer Architecture

### **Core Services**

#### **SchedulerService (`/Services/SchedulerService.cs`)**
```csharp
public interface ISchedulerService
{
    // Job management
    Task<SchedulerPageViewModel> GetSchedulerDataAsync(string? zoom, DateTime? startDate);
    Task<Job> CreateJobAsync(AddEditJobViewModel model, string username);
    Task<Job> UpdateJobAsync(AddEditJobViewModel model, string username);
    Task DeleteJobAsync(int jobId, string username);
    
    // Validation and business logic
    Task<bool> ValidateJobScheduleAsync(Job job, int? excludeJobId = null);
    Task<List<Job>> GetConflictingJobsAsync(Job job, int? excludeJobId = null);
    
    // Analytics and reporting
    Task<FooterSummaryViewModel> GetFooterSummaryAsync(DateTime? startDate = null);
}
```

**Responsibilities:**
- **Job Scheduling Logic**: Conflict detection and validation
- **Data Transformation**: Entity to ViewModel mapping
- **Business Rules**: Manufacturing-specific constraints
- **Performance Optimization**: Efficient data loading

#### **AuthenticationService (`/Services/AuthenticationService.cs`)**
```csharp
public interface IAuthenticationService
{
    Task<User?> ValidateUserAsync(string username, string password);
    Task<bool> IsUserActiveAsync(string username);
    Task UpdateLastLoginAsync(string username);
    Task<UserSettings> GetUserSettingsAsync(int userId);
    Task UpdateUserSettingsAsync(UserSettings settings);
}
```

**Responsibilities:**
- **User Validation**: Secure password verification
- **Session Management**: User state and preferences
- **Security Logging**: Authentication attempt tracking
- **Settings Management**: User preference storage

#### **PrintTrackingService (`/Services/PrintTrackingService.cs`)**
```csharp
public interface IPrintTrackingService
{
    Task<BuildJob> StartBuildJobAsync(StartPrintViewModel model, int userId);
    Task<BuildJob> CompleteBuildJobAsync(int buildId, PostPrintViewModel model);
    Task<DelayLog> LogDelayAsync(int buildId, string reason, int delayMinutes);
    Task<List<BuildJob>> GetActiveBuildJobsAsync();
    Task<PrintTrackingDashboardViewModel> GetDashboardDataAsync();
}
```

**Responsibilities:**
- **Print Operation Tracking**: Real build job management
- **Part Integration**: Connect with scheduled jobs
- **Delay Management**: Production delay tracking
- **Analytics**: Print performance metrics

### **Data Access Layer**

#### **SchedulerContext (`/Data/SchedulerContext.cs`)**
```csharp
public class SchedulerContext : DbContext
{
    // Core scheduling tables
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<JobLogEntry> JobLogEntries { get; set; }
    
    // SLS-specific tables
    public DbSet<SlsMachine> SlsMachines { get; set; }
    public DbSet<BuildJob> BuildJobs { get; set; }
    public DbSet<BuildJobPart> BuildJobParts { get; set; }
    
    // Future department tables (commented for initial release)
    // public DbSet<CoatingOperation> CoatingOperations { get; set; }
    // public DbSet<EDMOperation> EDMOperations { get; set; }
}
```

**Features:**
- **Entity Relationships**: Proper foreign key constraints
- **Performance Indexing**: Optimized query performance
- **Data Validation**: Entity-level validation rules
- **Migration Support**: Schema evolution management

## ?? Frontend Architecture

### **CSS Architecture (`/wwwroot/css/`)**

#### **Design System Structure**
```css
/* 1. CSS Custom Properties (Design Tokens) */
:root {
    --opcentrix-primary: #3B82F6;
    --opcentrix-secondary: #6366F1;
    /* ... complete color palette */
}

/* 2. Global Reset & Base Styles */
* { box-sizing: border-box; }
body { font-family: Inter, sans-serif; }

/* 3. Component Classes */
.opcentrix-card { /* Card component */ }
.opcentrix-button { /* Button variants */ }
.opcentrix-input { /* Form inputs */ }

/* 4. Layout Systems */
.scheduler-grid { /* CSS Grid for scheduler */ }
.admin-layout { /* Admin sidebar layout */ }

/* 5. Utility Classes */
.text-primary { color: var(--opcentrix-primary); }
.bg-gradient { /* Gradient backgrounds */ }

/* 6. Responsive Design */
@media (max-width: 768px) { /* Mobile styles */ }
@media (min-width: 1024px) { /* Desktop styles */ }

/* 7. Animation Classes */
@keyframes slideInRight { /* Smooth animations */ }
.animate-slide-in { animation: slideInRight 0.3s ease-out; }
```

#### **Component-Specific Styles**
```
css/
??? site.css                     # Main stylesheet with complete design system
??? scheduler-modal.css          # Modal-specific styles
??? [future modules]             # Department-specific stylesheets
```

### **JavaScript Architecture (`/wwwroot/js/`)**

#### **Core JavaScript Files**
```javascript
// site.js - Main application JavaScript
window.OpCentrixErrorLogger = {
    // Comprehensive error logging system
    log: function(category, operation, error, context) { /* ... */ },
    getErrorReport: function() { /* ... */ }
};

// Enhanced function wrappers
window.safeExecute = function(category, operation, fn, context) {
    // Safe execution with error handling
};

// HTMX integration
document.addEventListener('htmx:afterRequest', function(e) {
    // Global response handling
});
```

#### **Scheduler-Specific JavaScript**
```javascript
// scheduler-ui.js - Scheduler interface logic
function initializeScheduler() {
    // Grid initialization and event handlers
}

function handleJobDrop(event) {
    // Drag-and-drop job management
}

function validateJobOverlap(job) {
    // Client-side conflict detection
}
```

**Features:**
- **Error Logging**: Comprehensive client-side error tracking
- **HTMX Integration**: Seamless partial page updates
- **Real-Time Updates**: Live grid refreshing
- **Mobile Support**: Touch-friendly interactions

## ?? Data Flow Architecture

### **Request Processing Flow**

#### **1. User Interaction**
```
User Action (Click/Submit)
         ?
HTMX Request (if applicable)
         ?
ASP.NET Core Routing
         ?
Razor Page Model
```

#### **2. Business Logic Processing**
```
Page Model Method
         ?
Service Layer Call
         ?
Business Rule Validation
         ?
Database Operation (EF Core)
         ?
Response Generation
```

#### **3. Response Handling**
```
Server Response
         ?
HTMX Target Update (partial)
    OR
Full Page Render
         ?
Client-Side Enhancement
         ?
User Feedback (notifications)
```

### **Real-Time Update Flow**

#### **Scheduler Updates**
```
Job Modification Request
         ?
Validation in SchedulerService
         ?
Database Update with Audit Log
         ?
HTMX Response with Updated Machine Row
         ?
Client-Side Grid Update
         ?
Success Notification Display
```

#### **Admin Operations**
```
CRUD Operation Request
         ?
Business Rule Validation
         ?
Database Transaction
         ?
Audit Log Entry Creation
         ?
Modal Close + Grid Refresh
         ?
User Notification
```

## ?? Security Architecture

### **Authentication Flow**
```
Login Request
         ?
AuthenticationService.ValidateUserAsync()
         ?
Password Verification
         ?
User Status Check (IsActive)
         ?
Cookie Authentication Setup
         ?
Role-Based Claims Assignment
         ?
Redirect to Authorized Page
```

### **Authorization Layers**

#### **1. Page-Level Security**
```csharp
[Authorize(Policy = "AdminOnly")]
public class AdminModel : PageModel
{
    // Only admins can access
}
```

#### **2. Role-Based Policies**
```csharp
// In Program.cs
options.AddPolicy("SchedulerAccess", policy =>
    policy.RequireRole("Admin", "Manager", "Scheduler", "Operator"));
```

#### **3. Feature-Level Security**
```html
@if (User.IsInRole("Admin"))
{
    <button>Delete Job</button>
}
```

## ?? Performance Architecture

### **Database Optimization**
- **Strategic Indexing**: Query-optimized indexes on frequently accessed columns
- **Efficient Relationships**: Proper foreign key constraints with cascade rules
- **Query Optimization**: LINQ expressions optimized for SQL generation
- **Connection Pooling**: EF Core connection management

### **Frontend Performance**
- **CSS Grid**: Hardware-accelerated layout for scheduler
- **Lazy Loading**: Progressive content loading
- **HTMX Optimization**: Minimal payload partial updates
- **Caching Strategy**: Browser caching for static assets

### **Scalability Considerations**
- **Service Layer**: Business logic separated for horizontal scaling
- **Database**: SQLite for development, easily upgradeable to SQL Server
- **Session State**: Cookie-based for stateless scaling
- **Static Assets**: CDN-ready static file organization

---

## ?? Integration Points

### **Machine Integration (OPC UA)**
```csharp
public class OpcUaService : IOpcUaService
{
    Task<bool> ConnectToMachineAsync(string endpoint);
    Task<MachineStatus> GetMachineStatusAsync(string machineId);
    Task<bool> StartJobAsync(string machineId, Job job);
}
```

### **External System APIs**
- **ERP Integration**: Ready for MRP/ERP system connectivity
- **Quality Systems**: Integration points for QC data
- **Inventory Management**: Material tracking capabilities
- **Reporting Systems**: Analytics export functionality

---

## ?? Deployment Architecture

### **Development Environment**
- **Local Database**: SQLite file-based database
- **Hot Reload**: ASP.NET Core development server
- **Debug Logging**: Comprehensive error tracking
- **Sample Data**: Development seed data available

### **Production Considerations**
- **Database**: Upgrade to SQL Server for production
- **Reverse Proxy**: IIS or nginx for static file serving
- **SSL/TLS**: HTTPS enforcement for security
- **Monitoring**: Application insights and health checks

---

This comprehensive site structure provides a solid foundation for understanding how OpCentrix is organized and how its components work together to deliver a complete SLS manufacturing management solution.

---

*Last Updated: December 2024*  
*Version: 2.0.0*