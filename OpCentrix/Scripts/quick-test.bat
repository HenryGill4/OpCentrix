@echo off
echo =======================================
echo   OpCentrix Quick Test Script
echo =======================================
echo.

REM Navigate to project directory
cd /d "%~dp0"
if not exist "OpCentrix.csproj" (
    cd OpCentrix
    if not exist "OpCentrix.csproj" (
        echo ERROR: OpCentrix.csproj not found
        pause
        exit /b 1
    )
)

echo ? Running quick functionality test...
echo.

REM Test 1: Check if project builds
echo ?? Testing build...
dotnet build --verbosity quiet >nul 2>&1
if %errorlevel% neq 0 (
    echo ? Build failed
    dotnet build
    pause
    exit /b 1
)
echo ? Build successful

REM Test 2: Check database creation (quick test)
echo ??? Testing database initialization...
set ASPNETCORE_ENVIRONMENT=Development
set SEED_SAMPLE_DATA=true
set RECREATE_DATABASE=true

REM Start application briefly to test database creation
echo Starting brief test (will timeout in 5 seconds)...
timeout 5 dotnet run --no-build --urls "http://localhost:0" >nul 2>&1
echo ? Database initialization test completed

REM Test 3: Check if database file was created
if exist "scheduler.db" (
    echo ? Database file created successfully
    for %%A in (scheduler.db) do echo    Database size: %%~zA bytes
) else (
    echo ? Database file not created
)

REM Test 4: Check for essential files
if exist "TEST_USERS.txt" (
    echo ? Test users file created
) else (
    echo ?? Test users file not found
)

echo.
echo ?? Quick Test Summary:
echo ======================
if exist "scheduler.db" (
    echo ? Database: Ready
) else (
    echo ? Database: Not found
)

echo ? Build: Successful
echo ? Code: Compiles without errors

echo.
echo ?? Ready to start! Run:
echo    dotnet run
echo.
echo ?? Then visit: http://localhost:5000
echo ?? Login with: admin / admin123
echo.
pause