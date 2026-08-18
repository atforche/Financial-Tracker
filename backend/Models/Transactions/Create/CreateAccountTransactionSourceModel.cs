using Models.Locations;

namespace Models.Transactions.Create;

/// <summary>
/// Model representing the source of an account transaction create request.
/// </summary>
public sealed class CreateAccountTransactionSourceModel
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
