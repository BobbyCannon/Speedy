using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Speedy.Samples.EntityFrameworkCore;

namespace Speedy.Samples.EntityFrameworkCore.Migrations
{
    [DbContext(typeof(EntityFrameworkCoreContosoDatabase))]
    [Migration("20160613125940_InitialDatabase")]
    partial class InitialDatabase
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rc2-20896")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Speedy.Samples.Entities.Address", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("City")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Line1")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("Line2")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<int?>("LinkedAddressId");

                    b.Property<Guid?>("LinkedAddressSyncId");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Postal")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 128);

                    b.Property<string>("State")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 128);

                    b.Property<Guid>("SyncId");

                    b.HasKey("Id");

                    b.HasIndex("LinkedAddressId");

                    b.HasIndex("SyncId")
                        .IsUnique();

                    b.ToTable("Addresses");
                });

            modelBuilder.Entity("Speedy.Samples.Entities.Food", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.ToTable("Foods");
                });

            modelBuilder.Entity("Speedy.Samples.Entities.FoodRelationship", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ChildId");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<int>("ParentId");

                    b.Property<decimal>("Quantity");

                    b.HasKey("Id");

                    b.HasIndex("ChildId");

                    b.HasIndex("ParentId");

                    b.ToTable("FoodRelationships");
                });

            modelBuilder.Entity("Speedy.Samples.Entities.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired();

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<Guid>("SyncId");

                    b.HasKey("Id");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("Speedy.Samples.Entities.GroupMember", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<int>("GroupId");

                    b.Property<Guid>("GroupSyncId");

                    b.Property<int>("MemberId");

                    b.Property<Guid>("MemberSyncId");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Role")
                        .IsRequired();

                    b.Property<Guid>("SyncId");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("MemberId");

                    b.ToTable("GroupMembers");
                });

            modelBuilder.Entity("Speedy.Samples.Entities.LogEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Message");

                    b.HasKey("Id");

                    b.ToTable("LogEvents");
                });

            modelBuilder.Entity("Speedy.Samples.Entities.Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AddressId");

                    b.Property<Guid>("AddressSyncId");

                    b.Property<int?>("BillingAddressId");

                    b.Property<Guid?>("BillingAddressSyncId");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<Guid>("SyncId");

                    b.HasKey("Id");

                    b.HasIndex("AddressId");

                    b.HasIndex("BillingAddressId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("People");
                });

            modelBuilder.Entity("Speedy.Sync.SyncTombstone", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("ReferenceId")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 128);

                    b.Property<Guid>("SyncId");

                    b.Property<string>("TypeName")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 768);

                    b.HasKey("Id");

                    b.ToTable("SyncTombstones");
                });

            modelBuilder.Entity("Speedy.Samples.Entities.Address", b =>
                {
                    b.HasOne("Speedy.Samples.Entities.Address")
                        .WithMany()
                        .HasForeignKey("LinkedAddressId");
                });

            modelBuilder.Entity("Speedy.Samples.Entities.FoodRelationship", b =>
                {
                    b.HasOne("Speedy.Samples.Entities.Food")
                        .WithMany()
                        .HasForeignKey("ChildId");

                    b.HasOne("Speedy.Samples.Entities.Food")
                        .WithMany()
                        .HasForeignKey("ParentId");
                });

            modelBuilder.Entity("Speedy.Samples.Entities.GroupMember", b =>
                {
                    b.HasOne("Speedy.Samples.Entities.Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Speedy.Samples.Entities.Person")
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Speedy.Samples.Entities.Person", b =>
                {
                    b.HasOne("Speedy.Samples.Entities.Address")
                        .WithMany()
                        .HasForeignKey("AddressId");

                    b.HasOne("Speedy.Samples.Entities.Address")
                        .WithMany()
                        .HasForeignKey("BillingAddressId");
                });
        }
    }
}
