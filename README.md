# 🎯 OpCentrix - SLS Metal Printing Scheduler

**Version 3.0** - Comprehensive Admin Control System

OpCentrix is a complete production scheduling and management system for SLS (Selective Laser Sintering) metal printing operations, featuring a comprehensive admin control panel, multi-stage workflow, and real-time production tracking.

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

## 🛡️ **Admin Control System**

### **Admin Panel Access**
- **URL**: `http://localhost:5090/Admin`
- **Required Role**: Admin
- **Features**: Complete system administration

### **🎛️ Admin Features Overview**

#### **1. Dashboard (`/Admin`)**
- System overview and statistics
- User counts, machine status, job metrics
- Recent activities and system health
- Quick access to all admin functions

#### **2. User Management (`/Admin/Users`)**
- **Features**: Complete user account management
- **Operations**:
  - ✅ Create user accounts with role assignment
  - ✅ Edit user profiles (username, email, role, department)
  - ✅ Reset passwords securely
  - ✅ Enable/disable user accounts
  - ✅ Delete users (with admin protection)
- **Roles**: Admin, Manager, Scheduler, Operator, Specialists (Printing, Coating, EDM, etc.)
- **Security**: Last admin protection, CSRF protection, audit logging

#### **3. Machine Management (`/Admin/Machines`)**
- **Features**: Dynamic machine configuration
- **Operations**:
  - ✅ Add/remove SLS machines
  - ✅ Configure machine capabilities and specifications
  - ✅ Set build volume, laser power, materials
  - ✅ Manage machine status and availability
  - ✅ OPC UA integration settings
- **Machine Types**: TruPrint series, custom configurations
- **Capabilities**: Dynamic capability assignment per machine

#### **4. Parts Management (`/Admin/Parts`)**
- **Features**: Enhanced part library management
- **Operations**:
  - ✅ Create and edit part definitions
  - ✅ Admin duration overrides with reason tracking
  - ✅ Material assignments and specifications
  - ✅ Quality control checkpoint assignments
- **Scheduler Integration**: Override values used in job scheduling
- **Audit Trail**: Track all part modifications and overrides

#### **5. Role-Based Permissions (`/Admin/Roles`)**
- **Features**: Granular permission management
- **Operations**:
  - ✅ Define feature access per role
  - ✅ Toggle permissions with visual grid interface
  - ✅ Copy permissions between roles
  - ✅ Reset to default permission sets
- **Features Controlled**: Admin panels, scheduling, machine access, reporting

#### **6. System Settings (`/Admin/Settings`)**
- **Features**: Global system configuration
- **Categories**:
  - 🕒 **Scheduler Settings**: Changeover duration (3h), cooldown time (1h)
  - 🏭 **Operations Settings**: Shift times, maintenance intervals
  - 🔔 **Notifications**: Email, SMS, Slack integration
  - 📊 **Quality Settings**: Inspection thresholds, defect tracking
  - 🔧 **System Settings**: Session timeouts, logging levels
- **Runtime Loading**: Settings loaded into application cache on startup

#### **7. Operating Shifts (`/Admin/Shifts`)**
- **Features**: Calendar-style shift management
- **Operations**:
  - ✅ Define working shifts by day of week
  - ✅ Set shift start/end times
  - ✅ Configure shift types (Regular, Overtime, Maintenance)
  - ✅ Activate/deactivate shifts
- **Scheduler Integration**: Job validation against defined shift windows

#### **8. System Logs (`/Admin/Logs`)**
- **Features**: Comprehensive log management
- **Operations**:
  - ✅ View application logs with filtering
  - ✅ Search logs by level, date, content
  - ✅ Download log files
  - ✅ Clear old logs
- **Log Levels**: Debug, Information, Warning, Error, Critical
- **Retention**: 30-day automatic rotation

### **🔐 Security Features**

#### **Authentication System**
- **Session Management**: Configurable timeout (default 2 hours)
- **Role-Based Access**: Granular permissions per feature
- **Password Security**: Secure hashing with salt
- **Session Warnings**: 5-minute logout warning with extension option

