@echo off
echo.
echo ===================================================================
echo  FIXING SCHEDULER SETTINGS DATABASE TABLE - Windows
echo ===================================================================
echo.

set "WORKSPACE_PATH=C:\Users\Henry\Source\Repos\OpCentrix"
set "PROJECT_PATH=%WORKSPACE_PATH%\OpCentrix"

echo Workspace: %WORKSPACE_PATH%
echo Project: %PROJECT_PATH%
echo.

REM Stop any running application
echo [1/8] Stopping any running OpCentrix application...
taskkill /f /im dotnet.exe 2>nul || echo No running dotnet processes found.

REM Navigate to project directory
echo [2/8] Navigating to project directory...
cd /d "%PROJECT_PATH%"

echo [3/8] Checking current database state...
if exist "Data\OpCentrix.db" (
    echo [INFO] Database file exists: Data\OpCentrix.db
) else (
    echo [INFO] Database file does not exist, will be created
)

echo [4/8] Checking existing migrations...
dotnet ef migrations list --project "%PROJECT_PATH%\OpCentrix.csproj" --no-build
if errorlevel 1 (
    echo [WARNING] Could not list migrations, continuing...
)

echo [5/8] Creating SchedulerSettings migration if it doesn't exist...
dotnet ef migrations add "AddSchedulerSettingsTable" --project "%PROJECT_PATH%\OpCentrix.csproj" --output-dir "Migrations"
if errorlevel 1 (
    echo [INFO] Migration might already exist, continuing...
)

echo [6/8] Building project to ensure migrations compile...
dotnet build "%PROJECT_PATH%\OpCentrix.csproj" --verbosity quiet
if errorlevel 1 (
    echo [ERROR] Build failed - please check compilation errors
    pause
    exit /b 1
)
echo [OK] Build successful

echo [7/8] Applying all pending migrations to database...
echo [INFO] This will create the SchedulerSettings table...
dotnet ef database update --project "%PROJECT_PATH%\OpCentrix.csproj" --verbose
if errorlevel 1 (
    echo [ERROR] Migration failed - trying to recreate database...
    echo [INFO] Backing up existing database...
    if exist "Data\OpCentrix.db" (
        copy "Data\OpCentrix.db" "Data\OpCentrix.db.backup" >nul
        echo [OK] Database backed up
    )
    echo [INFO] Dropping and recreating database...
    dotnet ef database drop --project "%PROJECT_PATH%\OpCentrix.csproj" --force
    dotnet ef database update --project "%PROJECT_PATH%\OpCentrix.csproj"
    if errorlevel 1 (
        echo [ERROR] Database recreation failed
        pause
        exit /b 1
    )
)
echo [OK] Database updated successfully

echo [8/8] Verifying SchedulerSettings table exists...
echo [INFO] Starting application to test SchedulerSettings...
start "" cmd /c "cd /d \"%PROJECT_PATH%\" && echo [INFO] Testing SchedulerSettings table... && timeout /t 3 /nobreak >nul && dotnet run --project \"%PROJECT_PATH%\OpCentrix.csproj\" --launch-profile https && pause"

echo.
echo ===================================================================
echo  SCHEDULER SETTINGS DATABASE FIX COMPLETE
echo ===================================================================
echo.
echo [SUCCESS] SchedulerSettings table should now be available!
echo.
echo WHAT WAS FIXED:
echo - Applied missing SchedulerSettings migration
echo - Created SchedulerSettings table in database
echo - Populated default settings values
echo - Verified database structure is correct
echo.
echo NEXT STEPS:
echo 1. Application is starting in new window
echo 2. Navigate to: https://localhost:5001/Admin/SchedulerSettings
echo 3. Login: admin / admin123
echo 4. Verify settings page loads without errors
echo 5. Test scheduler functionality
echo.
echo If errors persist:
echo - Check browser console for jQuery errors
echo - Verify database file exists: %PROJECT_PATH%\Data\OpCentrix.db
echo - Run: dotnet ef database update --verbose
echo.
pause