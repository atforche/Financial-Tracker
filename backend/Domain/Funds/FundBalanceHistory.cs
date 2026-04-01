using Domain.Accounts;
using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Entity class representing the balance of a Fund at some point in time.
/// </summary>
public class FundBalanceHistory : Entity<FundBalanceHistoryId>
{
    private List<AccountAmount> _accountBalances = [];
    private List<AccountAmount> _pendingDebits = [];
    private List<AccountAmount> _pendingCredits = [];

    /// <summary>
    /// Fund ID for this Fund Balance History
    /// </summary>
    public FundId FundId { get; init; }

    /// <summary>
    /// Transaction ID for this Fund Balance History
    /// </summary>
    public TransactionId TransactionId { get; init; }

    /// <summary>
    /// Date for this Fund Balance History
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// Sequence number for this Fund Balance History
    /// </summary>
    /// <remarks>
    /// The sequence number is used to order multiple balance history records for the same date.
    /// </remarks>
    public int Sequence { get; internal set; }

    /// <summary>
    /// Posted Balance for this Fund Balance History
    /// </summary>
    public decimal PostedBalance { get; private set; }

    /// <summary>
    /// Fund Balances for this Fund Balance History
    /// </summary>
    public IReadOnlyCollection<AccountAmount> AccountBalances
    {
        get => _accountBalances;
        private set => _accountBalances = value.ToList();
    }

    /// <summary>
    /// Pending Debits for this Fund Balance History
    /// </summary>
    public IReadOnlyCollection<AccountAmount> PendingDebits
    {
        get => _pendingDebits;
        private set => _pendingDebits = value.ToList();
    }

    /// <summary>
    /// Pending Credits for this Fund Balance History 
    /// </summary>
    public IReadOnlyCollection<AccountAmount> PendingCredits
    {
        get => _pendingCredits;
        private set => _pendingCredits = value.ToList();
    }

    /// <summary>
    /// Updates this Fund Balance History with a new Fund Balance.
    /// </summary>
    public void Update(FundBalance fundBalance)
    {
        if (fundBalance.FundId != FundId)
        {
            throw new InvalidOperationException("Cannot update Fund Balance History with a Fund Balance for a different Fund");
        }
        PostedBalance = fundBalance.PostedBalance;
        _accountBalances = fundBalance.AccountBalances.ToList();
        _pendingDebits = fundBalance.PendingDebits.ToList();
        _pendingCredits = fundBalance.PendingCredits.ToList();
    }

    /// <summary>
    /// Converts this Fund Balance History to a Fund Balance
    /// </summary>
    /// <returns></returns>
    public FundBalance ToFundBalance() => new(FundId, AccountBalances, PendingDebits, PendingCredits);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundBalanceHistory(FundId fundId,
        TransactionId transactionId,
        DateOnly date,
        int sequence,
        IEnumerable<AccountAmount> accountBalances,
        IEnumerable<AccountAmount> pendingDebits,
        IEnumerable<AccountAmount> pendingCredits)
        : base(new FundBalanceHistoryId(Guid.NewGuid()))
    {
        FundId = fundId;
        TransactionId = transactionId;
        Date = date;
        Sequence = sequence;
        _accountBalances = accountBalances.ToList();
        _pendingDebits = pendingDebits.ToList();
        _pendingCredits = pendingCredits.ToList();
    }

    /// <summary>
    /// Creates a default instance of this class
    /// </summary>
    private FundBalanceHistory()
    {
        FundId = null!;
        TransactionId = null!;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="FundBalanceHistory"/>
/// </summary>
public record FundBalanceHistoryId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// </summary>
    internal FundBalanceHistoryId(Guid value)
        : base(value)
    {
    }
}