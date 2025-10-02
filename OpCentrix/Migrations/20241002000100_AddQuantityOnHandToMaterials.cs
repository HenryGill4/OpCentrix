using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <summary>
    /// Adds QuantityOnHandKg column to Materials and back-fills initial inventory values.
    /// </summary>
    public partial class AddQuantityOnHandToMaterials : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new column with default 0 (SQLite stores decimal as TEXT/REAL depending on provider conventions)
            migrationBuilder.AddColumn<decimal>(
                name: "QuantityOnHandKg",
                table: "Materials",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            // Back-fill known current inventory levels (adjust as needed)
            migrationBuilder.Sql("UPDATE Materials SET QuantityOnHandKg = 52.40 WHERE MaterialCode = 'TI64-G5';");
            migrationBuilder.Sql("UPDATE Materials SET QuantityOnHandKg = 28.60 WHERE MaterialCode = 'TI64-G23';");
            migrationBuilder.Sql("UPDATE Materials SET QuantityOnHandKg = 38.70 WHERE MaterialCode = 'IN718';");
            migrationBuilder.Sql("UPDATE Materials SET QuantityOnHandKg = 0 WHERE MaterialCode = 'SS316L';");
            migrationBuilder.Sql("UPDATE Materials SET QuantityOnHandKg = 0 WHERE MaterialCode = 'ALSI10MG';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantityOnHandKg",
                table: "Materials");
        }
    }
}
