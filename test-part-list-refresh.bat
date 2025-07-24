@echo off
cls
echo.
echo ========================================
echo   PART CREATION LIST REFRESH TEST
echo ========================================
echo.
echo This script will test the part creation
echo and ensure the new part appears in the
echo list immediately after saving.
echo.
echo WHAT TO TEST:
echo 1. Modal opens properly
echo 2. Form saves successfully
echo 3. Modal closes automatically
echo 4. New part appears in list immediately
echo.
echo PRESS ANY KEY TO START THE APPLICATION...
pause >nul

echo.
echo Starting OpCentrix application...
dotnet run --project OpCentrix

pause