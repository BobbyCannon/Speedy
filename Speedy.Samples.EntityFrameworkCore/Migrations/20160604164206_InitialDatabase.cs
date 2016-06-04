#region References

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#endregion

namespace Speedy.Samples.EntityFrameworkCore.Migrations
{
	public partial class InitialDatabase : Migration
	{
		#region Methods

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				"FoodRelationships");

			migrationBuilder.DropTable(
				"GroupMembers");

			migrationBuilder.DropTable(
				"LogEvents");

			migrationBuilder.DropTable(
				"SyncTombstones");

			migrationBuilder.DropTable(
				"Foods");

			migrationBuilder.DropTable(
				"Groups");

			migrationBuilder.DropTable(
				"People");

			migrationBuilder.DropTable(
				"Addresses");
		}

		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				"Addresses",
				table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					City = table.Column<string>(nullable: false),
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					Line1 = table.Column<string>(nullable: false),
					Line2 = table.Column<string>(nullable: false),
					LinkedAddressId = table.Column<int>(nullable: true),
					LinkedAddressSyncId = table.Column<Guid>(nullable: true),
					ModifiedOn = table.Column<DateTime>("datetime2", nullable: false),
					Postal = table.Column<string>(nullable: false),
					State = table.Column<string>(nullable: false),
					SyncId = table.Column<Guid>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Addresses", x => x.Id);
					table.ForeignKey(
						"FK_Addresses_Addresses_LinkedAddressId",
						x => x.LinkedAddressId,
						"Addresses",
						"Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				"Foods",
				table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					ModifiedOn = table.Column<DateTime>("datetime2", nullable: false),
					Name = table.Column<string>(nullable: false)
				},
				constraints: table => { table.PrimaryKey("PK_Foods", x => x.Id); });

			migrationBuilder.CreateTable(
				"Groups",
				table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					Description = table.Column<string>(nullable: false),
					ModifiedOn = table.Column<DateTime>("datetime2", nullable: false),
					Name = table.Column<string>(nullable: false),
					SyncId = table.Column<Guid>(nullable: false)
				},
				constraints: table => { table.PrimaryKey("PK_Groups", x => x.Id); });

			migrationBuilder.CreateTable(
				"LogEvents",
				table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					Message = table.Column<string>(nullable: true)
				},
				constraints: table => { table.PrimaryKey("PK_LogEvents", x => x.Id); });

			migrationBuilder.CreateTable(
				"SyncTombstones",
				table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					ReferenceId = table.Column<string>(nullable: false),
					SyncId = table.Column<Guid>(nullable: false),
					TypeName = table.Column<string>(nullable: false)
				},
				constraints: table => { table.PrimaryKey("PK_SyncTombstones", x => x.Id); });

			migrationBuilder.CreateTable(
				"People",
				table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					AddressId = table.Column<int>(nullable: false),
					AddressSyncId = table.Column<Guid>(nullable: false),
					BillingAddressId = table.Column<int>(nullable: true),
					BillingAddressSyncId = table.Column<Guid>(nullable: true),
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					ModifiedOn = table.Column<DateTime>("datetime2", nullable: false),
					Name = table.Column<string>(nullable: false),
					SyncId = table.Column<Guid>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_People", x => x.Id);
					table.ForeignKey(
						"FK_People_Addresses_AddressId",
						x => x.AddressId,
						"Addresses",
						"Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						"FK_People_Addresses_BillingAddressId",
						x => x.BillingAddressId,
						"Addresses",
						"Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				"FoodRelationships",
				table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					ChildId = table.Column<int>(nullable: false),
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					ModifiedOn = table.Column<DateTime>("datetime2", nullable: false),
					ParentId = table.Column<int>(nullable: false),
					Quantity = table.Column<decimal>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_FoodRelationships", x => x.Id);
					table.ForeignKey(
						"FK_FoodRelationships_Foods_ChildId",
						x => x.ChildId,
						"Foods",
						"Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						"FK_FoodRelationships_Foods_ParentId",
						x => x.ParentId,
						"Foods",
						"Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				"GroupMembers",
				table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					CreatedOn = table.Column<DateTime>("datetime2", nullable: false),
					GroupId = table.Column<int>(nullable: false),
					GroupSyncId = table.Column<Guid>(nullable: false),
					MemberId = table.Column<int>(nullable: false),
					MemberSyncId = table.Column<Guid>(nullable: false),
					ModifiedOn = table.Column<DateTime>("datetime2", nullable: false),
					Role = table.Column<string>(nullable: false),
					SyncId = table.Column<Guid>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_GroupMembers", x => x.Id);
					table.ForeignKey(
						"FK_GroupMembers_Groups_GroupId",
						x => x.GroupId,
						"Groups",
						"Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						"FK_GroupMembers_People_MemberId",
						x => x.MemberId,
						"People",
						"Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				"IX_Addresses_LinkedAddressId",
				"Addresses",
				"LinkedAddressId");

			migrationBuilder.CreateIndex(
				"IX_FoodRelationships_ChildId",
				"FoodRelationships",
				"ChildId");

			migrationBuilder.CreateIndex(
				"IX_FoodRelationships_ParentId",
				"FoodRelationships",
				"ParentId");

			migrationBuilder.CreateIndex(
				"IX_GroupMembers_GroupId",
				"GroupMembers",
				"GroupId");

			migrationBuilder.CreateIndex(
				"IX_GroupMembers_MemberId",
				"GroupMembers",
				"MemberId");

			migrationBuilder.CreateIndex(
				"IX_People_AddressId",
				"People",
				"AddressId");

			migrationBuilder.CreateIndex(
				"IX_People_BillingAddressId",
				"People",
				"BillingAddressId");

			migrationBuilder.CreateIndex(
				"IX_People_Name",
				"People",
				"Name",
				unique: true);
		}

		#endregion
	}
}