#region References

using System.Data.Entity.Migrations;

#endregion

namespace Speedy.Samples.Migrations
{
	public partial class AddedOptionalBillingAddress : DbMigration
	{
		#region Methods

		public override void Down()
		{
			DropForeignKey("dbo.People", "BillingAddressId", "dbo.Addresses");
			DropIndex("dbo.People", new[] { "BillingAddressId" });
			DropColumn("dbo.People", "BillingAddressSyncId");
			DropColumn("dbo.People", "BillingAddressId");
		}

		public override void Up()
		{
			AddColumn("dbo.People", "BillingAddressId", c => c.Int());
			AddColumn("dbo.People", "BillingAddressSyncId", c => c.Guid());
			CreateIndex("dbo.People", "BillingAddressId");
			AddForeignKey("dbo.People", "BillingAddressId", "dbo.Addresses", "Id");
		}

		#endregion
	}
}