@echo off
echo ==================================================
echo FIXING SCHEDULER SETTINGS DATABASE MIGRATION ISSUE
echo ==================================================

cd /d "%~dp0OpCentrix"

echo.
echo [1/4] Stopping any running processes...
taskkill /f /im dotnet.exe 2>nul >nul

echo [2/4] Checking current migration state...
dotnet ef migrations list

echo.
echo [3/4] Creating manual SchedulerSettings table fix...

REM Create the SQL script to manually add the SchedulerSettings table
(
echo -- Fix for SchedulerSettings table creation
echo.
echo -- Drop table if exists ^(in case of partial creation^)
echo DROP TABLE IF EXISTS "SchedulerSettings";
echo.
echo -- Create SchedulerSettings table manually
echo CREATE TABLE "SchedulerSettings" ^(
echo     "Id" INTEGER NOT NULL CONSTRAINT "PK_SchedulerSettings" PRIMARY KEY AUTOINCREMENT,
echo     "TitaniumToTitaniumChangeoverMinutes" INTEGER NOT NULL DEFAULT 30,
echo     "InconelToInconelChangeoverMinutes" INTEGER NOT NULL DEFAULT 45,
echo     "CrossMaterialChangeoverMinutes" INTEGER NOT NULL DEFAULT 120,
echo     "DefaultMaterialChangeoverMinutes" INTEGER NOT NULL DEFAULT 60,
echo     "DefaultPreheatingTimeMinutes" INTEGER NOT NULL DEFAULT 60,
echo     "DefaultCoolingTimeMinutes" INTEGER NOT NULL DEFAULT 240,
echo     "DefaultPostProcessingTimeMinutes" INTEGER NOT NULL DEFAULT 90,
echo     "SetupTimeBufferMinutes" INTEGER NOT NULL DEFAULT 30,
echo     "StandardShiftStart" TEXT NOT NULL DEFAULT '07:00:00',
echo     "StandardShiftEnd" TEXT NOT NULL DEFAULT '15:00:00',
echo     "EveningShiftStart" TEXT NOT NULL DEFAULT '15:00:00',
echo     "EveningShiftEnd" TEXT NOT NULL DEFAULT '23:00:00',
echo     "NightShiftStart" TEXT NOT NULL DEFAULT '23:00:00',
echo     "NightShiftEnd" TEXT NOT NULL DEFAULT '07:00:00',
echo     "EnableWeekendOperations" INTEGER NOT NULL DEFAULT 0,
echo     "SaturdayOperations" INTEGER NOT NULL DEFAULT 0,
echo     "SundayOperations" INTEGER NOT NULL DEFAULT 0,
echo     "TI1MachinePriority" INTEGER NOT NULL DEFAULT 5,
echo     "TI2MachinePriority" INTEGER NOT NULL DEFAULT 5,
echo     "INCMachinePriority" INTEGER NOT NULL DEFAULT 5,
echo     "AllowConcurrentJobs" INTEGER NOT NULL DEFAULT 1,
echo     "MaxJobsPerMachinePerDay" INTEGER NOT NULL DEFAULT 8,
echo     "RequiredOperatorCertification" TEXT NOT NULL DEFAULT 'SLS Basic',
echo     "QualityCheckRequired" INTEGER NOT NULL DEFAULT 1,
echo     "MinimumTimeBetweenJobsMinutes" INTEGER NOT NULL DEFAULT 15,
echo     "EmergencyOverrideEnabled" INTEGER NOT NULL DEFAULT 1,
echo     "NotifyOnScheduleConflicts" INTEGER NOT NULL DEFAULT 1,
echo     "NotifyOnMaterialChanges" INTEGER NOT NULL DEFAULT 1,
echo     "AdvanceWarningTimeMinutes" INTEGER NOT NULL DEFAULT 60,
echo     "CreatedDate" TEXT NOT NULL DEFAULT ^(datetime^('now'^)^),
echo     "LastModifiedDate" TEXT NOT NULL DEFAULT ^(datetime^('now'^)^),
echo     "CreatedBy" TEXT NOT NULL DEFAULT 'System',
echo     "LastModifiedBy" TEXT NOT NULL DEFAULT 'System',
echo     "ChangeNotes" TEXT NOT NULL DEFAULT 'Default settings initialization'
echo ^);
echo.
echo -- Create index
echo CREATE UNIQUE INDEX "IX_SchedulerSettings_Id" ON "SchedulerSettings" ^("Id"^);
echo.
echo -- Insert default settings record
echo INSERT INTO "SchedulerSettings" ^(
echo     "TitaniumToTitaniumChangeoverMinutes",
echo     "InconelToInconelChangeoverMinutes", 
echo     "CrossMaterialChangeoverMinutes",
echo     "DefaultMaterialChangeoverMinutes",
echo     "DefaultPreheatingTimeMinutes",
echo     "DefaultCoolingTimeMinutes",
echo     "DefaultPostProcessingTimeMinutes",
echo     "SetupTimeBufferMinutes",
echo     "StandardShiftStart",
echo     "StandardShiftEnd",
echo     "EveningShiftStart", 
echo     "EveningShiftEnd",
echo     "NightShiftStart",
echo     "NightShiftEnd",
echo     "EnableWeekendOperations",
echo     "SaturdayOperations",
echo     "SundayOperations",
echo     "TI1MachinePriority",
echo     "TI2MachinePriority",
echo     "INCMachinePriority",
echo     "AllowConcurrentJobs",
echo     "MaxJobsPerMachinePerDay",
echo     "RequiredOperatorCertification",
echo     "QualityCheckRequired",
echo     "MinimumTimeBetweenJobsMinutes",
echo     "EmergencyOverrideEnabled",
echo     "NotifyOnScheduleConflicts",
echo     "NotifyOnMaterialChanges",
echo     "AdvanceWarningTimeMinutes",
echo     "CreatedBy",
echo     "LastModifiedBy",
echo     "ChangeNotes"
echo ^) VALUES ^(
echo     30, 45, 120, 60, 60, 240, 90, 30,
echo     '07:00:00', '15:00:00', '15:00:00', '23:00:00', '23:00:00', '07:00:00',
echo     0, 0, 0, 5, 5, 5, 1, 8,
echo     'SLS Basic', 1, 15, 1, 1, 1, 60,
echo     'System', 'System', 'Database fix - manual table creation'
echo ^);
) > fix_scheduler_settings.sql

echo [4/4] Applying the SQL fix to database...
sqlite3 opcentrix.db < fix_scheduler_settings.sql

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ================================
    echo ? SUCCESS: SchedulerSettings table created successfully!
    echo ================================
    echo.
    echo The application should now work properly without migration errors.
    echo You can test by running: dotnet run
    echo.
) else (
    echo.
    echo ================================
    echo ? ERROR: Failed to create SchedulerSettings table
    echo ================================
    echo.
    echo Please check the error messages above.
    echo You may need to delete the database and recreate it completely.
    echo.
)

echo Cleaning up temporary files...
del fix_scheduler_settings.sql 2>nul

echo.
echo Press any key to continue...
pause >nul