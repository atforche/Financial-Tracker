namespace Domain.Payroll;

/// <summary>
/// An employee-funded deduction from gross cash compensation.
/// </summary>
public sealed class EmployeePayrollDeduction(
    string description,
    decimal amount,
    EmployeeDeductionDisposition disposition,
    PayrollTaxTreatment reducesTaxableWagesFor)
{
    /// <summary>
    /// Description shown for the deduction.
    /// </summary>
    public string Description { get; private set; } = description;

    /// <summary>
    /// Amount deducted from gross cash compensation.
    /// </summary>
    public decimal Amount { get; private set; } = amount;

    /// <summary>
    /// Determines whether the deduction becomes untracked income.
    /// </summary>
    public EmployeeDeductionDisposition Disposition { get; private set; } = disposition;

    /// <summary>
    /// Wage bases reduced by the deduction.
    /// </summary>
    public PayrollTaxTreatment ReducesTaxableWagesFor { get; private set; } = reducesTaxableWagesFor;

    /// <summary>
    /// Constructs an employee payroll deduction.
    /// </summary>
    internal EmployeePayrollDeduction Snapshot() => new(Description, Amount, Disposition, ReducesTaxableWagesFor);

    private EmployeePayrollDeduction() : this("", 0, EmployeeDeductionDisposition.NonIncomeDeduction, PayrollTaxTreatment.FullyTaxable) { }
}