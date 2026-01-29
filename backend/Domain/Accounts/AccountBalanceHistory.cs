using Domain.Funds;
using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Value object class representing the balance of an Account at some point in time.
/// </summary>
public class AccountBalanceHistory : Entity<AccountBalanceHistoryId>
{
    private List<FundAmount> _fundBalances = [];
    private List<FundAmount> _pendingFundBalanceChanges = [];

    /// <summary>
    /// Account ID for this Account Balance History
    /// </summary>
    public AccountId AccountId { get; init; }

    /// <summary>
    /// Transaction ID for this Account Balance History
    /// </summary>
    public TransactionId TransactionId { get; init; }

    /// <summary>
    /// Date for this Account Balance History
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// Sequence number for this Account Balance History
    /// </summary>
    /// <remarks>
    /// The sequence number is used to order multiple balance history records for the same date.
    /// </remarks>
    public int Sequence { get; internal set; }

    /// <summary>
    /// Fund Balances for this Account Balance History
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundBalances
    {
        get => _fundBalances;
        internal set => _fundBalances = value.ToList();
    }

    /// <summary>
    /// Pending Fund Balance Changes for this Account Balance History
    /// </summary>
    public IReadOnlyCollection<FundAmount> PendingFundBalanceChanges
    {
        get => _pendingFundBalanceChanges;
        internal set => _pendingFundBalanceChanges = value.ToList();
    }

    /// <summary>
    /// Converts this Account Balance History to an Account Balance
    /// </summary>
    /// <returns></returns>
    public AccountBalance ToAccountBalance() => new(AccountId, FundBalances, PendingFundBalanceChanges);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountBalanceHistory(AccountId accountId,
        TransactionId transactionId,
        DateOnly date,
        int sequence,
        IEnumerable<FundAmount> fundBalances,
        IEnumerable<FundAmount> pendingFundBalanceChanges)
        : base(new AccountBalanceHistoryId(Guid.NewGuid()))
    {
        AccountId = accountId;
        TransactionId = transactionId;
        Date = date;
        Sequence = sequence;
        _fundBalances = fundBalances.ToList();
        _pendingFundBalanceChanges = pendingFundBalanceChanges.ToList();
    }

    /// <summary>
    /// Creates a default instance of this class
    /// </summary>
    private AccountBalanceHistory()
    {
        AccountId = null!;
        TransactionId = null!;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="AccountBalanceHistory"/>
/// </summary>
public record AccountBalanceHistoryId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// This constructor should only even be used when creating a new Account Balance History ID during Account Balance History creation. 
    /// </summary>
    /// <param name="value">Value for this Account ID</param>
    internal AccountBalanceHistoryId(Guid value)
        : base(value)
    {
    }
}