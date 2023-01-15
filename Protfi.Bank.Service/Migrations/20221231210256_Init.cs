using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.Service.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "AccountInformationModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankId = table.Column<string>(type: "citext", nullable: true),
                    Number = table.Column<string>(type: "citext", nullable: true),
                    CreationTimeUtcAsUnix = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountInformationModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankId = table.Column<string>(type: "citext", nullable: true),
                    Name = table.Column<string>(type: "citext", nullable: true),
                    Region = table.Column<string>(type: "citext", nullable: true),
                    CreationTimeUtcAsUnix = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankModel", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountInformationModel");

            migrationBuilder.DropTable(
                name: "BankModel");
        }
    }
}
