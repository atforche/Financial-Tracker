namespace Models.Income;

/// <summary>
/// Employee-funded deduction from gross compensation.
/// </summary>
public sealed class EmployeePayrollDeductionModel
{
    /// <summary>
    /// Description shown for the deduction.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Amount deducted from gross compensation.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Whether the deduction reduces income or becomes an untracked contribution.
    /// </summary>
    public required int Disposition { get; init; }

    /// <summary>
    /// Bit flags identifying wage bases reduced by this deduction.
    /// </summary>
    public required int ReducesTaxableWagesFor { get; init; }
}