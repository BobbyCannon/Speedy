﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Speedy.Samples.Sqlite.Migrations
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
                    City = table.Column<string>(unicode: false, maxLength: 256, nullable: false),
                    Line1 = table.Column<string>(unicode: false, maxLength: 256, nullable: false),
                    Line2 = table.Column<string>(unicode: false, maxLength: 256, nullable: false),
                    LinkedAddressId = table.Column<long>(nullable: true),
                    LinkedAddressSyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Postal = table.Column<string>(unicode: false, maxLength: 128, nullable: false),
                    State = table.Column<string>(unicode: false, maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Addresses_LinkedAddressId",
                        column: x => x.LinkedAddressId,
                        principalSchema: "dbo",
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Foods",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(unicode: false, maxLength: 4000, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogEvents",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(unicode: false, maxLength: 250, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(unicode: false, nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PetType",
                schema: "dbo",
                columns: table => new
                {
                    PetTypeId = table.Column<string>(unicode: false, maxLength: 25, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(unicode: false, maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetType", x => x.PetTypeId);
                });

            migrationBuilder.CreateTable(
                name: "People",
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
                    BillingAddressId = table.Column<long>(nullable: true),
                    BillingAddressSyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                    table.ForeignKey(
                        name: "FK_People_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalSchema: "dbo",
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_People_Addresses_BillingAddressId",
                        column: x => x.BillingAddressId,
                        principalSchema: "dbo",
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FoodRelationships",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChildId = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParentId = table.Column<int>(nullable: false),
                    Quantity = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodRelationships_Foods_ChildId",
                        column: x => x.ChildId,
                        principalSchema: "dbo",
                        principalTable: "Foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FoodRelationships_Foods_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "dbo",
                        principalTable: "Foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupMembers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GroupId = table.Column<int>(nullable: false),
                    GroupSyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<int>(nullable: false),
                    MemberSyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Role = table.Column<string>(unicode: false, maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMembers_Groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "dbo",
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMembers_People_MemberId",
                        column: x => x.MemberId,
                        principalSchema: "dbo",
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pets",
                schema: "dbo",
                columns: table => new
                {
                    Name = table.Column<string>(unicode: false, maxLength: 128, nullable: false),
                    OwnerId = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TypeId = table.Column<string>(unicode: false, maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => new { x.Name, x.OwnerId });
                    table.ForeignKey(
                        name: "FK_Pets_People_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "dbo",
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pets_PetType_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "dbo",
                        principalTable: "PetType",
                        principalColumn: "PetTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_LinkedAddressId",
                schema: "dbo",
                table: "Addresses",
                column: "LinkedAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_SyncId",
                schema: "dbo",
                table: "Addresses",
                column: "SyncId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoodRelationships_ChildId",
                schema: "dbo",
                table: "FoodRelationships",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodRelationships_ParentId",
                schema: "dbo",
                table: "FoodRelationships",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_GroupId",
                schema: "dbo",
                table: "GroupMembers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_MemberId",
                schema: "dbo",
                table: "GroupMembers",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_People_AddressId",
                schema: "dbo",
                table: "People",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_People_BillingAddressId",
                schema: "dbo",
                table: "People",
                column: "BillingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_People_Name",
                schema: "dbo",
                table: "People",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_People_SyncId",
                schema: "dbo",
                table: "People",
                column: "SyncId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pets_OwnerId",
                schema: "dbo",
                table: "Pets",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_TypeId",
                schema: "dbo",
                table: "Pets",
                column: "TypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodRelationships",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "GroupMembers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LogEvents",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Pets",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Foods",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Groups",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "People",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PetType",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Addresses",
                schema: "dbo");
        }
    }
}
