# 🎯 OpCentrix - SLS Metal Printing Scheduler

**Version 4.0** - Complete Manufacturing Execution System

OpCentrix is a comprehensive manufacturing execution system for SLS (Selective Laser Sintering) metal printing operations, featuring complete admin control, quality management, data archival, multi-stage workflow, and real-time production analytics.

---

## 🚀 **Quick Start**

### **Prerequisites**
- .NET 8 SDK
- SQLite (included)
- Visual Studio 2022 or VS Code

### **Installation**
```powershell
# Clone the repository
git clone <repository-url>
cd OpCentrix

# Restore packages
dotnet restore

# Build the application
dotnet build

# Run the application
cd OpCentrix
dotnet run
```

**Application URL**: `http://localhost:5090`

### **Default Login Credentials**
```
Admin User:     admin/admin123
Manager:        manager/manager123
Scheduler:      scheduler/scheduler123
Operator:       operator/operator123
```

---

## 🏭 **Complete Manufacturing System**

### **🎛️ Core System Features**

#### **1. Production Scheduler (`/Scheduler`)**
- **Multi-machine scheduling**: TI1, TI2, INC machines with capacity management
- **Drag & drop interface**: Interactive job scheduling with visual feedback
- **Zoom controls**: 1-hour to 12-hour views with optimized performance
- **Color coding**: Status-based visual indicators for quick assessment
- **Two-month planning**: Extended horizon for production planning
- **Conflict detection**: Automatic overlap prevention and validation

#### **2. Master Schedule Analytics (`/Scheduler/MasterSchedule`)**
- **Real-time dashboard**: Live production metrics and KPI tracking
- **Machine utilization**: Capacity analysis and optimization insights
- **Timeline visualization**: Interactive schedule view with conflict detection
- **Alert system**: Critical issue notification with severity levels
- **Resource planning**: Capacity analysis and workload balancing
- **Export capabilities**: Excel, PDF, and CSV reporting ready

#### **3. Multi-Stage Workflow Management (`/Admin/Stages`)**
- **Stage management**: Control multi-stage job progression
- **Department permissions**: Role-based stage access control
- **Progress tracking**: Real-time stage completion monitoring
- **Resource allocation**: Machine and operator assignment per stage
- **Timeline coordination**: Cross-stage dependency management

---

## 🛡️ **Complete Admin Control System**

### **Admin Panel Access**
- **URL**: `http://localhost:5090/Admin`
- **Required Role**: Admin
- **Features**: Complete system administration and management

### **🎛️ Comprehensive Admin Features**

#### **1. Admin Dashboard (`/Admin`)**
- **System overview**: Complete statistics and health monitoring
- **User metrics**: Account counts, role distribution, activity tracking
- **Machine status**: Real-time machine availability and utilization
- **Job analytics**: Production metrics, efficiency trends, quality scores
- **Quick actions**: Direct access to all admin functions

#### **2. User Management (`/Admin/Users`)**
- **Complete CRUD operations**: Create, edit, delete user accounts
- **Role assignment**: Assign roles with granular permission control
- **Security features**: Password reset, account enable/disable
- **Audit protection**: Cannot delete last admin or own account
- **Search and filtering**: Advanced user discovery and management
- **Statistics dashboard**: User distribution and activity analytics

#### **3. Enhanced Machine Management (`/Admin/Machines`)**
- **Dynamic configuration**: Add/remove SLS machines with specifications
- **Capability management**: Assign machine capabilities and parameters
- **Material assignments**: Configure compatible materials per machine
- **Status tracking**: Real-time machine availability and health
- **OPC UA integration**: Machine communication protocol support
- **Maintenance scheduling**: Planned downtime and service intervals

#### **4. Advanced Parts Management (`/Admin/Parts`)**
- **Part library**: Comprehensive part definitions and specifications
- **Admin overrides**: Duration and parameter overrides with audit trail
- **Material specifications**: Material requirements and constraints
- **Quality checkpoints**: Inspection requirements per part
- **Cost analysis**: Material and labor cost tracking
- **Scheduler integration**: Override values used in production planning

#### **5. Role-Based Permission System (`/Admin/Roles`)**
- **Granular permissions**: Feature-level access control matrix
- **Visual permission grid**: Interactive role and feature management
- **Permission copying**: Duplicate permissions between roles
- **Default templates**: Reset to standard permission sets
- **Security validation**: Comprehensive access control enforcement

