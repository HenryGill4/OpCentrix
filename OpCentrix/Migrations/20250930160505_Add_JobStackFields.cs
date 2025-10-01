using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class Add_JobStackFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActualUnitsPlanned",
                table: "Jobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatusChangeUtc",
                table: "Jobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MasterPartId",
                table: "Jobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OperatorUserId",
                table: "Jobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartsPerBuild",
                table: "Jobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedEndUtc",
                table: "Jobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PlannedStackDurationHours",
                table: "Jobs",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PowderAddedKg",
                table: "Jobs",
                type: "decimal(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PowderMaterial",
                table: "Jobs",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PredecessorJobId",
                table: "Jobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrototypeUnitsPlanned",
                table: "Jobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "StackLevel",
                table: "Jobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_MasterPartId",
                table: "Jobs",
                column: "MasterPartId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OperatorUserId",
                table: "Jobs",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PredecessorJobId",
                table: "Jobs",
                column: "PredecessorJobId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Jobs_PredecessorJobId",
                table: "Jobs",
                column: "PredecessorJobId",
                principalTable: "Jobs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_MasterParts_MasterPartId",
                table: "Jobs",
                column: "MasterPartId",
                principalTable: "MasterParts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Users_OperatorUserId",
                table: "Jobs",
                column: "OperatorUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Jobs_PredecessorJobId",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_MasterParts_MasterPartId",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Users_OperatorUserId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_MasterPartId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_OperatorUserId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_PredecessorJobId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ActualUnitsPlanned",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "LastStatusChangeUtc",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "MasterPartId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OperatorUserId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PartsPerBuild",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PlannedEndUtc",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PlannedStackDurationHours",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PowderAddedKg",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PowderMaterial",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PredecessorJobId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PrototypeUnitsPlanned",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "StackLevel",
                table: "Jobs");
        }
    }
}
