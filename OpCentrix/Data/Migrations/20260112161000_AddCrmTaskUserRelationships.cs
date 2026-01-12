using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmTaskUserRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add foreign key constraints for CrmTasks user relationships
            migrationBuilder.CreateIndex(
                name: "IX_CrmTasks_CreatedByUserId",
                table: "CrmTasks",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmTasks_Users_CreatedByUserId",
                table: "CrmTasks",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmTasks_Users_AssignedToUserId",
                table: "CrmTasks",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrmTasks_Users_AssignedToUserId",
                table: "CrmTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmTasks_Users_CreatedByUserId",
                table: "CrmTasks");

            migrationBuilder.DropIndex(
                name: "IX_CrmTasks_CreatedByUserId",
                table: "CrmTasks");
        }
    }
}