# OpCentrix Project Structure

## Project Organization

```
OpCentrix\
├── OpCentrix.csproj              # Main project file
├── Program.cs                    # Application entry point
├── Data\                         # Database context and configuration
│   └── SchedulerContext.cs
├── Models\                       # Data models and view models
│   ├── Job.cs
│   ├── Part.cs
│   ├── SchedulerSettings.cs
│   └── ViewModels\
├── Services\                     # Business logic services
│   ├── SchedulerService.cs
│   ├── SchedulerSettingsService.cs
│   └── SlsDataSeedingService.cs
├── Pages\                        # Razor Pages
│   ├── Scheduler\
│   ├── Admin\
│   ├── PrintTracking\
│   └── Shared\
├── wwwroot\                      # Static web assets
│   ├── css\
│   ├── js\
│   └── lib\
├── Migrations\                   # Entity Framework migrations
├── Documentation\               # Project documentation
│   ├── SCHEDULER-FIX-CHECKLIST.md
│   ├── WEEKEND-OPERATIONS-FIX-COMPLETE.md
│   └── [Other implementation docs]
└── Scripts\                      # Utility scripts
    ├── diagnose-system.bat
    ├── setup-clean-database.bat
    └── [Other utility scripts]
```

## Important Files

### Core Implementation Files
- **SCHEDULER-FIX-CHECKLIST.md**: Comprehensive checklist for fixing scheduler issues
- **WEEKEND-OPERATIONS-FIX-COMPLETE.md**: Weekend operations implementation
- **DATABASE-LOGIC-FIXES-COMPLETE.md**: Database fixes and optimizations

### Setup and Maintenance
- **Scripts\setup-clean-database.bat**: Database initialization
- **Scripts\diagnose-system.bat**: System health check
- **Scripts\start-application.bat**: Application launcher

Generated: Fri 07/25/2025  3:54:28.91
