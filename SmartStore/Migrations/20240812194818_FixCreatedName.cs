using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmarthStore.Migrations
{
    /// <inheritdoc />
    public partial class FixCreatedName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "Products",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Products",
                newName: "CreateAt");
        }
    }
}
