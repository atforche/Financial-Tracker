using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions.CreateRequests;

/// <summary>
/// Record representing a request to create an <see cref="IncomeTransaction"/>
/// </summary>
public record CreateIncomeTransactionRequest : CreateTransactionRequest
{
    /// <summary>
    /// Account for this Income Transaction
    /// </summary>
    public required Account Account { get; init; }

    /// <summary>
    /// Posted Date for this Income Transaction
    /// </summary>
    public DateOnly? PostedDate { get; init; }

    /// <summary>
    /// Fund assignments for the amount of this Income Transaction
    /// </summary>
    public required IReadOnlyCollection<FundAmount> FundAssignments { get; init; }

    /// <summary>
    /// True if this transaction is the initial transaction for the account, false otherwise
    /// </summary>
    public required bool IsInitialTransactionForAccount { get; init; }
}