using Domain.Funds;

namespace Domain.Transactions.Funds;

/// <summary>
/// Record representing a request to create an <see cref="FundTransaction"/>
/// </summary>
public record CreateFundTransactionRequest : CreateTransactionRequest
{
    /// <summary>
    /// Debit Fund for this Fund Transaction
    /// </summary>
    public required Fund DebitFund { get; init; }

    /// <summary>
    /// Credit Fund for this Fund Transaction
    /// </summary>
    public required Fund CreditFund { get; init; }
}