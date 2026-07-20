using Domain.FundPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.FundPlans;

/// <summary>
/// EF Core entity configuration for an <see cref="FundPlan"/>.
/// </summary>
internal sealed class FundPlanConfiguration : IEntityTypeConfiguration<FundPlan>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FundPlan> builder)
    {
        builder.HasKey(plan => plan.Id);
        builder.Property(plan => plan.Id)
            .HasConversion(id => id.Value, value => new FundPlanId(value));
        builder.HasOne(plan => plan.Fund).WithMany().HasForeignKey("FundId");
        builder.Navigation(plan => plan.Fund).AutoInclude();
        builder.HasOne(plan => plan.AccountingPeriod).WithMany();
        builder.Navigation(plan => plan.AccountingPeriod).AutoInclude();
        builder.HasIndex("FundId", "AccountingPeriodId").IsUnique();
        builder.HasIndex("FundId").IsUnique().HasFilter("\"AccountingPeriodId\" IS NULL");
    }
}