using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineProviderTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ActualAverageDurationHours",
                table: "PartStageRequirements",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActualSampleCount",
                table: "PartStageRequirements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimateLastUpdated",
                table: "PartStageRequirements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimateSource",
                table: "PartStageRequirements",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "LastActualDurationHours",
                table: "PartStageRequirements",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UpstreamGapHours",
                table: "Jobs",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "BuildJobs",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CrmAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Active"),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrmAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, defaultValue: "Info"),
                    IsDismissed = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmAlerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "General"),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true, defaultValue: "cog"),
                    ColorCode = table.Column<string>(type: "TEXT", maxLength: 7, nullable: true, defaultValue: "#6B7280"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineComponents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineConnectionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProviderType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Mock"),
                    ConnectionConfigJson = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}"),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    PollIntervalSeconds = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 30),
                    LastConnectedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastError = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ConsecutiveFailures = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    MaxConsecutiveFailures = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 10),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineConnectionSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineConnectionSettings_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MachineStateRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Unknown"),
                    BuildProgressPercent = table.Column<double>(type: "REAL", nullable: true),
                    CurrentJobReference = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsConnected = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    AlarmsSnapshot = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    StateDataJson = table.Column<string>(type: "TEXT", nullable: true),
                    ProviderType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Unknown"),
                    IsStateChange = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    PreviousStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineStateRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineStateRecords_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceActionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PerformedByUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ResetPerformed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceActionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceAssets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssetType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    Department = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    MetadataJson = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceAssets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceFactorDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    FactorType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, defaultValue: "Counter"),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, defaultValue: "BuildData"),
                    ParametersSchemaJson = table.Column<string>(type: "TEXT", nullable: true),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, defaultValue: ""),
                    SupportsParameterization = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceFactorDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceProcedureTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    AppliesToAssetType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    DefaultInstructions = table.Column<string>(type: "TEXT", nullable: true),
                    SafetyNotes = table.Column<string>(type: "TEXT", nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    DefaultPriority = table.Column<int>(type: "INTEGER", nullable: false),
                    RecurrenceStrategy = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, defaultValue: "FactorBased"),
                    GroupCombinationOperator = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false, defaultValue: "OR"),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    MetadataJson = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceProcedureTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    MachineComponentId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProductionStageId = table.Column<int>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TriggerType = table.Column<int>(type: "INTEGER", nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false),
                    ThresholdValue = table.Column<double>(type: "REAL", nullable: false),
                    IntervalDays = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    EarlyWarningPercent = table.Column<int>(type: "INTEGER", nullable: true),
                    EarlyWarningDays = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentValue = table.Column<double>(type: "REAL", nullable: false),
                    LastServiceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextDueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDue = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsOverdue = table.Column<bool>(type: "INTEGER", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TriggerJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    AffectedJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OriginalEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NewStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NewEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ShiftMinutes = table.Column<double>(type: "REAL", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleAdjustments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrmContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmContacts_CrmAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "CrmAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MachineComponentId = table.Column<int>(type: "INTEGER", nullable: true),
                    ScheduleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ScheduleType = table.Column<int>(type: "INTEGER", nullable: false),
                    IntervalDays = table.Column<int>(type: "INTEGER", nullable: true),
                    IntervalWeeks = table.Column<int>(type: "INTEGER", nullable: true),
                    IntervalMonths = table.Column<int>(type: "INTEGER", nullable: true),
                    OperatingHoursInterval = table.Column<double>(type: "REAL", nullable: true),
                    CyclesInterval = table.Column<int>(type: "INTEGER", nullable: true),
                    UnitsProducedInterval = table.Column<int>(type: "INTEGER", nullable: true),
                    ConditionThreshold = table.Column<double>(type: "REAL", nullable: true),
                    ConditionMetric = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LastMaintenanceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimatedDurationHours = table.Column<double>(type: "REAL", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    DefaultTechnician = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DefaultTechnicianUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    AutoCreateWorkOrders = table.Column<bool>(type: "INTEGER", nullable: false),
                    LeadTimeDays = table.Column<int>(type: "INTEGER", nullable: true),
                    Instructions = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    RequiredTools = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RequiredParts = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MachineId1 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceSchedules_MachineComponents_MachineComponentId",
                        column: x => x.MachineComponentId,
                        principalTable: "MachineComponents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceSchedules_Machines_MachineId1",
                        column: x => x.MachineId1,
                        principalTable: "Machines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceSchedules_Users_DefaultTechnicianUserId",
                        column: x => x.DefaultTechnicianUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServiceName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ServiceType = table.Column<int>(type: "INTEGER", nullable: false),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MachineComponentId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UpdateIntervalMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    DataSource = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Configuration = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CurrentValue = table.Column<double>(type: "REAL", nullable: false),
                    LastUpdateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastResetTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    WarningThreshold = table.Column<double>(type: "REAL", nullable: true),
                    CriticalThreshold = table.Column<double>(type: "REAL", nullable: true),
                    MaxValue = table.Column<double>(type: "REAL", nullable: true),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MachineId1 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceServices_MachineComponents_MachineComponentId",
                        column: x => x.MachineComponentId,
                        principalTable: "MachineComponents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceServices_Machines_MachineId1",
                        column: x => x.MachineId1,
                        principalTable: "Machines",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OperationalTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    TaskType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    AssetId = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    DueAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConfigJson = table.Column<string>(type: "TEXT", nullable: true),
                    OverdueFlag = table.Column<int>(type: "INTEGER", nullable: true),
                    LastResetAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationalTasks_MaintenanceAssets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "MaintenanceAssets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceCounterAggregates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssetId = table.Column<int>(type: "INTEGER", nullable: false),
                    FactorDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceCounterAggregates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceCounterAggregates_MaintenanceAssets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "MaintenanceAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceCounterAggregates_MaintenanceFactorDefinitions_FactorDefinitionId",
                        column: x => x.FactorDefinitionId,
                        principalTable: "MaintenanceFactorDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceProcedureTemplateFactors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProcedureTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    FactorDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParameterJson = table.Column<string>(type: "TEXT", nullable: true),
                    ThresholdValue = table.Column<decimal>(type: "TEXT", nullable: false),
                    ComparisonOperator = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, defaultValue: "GreaterOrEqual"),
                    LogicalGroupKey = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, defaultValue: "A"),
                    GroupOperator = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false, defaultValue: "AND"),
                    ResetCounterOnCompletion = table.Column<bool>(type: "INTEGER", nullable: false),
                    Optional = table.Column<bool>(type: "INTEGER", nullable: false),
                    SequenceOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Weight = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceProcedureTemplateFactors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceProcedureTemplateFactors_MaintenanceFactorDefinitions_FactorDefinitionId",
                        column: x => x.FactorDefinitionId,
                        principalTable: "MaintenanceFactorDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceProcedureTemplateFactors_MaintenanceProcedureTemplates_ProcedureTemplateId",
                        column: x => x.ProcedureTemplateId,
                        principalTable: "MaintenanceProcedureTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceScheduleInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProcedureTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssetId = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Active"),
                    NextDueAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastEvaluatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastCompletionAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastOccurrenceId = table.Column<int>(type: "INTEGER", nullable: true),
                    OverrideJson = table.Column<string>(type: "TEXT", nullable: true),
                    PriorityOverride = table.Column<int>(type: "INTEGER", nullable: true),
                    CalendarPattern = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceScheduleInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceScheduleInstances_MaintenanceAssets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "MaintenanceAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceScheduleInstances_MaintenanceProcedureTemplates_ProcedureTemplateId",
                        column: x => x.ProcedureTemplateId,
                        principalTable: "MaintenanceProcedureTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceWorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkOrderNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MachineComponentId = table.Column<int>(type: "INTEGER", nullable: true),
                    MaintenanceRuleId = table.Column<int>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    WorkOrderType = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduledStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ScheduledEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AssignedTechnician = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AssignedTechnicianUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    EstimatedHours = table.Column<double>(type: "REAL", nullable: false),
                    ActualHours = table.Column<double>(type: "REAL", nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    ActualCost = table.Column<decimal>(type: "TEXT", nullable: true),
                    WorkPerformed = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    PartsUsed = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CompletionNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RequiresShutdown = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShutdownDurationMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MachineId1 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceWorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceWorkOrders_MachineComponents_MachineComponentId",
                        column: x => x.MachineComponentId,
                        principalTable: "MachineComponents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceWorkOrders_Machines_MachineId1",
                        column: x => x.MachineId1,
                        principalTable: "Machines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceWorkOrders_MaintenanceRules_MaintenanceRuleId",
                        column: x => x.MaintenanceRuleId,
                        principalTable: "MaintenanceRules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceWorkOrders_Users_AssignedTechnicianUserId",
                        column: x => x.AssignedTechnicianUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CrmTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Open"),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    DueAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AssignedToUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    ContactId = table.Column<int>(type: "INTEGER", nullable: true),
                    HasReminder = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReminderDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReminderMinutesBefore = table.Column<int>(type: "INTEGER", nullable: false),
                    ReminderType = table.Column<string>(type: "TEXT", nullable: false),
                    IsReminderSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReminderSentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NotifyOnStatusChange = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifyAssignee = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifyCreator = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmTasks_CrmAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "CrmAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CrmTasks_CrmContacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "CrmContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CrmTasks_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceServiceData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaintenanceServiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsCalculated = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsReset = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaintenanceServiceId1 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceServiceData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceServiceData_MaintenanceServices_MaintenanceServiceId",
                        column: x => x.MaintenanceServiceId,
                        principalTable: "MaintenanceServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceServiceData_MaintenanceServices_MaintenanceServiceId1",
                        column: x => x.MaintenanceServiceId1,
                        principalTable: "MaintenanceServices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceOccurrences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScheduleInstanceId = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    DueAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Open"),
                    EvaluationSnapshotJson = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}"),
                    CompletionNotes = table.Column<string>(type: "TEXT", nullable: true),
                    ActualDurationMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    AutoGenerated = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedFromFactorGroup = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceOccurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceOccurrences_MaintenanceScheduleInstances_ScheduleInstanceId",
                        column: x => x.ScheduleInstanceId,
                        principalTable: "MaintenanceScheduleInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MaintenanceRuleId = table.Column<int>(type: "INTEGER", nullable: true),
                    MaintenanceServiceId = table.Column<int>(type: "INTEGER", nullable: true),
                    WorkOrderId = table.Column<int>(type: "INTEGER", nullable: true),
                    NotificationType = table.Column<int>(type: "INTEGER", nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    AcknowledgedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsEmailSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSmsSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBrowserNotificationSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    Recipients = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AutoDismissAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDismissed = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    MachineId1 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceNotifications_Machines_MachineId1",
                        column: x => x.MachineId1,
                        principalTable: "Machines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceNotifications_MaintenanceRules_MaintenanceRuleId",
                        column: x => x.MaintenanceRuleId,
                        principalTable: "MaintenanceRules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceNotifications_MaintenanceServices_MaintenanceServiceId",
                        column: x => x.MaintenanceServiceId,
                        principalTable: "MaintenanceServices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceNotifications_MaintenanceWorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "MaintenanceWorkOrders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CrmTaskProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProgressNote = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    PercentComplete = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    AttachmentPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ProgressType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Update"),
                    IsVisibleToClient = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmTaskProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmTaskProgress_CrmTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "CrmTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrmTaskProgress_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceCounterBaselines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssetId = table.Column<int>(type: "INTEGER", nullable: false),
                    FactorDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    OccurrenceId = table.Column<int>(type: "INTEGER", nullable: false),
                    BaselineValue = table.Column<double>(type: "REAL", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceCounterBaselines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceCounterBaselines_MaintenanceAssets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "MaintenanceAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceCounterBaselines_MaintenanceFactorDefinitions_FactorDefinitionId",
                        column: x => x.FactorDefinitionId,
                        principalTable: "MaintenanceFactorDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceCounterBaselines_MaintenanceOccurrences_OccurrenceId",
                        column: x => x.OccurrenceId,
                        principalTable: "MaintenanceOccurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrmAccounts_Name",
                table: "CrmAccounts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CrmAlerts_CreatedDate",
                table: "CrmAlerts",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CrmAlerts_IsDismissed",
                table: "CrmAlerts",
                column: "IsDismissed");

            migrationBuilder.CreateIndex(
                name: "IX_CrmAlerts_IsDismissed_CreatedDate",
                table: "CrmAlerts",
                columns: new[] { "IsDismissed", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmAlerts_Severity",
                table: "CrmAlerts",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_CrmContacts_AccountId",
                table: "CrmContacts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmContacts_Name",
                table: "CrmContacts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CrmTaskProgress_CreatedByUserId",
                table: "CrmTaskProgress",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmTaskProgress_CreatedDate",
                table: "CrmTaskProgress",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CrmTaskProgress_TaskId",
                table: "CrmTaskProgress",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmTaskProgress_TaskId_CreatedDate",
                table: "CrmTaskProgress",
                columns: new[] { "TaskId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmTasks_AccountId",
                table: "CrmTasks",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmTasks_AssignedToUserId",
                table: "CrmTasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmTasks_ContactId",
                table: "CrmTasks",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmTasks_DueAt",
                table: "CrmTasks",
                column: "DueAt");

            migrationBuilder.CreateIndex(
                name: "IX_CrmTasks_Status",
                table: "CrmTasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MachineComponents_MachineId",
                table: "MachineComponents",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineComponents_MachineId_Category",
                table: "MachineComponents",
                columns: new[] { "MachineId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_MachineComponents_MachineId_IsActive",
                table: "MachineComponents",
                columns: new[] { "MachineId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MachineConnectionSettings_IsEnabled",
                table: "MachineConnectionSettings",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_MachineConnectionSettings_MachineId",
                table: "MachineConnectionSettings",
                column: "MachineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineConnectionSettings_ProviderType",
                table: "MachineConnectionSettings",
                column: "ProviderType");

            migrationBuilder.CreateIndex(
                name: "IX_MachineStateRecords_IsStateChange",
                table: "MachineStateRecords",
                column: "IsStateChange");

            migrationBuilder.CreateIndex(
                name: "IX_MachineStateRecords_MachineId",
                table: "MachineStateRecords",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineStateRecords_MachineId_Timestamp",
                table: "MachineStateRecords",
                columns: new[] { "MachineId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_MachineStateRecords_Status",
                table: "MachineStateRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MachineStateRecords_Timestamp",
                table: "MachineStateRecords",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceActionLogs_MachineId",
                table: "MaintenanceActionLogs",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceActionLogs_PerformedAt",
                table: "MaintenanceActionLogs",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceActionLogs_PerformedByUserId",
                table: "MaintenanceActionLogs",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceActionLogs_RuleId",
                table: "MaintenanceActionLogs",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceAssets_Active",
                table: "MaintenanceAssets",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceAssets_MachineId",
                table: "MaintenanceAssets",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCounterAggregates_AssetId_FactorDefinitionId_PeriodStart",
                table: "MaintenanceCounterAggregates",
                columns: new[] { "AssetId", "FactorDefinitionId", "PeriodStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCounterAggregates_FactorDefinitionId",
                table: "MaintenanceCounterAggregates",
                column: "FactorDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCounterBaselines_AssetId_FactorDefinitionId",
                table: "MaintenanceCounterBaselines",
                columns: new[] { "AssetId", "FactorDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCounterBaselines_FactorDefinitionId",
                table: "MaintenanceCounterBaselines",
                column: "FactorDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCounterBaselines_OccurrenceId",
                table: "MaintenanceCounterBaselines",
                column: "OccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceFactorDefinitions_Active",
                table: "MaintenanceFactorDefinitions",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceFactorDefinitions_Code",
                table: "MaintenanceFactorDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_CreatedAt",
                table: "MaintenanceNotifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_IsDismissed",
                table: "MaintenanceNotifications",
                column: "IsDismissed");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_MachineId",
                table: "MaintenanceNotifications",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_MachineId1",
                table: "MaintenanceNotifications",
                column: "MachineId1");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_MaintenanceRuleId",
                table: "MaintenanceNotifications",
                column: "MaintenanceRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_MaintenanceServiceId",
                table: "MaintenanceNotifications",
                column: "MaintenanceServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_NotificationType",
                table: "MaintenanceNotifications",
                column: "NotificationType");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_Severity",
                table: "MaintenanceNotifications",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_WorkOrderId",
                table: "MaintenanceNotifications",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceOccurrences_ScheduleInstanceId_Status",
                table: "MaintenanceOccurrences",
                columns: new[] { "ScheduleInstanceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceProcedureTemplateFactors_FactorDefinitionId",
                table: "MaintenanceProcedureTemplateFactors",
                column: "FactorDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceProcedureTemplateFactors_ProcedureTemplateId_LogicalGroupKey",
                table: "MaintenanceProcedureTemplateFactors",
                columns: new[] { "ProcedureTemplateId", "LogicalGroupKey" });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceProcedureTemplates_Active",
                table: "MaintenanceProcedureTemplates",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRules_IsActive",
                table: "MaintenanceRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRules_MachineComponentId",
                table: "MaintenanceRules",
                column: "MachineComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRules_MachineComponentId_IsActive",
                table: "MaintenanceRules",
                columns: new[] { "MachineComponentId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRules_MachineId",
                table: "MaintenanceRules",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRules_MachineId_IsActive",
                table: "MaintenanceRules",
                columns: new[] { "MachineId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRules_ProductionStageId",
                table: "MaintenanceRules",
                column: "ProductionStageId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceScheduleInstances_AssetId",
                table: "MaintenanceScheduleInstances",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceScheduleInstances_ProcedureTemplateId",
                table: "MaintenanceScheduleInstances",
                column: "ProcedureTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceScheduleInstances_Status",
                table: "MaintenanceScheduleInstances",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_DefaultTechnicianUserId",
                table: "MaintenanceSchedules",
                column: "DefaultTechnicianUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_IsActive",
                table: "MaintenanceSchedules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_MachineComponentId",
                table: "MaintenanceSchedules",
                column: "MachineComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_MachineId",
                table: "MaintenanceSchedules",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_MachineId1",
                table: "MaintenanceSchedules",
                column: "MachineId1");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_ScheduleType",
                table: "MaintenanceSchedules",
                column: "ScheduleType");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceServiceData_MaintenanceServiceId",
                table: "MaintenanceServiceData",
                column: "MaintenanceServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceServiceData_MaintenanceServiceId1",
                table: "MaintenanceServiceData",
                column: "MaintenanceServiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceServiceData_Timestamp",
                table: "MaintenanceServiceData",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceServices_IsEnabled",
                table: "MaintenanceServices",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceServices_MachineComponentId",
                table: "MaintenanceServices",
                column: "MachineComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceServices_MachineId",
                table: "MaintenanceServices",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceServices_MachineId1",
                table: "MaintenanceServices",
                column: "MachineId1");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceServices_ServiceType",
                table: "MaintenanceServices",
                column: "ServiceType");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceStates_IsDue",
                table: "MaintenanceStates",
                column: "IsDue");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceStates_IsOverdue",
                table: "MaintenanceStates",
                column: "IsOverdue");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceStates_MachineId",
                table: "MaintenanceStates",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceStates_MachineId_RuleId",
                table: "MaintenanceStates",
                columns: new[] { "MachineId", "RuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceStates_RuleId",
                table: "MaintenanceStates",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_AssignedTechnicianUserId",
                table: "MaintenanceWorkOrders",
                column: "AssignedTechnicianUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_CreatedAt",
                table: "MaintenanceWorkOrders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_MachineComponentId",
                table: "MaintenanceWorkOrders",
                column: "MachineComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_MachineId",
                table: "MaintenanceWorkOrders",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_MachineId1",
                table: "MaintenanceWorkOrders",
                column: "MachineId1");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_MaintenanceRuleId",
                table: "MaintenanceWorkOrders",
                column: "MaintenanceRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_Priority",
                table: "MaintenanceWorkOrders",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_Status",
                table: "MaintenanceWorkOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_WorkOrderNumber",
                table: "MaintenanceWorkOrders",
                column: "WorkOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperationalTasks_AssetId",
                table: "OperationalTasks",
                column: "AssetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrmAlerts");

            migrationBuilder.DropTable(
                name: "CrmTaskProgress");

            migrationBuilder.DropTable(
                name: "MachineConnectionSettings");

            migrationBuilder.DropTable(
                name: "MachineStateRecords");

            migrationBuilder.DropTable(
                name: "MaintenanceActionLogs");

            migrationBuilder.DropTable(
                name: "MaintenanceCounterAggregates");

            migrationBuilder.DropTable(
                name: "MaintenanceCounterBaselines");

            migrationBuilder.DropTable(
                name: "MaintenanceNotifications");

            migrationBuilder.DropTable(
                name: "MaintenanceProcedureTemplateFactors");

            migrationBuilder.DropTable(
                name: "MaintenanceSchedules");

            migrationBuilder.DropTable(
                name: "MaintenanceServiceData");

            migrationBuilder.DropTable(
                name: "MaintenanceStates");

            migrationBuilder.DropTable(
                name: "OperationalTasks");

            migrationBuilder.DropTable(
                name: "ScheduleAdjustments");

            migrationBuilder.DropTable(
                name: "CrmTasks");

            migrationBuilder.DropTable(
                name: "MaintenanceOccurrences");

            migrationBuilder.DropTable(
                name: "MaintenanceWorkOrders");

            migrationBuilder.DropTable(
                name: "MaintenanceFactorDefinitions");

            migrationBuilder.DropTable(
                name: "MaintenanceServices");

            migrationBuilder.DropTable(
                name: "CrmContacts");

            migrationBuilder.DropTable(
                name: "MaintenanceScheduleInstances");

            migrationBuilder.DropTable(
                name: "MaintenanceRules");

            migrationBuilder.DropTable(
                name: "MachineComponents");

            migrationBuilder.DropTable(
                name: "CrmAccounts");

            migrationBuilder.DropTable(
                name: "MaintenanceAssets");

            migrationBuilder.DropTable(
                name: "MaintenanceProcedureTemplates");

            migrationBuilder.DropColumn(
                name: "ActualAverageDurationHours",
                table: "PartStageRequirements");

            migrationBuilder.DropColumn(
                name: "ActualSampleCount",
                table: "PartStageRequirements");

            migrationBuilder.DropColumn(
                name: "EstimateLastUpdated",
                table: "PartStageRequirements");

            migrationBuilder.DropColumn(
                name: "EstimateSource",
                table: "PartStageRequirements");

            migrationBuilder.DropColumn(
                name: "LastActualDurationHours",
                table: "PartStageRequirements");

            migrationBuilder.DropColumn(
                name: "UpstreamGapHours",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "BuildJobs");
        }
    }
}
