using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class BTPartsSystemEnhancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ATFClassification",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalWorkflow",
                table: "Parts",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "BTBackPressurePSI",
                table: "Parts",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BTBafflePosition",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BTCaliberCompatibility",
                table: "Parts",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BTComponentType",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BTFirearmCategory",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "BTLicensingCost",
                table: "Parts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BTQualitySpecification",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "BTSoundReductionDB",
                table: "Parts",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BTSuppressorType",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BTTestingProtocol",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BTThreadPitch",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BatchControlMethod",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChildComponents",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ComplianceCost",
                table: "Parts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DocumentationCost",
                table: "Parts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "EARClassification",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExportControlNotes",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FFLRequirements",
                table: "Parts",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ITARCategory",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAssemblyComponent",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubAssembly",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturingStage",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxBatchSize",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ParentComponents",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "ProofTestPressure",
                table: "Parts",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresATFForm1",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresATFForm4",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresAssembly",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresBTProofTesting",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresBackPressureTesting",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresCNCMachining",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresComplianceApproval",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresEDMOperations",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresEngineeringApproval",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresExportLicense",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresFinishing",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresQualityApproval",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresSLSPrinting",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresSoundTesting",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresTaxStamp",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresThreadVerification",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresTraceabilityDocuments",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresUniqueSerialNumber",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumberFormat",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StageDetails",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StageOrder",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxStampAmount",
                table: "Parts",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TestingCost",
                table: "Parts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "WorkflowTemplate",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ATFClassification",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ApprovalWorkflow",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BTBackPressurePSI",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BTBafflePosition",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BTCaliberCompatibility",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BTComponentType",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BTFirearmCategory",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BTLicensingCost",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BTQualitySpecification",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BTSoundReductionDB",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BTSuppressorType",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BTTestingProtocol",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BTThreadPitch",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BatchControlMethod",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ChildComponents",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ComplianceCost",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "DocumentationCost",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "EARClassification",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ExportControlNotes",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "FFLRequirements",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ITARCategory",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "IsAssemblyComponent",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "IsSubAssembly",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ManufacturingStage",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "MaxBatchSize",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ParentComponents",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ProofTestPressure",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresATFForm1",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresATFForm4",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresAssembly",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresBTProofTesting",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresBackPressureTesting",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresCNCMachining",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresComplianceApproval",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresEDMOperations",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresEngineeringApproval",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresExportLicense",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresFinishing",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresQualityApproval",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresSLSPrinting",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresSoundTesting",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresTaxStamp",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresThreadVerification",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresTraceabilityDocuments",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresUniqueSerialNumber",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "SerialNumberFormat",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "StageDetails",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "StageOrder",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "TaxStampAmount",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "TestingCost",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "WorkflowTemplate",
                table: "Parts");
        }
    }
}
