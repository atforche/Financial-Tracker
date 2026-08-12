using Domain.AccountingPeriods;
using Domain.Payroll.Withholding;

namespace Domain.Payroll;

/// <summary>
/// A payroll payment projected from withholding elections and published rules.
/// </summary>
public sealed class ExpectedPayrollPayment : PayrollPayment
{
    /// <summary>
    /// Number of payments made during a full year.
    /// </summary>
    public int PayPeriodsPerYear { get; private set; }

    /// <summary>
    /// Withholding elections and rules used for this projection.
    /// </summary>
    public PayrollWithholdingConfiguration WithholdingConfiguration { get; private set; }

    /// <summary>
    /// Current and year-to-date calculation facts used for this projection.
    /// </summary>
    public PayrollWithholdingContext WithholdingContext { get; private set; }

    internal ExpectedPayrollPayment(
        IEnumerable<PayrollEarning> earnings,
        IEnumerable<EmployeePayrollDeduction> employeeDeductions,
        IEnumerable<EmployerContribution> employerContributions,
        IEnumerable<PayrollTaxWithholding> taxWithholdings,
        int payPeriodsPerYear,
        PayrollWithholdingConfiguration withholdingConfiguration,
        PayrollWithholdingContext withholdingContext)
        : base(earnings, employeeDeductions, employerContributions, taxWithholdings)
    {
        PayPeriodsPerYear = payPeriodsPerYear;
        WithholdingConfiguration = withholdingConfiguration;
        WithholdingContext = withholdingContext;
    }

    private ExpectedPayrollPayment()
    {
        WithholdingConfiguration = null!;
        WithholdingContext = null!;
    }

    /// <inheritdoc/>
    public override PayrollPayment Snapshot() => new ExpectedPayrollPayment(
        Earnings.Select(earning => earning.Snapshot()),
        EmployeeDeductions.Select(deduction => deduction.Snapshot()),
        EmployerContributions.Select(contribution => contribution.Snapshot()),
        TaxWithholdings.Select(withholding => withholding.Snapshot()),
        PayPeriodsPerYear,
        WithholdingConfiguration.Snapshot(),
        WithholdingContext.Snapshot());
}