using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class Add_SLSBuildModeFields_To_MasterPart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns (nullable where appropriate to avoid blocking existing rows)
            migrationBuilder.AddColumn<int>(
                name: "PartsPerBuildSingle",
                table: "MasterParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PartsPerBuildDouble",
                table: "MasterParts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartsPerBuildTriple",
                table: "MasterParts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableDoubleStack",
                table: "MasterParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableTripleStack",
                table: "MasterParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "StageEstimateSingle",
                table: "MasterParts",
                type: "REAL",
                nullable: true);

            // Backfill defaults (defensive: ensure PartsPerBuildSingle is at least 1)
            migrationBuilder.Sql(@"
                UPDATE MasterParts
                SET PartsPerBuildSingle = 1
                WHERE PartsPerBuildSingle IS NULL OR PartsPerBuildSingle < 1;
            ");

            // If legacy double/triple durations have values, enable corresponding flags (fallback parts-per-build = 1 if null)
            migrationBuilder.Sql(@"
                UPDATE MasterParts
                SET EnableDoubleStack = 1,
                    PartsPerBuildDouble = COALESCE(PartsPerBuildDouble, 1)
                WHERE DoubleStackDurationHours IS NOT NULL;

                UPDATE MasterParts
                SET EnableTripleStack = 1,
                    PartsPerBuildTriple = COALESCE(PartsPerBuildTriple, 1)
                WHERE TripleStackDurationHours IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartsPerBuildSingle",
                table: "MasterParts");

            migrationBuilder.DropColumn(
                name: "PartsPerBuildDouble",
                table: "MasterParts");

            migrationBuilder.DropColumn(
                name: "PartsPerBuildTriple",
                table: "MasterParts");

            migrationBuilder.DropColumn(
                name: "EnableDoubleStack",
                table: "MasterParts");

            migrationBuilder.DropColumn(
                name: "EnableTripleStack",
                table: "MasterParts");

            migrationBuilder.DropColumn(
                name: "StageEstimateSingle",
                table: "MasterParts");
        }
    }
}
