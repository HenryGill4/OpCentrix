using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    public partial class AddMachineComponentsAndExtendedMaintenanceRule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns to MaintenanceRules
            migrationBuilder.AddColumn<int>(
                name: "MachineComponentId",
                table: "MaintenanceRules",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionStageId",
                table: "MaintenanceRules",
                type: "INTEGER",
                nullable: true);

            // Create MachineComponents table
            migrationBuilder.CreateTable(
                name: "MachineComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 100),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "General"),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true, defaultValue: "cog"),
                    ColorCode = table.Column<string>(type: "TEXT", maxLength: 7, nullable: true, defaultValue: "#6B7280"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineComponents", x => x.Id);
                });

            // Indexes for MachineComponents
            migrationBuilder.CreateIndex(
                name: "IX_MachineComponents_MachineId",
                table: "MachineComponents",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineComponents_MachineId_IsActive",
                table: "MachineComponents",
                columns: new[] { "MachineId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MachineComponents_MachineId_Category",
                table: "MachineComponents",
                columns: new[] { "MachineId", "Category" });

            // Indexes for new maintenance rule columns
            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRules_MachineComponentId",
                table: "MaintenanceRules",
                column: "MachineComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRules_ProductionStageId",
                table: "MaintenanceRules",
                column: "ProductionStageId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRules_MachineComponentId_IsActive",
                table: "MaintenanceRules",
                columns: new[] { "MachineComponentId", "IsActive" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRules_MachineComponentId",
                table: "MaintenanceRules");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRules_ProductionStageId",
                table: "MaintenanceRules");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRules_MachineComponentId_IsActive",
                table: "MaintenanceRules");

            migrationBuilder.DropTable(
                name: "MachineComponents");

            migrationBuilder.DropColumn(
                name: "MachineComponentId",
                table: "MaintenanceRules");

            migrationBuilder.DropColumn(
                name: "ProductionStageId",
                table: "MaintenanceRules");
        }
    }
}
