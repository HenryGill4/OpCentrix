using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchStageConfigToMasterPart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DepowderingDurationHours",
                table: "MasterParts",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepowderingPartsPerBatch",
                table: "MasterParts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HeatTreatmentDurationHours",
                table: "MasterParts",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeatTreatmentPartsPerBatch",
                table: "MasterParts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SlsBuildDurationHours",
                table: "MasterParts",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SlsPartsPerBuild",
                table: "MasterParts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WireEdmDurationHours",
                table: "MasterParts",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WireEdmPartsPerSession",
                table: "MasterParts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BuildPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PackageNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    TargetMachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Material = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BuildFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    BuildFilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    BuildFileHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    BuildFileSizeBytes = table.Column<long>(type: "INTEGER", nullable: true),
                    BuildFileUploadedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimatedBuildHours = table.Column<double>(type: "REAL", nullable: false),
                    EstimatedPowderKg = table.Column<double>(type: "REAL", nullable: false),
                    BuildHeightMm = table.Column<double>(type: "REAL", nullable: true),
                    LayerCount = table.Column<int>(type: "INTEGER", nullable: true),
                    LayerThicknessMicrons = table.Column<double>(type: "REAL", nullable: false),
                    SupportComplexity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    PreferredStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRushJob = table.Column<bool>(type: "INTEGER", nullable: false),
                    ScheduledJobId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildPackages_Jobs_ScheduledJobId",
                        column: x => x.ScheduledJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "QCInspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InspectionNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    JobId = table.Column<int>(type: "INTEGER", nullable: true),
                    BuildJobId = table.Column<int>(type: "INTEGER", nullable: true),
                    BuildCohortId = table.Column<int>(type: "INTEGER", nullable: true),
                    PartId = table.Column<int>(type: "INTEGER", nullable: false),
                    InspectionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    TotalQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    PassedQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    ReworkQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    ScrappedQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    InspectorUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    InspectorName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    QualityScore = table.Column<double>(type: "REAL", nullable: true),
                    DimensionalResultsJson = table.Column<string>(type: "TEXT", nullable: true),
                    SurfaceRoughnessRa = table.Column<double>(type: "REAL", nullable: true),
                    DensityPercent = table.Column<double>(type: "REAL", nullable: true),
                    VisualInspectionNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    DefectsJson = table.Column<string>(type: "TEXT", nullable: true),
                    CorrectiveActions = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QCInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QCInspections_BuildJobs_BuildJobId",
                        column: x => x.BuildJobId,
                        principalTable: "BuildJobs",
                        principalColumn: "BuildId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QCInspections_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QCInspections_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QCInspections_Users_InspectorUserId",
                        column: x => x.InspectorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BuildPackageParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BuildPackageId = table.Column<int>(type: "INTEGER", nullable: false),
                    PartId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Orientation = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Flat"),
                    SupportType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    EstimatedHours = table.Column<double>(type: "REAL", nullable: false),
                    PositionIndex = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildPackageParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildPackageParts_BuildPackages_BuildPackageId",
                        column: x => x.BuildPackageId,
                        principalTable: "BuildPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuildPackageParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QCChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QCInspectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Visual"),
                    Specification = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ActualValue = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Result = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "NA"),
                    IsCritical = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QCChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QCChecklistItems_QCInspections_QCInspectionId",
                        column: x => x.QCInspectionId,
                        principalTable: "QCInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildPackageParts_BuildPackageId",
                table: "BuildPackageParts",
                column: "BuildPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildPackageParts_PartId",
                table: "BuildPackageParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildPackages_CreatedAt",
                table: "BuildPackages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BuildPackages_PackageNumber",
                table: "BuildPackages",
                column: "PackageNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildPackages_ScheduledJobId",
                table: "BuildPackages",
                column: "ScheduledJobId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildPackages_Status",
                table: "BuildPackages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BuildPackages_TargetMachineId",
                table: "BuildPackages",
                column: "TargetMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_QCChecklistItems_Category",
                table: "QCChecklistItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_QCChecklistItems_QCInspectionId",
                table: "QCChecklistItems",
                column: "QCInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_QCInspections_BuildJobId",
                table: "QCInspections",
                column: "BuildJobId");

            migrationBuilder.CreateIndex(
                name: "IX_QCInspections_CreatedAt",
                table: "QCInspections",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QCInspections_InspectionNumber",
                table: "QCInspections",
                column: "InspectionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QCInspections_InspectionType",
                table: "QCInspections",
                column: "InspectionType");

            migrationBuilder.CreateIndex(
                name: "IX_QCInspections_InspectorUserId",
                table: "QCInspections",
                column: "InspectorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QCInspections_JobId",
                table: "QCInspections",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_QCInspections_PartId",
                table: "QCInspections",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_QCInspections_Status",
                table: "QCInspections",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildPackageParts");

            migrationBuilder.DropTable(
                name: "QCChecklistItems");

            migrationBuilder.DropTable(
                name: "BuildPackages");

            migrationBuilder.DropTable(
                name: "QCInspections");

            migrationBuilder.DropColumn(
                name: "DepowderingDurationHours",
                table: "MasterParts");

            migrationBuilder.DropColumn(
                name: "DepowderingPartsPerBatch",
                table: "MasterParts");

            migrationBuilder.DropColumn(
                name: "HeatTreatmentDurationHours",
                table: "MasterParts");

            migrationBuilder.DropColumn(
                name: "HeatTreatmentPartsPerBatch",
                table: "MasterParts");

            migrationBuilder.DropColumn(
                name: "SlsBuildDurationHours",
                table: "MasterParts");

            migrationBuilder.DropColumn(
                name: "SlsPartsPerBuild",
                table: "MasterParts");

            migrationBuilder.DropColumn(
                name: "WireEdmDurationHours",
                table: "MasterParts");

            migrationBuilder.DropColumn(
                name: "WireEdmPartsPerSession",
                table: "MasterParts");
        }
    }
}
