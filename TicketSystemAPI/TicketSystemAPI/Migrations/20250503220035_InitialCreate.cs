using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSystemAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    paymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    amount = table.Column<decimal>(type: "decimal(10)", precision: 10, nullable: false),
                    method = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, collation: "utf8mb3_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb3")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.paymentId);
                })
                .Annotation("MySql:CharSet", "utf8mb3")
                .Annotation("Relational:Collation", "utf8mb3_general_ci");

            migrationBuilder.CreateTable(
                name: "tickettypes",
                columns: table => new
                {
                    typeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, collation: "utf8mb3_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb3"),
                    baseDurationDays = table.Column<int>(type: "int", nullable: true),
                    baseRideLimit = table.Column<int>(type: "int", nullable: true),
                    basePrice = table.Column<decimal>(type: "decimal(10)", precision: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.typeId);
                })
                .Annotation("MySql:CharSet", "utf8mb3")
                .Annotation("Relational:Collation", "utf8mb3_general_ci");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    userId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    email = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, collation: "utf8mb3_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb3"),
                    password = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, collation: "utf8mb3_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb3")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.userId);
                })
                .Annotation("MySql:CharSet", "utf8mb3")
                .Annotation("Relational:Collation", "utf8mb3_general_ci");

            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    ticketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    purchaseTime = table.Column<DateTime>(type: "datetime(1)", nullable: false),
                    typeId = table.Column<int>(type: "int", nullable: false),
                    expirationTime = table.Column<DateTime>(type: "datetime(1)", nullable: false),
                    ridesTaken = table.Column<uint>(type: "int unsigned", nullable: true),
                    rideLimit = table.Column<uint>(type: "int unsigned", nullable: true),
                    price = table.Column<decimal>(type: "decimal(10)", precision: 10, nullable: true),
                    discountCode = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true, collation: "utf8mb3_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb3"),
                    userId = table.Column<int>(type: "int", nullable: false),
                    paymentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ticketId);
                    table.ForeignKey(
                        name: "fk_Tickets_1",
                        column: x => x.userId,
                        principalTable: "users",
                        principalColumn: "userId");
                    table.ForeignKey(
                        name: "fk_Tickets_3",
                        column: x => x.typeId,
                        principalTable: "tickettypes",
                        principalColumn: "typeId");
                    table.ForeignKey(
                        name: "fk_Tickets_Payments",
                        column: x => x.paymentId,
                        principalTable: "payments",
                        principalColumn: "paymentId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb3")
                .Annotation("Relational:Collation", "utf8mb3_general_ci");

            migrationBuilder.CreateIndex(
                name: "paymentId_UNIQUE",
                table: "payments",
                column: "paymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "fk_Tickets_1_idx",
                table: "tickets",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "fk_Tickets_3_idx",
                table: "tickets",
                column: "typeId");

            migrationBuilder.CreateIndex(
                name: "fk_Tickets_Payments",
                table: "tickets",
                column: "paymentId");

            migrationBuilder.CreateIndex(
                name: "ticketId_UNIQUE",
                table: "tickets",
                column: "ticketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "typeId_UNIQUE",
                table: "tickettypes",
                column: "typeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "userId_UNIQUE",
                table: "users",
                column: "userId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tickets");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "tickettypes");

            migrationBuilder.DropTable(
                name: "payments");
        }
    }
}
