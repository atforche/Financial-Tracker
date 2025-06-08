using System.Text.Json.Serialization;
using Domain.Transactions;
using Rest.Models.Funds;

namespace Rest.Models.Transactions;

/// <summary>
/// REST model representing a <see cref="Transaction"/>
/// </summary>
public class TransactionModel
{
    /// <inheritdoc cref="TransactionId"/>
    public Guid Id { get; init; }

    /// <inheritdoc cref="Transaction.Date"/>
    public DateOnly Date { get; init; }

    /// <inheritdoc cref="Transaction.DebitAccountId"/>
    public Guid? DebitAccountId { get; init; }

    /// <inheritdoc cref="Transaction.DebitFundAmounts"/>
    public ICollection<FundAmountModel>? DebitFundAmounts { get; init; }

    /// <inheritdoc cref="Transaction.CreditAccountId"/>
    public Guid? CreditAccountId { get; init; }

    /// <inheritdoc cref="Transaction.CreditFundAmounts"/>
    public ICollection<FundAmountModel>? CreditFundAmounts { get; init; }

    /// <inheritdoc cref="Transaction.TransactionBalanceEvents"/>
    public ICollection<TransactionBalanceEventModel> BalanceEvents { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public TransactionModel(Guid id,
        DateOnly date,
        Guid? debitAccountId,
        ICollection<FundAmountModel>? debitFundAmounts,
        Guid? creditAccountId,
        ICollection<FundAmountModel>? creditFundAmounts,
        ICollection<TransactionBalanceEventModel> balanceEvents)
    {
        Id = id;
        Date = date;
        DebitAccountId = debitAccountId;
        DebitFundAmounts = debitFundAmounts;
        CreditAccountId = creditAccountId;
        CreditFundAmounts = creditFundAmounts;
        BalanceEvents = balanceEvents;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="transaction">Transaction entity to build this Transaction REST model from</param>
    public TransactionModel(Transaction transaction)
    {
        Id = transaction.Id.Value;
        Date = transaction.Date;
        DebitAccountId = transaction.DebitAccountId?.Value;
        DebitFundAmounts = transaction.DebitFundAmounts?.Select(fundAmount => new FundAmountModel(fundAmount)).ToList();
        CreditAccountId = transaction.CreditAccountId?.Value;
        CreditFundAmounts = transaction.CreditFundAmounts?.Select(fundAmount => new FundAmountModel(fundAmount)).ToList();
        BalanceEvents = transaction.TransactionBalanceEvents.Select(balanceEvent => new TransactionBalanceEventModel(balanceEvent)).ToList();
    }
}