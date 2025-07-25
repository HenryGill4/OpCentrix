using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulerSettingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachineDataSnapshot_SlsMachines_SlsMachineId",
                table: "MachineDataSnapshot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MachineDataSnapshot",
                table: "MachineDataSnapshot");

            migrationBuilder.RenameTable(
                name: "MachineDataSnapshot",
                newName: "MachineDataSnapshots");

            migrationBuilder.RenameIndex(
                name: "IX_MachineDataSnapshot_Timestamp",
                table: "MachineDataSnapshots",
                newName: "IX_MachineDataSnapshots_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_MachineDataSnapshot_SlsMachineId",
                table: "MachineDataSnapshots",
                newName: "IX_MachineDataSnapshots_SlsMachineId");

            migrationBuilder.RenameIndex(
                name: "IX_MachineDataSnapshot_MachineId_Timestamp",
                table: "MachineDataSnapshots",
                newName: "IX_MachineDataSnapshots_MachineId_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_MachineDataSnapshot_MachineId",
                table: "MachineDataSnapshots",
                newName: "IX_MachineDataSnapshots_MachineId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MachineDataSnapshots",
                table: "MachineDataSnapshots",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SchedulerSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TitaniumToTitaniumChangeoverMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 30),
                    InconelToInconelChangeoverMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 45),
                    CrossMaterialChangeoverMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 120),
                    DefaultMaterialChangeoverMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 60),
                    DefaultPreheatingTimeMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 60),
                    DefaultCoolingTimeMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 240),
                    DefaultPostProcessingTimeMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 90),
                    SetupTimeBufferMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 30),
                    StandardShiftStart = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    StandardShiftEnd = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    EveningShiftStart = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    EveningShiftEnd = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    NightShiftStart = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    NightShiftEnd = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    EnableWeekendOperations = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    SaturdayOperations = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    SundayOperations = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    TI1MachinePriority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 5),
                    TI2MachinePriority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 5),
                    INCMachinePriority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 5),
                    AllowConcurrentJobs = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    MaxJobsPerMachinePerDay = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 8),
                    RequiredOperatorCertification = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "SLS Basic"),
                    QualityCheckRequired = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    MinimumTimeBetweenJobsMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 15),
                    EmergencyOverrideEnabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    NotifyOnScheduleConflicts = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    NotifyOnMaterialChanges = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    AdvanceWarningTimeMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 60),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ChangeNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerSettings_Id",
                table: "SchedulerSettings",
                column: "Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MachineDataSnapshots_SlsMachines_SlsMachineId",
                table: "MachineDataSnapshots",
                column: "SlsMachineId",
                principalTable: "SlsMachines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachineDataSnapshots_SlsMachines_SlsMachineId",
                table: "MachineDataSnapshots");

            migrationBuilder.DropTable(
                name: "SchedulerSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MachineDataSnapshots",
                table: "MachineDataSnapshots");

            migrationBuilder.RenameTable(
                name: "MachineDataSnapshots",
                newName: "MachineDataSnapshot");

            migrationBuilder.RenameIndex(
                name: "IX_MachineDataSnapshots_Timestamp",
                table: "MachineDataSnapshot",
                newName: "IX_MachineDataSnapshot_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_MachineDataSnapshots_SlsMachineId",
                table: "MachineDataSnapshot",
                newName: "IX_MachineDataSnapshot_SlsMachineId");

            migrationBuilder.RenameIndex(
                name: "IX_MachineDataSnapshots_MachineId_Timestamp",
                table: "MachineDataSnapshot",
                newName: "IX_MachineDataSnapshot_MachineId_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_MachineDataSnapshots_MachineId",
                table: "MachineDataSnapshot",
                newName: "IX_MachineDataSnapshot_MachineId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MachineDataSnapshot",
                table: "MachineDataSnapshot",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MachineDataSnapshot_SlsMachines_SlsMachineId",
                table: "MachineDataSnapshot",
                column: "SlsMachineId",
                principalTable: "SlsMachines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
