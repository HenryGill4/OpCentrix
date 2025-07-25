@echo off
REM IMMEDIATE FIX - OpCentrix Windows Commands
REM Run this first to fix your current issues

echo ============================================================
echo  OPCENTRIX WINDOWS POWERSHELL FIXES
echo ============================================================
echo.

echo [ISSUE 1] PowerShell Batch File Execution
echo Problem: organize-project-files.bat was not found
echo Solution: Use .\ prefix in PowerShell
echo.
echo WRONG:   organize-project-files.bat
echo CORRECT: .\organize-project-files.bat
echo.

echo [ISSUE 2] PowerShell Command Chaining  
echo Problem: The token '&&' is not a valid statement separator
echo Solution: Use semicolon or separate commands
echo.
echo WRONG:   cd OpCentrix && dotnet build
echo CORRECT: cd OpCentrix; dotnet build
echo.

echo [IMMEDIATE COMMANDS FOR YOU TO RUN]
echo.
echo Option 1 - PowerShell Commands:
echo   .\organize-project-files.bat
echo   cd OpCentrix
echo   dotnet build
echo   dotnet run
echo.
echo Option 2 - Use Command Prompt instead:
echo   Press Windows+R, type 'cmd', press Enter
echo   cd /d "C:\Users\Henry\Source\Repos\OpCentrix"
echo   organize-project-files.bat
echo   cd OpCentrix
echo   dotnet build
echo   dotnet run
echo.
echo Option 3 - Use our quick build script:
echo   .\quick-build.bat
echo.

echo [TECHNICAL EXPLANATION]
echo PowerShell Security: By default, PowerShell requires .\ for local files
echo Bash vs PowerShell: && works in bash/cmd, but not PowerShell
echo File Paths: Windows uses backslashes, Linux uses forward slashes
echo.

echo [FILES CREATED TO HELP YOU]
echo - quick-build.bat        : Complete build and run script
echo - opcentrix.ps1          : PowerShell script with parameters  
echo - WINDOWS-POWERSHELL-FIXES.md : Detailed command reference
echo.

echo Ready to continue? Try one of the options above!
pause