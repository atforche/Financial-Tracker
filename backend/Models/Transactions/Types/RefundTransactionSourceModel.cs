using Models.Accounts;
using Models.FundGoals;
using Models.Funds;
using Models.Locations;

namespace Models.Transactions.Types;

/// <summary>
/// Model representing a source of a refund transaction response.
/// </summary>
public sealed class RefundTransactionSourceModel
{
    /// <summary>
    /// Optional account for the source.
    /// </summary>
    public AccountBalanceEventModel? Account { get; init; }

    /// <summary>
    /// Optional location for the source.
    /// </summary>
    public LocationWithAmountModel? Location { get; init; }

    /// <summary>
    /// Amount received from this source.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Posted date for this source.
    /// </summary>
    public DateOnly? PostedDate { get; init; }

    /// <summary>
    /// Fund assignments for this source.
    /// </summary>
    public required IReadOnlyCollection<FundBalanceEventModel> FundAssignments { get; init; }

    /// <summary>
    /// Fund Goal balance events for this source.
    /// </summary>
    public required IReadOnlyCollection<FundGoalBalanceEventModel> FundGoals { get; init; }
}
