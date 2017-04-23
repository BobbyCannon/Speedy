#region References

using System.Data.Entity.Migrations;

#endregion

namespace Speedy.Samples.EntityFramework.Migrations
{
	public partial class AddedPetEntity : DbMigration
	{
		#region Methods

		public override void Down()
		{
			DropForeignKey("dbo.Pets", "OwnerId", "dbo.People");
			DropIndex("dbo.Pets", new[] { "OwnerId" });
			DropTable("dbo.Pets");
		}

		public override void Up()
		{
			CreateTable(
					"dbo.Pets",
					c => new
					{
						Name = c.String(false, 128),
						OwnerId = c.Int(false),
						ModifiedOn = c.DateTime(false, 7, storeType: "datetime2"),
						CreatedOn = c.DateTime(false, 7, storeType: "datetime2")
					})
				.PrimaryKey(t => new { t.Name, t.OwnerId })
				.ForeignKey("dbo.People", t => t.OwnerId)
				.Index(t => t.OwnerId);
		}

		#endregion
	}
}