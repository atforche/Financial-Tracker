using System.Text.Json.Serialization;
using Domain.BalanceEvents;
using Domain.Transactions;
using Rest.Models.Accounts;

namespace Rest.Models.Transactions;

/// <summary>
/// REST model representing a Transaction Account Detail
/// </summary>
public class TransactionAccountDetailModel
{
    /// <inheritdoc cref="BalanceEvent.Account"/>
    public AccountModel Account { get; init; }

    /// <summary>
    /// Statement date for this Transaction Account Detail once it has been posted
    /// </summary>
    public DateOnly? PostedStatementDate { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public TransactionAccountDetailModel(AccountModel account, DateOnly? postedStatementDate)
    {
        Account = account;
        PostedStatementDate = postedStatementDate;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="transaction">Transaction entity to build this Transaction Account Detail REST model from</param>
    /// <param name="type">Transaction Account Type for this Transaction Account Detail model</param>
    public TransactionAccountDetailModel(Transaction transaction, TransactionAccountType type)
    {
        var balanceEvents = transaction.TransactionBalanceEvents
            .Where(balanceEvent => balanceEvent.AccountType == type).ToList();
        if (balanceEvents.Count == 0)
        {
            throw new InvalidOperationException();
        }
        Account = new AccountModel(balanceEvents.First().Account);
        PostedStatementDate = balanceEvents
            .SingleOrDefault(balanceEvent => balanceEvent.EventType == TransactionBalanceEventType.Posted)
            ?.EventDate;
    }
}