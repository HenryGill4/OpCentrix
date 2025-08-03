using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase4LearningModelTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildTimeLearningData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BuildJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BuildFileHash = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    OperatorEstimatedHours = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    ActualHours = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    VariancePercent = table.Column<decimal>(type: "TEXT", precision: 6, scale: 2, nullable: false),
                    SupportComplexity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TimeFactors = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    QualityScore = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    DefectCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    BuildHeight = table.Column<decimal>(type: "TEXT", precision: 6, scale: 2, nullable: true),
                    LayerCount = table.Column<int>(type: "INTEGER", nullable: true),
                    TotalParts = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    PartOrientations = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildTimeLearningData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildTimeLearningData_BuildJobs_BuildJobId",
                        column: x => x.BuildJobId,
                        principalTable: "BuildJobs",
                        principalColumn: "BuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OperatorEstimateLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BuildJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedHours = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    TimeFactors = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OperatorNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LoggedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorEstimateLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperatorEstimateLogs_BuildJobs_BuildJobId",
                        column: x => x.BuildJobId,
                        principalTable: "BuildJobs",
                        principalColumn: "BuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartCompletionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BuildJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    PartNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    GoodParts = table.Column<int>(type: "INTEGER", nullable: false),
                    DefectiveParts = table.Column<int>(type: "INTEGER", nullable: false),
                    ReworkParts = table.Column<int>(type: "INTEGER", nullable: false),
                    QualityRate = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    InspectionNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartCompletionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartCompletionLogs_BuildJobs_BuildJobId",
                        column: x => x.BuildJobId,
                        principalTable: "BuildJobs",
                        principalColumn: "BuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildTimeLearningData_BuildFileHash",
                table: "BuildTimeLearningData",
                column: "BuildFileHash");

            migrationBuilder.CreateIndex(
                name: "IX_BuildTimeLearningData_BuildFileHash_SupportComplexity",
                table: "BuildTimeLearningData",
                columns: new[] { "BuildFileHash", "SupportComplexity" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildTimeLearningData_BuildJobId",
                table: "BuildTimeLearningData",
                column: "BuildJobId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildTimeLearningData_MachineId",
                table: "BuildTimeLearningData",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildTimeLearningData_MachineId_BuildFileHash",
                table: "BuildTimeLearningData",
                columns: new[] { "MachineId", "BuildFileHash" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildTimeLearningData_MachineId_SupportComplexity",
                table: "BuildTimeLearningData",
                columns: new[] { "MachineId", "SupportComplexity" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildTimeLearningData_MachineId_TotalParts_SupportComplexity",
                table: "BuildTimeLearningData",
                columns: new[] { "MachineId", "TotalParts", "SupportComplexity" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildTimeLearningData_QualityScore",
                table: "BuildTimeLearningData",
                column: "QualityScore");

            migrationBuilder.CreateIndex(
                name: "IX_BuildTimeLearningData_RecordedAt",
                table: "BuildTimeLearningData",
                column: "RecordedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BuildTimeLearningData_SupportComplexity",
                table: "BuildTimeLearningData",
                column: "SupportComplexity");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorEstimateLogs_BuildJobId",
                table: "OperatorEstimateLogs",
                column: "BuildJobId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorEstimateLogs_EstimatedHours",
                table: "OperatorEstimateLogs",
                column: "EstimatedHours");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorEstimateLogs_LoggedAt",
                table: "OperatorEstimateLogs",
                column: "LoggedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PartCompletionLogs_BuildJobId",
                table: "PartCompletionLogs",
                column: "BuildJobId");

            migrationBuilder.CreateIndex(
                name: "IX_PartCompletionLogs_BuildJobId_PartNumber",
                table: "PartCompletionLogs",
                columns: new[] { "BuildJobId", "PartNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_PartCompletionLogs_CompletedAt",
                table: "PartCompletionLogs",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PartCompletionLogs_IsPrimary",
                table: "PartCompletionLogs",
                column: "IsPrimary");

            migrationBuilder.CreateIndex(
                name: "IX_PartCompletionLogs_PartNumber",
                table: "PartCompletionLogs",
                column: "PartNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildTimeLearningData");

            migrationBuilder.DropTable(
                name: "OperatorEstimateLogs");

            migrationBuilder.DropTable(
                name: "PartCompletionLogs");
        }
    }
}
