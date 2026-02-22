using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions;

/// <summary>
/// Record representing a request to create a <see cref="Transaction"/>
/// </summary>
public record CreateTransactionRequest
{
    /// <summary>
    /// Accounting Period for the Transaction
    /// </summary>
    public required AccountingPeriodId AccountingPeriod { get; init; }

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
    public CreateTransactionAccountRequest? DebitAccount { get; init; }

    /// <summary>
    /// Credit Account for the Transaction
    /// </summary>
    public CreateTransactionAccountRequest? CreditAccount { get; init; }

    /// <summary>
    /// True if this transaction is the initial transaction for the account, false otherwise
    /// </summary>
    public bool IsInitialTransactionForAccount { get; init; }
}

/// <summary>
/// Record representing a request to create a <see cref="TransactionAccount"/>
/// </summary>
public record CreateTransactionAccountRequest
{
    /// <summary>
    /// Account for the Transaction Account
    /// </summary>
    public required Account Account { get; init; }

    /// <summary>
    /// Fund Amounts for the Transaction Account
    /// </summary>
    public required IEnumerable<FundAmount> FundAmounts { get; init; }
}