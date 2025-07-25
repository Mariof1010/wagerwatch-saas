﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WagerWatch.Data;

#nullable disable

namespace WagerWatch.Migrations
{
    [DbContext(typeof(WagerWatchDbContext))]
    [Migration("20250711192020_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WagerWatch.Models.Bet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("ActualPayout")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("BetType")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int>("GameId")
                        .HasColumnType("int");

                    b.Property<decimal?>("Line")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("Odds")
                        .HasColumnType("int");

                    b.Property<decimal>("PotentialPayout")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Selection")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime?>("SettledAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("Volatility")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.HasIndex("UserId");

                    b.ToTable("Bets");
                });

            modelBuilder.Entity("WagerWatch.Models.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("AwayScore")
                        .HasColumnType("int");

                    b.Property<int>("AwayTeamId")
                        .HasColumnType("int");

                    b.Property<string>("GamePeriod")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<DateTime>("GameTime")
                        .HasColumnType("datetime2");

                    b.Property<int?>("HomeScore")
                        .HasColumnType("int");

                    b.Property<int>("HomeTeamId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<string>("Sport")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasDefaultValue("Scheduled");

                    b.HasKey("Id");

                    b.HasIndex("AwayTeamId");

                    b.HasIndex("HomeTeamId");

                    b.ToTable("Games");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            AwayTeamId = 2,
                            GameTime = new DateTime(2025, 7, 11, 20, 0, 0, 0, DateTimeKind.Utc),
                            HomeTeamId = 1,
                            Sport = "NFL",
                            Status = "Scheduled"
                        },
                        new
                        {
                            Id = 2,
                            AwayTeamId = 4,
                            GameTime = new DateTime(2025, 7, 12, 17, 0, 0, 0, DateTimeKind.Utc),
                            HomeTeamId = 3,
                            Sport = "NFL",
                            Status = "Scheduled"
                        },
                        new
                        {
                            Id = 3,
                            AwayTeamId = 6,
                            GameTime = new DateTime(2025, 7, 11, 23, 0, 0, 0, DateTimeKind.Utc),
                            HomeTeamId = 5,
                            Sport = "NBA",
                            Status = "Scheduled"
                        },
                        new
                        {
                            Id = 4,
                            AwayTeamId = 8,
                            GameTime = new DateTime(2025, 7, 13, 19, 0, 0, 0, DateTimeKind.Utc),
                            HomeTeamId = 7,
                            Sport = "NBA",
                            Status = "Scheduled"
                        });
                });

            modelBuilder.Entity("WagerWatch.Models.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Abbreviation")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("LogoUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Sport")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Name", "Sport")
                        .IsUnique();

                    b.ToTable("Teams");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Abbreviation = "KC",
                            City = "Kansas City",
                            IsActive = true,
                            Name = "Kansas City Chiefs",
                            Sport = "NFL"
                        },
                        new
                        {
                            Id = 2,
                            Abbreviation = "BUF",
                            City = "Buffalo",
                            IsActive = true,
                            Name = "Buffalo Bills",
                            Sport = "NFL"
                        },
                        new
                        {
                            Id = 3,
                            Abbreviation = "DAL",
                            City = "Dallas",
                            IsActive = true,
                            Name = "Dallas Cowboys",
                            Sport = "NFL"
                        },
                        new
                        {
                            Id = 4,
                            Abbreviation = "GB",
                            City = "Green Bay",
                            IsActive = true,
                            Name = "Green Bay Packers",
                            Sport = "NFL"
                        },
                        new
                        {
                            Id = 5,
                            Abbreviation = "LAL",
                            City = "Los Angeles",
                            IsActive = true,
                            Name = "Los Angeles Lakers",
                            Sport = "NBA"
                        },
                        new
                        {
                            Id = 6,
                            Abbreviation = "BOS",
                            City = "Boston",
                            IsActive = true,
                            Name = "Boston Celtics",
                            Sport = "NBA"
                        },
                        new
                        {
                            Id = 7,
                            Abbreviation = "GSW",
                            City = "San Francisco",
                            IsActive = true,
                            Name = "Golden State Warriors",
                            Sport = "NBA"
                        },
                        new
                        {
                            Id = 8,
                            Abbreviation = "MIA",
                            City = "Miami",
                            IsActive = true,
                            Name = "Miami Heat",
                            Sport = "NBA"
                        });
                });

            modelBuilder.Entity("WagerWatch.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsPremium")
                        .HasColumnType("bit");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TimeZone")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasDefaultValue("America/New_York");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CreatedAt = new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                            Email = "test@wagerwatch.com",
                            IsActive = true,
                            IsPremium = true,
                            PasswordHash = "hashedpassword123",
                            TimeZone = "America/New_York",
                            Username = "testuser"
                        });
                });

            modelBuilder.Entity("WagerWatch.Models.Bet", b =>
                {
                    b.HasOne("WagerWatch.Models.Game", "Game")
                        .WithMany("Bets")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("WagerWatch.Models.User", "User")
                        .WithMany("Bets")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");

                    b.Navigation("User");
                });

            modelBuilder.Entity("WagerWatch.Models.Game", b =>
                {
                    b.HasOne("WagerWatch.Models.Team", "AwayTeam")
                        .WithMany()
                        .HasForeignKey("AwayTeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("WagerWatch.Models.Team", "HomeTeam")
                        .WithMany()
                        .HasForeignKey("HomeTeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("AwayTeam");

                    b.Navigation("HomeTeam");
                });

            modelBuilder.Entity("WagerWatch.Models.Game", b =>
                {
                    b.Navigation("Bets");
                });

            modelBuilder.Entity("WagerWatch.Models.User", b =>
                {
                    b.Navigation("Bets");
                });
#pragma warning restore 612, 618
        }
    }
}
