using System.Text.Json.Serialization;
using Domain.Transactions;

namespace Rest.Models.Transactions;

/// <summary>
/// REST model representing a Transaction Account Detail
/// </summary>
public class TransactionAccountDetailModel
{
    /// <inheritdoc cref="Transaction.DebitAccountId"/>
    public Guid AccountId { get; init; }

    /// <summary>
    /// Statement date for this Transaction Account Detail once it has been posted
    /// </summary>
    public DateOnly? PostedStatementDate { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public TransactionAccountDetailModel(Guid accountId, DateOnly? postedStatementDate)
    {
        AccountId = accountId;
        PostedStatementDate = postedStatementDate;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="transaction">Transaction entity to build this Transaction Account Detail REST model from</param>
    /// <param name="type">Transaction Account Type for this Transaction Account Detail model</param>
    public TransactionAccountDetailModel(Transaction transaction, TransactionAccountType type)
    {
        AccountId = (type == TransactionAccountType.Debit
            ? transaction.DebitAccountId ?? throw new InvalidOperationException()
            : transaction.CreditAccountId ?? throw new InvalidOperationException()).Value;

        TransactionBalanceEventPartType postedType = type == TransactionAccountType.Debit
            ? TransactionBalanceEventPartType.PostedDebit
            : TransactionBalanceEventPartType.PostedCredit;
        PostedStatementDate = transaction.TransactionBalanceEvents.SingleOrDefault(balanceEvent =>
            balanceEvent.Parts.Any(part => part.Type == postedType))?.EventDate;
    }
}