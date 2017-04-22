#region References

using System.Data.Entity.Migrations;

#endregion

namespace Speedy.Samples.EntityFramework.Migrations
{
	public partial class DroppedSyncItems : DbMigration
	{
		#region Methods

		public override void Down()
		{
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
				.PrimaryKey(t => t.Id);

			AddColumn("dbo.Groups", "SyncId", c => c.Guid(false));
			AddColumn("dbo.GroupMembers", "SyncId", c => c.Guid(false));
			AddColumn("dbo.People", "SyncId", c => c.Guid(false));
			AddColumn("dbo.Addresses", "SyncId", c => c.Guid(false));
			CreateIndex("dbo.SyncTombstones", "CreatedOn");
			CreateIndex("dbo.SyncTombstones", new[] { "ReferenceId", "TypeName" }, name: "IX_SyncTombstones_ReferenceId_TypeName");
			CreateIndex("dbo.Addresses", "SyncId", true);
		}

		public override void Up()
		{
			DropIndex("dbo.Addresses", new[] { "SyncId" });
			DropIndex("dbo.SyncTombstones", "IX_SyncTombstones_ReferenceId_TypeName");
			DropIndex("dbo.SyncTombstones", new[] { "CreatedOn" });
			DropColumn("dbo.Addresses", "SyncId");
			DropColumn("dbo.People", "SyncId");
			DropColumn("dbo.GroupMembers", "SyncId");
			DropColumn("dbo.Groups", "SyncId");
			DropTable("dbo.SyncTombstones");
		}

		#endregion
	}
}