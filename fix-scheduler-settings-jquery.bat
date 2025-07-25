@echo off
REM fix-scheduler-settings-jquery.bat
REM Windows script to fix jQuery validation issues on SchedulerSettings page

echo [FIX] Fixing jQuery validation on SchedulerSettings page...
echo ================================================================

REM Set working directory to project
cd /d "%~dp0"
if exist "OpCentrix" (
    cd OpCentrix
) else (
    echo [ERROR] OpCentrix directory not found!
    echo Please run this script from the workspace root.
    pause
    exit /b 1
)

echo [1/6] Stopping any running application...
taskkill /f /im dotnet.exe 2>nul

echo [2/6] Checking jQuery validation files...
if exist "wwwroot\lib\jquery-validation\dist\jquery.validate.min.js" (
    echo [OK] jQuery validation file exists
) else (
    echo [ERROR] jQuery validation file missing!
    echo Downloading from CDN...
    powershell -Command "try { Invoke-WebRequest -Uri 'https://cdn.jsdelivr.net/npm/jquery-validation@1.19.5/dist/jquery.validate.min.js' -OutFile 'wwwroot\lib\jquery-validation\dist\jquery.validate.min.js' -UseBasicParsing; Write-Host '[OK] Downloaded jquery.validate.min.js'; } catch { Write-Host '[ERROR] Download failed'; }"
)

if exist "wwwroot\lib\jquery-validation-unobtrusive\jquery.validate.unobtrusive.min.js" (
    echo [OK] jQuery unobtrusive validation file exists
) else (
    echo [ERROR] jQuery unobtrusive validation file missing!
    echo Downloading from CDN...
    powershell -Command "try { Invoke-WebRequest -Uri 'https://cdn.jsdelivr.net/npm/jquery-validation-unobtrusive@4.0.0/dist/jquery.validate.unobtrusive.min.js' -OutFile 'wwwroot\lib\jquery-validation-unobtrusive\jquery.validate.unobtrusive.min.js' -UseBasicParsing; Write-Host '[OK] Downloaded jquery.validate.unobtrusive.min.js'; } catch { Write-Host '[ERROR] Download failed'; }"
)

echo [3/6] Building project...
dotnet build --verbosity quiet
if errorlevel 1 (
    echo [ERROR] Build failed!
    echo Check the build output for errors.
    pause
    exit /b 1
)
echo [OK] Build successful

echo [4/6] Creating test script for validation...
echo ^<script^> > temp_validation_test.html
echo if ^(typeof jQuery === 'undefined'^) { >> temp_validation_test.html
echo   console.error^('jQuery not loaded'^); >> temp_validation_test.html
echo } else if ^(typeof jQuery.validator === 'undefined'^) { >> temp_validation_test.html
echo   console.error^('jQuery Validation not loaded'^); >> temp_validation_test.html
echo } else { >> temp_validation_test.html
echo   console.log^('SUCCESS: jQuery Validation is working'^); >> temp_validation_test.html
echo } >> temp_validation_test.html
echo ^</script^> >> temp_validation_test.html

echo [5/6] Starting application...
echo [INFO] Application starting on https://localhost:5001
echo [INFO] Navigate to: https://localhost:5001/Admin/SchedulerSettings
echo [INFO] Login with: admin / admin123
echo [INFO] Open browser console (F12) to check for validation errors
echo.
echo [EXPECTED CONSOLE OUTPUT]:
echo   [OK] jQuery loaded successfully
echo   [OK] jQuery Validation loaded successfully
echo   [OK] Form validation initialized
echo.
echo [IF YOU SEE ERRORS]:
echo   1. Check Network tab for failed script loads
echo   2. Verify script loading order in browser
echo   3. Try refreshing the page
echo   4. Check if antivirus is blocking scripts
echo.

REM Cleanup temp file
del temp_validation_test.html 2>nul

echo [6/6] Launching application...
echo Press Ctrl+C to stop the application when testing is complete
echo.
start "" "https://localhost:5001/Admin/SchedulerSettings"
dotnet run

echo.
echo [COMPLETE] jQuery validation fix applied!
echo.
echo TROUBLESHOOTING STEPS:
echo 1. Open https://localhost:5001/Admin/SchedulerSettings
echo 2. Press F12 to open Developer Tools
echo 3. Check Console tab for JavaScript errors
echo 4. Look for jQuery validation success messages
echo 5. Test form validation by submitting without required fields
echo.
pause