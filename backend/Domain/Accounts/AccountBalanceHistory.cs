using Domain.Funds;
using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Value object class representing the balance of an Account at some point in time.
/// </summary>
public class AccountBalanceHistory : Entity<AccountBalanceHistoryId>
{
    private List<FundAmount> _fundBalances = [];
    private List<FundAmount> _pendingDebits = [];
    private List<FundAmount> _pendingCredits = [];

    /// <summary>
    /// Account for this Account Balance History
    /// </summary>
    public Account Account { get; init; }

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
    /// Pending Debits for this Account Balance History
    /// </summary>
    public IReadOnlyCollection<FundAmount> PendingDebits
    {
        get => _pendingDebits;
        internal set => _pendingDebits = value.ToList();
    }

    /// <summary>
    /// Pending Credits for this Account Balance History
    /// </summary>
    public IReadOnlyCollection<FundAmount> PendingCredits
    {
        get => _pendingCredits;
        internal set => _pendingCredits = value.ToList();
    }

    /// <summary>
    /// Converts this Account Balance History to an Account Balance
    /// </summary>
    /// <returns></returns>
    public AccountBalance ToAccountBalance() => new(Account, FundBalances, PendingDebits, PendingCredits);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountBalanceHistory(Account account,
        TransactionId transactionId,
        DateOnly date,
        int sequence,
        IEnumerable<FundAmount> fundBalances,
        IEnumerable<FundAmount> pendingDebits,
        IEnumerable<FundAmount> pendingCredits)
        : base(new AccountBalanceHistoryId(Guid.NewGuid()))
    {
        Account = account;
        TransactionId = transactionId;
        Date = date;
        Sequence = sequence;
        _fundBalances = fundBalances.ToList();
        _pendingDebits = pendingDebits.ToList();
        _pendingCredits = pendingCredits.ToList();
    }

    /// <summary>
    /// Creates a default instance of this class
    /// </summary>
    private AccountBalanceHistory()
    {
        Account = null!;
        TransactionId = null!;
    }
}