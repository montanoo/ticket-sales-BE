using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSystemAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_Tickets_Payments",
                table: "tickets");

            migrationBuilder.AlterColumn<decimal>(
                name: "basePrice",
                table: "tickettypes",
                type: "decimal(10)",
                precision: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,30)",
                oldPrecision: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "purchaseTime",
                table: "tickets",
                type: "datetime(1)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(1)");

            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "tickets",
                type: "decimal(10)",
                precision: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,30)",
                oldPrecision: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "paymentId",
                table: "tickets",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "expirationTime",
                table: "tickets",
                type: "datetime(1)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(1)");

            migrationBuilder.AlterColumn<decimal>(
                name: "amount",
                table: "payments",
                type: "decimal(10)",
                precision: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,30)",
                oldPrecision: 10);

            migrationBuilder.AddForeignKey(
                name: "fk_Tickets_Payments",
                table: "tickets",
                column: "paymentId",
                principalTable: "payments",
                principalColumn: "paymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_Tickets_Payments",
                table: "tickets");

            migrationBuilder.AlterColumn<decimal>(
                name: "basePrice",
                table: "tickettypes",
                type: "decimal(10,30)",
                precision: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10)",
                oldPrecision: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "purchaseTime",
                table: "tickets",
                type: "datetime(1)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "tickets",
                type: "decimal(10,30)",
                precision: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10)",
                oldPrecision: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "paymentId",
                table: "tickets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "expirationTime",
                table: "tickets",
                type: "datetime(1)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "amount",
                table: "payments",
                type: "decimal(10,30)",
                precision: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10)",
                oldPrecision: 10);

            migrationBuilder.AddForeignKey(
                name: "fk_Tickets_Payments",
                table: "tickets",
                column: "paymentId",
                principalTable: "payments",
                principalColumn: "paymentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
