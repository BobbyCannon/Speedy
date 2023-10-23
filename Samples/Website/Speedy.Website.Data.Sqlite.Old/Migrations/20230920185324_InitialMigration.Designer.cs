﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Speedy.Website.Data.Sqlite;

namespace Speedy.Website.Data.Sqlite.Old.Migrations
{
    [DbContext(typeof(ContosoSqliteDatabase))]
    [Migration("20230920185324_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.32");

            modelBuilder.Entity("Speedy.Website.Data.Entities.AccountEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("AccountId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("AddressId")
                        .HasColumnName("AccountAddressId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("AddressSyncId")
                        .HasColumnName("AccountAddressSyncId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnName("AccountCreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("EmailAddress")
                        .HasColumnName("AccountEmailAddress")
                        .HasColumnType("TEXT");

                    b.Property<string>("ExternalId")
                        .HasColumnName("AccountExternalId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnName("AccountIsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastLoginDate")
                        .HasColumnName("AccountLastLoginDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnName("AccountModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("AccountName")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<string>("Nickname")
                        .HasColumnName("AccountNickname")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash")
                        .HasColumnName("AccountPasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("Roles")
                        .HasColumnName("AccountRoles")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SyncId")
                        .HasColumnName("AccountSyncId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("AddressId")
                        .HasName("IX_Accounts_AddressId");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasName("IX_Accounts_Name");

                    b.HasIndex("Nickname")
                        .IsUnique()
                        .HasName("IX_Accounts_Nickname");

                    b.HasIndex("SyncId")
                        .IsUnique()
                        .HasName("IX_Accounts_SyncId");

                    b.HasIndex("AddressId", "ExternalId")
                        .IsUnique()
                        .HasName("IX_Accounts_AddressId_ExternalId");

                    b.ToTable("Accounts","dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.AddressEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("AddressId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AccountId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("AccountSyncId")
                        .HasColumnName("AddressAccountSyncId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnName("AddressCity")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnName("AddressCreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnName("AddressIsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Line1")
                        .IsRequired()
                        .HasColumnName("AddressLineOne")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<string>("Line2")
                        .IsRequired()
                        .HasColumnName("AddressLineTwo")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<long?>("LinkedAddressId")
                        .HasColumnName("AddressLinkedAddressId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("LinkedAddressSyncId")
                        .HasColumnName("AddressLinkedAddressSyncId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnName("AddressModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Postal")
                        .IsRequired()
                        .HasColumnName("AddressPostal")
                        .HasColumnType("TEXT")
                        .HasMaxLength(128);

                    b.Property<string>("State")
                        .IsRequired()
                        .HasColumnName("AddressState")
                        .HasColumnType("TEXT")
                        .HasMaxLength(128);

                    b.Property<Guid>("SyncId")
                        .HasColumnName("AddressSyncId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("LinkedAddressId")
                        .HasName("IX_Address_LinkedAddressId");

                    b.HasIndex("SyncId")
                        .IsUnique()
                        .HasName("IX_Address_SyncId");

                    b.ToTable("Addresses","dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.FoodEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Id")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnName("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnName("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasName("IX_Foods_Name");

                    b.ToTable("Foods","dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.FoodRelationshipEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Id")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChildId")
                        .HasColumnName("ChildId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnName("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnName("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<int>("ParentId")
                        .HasColumnName("ParentId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Quantity")
                        .HasColumnName("Quantity")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.HasIndex("ChildId")
                        .HasName("IX_FoodRelationships_ChildId");

                    b.HasIndex("ParentId")
                        .HasName("IX_FoodRelationships_ParentId");

                    b.ToTable("FoodRelationships","dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.GroupEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Id")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnName("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnName("Description")
                        .HasColumnType("TEXT")
                        .HasMaxLength(4000);

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnName("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.ToTable("Groups","dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.GroupMemberEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Id")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnName("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<int>("GroupId")
                        .HasColumnName("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MemberId")
                        .HasColumnName("MemberId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("MemberSyncId")
                        .HasColumnName("MemberSyncId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnName("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnName("Role")
                        .HasColumnType("TEXT")
                        .HasMaxLength(4000);

                    b.HasKey("Id");

                    b.HasIndex("GroupId")
                        .HasName("IX_GroupMembers_GroupId");

                    b.HasIndex("MemberId")
                        .HasName("IX_GroupMembers_MemberId");

                    b.HasIndex("MemberSyncId")
                        .HasName("IX_GroupMembers_MemberSyncId");

                    b.ToTable("GroupMembers","dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.LogEventEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasMaxLength(250);

                    b.Property<DateTime?>("AcknowledgedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Level")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LoggedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Message")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("SyncId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("LogEvents","dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.PetEntity", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnName("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(128);

                    b.Property<int>("OwnerId")
                        .HasColumnName("OwnerId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnName("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnName("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("TypeId")
                        .HasColumnName("TypeId")
                        .HasColumnType("TEXT")
                        .HasMaxLength(25);

                    b.HasKey("Name", "OwnerId");

                    b.HasIndex("OwnerId")
                        .HasName("IX_Pets_OwnerId");

                    b.HasIndex("TypeId")
                        .HasName("IX_Pets_TypeId");

                    b.HasIndex("Name", "OwnerId")
                        .IsUnique();

                    b.ToTable("Pets","dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.PetTypeEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnName("PetTypeId")
                        .HasColumnType("TEXT")
                        .HasMaxLength(25);

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnName("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnName("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Type")
                        .HasColumnName("Type")
                        .HasColumnType("TEXT")
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.ToTable("PetType","dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.SettingEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Id")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnName("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnName("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnName("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<Guid>("SyncId")
                        .HasColumnName("SyncId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnName("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasName("IX_Settings_Name");

                    b.HasIndex("SyncId")
                        .IsUnique()
                        .HasName("IX_Settings_SyncId");

                    b.ToTable("Settings","dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.TrackerPathConfigurationEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CompletedOnName")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("DataName")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name01")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Name02")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Name03")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Name04")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Name05")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Name06")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Name07")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Name08")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Name09")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("PathName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(896);

                    b.Property<string>("PathType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("StartedOnName")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SyncId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Type01")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type02")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type03")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type04")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type05")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type06")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type07")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type08")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type09")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SyncId")
                        .IsUnique()
                        .HasName("IX_TrackerPathConfigurations_SyncId");

                    b.ToTable("TrackerPathConfigurations","dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.TrackerPathEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CompletedOn")
                        .HasColumnType("datetime2");

                    b.Property<int>("ConfigurationId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Data")
                        .HasColumnType("TEXT");

                    b.Property<long>("ElapsedTicks")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<long?>("ParentId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("StartedOn")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("SyncId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Value01")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Value02")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Value03")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Value04")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Value05")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Value06")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Value07")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Value08")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.Property<string>("Value09")
                        .HasColumnType("TEXT")
                        .HasMaxLength(900);

                    b.HasKey("Id");

                    b.HasIndex("ConfigurationId");

                    b.HasIndex("ParentId");

                    b.HasIndex("SyncId")
                        .IsUnique()
                        .HasName("IX_TrackerPaths_SyncId");

                    b.ToTable("TrackerPaths","dbo");
                });

            modelBuilder.Entity("Speedy.Website.Data.Entities.AccountEntity", b =>
                {
                    b.HasOne("Speedy.Website.Data.Entities.AddressEntity", "Address")
                        .WithMany("Accounts")
                        .HasForeignKey("AddressId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
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
                });
#pragma warning restore 612, 618
        }
    }
}