﻿// <auto-generated />
using System;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("Domain.Aggregates.AccountingPeriods.AccountBalanceCheckpoint", b =>
                {
                    b.Property<long>("_internalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("InternalId");

                    b.Property<long>("AccountId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("AccountingPeriodId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("_externalId")
                        .HasColumnType("TEXT")
                        .HasColumnName("ExternalId");

                    b.HasKey("_internalId");

                    b.HasIndex("AccountId")
                        .IsUnique();

                    b.HasIndex("AccountingPeriodId");

                    b.HasIndex("_externalId")
                        .IsUnique();

                    b.ToTable("AccountBalanceCheckpoint");
                });

            modelBuilder.Entity("Domain.Aggregates.AccountingPeriods.AccountingPeriod", b =>
                {
                    b.Property<long>("_internalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("InternalId");

                    b.Property<bool>("IsOpen")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Month")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Year")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("_externalId")
                        .HasColumnType("TEXT")
                        .HasColumnName("ExternalId");

                    b.HasKey("_internalId");

                    b.HasIndex("_externalId")
                        .IsUnique();

                    b.HasIndex("Year", "Month")
                        .IsUnique();

                    b.ToTable("AccountingPeriods");
                });

            modelBuilder.Entity("Domain.Aggregates.AccountingPeriods.Transaction", b =>
                {
                    b.Property<long>("_internalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("InternalId");

                    b.Property<long>("AccountingPeriodId")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("TransactionDate")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("_externalId")
                        .HasColumnType("TEXT")
                        .HasColumnName("ExternalId");

                    b.HasKey("_internalId");

                    b.HasIndex("AccountingPeriodId");

                    b.HasIndex("_externalId")
                        .IsUnique();

                    b.ToTable("Transaction");
                });

            modelBuilder.Entity("Domain.Aggregates.AccountingPeriods.TransactionBalanceEvent", b =>
                {
                    b.Property<long>("_internalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("InternalId");

                    b.Property<long>("AccountId")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("EventDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("EventSequence")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TransactionAccountType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TransactionEventType")
                        .HasColumnType("INTEGER");

                    b.Property<long>("TransactionId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("_externalId")
                        .HasColumnType("TEXT")
                        .HasColumnName("ExternalId");

                    b.HasKey("_internalId");

                    b.HasIndex("AccountId")
                        .IsUnique();

                    b.HasIndex("TransactionId");

                    b.HasIndex("_externalId")
                        .IsUnique();

                    b.HasIndex("EventDate", "EventSequence")
                        .IsUnique();

                    b.ToTable("TransactionBalanceEvent");
                });

            modelBuilder.Entity("Domain.Aggregates.Accounts.Account", b =>
                {
                    b.Property<long>("_internalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("InternalId");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("_externalId")
                        .HasColumnType("TEXT")
                        .HasColumnName("ExternalId");

                    b.HasKey("_internalId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("_externalId")
                        .IsUnique();

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("Domain.Aggregates.Funds.Fund", b =>
                {
                    b.Property<long>("_internalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("InternalId");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("_externalId")
                        .HasColumnType("TEXT")
                        .HasColumnName("ExternalId");

                    b.HasKey("_internalId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("_externalId")
                        .IsUnique();

                    b.ToTable("Funds");
                });

            modelBuilder.Entity("Domain.ValueObjects.FundAmount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long?>("AccountBalanceCheckpointId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Amount")
                        .HasColumnType("TEXT");

                    b.Property<long>("FundId")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("TransactionId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AccountBalanceCheckpointId");

                    b.HasIndex("FundId")
                        .IsUnique();

                    b.HasIndex("TransactionId");

                    b.ToTable("FundAmount");
                });

            modelBuilder.Entity("Domain.Aggregates.AccountingPeriods.AccountBalanceCheckpoint", b =>
                {
                    b.HasOne("Domain.Aggregates.Accounts.Account", "Account")
                        .WithOne()
                        .HasForeignKey("Domain.Aggregates.AccountingPeriods.AccountBalanceCheckpoint", "AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Aggregates.AccountingPeriods.AccountingPeriod", "AccountingPeriod")
                        .WithMany("AccountBalanceCheckpoints")
                        .HasForeignKey("AccountingPeriodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("AccountingPeriod");
                });

            modelBuilder.Entity("Domain.Aggregates.AccountingPeriods.Transaction", b =>
                {
                    b.HasOne("Domain.Aggregates.AccountingPeriods.AccountingPeriod", "AccountingPeriod")
                        .WithMany("Transactions")
                        .HasForeignKey("AccountingPeriodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AccountingPeriod");
                });

            modelBuilder.Entity("Domain.Aggregates.AccountingPeriods.TransactionBalanceEvent", b =>
                {
                    b.HasOne("Domain.Aggregates.Accounts.Account", "Account")
                        .WithOne()
                        .HasForeignKey("Domain.Aggregates.AccountingPeriods.TransactionBalanceEvent", "AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Aggregates.AccountingPeriods.Transaction", "Transaction")
                        .WithMany("TransactionBalanceEvents")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("Domain.ValueObjects.FundAmount", b =>
                {
                    b.HasOne("Domain.Aggregates.AccountingPeriods.AccountBalanceCheckpoint", null)
                        .WithMany("FundBalances")
                        .HasForeignKey("AccountBalanceCheckpointId");

                    b.HasOne("Domain.Aggregates.Funds.Fund", "Fund")
                        .WithOne()
                        .HasForeignKey("Domain.ValueObjects.FundAmount", "FundId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Aggregates.AccountingPeriods.Transaction", null)
                        .WithMany("AccountingEntries")
                        .HasForeignKey("TransactionId");

                    b.Navigation("Fund");
                });

            modelBuilder.Entity("Domain.Aggregates.AccountingPeriods.AccountBalanceCheckpoint", b =>
                {
                    b.Navigation("FundBalances");
                });

            modelBuilder.Entity("Domain.Aggregates.AccountingPeriods.AccountingPeriod", b =>
                {
                    b.Navigation("AccountBalanceCheckpoints");

                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("Domain.Aggregates.AccountingPeriods.Transaction", b =>
                {
                    b.Navigation("AccountingEntries");

                    b.Navigation("TransactionBalanceEvents");
                });
#pragma warning restore 612, 618
        }
    }
}
