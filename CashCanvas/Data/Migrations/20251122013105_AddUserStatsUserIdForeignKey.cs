using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashCanvas.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStatsUserIdForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "UserStats",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_UserStats_UserId",
                table: "UserStats",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserStats_AspNetUsers_UserId",
                table: "UserStats",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserStats_AspNetUsers_UserId",
                table: "UserStats");

            migrationBuilder.DropIndex(
                name: "IX_UserStats_UserId",
                table: "UserStats");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserStats");
        }
    }
}