#### **Authorization Policies**
```csharp
AdminOnly           // Full admin access
SupervisorAccess    // Admin + Supervisor roles
OperatorAccess      // Admin + Supervisor + Operator roles
SchedulerAccess     // Authenticated users with scheduling rights
```

#### **Protection Mechanisms**
- **CSRF Protection**: Anti-forgery tokens on all forms
- **Admin Protection**: Cannot delete/disable last admin user
- **Input Validation**: Server-side validation for all inputs
- **Audit Logging**: Track all administrative changes

---

## 📋 **User Roles & Permissions**

### **🛡️ Admin**
- **Access**: Full system administration
- **Features**: All admin panels, user management, system settings
- **Restrictions**: Cannot delete own account or last admin

### **👨‍💼 Manager**
- **Access**: Management oversight and reporting
- **Features**: Scheduler, analytics, limited admin functions
- **Restrictions**: Cannot modify system settings or users

### **📅 Scheduler**
- **Access**: Production scheduling operations
- **Features**: Scheduler grid, job management, machine status
- **Restrictions**: Cannot access admin functions

### **👷 Operator**
- **Access**: Operational task execution
- **Features**: Print tracking, job updates, machine status
- **Restrictions**: Read-only on scheduler, no admin access

### **🔧 Specialists**
- **PrintingSpecialist**: SLS printing operations
- **CoatingSpecialist**: Post-processing operations
- **EDMSpecialist**: EDM machining operations
- **QCSpecialist**: Quality control and inspection
- **ShippingSpecialist**: Packaging and shipping
- **MachiningSpecialist**: CNC machining operations
- **MediaSpecialist**: Media handling and preparation
- **Analyst**: Reporting and analytics

---

## 🏗️ **System Architecture**

### **Project Structure**
```
📁 OpCentrix/
├── 📁 Pages/Admin/              # Admin control panel pages
│   ├── 📁 Shared/               # Admin shared components
│   ├── Index.cshtml             # Admin dashboard
│   ├── Users.cshtml             # User management
│   ├── Machines.cshtml          # Machine management
│   ├── Parts.cshtml             # Parts management
│   ├── Roles.cshtml             # Role permissions
│   ├── Settings.cshtml          # System settings
│   ├── Shifts.cshtml            # Operating shifts
│   └── Logs.cshtml              # System logs
├── 📁 Services/Admin/           # Admin business logic
│   ├── AdminDashboardService.cs
│   ├── SystemSettingService.cs
│   ├── RolePermissionService.cs
│   ├── MachineManagementService.cs
│   └── LogViewerService.cs
├── 📁 Models/                   # Database entities
│   ├── User.cs                  # User accounts
│   ├── Machine.cs               # Machine definitions
│   ├── SystemSetting.cs         # Configuration settings
│   ├── RolePermission.cs        # Permission matrix
│   └── OperatingShift.cs        # Shift definitions
└── 📁 Data/                     # Database context
    └── SchedulerContext.cs      # Entity Framework context
```

### **Database Design**
- **SQLite Database**: File-based, no server required
- **Entity Framework Core**: Code-first migrations
- **Automatic Seeding**: Default data populated on startup
- **Migration Commands**:
  ```powershell
  dotnet ef migrations add <MigrationName>
  dotnet ef database update
  ```

### **Services Architecture**
- **Dependency Injection**: All services registered in `Program.cs`
- **Interface Abstractions**: Testable service implementations
- **Async/Await**: Performance-optimized async operations
- **Error Handling**: Comprehensive logging and exception handling

---

## 🔧 **Configuration**

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
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### **System Settings (Database)**
| Category | Setting | Default | Description |
|----------|---------|---------|-------------|
| **Scheduler** | `DefaultChangeoverDurationHours` | 3 | Material changeover time |
| **Scheduler** | `DefaultCooldownTimeHours` | 1 | Machine cooldown time |
| **Operations** | `DefaultShiftStartTime` | 08:00 | Standard shift start |
| **Operations** | `DefaultShiftEndTime` | 17:00 | Standard shift end |
| **System** | `SessionTimeoutMinutes` | 120 | User session timeout |
| **Quality** | `DefaultQualityThreshold` | 95.0 | Quality pass threshold |

