﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SharkSync.PostgreSQL;

namespace SharkSync.PostgreSQL.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20180709194457_AddedMoreSeedData")]
    partial class AddedMoreSeedData
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("SharkSync.PostgreSQL.Entities.Account", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("AvatarUrl")
                        .HasColumnName("avatarurl");

                    b.Property<int>("Balance")
                        .HasColumnName("balance");

                    b.Property<string>("EmailAddress")
                        .HasColumnName("emailaddress");

                    b.Property<string>("GitHubId")
                        .HasColumnName("githubid");

                    b.Property<string>("GoogleId")
                        .HasColumnName("googleid");

                    b.Property<string>("MicrosoftId")
                        .HasColumnName("microsoftid");

                    b.Property<string>("Name")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("account");

                    b.HasData(
                        new { Id = new Guid("250c6f28-4611-4c28-902c-8464fabc510b"), Balance = 0, Name = "Integration Tests Account" }
                    );
                });

            modelBuilder.Entity("SharkSync.PostgreSQL.Entities.Application", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<Guid>("AccessKey")
                        .HasColumnName("accesskey");

                    b.Property<Guid>("AccountId")
                        .HasColumnName("accountid");

                    b.Property<string>("Name")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("application");

                    b.HasData(
                        new { Id = new Guid("afd8db1e-73b8-4d5f-9cb1-6b49d205555a"), AccessKey = new Guid("3d65a27c-9d1d-48a3-a888-89cc0f7851d0"), AccountId = new Guid("250c6f28-4611-4c28-902c-8464fabc510b"), Name = "Integration Test App" },
                        new { Id = new Guid("59eadf1b-c4bf-4ded-8a2b-b80305b960fe"), AccessKey = new Guid("e7b40cf0-2781-4dc7-9545-91fd812fc506"), AccountId = new Guid("250c6f28-4611-4c28-902c-8464fabc510b"), Name = "Integration Test App 2" },
                        new { Id = new Guid("b858ceb1-00d0-4427-b45d-e9890b77da36"), AccessKey = new Guid("03172495-6158-44ae-b5b4-6ea5163f02d8"), AccountId = new Guid("250c6f28-4611-4c28-902c-8464fabc510b"), Name = "Integration Test App 3" },
                        new { Id = new Guid("19d8856c-a439-46ae-9932-c81fd0fe5556"), AccessKey = new Guid("0f458ce8-1a0e-450c-a2c4-2b50b3c4f41d"), AccountId = new Guid("250c6f28-4611-4c28-902c-8464fabc510b"), Name = "Integration Test App 4" }
                    );
                });

            modelBuilder.Entity("SharkSync.PostgreSQL.Entities.Change", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<Guid>("AccountId")
                        .HasColumnName("accountid");

                    b.Property<Guid>("ApplicationId")
                        .HasColumnName("applicationid");

                    b.Property<long>("ClientModified")
                        .HasColumnName("clientmodified");

                    b.Property<string>("Entity")
                        .HasColumnName("entity");

                    b.Property<string>("GroupId")
                        .HasColumnName("groupid");

                    b.Property<string>("Property")
                        .HasColumnName("property");

                    b.Property<Guid>("RecordId")
                        .HasColumnName("recordid");

                    b.Property<string>("RecordValue")
                        .HasColumnName("recordvalue");

                    b.HasKey("Id");

                    b.ToTable("change");
                });
#pragma warning restore 612, 618
        }
    }
}
