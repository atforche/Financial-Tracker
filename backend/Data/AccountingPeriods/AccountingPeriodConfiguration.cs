using Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.AccountingPeriods;

/// <summary>
/// EF Core entity configuration for an <see cref="AccountingPeriod"/>
/// </summary>
internal sealed class AccountingPeriodConfiguration : IEntityTypeConfiguration<AccountingPeriod>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AccountingPeriod> builder)
    {
        builder.HasKey(accountingPeriod => accountingPeriod.Id);
        builder.Property(accountingPeriod => accountingPeriod.Id)
            .HasConversion(accountingPeriodId => accountingPeriodId.Value, value => new AccountingPeriodId(value));

        builder.HasIndex(accountingPeriod => accountingPeriod.Name);
        builder.HasMany(accountingPeriod => accountingPeriod.ExpectedIncomeSources)
            .WithOne(source => source.AccountingPeriod)
            .HasForeignKey("AccountingPeriodId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(accountingPeriod => accountingPeriod.ExpectedIncomeSources).AutoInclude(false);
    }
}

/// <summary>
/// EF Core entity configuration for an <see cref="ExpectedIncomeSource"/>.
/// </summary>
internal sealed class ExpectedIncomeSourceConfiguration : IEntityTypeConfiguration<ExpectedIncomeSource>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ExpectedIncomeSource> builder)
    {
        builder.ToTable("ExpectedIncomeSources");
        builder.HasKey(source => source.Id);
        builder.Property(source => source.Id)
            .HasConversion(id => id.Value, value => new ExpectedIncomeSourceId(value))
            .ValueGeneratedNever();
        builder.Property(source => source.Name);
        builder.HasOne(source => source.Income)
            .WithOne()
            .HasForeignKey<Domain.Income.IncomeBreakdown>("ExpectedIncomeSourceId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(source => source.Income).AutoInclude();
        builder.OwnsMany<ExpectedIncomeDate>(nameof(ExpectedIncomeSource.ExpectedDates), dateBuilder =>
        {
            dateBuilder.ToTable("ExpectedIncomeSourceDates");
            dateBuilder.WithOwner().HasForeignKey("ExpectedIncomeSourceId");
            dateBuilder.Property<int>("Id");
            dateBuilder.HasKey("Id");
        });
    }
}