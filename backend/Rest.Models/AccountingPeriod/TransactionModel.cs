using System.Text.Json.Serialization;
using Domain;
using Domain.AccountingPeriods;
using Domain.BalanceEvents;
using Rest.Models.Account;
using Rest.Models.FundAmount;

namespace Rest.Models.AccountingPeriod;

/// <summary>
/// REST model representing a Transaction
/// </summary>
public class TransactionModel
{
    /// <inheritdoc cref="Entity.Id"/>
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

    /// <inheritdoc cref="Transaction.AccountingEntries"/>
    public ICollection<FundAmountModel> AccountingEntries { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public TransactionModel(Guid id,
        DateOnly date,
        TransactionAccountDetailModel? debitDetail,
        TransactionAccountDetailModel? creditDetail,
        ICollection<FundAmountModel> accountingEntries)
    {
        Id = id;
        Date = date;
        DebitDetail = debitDetail;
        CreditDetail = creditDetail;
        AccountingEntries = accountingEntries;
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
        AccountingEntries = transaction.AccountingEntries.Select(fundAmount => new FundAmountModel(fundAmount)).ToList();
    }
}

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