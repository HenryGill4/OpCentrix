# OpCentrix SLS Metal Printing Scheduler

**Professional-grade Selective Laser Sintering (SLS) production scheduling system built with .NET 8 and Razor Pages**

---

## ?? Quick Start

### Immediate Setup (5 minutes)
```cmd
# 1. Clone/navigate to workspace
cd "C:\Users\Henry\Source\Repos\OpCentrix"

# 2. Run complete cleanup and implementation
complete-cleanup-and-implementation.bat

# 3. Start application  
start-application.bat

# 4. Open browser
# Navigate to: https://localhost:5001
# Login: admin / admin123
```

### Access Scheduler Settings
- Navigate to: `https://localhost:5001/Admin/SchedulerSettings`
- Configure material changeover times, shift schedules, machine priorities
- All settings take effect immediately

---

## ?? Workspace Organization

```
C:\Users\Henry\Source\Repos\OpCentrix\
??? OpCentrix\                          # ?? MAIN PROJECT (all code here)
?   ??? OpCentrix.csproj               # Project file
?   ??? Program.cs                     # Application entry point
?   ??? Data\SchedulerContext.cs       # Database context
?   ??? Models\                        # Data models
?   ??? Services\                      # Business logic
?   ??? Pages\                         # Razor Pages UI
?   ??? wwwroot\                       # Static files (CSS, JS, images)
??? scripts\                           # ?? ALL AUTOMATION SCRIPTS
?   ??? Windows\                       # .bat files for Windows
?   ??? Linux\                         # .sh files for Linux/macOS
??? docs\                              # ?? ALL DOCUMENTATION
?   ??? SCHEDULER-SETTINGS-*.md        # Scheduler settings docs
?   ??? JQUERY-VALIDATION-*.md         # JavaScript fixes
?   ??? DATABASE-*.md                  # Database guides
??? README.md                          # This file
??? SETUP_COMPLETE.md                  # Detailed setup guide
??? setup-clean-database.bat          # Quick database reset
??? start-application.bat             # Application launcher
??? diagnose-system.bat               # System diagnostic
```

---

## ?? Core Features

### ? Fully Implemented
- **Scheduler Settings Management** - Complete admin interface for all scheduling parameters
- **Material Changeover Control** - Ti-Ti (30min), Inconel-Inconel (45min), Cross-material (120min)  
- **Shift-Based Operations** - Standard/Evening/Night shifts with weekend controls
- **Machine Priority Management** - TI1/TI2/INC machine priorities and job limits
- **jQuery Validation System** - Proper script loading and form validation
- **Database Auto-Recovery** - Automatic table creation and migration handling
- **Production-Ready Structure** - Clean file organization and error handling

### ?? Technical Architecture
- **.NET 8** with **Razor Pages** (not MVC or Blazor)
- **Entity Framework Core** with **SQLite** database
- **Bootstrap 5** responsive UI with **HTMX** for dynamic updates
- **jQuery Validation** for client-side form validation
- **Comprehensive Error Handling** with fallback mechanisms

---

## ??? Development Workflow

### Essential Scripts (Quick Access)
```cmd
# System diagnostic
diagnose-system.bat

# Clean database reset
setup-clean-database.bat

# Start application
start-application.bat
```

### Organized Scripts by Category
```cmd
# Database operations
scripts\Windows\setup-database.bat
scripts\Windows\verify-parts-database.bat
scripts\Windows\apply-scheduler-settings.bat

# Testing and validation
scripts\Windows\test-complete-system.bat
scripts\Windows\verify-final-system.bat
scripts\Windows\quick-test.bat

# jQuery and JavaScript fixes
scripts\Windows\fix-jquery-validation.bat

# Production deployment
scripts\Windows\reset-to-production.bat
```

### Documentation by Topic
```cmd
# Scheduler settings implementation
docs\SCHEDULER-SETTINGS-IMPLEMENTATION-COMPLETE.md

# jQuery validation fixes
docs\JQUERY-VALIDATION-FIX-COMPLETE.md

# Database analysis and troubleshooting
docs\DATABASE-LOGIC-ANALYSIS.md
docs\PARTS-TROUBLESHOOTING-GUIDE.md

# File structure resolution
docs\FILE-STRUCTURE-ISSUE-RESOLVED.md
```

---

