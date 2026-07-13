using Domain.AccountingPeriods;
using Domain.Goals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Goals;

/// <summary>
/// EF Core entity configuration for an <see cref="AssignmentGoal"/>.
/// </summary>
internal sealed class AssignmentGoalConfiguration : IEntityTypeConfiguration<AssignmentGoal>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AssignmentGoal> builder)
    {
        builder.HasKey(goal => goal.Id);
        builder.Property(goal => goal.Id)
            .HasConversion(id => id.Value, value => new AssignmentGoalId(value));

        builder.HasOne(goal => goal.Fund).WithMany();
        builder.Navigation(goal => goal.Fund).AutoInclude();

        builder.HasIndex("FundId", nameof(AssignmentGoal.AccountingPeriodId)).IsUnique();
        builder.HasIndex("FundId")
            .IsUnique()
            .HasFilter("\"AccountingPeriodId\" IS NULL");

        builder.Property(goal => goal.AssignmentGoalType).HasConversion<string>();

        builder.Property(goal => goal.AccountingPeriodId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : new AccountingPeriodId(value.Value));
    }
}