using Domain;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.Accounts;

/// <summary>
/// EF Core entity configuration for an <see cref="AccountBalanceCheckpoint"/>
/// </summary>
internal sealed class AccountBalanceCheckpointConfiguration : IEntityTypeConfiguration<AccountBalanceCheckpoint>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AccountBalanceCheckpoint> builder)
    {
        builder.HasKey(accountBalanceCheckpoint => accountBalanceCheckpoint.Id);
        builder.Property(accountBalanceCheckpoint => accountBalanceCheckpoint.Id)
            .HasConversion(accountBalanceCheckpointId => accountBalanceCheckpointId.Value, value => new AccountBalanceCheckpointId(value));

        builder.Property(accountBalanceCheckpoint => accountBalanceCheckpoint.AccountingPeriodId)
            .HasConversion(entityId => entityId.Value, value => new EntityId(value));
        builder.HasOne<AccountingPeriod>()
            .WithMany()
            .HasForeignKey(accountBalanceCheckpoint => accountBalanceCheckpoint.AccountingPeriodId);

        builder.HasMany(accountBalanceCheckpoint => accountBalanceCheckpoint.FundBalances)
            .WithOne()
            .HasForeignKey("AccountBalanceCheckpointId");
        builder.Navigation(accountBalanceCheckpoint => accountBalanceCheckpoint.FundBalances).AutoInclude();
    }
}