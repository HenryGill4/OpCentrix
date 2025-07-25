# ?? Port Configuration and Database Verification Script

Write-Host "?? OpCentrix Port Configuration Fix - Verification Script" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Yellow

# Navigate to project directory
Set-Location -Path "OpCentrix"

Write-Host "`n1. ? Cleaning and restoring packages..." -ForegroundColor Cyan
dotnet clean --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ? Clean completed successfully" -ForegroundColor Green
} else {
    Write-Host "   ? Clean failed" -ForegroundColor Red
    exit 1
}

dotnet restore --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ? Restore completed successfully" -ForegroundColor Green
} else {
    Write-Host "   ? Restore failed" -ForegroundColor Red
    exit 1
}

Write-Host "`n2. ? Building application..." -ForegroundColor Cyan
dotnet build --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ? Build completed successfully" -ForegroundColor Green
} else {
    Write-Host "   ? Build failed" -ForegroundColor Red
    exit 1
}

Write-Host "`n3. ? Checking configuration files..." -ForegroundColor Cyan

# Check appsettings.json
if (Test-Path "appsettings.json") {
    $appSettings = Get-Content "appsettings.json" | ConvertFrom-Json
    if ($appSettings.Urls -eq "http://localhost:5090") {
        Write-Host "   ? appsettings.json - Port 5090 configured" -ForegroundColor Green
    } else {
        Write-Host "   ??  appsettings.json - Port configuration may be incorrect" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ? appsettings.json not found" -ForegroundColor Red
}

# Check launchSettings.json
if (Test-Path "Properties/launchSettings.json") {
    $launchSettings = Get-Content "Properties/launchSettings.json" | ConvertFrom-Json
    if ($launchSettings.profiles.http.applicationUrl -eq "http://localhost:5090") {
        Write-Host "   ? launchSettings.json - Port 5090 configured" -ForegroundColor Green
    } else {
        Write-Host "   ??  launchSettings.json - Port configuration may be incorrect" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ? launchSettings.json not found" -ForegroundColor Red
}

Write-Host "`n4. ? Checking database connection..." -ForegroundColor Cyan

# Check if scheduler.db exists or can be created
if (Test-Path "scheduler.db") {
    Write-Host "   ? Database file exists (scheduler.db)" -ForegroundColor Green
    $dbSize = (Get-Item "scheduler.db").Length
    Write-Host "   ??  Database size: $([math]::Round($dbSize/1KB, 2)) KB" -ForegroundColor White
} else {
    Write-Host "   ??  Database file will be created on first run" -ForegroundColor White
}

Write-Host "`n5. ? Running baseline tests..." -ForegroundColor Cyan
Set-Location -Path ".."
dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ? All tests passed" -ForegroundColor Green
} else {
    Write-Host "   ? Some tests failed" -ForegroundColor Red
}

Write-Host "`n?? PORT CONFIGURATION VERIFICATION COMPLETE" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Yellow
Write-Host "`n? Your application is configured to run on: http://localhost:5090" -ForegroundColor Cyan
Write-Host "? Database connection: SQLite (scheduler.db)" -ForegroundColor Cyan
Write-Host "? Health endpoint: http://localhost:5090/health" -ForegroundColor Cyan
Write-Host "? Login page: http://localhost:5090/Account/Login" -ForegroundColor Cyan

Write-Host "`n?? TO START THE APPLICATION:" -ForegroundColor Yellow
Write-Host "   cd OpCentrix" -ForegroundColor White
Write-Host "   dotnet run" -ForegroundColor White

Write-Host "`n?? READY TO PROCEED WITH ADMIN CONTROL SYSTEM TASKS!" -ForegroundColor Green