using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cashregister.Database.Migrations
{
    /// <inheritdoc />
    public partial class RetiredArticlesFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Retired",
                table: "Articles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Retired",
                table: "Articles");
        }
    }
}