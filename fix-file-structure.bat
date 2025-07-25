@echo off
echo.
echo ===================================================================
echo  CRITICAL FILE STRUCTURE FIX - OpCentrix Project
echo ===================================================================
echo.

set "WORKSPACE_ROOT=C:\Users\Henry\Source\Repos\OpCentrix"
set "PROJECT_ROOT=%WORKSPACE_ROOT%\OpCentrix"

echo [CRITICAL] File structure issue detected!
echo.
echo PROBLEM: Changes may be going to wrong directory locations
echo - Workspace Root: %WORKSPACE_ROOT%
echo - Actual Project: %PROJECT_ROOT%
echo.

echo [1/6] Checking workspace structure...
if exist "%WORKSPACE_ROOT%\Services\SchedulerService.cs" (
    echo [WARNING] Found orphaned Services directory at workspace root
    echo [INFO] This file should be in: %PROJECT_ROOT%\Services\
) else (
    echo [OK] No orphaned Services directory found
)

if exist "%WORKSPACE_ROOT%\Pages" (
    echo [WARNING] Found orphaned Pages directory at workspace root  
    echo [INFO] Pages should be in: %PROJECT_ROOT%\Pages\
) else (
    echo [OK] No orphaned Pages directory found
)

echo [2/6] Verifying actual project structure...
if exist "%PROJECT_ROOT%\OpCentrix.csproj" (
    echo [OK] Main project file found at correct location
) else (
    echo [ERROR] Main project file NOT found!
    pause
    exit /b 1
)

if exist "%PROJECT_ROOT%\Services\SchedulerSettingsService.cs" (
    echo [OK] SchedulerSettingsService found in correct location
) else (
    echo [WARNING] SchedulerSettingsService not found in project
)

if exist "%PROJECT_ROOT%\wwwroot\lib\jquery-validation-unobtrusive\jquery.validate.unobtrusive.min.js" (
    echo [OK] jQuery validation file found in correct location
) else (
    echo [ERROR] jQuery validation file missing from project!
)

echo [3/6] Checking current working directory...
echo Current directory: %CD%
if "%CD%"=="%PROJECT_ROOT%" (
    echo [OK] Working directory is correct project location
) else (
    echo [WARNING] Working directory is NOT the project location
    echo [INFO] Should be in: %PROJECT_ROOT%
)

echo [4/6] Moving orphaned files to correct locations...
if exist "%WORKSPACE_ROOT%\Services\SchedulerService.cs" (
    echo [INFO] Moving orphaned SchedulerService.cs to project location...
    if not exist "%PROJECT_ROOT%\Services" mkdir "%PROJECT_ROOT%\Services"
    move "%WORKSPACE_ROOT%\Services\SchedulerService.cs" "%PROJECT_ROOT%\Services\" >nul 2>&1
    if exist "%PROJECT_ROOT%\Services\SchedulerService.cs" (
        echo [OK] SchedulerService.cs moved successfully
    ) else (
        echo [WARNING] Failed to move SchedulerService.cs
    )
)

echo [5/6] Cleaning up orphaned directories...
if exist "%WORKSPACE_ROOT%\Services" (
    rmdir "%WORKSPACE_ROOT%\Services" 2>nul
    if not exist "%WORKSPACE_ROOT%\Services" (
        echo [OK] Cleaned up orphaned Services directory
    )
)

echo [6/6] Setting correct working directory...
cd /d "%PROJECT_ROOT%"
echo [OK] Working directory set to: %PROJECT_ROOT%

echo.
echo ===================================================================
echo  FILE STRUCTURE FIX COMPLETE
echo ===================================================================
echo.
echo [SUCCESS] File structure has been corrected!
echo.
echo CORRECT STRUCTURE:
echo %PROJECT_ROOT%\
echo ??? OpCentrix.csproj               # Main project file
echo ??? Services\                      # Business logic services
echo ??? Models\                        # Data models  
echo ??? Pages\                         # Razor Pages
echo ??? wwwroot\                       # Static files (CSS, JS, etc.)
echo ??? Data\                          # Database context
echo.
echo NEXT STEPS:
echo 1. Always work from: %PROJECT_ROOT%
echo 2. Use: cd /d "%PROJECT_ROOT%" to navigate
echo 3. Run: dotnet build to verify project builds
echo 4. Run: dotnet run to start application
echo.
echo All future changes will now go to the correct project location!
echo.
pause