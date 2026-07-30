using Models.Funds;

namespace Models.Transactions.Update;

/// <summary>
/// Model representing a destination of an income transaction update request.
/// </summary>
public sealed class UpdateIncomeTransactionDestinationModel
{
    /// <summary>
    /// Account ID for the destination account.
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Amount directed to this destination.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Fund assignments for this destination.
    /// </summary>
    public required IReadOnlyCollection<CreateFundAmountModel> FundAssignments { get; init; }
}