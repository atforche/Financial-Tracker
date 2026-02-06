using Models.Funds;

namespace Models.Transactions;

/// <summary>
/// Model representing a Transaction
/// </summary>
public class TransactionModel
{
    /// <summary>
    /// ID for the Transaction
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Accounting Period ID for the Transaction
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Name of the Accounting Period for the Transaction
    /// </summary>
    public required string AccountingPeriodName { get; init; }

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
    /// Amount for the Transaction
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Debit Account for the Transaction
    /// </summary>
    public TransactionAccountModel? DebitAccount { get; init; }

    /// <summary>
    /// Credit Account for the Transaction
    /// </summary>
    public TransactionAccountModel? CreditAccount { get; init; }

    /// <summary>
    /// Previous Fund Balances for the Transaction
    /// </summary>
    public required IReadOnlyCollection<FundBalanceModel> PreviousFundBalances { get; init; }

    /// <summary>
    /// New Fund Balances for the Transaction
    /// </summary>
    public required IReadOnlyCollection<FundBalanceModel> NewFundBalances { get; init; }
}