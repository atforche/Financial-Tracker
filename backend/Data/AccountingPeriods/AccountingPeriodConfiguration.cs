using Domain.AccountingPeriods;
using Domain.Transactions.Income;
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
        builder.OwnsMany(accountingPeriod => accountingPeriod.ExpectedIncomeSources, sourceBuilder =>
        {
            sourceBuilder.ToTable("ExpectedIncomeSources");
            sourceBuilder.WithOwner(source => source.AccountingPeriod)
                .HasForeignKey("AccountingPeriodId");
            sourceBuilder.HasKey(source => source.Id);
            sourceBuilder.Property(source => source.Id)
                .HasConversion(id => id.Value, value => new ExpectedIncomeSourceId(value));
            sourceBuilder.Property(source => source.Name);
            sourceBuilder.OwnsMany<IncomeLine>(nameof(ExpectedIncomeSource.IncomeLines), lineBuilder =>
            {
                lineBuilder.ToTable("ExpectedIncomeSourceIncomeLines");
                lineBuilder.WithOwner().HasForeignKey("ExpectedIncomeSourceId");
                lineBuilder.Property<int>("Id");
                lineBuilder.HasKey("Id");
            });
            sourceBuilder.OwnsMany<IncomeDeduction>(nameof(ExpectedIncomeSource.IncomeDeductions), deductionBuilder =>
            {
                deductionBuilder.ToTable("ExpectedIncomeSourceIncomeDeductions");
                deductionBuilder.WithOwner().HasForeignKey("ExpectedIncomeSourceId");
                deductionBuilder.Property<int>("Id");
                deductionBuilder.HasKey("Id");
            });
            sourceBuilder.OwnsMany<ExpectedIncomeDate>(nameof(ExpectedIncomeSource.ExpectedDates), dateBuilder =>
            {
                dateBuilder.ToTable("ExpectedIncomeSourceDates");
                dateBuilder.WithOwner().HasForeignKey("ExpectedIncomeSourceId");
                dateBuilder.Property<int>("Id");
                dateBuilder.HasKey("Id");
            });
        });
        builder.Navigation(accountingPeriod => accountingPeriod.ExpectedIncomeSources).AutoInclude(false);
    }
}