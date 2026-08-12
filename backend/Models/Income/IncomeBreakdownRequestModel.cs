namespace Models.Income;

/// <summary>
/// Request model describing simple income or an actual payroll payment.
/// </summary>
public sealed class IncomeBreakdownRequestModel
{
    /// <summary>
    /// Kind of income to create.
    /// </summary>
    public required IncomeBreakdownKindModel Kind { get; init; }

    /// <summary>
    /// Tracked amount for simple income.
    /// </summary>
    public decimal? TrackedAmount { get; init; }

    /// <summary>
    /// Untracked amount for simple income.
    /// </summary>
    public decimal? UntrackedAmount { get; init; }

    /// <summary>
    /// Payroll earnings.
    /// </summary>
    public required IReadOnlyCollection<PayrollEarningModel> Earnings { get; init; }

    /// <summary>
    /// Employee payroll deductions.
    /// </summary>
    public required IReadOnlyCollection<EmployeePayrollDeductionModel> EmployeeDeductions { get; init; }

    /// <summary>
    /// Employer payroll contributions.
    /// </summary>
    public required IReadOnlyCollection<EmployerContributionModel> EmployerContributions { get; init; }

    /// <summary>
    /// Actual payroll tax withholdings.
    /// </summary>
    public required IReadOnlyCollection<PayrollTaxWithholdingModel> TaxWithholdings { get; init; }

    /// <summary>
    /// State code for the payroll payment's state income wage base.
    /// </summary>
    public string? StateIncomeStateCode { get; init; }
}