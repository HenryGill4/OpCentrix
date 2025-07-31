using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class AddEDMLogEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartId",
                table: "BuildJobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EDMLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LogNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PartNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    LogDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Shift = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    OperatorName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OperatorInitials = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    StartTime = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, defaultValue: ""),
                    EndTime = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, defaultValue: ""),
                    Measurement1 = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    Measurement2 = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    ToleranceStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    ScrapIssues = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, defaultValue: ""),
                    TotalTime = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    MachineUsed = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    ProcessType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "EDM"),
                    QualityNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    RequiresReview = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ReviewedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    ReviewedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReviewNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    ProcessParameters = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, defaultValue: "{}"),
                    Measurements = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false, defaultValue: "{}"),
                    PartId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EDMLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EDMLogs_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildJobs_PartId",
                table: "BuildJobs",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_EDMLogs_CreatedDate",
                table: "EDMLogs",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_EDMLogs_IsActive",
                table: "EDMLogs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EDMLogs_IsCompleted",
                table: "EDMLogs",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_EDMLogs_LogDate",
                table: "EDMLogs",
                column: "LogDate");

            migrationBuilder.CreateIndex(
                name: "IX_EDMLogs_LogDate_OperatorName",
                table: "EDMLogs",
                columns: new[] { "LogDate", "OperatorName" });

            migrationBuilder.CreateIndex(
                name: "IX_EDMLogs_LogNumber",
                table: "EDMLogs",
                column: "LogNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EDMLogs_OperatorName",
                table: "EDMLogs",
                column: "OperatorName");

            migrationBuilder.CreateIndex(
                name: "IX_EDMLogs_PartId",
                table: "EDMLogs",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_EDMLogs_PartNumber",
                table: "EDMLogs",
                column: "PartNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EDMLogs_PartNumber_LogDate",
                table: "EDMLogs",
                columns: new[] { "PartNumber", "LogDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EDMLogs_RequiresReview",
                table: "EDMLogs",
                column: "RequiresReview");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildJobs_Parts_PartId",
                table: "BuildJobs",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildJobs_Parts_PartId",
                table: "BuildJobs");

            migrationBuilder.DropTable(
                name: "EDMLogs");

            migrationBuilder.DropIndex(
                name: "IX_BuildJobs_PartId",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "PartId",
                table: "BuildJobs");
        }
    }
}
