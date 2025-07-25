@echo off
REM OpCentrix Quick Build and Run Script for Windows
REM This script works in both Command Prompt and PowerShell

setlocal EnableDelayedExpansion

echo [INFO] OpCentrix Quick Build Script Starting...
echo.

REM Define paths
set "WORKSPACE_ROOT=C:\Users\Henry\Source\Repos\OpCentrix"
set "PROJECT_ROOT=%WORKSPACE_ROOT%\OpCentrix"
set "PROJECT_FILE=%PROJECT_ROOT%\OpCentrix.csproj"

echo [INFO] Workspace: %WORKSPACE_ROOT%
echo [INFO] Project: %PROJECT_ROOT%
echo.

REM Check if we're in the right location
if not exist "%PROJECT_FILE%" (
    echo [ERROR] OpCentrix project file not found!
    echo [ERROR] Expected: %PROJECT_FILE%
    echo [ERROR] Please ensure you're running this from the correct directory.
    pause
    exit /b 1
)

echo [OK] OpCentrix project file found
echo.

REM Step 1: Organize project files if script exists
if exist "%WORKSPACE_ROOT%\organize-project-files.bat" (
    echo [INFO] Running project organization script...
    call "%WORKSPACE_ROOT%\organize-project-files.bat"
    echo [OK] Project organization completed
    echo.
) else (
    echo [SKIP] Project organization script not found
    echo.
)

REM Step 2: Navigate to project directory
echo [INFO] Navigating to project directory...
cd /d "%PROJECT_ROOT%"
if %errorlevel% neq 0 (
    echo [ERROR] Failed to navigate to project directory
    pause
    exit /b 1
)
echo [OK] Now in project directory
echo.

REM Step 3: Check .NET installation
echo [INFO] Checking .NET installation...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] .NET not found or not working
    echo [ERROR] Please install .NET 8 SDK
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do (
    echo [OK] .NET SDK version: %%i
)
echo.

REM Step 4: Clean previous build
echo [INFO] Cleaning previous build...
dotnet clean --verbosity quiet >nul 2>&1
if %errorlevel% neq 0 (
    echo [WARNING] Clean command failed, continuing anyway...
) else (
    echo [OK] Clean completed
)
echo.

REM Step 5: Restore packages
echo [INFO] Restoring NuGet packages...
dotnet restore --verbosity quiet
if %errorlevel% neq 0 (
    echo [ERROR] Package restore failed
    pause
    exit /b 1
)
echo [OK] Packages restored
echo.

REM Step 6: Build project
echo [INFO] Building project...
dotnet build --verbosity minimal --no-restore
if %errorlevel% neq 0 (
    echo [ERROR] Build failed!
    echo [ERROR] Check the output above for details
    pause
    exit /b 1
)
echo [OK] Build completed successfully
echo.

REM Step 7: Ask user if they want to run the application
echo [QUESTION] Do you want to run the application now? (Y/N)
set /p RUN_APP="Enter Y to run, N to exit: "

if /i "%RUN_APP%"=="Y" (
    echo.
    echo [INFO] Starting application...
    echo [INFO] Application will be available at: https://localhost:5001
    echo [INFO] Press Ctrl+C to stop the application
    echo.
    dotnet run --no-build
) else (
    echo [INFO] Build completed. Application not started.
    echo [INFO] To run manually, use: dotnet run
)

echo.
echo [SUCCESS] Script completed successfully!
pause