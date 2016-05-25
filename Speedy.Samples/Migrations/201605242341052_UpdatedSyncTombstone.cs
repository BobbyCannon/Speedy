#region References

using System.Data.Entity.Migrations;

#endregion

namespace Speedy.Samples.Migrations
{
	public partial class UpdatedSyncTombstone : DbMigration
	{
		#region Methods

		public override void Down()
		{
			DropIndex("dbo.SyncTombstones", "IX_SyncTombstones_ReferenceId_TypeName");
			AlterColumn("dbo.SyncTombstones", "TypeName", c => c.String());
			DropColumn("dbo.SyncTombstones", "ReferenceId");
		}

		public override void Up()
		{
			AddColumn("dbo.SyncTombstones", "ReferenceId", c => c.String(false, 128));
			AlterColumn("dbo.SyncTombstones", "TypeName", c => c.String(false, 768));
			CreateIndex("dbo.SyncTombstones", new[] { "ReferenceId", "TypeName" }, name: "IX_SyncTombstones_ReferenceId_TypeName");
		}

		#endregion
	}
}