#### **6. Global System Settings (`/Admin/Settings`)**
- **Runtime configuration**: Dynamic settings without restarts
- **Category organization**: Grouped settings for easy management
- **Scheduler parameters**: Changeover times, cooldown periods
- **Operation settings**: Shift schedules, maintenance windows
- **Quality thresholds**: Inspection criteria and pass rates
- **System behavior**: Session timeouts, logging levels

#### **7. Operating Shift Management (`/Admin/Shifts`)**
- **Calendar interface**: Visual weekly shift planning
- **Shift definitions**: Start/end times, shift types, capacity limits
- **Overlap handling**: Multiple shifts per day with validation
- **Holiday management**: Special schedules and non-working days
- **Scheduler integration**: Job validation against shift windows

#### **8. Quality Management System**

##### **Inspection Checkpoints (`/Admin/Checkpoints`)**
- **Quality control configuration**: Define inspection requirements per part
- **Checkpoint types**: Dimensional, visual, functional, material checks
- **Sequence management**: Order and dependency management
- **Tolerance specifications**: Pass/fail criteria and measurement ranges
- **Documentation requirements**: Notes, photos, certificates

##### **Defect Category Management (`/Admin/Defects`)**
- **Defect classification**: Comprehensive defect category system
- **Severity levels**: 1-Critical to 5-Minor with color coding
- **Category groups**: Surface, dimensional, material, functional defects
- **Process assignment**: Applicable manufacturing processes
- **Corrective actions**: Standard procedures and prevention methods
- **Statistics tracking**: Defect trends and quality metrics

#### **9. Data Management & Archival (`/Admin/Archive`)**
- **Job archival system**: Archive completed jobs for performance
- **Bulk operations**: Multi-select archive and cleanup tools
- **Archive statistics**: Comprehensive analytics and reporting
- **Cleanup recommendations**: Smart suggestions based on job age
- **Restore functionality**: Restore archived jobs when needed
- **Retention policies**: Configurable data retention periods

#### **10. Database Management (`/Admin/Database`)**
- **Export/Import tools**: Complete data backup and migration
- **Diagnostics**: Database health monitoring and optimization
- **Schema validation**: Database integrity checking
- **Performance monitoring**: Query performance and statistics
- **Maintenance tools**: Cleanup and optimization utilities

#### **11. System Monitoring (`/Admin/Logs`)**
- **Comprehensive logging**: Application, error, and audit logs
- **Log filtering**: Search by level, date, content, and user
- **Download capabilities**: Export logs for analysis
- **Retention management**: Automatic log rotation and cleanup
- **Real-time monitoring**: Live log streaming for troubleshooting

---

## 📋 **User Roles & Comprehensive Permissions**

### **🛡️ Admin**
- **Access**: Complete system administration and management
- **Features**: All admin panels, user management, system configuration
- **Special privileges**: System settings, user roles, database management
- **Restrictions**: Cannot delete own account or last admin user

### **👨‍💼 Manager**
- **Access**: Management oversight, reporting, and operational control
- **Features**: Scheduler, analytics, stage management, quality oversight
- **Reports**: Production metrics, quality reports, efficiency analytics
- **Restrictions**: Cannot modify system settings or user accounts

### **📅 Scheduler**
- **Access**: Production scheduling and job management
- **Features**: Scheduler grid, job creation/editing, machine allocation
- **Planning**: Capacity planning, resource optimization, timeline management
- **Restrictions**: Cannot access admin functions or modify system settings

### **👷 Operator**
- **Access**: Operational task execution and status updates
- **Features**: Job tracking, status updates, quality reporting
- **Operations**: Start/complete jobs, record delays, update progress
- **Restrictions**: Read-only scheduler access, no admin functions

### **🔧 Manufacturing Specialists**
- **PrintingSpecialist**: SLS printing operations and machine management
- **CoatingSpecialist**: Post-processing operations and coating procedures
- **EDMSpecialist**: EDM machining operations and programming
- **QCSpecialist**: Quality control inspections and defect reporting
- **ShippingSpecialist**: Packaging, shipping, and customer fulfillment
- **MachiningSpecialist**: CNC machining and secondary operations
- **MediaSpecialist**: Media preparation and powder management
- **Analyst**: Reporting, analytics, and performance optimization

