using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Goals;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Goals;

/// <summary>
/// EF Core entity configuration for a <see cref="GoalBalanceHistory"/>.
/// </summary>
internal sealed class GoalBalanceHistoryConfiguration : IEntityTypeConfiguration<GoalBalanceHistory>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<GoalBalanceHistory> builder)
    {
        builder.HasKey(history => history.Id);
        builder.Property(history => history.Id)
            .HasConversion(id => id.Value, value => new GoalBalanceHistoryId(value));

        builder.Property(history => history.FundId)
            .HasConversion(id => id.Value, value => new FundId(value));

        builder.Property(history => history.AccountingPeriodId)
            .HasConversion(id => id.Value, value => new AccountingPeriodId(value));

        builder.Property(history => history.TransactionId)
            .HasConversion(id => id.Value, value => new TransactionId(value));

        builder.HasIndex(history => new { history.FundId, history.AccountingPeriodId, history.Date, history.Sequence })
            .IsUnique();
    }
}