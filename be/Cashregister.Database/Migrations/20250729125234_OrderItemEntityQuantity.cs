using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cashregister.Database.Migrations
{
    /// <inheritdoc />
    public partial class OrderItemEntityQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "Quantity",
                table: "OrderItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "OrderItems");
        }
    }
}