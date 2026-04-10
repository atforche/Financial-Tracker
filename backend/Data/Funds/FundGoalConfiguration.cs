using Domain.AccountingPeriods;
using Domain.Funds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Funds;

/// <summary>
/// EF Core entity configuration for a <see cref="FundGoal"/>
/// </summary>
internal sealed class FundGoalConfiguration : IEntityTypeConfiguration<FundGoal>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FundGoal> builder)
    {
        builder.HasKey(fundGoal => fundGoal.Id);
        builder.Property(fundGoal => fundGoal.Id)
            .HasConversion(id => id.Value, value => new FundGoalId(value));

        builder.HasOne(fundGoal => fundGoal.Fund).WithMany();
        builder.Navigation(fundGoal => fundGoal.Fund).AutoInclude();

        builder.Property(fundGoal => fundGoal.AccountingPeriodId)
            .HasConversion(id => id.Value, value => new AccountingPeriodId(value));
    }
}
