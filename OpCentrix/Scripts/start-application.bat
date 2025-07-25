@echo off
echo ================================
echo   OpCentrix Application Starter
echo ================================
echo.

REM Check if database exists
if not exist "scheduler.db" (
    echo ?? Database not found!
    echo ?? Running database setup first...
    call setup-database.bat
    if %errorlevel% neq 0 (
        echo ERROR: Database setup failed
        pause
        exit /b 1
    )
    echo.
)

REM Navigate to project directory
cd /d "%~dp0"
if not exist "OpCentrix.csproj" (
    cd OpCentrix
)

echo ?? Starting OpCentrix Application...
echo ?? URL: http://localhost:5000
echo ?? Login: admin / admin123
echo ?? Press Ctrl+C to stop
echo.

REM Start the application
dotnet run