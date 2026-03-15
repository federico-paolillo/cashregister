using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cashregister.Database.Migrations
{
    /// <inheritdoc />
    public partial class TotalOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TotalOverride",
                table: "Orders",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalOverride",
                table: "Orders");
        }
    }
}
