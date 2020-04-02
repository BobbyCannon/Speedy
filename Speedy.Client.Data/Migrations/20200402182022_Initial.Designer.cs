﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Speedy.Client.Data;

namespace Speedy.Client.Data.Migrations
{
    [DbContext(typeof(ContosoClientDatabase))]
    [Migration("20200402182022_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3");

            modelBuilder.Entity("Speedy.Data.Client.ClientAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("AddressId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("AddressSyncId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(128)
                        .IsUnicode(false);

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastClientUpdate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .IsUnicode(false);

                    b.Property<string>("Roles")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .IsUnicode(false);

                    b.Property<Guid>("SyncId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("AddressId");

                    b.HasIndex("LastClientUpdate")
                        .HasName("IX_Accounts_LastClientUpdate");

                    b.HasIndex("SyncId")
                        .IsUnique()
                        .HasName("IX_Accounts_SyncId");

                    b.ToTable("Accounts","dbo");
                });

            modelBuilder.Entity("Speedy.Data.Client.ClientAddress", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("City")
                        .HasColumnType("TEXT")
                        .IsUnicode(false);

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastClientUpdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Line1")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(128)
                        .IsUnicode(false);

                    b.Property<string>("Line2")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(128)
                        .IsUnicode(false);

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Postal")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(25)
                        .IsUnicode(false);

                    b.Property<string>("State")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(25)
                        .IsUnicode(false);

                    b.Property<Guid>("SyncId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("LastClientUpdate")
                        .HasName("IX_Addresses_LastClientUpdate");

                    b.HasIndex("SyncId")
                        .IsUnique()
                        .HasName("IX_Addresses_SyncId");

                    b.ToTable("Addresses","dbo");
                });

            modelBuilder.Entity("Speedy.Data.Client.ClientLogEvent", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastClientUpdate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Level")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(256)
                        .IsUnicode(false);

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("SyncId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("LastClientUpdate")
                        .HasName("IX_LogEvents_LastClientUpdate");

                    b.HasIndex("SyncId")
                        .IsUnique()
                        .HasName("IX_LogEvents_SyncId");

                    b.ToTable("LogEvents","dbo");
                });

            modelBuilder.Entity("Speedy.Data.Client.ClientAccount", b =>
                {
                    b.HasOne("Speedy.Data.Client.ClientAddress", "Address")
                        .WithMany("Accounts")
                        .HasForeignKey("AddressId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
