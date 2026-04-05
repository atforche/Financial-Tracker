using Domain.Budgets;
using Domain.Funds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Budgets;

/// <summary>
/// EF Core entity configuration for a <see cref="Budget"/>
/// </summary>
internal sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(budget => budget.Id);
        builder.Property(budget => budget.Id).HasConversion(budgetId => budgetId.Value, value => new BudgetId(value));

        builder.HasIndex(budget => budget.Name).IsUnique();
        builder.Property(budget => budget.Type).HasConversion<string>();

        builder.Property(budget => budget.FundId)
            .HasConversion(fundId => fundId.Value, value => new FundId(value));
    }
}
