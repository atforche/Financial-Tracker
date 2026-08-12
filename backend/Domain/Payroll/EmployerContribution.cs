namespace Domain.Payroll;

/// <summary>
/// Compensation contributed by an employer without entering the employee's cash deposit.
/// </summary>
public sealed class EmployerContribution(string description, decimal amount)
{
    /// <summary>
    /// Description shown for the contribution.
    /// </summary>
    public string Description { get; private set; } = description;

    /// <summary>
    /// Amount contributed by the employer.
    /// </summary>
    public decimal Amount { get; private set; } = amount;

    internal EmployerContribution Snapshot() => new(Description, Amount);

    private EmployerContribution() : this("", 0) { }
}