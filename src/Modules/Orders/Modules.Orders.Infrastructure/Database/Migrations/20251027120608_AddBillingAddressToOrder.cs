using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Orders.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingAddressToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "shipping_address_zip_code",
                schema: "orders",
                table: "orders",
                newName: "shipping_zip_code");

            migrationBuilder.RenameColumn(
                name: "shipping_address_street",
                schema: "orders",
                table: "orders",
                newName: "shipping_street");

            migrationBuilder.RenameColumn(
                name: "shipping_address_state",
                schema: "orders",
                table: "orders",
                newName: "shipping_state");

            migrationBuilder.RenameColumn(
                name: "shipping_address_city",
                schema: "orders",
                table: "orders",
                newName: "shipping_city");

            migrationBuilder.AddColumn<string>(
                name: "billing_city",
                schema: "orders",
                table: "orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "billing_state",
                schema: "orders",
                table: "orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "billing_street",
                schema: "orders",
                table: "orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "billing_zip_code",
                schema: "orders",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "billing_city",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "billing_state",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "billing_street",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "billing_zip_code",
                schema: "orders",
                table: "orders");

            migrationBuilder.RenameColumn(
                name: "shipping_zip_code",
                schema: "orders",
                table: "orders",
                newName: "shipping_address_zip_code");

            migrationBuilder.RenameColumn(
                name: "shipping_street",
                schema: "orders",
                table: "orders",
                newName: "shipping_address_street");

            migrationBuilder.RenameColumn(
                name: "shipping_state",
                schema: "orders",
                table: "orders",
                newName: "shipping_address_state");

            migrationBuilder.RenameColumn(
                name: "shipping_city",
                schema: "orders",
                table: "orders",
                newName: "shipping_address_city");
        }
    }
}
