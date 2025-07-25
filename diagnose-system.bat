@echo off
echo.
echo ===================================================================
echo  JQUERY VALIDATION DIAGNOSTIC - Windows
echo ===================================================================
echo.

REM Get the full path to the workspace directory
set "WORKSPACE_PATH=C:\Users\Henry\Source\Repos\OpCentrix"
set "PROJECT_PATH=%WORKSPACE_PATH%\OpCentrix"

echo Current workspace: %WORKSPACE_PATH%
echo Project path: %PROJECT_PATH%
echo.

REM Check if we're in the right directory structure
if not exist "%WORKSPACE_PATH%\OpCentrix" (
    echo [ERROR] OpCentrix directory not found at %WORKSPACE_PATH%!
    echo Make sure you're running this from the correct workspace directory.
    echo Expected structure: C:\Users\Henry\Source\Repos\OpCentrix\OpCentrix\
    pause
    exit /b 1
)

REM Navigate to project directory using full path
cd /d "%PROJECT_PATH%"

echo [1/5] Checking project structure...
if exist "%PROJECT_PATH%\OpCentrix.csproj" (
    echo [OK] OpCentrix.csproj found at %PROJECT_PATH%\OpCentrix.csproj
) else (
    echo [ERROR] OpCentrix.csproj NOT found at %PROJECT_PATH%\OpCentrix.csproj
)

if exist "%PROJECT_PATH%\wwwroot\lib\jquery\dist\jquery.min.js" (
    echo [OK] jQuery file found at %PROJECT_PATH%\wwwroot\lib\jquery\dist\jquery.min.js
) else (
    echo [ERROR] jQuery file NOT found at %PROJECT_PATH%\wwwroot\lib\jquery\dist\jquery.min.js
)

if exist "%PROJECT_PATH%\wwwroot\lib\jquery-validation\dist\jquery.validate.min.js" (
    echo [OK] jQuery Validation file found at %PROJECT_PATH%\wwwroot\lib\jquery-validation\dist\jquery.validate.min.js
) else (
    echo [ERROR] jQuery Validation file NOT found at %PROJECT_PATH%\wwwroot\lib\jquery-validation\dist\jquery.validate.min.js
)

if exist "%PROJECT_PATH%\Pages\Shared\_Layout.cshtml" (
    echo [OK] Layout file found at %PROJECT_PATH%\Pages\Shared\_Layout.cshtml
) else (
    echo [ERROR] Layout file NOT found at %PROJECT_PATH%\Pages\Shared\_Layout.cshtml
)

echo.
echo [2/5] Checking .NET installation...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK not found or not in PATH
    echo Please install .NET 8 SDK from https://dot.net
) else (
    for /f "tokens=*" %%i in ('dotnet --version') do echo [OK] .NET SDK version: %%i
)

echo.
echo [3/5] Checking project dependencies...
echo Restoring packages for project at %PROJECT_PATH%...
dotnet restore "%PROJECT_PATH%\OpCentrix.csproj" --verbosity quiet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Failed to restore NuGet packages for %PROJECT_PATH%\OpCentrix.csproj
) else (
    echo [OK] NuGet packages restored successfully for %PROJECT_PATH%\OpCentrix.csproj
)

echo.
echo [4/5] Testing project build...
echo Building project at %PROJECT_PATH%...
dotnet build "%PROJECT_PATH%\OpCentrix.csproj" --verbosity quiet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Project build failed for %PROJECT_PATH%\OpCentrix.csproj
    echo Run 'dotnet build "%PROJECT_PATH%\OpCentrix.csproj"' to see detailed errors
) else (
    echo [OK] Project builds successfully at %PROJECT_PATH%\OpCentrix.csproj
)

echo.
echo [5/5] Checking for running processes...
tasklist /FI "IMAGENAME eq dotnet.exe" 2>nul | find /I "dotnet.exe" >nul
if errorlevel 1 (
    echo [OK] No conflicting dotnet processes running
) else (
    echo [WARNING] Found running dotnet processes:
    tasklist /FI "IMAGENAME eq dotnet.exe" 2>nul 
)

echo.
echo ===================================================================
echo  DIAGNOSTIC COMPLETE
echo ===================================================================
echo.

REM Check port availability
echo Checking if port 5001 is available...
netstat -an | find ":5001" >nul
if errorlevel 1 (
    echo [OK] Port 5001 is available
) else (
    echo [WARNING] Port 5001 is in use:
    netstat -an | find ":5001"
)

echo.
echo NEXT STEPS:
echo 1. If all checks passed: Run '%WORKSPACE_PATH%\fix-jquery-validation.bat'
echo 2. If errors found: Address the issues above first
echo 3. For detailed errors: Run 'dotnet build "%PROJECT_PATH%\OpCentrix.csproj"' to see full output
echo 4. To run application: Run 'dotnet run --project "%PROJECT_PATH%\OpCentrix.csproj"'
echo.
echo Full paths being used:
echo - Workspace: %WORKSPACE_PATH%
echo - Project: %PROJECT_PATH%
echo - Solution: %WORKSPACE_PATH%\OpCentrix.sln
echo.
pause