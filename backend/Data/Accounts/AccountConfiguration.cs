using Domain.AccountingPeriods;
using Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Accounts;

/// <summary>
/// EF Core entity configuration for an <see cref="Account"/>
/// </summary>
internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Id).HasConversion(accountId => accountId.Value, value => new AccountId(value));

        builder.HasIndex(account => account.Name).IsUnique();
        builder.Property(account => account.Type).HasConversion<string>();

        builder.Property(account => account.OpeningAccountingPeriodId).HasConversion(
            accountingPeriodId => accountingPeriodId == null ? (Guid?)null : accountingPeriodId.Value,
            value => value == null ? null : new AccountingPeriodId(value.Value));
    }
}