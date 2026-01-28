using Domain.Funds;

namespace Domain.Transactions;

/// <summary>
/// Record representing a request to update a <see cref="Transaction"/>
/// </summary>
public record UpdateTransactionRequest
{
    /// <summary>
    /// Date for the Transaction
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Location for the Transaction
    /// </summary>
    public required string Location { get; init; }

    /// <summary>
    /// Description for the Transaction
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Debit Account for the Transaction
    /// </summary>
    public UpdateTransactionAccountRequest? DebitAccount { get; init; }

    /// <summary>
    /// Credit Account for the Transaction
    /// </summary>
    public UpdateTransactionAccountRequest? CreditAccount { get; init; }
}

/// <summary>
/// Record representing a request to update a <see cref="TransactionAccount"/>
/// </summary>
public record UpdateTransactionAccountRequest
{
    /// <summary>
    /// Fund Amounts for the Transaction Account
    /// </summary>
    public required IEnumerable<FundAmount> FundAmounts { get; init; }
}