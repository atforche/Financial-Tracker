using Domain;
using Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.Accounts;

/// <summary>
/// EF Core entity configuration for an Account Added Balance Event
/// </summary>
internal sealed class AccountAddedBalanceEventConfiguration : IEntityTypeConfiguration<AccountAddedBalanceEvent>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AccountAddedBalanceEvent> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasConversion(entityId => entityId.Value, value => new EntityId(value));

        builder.OwnsOne(accountAddedBalanceEvent => accountAddedBalanceEvent.AccountingPeriodKey);
        builder.Navigation(accountAddedBalanceEvent => accountAddedBalanceEvent.AccountingPeriodKey).AutoInclude();

        builder.HasMany(accountAddedBalanceEvent => accountAddedBalanceEvent.FundAmounts)
            .WithOne()
            .HasForeignKey("AccountAddedBalanceEventId");
        builder.Navigation(accountAddedBalanceEvent => accountAddedBalanceEvent.FundAmounts).AutoInclude();
    }
}