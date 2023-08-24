﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Nerve.Service.Domain;

#nullable disable

namespace Nerve.Service.Migrations
{
    [DbContext(typeof(NerveDbContext))]
    partial class NerveDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.10");

            modelBuilder.Entity("Nerve.Service.Domain.Counters.Counter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("Value")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Counters");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Value = 0L
                        },
                        new
                        {
                            Id = 2,
                            Value = 0L
                        },
                        new
                        {
                            Id = 3,
                            Value = 0L
                        });
                });

            modelBuilder.Entity("Nerve.Service.Domain.Queries.Query", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Client")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Domain")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<float>("Duration")
                        .HasColumnType("REAL");

                    b.Property<byte>("ResponseCode")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Timestamp")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Domain");

                    b.ToTable("Queries");
                });
#pragma warning restore 612, 618
        }
    }
}