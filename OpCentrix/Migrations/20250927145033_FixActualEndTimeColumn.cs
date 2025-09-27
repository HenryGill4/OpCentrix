using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class FixActualEndTimeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ToleranceRequirements",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "+/-0.1mm typical",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldDefaultValue: "±0.1mm typical");

            migrationBuilder.AddColumn<bool>(
                name: "AllowStacking",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "DoubleStackDurationHours",
                table: "Parts",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SingleStackDurationHours",
                table: "Parts",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TripleStackDurationHours",
                table: "Parts",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MachineId",
                table: "OperatingShifts",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MachineOperatorAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System"),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "System")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineOperatorAssignments", x => x.Id);
                    table.CheckConstraint("CK_Assignment_DateRange", "(EffectiveTo IS NULL) OR (EffectiveFrom IS NULL) OR (EffectiveTo >= EffectiveFrom)");
                    table.ForeignKey(
                        name: "FK_MachineOperatorAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperatingShifts_MachineId_DayOfWeek",
                table: "OperatingShifts",
                columns: new[] { "MachineId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_MachineOperatorAssignments_IsActive",
                table: "MachineOperatorAssignments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MachineOperatorAssignments_MachineId",
                table: "MachineOperatorAssignments",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineOperatorAssignments_MachineId_IsPrimary",
                table: "MachineOperatorAssignments",
                columns: new[] { "MachineId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_MachineOperatorAssignments_UserId",
                table: "MachineOperatorAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineOperatorAssignments_UserId_IsActive",
                table: "MachineOperatorAssignments",
                columns: new[] { "UserId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineOperatorAssignments");

            migrationBuilder.DropIndex(
                name: "IX_OperatingShifts_MachineId_DayOfWeek",
                table: "OperatingShifts");

            migrationBuilder.DropColumn(
                name: "AllowStacking",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "DoubleStackDurationHours",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "SingleStackDurationHours",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "TripleStackDurationHours",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "MachineId",
                table: "OperatingShifts");

            migrationBuilder.AlterColumn<string>(
                name: "ToleranceRequirements",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "±0.1mm typical",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldDefaultValue: "+/-0.1mm typical");
        }
    }
}
