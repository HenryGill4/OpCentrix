using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedJobAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Parts_PartId",
                table: "Jobs");

            migrationBuilder.AddColumn<double>(
                name: "AverageActualHours",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AverageDefectRate",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AverageEfficiencyPercent",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AverageQualityScore",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ChangeoverTimeMinutes",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ConsumableMaterials",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')");

            migrationBuilder.AddColumn<string>(
                name: "CustomerPartNumber",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Dimensions",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "EstimatedHours",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastProduced",
                table: "Parts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialCostPerUnit",
                table: "Parts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PartCategory",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PartClass",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferredMachines",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProcessParameters",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "ProcessType",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QualityCheckpoints",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "QualityStandards",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequiredCertifications",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequiredMachineType",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequiredSkills",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequiredTooling",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresInspection",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SetupCost",
                table: "Parts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "SetupTimeMinutes",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "StandardLaborCostPerHour",
                table: "Parts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ToleranceRequirements",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ToolingCost",
                table: "Parts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalJobsCompleted",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalUnitsProduced",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "VolumeM3",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WeightKg",
                table: "Parts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Jobs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Scheduled",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualEnd",
                table: "Jobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualStart",
                table: "Jobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ChangeoverTimeMinutes",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')");

            migrationBuilder.AddColumn<DateTime>(
                name: "CustomerDueDate",
                table: "Jobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerOrderNumber",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DefectQuantity",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "EnergyConsumptionKwh",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "EstimatedHours",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "HoldReason",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsRushJob",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LaborCostPerHour",
                table: "Jobs",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')");

            migrationBuilder.AddColumn<double>(
                name: "MachineUtilizationPercent",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

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

            migrationBuilder.AddColumn<string>(
                name: "PreviousJobPartNumber",
                table: "Jobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<string>(
                name: "ProcessParameters",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<int>(
                name: "ProducedQuantity",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QualityCheckpoints",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "QualityInspector",
                table: "Jobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredMaterials",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequiredSkills",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequiredTooling",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ReworkQuantity",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "SetupTimeMinutes",
                table: "Jobs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "SpecialInstructions",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Supervisor",
                table: "Jobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parts_IsActive",
                table: "Parts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Material",
                table: "Parts",
                column: "Material");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartNumber",
                table: "Parts",
                column: "PartNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_MachineId",
                table: "Jobs",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_MachineId_ScheduledStart",
                table: "Jobs",
                columns: new[] { "MachineId", "ScheduledStart" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PartNumber",
                table: "Jobs",
                column: "PartNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ScheduledStart",
                table: "Jobs",
                column: "ScheduledStart");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status",
                table: "Jobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_JobLogEntries_Action",
                table: "JobLogEntries",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_JobLogEntries_MachineId",
                table: "JobLogEntries",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_JobLogEntries_Timestamp",
                table: "JobLogEntries",
                column: "Timestamp");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Parts_PartId",
                table: "Jobs",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Parts_PartId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Parts_IsActive",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_Material",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_PartNumber",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_MachineId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_MachineId_ScheduledStart",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_PartNumber",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_ScheduledStart",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_Status",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_JobLogEntries_Action",
                table: "JobLogEntries");

            migrationBuilder.DropIndex(
                name: "IX_JobLogEntries_MachineId",
                table: "JobLogEntries");

            migrationBuilder.DropIndex(
                name: "IX_JobLogEntries_Timestamp",
                table: "JobLogEntries");

            migrationBuilder.DropColumn(
                name: "AverageActualHours",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "AverageDefectRate",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "AverageEfficiencyPercent",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "AverageQualityScore",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ChangeoverTimeMinutes",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ConsumableMaterials",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "CustomerPartNumber",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "Dimensions",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "EstimatedHours",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "LastProduced",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "MaterialCostPerUnit",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PartCategory",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PartClass",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PreferredMachines",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ProcessParameters",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ProcessType",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "QualityCheckpoints",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "QualityStandards",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiredCertifications",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiredMachineType",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiredSkills",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiredTooling",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresInspection",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "SetupCost",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "SetupTimeMinutes",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "StandardLaborCostPerHour",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ToleranceRequirements",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ToolingCost",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "TotalJobsCompleted",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "TotalUnitsProduced",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "VolumeM3",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ActualEnd",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ActualStart",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ChangeoverTimeMinutes",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CustomerDueDate",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CustomerOrderNumber",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "DefectQuantity",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "EnergyConsumptionKwh",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "EstimatedHours",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "HoldReason",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "IsRushJob",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "LaborCostPerHour",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "MachineUtilizationPercent",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "MaterialCostPerUnit",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OverheadCostPerHour",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PreviousJobPartNumber",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ProcessParameters",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ProducedQuantity",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "QualityCheckpoints",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "QualityInspector",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RequiredMaterials",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RequiredSkills",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RequiredTooling",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ReworkQuantity",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SetupTimeMinutes",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SpecialInstructions",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Supervisor",
                table: "Jobs");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "Scheduled");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Parts_PartId",
                table: "Jobs",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
