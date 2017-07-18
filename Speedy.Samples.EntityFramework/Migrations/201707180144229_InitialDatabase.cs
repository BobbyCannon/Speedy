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
			DropForeignKey("dbo.Addresses", "LinkedAddressId", "dbo.Addresses");
			DropForeignKey("dbo.Pets", "TypeId", "dbo.PetType");
			DropForeignKey("dbo.Pets", "OwnerId", "dbo.People");
			DropForeignKey("dbo.GroupMembers", "MemberId", "dbo.People");
			DropForeignKey("dbo.GroupMembers", "GroupId", "dbo.Groups");
			DropForeignKey("dbo.People", "BillingAddressId", "dbo.Addresses");
			DropForeignKey("dbo.People", "AddressId", "dbo.Addresses");
			DropIndex("dbo.SyncTombstones", new[] { "CreatedOn" });
			DropIndex("dbo.SyncTombstones", "IX_SyncTombstones_ReferenceId_TypeName");
			DropIndex("dbo.FoodRelationships", new[] { "ParentId" });
			DropIndex("dbo.FoodRelationships", new[] { "ChildId" });
			DropIndex("dbo.Pets", new[] { "TypeId" });
			DropIndex("dbo.Pets", new[] { "OwnerId" });
			DropIndex("dbo.GroupMembers", new[] { "MemberId" });
			DropIndex("dbo.GroupMembers", new[] { "GroupId" });
			DropIndex("dbo.People", new[] { "SyncId" });
			DropIndex("dbo.People", new[] { "Name" });
			DropIndex("dbo.People", new[] { "BillingAddressId" });
			DropIndex("dbo.People", new[] { "AddressId" });
			DropIndex("dbo.Addresses", new[] { "SyncId" });
			DropIndex("dbo.Addresses", new[] { "LinkedAddressId" });
			DropTable("dbo.SyncTombstones");
			DropTable("dbo.LogEvents");
			DropTable("dbo.FoodRelationships");
			DropTable("dbo.Foods");
			DropTable("dbo.PetType");
			DropTable("dbo.Pets");
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
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Addresses", t => t.LinkedAddressId)
				.Index(t => t.LinkedAddressId)
				.Index(t => t.SyncId, unique: true);

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
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Addresses", t => t.AddressId)
				.ForeignKey("dbo.Addresses", t => t.BillingAddressId)
				.Index(t => t.AddressId)
				.Index(t => t.BillingAddressId)
				.Index(t => t.Name, unique: true)
				.Index(t => t.SyncId, unique: true);

			CreateTable(
					"dbo.GroupMembers",
					c => new
					{
						Id = c.Int(false, true),
						GroupId = c.Int(false),
						GroupSyncId = c.Guid(false),
						MemberId = c.Int(false),
						MemberSyncId = c.Guid(false),
						Role = c.String(false, 4000),
						ModifiedOn = c.DateTime(false, 7, storeType: "datetime2"),
						CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
					})
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Groups", t => t.GroupId, true)
				.ForeignKey("dbo.People", t => t.MemberId, true)
				.Index(t => t.GroupId)
				.Index(t => t.MemberId);

			CreateTable(
					"dbo.Groups",
					c => new
					{
						Id = c.Int(false, true),
						Description = c.String(false, 4000),
						Name = c.String(false, 256),
						ModifiedOn = c.DateTime(false, 7, storeType: "datetime2"),
						CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
					})
				.PrimaryKey(t => t.Id);

			CreateTable(
					"dbo.Pets",
					c => new
					{
						Name = c.String(false, 128),
						OwnerId = c.Int(false),
						CreatedOn = c.DateTime(false, 7, storeType: "datetime2"),
						ModifiedOn = c.DateTime(false, 7, storeType: "datetime2"),
						TypeId = c.String(false, 25)
					})
				.PrimaryKey(t => new { t.Name, t.OwnerId })
				.ForeignKey("dbo.People", t => t.OwnerId)
				.ForeignKey("dbo.PetType", t => t.TypeId)
				.Index(t => t.OwnerId)
				.Index(t => t.TypeId);

			CreateTable(
					"dbo.PetType",
					c => new
					{
						PetTypeId = c.String(false, 25),
						Type = c.String(maxLength: 200)
					})
				.PrimaryKey(t => t.PetTypeId);

			CreateTable(
					"dbo.Foods",
					c => new
					{
						Id = c.Int(false, true),
						Name = c.String(false, 256),
						ModifiedOn = c.DateTime(false, 7, storeType: "datetime2"),
						CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
					})
				.PrimaryKey(t => t.Id);

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
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Foods", t => t.ChildId)
				.ForeignKey("dbo.Foods", t => t.ParentId)
				.Index(t => t.ChildId)
				.Index(t => t.ParentId);

			CreateTable(
					"dbo.LogEvents",
					c => new
					{
						Id = c.String(false, 250),
						Message = c.String(maxLength: 4000),
						CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
					})
				.PrimaryKey(t => t.Id);

			CreateTable(
					"dbo.SyncTombstones",
					c => new
					{
						Id = c.Long(false, true),
						ReferenceId = c.String(false, 128),
						SyncId = c.Guid(false),
						TypeName = c.String(false, 768),
						CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
					})
				.PrimaryKey(t => t.Id)
				.Index(t => new { t.ReferenceId, t.TypeName }, "IX_SyncTombstones_ReferenceId_TypeName")
				.Index(t => t.CreatedOn);
		}

		#endregion
	}
}