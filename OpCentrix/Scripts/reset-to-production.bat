@echo off
echo ===================================
echo   OpCentrix Production Reset
echo ===================================
echo.
echo This will:
echo - Remove all sample/test data
echo - Keep essential users and machines
echo - Optimize for production use
echo.

set /p confirm="Continue with production reset? (y/N): "
if /i not "%confirm%"=="y" (
    echo Operation cancelled.
    pause
    exit /b 0
)

REM Navigate to project directory
cd /d "%~dp0"
if not exist "OpCentrix.csproj" (
    cd OpCentrix
)

echo ?? Resetting to production configuration...

REM Set production environment
set ASPNETCORE_ENVIRONMENT=Production
set SEED_SAMPLE_DATA=false
set RECREATE_DATABASE=true

REM Clean database
if exist "scheduler.db" del /q "scheduler.db"
if exist "scheduler.db-shm" del /q "scheduler.db-shm"
if exist "scheduler.db-wal" del /q "scheduler.db-wal"

echo ?? Creating production database...
dotnet run --no-build --urls "http://localhost:0" -- --seed-only 2>nul

REM Create production users file
(
echo OpCentrix Production Users
echo =========================
echo.
echo DEFAULT ADMIN ACCOUNT:
echo - Username: admin
echo - Password: admin123
echo - Role: Admin
echo.
echo IMPORTANT: Change default passwords before production use!
echo.
echo Production Features Enabled:
echo - No sample data
echo - Optimized logging
echo - Essential users only
echo - Ready for real data
echo.
echo Login URL: http://localhost:5000/Account/Login
) > PRODUCTION_USERS.txt

echo ? Production reset completed!
echo ?? See PRODUCTION_USERS.txt for login details
echo ?? Remember to change default passwords!
echo.
pause