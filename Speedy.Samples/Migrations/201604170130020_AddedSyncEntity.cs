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
			DropColumn("dbo.People", "SyncStatus");
			DropColumn("dbo.People", "SyncId");
			DropColumn("dbo.Addresses", "SyncStatus");
			DropColumn("dbo.Addresses", "SyncId");
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
					TypeFullName = c.String(false),
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
				})
				.PrimaryKey(t => t.Id)
				.Index(t => t.CreatedOn);

			AddColumn("dbo.Addresses", "SyncId", c => c.Guid(false));
			AddColumn("dbo.Addresses", "SyncStatus", c => c.Int(false));
			AddColumn("dbo.People", "SyncId", c => c.Guid(false));
			AddColumn("dbo.People", "SyncStatus", c => c.Int(false));
			CreateIndex("dbo.Addresses", "ModifiedOn");
			CreateIndex("dbo.People", "ModifiedOn");
		}

		#endregion
	}
}