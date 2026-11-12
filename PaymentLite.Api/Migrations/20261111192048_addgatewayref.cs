using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentLite.Api.Migrations
{
    /// <inheritdoc />
    public partial class addgatewayref : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GatewayReference",
                table: "Payments",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GatewayReference",
                table: "Payments");
        }
    }
}
