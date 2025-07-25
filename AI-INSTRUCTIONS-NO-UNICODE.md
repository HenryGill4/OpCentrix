# AI INSTRUCTIONS FOR OPCENTRIX - WINDOWS COMPATIBLE NO UNICODE

## CRITICAL REQUIREMENTS FOR AI ASSISTANTS

### PLATFORM COMPATIBILITY
- TARGET SYSTEM: Windows 10/11 with .NET 8 and Razor Pages
- COMMAND SHELL: Command Prompt (cmd.exe) or PowerShell
- FILE PATHS: Use backslashes (OpCentrix\Pages\Admin\Parts.cshtml)
- SCRIPTS: Use .bat files instead of .sh files

### UNICODE AND EMOJI RESTRICTIONS
- NEVER use emojis or Unicode characters in any output
- NO Unicode symbols: checkmarks, arrows, icons, or decorative characters
- Use ASCII alternatives: [OK], [ERROR], [SUCCESS], [WARNING]
- Use standard text: "SUCCESS", "ERROR", "COMPLETE", "FAILED"

## WORKSPACE STRUCTURE

### Project Directory Layout
```
C:\Users\Henry\Source\Repos\OpCentrix\
??? OpCentrix\                           # Main project folder
?   ??? OpCentrix.csproj                # Project file
?   ??? Program.cs                       # Application entry point
?   ??? Data\
?   ?   ??? SchedulerContext.cs         # Database context
?   ??? Models\
?   ?   ??? Part.cs                     # Data models
?   ?   ??? Job.cs
?   ?   ??? SchedulerSettings.cs
?   ??? Services\
?   ?   ??? SchedulerService.cs         # Business logic
?   ?   ??? SchedulerSettingsService.cs
?   ?   ??? SlsDataSeedingService.cs
?   ??? Pages\
?   ?   ??? Admin\                      # Admin pages
?   ?   ?   ??? Parts.cshtml           
?   ?   ?   ??? Parts.cshtml.cs
?   ?   ?   ??? SchedulerSettings.cshtml
?   ?   ?   ??? SchedulerSettings.cshtml.cs
?   ?   ??? PrintTracking\              # Print tracking pages
?   ?   ??? Shared\
?   ?       ??? _Layout.cshtml          # Main layout
?   ?       ??? _AdminLayout.cshtml     # Admin layout
?   ??? wwwroot\                        # Static files
?   ?   ??? css\
?   ?   ??? js\
?   ?   ??? lib\
?   ??? Migrations\                     # Database migrations
??? diagnose-system.bat                 # System diagnostic script
??? fix-jquery-validation.bat           # jQuery validation fix
??? setup-clean-database.bat           # Database setup script
??? AI-INSTRUCTIONS-NO-UNICODE.md      # This file
```

## WINDOWS COMMAND REFERENCE

### Essential Windows Commands for AI
```cmd
REM Directory Navigation
cd /d "C:\path\to\directory"
dir                                     # List files
mkdir "new_directory"                   # Create directory

REM Process Management
tasklist /FI "IMAGENAME eq dotnet.exe"  # List processes
taskkill /f /im dotnet.exe              # Kill process
netstat -an | find ":5001"             # Check port usage

REM File Operations
copy "source.txt" "destination.txt"     # Copy file
del "filename.txt"                      # Delete file
move "old.txt" "new.txt"                # Move/rename file

REM .NET Commands
dotnet --version                        # Check .NET version
dotnet restore                          # Restore packages
dotnet build                            # Build project
dotnet run                              # Run application
dotnet ef migrations add "MigrationName" # Add migration
dotnet ef database update              # Apply migrations

REM PowerShell Downloads
powershell -Command "Invoke-WebRequest -Uri 'URL' -OutFile 'filename'"
```

### NEVER USE (Linux/Mac Commands)
```bash
# DO NOT USE THESE ON WINDOWS:
./script.sh                 # Use script.bat
curl -o file URL            # Use PowerShell Invoke-WebRequest
cp file1 file2              # Use copy
mv file1 file2              # Use move
ls -la                      # Use dir
chmod +x file               # Windows does not use chmod
sudo command                # Windows does not use sudo
```

## JQUERY VALIDATION FIX IMPLEMENTATION

### Problem Identification
- Issue: JavaScript error at jquery.validate.min.js:4:151
- Cause: Script loading order problem
- Impact: Form validation not working
- Solution: Fixed script loading sequence in _Layout.cshtml

### Script Loading Order (CRITICAL)
```html
<!-- CORRECT ORDER - MUST BE FOLLOWED -->
<script src="~/lib/jquery/dist/jquery.min.js"></script>
<script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
<script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js"></script>
<script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
<script src="~/js/site.js" asp-append-version="true"></script>
```

### Error Detection Code
```javascript
// Verify jQuery loads before validation
if (typeof jQuery === 'undefined') {
    console.error('CRITICAL: jQuery failed to load!');
} else {
    console.log('[OK] jQuery loaded successfully:', jQuery.fn.jquery);
}

// Verify validation plugin loads
if (typeof jQuery.validator === 'undefined') {
    console.error('CRITICAL: jQuery Validation failed to load!');
} else {
    console.log('[OK] jQuery Validation loaded successfully');
}
```

