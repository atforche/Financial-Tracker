using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions.CreateRequests;

namespace Domain.Transactions;

/// <summary>
/// Abstract base class representing a Transaction
/// </summary>
public abstract class Transaction : Entity<TransactionId>
{
    /// <summary>
    /// Type for this Transaction
    /// </summary>
    public TransactionType Type { get; private set; }

    /// <summary>
    /// Accounting Period for this Transaction
    /// </summary>
    public AccountingPeriodId AccountingPeriodId { get; private set; }

    /// <summary>
    /// Date for this Transaction
    /// </summary>
    public DateOnly Date { get; internal set; }

    /// <summary>
    /// Sequence number for this Transaction
    /// </summary> 
    /// <remarks>
    /// The sequence number is used to order multiple transactions for the same date.
    /// </remarks>
    public int Sequence { get; internal set; }

    /// <summary>
    /// Location for this Transaction
    /// </summary>
    public string Location { get; internal set; }

    /// <summary>
    /// Description for this Transaction
    /// </summary>
    public string Description { get; internal set; }

    /// <summary>
    /// Amount for this Transaction
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Gets all Account IDs affected by this Transaction
    /// </summary>
    public abstract IEnumerable<AccountId> GetAllAffectedAccountIds();

    /// <summary>
    /// Gets the posted date for the provided account ID
    /// </summary>
    public abstract DateOnly? GetPostedDateForAccount(AccountId accountId);

    /// <summary>
    /// Applies this Transaction to the provided existing Account Balance
    /// </summary>
    /// <param name="existingAccountBalance">The existing Account Balance to apply this Transaction to</param>
    /// <param name="asOfDate">If provided, only applies the portions of this Transaction that occurred on the specified date</param>
    /// <param name="reverse">If true, reverses the effects of this Transaction</param>
    public AccountBalance ApplyToAccountBalance(
        AccountBalance existingAccountBalance,
        DateOnly? asOfDate = null,
        bool reverse = false)
    {
        AccountBalance newBalance = existingAccountBalance;
        if (asOfDate == null || Date == asOfDate)
        {
            newBalance = AddToAccountBalance(existingAccountBalance, reverse);
        }
        DateOnly? postedDate = GetPostedDateForAccount(existingAccountBalance.Account.Id);
        if (postedDate != null && (asOfDate == null || postedDate == asOfDate))
        {
            newBalance = PostToAccountBalance(existingAccountBalance, reverse);
        }
        return newBalance;
    }

    /// <summary>
    /// Gets all Fund IDs affected by this Transaction for the provided account ID
    /// </summary>
    public abstract IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId);

    /// <summary>
    /// Applies this Transaction to the provided existing Fund Balance
    /// </summary>
    /// <param name="existingFundBalance">The existing Fund Balance to apply this Transaction to</param>
    /// <param name="asOfDate">If provided, only applies the portions of this Transaction that occurred on the specified date</param>
    /// <param name="accountId">If provided, only applies the portions of this Transaction that affect the specified account</param>
    /// <param name="reverse">If true, reverses the effects of this Transaction</param>
    public FundBalance ApplyToFundBalance(
        FundBalance existingFundBalance,
        DateOnly? asOfDate = null,
        AccountId? accountId = null,
        bool reverse = false)
    {
        FundBalance newBalance = existingFundBalance;
        if (asOfDate == null || Date == asOfDate)
        {
            newBalance = AddToFundBalance(existingFundBalance, reverse);
        }
        foreach (AccountId affectedAccountId in GetAllAffectedAccountIds())
        {
            if (accountId != null && affectedAccountId != accountId)
            {
                continue;
            }
            DateOnly? postedDate = GetPostedDateForAccount(affectedAccountId);
            if (postedDate != null && (asOfDate == null || postedDate == asOfDate))
            {
                newBalance = PostToFundBalance(existingFundBalance, affectedAccountId, reverse);
            }
        }
        return newBalance;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal Transaction(CreateTransactionRequest request, int sequence, TransactionType type)
        : base(new TransactionId(Guid.NewGuid()))
    {
        Type = type;
        AccountingPeriodId = request.AccountingPeriodId;
        Date = request.TransactionDate;
        Sequence = sequence;
        Location = request.Location;
        Description = request.Description;
        Amount = request.Amount;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected Transaction() : base()
    {
        AccountingPeriodId = null!;
        Location = null!;
        Description = null!;
    }

    /// <summary>
    /// Adds this Transaction to the provided existing Account Balance
    /// </summary>
    protected abstract AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse);

    /// <summary>
    /// Posts this Transaction to the provided account balance
    /// </summary>
    protected abstract AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse);

    /// <summary>
    /// Adds this Transaction to the provided existing Fund Balance
    /// </summary>
    protected abstract FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse);

    /// <summary>
    /// Posts this Transaction to the provided fund balance
    /// </summary>
    protected abstract FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse);
}

/// <summary>
/// Value object class representing the ID of a <see cref="Transaction"/>
/// </summary>
public record TransactionId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// </summary>
    internal TransactionId(Guid value)
        : base(value)
    {
    }
}