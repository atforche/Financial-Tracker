using Models.Locations;

namespace Models.Transactions.Update;

/// <summary>
/// Model representing a destination of an account transaction update request.
/// </summary>
public sealed class UpdateAccountTransactionDestinationModel
{
    /// <summary>
    /// Optional account ID for the destination account.
    /// </summary>
    public Guid? AccountId { get; init; }

    /// <summary>
    /// Optional location for the destination.
    /// </summary>
    public LocationInputModel? Location { get; init; }

    /// <summary>
    /// Amount directed to this destination.
    /// </summary>
    public required decimal Amount { get; init; }
}