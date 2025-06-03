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

    /// <summary>
    /// Transaction Account Detail Model for the Account being debited by this Transaction
    /// </summary>
    public TransactionAccountDetailModel? DebitDetail { get; init; }

    /// <summary>
    /// Transaction Account Detail Model for the Account being credited by this Transaction
    /// </summary>
    public TransactionAccountDetailModel? CreditDetail { get; init; }

    /// <inheritdoc cref="Transaction.FundAmounts"/>
    public ICollection<FundAmountModel> FundAmounts { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public TransactionModel(Guid id,
        DateOnly date,
        TransactionAccountDetailModel? debitDetail,
        TransactionAccountDetailModel? creditDetail,
        ICollection<FundAmountModel> fundAmounts)
    {
        Id = id;
        Date = date;
        DebitDetail = debitDetail;
        CreditDetail = creditDetail;
        FundAmounts = fundAmounts;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="transaction">Transaction entity to build this Transaction REST model from</param>
    public TransactionModel(Transaction transaction)
    {
        Id = transaction.Id.Value;
        Date = transaction.Date;
        DebitDetail = transaction.TransactionBalanceEvents
            .Any(balanceEvent => balanceEvent.AccountType == TransactionAccountType.Debit)
            ? new TransactionAccountDetailModel(transaction, TransactionAccountType.Debit)
            : null;
        CreditDetail = transaction.TransactionBalanceEvents
            .Any(balanceEvent => balanceEvent.AccountType == TransactionAccountType.Credit)
            ? new TransactionAccountDetailModel(transaction, TransactionAccountType.Credit)
            : null;
        FundAmounts = transaction.FundAmounts.Select(fundAmount => new FundAmountModel(fundAmount)).ToList();
    }
}