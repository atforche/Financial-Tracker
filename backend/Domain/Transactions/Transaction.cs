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
    /// Applies this Transaction to the provided existing Account Balance as of the provided date
    /// </summary>
    public AccountBalance ApplyToAccountBalance(AccountBalance existingAccountBalance, DateOnly asOfDate)
    {
        AccountBalance newBalance = existingAccountBalance;
        if (Date == asOfDate)
        {
            newBalance = AddToAccountBalance(existingAccountBalance, false);
        }
        if (GetPostedDateForAccount(existingAccountBalance.Account.Id) == asOfDate)
        {
            newBalance = PostToAccountBalance(existingAccountBalance, false);
        }
        return newBalance;
    }

    /// <summary>
    /// Applies this Transaction to the provided existing Fund Balance as of the provided date
    /// </summary>
    public FundBalance ApplyToFundBalance(FundBalance existingFundBalance, DateOnly asOfDate)
    {
        FundBalance newBalance = existingFundBalance;
        if (Date == asOfDate)
        {
            newBalance = AddToFundBalance(existingFundBalance, false);
        }
        foreach (AccountId accountId in GetAllAffectedAccountIds())
        {
            if (GetPostedDateForAccount(accountId) == asOfDate)
            {
                newBalance = PostToFundBalance(existingFundBalance, accountId, false);
            }
        }
        return newBalance;
    }

    /// <summary>
    /// Gets all Account IDs affected by this Transaction
    /// </summary>
    internal abstract IEnumerable<AccountId> GetAllAffectedAccountIds();

    /// <summary>
    /// Gets the posted date for the provided account ID
    /// </summary>
    internal abstract DateOnly? GetPostedDateForAccount(AccountId accountId);

    /// <summary>
    /// Adds this Transaction to the provided existing Account Balance
    /// </summary>
    internal abstract AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse);

    /// <summary>
    /// Posts this Transaction to the provided account balance
    /// </summary>
    internal abstract AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse);

    /// <summary>
    /// Gets all Fund IDs affected by this Transaction for the provided account ID
    /// </summary>
    internal abstract IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId);

    /// <summary>
    /// Adds this Transaction to the provided existing Fund Balance
    /// </summary>
    internal abstract FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse);

    /// <summary>
    /// Posts this Transaction to the provided fund balance
    /// </summary>
    internal abstract FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse);

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