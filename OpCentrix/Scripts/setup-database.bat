@echo off
echo =========================================
echo     OpCentrix Database Setup Script
echo =========================================
echo.

REM Check if .NET is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET 8.0 is not installed or not in PATH
    echo Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo ? .NET SDK detected
echo.

REM Navigate to project directory
cd /d "%~dp0"
if not exist "OpCentrix.csproj" (
    cd OpCentrix
    if not exist "OpCentrix.csproj" (
        echo ERROR: OpCentrix.csproj not found
        echo Please run this script from the OpCentrix root directory
        pause
        exit /b 1
    )
)

echo ? Project directory found
echo.

REM Restore NuGet packages
echo ?? Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)
echo ? Packages restored successfully
echo.

REM Clean any existing database
echo ??? Cleaning existing database...
if exist "scheduler.db" del /q "scheduler.db"
if exist "scheduler.db-shm" del /q "scheduler.db-shm"
if exist "scheduler.db-wal" del /q "scheduler.db-wal"
echo ? Database cleaned
echo.

REM Build the project
echo ?? Building project...
dotnet build
if %errorlevel% neq 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo ? Build successful
echo.

REM Set environment variables for full database seeding
echo ?? Setting up environment for database seeding...
set ASPNETCORE_ENVIRONMENT=Development
set SEED_SAMPLE_DATA=true
set RECREATE_DATABASE=true

REM Create database and seed data
echo ??? Creating and seeding database...
echo This may take a few moments...
dotnet run --no-build --urls "http://localhost:0" -- --seed-only 2>nul
if %errorlevel% neq 0 (
    echo WARNING: Database seeding may have had issues, but continuing...
)

REM Create test users file for reference
echo ?? Creating test users reference file...
(
echo OpCentrix Test Users
echo ==================
echo.
echo ADMIN USERS:
echo - Username: admin      ^| Password: admin123      ^| Role: Admin
echo - Username: manager    ^| Password: manager123    ^| Role: Manager
echo.
echo PRODUCTION STAFF:
echo - Username: scheduler  ^| Password: scheduler123  ^| Role: Scheduler
echo - Username: operator   ^| Password: operator123   ^| Role: Operator
echo - Username: printer    ^| Password: printer123    ^| Role: PrintingSpecialist
echo.
echo SPECIALISTS:
echo - Username: coating    ^| Password: coating123    ^| Role: CoatingSpecialist
echo - Username: edm        ^| Password: edm123        ^| Role: EDMSpecialist
echo - Username: machining  ^| Password: machining123  ^| Role: MachiningSpecialist
echo - Username: qc         ^| Password: qc123         ^| Role: QCSpecialist
echo - Username: shipping   ^| Password: shipping123   ^| Role: ShippingSpecialist
echo - Username: media      ^| Password: media123      ^| Role: MediaSpecialist
echo - Username: analyst    ^| Password: analyst123    ^| Role: Analyst
echo.
echo LOGIN URL: http://localhost:5000/Account/Login
echo ADMIN URL: http://localhost:5000/Admin
echo SCHEDULER URL: http://localhost:5000/Scheduler
echo.
echo Database Location: %cd%\scheduler.db
echo Database Type: SQLite
echo.
echo To reset database: Run setup-database.bat again
echo To start application: Run dotnet run or start-application.bat
) > TEST_USERS.txt

echo ? Database setup completed successfully!
echo.
echo ?? Test user accounts have been created (see TEST_USERS.txt)
echo ?? You can now start the application with: dotnet run
echo ?? Login URL: http://localhost:5000/Account/Login
echo ?? Quick start: admin / admin123
echo.
echo Press any key to exit...
pause >nul