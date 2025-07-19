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

    /// <inheritdoc cref="Transaction.DebitAccountId"/>
    public Guid? DebitAccountId { get; init; }

    /// <inheritdoc cref="Transaction.DebitFundAmounts"/>
    public IReadOnlyCollection<CreateFundAmountModel>? DebitFundAmounts { get; init; }

    /// <inheritdoc cref="Transaction.CreditAccountId"/>
    public Guid? CreditAccountId { get; init; }

    /// <inheritdoc cref="Transaction.CreditFundAmounts"/>
    public IReadOnlyCollection<CreateFundAmountModel>? CreditFundAmounts { get; init; }
}