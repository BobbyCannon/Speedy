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
			DropColumn("dbo.People", "SyncStatus");
			DropColumn("dbo.People", "SyncId");
			DropColumn("dbo.Addresses", "SyncStatus");
			DropColumn("dbo.Addresses", "SyncId");
		}

		public override void Up()
		{
			AddColumn("dbo.Addresses", "SyncId", c => c.Guid(nullable: false));
			AddColumn("dbo.Addresses", "SyncStatus", c => c.Int(nullable: false));
			AddColumn("dbo.People", "SyncId", c => c.Guid(nullable: false));
			AddColumn("dbo.People", "SyncStatus", c => c.Int(nullable: false));
		}

		#endregion
	}
}