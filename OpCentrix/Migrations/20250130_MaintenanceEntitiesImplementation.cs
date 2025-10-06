using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class MaintenanceEntitiesImplementation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MaintenanceService table is already created - just ensure proper schema
            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceServices_ServiceType",
                table: "MaintenanceServices",
                column: "ServiceType");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceServices_IsEnabled",
                table: "MaintenanceServices",
                column: "IsEnabled");

            // MaintenanceWorkOrder table
            migrationBuilder.CreateTable(
                name: "MaintenanceWorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkOrderNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MachineComponentId = table.Column<int>(type: "INTEGER", nullable: true),
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
                    RequiresShutdown = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShutdownDurationMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    WorkPerformed = table.Column<string>(type: "TEXT", nullable: true),
                    PartsUsed = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceWorkOrders", x => x.Id);
                });

            // MaintenanceSchedule table
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
                    AutoCreateWorkOrders = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    LeadTimeDays = table.Column<int>(type: "INTEGER", nullable: true, defaultValue: 7),
                    Instructions = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    RequiredTools = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RequiredParts = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceSchedules", x => x.Id);
                });

            // MaintenanceServiceData table
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
                    IsReset = table.Column<bool>(type: "INTEGER", nullable: false)
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
                });

            // MaintenanceNotification table
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
                    IsDismissed = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceNotifications", x => x.Id);
                });

            // Create indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_WorkOrderNumber",
                table: "MaintenanceWorkOrders",
                column: "WorkOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_MachineId",
                table: "MaintenanceWorkOrders",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_Status",
                table: "MaintenanceWorkOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_Priority",
                table: "MaintenanceWorkOrders",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceWorkOrders_CreatedAt",
                table: "MaintenanceWorkOrders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_MachineId",
                table: "MaintenanceSchedules",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_ScheduleType",
                table: "MaintenanceSchedules",
                column: "ScheduleType");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_IsActive",
                table: "MaintenanceSchedules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceServiceData_MaintenanceServiceId",
                table: "MaintenanceServiceData",
                column: "MaintenanceServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceServiceData_Timestamp",
                table: "MaintenanceServiceData",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_MachineId",
                table: "MaintenanceNotifications",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_NotificationType",
                table: "MaintenanceNotifications",
                column: "NotificationType");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_Severity",
                table: "MaintenanceNotifications",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_IsDismissed",
                table: "MaintenanceNotifications",
                column: "IsDismissed");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_CreatedAt",
                table: "MaintenanceNotifications",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceNotifications");

            migrationBuilder.DropTable(
                name: "MaintenanceServiceData");

            migrationBuilder.DropTable(
                name: "MaintenanceSchedules");

            migrationBuilder.DropTable(
                name: "MaintenanceWorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceServices_ServiceType",
                table: "MaintenanceServices");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceServices_IsEnabled",
                table: "MaintenanceServices");
        }
    }
}