---

## 🏗️ **Complete System Architecture**

### **Enhanced Project Structure**
```
📁 OpCentrix/
├── 📁 Pages/
│   ├── 📁 Admin/                    # Complete admin control system
│   │   ├── 📁 Shared/               # Admin shared components
│   │   ├── Index.cshtml             # Admin dashboard
│   │   ├── Users.cshtml             # User management
│   │   ├── Machines.cshtml          # Machine management
│   │   ├── Parts.cshtml             # Parts management
│   │   ├── Roles.cshtml             # Role permissions
│   │   ├── Settings.cshtml          # System settings
│   │   ├── Shifts.cshtml            # Operating shifts
│   │   ├── Stages.cshtml            # Multi-stage management
│   │   ├── Checkpoints.cshtml       # Quality checkpoints
│   │   ├── Defects.cshtml           # Defect categories
│   │   ├── Archive.cshtml           # Job archival
│   │   ├── Database.cshtml          # Database management
│   │   └── Logs.cshtml              # System logs
│   └── 📁 Scheduler/                # Production scheduling
│       ├── Index.cshtml             # Main scheduler grid
│       └── MasterSchedule.cshtml    # Analytics dashboard
├── 📁 Services/
│   ├── 📁 Admin/                    # Admin business logic
│   │   ├── AdminDashboardService.cs
│   │   ├── SystemSettingService.cs
│   │   ├── RolePermissionService.cs
│   │   ├── MachineManagementService.cs
│   │   ├── MultiStageJobService.cs
│   │   ├── InspectionCheckpointService.cs
│   │   ├── DefectCategoryService.cs
│   │   ├── JobArchiveService.cs
│   │   ├── DatabaseManagementService.cs
│   │   └── LogViewerService.cs
│   ├── SchedulerService.cs          # Core scheduling logic
│   └── MasterScheduleService.cs     # Analytics and reporting
├── 📁 Models/                       # Complete data model
│   ├── 📁 ViewModels/               # UI view models
│   ├── User.cs                      # User accounts
│   ├── Machine.cs                   # Machine definitions
│   ├── Part.cs                      # Part specifications
│   ├── Job.cs                       # Production jobs
│   ├── JobStage.cs                  # Multi-stage workflow
│   ├── SystemSetting.cs             # Configuration
│   ├── RolePermission.cs            # Permissions
│   ├── OperatingShift.cs            # Shift definitions
│   ├── InspectionCheckpoint.cs      # Quality checkpoints
│   ├── DefectCategory.cs            # Defect classification
│   ├── ArchivedJob.cs               # Archived job data
│   ├── MachineCapability.cs         # Machine specifications
│   ├── AdminAlert.cs                # System alerts
│   └── FeatureToggle.cs             # Feature management
└── 📁 Data/                         # Database layer
    └── SchedulerContext.cs          # Entity Framework context
```

### **Advanced Database Design**
- **SQLite Database**: High-performance file-based database
- **Entity Framework Core 8**: Latest ORM with performance optimizations
- **Code-first migrations**: Version-controlled schema evolution
- **Automatic seeding**: Default data and test users
- **Archive system**: Historical data management with cleanup
- **Index optimization**: Performance-tuned queries

### **Comprehensive Services Architecture**
- **Dependency injection**: All services registered with proper lifetimes
- **Interface abstractions**: Fully testable service implementations
- **Async/await patterns**: Performance-optimized async operations
- **Error handling**: Comprehensive logging and exception management
- **Caching strategies**: Performance optimization for frequent data
- **Transaction management**: ACID compliance for critical operations

---

## 🔧 **Advanced Configuration**

### **Application Settings**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=scheduler.db"
  },
  "Urls": "http://localhost:5090",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "OpCentrix": "Debug"
    }
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/opcentrix-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

