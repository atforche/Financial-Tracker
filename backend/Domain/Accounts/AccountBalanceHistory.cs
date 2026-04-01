using Domain.Funds;
using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Entity class representing the balance of an Account at some point in time.
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
    /// Posted Balance for this Account Balance History
    /// </summary>
    public decimal PostedBalance { get; private set; }

    /// <summary>
    /// Available to Spend for this Account Balance History
    /// </summary>
    public decimal? AvailableToSpend { get; private set; }

    /// <summary>
    /// Fund Balances for this Account Balance History
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundBalances
    {
        get => _fundBalances;
        private set => _fundBalances = value.ToList();
    }

    /// <summary>
    /// Pending Debits for this Account Balance History
    /// </summary>
    public IReadOnlyCollection<FundAmount> PendingDebits
    {
        get => _pendingDebits;
        private set => _pendingDebits = value.ToList();
    }

    /// <summary>
    /// Pending Credits for this Account Balance History
    /// </summary>
    public IReadOnlyCollection<FundAmount> PendingCredits
    {
        get => _pendingCredits;
        private set => _pendingCredits = value.ToList();
    }

    /// <summary>
    /// Updates this Account Balance History with a new Account Balance.
    /// </summary>
    public void Update(AccountBalance newBalance)
    {
        if (newBalance.Account.Id != Account.Id)
        {
            throw new ArgumentException("New balance must be for the same account", nameof(newBalance));
        }
        PostedBalance = newBalance.PostedBalance;
        AvailableToSpend = newBalance.AvailableToSpend;
        _fundBalances = newBalance.FundBalances.ToList();
        _pendingDebits = newBalance.PendingDebits.ToList();
        _pendingCredits = newBalance.PendingCredits.ToList();
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

/// <summary>
/// Value object class representing the ID of an <see cref="AccountBalanceHistory"/>
/// </summary>
public record AccountBalanceHistoryId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// </summary>
    internal AccountBalanceHistoryId(Guid value)
        : base(value)
    {
    }
}