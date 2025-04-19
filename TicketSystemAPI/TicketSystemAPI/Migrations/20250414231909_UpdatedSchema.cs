using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSystemAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUnlimited",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Tickets",
                newName: "Variant");

            migrationBuilder.RenameColumn(
                name: "RideLimit",
                table: "Tickets",
                newName: "MaxRides");

            migrationBuilder.AddColumn<string>(
                name: "DiscountCode",
                table: "Tickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDate",
                table: "Tickets",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountCode",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PurchaseDate",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "Variant",
                table: "Tickets",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "MaxRides",
                table: "Tickets",
                newName: "RideLimit");

            migrationBuilder.AddColumn<bool>(
                name: "IsUnlimited",
                table: "Tickets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
