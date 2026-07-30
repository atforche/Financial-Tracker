using Domain.Accounts;

namespace Domain.Transactions;

/// <summary>
/// Record representing a request to post a <see cref="Transaction"/>
/// </summary>
public record PostTransactionRequest
{
    /// <summary>
    /// Account to post the Transaction to
    /// </summary>
    public required AccountId AccountId { get; init; }

    /// <summary>
    /// Date the Transaction was posted
    /// </summary>
    public required DateOnly PostedDate { get; init; }
}