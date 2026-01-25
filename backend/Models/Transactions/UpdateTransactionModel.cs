using Models.Funds;

namespace Models.Transactions;

/// <summary>
/// Model representing a request to update a Transaction.
/// </summary>
public class UpdateTransactionModel
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
    public required UpdateTransactionAccountModel? DebitAccount { get; init; }

    /// <summary>
    /// Credit Account for the Transaction
    /// </summary>
    public required UpdateTransactionAccountModel? CreditAccount { get; init; }
}

/// <summary>
/// Model representing a request to update a Transaction Account.
/// </summary>
public class UpdateTransactionAccountModel
{
    /// <summary>
    /// Fund Amounts for the Transaction Account
    /// </summary>
    public required IEnumerable<FundAmountModel> FundAmounts { get; init; }
}