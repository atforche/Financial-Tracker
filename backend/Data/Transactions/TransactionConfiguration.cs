using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Locations;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Data.Transactions;

/// <summary>
/// EF Core entity configuration for <see cref="Transaction"/> and its subtypes.
/// </summary>
internal sealed class TransactionConfiguration :
    IEntityTypeConfiguration<Transaction>,
    IEntityTypeConfiguration<SpendingTransaction>,
    IEntityTypeConfiguration<IncomeTransaction>,
    IEntityTypeConfiguration<AccountTransaction>,
    IEntityTypeConfiguration<FundTransaction>
{
    private static readonly ValueConverter<AccountId, Guid> AccountIdConverter =
        new(accountId => accountId.Value, value => new AccountId(value));

    private static readonly ValueConverter<AccountId?, Guid?> NullableAccountIdConverter =
        new(accountId => ConvertNullableAccountIdToGuid(accountId), value => ConvertGuidToNullableAccountId(value));

    private static readonly ValueConverter<FundId, Guid> FundIdConverter =
        new(fundId => fundId.Value, value => new FundId(value));

    private static readonly ValueConverter<LocationId?, Guid?> NullableLocationIdConverter =
        new(locationId => ConvertNullableLocationIdToGuid(locationId), value => ConvertGuidToNullableLocationId(value));

    private static Guid? ConvertNullableAccountIdToGuid(AccountId? accountId)
    {
        if (accountId is null)
        {
            return null;
        }
        return accountId.Value;
    }

    private static AccountId? ConvertGuidToNullableAccountId(Guid? value)
    {
        if (value is null)
        {
            return null;
        }
        return new AccountId(value.Value);
    }

    private static Guid? ConvertNullableLocationIdToGuid(LocationId? locationId) => locationId?.Value;

    private static LocationId? ConvertGuidToNullableLocationId(Guid? value) => value == null ? null : new LocationId(value.Value);

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(transaction => transaction.Id);
        builder.Property(transaction => transaction.Id).HasConversion(transactionId => transactionId.Value, value => new TransactionId(value));

        builder.Property(transaction => transaction.AccountingPeriodId)
            .HasConversion(accountingPeriodId => accountingPeriodId.Value, value => new AccountingPeriodId(value));

        builder.HasIndex(transaction => new { transaction.Date, transaction.Sequence }).IsUnique();

        builder.HasDiscriminator(transaction => transaction.Type)
            .HasValue<SpendingTransaction>(TransactionType.Spending)
            .HasValue<IncomeTransaction>(TransactionType.Income)
            .HasValue<AccountTransaction>(TransactionType.Account)
            .HasValue<FundTransaction>(TransactionType.Fund);
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SpendingTransaction> builder)
    {
        builder.OwnsOne(transaction => transaction.Source, sourceBuilder =>
        {
            sourceBuilder.Property<AccountId>("AccountId")
                .HasColumnName("SpendingTransaction_DebitAccountId")
                .HasConversion(AccountIdConverter);
            sourceBuilder.Property(source => source.PostedDate)
                .HasColumnName("SpendingTransaction_DebitPostedDate");
            sourceBuilder.HasOne(source => source.Account).WithMany().HasForeignKey("AccountId");
            sourceBuilder.Navigation(source => source.Account).AutoInclude();
        });

        builder.OwnsMany<SpendingTransactionDestination>(nameof(SpendingTransaction.Destinations), destinationBuilder =>
        {
            destinationBuilder.ToTable("SpendingTransactionDestinations");
            destinationBuilder.WithOwner().HasForeignKey("SpendingTransactionId");
            destinationBuilder.Property<int>("Id");
            destinationBuilder.HasKey("Id");
            destinationBuilder.Property<AccountId?>("AccountId")
                .HasColumnName("CreditAccountId")
                .HasConversion(NullableAccountIdConverter);
            destinationBuilder.Property(destination => destination.PostedDate)
                .HasColumnName("CreditPostedDate");
            destinationBuilder.Property<LocationId?>("LocationId")
                .HasColumnName("LocationId")
                .HasConversion(NullableLocationIdConverter);
            destinationBuilder.Property(destination => destination.Amount)
                .HasColumnName("Amount");
            destinationBuilder.HasOne(destination => destination.Account).WithMany().HasForeignKey("AccountId");
            destinationBuilder.Navigation(destination => destination.Account).AutoInclude();
            destinationBuilder.HasOne(destination => destination.Location).WithMany().HasForeignKey("LocationId").OnDelete(DeleteBehavior.Restrict);
            destinationBuilder.Navigation(destination => destination.Location).AutoInclude();

            destinationBuilder.OwnsMany<FundAmount>(nameof(SpendingTransactionDestination.FundAssignments), fundAssignmentBuilder =>
            {
                fundAssignmentBuilder.ToTable("SpendingTransactionDestinationFundAssignments");
                fundAssignmentBuilder.WithOwner().HasForeignKey("DestinationId");
                fundAssignmentBuilder.Property<int>("Id");
                fundAssignmentBuilder.HasKey("Id");
                fundAssignmentBuilder.Property(fundAssignment => fundAssignment.FundId)
                    .HasConversion(FundIdConverter);
            });
        });
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<IncomeTransaction> builder)
    {
        builder.Property(transaction => transaction.TrackedAmount);

        builder.OwnsOne(transaction => transaction.Source, sourceBuilder =>
        {
            sourceBuilder.Property<AccountId?>("AccountId")
                .HasColumnName("IncomeTransaction_SourceAccountId")
                .HasConversion(NullableAccountIdConverter);
            sourceBuilder.Property(source => source.PostedDate)
                .HasColumnName("IncomeTransaction_SourcePostedDate");
            sourceBuilder.Property<LocationId?>("LocationId")
                .HasColumnName("IncomeTransaction_SourceLocationId")
                .HasConversion(NullableLocationIdConverter);
            sourceBuilder.HasOne(source => source.Account).WithMany().HasForeignKey("AccountId");
            sourceBuilder.Navigation(source => source.Account).AutoInclude();
            sourceBuilder.HasOne(source => source.Location).WithMany().HasForeignKey("LocationId").OnDelete(DeleteBehavior.Restrict);
            sourceBuilder.Navigation(source => source.Location).AutoInclude();

            sourceBuilder.OwnsMany<IncomeLine>(nameof(IncomeTransactionSource.IncomeLines), incomeLineBuilder =>
            {
                incomeLineBuilder.ToTable("IncomeTransactionIncomeLines");
                incomeLineBuilder.WithOwner().HasForeignKey("IncomeTransactionId");
                incomeLineBuilder.Property<int>("Id");
                incomeLineBuilder.HasKey("Id");
            });

            sourceBuilder.OwnsMany<IncomeDeduction>(nameof(IncomeTransactionSource.IncomeDeductions), incomeDeductionBuilder =>
            {
                incomeDeductionBuilder.ToTable("IncomeTransactionIncomeDeductions");
                incomeDeductionBuilder.WithOwner().HasForeignKey("IncomeTransactionId");
                incomeDeductionBuilder.Property<int>("Id");
                incomeDeductionBuilder.HasKey("Id");
            });
        });

        builder.OwnsMany<IncomeTransactionDestination>(nameof(IncomeTransaction.Destinations), destinationBuilder =>
        {
            destinationBuilder.ToTable("IncomeTransactionIncomeDestinations");
            destinationBuilder.WithOwner().HasForeignKey("IncomeTransactionId");
            destinationBuilder.Property<int>("Id");
            destinationBuilder.HasKey("Id");
            destinationBuilder.Property(destination => destination.PostedDate);
            destinationBuilder.Property(destination => destination.Amount);
            destinationBuilder.Property<AccountId>("AccountId")
                .HasConversion(AccountIdConverter);
            destinationBuilder.HasOne(destination => destination.Account).WithMany().HasForeignKey("AccountId");
            destinationBuilder.Navigation(destination => destination.Account).AutoInclude();

            destinationBuilder.OwnsMany<IncomeFundAmount>(nameof(IncomeTransactionDestination.FundAssignments), fundAssignmentBuilder =>
            {
                fundAssignmentBuilder.ToTable("IncomeTransactionIncomeDestinationFundAssignments");
                fundAssignmentBuilder.WithOwner().HasForeignKey("IncomeDestinationId");
                fundAssignmentBuilder.Property<int>("Id");
                fundAssignmentBuilder.HasKey("Id");
                fundAssignmentBuilder.Property(fundAssignment => fundAssignment.FundId)
                    .HasConversion(FundIdConverter);
            });
        });
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AccountTransaction> builder)
    {
        builder.OwnsOne(transaction => transaction.Source, sourceBuilder =>
        {
            sourceBuilder.Property<AccountId?>("AccountId")
                .HasColumnName("AccountTransaction_DebitAccountId")
                .HasConversion(NullableAccountIdConverter);
            sourceBuilder.Property(source => source.PostedDate)
                .HasColumnName("AccountTransaction_DebitPostedDate");
            sourceBuilder.Property<LocationId?>("LocationId")
                .HasColumnName("AccountTransaction_SourceLocationId")
                .HasConversion(NullableLocationIdConverter);
            sourceBuilder.HasOne(source => source.Account).WithMany().HasForeignKey("AccountId");
            sourceBuilder.Navigation(source => source.Account).AutoInclude();
            sourceBuilder.HasOne(source => source.Location).WithMany().HasForeignKey("LocationId").OnDelete(DeleteBehavior.Restrict);
            sourceBuilder.Navigation(source => source.Location).AutoInclude();
        });

        builder.OwnsMany<AccountTransactionDestination>(nameof(AccountTransaction.Destinations), destinationBuilder =>
        {
            destinationBuilder.ToTable("AccountTransactionDestinations");
            destinationBuilder.WithOwner().HasForeignKey("AccountTransactionId");
            destinationBuilder.Property<int>("Id");
            destinationBuilder.HasKey("Id");
            destinationBuilder.Property<AccountId?>("AccountId")
                .HasColumnName("CreditAccountId")
                .HasConversion(NullableAccountIdConverter);
            destinationBuilder.Property(destination => destination.PostedDate)
                .HasColumnName("CreditPostedDate");
            destinationBuilder.Property<LocationId?>("LocationId")
                .HasColumnName("LocationId")
                .HasConversion(NullableLocationIdConverter);
            destinationBuilder.Property(destination => destination.Amount)
                .HasColumnName("Amount");
            destinationBuilder.HasOne(destination => destination.Account).WithMany().HasForeignKey("AccountId");
            destinationBuilder.Navigation(destination => destination.Account).AutoInclude();
            destinationBuilder.HasOne(destination => destination.Location).WithMany().HasForeignKey("LocationId").OnDelete(DeleteBehavior.Restrict);
            destinationBuilder.Navigation(destination => destination.Location).AutoInclude();
        });
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FundTransaction> builder)
    {
        builder.OwnsOne(transaction => transaction.Source, sourceBuilder =>
        {
            sourceBuilder.Property<FundId>("FundId")
                .HasColumnName("FundTransaction_DebitFundId")
                .HasConversion(FundIdConverter);
            sourceBuilder.HasOne(source => source.Fund).WithMany().HasForeignKey("FundId");
            sourceBuilder.Navigation(source => source.Fund).AutoInclude();
        });

        builder.OwnsMany<FundTransactionDestination>(nameof(FundTransaction.Destinations), destinationBuilder =>
        {
            destinationBuilder.ToTable("FundTransactionDestinations");
            destinationBuilder.WithOwner().HasForeignKey("FundTransactionId");
            destinationBuilder.Property<int>("Id");
            destinationBuilder.HasKey("Id");
            destinationBuilder.Property<FundId>("FundId")
                .HasColumnName("CreditFundId")
                .HasConversion(FundIdConverter);
            destinationBuilder.Property(destination => destination.Amount)
                .HasColumnName("Amount");
            destinationBuilder.HasOne(destination => destination.Fund).WithMany().HasForeignKey("FundId");
            destinationBuilder.Navigation(destination => destination.Fund).AutoInclude();
        });
    }
}