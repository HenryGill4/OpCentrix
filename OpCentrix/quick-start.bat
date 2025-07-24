@echo off
REM OpCentrix Quick Start Script for Windows
REM This script tests and starts your OpCentrix manufacturing scheduler

echo ========================================
echo    OpCentrix SLS Manufacturing Scheduler
echo ========================================
echo. 

echo [1/4] Checking .NET 8 installation...
dotnet --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ .NET 8 SDK not found!
    echo Please install .NET 8 SDK from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)
echo ✅ .NET 8 SDK found

echo.
echo [2/4] Building application...
dotnet build --verbosity quiet --nologo
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Build failed! Check for compilation errors.
    pause
    exit /b 1
)
echo ✅ Build successful

echo.
echo [3/4] Checking core files...
set "MISSING_FILES="
if not exist "Pages\Scheduler\Index.cshtml" set "MISSING_FILES=Scheduler Page"
if not exist "Pages\Admin\Index.cshtml" set "MISSING_FILES=%MISSING_FILES% Admin Panel"
if not exist "wwwroot\js\scheduler-ui.js" set "MISSING_FILES=%MISSING_FILES% JavaScript"

if not "%MISSING_FILES%"=="" (
    echo ❌ Missing critical files: %MISSING_FILES%
    pause
    exit /b 1
)
echo ✅ All core files present

echo.
echo [4/4] Starting OpCentrix...
echo.
echo 🚀 OpCentrix is starting...
echo 📱 Open your browser to: http://localhost:5000
echo 🔑 Login with: admin / admin123
echo.
echo ⏹️  Press Ctrl+C to stop the application
echo.

REM Start the application
dotnet run

REM This line only executes if dotnet run exits
echo.
echo OpCentrix has stopped.
pause