### **Complete System Settings (Database)**
| Category | Setting | Default | Description |
|----------|---------|---------|-------------|
| **Scheduler** | `DefaultChangeoverDurationHours` | 3 | Material changeover time |
| **Scheduler** | `DefaultCooldownTimeHours` | 1 | Machine cooldown time |
| **Scheduler** | `MaxConcurrentJobsPerMachine` | 1 | Job capacity per machine |
| **Operations** | `DefaultShiftStartTime` | 08:00 | Standard shift start |
| **Operations** | `DefaultShiftEndTime` | 17:00 | Standard shift end |
| **Operations** | `MaintenanceWindowHours` | 2 | Weekly maintenance time |
| **Quality** | `DefaultQualityThreshold` | 95.0 | Quality pass threshold |
| **Quality** | `RequiredInspectionCheckpoints` | 3 | Minimum checkpoints |
| **Archive** | `AutoArchiveAfterDays` | 30 | Automatic archival period |
| **Archive** | `ArchiveRetentionDays` | 365 | Archive retention period |
| **System** | `SessionTimeoutMinutes` | 120 | User session timeout |
| **System** | `LogRetentionDays` | 30 | Log file retention |
| **Notifications** | `EnableEmailAlerts` | true | Email notification system |
| **Performance** | `DatabaseCleanupIntervalHours` | 24 | Auto-cleanup frequency |

---

## 🧪 **Comprehensive Testing**

### **Test Suite Overview**
- **Unit Tests**: 63+ tests covering core functionality
- **Integration Tests**: End-to-end workflow validation
- **Authentication Tests**: Security and access control validation
- **Database Tests**: Data integrity and migration testing
- **Performance Tests**: Load testing and optimization validation

### **Test Commands**
```powershell
# Run all tests
dotnet test --verbosity minimal

# Run with coverage reporting
dotnet test --collect:"XPlat Code Coverage"

# Run specific test categories
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Authentication"
dotnet test --filter "Category=Performance"

# Baseline validation tests
dotnet test --filter "FullyQualifiedName~BaselineValidationTests"
```

### **Complete Test User Matrix**
```
Admin Users:
admin/admin123              (Admin - Full system access)

Management Users:
manager/manager123          (Manager - Oversight and reporting)
supervisor/supervisor123    (Supervisor - Operational management)

Operations Users:
scheduler/scheduler123      (Scheduler - Production planning)
operator/operator123        (Operator - Job execution)

Specialist Users:
printer/printer123          (PrintingSpecialist - SLS operations)
coating/coating123          (CoatingSpecialist - Post-processing)
edm/edm123                 (EDMSpecialist - EDM machining)
qc/qc123                   (QCSpecialist - Quality control)
shipping/shipping123        (ShippingSpecialist - Fulfillment)
machining/machining123      (MachiningSpecialist - CNC operations)
media/media123             (MediaSpecialist - Media preparation)
analyst/analyst123          (Analyst - Reporting and analytics)
```

---

## 📊 **Complete Production Features**

### **Enhanced Scheduler Grid**
- **Multi-machine visualization**: TI1, TI2, INC machines with capacity indicators
- **Interactive scheduling**: Drag & drop with snap-to-grid precision
- **Advanced zoom controls**: 1-hour to 12-hour views with smooth transitions
- **Status color coding**: Real-time visual status indicators
- **Conflict prevention**: Automatic overlap detection and resolution
- **Resource optimization**: Intelligent job placement suggestions

### **Comprehensive Print Tracking**
- **Real-time monitoring**: Live job status with automatic updates
- **Delay analysis**: Start time variance tracking and root cause analysis
- **Resource consumption**: Gas, powder, and energy usage monitoring
- **Operator accountability**: User attribution and performance tracking
- **Quality integration**: Inspection checkpoint completion tracking

### **Advanced Multi-Stage Workflow**
- **Stage coordination**: Cross-department workflow management
- **Progress tracking**: Real-time stage completion monitoring
- **Resource allocation**: Machine and operator assignment optimization
- **Timeline management**: Critical path analysis and bottleneck identification
- **Quality gates**: Stage-specific quality checkpoints and approvals

### **Quality Management System**
- **Inspection workflows**: Comprehensive quality control processes
- **Defect tracking**: Complete defect classification and analysis
- **Corrective actions**: Standard procedures and prevention methods
- **Statistical analysis**: Quality trends and improvement opportunities
- **Compliance reporting**: Audit-ready quality documentation

### **Data Management & Analytics**
- **Job archival**: Automated data lifecycle management
- **Performance analytics**: Production efficiency and cost analysis
- **Predictive insights**: Machine utilization and capacity planning
- **Export capabilities**: Comprehensive reporting and data export
- **Historical analysis**: Long-term trend analysis and optimization

