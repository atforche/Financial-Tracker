using Domain.AccountingPeriods;
using Domain.Goals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Goals;

/// <summary>
/// EF Core entity configuration for a <see cref="Goal"/>
/// </summary>
internal sealed class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.HasKey(goal => goal.Id);
        builder.Property(goal => goal.Id)
            .HasConversion(id => id.Value, value => new GoalId(value));

        builder.HasOne(goal => goal.Fund).WithMany();
        builder.Navigation(goal => goal.Fund).AutoInclude();

        builder.Property(goal => goal.GoalType).HasConversion<string>();

        builder.Property(goal => goal.AccountingPeriodId)
            .HasConversion(id => id.Value, value => new AccountingPeriodId(value));
    }
}