@echo off
echo.
echo ===================================================================
echo  CREATING SCHEDULER SETTINGS MIGRATION - Windows
echo ===================================================================
echo.

set "WORKSPACE_PATH=C:\Users\Henry\Source\Repos\OpCentrix"
set "PROJECT_PATH=%WORKSPACE_PATH%\OpCentrix"

echo Workspace: %WORKSPACE_PATH%
echo Project: %PROJECT_PATH%
echo.

REM Stop any running application
echo [1/6] Stopping any running OpCentrix application...
taskkill /f /im dotnet.exe 2>nul || echo No running dotnet processes found.

REM Navigate to project directory
echo [2/6] Navigating to project directory...
cd /d "%PROJECT_PATH%"

echo [3/6] Creating SchedulerSettings migration...
dotnet ef migrations add AddSchedulerSettingsTable --verbose
if errorlevel 1 (
    echo [ERROR] Failed to create migration
    pause
    exit /b 1
)
echo [OK] Migration created successfully

echo [4/6] Applying migration to database... 
dotnet ef database update --verbose
if errorlevel 1 (
    echo [ERROR] Failed to apply migration
    pause
    exit /b 1
)
echo [OK] Migration applied successfully

echo [5/6] Building project...
dotnet build "%PROJECT_PATH%\OpCentrix.csproj" --verbosity quiet
if errorlevel 1 (
    echo [ERROR] Build failed
    pause
    exit /b 1
)
echo [OK] Build successful

echo [6/6] Testing application startup...
echo [INFO] Starting application to test scheduler settings...
start "" cmd /c "cd /d \"%PROJECT_PATH%\" && dotnet run --project \"%PROJECT_PATH%\OpCentrix.csproj\" && pause"

REM Wait for startup
timeout /t 3 /nobreak >nul

echo.
echo ===================================================================
echo  SCHEDULER SETTINGS MIGRATION COMPLETE
echo ===================================================================
echo.
echo [SUCCESS] SchedulerSettings table created and ready!
echo.
echo NEXT STEPS:
echo 1. Application is starting in new window
echo 2. Navigate to: https://localhost:5001/Admin/SchedulerSettings
echo 3. Login: admin / admin123
echo 4. Configure scheduler settings
echo.
echo DATABASE CHANGES:
echo - SchedulerSettings table created
echo - Default settings will be auto-created on first access
echo - All scheduler logic now uses these settings
echo.
pause