#region References

using System.Data.Entity.Migrations;

#endregion

namespace Speedy.Samples.Migrations
{
	public partial class AddedSyncEntity : DbMigration
	{
		#region Methods

		public override void Down()
		{
			DropIndex("dbo.SyncTombstones", new[] { "CreatedOn" });
			DropIndex("dbo.People", new[] { "ModifiedOn" });
			DropIndex("dbo.Addresses", new[] { "ModifiedOn" });
			DropColumn("dbo.People", "SyncId");
			DropColumn("dbo.People", "AddressSyncId");
			DropColumn("dbo.Addresses", "SyncId");
			DropColumn("dbo.Addresses", "LinkedAddressSyncId");
			DropTable("dbo.SyncTombstones");
		}

		public override void Up()
		{
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
			CreateIndex("dbo.Addresses", "ModifiedOn");
			CreateIndex("dbo.People", "ModifiedOn");
		}

		#endregion
	}
}