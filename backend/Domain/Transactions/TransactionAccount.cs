using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions;

/// <summary>
/// Value object class representing an Account involved in a Transaction
/// </summary>
public class TransactionAccount
{
    private readonly List<FundAmount> _fundAmounts = [];

    /// <summary>
    /// Account for this Transaction Account
    /// </summary>
    public AccountId Account { get; private set; }

    /// <summary>
    /// Fund Amounts for this Transaction Account
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundAmounts => _fundAmounts;

    /// <summary>
    /// Date that the Transaction was posted to this Account
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal TransactionAccount(AccountId account, IEnumerable<FundAmount> fundAmounts)
    {
        Account = account;
        _fundAmounts = fundAmounts.ToList();
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private TransactionAccount() => Account = null!;
}