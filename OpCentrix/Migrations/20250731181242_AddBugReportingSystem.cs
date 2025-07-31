using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class AddBugReportingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EDMLogs_Parts_PartId",
                table: "EDMLogs");

            migrationBuilder.DropIndex(
                name: "IX_EDMLogs_CreatedDate",
                table: "EDMLogs");

            migrationBuilder.DropIndex(
                name: "IX_EDMLogs_IsActive",
                table: "EDMLogs");

            migrationBuilder.DropIndex(
                name: "IX_EDMLogs_IsCompleted",
                table: "EDMLogs");

            migrationBuilder.DropIndex(
                name: "IX_EDMLogs_LogDate",
                table: "EDMLogs");

            migrationBuilder.DropIndex(
                name: "IX_EDMLogs_LogDate_OperatorName",
                table: "EDMLogs");

            migrationBuilder.DropIndex(
                name: "IX_EDMLogs_LogNumber",
                table: "EDMLogs");

            migrationBuilder.DropIndex(
                name: "IX_EDMLogs_OperatorName",
                table: "EDMLogs");

            migrationBuilder.DropIndex(
                name: "IX_EDMLogs_PartNumber",
                table: "EDMLogs");

            migrationBuilder.DropIndex(
                name: "IX_EDMLogs_PartNumber_LogDate",
                table: "EDMLogs");

            migrationBuilder.DropIndex(
                name: "IX_EDMLogs_RequiresReview",
                table: "EDMLogs");

            migrationBuilder.AlterColumn<string>(
                name: "TotalTime",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ToleranceStatus",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "StartTime",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Shift",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ScrapIssues",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewedBy",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewNotes",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "RequiresReview",
                table: "EDMLogs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "QualityNotes",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ProcessType",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "EDM");

            migrationBuilder.AlterColumn<string>(
                name: "ProcessParameters",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000,
                oldDefaultValue: "{}");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Measurements",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000,
                oldDefaultValue: "{}");

            migrationBuilder.AlterColumn<string>(
                name: "Measurement2",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Measurement1",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "MachineUsed",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedDate",
                table: "EDMLogs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "datetime('now')");

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "EDMLogs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "EDMLogs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "EndTime",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "EDMLogs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "datetime('now')");

            migrationBuilder.CreateTable(
                name: "BugReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BugId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "New"),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "General"),
                    PageUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PageName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PageArea = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    PageController = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    PageAction = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    ReportedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UserRole = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: ""),
                    UserEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    ReportedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    BrowserName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    BrowserVersion = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: ""),
                    OperatingSystem = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    ScreenResolution = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: ""),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    ErrorType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    StackTrace = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    OperationId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    StepsToReproduce = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    ExpectedBehavior = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    ActualBehavior = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    AdditionalNotes = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    AttachedFiles = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    FormData = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    NetworkRequests = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    ConsoleErrors = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    AssignedTo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    AssignedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolvedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    ResolvedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    ResolutionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    ViewCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    LastViewedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastViewedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    IsReproduced = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ReproducedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    ReproducedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    NotifyReporter = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    RelatedBugIds = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    DuplicateOf = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    PerformanceImpact = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "None"),
                    PageLoadTime = table.Column<decimal>(type: "TEXT", precision: 8, scale: 2, nullable: true),
                    MemoryUsage = table.Column<decimal>(type: "TEXT", precision: 8, scale: 2, nullable: true),
                    CpuUsage = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    CustomMetadata = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BugReports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_AssignedTo",
                table: "BugReports",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_BugId",
                table: "BugReports",
                column: "BugId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_Category",
                table: "BugReports",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_IsActive",
                table: "BugReports",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_IsPublic",
                table: "BugReports",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_OperationId",
                table: "BugReports",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_PageArea",
                table: "BugReports",
                column: "PageArea");

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_PageArea_Status",
                table: "BugReports",
                columns: new[] { "PageArea", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_Priority",
                table: "BugReports",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_ReportedBy",
                table: "BugReports",
                column: "ReportedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_ReportedDate",
                table: "BugReports",
                column: "ReportedDate");

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_ReportedDate_Status",
                table: "BugReports",
                columns: new[] { "ReportedDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_ResolvedDate",
                table: "BugReports",
                column: "ResolvedDate");

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_Severity",
                table: "BugReports",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_Severity_Priority",
                table: "BugReports",
                columns: new[] { "Severity", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_Status",
                table: "BugReports",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_EDMLogs_Parts_PartId",
                table: "EDMLogs",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EDMLogs_Parts_PartId",
                table: "EDMLogs");

            migrationBuilder.DropTable(
                name: "BugReports");

            migrationBuilder.AlterColumn<string>(
                name: "TotalTime",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ToleranceStatus",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "StartTime",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Shift",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ScrapIssues",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ReviewedBy",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ReviewNotes",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<bool>(
                name: "RequiresReview",
                table: "EDMLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "QualityNotes",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessType",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "EDM",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessParameters",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Measurements",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Measurement2",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Measurement1",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "MachineUsed",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedDate",
                table: "EDMLogs",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "EDMLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "EDMLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "EndTime",
                table: "EDMLogs",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "EDMLogs",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

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
                name: "FK_EDMLogs_Parts_PartId",
                table: "EDMLogs",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
