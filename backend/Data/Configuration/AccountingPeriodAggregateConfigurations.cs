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

        builder.HasMany(accountingPeriod => accountingPeriod.FundConversions)
            .WithOne(fundConversion => fundConversion.AccountingPeriod)
            .HasForeignKey("AccountingPeriodId");
        builder.Navigation(accountingPeriod => accountingPeriod.FundConversions).AutoInclude();

        builder.HasMany(accountingPeriod => accountingPeriod.ChangeInValues)
            .WithOne(changeInValue => changeInValue.AccountingPeriod)
            .HasForeignKey("AccountingPeriodId");
        builder.Navigation(accountingPeriod => accountingPeriod.ChangeInValues).AutoInclude();

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
/// EF Core configuration for the Fund Conversion entity
/// </summary>
internal sealed class FundConversionEntityConfiguration : EntityConfigurationBase<FundConversion>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<FundConversion> builder)
    {
        builder.HasOne(fundConversion => fundConversion.Account).WithMany().HasForeignKey("AccountId");
        builder.Navigation(fundConversion => fundConversion.Account).IsRequired().AutoInclude();

        builder.HasOne(fundConversion => fundConversion.FromFund).WithMany().HasForeignKey("FromFundId");
        builder.Navigation(fundConversion => fundConversion.FromFund).IsRequired().AutoInclude();

        builder.HasOne(fundConversion => fundConversion.ToFund).WithMany().HasForeignKey("ToFundId");
        builder.Navigation(fundConversion => fundConversion.ToFund).IsRequired().AutoInclude();
    }
}

/// <summary>
/// EF Core configuration for the Change In Value entity
/// </summary>
internal sealed class ChangeInValueEntityConfiguration : EntityConfigurationBase<ChangeInValue>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<ChangeInValue> builder)
    {
        builder.HasOne(changeInValue => changeInValue.Account).WithMany().HasForeignKey("AccountId");
        builder.Navigation(changeInValue => changeInValue.Account).IsRequired().AutoInclude();

        builder.HasOne(changeInValue => changeInValue.AccountingEntry).WithOne().HasForeignKey<ChangeInValue>("FundAmountId");
        builder.Navigation(changeInValue => changeInValue.AccountingEntry).IsRequired().AutoInclude();
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
        builder.HasOne(accountBalanceCheckpoint => accountBalanceCheckpoint.Account).WithMany().HasForeignKey("AccountId");
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

        builder.HasOne(transactionBalanceEvent => transactionBalanceEvent.Account).WithMany().HasForeignKey("AccountId");
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

        builder.HasOne(fundAmount => fundAmount.Fund).WithMany().HasForeignKey("FundId");
        builder.Navigation(fundAmount => fundAmount.Fund).IsRequired().AutoInclude();
    }
}