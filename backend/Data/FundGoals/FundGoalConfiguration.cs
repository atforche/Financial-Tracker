using Domain.FundGoals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.FundGoals;

/// <summary>
/// EF Core entity configuration for an <see cref="FundGoal"/>.
/// </summary>
internal sealed class FundGoalConfiguration : IEntityTypeConfiguration<FundGoal>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FundGoal> builder)
    {
        builder.HasKey(fundGoal => fundGoal.Id);
        builder.Property(fundGoal => fundGoal.Id)
            .HasConversion(id => id.Value, value => new FundGoalId(value));
        builder.HasOne(fundGoal => fundGoal.Fund).WithMany().HasForeignKey("FundId");
        builder.Navigation(fundGoal => fundGoal.Fund).AutoInclude();
        builder.HasOne(fundGoal => fundGoal.AccountingPeriod).WithMany();
        builder.Navigation(fundGoal => fundGoal.AccountingPeriod).AutoInclude();
        builder.HasIndex("FundId", "AccountingPeriodId").IsUnique();
        builder.HasIndex("FundId").IsUnique().HasFilter("\"AccountingPeriodId\" IS NULL");
    }
}
