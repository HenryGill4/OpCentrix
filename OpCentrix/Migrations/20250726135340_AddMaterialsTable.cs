using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaterialCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MaterialName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MaterialType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Density = table.Column<double>(type: "REAL", nullable: false),
                    MeltingPointC = table.Column<double>(type: "REAL", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CostPerGram = table.Column<decimal>(type: "TEXT", nullable: false),
                    DefaultLayerThicknessMicrons = table.Column<double>(type: "REAL", nullable: false),
                    DefaultLaserPowerPercent = table.Column<double>(type: "REAL", nullable: false),
                    DefaultScanSpeedMmPerSec = table.Column<double>(type: "REAL", nullable: false),
                    MaterialProperties = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CompatibleMachineTypes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SafetyNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Materials");
        }
    }
}