## ??? Scheduler Settings Configuration

The scheduler settings system provides complete control over:

### Material Management
- **Titanium-to-Titanium**: 30 minutes (configurable)
- **Inconel-to-Inconel**: 45 minutes (configurable)  
- **Cross-Material Changes**: 120 minutes (configurable)
- **Default Changeover**: 60 minutes (configurable)

### Shift Operations
- **Standard Shift**: 7:00 AM - 3:00 PM
- **Evening Shift**: 3:00 PM - 11:00 PM  
- **Night Shift**: 11:00 PM - 7:00 AM
- **Weekend Operations**: Saturday/Sunday toggles

### Machine Configuration
- **TI1/TI2/INC Priorities**: 1-10 scale (configurable)
- **Max Jobs Per Day**: 8 per machine (configurable)
- **Minimum Time Between Jobs**: 15 minutes (configurable)
- **Concurrent Jobs**: Enabled/disabled (configurable)

### Quality & Safety
- **Operator Certification**: "SLS Basic" requirement (configurable)
- **Quality Checks**: Required/optional (configurable)
- **Emergency Override**: Enabled/disabled (configurable)
- **Advance Warnings**: 60 minutes (configurable)

---

## ?? Troubleshooting

### Common Issues

#### "SchedulerSettings table does not exist"
```cmd
# The system auto-recovers, but you can force it:
scripts\Windows\apply-scheduler-settings.bat
```

#### "jQuery is not defined" 
```cmd
# Fix script loading order:
scripts\Windows\fix-jquery-validation.bat
```

#### Database connection issues
```cmd
# Reset database completely:
setup-clean-database.bat
```

#### File structure problems
```cmd
# Organize and clean workspace:
complete-cleanup-and-implementation.bat
```

### System Diagnostic
```cmd
# Comprehensive system check:
diagnose-system.bat
```

### Getting Help
1. **Check the logs**: Application provides detailed console logging
2. **Review documentation**: All fixes documented in `docs\` directory
3. **Run diagnostics**: Use `diagnose-system.bat` for system status
4. **Check browser console**: F12 for JavaScript validation issues

---

## ??? Development Notes

### Working Directory
**Always work from the project directory:**
```cmd
cd "C:\Users\Henry\Source\Repos\OpCentrix\OpCentrix"
```

### Build and Run
```cmd
# Build project
dotnet build

# Run application
dotnet run

# Run with specific profile
dotnet run --launch-profile https
```

### Database Operations
```cmd
# Create migration
dotnet ef migrations add "MigrationName"

# Apply migrations
dotnet ef database update

# Reset database
dotnet ef database drop --force
dotnet ef database update
```

---

## ?? Production Deployment

### Prerequisites
- Windows Server 2019+ or Windows 10/11
- .NET 8 Runtime
- IIS (optional, can run standalone)

### Deployment Steps
```cmd
# 1. Prepare for production
scripts\Windows\reset-to-production.bat

# 2. Build release version
dotnet publish -c Release -o deploy

# 3. Run final verification
scripts\Windows\verify-final-system.bat
```

### Configuration
- Database: SQLite (included, no SQL Server required)
- Logging: Console + File (configurable in appsettings.json)
- Authentication: Cookie-based with role management
- Performance: Optimized for 50+ concurrent users

---

## ?? Success Metrics

### When Everything is Working
- ? Application starts without errors
- ? Scheduler settings page loads with default values
- ? Material changeover calculations work correctly
- ? jQuery validation works on all forms
- ? Database auto-creates and migrates properly
- ? No JavaScript errors in browser console
- ? All CRUD operations work smoothly

### Performance Indicators
- **Startup Time**: Under 5 seconds
- **Page Load Time**: Under 2 seconds
- **Database Operations**: Under 500ms
- **Memory Usage**: Under 200MB
- **Error Rate**: Zero application errors

---

## ?? License

Internal business application for SLS metal printing operations.

---

## ?? Support

For technical support:
1. Run `diagnose-system.bat` and share results
2. Check `docs\` directory for specific issue guides
3. Review application logs for detailed error information
4. Test with clean database using `setup-clean-database.bat`

**OpCentrix SLS Scheduler** - Professional metal printing production management.

---
*Last Updated: July 2025*  
*Version: 2.0.0*  
*Status: Production Ready*