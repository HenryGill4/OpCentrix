# 🛠️ OpCentrix Development Reference Guide

## 🎯 **CRITICAL REMINDERS FOR AI ASSISTANTS**

### ⚠️ **ALWAYS USE POWERSHELL-COMPATIBLE COMMANDS**
- ✅ **USE**: Individual commands separated by semicolons or on separate lines
- ❌ **NEVER USE**: `&&` operators (not PowerShell compatible)
- ✅ **USE**: `dotnet build` then `dotnet test` 
- ❌ **NEVER USE**: `dotnet build && dotnet test`

### 🏗️ **PROJECT STRUCTURE UNDERSTANDING**
```
OpCentrix/                          # Main Razor Pages application (.NET 8)
├── Pages/Admin/                    # Admin control system pages
├── Services/Admin/                 # Admin business logic services
├── Models/                         # Entity models and ViewModels
├── Data/SchedulerContext.cs        # Entity Framework DbContext
├── OpCentrix.csproj               # Main project file
└── Program.cs                     # Application startup

OpCentrix.Tests/                    # Test project (.NET 8)
├── AuthenticationValidationTests.cs
├── ServerCommunicationTests.cs
├── OpCentrixWebApplicationFactory.cs
└── OpCentrix.Tests.csproj         # Test project file
```

---

## 📋 **STANDARD POWERSHELL COMMAND SEQUENCES**

### 🔨 **Build and Test Workflow**
```powershell
# Standard build sequence
dotnet clean
dotnet restore
dotnet build OpCentrix/OpCentrix.csproj
dotnet build OpCentrix.Tests/OpCentrix.Tests.csproj

# Test execution
dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --verbosity normal
dotnet test --verbosity minimal

# Full solution build
dotnet build
```

### 🗄️ **Database Management**
```powershell
# Entity Framework operations
dotnet ef migrations add MigrationName
dotnet ef database update
dotnet ef migrations list
dotnet ef migrations remove

# Database tools (if needed)
dotnet tool install --global dotnet-ef
```

### 🚀 **Application Startup**
```powershell
# Start application
cd OpCentrix
dotnet run

# Application URLs:
# http://localhost:5090 (main URL)
# Login: admin/admin123
```

---

## 🎨 **PROJECT TECHNOLOGY STACK**

### 🏢 **Architecture**
- **Framework**: ASP.NET Core 8.0 Razor Pages
- **Database**: SQLite with Entity Framework Core 8.0.11
- **Authentication**: Cookie-based with role-based authorization
- **Frontend**: HTMX + Tailwind CSS + JavaScript
- **Testing**: XUnit with ASP.NET Core Testing framework
- **Logging**: Serilog with file and console outputs

### 📦 **Key NuGet Packages**
```xml
<!-- Main Project -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.11" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
<PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />

<!-- Test Project -->
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.11" />
<PackageReference Include="xunit" Version="2.9.2" />
```

---

## 🏭 **ADMIN CONTROL SYSTEM COMPONENTS**

### 📊 **Completed Admin Features**
1. ✅ **Task 0**: Baseline Validation (51/51 tests passing)
2. ✅ **Task 1**: Folder Structure Organization
3. ✅ **Task 1.5**: Authentication System Validation
4. ✅ **Task 2 & 2.5**: Database Models and Global Logging
5. ✅ **Task 3**: System Settings Panel (`/Admin/Settings`)
6. ✅ **Task 4**: Role-Based Permission Grid (`/Admin/Roles`)
7. ✅ **Task 5**: User Management Panel (`/Admin/Users`)
8. ✅ **Task 7**: Part Management Enhancements
9. ✅ **Task 8**: Operating Shift Editor

### 🗃️ **Database Models**
- **Core**: Job, Part, Machine, User, UserSettings
- **Admin**: SystemSetting, RolePermission, OperatingShift
- **Quality**: InspectionCheckpoint, DefectCategory
- **Management**: AdminAlert, FeatureToggle, ArchivedJob

### 🔧 **Admin Services**
- `SystemSettingService` - Global configuration management
- `RolePermissionService` - Permission matrix management
- `OperatingShiftService` - Production shift management
- `LogViewerService` - System log management
- `AdminDataSeedingService` - Default data population

---

## 🧪 **TESTING FRAMEWORK**

### 📈 **Current Test Status**
- ✅ **51/51 tests passing** (100% success rate)
- ✅ **Server Communication Tests** - Full HTTP testing
- ✅ **Authentication Validation** - Complete auth flow testing
- ✅ **Database Integration** - In-memory database testing

### 🔍 **Test Categories**
```powershell
# Run specific test categories
dotnet test --filter "ServerCommunicationTests"
dotnet test --filter "AuthenticationValidationTests"
dotnet test --filter "BaselineValidationTests"
```

