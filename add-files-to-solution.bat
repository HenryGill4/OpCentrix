@echo off
echo.
echo ===================================================================
echo  ADDING ALL FILES TO SOLUTION - Windows
echo ===================================================================
echo.

set "WORKSPACE_PATH=C:\Users\Henry\Source\Repos\OpCentrix"
set "SOLUTION_FILE=%WORKSPACE_PATH%\OpCentrix.sln"

echo Workspace: %WORKSPACE_PATH%
echo Solution: %SOLUTION_FILE%
echo.

REM Navigate to workspace
cd /d "%WORKSPACE_PATH%"

echo [1/3] Checking solution file...
if exist "%SOLUTION_FILE%" (
    echo [OK] Solution file found: %SOLUTION_FILE%
) else (
    echo [ERROR] Solution file not found: %SOLUTION_FILE%
    pause
    exit /b 1
)

echo.
echo [2/3] Creating backup of current solution...
copy "%SOLUTION_FILE%" "%SOLUTION_FILE%.backup"
echo [OK] Backup created: %SOLUTION_FILE%.backup

echo.
echo [3/3] Updating solution file with all diagnostic and setup files...

REM Create new solution content
(
echo Microsoft Visual Studio Solution File, Format Version 12.00
echo # Visual Studio Version 17
echo VisualStudioVersion = 17.9.34728.123
echo MinimumVisualStudioVersion = 10.0.40219.1
echo Project^("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}"^) = "OpCentrix", "OpCentrix\OpCentrix.csproj", "{F7264BD9-2623-4258-9C3A-8BDD7C4453AD}"
echo EndProject
echo Project^("{2150E333-8FDC-42A3-9474-1A3956D46DE8}"^) = "Solution Items", "Solution Items", "{A1234567-1234-1234-1234-123456789ABC}"
echo 	ProjectSection^(SolutionItems^) = preProject
echo 		.env.example = .env.example
echo 		AI-INSTRUCTIONS-JQUERY-FIX.md = AI-INSTRUCTIONS-JQUERY-FIX.md
echo 		AI-INSTRUCTIONS-NO-UNICODE.md = AI-INSTRUCTIONS-NO-UNICODE.md
echo 		DATABASE-LOGIC-ANALYSIS.md = DATABASE-LOGIC-ANALYSIS.md
echo 		DATABASE-LOGIC-FIXES-COMPLETE.md = DATABASE-LOGIC-FIXES-COMPLETE.md
echo 		DATABASE_SETUP.md = DATABASE_SETUP.md
echo 		JQUERY-VALIDATION-FIX-COMPLETE.md = JQUERY-VALIDATION-FIX-COMPLETE.md
echo 		PARTS-TROUBLESHOOTING-GUIDE.md = PARTS-TROUBLESHOOTING-GUIDE.md
echo 		README.md = README.md
echo 		SETUP_COMPLETE.md = SETUP_COMPLETE.md
echo 		UNICODE-CLEANUP-COMPLETE.md = UNICODE-CLEANUP-COMPLETE.md
echo 	EndProjectSection
echo EndProject
echo Project^("{2150E333-8FDC-42A3-9474-1A3956D46DE8}"^) = "Windows Scripts", "Windows Scripts", "{B2345678-2345-2345-2345-234567890BCD}"
echo 	ProjectSection^(SolutionItems^) = preProject
echo 		add-files-to-solution.bat = add-files-to-solution.bat
echo 		diagnose-system.bat = diagnose-system.bat
echo 		fix-jquery-validation.bat = fix-jquery-validation.bat
echo 		quick-test.bat = quick-test.bat
echo 		reset-to-production.bat = reset-to-production.bat
echo 		setup-clean-database.bat = setup-clean-database.bat
echo 		setup-database.bat = setup-database.bat
echo 		start-application.bat = start-application.bat
echo 		test-complete-system.bat = test-complete-system.bat
echo 		verify-final-system.bat = verify-final-system.bat
echo 		verify-parts-database.bat = verify-parts-database.bat
echo 		verify-setup.bat = verify-setup.bat
echo 	EndProjectSection
echo EndProject
echo Project^("{2150E333-8FDC-42A3-9474-1A3956D46DE8}"^) = "Linux Scripts", "Linux Scripts", "{C3456789-3456-3456-3456-3456789ABCDE}"
echo 	ProjectSection^(SolutionItems^) = preProject
echo 		fix-jquery-validation.sh = fix-jquery-validation.sh
echo 		quick-test.sh = quick-test.sh
echo 		reset-to-production.sh = reset-to-production.sh
echo 		setup-clean-database.sh = setup-clean-database.sh
echo 		setup-database.sh = setup-database.sh
echo 		test-complete-system.sh = test-complete-system.sh
echo 		verify-final-system.sh = verify-final-system.sh
echo 		verify-parts-database.sh = verify-parts-database.sh
echo 		verify-setup.sh = verify-setup.sh
echo 	EndProjectSection
echo EndProject
echo Global
echo 	GlobalSection^(SolutionConfigurationPlatforms^) = preSolution
echo 		Debug^|Any CPU = Debug^|Any CPU
echo 		Release^|Any CPU = Release^|Any CPU
echo 	EndGlobalSection
echo 	GlobalSection^(ProjectConfigurationPlatforms^) = postSolution
echo 		{F7264BD9-2623-4258-9C3A-8BDD7C4453AD}.Debug^|Any CPU.ActiveCfg = Debug^|Any CPU
echo 		{F7264BD9-2623-4258-9C3A-8BDD7C4453AD}.Debug^|Any CPU.Build.0 = Debug^|Any CPU
echo 		{F7264BD9-2623-4258-9C3A-8BDD7C4453AD}.Release^|Any CPU.ActiveCfg = Release^|Any CPU
echo 		{F7264BD9-2623-4258-9C3A-8BDD7C4453AD}.Release^|Any CPU.Build.0 = Release^|Any CPU
echo 	EndGlobalSection
echo 	GlobalSection^(SolutionProperties^) = preSolution
echo 		HideSolutionNode = FALSE
echo 	EndGlobalSection
echo 	GlobalSection^(ExtensibilityGlobals^) = postSolution
echo 		SolutionGuid = {05A529AD-B9D0-4B03-A935-23E424E31B44}
echo 	EndGlobalSection
echo EndGlobal
) > "%SOLUTION_FILE%"

echo [OK] Solution file updated successfully!
echo.
echo ===================================================================
echo  SOLUTION UPDATE COMPLETE
echo ===================================================================
echo.
echo The solution file has been updated to include:
echo.
echo SOLUTION FOLDERS CREATED:
echo 1. Solution Items - Documentation and configuration files
echo    - All .md files (AI instructions, troubleshooting guides, etc.)
echo    - .env.example file
echo.
echo 2. Windows Scripts - All .bat files for Windows
echo    - diagnose-system.bat
echo    - fix-jquery-validation.bat
echo    - setup-clean-database.bat
echo    - All other Windows batch scripts
echo.
echo 3. Linux Scripts - All .sh files for Linux/Mac
echo    - All shell script equivalents
echo.
echo NEXT STEPS:
echo 1. Close and reopen Visual Studio
echo 2. Open the solution: %SOLUTION_FILE%
echo 3. All files should now be visible in Solution Explorer
echo 4. Use full paths when running commands from scripts
echo.
echo BACKUP AVAILABLE:
echo Original solution backed up to: %SOLUTION_FILE%.backup
echo.
pause