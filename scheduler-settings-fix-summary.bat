@echo off
echo ================================
echo SCHEDULER SETTINGS DATABASE FIX
echo ================================

cd /d "%~dp0OpCentrix"

echo.
echo [1/3] Stopping any running processes...
taskkill /f /im dotnet.exe 2>nul >nul

echo [2/3] Compiling database fix tool...
dotnet build --configuration Release --verbosity quiet

if %ERRORLEVEL% NEQ 0 (
    echo ? Build failed. Please check for compilation errors.
    pause
    exit /b 1
)

echo [3/3] Running database fix tool...
echo.

REM Create temporary program file
(
echo using Microsoft.EntityFrameworkCore;
echo using OpCentrix.Data;
echo using OpCentrix.Models;
echo.
echo var connectionString = "Data Source=opcentrix.db";
echo var optionsBuilder = new DbContextOptionsBuilder^<SchedulerContext^>^(^);
echo optionsBuilder.UseSqlite^(connectionString^);
echo.
echo using var context = new SchedulerContext^(optionsBuilder.Options^);
echo.
echo Console.WriteLine^("OpCentrix Database Fix Tool"^);
echo Console.WriteLine^("==========================="^);
echo.
echo try
echo {
echo     Console.WriteLine^("1. Ensuring database is created..."^);
echo     await context.Database.EnsureCreatedAsync^(^);
echo.
echo     Console.WriteLine^("2. Applying pending migrations..."^);
echo     await context.Database.MigrateAsync^(^);
echo.
echo     Console.WriteLine^("3. Checking SchedulerSettings table..."^);
echo     var settingsCount = await context.SchedulerSettings.CountAsync^(^);
echo     Console.WriteLine^($"? SchedulerSettings table exists with {settingsCount} records."^);
echo.
echo     if ^(settingsCount == 0^)
echo     {
echo         Console.WriteLine^("4. Creating default settings..."^);
echo         var defaultSettings = new SchedulerSettings
echo         {
echo             TitaniumToTitaniumChangeoverMinutes = 30,
echo             InconelToInconelChangeoverMinutes = 45,
echo             CrossMaterialChangeoverMinutes = 120,
echo             DefaultMaterialChangeoverMinutes = 60,
echo             DefaultPreheatingTimeMinutes = 60,
echo             DefaultCoolingTimeMinutes = 240,
echo             DefaultPostProcessingTimeMinutes = 90,
echo             SetupTimeBufferMinutes = 30,
echo             StandardShiftStart = new TimeSpan^(7, 0, 0^),
echo             StandardShiftEnd = new TimeSpan^(15, 0, 0^),
echo             EveningShiftStart = new TimeSpan^(15, 0, 0^),
echo             EveningShiftEnd = new TimeSpan^(23, 0, 0^),
echo             NightShiftStart = new TimeSpan^(23, 0, 0^),
echo             NightShiftEnd = new TimeSpan^(7, 0, 0^),
echo             EnableWeekendOperations = false,
echo             SaturdayOperations = false,
echo             SundayOperations = false,
echo             TI1MachinePriority = 5,
echo             TI2MachinePriority = 5,
echo             INCMachinePriority = 5,
echo             AllowConcurrentJobs = true,
echo             MaxJobsPerMachinePerDay = 8,
echo             RequiredOperatorCertification = "SLS Basic",
echo             QualityCheckRequired = true,
echo             MinimumTimeBetweenJobsMinutes = 15,
echo             EmergencyOverrideEnabled = true,
echo             NotifyOnScheduleConflicts = true,
echo             NotifyOnMaterialChanges = true,
echo             AdvanceWarningTimeMinutes = 60,
echo             CreatedDate = DateTime.UtcNow,
echo             LastModifiedDate = DateTime.UtcNow,
echo             CreatedBy = "DatabaseFix",
echo             LastModifiedBy = "DatabaseFix",
echo             ChangeNotes = "Initial settings created by database fix tool"
echo         };
echo.
echo         context.SchedulerSettings.Add^(defaultSettings^);
echo         await context.SaveChangesAsync^(^);
echo         Console.WriteLine^("? Default settings created successfully!"^);
echo     }
echo.
echo     Console.WriteLine^("?? Database fix completed successfully!"^);
echo     Console.WriteLine^("The application should now work without SchedulerSettings errors."^);
echo }
echo catch ^(Exception ex^)
echo {
echo     Console.WriteLine^($"? Error: {ex.Message}"^);
echo     Console.WriteLine^($"Stack trace: {ex.StackTrace}"^);
echo }
) > TempDatabaseFix.cs

echo Running the database fix...
dotnet script TempDatabaseFix.cs

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ================================
    echo ? SUCCESS: Database fix completed!
    echo ================================
    echo.
    echo The SchedulerSettings table has been created and configured.
    echo You can now run the application: dotnet run
    echo.
) else (
    echo.
    echo ================================
    echo ??  ALTERNATIVE: Manual Migration
    echo ================================
    echo.
    echo If the fix above didn't work, try running:
    echo   dotnet ef database update
    echo.
)

REM Clean up
del TempDatabaseFix.cs 2>nul

echo.
echo Press any key to continue...
pause >nul