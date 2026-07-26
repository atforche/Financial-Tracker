using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.FundGoals;
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
        DateOnly? postedDate = GetPostedDateForAccount(existingAccountBalance.Account.Id);
        if (postedDate != null && (asOfDate == null || postedDate == asOfDate))
        {
            newBalance = PostToAccountBalance(newBalance, reverse);
        }
        return newBalance;
    }

    /// <summary>
    /// Applies this Transaction's Account effect as a posted balance change.
    /// </summary>
    public AccountBalance ApplyAsPostedToAccountBalance(
        AccountBalance existingAccountBalance,
        bool reverse = false) =>
        PostToAccountBalance(existingAccountBalance, reverse);

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
    /// Applies this Transaction's posted Fund effects for the supplied date.
    /// </summary>
    public FundBalance ApplyPostedEffectsToFundBalance(
        FundBalance existingFundBalance,
        DateOnly postedDate,
        bool reverse = false)
    {
        if (!GetAllAffectedAccountIds().Any())
        {
            return Date == postedDate ? AddToFundBalance(existingFundBalance, reverse) : existingFundBalance;
        }
        FundBalance newBalance = existingFundBalance;
        foreach (AccountId accountId in GetAllAffectedAccountIds().Where(accountId => GetPostedDateForAccount(accountId) == postedDate))
        {
            newBalance = PostToFundBalance(newBalance, accountId, reverse);
        }
        return newBalance;
    }

    /// <summary>
    /// Applies all of this Transaction's Fund effects as posted balance changes.
    /// </summary>
    public FundBalance ApplyAsPostedToFundBalance(FundBalance existingFundBalance, bool reverse = false)
    {
        FundBalance newBalance = AddToFundBalance(existingFundBalance, reverse);
        foreach (AccountId accountId in GetAllAffectedAccountIds())
        {
            newBalance = PostToFundBalance(newBalance, accountId, reverse);
        }
        return newBalance;
    }

    /// <summary>
    /// Applies this Transaction to the provided Fund Goal totals.
    /// </summary>
    public FundGoalTotals ApplyToFundGoalTotals(
        FundGoalTotals existingTotals,
        DateOnly? asOfDate = null,
        AccountId? accountId = null,
        bool reverse = false,
        bool postingOnly = false)
    {
        FundGoalTotals newTotals = existingTotals;
        if (!postingOnly && (asOfDate == null || Date == asOfDate))
        {
            newTotals = AddToFundGoalTotals(existingTotals, reverse);
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
                newTotals = PostToFundGoalTotals(newTotals, affectedAccountId, reverse);
            }
        }
        return newTotals;
    }

    /// <summary>
    /// Applies this Transaction's posted Fund Goal effects for the supplied date.
    /// </summary>
    public FundGoalTotals ApplyPostedEffectsToFundGoalTotals(
        FundGoalTotals existingTotals,
        DateOnly postedDate,
        bool reverse = false)
    {
        if (!GetAllAffectedAccountIds().Any())
        {
            return Date == postedDate ? AddToFundGoalTotals(existingTotals, reverse) : existingTotals;
        }
        FundGoalTotals newTotals = existingTotals;
        foreach (AccountId accountId in GetAllAffectedAccountIds().Where(accountId => GetPostedDateForAccount(accountId) == postedDate))
        {
            newTotals = PostToFundGoalTotals(newTotals, accountId, reverse);
        }
        return newTotals;
    }

    /// <summary>
    /// Applies all currently posted Fund Goal effects, regardless of their posting dates.
    /// </summary>
    public FundGoalTotals ApplyAllPostedEffectsToFundGoalTotals(FundGoalTotals existingTotals, bool reverse = false)
    {
        if (!GetAllAffectedAccountIds().Any())
        {
            return AddToFundGoalTotals(existingTotals, reverse);
        }
        FundGoalTotals newTotals = existingTotals;
        foreach (AccountId accountId in GetAllAffectedAccountIds().Where(accountId => GetPostedDateForAccount(accountId) != null))
        {
            newTotals = PostToFundGoalTotals(newTotals, accountId, reverse);
        }
        return newTotals;
    }

    /// <summary>
    /// Applies all of this Transaction's Fund Goal effects as posted totals changes.
    /// </summary>
    public FundGoalTotals ApplyAsPostedToFundGoalTotals(FundGoalTotals existingTotals, bool reverse = false)
    {
        FundGoalTotals newTotals = AddToFundGoalTotals(existingTotals, reverse);
        foreach (AccountId accountId in GetAllAffectedAccountIds())
        {
            newTotals = PostToFundGoalTotals(newTotals, accountId, reverse);
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
    /// Adds this Transaction to the provided Fund Goal totals.
    /// </summary>
    protected abstract FundGoalTotals AddToFundGoalTotals(FundGoalTotals existingTotals, bool reverse);

    /// <summary>
    /// Posts this Transaction to the provided Fund Goal totals.
    /// </summary>
    protected abstract FundGoalTotals PostToFundGoalTotals(FundGoalTotals existingTotals, AccountId accountId, bool reverse);
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