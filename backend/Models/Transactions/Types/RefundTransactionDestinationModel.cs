using Models.Accounts;

namespace Models.Transactions.Types;

/// <summary>
/// Model representing the destination of a refund transaction response.
/// </summary>
public sealed class RefundTransactionDestinationModel
{
    /// <summary>
    /// Account for the destination.
    /// </summary>
    public required AccountBalanceEventModel Account { get; init; }

    /// <summary>
    /// Posted date for the destination.
    /// </summary>
    public DateOnly? PostedDate { get; init; }
}
