using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cashregister.Database.Migrations
{
    /// <inheritdoc />
    public partial class ArticleQuantityAvailable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "QuantityAvailable",
                table: "Articles",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantityAvailable",
                table: "Articles");
        }
    }
}