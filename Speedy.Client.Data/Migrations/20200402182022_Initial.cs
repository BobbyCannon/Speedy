using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Speedy.Client.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Addresses",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    City = table.Column<string>(unicode: false, nullable: true),
                    LastClientUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Line1 = table.Column<string>(unicode: false, maxLength: 128, nullable: false),
                    Line2 = table.Column<string>(unicode: false, maxLength: 128, nullable: false),
                    Postal = table.Column<string>(unicode: false, maxLength: 25, nullable: false),
                    State = table.Column<string>(unicode: false, maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogEvents",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<int>(nullable: false),
                    Message = table.Column<string>(unicode: false, maxLength: 256, nullable: false),
                    LastClientUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressId = table.Column<long>(nullable: false),
                    AddressSyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailAddress = table.Column<string>(unicode: false, maxLength: 128, nullable: false),
                    LastClientUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(unicode: false, nullable: false),
                    Roles = table.Column<string>(unicode: false, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalSchema: "dbo",
                        principalTable: "Addresses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AddressId",
                schema: "dbo",
                table: "Accounts",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_LastClientUpdate",
                schema: "dbo",
                table: "Accounts",
                column: "LastClientUpdate");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SyncId",
                schema: "dbo",
                table: "Accounts",
                column: "SyncId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_LastClientUpdate",
                schema: "dbo",
                table: "Addresses",
                column: "LastClientUpdate");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_SyncId",
                schema: "dbo",
                table: "Addresses",
                column: "SyncId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogEvents_LastClientUpdate",
                schema: "dbo",
                table: "LogEvents",
                column: "LastClientUpdate");

            migrationBuilder.CreateIndex(
                name: "IX_LogEvents_SyncId",
                schema: "dbo",
                table: "LogEvents",
                column: "SyncId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LogEvents",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Addresses",
                schema: "dbo");
        }
    }
}
