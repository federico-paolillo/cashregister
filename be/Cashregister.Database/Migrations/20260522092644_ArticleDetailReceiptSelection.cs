using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cashregister.Database.Migrations
{
    /// <inheritdoc />
    public partial class ArticleDetailReceiptSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PrintDetailReceipt",
                table: "Articles",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrintDetailReceipt",
                table: "Articles");
        }
    }
}