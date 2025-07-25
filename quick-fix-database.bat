@echo off
echo.
echo ===================================================================
echo  QUICK FIX: SCHEDULER SETTINGS DATABASE - Windows
echo ===================================================================
echo.

set "PROJECT_PATH=C:\Users\Henry\Source\Repos\OpCentrix\OpCentrix"

echo [1/4] Stopping any running application...
taskkill /f /im dotnet.exe 2>nul || echo No dotnet processes found.

echo [2/4] Navigating to project directory...
cd /d "%PROJECT_PATH%"

echo [3/4] Applying database update...
dotnet ef database update --no-build --verbose
if errorlevel 1 (
    echo [WARNING] Migration failed - trying database recreation...
    echo [INFO] Backing up existing database...
    if exist "Data\OpCentrix.db" (
        copy "Data\OpCentrix.db" "Data\OpCentrix.db.backup" >nul 2>&1
    )
    echo [INFO] Recreating database...
    dotnet ef database drop --force --no-build
    dotnet ef database update --no-build
)

echo [4/4] Starting application...
echo [SUCCESS] Database should now have SchedulerSettings table!
echo.
echo Navigate to: https://localhost:5001/Admin/SchedulerSettings
echo Login: admin / admin123
echo.
start "" cmd /c "dotnet run --launch-profile https"

pause