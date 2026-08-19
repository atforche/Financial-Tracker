namespace Models;

/// <summary>
/// Model representing an amount associated with a specific date.
/// </summary>
public class DateAmountModel
{
    /// <summary>
    /// The date associated with the amount.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// The total amount for the specified date.
    /// </summary>
    public required decimal Amount { get; init; }
}
