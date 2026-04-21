using Domain.Funds;

namespace Domain.Transactions.CreateRequests;

/// <summary>
/// Record representing a request to create an <see cref="FundTransferTransaction"/>
/// </summary>
public record CreateFundTransferTransactionRequest : CreateTransactionRequest
{

    /// <summary>
    /// Debit Fund for this Fund Transfer Transaction
    /// </summary>
    public required Fund DebitFund { get; init; }

    /// <summary>
    /// Credit Fund for this Fund Transfer Transaction
    /// </summary>
    public required Fund CreditFund { get; init; }
}