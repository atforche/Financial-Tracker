using Models.Locations;

namespace Models.Transactions.Update;

/// <summary>
/// Model representing the source of an account transaction update request.
/// </summary>
public sealed class UpdateAccountTransactionSourceModel
{
    /// <summary>
    /// Optional account ID for the source account.
    /// </summary>
    public Guid? AccountId { get; init; }

    /// <summary>
    /// Optional location for the source.
    /// </summary>
    public LocationInputModel? Location { get; init; }
}
