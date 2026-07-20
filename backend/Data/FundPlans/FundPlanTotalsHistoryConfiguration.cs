using Domain.AccountingPeriods;
using Domain.FundPlans;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.FundPlans;

/// <summary>
/// EF Core configuration for Fund Plan totals history.
/// </summary>
internal sealed class FundPlanTotalsHistoryConfiguration : IEntityTypeConfiguration<FundPlanTotalsHistory>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FundPlanTotalsHistory> builder)
    {
        builder.HasKey(history => history.Id);
        builder.Property(history => history.Id)
            .HasConversion(id => id.Value, value => new FundPlanTotalsHistoryId(value));
        builder.Property(history => history.FundId)
            .HasConversion(id => id.Value, value => new FundId(value));
        builder.Property(history => history.AccountingPeriodId)
            .HasConversion(id => id.Value, value => new AccountingPeriodId(value));
        builder.Property(history => history.TransactionId)
            .HasConversion(id => id.Value, value => new TransactionId(value));
        builder.HasIndex(history => new
        {
            history.FundId,
            history.AccountingPeriodId,
            history.Date,
            history.Sequence,
        }).IsUnique();
    }
}