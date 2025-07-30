using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class AddPrototypeTrackingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DefaultSetupMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 30),
                    DefaultHourlyRate = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 85.00m),
                    RequiresQualityCheck = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    RequiresApproval = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    AllowSkip = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsOptional = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    RequiredRole = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionStages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrototypeJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartId = table.Column<int>(type: "INTEGER", nullable: false),
                    PrototypeNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CustomerOrderNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    RequestedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Standard"),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "InProgress"),
                    TotalActualCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    TotalEstimatedCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CostVariancePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TotalActualHours = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    TotalEstimatedHours = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    TimeVariancePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "INTEGER", nullable: true),
                    AdminReviewStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    AdminReviewBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AdminReviewDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AdminReviewNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrototypeJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrototypeJobs_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssemblyComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PrototypeJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    ComponentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ComponentPartNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ComponentDescription = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    QuantityRequired = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    QuantityUsed = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    UnitCost = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Supplier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SupplierPartNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LeadTimeDays = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Needed"),
                    OrderDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InspectionRequired = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    InspectionPassed = table.Column<bool>(type: "INTEGER", nullable: true),
                    InspectionNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssemblyComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssemblyComponents_PrototypeJobs_PrototypeJobId",
                        column: x => x.PrototypeJobId,
                        principalTable: "PrototypeJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionStageExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PrototypeJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductionStageId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "NotStarted"),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimatedHours = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    ActualHours = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    SetupHours = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    RunHours = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    ActualCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    MaterialCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    LaborCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    OverheadCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    QualityCheckRequired = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    QualityCheckPassed = table.Column<bool>(type: "INTEGER", nullable: true),
                    QualityCheckBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    QualityCheckDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    QualityNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ProcessParameters = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, defaultValue: "{}"),
                    Issues = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Improvements = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ExecutedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ReviewedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionStageExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionStageExecutions_ProductionStages_ProductionStageId",
                        column: x => x.ProductionStageId,
                        principalTable: "ProductionStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionStageExecutions_PrototypeJobs_PrototypeJobId",
                        column: x => x.PrototypeJobId,
                        principalTable: "PrototypeJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrototypeTimeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductionStageExecutionId = table.Column<int>(type: "INTEGER", nullable: false),
                    LogDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ElapsedMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    ActivityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ActivityDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Employee = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IssuesEncountered = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ImprovementSuggestions = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrototypeTimeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrototypeTimeLogs_ProductionStageExecutions_ProductionStageExecutionId",
                        column: x => x.ProductionStageExecutionId,
                        principalTable: "ProductionStageExecutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyComponents_ComponentType",
                table: "AssemblyComponents",
                column: "ComponentType");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyComponents_IsActive",
                table: "AssemblyComponents",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyComponents_PrototypeJobId",
                table: "AssemblyComponents",
                column: "PrototypeJobId");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyComponents_Status",
                table: "AssemblyComponents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyComponents_Supplier",
                table: "AssemblyComponents",
                column: "Supplier");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageExecutions_CompletionDate",
                table: "ProductionStageExecutions",
                column: "CompletionDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageExecutions_ExecutedBy",
                table: "ProductionStageExecutions",
                column: "ExecutedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageExecutions_ProductionStageId",
                table: "ProductionStageExecutions",
                column: "ProductionStageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageExecutions_PrototypeJobId",
                table: "ProductionStageExecutions",
                column: "PrototypeJobId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageExecutions_PrototypeJobId_ProductionStageId",
                table: "ProductionStageExecutions",
                columns: new[] { "PrototypeJobId", "ProductionStageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageExecutions_StartDate",
                table: "ProductionStageExecutions",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStageExecutions_Status",
                table: "ProductionStageExecutions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStages_DisplayOrder",
                table: "ProductionStages",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStages_IsActive",
                table: "ProductionStages",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStages_Name",
                table: "ProductionStages",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStages_RequiredRole",
                table: "ProductionStages",
                column: "RequiredRole");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeJobs_AdminReviewStatus",
                table: "PrototypeJobs",
                column: "AdminReviewStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeJobs_IsActive",
                table: "PrototypeJobs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeJobs_PartId",
                table: "PrototypeJobs",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeJobs_Priority",
                table: "PrototypeJobs",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeJobs_PrototypeNumber",
                table: "PrototypeJobs",
                column: "PrototypeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeJobs_RequestDate",
                table: "PrototypeJobs",
                column: "RequestDate");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeJobs_RequestedBy",
                table: "PrototypeJobs",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeJobs_Status",
                table: "PrototypeJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeTimeLogs_ActivityType",
                table: "PrototypeTimeLogs",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeTimeLogs_Employee",
                table: "PrototypeTimeLogs",
                column: "Employee");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeTimeLogs_LogDate",
                table: "PrototypeTimeLogs",
                column: "LogDate");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeTimeLogs_ProductionStageExecutionId",
                table: "PrototypeTimeLogs",
                column: "ProductionStageExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeTimeLogs_StartTime",
                table: "PrototypeTimeLogs",
                column: "StartTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssemblyComponents");

            migrationBuilder.DropTable(
                name: "PrototypeTimeLogs");

            migrationBuilder.DropTable(
                name: "ProductionStageExecutions");

            migrationBuilder.DropTable(
                name: "ProductionStages");

            migrationBuilder.DropTable(
                name: "PrototypeJobs");
        }
    }
}
