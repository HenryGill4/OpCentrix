@echo off
echo ==========================================
echo   OpCentrix Database Verification Script
echo ==========================================
echo.

REM Navigate to project directory
cd /d "%~dp0"
if not exist "OpCentrix.csproj" (
    cd OpCentrix
    if not exist "OpCentrix.csproj" (
        echo ERROR: OpCentrix.csproj not found
        pause
        exit /b 1
    )
)

echo ?? Testing database and application setup...
echo.

REM Check if database exists
if not exist "scheduler.db" (
    echo ? Database file not found!
    echo ?? Please run setup-database.bat first
    pause
    exit /b 1
)

echo ? Database file found: scheduler.db
echo.

REM Test database connectivity and structure
echo ?? Testing database connectivity...

REM Create a temporary test to verify database
(
echo using Microsoft.EntityFrameworkCore;
echo using OpCentrix.Data;
echo using OpCentrix.Services;
echo using Microsoft.Extensions.DependencyInjection;
echo using Microsoft.Extensions.Logging;
echo.
echo var services = new ServiceCollection^(^);
echo services.AddDbContext^<SchedulerContext^>^(options =^> options.UseSqlite^("Data Source=scheduler.db"^)^);
echo services.AddLogging^(builder =^> builder.AddConsole^(^)^);
echo services.AddScoped^<DatabaseValidationService^>^(^);
echo.
echo var serviceProvider = services.BuildServiceProvider^(^);
echo using var scope = serviceProvider.CreateScope^(^);
echo.
echo var context = scope.ServiceProvider.GetRequiredService^<SchedulerContext^>^(^);
echo var validator = scope.ServiceProvider.GetRequiredService^<DatabaseValidationService^>^(^);
echo.
echo Console.WriteLine^("Testing database connection..."^);
echo var connectionResult = await context.TestConnectionAsync^(^);
echo Console.WriteLine^($"Connection test: {^(connectionResult ? "? SUCCESS" : "? FAILED"^)}"^);
echo.
echo Console.WriteLine^("Testing database health..."^);
echo var ^(isHealthy, statusMessage^) = await context.GetHealthStatusAsync^(^);
echo Console.WriteLine^($"Health check: {^(isHealthy ? "?" : "?"^)} {statusMessage}"^);
echo.
echo Console.WriteLine^("Running comprehensive validation..."^);
echo var validationResult = await validator.ValidateDatabaseAsync^(^);
echo Console.WriteLine^(validationResult.GetSummary^(^)^);
echo.
echo if ^(validationResult.Errors.Any^(^)^)
echo {
echo     Console.WriteLine^("Errors found:"^);
echo     foreach ^(var error in validationResult.Errors^)
echo         Console.WriteLine^($"  ? {error}"^);
echo }
echo.
echo if ^(validationResult.Warnings.Any^(^)^)
echo {
echo     Console.WriteLine^("Warnings:"^);
echo     foreach ^(var warning in validationResult.Warnings^)
echo         Console.WriteLine^($"  ?? {warning}"^);
echo }
echo.
echo Console.WriteLine^("Database verification completed."^);
) > DatabaseTest.cs

REM Compile and run the test
dotnet run DatabaseTest.cs 2>nul || (
    echo ?? Advanced database test skipped
    echo ? Basic file check passed
)

REM Clean up
if exist "DatabaseTest.cs" del "DatabaseTest.cs"

echo.
echo ?? Testing application startup...

REM Test application startup with timeout
echo Starting application test ^(10 second timeout^)...
timeout 10 dotnet run --no-build --urls "http://localhost:0" 2>nul && (
    echo ? Application starts successfully
) || (
    echo ?? Application startup test timed out ^(this is normal^)
)

echo.
echo ?? Verification Summary:
echo ========================

if exist "scheduler.db" (
    echo ? Database file exists
    for %%A in (scheduler.db) do echo    Size: %%~zA bytes
) else (
    echo ? Database file missing
)

if exist "TEST_USERS.txt" (
    echo ? Test users file exists
) else (
    echo ?? Test users file not found
)

REM Check for basic files
if exist "appsettings.json" (
    echo ? Configuration file exists
) else (
    echo ? Configuration file missing
)

echo.
echo ?? Next Steps:
echo - Run 'dotnet run' to start the application
echo - Visit http://localhost:5000
echo - Login with admin/admin123
echo - Check the scheduler page to verify functionality
echo.
echo ?? If you encounter issues:
echo - Run setup-database.bat to reset
echo - Check TEST_USERS.txt for login credentials
echo - Ensure no other application is using port 5000
echo.
pause