using System.Text.Json.Serialization;
using Domain.Aggregates.AccountingPeriods;
using RestApi.Models.Account;
using RestApi.Models.FundAmount;

namespace RestApi.Models.AccountingPeriod;

/// <summary>
/// REST model representing a Transaction
/// </summary>
public class TransactionModel
{
    /// <inheritdoc cref="Domain.Aggregates.EntityBase.Id"/>
    public Guid Id { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.Transaction.TransactionDate"/>
    public DateOnly TransactionDate { get; init; }

    /// <summary>
    /// Transaction Account Detail Model for the Account being debited by this Transaction
    /// </summary>
    public TransactionAccountDetailModel? DebitDetail { get; init; }

    /// <summary>
    /// Transaction Account Detail Model for the Account being credited by this Transaction
    /// </summary>
    public TransactionAccountDetailModel? CreditDetail { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.Transaction.AccountingEntries"/>
    public ICollection<FundAmountModel> AccountingEntries { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public TransactionModel(Guid id,
        DateOnly transactionDate,
        TransactionAccountDetailModel? debitDetail,
        TransactionAccountDetailModel? creditDetail,
        ICollection<FundAmountModel> accountingEntries)
    {
        Id = id;
        TransactionDate = transactionDate;
        DebitDetail = debitDetail;
        CreditDetail = creditDetail;
        AccountingEntries = accountingEntries.ToList();
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="transaction">Transaction entity to build this Transaction REST model from</param>
    public TransactionModel(Domain.Aggregates.AccountingPeriods.Transaction transaction)
    {
        Id = transaction.Id.ExternalId;
        TransactionDate = transaction.TransactionDate;
        DebitDetail = transaction.TransactionBalanceEvents
            .Any(balanceEvent => balanceEvent.TransactionAccountType == TransactionAccountType.Debit)
            ? new TransactionAccountDetailModel(transaction, TransactionAccountType.Debit)
            : null;
        CreditDetail = transaction.TransactionBalanceEvents
            .Any(balanceEvent => balanceEvent.TransactionAccountType == TransactionAccountType.Credit)
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
    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.BalanceEventBase.Account"/>
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
    public TransactionAccountDetailModel(
        Domain.Aggregates.AccountingPeriods.Transaction transaction,
        TransactionAccountType type)
    {
        List<TransactionBalanceEvent> balanceEvents = transaction.TransactionBalanceEvents
            .Where(balanceEvent => balanceEvent.TransactionAccountType == type).ToList();
        if (balanceEvents.Count == 0)
        {
            throw new InvalidOperationException();
        }
        Account = new AccountModel(balanceEvents.First().Account);
        PostedStatementDate = balanceEvents
            .SingleOrDefault(balanceEvent => balanceEvent.TransactionEventType == TransactionBalanceEventType.Posted)
            ?.EventDate;
    }
}