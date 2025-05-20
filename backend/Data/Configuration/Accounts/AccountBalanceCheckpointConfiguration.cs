using Domain;
using Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.Accounts;

/// <summary>
/// EF Core entity configuration for an Account Balance Checkpoint
/// </summary>
internal sealed class AccountBalanceCheckpointConfiguration : IEntityTypeConfiguration<AccountBalanceCheckpoint>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AccountBalanceCheckpoint> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasConversion(entityId => entityId.Value, value => new EntityId(value));

        builder.OwnsOne(accountBalanceCheckpoint => accountBalanceCheckpoint.AccountingPeriodKey);
        builder.Navigation(accountBalanceCheckpoint => accountBalanceCheckpoint.AccountingPeriodKey).AutoInclude();

        builder.HasMany(accountBalanceCheckpoint => accountBalanceCheckpoint.FundBalances)
            .WithOne()
            .HasForeignKey("AccountBalanceCheckpointId");
        builder.Navigation(accountBalanceCheckpoint => accountBalanceCheckpoint.FundBalances).AutoInclude();
    }
}