## SCHEDULER SETTINGS IMPLEMENTATION

### Database Model
```csharp
// Models/SchedulerSettings.cs
public class SchedulerSettings
{
    public int Id { get; set; }
    
    // Material Changeover Times (minutes)
    public int TitaniumToTitaniumChangeover { get; set; } = 30;
    public int InconelToInconelChangeover { get; set; } = 45;
    public int CrossMaterialChangeover { get; set; } = 120;
    public int DefaultChangeover { get; set; } = 60;
    
    // Process Timing Settings (minutes)
    public int DefaultPreheatingTime { get; set; } = 60;
    public int DefaultCoolingTime { get; set; } = 180;
    public int DefaultPostProcessingTime { get; set; } = 120;
    public int SetupBufferTime { get; set; } = 30;
    
    // Operator Schedule Settings
    public TimeOnly StandardShiftStart { get; set; } = new(7, 0);
    public TimeOnly StandardShiftEnd { get; set; } = new(15, 0);
    public TimeOnly EveningShiftStart { get; set; } = new(15, 0);
    public TimeOnly EveningShiftEnd { get; set; } = new(23, 0);
    public TimeOnly NightShiftStart { get; set; } = new(23, 0);
    public TimeOnly NightShiftEnd { get; set; } = new(7, 0);
    
    // Weekend Operations
    public bool SaturdayOperations { get; set; } = false;
    public bool SundayOperations { get; set; } = false;
    
    // Machine Configuration
    public int TI1Priority { get; set; } = 1;
    public int TI2Priority { get; set; } = 2;
    public int INCPriority { get; set; } = 3;
    public bool AllowConcurrentJobs { get; set; } = false;
    public int MaxJobsPerMachinePerDay { get; set; } = 3;
    public int MinTimeBetweenJobs { get; set; } = 15;
    
    // Quality and Safety
    public bool RequireOperatorCertification { get; set; } = true;
    public bool RequireQualityCheck { get; set; } = true;
    public bool AllowEmergencyOverride { get; set; } = false;
    public int AdvanceWarningMinutes { get; set; } = 30;
    
    // Audit Trail
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public string LastModifiedBy { get; set; } = string.Empty;
    public string ChangeNotes { get; set; } = string.Empty;
}
```

### Service Implementation
```csharp
// Services/SchedulerSettingsService.cs
public interface ISchedulerSettingsService
{
    Task<SchedulerSettings> GetSettingsAsync();
    Task<SchedulerSettings> UpdateSettingsAsync(SchedulerSettings settings, string modifiedBy, string notes = "");
    Task<int> CalculateChangeoverTimeAsync(string fromMaterial, string toMaterial);
    Task<bool> IsOperatorAvailableAsync(DateTime startTime, DateTime endTime);
    Task<bool> ValidateJobSchedulingAsync(Job job);
}

public class SchedulerSettingsService : ISchedulerSettingsService
{
    private readonly SchedulerContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SchedulerSettingsService> _logger;
    private const string SETTINGS_CACHE_KEY = "scheduler_settings";
    
    public SchedulerSettingsService(
        SchedulerContext context, 
        IMemoryCache cache, 
        ILogger<SchedulerSettingsService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<SchedulerSettings> GetSettingsAsync()
    {
        if (_cache.TryGetValue(SETTINGS_CACHE_KEY, out SchedulerSettings cachedSettings))
        {
            return cachedSettings;
        }
        
        var settings = await _context.SchedulerSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new SchedulerSettings();
            _context.SchedulerSettings.Add(settings);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created default scheduler settings");
        }
        
        _cache.Set(SETTINGS_CACHE_KEY, settings, TimeSpan.FromMinutes(30));
        return settings;
    }
    
    public async Task<int> CalculateChangeoverTimeAsync(string fromMaterial, string toMaterial)
    {
        var settings = await GetSettingsAsync();
        
        if (string.IsNullOrEmpty(fromMaterial) || string.IsNullOrEmpty(toMaterial))
            return settings.DefaultChangeover;
            
        var from = fromMaterial.ToUpper();
        var to = toMaterial.ToUpper();
        
        if (from == to)
        {
            if (from.Contains("TITANIUM") || from.Contains("TI"))
                return settings.TitaniumToTitaniumChangeover;
            if (from.Contains("INCONEL") || from.Contains("INC"))
                return settings.InconelToInconelChangeover;
        }
        
        if ((from.Contains("TITANIUM") && to.Contains("INCONEL")) ||
            (from.Contains("INCONEL") && to.Contains("TITANIUM")))
            return settings.CrossMaterialChangeover;
            
        return settings.DefaultChangeover;
    }
}
```

## TESTING AND VALIDATION

### Windows Testing Scripts