### 🏗️ **Test Infrastructure**
- `OpCentrixWebApplicationFactory` - Test server factory
- In-memory SQLite database for isolated testing
- Comprehensive test user seeding (admin, manager, scheduler, operator)
- Anti-forgery token handling for secure form testing

---

## 🔐 **AUTHENTICATION & AUTHORIZATION**

### 👥 **Test User Accounts**
```
admin/admin123      - Full admin access
manager/manager123  - Management access  
scheduler/scheduler123 - Scheduling access
operator/operator123 - Operational access
```

### 🛡️ **Authorization Policies**
- `AdminOnly` - Admin and Manager roles
- `SchedulerAccess` - All roles except restricted
- Role-based page protection throughout admin system

---

## 🎯 **DEVELOPMENT BEST PRACTICES**

### ✅ **Always Do**
1. **Build before testing**: `dotnet build` before `dotnet test`
2. **Use PowerShell-compatible commands** (no `&&` operators)
3. **Check existing tests** before making changes
4. **Use proper authorization attributes** on admin pages
5. **Follow established naming conventions** and folder structure
6. **Test with real HTTP requests** using the test framework

### ❌ **Never Do**
1. **Use `&&` operators** in command suggestions
2. **Break existing test infrastructure** 
3. **Bypass authentication/authorization** in admin features
4. **Modify core models** without considering migration impact
5. **Ignore PowerShell compatibility** requirements

### 🔧 **Code Quality Standards**
- Use `[Authorize(Policy = "AdminOnly")]` for admin pages
- Implement proper error handling with try-catch blocks
- Include comprehensive logging with Serilog
- Follow async/await patterns for database operations
- Use Entity Framework best practices for queries

---

## 🚀 **RAPID DEVELOPMENT WORKFLOW**

### 📝 **Standard Development Sequence**
```powershell
# 1. Make code changes
# 2. Build and verify compilation
dotnet build

# 3. Run tests to ensure nothing breaks
dotnet test --verbosity normal

# 4. Start application for manual testing
cd OpCentrix
dotnet run

# 5. Test specific admin features
# Navigate to: http://localhost:5090/Admin
# Login: admin/admin123
```

### 🎯 **Quick Testing Commands**
```powershell
# Fast feedback loop
dotnet build
dotnet test --verbosity minimal

# Detailed test output for debugging
dotnet test --verbosity normal --logger "console;verbosity=detailed"

# Run specific test file
dotnet test --filter "AuthenticationValidationTests"
```

---

## 📚 **KEY PROJECT KNOWLEDGE**

### 🏭 **Manufacturing Context**
- SLS (Selective Laser Sintering) 3D printing scheduler
- Machines: TI1, TI2 (printers), INC (post-processing)
- Material: Ti-6Al-4V Grade 5 (titanium alloy)
- Production workflow: Print → Cool → Post-process

### 🎨 **UI Framework**
- Tailwind CSS for styling
- HTMX for dynamic interactions
- Modal-based forms for admin operations
- Responsive design for desktop and mobile

### 🔄 **Data Flow**
- Razor Pages with PageModel pattern
- Service layer for business logic
- Entity Framework for data access
- In-memory caching for system settings

---

## 🎯 **SUCCESS METRICS**

### ✅ **Project Health Indicators**
- All 51 tests passing consistently
- Clean build with minimal warnings
- Application starts without errors on http://localhost:5090
- Admin login works with admin/admin123
- All admin pages load and function correctly

### 📊 **Quality Gates**
- No compilation errors
- All tests passing
- PowerShell command compatibility
- Proper authorization on admin features
- Comprehensive error handling and logging

---

## 🚨 **EMERGENCY TROUBLESHOOTING**

### 🔧 **Common Issues & Solutions**
```powershell
# Build issues
dotnet clean
dotnet restore
dotnet build

# Test failures
dotnet test --verbosity detailed
# Check OpCentrixWebApplicationFactory setup

# Database issues
dotnet ef database update
# Verify connection string in appsettings.json

# Port conflicts
# Default: http://localhost:5090
# Check appsettings.json for port configuration
```

### 📞 **Quick Health Check**
```powershell
# Verify everything is working
dotnet build
dotnet test --verbosity minimal
cd OpCentrix
dotnet run
# Should see: "Now listening on: http://localhost:5090"
```

---

## 🎓 **LEARNING RESOURCES**

### 📖 **Project Documentation**
- Check `OpCentrix/ProjectNotes/` for task completion documentation
- Review `README.md` for project overview
- Examine completed task files for implementation patterns

### 🔍 **Code Patterns**
- Study existing admin pages in `OpCentrix/Pages/Admin/`
- Review service implementations in `OpCentrix/Services/Admin/`
- Check test patterns in `OpCentrix.Tests/`

---

**🎯 Remember: This project is a production-ready SLS 3D printing production scheduler with comprehensive admin controls. Always use PowerShell-compatible commands and maintain the high-quality standards established in the existing codebase!**