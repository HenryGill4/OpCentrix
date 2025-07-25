# OpCentrix Windows PowerShell Quick Commands

## IMMEDIATE FIXES FOR WINDOWS POWERSHELL ISSUES

### Issue 1: Running Batch Files in PowerShell
**Problem**: PowerShell doesn't run commands from current directory by default
**Solution**: Use `.\` prefix for batch files

```powershell
# WRONG (doesn't work)
organize-project-files.bat

# CORRECT (works)
.\organize-project-files.bat
```

### Issue 2: PowerShell Command Chaining
**Problem**: PowerShell doesn't support `&&` operator like bash
**Solution**: Use PowerShell syntax instead

```powershell
# WRONG (bash syntax - doesn't work in PowerShell)
cd OpCentrix && dotnet build

# CORRECT (PowerShell syntax - works)
cd OpCentrix; dotnet build

# OR BETTER (PowerShell with error handling)
cd OpCentrix; if ($?) { dotnet build }
```

## QUICK COMMAND FIXES

### 1. Run Project Organization Script
```powershell
# From workspace root (C:\Users\Henry\Source\Repos\OpCentrix)
.\organize-project-files.bat
```

### 2. Build Project (PowerShell Compatible)
```powershell
# Method 1: Simple sequential commands
cd OpCentrix; dotnet build

# Method 2: With error checking
cd OpCentrix; if ($LASTEXITCODE -eq 0) { dotnet build }

# Method 3: One-liner with full path (most reliable)
dotnet build "OpCentrix\OpCentrix.csproj"
```

### 3. Run Project (PowerShell Compatible)
```powershell
# Method 1: Simple
cd OpCentrix; dotnet run

# Method 2: With full path (most reliable)
dotnet run --project "OpCentrix\OpCentrix.csproj"
```

## ALTERNATIVE: USE COMMAND PROMPT INSTEAD

If PowerShell continues to cause issues, use Command Prompt (cmd.exe):

### Open Command Prompt
1. Press `Windows + R`
2. Type `cmd`
3. Press Enter
4. Navigate to your workspace: `cd /d "C:\Users\Henry\Source\Repos\OpCentrix"`

### Command Prompt Commands (These work perfectly)
```cmd
REM Run organization script
organize-project-files.bat

REM Build and run with && operator (works in cmd)
cd OpCentrix && dotnet build && dotnet run

REM Or step by step
cd OpCentrix
dotnet build
dotnet run
```

## RECOMMENDED WORKFLOW FOR WINDOWS

### Step 1: Organize Project Files
```powershell
# In PowerShell (from workspace root)
.\organize-project-files.bat
```

### Step 2: Build Project
```powershell
# Option A: Use full path (most reliable)
dotnet build "OpCentrix\OpCentrix.csproj"

# Option B: Change directory first
cd OpCentrix
dotnet build
```

### Step 3: Run Project
```powershell
# Option A: Use full path
dotnet run --project "OpCentrix\OpCentrix.csproj"

# Option B: From project directory
cd OpCentrix
dotnet run
```

## DIAGNOSTIC COMMANDS (Windows Compatible)

### Check Current Location
```powershell
Get-Location
# Should show: C:\Users\Henry\Source\Repos\OpCentrix
```

### List Files in Current Directory
```powershell
Get-ChildItem *.bat
# Should show: organize-project-files.bat and other .bat files
```

### Check if Project Exists
```powershell
Test-Path "OpCentrix\OpCentrix.csproj"
# Should return: True
```

### Check .NET Version
```powershell
dotnet --version
# Should show: 8.x.x
```

## WINDOWS-SPECIFIC BATCH SCRIPTS

### Create a PowerShell-Friendly Build Script
Create `build.ps1`:
```powershell
# OpCentrix Build Script for PowerShell
Set-Location "OpCentrix"
dotnet build
if ($LASTEXITCODE -eq 0) {
    Write-Host "[SUCCESS] Build completed successfully!" -ForegroundColor Green
} else {
    Write-Host "[ERROR] Build failed!" -ForegroundColor Red
}
```

### Create a Command Prompt Build Script
Create `build.bat`:
```cmd
@echo off
echo [INFO] Building OpCentrix project...
cd OpCentrix
dotnet build
if %errorlevel% equ 0 (
    echo [SUCCESS] Build completed successfully!
) else (
    echo [ERROR] Build failed!
)
pause
```

## IMMEDIATE ACTION ITEMS

### Right Now - Run These Commands:
```powershell
# 1. Organize project files
.\organize-project-files.bat

# 2. Build the project
dotnet build "OpCentrix\OpCentrix.csproj"

# 3. If build succeeds, run the project
dotnet run --project "OpCentrix\OpCentrix.csproj"
```

### If That Doesn't Work - Use Command Prompt:
```cmd
REM Open new Command Prompt window and run:
cd /d "C:\Users\Henry\Source\Repos\OpCentrix"
organize-project-files.bat
cd OpCentrix
dotnet build
dotnet run
```

## NO UNICODE CHARACTERS - ALL ASCII

This guide uses only standard ASCII characters as required:
- [OK] instead of checkmarks
- [ERROR] instead of X marks  
- [SUCCESS] instead of other symbols
- [INFO] for information messages

All commands are Windows PowerShell and Command Prompt compatible with no special characters or symbols.