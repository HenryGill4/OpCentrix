@echo off
echo =====================================
echo OpCentrix Database Reset and Setup
echo =====================================
echo.

REM Check if OpCentrix directory exists
if not exist "OpCentrix" (
    echo ERROR: OpCentrix directory not found. Please run from repository root.
    pause
    exit /b 1
)

cd OpCentrix

echo Step 1: Checking database directory...
if not exist "Data" (
    mkdir Data
    echo INFO: Created Data directory
)

echo Step 2: Removing existing database...
if exist "Data\OpCentrix.db" (
    del "Data\OpCentrix.db"
    echo SUCCESS: Removed existing database
) else (
    echo INFO: No existing database found
)

echo Step 3: Removing any existing migration files...
if exist "Migrations" (
    rmdir /s /q "Migrations"
    echo INFO: Removed existing migrations
)

echo Step 4: Building project...
dotnet build >nul 2>&1
if %errorlevel% equ 0 (
    echo SUCCESS: Project built successfully
) else (
    echo ERROR: Build failed
    pause
    exit /b 1
)

echo Step 5: Creating fresh database...
echo INFO: Starting application to initialize database...
start /min cmd /c "dotnet run --no-build > database-init.log 2>&1"

REM Wait for database creation
echo INFO: Waiting for database initialization...
timeout /t 15 /nobreak >nul

REM Stop the application
taskkill /f /im dotnet.exe >nul 2>&1

echo Step 6: Verifying database creation...
if exist "Data\OpCentrix.db" (
    echo SUCCESS: Database created successfully
    
    REM Check database content if sqlite3 is available
    where sqlite3 >nul 2>&1
    if %errorlevel% equ 0 (
        echo Step 7: Verifying database content...
        for /f %%i in ('sqlite3 Data\OpCentrix.db "SELECT COUNT(*) FROM Users;" 2^>nul') do set USER_COUNT=%%i
        for /f %%i in ('sqlite3 Data\OpCentrix.db "SELECT COUNT(*) FROM Parts;" 2^>nul') do set PART_COUNT=%%i
        
        if defined USER_COUNT if defined PART_COUNT (
            echo SUCCESS: Database contains %USER_COUNT% users and %PART_COUNT% parts
        )
        
        echo.
        echo Sample users created:
        sqlite3 -header -column Data\OpCentrix.db "SELECT Username, Role, FullName FROM Users LIMIT 5;" 2>nul
    ) else (
        echo INFO: sqlite3 not available - cannot verify database content
    )
) else (
    echo ERROR: Database was not created
    echo Check database-init.log for details:
    type database-init.log 2>nul
    pause
    exit /b 1
)

REM Clean up log file
if exist database-init.log del database-init.log

echo.
echo =====================================
echo DATABASE SETUP COMPLETE!
echo =====================================
echo.
echo The database has been created and seeded with sample data.
echo You can now start the application with: dotnet run
echo.
echo Login credentials:
echo - admin / admin123 (Administrator)
echo - manager / manager123 (Manager)
echo - scheduler / scheduler123 (Scheduler)
echo.

cd ..
pause