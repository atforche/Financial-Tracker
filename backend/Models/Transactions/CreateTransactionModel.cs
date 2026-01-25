using Models.Funds;

namespace Models.Transactions;

/// <summary>
/// Model representing a request to create a Transaction.
/// </summary>
public class CreateTransactionModel
{
    /// <summary>
    /// Accounting Period for the Transaction
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

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
    public required CreateTransactionAccountModel? DebitAccount { get; init; }

    /// <summary>
    /// Credit Account for the Transaction
    /// </summary>
    public required CreateTransactionAccountModel? CreditAccount { get; init; }
}

/// <summary>
/// Model representing a request to create a Transaction Account.
/// </summary>
public class CreateTransactionAccountModel
{
    /// <summary>
    /// Account for the Transaction Account
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Fund Amounts for the Transaction Account
    /// </summary>
    public required IEnumerable<FundAmountModel> FundAmounts { get; init; }
}