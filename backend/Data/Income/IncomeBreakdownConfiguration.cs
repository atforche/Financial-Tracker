using Domain.Income;
using Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Income;

/// <summary>
/// EF Core configuration for relational income breakdown persistence.
/// </summary>
internal sealed class IncomeBreakdownConfiguration :
    IEntityTypeConfiguration<IncomeBreakdown>,
    IEntityTypeConfiguration<SimpleIncome>,
    IEntityTypeConfiguration<PayrollPayment>,
    IEntityTypeConfiguration<ExpectedPayrollPayment>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<IncomeBreakdown> builder)
    {
        builder.ToTable("IncomeBreakdowns");
        builder.HasKey(income => income.Id);
        builder.Property(income => income.Id)
            .HasConversion(id => id.Value, value => new IncomeBreakdownId(value))
            .ValueGeneratedNever();
        builder.HasDiscriminator<string>("IncomeType")
            .HasValue<SimpleIncome>("Simple")
            .HasValue<PayrollPayment>("Payroll")
            .HasValue<ExpectedPayrollPayment>("ExpectedPayroll");
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SimpleIncome> builder)
    {
        builder.Property(income => income.TrackedAmount)
            .HasField("_trackedAmount")
            .HasColumnName("TrackedAmount");
        builder.Property(income => income.UntrackedAmount)
            .HasField("_untrackedAmount")
            .HasColumnName("UntrackedAmount");
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PayrollPayment> builder)
    {
        builder.Property(payment => payment.StateIncomeStateCode)
            .HasColumnName("StateIncomeStateCode");
        builder.OwnsMany(payment => payment.Earnings, earningBuilder =>
        {
            ConfigureOwnedCollection(earningBuilder, "PayrollEarnings", "IncomeBreakdownId");
        });
        builder.OwnsMany(payment => payment.EmployeeDeductions, deductionBuilder =>
        {
            ConfigureOwnedCollection(deductionBuilder, "EmployeePayrollDeductions", "IncomeBreakdownId");
        });
        builder.OwnsMany(payment => payment.EmployerContributions, contributionBuilder =>
            ConfigureOwnedCollection(contributionBuilder, "EmployerContributions", "IncomeBreakdownId"));
        builder.OwnsMany(payment => payment.TaxWithholdings, withholdingBuilder =>
        {
            ConfigureOwnedCollection(withholdingBuilder, "PayrollTaxWithholdings", "IncomeBreakdownId");
            withholdingBuilder.OwnsOne(withholding => withholding.Jurisdiction, jurisdictionBuilder =>
            {
                jurisdictionBuilder.Property(jurisdiction => jurisdiction.CountryCode).HasColumnName("CountryCode");
                jurisdictionBuilder.Property(jurisdiction => jurisdiction.SubdivisionCode).HasColumnName("SubdivisionCode");
                jurisdictionBuilder.Property(jurisdiction => jurisdiction.Locality).HasColumnName("Locality");
            });
        });
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ExpectedPayrollPayment> builder)
    {
        builder.Property(payment => payment.PayPeriodsPerYear);
        builder.Ignore(payment => payment.WithholdingConfiguration);
        builder.Ignore(payment => payment.WithholdingContext);
    }

    private static void ConfigureOwnedCollection<TOwned>(
        OwnedNavigationBuilder<PayrollPayment, TOwned> builder,
        string tableName,
        string foreignKeyName)
        where TOwned : class
    {
        builder.ToTable(tableName);
        builder.WithOwner().HasForeignKey(foreignKeyName);
        builder.Property<int>("Id");
        builder.HasKey("Id");
    }
}
