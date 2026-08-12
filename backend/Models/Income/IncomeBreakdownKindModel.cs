namespace Models.Income;

/// <summary>
/// Supported economic income breakdowns.
/// </summary>
public enum IncomeBreakdownKindModel
{
    /// <summary>
    /// Income without a payroll breakdown.
    /// </summary>
    Simple,

    /// <summary>
    /// An actual payroll payment.
    /// </summary>
    Payroll,

    /// <summary>
    /// A payroll payment projected from withholding elections and rules.
    /// </summary>
    ExpectedPayroll,
}