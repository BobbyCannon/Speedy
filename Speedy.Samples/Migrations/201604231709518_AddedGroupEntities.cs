#region References

using System.Data.Entity.Migrations;

#endregion

namespace Speedy.Samples.Migrations
{
	public partial class AddedGroupEntities : DbMigration
	{
		#region Methods

		public override void Down()
		{
			DropForeignKey("dbo.People", "AddressId", "dbo.Addresses");
			DropForeignKey("dbo.GroupMembers", "MemberId", "dbo.People");
			DropForeignKey("dbo.GroupMembers", "GroupId", "dbo.Groups");
			DropIndex("dbo.Groups", new[] { "ModifiedOn" });
			DropIndex("dbo.Groups", new[] { "Name" });
			DropIndex("dbo.GroupMembers", new[] { "ModifiedOn" });
			DropIndex("dbo.GroupMembers", new[] { "MemberId" });
			DropIndex("dbo.GroupMembers", new[] { "GroupId" });
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

			AddForeignKey("dbo.People", "AddressId", "dbo.Addresses", "Id");
		}

		#endregion
	}
}