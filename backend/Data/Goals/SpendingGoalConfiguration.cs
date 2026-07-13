using Domain.AccountingPeriods;
using Domain.Goals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Goals;

/// <summary>
/// EF Core entity configuration for a <see cref="SpendingGoal"/>.
/// </summary>
internal sealed class SpendingGoalConfiguration : IEntityTypeConfiguration<SpendingGoal>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SpendingGoal> builder)
    {
        builder.HasKey(goal => goal.Id);
        builder.Property(goal => goal.Id)
            .HasConversion(id => id.Value, value => new SpendingGoalId(value));

        builder.HasOne(goal => goal.Fund).WithMany();
        builder.Navigation(goal => goal.Fund).AutoInclude();

        builder.HasIndex("FundId", nameof(SpendingGoal.AccountingPeriodId)).IsUnique();
        builder.HasIndex("FundId")
            .IsUnique()
            .HasFilter("\"AccountingPeriodId\" IS NULL");

        builder.Property(goal => goal.SpendingGoalType).HasConversion<string>();

        builder.Property(goal => goal.AccountingPeriodId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : new AccountingPeriodId(value.Value));
    }
}