---

## 🔍 **Complete Troubleshooting Guide**

### **System Startup Issues**

#### **Port Conflicts**
```powershell
# Check port usage
netstat -ano | findstr :5090

# Kill conflicting process
taskkill /PID <ProcessID> /F

# Use alternative port
$env:ASPNETCORE_URLS = "http://localhost:5091"
dotnet run
```

#### **Database Issues**
```powershell
# Reset database (development only)
Remove-Item -Force scheduler.db*
dotnet run  # Database will be recreated with seeded data

# Update database schema
dotnet ef database update

# Create new migration
dotnet ef migrations add "YourMigrationName"
```

### **Authentication Problems**

#### **Admin Access Issues**
1. **Verify admin user exists**: Check database or use seeded admin account
2. **Check authentication configuration**: Verify `Program.cs` setup
3. **Validate authorization policies**: Ensure `AdminOnly` policy is configured
4. **Clear browser cache**: Remove cookies and cached authentication data

#### **Permission Errors**
1. **Check user roles**: Verify role assignment in user management
2. **Review permission matrix**: Use `/Admin/Roles` to verify permissions
3. **Test with known good account**: Use seeded test accounts
4. **Check authorization policies**: Verify policy implementation

### **Performance Issues**

#### **Slow Database Operations**
```powershell
# Check database size
dir scheduler.db

# Run archive cleanup
# Use /Admin/Archive for automated cleanup

# Rebuild database indexes (if needed)
# Use /Admin/Database diagnostics
```

#### **Memory Usage**
- **Monitor log files**: Check `/Admin/Logs` for memory warnings
- **Review archive policies**: Ensure old data is being archived
- **Check concurrent users**: Verify session management is working

### **Feature-Specific Issues**

#### **Scheduler Grid Problems**
1. **Clear browser cache**: Refresh cached JavaScript and CSS
2. **Check machine definitions**: Verify machines exist in `/Admin/Machines`
3. **Validate time zones**: Ensure consistent time zone handling
4. **Review job conflicts**: Check for overlapping jobs or invalid schedules

#### **Quality Management Issues**
1. **Verify checkpoint definitions**: Check `/Admin/Checkpoints` configuration
2. **Review defect categories**: Ensure `/Admin/Defects` are properly configured
3. **Check part assignments**: Verify parts have required quality checkpoints

#### **Archive System Problems**
1. **Check disk space**: Ensure sufficient storage for archive operations
2. **Verify permissions**: Ensure admin access for archive operations
3. **Review retention policies**: Check archive cleanup settings
4. **Monitor archive logs**: Use system logs to diagnose archive issues

### **Logging and Diagnostics**
- **Application Logs**: `logs/opcentrix-*.log` with 30-day retention
- **Admin Log Viewer**: `/Admin/Logs` for web-based log access
- **Database Diagnostics**: `/Admin/Database` for health monitoring
- **System Monitoring**: Built-in performance and health checks

---

## 🚀 **Production Deployment**

### **Development Environment**
```powershell
# Start development server
cd OpCentrix
dotnet run --environment Development

# With database recreation
$env:RECREATE_DATABASE = "true"
dotnet run
```

### **Production Deployment**
```powershell
# Build for production
dotnet publish -c Release -o ./publish

# Configure production settings
# Edit appsettings.Production.json

# Run production build
cd publish
./OpCentrix.exe --environment Production
```

### **Docker Deployment**
```dockerfile
# Production Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY ./publish .

# Set production environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5090

EXPOSE 5090
ENTRYPOINT ["dotnet", "OpCentrix.dll"]
```

### **Production Checklist**
- [ ] Configure secure connection strings
- [ ] Set up proper logging infrastructure
- [ ] Configure backup procedures for SQLite database
- [ ] Set up monitoring and alerting
- [ ] Configure SSL/TLS certificates
- [ ] Review and set production system settings
- [ ] Set up automated archive and cleanup jobs
- [ ] Configure email notifications
- [ ] Test all authentication and authorization
- [ ] Verify performance under load

---

## 📈 **System Health & Monitoring**

