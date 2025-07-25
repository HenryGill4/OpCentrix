# OpCentrix Build and Run Script for PowerShell
# Compatible with Windows PowerShell and PowerShell Core

param(
    [switch]$Build,
    [switch]$Run,
    [switch]$Clean,
    [switch]$All
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Define paths
$WorkspaceRoot = "C:\Users\Henry\Source\Repos\OpCentrix"
$ProjectRoot = "$WorkspaceRoot\OpCentrix"
$ProjectFile = "$ProjectRoot\OpCentrix.csproj"

Write-Host "[INFO] OpCentrix PowerShell Build Script" -ForegroundColor Cyan
Write-Host "[INFO] Workspace: $WorkspaceRoot" -ForegroundColor Gray
Write-Host "[INFO] Project: $ProjectRoot" -ForegroundColor Gray

# Function to check if we're in the right directory
function Test-OpCentrixWorkspace {
    if (-not (Test-Path $WorkspaceRoot)) {
        Write-Host "[ERROR] Workspace directory not found: $WorkspaceRoot" -ForegroundColor Red
        return $false
    }
    
    if (-not (Test-Path $ProjectFile)) {
        Write-Host "[ERROR] Project file not found: $ProjectFile" -ForegroundColor Red
        return $false
    }
    
    Write-Host "[OK] OpCentrix workspace verified" -ForegroundColor Green
    return $true
}

# Function to organize project files
function Invoke-OrganizeFiles {
    Write-Host "[INFO] Organizing project files..." -ForegroundColor Yellow
    
    $OrgScript = "$WorkspaceRoot\organize-project-files.bat"
    if (Test-Path $OrgScript) {
        try {
            Set-Location $WorkspaceRoot
            & cmd.exe /c $OrgScript
            Write-Host "[OK] Project files organized" -ForegroundColor Green
        }
        catch {
            Write-Host "[WARNING] Could not run organize script: $($_.Exception.Message)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "[WARNING] Organize script not found: $OrgScript" -ForegroundColor Yellow
    }
}

# Function to clean the project
function Invoke-CleanProject {
    Write-Host "[INFO] Cleaning project..." -ForegroundColor Yellow
    
    try {
        Set-Location $ProjectRoot
        dotnet clean
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] Project cleaned successfully" -ForegroundColor Green
        } else {
            Write-Host "[ERROR] Project clean failed" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "[ERROR] Clean failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    
    return $true
}

# Function to build the project
function Invoke-BuildProject {
    Write-Host "[INFO] Building project..." -ForegroundColor Yellow
    
    try {
        Set-Location $ProjectRoot
        dotnet build --verbosity minimal
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] Build completed successfully" -ForegroundColor Green
        } else {
            Write-Host "[ERROR] Build failed" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "[ERROR] Build failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    
    return $true
}

# Function to run the project
function Invoke-RunProject {
    Write-Host "[INFO] Starting application..." -ForegroundColor Yellow
    
    try {
        Set-Location $ProjectRoot
        Write-Host "[INFO] Application will start at: https://localhost:5001" -ForegroundColor Cyan
        Write-Host "[INFO] Press Ctrl+C to stop the application" -ForegroundColor Cyan
        dotnet run
    }
    catch {
        Write-Host "[ERROR] Run failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    
    return $true
}

# Main execution
try {
    # Verify workspace
    if (-not (Test-OpCentrixWorkspace)) {
        exit 1
    }
    
    # Always organize files first
    Invoke-OrganizeFiles
    
    # Execute based on parameters
    if ($All) {
        Write-Host "[INFO] Running complete build and run sequence..." -ForegroundColor Cyan
        
        if (-not (Invoke-CleanProject)) { exit 1 }
        if (-not (Invoke-BuildProject)) { exit 1 }
        Invoke-RunProject
    }
    elseif ($Clean) {
        Invoke-CleanProject
    }
    elseif ($Build) {
        Invoke-BuildProject
    }
    elseif ($Run) {
        Invoke-RunProject
    }
    else {
        # Default: build and run
        Write-Host "[INFO] No specific action specified, building and running..." -ForegroundColor Cyan
        
        if (-not (Invoke-BuildProject)) { exit 1 }
        Invoke-RunProject
    }
}
catch {
    Write-Host "[ERROR] Script failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    # Return to original directory
    Set-Location $WorkspaceRoot
}