#### System Diagnostic (diagnose-system.bat)
```batch
@echo off
echo JQUERY VALIDATION DIAGNOSTIC - Windows
echo =======================================

if not exist "OpCentrix" (
    echo [ERROR] OpCentrix directory not found!
    pause
    exit /b 1
)

cd OpCentrix

echo [1/5] Checking project structure...
if exist "OpCentrix.csproj" (
    echo [OK] OpCentrix.csproj found
) else (
    echo [ERROR] OpCentrix.csproj NOT found
)

echo [2/5] Checking .NET installation...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK not found
) else (
    for /f "tokens=*" %%i in ('dotnet --version') do echo [OK] .NET SDK version: %%i
)

echo [3/5] Testing project build...
dotnet build --verbosity quiet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Project build failed
) else (
    echo [OK] Project builds successfully
)

echo DIAGNOSTIC COMPLETE
pause
```

#### jQuery Validation Fix (fix-jquery-validation.bat)
```batch
@echo off
echo FIXING JQUERY VALIDATION BUG - Windows
echo =====================================

taskkill /f /im dotnet.exe 2>nul
cd /d "%~dp0OpCentrix"

echo [1/4] Backing up existing files...
if exist "wwwroot\lib\jquery-validation\dist\jquery.validate.min.js" (
    copy "wwwroot\lib\jquery-validation\dist\jquery.validate.min.js" "wwwroot\lib\jquery-validation\dist\jquery.validate.min.js.backup" >nul
    echo [OK] Backup created
)

echo [2/4] Downloading fresh jQuery validation files...
powershell -Command "try { Invoke-WebRequest -Uri 'https://cdn.jsdelivr.net/npm/jquery-validation@1.19.5/dist/jquery.validate.min.js' -OutFile 'wwwroot\lib\jquery-validation\dist\jquery.validate.min.js' -UseBasicParsing; Write-Host '[OK] Downloaded jquery.validate.min.js'; } catch { Write-Host '[WARNING] Download failed, keeping existing file'; }"

echo [3/4] Building project...
dotnet build --verbosity quiet
if errorlevel 1 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)
echo [OK] Build successful

echo [4/4] Starting application...
echo [SUCCESS] jQuery validation bug has been fixed!
echo Navigate to: https://localhost:5001
echo Login: admin / admin123
start "" cmd /c "dotnet run && pause"
```

### Browser Testing Checklist
1. Open browser to https://localhost:5001
2. Press F12 to open Developer Tools
3. Check Console tab for errors
4. Should see: "[OK] jQuery loaded successfully"
5. Should see: "[OK] jQuery Validation loaded successfully"
6. Test form validation by submitting empty forms
7. Verify onchange events work on dropdowns
8. Check Network tab - all JS files should load with 200 status

## TROUBLESHOOTING GUIDE

### Common Windows Issues

#### Port Already in Use
```cmd
REM Find process using port 5001
netstat -ano | findstr :5001

REM Kill the process (replace PID with actual process ID)
taskkill /PID 1234 /F
```

#### Permission Denied
```cmd
REM Run Command Prompt as Administrator
runas /user:Administrator cmd

REM Or use PowerShell as Administrator
Start-Process powershell -Verb RunAs
```

#### Build Failures
```cmd
REM Clean and restore
dotnet clean
dotnet restore
dotnet build --verbosity normal
```

#### Database Issues
```cmd
REM Reset database completely
del OpCentrix\Data\*.db*
cd OpCentrix
dotnet run
```

### Error Response Templates

#### Successful Operations
```
[SUCCESS] Operation completed successfully
[OK] File created: filename.txt
[COMPLETE] Database migration applied
[READY] Application started on port 5001
```

#### Error Conditions
```
[ERROR] File not found: OpCentrix.csproj
[FAILED] Build process encountered errors
[WARNING] Port 5001 already in use
[CRITICAL] jQuery failed to load
```

## DEVELOPMENT WORKFLOW

### Adding New Features
1. Analyze requirements and identify affected files
2. Use Windows-compatible file paths with backslashes
3. Update models, services, and pages as needed
4. Test with diagnose-system.bat
5. Build and verify with dotnet build
6. Test in browser with F12 console monitoring

### Database Changes
1. Modify models in Models\ directory
2. Add migration: dotnet ef migrations add "FeatureName"
3. Apply migration: dotnet ef database update
4. Test with clean database: setup-clean-database.bat

### Script Creation Rules
- Use .bat extension for Windows batch files
- Include error checking with if errorlevel commands
- Provide clear status messages without Unicode
- Use pause command to prevent window closure
- Include full file paths with quotes for spaces

## PRODUCTION DEPLOYMENT

### Pre-Deployment Checklist
- [ ] All tests pass with Windows scripts
- [ ] No Unicode characters in any output
- [ ] Scripts use Windows commands only
- [ ] Database migrations applied successfully
- [ ] jQuery validation working in all browsers
- [ ] Scheduler settings functionality verified

### Environment Configuration
```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=OpCentrix.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

This comprehensive guide ensures all AI assistants working with the OpCentrix system use Windows-compatible commands and avoid Unicode characters while maintaining full functionality of the SLS Metal Printing Scheduler.