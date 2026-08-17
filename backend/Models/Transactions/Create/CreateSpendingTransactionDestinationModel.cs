using Models.Funds;
using Models.Locations;

namespace Models.Transactions.Create;

/// <summary>
/// Model representing a destination of a spending transaction create request.
/// </summary>
public sealed class CreateSpendingTransactionDestinationModel
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

    /// <summary>
    /// Fund assignments for this destination.
    /// </summary>
    public required IReadOnlyCollection<CreateFundAmountModel> FundAssignments { get; init; }
}