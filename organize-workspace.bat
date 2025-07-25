@echo off
echo.
echo ===================================================================
echo  OPCENTRIX SCRIPT ORGANIZATION - CREATING CLEAN STRUCTURE
echo ===================================================================
echo.

set "WORKSPACE_ROOT=C:\Users\Henry\Source\Repos\OpCentrix"

echo [1/4] Creating organized directory structure...
if not exist "%WORKSPACE_ROOT%\scripts" mkdir "%WORKSPACE_ROOT%\scripts"
if not exist "%WORKSPACE_ROOT%\scripts\Windows" mkdir "%WORKSPACE_ROOT%\scripts\Windows"
if not exist "%WORKSPACE_ROOT%\scripts\Linux" mkdir "%WORKSPACE_ROOT%\scripts\Linux"
if not exist "%WORKSPACE_ROOT%\docs" mkdir "%WORKSPACE_ROOT%\docs"

echo [2/4] Moving Windows scripts to organized location...
for %%f in (
    "add-files-to-solution.bat"
    "apply-scheduler-settings.bat"
    "create-scheduler-settings-migration.bat"
    "fix-file-structure.bat"
    "fix-jquery-validation.bat"
    "fix-scheduler-settings-database.bat"
    "quick-fix-database.bat"
    "quick-test.bat"
    "reset-to-production.bat"
    "scheduler-settings-fix-summary.bat"
    "setup-clean-database.bat"
    "setup-database.bat"
    "test-complete-system.bat"
    "verify-final-system.bat"
    "verify-parts-database.bat"
    "verify-setup.bat"
    "complete-scheduler-implementation.bat"
) do (
    if exist "%WORKSPACE_ROOT%\%%~f" (
        move "%WORKSPACE_ROOT%\%%~f" "%WORKSPACE_ROOT%\scripts\Windows\" >nul 2>&1
        echo [MOVED] %%~f to scripts\Windows\
    )
)

echo [3/4] Moving Linux scripts to organized location...
for %%f in (
    "fix-jquery-validation.sh"
    "quick-test.sh"
    "reset-to-production.sh"
    "setup-clean-database.sh"
    "setup-database.sh"
    "test-complete-system.sh"
    "verify-final-system.sh"
    "verify-parts-database.sh"
    "verify-setup.sh"
) do (
    if exist "%WORKSPACE_ROOT%\%%~f" (
        move "%WORKSPACE_ROOT%\%%~f" "%WORKSPACE_ROOT%\scripts\Linux\" >nul 2>&1
        echo [MOVED] %%~f to scripts\Linux\
    )
)

echo [4/4] Moving documentation to organized location...
for %%f in (
    "AI-INSTRUCTIONS-JQUERY-FIX.md"
    "AI-INSTRUCTIONS-NO-UNICODE.md"
    "DATABASE-ANALYSIS-AND-TODO.md"
    "DATABASE-LOGIC-ANALYSIS.md"
    "DATABASE-LOGIC-FIXES-COMPLETE.md"
    "DATABASE_SETUP.md"
    "FILE-STRUCTURE-ISSUE-RESOLVED.md"
    "FULL-PATH-INTEGRATION-COMPLETE.md"
    "JQUERY-VALIDATION-FIX-COMPLETE.md"
    "PARTS-TROUBLESHOOTING-GUIDE.md"
    "SCHEDULER-SETTINGS-IMPLEMENTATION-COMPLETE.md"
    "UNICODE-CLEANUP-COMPLETE.md"
) do (
    if exist "%WORKSPACE_ROOT%\%%~f" (
        move "%WORKSPACE_ROOT%\%%~f" "%WORKSPACE_ROOT%\docs\" >nul 2>&1
        echo [MOVED] %%~f to docs\
    )
)

echo.
echo ===================================================================
echo  SCRIPT ORGANIZATION COMPLETE
echo ===================================================================
echo.
echo ORGANIZED STRUCTURE:
echo.
echo %WORKSPACE_ROOT%\
echo ??? scripts\
echo ?   ??? Windows\          # All .bat files
echo ?   ??? Linux\            # All .sh files
echo ??? docs\                 # All .md documentation
echo ??? OpCentrix\           # Main project code
echo ??? README.md            # Main readme
echo ??? SETUP_COMPLETE.md    # Setup guide
echo ??? setup-clean-database.bat     # Quick access
echo ??? start-application.bat        # Quick access
echo ??? diagnose-system.bat          # Quick access
echo.
echo [SUCCESS] All scripts and documentation organized!
echo.
echo TO USE SCRIPTS:
echo - Windows: %WORKSPACE_ROOT%\scripts\Windows\[script-name].bat
echo - Linux:   %WORKSPACE_ROOT%\scripts\Linux\[script-name].sh
echo - Docs:    %WORKSPACE_ROOT%\docs\[document-name].md
echo.
pause