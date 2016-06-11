#region References

using System.Data.Entity.Migrations;

#endregion

namespace Speedy.Samples.EntityFramework.Migrations
{
	public partial class InitialDatabase : DbMigration
	{
		#region Methods

		public override void Down()
		{
			DropForeignKey("dbo.FoodRelationships", "ParentId", "dbo.Foods");
			DropForeignKey("dbo.FoodRelationships", "ChildId", "dbo.Foods");
			DropForeignKey("dbo.GroupMembers", "MemberId", "dbo.People");
			DropForeignKey("dbo.GroupMembers", "GroupId", "dbo.Groups");
			DropForeignKey("dbo.People", "BillingAddressId", "dbo.Addresses");
			DropForeignKey("dbo.People", "AddressId", "dbo.Addresses");
			DropForeignKey("dbo.Addresses", "LinkedAddressId", "dbo.Addresses");
			DropIndex("dbo.SyncTombstones", new[] { "CreatedOn" });
			DropIndex("dbo.SyncTombstones", "IX_SyncTombstones_ReferenceId_TypeName");
			DropIndex("dbo.FoodRelationships", new[] { "ParentId" });
			DropIndex("dbo.FoodRelationships", new[] { "ChildId" });
			DropIndex("dbo.Groups", new[] { "ModifiedOn" });
			DropIndex("dbo.Groups", new[] { "Name" });
			DropIndex("dbo.GroupMembers", new[] { "ModifiedOn" });
			DropIndex("dbo.GroupMembers", new[] { "MemberId" });
			DropIndex("dbo.GroupMembers", new[] { "GroupId" });
			DropIndex("dbo.People", new[] { "ModifiedOn" });
			DropIndex("dbo.People", new[] { "Name" });
			DropIndex("dbo.People", new[] { "BillingAddressId" });
			DropIndex("dbo.People", new[] { "AddressId" });
			DropIndex("dbo.Addresses", new[] { "ModifiedOn" });
			DropIndex("dbo.Addresses", new[] { "LinkedAddressId" });
			DropIndex("dbo.Addresses", new[] { "Line1" });
			DropTable("dbo.SyncTombstones");
			DropTable("dbo.LogEvents");
			DropTable("dbo.FoodRelationships");
			DropTable("dbo.Foods");
			DropTable("dbo.Groups");
			DropTable("dbo.GroupMembers");
			DropTable("dbo.People");
			DropTable("dbo.Addresses");
		}

		public override void Up()
		{
			CreateTable(
				"dbo.Addresses",
				c => new
				{
					Id = c.Int(false, true),
					City = c.String(false, 256),
					Line1 = c.String(false, 256),
					Line2 = c.String(false, 256),
					LinkedAddressId = c.Int(),
					LinkedAddressSyncId = c.Guid(),
					Postal = c.String(false, 128),
					State = c.String(false, 128),
					SyncId = c.Guid(false),
					ModifiedOn = c.DateTime(false, 7, storeType: "datetime2"),
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
				})
				.PrimaryKey(x => x.Id)
				.ForeignKey("dbo.Addresses", x => x.LinkedAddressId)
				.Index(x => x.Line1)
				.Index(x => x.LinkedAddressId)
				.Index(x => x.ModifiedOn);

			CreateTable(
				"dbo.People",
				c => new
				{
					Id = c.Int(false, true),
					AddressId = c.Int(false),
					AddressSyncId = c.Guid(false),
					BillingAddressId = c.Int(),
					BillingAddressSyncId = c.Guid(),
					Name = c.String(false, 256),
					SyncId = c.Guid(false),
					ModifiedOn = c.DateTime(false, 7, storeType: "datetime2"),
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
				})
				.PrimaryKey(x => x.Id)
				.ForeignKey("dbo.Addresses", x => x.AddressId)
				.ForeignKey("dbo.Addresses", x => x.BillingAddressId)
				.Index(x => x.AddressId)
				.Index(x => x.BillingAddressId)
				.Index(x => x.Name, unique: true)
				.Index(x => x.ModifiedOn);

			CreateTable(
				"dbo.GroupMembers",
				c => new
				{
					Id = c.Int(false, true),
					GroupId = c.Int(false),
					GroupSyncId = c.Guid(false),
					MemberId = c.Int(false),
					MemberSyncId = c.Guid(false),
					Role = c.String(false),
					SyncId = c.Guid(false),
					ModifiedOn = c.DateTime(false, 7, storeType: "datetime2"),
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
				})
				.PrimaryKey(x => x.Id)
				.ForeignKey("dbo.Groups", x => x.GroupId, true)
				.ForeignKey("dbo.People", x => x.MemberId, true)
				.Index(x => x.GroupId)
				.Index(x => x.MemberId)
				.Index(x => x.ModifiedOn);

			CreateTable(
				"dbo.Groups",
				c => new
				{
					Id = c.Int(false, true),
					Description = c.String(false),
					Name = c.String(false, 256),
					SyncId = c.Guid(false),
					ModifiedOn = c.DateTime(false, 7, storeType: "datetime2"),
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
				})
				.PrimaryKey(x => x.Id)
				.Index(x => x.Name, unique: true)
				.Index(x => x.ModifiedOn);

			CreateTable(
				"dbo.Foods",
				c => new
				{
					Id = c.Int(false, true),
					Name = c.String(false, 256),
					ModifiedOn = c.DateTime(false, 7, storeType: "datetime2"),
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
				})
				.PrimaryKey(x => x.Id);

			CreateTable(
				"dbo.FoodRelationships",
				c => new
				{
					Id = c.Int(false, true),
					ChildId = c.Int(false),
					ParentId = c.Int(false),
					Quantity = c.Decimal(false, 18, 2),
					ModifiedOn = c.DateTime(false, 7, storeType: "datetime2"),
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
				})
				.PrimaryKey(x => x.Id)
				.ForeignKey("dbo.Foods", x => x.ChildId)
				.ForeignKey("dbo.Foods", x => x.ParentId)
				.Index(x => x.ChildId)
				.Index(x => x.ParentId);

			CreateTable(
				"dbo.LogEvents",
				c => new
				{
					Id = c.Int(false, true),
					Message = c.String(),
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
				})
				.PrimaryKey(x => x.Id);

			CreateTable(
				"dbo.SyncTombstones",
				c => new
				{
					Id = c.Int(false, true),
					ReferenceId = c.String(false, 128),
					SyncId = c.Guid(false),
					TypeName = c.String(false, 768),
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
				})
				.PrimaryKey(x => x.Id)
				.Index(t => new { t.ReferenceId, t.TypeName }, "IX_SyncTombstones_ReferenceId_TypeName")
				.Index(x => x.CreatedOn);
		}

		#endregion
	}
}