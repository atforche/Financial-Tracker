using Models.Accounts;
using Models.FundGoals;
using Models.Funds;

namespace Models.Transactions.Types;

/// <summary>
/// Model representing a destination of an income transaction response.
/// </summary>
public sealed class IncomeTransactionDestinationModel
{
    /// <summary>
    /// Account for the destination.
    /// </summary>
    public required AccountBalanceEventModel Account { get; init; }

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
    /// Fund Goal balance events for this destination.
    /// </summary>
    public required IReadOnlyCollection<FundGoalBalanceEventModel> FundGoals { get; init; }
}