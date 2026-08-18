using Models.Accounts;
using Models.Locations;

namespace Models.Transactions.Types;

/// <summary>
/// Model representing a destination of an account transaction response.
/// </summary>
public sealed class AccountTransactionDestinationModel
{
    /// <summary>
    /// Optional account for the destination.
    /// </summary>
    public AccountBalanceEventModel? Account { get; init; }

    /// <summary>
    /// Optional location for the destination.
    /// </summary>
    public LocationWithAmountModel? Location { get; init; }

    /// <summary>
    /// Amount directed to this destination.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Posted date for this destination.
    /// </summary>
    public required DateOnly? PostedDate { get; init; }
}
