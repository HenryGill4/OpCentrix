@echo off
REM OpCentrix Project File Organization and Synchronization Script
REM This script ensures all project files are in the correct locations
 
setlocal EnableDelayedExpansion

echo [INFO] OpCentrix Project File Organization Starting...
echo.

REM Define paths
set "WORKSPACE_ROOT=C:\Users\Henry\Source\Repos\OpCentrix"
set "PROJECT_ROOT=%WORKSPACE_ROOT%\OpCentrix"
set "DOCS_FOLDER=%PROJECT_ROOT%\Documentation"
set "SCRIPTS_FOLDER=%PROJECT_ROOT%\Scripts"

echo [INFO] Workspace Root: %WORKSPACE_ROOT%
echo [INFO] Project Root: %PROJECT_ROOT%
echo [INFO] Documentation Folder: %DOCS_FOLDER%
echo [INFO] Scripts Folder: %SCRIPTS_FOLDER%
echo.

REM Create documentation and scripts folders in project
if not exist "%DOCS_FOLDER%" (
    echo [CREATE] Creating Documentation folder...
    mkdir "%DOCS_FOLDER%"
)

if not exist "%SCRIPTS_FOLDER%" (
    echo [CREATE] Creating Scripts folder...
    mkdir "%SCRIPTS_FOLDER%"
)

echo [INFO] Moving critical documentation files to project...

REM Critical Implementation Documentation (move to project Documentation folder)
set "CRITICAL_DOCS=SCHEDULER-FIX-CHECKLIST.md WEEKEND-OPERATIONS-FIX-COMPLETE.md 2-MONTH-SCHEDULER-IMPLEMENTATION-COMPLETE.md COMPREHENSIVE-SCHEDULER-FIX-PLAN.md HEADER-SIZING-FIX-COMPLETE.md SCHEDULER-ENHANCEMENT-COMPLETE.md DATABASE-LOGIC-ANALYSIS.md DATABASE-LOGIC-FIXES-COMPLETE.md PARTS-TROUBLESHOOTING-GUIDE.md UNICODE-CLEANUP-COMPLETE.md AI-INSTRUCTIONS-NO-UNICODE.md FULL-PATH-INTEGRATION-COMPLETE.md FILE-STRUCTURE-ISSUE-RESOLVED.md SCHEDULER-SETTINGS-IMPLEMENTATION-COMPLETE.md JQUERY-VALIDATION-FIX-COMPLETE.md"

for %%f in (%CRITICAL_DOCS%) do (
    if exist "%WORKSPACE_ROOT%\%%f" (
        echo [MOVE] Moving %%f to Documentation folder...
        move "%WORKSPACE_ROOT%\%%f" "%DOCS_FOLDER%\%%f" >nul 2>&1
        if !errorlevel! equ 0 (
            echo [OK] Successfully moved %%f
        ) else (
            echo [ERROR] Failed to move %%f
        )
    ) else if exist "%PROJECT_ROOT%\%%f" (
        echo [MOVE] Moving %%f from project root to Documentation folder...
        move "%PROJECT_ROOT%\%%f" "%DOCS_FOLDER%\%%f" >nul 2>&1
        if !errorlevel! equ 0 (
            echo [OK] Successfully moved %%f
        ) else (
            echo [ERROR] Failed to move %%f
        )
    ) else (
        echo [SKIP] %%f not found in workspace or project root
    )
)

echo.
echo [INFO] Moving scripts to project Scripts folder...

REM Move script files to project Scripts folder
set "SCRIPT_FILES=diagnose-system.bat fix-jquery-validation.bat setup-clean-database.bat setup-database.bat start-application.bat test-complete-system.bat verify-final-system.bat verify-parts-database.bat quick-test.bat reset-to-production.bat verify-setup.bat add-files-to-solution.bat"

for %%f in (%SCRIPT_FILES%) do (
    if exist "%WORKSPACE_ROOT%\%%f" (
        echo [MOVE] Moving %%f to Scripts folder...
        move "%WORKSPACE_ROOT%\%%f" "%SCRIPTS_FOLDER%\%%f" >nul 2>&1
        if !errorlevel! equ 0 (
            echo [OK] Successfully moved %%f
        ) else (
            echo [ERROR] Failed to move %%f
        )
    ) else (
        echo [SKIP] %%f not found in workspace root
    )
)

echo.
echo [INFO] Copying essential files that should remain in both locations...

REM Copy essential files that need to be accessible from workspace root
set "ESSENTIAL_FILES=README.md SETUP_COMPLETE.md DATABASE_SETUP.md .env.example"

