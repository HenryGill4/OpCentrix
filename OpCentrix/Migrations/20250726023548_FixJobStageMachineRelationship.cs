using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class FixJobStageMachineRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildJobParts_BuildJobs_BuildId",
                table: "BuildJobParts");

            migrationBuilder.DropForeignKey(
                name: "FK_BuildJobs_Users_UserId",
                table: "BuildJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_DelayLogs_BuildJobs_BuildId",
                table: "DelayLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Parts_PartId",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_MachineCapabilities_SlsMachines_SlsMachineId",
                table: "MachineCapabilities");

            migrationBuilder.DropTable(
                name: "MachineDataSnapshot");

            migrationBuilder.DropTable(
                name: "SlsMachines");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Parts_Industry",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_IsActive",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_Material",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_PartCategory",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_PartClass",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_ProcessType",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_RequiredMachineType",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_SlsMaterial",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_CustomerOrderNumber",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_MachineId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_OpcUaJobId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_ScheduledStart",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_SlsMaterial",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_JobLogEntries_Action",
                table: "JobLogEntries");

            migrationBuilder.DropIndex(
                name: "IX_JobLogEntries_MachineId",
                table: "JobLogEntries");

            migrationBuilder.DropIndex(
                name: "IX_JobLogEntries_Timestamp",
                table: "JobLogEntries");

            migrationBuilder.DropIndex(
                name: "IX_DelayLogs_BuildId",
                table: "DelayLogs");

            migrationBuilder.DropIndex(
                name: "IX_DelayLogs_CreatedAt",
                table: "DelayLogs");

            migrationBuilder.DropIndex(
                name: "IX_DelayLogs_DelayDuration",
                table: "DelayLogs");

            migrationBuilder.DropIndex(
                name: "IX_DelayLogs_DelayReason",
                table: "DelayLogs");

            migrationBuilder.DropIndex(
                name: "IX_BuildJobs_ActualStartTime",
                table: "BuildJobs");

            migrationBuilder.DropIndex(
                name: "IX_BuildJobs_AssociatedScheduledJobId",
                table: "BuildJobs");

            migrationBuilder.DropIndex(
                name: "IX_BuildJobs_PrinterName",
                table: "BuildJobs");

            migrationBuilder.DropIndex(
                name: "IX_BuildJobs_PrinterName_ActualStartTime",
                table: "BuildJobs");

            migrationBuilder.DropIndex(
                name: "IX_BuildJobs_Status",
                table: "BuildJobs");

            migrationBuilder.DropIndex(
                name: "IX_BuildJobParts_BuildId",
                table: "BuildJobParts");

            migrationBuilder.DropIndex(
                name: "IX_BuildJobParts_BuildId_IsPrimary",
                table: "BuildJobParts");

            migrationBuilder.DropIndex(
                name: "IX_BuildJobParts_IsPrimary",
                table: "BuildJobParts");

            migrationBuilder.DropIndex(
                name: "IX_BuildJobParts_PartNumber",
                table: "BuildJobParts");

            migrationBuilder.RenameColumn(
                name: "SlsMachineId",
                table: "MachineCapabilities",
                newName: "MachineId");

            migrationBuilder.RenameIndex(
                name: "IX_MachineCapabilities_SlsMachineId_CapabilityType",
                table: "MachineCapabilities",
                newName: "IX_MachineCapabilities_MachineId_CapabilityType");

            migrationBuilder.RenameIndex(
                name: "IX_MachineCapabilities_SlsMachineId",
                table: "MachineCapabilities",
                newName: "IX_MachineCapabilities_MachineId");

            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                table: "UserSettings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "UTC");

            migrationBuilder.AlterColumn<string>(
                name: "Theme",
                table: "UserSettings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20,
                oldDefaultValue: "Light");

            migrationBuilder.AlterColumn<int>(
                name: "SessionTimeoutMinutes",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 120);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedDate",
                table: "UserSettings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "datetime('now')");

            migrationBuilder.AlterColumn<int>(
                name: "ItemsPerPage",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 20);

            migrationBuilder.AlterColumn<bool>(
                name: "EmailNotifications",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "DefaultPage",
                table: "UserSettings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "/Scheduler");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "UserSettings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "datetime('now')");

            migrationBuilder.AlterColumn<bool>(
                name: "BrowserNotifications",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "datetime('now')");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "datetime('now')");

            migrationBuilder.AlterColumn<string>(
                name: "SlsMaterial",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "Ti-6Al-4V Grade 5");

            migrationBuilder.AlterColumn<string>(
                name: "RequiredMachineType",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "TruPrint 3000");

            migrationBuilder.AlterColumn<string>(
                name: "QualityCheckpoints",
                table: "Parts",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000,
                oldDefaultValue: "{}");

            migrationBuilder.AlterColumn<string>(
                name: "ProcessType",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "SLS Metal");

            migrationBuilder.AlterColumn<string>(
                name: "ProcessParameters",
                table: "Parts",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000,
                oldDefaultValue: "{}");

            migrationBuilder.AlterColumn<string>(
                name: "PreferredMachines",
                table: "Parts",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldDefaultValue: "TI1,TI2");

            migrationBuilder.AlterColumn<string>(
                name: "PowderSpecification",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "15-45 μm particle size");

            migrationBuilder.AlterColumn<string>(
                name: "PartNumber",
                table: "Parts",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Material",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "Ti-6Al-4V Grade 5");

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "System");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<double>(
                name: "EstimatedHours",
                table: "Parts",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldDefaultValue: 8.0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "System");

            migrationBuilder.AlterColumn<string>(
                name: "AvgDuration",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "8h");

            migrationBuilder.AddColumn<double>(
                name: "AdminEstimatedHoursOverride",
                table: "Parts",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminOverrideBy",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AdminOverrideDate",
                table: "Parts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminOverrideReason",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MachineId1",
                table: "MachineCapabilities",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Supervisor",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "Scheduled");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialInstructions",
                table: "Jobs",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "SlsMaterial",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "Ti-6Al-4V Grade 5");

            migrationBuilder.AlterColumn<double>(
                name: "ScanSpeedMmPerSec",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 1000.0,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldDefaultValue: 1200.0);

            migrationBuilder.AlterColumn<bool>(
                name: "RequiresPreheating",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "RequiresPowderSieving",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "RequiresArgonPurge",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "RequiredTooling",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldDefaultValue: "Build Platform,Powder Sieve,Inert Gas Setup");

            migrationBuilder.AlterColumn<string>(
                name: "RequiredSkills",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldDefaultValue: "SLS Operation,Powder Handling,Inert Gas Safety");

            migrationBuilder.AlterColumn<string>(
                name: "RequiredMaterials",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "QualityInspector",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "QualityCheckpoints",
                table: "Jobs",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000,
                oldDefaultValue: "{}");

            migrationBuilder.AlterColumn<string>(
                name: "ProcessParameters",
                table: "Jobs",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000,
                oldDefaultValue: "{}");

            migrationBuilder.AlterColumn<double>(
                name: "PreheatingTimeMinutes",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldDefaultValue: 60.0);

            migrationBuilder.AlterColumn<double>(
                name: "PowderRecyclePercentage",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldDefaultValue: 85.0);

            migrationBuilder.AlterColumn<string>(
                name: "PowderLotNumber",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<double>(
                name: "PowderChangeoverTimeMinutes",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldDefaultValue: 30.0);

            migrationBuilder.AlterColumn<string>(
                name: "PartNumber",
                table: "Jobs",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Operator",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "OpcUaStatus",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "OpcUaJobId",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "OpcUaErrorMessages",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Jobs",
                type: "TEXT",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000,
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "MachineId",
                table: "Jobs",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "System");

            migrationBuilder.AlterColumn<double>(
                name: "LaserPowerWatts",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 170.0,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldDefaultValue: 200.0);

            migrationBuilder.AlterColumn<string>(
                name: "HoldReason",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<double>(
                name: "EstimatedPowderUsageKg",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.5,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<double>(
                name: "DensityPercentage",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldDefaultValue: 99.5);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerOrderNumber",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "System");

            migrationBuilder.AlterColumn<double>(
                name: "CoolingTimeMinutes",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldDefaultValue: 240.0);

            migrationBuilder.AlterColumn<string>(
                name: "BuildPlatformId",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "BuildFilePath",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "BuildFileName",
                table: "Jobs",
                type: "TEXT",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255,
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<double>(
                name: "ArgonPurityPercent",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 99.5,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldDefaultValue: 99.900000000000006);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EstimatedDuration",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "InspectionCheckpoints",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                table: "InspectionCheckpoints",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "System");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "InspectionCheckpoints",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "System");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "DelayLogs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "datetime('now')");

            migrationBuilder.AddColumn<int>(
                name: "BuildJobBuildId",
                table: "DelayLogs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "BuildJobs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "In Progress");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BuildJobs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "datetime('now')");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrimary",
                table: "BuildJobParts",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BuildJobParts",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "datetime('now')");

            migrationBuilder.AddColumn<int>(
                name: "BuildJobBuildId",
                table: "BuildJobParts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ArchivedJobs",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "HoldReason",
                table: "ArchivedJobs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.CreateTable(
                name: "JobNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobId = table.Column<int>(type: "INTEGER", nullable: false),
                    Step = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    StepTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 3),
                    NoteType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Info"),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    PartId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobNotes_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobNotes_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MachineName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MachineType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MachineModel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Idle"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAvailableForScheduling = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    LastStatusUpdate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TechnicalSpecifications = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    SupportedMaterials = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CurrentMaterial = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MaintenanceIntervalHours = table.Column<double>(type: "REAL", nullable: false),
                    HoursSinceLastMaintenance = table.Column<double>(type: "REAL", nullable: false),
                    LastMaintenanceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AverageUtilizationPercent = table.Column<double>(type: "REAL", nullable: false),
                    MaintenanceNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    OperatorNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    OpcUaEndpointUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    OpcUaEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CommunicationSettings = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CurrentJobId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    BuildLengthMm = table.Column<double>(type: "REAL", nullable: false),
                    BuildWidthMm = table.Column<double>(type: "REAL", nullable: false),
                    BuildHeightMm = table.Column<double>(type: "REAL", nullable: false),
                    MaxLaserPowerWatts = table.Column<double>(type: "REAL", nullable: false),
                    MaxScanSpeedMmPerSec = table.Column<double>(type: "REAL", nullable: false),
                    MinLayerThicknessMicrons = table.Column<double>(type: "REAL", nullable: false),
                    MaxLayerThicknessMicrons = table.Column<double>(type: "REAL", nullable: false),
                    TotalOperatingHours = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                    table.UniqueConstraint("AK_Machines_MachineId", x => x.MachineId);
                    table.ForeignKey(
                        name: "FK_Machines_Jobs_CurrentJobId",
                        column: x => x.CurrentJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JobStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobId = table.Column<int>(type: "INTEGER", nullable: false),
                    StageType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StageName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ExecutionOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ScheduledStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScheduledEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualStart = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualEnd = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Scheduled"),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 3),
                    EstimatedDurationHours = table.Column<double>(type: "REAL", nullable: false, defaultValue: 1.0),
                    CanStart = table.Column<bool>(type: "INTEGER", nullable: false),
                    SetupTimeHours = table.Column<double>(type: "REAL", nullable: false),
                    CooldownTimeHours = table.Column<double>(type: "REAL", nullable: false),
                    AssignedOperator = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    QualityRequirements = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    RequiredMaterials = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    RequiredTooling = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    ActualCost = table.Column<decimal>(type: "TEXT", nullable: true),
                    IsBlocking = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowParallel = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProgressPercent = table.Column<double>(type: "REAL", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobStages_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobStages_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StageDependencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DependentStageId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiredStageId = table.Column<int>(type: "INTEGER", nullable: false),
                    DependencyType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "FinishToStart"),
                    LagTimeHours = table.Column<double>(type: "REAL", nullable: false),
                    IsMandatory = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageDependencies", x => x.Id);
                    table.CheckConstraint("CK_StageDependency_NoSelfReference", "DependentStageId != RequiredStageId");
                    table.ForeignKey(
                        name: "FK_StageDependencies_JobStages_DependentStageId",
                        column: x => x.DependentStageId,
                        principalTable: "JobStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StageDependencies_JobStages_RequiredStageId",
                        column: x => x.RequiredStageId,
                        principalTable: "JobStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StageNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StageId = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    NoteType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Info"),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 3),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageNotes_JobStages_StageId",
                        column: x => x.StageId,
                        principalTable: "JobStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineCapabilities_MachineId1",
                table: "MachineCapabilities",
                column: "MachineId1");

            migrationBuilder.CreateIndex(
                name: "IX_DelayLogs_BuildJobBuildId",
                table: "DelayLogs",
                column: "BuildJobBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildJobParts_BuildJobBuildId",
                table: "BuildJobParts",
                column: "BuildJobBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_JobNotes_IsCompleted",
                table: "JobNotes",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_JobNotes_JobId",
                table: "JobNotes",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobNotes_JobId_Step",
                table: "JobNotes",
                columns: new[] { "JobId", "Step" });

            migrationBuilder.CreateIndex(
                name: "IX_JobNotes_PartId",
                table: "JobNotes",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_JobNotes_Priority",
                table: "JobNotes",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_JobStages_Department",
                table: "JobStages",
                column: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_JobStages_JobId",
                table: "JobStages",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobStages_JobId_ExecutionOrder",
                table: "JobStages",
                columns: new[] { "JobId", "ExecutionOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_JobStages_MachineId",
                table: "JobStages",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_JobStages_ScheduledEnd",
                table: "JobStages",
                column: "ScheduledEnd");

            migrationBuilder.CreateIndex(
                name: "IX_JobStages_ScheduledStart",
                table: "JobStages",
                column: "ScheduledStart");

            migrationBuilder.CreateIndex(
                name: "IX_JobStages_StageType",
                table: "JobStages",
                column: "StageType");

            migrationBuilder.CreateIndex(
                name: "IX_JobStages_Status",
                table: "JobStages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_CurrentJobId",
                table: "Machines",
                column: "CurrentJobId");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_IsActive",
                table: "Machines",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MachineId",
                table: "Machines",
                column: "MachineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MachineType",
                table: "Machines",
                column: "MachineType");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_Status",
                table: "Machines",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StageDependencies_DependencyType",
                table: "StageDependencies",
                column: "DependencyType");

            migrationBuilder.CreateIndex(
                name: "IX_StageDependencies_DependentStageId",
                table: "StageDependencies",
                column: "DependentStageId");

            migrationBuilder.CreateIndex(
                name: "IX_StageDependencies_RequiredStageId",
                table: "StageDependencies",
                column: "RequiredStageId");

            migrationBuilder.CreateIndex(
                name: "IX_StageNotes_CreatedDate",
                table: "StageNotes",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_StageNotes_NoteType",
                table: "StageNotes",
                column: "NoteType");

            migrationBuilder.CreateIndex(
                name: "IX_StageNotes_Priority",
                table: "StageNotes",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_StageNotes_StageId",
                table: "StageNotes",
                column: "StageId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildJobParts_BuildJobs_BuildJobBuildId",
                table: "BuildJobParts",
                column: "BuildJobBuildId",
                principalTable: "BuildJobs",
                principalColumn: "BuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildJobs_Users_UserId",
                table: "BuildJobs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DelayLogs_BuildJobs_BuildJobBuildId",
                table: "DelayLogs",
                column: "BuildJobBuildId",
                principalTable: "BuildJobs",
                principalColumn: "BuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Parts_PartId",
                table: "Jobs",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MachineCapabilities_Machines_MachineId",
                table: "MachineCapabilities",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MachineCapabilities_Machines_MachineId1",
                table: "MachineCapabilities",
                column: "MachineId1",
                principalTable: "Machines",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildJobParts_BuildJobs_BuildJobBuildId",
                table: "BuildJobParts");

            migrationBuilder.DropForeignKey(
                name: "FK_BuildJobs_Users_UserId",
                table: "BuildJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_DelayLogs_BuildJobs_BuildJobBuildId",
                table: "DelayLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Parts_PartId",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_MachineCapabilities_Machines_MachineId",
                table: "MachineCapabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_MachineCapabilities_Machines_MachineId1",
                table: "MachineCapabilities");

            migrationBuilder.DropTable(
                name: "JobNotes");

            migrationBuilder.DropTable(
                name: "StageDependencies");

            migrationBuilder.DropTable(
                name: "StageNotes");

            migrationBuilder.DropTable(
                name: "JobStages");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_MachineCapabilities_MachineId1",
                table: "MachineCapabilities");

            migrationBuilder.DropIndex(
                name: "IX_DelayLogs_BuildJobBuildId",
                table: "DelayLogs");

            migrationBuilder.DropIndex(
                name: "IX_BuildJobParts_BuildJobBuildId",
                table: "BuildJobParts");

            migrationBuilder.DropColumn(
                name: "AdminEstimatedHoursOverride",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "AdminOverrideBy",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "AdminOverrideDate",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "AdminOverrideReason",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "MachineId1",
                table: "MachineCapabilities");

            migrationBuilder.DropColumn(
                name: "EstimatedDuration",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "BuildJobBuildId",
                table: "DelayLogs");

            migrationBuilder.DropColumn(
                name: "BuildJobBuildId",
                table: "BuildJobParts");

            migrationBuilder.RenameColumn(
                name: "MachineId",
                table: "MachineCapabilities",
                newName: "SlsMachineId");

            migrationBuilder.RenameIndex(
                name: "IX_MachineCapabilities_MachineId_CapabilityType",
                table: "MachineCapabilities",
                newName: "IX_MachineCapabilities_SlsMachineId_CapabilityType");

            migrationBuilder.RenameIndex(
                name: "IX_MachineCapabilities_MachineId",
                table: "MachineCapabilities",
                newName: "IX_MachineCapabilities_SlsMachineId");

            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                table: "UserSettings",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "UTC",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Theme",
                table: "UserSettings",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Light",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "SessionTimeoutMinutes",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 120,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedDate",
                table: "UserSettings",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "ItemsPerPage",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 20,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "EmailNotifications",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "DefaultPage",
                table: "UserSettings",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "/Scheduler",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "UserSettings",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "BrowserNotifications",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "SlsMaterial",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "Ti-6Al-4V Grade 5",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "RequiredMachineType",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "TruPrint 3000",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "QualityCheckpoints",
                table: "Parts",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessType",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "SLS Metal",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessParameters",
                table: "Parts",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "PreferredMachines",
                table: "Parts",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "TI1,TI2",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "PowderSpecification",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "15-45 μm particle size",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "PartNumber",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Material",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "Ti-6Al-4V Grade 5",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "System",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<double>(
                name: "EstimatedHours",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 8.0,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "System",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "AvgDuration",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "8h",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Supervisor",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Scheduled",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "SpecialInstructions",
                table: "Jobs",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "SlsMaterial",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "Ti-6Al-4V Grade 5",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<double>(
                name: "ScanSpeedMmPerSec",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 1200.0,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldDefaultValue: 1000.0);

            migrationBuilder.AlterColumn<bool>(
                name: "RequiresPreheating",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "RequiresPowderSieving",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "RequiresArgonPurge",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "RequiredTooling",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "Build Platform,Powder Sieve,Inert Gas Setup",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "RequiredSkills",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "SLS Operation,Powder Handling,Inert Gas Safety",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "RequiredMaterials",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "QualityInspector",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "QualityCheckpoints",
                table: "Jobs",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessParameters",
                table: "Jobs",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<double>(
                name: "PreheatingTimeMinutes",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 60.0,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<double>(
                name: "PowderRecyclePercentage",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 85.0,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<string>(
                name: "PowderLotNumber",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<double>(
                name: "PowderChangeoverTimeMinutes",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 30.0,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<string>(
                name: "PartNumber",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Operator",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OpcUaStatus",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OpcUaJobId",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OpcUaErrorMessages",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Jobs",
                type: "TEXT",
                maxLength: 1000,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MachineId",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "System",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<double>(
                name: "LaserPowerWatts",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 200.0,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldDefaultValue: 170.0);

            migrationBuilder.AlterColumn<string>(
                name: "HoldReason",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<double>(
                name: "EstimatedPowderUsageKg",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldDefaultValue: 0.5);

            migrationBuilder.AlterColumn<double>(
                name: "DensityPercentage",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 99.5,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerOrderNumber",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "System",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<double>(
                name: "CoolingTimeMinutes",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 240.0,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<string>(
                name: "BuildPlatformId",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "BuildFilePath",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BuildFileName",
                table: "Jobs",
                type: "TEXT",
                maxLength: 255,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "ArgonPurityPercent",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 99.900000000000006,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldDefaultValue: 99.5);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "InspectionCheckpoints",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                table: "InspectionCheckpoints",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "System",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "InspectionCheckpoints",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "System",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "DelayLogs",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "BuildJobs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "In Progress",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BuildJobs",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrimary",
                table: "BuildJobParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BuildJobParts",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ArchivedJobs",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "HoldReason",
                table: "ArchivedJobs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldDefaultValue: "");

            migrationBuilder.CreateTable(
                name: "SlsMachines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CurrentJobId = table.Column<int>(type: "INTEGER", nullable: true),
                    AmbientTemperature = table.Column<double>(type: "REAL", nullable: false),
                    ArgonFlowRate = table.Column<double>(type: "REAL", nullable: false),
                    ArgonPressure = table.Column<double>(type: "REAL", nullable: false),
                    AverageUtilizationPercent = table.Column<double>(type: "REAL", nullable: false),
                    BuildHeightMm = table.Column<double>(type: "REAL", nullable: false, defaultValue: 300.0),
                    BuildLengthMm = table.Column<double>(type: "REAL", nullable: false, defaultValue: 250.0),
                    BuildWidthMm = table.Column<double>(type: "REAL", nullable: false, defaultValue: 250.0),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CurrentBuildHeight = table.Column<double>(type: "REAL", nullable: false),
                    CurrentBuildProgress = table.Column<double>(type: "REAL", nullable: false),
                    CurrentBuildTemperature = table.Column<double>(type: "REAL", nullable: false),
                    CurrentJobStartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CurrentLaserPower = table.Column<double>(type: "REAL", nullable: false),
                    CurrentMaterial = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CurrentOxygenLevel = table.Column<double>(type: "REAL", nullable: false),
                    EstimatedCompletionTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HoursSinceLastMaintenance = table.Column<double>(type: "REAL", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    IsAvailableForScheduling = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    LaserOnTime = table.Column<double>(type: "REAL", nullable: false),
                    LaserStatus = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastMaintenanceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastPowderRefill = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastStatusUpdate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MachineModel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "TruPrint 3000"),
                    MachineName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MaintenanceIntervalHours = table.Column<double>(type: "REAL", nullable: false, defaultValue: 500.0),
                    MaintenanceNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    MaxLaserPowerWatts = table.Column<double>(type: "REAL", nullable: false, defaultValue: 400.0),
                    MaxLayerThicknessMicrons = table.Column<double>(type: "REAL", nullable: false, defaultValue: 60.0),
                    MaxScanSpeedMmPerSec = table.Column<double>(type: "REAL", nullable: false, defaultValue: 7000.0),
                    MinLayerThicknessMicrons = table.Column<double>(type: "REAL", nullable: false, defaultValue: 20.0),
                    NextMaintenanceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OpcUaConnectionStatus = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: "Disconnected"),
                    OpcUaEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    OpcUaEndpointUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    OpcUaLastConnection = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OpcUaNamespace = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OpcUaPasswordHash = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OpcUaUsername = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OperatorNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    PowderLevelPercent = table.Column<double>(type: "REAL", nullable: false),
                    PowderRemainingKg = table.Column<double>(type: "REAL", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 3),
                    QualityScorePercent = table.Column<double>(type: "REAL", nullable: false, defaultValue: 100.0),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Offline"),
                    StatusMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SupportedMaterials = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: "Ti-6Al-4V Grade 5,Ti-6Al-4V ELI Grade 23"),
                    TargetBuildTemperature = table.Column<double>(type: "REAL", nullable: false),
                    TotalJobsCompleted = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalLayersCompleted = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalLayersPlanned = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalOperatingHours = table.Column<double>(type: "REAL", nullable: false),
                    TotalPartsPrinted = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlsMachines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlsMachines_Jobs_CurrentJobId",
                        column: x => x.CurrentJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MachineDataSnapshot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SlsMachineId = table.Column<int>(type: "INTEGER", nullable: false),
                    AlarmDataJson = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, defaultValue: "{}"),
                    ArgonConsumedM3 = table.Column<double>(type: "REAL", nullable: false),
                    EnergyConsumptionKwh = table.Column<double>(type: "REAL", nullable: false),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PowderConsumedKg = table.Column<double>(type: "REAL", nullable: false),
                    ProcessDataJson = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, defaultValue: "{}"),
                    QualityDataJson = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, defaultValue: "{}"),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UtilizationPercent = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineDataSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineDataSnapshot_SlsMachines_SlsMachineId",
                        column: x => x.SlsMachineId,
                        principalTable: "SlsMachines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Industry",
                table: "Parts",
                column: "Industry");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_IsActive",
                table: "Parts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Material",
                table: "Parts",
                column: "Material");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartCategory",
                table: "Parts",
                column: "PartCategory");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartClass",
                table: "Parts",
                column: "PartClass");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_ProcessType",
                table: "Parts",
                column: "ProcessType");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_RequiredMachineType",
                table: "Parts",
                column: "RequiredMachineType");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_SlsMaterial",
                table: "Parts",
                column: "SlsMaterial");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CustomerOrderNumber",
                table: "Jobs",
                column: "CustomerOrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_MachineId",
                table: "Jobs",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OpcUaJobId",
                table: "Jobs",
                column: "OpcUaJobId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ScheduledStart",
                table: "Jobs",
                column: "ScheduledStart");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_SlsMaterial",
                table: "Jobs",
                column: "SlsMaterial");

            migrationBuilder.CreateIndex(
                name: "IX_JobLogEntries_Action",
                table: "JobLogEntries",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_JobLogEntries_MachineId",
                table: "JobLogEntries",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_JobLogEntries_Timestamp",
                table: "JobLogEntries",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_DelayLogs_BuildId",
                table: "DelayLogs",
                column: "BuildId");

            migrationBuilder.CreateIndex(
                name: "IX_DelayLogs_CreatedAt",
                table: "DelayLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DelayLogs_DelayDuration",
                table: "DelayLogs",
                column: "DelayDuration");

            migrationBuilder.CreateIndex(
                name: "IX_DelayLogs_DelayReason",
                table: "DelayLogs",
                column: "DelayReason");

            migrationBuilder.CreateIndex(
                name: "IX_BuildJobs_ActualStartTime",
                table: "BuildJobs",
                column: "ActualStartTime");

            migrationBuilder.CreateIndex(
                name: "IX_BuildJobs_AssociatedScheduledJobId",
                table: "BuildJobs",
                column: "AssociatedScheduledJobId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildJobs_PrinterName",
                table: "BuildJobs",
                column: "PrinterName");

            migrationBuilder.CreateIndex(
                name: "IX_BuildJobs_PrinterName_ActualStartTime",
                table: "BuildJobs",
                columns: new[] { "PrinterName", "ActualStartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildJobs_Status",
                table: "BuildJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BuildJobParts_BuildId",
                table: "BuildJobParts",
                column: "BuildId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildJobParts_BuildId_IsPrimary",
                table: "BuildJobParts",
                columns: new[] { "BuildId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildJobParts_IsPrimary",
                table: "BuildJobParts",
                column: "IsPrimary");

            migrationBuilder.CreateIndex(
                name: "IX_BuildJobParts_PartNumber",
                table: "BuildJobParts",
                column: "PartNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MachineDataSnapshot_MachineId",
                table: "MachineDataSnapshot",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineDataSnapshot_MachineId_Timestamp",
                table: "MachineDataSnapshot",
                columns: new[] { "MachineId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_MachineDataSnapshot_SlsMachineId",
                table: "MachineDataSnapshot",
                column: "SlsMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineDataSnapshot_Timestamp",
                table: "MachineDataSnapshot",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SlsMachines_CurrentJobId",
                table: "SlsMachines",
                column: "CurrentJobId");

            migrationBuilder.CreateIndex(
                name: "IX_SlsMachines_CurrentMaterial",
                table: "SlsMachines",
                column: "CurrentMaterial");

            migrationBuilder.CreateIndex(
                name: "IX_SlsMachines_IsActive",
                table: "SlsMachines",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SlsMachines_IsAvailableForScheduling",
                table: "SlsMachines",
                column: "IsAvailableForScheduling");

            migrationBuilder.CreateIndex(
                name: "IX_SlsMachines_LastStatusUpdate",
                table: "SlsMachines",
                column: "LastStatusUpdate");

            migrationBuilder.CreateIndex(
                name: "IX_SlsMachines_MachineId",
                table: "SlsMachines",
                column: "MachineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlsMachines_Status",
                table: "SlsMachines",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildJobParts_BuildJobs_BuildId",
                table: "BuildJobParts",
                column: "BuildId",
                principalTable: "BuildJobs",
                principalColumn: "BuildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BuildJobs_Users_UserId",
                table: "BuildJobs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DelayLogs_BuildJobs_BuildId",
                table: "DelayLogs",
                column: "BuildId",
                principalTable: "BuildJobs",
                principalColumn: "BuildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Parts_PartId",
                table: "Jobs",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MachineCapabilities_SlsMachines_SlsMachineId",
                table: "MachineCapabilities",
                column: "SlsMachineId",
                principalTable: "SlsMachines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
