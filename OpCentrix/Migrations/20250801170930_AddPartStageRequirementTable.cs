using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class AddPartStageRequirementTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartStageRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductionStageId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExecutionOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    EstimatedHours = table.Column<double>(type: "REAL", nullable: true),
                    SetupTimeMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    StageParameters = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    SpecialInstructions = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    QualityRequirements = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    RequiredMaterials = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    RequiredTooling = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AllowParallel = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBlocking = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequirementNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartStageRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartStageRequirements_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartStageRequirements_ProductionStages_ProductionStageId",
                        column: x => x.ProductionStageId,
                        principalTable: "ProductionStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartStageRequirements_PartId",
                table: "PartStageRequirements",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_PartStageRequirements_ProductionStageId",
                table: "PartStageRequirements",
                column: "ProductionStageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartStageRequirements");
        }
    }
}
