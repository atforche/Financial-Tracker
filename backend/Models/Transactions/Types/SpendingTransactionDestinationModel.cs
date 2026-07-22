using Models.Accounts;
using Models.FundPlans;
using Models.Funds;

namespace Models.Transactions.Types;

/// <summary>
/// Model representing a destination of a spending transaction response.
/// </summary>
public sealed class SpendingTransactionDestinationModel
{
    /// <summary>
    /// Optional account for the destination.
    /// </summary>
    public AccountBalanceEventModel? Account { get; init; }

    /// <summary>
    /// Optional location for the destination.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Amount directed to this destination.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Posted date for this destination.
    /// </summary>
    public required DateOnly? PostedDate { get; init; }

    /// <summary>
    /// Fund assignments for this destination.
    /// </summary>
    public required IReadOnlyCollection<FundBalanceEventModel> FundAssignments { get; init; }

    /// <summary>
    /// Fund Plan balance events for this destination.
    /// </summary>
    public required IReadOnlyCollection<FundPlanBalanceEventModel> FundPlans { get; init; }
}