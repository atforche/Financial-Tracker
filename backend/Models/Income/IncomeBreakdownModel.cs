namespace Models.Income;

/// <summary>
/// Economic composition of an income receipt.
/// </summary>
public sealed class IncomeBreakdownModel
{
    /// <summary>
    /// Kind of income represented by this breakdown.
    /// </summary>
    public required IncomeBreakdownKindModel Kind { get; init; }

    /// <summary>
    /// Income deposited into tracked accounts.
    /// </summary>
    public required decimal TrackedAmount { get; init; }

    /// <summary>
    /// Income deposited into untracked accounts.
    /// </summary>
    public required decimal UntrackedAmount { get; init; }

    /// <summary>
    /// Total recognized income.
    /// </summary>
    public required decimal TotalAmount { get; init; }

    /// <summary>
    /// Payroll earnings, empty for simple income.
    /// </summary>
    public required IReadOnlyCollection<PayrollEarningModel> Earnings { get; init; }

    /// <summary>
    /// Employee payroll deductions, empty for simple income.
    /// </summary>
    public required IReadOnlyCollection<EmployeePayrollDeductionModel> EmployeeDeductions { get; init; }

    /// <summary>
    /// Employer contributions, empty for simple income.
    /// </summary>
    public required IReadOnlyCollection<EmployerContributionModel> EmployerContributions { get; init; }

    /// <summary>
    /// Tax withholdings, empty for simple income.
    /// </summary>
    public required IReadOnlyCollection<PayrollTaxWithholdingModel> TaxWithholdings { get; init; }

    /// <summary>
    /// State code for the payroll payment's state income wage base.
    /// </summary>
    public string? StateIncomeStateCode { get; init; }

    /// <summary>
    /// Annual payment count for projected payroll.
    /// </summary>
    public int? PayPeriodsPerYear { get; init; }
}