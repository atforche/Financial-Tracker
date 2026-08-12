namespace Domain.Payroll;

/// <summary>
/// Economic disposition of an employee payroll deduction.
/// </summary>
public enum EmployeeDeductionDisposition
{
    /// <summary>
    /// The deduction reduces recognized income, such as an insurance premium.
    /// </summary>
    NonIncomeDeduction,

    /// <summary>
    /// The deduction becomes income in an untracked destination, such as a retirement contribution.
    /// </summary>
    UntrackedContribution,
}