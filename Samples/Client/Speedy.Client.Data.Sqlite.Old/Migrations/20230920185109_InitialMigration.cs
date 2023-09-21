using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Speedy.Client.Data.Sqlite.Old.Migrations
{
    public partial class InitialMigration : Migration
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
                    AddressId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AddressCreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddressIsDeleted = table.Column<bool>(nullable: false),
                    AddressModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddressSyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressCity = table.Column<string>(nullable: false),
                    AddressLineOne = table.Column<string>(maxLength: 128, nullable: false),
                    AddressLineTwo = table.Column<string>(maxLength: 128, nullable: false),
                    AddressPostal = table.Column<string>(maxLength: 25, nullable: false),
                    AddressState = table.Column<string>(maxLength: 25, nullable: false),
                    AddressLastClientUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.AddressId);
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
                    Message = table.Column<string>(maxLength: 256, nullable: false),
                    LastClientUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    Value = table.Column<string>(nullable: false),
                    LastClientUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                schema: "dbo",
                columns: table => new
                {
                    AccountId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountCreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccountIsDeleted = table.Column<bool>(nullable: false),
                    AccountModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccountSyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountAddressId = table.Column<long>(nullable: false),
                    AccountAddressSyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountEmailAddress = table.Column<string>(maxLength: 128, nullable: false),
                    AccountLastClientUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccountName = table.Column<string>(nullable: false),
                    AccountRoles = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_Accounts_Addresses_AccountAddressId",
                        column: x => x.AccountAddressId,
                        principalSchema: "dbo",
                        principalTable: "Addresses",
                        principalColumn: "AddressId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountAddressId",
                schema: "dbo",
                table: "Accounts",
                column: "AccountAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_LastClientUpdate",
                schema: "dbo",
                table: "Accounts",
                column: "AccountLastClientUpdate");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SyncId",
                schema: "dbo",
                table: "Accounts",
                column: "AccountSyncId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_LastClientUpdate",
                schema: "dbo",
                table: "Addresses",
                column: "AddressLastClientUpdate");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_SyncId",
                schema: "dbo",
                table: "Addresses",
                column: "AddressSyncId",
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

            migrationBuilder.CreateIndex(
                name: "IX_Settings_LastClientUpdate",
                schema: "dbo",
                table: "Settings",
                column: "LastClientUpdate");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_SyncId",
                schema: "dbo",
                table: "Settings",
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
                name: "Settings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Addresses",
                schema: "dbo");
        }
    }
}
