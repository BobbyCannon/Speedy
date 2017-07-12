namespace Speedy.Samples.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Addresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        City = c.String(nullable: false, maxLength: 256),
                        Line1 = c.String(nullable: false, maxLength: 256),
                        Line2 = c.String(nullable: false, maxLength: 256),
                        LinkedAddressId = c.Int(),
                        LinkedAddressSyncId = c.Guid(),
                        Postal = c.String(nullable: false, maxLength: 128),
                        State = c.String(nullable: false, maxLength: 128),
                        ModifiedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Addresses", t => t.LinkedAddressId)
                .Index(t => t.LinkedAddressId);
            
            CreateTable(
                "dbo.People",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AddressId = c.Int(nullable: false),
                        AddressSyncId = c.Guid(nullable: false),
                        BillingAddressId = c.Int(),
                        BillingAddressSyncId = c.Guid(),
                        Name = c.String(nullable: false, maxLength: 256),
                        ModifiedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Addresses", t => t.AddressId)
                .ForeignKey("dbo.Addresses", t => t.BillingAddressId)
                .Index(t => t.AddressId)
                .Index(t => t.BillingAddressId);
            
            CreateTable(
                "dbo.GroupMembers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        GroupSyncId = c.Guid(nullable: false),
                        MemberId = c.Int(nullable: false),
                        MemberSyncId = c.Guid(nullable: false),
                        Role = c.String(nullable: false, maxLength: 4000),
                        ModifiedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Groups", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.People", t => t.MemberId, cascadeDelete: true)
                .Index(t => t.GroupId)
                .Index(t => t.MemberId);
            
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(nullable: false, maxLength: 4000),
                        Name = c.String(nullable: false, maxLength: 256),
                        ModifiedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Pets",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                        OwnerId = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        ModifiedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        TypeId = c.String(nullable: false, maxLength: 25),
                    })
                .PrimaryKey(t => new { t.Name, t.OwnerId })
                .ForeignKey("dbo.People", t => t.OwnerId)
                .ForeignKey("dbo.PetType", t => t.TypeId)
                .Index(t => t.OwnerId)
                .Index(t => t.TypeId);
            
            CreateTable(
                "dbo.PetType",
                c => new
                    {
                        PetTypeId = c.String(nullable: false, maxLength: 25),
                        Type = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.PetTypeId);
            
            CreateTable(
                "dbo.Foods",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 256),
                        ModifiedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.FoodRelationships",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ChildId = c.Int(nullable: false),
                        ParentId = c.Int(nullable: false),
                        Quantity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ModifiedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Foods", t => t.ChildId)
                .ForeignKey("dbo.Foods", t => t.ParentId)
                .Index(t => t.ChildId)
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.LogEvents",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 250),
                        Message = c.String(maxLength: 4000),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FoodRelationships", "ParentId", "dbo.Foods");
            DropForeignKey("dbo.FoodRelationships", "ChildId", "dbo.Foods");
            DropForeignKey("dbo.Addresses", "LinkedAddressId", "dbo.Addresses");
            DropForeignKey("dbo.Pets", "TypeId", "dbo.PetType");
            DropForeignKey("dbo.Pets", "OwnerId", "dbo.People");
            DropForeignKey("dbo.GroupMembers", "MemberId", "dbo.People");
            DropForeignKey("dbo.GroupMembers", "GroupId", "dbo.Groups");
            DropForeignKey("dbo.People", "BillingAddressId", "dbo.Addresses");
            DropForeignKey("dbo.People", "AddressId", "dbo.Addresses");
            DropIndex("dbo.FoodRelationships", new[] { "ParentId" });
            DropIndex("dbo.FoodRelationships", new[] { "ChildId" });
            DropIndex("dbo.Pets", new[] { "TypeId" });
            DropIndex("dbo.Pets", new[] { "OwnerId" });
            DropIndex("dbo.GroupMembers", new[] { "MemberId" });
            DropIndex("dbo.GroupMembers", new[] { "GroupId" });
            DropIndex("dbo.People", new[] { "BillingAddressId" });
            DropIndex("dbo.People", new[] { "AddressId" });
            DropIndex("dbo.Addresses", new[] { "LinkedAddressId" });
            DropTable("dbo.LogEvents");
            DropTable("dbo.FoodRelationships");
            DropTable("dbo.Foods");
            DropTable("dbo.PetType");
            DropTable("dbo.Pets");
            DropTable("dbo.Groups");
            DropTable("dbo.GroupMembers");
            DropTable("dbo.People");
            DropTable("dbo.Addresses");
        }
    }
}
