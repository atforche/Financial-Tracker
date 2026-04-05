using Domain.AccountingPeriods;
using Domain.Budgets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Budgets;

/// <summary>
/// EF Core entity configuration for a <see cref="BudgetGoal"/>
/// </summary>
internal sealed class BudgetGoalConfiguration : IEntityTypeConfiguration<BudgetGoal>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<BudgetGoal> builder)
    {
        builder.HasKey(budgetGoal => budgetGoal.Id);
        builder.Property(budgetGoal => budgetGoal.Id).HasConversion(budgetGoalId => budgetGoalId.Value, value => new BudgetGoalId(value));

        builder.HasOne(budgetGoal => budgetGoal.Budget).WithMany().HasForeignKey("BudgetId");
        builder.Navigation(budgetGoal => budgetGoal.Budget).AutoInclude();

        builder.Property(budgetGoal => budgetGoal.AccountingPeriodId)
            .HasConversion(accountingPeriodId => accountingPeriodId.Value, value => new AccountingPeriodId(value));

        builder.HasMany(budgetGoal => budgetGoal.BudgetBalanceHistories)
            .WithOne(budgetBalanceHistory => budgetBalanceHistory.BudgetGoal);
        builder.Navigation(budgetGoal => budgetGoal.BudgetBalanceHistories).AutoInclude();
    }
}
