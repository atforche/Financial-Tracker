using Domain.Budgets;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Budgets;

/// <summary>
/// EF Core entity configuration for a <see cref="BudgetBalanceHistory"/>
/// </summary>
internal sealed class BudgetBalanceHistoryConfiguration : IEntityTypeConfiguration<BudgetBalanceHistory>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<BudgetBalanceHistory> builder)
    {
        builder.HasKey(budgetBalanceHistory => budgetBalanceHistory.Id);
        builder.Property(budgetBalanceHistory => budgetBalanceHistory.Id)
            .HasConversion(budgetBalanceHistoryId => budgetBalanceHistoryId.Value, value => new BudgetBalanceHistoryId(value));

        builder.Property(budgetBalanceHistory => budgetBalanceHistory.TransactionId)
            .HasConversion(transactionId => transactionId.Value, value => new TransactionId(value));
    }
}
