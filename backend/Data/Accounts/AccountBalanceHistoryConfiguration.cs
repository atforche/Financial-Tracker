using Domain.Accounts;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Accounts;

/// <summary>
/// EF Core entity configuration for a <see cref="AccountBalanceHistory"/>
/// </summary>
internal sealed class AccountBalanceHistoryConfiguration : IEntityTypeConfiguration<AccountBalanceHistory>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AccountBalanceHistory> builder)
    {
        builder.HasKey(accountBalanceHistory => accountBalanceHistory.Id);
        builder.Property(accountBalanceHistory => accountBalanceHistory.Id).HasConversion(accountBalanceHistoryId => accountBalanceHistoryId.Value, value => new AccountBalanceHistoryId(value));

        builder.HasOne(accountBalanceHistory => accountBalanceHistory.Account).WithMany();
        builder.Navigation(accountBalanceHistory => accountBalanceHistory.Account).AutoInclude();

        builder.Property(accountBalanceHistory => accountBalanceHistory.TransactionId)
            .HasConversion(transactionId => transactionId.Value, value => new TransactionId(value));
    }
}