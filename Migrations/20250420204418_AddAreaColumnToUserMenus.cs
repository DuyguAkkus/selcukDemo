using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SelcukDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaColumnToUserMenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "UserMenus",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "UserMenus");
        }
    }
}