### **Built-in Health Checks**
- **Database connectivity**: Automatic database health monitoring
- **Authentication system**: Login and authorization validation
- **File system access**: Log and database file permissions
- **Memory usage**: Application memory consumption tracking
- **Service availability**: All registered services health validation

### **Performance Metrics**
- **Job scheduling efficiency**: Average scheduling time and conflicts
- **Database performance**: Query execution times and optimization
- **User session management**: Active sessions and timeout handling
- **Archive system efficiency**: Archive and cleanup operation performance
- **Quality system metrics**: Inspection completion rates and defect trends

### **Monitoring Endpoints**
```
/health                 # Basic health check
/Admin/Database        # Database diagnostics
/Admin/Logs           # System log monitoring
/Admin/Archive        # Data management statistics
/Admin                # System overview dashboard
```

---

## 📞 **Complete Support Information**

### **System Specifications**
- **Version**: OpCentrix v4.0 - Complete Manufacturing Execution System
- **Framework**: .NET 8 with latest performance optimizations
- **Database**: SQLite with Entity Framework Core 8
- **UI Framework**: Tailwind CSS + Bootstrap with custom components
- **Authentication**: ASP.NET Core Identity with role-based authorization
- **Logging**: Serilog with structured logging and retention management

### **Feature Completion Status**
- ✅ **Core Scheduling**: Complete with multi-machine support
- ✅ **User Management**: Complete with role-based permissions
- ✅ **Machine Management**: Complete with dynamic capabilities
- ✅ **Parts Management**: Complete with admin overrides
- ✅ **Quality Management**: Complete with checkpoints and defects
- ✅ **Data Management**: Complete with archival and cleanup
- ✅ **Multi-Stage Workflow**: Complete with stage management
- ✅ **Master Schedule Analytics**: Complete with real-time reporting
- ✅ **System Administration**: Complete with comprehensive tools
- ✅ **Database Management**: Complete with export/import tools
- ✅ **Monitoring & Logging**: Complete with web-based log viewer

### **System Capabilities**
- **Users**: Unlimited user accounts with role-based access
- **Machines**: Dynamic machine configuration and management
- **Jobs**: Unlimited job scheduling with conflict detection
- **Stages**: Multi-stage workflow with department coordination
- **Quality**: Comprehensive inspection and defect management
- **Archive**: Automated data lifecycle management
- **Analytics**: Real-time production metrics and reporting
- **Export**: Complete data export and backup capabilities

### **Getting Help**
1. **Admin Dashboard**: Start with `/Admin` for system overview
2. **System Logs**: Check `/Admin/Logs` for error messages
3. **Database Health**: Use `/Admin/Database` for diagnostics
4. **User Management**: Verify permissions at `/Admin/Users`
5. **System Settings**: Review configuration at `/Admin/Settings`
6. **Archive Status**: Monitor data management at `/Admin/Archive`

### **Support Resources**
- **Built-in Documentation**: Comprehensive help system within application
- **Admin Tools**: Complete diagnostic and management tools
- **Test Accounts**: Pre-configured accounts for all roles
- **Sample Data**: Production-ready test data for validation
- **Migration Tools**: Database upgrade and migration utilities

---

## 🎯 **Production Ready Features**

### **Enterprise-Grade Capabilities**
- **🔒 Security**: Multi-level authentication with role-based permissions
- **📊 Analytics**: Real-time production metrics and performance tracking
- **🔧 Administration**: Comprehensive system management tools
- **📋 Quality**: Complete quality management and inspection systems
- **💾 Data Management**: Automated archival and cleanup with retention policies
- **🔍 Monitoring**: Built-in health checks and diagnostic tools
- **📈 Reporting**: Advanced analytics with export capabilities
- **🎛️ Configuration**: Runtime settings management without restarts

### **Manufacturing Excellence**
- **Production Planning**: Multi-machine scheduling with capacity optimization
- **Quality Assurance**: Inspection checkpoints and defect tracking
- **Resource Management**: Machine, material, and operator coordination
- **Workflow Control**: Multi-stage process management with approvals
- **Performance Analytics**: Efficiency tracking and improvement insights
- **Compliance**: Audit-ready documentation and traceability

---

**OpCentrix v4.0** - The complete manufacturing execution system for SLS metal printing operations! 🏭✨

*Comprehensive • Scalable • Production-Ready • Future-Proof*