for %%f in (%ESSENTIAL_FILES%) do (
    if exist "%WORKSPACE_ROOT%\%%f" (
        if not exist "%PROJECT_ROOT%\%%f" (
            echo [COPY] Copying %%f to project root...
            copy "%WORKSPACE_ROOT%\%%f" "%PROJECT_ROOT%\%%f" >nul 2>&1
            if !errorlevel! equ 0 (
                echo [OK] Successfully copied %%f
            ) else (
                echo [ERROR] Failed to copy %%f
            )
        ) else (
            echo [EXISTS] %%f already exists in project root
        )
    ) else (
        echo [SKIP] %%f not found in workspace root
    )
)

echo.
echo [INFO] Creating project structure reference files...

REM Create a project structure documentation file
(
echo # OpCentrix Project Structure
echo.
echo ## Project Organization
echo.
echo ```
echo OpCentrix\
echo ├── OpCentrix.csproj              # Main project file
echo ├── Program.cs                    # Application entry point
echo ├── Data\                         # Database context and configuration
echo │   └── SchedulerContext.cs
echo ├── Models\                       # Data models and view models
echo │   ├── Job.cs
echo │   ├── Part.cs
echo │   ├── SchedulerSettings.cs
echo │   └── ViewModels\
echo ├── Services\                     # Business logic services
echo │   ├── SchedulerService.cs
echo │   ├── SchedulerSettingsService.cs
echo │   └── SlsDataSeedingService.cs
echo ├── Pages\                        # Razor Pages
echo │   ├── Scheduler\
echo │   ├── Admin\
echo │   ├── PrintTracking\
echo │   └── Shared\
echo ├── wwwroot\                      # Static web assets
echo │   ├── css\
echo │   ├── js\
echo │   └── lib\
echo ├── Migrations\                   # Entity Framework migrations
echo ├── Documentation\               # Project documentation
echo │   ├── SCHEDULER-FIX-CHECKLIST.md
echo │   ├── WEEKEND-OPERATIONS-FIX-COMPLETE.md
echo │   └── [Other implementation docs]
echo └── Scripts\                      # Utility scripts
echo     ├── diagnose-system.bat
echo     ├── setup-clean-database.bat
echo     └── [Other utility scripts]
echo ```
echo.
echo ## Important Files
echo.
echo ### Core Implementation Files
echo - **SCHEDULER-FIX-CHECKLIST.md**: Comprehensive checklist for fixing scheduler issues
echo - **WEEKEND-OPERATIONS-FIX-COMPLETE.md**: Weekend operations implementation
echo - **DATABASE-LOGIC-FIXES-COMPLETE.md**: Database fixes and optimizations
echo.
echo ### Setup and Maintenance
echo - **Scripts\setup-clean-database.bat**: Database initialization
echo - **Scripts\diagnose-system.bat**: System health check
echo - **Scripts\start-application.bat**: Application launcher
echo.
echo Generated: %date% %time%
) > "%PROJECT_ROOT%\PROJECT-STRUCTURE.md"

echo [CREATE] Created PROJECT-STRUCTURE.md in project root

echo.
echo [INFO] Creating convenience scripts in project root...

REM Create a convenience script in project root that calls the moved scripts
(
echo @echo off
echo REM Convenience script to run system diagnostic from project root
echo echo [INFO] Running system diagnostic...
echo "%~dp0Scripts\diagnose-system.bat"
echo pause
) > "%PROJECT_ROOT%\diagnose.bat"

(
echo @echo off
echo REM Convenience script to setup database from project root
echo echo [INFO] Setting up clean database...
echo "%~dp0Scripts\setup-clean-database.bat"
echo pause
) > "%PROJECT_ROOT%\setup-db.bat"

(
echo @echo off
echo REM Convenience script to start application from project root
echo echo [INFO] Starting application...
echo "%~dp0Scripts\start-application.bat"
echo pause
) > "%PROJECT_ROOT%\start.bat"

echo [CREATE] Created convenience scripts in project root

echo.
echo [INFO] Updating project file to include new files...

REM Check if project file exists
if exist "%PROJECT_ROOT%\OpCentrix.csproj" (
    echo [FOUND] Project file exists
    
    REM Create backup of project file
    copy "%PROJECT_ROOT%\OpCentrix.csproj" "%PROJECT_ROOT%\OpCentrix.csproj.backup" >nul 2>&1
    echo [BACKUP] Created backup of project file
    
    echo [INFO] Project file will be updated by Visual Studio when solution is reloaded
) else (
    echo [ERROR] Project file not found at %PROJECT_ROOT%\OpCentrix.csproj
)

echo.
echo [SUMMARY] File Organization Complete
echo.
echo Project files organized into:
echo - Documentation\ folder: Implementation docs, fix guides, and analysis
echo - Scripts\ folder: Utility scripts and automation tools
echo - Convenience scripts in project root for easy access
echo.
echo Next Steps:
echo 1. Open Visual Studio
echo 2. Reload the solution to see new files
echo 3. Use convenience scripts: diagnose.bat, setup-db.bat, start.bat
echo 4. Review Documentation\ folder for implementation guides
echo.
echo [SUCCESS] OpCentrix project file organization completed successfully!

pause