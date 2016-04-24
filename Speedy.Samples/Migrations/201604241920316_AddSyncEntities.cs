#region References

using System.Data.Entity.Migrations;

#endregion

namespace Speedy.Samples.Migrations
{
	public partial class AddSyncEntities : DbMigration
	{
		#region Methods

		public override void Down()
		{
			DropForeignKey("dbo.People", "AddressId", "dbo.Addresses");
			DropForeignKey("dbo.GroupMembers", "MemberId", "dbo.People");
			DropForeignKey("dbo.GroupMembers", "GroupId", "dbo.Groups");
			DropIndex("dbo.SyncTombstones", new[] { "CreatedOn" });
			DropIndex("dbo.Addresses", new[] { "ModifiedOn" });
			DropIndex("dbo.People", new[] { "ModifiedOn" });
			DropIndex("dbo.Groups", new[] { "ModifiedOn" });
			DropIndex("dbo.Groups", new[] { "Name" });
			DropIndex("dbo.GroupMembers", new[] { "ModifiedOn" });
			DropIndex("dbo.GroupMembers", new[] { "MemberId" });
			DropIndex("dbo.GroupMembers", new[] { "GroupId" });
			DropColumn("dbo.People", "SyncId");
			DropColumn("dbo.People", "AddressSyncId");
			DropColumn("dbo.Addresses", "SyncId");
			DropColumn("dbo.Addresses", "LinkedAddressSyncId");
			DropTable("dbo.SyncTombstones");
			DropTable("dbo.Groups");
			DropTable("dbo.GroupMembers");
			AddForeignKey("dbo.People", "AddressId", "dbo.Addresses", "Id", true);
		}

		public override void Up()
		{
			DropForeignKey("dbo.People", "AddressId", "dbo.Addresses");
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
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Groups", t => t.GroupId, true)
				.ForeignKey("dbo.People", t => t.MemberId, true)
				.Index(t => t.GroupId)
				.Index(t => t.MemberId)
				.Index(t => t.ModifiedOn);

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
				.PrimaryKey(t => t.Id)
				.Index(t => t.Name, unique: true)
				.Index(t => t.ModifiedOn);

			CreateTable(
				"dbo.SyncTombstones",
				c => new
				{
					Id = c.Int(false, true),
					SyncId = c.Guid(false),
					TypeName = c.String(),
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
				})
				.PrimaryKey(t => t.Id)
				.Index(t => t.CreatedOn);

			AddColumn("dbo.Addresses", "LinkedAddressSyncId", c => c.Guid());
			AddColumn("dbo.Addresses", "SyncId", c => c.Guid(false));
			AddColumn("dbo.People", "AddressSyncId", c => c.Guid(false));
			AddColumn("dbo.People", "SyncId", c => c.Guid(false));
			CreateIndex("dbo.People", "ModifiedOn");
			CreateIndex("dbo.Addresses", "ModifiedOn");
			AddForeignKey("dbo.People", "AddressId", "dbo.Addresses", "Id");
		}

		#endregion
	}
}