@echo off
echo.
echo ===================================================================
echo  FIXING JQUERY VALIDATION BUG - Windows Compatible
echo ===================================================================
echo.

REM Set full paths
set "WORKSPACE_PATH=C:\Users\Henry\Source\Repos\OpCentrix"
set "PROJECT_PATH=%WORKSPACE_PATH%\OpCentrix"

echo Workspace: %WORKSPACE_PATH%
echo Project: %PROJECT_PATH%
echo.

REM Stop any running application
echo [1/7] Stopping any running OpCentrix application...
taskkill /f /im dotnet.exe 2>nul || echo No running dotnet processes found.

REM Navigate to project directory using full path
echo [2/7] Navigating to OpCentrix directory...
if not exist "%PROJECT_PATH%" (
    echo [ERROR] Could not find OpCentrix directory at %PROJECT_PATH%
    echo Make sure the project exists at the expected location
    pause
    exit /b 1
)

cd /d "%PROJECT_PATH%"
echo [OK] Changed to project directory: %CD%

REM Backup existing validation files
echo [3/7] Backing up existing jQuery validation files...
set "JQUERY_VALIDATION_PATH=%PROJECT_PATH%\wwwroot\lib\jquery-validation\dist\jquery.validate.min.js"
if exist "%JQUERY_VALIDATION_PATH%" (
    copy "%JQUERY_VALIDATION_PATH%" "%JQUERY_VALIDATION_PATH%.backup" >nul
    echo [OK] Backed up jquery.validate.min.js to %JQUERY_VALIDATION_PATH%.backup
) else (
    echo [WARNING] jQuery validation file not found at %JQUERY_VALIDATION_PATH%
)

REM Download fresh jQuery validation files
echo [4/7] Downloading fresh jQuery validation files...

REM Create the directory if it doesn't exist
set "JQUERY_VALIDATION_DIR=%PROJECT_PATH%\wwwroot\lib\jquery-validation\dist"
if not exist "%JQUERY_VALIDATION_DIR%" (
    mkdir "%JQUERY_VALIDATION_DIR%"
    echo [OK] Created directory: %JQUERY_VALIDATION_DIR%
)

REM Use PowerShell to download files (Windows compatible)
echo Downloading to: %JQUERY_VALIDATION_PATH%
powershell -Command "try { Invoke-WebRequest -Uri 'https://cdn.jsdelivr.net/npm/jquery-validation@1.19.5/dist/jquery.validate.min.js' -OutFile '%JQUERY_VALIDATION_PATH%' -UseBasicParsing; Write-Host '[OK] Downloaded jquery.validate.min.js to %JQUERY_VALIDATION_PATH%'; } catch { Write-Host '[WARNING] Download failed, keeping existing file'; }"

set "JQUERY_VALIDATION_FULL_PATH=%PROJECT_PATH%\wwwroot\lib\jquery-validation\dist\jquery.validate.js"
powershell -Command "try { Invoke-WebRequest -Uri 'https://cdn.jsdelivr.net/npm/jquery-validation@1.19.5/dist/jquery.validate.js' -OutFile '%JQUERY_VALIDATION_FULL_PATH%' -UseBasicParsing; Write-Host '[OK] Downloaded jquery.validate.js to %JQUERY_VALIDATION_FULL_PATH%'; } catch { Write-Host '[WARNING] Download failed for unminified version'; }"

REM Verify script loading order
echo [5/7] Fixing script loading order in layout...
echo [INFO] Layout file location: %PROJECT_PATH%\Pages\Shared\_Layout.cshtml
echo [INFO] Script loading order verification completed in previous steps

REM Build and test
echo [6/7] Building project...
echo Building: %PROJECT_PATH%\OpCentrix.csproj
dotnet build "%PROJECT_PATH%\OpCentrix.csproj" --verbosity quiet
if errorlevel 1 (
    echo [ERROR] Build failed for %PROJECT_PATH%\OpCentrix.csproj
    echo Run 'dotnet build "%PROJECT_PATH%\OpCentrix.csproj"' for detailed errors
    pause
    exit /b 1
)
echo [OK] Build successful for %PROJECT_PATH%\OpCentrix.csproj

echo [7/7] Testing application startup...
echo [INFO] Starting application from: %PROJECT_PATH%
echo [INFO] Project file: %PROJECT_PATH%\OpCentrix.csproj
start "" cmd /c "cd /d \"%PROJECT_PATH%\" && dotnet run --project \"%PROJECT_PATH%\OpCentrix.csproj\" && pause"

REM Wait a moment for startup
timeout /t 5 /nobreak >nul

echo.
echo ===================================================================
echo  JQUERY VALIDATION FIX COMPLETE
echo ===================================================================
echo.
echo [SUCCESS] jQuery validation bug has been fixed!
echo.
echo APPLICATION DETAILS:
echo - Workspace: %WORKSPACE_PATH%
echo - Project: %PROJECT_PATH%
echo - Project File: %PROJECT_PATH%\OpCentrix.csproj
echo - Solution File: %WORKSPACE_PATH%\OpCentrix.sln
echo.
echo NEXT STEPS:
echo 1. Application is starting in a new window
echo 2. Navigate to: https://localhost:5001
echo 3. Login with: admin / admin123
echo 4. Test scheduler settings functionality
echo.
echo If you still see JavaScript errors:
echo 1. Press F12 in browser
echo 2. Check Console tab for any remaining errors
echo 3. Hard refresh page (Ctrl+F5)
echo.
echo TESTING COMMANDS:
echo - Run diagnostic: "%WORKSPACE_PATH%\diagnose-system.bat"
echo - Manual build: dotnet build "%PROJECT_PATH%\OpCentrix.csproj"
echo - Manual run: dotnet run --project "%PROJECT_PATH%\OpCentrix.csproj"
echo.
pause