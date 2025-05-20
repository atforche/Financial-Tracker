using Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.Accounts;

/// <summary>
/// EF Core entity configuration for an Account
/// </summary>
internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasConversion(entityId => entityId.Value, value => new AccountId(value));

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