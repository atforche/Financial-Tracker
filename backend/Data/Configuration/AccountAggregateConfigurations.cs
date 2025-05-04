using Domain.Aggregates.Accounts;
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
        builder.HasMany(accountBalanceCheckpoint => accountBalanceCheckpoint.FundBalances).WithOne()
            .HasForeignKey("AccountBalanceCheckpointId");
        builder.Navigation(accountBalanceCheckpoint => accountBalanceCheckpoint.FundBalances).AutoInclude();
    }
}