﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OrdersOrchestrator.Database;

#nullable disable

namespace OrdersOrchestrator.Migrations
{
    [DbContext(typeof(StateMachineDbContext))]
    [Migration("20231026180356_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("OrdersOrchestrator.StateMachines.OrderRequestSagaInstance", b =>
                {
                    b.Property<Guid>("CorrelationId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp");

                    b.Property<string>("CurrentState")
                        .HasColumnType("varchar");

                    b.Property<string>("CustomerId")
                        .HasColumnType("varchar");

                    b.Property<string>("CustomerType")
                        .HasColumnType("varchar");

                    b.Property<string>("ItemId")
                        .HasColumnType("varchar");

                    b.Property<string>("Reason")
                        .HasColumnType("varchar");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp");

                    b.Property<int>("Version")
                        .HasColumnType("int");

                    b.HasKey("CorrelationId");

                    b.ToTable("OrderRequestSagaInstance");
                });
#pragma warning restore 612, 618
        }
    }
}
