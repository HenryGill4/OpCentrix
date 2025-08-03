using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AllowParallel",
                table: "PartStageRequirements",
                newName: "RequiresSpecificMachine");

            migrationBuilder.AddColumn<bool>(
                name: "AllowParallelExecution",
                table: "ProductionStages",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AssignedMachineIds",
                table: "ProductionStages",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ProductionStages",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.AddColumn<string>(
                name: "CustomFieldsConfig",
                table: "ProductionStages",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<double>(
                name: "DefaultDurationHours",
                table: "ProductionStages",
                type: "REAL",
                nullable: false,
                defaultValue: 1.0);

            migrationBuilder.AddColumn<string>(
                name: "DefaultMachineId",
                table: "ProductionStages",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultMaterialCost",
                table: "ProductionStages",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "ProductionStages",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "ProductionStages",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "ProductionStages",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresMachineAssignment",
                table: "ProductionStages",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StageColor",
                table: "ProductionStages",
                type: "TEXT",
                maxLength: 7,
                nullable: false,
                defaultValue: "#007bff");

            migrationBuilder.AddColumn<string>(
                name: "StageIcon",
                table: "ProductionStages",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "fas fa-cogs");

            migrationBuilder.AlterColumn<int>(
                name: "SetupTimeMinutes",
                table: "PartStageRequirements",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<bool>(
                name: "AllowParallelExecution",
                table: "PartStageRequirements",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AssignedMachineId",
                table: "PartStageRequirements",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomFieldValues",
                table: "PartStageRequirements",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRateOverride",
                table: "PartStageRequirements",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialCost",
                table: "PartStageRequirements",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PreferredMachineIds",
                table: "PartStageRequirements",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirearmType",
                table: "PartClassifications",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuildCohortId",
                table: "Jobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StageOrder",
                table: "Jobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalStages",
                table: "Jobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkflowStage",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BuildCohorts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BuildJobId = table.Column<int>(type: "INTEGER", nullable: true),
                    BuildNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PartCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Material = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "Ti-6Al-4V Grade 5"),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "InProgress"),
                    CompletedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildCohorts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildCohorts_BuildJobs_BuildJobId",
                        column: x => x.BuildJobId,
                        principalTable: "BuildJobs",
                        principalColumn: "BuildId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "JobStageHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductionStageId = table.Column<int>(type: "INTEGER", nullable: true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StageName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Operator = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    StageHours = table.Column<double>(type: "REAL", nullable: true),
                    QualityResult = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobStageHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobStageHistories_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobStageHistories_ProductionStages_ProductionStageId",
                        column: x => x.ProductionStageId,
                        principalTable: "ProductionStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStages_Department",
                table: "ProductionStages",
                column: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStages_DisplayOrder_IsActive",
                table: "ProductionStages",
                columns: new[] { "DisplayOrder", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStages_RequiresMachineAssignment",
                table: "ProductionStages",
                column: "RequiresMachineAssignment");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStages_StageColor",
                table: "ProductionStages",
                column: "StageColor");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_BuildCohortId",
                table: "Jobs",
                column: "BuildCohortId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_BuildCohortId_StageOrder",
                table: "Jobs",
                columns: new[] { "BuildCohortId", "StageOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_StageOrder",
                table: "Jobs",
                column: "StageOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_WorkflowStage",
                table: "Jobs",
                column: "WorkflowStage");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_WorkflowStage_Status",
                table: "Jobs",
                columns: new[] { "WorkflowStage", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildCohorts_BuildJobId",
                table: "BuildCohorts",
                column: "BuildJobId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildCohorts_BuildNumber",
                table: "BuildCohorts",
                column: "BuildNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildCohorts_CompletedDate",
                table: "BuildCohorts",
                column: "CompletedDate");

            migrationBuilder.CreateIndex(
                name: "IX_BuildCohorts_CreatedDate",
                table: "BuildCohorts",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_BuildCohorts_Material",
                table: "BuildCohorts",
                column: "Material");

            migrationBuilder.CreateIndex(
                name: "IX_BuildCohorts_Status",
                table: "BuildCohorts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_JobStageHistories_Action",
                table: "JobStageHistories",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_JobStageHistories_JobId",
                table: "JobStageHistories",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobStageHistories_JobId_Timestamp",
                table: "JobStageHistories",
                columns: new[] { "JobId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_JobStageHistories_MachineId",
                table: "JobStageHistories",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_JobStageHistories_Operator",
                table: "JobStageHistories",
                column: "Operator");

            migrationBuilder.CreateIndex(
                name: "IX_JobStageHistories_ProductionStageId",
                table: "JobStageHistories",
                column: "ProductionStageId");

            migrationBuilder.CreateIndex(
                name: "IX_JobStageHistories_StageName",
                table: "JobStageHistories",
                column: "StageName");

            migrationBuilder.CreateIndex(
                name: "IX_JobStageHistories_Timestamp",
                table: "JobStageHistories",
                column: "Timestamp");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_BuildCohorts_BuildCohortId",
                table: "Jobs",
                column: "BuildCohortId",
                principalTable: "BuildCohorts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_BuildCohorts_BuildCohortId",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "BuildCohorts");

            migrationBuilder.DropTable(
                name: "JobStageHistories");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStages_Department",
                table: "ProductionStages");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStages_DisplayOrder_IsActive",
                table: "ProductionStages");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStages_RequiresMachineAssignment",
                table: "ProductionStages");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStages_StageColor",
                table: "ProductionStages");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_BuildCohortId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_BuildCohortId_StageOrder",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_StageOrder",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_WorkflowStage",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_WorkflowStage_Status",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "AllowParallelExecution",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "AssignedMachineIds",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "CustomFieldsConfig",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "DefaultDurationHours",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "DefaultMachineId",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "DefaultMaterialCost",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "RequiresMachineAssignment",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "StageColor",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "StageIcon",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "AllowParallelExecution",
                table: "PartStageRequirements");

            migrationBuilder.DropColumn(
                name: "AssignedMachineId",
                table: "PartStageRequirements");

            migrationBuilder.DropColumn(
                name: "CustomFieldValues",
                table: "PartStageRequirements");

            migrationBuilder.DropColumn(
                name: "HourlyRateOverride",
                table: "PartStageRequirements");

            migrationBuilder.DropColumn(
                name: "MaterialCost",
                table: "PartStageRequirements");

            migrationBuilder.DropColumn(
                name: "PreferredMachineIds",
                table: "PartStageRequirements");

            migrationBuilder.DropColumn(
                name: "BuildCohortId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "StageOrder",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "TotalStages",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "WorkflowStage",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "RequiresSpecificMachine",
                table: "PartStageRequirements",
                newName: "AllowParallel");

            migrationBuilder.AlterColumn<int>(
                name: "SetupTimeMinutes",
                table: "PartStageRequirements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirearmType",
                table: "PartClassifications",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValue: "");
        }
    }
}
