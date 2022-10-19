﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Speedy.Website.Data.Sql;

#nullable disable

namespace Speedy.Website.Data.Sql.Migrations
{
    [DbContext(typeof(ContosoSqlDatabase))]
    [Migration("20220725002532_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Speedy.Website.Data.Entities.AccountEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("AccountId");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<long>("AddressId")
                        .HasColumnType("bigint")
                        .HasColumnName("AccountAddressId");

                    b.Property<Guid>("AddressSyncId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("AccountAddressSyncId");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("AccountCreatedOn");

                    b.Property<string>("EmailAddress")
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)")
                        .HasColumnName("AccountEmailAddress");

                    b.Property<string>("ExternalId")
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)")
                        .HasColumnName("AccountExternalId");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit")
                        .HasColumnName("AccountIsDeleted");

                    b.Property<DateTime>("LastLoginDate")
                        .HasColumnType("datetime2")
                        .HasColumnName("AccountLastLoginDate");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("AccountModifiedOn");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("AccountName");

                    b.Property<string>("Nickname")
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("AccountNickname");

                    b.Property<string>("PasswordHash")
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)")
                        .HasColumnName("AccountPasswordHash");

                    b.Property<string>("Roles")
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)")
                        .HasColumnName("AccountRoles");

                    b.Property<Guid>("SyncId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("AccountSyncId");

                    b.HasKey("Id");

                    b.HasIndex("AddressId")
                        .HasDatabaseName("IX_Accounts_AddressId");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("IX_Accounts_Name");

                    b.HasIndex("Nickname")
                        .IsUnique()
                        .HasDatabaseName("IX_Accounts_Nickname")
                        .HasFilter("[AccountNickname] IS NOT NULL");

                    b.HasIndex("SyncId")
                        .IsUnique()
                        .HasDatabaseName("IX_Accounts_SyncId");

                    b.HasIndex("AddressId", "ExternalId")
                        .IsUnique()
                        .HasDatabaseName("IX_Accounts_AddressId_ExternalId")
                        .HasFilter("[AccountExternalId] IS NOT NULL");

                    b.ToTable("Accounts", "dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.AddressEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("AddressId");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<int?>("AccountId")
                        .HasColumnType("int");

                    b.Property<Guid?>("AccountSyncId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("AddressAccountSyncId");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("AddressCity");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("AddressCreatedOn");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit")
                        .HasColumnName("AddressIsDeleted");

                    b.Property<string>("Line1")
                        .IsRequired()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("AddressLineOne");

                    b.Property<string>("Line2")
                        .IsRequired()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("AddressLineTwo");

                    b.Property<long?>("LinkedAddressId")
                        .HasColumnType("bigint")
                        .HasColumnName("AddressLinkedAddressId");

                    b.Property<Guid?>("LinkedAddressSyncId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("AddressLinkedAddressSyncId");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("AddressModifiedOn");

                    b.Property<string>("Postal")
                        .IsRequired()
                        .HasMaxLength(128)
                        .IsUnicode(false)
                        .HasColumnType("varchar(128)")
                        .HasColumnName("AddressPostal");

                    b.Property<string>("State")
                        .IsRequired()
                        .HasMaxLength(128)
                        .IsUnicode(false)
                        .HasColumnType("varchar(128)")
                        .HasColumnName("AddressState");

                    b.Property<Guid>("SyncId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("AddressSyncId");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("LinkedAddressId")
                        .HasDatabaseName("IX_Address_LinkedAddressId");

                    b.HasIndex("SyncId")
                        .IsUnique()
                        .HasDatabaseName("IX_Address_SyncId");

                    b.ToTable("Addresses", "dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.FoodEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("CreatedOn");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("ModifiedOn");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("Name");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("IX_Foods_Name");

                    b.ToTable("Foods", "dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.FoodRelationshipEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("ChildId")
                        .HasColumnType("int")
                        .HasColumnName("ChildId");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("CreatedOn");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("ModifiedOn");

                    b.Property<int>("ParentId")
                        .HasColumnType("int")
                        .HasColumnName("ParentId");

                    b.Property<double>("Quantity")
                        .HasColumnType("float")
                        .HasColumnName("Quantity");

                    b.HasKey("Id");

                    b.HasIndex("ChildId")
                        .HasDatabaseName("IX_FoodRelationships_ChildId");

                    b.HasIndex("ParentId")
                        .HasDatabaseName("IX_FoodRelationships_ParentId");

                    b.ToTable("FoodRelationships", "dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.GroupEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("CreatedOn");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .IsUnicode(false)
                        .HasColumnType("varchar(4000)")
                        .HasColumnName("Description");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("ModifiedOn");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("Name");

                    b.HasKey("Id");

                    b.ToTable("Groups", "dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.GroupMemberEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("CreatedOn");

                    b.Property<int>("GroupId")
                        .HasColumnType("int")
                        .HasColumnName("GroupId");

                    b.Property<int>("MemberId")
                        .HasColumnType("int")
                        .HasColumnName("MemberId");

                    b.Property<Guid>("MemberSyncId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("MemberSyncId");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("ModifiedOn");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .IsUnicode(false)
                        .HasColumnType("varchar(4000)")
                        .HasColumnName("Role");

                    b.HasKey("Id");

                    b.HasIndex("GroupId")
                        .HasDatabaseName("IX_GroupMembers_GroupId");

                    b.HasIndex("MemberId")
                        .HasDatabaseName("IX_GroupMembers_MemberId");

                    b.HasIndex("MemberSyncId")
                        .HasDatabaseName("IX_GroupMembers_MemberSyncId");

                    b.ToTable("GroupMembers", "dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.LogEventEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(250)
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<DateTime?>("AcknowledgedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("Level")
                        .HasColumnType("int");

                    b.Property<DateTime>("LoggedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Message")
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("SyncId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("LogEvents", "dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.PetEntity", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .IsUnicode(false)
                        .HasColumnType("varchar(128)")
                        .HasColumnName("Name");

                    b.Property<int>("OwnerId")
                        .HasColumnType("int")
                        .HasColumnName("OwnerId");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("CreatedOn");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("ModifiedOn");

                    b.Property<string>("TypeId")
                        .HasMaxLength(25)
                        .IsUnicode(false)
                        .HasColumnType("varchar(25)")
                        .HasColumnName("TypeId");

                    b.HasKey("Name", "OwnerId");

                    b.HasIndex("OwnerId")
                        .HasDatabaseName("IX_Pets_OwnerId");

                    b.HasIndex("TypeId")
                        .HasDatabaseName("IX_Pets_TypeId");

                    b.HasIndex("Name", "OwnerId")
                        .IsUnique();

                    b.ToTable("Pets", "dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.PetTypeEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(25)
                        .IsUnicode(false)
                        .HasColumnType("varchar(25)")
                        .HasColumnName("PetTypeId");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("CreatedOn");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("ModifiedOn");

                    b.Property<string>("Type")
                        .HasMaxLength(200)
                        .IsUnicode(false)
                        .HasColumnType("varchar(200)")
                        .HasColumnName("Type");

                    b.HasKey("Id");

                    b.ToTable("PetType", "dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.SettingEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("CreatedOn");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit")
                        .HasColumnName("IsDeleted");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2")
                        .HasColumnName("ModifiedOn");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("Name");

                    b.Property<Guid>("SyncId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("SyncId");

                    b.Property<string>("Value")
                        .IsRequired()
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)")
                        .HasColumnName("Value");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("IX_Settings_Name");

                    b.HasIndex("SyncId")
                        .IsUnique()
                        .HasDatabaseName("IX_Settings_SyncId");

                    b.ToTable("Settings", "dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.TrackerPathConfigurationEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("CompletedOnName")
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("DataName")
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name01")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Name02")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Name03")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Name04")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Name05")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Name06")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Name07")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Name08")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Name09")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("PathName")
                        .IsRequired()
                        .HasMaxLength(896)
                        .IsUnicode(false)
                        .HasColumnType("varchar(896)");

                    b.Property<string>("PathType")
                        .IsRequired()
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)");

                    b.Property<string>("StartedOnName")
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)");

                    b.Property<Guid>("SyncId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Type01")
                        .HasColumnType("int");

                    b.Property<int>("Type02")
                        .HasColumnType("int");

                    b.Property<int>("Type03")
                        .HasColumnType("int");

                    b.Property<int>("Type04")
                        .HasColumnType("int");

                    b.Property<int>("Type05")
                        .HasColumnType("int");

                    b.Property<int>("Type06")
                        .HasColumnType("int");

                    b.Property<int>("Type07")
                        .HasColumnType("int");

                    b.Property<int>("Type08")
                        .HasColumnType("int");

                    b.Property<int>("Type09")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SyncId")
                        .IsUnique()
                        .HasDatabaseName("IX_TrackerPathConfigurations_SyncId");

                    b.ToTable("TrackerPathConfigurations", "dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.TrackerPathEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<DateTime>("CompletedOn")
                        .HasColumnType("datetime2");

                    b.Property<int>("ConfigurationId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Data")
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)");

                    b.Property<long>("ElapsedTicks")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<long?>("ParentId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("StartedOn")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("SyncId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Value01")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Value02")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Value03")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Value04")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Value05")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Value06")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Value07")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Value08")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.Property<string>("Value09")
                        .HasMaxLength(900)
                        .IsUnicode(false)
                        .HasColumnType("varchar(900)");

                    b.HasKey("Id");

                    b.HasIndex("ConfigurationId");

                    b.HasIndex("ParentId");

                    b.HasIndex("SyncId")
                        .IsUnique()
                        .HasDatabaseName("IX_TrackerPaths_SyncId");

                    b.ToTable("TrackerPaths", "dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.AccountEntity", b =>
                {
                    b.HasOne("Speedy.Website.Data.Entities.AddressEntity", "Address")
                        .WithMany("Accounts")
                        .HasForeignKey("AddressId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Address");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.AddressEntity", b =>
                {
                    b.HasOne("Speedy.Website.Data.Entities.AccountEntity", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Speedy.Website.Data.Entities.AddressEntity", "LinkedAddress")
                        .WithMany("LinkedAddresses")
                        .HasForeignKey("LinkedAddressId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Account");

                    b.Navigation("LinkedAddress");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.FoodRelationshipEntity", b =>
                {
                    b.HasOne("Speedy.Website.Data.Entities.FoodEntity", "Child")
                        .WithMany("ParentRelationships")
                        .HasForeignKey("ChildId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Speedy.Website.Data.Entities.FoodEntity", "Parent")
                        .WithMany("ChildRelationships")
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Child");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.GroupMemberEntity", b =>
                {
                    b.HasOne("Speedy.Website.Data.Entities.GroupEntity", "Group")
                        .WithMany("Members")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Speedy.Website.Data.Entities.AccountEntity", "Member")
                        .WithMany("Groups")
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Member");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.PetEntity", b =>
                {
                    b.HasOne("Speedy.Website.Data.Entities.AccountEntity", "Owner")
                        .WithMany("Pets")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Speedy.Website.Data.Entities.PetTypeEntity", "Type")
                        .WithMany("Types")
                        .HasForeignKey("TypeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Owner");

                    b.Navigation("Type");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.TrackerPathEntity", b =>
                {
                    b.HasOne("Speedy.Website.Data.Entities.TrackerPathConfigurationEntity", "Configuration")
                        .WithMany("Paths")
                        .HasForeignKey("ConfigurationId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Speedy.Website.Data.Entities.TrackerPathEntity", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Configuration");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.AccountEntity", b =>
                {
                    b.Navigation("Groups");

                    b.Navigation("Pets");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.AddressEntity", b =>
                {
                    b.Navigation("Accounts");

                    b.Navigation("LinkedAddresses");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.FoodEntity", b =>
                {
                    b.Navigation("ChildRelationships");

                    b.Navigation("ParentRelationships");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.GroupEntity", b =>
                {
                    b.Navigation("Members");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.PetTypeEntity", b =>
                {
                    b.Navigation("Types");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.TrackerPathConfigurationEntity", b =>
                {
                    b.Navigation("Paths");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.TrackerPathEntity", b =>
                {
                    b.Navigation("Children");
                });
#pragma warning restore 612, 618
        }
    }
}