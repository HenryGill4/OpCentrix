using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    public partial class AddScheduleAdjustments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TriggerJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    AffectedJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OriginalEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NewStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NewEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ShiftMinutes = table.Column<double>(type: "REAL", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleAdjustments", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleAdjustments");
        }
    }
}
