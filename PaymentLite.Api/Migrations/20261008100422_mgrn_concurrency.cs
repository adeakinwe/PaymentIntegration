using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentLite.Api.Migrations
{
    /// <inheritdoc />
    public partial class mgrn_concurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Accounts",
                type: "int",
                rowVersion: true,
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("""
                CREATE TRIGGER TR_Accounts_Version
                BEFORE UPDATE ON Accounts
                FOR EACH ROW
                SET NEW.Version = OLD.Version + 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS TR_Accounts_Version;
                """);
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Accounts");
        }
    }
}