### **Environment Variables**
```powershell
# Development
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Database recreation (optional)
$env:RECREATE_DATABASE = "true"

# HTTPS redirection (optional)
$env:UseHttpsRedirection = "false"
```

---

## 🧪 **Testing**

### **Test Commands**
```powershell
# Run all tests
dotnet test

# Run specific test project
dotnet test OpCentrix.Tests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run integration tests
dotnet test --filter "Category=Integration"
```

### **Test Users Available**
```
admin/admin123          (Admin)
manager/manager123      (Manager)
scheduler/scheduler123  (Scheduler)
operator/operator123    (Operator)
printer/printer123      (PrintingSpecialist)
coating/coating123      (CoatingSpecialist)
edm/edm123             (EDMSpecialist)
qc/qc123               (QCSpecialist)
shipping/shipping123    (ShippingSpecialist)
machining/machining123  (MachiningSpecialist)
media/media123         (MediaSpecialist)
analyst/analyst123      (Analyst)
```

---

## 📊 **Production Features**

### **Scheduler Grid**
- **Multi-machine View**: TI1, TI2, INC machines
- **Drag & Drop**: Interactive job scheduling
- **Color Coding**: Status-based visual indicators
- **Zoom Levels**: 1 hour to 12 hour views
- **Two-month Span**: Extended planning horizon

### **Print Tracking**
- **Real-time Monitoring**: Live job status updates
- **Delay Tracking**: Start time variance monitoring
- **Resource Usage**: Gas, powder consumption tracking
- **Operator Accountability**: User-attributed job tracking

### **Multi-stage Workflow**
- **Printing**: SLS metal printing operations
- **EDM**: Electrical discharge machining
- **Coating**: Cerakote post-processing
- **QC**: Quality control inspection
- **Shipping**: Packaging and fulfillment

---

## 🔍 **Troubleshooting**

### **Common Issues**

#### **Port Already in Use**
```powershell
# Check what's using port 5090
netstat -ano | findstr :5090

# Kill the process (replace <PID> with actual process ID)
taskkill /PID <PID> /F
```

#### **Database Issues**
```powershell
# Remove database and recreate
Remove-Item -Force scheduler.db*
dotnet run  # Database will be recreated
```

#### **Admin Login Not Working**
1. Verify admin user exists: Check `Users` table
2. Check authentication service registration in `Program.cs`
3. Verify `AdminOnly` policy configuration
4. Check browser console for JavaScript errors

#### **Missing Dependencies**
```powershell
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

### **Logging**
- **File Logs**: `logs/opcentrix-*.log` (30-day retention)
- **Console Logs**: Real-time development output
- **Admin Log Viewer**: `/Admin/Logs` for web-based log access

---

## 🚀 **Deployment**

### **Development**
```powershell
cd OpCentrix
dotnet run
```

### **Production**
```powershell
# Publish for production
dotnet publish -c Release -o ./publish

# Run published application
cd publish
./OpCentrix.exe
```

### **Docker (Optional)**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY ./publish .
EXPOSE 5090
ENTRYPOINT ["dotnet", "OpCentrix.dll"]
```

---

## 📞 **Support**

### **System Information**
- **Version**: OpCentrix v3.0
- **Framework**: .NET 8
- **Database**: SQLite
- **UI Framework**: Tailwind CSS + Bootstrap
- **Authentication**: Cookie-based ASP.NET Core Identity

### **Feature Status**
- ✅ **User Management**: Complete
- ✅ **Machine Management**: Complete  
- ✅ **Parts Management**: Complete with admin overrides
- ✅ **Role Permissions**: Complete
- ✅ **System Settings**: Complete
- ✅ **Operating Shifts**: Complete
- ✅ **System Logs**: Complete
- ✅ **Production Scheduling**: Complete
- ✅ **Print Tracking**: Complete

### **Getting Help**
1. Check the admin logs at `/Admin/Logs`
2. Review console output for errors
3. Verify user permissions and roles
4. Check system settings configuration

---

**OpCentrix Admin Control System** - Comprehensive production management for SLS metal printing operations! 🎯