@echo off
REM Quick Project File Sync Script
REM Copies essential documentation from workspace root to project structure

echo [INFO] OpCentrix Project File Sync Starting...

set "WORKSPACE_ROOT=C:\Users\Henry\Source\Repos\OpCentrix"
set "PROJECT_ROOT=%WORKSPACE_ROOT%\OpCentrix"

REM Create Documentation folder if it doesn't exist
if not exist "%PROJECT_ROOT%\Documentation" mkdir "%PROJECT_ROOT%\Documentation"

echo [INFO] Copying essential documentation files...

REM Copy critical documentation files
if exist "%WORKSPACE_ROOT%\SCHEDULER-FIX-CHECKLIST.md" (
    copy "%WORKSPACE_ROOT%\SCHEDULER-FIX-CHECKLIST.md" "%PROJECT_ROOT%\Documentation\" >nul 2>&1
    echo [OK] Copied SCHEDULER-FIX-CHECKLIST.md
)

if exist "%WORKSPACE_ROOT%\WEEKEND-OPERATIONS-FIX-COMPLETE.md" (
    copy "%WORKSPACE_ROOT%\WEEKEND-OPERATIONS-FIX-COMPLETE.md" "%PROJECT_ROOT%\Documentation\" >nul 2>&1
    echo [OK] Copied WEEKEND-OPERATIONS-FIX-COMPLETE.md
)

if exist "%WORKSPACE_ROOT%\DATABASE-LOGIC-ANALYSIS.md" (
    copy "%WORKSPACE_ROOT%\DATABASE-LOGIC-ANALYSIS.md" "%PROJECT_ROOT%\Documentation\" >nul 2>&1
    echo [OK] Copied DATABASE-LOGIC-ANALYSIS.md
)

if exist "%WORKSPACE_ROOT%\COMPREHENSIVE-SCHEDULER-FIX-PLAN.md" (
    copy "%WORKSPACE_ROOT%\COMPREHENSIVE-SCHEDULER-FIX-PLAN.md" "%PROJECT_ROOT%\Documentation\" >nul 2>&1
    echo [OK] Copied COMPREHENSIVE-SCHEDULER-FIX-PLAN.md
)

if exist "%WORKSPACE_ROOT%\AI-INSTRUCTIONS-NO-UNICODE.md" (
    copy "%WORKSPACE_ROOT%\AI-INSTRUCTIONS-NO-UNICODE.md" "%PROJECT_ROOT%\Documentation\" >nul 2>&1
    echo [OK] Copied AI-INSTRUCTIONS-NO-UNICODE.md
)

REM Copy essential project files
if exist "%WORKSPACE_ROOT%\README.md" (
    if not exist "%PROJECT_ROOT%\README.md" (
        copy "%WORKSPACE_ROOT%\README.md" "%PROJECT_ROOT%\" >nul 2>&1
        echo [OK] Copied README.md to project root
    )
)

if exist "%WORKSPACE_ROOT%\.env.example" (
    if not exist "%PROJECT_ROOT%\.env.example" (
        copy "%WORKSPACE_ROOT%\.env.example" "%PROJECT_ROOT%\" >nul 2>&1
        echo [OK] Copied .env.example to project root
    )
)

echo.
echo [SUCCESS] Essential files synchronized to project structure
echo.
echo Documentation files are now in: %PROJECT_ROOT%\Documentation\
echo.
echo Next steps:
echo 1. Open Visual Studio
echo 2. Reload the OpCentrix solution
echo 3. Files will appear in Solution Explorer
echo 4. Use SCHEDULER-FIX-CHECKLIST.md to continue implementation

pause