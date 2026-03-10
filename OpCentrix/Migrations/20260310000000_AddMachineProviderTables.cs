using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineProviderTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MachineConnectionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ProviderType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Mock"),
                    RestApiBaseUrl = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    OAuthClientId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    OAuthClientSecretEncrypted = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OpcUaEndpointUrl = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    OpcUaUsername = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    OpcUaPasswordHash = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    JobControlEnabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    PollIntervalSeconds = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 30),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    LastSuccessfulSync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastSyncError = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ConsecutiveFailures = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineConnectionSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineStateRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ProviderType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    State = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Unknown"),
                    BuildProgressPercent = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    ActiveJobId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ActiveJobName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    EstimatedCompletion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TelemetryJson = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}"),
                    AlertsJson = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "[]"),
                    RecordedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineStateRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineConnectionSettings_MachineId",
                table: "MachineConnectionSettings",
                column: "MachineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineStateRecords_MachineId",
                table: "MachineStateRecords",
                column: "MachineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineStateRecords_RecordedAt",
                table: "MachineStateRecords",
                column: "RecordedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MachineConnectionSettings");
            migrationBuilder.DropTable(name: "MachineStateRecords");
        }
    }
}
