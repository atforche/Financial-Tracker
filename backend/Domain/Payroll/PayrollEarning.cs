namespace Domain.Payroll;

/// <summary>
/// Cash compensation included in a payroll payment.
/// </summary>
public sealed class PayrollEarning(
    string description,
    decimal amount)
{
    /// <summary>
    /// Description shown for the earning.
    /// </summary>
    public string Description { get; private set; } = description;

    /// <summary>
    /// Gross amount of the earning.
    /// </summary>
    public decimal Amount { get; private set; } = amount;

    /// <summary>
    /// Creates a snapshot of the current payroll earnings object.
    /// </summary>
    internal PayrollEarning Snapshot() => new(Description, Amount);

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private PayrollEarning() : this("", 0) { }
}