using Domain.Transactions;
using Rest.Models.Funds;

namespace Rest.Models.Transactions;

/// <summary>
/// REST model representing a request to create a <see cref="Transaction"/>
/// </summary>
public class CreateTransactionModel
{
    /// <inheritdoc cref="Transaction.AccountingPeriodId"/>
    public required Guid AccountingPeriodId { get; init; }

    /// <inheritdoc cref="Transaction.Date"/>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// ID of the Account being debited by this Transaction
    /// </summary>
    public Guid? DebitAccountId { get; init; }

    /// <summary>
    /// ID of the Account being credited by this Transaction
    /// </summary>
    public Guid? CreditAccountId { get; init; }

    /// <inheritdoc cref="Transaction.FundAmounts"/>
    public required IReadOnlyCollection<CreateFundAmountModel> FundAmounts { get; init; }
}