﻿// <auto-generated />
using System;
using Bank.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bank.Service.Migrations
{
    [DbContext(typeof(BankDbContext))]
    partial class BankDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "citext");
            NpgsqlModelBuilderExtensions.UseSerialColumns(modelBuilder);

            modelBuilder.Entity("Bank.Service.DataModels.AccountInformationModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnOrder(0);

                    b.Property<string>("BankId")
                        .HasColumnType("citext");

                    b.Property<long>("CreationTimeUtcAsUnix")
                        .HasColumnType("bigint");

                    b.Property<string>("Number")
                        .HasColumnType("citext");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnOrder(1);

                    b.HasKey("Id");

                    b.ToTable("AccountInformationModel");
                });

            modelBuilder.Entity("Bank.Service.DataModels.BankModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnOrder(0);

                    b.Property<string>("BankId")
                        .HasColumnType("citext");

                    b.Property<long>("CreationTimeUtcAsUnix")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .HasColumnType("citext");

                    b.Property<string>("Region")
                        .HasColumnType("citext");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnOrder(1);

                    b.HasKey("Id");

                    b.ToTable("BankModel");
                });
#pragma warning restore 612, 618
        }
    }
}