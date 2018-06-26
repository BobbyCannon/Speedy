#region References

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#endregion

namespace Speedy.Samples.Migrations
{
	[DbContext(typeof(ContosoDatabase))]
	internal class ContosoDatabaseModelSnapshot : ModelSnapshot
	{
		#region Methods

		protected override void BuildModel(ModelBuilder modelBuilder)
		{
#pragma warning disable 612, 618
			modelBuilder
				.HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
				.HasAnnotation("Relational:MaxIdentifierLength", 128)
				.HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

			modelBuilder.Entity("Speedy.Samples.Entities.Address", b =>
			{
				b.Property<int>("Id")
					.ValueGeneratedOnAdd()
					.HasColumnName("Id")
					.HasColumnType("int")
					.HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

				b.Property<string>("City")
					.IsRequired()
					.HasColumnName("City")
					.HasColumnType("nvarchar(256)");

				b.Property<DateTime>("CreatedOn")
					.HasColumnName("CreatedOn")
					.HasColumnType("datetime2");

				b.Property<string>("Line1")
					.IsRequired()
					.HasColumnName("Line1")
					.HasColumnType("nvarchar(256)");

				b.Property<string>("Line2")
					.IsRequired()
					.HasColumnName("Line2")
					.HasColumnType("nvarchar(256)");

				b.Property<int?>("LinkedAddressId")
					.HasColumnName("LinkedAddressId")
					.HasColumnType("int");

				b.Property<Guid?>("LinkedAddressSyncId")
					.HasColumnName("LinkedAddressSyncId")
					.HasColumnType("uniqueidentifier");

				b.Property<DateTime>("ModifiedOn")
					.HasColumnName("ModifiedOn")
					.HasColumnType("datetime2");

				b.Property<string>("Postal")
					.IsRequired()
					.HasColumnName("Postal")
					.HasColumnType("nvarchar(128)");

				b.Property<string>("State")
					.IsRequired()
					.HasColumnName("State")
					.HasColumnType("nvarchar(128)");

				b.Property<Guid>("SyncId")
					.HasColumnName("SyncId")
					.HasColumnType("uniqueidentifier");

				b.HasKey("Id");

				b.HasIndex("LinkedAddressId")
					.HasName("IX_LinkedAddressId");

				b.HasIndex("SyncId")
					.IsUnique()
					.HasName("IX_SyncId");

				b.ToTable("Addresses", "dbo");
			});

			modelBuilder.Entity("Speedy.Samples.Entities.Food", b =>
			{
				b.Property<int>("Id")
					.ValueGeneratedOnAdd()
					.HasColumnName("Id")
					.HasColumnType("int")
					.HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

				b.Property<DateTime>("CreatedOn")
					.HasColumnName("CreatedOn")
					.HasColumnType("datetime2");

				b.Property<DateTime>("ModifiedOn")
					.HasColumnName("ModifiedOn")
					.HasColumnType("datetime2");

				b.Property<string>("Name")
					.IsRequired()
					.HasColumnName("Name")
					.HasColumnType("nvarchar(256)");

				b.HasKey("Id");

				b.ToTable("Foods", "dbo");
			});

			modelBuilder.Entity("Speedy.Samples.Entities.FoodRelationship", b =>
			{
				b.Property<int>("Id")
					.ValueGeneratedOnAdd()
					.HasColumnName("Id")
					.HasColumnType("int")
					.HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

				b.Property<int>("ChildId")
					.HasColumnName("ChildId")
					.HasColumnType("int");

				b.Property<DateTime>("CreatedOn")
					.HasColumnName("CreatedOn")
					.HasColumnType("datetime2");

				b.Property<DateTime>("ModifiedOn")
					.HasColumnName("ModifiedOn")
					.HasColumnType("datetime2");

				b.Property<int>("ParentId")
					.HasColumnName("ParentId")
					.HasColumnType("int");

				b.Property<decimal>("Quantity")
					.HasColumnName("Quantity")
					.HasColumnType("decimal");

				b.HasKey("Id");

				b.HasIndex("ChildId")
					.HasName("IX_ChildId");

				b.HasIndex("ParentId")
					.HasName("IX_ParentId");

				b.ToTable("FoodRelationships", "dbo");
			});

			modelBuilder.Entity("Speedy.Samples.Entities.Group", b =>
			{
				b.Property<int>("Id")
					.ValueGeneratedOnAdd()
					.HasColumnName("Id")
					.HasColumnType("int")
					.HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

				b.Property<DateTime>("CreatedOn")
					.HasColumnName("CreatedOn")
					.HasColumnType("datetime2");

				b.Property<string>("Description")
					.IsRequired()
					.HasColumnName("Description")
					.HasColumnType("nvarchar(4000)");

				b.Property<DateTime>("ModifiedOn")
					.HasColumnName("ModifiedOn")
					.HasColumnType("datetime2");

				b.Property<string>("Name")
					.IsRequired()
					.HasColumnName("Name")
					.HasColumnType("nvarchar(256)");

				b.HasKey("Id");

				b.ToTable("Groups", "dbo");
			});

			modelBuilder.Entity("Speedy.Samples.Entities.GroupMember", b =>
			{
				b.Property<int>("Id")
					.ValueGeneratedOnAdd()
					.HasColumnName("Id")
					.HasColumnType("int")
					.HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

				b.Property<DateTime>("CreatedOn")
					.HasColumnName("CreatedOn")
					.HasColumnType("datetime2");

				b.Property<int>("GroupId")
					.HasColumnName("GroupId")
					.HasColumnType("int");

				b.Property<Guid>("GroupSyncId")
					.HasColumnName("GroupSyncId")
					.HasColumnType("uniqueidentifier");

				b.Property<int>("MemberId")
					.HasColumnName("MemberId")
					.HasColumnType("int");

				b.Property<Guid>("MemberSyncId")
					.HasColumnName("MemberSyncId")
					.HasColumnType("uniqueidentifier");

				b.Property<DateTime>("ModifiedOn")
					.HasColumnName("ModifiedOn")
					.HasColumnType("datetime2");

				b.Property<string>("Role")
					.IsRequired()
					.HasColumnName("Role")
					.HasColumnType("nvarchar(4000)");

				b.HasKey("Id");

				b.HasIndex("GroupId")
					.HasName("IX_GroupId");

				b.HasIndex("MemberId")
					.HasName("IX_MemberId");

				b.ToTable("GroupMembers", "dbo");
			});

			modelBuilder.Entity("Speedy.Samples.Entities.LogEvent", b =>
			{
				b.Property<string>("Id")
					.ValueGeneratedOnAdd()
					.HasColumnName("Id")
					.HasColumnType("nvarchar(250)");

				b.Property<DateTime>("CreatedOn")
					.HasColumnName("CreatedOn")
					.HasColumnType("datetime2");

				b.Property<string>("Message")
					.HasColumnName("Message")
					.HasColumnType("nvarchar(4000)");

				b.HasKey("Id");

				b.ToTable("LogEvents", "dbo");
			});

			modelBuilder.Entity("Speedy.Samples.Entities.Person", b =>
			{
				b.Property<int>("Id")
					.ValueGeneratedOnAdd()
					.HasColumnName("Id")
					.HasColumnType("int")
					.HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

				b.Property<int>("AddressId")
					.HasColumnName("AddressId")
					.HasColumnType("int");

				b.Property<Guid>("AddressSyncId")
					.HasColumnName("AddressSyncId")
					.HasColumnType("uniqueidentifier");

				b.Property<int?>("BillingAddressId")
					.HasColumnName("BillingAddressId")
					.HasColumnType("int");

				b.Property<Guid?>("BillingAddressSyncId")
					.HasColumnName("BillingAddressSyncId")
					.HasColumnType("uniqueidentifier");

				b.Property<DateTime>("CreatedOn")
					.HasColumnName("CreatedOn")
					.HasColumnType("datetime2");

				b.Property<DateTime>("ModifiedOn")
					.HasColumnName("ModifiedOn")
					.HasColumnType("datetime2");

				b.Property<string>("Name")
					.IsRequired()
					.HasColumnName("Name")
					.HasColumnType("nvarchar(256)");

				b.Property<Guid>("SyncId")
					.HasColumnName("SyncId")
					.HasColumnType("uniqueidentifier");

				b.HasKey("Id");

				b.HasIndex("AddressId")
					.HasName("IX_AddressId");

				b.HasIndex("BillingAddressId")
					.HasName("IX_BillingAddressId");

				b.HasIndex("Name")
					.IsUnique()
					.HasName("IX_Name");

				b.HasIndex("SyncId")
					.IsUnique()
					.HasName("IX_SyncId");

				b.ToTable("People", "dbo");
			});

			modelBuilder.Entity("Speedy.Samples.Entities.Pet", b =>
			{
				b.Property<string>("Name")
					.HasColumnName("Name")
					.HasColumnType("nvarchar(128)");

				b.Property<int>("OwnerId")
					.HasColumnName("OwnerId")
					.HasColumnType("int");

				b.Property<DateTime>("CreatedOn")
					.HasColumnName("CreatedOn")
					.HasColumnType("datetime2");

				b.Property<DateTime>("ModifiedOn")
					.HasColumnName("ModifiedOn")
					.HasColumnType("datetime2");

				b.Property<string>("TypeId")
					.IsRequired()
					.HasColumnName("TypeId")
					.HasColumnType("nvarchar(25)");

				b.HasKey("Name", "OwnerId");

				b.HasIndex("OwnerId")
					.HasName("IX_OwnerId");

				b.HasIndex("TypeId")
					.HasName("IX_TypeId");

				b.ToTable("Pets", "dbo");
			});

			modelBuilder.Entity("Speedy.Samples.Entities.PetType", b =>
			{
				b.Property<string>("Id")
					.ValueGeneratedOnAdd()
					.HasColumnName("PetTypeId")
					.HasColumnType("nvarchar(25)");

				b.Property<string>("Type")
					.HasColumnName("Type")
					.HasColumnType("nvarchar(200)");

				b.HasKey("Id");

				b.ToTable("PetType", "dbo");
			});

			modelBuilder.Entity("Speedy.Sync.SyncTombstone", b =>
			{
				b.Property<long>("Id")
					.ValueGeneratedOnAdd()
					.HasColumnName("Id")
					.HasColumnType("bigint")
					.HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

				b.Property<DateTime>("CreatedOn")
					.HasColumnName("CreatedOn")
					.HasColumnType("datetime2");

				b.Property<string>("ReferenceId")
					.IsRequired()
					.HasColumnName("ReferenceId")
					.HasColumnType("nvarchar(128)");

				b.Property<Guid>("SyncId")
					.HasColumnName("SyncId")
					.HasColumnType("uniqueidentifier");

				b.Property<string>("TypeName")
					.IsRequired()
					.HasColumnName("TypeName")
					.HasColumnType("nvarchar(768)");

				b.HasKey("Id");

				b.HasIndex("CreatedOn")
					.HasName("IX_CreatedOn");

				b.HasIndex("TypeName", "ReferenceId")
					.HasName("IX_SyncTombstones_TypeName_ReferenceId");

				b.ToTable("SyncTombstones", "dbo");
			});

			modelBuilder.Entity("Speedy.Samples.Entities.Address", b =>
			{
				b.HasOne("Speedy.Samples.Entities.Address", "LinkedAddress")
					.WithMany("LinkedAddresses")
					.HasForeignKey("LinkedAddressId")
					.OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity("Speedy.Samples.Entities.FoodRelationship", b =>
			{
				b.HasOne("Speedy.Samples.Entities.Food", "Child")
					.WithMany("ParentRelationships")
					.HasForeignKey("ChildId")
					.OnDelete(DeleteBehavior.Restrict);

				b.HasOne("Speedy.Samples.Entities.Food", "Parent")
					.WithMany("ChildRelationships")
					.HasForeignKey("ParentId")
					.OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity("Speedy.Samples.Entities.GroupMember", b =>
			{
				b.HasOne("Speedy.Samples.Entities.Group", "Group")
					.WithMany("Members")
					.HasForeignKey("GroupId")
					.OnDelete(DeleteBehavior.Cascade);

				b.HasOne("Speedy.Samples.Entities.Person", "Member")
					.WithMany("Groups")
					.HasForeignKey("MemberId")
					.OnDelete(DeleteBehavior.Cascade);
			});

			modelBuilder.Entity("Speedy.Samples.Entities.Person", b =>
			{
				b.HasOne("Speedy.Samples.Entities.Address", "Address")
					.WithMany("People")
					.HasForeignKey("AddressId")
					.OnDelete(DeleteBehavior.Restrict);

				b.HasOne("Speedy.Samples.Entities.Address", "BillingAddress")
					.WithMany("BillingPeople")
					.HasForeignKey("BillingAddressId")
					.OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity("Speedy.Samples.Entities.Pet", b =>
			{
				b.HasOne("Speedy.Samples.Entities.Person", "Owner")
					.WithMany("Owners")
					.HasForeignKey("OwnerId")
					.OnDelete(DeleteBehavior.Restrict);

				b.HasOne("Speedy.Samples.Entities.PetType", "Type")
					.WithMany("Types")
					.HasForeignKey("TypeId")
					.OnDelete(DeleteBehavior.Restrict);
			});
#pragma warning restore 612, 618
		}

		#endregion
	}
}