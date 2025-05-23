using Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration;

/// <summary>
/// EF Core configuration for the Accounting Period entity
/// </summary>
internal sealed class AccountingPeriodEntityConfiguration : EntityConfiguration<AccountingPeriod>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<AccountingPeriod> builder)
    {
        builder.OwnsOne(accountingPeriod => accountingPeriod.Key);

        builder.HasMany(accountingPeriod => accountingPeriod.Transactions)
            .WithOne(transaction => transaction.AccountingPeriod)
            .HasForeignKey("AccountingPeriodId");
        builder.Navigation(accountingPeriod => accountingPeriod.Transactions).AutoInclude();

        builder.HasMany(accountingPeriod => accountingPeriod.FundConversions)
            .WithOne()
            .HasForeignKey("AccountingPeriodId");
        builder.Navigation(accountingPeriod => accountingPeriod.FundConversions).AutoInclude();

        builder.HasMany(accountingPeriod => accountingPeriod.ChangeInValues)
            .WithOne()
            .HasForeignKey("AccountingPeriodId");
        builder.Navigation(accountingPeriod => accountingPeriod.ChangeInValues).AutoInclude();

        builder.HasMany(accountingPeriod => accountingPeriod.AccountAddedBalanceEvents)
            .WithOne()
            .HasForeignKey("AccountingPeriodId");
        builder.Navigation(accountingPeriod => accountingPeriod.AccountAddedBalanceEvents).AutoInclude();
    }
}

/// <summary>
/// EF Core configuration for the Transaction entity
/// </summary>
internal sealed class TransactionEntityConfiguration : EntityConfiguration<Transaction>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasMany(transaction => transaction.AccountingEntries)
            .WithOne()
            .HasForeignKey("TransactionId");
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
internal sealed class FundConversionEntityConfiguration : EntityConfiguration<FundConversion>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<FundConversion> builder)
    {
        builder.OwnsOne(fundConversion => fundConversion.AccountingPeriodKey);
        builder.Navigation(fundConversion => fundConversion.AccountingPeriodKey).AutoInclude();

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
internal sealed class ChangeInValueEntityConfiguration : EntityConfiguration<ChangeInValue>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<ChangeInValue> builder)
    {
        builder.OwnsOne(changeInValue => changeInValue.AccountingPeriodKey);
        builder.Navigation(changeInValue => changeInValue.AccountingPeriodKey).AutoInclude();

        builder.HasOne(changeInValue => changeInValue.Account).WithMany().HasForeignKey("AccountId");
        builder.Navigation(changeInValue => changeInValue.Account).IsRequired().AutoInclude();

        builder.HasOne(changeInValue => changeInValue.AccountingEntry).WithOne().HasForeignKey<ChangeInValue>("FundAmountId");
        builder.Navigation(changeInValue => changeInValue.AccountingEntry).IsRequired().AutoInclude();
    }
}

/// <summary>
/// EF Core configuration for the Transaction Balance Event entity
/// </summary>
internal sealed class TransactionBalanceEventEntityConfiguration : EntityConfiguration<TransactionBalanceEvent>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<TransactionBalanceEvent> builder)
    {
        builder.OwnsOne(transactionBalanceEvent => transactionBalanceEvent.AccountingPeriodKey);
        builder.Navigation(transactionBalanceEvent => transactionBalanceEvent.AccountingPeriodKey).AutoInclude();

        builder.HasIndex(transactionBalanceEvent => new { transactionBalanceEvent.EventDate, transactionBalanceEvent.EventSequence }).IsUnique();

        builder.HasOne(transactionBalanceEvent => transactionBalanceEvent.Account).WithMany().HasForeignKey("AccountId");
        builder.Navigation(transactionBalanceEvent => transactionBalanceEvent.Account).IsRequired().AutoInclude();
    }
}