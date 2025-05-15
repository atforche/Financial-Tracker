using Domain.Accounts;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration;

/// <summary>
/// EF Core configuration for the Account entity
/// </summary>
internal sealed class AccountEntityConfiguration : EntityConfiguration<Account>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<Account> builder)
    {
        builder.HasIndex(account => account.Name).IsUnique();
        builder.Property(account => account.Type).HasConversion<string>();

        builder.HasMany(account => account.AccountBalanceCheckpoints)
            .WithOne(accountingBalanceCheckpoint => accountingBalanceCheckpoint.Account)
            .HasForeignKey("AccountId");
        builder.Navigation(account => account.AccountBalanceCheckpoints).AutoInclude();

        builder.HasOne(account => account.AccountAddedBalanceEvent)
            .WithOne()
            .HasForeignKey<Account>("AccountAddedBalanceEventId");
        builder.Navigation(account => account.AccountAddedBalanceEvent).AutoInclude();
    }
}

/// <summary>
/// EF Core configuration for the Account Balance Checkpoint entity
/// </summary>
internal sealed class AccountBalanceCheckpointEntityConfiguration : EntityConfiguration<AccountBalanceCheckpoint>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<AccountBalanceCheckpoint> builder)
    {
        builder.OwnsOne(accountBalanceCheckpoint => accountBalanceCheckpoint.AccountingPeriodKey);
        builder.Navigation(accountBalanceCheckpoint => accountBalanceCheckpoint.AccountingPeriodKey).AutoInclude();

        builder.HasMany(accountBalanceCheckpoint => accountBalanceCheckpoint.FundBalances)
            .WithOne()
            .HasForeignKey("AccountBalanceCheckpointId");
        builder.Navigation(accountBalanceCheckpoint => accountBalanceCheckpoint.FundBalances).AutoInclude();
    }
}

/// <summary>
/// EF Core configuration for the Account Added Balance Event entity
/// </summary>
internal sealed class AccountAddedBalanceEventEntityConfiguration : EntityConfiguration<AccountAddedBalanceEvent>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<AccountAddedBalanceEvent> builder)
    {
        builder.OwnsOne(accountAddedBalanceEvent => accountAddedBalanceEvent.AccountingPeriodKey);
        builder.Navigation(accountAddedBalanceEvent => accountAddedBalanceEvent.AccountingPeriodKey).AutoInclude();

        builder.HasMany(accountAddedBalanceEvent => accountAddedBalanceEvent.FundAmounts)
            .WithOne()
            .HasForeignKey("AccountAddedBalanceEventId");
        builder.Navigation(accountAddedBalanceEvent => accountAddedBalanceEvent.FundAmounts).AutoInclude();
    }
}