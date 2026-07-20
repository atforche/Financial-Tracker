using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.FundPlans;
using Domain.Funds;

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
    public int Sequence { get; internal set; }

    /// <summary>
    /// Description for this Transaction
    /// </summary>
    public string Description { get; internal set; }

    /// <summary>
    /// Amount for this Transaction
    /// </summary>
    public decimal Amount { get; internal set; }

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
            newBalance = PostToAccountBalance(newBalance, reverse);
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
    public FundBalance ApplyToFundBalance(
        FundBalance existingFundBalance,
        DateOnly? asOfDate = null,
        AccountId? accountId = null,
        bool reverse = false,
        bool postingOnly = false)
    {
        FundBalance newBalance = existingFundBalance;
        if (!postingOnly && (asOfDate == null || Date == asOfDate))
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
                newBalance = PostToFundBalance(newBalance, affectedAccountId, reverse);
            }
        }
        return newBalance;
    }

    /// <summary>
    /// Applies this Transaction to the provided Fund Plan totals.
    /// </summary>
    public FundPlanTotals ApplyToFundPlanTotals(
        FundPlanTotals existingTotals,
        DateOnly? asOfDate = null,
        AccountId? accountId = null,
        bool reverse = false,
        bool postingOnly = false)
    {
        FundPlanTotals newTotals = existingTotals;
        if (!postingOnly && (asOfDate == null || Date == asOfDate))
        {
            newTotals = AddToFundPlanTotals(existingTotals, reverse);
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
                newTotals = PostToFundPlanTotals(newTotals, affectedAccountId, reverse);
            }
        }
        return newTotals;
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
        Description = request.Description;
        Amount = request.Amount;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected Transaction() : base()
    {
        AccountingPeriodId = null!;
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

    /// <summary>
    /// Adds this Transaction to the provided Fund Plan totals.
    /// </summary>
    protected abstract FundPlanTotals AddToFundPlanTotals(FundPlanTotals existingTotals, bool reverse);

    /// <summary>
    /// Posts this Transaction to the provided Fund Plan totals.
    /// </summary>
    protected abstract FundPlanTotals PostToFundPlanTotals(FundPlanTotals existingTotals, AccountId accountId, bool reverse);
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