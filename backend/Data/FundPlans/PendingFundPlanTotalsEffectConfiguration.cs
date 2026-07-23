using Domain.AccountingPeriods;
using Domain.FundPlans;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.FundPlans;

/// <summary>
/// EF Core configuration for <see cref="PendingFundPlanTotalsEffect"/>.
/// </summary>
internal sealed class PendingFundPlanTotalsEffectConfiguration : IEntityTypeConfiguration<PendingFundPlanTotalsEffect>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PendingFundPlanTotalsEffect> builder)
    {
        builder.HasKey(effect => effect.Id);
        builder.Property(effect => effect.Id).HasConversion(id => id.Value, value => new PendingFundPlanTotalsEffectId(value));

        builder.Property(effect => effect.FundId).HasConversion(id => id.Value, value => new FundId(value));

        builder.Property(effect => effect.AccountingPeriodId).HasConversion(id => id.Value, value => new AccountingPeriodId(value));

        builder.Property(effect => effect.TransactionId).HasConversion(id => id.Value, value => new TransactionId(value));

        builder.HasIndex(effect => new { effect.FundId, effect.AccountingPeriodId });
        builder.HasIndex(effect => new { effect.TransactionId, effect.FundId, effect.AccountingPeriodId }).IsUnique();
    }
}