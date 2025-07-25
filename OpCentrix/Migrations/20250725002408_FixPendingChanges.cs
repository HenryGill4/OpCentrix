using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaterialCostPerUnit",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OverheadCostPerHour",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "WeightKg",
                table: "Parts",
                newName: "WidthMm");

            migrationBuilder.RenameColumn(
                name: "VolumeM3",
                table: "Parts",
                newName: "WeightGrams");

            migrationBuilder.RenameColumn(
                name: "ToolingCost",
                table: "Parts",
                newName: "StandardSellingPrice");

            migrationBuilder.RenameColumn(
                name: "MaterialCostPerUnit",
                table: "Parts",
                newName: "QualityInspectionCost");

            migrationBuilder.RenameColumn(
                name: "ChangeoverTimeMinutes",
                table: "Parts",
                newName: "VolumeMm3");

            migrationBuilder.AlterColumn<string>(
                name: "RequiredMachineType",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "TruPrint 3000",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "ProcessType",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "SLS Metal",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "PreferredMachines",
                table: "Parts",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "TI1,TI2",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "PartNumber",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

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
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Application",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ArgonCostPerHour",
                table: "Parts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageCostPerUnit",
                table: "Parts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "AveragePowderUtilization",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "BuildFileTemplate",
                table: "Parts",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CadFilePath",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CadFileVersion",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "CoolingTimeMinutes",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "HeightMm",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "LengthMm",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "MachineOperatingCostPerHour",
                table: "Parts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialCostPerKg",
                table: "Parts",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "MaxOxygenContent",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxSurfaceRoughnessRa",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "PostProcessingCost",
                table: "Parts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "PostProcessingTimeMinutes",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PowderChangeoverTimeMinutes",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PowderRequirementKg",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "PowderSpecification",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "15-45 μm particle size");

            migrationBuilder.AddColumn<double>(
                name: "PreheatingTimeMinutes",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RecommendedBuildTemperature",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RecommendedHatchSpacing",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RecommendedLaserPower",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RecommendedLayerThickness",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RecommendedScanSpeed",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RequiredArgonPurity",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresAS9100",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresCertification",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresFDA",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresNADCAP",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresSupports",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SlsMaterial",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "Ti-6Al-4V Grade 5");

            migrationBuilder.AddColumn<double>(
                name: "SupportRemovalTimeMinutes",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "SupportStrategy",
                table: "Parts",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SurfaceFinishRequirement",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Supervisor",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SpecialInstructions",
                table: "Jobs",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "RequiredTooling",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "Build Platform,Powder Sieve,Inert Gas Setup",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "RequiredSkills",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "SLS Operation,Powder Handling,Inert Gas Safety",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "RequiredMaterials",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "QualityInspector",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PartNumber",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

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
                name: "Notes",
                table: "Jobs",
                type: "TEXT",
                maxLength: 1000,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
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
                oldMaxLength: 50);

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

            migrationBuilder.AlterColumn<string>(
                name: "HoldReason",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerOrderNumber",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT");

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

            migrationBuilder.AddColumn<double>(
                name: "ActualPowderUsageKg",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "ArgonCostPerHour",
                table: "Jobs",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "ArgonPurityPercent",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 99.900000000000006);

            migrationBuilder.AddColumn<DateTime>(
                name: "BuildFileCreatedDate",
                table: "Jobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildFileName",
                table: "Jobs",
                type: "TEXT",
                maxLength: 255,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BuildFilePath",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "BuildFileSizeBytes",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "BuildLayerNumber",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BuildPlatformId",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "BuildTemperatureCelsius",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 180.0);

            migrationBuilder.AddColumn<double>(
                name: "BuildTimeMinutes",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CoolingTimeMinutes",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 240.0);

            migrationBuilder.AddColumn<double>(
                name: "CurrentArgonFlowRate",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CurrentBuildTemperature",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CurrentLaserPowerWatts",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CurrentOxygenLevel",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DensityPercentage",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 99.5);

            migrationBuilder.AddColumn<double>(
                name: "EstimatedPowderUsageKg",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "HatchSpacingMicrons",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 120.0);

            migrationBuilder.AddColumn<double>(
                name: "LaserPowerWatts",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 200.0);

            migrationBuilder.AddColumn<double>(
                name: "LayerThicknessMicrons",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 30.0);

            migrationBuilder.AddColumn<decimal>(
                name: "MachineOperatingCostPerHour",
                table: "Jobs",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialCostPerKg",
                table: "Jobs",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "OpcUaBuildProgress",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "OpcUaErrorMessages",
                table: "Jobs",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OpcUaJobId",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "OpcUaLastUpdate",
                table: "Jobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpcUaStatus",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "OxygenContentPpm",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 50.0);

            migrationBuilder.AddColumn<double>(
                name: "PostProcessingTimeMinutes",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PowderChangeoverTimeMinutes",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 30.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PowderExpirationDate",
                table: "Jobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PowderLotNumber",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "PowderRecyclePercentage",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 85.0);

            migrationBuilder.AddColumn<decimal>(
                name: "PowerCostPerKwh",
                table: "Jobs",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "PreheatingTimeMinutes",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 60.0);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresArgonPurge",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresPostProcessing",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresPowderSieving",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresPreheating",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<double>(
                name: "ScanSpeedMmPerSec",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 1200.0);

            migrationBuilder.AddColumn<string>(
                name: "SlsMaterial",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "Ti-6Al-4V Grade 5");

            migrationBuilder.AddColumn<double>(
                name: "SurfaceRoughnessRa",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "UltimateTensileStrengthMPa",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "SlsMachines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MachineName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MachineModel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "TruPrint 3000"),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BuildLengthMm = table.Column<double>(type: "REAL", nullable: false, defaultValue: 250.0),
                    BuildWidthMm = table.Column<double>(type: "REAL", nullable: false, defaultValue: 250.0),
                    BuildHeightMm = table.Column<double>(type: "REAL", nullable: false, defaultValue: 300.0),
                    SupportedMaterials = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: "Ti-6Al-4V Grade 5,Ti-6Al-4V ELI Grade 23"),
                    CurrentMaterial = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MaxLaserPowerWatts = table.Column<double>(type: "REAL", nullable: false, defaultValue: 400.0),
                    MaxScanSpeedMmPerSec = table.Column<double>(type: "REAL", nullable: false, defaultValue: 7000.0),
                    MinLayerThicknessMicrons = table.Column<double>(type: "REAL", nullable: false, defaultValue: 20.0),
                    MaxLayerThicknessMicrons = table.Column<double>(type: "REAL", nullable: false, defaultValue: 60.0),
                    OpcUaEndpointUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    OpcUaUsername = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OpcUaPasswordHash = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OpcUaNamespace = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OpcUaEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    OpcUaLastConnection = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OpcUaConnectionStatus = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: "Disconnected"),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Offline"),
                    StatusMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    LastStatusUpdate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CurrentJobId = table.Column<int>(type: "INTEGER", nullable: true),
                    CurrentBuildProgress = table.Column<double>(type: "REAL", nullable: false),
                    CurrentJobStartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimatedCompletionTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CurrentBuildTemperature = table.Column<double>(type: "REAL", nullable: false),
                    TargetBuildTemperature = table.Column<double>(type: "REAL", nullable: false),
                    AmbientTemperature = table.Column<double>(type: "REAL", nullable: false),
                    CurrentOxygenLevel = table.Column<double>(type: "REAL", nullable: false),
                    ArgonFlowRate = table.Column<double>(type: "REAL", nullable: false),
                    ArgonPressure = table.Column<double>(type: "REAL", nullable: false),
                    CurrentLaserPower = table.Column<double>(type: "REAL", nullable: false),
                    LaserOnTime = table.Column<double>(type: "REAL", nullable: false),
                    LaserStatus = table.Column<bool>(type: "INTEGER", nullable: false),
                    PowderLevelPercent = table.Column<double>(type: "REAL", nullable: false),
                    PowderRemainingKg = table.Column<double>(type: "REAL", nullable: false),
                    LastPowderRefill = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CurrentBuildHeight = table.Column<double>(type: "REAL", nullable: false),
                    TotalLayersCompleted = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalLayersPlanned = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalOperatingHours = table.Column<double>(type: "REAL", nullable: false),
                    HoursSinceLastMaintenance = table.Column<double>(type: "REAL", nullable: false),
                    MaintenanceIntervalHours = table.Column<double>(type: "REAL", nullable: false, defaultValue: 500.0),
                    LastMaintenanceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TotalJobsCompleted = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalPartsPrinted = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageUtilizationPercent = table.Column<double>(type: "REAL", nullable: false),
                    QualityScorePercent = table.Column<double>(type: "REAL", nullable: false, defaultValue: 100.0),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    IsAvailableForScheduling = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 3),
                    MaintenanceNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    OperatorNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
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
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastLoginDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineDataSnapshot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SlsMachineId = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    ProcessDataJson = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, defaultValue: "{}"),
                    QualityDataJson = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, defaultValue: "{}"),
                    AlarmDataJson = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, defaultValue: "{}"),
                    UtilizationPercent = table.Column<double>(type: "REAL", nullable: false),
                    EnergyConsumptionKwh = table.Column<double>(type: "REAL", nullable: false),
                    PowderConsumedKg = table.Column<double>(type: "REAL", nullable: false),
                    ArgonConsumedM3 = table.Column<double>(type: "REAL", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "BuildJobs",
                columns: table => new
                {
                    BuildId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PrinterName = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ActualStartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualEndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ScheduledStartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ScheduledEndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "In Progress"),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    LaserRunTime = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    GasUsed_L = table.Column<float>(type: "REAL", nullable: true),
                    PowderUsed_L = table.Column<float>(type: "REAL", nullable: true),
                    ReasonForEnd = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    SetupNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    AssociatedScheduledJobId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildJobs", x => x.BuildId);
                    table.ForeignKey(
                        name: "FK_BuildJobs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionTimeoutMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 120),
                    Theme = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Light"),
                    EmailNotifications = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    BrowserNotifications = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    DefaultPage = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "/Scheduler"),
                    ItemsPerPage = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 20),
                    TimeZone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "UTC"),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildJobParts",
                columns: table => new
                {
                    PartEntryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BuildId = table.Column<int>(type: "INTEGER", nullable: false),
                    PartNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Material = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EstimatedHours = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildJobParts", x => x.PartEntryId);
                    table.ForeignKey(
                        name: "FK_BuildJobParts_BuildJobs_BuildId",
                        column: x => x.BuildId,
                        principalTable: "BuildJobs",
                        principalColumn: "BuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DelayLogs",
                columns: table => new
                {
                    DelayId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BuildId = table.Column<int>(type: "INTEGER", nullable: false),
                    DelayReason = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DelayDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelayLogs", x => x.DelayId);
                    table.ForeignKey(
                        name: "FK_DelayLogs_BuildJobs_BuildId",
                        column: x => x.BuildId,
                        principalTable: "BuildJobs",
                        principalColumn: "BuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Industry",
                table: "Parts",
                column: "Industry");

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
                name: "IX_Jobs_OpcUaJobId",
                table: "Jobs",
                column: "OpcUaJobId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Priority",
                table: "Jobs",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_SlsMaterial",
                table: "Jobs",
                column: "SlsMaterial");

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
                name: "IX_BuildJobs_UserId",
                table: "BuildJobs",
                column: "UserId");

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
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildJobParts");

            migrationBuilder.DropTable(
                name: "DelayLogs");

            migrationBuilder.DropTable(
                name: "MachineDataSnapshot");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "BuildJobs");

            migrationBuilder.DropTable(
                name: "SlsMachines");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Parts_Industry",
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
                name: "IX_Jobs_OpcUaJobId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_Priority",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_SlsMaterial",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Application",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ArgonCostPerHour",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "AverageCostPerUnit",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "AveragePowderUtilization",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BuildFileTemplate",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "CadFilePath",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "CadFileVersion",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "CoolingTimeMinutes",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "HeightMm",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "LengthMm",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "MachineOperatingCostPerHour",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "MaterialCostPerKg",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "MaxOxygenContent",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "MaxSurfaceRoughnessRa",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PostProcessingCost",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PostProcessingTimeMinutes",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PowderChangeoverTimeMinutes",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PowderRequirementKg",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PowderSpecification",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PreheatingTimeMinutes",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RecommendedBuildTemperature",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RecommendedHatchSpacing",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RecommendedLaserPower",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RecommendedLayerThickness",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RecommendedScanSpeed",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiredArgonPurity",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresAS9100",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresCertification",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresFDA",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresNADCAP",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresSupports",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "SlsMaterial",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "SupportRemovalTimeMinutes",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "SupportStrategy",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "SurfaceFinishRequirement",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ActualPowderUsageKg",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ArgonCostPerHour",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ArgonPurityPercent",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "BuildFileCreatedDate",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "BuildFileName",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "BuildFilePath",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "BuildFileSizeBytes",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "BuildLayerNumber",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "BuildPlatformId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "BuildTemperatureCelsius",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "BuildTimeMinutes",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CoolingTimeMinutes",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CurrentArgonFlowRate",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CurrentBuildTemperature",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CurrentLaserPowerWatts",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CurrentOxygenLevel",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "DensityPercentage",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "EstimatedPowderUsageKg",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "HatchSpacingMicrons",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "LaserPowerWatts",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "LayerThicknessMicrons",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "MachineOperatingCostPerHour",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "MaterialCostPerKg",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OpcUaBuildProgress",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OpcUaErrorMessages",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OpcUaJobId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OpcUaLastUpdate",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OpcUaStatus",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OxygenContentPpm",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PostProcessingTimeMinutes",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PowderChangeoverTimeMinutes",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PowderExpirationDate",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PowderLotNumber",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PowderRecyclePercentage",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PowerCostPerKwh",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PreheatingTimeMinutes",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RequiresArgonPurge",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RequiresPostProcessing",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RequiresPowderSieving",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RequiresPreheating",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ScanSpeedMmPerSec",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SlsMaterial",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SurfaceRoughnessRa",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "UltimateTensileStrengthMPa",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "WidthMm",
                table: "Parts",
                newName: "WeightKg");

            migrationBuilder.RenameColumn(
                name: "WeightGrams",
                table: "Parts",
                newName: "VolumeM3");

            migrationBuilder.RenameColumn(
                name: "VolumeMm3",
                table: "Parts",
                newName: "ChangeoverTimeMinutes");

            migrationBuilder.RenameColumn(
                name: "StandardSellingPrice",
                table: "Parts",
                newName: "ToolingCost");

            migrationBuilder.RenameColumn(
                name: "QualityInspectionCost",
                table: "Parts",
                newName: "MaterialCostPerUnit");

            migrationBuilder.AlterColumn<string>(
                name: "RequiredMachineType",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "TruPrint 3000");

            migrationBuilder.AlterColumn<string>(
                name: "ProcessType",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "SLS Metal");

            migrationBuilder.AlterColumn<string>(
                name: "PreferredMachines",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldDefaultValue: "TI1,TI2");

            migrationBuilder.AlterColumn<string>(
                name: "PartNumber",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
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
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "8h");

            migrationBuilder.AlterColumn<string>(
                name: "Supervisor",
                table: "Jobs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialInstructions",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "RequiredTooling",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldDefaultValue: "Build Platform,Powder Sieve,Inert Gas Setup");

            migrationBuilder.AlterColumn<string>(
                name: "RequiredSkills",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldDefaultValue: "SLS Operation,Powder Handling,Inert Gas Safety");

            migrationBuilder.AlterColumn<string>(
                name: "RequiredMaterials",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "QualityInspector",
                table: "Jobs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "PartNumber",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
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
                name: "Notes",
                table: "Jobs",
                type: "TEXT",
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
                maxLength: 50,
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

            migrationBuilder.AlterColumn<string>(
                name: "HoldReason",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerOrderNumber",
                table: "Jobs",
                type: "TEXT",
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

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialCostPerUnit",
                table: "Jobs",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OverheadCostPerHour",
                table: "Jobs",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
