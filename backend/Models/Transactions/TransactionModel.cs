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
    /// ID of the Debit Account for the Transaction
    /// </summary>
    public Guid? DebitAccountId { get; init; }

    /// <summary>
    /// Name of the Debit Account for the Transaction
    /// </summary>
    public string? DebitAccountName { get; init; }

    /// <summary>
    /// Fund Amounts for the Debit Account of the Transaction
    /// </summary>
    public IEnumerable<FundAmountModel>? DebitFundAmounts { get; init; }

    /// <summary>
    /// ID of the Credit Account for the Transaction
    /// </summary>
    public Guid? CreditAccountId { get; init; }

    /// <summary>
    /// Name of the Credit Account for the Transaction
    /// </summary>
    public string? CreditAccountName { get; init; }

    /// <summary>
    /// Fund Amounts for the Credit Account of the Transaction
    /// </summary>
    public IEnumerable<FundAmountModel>? CreditFundAmounts { get; init; }
}