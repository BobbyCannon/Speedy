#region References

using System.Data.Entity.Migrations;

#endregion

namespace Speedy.Samples.EntityFramework.Migrations
{
	public partial class ChangeLogEventPrimaryKey : DbMigration
	{
		#region Methods

		public override void Down()
		{
			DropPrimaryKey("dbo.LogEvents");
			AlterColumn("dbo.LogEvents", "Id", c => c.Int(false, true));
			AddPrimaryKey("dbo.LogEvents", "Id");
		}

		public override void Up()
		{
			DropPrimaryKey("dbo.LogEvents");
			AlterColumn("dbo.LogEvents", "Id", c => c.String(false, 250));
			AddPrimaryKey("dbo.LogEvents", "Id");
		}

		#endregion
	}
}