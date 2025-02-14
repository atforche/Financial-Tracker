using Rest.Models.FundAmount;

namespace Rest.Models.AccountingPeriod;

/// <summary>
/// REST model representing a request to create a Transaction
/// </summary>
public class CreateTransactionModel
{
    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.Transaction.TransactionDate"/>
    public required DateOnly TransactionDate { get; init; }

    /// <summary>
    /// ID of the Account being debited by this Transaction
    /// </summary>
    public Guid? DebitAccountId { get; init; }

    /// <summary>
    /// ID of the Account being credited by this Transaction
    /// </summary>
    public Guid? CreditAccountId { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.Transaction.AccountingEntries"/>
    public required IReadOnlyCollection<CreateFundAmountModel> AccountingEntries { get; init; }
}