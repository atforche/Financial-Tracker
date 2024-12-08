using Domain.Aggregates.AccountingPeriods;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration;

/// <summary>
/// EF Core configuration for the Accounting Period entity
/// </summary>
internal sealed class AccountingPeriodEntityConfiguration : EntityConfigurationBase<AccountingPeriod>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<AccountingPeriod> builder)
    {
        builder.HasIndex(accountingPeriod => new { accountingPeriod.Year, accountingPeriod.Month }).IsUnique();

        builder.HasMany(accountingPeriod => accountingPeriod.Transactions)
            .WithOne(transaction => transaction.AccountingPeriod)
            .HasForeignKey("AccountingPeriodId");
        builder.Navigation(accountingPeriod => accountingPeriod.Transactions).AutoInclude();

        builder.HasMany(accountingPeriod => accountingPeriod.AccountBalanceCheckpoints)
            .WithOne(accountingBalanceCheckpoint => accountingBalanceCheckpoint.AccountingPeriod)
            .HasForeignKey("AccountingPeriodId");
        builder.Navigation(accountingPeriod => accountingPeriod.AccountBalanceCheckpoints).AutoInclude();
    }
}

/// <summary>
/// EF Core configuration for the Transaction entity
/// </summary>
internal sealed class TransactionEntityConfiguration : EntityConfigurationBase<Transaction>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasMany(transaction => transaction.AccountingEntries).WithOne().HasForeignKey("TransactionId");
        builder.Navigation(transaction => transaction.AccountingEntries).AutoInclude();

        builder.HasMany(transaction => transaction.TransactionBalanceEvents)
            .WithOne(transactionBalanceEvent => transactionBalanceEvent.Transaction)
            .HasForeignKey("TransactionId");
        builder.Navigation(transaction => transaction.TransactionBalanceEvents).AutoInclude();
    }
}

/// <summary>
/// EF Core configuration for the Account Balance Checkpoint entity
/// </summary>
internal sealed class AccountBalanceCheckpointEntityConfiguration : EntityConfigurationBase<AccountBalanceCheckpoint>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<AccountBalanceCheckpoint> builder)
    {
        builder.HasOne(accountBalanceCheckpoint => accountBalanceCheckpoint.Account).WithOne()
            .HasForeignKey<AccountBalanceCheckpoint>("AccountId");
        builder.Navigation(accountBalanceCheckpoint => accountBalanceCheckpoint.Account).IsRequired().AutoInclude();

        builder.HasMany(accountBalanceCheckpoint => accountBalanceCheckpoint.FundBalances).WithOne()
            .HasForeignKey("AccountBalanceCheckpointId");
        builder.Navigation(accountBalanceCheckpoint => accountBalanceCheckpoint.FundBalances).AutoInclude();
    }
}

/// <summary>
/// EF Core configuration for the Transaction Balance Event entity
/// </summary>
internal sealed class TransactionBalanceEventEntityConfiguration : EntityConfigurationBase<TransactionBalanceEvent>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<TransactionBalanceEvent> builder)
    {
        builder.HasIndex(transactionDetail => new { transactionDetail.EventDate, transactionDetail.EventSequence }).IsUnique();

        builder.HasOne(transactionBalanceEvent => transactionBalanceEvent.Account).WithOne()
            .HasForeignKey<TransactionBalanceEvent>("AccountId");
        builder.Navigation(transactionBalanceEvent => transactionBalanceEvent.Account).IsRequired().AutoInclude();
    }
}

/// <summary>
/// EF Core configuration for the Fund Amount entity
/// </summary>
internal sealed class FundAmountEntityConfiguration : IEntityTypeConfiguration<FundAmount>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FundAmount> builder)
    {
        builder.Property<int>("Id");
        builder.HasKey("Id");

        builder.HasOne(fundAmount => fundAmount.Fund).WithOne().HasForeignKey<FundAmount>("FundId");
        builder.Navigation(fundAmount => fundAmount.Fund).IsRequired().AutoInclude();
    }
}