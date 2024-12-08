namespace Domain.ValueObjects;

/// <summary>
/// Value object class representing an Account Balance associated with a Date
/// </summary>
public class AccountBalanceByDate
{
    /// <summary>
    /// Date for this Account Balance by Date
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Account Balance for this Account Balance by Date
    /// </summary>
    public required AccountBalance AccountBalance { get; init; }
}