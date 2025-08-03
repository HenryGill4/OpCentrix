using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedStageManagementTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StageDependency_NoSelfReference",
                table: "StageDependencies");

            migrationBuilder.AddColumn<int>(
                name: "WorkflowTemplateId",
                table: "ProductionStageExecutions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowTemplateId",
                table: "PartStageRequirements",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductionStageDependencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DependentStageId = table.Column<int>(type: "INTEGER", nullable: false),
                    PrerequisiteStageId = table.Column<int>(type: "INTEGER", nullable: false),
                    DependencyType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "FinishToStart"),
                    DelayHours = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    IsOptional = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Condition = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionStageDependencies", x => x.Id);
                    table.CheckConstraint("CK_ProductionStageDependency_NoSelfReference", "DependentStageId != PrerequisiteStageId");
                    table.ForeignKey(
                        name: "FK_ProductionStageDependencies_ProductionStages_DependentStageId",
                        column: x => x.DependentStageId,
                        principalTable: "ProductionStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionStageDependencies_ProductionStages_PrerequisiteStageId",
                        column: x => x.PrerequisiteStageId,
                        principalTable: "ProductionStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResourcePools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ResourceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Machine"),
                    ResourceConfiguration = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false, defaultValue: "[]"),
                    MaxConcurrentAllocations = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    AutoAssign = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    AssignmentCriteria = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourcePools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Complexity = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Medium"),
                    EstimatedDurationHours = table.Column<double>(type: "REAL", nullable: false, defaultValue: 8.0),
                    EstimatedCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0.00m),
                    StageConfiguration = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false, defaultValue: "[]"),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTemplates", x => x.Id);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_JobStageDependency_NoSelfReference",
                table: "StageDependencies",
                sql: "DependentStageId != RequiredStageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageExecutions_WorkflowTemplateId",
                table: "ProductionStageExecutions",
                column: "WorkflowTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PartStageRequirements_WorkflowTemplateId",
                table: "PartStageRequirements",
                column: "WorkflowTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageDependencies_DependencyType",
                table: "ProductionStageDependencies",
                column: "DependencyType");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageDependencies_DependentStageId",
                table: "ProductionStageDependencies",
                column: "DependentStageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageDependencies_DependentStageId_PrerequisiteStageId",
                table: "ProductionStageDependencies",
                columns: new[] { "DependentStageId", "PrerequisiteStageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageDependencies_IsActive",
                table: "ProductionStageDependencies",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageDependencies_PrerequisiteStageId",
                table: "ProductionStageDependencies",
                column: "PrerequisiteStageId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePools_AutoAssign",
                table: "ResourcePools",
                column: "AutoAssign");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePools_IsActive",
                table: "ResourcePools",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePools_Name",
                table: "ResourcePools",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePools_ResourceType",
                table: "ResourcePools",
                column: "ResourceType");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePools_ResourceType_IsActive",
                table: "ResourcePools",
                columns: new[] { "ResourceType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplates_Category",
                table: "WorkflowTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplates_Category_IsActive",
                table: "WorkflowTemplates",
                columns: new[] { "Category", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplates_Complexity",
                table: "WorkflowTemplates",
                column: "Complexity");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplates_IsActive",
                table: "WorkflowTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplates_Name",
                table: "WorkflowTemplates",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_PartStageRequirements_WorkflowTemplates_WorkflowTemplateId",
                table: "PartStageRequirements",
                column: "WorkflowTemplateId",
                principalTable: "WorkflowTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionStageExecutions_WorkflowTemplates_WorkflowTemplateId",
                table: "ProductionStageExecutions",
                column: "WorkflowTemplateId",
                principalTable: "WorkflowTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartStageRequirements_WorkflowTemplates_WorkflowTemplateId",
                table: "PartStageRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionStageExecutions_WorkflowTemplates_WorkflowTemplateId",
                table: "ProductionStageExecutions");

            migrationBuilder.DropTable(
                name: "ProductionStageDependencies");

            migrationBuilder.DropTable(
                name: "ResourcePools");

            migrationBuilder.DropTable(
                name: "WorkflowTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_JobStageDependency_NoSelfReference",
                table: "StageDependencies");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStageExecutions_WorkflowTemplateId",
                table: "ProductionStageExecutions");

            migrationBuilder.DropIndex(
                name: "IX_PartStageRequirements_WorkflowTemplateId",
                table: "PartStageRequirements");

            migrationBuilder.DropColumn(
                name: "WorkflowTemplateId",
                table: "ProductionStageExecutions");

            migrationBuilder.DropColumn(
                name: "WorkflowTemplateId",
                table: "PartStageRequirements");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StageDependency_NoSelfReference",
                table: "StageDependencies",
                sql: "DependentStageId != RequiredStageId");
        }
    }
}
