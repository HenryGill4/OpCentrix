using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedBuildJobTimeTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CustomerOrderNumber",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "BuildFileHash",
                table: "BuildJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BuildHeight",
                table: "BuildJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefectCount",
                table: "BuildJobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLearningBuild",
                table: "BuildJobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LaserOnTime",
                table: "BuildJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LayerCount",
                table: "BuildJobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LessonsLearned",
                table: "BuildJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MachinePerformanceNotes",
                table: "BuildJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OperatorActualHours",
                table: "BuildJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatorBuildAssessment",
                table: "BuildJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OperatorEstimatedHours",
                table: "BuildJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartOrientations",
                table: "BuildJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostProcessingNeeded",
                table: "BuildJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PowerConsumption",
                table: "BuildJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportComplexity",
                table: "BuildJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeFactors",
                table: "BuildJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalPartsInBuild",
                table: "BuildJobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuildFileHash",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "BuildHeight",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "DefectCount",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "IsLearningBuild",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "LaserOnTime",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "LayerCount",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "LessonsLearned",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "MachinePerformanceNotes",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "OperatorActualHours",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "OperatorBuildAssessment",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "OperatorEstimatedHours",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "PartOrientations",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "PostProcessingNeeded",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "PowerConsumption",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "SupportComplexity",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "TimeFactors",
                table: "BuildJobs");

            migrationBuilder.DropColumn(
                name: "TotalPartsInBuild",
                table: "BuildJobs");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerOrderNumber",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldDefaultValue: "");
        }
    }
}
