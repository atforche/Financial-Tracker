namespace Models.Income;

/// <summary>
/// Cash compensation included in a payroll payment.
/// </summary>
public sealed class PayrollEarningModel
{
    /// <summary>
    /// Description shown for the earning.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gross amount of the earning.
    /// </summary>
    public required decimal Amount { get; init; }
}