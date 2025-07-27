using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class FixPartsTableSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobStages_Machines_MachineId",
                table: "JobStages");

            migrationBuilder.DropForeignKey(
                name: "FK_MachineCapabilities_Machines_MachineId1",
                table: "MachineCapabilities");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Machines_MachineId",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_MachineCapabilities_MachineId1",
                table: "MachineCapabilities");

            migrationBuilder.DropColumn(
                name: "MachineId1",
                table: "MachineCapabilities");

            migrationBuilder.AddColumn<int>(
                name: "MachineId1",
                table: "JobStages",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parts_CreatedDate",
                table: "Parts",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Industry",
                table: "Parts",
                column: "Industry");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_IsActive",
                table: "Parts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Material",
                table: "Parts",
                column: "Material");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartCategory",
                table: "Parts",
                column: "PartCategory");

            migrationBuilder.CreateIndex(
                name: "IX_JobStages_MachineId1",
                table: "JobStages",
                column: "MachineId1");

            migrationBuilder.AddForeignKey(
                name: "FK_JobStages_Machines_MachineId1",
                table: "JobStages",
                column: "MachineId1",
                principalTable: "Machines",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobStages_Machines_MachineId1",
                table: "JobStages");

            migrationBuilder.DropIndex(
                name: "IX_Parts_CreatedDate",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_Industry",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_IsActive",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_Material",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_PartCategory",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_JobStages_MachineId1",
                table: "JobStages");

            migrationBuilder.DropColumn(
                name: "MachineId1",
                table: "JobStages");

            migrationBuilder.AddColumn<int>(
                name: "MachineId1",
                table: "MachineCapabilities",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Machines_MachineId",
                table: "Machines",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineCapabilities_MachineId1",
                table: "MachineCapabilities",
                column: "MachineId1");

            migrationBuilder.AddForeignKey(
                name: "FK_JobStages_Machines_MachineId",
                table: "JobStages",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "MachineId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MachineCapabilities_Machines_MachineId1",
                table: "MachineCapabilities",
                column: "MachineId1",
                principalTable: "Machines",
                principalColumn: "Id");
        }
    }
}
