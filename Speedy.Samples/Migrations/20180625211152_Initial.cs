#region References

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#endregion

namespace Speedy.Samples.Migrations
{
	public partial class Initial : Migration
	{
		#region Methods

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				"FoodRelationships",
				"dbo");

			migrationBuilder.DropTable(
				"GroupMembers",
				"dbo");

			migrationBuilder.DropTable(
				"LogEvents",
				"dbo");

			migrationBuilder.DropTable(
				"Pets",
				"dbo");

			migrationBuilder.DropTable(
				"SyncTombstones",
				"dbo");

			migrationBuilder.DropTable(
				"Foods",
				"dbo");

			migrationBuilder.DropTable(
				"Groups",
				"dbo");

			migrationBuilder.DropTable(
				"People",
				"dbo");

			migrationBuilder.DropTable(
				"PetType",
				"dbo");

			migrationBuilder.DropTable(
				"Addresses",
				"dbo");
		}

		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.EnsureSchema(
				"dbo");

			migrationBuilder.CreateTable(
				"Addresses",
				schema: "dbo",
				columns: table => new
				{
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					ModifiedOn = table.Column<DateTime>("datetime2", nullable: false),
					SyncId = table.Column<Guid>("uniqueidentifier", nullable: false),
					City = table.Column<string>("nvarchar(256)", nullable: false),
					Id = table.Column<int>("int", nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					Line1 = table.Column<string>("nvarchar(256)", nullable: false),
					Line2 = table.Column<string>("nvarchar(256)", nullable: false),
					LinkedAddressId = table.Column<int>("int", nullable: true),
					LinkedAddressSyncId = table.Column<Guid>("uniqueidentifier", nullable: true),
					Postal = table.Column<string>("nvarchar(128)", nullable: false),
					State = table.Column<string>("nvarchar(128)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Addresses", x => x.Id);
					table.ForeignKey(
						"FK_Addresses_Addresses_LinkedAddressId",
						x => x.LinkedAddressId,
						principalSchema: "dbo",
						principalTable: "Addresses",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				"Foods",
				schema: "dbo",
				columns: table => new
				{
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					ModifiedOn = table.Column<DateTime>("datetime2", nullable: false),
					Id = table.Column<int>("int", nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					Name = table.Column<string>("nvarchar(256)", nullable: false)
				},
				constraints: table => { table.PrimaryKey("PK_Foods", x => x.Id); });

			migrationBuilder.CreateTable(
				"Groups",
				schema: "dbo",
				columns: table => new
				{
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					ModifiedOn = table.Column<DateTime>("datetime2", nullable: false),
					Description = table.Column<string>("nvarchar(4000)", nullable: false),
					Id = table.Column<int>("int", nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					Name = table.Column<string>("nvarchar(256)", nullable: false)
				},
				constraints: table => { table.PrimaryKey("PK_Groups", x => x.Id); });

			migrationBuilder.CreateTable(
				"LogEvents",
				schema: "dbo",
				columns: table => new
				{
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					Id = table.Column<string>("nvarchar(250)", nullable: false),
					Message = table.Column<string>("nvarchar(4000)", nullable: true)
				},
				constraints: table => { table.PrimaryKey("PK_LogEvents", x => x.Id); });

			migrationBuilder.CreateTable(
				"PetType",
				schema: "dbo",
				columns: table => new
				{
					PetTypeId = table.Column<string>("nvarchar(25)", nullable: false),
					Type = table.Column<string>("nvarchar(200)", nullable: true)
				},
				constraints: table => { table.PrimaryKey("PK_PetType", x => x.PetTypeId); });

			migrationBuilder.CreateTable(
				"SyncTombstones",
				schema: "dbo",
				columns: table => new
				{
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					Id = table.Column<long>("bigint", nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					ReferenceId = table.Column<string>("nvarchar(128)", nullable: false),
					SyncId = table.Column<Guid>("uniqueidentifier", nullable: false),
					TypeName = table.Column<string>("nvarchar(768)", nullable: false)
				},
				constraints: table => { table.PrimaryKey("PK_SyncTombstones", x => x.Id); });

			migrationBuilder.CreateTable(
				"People",
				schema: "dbo",
				columns: table => new
				{
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					ModifiedOn = table.Column<DateTime>("datetime2", nullable: false),
					SyncId = table.Column<Guid>("uniqueidentifier", nullable: false),
					AddressId = table.Column<int>("int", nullable: false),
					AddressSyncId = table.Column<Guid>("uniqueidentifier", nullable: false),
					BillingAddressId = table.Column<int>("int", nullable: true),
					BillingAddressSyncId = table.Column<Guid>("uniqueidentifier", nullable: true),
					Id = table.Column<int>("int", nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					Name = table.Column<string>("nvarchar(256)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_People", x => x.Id);
					table.ForeignKey(
						"FK_People_Addresses_AddressId",
						x => x.AddressId,
						principalSchema: "dbo",
						principalTable: "Addresses",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						"FK_People_Addresses_BillingAddressId",
						x => x.BillingAddressId,
						principalSchema: "dbo",
						principalTable: "Addresses",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				"FoodRelationships",
				schema: "dbo",
				columns: table => new
				{
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					ModifiedOn = table.Column<DateTime>("datetime2", nullable: false),
					ChildId = table.Column<int>("int", nullable: false),
					Id = table.Column<int>("int", nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					ParentId = table.Column<int>("int", nullable: false),
					Quantity = table.Column<decimal>("decimal", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_FoodRelationships", x => x.Id);
					table.ForeignKey(
						"FK_FoodRelationships_Foods_ChildId",
						x => x.ChildId,
						principalSchema: "dbo",
						principalTable: "Foods",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						"FK_FoodRelationships_Foods_ParentId",
						x => x.ParentId,
						principalSchema: "dbo",
						principalTable: "Foods",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				"GroupMembers",
				schema: "dbo",
				columns: table => new
				{
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					ModifiedOn = table.Column<DateTime>("datetime2", nullable: false),
					GroupId = table.Column<int>("int", nullable: false),
					GroupSyncId = table.Column<Guid>("uniqueidentifier", nullable: false),
					Id = table.Column<int>("int", nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					MemberId = table.Column<int>("int", nullable: false),
					MemberSyncId = table.Column<Guid>("uniqueidentifier", nullable: false),
					Role = table.Column<string>("nvarchar(4000)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_GroupMembers", x => x.Id);
					table.ForeignKey(
						"FK_GroupMembers_Groups_GroupId",
						x => x.GroupId,
						principalSchema: "dbo",
						principalTable: "Groups",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						"FK_GroupMembers_People_MemberId",
						x => x.MemberId,
						principalSchema: "dbo",
						principalTable: "People",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				"Pets",
				schema: "dbo",
				columns: table => new
				{
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					ModifiedOn = table.Column<DateTime>("datetime2", nullable: false),
					Name = table.Column<string>("nvarchar(128)", nullable: false),
					OwnerId = table.Column<int>("int", nullable: false),
					TypeId = table.Column<string>("nvarchar(25)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Pets", x => new { x.Name, x.OwnerId });
					table.ForeignKey(
						"FK_Pets_People_OwnerId",
						x => x.OwnerId,
						principalSchema: "dbo",
						principalTable: "People",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						"FK_Pets_PetType_TypeId",
						x => x.TypeId,
						principalSchema: "dbo",
						principalTable: "PetType",
						principalColumn: "PetTypeId",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateIndex(
				"IX_LinkedAddressId",
				schema: "dbo",
				table: "Addresses",
				column: "LinkedAddressId");

			migrationBuilder.CreateIndex(
				"IX_SyncId",
				schema: "dbo",
				table: "Addresses",
				column: "SyncId",
				unique: true);

			migrationBuilder.CreateIndex(
				"IX_ChildId",
				schema: "dbo",
				table: "FoodRelationships",
				column: "ChildId");

			migrationBuilder.CreateIndex(
				"IX_ParentId",
				schema: "dbo",
				table: "FoodRelationships",
				column: "ParentId");

			migrationBuilder.CreateIndex(
				"IX_GroupId",
				schema: "dbo",
				table: "GroupMembers",
				column: "GroupId");

			migrationBuilder.CreateIndex(
				"IX_MemberId",
				schema: "dbo",
				table: "GroupMembers",
				column: "MemberId");

			migrationBuilder.CreateIndex(
				"IX_AddressId",
				schema: "dbo",
				table: "People",
				column: "AddressId");

			migrationBuilder.CreateIndex(
				"IX_BillingAddressId",
				schema: "dbo",
				table: "People",
				column: "BillingAddressId");

			migrationBuilder.CreateIndex(
				"IX_Name",
				schema: "dbo",
				table: "People",
				column: "Name",
				unique: true);

			migrationBuilder.CreateIndex(
				"IX_SyncId",
				schema: "dbo",
				table: "People",
				column: "SyncId",
				unique: true);

			migrationBuilder.CreateIndex(
				"IX_OwnerId",
				schema: "dbo",
				table: "Pets",
				column: "OwnerId");

			migrationBuilder.CreateIndex(
				"IX_TypeId",
				schema: "dbo",
				table: "Pets",
				column: "TypeId");

			migrationBuilder.CreateIndex(
				"IX_CreatedOn",
				schema: "dbo",
				table: "SyncTombstones",
				column: "CreatedOn");

			migrationBuilder.CreateIndex(
				"IX_SyncTombstones_TypeName_ReferenceId",
				schema: "dbo",
				table: "SyncTombstones",
				columns: new[] { "TypeName", "ReferenceId" });
		}

		#endregion
	}
}