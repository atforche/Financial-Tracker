using Models.Funds;
using Models.Locations;

namespace Models.Transactions.Create;

/// <summary>
/// Model representing a source of a refund transaction create request.
/// </summary>
public sealed class CreateRefundTransactionSourceModel
{
    /// <summary>
    /// Optional account ID for the source account.
    /// </summary>
    public Guid? AccountId { get; init; }

    /// <summary>
    /// Optional location for the source.
    /// </summary>
    public LocationInputModel? Location { get; init; }

    /// <summary>
    /// Amount received from this source.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Fund assignments for this source.
    /// </summary>
    public required IReadOnlyCollection<CreateFundAmountModel> FundAssignments { get; init; }
}
