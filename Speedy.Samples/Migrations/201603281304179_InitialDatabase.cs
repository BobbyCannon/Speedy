#region References

using System.Data.Entity.Migrations;

#endregion

namespace Speedy.Samples.Migrations
{
	public partial class InitialDatabase : DbMigration
	{
		#region Methods

		public override void Down()
		{
			DropForeignKey("dbo.FoodRelationships", "ParentId", "dbo.Foods");
			DropForeignKey("dbo.FoodRelationships", "ChildId", "dbo.Foods");
			DropForeignKey("dbo.People", "AddressId", "dbo.Addresses");
			DropForeignKey("dbo.Addresses", "LinkedAddressId", "dbo.Addresses");
			DropIndex("dbo.FoodRelationships", new[] { "ParentId" });
			DropIndex("dbo.FoodRelationships", new[] { "ChildId" });
			DropIndex("dbo.People", new[] { "Name" });
			DropIndex("dbo.People", new[] { "AddressId" });
			DropIndex("dbo.Addresses", new[] { "LinkedAddressId" });
			DropIndex("dbo.Addresses", new[] { "Line1" });
			DropTable("dbo.FoodRelationships");
			DropTable("dbo.Foods");
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
					Postal = c.String(false, 128),
					State = c.String(false, 128),
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2"),
					ModifiedOn = c.DateTime(false, 7, storeType: "datetime2")
				})
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Addresses", t => t.LinkedAddressId)
				.Index(t => t.Line1, unique: true)
				.Index(t => t.LinkedAddressId);

			CreateTable(
				"dbo.People",
				c => new
				{
					Id = c.Int(false, true),
					AddressId = c.Int(false),
					Name = c.String(false, 256),
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2"),
					ModifiedOn = c.DateTime(false, 7, storeType: "datetime2")
				})
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Addresses", t => t.AddressId, true)
				.Index(t => t.AddressId)
				.Index(t => t.Name, unique: true);

			CreateTable(
				"dbo.Foods",
				c => new
				{
					Id = c.Int(false, true),
					Name = c.String(false, 256),
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2"),
					ModifiedOn = c.DateTime(false, 7, storeType: "datetime2")
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
					CreatedOn = c.DateTime(false, 7, storeType: "datetime2"),
					ModifiedOn = c.DateTime(false, 7, storeType: "datetime2")
				})
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.Foods", t => t.ChildId)
				.ForeignKey("dbo.Foods", t => t.ParentId)
				.Index(t => t.ChildId)
				.Index(t => t.ParentId);
		}

		#endregion
	}
}